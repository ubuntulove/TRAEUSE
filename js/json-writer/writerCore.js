/* =========================================================
 * writerCore.js — Indexed JSON Writer 核心
 *  用来替代 write 工具：
 *    - 不整文件覆盖，只做最小修改
 *    - 每次修改都是 {op, idx, ...} 的 patch
 *    - 可序列化、可审计、可回放、可合并、可冲突检测
 *    - 支持写入内存源 / 或（如果有 Node FS）真实文件
 *
 *  Patch 格式定义（IndexedJSONPatch v1）：
 *  [
 *    {"op":"set",    "idx":[...],  "value":any   [, "key":string] }  // 设置节点值
 *    {"op":"insert", "idx":[...],  "value":any }                     // 数组插入
 *    {"op":"delete", "idx":[...] }                                    // 删除节点
 *    {"op":"rename", "idx":[...],  "key":string }                     // 对象键重命名
 *    {"op":"move",   "from":[...], "to":[...] }                       // 移动节点
 *    {"op":"copy",   "from":[...], "to":[...] }                       // 复制节点
 *    {"op":"merge",  "idx":[...],  "value":object }                   // 对象浅合并
 *  ]
 * ========================================================= */
class IndexedJSONWriter {
  constructor(indexer) {
    this.indexer = indexer;
    this.sources = new Map(); // path -> jsonObj
    this.history = []; // 每次 applyPatch 记录快照
    this.listeners = [];
  }
  setMemorySource(path, json) {
    this.sources.set(path, JSON.parse(JSON.stringify(json)));
  }
  getMemorySource(path) {
    return this.sources.get(path);
  }
  onUpdate(fn) { this.listeners.push(fn); }

  /**
   * 核心：对 source（或 path 指向的内存源）应用一组 patch
   * 返回 { result, logs, errors }
   */
  apply(sourceOrPath, patch) {
    let src = typeof sourceOrPath === 'string' ? this.sources.get(sourceOrPath) : sourceOrPath;
    if (!src) return { ok:false, errors:['source not found: ' + sourceOrPath], result: null, logs:[] };
    // 深拷贝（确保对源不破坏；如果是内存源则写回）
    let working = JSON.parse(JSON.stringify(src));
    const logs = [];
    const errors = [];

    for (let i=0;i<patch.length;i++){
      const p = patch[i];
      const r = this._op(working, p);
      if (r === null) { errors.push(`op#${i} 未知操作: ${p.op}`); continue; }
      if (r.ok === false || r.replacedRoot === undefined && !r.ok) {
        errors.push(`op#${i} [${p.op}] 失败: ${r.reason||JSON.stringify(r)} @ idx=${JSON.stringify(p.idx||p.from)}`);
        continue;
      }
      if (r.replacedRoot) working = r.value;
      logs.push({ i, op: p.op, path: r.path || p.idx || p.from, ok: true, meta: r });
    }

    // 如果来自内存源，写回
    if (typeof sourceOrPath === 'string' && this.sources.has(sourceOrPath)) {
      this.sources.set(sourceOrPath, working);
    }

    // 历史
    this.history.push({
      time: new Date(),
      target: typeof sourceOrPath === 'string' ? sourceOrPath : '(direct)',
      patch: JSON.parse(JSON.stringify(patch)),
      result: JSON.parse(JSON.stringify(working)),
      logs, errors,
    });
    if (this.history.length > 100) this.history.shift();

    for (const fn of this.listeners) {
      try { fn({ target: sourceOrPath, result: working, logs, errors }); } catch(e){}
    }

    return { ok: errors.length === 0, result: working, logs, errors };
  }

  _op(src, p) {
    switch (p.op) {
      case 'set':    return this.indexer.setAt(src, p.idx, p.value);
      case 'insert': return this.indexer.insertAt(src, p.idx, p.value);
      case 'delete': return this.indexer.deleteAt(src, p.idx);
      case 'rename': return this._rename(src, p.idx, p.key);
      case 'move':   return this._move(src, p.from, p.to);
      case 'copy':   return this._copy(src, p.from, p.to);
      case 'merge':  return this._merge(src, p.idx, p.value);
      default:       return null;
    }
  }

  _rename(src, idx, newKey) {
    if (idx.length === 0) return { ok:false, reason:'cannot-rename-root' };
    const key = idx[idx.length-1];
    if (typeof key === 'number') return { ok:false, reason:'cannot-rename-array-index' };
    const parentIdx = idx.slice(0,-1);
    const p = this.indexer.getAt(src, parentIdx);
    if (!p.found) return { ok:false, ...p };
    if (typeof p.value !== 'object' || Array.isArray(p.value)) return { ok:false, reason:'not-object' };
    if (!(key in p.value)) return { ok:false, reason:'key-not-exist', key };
    if (newKey in p.value) return { ok:false, reason:'key-already-exists', newKey };
    const val = p.value[key];
    delete p.value[key];
    p.value[newKey] = val;
    return { ok:true, path:[...parentIdx, newKey], old:key, newKey, op:'rename' };
  }

  _move(src, from, to) {
    if (from.length === 0 || to.length === 0) return { ok:false, reason:'root-move' };
    // 先取 from 值
    const g = this.indexer.getAt(src, from);
    if (!g.found) return { ok:false, ...g };
    const val = g.value;
    const d = this.indexer.deleteAt(src, from);
    if (d.ok === false) return { ok:false, reason:'delete-failed', info:d };
    // 插入到 to
    const toKey = to[to.length-1];
    if (typeof toKey === 'number') {
      // 数组插入
      const r = this.indexer.insertAt(src, to, val);
      return r.ok ? { ok:true, from, to, val, op:'move', inserted:r } : r;
    } else {
      // 设置对象键
      const r = this.indexer.setAt(src, to, val);
      return r;
    }
  }

  _copy(src, from, to) {
    const g = this.indexer.getAt(src, from);
    if (!g.found) return { ok:false, ...g };
    const val = JSON.parse(JSON.stringify(g.value));
    const toKey = to[to.length-1];
    if (typeof toKey === 'number') return this.indexer.insertAt(src, to, val);
    return this.indexer.setAt(src, to, val);
  }

  _merge(src, idx, obj) {
    const g = this.indexer.getAt(src, idx);
    if (!g.found) return { ok:false, ...g };
    if (typeof g.value !== 'object' || Array.isArray(g.value)) return { ok:false, reason:'not-object' };
    const before = JSON.parse(JSON.stringify(g.value));
    Object.assign(g.value, obj);
    return { ok:true, path:idx, before, merged: obj, op:'merge' };
  }

  /**
   * 把一组 patch 压缩为等价的最小 patch（用于生成简洁 diff）
   */
  compress(patches) {
    // 简单规则：同一 idx 的连续 set -> 只保留最后一个
    //         同一 idx 的 delete + insert -> rename
    const out = [];
    const lastSeen = new Map();
    for (let i=0;i<patches.length;i++){
      const p = patches[i];
      const pathKey = this.indexer.pathKey(p.idx || p.from || p.to || []);
      if (p.op === 'set') {
        // 覆盖之前同路径的 set
        if (lastSeen.has(pathKey) && out[lastSeen.get(pathKey)].op === 'set') {
          out[lastSeen.get(pathKey)] = p;
        } else {
          lastSeen.set(pathKey, out.length);
          out.push(p);
        }
      } else {
        lastSeen.set(pathKey, out.length);
        out.push(p);
      }
    }
    return out;
  }
}

/* =========================================================
 * indexer.js — Indexed JSON Indexer
 *  核心思想：
 *   - 给 JSON 中每一个"节点"分配一个确定性的"坐标路径"
 *     即路径数组 idx = [k1, k2, k3, ...]
 *     其中数字代表数组下标，字符串代表对象 key
 *   - 建立倒排索引，加速查找、校验和 diff
 *   - 与 renderer/coordinate 工具复用相同的"坐标"概念
 *
 *  这是下一代 write 工具的基础：
 *   之前的 write 工具 = 整文件覆盖
 *   现在的 IndexedJSON = 精准节点级 patch + 可审计 + 可合并
 * ========================================================= */
class IndexedJSONIndexer {
  constructor() {
    this.cache = new WeakMap();
  }

  /**
   * 为一个 JSON 值构建完整索引
   * @param {any} json
   * @returns {{
   *   nodes: Map<string, {path:array, value:any, type:string, parent:string|null}>,
   *   pathMap: Map<string, Node>,
   *   maxLine: number,
   *   meta: any
   * }}
   */
  index(json) {
    const nodes = new Map();
    const pathMap = new Map();
    const rootId = 'root';
    let _id = 0;
    const newId = () => `n${(_id++).toString(36)}`;

    const walk = (node, path, parentId) => {
      const id = path.length === 0 ? rootId : newId();
      let type = typeof node;
      if (node === null) type = 'null';
      else if (Array.isArray(node)) type = 'array';
      const record = {
        id, path: path.slice(), value: node, type, parent: parentId || null,
        key: path.length ? path[path.length-1] : null,
      };
      nodes.set(id, record);
      pathMap.set(this.pathKey(path), id);
      if (type === 'object') {
        for (const k of Object.keys(node)) {
          walk(node[k], [...path, k], id);
        }
      } else if (type === 'array') {
        for (let i=0;i<node.length;i++){
          walk(node[i], [...path, i], id);
        }
      }
      return id;
    };
    walk(json, [], null);
    return {
      nodes,
      pathMap,
      size: nodes.size,
      meta: this._analyze(json, nodes),
    };
  }

  pathKey(idx) {
    return idx.map(x => typeof x === 'number' ? `[${x}]` : ('.' + x)).join('') || '/';
  }

  _analyze(json, nodes) {
    let arrays = 0, objects = 0, strings = 0, numbers = 0, booleans = 0, nulls = 0;
    let maxDepth = 0, totalArrayElements = 0;
    for (const n of nodes.values()) {
      if (n.type === 'array') arrays++;
      else if (n.type === 'object') objects++;
      else if (n.type === 'string') strings++;
      else if (n.type === 'number') numbers++;
      else if (n.type === 'boolean') booleans++;
      else if (n.type === 'null') nulls++;
      if (n.path.length > maxDepth) maxDepth = n.path.length;
      if (Array.isArray(n.value)) totalArrayElements += n.value.length;
    }
    return { arrays, objects, strings, numbers, booleans, nulls, maxDepth, totalArrayElements };
  }

  /**
   * 根据 idx 路径获取节点
   */
  getAt(json, idx) {
    let cur = json;
    for (const seg of idx) {
      if (cur == null) return { found:false, at: seg };
      if (typeof seg === 'number') {
        if (!Array.isArray(cur)) return { found:false, reason:'not-array', at:seg };
        if (seg < 0 || seg > cur.length) return { found:false, reason:'out-of-bounds', at:seg, max:cur.length };
      } else {
        if (typeof cur !== 'object' || Array.isArray(cur)) return { found:false, reason:'not-object', at:seg };
        if (!(seg in cur)) return { found:false, reason:'key-not-exist', at:seg };
      }
      cur = cur[seg];
    }
    return { found:true, value:cur };
  }

  /**
   * 安全地对 idx 所指节点写入值（原地修改会返回可审计的描述）
   */
  setAt(json, idx, newValue) {
    if (idx.length === 0) {
      const old = json;
      // 替换根 — 需要调用方接收返回值
      return { replacedRoot: true, old, value: newValue, path: idx };
    }
    // 找到父节点
    const parentIdx = idx.slice(0, -1);
    const key = idx[idx.length-1];
    const p = this.getAt(json, parentIdx);
    if (!p.found) return { ok:false, ...p };
    const parent = p.value;
    if (typeof key === 'number') {
      if (!Array.isArray(parent)) return { ok:false, reason:'parent-not-array', path:parentIdx };
      if (key > parent.length) return { ok:false, reason:'out-of-bounds', path:idx, max:parent.length };
      const old = parent[key];
      parent[key] = newValue;
      return { ok:true, path:idx, old, value:newValue, op:'set' };
    } else {
      if (typeof parent !== 'object' || Array.isArray(parent)) return { ok:false, reason:'parent-not-object', path:parentIdx };
      const existed = key in parent;
      const old = parent[key];
      parent[key] = newValue;
      return { ok:true, path:idx, old, value:newValue, op: existed ? 'set':'add-key', added: !existed };
    }
  }

  insertAt(json, idx, value) {
    if (idx.length === 0) return { ok:false, reason:'cannot-insert-root' };
    const parentIdx = idx.slice(0, -1);
    const pos = idx[idx.length-1];
    if (typeof pos !== 'number') return { ok:false, reason:'insert-requires-numeric-index', pos };
    const p = this.getAt(json, parentIdx);
    if (!p.found) return { ok:false, ...p };
    const parent = p.value;
    if (!Array.isArray(parent)) return { ok:false, reason:'parent-not-array', path:parentIdx };
    if (pos < 0 || pos > parent.length) return { ok:false, reason:'out-of-bounds', pos, max:parent.length };
    parent.splice(pos, 0, value);
    return { ok:true, path:idx, value, op:'insert', length:parent.length };
  }

  deleteAt(json, idx) {
    if (idx.length === 0) return { ok:false, reason:'cannot-delete-root' };
    const parentIdx = idx.slice(0, -1);
    const key = idx[idx.length-1];
    const p = this.getAt(json, parentIdx);
    if (!p.found) return { ok:false, ...p };
    const parent = p.value;
    if (typeof key === 'number') {
      if (!Array.isArray(parent)) return { ok:false, reason:'parent-not-array' };
      if (key < 0 || key >= parent.length) return { ok:false, reason:'out-of-bounds', max:parent.length-1 };
      const old = parent.splice(key,1)[0];
      return { ok:true, path:idx, old, op:'delete' };
    } else {
      if (typeof parent !== 'object' || Array.isArray(parent)) return { ok:false, reason:'parent-not-object' };
      if (!(key in parent)) return { ok:false, reason:'key-not-exist', key };
      const old = parent[key];
      delete parent[key];
      return { ok:true, path:idx, old, op:'delete-key', key };
    }
  }

  /**
   * 为 JSON 内容分配"行号"式的平面索引，用于代码级定位
   */
  computeLineIndex(jsonString) {
    const lines = jsonString.split('\n');
    const out = {};
    const stack = []; // [{type:'obj'|'arr', children:[...], keyCount, done:false}]
    let inStr = false, strEsc = false;
    let curKey = null, pendingColon = false, inVal = false;
    // 这是一个简化行索引（给代码提示用），完整解析交给 JSON.parse
    for (let i=0;i<lines.length;i++){
      const line = lines[i];
      out[i+1] = { line: i+1, text: line, pathHint: stack[stack.length-1]?.path || [] };
      const trimmed = line.trim();
      if (trimmed.startsWith('{')) stack.push({type:'obj', path: stack.length ? [...stack[stack.length-1].path, curKey||'(root)'] : []});
      else if (trimmed.startsWith('[')) stack.push({type:'arr', path: stack.length ? [...stack[stack.length-1].path, curKey||0] : []});
      else if (trimmed.startsWith('}') || trimmed.startsWith(']')) stack.pop();
      const keyMatch = trimmed.match(/^"([^"]+)"\s*:/);
      if (keyMatch) {
        curKey = keyMatch[1];
        const sp = stack[stack.length-1];
        if (sp) sp.currentKey = curKey;
      }
      if (/^\s*\d/.test(trimmed) || /^\s*"/.test(trimmed) || /^\s*(true|false|null)/.test(trimmed)) {
        const sp = stack[stack.length-1];
        if (sp && sp.type === 'arr') {
          if (!sp.cnt) sp.cnt = 0;
          out[i+1].pathHint = [...sp.path, sp.cnt++];
        }
      }
    }
    return out;
  }
}

/* =========================================================
 * transform.js — Indexed JSON 转换器（高层 API）
 *  把 UI 工具（坐标/滑块/游戏状态）的输出直接转成 patch，
 *  并提供 validate、applyPatch 等便捷方法。
 *  下一代 write 工具的主要入口。
 * ========================================================= */
class IndexedJSONTransform {
  constructor(indexer, writer) {
    this.indexer = indexer;
    this.writer = writer;
  }

  /**
   * 应用一组 patch 到 source；返回与 writer.apply 相同结构，但增加"分析"。
   */
  applyPatch(source, patch) {
    if (!Array.isArray(patch)) {
      // 允许传入单一 patch 对象
      patch = [patch];
    }
    const preIndex = this.indexer.index(source);
    const res = this.writer.apply(source, patch);
    const postIndex = this.indexer.index(res.result || source);
    // 计算 diff 摘要
    res.summary = {
      nodesBefore: preIndex.size,
      nodesAfter: postIndex.size,
      depthBefore: preIndex.meta.maxDepth,
      depthAfter: postIndex.meta.maxDepth,
      ops: patch.length,
      changed: patch.filter(p => !this._noop(p, source, res.result)).length,
    };
    return res;
  }

  _noop(p, before, after) {
    try {
      if (p.op === 'set' && p.idx) {
        const b = this.indexer.getAt(before, p.idx);
        const a = this.indexer.getAt(after, p.idx);
        return b.found && a.found && JSON.stringify(b.value) === JSON.stringify(a.value);
      }
    } catch(e){}
    return false;
  }

  /**
   * 校验 patch 是否合法（不修改数据）
   */
  validate(patch, source) {
    const report = { total: 0, valid: 0, warnings: [], errors: [] };
    if (!Array.isArray(patch)) { report.errors.push('patch 必须是数组'); return report; }
    report.total = patch.length;
    const allowedOps = new Set(['set','insert','delete','rename','move','copy','merge']);
    for (let i=0;i<patch.length;i++){
      const p = patch[i];
      if (!p || typeof p !== 'object') { report.errors.push(`#${i} 不是对象`); continue; }
      if (!allowedOps.has(p.op)) { report.errors.push(`#${i} 未知 op: ${p.op}`); continue; }
      // idx 检查
      if (['set','insert','delete','rename','merge'].includes(p.op)) {
        if (!Array.isArray(p.idx)) { report.errors.push(`#${i} [${p.op}] 缺少 idx 数组`); continue; }
      }
      if (['move','copy'].includes(p.op)) {
        if (!Array.isArray(p.from) || !Array.isArray(p.to)) {
          report.errors.push(`#${i} [${p.op}] 需要 from 和 to 数组`); continue;
        }
      }
      // set/insert/merge 需要 value
      if (['set','insert','merge'].includes(p.op) && p.value === undefined) {
        report.errors.push(`#${i} [${p.op}] 缺少 value`); continue;
      }
      // rename 需要 key
      if (p.op === 'rename' && (typeof p.key !== 'string' || p.key.length === 0)) {
        report.errors.push(`#${i} [rename] 需要非空 string key`); continue;
      }

      // 如果提供了 source，检查路径可达性
      if (source) {
        if (p.op === 'delete' || p.op === 'rename' || p.op === 'move' || p.op === 'copy' || p.op === 'set' || p.op === 'merge') {
          const checkIdx = p.op === 'move' || p.op === 'copy' ? p.from : p.idx;
          const g = this.indexer.getAt(source, checkIdx);
          if (!g.found) {
            if (p.op === 'set' || p.op === 'merge') {
              // set 允许在数组末尾（insert-at-end 语义），但其他要检查
              if (checkIdx.length && typeof checkIdx[checkIdx.length-1] === 'number') {
                const parentIdx = checkIdx.slice(0,-1);
                const pg = this.indexer.getAt(source, parentIdx);
                if (pg.found && Array.isArray(pg.value)) {
                  const k = checkIdx[checkIdx.length-1];
                  if (k === pg.value.length) {
                    report.warnings.push(`#${i} [set] idx 指向数组末尾 (${k})，将当作 insert。`);
                  } else if (k > pg.value.length) {
                    report.errors.push(`#${i} [set] 数组下标 ${k} 越界，数组长度 ${pg.value.length}`); continue;
                  }
                } else if (!pg.found) { report.errors.push(`#${i} 路径不存在: ${JSON.stringify(checkIdx)}`); continue; }
              } else {
                report.warnings.push(`#${i} [${p.op}] 目标路径不存在: ${JSON.stringify(checkIdx)}，将创建该键`);
              }
            } else {
              report.errors.push(`#${i} [${p.op}] 路径不存在: ${JSON.stringify(checkIdx)} (${g.reason})`); continue;
            }
          }
        }
        if (p.op === 'insert') {
          const parentIdx = p.idx.slice(0,-1);
          const pos = p.idx[p.idx.length-1];
          const pg = this.indexer.getAt(source, parentIdx);
          if (!pg.found) { report.errors.push(`#${i} [insert] 父路径不存在 ${JSON.stringify(parentIdx)}`); continue; }
          if (!Array.isArray(pg.value)) { report.errors.push(`#${i} [insert] 父节点不是数组`); continue; }
          if (typeof pos !== 'number') { report.errors.push(`#${i} [insert] 最后一层 idx 必须是数字`); continue; }
          if (pos < 0 || pos > pg.value.length) { report.errors.push(`#${i} [insert] pos 越界 ${pos}/${pg.value.length}`); continue; }
        }
      }
      report.valid++;
    }
    return report;
  }

  /* =============================================================
   *  UI -> Patch 生成器
   * ============================================================= */

  /**
   * 坐标工具的一组点 -> patch
   */
  pointsToPatch(points, opts={}) {
    const key = opts.pointsKey || 'points';
    const mode = opts.mode || 'replace'; // replace / append / set
    const patch = [];
    if (mode === 'replace') {
      patch.push({ op:'set', idx:[key], value:[] });
      for (let i=0;i<points.length;i++){
        const p = points[i];
        patch.push({
          op:'insert', idx:[key, i],
          value: { grid:[p.gx, p.gy], norm:{x:p.normX,y:p.normY}, label:p.label, color:p.color, anchor:!!p.anchor }
        });
      }
    } else if (mode === 'append') {
      patch.push({ op:'set', idx:[key, '__len__'], value: points.length });
    } else {
      for (let i=0;i<points.length;i++){
        const p = points[i];
        patch.push({ op:'set', idx:[key, i], key:i, value:{ grid:[p.gx,p.gy], norm:{x:p.normX,y:p.normY}, label:p.label } });
      }
    }
    return patch;
  }

  /**
   * 滑块快照 -> patch
   */
  paramsToPatch(snapshot, opts={}) {
    const key = opts.paramsKey || 'params';
    const patch = [];
    for (const [k, v] of Object.entries(snapshot)) {
      patch.push({ op:'set', idx:[key, k], key:k, value:v });
    }
    return patch;
  }

  /**
   * 游戏状态 -> patch
   */
  gameStateToPatch(game, opts={}) {
    const root = opts.root || 'lifeState';
    const dims = {};
    for (const d of game.dimensions) dims[d.id] = { value: d.value, name: d.name, color: d.color };
    return [
      { op:'set', idx:[root,'dimensions'], value: dims },
      { op:'set', idx:[root,'meaning'], value: game.meaning },
      { op:'set', idx:[root,'entropy'], value: game.entropy },
      { op:'set', idx:[root,'flow'], value: game.flowState },
      { op:'set', idx:[root,'weights'], value: {...game.weights} },
      { op:'set', idx:[root,'updatedAt'], value: new Date().toISOString() },
    ];
  }

  /**
   * 把 patch 转成"人类可读"的自然语言操作序列（用于 audit）
   */
  patchToDescription(patch) {
    if (!Array.isArray(patch)) patch = [patch];
    return patch.map((p,i) => {
      const pk = this.indexer.pathKey(p.idx || p.from || []);
      switch (p.op) {
        case 'set':    return `  ${i+1}. ✏  SET    ${pk}  ➜  ${this._truncate(JSON.stringify(p.value))}`;
        case 'insert': return `  ${i+1}. ➕ INSERT ${pk}  ➜  ${this._truncate(JSON.stringify(p.value))}`;
        case 'delete': return `  ${i+1}. 🗑  DELETE ${pk}`;
        case 'rename': return `  ${i+1}. ✏  RENAME ${pk}  ➜  "${p.key}"`;
        case 'move':   return `  ${i+1}. ⇄  MOVE   ${this.indexer.pathKey(p.from)}  ➜  ${this.indexer.pathKey(p.to)}`;
        case 'copy':   return `  ${i+1}. 📋 COPY   ${this.indexer.pathKey(p.from)}  ➜  ${this.indexer.pathKey(p.to)}`;
        case 'merge':  return `  ${i+1}. ⧉  MERGE  ${pk}  ➜  keys(${Object.keys(p.value||{}).length})`;
        default:       return `  ${i+1}. ?  UNKNOWN ${JSON.stringify(p)}`;
      }
    }).join('\n');
  }
  _truncate(s, n=80) { return s.length > n ? s.slice(0,n) + '…' : s; }
}

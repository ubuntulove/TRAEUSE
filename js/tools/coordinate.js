/* =========================================================
 * coordinate.js — 📍 坐标定位工具
 *  - 2D 网格画布，吸附到网格/锚点
 *  - 支持多选、框选、标记
 *  - 直接输出 JSON 片段，替代自然语言描述
 * ========================================================= */
class CoordinateTool {
  constructor() {
    this.canvas = null;
    this.ctx = null;
    this.density = 12; // 每边 12 格
    this.w = 420; this.h = 320;
    this.dpr = 1;
    this.points = []; // 选中的点 [{x,y,label,color}]
    this.anchors = []; // 预设锚点
    this.hover = null;
    this.dragStart = null;
    this.selectRect = null;
    this._onOutput = null;
  }
  init() {
    this.canvas = document.getElementById('coord-canvas');
    this.ctx = this.canvas.getContext('2d');
    this._setupDPR();
    this._genAnchors();

    // 事件
    this.canvas.addEventListener('mousemove', (e) => this._onMove(e));
    this.canvas.addEventListener('click', (e) => this._onClick(e));
    this.canvas.addEventListener('mousedown', (e) => this._onDown(e));
    this.canvas.addEventListener('mouseup', (e) => this._onUp(e));
    this.canvas.addEventListener('contextmenu', (e) => this._onRight(e));

    document.getElementById('coord-density').addEventListener('input', (e) => {
      this.density = parseInt(e.target.value) || 12;
      document.getElementById('coord-density-val').textContent = this.density;
      this._redraw();
    });
    document.getElementById('coord-emit').addEventListener('click', () => this._emit());

    window.addEventListener('resize', () => this._setupDPR());
    this._redraw();
  }
  setOnOutput(fn) { this._onOutput = fn; }

  _setupDPR() {
    this.dpr = window.devicePixelRatio || 1;
    const r = this.canvas.getBoundingClientRect();
    this.w = r.width; this.h = r.height;
    this.canvas.width = this.w * this.dpr;
    this.canvas.height = this.h * this.dpr;
    this.ctx.setTransform(this.dpr, 0, 0, this.dpr, 0, 0);
  }

  _genAnchors() {
    // 预定义几个有意义的锚点（可理解为游戏关卡关键点）
    this.anchors = [
      { x:0.2, y:0.3, label:'起点 / 出生',  color:'#22d3ee' },
      { x:0.5, y:0.5, label:'当下 / 此刻',  color:'#7c5cff' },
      { x:0.8, y:0.7, label:'远方 / 愿景',  color:'#c084fc' },
      { x:0.15, y:0.8, label:'阴影 / 恐惧',  color:'#ef4444' },
      { x:0.85, y:0.2, label:'高光 / 成就',  color:'#f59e0b' },
    ];
  }

  _getCoord(e) {
    const r = this.canvas.getBoundingClientRect();
    return { px: e.clientX - r.left, py: e.clientY - r.top };
  }

  _snap(px, py) {
    const cellW = this.w / this.density;
    const cellH = this.h / this.density;
    const gx = Math.round(px / cellW);
    const gy = Math.round(py / cellH);
    const nx = gx * cellW;
    const ny = gy * cellH;
    return { gx, gy, nx, ny,
      normX: +(gx / this.density).toFixed(3),
      normY: +(gy / this.density).toFixed(3),
    };
  }

  _findNearestAnchor(nx, ny) {
    let best = null, bd = Infinity;
    for (const a of this.anchors){
      const ax = a.x * this.w, ay = a.y * this.h;
      const d = Math.hypot(ax-nx, ay-ny);
      if (d < bd && d < 40){ bd = d; best = a; }
    }
    return best;
  }

  _onMove(e) {
    const { px, py } = this._getCoord(e);
    const snap = this._snap(px, py);
    const anchor = this._findNearestAnchor(snap.nx, snap.ny);
    this.hover = { ...snap, anchor, px, py };
    document.getElementById('coord-selected').textContent =
      `(${snap.gx}, ${snap.gy}) ≈ (${snap.normX}, ${snap.normY})`;
    document.getElementById('coord-anchor').textContent =
      anchor ? `${anchor.label} (${anchor.x},${anchor.y})` : '—';
    // 框选
    if (this.dragStart){
      this.selectRect = {
        x0: Math.min(this.dragStart.px, px),
        y0: Math.min(this.dragStart.py, py),
        x1: Math.max(this.dragStart.px, px),
        y1: Math.max(this.dragStart.py, py),
      };
    }
    this._redraw();
    if (window.__audio) window.__audio.triggerClick(Math.random()*0.5 + snap.normX*0.5);
  }

  _onDown(e){
    const { px, py } = this._getCoord(e);
    if (e.button === 0) this.dragStart = { px, py, shift:e.shiftKey };
  }

  _onUp(e){
    if (!this.dragStart) return;
    const { px, py } = this._getCoord(e);
    const dist = Math.hypot(px-this.dragStart.px, py-this.dragStart.py);
    if (dist > 6 && this.selectRect){
      // 框选：批量添加
      const added = this._selectInRect(this.selectRect);
      this._emitText(`// 框选添加了 ${added} 个坐标点`);
    }
    this.dragStart = null;
    this.selectRect = null;
    this._redraw();
  }

  _onClick(e) {
    if (this.dragStart && this.selectRect) return; // 框选已经处理
    const { px, py } = this._getCoord(e);
    const snap = this._snap(px, py);
    const anchor = this._findNearestAnchor(snap.nx, snap.ny);
    if (e.shiftKey){
      // Shift+Click: 删除最近点
      const idx = this._findPointIdx(snap.nx, snap.ny);
      if (idx >= 0){ this.points.splice(idx,1); this._redraw(); return; }
    }
    const label = anchor ? anchor.label : `P${this.points.length+1}`;
    const color = anchor ? anchor.color : this._pickColor(this.points.length);
    this.points.push({
      gx: snap.gx, gy: snap.gy,
      nx: snap.nx, ny: snap.ny,
      normX: snap.normX, normY: snap.normY,
      label, color,
      anchor: !!anchor,
      t: Date.now(),
    });
    this._redraw();
    if (window.__audio) window.__audio.triggerClick(snap.normX);
  }

  _onRight(e) {
    e.preventDefault();
    const { px, py } = this._getCoord(e);
    const snap = this._snap(px, py);
    const idx = this._findPointIdx(snap.nx, snap.ny);
    if (idx >= 0) this.points.splice(idx,1);
    this._redraw();
  }

  _findPointIdx(nx, ny, tol=14){
    let best = -1, bd = tol;
    for (let i=0;i<this.points.length;i++){
      const d = Math.hypot(this.points[i].nx-nx, this.points[i].ny-ny);
      if (d < bd){ bd = d; best = i; }
    }
    return best;
  }
  _selectInRect(rect){
    const { x0,y0,x1,y1 } = rect;
    let count = 0;
    for (let i=0;i<=this.density;i++){
      for (let j=0;j<=this.density;j++){
        const cellW = this.w/this.density, cellH = this.h/this.density;
        const x = i*cellW, y = j*cellH;
        if (x>=x0&&x<=x1&&y>=y0&&y<=y1){
          const exists = this.points.some(p => p.gx===i && p.gy===j);
          if (!exists){
            const anchor = this._findNearestAnchor(x,y);
            this.points.push({
              gx:i, gy:j, nx:x, ny:y,
              normX: +(i/this.density).toFixed(3),
              normY: +(j/this.density).toFixed(3),
              label: anchor?anchor.label:`P${this.points.length+1}`,
              color: anchor?anchor.color:this._pickColor(this.points.length),
              anchor: !!anchor, t:Date.now(),
            });
            count++;
          }
        }
      }
    }
    return count;
  }

  _pickColor(i){
    const palette = ['#22d3ee','#7c5cff','#c084fc','#f59e0b','#22c55e','#ec4899','#ef4444','#38bdf8'];
    return palette[i % palette.length];
  }

  _redraw() {
    const ctx = this.ctx;
    ctx.clearRect(0,0,this.w,this.h);
    // 背景渐变
    const g = ctx.createLinearGradient(0,0,this.w,this.h);
    g.addColorStop(0,'rgba(124,92,255,0.08)');
    g.addColorStop(1,'rgba(34,211,238,0.06)');
    ctx.fillStyle = g;
    ctx.fillRect(0,0,this.w,this.h);

    // 网格
    ctx.strokeStyle = 'rgba(120,140,255,0.15)';
    ctx.lineWidth = 1;
    const cellW = this.w / this.density;
    const cellH = this.h / this.density;
    for (let i=0;i<=this.density;i++){
      ctx.beginPath();
      ctx.moveTo(i*cellW, 0); ctx.lineTo(i*cellW, this.h);
      ctx.stroke();
      ctx.beginPath();
      ctx.moveTo(0, i*cellH); ctx.lineTo(this.w, i*cellH);
      ctx.stroke();
    }
    // 高亮中心 & 轴
    ctx.strokeStyle = 'rgba(124,92,255,0.3)';
    ctx.setLineDash([5,5]);
    ctx.beginPath();
    ctx.moveTo(0, this.h/2); ctx.lineTo(this.w, this.h/2);
    ctx.moveTo(this.w/2, 0); ctx.lineTo(this.w/2, this.h);
    ctx.stroke();
    ctx.setLineDash([]);

    // 坐标轴刻度
    ctx.fillStyle = 'rgba(154,163,201,0.6)';
    ctx.font = '10px monospace';
    for (let i=0;i<=this.density;i+=Math.max(1,Math.floor(this.density/6))){
      ctx.fillText((i/this.density).toFixed(2), i*cellW - 12, this.h - 4);
    }
    for (let j=0;j<=this.density;j+=Math.max(1,Math.floor(this.density/5))){
      ctx.fillText((j/this.density).toFixed(2), 4, j*cellH + 4);
    }

    // 锚点
    for (const a of this.anchors){
      const x = a.x*this.w, y = a.y*this.h;
      ctx.beginPath();
      ctx.arc(x, y, 14, 0, Math.PI*2);
      ctx.strokeStyle = a.color + '66';
      ctx.lineWidth = 2;
      ctx.stroke();
      ctx.beginPath();
      ctx.arc(x, y, 3, 0, Math.PI*2);
      ctx.fillStyle = a.color;
      ctx.fill();
      ctx.fillStyle = a.color;
      ctx.font = '10.5px PingFang SC,sans-serif';
      ctx.fillText(a.label, x + 10, y - 10);
    }

    // 选中点
    for (const p of this.points){
      ctx.beginPath();
      ctx.arc(p.nx, p.ny, 5, 0, Math.PI*2);
      ctx.fillStyle = p.color;
      ctx.shadowBlur = 10; ctx.shadowColor = p.color;
      ctx.fill();
      ctx.shadowBlur = 0;
      ctx.fillStyle = '#fff';
      ctx.font = '10px monospace';
      ctx.fillText(`(${p.gx},${p.gy})`, p.nx + 8, p.ny - 8);
    }

    // 悬停预览
    if (this.hover){
      ctx.beginPath();
      ctx.arc(this.hover.nx, this.hover.ny, 7, 0, Math.PI*2);
      ctx.strokeStyle = 'rgba(192,132,252,0.8)';
      ctx.lineWidth = 1.5;
      ctx.setLineDash([3,3]);
      ctx.stroke();
      ctx.setLineDash([]);
      // 十字准线
      ctx.strokeStyle = 'rgba(192,132,252,0.35)';
      ctx.beginPath();
      ctx.moveTo(this.hover.nx, 0); ctx.lineTo(this.hover.nx, this.h);
      ctx.moveTo(0, this.hover.ny); ctx.lineTo(this.w, this.hover.ny);
      ctx.stroke();
    }

    // 框选矩形
    if (this.selectRect){
      const { x0,y0,x1,y1 } = this.selectRect;
      ctx.fillStyle = 'rgba(124,92,255,0.15)';
      ctx.strokeStyle = 'rgba(192,132,252,0.8)';
      ctx.lineWidth = 1;
      ctx.fillRect(x0,y0,x1-x0,y1-y0);
      ctx.strokeRect(x0,y0,x1-x0,y1-y0);
    }

    // 右上角信息
    ctx.fillStyle = 'rgba(154,163,201,0.8)';
    ctx.font = '10.5px monospace';
    ctx.textAlign = 'right';
    ctx.fillText(`已选点: ${this.points.length} / 网格: ${this.density}×${this.density}`, this.w - 8, 14);
    ctx.fillText(`左键=添加  Shift+左键/右键=删除  拖拽=框选`, this.w - 8, 30);
    ctx.textAlign = 'left';
  }

  _emit() {
    if (this.points.length === 0) {
      this._emitText('// 还没有选择任何点。请在网格画布上点击。');
      return;
    }
    // 生成多种格式
    const simple = this.points.map(p => ({
      x: p.normX, y: p.normY, label: p.label
    }));
    const indexed = this.points.map((p, i) => ({
      idx: i, grid:[p.gx, p.gy], x:p.normX, y:p.normY,
      label: p.label, color: p.color, anchor: p.anchor
    }));
    const code = `// ========================================================
// 📍 CoordinateTool 输出 · ${this.points.length} 个点
// 格式 A: 简洁点集 (可直接用于图表/游戏)
const points_simple = ${JSON.stringify(simple, null, 2)};

// 格式 B: 带索引与网格的详细点集（供 Indexed JSON Writer 使用）
const points_indexed = ${JSON.stringify(indexed, null, 2)};

// 格式 C: 函数调用式生成（用于代码插入）
function getPoint(i){ return points_indexed[i]; }
// 便捷访问: 第一个锚点 = getPoint(0)
`;
    this._emitText(code);
    if (this._onOutput) this._onOutput({ type:'points', simple, indexed });
  }
  _emitText(text) {
    const el = document.getElementById('coord-output');
    el.textContent = text;
    // 高亮
    el.style.transition = 'border-color .3s';
    el.style.borderColor = '#c084fc';
    setTimeout(()=>el.style.borderColor='', 400);
  }
  getPoints() { return this.points.slice(); }
  clear() { this.points = []; this._redraw(); }
}

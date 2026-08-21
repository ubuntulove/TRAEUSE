/* =========================================================
 * renderer.js — 游戏可视化渲染
 *  - 背景星海粒子（对应历史事件）
 *  - 前景八维度雷达图
 *  - 意义值仪表 & 顿悟弹层
 * ========================================================= */
class Renderer {
  constructor(game, audio) {
    this.game = game;
    this.audio = audio;
    this.stars = [];
    this.particles = [];
    this.rafId = null;
    this.t = 0;
  }
  init() {
    this.bg = document.getElementById('starfield');
    this.fg = document.getElementById('radar');
    this._resize();
    window.addEventListener('resize', ()=>this._resize());
    this._genStars();
    this._loop();
    // 雷达点击：触发顿悟点
    this.fg.addEventListener('click', (e)=>this._onRadarClick(e));
    this.fg.style.cursor = 'pointer';
  }
  _resize() {
    const dpr = window.devicePixelRatio || 1;
    for (const c of [this.bg, this.fg]){
      const r = c.getBoundingClientRect();
      c.width = r.width * dpr;
      c.height = r.height * dpr;
      const ctx = c.getContext('2d');
      ctx.setTransform(dpr,0,0,dpr,0,0);
    }
    this.w = this.bg.getBoundingClientRect().width;
    this.h = this.bg.getBoundingClientRect().height;
  }
  _genStars() {
    this.stars = [];
    const N = 160;
    for (let i=0;i<N;i++){
      this.stars.push({
        x: Math.random()*this.w,
        y: Math.random()*this.h,
        r: Math.random()*1.4 + 0.2,
        vx: (Math.random()-0.5) * 0.08,
        vy: (Math.random()-0.5) * 0.08,
        a: Math.random()*0.8 + 0.2,
        phase: Math.random()*Math.PI*2,
      });
    }
  }
  _loop() {
    this.t++;
    this._drawBg();
    this._drawRadar();
    this._drawParticles();
    this.rafId = requestAnimationFrame(()=>this._loop());
  }
  _drawBg() {
    const ctx = this.bg.getContext('2d');
    ctx.clearRect(0,0,this.w,this.h);
    // 渐变星雾
    const grad = ctx.createRadialGradient(this.w/2, this.h/2, 50, this.w/2, this.h/2, this.w*0.7);
    grad.addColorStop(0,'rgba(124,92,255,0.08)');
    grad.addColorStop(1,'rgba(5,6,15,0)');
    ctx.fillStyle = grad;
    ctx.fillRect(0,0,this.w,this.h);
    // 星星
    for (const s of this.stars) {
      s.x += s.vx; s.y += s.vy;
      if (s.x < 0) s.x += this.w; if (s.x > this.w) s.x -= this.w;
      if (s.y < 0) s.y += this.h; if (s.y > this.h) s.y -= this.h;
      const twinkle = 0.6 + 0.4*Math.sin(this.t*0.02 + s.phase);
      ctx.beginPath();
      ctx.arc(s.x, s.y, s.r, 0, Math.PI*2);
      ctx.fillStyle = `rgba(220,230,255,${s.a*twinkle})`;
      ctx.shadowBlur = 8;
      ctx.shadowColor = 'rgba(124,92,255,.6)';
      ctx.fill();
    }
    ctx.shadowBlur = 0;
    // 意义值能量流：根据 meaning 画流动螺旋
    const m = this.game.meaning / 1000;
    const cx = this.w/2, cy = this.h/2;
    for (let i=0;i<3;i++){
      ctx.beginPath();
      for (let a=0;a<Math.PI*6;a+=0.05){
        const R = 20 + a*6*(0.5 + m);
        const x = cx + Math.cos(a + this.t*0.01 + i*2)*R*(0.8 + 0.2*Math.sin(this.t*0.005 + i));
        const y = cy + Math.sin(a + this.t*0.01 + i*2)*R*(0.8 + 0.2*Math.cos(this.t*0.005 + i));
        if (a===0) ctx.moveTo(x,y); else ctx.lineTo(x,y);
      }
      ctx.strokeStyle = `hsla(${260 - i*40 + m*60}, 85%, 70%, ${0.05 + m*0.12})`;
      ctx.lineWidth = 1 + m*2;
      ctx.stroke();
    }
  }
  _drawRadar() {
    const ctx = this.fg.getContext('2d');
    ctx.clearRect(0,0,this.w,this.h);
    const cx = this.w/2;
    const cy = this.h*0.47;
    const R = Math.min(this.w, this.h) * 0.28;
    const dims = this.game.dimensions;
    const N = dims.length;
    // 雷达网格
    ctx.strokeStyle = 'rgba(120,140,255,0.18)';
    ctx.fillStyle = 'rgba(120,140,255,0.04)';
    ctx.lineWidth = 1;
    for (let ring=1; ring<=5; ring++){
      ctx.beginPath();
      for (let i=0;i<=N;i++){
        const a = -Math.PI/2 + (i/N)*Math.PI*2;
        const r = R * ring/5;
        const x = cx + Math.cos(a)*r;
        const y = cy + Math.sin(a)*r;
        if (i===0) ctx.moveTo(x,y); else ctx.lineTo(x,y);
      }
      ctx.stroke();
      if (ring===5) ctx.fill();
    }
    // 轴线
    for (let i=0;i<N;i++){
      const a = -Math.PI/2 + (i/N)*Math.PI*2;
      ctx.beginPath();
      ctx.moveTo(cx, cy);
      ctx.lineTo(cx + Math.cos(a)*R*1.1, cy + Math.sin(a)*R*1.1);
      ctx.strokeStyle = 'rgba(120,140,255,0.15)';
      ctx.stroke();
    }
    // 数值多边形
    const pts = [];
    for (let i=0;i<N;i++){
      const a = -Math.PI/2 + (i/N)*Math.PI*2;
      const r = R * (dims[i].value/100);
      pts.push({ x: cx + Math.cos(a)*r, y: cy + Math.sin(a)*r });
    }
    // 渐变填充
    ctx.beginPath();
    pts.forEach((p,i)=> i===0 ? ctx.moveTo(p.x,p.y) : ctx.lineTo(p.x,p.y));
    ctx.closePath();
    const grad = ctx.createRadialGradient(cx, cy, 10, cx, cy, R);
    grad.addColorStop(0,'rgba(124,92,255,0.55)');
    grad.addColorStop(0.6,'rgba(34,211,238,0.28)');
    grad.addColorStop(1,'rgba(192,132,252,0.12)');
    ctx.fillStyle = grad;
    ctx.fill();
    ctx.strokeStyle = 'rgba(192,132,252,0.9)';
    ctx.lineWidth = 2;
    ctx.stroke();
    // 锚点节点
    for (let i=0;i<N;i++){
      const p = pts[i];
      ctx.beginPath();
      ctx.arc(p.x, p.y, 5, 0, Math.PI*2);
      ctx.fillStyle = dims[i].color;
      ctx.shadowBlur = 14; ctx.shadowColor = dims[i].color;
      ctx.fill();
      ctx.shadowBlur = 0;
    }
    // 标签
    ctx.font = '12px PingFang SC,Microsoft YaHei,sans-serif';
    for (let i=0;i<N;i++){
      const a = -Math.PI/2 + (i/N)*Math.PI*2;
      const x = cx + Math.cos(a)*R*1.28;
      const y = cy + Math.sin(a)*R*1.28;
      const d = dims[i];
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillStyle = d.color;
      ctx.shadowBlur = 8; ctx.shadowColor = d.color;
      ctx.fillText(`${d.emoji} ${d.name}`, x, y);
      ctx.shadowBlur = 0;
      ctx.fillStyle = '#c7d0ff';
      ctx.font = '11px monospace';
      ctx.fillText(`${Math.round(d.value)}`, x, y + 14);
      ctx.font = '12px PingFang SC,Microsoft YaHei,sans-serif';
    }
    // 中心意义水晶
    const pulse = 0.9 + 0.1*Math.sin(this.t*0.08);
    const crystR = 10 + (this.game.meaning/1000)*18;
    const cg = ctx.createRadialGradient(cx, cy, 0, cx, cy, crystR*2);
    cg.addColorStop(0, 'rgba(255,255,255,0.9)');
    cg.addColorStop(0.3,'rgba(192,132,252,0.6)');
    cg.addColorStop(1,'rgba(124,92,255,0)');
    ctx.fillStyle = cg;
    ctx.beginPath(); ctx.arc(cx, cy, crystR*2*pulse, 0, Math.PI*2); ctx.fill();
    ctx.fillStyle = '#fff';
    ctx.beginPath(); ctx.arc(cx, cy, 4*pulse, 0, Math.PI*2); ctx.fill();
  }
  _onRadarClick(e) {
    const r = this.fg.getBoundingClientRect();
    const x = e.clientX - r.left;
    const y = e.clientY - r.top;
    // 找到最近的维度锚点，将其升高 5-15
    const cx = this.w/2, cy = this.h*0.47;
    const dims = this.game.dimensions;
    const N = dims.length;
    let best = null, bestD = Infinity;
    for (let i=0;i<N;i++){
      const a = -Math.PI/2 + (i/N)*Math.PI*2;
      const R = Math.min(this.w, this.h) * 0.28;
      const ax = cx + Math.cos(a)*R;
      const ay = cy + Math.sin(a)*R;
      const d = (x-ax)**2 + (y-ay)**2;
      if (d < bestD){ bestD = d; best = dims[i]; }
    }
    if (best && bestD < 900) {
      const up = 5 + Math.floor(Math.random()*10);
      const delta = this.game.setDimension(best.id, best.value + up);
      this._burst(best.color,
        cx + Math.cos(-Math.PI/2 + (dims.indexOf(best)/N)*Math.PI*2) * Math.min(this.w,this.h)*0.28 * best.value/100,
        cy + Math.sin(-Math.PI/2 + (dims.indexOf(best)/N)*Math.PI*2) * Math.min(this.w,this.h)*0.28 * best.value/100,
        delta);
      this.audio?.triggerMeaningDelta(delta, this.game.meaning);
    } else {
      // 点在空白处：小顿悟
      this._burst('#c084fc', x, y, 0, true);
    }
  }
  _burst(color, x, y, delta, epiphany=false) {
    const n = epiphany ? 30 : 12 + Math.min(20, Math.abs(delta));
    for (let i=0;i<n;i++){
      const ang = Math.random()*Math.PI*2;
      const sp = (epiphany ? 2.5 : 1.2) + Math.random()*2.5;
      this.particles.push({
        x, y,
        vx: Math.cos(ang)*sp,
        vy: Math.sin(ang)*sp,
        life: 40 + Math.random()*20,
        maxLife: 60,
        color,
        size: 1.5 + Math.random()*2.5,
      });
    }
  }
  _drawParticles() {
    const ctx = this.fg.getContext('2d');
    for (let i = this.particles.length-1; i>=0; i--){
      const p = this.particles[i];
      p.x += p.vx; p.y += p.vy;
      p.vx *= 0.96; p.vy *= 0.96;
      p.life--;
      const alpha = Math.max(0, p.life / p.maxLife);
      ctx.beginPath();
      ctx.arc(p.x, p.y, p.size*alpha, 0, Math.PI*2);
      ctx.fillStyle = p.color;
      ctx.globalAlpha = alpha;
      ctx.shadowBlur = 10; ctx.shadowColor = p.color;
      ctx.fill();
      ctx.shadowBlur = 0;
      ctx.globalAlpha = 1;
      if (p.life <= 0) this.particles.splice(i,1);
    }
  }
  // 公共 API：弹顿悟层
  showEpiphany(text, tag='') {
    const layer = document.getElementById('epiphany-layer');
    const el = document.createElement('div');
    el.className = 'epiphany';
    el.innerHTML = text + (tag ? `<small>${tag}</small>` : '');
    layer.appendChild(el);
    setTimeout(()=>el.remove(), 4000);
  }
}

/* =========================================================
 * gameCore.js — 人生意义量化游戏核心
 *  - 八维度数值系统
 *  - 意义值(Meaning)聚合公式
 *  - 熵(Entropy)、流状态(Flow)推导
 *  - 状态历史与事件存储
 * ========================================================= */
class LifeGame {
  constructor() {
    // 八维度定义：id, name, emoji, 初始值(0-100), 颜色
    this.dimensions = [
      { id: 'health',    name: '健康',   emoji: '❤️',  value: 60, color: '#ef4444' },
      { id: 'growth',    name: '成长',   emoji: '🌱',  value: 50, color: '#22c55e' },
      { id: 'freedom',   name: '自由',   emoji: '🕊️',  value: 45, color: '#22d3ee' },
      { id: 'love',      name: '爱与联结', emoji: '💞', value: 55, color: '#ec4899' },
      { id: 'purpose',   name: '目标意义', emoji: '🎯', value: 40, color: '#7c5cff' },
      { id: 'wisdom',    name: '智慧',   emoji: '🦉',  value: 35, color: '#c084fc' },
      { id: 'contribution', name: '贡献',emoji: '🌟',  value: 30, color: '#f59e0b' },
      { id: 'presence',  name: '当下觉察', emoji: '🧘', value: 25, color: '#38bdf8' },
    ];

    // 派生属性
    this.meaning = 0;       // 综合意义值 0-1000
    this.entropy = 0;       // 熵（无序度）0-100
    this.flowState = '平静'; // 流状态
    this.nextStep = '请调整八维度参数开始';

    // 历史 & 事件
    this.history = [];      // {t, dims, meaning}
    this.events = [];       // 发生的事件
    this.tick = 0;

    // 权重：各维度对意义值的贡献系数（可被顿悟事件修改）
    this.weights = {
      health: 0.9, growth: 1.1, freedom: 1.0, love: 1.2,
      purpose: 1.4, wisdom: 1.0, contribution: 1.3, presence: 1.1,
    };

    // 内部回调
    this.onChange = null;
  }

  init() {
    this._recompute();
    this._pushHistory();
  }

  /* === 数值操作 === */
  setDimension(id, value) {
    const d = this.dimensions.find(x => x.id === id);
    if (!d) return;
    value = Math.max(0, Math.min(100, Number(value) || 0));
    const prev = d.value;
    d.value = value;
    const delta = value - prev;
    this._recompute();
    this._pushHistory();
    if (this.onChange) this.onChange({ type:'dim', id, prev, now:value, delta });
    return delta;
  }

  getDimension(id) {
    const d = this.dimensions.find(x => x.id === id);
    return d ? d.value : 0;
  }

  /* === 核心聚合算法 === */
  _recompute() {
    // 意义值 = Σ(dim * weight) * 协同系数 * 平衡系数
    let sum = 0;
    const vals = [];
    for (const d of this.dimensions) {
      const w = this.weights[d.id] || 1;
      sum += d.value * w;
      vals.push(d.value);
    }
    // 协同系数：如果所有维度都超过阈值，有额外加成
    const allThreshold = vals.every(v => v >= 30) ? 1.15 : 1.0;
    // 平衡系数：维度间越均衡，系数越高（使用变异系数的倒数）
    const mean = vals.reduce((a,b)=>a+b,0)/vals.length;
    const variance = vals.reduce((a,b)=>a+(b-mean)**2,0)/vals.length;
    const std = Math.sqrt(variance);
    const cv = mean === 0 ? 1 : std / mean; // 变异系数
    const balance = 1 / (1 + cv); // 0.5 ~ 1

    this.meaning = Math.round(sum * allThreshold * balance);
    this.meaning = Math.max(0, Math.min(1000, this.meaning));

    // 熵：数值变化的不稳定程度 + 高/低极端值的惩罚
    const extremes = vals.filter(v => v < 15 || v > 85).length;
    const histDelta = this.history.length >= 2
      ? Math.abs(this.history[this.history.length-1].meaning - this.history[this.history.length-2].meaning)
      : 0;
    this.entropy = Math.round(Math.min(100, extremes * 10 + histDelta * 0.3 + (1 - balance) * 30));

    // 流状态：根据熵和意义分档
    const m = this.meaning, e = this.entropy;
    if (m >= 800 && e < 40) this.flowState = '✨ 天人合一';
    else if (m >= 650) this.flowState = '🔥 深度沉浸';
    else if (m >= 500 && e < 50) this.flowState = '🎯 心流中';
    else if (m >= 350) this.flowState = '🌤 专注';
    else if (e > 70) this.flowState = '🌪 混乱焦虑';
    else if (m < 150) this.flowState = '🌫 迷茫';
    else this.flowState = '平静';

    // 下一步建议
    this.nextStep = this._suggestNext();
  }

  _suggestNext() {
    // 找出最低的两个维度
    const sorted = [...this.dimensions].sort((a,b)=>a.value-b.value);
    const [lo1, lo2] = sorted;
    const m = this.meaning;
    if (m < 200) return `先提升 ${lo1.emoji}${lo1.name} 与 ${lo2.emoji}${lo2.name}，它们是基础`;
    if (this.entropy > 65) return `当前熵值高，建议停下 — 提升 ${lo1.emoji}${lo1.name} 让系统稳定`;
    if (m >= 600 && sorted[0].value < 40) return `你已高度发展！但 ${lo1.emoji}${lo1.name} 仍是短板，补齐它将产生顿悟`;
    if (m >= 800) return `♾ 你已接近生命大和谐 — 向外 ${sorted[6].emoji}贡献 与向内 ${sorted[7].emoji}觉察 并行`;
    return `尝试提升 ${lo1.emoji}${lo1.name}，或滑动其他维度探索变化`;
  }

  /* === 历史记录 === */
  _pushHistory() {
    const snap = {
      t: this.tick++,
      dims: Object.fromEntries(this.dimensions.map(d => [d.id, d.value])),
      meaning: this.meaning,
      entropy: this.entropy,
    };
    this.history.push(snap);
    if (this.history.length > 200) this.history.shift();
  }

  /* === 事件系统（外部写入） === */
  addEvent(ev) {
    this.events.unshift({
      id: Math.random().toString(36).slice(2,9),
      time: new Date(),
      ...ev,
    });
    if (this.events.length > 50) this.events.pop();
  }

  /* === 顿悟：随机调整权重 === */
  epiphany() {
    // 随机选 2-3 个权重放大，同时选 1 个缩小 — 模拟视角切换
    const ids = this.dimensions.map(d=>d.id);
    const shuffled = ids.sort(()=>Math.random()-0.5);
    const boost = shuffled.slice(0, 2 + (Math.random()>.5?1:0));
    const reduce = shuffled.slice(-1)[0];
    const changes = [];
    for (const id of boost) {
      const prev = this.weights[id];
      const next = +(prev * (1.2 + Math.random()*0.5)).toFixed(2);
      this.weights[id] = next;
      changes.push({ id, prev, next, dir:'↑' });
    }
    {
      const prev = this.weights[reduce];
      const next = +(prev * (0.6 + Math.random()*0.2)).toFixed(2);
      this.weights[reduce] = next;
      changes.push({ id:reduce, prev, next, dir:'↓' });
    }
    this._recompute();
    if (this.onChange) this.onChange({ type:'epiphany', changes });
    return changes;
  }

  /* === 重置 === */
  reset() {
    for (const d of this.dimensions) d.value = 50;
    for (const k of Object.keys(this.weights)) this.weights[k] = 1.0;
    this.meaning = 0; this.entropy = 0;
    this.events = []; this.history = []; this.tick = 0;
    this._recompute();
    this._pushHistory();
    if (this.onChange) this.onChange({ type:'reset' });
  }
}

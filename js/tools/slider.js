/* =========================================================
 * slider.js — 🎚 滑块矩阵工具
 *  - 多轴参数调节
 *  - 支持添加/删除轴、锁定、快照
 *  - 预设 & 参数空间导出
 * ========================================================= */
class SliderMatrix {
  constructor() {
    this.axes = [];
    this.container = null;
    this.outEl = null;
    this.presets = {};
    this._onOutput = null;
  }
  init() {
    this.container = document.getElementById('slider-matrix');
    this.outEl = document.getElementById('slider-output');
    this._loadPresets();

    // 默认 5 个轴（与游戏维度相关）
    this._addAxis({ name:'intensity', min:0, max:100, value:50, step:1, desc:'强度' });
    this._addAxis({ name:'speed',     min:0.1, max:10, value:2, step:0.1, desc:'速度', locked:false });
    this._addAxis({ name:'entropy',   min:0, max:1, value:0.3, step:0.01, desc:'熵' });
    this._addAxis({ name:'coherence', min:0, max:1, value:0.65, step:0.01, desc:'一致性' });
    this._addAxis({ name:'resonance', min:-1, max:1, value:0.2, step:0.01, desc:'共振' });

    document.getElementById('slider-add').onclick = () => this._onAdd();
    document.getElementById('slider-snap').onclick = () => this._snapshot();
    document.getElementById('slider-lock').onclick = () => this._toggleAllLock();
    document.getElementById('slider-preset').onclick = () => this._showPresets();

    this._render();
  }
  setOnOutput(fn) { this._onOutput = fn; }

  _loadPresets() {
    this.presets = {
      '禅定':    [['intensity',15],['speed',0.4],['entropy',0.05],['coherence',0.95],['resonance',0.7]],
      '心流':    [['intensity',70],['speed',3.5],['entropy',0.25],['coherence',0.70],['resonance',0.8]],
      '狂想':    [['intensity',95],['speed',8],['entropy',0.85],['coherence',0.25],['resonance',-0.4]],
      '抑郁':    [['intensity',10],['speed',0.2],['entropy',0.75],['coherence',0.2],['resonance',-0.9]],
      '创作峰值': [['intensity',85],['speed',5],['entropy',0.35],['coherence',0.8],['resonance',0.9]],
    };
  }

  _addAxis(cfg) {
    this.axes.push({
      id: Math.random().toString(36).slice(2,8),
      name: cfg.name || `p${this.axes.length+1}`,
      desc: cfg.desc || cfg.name || '',
      min: cfg.min ?? 0,
      max: cfg.max ?? 100,
      step: cfg.step ?? 1,
      value: cfg.value ?? ((cfg.min??0)+(cfg.max??100))/2,
      locked: !!cfg.locked,
    });
  }

  _onAdd() {
    const name = prompt('新参数名（英文，用作对象 key）：', 'newAxis');
    if (!name) return;
    const desc = prompt('中文描述（可选）：', '') || '';
    const min = parseFloat(prompt('最小值：', '0')) || 0;
    const max = parseFloat(prompt('最大值：', '100')) || 100;
    const step = parseFloat(prompt('步长：', '1')) || 1;
    this._addAxis({ name, desc, min, max, step });
    this._render();
  }

  _render() {
    this.container.innerHTML = '';
    // 排序插入位置提示
    const addBtn = document.createElement('div');
    addBtn.className = 'add-placeholder';
    addBtn.textContent = '+ 或点此添加新参数轴';
    addBtn.onclick = () => this._onAdd();

    for (let i=0;i<this.axes.length;i++){
      const a = this.axes[i];
      const row = document.createElement('div');
      row.className = 'slider-row' + (a.locked?' locked':'');
      row.innerHTML = `
        <span class="dim-handle" title="拖拽可排序（示意）">⋮⋮</span>
        <input type="text" class="axis-name" value="${a.name}" data-i="${i}" title="参数名（对象key）"/>
        <div style="display:flex; align-items:center; gap:8px;">
          <input type="range" class="axis-range" min="${a.min}" max="${a.max}" step="${a.step}" value="${a.value}" data-i="${i}" ${a.locked?'disabled':''}/>
        </div>
        <input type="number" class="axis-number" min="${a.min}" max="${a.max}" step="${a.step}" value="${a.value}" data-i="${i}" ${a.locked?'disabled':''}/>
        <button class="lock-btn" data-i="${i}" title="${a.locked?'解锁':'锁定'}">${a.locked?'🔒':'🔓'}</button>
      `;
      this.container.appendChild(row);
    }
    this.container.appendChild(addBtn);
    this._bind();
    this._refreshOutput();
  }

  _bind() {
    // ranges
    this.container.querySelectorAll('.axis-range').forEach(r => {
      r.oninput = (e) => {
        const i = +e.target.dataset.i;
        this.axes[i].value = parseFloat(e.target.value);
        const num = this.container.querySelector(`.axis-number[data-i="${i}"]`);
        if (num) num.value = this.axes[i].value;
        this._refreshOutput();
      };
    });
    // number inputs
    this.container.querySelectorAll('.axis-number').forEach(r => {
      r.oninput = (e) => {
        const i = +e.target.dataset.i;
        let v = parseFloat(e.target.value);
        if (isNaN(v)) return;
        v = Math.max(this.axes[i].min, Math.min(this.axes[i].max, v));
        this.axes[i].value = v;
        const sld = this.container.querySelector(`.axis-range[data-i="${i}"]`);
        if (sld) sld.value = v;
        this._refreshOutput();
      };
    });
    // name inputs
    this.container.querySelectorAll('.axis-name').forEach(r => {
      r.onchange = (e) => {
        const i = +e.target.dataset.i;
        const v = e.target.value.trim().replace(/[^a-zA-Z0-9_$]/g, '');
        if (v) this.axes[i].name = v;
        e.target.value = this.axes[i].name;
        this._refreshOutput();
      };
    });
    // lock buttons
    this.container.querySelectorAll('.lock-btn').forEach(btn => {
      btn.onclick = (e) => {
        const i = +e.currentTarget.dataset.i;
        this.axes[i].locked = !this.axes[i].locked;
        this._render();
      };
    });
  }

  _toggleAllLock() {
    const allLocked = this.axes.every(a => a.locked);
    for (const a of this.axes) a.locked = !allLocked;
    this._render();
  }

  _snapshot() {
    const snap = this.export();
    const time = new Date().toLocaleTimeString();
    const code = `// 📸 快照 @ ${time}
// 当前参数空间（共 ${Object.keys(snap).length} 轴）
const params_${Date.now().toString(36)} = ${JSON.stringify(snap, null, 2)};

// 归一化版本 (0..1)
const params_normalized = ${JSON.stringify(this.exportNormalized(), null, 2)};

// 线性插值（t ∈ [0,1]）函数式
function lerpParams(other, t){
  const out = {};
  for (const k in params_current){
    const a = params_current[k], b = other[k] ?? a;
    out[k] = a + (b - a) * t;
  }
  return out;
}

// 当前引用
const params_current = params_${Date.now().toString(36)};
`;
    this.outEl.textContent = code;
    this.outEl.style.borderColor = '#22d3ee';
    setTimeout(()=>this.outEl.style.borderColor='', 400);
    if (this._onOutput) this._onOutput({ type:'snapshot', snapshot:snap, normalized:this.exportNormalized() });
  }

  _showPresets() {
    const names = Object.keys(this.presets);
    const q = prompt(
      '选择预设：\n' + names.map((n,i)=>`  ${i+1}. ${n}`).join('\n'),
      '1'
    );
    if (!q) return;
    const idx = parseInt(q);
    if (isNaN(idx) || idx < 1 || idx > names.length) return;
    const name = names[idx-1];
    const preset = this.presets[name];
    for (const [k, v] of preset) {
      const a = this.axes.find(x => x.name === k);
      if (a && !a.locked) a.value = v;
    }
    this._render();
    this.outEl.textContent = `// ✨ 已应用预设: "${name}"\n` +
      Object.entries(this.export()).map(([k,v])=>`${k} = ${v}`).join('\n');
    if (window.__audio) window.__audio.triggerEpiphany();
  }

  export() {
    const out = {};
    for (const a of this.axes) out[a.name] = +(a.value).toFixed(4);
    return out;
  }
  exportNormalized() {
    const out = {};
    for (const a of this.axes) {
      out[a.name] = a.max === a.min ? 0 : +((a.value - a.min) / (a.max - a.min)).toFixed(4);
    }
    return out;
  }
  getAxes() { return this.axes.slice(); }

  _refreshOutput() {
    const data = this.export();
    const norm = this.exportNormalized();
    const entries = this.axes.map(a => {
      const v = (+a.value).toFixed(4);
      const nv = norm[a.name];
      return `  ${a.name.padEnd(12)}: ${String(v).padStart(8)}  (norm=${nv.toFixed(3)})  ${a.locked?'🔒':''}`;
    });
    this.outEl.textContent = `// 实时参数空间\n${entries.join('\n')}\n\n// object:\n${JSON.stringify(data, null, 2)}`;
    if (this._onOutput) this._onOutput({ type:'change', data, normalized:norm });
  }
}

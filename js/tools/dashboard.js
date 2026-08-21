/* =========================================================
 * dashboard.js — AI 工具面板整合控制器
 *  - Tab 切换
 *  - 折叠/展开
 *  - 协调各工具到 JSON Writer
 *  - 更新状态栏
 * ========================================================= */
class Dashboard {
  constructor(game, coord, slider, transform) {
    this.game = game;
    this.coord = coord;
    this.slider = slider;
    this.transform = transform;
  }
  init() {
    this._bindUI();
    this._bindCoordToWriter();
    this._bindSliderToWriter();
    this._bindJsonPanel();
    this._initStatusBar();
    this._initDimPanel();
  }

  _bindUI() {
    // 折叠按钮
    const toolsZone = document.getElementById('tools-zone');
    document.getElementById('btn-tools').onclick = () => {
      const open = !document.body.classList.contains('tools-open');
      document.body.classList.toggle('tools-open', open);
      toolsZone.classList.toggle('collapsed', !open);
    };
    document.getElementById('btn-collapse-tools').onclick = () => {
      document.body.classList.remove('tools-open');
      toolsZone.classList.add('collapsed');
    };

    // Tab 切换
    document.querySelectorAll('.tab').forEach(tab => {
      tab.onclick = () => {
        document.querySelectorAll('.tab').forEach(t=>t.classList.remove('active'));
        document.querySelectorAll('.tab-panel').forEach(p=>p.classList.remove('active'));
        tab.classList.add('active');
        const id = 'tab-' + tab.dataset.tab;
        document.getElementById(id).classList.add('active');
      };
    });
  }

  _bindCoordToWriter() {
    this.coord.setOnOutput((res) => {
      // 自动把点集作为 Indexed JSON patch 生成候选
      if (res.type === 'points' && res.indexed.length > 0) {
        const patch = res.indexed.map(p => ({
          op: 'insert',
          idx: ['points', p.idx],
          value: {
            grid: p.grid,
            norm: { x: p.x, y: p.y },
            label: p.label,
            color: p.color,
          }
        }));
        this._pushPatchCandidate(patch, `坐标工具 · ${res.indexed.length} 个点`);
      }
    });
  }

  _bindSliderToWriter() {
    this.slider.setOnOutput((res) => {
      if (res.type === 'snapshot') {
        // 快照：对 "params" 对象做 set patch
        const patch = [];
        let i = 0;
        for (const [k,v] of Object.entries(res.snapshot)) {
          patch.push({ op:'set', idx:['params', k], key:k, value:v });
          i++;
        }
        this._pushPatchCandidate(patch, `滑块快照 · ${i} 个参数`);
      }
    });
  }

  _pushPatchCandidate(patch, label) {
    const ta = document.getElementById('json-patch');
    if (!ta) return;
    // 如果用户还没写，就填入；否则追加提示
    const current = ta.value.trim();
    const banner = `\n// ✨ 来自 ${label} 的候选补丁（可与其他合并）:\n`;
    if (!current) {
      ta.value = JSON.stringify(patch, null, 2);
    } else {
      ta.value = current + banner + JSON.stringify(patch, null, 2);
    }
    ta.style.borderColor = '#22c55e';
    setTimeout(()=>ta.style.borderColor='', 600);
  }

  _bindJsonPanel() {
    // 初始化 demo.json
    const demoPath = '/workspace/data/demo.json';
    const sourceEl = document.getElementById('json-source');
    const patchEl = document.getElementById('json-patch');
    const outputEl = document.getElementById('json-output');
    const fileSel = document.getElementById('json-target-file');

    const initial = {
      schema: 'lifeos/demo.v1',
      points: [
        { grid:[6,6], norm:{x:0.5,y:0.5}, label:'origin', color:'#7c5cff' }
      ],
      params: { intensity:50, speed:2, entropy:0.3, coherence:0.65, resonance:0.2 },
      notes: {
        v1: 'Indexed JSON Writer 演示 — 所有改动通过带索引补丁完成',
        v2: 'idx 字段使用路径数组（字符串=键名，数字=数组下标）'
      }
    };
    // 尝试写入本地（浏览器环境我们只放在内存；如果是 node 环境会写入）
    try {
      if (window.__writer && typeof window.__writer.setMemorySource === 'function') {
        window.__writer.setMemorySource(demoPath, initial);
      }
    } catch(e){}
    sourceEl.value = JSON.stringify(initial, null, 2);
    this._memorySource = initial;
    this._memoryPath = demoPath;

    // apply
    document.getElementById('json-apply').onclick = async () => {
      try {
        const patch = JSON.parse(patchEl.value);
        const source = JSON.parse(sourceEl.value);
        const result = this.transform.applyPatch(source, patch);
        if (result.errors && result.errors.length) {
          outputEl.innerHTML = '❌ 部分错误:\n' +
            result.errors.map(e=>`  · ${e}`).join('\n') +
            '\n\n✅ 已完成部分的结果:\n' +
            this._colorizeDiff(source, result.result);
        } else {
          outputEl.innerHTML = '✅ 补丁应用成功\n\n' + this._colorizeDiff(source, result.result);
          // 更新 source
          sourceEl.value = JSON.stringify(result.result, null, 2);
          this._memorySource = result.result;
          if (window.__audio) window.__audio.triggerClick(0.8);
        }
      } catch (e) {
        outputEl.textContent = '❌ 解析/应用失败: ' + e.message + '\n\n请检查 JSON 格式与 idx 路径格式。';
        if (window.__audio) window.__audio.triggerMeaningDelta(-5, 50);
      }
    };

    // generate
    document.getElementById('json-generate').onclick = () => {
      const pts = this.coord.getPoints();
      const snap = this.slider.export();
      const patch = [];
      pts.forEach((p,i) => {
        patch.push({
          op: 'set', idx: ['points', i+1],
          value: { grid:[p.gx,p.gy], norm:{x:p.normX, y:p.normY}, label:p.label, color:p.color }
        });
      });
      let i = 0;
      for (const [k,v] of Object.entries(snap)) {
        patch.push({ op:'set', idx:['params', k], key:k, value:v });
        i++;
      }
      patchEl.value = JSON.stringify(patch, null, 2);
      outputEl.textContent = `🧩 已从坐标(${pts.length}个点) + 滑块(${i}个参数) 生成补丁。点击「▶ 应用补丁」执行。`;
      if (window.__audio) window.__audio.triggerClick(0.6);
    };

    // validate
    document.getElementById('json-validate').onclick = () => {
      try {
        const patch = JSON.parse(patchEl.value);
        const source = JSON.parse(sourceEl.value);
        const report = this.transform.validate(patch, source);
        outputEl.textContent = '✓ 校验报告\n' +
          `  补丁条目：${report.total}\n` +
          `  有效条目：${report.valid}\n` +
          `  警告：${report.warnings.length}\n` +
          (report.warnings.length ? report.warnings.map(w=>'  ⚠ '+w).join('\n') : '') +
          `\n  错误：${report.errors.length}\n` +
          (report.errors.length ? report.errors.map(e=>'  ✗ '+e).join('\n') : '  无错误，可以安全应用。');
        if (report.errors.length === 0) {
          if (window.__audio) window.__audio.triggerClick(0.9);
        }
      } catch(e) {
        outputEl.textContent = '❌ 校验失败: ' + e.message;
      }
    };

    // 文件切换
    fileSel.onchange = () => {
      if (fileSel.value === 'memory') {
        sourceEl.value = JSON.stringify(this._memorySource, null, 2);
      } else {
        sourceEl.value = JSON.stringify(this._memorySource, null, 2);
      }
    };
  }

  _colorizeDiff(a, b) {
    const aLines = JSON.stringify(a, null, 2).split('\n');
    const bLines = JSON.stringify(b, null, 2).split('\n');
    // 简易 LCS diff（对代码演示足够）
    const m = aLines.length, n = bLines.length;
    const dp = Array.from({length:m+1}, ()=>new Array(n+1).fill(0));
    for (let i=1;i<=m;i++)for(let j=1;j<=n;j++){
      dp[i][j] = aLines[i-1]===bLines[j-1] ? dp[i-1][j-1]+1 : Math.max(dp[i-1][j], dp[i][j-1]);
    }
    const diff = [];
    let i=m, j=n;
    while(i>0 || j>0){
      if (i>0 && j>0 && aLines[i-1]===bLines[j-1]){ diff.push({t:'=', v:bLines[--j]}); i--; }
      else if (j>0 && (i===0 || dp[i][j-1]>=dp[i-1][j])){ diff.push({t:'+', v:bLines[--j]}); }
      else { diff.push({t:'-', v:aLines[--i]}); }
    }
    diff.reverse();
    let ln = 0;
    return diff.map(d => {
      if (d.t==='='){ ln++; return `<span class="json-ln-num">${ln}</span>${this._escape(d.v)}`; }
      if (d.t==='+'){ ln++; return `<span class="json-ln-num">${ln}</span><span class="json-diff-add">+ ${this._escape(d.v)}</span>`; }
      return `<span class="json-ln-num">${ln||''}</span><span class="json-diff-remove">- ${this._escape(d.v)}</span>`;
    }).join('\n');
  }
  _escape(s){ return String(s).replace(/[<>&]/g, c=>({'<':'&lt;','>':'&gt;','&':'&amp;'}[c])); }

  /* ====================================================
   *  状态栏更新
   * ==================================================== */
  _initStatusBar() {
    const clock = document.getElementById('clock');
    const entropy = document.getElementById('entropy');
    const flow = document.getElementById('flow-state');
    const next = document.getElementById('next-step');
    const meterFill = document.getElementById('meter-fill');
    const meterNum = document.getElementById('meter-number');
    const dimPanel = document.querySelector('.dim-grid');

    // 定时更新
    const tick = () => {
      const d = new Date();
      clock.textContent = d.toTimeString().slice(0,8);
      entropy.textContent = this.game.entropy;
      flow.textContent = this.game.flowState;
      next.textContent = this.game.nextStep;
      const pct = this.game.meaning / 10; // 0..100
      meterFill.style.width = pct + '%';
      meterNum.textContent = String(this.game.meaning).padStart(4,'0');
    };
    tick();
    setInterval(tick, 500);
  }

  /* ====================================================
   *  八维度面板构建
   * ==================================================== */
  _initDimPanel() {
    const grid = document.querySelector('.dim-grid');
    grid.innerHTML = '';
    for (const d of this.game.dimensions) {
      const cell = document.createElement('div');
      cell.className = 'dim';
      cell.innerHTML = `
        <label class="dim-label" for="dim-${d.id}">
          <span class="emoji">${d.emoji}</span>${d.name}
        </label>
        <input type="range" id="dim-${d.id}" class="dim-slider"
               min="0" max="100" step="1" value="${d.value}"
               style="accent-color:${d.color};">
        <span class="dim-value" id="dimval-${d.id}">${Math.round(d.value)}</span>
      `;
      grid.appendChild(cell);
      const input = cell.querySelector('input');
      const label = cell.querySelector('.dim-value');
      input.addEventListener('input', () => {
        const v = +input.value;
        label.textContent = v;
        this.game.setDimension(d.id, v);
      });
    }
    // 双向同步：如果其他途径改变了维度值，也更新 UI
    const origOnChange = this.game.onChange;
    this.game.onChange = (ev) => {
      if (ev.type === 'dim') {
        const inp = document.getElementById('dim-' + ev.id);
        const val = document.getElementById('dimval-' + ev.id);
        if (inp) inp.value = ev.now;
        if (val) val.textContent = Math.round(ev.now);
      }
      if (ev.type === 'reset') {
        for (const dd of this.game.dimensions){
          const inp = document.getElementById('dim-' + dd.id);
          const val = document.getElementById('dimval-' + dd.id);
          if (inp) inp.value = dd.value;
          if (val) val.textContent = Math.round(dd.value);
        }
      }
      if (origOnChange) origOnChange(ev);
    };
  }
}

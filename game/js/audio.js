// audio.js — 用 Web Audio API 程序合成音效，无需任何外部音频文件
// 浏览器要求用户手势才能启动 AudioContext，故延迟到首次交互时初始化。

class AudioEngine {
  constructor() {
    this.ctx = null;
    this.master = null;
    this.enabled = true;
    this.muted = false;
  }

  /** 在用户首次交互时调用 */
  init() {
    if (this.ctx) return;
    const AC = window.AudioContext || window.webkitAudioContext;
    if (!AC) { this.enabled = false; return; }
    this.ctx = new AC();
    this.master = this.ctx.createGain();
    this.master.gain.value = 0.4;
    this.master.connect(this.ctx.destination);
  }

  setMuted(m) { this.muted = m; if (this.master) this.master.gain.value = m ? 0 : 0.4; }
  toggleMute() { this.setMuted(!this.muted); return this.muted; }

  /** 通用：播放一个带 ADSR 包络的振荡器 */
  _tone({ freq = 440, freqEnd = null, type = "sine", dur = 0.15, vol = 0.3, attack = 0.005, decay = 0.05 }) {
    if (!this.enabled || !this.ctx || this.muted) return;
    const t = this.ctx.currentTime;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(freq, t);
    if (freqEnd !== null) osc.frequency.exponentialRampToValueAtTime(Math.max(1, freqEnd), t + dur);

    gain.gain.setValueAtTime(0, t);
    gain.gain.linearRampToValueAtTime(vol, t + attack);
    gain.gain.exponentialRampToValueAtTime(0.0001, t + attack + decay + dur);

    osc.connect(gain).connect(this.master);
    osc.start(t);
    osc.stop(t + dur + 0.05);
  }

  /** 白噪声缓冲（用于爆炸） */
  _noiseBuffer() {
    const len = this.ctx.sampleRate * 1;
    const buf = this.ctx.createBuffer(1, len, this.ctx.sampleRate);
    const data = buf.getChannelData(0);
    for (let i = 0; i < len; i++) data[i] = Math.random() * 2 - 1;
    return buf;
  }

  _noise({ dur = 0.3, vol = 0.4, filterFreq = 1000, filterEnd = 200 }) {
    if (!this.enabled || !this.ctx || this.muted) return;
    const t = this.ctx.currentTime;
    const src = this.ctx.createBufferSource();
    src.buffer = this._noiseBuffer();
    const filter = this.ctx.createBiquadFilter();
    filter.type = "lowpass";
    filter.frequency.setValueAtTime(filterFreq, t);
    filter.frequency.exponentialRampToValueAtTime(filterEnd, t + dur);
    const gain = this.ctx.createGain();
    gain.gain.setValueAtTime(vol, t);
    gain.gain.exponentialRampToValueAtTime(0.0001, t + dur);
    src.connect(filter).connect(gain).connect(this.master);
    src.start(t);
    src.stop(t + dur + 0.05);
  }

  // ===== 音效集合 =====
  shoot()       { this._tone({ freq: 880, freqEnd: 320, type: "square",   dur: 0.08, vol: 0.12, decay: 0.04 }); }
  enemyShoot()  { this._tone({ freq: 220, freqEnd: 120, type: "sawtooth", dur: 0.12, vol: 0.08, decay: 0.06 }); }
  hit()         { this._tone({ freq: 200, freqEnd: 80,  type: "square",   dur: 0.1,  vol: 0.2,  decay: 0.08 }); }
  explode()     { this._noise({ dur: 0.35, vol: 0.4, filterFreq: 1800, filterEnd: 120 });
                  this._tone({ freq: 120, freqEnd: 40, type: "triangle", dur: 0.3, vol: 0.2, decay: 0.25 }); }
  pickup()      { this._tone({ freq: 660, type: "sine", dur: 0.08, vol: 0.2 });
                  setTimeout(() => this._tone({ freq: 990, type: "sine", dur: 0.12, vol: 0.2 }), 70); }
  shieldUp()    { this._tone({ freq: 440, freqEnd: 880, type: "sine", dur: 0.3, vol: 0.18, decay: 0.2 }); }
  playerDie()   { this._noise({ dur: 0.6, vol: 0.5, filterFreq: 1200, filterEnd: 80 });
                  this._tone({ freq: 300, freqEnd: 40, type: "sawtooth", dur: 0.7, vol: 0.25, decay: 0.5 }); }
  waveStart()   { this._tone({ freq: 330, type: "square", dur: 0.12, vol: 0.18 });
                  setTimeout(() => this._tone({ freq: 495, type: "square", dur: 0.18, vol: 0.18 }), 110); }
  uiClick()     { this._tone({ freq: 660, type: "square", dur: 0.05, vol: 0.15, decay: 0.02 }); }
  /** 受击屏幕震动的低频轰鸣 */
  boom()        { this._tone({ freq: 80, freqEnd: 30, type: "sine", dur: 0.5, vol: 0.3, decay: 0.4 }); }
}

export const Audio = new AudioEngine();

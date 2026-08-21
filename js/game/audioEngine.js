/* =========================================================
 * audioEngine.js — 音频引擎（Web Audio API 合成）
 *  意义值升高：和弦泛音 + 清脆叮声
 *  意义值降低：低沉衰减
 *  顿悟：金色琶音
 * ========================================================= */
class AudioEngine {
  constructor() {
    this.ctx = null;
    this.enabled = true;
    this.lastMeaning = -1;
    this.masterGain = null;
  }
  init() {
    try {
      this.ctx = new (window.AudioContext || window.webkitAudioContext)();
      this.masterGain = this.ctx.createGain();
      this.masterGain.gain.value = 0.15;
      this.masterGain.connect(this.ctx.destination);
    } catch(e) {
      this.enabled = false;
    }
    // 浏览器自动播放策略：首次点击后解锁
    const unlock = () => {
      if (this.ctx && this.ctx.state === 'suspended') this.ctx.resume();
      window.removeEventListener('pointerdown', unlock);
    };
    window.addEventListener('pointerdown', unlock);
  }
  _note(freq, dur=0.5, type='sine', gain=0.3, when=0) {
    if (!this.enabled || !this.ctx) return;
    const t = this.ctx.currentTime + when;
    const osc = this.ctx.createOscillator();
    const g = this.ctx.createGain();
    osc.type = type;
    osc.frequency.value = freq;
    g.gain.setValueAtTime(0.0001, t);
    g.gain.exponentialRampToValueAtTime(gain, t + 0.01);
    g.gain.exponentialRampToValueAtTime(0.0001, t + dur);
    osc.connect(g).connect(this.masterGain);
    osc.start(t); osc.stop(t + dur + 0.05);
  }
  // 五声音阶（C 大调五声）用于积极反馈
  _happyScale(offset=0) {
    const base = [261.63, 293.66, 329.63, 392.00, 440.00, 523.25];
    return base.map((f,i)=>f * Math.pow(2, offset/12));
  }
  // 反馈：意义值变化
  triggerMeaningDelta(delta, meaningNow) {
    if (!this.enabled) return;
    if (delta === 0) return;
    const scale = this._happyScale(0);
    if (delta > 0) {
      // 升高：叮声，越大越多个音
      const idx = Math.min(scale.length-1, Math.floor(meaningNow/180));
      const n = Math.min(3, 1 + Math.floor(delta/20));
      for (let i=0;i<n;i++){
        this._note(scale[Math.min(scale.length-1, idx+i)], 0.35 + i*0.1, 'triangle', 0.2, i*0.06);
      }
    } else {
      // 降低：低频
      this._note(110 + Math.random()*30, 0.4, 'sawtooth', 0.12);
      this._note(80, 0.5, 'sine', 0.1, 0.05);
    }
  }
  // 顿悟：金色琶音
  triggerEpiphany() {
    if (!this.enabled) return;
    const arp = [523.25, 659.25, 783.99, 1046.50, 1318.51, 1567.98];
    arp.forEach((f,i)=>this._note(f, 0.5, 'sine', 0.18, i*0.08));
    arp.slice().reverse().forEach((f,i)=>this._note(f*0.5, 0.7, 'triangle', 0.1, 0.5 + i*0.08));
  }
  // 冥想：低频432Hz + 缓慢脉冲
  triggerMeditate(seconds=4) {
    if (!this.enabled) return;
    for (let i=0;i<seconds;i++){
      this._note(432, 1.6, 'sine', 0.22, i*1.1);
      this._note(216, 1.8, 'triangle', 0.1, i*1.1 + 0.3);
    }
  }
  // 坐标点击：高频空间音效
  triggerClick(freqScale=0.5) {
    if (!this.enabled) return;
    const f = 440 + freqScale * 880;
    this._note(f, 0.08, 'square', 0.08);
    this._note(f*1.5, 0.08, 'sine', 0.06, 0.02);
  }
}

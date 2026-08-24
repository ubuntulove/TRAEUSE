/**
 * audio.js — 基于 Web Audio API 的程序化音效合成器
 * 不依赖任何音频文件，用振荡器实时合成吃食物、道具、死亡等音效。
 */
(function (global) {
  "use strict";

  const SFX = {
    eat:    { freq: 660, type: "square",   dur: 0.08, ramp: 1.2 },
    gold:   { freq: 880, type: "triangle", dur: 0.18, ramp: 1.6 },
    power:  { freq: 440, type: "sawtooth", dur: 0.25, ramp: 2.0 },
    die:    { freq: 220, type: "sawtooth", dur: 0.6,  ramp: 0.25 },
    level:  { freq: 523, type: "triangle", dur: 0.3,  ramp: 1.8 },
    click:  { freq: 980, type: "square",   dur: 0.04, ramp: 1.0 },
  };

  class AudioManager {
    constructor() {
      this.ctx = null;
      this.master = null;
      this.muted = Storage.isMuted();
    }

    // 懒加载 AudioContext：必须在用户首次交互后创建，否则浏览器会拦截。
    ensure() {
      if (this.ctx) return;
      const Ctx = global.AudioContext || global.webkitAudioContext;
      if (!Ctx) return;
      this.ctx = new Ctx();
      this.master = this.ctx.createGain();
      this.master.gain.value = this.muted ? 0 : 0.18;
      this.master.connect(this.ctx.destination);
    }

    resume() {
      if (this.ctx && this.ctx.state === "suspended") {
        this.ctx.resume();
      }
    }

    setMuted(v) {
      this.muted = v;
      Storage.setMuted(v);
      if (this.master) {
        this.master.gain.setTargetAtTime(v ? 0 : 0.18, this.ctx.currentTime, 0.01);
      }
    }

    /** 触发一个短促的合成音效 */
    play(name) {
      if (this.muted) return;
      this.ensure();
      if (!this.ctx) return;
      this.resume();

      const cfg = SFX[name] || SFX.click;
      const now = this.ctx.currentTime;
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = cfg.type;
      osc.frequency.setValueAtTime(cfg.freq, now);
      osc.frequency.exponentialRampToValueAtTime(
        Math.max(40, cfg.freq * cfg.ramp),
        now + cfg.dur
      );

      // ADSR 包络（简化版 attack + decay）
      gain.gain.setValueAtTime(0.0001, now);
      gain.gain.exponentialRampToValueAtTime(0.9, now + 0.008);
      gain.gain.exponentialRampToValueAtTime(0.0001, now + cfg.dur);

      osc.connect(gain);
      gain.connect(this.master);
      osc.start(now);
      osc.stop(now + cfg.dur + 0.02);
    }
  }

  global.Audio = new AudioManager();
})(window);

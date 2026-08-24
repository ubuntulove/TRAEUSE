/**
 * input.js — 输入控制：键盘 + 触摸滑动 + 虚拟方向键
 * 通过 Input.direction 给出当前期望方向，避免单帧内反向自杀。
 */
(function (global) {
  "use strict";

  const KEYS = {
    ArrowUp: "up", ArrowDown: "down", ArrowLeft: "left", ArrowRight: "right",
    KeyW: "up", KeyS: "down", KeyA: "left", KeyD: "right",
  };

  class InputManager {
    constructor() {
      this.nextDir = null;     // 待应用方向（下一逻辑帧生效）
      this.lastDir = "right";  // 上一个已应用方向，用于防反向
      this.onPause = null;     // 暂停回调（外部注入）
      this.onMute = null;      // 静音回调
      this.onRestart = null;   // 死亡后重新开始
    }

    bind() {
      window.addEventListener("keydown", (e) => this._onKey(e), { passive: false });
      this._bindTouch();
    }

    _onKey(e) {
      // 全局快捷键
      if (e.code === "Space") {
        e.preventDefault();
        if (this.onPause) this.onPause();
        return;
      }
      if (e.code === "KeyM") {
        if (this.onMute) this.onMute();
        return;
      }
      if (e.code === "Enter" && this.onRestart) {
        this.onRestart();
        return;
      }

      const dir = KEYS[e.code];
      if (dir) {
        e.preventDefault();
        this.request(dir);
      }
    }

    /** 提交一个方向请求，会过滤掉与当前方向完全相反的情况 */
    request(dir) {
      const opposite = { up: "down", down: "up", left: "right", right: "left" };
      if (dir === opposite[this.lastDir]) return; // 防止 180° 自杀
      this.nextDir = dir;
    }

    /** 由游戏循环在每个 tick 调用：取出并落实方向 */
    consume() {
      if (this.nextDir) {
        this.lastDir = this.nextDir;
        this.nextDir = null;
      }
      return this.lastDir;
    }

    reset() {
      this.nextDir = null;
      this.lastDir = "right";
    }

    _bindTouch() {
      const stage = document.querySelector(".stage");
      if (!stage) return;

      // 滑动手势
      let sx = 0, sy = 0, active = false;
      stage.addEventListener("touchstart", (e) => {
        const t = e.touches[0];
        sx = t.clientX; sy = t.clientY; active = true;
      }, { passive: true });

      stage.addEventListener("touchmove", (e) => {
        if (!active) return;
        const t = e.touches[0];
        const dx = t.clientX - sx;
        const dy = t.clientY - sy;
        const threshold = 24;
        if (Math.abs(dx) < threshold && Math.abs(dy) < threshold) return;

        if (Math.abs(dx) > Math.abs(dy)) {
          this.request(dx > 0 ? "right" : "left");
        } else {
          this.request(dy > 0 ? "down" : "up");
        }
        active = false; // 一次滑动只触发一次
      }, { passive: true });

      // 虚拟方向键
      document.querySelectorAll("[data-dir]").forEach((btn) => {
        const fire = (e) => {
          e.preventDefault();
          this.request(btn.dataset.dir);
        };
        btn.addEventListener("touchstart", fire, { passive: false });
        btn.addEventListener("mousedown", fire);
      });
    }
  }

  global.Input = new InputManager();
})(window);

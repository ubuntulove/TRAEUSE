/**
 * storage.js — 本地存档（最高分 + 静音偏好）
 * 暴露在 window.Storage 命名空间下，避免全局污染。
 */
(function (global) {
  "use strict";

  const KEY_BEST = "neon_snake.best";
  const KEY_MUTE = "neon_snake.muted";

  const Storage = {
    getBest() {
      const v = parseInt(localStorage.getItem(KEY_BEST), 10);
      return Number.isFinite(v) ? v : 0;
    },
    setBest(score) {
      const cur = this.getBest();
      if (score > cur) {
        localStorage.setItem(KEY_BEST, String(score));
        return true; // 表示刷新了记录
      }
      return false;
    },
    isMuted() {
      return localStorage.getItem(KEY_MUTE) === "1";
    },
    setMuted(v) {
      localStorage.setItem(KEY_MUTE, v ? "1" : "0");
    },
  };

  global.Storage = Storage;
})(window);

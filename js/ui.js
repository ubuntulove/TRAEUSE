/**
 * ui.js — 覆盖层与 HUD 的展示控制
 */
(function (global) {
  "use strict";

  const el = (id) => document.getElementById(id);

  class UI {
    constructor() {
      this.overlay   = el("overlay");
      this.panel     = el("overlay-panel");
      this.title     = el("overlay-title");
      this.subtitle  = el("overlay-subtitle");
      this.startBtn  = el("start-btn");
      this.scoreEl   = el("score");
      this.levelEl   = el("level");
      this.bestEl    = el("best");

      this.startBtn.addEventListener("click", () => {
        Audio.play("click");
        if (this.onStart) this.onStart();
      });
    }

    showOverlay(title, subtitle, btnText = "开始游戏") {
      this.overlay.classList.remove("is-hidden");
      this.title.textContent = title;
      this.subtitle.textContent = subtitle;
      this.startBtn.textContent = btnText;
      this.startBtn.style.display = btnText ? "" : "none";
    }

    hideOverlay() {
      this.overlay.classList.add("is-hidden");
    }

    setScore(v) { this.scoreEl.textContent = v; }
    setLevel(v) { this.levelEl.textContent = v; }
    setBest(v)  { this.bestEl.textContent = v; }

    /** 显示死亡界面 */
    showGameOver(score, best, isNewBest) {
      const subtitle = `本局得分 ${score}　|　最高 ${best}` +
        (isNewBest ? "　·　新纪录！" : "");
      this.showOverlay("GAME OVER", subtitle, "再来一局");
    }

    showPause() {
      this.showOverlay("已暂停", "按空格或点击按钮继续", "继续");
    }

    showStart() {
      this.showOverlay(
        "NEON SNAKE",
        "方向键 / WASD 移动　·　空格暂停　·　M 静音",
        "开始游戏"
      );
    }
  }

  global.UI = new UI();
})(window);

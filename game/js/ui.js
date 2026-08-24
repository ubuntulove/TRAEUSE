// ui.js — HUD 与覆盖层（菜单/暂停/结束）的 DOM 操作

export class UI {
  constructor() {
    this.el = {
      menu: document.getElementById("menu"),
      pause: document.getElementById("pause"),
      gameover: document.getElementById("gameover"),
      hud: document.getElementById("hud"),
      hp: document.getElementById("hp-bar"),
      shield: document.getElementById("shield-bar"),
      score: document.getElementById("score"),
      wave: document.getElementById("wave"),
      menuHi: document.getElementById("menu-hi"),
      finalScore: document.getElementById("final-score"),
      finalHi: document.getElementById("final-hi"),
      finalWave: document.getElementById("final-wave"),
      hint: document.getElementById("hint"),
    };
    this._hintTimer = 0;
  }

  showOverlay(name) {
    for (const k of ["menu", "pause", "gameover"]) {
      this.el[k].classList.toggle("active", k === name);
    }
    this.el.hud.style.opacity = name ? 0.3 : 1;
  }
  hideOverlays() {
    this.el.menu.classList.remove("active");
    this.el.pause.classList.remove("active");
    this.el.gameover.classList.remove("active");
    this.el.hud.style.opacity = 1;
  }

  updateHUD(game) {
    const p = game.player;
    this.el.hp.style.width = `${(p.hp / p.maxHp) * 100}%`;
    this.el.shield.style.width = `${(p.shield / p.maxShield) * 100}%`;
    this.el.score.textContent = game.score;
    this.el.wave.textContent = game.wave;
  }

  showHint(text) {
    this.el.hint.textContent = text;
    this.el.hint.classList.add("show");
    this._hintTimer = 1.6;
  }
  updateHint(dt) {
    if (this._hintTimer > 0) {
      this._hintTimer -= dt;
      if (this._hintTimer <= 0) this.el.hint.classList.remove("show");
    }
  }

  showGameOver(score, hi, wave) {
    this.el.finalScore.textContent = score;
    this.el.finalHi.textContent = hi;
    this.el.finalWave.textContent = wave;
    this.showOverlay("gameover");
  }

  setMenuHi(hi) { this.el.menuHi.textContent = hi; }
}

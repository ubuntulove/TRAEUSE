// input.js — 统一的键盘输入管理器（支持瞬时按下与持续按住）

class InputManager {
  constructor() {
    this.keys = new Set();          // 当前按住的键
    this.pressed = new Set();      // 本帧刚按下的键（用于"一次性"事件）
    this.justReleased = new Set(); // 本帧刚松开的键
    this.mouse = { x: 0, y: 0, down: false, pressed: false };

    this._onKeyDown = (e) => {
      // 防止页面滚动
      if ([" ", "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight"].includes(e.key)) {
        e.preventDefault();
      }
      if (!this.keys.has(e.code)) this.pressed.add(e.code);
      this.keys.add(e.code);
    };
    this._onKeyUp = (e) => {
      this.keys.delete(e.code);
      this.justReleased.add(e.code);
    };
    this._onBlur = () => this.keys.clear();
    this._onMouseDown = (e) => {
      this.mouse.down = true;
      this.mouse.pressed = true;
      this._updateMouse(e);
    };
    this._onMouseUp = () => { this.mouse.down = false; };
    this._onMouseMove = (e) => this._updateMouse(e);
    this._onContextMenu = (e) => e.preventDefault();

    window.addEventListener("keydown", this._onKeyDown);
    window.addEventListener("keyup", this._onKeyUp);
    window.addEventListener("blur", this._onBlur);
    window.addEventListener("mousedown", this._onMouseDown);
    window.addEventListener("mouseup", this._onMouseUp);
    window.addEventListener("mousemove", this._onMouseMove);
    window.addEventListener("contextmenu", this._onContextMenu);
  }

  _updateMouse(e) {
    const rect = canvas.getBoundingClientRect();
    const sx = canvas.width / rect.width;
    const sy = canvas.height / rect.height;
    this.mouse.x = (e.clientX - rect.left) * sx;
    this.mouse.y = (e.clientY - rect.top) * sy;
  }

  isDown(...codes) { return codes.some((c) => this.keys.has(c)); }
  wasPressed(...codes) { return codes.some((c) => this.pressed.has(c)); }
  wasReleased(...codes) { return codes.some((c) => this.justReleased.has(c)); }

  /** 每帧末调用，清空瞬时状态 */
  endFrame() {
    this.pressed.clear();
    this.justReleased.clear();
    this.mouse.pressed = false;
  }

  destroy() {
    window.removeEventListener("keydown", this._onKeyDown);
    window.removeEventListener("keyup", this._onKeyUp);
    window.removeEventListener("blur", this._onBlur);
    window.removeEventListener("mousedown", this._onMouseDown);
    window.removeEventListener("mouseup", this._onMouseUp);
    window.removeEventListener("mousemove", this._onMouseMove);
    window.removeEventListener("contextmenu", this._onContextMenu);
  }
}

// canvas 全局引用（由 main.js 在初始化时注入）
let canvas = document.getElementById("game");

export function setCanvas(c) { canvas = c; }

export const Input = new InputManager();

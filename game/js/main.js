// main.js — 入口：实例化游戏、绑定 UI 按钮、启动主循环
import { Game } from "./game.js";
import { setCanvas } from "./input.js";

const canvas = document.getElementById("game");
setCanvas(canvas);

const game = new Game(canvas);
game.ui.showOverlay("menu");
game.startLoop();

// 按钮绑定
const $ = (id) => document.getElementById(id);

$("btn-start").addEventListener("click", () => game.start());
$("btn-retry").addEventListener("click", () => game.start());
$("btn-menu").addEventListener("click", () => game.quitToMenu());
$("btn-resume").addEventListener("click", () => game.togglePause());
$("btn-quit").addEventListener("click", () => game.quitToMenu());

// 触屏：点击屏幕也能射击（移动端友好）
canvas.addEventListener("touchstart", (e) => {
  e.preventDefault();
}, { passive: false });

// 防止页面被空格/方向键滚动 —— 已在 input.js 处理，这里再保险一道
window.addEventListener("keydown", (e) => {
  if ([" ", "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight"].includes(e.key)) {
    e.preventDefault();
  }
}, { passive: false });

// 把 game 暴露到 window 方便调试
window.__game = game;

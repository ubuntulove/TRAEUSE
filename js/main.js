/**
 * main.js — 入口：装配各模块、绑定回调、启动主循环
 */
(function () {
  "use strict";

  const canvas = document.getElementById("game");
  const game = new Game(canvas);

  // 注入输入回调
  Input.onPause   = () => game.togglePause();
  Input.onMute    = () => {
    const next = !Audio.muted;
    Audio.setMuted(next);
    console.log(next ? "已静音" : "已开启音效");
  };
  Input.onRestart = () => game.restart();

  // 注入 UI 回调
  UI.onStart = () => game.start();

  // 初始界面
  UI.showStart();

  // 绑定输入
  Input.bind();

  // 启动主循环
  requestAnimationFrame((t) => game.loop(t));

  // 暴露到 window 方便调试
  window.__game = game;
})();

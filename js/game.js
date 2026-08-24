/**
 * game.js — 核心游戏循环、状态机、渲染、难度递增
 *
 * 状态：IDLE → PLAYING ⇄ PAUSED → GAME_OVER → IDLE
 * 使用固定时间步推进蛇的逻辑，渲染用 requestAnimationFrame 平滑插值。
 */
(function (global) {
  "use strict";

  const { Snake, Food, ParticleSystem, FloatingText, spawnFood, FOOD_TYPES } = Entities;

  const State = { IDLE: "idle", PLAYING: "playing", PAUSED: "paused", OVER: "over" };

  // 棋盘格子数（偶数便于居中起始）
  const GRID = 24;

  class Game {
    constructor(canvas) {
      this.canvas = canvas;
      this.ctx = canvas.getContext("2d");

      // 逻辑参数
      this.cell = canvas.width / GRID; // 每格像素
      this.state = State.IDLE;

      this.snake = null;
      this.food = null;
      this.particles = new ParticleSystem(300);
      this.floats = []; // 飘字数组

      this.score = 0;
      this.level = 1;
      this.eatenCount = 0;          // 本局累计吃到的食物数（用于升级）
      this.tickInterval = 0.16;     // 蛇推进间隔（秒），随关卡降低
      this.tickAccumulator = 0;    // 累计时间，达到 interval 则推进一帧

      // 用于平滑插值的渲染状态
      this.lastBody = [];           // 上一帧蛇身（用于插值）
      this.moveProgress = 0;        // 0..1，本 tick 内的进度

      this.best = Storage.getBest();
      UI.setBest(this.best);
    }

    /* --------------------------- 生命周期 --------------------------- */
    start() {
      this._reset();
      this.state = State.PLAYING;
      UI.hideOverlay();
      Input.reset();
    }

    togglePause() {
      if (this.state === State.PLAYING) {
        this.state = State.PAUSED;
        UI.showPause();
      } else if (this.state === State.PAUSED) {
        this.state = State.PLAYING;
        UI.hideOverlay();
      }
    }

    restart() {
      if (this.state === State.OVER || this.state === State.IDLE) this.start();
    }

    _reset() {
      const mid = Math.floor(GRID / 2);
      this.snake = new Snake(mid, mid, 3);
      this.score = 0;
      this.level = 1;
      this.eatenCount = 0;
      this.tickInterval = 0.14;
      this.tickAccumulator = 0;
      this.moveProgress = 0;
      this.particles.clear();
      this.floats.length = 0;
      this.lastBody = this.snake.body.map((c) => ({ ...c }));
      this._spawnFood();
      UI.setScore(0);
      UI.setLevel(1);
    }

    _spawnFood() {
      this.food = spawnFood(GRID, GRID, this.snake.occupiedCells());
    }

    /* ----------------------------- 主循环 ----------------------------- */
    loop(timestamp) {
      if (!this._lastTime) this._lastTime = timestamp;
      let dt = (timestamp - this._lastTime) / 1000;
      this._lastTime = timestamp;
      // 防止切后台后 dt 过大造成大跳步
      dt = Math.min(dt, 0.1);

      if (this.state === State.PLAYING) {
        this._update(dt);
      } else {
        // 暂停/结束时仍更新粒子，避免画面卡死
        this.particles.update(dt);
        this.floats = this.floats.filter((f) => f.update(dt));
      }

      this._render(dt);
      requestAnimationFrame((t) => this.loop(t));
    }

    _update(dt) {
      // 计时器（双倍/减速）
      this.snake.updateTimers(dt);

      // 推进逻辑帧
      this.tickAccumulator += dt;
      // 减速道具延长 tick 间隔
      const interval = this.snake.slowTimer > 0
        ? this.tickInterval * 1.8
        : this.tickInterval;

      while (this.tickAccumulator >= interval) {
        this.tickAccumulator -= interval;
        this._tick();
      }
      this.moveProgress = this.tickAccumulator / interval;

      // 食物动画
      if (this.food) this.food.update(dt);

      // 粒子 & 飘字
      this.particles.update(dt);
      this.floats = this.floats.filter((f) => f.update(dt));
    }

    /** 一个逻辑 tick：蛇推进一格、检测碰撞、吃食物 */
    _tick() {
      // 应用输入方向
      const dir = Input.consume();
      this.snake.setDirection(dir);

      // 记录推进前的身体，用于渲染插值
      this.lastBody = this.snake.body.map((c) => ({ ...c }));

      const newHead = this.snake.step();

      // 撞墙
      if (newHead.x < 0 || newHead.y < 0 ||
          newHead.x >= GRID || newHead.y >= GRID) {
        this._die("wall");
        return;
      }
      // 撞自己
      if (this.snake.hitsSelf(newHead)) {
        this._die("self");
        return;
      }

      // 吃食物
      if (this.food && newHead.x === this.food.x && newHead.y === this.food.y) {
        this._eat();
      }
    }

    _eat() {
      const f = this.food;
      const cfg = f.cfg;
      const cx = (f.x + 0.5) * this.cell;
      const cy = (f.y + 0.5) * this.cell;

      // 粒子爆炸
      this.particles.burst(cx, cy, cfg.color, cfg.score >= 50 ? 24 : 14, {
        speed: 130, life: 0.7, size: 4,
      });

      // 加分（考虑双倍）
      let gained = cfg.score;
      if (this.snake.doubleScore && cfg.score > 0) gained *= 2;
      this.score += gained;
      UI.setScore(this.score);

      // 飘字
      this.floats.push(new FloatingText(cx, cy, "+" + gained, cfg.color));

      // 生长
      this.snake.grow(cfg.grow);

      // 道具效果
      if (cfg.effect === "slow") {
        this.snake.slowTimer = 4.0;
        Audio.play("power");
      } else if (cfg.effect === "double") {
        this.snake.doubleScore = true;
        this.snake.doubleTimer = 6.0;
        Audio.play("power");
      } else if (f.type === "gold") {
        Audio.play("gold");
      } else {
        Audio.play("eat");
      }

      // 升级判定
      this.eatenCount++;
      const needForNext = 5 + (this.level - 1) * 2;
      if (this.eatenCount >= needForNext) {
        this._levelUp();
      }

      this._spawnFood();
    }

    _levelUp() {
      this.level++;
      // 越高等级越快，但有下限
      this.tickInterval = Math.max(0.05, 0.16 - (this.level - 1) * 0.012);
      UI.setLevel(this.level);
      Audio.play("level");

      // 升级粒子环
      const h = this.snake.head();
      const cx = (h.x + 0.5) * this.cell;
      const cy = (h.y + 0.5) * this.cell;
      this.particles.burst(cx, cy, "#ffd23f", 30, { speed: 180, life: 0.8, size: 4 });
      this.floats.push(new FloatingText(cx, cy - 10, "LV " + this.level, "#ffd23f"));
    }

    _die(reason) {
      this.state = State.OVER;
      this.snake.dead = true;
      Audio.play("die");

      // 死亡爆炸：整条蛇碎裂
      for (const seg of this.snake.body) {
        const cx = (seg.x + 0.5) * this.cell;
        const cy = (seg.y + 0.5) * this.cell;
        this.particles.burst(cx, cy, "#ff4060", 8, { speed: 140, life: 0.6, size: 3 });
      }

      const isNewBest = Storage.setBest(this.score);
      if (isNewBest) {
        this.best = this.score;
        UI.setBest(this.best);
      }
      setTimeout(() => UI.showGameOver(this.score, this.best, isNewBest), 600);
    }

    /* ----------------------------- 渲染 ----------------------------- */
    _render(dt) {
      const ctx = this.ctx;
      const W = this.canvas.width;
      const H = this.canvas.height;

      // 背景
      ctx.fillStyle = "#06061a";
      ctx.fillRect(0, 0, W, H);

      this._drawGrid(ctx);

      if (this.state === State.IDLE) {
        this._drawIdleSnake(ctx);
        return;
      }

      this._drawFood(ctx);
      this._drawSnake(ctx);
      this.particles.draw(ctx);
      for (const f of this.floats) f.draw(ctx);
    }

    _drawGrid(ctx) {
      ctx.strokeStyle = "rgba(120,120,220,0.06)";
      ctx.lineWidth = 1;
      ctx.beginPath();
      for (let i = 1; i < GRID; i++) {
        const p = i * this.cell;
        ctx.moveTo(p, 0); ctx.lineTo(p, this.canvas.height);
        ctx.moveTo(0, p); ctx.lineTo(this.canvas.width, p);
      }
      ctx.stroke();
    }

    /** 待机画面：静态展示一条蛇 + 中心食物 */
    _drawIdleSnake(ctx) {
      const mid = Math.floor(GRID / 2);
      const cells = [];
      for (let i = 0; i < 4; i++) cells.push({ x: mid - i + 1, y: mid });
      cells.forEach((c, i) => {
        this._drawCell(ctx, c.x, c.y, i === 0 ? "#00ffd1" : "rgba(0,255,209,0.7)", i === 0, 1 - i * 0.15);
      });
      // 一个示例食物
      this._drawFoodAt(ctx, mid + 3, mid, "normal", 1, performance.now() / 250);
    }

    _drawCell(ctx, gx, gy, color, isHead, alpha = 1) {
      const c = this.cell;
      const x = gx * c;
      const y = gy * c;
      const pad = c * 0.12;
      const r = c * 0.25; // 圆角

      ctx.globalAlpha = Math.max(0.1, alpha);
      ctx.fillStyle = color;
      ctx.shadowBlur = isHead ? 16 : 8;
      ctx.shadowColor = color;
      this._roundRect(ctx, x + pad, y + pad, c - pad * 2, c - pad * 2, r);
      ctx.fill();
      ctx.shadowBlur = 0;
      ctx.globalAlpha = 1;
    }

    _roundRect(ctx, x, y, w, h, r) {
      r = Math.min(r, w / 2, h / 2);
      ctx.beginPath();
      ctx.moveTo(x + r, y);
      ctx.arcTo(x + w, y, x + w, y + h, r);
      ctx.arcTo(x + w, y + h, x, y + h, r);
      ctx.arcTo(x, y + h, x, y, r);
      ctx.arcTo(x, y, x + w, y, r);
      ctx.closePath();
    }

    _drawFood(ctx) {
      if (!this.food) return;
      this._drawFoodAt(ctx, this.food.x, this.food.y, this.food.type, this.food.spawnAge, this.food.phase);
    }

    _drawFoodAt(ctx, gx, gy, type, spawnAge, phase) {
      const cfg = FOOD_TYPES[type];
      const c = this.cell;
      const cx = (gx + 0.5) * c;
      const cy = (gy + 0.5) * c;
      // 脉动半径
      const pulse = 1 + Math.sin(phase) * 0.08;
      const baseR = c * 0.32 * spawnAge * pulse;

      // 外发光圈
      const glow = ctx.createRadialGradient(cx, cy, 0, cx, cy, c * 0.8);
      glow.addColorStop(0, cfg.glow);
      glow.addColorStop(1, "rgba(0,0,0,0)");
      ctx.fillStyle = glow;
      ctx.beginPath();
      ctx.arc(cx, cy, c * 0.8, 0, Math.PI * 2);
      ctx.fill();

      // 主体
      ctx.fillStyle = cfg.color;
      ctx.shadowBlur = 14;
      ctx.shadowColor = cfg.color;
      ctx.beginPath();
      ctx.arc(cx, cy, Math.max(1, baseR), 0, Math.PI * 2);
      ctx.fill();

      // 高光
      ctx.shadowBlur = 0;
      ctx.fillStyle = "rgba(255,255,255,0.7)";
      ctx.beginPath();
      ctx.arc(cx - baseR * 0.3, cy - baseR * 0.3, Math.max(0.5, baseR * 0.3), 0, Math.PI * 2);
      ctx.fill();
    }

    _drawSnake(ctx) {
      if (!this.snake) return;
      const body = this.snake.body;
      const last = this.lastBody.length === body.length
        ? this.lastBody
        : body.map((c) => ({ ...c }));

      const t = this.state === State.PLAYING ? this.moveProgress : 0;
      const doubleOn = this.snake.doubleScore && (Math.floor(performance.now() / 150) % 2 === 0);

      // 从尾到头绘制，让头部覆盖在上
      for (let i = body.length - 1; i >= 0; i--) {
        const cur = body[i];
        const prev = last[i] || cur;
        // 插值
        const ix = prev.x + (cur.x - prev.x) * t;
        const iy = prev.y + (cur.y - prev.y) * t;
        const isHead = i === 0;
        let color = isHead ? "#9bffea" : "#00ffd1";
        if (doubleOn) color = isHead ? "#ff9bf0" : "#ff3df0";
        if (this.snake.slowTimer > 0) color = isHead ? "#a0e0ff" : "#6ad1ff";

        const alpha = 1 - (i / body.length) * 0.55; // 尾部渐淡
        this._drawInterpCell(ctx, ix, iy, color, isHead, alpha);
      }

      // 状态条（双倍/减速剩余时间）
      this._drawStatusBars(ctx);
    }

    _drawInterpCell(ctx, fx, fy, color, isHead, alpha) {
      const c = this.cell;
      const x = fx * c;
      const y = fy * c;
      const pad = c * 0.12;
      const r = c * 0.25;
      ctx.globalAlpha = Math.max(0.15, alpha);
      ctx.fillStyle = color;
      ctx.shadowBlur = isHead ? 18 : 8;
      ctx.shadowColor = color;
      this._roundRect(ctx, x + pad, y + pad, c - pad * 2, c - pad * 2, r);
      ctx.fill();

      // 蛇头眼睛
      if (isHead) {
        ctx.shadowBlur = 0;
        ctx.fillStyle = "#06061a";
        const dir = this.snake.direction;
        const cx = x + c / 2;
        const cy = y + c / 2;
        const off = c * 0.18;
        const er = Math.max(1, c * 0.07);
        let e1x = cx, e1y = cy, e2x = cx, e2y = cy;
        if (dir === "right") { e1x = cx + off; e1y = cy - off; e2x = cx + off; e2y = cy + off; }
        else if (dir === "left")  { e1x = cx - off; e1y = cy - off; e2x = cx - off; e2y = cy + off; }
        else if (dir === "up")     { e1x = cx - off; e1y = cy - off; e2x = cx + off; e2y = cy - off; }
        else                       { e1x = cx - off; e1y = cy + off; e2x = cx + off; e2y = cy + off; }
        ctx.beginPath(); ctx.arc(e1x, e1y, er, 0, Math.PI * 2); ctx.fill();
        ctx.beginPath(); ctx.arc(e2x, e2y, er, 0, Math.PI * 2); ctx.fill();
      }
      ctx.shadowBlur = 0;
      ctx.globalAlpha = 1;
    }

    _drawStatusBars(ctx) {
      const W = this.canvas.width;
      let y = 8;
      const bars = [];
      if (this.snake.doubleTimer > 0) {
        bars.push({ label: "2X", ratio: this.snake.doubleTimer / 6.0, color: "#ff3df0" });
      }
      if (this.snake.slowTimer > 0) {
        bars.push({ label: "SLOW", ratio: this.snake.slowTimer / 4.0, color: "#6ad1ff" });
      }
      for (const b of bars) {
        ctx.fillStyle = "rgba(0,0,0,0.4)";
        ctx.fillRect(8, y, 70, 10);
        ctx.fillStyle = b.color;
        ctx.fillRect(8, y, 70 * Math.max(0, b.ratio), 10);
        ctx.fillStyle = "#fff";
        ctx.font = "bold 9px sans-serif";
        ctx.textAlign = "left";
        ctx.fillText(b.label, 12, y + 8);
        y += 14;
      }
    }

    /* ----------------------------- 适配 ----------------------------- */
    // canvas 内部分辨率固定 600×600（HTML 定义），显示尺寸由 CSS 控制。
    // 这样所有逻辑坐标稳定，无需监听 resize。
  }

  Game.GRID = GRID;
  Game.State = State;
  global.Game = Game;
})(window);

/**
 * entities.js — 游戏实体：蛇、食物、道具、粒子系统
 * 全部使用 grid 坐标（{x,y}），渲染时由 game.js 乘以格子尺寸。
 */
(function (global) {
  "use strict";

  /* ----------------------------- 工具函数 ----------------------------- */
  const rand = (min, max) => Math.random() * (max - min) + min;
  const randInt = (min, max) => Math.floor(rand(min, max + 1));

  /* ============================== 蛇 ============================== */
  /**
   * Snake —— 网格化的蛇身，带头部方向、生长队列、特效（无敌帧/双倍/减速）。
   */
  class Snake {
    constructor(cx, cy, length = 3) {
      this.body = [];
      for (let i = 0; i < length; i++) this.body.push({ x: cx - i, y: cy });
      this.direction = "right";
      this.growQueue = 0;       // 待生长的段数（吃食物后累加）
      this.doubleScore = false; // 双倍分数状态
      this.doubleTimer = 0;     // 双倍剩余时间（秒）
      this.slowTimer = 0;       // 减速剩余时间（秒）
      this.dead = false;
    }

    head() { return this.body[0]; }

    setDirection(dir) {
      const opp = { up: "down", down: "up", left: "right", right: "left" };
      // 蛇身长度 > 1 时禁止直接反向
      if (this.body.length > 1 && dir === opp[this.direction]) return false;
      this.direction = dir;
      return true;
    }

    /** 推进一帧，返回新的头部坐标（不检测碰撞，由 game 处理） */
    step() {
      const h = this.head();
      const next = { x: h.x, y: h.y };
      switch (this.direction) {
        case "up":    next.y -= 1; break;
        case "down":  next.y += 1; break;
        case "left":  next.x -= 1; break;
        case "right": next.x += 1; break;
      }
      this.body.unshift(next);

      // 若没有待生长段，则去掉尾巴（保持长度）；否则保留尾巴（生长）
      if (this.growQueue > 0) {
        this.growQueue--;
      } else {
        this.body.pop();
      }
      return next;
    }

    grow(n = 1) { this.growQueue += n; }

    /** 自身碰撞检测：头部是否撞到身体（跳过紧邻头部的第一节，避免误判） */
    hitsSelf(head) {
      for (let i = 1; i < this.body.length; i++) {
        const s = this.body[i];
        if (s.x === head.x && s.y === head.y) return true;
      }
      return false;
    }

    /** 占用格集合，用于食物/道具避让 */
    occupiedCells() {
      const set = new Set();
      for (const s of this.body) set.add(s.x + "," + s.y);
      return set;
    }

    updateTimers(dt) {
      if (this.doubleTimer > 0) {
        this.doubleTimer -= dt;
        if (this.doubleTimer <= 0) { this.doubleTimer = 0; this.doubleScore = false; }
      }
      if (this.slowTimer > 0) {
        this.slowTimer -= dt;
        if (this.slowTimer < 0) this.slowTimer = 0;
      }
    }
  }

  /* ============================ 食物 / 道具 ============================ */
  const FOOD_TYPES = {
    normal: { score: 10, color: "#00ffd1", glow: "rgba(0,255,209,0.7)", grow: 1, weight: 70 },
    gold:   { score: 50, color: "#ffd23f", glow: "rgba(255,210,63,0.8)", grow: 2, weight: 12 },
    slow:   { score: 5,  color: "#6ad1ff", glow: "rgba(106,209,255,0.8)", grow: 1, weight: 9,  effect: "slow" },
    double: { score: 5,  color: "#ff3df0", glow: "rgba(255,61,240,0.8)", grow: 1, weight: 9,  effect: "double" },
  };

  /**
   * Food —— 单个食物/道具格子，自带类型、生成动画进度、脉动相位。
   */
  class Food {
    constructor(x, y, type = "normal") {
      this.x = x; this.y = y;
      this.type = type;
      this.cfg = FOOD_TYPES[type];
      this.spawnAge = 0;     // 0 → 1，生成时的弹出动画
      this.phase = Math.random() * Math.PI * 2; // 脉动相位
    }

    update(dt) {
      if (this.spawnAge < 1) this.spawnAge = Math.min(1, this.spawnAge + dt * 4);
      this.phase += dt * 4;
    }
  }

  /** 食物工厂：在空格上按权重随机生成 */
  function spawnFood(gridW, gridH, occupied) {
    const occupiedSet = occupied || new Set();

    // 加权随机类型
    const total = Object.values(FOOD_TYPES).reduce((s, c) => s + c.weight, 0);
    let r = Math.random() * total;
    let type = "normal";
    for (const [t, c] of Object.entries(FOOD_TYPES)) {
      r -= c.weight;
      if (r <= 0) { type = t; break; }
    }

    // 找一个空闲格
    let tries = 0;
    while (tries < 200) {
      const x = randInt(0, gridW - 1);
      const y = randInt(0, gridH - 1);
      if (!occupiedSet.has(x + "," + y)) {
        return new Food(x, y, type);
      }
      tries++;
    }
    return null; // 棋盘几乎填满
  }

  /* ============================== 粒子系统 ============================== */
  /**
   * ParticleSystem — 简单的池化粒子，用于吃食物爆炸、死亡碎片。
   * 粒子有位置、速度、生命、颜色；每帧更新位置并衰减生命。
   */
  class ParticleSystem {
    constructor(cap = 240) {
      this.pool = [];
      this.cap = cap;
    }

    burst(x, y, color, count = 12, opts = {}) {
      const speed = opts.speed || 90;
      const life = opts.life || 0.6;
      const size = opts.size || 3;
      for (let i = 0; i < count; i++) {
        if (this.pool.length >= this.cap) this.pool.shift();
        const ang = Math.random() * Math.PI * 2;
        const sp = rand(speed * 0.4, speed);
        this.pool.push({
          x, y,
          vx: Math.cos(ang) * sp,
          vy: Math.sin(ang) * sp,
          life, maxLife: life,
          color, size: rand(size * 0.6, size),
          gravity: opts.gravity || 0,
        });
      }
    }

    update(dt) {
      for (let i = this.pool.length - 1; i >= 0; i--) {
        const p = this.pool[i];
        p.x += p.vx * dt;
        p.y += p.vy * dt;
        p.vy += p.gravity * dt;
        p.vx *= 0.96; // 阻尼
        p.life -= dt;
        if (p.life <= 0) this.pool.splice(i, 1);
      }
    }

    draw(ctx) {
      for (const p of this.pool) {
        const alpha = Math.max(0, p.life / p.maxLife);
        ctx.globalAlpha = alpha;
        ctx.fillStyle = p.color;
        ctx.shadowBlur = 8;
        ctx.shadowColor = p.color;
        const r = Math.max(0.5, p.size * alpha);
        ctx.beginPath();
        ctx.arc(p.x, p.y, r, 0, Math.PI * 2);
        ctx.fill();
      }
      ctx.globalAlpha = 1;
      ctx.shadowBlur = 0;
    }

    clear() { this.pool.length = 0; }
  }

  /* ============================== 飘字 ============================== */
  /**
   * FloatingText — 拾取食物时显示 "+10" 之类的飘字。
   */
  class FloatingText {
    constructor(x, y, text, color) {
      this.x = x; this.y = y; this.text = text; this.color = color;
      this.life = 0.9; this.maxLife = 0.9;
    }
    update(dt) { this.y -= 36 * dt; this.life -= dt; return this.life > 0; }
    draw(ctx) {
      const a = Math.max(0, this.life / this.maxLife);
      ctx.globalAlpha = a;
      ctx.fillStyle = this.color;
      ctx.font = "bold 18px 'Segoe UI', sans-serif";
      ctx.textAlign = "center";
      ctx.shadowBlur = 8;
      ctx.shadowColor = this.color;
      ctx.fillText(this.text, this.x, this.y);
      ctx.globalAlpha = 1;
      ctx.shadowBlur = 0;
    }
  }

  global.Entities = {
    Snake, Food, ParticleSystem, FloatingText,
    spawnFood, FOOD_TYPES,
  };
})(window);

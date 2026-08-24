// enemy.js — 多种敌人类型 + 波次生成器
import { Pool, rand, randInt, pick, TAU } from "./utils.js";

class Enemy {
  constructor() { this.reset(); }
  reset() {
    this.x = 0; this.y = 0;
    this.vx = 0; this.vy = 0;
    this.r = 14;
    this.type = "grunt";
    this.hp = 1; this.maxHp = 1;
    this.score = 100;
    this.color = "#ff6b8a";
    this.fireCd = 0;
    this.fireRate = 2;
    this.t = 0;
    this.amp = 0; this.freq = 0; this.baseX = 0;
    this.dropChance = 0.08;
    this.isBoss = false;
    this.dead = false;
  }
  update(dt, game) {
    this.t += dt;
    switch (this.type) {
      case "grunt":
        this.y += this.vy * dt;
        break;
      case "zigzag":
        this.y += this.vy * dt;
        this.x = this.baseX + Math.sin(this.t * this.freq) * this.amp;
        break;
      case "hunter": {
        const dx = game.player.x - this.x, dy = game.player.y - this.y;
        const d = Math.hypot(dx, dy) || 1;
        this.x += (dx / d) * this.vy * dt;
        this.y += (dy / d) * this.vy * dt;
        break;
      }
      case "shooter":
        this.y += this.vy * dt;
        // 进入射程后减速并射击
        if (this.y > 80) this.vy = Math.max(20, this.vy * 0.96);
        this.fireCd -= dt;
        if (this.fireCd <= 0 && this.y > 40 && this.y < game.h - 80) {
          this.fireCd = this.fireRate;
          const dx = game.player.x - this.x, dy = game.player.y - this.y;
          const d = Math.hypot(dx, dy) || 1;
          const sp = 280;
          game.bullets.spawn({
            x: this.x, y: this.y + this.r,
            vx: (dx / d) * sp, vy: (dy / d) * sp,
            r: 4, side: "enemy", damage: 12, color: "#ff6b8a", life: 4,
          });
          game.audio.enemyShoot();
        }
        break;
      case "boss":
        // Boss 在顶部来回横移，定期射击弹幕
        this.x = this.baseX + Math.sin(this.t * this.freq) * this.amp;
        this.y = lerp(this.y, 110, 0.02);
        this.fireCd -= dt;
        if (this.fireCd <= 0) {
          this.fireCd = this.fireRate;
          this._bossFire(game);
        }
        break;
    }
    // 尾焰
    if (Math.random() < 0.3) {
      game.particles.trail(this.x, this.y - this.r, 0, -60, this.isBoss ? 280 : 340);
    }
  }
  _bossFire(game) {
    const n = 8;
    for (let i = 0; i < n; i++) {
      const a = (i / n) * TAU + this.t * 0.5;
      game.bullets.spawn({
        x: this.x, y: this.y + this.r,
        vx: Math.cos(a) * 220, vy: Math.sin(a) * 220,
        r: 5, side: "enemy", damage: 14, color: "#ff9a4d", life: 4,
      });
    }
    // 朝玩家额外一发
    const dx = game.player.x - this.x, dy = game.player.y - this.y;
    const d = Math.hypot(dx, dy) || 1;
    game.bullets.spawn({
      x: this.x, y: this.y,
      vx: (dx / d) * 320, vy: (dy / d) * 320,
      r: 6, side: "enemy", damage: 18, color: "#ff4d6d", life: 5,
    });
    game.audio.enemyShoot();
  }
  draw(ctx) {
    ctx.save();
    ctx.translate(this.x, this.y);
    ctx.shadowBlur = 12; ctx.shadowColor = this.color;

    switch (this.type) {
      case "grunt": {
        // 倒三角红色
        ctx.fillStyle = "#2a0a14";
        ctx.strokeStyle = this.color;
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(0, this.r);
        ctx.lineTo(this.r, -this.r * 0.7);
        ctx.lineTo(-this.r, -this.r * 0.7);
        ctx.closePath();
        ctx.fill(); ctx.stroke();
        break;
      }
      case "zigzag": {
        // 菱形紫色
        ctx.fillStyle = "#1a0a2a";
        ctx.strokeStyle = this.color;
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(0, -this.r);
        ctx.lineTo(this.r * 0.8, 0);
        ctx.lineTo(0, this.r);
        ctx.lineTo(-this.r * 0.8, 0);
        ctx.closePath();
        ctx.fill(); ctx.stroke();
        break;
      }
      case "hunter": {
        // 六边形橙黄，朝向玩家
        ctx.fillStyle = "#2a1a05";
        ctx.strokeStyle = this.color;
        ctx.lineWidth = 2;
        ctx.beginPath();
        for (let i = 0; i < 6; i++) {
          const a = (i / 6) * TAU + this.t * 1.5;
          const px = Math.cos(a) * this.r, py = Math.sin(a) * this.r;
          i ? ctx.lineTo(px, py) : ctx.moveTo(px, py);
        }
        ctx.closePath();
        ctx.fill(); ctx.stroke();
        break;
      }
      case "shooter": {
        // 梯形粉红
        ctx.fillStyle = "#2a0a1a";
        ctx.strokeStyle = this.color;
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(-this.r, -this.r * 0.6);
        ctx.lineTo(this.r, -this.r * 0.6);
        ctx.lineTo(this.r * 0.6, this.r * 0.6);
        ctx.lineTo(-this.r * 0.6, this.r * 0.6);
        ctx.closePath();
        ctx.fill(); ctx.stroke();
        // 炮口
        ctx.fillStyle = this.color;
        ctx.fillRect(-2, this.r * 0.4, 4, this.r * 0.5);
        break;
      }
      case "boss": {
        // 巨型多边形
        ctx.fillStyle = "#1a0510";
        ctx.strokeStyle = this.color;
        ctx.lineWidth = 3;
        ctx.beginPath();
        const sides = 8;
        for (let i = 0; i < sides; i++) {
          const a = (i / sides) * TAU + this.t * 0.6;
          const rr = this.r * (1 + 0.1 * Math.sin(this.t * 3 + i));
          const px = Math.cos(a) * rr, py = Math.sin(a) * rr;
          i ? ctx.lineTo(px, py) : ctx.moveTo(px, py);
        }
        ctx.closePath();
        ctx.fill(); ctx.stroke();
        // 核心
        ctx.fillStyle = "#fff";
        ctx.beginPath();
        ctx.arc(0, 0, this.r * 0.35, 0, TAU);
        ctx.fill();
        break;
      }
    }
    ctx.shadowBlur = 0;

    // 血条（多血敌人）
    if (this.hp < this.maxHp && this.maxHp > 1) {
      const w = this.r * 2, h = 4;
      ctx.fillStyle = "rgba(0,0,0,0.5)";
      ctx.fillRect(-w / 2, -this.r - 12, w, h);
      ctx.fillStyle = this.color;
      ctx.fillRect(-w / 2, -this.r - 12, w * (this.hp / this.maxHp), h);
    }
    ctx.restore();
  }
}

function lerp(a, b, t) { return a + (b - a) * t; }

export class EnemySystem {
  constructor() { this.pool = new Pool(() => new Enemy()); }

  /** 工厂：根据类型生成敌人 */
  spawn(type, opts = {}) {
    const e = this.pool.acquire();
    e.reset();
    e.type = type;
    e.x = opts.x ?? 0; e.y = opts.y ?? -30;
    e.baseX = opts.x ?? 0;
    const cfg = ENEMY_CONFIG[type];
    e.r = cfg.r;
    e.hp = cfg.hp; e.maxHp = cfg.hp;
    e.score = cfg.score;
    e.color = cfg.color;
    e.vy = cfg.speed;
    e.fireRate = cfg.fireRate ?? 2;
    e.amp = cfg.amp ?? 0; e.freq = cfg.freq ?? 1;
    e.dropChance = cfg.dropChance ?? 0.08;
    e.isBoss = type === "boss";
    return e;
  }

  update(dt, game) {
    this.pool.sweep((e) => e.dead || e.y > game.h + 60 || e.x < -60 || e.x > game.w + 60);
    for (const e of this.pool.active) e.update(dt, game);
  }
  draw(ctx) { for (const e of this.pool.active) e.draw(ctx); }
  kill(e) { e.dead = true; this.pool.release(e); }
  clear() { this.pool.clear(); }
  get active() { return this.pool.active; }
}

export const ENEMY_CONFIG = {
  grunt:   { r: 14, hp: 1,  score: 100, speed: 90,  color: "#ff6b8a", dropChance: 0.08 },
  zigzag:  { r: 14, hp: 2,  score: 150, speed: 80,  color: "#c46bff", amp: 70, freq: 3, dropChance: 0.12 },
  hunter:  { r: 14, hp: 3,  score: 220, speed: 70,  color: "#ffb24d", dropChance: 0.18 },
  shooter: { r: 16, hp: 4,  score: 280, speed: 110, color: "#ff8ac4", fireRate: 1.6, dropChance: 0.22 },
  boss:    { r: 44, hp: 60, score: 3000, speed: 0,  color: "#ff4d6d", amp: 0, freq: 0.8, fireRate: 1.4, dropChance: 1 },
};

/** 波次生成器：返回该波次的敌人生成任务清单 */
export function buildWave(n, w) {
  const tasks = []; // { t, type, x }
  const cx = w / 2;
  if (n % 5 === 0) {
    // Boss 波次
    tasks.push({ t: 0.5, type: "boss", x: cx });
    // 加点小怪陪衬
    for (let i = 0; i < 4 + n / 5; i++) {
      tasks.push({ t: 2 + i * 0.6, type: pick(["zigzag", "shooter"]), x: rand(80, w - 80) });
    }
    return tasks;
  }
  const base = 6 + n * 2;
  let t = 0.5;
  // 编队：grunt 横排
  for (let i = 0; i < Math.min(8, base); i++) {
    tasks.push({ t: t + i * 0.25, type: "grunt", x: 60 + (i % 8) * ((w - 120) / 7) });
  }
  t += 2.5;
  // 之字队
  if (n >= 2) {
    for (let i = 0; i < 3 + Math.floor(n / 2); i++) {
      tasks.push({ t: t + i * 0.5, type: "zigzag", x: rand(80, w - 80) });
    }
    t += 2;
  }
  // 追踪者
  if (n >= 3) {
    for (let i = 0; i < 2 + Math.floor(n / 3); i++) {
      tasks.push({ t: t + i * 0.8, type: "hunter", x: rand(80, w - 80) });
    }
    t += 2;
  }
  // 射手
  if (n >= 4) {
    for (let i = 0; i < 2 + Math.floor(n / 4); i++) {
      tasks.push({ t: t + i * 0.9, type: "shooter", x: rand(80, w - 80) });
    }
  }
  return tasks;
}

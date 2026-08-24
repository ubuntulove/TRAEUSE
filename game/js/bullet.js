// bullet.js — 子弹/弹幕（玩家与敌人共用，通过 side 区分阵营）
import { Pool } from "./utils.js";

class Bullet {
  constructor() { this.reset(0, 0); }
  reset(x, y) {
    this.x = x; this.y = y;
    this.vx = 0; this.vy = 0;
    this.r = 3;
    this.side = "player"; // "player" | "enemy"
    this.damage = 1;
    this.life = 3;
    this.color = "#5ad1ff";
    this.trail = [];
    this.dead = false;
  }
  update(dt) {
    this.x += this.vx * dt;
    this.y += this.vy * dt;
    this.life -= dt;
    // 拖尾点
    this.trail.push(this.x, this.y);
    if (this.trail.length > 8) this.trail.splice(0, 2);
  }
  draw(ctx) {
    // 拖尾
    if (this.trail.length >= 4) {
      ctx.strokeStyle = this.color;
      ctx.globalAlpha = 0.4;
      ctx.lineWidth = this.r * 1.2;
      ctx.beginPath();
      ctx.moveTo(this.trail[0], this.trail[1]);
      for (let i = 2; i < this.trail.length; i += 2) {
        ctx.lineTo(this.trail[i], this.trail[i + 1]);
      }
      ctx.stroke();
      ctx.globalAlpha = 1;
    }
    // 弹头：外光晕 + 内核
    ctx.fillStyle = this.color;
    ctx.shadowBlur = 12;
    ctx.shadowColor = this.color;
    ctx.beginPath();
    ctx.arc(this.x, this.y, this.r, 0, Math.PI * 2);
    ctx.fill();
    ctx.shadowBlur = 0;
    ctx.fillStyle = "#fff";
    ctx.beginPath();
    ctx.arc(this.x, this.y, this.r * 0.5, 0, Math.PI * 2);
    ctx.fill();
  }
}

export class BulletSystem {
  constructor() { this.pool = new Pool(() => new Bullet()); }

  spawn(opts) {
    const b = this.pool.acquire();
    b.x = opts.x; b.y = opts.y;
    b.vx = opts.vx; b.vy = opts.vy;
    b.r = opts.r ?? 3;
    b.side = opts.side ?? "player";
    b.damage = opts.damage ?? 1;
    b.life = opts.life ?? 3;
    b.color = opts.color ?? (b.side === "player" ? "#5ad1ff" : "#ff6b8a");
    b.trail.length = 0;
    b.dead = false;
  }

  update(dt, bounds) {
    this.pool.sweep((b) => b.life <= 0 ||
      b.y < -20 || b.y > bounds.h + 20 ||
      b.x < -20 || b.x > bounds.w + 20);
    for (const b of this.pool.active) b.update(dt);
  }

  draw(ctx) { for (const b of this.pool.active) b.draw(ctx); }

  /** 命中后标记并回收 */
  kill(b) { b.dead = true; this.pool.release(b); }

  clear() { this.pool.clear(); }
  get active() { return this.pool.active; }
}

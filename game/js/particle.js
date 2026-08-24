// particle.js — 粒子系统（爆炸、推进尾焰、火花）
import { Pool, rand, TAU } from "./utils.js";

class Particle {
  constructor() { this.reset(0, 0); }
  reset(x, y) {
    this.x = x; this.y = y;
    this.vx = 0; this.vy = 0;
    this.life = 0; this.max = 1;
    this.size = 2;
    this.hue = 200; this.sat = 100; this.lum = 60;
    this.drag = 0.92;
    this.gravity = 0;
    this.shape = "circle";
  }
  update(dt) {
    this.x += this.vx * dt;
    this.y += this.vy * dt;
    const dragF = Math.pow(this.drag, dt * 60);
    this.vx *= dragF; this.vy *= dragF;
    this.vy += this.gravity * dt;
    this.life -= dt;
  }
  draw(ctx) {
    const a = Math.max(0, this.life / this.max);
    ctx.globalAlpha = a;
    ctx.fillStyle = `hsl(${this.hue},${this.sat}%,${this.lum}%)`;
    if (this.shape === "spark") {
      ctx.strokeStyle = ctx.fillStyle;
      ctx.lineWidth = this.size * a;
      ctx.beginPath();
      ctx.moveTo(this.x, this.y);
      ctx.lineTo(this.x - this.vx * 0.02, this.y - this.vy * 0.02);
      ctx.stroke();
    } else {
      ctx.beginPath();
      ctx.arc(this.x, this.y, this.size * (0.4 + a * 0.6), 0, TAU);
      ctx.fill();
    }
    ctx.globalAlpha = 1;
  }
}

export class ParticleSystem {
  constructor() { this.pool = new Pool(() => new Particle()); }

  /** 一次性爆炸 */
  burst(x, y, opts = {}) {
    const {
      count = 24, speed = 220, speedVar = 120,
      hue = 30, hueVar = 30, sat = 100, lum = 60,
      life = 0.6, lifeVar = 0.3, size = 3, drag = 0.9,
    } = opts;
    for (let i = 0; i < count; i++) {
      const p = this.pool.acquire();
      const a = rand(0, TAU);
      const sp = speed + rand(-speedVar, speedVar);
      p.x = x; p.y = y;
      p.vx = Math.cos(a) * sp; p.vy = Math.sin(a) * sp;
      p.life = life + rand(-lifeVar, lifeVar);
      p.max = p.life;
      p.size = size * rand(0.6, 1.2);
      p.hue = hue + rand(-hueVar, hueVar);
      p.sat = sat; p.lum = lum;
      p.drag = drag; p.gravity = 0;
      p.shape = Math.random() < 0.4 ? "spark" : "circle";
    }
  }

  /** 飞船尾焰 */
  trail(x, y, vx = 0, vy = 80, hue = 200) {
    const p = this.pool.acquire();
    p.x = x + rand(-1.5, 1.5); p.y = y;
    p.vx = vx + rand(-20, 20); p.vy = vy + rand(20, 80);
    p.life = 0.35; p.max = 0.35;
    p.size = rand(1.5, 3);
    p.hue = hue + rand(-15, 15); p.sat = 100; p.lum = 70;
    p.drag = 0.85; p.gravity = 0;
    p.shape = "circle";
  }

  update(dt) {
    this.pool.sweep((p) => p.life <= 0);
    for (const p of this.pool.active) p.update(dt);
  }

  draw(ctx) {
    ctx.save();
    ctx.globalCompositeOperation = "lighter";
    for (const p of this.pool.active) p.draw(ctx);
    ctx.restore();
  }

  clear() { this.pool.clear(); }
}

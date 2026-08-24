// starfield.js — 多层视差星空背景
import { rand, randInt } from "./utils.js";

export class Starfield {
  constructor(w, h, layers = 3) {
    this.w = w; this.h = h;
    this.layers = [];
    // 每层有不同速度、数量、亮度
    for (let i = 0; i < layers; i++) {
      const stars = [];
      const count = [60, 45, 25][i] || 30;
      const speed = [20, 55, 110][i] || 50;     // 像素/秒
      const size = [0.6, 1.1, 1.8][i] || 1;
      const alpha = [0.35, 0.6, 0.95][i] || 0.5;
      const hue = [225, 215, 200][i] || 220;
      for (let k = 0; k < count; k++) {
        stars.push({
          x: rand(0, w), y: rand(0, h),
          tw: rand(0, TAU), twSpd: rand(1.5, 4),
        });
      }
      this.layers.push({ stars, speed, size, alpha, hue });
    }
    this.shootingStars = [];
    this.spawnTimer = 4;
  }

  resize(w, h) {
    const sx = w / this.w, sy = h / this.h;
    this.w = w; this.h = h;
    for (const layer of this.layers) {
      for (const s of layer.stars) { s.x *= sx; s.y *= sy; }
    }
  }

  update(dt, speedMul = 1) {
    for (const layer of this.layers) {
      for (const s of layer.stars) {
        s.y += layer.speed * dt * speedMul;
        s.tw += layer.twSpd * dt;
        if (s.y > this.h + 4) { s.y = -4; s.x = rand(0, this.w); }
      }
    }
    // 偶尔出现流星
    this.spawnTimer -= dt;
    if (this.spawnTimer <= 0) {
      this.spawnTimer = rand(5, 14);
      this.shootingStars.push({
        x: rand(this.w * 0.1, this.w * 0.9),
        y: rand(-20, this.h * 0.3),
        vx: rand(-180, 180),
        vy: rand(260, 420),
        life: 1.2,
        max: 1.2,
      });
    }
    for (let i = this.shootingStars.length - 1; i >= 0; i--) {
      const s = this.shootingStars[i];
      s.x += s.vx * dt; s.y += s.vy * dt; s.life -= dt;
      if (s.life <= 0 || s.y > this.h + 50) this.shootingStars.splice(i, 1);
    }
  }

  draw(ctx) {
    for (const layer of this.layers) {
      const { stars, size, alpha, hue } = layer;
      for (const s of stars) {
        const tw = 0.6 + 0.4 * Math.sin(s.tw);
        ctx.globalAlpha = alpha * tw;
        ctx.fillStyle = `hsl(${hue}, 80%, 88%)`;
        ctx.beginPath();
        ctx.arc(s.x, s.y, size, 0, TAU);
        ctx.fill();
      }
    }
    // 流星：用渐变拖尾
    for (const s of this.shootingStars) {
      const a = Math.max(0, s.life / s.max);
      const grad = ctx.createLinearGradient(s.x, s.y, s.x - s.vx * 0.08, s.y - s.vy * 0.08);
      grad.addColorStop(0, `rgba(180,220,255,${a})`);
      grad.addColorStop(1, "rgba(180,220,255,0)");
      ctx.strokeStyle = grad;
      ctx.lineWidth = 2;
      ctx.globalAlpha = 1;
      ctx.beginPath();
      ctx.moveTo(s.x, s.y);
      ctx.lineTo(s.x - s.vx * 0.08, s.y - s.vy * 0.08);
      ctx.stroke();
    }
    ctx.globalAlpha = 1;
  }
}

const TAU = Math.PI * 2;

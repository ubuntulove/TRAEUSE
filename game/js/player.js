// player.js — 玩家飞船：移动、射击、护盾、武器升级
import { Input } from "./input.js";
import { clamp, TAU } from "./utils.js";

export class Player {
  constructor(x, y) {
    this.x = x; this.y = y;
    this.r = 14;              // 碰撞半径
    this.speed = 360;         // 像素/秒
    this.hp = 100; this.maxHp = 100;
    this.shield = 0; this.maxShield = 50;
    this.shieldRegenTimer = 0;
    this.shieldRegenDelay = 3;
    this.shieldRegenRate = 8; // 每秒

    // 武器
    this.fireCd = 0;
    this.fireRate = 0.18;     // 秒/发
    this.weaponLevel = 1;     // 1..3
    this.maxWeapon = 3;

    this.iframes = 0;          // 受击无敌帧
    this.thrustVisual = 0;     // 推进视觉抖动
    this.alive = true;
    this.angle = -Math.PI / 2; // 朝上
  }

  takeDamage(amount, game) {
    if (this.iframes > 0) return false;
    // 先扣护盾
    let remain = amount;
    if (this.shield > 0) {
      const absorb = Math.min(this.shield, remain);
      this.shield -= absorb;
      remain -= absorb;
    }
    if (remain > 0) this.hp -= remain;
    this.iframes = 0.8;
    this.shieldRegenTimer = 0;
    if (this.hp <= 0) { this.hp = 0; this.alive = false; }
    return true;
  }

  heal(n) { this.hp = Math.min(this.maxHp, this.hp + n); }
  addShield(n) { this.shield = Math.min(this.maxShield, this.shield + n); }
  upgradeWeapon() { if (this.weaponLevel < this.maxWeapon) this.weaponLevel++; }

  update(dt, game) {
    if (!this.alive) return;

    // —— 移动 ——
    let ax = 0, ay = 0;
    if (Input.isDown("KeyA", "ArrowLeft")) ax -= 1;
    if (Input.isDown("KeyD", "ArrowRight")) ax += 1;
    if (Input.isDown("KeyW", "ArrowUp")) ay -= 1;
    if (Input.isDown("KeyS", "ArrowDown")) ay += 1;
    const len = Math.hypot(ax, ay);
    if (len > 0) { ax /= len; ay /= len; this.thrustVisual = 1; }
    else this.thrustVisual *= 0.85;

    this.x += ax * this.speed * dt;
    this.y += ay * this.speed * dt;
    this.x = clamp(this.x, this.r, game.w - this.r);
    this.y = clamp(this.y, this.r, game.h - this.r);

    // —— 推进尾焰 ——
    if (this.thrustVisual > 0.1 || ay < 0) {
      game.particles.trail(this.x, this.y + this.r, 0, 160, 195);
    }

    // —— 射击 ——
    this.fireCd -= dt;
    const shooting = Input.isDown("Space") || Input.mouse.down;
    if (shooting && this.fireCd <= 0) {
      this.fireCd = this.fireRate;
      this._fire(game);
      game.audio.shoot();
    }

    // —— 计时器 ——
    if (this.iframes > 0) this.iframes -= dt;
    if (this.shield < this.maxShield) {
      this.shieldRegenTimer += dt;
      if (this.shieldRegenTimer >= this.shieldRegenDelay) {
        this.shield = Math.min(this.maxShield, this.shield + this.shieldRegenRate * dt);
      }
    }
  }

  _fire(game) {
    const bx = this.x, by = this.y - this.r;
    const speed = 720;
    const mk = (x, y, vx, vy) => game.bullets.spawn({
      x, y, vx, vy, r: 3, side: "player", damage: 1, color: "#5ad1ff", life: 2,
    });
    switch (this.weaponLevel) {
      case 1:
        mk(bx, by, 0, -speed);
        break;
      case 2:
        mk(bx - 7, by, 0, -speed);
        mk(bx + 7, by, 0, -speed);
        break;
      case 3:
        mk(bx, by, 0, -speed);
        mk(bx - 9, by + 4, -120, -speed * 0.97);
        mk(bx + 9, by + 4, 120, -speed * 0.97);
        break;
    }
  }

  draw(ctx) {
    if (!this.alive) return;
    const blink = this.iframes > 0 && Math.floor(this.iframes * 20) % 2 === 0;
    if (blink) { ctx.globalAlpha = 0.35; }

    ctx.save();
    ctx.translate(this.x, this.y);
    ctx.rotate(Math.PI / 2); // 让飞船尖头朝上

    // 护盾
    if (this.shield > 0) {
      ctx.strokeStyle = `rgba(90,160,255,${0.3 + 0.4 * (this.shield / this.maxShield)})`;
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(0, 0, this.r + 8 + Math.sin(performance.now() / 120) * 1.5, 0, TAU);
      ctx.stroke();
    }

    // 飞船主体：青色三角战机
    ctx.shadowBlur = 14;
    ctx.shadowColor = "#5ad1ff";
    ctx.fillStyle = "#0a2a4a";
    ctx.strokeStyle = "#7ad6ff";
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(0, -this.r * 1.4);
    ctx.lineTo(this.r, this.r);
    ctx.lineTo(0, this.r * 0.5);
    ctx.lineTo(-this.r, this.r);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    // 驾驶舱
    ctx.shadowBlur = 0;
    ctx.fillStyle = "#bfe9ff";
    ctx.beginPath();
    ctx.arc(0, -this.r * 0.2, this.r * 0.35, 0, TAU);
    ctx.fill();

    // 引擎光
    if (this.thrustVisual > 0.15) {
      ctx.fillStyle = `rgba(120,200,255,${this.thrustVisual})`;
      ctx.beginPath();
      ctx.moveTo(-4, this.r);
      ctx.lineTo(0, this.r + 6 + this.thrustVisual * 8);
      ctx.lineTo(4, this.r);
      ctx.closePath();
      ctx.fill();
    }
    ctx.restore();
    ctx.globalAlpha = 1;
  }
}

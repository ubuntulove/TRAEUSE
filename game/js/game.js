// game.js — 游戏核心：状态机、主循环、碰撞、波次、道具、屏幕震动
import { Input } from "./input.js";
import { Audio } from "./audio.js";
import { Starfield } from "./starfield.js";
import { ParticleSystem } from "./particle.js";
import { BulletSystem } from "./bullet.js";
import { EnemySystem, buildWave, ENEMY_CONFIG } from "./enemy.js";
import { Player } from "./player.js";
import { UI } from "./ui.js";
import { rand, pick, hitCircle } from "./utils.js";

const STATE = { MENU: "menu", PLAYING: "playing", PAUSED: "paused", GAMEOVER: "gameover" };
const HI_KEY = "stellar_defense_hiscore";

const POWERUP_TYPES = {
  heal:   { color: "#3aff7a", label: "+25 HP" },
  shield: { color: "#5ad1ff", label: "护盾充能" },
  weapon: { color: "#ffd24d", label: "武器升级!" },
};

export class Game {
  constructor(canvas) {
    this.canvas = canvas;
    this.ctx = canvas.getContext("2d");
    this.state = STATE.MENU;
    this.ui = new UI();
    this.starfield = new Starfield(1, 1);
    this.particles = new ParticleSystem();
    this.bullets = new BulletSystem();
    this.enemies = new EnemySystem();
    this.player = new Player(0, 0);
    this.audio = Audio;
    this.powerups = []; // {x,y,vy,type,r,color,life}
    this.score = 0;
    this.hi = Number(localStorage.getItem(HI_KEY) || 0);
    this.wave = 0;
    this.waveTasks = [];
    this.waveTime = 0;
    this.waveActive = false;
    this.waveCleared = false;
    this.shake = 0;
    this.flash = 0;          // 红色受伤闪屏
    this.time = 0;
    this._last = 0;
    this._raf = null;

    this.resize();
    window.addEventListener("resize", () => this.resize());
    this.ui.setMenuHi(this.hi);
    this._loop = this._loop.bind(this);
  }

  resize() {
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    const w = window.innerWidth, h = window.innerHeight;
    this.canvas.width = w * dpr;
    this.canvas.height = h * dpr;
    this.canvas.style.width = w + "px";
    this.canvas.style.height = h + "px";
    this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    this.w = w; this.h = h;
    this.starfield.resize(w, h);
    if (this.player) {
      this.player.x = Math.min(this.player.x || w / 2, w - this.player.r);
      this.player.y = this.player.y || h - 80;
    }
  }

  // ===== 状态控制 =====
  start() {
    this.audio.init();
    this.audio.uiClick();
    this.score = 0; this.wave = 0;
    this.particles.clear(); this.bullets.clear(); this.enemies.clear();
    this.powerups = [];
    this.player = new Player(this.w / 2, this.h - 80);
    this.state = STATE.PLAYING;
    this.ui.hideOverlays();
    this._nextWave();
  }

  togglePause() {
    if (this.state === STATE.PLAYING) {
      this.state = STATE.PAUSED;
      this.ui.showOverlay("pause");
      this.audio.uiClick();
    } else if (this.state === STATE.PAUSED) {
      this.state = STATE.PLAYING;
      this.ui.hideOverlays();
      this.audio.uiClick();
    }
  }

  quitToMenu() {
    this.state = STATE.MENU;
    this.ui.setMenuHi(this.hi);
    this.ui.showOverlay("menu");
    this.audio.uiClick();
  }

  gameOver() {
    this.state = STATE.GAMEOVER;
    this.audio.playerDie();
    this.shake = 1; this.flash = 1;
    // 大爆炸
    this.particles.burst(this.player.x, this.player.y, {
      count: 60, speed: 320, speedVar: 200,
      hue: 20, hueVar: 40, life: 1.2, lifeVar: 0.5, size: 4, drag: 0.92,
    });
    this.particles.burst(this.player.x, this.player.y, {
      count: 30, speed: 180, speedVar: 100,
      hue: 200, hueVar: 30, life: 0.8, lifeVar: 0.3, size: 3,
    });
    if (this.score > this.hi) {
      this.hi = this.score;
      localStorage.setItem(HI_KEY, String(this.hi));
    }
    setTimeout(() => this.ui.showGameOver(this.score, this.hi, this.wave), 600);
  }

  // ===== 波次 =====
  _nextWave() {
    this.wave++;
    this.waveTasks = buildWave(this.wave, this.w);
    this.waveTime = 0;
    this.waveActive = true;
    this.waveCleared = false;
    this.audio.waveStart();
    this.ui.showHint(`第 ${this.wave} 波${this.wave % 5 === 0 ? " · BOSS" : ""}`);
  }

  _updateWave(dt) {
    if (!this.waveActive) return;
    this.waveTime += dt;
    while (this.waveTasks.length && this.waveTasks[0].t <= this.waveTime) {
      const t = this.waveTasks.shift();
      this.enemies.spawn(t.type, { x: t.x });
    }
    // 全部任务完成且场上无敌人 → 进入下一波
    if (this.waveTasks.length === 0 && this.enemies.active.length === 0 && !this.waveCleared) {
      this.waveCleared = true;
      this.waveActive = false;
      // 短暂停顿后开始下一波
      setTimeout(() => {
        if (this.state === STATE.PLAYING) {
          // 波次结束奖励：偶尔给个道具
          if (Math.random() < 0.6) this._spawnPowerup(rand(80, this.w - 80), -20);
          this._nextWave();
        }
      }, 1400);
    }
  }

  // ===== 道具 =====
  _spawnPowerup(x, y) {
    const type = pick(["heal", "shield", "weapon"]);
    this.powerups.push({ x, y, vy: 90, type, r: 10, color: POWERUP_TYPES[type].color, life: 12, pulse: 0 });
  }

  _updatePowerups(dt) {
    for (let i = this.powerups.length - 1; i >= 0; i--) {
      const p = this.powerups[i];
      p.y += p.vy * dt;
      p.pulse += dt * 4;
      p.life -= dt;
      if (p.y > this.h + 30 || p.life <= 0) { this.powerups.splice(i, 1); continue; }
      // 拾取
      if (this.player.alive && hitCircle(p, this.player)) {
        this._applyPowerup(p.type);
        this.audio.pickup();
        this.particles.burst(p.x, p.y, { count: 12, speed: 120, hue: 90, hueVar: 30, life: 0.4, size: 2.5 });
        this.powerups.splice(i, 1);
      }
    }
  }

  _applyPowerup(type) {
    switch (type) {
      case "heal":
        this.player.heal(25);
        this.ui.showHint(POWERUP_TYPES.heal.label);
        break;
      case "shield":
        this.player.addShield(this.player.maxShield);
        this.audio.shieldUp();
        this.ui.showHint(POWERUP_TYPES.shield.label);
        break;
      case "weapon":
        if (this.player.weaponLevel >= this.player.maxWeapon) {
          this.score += 500;
          this.ui.showHint("武器已满 +500");
        } else {
          this.player.upgradeWeapon();
          this.ui.showHint(POWERUP_TYPES.weapon.label);
        }
        break;
    }
  }

  _drawPowerups(ctx) {
    for (const p of this.powerups) {
      const blink = p.life < 3 && Math.floor(p.life * 8) % 2 === 0;
      if (blink) continue;
      const s = 1 + 0.15 * Math.sin(p.pulse);
      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.scale(s, s);
      ctx.shadowBlur = 16; ctx.shadowColor = p.color;
      ctx.fillStyle = "rgba(10,20,40,0.85)";
      ctx.strokeStyle = p.color; ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(0, 0, p.r, 0, Math.PI * 2);
      ctx.fill(); ctx.stroke();
      ctx.shadowBlur = 0;
      ctx.fillStyle = p.color;
      ctx.font = "bold 11px monospace";
      ctx.textAlign = "center"; ctx.textBaseline = "middle";
      const sym = { heal: "+", shield: "S", weapon: "W" };
      ctx.fillText(sym[p.type], 0, 1);
      ctx.restore();
    }
  }

  // ===== 碰撞 =====
  _handleCollisions() {
    // 玩家子弹 vs 敌人
    for (const b of this.bullets.active) {
      if (b.side !== "player" || b.dead) continue;
      for (const e of this.enemies.active) {
        if (e.dead) continue;
        if (hitCircle(b, e)) {
          e.hp -= b.damage;
          this.bullets.kill(b);
          this.particles.burst(b.x, b.y, { count: 6, speed: 140, hue: 200, hueVar: 30, life: 0.25, size: 2 });
          if (e.hp <= 0) {
            this._killEnemy(e);
          } else {
            this.audio.hit();
          }
          break;
        }
      }
    }
    if (!this.player.alive) return;
    // 敌人子弹 vs 玩家
    for (const b of this.bullets.active) {
      if (b.side !== "enemy" || b.dead) continue;
      if (hitCircle(b, this.player)) {
        this.bullets.kill(b);
        if (this.player.takeDamage(b.damage, this)) {
          this.audio.hit();
          this.shake = Math.max(this.shake, 0.4);
          this.flash = 0.5;
          this.particles.burst(b.x, b.y, { count: 10, speed: 160, hue: 0, hueVar: 20, life: 0.3, size: 2.5 });
          if (!this.player.alive) this.gameOver();
        }
      }
    }
    // 敌人撞击玩家
    for (const e of this.enemies.active) {
      if (e.dead) continue;
      if (hitCircle(e, this.player)) {
        if (this.player.takeDamage(20, this)) {
          this.audio.explode();
          this.shake = 0.7;
          this.flash = 0.6;
          // 撞击的敌人也受损
          e.hp -= 3;
          if (e.hp <= 0) this._killEnemy(e);
          if (!this.player.alive) { this.gameOver(); return; }
        }
      }
    }
  }

  _killEnemy(e) {
    this.score += e.score;
    this.enemies.kill(e);
    const cfg = ENEMY_CONFIG[e.type];
    this.audio.explode();
    // 爆炸粒子
    this.particles.burst(e.x, e.y, {
      count: e.isBoss ? 80 : 18,
      speed: e.isBoss ? 380 : 220,
      speedVar: e.isBoss ? 220 : 100,
      hue: e.isBoss ? 350 : 20, hueVar: 30,
      life: e.isBoss ? 1.4 : 0.6, lifeVar: e.isBoss ? 0.6 : 0.25,
      size: e.isBoss ? 5 : 3, drag: 0.9,
    });
    this.particles.burst(e.x, e.y, {
      count: e.isBoss ? 40 : 8, speed: 120, hue: 200, hueVar: 20, life: 0.5, size: 2,
    });
    if (e.isBoss) {
      this.shake = 1;
      this.audio.boom();
      this.ui.showHint("BOSS 已被击破!");
    }
    // 掉落道具
    if (Math.random() < e.dropChance) this._spawnPowerup(e.x, e.y);
  }

  // ===== 主循环 =====
  startLoop() {
    if (this._raf) return;
    this._last = performance.now();
    this._raf = requestAnimationFrame(this._loop);
  }
  stopLoop() {
    if (this._raf) cancelAnimationFrame(this._raf);
    this._raf = null;
  }

  _loop(now) {
    this._raf = requestAnimationFrame(this._loop);
    let dt = (now - this._last) / 1000;
    this._last = now;
    if (dt > 0.05) dt = 0.05; // 防止卡顿后大跳

    this.update(dt);
    this.draw();
    Input.endFrame();
  }

  update(dt) {
    this.time += dt;
    // 暂停键全局可用
    if (Input.wasPressed("KeyP")) {
      if (this.state === STATE.PLAYING || this.state === STATE.PAUSED) this.togglePause();
    }
    if (Input.wasPressed("KeyM")) {
      const m = this.audio.toggleMute();
      this.ui.showHint(m ? "静音" : "取消静音");
    }

    // 星空永远在动
    const speedMul = this.state === STATE.PLAYING ? 1 + Math.min(this.wave, 10) * 0.15 : 0.6;
    this.starfield.update(dt, speedMul);

    if (this.state !== STATE.PLAYING) {
      this.ui.updateHint(dt);
      // 菜单/结束界面也持续画粒子作为氛围
      if (this.shake > 0) this.shake = Math.max(0, this.shake - dt * 2);
      if (this.flash > 0) this.flash = Math.max(0, this.flash - dt * 2);
      this.particles.update(dt);
      return;
    }

    this.player.update(dt, this);
    this.enemies.update(dt, this);
    this.bullets.update(dt, this);
    this.particles.update(dt);
    this._updatePowerups(dt);
    this._updateWave(dt);
    this._handleCollisions();

    if (this.shake > 0) this.shake = Math.max(0, this.shake - dt * 2.5);
    if (this.flash > 0) this.flash = Math.max(0, this.flash - dt * 2.5);

    this.ui.updateHUD(this);
    this.ui.updateHint(dt);
  }

  draw() {
    const ctx = this.ctx;
    ctx.clearRect(0, 0, this.w, this.h);

    ctx.save();
    // 屏幕震动
    if (this.shake > 0) {
      const s = this.shake * 14;
      ctx.translate(rand(-s, s), rand(-s, s));
    }

    // 星空
    this.starfield.draw(ctx);

    // 游戏元素
    this.particles.draw(ctx);
    this._drawPowerups(ctx);
    this.enemies.draw(ctx);
    this.bullets.draw(ctx);
    if (this.state !== STATE.MENU) this.player.draw(ctx);

    ctx.restore();

    // 受伤红闪（覆盖整屏）
    if (this.flash > 0) {
      ctx.fillStyle = `rgba(255,40,60,${this.flash * 0.35})`;
      ctx.fillRect(0, 0, this.w, this.h);
    }

    // 暂停遮罩文字
    if (this.state === STATE.PAUSED) {
      ctx.fillStyle = "rgba(5,8,20,0.4)";
      ctx.fillRect(0, 0, this.w, this.h);
    }
  }
}

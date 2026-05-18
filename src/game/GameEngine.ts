import { Player } from './Player';
import { GAME_CONFIG, MECH_CONFIG, COLORS } from '../constants/gameConfig';
import { Particle, DamageNumber } from '../types/game';

export class GameEngine {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private player1: Player;
  private player2: Player;
  private pressedKeys: Set<string> = new Set();
  private particles: Particle[] = [];
  private damageNumbers: DamageNumber[] = [];
  private lastTime: number = 0;
  private animationId: number = 0;
  private gameStatus: 'menu' | 'playing' | 'paused' | 'gameover' = 'menu';
  private winner: 1 | 2 | null = null;
  private backgroundOffset: number = 0;
  private onStateChange: (status: 'menu' | 'playing' | 'paused' | 'gameover', winner: 1 | 2 | null) => void;

  constructor(
    canvas: HTMLCanvasElement,
    onStateChange: (status: 'menu' | 'playing' | 'paused' | 'gameover', winner: 1 | 2 | null) => void
  ) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
    this.ctx.imageSmoothingEnabled = false;
    this.onStateChange = onStateChange;
    this.player1 = new Player(1, 150);
    this.player2 = new Player(2, 600);
    this.setupInputHandlers();
  }

  private setupInputHandlers(): void {
    window.addEventListener('keydown', (e) => {
      this.pressedKeys.add(e.code);
      e.preventDefault();
    });

    window.addEventListener('keyup', (e) => {
      this.pressedKeys.delete(e.code);
      e.preventDefault();
    });
  }

  start(): void {
    this.gameStatus = 'playing';
    this.onStateChange('playing', null);
    this.lastTime = performance.now();
    this.gameLoop();
  }

  pause(): void {
    this.gameStatus = 'paused';
    this.onStateChange('paused', null);
    cancelAnimationFrame(this.animationId);
  }

  resume(): void {
    this.gameStatus = 'playing';
    this.onStateChange('playing', null);
    this.lastTime = performance.now();
    this.gameLoop();
  }

  reset(): void {
    this.player1.reset(150);
    this.player2.reset(600);
    this.particles = [];
    this.damageNumbers = [];
    this.winner = null;
    this.gameStatus = 'playing';
    this.onStateChange('playing', null);
    this.lastTime = performance.now();
    this.gameLoop();
  }

  getStatus(): { status: 'menu' | 'playing' | 'paused' | 'gameover'; winner: 1 | 2 | null } {
    return { status: this.gameStatus, winner: this.winner };
  }

  private gameLoop = (): void => {
    if (this.gameStatus !== 'playing') return;

    const currentTime = performance.now();
    const deltaTime = currentTime - this.lastTime;
    this.lastTime = currentTime;

    this.update(deltaTime);
    this.render();

    this.animationId = requestAnimationFrame(this.gameLoop);
  };

  private update(deltaTime: number): void {
    this.player1.update(deltaTime, this.pressedKeys, this.player2);
    this.player2.update(deltaTime, this.pressedKeys, this.player1);

    this.checkAttacks();
    this.checkWinCondition();
    this.updateParticles(deltaTime);
    this.updateDamageNumbers(deltaTime);
    this.backgroundOffset += 0.5;
  }

  private checkAttacks(): void {
    const p1Attack = this.player1.getAttackHitbox();
    const p2Attack = this.player2.getAttackHitbox();

    if (p1Attack) {
      const p2Hitbox = this.player2.getHitbox();
      if (this.checkCollision(p1Attack, p2Hitbox)) {
        const damage = this.calculateDamage(this.player1.stats.attackDamage, this.player1.state.isSpecialAttacking);
        const actualDamage = this.player2.takeDamage(damage);
        if (actualDamage > 0) {
          this.spawnDamageNumber(this.player2.state.x + 20, this.player2.state.y - 50, actualDamage);
          this.spawnHitParticles(this.player2.state.x + 20, this.player2.state.y - 30, MECH_CONFIG.P1.color);
        }
      }
    }

    if (p2Attack) {
      const p1Hitbox = this.player1.getHitbox();
      if (this.checkCollision(p2Attack, p1Hitbox)) {
        const damage = this.calculateDamage(this.player2.stats.attackDamage, this.player2.state.isSpecialAttacking);
        const actualDamage = this.player1.takeDamage(damage);
        if (actualDamage > 0) {
          this.spawnDamageNumber(this.player1.state.x + 20, this.player1.state.y - 50, actualDamage);
          this.spawnHitParticles(this.player1.state.x + 20, this.player1.state.y - 30, MECH_CONFIG.P2.color);
        }
      }
    }
  }

  private checkCollision(a: { x: number; y: number; width: number; height: number }, b: { x: number; y: number; width: number; height: number }): boolean {
    return a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y;
  }

  private calculateDamage(damageRange: [number, number], isSpecial: boolean): number {
    const baseDamage = Math.floor(Math.random() * (damageRange[1] - damageRange[0] + 1)) + damageRange[0];
    return isSpecial ? Math.floor(baseDamage * 2.5) : baseDamage;
  }

  private spawnDamageNumber(x: number, y: number, damage: number): void {
    this.damageNumbers.push({
      x,
      y,
      damage,
      life: 1000,
    });
  }

  private spawnHitParticles(x: number, y: number, color: string): void {
    for (let i = 0; i < 8; i++) {
      this.particles.push({
        x,
        y,
        vx: (Math.random() - 0.5) * 6,
        vy: (Math.random() - 0.5) * 6,
        life: 300,
        maxLife: 300,
        color,
        size: Math.random() * 4 + 2,
      });
    }
  }

  private updateParticles(deltaTime: number): void {
    this.particles = this.particles.filter((p) => {
      p.x += p.vx;
      p.y += p.vy;
      p.vy += 0.2;
      p.life -= deltaTime;
      return p.life > 0;
    });
  }

  private updateDamageNumbers(deltaTime: number): void {
    this.damageNumbers = this.damageNumbers.filter((d) => {
      d.y -= 1;
      d.life -= deltaTime;
      return d.life > 0;
    });
  }

  private checkWinCondition(): void {
    if (this.player1.state.hp <= 0) {
      this.winner = 2;
      this.gameStatus = 'gameover';
      this.player2.state.currentAnimation = 'victory';
      this.onStateChange('gameover', 2);
      cancelAnimationFrame(this.animationId);
    } else if (this.player2.state.hp <= 0) {
      this.winner = 1;
      this.gameStatus = 'gameover';
      this.player1.state.currentAnimation = 'victory';
      this.onStateChange('gameover', 1);
      cancelAnimationFrame(this.animationId);
    }
  }

  private render(): void {
    this.drawBackground();
    this.drawGround();
    this.drawDecorations();
    this.drawMech(this.player1);
    this.drawMech(this.player2);
    this.drawParticles();
    this.drawDamageNumbers();
    this.drawAttackEffects();
  }

  private drawBackground(): void {
    const ctx = this.ctx;
    const gradient = ctx.createLinearGradient(0, 0, 0, GAME_CONFIG.CANVAS_HEIGHT);
    gradient.addColorStop(0, '#0f0f23');
    gradient.addColorStop(1, '#1a1a2e');
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, GAME_CONFIG.CANVAS_WIDTH, GAME_CONFIG.CANVAS_HEIGHT);

    for (let i = 0; i < 5; i++) {
      const x = (this.backgroundOffset * 0.2 + i * 200) % (GAME_CONFIG.CANVAS_WIDTH + 100) - 50;
      ctx.fillStyle = 'rgba(74, 74, 74, 0.3)';
      ctx.fillRect(x, 100, 60, 200);
      ctx.fillRect(x + 10, 80, 40, 20);
    }

    for (let i = 0; i < 20; i++) {
      const x = (i * 50 + this.backgroundOffset * 0.1) % GAME_CONFIG.CANVAS_WIDTH;
      ctx.fillStyle = 'rgba(255, 217, 61, 0.1)';
      ctx.fillRect(x, 50 + Math.sin(i) * 20, 2, 2);
    }
  }

  private drawGround(): void {
    const ctx = this.ctx;
    ctx.fillStyle = COLORS.GROUND;
    ctx.fillRect(0, GAME_CONFIG.GROUND_Y + 5, GAME_CONFIG.CANVAS_WIDTH, 100);

    ctx.fillStyle = COLORS.GROUND_LIGHT;
    for (let x = 0; x < GAME_CONFIG.CANVAS_WIDTH; x += 40) {
      ctx.fillRect(x, GAME_CONFIG.GROUND_Y + 5, 35, 8);
    }

    ctx.fillStyle = '#3a3a3a';
    ctx.fillRect(0, GAME_CONFIG.GROUND_Y + 3, GAME_CONFIG.CANVAS_WIDTH, 2);
  }

  private drawDecorations(): void {
    const ctx = this.ctx;
    
    ctx.fillStyle = 'rgba(255, 107, 53, 0.1)';
    ctx.beginPath();
    ctx.arc(100, 80, 30, 0, Math.PI * 2);
    ctx.fill();

    ctx.fillStyle = 'rgba(78, 205, 196, 0.1)';
    ctx.beginPath();
    ctx.arc(700, 60, 25, 0, Math.PI * 2);
    ctx.fill();
  }

  private drawMech(player: Player): void {
    const ctx = this.ctx;
    const state = player.state;
    const stats = player.stats;
    const scale = GAME_CONFIG.PIXEL_SCALE;

    ctx.save();
    
    if (state.invincible && Math.floor(Date.now() / 100) % 2 === 0) {
      ctx.globalAlpha = 0.5;
    }

    const centerX = state.x + 20;
    const baseY = state.y;

    if (!state.facingRight) {
      ctx.translate(centerX * 2, 0);
      ctx.scale(-1, 1);
    }

    ctx.fillStyle = stats.color;
    ctx.fillRect((state.x) * scale / scale, (baseY - 48) * scale / scale, 40, 48);

    ctx.fillStyle = stats.secondaryColor;
    ctx.fillRect((state.x + 8) * scale / scale, (baseY - 44) * scale / scale, 24, 8);

    ctx.fillStyle = stats.accentColor;
    ctx.fillRect((state.x + 10) * scale / scale, (baseY - 42) * scale / scale, 6, 4);
    ctx.fillRect((state.x + 24) * scale / scale, (baseY - 42) * scale / scale, 6, 4);

    ctx.fillStyle = '#222';
    ctx.fillRect((state.x + 12) * scale / scale, (baseY - 52) * scale / scale, 16, 16);
    ctx.fillStyle = state.facingRight ? stats.accentColor : stats.color;
    ctx.fillRect((state.x + 14) * scale / scale, (baseY - 50) * scale / scale, 12, 12);

    ctx.fillStyle = stats.color;
    ctx.fillRect((state.x - 4) * scale / scale, (baseY - 40) * scale / scale, 8, 32);
    ctx.fillRect((state.x + 36) * scale / scale, (baseY - 40) * scale / scale, 8, 32);

    ctx.fillStyle = stats.accentColor;
    ctx.fillRect((state.x - 6) * scale / scale, (baseY - 42) * scale / scale, 4, 8);
    ctx.fillRect((state.x + 42) * scale / scale, (baseY - 42) * scale / scale, 4, 8);

    if (state.currentAnimation === 'attack' || state.currentAnimation === 'special') {
      ctx.fillStyle = stats.accentColor;
      const attackExtend = state.currentAnimation === 'special' ? 25 : 15;
      ctx.fillRect((state.x + 40) * scale / scale, (baseY - 35) * scale / scale, attackExtend, 8);
      ctx.fillRect((state.x + 45 + (state.currentAnimation === 'special' ? 10 : 0)) * scale / scale, (baseY - 38) * scale / scale, 8, 14);
    }

    if (state.currentAnimation === 'defend') {
      ctx.fillStyle = `${stats.color}88`;
      ctx.fillRect((state.x - 10) * scale / scale, (baseY - 55) * scale / scale, 60, 58);
      
      ctx.strokeStyle = stats.accentColor;
      ctx.lineWidth = 2;
      ctx.strokeRect((state.x - 8) * scale / scale, (baseY - 53) * scale / scale, 56, 54);
    }

    ctx.fillStyle = stats.color;
    ctx.fillRect((state.x + 8) * scale / scale, (baseY - 10) * scale / scale, 10, 12);
    ctx.fillRect((state.x + 22) * scale / scale, (baseY - 10) * scale / scale, 10, 12);

    ctx.fillStyle = stats.secondaryColor;
    ctx.fillRect((state.x + 10) * scale / scale, (baseY - 8) * scale / scale, 6, 10);
    ctx.fillRect((state.x + 24) * scale / scale, (baseY - 8) * scale / scale, 6, 10);

    ctx.restore();
  }

  private drawParticles(): void {
    const ctx = this.ctx;
    this.particles.forEach((p) => {
      const alpha = p.life / p.maxLife;
      ctx.fillStyle = p.color;
      ctx.globalAlpha = alpha;
      ctx.fillRect(p.x - p.size / 2, p.y - p.size / 2, p.size, p.size);
    });
    ctx.globalAlpha = 1;
  }

  private drawDamageNumbers(): void {
    const ctx = this.ctx;
    this.damageNumbers.forEach((d) => {
      const alpha = d.life / 1000;
      ctx.font = 'bold 20px monospace';
      ctx.fillStyle = `rgba(255, 107, 107, ${alpha})`;
      ctx.strokeStyle = `rgba(0, 0, 0, ${alpha})`;
      ctx.lineWidth = 3;
      ctx.strokeText(`-${d.damage}`, d.x, d.y);
      ctx.fillText(`-${d.damage}`, d.x, d.y);
    });
  }

  private drawAttackEffects(): void {
    const ctx = this.ctx;
    
    if (this.player1.state.isAttacking || this.player1.state.isSpecialAttacking) {
      this.drawSlashEffect(this.player1, this.player1.state.isSpecialAttacking);
    }
    if (this.player2.state.isAttacking || this.player2.state.isSpecialAttacking) {
      this.drawSlashEffect(this.player2, this.player2.state.isSpecialAttacking);
    }
  }

  private drawSlashEffect(player: Player, isSpecial: boolean): void {
    const ctx = this.ctx;
    const state = player.state;
    const stats = player.stats;
    
    const slashX = state.facingRight ? state.x + 50 : state.x - 40;
    const slashY = state.y - 30;
    
    ctx.save();
    ctx.strokeStyle = isSpecial ? stats.accentColor : stats.color;
    ctx.lineWidth = isSpecial ? 4 : 2;
    ctx.globalAlpha = 0.8;
    
    const time = Date.now() % 200;
    const angle = state.facingRight ? -0.5 + (time / 200) * Math.PI : 0.5 - (time / 200) * Math.PI;
    
    ctx.beginPath();
    ctx.moveTo(slashX, slashY);
    ctx.lineTo(slashX + Math.cos(angle) * (isSpecial ? 50 : 30), slashY + Math.sin(angle) * 20);
    ctx.stroke();
    
    ctx.restore();
  }

  destroy(): void {
    cancelAnimationFrame(this.animationId);
    window.removeEventListener('keydown', () => {});
    window.removeEventListener('keyup', () => {});
  }
}

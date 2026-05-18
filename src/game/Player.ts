import { PlayerState, MechStats, AnimationState, ControlScheme } from '../types/game';
import { GAME_CONFIG, MECH_CONFIG, CONTROLS } from '../constants/gameConfig';

export class Player {
  id: 1 | 2;
  state: PlayerState;
  controls: ControlScheme;
  stats: MechStats;
  private animationFrame: number = 0;
  private animationTimer: number = 0;

  constructor(id: 1 | 2, x: number) {
    this.id = id;
    this.stats = id === 1 ? MECH_CONFIG.P1 : MECH_CONFIG.P2;
    this.controls = id === 1 ? CONTROLS.P1 : CONTROLS.P2;
    this.state = this.createInitialState(x);
  }

  private createInitialState(x: number): PlayerState {
    return {
      x,
      y: GAME_CONFIG.GROUND_Y,
      velocityX: 0,
      velocityY: 0,
      hp: this.stats.maxHp,
      maxHp: this.stats.maxHp,
      energy: this.stats.maxEnergy,
      maxEnergy: this.stats.maxEnergy,
      isJumping: false,
      isAttacking: false,
      isDefending: false,
      isSpecialAttacking: false,
      facingRight: this.id === 1,
      currentAnimation: 'idle',
      attackCooldown: 0,
      specialCooldown: 0,
      invincible: false,
      invincibleTimer: 0,
    };
  }

  update(deltaTime: number, pressedKeys: Set<string>, opponent: Player): void {
    if (this.state.hp <= 0) {
      this.state.currentAnimation = 'defeat';
      return;
    }

    this.updateTimers(deltaTime);
    this.handleInput(pressedKeys);
    this.applyPhysics();
    this.updateAnimation(deltaTime);
    this.updateFacingDirection(opponent);
  }

  private updateTimers(deltaTime: number): void {
    if (this.state.attackCooldown > 0) {
      this.state.attackCooldown -= deltaTime;
    }
    if (this.state.specialCooldown > 0) {
      this.state.specialCooldown -= deltaTime;
    }
    if (this.state.invincibleTimer > 0) {
      this.state.invincibleTimer -= deltaTime;
      if (this.state.invincibleTimer <= 0) {
        this.state.invincible = false;
      }
    }
    if (this.state.energy < this.state.maxEnergy) {
      this.state.energy = Math.min(
        this.state.maxEnergy,
        this.state.energy + (10 / 5000) * (deltaTime / 16)
      );
    }
  }

  private handleInput(pressedKeys: Set<string>): void {
    if (this.state.isAttacking || this.state.isSpecialAttacking || this.state.isDefending) {
      return;
    }

    if (pressedKeys.has(this.controls.left)) {
      this.state.velocityX = -GAME_CONFIG.MOVE_SPEED;
    } else if (pressedKeys.has(this.controls.right)) {
      this.state.velocityX = GAME_CONFIG.MOVE_SPEED;
    } else {
      this.state.velocityX = 0;
    }

    if (pressedKeys.has(this.controls.jump) && !this.state.isJumping) {
      this.state.velocityY = GAME_CONFIG.JUMP_FORCE;
      this.state.isJumping = true;
    }

    if (pressedKeys.has(this.controls.attack) && this.state.attackCooldown <= 0) {
      this.performAttack();
    }

    if (pressedKeys.has(this.controls.special) && this.state.specialCooldown <= 0 && this.state.energy >= 30) {
      this.performSpecialAttack();
    }

    if (pressedKeys.has(this.controls.defend)) {
      this.state.isDefending = true;
    } else {
      this.state.isDefending = false;
    }
  }

  private applyPhysics(): void {
    this.state.x += this.state.velocityX;
    this.state.y += this.state.velocityY;

    if (this.state.y < GAME_CONFIG.GROUND_Y) {
      this.state.velocityY += GAME_CONFIG.GRAVITY;
    } else {
      this.state.y = GAME_CONFIG.GROUND_Y;
      this.state.velocityY = 0;
      this.state.isJumping = false;
    }

    this.state.x = Math.max(20, Math.min(GAME_CONFIG.CANVAS_WIDTH - 60, this.state.x));
  }

  private updateAnimation(deltaTime: number): void {
    if (this.state.isAttacking) {
      this.state.currentAnimation = 'attack';
      return;
    }
    if (this.state.isSpecialAttacking) {
      this.state.currentAnimation = 'special';
      return;
    }
    if (this.state.isDefending) {
      this.state.currentAnimation = 'defend';
      return;
    }
    if (this.state.invincible) {
      this.state.currentAnimation = 'hurt';
      return;
    }
    if (this.state.isJumping) {
      this.state.currentAnimation = 'jump';
      return;
    }
    if (this.state.velocityX !== 0) {
      this.state.currentAnimation = 'walk';
    } else {
      this.state.currentAnimation = 'idle';
    }
  }

  private updateFacingDirection(opponent: Player): void {
    if (this.state.velocityX === 0 && !this.state.isAttacking && !this.state.isSpecialAttacking) {
      this.state.facingRight = opponent.state.x > this.state.x;
    }
  }

  private performAttack(): void {
    this.state.isAttacking = true;
    this.state.attackCooldown = 500;
    this.state.currentAnimation = 'attack';
    
    setTimeout(() => {
      this.state.isAttacking = false;
    }, 200);
  }

  private performSpecialAttack(): void {
    this.state.isSpecialAttacking = true;
    this.state.specialCooldown = 1500;
    this.state.energy -= 30;
    this.state.currentAnimation = 'special';
    
    setTimeout(() => {
      this.state.isSpecialAttacking = false;
    }, 400);
  }

  takeDamage(amount: number): number {
    if (this.state.invincible) return 0;

    let damage = amount;
    if (this.state.isDefending) {
      damage = Math.floor(damage * 0.5);
      this.state.energy = Math.min(this.state.maxEnergy, this.state.energy + 5);
    }
    damage = Math.floor(damage * (1 - this.stats.defense));

    this.state.hp = Math.max(0, this.state.hp - damage);
    this.state.invincible = true;
    this.state.invincibleTimer = 500;
    this.state.velocityX = this.state.facingRight ? -3 : 3;

    return damage;
  }

  getHitbox(): { x: number; y: number; width: number; height: number } {
    return {
      x: this.state.x,
      y: this.state.y - 48,
      width: 40,
      height: 48,
    };
  }

  getAttackHitbox(): { x: number; y: number; width: number; height: number } | null {
    if (!this.state.isAttacking && !this.state.isSpecialAttacking) return null;

    const range = this.state.isSpecialAttacking ? 70 : 50;
    return {
      x: this.state.facingRight ? this.state.x + 30 : this.state.x - range,
      y: this.state.y - 40,
      width: range,
      height: 30,
    };
  }

  reset(x: number): void {
    this.state = this.createInitialState(x);
    this.animationFrame = 0;
    this.animationTimer = 0;
  }
}

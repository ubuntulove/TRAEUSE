export interface PlayerState {
  x: number;
  y: number;
  velocityX: number;
  velocityY: number;
  hp: number;
  maxHp: number;
  energy: number;
  maxEnergy: number;
  isJumping: boolean;
  isAttacking: boolean;
  isDefending: boolean;
  isSpecialAttacking: boolean;
  facingRight: boolean;
  currentAnimation: AnimationState;
  attackCooldown: number;
  specialCooldown: number;
  invincible: boolean;
  invincibleTimer: number;
}

export type AnimationState = 
  | 'idle'
  | 'walk'
  | 'jump'
  | 'attack'
  | 'special'
  | 'defend'
  | 'hurt'
  | 'victory'
  | 'defeat';

export interface MechStats {
  name: string;
  maxHp: number;
  maxEnergy: number;
  attackDamage: [number, number];
  specialDamage: [number, number];
  defense: number;
  color: string;
  accentColor: string;
  secondaryColor: string;
}

export interface ControlScheme {
  left: string;
  right: string;
  jump: string;
  attack: string;
  defend: string;
  special: string;
}

export interface GameState {
  status: 'menu' | 'playing' | 'paused' | 'gameover';
  winner: 1 | 2 | null;
  players: [PlayerState, PlayerState];
}

export interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  maxLife: number;
  color: string;
  size: number;
}

export interface DamageNumber {
  x: number;
  y: number;
  damage: number;
  life: number;
}

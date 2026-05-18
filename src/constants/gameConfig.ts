export const GAME_CONFIG = {
  CANVAS_WIDTH: 800,
  CANVAS_HEIGHT: 450,
  FPS: 60,
  GROUND_Y: 350,
  GRAVITY: 0.8,
  JUMP_FORCE: -15,
  MOVE_SPEED: 5,
  PIXEL_SCALE: 4,
};

export const MECH_CONFIG: Record<'P1' | 'P2', {
  name: string;
  maxHp: number;
  maxEnergy: number;
  attackDamage: [number, number];
  specialDamage: [number, number];
  defense: number;
  color: string;
  accentColor: string;
  secondaryColor: string;
}> = {
  P1: {
    name: '烈焰战将',
    maxHp: 100,
    maxEnergy: 100,
    attackDamage: [10, 15] as [number, number],
    specialDamage: [25, 30] as [number, number],
    defense: 0.2,
    color: '#ff6b35',
    accentColor: '#f7c59f',
    secondaryColor: '#d63031',
  },
  P2: {
    name: '寒冰守卫',
    maxHp: 100,
    maxEnergy: 100,
    attackDamage: [10, 15] as [number, number],
    specialDamage: [25, 30] as [number, number],
    defense: 0.2,
    color: '#4ecdc4',
    accentColor: '#a8e6cf',
    secondaryColor: '#0984e3',
  },
};

export const COLORS = {
  BACKGROUND: '#1a1a2e',
  GROUND: '#4a4a4a',
  GROUND_LIGHT: '#5a5a5a',
  UI_BG: 'rgba(0, 0, 0, 0.7)',
  HP_HIGH: '#6bcb77',
  HP_MEDIUM: '#ffd93d',
  HP_LOW: '#ff6b6b',
  ENERGY: '#9d4edd',
  TEXT: '#ffffff',
  TEXT_GOLD: '#ffd93d',
};

export const CONTROLS = {
  P1: {
    left: 'KeyA',
    right: 'KeyD',
    jump: 'KeyW',
    attack: 'KeyJ',
    defend: 'KeyK',
    special: 'KeyL',
  },
  P2: {
    left: 'ArrowLeft',
    right: 'ArrowRight',
    jump: 'ArrowUp',
    attack: 'Numpad1',
    defend: 'Numpad2',
    special: 'Numpad3',
  },
};

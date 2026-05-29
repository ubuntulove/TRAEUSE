import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type GamePhase = 'title' | 'hacking' | 'combat' | 'victory' | 'gameover' | 'shop' | 'select';
export type NodeType = 'start' | 'normal' | 'end';

export interface Node {
  id: string;
  x: number;
  y: number;
  type: NodeType;
  connected: boolean;
}

export interface Enemy {
  id: string;
  x: number;
  y: number;
  health: number;
  maxHealth: number;
  hacked: boolean;
  type: 'basic' | 'armored' | 'fast';
}

export interface Projectile {
  id: string;
  x: number;
  y: number;
  vx: number;
  vy: number;
  isPlayer: boolean;
}

export interface GameState {
  phase: GamePhase;
  playerX: number;
  playerY: number;
  playerHealth: number;
  maxHealth: number;
  score: number;
  sector: number;
  wave: number;
  enemies: Enemy[];
  nodes: Node[];
  connections: { from: string; to: string }[];
  currentPath: string[];
  hackingTimeLeft: number;
  maxHackingTime: number;
  totalDataCollected: number;
  weaponsUnlocked: string[];
  activeWeapon: string;
  specialAmmo: number;
  projectiles: Projectile[];
}

export interface SaveData {
  sectorsUnlocked: number;
  highScore: number;
  totalDataCollected: number;
  weaponsUnlocked: string[];
  upgradesPurchased: string[];
}

interface GameStore extends GameState {
  setPhase: (phase: GamePhase) => void;
  setPlayerPosition: (x: number, y: number) => void;
  damagePlayer: (amount: number) => void;
  healPlayer: (amount: number) => void;
  addScore: (points: number) => void;
  nextSector: () => void;
  nextWave: () => void;
  setNodes: (nodes: Node[]) => void;
  addConnection: (from: string, to: string) => void;
  clearConnections: () => void;
  setCurrentPath: (path: string[]) => void;
  addToPath: (nodeId: string) => void;
  clearPath: () => void;
  setHackingTime: (time: number) => void;
  updateEnemies: (enemies: Enemy[]) => void;
  damageEnemy: (id: string, amount: number) => void;
  hackEnemy: (id: string) => void;
  spawnProjectile: (x: number, y: number, vx: number, vy: number, isPlayer: boolean) => void;
  updateProjectiles: (projectiles: Projectile[]) => void;
  unlockWeapon: (weapon: string) => void;
  setActiveWeapon: (weapon: string) => void;
  addSpecialAmmo: (amount: number) => void;
  useSpecialAmmo: () => boolean;
  resetGame: () => void;
  loadSave: () => SaveData | null;
  saveProgress: (data: SaveData) => void;
  initNewGame: () => void;
  startHackingPhase: () => void;
  startCombatPhase: () => void;
}

const initialState: GameState = {
  phase: 'title',
  playerX: 400,
  playerY: 400,
  playerHealth: 100,
  maxHealth: 100,
  score: 0,
  sector: 1,
  wave: 1,
  enemies: [],
  nodes: [],
  connections: [],
  currentPath: [],
  hackingTimeLeft: 10,
  maxHackingTime: 10,
  totalDataCollected: 0,
  weaponsUnlocked: ['blaster'],
  activeWeapon: 'blaster',
  specialAmmo: 3,
  projectiles: [],
};

export const useGameStore = create<GameStore>()(
  persist(
    (set, get) => ({
      ...initialState,

      setPhase: (phase) => set({ phase }),
      
      setPlayerPosition: (x, y) => set({ playerX: x, playerY: y }),
      
      damagePlayer: (amount) => {
        const newHealth = Math.max(0, get().playerHealth - amount);
        set({ playerHealth: newHealth });
        if (newHealth <= 0) {
          set({ phase: 'gameover' });
        }
      },
      
      healPlayer: (amount) => {
        const newHealth = Math.min(get().maxHealth, get().playerHealth + amount);
        set({ playerHealth: newHealth });
      },
      
      addScore: (points) => set((state) => ({ score: state.score + points, totalDataCollected: state.totalDataCollected + points })),
      
      nextSector: () => set((state) => ({ sector: state.sector + 1, wave: 1 })),
      
      nextWave: () => set((state) => ({ wave: state.wave + 1 })),
      
      setNodes: (nodes) => set({ nodes }),
      
      addConnection: (from, to) => set((state) => ({
        connections: [...state.connections, { from, to }]
      })),
      
      clearConnections: () => set({ connections: [] }),
      
      setCurrentPath: (path) => set({ currentPath: path }),
      
      addToPath: (nodeId) => set((state) => ({
        currentPath: [...state.currentPath, nodeId]
      })),
      
      clearPath: () => set({ currentPath: [] }),
      
      setHackingTime: (time) => set({ hackingTimeLeft: time }),
      
      updateEnemies: (enemies) => set({ enemies }),
      
      damageEnemy: (id, amount) => set((state) => ({
        enemies: state.enemies.map(e => 
          e.id === id ? { ...e, health: Math.max(0, e.health - amount) } : e
        )
      })),
      
      hackEnemy: (id) => set((state) => ({
        enemies: state.enemies.map(e => 
          e.id === id ? { ...e, hacked: true, health: Math.floor(e.health * 0.5) } : e
        )
      })),
      
      spawnProjectile: (x, y, vx, vy, isPlayer) => set((state) => ({
        projectiles: [...state.projectiles, { id: crypto.randomUUID(), x, y, vx, vy, isPlayer }]
      })),
      
      updateProjectiles: (projectiles) => set({ projectiles }),
      
      unlockWeapon: (weapon) => set((state) => ({
        weaponsUnlocked: state.weaponsUnlocked.includes(weapon) 
          ? state.weaponsUnlocked 
          : [...state.weaponsUnlocked, weapon]
      })),
      
      setActiveWeapon: (weapon) => set({ activeWeapon: weapon }),
      
      addSpecialAmmo: (amount) => set((state) => ({ specialAmmo: state.specialAmmo + amount })),
      
      useSpecialAmmo: () => {
        const ammo = get().specialAmmo;
        if (ammo > 0) {
          set({ specialAmmo: ammo - 1 });
          return true;
        }
        return false;
      },
      
      resetGame: () => set({ 
        ...initialState, 
        totalDataCollected: get().totalDataCollected,
        weaponsUnlocked: get().weaponsUnlocked 
      }),
      
      loadSave: () => {
        const saved = localStorage.getItem('nodebreaker-save');
        return saved ? JSON.parse(saved) : null;
      },
      
      saveProgress: (data) => {
        localStorage.setItem('nodebreaker-save', JSON.stringify(data));
      },
      
      initNewGame: () => set({ 
        ...initialState, 
        totalDataCollected: get().totalDataCollected,
        weaponsUnlocked: get().weaponsUnlocked 
      }),
      
      startHackingPhase: () => {
        const sector = get().sector;
        const wave = get().wave;
        const timeLimit = Math.max(5, 15 - sector * 2 - wave * 0.5);
        set({ 
          phase: 'hacking', 
          hackingTimeLeft: timeLimit,
          maxHackingTime: timeLimit,
          currentPath: [],
          connections: []
        });
      },
      
      startCombatPhase: () => set({ phase: 'combat' }),
    }),
    {
      name: 'nodebreaker-save',
      partialize: (state) => ({
        sector: state.sector,
        score: state.score,
        totalDataCollected: state.totalDataCollected,
        weaponsUnlocked: state.weaponsUnlocked,
      }),
    }
  )
);

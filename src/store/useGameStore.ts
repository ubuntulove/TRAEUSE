
import { create } from 'zustand';

export interface Building {
  id: string;
  type: 'refinery' | 'power-plant' | 'barracks' | 'factory';
  x: number;
  y: number;
  health: number;
  maxHealth: number;
  owner: 'player' | 'enemy';
}

export interface Unit {
  id: string;
  type: 'miner' | 'soldier' | 'tank';
  x: number;
  y: number;
  health: number;
  maxHealth: number;
  owner: 'player' | 'enemy';
  targetX?: number;
  targetY?: number;
  speed: number;
  damage: number;
  attackCooldown: number;
  lastAttackTime: number;
}

interface GameState {
  credits: number;
  power: number;
  ore: number;
  buildings: Building[];
  units: Unit[];
  selectedUnits: string[];
  gamePhase: 'menu' | 'playing' | 'paused' | 'victory' | 'defeat';
  addBuilding: (building: Building) => void;
  addUnit: (unit: Unit) => void;
  updateUnit: (id: string, updates: Partial<Unit>) => void;
  selectUnit: (id: string, multiSelect: boolean) => void;
  setGamePhase: (phase: GameState['gamePhase']) => void;
  updateResources: (credits?: number, power?: number, ore?: number) => void;
}

export const useGameStore = create<GameState>((set) => ({
  credits: 1000,
  power: 100,
  ore: 500,
  buildings: [],
  units: [],
  selectedUnits: [],
  gamePhase: 'menu',
  
  addBuilding: (building) => set((state) => ({
    buildings: [...state.buildings, building]
  })),
  
  addUnit: (unit) => set((state) => ({
    units: [...state.units, unit]
  })),
  
  updateUnit: (id, updates) => set((state) => ({
    units: state.units.map(u => u.id === id ? { ...u, ...updates } : u)
  })),
  
  selectUnit: (id, multiSelect) => set((state) => {
    if (multiSelect) {
      return {
        selectedUnits: state.selectedUnits.includes(id)
          ? state.selectedUnits.filter(uid => uid !== id)
          : [...state.selectedUnits, id]
      };
    }
    return { selectedUnits: [id] };
  }),
  
  setGamePhase: (phase) => set(() => ({ gamePhase: phase })),
  
  updateResources: (credits, power, ore) => set((state) => ({
    credits: credits !== undefined ? state.credits + credits : state.credits,
    power: power !== undefined ? state.power + power : state.power,
    ore: ore !== undefined ? state.ore + ore : state.ore,
  })),
}));

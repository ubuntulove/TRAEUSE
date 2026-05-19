export interface SaveGame {
  id: string;
  name: string;
  timestamp: number;
  playTime: number;
  mission: string;
  faction: string;
  map: string;
  thumbnail?: string;
  data: GameState;
}

export interface GameState {
  version: string;
  timestamp: number;
  player: PlayerState;
  entities: EntityState[];
  resources: ResourceState;
  techTree: TechTreeState;
  missions: MissionState;
  factions: FactionState;
  settings: SettingsState;
}

export interface PlayerState {
  id: string;
  name: string;
  level: number;
  experience: number;
  achievements: string[];
}

export interface EntityState {
  id: string;
  type: 'unit' | 'building' | 'resource';
  entityType: string;
  position: { x: number; y: number };
  health: number;
  maxHealth: number;
  team: number;
  data: any;
}

export interface ResourceState {
  credits: number;
  power: number;
  ore: number;
  gems: number;
  energy: number;
}

export interface TechTreeState {
  unlockedTechnologies: string[];
  currentResearch: string | null;
  researchProgress: number;
}

export interface MissionState {
  currentMission: string | null;
  completedMissions: string[];
  missionProgress: Map<string, any>;
}

export interface FactionState {
  playerFaction: string;
  relations: Array<{ factionA: string; factionB: string; relation: string }>;
}

export interface SettingsState {
  difficulty: 'easy' | 'normal' | 'hard' | 'expert';
  gameSpeed: number;
  soundVolume: number;
  musicVolume: number;
  graphicsQuality: 'low' | 'medium' | 'high';
}

export class SaveManager {
  private saves: Map<string, SaveGame> = new Map();
  private autoSaveEnabled: boolean = true;
  private autoSaveInterval: number = 300;
  private maxSaves: number = 10;
  private currentSaveId: string | null = null;
  private listeners: Map<string, Array<(data: any) => void>> = new Map();

  constructor() {
    this.loadFromStorage();
  }

  public save(
    name: string,
    gameState: GameState,
    options: {
      mission?: string;
      faction?: string;
      map?: string;
      thumbnail?: string;
    } = {}
  ): string {
    const id = `save_${Date.now()}`;

    const saveGame: SaveGame = {
      id,
      name,
      timestamp: Date.now(),
      playTime: gameState.timestamp,
      mission: options.mission || '',
      faction: options.faction || '',
      map: options.map || '',
      thumbnail: options.thumbnail,
      data: gameState,
    };

    this.saves.set(id, saveGame);
    this.currentSaveId = id;

    this.cleanupOldSaves();
    this.saveToStorage();

    this.emit('game-saved', { id, saveGame });

    return id;
  }

  public quickSave(gameState: GameState): string {
    return this.save('Quick Save', gameState, {
      mission: (gameState as any).currentMission,
      faction: (gameState as any).playerFaction,
      map: (gameState as any).currentMap,
    });
  }

  public load(id: string): SaveGame | null {
    const saveGame = this.saves.get(id);
    if (!saveGame) {
      return null;
    }

    this.currentSaveId = id;
    this.emit('game-loaded', { id, saveGame });

    return saveGame;
  }

  public loadLatest(): SaveGame | null {
    const saves = this.getAllSaves();
    if (saves.length === 0) {
      return null;
    }

    saves.sort((a, b) => b.timestamp - a.timestamp);
    return this.load(saves[0].id);
  }

  public delete(id: string): boolean {
    const saveGame = this.saves.get(id);
    if (!saveGame) {
      return false;
    }

    this.saves.delete(id);

    if (this.currentSaveId === id) {
      this.currentSaveId = null;
    }

    this.saveToStorage();

    this.emit('game-deleted', { id });

    return true;
  }

  public getSave(id: string): SaveGame | undefined {
    return this.saves.get(id);
  }

  public getAllSaves(): SaveGame[] {
    return Array.from(this.saves.values()).sort((a, b) => b.timestamp - a.timestamp);
  }

  public getSavesByMission(mission: string): SaveGame[] {
    return this.getAllSaves().filter(s => s.mission === mission);
  }

  public getCurrentSaveId(): string | null {
    return this.currentSaveId;
  }

  public rename(id: string, newName: string): boolean {
    const saveGame = this.saves.get(id);
    if (!saveGame) {
      return false;
    }

    saveGame.name = newName;
    this.saveToStorage();

    this.emit('game-renamed', { id, name: newName });

    return true;
  }

  public duplicate(id: string, newName: string): string | null {
    const saveGame = this.saves.get(id);
    if (!saveGame) {
      return null;
    }

    return this.save(newName, saveGame.data, {
      mission: saveGame.mission,
      faction: saveGame.faction,
      map: saveGame.map,
      thumbnail: saveGame.thumbnail,
    });
  }

  public export(id: string): string | null {
    const saveGame = this.saves.get(id);
    if (!saveGame) {
      return null;
    }

    return JSON.stringify(saveGame);
  }

  public import(jsonString: string): string | null {
    try {
      const saveGame: SaveGame = JSON.parse(jsonString);

      if (!saveGame.id || !saveGame.name || !saveGame.data) {
        throw new Error('Invalid save game format');
      }

      const newId = `save_${Date.now()}`;
      saveGame.id = newId;
      saveGame.timestamp = Date.now();

      this.saves.set(newId, saveGame);
      this.cleanupOldSaves();
      this.saveToStorage();

      this.emit('game-imported', { id: newId, saveGame });

      return newId;
    } catch (e) {
      console.error('Failed to import save:', e);
      return null;
    }
  }

  public setAutoSave(enabled: boolean, interval?: number): void {
    this.autoSaveEnabled = enabled;
    if (interval !== undefined) {
      this.autoSaveInterval = interval;
    }
  }

  public isAutoSaveEnabled(): boolean {
    return this.autoSaveEnabled;
  }

  public getAutoSaveInterval(): number {
    return this.autoSaveInterval;
  }

  public getSaveCount(): number {
    return this.saves.size;
  }

  public getMaxSaves(): number {
    return this.maxSaves;
  }

  public setMaxSaves(max: number): void {
    this.maxSaves = Math.max(1, max);
    this.cleanupOldSaves();
  }

  private cleanupOldSaves(): void {
    while (this.saves.size > this.maxSaves) {
      const saves = this.getAllSaves();
      if (saves.length > 0) {
        this.delete(saves[saves.length - 1].id);
      }
    }
  }

  private saveToStorage(): void {
    try {
      const savesArray = Array.from(this.saves.entries());
      localStorage.setItem('red-alert-saves', JSON.stringify(savesArray));
    } catch (e) {
      console.error('Failed to save to storage:', e);
    }
  }

  private loadFromStorage(): void {
    try {
      const data = localStorage.getItem('red-alert-saves');
      if (data) {
        const savesArray = JSON.parse(data);
        for (const [id, save] of savesArray) {
          this.saves.set(id, save);
        }
      }
    } catch (e) {
      console.error('Failed to load from storage:', e);
    }
  }

  public clearAll(): void {
    this.saves.clear();
    this.currentSaveId = null;
    localStorage.removeItem('red-alert-saves');
    this.emit('all-saves-cleared', {});
  }

  public on(event: string, callback: (data: any) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(callback);
  }

  public off(event: string, callback: (data: any) => void): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      const index = callbacks.indexOf(callback);
      if (index !== -1) {
        callbacks.splice(index, 1);
      }
    }
  }

  private emit(event: string, data: any): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      for (const callback of callbacks) {
        callback(data);
      }
    }
  }
}

export class Serializer {
  public static serializeEntity(entity: any): EntityState {
    return {
      id: entity.id,
      type: entity.type,
      entityType: entity.entityType || entity.type,
      position: { x: entity.position.x, y: entity.position.y },
      health: entity.health,
      maxHealth: entity.maxHealth,
      team: entity.team,
      data: {
        ...entity.stats,
        ...entity,
      },
    };
  }

  public static deserializeEntity(state: EntityState): any {
    return {
      id: state.id,
      type: state.type,
      entityType: state.entityType,
      position: { ...state.position },
      health: state.health,
      maxHealth: state.maxHealth,
      team: state.team,
      ...state.data,
    };
  }

  public static createGameState(
    version: string,
    player: PlayerState,
    entities: EntityState[],
    resources: ResourceState,
    techTree: TechTreeState,
    missions: MissionState,
    factions: FactionState,
    settings: SettingsState
  ): GameState {
    return {
      version,
      timestamp: Date.now(),
      player,
      entities,
      resources,
      techTree,
      missions,
      factions,
      settings,
    };
  }

  public static validateGameState(state: GameState): boolean {
    if (!state.version) return false;
    if (!state.player) return false;
    if (!state.resources) return false;
    if (!state.entities) return false;
    return true;
  }

  public static compress(state: GameState): string {
    return JSON.stringify(state);
  }

  public static decompress(json: string): GameState | null {
    try {
      return JSON.parse(json);
    } catch (e) {
      return null;
    }
  }
}

export class AutoSave {
  private saveManager: SaveManager;
  private intervalId: number | null = null;
  private lastAutoSave: number = 0;

  constructor(saveManager: SaveManager) {
    this.saveManager = saveManager;
  }

  public start(getStateCallback: () => GameState, onSaveCallback?: () => void): void {
    if (this.intervalId !== null) {
      return;
    }

    const interval = this.saveManager.getAutoSaveInterval() * 1000;

    this.intervalId = window.setInterval(() => {
      if (!this.saveManager.isAutoSaveEnabled()) {
        return;
      }

      const state = getStateCallback();
      const saveId = this.saveManager.quickSave(state);
      this.lastAutoSave = Date.now();

      if (onSaveCallback) {
        onSaveCallback();
      }

      console.log(`Auto-saved: ${saveId}`);
    }, interval);
  }

  public stop(): void {
    if (this.intervalId !== null) {
      window.clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  public getLastAutoSaveTime(): number {
    return this.lastAutoSave;
  }

  public forceSave(getStateCallback: () => GameState): string {
    const state = getStateCallback();
    const saveId = this.saveManager.quickSave(state);
    this.lastAutoSave = Date.now();
    return saveId;
  }
}

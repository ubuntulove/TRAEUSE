import { Vector2D } from '../../entities/Entity';

export interface Tile {
  x: number;
  y: number;
  type: string;
  elevation: number;
  walkable: boolean;
  buildable: boolean;
  visible: boolean;
  explored: boolean;
  resourceType?: string;
  resourceAmount?: number;
}

export interface MapRegion {
  id: string;
  name: string;
  tiles: Vector2D[];
  isOwned: boolean;
  ownerTeam?: number;
  resourceValue: number;
}

export interface MapConfig {
  width: number;
  height: number;
  tileSize: number;
  name: string;
  description: string;
  difficulty: 'tutorial' | 'easy' | 'normal' | 'hard' | 'expert';
  maxPlayers: number;
  allowedTeams: number[];
}

export interface MapEvent {
  id: string;
  type: 'spawn' | 'reinforce' | 'objective' | 'timer';
  triggerTime: number;
  data: any;
  triggered: boolean;
}

export class GameMap {
  private config: MapConfig;
  private tiles: Tile[][] = [];
  private regions: Map<string, MapRegion> = new Map();
  private events: MapEvent[] = [];
  private exploredTiles: Set<string> = new Set();
  private visibleTiles: Set<string> = new Set();
  private fogOfWarEnabled: boolean = true;

  constructor(config: Partial<MapConfig> = {}) {
    this.config = {
      width: config.width || 100,
      height: config.height || 100,
      tileSize: config.tileSize || 40,
      name: config.name || 'Unknown Map',
      description: config.description || '',
      difficulty: config.difficulty || 'normal',
      maxPlayers: config.maxPlayers || 2,
      allowedTeams: config.allowedTeams || [0, 1],
    };

    this.initializeTiles();
  }

  private initializeTiles(): void {
    this.tiles = [];

    for (let x = 0; x < this.config.width; x++) {
      this.tiles[x] = [];
      for (let y = 0; y < this.config.height; y++) {
        this.tiles[x][y] = {
          x,
          y,
          type: 'grass',
          elevation: 0,
          walkable: true,
          buildable: true,
          visible: false,
          explored: false,
        };
      }
    }
  }

  public generateProcedural(seed: number = Date.now()): void {
    const random = this.seededRandom(seed);

    for (let x = 0; x < this.config.width; x++) {
      for (let y = 0; y < this.config.height; y++) {
        const tile = this.tiles[x][y];

        const value = random();
        if (value < 0.1) {
          tile.type = 'water';
          tile.walkable = false;
          tile.buildable = false;
        } else if (value < 0.15) {
          tile.type = 'mountain';
          tile.walkable = false;
          tile.buildable = false;
          tile.elevation = 2;
        } else if (value < 0.2) {
          tile.type = 'forest';
          tile.walkable = true;
          tile.buildable = true;
        } else if (value < 0.25) {
          tile.type = 'ore';
          tile.resourceType = 'ore';
          tile.resourceAmount = Math.floor(random() * 1000) + 500;
        } else {
          tile.type = 'grass';
        }

        tile.elevation = random() * 2;
      }
    }
  }

  private seededRandom(seed: number): () => number {
    return () => {
      seed = (seed * 1103515245 + 12345) & 0x7fffffff;
      return seed / 0x7fffffff;
    };
  }

  public getTile(x: number, y: number): Tile | null {
    if (x < 0 || x >= this.config.width || y < 0 || y >= this.config.height) {
      return null;
    }
    return this.tiles[x][y];
  }

  public setTile(x: number, y: number, tile: Partial<Tile>): void {
    if (x < 0 || x >= this.config.width || y < 0 || y >= this.config.height) {
      return;
    }
    Object.assign(this.tiles[x][y], tile);
  }

  public getTiles(): Tile[][] {
    return this.tiles;
  }

  public getConfig(): MapConfig {
    return { ...this.config };
  }

  public getSize(): { width: number; height: number } {
    return { width: this.config.width, height: this.config.height };
  }

  public getTileSize(): number {
    return this.config.tileSize;
  }

  public worldToTile(worldX: number, worldY: number): Vector2D {
    return {
      x: Math.floor(worldX / this.config.tileSize),
      y: Math.floor(worldY / this.config.tileSize),
    };
  }

  public tileToWorld(tileX: number, tileY: number): Vector2D {
    return {
      x: tileX * this.config.tileSize + this.config.tileSize / 2,
      y: tileY * this.config.tileSize + this.config.tileSize / 2,
    };
  }

  public isWalkable(x: number, y: number): boolean {
    const tile = this.getTile(x, y);
    return tile ? tile.walkable : false;
  }

  public isBuildable(x: number, y: number): boolean {
    const tile = this.getTile(x, y);
    return tile ? tile.buildable : false;
  }

  public setVisible(x: number, y: number, visible: boolean, team: number): void {
    const tile = this.getTile(x, y);
    if (!tile) return;

    tile.visible = visible;

    if (visible) {
      this.visibleTiles.add(`${x},${y}`);
      tile.explored = true;
      this.exploredTiles.add(`${x},${y}`);
    } else {
      this.visibleTiles.delete(`${x},${y}`);
    }
  }

  public isExplored(x: number, y: number): boolean {
    const tile = this.getTile(x, y);
    return tile ? tile.explored : false;
  }

  public isVisible(x: number, y: number): boolean {
    const tile = this.getTile(x, y);
    return tile ? tile.visible : false;
  }

  public revealAll(team: number): void {
    for (let x = 0; x < this.config.width; x++) {
      for (let y = 0; y < this.config.height; y++) {
        this.setVisible(x, y, true, team);
      }
    }
  }

  public hideAll(): void {
    for (let x = 0; x < this.config.width; x++) {
      for (let y = 0; y < this.config.height; y++) {
        this.tiles[x][y].visible = false;
      }
    }
    this.visibleTiles.clear();
  }

  public getExploredTiles(): Tile[] {
    return Array.from(this.exploredTiles).map(key => {
      const [x, y] = key.split(',').map(Number);
      return this.tiles[x][y];
    });
  }

  public getVisibleTiles(): Tile[] {
    return Array.from(this.visibleTiles).map(key => {
      const [x, y] = key.split(',').map(Number);
      return this.tiles[x][y];
    });
  }

  public setFogOfWar(enabled: boolean): void {
    this.fogOfWarEnabled = enabled;
  }

  public isFogOfWarEnabled(): boolean {
    return this.fogOfWarEnabled;
  }

  public addRegion(region: MapRegion): void {
    this.regions.set(region.id, region);
  }

  public removeRegion(id: string): void {
    this.regions.delete(id);
  }

  public getRegion(id: string): MapRegion | undefined {
    return this.regions.get(id);
  }

  public getAllRegions(): MapRegion[] {
    return Array.from(this.regions.values());
  }

  public getRegionAt(x: number, y: number): MapRegion | undefined {
    for (const region of this.regions.values()) {
      if (region.tiles.some(t => t.x === x && t.y === y)) {
        return region;
      }
    }
    return undefined;
  }

  public captureRegion(regionId: string, team: number): void {
    const region = this.regions.get(regionId);
    if (region) {
      region.isOwned = true;
      region.ownerTeam = team;
    }
  }

  public addEvent(event: MapEvent): void {
    this.events.push(event);
  }

  public removeEvent(id: string): void {
    this.events = this.events.filter(e => e.id !== id);
  }

  public getEvent(id: string): MapEvent | undefined {
    return this.events.find(e => e.id === id);
  }

  public getAllEvents(): MapEvent[] {
    return [...this.events];
  }

  public updateEvents(currentTime: number): void {
    for (const event of this.events) {
      if (!event.triggered && currentTime >= event.triggerTime) {
        event.triggered = true;
      }
    }
  }

  public getTriggeredEvents(): MapEvent[] {
    return this.events.filter(e => e.triggered);
  }

  public reset(): void {
    this.initializeTiles();
    this.regions.clear();
    this.events = [];
    this.exploredTiles.clear();
    this.visibleTiles.clear();
  }

  public toJSON(): object {
    return {
      config: this.config,
      tiles: this.tiles,
      regions: Array.from(this.regions.entries()),
      events: this.events,
    };
  }

  public fromJSON(data: any): void {
    if (data.config) {
      this.config = { ...this.config, ...data.config };
    }
    if (data.tiles) {
      this.tiles = data.tiles;
    }
    if (data.regions) {
      this.regions = new Map(data.regions);
    }
    if (data.events) {
      this.events = data.events;
    }
  }
}

export class MapGenerator {
  public static createRandomMap(
    width: number,
    height: number,
    seed: number = Date.now(),
    options: {
      waterCoverage?: number;
      forestCoverage?: number;
      mountainCoverage?: number;
      resourceDensity?: number;
    } = {}
  ): GameMap {
    const config: Partial<MapConfig> = {
      width,
      height,
      tileSize: 40,
      name: `Random Map ${Date.now()}`,
      difficulty: 'normal',
      maxPlayers: 2,
      allowedTeams: [0, 1],
    };

    const map = new GameMap(config);
    map.generateProcedural(seed);

    return map;
  }

  public static createBattleMap(
    width: number,
    height: number,
    teams: number = 2
  ): GameMap {
    const config: Partial<MapConfig> = {
      width,
      height,
      tileSize: 40,
      name: `Battle Map ${teams}v${teams}`,
      difficulty: 'normal',
      maxPlayers: teams * 2,
      allowedTeams: Array.from({ length: teams * 2 }, (_, i) => i),
    };

    const map = new GameMap(config);

    const random = () => Math.random();
    for (let x = 0; x < width; x++) {
      for (let y = 0; y < height; y++) {
        const tile = map.getTile(x, y)!;

        if (random() < 0.1) {
          tile.type = 'water';
          tile.walkable = false;
          tile.buildable = false;
        } else if (random() < 0.05) {
          tile.type = 'mountain';
          tile.walkable = false;
          tile.buildable = false;
        }

        if (x < 10 || x > width - 10 || y < 10 || y > height - 10) {
          if (random() < 0.3) {
            tile.type = 'ore';
            tile.resourceType = 'ore';
            tile.resourceAmount = Math.floor(random() * 1000) + 500;
          }
        }
      }
    }

    return map;
  }

  public static createArenaMap(size: number = 50): GameMap {
    const config: Partial<MapConfig> = {
      width: size,
      height: size,
      tileSize: 40,
      name: 'Arena',
      difficulty: 'normal',
      maxPlayers: 4,
      allowedTeams: [0, 1, 2, 3],
    };

    const map = new GameMap(config);

    for (let x = 0; x < size; x++) {
      for (let y = 0; y < size; y++) {
        const tile = map.getTile(x, y)!;

        const distFromCenter = Math.sqrt(
          Math.pow(x - size / 2, 2) + Math.pow(y - size / 2, 2)
        );

        if (distFromCenter < 5) {
          tile.type = 'grass';
          tile.buildable = false;
        }

        if (x < 5 || x >= size - 5 || y < 5 || y >= size - 5) {
          tile.type = 'wall';
          tile.walkable = false;
          tile.buildable = false;
        }
      }
    }

    return map;
  }
}

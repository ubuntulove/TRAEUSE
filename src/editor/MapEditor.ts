export interface MapEditorConfig {
  width: number;
  height: number;
  tileSize: number;
  layers: number;
  gridSize: number;
  snapToGrid: boolean;
  showGrid: boolean;
  showCoordinates: boolean;
  showBoundingBoxes: boolean;
  autoSave: boolean;
  autoSaveInterval: number;
  undoLevels: number;
  redoLevels: number;
}

export interface MapEditorState {
  mode: EditorMode;
  tool: EditorTool;
  brushSize: number;
  brushShape: BrushShape;
  selectedTiles: Set<string>;
  selectedObjects: Set<string>;
  clipboard: ClipboardData | null;
  history: HistoryState;
  isDirty: boolean;
  lastSaved: number;
}

export enum EditorMode {
  SELECT = 'select',
  PAINT = 'paint',
  ERASE = 'erase',
  FILL = 'fill',
  DECORATE = 'decorate',
  SCRIPT = 'script',
  TEST = 'test',
}

export enum EditorTool {
  BRUSH = 'brush',
  RECTANGLE = 'rectangle',
  ELLIPSE = 'ellipse',
  LINE = 'line',
  POLYGON = 'polygon',
  EYEDROPPER = 'eyedropper',
  SELECTION = 'selection',
  PAN = 'pan',
  ZOOM = 'zoom',
  ERASER = 'eraser',
  CLONE = 'clone',
  TRANSFORM = 'transform',
}

export enum BrushShape {
  SQUARE = 'square',
  CIRCLE = 'circle',
  DIAMOND = 'diamond',
  CROSS = 'cross',
}

export interface ClipboardData {
  type: ClipboardType;
  data: unknown;
  offset: Vec2;
}

export enum ClipboardType {
  TILES = 'tiles',
  OBJECTS = 'objects',
  REGION = 'region',
}

export interface HistoryState {
  undoStack: HistoryEntry[];
  redoStack: HistoryEntry[];
  maxUndoLevels: number;
}

export interface HistoryEntry {
  type: HistoryType;
  data: unknown;
  timestamp: number;
  description: string;
}

export enum HistoryType {
  TILE_PAINT = 'tile_paint',
  TILE_ERASE = 'tile_erase',
  TILE_FILL = 'tile_fill',
  OBJECT_CREATE = 'object_create',
  OBJECT_DELETE = 'object_delete',
  OBJECT_MOVE = 'object_move',
  OBJECT_TRANSFORM = 'object_transform',
  OBJECT_PROPERTY = 'object_property',
  REGION_CREATE = 'region_create',
  REGION_DELETE = 'region_delete',
  SCRIPT_ADD = 'script_add',
  SCRIPT_REMOVE = 'script_remove',
  LAYER_ADD = 'layer_add',
  LAYER_DELETE = 'layer_delete',
  LAYER_REORDER = 'layer_reorder',
  MAP_SETTINGS = 'map_settings',
}

export interface Vec2 {
  x: number;
  y: number;
}

export interface Vec3 {
  x: number;
  y: number;
  z: number;
}

export interface Rect {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface TileLayer {
  id: string;
  name: string;
  type: LayerType;
  visible: boolean;
  locked: boolean;
  opacity: number;
  parallaxX: number;
  parallaxY: number;
  tiles: Map<string, TileData>;
  offsetX: number;
  offsetY: number;
}

export enum LayerType {
  TILE = 'tile',
  OBJECT = 'object',
  COLLISION = 'collision',
  NAVIGATION = 'navigation',
  TRIGGER = 'trigger',
  DECORATION = 'decoration',
  LIGHT = 'light',
  FOG = 'fog',
}

export interface TileData {
  x: number;
  y: number;
  tilesetId: string;
  tileId: number;
  rotation: number;
  flipX: boolean;
  flipY: boolean;
  color?: Color;
  properties: Record<string, unknown>;
}

export interface Tileset {
  id: string;
  name: string;
  image: string;
  imageWidth: number;
  imageHeight: number;
  tileWidth: number;
  tileHeight: number;
  spacing: number;
  margin: number;
  tileCount: number;
  columns: number;
  firstgid: number;
  tileOffset?: Vec2;
  properties: Record<string, TilesetProperty>;
  animations: Map<number, TileAnimation>;
}

export interface TilesetProperty {
  name: string;
  type: PropertyType;
  value: unknown;
  default?: unknown;
}

export enum PropertyType {
  STRING = 'string',
  NUMBER = 'number',
  BOOLEAN = 'boolean',
  COLOR = 'color',
  FILE = 'file',
  OBJECT = 'object',
}

export interface TileAnimation {
  tileId: number;
  frames: TileAnimationFrame[];
  duration: number;
}

export interface TileAnimationFrame {
  tileId: number;
  duration: number;
}

export interface MapObject {
  id: string;
  name: string;
  type: string;
  x: number;
  y: number;
  width: number;
  height: number;
  rotation: number;
  visible: boolean;
  locked: boolean;
  properties: Record<string, unknown>;
  children: MapObject[];
  parentId?: string;
  shape: ObjectShape;
  vertices?: Vec2[];
  points?: Vec2[];
}

export enum ObjectShape {
  RECTANGLE = 'rectangle',
  ELLIPSE = 'ellipse',
  POINT = 'point',
  POLYGON = 'polygon',
  POLYLINE = 'polyline',
  TEXT = 'text',
  TEMPLATE = 'template',
}

export interface MapRegion {
  id: string;
  name: string;
  type: string;
  x: number;
  y: number;
  width: number;
  height: number;
  color: Color;
  properties: Record<string, unknown>;
}

export interface Color {
  r: number;
  g: number;
  b: number;
  a: number;
}

export interface MapTrigger {
  id: string;
  name: string;
  type: TriggerType;
  conditions: TriggerCondition[];
  actions: TriggerAction[];
  enabled: boolean;
  once: boolean;
  cooldowns: Map<string, number>;
}

export enum TriggerType {
  ON_ENTER = 'on_enter',
  ON_EXIT = 'on_exit',
  ON_INTERACT = 'on_interact',
  ON_DESTROY = 'on_destroy',
  ON_TIMER = 'on_timer',
  ON_VALUE = 'on_value',
  ON_CUSTOM = 'on_custom',
}

export interface TriggerCondition {
  type: ConditionType;
  parameters: Record<string, unknown>;
  negate: boolean;
}

export enum ConditionType {
  PLAYER_IN_REGION = 'player_in_region',
  PLAYER_HAS_ITEM = 'player_has_item',
  PLAYER_HEALTH = 'player_health',
  OBJECT_EXISTS = 'object_exists',
  VALUE_COMPARE = 'value_compare',
  TIME_OF_DAY = 'time_of_day',
  GAME_STATE = 'game_state',
  RANDOM = 'random',
}

export interface TriggerAction {
  type: ActionType;
  parameters: Record<string, unknown>;
  delay: number;
}

export enum ActionType {
  SPAWN_OBJECT = 'spawn_object',
  DESTROY_OBJECT = 'destroy_object',
  MOVE_OBJECT = 'move_object',
  PLAY_SOUND = 'play_sound',
  PLAY_MUSIC = 'play_music',
  SHOW_MESSAGE = 'show_message',
  SET_VARIABLE = 'set_variable',
  CHANGE_TILES = 'change_tiles',
  ENABLE_TRIGGER = 'enable_trigger',
  DISABLE_TRIGGER = 'disable_trigger',
  CAMERA_SHAKE = 'camera_shake',
  START_CUTSCENE = 'start_cutscene',
  GRANT_ACHIEVEMENT = 'grant_achievement',
}

export interface MapEvent {
  id: string;
  name: string;
  type: EventType;
  position: Vec2;
  radius: number;
  script: string;
  properties: Record<string, unknown>;
}

export enum EventType {
  TRIGGER = 'trigger',
  SPAWNER = 'spawner',
  CHECKPOINT = 'checkpoint',
  PORTAL = 'portal',
  DIALOGUE = 'dialogue',
  SHOP = 'shop',
  TREASURE = 'treasure',
  DECORATION = 'decoration',
}

export interface MapLayer {
  id: string;
  name: string;
  type: LayerType;
  visible: boolean;
  locked: boolean;
  opacity: number;
  data: unknown;
}

export interface MapMarker {
  id: string;
  name: string;
  type: MarkerType;
  position: Vec3;
  color: Color;
  icon: string;
  size: number;
  rotation: number;
  properties: Record<string, unknown>;
}

export enum MarkerType {
  START = 'start',
  END = 'end',
  CHECKPOINT = 'checkpoint',
  ENEMY = 'enemy',
  ITEM = 'item',
  CUSTOM = 'custom',
}

export interface MapCollision {
  x: number;
  y: number;
  width: number;
  height: number;
  type: CollisionType;
  properties: Record<string, unknown>;
}

export enum CollisionType {
  SOLID = 'solid',
  PLATFORM = 'platform',
  LADDER = 'ladder',
  WATER = 'water',
  HAZARD = 'hazard',
  TRIGGER = 'trigger',
}

export interface NavigationNode {
  x: number;
  y: number;
  connections: number[];
  cost: number;
  flags: NavigationFlags;
}

export enum NavigationFlags {
  WALKABLE = 1,
  SWIMMABLE = 2,
  FLYABLE = 4,
  JUMPABLE = 8,
}

export interface MapLight {
  id: string;
  name: string;
  x: number;
  y: number;
  radius: number;
  color: Color;
  intensity: number;
  angle: number;
  type: LightType;
  falloff: number;
  shadows: boolean;
}

export enum LightType {
  POINT = 'point',
  SPOT = 'spot',
  DIRECTIONAL = 'directional',
  AMBIENT = 'ambient',
}

export interface MapFog {
  density: number;
  color: Color;
  startDistance: number;
  endDistance: number;
  type: FogType;
}

export enum FogType {
  LINEAR = 'linear',
  EXPONENTIAL = 'exponential',
  EXPONENTIAL2 = 'exponential2',
}

export interface MapWeather {
  type: WeatherType;
  intensity: number;
  direction: Vec2;
  color?: Color;
  particles: WeatherParticleConfig;
}

export enum WeatherType {
  CLEAR = 'clear',
  RAIN = 'rain',
  SNOW = 'snow',
  FOG = 'fog',
  SANDSTORM = 'sandstorm',
  STORM = 'storm',
}

export interface WeatherParticleConfig {
  count: number;
  speed: number;
  lifetime: number;
  size: number;
  spread: number;
}

export interface MapTemplate {
  id: string;
  name: string;
  category: string;
  thumbnail: string;
  width: number;
  height: number;
  layers: MapLayer[];
  objects: MapObject[];
  tilesets: string[];
  properties: Record<string, unknown>;
}

export interface EditorBrush {
  id: string;
  name: string;
  type: BrushType;
  size: number;
  shape: BrushShape;
  tilesetId?: string;
  tileId?: number;
  objectType?: string;
  color?: Color;
  rotation: number;
  flipX: boolean;
  flipY: boolean;
  opacity: number;
  noise?: NoiseConfig;
  gradient?: GradientConfig;
}

export enum BrushType {
  TILE = 'tile',
  OBJECT = 'object',
  COLLISION = 'collision',
  DECORATION = 'decoration',
  ERASE = 'erase',
}

export interface NoiseConfig {
  scale: number;
  octaves: number;
  persistence: number;
  lacunarity: number;
  seed: number;
}

export interface GradientConfig {
  type: GradientType;
  stops: GradientStop[];
  angle: number;
}

export enum GradientType {
  LINEAR = 'linear',
  RADIAL = 'radial',
  CONICAL = 'conical',
}

export interface GradientStop {
  position: number;
  color: Color;
}

export class MapEditor {
  private config: MapEditorConfig;
  private state: MapEditorState;
  private map: GameMap;
  private tilesets: Map<string, Tileset>;
  private templates: Map<string, MapTemplate>;
  private layers: Map<string, MapLayer>;
  private brushes: Map<string, EditorBrush>;
  private activeBrush: EditorBrush | null = null;
  private selection: SelectionBox | null = null;
  private undoManager: UndoManager;
  private autoSaveTimer: number = 0;

  constructor(config: Partial<MapEditorConfig> = {}) {
    this.config = {
      width: config.width || 100,
      height: config.height || 100,
      tileSize: config.tileSize || 32,
      layers: config.layers || 4,
      gridSize: config.gridSize || 1,
      snapToGrid: config.snapToGrid ?? true,
      showGrid: config.showGrid ?? true,
      showCoordinates: config.showCoordinates ?? false,
      showBoundingBoxes: config.showBoundingBoxes ?? true,
      autoSave: config.autoSave ?? true,
      autoSaveInterval: config.autoSaveInterval || 60000,
      undoLevels: config.undoLevels || 50,
      redoLevels: config.redoLevels || 50,
    };

    this.state = this.createInitialState();
    this.map = this.createEmptyMap();
    this.tilesets = new Map();
    this.templates = new Map();
    this.layers = new Map();
    this.brushes = new Map();
    this.undoManager = new UndoManager(this.config.undoLevels);
    this.initializeDefaultLayers();
    this.initializeDefaultBrushes();
  }

  private createInitialState(): MapEditorState {
    return {
      mode: EditorMode.PAINT,
      tool: EditorTool.BRUSH,
      brushSize: 1,
      brushShape: BrushShape.SQUARE,
      selectedTiles: new Set(),
      selectedObjects: new Set(),
      clipboard: null,
      history: {
        undoStack: [],
        redoStack: [],
        maxUndoLevels: this.config.undoLevels,
      },
      isDirty: false,
      lastSaved: 0,
    };
  }

  private createEmptyMap(): GameMap {
    return {
      id: this.generateId(),
      name: 'Untitled Map',
      width: this.config.width,
      height: this.config.height,
      tileWidth: this.config.tileSize,
      tileHeight: this.config.tileSize,
      properties: {},
      tilesets: [],
      layers: [],
      objects: [],
      collision: [],
      navigation: [],
      triggers: [],
      events: [],
      lights: [],
      fog: { density: 0, color: { r: 255, g: 255, b: 255, a: 255 }, startDistance: 0, endDistance: 100, type: FogType.LINEAR },
      weather: { type: WeatherType.CLEAR, intensity: 0, direction: { x: 0, y: 1 }, particles: { count: 0, speed: 0, lifetime: 0, size: 0, spread: 0 } },
      backgroundColor: { r: 64, g: 64, b: 64, a: 255 },
      music: '',
      ambientSound: '',
      spawnPoints: [],
      playerStart: { x: 0, y: 0 },
      victoryConditions: [],
    };
  }

  private initializeDefaultLayers(): void {
    this.createLayer('Background', LayerType.TILE);
    this.createLayer('Collision', LayerType.COLLISION);
    this.createLayer('Objects', LayerType.OBJECT);
    this.createLayer('Triggers', LayerType.TRIGGER);
  }

  private initializeDefaultBrushes(): void {
    const defaultBrushes: EditorBrush[] = [
      { id: 'brush-square', name: 'Square Brush', type: BrushType.TILE, size: 1, shape: BrushShape.SQUARE, rotation: 0, flipX: false, flipY: false, opacity: 1 },
      { id: 'brush-circle', name: 'Circle Brush', type: BrushType.TILE, size: 1, shape: BrushShape.CIRCLE, rotation: 0, flipX: false, flipY: false, opacity: 1 },
      { id: 'brush-diamond', name: 'Diamond Brush', type: BrushType.TILE, size: 1, shape: BrushShape.DIAMOND, rotation: 0, flipX: false, flipY: false, opacity: 1 },
      { id: 'brush-cross', name: 'Cross Brush', type: BrushType.TILE, size: 1, shape: BrushShape.CROSS, rotation: 0, flipX: false, flipY: false, opacity: 1 },
      { id: 'eraser', name: 'Eraser', type: BrushType.ERASE, size: 1, shape: BrushShape.SQUARE, rotation: 0, flipX: false, flipY: false, opacity: 1 },
    ];

    defaultBrushes.forEach(brush => {
      this.brushes.set(brush.id, brush);
    });

    this.activeBrush = defaultBrushes[0];
  }

  public setMode(mode: EditorMode): void {
    this.state.mode = mode;
    this.updateToolForMode(mode);
  }

  private updateToolForMode(mode: EditorMode): void {
    switch (mode) {
      case EditorMode.SELECT:
        this.state.tool = EditorTool.SELECTION;
        break;
      case EditorMode.PAINT:
        this.state.tool = EditorTool.BRUSH;
        break;
      case EditorMode.ERASE:
        this.state.tool = EditorTool.ERASER;
        break;
      case EditorMode.FILL:
        this.state.tool = EditorTool.FLOOD_FILL;
        break;
      default:
        break;
    }
  }

  public setTool(tool: EditorTool): void {
    this.state.tool = tool;
    this.updateModeForTool(tool);
  }

  private updateModeForTool(tool: EditorTool): void {
    switch (tool) {
      case EditorTool.SELECTION:
        this.state.mode = EditorMode.SELECT;
        break;
      case EditorTool.BRUSH:
        this.state.mode = EditorMode.PAINT;
        break;
      case EditorTool.ERASER:
        this.state.mode = EditorMode.ERASE;
        break;
      default:
        break;
    }
  }

  public setBrush(brush: EditorBrush): void {
    this.activeBrush = brush;
    this.state.brushSize = brush.size;
    this.state.brushShape = brush.shape;
  }

  public setBrushSize(size: number): void {
    this.state.brushSize = Math.max(1, Math.min(32, size));
    if (this.activeBrush) {
      this.activeBrush.size = this.state.brushSize;
    }
  }

  public setBrushShape(shape: BrushShape): void {
    this.state.brushShape = shape;
    if (this.activeBrush) {
      this.activeBrush.shape = shape;
    }
  }

  public paintTiles(layerId: string, x: number, y: number, tileId: number, tilesetId: string): void {
    const layer = this.layers.get(layerId);
    if (!layer || layer.type !== LayerType.TILE) {
      return;
    }

    const tiles = layer.data as Map<string, TileData>;
    const affectedTiles = this.getTilesInBrush(x, y);

    const entry: HistoryEntry = {
      type: HistoryType.TILE_PAINT,
      data: {
        layerId,
        tiles: new Map(affectedTiles.map(pos => {
          const key = `${pos.x},${pos.y}`;
          return [key, tiles.get(key) ? { ...tiles.get(key)! } : null];
        })),
      },
      timestamp: Date.now(),
      description: 'Paint tiles',
    };
    this.undoManager.push(entry);

    affectedTiles.forEach(pos => {
      const key = `${pos.x},${pos.y}`;
      const tile: TileData = {
        x: pos.x,
        y: pos.y,
        tilesetId,
        tileId,
        rotation: this.activeBrush?.rotation || 0,
        flipX: this.activeBrush?.flipX || false,
        flipY: this.activeBrush?.flipY || false,
        properties: {},
      };
      tiles.set(key, tile);
    });

    this.state.isDirty = true;
  }

  public eraseTiles(layerId: string, x: number, y: number): void {
    const layer = this.layers.get(layerId);
    if (!layer || layer.type !== LayerType.TILE) {
      return;
    }

    const tiles = layer.data as Map<string, TileData>;
    const affectedTiles = this.getTilesInBrush(x, y);

    const entry: HistoryEntry = {
      type: HistoryType.TILE_ERASE,
      data: {
        layerId,
        tiles: new Map(affectedTiles.map(pos => {
          const key = `${pos.x},${pos.y}`;
          return [key, tiles.get(key) ? { ...tiles.get(key)! } : null];
        })),
      },
      timestamp: Date.now(),
      description: 'Erase tiles',
    };
    this.undoManager.push(entry);

    affectedTiles.forEach(pos => {
      const key = `${pos.x},${pos.y}`;
      tiles.delete(key);
    });

    this.state.isDirty = true;
  }

  public fillTiles(layerId: string, startX: number, startY: number, tileId: number, tilesetId: string): void {
    const layer = this.layers.get(layerId);
    if (!layer || layer.type !== LayerType.TILE) {
      return;
    }

    const tiles = layer.data as Map<string, TileData>;
    const startKey = `${startX},${startY}`;
    const startTile = tiles.get(startKey);
    const targetTileId = startTile?.tileId;

    if (targetTileId === tileId) {
      return;
    }

    const visited = new Set<string>();
    const queue: Vec2[] = [{ x: startX, y: startY }];
    const replacements: Map<string, TileData | null> = new Map();

    while (queue.length > 0) {
      const current = queue.shift()!;
      const key = `${current.x},${current.y}`;

      if (visited.has(key)) {
        continue;
      }

      visited.add(key);

      const tile = tiles.get(key);
      if (!tile || tile.tileId !== targetTileId) {
        continue;
      }

      replacements.set(key, { ...tile });

      const neighbors = [
        { x: current.x + 1, y: current.y },
        { x: current.x - 1, y: current.y },
        { x: current.x, y: current.y + 1 },
        { x: current.x, y: current.y - 1 },
      ];

      neighbors.forEach(n => {
        const nKey = `${n.x},${n.y}`;
        if (!visited.has(nKey) && n.x >= 0 && n.y >= 0 && n.x < this.map.width && n.y < this.map.height) {
          queue.push(n);
        }
      });
    }

    const entry: HistoryEntry = {
      type: HistoryType.TILE_FILL,
      data: { layerId, tiles: replacements },
      timestamp: Date.now(),
      description: 'Fill tiles',
    };
    this.undoManager.push(entry);

    replacements.forEach((oldTile, key) => {
      const [x, y] = key.split(',').map(Number);
      if (oldTile) {
        tiles.set(key, {
          x,
          y,
          tilesetId,
          tileId,
          rotation: this.activeBrush?.rotation || 0,
          flipX: this.activeBrush?.flipX || false,
          flipY: this.activeBrush?.flipY || false,
          properties: {},
        });
      } else {
        tiles.delete(key);
      }
    });

    this.state.isDirty = true;
  }

  private getTilesInBrush(centerX: number, centerY: number): Vec2[] {
    const tiles: Vec2[] = [];
    const halfSize = Math.floor(this.state.brushSize / 2);
    const startX = centerX - halfSize;
    const startY = centerY - halfSize;

    for (let dx = 0; dx < this.state.brushSize; dx++) {
      for (let dy = 0; dy < this.state.brushSize; dy++) {
        const x = startX + dx;
        const y = startY + dy;

        if (this.isPositionInBrush(x, y, centerX, centerY)) {
          tiles.push({ x, y });
        }
      }
    }

    return tiles;
  }

  private isPositionInBrush(x: number, y: number, centerX: number, centerY: number): boolean {
    const dx = x - centerX + 0.5;
    const dy = y - centerY + 0.5;

    switch (this.state.brushShape) {
      case BrushShape.SQUARE:
        return true;

      case BrushShape.CIRCLE:
        return dx * dx + dy * dy <= (this.state.brushSize / 2) * (this.state.brushSize / 2);

      case BrushShape.DIAMOND:
        return Math.abs(dx) + Math.abs(dy) <= this.state.brushSize / 2;

      case BrushShape.CROSS:
        return Math.abs(dx) <= 0.5 || Math.abs(dy) <= 0.5;

      default:
        return true;
    }
  }

  public createObject(layerId: string, type: string, x: number, y: number, properties: Record<string, unknown> = {}): MapObject {
    const object: MapObject = {
      id: this.generateId(),
      name: `Object_${this.map.objects.length + 1}`,
      type,
      x,
      y,
      width: 32,
      height: 32,
      rotation: 0,
      visible: true,
      locked: false,
      properties,
      children: [],
      shape: ObjectShape.RECTANGLE,
    };

    const entry: HistoryEntry = {
      type: HistoryType.OBJECT_CREATE,
      data: { object: { ...object }, layerId },
      timestamp: Date.now(),
      description: `Create ${type}`,
    };
    this.undoManager.push(entry);

    this.map.objects.push(object);
    this.state.isDirty = true;

    return object;
  }

  public deleteObject(objectId: string): void {
    const index = this.map.objects.findIndex(o => o.id === objectId);
    if (index === -1) {
      return;
    }

    const object = this.map.objects[index];

    const entry: HistoryEntry = {
      type: HistoryType.OBJECT_DELETE,
      data: { object: { ...object }, index },
      timestamp: Date.now(),
      description: `Delete ${object.type}`,
    };
    this.undoManager.push(entry);

    this.map.objects.splice(index, 1);
    this.state.isDirty = true;
  }

  public moveObject(objectId: string, x: number, y: number): void {
    const object = this.map.objects.find(o => o.id === objectId);
    if (!object) {
      return;
    }

    const oldX = object.x;
    const oldY = object.y;

    const entry: HistoryEntry = {
      type: HistoryType.OBJECT_MOVE,
      data: { objectId, oldX, oldY, newX: x, newY: y },
      timestamp: Date.now(),
      description: `Move ${object.name}`,
    };
    this.undoManager.push(entry);

    object.x = x;
    object.y = y;
    this.state.isDirty = true;
  }

  public transformObject(objectId: string, width: number, height: number, rotation: number): void {
    const object = this.map.objects.find(o => o.id === objectId);
    if (!object) {
      return;
    }

    const entry: HistoryEntry = {
      type: HistoryType.OBJECT_TRANSFORM,
      data: {
        objectId,
        oldWidth: object.width,
        oldHeight: object.height,
        oldRotation: object.rotation,
        newWidth: width,
        newHeight: height,
        newRotation: rotation,
      },
      timestamp: Date.now(),
      description: `Transform ${object.name}`,
    };
    this.undoManager.push(entry);

    object.width = width;
    object.height = height;
    object.rotation = rotation;
    this.state.isDirty = true;
  }

  public setObjectProperty(objectId: string, key: string, value: unknown): void {
    const object = this.map.objects.find(o => o.id === objectId);
    if (!object) {
      return;
    }

    const oldValue = object.properties[key];

    const entry: HistoryEntry = {
      type: HistoryType.OBJECT_PROPERTY,
      data: { objectId, key, oldValue, newValue: value },
      timestamp: Date.now(),
      description: `Set ${object.name}.${key}`,
    };
    this.undoManager.push(entry);

    object.properties[key] = value;
    this.state.isDirty = true;
  }

  public createLayer(name: string, type: LayerType): MapLayer {
    const layer: MapLayer = {
      id: this.generateId(),
      name,
      type,
      visible: true,
      locked: false,
      opacity: 1,
      data: type === LayerType.TILE ? new Map() : [],
    };

    this.layers.set(layer.id, layer);

    const entry: HistoryEntry = {
      type: HistoryType.LAYER_ADD,
      data: { layer: { ...layer } },
      timestamp: Date.now(),
      description: `Add layer ${name}`,
    };
    this.undoManager.push(entry);

    return layer;
  }

  public deleteLayer(layerId: string): void {
    const layer = this.layers.get(layerId);
    if (!layer) {
      return;
    }

    const entry: HistoryEntry = {
      type: HistoryType.LAYER_DELETE,
      data: { layer: { ...layer }, index: Array.from(this.layers.keys()).indexOf(layerId) },
      timestamp: Date.now(),
      description: `Delete layer ${layer.name}`,
    };
    this.undoManager.push(entry);

    this.layers.delete(layerId);
    this.state.isDirty = true;
  }

  public reorderLayers(fromIndex: number, toIndex: number): void {
    const layerIds = Array.from(this.layers.keys());
    const layerId = layerIds[fromIndex];

    layerIds.splice(fromIndex, 1);
    layerIds.splice(toIndex, 0, layerId);

    const entry: HistoryEntry = {
      type: HistoryType.LAYER_REORDER,
      data: { fromIndex, toIndex },
      timestamp: Date.now(),
      description: 'Reorder layers',
    };
    this.undoManager.push(entry);

    this.state.isDirty = true;
  }

  public addTileset(tileset: Tileset): void {
    this.tilesets.set(tileset.id, tileset);
    this.map.tilesets.push(tileset.id);
    this.state.isDirty = true;
  }

  public removeTileset(tilesetId: string): void {
    this.tilesets.delete(tilesetId);
    const index = this.map.tilesets.indexOf(tilesetId);
    if (index !== -1) {
      this.map.tilesets.splice(index, 1);
    }
    this.state.isDirty = true;
  }

  public createTemplate(name: string, category: string, objects: MapObject[]): MapTemplate {
    const template: MapTemplate = {
      id: this.generateId(),
      name,
      category,
      thumbnail: '',
      width: 0,
      height: 0,
      layers: Array.from(this.layers.values()),
      objects: objects.map(o => ({ ...o })),
      tilesets: this.map.tilesets.slice(),
      properties: {},
    };

    objects.forEach(obj => {
      template.width = Math.max(template.width, obj.x + obj.width);
      template.height = Math.max(template.height, obj.y + obj.height);
    });

    this.templates.set(template.id, template);
    return template;
  }

  public placeTemplate(templateId: string, x: number, y: number): void {
    const template = this.templates.get(templateId);
    if (!template) {
      return;
    }

    const createdObjects: MapObject[] = [];

    template.objects.forEach(obj => {
      const newObject = this.createObject('Objects', obj.type, x + obj.x, y + obj.y, { ...obj.properties });
      newObject.width = obj.width;
      newObject.height = obj.height;
      newObject.rotation = obj.rotation;
      createdObjects.push(newObject);
    });

    this.state.isDirty = true;
  }

  public setSelection(rect: Rect | null): void {
    this.selection = rect;
    this.state.selectedTiles.clear();
    this.state.selectedObjects.clear();

    if (rect) {
      this.selectTilesInRect(rect);
      this.selectObjectsInRect(rect);
    }
  }

  private selectTilesInRect(rect: Rect): void {
    this.layers.forEach(layer => {
      if (layer.type === LayerType.TILE) {
        const tiles = layer.data as Map<string, TileData>;
        tiles.forEach((tile, key) => {
          if (tile.x >= rect.x && tile.x < rect.x + rect.width && tile.y >= rect.y && tile.y < rect.y + rect.height) {
            this.state.selectedTiles.add(key);
          }
        });
      }
    });
  }

  private selectObjectsInRect(rect: Rect): void {
    this.map.objects.forEach(obj => {
      if (this.isObjectInRect(obj, rect)) {
        this.state.selectedObjects.add(obj.id);
      }
    });
  }

  private isObjectInRect(obj: MapObject, rect: Rect): boolean {
    return (
      obj.x < rect.x + rect.width &&
      obj.x + obj.width > rect.x &&
      obj.y < rect.y + rect.height &&
      obj.y + obj.height > rect.y
    );
  }

  public copySelection(): void {
    if (!this.selection) {
      return;
    }

    const copiedTiles = new Map<string, TileData | null>();
    const copiedObjects: MapObject[] = [];

    this.state.selectedTiles.forEach(key => {
      this.layers.forEach(layer => {
        if (layer.type === LayerType.TILE) {
          const tiles = layer.data as Map<string, TileData>;
          if (tiles.has(key)) {
            copiedTiles.set(key, { ...tiles.get(key)! });
          }
        }
      });
    });

    this.state.selectedObjects.forEach(id => {
      const obj = this.map.objects.find(o => o.id === id);
      if (obj) {
        copiedObjects.push({ ...obj });
      }
    });

    this.state.clipboard = {
      type: copiedTiles.size > 0 ? ClipboardType.TILES : ClipboardType.OBJECTS,
      data: copiedTiles.size > 0 ? copiedTiles : copiedObjects,
      offset: { x: this.selection.x, y: this.selection.y },
    };
  }

  public cutSelection(): void {
    this.copySelection();
    this.deleteSelection();
  }

  public pasteClipboard(x: number, y: number): void {
    if (!this.state.clipboard) {
      return;
    }

    const offsetX = x - this.state.clipboard.offset.x;
    const offsetY = y - this.state.clipboard.offset.y;

    if (this.state.clipboard.type === ClipboardType.TILES) {
      const tiles = this.state.clipboard.data as Map<string, TileData | null>;
      tiles.forEach((tile, key) => {
        if (tile) {
          const newTile = { ...tile, x: tile.x + offsetX, y: tile.y + offsetY };
          this.state.selectedTiles.add(`${newTile.x},${newTile.y}`);
        }
      });
    } else if (this.state.clipboard.type === ClipboardType.OBJECTS) {
      const objects = this.state.clipboard.data as MapObject[];
      objects.forEach(obj => {
        const newObject = this.createObject('Objects', obj.type, obj.x + offsetX, obj.y + offsetY, { ...obj.properties });
        newObject.width = obj.width;
        newObject.height = obj.height;
        newObject.rotation = obj.rotation;
        this.state.selectedObjects.add(newObject.id);
      });
    }

    this.state.isDirty = true;
  }

  public deleteSelection(): void {
    this.state.selectedTiles.forEach(key => {
      this.layers.forEach(layer => {
        if (layer.type === LayerType.TILE) {
          const tiles = layer.data as Map<string, TileData>;
          tiles.delete(key);
        }
      });
    });

    this.state.selectedObjects.forEach(id => {
      this.deleteObject(id);
    });

    this.state.selectedTiles.clear();
    this.state.selectedObjects.clear();
    this.selection = null;
    this.state.isDirty = true;
  }

  public undo(): void {
    const entry = this.undoManager.undo();
    if (!entry) {
      return;
    }

    this.applyHistoryEntry(entry, true);
    this.state.history.redoStack.push(entry);
    this.state.isDirty = true;
  }

  public redo(): void {
    const entry = this.state.history.redoStack.pop();
    if (!entry) {
      return;
    }

    this.applyHistoryEntry(entry, false);
    this.state.history.undoStack.push(entry);
    this.state.isDirty = true;
  }

  private applyHistoryEntry(entry: HistoryEntry, isUndo: boolean): void {
    const data = entry.data as Record<string, unknown>;

    switch (entry.type) {
      case HistoryType.TILE_PAINT:
      case HistoryType.TILE_ERASE:
      case HistoryType.TILE_FILL:
        this.applyTileHistory(data, isUndo);
        break;

      case HistoryType.OBJECT_CREATE:
        if (isUndo) {
          const obj = data.object as MapObject;
          const index = this.map.objects.findIndex(o => o.id === obj.id);
          if (index !== -1) {
            this.map.objects.splice(index, 1);
          }
        } else {
          this.map.objects.push(data.object as MapObject);
        }
        break;

      case HistoryType.OBJECT_DELETE:
        if (isUndo) {
          this.map.objects.splice(data.index as number, 0, data.object as MapObject);
        } else {
          const obj = data.object as MapObject;
          const index = this.map.objects.findIndex(o => o.id === obj.id);
          if (index !== -1) {
            this.map.objects.splice(index, 1);
          }
        }
        break;

      case HistoryType.OBJECT_MOVE:
        const moveObj = this.map.objects.find(o => o.id === data.objectId);
        if (moveObj) {
          if (isUndo) {
            moveObj.x = data.oldX as number;
            moveObj.y = data.oldY as number;
          } else {
            moveObj.x = data.newX as number;
            moveObj.y = data.newY as number;
          }
        }
        break;

      case HistoryType.OBJECT_TRANSFORM:
        const transformObj = this.map.objects.find(o => o.id === data.objectId);
        if (transformObj) {
          if (isUndo) {
            transformObj.width = data.oldWidth as number;
            transformObj.height = data.oldHeight as number;
            transformObj.rotation = data.oldRotation as number;
          } else {
            transformObj.width = data.newWidth as number;
            transformObj.height = data.newHeight as number;
            transformObj.rotation = data.newRotation as number;
          }
        }
        break;

      case HistoryType.OBJECT_PROPERTY:
        const propObj = this.map.objects.find(o => o.id === data.objectId);
        if (propObj) {
          if (isUndo) {
            propObj.properties[data.key as string] = data.oldValue;
          } else {
            propObj.properties[data.key as string] = data.newValue;
          }
        }
        break;

      case HistoryType.LAYER_ADD:
        if (isUndo) {
          this.layers.delete((data.layer as MapLayer).id);
        } else {
          this.layers.set((data.layer as MapLayer).id, data.layer as MapLayer);
        }
        break;

      case HistoryType.LAYER_DELETE:
        if (isUndo) {
          this.layers.set((data.layer as MapLayer).id, data.layer as MapLayer);
          this.map.layers.splice(data.index as number, 0, (data.layer as MapLayer).id);
        } else {
          this.layers.delete((data.layer as MapLayer).id);
          const idx = this.map.layers.indexOf((data.layer as MapLayer).id);
          if (idx !== -1) {
            this.map.layers.splice(idx, 1);
          }
        }
        break;
    }
  }

  private applyTileHistory(data: Record<string, unknown>, isUndo: boolean): void {
    const layerId = data.layerId as string;
    const layer = this.layers.get(layerId);

    if (!layer || layer.type !== LayerType.TILE) {
      return;
    }

    const tiles = layer.data as Map<string, TileData | null>;

    if (isUndo) {
      (data.tiles as Map<string, TileData | null>).forEach((tile, key) => {
        if (tile) {
          tiles.set(key, tile);
        } else {
          tiles.delete(key);
        }
      });
    } else {
      (data.tiles as Map<string, TileData | null>).forEach((tile, key) => {
        tiles.delete(key);
      });
    }
  }

  public canUndo(): boolean {
    return this.state.history.undoStack.length > 0;
  }

  public canRedo(): boolean {
    return this.state.history.redoStack.length > 0;
  }

  public saveMap(): string {
    const mapData = this.serializeMap();
    const json = JSON.stringify(mapData);
    this.state.isDirty = false;
    this.state.lastSaved = Date.now();
    return json;
  }

  public loadMap(json: string): void {
    const mapData = JSON.parse(json) as GameMap;
    this.map = mapData;
    this.rebuildLayersFromMap();
    this.state.isDirty = false;
  }

  private serializeMap(): GameMap {
    const serializedLayers: Record<string, unknown>[] = [];
    this.layers.forEach((layer, id) => {
      serializedLayers.push({ id, ...layer });
    });

    return {
      ...this.map,
      layers: serializedLayers as unknown as string[],
    };
  }

  private rebuildLayersFromMap(): void {
    this.layers.clear();
    if (this.map.layers) {
      this.map.layers.forEach((layerData: unknown) => {
        if (typeof layerData === 'object' && layerData !== null) {
          const layer = layerData as MapLayer;
          this.layers.set(layer.id, layer);
        }
      });
    }
  }

  public exportMap(format: ExportFormat): string {
    switch (format) {
      case ExportFormat.JSON:
        return this.exportAsJSON();

      case ExportFormat.IMAGE:
        return this.exportAsImage();

      case ExportFormat.OBJ:
        return this.exportAsOBJ();

      default:
        return '';
    }
  }

  private exportAsJSON(): string {
    return JSON.stringify(this.serializeMap(), null, 2);
  }

  private exportAsImage(): string {
    return '';
  }

  private exportAsOBJ(): string {
    let obj = '# Map Export\n';
    let vertexOffset = 1;

    this.map.objects.forEach((objData, i) => {
      const vertices = this.getObjectVertices(objData);
      vertices.forEach(v => {
        obj += `v ${v.x} ${v.y} ${v.z}\n`;
      });

      for (let j = 0; j < vertices.length; j++) {
        obj += `f ${vertexOffset + j} ${vertexOffset + ((j + 1) % vertices.length)}\n`;
      }

      vertexOffset += vertices.length;
    });

    return obj;
  }

  private getObjectVertices(obj: MapObject): Vec3[] {
    const cos = Math.cos(obj.rotation * Math.PI / 180);
    const sin = Math.sin(obj.rotation * Math.PI / 180);
    const hw = obj.width / 2;
    const hh = obj.height / 2;

    const corners = [
      { x: -hw, y: -hh, z: 0 },
      { x: hw, y: -hh, z: 0 },
      { x: hw, y: hh, z: 0 },
      { x: -hw, y: hh, z: 0 },
    ];

    return corners.map(c => ({
      x: obj.x + c.x * cos - c.y * sin,
      y: 0,
      z: obj.y + c.x * sin + c.y * cos,
    }));
  }

  public update(deltaTime: number): void {
    if (this.config.autoSave && this.state.isDirty) {
      this.autoSaveTimer += deltaTime;
      if (this.autoSaveTimer >= this.config.autoSaveInterval) {
        this.saveMap();
        this.autoSaveTimer = 0;
      }
    }
  }

  public getMap(): GameMap {
    return { ...this.map };
  }

  public getLayers(): Map<string, MapLayer> {
    return new Map(this.layers);
  }

  public getTilesets(): Map<string, Tileset> {
    return new Map(this.tilesets);
  }

  public getTemplates(): Map<string, MapTemplate> {
    return new Map(this.templates);
  }

  public getBrushes(): Map<string, EditorBrush> {
    return new Map(this.brushes);
  }

  public getActiveBrush(): EditorBrush | null {
    return this.activeBrush;
  }

  public getState(): MapEditorState {
    return { ...this.state };
  }

  public getConfig(): MapEditorConfig {
    return { ...this.config };
  }

  public isDirty(): boolean {
    return this.state.isDirty;
  }

  public setShowGrid(show: boolean): void {
    this.config.showGrid = show;
  }

  public setSnapToGrid(snap: boolean): void {
    this.config.snapToGrid = snap;
  }

  public setShowCoordinates(show: boolean): void {
    this.config.showCoordinates = show;
  }

  public snapToGrid(value: number): number {
    if (!this.config.snapToGrid) {
      return value;
    }
    return Math.round(value / this.config.gridSize) * this.config.gridSize;
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  public resize(width: number, height: number): void {
    const oldWidth = this.map.width;
    const oldHeight = this.map.height;

    this.map.width = width;
    this.map.height = height;

    this.layers.forEach(layer => {
      if (layer.type === LayerType.TILE) {
        const tiles = layer.data as Map<string, TileData>;
        const keysToDelete: string[] = [];
        tiles.forEach((tile, key) => {
          if (tile.x >= width || tile.y >= height) {
            keysToDelete.push(key);
          }
        });
        keysToDelete.forEach(key => tiles.delete(key));
      }
    });

    this.map.objects = this.map.objects.filter(obj =>
      obj.x < width && obj.y < height
    );

    const entry: HistoryEntry = {
      type: HistoryType.MAP_SETTINGS,
      data: { oldWidth, oldHeight, newWidth: width, newHeight: height },
      timestamp: Date.now(),
      description: 'Resize map',
    };
    this.undoManager.push(entry);

    this.state.isDirty = true;
  }

  public clear(): void {
    this.layers.forEach(layer => {
      if (layer.type === LayerType.TILE) {
        (layer.data as Map<string, TileData>).clear();
      } else {
        (layer.data as unknown[]) = [];
      }
    });

    this.map.objects = [];
    this.map.collision = [];
    this.map.navigation = [];
    this.map.triggers = [];
    this.map.events = [];

    this.state.isDirty = true;
  }

  public destroy(): void {
    this.layers.clear();
    this.tilesets.clear();
    this.templates.clear();
    this.brushes.clear();
    this.state.selectedTiles.clear();
    this.state.selectedObjects.clear();
    this.state.history.undoStack = [];
    this.state.history.redoStack = [];
  }
}

export enum ExportFormat {
  JSON = 'json',
  IMAGE = 'image',
  OBJ = 'obj',
}

export interface GameMap {
  id: string;
  name: string;
  width: number;
  height: number;
  tileWidth: number;
  tileHeight: number;
  properties: Record<string, unknown>;
  tilesets: string[];
  layers: unknown[];
  objects: MapObject[];
  collision: MapCollision[];
  navigation: NavigationNode[];
  triggers: MapTrigger[];
  events: MapEvent[];
  lights: MapLight[];
  fog: MapFog;
  weather: MapWeather;
  backgroundColor: Color;
  music: string;
  ambientSound: string;
  spawnPoints: Vec2[];
  playerStart: Vec2;
  victoryConditions: string[];
}

export interface SelectionBox {
  x: number;
  y: number;
  width: number;
  height: number;
  startX: number;
  startY: number;
}

export class UndoManager {
  private undoStack: HistoryEntry[] = [];
  private redoStack: HistoryEntry[] = [];
  private maxLevels: number;

  constructor(maxLevels: number) {
    this.maxLevels = maxLevels;
  }

  public push(entry: HistoryEntry): void {
    this.undoStack.push(entry);
    if (this.undoStack.length > this.maxLevels) {
      this.undoStack.shift();
    }
    this.redoStack = [];
  }

  public undo(): HistoryEntry | undefined {
    return this.undoStack.pop();
  }

  public redo(): HistoryEntry | undefined {
    const entry = this.redoStack.pop();
    if (entry) {
      this.undoStack.push(entry);
    }
    return entry;
  }

  public canUndo(): boolean {
    return this.undoStack.length > 0;
  }

  public canRedo(): boolean {
    return this.redoStack.length > 0;
  }

  public clear(): void {
    this.undoStack = [];
    this.redoStack = [];
  }
}

export const createMapEditor = (config?: Partial<MapEditorConfig>): MapEditor => {
  return new MapEditor(config);
};

export default MapEditor;

import { Point, Rectangle, Color, Unit, Building, Resource, Terrain, Vision, Player, MapData } from '../core/GameTypes';

export enum MarkerType {
  UNIT = 'unit',
  BUILDING = 'building',
  RESOURCE = 'resource',
  PING = 'ping',
  ALERT = 'alert',
  SELECTION = 'selection',
  VISION = 'vision',
  FOG = 'fog',
  ATTACK = 'attack',
  DEFEND = 'defend',
  MOVE = 'move',
  BUILD = 'build',
  HARVEST = 'harvest',
  REPAIR = 'repair',
}

export enum UnitGroup {
  INFANTRY = 'infantry',
  VEHICLE = 'vehicle',
  AIR = 'air',
  NAVAL = 'naval',
  WORKER = 'worker',
  BUILDING = 'building',
}

export enum Alliance {
  ALLY = 'ally',
  ENEMY = 'enemy',
  NEUTRAL = 'neutral',
  UNKNOWN = 'unknown',
}

export interface RadarConfig {
  width: number;
  height: number;
  mapWidth: number;
  mapHeight: number;
  updateInterval: number;
  scanRadius: number;
  pingDuration: number;
  alertDuration: number;
  fogEnabled: boolean;
  showResources: boolean;
  showBuildings: boolean;
  showUnits: boolean;
  showTerrain: boolean;
  showVision: boolean;
  showAlerts: boolean;
  zoomLevels: number[];
  defaultZoom: number;
  minZoom: number;
  maxZoom: number;
  colors: RadarColors;
  scanQuality: number;
  maxPings: number;
  maxAlerts: number;
  clickThrottle: number;
}

export interface RadarColors {
  ally: Color;
  enemy: Color;
  neutral: Color;
  unknown: Color;
  friendly: Color;
  hostile: Color;
  resource: Color;
  building: Color;
  selection: Color;
  ping: Color;
  alert: Color;
  vision: Color;
  fog: Color;
  terrain: Color;
  grid: Color;
  background: Color;
  border: Color;
  text: Color;
  healthBar: Color;
  energyBar: Color;
  shieldBar: Color;
}

export interface MinimapSettings {
  position: Point;
  size: Rectangle;
  borderWidth: number;
  borderColor: Color;
  backgroundColor: Color;
  opacity: number;
  visibility: 'always' | 'combat' | 'manual';
  lockPosition: boolean;
  showMinimap: boolean;
  showFullMap: boolean;
  autoFollow: boolean;
  showCoordinates: boolean;
  showGrid: boolean;
  gridSize: number;
  enableClicks: boolean;
  enableDrag: boolean;
  smoothScroll: boolean;
  scrollSpeed: number;
  showTooltips: boolean;
  tooltipDelay: number;
}

export interface RadarState {
  visible: boolean;
  zoom: number;
  panOffset: Point;
  currentView: Rectangle;
  isScanning: boolean;
  lastScanTime: number;
  scanProgress: number;
  totalUnits: number;
  visibleUnits: number;
  totalBuildings: number;
  visibleBuildings: number;
  totalResources: number;
  visibleResources: number;
  alerts: number;
  pings: number;
  selectionStart: Point | null;
  selectionEnd: Point | null;
  isDragging: boolean;
  isPanning: boolean;
  hoveredUnit: Unit | null;
  selectedUnits: Unit[];
  focusedPlayer: Player | null;
}

export interface MinimapData {
  terrain: TerrainData;
  units: UnitData[];
  buildings: BuildingData[];
  resources: ResourceData[];
  vision: VisionData;
  fog: FogData;
  alerts: AlertData[];
  pings: PingMarker[];
  grid: GridData | null;
  viewport: Rectangle;
  zoom: number;
  panOffset: Point;
  timestamp: number;
}

export interface TerrainData {
  heightMap: Float32Array;
  walkabilityMap: Uint8Array;
  terrainTypes: Uint8Array;
  features: TerrainFeature[];
  width: number;
  height: number;
  resolution: number;
}

export interface TerrainFeature {
  type: TerrainFeatureType;
  position: Point;
  size: number;
  name: string;
  passable: boolean;
}

export enum TerrainFeatureType {
  ROCK = 'rock',
  TREE = 'tree',
  WATER = 'water',
  CLIFF = 'cliff',
  BUILDING = 'building',
  ROAD = 'road',
  BRIDGE = 'bridge',
  RUIN = 'ruin',
  VILLAGE = 'village',
  CASTLE = 'castle',
  TOWER = 'tower',
  WALL = 'wall',
}

export interface UnitData {
  id: string;
  type: string;
  position: Point;
  alliance: Alliance;
  health: number;
  maxHealth: number;
  energy: number;
  maxEnergy: number;
  shield: number;
  maxShield: number;
  group: UnitGroup;
  isSelected: boolean;
  isVisible: boolean;
  isAlive: boolean;
  isMoving: boolean;
  isAttacking: boolean;
  isConstructing: boolean;
  isHarvesting: boolean;
  owner: string;
  velocity: Point;
  facing: number;
  size: number;
  sightRange: number;
  radius: number;
  icon: string;
  status: string[];
}

export interface BuildingData {
  id: string;
  type: string;
  position: Point;
  alliance: Alliance;
  health: number;
  maxHealth: number;
  energy: number;
  maxEnergy: number;
  shield: number;
  maxShield: number;
  isSelected: boolean;
  isVisible: boolean;
  isAlive: boolean;
  isConstructing: boolean;
  isDamaged: boolean;
  isDestroyed: boolean;
  owner: string;
  size: Point;
  sightRange: number;
  radius: number;
  icon: string;
  unitCapacity: number;
  currentUnits: number;
  productionQueue: string[];
  is RallyPoint: boolean;
  rallyPoint: Point | null;
}

export interface ResourceData {
  id: string;
  type: ResourceType;
  position: Point;
  alliance: Alliance;
  amount: number;
  maxAmount: number;
  isVisible: boolean;
  isDepleted: boolean;
  owner: string | null;
  harvesters: string[];
  capacity: number;
  regenRate: number;
  lastRegenTime: number;
}

export enum ResourceType {
  GOLD = 'gold',
  WOOD = 'wood',
  STONE = 'stone',
  FOOD = 'food',
  IRON = 'iron',
  CRYSTAL = 'crystal',
  OIL = 'oil',
  ENERGY = 'energy',
}

export interface VisionData {
  visibleAreas: VisionArea[];
  unitVision: UnitVision[];
  buildingVision: BuildingVision[];
  sharedVision: SharedVision[];
  timestamp: number;
}

export interface VisionArea {
  playerId: string;
  areas: Rectangle[];
}

export interface UnitVision {
  unitId: string;
  ownerId: string;
  center: Point;
  radius: number;
  shape: VisionShape;
}

export enum VisionShape {
  CIRCULAR = 'circular',
  SQUARE = 'square',
  CONE = 'cone',
}

export interface BuildingVision {
  buildingId: string;
  ownerId: string;
  center: Point;
  radius: number;
  shape: VisionShape;
  isActive: boolean;
}

export interface SharedVision {
  playerId: string;
  sharedWith: string[];
}

export interface FogData {
  explored: Uint8Array;
  visible: Uint8Array;
  lastUpdate: number;
  resolution: number;
  width: number;
  height: number;
}

export interface AlertData {
  id: string;
  type: AlertType;
  position: Point;
  message: string;
  timestamp: number;
  duration: number;
  priority: AlertPriority;
  acknowledged: boolean;
  source: string;
  target: string | null;
  alliance: Alliance;
}

export enum AlertType {
  UNIT_ATTACKED = 'unit_attacked',
  BUILDING_ATTACKED = 'building_attacked',
  UNIT_DESTROYED = 'unit_destroyed',
  BUILDING_DESTROYED = 'building_destroyed',
  UNIT_CREATED = 'unit_created',
  BUILDING_COMPLETED = 'building_completed',
  RESOURCE_DEPLETED = 'resource_depleted',
  UNIT_HEALED = 'unit_healed',
  VISION_GAINED = 'vision_gained',
  VISION_LOST = 'vision_lost',
  AREA_UNDER_ATTACK = 'area_under_attack',
  ENEMY_SPOTTED = 'enemy_spotted',
  ALLY_NEEDS_HELP = 'ally_needs_help',
  GARRISON_COMPLETE = 'garrison_complete',
  RESEARCH_COMPLETE = 'research_complete',
  UPGRADE_AVAILABLE = 'upgrade_available',
}

export enum AlertPriority {
  CRITICAL = 'critical',
  HIGH = 'high',
  MEDIUM = 'medium',
  LOW = 'low',
  INFO = 'info',
}

export interface PingMarker {
  id: string;
  type: PingType;
  position: Point;
  playerId: string;
  playerName: string;
  message: string;
  timestamp: number;
  duration: number;
  color: Color;
  size: number;
  fadeOut: boolean;
  isExpired: boolean;
  icon: string;
}

export enum PingType {
  ATTENTION = 'attention',
  ATTACK = 'attack',
  DEFEND = 'defend',
  MOVE = 'move',
  BUILD = 'build',
  HARVEST = 'harvest',
  REPAIR = 'repair',
  DANGER = 'danger',
  REINFORCE = 'reinforce',
  GATHER = 'gather',
  EXPLORE = 'explore',
  CANCEL = 'cancel',
}

export interface GridData {
  cells: GridCell[];
  cellSize: number;
  width: number;
  height: number;
  showGrid: boolean;
  showCoordinates: boolean;
}

export interface GridCell {
  x: number;
  y: number;
  centerX: number;
  centerY: number;
  isOccupied: boolean;
  isPassable: boolean;
  terrainType: number;
}

export interface SelectionBox {
  start: Point;
  end: Point;
  isActive: boolean;
  units: Unit[];
  buildings: Building[];
  isValid: boolean;
  contains: Point[];
}

export interface MinimapClickEvent {
  position: Point;
  worldPosition: Point;
  button: MouseButton;
  shiftKey: boolean;
  ctrlKey: boolean;
  altKey: boolean;
  type: ClickType;
}

export enum MouseButton {
  LEFT = 0,
  MIDDLE = 1,
  RIGHT = 2,
}

export enum ClickType {
  CLICK = 'click',
  DOUBLE_CLICK = 'double_click',
  DRAG_START = 'drag_start',
  DRAG = 'drag',
  DRAG_END = 'drag_end',
}

export interface MinimapHoverEvent {
  position: Point;
  worldPosition: Point;
  hoveredElement: HoveredElement | null;
  timestamp: number;
}

export interface HoveredElement {
  type: MarkerType;
  id: string;
  data: UnitData | BuildingData | ResourceData | PingMarker;
  position: Point;
}

export class RadarSystem {
  private config: RadarConfig;
  private settings: MinimapSettings;
  private state: RadarState;
  private canvas: HTMLCanvasElement | null = null;
  private ctx: CanvasRenderingContext2D | null = null;
  private fullMapCanvas: HTMLCanvasElement | null = null;
  private fullMapCtx: CanvasRenderingContext2D | null = null;
  private minimapRenderer: MinimapRenderer;
  private radarScanner: RadarScanner;
  private mapRenderer: MapRenderer;
  private pingSystem: PingSystem;
  private alertRenderer: AlertRenderer;
  private unitMarker: UnitMarker;
  private terrainFeatures: TerrainFeatures;
  private fogOverlay: FogOverlay;
  private visionRange: VisionRange;
  private resourceIndicator: ResourceIndicator;
  private buildingIndicator: BuildingIndicator;
  private selectionBox: SelectionBox;
  private minimapClickHandler: MinimapClickHandler;
  private terrain: TerrainData | null = null;
  private units: Map<string, UnitData> = new Map();
  private buildings: Map<string, BuildingData> = new Map();
  private resources: Map<string, ResourceData> = new Map();
  private pings: Map<string, PingMarker> = new Map();
  private alerts: Map<string, AlertData> = new Map();
  private vision: VisionData | null = null;
  private fog: FogData | null = null;
  private currentPlayer: Player | null = null;
  private lastUpdate: number = 0;
  private animationFrame: number | null = null;
  private isRunning: boolean = false;
  private eventListeners: Map<string, Function[]> = new Map();
  private cachedMinimap: ImageData | null = null;
  private cachedFullMap: ImageData | null = null;
  private dirty: boolean = true;

  constructor(config: Partial<RadarConfig> = {}, settings: Partial<MinimapSettings> = {}) {
    this.config = this.createDefaultConfig(config);
    this.settings = this.createDefaultSettings(settings);
    this.state = this.createInitialState();
    this.selectionBox = { start: { x: 0, y: 0 }, end: { x: 0, y: 0 }, isActive: false, units: [], buildings: [], isValid: true, contains: [] };
    
    this.minimapRenderer = new MinimapRenderer(this);
    this.radarScanner = new RadarScanner(this);
    this.mapRenderer = new MapRenderer(this);
    this.pingSystem = new PingSystem(this);
    this.alertRenderer = new AlertRenderer(this);
    this.unitMarker = new UnitMarker(this);
    this.terrainFeatures = new TerrainFeatures(this);
    this.fogOverlay = new FogOverlay(this);
    this.visionRange = new VisionRange(this);
    this.resourceIndicator = new ResourceIndicator(this);
    this.buildingIndicator = new BuildingIndicator(this);
    this.selectionBox = new SelectionBox(this);
    this.minimapClickHandler = new MinimapClickHandler(this);
  }

  private createDefaultConfig(config: Partial<RadarConfig>): RadarConfig {
    return {
      width: 300,
      height: 300,
      mapWidth: 4096,
      mapHeight: 4096,
      updateInterval: 100,
      scanRadius: 500,
      pingDuration: 5000,
      alertDuration: 3000,
      fogEnabled: true,
      showResources: true,
      showBuildings: true,
      showUnits: true,
      showTerrain: true,
      showVision: true,
      showAlerts: true,
      zoomLevels: [0.5, 1, 2, 4],
      defaultZoom: 1,
      minZoom: 0.25,
      maxZoom: 8,
      colors: {
        ally: { r: 0, g: 255, b: 0, a: 255 },
        enemy: { r: 255, g: 0, b: 0, a: 255 },
        neutral: { r: 128, g: 128, b: 128, a: 255 },
        unknown: { r: 50, g: 50, b: 50, a: 255 },
        friendly: { r: 0, g: 200, b: 0, a: 255 },
        hostile: { r: 200, g: 0, b: 0, a: 255 },
        resource: { r: 255, g: 215, b: 0, a: 255 },
        building: { r: 139, g: 69, b: 19, a: 255 },
        selection: { r: 255, g: 255, b: 0, a: 255 },
        ping: { r: 255, g: 255, b: 255, a: 255 },
        alert: { r: 255, g: 0, b: 0, a: 255 },
        vision: { r: 255, g: 255, b: 255, a: 100 },
        fog: { r: 0, g: 0, b: 0, a: 200 },
        terrain: { r: 34, g: 139, b: 34, a: 255 },
        grid: { r: 255, g: 255, b: 255, a: 50 },
        background: { r: 0, g: 0, b: 0, a: 255 },
        border: { r: 100, g: 100, b: 100, a: 255 },
        text: { r: 255, g: 255, b: 255, a: 255 },
        healthBar: { r: 0, g: 255, b: 0, a: 255 },
        energyBar: { r: 0, g: 191, b: 255, a: 255 },
        shieldBar: { r: 65, g: 105, b: 225, a: 255 },
      },
      scanQuality: 1,
      maxPings: 50,
      maxAlerts: 100,
      clickThrottle: 50,
      ...config,
    };
  }

  private createDefaultSettings(settings: Partial<MinimapSettings>): MinimapSettings {
    return {
      position: { x: 0, y: 0 },
      size: { x: 0, y: 0, width: 300, height: 300 },
      borderWidth: 2,
      borderColor: { r: 100, g: 100, b: 100, a: 255 },
      backgroundColor: { r: 20, g: 20, b: 20, a: 255 },
      opacity: 1,
      visibility: 'always',
      lockPosition: false,
      showMinimap: true,
      showFullMap: false,
      autoFollow: false,
      showCoordinates: false,
      showGrid: false,
      gridSize: 64,
      enableClicks: true,
      enableDrag: true,
      smoothScroll: true,
      scrollSpeed: 1,
      showTooltips: true,
      tooltipDelay: 200,
      ...settings,
    };
  }

  private createInitialState(): RadarState {
    return {
      visible: true,
      zoom: this.config.defaultZoom,
      panOffset: { x: 0, y: 0 },
      currentView: { x: 0, y: 0, width: this.config.mapWidth, height: this.config.mapHeight },
      isScanning: false,
      lastScanTime: 0,
      scanProgress: 0,
      totalUnits: 0,
      visibleUnits: 0,
      totalBuildings: 0,
      visibleBuildings: 0,
      totalResources: 0,
      visibleResources: 0,
      alerts: 0,
      pings: 0,
      selectionStart: null,
      selectionEnd: null,
      isDragging: false,
      isPanning: false,
      hoveredUnit: null,
      selectedUnits: [],
      focusedPlayer: null,
    };
  }

  public initialize(canvas: HTMLCanvasElement, fullMapCanvas?: HTMLCanvasElement): void {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d');
    
    if (fullMapCanvas) {
      this.fullMapCanvas = fullMapCanvas;
      this.fullMapCtx = fullMapCanvas.getContext('2d');
    }
    
    this.resize();
    this.setupEventListeners();
    this.start();
  }

  private resize(): void {
    if (this.canvas) {
      this.canvas.width = this.config.width;
      this.canvas.height = this.config.height;
      this.settings.size = {
        x: this.settings.position.x,
        y: this.settings.position.y,
        width: this.config.width,
        height: this.config.height,
      };
    }
    
    if (this.fullMapCanvas) {
      this.fullMapCanvas.width = this.config.mapWidth / 4;
      this.fullMapCanvas.height = this.config.mapHeight / 4;
    }
    
    this.dirty = true;
  }

  private setupEventListeners(): void {
    if (!this.canvas) return;
    
    this.canvas.addEventListener('mousedown', (e) => this.handleMouseDown(e));
    this.canvas.addEventListener('mouseup', (e) => this.handleMouseUp(e));
    this.canvas.addEventListener('mousemove', (e) => this.handleMouseMove(e));
    this.canvas.addEventListener('click', (e) => this.handleClick(e));
    this.canvas.addEventListener('dblclick', (e) => this.handleDoubleClick(e));
    this.canvas.addEventListener('wheel', (e) => this.handleWheel(e));
    this.canvas.addEventListener('contextmenu', (e) => {
      e.preventDefault();
      this.handleRightClick(e);
    });
  }

  private handleMouseDown(e: MouseEvent): void {
    const pos = this.getCanvasPosition(e);
    this.state.selectionStart = pos;
    this.state.isDragging = true;
    
    this.emit('mousedown', {
      position: pos,
      worldPosition: this.minimapToWorld(pos),
      button: e.button,
      shiftKey: e.shiftKey,
      ctrlKey: e.ctrlKey,
      altKey: e.altKey,
    });
  }

  private handleMouseUp(e: MouseEvent): void {
    if (this.state.isDragging && this.state.selectionStart) {
      const pos = this.getCanvasPosition(e);
      this.state.selectionEnd = pos;
      this.updateSelectionBox();
    }
    
    this.state.isDragging = false;
    this.state.selectionStart = null;
    this.state.selectionEnd = null;
    
    this.emit('mouseup', {
      position: this.getCanvasPosition(e),
      worldPosition: this.minimapToWorld(this.getCanvasPosition(e)),
      button: e.button,
    });
  }

  private handleMouseMove(e: MouseEvent): void {
    const pos = this.getCanvasPosition(e);
    const worldPos = this.minimapToWorld(pos);
    
    if (this.state.isDragging && this.settings.enableDrag) {
      this.state.panOffset = {
        x: this.state.panOffset.x + e.movementX,
        y: this.state.panOffset.y + e.movementY,
      };
      this.state.isPanning = true;
      this.dirty = true;
    }
    
    const hoveredElement = this.getHoveredElement(pos);
    
    this.emit('mousemove', {
      position: pos,
      worldPosition: worldPos,
      hoveredElement,
      timestamp: Date.now(),
    });
  }

  private handleClick(e: MouseEvent): void {
    const pos = this.getCanvasPosition(e);
    const worldPos = this.minimapToWorld(pos);
    
    this.minimapClickHandler.handleClick({
      position: pos,
      worldPosition: worldPos,
      button: e.button as MouseButton,
      shiftKey: e.shiftKey,
      ctrlKey: e.ctrlKey,
      altKey: e.altKey,
      type: ClickType.CLICK,
    });
    
    this.emit('click', {
      position: pos,
      worldPosition: worldPos,
      button: e.button,
      shiftKey: e.shiftKey,
      ctrlKey: e.ctrlKey,
      altKey: e.altKey,
    });
  }

  private handleDoubleClick(e: MouseEvent): void {
    const pos = this.getCanvasPosition(e);
    const worldPos = this.minimapToWorld(pos);
    
    this.minimapClickHandler.handleClick({
      position: pos,
      worldPosition: worldPos,
      button: e.button as MouseButton,
      shiftKey: e.shiftKey,
      ctrlKey: e.ctrlKey,
      altKey: e.altKey,
      type: ClickType.DOUBLE_CLICK,
    });
    
    this.emit('doubleclick', {
      position: pos,
      worldPosition: worldPos,
      button: e.button,
    });
  }

  private handleRightClick(e: MouseEvent): void {
    const pos = this.getCanvasPosition(e);
    const worldPos = this.minimapToWorld(pos);
    
    this.pingSystem.createPing({
      id: this.generateId(),
      type: PingType.ATTENTION,
      position: worldPos,
      playerId: this.currentPlayer?.id || 'unknown',
      playerName: this.currentPlayer?.name || 'Unknown',
      message: '',
      timestamp: Date.now(),
      duration: this.config.pingDuration,
      color: this.config.colors.ally,
      size: 10,
      fadeOut: true,
      isExpired: false,
      icon: '!',
    });
    
    this.emit('rightclick', {
      position: pos,
      worldPosition: worldPos,
      button: e.button,
    });
  }

  private handleWheel(e: WheelEvent): void {
    e.preventDefault();
    const delta = e.deltaY > 0 ? -0.1 : 0.1;
    this.zoom(delta);
  }

  private getCanvasPosition(e: MouseEvent): Point {
    if (!this.canvas) return { x: 0, y: 0 };
    const rect = this.canvas.getBoundingClientRect();
    return {
      x: e.clientX - rect.left,
      y: e.clientY - rect.top,
    };
  }

  private minimapToWorld(pos: Point): Point {
    const scaleX = this.config.mapWidth / this.config.width;
    const scaleY = this.config.mapHeight / this.config.height;
    return {
      x: pos.x * scaleX - this.state.panOffset.x,
      y: pos.y * scaleY - this.state.panOffset.y,
    };
  }

  private worldToMinimap(worldPos: Point): Point {
    const scaleX = this.config.width / this.config.mapWidth;
    const scaleY = this.config.height / this.config.mapHeight;
    return {
      x: (worldPos.x + this.state.panOffset.x) * scaleX,
      y: (worldPos.y + this.state.panOffset.y) * scaleY,
    };
  }

  private getHoveredElement(pos: Point): HoveredElement | null {
    const worldPos = this.minimapToWorld(pos);
    
    for (const [id, unit] of this.units) {
      if (this.isPointInCircle(worldPos, unit.position, unit.radius)) {
        return { type: MarkerType.UNIT, id, data: unit, position: unit.position };
      }
    }
    
    for (const [id, building] of this.buildings) {
      const rect = {
        x: building.position.x - building.size.x / 2,
        y: building.position.y - building.size.y / 2,
        width: building.size.x,
        height: building.size.y,
      };
      if (this.isPointInRect(worldPos, rect)) {
        return { type: MarkerType.BUILDING, id, data: building, position: building.position };
      }
    }
    
    for (const [id, resource] of this.resources) {
      if (this.isPointInCircle(worldPos, resource.position, 20)) {
        return { type: MarkerType.RESOURCE, id, data: resource, position: resource.position };
      }
    }
    
    for (const [id, ping] of this.pings) {
      if (this.isPointInCircle(worldPos, ping.position, ping.size)) {
        return { type: MarkerType.PING, id, data: ping, position: ping.position };
      }
    }
    
    return null;
  }

  private isPointInCircle(point: Point, center: Point, radius: number): boolean {
    const dx = point.x - center.x;
    const dy = point.y - center.y;
    return dx * dx + dy * dy <= radius * radius;
  }

  private isPointInRect(point: Point, rect: Rectangle): boolean {
    return (
      point.x >= rect.x &&
      point.x <= rect.x + rect.width &&
      point.y >= rect.y &&
      point.y <= rect.y + rect.height
    );
  }

  private updateSelectionBox(): void {
    if (!this.state.selectionStart || !this.state.selectionEnd) return;
    
    const start = this.minimapToWorld(this.state.selectionStart);
    const end = this.minimapToWorld(this.state.selectionEnd);
    
    const minX = Math.min(start.x, end.x);
    const maxX = Math.max(start.x, end.x);
    const minY = Math.min(start.y, end.y);
    const maxY = Math.max(start.y, end.y);
    
    this.selectionBox.start = start;
    this.selectionBox.end = end;
    this.selectionBox.isActive = true;
    this.selectionBox.isValid = true;
    this.selectionBox.contains = [];
    
    const selectedUnits: Unit[] = [];
    for (const [id, unit] of this.units) {
      if (
        unit.position.x >= minX &&
        unit.position.x <= maxX &&
        unit.position.y >= minY &&
        unit.position.y <= maxY
      ) {
        selectedUnits.push(unit as unknown as Unit);
        this.selectionBox.contains.push(unit.position);
      }
    }
    
    this.selectionBox.units = selectedUnits;
    this.dirty = true;
    
    this.emit('selectionchange', {
      units: selectedUnits,
      bounds: { x: minX, y: minY, width: maxX - minX, height: maxY - minY },
    });
  }

  public zoom(delta: number): void {
    const oldZoom = this.state.zoom;
    const newZoom = Math.max(
      this.config.minZoom,
      Math.min(this.config.maxZoom, this.state.zoom + delta)
    );
    
    if (newZoom !== oldZoom) {
      this.state.zoom = newZoom;
      this.updateCurrentView();
      this.dirty = true;
      
      this.emit('zoomchange', {
        oldZoom,
        newZoom,
        center: this.state.currentView,
      });
    }
  }

  public setZoom(zoom: number): void {
    this.zoom(zoom - this.state.zoom);
  }

  public pan(deltaX: number, deltaY: number): void {
    this.state.panOffset.x += deltaX;
    this.state.panOffset.y += deltaY;
    this.updateCurrentView();
    this.dirty = true;
    
    this.emit('pan', {
      offset: { ...this.state.panOffset },
      view: { ...this.state.currentView },
    });
  }

  public centerOn(position: Point): void {
    const viewCenterX = this.config.mapWidth / 2;
    const viewCenterY = this.config.mapHeight / 2;
    
    this.state.panOffset.x = viewCenterX - position.x;
    this.state.panOffset.y = viewCenterY - position.y;
    
    this.updateCurrentView();
    this.dirty = true;
    
    this.emit('center', { position: { ...position } });
  }

  private updateCurrentView(): void {
    const viewWidth = this.config.mapWidth / this.state.zoom;
    const viewHeight = this.config.mapHeight / this.state.zoom;
    
    this.state.currentView = {
      x: -this.state.panOffset.x,
      y: -this.state.panOffset.y,
      width: viewWidth,
      height: viewHeight,
    };
  }

  public start(): void {
    if (this.isRunning) return;
    
    this.isRunning = true;
    this.lastUpdate = Date.now();
    this.radarScanner.start();
    this.renderLoop();
  }

  public stop(): void {
    this.isRunning = false;
    this.radarScanner.stop();
    
    if (this.animationFrame !== null) {
      cancelAnimationFrame(this.animationFrame);
      this.animationFrame = null;
    }
  }

  private renderLoop(): void {
    if (!this.isRunning) return;
    
    const now = Date.now();
    const deltaTime = now - this.lastUpdate;
    
    if (deltaTime >= this.config.updateInterval || this.dirty) {
      this.update();
      this.render();
      this.lastUpdate = now;
      this.dirty = false;
    }
    
    this.animationFrame = requestAnimationFrame(() => this.renderLoop());
  }

  public update(): void {
    this.updateTerrain();
    this.updateUnits();
    this.updateBuildings();
    this.updateResources();
    this.updateVision();
    this.updateFog();
    this.updatePings();
    this.updateAlerts();
    this.radarScanner.update();
    
    this.state.totalUnits = this.units.size;
    this.state.visibleUnits = this.countVisibleUnits();
    this.state.totalBuildings = this.buildings.size;
    this.state.visibleBuildings = this.countVisibleBuildings();
    this.state.totalResources = this.resources.size;
    this.state.visibleResources = this.countVisibleResources();
    this.state.alerts = this.alerts.size;
    this.state.pings = this.pings.size;
    
    this.emit('update', {
      state: this.getState(),
      timestamp: Date.now(),
    });
  }

  private updateTerrain(): void {
    if (!this.terrain) return;
  }

  private updateUnits(): void {
    for (const [id, unit] of this.units) {
      if (unit.isMoving) {
        unit.position.x += unit.velocity.x;
        unit.position.y += unit.velocity.y;
      }
    }
  }

  private updateBuildings(): void {
  }

  private updateResources(): void {
    const now = Date.now();
    for (const [id, resource] of this.resources) {
      if (!resource.isDepleted && resource.regenRate > 0) {
        const timeSinceRegen = now - resource.lastRegenTime;
        if (timeSinceRegen >= 1000) {
          const regenAmount = (timeSinceRegen / 1000) * resource.regenRate;
          resource.amount = Math.min(resource.maxAmount, resource.amount + regenAmount);
          resource.lastRegenTime = now;
        }
      }
      
      if (resource.amount <= 0 && !resource.isDepleted) {
        resource.isDepleted = true;
        this.createAlert({
          id: this.generateId(),
          type: AlertType.RESOURCE_DEPLETED,
          position: resource.position,
          message: `Resource at (${resource.position.x}, ${resource.position.y}) depleted`,
          timestamp: now,
          duration: this.config.alertDuration,
          priority: AlertPriority.MEDIUM,
          acknowledged: false,
          source: resource.id,
          target: null,
          alliance: Alliance.NEUTRAL,
        });
      }
    }
  }

  private updateVision(): void {
  }

  private updateFog(): void {
    if (!this.fog || !this.config.fogEnabled) return;
    
    const now = Date.now();
    if (now - this.fog.lastUpdate > this.config.updateInterval) {
      this.recalculateFog();
      this.fog.lastUpdate = now;
    }
  }

  private recalculateFog(): void {
    if (!this.fog) return;
    
    for (let i = 0; i < this.fog.visible.length; i++) {
      this.fog.visible[i] = 0;
    }
    
    if (this.vision) {
      for (const unitVision of this.vision.unitVision) {
        this.markAreaVisible(unitVision.center, unitVision.radius);
      }
      
      for (const buildingVision of this.vision.buildingVision) {
        if (buildingVision.isActive) {
          this.markAreaVisible(buildingVision.center, buildingVision.radius);
        }
      }
    }
  }

  private markAreaVisible(center: Point, radius: number): void {
    if (!this.fog) return;
    
    const centerX = Math.floor(center.x / this.fog.resolution);
    const centerY = Math.floor(center.y / this.fog.resolution);
    const radiusCells = Math.floor(radius / this.fog.resolution);
    
    for (let dy = -radiusCells; dy <= radiusCells; dy++) {
      for (let dx = -radiusCells; dx <= radiusCells; dx++) {
        const x = centerX + dx;
        const y = centerY + dy;
        
        if (x >= 0 && x < this.fog.width && y >= 0 && y < this.fog.height) {
          if (dx * dx + dy * dy <= radiusCells * radiusCells) {
            this.fog.visible[y * this.fog.width + x] = 1;
            this.fog.explored[y * this.fog.width + x] = 1;
          }
        }
      }
    }
  }

  private updatePings(): void {
    const now = Date.now();
    for (const [id, ping] of this.pings) {
      const age = now - ping.timestamp;
      
      if (age >= ping.duration) {
        ping.isExpired = true;
        ping.fadeOut = true;
      }
      
      if (age >= ping.duration + 1000) {
        this.pings.delete(id);
      }
    }
    this.dirty = true;
  }

  private updateAlerts(): void {
    const now = Date.now();
    for (const [id, alert] of this.alerts) {
      const age = now - alert.timestamp;
      
      if (age >= alert.duration) {
        this.alerts.delete(id);
      }
    }
    this.dirty = true;
  }

  private countVisibleUnits(): number {
    let count = 0;
    for (const [id, unit] of this.units) {
      if (unit.isVisible) count++;
    }
    return count;
  }

  private countVisibleBuildings(): number {
    let count = 0;
    for (const [id, building] of this.buildings) {
      if (building.isVisible) count++;
    }
    return count;
  }

  private countVisibleResources(): number {
    let count = 0;
    for (const [id, resource] of this.resources) {
      if (resource.isVisible) count++;
    }
    return count;
  }

  public render(): void {
    if (!this.ctx || !this.canvas) return;
    
    this.minimapRenderer.render();
    
    if (this.settings.showFullMap && this.fullMapCtx) {
      this.mapRenderer.render();
    }
  }

  public getState(): RadarState {
    return { ...this.state };
  }

  public getConfig(): RadarConfig {
    return { ...this.config };
  }

  public getSettings(): MinimapSettings {
    return { ...this.settings };
  }

  public setCurrentPlayer(player: Player): void {
    this.currentPlayer = player;
    this.dirty = true;
  }

  public getCurrentPlayer(): Player | null {
    return this.currentPlayer;
  }

  public addUnit(unit: UnitData): void {
    this.units.set(unit.id, unit);
    this.dirty = true;
  }

  public removeUnit(id: string): void {
    this.units.delete(id);
    this.dirty = true;
  }

  public getUnit(id: string): UnitData | undefined {
    return this.units.get(id);
  }

  public getAllUnits(): UnitData[] {
    return Array.from(this.units.values());
  }

  public addBuilding(building: BuildingData): void {
    this.buildings.set(building.id, building);
    this.dirty = true;
  }

  public removeBuilding(id: string): void {
    this.buildings.delete(id);
    this.dirty = true;
  }

  public getBuilding(id: string): BuildingData | undefined {
    return this.buildings.get(id);
  }

  public getAllBuildings(): BuildingData[] {
    return Array.from(this.buildings.values());
  }

  public addResource(resource: ResourceData): void {
    this.resources.set(resource.id, resource);
    this.dirty = true;
  }

  public removeResource(id: string): void {
    this.resources.delete(id);
    this.dirty = true;
  }

  public getResource(id: string): ResourceData | undefined {
    return this.resources.get(id);
  }

  public getAllResources(): ResourceData[] {
    return Array.from(this.resources.values());
  }

  public createPing(ping: PingMarker): void {
    if (this.pings.size >= this.config.maxPings) {
      const oldest = Array.from(this.pings.values()).sort(
        (a, b) => a.timestamp - b.timestamp
      )[0];
      if (oldest) {
        this.pings.delete(oldest.id);
      }
    }
    
    this.pings.set(ping.id, ping);
    this.dirty = true;
    
    this.emit('pingcreated', { ping });
  }

  public removePing(id: string): void {
    this.pings.delete(id);
    this.dirty = true;
  }

  public getPing(id: string): PingMarker | undefined {
    return this.pings.get(id);
  }

  public getAllPings(): PingMarker[] {
    return Array.from(this.pings.values());
  }

  public createAlert(alert: AlertData): void {
    if (this.alerts.size >= this.config.maxAlerts) {
      const oldest = Array.from(this.alerts.values()).sort(
        (a, b) => a.timestamp - b.timestamp
      )[0];
      if (oldest) {
        this.alerts.delete(oldest.id);
      }
    }
    
    this.alerts.set(alert.id, alert);
    this.dirty = true;
    
    this.emit('alertcreated', { alert });
  }

  public removeAlert(id: string): void {
    this.alerts.delete(id);
    this.dirty = true;
  }

  public getAlert(id: string): AlertData | undefined {
    return this.alerts.get(id);
  }

  public getAllAlerts(): AlertData[] {
    return Array.from(this.alerts.values());
  }

  public acknowledgeAlert(id: string): void {
    const alert = this.alerts.get(id);
    if (alert) {
      alert.acknowledged = true;
      this.dirty = true;
    }
  }

  public setTerrain(terrain: TerrainData): void {
    this.terrain = terrain;
    this.dirty = true;
  }

  public getTerrain(): TerrainData | null {
    return this.terrain;
  }

  public setVision(vision: VisionData): void {
    this.vision = vision;
    this.dirty = true;
  }

  public getVision(): VisionData | null {
    return this.vision;
  }

  public setFog(fog: FogData): void {
    this.fog = fog;
    this.dirty = true;
  }

  public getFog(): FogData | null {
    return this.fog;
  }

  public isPointVisible(position: Point): boolean {
    if (!this.fog || !this.config.fogEnabled) return true;
    
    const x = Math.floor(position.x / this.fog.resolution);
    const y = Math.floor(position.y / this.fog.resolution);
    
    if (x < 0 || x >= this.fog.width || y < 0 || y >= this.fog.height) {
      return false;
    }
    
    return this.fog.visible[y * this.fog.width + x] === 1;
  }

  public isPointExplored(position: Point): boolean {
    if (!this.fog) return true;
    
    const x = Math.floor(position.x / this.fog.resolution);
    const y = Math.floor(position.y / this.fog.resolution);
    
    if (x < 0 || x >= this.fog.width || y < 0 || y >= this.fog.height) {
      return false;
    }
    
    return this.fog.explored[y * this.fog.width + x] === 1;
  }

  public show(): void {
    this.state.visible = true;
    this.settings.showMinimap = true;
    this.dirty = true;
  }

  public hide(): void {
    this.state.visible = false;
    this.settings.showMinimap = false;
    this.dirty = true;
  }

  public toggle(): void {
    if (this.state.visible) {
      this.hide();
    } else {
      this.show();
    }
  }

  public on(event: string, callback: Function): void {
    if (!this.eventListeners.has(event)) {
      this.eventListeners.set(event, []);
    }
    this.eventListeners.get(event)!.push(callback);
  }

  public off(event: string, callback: Function): void {
    const listeners = this.eventListeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(callback);
      if (index !== -1) {
        listeners.splice(index, 1);
      }
    }
  }

  public emit(event: string, data?: any): void {
    const listeners = this.eventListeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  public destroy(): void {
    this.stop();
    this.units.clear();
    this.buildings.clear();
    this.resources.clear();
    this.pings.clear();
    this.alerts.clear();
    this.eventListeners.clear();
    this.canvas = null;
    this.ctx = null;
    this.fullMapCanvas = null;
    this.fullMapCtx = null;
  }
}

export class MinimapRenderer {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const config = this.radar.getConfig();
    const settings = this.radar.getSettings();
    
    ctx.save();
    ctx.globalAlpha = settings.opacity;
    
    this.renderBackground(ctx, config);
    this.renderTerrain(ctx, config);
    this.renderGrid(ctx, config, settings);
    this.renderResources(ctx, config);
    this.renderBuildings(ctx, config);
    this.renderUnits(ctx, config);
    this.renderVision(ctx, config);
    this.renderFog(ctx, config);
    this.renderPings(ctx, config);
    this.renderAlerts(ctx, config);
    this.renderSelectionBox(ctx, config);
    this.renderViewport(ctx, config);
    this.renderBorder(ctx, config, settings);
    this.renderCoordinates(ctx, config, settings);
    
    ctx.restore();
  }
  
  private renderBackground(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    ctx.fillStyle = this.radar.colorToString(config.colors.background);
    ctx.fillRect(0, 0, config.width, config.height);
  }
  
  private renderTerrain(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    if (!config.showTerrain) return;
    
    const terrain = this.radar.getTerrain();
    if (!terrain) return;
    
    const imageData = ctx.createImageData(config.width, config.height);
    const data = imageData.data;
    
    const scaleX = terrain.width / config.width;
    const scaleY = terrain.height / config.height;
    
    for (let y = 0; y < config.height; y++) {
      for (let x = 0; x < config.width; x++) {
        const terrainX = Math.floor(x * scaleX);
        const terrainY = Math.floor(y * scaleY);
        const index = terrainY * terrain.width + terrainX;
        
        const terrainType = terrain.terrainTypes[index];
        let color: Color;
        
        switch (terrainType) {
          case 0: color = { r: 34, g: 139, b: 34, a: 255 }; break;
          case 1: color = { r: 139, g: 119, b: 101, a: 255 }; break;
          case 2: color = { r: 64, g: 164, b: 223, a: 255 }; break;
          case 3: color = { r: 85, g: 85, b: 85, a: 255 }; break;
          default: color = { r: 34, g: 139, b: 34, a: 255 };
        }
        
        const dataIndex = (y * config.width + x) * 4;
        data[dataIndex] = color.r;
        data[dataIndex + 1] = color.g;
        data[dataIndex + 2] = color.b;
        data[dataIndex + 3] = color.a;
      }
    }
    
    ctx.putImageData(imageData, 0, 0);
  }
  
  private renderGrid(ctx: CanvasRenderingContext2D, config: RadarConfig, settings: MinimapSettings): void {
    if (!settings.showGrid) return;
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.grid);
    ctx.lineWidth = 1;
    
    const cellWidth = settings.gridSize * (config.width / config.mapWidth);
    const cellHeight = settings.gridSize * (config.height / config.mapHeight);
    
    for (let x = 0; x < config.width; x += cellWidth) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, config.height);
      ctx.stroke();
    }
    
    for (let y = 0; y < config.height; y += cellHeight) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(config.width, y);
      ctx.stroke();
    }
  }
  
  private renderResources(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    if (!config.showResources) return;
    
    const resources = this.radar.getAllResources();
    
    for (const resource of resources) {
      if (!resource.isVisible) continue;
      
      const screenPos = this.radar.worldToMinimap(resource.position);
      const size = Math.max(2, resource.capacity / 10);
      
      ctx.fillStyle = this.radar.colorToString(config.colors.resource);
      ctx.beginPath();
      
      switch (resource.type) {
        case ResourceType.GOLD:
        case ResourceType.IRON:
        case ResourceType.CRYSTAL:
          ctx.arc(screenPos.x, screenPos.y, size, 0, Math.PI * 2);
          break;
        case ResourceType.WOOD:
          ctx.fillRect(screenPos.x - size, screenPos.y - size, size * 2, size * 2);
          break;
        case ResourceType.STONE:
          ctx.moveTo(screenPos.x, screenPos.y - size);
          ctx.lineTo(screenPos.x + size, screenPos.y + size);
          ctx.lineTo(screenPos.x - size, screenPos.y + size);
          ctx.closePath();
          break;
        default:
          ctx.arc(screenPos.x, screenPos.y, size, 0, Math.PI * 2);
      }
      
      ctx.fill();
      
      if (resource.isDepleted) {
        ctx.strokeStyle = 'rgba(128, 128, 128, 0.5)';
        ctx.lineWidth = 1;
        ctx.stroke();
      }
    }
  }
  
  private renderBuildings(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    if (!config.showBuildings) return;
    
    const buildings = this.radar.getAllBuildings();
    
    for (const building of buildings) {
      if (!building.isVisible) continue;
      
      const screenPos = this.radar.worldToMinimap(building.position);
      const sizeX = building.size.x * (config.width / config.mapWidth);
      const sizeY = building.size.y * (config.height / config.mapHeight);
      
      let color = config.colors.neutral;
      switch (building.alliance) {
        case Alliance.ALLY: color = config.colors.ally; break;
        case Alliance.ENEMY: color = config.colors.enemy; break;
        case Alliance.NEUTRAL: color = config.colors.neutral; break;
      }
      
      ctx.fillStyle = this.radar.colorToString(color);
      ctx.fillRect(
        screenPos.x - sizeX / 2,
        screenPos.y - sizeY / 2,
        Math.max(2, sizeX),
        Math.max(2, sizeY)
      );
      
      if (building.isSelected) {
        ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
        ctx.lineWidth = 2;
        ctx.strokeRect(
          screenPos.x - sizeX / 2 - 1,
          screenPos.y - sizeY / 2 - 1,
          Math.max(2, sizeX) + 2,
          Math.max(2, sizeY) + 2
        );
      }
      
      if (building.isDamaged && building.health < building.maxHealth) {
        this.renderHealthBar(
          ctx,
          screenPos.x - sizeX / 2,
          screenPos.y + sizeY / 2 + 2,
          sizeX,
          3,
          building.health,
          building.maxHealth
        );
      }
    }
  }
  
  private renderUnits(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    if (!config.showUnits) return;
    
    const units = this.radar.getAllUnits();
    
    for (const unit of units) {
      if (!unit.isVisible || !unit.isAlive) continue;
      
      const screenPos = this.radar.worldToMinimap(unit.position);
      let size = Math.max(2, unit.size * (config.width / config.mapWidth));
      
      if (size < 3) size = 3;
      
      let color = config.colors.neutral;
      switch (unit.alliance) {
        case Alliance.ALLY: color = config.colors.ally; break;
        case Alliance.ENEMY: color = config.colors.enemy; break;
        case Alliance.NEUTRAL: color = config.colors.neutral; break;
      }
      
      if (unit.group === UnitGroup.AIR) {
        ctx.strokeStyle = this.radar.colorToString(color);
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(screenPos.x, screenPos.y - size);
        ctx.lineTo(screenPos.x + size, screenPos.y + size);
        ctx.lineTo(screenPos.x - size, screenPos.y + size);
        ctx.closePath();
        ctx.stroke();
      } else if (unit.group === UnitGroup.NAVAL) {
        ctx.fillStyle = this.radar.colorToString(color);
        ctx.beginPath();
        ctx.ellipse(screenPos.x, screenPos.y, size, size * 0.6, 0, 0, Math.PI * 2);
        ctx.fill();
      } else {
        ctx.fillStyle = this.radar.colorToString(color);
        ctx.beginPath();
        ctx.arc(screenPos.x, screenPos.y, size, 0, Math.PI * 2);
        ctx.fill();
      }
      
      if (unit.isSelected) {
        ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.arc(screenPos.x, screenPos.y, size + 2, 0, Math.PI * 2);
        ctx.stroke();
      }
      
      if (unit.isMoving) {
        ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
        ctx.lineWidth = 1;
        ctx.setLineDash([2, 2]);
        ctx.beginPath();
        ctx.moveTo(screenPos.x, screenPos.y);
        ctx.lineTo(
          screenPos.x + unit.velocity.x * 10,
          screenPos.y + unit.velocity.y * 10
        );
        ctx.stroke();
        ctx.setLineDash([]);
      }
      
      if (unit.isAttacking) {
        ctx.strokeStyle = this.radar.colorToString(config.colors.alert);
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.arc(screenPos.x, screenPos.y, size + 4, 0, Math.PI * 2);
        ctx.stroke();
      }
      
      if (unit.health < unit.maxHealth) {
        this.renderHealthBar(
          ctx,
          screenPos.x - size,
          screenPos.y - size - 3,
          size * 2,
          2,
          unit.health,
          unit.maxHealth
        );
      }
    }
  }
  
  private renderHealthBar(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    width: number,
    height: number,
    current: number,
    max: number
  ): void {
    const config = this.radar.getConfig();
    const healthPercent = current / max;
    
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(x, y, width, height);
    
    let healthColor = config.colors.healthBar;
    if (healthPercent < 0.5) {
      healthColor = { r: 255, g: 165, b: 0, a: 255 };
    }
    if (healthPercent < 0.25) {
      healthColor = { r: 255, g: 0, b: 0, a: 255 };
    }
    
    ctx.fillStyle = this.radar.colorToString(healthColor);
    ctx.fillRect(x, y, width * healthPercent, height);
    
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
    ctx.lineWidth = 1;
    ctx.strokeRect(x, y, width, height);
  }
  
  private renderVision(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    if (!config.showVision) return;
    
    const vision = this.radar.getVision();
    if (!vision) return;
    
    ctx.fillStyle = this.radar.colorToString(config.colors.vision);
    
    for (const unitVision of vision.unitVision) {
      const screenPos = this.radar.worldToMinimap(unitVision.center);
      const screenRadius = unitVision.radius * (config.width / config.mapWidth);
      
      ctx.beginPath();
      ctx.arc(screenPos.x, screenPos.y, screenRadius, 0, Math.PI * 2);
      ctx.fill();
    }
    
    for (const buildingVision of vision.buildingVision) {
      if (!buildingVision.isActive) continue;
      
      const screenPos = this.radar.worldToMinimap(buildingVision.center);
      const screenRadius = buildingVision.radius * (config.width / config.mapWidth);
      
      ctx.beginPath();
      ctx.arc(screenPos.x, screenPos.y, screenRadius, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  
  private renderFog(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    if (!config.fogEnabled) return;
    
    const fog = this.radar.getFog();
    if (!fog) return;
    
    ctx.fillStyle = this.radar.colorToString(config.colors.fog);
    
    const scaleX = fog.width / config.width;
    const scaleY = fog.height / config.height;
    
    for (let y = 0; y < config.height; y++) {
      for (let x = 0; x < config.width; x++) {
        const fogX = Math.floor(x * scaleX);
        const fogY = Math.floor(y * scaleY);
        const index = fogY * fog.width + fogX;
        
        if (fog.visible[index] === 0) {
          ctx.fillRect(x, y, 1, 1);
        } else if (fog.explored[index] === 0) {
          ctx.globalAlpha = 0.5;
          ctx.fillRect(x, y, 1, 1);
          ctx.globalAlpha = 1;
        }
      }
    }
  }
  
  private renderPings(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    const pings = this.radar.getAllPings();
    
    for (const ping of pings) {
      if (ping.isExpired) continue;
      
      const screenPos = this.radar.worldToMinimap(ping.position);
      const age = Date.now() - ping.timestamp;
      const lifePercent = 1 - age / ping.duration;
      
      let alpha = lifePercent;
      if (ping.fadeOut) {
        alpha *= lifePercent;
      }
      
      ctx.globalAlpha = alpha;
      ctx.fillStyle = this.radar.colorToString(ping.color);
      ctx.font = `${ping.size}px Arial`;
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText(ping.icon, screenPos.x, screenPos.y);
      
      if (ping.message) {
        ctx.font = '10px Arial';
        ctx.fillText(ping.message, screenPos.x, screenPos.y + ping.size + 8);
      }
      
      ctx.globalAlpha = 1;
    }
  }
  
  private renderAlerts(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    if (!config.showAlerts) return;
    
    const alerts = this.radar.getAllAlerts();
    
    for (const alert of alerts) {
      if (alert.acknowledged) continue;
      
      const screenPos = this.radar.worldToMinimap(alert.position);
      const age = Date.now() - alert.timestamp;
      const lifePercent = 1 - age / alert.duration;
      
      ctx.globalAlpha = lifePercent;
      ctx.fillStyle = this.radar.colorToString(config.colors.alert);
      ctx.font = 'bold 12px Arial';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      
      let icon = '!';
      switch (alert.type) {
        case AlertType.UNIT_ATTACKED: icon = '⚔'; break;
        case AlertType.BUILDING_ATTACKED: icon = '🏠'; break;
        case AlertType.UNIT_DESTROYED: icon = '💀'; break;
        case AlertType.BUILDING_DESTROYED: icon = '🔥'; break;
        case AlertType.ENEMY_SPOTTED: icon = '👁'; break;
        case AlertType.AREA_UNDER_ATTACK: icon = '🚨'; break;
      }
      
      ctx.fillText(icon, screenPos.x, screenPos.y);
      ctx.globalAlpha = 1;
    }
  }
  
  private renderSelectionBox(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
  }
  
  private renderViewport(ctx: CanvasRenderingContext2D, config: RadarConfig): void {
    const state = this.radar.getState();
    
    const topLeft = this.radar.worldToMinimap({
      x: state.currentView.x,
      y: state.currentView.y,
    });
    const bottomRight = this.radar.worldToMinimap({
      x: state.currentView.x + state.currentView.width,
      y: state.currentView.y + state.currentView.height,
    });
    
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.8)';
    ctx.lineWidth = 2;
    ctx.strokeRect(
      topLeft.x,
      topLeft.y,
      bottomRight.x - topLeft.x,
      bottomRight.y - topLeft.y
    );
    
    ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
    ctx.fillRect(
      topLeft.x,
      topLeft.y,
      bottomRight.x - topLeft.x,
      bottomRight.y - topLeft.y
    );
  }
  
  private renderBorder(ctx: CanvasRenderingContext2D, config: RadarConfig, settings: MinimapSettings): void {
    ctx.strokeStyle = this.radar.colorToString(settings.borderColor);
    ctx.lineWidth = settings.borderWidth;
    ctx.strokeRect(0, 0, config.width, config.height);
  }
  
  private renderCoordinates(ctx: CanvasRenderingContext2D, config: RadarConfig, settings: MinimapSettings): void {
    if (!settings.showCoordinates) return;
    
    const state = this.radar.getState();
    
    ctx.fillStyle = this.radar.colorToString(config.colors.text);
    ctx.font = '10px Arial';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'top';
    
    const coordX = Math.floor(state.currentView.x / 100) * 100;
    const coordY = Math.floor(state.currentView.y / 100) * 100;
    
    ctx.fillText(`${coordX}, ${coordY}`, 5, 5);
  }
  
  public colorToString(color: Color): string {
    return `rgba(${color.r}, ${color.g}, ${color.b}, ${color.a / 255})`;
  }
}

export class RadarScanner {
  private radar: RadarSystem;
  private scanInterval: number | null = null;
  private isScanning: boolean = false;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public start(): void {
    if (this.scanInterval) return;
    
    const config = this.radar.getConfig();
    this.scanInterval = window.setInterval(() => {
      this.scan();
    }, config.updateInterval);
    
    this.isScanning = true;
  }
  
  public stop(): void {
    if (this.scanInterval) {
      clearInterval(this.scanInterval);
      this.scanInterval = null;
    }
    
    this.isScanning = false;
  }
  
  public scan(): void {
    const config = this.radar.getConfig();
    const state = this.radar.getState();
    
    state.isScanning = true;
    state.lastScanTime = Date.now();
    
    this.scanUnits();
    this.scanBuildings();
    this.scanResources();
    this.updateVisibility();
    
    state.scanProgress = 1;
    state.isScanning = false;
    
    this.radar.markDirty();
  }
  
  private scanUnits(): void {
  }
  
  private scanBuildings(): void {
  }
  
  private scanResources(): void {
  }
  
  private updateVisibility(): void {
    const units = this.radar.getAllUnits();
    
    for (const unit of units) {
      unit.isVisible = this.radar.isPointVisible(unit.position);
    }
    
    const buildings = this.radar.getAllBuildings();
    
    for (const building of buildings) {
      building.isVisible = this.radar.isPointVisible(building.position);
    }
    
    const resources = this.radar.getAllResources();
    
    for (const resource of resources) {
      resource.isVisible = this.radar.isPointVisible(resource.position);
    }
  }
  
  public update(): void {
  }
  
  public isActive(): boolean {
    return this.isScanning;
  }
}

export class MapRenderer {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getFullMapContext();
    const canvas = this.radar.getFullMapCanvas();
    if (!ctx || !canvas) return;
    
    this.renderTerrain(ctx, canvas.width, canvas.height);
    this.renderResources(ctx);
    this.renderBuildings(ctx);
    this.renderUnits(ctx);
    this.renderVision(ctx);
    this.renderPings(ctx);
    this.renderViewport(ctx);
  }
  
  private renderTerrain(ctx: CanvasRenderingContext2D, width: number, height: number): void {
    const terrain = this.radar.getTerrain();
    if (!terrain) return;
    
    const imageData = ctx.createImageData(width, height);
    const data = imageData.data;
    
    const scaleX = terrain.width / width;
    const scaleY = terrain.height / height;
    
    for (let y = 0; y < height; y++) {
      for (let x = 0; x < width; x++) {
        const terrainX = Math.floor(x * scaleX);
        const terrainY = Math.floor(y * scaleY);
        const index = terrainY * terrain.width + terrainX;
        
        const terrainType = terrain.terrainTypes[index];
        let color: Color;
        
        switch (terrainType) {
          case 0: color = { r: 34, g: 139, b: 34, a: 255 }; break;
          case 1: color = { r: 139, g: 119, b: 101, a: 255 }; break;
          case 2: color = { r: 64, g: 164, b: 223, a: 255 }; break;
          case 3: color = { r: 85, g: 85, b: 85, a: 255 }; break;
          default: color = { r: 34, g: 139, b: 34, a: 255 };
        }
        
        const dataIndex = (y * width + x) * 4;
        data[dataIndex] = color.r;
        data[dataIndex + 1] = color.g;
        data[dataIndex + 2] = color.b;
        data[dataIndex + 3] = color.a;
      }
    }
    
    ctx.putImageData(imageData, 0, 0);
  }
  
  private renderResources(ctx: CanvasRenderingContext2D): void {
    const resources = this.radar.getAllResources();
    const config = this.radar.getConfig();
    
    for (const resource of resources) {
      const screenX = (resource.position.x / config.mapWidth) * ctx.canvas.width;
      const screenY = (resource.position.y / config.mapHeight) * ctx.canvas.height;
      
      ctx.fillStyle = this.radar.colorToString(config.colors.resource);
      ctx.beginPath();
      ctx.arc(screenX, screenY, 2, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  
  private renderBuildings(ctx: CanvasRenderingContext2D): void {
    const buildings = this.radar.getAllBuildings();
    const config = this.radar.getConfig();
    
    for (const building of buildings) {
      if (!building.isVisible) continue;
      
      const screenX = (building.position.x / config.mapWidth) * ctx.canvas.width;
      const screenY = (building.position.y / config.mapHeight) * ctx.canvas.height;
      const sizeX = (building.size.x / config.mapWidth) * ctx.canvas.width;
      const sizeY = (building.size.y / config.mapHeight) * ctx.canvas.height;
      
      let color = config.colors.neutral;
      switch (building.alliance) {
        case Alliance.ALLY: color = config.colors.ally; break;
        case Alliance.ENEMY: color = config.colors.enemy; break;
      }
      
      ctx.fillStyle = this.radar.colorToString(color);
      ctx.fillRect(screenX - sizeX / 2, screenY - sizeY / 2, Math.max(2, sizeX), Math.max(2, sizeY));
    }
  }
  
  private renderUnits(ctx: CanvasRenderingContext2D): void {
    const units = this.radar.getAllUnits();
    const config = this.radar.getConfig();
    
    for (const unit of units) {
      if (!unit.isVisible || !unit.isAlive) continue;
      
      const screenX = (unit.position.x / config.mapWidth) * ctx.canvas.width;
      const screenY = (unit.position.y / config.mapHeight) * ctx.canvas.height;
      
      let color = config.colors.neutral;
      switch (unit.alliance) {
        case Alliance.ALLY: color = config.colors.ally; break;
        case Alliance.ENEMY: color = config.colors.enemy; break;
      }
      
      ctx.fillStyle = this.radar.colorToString(color);
      ctx.beginPath();
      ctx.arc(screenX, screenY, 1, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  
  private renderVision(ctx: CanvasRenderingContext2D): void {
    const vision = this.radar.getVision();
    if (!vision) return;
    
    const config = this.radar.getConfig();
    ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
    
    for (const unitVision of vision.unitVision) {
      const screenX = (unitVision.center.x / config.mapWidth) * ctx.canvas.width;
      const screenY = (unitVision.center.y / config.mapHeight) * ctx.canvas.height;
      const screenRadius = (unitVision.radius / config.mapWidth) * ctx.canvas.width;
      
      ctx.beginPath();
      ctx.arc(screenX, screenY, screenRadius, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  
  private renderPings(ctx: CanvasRenderingContext2D): void {
    const pings = this.radar.getAllPings();
    const config = this.radar.getConfig();
    
    for (const ping of pings) {
      if (ping.isExpired) continue;
      
      const screenX = (ping.position.x / config.mapWidth) * ctx.canvas.width;
      const screenY = (ping.position.y / config.mapHeight) * ctx.canvas.height;
      
      ctx.fillStyle = this.radar.colorToString(ping.color);
      ctx.font = '8px Arial';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText(ping.icon, screenX, screenY);
    }
  }
  
  private renderViewport(ctx: CanvasRenderingContext2D): void {
    const state = this.radar.getState();
    const config = this.radar.getConfig();
    
    const topLeftX = (state.currentView.x / config.mapWidth) * ctx.canvas.width;
    const topLeftY = (state.currentView.y / config.mapHeight) * ctx.canvas.height;
    const width = (state.currentView.width / config.mapWidth) * ctx.canvas.width;
    const height = (state.currentView.height / config.mapHeight) * ctx.canvas.height;
    
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.8)';
    ctx.lineWidth = 2;
    ctx.strokeRect(topLeftX, topLeftY, width, height);
    
    ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
    ctx.fillRect(topLeftX, topLeftY, width, height);
  }
}

export class PingSystem {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public createPing(pingData: Partial<PingMarker> & { position: Point }): PingMarker {
    const config = this.radar.getConfig();
    const currentPlayer = this.radar.getCurrentPlayer();
    
    const ping: PingMarker = {
      id: this.generateId(),
      type: pingData.type || PingType.ATTENTION,
      position: pingData.position,
      playerId: pingData.playerId || currentPlayer?.id || 'unknown',
      playerName: pingData.playerName || currentPlayer?.name || 'Unknown',
      message: pingData.message || '',
      timestamp: Date.now(),
      duration: pingData.duration || config.pingDuration,
      color: pingData.color || config.colors.ping,
      size: pingData.size || 10,
      fadeOut: pingData.fadeOut !== undefined ? pingData.fadeOut : true,
      isExpired: false,
      icon: this.getIconForType(pingData.type || PingType.ATTENTION),
      ...pingData,
    };
    
    this.radar.createPing(ping);
    return ping;
  }
  
  private getIconForType(type: PingType): string {
    switch (type) {
      case PingType.ATTENTION: return '!';
      case PingType.ATTACK: return '⚔';
      case PingType.DEFEND: return '🛡';
      case PingType.MOVE: return '→';
      case PingType.BUILD: return '🔨';
      case PingType.HARVEST: return '⛏';
      case PingType.REPAIR: return '🔧';
      case PingType.DANGER: return '⚠';
      case PingType.REINFORCE: return '➕';
      case PingType.GATHER: return '📦';
      case PingType.EXPLORE: return '🔍';
      case PingType.CANCEL: return '✖';
      default: return '!';
    }
  }
  
  public removePing(id: string): void {
    this.radar.removePing(id);
  }
  
  public getPing(id: string): PingMarker | undefined {
    return this.radar.getPing(id);
  }
  
  public getAllPings(): PingMarker[] {
    return this.radar.getAllPings();
  }
  
  public getPingsByPlayer(playerId: string): PingMarker[] {
    return this.radar.getAllPings().filter(ping => ping.playerId === playerId);
  }
  
  public getPingsInArea(center: Point, radius: number): PingMarker[] {
    return this.radar.getAllPings().filter(ping => {
      const dx = ping.position.x - center.x;
      const dy = ping.position.y - center.y;
      return dx * dx + dy * dy <= radius * radius;
    });
  }
  
  public clearPings(): void {
    const pings = this.radar.getAllPings();
    for (const ping of pings) {
      this.radar.removePing(ping.id);
    }
  }
  
  public clearPlayerPings(playerId: string): void {
    const pings = this.getPingsByPlayer(playerId);
    for (const ping of pings) {
      this.radar.removePing(ping.id);
    }
  }
  
  private generateId(): string {
    return `ping-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}

export class AlertRenderer {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const alerts = this.radar.getAllAlerts();
    
    for (const alert of alerts) {
      if (alert.acknowledged) continue;
      
      const screenPos = this.radar.worldToMinimap(alert.position);
      this.renderAlertIcon(ctx, screenPos, alert);
    }
  }
  
  private renderAlertIcon(ctx: CanvasRenderingContext2D, position: Point, alert: AlertData): void {
    const config = this.radar.getConfig();
    const age = Date.now() - alert.timestamp;
    const lifePercent = 1 - age / alert.duration;
    
    ctx.globalAlpha = lifePercent;
    
    const size = this.getAlertSize(alert.priority);
    const color = this.getAlertColor(alert.priority);
    
    ctx.fillStyle = this.radar.colorToString(color);
    ctx.font = `bold ${size}px Arial`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    
    const icon = this.getAlertIcon(alert.type);
    ctx.fillText(icon, position.x, position.y);
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.alert);
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(position.x, position.y, size / 2 + 4, 0, Math.PI * 2);
    ctx.stroke();
    
    ctx.globalAlpha = 1;
  }
  
  private getAlertIcon(type: AlertType): string {
    switch (type) {
      case AlertType.UNIT_ATTACKED: return '⚔';
      case AlertType.BUILDING_ATTACKED: return '🏠';
      case AlertType.UNIT_DESTROYED: return '💀';
      case AlertType.BUILDING_DESTROYED: return '🔥';
      case AlertType.UNIT_CREATED: return '✓';
      case AlertType.BUILDING_COMPLETED: return '🏗';
      case AlertType.RESOURCE_DEPLETED: return '💰';
      case AlertType.UNIT_HEALED: return '❤';
      case AlertType.VISION_GAINED: return '👁';
      case AlertType.VISION_LOST: return '👁‍🗨';
      case AlertType.AREA_UNDER_ATTACK: return '🚨';
      case AlertType.ENEMY_SPOTTED: return '❗';
      case AlertType.ALLY_NEEDS_HELP: return '🆘';
      case AlertType.GARRISON_COMPLETE: return '🚪';
      case AlertType.RESEARCH_COMPLETE: return '🔬';
      case AlertType.UPGRADE_AVAILABLE: return '⬆';
      default: return '!';
    }
  }
  
  private getAlertColor(priority: AlertPriority): Color {
    const config = this.radar.getConfig();
    
    switch (priority) {
      case AlertPriority.CRITICAL: return { r: 255, g: 0, b: 0, a: 255 };
      case AlertPriority.HIGH: return { r: 255, g: 165, b: 0, a: 255 };
      case AlertPriority.MEDIUM: return { r: 255, g: 255, b: 0, a: 255 };
      case AlertPriority.LOW: return { r: 0, g: 255, b: 0, a: 255 };
      case AlertPriority.INFO: return { r: 128, g: 128, b: 128, a: 255 };
      default: return config.colors.alert;
    }
  }
  
  private getAlertSize(priority: AlertPriority): number {
    switch (priority) {
      case AlertPriority.CRITICAL: return 16;
      case AlertPriority.HIGH: return 14;
      case AlertPriority.MEDIUM: return 12;
      case AlertPriority.LOW: return 10;
      case AlertPriority.INFO: return 8;
      default: return 12;
    }
  }
  
  public createAlert(alertData: Partial<AlertData> & { position: Point; type: AlertType }): AlertData {
    const config = this.radar.getConfig();
    
    const alert: AlertData = {
      id: this.generateId(),
      type: alertData.type,
      position: alertData.position,
      message: alertData.message || '',
      timestamp: Date.now(),
      duration: alertData.duration || config.alertDuration,
      priority: alertData.priority || AlertPriority.MEDIUM,
      acknowledged: false,
      source: alertData.source || 'system',
      target: alertData.target || null,
      alliance: alertData.alliance || Alliance.NEUTRAL,
      ...alertData,
    };
    
    this.radar.createAlert(alert);
    return alert;
  }
  
  public removeAlert(id: string): void {
    this.radar.removeAlert(id);
  }
  
  public acknowledgeAlert(id: string): void {
    this.radar.acknowledgeAlert(id);
  }
  
  public getAlert(id: string): AlertData | undefined {
    return this.radar.getAlert(id);
  }
  
  public getAllAlerts(): AlertData[] {
    return this.radar.getAllAlerts();
  }
  
  public getUnacknowledgedAlerts(): AlertData[] {
    return this.radar.getAllAlerts().filter(alert => !alert.acknowledged);
  }
  
  public getAlertsByPriority(priority: AlertPriority): AlertData[] {
    return this.radar.getAllAlerts().filter(alert => alert.priority === priority);
  }
  
  public getAlertsInArea(center: Point, radius: number): AlertData[] {
    return this.radar.getAllAlerts().filter(alert => {
      const dx = alert.position.x - center.x;
      const dy = alert.position.y - center.y;
      return dx * dx + dy * dy <= radius * radius;
    });
  }
  
  public clearAlerts(): void {
    const alerts = this.radar.getAllAlerts();
    for (const alert of alerts) {
      this.radar.removeAlert(alert.id);
    }
  }
  
  public acknowledgeAllAlerts(): void {
    const alerts = this.radar.getAllAlerts();
    for (const alert of alerts) {
      this.radar.acknowledgeAlert(alert.id);
    }
  }
  
  private generateId(): string {
    return `alert-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}

export class UnitMarker {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const units = this.radar.getAllUnits();
    const config = this.radar.getConfig();
    
    for (const unit of units) {
      if (!unit.isVisible || !unit.isAlive) continue;
      
      const screenPos = this.radar.worldToMinimap(unit.position);
      const size = this.getUnitSize(unit);
      
      this.renderUnitIcon(ctx, screenPos, size, unit);
      
      if (unit.isSelected) {
        this.renderSelectionIndicator(ctx, screenPos, size);
      }
      
      if (unit.isMoving) {
        this.renderMovementIndicator(ctx, screenPos, unit);
      }
      
      if (unit.isAttacking) {
        this.renderAttackIndicator(ctx, screenPos, size);
      }
      
      if (unit.isConstructing) {
        this.renderConstructionIndicator(ctx, screenPos, size);
      }
      
      if (unit.isHarvesting) {
        this.renderHarvestingIndicator(ctx, screenPos, size);
      }
      
      if (unit.status.length > 0) {
        this.renderStatusIndicators(ctx, screenPos, size, unit);
      }
      
      this.renderHealthBar(screenPos, size, unit);
      
      if (unit.energy > 0 || unit.maxEnergy > 0) {
        this.renderEnergyBar(screenPos, size, unit);
      }
      
      if (unit.shield > 0 || unit.maxShield > 0) {
        this.renderShieldBar(screenPos, size, unit);
      }
    }
  }
  
  private getUnitSize(unit: UnitData): number {
    const config = this.radar.getConfig();
    let size = Math.max(2, unit.size * (config.width / config.mapWidth));
    
    if (unit.group === UnitGroup.AIR) {
      size *= 1.2;
    } else if (unit.group === UnitGroup.BUILDING) {
      size *= 0.8;
    }
    
    return Math.max(3, Math.min(10, size));
  }
  
  private renderUnitIcon(ctx: CanvasRenderingContext2D, position: Point, size: number, unit: UnitData): void {
    const config = this.radar.getConfig();
    const color = this.getAllianceColor(unit.alliance);
    
    ctx.fillStyle = this.radar.colorToString(color);
    
    switch (unit.group) {
      case UnitGroup.INFANTRY:
        ctx.beginPath();
        ctx.arc(position.x, position.y, size, 0, Math.PI * 2);
        ctx.fill();
        break;
        
      case UnitGroup.VEHICLE:
        ctx.fillRect(position.x - size, position.y - size * 0.7, size * 2, size * 1.4);
        break;
        
      case UnitGroup.AIR:
        ctx.beginPath();
        ctx.moveTo(position.x, position.y - size);
        ctx.lineTo(position.x + size, position.y + size);
        ctx.lineTo(position.x - size, position.y + size);
        ctx.closePath();
        ctx.fill();
        break;
        
      case UnitGroup.NAVAL:
        ctx.beginPath();
        ctx.ellipse(position.x, position.y, size * 1.5, size, 0, 0, Math.PI * 2);
        ctx.fill();
        break;
        
      case UnitGroup.WORKER:
        ctx.beginPath();
        ctx.arc(position.x, position.y, size, 0, Math.PI * 2);
        ctx.fill();
        ctx.strokeStyle = this.radar.colorToString(config.colors.resource);
        ctx.lineWidth = 2;
        ctx.stroke();
        break;
        
      case UnitGroup.BUILDING:
        ctx.fillRect(position.x - size, position.y - size, size * 2, size * 2);
        break;
        
      default:
        ctx.beginPath();
        ctx.arc(position.x, position.y, size, 0, Math.PI * 2);
        ctx.fill();
    }
  }
  
  private getAllianceColor(alliance: Alliance): Color {
    const config = this.radar.getConfig();
    
    switch (alliance) {
      case Alliance.ALLY: return config.colors.ally;
      case Alliance.ENEMY: return config.colors.enemy;
      case Alliance.NEUTRAL: return config.colors.neutral;
      case Alliance.UNKNOWN: return config.colors.unknown;
      default: return config.colors.neutral;
    }
  }
  
  private renderSelectionIndicator(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    const config = this.radar.getConfig();
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
    ctx.lineWidth = 2;
    
    ctx.beginPath();
    ctx.arc(position.x, position.y, size + 3, 0, Math.PI * 2);
    ctx.stroke();
    
    const numDots = 8;
    for (let i = 0; i < numDots; i++) {
      const angle = (i / numDots) * Math.PI * 2;
      const dotX = position.x + Math.cos(angle) * (size + 5);
      const dotY = position.y + Math.sin(angle) * (size + 5);
      
      ctx.fillStyle = this.radar.colorToString(config.colors.selection);
      ctx.beginPath();
      ctx.arc(dotX, dotY, 1, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  
  private renderMovementIndicator(ctx: CanvasRenderingContext2D, position: Point, unit: UnitData): void {
    const config = this.radar.getConfig();
    
    const velocity = Math.sqrt(unit.velocity.x * unit.velocity.x + unit.velocity.y * unit.velocity.y);
    if (velocity === 0) return;
    
    const direction = {
      x: unit.velocity.x / velocity,
      y: unit.velocity.y / velocity,
    };
    
    const lineLength = 15;
    const endX = position.x + direction.x * lineLength;
    const endY = position.y + direction.y * lineLength;
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
    ctx.lineWidth = 1;
    ctx.setLineDash([3, 3]);
    
    ctx.beginPath();
    ctx.moveTo(position.x, position.y);
    ctx.lineTo(endX, endY);
    ctx.stroke();
    
    ctx.setLineDash([]);
  }
  
  private renderAttackIndicator(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    const config = this.radar.getConfig();
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.alert);
    ctx.lineWidth = 2;
    
    ctx.beginPath();
    ctx.arc(position.x, position.y, size + 6, 0, Math.PI * 2);
    ctx.stroke();
    
    const time = Date.now() / 100;
    ctx.setLineDash([4, 4]);
    ctx.lineDashOffset = -time % 8;
    
    ctx.beginPath();
    ctx.arc(position.x, position.y, size + 9, 0, Math.PI * 2);
    ctx.stroke();
    
    ctx.setLineDash([]);
  }
  
  private renderConstructionIndicator(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    const config = this.radar.getConfig();
    
    const time = Date.now() / 500;
    const progress = (Math.sin(time * Math.PI) + 1) / 2;
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.ally);
    ctx.lineWidth = 2;
    
    ctx.beginPath();
    ctx.arc(position.x, position.y, size + 4, -Math.PI / 2, -Math.PI / 2 + progress * Math.PI * 2);
    ctx.stroke();
  }
  
  private renderHarvestingIndicator(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    const config = this.radar.getConfig();
    
    ctx.fillStyle = this.radar.colorToString(config.colors.resource);
    
    const time = Date.now() / 200;
    const angle = time % (Math.PI * 2);
    const bobY = Math.sin(angle * 4) * 2;
    
    ctx.beginPath();
    ctx.arc(position.x + size + 3, position.y + bobY, 2, 0, Math.PI * 2);
    ctx.fill();
  }
  
  private renderStatusIndicators(ctx: CanvasRenderingContext2D, position: Point, size: number, unit: UnitData): void {
    const config = this.radar.getConfig();
    
    let offsetY = -size - 8;
    
    if (unit.isMoving) {
      ctx.fillStyle = this.radar.colorToString(config.colors.selection);
      ctx.font = '8px Arial';
      ctx.textAlign = 'center';
      ctx.fillText('→', position.x, position.y + offsetY);
      offsetY -= 8;
    }
    
    if (unit.isAttacking) {
      ctx.fillStyle = this.radar.colorToString(config.colors.alert);
      ctx.font = '8px Arial';
      ctx.textAlign = 'center';
      ctx.fillText('⚔', position.x, position.y + offsetY);
      offsetY -= 8;
    }
    
    if (unit.isConstructing) {
      ctx.fillStyle = this.radar.colorToString(config.colors.ally);
      ctx.font = '8px Arial';
      ctx.textAlign = 'center';
      ctx.fillText('🔨', position.x, position.y + offsetY);
      offsetY -= 8;
    }
    
    if (unit.isHarvesting) {
      ctx.fillStyle = this.radar.colorToString(config.colors.resource);
      ctx.font = '8px Arial';
      ctx.textAlign = 'center';
      ctx.fillText('⛏', position.x, position.y + offsetY);
      offsetY -= 8;
    }
  }
  
  private renderHealthBar(position: Point, size: number, unit: UnitData): void {
    if (unit.health >= unit.maxHealth) return;
    
    const config = this.radar.getConfig();
    const barWidth = size * 2;
    const barHeight = 2;
    const barX = position.x - size;
    const barY = position.y + size + 2;
    
    const healthPercent = unit.health / unit.maxHealth;
    
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(barX, barY, barWidth, barHeight);
    
    let healthColor = config.colors.healthBar;
    if (healthPercent < 0.5) {
      healthColor = { r: 255, g: 165, b: 0, a: 255 };
    }
    if (healthPercent < 0.25) {
      healthColor = { r: 255, g: 0, b: 0, a: 255 };
    }
    
    ctx.fillStyle = this.radar.colorToString(healthColor);
    ctx.fillRect(barX, barY, barWidth * healthPercent, barHeight);
  }
  
  private renderEnergyBar(position: Point, size: number, unit: UnitData): void {
    const config = this.radar.getConfig();
    const barWidth = size * 2;
    const barHeight = 2;
    const barX = position.x - size;
    const barY = position.y + size + 5;
    
    const energyPercent = unit.energy / unit.maxEnergy;
    
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(barX, barY, barWidth, barHeight);
    
    ctx.fillStyle = this.radar.colorToString(config.colors.energyBar);
    ctx.fillRect(barX, barY, barWidth * energyPercent, barHeight);
  }
  
  private renderShieldBar(position: Point, size: number, unit: UnitData): void {
    const config = this.radar.getConfig();
    const barWidth = size * 2;
    const barHeight = 2;
    const barX = position.x - size;
    const barY = position.y + size + 8;
    
    const shieldPercent = unit.shield / unit.maxShield;
    
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(barX, barY, barWidth, barHeight);
    
    ctx.fillStyle = this.radar.colorToString(config.colors.shieldBar);
    ctx.fillRect(barX, barY, barWidth * shieldPercent, barHeight);
  }
}

export class TerrainFeatures {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const terrain = this.radar.getTerrain();
    if (!terrain) return;
    
    for (const feature of terrain.features) {
      if (!this.radar.isPointExplored(feature.position)) continue;
      
      const screenPos = this.radar.worldToMinimap(feature.position);
      const size = feature.size * (this.radar.getConfig().width / this.radar.getConfig().mapWidth);
      
      this.renderFeature(ctx, screenPos, size, feature);
    }
  }
  
  private renderFeature(ctx: CanvasRenderingContext2D, position: Point, size: number, feature: TerrainFeature): void {
    switch (feature.type) {
      case TerrainFeatureType.ROCK:
        this.renderRock(ctx, position, size);
        break;
      case TerrainFeatureType.TREE:
        this.renderTree(ctx, position, size);
        break;
      case TerrainFeatureType.WATER:
        this.renderWater(ctx, position, size);
        break;
      case TerrainFeatureType.CLIFF:
        this.renderCliff(ctx, position, size);
        break;
      case TerrainFeatureType.ROAD:
        this.renderRoad(ctx, position, size);
        break;
      case TerrainFeatureType.BRIDGE:
        this.renderBridge(ctx, position, size);
        break;
      case TerrainFeatureType.RUIN:
        this.renderRuin(ctx, position, size);
        break;
      case TerrainFeatureType.VILLAGE:
        this.renderVillage(ctx, position, size);
        break;
      case TerrainFeatureType.CASTLE:
        this.renderCastle(ctx, position, size);
        break;
      case TerrainFeatureType.TOWER:
        this.renderTower(ctx, position, size);
        break;
      case TerrainFeatureType.WALL:
        this.renderWall(ctx, position, size);
        break;
    }
  }
  
  private renderRock(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(128, 128, 128, 0.8)';
    ctx.beginPath();
    ctx.moveTo(position.x, position.y - size);
    ctx.lineTo(position.x + size * 0.8, position.y);
    ctx.lineTo(position.x + size * 0.5, position.y + size * 0.8);
    ctx.lineTo(position.x - size * 0.5, position.y + size * 0.8);
    ctx.lineTo(position.x - size * 0.8, position.y);
    ctx.closePath();
    ctx.fill();
  }
  
  private renderTree(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(34, 139, 34, 0.8)';
    ctx.beginPath();
    ctx.arc(position.x, position.y - size * 0.3, size * 0.7, 0, Math.PI * 2);
    ctx.fill();
    
    ctx.fillStyle = 'rgba(139, 90, 43, 0.8)';
    ctx.fillRect(position.x - size * 0.1, position.y, size * 0.2, size * 0.5);
  }
  
  private renderWater(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(64, 164, 223, 0.6)';
    ctx.fillRect(position.x - size, position.y - size, size * 2, size * 2);
    
    ctx.strokeStyle = 'rgba(100, 200, 255, 0.4)';
    ctx.lineWidth = 1;
    const time = Date.now() / 1000;
    
    for (let i = 0; i < 3; i++) {
      const offset = Math.sin(time + i) * 2;
      ctx.beginPath();
      ctx.moveTo(position.x - size, position.y + offset + i * 4);
      ctx.quadraticCurveTo(
        position.x, position.y + offset + i * 4 - 2,
        position.x + size, position.y + offset + i * 4
      );
      ctx.stroke();
    }
  }
  
  private renderCliff(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(105, 105, 105, 0.9)';
    ctx.fillRect(position.x - size, position.y - size, size * 2, size * 2);
    
    ctx.fillStyle = 'rgba(85, 85, 85, 1)';
    ctx.beginPath();
    ctx.moveTo(position.x - size, position.y - size);
    ctx.lineTo(position.x + size, position.y - size);
    ctx.lineTo(position.x + size, position.y - size * 0.3);
    ctx.lineTo(position.x - size, position.y);
    ctx.closePath();
    ctx.fill();
  }
  
  private renderRoad(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(139, 119, 101, 0.8)';
    ctx.fillRect(position.x - size * 0.3, position.y - size, size * 0.6, size * 2);
  }
  
  private renderBridge(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(139, 90, 43, 0.9)';
    ctx.fillRect(position.x - size * 0.5, position.y - size * 0.3, size, size * 0.6);
    
    ctx.strokeStyle = 'rgba(100, 60, 30, 1)';
    ctx.lineWidth = 1;
    ctx.strokeRect(position.x - size * 0.5, position.y - size * 0.3, size, size * 0.6);
  }
  
  private renderRuin(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(100, 100, 100, 0.7)';
    
    ctx.fillRect(position.x - size * 0.7, position.y - size * 0.5, size * 1.4, size);
    
    ctx.fillStyle = 'rgba(80, 80, 80, 0.9)';
    ctx.beginPath();
    ctx.moveTo(position.x - size * 0.5, position.y - size * 0.5);
    ctx.lineTo(position.x - size * 0.3, position.y - size);
    ctx.lineTo(position.x - size * 0.1, position.y - size * 0.5);
    ctx.closePath();
    ctx.fill();
  }
  
  private renderVillage(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(210, 180, 140, 0.8)';
    ctx.beginPath();
    ctx.arc(position.x, position.y, size * 0.6, 0, Math.PI * 2);
    ctx.fill();
    
    ctx.fillStyle = 'rgba(139, 69, 19, 0.8)';
    const houseCount = 3;
    for (let i = 0; i < houseCount; i++) {
      const angle = (i / houseCount) * Math.PI * 2;
      const hx = position.x + Math.cos(angle) * size * 0.4;
      const hy = position.y + Math.sin(angle) * size * 0.4;
      
      ctx.fillRect(hx - 3, hy - 3, 6, 6);
    }
  }
  
  private renderCastle(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(128, 128, 128, 0.9)';
    ctx.fillRect(position.x - size, position.y - size, size * 2, size * 2);
    
    ctx.fillStyle = 'rgba(100, 100, 100, 1)';
    const towerCount = 4;
    for (let i = 0; i < towerCount; i++) {
      const angle = (i / towerCount) * Math.PI * 2;
      const tx = position.x + Math.cos(angle) * size * 0.9;
      const ty = position.y + Math.sin(angle) * size * 0.9;
      
      ctx.beginPath();
      ctx.arc(tx, ty, size * 0.25, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  
  private renderTower(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(128, 128, 128, 0.9)';
    ctx.fillRect(position.x - size * 0.3, position.y - size, size * 0.6, size * 2);
    
    ctx.fillStyle = 'rgba(100, 100, 100, 1)';
    ctx.beginPath();
    ctx.moveTo(position.x - size * 0.4, position.y - size);
    ctx.lineTo(position.x, position.y - size * 1.3);
    ctx.lineTo(position.x + size * 0.4, position.y - size);
    ctx.closePath();
    ctx.fill();
  }
  
  private renderWall(ctx: CanvasRenderingContext2D, position: Point, size: number): void {
    ctx.fillStyle = 'rgba(128, 128, 128, 0.9)';
    ctx.fillRect(position.x - size, position.y - size * 0.2, size * 2, size * 0.4);
    
    ctx.fillStyle = 'rgba(100, 100, 100, 1)';
    const segmentCount = 5;
    for (let i = 0; i < segmentCount; i++) {
      const sx = position.x - size + (i * 2 * size) / segmentCount;
      ctx.fillRect(sx, position.y - size * 0.3, size * 0.2, size * 0.3);
    }
  }
}

export class FogOverlay {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const config = this.radar.getConfig();
    if (!config.fogEnabled) return;
    
    const fog = this.radar.getFog();
    if (!fog) return;
    
    this.renderFogOfWar(ctx, config, fog);
  }
  
  private renderFogOfWar(ctx: CanvasRenderingContext2D, config: RadarConfig, fog: FogData): void {
    const scaleX = fog.width / config.width;
    const scaleY = fog.height / config.height;
    
    const chunkSize = 4;
    
    for (let y = 0; y < config.height; y += chunkSize) {
      for (let x = 0; x < config.width; x += chunkSize) {
        const fogX = Math.floor(x * scaleX);
        const fogY = Math.floor(y * scaleY);
        const index = fogY * fog.width + fogX;
        
        let alpha = 0;
        
        if (fog.visible[index] === 0) {
          if (fog.explored[index] === 0) {
            alpha = 1;
          } else {
            alpha = 0.5;
          }
        }
        
        if (alpha > 0) {
          ctx.fillStyle = `rgba(${config.colors.fog.r}, ${config.colors.fog.g}, ${config.colors.fog.b}, ${alpha})`;
          ctx.fillRect(x, y, chunkSize, chunkSize);
        }
      }
    }
  }
  
  public updateFog(): void {
    this.radar.updateFog();
  }
  
  public isPointVisible(position: Point): boolean {
    return this.radar.isPointVisible(position);
  }
  
  public isPointExplored(position: Point): boolean {
    return this.radar.isPointExplored(position);
  }
  
  public setFogEnabled(enabled: boolean): void {
    const config = this.radar.getConfig();
    config.fogEnabled = enabled;
    this.radar.markDirty();
  }
  
  public isFogEnabled(): boolean {
    return this.radar.getConfig().fogEnabled;
  }
}

export class VisionRange {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const config = this.radar.getConfig();
    if (!config.showVision) return;
    
    const vision = this.radar.getVision();
    if (!vision) return;
    
    this.renderUnitVision(ctx, config, vision);
    this.renderBuildingVision(ctx, config, vision);
  }
  
  private renderUnitVision(ctx: CanvasRenderingContext2D, config: RadarConfig, vision: VisionData): void {
    for (const unitVision of vision.unitVision) {
      const screenPos = this.radar.worldToMinimap(unitVision.center);
      const screenRadius = unitVision.radius * (config.width / config.mapWidth);
      
      this.renderVisionArea(ctx, screenPos, screenRadius, unitVision.shape, config.colors.vision);
    }
  }
  
  private renderBuildingVision(ctx: CanvasRenderingContext2D, config: RadarConfig, vision: VisionData): void {
    for (const buildingVision of vision.buildingVision) {
      if (!buildingVision.isActive) continue;
      
      const screenPos = this.radar.worldToMinimap(buildingVision.center);
      const screenRadius = buildingVision.radius * (config.width / config.mapWidth);
      
      this.renderVisionArea(ctx, screenPos, screenRadius, buildingVision.shape, config.colors.vision);
    }
  }
  
  private renderVisionArea(
    ctx: CanvasRenderingContext2D,
    center: Point,
    radius: number,
    shape: VisionShape,
    color: Color
  ): void {
    const gradient = ctx.createRadialGradient(
      center.x, center.y, 0,
      center.x, center.y, radius
    );
    
    gradient.addColorStop(0, `rgba(${color.r}, ${color.g}, ${color.b}, 0.3)`);
    gradient.addColorStop(0.5, `rgba(${color.r}, ${color.g}, ${color.b}, 0.15)`);
    gradient.addColorStop(1, `rgba(${color.r}, ${color.g}, ${color.b}, 0)`);
    
    ctx.fillStyle = gradient;
    ctx.beginPath();
    
    switch (shape) {
      case VisionShape.CIRCULAR:
        ctx.arc(center.x, center.y, radius, 0, Math.PI * 2);
        break;
        
      case VisionShape.SQUARE:
        ctx.rect(center.x - radius, center.y - radius, radius * 2, radius * 2);
        break;
        
      case VisionShape.CONE:
        const angle = 0;
        const coneAngle = Math.PI / 4;
        ctx.moveTo(center.x, center.y);
        ctx.arc(center.x, center.y, radius, angle - coneAngle, angle + coneAngle);
        ctx.closePath();
        break;
    }
    
    ctx.fill();
    
    ctx.strokeStyle = `rgba(${color.r}, ${color.g}, ${color.b}, 0.5)`;
    ctx.lineWidth = 1;
    ctx.stroke();
  }
  
  public renderUnitVisionRange(unit: UnitData): void {
    const ctx = this.radar.getContext();
    if (!ctx) return;
    
    const config = this.radar.getConfig();
    const screenPos = this.radar.worldToMinimap(unit.position);
    const screenRadius = unit.sightRange * (config.width / config.mapWidth);
    
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
    ctx.lineWidth = 1;
    ctx.setLineDash([3, 3]);
    ctx.beginPath();
    ctx.arc(screenPos.x, screenPos.y, screenRadius, 0, Math.PI * 2);
    ctx.stroke();
    ctx.setLineDash([]);
  }
  
  public renderBuildingVisionRange(building: BuildingData): void {
    const ctx = this.radar.getContext();
    if (!ctx) return;
    
    const config = this.radar.getConfig();
    const screenPos = this.radar.worldToMinimap(building.position);
    const screenRadius = building.sightRange * (config.width / config.mapWidth);
    
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.3)';
    ctx.lineWidth = 1;
    ctx.setLineDash([3, 3]);
    ctx.beginPath();
    ctx.arc(screenPos.x, screenPos.y, screenRadius, 0, Math.PI * 2);
    ctx.stroke();
    ctx.setLineDash([]);
  }
}

export class ResourceIndicator {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const config = this.radar.getConfig();
    if (!config.showResources) return;
    
    const resources = this.radar.getAllResources();
    
    for (const resource of resources) {
      if (!resource.isVisible) continue;
      
      const screenPos = this.radar.worldToMinimap(resource.position);
      const size = this.getResourceSize(resource);
      
      this.renderResourceIcon(ctx, screenPos, size, resource);
      this.renderResourceAmount(ctx, screenPos, size, resource);
      
      if (resource.harvesters.length > 0) {
        this.renderHarvesterIndicators(ctx, screenPos, size, resource);
      }
      
      if (resource.owner) {
        this.renderOwnerIndicator(ctx, screenPos, size, resource);
      }
    }
  }
  
  private getResourceSize(resource: ResourceData): number {
    const config = this.radar.getConfig();
    let size = Math.max(2, (resource.capacity / 10) * (config.width / config.mapWidth));
    
    return Math.max(3, Math.min(8, size));
  }
  
  private renderResourceIcon(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    const config = this.radar.getConfig();
    
    if (resource.isDepleted) {
      ctx.fillStyle = 'rgba(128, 128, 128, 0.5)';
    } else {
      ctx.fillStyle = this.radar.colorToString(config.colors.resource);
    }
    
    switch (resource.type) {
      case ResourceType.GOLD:
      case ResourceType.IRON:
      case ResourceType.CRYSTAL:
        this.renderMineral(ctx, position, size, resource);
        break;
        
      case ResourceType.WOOD:
        this.renderTree(ctx, position, size, resource);
        break;
        
      case ResourceType.STONE:
        this.renderStone(ctx, position, size, resource);
        break;
        
      case ResourceType.FOOD:
        this.renderFood(ctx, position, size, resource);
        break;
        
      case ResourceType.OIL:
        this.renderOil(ctx, position, size, resource);
        break;
        
      case ResourceType.ENERGY:
        this.renderEnergy(ctx, position, size, resource);
        break;
    }
  }
  
  private renderMineral(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    ctx.beginPath();
    ctx.moveTo(position.x, position.y - size);
    ctx.lineTo(position.x + size * 0.7, position.y - size * 0.3);
    ctx.lineTo(position.x + size * 0.5, position.y + size * 0.7);
    ctx.lineTo(position.x - size * 0.5, position.y + size * 0.7);
    ctx.lineTo(position.x - size * 0.7, position.y - size * 0.3);
    ctx.closePath();
    ctx.fill();
    
    ctx.strokeStyle = 'rgba(255, 215, 0, 0.5)';
    ctx.lineWidth = 1;
    ctx.stroke();
  }
  
  private renderTree(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    ctx.fillRect(position.x - size * 0.3, position.y, size * 0.6, size);
    
    ctx.beginPath();
    ctx.arc(position.x, position.y, size, 0, Math.PI * 2);
    ctx.fill();
  }
  
  private renderStone(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    ctx.beginPath();
    ctx.moveTo(position.x, position.y - size);
    ctx.lineTo(position.x + size, position.y + size * 0.5);
    ctx.lineTo(position.x - size, position.y + size * 0.5);
    ctx.closePath();
    ctx.fill();
  }
  
  private renderFood(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    ctx.fillStyle = 'rgba(255, 200, 100, 0.9)';
    ctx.beginPath();
    ctx.arc(position.x, position.y, size, 0, Math.PI * 2);
    ctx.fill();
  }
  
  private renderOil(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    ctx.fillStyle = 'rgba(50, 50, 50, 0.9)';
    ctx.fillRect(position.x - size, position.y - size * 0.5, size * 2, size);
    
    ctx.fillStyle = 'rgba(100, 100, 100, 0.5)';
    ctx.beginPath();
    ctx.arc(position.x - size * 0.3, position.y, size * 0.3, 0, Math.PI * 2);
    ctx.fill();
  }
  
  private renderEnergy(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    ctx.strokeStyle = this.radar.getConfig().colors.resource;
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(position.x, position.y, size, 0, Math.PI * 2);
    ctx.stroke();
    
    const time = Date.now() / 200;
    const lightning = Math.sin(time) > 0;
    
    if (lightning) {
      ctx.fillStyle = 'rgba(255, 255, 0, 0.8)';
      ctx.beginPath();
      ctx.arc(position.x, position.y, size * 0.5, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  
  private renderResourceAmount(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    if (resource.isDepleted) return;
    
    ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
    ctx.font = '8px Arial';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'top';
    
    const amount = Math.floor(resource.amount);
    const displayText = amount >= 1000 ? `${Math.floor(amount / 1000)}k` : `${amount}`;
    
    ctx.fillText(displayText, position.x, position.y + size + 2);
  }
  
  private renderHarvesterIndicators(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    const config = this.radar.getConfig();
    const harvesterCount = resource.harvesters.length;
    
    if (harvesterCount === 0) return;
    
    const maxVisible = 4;
    const visibleCount = Math.min(harvesterCount, maxVisible);
    
    for (let i = 0; i < visibleCount; i++) {
      const angle = ((i - visibleCount / 2 + 0.5) / visibleCount) * Math.PI * 0.5;
      const indicatorX = position.x + Math.cos(angle - Math.PI / 2) * (size + 6);
      const indicatorY = position.y + Math.sin(angle - Math.PI / 2) * (size + 6);
      
      ctx.fillStyle = this.radar.colorToString(config.colors.ally);
      ctx.beginPath();
      ctx.arc(indicatorX, indicatorY, 2, 0, Math.PI * 2);
      ctx.fill();
    }
    
    if (harvesterCount > maxVisible) {
      ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
      ctx.font = '6px Arial';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText(`+${harvesterCount - maxVisible}`, position.x + size + 8, position.y);
    }
  }
  
  private renderOwnerIndicator(ctx: CanvasRenderingContext2D, position: Point, size: number, resource: ResourceData): void {
    const config = this.radar.getConfig();
    
    let color = config.colors.neutral;
    
    if (resource.owner) {
      color = config.colors.ally;
    }
    
    ctx.strokeStyle = this.radar.colorToString(color);
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.arc(position.x, position.y, size + 2, 0, Math.PI * 2);
    ctx.stroke();
  }
}

export class BuildingIndicator {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const config = this.radar.getConfig();
    if (!config.showBuildings) return;
    
    const buildings = this.radar.getAllBuildings();
    
    for (const building of buildings) {
      if (!building.isVisible) continue;
      
      const screenPos = this.radar.worldToMinimap(building.position);
      const size = this.getBuildingSize(building);
      
      this.renderBuildingIcon(ctx, screenPos, size, building);
      
      if (building.isSelected) {
        this.renderSelectionIndicator(ctx, screenPos, size, building);
      }
      
      if (building.isConstructing) {
        this.renderConstructionIndicator(ctx, screenPos, size, building);
      }
      
      if (building.isDamaged) {
        this.renderDamageIndicator(ctx, screenPos, size, building);
      }
      
      if (building.isDestroyed) {
        this.renderDestroyedIndicator(ctx, screenPos, size, building);
      }
      
      this.renderHealthBar(screenPos, size, building);
      
      if (building.currentUnits > 0) {
        this.renderGarrisonIndicator(ctx, screenPos, size, building);
      }
      
      if (building.productionQueue.length > 0) {
        this.renderProductionIndicator(ctx, screenPos, size, building);
      }
      
      if (building.isRallyPoint && building.rallyPoint) {
        this.renderRallyPointIndicator(ctx, screenPos, building);
      }
    }
  }
  
  private getBuildingSize(building: BuildingData): Point {
    const config = this.radar.getConfig();
    return {
      x: Math.max(4, building.size.x * (config.width / config.mapWidth)),
      y: Math.max(4, building.size.y * (config.height / config.mapHeight)),
    };
  }
  
  private renderBuildingIcon(ctx: CanvasRenderingContext2D, position: Point, size: Point, building: BuildingData): void {
    const config = this.radar.getConfig();
    
    let color = config.colors.neutral;
    switch (building.alliance) {
      case Alliance.ALLY: color = config.colors.ally; break;
      case Alliance.ENEMY: color = config.colors.enemy; break;
    }
    
    ctx.fillStyle = this.radar.colorToString(color);
    ctx.fillRect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y);
    
    if (building.alliance !== Alliance.NEUTRAL) {
      ctx.strokeStyle = this.radar.colorToString(config.colors.building);
      ctx.lineWidth = 1;
      ctx.strokeRect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y);
    }
  }
  
  private renderSelectionIndicator(ctx: CanvasRenderingContext2D, position: Point, size: Point, building: BuildingData): void {
    const config = this.radar.getConfig();
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
    ctx.lineWidth = 2;
    ctx.strokeRect(
      position.x - size.x / 2 - 2,
      position.y - size.y / 2 - 2,
      size.x + 4,
      size.y + 4
    );
  }
  
  private renderConstructionIndicator(ctx: CanvasRenderingContext2D, position: Point, size: Point, building: BuildingData): void {
    const config = this.radar.getConfig();
    
    const time = Date.now() / 500;
    const progress = (Math.sin(time * Math.PI) + 1) / 2;
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.ally);
    ctx.lineWidth = 2;
    
    ctx.beginPath();
    ctx.arc(position.x, position.y + size.y / 2 + 4, size.x / 2 + 2, -Math.PI / 2, -Math.PI / 2 + progress * Math.PI * 2);
    ctx.stroke();
    
    ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
    ctx.fillRect(position.x - size.x / 2, position.y + size.y / 2 + 8, size.x, 6);
    
    ctx.fillStyle = this.radar.colorToString(config.colors.ally);
    const healthPercent = building.health / building.maxHealth;
    ctx.fillRect(position.x - size.x / 2, position.y + size.y / 2 + 8, size.x * healthPercent, 6);
  }
  
  private renderDamageIndicator(ctx: CanvasRenderingContext2D, position: Point, size: Point, building: BuildingData): void {
    const config = this.radar.getConfig();
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.alert);
    ctx.lineWidth = 1;
    
    const crackCount = 3;
    for (let i = 0; i < crackCount; i++) {
      const startX = position.x - size.x / 2 + Math.random() * size.x;
      const startY = position.y - size.y / 2 + Math.random() * size.y;
      
      ctx.beginPath();
      ctx.moveTo(startX, startY);
      ctx.lineTo(startX + (Math.random() - 0.5) * 10, startY + (Math.random() - 0.5) * 10);
      ctx.stroke();
    }
  }
  
  private renderDestroyedIndicator(ctx: CanvasRenderingContext2D, position: Point, size: Point, building: BuildingData): void {
    ctx.fillStyle = 'rgba(50, 50, 50, 0.7)';
    ctx.fillRect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y);
    
    ctx.strokeStyle = 'rgba(100, 100, 100, 0.5)';
    ctx.lineWidth = 1;
    ctx.strokeRect(position.x - size.x / 2, position.y - size.y / 2, size.x, size.y);
  }
  
  private renderHealthBar(position: Point, size: Point, building: BuildingData): void {
    if (building.health >= building.maxHealth && !building.isDamaged) return;
    
    const config = this.radar.getConfig();
    const barWidth = size.x;
    const barHeight = 3;
    const barX = position.x - size.x / 2;
    const barY = position.y + size.y / 2 + 2;
    
    const healthPercent = building.health / building.maxHealth;
    
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(barX, barY, barWidth, barHeight);
    
    let healthColor = config.colors.healthBar;
    if (healthPercent < 0.5) {
      healthColor = { r: 255, g: 165, b: 0, a: 255 };
    }
    if (healthPercent < 0.25) {
      healthColor = { r: 255, g: 0, b: 0, a: 255 };
    }
    
    ctx.fillStyle = this.radar.colorToString(healthColor);
    ctx.fillRect(barX, barY, barWidth * healthPercent, barHeight);
  }
  
  private renderGarrisonIndicator(ctx: CanvasRenderingContext2D, position: Point, size: Point, building: BuildingData): void {
    const config = this.radar.getConfig();
    
    ctx.fillStyle = this.radar.colorToString(config.colors.ally);
    ctx.font = '8px Arial';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    
    ctx.fillText(`${building.currentUnits}/${building.unitCapacity}`, position.x, position.y);
  }
  
  private renderProductionIndicator(ctx: CanvasRenderingContext2D, position: Point, size: Point, building: BuildingData): void {
    const config = this.radar.getConfig();
    
    ctx.fillStyle = this.radar.colorToString(config.colors.ally);
    ctx.font = '6px Arial';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'top';
    
    const queueText = building.productionQueue.slice(0, 3).join(', ');
    ctx.fillText(queueText, position.x + size.x / 2 + 2, position.y - size.y / 2);
    
    if (building.productionQueue.length > 3) {
      ctx.fillText(`+${building.productionQueue.length - 3}`, position.x + size.x / 2 + 2, position.y - size.y / 2 + 8);
    }
  }
  
  private renderRallyPointIndicator(ctx: CanvasRenderingContext2D, buildingPos: Point, building: BuildingData): void {
    if (!building.rallyPoint) return;
    
    const config = this.radar.getConfig();
    const screenPos = this.radar.worldToMinimap(building.rallyPoint);
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
    ctx.lineWidth = 1;
    ctx.setLineDash([3, 3]);
    
    ctx.beginPath();
    ctx.moveTo(buildingPos.x, buildingPos.y);
    ctx.lineTo(screenPos.x, screenPos.y);
    ctx.stroke();
    
    ctx.setLineDash([]);
    
    ctx.fillStyle = this.radar.colorToString(config.colors.selection);
    ctx.beginPath();
    ctx.arc(screenPos.x, screenPos.y, 3, 0, Math.PI * 2);
    ctx.fill();
  }
}

export class SelectionBox {
  private radar: RadarSystem;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public render(): void {
    const ctx = this.radar.getContext();
    const canvas = this.radar.getCanvas();
    if (!ctx || !canvas) return;
    
    const selectionBox = this.radar.getSelectionBox();
    if (!selectionBox.isActive) return;
    
    this.renderSelectionRectangle(ctx, selectionBox);
    this.renderSelectionInfo(ctx, selectionBox);
  }
  
  private renderSelectionRectangle(ctx: CanvasRenderingContext2D, selectionBox: SelectionBox): void {
    const config = this.radar.getConfig();
    
    const start = this.radar.worldToMinimap(selectionBox.start);
    const end = this.radar.worldToMinimap(selectionBox.end);
    
    const minX = Math.min(start.x, end.x);
    const maxX = Math.max(start.x, end.x);
    const minY = Math.min(start.y, end.y);
    const maxY = Math.max(start.y, end.y);
    
    ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
    ctx.fillRect(minX, minY, maxX - minX, maxY - minY);
    
    ctx.strokeStyle = this.radar.colorToString(config.colors.selection);
    ctx.lineWidth = 1;
    ctx.strokeRect(minX, minY, maxX - minX, maxY - minY);
    
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.5)';
    ctx.setLineDash([5, 5]);
    ctx.strokeRect(minX, minY, maxX - minX, maxY - minY);
    ctx.setLineDash([]);
  }
  
  private renderSelectionInfo(ctx: CanvasRenderingContext2D, selectionBox: SelectionBox): void {
    const config = this.radar.getConfig();
    
    const start = this.radar.worldToMinimap(selectionBox.start);
    const end = this.radar.worldToMinimap(selectionBox.end);
    
    const minX = Math.min(start.x, end.x);
    const minY = Math.min(start.y, end.y);
    
    const unitCount = selectionBox.units.length;
    const buildingCount = selectionBox.buildings.length;
    const totalCount = unitCount + buildingCount;
    
    if (totalCount === 0) return;
    
    const infoText = `${totalCount} selected (${unitCount} units, ${buildingCount} buildings)`;
    
    ctx.fillStyle = 'rgba(0, 0, 0, 0.8)';
    const textWidth = ctx.measureText(infoText).width;
    ctx.fillRect(minX, minY - 15, textWidth + 10, 12);
    
    ctx.fillStyle = 'rgba(255, 255, 255, 0.9)';
    ctx.font = '10px Arial';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'middle';
    ctx.fillText(infoText, minX + 5, minY - 9);
  }
  
  public setSelectionBox(start: Point, end: Point): void {
  }
  
  public clearSelectionBox(): void {
  }
}

export class MinimapClickHandler {
  private radar: RadarSystem;
  private lastClickTime: number = 0;
  
  constructor(radar: RadarSystem) {
    this.radar = radar;
  }
  
  public handleClick(event: MinimapClickEvent): void {
    const config = this.radar.getConfig();
    const now = Date.now();
    
    if (now - this.lastClickTime < config.clickThrottle) {
      return;
    }
    this.lastClickTime = now;
    
    switch (event.type) {
      case ClickType.CLICK:
        this.handleSingleClick(event);
        break;
      case ClickType.DOUBLE_CLICK:
        this.handleDoubleClick(event);
        break;
      case ClickType.DRAG_START:
        this.handleDragStart(event);
        break;
      case ClickType.DRAG:
        this.handleDrag(event);
        break;
      case ClickType.DRAG_END:
        this.handleDragEnd(event);
        break;
    }
  }
  
  private handleSingleClick(event: MinimapClickEvent): void {
    const hoveredElement = this.radar.getHoveredElement(event.position);
    
    if (hoveredElement) {
      this.handleElementClick(hoveredElement, event);
    } else {
      this.handleMapClick(event);
    }
  }
  
  private handleDoubleClick(event: MinimapClickEvent): void {
    const hoveredElement = this.radar.getHoveredElement(event.position);
    
    if (hoveredElement) {
      this.radar.centerOn(hoveredElement.position);
      
      this.radar.emit('doubleclickcenter', {
        element: hoveredElement,
        position: hoveredElement.position,
      });
    } else {
      const state = this.radar.getState();
      const zoomLevel = this.getNextZoomLevel(state.zoom);
      this.radar.setZoom(zoomLevel);
      
      this.radar.centerOn(event.worldPosition);
    }
  }
  
  private handleElementClick(element: HoveredElement, event: MinimapClickEvent): void {
    switch (element.type) {
      case MarkerType.UNIT:
        if (event.button === MouseButton.LEFT) {
          if (event.shiftKey) {
            this.radar.emit('addselection', { unitId: element.id });
          } else {
            this.radar.emit('select', { unitId: element.id });
          }
        } else if (event.button === MouseButton.RIGHT) {
          this.radar.emit('unitcommand', {
            unitId: element.id,
            targetPosition: event.worldPosition,
            command: 'move',
          });
        }
        break;
        
      case MarkerType.BUILDING:
        if (event.button === MouseButton.LEFT) {
          this.radar.emit('selectbuilding', { buildingId: element.id });
        } else if (event.button === MouseButton.RIGHT) {
          this.radar.emit('buildingcommand', {
            buildingId: element.id,
            targetPosition: event.worldPosition,
            command: 'rally',
          });
        }
        break;
        
      case MarkerType.RESOURCE:
        this.radar.emit('selectresource', { resourceId: element.id });
        break;
        
      case MarkerType.PING:
        this.radar.emit('selectping', { pingId: element.id });
        break;
    }
  }
  
  private handleMapClick(event: MinimapClickEvent): void {
    if (event.button === MouseButton.LEFT) {
      if (event.shiftKey) {
      } else {
        this.radar.emit('deselectall');
        this.radar.centerOn(event.worldPosition);
      }
    } else if (event.button === MouseButton.RIGHT) {
      this.radar.emit('movecommand', {
        position: event.worldPosition,
        shiftKey: event.shiftKey,
        ctrlKey: event.ctrlKey,
        altKey: event.altKey,
      });
    } else if (event.button === MouseButton.MIDDLE) {
      this.radar.centerOn(event.worldPosition);
    }
  }
  
  private handleDragStart(event: MinimapClickEvent): void {
    this.radar.emit('dragstart', {
      position: event.position,
      worldPosition: event.worldPosition,
    });
  }
  
  private handleDrag(event: MinimapClickEvent): void {
    this.radar.pan(-event.position.x, -event.position.y);
    
    this.radar.emit('drag', {
      position: event.position,
      worldPosition: event.worldPosition,
    });
  }
  
  private handleDragEnd(event: MinimapClickEvent): void {
    const state = this.radar.getState();
    
    if (state.selectionStart && state.selectionEnd) {
      const worldStart = this.radar.minimapToWorld(state.selectionStart);
      const worldEnd = this.radar.minimapToWorld(state.selectionEnd);
      
      this.radar.emit('selectionbox', {
        start: worldStart,
        end: worldEnd,
      });
    }
    
    this.radar.emit('dragend', {
      position: event.position,
      worldPosition: event.worldPosition,
    });
  }
  
  private getNextZoomLevel(currentZoom: number): number {
    const config = this.radar.getConfig();
    const zoomLevels = config.zoomLevels;
    
    for (let i = 0; i < zoomLevels.length; i++) {
      if (zoomLevels[i] > currentZoom) {
        return zoomLevels[i];
      }
    }
    
    return zoomLevels[0];
  }
  
  public createPing(position: Point, type: PingType = PingType.ATTENTION): void {
    this.radar.pingSystem.createPing({
      id: this.radar.generateId(),
      type,
      position,
      playerId: this.radar.getCurrentPlayer()?.id || 'unknown',
      playerName: this.radar.getCurrentPlayer()?.name || 'Unknown',
      message: '',
      timestamp: Date.now(),
      duration: this.radar.getConfig().pingDuration,
      color: this.radar.getConfig().colors.ping,
      size: 10,
      fadeOut: true,
      isExpired: false,
      icon: '!',
    });
  }
}

export default RadarSystem;

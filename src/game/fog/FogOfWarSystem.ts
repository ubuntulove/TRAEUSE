import { EventEmitter } from '../../core/EventEmitter';
import { Grid } from '../../core/Grid';
import { Vector2 } from '../../core/Vector2';
import { Camera } from '../camera/Camera';

export enum VisibilityLevel {
  HIDDEN = 0,
  SHRINE = 1,
  CLOAKED = 2,
  VISIBLE = 3,
  FULLY_VISIBLE = 4
}

export enum FogType {
  NONE = 'none',
  BLACK = 'black',
  DARK = 'dark',
  EXPLORED = 'explored',
  REVEALED = 'revealed'
}

export interface FogState {
  visibilityGrid: Map<number, VisibilityLevel>;
  revealedAreas: RevealedArea[];
  shroudRemovedAreas: Set<string>;
  lastUpdate: number;
}

export interface PlayerFogState {
  playerId: number;
  visibilityGrid: Map<string, VisibilityLevel>;
  revealedAreas: RevealedArea[];
  shroudRemovedAreas: Set<string>;
  visionHistory: FogUpdate[];
  pendingUpdates: FogUpdate[];
}

export interface RevealedArea {
  id: string;
  x: number;
  y: number;
  radius: number;
  playerId: number;
  timestamp: number;
  permanent: boolean;
  centerX: number;
  centerY: number;
}

export interface FogCell {
  visibilityLevel: VisibilityLevel;
  lastSeen: number;
  isShroudRemoved: boolean;
  visibilitySources: Set<string>;
  terrainType: number;
  elevation: number;
}

export interface FogChunk {
  chunkX: number;
  chunkY: number;
  cells: FogCell[][];
  lastUpdate: number;
  isDirty: boolean;
  version: number;
}

export interface VisionModifier {
  id: string;
  type: 'unit' | 'building' | 'ability' | 'item' | 'buff';
  multiplier: number;
  flatBonus: number;
  priority: number;
  stacks: boolean;
  maxStacks: number;
  currentStacks: number;
  duration: number;
  startTime: number;
  sourceId: string;
  targetId: string;
}

export interface FogUpdate {
  type: 'visibility' | 'reveal' | 'shroud' | 'cloak' | 'detection';
  playerId: number;
  x: number;
  y: number;
  radius?: number;
  visibilityLevel?: VisibilityLevel;
  timestamp: number;
  sourceId?: string;
  sourceType?: 'unit' | 'building' | 'ability';
  reason?: string;
}

export interface RevealTrigger {
  id: string;
  type: 'area' | 'unit' | 'building' | 'event';
  x: number;
  y: number;
  radius: number;
  playerId: number;
  active: boolean;
  triggeredBy: Set<string>;
  onTrigger?: () => void;
}

export interface VisionRay {
  startX: number;
  startY: number;
  endX: number;
  endY: number;
  blocked: boolean;
  blockedAt?: Vector2;
  distance: number;
  steps: Vector2[];
}

export interface VisionResult {
  visible: boolean;
  visibilityLevel: VisibilityLevel;
  blockers: Vector2[];
  pathClear: boolean;
  actualDistance: number;
  maxDistance: number;
}

export interface StealthUnit {
  id: string;
  x: number;
  y: number;
  isStealthed: boolean;
  stealthAbilityId?: string;
  lastPositionUpdate: number;
  movementSpeed: number;
  detectionRadius: number;
}

export interface CloakedUnit {
  id: string;
  x: number;
  y: number;
  cloakLevel: number;
  isDetected: boolean;
  detectors: Set<string>;
  lastCloakUpdate: number;
  cloakAbilityId?: string;
}

export interface DetectionResult {
  detected: boolean;
  detectorId: string;
  detectedUnitId: string;
  detectionMethod: 'vision' | 'ability' | 'trap' | 'ally';
  detectionChance: number;
  distance: number;
}

export interface AllyVisionShare {
  allyId: number;
  shareEnabled: boolean;
  sharePercentage: number;
  sharedVisionGrid: Map<string, VisibilityLevel>;
  lastSync: number;
}

export interface UnitVisionProfile {
  unitId: string;
  baseVisionRange: number;
  currentVisionRange: number;
  visionType: 'normal' | 'infrared' | 'magic' | 'true';
  canSeeCloaked: boolean;
  canSeeStealthed: boolean;
  modifiers: VisionModifier[];
  hasLineOfSight: boolean;
  terrainModifiers: Map<number, number>;
}

export interface BuildingVisionProfile {
  buildingId: string;
  visionRange: number;
  visionType: 'normal' | 'tower' | 'watch' | 'omniscient';
  isActive: boolean;
  canDetectCloaked: boolean;
  canDetectStealthed: boolean;
  currentPowerLevel: number;
  requiredPowerLevel: number;
  modifiers: VisionModifier[];
}

export interface FogSyncMessage {
  type: 'full' | 'delta' | 'request' | 'response';
  playerId: number;
  timestamp: number;
  data: any;
  checksum?: string;
  sequenceNumber: number;
}

export interface PerformanceMetrics {
  lastCalculationTime: number;
  averageCalculationTime: number;
  chunkUpdatesPerSecond: number;
  visibilityChecksPerSecond: number;
  activeChunks: number;
  totalChunks: number;
  memoryUsage: number;
}

export interface FogEventMap {
  'visibilityChanged': { playerId: number; x: number; y: number; oldLevel: VisibilityLevel; newLevel: VisibilityLevel };
  'areaRevealed': { playerId: number; area: RevealedArea };
  'areaShroudRemoved': { playerId: number; x: number; y: number; radius: number };
  'unitDetected': { playerId: number; unitId: string; detectorId: string };
  'unitCloaked': { playerId: number; unitId: string };
  'unitDecloaked': { playerId: number; unitId: string; reason: string };
  'allyVisionShared': { playerId: number; allyId: number };
  'fogStateRestored': { playerId: number; timestamp: number };
  'chunkUpdated': { chunkX: number; chunkY: number };
}

export class FogCellImpl implements FogCell {
  visibilityLevel: VisibilityLevel;
  lastSeen: number;
  isShroudRemoved: boolean;
  visibilitySources: Set<string>;
  terrainType: number;
  elevation: number;

  constructor() {
    this.visibilityLevel = VisibilityLevel.HIDDEN;
    this.lastSeen = 0;
    this.isShroudRemoved = false;
    this.visibilitySources = new Set();
    this.terrainType = 0;
    this.elevation = 0;
  }

  copy(): FogCellImpl {
    const cell = new FogCellImpl();
    cell.visibilityLevel = this.visibilityLevel;
    cell.lastSeen = this.lastSeen;
    cell.isShroudRemoved = this.isShroudRemoved;
    cell.visibilitySources = new Set(this.visibilitySources);
    cell.terrainType = this.terrainType;
    cell.elevation = this.elevation;
    return cell;
  }

  reset(): void {
    this.visibilityLevel = VisibilityLevel.HIDDEN;
    this.lastSeen = 0;
    this.visibilitySources.clear();
  }
}

export class FogChunkImpl implements FogChunk {
  chunkX: number;
  chunkY: number;
  cells: FogCell[][];
  lastUpdate: number;
  isDirty: boolean;
  version: number;
  private chunkSize: number;

  constructor(chunkX: number, chunkY: number, chunkSize: number = 16) {
    this.chunkX = chunkX;
    this.chunkY = chunkY;
    this.chunkSize = chunkSize;
    this.cells = [];
    this.lastUpdate = 0;
    this.isDirty = false;
    this.version = 0;
    this.initializeCells();
  }

  private initializeCells(): void {
    for (let y = 0; y < this.chunkSize; y++) {
      this.cells[y] = [];
      for (let x = 0; x < this.chunkSize; x++) {
        this.cells[y][x] = new FogCellImpl();
      }
    }
  }

  getCell(localX: number, localY: number): FogCell {
    if (localX < 0 || localX >= this.chunkSize || localY < 0 || localY >= this.chunkSize) {
      throw new Error(`Invalid cell coordinates: (${localX}, ${localY})`);
    }
    return this.cells[localY][localX];
  }

  setCell(localX: number, localY: number, cell: FogCell): void {
    if (localX < 0 || localX >= this.chunkSize || localY < 0 || localY >= this.chunkSize) {
      return;
    }
    this.cells[localY][localX] = cell;
    this.markDirty();
  }

  getWorldX(): number {
    return this.chunkX * this.chunkSize;
  }

  getWorldY(): number {
    return this.chunkY * this.chunkSize;
  }

  markDirty(): void {
    this.isDirty = true;
    this.lastUpdate = Date.now();
    this.version++;
  }

  clearDirty(): void {
    this.isDirty = false;
  }

  reset(): void {
    for (let y = 0; y < this.chunkSize; y++) {
      for (let x = 0; x < this.chunkSize; x++) {
        this.cells[y][x].reset();
      }
    }
    this.markDirty();
  }

  copy(): FogChunkImpl {
    const chunk = new FogChunkImpl(this.chunkX, this.chunkY, this.chunkSize);
    chunk.lastUpdate = this.lastUpdate;
    chunk.isDirty = this.isDirty;
    chunk.version = this.version;
    for (let y = 0; y < this.chunkSize; y++) {
      for (let x = 0; x < this.chunkSize; x++) {
        chunk.cells[y][x] = this.cells[y][x].copy();
      }
    }
    return chunk;
  }
}

export class VisionModifierImpl implements VisionModifier {
  id: string;
  type: 'unit' | 'building' | 'ability' | 'item' | 'buff';
  multiplier: number;
  flatBonus: number;
  priority: number;
  stacks: boolean;
  maxStacks: number;
  currentStacks: number;
  duration: number;
  startTime: number;
  sourceId: string;
  targetId: string;

  constructor(
    id: string,
    type: VisionModifier['type'],
    multiplier: number = 1.0,
    flatBonus: number = 0,
    priority: number = 0,
    stacks: boolean = false,
    maxStacks: number = 1,
    duration: number = -1
  ) {
    this.id = id;
    this.type = type;
    this.multiplier = multiplier;
    this.flatBonus = flatBonus;
    this.priority = priority;
    this.stacks = stacks;
    this.maxStacks = maxStacks;
    this.currentStacks = stacks ? 1 : 0;
    this.duration = duration;
    this.startTime = Date.now();
    this.sourceId = '';
    this.targetId = '';
  }

  isExpired(): boolean {
    if (this.duration <= 0) return false;
    return Date.now() - this.startTime > this.duration;
  }

  addStack(): void {
    if (this.stacks && this.currentStacks < this.maxStacks) {
      this.currentStacks++;
    }
  }

  removeStack(): void {
    if (this.stacks && this.currentStacks > 0) {
      this.currentStacks--;
    }
  }

  getEffectiveMultiplier(): number {
    return this.multiplier * (this.stacks ? this.currentStacks : 1);
  }
}

export class FogUpdateImpl implements FogUpdate {
  type: 'visibility' | 'reveal' | 'shroud' | 'cloak' | 'detection';
  playerId: number;
  x: number;
  y: number;
  radius?: number;
  visibilityLevel?: VisibilityLevel;
  timestamp: number;
  sourceId?: string;
  sourceType?: 'unit' | 'building' | 'ability';
  reason?: string;

  constructor(
    type: FogUpdate['type'],
    playerId: number,
    x: number,
    y: number,
    timestamp: number,
    options?: Partial<FogUpdate>
  ) {
    this.type = type;
    this.playerId = playerId;
    this.x = x;
    this.y = y;
    this.timestamp = timestamp;
    if (options) {
      this.radius = options.radius;
      this.visibilityLevel = options.visibilityLevel;
      this.sourceId = options.sourceId;
      this.sourceType = options.sourceType;
      this.reason = options.reason;
    }
  }

  getKey(): string {
    return `${this.playerId}_${this.x}_${this.y}_${this.type}`;
  }
}

export class RevealTriggerImpl implements RevealTrigger {
  id: string;
  type: 'area' | 'unit' | 'building' | 'event';
  x: number;
  y: number;
  radius: number;
  playerId: number;
  active: boolean;
  triggeredBy: Set<string>;
  onTrigger?: () => void;

  constructor(
    id: string,
    type: RevealTrigger['type'],
    x: number,
    y: number,
    radius: number,
    playerId: number
  ) {
    this.id = id;
    this.type = type;
    this.x = x;
    this.y = y;
    this.radius = radius;
    this.playerId = playerId;
    this.active = true;
    this.triggeredBy = new Set();
  }

  isInside(x: number, y: number): boolean {
    const dx = x - this.x;
    const dy = y - this.y;
    return Math.sqrt(dx * dx + dy * dy) <= this.radius;
  }

  trigger(sourceId: string): void {
    if (!this.active) return;
    if (this.triggeredBy.has(sourceId)) return;
    
    this.triggeredBy.add(sourceId);
    if (this.onTrigger) {
      this.onTrigger();
    }
  }

  reset(): void {
    this.triggeredBy.clear();
    this.active = true;
  }
}

export class VisibilityTracker {
  private playerStates: Map<number, PlayerFogState>;
  private globalState: FogState;
  private maxHistorySize: number;

  constructor() {
    this.playerStates = new Map();
    this.globalState = this.createEmptyState();
    this.maxHistorySize = 1000;
  }

  private createEmptyState(): FogState {
    return {
      visibilityGrid: new Map(),
      revealedAreas: [],
      shroudRemovedAreas: new Set(),
      lastUpdate: Date.now()
    };
  }

  createPlayerState(playerId: number): PlayerFogState {
    const state: PlayerFogState = {
      playerId,
      visibilityGrid: new Map(),
      revealedAreas: [],
      shroudRemovedAreas: new Set(),
      visionHistory: [],
      pendingUpdates: []
    };
    this.playerStates.set(playerId, state);
    return state;
  }

  getPlayerState(playerId: number): PlayerFogState | undefined {
    return this.playerStates.get(playerId);
  }

  getOrCreatePlayerState(playerId: number): PlayerFogState {
    let state = this.playerStates.get(playerId);
    if (!state) {
      state = this.createPlayerState(playerId);
    }
    return state;
  }

  updateVisibility(playerId: number, x: number, y: number, level: VisibilityLevel, sourceId?: string): void {
    const state = this.getOrCreatePlayerState(playerId);
    const key = `${x},${y}`;
    
    const oldLevel = state.visibilityGrid.get(key) || VisibilityLevel.HIDDEN;
    state.visibilityGrid.set(key, level);

    if (level > VisibilityLevel.HIDDEN) {
      const update = new FogUpdateImpl('visibility', playerId, x, y, Date.now(), {
        visibilityLevel: level,
        sourceId
      });
      state.visionHistory.push(update);
      state.pendingUpdates.push(update);
      
      if (state.visionHistory.length > this.maxHistorySize) {
        state.visionHistory.shift();
      }
    }
  }

  getVisibility(playerId: number, x: number, y: number): VisibilityLevel {
    const state = this.playerStates.get(playerId);
    if (!state) return VisibilityLevel.HIDDEN;
    
    const key = `${x},${y}`;
    return state.visibilityGrid.get(key) || VisibilityLevel.HIDDEN;
  }

  addRevealedArea(playerId: number, area: RevealedArea): void {
    const state = this.getOrCreatePlayerState(playerId);
    state.revealedAreas.push(area);
    this.globalState.revealedAreas.push(area);
  }

  removeShroud(playerId: number, x: number, y: number): void {
    const state = this.getOrCreatePlayerState(playerId);
    const key = `${x},${y}`;
    state.shroudRemovedAreas.add(key);
    this.globalState.shroudRemovedAreas.add(key);
  }

  isShroudRemoved(playerId: number, x: number, y: number): boolean {
    const state = this.playerStates.get(playerId);
    if (!state) return false;
    
    const key = `${x},${y}`;
    return state.shroudRemovedAreas.has(key);
  }

  getPendingUpdates(playerId: number): FogUpdate[] {
    const state = this.playerStates.get(playerId);
    return state ? [...state.pendingUpdates] : [];
  }

  clearPendingUpdates(playerId: number): void {
    const state = this.playerStates.get(playerId);
    if (state) {
      state.pendingUpdates = [];
    }
  }

  getVisibleCells(playerId: number, minLevel: VisibilityLevel = VisibilityLevel.VISIBLE): Vector2[] {
    const state = this.playerStates.get(playerId);
    if (!state) return [];
    
    const visibleCells: Vector2[] = [];
    state.visibilityGrid.forEach((level, key) => {
      if (level >= minLevel) {
        const [x, y] = key.split(',').map(Number);
        visibleCells.push(new Vector2(x, y));
      }
    });
    return visibleCells;
  }

  getAllPlayerStates(): Map<number, PlayerFogState> {
    return new Map(this.playerStates);
  }

  clearPlayerState(playerId: number): void {
    this.playerStates.delete(playerId);
  }

  clearAll(): void {
    this.playerStates.clear();
    this.globalState = this.createEmptyState();
  }
}

export class VisionRangeCalculator {
  private terrainGrid: Grid<number>;
  private obstacleGrid: Grid<boolean>;
  private elevationGrid: Grid<number>;
  private defaultVisionRange: number = 10;

  constructor(
    mapWidth: number,
    mapHeight: number,
    chunkSize: number = 16
  ) {
    this.terrainGrid = new Grid<number>(mapWidth, mapHeight, 0);
    this.obstacleGrid = new Grid<boolean>(mapWidth, mapHeight, false);
    this.elevationGrid = new Grid<number>(mapWidth, mapHeight, 0);
  }

  setTerrain(x: number, y: number, terrainType: number): void {
    this.terrainGrid.set(x, y, terrainType);
  }

  setObstacle(x: number, y: number, isObstacle: boolean): void {
    this.obstacleGrid.set(x, y, isObstacle);
  }

  setElevation(x: number, y: number, elevation: number): void {
    this.elevationGrid.set(x, y, elevation);
  }

  calculateVisionRange(
    baseRange: number,
    modifiers: VisionModifier[],
    position: Vector2
  ): number {
    let effectiveRange = baseRange;

    for (const modifier of modifiers) {
      if (modifier.isExpired?.()) continue;
      effectiveRange *= modifier.getEffectiveMultiplier?.() ?? modifier.multiplier;
      effectiveRange += modifier.flatBonus;
    }

    const terrainBonus = this.getTerrainVisionBonus(position.x, position.y);
    effectiveRange *= terrainBonus;

    return Math.max(0, effectiveRange);
  }

  private getTerrainVisionBonus(x: number, y: number): number {
    const terrainType = this.terrainGrid.get(x, y);
    
    switch (terrainType) {
      case 1: return 1.2;
      case 2: return 0.8;
      case 3: return 1.5;
      case 4: return 0.5;
      default: return 1.0;
    }
  }

  hasLineOfSight(
    fromX: number,
    fromY: number,
    toX: number,
    toY: number,
    maxDistance?: number
  ): boolean {
    const dx = toX - fromX;
    const dy = toY - fromY;
    const distance = Math.sqrt(dx * dx + dy * dy);
    
    if (maxDistance !== undefined && distance > maxDistance) {
      return false;
    }

    const steps = Math.ceil(distance);
    if (steps === 0) return true;

    const stepX = dx / steps;
    const stepY = dy / steps;

    for (let i = 1; i < steps; i++) {
      const checkX = Math.floor(fromX + stepX * i);
      const checkY = Math.floor(fromY + stepY * i);

      if (this.obstacleGrid.get(checkX, checkY)) {
        const checkElevation = this.elevationGrid.get(checkX, checkY);
        const fromElevation = this.elevationGrid.get(fromX, fromY);
        
        if (checkElevation > fromElevation + 2) {
          return false;
        }
      }
    }

    return true;
  }

  getVisionRay(
    fromX: number,
    fromY: number,
    toX: number,
    toY: number,
    maxDistance?: number
  ): VisionRay {
    const dx = toX - fromX;
    const dy = toY - fromY;
    const distance = Math.sqrt(dx * dx + dy * dy);
    const steps: Vector2[] = [new Vector2(fromX, fromY)];
    
    let blocked = false;
    let blockedAt: Vector2 | undefined;
    let actualDistance = distance;

    if (maxDistance !== undefined && distance > maxDistance) {
      actualDistance = maxDistance;
    }

    const stepCount = Math.ceil(actualDistance);
    const stepX = dx / Math.max(1, distance);
    const stepY = dy / Math.max(1, distance);

    for (let i = 1; i <= stepCount; i++) {
      const checkX = Math.floor(fromX + stepX * i);
      const checkY = Math.floor(fromY + stepY * i);
      const currentPos = new Vector2(checkX, checkY);
      steps.push(currentPos);

      if (this.obstacleGrid.get(checkX, checkY)) {
        blocked = true;
        blockedAt = currentPos;
        actualDistance = Math.sqrt(
          (checkX - fromX) ** 2 + (checkY - fromY) ** 2
        );
        break;
      }
    }

    return {
      startX: fromX,
      startY: fromY,
      endX: toX,
      endY: toY,
      blocked,
      blockedAt,
      distance: actualDistance,
      steps
    };
  }

  getVisibleCellsInRange(
    centerX: number,
    centerY: number,
    range: number,
    checkLineOfSight: boolean = true
  ): Vector2[] {
    const visibleCells: Vector2[] = [];
    const rangeSq = range * range;
    const minX = Math.max(0, Math.floor(centerX - range));
    const maxX = Math.min(this.terrainGrid.width - 1, Math.ceil(centerX + range));
    const minY = Math.max(0, Math.floor(centerY - range));
    const maxY = Math.min(this.terrainGrid.height - 1, Math.ceil(centerY + range));

    for (let y = minY; y <= maxY; y++) {
      for (let x = minX; x <= maxX; x++) {
        const dx = x - centerX;
        const dy = y - centerY;
        const distanceSq = dx * dx + dy * dy;

        if (distanceSq <= rangeSq) {
          if (!checkLineOfSight || this.hasLineOfSight(centerX, centerY, x, y, range)) {
            visibleCells.push(new Vector2(x, y));
          }
        }
      }
    }

    return visibleCells;
  }

  getElevationDifference(x1: number, y1: number, x2: number, y2: number): number {
    return this.elevationGrid.get(x2, y2) - this.elevationGrid.get(x1, y1);
  }
}

export class StealthSystem {
  private stealthedUnits: Map<string, StealthUnit>;
  private detectionRanges: Map<string, number>;
  private stealthAbilities: Map<string, { duration: number; cooldown: number; lastUsed: number }>;
  private onStealthChanged?: (unitId: string, isStealthed: boolean) => void;

  constructor() {
    this.stealthedUnits = new Map();
    this.detectionRanges = new Map();
    this.stealthAbilities = new Map();
  }

  setStealthChangedCallback(callback: (unitId: string, isStealthed: boolean) => void): void {
    this.onStealthChanged = callback;
  }

  registerStealthAbility(
    abilityId: string,
    duration: number,
    cooldown: number
  ): void {
    this.stealthAbilities.set(abilityId, {
      duration,
      cooldown,
      lastUsed: 0
    });
  }

  enterStealth(unitId: string, x: number, y: number, abilityId?: string): void {
    const unit: StealthUnit = {
      id: unitId,
      x,
      y,
      isStealthed: true,
      stealthAbilityId: abilityId,
      lastPositionUpdate: Date.now(),
      movementSpeed: 0,
      detectionRadius: 5
    };
    this.stealthedUnits.set(unitId, unit);
    
    if (this.onStealthChanged) {
      this.onStealthChanged(unitId, true);
    }
  }

  exitStealth(unitId: string): void {
    if (this.stealthedUnits.has(unitId)) {
      this.stealthedUnits.delete(unitId);
      
      if (this.onStealthChanged) {
        this.onStealthChanged(unitId, false);
      }
    }
  }

  isStealthed(unitId: string): boolean {
    const unit = this.stealthedUnits.get(unitId);
    return unit ? unit.isStealthed : false;
  }

  getStealthedUnit(unitId: string): StealthUnit | undefined {
    return this.stealthedUnits.get(unitId);
  }

  updateUnitPosition(unitId: string, x: number, y: number): void {
    const unit = this.stealthedUnits.get(unitId);
    if (unit) {
      const dx = x - unit.x;
      const dy = y - unit.y;
      unit.movementSpeed = Math.sqrt(dx * dx + dy * dy);
      unit.x = x;
      unit.y = y;
      unit.lastPositionUpdate = Date.now();

      if (unit.movementSpeed > 2) {
        this.exitStealth(unitId);
      }
    }
  }

  isDetected(unitId: string, detectorX: number, detectorY: number): boolean {
    const unit = this.stealthedUnits.get(unitId);
    if (!unit) return false;

    const dx = unit.x - detectorX;
    const dy = unit.y - detectorY;
    const distance = Math.sqrt(dx * dx + dy * dy);

    return distance <= unit.detectionRadius;
  }

  getAllStealthedUnits(): StealthUnit[] {
    return Array.from(this.stealthedUnits.values());
  }

  setDetectionRadius(unitId: string, radius: number): void {
    this.detectionRanges.set(unitId, radius);
  }

  getDetectionRadius(unitId: string): number {
    return this.detectionRanges.get(unitId) || 5;
  }

  canUseStealthAbility(abilityId: string): boolean {
    const ability = this.stealthAbilities.get(abilityId);
    if (!ability) return false;
    
    const now = Date.now();
    return now - ability.lastUsed >= ability.cooldown;
  }

  useStealthAbility(abilityId: string): void {
    const ability = this.stealthAbilities.get(abilityId);
    if (ability) {
      ability.lastUsed = Date.now();
    }
  }

  clear(): void {
    this.stealthedUnits.clear();
    this.detectionRanges.clear();
  }
}

export class CloakDetector {
  private cloakedUnits: Map<string, CloakedUnit>;
  private detectorRanges: Map<string, number>;
  private detectionAbilities: Map<string, { range: number; detectionLevel: number }>;
  private onCloakStateChanged?: (unitId: string, isCloaked: boolean, isDetected: boolean) => void;

  constructor() {
    this.cloakedUnits = new Map();
    this.detectorRanges = new Map();
    this.detectionAbilities = new Map();
  }

  setCloakStateChangedCallback(
    callback: (unitId: string, isCloaked: boolean, isDetected: boolean) => void
  ): void {
    this.onCloakStateChanged = callback;
  }

  registerDetectionAbility(
    abilityId: string,
    range: number,
    detectionLevel: number
  ): void {
    this.detectionAbilities.set(abilityId, {
      range,
      detectionLevel
    });
  }

  enterCloak(unitId: string, x: number, y: number, cloakLevel: number = 1, abilityId?: string): void {
    const unit: CloakedUnit = {
      id: unitId,
      x,
      y,
      cloakLevel,
      isDetected: false,
      detectors: new Set(),
      lastCloakUpdate: Date.now(),
      cloakAbilityId: abilityId
    };
    this.cloakedUnits.set(unitId, unit);
    
    if (this.onCloakStateChanged) {
      this.onCloakStateChanged(unitId, true, false);
    }
  }

  exitCloak(unitId: string): void {
    if (this.cloakedUnits.has(unitId)) {
      this.cloakedUnits.delete(unitId);
      
      if (this.onCloakStateChanged) {
        this.onCloakStateChanged(unitId, false, false);
      }
    }
  }

  isCloaked(unitId: string): boolean {
    const unit = this.cloakedUnits.get(unitId);
    return unit ? unit.cloakLevel > 0 : false;
  }

  isDetected(unitId: string): boolean {
    const unit = this.cloakedUnits.get(unitId);
    return unit ? unit.isDetected : false;
  }

  getCloakedUnit(unitId: string): CloakedUnit | undefined {
    return this.cloakedUnits.get(unitId);
  }

  updateUnitPosition(unitId: string, x: number, y: number): void {
    const unit = this.cloakedUnits.get(unitId);
    if (unit) {
      unit.x = x;
      unit.y = y;
      unit.lastCloakUpdate = Date.now();
    }
  }

  checkDetection(
    detectorId: string,
    detectorX: number,
    detectorY: number,
    detectorRange: number,
    detectionLevel: number
  ): DetectionResult[] {
    const results: DetectionResult[] = [];

    this.cloakedUnits.forEach((unit, unitId) => {
      const dx = unit.x - detectorX;
      const dy = unit.y - detectorY;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance <= detectorRange) {
        const detectionChance = this.calculateDetectionChance(
          distance,
          detectorRange,
          unit.cloakLevel,
          detectionLevel
        );
        
        const detected = Math.random() < detectionChance;
        
        if (detected) {
          unit.isDetected = true;
          unit.detectors.add(detectorId);
        }

        results.push({
          detected,
          detectorId,
          detectedUnitId: unitId,
          detectionMethod: 'vision',
          detectionChance,
          distance
        });
      }
    });

    return results;
  }

  private calculateDetectionChance(
    distance: number,
    maxRange: number,
    cloakLevel: number,
    detectionLevel: number
  ): number {
    const distanceFactor = 1 - (distance / maxRange);
    const levelDifference = detectionLevel - cloakLevel;
    
    let chance = distanceFactor * 0.5 + (levelDifference * 0.1);
    
    return Math.max(0, Math.min(1, chance));
  }

  removeDetector(unitId: string, detectorId: string): void {
    const unit = this.cloakedUnits.get(unitId);
    if (unit) {
      unit.detectors.delete(detectorId);
      if (unit.detectors.size === 0) {
        unit.isDetected = false;
        
        if (this.onCloakStateChanged) {
          this.onCloakStateChanged(unitId, true, false);
        }
      }
    }
  }

  getAllCloakedUnits(): CloakedUnit[] {
    return Array.from(this.cloakedUnits.values());
  }

  getDetectedUnits(): CloakedUnit[] {
    return this.getAllCloakedUnits().filter(unit => unit.isDetected);
  }

  clearDetections(): void {
    this.cloakedUnits.forEach(unit => {
      unit.isDetected = false;
      unit.detectors.clear();
    });
  }

  clear(): void {
    this.cloakedUnits.clear();
    this.detectorRanges.clear();
    this.detectionAbilities.clear();
  }
}

export class AllySharing {
  private allyShares: Map<number, AllyVisionShare>;
  private playerId: number;

  constructor(playerId: number) {
    this.playerId = playerId;
    this.allyShares = new Map();
  }

  setPlayerId(playerId: number): void {
    this.playerId = playerId;
  }

  addAlly(allyId: number, sharePercentage: number = 100): void {
    const share: AllyVisionShare = {
      allyId,
      shareEnabled: true,
      sharePercentage,
      sharedVisionGrid: new Map(),
      lastSync: 0
    };
    this.allyShares.set(allyId, share);
  }

  removeAlly(allyId: number): void {
    this.allyShares.delete(allyId);
  }

  setShareEnabled(allyId: number, enabled: boolean): void {
    const share = this.allyShares.get(allyId);
    if (share) {
      share.shareEnabled = enabled;
    }
  }

  setSharePercentage(allyId: number, percentage: number): void {
    const share = this.allyShares.get(allyId);
    if (share) {
      share.sharePercentage = Math.max(0, Math.min(100, percentage));
    }
  }

  syncVision(allyId: number, visionGrid: Map<string, VisibilityLevel>): void {
    const share = this.allyShares.get(allyId);
    if (!share || !share.shareEnabled) return;

    const percentage = share.sharePercentage / 100;
    visionGrid.forEach((level, key) => {
      if (Math.random() < percentage) {
        share.sharedVisionGrid.set(key, level);
      }
    });
    
    share.lastSync = Date.now();
  }

  getSharedVision(allyId: number): Map<string, VisibilityLevel> {
    const share = this.allyShares.get(allyId);
    return share ? new Map(share.sharedVisionGrid) : new Map();
  }

  getAllSharedVision(): Map<string, VisibilityLevel> {
    const combinedVision = new Map<string, VisibilityLevel>();
    
    this.allyShares.forEach(share => {
      if (share.shareEnabled) {
        share.sharedVisionGrid.forEach((level, key) => {
          const existingLevel = combinedVision.get(key) || VisibilityLevel.HIDDEN;
          if (level > existingLevel) {
            combinedVision.set(key, level);
          }
        });
      }
    });

    return combinedVision;
  }

  isAlly(allyId: number): boolean {
    return this.allyShares.has(allyId);
  }

  isSharingEnabled(allyId: number): boolean {
    const share = this.allyShares.get(allyId);
    return share ? share.shareEnabled : false;
  }

  getAllAllies(): number[] {
    return Array.from(this.allyShares.keys());
  }

  getShareInfo(allyId: number): AllyVisionShare | undefined {
    return this.allyShares.get(allyId);
  }

  clear(): void {
    this.allyShares.clear();
  }
}

export class ShroudRemover {
  private removedAreas: Map<string, { x: number; y: number; radius: number; timestamp: number }>;
  private removeOperations: Map<string, { playerId: number; areaId: string }[]>;
  private maxUndoHistory: number = 10;

  constructor() {
    this.removedAreas = new Map();
    this.removeOperations = new Map();
  }

  removeShroud(x: number, y: number, radius: number, playerId: number, operationId?: string): string {
    const id = operationId || `shroud_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    this.removedAreas.set(id, {
      x,
      y,
      radius,
      timestamp: Date.now()
    });

    if (!this.removeOperations.has(playerId)) {
      this.removeOperations.set(playerId, []);
    }
    const operations = this.removeOperations.get(playerId)!;
    operations.push({ playerId, areaId: id });

    if (operations.length > this.maxUndoHistory) {
      const oldest = operations.shift();
      if (oldest) {
        this.removedAreas.delete(oldest.areaId);
      }
    }

    return id;
  }

  isShroudRemoved(x: number, y: number): boolean {
    for (const area of this.removedAreas.values()) {
      const dx = x - area.x;
      const dy = y - area.y;
      if (Math.sqrt(dx * dx + dy * dy) <= area.radius) {
        return true;
      }
    }
    return false;
  }

  getRemovedArea(id: string): { x: number; y: number; radius: number; timestamp: number } | undefined {
    return this.removedAreas.get(id);
  }

  undoLastRemoval(playerId: number): boolean {
    const operations = this.removeOperations.get(playerId);
    if (!operations || operations.length === 0) return false;

    const lastOperation = operations.pop();
    if (lastOperation) {
      this.removedAreas.delete(lastOperation.areaId);
      return true;
    }
    return false;
  }

  getRemovedAreasInRadius(x: number, y: number, radius: number): string[] {
    const areas: string[] = [];
    
    this.removedAreas.forEach((area, id) => {
      const dx = area.x - x;
      const dy = area.y - y;
      const distance = Math.sqrt(dx * dx + dy * dy);
      
      if (distance - area.radius <= radius) {
        areas.push(id);
      }
    });

    return areas;
  }

  clear(): void {
    this.removedAreas.clear();
    this.removeOperations.clear();
  }

  saveState(): string {
    const state = {
      removedAreas: Array.from(this.removedAreas.entries()),
      removeOperations: Array.from(this.removeOperations.entries())
    };
    return JSON.stringify(state);
  }

  loadState(stateJson: string): void {
    try {
      const state = JSON.parse(stateJson);
      this.removedAreas = new Map(state.removedAreas);
      this.removeOperations = new Map(state.removeOperations);
    } catch (e) {
      console.error('Failed to load shroud remover state:', e);
    }
  }
}

export class RevelationManager {
  private revealedAreas: Map<string, RevealedArea>;
  private activeReveals: Map<string, { endTime: number; area: RevealedArea }>;
  private revealAbilities: Map<string, { duration: number; cooldown: number; lastUsed: number }>;
  private pendingReveals: RevealedArea[];
  private onAreaRevealed?: (area: RevealedArea) => void;
  private onRevealExpired?: (areaId: string) => void;

  constructor() {
    this.revealedAreas = new Map();
    this.activeReveals = new Map();
    this.revealAbilities = new Map();
    this.pendingReveals = [];
  }

  setCallbacks(
    onAreaRevealed?: (area: RevealedArea) => void,
    onRevealExpired?: (areaId: string) => void
  ): void {
    this.onAreaRevealed = onAreaRevealed;
    this.onRevealExpired = onRevealExpired;
  }

  registerRevealAbility(
    abilityId: string,
    duration: number,
    cooldown: number
  ): void {
    this.revealAbilities.set(abilityId, {
      duration,
      cooldown,
      lastUsed: 0
    });
  }

  createReveal(
    x: number,
    y: number,
    radius: number,
    playerId: number,
    permanent: boolean = false,
    duration?: number
  ): RevealedArea {
    const area: RevealedArea = {
      id: `reveal_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      x,
      y,
      radius,
      playerId,
      timestamp: Date.now(),
      permanent,
      centerX: x,
      centerY: y
    };

    this.revealedAreas.set(area.id, area);

    if (!permanent && duration !== undefined && duration > 0) {
      this.activeReveals.set(area.id, {
        endTime: Date.now() + duration,
        area
      });
    }

    this.pendingReveals.push(area);

    if (this.onAreaRevealed) {
      this.onAreaRevealed(area);
    }

    return area;
  }

  removeReveal(areaId: string): boolean {
    const removed = this.revealedAreas.delete(areaId);
    this.activeReveals.delete(areaId);
    return removed;
  }

  isRevealed(x: number, y: number, playerId: number): boolean {
    for (const area of this.revealedAreas.values()) {
      if (area.playerId !== playerId) continue;
      
      const dx = x - area.x;
      const dy = y - area.y;
      if (Math.sqrt(dx * dx + dy * dy) <= area.radius) {
        return true;
      }
    }
    return false;
  }

  getVisibilityLevel(x: number, y: number, playerId: number): VisibilityLevel {
    let highestLevel = VisibilityLevel.HIDDEN;
    let distanceToClosest = Infinity;

    for (const area of this.revealedAreas.values()) {
      if (area.playerId !== playerId) continue;
      
      const dx = x - area.x;
      const dy = y - area.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance <= area.radius) {
        const normalizedDistance = distance / area.radius;
        let level = VisibilityLevel.VISIBLE;
        
        if (normalizedDistance < 0.25) {
          level = VisibilityLevel.FULLY_VISIBLE;
        } else if (normalizedDistance < 0.5) {
          level = VisibilityLevel.VISIBLE;
        } else if (normalizedDistance < 0.75) {
          level = VisibilityLevel.SHRINE;
        } else {
          level = VisibilityLevel.HIDDEN;
        }

        if (level > highestLevel) {
          highestLevel = level;
        }
      }

      distanceToClosest = Math.min(distanceToClosest, distance);
    }

    if (highestLevel === VisibilityLevel.HIDDEN && distanceToClosest < 2) {
      return VisibilityLevel.CLOAKED;
    }

    return highestLevel;
  }

  update(deltaTime: number): void {
    const now = Date.now();
    const expired: string[] = [];

    this.activeReveals.forEach((reveal, id) => {
      if (now >= reveal.endTime) {
        this.revealedAreas.delete(id);
        expired.push(id);
      }
    });

    expired.forEach(id => {
      this.activeReveals.delete(id);
      if (this.onRevealExpired) {
        this.onRevealExpired(id);
      }
    });
  }

  getRevealedAreas(playerId: number): RevealedArea[] {
    return Array.from(this.revealedAreas.values()).filter(
      area => area.playerId === playerId
    );
  }

  getAllRevealedAreas(): RevealedArea[] {
    return Array.from(this.revealedAreas.values());
  }

  getPendingReveals(): RevealedArea[] {
    const pending = [...this.pendingReveals];
    this.pendingReveals = [];
    return pending;
  }

  canUseRevealAbility(abilityId: string): boolean {
    const ability = this.revealAbilities.get(abilityId);
    if (!ability) return false;
    
    const now = Date.now();
    return now - ability.lastUsed >= ability.cooldown;
  }

  useRevealAbility(abilityId: string): void {
    const ability = this.revealAbilities.get(abilityId);
    if (ability) {
      ability.lastUsed = Date.now();
    }
  }

  clear(): void {
    this.revealedAreas.clear();
    this.activeReveals.clear();
    this.pendingReveals = [];
  }

  saveState(): string {
    return JSON.stringify({
      revealedAreas: Array.from(this.revealedAreas.entries()),
      activeReveals: Array.from(this.activeReveals.entries()).map(([id, data]) => ({
        id,
        endTime: data.endTime,
        area: data.area
      }))
    });
  }

  loadState(stateJson: string): void {
    try {
      const state = JSON.parse(stateJson);
      this.revealedAreas = new Map(state.revealedAreas);
      this.activeReveals = new Map(
        state.activeReveals.map((item: any) => [
          item.id,
          { endTime: item.endTime, area: item.area }
        ])
      );
    } catch (e) {
      console.error('Failed to load revelation manager state:', e);
    }
  }
}

export class FogStateManager {
  private savedStates: Map<number, { state: FogState; timestamp: number }>;
  private undoStack: Map<number, FogState[]>;
  private redoStack: Map<number, FogState[]>;
  private maxUndoLevels: number;

  constructor(maxUndoLevels: number = 10) {
    this.savedStates = new Map();
    this.undoStack = new Map();
    this.redoStack = new Map();
    this.maxUndoLevels = maxUndoLevels;
  }

  saveState(playerId: number, state: FogState): void {
    const stateCopy = this.deepCopyState(state);
    this.savedStates.set(playerId, {
      state: stateCopy,
      timestamp: Date.now()
    });

    if (!this.undoStack.has(playerId)) {
      this.undoStack.set(playerId, []);
    }
    const undoStack = this.undoStack.get(playerId)!;
    undoStack.push(stateCopy);

    if (undoStack.length > this.maxUndoLevels) {
      undoStack.shift();
    }

    this.redoStack.set(playerId, []);
  }

  getSavedState(playerId: number): FogState | undefined {
    const saved = this.savedStates.get(playerId);
    return saved ? this.deepCopyState(saved.state) : undefined;
  }

  undo(playerId: number, currentState: FogState): FogState | undefined {
    const undoStack = this.undoStack.get(playerId);
    if (!undoStack || undoStack.length === 0) return undefined;

    const previousState = undoStack.pop()!;
    
    if (!this.redoStack.has(playerId)) {
      this.redoStack.set(playerId, []);
    }
    const redoStack = this.redoStack.get(playerId)!;
    redoStack.push(this.deepCopyState(currentState));

    return this.deepCopyState(previousState);
  }

  redo(playerId: number, currentState: FogState): FogState | undefined {
    const redoStack = this.redoStack.get(playerId);
    if (!redoStack || redoStack.length === 0) return undefined;

    const nextState = redoStack.pop()!;
    
    if (!this.undoStack.has(playerId)) {
      this.undoStack.set(playerId, []);
    }
    const undoStack = this.undoStack.get(playerId)!;
    undoStack.push(this.deepCopyState(currentState));

    return this.deepCopyState(nextState);
  }

  canUndo(playerId: number): boolean {
    const undoStack = this.undoStack.get(playerId);
    return undoStack !== undefined && undoStack.length > 0;
  }

  canRedo(playerId: number): boolean {
    const redoStack = this.redoStack.get(playerId);
    return redoStack !== undefined && redoStack.length > 0;
  }

  clearHistory(playerId: number): void {
    this.undoStack.delete(playerId);
    this.redoStack.delete(playerId);
  }

  clearAllHistory(): void {
    this.undoStack.clear();
    this.redoStack.clear();
  }

  private deepCopyState(state: FogState): FogState {
    return {
      visibilityGrid: new Map(state.visibilityGrid),
      revealedAreas: state.revealedAreas.map(area => ({ ...area })),
      shroudRemovedAreas: new Set(state.shroudRemovedAreas),
      lastUpdate: state.lastUpdate
    };
  }

  exportState(playerId: number): string | undefined {
    const state = this.getSavedState(playerId);
    return state ? JSON.stringify(state) : undefined;
  }

  importState(playerId: number, stateJson: string): boolean {
    try {
      const state = JSON.parse(stateJson) as FogState;
      this.saveState(playerId, state);
      return true;
    } catch (e) {
      console.error('Failed to import fog state:', e);
      return false;
    }
  }
}

export class FogEventHandler extends EventEmitter<FogEventMap> {
  private eventQueue: Array<{ event: string; data: any; timestamp: number }>;
  private maxQueueSize: number;
  private listeners: Map<string, Set<Function>>;

  constructor(maxQueueSize: number = 1000) {
    super();
    this.eventQueue = [];
    this.maxQueueSize = maxQueueSize;
    this.listeners = new Map();
  }

  emitVisibilityChanged(
    playerId: number,
    x: number,
    y: number,
    oldLevel: VisibilityLevel,
    newLevel: VisibilityLevel
  ): void {
    this.queueEvent('visibilityChanged', { playerId, x, y, oldLevel, newLevel });
  }

  emitAreaRevealed(playerId: number, area: RevealedArea): void {
    this.queueEvent('areaRevealed', { playerId, area });
  }

  emitAreaShroudRemoved(playerId: number, x: number, y: number, radius: number): void {
    this.queueEvent('areaShroudRemoved', { playerId, x, y, radius });
  }

  emitUnitDetected(playerId: number, unitId: string, detectorId: string): void {
    this.queueEvent('unitDetected', { playerId, unitId, detectorId });
  }

  emitUnitCloaked(playerId: number, unitId: string): void {
    this.queueEvent('unitCloaked', { playerId, unitId });
  }

  emitUnitDecloaked(playerId: number, unitId: string, reason: string): void {
    this.queueEvent('unitDecloaked', { playerId, unitId, reason });
  }

  emitAllyVisionShared(playerId: number, allyId: number): void {
    this.queueEvent('allyVisionShared', { playerId, allyId });
  }

  emitFogStateRestored(playerId: number, timestamp: number): void {
    this.queueEvent('fogStateRestored', { playerId, timestamp });
  }

  emitChunkUpdated(chunkX: number, chunkY: number): void {
    this.queueEvent('chunkUpdated', { chunkX, chunkY });
  }

  private queueEvent(event: string, data: any): void {
    this.eventQueue.push({
      event,
      data,
      timestamp: Date.now()
    });

    if (this.eventQueue.length > this.maxQueueSize) {
      this.eventQueue.shift();
    }

    this.emit(event as keyof FogEventMap, data);
  }

  processEventQueue(): void {
    const events = [...this.eventQueue];
    this.eventQueue = [];
    
    events.forEach(({ event, data }) => {
      this.emit(event as keyof FogEventMap, data);
    });
  }

  getEventQueue(): Array<{ event: string; data: any; timestamp: number }> {
    return [...this.eventQueue];
  }

  clearEventQueue(): void {
    this.eventQueue = [];
  }

  subscribe(event: string, callback: Function): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);

    return () => {
      const eventListeners = this.listeners.get(event);
      if (eventListeners) {
        eventListeners.delete(callback);
      }
    };
  }

  getRecentEvents(count: number = 10): Array<{ event: string; data: any; timestamp: number }> {
    return this.eventQueue.slice(-count);
  }
}

export class MapFog {
  private fogCells: Map<string, FogCell>;
  private mapWidth: number;
  private mapHeight: number;
  private defaultVisibility: VisibilityLevel;
  private terrainModifiers: Map<number, number>;

  constructor(mapWidth: number, mapHeight: number, defaultVisibility: VisibilityLevel = VisibilityLevel.HIDDEN) {
    this.mapWidth = mapWidth;
    this.mapHeight = mapHeight;
    this.defaultVisibility = defaultVisibility;
    this.fogCells = new Map();
    this.terrainModifiers = new Map();
    
    this.initializeFog();
  }

  private initializeFog(): void {
    for (let y = 0; y < this.mapHeight; y++) {
      for (let x = 0; x < this.mapWidth; x++) {
        const key = this.getCellKey(x, y);
        const cell = new FogCellImpl();
        cell.visibilityLevel = this.defaultVisibility;
        this.fogCells.set(key, cell);
      }
    }
  }

  private getCellKey(x: number, y: number): string {
    return `${x},${y}`;
  }

  getCell(x: number, y: number): FogCell | undefined {
    return this.fogCells.get(this.getCellKey(x, y));
  }

  setCell(x: number, y: number, cell: FogCell): void {
    this.fogCells.set(this.getCellKey(x, y), cell);
  }

  setVisibility(x: number, y: number, level: VisibilityLevel): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.visibilityLevel = level;
      cell.lastSeen = Date.now();
    }
  }

  getVisibility(x: number, y: number): VisibilityLevel {
    const cell = this.getCell(x, y);
    return cell ? cell.visibilityLevel : this.defaultVisibility;
  }

  addVisibilitySource(x: number, y: number, sourceId: string): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.visibilitySources.add(sourceId);
    }
  }

  removeVisibilitySource(x: number, y: number, sourceId: string): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.visibilitySources.delete(sourceId);
    }
  }

  setTerrainType(x: number, y: number, terrainType: number): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.terrainType = terrainType;
    }
  }

  setElevation(x: number, y: number, elevation: number): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.elevation = elevation;
    }
  }

  setShroudRemoved(x: number, y: number, removed: boolean): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.isShroudRemoved = removed;
    }
  }

  isShroudRemoved(x: number, y: number): boolean {
    const cell = this.getCell(x, y);
    return cell ? cell.isShroudRemoved : false;
  }

  registerTerrainModifier(terrainType: number, modifier: number): void {
    this.terrainModifiers.set(terrainType, modifier);
  }

  getTerrainModifier(terrainType: number): number {
    return this.terrainModifiers.get(terrainType) || 1.0;
  }

  resetVisibility(): void {
    this.fogCells.forEach(cell => {
      cell.visibilityLevel = this.defaultVisibility;
      cell.visibilitySources.clear();
    });
  }

  resetCell(x: number, y: number): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.visibilityLevel = this.defaultVisibility;
      cell.lastSeen = 0;
      cell.visibilitySources.clear();
    }
  }

  getVisibleCells(minLevel: VisibilityLevel = VisibilityLevel.VISIBLE): Vector2[] {
    const visibleCells: Vector2[] = [];
    
    this.fogCells.forEach((cell, key) => {
      if (cell.visibilityLevel >= minLevel) {
        const [x, y] = key.split(',').map(Number);
        visibleCells.push(new Vector2(x, y));
      }
    });

    return visibleCells;
  }

  exportState(): string {
    const state: any = {
      mapWidth: this.mapWidth,
      mapHeight: this.mapHeight,
      cells: []
    };

    this.fogCells.forEach((cell, key) => {
      state.cells.push({
        key,
        visibilityLevel: cell.visibilityLevel,
        lastSeen: cell.lastSeen,
        isShroudRemoved: cell.isShroudRemoved,
        terrainType: cell.terrainType,
        elevation: cell.elevation
      });
    });

    return JSON.stringify(state);
  }

  importState(stateJson: string): void {
    try {
      const state = JSON.parse(stateJson);
      this.mapWidth = state.mapWidth;
      this.mapHeight = state.mapHeight;
      this.fogCells.clear();

      state.cells.forEach((cellData: any) => {
        const cell = new FogCellImpl();
        cell.visibilityLevel = cellData.visibilityLevel;
        cell.lastSeen = cellData.lastSeen;
        cell.isShroudRemoved = cellData.isShroudRemoved;
        cell.terrainType = cellData.terrainType;
        cell.elevation = cellData.elevation;
        this.fogCells.set(cellData.key, cell);
      });
    } catch (e) {
      console.error('Failed to import map fog state:', e);
    }
  }
}

export class UnitVision {
  private unitProfiles: Map<string, UnitVisionProfile>;
  private activeUnits: Set<string>;
  private onVisionChanged?: (unitId: string, oldRange: number, newRange: number) => void;

  constructor() {
    this.unitProfiles = new Map();
    this.activeUnits = new Set();
  }

  setVisionChangedCallback(
    callback: (unitId: string, oldRange: number, newRange: number) => void
  ): void {
    this.onVisionChanged = callback;
  }

  registerUnit(
    unitId: string,
    baseVisionRange: number,
    visionType: UnitVisionProfile['visionType'] = 'normal'
  ): UnitVisionProfile {
    const profile: UnitVisionProfile = {
      unitId,
      baseVisionRange,
      currentVisionRange: baseVisionRange,
      visionType,
      canSeeCloaked: false,
      canSeeStealthed: false,
      modifiers: [],
      hasLineOfSight: true,
      terrainModifiers: new Map()
    };

    this.unitProfiles.set(unitId, profile);
    this.activeUnits.add(unitId);
    
    return profile;
  }

  unregisterUnit(unitId: string): void {
    this.unitProfiles.delete(unitId);
    this.activeUnits.delete(unitId);
  }

  getProfile(unitId: string): UnitVisionProfile | undefined {
    return this.unitProfiles.get(unitId);
  }

  updateVisionRange(unitId: string, range: number): void {
    const profile = this.unitProfiles.get(unitId);
    if (profile) {
      const oldRange = profile.currentVisionRange;
      profile.currentVisionRange = Math.max(0, range);
      
      if (this.onVisionChanged && oldRange !== profile.currentVisionRange) {
        this.onVisionChanged(unitId, oldRange, profile.currentVisionRange);
      }
    }
  }

  addModifier(unitId: string, modifier: VisionModifier): void {
    const profile = this.unitProfiles.get(unitId);
    if (profile) {
      profile.modifiers.push(modifier);
      this.recalculateVisionRange(unitId);
    }
  }

  removeModifier(unitId: string, modifierId: string): void {
    const profile = this.unitProfiles.get(unitId);
    if (profile) {
      profile.modifiers = profile.modifiers.filter(m => m.id !== modifierId);
      this.recalculateVisionRange(unitId);
    }
  }

  private recalculateVisionRange(unitId: string): void {
    const profile = this.unitProfiles.get(unitId);
    if (!profile) return;

    let effectiveRange = profile.baseVisionRange;

    profile.modifiers.forEach(modifier => {
      effectiveRange *= modifier.getEffectiveMultiplier();
      effectiveRange += modifier.flatBonus;
    });

    profile.terrainModifiers.forEach((modifier, terrainType) => {
      if (modifier !== 1.0) {
        effectiveRange *= modifier;
      }
    });

    this.updateVisionRange(unitId, effectiveRange);
  }

  setCanSeeCloaked(unitId: string, canSee: boolean): void {
    const profile = this.unitProfiles.get(unitId);
    if (profile) {
      profile.canSeeCloaked = canSee;
    }
  }

  setCanSeeStealthed(unitId: string, canSee: boolean): void {
    const profile = this.unitProfiles.get(unitId);
    if (profile) {
      profile.canSeeStealthed = canSee;
    }
  }

  setHasLineOfSight(unitId: string, hasLos: boolean): void {
    const profile = this.unitProfiles.get(unitId);
    if (profile) {
      profile.hasLineOfSight = hasLos;
    }
  }

  getActiveUnits(): string[] {
    return Array.from(this.activeUnits);
  }

  clear(): void {
    this.unitProfiles.clear();
    this.activeUnits.clear();
  }
}

export class BuildingVision {
  private buildingProfiles: Map<string, BuildingVisionProfile>;
  private activeBuildings: Set<string>;

  constructor() {
    this.buildingProfiles = new Map();
    this.activeBuildings = new Set();
  }

  registerBuilding(
    buildingId: string,
    visionRange: number,
    visionType: BuildingVisionProfile['visionType'] = 'normal'
  ): BuildingVisionProfile {
    const profile: BuildingVisionProfile = {
      buildingId,
      visionRange,
      visionType,
      isActive: true,
      canDetectCloaked: false,
      canDetectStealthed: false,
      currentPowerLevel: 100,
      requiredPowerLevel: 0,
      modifiers: []
    };

    this.buildingProfiles.set(buildingId, profile);
    this.activeBuildings.add(buildingId);
    
    return profile;
  }

  unregisterBuilding(buildingId: string): void {
    this.buildingProfiles.delete(buildingId);
    this.activeBuildings.delete(buildingId);
  }

  getProfile(buildingId: string): BuildingVisionProfile | undefined {
    return this.buildingProfiles.get(buildingId);
  }

  setActive(buildingId: string, active: boolean): void {
    const profile = this.buildingProfiles.get(buildingId);
    if (profile) {
      profile.isActive = active;
    }
  }

  isActive(buildingId: string): boolean {
    const profile = this.buildingProfiles.get(buildingId);
    return profile ? profile.isActive : false;
  }

  setPowerLevel(buildingId: string, powerLevel: number): void {
    const profile = this.buildingProfiles.get(buildingId);
    if (profile) {
      profile.currentPowerLevel = powerLevel;
    }
  }

  setRequiredPowerLevel(buildingId: string, required: number): void {
    const profile = this.buildingProfiles.get(buildingId);
    if (profile) {
      profile.requiredPowerLevel = required;
    }
  }

  hasSufficientPower(buildingId: string): boolean {
    const profile = this.buildingProfiles.get(buildingId);
    if (!profile) return false;
    return profile.currentPowerLevel >= profile.requiredPowerLevel;
  }

  getEffectiveVisionRange(buildingId: string): number {
    const profile = this.buildingProfiles.get(buildingId);
    if (!profile || !profile.isActive || !this.hasSufficientPower(buildingId)) {
      return 0;
    }

    let effectiveRange = profile.visionRange;

    profile.modifiers.forEach(modifier => {
      effectiveRange *= modifier.getEffectiveMultiplier();
      effectiveRange += modifier.flatBonus;
    });

    return Math.max(0, effectiveRange);
  }

  addModifier(buildingId: string, modifier: VisionModifier): void {
    const profile = this.buildingProfiles.get(buildingId);
    if (profile) {
      profile.modifiers.push(modifier);
    }
  }

  removeModifier(buildingId: string, modifierId: string): void {
    const profile = this.buildingProfiles.get(buildingId);
    if (profile) {
      profile.modifiers = profile.modifiers.filter(m => m.id !== modifierId);
    }
  }

  setCanDetectCloaked(buildingId: string, canDetect: boolean): void {
    const profile = this.buildingProfiles.get(buildingId);
    if (profile) {
      profile.canDetectCloaked = canDetect;
    }
  }

  setCanDetectStealthed(buildingId: string, canDetect: boolean): void {
    const profile = this.buildingProfiles.get(buildingId);
    if (profile) {
      profile.canDetectStealthed = canDetect;
    }
  }

  getActiveBuildings(): string[] {
    return Array.from(this.activeBuildings);
  }

  getVisibleBuildings(): string[] {
    return Array.from(this.activeBuildings).filter(id => this.isActive(id) && this.hasSufficientPower(id));
  }

  clear(): void {
    this.buildingProfiles.clear();
    this.activeBuildings.clear();
  }
}

export class FogRenderer {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private fogTexture: ImageData | null;
  private fogColors: Map<VisibilityLevel, string>;
  private fogAlphas: Map<VisibilityLevel, number>;
  private gradientCache: Map<string, CanvasGradient>;
  private useGradients: boolean;
  private renderQuality: 'low' | 'medium' | 'high';

  constructor(canvas: HTMLCanvasElement, quality: 'low' | 'medium' | 'high' = 'medium') {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
    this.fogTexture = null;
    this.fogColors = new Map();
    this.fogAlphas = new Map();
    this.gradientCache = new Map();
    this.useGradients = true;
    this.renderQuality = quality;
    
    this.initializeDefaultColors();
  }

  private initializeDefaultColors(): void {
    this.fogColors.set(VisibilityLevel.HIDDEN, '#000000');
    this.fogColors.set(VisibilityLevel.SHRINE, '#1a1a2e');
    this.fogColors.set(VisibilityLevel.CLOAKED, '#2d2d44');
    this.fogColors.set(VisibilityLevel.VISIBLE, '#3d3d5c');
    this.fogColors.set(VisibilityLevel.FULLY_VISIBLE, '#ffffff');

    this.fogAlphas.set(VisibilityLevel.HIDDEN, 1.0);
    this.fogAlphas.set(VisibilityLevel.SHRINE, 0.7);
    this.fogAlphas.set(VisibilityLevel.CLOAKED, 0.5);
    this.fogAlphas.set(VisibilityLevel.VISIBLE, 0.2);
    this.fogAlphas.set(VisibilityLevel.FULLY_VISIBLE, 0.0);
  }

  setColor(level: VisibilityLevel, color: string): void {
    this.fogColors.set(level, color);
  }

  setAlpha(level: VisibilityLevel, alpha: number): void {
    this.fogAlphas.set(level, Math.max(0, Math.min(1, alpha)));
  }

  setUseGradients(useGradients: boolean): void {
    this.useGradients = useGradients;
  }

  setRenderQuality(quality: 'low' | 'medium' | 'high'): void {
    this.renderQuality = quality;
  }

  createGradient(
    x: number,
    y: number,
    radius: number,
    innerColor: string,
    outerColor: string
  ): CanvasGradient {
    const cacheKey = `${x},${y},${radius},${innerColor},${outerColor}`;
    
    if (this.gradientCache.has(cacheKey)) {
      return this.gradientCache.get(cacheKey)!;
    }

    const gradient = this.ctx.createRadialGradient(x, y, 0, x, y, radius);
    gradient.addColorStop(0, innerColor);
    gradient.addColorStop(1, outerColor);

    if (this.gradientCache.size > 100) {
      this.gradientCache.clear();
    }
    this.gradientCache.set(cacheKey, gradient);

    return gradient;
  }

  renderFog(
    fogData: Map<string, VisibilityLevel>,
    camera: Camera,
    cellSize: number = 32
  ): void {
    const startX = Math.floor(camera.x / cellSize);
    const startY = Math.floor(camera.y / cellSize);
    const endX = Math.ceil((camera.x + this.canvas.width) / cellSize);
    const endY = Math.ceil((camera.y + this.canvas.height) / cellSize);

    this.ctx.save();
    this.ctx.globalCompositeOperation = 'source-over';

    for (let y = startY; y <= endY; y++) {
      for (let x = startX; x <= endX; x++) {
        const key = `${x},${y}`;
        const visibility = fogData.get(key);
        
        if (visibility !== undefined && visibility !== VisibilityLevel.FULLY_VISIBLE) {
          this.renderCell(x, y, visibility, cellSize, camera);
        }
      }
    }

    this.ctx.restore();
  }

  private renderCell(
    worldX: number,
    worldY: number,
    visibility: VisibilityLevel,
    cellSize: number,
    camera: Camera
  ): void {
    const screenX = worldX * cellSize - camera.x;
    const screenY = worldY * cellSize - camera.y;

    const alpha = this.fogAlphas.get(visibility) || 1.0;
    const color = this.fogColors.get(visibility) || '#000000';

    if (this.useGradients && visibility === VisibilityLevel.VISIBLE) {
      const centerX = screenX + cellSize / 2;
      const centerY = screenY + cellSize / 2;
      const gradient = this.createGradient(
        centerX,
        centerY,
        cellSize,
        `rgba(61, 61, 92, ${alpha * 0.5})`,
        `rgba(0, 0, 0, ${alpha})`
      );
      this.ctx.fillStyle = gradient;
    } else {
      this.ctx.globalAlpha = alpha;
      this.ctx.fillStyle = color;
    }

    const qualityMultiplier = this.renderQuality === 'high' ? 1 : this.renderQuality === 'medium' ? 2 : 4;
    const renderSize = cellSize / qualityMultiplier;

    this.ctx.fillRect(
      Math.floor(screenX / renderSize) * renderSize,
      Math.floor(screenY / renderSize) * renderSize,
      renderSize + 1,
      renderSize + 1
    );

    this.ctx.globalAlpha = 1.0;
  }

  renderAreaReveal(
    x: number,
    y: number,
    radius: number,
    camera: Camera,
    cellSize: number = 32
  ): void {
    const screenX = x * cellSize - camera.x;
    const screenY = y * cellSize - camera.y;

    this.ctx.save();
    this.ctx.globalCompositeOperation = 'destination-out';
    
    const gradient = this.ctx.createRadialGradient(
      screenX,
      screenY,
      0,
      screenX,
      screenY,
      radius * cellSize
    );
    gradient.addColorStop(0, 'rgba(0, 0, 0, 1)');
    gradient.addColorStop(0.7, 'rgba(0, 0, 0, 0.8)');
    gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');

    this.ctx.fillStyle = gradient;
    this.ctx.beginPath();
    this.ctx.arc(screenX, screenY, radius * cellSize, 0, Math.PI * 2);
    this.ctx.fill();

    this.ctx.restore();
  }

  clearFog(): void {
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
  }

  clearCache(): void {
    this.gradientCache.clear();
  }

  getImageData(): ImageData {
    return this.ctx.getImageData(0, 0, this.canvas.width, this.canvas.height);
  }

  putImageData(data: ImageData): void {
    this.ctx.putImageData(data, 0, 0);
  }
}

export class MultiplayerSynchronizer {
  private playerId: number;
  private sequenceNumber: number;
  private pendingAcks: Map<number, { timestamp: number; data: FogSyncMessage }>;
  private syncInterval: number;
  private lastSyncTime: number;
  private compressionEnabled: boolean;
  private deltaHistory: Map<string, FogUpdate[]>;

  constructor(playerId: number, syncInterval: number = 1000) {
    this.playerId = playerId;
    this.sequenceNumber = 0;
    this.pendingAcks = new Map();
    this.syncInterval = syncInterval;
    this.lastSyncTime = 0;
    this.compressionEnabled = true;
    this.deltaHistory = new Map();
  }

  setPlayerId(playerId: number): void {
    this.playerId = playerId;
  }

  generateFullSync(state: FogState): FogSyncMessage {
    const message: FogSyncMessage = {
      type: 'full',
      playerId: this.playerId,
      timestamp: Date.now(),
      data: this.serializeState(state),
      sequenceNumber: ++this.sequenceNumber
    };

    this.pendingAcks.set(message.sequenceNumber, {
      timestamp: message.timestamp,
      data: message
    });

    return message;
  }

  generateDeltaSync(updates: FogUpdate[]): FogSyncMessage {
    const compressedUpdates = this.compressionEnabled
      ? this.compressUpdates(updates)
      : updates;

    const message: FogSyncMessage = {
      type: 'delta',
      playerId: this.playerId,
      timestamp: Date.now(),
      data: compressedUpdates,
      sequenceNumber: ++this.sequenceNumber
    };

    this.pendingAcks.set(message.sequenceNumber, {
      timestamp: message.timestamp,
      data: message
    });

    this.deltaHistory.set(message.sequenceNumber.toString(), updates);

    return message;
  }

  generateSyncRequest(targetPlayerId: number): FogSyncMessage {
    return {
      type: 'request',
      playerId: this.playerId,
      timestamp: Date.now(),
      data: { requestedPlayerId: targetPlayerId },
      sequenceNumber: ++this.sequenceNumber
    };
  }

  processSyncResponse(message: FogSyncMessage): FogUpdate[] {
    if (message.type === 'full') {
      return this.deserializeState(message.data);
    } else if (message.type === 'delta') {
      return this.decompressUpdates(message.data);
    } else if (message.type === 'response') {
      return this.deserializeState(message.data);
    }
    return [];
  }

  acknowledgeMessage(sequenceNumber: number): boolean {
    return this.pendingAcks.delete(sequenceNumber);
  }

  getPendingMessages(): FogSyncMessage[] {
    return Array.from(this.pendingAcks.values()).map(entry => entry.data);
  }

  getUnacknowledgedCount(): number {
    return this.pendingAcks.size;
  }

  shouldSync(): boolean {
    const now = Date.now();
    return now - this.lastSyncTime >= this.syncInterval;
  }

  markSyncComplete(): void {
    this.lastSyncTime = Date.now();
  }

  setCompressionEnabled(enabled: boolean): void {
    this.compressionEnabled = enabled;
  }

  private serializeState(state: FogState): any {
    return {
      visibilityGrid: Array.from(state.visibilityGrid.entries()),
      revealedAreas: state.revealedAreas,
      shroudRemovedAreas: Array.from(state.shroudRemovedAreas),
      lastUpdate: state.lastUpdate
    };
  }

  private deserializeState(data: any): FogUpdate[] {
    const updates: FogUpdate[] = [];
    
    data.visibilityGrid.forEach(([key, level]: [string, VisibilityLevel]) => {
      const [x, y] = key.split(',').map(Number);
      updates.push({
        type: 'visibility',
        playerId: this.playerId,
        x,
        y,
        visibilityLevel: level,
        timestamp: Date.now()
      });
    });

    return updates;
  }

  private compressUpdates(updates: FogUpdate[]): FogUpdate[] {
    if (updates.length <= 1) return updates;

    const compressed: FogUpdate[] = [];
    let lastUpdate: FogUpdate | null = null;

    for (const update of updates) {
      if (lastUpdate && this.isSimilar(lastUpdate, update)) {
        if (lastUpdate.visibilityLevel === update.visibilityLevel) {
          continue;
        }
      }
      compressed.push(update);
      lastUpdate = update;
    }

    return compressed;
  }

  private decompressUpdates(updates: FogUpdate[]): FogUpdate[] {
    return updates;
  }

  private isSimilar(a: FogUpdate, b: FogUpdate): boolean {
    return (
      a.type === b.type &&
      a.playerId === b.playerId &&
      Math.abs(a.x - b.x) <= 1 &&
      Math.abs(a.y - b.y) <= 1 &&
      Math.abs(a.timestamp - b.timestamp) < 100
    );
  }

  calculateChecksum(message: FogSyncMessage): string {
    const str = JSON.stringify(message.data);
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash;
    }
    return hash.toString(16);
  }

  verifyChecksum(message: FogSyncMessage): boolean {
    if (!message.checksum) return true;
    return this.calculateChecksum(message) === message.checksum;
  }

  clearPendingAcks(): void {
    this.pendingAcks.clear();
  }

  clearDeltaHistory(): void {
    this.deltaHistory.clear();
  }
}

export class FogOfWarSystem {
  private mapWidth: number;
  private mapHeight: number;
  private chunkSize: number;
  private chunks: Map<string, FogChunkImpl>;
  private visibilityTracker: VisibilityTracker;
  private visionCalculator: VisionRangeCalculator;
  private stealthSystem: StealthSystem;
  private cloakDetector: CloakDetector;
  private allySharing: AllySharing;
  private shroudRemover: ShroudRemover;
  private revelationManager: RevelationManager;
  private fogStateManager: FogStateManager;
  private eventHandler: FogEventHandler;
  private mapFog: MapFog;
  private unitVision: UnitVision;
  private buildingVision: BuildingVision;
  private renderer: FogRenderer | null;
  private synchronizer: MultiplayerSynchronizer;
  private performanceMetrics: PerformanceMetrics;
  private lastUpdateTime: number;
  private updateQueue: FogUpdate[];
  private maxUpdateQueueSize: number;
  private isPaused: boolean;

  constructor(
    mapWidth: number,
    mapHeight: number,
    chunkSize: number = 16,
    playerId: number = 0
  ) {
    this.mapWidth = mapWidth;
    this.mapHeight = mapHeight;
    this.chunkSize = chunkSize;
    this.chunks = new Map();
    this.visibilityTracker = new VisibilityTracker();
    this.visionCalculator = new VisionRangeCalculator(mapWidth, mapHeight, chunkSize);
    this.stealthSystem = new StealthSystem();
    this.cloakDetector = new CloakDetector();
    this.allySharing = new AllySharing(playerId);
    this.shroudRemover = new ShroudRemover();
    this.revelationManager = new RevelationManager();
    this.fogStateManager = new FogStateManager();
    this.eventHandler = new FogEventHandler();
    this.mapFog = new MapFog(mapWidth, mapHeight);
    this.unitVision = new UnitVision();
    this.buildingVision = new BuildingVision();
    this.renderer = null;
    this.synchronizer = new MultiplayerSynchronizer(playerId);
    this.performanceMetrics = {
      lastCalculationTime: 0,
      averageCalculationTime: 0,
      chunkUpdatesPerSecond: 0,
      visibilityChecksPerSecond: 0,
      activeChunks: 0,
      totalChunks: 0,
      memoryUsage: 0
    };
    this.lastUpdateTime = Date.now();
    this.updateQueue = [];
    this.maxUpdateQueueSize = 1000;
    this.isPaused = false;

    this.initializeChunks();
    this.setupCallbacks();
  }

  private initializeChunks(): void {
    const numChunksX = Math.ceil(this.mapWidth / this.chunkSize);
    const numChunksY = Math.ceil(this.mapHeight / this.chunkSize);

    for (let y = 0; y < numChunksY; y++) {
      for (let x = 0; x < numChunksX; x++) {
        const chunk = new FogChunkImpl(x, y, this.chunkSize);
        this.chunks.set(this.getChunkKey(x, y), chunk);
      }
    }

    this.performanceMetrics.totalChunks = this.chunks.size;
  }

  private getChunkKey(chunkX: number, chunkY: number): string {
    return `${chunkX},${chunkY}`;
  }

  private getChunk(chunkX: number, chunkY: number): FogChunkImpl | undefined {
    return this.chunks.get(this.getChunkKey(chunkX, chunkY));
  }

  private worldToChunk(worldX: number, worldY: number): { chunkX: number; chunkY: number; localX: number; localY: number } {
    const chunkX = Math.floor(worldX / this.chunkSize);
    const chunkY = Math.floor(worldY / this.chunkSize);
    const localX = worldX % this.chunkSize;
    const localY = worldY % this.chunkSize;
    return { chunkX, chunkY, localX, localY };
  }

  private setupCallbacks(): void {
    this.stealthSystem.setStealthChangedCallback((unitId, isStealthed) => {
      if (isStealthed) {
        this.eventHandler.emitUnitCloaked(0, unitId);
      } else {
        this.eventHandler.emitUnitDecloaked(0, unitId, 'left stealth');
      }
    });

    this.cloakDetector.setCloakStateChangedCallback((unitId, isCloaked, isDetected) => {
      if (isCloaked && !isDetected) {
        this.eventHandler.emitUnitCloaked(0, unitId);
      } else if (!isCloaked) {
        this.eventHandler.emitUnitDecloaked(0, unitId, 'cloak expired');
      }
    });

    this.revelationManager.setCallbacks(
      (area) => {
        this.eventHandler.emitAreaRevealed(area.playerId, area);
      },
      (areaId) => {
        console.log(`Reveal expired: ${areaId}`);
      }
    );

    this.unitVision.setVisionChangedCallback((unitId, oldRange, newRange) => {
      this.recalculateVisibilityForUnit(unitId);
    });
  }

  setRenderer(canvas: HTMLCanvasElement, quality: 'low' | 'medium' | 'high' = 'medium'): void {
    this.renderer = new FogRenderer(canvas, quality);
  }

  getRenderer(): FogRenderer | null {
    return this.renderer;
  }

  registerUnit(
    unitId: string,
    x: number,
    y: number,
    visionRange: number,
    visionType: UnitVisionProfile['visionType'] = 'normal'
  ): void {
    const profile = this.unitVision.registerUnit(unitId, visionRange, visionType);
    this.updateVisibilityForUnit(unitId, x, y);
  }

  unregisterUnit(unitId: string): void {
    this.unitVision.unregisterUnit(unitId);
  }

  updateUnitPosition(unitId: string, x: number, y: number): void {
    this.updateVisibilityForUnit(unitId, x, y);
    this.stealthSystem.updateUnitPosition(unitId, x, y);
    this.cloakDetector.updateUnitPosition(unitId, x, y);
  }

  private updateVisibilityForUnit(unitId: string, x: number, y: number): void {
    const profile = this.unitVision.getProfile(unitId);
    if (!profile) return;

    const effectiveRange = this.visionCalculator.calculateVisionRange(
      profile.currentVisionRange,
      profile.modifiers,
      new Vector2(x, y)
    );

    const visibleCells = this.visionCalculator.getVisibleCellsInRange(
      x,
      y,
      effectiveRange,
      profile.hasLineOfSight
    );

    for (const cell of visibleCells) {
      this.setVisibility(cell.x, cell.y, VisibilityLevel.VISIBLE, unitId);
    }

    if (effectiveRange > 0) {
      this.setVisibility(x, y, VisibilityLevel.FULLY_VISIBLE, unitId);
    }
  }

  private recalculateVisibilityForUnit(unitId: string): void {
    this.clearVisibilityForSource(unitId);
    
    const profile = this.unitVision.getProfile(unitId);
    if (profile) {
      console.log(`Recalculating visibility for unit ${unitId} with new range ${profile.currentVisionRange}`);
    }
  }

  registerBuilding(
    buildingId: string,
    x: number,
    y: number,
    visionRange: number,
    visionType: BuildingVisionProfile['visionType'] = 'normal'
  ): void {
    this.buildingVision.registerBuilding(buildingId, visionRange, visionType);
    this.updateVisibilityForBuilding(buildingId, x, y);
  }

  unregisterBuilding(buildingId: string): void {
    this.buildingVision.unregisterBuilding(buildingId);
  }

  updateBuildingPower(buildingId: string, powerLevel: number): void {
    this.buildingVision.setPowerLevel(buildingId, powerLevel);
  }

  private updateVisibilityForBuilding(buildingId: string, x: number, y: number): void {
    const profile = this.buildingVision.getProfile(buildingId);
    if (!profile) return;

    const effectiveRange = this.buildingVision.getEffectiveVisionRange(buildingId);
    if (effectiveRange <= 0) return;

    const visibleCells = this.visionCalculator.getVisibleCellsInRange(
      x,
      y,
      effectiveRange,
      true
    );

    for (const cell of visibleCells) {
      this.setVisibility(cell.x, cell.y, VisibilityLevel.VISIBLE, buildingId);
    }

    this.setVisibility(x, y, VisibilityLevel.FULLY_VISIBLE, buildingId);
  }

  setVisibility(x: number, y: number, level: VisibilityLevel, sourceId?: string): void {
    if (x < 0 || x >= this.mapWidth || y < 0 || y >= this.mapHeight) {
      return;
    }

    const { chunkX, chunkY, localX, localY } = this.worldToChunk(x, y);
    const chunk = this.getChunk(chunkX, chunkY);
    if (!chunk) return;

    const cell = chunk.getCell(localX, localY);
    const oldLevel = cell.visibilityLevel;

    if (level > cell.visibilityLevel) {
      cell.visibilityLevel = level;
      cell.lastSeen = Date.now();
      
      if (sourceId) {
        cell.visibilitySources.add(sourceId);
      }

      chunk.markDirty();

      if (oldLevel !== level) {
        this.eventHandler.emitVisibilityChanged(0, x, y, oldLevel, level);
        
        this.visibilityTracker.updateVisibility(0, x, y, level, sourceId);
        
        const update = new FogUpdateImpl('visibility', 0, x, y, Date.now(), {
          visibilityLevel: level,
          sourceId
        });
        this.queueUpdate(update);
      }
    }
  }

  getVisibility(x: number, y: number): VisibilityLevel {
    const { chunkX, chunkY, localX, localY } = this.worldToChunk(x, y);
    const chunk = this.getChunk(chunkX, chunkY);
    if (!chunk) return VisibilityLevel.HIDDEN;

    const cell = chunk.getCell(localX, localY);
    return cell.visibilityLevel;
  }

  private clearVisibilityForSource(sourceId: string): void {
    this.chunks.forEach(chunk => {
      for (let y = 0; y < this.chunkSize; y++) {
        for (let x = 0; x < this.chunkSize; x++) {
          const cell = chunk.getCell(x, y);
          if (cell.visibilitySources.has(sourceId)) {
            cell.visibilitySources.delete(sourceId);
            
            if (cell.visibilitySources.size === 0) {
              cell.visibilityLevel = VisibilityLevel.HIDDEN;
            }
          }
        }
      }
      chunk.markDirty();
    });
  }

  enterStealth(unitId: string, x: number, y: number, abilityId?: string): void {
    this.stealthSystem.enterStealth(unitId, x, y, abilityId);
  }

  exitStealth(unitId: string): void {
    this.stealthSystem.exitStealth(unitId);
  }

  isStealthed(unitId: string): boolean {
    return this.stealthSystem.isStealthed(unitId);
  }

  enterCloak(unitId: string, x: number, y: number, cloakLevel: number = 1, abilityId?: string): void {
    this.cloakDetector.enterCloak(unitId, x, y, cloakLevel, abilityId);
  }

  exitCloak(unitId: string): void {
    this.cloakDetector.exitCloak(unitId);
  }

  isCloaked(unitId: string): boolean {
    return this.cloakDetector.isCloaked(unitId);
  }

  detectStealthedUnits(detectorId: string, x: number, y: number, range: number): DetectionResult[] {
    const results: DetectionResult[] = [];
    const stealthedUnits = this.stealthSystem.getAllStealthedUnits();

    for (const unit of stealthedUnits) {
      const dx = unit.x - x;
      const dy = unit.y - y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance <= range) {
        const detected = this.stealthSystem.isDetected(unit.id, x, y);
        
        if (detected) {
          this.eventHandler.emitUnitDetected(0, unit.id, detectorId);
        }

        results.push({
          detected,
          detectorId,
          detectedUnitId: unit.id,
          detectionMethod: 'vision',
          detectionChance: 1 - (distance / range),
          distance
        });
      }
    }

    return results;
  }

  detectCloakedUnits(detectorId: string, x: number, y: number, range: number, detectionLevel: number): DetectionResult[] {
    return this.cloakDetector.checkDetection(detectorId, x, y, range, detectionLevel);
  }

  createReveal(x: number, y: number, radius: number, playerId: number = 0, permanent: boolean = false, duration?: number): RevealedArea {
    return this.revelationManager.createReveal(x, y, radius, playerId, permanent, duration);
  }

  removeShroud(x: number, y: number, radius: number, playerId: number = 0): string {
    const areaId = this.shroudRemover.removeShroud(x, y, radius, playerId);
    
    for (let dy = -radius; dy <= radius; dy++) {
      for (let dx = -radius; dx <= radius; dx++) {
        const checkX = x + dx;
        const checkY = y + dy;
        
        if (checkX >= 0 && checkX < this.mapWidth && checkY >= 0 && checkY < this.mapHeight) {
          const dist = Math.sqrt(dx * dx + dy * dy);
          if (dist <= radius) {
            this.setVisibility(checkX, checkY, VisibilityLevel.VISIBLE, 'shroud_removal');
          }
        }
      }
    }

    this.eventHandler.emitAreaShroudRemoved(playerId, x, y, radius);
    
    return areaId;
  }

  addAlly(allyId: number, sharePercentage: number = 100): void {
    this.allySharing.addAlly(allyId, sharePercentage);
  }

  removeAlly(allyId: number): void {
    this.allySharing.removeAlly(allyId);
  }

  syncAllyVision(allyId: number): void {
    const playerState = this.visibilityTracker.getPlayerState(0);
    if (playerState) {
      this.allySharing.syncVision(allyId, playerState.visibilityGrid);
      this.eventHandler.emitAllyVisionShared(0, allyId);
    }
  }

  saveState(playerId: number = 0): void {
    const state: FogState = {
      visibilityGrid: new Map(),
      revealedAreas: this.revelationManager.getRevealedAreas(playerId),
      shroudRemovedAreas: new Set<string>(),
      lastUpdate: Date.now()
    };

    this.chunks.forEach((chunk, key) => {
      for (let y = 0; y < this.chunkSize; y++) {
        for (let x = 0; x < this.chunkSize; x++) {
          const cell = chunk.getCell(x, y);
          const worldX = chunk.getWorldX() + x;
          const worldY = chunk.getWorldY() + y;
          state.visibilityGrid.set(this.mapWidth * worldY + worldX, cell.visibilityLevel);
        }
      }
    });

    this.fogStateManager.saveState(playerId, state);
  }

  undo(playerId: number = 0): boolean {
    const currentState = this.createCurrentState(playerId);
    const previousState = this.fogStateManager.undo(playerId, currentState);
    
    if (previousState) {
      this.restoreState(playerId, previousState);
      this.eventHandler.emitFogStateRestored(playerId, previousState.lastUpdate);
      return true;
    }
    return false;
  }

  redo(playerId: number = 0): boolean {
    const currentState = this.createCurrentState(playerId);
    const nextState = this.fogStateManager.redo(playerId, currentState);
    
    if (nextState) {
      this.restoreState(playerId, nextState);
      this.eventHandler.emitFogStateRestored(playerId, nextState.lastUpdate);
      return true;
    }
    return false;
  }

  private createCurrentState(playerId: number): FogState {
    const state: FogState = {
      visibilityGrid: new Map(),
      revealedAreas: this.revelationManager.getRevealedAreas(playerId),
      shroudRemovedAreas: new Set<string>(),
      lastUpdate: Date.now()
    };

    this.chunks.forEach((chunk, key) => {
      for (let y = 0; y < this.chunkSize; y++) {
        for (let x = 0; x < this.chunkSize; x++) {
          const cell = chunk.getCell(x, y);
          const worldX = chunk.getWorldX() + x;
          const worldY = chunk.getWorldY() + y;
          state.visibilityGrid.set(this.mapWidth * worldY + worldX, cell.visibilityLevel);
        }
      }
    });

    return state;
  }

  private restoreState(playerId: number, state: FogState): void {
    this.revelationManager.clear();
    state.revealedAreas.forEach(area => {
      this.revelationManager.createReveal(area.x, area.y, area.radius, area.playerId, area.permanent);
    });

    this.resetAllVisibility();

    state.visibilityGrid.forEach((level, key) => {
      const x = key % this.mapWidth;
      const y = Math.floor(key / this.mapWidth);
      this.setVisibility(x, y, level);
    });
  }

  private resetAllVisibility(): void {
    this.chunks.forEach(chunk => {
      chunk.reset();
    });
  }

  setTerrain(x: number, y: number, terrainType: number): void {
    this.visionCalculator.setTerrain(x, y, terrainType);
    this.mapFog.setTerrainType(x, y, terrainType);
  }

  setObstacle(x: number, y: number, isObstacle: boolean): void {
    this.visionCalculator.setObstacle(x, y, isObstacle);
  }

  setElevation(x: number, y: number, elevation: number): void {
    this.visionCalculator.setElevation(x, y, elevation);
    this.mapFog.setElevation(x, y, elevation);
  }

  update(deltaTime: number): void {
    if (this.isPaused) return;

    const startTime = performance.now();

    this.revelationManager.update(deltaTime);
    this.eventHandler.processEventQueue();

    this.processUpdateQueue();

    this.performanceMetrics.lastCalculationTime = performance.now() - startTime;
    this.updatePerformanceMetrics();
    
    this.lastUpdateTime = Date.now();
  }

  private queueUpdate(update: FogUpdate): void {
    this.updateQueue.push(update);
    if (this.updateQueue.length > this.maxUpdateQueueSize) {
      this.updateQueue.shift();
    }
  }

  private processUpdateQueue(): void {
    while (this.updateQueue.length > 0) {
      const update = this.updateQueue.shift()!;
      this.visibilityTracker.updateVisibility(
        update.playerId,
        update.x,
        update.y,
        update.visibilityLevel || VisibilityLevel.HIDDEN,
        update.sourceId
      );
    }
  }

  private updatePerformanceMetrics(): void {
    const current = this.performanceMetrics.lastCalculationTime;
    const average = this.performanceMetrics.averageCalculationTime;
    this.performanceMetrics.averageCalculationTime = (average * 0.9) + (current * 0.1);

    let activeChunks = 0;
    this.chunks.forEach(chunk => {
      if (chunk.isDirty) {
        activeChunks++;
      }
    });
    this.performanceMetrics.activeChunks = activeChunks;

    const memoryEstimate = this.chunks.size * this.chunkSize * this.chunkSize * 64;
    this.performanceMetrics.memoryUsage = memoryEstimate;
  }

  render(camera: Camera, cellSize: number = 32): void {
    if (!this.renderer) return;

    this.renderer.clearFog();

    const fogData = new Map<string, VisibilityLevel>();
    
    this.chunks.forEach(chunk => {
      const worldX = chunk.getWorldX();
      const worldY = chunk.getWorldY();
      
      for (let y = 0; y < this.chunkSize; y++) {
        for (let x = 0; x < this.chunkSize; x++) {
          const cell = chunk.getCell(x, y);
          const globalX = worldX + x;
          const globalY = worldY + y;
          
          if (globalX < this.mapWidth && globalY < this.mapHeight) {
            const key = `${globalX},${globalY}`;
            fogData.set(key, cell.visibilityLevel);
          }
        }
      }
    });

    this.renderer.renderFog(fogData, camera, cellSize);

    const revealedAreas = this.revelationManager.getAllRevealedAreas();
    for (const area of revealedAreas) {
      this.renderer.renderAreaReveal(area.x, area.y, area.radius, camera, cellSize);
    }
  }

  getMultiplayerSync(): MultiplayerSynchronizer {
    return this.synchronizer;
  }

  generateSyncMessage(type: 'full' | 'delta'): FogSyncMessage {
    if (type === 'full') {
      return this.synchronizer.generateFullSync(this.createCurrentState(0));
    } else {
      return this.synchronizer.generateDeltaSync(this.updateQueue);
    }
  }

  processSyncMessage(message: FogSyncMessage): void {
    const updates = this.synchronizer.processSyncResponse(message);
    
    for (const update of updates) {
      this.setVisibility(update.x, update.y, update.visibilityLevel || VisibilityLevel.HIDDEN, update.sourceId);
    }

    this.synchronizer.acknowledgeMessage(message.sequenceNumber);
  }

  pause(): void {
    this.isPaused = true;
  }

  resume(): void {
    this.isPaused = false;
  }

  isPausedState(): boolean {
    return this.isPaused;
  }

  getPerformanceMetrics(): PerformanceMetrics {
    return { ...this.performanceMetrics };
  }

  getEventHandler(): FogEventHandler {
    return this.eventHandler;
  }

  getVisibilityTracker(): VisibilityTracker {
    return this.visibilityTracker;
  }

  getRevealManager(): RevelationManager {
    return this.revelationManager;
  }

  getStealthSystem(): StealthSystem {
    return this.stealthSystem;
  }

  getCloakDetector(): CloakDetector {
    return this.cloakDetector;
  }

  getVisibleCells(playerId: number = 0, minLevel: VisibilityLevel = VisibilityLevel.VISIBLE): Vector2[] {
    return this.visibilityTracker.getVisibleCells(playerId, minLevel);
  }

  isPointVisible(x: number, y: number, playerId: number = 0): boolean {
    return this.getVisibility(x, y) >= VisibilityLevel.VISIBLE;
  }

  reset(): void {
    this.resetAllVisibility();
    this.revelationManager.clear();
    this.shroudRemover.clear();
    this.stealthSystem.clear();
    this.cloakDetector.clear();
    this.visibilityTracker.clearAll();
    this.eventHandler.clearEventQueue();
  }

  destroy(): void {
    this.reset();
    this.chunks.clear();
    this.unitVision.clear();
    this.buildingVision.clear();
    this.eventHandler.clearEventQueue();
    this.synchronizer.clearPendingAcks();
    this.synchronizer.clearDeltaHistory();
    this.renderer = null;
  }
}

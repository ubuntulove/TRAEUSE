export interface TerrainConfig {
  seed: number;
  width: number;
  height: number;
  tileSize: number;
  octaves: number;
  persistence: number;
  lacunarity: number;
  scale: number;
  offsetX: number;
  offsetY: number;
}

export interface TerrainChunk {
  x: number;
  y: number;
  width: number;
  height: number;
  tiles: TerrainTile[];
  heightmap: Float32Array;
  alphamap: Uint8Array;
  objects: TerrainObject[];
  decorations: Decoration[];
  navigationMesh: NavigationMesh;
}

export interface TerrainTile {
  x: number;
  y: number;
  height: number;
  normal: Vec3;
  material: TerrainMaterial;
  walkable: boolean;
  buildable: boolean;
  cost: number;
  moisture: number;
  temperature: number;
  fertility: number;
  decoration: Decoration | null;
  object: TerrainObject | null;
  structure: Structure | null;
  entity: Entity | null;
  zone: Zone | null;
  effects: TileEffect[];
}

export interface Vec3 {
  x: number;
  y: number;
  z: number;
}

export interface Vec2 {
  x: number;
  y: number;
}

export interface Vec4 {
  x: number;
  y: number;
  z: number;
  w: number;
}

export interface Color {
  r: number;
  g: number;
  b: number;
  a: number;
}

export interface TerrainMaterial {
  id: string;
  name: string;
  type: MaterialType;
  texture: string;
  normalMap: string;
  roughness: number;
  metallic: number;
  color: Color;
  tileSize: number;
  uvScale: number;
  blendMap: BlendMap | null;
  splatmap: Splatmap | null;
  properties: Record<string, number>;
}

export enum MaterialType {
  GRASS = 'grass',
  DIRT = 'dirt',
  SAND = 'sand',
  ROCK = 'rock',
  WATER = 'water',
  SNOW = 'snow',
  MUD = 'mud',
  CONCRETE = 'concrete',
  METAL = 'metal',
  WOOD = 'wood',
  ICE = 'ice',
  LAVA = 'lava',
  SWAMP = 'swamp',
  FOREST = 'forest',
  MOUNTAIN = 'mountain',
}

export interface BlendMap {
  texture: string;
  channel: number;
  opacity: number;
  threshold: number;
}

export interface Splatmap {
  channels: SplatmapChannel[];
  resolution: number;
}

export interface SplatmapChannel {
  materialId: string;
  strength: number;
  texture: string | null;
}

export interface TerrainObject {
  id: string;
  type: ObjectType;
  name: string;
  x: number;
  y: number;
  z: number;
  width: number;
  height: number;
  depth: number;
  rotation: Vec3;
  scale: Vec3;
  model: string;
  material: string;
  health: number;
  maxHealth: number;
  destructible: boolean;
  burnable: boolean;
  harvestable: boolean;
  lootTable: LootItem[];
  interactable: boolean;
  blocking: boolean;
  castShadow: boolean;
  receiveShadow: boolean;
  properties: Record<string, unknown>;
}

export enum ObjectType {
  TREE = 'tree',
  ROCK = 'rock',
  BUSH = 'bush',
  GRASS = 'grass',
  FLOWER = 'flower',
  MUSHROOM = 'mushroom',
  STUMP = 'stump',
  LOG = 'log',
  DEBRIS = 'debris',
  RUBBLE = 'rubble',
  BONES = 'bones',
  CRATE = 'crate',
  BARREL = 'barrel',
  CHEST = 'chest',
  FURNITURE = 'furniture',
  VEHICLE = 'vehicle',
  WRECKAGE = 'wreckage',
  MONUMENT = 'monument',
  STATUE = 'statue',
  FOUNTAIN = 'fountain',
  WELL = 'well',
  SIGN = 'sign',
  FLAG = 'flag',
}

export interface LootItem {
  itemId: string;
  name: string;
  quantity: number;
  probability: number;
  minQuantity: number;
  maxQuantity: number;
  quality: ItemQuality;
}

export enum ItemQuality {
  COMMON = 1,
  UNCOMMON = 2,
  RARE = 3,
  EPIC = 4,
  LEGENDARY = 5,
}

export interface Decoration {
  id: string;
  type: DecorationType;
  name: string;
  x: number;
  y: number;
  z: number;
  rotation: number;
  scale: number;
  sprite: string;
  color: Color;
  opacity: number;
  tint: Color | null;
  flipX: boolean;
  flipY: boolean;
  animation: string | null;
  parallax: number;
  visible: boolean;
  order: number;
  properties: Record<string, unknown>;
}

export enum DecorationType {
  GRASS_TUFT = 'grass_tuft',
  FLOWER = 'flower',
  MUSHROOM = 'mushroom',
  CLUMP = 'clump',
  STONE = 'stone',
  SHELL = 'shell',
  FEATHER = 'feather',
  BONES = 'bones',
  DUST = 'dust',
  SPARKLE = 'sparkle',
  BUG = 'bug',
  BUTTERFLY = 'butterfly',
  BIRD = 'bird',
  SMOKE = 'smoke',
  FIREFLY = 'firefly',
  LEAF = 'leaf',
}

export interface Structure {
  id: string;
  type: StructureType;
  name: string;
  x: number;
  y: number;
  z: number;
  width: number;
  height: number;
  rotation: number;
  model: string;
  material: string;
  health: number;
  maxHealth: number;
  faction: string | null;
  state: StructureState;
  upgrades: Upgrade[];
  production: Production | null;
  storage: Storage | null;
  power: PowerConnection | null;
  defense: Defense | null;
  blocking: boolean;
  visible: boolean;
  intact: boolean;
  fireProgress: number;
  damaged: boolean;
}

export enum StructureType {
  BUILDING = 'building',
  WALL = 'wall',
  TOWER = 'tower',
  GATE = 'gate',
  BRIDGE = 'bridge',
  ROAD = 'road',
  PIPE = 'pipe',
  CABLE = 'cable',
  TOWER_DEFENSE = 'tower_defense',
  BARRICADE = 'barricade',
  TRENCH = 'trench',
  SANDBAG = 'sandbag',
  RAMPART = 'rampart',
}

export enum StructureState {
  INTACT = 'intact',
  DAMAGED = 'damaged',
  CRITICAL = 'critical',
  DESTROYED = 'destroyed',
  CONSTRUCTING = 'constructing',
  DECONSTRUCTING = 'deconstructing',
  UPGRADING = 'upgrading',
  REPAIRING = 'repairing',
  ACTIVE = 'active',
  INACTIVE = 'inactive',
  BURNT = 'burnt',
  RUINED = 'ruined',
}

export interface Upgrade {
  id: string;
  name: string;
  level: number;
  maxLevel: number;
  cost: number;
  time: number;
  requirements: string[];
  effects: UpgradeEffect[];
  progress: number;
  available: boolean;
}

export interface UpgradeEffect {
  type: EffectType;
  target: string;
  value: number;
  operation: Operation;
}

export enum EffectType {
  HEALTH = 'health',
  DAMAGE = 'damage',
  ARMOR = 'armor',
  SPEED = 'speed',
  RANGE = 'range',
  ACCURACY = 'accuracy',
  RATE_OF_FIRE = 'rate_of_fire',
  CAPACITY = 'capacity',
  EFFICIENCY = 'efficiency',
  VISION = 'vision',
  DETECTION = 'detection',
}

export enum Operation {
  ADD = 'add',
  MULTIPLY = 'multiply',
  SET = 'set',
  PERCENT = 'percent',
}

export interface Production {
  queue: ProductionItem[];
  maxQueue: number;
  currentItem: ProductionItem | null;
  progress: number;
  speed: number;
  paused: boolean;
  output: string;
  efficiency: number;
}

export interface ProductionItem {
  id: string;
  type: string;
  name: string;
  cost: number;
  time: number;
  requirements: string[];
  output: string;
  quantity: number;
}

export interface Storage {
  resources: Map<string, number>;
  capacity: Map<string, number>;
  overflow: boolean;
  shared: boolean;
}

export interface PowerConnection {
  producer: boolean;
  consumer: boolean;
  output: number;
  input: number;
  efficiency: number;
  network: string | null;
}

export interface Defense {
  type: DefenseType;
  health: number;
  armor: number;
  resistances: Map<string, number>;
  immuneTo: string[];
}

export enum DefenseType {
  LIGHT = 'light',
  MEDIUM = 'medium',
  HEAVY = 'heavy',
  ARMORED = 'armored',
  FORTIFIED = 'fortified',
}

export interface Entity {
  id: string;
  type: EntityType;
  faction: string;
  owner: string;
  position: Vec3;
  rotation: Vec3;
  scale: Vec3;
  health: number;
  maxHealth: number;
  shield: number;
  maxShield: number;
  energy: number;
  maxEnergy: number;
  state: EntityState;
  inventory: InventoryItem[];
  equipment: Equipment | null;
  statusEffects: StatusEffect[];
  ai: AIBehavior | null;
  team: number;
  name: string;
  level: number;
  experience: number;
  isAlive: boolean;
  isVisible: boolean;
  isSelectable: boolean;
  isInteractive: boolean;
  minimapColor: Color;
}

export enum EntityType {
  UNIT = 'unit',
  BUILDING = 'building',
  VEHICLE = 'vehicle',
  AIRCRAFT = 'aircraft',
  SHIP = 'ship',
  PROJECTILE = 'projectile',
  ITEM = 'item',
  DROPPED_ITEM = 'dropped_item',
  RESOURCE = 'resource',
  TREASURE = 'treasure',
  TRAP = 'trap',
  NPC = 'npc',
  MERCHANT = 'merchant',
  GUARDIAN = 'guardian',
}

export enum EntityState {
  IDLE = 'idle',
  MOVING = 'moving',
  ATTACKING = 'attacking',
  DEFENDING = 'defending',
  HEALING = 'healing',
  GATHERING = 'gathering',
  BUILDING = 'building',
  REPAIRING = 'repairing',
  CASTING = 'casting',
  STUNNED = 'stunned',
  SLEEPING = 'sleeping',
  DEAD = 'dead',
  DYING = 'dying',
  RESPAWNING = 'respawning',
  CAPTURING = 'capturing',
  BEING_CAPTURED = 'being_captured',
  MOUNTING = 'mounting',
  DISMOUNTING = 'dismounting',
  ENTERING = 'entering',
  EXITING = 'exiting',
  BOARDING = 'boarding',
  UNBOARDING = 'unboarding',
}

export interface InventoryItem {
  id: string;
  type: string;
  name: string;
  quantity: number;
  quality: ItemQuality;
  durability: number;
  maxDurability: number;
  properties: Record<string, unknown>;
  equipped: boolean;
  soulbound: boolean;
}

export interface Equipment {
  head: InventoryItem | null;
  chest: InventoryItem | null;
  legs: InventoryItem | null;
  feet: InventoryItem | null;
  mainHand: InventoryItem | null;
  offHand: InventoryItem | null;
  ring1: InventoryItem | null;
  ring2: InventoryItem | null;
  amulet: InventoryItem | null;
  backpack: InventoryItem[];
}

export interface StatusEffect {
  id: string;
  name: string;
  type: EffectCategory;
  duration: number;
  remaining: number;
  stacking: StackingType;
  stacks: number;
  maxStacks: number;
  source: string;
  magnitude: number;
  refreshable: boolean;
  dispellable: boolean;
  modifiers: EffectModifier[];
  visuals: VisualEffect[];
}

export enum EffectCategory {
  BUFF = 'buff',
  DEBUFF = 'debuff',
  CROWD_CONTROL = 'crowd_control',
  DOT = 'dot',
  HOT = 'hot',
  SHIELD = 'shield',
  BUFF_ARMOR = 'buff_armor',
  BUFF_ATTACK = 'buff_attack',
  DEBUFF_ARMOR = 'debuff_armor',
  DEBUFF_ATTACK = 'debuff_attack',
  CROWD_CONTROL_STUN = 'crowd_control_stun',
  CROWD_CONTROL_ROOT = 'crowd_control_root',
  CROWD_CONTROL_SLOW = 'crowd_control_slow',
  CROWD_CONTROL_SILENCE = 'crowd_control_silence',
  CROWD_CONTROL_FEAR = 'crowd_control_fear',
  CROWD_CONTROL_KNOCKBACK = 'crowd_control_knockback',
  CROWD_CONTROL_BLIND = 'crowd_control_blind',
}

export enum StackingType {
  NONE = 'none',
  INTENSITY = 'intensity',
  DURATION = 'duration',
  UNIQUE = 'unique',
}

export interface EffectModifier {
  type: string;
  operation: Operation;
  value: number;
  target: string;
}

export interface VisualEffect {
  type: VisualType;
  position: Vec3;
  rotation: Vec3;
  scale: number;
  duration: number;
  loop: boolean;
  color: Color | null;
  billboard: boolean;
}

export enum VisualType {
  PARTICLE = 'particle',
  AURA = 'aura',
  BEAM = 'beam',
  RING = 'ring',
  SPIRAL = 'spiral',
  EXPLOSION = 'explosion',
  TRAIL = 'trail',
  PROJECTILE = 'projectile',
  BUFF_ICON = 'buff_icon',
  DEBUFF_ICON = 'debuff_icon',
  SCREEN_EFFECT = 'screen_effect',
  SHADOW = 'shadow',
  LIGHT = 'light',
}

export interface AIBehavior {
  type: AIType;
  state: string;
  target: string | null;
  homePosition: Vec3;
  patrolPath: Vec3[];
  currentWaypoint: number;
  lastSeenTarget: Vec3 | null;
  lastAttackTime: number;
  aggroRange: number;
  chaseRange: number;
  attackRange: number;
  abilities: AIAbility[];
  priorities: AIPriority[];
  personality: AIPersonality;
}

export enum AIType {
  IDLE = 'idle',
  PATROL = 'patrol',
  GUARD = 'guard',
  CHASE = 'chase',
  ATTACK = 'attack',
  FLEE = 'flee',
  WANDER = 'wander',
  FOLLOW = 'follow',
  SEARCH = 'search',
  HUNT = 'hunt',
  ESCAPE = 'escape',
  GATHER = 'gather',
  BUILD = 'build',
  REPAIR = 'repair',
  HEAL = 'heal',
}

export interface AIAbility {
  id: string;
  name: string;
  cooldown: number;
  remainingCooldown: number;
  castRange: number;
  minHealthPercent: number;
  maxHealthPercent: number;
  targetSelf: boolean;
  targetEnemy: boolean;
  targetAlly: boolean;
  probability: number;
}

export interface AIPriority {
  target: string;
  priority: number;
  conditions: string[];
}

export interface AIPersonality {
  aggression: number;
  caution: number;
  bravery: number;
  loyalty: number;
  intelligence: number;
  randomness: number;
}

export interface Zone {
  id: string;
  name: string;
  type: ZoneType;
  x: number;
  y: number;
  width: number;
  height: number;
  shape: ZoneShape;
  points: Vec2[];
  color: Color;
  visible: boolean;
  properties: Record<string, unknown>;
}

export enum ZoneType {
  SAFE = 'safe',
  DANGER = 'danger',
  COMBAT = 'combat',
  RESOURCE = 'resource',
  BUILDING = 'building',
  RESTRICTED = 'restricted',
  PVP = 'pvp',
  PVE = 'pve',
  TOWN = 'town',
  WILDERNESS = 'wilderness',
  DUNGEON = 'dungeon',
  RAID = 'raid',
}

export enum ZoneShape {
  RECTANGLE = 'rectangle',
  CIRCLE = 'circle',
  POLYGON = 'polygon',
}

export interface TileEffect {
  type: EffectCategory;
  damage: number;
  healing: number;
  speedModifier: number;
  visionModifier: number;
  duration: number;
  source: string;
}

export interface NavigationMesh {
  nodes: NavigationNode[];
  edges: NavigationEdge[];
  regions: NavigationRegion[];
}

export interface NavigationNode {
  id: number;
  x: number;
  y: number;
  z: number;
  connections: number[];
  cost: number;
  flags: NavigationFlags;
  area: number;
}

export enum NavigationFlags {
  WALKABLE = 1,
  SWIMMABLE = 2,
  FLYABLE = 4,
  JUMPABLE = 8,
  CLIMBABLE = 16,
  VAULTABLE = 32,
  DOOR = 64,
  LADDER = 128,
  ELEVATOR = 256,
  PORTAL = 512,
}

export interface NavigationEdge {
  from: number;
  to: number;
  cost: number;
  flags: NavigationFlags;
  bidirectional: boolean;
}

export interface NavigationRegion {
  id: number;
  name: string;
  type: RegionType;
  nodes: number[];
  cost: number;
}

export enum RegionType {
  GROUND = 'ground',
  WATER = 'water',
  AIR = 'air',
  UNDERGROUND = 'underground',
  BUILDING = 'building',
  SHIP = 'ship',
}

export class TerrainGenerator {
  private config: TerrainConfig;
  private random: SeededRandom;
  private noise: SimplexNoise;
  private chunks: Map<string, TerrainChunk> = new Map();
  private materials: Map<string, TerrainMaterial> = new Map();
  private objectTemplates: Map<ObjectType, TerrainObject[]> = new Map();
  private decorationTemplates: Map<DecorationType, Decoration[]> = new Map();
  private chunkSize: number = 16;
  private maxHeight: number = 100;
  private seaLevel: number = 0.3;

  constructor(config: Partial<TerrainConfig> = {}) {
    this.config = {
      seed: config.seed || Date.now(),
      width: config.width || 512,
      height: config.height || 512,
      tileSize: config.tileSize || 1,
      octaves: config.octaves || 6,
      persistence: config.persistence || 0.5,
      lacunarity: config.lacunarity || 2.0,
      scale: config.scale || 50,
      offsetX: config.offsetX || 0,
      offsetY: config.offsetY || 0,
    };

    this.random = new SeededRandom(this.config.seed);
    this.noise = new SimplexNoise(this.config.seed);
    this.initializeMaterials();
    this.initializeObjectTemplates();
  }

  private initializeMaterials(): void {
    const baseMaterials: TerrainMaterial[] = [
      { id: 'grass', name: 'Grass', type: MaterialType.GRASS, texture: 'grass_diffuse', normalMap: 'grass_normal', roughness: 0.8, metallic: 0, color: { r: 100, g: 150, b: 80, a: 255 }, tileSize: 4, uvScale: 1, blendMap: null, splatmap: null, properties: {} },
      { id: 'dirt', name: 'Dirt', type: MaterialType.DIRT, texture: 'dirt_diffuse', normalMap: 'dirt_normal', roughness: 0.9, metallic: 0, color: { r: 120, g: 90, b: 70, a: 255 }, tileSize: 4, uvScale: 1, blendMap: null, splatmap: null, properties: {} },
      { id: 'sand', name: 'Sand', type: MaterialType.SAND, texture: 'sand_diffuse', normalMap: 'sand_normal', roughness: 0.95, metallic: 0, color: { r: 220, g: 200, b: 150, a: 255 }, tileSize: 4, uvScale: 1, blendMap: null, splatmap: null, properties: {} },
      { id: 'rock', name: 'Rock', type: MaterialType.ROCK, texture: 'rock_diffuse', normalMap: 'rock_normal', roughness: 0.7, metallic: 0.1, color: { r: 100, g: 100, b: 100, a: 255 }, tileSize: 4, uvScale: 1, blendMap: null, splatmap: null, properties: {} },
      { id: 'water', name: 'Water', type: MaterialType.WATER, texture: 'water_diffuse', normalMap: 'water_normal', roughness: 0.1, metallic: 0.3, color: { r: 50, g: 100, b: 200, a: 200 }, tileSize: 8, uvScale: 2, blendMap: null, splatmap: null, properties: {} },
      { id: 'snow', name: 'Snow', type: MaterialType.SNOW, texture: 'snow_diffuse', normalMap: 'snow_normal', roughness: 0.6, metallic: 0.1, color: { r: 250, g: 250, b: 255, a: 255 }, tileSize: 4, uvScale: 1, blendMap: null, splatmap: null, properties: {} },
      { id: 'mud', name: 'Mud', type: MaterialType.MUD, texture: 'mud_diffuse', normalMap: 'mud_normal', roughness: 0.95, metallic: 0, color: { r: 80, g: 60, b: 40, a: 255 }, tileSize: 4, uvScale: 1, blendMap: null, splatmap: null, properties: {} },
      { id: 'concrete', name: 'Concrete', type: MaterialType.CONCRETE, texture: 'concrete_diffuse', normalMap: 'concrete_normal', roughness: 0.85, metallic: 0, color: { r: 150, g: 150, b: 150, a: 255 }, tileSize: 4, uvScale: 1, blendMap: null, splatmap: null, properties: {} },
    ];

    baseMaterials.forEach(m => this.materials.set(m.id, m));
  }

  private initializeObjectTemplates(): void {
    this.objectTemplates.set(ObjectType.TREE, [
      { id: 'pine', type: ObjectType.TREE, name: 'Pine Tree', x: 0, y: 0, z: 0, width: 3, height: 10, depth: 3, rotation: { x: 0, y: 0, z: 0 }, scale: { x: 1, y: 1, z: 1 }, model: 'pine_tree', material: 'bark', health: 100, maxHealth: 100, destructible: true, burnable: true, harvestable: true, lootTable: [], interactable: false, blocking: true, castShadow: true, receiveShadow: true, properties: {} },
      { id: 'oak', type: ObjectType.TREE, name: 'Oak Tree', x: 0, y: 0, z: 0, width: 4, height: 8, depth: 4, rotation: { x: 0, y: 0, z: 0 }, scale: { x: 1, y: 1, z: 1 }, model: 'oak_tree', material: 'bark', health: 150, maxHealth: 150, destructible: true, burnable: true, harvestable: true, lootTable: [], interactable: false, blocking: true, castShadow: true, receiveShadow: true, properties: {} },
    ]);

    this.objectTemplates.set(ObjectType.ROCK, [
      { id: 'boulder', type: ObjectType.ROCK, name: 'Boulder', x: 0, y: 0, z: 0, width: 2, height: 1.5, depth: 2, rotation: { x: 0, y: 0, z: 0 }, scale: { x: 1, y: 1, z: 1 }, model: 'boulder', material: 'rock', health: 500, maxHealth: 500, destructible: true, burnable: false, harvestable: true, lootTable: [], interactable: false, blocking: true, castShadow: true, receiveShadow: true, properties: {} },
      { id: 'pebbles', type: ObjectType.ROCK, name: 'Pebbles', x: 0, y: 0, z: 0, width: 1, height: 0.3, depth: 1, rotation: { x: 0, y: 0, z: 0 }, scale: { x: 1, y: 1, z: 1 }, model: 'pebbles', material: 'rock', health: 200, maxHealth: 200, destructible: true, burnable: false, harvestable: true, lootTable: [], interactable: false, blocking: false, castShadow: true, receiveShadow: true, properties: {} },
    ]);
  }

  public generateTerrain(): TerrainChunk[] {
    const chunks: TerrainChunk[] = [];
    const chunkCountX = Math.ceil(this.config.width / this.chunkSize);
    const chunkCountY = Math.ceil(this.config.height / this.chunkSize);

    for (let cx = 0; cx < chunkCountX; cx++) {
      for (let cy = 0; cy < chunkCountY; cy++) {
        const chunk = this.generateChunk(cx, cy);
        chunks.push(chunk);
        this.chunks.set(`${cx},${cy}`, chunk);
      }
    }

    return chunks;
  }

  public generateChunk(chunkX: number, chunkY: number): TerrainChunk {
    const tiles: TerrainTile[] = [];
    const heightmap = new Float32Array(this.chunkSize * this.chunkSize);
    const alphamap = new Uint8Array(this.chunkSize * this.chunkSize * 4);
    const objects: TerrainObject[] = [];
    const decorations: Decoration[] = [];

    const startX = chunkX * this.chunkSize;
    const startY = chunkY * this.chunkSize;

    for (let y = 0; y < this.chunkSize; y++) {
      for (let x = 0; x < this.chunkSize; x++) {
        const worldX = startX + x;
        const worldY = startY + y;
        const index = y * this.chunkSize + x;

        const height = this.getHeight(worldX, worldY);
        heightmap[index] = height;

        const moisture = this.getMoisture(worldX, worldY);
        const temperature = this.getTemperature(worldX, worldY);

        const material = this.determineMaterial(height, moisture, temperature);

        const tile: TerrainTile = {
          x: worldX,
          y: worldY,
          height,
          normal: this.calculateNormal(worldX, worldY),
          material,
          walkable: material.type !== MaterialType.WATER,
          buildable: material.type !== MaterialType.WATER && material.type !== MaterialType.ROCK,
          cost: this.calculateMovementCost(material.type),
          moisture,
          temperature,
          fertility: this.calculateFertility(moisture, temperature),
          decoration: null,
          object: null,
          structure: null,
          entity: null,
          zone: null,
          effects: [],
        };

        if (this.random.random() < 0.05) {
          tile.decoration = this.generateDecoration(tile);
          decorations.push(tile.decoration);
        }

        if (this.random.random() < 0.02 && tile.walkable) {
          const obj = this.generateObject(tile);
          if (obj) {
            tile.object = obj;
            objects.push(obj);
          }
        }

        tiles.push(tile);
      }
    }

    const navigationMesh = this.generateNavigationMesh(tiles);

    return {
      x: startX,
      y: startY,
      width: this.chunkSize,
      height: this.chunkSize,
      tiles,
      heightmap,
      alphamap,
      objects,
      decorations,
      navigationMesh,
    };
  }

  private getHeight(x: number, y: number): number {
    let amplitude = 1;
    let frequency = 1;
    let height = 0;

    for (let octave = 0; octave < this.config.octaves; octave++) {
      const noiseValue = this.noise.noise2D(
        (x + this.config.offsetX + frequency * 100) / (this.config.scale / frequency),
        (y + this.config.offsetY + frequency * 100) / (this.config.scale / frequency)
      );

      height += noiseValue * amplitude;
      amplitude *= this.config.persistence;
      frequency *= this.config.lacunarity;
    }

    return (height + 1) / 2;
  }

  private getMoisture(x: number, y: number): number {
    const noiseValue = this.noise.noise2D(x / 80 + 1000, y / 80 + 1000);
    return (noiseValue + 1) / 2;
  }

  private getTemperature(x: number, y: number): number {
    const latitudeNoise = this.noise.noise2D(x / 200, 0);
    const elevationNoise = this.getHeight(x, y);
    const baseTemp = 1 - Math.abs(latitudeNoise) * 0.5;
    const elevationMod = 1 - elevationNoise * 0.3;
    return baseTemp * elevationMod;
  }

  private determineMaterial(height: number, moisture: number, temperature: number): TerrainMaterial {
    if (height < this.seaLevel) {
      return this.materials.get('water')!;
    }

    if (height > 0.8) {
      if (temperature < 0.3) {
        return this.materials.get('snow')!;
      }
      return this.materials.get('rock')!;
    }

    if (moisture > 0.6 && temperature > 0.4) {
      return this.materials.get('grass')!;
    }

    if (moisture < 0.3) {
      if (temperature > 0.6) {
        return this.materials.get('sand')!;
      }
      return this.materials.get('dirt')!;
    }

    if (moisture > 0.7 && temperature < 0.3) {
      return this.materials.get('mud')!;
    }

    return this.materials.get('grass')!;
  }

  private calculateNormal(x: number, y: number): Vec3 {
    const delta = 0.1;
    const heightL = this.getHeight(x - delta, y);
    const heightR = this.getHeight(x + delta, y);
    const heightD = this.getHeight(x, y - delta);
    const heightU = this.getHeight(x, y + delta);

    const nx = heightL - heightR;
    const ny = delta * 2;
    const nz = heightD - heightU;
    const length = Math.sqrt(nx * nx + ny * ny + nz * nz);

    return {
      x: nx / length,
      y: ny / length,
      z: nz / length,
    };
  }

  private calculateMovementCost(type: MaterialType): number {
    switch (type) {
      case MaterialType.GRASS: return 1;
      case MaterialType.DIRT: return 1.2;
      case MaterialType.SAND: return 1.5;
      case MaterialType.ROCK: return 2;
      case MaterialType.WATER: return Infinity;
      case MaterialType.SNOW: return 1.3;
      case MaterialType.MUD: return 2;
      case MaterialType.CONCRETE: return 1;
      case MaterialType.METAL: return 1;
      case MaterialType.WOOD: return 1.1;
      case MaterialType.ICE: return 0.8;
      default: return 1;
    }
  }

  private calculateFertility(moisture: number, temperature: number): number {
    const optimalMoisture = 0.5;
    const optimalTemperature = 0.6;
    const moistureFactor = 1 - Math.abs(moisture - optimalMoisture) * 2;
    const temperatureFactor = 1 - Math.abs(temperature - optimalTemperature) * 2;
    return Math.max(0, moistureFactor * temperatureFactor);
  }

  private generateDecoration(tile: TerrainTile): Decoration {
    const decorationTypes = [DecorationType.GRASS_TUFT, DecorationType.FLOWER, DecorationType.STONE, DecorationType.MUSHROOM];
    const type = decorationTypes[Math.floor(this.random.random() * decorationTypes.length)];

    return {
      id: `deco_${Date.now()}_${this.random.random().toString(36).substr(2, 9)}`,
      type,
      name: type,
      x: tile.x + (this.random.random() - 0.5) * 0.5,
      y: tile.y + (this.random.random() - 0.5) * 0.5,
      z: tile.height,
      rotation: this.random.random() * 360,
      scale: 0.5 + this.random.random() * 0.5,
      sprite: `${type}_sprite`,
      color: { r: 255, g: 255, b: 255, a: 255 },
      opacity: 1,
      tint: null,
      flipX: this.random.random() < 0.5,
      flipY: false,
      animation: null,
      parallax: 1,
      visible: true,
      order: 0,
      properties: {},
    };
  }

  private generateObject(tile: TerrainTile): TerrainObject | null {
    if (tile.material.type === MaterialType.GRASS || tile.material.type === MaterialType.DIRT) {
      const treeType = Math.random() < 0.5 ? ObjectType.TREE : ObjectType.ROCK;
      const templates = this.objectTemplates.get(treeType);
      if (templates && templates.length > 0) {
        const template = templates[Math.floor(this.random.random() * templates.length)];
        return {
          ...template,
          id: `obj_${Date.now()}_${this.random.random().toString(36).substr(2, 9)}`,
          x: tile.x,
          y: tile.y,
          z: tile.height,
        };
      }
    }
    return null;
  }

  private generateNavigationMesh(tiles: TerrainTile[]): NavigationMesh {
    const nodes: NavigationNode[] = [];
    const edges: NavigationEdge[] = [];
    const regions: NavigationRegion[] = [];

    tiles.forEach((tile, index) => {
      const node: NavigationNode = {
        id: index,
        x: tile.x,
        y: tile.height,
        z: tile.y,
        connections: [],
        cost: tile.cost,
        flags: tile.walkable ? NavigationFlags.WALKABLE : 0,
        area: 0,
      };

      if (index % this.chunkSize > 0 && tiles[index - 1].walkable) {
        edges.push({ from: index, to: index - 1, cost: tile.cost, flags: node.flags, bidirectional: true });
        node.connections.push(index - 1);
      }

      if (index >= this.chunkSize && tiles[index - this.chunkSize].walkable) {
        edges.push({ from: index, to: index - this.chunkSize, cost: tile.cost, flags: node.flags, bidirectional: true });
        node.connections.push(index - this.chunkSize);
      }

      nodes.push(node);
    });

    return { nodes, edges, regions };
  }

  public getChunk(chunkX: number, chunkY: number): TerrainChunk | undefined {
    return this.chunks.get(`${chunkX},${chunkY}`);
  }

  public getTileAt(worldX: number, worldY: number): TerrainTile | null {
    const chunkX = Math.floor(worldX / this.chunkSize);
    const chunkY = Math.floor(worldY / this.chunkSize);
    const chunk = this.getChunk(chunkX, chunkY);

    if (!chunk) {
      return null;
    }

    const localX = Math.floor(worldX % this.chunkSize);
    const localY = Math.floor(worldY % this.chunkSize);
    const index = localY * this.chunkSize + localX;

    return chunk.tiles[index] || null;
  }

  public setTileAt(worldX: number, worldY: number, tile: TerrainTile): void {
    const chunkX = Math.floor(worldX / this.chunkSize);
    const chunkY = Math.floor(worldY / this.chunkSize);
    const chunk = this.getChunk(chunkX, chunkY);

    if (!chunk) {
      return;
    }

    const localX = Math.floor(worldX % this.chunkSize);
    const localY = Math.floor(worldY % this.chunkSize);
    const index = localY * this.chunkSize + localX;

    chunk.tiles[index] = tile;
  }

  public modifyHeight(worldX: number, worldY: number, delta: number, radius: number): void {
    const affectedTiles: TerrainTile[] = [];

    for (let dy = -radius; dy <= radius; dy++) {
      for (let dx = -radius; dx <= radius; dx++) {
        const distance = Math.sqrt(dx * dx + dy * dy);
        if (distance <= radius) {
          const tile = this.getTileAt(worldX + dx, worldY + dy);
          if (tile) {
            const influence = 1 - distance / radius;
            tile.height = Math.max(0, Math.min(1, tile.height + delta * influence));
            affectedTiles.push(tile);
          }
        }
      }
    }

    affectedTiles.forEach(tile => {
      tile.normal = this.calculateNormal(tile.x, tile.y);
      tile.material = this.determineMaterial(tile.height, tile.moisture, tile.temperature);
      tile.walkable = tile.material.type !== MaterialType.WATER;
      tile.buildable = tile.material.type !== MaterialType.WATER && tile.material.type !== MaterialType.ROCK;
      tile.cost = this.calculateMovementCost(tile.material.type);
    });
  }

  public paintMaterial(worldX: number, worldY: number, materialId: string, radius: number, opacity: number = 1): void {
    const material = this.materials.get(materialId);
    if (!material) {
      return;
    }

    for (let dy = -radius; dy <= radius; dy++) {
      for (let dx = -radius; dx <= radius; dx++) {
        const distance = Math.sqrt(dx * dx + dy * dy);
        if (distance <= radius) {
          const tile = this.getTileAt(worldX + dx, worldY + dy);
          if (tile) {
            const influence = (1 - distance / radius) * opacity;
            tile.material = this.lerpMaterial(tile.material, material, influence);
            tile.walkable = tile.material.type !== MaterialType.WATER;
            tile.buildable = tile.material.type !== MaterialType.WATER && tile.material.type !== MaterialType.ROCK;
            tile.cost = this.calculateMovementCost(tile.material.type);
          }
        }
      }
    }
  }

  private lerpMaterial(m1: TerrainMaterial, m2: TerrainMaterial, t: number): TerrainMaterial {
    return {
      ...m1,
      id: t > 0.5 ? m2.id : m1.id,
      type: t > 0.5 ? m2.type : m1.type,
      color: this.lerpColor(m1.color, m2.color, t),
      roughness: m1.roughness + (m2.roughness - m1.roughness) * t,
      metallic: m1.metallic + (m2.metallic - m1.metallic) * t,
    };
  }

  private lerpColor(c1: Color, c2: Color, t: number): Color {
    return {
      r: Math.round(c1.r + (c2.r - c1.r) * t),
      g: Math.round(c1.g + (c2.g - c1.g) * t),
      b: Math.round(c1.b + (c2.b - c1.b) * t),
      a: Math.round(c1.a + (c2.a - c1.a) * t),
    };
  }

  public addDecoration(worldX: number, worldY: number, decorationType: DecorationType, scale: number = 1, rotation: number = 0): Decoration | null {
    const tile = this.getTileAt(worldX, worldY);
    if (!tile) {
      return null;
    }

    const decoration: Decoration = {
      id: `deco_${Date.now()}_${this.random.random().toString(36).substr(2, 9)}`,
      type: decorationType,
      name: decorationType,
      x: worldX,
      y: worldY,
      z: tile.height,
      rotation,
      scale,
      sprite: `${decorationType}_sprite`,
      color: { r: 255, g: 255, b: 255, a: 255 },
      opacity: 1,
      tint: null,
      flipX: false,
      flipY: false,
      animation: null,
      parallax: 1,
      visible: true,
      order: 0,
      properties: {},
    };

    tile.decoration = decoration;

    const chunkX = Math.floor(worldX / this.chunkSize);
    const chunkY = Math.floor(worldY / this.chunkSize);
    const chunk = this.getChunk(chunkX, chunkY);
    if (chunk) {
      chunk.decorations.push(decoration);
    }

    return decoration;
  }

  public addObject(worldX: number, worldY: number, objectType: ObjectType, scale: number = 1): TerrainObject | null {
    const tile = this.getTileAt(worldX, worldY);
    if (!tile || !tile.walkable) {
      return null;
    }

    const templates = this.objectTemplates.get(objectType);
    if (!templates || templates.length === 0) {
      return null;
    }

    const template = templates[Math.floor(this.random.random() * templates.length)];
    const obj: TerrainObject = {
      ...template,
      id: `obj_${Date.now()}_${this.random.random().toString(36).substr(2, 9)}`,
      x: worldX,
      y: worldY,
      z: tile.height,
      scale: { x: scale, y: scale, z: scale },
    };

    tile.object = obj;

    const chunkX = Math.floor(worldX / this.chunkSize);
    const chunkY = Math.floor(worldY / this.chunkSize);
    const chunk = this.getChunk(chunkX, chunkY);
    if (chunk) {
      chunk.objects.push(obj);
    }

    return obj;
  }

  public removeObject(worldX: number, worldY: number): boolean {
    const tile = this.getTileAt(worldX, worldY);
    if (!tile || !tile.object) {
      return false;
    }

    tile.object = null;
    return true;
  }

  public addMaterial(id: string, material: TerrainMaterial): void {
    this.materials.set(id, material);
  }

  public getMaterial(id: string): TerrainMaterial | undefined {
    return this.materials.get(id);
  }

  public getAllMaterials(): TerrainMaterial[] {
    return Array.from(this.materials.values());
  }

  public getChunks(): TerrainChunk[] {
    return Array.from(this.chunks.values());
  }

  public getConfig(): TerrainConfig {
    return { ...this.config };
  }

  public setSeed(seed: number): void {
    this.config.seed = seed;
    this.random = new SeededRandom(seed);
    this.noise = new SimplexNoise(seed);
  }

  public setScale(scale: number): void {
    this.config.scale = scale;
  }

  public setOctaves(octaves: number): void {
    this.config.octaves = octaves;
  }

  public setPersistence(persistence: number): void {
    this.config.persistence = persistence;
  }

  public setLacunarity(lacunarity: number): void {
    this.config.lacunarity = lacunarity;
  }

  public regenerate(): TerrainChunk[] {
    this.chunks.clear();
    return this.generateTerrain();
  }

  public destroy(): void {
    this.chunks.clear();
    this.materials.clear();
    this.objectTemplates.clear();
    this.decorationTemplates.clear();
  }
}

export class SeededRandom {
  private seed: number;

  constructor(seed: number) {
    this.seed = seed;
  }

  public random(): number {
    this.seed = (this.seed * 1103515245 + 12345) & 0x7fffffff;
    return this.seed / 0x7fffffff;
  }

  public range(min: number, max: number): number {
    return min + this.random() * (max - min);
  }

  public int(min: number, max: number): number {
    return Math.floor(this.range(min, max + 1));
  }

  public choice<T>(array: T[]): T {
    return array[this.int(0, array.length - 1)];
  }

  public shuffle<T>(array: T[]): T[] {
    const result = [...array];
    for (let i = result.length - 1; i > 0; i--) {
      const j = this.int(0, i);
      [result[i], result[j]] = [result[j], result[i]];
    }
    return result;
  }
}

export class SimplexNoise {
  private perm: Uint8Array;
  private permMod12: Uint8Array;

  private grad3 = [
    [1, 1, 0], [-1, 1, 0], [1, -1, 0], [-1, -1, 0],
    [1, 0, 1], [-1, 0, 1], [1, 0, -1], [-1, 0, -1],
    [0, 1, 1], [0, -1, 1], [0, 1, -1], [0, -1, -1]
  ];

  constructor(seed: number) {
    const p: number[] = [];
    for (let i = 0; i < 256; i++) {
      p[i] = i;
    }

    let s = seed;
    for (let i = 255; i > 0; i--) {
      s = (s * 1103515245 + 12345) & 0x7fffffff;
      const j = s % (i + 1);
      [p[i], p[j]] = [p[j], p[i]];
    }

    this.perm = new Uint8Array(512);
    this.permMod12 = new Uint8Array(512);
    for (let i = 0; i < 512; i++) {
      this.perm[i] = p[i & 255];
      this.permMod12[i] = this.perm[i] % 12;
    }
  }

  public noise2D(x: number, y: number): number {
    const F2 = 0.5 * (Math.sqrt(3) - 1);
    const G2 = (3 - Math.sqrt(3)) / 6;

    const s = (x + y) * F2;
    const i = Math.floor(x + s);
    const j = Math.floor(y + s);

    const t = (i + j) * G2;
    const X0 = i - t;
    const Y0 = j - t;
    const x0 = x - X0;
    const y0 = y - Y0;

    const i1 = x0 > y0 ? 1 : 0;
    const j1 = x0 > y0 ? 0 : 1;

    const x1 = x0 - i1 + G2;
    const y1 = y0 - j1 + G2;
    const x2 = x0 - 1 + 2 * G2;
    const y2 = y0 - 1 + 2 * G2;

    const ii = i & 255;
    const jj = j & 255;

    let n0 = 0, n1 = 0, n2 = 0;

    let t0 = 0.5 - x0 * x0 - y0 * y0;
    if (t0 >= 0) {
      const gi0 = this.permMod12[ii + this.perm[jj]];
      t0 *= t0;
      n0 = t0 * t0 * this.dot2(this.grad3[gi0], x0, y0);
    }

    let t1 = 0.5 - x1 * x1 - y1 * y1;
    if (t1 >= 0) {
      const gi1 = this.permMod12[ii + i1 + this.perm[jj + j1]];
      t1 *= t1;
      n1 = t1 * t1 * this.dot2(this.grad3[gi1], x1, y1);
    }

    let t2 = 0.5 - x2 * x2 - y2 * y2;
    if (t2 >= 0) {
      const gi2 = this.permMod12[ii + 1 + this.perm[jj + 1]];
      t2 *= t2;
      n2 = t2 * t2 * this.dot2(this.grad3[gi2], x2, y2);
    }

    return 70 * (n0 + n1 + n2);
  }

  private dot2(g: number[], x: number, y: number): number {
    return g[0] * x + g[1] * y;
  }
}

export const createTerrainGenerator = (config?: Partial<TerrainConfig>): TerrainGenerator => {
  return new TerrainGenerator(config);
};

export default TerrainGenerator;

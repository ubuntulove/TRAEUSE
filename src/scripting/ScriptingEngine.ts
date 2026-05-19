export interface ScriptingConfig {
  enableSandbox: boolean;
  maxExecutionTime: number;
  maxMemoryUsage: number;
  maxInstructions: number;
  allowFileAccess: boolean;
  allowNetworkAccess: boolean;
  allowEval: boolean;
  strictMode: boolean;
  cacheScripts: boolean;
  hotReload: boolean;
}

export interface Script {
  id: string;
  name: string;
  source: string;
  type: ScriptType;
  language: ScriptLanguage;
  version: string;
  author: string;
  description: string;
  tags: string[];
  dependencies: ScriptDependency[];
  entryPoint: string;
  config: Record<string, unknown>;
  compiled: CompiledScript | null;
  metadata: ScriptMetadata;
  permissions: ScriptPermission[];
}

export enum ScriptType {
  GAME = 'game',
  UI = 'ui',
  MOD = 'mod',
  PLUGIN = 'plugin',
  LIBRARY = 'library',
  TEMPLATE = 'template',
  CAMPAIGN = 'campaign',
  AI = 'ai',
  CUSTOM = 'custom',
}

export enum ScriptLanguage {
  JAVASCRIPT = 'javascript',
  TYPESCRIPT = 'typescript',
  LUA = 'lua',
  PYTHON = 'python',
}

export interface ScriptDependency {
  id: string;
  version: string;
  required: boolean;
  optional: boolean;
}

export interface CompiledScript {
  bytecode: Uint8Array;
  sourceMap: string;
  hash: string;
  timestamp: number;
}

export interface ScriptMetadata {
  createdAt: number;
  updatedAt: number;
  lastExecuted: number | null;
  executionCount: number;
  totalExecutionTime: number;
  errors: ScriptError[];
  warnings: ScriptWarning[];
}

export interface ScriptError {
  message: string;
  line: number;
  column: number;
  stack?: string;
  timestamp: number;
}

export interface ScriptWarning {
  message: string;
  line: number;
  column: number;
  timestamp: number;
}

export interface ScriptPermission {
  type: PermissionType;
  resource?: string;
  granted: boolean;
}

export enum PermissionType {
  FILE_READ = 'file_read',
  FILE_WRITE = 'file_write',
  NETWORK = 'network',
  EVAL = 'eval',
  CONSOLE = 'console',
  GAME_STATE = 'game_state',
  ENTITIES = 'entities',
  UI = 'ui',
  AUDIO = 'audio',
  STORAGE = 'storage',
}

export interface Mod {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  category: ModCategory;
  tags: string[];
  thumbnail: string;
  previewImages: string[];
  scripts: Script[];
  assets: ModAsset[];
  dependencies: ModDependency[];
  conflicts: string[];
  loadOrder: number;
  enabled: boolean;
  installed: boolean;
  verified: boolean;
  rating: number;
  downloads: number;
  subscribers: number;
  size: number;
  compatibility: CompatibilityInfo;
  changelog: ChangelogEntry[];
  metadata: ModMetadata;
}

export enum ModCategory {
  GAMEPLAY = 'gameplay',
  GRAPHICS = 'graphics',
  AUDIO = 'audio',
  UI = 'ui',
  MAPS = 'maps',
  MODELS = 'models',
  TEXTURES = 'textures',
  SCRIPTS = 'scripts',
  AI = 'ai',
  BALANCE = 'balance',
  FIXES = 'fixes',
  TOOLS = 'tools',
  MISC = 'misc',
}

export interface ModAsset {
  id: string;
  name: string;
  type: AssetType;
  path: string;
  size: number;
  format: string;
  compression: string | null;
  hash: string;
}

export enum AssetType {
  TEXTURE = 'texture',
  MODEL = 'model',
  AUDIO = 'audio',
  VIDEO = 'video',
  FONT = 'font',
  DATA = 'data',
  SCRIPT = 'script',
  SHADER = 'shader',
  ANIMATION = 'animation',
  OTHER = 'other',
}

export interface ModDependency {
  modId: string;
  version: string;
  required: boolean;
  optional: boolean;
}

export interface CompatibilityInfo {
  gameVersion: string;
  apiVersion: string;
  compatibleVersions: string[];
  conflicts: ConflictInfo[];
}

export interface ConflictInfo {
  modId: string;
  type: ConflictType;
  description: string;
  resolution?: string;
}

export enum ConflictType {
  INCOMPATIBLE = 'incompatible',
  PARTIAL = 'partial',
  REQUIRES_PATCH = 'requires_patch',
}

export interface ChangelogEntry {
  version: string;
  date: number;
  changes: string[];
  fixes: string[];
  knownIssues: string[];
}

export interface ModMetadata {
  createdAt: number;
  updatedAt: number;
  publishedAt: number;
  lastPlayed: number | null;
  playTime: number;
  ratings: RatingInfo[];
  reviews: ReviewInfo[];
  flags: ModFlags;
}

export interface RatingInfo {
  userId: string;
  rating: number;
  comment?: string;
  timestamp: number;
}

export interface ReviewInfo {
  id: string;
  userId: string;
  userName: string;
  rating: number;
  text: string;
  helpful: number;
  notHelpful: number;
  timestamp: number;
  response?: string;
}

export interface ModFlags {
  featured: boolean;
  staffPick: boolean;
  nsfw: boolean;
  deprecated: boolean;
  hidden: boolean;
  beta: boolean;
  premium: boolean;
}

export interface GameAPI {
  version: string;
  entities: EntityAPI;
  world: WorldAPI;
  ui: UIAPI;
  audio: AudioAPI;
  storage: StorageAPI;
  events: EventAPI;
  network: NetworkAPI;
  time: TimeAPI;
  input: InputAPI;
  camera: CameraAPI;
  config: ConfigAPI;
  logger: LoggerAPI;
}

export interface EntityAPI {
  create: (type: string, x: number, y: number, config?: Record<string, unknown>) => string;
  destroy: (id: string) => void;
  get: (id: string) => Entity | null;
  getAll: (filter?: EntityFilter) => Entity[];
  setPosition: (id: string, x: number, y: number) => void;
  getPosition: (id: string) => { x: number; y: number };
  setHealth: (id: string, health: number) => void;
  getHealth: (id: string) => number;
  setProperty: (id: string, key: string, value: unknown) => void;
  getProperty: (id: string, key: string) => unknown;
  addComponent: (id: string, componentType: string, config?: Record<string, unknown>) => void;
  removeComponent: (id: string, componentType: string) => void;
  hasComponent: (id: string, componentType: string) => boolean;
  emit: (id: string, event: string, data?: unknown) => void;
  findNearest: (x: number, y: number, type?: string, radius?: number) => string | null;
  findInRect: (x: number, y: number, width: number, height: number, filter?: EntityFilter) => string[];
}

export interface Entity {
  id: string;
  type: string;
  name: string;
  x: number;
  y: number;
  z: number;
  width: number;
  height: number;
  health: number;
  maxHealth: number;
  team: number;
  faction: string;
  visible: boolean;
  active: boolean;
  properties: Record<string, unknown>;
  components: string[];
}

export interface EntityFilter {
  type?: string;
  team?: number;
  faction?: string;
  name?: string;
  tags?: string[];
  position?: { x: number; y: number; radius?: number };
  health?: { min?: number; max?: number };
  properties?: Record<string, unknown>;
}

export interface WorldAPI {
  getWidth: () => number;
  getHeight: () => number;
  getTile: (x: number, y: number) => Tile | null;
  setTile: (x: number, y: number, tile: Partial<Tile>) => void;
  getTerrainHeight: (x: number, y: number) => number;
  isWalkable: (x: number, y: number) => boolean;
  isBuildable: (x: number, y: number) => boolean;
  getEntitiesAt: (x: number, y: number) => string[];
  raycast: (x1: number, y1: number, x2: number, y2: number) => RaycastResult | null;
  findPath: (startX: number, startY: number, endX: number, endY: number, config?: PathConfig) => PathResult;
  addZone: (zone: Zone) => string;
  removeZone: (id: string) => void;
  getZone: (id: string) => Zone | null;
  getZonesInRect: (x: number, y: number, width: number, height: number) => Zone[];
  spawnEffect: (type: string, x: number, y: number, config?: Record<string, unknown>) => void;
  setWeather: (weather: string, intensity: number) => void;
  getWeather: () => { type: string; intensity: number };
  setTimeOfDay: (hour: number) => void;
  getTimeOfDay: () => number;
}

export interface Tile {
  x: number;
  y: number;
  type: string;
  height: number;
  material: string;
  walkable: boolean;
  buildable: boolean;
  properties: Record<string, unknown>;
}

export interface Zone {
  id: string;
  name: string;
  type: string;
  x: number;
  y: number;
  width: number;
  height: number;
  properties: Record<string, unknown>;
}

export interface RaycastResult {
  x: number;
  y: number;
  entity: string | null;
  distance: number;
  normal: { x: number; y: number };
}

export interface PathConfig {
  allowDiagonal: boolean;
  canSwim: boolean;
  canFly: boolean;
  maxCost: number;
  heuristic: string;
}

export interface PathResult {
  found: boolean;
  waypoints: { x: number; y: number }[];
  cost: number;
  iterations: number;
}

export interface UIAPI {
  createElement: (type: string, config?: UIConfig) => UIElement;
  removeElement: (id: string) => void;
  getElement: (id: string) => UIElement | null;
  showMessage: (title: string, message: string, type?: MessageType) => void;
  showConfirm: (title: string, message: string, onConfirm: () => void, onCancel?: () => void) => void;
  showPrompt: (title: string, message: string, defaultValue?: string, onSubmit?: (value: string) => void) => void;
  showDialog: (config: DialogConfig) => DialogHandle;
  closeDialog: (id: string) => void;
  updateProgress: (id: string, progress: number) => void;
  setTooltip: (elementId: string, text: string) => void;
  showNotification: (message: string, type?: NotificationType, duration?: number) => void;
}

export interface UIElement {
  id: string;
  type: string;
  parent: string | null;
  children: string[];
  x: number;
  y: number;
  width: number;
  height: number;
  visible: boolean;
  enabled: boolean;
  properties: Record<string, unknown>;
  styles: Record<string, string>;
}

export interface UIConfig {
  parent?: string;
  x?: number;
  y?: number;
  width?: number;
  height?: number;
  text?: string;
  image?: string;
  styles?: Record<string, string>;
  properties?: Record<string, unknown>;
  events?: Record<string, (data?: unknown) => void>;
}

export enum MessageType {
  INFO = 'info',
  WARNING = 'warning',
  ERROR = 'error',
  SUCCESS = 'success',
}

export enum NotificationType {
  INFO = 'info',
  WARNING = 'warning',
  ERROR = 'error',
  SUCCESS = 'success',
  ACHIEVEMENT = 'achievement',
}

export interface DialogConfig {
  title: string;
  message?: string;
  buttons?: DialogButton[];
  closable?: boolean;
  modal?: boolean;
  width?: number;
  height?: number;
}

export interface DialogButton {
  id: string;
  text: string;
  type?: ButtonType;
  callback?: () => void;
}

export enum ButtonType {
  PRIMARY = 'primary',
  SECONDARY = 'secondary',
  DANGER = 'danger',
}

export interface DialogHandle {
  id: string;
  close: () => void;
  update: (config: Partial<DialogConfig>) => void;
}

export interface AudioAPI {
  play: (sound: string, config?: AudioConfig) => AudioHandle;
  stop: (handle: AudioHandle) => void;
  pause: (handle: AudioHandle) => void;
  resume: (handle: AudioHandle) => void;
  setVolume: (handle: AudioHandle, volume: number) => void;
  setPitch: (handle: AudioHandle, pitch: number) => void;
  setPosition: (handle: AudioHandle, x: number, y: number) => void;
  playMusic: (track: string, config?: MusicConfig) => void;
  stopMusic: (fadeOut?: number) => void;
  pauseMusic: () => void;
  resumeMusic: () => void;
  setMusicVolume: (volume: number) => void;
  isPlaying: (handle: AudioHandle) => boolean;
}

export interface AudioConfig {
  volume?: number;
  pitch?: number;
  pan?: number;
  loop?: boolean;
  position?: { x: number; y: number };
  fadeIn?: number;
}

export interface MusicConfig extends AudioConfig {
  crossfade?: number;
}

export interface AudioHandle {
  id: string;
  sound: string;
  playing: boolean;
  paused: boolean;
  volume: number;
  position: { x: number; y: number };
}

export interface StorageAPI {
  set: (key: string, value: unknown) => void;
  get: (key: string, defaultValue?: unknown) => unknown;
  delete: (key: string) => void;
  has: (key: string) => boolean;
  clear: () => void;
  getAll: () => Record<string, unknown>;
  setPersistent: (key: string, value: unknown) => Promise<void>;
  getPersistent: (key: string, defaultValue?: unknown) => Promise<unknown>;
  save: () => Promise<void>;
  load: () => Promise<void>;
}

export interface EventAPI {
  on: (event: string, callback: (data?: unknown) => void, context?: unknown) => string;
  off: (event: string, callbackId?: string) => void;
  once: (event: string, callback: (data?: unknown) => void, context?: unknown) => void;
  emit: (event: string, data?: unknown) => void;
  emitAsync: (event: string, data?: unknown) => Promise<void>;
  getListeners: (event: string) => EventListener[];
  clear: (event?: string) => void;
}

export interface EventListener {
  id: string;
  event: string;
  callback: (data?: unknown) => void;
  context: unknown;
  once: boolean;
}

export interface NetworkAPI {
  isConnected: () => boolean;
  isHost: () => boolean;
  getPlayerId: () => string;
  getPlayers: () => Player[];
  send: (type: string, data?: unknown, reliable?: boolean) => void;
  sendTo: (playerId: string, type: string, data?: unknown, reliable?: boolean) => void;
  broadcast: (type: string, data?: unknown, reliable?: boolean) => void;
  onMessage: (type: string, callback: (data: unknown, senderId: string) => void) => void;
  createLobby: (name: string, maxPlayers: number) => Promise<string>;
  joinLobby: (lobbyId: string) => Promise<void>;
  leaveLobby: () => void;
  startGame: () => void;
  setReady: (ready: boolean) => void;
}

export interface Player {
  id: string;
  name: string;
  team: number;
  ready: boolean;
  ping: number;
  isHost: boolean;
}

export interface TimeAPI {
  getTime: () => number;
  getDeltaTime: () => number;
  getScale: () => number;
  setScale: (scale: number) => void;
  getFrameCount: () => number;
  getFPS: () => number;
  isPaused: () => boolean;
  pause: () => void;
  resume: () => void;
  schedule: (callback: () => void, delay: number) => string;
  cancelSchedule: (id: string) => void;
}

export interface InputAPI {
  isKeyDown: (key: string) => boolean;
  isKeyPressed: (key: string) => boolean;
  getMouseX: () => number;
  getMouseY: () => number;
  isMouseButtonDown: (button: number) => boolean;
  isMouseButtonPressed: (button: number) => boolean;
  getMouseWheel: () => number;
  isGamepadConnected: (index: number) => boolean;
  getGamepadAxis: (index: number, axis: number) => number;
  isGamepadButtonDown: (index: number, button: number) => boolean;
  onKeyDown: (key: string, callback: () => void) => void;
  onKeyUp: (key: string, callback: () => void) => void;
  onMouseDown: (button: number, callback: () => void) => void;
  onMouseUp: (button: number, callback: () => void) => void;
  onMouseMove: (callback: (x: number, y: number) => void) => void;
}

export interface CameraAPI {
  getX: () => number;
  getY: () => number;
  getZoom: () => number;
  setPosition: (x: number, y: number, smooth?: boolean) => void;
  setZoom: (zoom: number, smooth?: boolean) => void;
  getBounds: () => { x: number; y: number; width: number; height: number };
  screenToWorld: (screenX: number, screenY: number) => { x: number; y: number };
  worldToScreen: (worldX: number, worldY: number) => { x: number; y: number };
  shake: (intensity: number, duration: number) => void;
  follow: (entityId: string | null) => void;
}

export interface ConfigAPI {
  get: (key: string, defaultValue?: unknown) => unknown;
  set: (key: string, value: unknown) => void;
  has: (key: string) => boolean;
  getAll: () => Record<string, unknown>;
  onChange: (key: string, callback: (value: unknown, oldValue: unknown) => void) => void;
}

export interface LoggerAPI {
  debug: (message: string, ...args: unknown[]) => void;
  info: (message: string, ...args: unknown[]) => void;
  warn: (message: string, ...args: unknown[]) => void;
  error: (message: string, ...args: unknown[]) => void;
}

export class ScriptEngine {
  private config: ScriptingConfig;
  private scripts: Map<string, Script> = new Map();
  private contexts: Map<string, ScriptContext> = new Map();
  private api: GameAPI;
  private eventBus: EventBus;
  private fileSystem: VirtualFileSystem;
  private compiledCache: Map<string, CompiledScript> = new Map();
  private executionStack: ExecutionFrame[] = [];

  constructor(config: Partial<ScriptingConfig> = {}, api: GameAPI) {
    this.config = {
      enableSandbox: config.enableSandbox ?? true,
      maxExecutionTime: config.maxExecutionTime ?? 5000,
      maxMemoryUsage: config.maxMemoryUsage ?? 50 * 1024 * 1024,
      maxInstructions: config.maxInstructions ?? 100000,
      allowFileAccess: config.allowFileAccess ?? false,
      allowNetworkAccess: config.allowNetworkAccess ?? false,
      allowEval: config.allowEval ?? false,
      strictMode: config.strictMode ?? false,
      cacheScripts: config.cacheScripts ?? true,
      hotReload: config.hotReload ?? true,
    };

    this.api = api;
    this.eventBus = new EventBus();
    this.fileSystem = new VirtualFileSystem();
  }

  public loadScript(source: string, language: ScriptLanguage = ScriptLanguage.JAVASCRIPT): Script {
    const script: Script = {
      id: `script_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      name: 'Unnamed Script',
      source,
      type: ScriptType.GAME,
      language,
      version: '1.0.0',
      author: 'Unknown',
      description: '',
      tags: [],
      dependencies: [],
      entryPoint: 'main',
      config: {},
      compiled: null,
      metadata: {
        createdAt: Date.now(),
        updatedAt: Date.now(),
        lastExecuted: null,
        executionCount: 0,
        totalExecutionTime: 0,
        errors: [],
        warnings: [],
      },
      permissions: [],
    };

    this.scripts.set(script.id, script);
    return script;
  }

  public compileScript(scriptId: string): boolean {
    const script = this.scripts.get(scriptId);
    if (!script) {
      return false;
    }

    try {
      const startTime = performance.now();
      let compiled: CompiledScript;

      switch (script.language) {
        case ScriptLanguage.JAVASCRIPT:
        case ScriptLanguage.TYPESCRIPT:
          compiled = this.compileJavaScript(script.source);
          break;

        case ScriptLanguage.LUA:
          compiled = this.compileLua(script.source);
          break;

        case ScriptLanguage.PYTHON:
          compiled = this.compilePython(script.source);
          break;

        default:
          throw new Error(`Unsupported language: ${script.language}`);
      }

      compiled.timestamp = Date.now();
      script.compiled = compiled;

      const endTime = performance.now();
      script.metadata.totalExecutionTime += endTime - startTime;
      script.metadata.updatedAt = Date.now();

      return true;
    } catch (error) {
      script.metadata.errors.push({
        message: String(error),
        line: 0,
        column: 0,
        timestamp: Date.now(),
      });
      return false;
    }
  }

  private compileJavaScript(source: string): CompiledScript {
    const hash = this.hashString(source);
    const cacheKey = `js_${hash}`;

    if (this.config.cacheScripts && this.compiledCache.has(cacheKey)) {
      return this.compiledCache.get(cacheKey)!;
    }

    const wrappedSource = this.wrapSource(source);

    const compiled: CompiledScript = {
      bytecode: new TextEncoder().encode(wrappedSource),
      sourceMap: '',
      hash,
      timestamp: Date.now(),
    };

    if (this.config.cacheScripts) {
      this.compiledCache.set(cacheKey, compiled);
    }

    return compiled;
  }

  private compileLua(_source: string): CompiledScript {
    return {
      bytecode: new Uint8Array(),
      sourceMap: '',
      hash: this.hashString(_source),
      timestamp: Date.now(),
    };
  }

  private compilePython(_source: string): CompiledScript {
    return {
      bytecode: new Uint8Array(),
      sourceMap: '',
      hash: this.hashString(_source),
      timestamp: Date.now(),
    };
  }

  private wrapSource(source: string): string {
    return `
      (function(api, events, storage, logger) {
        'use strict';
        ${source}
      })
    `;
  }

  public executeScript(scriptId: string, context?: Record<string, unknown>): unknown {
    const script = this.scripts.get(scriptId);
    if (!script) {
      throw new Error(`Script not found: ${scriptId}`);
    }

    if (!script.compiled) {
      if (!this.compileScript(scriptId)) {
        throw new Error(`Failed to compile script: ${scriptId}`);
      }
    }

    script.metadata.executionCount++;
    script.metadata.lastExecuted = Date.now();

    const startTime = performance.now();

    const frame: ExecutionFrame = {
      scriptId,
      startTime,
      instructionCount: 0,
      memoryUsage: 0,
      paused: false,
      error: null,
    };

    this.executionStack.push(frame);

    try {
      const contextObj = this.createSandboxedContext(script, context);
      const wrappedFn = new Function(
        'api',
        'events',
        'storage',
        'logger',
        new TextDecoder().decode(script.compiled!.bytecode)
      );

      const result = wrappedFn(
        this.api,
        this.eventBus,
        new StorageAdapter(this.api.storage),
        this.createLogger(script)
      );

      return result;
    } catch (error) {
      frame.error = String(error);
      script.metadata.errors.push({
        message: String(error),
        line: 0,
        column: 0,
        stack: (error as Error).stack,
        timestamp: Date.now(),
      });
      throw error;
    } finally {
      const endTime = performance.now();
      frame.endTime = endTime;
      this.executionStack.pop();

      script.metadata.totalExecutionTime += endTime - startTime;
    }
  }

  private createSandboxedContext(script: Script, context?: Record<string, unknown>): Record<string, unknown> {
    const sandbox: Record<string, unknown> = {
      console: this.createConsole(script),
      Math,
      Date,
      JSON,
      Array,
      Object,
      String,
      Number,
      Boolean,
      RegExp,
      Error,
      TypeError,
      RangeError,
      SyntaxError,
      Promise,
      setTimeout,
      setInterval,
      clearTimeout,
      clearInterval,
      ...context,
    };

    if (this.config.allowFileAccess) {
      sandbox.fileSystem = this.fileSystem;
    }

    if (!this.config.allowEval) {
      delete sandbox.eval;
      delete sandbox.Function;
    }

    return sandbox;
  }

  private createConsole(script: Script): Console {
    return {
      log: (...args: unknown[]) => this.log(script, 'info', args),
      debug: (...args: unknown[]) => this.log(script, 'debug', args),
      info: (...args: unknown[]) => this.log(script, 'info', args),
      warn: (...args: unknown[]) => this.log(script, 'warn', args),
      error: (...args: unknown[]) => this.log(script, 'error', args),
      group: () => {},
      groupEnd: () => {},
      table: (data: unknown) => this.log(script, 'info', [JSON.stringify(data, null, 2)]),
      time: (_label?: string) => {},
      timeEnd: (_label?: string) => {},
      assert: (condition: boolean, ...args: unknown[]) => {
        if (!condition) {
          this.log(script, 'error', args);
        }
      },
      clear: () => {},
      count: (_label?: string) => 0,
      dir: (obj: unknown) => this.log(script, 'info', [JSON.stringify(obj)]),
    };
  }

  private log(script: Script, level: string, args: unknown[]): void {
    const message = args.map(arg => {
      if (typeof arg === 'object') {
        try {
          return JSON.stringify(arg);
        } catch {
          return String(arg);
        }
      }
      return String(arg);
    }).join(' ');

    console[level as keyof Console]?.(`[${script.name}] ${message}`);
  }

  private createLogger(script: Script): GameAPI['logger'] {
    return {
      debug: (message: string, ...args: unknown[]) => this.log(script, 'debug', [message, ...args]),
      info: (message: string, ...args: unknown[]) => this.log(script, 'info', [message, ...args]),
      warn: (message: string, ...args: unknown[]) => this.log(script, 'warn', [message, ...args]),
      error: (message: string, ...args: unknown[]) => this.log(script, 'error', [message, ...args]),
    };
  }

  public stopScript(scriptId: string): boolean {
    const frameIndex = this.executionStack.findIndex(f => f.scriptId === scriptId);
    if (frameIndex === -1) {
      return false;
    }

    const frame = this.executionStack[frameIndex];
    frame.paused = true;
    frame.error = 'Script execution stopped';

    return true;
  }

  public stopAllScripts(): void {
    this.executionStack.forEach(frame => {
      frame.paused = true;
      frame.error = 'All scripts stopped';
    });
    this.executionStack = [];
  }

  public getScript(scriptId: string): Script | undefined {
    return this.scripts.get(scriptId);
  }

  public getAllScripts(): Script[] {
    return Array.from(this.scripts.values());
  }

  public removeScript(scriptId: string): boolean {
    const script = this.scripts.get(scriptId);
    if (!script) {
      return false;
    }

    this.stopScript(scriptId);
    this.scripts.delete(scriptId);
    return true;
  }

  public reloadScript(scriptId: string): boolean {
    const script = this.scripts.get(scriptId);
    if (!script) {
      return false;
    }

    this.stopScript(scriptId);
    return this.compileScript(scriptId);
  }

  public update(): void {
    this.executionStack = this.executionStack.filter(frame => {
      if (frame.paused) {
        return true;
      }

      const executionTime = performance.now() - frame.startTime;
      if (executionTime > this.config.maxExecutionTime) {
        frame.error = 'Script execution timeout';
        return false;
      }

      return true;
    });
  }

  private hashString(str: string): string {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash;
    }
    return Math.abs(hash).toString(16);
  }

  public getAPI(): GameAPI {
    return this.api;
  }

  public destroy(): void {
    this.stopAllScripts();
    this.scripts.clear();
    this.contexts.clear();
    this.compiledCache.clear();
  }
}

export interface ExecutionFrame {
  scriptId: string;
  startTime: number;
  endTime?: number;
  instructionCount: number;
  memoryUsage: number;
  paused: boolean;
  error: string | null;
}

export class EventBus {
  private listeners: Map<string, EventListener[]> = new Map();

  public on(event: string, callback: (data?: unknown) => void, context?: unknown): string {
    const id = `${event}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }

    this.listeners.get(event)!.push({ id, callback, context, once: false });

    return id;
  }

  public once(event: string, callback: (data?: unknown) => void, context?: unknown): string {
    const id = this.on(event, callback, context);
    const listeners = this.listeners.get(event);
    if (listeners) {
      const listener = listeners.find(l => l.id === id);
      if (listener) {
        listener.once = true;
      }
    }
    return id;
  }

  public off(event: string, id?: string): void {
    if (!id) {
      this.listeners.delete(event);
      return;
    }

    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.findIndex(l => l.id === id);
      if (index !== -1) {
        listeners.splice(index, 1);
      }
    }
  }

  public emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (!listeners) {
      return;
    }

    for (let i = listeners.length - 1; i >= 0; i--) {
      const listener = listeners[i];
      try {
        listener.callback.call(listener.context, data);
      } catch (error) {
        console.error(`Error in event listener: ${error}`);
      }

      if (listener.once) {
        listeners.splice(i, 1);
      }
    }
  }

  public getListeners(event?: string): EventListener[] {
    if (event) {
      return this.listeners.get(event) || [];
    }
    return Array.from(this.listeners.values()).flat();
  }

  public clear(): void {
    this.listeners.clear();
  }
}

export class VirtualFileSystem {
  private files: Map<string, FileData> = new Map();

  public read(path: string): string | null {
    const file = this.files.get(path);
    return file?.content || null;
  }

  public write(path: string, content: string): void {
    this.files.set(path, { content, modified: Date.now() });
  }

  public exists(path: string): boolean {
    return this.files.has(path);
  }

  public delete(path: string): boolean {
    return this.files.delete(path);
  }

  public list(path: string): string[] {
    return Array.from(this.files.keys()).filter(k => k.startsWith(path));
  }

  public clear(): void {
    this.files.clear();
  }
}

export interface FileData {
  content: string;
  modified: number;
}

export class StorageAdapter {
  private storage: StorageAPI;

  constructor(storage: StorageAPI) {
    this.storage = storage;
  }

  public get(key: string, defaultValue?: unknown): unknown {
    return this.storage.get(key, defaultValue);
  }

  public set(key: string, value: unknown): void {
    this.storage.set(key, value);
  }

  public delete(key: string): void {
    this.storage.delete(key);
  }

  public has(key: string): boolean {
    return this.storage.has(key);
  }

  public clear(): void {
    this.storage.clear();
  }
}

export class ModManager {
  private mods: Map<string, Mod> = new Map();
  private scriptEngine: ScriptEngine;
  private loadOrder: string[] = [];
  private activeModIds: Set<string> = new Set();

  constructor(scriptEngine: ScriptEngine) {
    this.scriptEngine = scriptEngine;
  }

  public loadMod(mod: Mod): boolean {
    try {
      this.validateMod(mod);

      this.mods.set(mod.id, mod);

      if (mod.enabled && mod.installed) {
        this.enableMod(mod.id);
      }

      return true;
    } catch (error) {
      console.error(`Failed to load mod: ${error}`);
      return false;
    }
  }

  public unloadMod(modId: string): boolean {
    const mod = this.mods.get(modId);
    if (!mod) {
      return false;
    }

    this.disableMod(modId);
    this.mods.delete(modId);
    return true;
  }

  public enableMod(modId: string): boolean {
    const mod = this.mods.get(modId);
    if (!mod || !mod.installed) {
      return false;
    }

    for (const dep of mod.dependencies) {
      if (dep.required && !this.activeModIds.has(dep.modId)) {
        return false;
      }
    }

    for (const conflict of mod.conflicts) {
      if (this.activeModIds.has(conflict)) {
        return false;
      }
    }

    this.activeModIds.add(modId);
    mod.enabled = true;

    for (const script of mod.scripts) {
      this.scriptEngine.loadScript(script.source, script.language);
    }

    this.reorderMods();
    this.updateLoadOrder();

    return true;
  }

  public disableMod(modId: string): boolean {
    const mod = this.mods.get(modId);
    if (!mod) {
      return false;
    }

    for (const otherMod of this.mods.values()) {
      for (const dep of otherMod.dependencies) {
        if (dep.modId === modId && dep.required && otherMod.enabled) {
          return false;
        }
      }
    }

    this.activeModIds.delete(modId);
    mod.enabled = false;

    for (const script of mod.scripts) {
      this.scriptEngine.removeScript(script.id);
    }

    this.updateLoadOrder();

    return true;
  }

  private validateMod(mod: Mod): void {
    if (!mod.id || !mod.name || !mod.version) {
      throw new Error('Mod missing required fields');
    }
  }

  private reorderMods(): void {
    const sorted = Array.from(this.mods.values())
      .filter(m => m.enabled)
      .sort((a, b) => {
        if (this.dependsOn(a.id, b.id)) return 1;
        if (this.dependsOn(b.id, a.id)) return -1;
        return a.loadOrder - b.loadOrder;
      });

    sorted.forEach((mod, index) => {
      mod.loadOrder = index;
    });
  }

  private dependsOn(modId: string, dependencyId: string): boolean {
    const mod = this.mods.get(modId);
    if (!mod) return false;

    return mod.dependencies.some(dep => dep.modId === dependencyId);
  }

  private updateLoadOrder(): void {
    this.loadOrder = Array.from(this.mods.values())
      .filter(m => m.enabled)
      .sort((a, b) => a.loadOrder - b.loadOrder)
      .map(m => m.id);
  }

  public getMod(modId: string): Mod | undefined {
    return this.mods.get(modId);
  }

  public getAllMods(): Mod[] {
    return Array.from(this.mods.values());
  }

  public getEnabledMods(): Mod[] {
    return this.loadOrder.map(id => this.mods.get(id)!).filter(Boolean);
  }

  public getLoadOrder(): string[] {
    return [...this.loadOrder];
  }

  public isModEnabled(modId: string): boolean {
    return this.activeModIds.has(modId);
  }

  public getModConflicts(modId: string): ConflictInfo[] {
    const mod = this.mods.get(modId);
    if (!mod) return [];

    const conflicts: ConflictInfo[] = [];

    for (const conflictId of mod.conflicts) {
      const conflictMod = this.mods.get(conflictId);
      if (conflictMod && conflictMod.enabled) {
        conflicts.push({
          modId: conflictId,
          type: ConflictType.INCOMPATIBLE,
          description: `${mod.name} conflicts with ${conflictMod.name}`,
        });
      }
    }

    return conflicts;
  }

  public checkCompatibility(modId: string): CompatibilityInfo | null {
    const mod = this.mods.get(modId);
    return mod?.compatibility || null;
  }

  public destroy(): void {
    this.mods.clear();
    this.activeModIds.clear();
    this.loadOrder = [];
  }
}

export const createScriptEngine = (config?: Partial<ScriptingConfig>, api?: GameAPI): ScriptEngine => {
  return new ScriptEngine(config, api!);
};

export const createModManager = (scriptEngine: ScriptEngine): ModManager => {
  return new ModManager(scriptEngine);
};

export default ScriptEngine;

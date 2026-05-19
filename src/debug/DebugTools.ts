export interface DebugConfig {
  enabled: boolean;
  showFps: boolean;
  showMemory: boolean;
  showNetwork: boolean;
  showPhysics: boolean;
  showEntities: boolean;
  showAudio: boolean;
  showRender: boolean;
  showAI: boolean;
  showCollisions: boolean;
  showPaths: boolean;
  showZones: boolean;
  showGrid: boolean;
  showCoordinates: boolean;
  showBoundingBoxes: boolean;
  showHealthBars: boolean;
  showNames: boolean;
  showVision: boolean;
  autoOpen: boolean;
  maxLogEntries: number;
  logLevel: LogLevel;
  consoleKey: string;
  profilerKey: string;
  debugOverlayKey: string;
}

export enum LogLevel {
  DEBUG = 0,
  INFO = 1,
  WARN = 2,
  ERROR = 3,
  FATAL = 4,
}

export interface LogEntry {
  id: string;
  timestamp: number;
  level: LogLevel;
  category: string;
  message: string;
  data?: unknown;
  stack?: string;
  source?: string;
  count?: number;
}

export interface PerformanceMetric {
  name: string;
  value: number;
  min: number;
  max: number;
  average: number;
  samples: number[];
  unit: string;
  category: MetricCategory;
}

export enum MetricCategory {
  FRAME = 'frame',
  RENDER = 'render',
  PHYSICS = 'physics',
  AI = 'ai',
  AUDIO = 'audio',
  NETWORK = 'network',
  MEMORY = 'memory',
  GAME = 'game',
  CUSTOM = 'custom',
}

export interface FrameMetrics {
  deltaTime: number;
  fps: number;
  frameTime: number;
  fixedUpdateTime: number;
  variableUpdateTime: number;
  renderTime: number;
  gpuTime: number;
  totalTime: number;
  skipRate: number;
}

export interface MemoryMetrics {
  totalHeap: number;
  usedHeap: number;
  heapSizeLimit: number;
  externalMemory: number;
  arrayBuffers: number;
  rss: number;
  heapUsed: number;
  heapAvailable: number;
}

export interface RenderMetrics {
  drawCalls: number;
  triangles: number;
  vertices: number;
  textures: number;
  textureMemory: number;
  shaders: number;
  meshes: number;
  particles: number;
  sprites: number;
  uiElements: number;
  lightCount: number;
  shadowCasters: number;
  visibleObjects: number;
  culledObjects: number;
  batchSize: number;
}

export interface PhysicsMetrics {
  bodies: number;
  activeBodies: number;
  sleepingBodies: number;
  constraints: number;
  collisions: number;
  narrowPhase: number;
  broadPhase: number;
  islandCount: number;
  sleepTime: number;
}

export interface NetworkMetrics {
  packetsSent: number;
  packetsReceived: number;
  bytesSent: number;
  bytesReceived: number;
  ping: number;
  jitter: number;
  packetLoss: number;
  latency: number;
  bandwidth: number;
  connections: number;
  disconnections: number;
  errors: number;
}

export interface AudioMetrics {
  sources: number;
  playing: number;
  paused: number;
  stopped: number;
  memoryUsage: number;
  cpuUsage: number;
  streams: number;
  effects: number;
}

export interface AIMetrics {
  entities: number;
  activeBehaviors: number;
  decisionsPerSecond: number;
  pathfindingRequests: number;
  pathfindingTime: number;
  pathLength: number;
  memoryUsage: number;
  nodesEvaluated: number;
}

export interface DebugPanel {
  id: string;
  title: string;
  position: Vec2;
  size: Vec2;
  collapsed: boolean;
  visible: boolean;
  resizable: boolean;
  draggable: boolean;
  children: DebugPanel[];
  content: DebugPanelContent;
  style: DebugPanelStyle;
}

export interface Vec2 {
  x: number;
  y: number;
}

export interface DebugPanelContent {
  type: PanelContentType;
  data: unknown;
  refreshRate: number;
}

export enum PanelContentType {
  GRAPH = 'graph',
  LIST = 'list',
  TREE = 'tree',
  TABLE = 'table',
  TEXT = 'text',
  CHART = 'chart',
  HEATMAP = 'heatmap',
  PROFILER = 'profiler',
  LOGS = 'logs',
}

export interface DebugPanelStyle {
  backgroundColor: string;
  borderColor: string;
  borderWidth: number;
  borderRadius: number;
  padding: number;
  fontFamily: string;
  fontSize: number;
  textColor: string;
}

export interface DebugCommand {
  name: string;
  description: string;
  syntax: string;
  aliases: string[];
  execute: (args: string[]) => CommandResult;
  category: CommandCategory;
  flags: CommandFlag[];
}

export enum CommandCategory {
  GENERAL = 'general',
  GAME = 'game',
  RENDER = 'render',
  PHYSICS = 'physics',
  AI = 'ai',
  NETWORK = 'network',
  AUDIO = 'audio',
  DEBUG = 'debug',
  CONSOLE = 'console',
}

export enum CommandFlag {
  HIDDEN = 'hidden',
  CHEAT = 'cheat',
  NETWORK = 'network',
  SERVER = 'server',
  CLIENT = 'client',
  DEVELOPER = 'developer',
}

export interface CommandResult {
  success: boolean;
  output: string;
  error?: string;
  data?: unknown;
}

export interface Breakpoint {
  id: string;
  file: string;
  line: number;
  condition?: string;
  enabled: boolean;
  hitCount: number;
  hitCondition?: HitCondition;
  logMessage?: string;
  temporary: boolean;
  createdAt: number;
}

export enum HitCondition {
  ALWAYS = 'always',
  EQUALS = 'equals',
  NOT_EQUALS = 'not_equals',
  GREATER_THAN = 'greater_than',
  LESS_THAN = 'less_than',
  MODULO = 'modulo',
}

export interface Watchpoint {
  id: string;
  variable: string;
  expression: string;
  scope: WatchScope;
  enabled: boolean;
  hitCount: number;
  oldValue?: unknown;
  newValue?: unknown;
  conditions: string[];
}

export enum WatchScope {
  GLOBAL = 'global',
  LOCAL = 'local',
  ENTITY = 'entity',
  CLASS = 'class',
}

export interface ProfilerSample {
  name: string;
  startTime: number;
  endTime: number;
  duration: number;
  depth: number;
  parent?: string;
  children: ProfilerSample[];
  hitCount: number;
  minDuration: number;
  maxDuration: number;
  totalDuration: number;
  averageDuration: number;
}

export interface DebuggerState {
  isPaused: boolean;
  isStepping: boolean;
  stepMode: StepMode;
  currentLine: number;
  currentFile: string;
  callStack: CallStackFrame[];
  variables: Map<string, unknown>;
  watchpoints: Watchpoint[];
  breakpoints: Breakpoint[];
  exceptionBreakpoints: boolean;
  pauseOnException: boolean;
}

export enum StepMode {
  NONE = 'none',
  OVER = 'over',
  INTO = 'into',
  OUT = 'out',
}

export interface CallStackFrame {
  id: string;
  functionName: string;
  file: string;
  line: number;
  column: number;
  scope: string;
  locals: Map<string, unknown>;
}

export interface MemorySnapshot {
  id: string;
  timestamp: number;
  heapUsed: number;
  heapTotal: number;
  external: number;
  arrays: MemoryAllocation[];
  objects: MemoryAllocation[];
  functions: MemoryAllocation[];
  strings: MemoryAllocation[];
}

export interface MemoryAllocation {
  id: string;
  type: string;
  name: string;
  size: number;
  count: number;
  percentage: number;
  retainedBy: string[];
}

export interface PerformanceSnapshot {
  id: string;
  timestamp: number;
  frameTime: number;
  fps: number;
  metrics: Record<string, number>;
}

export interface NetworkPacket {
  id: string;
  type: string;
  size: number;
  timestamp: number;
  direction: PacketDirection;
  sequence: number;
  channel: string;
}

export enum PacketDirection {
  INCOMING = 'incoming',
  OUTGOING = 'outgoing',
}

export interface EntityDebugInfo {
  id: string;
  name: string;
  type: string;
  position: Vec2;
  velocity: Vec2;
  health: number;
  maxHealth: number;
  state: string;
  faction: string;
  aiState?: string;
  components: ComponentDebugInfo[];
}

export interface ComponentDebugInfo {
  name: string;
  type: string;
  properties: Record<string, unknown>;
  enabled: boolean;
}

export interface CollisionDebugInfo {
  entityA: string;
  entityB: string;
  point: Vec2;
  normal: Vec2;
  penetration: number;
  manifold: Vec2[];
}

export interface PathDebugInfo {
  start: Vec2;
  end: Vec2;
  waypoints: Vec2[];
  cost: number;
  iterations: number;
  time: number;
  found: boolean;
}

export interface AIDebugInfo {
  entityId: string;
  behavior: string;
  state: string;
  target?: string;
  priority: number;
  confidence: number;
  decisions: AIDecision[];
  memory: AIMemoryEntry[];
}

export interface AIDecision {
  type: string;
  reason: string;
  utility: number;
  action: string;
}

export interface AIMemoryEntry {
  key: string;
  value: unknown;
  age: number;
  importance: number;
}

export interface DebugCamera {
  position: Vec2;
  zoom: number;
  rotation: number;
  fov: number;
  following?: string;
  locked: boolean;
  smoothFollow: boolean;
  followSpeed: number;
}

export class DebugConsole {
  private config: DebugConfig;
  private isOpen: boolean = false;
  private inputHistory: string[] = [];
  private historyIndex: number = -1;
  private autocompleteCandidates: string[] = [];
  private commandHandlers: Map<string, DebugCommand> = new Map();
  private eventListeners: Map<string, Set<Function>> = new Map();
  private keyBindings: Map<string, string> = new Map();
  private registeredCategories: Set<string> = new Set();
  private inputCallback?: (input: string) => void;
  private closeCallback?: () => void;

  constructor(config: Partial<DebugConfig> = {}) {
    this.config = {
      enabled: config.enabled ?? true,
      showFps: config.showFps ?? true,
      showMemory: config.showMemory ?? true,
      showNetwork: config.showNetwork ?? false,
      showPhysics: config.showPhysics ?? false,
      showEntities: config.showEntities ?? false,
      showAudio: config.showAudio ?? false,
      showRender: config.showRender ?? false,
      showAI: config.showAI ?? false,
      showCollisions: config.showCollisions ?? false,
      showPaths: config.showPaths ?? false,
      showZones: config.showZones ?? false,
      showGrid: config.showGrid ?? false,
      showCoordinates: config.showCoordinates ?? false,
      showBoundingBoxes: config.showBoundingBoxes ?? false,
      showHealthBars: config.showHealthBars ?? false,
      showNames: config.showNames ?? false,
      showVision: config.showVision ?? false,
      autoOpen: config.autoOpen ?? false,
      maxLogEntries: config.maxLogEntries ?? 1000,
      logLevel: config.logLevel ?? LogLevel.INFO,
      consoleKey: config.consoleKey ?? 'F1',
      profilerKey: config.profilerKey ?? 'F2',
      debugOverlayKey: config.debugOverlayKey ?? 'F3',
    };

    this.initializeDefaultCommands();
    this.initializeKeyBindings();
  }

  private initializeDefaultCommands(): void {
    this.registerCommand({
      name: 'help',
      description: 'Display help information',
      syntax: 'help [command]',
      aliases: ['?', 'h'],
      category: CommandCategory.CONSOLE,
      flags: [],
      execute: (args) => this.executeHelp(args),
    });

    this.registerCommand({
      name: 'clear',
      description: 'Clear the console',
      syntax: 'clear',
      aliases: ['cls'],
      category: CommandCategory.CONSOLE,
      flags: [],
      execute: () => this.executeClear(),
    });

    this.registerCommand({
      name: 'set',
      description: 'Set a configuration value',
      syntax: 'set <key> <value>',
      aliases: [],
      category: CommandCategory.CONSOLE,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeSet(args),
    });

    this.registerCommand({
      name: 'get',
      description: 'Get a configuration value',
      syntax: 'get <key>',
      aliases: [],
      category: CommandCategory.CONSOLE,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeGet(args),
    });

    this.registerCommand({
      name: 'echo',
      description: 'Print text to the console',
      syntax: 'echo <message>',
      aliases: ['print', 'log'],
      category: CommandCategory.CONSOLE,
      flags: [],
      execute: (args) => this.executeEcho(args),
    });

    this.registerCommand({
      name: 'time',
      description: 'Timer operations',
      syntax: 'time <start|stop|reset|print>',
      aliases: [],
      category: CommandCategory.CONSOLE,
      flags: [],
      execute: (args) => this.executeTime(args),
    });

    this.registerCommand({
      name: 'exec',
      description: 'Execute JavaScript code',
      syntax: 'exec <code>',
      aliases: ['eval', 'js'],
      category: CommandCategory.CONSOLE,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeExec(args),
    });

    this.registerCommand({
      name: 'spawn',
      description: 'Spawn an entity',
      syntax: 'spawn <type> [count] [x] [y]',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [CommandFlag.CHEAT, CommandFlag.DEVELOPER],
      execute: (args) => this.executeSpawn(args),
    });

    this.registerCommand({
      name: 'kill',
      description: 'Kill entities',
      syntax: 'kill [id|all]',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [CommandFlag.CHEAT, CommandFlag.DEVELOPER],
      execute: (args) => this.executeKill(args),
    });

    this.registerCommand({
      name: 'give',
      description: 'Give resources or items',
      syntax: 'give <resource|item> <amount>',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [CommandFlag.CHEAT, CommandFlag.DEVELOPER],
      execute: (args) => this.executeGive(args),
    });

    this.registerCommand({
      name: 'tp',
      description: 'Teleport player or entity',
      syntax: 'tp [target] <x> <y>',
      aliases: ['teleport'],
      category: CommandCategory.GAME,
      flags: [CommandFlag.CHEAT, CommandFlag.DEVELOPER],
      execute: (args) => this.executeTeleport(args),
    });

    this.registerCommand({
      name: 'speed',
      description: 'Set game speed',
      syntax: 'speed <multiplier>',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [CommandFlag.CHEAT],
      execute: (args) => this.executeSpeed(args),
    });

    this.registerCommand({
      name: 'pause',
      description: 'Pause or unpause the game',
      syntax: 'pause [toggle]',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [CommandFlag.CHEAT],
      execute: (args) => this.executePause(args),
    });

    this.registerCommand({
      name: 'list',
      description: 'List entities, commands, or other items',
      syntax: 'list <entities|commands|settings>',
      aliases: ['ls', 'l'],
      category: CommandCategory.CONSOLE,
      flags: [],
      execute: (args) => this.executeList(args),
    });

    this.registerCommand({
      name: 'inspect',
      description: 'Inspect an entity or component',
      syntax: 'inspect <entityId>',
      aliases: ['i'],
      category: CommandCategory.DEBUG,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeInspect(args),
    });

    this.registerCommand({
      name: 'stats',
      description: 'Show performance statistics',
      syntax: 'stats [reset]',
      aliases: ['st'],
      category: CommandCategory.DEBUG,
      flags: [],
      execute: (args) => this.executeStats(args),
    });

    this.registerCommand({
      name: 'profile',
      description: 'Profile performance',
      syntax: 'profile <start|stop|snapshot>',
      aliases: ['pf'],
      category: CommandCategory.DEBUG,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeProfile(args),
    });

    this.registerCommand({
      name: 'mem',
      description: 'Memory information',
      syntax: 'mem [gc|snapshot|leak]',
      aliases: [],
      category: CommandCategory.DEBUG,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeMemory(args),
    });

    this.registerCommand({
      name: 'net',
      description: 'Network information',
      syntax: 'net [connect|disconnect|stats|ping]',
      aliases: [],
      category: CommandCategory.NETWORK,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeNetwork(args),
    });

    this.registerCommand({
      name: 'save',
      description: 'Save game or data',
      syntax: 'save <game|config|slot>',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [],
      execute: (args) => this.executeSave(args),
    });

    this.registerCommand({
      name: 'load',
      description: 'Load game or data',
      syntax: 'load <game|config|slot>',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [],
      execute: (args) => this.executeLoad(args),
    });

    this.registerCommand({
      name: 'ai',
      description: 'AI control commands',
      syntax: 'ai <enable|disable|dump|pathfinding>',
      aliases: [],
      category: CommandCategory.AI,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeAI(args),
    });

    this.registerCommand({
      name: 'render',
      description: 'Render debug options',
      syntax: 'render <option> [value]',
      aliases: [],
      category: CommandCategory.RENDER,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeRender(args),
    });

    this.registerCommand({
      name: 'physics',
      description: 'Physics debug options',
      syntax: 'physics <option> [value]',
      aliases: [],
      category: CommandCategory.PHYSICS,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executePhysics(args),
    });

    this.registerCommand({
      name: 'audio',
      description: 'Audio debug options',
      syntax: 'audio <play|stop|volume>',
      aliases: [],
      category: CommandCategory.AUDIO,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeAudio(args),
    });

    this.registerCommand({
      name: 'reload',
      description: 'Reload configurations or resources',
      syntax: 'reload <textures|shaders|audio|all>',
      aliases: [],
      category: CommandCategory.GAME,
      flags: [CommandFlag.DEVELOPER],
      execute: (args) => this.executeReload(args),
    });
  }

  private initializeKeyBindings(): void {
    this.keyBindings.set('Tab', 'autocomplete');
    this.keyBindings.set('Escape', 'close');
    this.keyBindings.set('Enter', 'execute');
    this.keyBindings.set('ArrowUp', 'history_prev');
    this.keyBindings.set('ArrowDown', 'history_next');
    this.keyBindings.set('ArrowLeft', 'cursor_left');
    this.keyBindings.set('ArrowRight', 'cursor_right');
    this.keyBindings.set('Home', 'cursor_home');
    this.keyBindings.set('End', 'cursor_end');
    this.keyBindings.set('Delete', 'delete');
    this.keyBindings.set('Backspace', 'backspace');
  }

  public open(): void {
    this.isOpen = true;
    this.emit('open');
  }

  public close(): void {
    this.isOpen = false;
    this.emit('close');
  }

  public toggle(): void {
    if (this.isOpen) {
      this.close();
    } else {
      this.open();
    }
  }

  public isOpenState(): boolean {
    return this.isOpen;
  }

  public execute(input: string): CommandResult {
    const trimmed = input.trim();
    if (!trimmed) {
      return { success: false, output: '', error: 'No command entered' };
    }

    this.inputHistory.unshift(trimmed);
    if (this.inputHistory.length > 100) {
      this.inputHistory.pop();
    }
    this.historyIndex = -1;

    const parts = this.parseCommand(trimmed);
    const commandName = parts[0].toLowerCase();
    const args = parts.slice(1);

    const command = this.findCommand(commandName);
    if (!command) {
      return { success: false, output: '', error: `Unknown command: ${commandName}` };
    }

    try {
      return command.execute(args);
    } catch (error) {
      return { success: false, output: '', error: String(error) };
    }
  }

  private parseCommand(input: string): string[] {
    const parts: string[] = [];
    let current = '';
    let inString = false;
    let stringChar = '';

    for (let i = 0; i < input.length; i++) {
      const char = input[i];

      if (!inString && (char === '"' || char === "'")) {
        inString = true;
        stringChar = char;
      } else if (inString && char === stringChar) {
        inString = false;
      } else if (!inString && char === ' ') {
        if (current) {
          parts.push(current);
          current = '';
        }
      } else {
        current += char;
      }
    }

    if (current) {
      parts.push(current);
    }

    return parts;
  }

  private findCommand(name: string): DebugCommand | undefined {
    const command = this.commandHandlers.get(name);
    if (command) {
      return command;
    }

    for (const [cmdName, cmd] of this.commandHandlers) {
      if (cmd.aliases.includes(name)) {
        return cmd;
      }
    }

    return undefined;
  }

  public registerCommand(command: DebugCommand): void {
    this.commandHandlers.set(command.name.toLowerCase(), command);
    this.registeredCategories.add(command.category);

    command.aliases.forEach(alias => {
      this.commandHandlers.set(alias.toLowerCase(), command);
    });
  }

  public unregisterCommand(name: string): boolean {
    const command = this.commandHandlers.get(name);
    if (command) {
      command.aliases.forEach(alias => {
        this.commandHandlers.delete(alias.toLowerCase());
      });
      this.registeredCategories.delete(command.category);
      return this.commandHandlers.delete(name.toLowerCase());
    }
    return false;
  }

  public getCommands(category?: CommandCategory): DebugCommand[] {
    const commands = Array.from(this.commandHandlers.values());
    const uniqueCommands = commands.filter((cmd, index, self) =>
      index === self.findIndex(c => c.name === cmd.name)
    );

    if (category) {
      return uniqueCommands.filter(cmd => cmd.category === category);
    }

    return uniqueCommands;
  }

  public getAutocompleteCandidates(input: string): string[] {
    const lower = input.toLowerCase();
    const candidates: string[] = [];

    this.commandHandlers.forEach((cmd, name) => {
      if (name.startsWith(lower) || cmd.aliases.some(a => a.startsWith(lower))) {
        candidates.push(cmd.name);
      }
    });

    return candidates;
  }

  private executeHelp(args: string[]): CommandResult {
    if (args.length === 0) {
      const categories = Array.from(this.registeredCategories);
      let output = 'Available categories:\n';
      categories.forEach(cat => {
        output += `  ${cat}\n`;
      });
      output += '\nType "help <category>" for commands in that category.';
      return { success: true, output };
    }

    const category = args[0].toLowerCase();
    const commands = this.getCommands(category as CommandCategory);

    if (commands.length === 0) {
      const cmd = this.findCommand(category);
      if (cmd) {
        return {
          success: true,
          output: `${cmd.name}: ${cmd.description}\nSyntax: ${cmd.syntax}\nAliases: ${cmd.aliases.join(', ') || 'none'}`,
        };
      }
      return { success: false, output: '', error: `Unknown category or command: ${category}` };
    }

    let output = `Commands in ${category}:\n`;
    commands.forEach(cmd => {
      output += `  ${cmd.name} - ${cmd.description}\n`;
    });

    return { success: true, output };
  }

  private executeClear(): CommandResult {
    this.emit('clear');
    return { success: true, output: '' };
  }

  private executeSet(args: string[]): CommandResult {
    if (args.length < 2) {
      return { success: false, output: '', error: 'Usage: set <key> <value>' };
    }

    const key = args[0];
    const value = args[1];

    (this.config as Record<string, unknown>)[key] = this.parseValue(value);
    this.emit('config_change', { key, value: this.config[key as keyof DebugConfig] });

    return { success: true, output: `Set ${key} = ${value}` };
  }

  private executeGet(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: get <key>' };
    }

    const key = args[0];
    const value = (this.config as Record<string, unknown>)[key];

    if (value === undefined) {
      return { success: false, output: '', error: `Unknown config key: ${key}` };
    }

    return { success: true, output: `${key} = ${JSON.stringify(value)}` };
  }

  private parseValue(value: string): unknown {
    if (value === 'true') return true;
    if (value === 'false') return false;
    if (value === 'null') return null;
    if (value === 'undefined') return undefined;
    if (!isNaN(Number(value)) && value !== '') return Number(value);

    if ((value.startsWith('"') && value.endsWith('"')) ||
        (value.startsWith("'") && value.endsWith("'"))) {
      return value.slice(1, -1);
    }

    return value;
  }

  private executeEcho(args: string[]): CommandResult {
    const message = args.join(' ');
    this.emit('echo', { message });
    return { success: true, output: message };
  }

  private timers: Map<string, number> = new Map();

  private executeTime(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: time <start|stop|reset|print>' };
    }

    const action = args[0].toLowerCase();

    switch (action) {
      case 'start':
        const startName = args.slice(1).join(' ') || 'default';
        this.timers.set(startName, performance.now());
        return { success: true, output: `Timer '${startName}' started` };

      case 'stop':
        const stopName = args.slice(1).join(' ') || 'default';
        const startTime = this.timers.get(stopName);
        if (startTime === undefined) {
          return { success: false, output: '', error: `Timer '${stopName}' not found` };
        }
        const elapsed = performance.now() - startTime;
        return { success: true, output: `${stopName}: ${elapsed.toFixed(2)}ms` };

      case 'reset':
        const resetName = args.slice(1).join(' ') || 'default';
        this.timers.delete(resetName);
        return { success: true, output: `Timer '${resetName}' reset` };

      case 'print':
        let output = 'Active timers:\n';
        this.timers.forEach((time, name) => {
          const elapsed = performance.now() - time;
          output += `  ${name}: ${elapsed.toFixed(2)}ms\n`;
        });
        return { success: true, output };

      default:
        return { success: false, output: '', error: `Unknown action: ${action}` };
    }
  }

  private executeExec(args: string[]): CommandResult {
    const code = args.join(' ');
    try {
      const result = new Function(`return (${code})`)();
      return { success: true, output: JSON.stringify(result, null, 2), data: result };
    } catch (error) {
      return { success: false, output: '', error: String(error) };
    }
  }

  private executeSpawn(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: spawn <type> [count] [x] [y]' };
    }

    const type = args[0];
    const count = parseInt(args[1]) || 1;
    const x = parseFloat(args[2]) || 0;
    const y = parseFloat(args[3]) || 0;

    this.emit('spawn', { type, count, x, y });
    return { success: true, output: `Spawning ${count} ${type}(s) at (${x}, ${y})` };
  }

  private executeKill(args: string[]): CommandResult {
    const target = args[0] || 'all';
    this.emit('kill', { target });
    return { success: true, output: `Killing ${target}` };
  }

  private executeGive(args: string[]): CommandResult {
    if (args.length < 2) {
      return { success: false, output: '', error: 'Usage: give <resource|item> <amount>' };
    }

    const resource = args[0];
    const amount = parseInt(args[1]) || 1;

    this.emit('give', { resource, amount });
    return { success: true, output: `Gave ${amount} ${resource}(s)` };
  }

  private executeTeleport(args: string[]): CommandResult {
    if (args.length < 2) {
      return { success: false, output: '', error: 'Usage: tp [target] <x> <y>' };
    }

    let target: string, x: number, y: number;

    if (args.length >= 3) {
      target = args[0];
      x = parseFloat(args[1]);
      y = parseFloat(args[2]);
    } else {
      target = 'player';
      x = parseFloat(args[0]);
      y = parseFloat(args[1]);
    }

    if (isNaN(x) || isNaN(y)) {
      return { success: false, output: '', error: 'Invalid coordinates' };
    }

    this.emit('teleport', { target, x, y });
    return { success: true, output: `Teleporting ${target} to (${x}, ${y})` };
  }

  private executeSpeed(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: speed <multiplier>' };
    }

    const speed = parseFloat(args[0]);
    if (isNaN(speed) || speed < 0) {
      return { success: false, output: '', error: 'Invalid speed value' };
    }

    this.emit('speed_change', { speed });
    return { success: true, output: `Game speed set to ${speed}x` };
  }

  private executePause(args: string[]): CommandResult {
    const toggle = args[0]?.toLowerCase();
    let paused: boolean;

    if (toggle === 'toggle' || !toggle) {
      this.emit('pause_toggle');
      return { success: true, output: 'Toggled pause' };
    } else if (toggle === 'on' || toggle === 'true') {
      paused = true;
    } else if (toggle === 'off' || toggle === 'false') {
      paused = false;
    } else {
      return { success: false, output: '', error: 'Usage: pause [on|off|toggle]' };
    }

    this.emit('pause', { paused });
    return { success: true, output: `Game ${paused ? 'paused' : 'resumed'}` };
  }

  private executeList(args: string[]): CommandResult {
    const type = args[0]?.toLowerCase() || 'commands';

    switch (type) {
      case 'entities':
        this.emit('list_entities');
        return { success: true, output: 'Listing entities...' };

      case 'commands':
        const commands = this.getCommands();
        let output = 'Available commands:\n';
        commands.forEach(cmd => {
          output += `  ${cmd.name} - ${cmd.description}\n`;
        });
        return { success: true, output };

      case 'settings':
        let settingsOutput = 'Configuration settings:\n';
        Object.entries(this.config).forEach(([key, value]) => {
          settingsOutput += `  ${key}: ${value}\n`;
        });
        return { success: true, output: settingsOutput };

      default:
        return { success: false, output: '', error: `Unknown list type: ${type}` };
    }
  }

  private executeInspect(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: inspect <entityId>' };
    }

    const entityId = args[0];
    this.emit('inspect', { entityId });
    return { success: true, output: `Inspecting ${entityId}...` };
  }

  private executeStats(args: string[]): CommandResult {
    if (args[0]?.toLowerCase() === 'reset') {
      this.emit('stats_reset');
      return { success: true, output: 'Statistics reset' };
    }

    this.emit('stats_request');
    return { success: true, output: 'Requesting statistics...' };
  }

  private executeProfile(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: profile <start|stop|snapshot>' };
    }

    const action = args[0].toLowerCase();
    this.emit('profile', { action });
    return { success: true, output: `Profile ${action}` };
  }

  private executeMemory(args: string[]): CommandResult {
    const action = args[0]?.toLowerCase() || 'info';

    switch (action) {
      case 'gc':
        this.emit('memory_gc');
        return { success: true, output: 'Garbage collection triggered' };

      case 'snapshot':
        this.emit('memory_snapshot');
        return { success: true, output: 'Memory snapshot taken' };

      case 'leak':
        this.emit('memory_leak_check');
        return { success: true, output: 'Checking for memory leaks...' };

      case 'info':
      default:
        if (performance.memory) {
          const mem = performance.memory as PerformanceMemory;
          return {
            success: true,
            output: `Memory: ${Math.round(mem.usedJSHeapSize / 1048576)}MB / ${Math.round(mem.totalJSHeapSize / 1048576)}MB`,
            data: {
              used: mem.usedJSHeapSize,
              total: mem.totalJSHeapSize,
              limit: mem.jsHeapSizeLimit,
            },
          };
        }
        return { success: false, output: '', error: 'Memory API not available' };
    }
  }

  private executeNetwork(args: string[]): CommandResult {
    if (args.length < 1) {
      this.emit('net_stats');
      return { success: true, output: 'Requesting network stats...' };
    }

    const action = args[0].toLowerCase();
    this.emit('net', { action, args: args.slice(1) });
    return { success: true, output: `Network ${action}` };
  }

  private executeSave(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: save <game|config|slot>' };
    }

    const type = args[0].toLowerCase();
    this.emit('save', { type });
    return { success: true, output: `Saving ${type}...` };
  }

  private executeLoad(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: load <game|config|slot>' };
    }

    const type = args[0].toLowerCase();
    this.emit('load', { type });
    return { success: true, output: `Loading ${type}...` };
  }

  private executeAI(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: ai <enable|disable|dump|pathfinding>' };
    }

    const action = args[0].toLowerCase();
    this.emit('ai', { action, args: args.slice(1) });
    return { success: true, output: `AI ${action}` };
  }

  private executeRender(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: render <option> [value]' };
    }

    const option = args[0].toLowerCase();
    const value = args[1];

    this.emit('render', { option, value });
    return { success: true, output: `Render ${option} ${value || ''}` };
  }

  private executePhysics(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: physics <option> [value]' };
    }

    const option = args[0].toLowerCase();
    const value = args[1];

    this.emit('physics', { option, value });
    return { success: true, output: `Physics ${option} ${value || ''}` };
  }

  private executeAudio(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: audio <play|stop|volume>' };
    }

    const action = args[0].toLowerCase();
    const params = args.slice(1);

    this.emit('audio', { action, params });
    return { success: true, output: `Audio ${action}` };
  }

  private executeReload(args: string[]): CommandResult {
    if (args.length < 1) {
      return { success: false, output: '', error: 'Usage: reload <textures|shaders|audio|all>' };
    }

    const type = args[0].toLowerCase();
    this.emit('reload', { type });
    return { success: true, output: `Reloading ${type}...` };
  }

  public on(event: string, callback: Function): void {
    if (!this.eventListeners.has(event)) {
      this.eventListeners.set(event, new Set());
    }
    this.eventListeners.get(event)!.add(callback);
  }

  public off(event: string, callback: Function): void {
    const listeners = this.eventListeners.get(event);
    if (listeners) {
      listeners.delete(callback);
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.eventListeners.get(event);
    if (listeners) {
      listeners.forEach(callback => callback(data));
    }
  }

  public getConfig(): DebugConfig {
    return { ...this.config };
  }

  public setConfig(config: Partial<DebugConfig>): void {
    this.config = { ...this.config, ...config };
  }

  public getInputHistory(): string[] {
    return [...this.inputHistory];
  }

  public navigateHistory(direction: 'up' | 'down'): string {
    if (direction === 'up') {
      if (this.historyIndex < this.inputHistory.length - 1) {
        this.historyIndex++;
      }
    } else {
      if (this.historyIndex > 0) {
        this.historyIndex--;
      } else {
        this.historyIndex = -1;
        return '';
      }
    }

    return this.historyIndex >= 0 ? this.inputHistory[this.historyIndex] : '';
  }

  public getCategories(): string[] {
    return Array.from(this.registeredCategories);
  }

  public getKeyBindings(): Map<string, string> {
    return new Map(this.keyBindings);
  }

  public bindKey(key: string, action: string): void {
    this.keyBindings.set(key, action);
  }

  public unbindKey(key: string): boolean {
    return this.keyBindings.delete(key);
  }

  public destroy(): void {
    this.commandHandlers.clear();
    this.eventListeners.forEach(set => set.clear());
    this.eventListeners.clear();
    this.timers.clear();
  }
}

export interface PerformanceMemory {
  jsHeapSizeLimit: number;
  totalJSHeapSize: number;
  usedJSHeapSize: number;
}

export class PerformanceMonitor {
  private metrics: Map<string, PerformanceMetric> = new Map();
  private frameStartTime: number = 0;
  private frameTimes: number[] = [];
  private maxSamples: number = 60;
  private frameCallback?: (metrics: FrameMetrics) => void;
  private isPaused: boolean = false;

  constructor() {
    this.initializeMetrics();
  }

  private initializeMetrics(): void {
    this.addMetric('fps', 'FPS', MetricCategory.FRAME, '');
    this.addMetric('frameTime', 'Frame Time', MetricCategory.FRAME, 'ms');
    this.addMetric('deltaTime', 'Delta Time', MetricCategory.FRAME, 'ms');
    this.addMetric('updateTime', 'Update Time', MetricCategory.GAME, 'ms');
    this.addMetric('renderTime', 'Render Time', MetricCategory.RENDER, 'ms');
    this.addMetric('physicsTime', 'Physics Time', MetricCategory.PHYSICS, 'ms');
    this.addMetric('aiTime', 'AI Time', MetricCategory.AI, 'ms');
    this.addMetric('audioTime', 'Audio Time', MetricCategory.AUDIO, 'ms');
    this.addMetric('memoryUsage', 'Memory', MetricCategory.MEMORY, 'MB');
    this.addMetric('drawCalls', 'Draw Calls', MetricCategory.RENDER, '');
    this.addMetric('triangles', 'Triangles', MetricCategory.RENDER, '');
    this.addMetric('entities', 'Entities', MetricCategory.GAME, '');
    this.addMetric('activeEntities', 'Active Entities', MetricCategory.GAME, '');
  }

  private addMetric(name: string, displayName: string, category: MetricCategory, unit: string): void {
    this.metrics.set(name, {
      name: displayName,
      value: 0,
      min: Infinity,
      max: -Infinity,
      average: 0,
      samples: [],
      unit,
      category,
    });
  }

  public beginFrame(): void {
    if (this.isPaused) return;
    this.frameStartTime = performance.now();
  }

  public endFrame(): void {
    if (this.isPaused) return;

    const frameTime = performance.now() - this.frameStartTime;
    this.frameTimes.push(frameTime);

    if (this.frameTimes.length > this.maxSamples) {
      this.frameTimes.shift();
    }

    const fps = 1000 / frameTime;
    this.updateMetric('fps', fps);
    this.updateMetric('frameTime', frameTime);
    this.updateMetric('deltaTime', frameTime);

    const frameMetrics = this.getFrameMetrics();
    if (this.frameCallback) {
      this.frameCallback(frameMetrics);
    }
  }

  public updateMetric(name: string, value: number): void {
    const metric = this.metrics.get(name);
    if (!metric) return;

    metric.value = value;
    metric.samples.push(value);

    if (metric.samples.length > this.maxSamples) {
      metric.samples.shift();
    }

    if (value < metric.min) metric.min = value;
    if (value > metric.max) metric.max = value;

    const sum = metric.samples.reduce((a, b) => a + b, 0);
    metric.average = sum / metric.samples.length;
  }

  public getMetric(name: string): PerformanceMetric | undefined {
    return this.metrics.get(name);
  }

  public getMetrics(category?: MetricCategory): PerformanceMetric[] {
    const all = Array.from(this.metrics.values());
    if (category) {
      return all.filter(m => m.category === category);
    }
    return all;
  }

  public getFrameMetrics(): FrameMetrics {
    const avgFrameTime = this.frameTimes.reduce((a, b) => a + b, 0) / this.frameTimes.length;

    return {
      deltaTime: this.getMetric('deltaTime')?.value || 0,
      fps: this.getMetric('fps')?.value || 0,
      frameTime: avgFrameTime,
      fixedUpdateTime: this.getMetric('fixedUpdateTime')?.value || 0,
      variableUpdateTime: this.getMetric('updateTime')?.value || 0,
      renderTime: this.getMetric('renderTime')?.value || 0,
      gpuTime: this.getMetric('gpuTime')?.value || 0,
      totalTime: avgFrameTime,
      skipRate: 0,
    };
  }

  public getAverageFps(): number {
    return this.getMetric('fps')?.average || 0;
  }

  public getAverageFrameTime(): number {
    return this.getMetric('frameTime')?.average || 0;
  }

  public isPerformanceGood(): boolean {
    return this.getAverageFps() >= 55;
  }

  public isPerformanceWarning(): boolean {
    const fps = this.getAverageFps();
    return fps >= 30 && fps < 55;
  }

  public isPerformanceCritical(): boolean {
    return this.getAverageFps() < 30;
  }

  public onFrameMetrics(callback: (metrics: FrameMetrics) => void): void {
    this.frameCallback = callback;
  }

  public pause(): void {
    this.isPaused = true;
  }

  public resume(): void {
    this.isPaused = false;
  }

  public reset(): void {
    this.metrics.forEach(metric => {
      metric.samples = [];
      metric.value = 0;
      metric.min = Infinity;
      metric.max = -Infinity;
      metric.average = 0;
    });
    this.frameTimes = [];
  }

  public resetMetric(name: string): void {
    const metric = this.metrics.get(name);
    if (metric) {
      metric.samples = [];
      metric.value = 0;
      metric.min = Infinity;
      metric.max = -Infinity;
      metric.average = 0;
    }
  }

  public takeSnapshot(): PerformanceSnapshot {
    return {
      id: `snapshot_${Date.now()}`,
      timestamp: Date.now(),
      frameTime: this.getMetric('frameTime')?.value || 0,
      fps: this.getMetric('fps')?.value || 0,
      metrics: this.getMetrics().reduce((acc, m) => {
        acc[m.name] = m.value;
        return acc;
      }, {} as Record<string, number>),
    };
  }

  public getSummary(): string {
    const fps = this.getMetric('fps');
    const frameTime = this.getMetric('frameTime');
    const memory = this.getMetric('memoryUsage');
    const entities = this.getMetric('entities');

    let summary = 'Performance Summary:\n';
    if (fps) summary += `  FPS: ${fps.value.toFixed(1)} (avg: ${fps.average.toFixed(1)})\n`;
    if (frameTime) summary += `  Frame Time: ${frameTime.value.toFixed(2)}ms (avg: ${frameTime.average.toFixed(2)}ms)\n`;
    if (memory) summary += `  Memory: ${memory.value.toFixed(2)}${memory.unit}\n`;
    if (entities) summary += `  Entities: ${entities.value.toFixed(0)}\n`;

    return summary;
  }

  public destroy(): void {
    this.metrics.clear();
    this.frameTimes = [];
    this.frameCallback = undefined;
  }
}

export const createDebugConsole = (config?: Partial<DebugConfig>): DebugConsole => {
  return new DebugConsole(config);
};

export const createPerformanceMonitor = (): PerformanceMonitor => {
  return new PerformanceMonitor();
};

export default DebugConsole;

export interface NetworkConfig {
  serverUrl: string;
  port: number;
  protocol: 'tcp' | 'udp' | 'websocket';
  maxConnections: number;
  timeout: number;
  heartbeatInterval: number;
  reconnectAttempts: number;
  reconnectDelay: number;
  packetSize: number;
  enableCompression: boolean;
  enableEncryption: boolean;
  enableReliability: boolean;
  enableOrdering: boolean;
}

export interface ConnectionState {
  connected: boolean;
  connecting: boolean;
  disconnecting: boolean;
  reconnecting: boolean;
  error: string | null;
  lastPing: number;
  averagePing: number;
  packetsSent: number;
  packetsReceived: number;
  bytesSent: number;
  bytesReceived: number;
  latency: number;
  jitter: number;
  packetLoss: number;
}

export interface NetworkPeer {
  id: string;
  name: string;
  address: string;
  port: number;
  isHost: boolean;
  isLocal: boolean;
  isReady: boolean;
  ping: number;
  score: number;
  faction: string;
  color: string;
  avatar: string;
  isConnected: boolean;
  lastActivity: number;
  metadata: Record<string, unknown>;
}

export interface Packet {
  id: string;
  type: PacketType;
  senderId: string;
  timestamp: number;
  sequence: number;
  channel: PacketChannel;
  reliability: PacketReliability;
  data: Uint8Array;
  size: number;
  compressed: boolean;
  encrypted: boolean;
}

export enum PacketType {
  HANDSHAKE = 'handshake',
  HANDSHAKE_ACCEPT = 'handshake_accept',
  HANDSHAKE_REJECT = 'handshake_reject',
  PING = 'ping',
  PONG = 'pong',
  DISCONNECT = 'disconnect',
  HEARTBEAT = 'heartbeat',
  SYNC = 'sync',
  INPUT = 'input',
  COMMAND = 'command',
  GAME_STATE = 'game_state',
  ENTITY_UPDATE = 'entity_update',
  ENTITY_CREATE = 'entity_create',
  ENTITY_DESTROY = 'entity_destroy',
  CHAT = 'chat',
  WHISPER = 'whisper',
  BROADCAST = 'broadcast',
  LOBBY = 'lobby',
  LOBBY_UPDATE = 'lobby_update',
  LOBBY_JOIN = 'lobby_join',
  LOBBY_LEAVE = 'lobby_leave',
  LOBBY_KICK = 'lobby_kick',
  LOBBY_READY = 'lobby_ready',
  GAME_START = 'game_start',
  GAME_PAUSE = 'game_pause',
  GAME_RESUME = 'game_resume',
  GAME_END = 'game_end',
  VOTE = 'vote',
  VOTE_START = 'vote_start',
  VOTE_RESULT = 'vote_result',
  MATCHMAKING = 'matchmaking',
  MATCHMAKING_JOIN = 'matchmaking_join',
  MATCHMAKING_LEAVE = 'matchmaking_leave',
  MATCHMAKING_UPDATE = 'matchmaking_update',
  CUSTOM = 'custom',
}

export enum PacketChannel {
  UNRELIABLE = 0,
  UNRELIABLE_ORDERED = 1,
  RELIABLE = 2,
  RELIABLE_ORDERED = 3,
  RELIABLE_SEQUENCED = 4,
  STREAM = 5,
  FRAGMENT = 6,
}

export enum PacketReliability {
  UNRELIABLE = 0,
  UNRELIABLE_SEQUENCED = 1,
  RELIABLE = 2,
  RELIABLE_ORDERED = 3,
  RELIABLE_SEQUENCED = 4,
}

export interface NetworkEvent {
  type: NetworkEventType;
  peerId?: string;
  data?: unknown;
  timestamp: number;
}

export enum NetworkEventType {
  CONNECTED = 'connected',
  DISCONNECTED = 'disconnected',
  PEER_CONNECTED = 'peer_connected',
  PEER_DISCONNECTED = 'peer_disconnected',
  PACKET_SENT = 'packet_sent',
  PACKET_RECEIVED = 'packet_received',
  ERROR = 'error',
  TIMEOUT = 'timeout',
  RECONNECTING = 'reconnecting',
  STATE_CHANGED = 'state_changed',
  LAG = 'lag',
  SYNC = 'sync',
}

export interface SyncState {
  frameNumber: number;
  checksum: number;
  authoritative: boolean;
  lastSync: number;
  pendingSyncs: number;
}

export interface InputState {
  keys: Set<string>;
  mouseX: number;
  mouseY: number;
  mouseButtons: Set<number>;
  scrollDelta: number;
  timestamp: number;
}

export interface GameCommand {
  type: CommandType;
  playerId: string;
  frameNumber: number;
  timestamp: number;
  data: CommandData;
  validated: boolean;
  executed: boolean;
}

export enum CommandType {
  UNIT_MOVE = 'unit_move',
  UNIT_ATTACK = 'unit_attack',
  UNIT_STOP = 'unit_stop',
  UNIT_PATROL = 'unit_patrol',
  UNIT_HOLD = 'unit_hold',
  UNIT_GUARD = 'unit_guard',
  UNIT_REPAIR = 'unit_repair',
  UNIT_SCOUT = 'unit_scout',
  UNIT_BUILD = 'unit_build',
  UNIT_GATHER = 'unit_gather',
  UNIT_RETURN = 'unit_return',
  UNIT_UNLOAD = 'unit_unload',
  UNIT_ENTER = 'unit_enter',
  UNIT_EXIT = 'unit_exit',
  UNIT_SET_RALLY = 'unit_set_rally',
  UNIT_CANCEL = 'unit_cancel',
  BUILDING_PLACE = 'building_place',
  BUILDING_CANCEL = 'building_cancel',
  BUILDING_UPGRADE = 'building_upgrade',
  BUILDING_REPAIR = 'building_repair',
  RESEARCH_START = 'research_start',
  RESEARCH_CANCEL = 'research_cancel',
  PRODUCTION_START = 'production_start',
  PRODUCTION_CANCEL = 'production_cancel',
  PRODUCTION_PAUSE = 'production_pause',
  UNIT_SELECT = 'unit_select',
  UNIT_DESELECT = 'unit_deselect',
  GROUP_SELECT = 'group_select',
  GROUP_ASSIGN = 'group_assign',
  RESOURCE_SPEND = 'resource_spend',
  ALLIANCE_OFFER = 'alliance_offer',
  ALLIANCE_ACCEPT = 'alliance_accept',
  ALLIANCE_BREAK = 'alliance_break',
  CHEAT = 'cheat',
}

export type CommandData = Record<string, unknown>;

export interface LobbyState {
  id: string;
  name: string;
  hostId: string;
  mapId: string;
  mapName: string;
  maxPlayers: number;
  currentPlayers: number;
  players: LobbyPlayer[];
  settings: LobbySettings;
  chat: ChatMessage[];
  ready: Set<string>;
  kicked: Set<string>;
  gameStarted: boolean;
  gameSettings: GameSettings;
}

export interface LobbyPlayer {
  id: string;
  name: string;
  faction: string;
  color: number;
  team: number;
  slot: number;
  isReady: boolean;
  isHost: boolean;
  isSpectator: boolean;
  ping: number;
  isConnected: boolean;
}

export interface LobbySettings {
  gameMode: GameMode;
  mapSize: MapSize;
  startingResources: number;
  gameSpeed: GameSpeed;
  victoryCondition: VictoryCondition;
  fogOfWar: boolean;
  cheatsEnabled: boolean;
  lockTeams: boolean;
  allowSpectators: boolean;
  password: string;
  autoBalance: boolean;
}

export enum GameMode {
  SKIRMISH = 'skirmish',
  MULTIPLAYER = 'multiplayer',
  COOPERATIVE = 'cooperative',
  CAPTAIN = 'captain',
  DUEL = 'duel',
  FREE_FOR_ALL = 'free_for_all',
  TEAM_GAME = 'team_game',
}

export enum MapSize {
  TINY = 'tiny',
  SMALL = 'small',
  MEDIUM = 'medium',
  LARGE = 'large',
  HUGE = 'huge',
  GIANT = 'giant',
}

export enum GameSpeed {
  SLOW = 0.5,
  NORMAL = 1.0,
  FAST = 1.5,
  FASTER = 2.0,
  FASTEST = 3.0,
}

export enum VictoryCondition {
  ANNIHILATION = 'annihilation',
  DOMINATION = 'domination',
  CAPTURE_THE_FLAG = 'ctf',
  KING_OF_THE_HILL = 'koth',
  TIME_LIMIT = 'time_limit',
  SUDDEN_DEATH = 'sudden_death',
  ESCORT = 'escort',
}

export interface GameSettings extends LobbySettings {
  seed: number;
  timeLimit: number;
  scoreLimit: number;
  unitCap: number;
  defenseStructures: boolean;
  resourceRichness: ResourceRichness;
  airUnits: boolean;
  ordnance: boolean;
}

export enum ResourceRichness {
  SCARCE = 0.5,
  NORMAL = 1.0,
  ABUNDANT = 1.5,
  EXTREME = 2.0,
}

export interface ChatMessage {
  id: string;
  senderId: string;
  senderName: string;
  content: string;
  timestamp: number;
  channel: ChatChannel;
  recipientId?: string;
}

export enum ChatChannel {
  ALL = 'all',
  TEAM = 'team',
  SQUAD = 'squad',
  WHISPER = 'whisper',
  SYSTEM = 'system',
  spectators = 'spectators',
}

export interface VoteState {
  active: boolean;
  voteId: string;
  voteType: VoteType;
  initiatorId: string;
  targetId?: string;
  votesYes: Set<string>;
  votesNo: Set<string>;
  voters: string[];
  requiredYes: number;
  timeRemaining: number;
}

export enum VoteType {
  KICK = 'kick',
  SURRENDER = 'surrender',
  REMATCH = 'rematch',
  PAUSE = 'pause',
  UNPAUSE = 'unpause',
  CHANGE_SETTINGS = 'change_settings',
}

export interface MatchmakingState {
  status: MatchmakingStatus;
  queueId: string;
  region: string;
  rank: number;
  rating: number;
  wins: number;
  losses: number;
  queueTime: number;
  estimatedWait: number;
  opponents: MatchmakingCandidate[];
}

export enum MatchmakingStatus {
  IDLE = 'idle',
  SEARCHING = 'searching',
  FOUND = 'found',
  ACCEPTING = 'accepting',
  COUNTDOWN = 'countdown',
  CANCELLED = 'cancelled',
  ERROR = 'error',
}

export interface MatchmakingCandidate {
  id: string;
  name: string;
  rank: number;
  rating: number;
  region: string;
  latency: number;
  faction?: string;
}

export interface ReplayData {
  id: string;
  version: string;
  mapId: string;
  mapName: string;
  gameMode: GameMode;
  players: ReplayPlayer[];
  duration: number;
  startTime: number;
  endTime: number;
  winner: string;
  events: ReplayEvent[];
  chat: ChatMessage[];
  settings: GameSettings;
}

export interface ReplayPlayer {
  id: string;
  name: string;
  faction: string;
  team: number;
  color: number;
  isHuman: boolean;
  isWinner: boolean;
  finalScore: number;
}

export interface ReplayEvent {
  frame: number;
  type: ReplayEventType;
  playerId: string;
  data: unknown;
}

export enum ReplayEventType {
  COMMAND = 'command',
  SYNC = 'sync',
  CHAT = 'chat',
  VOTE = 'vote',
  GAME_STATE = 'game_state',
}

export interface LeaderboardEntry {
  rank: number;
  playerId: string;
  playerName: string;
  rating: number;
  wins: number;
  losses: number;
  winRate: number;
  gamesPlayed: number;
  region: string;
  lastPlayed: number;
  clan?: string;
  title?: string;
}

export interface PlayerProfile {
  id: string;
  username: string;
  displayName: string;
  avatar: string;
  level: number;
  xp: number;
  rank: number;
  rating: number;
  wins: number;
  losses: number;
  draws: number;
  clan?: string;
  clanRank?: string;
  titles: string[];
  achievements: string[];
  stats: PlayerStats;
  history: GameHistory[];
  friends: string[];
  blocked: string[];
  settings: PlayerSettings;
  createdAt: number;
  lastOnline: number;
}

export interface PlayerStats {
  unitsProduced: number;
  unitsLost: number;
  unitsKilled: number;
  buildingsBuilt: number;
  buildingsLost: number;
  buildingsDestroyed: number;
  resourcesGathered: number;
  resourcesSpent: number;
  researchCompleted: number;
  damageDealt: number;
  damageReceived: number;
  healingDone: number;
  timePlayed: number;
  gamesCompleted: number;
  gamesWon: number;
  gamesLost: number;
  longestWinStreak: number;
  longestLoseStreak: number;
  currentWinStreak: number;
  currentLoseStreak: number;
  totalPlayTime: number;
  favoriteFaction: string;
  mostPlayedMap: string;
  averageGameDuration: number;
  peakRating: number;
  currentRating: number;
}

export interface GameHistory {
  id: string;
  mapName: string;
  gameMode: GameMode;
  result: GameResult;
  ratingChange: number;
  duration: number;
  date: number;
  players: string[];
  faction: string;
}

export enum GameResult {
  WIN = 'win',
  LOSS = 'loss',
  DRAW = 'draw',
  SURRENDER = 'surrender',
  DISCONNECT = 'disconnect',
}

export interface PlayerSettings {
  audio: AudioSettings;
  video: VideoSettings;
  controls: ControlSettings;
  gameplay: GameplaySettings;
  social: SocialSettings;
}

export interface AudioSettings {
  masterVolume: number;
  musicVolume: number;
  sfxVolume: number;
  voiceVolume: number;
  ambientVolume: number;
  muted: boolean;
  pushToTalk: boolean;
  audioDevice: string;
  spatialAudio: boolean;
  audioQuality: AudioQuality;
}

export enum AudioQuality {
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high',
}

export interface VideoSettings {
  resolution: Resolution;
  fullscreen: boolean;
  vsync: boolean;
  antiAliasing: AntiAliasing;
  textureQuality: TextureQuality;
  shadowQuality: ShadowQuality;
  particleQuality: ParticleQuality;
  postProcessing: boolean;
  colorBlind: ColorBlindMode;
  fpsLimit: number;
  uiScale: number;
}

export interface Resolution {
  width: number;
  height: number;
}

export enum AntiAliasing {
  OFF = 0,
  FXAA = 1,
  MSAA2 = 2,
  MSAA4 = 4,
  MSAA8 = 8,
}

export enum TextureQuality {
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high',
  ULTRA = 'ultra',
}

export enum ShadowQuality {
  OFF = 'off',
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high',
  ULTRA = 'ultra',
}

export enum ParticleQuality {
  LOW = 0.25,
  MEDIUM = 0.5,
  HIGH = 0.75,
  ULTRA = 1.0,
}

export enum ColorBlindMode {
  NONE = 'none',
  PROTANOPIA = 'protanopia',
  DEUTERANOPIA = 'deuteranopia',
  TRITANOPIA = 'tritanopia',
}

export interface ControlSettings {
  mouseSensitivity: number;
  scrollSpeed: number;
  keybindings: Keybindings;
  gamepadEnabled: boolean;
  gamepadSensitivity: number;
  edgeScrolling: boolean;
  middleMousePan: boolean;
  doubleClickSelect: boolean;
  clickDragSelect: boolean;
  rightClickMove: boolean;
  rightClickAttack: boolean;
}

export interface Keybindings {
  move: string;
  attack: string;
  stop: string;
  holdPosition: string;
  patrol: string;
  ability1: string;
  ability2: string;
  ability3: string;
  ability4: string;
  ability5: string;
  building1: string;
  building2: string;
  building3: string;
  building4: string;
  building5: string;
  building6: string;
  building7: string;
  building8: string;
  building9: string;
  group1: string;
  group2: string;
  group3: string;
  group4: string;
  group5: string;
  group6: string;
  group7: string;
  group8: string;
  group9: string;
  selectAll: string;
  deselectAll: string;
  lastUnit: string;
  nextUnit: string;
  previousUnit: string;
  minimap: string;
  menu: string;
  chat: string;
  teamChat: string;
  pause: string;
  screenshot: string;
  fps: string;
}

export interface GameplaySettings {
  showHealthBars: boolean;
  healthBarScale: number;
  showDamage: boolean;
  showUnitNames: boolean;
  showFps: boolean;
  showPing: boolean;
  showTimer: boolean;
  autoSave: boolean;
  autoPause: boolean;
  tutorialHints: boolean;
  showTips: boolean;
  confirmActions: boolean;
  queueOrders: boolean;
  autoBuild: boolean;
  autoRepair: boolean;
  autoUpgrade: boolean;
}

export interface SocialSettings {
  allowWhispers: boolean;
  allowFriendRequests: boolean;
  showOnlineStatus: boolean;
  showInGameStatus: boolean;
  blockedPlayers: string[];
  mutedPlayers: string[];
  clanMessages: boolean;
  publicChat: boolean;
}

export interface Achievement {
  id: string;
  name: string;
  description: string;
  icon: string;
  category: AchievementCategory;
  difficulty: AchievementDifficulty;
  points: number;
  unlocked: boolean;
  unlockTime?: number;
  progress?: number;
  target?: number;
  hidden: boolean;
  rare: boolean;
}

export enum AchievementCategory {
  COMBAT = 'combat',
  ECONOMY = 'economy',
  EXPLORATION = 'exploration',
  BUILDING = 'building',
  RESEARCH = 'research',
  SOCIAL = 'social',
  SPECIAL = 'special',
  SEASONAL = 'seasonal',
}

export enum AchievementDifficulty {
  EASY = 1,
  MEDIUM = 2,
  HARD = 3,
  EXTREME = 4,
  IMPOSSIBLE = 5,
}

export interface TutorialStep {
  id: string;
  title: string;
  description: string;
  targetX?: number;
  targetY?: number;
  targetType?: string;
  pointer?: PointerDirection;
  action?: TutorialAction;
  condition?: () => boolean;
  dismissable: boolean;
  autoAdvance: boolean;
  delay: number;
}

export enum PointerDirection {
  UP = 'up',
  DOWN = 'down',
  LEFT = 'left',
  RIGHT = 'right',
}

export enum TutorialAction {
  CLICK = 'click',
  RIGHT_CLICK = 'right_click',
  DRAG = 'drag',
  SELECT = 'select',
  BUILD = 'build',
  RESEARCH = 'research',
  PRODUCE = 'produce',
  ATTACK = 'attack',
  MOVE = 'move',
}

export interface CloudSave {
  id: string;
  name: string;
  slot: number;
  thumbnail: string;
  timestamp: number;
  size: number;
  checksum: string;
  metadata: SaveMetadata;
}

export interface SaveMetadata {
  mapName: string;
  gameMode: GameMode;
  duration: number;
  players: string[];
  factions: string[];
  result?: GameResult;
}

export interface TournamentInfo {
  id: string;
  name: string;
  description: string;
  prizePool: string;
  startDate: number;
  endDate: number;
  registrationDeadline: number;
  maxPlayers: number;
  currentPlayers: number;
  format: TournamentFormat;
  rules: string;
  prizeDistribution: PrizeDistribution[];
  status: TournamentStatus;
  streams: string[];
}

export enum TournamentFormat {
  SINGLE_ELIMINATION = 'single_elimination',
  DOUBLE_ELIMINATION = 'double_elimination',
  ROUND_ROBIN = 'round_robin',
  SWISS = 'swiss',
  LEAGUE = 'league',
}

export enum TournamentStatus {
  UPCOMING = 'upcoming',
  REGISTRATION = 'registration',
  IN_PROGRESS = 'in_progress',
  COMPLETED = 'completed',
  CANCELLED = 'cancelled',
}

export interface PrizeDistribution {
  place: number;
  prize: string;
  percentage: number;
}

export interface ClanInfo {
  id: string;
  name: string;
  tag: string;
  emblem: string;
  leaderId: string;
  officers: string[];
  members: string[];
  stats: ClanStats;
  createdAt: number;
  description: string;
  website?: string;
  recruitment: boolean;
  minimumRating: number;
  requiredLevel: number;
}

export interface ClanStats {
  totalWins: number;
  totalLosses: number;
  tournamentWins: number;
  totalMembers: number;
  averageRating: number;
  weeklyXP: number;
  rank: number;
}

export interface ModInfo {
  id: string;
  name: string;
  author: string;
  version: string;
  description: string;
  category: ModCategory;
  tags: string[];
  rating: number;
  downloads: number;
  subscribers: number;
  previewImages: string[];
  dependencies: string[];
  compatible: boolean;
  enabled: boolean;
  size: number;
  createdAt: number;
  updatedAt: number;
}

export enum ModCategory {
  MAPS = 'maps',
  MODS = 'mods',
  UI = 'ui',
  AUDIO = 'audio',
  TEXTURES = 'textures',
  SCRIPTS = 'scripts',
  OTHER = 'other',
}

export interface PatchNotes {
  version: string;
  releaseDate: number;
  title: string;
  summary: string;
  features: string[];
  fixes: string[];
  balance: string[];
  knownIssues: string[];
  upcoming: string[];
}

export class NetworkManager {
  private config: NetworkConfig;
  private state: ConnectionState;
  private peers: Map<string, NetworkPeer>;
  private packetQueue: Packet[];
  private eventListeners: Map<NetworkEventType, Set<(event: NetworkEvent) => void>>;
  private socket: WebSocket | null = null;
  private reconnectTimer: number = 0;
  private heartbeatTimer: number = 0;
  private sequenceNumber: number = 0;
  private lastReceivedSequence: Map<string, number> = new Map();
  private pendingPackets: Map<string, Packet> = new Map();
  private compressionEnabled: boolean = true;
  private encryptionKey: CryptoKey | null = null;
  private messageHandlers: Map<PacketType, (packet: Packet) => void> = new Map();
  private syncState: SyncState;
  private inputBuffer: InputState[] = [];
  private commandBuffer: GameCommand[] = [];
  private lobbyState: LobbyState | null = null;
  private matchmakingState: MatchmakingState;
  private replayData: ReplayData | null = null;

  constructor(config: Partial<NetworkConfig> = {}) {
    this.config = {
      serverUrl: config.serverUrl || 'ws://localhost',
      port: config.port || 8080,
      protocol: config.protocol || 'websocket',
      maxConnections: config.maxConnections || 8,
      timeout: config.timeout || 30000,
      heartbeatInterval: config.heartbeatInterval || 5000,
      reconnectAttempts: config.reconnectAttempts || 5,
      reconnectDelay: config.reconnectDelay || 1000,
      packetSize: config.packetSize || 1024,
      enableCompression: config.enableCompression ?? true,
      enableEncryption: config.enableEncryption ?? false,
      enableReliability: config.enableReliability ?? true,
      enableOrdering: config.enableOrdering ?? true,
    };
    this.state = this.createInitialState();
    this.peers = new Map();
    this.packetQueue = [];
    this.eventListeners = new Map();
    this.syncState = this.createInitialSyncState();
    this.matchmakingState = this.createInitialMatchmakingState();
    this.initializeMessageHandlers();
  }

  private createInitialState(): ConnectionState {
    return {
      connected: false,
      connecting: false,
      disconnecting: false,
      reconnecting: false,
      error: null,
      lastPing: 0,
      averagePing: 0,
      packetsSent: 0,
      packetsReceived: 0,
      bytesSent: 0,
      bytesReceived: 0,
      latency: 0,
      jitter: 0,
      packetLoss: 0,
    };
  }

  private createInitialSyncState(): SyncState {
    return {
      frameNumber: 0,
      checksum: 0,
      authoritative: false,
      lastSync: 0,
      pendingSyncs: 0,
    };
  }

  private createInitialMatchmakingState(): MatchmakingState {
    return {
      status: MatchmakingStatus.IDLE,
      queueId: '',
      region: 'us-west',
      rank: 0,
      rating: 1000,
      wins: 0,
      losses: 0,
      queueTime: 0,
      estimatedWait: 0,
      opponents: [],
    };
  }

  private initializeMessageHandlers(): void {
    this.messageHandlers.set(PacketType.HANDSHAKE, this.handleHandshake.bind(this));
    this.messageHandlers.set(PacketType.HANDSHAKE_ACCEPT, this.handleHandshakeAccept.bind(this));
    this.messageHandlers.set(PacketType.HANDSHAKE_REJECT, this.handleHandshakeReject.bind(this));
    this.messageHandlers.set(PacketType.PING, this.handlePing.bind(this));
    this.messageHandlers.set(PacketType.PONG, this.handlePong.bind(this));
    this.messageHandlers.set(PacketType.DISCONNECT, this.handleDisconnect.bind(this));
    this.messageHandlers.set(PacketType.HEARTBEAT, this.handleHeartbeat.bind(this));
    this.messageHandlers.set(PacketType.SYNC, this.handleSync.bind(this));
    this.messageHandlers.set(PacketType.INPUT, this.handleInput.bind(this));
    this.messageHandlers.set(PacketType.COMMAND, this.handleCommand.bind(this));
    this.messageHandlers.set(PacketType.GAME_STATE, this.handleGameState.bind(this));
    this.messageHandlers.set(PacketType.ENTITY_UPDATE, this.handleEntityUpdate.bind(this));
    this.messageHandlers.set(PacketType.ENTITY_CREATE, this.handleEntityCreate.bind(this));
    this.messageHandlers.set(PacketType.ENTITY_DESTROY, this.handleEntityDestroy.bind(this));
    this.messageHandlers.set(PacketType.CHAT, this.handleChat.bind(this));
    this.messageHandlers.set(PacketType.WHISPER, this.handleWhisper.bind(this));
    this.messageHandlers.set(PacketType.LOBBY, this.handleLobby.bind(this));
    this.messageHandlers.set(PacketType.LOBBY_UPDATE, this.handleLobbyUpdate.bind(this));
    this.messageHandlers.set(PacketType.LOBBY_JOIN, this.handleLobbyJoin.bind(this));
    this.messageHandlers.set(PacketType.LOBBY_LEAVE, this.handleLobbyLeave.bind(this));
    this.messageHandlers.set(PacketType.LOBBY_KICK, this.handleLobbyKick.bind(this));
    this.messageHandlers.set(PacketType.LOBBY_READY, this.handleLobbyReady.bind(this));
    this.messageHandlers.set(PacketType.GAME_START, this.handleGameStart.bind(this));
    this.messageHandlers.set(PacketType.GAME_PAUSE, this.handleGamePause.bind(this));
    this.messageHandlers.set(PacketType.GAME_RESUME, this.handleGameResume.bind(this));
    this.messageHandlers.set(PacketType.GAME_END, this.handleGameEnd.bind(this));
    this.messageHandlers.set(PacketType.VOTE, this.handleVote.bind(this));
    this.messageHandlers.set(PacketType.VOTE_START, this.handleVoteStart.bind(this));
    this.messageHandlers.set(PacketType.VOTE_RESULT, this.handleVoteResult.bind(this));
    this.messageHandlers.set(PacketType.MATCHMAKING, this.handleMatchmaking.bind(this));
    this.messageHandlers.set(PacketType.MATCHMAKING_JOIN, this.handleMatchmakingJoin.bind(this));
    this.messageHandlers.set(PacketType.MATCHMAKING_LEAVE, this.handleMatchmakingLeave.bind(this));
    this.messageHandlers.set(PacketType.MATCHMAKING_UPDATE, this.handleMatchmakingUpdate.bind(this));
  }

  public async connect(serverUrl?: string, port?: number): Promise<boolean> {
    const url = serverUrl || this.config.serverUrl;
    const p = port || this.config.port;
    const wsUrl = `${this.config.protocol === 'websocket' ? 'ws' : 'wss'}://${url}:${p}`;

    this.state.connecting = true;
    this.emitEvent(NetworkEventType.STATE_CHANGED, { state: this.state });

    return new Promise((resolve, reject) => {
      try {
        this.socket = new WebSocket(wsUrl);
        this.socket.binaryType = 'arraybuffer';

        this.socket.onopen = () => {
          this.state.connected = true;
          this.state.connecting = false;
          this.state.error = null;
          this.emitEvent(NetworkEventType.CONNECTED);
          this.startHeartbeat();
          this.sendHandshake();
          resolve(true);
        };

        this.socket.onclose = (event) => {
          this.state.connected = false;
          this.state.connecting = false;
          this.emitEvent(NetworkEventType.DISCONNECTED, { code: event.code, reason: event.reason });
          this.stopHeartbeat();
          this.handleDisconnect();
        };

        this.socket.onerror = (error) => {
          this.state.error = 'Connection error';
          this.state.connecting = false;
          this.emitEvent(NetworkEventType.ERROR, { error });
          reject(error);
        };

        this.socket.onmessage = (event) => {
          this.processMessage(event);
        };
      } catch (error) {
        this.state.error = String(error);
        this.state.connecting = false;
        reject(error);
      }
    });
  }

  public disconnect(): void {
    this.state.disconnecting = true;
    this.sendPacket(PacketType.DISCONNECT, PacketChannel.RELIABLE);
    this.cleanup();
  }

  private cleanup(): void {
    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }
    this.state = this.createInitialState();
    this.peers.clear();
    this.packetQueue = [];
    this.pendingPackets.clear();
    this.stopHeartbeat();
    this.emitEvent(NetworkEventType.DISCONNECTED);
  }

  private async sendHandshake(): Promise<void> {
    const handshakeData = {
      version: '1.0.0',
      protocol: 'tcp',
      clientType: 'game',
      timestamp: Date.now(),
      supportedFeatures: ['compression', 'encryption', 'reliability'],
    };
    await this.sendPacket(PacketType.HANDSHAKE, PacketChannel.RELIABLE, handshakeData);
  }

  private handleHandshake(packet: Packet): void {
    const data = this.decodePacket(packet);
    if (data && data.accepted) {
      this.sendPacket(PacketType.HANDSHAKE_ACCEPT, PacketChannel.RELIABLE);
    } else {
      this.sendPacket(PacketType.HANDSHAKE_REJECT, PacketChannel.RELIABLE, { reason: data?.reason });
    }
  }

  private handleHandshakeAccept(_packet: Packet): void {
    this.emitEvent(NetworkEventType.CONNECTED);
  }

  private handleHandshakeReject(packet: Packet): void {
    const data = this.decodePacket(packet);
    this.state.error = data?.reason || 'Handshake rejected';
    this.emitEvent(NetworkEventType.ERROR, { reason: this.state.error });
    this.disconnect();
  }

  private handlePing(packet: Packet): void {
    this.sendPacket(PacketType.PONG, PacketChannel.UNRELIABLE);
  }

  private handlePong(packet: Packet): void {
    const now = Date.now();
    const data = this.decodePacket(packet);
    if (data && data.timestamp) {
      const ping = now - data.timestamp;
      this.state.lastPing = ping;
      this.updateAveragePing(ping);
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { type: PacketType.PONG, ping });
    }
  }

  private updateAveragePing(ping: number): void {
    const prevAvg = this.state.averagePing;
    this.state.averagePing = (prevAvg * 0.9 + ping * 0.1);
    this.state.latency = this.state.averagePing;
    if (prevAvg > 0) {
      this.state.jitter = Math.abs(this.state.averagePing - prevAvg);
    }
  }

  private handleDisconnect(): void {
    this.peers.clear();
    this.state.connected = false;
    this.emitEvent(NetworkEventType.DISCONNECTED);
  }

  private startHeartbeat(): void {
    this.heartbeatTimer = window.setInterval(() => {
      if (this.state.connected) {
        this.sendPacket(PacketType.HEARTBEAT, PacketChannel.UNRELIABLE);
      }
    }, this.config.heartbeatInterval);
  }

  private stopHeartbeat(): void {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = 0;
    }
  }

  private processMessage(event: MessageEvent): void {
    try {
      const data = event.data;
      if (data instanceof ArrayBuffer) {
        const packet = this.decodeRawPacket(new Uint8Array(data));
        if (packet) {
          this.handlePacket(packet);
        }
      } else if (typeof data === 'string') {
        const packet = JSON.parse(data);
        this.handlePacket(packet);
      }
    } catch (error) {
      console.error('Error processing message:', error);
    }
  }

  private decodeRawPacket(data: Uint8Array): Packet | null {
    try {
      if (this.config.enableCompression) {
        const decompressed = this.decompress(data);
        if (decompressed) {
          data = decompressed;
        }
      }
      if (this.config.enableEncryption && this.encryptionKey) {
        data = this.decrypt(data);
      }
      const packetStr = new TextDecoder().decode(data);
      return JSON.parse(packetStr);
    } catch (error) {
      console.error('Error decoding packet:', error);
      return null;
    }
  }

  private decodePacket(packet: Packet): unknown {
    try {
      const str = new TextDecoder().decode(packet.data);
      return JSON.parse(str);
    } catch (error) {
      return null;
    }
  }

  private async sendPacket(
    type: PacketType,
    channel: PacketChannel,
    data?: unknown,
    reliability?: PacketReliability
  ): Promise<void> {
    if (!this.state.connected || !this.socket) {
      return;
    }

    const packet: Packet = {
      id: this.generatePacketId(),
      type,
      senderId: this.getLocalPeerId(),
      timestamp: Date.now(),
      sequence: this.sequenceNumber++,
      channel,
      reliability: reliability || this.getReliabilityFromChannel(channel),
      data: this.encodeData(data),
      size: 0,
      compressed: this.config.enableCompression,
      encrypted: this.config.enableEncryption,
    };

    packet.size = packet.data.length;
    this.state.packetsSent++;
    this.state.bytesSent += packet.size;

    const rawData = this.preparePacketForSend(packet);
    this.socket.send(rawData);

    this.emitEvent(NetworkEventType.PACKET_SENT, { type, size: packet.size });
  }

  private encodeData(data: unknown): Uint8Array {
    const str = JSON.stringify(data || {});
    return new TextEncoder().encode(str);
  }

  private preparePacketForSend(packet: Packet): ArrayBuffer {
    let data = new TextEncoder().encode(JSON.stringify(packet));
    if (this.config.enableEncryption && this.encryptionKey) {
      data = this.encrypt(data);
    }
    if (this.config.enableCompression && data.length > 100) {
      data = this.compress(data);
      packet.compressed = true;
    }
    return data.buffer;
  }

  private compress(data: Uint8Array): Uint8Array {
    return data;
  }

  private decompress(data: Uint8Array): Uint8Array {
    return data;
  }

  private async encrypt(data: Uint8Array): Promise<Uint8Array> {
    return data;
  }

  private decrypt(data: Uint8Array): Uint8Array {
    return data;
  }

  private getReliabilityFromChannel(channel: PacketChannel): PacketReliability {
    switch (channel) {
      case PacketChannel.UNRELIABLE:
        return PacketReliability.UNRELIABLE;
      case PacketChannel.UNRELIABLE_ORDERED:
        return PacketReliability.UNRELIABLE_SEQUENCED;
      case PacketChannel.RELIABLE:
        return PacketReliability.RELIABLE;
      case PacketChannel.RELIABLE_ORDERED:
        return PacketReliability.RELIABLE_ORDERED;
      case PacketChannel.RELIABLE_SEQUENCED:
        return PacketReliability.RELIABLE_SEQUENCED;
      default:
        return PacketReliability.RELIABLE_ORDERED;
    }
  }

  private handlePacket(packet: Packet): void {
    this.state.packetsReceived++;
    this.state.bytesReceived += packet.size;

    if (packet.channel === PacketChannel.RELIABLE_SEQUENCED ||
        packet.channel === PacketChannel.UNRELIABLE_SEQUENCED) {
      const lastSeq = this.lastReceivedSequence.get(packet.senderId) || 0;
      if (packet.sequence <= lastSeq) {
        return;
      }
      this.lastReceivedSequence.set(packet.senderId, packet.sequence);
    }

    const handler = this.messageHandlers.get(packet.type);
    if (handler) {
      handler(packet);
    }

    this.emitEvent(NetworkEventType.PACKET_RECEIVED, { type: packet.type, size: packet.size });
  }

  private handleHeartbeat(_packet: Packet): void {
  }

  private handleSync(packet: Packet): void {
    const data = this.decodePacket(packet) as SyncState;
    if (data) {
      this.syncState = { ...this.syncState, ...data };
      this.syncState.lastSync = Date.now();
      this.emitEvent(NetworkEventType.SYNC, { sync: this.syncState });
    }
  }

  private handleInput(packet: Packet): void {
    const data = this.decodePacket(packet) as InputState;
    if (data) {
      this.inputBuffer.push(data);
      if (this.inputBuffer.length > 60) {
        this.inputBuffer.shift();
      }
    }
  }

  private handleCommand(packet: Packet): void {
    const data = this.decodePacket(packet) as GameCommand;
    if (data) {
      this.commandBuffer.push(data);
      this.validateAndExecuteCommand(data);
    }
  }

  private validateAndExecuteCommand(command: GameCommand): void {
    command.validated = true;
    command.executed = true;
  }

  private handleGameState(packet: Packet): void {
    const data = this.decodePacket(packet);
    if (data) {
      this.emitEvent(NetworkEventType.SYNC, { gameState: data });
    }
  }

  private handleEntityUpdate(packet: Packet): void {
    const data = this.decodePacket(packet);
    if (data) {
      this.emitEvent(NetworkEventType.SYNC, { entityUpdate: data });
    }
  }

  private handleEntityCreate(packet: Packet): void {
    const data = this.decodePacket(packet);
    if (data) {
      this.emitEvent(NetworkEventType.SYNC, { entityCreate: data });
    }
  }

  private handleEntityDestroy(packet: Packet): void {
    const data = this.decodePacket(packet);
    if (data) {
      this.emitEvent(NetworkEventType.SYNC, { entityDestroy: data });
    }
  }

  private handleChat(packet: Packet): void {
    const data = this.decodePacket(packet) as ChatMessage;
    if (data) {
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { chat: data });
    }
  }

  private handleWhisper(packet: Packet): void {
    const data = this.decodePacket(packet) as ChatMessage;
    if (data) {
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { whisper: data });
    }
  }

  private handleLobby(packet: Packet): void {
    const data = this.decodePacket(packet);
    if (data) {
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { lobby: data });
    }
  }

  private handleLobbyUpdate(packet: Packet): void {
    const data = this.decodePacket(packet) as LobbyState;
    if (data) {
      this.lobbyState = data;
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { lobbyUpdate: data });
    }
  }

  private handleLobbyJoin(packet: Packet): void {
    const data = this.decodePacket(packet) as LobbyPlayer;
    if (data && this.lobbyState) {
      this.lobbyState.players.push(data);
      this.peers.set(data.id, this.createPeerFromLobbyPlayer(data));
      this.emitEvent(NetworkEventType.PEER_CONNECTED, { peerId: data.id, player: data });
    }
  }

  private handleLobbyLeave(packet: Packet): void {
    const data = this.decodePacket(packet) as { playerId: string };
    if (data && this.lobbyState) {
      this.lobbyState.players = this.lobbyState.players.filter(p => p.id !== data.playerId);
      this.peers.delete(data.playerId);
      this.emitEvent(NetworkEventType.PEER_DISCONNECTED, { peerId: data.playerId });
    }
  }

  private handleLobbyKick(packet: Packet): void {
    const data = this.decodePacket(packet) as { playerId: string; reason: string };
    if (data) {
      if (data.playerId === this.getLocalPeerId()) {
        this.lobbyState = null;
        this.emitEvent(NetworkEventType.DISCONNECTED, { reason: data.reason });
      } else if (this.lobbyState) {
        this.lobbyState.players = this.lobbyState.players.filter(p => p.id !== data.playerId);
        this.lobbyState.kicked.add(data.playerId);
        this.peers.delete(data.playerId);
      }
    }
  }

  private handleLobbyReady(packet: Packet): void {
    const data = this.decodePacket(packet) as { playerId: string; ready: boolean };
    if (data && this.lobbyState) {
      const player = this.lobbyState.players.find(p => p.id === data.playerId);
      if (player) {
        player.isReady = data.ready;
        if (data.ready) {
          this.lobbyState.ready.add(data.playerId);
        } else {
          this.lobbyState.ready.delete(data.playerId);
        }
      }
    }
  }

  private handleGameStart(_packet: Packet): void {
    this.emitEvent(NetworkEventType.STATE_CHANGED, { gameStarting: true });
  }

  private handleGamePause(_packet: Packet): void {
    this.emitEvent(NetworkEventType.STATE_CHANGED, { gamePaused: true });
  }

  private handleGameResume(_packet: Packet): void {
    this.emitEvent(NetworkEventType.STATE_CHANGED, { gameResumed: true });
  }

  private handleGameEnd(packet: Packet): void {
    const data = this.decodePacket(packet) as { winner: string; reason: string };
    if (data) {
      this.emitEvent(NetworkEventType.STATE_CHANGED, { gameEnded: true, ...data });
    }
  }

  private handleVote(packet: Packet): void {
    const data = this.decodePacket(packet) as { playerId: string; vote: boolean };
    if (data) {
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { vote: data });
    }
  }

  private handleVoteStart(packet: Packet): void {
    const data = this.decodePacket(packet) as VoteState;
    if (data) {
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { voteStart: data });
    }
  }

  private handleVoteResult(packet: Packet): void {
    const data = this.decodePacket(packet) as { passed: boolean; voteId: string };
    if (data) {
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { voteResult: data });
    }
  }

  private handleMatchmaking(packet: Packet): void {
    const data = this.decodePacket(packet);
    if (data) {
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { matchmaking: data });
    }
  }

  private handleMatchmakingJoin(packet: Packet): void {
    const data = this.decodePacket(packet) as MatchmakingState;
    if (data) {
      this.matchmakingState = { ...this.matchmakingState, ...data };
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { matchmakingJoined: data });
    }
  }

  private handleMatchmakingLeave(_packet: Packet): void {
    this.matchmakingState.status = MatchmakingStatus.IDLE;
    this.emitEvent(NetworkEventType.PACKET_RECEIVED, { matchmakingLeft: true });
  }

  private handleMatchmakingUpdate(packet: Packet): void {
    const data = this.decodePacket(packet) as Partial<MatchmakingState>;
    if (data) {
      this.matchmakingState = { ...this.matchmakingState, ...data };
      this.emitEvent(NetworkEventType.PACKET_RECEIVED, { matchmakingUpdate: data });
    }
  }

  public sendChatMessage(content: string, channel: ChatChannel = ChatChannel.ALL): void {
    const message: ChatMessage = {
      id: this.generateId(),
      senderId: this.getLocalPeerId(),
      senderName: this.getLocalPeerName(),
      content,
      timestamp: Date.now(),
      channel,
    };
    this.sendPacket(PacketType.CHAT, PacketChannel.RELIABLE, message);
  }

  public sendWhisper(recipientId: string, content: string): void {
    const message: ChatMessage = {
      id: this.generateId(),
      senderId: this.getLocalPeerId(),
      senderName: this.getLocalPeerName(),
      content,
      timestamp: Date.now(),
      channel: ChatChannel.WHISPER,
      recipientId,
    };
    this.sendPacket(PacketType.WHISPER, PacketChannel.RELIABLE, message);
  }

  public sendInput(input: InputState): void {
    this.sendPacket(PacketType.INPUT, PacketChannel.UNRELIABLE, input);
  }

  public sendCommand(command: GameCommand): void {
    command.playerId = this.getLocalPeerId();
    command.frameNumber = this.syncState.frameNumber;
    command.timestamp = Date.now();
    this.sendPacket(PacketType.COMMAND, PacketChannel.RELIABLE_ORDERED, command);
  }

  public createLobby(name: string, settings: Partial<LobbySettings> = {}): void {
    const lobbySettings: LobbySettings = {
      gameMode: settings.gameMode || GameMode.SKIRMISH,
      mapSize: settings.mapSize || MapSize.MEDIUM,
      startingResources: settings.startingResources || 5000,
      gameSpeed: settings.gameSpeed || GameSpeed.NORMAL,
      victoryCondition: settings.victoryCondition || VictoryCondition.ANNIHILATION,
      fogOfWar: settings.fogOfWar ?? true,
      cheatsEnabled: settings.cheatsEnabled ?? false,
      lockTeams: settings.lockTeams ?? false,
      allowSpectators: settings.allowSpectators ?? true,
      password: settings.password || '',
      autoBalance: settings.autoBalance ?? false,
    };

    const localPlayer: LobbyPlayer = {
      id: this.getLocalPeerId(),
      name: this.getLocalPeerName(),
      faction: 'default',
      color: 0,
      team: 0,
      slot: 0,
      isReady: false,
      isHost: true,
      isSpectator: false,
      ping: this.state.lastPing,
      isConnected: true,
    };

    this.lobbyState = {
      id: this.generateId(),
      name,
      hostId: localPlayer.id,
      mapId: '',
      mapName: 'Default Map',
      maxPlayers: 8,
      currentPlayers: 1,
      players: [localPlayer],
      settings: lobbySettings,
      chat: [],
      ready: new Set(),
      kicked: new Set(),
      gameStarted: false,
      gameSettings: { ...lobbySettings, seed: Date.now(), timeLimit: 3600, scoreLimit: 0, unitCap: 200, resourceRichness: ResourceRichness.NORMAL, airUnits: true, ordnance: true },
    };

    this.sendPacket(PacketType.LOBBY_CREATE, PacketChannel.RELIABLE, this.lobbyState);
  }

  public joinLobby(lobbyId: string, password?: string): void {
    const joinData = { lobbyId, password: password || '' };
    this.sendPacket(PacketType.LOBBY_JOIN, PacketChannel.RELIABLE, joinData);
  }

  public leaveLobby(): void {
    if (this.lobbyState) {
      this.sendPacket(PacketType.LOBBY_LEAVE, PacketChannel.RELIABLE, { lobbyId: this.lobbyState.id });
      this.lobbyState = null;
    }
  }

  public kickPlayer(playerId: string): void {
    if (this.lobbyState && this.lobbyState.hostId === this.getLocalPeerId()) {
      this.sendPacket(PacketType.LOBBY_KICK, PacketChannel.RELIABLE, { lobbyId: this.lobbyState.id, playerId });
    }
  }

  public setReady(ready: boolean): void {
    if (this.lobbyState) {
      this.sendPacket(PacketType.LOBBY_READY, PacketChannel.RELIABLE, {
        lobbyId: this.lobbyState.id,
        playerId: this.getLocalPeerId(),
        ready,
      });
    }
  }

  public startGame(): void {
    if (this.lobbyState && this.lobbyState.hostId === this.getLocalPeerId()) {
      this.sendPacket(PacketType.GAME_START, PacketChannel.RELIABLE, {
        lobbyId: this.lobbyState.id,
        settings: this.lobbyState.gameSettings,
      });
      this.lobbyState.gameStarted = true;
    }
  }

  public startMatchmaking(region: string = 'us-west'): void {
    this.matchmakingState.status = MatchmakingStatus.SEARCHING;
    this.matchmakingState.region = region;
    this.matchmakingState.queueTime = Date.now();
    this.sendPacket(PacketType.MATCHMAKING_JOIN, PacketChannel.RELIABLE, this.matchmakingState);
  }

  public cancelMatchmaking(): void {
    if (this.matchmakingState.status === MatchmakingStatus.SEARCHING) {
      this.sendPacket(PacketType.MATCHMAKING_LEAVE, PacketChannel.RELIABLE, {
        queueId: this.matchmakingState.queueId,
      });
      this.matchmakingState.status = MatchmakingStatus.IDLE;
    }
  }

  public vote(voteType: VoteType, vote: boolean, targetId?: string): void {
    this.sendPacket(PacketType.VOTE, PacketChannel.RELIABLE, {
      voteType,
      vote,
      targetId,
      voterId: this.getLocalPeerId(),
    });
  }

  public startVote(voteType: VoteType, targetId?: string): void {
    this.sendPacket(PacketType.VOTE_START, PacketChannel.RELIABLE, {
      voteType,
      initiatorId: this.getLocalPeerId(),
      targetId,
      lobbyId: this.lobbyState?.id,
    });
  }

  private createPeerFromLobbyPlayer(player: LobbyPlayer): NetworkPeer {
    return {
      id: player.id,
      name: player.name,
      address: '',
      port: 0,
      isHost: player.isHost,
      isLocal: false,
      isReady: player.isReady,
      ping: player.ping,
      score: 0,
      faction: player.faction,
      color: String(player.color),
      avatar: '',
      isConnected: player.isConnected,
      lastActivity: Date.now(),
      metadata: {},
    };
  }

  public getPeer(peerId: string): NetworkPeer | undefined {
    return this.peers.get(peerId);
  }

  public getAllPeers(): NetworkPeer[] {
    return Array.from(this.peers.values());
  }

  public getConnectionState(): ConnectionState {
    return { ...this.state };
  }

  public getSyncState(): SyncState {
    return { ...this.syncState };
  }

  public getLobbyState(): LobbyState | null {
    return this.lobbyState ? { ...this.lobbyState } : null;
  }

  public getMatchmakingState(): MatchmakingState {
    return { ...this.matchmakingState };
  }

  public getInputBuffer(): InputState[] {
    return [...this.inputBuffer];
  }

  public getCommandBuffer(): GameCommand[] {
    return [...this.commandBuffer];
  }

  private getLocalPeerId(): string {
    return 'local-player';
  }

  private getLocalPeerName(): string {
    return 'Player';
  }

  private generatePacketId(): string {
    return `pkt-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  public on(event: NetworkEventType, callback: (event: NetworkEvent) => void): void {
    if (!this.eventListeners.has(event)) {
      this.eventListeners.set(event, new Set());
    }
    this.eventListeners.get(event)!.add(callback);
  }

  public off(event: NetworkEventType, callback: (event: NetworkEvent) => void): void {
    const listeners = this.eventListeners.get(event);
    if (listeners) {
      listeners.delete(callback);
    }
  }

  private emitEvent(type: NetworkEventType, data?: unknown): void {
    const event: NetworkEvent = {
      type,
      data,
      timestamp: Date.now(),
    };
    const listeners = this.eventListeners.get(type);
    if (listeners) {
      listeners.forEach(callback => callback(event));
    }
    const allListeners = this.eventListeners.get(NetworkEventType.STATE_CHANGED);
    if (allListeners && type !== NetworkEventType.STATE_CHANGED) {
      allListeners.forEach(callback => callback(event));
    }
  }

  public update(deltaTime: number): void {
    this.processPacketQueue();
    this.updateSyncState(deltaTime);
    this.updateMatchmaking(deltaTime);
    this.updatePeers(deltaTime);
    this.checkTimeouts();
  }

  private processPacketQueue(): void {
    while (this.packetQueue.length > 0) {
      const packet = this.packetQueue.shift();
      if (packet) {
        this.handlePacket(packet);
      }
    }
  }

  private updateSyncState(deltaTime: number): void {
    if (this.state.connected) {
      this.syncState.frameNumber += deltaTime * 60;
      this.syncState.checksum = this.calculateChecksum();
    }
  }

  private calculateChecksum(): number {
    return Math.floor(Math.random() * 0xFFFFFFFF);
  }

  private updateMatchmaking(deltaTime: number): void {
    if (this.matchmakingState.status === MatchmakingStatus.SEARCHING) {
      this.matchmakingState.queueTime += deltaTime * 1000;
      this.matchmakingState.estimatedWait = Math.max(0, 60 - this.matchmakingState.queueTime / 1000);
    }
  }

  private updatePeers(deltaTime: number): void {
    const timeoutThreshold = 60000;
    const now = Date.now();
    for (const [peerId, peer] of this.peers) {
      if (now - peer.lastActivity > timeoutThreshold) {
        peer.isConnected = false;
        this.emitEvent(NetworkEventType.TIMEOUT, { peerId });
      }
    }
  }

  private checkTimeouts(): void {
    if (this.state.connected && this.state.lastPing > this.config.timeout) {
      this.emitEvent(NetworkEventType.TIMEOUT);
      if (this.config.reconnectAttempts > 0) {
        this.attemptReconnect();
      }
    }
  }

  private async attemptReconnect(): Promise<void> {
    this.state.reconnecting = true;
    this.emitEvent(NetworkEventType.RECONNECTING);
    for (let attempt = 0; attempt < this.config.reconnectAttempts; attempt++) {
      try {
        await new Promise(resolve => setTimeout(resolve, this.config.reconnectDelay * Math.pow(2, attempt)));
        await this.connect();
        this.state.reconnecting = false;
        return;
      } catch {
        console.log(`Reconnect attempt ${attempt + 1} failed`);
      }
    }
    this.state.reconnecting = false;
    this.state.error = 'Failed to reconnect';
    this.emitEvent(NetworkEventType.ERROR, { reason: 'Reconnection failed' });
  }

  public setEncryptionKey(key: CryptoKey): void {
    this.encryptionKey = key;
  }

  public enableCompression(enable: boolean): void {
    this.config.enableCompression = enable;
  }

  public setMaxConnections(max: number): void {
    this.config.maxConnections = max;
  }

  public getPacketStats(): { sent: number; received: number; bytesSent: number; bytesReceived: number } {
    return {
      sent: this.state.packetsSent,
      received: this.state.packetsReceived,
      bytesSent: this.state.bytesSent,
      bytesReceived: this.state.bytesReceived,
    };
  }

  public getPeerCount(): number {
    return this.peers.size;
  }

  public isConnected(): boolean {
    return this.state.connected;
  }

  public isHost(): boolean {
    return this.lobbyState?.hostId === this.getLocalPeerId();
  }

  public isInLobby(): boolean {
    return this.lobbyState !== null && !this.lobbyState.gameStarted;
  }

  public isInGame(): boolean {
    return this.lobbyState?.gameStarted ?? false;
  }

  public isMatchmaking(): boolean {
    return this.matchmakingState.status === MatchmakingStatus.SEARCHING ||
           this.matchmakingState.status === MatchmakingStatus.FOUND ||
           this.matchmakingState.status === MatchmakingStatus.ACCEPTING;
  }

  public clearBuffers(): void {
    this.inputBuffer = [];
    this.commandBuffer = [];
    this.packetQueue = [];
  }

  public reset(): void {
    this.cleanup();
    this.lobbyState = null;
    this.matchmakingState = this.createInitialMatchmakingState();
    this.syncState = this.createInitialSyncState();
    this.replayData = null;
    this.clearBuffers();
  }

  public destroy(): void {
    this.reset();
    this.eventListeners.clear();
    this.messageHandlers.clear();
  }
}

export const createNetworkManager = (config?: Partial<NetworkConfig>): NetworkManager => {
  return new NetworkManager(config);
};

export default NetworkManager;

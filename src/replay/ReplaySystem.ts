export interface ReplayFile {
  id: string;
  metadata: ReplayMetadata;
  header: ReplayHeader;
  frames: ReplayFrame[];
  events: ReplayEvent[];
  checksums: ReplayChecksum[];
  footer: ReplayFooter;
}

export interface ReplayMetadata {
  id: string;
  gameVersion: string;
  gameMode: GameMode;
  mapName: string;
  mapId: string;
  mapHash: string;
  duration: number;
  timestamp: number;
  players: PlayerInfo[];
  winner: number;
  matchId: string;
  serverName: string;
  serverRegion: string;
  ranked: boolean;
  tournament: boolean;
  modInfo?: ModInfo;
  gameSettings: GameSettings;
}

export type GameMode =
  | 'skirmish'
  | 'multiplayer'
  | 'campaign'
  | 'tournament'
  | 'co_op'
  | 'custom';

export interface PlayerInfo {
  id: string;
  name: string;
  faction: string;
  team: number;
  color: string;
  isAI: boolean;
  difficulty?: Difficulty;
  isHost: boolean;
  isSpectator: boolean;
  ping: number;
  rank?: PlayerRank;
  country?: string;
  avatar?: string;
  hand: number;
}

export type Difficulty = 'easy' | 'normal' | 'hard' | 'brutal';

export interface PlayerRank {
  rating: number;
  division: string;
  league: string;
  wins: number;
  losses: number;
  streak: number;
}

export interface ModInfo {
  id: string;
  name: string;
  version: string;
  checksum: string;
}

export interface GameSettings {
  gameSpeed: GameSpeed;
  startingCredits: number;
  mapRevealed: boolean;
  fogOfWar: boolean;
  friendlyFire: boolean;
  superweapons: boolean;
  crateSpawn: boolean;
  shortGame: boolean;
  resources: string;
}

export type GameSpeed = 'slow' | 'normal' | 'fast' | 'very_fast';

export interface ReplayHeader {
  magic: string;
  version: number;
  offsetToFrames: number;
  offsetToEvents: number;
  offsetToFooter: number;
  compression: CompressionType;
  encryption: EncryptionType;
  reserved: number;
}

export type CompressionType =
  | 'none'
  | 'zlib'
  | 'lzma'
  | 'zstd'
  | 'lz4';

export type EncryptionType =
  | 'none'
  | 'xor'
  | 'aes128'
  | 'aes256';

export interface ReplayFrame {
  index: number;
  timestamp: number;
  gameTime: number;
  actions: GameAction[];
  state: GameState;
}

export interface GameAction {
  id: string;
  type: ActionType;
  playerId: string;
  timestamp: number;
  parameters: ActionParameters;
  target?: ActionTarget;
  result?: ActionResult;
}

export type ActionType =
  | 'unit_created'
  | 'unit_destroyed'
  | 'unit_moved'
  | 'unit_attacked'
  | 'unit_sold'
  | 'unit_captured'
  | 'building_placed'
  | 'building_completed'
  | 'building_destroyed'
  | 'building_sold'
  | 'building_upgraded'
  | 'research_started'
  | 'research_completed'
  | 'resource_collected'
  | 'resource_spent'
  | 'territory_captured'
  | 'territory_lost'
  | 'unit_trained'
  | 'ability_used'
  | 'ability_cancelled'
  | 'group_selected'
  | 'group_formed'
  | 'attack_moved'
  | 'patrol_set'
  | 'rally_point_set'
  | 'guard_mode_changed'
  | 'stance_changed'
  | 'waypoint_added'
  | 'waypoint_cleared'
  | 'ally_changed'
  | 'chat_sent'
  | 'pause_game'
  | 'resume_game'
  | 'surrender'
  | 'vote_started'
  | 'vote_cast'
  | 'cheat_used'
  | 'debug_command'
  | 'sync_error'
  | 'connection_lost'
  | 'connection_established';

export interface ActionParameters {
  unitType?: string;
  buildingType?: string;
  researchId?: string;
  abilityId?: string;
  unitId?: string;
  groupId?: number;
  targetId?: string;
  amount?: number;
  x?: number;
  y?: number;
  z?: number;
  data?: Record<string, unknown>;
}

export interface ActionTarget {
  type: 'unit' | 'building' | 'position' | 'terrain' | 'none';
  id?: string;
  x?: number;
  y?: number;
  z?: number;
  radius?: number;
}

export interface ActionResult {
  success: boolean;
  error?: string;
  damage?: number;
  healing?: number;
  credits?: number;
  ore?: number;
}

export interface GameState {
  time: number;
  tick: number;
  paused: boolean;
  pausedBy?: string;
  players: PlayerState[];
  units: UnitState[];
  buildings: BuildingState[];
  resources: ResourceState;
  map: MapState;
  events: EventState;
  teams: TeamState[];
}

export interface PlayerState {
  id: string;
  active: boolean;
  credits: number;
  ore: number;
  power: number;
  maxPower: number;
  unitsProduced: number;
  unitsLost: number;
  buildingsConstructed: number;
  buildingsLost: number;
  enemiesDestroyed: number;
  score: number;
  isDefeated: boolean;
  defeatReason?: string;
}

export interface UnitState {
  id: string;
  owner: string;
  type: string;
  position: Position;
  health: number;
  maxHealth: number;
  experience: number;
  level: number;
  status: UnitStatus;
  target?: string;
  path?: Position[];
  state: string;
  specialPowerReady?: boolean;
  ammo?: number;
  fuel?: number;
  passengers?: string[];
}

export type Position = { x: number; y: number; z: number };

export interface BuildingState {
  id: string;
  owner: string;
  type: string;
  position: Position;
  health: number;
  maxHealth: number;
  active: boolean;
  powered: boolean;
  constructing: boolean;
  constructionPercent: number;
  upgradeLevel: number;
  productionQueue?: ProductionQueueItem[];
  garrisoned?: string[];
  rallyPoint?: Position;
  specialPowerReady?: boolean;
  isFogged: boolean;
  isHidden: boolean;
}

export interface ProductionQueueItem {
  id: string;
  type: 'unit' | 'upgrade' | 'special_power';
  itemId: string;
  progress: number;
  cost: number;
  timeRemaining: number;
}

export interface ResourceState {
  globalCredits: number;
  globalOre: number;
  oreMineCount: number;
  creditBuildings: number;
  totalIncome: number;
  totalSpent: number;
  tradeShips: number;
  cratesCollected: number;
}

export interface MapState {
  width: number;
  height: number;
  revealedTiles: Map<string, boolean>;
  territoryControl: Map<string, TerritoryInfo>;
  structures: MapStructure[];
  decorations: MapDecoration[];
  cliffs: Cliff[];
  water: WaterBody[];
}

export interface TerritoryInfo {
  owner: string;
  value: number;
  lastCaptureTime: number;
}

export interface MapStructure {
  id: string;
  type: string;
  position: Position;
  health: number;
  owner?: string;
  isTarget: boolean;
}

export interface MapDecoration {
  type: string;
  position: Position;
  scale: number;
  rotation: number;
}

export interface Cliff {
  start: Position;
  end: Position;
  height: number;
}

export interface WaterBody {
  type: 'shallow' | 'deep' | 'ice';
  vertices: Position[];
  animated: boolean;
}

export interface EventState {
  victoryConditions: VictoryCondition[];
  objectives: Objective[];
  alerts: Alert[];
  cinematics: CinematicEvent[];
}

export interface VictoryCondition {
  type: VictoryType;
  targetValue: number;
  currentValue: number;
  completed: boolean;
  team?: number;
}

export type VictoryType =
  | 'annihilation'
  | 'supremacy'
  | 'time_limit'
  | 'capture_all_tech_buildings'
  | 'destroy_all_structures'
  | 'collect_ores'
  | 'survival';

export interface Objective {
  id: string;
  type: ObjectiveType;
  description: string;
  target?: string;
  targetPosition?: Position;
  completed: boolean;
  bonus: boolean;
  bonusAwarded: boolean;
  timeLimit?: number;
  playerId?: string;
}

export type ObjectiveType =
  | 'primary'
  | 'secondary'
  | 'bonus'
  | 'hidden'
  | 'campaign';

export interface Alert {
  id: string;
  type: AlertType;
  message: string;
  position?: Position;
  duration: number;
  timestamp: number;
  playerId?: string;
  read: boolean;
}

export type AlertType =
  | 'info'
  | 'warning'
  | 'error'
  | 'victory'
  | 'defeat'
  | 'unit_ready'
  | 'base_under_attack'
  | 'low_power'
  | 'low_credits'
  | 'structure_destroyed';

export interface CinematicEvent {
  id: string;
  type: CinematicType;
  startTime: number;
  endTime: number;
  data: CinematicData;
  skipped: boolean;
}

export type CinematicType =
  | 'camera_move'
  | 'dialogue'
  | 'fade'
  | 'text_overlay'
  | 'music_change'
  | 'sound_effect';

export interface CinematicData {
  cameraPath?: Position[];
  dialogue?: DialogueLine[];
  fadeColor?: string;
  fadeIn?: boolean;
  text?: string;
  textStyle?: TextStyle;
  musicTrack?: string;
  soundEffect?: string;
}

export interface DialogueLine {
  speaker: string;
  text: string;
  emotion: string;
  duration: number;
}

export interface TextStyle {
  fontSize: number;
  fontFamily: string;
  color: string;
  backgroundColor?: string;
  position: 'top' | 'center' | 'bottom';
}

export interface TeamState {
  id: number;
  players: string[];
  sharedControl: boolean;
  sharedVisibility: boolean;
  alliedVictory: boolean;
}

export interface UnitStatus {
  moving: boolean;
  attacking: boolean;
  harvesting: boolean;
  repairing: boolean;
  constructing: boolean;
  idle: boolean;
  selected: boolean;
  loaded: boolean;
  garrisoned: boolean;
  stunned: boolean;
  cloak: boolean;
  ghost: boolean;
}

export interface ReplayEvent {
  id: string;
  type: ReplayEventType;
  timestamp: number;
  gameTime: number;
  data: unknown;
}

export type ReplayEventType =
  | 'game_started'
  | 'game_paused'
  | 'game_resumed'
  | 'game_ended'
  | 'player_joined'
  | 'player_left'
  | 'player_defeated'
  | 'player_victorious'
  | 'player_surrendered'
  | 'vote_initiated'
  | 'vote_concluded'
  | 'chat_message'
  | 'alliance_changed'
  | 'spectator_added'
  | 'cheat_activated'
  | 'bookmark'
  | 'marker'
  | 'critical_error'
  | 'save_point';

export interface ReplayChecksum {
  frame: number;
  timestamp: number;
  hash: string;
  seed: number;
  stateSnapshot?: string;
  verified: boolean;
}

export interface ReplayFooter {
  totalFrames: number;
  totalEvents: number;
  duration: number;
  finalScores: FinalScore[];
  awards: Award[];
  checksum: string;
  signature?: string;
}

export interface FinalScore {
  playerId: string;
  rank: number;
  score: number;
  breakdown: ScoreBreakdown;
}

export interface ScoreBreakdown {
  combat: number;
  economy: number;
  technology: number;
  structures: number;
  objectives: number;
  bonuses: number;
  penalties: number;
  total: number;
}

export interface Award {
  type: AwardType;
  playerId: string;
  value: number;
  description: string;
}

export type AwardType =
  | 'most_units_produced'
  | 'most_units_destroyed'
  | 'most_buildings_constructed'
  | 'most_buildings_destroyed'
  | 'most_ore_collected'
  | 'fastest_victory'
  | 'longest_survival'
  | 'highest_kill_streak'
  | 'most_ability_uses'
  | 'least_units_lost'
  | 'highest_damage_dealt'
  | 'most_territory_captured';

export interface ReplayBookmark {
  id: string;
  name: string;
  frame: number;
  gameTime: number;
  description?: string;
  color?: string;
  createdAt: number;
}

export interface ReplayMarker {
  id: string;
  type: 'important' | 'bookmark' | 'comment';
  frame: number;
  gameTime: number;
  text: string;
  playerId?: string;
  position?: Position;
}

export class ReplayRecorder {
  private replay: ReplayFile;
  private frameBuffer: ReplayFrame[] = [];
  private eventBuffer: ReplayEvent[] = [];
  private actionBuffer: GameAction[] = [];
  private currentState: GameState;
  private isRecording: boolean = false;
  private frameIndex: number = 0;
  private startTime: number = 0;
  private gameTime: number = 0;
  private compression: CompressionType = 'zlib';
  private encryption: EncryptionType = 'none';
  private maxBufferSize: number = 100;
  private checksumInterval: number = 30;
  private bookmarks: ReplayBookmark[] = [];
  private markers: ReplayMarker[] = [];
  private listeners: Map<string, Set<Function>> = new Map();
  private syncCheckInterval: number = 60;
  private lastSyncCheck: number = 0;

  constructor(metadata: ReplayMetadata) {
    this.replay = this.createReplay(metadata);
    this.currentState = this.createInitialState();
  }

  private createReplay(metadata: ReplayMetadata): ReplayFile {
    return {
      id: `replay_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      metadata,
      header: {
        magic: 'RPLY',
        version: 3,
        offsetToFrames: 0,
        offsetToEvents: 0,
        offsetToFooter: 0,
        compression: this.compression,
        encryption: this.encryption,
        reserved: 0
      },
      frames: [],
      events: [],
      checksums: [],
      footer: {
        totalFrames: 0,
        totalEvents: 0,
        duration: 0,
        finalScores: [],
        awards: [],
        checksum: ''
      }
    };
  }

  private createInitialState(): GameState {
    return {
      time: 0,
      tick: 0,
      paused: false,
      players: [],
      units: [],
      buildings: [],
      resources: {
        globalCredits: 0,
        globalOre: 0,
        oreMineCount: 0,
        creditBuildings: 0,
        totalIncome: 0,
        totalSpent: 0,
        tradeShips: 0,
        cratesCollected: 0
      },
      map: {
        width: 0,
        height: 0,
        revealedTiles: new Map(),
        territoryControl: new Map(),
        structures: [],
        decorations: [],
        cliffs: [],
        water: []
      },
      events: {
        victoryConditions: [],
        objectives: [],
        alerts: [],
        cinematics: []
      },
      teams: []
    };
  }

  startRecording(): void {
    if (this.isRecording) return;

    this.isRecording = true;
    this.startTime = Date.now();
    this.frameIndex = 0;
    this.gameTime = 0;

    this.recordEvent({
      id: `event_${Date.now()}`,
      type: 'game_started',
      timestamp: this.startTime,
      gameTime: this.gameTime,
      data: {
        version: this.replay.metadata.gameVersion,
        mode: this.replay.metadata.gameMode
      }
    });

    this.emit('recording_started', { replayId: this.replay.id });
  }

  stopRecording(): void {
    if (!this.isRecording) return;

    this.isRecording = false;

    this.flushBuffers();

    this.recordEvent({
      id: `event_${Date.now()}`,
      type: 'game_ended',
      timestamp: Date.now(),
      gameTime: this.gameTime,
      data: {
        winner: this.replay.metadata.winner,
        duration: this.replay.metadata.duration
      }
    });

    this.finalizeReplay();
    this.emit('recording_stopped', { replayId: this.replay.id });
  }

  pauseRecording(): void {
    this.currentState.paused = true;
    this.recordEvent({
      id: `event_${Date.now()}`,
      type: 'game_paused',
      timestamp: Date.now(),
      gameTime: this.gameTime,
      data: {}
    });
  }

  resumeRecording(): void {
    this.currentState.paused = false;
    this.recordEvent({
      id: `event_${Date.now()}`,
      type: 'game_resumed',
      timestamp: Date.now(),
      gameTime: this.gameTime,
      data: {}
    });
  }

  recordAction(action: GameAction): void {
    if (!this.isRecording) return;

    action.timestamp = Date.now();
    action.id = `action_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    action.parameters = action.parameters || {};

    this.actionBuffer.push(action);

    if (action.type === 'unit_created' || action.type === 'unit_destroyed') {
      this.updateUnitState(action);
    } else if (action.type === 'building_placed' || action.type === 'building_destroyed') {
      this.updateBuildingState(action);
    } else if (action.type === 'resource_collected' || action.type === 'resource_spent') {
      this.updateResourceState(action);
    }
  }

  private updateUnitState(action: GameAction): void {
    const unitId = action.parameters.unitId || action.target?.id;
    if (!unitId) return;

    switch (action.type) {
      case 'unit_created':
        const newUnit: UnitState = {
          id: unitId,
          owner: action.playerId,
          type: action.parameters.unitType || '',
          position: {
            x: action.target?.x || 0,
            y: action.target?.y || 0,
            z: action.target?.z || 0
          },
          health: 100,
          maxHealth: 100,
          experience: 0,
          level: 1,
          status: {
            moving: false,
            attacking: false,
            harvesting: false,
            repairing: false,
            constructing: false,
            idle: true,
            selected: false,
            loaded: false,
            garrisoned: false,
            stunned: false,
            cloak: false,
            ghost: false
          }
        };
        this.currentState.units.push(newUnit);
        break;

      case 'unit_destroyed':
        this.currentState.units = this.currentState.units.filter(u => u.id !== unitId);
        break;
    }
  }

  private updateBuildingState(action: GameAction): void {
    const buildingId = action.target?.id;
    if (!buildingId) return;

    switch (action.type) {
      case 'building_placed':
        const newBuilding: BuildingState = {
          id: buildingId,
          owner: action.playerId,
          type: action.parameters.buildingType || '',
          position: {
            x: action.target?.x || 0,
            y: action.target?.y || 0,
            z: action.target?.z || 0
          },
          health: 100,
          maxHealth: 100,
          active: false,
          powered: true,
          constructing: true,
          constructionPercent: 0,
          upgradeLevel: 1,
          isFogged: false,
          isHidden: false
        };
        this.currentState.buildings.push(newBuilding);
        break;

      case 'building_destroyed':
        this.currentState.buildings = this.currentState.buildings.filter(b => b.id !== buildingId);
        break;
    }
  }

  private updateResourceState(action: GameAction): void {
    const amount = action.parameters.amount || 0;

    if (action.type === 'resource_collected') {
      this.currentState.resources.globalCredits += amount;
      this.currentState.resources.totalIncome += amount;
    } else if (action.type === 'resource_spent') {
      this.currentState.resources.globalCredits -= amount;
      this.currentState.resources.totalSpent += amount;
    }
  }

  recordEvent(event: ReplayEvent): void {
    if (!this.isRecording) return;

    event.id = `event_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    this.eventBuffer.push(event);
  }

  updateGameState(state: Partial<GameState>): void {
    this.currentState = {
      ...this.currentState,
      ...state,
      time: Date.now() - this.startTime,
      tick: this.frameIndex
    };
  }

  addBookmark(name: string, description?: string): ReplayBookmark {
    const bookmark: ReplayBookmark = {
      id: `bookmark_${Date.now()}`,
      name,
      frame: this.frameIndex,
      gameTime: this.gameTime,
      description,
      color: '#FFFF00',
      createdAt: Date.now()
    };

    this.bookmarks.push(bookmark);

    this.recordEvent({
      id: `event_${Date.now()}`,
      type: 'bookmark',
      timestamp: Date.now(),
      gameTime: this.gameTime,
      data: bookmark
    });

    return bookmark;
  }

  addMarker(type: 'important' | 'bookmark' | 'comment', text: string, position?: Position): ReplayMarker {
    const marker: ReplayMarker = {
      id: `marker_${Date.now()}`,
      type,
      frame: this.frameIndex,
      gameTime: this.gameTime,
      text,
      position
    };

    this.markers.push(marker);

    this.recordEvent({
      id: `event_${Date.now()}`,
      type: 'marker',
      timestamp: Date.now(),
      gameTime: this.gameTime,
      data: marker
    });

    return marker;
  }

  tick(deltaTime: number): void {
    if (!this.isRecording) return;

    this.gameTime += deltaTime;

    if (this.frameIndex % this.syncCheckInterval === 0) {
      this.performSyncCheck();
    }

    if (this.frameIndex % this.checksumInterval === 0) {
      this.recordChecksum();
    }

    this.frameIndex++;

    if (this.frameIndex % this.maxBufferSize === 0) {
      this.flushBuffers();
    }
  }

  private performSyncCheck(): void {
    const stateHash = this.calculateStateHash();

    this.recordEvent({
      id: `event_${Date.now()}`,
      type: 'sync_error',
      timestamp: Date.now(),
      gameTime: this.gameTime,
      data: {
        frame: this.frameIndex,
        hash: stateHash,
        valid: true
      }
    });

    this.lastSyncCheck = this.frameIndex;
  }

  private calculateStateHash(): string {
    const stateString = JSON.stringify(this.currentState);
    return this.hashString(stateString);
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

  private recordChecksum(): void {
    const checksum: ReplayChecksum = {
      frame: this.frameIndex,
      timestamp: Date.now(),
      hash: this.calculateStateHash(),
      seed: Math.random() * 0xFFFFFFFF,
      verified: true
    };

    this.replay.checksums.push(checksum);
  }

  private flushBuffers(): void {
    if (this.actionBuffer.length > 0 || this.frameIndex % 10 === 0) {
      const frame: ReplayFrame = {
        index: Math.floor(this.frameIndex / 10),
        timestamp: Date.now(),
        gameTime: this.gameTime,
        actions: [...this.actionBuffer],
        state: this.cloneState(this.currentState)
      };

      this.replay.frames.push(frame);
      this.actionBuffer = [];
    }

    if (this.eventBuffer.length > 0) {
      this.replay.events.push(...this.eventBuffer);
      this.eventBuffer = [];
    }
  }

  private cloneState(state: GameState): GameState {
    return JSON.parse(JSON.stringify(state));
  }

  private finalizeReplay(): void {
    this.flushBuffers();

    this.replay.footer.totalFrames = this.replay.frames.length;
    this.replay.footer.totalEvents = this.replay.events.length;
    this.replay.footer.duration = this.gameTime;

    this.replay.footer.finalScores = this.calculateFinalScores();
    this.replay.footer.awards = this.calculateAwards();
    this.replay.footer.checksum = this.calculateReplayChecksum();

    this.replay.header.offsetToFrames = this.calculateFramesOffset();
    this.replay.header.offsetToEvents = this.calculateEventsOffset();
    this.replay.header.offsetToFooter = this.calculateFooterOffset();

    this.replay.metadata.duration = this.gameTime;
  }

  private calculateFinalScores(): FinalScore[] {
    const scores: FinalScore[] = [];

    this.replay.metadata.players.forEach(player => {
      const playerUnits = this.currentState.units.filter(u => u.owner === player.id);
      const playerBuildings = this.currentState.buildings.filter(b => b.owner === player.id);

      const combat = playerUnits.length * 10;
      const economy = this.currentState.resources.globalCredits;
      const structures = playerBuildings.length * 50;

      const score: FinalScore = {
        playerId: player.id,
        rank: 0,
        score: combat + economy + structures,
        breakdown: {
          combat,
          economy,
          technology: 0,
          structures,
          objectives: 0,
          bonuses: 0,
          penalties: 0,
          total: combat + economy + structures
        }
      };

      scores.push(score);
    });

    scores.sort((a, b) => b.score - a.score);
    scores.forEach((score, index) => {
      score.rank = index + 1;
    });

    return scores;
  }

  private calculateAwards(): Award[] {
    const awards: Award[] = [];

    let maxUnitsProduced = 0;
    let mostUnitsProducedPlayer = '';

    this.replay.metadata.players.forEach(player => {
      const unitsProduced = this.replay.events.filter(
        e => e.type === 'player_defeated' && e.data === player.id
      ).length;

      if (unitsProduced > maxUnitsProduced) {
        maxUnitsProduced = unitsProduced;
        mostUnitsProducedPlayer = player.id;
      }
    });

    if (mostUnitsProducedPlayer) {
      awards.push({
        type: 'most_units_produced',
        playerId: mostUnitsProducedPlayer,
        value: maxUnitsProduced,
        description: `Produced ${maxUnitsProduced} units`
      });
    }

    return awards;
  }

  private calculateReplayChecksum(): string {
    const framesData = JSON.stringify(this.replay.frames);
    const eventsData = JSON.stringify(this.replay.events);
    const checksum = this.hashString(framesData + eventsData);
    return checksum;
  }

  private calculateFramesOffset(): number {
    return JSON.stringify(this.replay.header).length;
  }

  private calculateEventsOffset(): number {
    return this.calculateFramesOffset() + this.replay.frames.length * 100;
  }

  private calculateFooterOffset(): number {
    return this.calculateEventsOffset() + this.replay.events.length * 100;
  }

  getReplay(): ReplayFile {
    return this.replay;
  }

  getReplayJson(): string {
    return JSON.stringify(this.replay);
  }

  getDuration(): number {
    return this.gameTime;
  }

  getFrameCount(): number {
    return this.frameIndex;
  }

  isCurrentlyRecording(): boolean {
    return this.isRecording;
  }

  getBookmarks(): ReplayBookmark[] {
    return [...this.bookmarks];
  }

  getMarkers(): ReplayMarker[] {
    return [...this.markers];
  }

  getCurrentState(): GameState {
    return this.cloneState(this.currentState);
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  serialize(): string {
    return JSON.stringify(this.replay);
  }

  deserialize(json: string): void {
    try {
      this.replay = JSON.parse(json);
      this.frameIndex = this.replay.footer.totalFrames * 10;
      this.gameTime = this.replay.footer.duration;
    } catch (error) {
      console.error('Failed to deserialize replay:', error);
    }
  }
}

export class ReplayPlayer {
  private replay: ReplayFile;
  private currentFrameIndex: number = 0;
  private currentEventIndex: number = 0;
  private isPlaying: boolean = false;
  private isPaused: boolean = false;
  private playbackSpeed: number = 1.0;
  private position: number = 0;
  private volume: number = 1.0;
  private muted: boolean = false;
  private listeners: Map<string, Set<Function>> = new Map();
  private animationFrameId: number | null = null;
  private lastFrameTime: number = 0;
  private targetFrameTime: number = 1000 / 30;
  private loop: boolean = false;
  private currentState: GameState;
  private filteredEventTypes: Set<string> = new Set();
  private skipFrames: Set<number> = new Set();
  private visibilityFilter: Set<string> = new Set();

  constructor(replay: ReplayFile) {
    this.replay = replay;
    this.currentState = this.createEmptyState();
  }

  private createEmptyState(): GameState {
    return {
      time: 0,
      tick: 0,
      paused: false,
      players: [],
      units: [],
      buildings: [],
      resources: {
        globalCredits: 0,
        globalOre: 0,
        oreMineCount: 0,
        creditBuildings: 0,
        totalIncome: 0,
        totalSpent: 0,
        tradeShips: 0,
        cratesCollected: 0
      },
      map: {
        width: 0,
        height: 0,
        revealedTiles: new Map(),
        territoryControl: new Map(),
        structures: [],
        decorations: [],
        cliffs: [],
        water: []
      },
      events: {
        victoryConditions: [],
        objectives: [],
        alerts: [],
        cinematics: []
      },
      teams: []
    };
  }

  load(replayData: string | ReplayFile): boolean {
    try {
      if (typeof replayData === 'string') {
        this.replay = JSON.parse(replayData);
      } else {
        this.replay = replayData;
      }

      this.validateReplay();

      this.currentFrameIndex = 0;
      this.currentEventIndex = 0;
      this.position = 0;

      return true;
    } catch (error) {
      console.error('Failed to load replay:', error);
      return false;
    }
  }

  private validateReplay(): void {
    if (!this.replay.header.magic || this.replay.header.magic !== 'RPLY') {
      throw new Error('Invalid replay format');
    }

    if (!this.replay.metadata || !this.replay.metadata.gameVersion) {
      throw new Error('Missing replay metadata');
    }

    if (!Array.isArray(this.replay.frames)) {
      throw new Error('Invalid frames data');
    }

    if (!Array.isArray(this.replay.events)) {
      throw new Error('Invalid events data');
    }
  }

  play(): void {
    if (this.isPlaying) return;

    this.isPlaying = true;
    this.isPaused = false;
    this.lastFrameTime = performance.now();

    this.startPlaybackLoop();

    this.emit('play', { position: this.position });
  }

  pause(): void {
    if (!this.isPlaying) return;

    this.isPaused = true;

    if (this.animationFrameId !== null) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }

    this.emit('pause', { position: this.position });
  }

  stop(): void {
    this.isPlaying = false;
    this.isPaused = false;
    this.currentFrameIndex = 0;
    this.currentEventIndex = 0;
    this.position = 0;

    if (this.animationFrameId !== null) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }

    this.currentState = this.createEmptyState();

    this.emit('stop', {});
  }

  seek(position: number): void {
    position = Math.max(0, Math.min(1, position));
    this.position = position;

    const frameIndex = Math.floor(position * this.replay.frames.length);
    this.currentFrameIndex = frameIndex;

    const eventThreshold = this.replay.frames[frameIndex]?.gameTime || 0;
    this.currentEventIndex = this.replay.events.findIndex(e => e.gameTime >= eventThreshold);

    this.rebuildStateAtPosition();

    this.emit('seek', { position, frameIndex });
  }

  private rebuildStateAtPosition(): void {
    this.currentState = this.createEmptyState();

    for (let i = 0; i <= this.currentFrameIndex; i++) {
      const frame = this.replay.frames[i];
      if (!frame) continue;

      frame.actions.forEach(action => {
        this.applyAction(action);
      });

      this.currentState = frame.state;
    }
  }

  private applyAction(action: GameAction): void {
    switch (action.type) {
      case 'unit_created':
        this.handleUnitCreated(action);
        break;
      case 'unit_destroyed':
        this.handleUnitDestroyed(action);
        break;
      case 'building_placed':
        this.handleBuildingPlaced(action);
        break;
      case 'building_destroyed':
        this.handleBuildingDestroyed(action);
        break;
      case 'resource_collected':
        this.handleResourceCollected(action);
        break;
      case 'resource_spent':
        this.handleResourceSpent(action);
        break;
    }
  }

  private handleUnitCreated(action: GameAction): void {
    const unit: UnitState = {
      id: action.parameters.unitId || '',
      owner: action.playerId,
      type: action.parameters.unitType || '',
      position: {
        x: action.target?.x || 0,
        y: action.target?.y || 0,
        z: action.target?.z || 0
      },
      health: 100,
      maxHealth: 100,
      experience: 0,
      level: 1,
      status: {
        moving: false,
        attacking: false,
        harvesting: false,
        repairing: false,
        constructing: false,
        idle: true,
        selected: false,
        loaded: false,
        garrisoned: false,
        stunned: false,
        cloak: false,
        ghost: false
      }
    };

    this.currentState.units.push(unit);
  }

  private handleUnitDestroyed(action: GameAction): void {
    const unitId = action.parameters.unitId || action.target?.id;
    if (!unitId) return;

    this.currentState.units = this.currentState.units.filter(u => u.id !== unitId);
  }

  private handleBuildingPlaced(action: GameAction): void {
    const building: BuildingState = {
      id: action.target?.id || '',
      owner: action.playerId,
      type: action.parameters.buildingType || '',
      position: {
        x: action.target?.x || 0,
        y: action.target?.y || 0,
        z: action.target?.z || 0
      },
      health: 100,
      maxHealth: 100,
      active: false,
      powered: true,
      constructing: true,
      constructionPercent: 0,
      upgradeLevel: 1,
      isFogged: false,
      isHidden: false
    };

    this.currentState.buildings.push(building);
  }

  private handleBuildingDestroyed(action: GameAction): void {
    const buildingId = action.target?.id;
    if (!buildingId) return;

    this.currentState.buildings = this.currentState.buildings.filter(b => b.id !== buildingId);
  }

  private handleResourceCollected(action: GameAction): void {
    const amount = action.parameters.amount || 0;
    this.currentState.resources.globalCredits += amount;
  }

  private handleResourceSpent(action: GameAction): void {
    const amount = action.parameters.amount || 0;
    this.currentState.resources.globalCredits -= amount;
  }

  private startPlaybackLoop(): void {
    const loop = () => {
      if (!this.isPlaying || this.isPaused) return;

      const currentTime = performance.now();
      const deltaTime = currentTime - this.lastFrameTime;

      if (deltaTime >= this.targetFrameTime / this.playbackSpeed) {
        this.advanceFrame();
        this.lastFrameTime = currentTime;
      }

      this.animationFrameId = requestAnimationFrame(loop);
    };

    this.animationFrameId = requestAnimationFrame(loop);
  }

  private advanceFrame(): void {
    if (this.currentFrameIndex >= this.replay.frames.length) {
      if (this.loop) {
        this.seek(0);
        this.play();
      } else {
        this.stop();
        this.emit('ended', {});
      }
      return;
    }

    const frame = this.replay.frames[this.currentFrameIndex];
    if (!frame) return;

    this.currentState = frame.state;

    while (this.currentEventIndex < this.replay.events.length) {
      const event = this.replay.events[this.currentEventIndex];
      if (event.gameTime > frame.gameTime) break;

      if (this.shouldEmitEvent(event)) {
        this.emit('event', event);
      }

      this.currentEventIndex++;
    }

    this.position = this.currentFrameIndex / this.replay.frames.length;
    this.currentFrameIndex++;

    this.emit('frame', {
      frameIndex: this.currentFrameIndex,
      state: this.currentState,
      position: this.position
    });
  }

  private shouldEmitEvent(event: ReplayEvent): boolean {
    if (this.filteredEventTypes.size > 0 && !this.filteredEventTypes.has(event.type)) {
      return false;
    }

    if (this.visibilityFilter.size > 0 && event.data) {
      const eventData = event.data as Record<string, unknown>;
      if (eventData.playerId && !this.visibilityFilter.has(eventData.playerId as string)) {
        return false;
      }
    }

    return true;
  }

  setPlaybackSpeed(speed: number): void {
    this.playbackSpeed = Math.max(0.1, Math.min(10, speed));
    this.emit('speed_change', { speed: this.playbackSpeed });
  }

  getPlaybackSpeed(): number {
    return this.playbackSpeed;
  }

  setPosition(position: number): void {
    this.seek(position);
  }

  getPosition(): number {
    return this.position;
  }

  setVolume(volume: number): void {
    this.volume = Math.max(0, Math.min(1, volume));
    this.muted = this.volume === 0;
    this.emit('volume_change', { volume: this.volume, muted: this.muted });
  }

  getVolume(): number {
    return this.volume;
  }

  setMuted(muted: boolean): void {
    this.muted = muted;
    this.emit('volume_change', { volume: this.volume, muted: this.muted });
  }

  isMuted(): boolean {
    return this.muted;
  }

  setLoop(loop: boolean): void {
    this.loop = loop;
    this.emit('loop_change', { loop: this.loop });
  }

  isLooping(): boolean {
    return this.loop;
  }

  setFilterEventTypes(types: string[]): void {
    this.filteredEventTypes = new Set(types);
  }

  addFilterEventType(type: string): void {
    this.filteredEventTypes.add(type);
  }

  removeFilterEventType(type: string): void {
    this.filteredEventTypes.delete(type);
  }

  clearEventFilters(): void {
    this.filteredEventTypes.clear();
  }

  setVisibilityFilter(playerIds: string[]): void {
    this.visibilityFilter = new Set(playerIds);
  }

  addToVisibilityFilter(playerId: string): void {
    this.visibilityFilter.add(playerId);
  }

  removeFromVisibilityFilter(playerId: string): void {
    this.visibilityFilter.delete(playerId);
  }

  clearVisibilityFilter(): void {
    this.visibilityFilter.clear();
  }

  skipToFrame(frameIndex: number): void {
    const position = frameIndex / this.replay.frames.length;
    this.seek(position);
  }

  skipToBookmark(bookmark: ReplayBookmark): void {
    this.seek(bookmark.frame / this.replay.frames.length);
  }

  skipForward(frames: number = 1): void {
    const newFrame = Math.min(
      this.currentFrameIndex + frames,
      this.replay.frames.length - 1
    );
    this.skipToFrame(newFrame);
  }

  skipBackward(frames: number = 1): void {
    const newFrame = Math.max(this.currentFrameIndex - frames, 0);
    this.skipToFrame(newFrame);
  }

  skipToNextEvent(): void {
    if (this.currentEventIndex < this.replay.events.length - 1) {
      this.currentEventIndex++;
      const event = this.replay.events[this.currentEventIndex];
      const frameIndex = this.replay.frames.findIndex(
        f => f.gameTime >= event.gameTime
      );
      if (frameIndex >= 0) {
        this.skipToFrame(frameIndex);
      }
    }
  }

  skipToPreviousEvent(): void {
    if (this.currentEventIndex > 0) {
      this.currentEventIndex--;
      const event = this.replay.events[this.currentEventIndex];
      const frameIndex = this.replay.frames.findIndex(
        f => f.gameTime >= event.gameTime
      );
      if (frameIndex >= 0) {
        this.skipToFrame(frameIndex);
      }
    }
  }

  getCurrentState(): GameState {
    return this.currentState;
  }

  getCurrentFrame(): ReplayFrame | null {
    return this.replay.frames[this.currentFrameIndex] || null;
  }

  getCurrentEvents(): ReplayEvent[] {
    const threshold = this.replay.frames[this.currentFrameIndex]?.gameTime || 0;
    return this.replay.events.filter(e => e.gameTime <= threshold);
  }

  getMetadata(): ReplayMetadata {
    return this.replay.metadata;
  }

  getDuration(): number {
    return this.replay.metadata.duration;
  }

  getTotalFrames(): number {
    return this.replay.frames.length;
  }

  getCurrentFrameIndex(): number {
    return this.currentFrameIndex;
  }

  isPlayingState(): boolean {
    return this.isPlaying;
  }

  isPausedState(): boolean {
    return this.isPaused;
  }

  getBookmarkAtPosition(position: number): ReplayBookmark | null {
    const frameIndex = Math.floor(position * this.replay.frames.length);
    return this.replay.events.find(
      e => e.type === 'bookmark' && e.gameTime >= this.replay.frames[frameIndex]?.gameTime
    ) as unknown as ReplayBookmark || null;
  }

  getMarkersAtPosition(position: number): ReplayMarker[] {
    const frameIndex = Math.floor(position * this.replay.frames.length);
    const frame = this.replay.frames[frameIndex];
    if (!frame) return [];

    return this.replay.events
      .filter(e => e.type === 'marker' && e.gameTime >= frame.gameTime)
      .map(e => e.data as ReplayMarker);
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  getReplayInfo(): ReplayInfo {
    return {
      id: this.replay.id,
      gameVersion: this.replay.metadata.gameVersion,
      gameMode: this.replay.metadata.gameMode,
      mapName: this.replay.metadata.mapName,
      duration: this.replay.metadata.duration,
      timestamp: this.replay.metadata.timestamp,
      players: this.replay.metadata.players,
      winner: this.replay.metadata.winner,
      totalFrames: this.replay.frames.length,
      totalEvents: this.replay.events.length,
      totalActions: this.replay.frames.reduce((sum, f) => sum + f.actions.length, 0),
      checksum: this.replay.footer.checksum,
      valid: this.validateChecksum()
    };
  }

  validateChecksum(): boolean {
    return this.replay.checksums.every(c => c.verified);
  }

  exportClip(startPosition: number, endPosition: number): ReplayFile {
    const startFrame = Math.floor(startPosition * this.replay.frames.length);
    const endFrame = Math.floor(endPosition * this.replay.frames.length);

    const startTime = this.replay.frames[startFrame]?.gameTime || 0;
    const endTime = this.replay.frames[endFrame]?.gameTime || this.replay.metadata.duration;

    const frames = this.replay.frames.slice(startFrame, endFrame + 1);
    const events = this.replay.events.filter(
      e => e.gameTime >= startTime && e.gameTime <= endTime
    );

    const clip: ReplayFile = {
      ...this.replay,
      id: `clip_${this.replay.id}_${Date.now()}`,
      frames,
      events,
      footer: {
        ...this.replay.footer,
        totalFrames: frames.length,
        totalEvents: events.length,
        duration: endTime - startTime
      }
    };

    return clip;
  }

  getStatistics(): ReplayStatistics {
    const unitCreatedCount = this.replay.events.filter(e => e.type === 'unit_created').length;
    const unitDestroyedCount = this.replay.events.filter(e => e.type === 'unit_destroyed').length;
    const buildingPlacedCount = this.replay.events.filter(e => e.type === 'building_placed').length;
    const buildingDestroyedCount = this.replay.events.filter(e => e.type === 'building_destroyed').length;

    const playerStats: Record<string, PlayerReplayStats> = {};

    this.replay.metadata.players.forEach(player => {
      playerStats[player.id] = {
        playerId: player.id,
        playerName: player.name,
        actions: 0,
        unitsProduced: 0,
        unitsDestroyed: 0,
        buildingsPlaced: 0,
        buildingsDestroyed: 0,
        resourcesCollected: 0,
        resourcesSpent: 0,
        chatMessages: 0
      };
    });

    this.replay.events.forEach(event => {
      if (event.type === 'chat_message' && event.data) {
        const data = event.data as Record<string, unknown>;
        if (data.playerId && playerStats[data.playerId as string]) {
          playerStats[data.playerId as string].chatMessages++;
        }
      }
    });

    return {
      totalFrames: this.replay.frames.length,
      totalEvents: this.replay.events.length,
      totalActions: this.replay.frames.reduce((sum, f) => sum + f.actions.length, 0),
      unitCreatedCount,
      unitDestroyedCount,
      buildingPlacedCount,
      buildingDestroyedCount,
      playerStats,
      averageActionsPerFrame: this.replay.frames.reduce((sum, f) => sum + f.actions.length, 0) / this.replay.frames.length,
      peakActionsInFrame: Math.max(...this.replay.frames.map(f => f.actions.length))
    };
  }

  compare(otherReplay: ReplayFile | string): ReplayComparison {
    let other: ReplayFile;

    if (typeof otherReplay === 'string') {
      other = JSON.parse(otherReplay);
    } else {
      other = otherReplay;
    }

    const comparison: ReplayComparison = {
      replay1: this.replay.id,
      replay2: other.id,
      durationDifference: this.replay.metadata.duration - other.metadata.duration,
      frameDifference: this.replay.frames.length - other.frames.length,
      eventDifference: this.replay.events.length - other.events.length,
      mapMatch: this.replay.metadata.mapId === other.metadata.mapId,
      modeMatch: this.replay.metadata.gameMode === other.metadata.gameMode,
      versionMatch: this.replay.metadata.gameVersion === other.metadata.gameVersion,
      playerCountDifference: this.replay.metadata.players.length - other.metadata.players.length,
      winnerMatch: this.replay.metadata.winner === other.metadata.winner,
      similarities: [],
      differences: []
    };

    if (comparison.durationDifference === 0) {
      comparison.similarities.push('Same duration');
    } else {
      comparison.differences.push(
        `Duration differs by ${Math.abs(comparison.durationDifference / 1000).toFixed(1)}s`
      );
    }

    if (comparison.mapMatch) {
      comparison.similarities.push('Same map');
    } else {
      comparison.differences.push('Different maps');
    }

    return comparison;
  }
}

export interface ReplayInfo {
  id: string;
  gameVersion: string;
  gameMode: GameMode;
  mapName: string;
  duration: number;
  timestamp: number;
  players: PlayerInfo[];
  winner: number;
  totalFrames: number;
  totalEvents: number;
  totalActions: number;
  checksum: string;
  valid: boolean;
}

export interface ReplayStatistics {
  totalFrames: number;
  totalEvents: number;
  totalActions: number;
  unitCreatedCount: number;
  unitDestroyedCount: number;
  buildingPlacedCount: number;
  buildingDestroyedCount: number;
  playerStats: Record<string, PlayerReplayStats>;
  averageActionsPerFrame: number;
  peakActionsInFrame: number;
}

export interface PlayerReplayStats {
  playerId: string;
  playerName: string;
  actions: number;
  unitsProduced: number;
  unitsDestroyed: number;
  buildingsPlaced: number;
  buildingsDestroyed: number;
  resourcesCollected: number;
  resourcesSpent: number;
  chatMessages: number;
}

export interface ReplayComparison {
  replay1: string;
  replay2: string;
  durationDifference: number;
  frameDifference: number;
  eventDifference: number;
  mapMatch: boolean;
  modeMatch: boolean;
  versionMatch: boolean;
  playerCountDifference: number;
  winnerMatch: boolean;
  similarities: string[];
  differences: string[];
}

export class ReplayManager {
  private replays: Map<string, ReplayFile> = new Map();
  private maxStoredReplays: number = 100;
  private storage: ReplayStorageAdapter;
  private listeners: Map<string, Set<Function>> = new Map();

  constructor() {
    this.storage = new LocalStorageReplayAdapter();
    this.loadStoredReplays();
  }

  private loadStoredReplays(): void {
    const stored = this.storage.getAll();
    stored.forEach((data, id) => {
      try {
        const replay = JSON.parse(data);
        this.replays.set(id, replay);
      } catch (error) {
        console.error(`Failed to load replay ${id}:`, error);
      }
    });
  }

  saveReplay(replay: ReplayFile): boolean {
    try {
      this.replays.set(replay.id, replay);
      this.storage.set(replay.id, JSON.stringify(replay));
      this.cleanupOldReplays();
      this.emit('replay_saved', { replayId: replay.id });
      return true;
    } catch (error) {
      console.error('Failed to save replay:', error);
      return false;
    }
  }

  loadReplay(replayId: string): ReplayFile | null {
    return this.replays.get(replayId) || null;
  }

  deleteReplay(replayId: string): boolean {
    if (this.replays.has(replayId)) {
      this.replays.delete(replayId);
      this.storage.remove(replayId);
      this.emit('replay_deleted', { replayId });
      return true;
    }
    return false;
  }

  getAllReplays(): ReplayFile[] {
    return Array.from(this.replays.values()).sort(
      (a, b) => b.metadata.timestamp - a.metadata.timestamp
    );
  }

  getReplaysByMode(mode: GameMode): ReplayFile[] {
    return this.getAllReplays().filter(r => r.metadata.gameMode === mode);
  }

  getReplaysByPlayer(playerName: string): ReplayFile[] {
    return this.getAllReplays().filter(r =>
      r.metadata.players.some(p => p.name.toLowerCase().includes(playerName.toLowerCase()))
    );
  }

  getReplaysByMap(mapId: string): ReplayFile[] {
    return this.getAllReplays().filter(r => r.metadata.mapId === mapId);
  }

  getRecentReplays(count: number = 10): ReplayFile[] {
    return this.getAllReplays().slice(0, count);
  }

  private cleanupOldReplays(): void {
    if (this.replays.size <= this.maxStoredReplays) return;

    const sorted = this.getAllReplays();
    const toDelete = sorted.slice(this.maxStoredReplays);

    toDelete.forEach(replay => {
      this.deleteReplay(replay.id);
    });
  }

  exportReplay(replayId: string, format: 'json' | 'binary' = 'json'): string | ArrayBuffer {
    const replay = this.replays.get(replayId);
    if (!replay) return '';

    if (format === 'json') {
      return JSON.stringify(replay);
    }

    return this.convertToBinary(replay);
  }

  private convertToBinary(replay: ReplayFile): ArrayBuffer {
    const json = JSON.stringify(replay);
    const encoder = new TextEncoder();
    return encoder.encode(json).buffer;
  }

  importReplay(data: string | ArrayBuffer): ReplayFile | null {
    try {
      let replay: ReplayFile;

      if (data instanceof ArrayBuffer) {
        const decoder = new TextDecoder();
        replay = JSON.parse(decoder.decode(data));
      } else {
        replay = JSON.parse(data);
      }

      this.saveReplay(replay);
      return replay;
    } catch (error) {
      console.error('Failed to import replay:', error);
      return null;
    }
  }

  createPlayer(replayId: string): ReplayPlayer | null {
    const replay = this.replays.get(replayId);
    if (!replay) return null;

    return new ReplayPlayer(replay);
  }

  createRecorder(metadata: ReplayMetadata): ReplayRecorder {
    return new ReplayRecorder(metadata);
  }

  getReplayStorageSize(): number {
    let totalSize = 0;

    this.replays.forEach(replay => {
      const json = JSON.stringify(replay);
      totalSize += new Blob([json]).size;
    });

    return totalSize;
  }

  clearAllReplays(): void {
    this.replays.clear();
    this.storage.clear();
    this.emit('all_replays_cleared', {});
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }
}

class ReplayStorageAdapter {
  private prefix: string = 'replay_';

  get(id: string): string | null {
    return localStorage.getItem(this.prefix + id);
  }

  set(id: string, data: string): void {
    localStorage.setItem(this.prefix + id, data);
  }

  remove(id: string): void {
    localStorage.removeItem(this.prefix + id);
  }

  getAll(): Map<string, string> {
    const result = new Map<string, string>();

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key?.startsWith(this.prefix)) {
        const value = localStorage.getItem(key);
        if (value) {
          result.set(key.replace(this.prefix, ''), value);
        }
      }
    }

    return result;
  }

  clear(): void {
    const keysToRemove: string[] = [];

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key?.startsWith(this.prefix)) {
        keysToRemove.push(key);
      }
    }

    keysToRemove.forEach(key => localStorage.removeItem(key));
  }
}

class LocalStorageReplayAdapter extends ReplayStorageAdapter {}

export const replayManager = new ReplayManager();

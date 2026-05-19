export interface GameSession {
  id: string;
  name: string;
  type: SessionType;
  state: SessionState;
  host: Player;
  players: Player[];
  spectators: Player[];
  settings: SessionSettings;
  map: MapInfo;
  teams: Team[];
  rules: GameRules;
  chat: ChatMessage[];
  startTime?: number;
  endTime?: number;
  duration: number;
  result?: SessionResult;
  stats: SessionStatistics;
}

export type SessionType =
  | 'quick_match'
  | 'custom_lobby'
  | 'ranked'
  | 'private'
  | 'tournament'
  | 'cooperative'
  | 'campaign';

export type SessionState =
  | 'lobby'
  | 'loading'
  | 'in_progress'
  | 'paused'
  | 'ending'
  | 'ended';

export interface Player {
  id: string;
  name: string;
  avatar?: string;
  level: number;
  rank: PlayerRank;
  faction?: string;
  team: number;
  ready: boolean;
  isHost: boolean;
  isAI: boolean;
  isSpectator: boolean;
  isConnected: boolean;
  ping: number;
  country?: string;
  joinedAt: number;
  lastActivity: number;
  stats: PlayerSessionStats;
}

export interface PlayerRank {
  rating: number;
  division: string;
  league: string;
  stars: number;
  points: number;
}

export interface PlayerSessionStats {
  unitsProduced: number;
  unitsLost: number;
  enemiesKilled: number;
  buildingsBuilt: number;
  buildingsLost: number;
  creditsEarned: number;
  damageDealt: number;
  score: number;
}

export interface SessionSettings {
  maxPlayers: number;
  maxSpectators: number;
  allowSpectators: boolean;
  private: boolean;
  password?: string;
  friendlyFire: boolean;
  mapRevealed: boolean;
  fogOfWar: boolean;
  teamsLocked: boolean;
  autoBalance: boolean;
  gameSpeed: GameSpeed;
  startingCredits: number;
  resourceMode: ResourceMode;
  crateSpawn: boolean;
  shortGame: boolean;
  superweapons: boolean;
  voteEnabled: boolean;
  voteTimeout: number;
  autoKickIdle: boolean;
  idleTimeout: number;
}

export type GameSpeed = 'slow' | 'normal' | 'fast' | 'very_fast';
export type ResourceMode = 'standard' | 'megarich' | 'multiplier';

export interface MapInfo {
  id: string;
  name: string;
  description: string;
  author: string;
  thumbnail: string;
  width: number;
  height: number;
  maxPlayers: number;
  recommendedPlayers: number;
  difficulty: MapDifficulty;
  terrain: string[];
  features: string[];
  rating: number;
  plays: number;
  likes: number;
  hash: string;
  size: number;
}

export type MapDifficulty = 'easy' | 'medium' | 'hard' | 'expert';

export interface Team {
  id: number;
  name: string;
  color: string;
  players: string[];
  allies: number[];
  score: number;
  eliminated: boolean;
}

export interface GameRules {
  victoryCondition: VictoryCondition;
  timeLimit?: number;
  scoreLimit?: number;
  deathLimit?: number;
  handicap: Handicaps;
  allowCheats: boolean;
  dedicatedServer: boolean;
}

export type VictoryCondition =
  | 'annihilation'
  | 'supremacy'
  | 'time_limit'
  | 'capture_all_tech'
  | 'destroy_structures'
  | 'collect_ore'
  | 'survival'
  | 'king_of_hill';

export interface Handicaps {
  enabled: boolean;
  startingCredits?: number;
  incomeMultiplier?: number;
  buildSpeed?: number;
  researchSpeed?: number;
}

export interface ChatMessage {
  id: string;
  type: ChatType;
  sender?: string;
  senderName?: string;
  content: string;
  timestamp: number;
  team?: number;
  recipient?: string;
  deleted: boolean;
}

export type ChatType =
  | 'global'
  | 'team'
  | 'private'
  | 'system'
  | 'announcement';

export interface SessionResult {
  winner: number;
  reason: VictoryCondition;
  scores: PlayerScore[];
  duration: number;
  replayId?: string;
}

export interface PlayerScore {
  playerId: string;
  rank: number;
  score: number;
  stats: ScoreBreakdown;
}

export interface ScoreBreakdown {
  combat: number;
  economy: number;
  technology: number;
  structures: number;
  objectives: number;
  penalties: number;
}

export interface SessionStatistics {
  totalFrames: number;
  peakPlayers: number;
  avgPing: number;
  desyncs: number;
  disconnections: number;
  kicks: number;
  votes: VoteStatistics;
}

export interface VoteStatistics {
  total: number;
  passed: number;
  failed: number;
  current?: ActiveVote;
}

export interface ActiveVote {
  type: VoteType;
  initiator: string;
  target?: string;
  votesFor: string[];
  votesAgainst: string[];
  required: number;
  startTime: number;
  expiresAt: number;
  result?: 'passed' | 'failed' | 'expired';
}

export type VoteType =
  | 'kick'
  | 'surrender'
  | 'pause'
  | 'resume'
  | 'map_change'
  | 'settings_change'
  | 'rematch';

export interface MatchmakingQueue {
  id: string;
  type: QueueType;
  region: Region;
  players: MatchmakingPlayer[];
  status: QueueStatus;
  startTime: number;
  estimatedTime: number;
  targetTime: number;
  settings: MatchmakingSettings;
}

export type QueueType =
  | 'quick_match'
  | 'ranked_solo'
  | 'ranked_team'
  | 'co_op'
  | 'custom';

export type Region =
  | 'na'
  | 'eu'
  | 'asia'
  | 'oce'
  | 'sa'
  | 'af'
  | 'me';

export type QueueStatus =
  | 'idle'
  | 'searching'
  | 'evaluating'
  | 'forming'
  | 'ready'
  | 'cancelled';

export interface MatchmakingPlayer {
  id: string;
  name: string;
  rank: PlayerRank;
  ping: number;
  region: Region;
  preferences: PlayerPreferences;
  joinedAt: number;
}

export interface PlayerPreferences {
  factions: string[];
  maps: string[];
  avoidTeammates: string[];
  preferTeammates: string[];
}

export interface MatchmakingSettings {
  minRank?: number;
  maxRank?: number;
  rankDifference?: number;
  sameRegion?: boolean;
  fairMatch?: boolean;
  allowParties?: boolean;
  partySize?: number;
  maxQueueTime?: number;
}

export interface GameServer {
  id: string;
  name: string;
  region: Region;
  address: string;
  port: number;
  load: ServerLoad;
  players: number;
  maxPlayers: number;
  gamemodes: string[];
  version: string;
  pingTest?: number;
  status: ServerStatus;
  uptime: number;
  regionData?: RegionLatency;
}

export type ServerLoad =
  | 'empty'
  | 'low'
  | 'medium'
  | 'high'
  | 'full';

export type ServerStatus =
  | 'online'
  | 'offline'
  | 'maintenance'
  | 'updating';

export interface RegionLatency {
  region: Region;
  ping: number;
  jitter: number;
  packetLoss: number;
}

export interface Party {
  id: string;
  leader: string;
  members: PartyMember[];
  invites: PartyInvite[];
  settings: PartySettings;
  createdAt: number;
  status: PartyStatus;
}

export interface PartyMember {
  id: string;
  name: string;
  rank: PlayerRank;
  ready: boolean;
  isLeader: boolean;
}

export interface PartyInvite {
  id: string;
  player: { id: string; name: string };
  inviter: string;
  expiresAt: number;
  status: InviteStatus;
}

export type InviteStatus =
  | 'pending'
  | 'accepted'
  | 'declined'
  | 'expired';

export interface PartySettings {
  open: boolean;
  minLevel?: number;
  maxSize: number;
  friendsOnly: boolean;
  regionLock?: Region;
}

export type PartyStatus =
  | 'idle'
  | 'searching'
  | 'in_lobby'
  | 'in_game';

export interface Tournament {
  id: string;
  name: string;
  description: string;
  banner: string;
  organizer: string;
  sponsor?: string;
  prize?: PrizePool;
  format: TournamentFormat;
  rules: TournamentRules;
  schedule: TournamentSchedule;
  registration: RegistrationInfo;
  participants: TournamentParticipant[];
  matches: TournamentMatch[];
  status: TournamentStatus;
  settings: TournamentSettings;
  stream?: StreamInfo;
}

export type TournamentFormat =
  | 'single_elimination'
  | 'double_elimination'
  | 'round_robin'
  | 'swiss'
  | 'league';

export interface TournamentRules {
  gameMode: string;
  mapPool: string[];
  mapSelection: 'veto' | 'pick' | 'random';
  gameSettings: Partial<SessionSettings>;
  replaysRequired: boolean;
  pauseAllowed: boolean;
  maxPauseTime?: number;
  antiCheat: boolean;
  coaching: boolean;
  streamingDelay?: number;
}

export interface TournamentSchedule {
  registrationStart: number;
  registrationEnd: number;
  startTime: number;
  endTime: number;
  phases: TournamentPhase[];
}

export interface TournamentPhase {
  name: string;
  startTime: number;
  endTime: number;
  format: TournamentFormat;
  matches: string[];
}

export interface RegistrationInfo {
  open: boolean;
  fee?: number;
  currency?: string;
  minTeamSize: number;
  maxTeamSize: number;
  regions: Region[];
  verificationRequired: boolean;
}

export interface TournamentParticipant {
  id: string;
  type: 'player' | 'team';
  name: string;
  players: string[];
  seed?: number;
  rank?: number;
  wins: number;
  losses: number;
  status: ParticipantStatus;
  checkedIn: boolean;
  checkInTime?: number;
  prizeWon?: number;
}

export type ParticipantStatus =
  | 'registered'
  | 'checked_in'
  | 'playing'
  | 'eliminated'
  | 'withdrawn'
  | 'disqualified';

export interface TournamentMatch {
  id: string;
  round: number;
  bracketPosition: number;
  phase: string;
  format: 'bo1' | 'bo3' | 'bo5' | 'bo7';
  games: GameResult[];
  players: [string, string];
  winner?: string;
  status: MatchStatus;
  scheduledTime?: number;
  actualTime?: number;
  stream?: StreamInfo;
}

export type MatchStatus =
  | 'scheduled'
  | 'ready'
  | 'in_progress'
  | 'completed'
  | 'cancelled'
  | 'postponed';

export interface GameResult {
  gameNumber: number;
  map: string;
  players: GamePlayerResult[];
  winner?: string;
  duration: number;
  replayId?: string;
}

export interface GamePlayerResult {
  playerId: string;
  result: 'win' | 'loss' | 'draw';
  score: number;
}

export interface PrizePool {
  total: number;
  currency: string;
  distribution: PrizeDistribution[];
}

export interface PrizeDistribution {
  position: number;
  amount: number;
  percentage: number;
}

export type TournamentStatus =
  | 'upcoming'
  | 'registration'
  | 'check_in'
  | 'in_progress'
  | 'completed'
  | 'cancelled';

export interface TournamentSettings {
  public: boolean;
  ranked: boolean;
  allowSpectators: boolean;
  chatEnabled: boolean;
  autoAdvance: boolean;
  pauseBetweenMatches: number;
}

export interface StreamInfo {
  platform: 'twitch' | 'youtube' | 'mixer' | 'other';
  channel: string;
  url: string;
  viewerCount: number;
  live: boolean;
  delay?: number;
}

export class MultiplayerSessionManager {
  private sessions: Map<string, GameSession> = new Map();
  private activeSession?: GameSession;
  private matchmakingQueues: Map<QueueType, MatchmakingQueue> = new Map();
  private parties: Map<string, Party> = new Map();
  private servers: GameServer[] = [];
  private tournaments: Map<string, Tournament> = new Map();
  private chatHistory: Map<string, ChatMessage[]> = new Map();
  private connection: NetworkConnection;
  private sync: SessionSynchronizer;
  private listeners: Map<string, Set<Function>> = new Map();
  private reconnectAttempts: number = 0;
  private maxReconnectAttempts: number = 5;
  private heartbeatInterval?: NodeJS.Timeout;
  private lastHeartbeat: number = 0;

  constructor() {
    this.connection = new WebSocketConnection();
    this.sync = new SessionSynchronizer();

    this.setupConnectionHandlers();
    this.startHeartbeat();
  }

  private setupConnectionHandlers(): void {
    this.connection.on('connect', () => {
      this.reconnectAttempts = 0;
      this.emit('connected', {});
      this.resyncSession();
    });

    this.connection.on('disconnect', () => {
      this.emit('disconnected', {});
      this.attemptReconnect();
    });

    this.connection.on('error', (error) => {
      this.emit('error', { error });
    });

    this.connection.on('message', (message) => {
      this.handleMessage(message);
    });
  }

  private startHeartbeat(): void {
    this.heartbeatInterval = setInterval(() => {
      if (this.connection.isConnected()) {
        this.connection.send({
          type: 'heartbeat',
          timestamp: Date.now()
        });
        this.lastHeartbeat = Date.now();
      }
    }, 30000);
  }

  private attemptReconnect(): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      this.emit('reconnect_failed', { attempts: this.reconnectAttempts });
      return;
    }

    this.reconnectAttempts++;
    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);

    setTimeout(() => {
      this.connection.connect();
    }, delay);
  }

  private handleMessage(message: NetworkMessage): void {
    switch (message.type) {
      case 'player_joined':
        this.handlePlayerJoined(message.data as Player);
        break;
      case 'player_left':
        this.handlePlayerLeft(message.data as { playerId: string; reason: string });
        break;
      case 'player_ready':
        this.handlePlayerReady(message.data as { playerId: string; ready: boolean });
        break;
      case 'chat_message':
        this.handleChatMessage(message.data as ChatMessage);
        break;
      case 'settings_changed':
        this.handleSettingsChanged(message.data as Partial<SessionSettings>);
        break;
      case 'game_starting':
        this.handleGameStarting(message.data as { countdown: number });
        break;
      case 'game_started':
        this.handleGameStarted(message.data as { startTime: number });
        break;
      case 'game_ended':
        this.handleGameEnded(message.data as SessionResult);
        break;
      case 'vote_started':
        this.handleVoteStarted(message.data as ActiveVote);
        break;
      case 'vote_cast':
        this.handleVoteCast(message.data as { playerId: string; vote: boolean });
        break;
      case 'vote_ended':
        this.handleVoteEnded(message.data as { result: 'passed' | 'failed'; vote: ActiveVote });
        break;
      case 'sync_state':
        this.sync.receiveSync(message.data as SessionSyncState);
        break;
      case 'player_ping':
        this.handlePlayerPing(message.data as { playerId: string; ping: number });
        break;
    }
  }

  createSession(settings: CreateSessionOptions): GameSession {
    const session: GameSession = {
      id: this.generateSessionId(),
      name: settings.name || `Game ${Date.now()}`,
      type: settings.type || 'custom_lobby',
      state: 'lobby',
      host: settings.host || this.createLocalPlayer(true),
      players: [settings.host || this.createLocalPlayer(true)],
      spectators: [],
      settings: settings.settings || this.getDefaultSettings(),
      map: settings.map || this.getDefaultMap(),
      teams: this.createTeams(settings.settings?.maxPlayers || 8),
      rules: settings.rules || this.getDefaultRules(),
      chat: [],
      duration: 0,
      stats: {
        totalFrames: 0,
        peakPlayers: 0,
        avgPing: 0,
        desyncs: 0,
        disconnections: 0,
        kicks: 0,
        votes: { total: 0, passed: 0, failed: 0 }
      }
    };

    this.sessions.set(session.id, session);
    this.activeSession = session;

    this.connection.send({
      type: 'session_create',
      session
    });

    this.emit('session_created', { session });
    return session;
  }

  private createLocalPlayer(isHost: boolean): Player {
    return {
      id: `local_${Date.now()}`,
      name: 'Commander',
      level: 1,
      rank: { rating: 1000, division: 'Bronze', league: 'V', stars: 0, points: 0 },
      team: 1,
      ready: false,
      isHost,
      isAI: false,
      isSpectator: false,
      isConnected: true,
      ping: 0,
      joinedAt: Date.now(),
      lastActivity: Date.now(),
      stats: {
        unitsProduced: 0,
        unitsLost: 0,
        enemiesKilled: 0,
        buildingsBuilt: 0,
        buildingsLost: 0,
        creditsEarned: 0,
        damageDealt: 0,
        score: 0
      }
    };
  }

  private getDefaultSettings(): SessionSettings {
    return {
      maxPlayers: 8,
      maxSpectators: 10,
      allowSpectators: true,
      private: false,
      friendlyFire: false,
      mapRevealed: false,
      fogOfWar: true,
      teamsLocked: false,
      autoBalance: true,
      gameSpeed: 'normal',
      startingCredits: 2000,
      resourceMode: 'standard',
      crateSpawn: true,
      shortGame: false,
      superweapons: true,
      voteEnabled: true,
      voteTimeout: 60,
      autoKickIdle: false,
      idleTimeout: 300
    };
  }

  private getDefaultMap(): MapInfo {
    return {
      id: 'default_map',
      name: 'Central Plains',
      description: 'A balanced map with moderate terrain',
      author: 'Red Alert Team',
      thumbnail: '',
      width: 100,
      height: 100,
      maxPlayers: 8,
      recommendedPlayers: 4,
      difficulty: 'medium',
      terrain: ['plains', 'hills'],
      features: ['ore', 'water'],
      rating: 4.5,
      plays: 10000,
      likes: 8000,
      hash: '',
      size: 5000000
    };
  }

  private createTeams(maxPlayers: number): Team[] {
    const teams: Team[] = [];
    const teamCount = Math.ceil(maxPlayers / 4);

    for (let i = 0; i < teamCount; i++) {
      teams.push({
        id: i + 1,
        name: `Team ${i + 1}`,
        color: this.getTeamColor(i),
        players: [],
        allies: [],
        score: 0,
        eliminated: false
      });
    }

    return teams;
  }

  private getTeamColor(index: number): string {
    const colors = ['#FF0000', '#0000FF', '#00FF00', '#FFFF00', '#FF00FF', '#00FFFF'];
    return colors[index % colors.length];
  }

  private getDefaultRules(): GameRules {
    return {
      victoryCondition: 'annihilation',
      handicap: { enabled: false },
      allowCheats: false,
      dedicatedServer: false
    };
  }

  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  joinSession(sessionId: string, password?: string): Promise<JoinResult> {
    return new Promise((resolve, reject) => {
      this.connection.send({
        type: 'session_join',
        sessionId,
        password
      });

      const timeout = setTimeout(() => {
        reject(new Error('Join request timed out'));
      }, 30000);

      const handler = (message: NetworkMessage) => {
        if (message.type === 'join_result') {
          clearTimeout(timeout);
          const result = message.data as JoinResult;
          if (result.success) {
            this.activeSession = this.sessions.get(sessionId);
            this.emit('joined_session', { sessionId });
          }
          resolve(result);
        }
      };

      this.connection.on('message', handler);
    });
  }

  leaveSession(): void {
    if (!this.activeSession) return;

    this.connection.send({
      type: 'session_leave',
      sessionId: this.activeSession.id
    });

    this.emit('left_session', { sessionId: this.activeSession.id });
    this.activeSession = undefined;
  }

  setPlayerReady(ready: boolean): void {
    if (!this.activeSession) return;

    const player = this.activeSession.players.find(p => !p.isSpectator);
    if (player) {
      player.ready = ready;
      player.lastActivity = Date.now();

      this.connection.send({
        type: 'player_ready',
        sessionId: this.activeSession.id,
        playerId: player.id,
        ready
      });

      this.checkAllReady();
      this.emit('player_ready_changed', { playerId: player.id, ready });
    }
  }

  private checkAllReady(): void {
    if (!this.activeSession) return;

    const nonHostPlayers = this.activeSession.players.filter(p => !p.isHost && !p.isSpectator && !p.isAI);
    const allReady = nonHostPlayers.every(p => p.ready) || nonHostPlayers.length === 0;

    this.emit('all_ready_changed', { allReady });
  }

  changeFaction(faction: string): void {
    if (!this.activeSession) return;

    const player = this.activeSession.players.find(p => !p.isSpectator);
    if (player) {
      player.faction = faction;
      player.lastActivity = Date.now();

      this.connection.send({
        type: 'faction_change',
        sessionId: this.activeSession.id,
        playerId: player.id,
        faction
      });

      this.emit('faction_changed', { playerId: player.id, faction });
    }
  }

  changeTeam(teamId: number): void {
    if (!this.activeSession) return;
    if (this.activeSession.settings.teamsLocked) return;

    const player = this.activeSession.players.find(p => !p.isSpectator);
    if (!player) return;

    const oldTeam = this.activeSession.teams.find(t => t.id === player.team);
    if (oldTeam) {
      oldTeam.players = oldTeam.players.filter(id => id !== player.id);
    }

    const newTeam = this.activeSession.teams.find(t => t.id === teamId);
    if (newTeam) {
      newTeam.players.push(player.id);
      player.team = teamId;
    }

    this.connection.send({
      type: 'team_change',
      sessionId: this.activeSession.id,
      playerId: player.id,
      teamId
    });

    this.emit('team_changed', { playerId: player.id, teamId });
  }

  setMap(map: MapInfo): void {
    if (!this.activeSession) return;
    if (!this.isHost()) return;

    this.activeSession.map = map;

    this.connection.send({
      type: 'map_change',
      sessionId: this.activeSession.id,
      map
    });

    this.emit('map_changed', { map });
  }

  updateSettings(settings: Partial<SessionSettings>): void {
    if (!this.activeSession) return;
    if (!this.isHost()) return;

    this.activeSession.settings = {
      ...this.activeSession.settings,
      ...settings
    };

    this.connection.send({
      type: 'settings_change',
      sessionId: this.activeSession.id,
      settings: this.activeSession.settings
    });

    this.emit('settings_changed', { settings: this.activeSession.settings });
  }

  private handleSettingsChanged(settings: Partial<SessionSettings>): void {
    if (!this.activeSession) return;

    this.activeSession.settings = {
      ...this.activeSession.settings,
      ...settings
    };

    this.emit('settings_changed', { settings: this.activeSession.settings });
  }

  kickPlayer(playerId: string, reason?: string): boolean {
    if (!this.activeSession) return false;
    if (!this.isHost()) return false;

    const player = this.activeSession.players.find(p => p.id === playerId);
    if (!player) return false;
    if (player.isHost) return false;

    player.isConnected = false;

    this.connection.send({
      type: 'player_kick',
      sessionId: this.activeSession.id,
      playerId,
      reason
    });

    this.activeSession.stats.kicks++;
    this.emit('player_kicked', { playerId, reason });

    return true;
  }

  banPlayer(playerId: string, duration?: number): boolean {
    if (!this.activeSession) return false;
    if (!this.isHost()) return false;

    this.connection.send({
      type: 'player_ban',
      sessionId: this.activeSession.id,
      playerId,
      duration
    });

    this.emit('player_banned', { playerId, duration });
    return true;
  }

  startGame(): Promise<void> {
    if (!this.activeSession) return Promise.reject(new Error('No active session'));
    if (!this.isHost()) return Promise.reject(new Error('Not host'));
    if (!this.canStartGame()) return Promise.reject(new Error('Cannot start game'));

    return new Promise((resolve, reject) => {
      this.connection.send({
        type: 'game_start',
        sessionId: this.activeSession!.id
      });

      const timeout = setTimeout(() => {
        reject(new Error('Game start timed out'));
      }, 60000);

      const handler = (message: NetworkMessage) => {
        if (message.type === 'game_started') {
          clearTimeout(timeout);
          this.handleGameStarted(message.data as { startTime: number });
          resolve();
        } else if (message.type === 'game_start_failed') {
          clearTimeout(timeout);
          reject(new Error('Game start failed'));
        }
      };

      this.connection.on('message', handler);
    });
  }

  private canStartGame(): boolean {
    if (!this.activeSession) return false;

    const minPlayers = 2;
    const connectedPlayers = this.activeSession.players.filter(p => !p.isSpectator && p.isConnected);

    if (connectedPlayers.length < minPlayers) return false;

    const nonHostReady = connectedPlayers.filter(p => !p.isHost).every(p => p.ready);
    if (!nonHostReady && connectedPlayers.length > 1) return false;

    return true;
  }

  private handleGameStarting(data: { countdown: number }): void {
    if (!this.activeSession) return;

    this.activeSession.state = 'loading';
    this.emit('game_starting', data);
  }

  private handleGameStarted(data: { startTime: number }): void {
    if (!this.activeSession) return;

    this.activeSession.state = 'in_progress';
    this.activeSession.startTime = data.startTime;

    this.emit('game_started', { startTime: data.startTime });
  }

  pauseGame(): void {
    if (!this.activeSession) return;
    if (this.activeSession.state !== 'in_progress') return;

    this.connection.send({
      type: 'game_pause',
      sessionId: this.activeSession.id
    });

    this.activeSession.state = 'paused';
    this.emit('game_paused', {});
  }

  resumeGame(): void {
    if (!this.activeSession) return;
    if (this.activeSession.state !== 'paused') return;

    this.connection.send({
      type: 'game_resume',
      sessionId: this.activeSession.id
    });

    this.activeSession.state = 'in_progress';
    this.emit('game_resumed', {});
  }

  surrender(): void {
    if (!this.activeSession) return;
    if (this.activeSession.state !== 'in_progress') return;

    const player = this.activeSession.players.find(p => !p.isSpectator);
    if (!player) return;

    this.connection.send({
      type: 'player_surrender',
      sessionId: this.activeSession.id,
      playerId: player.id
    });

    this.emit('player_surrendered', { playerId: player.id });
  }

  endGame(result: SessionResult): void {
    if (!this.activeSession) return;

    this.activeSession.state = 'ended';
    this.activeSession.endTime = Date.now();
    this.activeSession.duration = this.activeSession.endTime - (this.activeSession.startTime || 0);
    this.activeSession.result = result;

    this.connection.send({
      type: 'game_end',
      sessionId: this.activeSession.id,
      result
    });

    this.emit('game_ended', { result });
  }

  private handleGameEnded(result: SessionResult): void {
    if (!this.activeSession) return;

    this.activeSession.state = 'ended';
    this.activeSession.endTime = Date.now();
    this.activeSession.duration = this.activeSession.endTime - (this.activeSession.startTime || 0);
    this.activeSession.result = result;

    this.emit('game_ended', { result });
  }

  sendChatMessage(content: string, type: ChatType = 'global', recipient?: string): ChatMessage {
    if (!this.activeSession) throw new Error('No active session');

    const player = this.activeSession.players.find(p => !p.isSpectator);
    if (!player) throw new Error('No player found');

    const message: ChatMessage = {
      id: `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      type,
      sender: player.id,
      senderName: player.name,
      content,
      timestamp: Date.now(),
      team: type === 'team' ? player.team : undefined,
      recipient,
      deleted: false
    };

    this.activeSession.chat.push(message);

    this.connection.send({
      type: 'chat_message',
      sessionId: this.activeSession.id,
      message
    });

    this.emit('chat_message', { message });
    return message;
  }

  private handleChatMessage(message: ChatMessage): void {
    if (!this.activeSession) return;

    const existing = this.activeSession.chat.find(m => m.id === message.id);
    if (!existing) {
      this.activeSession.chat.push(message);
    }

    this.emit('chat_message', { message });
  }

  private handlePlayerJoined(player: Player): void {
    if (!this.activeSession) return;

    const existing = this.activeSession.players.find(p => p.id === player.id);
    if (existing) {
      existing.isConnected = true;
    } else {
      this.activeSession.players.push(player);

      const team = this.activeSession.teams.find(t => t.id === player.team);
      if (team) {
        team.players.push(player.id);
      }
    }

    if (this.activeSession.players.length > this.activeSession.stats.peakPlayers) {
      this.activeSession.stats.peakPlayers = this.activeSession.players.length;
    }

    const systemMessage: ChatMessage = {
      id: `msg_${Date.now()}`,
      type: 'system',
      content: `${player.name} has joined the game`,
      timestamp: Date.now(),
      deleted: false
    };

    this.activeSession.chat.push(systemMessage);

    this.emit('player_joined', { player });
  }

  private handlePlayerLeft(data: { playerId: string; reason: string }): void {
    if (!this.activeSession) return;

    const player = this.activeSession.players.find(p => p.id === data.playerId);
    if (player) {
      player.isConnected = false;

      const systemMessage: ChatMessage = {
        id: `msg_${Date.now()}`,
        type: 'system',
        content: `${player.name} has left the game (${data.reason})`,
        timestamp: Date.now(),
        deleted: false
      };

      this.activeSession.chat.push(systemMessage);
    }

    this.activeSession.stats.disconnections++;

    this.emit('player_left', data);
  }

  private handlePlayerReady(data: { playerId: string; ready: boolean }): void {
    if (!this.activeSession) return;

    const player = this.activeSession.players.find(p => p.id === data.playerId);
    if (player) {
      player.ready = data.ready;
    }

    this.checkAllReady();
    this.emit('player_ready_changed', data);
  }

  private handlePlayerPing(data: { playerId: string; ping: number }): void {
    if (!this.activeSession) return;

    const player = this.activeSession.players.find(p => p.id === data.playerId);
    if (player) {
      player.ping = data.ping;
    }

    const totalPing = this.activeSession.players.reduce((sum, p) => sum + p.ping, 0);
    this.activeSession.stats.avgPing = totalPing / this.activeSession.players.length;
  }

  startVote(type: VoteType, target?: string): boolean {
    if (!this.activeSession) return false;
    if (!this.activeSession.settings.voteEnabled) return false;

    const player = this.activeSession.players.find(p => !p.isSpectator);
    if (!player) return false;

    const activeVote = this.activeSession.stats.votes.current;
    if (activeVote) return false;

    const required = Math.ceil(this.activeSession.players.filter(p => !p.isSpectator).length / 2);

    const vote: ActiveVote = {
      type,
      initiator: player.id,
      target,
      votesFor: [player.id],
      votesAgainst: [],
      required,
      startTime: Date.now(),
      expiresAt: Date.now() + (this.activeSession.settings.voteTimeout * 1000)
    };

    this.activeSession.stats.votes.current = vote;
    this.activeSession.stats.votes.total++;

    this.connection.send({
      type: 'vote_start',
      sessionId: this.activeSession.id,
      vote
    });

    this.emit('vote_started', { vote });
    return true;
  }

  castVote(voteFor: boolean): boolean {
    if (!this.activeSession) return false;

    const activeVote = this.activeSession.stats.votes.current;
    if (!activeVote) return false;

    const player = this.activeSession.players.find(p => !p.isSpectator);
    if (!player) return false;

    if (activeVote.votesFor.includes(player.id) || activeVote.votesAgainst.includes(player.id)) {
      return false;
    }

    if (voteFor) {
      activeVote.votesFor.push(player.id);
    } else {
      activeVote.votesAgainst.push(player.id);
    }

    this.connection.send({
      type: 'vote_cast',
      sessionId: this.activeSession.id,
      playerId: player.id,
      vote: voteFor
    });

    this.checkVoteResult();
    this.emit('vote_cast', { playerId: player.id, vote: voteFor });

    return true;
  }

  private handleVoteStarted(vote: ActiveVote): void {
    if (!this.activeSession) return;

    this.activeSession.stats.votes.current = vote;
    this.activeSession.stats.votes.total++;

    this.emit('vote_started', { vote });
  }

  private handleVoteCast(data: { playerId: string; vote: boolean }): void {
    if (!this.activeSession) return;

    const activeVote = this.activeSession.stats.votes.current;
    if (!activeVote) return;

    if (data.vote) {
      if (!activeVote.votesFor.includes(data.playerId)) {
        activeVote.votesFor.push(data.playerId);
      }
    } else {
      if (!activeVote.votesAgainst.includes(data.playerId)) {
        activeVote.votesAgainst.push(data.playerId);
      }
    }

    this.checkVoteResult();
  }

  private checkVoteResult(): void {
    if (!this.activeSession) return;

    const activeVote = this.activeSession.stats.votes.current;
    if (!activeVote) return;

    if (Date.now() > activeVote.expiresAt) {
      this.endVote();
    } else if (activeVote.votesFor.length >= activeVote.required) {
      activeVote.result = 'passed';
      this.executeVote(activeVote);
    } else if (activeVote.votesFor.length + activeVote.votesAgainst >=
               this.activeSession.players.filter(p => !p.isSpectator).length) {
      activeVote.result = 'failed';
    }
  }

  private endVote(): void {
    if (!this.activeSession) return;

    const activeVote = this.activeSession.stats.votes.current;
    if (!activeVote) return;

    if (!activeVote.result) {
      activeVote.result = 'failed';
    }

    if (activeVote.result === 'passed') {
      this.activeSession.stats.votes.passed++;
      this.executeVote(activeVote);
    } else {
      this.activeSession.stats.votes.failed++;
    }

    this.emit('vote_ended', { result: activeVote.result, vote: activeVote });
    this.activeSession.stats.votes.current = undefined;
  }

  private executeVote(vote: ActiveVote): void {
    if (!this.activeSession) return;

    switch (vote.type) {
      case 'kick':
        if (vote.target) {
          this.kickPlayer(vote.target, 'Vote kick');
        }
        break;
      case 'surrender':
        this.surrender();
        break;
      case 'pause':
        this.pauseGame();
        break;
      case 'resume':
        this.resumeGame();
        break;
    }
  }

  private handleVoteEnded(data: { result: 'passed' | 'failed'; vote: ActiveVote }): void {
    if (!this.activeSession) return;

    if (data.result === 'passed') {
      this.activeSession.stats.votes.passed++;
      this.executeVote(data.vote);
    } else {
      this.activeSession.stats.votes.failed++;
    }

    this.activeSession.stats.votes.current = undefined;
    this.emit('vote_ended', data);
  }

  isHost(): boolean {
    if (!this.activeSession) return false;

    const localPlayer = this.activeSession.players.find(p => !p.isSpectator);
    return localPlayer?.isHost || false;
  }

  getSession(): GameSession | undefined {
    return this.activeSession;
  }

  getPlayers(): Player[] {
    return this.activeSession?.players || [];
  }

  getTeams(): Team[] {
    return this.activeSession?.teams || [];
  }

  getChatHistory(): ChatMessage[] {
    return this.activeSession?.chat || [];
  }

  private resyncSession(): void {
    if (!this.activeSession) return;

    this.connection.send({
      type: 'session_sync',
      sessionId: this.activeSession.id
    });
  }

  createParty(settings?: Partial<PartySettings>): Party {
    const localPlayer = this.getLocalPlayer();
    if (!localPlayer) throw new Error('Not logged in');

    const party: Party = {
      id: `party_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      leader: localPlayer.id,
      members: [{
        id: localPlayer.id,
        name: localPlayer.name,
        rank: localPlayer.rank,
        ready: false,
        isLeader: true
      }],
      invites: [],
      settings: {
        open: settings?.open ?? false,
        minLevel: settings?.minLevel,
        maxSize: settings?.maxSize || 4,
        friendsOnly: settings?.friendsOnly ?? false,
        regionLock: settings?.regionLock
      },
      createdAt: Date.now(),
      status: 'idle'
    };

    this.parties.set(party.id, party);

    this.emit('party_created', { party });
    return party;
  }

  leaveParty(): void {
    const player = this.getLocalPlayer();
    if (!player) return;

    for (const [partyId, party] of this.parties) {
      const member = party.members.find(m => m.id === player.id);
      if (member) {
        party.members = party.members.filter(m => m.id !== player.id);

        if (party.leader === player.id && party.members.length > 0) {
          party.leader = party.members[0].id;
          party.members[0].isLeader = true;
        }

        if (party.members.length === 0) {
          this.parties.delete(partyId);
        }

        this.emit('party_left', { partyId });
        break;
      }
    }
  }

  inviteToParty(playerId: string): boolean {
    const party = this.getPlayerParty();
    if (!party) return false;
    if (party.members.length >= party.settings.maxSize) return false;

    const existing = party.members.find(m => m.id === playerId);
    if (existing) return false;

    const invite: PartyInvite = {
      id: `invite_${Date.now()}`,
      player: { id: playerId, name: '' },
      inviter: party.leader,
      expiresAt: Date.now() + 60000,
      status: 'pending'
    };

    party.invites.push(invite);

    this.connection.send({
      type: 'party_invite',
      partyId: party.id,
      invite
    });

    this.emit('party_invite_sent', { invite });
    return true;
  }

  respondToInvite(inviteId: string, accept: boolean): boolean {
    for (const [partyId, party] of this.parties) {
      const invite = party.invites.find(i => i.id === inviteId);
      if (invite) {
        if (accept) {
          const player = this.getLocalPlayer();
          if (player) {
            party.members.push({
              id: player.id,
              name: player.name,
              rank: player.rank,
              ready: false,
              isLeader: false
            });
          }
          invite.status = 'accepted';
        } else {
          invite.status = 'declined';
        }

        party.invites = party.invites.filter(i => i.id !== inviteId);

        this.emit('party_invite_response', { invite });
        return true;
      }
    }
    return false;
  }

  getPlayerParty(): Party | undefined {
    const player = this.getLocalPlayer();
    if (!player) return undefined;

    for (const party of this.parties.values()) {
      if (party.members.find(m => m.id === player.id)) {
        return party;
      }
    }
    return undefined;
  }

  private getLocalPlayer(): Player | undefined {
    return this.activeSession?.players.find(p => !p.isSpectator);
  }

  startMatchmaking(type: QueueType, settings?: MatchmakingSettings): MatchmakingQueue {
    const queue: MatchmakingQueue = {
      id: `queue_${Date.now()}`,
      type,
      region: settings?.sameRegion ? this.getPlayerRegion() : 'na',
      players: [{
        id: 'local',
        name: 'Commander',
        rank: { rating: 1000, division: 'Bronze', league: 'V', stars: 0, points: 0 },
        ping: 0,
        region: this.getPlayerRegion(),
        preferences: { factions: [], maps: [], avoidTeammates: [], preferTeammates: [] },
        joinedAt: Date.now()
      }],
      status: 'searching',
      startTime: Date.now(),
      estimatedTime: 0,
      targetTime: settings?.maxQueueTime ? settings.maxQueueTime * 1000 : 120000,
      settings: settings || {}
    };

    this.matchmakingQueues.set(type, queue);

    this.connection.send({
      type: 'matchmaking_start',
      queue
    });

    this.emit('matchmaking_started', { queue });
    return queue;
  }

  cancelMatchmaking(type?: QueueType): boolean {
    if (type) {
      const queue = this.matchmakingQueues.get(type);
      if (queue) {
        queue.status = 'cancelled';

        this.connection.send({
          type: 'matchmaking_cancel',
          queueId: queue.id
        });

        this.emit('matchmaking_cancelled', { queue });
        return true;
      }
    } else {
      for (const [queueType, queue] of this.matchmakingQueues) {
        if (queue.status === 'searching' || queue.status === 'evaluating') {
          queue.status = 'cancelled';

          this.connection.send({
            type: 'matchmaking_cancel',
            queueId: queue.id
          });

          this.emit('matchmaking_cancelled', { queue });
          return true;
        }
      }
    }
    return false;
  }

  private handleMatchmakingUpdate(queue: MatchmakingQueue): void {
    this.matchmakingQueues.set(queue.type, queue);

    if (queue.status === 'ready') {
      this.emit('matchmaking_ready', { queue });
    } else if (queue.status === 'forming') {
      this.emit('matchmaking_forming', { queue });
    } else {
      this.emit('matchmaking_update', { queue });
    }
  }

  acceptMatch(): boolean {
    for (const queue of this.matchmakingQueues.values()) {
      if (queue.status === 'ready') {
        queue.status = 'forming';

        this.connection.send({
          type: 'matchmaking_accept',
          queueId: queue.id
        });

        this.emit('match_accepted', { queue });
        return true;
      }
    }
    return false;
  }

  declineMatch(): boolean {
    for (const queue of this.matchmakingQueues.values()) {
      if (queue.status === 'ready') {
        this.cancelMatchmaking(queue.type);
        return true;
      }
    }
    return false;
  }

  private getPlayerRegion(): Region {
    return 'na';
  }

  getAvailableServers(region?: Region): GameServer[] {
    return this.servers.filter(server => {
      if (region && server.region !== region) return false;
      return server.status === 'online';
    });
  }

  selectServer(serverId: string): boolean {
    const server = this.servers.find(s => s.id === serverId);
    if (!server) return false;

    this.connection.send({
      type: 'server_select',
      serverId
    });

    this.emit('server_selected', { server });
    return true;
  }

  createTournament(settings: CreateTournamentOptions): Tournament {
    const tournament: Tournament = {
      id: `tournament_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      name: settings.name,
      description: settings.description || '',
      banner: settings.banner || '',
      organizer: settings.organizer,
      sponsor: settings.sponsor,
      prize: settings.prize,
      format: settings.format,
      rules: settings.rules,
      schedule: settings.schedule,
      registration: settings.registration,
      participants: [],
      matches: [],
      status: 'upcoming',
      settings: {
        public: settings.public ?? true,
        ranked: settings.ranked ?? true,
        allowSpectators: settings.allowSpectators ?? true,
        chatEnabled: true,
        autoAdvance: false,
        pauseBetweenMatches: 300
      }
    };

    this.tournaments.set(tournament.id, tournament);

    this.emit('tournament_created', { tournament });
    return tournament;
  }

  registerForTournament(tournamentId: string, teamMembers?: string[]): boolean {
    const tournament = this.tournaments.get(tournamentId);
    if (!tournament) return false;

    if (tournament.status !== 'registration') return false;

    const participant: TournamentParticipant = {
      id: `participant_${Date.now()}`,
      type: teamMembers ? 'team' : 'player',
      name: 'Commander',
      players: teamMembers || ['local'],
      status: 'registered',
      checkedIn: false,
      wins: 0,
      losses: 0
    };

    tournament.participants.push(participant);

    this.connection.send({
      type: 'tournament_register',
      tournamentId,
      participant
    });

    this.emit('tournament_registered', { tournamentId, participant });
    return true;
  }

  checkInForTournament(tournamentId: string): boolean {
    const tournament = this.tournaments.get(tournamentId);
    if (!tournament) return false;

    if (tournament.status !== 'check_in') return false;

    const participant = tournament.participants.find(p =>
      p.players.includes('local') && p.status === 'registered'
    );

    if (!participant) return false;

    participant.checkedIn = true;
    participant.checkInTime = Date.now();
    participant.status = 'checked_in';

    this.connection.send({
      type: 'tournament_checkin',
      tournamentId,
      participantId: participant.id
    });

    this.emit('tournament_checkin', { tournamentId, participant });
    return true;
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

  destroy(): void {
    if (this.heartbeatInterval) {
      clearInterval(this.heartbeatInterval);
    }

    this.connection.disconnect();
    this.sessions.clear();
    this.parties.clear();
    this.matchmakingQueues.clear();
    this.tournaments.clear();
    this.listeners.clear();
  }
}

export interface NetworkMessage {
  type: string;
  data?: unknown;
  timestamp?: number;
}

export interface JoinResult {
  success: boolean;
  error?: string;
  session?: GameSession;
}

export interface CreateSessionOptions {
  name?: string;
  type?: SessionType;
  host?: Player;
  settings?: Partial<SessionSettings>;
  map?: MapInfo;
  rules?: Partial<GameRules>;
}

export interface CreateTournamentOptions {
  name: string;
  description?: string;
  banner?: string;
  organizer: string;
  sponsor?: string;
  prize?: PrizePool;
  format: TournamentFormat;
  rules: TournamentRules;
  schedule: TournamentSchedule;
  registration: RegistrationInfo;
  public?: boolean;
  ranked?: boolean;
  allowSpectators?: boolean;
}

interface NetworkConnection {
  connect(): void;
  disconnect(): void;
  send(message: NetworkMessage): void;
  isConnected(): boolean;
  on(event: string, callback: (data?: unknown) => void): void;
  off(event: string, callback: (data?: unknown) => void): void;
}

class WebSocketConnection implements NetworkConnection {
  private socket?: WebSocket;
  private connected: boolean = false;
  private listeners: Map<string, Set<(data?: unknown) => void>> = new Map();
  private messageQueue: NetworkMessage[] = [];
  private reconnectTimeout?: NodeJS.Timeout;
  private url: string = '';

  connect(url: string = 'wss://game.example.com'): void {
    this.url = url;

    try {
      this.socket = new WebSocket(url);
      this.socket.onopen = () => {
        this.connected = true;
        this.emit('connect', {});
        this.flushMessageQueue();
      };
      this.socket.onclose = () => {
        this.connected = false;
        this.emit('disconnect', {});
        this.scheduleReconnect();
      };
      this.socket.onerror = (error) => {
        this.emit('error', { error });
      };
      this.socket.onmessage = (event) => {
        try {
          const message = JSON.parse(event.data);
          this.emit('message', message);
        } catch (e) {
          console.error('Failed to parse message:', e);
        }
      };
    } catch (error) {
      this.emit('error', { error });
    }
  }

  disconnect(): void {
    if (this.reconnectTimeout) {
      clearTimeout(this.reconnectTimeout);
      this.reconnectTimeout = undefined;
    }

    if (this.socket) {
      this.socket.close();
      this.socket = undefined;
    }

    this.connected = false;
  }

  send(message: NetworkMessage): void {
    if (this.connected && this.socket) {
      this.socket.send(JSON.stringify(message));
    } else {
      this.messageQueue.push(message);
    }
  }

  isConnected(): boolean {
    return this.connected;
  }

  on(event: string, callback: (data?: unknown) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: (data?: unknown) => void): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data?: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(cb => cb(data));
    }
  }

  private flushMessageQueue(): void {
    while (this.messageQueue.length > 0) {
      const message = this.messageQueue.shift();
      if (message) {
        this.send(message);
      }
    }
  }

  private scheduleReconnect(): void {
    this.reconnectTimeout = setTimeout(() => {
      this.connect(this.url);
    }, 5000);
  }
}

class SessionSynchronizer {
  private syncState: Map<string, SessionSyncState> = new Map();

  sendSync(sessionId: string, state: SessionSyncState): void {
    this.syncState.set(sessionId, state);
  }

  receiveSync(state: SessionSyncState): void {
    const sessionId = state.sessionId;
    this.syncState.set(sessionId, state);
  }

  getSyncState(sessionId: string): SessionSyncState | undefined {
    return this.syncState.get(sessionId);
  }

  clearSyncState(sessionId: string): void {
    this.syncState.delete(sessionId);
  }
}

interface SessionSyncState {
  sessionId: string;
  timestamp: number;
  frame: number;
  players: Player[];
  gameState?: Record<string, unknown>;
  version: number;
}

export const multiplayerSession = new MultiplayerSessionManager();

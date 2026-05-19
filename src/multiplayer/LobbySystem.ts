export interface LobbyConfig {
  maxPlayers: number;
  maxSpectators: number;
  allowLateJoin: boolean;
  readyRequired: boolean;
  teamBalance: TeamBalanceMode;
  voteKickEnabled: boolean;
  voteKickThreshold: number;
  chatEnabled: boolean;
  privateLobby: boolean;
  passwordProtected: boolean;
  autoStart: boolean;
  countdownTime: number;
  readyTimeout: number;
  kickOnDisconnect: boolean;
  reconnectionTime: number;
}

export enum TeamBalanceMode {
  OFF = 'off',
  AUTO = 'auto',
  MANUAL = 'manual',
  CAPTAIN = 'captain',
}

export interface Lobby {
  id: string;
  name: string;
  host: LobbyPlayer;
  players: LobbyPlayer[];
  spectators: LobbyPlayer[];
  map: LobbyMap;
  settings: LobbySettings;
  chat: ChatMessage[];
  votes: LobbyVote[];
  state: LobbyState;
  createdAt: number;
  isPrivate: boolean;
  hasPassword: boolean;
  region: string;
  tags: string[];
}

export interface LobbyPlayer {
  id: string;
  odId: string;
  name: string;
  avatar: string;
  level: number;
  rank: number;
  rating: number;
  faction: string;
  team: number;
  slot: number;
  isReady: boolean;
  isHost: boolean;
  isSpectator: boolean;
  isConnected: boolean;
  ping: number;
  isAfk: boolean;
  lastActivity: number;
  settings: PlayerSettings;
  metadata: Record<string, unknown>;
}

export interface PlayerSettings {
  audioEnabled: boolean;
  videoEnabled: boolean;
  pushToTalk: boolean;
  friendlyFire: boolean;
  color: string;
  readyShortcut: boolean;
  autoReady: boolean;
}

export interface LobbyMap {
  id: string;
  name: string;
  file: string;
  thumbnail: string;
  width: number;
  height: number;
  players: number;
  difficulty: string;
  gameMode: string;
  rating: number;
  downloads: number;
  author: string;
  tags: string[];
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
  autoBalance: boolean;
  handicapEnabled: boolean;
  sharedControl: boolean;
  unitCap: number;
  timeLimit: number;
  scoreLimit: number;
  gracePeriod: number;
  buildSpeed: number;
  resourceMultipler: number;
}

export enum GameMode {
  SKIRMISH = 'skirmish',
  MULTIPLAYER = 'multiplayer',
  COOPERATIVE = 'cooperative',
  CAPTAIN = 'captain',
  DUEL = 'duel',
  FREE_FOR_ALL = 'free_for_all',
  TEAM_GAME = 'team_game',
  SUDDEN_DEATH = 'sudden_death',
  RAGE = 'rage',
  ARENA = 'arena',
}

export enum MapSize {
  TINY = 4,
  SMALL = 6,
  MEDIUM = 8,
  LARGE = 10,
  HUGE = 12,
  GIANT = 16,
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
  ESCAPE = 'escape',
  CONQUEST = 'conquest',
}

export interface ChatMessage {
  id: string;
  senderId: string;
  senderName: string;
  content: string;
  timestamp: number;
  channel: ChatChannel;
  recipientId?: string;
  isSystem: boolean;
  muted: boolean;
}

export enum ChatChannel {
  ALL = 'all',
  TEAM = 'team',
  SQUAD = 'squad',
  WHISPER = 'whisper',
  SYSTEM = 'system',
  SPECTATORS = 'spectators',
}

export interface LobbyVote {
  id: string;
  type: VoteType;
  initiatorId: string;
  targetId?: string;
  votesYes: Set<string>;
  votesNo: Set<string>;
  requiredYes: number;
  timeRemaining: number;
  passed: boolean | null;
  result?: boolean;
}

export enum VoteType {
  KICK = 'kick',
  SURRENDER = 'surrender',
  REMATCH = 'rematch',
  PAUSE = 'pause',
  UNPAUSE = 'unpause',
  CHANGE_MAP = 'change_map',
  CHANGE_SETTINGS = 'change_settings',
  EXTEND_TIME = 'extend_time',
}

export enum LobbyState {
  WAITING = 'waiting',
  COUNTDOWN = 'countdown',
  STARTING = 'starting',
  IN_PROGRESS = 'in_progress',
  ENDED = 'ended',
  PAUSED = 'paused',
}

export interface MatchmakingConfig {
  enabled: boolean;
  region: string;
  rankRange: number;
  teamSize: number;
  queueTimeout: number;
  acceptTimeout: number;
  partyEnabled: boolean;
  skillBalancing: boolean;
  preferredModes: GameMode[];
}

export interface MatchmakingQueue {
  id: string;
  mode: GameMode;
  region: string;
  rating: number;
  players: MatchmakingPlayer[];
  status: MatchmakingStatus;
  startTime: number;
  estimatedWait: number;
  matchFound: boolean;
  matchId?: string;
}

export interface MatchmakingPlayer {
  id: string;
  name: string;
  rating: number;
  rank: number;
  faction: string;
  partyId?: string;
  ready: boolean;
  accepted: boolean;
  acceptedAt?: number;
}

export enum MatchmakingStatus {
  IDLE = 'idle',
  SEARCHING = 'searching',
  FOUND = 'found',
  ACCEPTING = 'accepting',
  COUNTDOWN = 'countdown',
  CANCELLED = 'cancelled',
  ERROR = 'error',
  TIMEOUT = 'timeout',
}

export interface Match {
  id: string;
  lobbyId: string;
  players: MatchPlayer[];
  teams: MatchTeam[];
  map: LobbyMap;
  settings: LobbySettings;
  state: MatchState;
  startTime: number;
  endTime?: number;
  duration: number;
  winner?: string;
  scores: Map<string, number>;
  events: MatchEvent[];
  statistics: MatchStatistics;
}

export interface MatchPlayer {
  id: string;
  name: string;
  odId: string;
  faction: string;
  team: number;
  isAlive: boolean;
  isConnected: boolean;
  resources: PlayerResources;
  units: PlayerUnitStats;
  buildings: PlayerBuildingStats;
  combat: PlayerCombatStats;
  rating: RatingChange;
}

export interface PlayerResources {
  credits: number;
  ore: number;
  power: number;
  gems: number;
  harvested: number;
  spent: number;
}

export interface PlayerUnitStats {
  produced: number;
  lost: number;
  killed: number;
  currentCount: number;
  maxCount: number;
}

export interface PlayerBuildingStats {
  built: number;
  lost: number;
  destroyed: number;
  currentCount: number;
  productionRate: number;
}

export interface PlayerCombatStats {
  damageDealt: number;
  damageReceived: number;
  healingDone: number;
  accuracy: number;
  kills: number;
  deaths: number;
  assists: number;
}

export interface RatingChange {
  previous: number;
  new: number;
  change: number;
  reason: string;
}

export interface MatchTeam {
  id: number;
  name: string;
  players: string[];
  isAlive: boolean;
  score: number;
  rating: number;
  structuresDestroyed: number;
  unitsProduced: number;
  unitsLost: number;
}

export enum MatchState {
  LOADING = 'loading',
  COUNTDOWN = 'countdown',
  IN_PROGRESS = 'in_progress',
  PAUSED = 'paused',
  ENDED = 'ended',
  SPECTATING = 'spectating',
}

export interface MatchEvent {
  id: string;
  type: MatchEventType;
  timestamp: number;
  playerId?: string;
  data: Record<string, unknown>;
}

export enum MatchEventType {
  BUILDING_DESTROYED = 'building_destroyed',
  UNIT_KILLED = 'unit_killed',
  RESOURCE_CAPTURED = 'resource_captured',
  UPGRADE_COMPLETED = 'upgrade_completed',
  TECH_RESEARCHED = 'tech_researched',
  PLAYER_DISCONNECTED = 'player_disconnected',
  PLAYER_RECONNECTED = 'player_reconnected',
  SURRENDER = 'surrender',
  VICTORY = 'victory',
  PAUSE = 'pause',
  UNPAUSE = 'unpause',
  CHAT = 'chat',
}

export interface MatchStatistics {
  totalDamage: number;
  totalHealing: number;
  totalResources: number;
  totalUnitsProduced: number;
  totalUnitsLost: number;
  totalBuildingsBuilt: number;
  totalBuildingsLost: number;
  avgGameDuration: number;
  peakUnits: number;
  peakBandwidth: number;
}

export interface LeaderboardEntry {
  rank: number;
  playerId: string;
  playerName: string;
  clan?: string;
  clanTag?: string;
  rating: number;
  rankTitle: string;
  wins: number;
  losses: number;
  winRate: number;
  gamesPlayed: number;
  favoriteFaction: string;
  totalPlayTime: number;
  lastPlayed: number;
  streak: number;
  peakRating: number;
  region: string;
  avatar: string;
  badges: Badge[];
}

export interface Badge {
  id: string;
  name: string;
  icon: string;
  description: string;
  tier: BadgeTier;
  rarity: BadgeRarity;
  earnedAt: number;
}

export enum BadgeTier {
  BRONZE = 'bronze',
  SILVER = 'silver',
  GOLD = 'gold',
  PLATINUM = 'platinum',
  DIAMOND = 'diamond',
  MASTER = 'master',
  GRANDMASTER = 'grandmaster',
}

export enum BadgeRarity {
  COMMON = 'common',
  UNCOMMON = 'uncommon',
  RARE = 'rare',
  EPIC = 'epic',
  LEGENDARY = 'legendary',
}

export interface ClanInfo {
  id: string;
  name: string;
  tag: string;
  emblem: string;
  leaderId: string;
  officers: string[];
  members: ClanMember[];
  stats: ClanStats;
  recruitment: ClanRecruitment;
  createdAt: number;
  description: string;
  motto: string;
  website?: string;
  discord?: string;
  founded: number;
  wins: number;
  losses: number;
  rank: number;
  division: string;
}

export interface ClanMember {
  id: string;
  name: string;
  rank: ClanRank;
  role: ClanRole;
  rating: number;
  joinedAt: number;
  lastActive: number;
  contributed: number;
}

export enum ClanRank {
  RECRUIT = 'recruit',
  MEMBER = 'member',
  VETERAN = 'veteran',
  OFFICER = 'officer',
  LEADER = 'leader',
}

export enum ClanRole {
  MEMBER = 'member',
  RECRUITER = 'recruiter',
  OFFICER = 'officer',
  LEADER = 'leader',
  FOUNDER = 'founder',
}

export interface ClanStats {
  totalRating: number;
  averageRating: number;
  gamesPlayed: number;
  gamesWon: number;
  gamesLost: number;
  tournamentWins: number;
  tournamentParticipated: number;
  weeklyXP: number;
  totalXP: number;
  membersOnline: number;
  totalMembers: number;
}

export interface ClanRecruitment {
  enabled: boolean;
  minimumRating: number;
  minimumLevel: number;
  applicationsOpen: boolean;
  requirements: string[];
}

export interface Tournament {
  id: string;
  name: string;
  description: string;
  format: TournamentFormat;
  size: TournamentSize;
  prizePool: PrizeDistribution[];
  status: TournamentStatus;
  registration: TournamentRegistration;
  schedule: TournamentSchedule;
  rules: TournamentRules;
  matches: TournamentMatch[];
  participants: TournamentParticipant[];
  streams: TournamentStream[];
  sponsors: string[];
}

export enum TournamentFormat {
  SINGLE_ELIMINATION = 'single_elimination',
  DOUBLE_ELIMINATION = 'double_elimination',
  ROUND_ROBIN = 'round_robin',
  SWISS = 'swiss',
  LEAGUE = 'league',
}

export enum TournamentSize {
  FOUR = 4,
  EIGHT = 8,
  SIXTEEN = 16,
  THIRTYTWO = 32,
  SIXTYFOUR = 64,
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
  amount: number;
}

export interface TournamentRegistration {
  open: boolean;
  deadline: number;
  cost: number;
  maxParticipants: number;
  currentParticipants: number;
  requirements: string[];
  acceptedPlayers: string[];
  waitlist: string[];
}

export interface TournamentSchedule {
  startDate: number;
  endDate: number;
  rounds: TournamentRound[];
}

export interface TournamentRound {
  number: number;
  name: string;
  startTime: number;
  endTime: number;
  matches: TournamentMatch[];
  completed: boolean;
}

export interface TournamentMatch {
  id: string;
  round: number;
  bracketPosition: number;
  player1Id: string;
  player2Id: string;
  player1Score: number;
  player2Score: number;
  winnerId?: string;
  status: MatchStatus;
  scheduledTime: number;
  actualTime?: number;
  map: string;
  replays: string[];
}

export enum MatchStatus {
  SCHEDULED = 'scheduled',
  IN_PROGRESS = 'in_progress',
  COMPLETED = 'completed',
  CANCELLED = 'cancelled',
  BYE = 'bye',
}

export interface TournamentRules {
  gameVersion: string;
  format: string;
  gameSettings: LobbySettings;
  matchFormat: MatchFormat;
  mapPool: string[];
  banPhase: BanPhase;
  settings: RuleSetting[];
}

export interface MatchFormat {
  games: number;
  winsRequired: number;
  mapSelection: MapSelectionMode;
}

export enum MapSelectionMode {
  RANDOM = 'random',
  VOTING = 'voting',
  HOST_CHOICE = 'host_choice',
  ALTERNATE = 'alternate',
}

export interface BanPhase {
  enabled: boolean;
  bansPerTeam: number;
  mapsBanned: string[];
}

export interface RuleSetting {
  key: string;
  name: string;
  value: unknown;
  description: string;
}

export interface TournamentParticipant {
  id: string;
  name: string;
  rating: number;
  seed: number;
  registeredAt: number;
  checkedIn: boolean;
  checkedInAt?: number;
  status: ParticipantStatus;
  currentRound: number;
  currentMatch?: string;
  overallRank?: number;
  winnings: number;
}

export enum ParticipantStatus {
  REGISTERED = 'registered',
  CHECKED_IN = 'checked_in',
  COMPETING = 'competing',
  ELIMINATED = 'eliminated',
  WINNER = 'winner',
  RUNNER_UP = 'runner_up',
  DROPPED = 'dropped',
}

export interface TournamentStream {
  name: string;
  url: string;
  viewers: number;
  live: boolean;
  casters: string[];
}

export class LobbyManager {
  private config: LobbyConfig;
  private currentLobby: Lobby | null = null;
  private player: LobbyPlayer | null = null;
  private lobbyHistory: string[] = [];
  private eventCallbacks: Map<string, Set<Function>> = new Map();
  private readyTimers: Map<string, number> = new Map();
  private afkTimers: Map<string, number> = new Map();
  private chatHistory: Map<string, ChatMessage[]> = new Map();
  private maxChatHistory: number = 100;

  constructor(config: Partial<LobbyConfig> = {}) {
    this.config = {
      maxPlayers: config.maxPlayers || 8,
      maxSpectators: config.maxSpectators || 10,
      allowLateJoin: config.allowLateJoin ?? true,
      readyRequired: config.readyRequired ?? true,
      teamBalance: config.teamBalance || TeamBalanceMode.AUTO,
      voteKickEnabled: config.voteKickEnabled ?? true,
      voteKickThreshold: config.voteKickThreshold || 0.5,
      chatEnabled: config.chatEnabled ?? true,
      privateLobby: config.privateLobby ?? false,
      passwordProtected: config.passwordProtected ?? false,
      autoStart: config.autoStart ?? false,
      countdownTime: config.countdownTime || 10,
      readyTimeout: config.readyTimeout || 120,
      kickOnDisconnect: config.kickOnDisconnect ?? true,
      reconnectionTime: config.reconnectionTime || 60,
    };
  }

  public createLobby(name: string, map: LobbyMap, settings?: Partial<LobbySettings>): Lobby {
    const lobbyId = this.generateId('lobby');

    this.player = this.createLocalPlayer(true);

    const lobby: Lobby = {
      id: lobbyId,
      name,
      host: this.player,
      players: [this.player],
      spectators: [],
      map,
      settings: { ...this.getDefaultSettings(), ...settings },
      chat: [],
      votes: [],
      state: LobbyState.WAITING,
      createdAt: Date.now(),
      isPrivate: this.config.privateLobby,
      hasPassword: this.config.passwordProtected,
      region: 'us-west',
      tags: [],
    };

    this.currentLobby = lobby;
    this.emit('lobby_created', { lobby });
    this.emit('joined_lobby', { lobby });

    return lobby;
  }

  public joinLobby(lobbyId: string, password?: string): boolean {
    if (this.currentLobby) {
      this.leaveLobby();
    }

    this.player = this.createLocalPlayer(false);

    const lobby: Lobby = {
      id: lobbyId,
      name: 'Remote Lobby',
      host: { ...this.player, isHost: true },
      players: [this.player],
      spectators: [],
      map: this.getDefaultMap(),
      settings: this.getDefaultSettings(),
      chat: [],
      votes: [],
      state: LobbyState.WAITING,
      createdAt: Date.now(),
      isPrivate: false,
      hasPassword: !!password,
      region: 'us-west',
      tags: [],
    };

    this.currentLobby = lobby;
    this.lobbyHistory.push(lobbyId);
    this.emit('joined_lobby', { lobby });
    this.emit('player_joined', { player: this.player });

    return true;
  }

  public leaveLobby(): void {
    if (!this.currentLobby) {
      return;
    }

    const lobbyId = this.currentLobby.id;

    if (this.player) {
      if (this.player.isHost) {
        this.handleHostLeave();
      } else {
        this.currentLobby.players = this.currentLobby.players.filter(p => p.id !== this.player!.id);
        this.currentLobby.spectators = this.currentLobby.spectators.filter(p => p.id !== this.player!.id);
      }

      this.emit('player_left', { player: this.player, lobbyId });
    }

    this.player = null;
    this.currentLobby = null;
    this.readyTimers.clear();
    this.afkTimers.clear();
    this.emit('left_lobby', { lobbyId });
  }

  private handleHostLeave(): void {
    if (!this.currentLobby) return;

    const players = this.currentLobby.players.filter(p => p.id !== this.player!.id);
    if (players.length > 0) {
      const newHost = players[0];
      newHost.isHost = true;
      this.currentLobby.host = newHost;
      this.emit('host_changed', { newHostId: newHost.id });
    } else {
      this.currentLobby.state = LobbyState.ENDED;
      this.emit('lobby_ended', { reason: 'Host left' });
    }
  }

  public setReady(ready: boolean): void {
    if (!this.currentLobby || !this.player) return;

    this.player.isReady = ready;
    this.emit('player_ready_changed', { player: this.player });

    if (ready) {
      this.readyTimers.delete(this.player.id);
    }

    this.checkAllReady();
  }

  private checkAllReady(): void {
    if (!this.currentLobby) return;

    if (this.config.readyRequired) {
      const allReady = this.currentLobby.players
        .filter(p => !p.isSpectator)
        .every(p => p.isReady);

      if (allReady && this.currentLobby.players.filter(p => !p.isSpectator).length >= 2) {
        this.startCountdown();
      } else if (this.currentLobby.state === LobbyState.COUNTDOWN) {
        this.cancelCountdown();
      }
    } else if (this.config.autoStart) {
      const playerCount = this.currentLobby.players.filter(p => !p.isSpectator).length;
      if (playerCount >= 2) {
        this.startCountdown();
      }
    }
  }

  public startCountdown(): void {
    if (!this.currentLobby || this.currentLobby.state !== LobbyState.WAITING) return;

    this.currentLobby.state = LobbyState.COUNTDOWN;
    this.emit('countdown_started', { duration: this.config.countdownTime });

    const countdownEnd = Date.now() + this.config.countdownTime * 1000;

    const checkCountdown = () => {
      if (!this.currentLobby || this.currentLobby.state !== LobbyState.COUNTDOWN) return;

      const remaining = Math.max(0, countdownEnd - Date.now());
      const seconds = Math.ceil(remaining / 1000);

      this.emit('countdown_tick', { remaining: seconds });

      if (remaining <= 0) {
        this.startGame();
      } else {
        setTimeout(checkCountdown, 100);
      }
    };

    setTimeout(checkCountdown, 100);
  }

  public cancelCountdown(): void {
    if (!this.currentLobby) return;

    this.currentLobby.state = LobbyState.WAITING;
    this.emit('countdown_cancelled');
  }

  public startGame(): void {
    if (!this.currentLobby) return;

    this.currentLobby.state = LobbyState.STARTING;
    this.emit('game_starting');

    setTimeout(() => {
      if (this.currentLobby) {
        this.currentLobby.state = LobbyState.IN_PROGRESS;
        this.emit('game_started');
      }
    }, 3000);
  }

  public endGame(winnerId?: string): void {
    if (!this.currentLobby) return;

    this.currentLobby.state = LobbyState.ENDED;
    this.emit('game_ended', { winnerId });
  }

  public sendChat(content: string, channel: ChatChannel = ChatChannel.ALL): void {
    if (!this.currentLobby || !this.player || !this.config.chatEnabled) return;

    const message: ChatMessage = {
      id: this.generateId('msg'),
      senderId: this.player.id,
      senderName: this.player.name,
      content,
      timestamp: Date.now(),
      channel,
      isSystem: false,
      muted: false,
    };

    this.addChatMessage(message);
    this.emit('chat_message', { message });
  }

  private addChatMessage(message: ChatMessage): void {
    if (!this.currentLobby) return;

    this.currentLobby.chat.push(message);

    if (this.currentLobby.chat.length > this.maxChatHistory) {
      this.currentLobby.chat.shift();
    }

    const history = this.chatHistory.get(message.channel) || [];
    history.push(message);
    if (history.length > this.maxChatHistory) {
      history.shift();
    }
    this.chatHistory.set(message.channel, history);
  }

  public kickPlayer(playerId: string, reason?: string): boolean {
    if (!this.currentLobby || !this.player?.isHost) return false;

    const player = this.findPlayer(playerId);
    if (!player || player.isHost) return false;

    this.currentLobby.players = this.currentLobby.players.filter(p => p.id !== playerId);
    this.currentLobby.spectators = this.currentLobby.spectators.filter(p => p.id !== playerId);

    const systemMessage: ChatMessage = {
      id: this.generateId('sys'),
      senderId: 'system',
      senderName: 'System',
      content: `${player.name} was kicked from the lobby${reason ? `: ${reason}` : '.'}`,
      timestamp: Date.now(),
      channel: ChatChannel.SYSTEM,
      isSystem: true,
      muted: false,
    };
    this.addChatMessage(systemMessage);

    this.emit('player_kicked', { player, reason });
    return true;
  }

  public banPlayer(playerId: string, reason?: string): boolean {
    if (!this.kickPlayer(playerId, reason)) return false;

    this.emit('player_banned', { playerId, reason });
    return true;
  }

  public startVote(type: VoteType, targetId?: string): boolean {
    if (!this.currentLobby || !this.player) return false;

    if (type === VoteType.KICK && !targetId) return false;

    const existingVote = this.currentLobby.votes.find(v => v.type === type && v.passed === null);
    if (existingVote) return false;

    const requiredYes = Math.ceil(this.currentLobby.players.length * this.config.voteKickThreshold);

    const vote: LobbyVote = {
      id: this.generateId('vote'),
      type,
      initiatorId: this.player.id,
      targetId,
      votesYes: new Set([this.player.id]),
      votesNo: new Set(),
      requiredYes,
      timeRemaining: 60,
      passed: null,
    };

    this.currentLobby.votes.push(vote);

    const systemMessage: ChatMessage = {
      id: this.generateId('sys'),
      senderId: 'system',
      senderName: 'System',
      content: `Vote started: ${type}${targetId ? ` against ${this.findPlayer(targetId)?.name}` : ''}`,
      timestamp: Date.now(),
      channel: ChatChannel.SYSTEM,
      isSystem: true,
      muted: false,
    };
    this.addChatMessage(systemMessage);

    this.emit('vote_started', { vote });

    this.updateVoteTimers(vote);

    return true;
  }

  private updateVoteTimers(vote: LobbyVote): void {
    const interval = setInterval(() => {
      if (!this.currentLobby) {
        clearInterval(interval);
        return;
      }

      const currentVote = this.currentLobby.votes.find(v => v.id === vote.id);
      if (!currentVote) {
        clearInterval(interval);
        return;
      }

      currentVote.timeRemaining--;

      if (currentVote.timeRemaining <= 0) {
        this.resolveVote(currentVote);
        clearInterval(interval);
      }
    }, 1000);
  }

  public vote(voteId: string, approve: boolean): boolean {
    if (!this.currentLobby || !this.player) return false;

    const vote = this.currentLobby.votes.find(v => v.id === voteId && v.passed === null);
    if (!vote) return false;

    if (approve) {
      vote.votesYes.add(this.player.id);
      vote.votesNo.delete(this.player.id);
    } else {
      vote.votesNo.add(this.player.id);
      vote.votesYes.delete(this.player.id);
    }

    const totalPlayers = this.currentLobby.players.filter(p => !p.isSpectator).length;
    if (vote.votesYes.size >= vote.requiredYes) {
      this.resolveVote(vote, true);
    } else if (vote.votesNo.size >= totalPlayers - vote.requiredYes) {
      this.resolveVote(vote, false);
    }

    this.emit('vote_updated', { vote });

    return true;
  }

  private resolveVote(vote: LobbyVote, passed: boolean): void {
    vote.passed = passed;
    vote.result = passed;

    switch (vote.type) {
      case VoteType.KICK:
        if (passed && vote.targetId) {
          this.kickPlayer(vote.targetId, 'Vote to kick');
        }
        break;
      case VoteType.SURRENDER:
        if (passed) {
          this.endGame();
        }
        break;
      case VoteType.PAUSE:
        if (passed) {
          this.currentLobby!.state = LobbyState.PAUSED;
          this.emit('lobby_paused', { initiatorId: vote.initiatorId });
        }
        break;
      case VoteType.UNPAUSE:
        if (passed) {
          this.currentLobby!.state = LobbyState.IN_PROGRESS;
          this.emit('lobby_unpaused');
        }
        break;
    }

    this.emit('vote_ended', { vote });
  }

  public changeTeam(playerId: string, newTeam: number): boolean {
    if (!this.currentLobby) return false;

    const player = this.findPlayer(playerId);
    if (!player) return false;

    player.team = newTeam;

    if (this.config.teamBalance === TeamBalanceMode.AUTO) {
      this.balanceTeams();
    }

    this.emit('team_changed', { player, newTeam });
    return true;
  }

  private balanceTeams(): void {
    if (!this.currentLobby) return;

    const players = this.currentLobby.players.filter(p => !p.isSpectator);
    const teams = new Map<number, LobbyPlayer[]>();

    players.forEach(p => {
      const team = teams.get(p.team) || [];
      team.push(p);
      teams.set(p.team, team);
    });

    let teamId = 0;
    const sortedPlayers = [...players].sort((a, b) => b.rating - a.rating);

    sortedPlayers.forEach((player, index) => {
      player.team = index % 2;
    });
  }

  public updateSettings(settings: Partial<LobbySettings>): void {
    if (!this.currentLobby || !this.player?.isHost) return;

    this.currentLobby.settings = { ...this.currentLobby.settings, ...settings };
    this.emit('settings_changed', { settings: this.currentLobby.settings });
  }

  public changeMap(map: LobbyMap): boolean {
    if (!this.currentLobby || !this.player?.isHost) return false;

    this.currentLobby.map = map;
    this.emit('map_changed', { map });
    return true;
  }

  public makeSpectator(playerId: string): boolean {
    if (!this.currentLobby || !this.player?.isHost) return false;

    const player = this.findPlayer(playerId);
    if (!player || player.isHost) return false;

    player.isSpectator = true;
    player.isReady = false;
    player.team = -1;

    this.currentLobby.spectators.push(player);
    this.emit('made_spectator', { player });
    return true;
  }

  public makePlayer(playerId: string): boolean {
    if (!this.currentLobby || !this.player?.isHost) return false;

    const spectator = this.currentLobby.spectators.find(p => p.id === playerId);
    if (!spectator) return false;

    if (this.currentLobby.players.length >= this.config.maxPlayers) return false;

    spectator.isSpectator = false;
    spectator.team = this.findEmptyTeam();

    this.currentLobby.players.push(spectator);
    this.currentLobby.spectators = this.currentLobby.spectators.filter(p => p.id !== playerId);

    this.emit('made_player', { player: spectator });
    return true;
  }

  private findEmptyTeam(): number {
    if (!this.currentLobby) return 0;

    const teamCounts = new Map<number, number>();
    this.currentLobby.players.forEach(p => {
      if (!p.isSpectator) {
        teamCounts.set(p.team, (teamCounts.get(p.team) || 0) + 1);
      }
    });

    let minTeam = 0;
    let minCount = Infinity;
    teamCounts.forEach((count, team) => {
      if (count < minCount) {
        minCount = count;
        minTeam = team;
      }
    });

    return minTeam;
  }

  private findPlayer(playerId: string): LobbyPlayer | undefined {
    if (!this.currentLobby) return undefined;
    return this.currentLobby.players.find(p => p.id === playerId) ||
           this.currentLobby.spectators.find(p => p.id === playerId);
  }

  private createLocalPlayer(isHost: boolean): LobbyPlayer {
    return {
      id: this.generateId('player'),
      odId: '',
      name: 'Player',
      avatar: '',
      level: 1,
      rank: 1,
      rating: 1000,
      faction: 'default',
      team: 0,
      slot: 0,
      isReady: false,
      isHost,
      isSpectator: false,
      isConnected: true,
      ping: 0,
      isAfk: false,
      lastActivity: Date.now(),
      settings: {
        audioEnabled: true,
        videoEnabled: true,
        pushToTalk: false,
        friendlyFire: false,
        color: '#ffffff',
        readyShortcut: true,
        autoReady: false,
      },
      metadata: {},
    };
  }

  private getDefaultSettings(): LobbySettings {
    return {
      gameMode: GameMode.SKIRMISH,
      mapSize: MapSize.MEDIUM,
      startingResources: 5000,
      gameSpeed: GameSpeed.NORMAL,
      victoryCondition: VictoryCondition.ANNIHILATION,
      fogOfWar: true,
      cheatsEnabled: false,
      lockTeams: false,
      allowSpectators: true,
      autoBalance: true,
      handicapEnabled: false,
      sharedControl: false,
      unitCap: 200,
      timeLimit: 3600,
      scoreLimit: 0,
      gracePeriod: 60,
      buildSpeed: 1.0,
      resourceMultipler: 1.0,
    };
  }

  private getDefaultMap(): LobbyMap {
    return {
      id: 'default',
      name: 'Default Map',
      file: 'maps/default.zip',
      thumbnail: 'maps/default_thumb.png',
      width: 100,
      height: 100,
      players: 4,
      difficulty: 'normal',
      gameMode: 'skirmish',
      rating: 0,
      downloads: 0,
      author: 'Unknown',
      tags: [],
    };
  }

  private generateId(prefix: string): string {
    return `${prefix}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  public getLobby(): Lobby | null {
    return this.currentLobby ? { ...this.currentLobby } : null;
  }

  public getPlayer(): LobbyPlayer | null {
    return this.player ? { ...this.player } : null;
  }

  public getConfig(): LobbyConfig {
    return { ...this.config };
  }

  public on(event: string, callback: Function): void {
    if (!this.eventCallbacks.has(event)) {
      this.eventCallbacks.set(event, new Set());
    }
    this.eventCallbacks.get(event)!.add(callback);
  }

  public off(event: string, callback: Function): void {
    const callbacks = this.eventCallbacks.get(event);
    if (callbacks) {
      callbacks.delete(callback);
    }
  }

  private emit(event: string, data?: unknown): void {
    const callbacks = this.eventCallbacks.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  public destroy(): void {
    this.leaveLobby();
    this.eventCallbacks.forEach(set => set.clear());
    this.eventCallbacks.clear();
    this.readyTimers.clear();
    this.afkTimers.clear();
    this.chatHistory.clear();
  }
}

export class MatchmakingManager {
  private config: MatchmakingConfig;
  private currentQueue: MatchmakingQueue | null = null;
  private playerRatings: Map<string, number> = new Map();
  private eventCallbacks: Map<string, Set<Function>> = new Map();
  private searchTimer: number = 0;

  constructor(config: Partial<MatchmakingConfig> = {}) {
    this.config = {
      enabled: config.enabled ?? true,
      region: config.region || 'us-west',
      rankRange: config.rankRange || 500,
      teamSize: config.teamSize || 1,
      queueTimeout: config.queueTimeout || 300,
      acceptTimeout: config.acceptTimeout || 30,
      partyEnabled: config.partyEnabled ?? true,
      skillBalancing: config.skillBalancing ?? true,
      preferredModes: config.preferredModes || [GameMode.SKIRMISH],
    };
  }

  public joinQueue(mode: GameMode, rating: number): MatchmakingQueue {
    if (this.currentQueue) {
      this.leaveQueue();
    }

    this.currentQueue = {
      id: `queue_${Date.now()}`,
      mode,
      region: this.config.region,
      rating,
      players: [{
        id: 'local_player',
        name: 'Player',
        rating,
        rank: this.calculateRank(rating),
        faction: 'default',
        ready: false,
        accepted: false,
      }],
      status: MatchmakingStatus.SEARCHING,
      startTime: Date.now(),
      estimatedWait: 0,
      matchFound: false,
    };

    this.searchTimer = 0;
    this.emit('queue_joined', { queue: this.currentQueue });
    this.emit('matchmaking_started');

    return this.currentQueue;
  }

  public leaveQueue(): void {
    if (!this.currentQueue) return;

    const queueId = this.currentQueue.id;
    this.currentQueue = null;
    this.searchTimer = 0;

    this.emit('queue_left', { queueId });
    this.emit('matchmaking_cancelled');
  }

  public update(deltaTime: number): void {
    if (!this.currentQueue || this.currentQueue.status !== MatchmakingStatus.SEARCHING) return;

    this.searchTimer += deltaTime;

    const waitTime = this.searchTimer / 1000;

    const baseWait = 10;
    const maxWait = this.config.queueTimeout;
    this.currentQueue.estimatedWait = Math.max(0, Math.min(baseWait, maxWait - waitTime));

    if (waitTime > this.config.queueTimeout) {
      this.cancelMatchmaking('timeout');
      return;
    }

    if (Math.random() < 0.001) {
      this.findMatch();
    }

    this.emit('matchmaking_update', { queue: this.currentQueue });
  }

  private findMatch(): void {
    if (!this.currentQueue) return;

    this.currentQueue.status = MatchmakingStatus.FOUND;
    this.currentQueue.matchFound = true;
    this.currentQueue.matchId = `match_${Date.now()}`;

    this.emit('match_found', { matchId: this.currentQueue.matchId });

    this.currentQueue.status = MatchmakingStatus.ACCEPTING;

    setTimeout(() => {
      if (this.currentQueue?.status === MatchmakingStatus.ACCEPTING) {
        this.acceptMatch();
      }
    }, this.config.acceptTimeout * 1000);
  }

  public acceptMatch(): boolean {
    if (!this.currentQueue || this.currentQueue.status !== MatchmakingStatus.ACCEPTING) return false;

    const player = this.currentQueue.players[0];
    player.accepted = true;
    player.acceptedAt = Date.now();

    if (this.currentQueue.players.every(p => p.accepted)) {
      this.startMatchCountdown();
    }

    this.emit('match_accepted');
    return true;
  }

  private startMatchCountdown(): void {
    if (!this.currentQueue) return;

    this.currentQueue.status = MatchmakingStatus.COUNTDOWN;

    this.emit('match_countdown_started', { countdown: 5 });

    setTimeout(() => {
      if (this.currentQueue) {
        this.emit('match_started', { matchId: this.currentQueue.matchId });
        this.currentQueue = null;
      }
    }, 5000);
  }

  public cancelMatchmaking(reason: string = 'cancelled'): void {
    if (!this.currentQueue) return;

    this.emit('matchmaking_failed', { reason });
    this.currentQueue = null;
    this.searchTimer = 0;
  }

  public addPartyMember(player: MatchmakingPlayer): boolean {
    if (!this.currentQueue || !this.config.partyEnabled) return false;

    if (this.currentQueue.players.length >= this.config.teamSize) return false;

    this.currentQueue.players.push(player);
    this.emit('party_member_added', { player });
    return true;
  }

  public removePartyMember(playerId: string): boolean {
    if (!this.currentQueue) return false;

    const index = this.currentQueue.players.findIndex(p => p.id === playerId);
    if (index <= 0) return false;

    this.currentQueue.players.splice(index, 1);
    this.emit('party_member_removed', { playerId });
    return true;
  }

  private calculateRank(rating: number): number {
    if (rating < 1000) return 1;
    if (rating < 1200) return 2;
    if (rating < 1400) return 3;
    if (rating < 1600) return 4;
    if (rating < 1800) return 5;
    if (rating < 2000) return 6;
    if (rating < 2200) return 7;
    if (rating < 2400) return 8;
    return 9;
  }

  public getQueue(): MatchmakingQueue | null {
    return this.currentQueue ? { ...this.currentQueue } : null;
  }

  public getConfig(): MatchmakingConfig {
    return { ...this.config };
  }

  public setRegion(region: string): void {
    this.config.region = region;
  }

  public on(event: string, callback: Function): void {
    if (!this.eventCallbacks.has(event)) {
      this.eventCallbacks.set(event, new Set());
    }
    this.eventCallbacks.get(event)!.add(callback);
  }

  public off(event: string, callback: Function): void {
    const callbacks = this.eventCallbacks.get(event);
    if (callbacks) {
      callbacks.delete(callback);
    }
  }

  private emit(event: string, data?: unknown): void {
    const callbacks = this.eventCallbacks.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  public destroy(): void {
    this.leaveQueue();
    this.eventCallbacks.forEach(set => set.clear());
    this.eventCallbacks.clear();
    this.playerRatings.clear();
  }
}

export const createLobbyManager = (config?: Partial<LobbyConfig>): LobbyManager => {
  return new LobbyManager(config);
};

export const createMatchmakingManager = (config?: Partial<MatchmakingConfig>): MatchmakingManager => {
  return new MatchmakingManager(config);
};

export default LobbyManager;

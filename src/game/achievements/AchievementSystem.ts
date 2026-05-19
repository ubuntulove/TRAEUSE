export interface AchievementSystem {
  achievements: Map<string, Achievement>;
  unlocked: Set<string>;
  progress: Map<string, AchievementProgress>;
  eventListeners: Map<string, Set<Function>>;
}

export interface Achievement {
  id: string;
  name: string;
  description: string;
  icon: string;
  category: AchievementCategory;
  difficulty: AchievementDifficulty;
  rarity: AchievementRarity;
  points: number;
  secret: boolean;
  requirements: AchievementRequirement[];
  rewards: AchievementReward[];
  metadata: AchievementMetadata;
}

export enum AchievementCategory {
  COMBAT = 'combat',
  ECONOMY = 'economy',
  EXPLORATION = 'exploration',
  BUILDING = 'building',
  RESEARCH = 'research',
  SOCIAL = 'social',
  SPEED = 'speed',
  PERFECTION = 'perfection',
  COLLECTION = 'collection',
  SPECIAL = 'special',
  SEASONAL = 'seasonal',
  EVENT = 'event',
  HIDDEN = 'hidden',
  LEGENDARY = 'legendary',
}

export enum AchievementDifficulty {
  EASY = 1,
  MEDIUM = 2,
  HARD = 3,
  EXTREME = 4,
  IMPOSSIBLE = 5,
  LEGENDARY = 6,
}

export enum AchievementRarity {
  COMMON = 'common',
  UNCOMMON = 'uncommon',
  RARE = 'rare',
  EPIC = 'epic',
  LEGENDARY = 'legendary',
  ULTRA_RARE = 'ultra_rare',
}

export interface AchievementRequirement {
  type: RequirementType;
  target: string;
  value: number;
  current?: number;
  operator: ComparisonOperator;
  nested?: AchievementRequirement[];
  invert?: boolean;
}

export enum RequirementType {
  KILL = 'kill',
  BUILD = 'build',
  RESEARCH = 'research',
  GATHER = 'gather',
  EXPLORE = 'explore',
  TIME = 'time',
  SCORE = 'score',
  WIN = 'win',
  STREAK = 'streak',
  CUSTOM = 'custom',
  COMPOUND = 'compound',
}

export enum ComparisonOperator {
  EQUALS = '==',
  NOT_EQUALS = '!=',
  GREATER_THAN = '>',
  LESS_THAN = '<',
  GREATER_OR_EQUAL = '>=',
  LESS_OR_EQUAL = '<=',
  MODULO = '%',
}

export interface AchievementReward {
  type: RewardType;
  id: string;
  quantity?: number;
  probability?: number;
}

export enum RewardType {
  CREDITS = 'credits',
  XP = 'xp',
  SKIN = 'skin',
  BANNER = 'banner',
  BADGE = 'badge',
  TITLE = 'title',
  EMOTE = 'emote',
  FRAME = 'frame',
  MUSIC = 'music',
  VFX = 'vfx',
}

export interface AchievementMetadata {
  displayOrder: number;
  releaseVersion: string;
  releaseDate: number;
  tier: number;
  prestige: number;
  relatedAchievements: string[];
  hints: string[];
  flavorText: string;
  funFact: string;
}

export interface AchievementProgress {
  achievementId: string;
  currentValue: number;
  targetValue: number;
  percentage: number;
  startedAt: number;
  updatedAt: number;
  completed: boolean;
  completedAt?: number;
  notificationShown: boolean;
  hiddenProgress: boolean;
}

export interface PlayerProfile {
  id: string;
  username: string;
  displayName: string;
  avatar: string;
  level: number;
  xp: number;
  xpToNextLevel: number;
  rank: PlayerRank;
  title: string;
  prestige: number;
  stats: PlayerStatistics;
  achievements: PlayerAchievements;
  inventory: InventoryItem[];
  loadouts: Loadout[];
  skins: UnlockedSkin[];
  banners: UnlockedBanner[];
  badges: UnlockedBadge[];
  emotes: UnlockedEmote[];
  titles: UnlockedTitle[];
  friends: Friend[];
  clans: PlayerClan[];
  settings: PlayerSettings;
  preferences: PlayerPreferences;
  statistics: PlayerStats;
  history: PlayerHistory;
  progression: PlayerProgression;
  rewards: PlayerRewards;
  createdAt: number;
  lastOnline: number;
  totalPlayTime: number;
  recentActivity: Activity[];
}

export interface PlayerRank {
  current: number;
  title: string;
  tier: RankTier;
  division: RankDivision;
  ladderPoints: number;
  seasonWins: number;
  seasonLosses: number;
  peakRank: number;
  rankName: string;
}

export enum RankTier {
  BRONZE = 'bronze',
  SILVER = 'silver',
  GOLD = 'gold',
  PLATINUM = 'platinum',
  DIAMOND = 'diamond',
  MASTER = 'master',
  GRANDMASTER = 'grandmaster',
  CHALLENGER = 'challenger',
}

export enum RankDivision {
  V = 'V',
  IV = 'IV',
  III = 'III',
  II = 'II',
  I = 'I',
}

export interface PlayerStatistics {
  gamesPlayed: number;
  gamesWon: number;
  gamesLost: number;
  gamesDraw: number;
  winRate: number;
  currentWinStreak: number;
  currentLoseStreak: number;
  longestWinStreak: number;
  longestLoseStreak: number;
  totalPlayTime: number;
  averageGameDuration: number;
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
  objectivesCompleted: number;
  fastestVictory: number;
  longestGame: number;
  perfectGames: number;
  comebackWins: number;
  comebackLosses: number;
  largestArmy: number;
  mostCredits: number;
  mostUnits: number;
  fastestWin: number;
}

export interface PlayerAchievements {
  unlocked: string[];
  progress: Map<string, AchievementProgress>;
  totalPoints: number;
  completionPercentage: number;
  categoryProgress: Map<AchievementCategory, CategoryProgress>;
  recentUnlocks: UnlockHistory[];
  favorites: string[];
}

export interface CategoryProgress {
  category: AchievementCategory;
  total: number;
  unlocked: number;
  percentage: number;
  points: number;
}

export interface UnlockHistory {
  achievementId: string;
  unlockedAt: number;
  timePlayed: number;
  gameMode: string;
}

export interface InventoryItem {
  id: string;
  type: InventoryItemType;
  itemId: string;
  quantity: number;
  quality: ItemQuality;
  acquiredAt: number;
  firstUsedAt?: number;
  uses: number;
  durability?: number;
  maxDurability?: number;
  equipped: boolean;
  soulbound: boolean;
  tradeable: boolean;
  expiresAt?: number;
  properties: Record<string, unknown>;
}

export enum InventoryItemType {
  SKIN = 'skin',
  BANNER = 'banner',
  BADGE = 'badge',
  EMOTE = 'emote',
  TITLE = 'title',
  FRAME = 'frame',
  MUSIC = 'music',
  VFX = 'vfx',
  CONSUMABLE = 'consumable',
  TOKEN = 'token',
}

export enum ItemQuality {
  COMMON = 1,
  UNCOMMON = 2,
  RARE = 3,
  EPIC = 4,
  LEGENDARY = 5,
  MYTHIC = 6,
  ULTIMATE = 7,
}

export interface Loadout {
  id: string;
  name: string;
  faction: string;
  units: LoadoutUnit[];
  buildings: LoadoutBuilding[];
  upgrades: string[];
  abilities: string[];
  isDefault: boolean;
  createdAt: number;
  lastUsed: number;
  wins: number;
  losses: number;
}

export interface LoadoutUnit {
  id: string;
  type: string;
  count: number;
  upgrades: string[];
  position: { x: number; y: number };
}

export interface LoadoutBuilding {
  id: string;
  type: string;
  position: { x: number; y: number };
  rallyPoint?: { x: number; y: number };
}

export interface UnlockedSkin {
  id: string;
  name: string;
  unitType: string;
  rarity: AchievementRarity;
  unlockedAt: number;
  unlockedVia: string;
  isFavorite: boolean;
}

export interface UnlockedBanner {
  id: string;
  name: string;
  pattern: string;
  color: string;
  rarity: AchievementRarity;
  unlockedAt: number;
  isEquipped: boolean;
}

export interface UnlockedBadge {
  id: string;
  name: string;
  description: string;
  icon: string;
  rarity: AchievementRarity;
  tier: number;
  unlockedAt: number;
  displayOrder: number;
}

export interface UnlockedEmote {
  id: string;
  name: string;
  icon: string;
  animation: string;
  unlockedAt: number;
  isFavorite: boolean;
}

export interface UnlockedTitle {
  id: string;
  name: string;
  unlockedAt: number;
  isActive: boolean;
  rarity: AchievementRarity;
}

export interface Friend {
  id: string;
  odId: string;
  name: string;
  avatar: string;
  level: number;
  status: FriendStatus;
  since: number;
  lastPlayed: number;
  mutualFriends: string[];
}

export enum FriendStatus {
  ONLINE = 'online',
  IN_GAME = 'in_game',
  AWAY = 'away',
  BUSY = 'busy',
  OFFLINE = 'offline',
}

export interface PlayerClan {
  id: string;
  name: string;
  tag: string;
  role: ClanRole;
  joinedAt: number;
  contributed: number;
  rank: number;
}

export enum ClanRole {
  RECRUIT = 'recruit',
  MEMBER = 'member',
  VETERAN = 'veteran',
  OFFICER = 'officer',
  LEADER = 'leader',
}

export interface PlayerSettings {
  audio: AudioSettings;
  video: VideoSettings;
  controls: ControlSettings;
  game: GameSettings;
  social: SocialSettings;
  accessibility: AccessibilitySettings;
}

export interface AudioSettings {
  masterVolume: number;
  musicVolume: number;
  sfxVolume: number;
  voiceVolume: number;
  ambientVolume: number;
  uiVolume: number;
  muted: boolean;
  pushToTalk: boolean;
  spatialAudio: boolean;
  audioQuality: AudioQuality;
  subtitleVolume: number;
}

export enum AudioQuality {
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high',
}

export interface VideoSettings {
  resolution: Resolution;
  fullscreen: boolean;
  borderless: boolean;
  vsync: boolean;
  antiAliasing: AntiAliasing;
  textureQuality: TextureQuality;
  shadowQuality: ShadowQuality;
  particleQuality: ParticleQuality;
  postProcessing: boolean;
  colorBlind: ColorBlindMode;
  fpsLimit: number;
  uiScale: number;
  brightness: number;
  contrast: number;
}

export interface Resolution {
  width: number;
  height: number;
  aspectRatio: string;
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
  OFF = 0,
  LOW = 0.25,
  MEDIUM = 0.5,
  HIGH = 0.75,
  ULTRA = 1,
}

export enum ColorBlindMode {
  NONE = 'none',
  PROTANOPIA = 'protanopia',
  DEUTERANOPIA = 'deuteranopia',
  TRITANOPIA = 'tritanopia',
  ACHROMATOPSIA = 'achromatopsia',
}

export interface ControlSettings {
  mouseSensitivity: number;
  scrollSpeed: number;
  invertY: boolean;
  keybindings: Keybindings;
  gamepadEnabled: boolean;
  gamepadSensitivity: number;
  edgeScrolling: boolean;
  middleMousePan: boolean;
  doubleClickSelect: boolean;
  clickDragSelect: boolean;
  rightClickMove: boolean;
  rightClickAttack: boolean;
  autoGroupUnits: boolean;
  quickCast: boolean;
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
  ability6: string;
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
  minimap: string;
  menu: string;
  chat: string;
  teamChat: string;
  whisper: string;
  pause: string;
  screenshot: string;
  fps: string;
  console: string;
  vote: string;
  surrender: string;
  quickSave: string;
  quickLoad: string;
}

export interface GameSettings {
  showHealthBars: boolean;
  healthBarScale: number;
  showDamage: boolean;
  showUnitNames: boolean;
  showFps: boolean;
  showPing: boolean;
  showTimer: boolean;
  autoSave: boolean;
  autoSaveInterval: number;
  autoPause: boolean;
  tutorialHints: boolean;
  confirmActions: boolean;
  queueOrders: boolean;
  autoBuild: boolean;
  autoRepair: boolean;
  autoUpgrade: boolean;
  showMinimapIcons: boolean;
  showResourceIcons: boolean;
  showAchievementNotifications: boolean;
  chatTimestamps: boolean;
  profanityFilter: boolean;
  radarRange: number;
  attackMove: boolean;
  smartSelect: boolean;
}

export interface SocialSettings {
  allowWhispers: boolean;
  allowFriendRequests: boolean;
  allowClanInvites: boolean;
  showOnlineStatus: boolean;
  showInGameStatus: boolean;
  showPlayTime: boolean;
  blockedPlayers: string[];
  mutedPlayers: string[];
  clanMessages: boolean;
  publicChat: boolean;
  partyInvites: boolean;
  duelRequests: boolean;
}

export interface AccessibilitySettings {
  screenReader: boolean;
  highContrast: boolean;
  largeText: boolean;
  reduceMotion: boolean;
  reduceNoise: boolean;
  subtitles: boolean;
  subtitleSize: SubtitleSize;
  subtitleColor: string;
  audioDescriptions: boolean;
  closedCaptions: boolean;
  keybindings: AccessibilityKeybindings;
  colorFilters: ColorFilters;
}

export enum SubtitleSize {
  SMALL = 'small',
  MEDIUM = 'medium',
  LARGE = 'large',
}

export interface AccessibilityKeybindings {
  tapToMove: boolean;
  swipeToCamera: boolean;
  holdToAttack: boolean;
  doubleTapToRun: boolean;
  toggleMode: boolean;
}

export interface ColorFilters {
  enabled: boolean;
  type: ColorBlindMode;
  intensity: number;
  customColors?: Record<string, string>;
}

export interface PlayerPreferences {
  language: string;
  region: string;
  timezone: string;
  dateFormat: string;
  timeFormat: TimeFormat;
  units: UnitSystem;
  currency: Currency;
}

export enum TimeFormat {
  TWELVE_HOUR = '12h',
  TWENTY_FOUR_HOUR = '24h',
}

export enum UnitSystem {
  METRIC = 'metric',
  IMPERIAL = 'imperial',
}

export enum Currency {
  USD = 'USD',
  EUR = 'EUR',
  GBP = 'GBP',
  JPY = 'JPY',
}

export interface PlayerStats {
  combat: CombatStats;
  economy: EconomyStats;
  social: SocialStats;
  missions: MissionStats;
  ranked: RankedStats;
}

export interface CombatStats {
  totalKills: number;
  totalDeaths: number;
  totalAssists: number;
  killDeathRatio: number;
  averageKills: number;
  averageDeaths: number;
  averageAssists: number;
  totalDamage: number;
  totalHealing: number;
  averageDamage: number;
  totalUnitsProduced: number;
  totalUnitsLost: number;
  totalBuildingsDestroyed: number;
  totalBuildingsLost: number;
  fastestWin: number;
  mostKillsInGame: number;
  mostDamageInGame: number;
}

export interface EconomyStats {
  totalCreditsEarned: number;
  totalCreditsSpent: number;
  totalResourcesGathered: number;
  averageResourcesPerMinute: number;
  totalHarvestingTime: number;
  totalProductionTime: number;
  averageIncome: number;
  peakCredits: number;
  fastestTech: number;
}

export interface SocialStats {
  totalFriends: number;
  totalClans: number;
  totalMessages: number;
  totalVoiceMinutes: number;
  totalTournaments: number;
  tournamentWins: number;
  totalParties: number;
}

export interface MissionStats {
  totalMissionsPlayed: number;
  missionsCompleted: number;
  missionsFailed: number;
  totalStarsEarned: number;
  perfectMissions: number;
  fastestMission: number;
  favoriteMission: string;
}

export interface RankedStats {
  currentRating: number;
  peakRating: number;
  totalRankedGames: number;
  rankedWins: number;
  rankedLosses: number;
  winRate: number;
  currentStreak: number;
  longestWinStreak: number;
  longestLoseStreak: number;
  gamesInCurrentRank: number;
  promotions: number;
  demotions: number;
}

export interface PlayerHistory {
  recentMatches: MatchRecord[];
  matchHistory: MatchRecord[];
  achievementsUnlocked: UnlockEvent[];
  rankedSeasons: RankedSeason[];
  milestones: MilestoneEvent[];
}

export interface MatchRecord {
  id: string;
  date: number;
  mapName: string;
  gameMode: string;
  result: MatchResult;
  duration: number;
  score: number;
  ratingChange: number;
  faction: string;
  teammates: string[];
  opponents: string[];
  stats: MatchStats;
}

export enum MatchResult {
  WIN = 'win',
  LOSS = 'loss',
  DRAW = 'draw',
  SURRENDER = 'surrender',
  DISCONNECT = 'disconnect',
  ABANDON = 'abandon',
}

export interface MatchStats {
  kills: number;
  deaths: number;
  assists: number;
  damage: number;
  healing: number;
  unitsProduced: number;
  unitsLost: number;
  buildingsBuilt: number;
  buildingsDestroyed: number;
  resourcesGathered: number;
  resourcesSpent: number;
  rating: number;
}

export interface UnlockEvent {
  type: UnlockEventType;
  id: string;
  timestamp: number;
  source: string;
  description: string;
}

export enum UnlockEventType {
  ACHIEVEMENT = 'achievement',
  LEVEL_UP = 'level_up',
  RANK_UP = 'rank_up',
  TITLE_UNLOCK = 'title_unlock',
  SKIN_UNLOCK = 'skin_unlock',
  BADGE_UNLOCK = 'badge_unlock',
  MILESTONE = 'milestone',
}

export interface RankedSeason {
  id: string;
  name: string;
  startDate: number;
  endDate: number;
  startingRank: number;
  endingRank: number;
  peakRank: number;
  gamesPlayed: number;
  wins: number;
  losses: number;
  rewardEarned: boolean;
}

export interface MilestoneEvent {
  type: MilestoneType;
  name: string;
  description: string;
  timestamp: number;
  value: number;
  rewards: AchievementReward[];
}

export enum MilestoneType {
  FIRST_WIN = 'first_win',
  GAMES_PLAYED = 'games_played',
  HOURS_PLAYED = 'hours_played',
  RANK_REACHED = 'rank_reached',
  ACHIEVEMENTS_UNLOCKED = 'achievements_unlocked',
}

export interface PlayerProgression {
  levels: LevelData[];
  currentLevel: number;
  currentXp: number;
  lifetimeXp: number;
  rankProgress: RankProgress[];
  seasonProgress: SeasonProgress;
  battlePass: BattlePassProgress;
}

export interface LevelData {
  level: number;
  xpRequired: number;
  rewards: AchievementReward[];
  unlockedAt?: number;
}

export interface RankProgress {
  tier: RankTier;
  division: RankDivision;
  points: number;
  pointsToNext: number;
  gamesPlayed: number;
  gamesWon: number;
  gamesLost: number;
}

export interface SeasonProgress {
  seasonId: string;
  currentTier: number;
  currentPoints: number;
  rewardsClaimed: string[];
  dailyQuests: DailyQuest[];
  weeklyQuests: WeeklyQuest[];
}

export interface DailyQuest {
  id: string;
  name: string;
  description: string;
  progress: number;
  target: number;
  reward: AchievementReward;
  completed: boolean;
  expiresAt: number;
}

export interface WeeklyQuest {
  id: string;
  name: string;
  description: string;
  progress: number;
  target: number;
  reward: AchievementReward[];
  completed: boolean;
  expiresAt: number;
}

export interface BattlePassProgress {
  level: number;
  xp: number;
  premiumUnlocked: boolean;
  tiersClaimed: number;
  totalTiers: number;
  rewardsPending: AchievementReward[];
}

export interface PlayerRewards {
  dailyLogin: DailyLoginReward;
  streak: LoginStreak;
  referral: ReferralRewards;
  seasonal: SeasonalRewards;
  firstPurchase: FirstPurchaseRewards;
}

export interface DailyLoginReward {
  lastClaimed: number;
  dayNumber: number;
  nextReward: AchievementReward;
  canClaim: boolean;
  history: number[];
}

export interface LoginStreak {
  current: number;
  longest: number;
  rewards: AchievementReward[];
  milestoneReached: number;
  activeBonus: number;
}

export interface ReferralRewards {
  code: string;
  uses: number;
  rewardsEarned: AchievementReward[];
  pendingRewards: AchievementReward[];
}

export interface SeasonalRewards {
  current: AchievementReward[];
  upcoming: AchievementReward[];
  claimed: string[];
}

export interface FirstPurchaseRewards {
  purchased: boolean;
  bonusCredits: number;
  bonusXp: number;
  bonusItems: AchievementReward[];
}

export interface Activity {
  type: ActivityType;
  timestamp: number;
  description: string;
  metadata: Record<string, unknown>;
}

export enum ActivityType {
  LOGIN = 'login',
  LOGOUT = 'logout',
  MATCH_STARTED = 'match_started',
  MATCH_ENDED = 'match_ended',
  ACHIEVEMENT_UNLOCKED = 'achievement_unlocked',
  LEVEL_UP = 'level_up',
  RANK_UP = 'rank_up',
  FRIEND_ADDED = 'friend_added',
  CLAN_JOINED = 'clan_joined',
}

export class AchievementManager {
  private achievements: Map<string, Achievement> = new Map();
  private unlocked: Set<string> = new Set();
  private progress: Map<string, AchievementProgress> = new Map();
  private eventCallbacks: Map<string, Set<Function>> = new Map();
  private listeners: Map<string, Function> = new Map();

  constructor() {
    this.initializeDefaultAchievements();
  }

  private initializeDefaultAchievements(): void {
    const defaultAchievements: Achievement[] = [
      {
        id: 'first_blood',
        name: 'First Blood',
        description: 'Destroy your first enemy unit',
        icon: 'achievement_first_blood',
        category: AchievementCategory.COMBAT,
        difficulty: AchievementDifficulty.EASY,
        rarity: AchievementRarity.COMMON,
        points: 10,
        secret: false,
        requirements: [{ type: RequirementType.KILL, target: 'unit', value: 1, operator: ComparisonOperator.GREATER_OR_EQUAL }],
        rewards: [{ type: RewardType.XP, id: 'xp_10', quantity: 10 }],
        metadata: { displayOrder: 1, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 1, prestige: 0, relatedAchievements: [], hints: [], flavorText: 'Every journey begins with a single step.', funFact: 'The first unit destroyed in competitive play was a Scout in 1992.' },
      },
      {
        id: 'war_machine',
        name: 'War Machine',
        description: 'Produce 1000 units',
        icon: 'achievement_war_machine',
        category: AchievementCategory.COMBAT,
        difficulty: AchievementDifficulty.HARD,
        rarity: AchievementRarity.RARE,
        points: 100,
        secret: false,
        requirements: [{ type: RequirementType.BUILD, target: 'unit', value: 1000, operator: ComparisonOperator.GREATER_OR_EQUAL }],
        rewards: [{ type: RewardType.XP, id: 'xp_500', quantity: 500 }, { type: RewardType.TITLE, id: 'warlord' }],
        metadata: { displayOrder: 10, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 5, prestige: 0, relatedAchievements: ['army_builder', 'mobilization'], hints: [], flavorText: 'An army of one... thousand.', funFact: 'Professional players average 800 units per game.' },
      },
      {
        id: 'economic_genius',
        name: 'Economic Genius',
        description: 'Accumulate 100,000 credits in a single game',
        icon: 'achievement_economic_genius',
        category: AchievementCategory.ECONOMY,
        difficulty: AchievementDifficulty.MEDIUM,
        rarity: AchievementRarity.UNCOMMON,
        points: 50,
        secret: false,
        requirements: [{ type: RequirementType.GATHER, target: 'credits', value: 100000, operator: ComparisonOperator.GREATER_OR_EQUAL }],
        rewards: [{ type: RewardType.XP, id: 'xp_250', quantity: 250 }, { type: RewardType.CREDITS, id: 'credits_5000', quantity: 5000 }],
        metadata: { displayOrder: 5, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 3, prestige: 0, relatedAchievements: ['scrooge', 'treasurer'], hints: [], flavorText: 'Money makes the world go round.', funFact: 'The average game produces 50,000 credits.' },
      },
      {
        id: 'flawless_victory',
        name: 'Flawless Victory',
        description: 'Win a game without losing any units',
        icon: 'achievement_flawless',
        category: AchievementCategory.COMBAT,
        difficulty: AchievementDifficulty.EXTREME,
        rarity: AchievementRarity.EPIC,
        points: 200,
        secret: false,
        requirements: [
          { type: RequirementType.WIN, target: 'game', value: 1, operator: ComparisonOperator.EQUALS },
          { type: RequirementType.KILL, target: 'units_lost', value: 0, operator: ComparisonOperator.EQUALS },
        ],
        rewards: [{ type: RewardType.XP, id: 'xp_1000', quantity: 1000 }, { type: RewardType.BADGE, id: 'badge_flawless' }],
        metadata: { displayOrder: 20, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 8, prestige: 1, relatedAchievements: ['unstoppable', 'perfect_game'], hints: ['Focus on defense and economic timing'], flavorText: 'Victory without sacrifice.', funFact: 'Only 0.1% of games end with zero unit losses.' },
      },
      {
        id: 'speed_demon',
        name: 'Speed Demon',
        description: 'Win a game in under 5 minutes',
        icon: 'achievement_speed',
        category: AchievementCategory.SPEED,
        difficulty: AchievementDifficulty.EXTREME,
        rarity: AchievementRarity.LEGENDARY,
        points: 300,
        secret: false,
        requirements: [
          { type: RequirementType.TIME, target: 'game_duration', value: 300, operator: ComparisonOperator.LESS_OR_EQUAL },
          { type: RequirementType.WIN, target: 'game', value: 1, operator: ComparisonOperator.EQUALS },
        ],
        rewards: [{ type: RewardType.XP, id: 'xp_1500', quantity: 1500 }, { type: RewardType.TITLE, id: 'speed_demon' }, { type: RewardType.SKIN, id: 'speed_skin' }],
        metadata: { displayOrder: 25, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 10, prestige: 2, relatedAchievements: ['rush_hour', 'lightning_war'], hints: ['Rush strategies work best'], flavorText: 'Time waits for no one.', funFact: 'The world record for fastest win is 2:34.' },
      },
      {
        id: 'legendary_commander',
        name: 'Legendary Commander',
        description: 'Reach rank Grandmaster',
        icon: 'achievement_legendary',
        category: AchievementCategory.SPECIAL,
        difficulty: AchievementDifficulty.IMPOSSIBLE,
        rarity: AchievementRarity.ULTRA_RARE,
        points: 1000,
        secret: false,
        requirements: [{ type: RequirementType.CUSTOM, target: 'rank', value: 1, operator: ComparisonOperator.GREATER_OR_EQUAL, nested: [] }],
        rewards: [{ type: RewardType.XP, id: 'xp_5000', quantity: 5000 }, { type: RewardType.TITLE, id: 'legendary' }, { type: RewardType.BADGE, id: 'badge_legendary' }, { type: RewardType.BANNER, id: 'banner_legendary' }],
        metadata: { displayOrder: 100, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 20, prestige: 5, relatedAchievements: [], hints: [], flavorText: 'True mastery knows no bounds.', funFact: 'Less than 0.01% of players reach Grandmaster.' },
      },
      {
        id: 'world_explorer',
        name: 'World Explorer',
        description: 'Reveal 100% of the map',
        icon: 'achievement_explorer',
        category: AchievementCategory.EXPLORATION,
        difficulty: AchievementDifficulty.MEDIUM,
        rarity: AchievementRarity.UNCOMMON,
        points: 75,
        secret: false,
        requirements: [{ type: RequirementType.EXPLORE, target: 'map_coverage', value: 100, operator: ComparisonOperator.GREATER_OR_EQUAL }],
        rewards: [{ type: RewardType.XP, id: 'xp_300', quantity: 300 }, { type: RewardType.TITLE, id: 'explorer' }],
        metadata: { displayOrder: 8, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 4, prestige: 0, relatedAchievements: ['scout', 'cartographer'], hints: [], flavorText: 'The world is a book.', funFact: 'Average map coverage is 65%.' },
      },
      {
        id: 'tech_master',
        name: 'Tech Master',
        description: 'Research all technologies',
        icon: 'achievement_tech',
        category: AchievementCategory.RESEARCH,
        difficulty: AchievementDifficulty.HARD,
        rarity: AchievementRarity.RARE,
        points: 150,
        secret: false,
        requirements: [{ type: RequirementType.RESEARCH, target: 'all_tech', value: 1, operator: ComparisonOperator.EQUALS }],
        rewards: [{ type: RewardType.XP, id: 'xp_750', quantity: 750 }, { type: RewardType.CREDITS, id: 'credits_10000', quantity: 10000 }],
        metadata: { displayOrder: 15, releaseVersion: '1.0.0', releaseDate: Date.now(), tier: 6, prestige: 1, relatedAchievements: ['research_queen', 'tech_advantage'], hints: [], flavorText: 'Knowledge is power.', funFact: 'There are 47 technologies to research.' },
      },
    ];

    defaultAchievements.forEach(achievement => {
      this.achievements.set(achievement.id, achievement);
    });
  }

  public registerAchievement(achievement: Achievement): void {
    this.achievements.set(achievement.id, achievement);
    this.emit('achievement_registered', { achievement });
  }

  public unregisterAchievement(achievementId: string): boolean {
    const achievement = this.achievements.get(achievementId);
    if (achievement) {
      this.achievements.delete(achievementId);
      this.emit('achievement_unregistered', { achievementId });
      return true;
    }
    return false;
  }

  public updateProgress(eventType: string, target: string, value: number): void {
    this.achievements.forEach((achievement, id) => {
      if (this.unlocked.has(id)) return;

      achievement.requirements.forEach(req => {
        if (req.type !== eventType && req.type !== RequirementType.CUSTOM) return;
        if (req.target !== target) return;

        let progress = this.progress.get(id);
        if (!progress) {
          progress = {
            achievementId: id,
            currentValue: 0,
            targetValue: req.value,
            percentage: 0,
            startedAt: Date.now(),
            updatedAt: Date.now(),
            completed: false,
            notificationShown: false,
            hiddenProgress: achievement.secret,
          };
          this.progress.set(id, progress);
        }

        progress.currentValue += value;
        progress.updatedAt = Date.now();
        progress.percentage = Math.min(100, (progress.currentValue / req.value) * 100);

        if (this.checkRequirement(req, progress.currentValue)) {
          if (this.checkAllRequirements(achievement)) {
            this.unlockAchievement(id);
          }
        }

        this.emit('progress_updated', { achievementId: id, progress });
      });
    });
  }

  private checkRequirement(req: AchievementRequirement, currentValue: number): boolean {
    switch (req.operator) {
      case ComparisonOperator.EQUALS:
        return currentValue === req.value;
      case ComparisonOperator.NOT_EQUALS:
        return currentValue !== req.value;
      case ComparisonOperator.GREATER_THAN:
        return currentValue > req.value;
      case ComparisonOperator.LESS_THAN:
        return currentValue < req.value;
      case ComparisonOperator.GREATER_OR_EQUAL:
        return currentValue >= req.value;
      case ComparisonOperator.LESS_OR_EQUAL:
        return currentValue <= req.value;
      case ComparisonOperator.MODULO:
        return currentValue % req.value === 0;
      default:
        return false;
    }
  }

  private checkAllRequirements(achievement: Achievement): boolean {
    return achievement.requirements.every(req => {
      const progress = this.progress.get(achievement.id);
      if (!progress) return false;
      return this.checkRequirement(req, progress.currentValue);
    });
  }

  public unlockAchievement(achievementId: string): boolean {
    const achievement = this.achievements.get(achievementId);
    if (!achievement || this.unlocked.has(achievementId)) return false;

    this.unlocked.add(achievementId);

    const progress = this.progress.get(achievementId);
    if (progress) {
      progress.completed = true;
      progress.completedAt = Date.now();
    }

    achievement.rewards.forEach(reward => {
      this.grantReward(reward);
    });

    this.emit('achievement_unlocked', { achievement });
    this.emit('notification', {
      type: 'achievement',
      title: achievement.name,
      message: achievement.description,
      icon: achievement.icon,
      points: achievement.points,
    });

    return true;
  }

  private grantReward(reward: AchievementReward): void {
    this.emit('reward_granted', { reward });
  }

  public getAchievement(achievementId: string): Achievement | undefined {
    return this.achievements.get(achievementId);
  }

  public getAllAchievements(): Achievement[] {
    return Array.from(this.achievements.values());
  }

  public getUnlockedAchievements(): Achievement[] {
    return Array.from(this.unlocked).map(id => this.achievements.get(id)).filter(Boolean) as Achievement[];
  }

  public getLockedAchievements(): Achievement[] {
    return Array.from(this.achievements.values()).filter(a => !this.unlocked.has(a.id));
  }

  public getProgress(achievementId: string): AchievementProgress | undefined {
    return this.progress.get(achievementId);
  }

  public getAllProgress(): AchievementProgress[] {
    return Array.from(this.progress.values());
  }

  public isUnlocked(achievementId: string): boolean {
    return this.unlocked.has(achievementId);
  }

  public getTotalPoints(): number {
    return this.getUnlockedAchievements().reduce((sum, a) => sum + a.points, 0);
  }

  public getCompletionPercentage(): number {
    const total = this.achievements.size;
    const unlocked = this.unlocked.size;
    return total > 0 ? (unlocked / total) * 100 : 0;
  }

  public getCategoryProgress(category: AchievementCategory): CategoryProgress {
    const categoryAchievements = Array.from(this.achievements.values()).filter(a => a.category === category);
    const unlocked = categoryAchievements.filter(a => this.unlocked.has(a.id));

    return {
      category,
      total: categoryAchievements.length,
      unlocked: unlocked.length,
      percentage: categoryAchievements.length > 0 ? (unlocked.length / categoryAchievements.length) * 100 : 0,
      points: unlocked.reduce((sum, a) => sum + a.points, 0),
    };
  }

  public resetProgress(): void {
    this.unlocked.clear();
    this.progress.clear();
    this.emit('progress_reset');
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
    this.achievements.clear();
    this.unlocked.clear();
    this.progress.clear();
    this.eventCallbacks.forEach(set => set.clear());
    this.eventCallbacks.clear();
    this.listeners.clear();
  }
}

export class PlayerProfileManager {
  private profile: PlayerProfile;
  private eventCallbacks: Map<string, Set<Function>> = new Map();
  private statsHistory: Map<string, number[]> = new Map();

  constructor(profile?: Partial<PlayerProfile>) {
    this.profile = this.createDefaultProfile(profile);
  }

  private createDefaultProfile(partial?: Partial<PlayerProfile>): PlayerProfile {
    return {
      id: partial?.id || `player_${Date.now()}`,
      username: partial?.username || 'Player',
      displayName: partial?.displayName || 'Player',
      avatar: partial?.avatar || '',
      level: partial?.level || 1,
      xp: partial?.xp || 0,
      xpToNextLevel: partial?.xpToNextLevel || 100,
      rank: partial?.rank || { current: 1, title: 'Beginner', tier: RankTier.BRONZE, division: RankDivision.V, ladderPoints: 0, seasonWins: 0, seasonLosses: 0, peakRank: 1, rankName: 'Bronze V' },
      title: partial?.title || '',
      prestige: partial?.prestige || 0,
      stats: partial?.stats || this.createDefaultStats(),
      achievements: partial?.achievements || { unlocked: [], progress: new Map(), totalPoints: 0, completionPercentage: 0, categoryProgress: new Map(), recentUnlocks: [], favorites: [] },
      inventory: partial?.inventory || [],
      loadouts: partial?.loadouts || [],
      skins: partial?.skins || [],
      banners: partial?.banners || [],
      badges: partial?.badges || [],
      emotes: partial?.emotes || [],
      titles: partial?.titles || [],
      friends: partial?.friends || [],
      clans: partial?.clans || [],
      settings: partial?.settings || this.createDefaultSettings(),
      preferences: partial?.preferences || this.createDefaultPreferences(),
      statistics: partial?.statistics || this.createDefaultStatistics(),
      history: partial?.history || { recentMatches: [], matchHistory: [], achievementsUnlocked: [], rankedSeasons: [], milestones: [] },
      progression: partial?.progression || this.createDefaultProgression(),
      rewards: partial?.rewards || this.createDefaultRewards(),
      createdAt: partial?.createdAt || Date.now(),
      lastOnline: partial?.lastOnline || Date.now(),
      totalPlayTime: partial?.totalPlayTime || 0,
      recentActivity: partial?.recentActivity || [],
    };
  }

  private createDefaultStats(): PlayerStatistics {
    return {
      gamesPlayed: 0,
      gamesWon: 0,
      gamesLost: 0,
      gamesDraw: 0,
      winRate: 0,
      currentWinStreak: 0,
      currentLoseStreak: 0,
      longestWinStreak: 0,
      longestLoseStreak: 0,
      totalPlayTime: 0,
      averageGameDuration: 0,
      unitsProduced: 0,
      unitsLost: 0,
      unitsKilled: 0,
      buildingsBuilt: 0,
      buildingsLost: 0,
      buildingsDestroyed: 0,
      resourcesGathered: 0,
      resourcesSpent: 0,
      researchCompleted: 0,
      damageDealt: 0,
      damageReceived: 0,
      healingDone: 0,
      objectivesCompleted: 0,
      fastestVictory: Infinity,
      longestGame: 0,
      perfectGames: 0,
      comebackWins: 0,
      comebackLosses: 0,
      largestArmy: 0,
      mostCredits: 0,
      mostUnits: 0,
      fastestWin: Infinity,
    };
  }

  private createDefaultSettings(): PlayerSettings {
    return {
      audio: { masterVolume: 0.8, musicVolume: 0.7, sfxVolume: 0.8, voiceVolume: 1.0, ambientVolume: 0.6, uiVolume: 0.7, muted: false, pushToTalk: false, spatialAudio: true, audioQuality: AudioQuality.HIGH, subtitleVolume: 1.0 },
      video: { resolution: { width: 1920, height: 1080, aspectRatio: '16:9' }, fullscreen: true, borderless: false, vsync: true, antiAliasing: AntiAliasing.FXAA, textureQuality: TextureQuality.HIGH, shadowQuality: ShadowQuality.HIGH, particleQuality: ParticleQuality.HIGH, postProcessing: true, colorBlind: ColorBlindMode.NONE, fpsLimit: 60, uiScale: 1.0, brightness: 1.0, contrast: 1.0 },
      controls: { mouseSensitivity: 1.0, scrollSpeed: 1.0, invertY: false, keybindings: {}, gamepadEnabled: false, gamepadSensitivity: 1.0, edgeScrolling: true, middleMousePan: true, doubleClickSelect: true, clickDragSelect: true, rightClickMove: true, rightClickAttack: true, autoGroupUnits: true, quickCast: false },
      game: { showHealthBars: true, healthBarScale: 1.0, showDamage: true, showUnitNames: false, showFps: true, showPing: true, showTimer: true, autoSave: true, autoSaveInterval: 300, autoPause: false, tutorialHints: true, confirmActions: false, queueOrders: true, autoBuild: false, autoRepair: true, autoUpgrade: false, showMinimapIcons: true, showResourceIcons: true, showAchievementNotifications: true, chatTimestamps: true, profanityFilter: true, radarRange: 100, attackMove: true, smartSelect: true },
      social: { allowWhispers: true, allowFriendRequests: true, allowClanInvites: true, showOnlineStatus: true, showInGameStatus: true, showPlayTime: true, blockedPlayers: [], mutedPlayers: [], clanMessages: true, publicChat: true, partyInvites: true, duelRequests: true },
      accessibility: { screenReader: false, highContrast: false, largeText: false, reduceMotion: false, reduceNoise: false, subtitles: false, subtitleSize: SubtitleSize.MEDIUM, subtitleColor: '#FFFFFF', audioDescriptions: false, closedCaptions: false, keybindings: { tapToMove: false, swipeToCamera: false, holdToAttack: false, doubleTapToRun: false, toggleMode: false }, colorFilters: { enabled: false, type: ColorBlindMode.NONE, intensity: 1.0 } },
    };
  }

  private createDefaultPreferences(): PlayerPreferences {
    return { language: 'en', region: 'us-west', timezone: 'PST', dateFormat: 'MM/DD/YYYY', timeFormat: TimeFormat.TWENTY_FOUR_HOUR, units: UnitSystem.METRIC, currency: Currency.USD };
  }

  private createDefaultStatistics(): PlayerStats {
    return {
      combat: { totalKills: 0, totalDeaths: 0, totalAssists: 0, killDeathRatio: 0, averageKills: 0, averageDeaths: 0, averageAssists: 0, totalDamage: 0, totalHealing: 0, averageDamage: 0, totalUnitsProduced: 0, totalUnitsLost: 0, totalBuildingsDestroyed: 0, totalBuildingsLost: 0, fastestWin: Infinity, mostKillsInGame: 0, mostDamageInGame: 0 },
      economy: { totalCreditsEarned: 0, totalCreditsSpent: 0, totalResourcesGathered: 0, averageResourcesPerMinute: 0, totalHarvestingTime: 0, totalProductionTime: 0, averageIncome: 0, peakCredits: 0, fastestTech: Infinity },
      social: { totalFriends: 0, totalClans: 0, totalMessages: 0, totalVoiceMinutes: 0, totalTournaments: 0, tournamentWins: 0, totalParties: 0 },
      missions: { totalMissionsPlayed: 0, missionsCompleted: 0, missionsFailed: 0, totalStarsEarned: 0, perfectMissions: 0, fastestMission: Infinity, favoriteMission: '' },
      ranked: { currentRating: 1000, peakRating: 1000, totalRankedGames: 0, rankedWins: 0, rankedLosses: 0, winRate: 0, currentStreak: 0, longestWinStreak: 0, longestLoseStreak: 0, gamesInCurrentRank: 0, promotions: 0, demotions: 0 },
    };
  }

  private createDefaultProgression(): PlayerProgression {
    return { levels: [], currentLevel: 1, currentXp: 0, lifetimeXp: 0, rankProgress: [], seasonProgress: { seasonId: '', currentTier: 1, currentPoints: 0, rewardsClaimed: [], dailyQuests: [], weeklyQuests: [] }, battlePass: { level: 1, xp: 0, premiumUnlocked: false, tiersClaimed: 0, totalTiers: 100, rewardsPending: [] } };
  }

  private createDefaultRewards(): PlayerRewards {
    return { dailyLogin: { lastClaimed: 0, dayNumber: 0, nextReward: { type: RewardType.XP, id: 'xp_10' }, canClaim: false, history: [] }, streak: { current: 0, longest: 0, rewards: [], milestoneReached: 0, activeBonus: 0 }, referral: { code: '', uses: 0, rewardsEarned: [], pendingRewards: [] }, seasonal: { current: [], upcoming: [], claimed: [] }, firstPurchase: { purchased: false, bonusCredits: 0, bonusXp: 0, bonusItems: [] } };
  }

  public getProfile(): PlayerProfile {
    return { ...this.profile };
  }

  public updateProfile(updates: Partial<PlayerProfile>): void {
    this.profile = { ...this.profile, ...updates };
    this.emit('profile_updated', { profile: this.profile });
  }

  public addXp(amount: number): void {
    this.profile.xp += amount;
    this.profile.lifetimeXp += amount;

    while (this.profile.xp >= this.profile.xpToNextLevel) {
      this.profile.xp -= this.profile.xpToNextLevel;
      this.levelUp();
    }

    this.emit('xp_gained', { amount, totalXp: this.profile.xp });
  }

  private levelUp(): void {
    this.profile.level++;
    this.profile.xpToNextLevel = Math.floor(this.profile.xpToNextLevel * 1.5);
    this.emit('level_up', { level: this.profile.level });
  }

  public recordMatch(result: MatchResult, stats: Partial<MatchStats>): void {
    const record: MatchRecord = {
      id: `match_${Date.now()}`,
      date: Date.now(),
      mapName: stats.mapName || 'Unknown',
      gameMode: stats.gameMode || 'Skirmish',
      result,
      duration: stats.duration || 0,
      score: stats.score || 0,
      ratingChange: stats.ratingChange || 0,
      faction: stats.faction || 'Default',
      teammates: stats.teammates || [],
      opponents: stats.opponents || [],
      stats: stats as MatchStats,
    };

    this.profile.history.recentMatches.unshift(record);
    if (this.profile.history.recentMatches.length > 20) {
      this.profile.history.recentMatches.pop();
    }

    this.profile.history.matchHistory.push(record);

    this.updateStatistics(record);
    this.emit('match_recorded', { record });
  }

  private updateStatistics(record: MatchRecord): void {
    this.profile.stats.gamesPlayed++;
    if (record.result === MatchResult.WIN) this.profile.stats.gamesWon++;
    if (record.result === MatchResult.LOSS) this.profile.stats.gamesLost++;
    this.profile.stats.winRate = this.profile.stats.gamesWon / this.profile.stats.gamesPlayed * 100;
    this.profile.stats.currentWinStreak = record.result === MatchResult.WIN ? this.profile.stats.currentWinStreak + 1 : 0;
    this.profile.stats.currentLoseStreak = record.result === MatchResult.LOSS ? this.profile.stats.currentLoseStreak + 1 : 0;
    this.profile.stats.longestWinStreak = Math.max(this.profile.stats.longestWinStreak, this.profile.stats.currentWinStreak);
  }

  public getSetting<K extends keyof PlayerSettings>(category: K): PlayerSettings[K] {
    return this.profile.settings[category];
  }

  public updateSetting<K extends keyof PlayerSettings>(category: K, value: PlayerSettings[K]): void {
    this.profile.settings[category] = value;
    this.emit('setting_changed', { category, value });
  }

  public addFriend(friend: Friend): void {
    this.profile.friends.push(friend);
    this.profile.statistics.social.totalFriends++;
    this.emit('friend_added', { friend });
  }

  public removeFriend(friendId: string): void {
    this.profile.friends = this.profile.friends.filter(f => f.id !== friendId);
    this.profile.statistics.social.totalFriends--;
    this.emit('friend_removed', { friendId });
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
    this.eventCallbacks.forEach(set => set.clear());
    this.eventCallbacks.clear();
    this.statsHistory.clear();
  }
}

export const createAchievementManager = (): AchievementManager => {
  return new AchievementManager();
};

export const createPlayerProfileManager = (profile?: Partial<PlayerProfile>): PlayerProfileManager => {
  return new PlayerProfileManager(profile);
};

export default AchievementManager;

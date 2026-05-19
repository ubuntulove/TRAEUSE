export interface CampaignSystem {
  id: string;
  name: string;
  description: string;
  campaigns: Campaign[];
  currentCampaignId: string | null;
  currentMissionId: string | null;
  progress: CampaignProgress;
  settings: CampaignSettings;
}

export interface Campaign {
  id: string;
  name: string;
  description: string;
  faction: Faction;
  difficulty: CampaignDifficulty;
  missions: Mission[];
  prerequisites: string[];
  rewards: CampaignRewards;
  unlocks: CampaignUnlocks;
  story: StoryNode[];
  metadata: CampaignMetadata;
}

export interface Mission {
  id: string;
  name: string;
  description: string;
  type: MissionType;
  map: MissionMap;
  objectives: Objective[];
  enemies: MissionEnemy[];
  allies: MissionAlly[];
  resources: MissionResources;
  timeLimit: number | null;
  scoreTarget: number | null;
  difficulty: MissionDifficulty;
  rewards: MissionRewards;
  cinematics: CinematicData[];
  dialogue: DialogueLine[];
  briefings: MissionBriefing;
  debriefings: MissionDebriefing;
  trivia: MissionTrivia;
  statistics: MissionStatistics;
}

export enum MissionType {
  TUTORIAL = 'tutorial',
  ASSAULT = 'assault',
  DEFENSE = 'defense',
  ESCORT = 'escort',
  CAPTURE = 'capture',
  DESTROY = 'destroy',
  SURVIVAL = 'survival',
  STEALTH = 'stealth',
  RACING = 'racing',
  PUZZLE = 'puzzle',
  ESCAPE = 'escape',
  EXTRACTION = 'extraction',
  BOMBING = 'bombing',
  RECONNAISSANCE = 'reconnaissance',
  SABOTAGE = 'sabotage',
  RESCUE = 'rescue',
  INFILTRATION = 'infiltration',
  GARRISON = 'garrison',
  INTERCEPT = 'intercept',
  PURSUIT = 'pursuit',
}

export enum CampaignDifficulty {
  RECRUIT = 'recruit',
  VETERAN = 'veteran',
  COMMANDER = 'commander',
  GENERAL = 'general',
  LEGENDARY = 'legendary',
}

export interface MissionMap {
  id: string;
  name: string;
  file: string;
  width: number;
  height: number;
  tileset: string;
  preview: string;
  environment: string;
  weather: string;
  timeOfDay: string;
  music: string;
}

export interface Objective {
  id: string;
  name: string;
  description: string;
  type: ObjectiveType;
  target: ObjectiveTarget;
  progress: number;
  required: number;
  completed: boolean;
  bonus: boolean;
  bonusCondition: string | null;
  hidden: boolean;
  secondary: boolean;
  failures: FailureCondition[];
  triggers: ObjectiveTrigger[];
}

export enum ObjectiveType {
  PRIMARY = 'primary',
  SECONDARY = 'secondary',
  BONUS = 'bonus',
  HIDDEN = 'hidden',
}

export interface ObjectiveTarget {
  entityId: string | null;
  entityType: string | null;
  location: { x: number; y: number } | null;
  radius: number;
  quantity: number;
  killCount: number;
  buildCount: number;
  collectCount: number;
}

export interface FailureCondition {
  type: FailureType;
  condition: string;
  message: string;
  action: FailureAction;
}

export enum FailureType {
  UNIT_LOST = 'unit_lost',
  BUILDING_DESTROYED = 'building_destroyed',
  TIME_EXPIRED = 'time_expired',
  ABANDONED = 'abandoned',
  TRIGGERED = 'triggered',
}

export enum FailureAction {
  FAIL_MISSION = 'fail_mission',
  FAIL_OBJECTIVE = 'fail_objective',
  WARN_PLAYER = 'warn_player',
  PAUSE = 'pause',
}

export interface ObjectiveTrigger {
  type: TriggerType;
  conditions: string[];
  actions: string[];
  delay: number;
  once: boolean;
}

export enum TriggerType {
  ON_COMPLETE = 'on_complete',
  ON_FAIL = 'on_fail',
  ON_TIMEOUT = 'on_timeout',
  ON_ENTER = 'on_enter',
  ON_DESTROY = 'on_destroy',
  ON_BUILD = 'on_build',
  ON_KILL = 'on_kill',
}

export interface MissionEnemy {
  faction: Faction;
  name: string;
  difficulty: CampaignDifficulty;
  aiProfile: string;
  behavior: EnemyBehavior;
  spawnPoints: SpawnPoint[];
  initialUnits: string[];
  initialBuildings: string[];
  reinforcements: Reinforcement[];
}

export interface EnemyBehavior {
  aggression: number;
  defense: number;
  resourceManagement: number;
  unitProduction: number;
  technology: number;
  tactics: string[];
  weaknesses: string[];
}

export interface SpawnPoint {
  id: string;
  x: number;
  y: number;
  faction: string;
  active: boolean;
  trigger: string | null;
}

export interface Reinforcement {
  id: string;
  delay: number;
  waves: ReinforcementWave[];
  trigger: string;
  repeatable: boolean;
  maxWaves: number;
}

export interface ReinforcementWave {
  units: string[];
  spawnPointId: string;
  formation: string;
  attackTarget: { x: number; y: number } | null;
}

export interface MissionAlly {
  faction: Faction;
  name: string;
  controlledBy: ControlType;
  aiProfile: string;
  spawnPoints: SpawnPoint[];
  initialUnits: string[];
  objectives: Objective[];
  personality: AllyPersonality;
}

export enum ControlType {
  PLAYER = 'player',
  AI = 'ai',
  NEUTRAL = 'neutral',
}

export interface AllyPersonality {
  helpfulness: number;
  aggression: number;
  bravery: number;
  loyalty: number;
  communication: number;
}

export interface MissionResources {
  startingCredits: number;
  startingOre: number;
  startingPower: number;
  startingUnits: StartingUnit[];
  startingBuildings: StartingBuilding[];
  capturePoints: CapturePoint[];
  harvesters: number;
}

export interface StartingUnit {
  type: string;
  x: number;
  y: number;
  health: number;
  experience: number;
  upgrades: string[];
}

export interface StartingBuilding {
  type: string;
  x: number;
  y: number;
  health: number;
  productionQueue: string[];
  upgrades: string[];
}

export interface CapturePoint {
  id: string;
  name: string;
  x: number;
  y: number;
  radius: number;
  captureTime: number;
  rewardCredits: number;
  faction: string | null;
  controllable: boolean;
}

export interface CinematicData {
  id: string;
  type: CinematicType;
  name: string;
  duration: number;
  scenes: CinematicScene[];
  skippable: boolean;
  conditions: string[];
}

export enum CinematicType {
  INTRO = 'intro',
  OUTRO = 'outro',
  MID_MISSION = 'mid_mission',
  FAILURE = 'failure',
  SUCCESS = 'success',
  TRANSITION = 'transition',
  REVEAL = 'reveal',
}

export interface CinematicScene {
  index: number;
  duration: number;
  camera: CameraData;
  entities: CinematicEntity[];
  effects: CinematicEffect[];
  audio: CinematicAudio[];
  text: CinematicText[];
  transitions: CinematicTransition[];
}

export interface CameraData {
  position: { x: number; y: number; z: number };
  rotation: { x: number; y: number; z: number };
  fov: number;
  zoom: number;
  target: { x: number; y: number; z: number } | null;
  interpolation: string;
  duration: number;
}

export interface CinematicEntity {
  id: string;
  type: string;
  action: string;
  position: { x: number; y: number; z: number };
  rotation: { x: number; y: number; z: number };
  scale: { x: number; y: number; z: number };
  animation: string;
  opacity: number;
  visible: boolean;
}

export interface CinematicEffect {
  type: EffectType;
  position: { x: number; y: number; z: number };
  parameters: Record<string, unknown>;
  startTime: number;
  endTime: number;
}

export enum EffectType {
  EXPLOSION = 'explosion',
  SMOKE = 'smoke',
  FIRE = 'fire',
  LIGHTNING = 'lightning',
  SCREEN_SHAKE = 'screen_shake',
  FLASH = 'flash',
  COLOR_CORRECTION = 'color_correction',
  FADE = 'fade',
  BLOOM = 'bloom',
  LENS_FLARE = 'lens_flare',
  PARTICLE = 'particle',
}

export interface CinematicAudio {
  type: AudioType;
  source: string;
  volume: number;
  pitch: number;
  startTime: number;
  endTime: number;
  position: { x: number; y: number; z: number } | null;
}

export enum AudioType {
  DIALOGUE = 'dialogue',
  MUSIC = 'music',
  SOUND_EFFECT = 'sound_effect',
  AMBIENT = 'ambient',
  VOICEOVER = 'voiceover',
}

export interface CinematicText {
  text: string;
  position: { x: number; y: number };
  font: string;
  size: number;
  color: { r: number; g: number; b: number; a: number };
  alignment: string;
  duration: number;
  delay: number;
  animation: string;
}

export interface CinematicTransition {
  type: TransitionType;
  duration: number;
  color: { r: number; g: number; b: number; a: number };
}

export enum TransitionType {
  NONE = 'none',
  FADE = 'fade',
  DISSOLVE = 'dissolve',
  WIPE = 'wipe',
  SLIDE = 'slide',
  ZOOM = 'zoom',
}

export interface DialogueLine {
  id: string;
  speaker: string;
  portrait: string | null;
  text: string;
  voiceLine: string | null;
  emotion: string;
  conditions: string[];
  triggers: string[];
  timing: DialogueTiming;
}

export interface DialogueTiming {
  displayDuration: number;
  autoAdvance: boolean;
  waitForInput: boolean;
  skipable: boolean;
}

export interface MissionBriefing {
  id: string;
  title: string;
  overview: string;
  objectives: BriefingObjective[];
  mapOverview: string;
  tacticalTips: string[];
  enemyIntel: EnemyIntel[];
  allyIntel: AllyIntel[];
  historicalContext: string;
  commanderDialogue: DialogueLine[];
  cutscenes: CinematicData[];
}

export interface BriefingObjective {
  id: string;
  name: string;
  description: string;
  type: ObjectiveType;
  visibility: 'shown' | 'hidden' | 'revealed';
}

export interface EnemyIntel {
  faction: string;
  strength: string;
  weaknesses: string[];
  knownUnits: string[];
  knownBuildings: string[];
  tactics: string[];
  commander: string;
}

export interface AllyIntel {
  faction: string;
  strengths: string[];
  capabilities: string[];
  limitations: string[];
  commander: string;
}

export interface MissionDebriefing {
  id: string;
  title: string;
  summary: string;
  statistics: DebriefingStatistics;
  rating: number;
  performance: PerformanceMetrics;
  rewardsEarned: MissionRewards;
  unlockedContent: string[];
  commanderDialogue: DialogueLine[];
  recommendations: string[];
}

export interface DebriefingStatistics {
  duration: number;
  unitsProduced: number;
  unitsLost: number;
  unitsDestroyed: number;
  buildingsBuilt: number;
  buildingsLost: number;
  buildingsDestroyed: number;
  resourcesGathered: number;
  resourcesSpent: number;
  damageDealt: number;
  damageReceived: number;
  accuracy: number;
  efficiency: number;
}

export interface PerformanceMetrics {
  combatRating: number;
  economyRating: number;
  strategyRating: number;
  efficiencyRating: number;
  speedRating: number;
  styleRating: number;
  overallRating: number;
}

export interface MissionTrivia {
  facts: string[];
  historicalNotes: string[];
  developerComments: string[];
  easterEggs: string[];
}

export interface MissionStatistics {
  totalPlays: number;
  completionRate: number;
  averageDuration: number;
  averageRating: number;
  winRate: number;
  failReasons: Record<string, number>;
  bestTime: number;
  bestScore: number;
  achievements: string[];
}

export interface CampaignRewards {
  credits: number;
  xp: number;
  unlocks: RewardUnlock[];
  bonusUnlocks: RewardUnlock[];
  achievements: Achievement[];
  rankings: RankingReward[];
}

export interface RewardUnlock {
  type: UnlockType;
  id: string;
  name: string;
  description: string;
  rarity: Rarity;
}

export enum UnlockType {
  UNIT = 'unit',
  BUILDING = 'building',
  UPGRADE = 'upgrade',
  ABILITY = 'ability',
  SKIN = 'skin',
  MAP = 'map',
  CAMPAIGN = 'campaign',
  FACTION = 'faction',
}

export enum Rarity {
  COMMON = 'common',
  UNCOMMON = 'uncommon',
  RARE = 'rare',
  EPIC = 'epic',
  LEGENDARY = 'legendary',
}

export interface Achievement {
  id: string;
  name: string;
  description: string;
  icon: string;
  points: number;
  rarity: Rarity;
  hidden: boolean;
  progress: number;
  target: number;
  unlocked: boolean;
  unlockedAt: number | null;
}

export interface RankingReward {
  rank: number;
  threshold: number;
  rewards: MissionRewards;
}

export interface CampaignUnlocks {
  nextMission: string | null;
  nextCampaign: string | null;
  units: string[];
  buildings: string[];
  upgrades: string[];
  abilities: string[];
  skins: string[];
  maps: string[];
  campaigns: string[];
  gameModes: string[];
  features: string[];
}

export interface StoryNode {
  id: string;
  type: StoryNodeType;
  text: string;
  speaker: string | null;
  portrait: string | null;
  choices: StoryChoice[];
  nextNode: string | null;
  conditions: string[];
  effects: StoryEffect[];
}

export enum StoryNodeType {
  DIALOGUE = 'dialogue',
  NARRATION = 'narration',
  CUTSCENE = 'cutscene',
  CHOICE = 'choice',
  BATTLE = 'battle',
  REWARD = 'reward',
  FAILURE = 'failure',
}

export interface StoryChoice {
  id: string;
  text: string;
  conditions: string[];
  effects: StoryEffect[];
  nextNode: string;
  rewards: MissionRewards | null;
}

export interface StoryEffect {
  type: EffectTarget;
  target: string;
  operation: string;
  value: unknown;
  conditions: string[];
}

export enum EffectTarget {
  VARIABLE = 'variable',
  FLAG = 'flag',
  UNLOCK = 'unlock',
  RELATIONSHIP = 'relationship',
  PROGRESS = 'progress',
}

export interface CampaignMetadata {
  author: string;
  version: string;
  createdAt: number;
  updatedAt: number;
  tags: string[];
  rating: number;
  downloads: number;
  completedBy: number;
  averageRating: number;
  userReviews: UserReview[];
  compatibility: string;
  language: string;
}

export interface UserReview {
  id: string;
  userId: string;
  rating: number;
  text: string;
  date: number;
  helpful: number;
  notHelpful: number;
}

export interface CampaignProgress {
  campaignId: string;
  missionProgress: Map<string, MissionProgressData>;
  unlockedMissions: Set<string>;
  completedMissions: Set<string>;
  failedMissions: Set<string>;
  skippedMissions: Set<string>;
  totalPlayTime: number;
  starsEarned: Map<string, number>;
  achievementsUnlocked: Set<string>;
  choicesMade: Map<string, string>;
  variables: Map<string, number>;
  flags: Map<string, boolean>;
  unlocks: Set<string>;
  rewardsClaimed: Set<string>;
}

export interface MissionProgressData {
  missionId: string;
  status: MissionStatus;
  bestTime: number | null;
  bestScore: number | null;
  attempts: number;
  wins: number;
  losses: number;
  lastPlayed: number | null;
  difficulty: CampaignDifficulty;
  rating: number | null;
  statistics: MissionStatistics;
  rewardsEarned: MissionRewards | null;
}

export enum MissionStatus {
  LOCKED = 'locked',
  UNLOCKED = 'unlocked',
  IN_PROGRESS = 'in_progress',
  COMPLETED = 'completed',
  FAILED = 'failed',
  SKIPPED = 'skipped',
  PERFECT = 'perfect',
}

export interface CampaignSettings {
  difficulty: CampaignDifficulty;
  autoSave: boolean;
  autoPause: boolean;
  showHints: boolean;
  skipCinematics: boolean;
  fastForward: boolean;
  cheatsEnabled: boolean;
  unlockedContent: boolean;
  language: string;
  subtitles: boolean;
  voiceOverVolume: number;
  musicVolume: number;
}

export interface Faction {
  id: string;
  name: string;
  abbreviation: string;
  description: string;
  leader: string;
  color: { r: number; g: number; b: number; a: number };
  emblem: string;
  strengths: string[];
  weaknesses: string[];
  units: string[];
  buildings: string[];
  abilities: string[];
  bonuses: FactionBonus[];
  lore: string;
  ideology: string;
  baseOfOperations: string;
}

export interface FactionBonus {
  id: string;
  name: string;
  description: string;
  effect: string;
  icon: string;
  stacks: boolean;
}

export interface CampaignManager {
  campaigns: Map<string, Campaign>;
  activeCampaign: Campaign | null;
  activeMission: Mission | null;
  progress: CampaignProgress;
  settings: CampaignSettings;
  saveSlots: CampaignSaveSlot[];
  statistics: CampaignStatistics;
  eventCallbacks: Map<string, Set<Function>>;
}

export interface CampaignSaveSlot {
  id: string;
  name: string;
  campaignId: string;
  currentMissionId: string;
  progress: CampaignProgress;
  timestamp: number;
  screenshot: string;
  metadata: Record<string, unknown>;
}

export interface CampaignStatistics {
  totalPlayTime: number;
  campaignsCompleted: number;
  missionsCompleted: number;
  missionsFailed: number;
  perfectMissions: number;
  totalStarsEarned: number;
  totalAchievements: number;
  totalCreditsEarned: number;
  favoriteFaction: string;
  favoriteMission: string;
  longestPlaySession: number;
  averageSessionLength: number;
}

export class CampaignManagerClass {
  private campaigns: Map<string, Campaign> = new Map();
  private activeCampaign: Campaign | null = null;
  private activeMission: Mission | null = null;
  private progress: CampaignProgress;
  private settings: CampaignSettings;
  private saveSlots: CampaignSaveSlot[] = [];
  private statistics: CampaignStatistics;
  private eventCallbacks: Map<string, Set<Function>> = new Map();
  private missionTimers: Map<string, number> = new Map();

  constructor() {
    this.progress = this.createInitialProgress();
    this.settings = this.createDefaultSettings();
    this.statistics = this.createInitialStatistics();
    this.initializeEventCallbacks();
  }

  private createInitialProgress(): CampaignProgress {
    return {
      campaignId: '',
      missionProgress: new Map(),
      unlockedMissions: new Set(),
      completedMissions: new Set(),
      failedMissions: new Set(),
      skippedMissions: new Set(),
      totalPlayTime: 0,
      starsEarned: new Map(),
      achievementsUnlocked: new Set(),
      choicesMade: new Map(),
      variables: new Map(),
      flags: new Map(),
      unlocks: new Set(),
      rewardsClaimed: new Set(),
    };
  }

  private createDefaultSettings(): CampaignSettings {
    return {
      difficulty: CampaignDifficulty.VETERAN,
      autoSave: true,
      autoPause: true,
      showHints: true,
      skipCinematics: false,
      fastForward: false,
      cheatsEnabled: false,
      unlockedContent: false,
      language: 'en',
      subtitles: true,
      voiceOverVolume: 1.0,
      musicVolume: 0.7,
    };
  }

  private createInitialStatistics(): CampaignStatistics {
    return {
      totalPlayTime: 0,
      campaignsCompleted: 0,
      missionsCompleted: 0,
      missionsFailed: 0,
      perfectMissions: 0,
      totalStarsEarned: 0,
      totalAchievements: 0,
      totalCreditsEarned: 0,
      favoriteFaction: '',
      favoriteMission: '',
      longestPlaySession: 0,
      averageSessionLength: 0,
    };
  }

  private initializeEventCallbacks(): void {
    const events = [
      'mission_start',
      'mission_complete',
      'mission_fail',
      'objective_complete',
      'objective_fail',
      'choice_made',
      'unlock_earned',
      'reward_claimed',
      'achievement_unlocked',
      'campaign_complete',
      'checkpoint_reached',
    ];

    events.forEach(event => {
      this.eventCallbacks.set(event, new Set());
    });
  }

  public addCampaign(campaign: Campaign): void {
    this.campaigns.set(campaign.id, campaign);

    if (this.activeCampaign === null) {
      this.activeCampaign = campaign;
      this.unlockMission(campaign.missions[0]?.id);
    }
  }

  public removeCampaign(campaignId: string): void {
    this.campaigns.delete(campaignId);
  }

  public setActiveCampaign(campaignId: string): boolean {
    const campaign = this.campaigns.get(campaignId);
    if (!campaign) {
      return false;
    }

    this.activeCampaign = campaign;
    this.activeMission = null;

    if (campaign.missions.length > 0) {
      this.unlockMission(campaign.missions[0].id);
    }

    this.emit('campaign_changed', { campaignId });
    return true;
  }

  public startMission(missionId: string): boolean {
    if (!this.activeCampaign) {
      return false;
    }

    const mission = this.activeCampaign.missions.find(m => m.id === missionId);
    if (!mission) {
      return false;
    }

    if (!this.isMissionUnlocked(missionId)) {
      return false;
    }

    this.activeMission = mission;
    this.missionTimers.set(missionId, 0);

    const missionProgress = this.progress.missionProgress.get(missionId);
    if (!missionProgress) {
      this.progress.missionProgress.set(missionId, {
        missionId,
        status: MissionStatus.IN_PROGRESS,
        bestTime: null,
        bestScore: null,
        attempts: 1,
        wins: 0,
        losses: 0,
        lastPlayed: Date.now(),
        difficulty: this.settings.difficulty,
        rating: null,
        statistics: {
          totalPlays: 0,
          completionRate: 0,
          averageDuration: 0,
          averageRating: 0,
          winRate: 0,
          failReasons: {},
          bestTime: 0,
          bestScore: 0,
          achievements: [],
        },
        rewardsEarned: null,
      });
    } else {
      missionProgress.attempts++;
      missionProgress.lastPlayed = Date.now();
      missionProgress.status = MissionStatus.IN_PROGRESS;
    }

    this.emit('mission_start', { campaignId: this.activeCampaign.id, missionId });
    return true;
  }

  public completeMission(success: boolean, score: number = 0, time: number = 0): void {
    if (!this.activeMission || !this.activeCampaign) {
      return;
    }

    const missionId = this.activeMission.id;
    const missionProgress = this.progress.missionProgress.get(missionId);

    if (success) {
      if (missionProgress) {
        missionProgress.status = MissionStatus.COMPLETED;
        missionProgress.wins++;

        if (missionProgress.bestTime === null || time < missionProgress.bestTime) {
          missionProgress.bestTime = time;
        }

        if (missionProgress.bestScore === null || score > missionProgress.bestScore) {
          missionProgress.bestScore = score;
        }

        missionProgress.rewardsEarned = this.activeMission.rewards;
      }

      this.progress.completedMissions.add(missionId);
      this.statistics.missionsCompleted++;
      this.statistics.totalCreditsEarned += this.activeMission.rewards.credits;

      this.unlockNextMission();

      this.calculateAndAwardStars();
      this.claimMissionRewards();
      this.emit('mission_complete', { campaignId: this.activeCampaign.id, missionId, score, time });
    } else {
      if (missionProgress) {
        missionProgress.status = MissionStatus.FAILED;
        missionProgress.losses++;
      }

      this.progress.failedMissions.add(missionId);
      this.statistics.missionsFailed++;
      this.emit('mission_fail', { campaignId: this.activeCampaign.id, missionId });
    }

    this.activeMission = null;
  }

  public skipMission(missionId: string): boolean {
    if (!this.activeCampaign) {
      return false;
    }

    const missionIndex = this.activeCampaign.missions.findIndex(m => m.id === missionId);
    if (missionIndex === -1) {
      return false;
    }

    this.progress.skippedMissions.add(missionId);

    const missionProgress = this.progress.missionProgress.get(missionId);
    if (missionProgress) {
      missionProgress.status = MissionStatus.SKIPPED;
    }

    this.unlockMission(this.activeCampaign.missions[missionIndex + 1]?.id);

    return true;
  }

  public unlockMission(missionId: string): void {
    if (!missionId) {
      return;
    }

    this.progress.unlockedMissions.add(missionId);

    if (!this.progress.missionProgress.has(missionId)) {
      this.progress.missionProgress.set(missionId, {
        missionId,
        status: MissionStatus.UNLOCKED,
        bestTime: null,
        bestScore: null,
        attempts: 0,
        wins: 0,
        losses: 0,
        lastPlayed: null,
        difficulty: this.settings.difficulty,
        rating: null,
        statistics: {
          totalPlays: 0,
          completionRate: 0,
          averageDuration: 0,
          averageRating: 0,
          winRate: 0,
          failReasons: {},
          bestTime: 0,
          bestScore: 0,
          achievements: [],
        },
        rewardsEarned: null,
      });
    }
  }

  private unlockNextMission(): void {
    if (!this.activeCampaign) {
      return;
    }

    const completedMissionIndex = this.activeCampaign.missions.findIndex(
      m => this.progress.completedMissions.has(m.id)
    );

    const nextMission = this.activeCampaign.missions[completedMissionIndex + 1];
    if (nextMission) {
      this.unlockMission(nextMission.id);
    } else {
      this.completeCampaign();
    }
  }

  public completeCampaign(): void {
    if (!this.activeCampaign) {
      return;
    }

    this.statistics.campaignsCompleted++;

    this.activeCampaign.rewards.credits;
    this.activeCampaign.unlocks.units;
    this.activeCampaign.unlocks.buildings;

    this.emit('campaign_complete', { campaignId: this.activeCampaign.id });
  }

  public makeChoice(choiceId: string, choiceData: unknown): void {
    if (!this.activeCampaign) {
      return;
    }

    this.progress.choicesMade.set(choiceId, JSON.stringify(choiceData));

    this.emit('choice_made', { choiceId, choiceData });
  }

  public setVariable(name: string, value: number): void {
    this.progress.variables.set(name, value);
  }

  public getVariable(name: string): number {
    return this.progress.variables.get(name) || 0;
  }

  public setFlag(name: string, value: boolean): void {
    this.progress.flags.set(name, value);
  }

  public getFlag(name: string): boolean {
    return this.progress.flags.get(name) || false;
  }

  public isMissionUnlocked(missionId: string): boolean {
    return this.progress.unlockedMissions.has(missionId);
  }

  public isMissionCompleted(missionId: string): boolean {
    return this.progress.completedMissions.has(missionId);
  }

  public getMissionProgress(missionId: string): MissionProgressData | undefined {
    return this.progress.missionProgress.get(missionId);
  }

  public calculateAndAwardStars(): void {
    if (!this.activeMission) {
      return;
    }

    let stars = 0;

    const missionProgress = this.progress.missionProgress.get(this.activeMission.id);
    if (!missionProgress) {
      return;
    }

    if (this.activeMission.scoreTarget) {
      if (missionProgress.bestScore && missionProgress.bestScore >= this.activeMission.scoreTarget * 0.5) {
        stars++;
      }
      if (missionProgress.bestScore && missionProgress.bestScore >= this.activeMission.scoreTarget * 0.75) {
        stars++;
      }
      if (missionProgress.bestScore && missionProgress.bestScore >= this.activeMission.scoreTarget) {
        stars++;
      }
    }

    this.activeMission.objectives.forEach(obj => {
      if (obj.type === ObjectiveType.BONUS && obj.completed) {
        stars++;
      }
    });

    const maxStars = 3 + this.activeMission.objectives.filter(o => o.type === ObjectiveType.BONUS).length;
    stars = Math.min(stars, maxStars);

    const currentStars = this.progress.starsEarned.get(this.activeMission.id) || 0;
    if (stars > currentStars) {
      this.progress.starsEarned.set(this.activeMission.id, stars);
      this.statistics.totalStarsEarned += stars - currentStars;
    }

    if (stars === maxStars) {
      this.statistics.perfectMissions++;
    }
  }

  public claimMissionRewards(): void {
    if (!this.activeMission) {
      return;
    }

    const missionId = this.activeMission.id;
    if (this.progress.rewardsClaimed.has(missionId)) {
      return;
    }

    const rewards = this.activeMission.rewards;

    rewards.unlocks.forEach(unlock => {
      this.progress.unlocks.add(unlock.id);
      this.emit('unlock_earned', { unlock });
    });

    rewards.achievements.forEach(achievement => {
      if (!this.progress.achievementsUnlocked.has(achievement.id)) {
        this.progress.achievementsUnlocked.add(achievement.id);
        this.statistics.totalAchievements++;
        this.emit('achievement_unlocked', { achievement });
      }
    });

    this.progress.rewardsClaimed.add(missionId);
    this.emit('reward_claimed', { rewards });
  }

  public getActiveCampaign(): Campaign | null {
    return this.activeCampaign;
  }

  public getActiveMission(): Mission | null {
    return this.activeMission;
  }

  public getAllCampaigns(): Campaign[] {
    return Array.from(this.campaigns.values());
  }

  public getCampaignProgress(campaignId: string): CampaignProgress | null {
    if (this.progress.campaignId !== campaignId) {
      return null;
    }
    return { ...this.progress };
  }

  public getStatistics(): CampaignStatistics {
    return { ...this.statistics };
  }

  public getSettings(): CampaignSettings {
    return { ...this.settings };
  }

  public updateSettings(settings: Partial<CampaignSettings>): void {
    this.settings = { ...this.settings, ...settings };
  }

  public saveGame(slotIndex: number): CampaignSaveSlot {
    if (!this.activeCampaign) {
      throw new Error('No active campaign');
    }

    const saveSlot: CampaignSaveSlot = {
      id: `save_${Date.now()}`,
      name: `Campaign ${this.activeCampaign.name} - Mission ${this.activeMission?.name || 'Start'}`,
      campaignId: this.activeCampaign.id,
      currentMissionId: this.activeMission?.id || '',
      progress: JSON.parse(JSON.stringify({
        ...this.progress,
        missionProgress: Array.from(this.progress.missionProgress.entries()),
        unlockedMissions: Array.from(this.progress.unlockedMissions),
        completedMissions: Array.from(this.progress.completedMissions),
        failedMissions: Array.from(this.progress.failedMissions),
        skippedMissions: Array.from(this.progress.skippedMissions),
        starsEarned: Array.from(this.progress.starsEarned.entries()),
        achievementsUnlocked: Array.from(this.progress.achievementsUnlocked),
        choicesMade: Array.from(this.progress.choicesMade.entries()),
        variables: Array.from(this.progress.variables.entries()),
        flags: Array.from(this.progress.flags.entries()),
        unlocks: Array.from(this.progress.unlocks),
        rewardsClaimed: Array.from(this.progress.rewardsClaimed),
      })),
      timestamp: Date.now(),
      screenshot: '',
      metadata: {},
    };

    this.saveSlots[slotIndex] = saveSlot;

    return saveSlot;
  }

  public loadGame(slotIndex: number): boolean {
    const saveSlot = this.saveSlots[slotIndex];
    if (!saveSlot) {
      return false;
    }

    const campaign = this.campaigns.get(saveSlot.campaignId);
    if (!campaign) {
      return false;
    }

    this.activeCampaign = campaign;
    this.activeMission = null;

    const loadedProgress = saveSlot.progress;
    this.progress = {
      ...loadedProgress,
      missionProgress: new Map(loadedProgress.missionProgress as [string, MissionProgressData][]),
      unlockedMissions: new Set(loadedProgress.unlockedMissions as string[]),
      completedMissions: new Set(loadedProgress.completedMissions as string[]),
      failedMissions: new Set(loadedProgress.failedMissions as string[]),
      skippedMissions: new Set(loadedProgress.skippedMissions as string[]),
      starsEarned: new Map(loadedProgress.starsEarned as [string, number][]),
      achievementsUnlocked: new Set(loadedProgress.achievementsUnlocked as string[]),
      choicesMade: new Map(loadedProgress.choicesMade as [string, string][]),
      variables: new Map(loadedProgress.variables as [string, number][]),
      flags: new Map(loadedProgress.flags as [string, boolean][]),
      unlocks: new Set(loadedProgress.unlocks as string[]),
      rewardsClaimed: new Set(loadedProgress.rewardsClaimed as string[]),
    };

    return true;
  }

  public getSaveSlots(): CampaignSaveSlot[] {
    return [...this.saveSlots];
  }

  public update(deltaTime: number): void {
    if (this.activeMission) {
      const currentTime = this.missionTimers.get(this.activeMission.id) || 0;
      this.missionTimers.set(this.activeMission.id, currentTime + deltaTime);
      this.progress.totalPlayTime += deltaTime;

      this.statistics.longestPlaySession = Math.max(
        this.statistics.longestPlaySession,
        this.progress.totalPlayTime
      );
    }
  }

  public on(event: string, callback: Function): void {
    const callbacks = this.eventCallbacks.get(event);
    if (callbacks) {
      callbacks.add(callback);
    }
  }

  public off(event: string, callback: Function): void {
    const callbacks = this.eventCallbacks.get(event);
    if (callbacks) {
      callbacks.delete(callback);
    }
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.eventCallbacks.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  public resetProgress(): void {
    this.progress = this.createInitialProgress();
    this.activeMission = null;
  }

  public unlockAllContent(): void {
    this.campaigns.forEach(campaign => {
      campaign.missions.forEach(mission => {
        this.unlockMission(mission.id);
      });
    });

    this.settings.unlockedContent = true;
  }

  public getCompletionPercentage(): number {
    if (!this.activeCampaign) {
      return 0;
    }

    const totalMissions = this.activeCampaign.missions.length;
    const completedMissions = this.activeCampaign.missions.filter(
      m => this.progress.completedMissions.has(m.id)
    ).length;

    return totalMissions > 0 ? (completedMissions / totalMissions) * 100 : 0;
  }

  public getTotalStars(): number {
    let total = 0;
    this.progress.starsEarned.forEach(stars => {
      total += stars;
    });
    return total;
  }

  public getMaxStars(): number {
    if (!this.activeCampaign) {
      return 0;
    }

    let max = 0;
    this.activeCampaign.missions.forEach(mission => {
      max += 3;
      max += mission.objectives.filter(o => o.type === ObjectiveType.BONUS).length;
    });
    return max;
  }

  public destroy(): void {
    this.campaigns.clear();
    this.activeCampaign = null;
    this.activeMission = null;
    this.missionTimers.clear();
    this.eventCallbacks.forEach(callbacks => callbacks.clear());
    this.eventCallbacks.clear();
    this.saveSlots = [];
  }
}

export const createCampaignManager = (): CampaignManagerClass => {
  return new CampaignManagerClass();
};

export default CampaignManagerClass;

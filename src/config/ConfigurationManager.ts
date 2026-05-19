export interface GameConfiguration {
  version: string;
  profile: PlayerProfile;
  graphics: GraphicsSettings;
  audio: AudioSettings;
  video: VideoSettings;
  gameplay: GameplaySettings;
  controls: ControlSettings;
  accessibility: AccessibilitySettings;
  network: NetworkSettings;
  ui: UISettings;
  modding: ModdingSettings;
  custom: Record<string, unknown>;
}

export interface PlayerProfile {
  id: string;
  name: string;
  avatar: string;
  level: number;
  experience: number;
  rank: string;
  rankPoints: number;
  wins: number;
  losses: number;
  draws: number;
  totalPlayTime: number;
  createdAt: number;
  lastPlayed: number;
  achievements: string[];
  statistics: PlayerStatistics;
  preferences: PlayerPreferences;
  banned: boolean;
  banReason?: string;
  banExpiry?: number;
}

export interface PlayerStatistics {
  combat: CombatStatistics;
  economy: EconomyStatistics;
  units: UnitStatistics;
  buildings: BuildingStatistics;
  technology: TechnologyStatistics;
  social: SocialStatistics;
  ranked: RankedStatistics;
  missions: MissionStatistics;
}

export interface CombatStatistics {
  unitsProduced: number;
  unitsLost: number;
  enemiesKilled: number;
  buildingsDestroyed: number;
  structuresLost: number;
  damageDealt: number;
  damageTaken: number;
  healingDone: number;
  criticalHits: number;
  perfectVictories: number;
  closeVictories: number;
  crushingDefeats: number;
  averageKillsPerGame: number;
  averageDeathsPerGame: number;
  kdRatio: number;
  mostKillsInGame: number;
  fastestVictory: number;
  longestGame: number;
}

export interface EconomyStatistics {
  creditsEarned: number;
  creditsSpent: number;
  oreCollected: number;
  unitsBuilt: number;
  structuresBuilt: number;
  upgradesPurchased: number;
  averageIncomePerMinute: number;
  peakIncome: number;
  tradesCompleted: number;
  resourcesWasted: number;
  efficientEconomyGames: number;
}

export interface UnitStatistics {
  infantryDeployed: number;
  vehiclesBuilt: number;
  aircraftDeployed: number;
  navalUnitsBuilt: number;
  specialUnitsBuilt: number;
  favoriteUnit: string;
  unitSurvivalRate: number;
  totalUnitValueProduced: number;
  totalUnitValueLost: number;
}

export interface BuildingStatistics {
  structuresBuilt: number;
  structuresDestroyed: number;
  defenseStructuresBuilt: number;
  resourceStructuresBuilt: number;
  productionStructuresBuilt: number;
  averageDefenseRating: number;
  structuresLostToScout: number;
  structuresLostToRush: number;
  structuresLostToSiege: number;
}

export interface TechnologyStatistics {
  technologiesResearched: number;
  advancedUnitsUnlocked: number;
  upgradesPurchased: number;
  timeSpentResearching: number;
  favoriteTechTree: string;
  maxTechLevel: number;
  techAdvantageGames: number;
}

export interface SocialStatistics {
  gamesPlayedWithFriends: number;
  gamesPlayedInClan: number;
  messagesSent: number;
  friendsAdded: number;
  blockedPlayers: number;
  reportsSubmitted: number;
  commendationsReceived: number;
  clanName?: string;
  clanRank?: string;
}

export interface RankedStatistics {
  currentRating: number;
  peakRating: number;
  currentDivision: string;
  currentLeague: string;
  placementMatchesPlayed: number;
  matchesWonInPlacement: number;
  gamesThisSeason: number;
  winRate: number;
  bestStreak: number;
  currentStreak: number;
  winStreak: number;
  loseStreak: number;
  decayWarning: boolean;
  lastRankedGame: number;
}

export interface MissionStatistics {
  campaignsCompleted: number;
  missionsCompleted: number;
  missionsFailed: number;
  bonusObjectivesCompleted: number;
  bonusObjectivesMissed: number;
  hiddenObjectivesFound: number;
  averageCompletionTime: number;
  fastestCompletion: number;
  hardestMissionCompleted: string;
  missionsWithNoDeaths: number;
}

export interface PlayerPreferences {
  favoriteFaction: string;
  favoriteMap: string;
  preferredGameMode: string;
  preferredTeamSize: string;
  chatEnabled: boolean;
  voiceChatEnabled: boolean;
  showTutorialHints: boolean;
  autoQueueEnabled: boolean;
  notificationSettings: NotificationPreferences;
}

export interface NotificationPreferences {
  friendOnline: boolean;
  friendOffline: boolean;
  clanInvite: boolean;
  matchFound: boolean;
  matchStarting: boolean;
  matchEnded: boolean;
  achievementsUnlocked: boolean;
  systemMessages: boolean;
  privateMessages: boolean;
  groupInvites: boolean;
}

export interface GraphicsSettings {
  resolution: Resolution;
  displayMode: DisplayMode;
  vsync: boolean;
  adaptiveVSync: boolean;
  frameRateLimit: number;
  gamma: number;
  contrast: number;
  brightness: number;
  fullscreen: boolean;
  borderless: boolean;
  antiAliasing: AntiAliasing;
  textureQuality: TextureQuality;
  shadowQuality: ShadowQuality;
  particleQuality: ParticleQuality;
  volumetricEffects: boolean;
  ambientOcclusion: boolean;
  screenSpaceReflections: boolean;
  motionBlur: boolean;
  depthOfField: boolean;
  bloom: boolean;
  lensFlare: boolean;
  weatherEffects: boolean;
  foliageDensity: number;
  waterQuality: WaterQuality;
  terrainQuality: TerrainQuality;
  uiScale: number;
  hudScale: number;
  cutsceneQuality: CutsceneQuality;
  vignette: boolean;
  chromaticAberration: boolean;
  filmGrain: boolean;
  colorBlindMode: ColorBlindMode;
  colorBlindIntensity: number;
}

export interface Resolution {
  width: number;
  height: number;
  refreshRate: number;
  aspectRatio: string;
}

export type DisplayMode = 'fullscreen' | 'windowed' | 'borderless';
export type AntiAliasing = 'none' | 'fxaa' | 'msaa2x' | 'msaa4x' | 'msaa8x' | 'taa';
export type TextureQuality = 'low' | 'medium' | 'high' | 'ultra';
export type ShadowQuality = 'off' | 'low' | 'medium' | 'high' | 'ultra';
export type ParticleQuality = 'low' | 'medium' | 'high' | 'ultra';
export type WaterQuality = 'low' | 'medium' | 'high' | 'ultra';
export type TerrainQuality = 'low' | 'medium' | 'high' | 'ultra';
export type CutsceneQuality = 'low' | 'medium' | 'high' | 'ultra';
export type ColorBlindMode = 'none' | 'protanopia' | 'deuteranopia' | 'tritanopia';

export interface AudioSettings {
  masterVolume: number;
  musicVolume: number;
  sfxVolume: number;
  voiceVolume: number;
  ambientVolume: number;
  uiVolume: number;
  dialogueVolume: number;
  muted: boolean;
  muteOnFocusLoss: boolean;
  audioQuality: AudioQuality;
  speakerConfiguration: SpeakerConfig;
  microphoneEnabled: boolean;
  microphoneVolume: number;
  pushToTalk: boolean;
  voiceActivation: boolean;
  noiseGate: number;
  echoCancellation: boolean;
  spatialAudio: boolean;
  audioDevice: string;
  inputDevice: string;
  outputDevice: string;
  dynamicRange: DynamicRange;
  audioLanguage: string;
  subtitleEnabled: boolean;
  subtitleSize: SubtitleSize;
  subtitleBackground: boolean;
}

export type AudioQuality = 'low' | 'medium' | 'high';
export type SpeakerConfig = 'mono' | 'stereo' | 'quad' | 'surround' | '51' | '71' | 'atmos';
export type DynamicRange = 'compressed' | 'normal' | 'full';
export type SubtitleSize = 'small' | 'medium' | 'large';

export interface VideoSettings {
  cinematicMode: boolean;
  cinematicCamera: boolean;
  cinematicSubtitles: boolean;
  skipIntroMovies: boolean;
  autoPlayTrailers: boolean;
  playbackSpeed: PlaybackSpeed;
  aspectRatio: AspectRatio;
  hdrEnabled: boolean;
  hdrBrightness: number;
  hdrContrast: number;
  wideColorGamut: boolean;
  frameInterpolation: boolean;
  vsyncMode: VSyncMode;
  lowLatencyMode: boolean;
  reduceLatency: boolean;
  gsyncCompatible: boolean;
  freesyncCompatible: boolean;
}

export type PlaybackSpeed = '0.5x' | '0.75x' | '1x' | '1.25x' | '1.5x' | '2x';
export type AspectRatio = 'auto' | '16:9' | '16:10' | '21:9' | '4:3';
export type VSyncMode = 'off' | 'on' | 'adaptive' | 'fast';

export interface GameplaySettings {
  gameSpeed: GameSpeed;
  difficulty: Difficulty;
  friendlyFire: boolean;
  teamsLocked: boolean;
  mapRevealed: boolean;
  resources: ResourceMode;
  startingCredits: StartingCredits;
  crateSpawn: boolean;
  shortGame: boolean;
  superweapons: boolean;
  buildOffAlly: boolean;
  alliedVsAlliedVictory: boolean;
  bridgeDestroyed: boolean;
  fogOfWar: boolean;
  goldBlood: boolean;
  unitNames: boolean;
  nameOnUnit: boolean;
  hitpointBars: HitpointBarDisplay;
  damageNumbers: boolean;
  killFeed: boolean;
  minimapAlerts: boolean;
  autoSaveInterval: number;
  quickSaveEnabled: boolean;
  replayAutoRecord: boolean;
  tutorialHints: boolean;
  tooltipsEnabled: boolean;
  tipsEnabled: boolean;
  loadingScreenTips: boolean;
  newsEnabled: boolean;
  updateNewsEnabled: boolean;
  replayShareEnabled: boolean;
  screenshotQuality: ScreenshotQuality;
  screenshotFormat: ScreenshotFormat;
  autoScreenshot: boolean;
  autoScreenshotInterval: number;
}

export type GameSpeed = 'pause' | 'slow' | 'normal' | 'fast' | 'very_fast';
export type Difficulty = 'easy' | 'normal' | 'hard' | 'brutal' | 'custom';
export type ResourceMode = 'standard' | 'megarich' | 'multiplier';
export type StartingCredits = '1000' | '2000' | '5000' | '10000' | 'unlimited';
export type HitpointBarDisplay = 'always' | 'damaged' | 'selected' | 'never';
export type ScreenshotQuality = 'low' | 'medium' | 'high' | 'ultra';
export type ScreenshotFormat = 'png' | 'jpg' | 'tga';

export interface ControlSettings {
  keybindings: Keybindings;
  mouseSettings: MouseSettings;
  controllerSettings: ControllerSettings;
  hotkeys: HotkeyProfile;
  wheelSettings: WheelSettings;
  inputDevices: InputDeviceSettings;
  accessibility: ControlAccessibilitySettings;
}

export interface Keybindings {
  primary: PrimaryKeybindings;
  secondary: SecondaryKeybindings;
  modifiers: ModifierKeybindings;
  profiles: KeybindingProfile[];
  activeProfile: string;
}

export interface PrimaryKeybindings {
  move: string;
  attack: string;
  stop: string;
  holdPosition: string;
  patrol: string;
  scatter: string;
  guard: string;
  capture: string;
  repair: string;
  sell: string;
  togglePower: string;
  detonate: string;
  placeBuilding: string;
  cancelConstruction: string;
  placeUnit: string;
  rallyPoint: string;
  terrainDecoration: string;
  special1: string;
  special2: string;
  special3: string;
  special4: string;
  ability1: string;
  ability2: string;
  ability3: string;
  ability4: string;
  ability5: string;
  ability6: string;
  ability7: string;
  ability8: string;
  queueUnit1: string;
  queueUnit2: string;
  queueUnit3: string;
  queueUnit4: string;
  queueUnit5: string;
  queueUnit6: string;
  queueUnit7: string;
  queueUnit8: string;
  queueUnit9: string;
  queueUnit10: string;
}

export interface SecondaryKeybindings {
  selectAll: string;
  selectAllOfType: string;
  selectAllOfBase: string;
  addToSelection: string;
  removeFromSelection: string;
  selectSimilar: string;
  selectIdleUnit: string;
  selectAllIdleUnits: string;
  selectDamagedUnit: string;
  selectAllDamagedUnits: string;
  cycleUnitSelection: string;
  cycleBuildingSelection: string;
  lastProduction: string;
  lastHarvesters: string;
  lastGroup: string;
}

export interface ModifierKeybindings {
  modifier1: string;
  modifier2: string;
  modifier3: string;
  modifier4: string;
}

export interface KeybindingProfile {
  id: string;
  name: string;
  bindings: Record<string, string>;
  isDefault: boolean;
}

export interface MouseSettings {
  sensitivity: number;
  invertY: boolean;
  invertX: boolean;
  smoothCamera: boolean;
  cameraAcceleration: number;
  cameraMaxSpeed: number;
  edgeScroll: boolean;
  edgeScrollSize: number;
  middleClickPan: boolean;
  doubleClickSelect: boolean;
  clickSelect: boolean;
  dragBox: boolean;
  rightClickMove: boolean;
  rightClickAttack: boolean;
  scrollWheelZoom: boolean;
  zoomSpeed: number;
  zoomMin: number;
  zoomMax: number;
  scrollInvert: boolean;
  cursorSize: CursorSize;
  customCursor: string;
}

export type CursorSize = 'small' | 'medium' | 'large' | 'extra_large';

export interface ControllerSettings {
  enabled: boolean;
  deadzone: number;
  vibration: boolean;
  vibrationIntensity: number;
  autoAim: boolean;
  autoAimStrength: number;
  aimAssist: boolean;
  aimAssistStrength: number;
  controllerScheme: ControllerScheme;
  leftStickSensitivity: number;
  rightStickSensitivity: number;
  triggerSensitivity: number;
  swapSticks: boolean;
  gyroAim: boolean;
  gyroSensitivity: number;
}

export type ControllerScheme = 'default' | 'southpaw' | 'legacy' | 'custom';

export interface HotkeyProfile {
  id: string;
  name: string;
  isDefault: boolean;
  groups: HotkeyGroup[];
}

export interface HotkeyGroup {
  name: string;
  hotkeys: Hotkey[];
}

export interface Hotkey {
  action: string;
  key: string;
  modifiers: string[];
  isDefault: boolean;
  conflict: boolean;
}

export interface WheelSettings {
  enabled: boolean;
  sensitivity: number;
  invertY: boolean;
  acceleration: number;
  maxSpeed: number;
}

export interface InputDeviceSettings {
  primaryDevice: InputDevice;
  secondaryDevice: InputDevice;
  autoDetect: boolean;
  inputRemapping: boolean;
}

export type InputDevice = 'keyboard' | 'mouse' | 'controller' | 'touch';

export interface ControlAccessibilitySettings {
  oneHandedMode: boolean;
  buttonHoldTime: number;
  doubleTapTime: number;
  autoFireEnabled: boolean;
  autoFireRate: number;
  toggleMode: ToggleMode;
  staminaMode: boolean;
  simplifiedControls: boolean;
  requiredButtons: string[];
  excludedButtons: string[];
}

export type ToggleMode = 'none' | 'walk' | 'sprint' | 'aim';

export interface AccessibilitySettings {
  colorBlind: ColorBlindSettings;
  audio: AccessibilityAudioSettings;
  visual: AccessibilityVisualSettings;
  motor: MotorAccessibilitySettings;
  cognitive: CognitiveAccessibilitySettings;
  subtitle: SubtitleSettings;
  motion: MotionSettings;
}

export interface ColorBlindSettings {
  enabled: boolean;
  type: ColorBlindMode;
  intensity: number;
  customFilters: ColorFilter[];
}

export interface ColorFilter {
  name: string;
  filter: string;
  enabled: boolean;
}

export interface AccessibilityAudioSettings {
  monoAudio: boolean;
  panningStrength: number;
  separateVolumeControls: boolean;
  audioDescriptions: boolean;
  importantSoundIndicators: boolean;
  spatialAudioDisabled: boolean;
  reduceAudioVariety: boolean;
}

export interface AccessibilityVisualSettings {
  highContrast: boolean;
  largeText: boolean;
  textScaling: number;
  uiScaling: number;
  boldText: boolean;
  reduceMotion: boolean;
  reduceTransparency: boolean;
  reduceAnimations: boolean;
  flashReduction: FlashReduction;
  screenReaderEnabled: boolean;
  screenReaderVerbosity: ScreenReaderVerbosity;
  focusIndicators: boolean;
  focusIndicatorSize: number;
  colorEnhancement: boolean;
}

export type FlashReduction = 'none' | 'moderate' | 'strong';
export type ScreenReaderVerbosity = 'minimal' | 'normal' | 'verbose';

export interface MotorAccessibilitySettings {
  aimAssist: boolean;
  aimAssistStrength: number;
  autoAim: boolean;
  autoAimStrength: number;
  slowMode: boolean;
  slowModeMultiplier: number;
  oneHandedMode: boolean;
  remapControllerButtons: boolean;
  buttonReassignment: ButtonReassignment[];
  macroSupport: boolean;
  macroList: GameMacro[];
  toggleActions: ToggleAction[];
  holdToSprint: boolean;
  autoSprint: boolean;
  doublePressToRun: boolean;
}

export interface ButtonReassignment {
  originalButton: string;
  newButton: string;
  device: InputDevice;
}

export interface GameMacro {
  id: string;
  name: string;
  actions: MacroAction[];
  triggerKey: string;
  triggerModifiers: string[];
}

export interface MacroAction {
  type: 'key' | 'mouse' | 'delay' | 'repeat';
  action?: string;
  key?: string;
  mouseButton?: number;
  duration?: number;
  count?: number;
}

export interface ToggleAction {
  action: string;
  toggleKey: string;
  enabled: boolean;
}

export interface CognitiveAccessibilitySettings {
  simplifiedUI: boolean;
  objectiveMarkers: boolean;
  objectiveHistory: boolean;
  hintsEnabled: boolean;
  hintFrequency: HintFrequency;
  tutorialMode: boolean;
  autoPathfinding: boolean;
  autoCover: boolean;
  autoQueue: boolean;
  simplifiedControls: boolean;
  gameSpeedControl: boolean;
  pauseOnDemand: boolean;
  clearGoals: boolean;
  progressIndicators: boolean;
  timeExtensions: boolean;
  additionalLives: boolean;
}

export type HintFrequency = 'none' | 'low' | 'medium' | 'high';

export interface SubtitleSettings {
  enabled: boolean;
  size: SubtitleSize;
  color: string;
  backgroundColor: string;
  backgroundOpacity: number;
  speakerNames: boolean;
  importantSounds: boolean;
  ambientSounds: boolean;
  textPosition: SubtitlePosition;
  maximumLines: number;
  fontFamily: string;
}

export type SubtitlePosition = 'bottom' | 'top' | 'custom';

export interface MotionSettings {
  reducedMotion: boolean;
  cameraShake: CameraShake;
  screenEffects: boolean;
  parallaxEffects: boolean;
  depthEffects: boolean;
  zoomEffects: boolean;
  transitionSpeed: TransitionSpeed;
}

export type CameraShake = 'full' | 'reduced' | 'minimal' | 'off';
export type TransitionSpeed = 'slow' | 'normal' | 'fast' | 'instant';

export interface NetworkSettings {
  connectionType: ConnectionType;
  serverRegion: ServerRegion;
  preferredServer: string;
  autoRegionSelect: boolean;
  bandwidthLimit: number;
  packetSize: number;
  pingLimit: number;
  allowConnections: boolean;
  allowInvites: boolean;
  maxConnections: number;
  qualityMode: QualityMode;
  upnpEnabled: boolean;
  natType: NATType;
  relayServerEnabled: boolean;
  encryption: EncryptionType;
  macAddressSpoofing: boolean;
  proxyEnabled: boolean;
  proxyServer: string;
  proxyPort: number;
}

export type ConnectionType = 'direct' | 'relay' | 'tunneled';
export type ServerRegion = 'na' | 'eu' | 'asia' | 'oce' | 'sa' | 'af' | 'me';
export type QualityMode = 'auto' | 'low' | 'medium' | 'high';
export type NATType = 'open' | 'moderate' | 'strict';
export type EncryptionType = 'none' | 'tls' | 'dtls';

export interface UISettings {
  theme: UITheme;
  colorScheme: ColorScheme;
  language: string;
  dateFormat: DateFormat;
  timeFormat: TimeFormat;
  numberFormat: NumberFormat;
  currencyFormat: CurrencyFormat;
  showFPS: boolean;
  showPing: boolean;
  showStats: boolean;
  showDecorations: boolean;
  showEffects: boolean;
  showParticles: boolean;
  showShadows: boolean;
  showWeather: boolean;
  showNametags: boolean;
  showDamageNumbers: boolean;
  showKillFeed: boolean;
  showMinimap: boolean;
  minimapSize: MinimapSize;
  minimapPosition: MinimapPosition;
  chatPosition: ChatPosition;
  chatOpacity: number;
  chatTimestamps: boolean;
  chatFilterProfanity: boolean;
  chatAutoScroll: boolean;
  notificationsPosition: NotificationPosition;
  notificationDuration: number;
  notificationSound: boolean;
  showTooltips: boolean;
  tooltipDelay: number;
  showTutorial: boolean;
  hudStyle: HUDStyle;
  hudColor: HUDColor;
  healthBars: HealthBarDisplay;
  unitIcons: UnitIconDisplay;
  resourceDisplay: ResourceDisplayMode;
  clockDisplay: ClockDisplay;
  matchTimerDisplay: boolean;
  spectatorOverlay: boolean;
}

export type UITheme = 'light' | 'dark' | 'auto' | 'custom';
export type ColorScheme = 'default' | 'military' | 'futuristic' | 'retro' | 'custom';
export type DateFormat = 'mm/dd/yyyy' | 'dd/mm/yyyy' | 'yyyy-mm-dd';
export type TimeFormat = '12h' | '24h';
export type NumberFormat = '1,000.00' | '1.000,00';
export type CurrencyFormat = 'symbol' | 'code';
export type MinimapSize = 'small' | 'medium' | 'large';
export type MinimapPosition = 'top_left' | 'top_right' | 'bottom_left' | 'bottom_right';
export type ChatPosition = 'left' | 'right' | 'bottom';
export type NotificationPosition = 'top_left' | 'top_right' | 'bottom_left' | 'bottom_right' | 'center';
export type HUDStyle = 'classic' | 'modern' | 'minimal' | 'custom';
export type HUDColor = 'default' | 'red' | 'blue' | 'green' | 'yellow' | 'purple';
export type HealthBarDisplay = 'always' | 'damaged' | 'selected' | 'never';
export type UnitIconDisplay = 'icons' | 'portraits' | 'both' | 'none';
export type ResourceDisplayMode = 'detailed' | 'compact' | 'minimal';
export type ClockDisplay = 'none' | 'game_time' | 'real_time' | 'both';

export interface ModdingSettings {
  modsEnabled: boolean;
  installedMods: InstalledMod[];
  enabledMods: string[];
  modOrder: string[];
  modPaths: string[];
  contentPacks: string[];
  autoLoadMods: boolean;
  modValidation: boolean;
  unsafeModsAllowed: boolean;
  modAPIEnabled: boolean;
  scriptSandbox: boolean;
  customAssets: CustomAsset[];
  assetOverrides: AssetOverride[];
  modBrowser: ModBrowserSettings;
  workshopEnabled: boolean;
  workshopAutoUpdate: boolean;
}

export interface InstalledMod {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string;
  thumbnail: string;
  enabled: boolean;
  loadOrder: number;
  dependencies: string[];
  conflicts: string[];
  size: number;
  installedAt: number;
  updatedAt: number;
  workshopId?: string;
  source: ModSource;
}

export type ModSource = 'local' | 'workshop' | 'official';

export interface CustomAsset {
  id: string;
  name: string;
  type: AssetType;
  path: string;
  format: string;
  size: number;
  loaded: boolean;
}

export type AssetType = 'texture' | 'model' | 'audio' | 'video' | 'font' | 'data';

export interface AssetOverride {
  originalAsset: string;
  replacementAsset: string;
  modId: string;
  enabled: boolean;
}

export interface ModBrowserSettings {
  sortBy: ModSortBy;
  sortOrder: SortOrder;
  filterType: ModFilterType;
  filterCategory: string[];
  searchQuery: string;
  showInstalled: boolean;
  showNotInstalled: boolean;
  showOutdated: boolean;
  showEnabled: boolean;
  showDisabled: boolean;
}

export type ModSortBy = 'name' | 'date' | 'size' | 'downloads' | 'rating' | 'popularity';
export type SortOrder = 'asc' | 'desc';
export type ModFilterType = 'all' | 'maps' | 'mods' | 'units' | 'sounds' | 'textures';

export class ConfigurationManager {
  private config: GameConfiguration;
  private defaults: GameConfiguration;
  private isDirty: boolean = false;
  private listeners: Map<string, Set<(value: unknown) => void>> = new Map();
  private autoSaveInterval: NodeJS.Timeout | null = null;
  private readonly CONFIG_VERSION = '1.0.0';
  private readonly storage: StorageAdapter;
  private readonly validation: ConfigValidator;
  private readonly migration: ConfigMigrator;

  constructor() {
    this.storage = new LocalStorageAdapter();
    this.validation = new ConfigValidator();
    this.migration = new ConfigMigrator();

    this.defaults = this.createDefaultConfiguration();
    this.config = this.loadConfiguration();
  }

  private createDefaultConfiguration(): GameConfiguration {
    return {
      version: this.CONFIG_VERSION,
      profile: {
        id: `player_${Date.now()}`,
        name: 'Commander',
        avatar: 'default',
        level: 1,
        experience: 0,
        rank: 'Rookie',
        rankPoints: 0,
        wins: 0,
        losses: 0,
        draws: 0,
        totalPlayTime: 0,
        createdAt: Date.now(),
        lastPlayed: Date.now(),
        achievements: [],
        statistics: {
          combat: this.createDefaultCombatStats(),
          economy: this.createDefaultEconomyStats(),
          units: this.createDefaultUnitStats(),
          buildings: this.createDefaultBuildingStats(),
          technology: this.createDefaultTechStats(),
          social: this.createDefaultSocialStats(),
          ranked: this.createDefaultRankedStats(),
          missions: this.createDefaultMissionStats()
        },
        preferences: {
          favoriteFaction: 'allied',
          favoriteMap: '',
          preferredGameMode: 'skirmish',
          preferredTeamSize: '2v2',
          chatEnabled: true,
          voiceChatEnabled: false,
          showTutorialHints: true,
          autoQueueEnabled: false,
          notificationSettings: {
            friendOnline: true,
            friendOffline: true,
            clanInvite: true,
            matchFound: true,
            matchStarting: true,
            matchEnded: true,
            achievementsUnlocked: true,
            systemMessages: true,
            privateMessages: true,
            groupInvites: true
          }
        },
        banned: false
      },
      graphics: this.createDefaultGraphicsSettings(),
      audio: this.createDefaultAudioSettings(),
      video: this.createDefaultVideoSettings(),
      gameplay: this.createDefaultGameplaySettings(),
      controls: this.createDefaultControlSettings(),
      accessibility: this.createDefaultAccessibilitySettings(),
      network: this.createDefaultNetworkSettings(),
      ui: this.createDefaultUISettings(),
      modding: this.createDefaultModdingSettings(),
      custom: {}
    };
  }

  private createDefaultCombatStats(): CombatStatistics {
    return {
      unitsProduced: 0,
      unitsLost: 0,
      enemiesKilled: 0,
      buildingsDestroyed: 0,
      structuresLost: 0,
      damageDealt: 0,
      damageTaken: 0,
      healingDone: 0,
      criticalHits: 0,
      perfectVictories: 0,
      closeVictories: 0,
      crushingDefeats: 0,
      averageKillsPerGame: 0,
      averageDeathsPerGame: 0,
      kdRatio: 0,
      mostKillsInGame: 0,
      fastestVictory: 0,
      longestGame: 0
    };
  }

  private createDefaultEconomyStats(): EconomyStatistics {
    return {
      creditsEarned: 0,
      creditsSpent: 0,
      oreCollected: 0,
      unitsBuilt: 0,
      structuresBuilt: 0,
      upgradesPurchased: 0,
      averageIncomePerMinute: 0,
      peakIncome: 0,
      tradesCompleted: 0,
      resourcesWasted: 0,
      efficientEconomyGames: 0
    };
  }

  private createDefaultUnitStats(): UnitStatistics {
    return {
      infantryDeployed: 0,
      vehiclesBuilt: 0,
      aircraftDeployed: 0,
      navalUnitsBuilt: 0,
      specialUnitsBuilt: 0,
      favoriteUnit: '',
      unitSurvivalRate: 0,
      totalUnitValueProduced: 0,
      totalUnitValueLost: 0
    };
  }

  private createDefaultBuildingStats(): BuildingStatistics {
    return {
      structuresBuilt: 0,
      structuresDestroyed: 0,
      defenseStructuresBuilt: 0,
      resourceStructuresBuilt: 0,
      productionStructuresBuilt: 0,
      averageDefenseRating: 0,
      structuresLostToScout: 0,
      structuresLostToRush: 0,
      structuresLostToSiege: 0
    };
  }

  private createDefaultTechStats(): TechnologyStatistics {
    return {
      technologiesResearched: 0,
      advancedUnitsUnlocked: 0,
      upgradesPurchased: 0,
      timeSpentResearching: 0,
      favoriteTechTree: '',
      maxTechLevel: 0,
      techAdvantageGames: 0
    };
  }

  private createDefaultSocialStats(): SocialStatistics {
    return {
      gamesPlayedWithFriends: 0,
      gamesPlayedInClan: 0,
      messagesSent: 0,
      friendsAdded: 0,
      blockedPlayers: 0,
      reportsSubmitted: 0,
      commendationsReceived: 0
    };
  }

  private createDefaultRankedStats(): RankedStatistics {
    return {
      currentRating: 1000,
      peakRating: 1000,
      currentDivision: 'Bronze',
      currentLeague: 'V',
      placementMatchesPlayed: 0,
      matchesWonInPlacement: 0,
      gamesThisSeason: 0,
      winRate: 0,
      bestStreak: 0,
      currentStreak: 0,
      winStreak: 0,
      loseStreak: 0,
      decayWarning: false,
      lastRankedGame: 0
    };
  }

  private createDefaultMissionStats(): MissionStatistics {
    return {
      campaignsCompleted: 0,
      missionsCompleted: 0,
      missionsFailed: 0,
      bonusObjectivesCompleted: 0,
      bonusObjectivesMissed: 0,
      hiddenObjectivesFound: 0,
      averageCompletionTime: 0,
      fastestCompletion: 0,
      hardestMissionCompleted: '',
      missionsWithNoDeaths: 0
    };
  }

  private createDefaultGraphicsSettings(): GraphicsSettings {
    return {
      resolution: { width: 1920, height: 1080, refreshRate: 60, aspectRatio: '16:9' },
      displayMode: 'fullscreen',
      vsync: true,
      adaptiveVSync: false,
      frameRateLimit: 60,
      gamma: 1.0,
      contrast: 1.0,
      brightness: 1.0,
      fullscreen: true,
      borderless: false,
      antiAliasing: 'msaa4x',
      textureQuality: 'high',
      shadowQuality: 'high',
      particleQuality: 'high',
      volumetricEffects: true,
      ambientOcclusion: true,
      screenSpaceReflections: false,
      motionBlur: false,
      depthOfField: false,
      bloom: true,
      lensFlare: true,
      weatherEffects: true,
      foliageDensity: 1.0,
      waterQuality: 'high',
      terrainQuality: 'high',
      uiScale: 1.0,
      hudScale: 1.0,
      cutsceneQuality: 'high',
      vignette: false,
      chromaticAberration: false,
      filmGrain: false,
      colorBlindMode: 'none',
      colorBlindIntensity: 0
    };
  }

  private createDefaultAudioSettings(): AudioSettings {
    return {
      masterVolume: 0.8,
      musicVolume: 0.7,
      sfxVolume: 1.0,
      voiceVolume: 1.0,
      ambientVolume: 0.8,
      uiVolume: 0.8,
      dialogueVolume: 1.0,
      muted: false,
      muteOnFocusLoss: false,
      audioQuality: 'high',
      speakerConfiguration: 'stereo',
      microphoneEnabled: false,
      microphoneVolume: 0.8,
      pushToTalk: true,
      voiceActivation: false,
      noiseGate: 0.5,
      echoCancellation: true,
      spatialAudio: true,
      audioDevice: 'default',
      inputDevice: 'default',
      outputDevice: 'default',
      dynamicRange: 'normal',
      audioLanguage: 'en',
      subtitleEnabled: false,
      subtitleSize: 'medium',
      subtitleBackground: true
    };
  }

  private createDefaultVideoSettings(): VideoSettings {
    return {
      cinematicMode: false,
      cinematicCamera: true,
      cinematicSubtitles: true,
      skipIntroMovies: false,
      autoPlayTrailers: true,
      playbackSpeed: '1x',
      aspectRatio: 'auto',
      hdrEnabled: false,
      hdrBrightness: 1.0,
      hdrContrast: 1.0,
      wideColorGamut: false,
      frameInterpolation: false,
      vsyncMode: 'on',
      lowLatencyMode: false,
      reduceLatency: false,
      gsyncCompatible: false,
      freesyncCompatible: false
    };
  }

  private createDefaultGameplaySettings(): GameplaySettings {
    return {
      gameSpeed: 'normal',
      difficulty: 'normal',
      friendlyFire: false,
      teamsLocked: false,
      mapRevealed: false,
      resources: 'standard',
      startingCredits: '2000',
      crateSpawn: true,
      shortGame: false,
      superweapons: true,
      buildOffAlly: true,
      alliedVsAlliedVictory: false,
      bridgeDestroyed: false,
      fogOfWar: true,
      goldBlood: false,
      unitNames: false,
      nameOnUnit: false,
      hitpointBars: 'damaged',
      damageNumbers: true,
      killFeed: true,
      minimapAlerts: true,
      autoSaveInterval: 300,
      quickSaveEnabled: true,
      replayAutoRecord: true,
      tutorialHints: true,
      tooltipsEnabled: true,
      tipsEnabled: true,
      loadingScreenTips: true,
      newsEnabled: true,
      updateNewsEnabled: true,
      replayShareEnabled: true,
      screenshotQuality: 'high',
      screenshotFormat: 'png',
      autoScreenshot: false,
      autoScreenshotInterval: 60
    };
  }

  private createDefaultControlSettings(): ControlSettings {
    return {
      keybindings: {
        primary: {
          move: 'M',
          attack: 'A',
          stop: 'S',
          holdPosition: 'H',
          patrol: 'P',
          scatter: 'X',
          guard: 'G',
          capture: 'C',
          repair: 'R',
          sell: 'Delete',
          togglePower: 'T',
          detonate: 'Y',
          placeBuilding: 'B',
          cancelConstruction: 'Escape',
          placeUnit: 'U',
          rallyPoint: 'Q',
          terrainDecoration: 'D',
          special1: 'F1',
          special2: 'F2',
          special3: 'F3',
          special4: 'F4',
          ability1: '1',
          ability2: '2',
          ability3: '3',
          ability4: '4',
          ability5: '5',
          ability6: '6',
          ability7: '7',
          ability8: '8',
          queueUnit1: 'Shift+1',
          queueUnit2: 'Shift+2',
          queueUnit3: 'Shift+3',
          queueUnit4: 'Shift+4',
          queueUnit5: 'Shift+5',
          queueUnit6: 'Shift+6',
          queueUnit7: 'Shift+7',
          queueUnit8: 'Shift+8',
          queueUnit9: 'Shift+9',
          queueUnit10: 'Shift+0'
        },
        secondary: {
          selectAll: 'Ctrl+A',
          selectAllOfType: 'Ctrl+Click',
          selectAllOfBase: 'Ctrl+B',
          addToSelection: 'Shift+Click',
          removeFromSelection: 'Ctrl+Click',
          selectSimilar: 'Ctrl+S',
          selectIdleUnit: 'Tab',
          selectAllIdleUnits: 'Shift+Tab',
          selectDamagedUnit: 'Shift+D',
          selectAllDamagedUnits: 'Ctrl+Shift+D',
          cycleUnitSelection: 'Z',
          cycleBuildingSelection: 'X',
          lastProduction: 'Space',
          lastHarvesters: 'H',
          lastGroup: 'F5'
        },
        modifiers: {
          modifier1: 'Shift',
          modifier2: 'Ctrl',
          modifier3: 'Alt',
          modifier4: 'Meta'
        },
        profiles: [],
        activeProfile: 'default'
      },
      mouseSettings: {
        sensitivity: 1.0,
        invertY: false,
        invertX: false,
        smoothCamera: true,
        cameraAcceleration: 0.5,
        cameraMaxSpeed: 1.0,
        edgeScroll: true,
        edgeScrollSize: 20,
        middleClickPan: true,
        doubleClickSelect: true,
        clickSelect: true,
        dragBox: true,
        rightClickMove: true,
        rightClickAttack: false,
        scrollWheelZoom: true,
        zoomSpeed: 1.0,
        zoomMin: 0.5,
        zoomMax: 2.0,
        scrollInvert: false,
        cursorSize: 'medium',
        customCursor: 'default'
      },
      controllerSettings: {
        enabled: false,
        deadzone: 0.15,
        vibration: true,
        vibrationIntensity: 0.5,
        autoAim: true,
        autoAimStrength: 0.5,
        aimAssist: true,
        aimAssistStrength: 0.5,
        controllerScheme: 'default',
        leftStickSensitivity: 1.0,
        rightStickSensitivity: 1.0,
        triggerSensitivity: 1.0,
        swapSticks: false,
        gyroAim: false,
        gyroSensitivity: 1.0
      },
      hotkeys: {
        id: 'default',
        name: 'Default',
        isDefault: true,
        groups: []
      },
      wheelSettings: {
        enabled: false,
        sensitivity: 1.0,
        invertY: false,
        acceleration: 0.5,
        maxSpeed: 1.0
      },
      inputDevices: {
        primaryDevice: 'keyboard',
        secondaryDevice: 'mouse',
        autoDetect: true,
        inputRemapping: true
      },
      accessibility: {
        oneHandedMode: false,
        buttonHoldTime: 0.2,
        doubleTapTime: 0.3,
        autoFireEnabled: false,
        autoFireRate: 5,
        toggleMode: 'none',
        staminaMode: false,
        simplifiedControls: false,
        requiredButtons: [],
        excludedButtons: []
      }
    };
  }

  private createDefaultAccessibilitySettings(): AccessibilitySettings {
    return {
      colorBlind: {
        enabled: false,
        type: 'none',
        intensity: 0,
        customFilters: []
      },
      audio: {
        monoAudio: false,
        panningStrength: 1.0,
        separateVolumeControls: true,
        audioDescriptions: false,
        importantSoundIndicators: false,
        spatialAudioDisabled: false,
        reduceAudioVariety: false
      },
      visual: {
        highContrast: false,
        largeText: false,
        textScaling: 1.0,
        uiScaling: 1.0,
        boldText: false,
        reduceMotion: false,
        reduceTransparency: false,
        reduceAnimations: false,
        flashReduction: 'none',
        screenReaderEnabled: false,
        screenReaderVerbosity: 'normal',
        focusIndicators: true,
        focusIndicatorSize: 1.0,
        colorEnhancement: false
      },
      motor: {
        aimAssist: false,
        aimAssistStrength: 0.5,
        autoAim: false,
        autoAimStrength: 0.5,
        slowMode: false,
        slowModeMultiplier: 0.5,
        oneHandedMode: false,
        remapControllerButtons: false,
        buttonReassignment: [],
        macroSupport: false,
        macroList: [],
        toggleActions: [],
        holdToSprint: false,
        autoSprint: false,
        doublePressToRun: false
      },
      cognitive: {
        simplifiedUI: false,
        objectiveMarkers: true,
        objectiveHistory: true,
        hintsEnabled: true,
        hintFrequency: 'medium',
        tutorialMode: false,
        autoPathfinding: true,
        autoCover: false,
        autoQueue: false,
        simplifiedControls: false,
        gameSpeedControl: true,
        pauseOnDemand: true,
        clearGoals: true,
        progressIndicators: true,
        timeExtensions: false,
        additionalLives: false
      },
      subtitle: {
        enabled: false,
        size: 'medium',
        color: '#FFFFFF',
        backgroundColor: '#000000',
        backgroundOpacity: 0.75,
        speakerNames: true,
        importantSounds: true,
        ambientSounds: false,
        textPosition: 'bottom',
        maximumLines: 2,
        fontFamily: 'default'
      },
      motion: {
        reducedMotion: false,
        cameraShake: 'full',
        screenEffects: true,
        parallaxEffects: true,
        depthEffects: true,
        zoomEffects: true,
        transitionSpeed: 'normal'
      }
    };
  }

  private createDefaultNetworkSettings(): NetworkSettings {
    return {
      connectionType: 'direct',
      serverRegion: 'na',
      preferredServer: '',
      autoRegionSelect: true,
      bandwidthLimit: 0,
      packetSize: 1400,
      pingLimit: 200,
      allowConnections: true,
      allowInvites: true,
      maxConnections: 8,
      qualityMode: 'auto',
      upnpEnabled: true,
      natType: 'moderate',
      relayServerEnabled: false,
      encryption: 'tls',
      macAddressSpoofing: false,
      proxyEnabled: false,
      proxyServer: '',
      proxyPort: 0
    };
  }

  private createDefaultUISettings(): UISettings {
    return {
      theme: 'dark',
      colorScheme: 'default',
      language: 'en',
      dateFormat: 'mm/dd/yyyy',
      timeFormat: '24h',
      numberFormat: '1,000.00',
      currencyFormat: 'symbol',
      showFPS: false,
      showPing: false,
      showStats: false,
      showDecorations: true,
      showEffects: true,
      showParticles: true,
      showShadows: true,
      showWeather: true,
      showNametags: true,
      showDamageNumbers: true,
      showKillFeed: true,
      showMinimap: true,
      minimapSize: 'medium',
      minimapPosition: 'top_right',
      chatPosition: 'left',
      chatOpacity: 0.8,
      chatTimestamps: true,
      chatFilterProfanity: false,
      chatAutoScroll: true,
      notificationsPosition: 'top_right',
      notificationDuration: 5,
      notificationSound: true,
      showTooltips: true,
      tooltipDelay: 200,
      showTutorial: true,
      hudStyle: 'modern',
      hudColor: 'default',
      healthBars: 'damaged',
      unitIcons: 'icons',
      resourceDisplay: 'detailed',
      clockDisplay: 'game_time',
      matchTimerDisplay: true,
      spectatorOverlay: false
    };
  }

  private createDefaultModdingSettings(): ModdingSettings {
    return {
      modsEnabled: false,
      installedMods: [],
      enabledMods: [],
      modOrder: [],
      modPaths: [],
      contentPacks: [],
      autoLoadMods: false,
      modValidation: true,
      unsafeModsAllowed: false,
      modAPIEnabled: false,
      scriptSandbox: true,
      customAssets: [],
      assetOverrides: [],
      modBrowser: {
        sortBy: 'popularity',
        sortOrder: 'desc',
        filterType: 'all',
        filterCategory: [],
        searchQuery: '',
        showInstalled: true,
        showNotInstalled: true,
        showOutdated: true,
        showEnabled: true,
        showDisabled: true
      },
      workshopEnabled: true,
      workshopAutoUpdate: true
    };
  }

  getConfiguration(): GameConfiguration {
    return { ...this.config };
  }

  getSection<K extends keyof GameConfiguration>(section: K): GameConfiguration[K] {
    return { ...this.config[section] } as GameConfiguration[K];
  }

  setSection<K extends keyof GameConfiguration>(section: K, value: GameConfiguration[K]): void {
    this.config[section] = { ...value } as GameConfiguration[K];
    this.markDirty();
    this.notifyListeners(section, value);
  }

  getSetting<K extends keyof GameConfiguration, S extends keyof GameConfiguration[K]>(
    section: K,
    key: S
  ): GameConfiguration[K][S] {
    return this.config[section][key];
  }

  setSetting<
    K extends keyof GameConfiguration,
    S extends keyof GameConfiguration[K]
  >(
    section: K,
    key: S,
    value: GameConfiguration[K][S]
  ): void {
    this.config[section][key] = value;
    this.markDirty();
    this.notifyListeners(`${section}.${String(key)}`, value);
  }

  resetSection<K extends keyof GameConfiguration>(section: K): void {
    const defaultSection = this.defaults[section];
    this.config[section] = JSON.parse(JSON.stringify(defaultSection));
    this.markDirty();
    this.notifyListeners(section, this.config[section]);
  }

  resetAll(): void {
    this.config = JSON.parse(JSON.stringify(this.defaults));
    this.markDirty();
    this.notifyListeners('all', this.config);
  }

  resetToDefaults(): void {
    this.config = this.createDefaultConfiguration();
    this.markDirty();
    this.save();
  }

  private markDirty(): void {
    this.isDirty = true;
  }

  private markClean(): void {
    this.isDirty = false;
  }

  isConfigurationDirty(): boolean {
    return this.isDirty;
  }

  save(): boolean {
    try {
      const configData = JSON.stringify(this.config);
      this.storage.setItem('game_config', configData);

      const validationResult = this.validation.validate(this.config);
      if (!validationResult.valid) {
        console.warn('Configuration validation warnings:', validationResult.errors);
      }

      this.markClean();
      return true;
    } catch (error) {
      console.error('Failed to save configuration:', error);
      return false;
    }
  }

  load(): boolean {
    try {
      const configData = this.storage.getItem('game_config');
      if (!configData) {
        this.config = this.createDefaultConfiguration();
        return true;
      }

      const loadedConfig = JSON.parse(configData);

      const migrationResult = this.migration.migrate(loadedConfig);
      this.config = migrationConfig;

      const validationResult = this.validation.validate(this.config);
      if (!validationResult.valid) {
        console.warn('Configuration has invalid settings:', validationResult.errors);
        this.validation.applyDefaults(this.config, validationResult.errors);
      }

      this.markClean();
      return true;
    } catch (error) {
      console.error('Failed to load configuration:', error);
      this.config = this.createDefaultConfiguration();
      return false;
    }
  }

  loadConfiguration(): GameConfiguration {
    this.load();
    return this.config;
  }

  exportConfiguration(): string {
    return JSON.stringify(this.config, null, 2);
  }

  importConfiguration(jsonString: string): boolean {
    try {
      const importedConfig = JSON.parse(jsonString);

      const validationResult = this.validation.validate(importedConfig);
      if (!validationResult.valid) {
        console.error('Imported configuration is invalid:', validationResult.errors);
        return false;
      }

      this.config = importedConfig;
      this.markDirty();
      this.save();

      return true;
    } catch (error) {
      console.error('Failed to import configuration:', error);
      return false;
    }
  }

  exportProfile(): string {
    return JSON.stringify(this.config.profile, null, 2);
  }

  importProfile(jsonString: string): boolean {
    try {
      const importedProfile = JSON.parse(jsonString);
      this.config.profile = importedProfile;
      this.markDirty();
      this.save();
      return true;
    } catch (error) {
      console.error('Failed to import profile:', error);
      return false;
    }
  }

  onSettingChange<K extends keyof GameConfiguration>(
    section: K,
    callback: (value: GameConfiguration[K]) => void
  ): () => void {
    const key = String(section);
    if (!this.listeners.has(key)) {
      this.listeners.set(key, new Set());
    }
    this.listeners.get(key)!.add(callback as (value: unknown) => void);

    return () => {
      this.listeners.get(key)?.delete(callback as (value: unknown) => void);
    };
  }

  private notifyListeners(path: string, value: unknown): void {
    const listeners = this.listeners.get(path);
    if (listeners) {
      listeners.forEach(callback => callback(value));
    }

    const sectionListeners = this.listeners.get('all');
    if (sectionListeners) {
      sectionListeners.forEach(callback => callback(value));
    }
  }

  startAutoSave(intervalMs: number = 60000): void {
    if (this.autoSaveInterval) {
      this.stopAutoSave();
    }

    this.autoSaveInterval = setInterval(() => {
      if (this.isConfigurationDirty()) {
        this.save();
      }
    }, intervalMs);
  }

  stopAutoSave(): void {
    if (this.autoSaveInterval) {
      clearInterval(this.autoSaveInterval);
      this.autoSaveInterval = null;
    }
  }

  createBackup(): string {
    const backup = {
      ...this.config,
      _backupTime: Date.now(),
      _backupVersion: this.CONFIG_VERSION
    };
    return JSON.stringify(backup);
  }

  restoreFromBackup(backupJson: string): boolean {
    try {
      const backup = JSON.parse(backupJson);

      if (!backup._backupTime || !backup._backupVersion) {
        console.error('Invalid backup format');
        return false;
      }

      const { _backupTime, _backupVersion, ...configData } = backup;

      this.config = configData;
      this.markDirty();
      this.save();

      return true;
    } catch (error) {
      console.error('Failed to restore from backup:', error);
      return false;
    }
  }

  compareConfigurations(other: GameConfiguration): ConfigurationDiff[] {
    const diffs: ConfigurationDiff[] = [];

    const compareRecursive = (path: string, value1: unknown, value2: unknown): void => {
      if (typeof value1 === 'object' && typeof value2 === 'object' && value1 !== null && value2 !== null) {
        const keys1 = Object.keys(value1 as object);
        const keys2 = Object.keys(value2 as object);

        const allKeys = new Set([...keys1, ...keys2]);

        allKeys.forEach(key => {
          const newPath = path ? `${path}.${key}` : key;
          const v1 = (value1 as Record<string, unknown>)[key];
          const v2 = (value2 as Record<string, unknown>)[key];

          if (v1 === undefined && v2 !== undefined) {
            diffs.push({ path: newPath, type: 'added', value: v2 });
          } else if (v1 !== undefined && v2 === undefined) {
            diffs.push({ path: newPath, type: 'removed', value: v1 });
          } else if (JSON.stringify(v1) !== JSON.stringify(v2)) {
            if (typeof v1 !== 'object' || typeof v2 !== 'object') {
              diffs.push({ path: newPath, type: 'changed', oldValue: v1, newValue: v2 });
            } else {
              compareRecursive(newPath, v1, v2);
            }
          }
        });
      } else if (value1 !== value2) {
        diffs.push({ path, type: 'changed', oldValue: value1, newValue: value2 });
      }
    };

    compareRecursive('', this.defaults, this.config);

    return diffs;
  }

  getDefaults(): GameConfiguration {
    return { ...this.defaults };
  }

  getVersion(): string {
    return this.config.version;
  }

  getConfigSize(): number {
    const jsonString = JSON.stringify(this.config);
    return new Blob([jsonString]).size;
  }
}

interface ConfigurationDiff {
  path: string;
  type: 'added' | 'removed' | 'changed';
  value?: unknown;
  oldValue?: unknown;
  newValue?: unknown;
}

class StorageAdapter {
  private prefix: string = 'redalert_';

  setItem(key: string, value: string): void {
    try {
      localStorage.setItem(this.prefix + key, value);
    } catch (error) {
      console.error('Storage setItem failed:', error);
      this.handleStorageError(error);
    }
  }

  getItem(key: string): string | null {
    try {
      return localStorage.getItem(this.prefix + key);
    } catch (error) {
      console.error('Storage getItem failed:', error);
      return null;
    }
  }

  removeItem(key: string): void {
    try {
      localStorage.removeItem(this.prefix + key);
    } catch (error) {
      console.error('Storage removeItem failed:', error);
    }
  }

  private handleStorageError(error: unknown): void {
    if (error instanceof DOMException && error.name === 'QuotaExceededError') {
      console.error('Storage quota exceeded, attempting cleanup...');
      this.cleanupOldData();
    }
  }

  private cleanupOldData(): void {
    const keysToRemove: string[] = [];

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key?.startsWith(this.prefix + 'backup_')) {
        keysToRemove.push(key);
      }
    }

    keysToRemove.forEach(key => localStorage.removeItem(key));
  }
}

class ConfigValidator {
  validate(config: GameConfiguration): ValidationResult {
    const errors: ValidationError[] = [];

    if (!config.version) {
      errors.push({ path: 'version', message: 'Version is required' });
    }

    if (config.graphics.resolution.width < 640 || config.graphics.resolution.height < 480) {
      errors.push({
        path: 'graphics.resolution',
        message: 'Resolution too small'
      });
    }

    if (config.audio.masterVolume < 0 || config.audio.masterVolume > 1) {
      errors.push({
        path: 'audio.masterVolume',
        message: 'Volume must be between 0 and 1'
      });
    }

    if (config.controls.mouseSettings.sensitivity < 0.1 || config.controls.mouseSettings.sensitivity > 10) {
      errors.push({
        path: 'controls.mouseSettings.sensitivity',
        message: 'Mouse sensitivity must be between 0.1 and 10'
      });
    }

    return {
      valid: errors.length === 0,
      errors
    };
  }

  applyDefaults(config: GameConfiguration, errors: ValidationError[]): void {
    errors.forEach(error => {
      const pathParts = error.path.split('.');
      let current: Record<string, unknown> = config as unknown as Record<string, unknown>;

      for (let i = 0; i < pathParts.length - 1; i++) {
        if (current[pathParts[i]] && typeof current[pathParts[i]] === 'object') {
          current = current[pathParts[i]] as Record<string, unknown>;
        }
      }

      const lastKey = pathParts[pathParts.length - 1];
      const defaultConfig = getDefaultConfig();

      let defaultValue: unknown = defaultConfig;
      pathParts.forEach(part => {
        if (defaultValue && typeof defaultValue === 'object') {
          defaultValue = (defaultValue as Record<string, unknown>)[part];
        }
      });

      if (defaultValue !== undefined) {
        current[lastKey] = defaultValue;
      }
    });
  }
}

class ConfigMigrator {
  migrate(config: GameConfiguration): GameConfiguration {
    let migrated = { ...config };

    if (!migrated.version) {
      migrated.version = '1.0.0';
    }

    const versionParts = migrated.version.split('.').map(Number);
    const targetParts = [1, 0, 0];

    if (versionParts[0] < targetParts[0] || versionParts[1] < targetParts[1]) {
      migrated = this.migrateFromOldVersion(migrated);
    }

    migrated.version = '1.0.0';

    return migrated;
  }

  private migrateFromOldVersion(config: GameConfiguration): GameConfiguration {
    const migrated = { ...config };

    migrated.accessibility = {
      ...this.createDefaultAccessibilitySettings(),
      ...migrated.accessibility
    };

    migrated.network = {
      ...this.createDefaultNetworkSettings(),
      ...migrated.network
    };

    migrated.ui = {
      ...this.createDefaultUISettings(),
      ...migrated.ui
    };

    migrated.modding = {
      ...this.createDefaultModdingSettings(),
      ...migrated.modding
    };

    return migrated;
  }

  private createDefaultAccessibilitySettings(): AccessibilitySettings {
    return {
      colorBlind: { enabled: false, type: 'none', intensity: 0, customFilters: [] },
      audio: {
        monoAudio: false,
        panningStrength: 1.0,
        separateVolumeControls: true,
        audioDescriptions: false,
        importantSoundIndicators: false,
        spatialAudioDisabled: false,
        reduceAudioVariety: false
      },
      visual: {
        highContrast: false,
        largeText: false,
        textScaling: 1.0,
        uiScaling: 1.0,
        boldText: false,
        reduceMotion: false,
        reduceTransparency: false,
        reduceAnimations: false,
        flashReduction: 'none',
        screenReaderEnabled: false,
        screenReaderVerbosity: 'normal',
        focusIndicators: true,
        focusIndicatorSize: 1.0,
        colorEnhancement: false
      },
      motor: {
        aimAssist: false,
        aimAssistStrength: 0.5,
        autoAim: false,
        autoAimStrength: 0.5,
        slowMode: false,
        slowModeMultiplier: 0.5,
        oneHandedMode: false,
        remapControllerButtons: false,
        buttonReassignment: [],
        macroSupport: false,
        macroList: [],
        toggleActions: [],
        holdToSprint: false,
        autoSprint: false,
        doublePressToRun: false
      },
      cognitive: {
        simplifiedUI: false,
        objectiveMarkers: true,
        objectiveHistory: true,
        hintsEnabled: true,
        hintFrequency: 'medium',
        tutorialMode: false,
        autoPathfinding: true,
        autoCover: false,
        autoQueue: false,
        simplifiedControls: false,
        gameSpeedControl: true,
        pauseOnDemand: true,
        clearGoals: true,
        progressIndicators: true,
        timeExtensions: false,
        additionalLives: false
      },
      subtitle: {
        enabled: false,
        size: 'medium',
        color: '#FFFFFF',
        backgroundColor: '#000000',
        backgroundOpacity: 0.75,
        speakerNames: true,
        importantSounds: true,
        ambientSounds: false,
        textPosition: 'bottom',
        maximumLines: 2,
        fontFamily: 'default'
      },
      motion: {
        reducedMotion: false,
        cameraShake: 'full',
        screenEffects: true,
        parallaxEffects: true,
        depthEffects: true,
        zoomEffects: true,
        transitionSpeed: 'normal'
      }
    };
  }

  private createDefaultNetworkSettings(): NetworkSettings {
    return {
      connectionType: 'direct',
      serverRegion: 'na',
      preferredServer: '',
      autoRegionSelect: true,
      bandwidthLimit: 0,
      packetSize: 1400,
      pingLimit: 200,
      allowConnections: true,
      allowInvites: true,
      maxConnections: 8,
      qualityMode: 'auto',
      upnpEnabled: true,
      natType: 'moderate',
      relayServerEnabled: false,
      encryption: 'tls',
      macAddressSpoofing: false,
      proxyEnabled: false,
      proxyServer: '',
      proxyPort: 0
    };
  }

  private createDefaultUISettings(): UISettings {
    return {
      theme: 'dark',
      colorScheme: 'default',
      language: 'en',
      dateFormat: 'mm/dd/yyyy',
      timeFormat: '24h',
      numberFormat: '1,000.00',
      currencyFormat: 'symbol',
      showFPS: false,
      showPing: false,
      showStats: false,
      showDecorations: true,
      showEffects: true,
      showParticles: true,
      showShadows: true,
      showWeather: true,
      showNametags: true,
      showDamageNumbers: true,
      showKillFeed: true,
      showMinimap: true,
      minimapSize: 'medium',
      minimapPosition: 'top_right',
      chatPosition: 'left',
      chatOpacity: 0.8,
      chatTimestamps: true,
      chatFilterProfanity: false,
      chatAutoScroll: true,
      notificationsPosition: 'top_right',
      notificationDuration: 5,
      notificationSound: true,
      showTooltips: true,
      tooltipDelay: 200,
      showTutorial: true,
      hudStyle: 'modern',
      hudColor: 'default',
      healthBars: 'damaged',
      unitIcons: 'icons',
      resourceDisplay: 'detailed',
      clockDisplay: 'game_time',
      matchTimerDisplay: true,
      spectatorOverlay: false
    };
  }

  private createDefaultModdingSettings(): ModdingSettings {
    return {
      modsEnabled: false,
      installedMods: [],
      enabledMods: [],
      modOrder: [],
      modPaths: [],
      contentPacks: [],
      autoLoadMods: false,
      modValidation: true,
      unsafeModsAllowed: false,
      modAPIEnabled: false,
      scriptSandbox: true,
      customAssets: [],
      assetOverrides: [],
      modBrowser: {
        sortBy: 'popularity',
        sortOrder: 'desc',
        filterType: 'all',
        filterCategory: [],
        searchQuery: '',
        showInstalled: true,
        showNotInstalled: true,
        showOutdated: true,
        showEnabled: true,
        showDisabled: true
      },
      workshopEnabled: true,
      workshopAutoUpdate: true
    };
  }
}

interface ValidationResult {
  valid: boolean;
  errors: ValidationError[];
}

interface ValidationError {
  path: string;
  message: string;
}

function getDefaultConfig(): GameConfiguration {
  return {
    version: '1.0.0',
    profile: {} as PlayerProfile,
    graphics: {} as GraphicsSettings,
    audio: {} as AudioSettings,
    video: {} as VideoSettings,
    gameplay: {} as GameplaySettings,
    controls: {} as ControlSettings,
    accessibility: {} as AccessibilitySettings,
    network: {} as NetworkSettings,
    ui: {} as UISettings,
    modding: {} as ModdingSettings,
    custom: {}
  };
}

export const configManager = new ConfigurationManager();

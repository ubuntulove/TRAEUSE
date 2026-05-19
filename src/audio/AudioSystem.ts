import { EventEmitter } from 'events';

export enum SoundType {
  EFFECT = 'effect',
  MUSIC = 'music',
  AMBIENT = 'ambient',
  VOICE = 'voice',
  UI = 'ui',
  COMBAT = 'combat',
  BUILDING = 'building',
  UNIT = 'unit',
  ENVIRONMENT = 'environment',
  WEATHER = 'weather',
  EXPLORATION = 'exploration',
}

export enum AudioChannel {
  MASTER = 'master',
  MUSIC = 'music',
  SFX = 'sfx',
  VOICE = 'voice',
  AMBIENT = 'ambient',
  UI = 'ui',
  COMBAT = 'combat',
  ENVIRONMENT = 'environment',
}

export enum AudioPriority {
  CRITICAL = 100,
  HIGH = 75,
  NORMAL = 50,
  LOW = 25,
  BACKGROUND = 10,
}

export enum AudioFadeType {
  LINEAR = 'linear',
  EXPONENTIAL = 'exponential',
  LOGARITHMIC = 'logarithmic',
  S_CURVE = 's_curve',
}

export enum AudioBusType {
  MIXER = 'mixer',
  SUBMIX = 'submix',
  AUX_SEND = 'aux_send',
  EFFECT_CHAIN = 'effect_chain',
}

export enum AudioEffectType {
  REVERB = 'reverb',
  DELAY = 'delay',
  EQ = 'eq',
  COMPRESSOR = 'compressor',
  DISTORTION = 'distortion',
  FLANGER = 'flanger',
  CHORUS = 'chorus',
  LOW_PASS = 'low_pass',
  HIGH_PASS = 'high_pass',
  WAH = 'wah',
}

export enum SpatialMode {
  NONE = 'none',
  STEREO = 'stereo',
  MONO = 'mono',
  SURROUND = 'surround',
  DOLBY_ATMOS = 'dolby_atmos',
}

export enum DistanceModel {
  LINEAR = 'linear',
  INVERSE = 'inverse',
  EXPONENTIAL = 'exponential',
}

export enum VoiceLineCategory {
  COMMAND = 'command',
  ATTACK = 'attack',
  MOVE = 'move',
  BUILD = 'build',
  SELECT = 'select',
  IDLE = 'idle',
  DEATH = 'death',
  VICTORY = 'victory',
  DEFEAT = 'defeat',
  ALERT = 'alert',
  GATHER = 'gather',
  REPAIR = 'repair',
  UPGRADE = 'upgrade',
}

export enum GameState {
  MENU = 'menu',
  INTRO = 'intro',
  PLAYING = 'playing',
  PAUSED = 'paused',
  COMBAT = 'combat',
  EXPLORATION = 'exploration',
  VICTORY = 'victory',
  DEFEAT = 'defeat',
  BUILDING = 'building',
  MULTIPLAYER = 'multiplayer',
}

export enum MusicLayer {
  BASE = 'base',
  TENSION = 'tension',
  ACTION = 'action',
  CLIMAX = 'climax',
  RESOLVE = 'resolve',
  MENU = 'menu',
  VICTORY = 'victory',
  DEFEAT = 'defeat',
}

export enum AudioCueEvent {
  START = 'start',
  STOP = 'stop',
  COMPLETE = 'complete',
  LOOP = 'loop',
  FADE_START = 'fade_start',
  FADE_COMPLETE = 'fade_complete',
  ERROR = 'error',
}

export interface AudioConfig {
  sampleRate?: number;
  channels?: number;
  bitDepth?: number;
  bufferSize?: number;
  maxVoices?: number;
  spatialAudioEnabled?: boolean;
  distanceModel?: DistanceModel;
  dopplerFactor?: number;
  speedOfSound?: number;
  masterVolume?: number;
  muted?: boolean;
}

export interface AudioEvent {
  id: string;
  type: AudioCueEvent;
  timestamp: number;
  data?: any;
  source?: string;
}

export interface SoundBank {
  id: string;
  name: string;
  sounds: Map<string, SoundEffect>;
  loadOrder: number;
  loaded: boolean;
  memorySize: number;
  metadata?: Record<string, any>;
}

export interface SoundEffect {
  id: string;
  name: string;
  type: SoundType;
  filePath?: string;
  buffer?: AudioBuffer;
  duration: number;
  volume: number;
  pitch: number;
  pan?: number;
  loop: boolean;
  priority: AudioPriority;
  fadeIn?: number;
  fadeOut?: number;
  tags?: string[];
  metadata?: Record<string, any>;
}

export interface MusicTrack {
  id: string;
  name: string;
  filePath?: string;
  buffer?: AudioBuffer;
  duration: number;
  volume: number;
  fadeIn: number;
  fadeOut: number;
  loop: boolean;
  layers: Map<MusicLayer, AudioBuffer>;
  tempo?: number;
  key?: string;
  tags?: string[];
  metadata?: Record<string, any>;
}

export interface AmbientSound {
  id: string;
  name: string;
  filePath?: string;
  buffer?: AudioBuffer;
  duration: number;
  volume: number;
  loop: boolean;
  crossfadeDuration: number;
  tags?: string[];
  metadata?: Record<string, any>;
}

export interface VoiceLine {
  id: string;
  unitType: string;
  category: VoiceLineCategory;
  filePath?: string;
  buffer?: AudioBuffer;
  duration: number;
  volume: number;
  pitch: number;
  text?: string;
  emotion?: string;
  priority: AudioPriority;
  cooldown?: number;
  lastPlayed?: number;
  metadata?: Record<string, any>;
}

export interface AudioCue {
  id: string;
  name: string;
  sounds: (string | SoundEffect)[];
  playbackMode: 'sequential' | 'random' | 'simultaneous' | 'weighted_random';
  loop: boolean;
  transitionTime: number;
  fadeType: AudioFadeType;
  crossfadeEnabled: boolean;
  priority: AudioPriority;
  volume: number;
  pitch?: number;
  tags?: string[];
  metadata?: Record<string, any>;
}

export interface AudioBus {
  id: string;
  name: string;
  type: AudioBusType;
  volume: number;
  muted: boolean;
  solo: boolean;
  parent?: string;
  effects: AudioEffect[];
  children: string[];
  sendLevels: Map<string, number>;
}

export interface AudioFade {
  id: string;
  startTime: number;
  duration: number;
  type: AudioFadeType;
  startVolume: number;
  endVolume: number;
  curve?: (t: number) => number;
}

export interface AudioEffect {
  id: string;
  type: AudioEffectType;
  enabled: boolean;
  parameters: Record<string, number>;
}

export interface SpatialAudioConfig {
  position: { x: number; y: number; z: number };
  orientation: { x: number; y: number; z: number };
  velocity?: { x: number; y: number; z: number };
  refDistance: number;
  maxDistance: number;
  rolloffFactor: number;
  coneInnerAngle: number;
  coneOuterAngle: number;
  coneOuterGain: number;
}

export interface VolumeSettings {
  master: number;
  music: number;
  sfx: number;
  voice: number;
  ambient: number;
  ui: number;
  combat: number;
  environment: number;
  channels: Map<AudioChannel, number>;
}

export interface AudioMixerNode {
  id: string;
  type: string;
  node: AudioNode;
  gainNode?: GainNode;
  panNode?: StereoPannerNode;
  filterNode?: BiquadFilterNode;
  children: AudioMixerNode[];
}

export interface SoundPoolItem {
  source: AudioBufferSourceNode | null;
  buffer: AudioBuffer;
  gainNode: GainNode;
  playing: boolean;
  startTime: number;
  fadeId?: string;
  priority: AudioPriority;
}

export interface DynamicMusicConfig {
  gameState: GameState;
  combatIntensity: number;
  tensionLevel: number;
  explorationProgress: number;
  playerFaction?: string;
  timeOfDay?: string;
  weatherType?: string;
}

export interface AudioState {
  isPlaying: boolean;
  currentMusic: string | null;
  currentAmbient: string | null;
  activeVoices: number;
  activeEffects: number;
  totalVoices: number;
  masterVolume: number;
  muted: boolean;
  gameState: GameState;
}

export interface AudioSettings {
  config: AudioConfig;
  volume: VolumeSettings;
  spatial: SpatialAudioConfig;
  dynamicMusic: DynamicMusicConfig;
  muteSettings: Map<AudioChannel, boolean>;
  soloSettings: Map<AudioChannel, boolean>;
}

export class AudioSettingsManager {
  private settings: AudioSettings;
  private defaultSettings: AudioSettings;

  constructor() {
    this.defaultSettings = this.createDefaultSettings();
    this.settings = this.cloneSettings(this.defaultSettings);
  }

  private createDefaultSettings(): AudioSettings {
    return {
      config: {
        sampleRate: 44100,
        channels: 2,
        bitDepth: 16,
        bufferSize: 4096,
        maxVoices: 32,
        spatialAudioEnabled: true,
        distanceModel: DistanceModel.INVERSE,
        dopplerFactor: 1.0,
        speedOfSound: 343,
        masterVolume: 1.0,
        muted: false,
      },
      volume: {
        master: 1.0,
        music: 0.7,
        sfx: 0.8,
        voice: 0.9,
        ambient: 0.6,
        ui: 0.7,
        combat: 0.8,
        environment: 0.6,
        channels: new Map(),
      },
      spatial: {
        position: { x: 0, y: 0, z: 0 },
        orientation: { x: 0, y: 0, z: -1 },
        refDistance: 1,
        maxDistance: 100,
        rolloffFactor: 1,
        coneInnerAngle: 360,
        coneOuterAngle: 360,
        coneOuterGain: 0,
      },
      dynamicMusic: {
        gameState: GameState.MENU,
        combatIntensity: 0,
        tensionLevel: 0,
        explorationProgress: 0,
      },
      muteSettings: new Map(),
      soloSettings: new Map(),
    };
  }

  private cloneSettings(settings: AudioSettings): AudioSettings {
    return {
      config: { ...settings.config },
      volume: {
        ...settings.volume,
        channels: new Map(settings.volume.channels),
      },
      spatial: { ...settings.spatial },
      dynamicMusic: { ...settings.dynamicMusic },
      muteSettings: new Map(settings.muteSettings),
      soloSettings: new Map(settings.soloSettings),
    };
  }

  getSettings(): AudioSettings {
    return this.settings;
  }

  getConfig(): AudioConfig {
    return this.settings.config;
  }

  getVolume(): VolumeSettings {
    return this.settings.volume;
  }

  getSpatialConfig(): SpatialAudioConfig {
    return this.settings.spatial;
  }

  getDynamicMusicConfig(): DynamicMusicConfig {
    return this.settings.dynamicMusic;
  }

  updateConfig(config: Partial<AudioConfig>): void {
    this.settings.config = { ...this.settings.config, ...config };
  }

  updateVolume(channel: AudioChannel, volume: number): void {
    this.settings.volume[channel] = Math.max(0, Math.min(1, volume));
    this.settings.volume.channels.set(channel, this.settings.volume[channel]);
  }

  setMasterVolume(volume: number): void {
    this.settings.volume.master = Math.max(0, Math.min(1, volume));
  }

  setChannelVolume(channel: AudioChannel, volume: number): void {
    this.settings.volume[channel] = Math.max(0, Math.min(1, volume));
    this.settings.volume.channels.set(channel, volume);
  }

  setMute(channel: AudioChannel, muted: boolean): void {
    this.settings.muteSettings.set(channel, muted);
  }

  setSolo(channel: AudioChannel, solo: boolean): void {
    this.settings.soloSettings.set(channel, solo);
  }

  isMuted(channel: AudioChannel): boolean {
    return this.settings.muteSettings.get(channel) || false;
  }

  isSolo(channel: AudioChannel): boolean {
    return this.settings.soloSettings.get(channel) || false;
  }

  updateSpatialConfig(config: Partial<SpatialAudioConfig>): void {
    this.settings.spatial = { ...this.settings.spatial, ...config };
  }

  updateDynamicMusicConfig(config: Partial<DynamicMusicConfig>): void {
    this.settings.dynamicMusic = { ...this.settings.dynamicMusic, ...config };
  }

  resetToDefaults(): void {
    this.settings = this.cloneSettings(this.defaultSettings);
  }

  loadSettings(settings: AudioSettings): void {
    this.settings = this.cloneSettings(settings);
  }

  serializeSettings(): string {
    const serializable = {
      ...this.settings,
      volume: {
        ...this.settings.volume,
        channels: Array.from(this.settings.volume.channels.entries()),
      },
      muteSettings: Array.from(this.settings.muteSettings.entries()),
      soloSettings: Array.from(this.settings.soloSettings.entries()),
    };
    return JSON.stringify(serializable);
  }

  deserializeSettings(json: string): void {
    const parsed = JSON.parse(json);
    this.settings = {
      ...parsed,
      volume: {
        ...parsed.volume,
        channels: new Map(parsed.volume.channels),
      },
      muteSettings: new Map(parsed.muteSettings),
      soloSettings: new Map(parsed.soloSettings),
    };
  }
}

export class AudioPool {
  private pool: Map<string, SoundPoolItem[]>;
  private maxPoolSize: number;
  private maxVoices: number;
  private activeVoices: number;
  private audioContext: AudioContext | null;
  private masterGain: GainNode | null;

  constructor(maxPoolSize: number = 10, maxVoices: number = 32) {
    this.pool = new Map();
    this.maxPoolSize = maxPoolSize;
    this.maxVoices = maxVoices;
    this.activeVoices = 0;
    this.audioContext = null;
    this.masterGain = null;
  }

  initialize(audioContext: AudioContext, masterGain: GainNode): void {
    this.audioContext = audioContext;
    this.masterGain = masterGain;
  }

  addToPool(soundId: string, buffer: AudioBuffer): void {
    if (!this.pool.has(soundId)) {
      this.pool.set(soundId, []);
    }

    const items = this.pool.get(soundId)!;
    if (items.length < this.maxPoolSize) {
      const gainNode = this.audioContext!.createGain();
      gainNode.connect(this.masterGain!);

      items.push({
        source: null,
        buffer,
        gainNode,
        playing: false,
        startTime: 0,
        priority: AudioPriority.NORMAL,
      });
    }
  }

  play(
    soundId: string,
    volume: number = 1,
    pitch: number = 1,
    loop: boolean = false,
    priority: AudioPriority = AudioPriority.NORMAL
  ): AudioBufferSourceNode | null {
    if (!this.audioContext || !this.masterGain) {
      console.error('AudioPool not initialized');
      return null;
    }

    if (this.activeVoices >= this.maxVoices) {
      this.freeLowPriorityVoice(priority);
    }

    if (!this.pool.has(soundId)) {
      console.warn(`Sound ${soundId} not found in pool`);
      return null;
    }

    const items = this.pool.get(soundId)!;
    const availableItem = items.find((item) => !item.playing);

    if (!availableItem) {
      if (items.length < this.maxPoolSize) {
        const newItem = this.createPoolItem(soundId);
        if (newItem) {
          items.push(newItem);
          return this.playFromItem(newItem, volume, pitch, loop, priority);
        }
      }
      return null;
    }

    return this.playFromItem(availableItem, volume, pitch, loop, priority);
  }

  private createPoolItem(soundId: string): SoundPoolItem | null {
    if (!this.audioContext || !this.masterGain) return null;

    const gainNode = this.audioContext.createGain();
    gainNode.connect(this.masterGain);

    return {
      source: null,
      buffer: this.pool.get(soundId)?.[0]?.buffer || this.audioContext.createBuffer(1, 1, 44100),
      gainNode,
      playing: false,
      startTime: 0,
      priority: AudioPriority.NORMAL,
    };
  }

  private playFromItem(
    item: SoundPoolItem,
    volume: number,
    pitch: number,
    loop: boolean,
    priority: AudioPriority
  ): AudioBufferSourceNode | null {
    if (!this.audioContext) return null;

    const source = this.audioContext.createBufferSource();
    source.buffer = item.buffer;
    source.playbackRate.value = pitch;
    source.loop = loop;

    source.connect(item.gainNode);
    item.gainNode.gain.value = volume;

    source.start(0);
    item.source = source;
    item.playing = true;
    item.startTime = this.audioContext.currentTime;
    item.priority = priority;

    this.activeVoices++;

    source.onended = () => {
      item.playing = false;
      item.source = null;
      this.activeVoices--;
    };

    return source;
  }

  private freeLowPriorityVoice(priority: AudioPriority): void {
    for (const [soundId, items] of this.pool.entries()) {
      for (const item of items) {
        if (item.playing && item.priority < priority) {
          this.stopItem(item);
          return;
        }
      }
    }
  }

  stopItem(item: SoundPoolItem): void {
    if (item.source && item.playing) {
      try {
        item.source.stop();
      } catch (e) {
      }
      item.playing = false;
      item.source = null;
      this.activeVoices--;
    }
  }

  stop(soundId: string): void {
    if (!this.pool.has(soundId)) return;

    const items = this.pool.get(soundId)!;
    for (const item of items) {
      this.stopItem(item);
    }
  }

  stopAll(): void {
    for (const [soundId, items] of this.pool.entries()) {
      for (const item of items) {
        this.stopItem(item);
      }
    }
  }

  getActiveVoices(): number {
    return this.activeVoices;
  }

  getAvailableVoices(): number {
    return this.maxVoices - this.activeVoices;
  }

  clearPool(): void {
    this.stopAll();
    this.pool.clear();
  }

  removeFromPool(soundId: string): void {
    if (this.pool.has(soundId)) {
      this.stop(soundId);
      this.pool.delete(soundId);
    }
  }

  getPoolSize(soundId?: string): number {
    if (soundId) {
      return this.pool.get(soundId)?.length || 0;
    }
    let total = 0;
    for (const items of this.pool.values()) {
      total += items.length;
    }
    return total;
  }

  isPlaying(soundId: string): boolean {
    if (!this.pool.has(soundId)) return false;
    return this.pool.get(soundId)!.some((item) => item.playing);
  }
}

export class AudioFadeManager {
  private fades: Map<string, AudioFade>;
  private activeFades: Map<string, NodeJS.Timeout>;

  constructor() {
    this.fades = new Map();
    this.activeFades = new Map();
  }

  createFade(
    id: string,
    duration: number,
    startVolume: number,
    endVolume: number,
    type: AudioFadeType = AudioFadeType.LINEAR
  ): AudioFade {
    const fade: AudioFade = {
      id,
      startTime: Date.now(),
      duration,
      type,
      startVolume,
      endVolume,
      curve: this.getFadeCurve(type),
    };

    this.fades.set(id, fade);
    return fade;
  }

  private getFadeCurve(type: AudioFadeType): (t: number) => number {
    switch (type) {
      case AudioFadeType.LINEAR:
        return (t) => t;
      case AudioFadeType.EXPONENTIAL:
        return (t) => Math.pow(t, 2);
      case AudioFadeType.LOGARITHMIC:
        return (t) => Math.sqrt(t);
      case AudioFadeType.S_CURVE:
        return (t) => t * t * (3 - 2 * t);
      default:
        return (t) => t;
    }
  }

  startFade(
    gainNode: GainNode,
    fadeId: string,
    duration: number,
    targetVolume: number,
    type: AudioFadeType = AudioFadeType.LINEAR,
    onComplete?: () => void
  ): void {
    const startVolume = gainNode.gain.value;
    const fade = this.createFade(fadeId, duration, startVolume, targetVolume, type);

    const startTime = Date.now();
    const updateInterval = 16;

    const intervalId = setInterval(() => {
      const elapsed = Date.now() - startTime;
      const progress = Math.min(elapsed / duration, 1);
      const curveProgress = fade.curve!(progress);
      const newVolume = startVolume + (targetVolume - startVolume) * curveProgress;

      gainNode.gain.setValueAtTime(newVolume, gainNode.context.currentTime);

      if (progress >= 1) {
        clearInterval(intervalId);
        this.fades.delete(fadeId);
        this.activeFades.delete(fadeId);
        gainNode.gain.setValueAtTime(targetVolume, gainNode.context.currentTime);
        if (onComplete) {
          onComplete();
        }
      }
    }, updateInterval);

    this.activeFades.set(fadeId, intervalId);
  }

  cancelFade(fadeId: string): void {
    const intervalId = this.activeFades.get(fadeId);
    if (intervalId) {
      clearInterval(intervalId);
      this.activeFades.delete(fadeId);
      this.fades.delete(fadeId);
    }
  }

  cancelAllFades(): void {
    for (const intervalId of this.activeFades.values()) {
      clearInterval(intervalId);
    }
    this.activeFades.clear();
    this.fades.clear();
  }

  getFade(fadeId: string): AudioFade | undefined {
    return this.fades.get(fadeId);
  }

  isFading(fadeId: string): boolean {
    return this.activeFades.has(fadeId);
  }

  getActiveFades(): string[] {
    return Array.from(this.activeFades.keys());
  }
}

export class SpatialAudioManager {
  private audioContext: AudioContext | null = null;
  private listenerPosition: { x: number; y: number; z: number };
  private listenerOrientation: { x: number; y: number; z: number };
  private listenerVelocity: { x: number; y: number; z: number };
  private distanceModel: DistanceModel;
  private dopplerFactor: number;
  private speedOfSound: number;
  private spatialNodes: Map<string, AudioNode>;

  constructor() {
    this.listenerPosition = { x: 0, y: 0, z: 0 };
    this.listenerOrientation = { x: 0, y: 0, z: -1 };
    this.listenerVelocity = { x: 0, y: 0, z: 0 };
    this.distanceModel = DistanceModel.INVERSE;
    this.dopplerFactor = 1.0;
    this.speedOfSound = 343;
    this.spatialNodes = new Map();
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;
  }

  setListenerPosition(x: number, y: number, z: number): void {
    this.listenerPosition = { x, y, z };
  }

  setListenerOrientation(forwardX: number, forwardY: number, forwardZ: number): void {
    this.listenerOrientation = { x: forwardX, y: forwardY, z: forwardZ };
  }

  setListenerVelocity(x: number, y: number, z: number): void {
    this.listenerVelocity = { x, y, z };
  }

  updateListener(config: {
    position?: { x: number; y: number; z: number };
    orientation?: { x: number; y: number; z: number };
    velocity?: { x: number; y: number; z: number };
  }): void {
    if (config.position) {
      this.setListenerPosition(config.position.x, config.position.y, config.position.z);
    }
    if (config.orientation) {
      this.setListenerOrientation(config.orientation.x, config.orientation.y, config.orientation.z);
    }
    if (config.velocity) {
      this.setListenerVelocity(config.velocity.x, config.velocity.y, config.velocity.z);
    }
  }

  createSpatialSource(
    sourcePosition: { x: number; y: number; z: number },
    config: Partial<SpatialAudioConfig> = {}
  ): PannerNode | null {
    if (!this.audioContext) return null;

    const panner = this.audioContext.createPanner();
    panner.panningModel = 'HRTF';
    panner.distanceModel = this.distanceModel;
    panner.refDistance = config.refDistance || 1;
    panner.maxDistance = config.maxDistance || 100;
    panner.rolloffFactor = config.rolloffFactor || 1;
    panner.coneInnerAngle = config.coneInnerAngle || 360;
    panner.coneOuterAngle = config.coneOuterAngle || 360;
    panner.coneOuterGain = config.coneOuterGain || 0;

    panner.positionX.value = sourcePosition.x;
    panner.positionY.value = sourcePosition.y;
    panner.positionZ.value = sourcePosition.z;

    return panner;
  }

  updateSourcePosition(
    panner: PannerNode,
    position: { x: number; y: number; z: number }
  ): void {
    panner.positionX.setValueAtTime(position.x, panner.context.currentTime);
    panner.positionY.setValueAtTime(position.y, panner.context.currentTime);
    panner.positionZ.setValueAtTime(position.z, panner.context.currentTime);
  }

  calculateDistance(
    sourcePosition: { x: number; y: number; z: number }
  ): number {
    const dx = sourcePosition.x - this.listenerPosition.x;
    const dy = sourcePosition.y - this.listenerPosition.y;
    const dz = sourcePosition.z - this.listenerPosition.z;
    return Math.sqrt(dx * dx + dy * dy + dz * dz);
  }

  calculateGain(distance: number, refDistance: number, maxDistance: number, rolloffFactor: number): number {
    if (distance <= refDistance) return 1;
    if (distance >= maxDistance) return 0;

    switch (this.distanceModel) {
      case DistanceModel.LINEAR:
        return 1 - (distance - refDistance) / (maxDistance - refDistance);
      case DistanceModel.INVERSE:
        return refDistance / (refDistance + rolloffFactor * (distance - refDistance));
      case DistanceModel.EXPONENTIAL:
        return Math.pow(distance / refDistance, -rolloffFactor);
      default:
        return 1;
    }
  }

  calculateDopplerShift(
    sourceVelocity: { x: number; y: number; z: number },
    listenerVelocity: { x: number; y: number; z: number }
  ): number {
    const relativeVelocity = this.dotProduct(sourceVelocity, listenerVelocity);
    const velocityTowardListener = relativeVelocity / this.speedOfSound;
    return 1 / (1 + this.dopplerFactor * velocityTowardListener);
  }

  private dotProduct(a: { x: number; y: number; z: number }, b: { x: number; y: number; z: number }): number {
    return a.x * b.x + a.y * b.y + a.z * b.z;
  }

  setDistanceModel(model: DistanceModel): void {
    this.distanceModel = model;
  }

  setDopplerFactor(factor: number): void {
    this.dopplerFactor = Math.max(0, Math.min(5, factor));
  }

  setSpeedOfSound(speed: number): void {
    this.speedOfSound = speed;
  }

  getListenerPosition(): { x: number; y: number; z: number } {
    return { ...this.listenerPosition };
  }

  getListenerOrientation(): { x: number; y: number; z: number } {
    return { ...this.listenerOrientation };
  }

  getListenerVelocity(): { x: number; y: number; z: number } {
    return { ...this.listenerVelocity };
  }

  registerSpatialNode(id: string, node: AudioNode): void {
    this.spatialNodes.set(id, node);
  }

  unregisterSpatialNode(id: string): void {
    this.spatialNodes.delete(id);
  }

  getSpatialNode(id: string): AudioNode | undefined {
    return this.spatialNodes.get(id);
  }
}

export class AudioBusManager {
  private buses: Map<string, AudioBus>;
  private gainNodes: Map<string, GainNode>;
  private audioContext: AudioContext | null;
  private masterGain: GainNode | null;

  constructor() {
    this.buses = new Map();
    this.gainNodes = new Map();
    this.audioContext = null;
    this.masterGain = null;
  }

  initialize(audioContext: AudioContext, masterGain: GainNode): void {
    this.audioContext = audioContext;
    this.masterGain = masterGain;
    this.createDefaultBuses();
  }

  private createDefaultBuses(): void {
    const defaultBusIds = [
      AudioChannel.MASTER,
      AudioChannel.MUSIC,
      AudioChannel.SFX,
      AudioChannel.VOICE,
      AudioChannel.AMBIENT,
      AudioChannel.UI,
      AudioChannel.COMBAT,
      AudioChannel.ENVIRONMENT,
    ];

    for (const busId of defaultBusIds) {
      this.createBus({
        id: busId,
        name: busId,
        type: AudioBusType.MIXER,
        volume: 1.0,
        muted: false,
        solo: false,
      });
    }
  }

  createBus(config: {
    id: string;
    name: string;
    type: AudioBusType;
    volume?: number;
    muted?: boolean;
    solo?: boolean;
    parent?: string;
  }): AudioBus | null {
    if (!this.audioContext || !this.masterGain) {
      console.error('AudioBusManager not initialized');
      return null;
    }

    const gainNode = this.audioContext.createGain();
    gainNode.gain.value = config.volume || 1.0;

    if (config.parent) {
      const parentGain = this.gainNodes.get(config.parent);
      if (parentGain) {
        gainNode.connect(parentGain);
      } else {
        gainNode.connect(this.masterGain);
      }
    } else {
      gainNode.connect(this.masterGain);
    }

    const bus: AudioBus = {
      id: config.id,
      name: config.name,
      type: config.type,
      volume: config.volume || 1.0,
      muted: config.muted || false,
      solo: config.solo || false,
      parent: config.parent,
      effects: [],
      children: [],
      sendLevels: new Map(),
    };

    this.buses.set(config.id, bus);
    this.gainNodes.set(config.id, gainNode);

    if (config.parent) {
      const parentBus = this.buses.get(config.parent);
      if (parentBus) {
        parentBus.children.push(config.id);
      }
    }

    return bus;
  }

  getBus(busId: string): AudioBus | undefined {
    return this.buses.get(busId);
  }

  getGainNode(busId: string): GainNode | undefined {
    return this.gainNodes.get(busId);
  }

  setVolume(busId: string, volume: number): void {
    const bus = this.buses.get(busId);
    const gainNode = this.gainNodes.get(busId);

    if (bus && gainNode) {
      bus.volume = Math.max(0, Math.min(1, volume));
      if (!bus.muted) {
        gainNode.gain.setValueAtTime(bus.volume, gainNode.context.currentTime);
      }
    }
  }

  setMute(busId: string, muted: boolean): void {
    const bus = this.buses.get(busId);
    const gainNode = this.gainNodes.get(busId);

    if (bus && gainNode) {
      bus.muted = muted;
      gainNode.gain.setValueAtTime(muted ? 0 : bus.volume, gainNode.context.currentTime);
    }
  }

  setSolo(busId: string, solo: boolean): void {
    const bus = this.buses.get(busId);
    if (bus) {
      bus.solo = solo;
      this.updateSoloState();
    }
  }

  private updateSoloState(): void {
    const soloBuses = Array.from(this.buses.values()).filter((b) => b.solo);

    if (soloBuses.length === 0) {
      for (const [id, gainNode] of this.gainNodes.entries()) {
        const bus = this.buses.get(id)!;
        gainNode.gain.setValueAtTime(bus.muted ? 0 : bus.volume, gainNode.context.currentTime);
      }
    } else {
      for (const [id, gainNode] of this.gainNodes.entries()) {
        const bus = this.buses.get(id)!;
        const isInSoloChain = this.isInSoloChain(id, soloBuses);
        gainNode.gain.setValueAtTime(
          bus.muted || !isInSoloChain ? 0 : bus.volume,
          gainNode.context.currentTime
        );
      }
    }
  }

  private isInSoloChain(busId: string, soloBuses: AudioBus[]): boolean {
    const bus = this.buses.get(busId);
    if (!bus) return false;

    if (soloBuses.includes(bus)) return true;

    if (bus.parent) {
      return this.isInSoloChain(bus.parent, soloBuses);
    }

    return false;
  }

  addEffect(busId: string, effect: AudioEffect): void {
    const bus = this.buses.get(busId);
    if (bus) {
      bus.effects.push(effect);
    }
  }

  removeEffect(busId: string, effectId: string): void {
    const bus = this.buses.get(busId);
    if (bus) {
      bus.effects = bus.effects.filter((e) => e.id !== effectId);
    }
  }

  setSendLevel(fromBusId: string, toBusId: string, level: number): void {
    const fromBus = this.buses.get(fromBusId);
    const toGain = this.gainNodes.get(toBusId);

    if (fromBus && toGain) {
      fromBus.sendLevels.set(toBusId, level);

      const sendGain = this.audioContext!.createGain();
      sendGain.gain.value = level;
      sendGain.connect(toGain);

      const fromGain = this.gainNodes.get(fromBusId);
      if (fromGain) {
        fromGain.disconnect();
        fromGain.connect(sendGain);
        sendGain.connect(this.masterGain!);
      }
    }
  }

  getAllBuses(): AudioBus[] {
    return Array.from(this.buses.values());
  }

  removeBus(busId: string): void {
    const gainNode = this.gainNodes.get(busId);
    if (gainNode) {
      gainNode.disconnect();
      this.gainNodes.delete(busId);
    }

    const bus = this.buses.get(busId);
    if (bus) {
      if (bus.parent) {
        const parentBus = this.buses.get(bus.parent);
        if (parentBus) {
          parentBus.children = parentBus.children.filter((id) => id !== busId);
        }
      }
      this.buses.delete(busId);
    }
  }
}

export class AudioMixer {
  private audioContext: AudioContext | null;
  private masterGain: GainNode | null;
  private channelGains: Map<AudioChannel, GainNode>;
  private compressor: DynamicsCompressorNode | null;
  private limiter: DynamicsCompressorNode | null;
  private equalizer: BiquadFilterNode[];

  constructor() {
    this.audioContext = null;
    this.masterGain = null;
    this.channelGains = new Map();
    this.compressor = null;
    this.limiter = null;
    this.equalizer = [];
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;

    this.masterGain = audioContext.createGain();
    this.masterGain.gain.value = 1.0;

    this.compressor = audioContext.createDynamicsCompressor();
    this.compressor.threshold.value = -24;
    this.compressor.knee.value = 30;
    this.compressor.ratio.value = 12;
    this.compressor.attack.value = 0.003;
    this.compressor.release.value = 0.25;

    this.limiter = audioContext.createDynamicsCompressor();
    this.limiter.threshold.value = -3;
    this.limiter.knee.value = 0;
    this.limiter.ratio.value = 20;
    this.limiter.attack.value = 0.001;
    this.limiter.release.value = 0.1;

    this.createChannelGains();
    this.createEqualizer();

    this.masterGain.connect(this.compressor);
    this.compressor.connect(this.limiter);
    this.limiter.connect(audioContext.destination);
  }

  private createChannelGains(): void {
    if (!this.audioContext || !this.masterGain) return;

    const channels = [
      AudioChannel.MUSIC,
      AudioChannel.SFX,
      AudioChannel.VOICE,
      AudioChannel.AMBIENT,
      AudioChannel.UI,
      AudioChannel.COMBAT,
      AudioChannel.ENVIRONMENT,
    ];

    for (const channel of channels) {
      const gain = this.audioContext.createGain();
      gain.gain.value = 1.0;
      gain.connect(this.masterGain);
      this.channelGains.set(channel, gain);
    }
  }

  private createEqualizer(): void {
    if (!this.audioContext || !this.masterGain) return;

    const frequencies = [60, 250, 1000, 4000, 16000];
    const types: BiquadFilterType[] = ['lowshelf', 'peaking', 'peaking', 'peaking', 'highshelf'];
    const gains = [0, 0, 0, 0, 0];

    let lastNode: AudioNode = this.masterGain;

    for (let i = 0; i < frequencies.length; i++) {
      const filter = this.audioContext.createBiquadFilter();
      filter.type = types[i];
      filter.frequency.value = frequencies[i];
      filter.Q.value = 1;
      filter.gain.value = gains[i];

      if (i === 0) {
        this.masterGain.disconnect();
        this.masterGain.connect(filter);
      }

      lastNode.connect(filter);
      lastNode = filter;
      this.equalizer.push(filter);
    }

    lastNode.connect(this.compressor!);
  }

  getMasterGain(): GainNode | null {
    return this.masterGain;
  }

  getChannelGain(channel: AudioChannel): GainNode | undefined {
    return this.channelGains.get(channel);
  }

  setMasterVolume(volume: number): void {
    if (this.masterGain) {
      this.masterGain.gain.setValueAtTime(
        Math.max(0, Math.min(1, volume)),
        this.masterGain.context.currentTime
      );
    }
  }

  setChannelVolume(channel: AudioChannel, volume: number): void {
    const gain = this.channelGains.get(channel);
    if (gain) {
      gain.gain.setValueAtTime(
        Math.max(0, Math.min(1, volume)),
        gain.context.currentTime
      );
    }
  }

  setEqBand(index: number, frequency: number, gain: number, Q: number = 1): void {
    if (index >= 0 && index < this.equalizer.length) {
      const filter = this.equalizer[index];
      filter.frequency.setValueAtTime(frequency, filter.context.currentTime);
      filter.gain.setValueAtTime(gain, filter.context.currentTime);
      filter.Q.setValueAtTime(Q, filter.context.currentTime);
    }
  }

  setCompressor(threshold: number, knee: number, ratio: number, attack: number, release: number): void {
    if (this.compressor) {
      this.compressor.threshold.setValueAtTime(threshold, this.compressor.context.currentTime);
      this.compressor.knee.setValueAtTime(knee, this.compressor.context.currentTime);
      this.compressor.ratio.setValueAtTime(ratio, this.compressor.context.currentTime);
      this.compressor.attack.setValueAtTime(attack, this.compressor.context.currentTime);
      this.compressor.release.setValueAtTime(release, this.compressor.context.currentTime);
    }
  }

  setLimiter(threshold: number, ratio: number, attack: number, release: number): void {
    if (this.limiter) {
      this.limiter.threshold.setValueAtTime(threshold, this.limiter.context.currentTime);
      this.limiter.ratio.setValueAtTime(ratio, this.limiter.context.currentTime);
      this.limiter.attack.setValueAtTime(attack, this.limiter.context.currentTime);
      this.limiter.release.setValueAtTime(release, this.limiter.context.currentTime);
    }
  }

  getAnalyser(): AnalyserNode | null {
    if (!this.audioContext) return null;
    const analyser = this.audioContext.createAnalyser();
    analyser.fftSize = 256;
    if (this.masterGain) {
      this.masterGain.connect(analyser);
    }
    return analyser;
  }

  muteAll(): void {
    if (this.masterGain) {
      this.masterGain.gain.setValueAtTime(0, this.masterGain.context.currentTime);
    }
  }

  unmuteAll(): void {
    if (this.masterGain) {
      this.masterGain.gain.setValueAtTime(1, this.masterGain.context.currentTime);
    }
  }

  muteChannel(channel: AudioChannel): void {
    const gain = this.channelGains.get(channel);
    if (gain) {
      gain.gain.setValueAtTime(0, gain.context.currentTime);
    }
  }

  unmuteChannel(channel: AudioChannel): void {
    const gain = this.channelGains.get(channel);
    if (gain) {
      gain.gain.setValueAtTime(1, gain.context.currentTime);
    }
  }
}

export class SoundPlayer {
  private audioContext: AudioContext | null;
  private pool: AudioPool;
  private fadeManager: AudioFadeManager;
  private settingsManager: AudioSettingsManager;
  private activeSounds: Map<string, AudioBufferSourceNode>;

  constructor(pool: AudioPool, fadeManager: AudioFadeManager, settingsManager: AudioSettingsManager) {
    this.audioContext = null;
    this.pool = pool;
    this.fadeManager = fadeManager;
    this.settingsManager = settingsManager;
    this.activeSounds = new Map();
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;
  }

  play(
    sound: SoundEffect,
    volume: number = 1,
    pitch: number = 1,
    loop: boolean = false,
    priority: AudioPriority = AudioPriority.NORMAL
  ): AudioBufferSourceNode | null {
    if (!sound.buffer) {
      console.warn(`Sound ${sound.id} has no buffer`);
      return null;
    }

    const adjustedVolume = volume * sound.volume * this.settingsManager.getVolume().sfx;
    const adjustedPitch = pitch * sound.pitch;

    const source = this.pool.play(sound.id, adjustedVolume, adjustedPitch, loop, priority);

    if (source && sound.fadeIn && sound.fadeIn > 0) {
      const fadeId = `fade_in_${sound.id}_${Date.now()}`;
      const gainNode = this.getGainNodeForSound(sound.id);
      if (gainNode) {
        this.fadeManager.startFade(gainNode, fadeId, sound.fadeIn, adjustedVolume, AudioFadeType.LINEAR);
      }
    }

    if (source) {
      this.activeSounds.set(sound.id, source);

      source.onended = () => {
        this.activeSounds.delete(sound.id);
      };
    }

    return source;
  }

  stop(soundId: string): void {
    this.pool.stop(soundId);
    this.activeSounds.delete(soundId);
  }

  stopAll(): void {
    this.pool.stopAll();
    this.activeSounds.clear();
  }

  fadeOut(soundId: string, duration: number, fadeType: AudioFadeType = AudioFadeType.LINEAR): void {
    const gainNode = this.getGainNodeForSound(soundId);
    if (gainNode) {
      const fadeId = `fade_out_${soundId}_${Date.now()}`;
      this.fadeManager.startFade(gainNode, fadeId, duration, 0, fadeType, () => {
        this.stop(soundId);
      });
    }
  }

  setVolume(soundId: string, volume: number): void {
    const gainNode = this.getGainNodeForSound(soundId);
    if (gainNode && this.audioContext) {
      gainNode.gain.setValueAtTime(Math.max(0, Math.min(1, volume)), this.audioContext.currentTime);
    }
  }

  setPitch(soundId: string, pitch: number): void {
    const source = this.activeSounds.get(soundId);
    if (source && this.audioContext) {
      source.playbackRate.setValueAtTime(pitch, this.audioContext.currentTime);
    }
  }

  private getGainNodeForSound(soundId: string): GainNode | null {
    const poolItems = (this.pool as any).pool.get(soundId);
    if (poolItems && poolItems.length > 0) {
      return poolItems[0].gainNode;
    }
    return null;
  }

  isPlaying(soundId: string): boolean {
    return this.activeSounds.has(soundId);
  }

  getActiveSoundCount(): number {
    return this.activeSounds.size;
  }
}

export class MusicPlayer {
  private audioContext: AudioContext | null;
  private currentTrack: MusicTrack | null;
  private currentSource: AudioBufferSourceNode | null;
  private gainNode: GainNode | null;
  private fadeManager: AudioFadeManager;
  private settingsManager: AudioSettingsManager;
  private isPlaying: boolean;
  private playlist: MusicTrack[];
  private playlistIndex: number;
  private crossfadeDuration: number;

  constructor(fadeManager: AudioFadeManager, settingsManager: AudioSettingsManager) {
    this.audioContext = null;
    this.currentTrack = null;
    this.currentSource = null;
    this.gainNode = null;
    this.fadeManager = fadeManager;
    this.settingsManager = settingsManager;
    this.isPlaying = false;
    this.playlist = [];
    this.playlistIndex = 0;
    this.crossfadeDuration = 2.0;
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;
    this.gainNode = audioContext.createGain();
    this.gainNode.connect(audioContext.destination);
  }

  loadTrack(track: MusicTrack): void {
    this.currentTrack = track;
  }

  play(track?: MusicTrack, fadeIn: boolean = true, volume?: number): void {
    if (!this.audioContext || !this.gainNode) {
      console.error('MusicPlayer not initialized');
      return;
    }

    if (track) {
      this.currentTrack = track;
    }

    if (!this.currentTrack || !this.currentTrack.buffer) {
      console.warn('No track loaded or buffer not available');
      return;
    }

    if (this.isPlaying) {
      this.stop(false);
    }

    const source = this.audioContext.createBufferSource();
    source.buffer = this.currentTrack.buffer;
    source.loop = this.currentTrack.loop;
    source.connect(this.gainNode);

    const targetVolume = volume !== undefined ? volume : this.currentTrack.volume * this.settingsManager.getVolume().music;
    this.gainNode.gain.value = fadeIn && this.currentTrack.fadeIn > 0 ? 0 : targetVolume;

    if (fadeIn && this.currentTrack.fadeIn > 0) {
      const fadeId = `music_fade_in_${Date.now()}`;
      this.fadeManager.startFade(this.gainNode, fadeId, this.currentTrack.fadeIn, targetVolume, AudioFadeType.EXPONENTIAL);
    }

    source.start(0);
    this.currentSource = source;
    this.isPlaying = true;

    source.onended = () => {
      if (this.isPlaying && this.playlist.length > 0) {
        this.playNextInPlaylist();
      } else {
        this.isPlaying = false;
        this.currentSource = null;
      }
    };
  }

  stop(fadeOut: boolean = true): void {
    if (!this.currentSource || !this.gainNode) return;

    if (fadeOut && this.currentTrack && this.currentTrack.fadeOut > 0) {
      const fadeId = `music_fade_out_${Date.now()}`;
      this.fadeManager.startFade(this.gainNode, fadeId, this.currentTrack.fadeOut, 0, AudioFadeType.EXPONENTIAL, () => {
        this.forceStop();
      });
    } else {
      this.forceStop();
    }
  }

  private forceStop(): void {
    if (this.currentSource) {
      try {
        this.currentSource.stop();
      } catch (e) {
      }
      this.currentSource = null;
    }
    this.isPlaying = false;
  }

  pause(): void {
    if (this.audioContext && this.isPlaying) {
      this.audioContext.suspend();
    }
  }

  resume(): void {
    if (this.audioContext) {
      this.audioContext.resume();
    }
  }

  setVolume(volume: number, fade: boolean = false): void {
    if (!this.gainNode) return;

    const targetVolume = Math.max(0, Math.min(1, volume));

    if (fade) {
      const fadeId = `music_volume_${Date.now()}`;
      this.fadeManager.startFade(this.gainNode, fadeId, 1.0, targetVolume, AudioFadeType.LINEAR);
    } else if (this.audioContext) {
      this.gainNode.gain.setValueAtTime(targetVolume, this.audioContext.currentTime);
    }
  }

  crossfadeTo(track: MusicTrack, duration?: number): void {
    const crossfadeDuration = duration || this.crossfadeDuration;

    if (!this.gainNode) return;

    const newSource = this.audioContext!.createBufferSource();
    newSource.buffer = track.buffer;
    newSource.loop = track.loop;

    const newGainNode = this.audioContext!.createGain();
    newGainNode.gain.value = 0;
    newSource.connect(newGainNode);
    newGainNode.connect(this.audioContext!.destination);

    newSource.start(0);

    const fadeOutId = `crossfade_out_${Date.now()}`;
    const fadeInId = `crossfade_in_${Date.now()}`;

    this.fadeManager.startFade(this.gainNode, fadeOutId, crossfadeDuration, 0, AudioFadeType.LINEAR, () => {
      if (this.currentSource) {
        try {
          this.currentSource.stop();
        } catch (e) {
        }
        this.currentSource = null;
      }
      if (this.gainNode) {
        this.gainNode.gain.value = 0;
      }
    });

    this.fadeManager.startFade(newGainNode, fadeInId, crossfadeDuration, track.volume, AudioFadeType.LINEAR);

    this.currentTrack = track;
    this.currentSource = newSource;
    this.gainNode = newGainNode;
    this.isPlaying = true;

    newSource.onended = () => {
      if (this.isPlaying) {
        this.isPlaying = false;
      }
    };
  }

  setPlaylist(tracks: MusicTrack[], shuffle: boolean = false): void {
    this.playlist = [...tracks];
    if (shuffle) {
      this.shufflePlaylist();
    }
    this.playlistIndex = 0;
  }

  private shufflePlaylist(): void {
    for (let i = this.playlist.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [this.playlist[i], this.playlist[j]] = [this.playlist[j], this.playlist[i]];
    }
  }

  private playNextInPlaylist(): void {
    if (this.playlist.length === 0) return;

    this.playlistIndex = (this.playlistIndex + 1) % this.playlist.length;
    const nextTrack = this.playlist[this.playlistIndex];

    if (this.crossfadeDuration > 0) {
      this.crossfadeTo(nextTrack, this.crossfadeDuration);
    } else {
      this.play(nextTrack, true);
    }
  }

  playPreviousInPlaylist(): void {
    if (this.playlist.length === 0) return;

    this.playlistIndex = (this.playlistIndex - 1 + this.playlist.length) % this.playlist.length;
    const prevTrack = this.playlist[this.playlistIndex];

    if (this.crossfadeDuration > 0) {
      this.crossfadeTo(prevTrack, this.crossfadeDuration);
    } else {
      this.play(prevTrack, true);
    }
  }

  setCrossfadeDuration(duration: number): void {
    this.crossfadeDuration = Math.max(0, duration);
  }

  getCurrentTrack(): MusicTrack | null {
    return this.currentTrack;
  }

  getIsPlaying(): boolean {
    return this.isPlaying;
  }

  getPlaylist(): MusicTrack[] {
    return this.playlist;
  }

  getPlaylistIndex(): number {
    return this.playlistIndex;
  }
}

export class AmbientManager {
  private audioContext: AudioContext | null;
  private currentAmbient: AmbientSound | null;
  private currentSource: AudioBufferSourceNode | null;
  private gainNode: GainNode | null;
  private fadeManager: AudioFadeManager;
  private settingsManager: AudioSettingsManager;
  private ambientLayers: Map<string, AudioBufferSourceNode>;
  private isPlaying: boolean;

  constructor(fadeManager: AudioFadeManager, settingsManager: AudioSettingsManager) {
    this.audioContext = null;
    this.currentAmbient = null;
    this.currentSource = null;
    this.gainNode = null;
    this.fadeManager = fadeManager;
    this.settingsManager = settingsManager;
    this.ambientLayers = new Map();
    this.isPlaying = false;
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;
    this.gainNode = audioContext.createGain();
    this.gainNode.connect(audioContext.destination);
  }

  play(ambient: AmbientSound, crossfade: boolean = true): void {
    if (!this.audioContext || !this.gainNode || !ambient.buffer) {
      console.error('AmbientManager not initialized or no buffer available');
      return;
    }

    const source = this.audioContext.createBufferSource();
    source.buffer = ambient.buffer;
    source.loop = ambient.loop;
    source.connect(this.gainNode);

    const targetVolume = ambient.volume * this.settingsManager.getVolume().ambient;

    if (crossfade && this.isPlaying && this.currentAmbient) {
      const newGainNode = this.audioContext.createGain();
      newGainNode.gain.value = 0;
      source.connect(newGainNode);
      newGainNode.connect(this.audioContext.destination);

      const fadeOutId = `ambient_fade_out_${Date.now()}`;
      const fadeInId = `ambient_fade_in_${Date.now()}`;

      this.fadeManager.startFade(this.gainNode, fadeOutId, ambient.crossfadeDuration, 0, AudioFadeType.EXPONENTIAL, () => {
        if (this.currentSource) {
          try {
            this.currentSource.stop();
          } catch (e) {
          }
        }
      });

      this.fadeManager.startFade(newGainNode, fadeInId, ambient.crossfadeDuration, targetVolume, AudioFadeType.EXPONENTIAL);

      this.gainNode = newGainNode;
    } else {
      this.gainNode.gain.value = targetVolume;
    }

    source.start(0);
    this.currentSource = source;
    this.currentAmbient = ambient;
    this.isPlaying = true;
  }

  stop(fadeOut: boolean = true): void {
    if (!this.currentSource) return;

    if (fadeOut && this.currentAmbient) {
      const fadeId = `ambient_stop_${Date.now()}`;
      this.fadeManager.startFade(this.gainNode!, fadeId, this.currentAmbient.crossfadeDuration, 0, AudioFadeType.EXPONENTIAL, () => {
        this.forceStop();
      });
    } else {
      this.forceStop();
    }
  }

  private forceStop(): void {
    if (this.currentSource) {
      try {
        this.currentSource.stop();
      } catch (e) {
      }
      this.currentSource = null;
    }
    this.isPlaying = false;
  }

  addLayer(layerId: string, buffer: AudioBuffer, volume: number = 1): void {
    if (!this.audioContext || !this.gainNode) return;

    const source = this.audioContext.createBufferSource();
    source.buffer = buffer;
    source.loop = true;

    const layerGain = this.audioContext.createGain();
    layerGain.gain.value = volume;

    source.connect(layerGain);
    layerGain.connect(this.gainNode);
    source.start(0);

    this.ambientLayers.set(layerId, source);
  }

  removeLayer(layerId: string, fadeOut: boolean = true): void {
    const source = this.ambientLayers.get(layerId);
    if (source) {
      if (fadeOut) {
        try {
          source.stop();
        } catch (e) {
        }
      }
      this.ambientLayers.delete(layerId);
    }
  }

  setLayerVolume(layerId: string, volume: number): void {
  }

  setMasterVolume(volume: number, fade: boolean = false): void {
    if (!this.gainNode) return;

    const targetVolume = Math.max(0, Math.min(1, volume));

    if (fade) {
      const fadeId = `ambient_master_${Date.now()}`;
      this.fadeManager.startFade(this.gainNode, fadeId, 1.0, targetVolume, AudioFadeType.LINEAR);
    } else if (this.audioContext) {
      this.gainNode.gain.setValueAtTime(targetVolume, this.audioContext.currentTime);
    }
  }

  getCurrentAmbient(): AmbientSound | null {
    return this.currentAmbient;
  }

  getIsPlaying(): boolean {
    return this.isPlaying;
  }

  getActiveLayers(): string[] {
    return Array.from(this.ambientLayers.keys());
  }

  stopAll(): void {
    this.forceStop();
    for (const source of this.ambientLayers.values()) {
      try {
        source.stop();
      } catch (e) {
      }
    }
    this.ambientLayers.clear();
  }
}

export class VoiceLineManager {
  private audioContext: AudioContext | null;
  private gainNode: GainNode | null;
  private fadeManager: AudioFadeManager;
  private settingsManager: AudioSettingsManager;
  private voiceLines: Map<string, VoiceLine>;
  private cooldowns: Map<string, number>;
  private activeVoices: Map<string, AudioBufferSourceNode>;
  private speechQueue: string[];
  private isSpeaking: boolean;

  constructor(fadeManager: AudioFadeManager, settingsManager: AudioSettingsManager) {
    this.audioContext = null;
    this.gainNode = null;
    this.fadeManager = fadeManager;
    this.settingsManager = settingsManager;
    this.voiceLines = new Map();
    this.cooldowns = new Map();
    this.activeVoices = new Map();
    this.speechQueue = [];
    this.isSpeaking = false;
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;
    this.gainNode = audioContext.createGain();
    this.gainNode.connect(audioContext.destination);
  }

  registerVoiceLine(voiceLine: VoiceLine): void {
    this.voiceLines.set(voiceLine.id, voiceLine);
  }

  registerVoiceLines(voiceLines: VoiceLine[]): void {
    for (const voiceLine of voiceLines) {
      this.registerVoiceLine(voiceLine);
    }
  }

  play(
    unitType: string,
    category: VoiceLineCategory,
    volume: number = 1,
    pitch: number = 1
  ): AudioBufferSourceNode | null {
    if (!this.audioContext || !this.gainNode) {
      console.error('VoiceLineManager not initialized');
      return null;
    }

    const voiceLine = this.findVoiceLine(unitType, category);
    if (!voiceLine || !voiceLine.buffer) {
      console.warn(`Voice line not found: ${unitType} - ${category}`);
      return null;
    }

    if (this.isOnCooldown(voiceLine)) {
      console.warn(`Voice line ${voiceLine.id} is on cooldown`);
      return null;
    }

    const source = this.audioContext.createBufferSource();
    source.buffer = voiceLine.buffer;
    source.playbackRate.value = pitch;

    const voiceGain = this.audioContext.createGain();
    voiceGain.gain.value = volume * voiceLine.volume * this.settingsManager.getVolume().voice;

    source.connect(voiceGain);
    voiceGain.connect(this.gainNode);

    source.start(0);
    this.activeVoices.set(voiceLine.id, source);
    this.isSpeaking = true;

    if (voiceLine.cooldown) {
      this.cooldowns.set(voiceLine.id, Date.now() + voiceLine.cooldown);
    }

    voiceLine.lastPlayed = Date.now();

    source.onended = () => {
      this.activeVoices.delete(voiceLine.id);
      this.isSpeaking = this.activeVoices.size > 0;
      this.processQueue();
    };

    return source;
  }

  private findVoiceLine(unitType: string, category: VoiceLineCategory): VoiceLine | null {
    for (const voiceLine of this.voiceLines.values()) {
      if (voiceLine.unitType === unitType && voiceLine.category === category) {
        return voiceLine;
      }
    }
    return null;
  }

  private isOnCooldown(voiceLine: VoiceLine): boolean {
    const cooldownEnd = this.cooldowns.get(voiceLine.id);
    if (cooldownEnd) {
      return Date.now() < cooldownEnd;
    }
    return false;
  }

  queueVoiceLine(unitType: string, category: VoiceLineCategory): void {
    const key = `${unitType}:${category}`;
    if (!this.speechQueue.includes(key)) {
      this.speechQueue.push(key);
    }
    if (!this.isSpeaking) {
      this.processQueue();
    }
  }

  private processQueue(): void {
    if (this.speechQueue.length === 0 || this.isSpeaking) return;

    const key = this.speechQueue.shift()!;
    const [unitType, category] = key.split(':') as [string, VoiceLineCategory];
    this.play(unitType, category);
  }

  clearQueue(): void {
    this.speechQueue = [];
  }

  stop(voiceLineId?: string): void {
    if (voiceLineId) {
      const source = this.activeVoices.get(voiceLineId);
      if (source) {
        try {
          source.stop();
        } catch (e) {
        }
        this.activeVoices.delete(voiceLineId);
      }
    } else {
      for (const source of this.activeVoices.values()) {
        try {
          source.stop();
        } catch (e) {
        }
      }
      this.activeVoices.clear();
      this.isSpeaking = false;
    }
  }

  setVolume(volume: number): void {
    if (this.gainNode && this.audioContext) {
      this.gainNode.gain.setValueAtTime(
        Math.max(0, Math.min(1, volume)),
        this.audioContext.currentTime
      );
    }
  }

  getVoiceLine(unitType: string, category: VoiceLineCategory): VoiceLine | null {
    return this.findVoiceLine(unitType, category);
  }

  getVoiceLinesForUnit(unitType: string): VoiceLine[] {
    return Array.from(this.voiceLines.values()).filter((vl) => vl.unitType === unitType);
  }

  getVoiceLinesByCategory(category: VoiceLineCategory): VoiceLine[] {
    return Array.from(this.voiceLines.values()).filter((vl) => vl.category === category);
  }

  getActiveVoiceCount(): number {
    return this.activeVoices.size;
  }

  isCurrentlySpeaking(): boolean {
    return this.isSpeaking;
  }

  clearCooldowns(): void {
    this.cooldowns.clear();
  }

  removeCooldown(voiceLineId: string): void {
    this.cooldowns.delete(voiceLineId);
  }
}

export class DynamicMusicSystem {
  private audioContext: AudioContext | null;
  private musicPlayer: MusicPlayer;
  private layerGains: Map<MusicLayer, GainNode>;
  private tracks: Map<GameState, MusicTrack>;
  private layers: Map<MusicLayer, AudioBuffer>;
  private currentGameState: GameState;
  private combatIntensity: number;
  private tensionLevel: number;
  private isActive: boolean;

  constructor(musicPlayer: MusicPlayer) {
    this.audioContext = null;
    this.musicPlayer = musicPlayer;
    this.layerGains = new Map();
    this.tracks = new Map();
    this.layers = new Map();
    this.currentGameState = GameState.MENU;
    this.combatIntensity = 0;
    this.tensionLevel = 0;
    this.isActive = false;
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;

    const layers = [
      MusicLayer.BASE,
      MusicLayer.TENSION,
      MusicLayer.ACTION,
      MusicLayer.CLIMAX,
      MusicLayer.RESOLVE,
    ];

    for (const layer of layers) {
      const gain = audioContext.createGain();
      gain.gain.value = 0;
      this.layerGains.set(layer, gain);
    }
  }

  registerTrack(gameState: GameState, track: MusicTrack): void {
    this.tracks.set(gameState, track);
  }

  registerLayer(layer: MusicLayer, buffer: AudioBuffer): void {
    this.layers.set(layer, buffer);
  }

  setGameState(state: GameState): void {
    if (state !== this.currentGameState) {
      this.currentGameState = state;
      this.updateMusic();
    }
  }

  setCombatIntensity(intensity: number): void {
    this.combatIntensity = Math.max(0, Math.min(1, intensity));
    this.updateLayerMix();
  }

  setTensionLevel(level: number): void {
    this.tensionLevel = Math.max(0, Math.min(1, level));
    this.updateLayerMix();
  }

  private updateMusic(): void {
    const track = this.tracks.get(this.currentGameState);
    if (track) {
      this.musicPlayer.play(track, true);
      this.isActive = true;
    }
  }

  private updateLayerMix(): void {
    const baseGain = this.layerGains.get(MusicLayer.BASE);
    const tensionGain = this.layerGains.get(MusicLayer.TENSION);
    const actionGain = this.layerGains.get(MusicLayer.ACTION);
    const climaxGain = this.layerGains.get(MusicLayer.CLIMAX);
    const resolveGain = this.layerGains.get(MusicLayer.RESOLVE);

    if (baseGain && this.audioContext) {
      baseGain.gain.setValueAtTime(1.0, this.audioContext.currentTime);
    }

    if (tensionGain && this.audioContext) {
      const tensionValue = this.tensionLevel * 0.5;
      tensionGain.gain.setValueAtTime(tensionValue, this.audioContext.currentTime);
    }

    if (actionGain && this.audioContext) {
      const actionValue = this.combatIntensity * 0.7;
      actionGain.gain.setValueAtTime(actionValue, this.audioContext.currentTime);
    }

    if (climaxGain && this.audioContext) {
      const climaxValue = this.combatIntensity > 0.8 ? (this.combatIntensity - 0.8) * 5 : 0;
      climaxGain.gain.setValueAtTime(climaxValue, this.audioContext.currentTime);
    }

    if (resolveGain && this.audioContext) {
      const resolveValue = this.tensionLevel < 0.3 ? (0.3 - this.tensionLevel) / 0.3 : 0;
      resolveGain.gain.setValueAtTime(resolveValue, this.audioContext.currentTime);
    }
  }

  startDynamicMusic(): void {
    this.isActive = true;
    this.updateMusic();
    this.updateLayerMix();
  }

  stopDynamicMusic(): void {
    this.isActive = false;
    this.musicPlayer.stop(true);

    for (const gain of this.layerGains.values()) {
      if (this.audioContext) {
        gain.gain.setValueAtTime(0, this.audioContext.currentTime);
      }
    }
  }

  getCurrentGameState(): GameState {
    return this.currentGameState;
  }

  getCombatIntensity(): number {
    return this.combatIntensity;
  }

  getTensionLevel(): number {
    return this.tensionLevel;
  }

  isDynamicMusicActive(): boolean {
    return this.isActive;
  }

  getAvailableStates(): GameState[] {
    return Array.from(this.tracks.keys());
  }

  getAvailableLayers(): MusicLayer[] {
    return Array.from(this.layers.keys());
  }
}

export class AudioCuePlayer {
  private audioContext: AudioContext | null;
  private soundPlayer: SoundPlayer;
  private cues: Map<string, AudioCue>;
  private activeCues: Map<string, {
    sources: AudioBufferSourceNode[];
    startTime: number;
    currentIndex: number;
  }>;

  constructor(soundPlayer: SoundPlayer) {
    this.audioContext = null;
    this.soundPlayer = soundPlayer;
    this.cues = new Map();
    this.activeCues = new Map();
  }

  initialize(audioContext: AudioContext): void {
    this.audioContext = audioContext;
  }

  registerCue(cue: AudioCue): void {
    this.cues.set(cue.id, cue);
  }

  registerCues(cues: AudioCue[]): void {
    for (const cue of cues) {
      this.registerCue(cue);
    }
  }

  play(cueId: string, volume?: number, pitch?: number): string | null {
    const cue = this.cues.get(cueId);
    if (!cue) {
      console.warn(`AudioCue ${cueId} not found`);
      return null;
    }

    const playId = `${cueId}_${Date.now()}`;
    const sources: AudioBufferSourceNode[] = [];

    switch (cue.playbackMode) {
      case 'sequential':
        this.playSequential(cue, sources, volume, pitch, 0);
        break;
      case 'random':
        this.playRandom(cue, sources, volume, pitch);
        break;
      case 'simultaneous':
        this.playSimultaneous(cue, sources, volume, pitch);
        break;
      case 'weighted_random':
        this.playWeightedRandom(cue, sources, volume, pitch);
        break;
    }

    this.activeCues.set(playId, {
      sources,
      startTime: Date.now(),
      currentIndex: 0,
    });

    return playId;
  }

  private playSequential(
    cue: AudioCue,
    sources: AudioBufferSourceNode[],
    volume?: number,
    pitch?: number,
    index: number
  ): void {
    if (index >= cue.sounds.length) return;

    const soundData = cue.sounds[index];
    const sound = typeof soundData === 'string' ? { id: soundData } as SoundEffect : soundData;

    const source = this.soundPlayer.play(
      sound,
      volume !== undefined ? volume : cue.volume,
      pitch !== undefined ? pitch : cue.pitch || 1,
      false,
      cue.priority
    );

    if (source) {
      sources.push(source);

      const duration = (sound as SoundEffect).duration || 1;
      const delay = duration * 1000 + cue.transitionTime * 1000;

      setTimeout(() => {
        this.playSequential(cue, sources, volume, pitch, index + 1);
      }, delay);
    }
  }

  private playRandom(
    cue: AudioCue,
    sources: AudioBufferSourceNode[],
    volume?: number,
    pitch?: number
  ): void {
    const randomIndex = Math.floor(Math.random() * cue.sounds.length);
    const soundData = cue.sounds[randomIndex];
    const sound = typeof soundData === 'string' ? { id: soundData } as SoundEffect : soundData;

    const source = this.soundPlayer.play(
      sound,
      volume !== undefined ? volume : cue.volume,
      pitch !== undefined ? pitch : cue.pitch || 1,
      cue.loop,
      cue.priority
    );

    if (source) {
      sources.push(source);
    }
  }

  private playSimultaneous(
    cue: AudioCue,
    sources: AudioBufferSourceNode[],
    volume?: number,
    pitch?: number
  ): void {
    for (const soundData of cue.sounds) {
      const sound = typeof soundData === 'string' ? { id: soundData } as SoundEffect : soundData;

      const source = this.soundPlayer.play(
        sound,
        volume !== undefined ? volume : cue.volume,
        pitch !== undefined ? pitch : cue.pitch || 1,
        cue.loop,
        cue.priority
      );

      if (source) {
        sources.push(source);
      }
    }
  }

  private playWeightedRandom(
    cue: AudioCue,
    sources: AudioBufferSourceNode[],
    volume?: number,
    pitch?: number
  ): void {
    const randomIndex = Math.floor(Math.random() * cue.sounds.length);
    const soundData = cue.sounds[randomIndex];
    const sound = typeof soundData === 'string' ? { id: soundData } as SoundEffect : soundData;

    const source = this.soundPlayer.play(
      sound,
      volume !== undefined ? volume : cue.volume,
      pitch !== undefined ? pitch : cue.pitch || 1,
      cue.loop,
      cue.priority
    );

    if (source) {
      sources.push(source);
    }
  }

  stop(playId: string): void {
    const activeCue = this.activeCues.get(playId);
    if (activeCue) {
      for (const source of activeCue.sources) {
        try {
          source.stop();
        } catch (e) {
        }
      }
      this.activeCues.delete(playId);
    }
  }

  stopAll(): void {
    for (const playId of this.activeCues.keys()) {
      this.stop(playId);
    }
  }

  getCue(cueId: string): AudioCue | undefined {
    return this.cues.get(cueId);
  }

  getActiveCues(): string[] {
    return Array.from(this.activeCues.keys());
  }

  isCuePlaying(playId: string): boolean {
    return this.activeCues.has(playId);
  }
}

export class AudioManager {
  private audioContext: AudioContext | null;
  private isInitialized: boolean;
  private isMuted: boolean;
  private settingsManager: AudioSettingsManager;
  private mixer: AudioMixer;
  private pool: AudioPool;
  private fadeManager: AudioFadeManager;
  private spatialAudio: SpatialAudioManager;
  private busManager: AudioBusManager;
  private soundPlayer: SoundPlayer;
  private musicPlayer: MusicPlayer;
  private ambientManager: AmbientManager;
  private voiceLineManager: VoiceLineManager;
  private dynamicMusic: DynamicMusicSystem;
  private cuePlayer: AudioCuePlayer;
  private eventEmitter: EventEmitter;
  private soundBanks: Map<string, SoundBank>;
  private audioState: AudioState;
  private analyzerData: Uint8Array;

  constructor() {
    this.audioContext = null;
    this.isInitialized = false;
    this.isMuted = false;
    this.settingsManager = new AudioSettingsManager();
    this.mixer = new AudioMixer();
    this.pool = new AudioPool(10, 32);
    this.fadeManager = new AudioFadeManager();
    this.spatialAudio = new SpatialAudioManager();
    this.busManager = new AudioBusManager();
    this.soundPlayer = new SoundPlayer(this.pool, this.fadeManager, this.settingsManager);
    this.musicPlayer = new MusicPlayer(this.fadeManager, this.settingsManager);
    this.ambientManager = new AmbientManager(this.fadeManager, this.settingsManager);
    this.voiceLineManager = new VoiceLineManager(this.fadeManager, this.settingsManager);
    this.dynamicMusic = new DynamicMusicSystem(this.musicPlayer);
    this.cuePlayer = new AudioCuePlayer(this.soundPlayer);
    this.eventEmitter = new EventEmitter();
    this.soundBanks = new Map();
    this.analyzerData = new Uint8Array(128);

    this.audioState = {
      isPlaying: false,
      currentMusic: null,
      currentAmbient: null,
      activeVoices: 0,
      activeEffects: 0,
      totalVoices: 32,
      masterVolume: 1.0,
      muted: false,
      gameState: GameState.MENU,
    };
  }

  async initialize(config?: AudioConfig): Promise<boolean> {
    try {
      this.audioContext = new AudioContext();

      if (config) {
        this.settingsManager.updateConfig(config);
      }

      const masterGain = this.mixer.getMasterGain();
      if (!masterGain) {
        console.error('Failed to get master gain');
        return false;
      }

      this.pool.initialize(this.audioContext, masterGain);
      this.spatialAudio.initialize(this.audioContext);
      this.busManager.initialize(this.audioContext, masterGain);
      this.mixer.initialize(this.audioContext);
      this.soundPlayer.initialize(this.audioContext);
      this.musicPlayer.initialize(this.audioContext);
      this.ambientManager.initialize(this.audioContext);
      this.voiceLineManager.initialize(this.audioContext);
      this.dynamicMusic.initialize(this.audioContext);
      this.cuePlayer.initialize(this.audioContext);

      if (this.audioContext.state === 'suspended') {
        await this.audioContext.resume();
      }

      this.isInitialized = true;
      this.eventEmitter.emit('initialized');

      return true;
    } catch (error) {
      console.error('Failed to initialize audio system:', error);
      return false;
    }
  }

  async loadSoundBank(bank: SoundBank): Promise<boolean> {
    if (!this.audioContext) {
      console.error('Audio system not initialized');
      return false;
    }

    try {
      const loadedSounds: SoundEffect[] = [];

      for (const sound of bank.sounds.values()) {
        if (sound.filePath) {
          const response = await fetch(sound.filePath);
          const arrayBuffer = await response.arrayBuffer();
          const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
          sound.buffer = audioBuffer;
          sound.duration = audioBuffer.duration;
        }
        loadedSounds.push(sound);
        this.pool.addToPool(sound.id, sound.buffer!);
      }

      bank.loaded = true;
      bank.sounds = new Map(loadedSounds.map((s) => [s.id, s]));

      this.soundBanks.set(bank.id, bank);
      this.eventEmitter.emit('bankLoaded', bank.id);

      return true;
    } catch (error) {
      console.error(`Failed to load sound bank ${bank.id}:`, error);
      return false;
    }
  }

  async loadMusicTrack(track: MusicTrack): Promise<boolean> {
    if (!this.audioContext || !track.filePath) {
      return false;
    }

    try {
      const response = await fetch(track.filePath);
      const arrayBuffer = await response.arrayBuffer();
      const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
      track.buffer = audioBuffer;
      track.duration = audioBuffer.duration;
      return true;
    } catch (error) {
      console.error(`Failed to load music track ${track.id}:`, error);
      return false;
    }
  }

  async loadAmbientSound(ambient: AmbientSound): Promise<boolean> {
    if (!this.audioContext || !ambient.filePath) {
      return false;
    }

    try {
      const response = await fetch(ambient.filePath);
      const arrayBuffer = await response.arrayBuffer();
      const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
      ambient.buffer = audioBuffer;
      ambient.duration = audioBuffer.duration;
      return true;
    } catch (error) {
      console.error(`Failed to load ambient sound ${ambient.id}:`, error);
      return false;
    }
  }

  async loadVoiceLine(voiceLine: VoiceLine): Promise<boolean> {
    if (!this.audioContext || !voiceLine.filePath) {
      return false;
    }

    try {
      const response = await fetch(voiceLine.filePath);
      const arrayBuffer = await response.arrayBuffer();
      const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
      voiceLine.buffer = audioBuffer;
      voiceLine.duration = audioBuffer.duration;
      this.voiceLineManager.registerVoiceLine(voiceLine);
      return true;
    } catch (error) {
      console.error(`Failed to load voice line ${voiceLine.id}:`, error);
      return false;
    }
  }

  playSound(
    soundId: string,
    volume: number = 1,
    pitch: number = 1,
    loop: boolean = false,
    priority: AudioPriority = AudioPriority.NORMAL
  ): AudioBufferSourceNode | null {
    const sound = this.findSoundById(soundId);
    if (!sound) {
      console.warn(`Sound ${soundId} not found`);
      return null;
    }
    return this.soundPlayer.play(sound, volume, pitch, loop, priority);
  }

  playSoundEffect(
    effect: SoundEffect,
    volume: number = 1,
    pitch: number = 1,
    loop: boolean = false
  ): AudioBufferSourceNode | null {
    return this.soundPlayer.play(effect, volume, pitch, loop, AudioPriority.NORMAL);
  }

  stopSound(soundId: string): void {
    this.soundPlayer.stop(soundId);
  }

  stopAllSounds(): void {
    this.soundPlayer.stopAll();
  }

  fadeOutSound(soundId: string, duration: number): void {
    this.soundPlayer.fadeOut(soundId, duration);
  }

  playMusic(track?: MusicTrack, fadeIn: boolean = true): void {
    this.musicPlayer.play(track, fadeIn);
    this.audioState.currentMusic = track?.id || null;
  }

  stopMusic(fadeOut: boolean = true): void {
    this.musicPlayer.stop(fadeOut);
    this.audioState.currentMusic = null;
  }

  pauseMusic(): void {
    this.musicPlayer.pause();
  }

  resumeMusic(): void {
    this.musicPlayer.resume();
  }

  setMusicVolume(volume: number, fade: boolean = false): void {
    this.musicPlayer.setVolume(volume, fade);
    this.settingsManager.setChannelVolume(AudioChannel.MUSIC, volume);
  }

  crossfadeToMusic(track: MusicTrack, duration?: number): void {
    this.musicPlayer.crossfadeTo(track, duration);
    this.audioState.currentMusic = track.id;
  }

  playAmbient(ambient: AmbientSound, crossfade: boolean = true): void {
    this.ambientManager.play(ambient, crossfade);
    this.audioState.currentAmbient = ambient.id;
  }

  stopAmbient(fadeOut: boolean = true): void {
    this.ambientManager.stop(fadeOut);
    this.audioState.currentAmbient = null;
  }

  setAmbientVolume(volume: number, fade: boolean = false): void {
    this.ambientManager.setMasterVolume(volume, fade);
    this.settingsManager.setChannelVolume(AudioChannel.AMBIENT, volume);
  }

  playVoiceLine(unitType: string, category: VoiceLineCategory, volume?: number, pitch?: number): AudioBufferSourceNode | null {
    return this.voiceLineManager.play(unitType, category, volume, pitch);
  }

  queueVoiceLine(unitType: string, category: VoiceLineCategory): void {
    this.voiceLineManager.queueVoiceLine(unitType, category);
  }

  playCue(cueId: string, volume?: number, pitch?: number): string | null {
    return this.cuePlayer.play(cueId, volume, pitch);
  }

  stopCue(playId: string): void {
    this.cuePlayer.stop(playId);
  }

  setMasterVolume(volume: number): void {
    this.mixer.setMasterVolume(volume);
    this.settingsManager.setMasterVolume(volume);
    this.audioState.masterVolume = volume;
  }

  setChannelVolume(channel: AudioChannel, volume: number): void {
    this.mixer.setChannelVolume(channel, volume);
    this.settingsManager.setChannelVolume(channel, volume);
  }

  mute(): void {
    this.mixer.muteAll();
    this.isMuted = true;
    this.audioState.muted = true;
    this.eventEmitter.emit('muteChanged', true);
  }

  unmute(): void {
    this.mixer.unmuteAll();
    this.isMuted = false;
    this.audioState.muted = false;
    this.eventEmitter.emit('muteChanged', false);
  }

  toggleMute(): void {
    if (this.isMuted) {
      this.unmute();
    } else {
      this.mute();
    }
  }

  isMutedState(): boolean {
    return this.isMuted;
  }

  setGameState(state: GameState): void {
    this.audioState.gameState = state;
    this.dynamicMusic.setGameState(state);
    this.eventEmitter.emit('gameStateChanged', state);
  }

  setCombatIntensity(intensity: number): void {
    this.dynamicMusic.setCombatIntensity(intensity);
  }

  setTensionLevel(level: number): void {
    this.dynamicMusic.setTensionLevel(level);
  }

  startDynamicMusic(): void {
    this.dynamicMusic.startDynamicMusic();
  }

  stopDynamicMusic(): void {
    this.dynamicMusic.stopDynamicMusic();
  }

  updateListenerPosition(x: number, y: number, z: number): void {
    this.spatialAudio.setListenerPosition(x, y, z);
  }

  updateListenerOrientation(forwardX: number, forwardY: number, forwardZ: number): void {
    this.spatialAudio.setListenerOrientation(forwardX, forwardY, forwardZ);
  }

  createSpatialSound(
    soundId: string,
    position: { x: number; y: number; z: number },
    config?: Partial<SpatialAudioConfig>
  ): PannerNode | null {
    return this.spatialAudio.createSpatialSource(position, config);
  }

  setDistanceModel(model: DistanceModel): void {
    this.spatialAudio.setDistanceModel(model);
  }

  setDopplerFactor(factor: number): void {
    this.spatialAudio.setDopplerFactor(factor);
  }

  private findSoundById(soundId: string): SoundEffect | null {
    for (const bank of this.soundBanks.values()) {
      if (bank.sounds.has(soundId)) {
        return bank.sounds.get(soundId)!;
      }
    }
    return null;
  }

  getState(): AudioState {
    this.audioState.activeVoices = this.pool.getActiveVoices();
    this.audioState.activeEffects = this.soundPlayer.getActiveSoundCount();
    return { ...this.audioState };
  }

  getSettings(): AudioSettings {
    return this.settingsManager.getSettings();
  }

  saveSettings(): string {
    return this.settingsManager.serializeSettings();
  }

  loadSettings(settingsJson: string): void {
    this.settingsManager.deserializeSettings(settingsJson);
    const settings = this.settingsManager.getSettings();

    this.setMasterVolume(settings.volume.master);
    for (const channel of Object.values(AudioChannel)) {
      this.setChannelVolume(channel, settings.volume[channel]);
    }

    if (settings.config.muted) {
      this.mute();
    } else {
      this.unmute();
    }
  }

  resetSettings(): void {
    this.settingsManager.resetToDefaults();
    const settings = this.settingsManager.getSettings();

    this.setMasterVolume(settings.volume.master);
    for (const channel of Object.values(AudioChannel)) {
      this.setChannelVolume(channel, settings.volume[channel]);
    }

    this.unmute();
  }

  on(event: string, listener: (...args: any[]) => void): void {
    this.eventEmitter.on(event, listener);
  }

  off(event: string, listener: (...args: any[]) => void): void {
    this.eventEmitter.off(event, listener);
  }

  once(event: string, listener: (...args: any[]) => void): void {
    this.eventEmitter.once(event, listener);
  }

  getAnalyzerData(): Uint8Array {
    const analyser = this.mixer.getAnalyser();
    if (analyser) {
      analyser.getByteFrequencyData(this.analyzerData);
    }
    return this.analyzerData;
  }

  getSoundBank(bankId: string): SoundBank | undefined {
    return this.soundBanks.get(bankId);
  }

  getAllSoundBanks(): SoundBank[] {
    return Array.from(this.soundBanks.values());
  }

  unloadSoundBank(bankId: string): void {
    const bank = this.soundBanks.get(bankId);
    if (bank) {
      for (const sound of bank.sounds.values()) {
        this.pool.removeFromPool(sound.id);
      }
      this.soundBanks.delete(bankId);
    }
  }

  dispose(): void {
    this.stopAllSounds();
    this.stopMusic();
    this.stopAmbient();
    this.voiceLineManager.stop();
    this.pool.clearPool();
    this.fadeManager.cancelAllFades();

    if (this.audioContext) {
      this.audioContext.close();
      this.audioContext = null;
    }

    this.isInitialized = false;
    this.eventEmitter.removeAllListeners();
  }

  isReady(): boolean {
    return this.isInitialized;
  }

  getVersion(): string {
    return '1.0.0';
  }
}

export default AudioManager;

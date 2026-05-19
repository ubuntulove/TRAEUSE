import { EventEmitter } from '../../core/EventEmitter';
import { Vector3 } from '../../math/Vector3';
import { Quaternion } from '../../math/Quaternion';
import { Color } from '../../graphics/Color';
import { EasingType } from '../../math/Easing';

export interface CinematicEvent {
  id: string;
  type: CinematicEventType;
  timestamp: number;
  duration: number;
  data: CinematicEventData;
  trigger?: CinematicTrigger;
  conditions?: CinematicCondition[];
  easing?: EasingType;
}

export enum CinematicEventType {
  CAMERA = 'camera',
  DIALOGUE = 'dialogue',
  EFFECT = 'effect',
  TRANSITION = 'transition',
  OBJECTIVE = 'objective',
  UNIT = 'unit',
  ENVIRONMENT = 'environment',
  AUDIO = 'audio',
  TEXT = 'text',
  ACTION = 'action',
}

export interface CinematicEventData {
  cameraShot?: CameraShot;
  dialogue?: DialogueData;
  effect?: EffectData;
  transition?: TransitionData;
  objective?: ObjectiveData;
  unitCommand?: UnitCommandData;
  environment?: EnvironmentData;
  audio?: AudioData;
  text?: CinematicText;
  action?: string;
  [key: string]: unknown;
}

export interface CinematicTrigger {
  type: CinematicTriggerType;
  condition?: string;
  delay?: number;
  callback?: () => void;
}

export enum CinematicTriggerType {
  TIME = 'time',
  EVENT = 'event',
  CONDITION = 'condition',
  USER_INPUT = 'user_input',
  COMPLETE = 'complete',
}

export interface CinematicCondition {
  type: string;
  value: unknown;
  operator?: 'equals' | 'not_equals' | 'greater' | 'less' | 'contains';
}

export interface CameraSequence {
  id: string;
  shots: CameraShot[];
  totalDuration: number;
  loop?: boolean;
  blendMode?: CameraBlendMode;
}

export enum CameraBlendMode {
  CUT = 'cut',
  LINEAR = 'linear',
  SMOOTH = 'smooth',
  CUBIC = 'cubic',
}

export interface CameraShot {
  id: string;
  type: CameraShotType;
  startPosition: Vector3;
  endPosition: Vector3;
  startRotation: Quaternion;
  endRotation: Quaternion;
  startFOV: number;
  endFOV: number;
  duration: number;
  target?: string;
  targetOffset?: Vector3;
  motionBlur?: boolean;
  depthOfField?: DepthOfFieldSettings;
  vignette?: VignetteSettings;
}

export enum CameraShotType {
  FIXED = 'fixed',
  PAN = 'pan',
  DOLLY = 'dolly',
  TRACK = 'track',
  CRANE = 'crane',
  HANDHELD = 'handheld',
  ORBIT = 'orbit',
  ZOOM = 'zoom',
  RACK_FOCUS = 'rack_focus',
}

export interface DepthOfFieldSettings {
  enabled: boolean;
  focusDistance: number;
  focalLength: number;
  aperture: number;
  bokehShape?: BokehShape;
}

export enum BokehShape {
  CIRCLE = 'circle',
  HEXAGON = 'hexagon',
  OCTAGON = 'octagon',
  HEART = 'heart',
}

export interface VignetteSettings {
  intensity: number;
  softness: number;
  color?: Color;
}

export interface DialogueData {
  speakerId?: string;
  speakerName?: string;
  text: string;
  voiceoverId?: string;
  subtitleDuration?: number;
  emotion?: DialogueEmotion;
  position?: SubtitlePosition;
}

export enum DialogueEmotion {
  NEUTRAL = 'neutral',
  SERIOUS = 'serious',
  TENSE = 'tense',
  DRAMATIC = 'dramatic',
  UPLIFTING = 'uplifting',
  SAD = 'sad',
}

export enum SubtitlePosition {
  BOTTOM_CENTER = 'bottom_center',
  BOTTOM_LEFT = 'bottom_left',
  BOTTOM_RIGHT = 'bottom_right',
  TOP_CENTER = 'top_center',
  MIDDLE = 'middle',
}

export interface EffectData {
  type: EffectType;
  position?: Vector3;
  target?: string;
  intensity?: number;
  color?: Color;
  duration?: number;
  parameters?: Record<string, unknown>;
}

export enum EffectType {
  EXPLOSION = 'explosion',
  SMOKE = 'smoke',
  FIRE = 'fire',
  SPARK = 'spark',
  SHOCKWAVE = 'shockwave',
  LENS_FLARE = 'lens_flare',
  SCREEN_SHAKE = 'screen_shake',
  COLOR_GRADING = 'color_grading',
  BLOOM = 'bloom',
  MOTION_BLUR = 'motion_blur',
  CHROMATIC_ABERRATION = 'chromatic_aberration',
  VHS_EFFECT = 'vhs_effect',
}

export interface TransitionData {
  type: TransitionType;
  duration: number;
  color?: Color;
  direction?: TransitionDirection;
  mask?: string;
  easing?: EasingType;
}

export enum TransitionType {
  FADE = 'fade',
  WIPE = 'wipe',
  SLIDE = 'slide',
  ZOOM = 'zoom',
  IRIS = 'iris',
  CUSTOM = 'custom',
}

export enum TransitionDirection {
  LEFT = 'left',
  RIGHT = 'right',
  UP = 'up',
  DOWN = 'down',
  CENTER_OUT = 'center_out',
  EDGES_IN = 'edges_in',
}

export interface ObjectiveData {
  id: string;
  title: string;
  description: string;
  type: ObjectiveType;
  status?: ObjectiveStatus;
  progress?: number;
  maxProgress?: number;
  optional?: boolean;
  timed?: boolean;
  timeLimit?: number;
  rewards?: ObjectiveReward[];
}

export enum ObjectiveType {
  PRIMARY = 'primary',
  SECONDARY = 'secondary',
  BONUS = 'bonus',
  HIDDEN = 'hidden',
}

export enum ObjectiveStatus {
  ACTIVE = 'active',
  COMPLETED = 'completed',
  FAILED = 'failed',
  UPDATED = 'updated',
}

export interface ObjectiveReward {
  type: string;
  amount?: number;
  description?: string;
}

export interface UnitCommandData {
  unitId?: string;
  unitGroup?: string;
  command: UnitCommand;
  targetPosition?: Vector3;
  targetUnit?: string;
  queued?: boolean;
}

export enum UnitCommand {
  MOVE = 'move',
  ATTACK = 'attack',
  PATROL = 'patrol',
  HOLD = 'hold',
  BUILD = 'build',
  TRAIN = 'train',
  ABILITY = 'ability',
  STOP = 'stop',
  FORMATION = 'formation',
  FOLLOW = 'follow',
}

export interface EnvironmentData {
  timeOfDay?: number;
  weather?: WeatherType;
  fog?: FogSettings;
  ambientLight?: Color;
  directionalLight?: DirectionalLightSettings;
  skybox?: string;
}

export enum WeatherType {
  CLEAR = 'clear',
  CLOUDY = 'cloudy',
  RAIN = 'rain',
  STORM = 'storm',
  SNOW = 'snow',
  FOG = 'fog',
  SANDSTORM = 'sandstorm',
}

export interface FogSettings {
  enabled: boolean;
  color?: Color;
  density?: number;
  startDistance?: number;
  endDistance?: number;
}

export interface DirectionalLightSettings {
  direction: Vector3;
  color?: Color;
  intensity?: number;
  shadowEnabled?: boolean;
}

export interface AudioData {
  soundId?: string;
  musicId?: string;
  voiceoverId?: string;
  volume?: number;
  pan?: number;
  loop?: boolean;
  fadeIn?: number;
  fadeOut?: number;
  duckOthers?: boolean;
  priority?: AudioPriority;
}

export enum AudioPriority {
  LOW = 0,
  NORMAL = 1,
  HIGH = 2,
  CRITICAL = 3,
}

export class CinematicText {
  public text: string;
  public font?: string;
  public fontSize?: number;
  public color?: Color;
  public position: SubtitlePosition;
  public outline?: boolean;
  public outlineColor?: Color;
  public shadow?: boolean;
  public shadowOffset?: { x: number; y: number };
  public animation?: TextAnimation;
  public duration?: number;

  constructor(text: string, position: SubtitlePosition = SubtitlePosition.BOTTOM_CENTER) {
    this.text = text;
    this.position = position;
    this.outline = true;
    this.shadow = true;
    this.shadowOffset = { x: 2, y: 2 };
    this.animation = TextAnimation.NONE;
  }
}

export enum TextAnimation {
  NONE = 'none',
  FADE_IN = 'fade_in',
  FADE_OUT = 'fade_out',
  TYPEWRITER = 'typewriter',
  SLIDE_UP = 'slide_up',
  SLIDE_DOWN = 'slide_down',
  SCALE = 'scale',
  SHAKE = 'shake',
  PULSE = 'pulse',
}

export interface TransitionEffect {
  type: TransitionType;
  progress: number;
  duration: number;
  active: boolean;
  color?: Color;
  maskTexture?: string;
  easing?: EasingType;
}

export interface CutsceneAction {
  id: string;
  name: string;
  execute: (context: CutsceneContext) => Promise<void>;
  validate?: (context: CutsceneContext) => boolean;
  canSkip?: boolean;
  interruptible?: boolean;
}

export interface CutsceneContext {
  cinematicSystem: CinematicSystem;
  cameraDirector: CameraDirector;
  dialogueManager: DialogueManager;
  effectSequencer: EffectSequencer;
  sceneTransition: SceneTransition;
  timeline: TimelineState;
  variables: Map<string, unknown>;
}

export interface TimelineState {
  currentTime: number;
  totalDuration: number;
  isPlaying: boolean;
  isPaused: boolean;
  isSkipping: boolean;
  playbackSpeed: number;
  markers: TimelineMarker[];
}

export interface TimelineMarker {
  time: number;
  label?: string;
  type: MarkerType;
}

export enum MarkerType {
  CHAPTER = 'chapter',
  CHECKPOINT = 'checkpoint',
  NOTE = 'note',
  TRIGGER = 'trigger',
}

export interface BriefingConfig {
  id: string;
  missionId: string;
  title: string;
  subtitle?: string;
  backgroundImage?: string;
  primaryObjectives: ObjectiveData[];
  secondaryObjectives: ObjectiveData[];
  intel: IntelItem[];
  dialogues: BriefingDialogue[];
  cinematics: CinematicEvent[];
  audio?: AudioData;
  mapPreview?: BriefingMapPreview;
  faction?: FactionInfo;
}

export interface BriefingDialogue {
  id: string;
  speaker: string;
  speakerTitle?: string;
  text: string;
  avatar?: string;
  timestamp?: number;
}

export interface IntelItem {
  id: string;
  type: IntelType;
  title: string;
  description: string;
  image?: string;
  unlocked?: boolean;
  category?: string;
  rarity?: IntelRarity;
}

export enum IntelType {
  DOCUMENT = 'document',
  AUDIO_LOG = 'audio_log',
  VIDEO = 'video',
  IMAGE = 'image',
  TRANSCRIPT = 'transcript',
  MAP = 'map',
}

export enum IntelRarity {
  COMMON = 'common',
  UNCOMMON = 'uncommon',
  RARE = 'rare',
  LEGENDARY = 'legendary',
}

export interface BriefingMapPreview {
  image: string;
  highlightedAreas: MapArea[];
  markers: MapMarker[];
}

export interface MapArea {
  id: string;
  type: MapAreaType;
  coordinates: { x: number; y: number; width: number; height: number };
  label?: string;
}

export enum MapAreaType {
  ALLIED = 'allied',
  ENEMY = 'enemy',
  NEUTRAL = 'neutral',
  OBJECTIVE = 'objective',
  DANGER = 'danger',
}

export interface MapMarker {
  id: string;
  type: MapMarkerType;
  position: { x: number; y: number };
  label: string;
  icon?: string;
}

export enum MapMarkerType {
  START = 'start',
  END = 'end',
  OBJECTIVE = 'objective',
  ENEMY = 'enemy',
  RESOURCE = 'resource',
  STRUCTURE = 'structure',
}

export interface FactionInfo {
  id: string;
  name: string;
  emblem?: string;
  color?: Color;
  description?: string;
}

export class CinematicPlayer {
  private events: CinematicEvent[] = [];
  private currentEventIndex: number = 0;
  private isPlaying: boolean = false;
  private isPaused: boolean = false;
  private isSkipping: boolean = false;
  private currentTime: number = 0;
  private playbackSpeed: number = 1.0;
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public loadEvents(events: CinematicEvent[]): void {
    this.events = [...events].sort((a, b) => a.timestamp - b.timestamp);
    this.currentEventIndex = 0;
    this.currentTime = 0;
  }

  public play(): void {
    if (this.isPaused) {
      this.isPaused = false;
      this.emit('resume');
    } else {
      this.isPlaying = true;
      this.emit('play');
    }
  }

  public pause(): void {
    if (this.isPlaying && !this.isPaused) {
      this.isPaused = true;
      this.emit('pause');
    }
  }

  public stop(): void {
    this.isPlaying = false;
    this.isPaused = false;
    this.currentEventIndex = 0;
    this.currentTime = 0;
    this.emit('stop');
  }

  public skip(): void {
    if (this.isPlaying && !this.isSkipping) {
      this.isSkipping = true;
      this.emit('skip_start');
      this.jumpToEnd();
      this.isSkipping = false;
      this.emit('skip_end');
    }
  }

  public restart(): void {
    this.stop();
    this.play();
  }

  public seek(time: number): void {
    this.currentTime = Math.max(0, Math.min(time, this.getTotalDuration()));
    this.updateCurrentEventIndex();
    this.emit('seek', { time: this.currentTime });
  }

  public setPlaybackSpeed(speed: number): void {
    this.playbackSpeed = Math.max(0.1, Math.min(speed, 10.0));
    this.emit('speed_change', { speed: this.playbackSpeed });
  }

  public update(deltaTime: number): void {
    if (!this.isPlaying || this.isPaused || this.isSkipping) return;

    const delta = deltaTime * this.playbackSpeed;
    this.currentTime += delta;

    while (this.currentEventIndex < this.events.length) {
      const event = this.events[this.currentEventIndex];
      if (this.currentTime >= event.timestamp && this.currentTime < event.timestamp + event.duration) {
        if (!event.data['executed']) {
          event.data['executed'] = true;
          this.emit('event_start', { event });
        }
        this.emit('event_update', { event, progress: (this.currentTime - event.timestamp) / event.duration });
      } else if (this.currentTime >= event.timestamp + event.duration && event.data['executed']) {
        event.data['executed'] = false;
        this.emit('event_complete', { event });
        this.currentEventIndex++;
      } else if (this.currentTime < event.timestamp) {
        break;
      }
    }

    if (this.currentTime >= this.getTotalDuration()) {
      this.stop();
      this.emit('complete');
    }

    this.emit('update', { time: this.currentTime });
  }

  private updateCurrentEventIndex(): void {
    for (let i = 0; i < this.events.length; i++) {
      if (this.events[i].timestamp <= this.currentTime) {
        this.currentEventIndex = i;
      } else {
        break;
      }
    }
  }

  private jumpToEnd(): void {
    this.currentTime = this.getTotalDuration();
    for (const event of this.events) {
      event.data['executed'] = false;
    }
    this.currentEventIndex = this.events.length - 1;
    this.emit('jump_to_end');
  }

  public getTotalDuration(): number {
    if (this.events.length === 0) return 0;
    return Math.max(...this.events.map(e => e.timestamp + e.duration));
  }

  public getCurrentTime(): number {
    return this.currentTime;
  }

  public getIsPlaying(): boolean {
    return this.isPlaying;
  }

  public getIsPaused(): boolean {
    return this.isPaused;
  }

  public getIsSkipping(): boolean {
    return this.isSkipping;
  }

  public getPlaybackSpeed(): number {
    return this.playbackSpeed;
  }

  public getCurrentEvent(): CinematicEvent | null {
    if (this.currentEventIndex < this.events.length) {
      return this.events[this.currentEventIndex];
    }
    return null;
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }

  public getEvents(): CinematicEvent[] {
    return [...this.events];
  }

  public getEventsByType(type: CinematicEventType): CinematicEvent[] {
    return this.events.filter(e => e.type === type);
  }

  public getEventsInRange(startTime: number, endTime: number): CinematicEvent[] {
    return this.events.filter(e => {
      const eventEnd = e.timestamp + e.duration;
      return e.timestamp < endTime && eventEnd > startTime;
    });
  }
}

export class CameraDirector {
  private shots: CameraShot[] = [];
  private currentShotIndex: number = 0;
  private currentTime: number = 0;
  private isActive: boolean = false;
  private lookAtTarget: Vector3 | null = null;
  private shakeOffset: Vector3 = new Vector3();
  private shakeIntensity: number = 0;
  private letterboxConfig: LetterboxConfig | null = null;

  constructor() {}

  public setShots(shots: CameraShot[]): void {
    this.shots = shots;
    this.currentShotIndex = 0;
    this.currentTime = 0;
  }

  public play(): void {
    this.isActive = true;
  }

  public stop(): void {
    this.isActive = false;
    this.currentShotIndex = 0;
    this.currentTime = 0;
    this.shakeOffset.set(0, 0, 0);
  }

  public pause(): void {
    this.isActive = false;
  }

  public resume(): void {
    this.isActive = true;
  }

  public update(deltaTime: number): CameraState {
    if (!this.isActive || this.currentShotIndex >= this.shots.length) {
      return this.getDefaultCameraState();
    }

    const shot = this.shots[this.currentShotIndex];
    this.currentTime += deltaTime;

    const progress = Math.min(this.currentTime / shot.duration, 1);
    const easedProgress = this.applyEasing(progress, EasingType.SMOOTH_STEP);

    const position = this.lerpVector(shot.startPosition, shot.endPosition, easedProgress);
    const rotation = this.lerpQuaternion(shot.startRotation, shot.endRotation, easedProgress);
    const fov = this.lerp(shot.startFOV, shot.endFOV, easedProgress);

    if (this.shakeIntensity > 0) {
      this.shakeOffset.set(
        (Math.random() - 0.5) * this.shakeIntensity,
        (Math.random() - 0.5) * this.shakeIntensity,
        (Math.random() - 0.5) * this.shakeIntensity
      );
      position.add(this.shakeOffset);
    }

    if (this.lookAtTarget) {
      rotation.lookAt(this.lookAtTarget);
    }

    if (progress >= 1) {
      this.currentShotIndex++;
      this.currentTime = 0;
    }

    return {
      position,
      rotation,
      fov,
      depthOfField: shot.depthOfField,
      vignette: shot.vignette,
    };
  }

  private getDefaultCameraState(): CameraState {
    return {
      position: new Vector3(0, 10, 0),
      rotation: new Quaternion(),
      fov: 60,
    };
  }

  private lerp(a: number, b: number, t: number): number {
    return a + (b - a) * t;
  }

  private lerpVector(a: Vector3, b: Vector3, t: number): Vector3 {
    return new Vector3(
      this.lerp(a.x, b.x, t),
      this.lerp(a.y, b.y, t),
      this.lerp(a.z, b.z, t)
    );
  }

  private lerpQuaternion(a: Quaternion, b: Quaternion, t: number): Quaternion {
    const result = new Quaternion();
    const dot = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    let blendA = t;
    let blendB = 1 - t;
    if (dot < 0) {
      blendB = -t;
    }
    result.x = blendA * a.x + blendB * b.x;
    result.y = blendA * a.y + blendB * b.y;
    result.z = blendA * a.z + blendB * b.z;
    result.w = blendA * a.w + blendB * b.w;
    result.normalize();
    return result;
  }

  private applyEasing(t: number, easing: EasingType): number {
    switch (easing) {
      case EasingType.LINEAR:
        return t;
      case EasingType.QUAD_IN:
        return t * t;
      case EasingType.QUAD_OUT:
        return t * (2 - t);
      case EasingType.QUAD_IN_OUT:
        return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
      case EasingType.CUBIC_IN:
        return t * t * t;
      case EasingType.CUBIC_OUT:
        return (--t) * t * t + 1;
      case EasingType.SMOOTH_STEP:
        return t * t * (3 - 2 * t);
      case EasingType.EASE_OUT_BOUNCE:
        if (t < 1 / 2.75) {
          return 7.5625 * t * t;
        } else if (t < 2 / 2.75) {
          t -= 1.5 / 2.75;
          return 7.5625 * t * t + 0.75;
        } else if (t < 2.5 / 2.75) {
          t -= 2.25 / 2.75;
          return 7.5625 * t * t + 0.9375;
        } else {
          t -= 2.625 / 2.75;
          return 7.5625 * t * t + 0.984375;
        }
      default:
        return t;
    }
  }

  public setLookAtTarget(target: Vector3 | null): void {
    this.lookAtTarget = target;
  }

  public setShake(intensity: number, duration: number): void {
    this.shakeIntensity = intensity;
    setTimeout(() => {
      this.shakeIntensity = 0;
      this.shakeOffset.set(0, 0, 0);
    }, duration * 1000);
  }

  public setLetterbox(config: LetterboxConfig | null): void {
    this.letterboxConfig = config;
  }

  public getLetterboxConfig(): LetterboxConfig | null {
    return this.letterboxConfig;
  }

  public getCurrentShot(): CameraShot | null {
    if (this.currentShotIndex < this.shots.length) {
      return this.shots[this.currentShotIndex];
    }
    return null;
  }

  public getTotalDuration(): number {
    return this.shots.reduce((sum, shot) => sum + shot.duration, 0);
  }
}

export interface CameraState {
  position: Vector3;
  rotation: Quaternion;
  fov: number;
  depthOfField?: DepthOfFieldSettings;
  vignette?: VignetteSettings;
}

export interface LetterboxConfig {
  top: number;
  bottom: number;
  left: number;
  right: number;
  color: Color;
  animated: boolean;
}

export class DialogueManager {
  private dialogues: Map<string, DialogueData> = new Map();
  private activeDialogue: DialogueData | null = null;
  private currentText: string = '';
  private displayedText: string = '';
  private typewriterProgress: number = 0;
  private isTyping: boolean = false;
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public addDialogue(id: string, dialogue: DialogueData): void {
    this.dialogues.set(id, dialogue);
  }

  public removeDialogue(id: string): void {
    this.dialogues.delete(id);
  }

  public playDialogue(id: string): boolean {
    const dialogue = this.dialogues.get(id);
    if (dialogue) {
      this.activeDialogue = dialogue;
      this.currentText = dialogue.text;
      this.displayedText = '';
      this.typewriterProgress = 0;
      this.isTyping = true;
      this.emit('dialogue_start', { dialogue });
      return true;
    }
    return false;
  }

  public playDialogueBySpeaker(speakerId: string): boolean {
    for (const [id, dialogue] of this.dialogues) {
      if (dialogue.speakerId === speakerId) {
        return this.playDialogue(id);
      }
    }
    return false;
  }

  public stop(): void {
    this.activeDialogue = null;
    this.currentText = '';
    this.displayedText = '';
    this.isTyping = false;
    this.emit('dialogue_end');
  }

  public pause(): void {
    this.isTyping = false;
    this.emit('dialogue_pause');
  }

  public resume(): void {
    if (this.activeDialogue) {
      this.isTyping = true;
      this.emit('dialogue_resume');
    }
  }

  public update(deltaTime: number, typewriterSpeed: number = 30): void {
    if (!this.isTyping || !this.activeDialogue) return;

    this.typewriterProgress += deltaTime * typewriterSpeed;
    const charIndex = Math.floor(this.typewriterProgress);
    
    if (charIndex >= this.currentText.length) {
      this.displayedText = this.currentText;
      this.isTyping = false;
      this.emit('typewriter_complete');
    } else {
      this.displayedText = this.currentText.substring(0, charIndex);
    }

    this.emit('update', { 
      displayedText: this.displayedText,
      progress: this.typewriterProgress / this.currentText.length 
    });
  }

  public skipTypewriter(): void {
    if (this.isTyping) {
      this.displayedText = this.currentText;
      this.isTyping = false;
      this.emit('typewriter_complete');
    }
  }

  public setVolume(volume: number): void {
    this.emit('volume_change', { volume });
  }

  public getActiveDialogue(): DialogueData | null {
    return this.activeDialogue;
  }

  public getDisplayedText(): string {
    return this.displayedText;
  }

  public getCurrentProgress(): number {
    if (!this.activeDialogue) return 0;
    return this.typewriterProgress / this.currentText.length;
  }

  public getIsTyping(): boolean {
    return this.isTyping;
  }

  public getAllDialogues(): DialogueData[] {
    return Array.from(this.dialogues.values());
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class EffectSequencer {
  private effects: Map<string, EffectData> = new Map();
  private activeEffects: Map<string, ActiveEffect> = new Map();
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public registerEffect(id: string, effect: EffectData): void {
    this.effects.set(id, effect);
  }

  public playEffect(id: string, position?: Vector3): boolean {
    const effect = this.effects.get(id);
    if (effect) {
      const activeEffect: ActiveEffect = {
        id: `${id}_${Date.now()}`,
        type: effect.type,
        position: position || effect.position || new Vector3(),
        intensity: effect.intensity || 1,
        color: effect.color,
        duration: effect.duration || 1,
        elapsed: 0,
        parameters: effect.parameters,
      };
      this.activeEffects.set(activeEffect.id, activeEffect);
      this.emit('effect_start', { effect: activeEffect });
      return true;
    }
    return false;
  }

  public stopEffect(effectId: string): void {
    const effect = this.activeEffects.get(effectId);
    if (effect) {
      effect.elapsed = effect.duration;
      this.emit('effect_end', { effect });
    }
  }

  public stopAllEffects(): void {
    for (const [id] of this.activeEffects) {
      this.stopEffect(id);
    }
    this.activeEffects.clear();
  }

  public update(deltaTime: number): void {
    for (const [id, effect] of this.activeEffects) {
      effect.elapsed += deltaTime;
      
      const progress = effect.elapsed / effect.duration;
      this.emit('effect_update', { effect, progress });

      if (effect.elapsed >= effect.duration) {
        this.activeEffects.delete(id);
        this.emit('effect_complete', { effect });
      }
    }
  }

  public getActiveEffects(): ActiveEffect[] {
    return Array.from(this.activeEffects.values());
  }

  public getEffectById(id: string): EffectData | undefined {
    return this.effects.get(id);
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export interface ActiveEffect {
  id: string;
  type: EffectType;
  position: Vector3;
  intensity: number;
  color?: Color;
  duration: number;
  elapsed: number;
  parameters?: Record<string, unknown>;
}

export class SceneTransition {
  private currentTransition: TransitionEffect | null = null;
  private pendingTransition: TransitionEffect | null = null;
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public startTransition(transition: TransitionData): void {
    const effect: TransitionEffect = {
      type: transition.type,
      progress: 0,
      duration: transition.duration,
      active: true,
      color: transition.color,
      easing: transition.easing || EasingType.LINEAR,
    };

    if (this.currentTransition && this.currentTransition.active) {
      this.pendingTransition = effect;
    } else {
      this.currentTransition = effect;
      this.emit('transition_start', { transition: effect });
    }
  }

  public update(deltaTime: number): void {
    if (!this.currentTransition || !this.currentTransition.active) return;

    this.currentTransition.progress += deltaTime / this.currentTransition.duration;

    if (this.currentTransition.progress >= 1) {
      this.currentTransition.progress = 1;
      this.currentTransition.active = false;
      this.emit('transition_complete', { transition: this.currentTransition });

      if (this.pendingTransition) {
        this.currentTransition = this.pendingTransition;
        this.pendingTransition = null;
        this.emit('transition_start', { transition: this.currentTransition });
      }
    }

    this.emit('transition_update', { transition: this.currentTransition });
  }

  public cancelTransition(): void {
    if (this.currentTransition) {
      this.currentTransition.active = false;
      this.emit('transition_cancel', { transition: this.currentTransition });
      this.currentTransition = null;
    }
  }

  public getCurrentTransition(): TransitionEffect | null {
    return this.currentTransition;
  }

  public fadeIn(duration: number, color: Color = new Color(0, 0, 0)): void {
    this.startTransition({
      type: TransitionType.FADE,
      duration,
      color,
    });
  }

  public fadeOut(duration: number, color: Color = new Color(0, 0, 0)): void {
    this.startTransition({
      type: TransitionType.FADE,
      duration,
      color,
    });
  }

  public wipe(duration: number, direction: TransitionDirection): void {
    this.startTransition({
      type: TransitionType.WIPE,
      duration,
      direction,
    });
  }

  public iris(duration: number): void {
    this.startTransition({
      type: TransitionType.IRIS,
      duration,
    });
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class BriefingBuilder {
  private config: Partial<BriefingConfig> = {};
  private objectives: ObjectiveData[] = [];
  private dialogues: BriefingDialogue[] = [];
  private intel: IntelItem[] = [];
  private cinematics: CinematicEvent[] = [];

  constructor() {}

  public setId(id: string): this {
    this.config.id = id;
    return this;
  }

  public setMissionId(missionId: string): this {
    this.config.missionId = missionId;
    return this;
  }

  public setTitle(title: string): this {
    this.config.title = title;
    return this;
  }

  public setSubtitle(subtitle: string): this {
    this.config.subtitle = subtitle;
    return this;
  }

  public setBackgroundImage(image: string): this {
    this.config.backgroundImage = image;
    return this;
  }

  public setFaction(faction: FactionInfo): this {
    this.config.faction = faction;
    return this;
  }

  public setMapPreview(preview: BriefingMapPreview): this {
    this.config.mapPreview = preview;
    return this;
  }

  public addPrimaryObjective(objective: ObjectiveData): this {
    this.objectives.push({ ...objective, type: ObjectiveType.PRIMARY });
    return this;
  }

  public addSecondaryObjective(objective: ObjectiveData): this {
    this.objectives.push({ ...objective, type: ObjectiveType.SECONDARY });
    return this;
  }

  public addBonusObjective(objective: ObjectiveData): this {
    this.objectives.push({ ...objective, type: ObjectiveType.BONUS });
    return this;
  }

  public addDialogue(dialogue: BriefingDialogue): this {
    this.dialogues.push(dialogue);
    return this;
  }

  public addIntel(intel: IntelItem): this {
    this.intel.push(intel);
    return this;
  }

  public addCinematicEvent(event: CinematicEvent): this {
    this.cinematics.push(event);
    return this;
  }

  public addCameraShot(shot: CameraShot, timestamp: number = 0): this {
    this.cinematics.push({
      id: `camera_${Date.now()}`,
      type: CinematicEventType.CAMERA,
      timestamp,
      duration: shot.duration,
      data: { cameraShot: shot },
    });
    return this;
  }

  public addDialogueEvent(dialogue: DialogueData, timestamp: number = 0): this {
    this.cinematics.push({
      id: `dialogue_${Date.now()}`,
      type: CinematicEventType.DIALOGUE,
      timestamp,
      duration: (dialogue.subtitleDuration || dialogue.text.length / 20) * 1000,
      data: { dialogue },
    });
    return this;
  }

  public addEffectEvent(effect: EffectData, timestamp: number = 0): this {
    this.cinematics.push({
      id: `effect_${Date.now()}`,
      type: CinematicEventType.EFFECT,
      timestamp,
      duration: effect.duration || 1000,
      data: { effect },
    });
    return this;
  }

  public addTransitionEvent(transition: TransitionData, timestamp: number = 0): this {
    this.cinematics.push({
      id: `transition_${Date.now()}`,
      type: CinematicEventType.TRANSITION,
      timestamp,
      duration: transition.duration,
      data: { transition },
    });
    return this;
  }

  public build(): BriefingConfig {
    if (!this.config.id || !this.config.missionId || !this.config.title) {
      throw new Error('Briefing must have id, missionId, and title');
    }

    return {
      id: this.config.id,
      missionId: this.config.missionId,
      title: this.config.title,
      subtitle: this.config.subtitle,
      backgroundImage: this.config.backgroundImage,
      primaryObjectives: this.objectives.filter(o => o.type === ObjectiveType.PRIMARY),
      secondaryObjectives: this.objectives.filter(o => o.type === ObjectiveType.SECONDARY || o.type === ObjectiveType.BONUS),
      intel: this.intel,
      dialogues: this.dialogues,
      cinematics: this.cinematics,
      audio: this.config.audio,
      mapPreview: this.config.mapPreview,
      faction: this.config.faction,
    };
  }

  public reset(): void {
    this.config = {};
    this.objectives = [];
    this.dialogues = [];
    this.intel = [];
    this.cinematics = [];
  }

  public static fromExisting(briefing: BriefingConfig): BriefingBuilder {
    const builder = new BriefingBuilder();
    builder.config = { ...briefing };
    builder.objectives = [...briefing.primaryObjectives, ...briefing.secondaryObjectives];
    builder.dialogues = [...briefing.dialogues];
    builder.intel = [...briefing.intel];
    builder.cinematics = [...briefing.cinematics];
    return builder;
  }
}

export class BriefingRenderer {
  private briefing: BriefingConfig | null = null;
  private isVisible: boolean = false;
  private listeners: Map<string, Function[]> = new Map();
  private renderedElements: Map<string, HTMLElement> = new Map();

  constructor() {}

  public loadBriefing(briefing: BriefingConfig): void {
    this.briefing = briefing;
    this.emit('briefing_load', { briefing });
  }

  public show(): void {
    if (this.briefing) {
      this.isVisible = true;
      this.emit('show');
      this.renderBriefingUI();
    }
  }

  public hide(): void {
    this.isVisible = false;
    this.emit('hide');
    this.clearUI();
  }

  public toggle(): void {
    if (this.isVisible) {
      this.hide();
    } else {
      this.show();
    }
  }

  private renderBriefingUI(): void {
    if (!this.briefing) return;

    this.emit('render_start');

    if (this.briefing.backgroundImage) {
      this.emit('render_background', { image: this.briefing.backgroundImage });
    }

    this.emit('render_title', { title: this.briefing.title, subtitle: this.briefing.subtitle });

    if (this.briefing.faction) {
      this.emit('render_faction', { faction: this.briefing.faction });
    }

    if (this.briefing.mapPreview) {
      this.emit('render_map', { mapPreview: this.briefing.mapPreview });
    }

    this.renderObjectives();
    this.renderIntel();
    this.renderDialogues();

    this.emit('render_complete');
  }

  private renderObjectives(): void {
    if (!this.briefing) return;

    const primary = this.briefing.primaryObjectives;
    const secondary = this.briefing.secondaryObjectives;

    if (primary.length > 0) {
      this.emit('render_objectives_section', { type: 'primary', objectives: primary });
    }

    if (secondary.length > 0) {
      this.emit('render_objectives_section', { type: 'secondary', objectives: secondary });
    }
  }

  private renderIntel(): void {
    if (!this.briefing || this.briefing.intel.length === 0) return;
    this.emit('render_intel', { intel: this.briefing.intel });
  }

  private renderDialogues(): void {
    if (!this.briefing || this.briefing.dialogues.length === 0) return;
    this.emit('render_dialogues', { dialogues: this.briefing.dialogues });
  }

  private clearUI(): void {
    this.renderedElements.clear();
    this.emit('clear');
  }

  public getBriefing(): BriefingConfig | null {
    return this.briefing;
  }

  public getIsVisible(): boolean {
    return this.isVisible;
  }

  public highlightObjective(objectiveId: string): void {
    this.emit('highlight_objective', { objectiveId });
  }

  public setObjectiveProgress(objectiveId: string, progress: number): void {
    this.emit('objective_progress', { objectiveId, progress });
  }

  public revealIntel(intelId: string): void {
    this.emit('reveal_intel', { intelId });
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class ObjectiveDisplay {
  private objectives: Map<string, ObjectiveData> = new Map();
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public addObjective(objective: ObjectiveData): void {
    this.objectives.set(objective.id, { ...objective, status: ObjectiveStatus.ACTIVE });
    this.emit('objective_added', { objective });
  }

  public removeObjective(id: string): void {
    const objective = this.objectives.get(id);
    if (objective) {
      this.objectives.delete(id);
      this.emit('objective_removed', { objective });
    }
  }

  public updateObjective(id: string, updates: Partial<ObjectiveData>): void {
    const objective = this.objectives.get(id);
    if (objective) {
      const updated = { ...objective, ...updates };
      this.objectives.set(id, updated);
      this.emit('objective_updated', { objective: updated, changes: updates });
    }
  }

  public completeObjective(id: string): void {
    this.updateObjective(id, { status: ObjectiveStatus.COMPLETED, progress: 100 });
  }

  public failObjective(id: string): void {
    this.updateObjective(id, { status: ObjectiveStatus.FAILED });
  }

  public setObjectiveProgress(id: string, progress: number, maxProgress?: number): void {
    const objective = this.objectives.get(id);
    if (objective) {
      const updates: Partial<ObjectiveData> = { progress };
      if (maxProgress !== undefined) {
        updates.maxProgress = maxProgress;
      }
      if (progress >= (maxProgress || 100)) {
        updates.status = ObjectiveStatus.COMPLETED;
      }
      this.updateObjective(id, updates);
    }
  }

  public getObjective(id: string): ObjectiveData | undefined {
    return this.objectives.get(id);
  }

  public getAllObjectives(): ObjectiveData[] {
    return Array.from(this.objectives.values());
  }

  public getPrimaryObjectives(): ObjectiveData[] {
    return this.getAllObjectives().filter(o => o.type === ObjectiveType.PRIMARY);
  }

  public getSecondaryObjectives(): ObjectiveData[] {
    return this.getAllObjectives().filter(o => o.type === ObjectiveType.SECONDARY || o.type === ObjectiveType.BONUS);
  }

  public getActiveObjectives(): ObjectiveData[] {
    return this.getAllObjectives().filter(o => o.status === ObjectiveStatus.ACTIVE);
  }

  public getCompletedObjectives(): ObjectiveData[] {
    return this.getAllObjectives().filter(o => o.status === ObjectiveStatus.COMPLETED);
  }

  public getFailedObjectives(): ObjectiveData[] {
    return this.getAllObjectives().filter(o => o.status === ObjectiveStatus.FAILED);
  }

  public getOptionalObjectives(): ObjectiveData[] {
    return this.getAllObjectives().filter(o => o.optional);
  }

  public checkCompletion(): ObjectiveCompletionStatus {
    const primary = this.getPrimaryObjectives();
    const primaryCompleted = primary.filter(o => o.status === ObjectiveStatus.COMPLETED).length;
    const primaryFailed = primary.filter(o => o.status === ObjectiveStatus.FAILED).length;

    return {
      allPrimaryComplete: primaryCompleted === primary.length,
      primaryProgress: `${primaryCompleted}/${primary.length}`,
      anyPrimaryFailed: primaryFailed > 0,
      totalObjectives: this.objectives.size,
      completedObjectives: this.getCompletedObjectives().length,
    };
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export interface ObjectiveCompletionStatus {
  allPrimaryComplete: boolean;
  primaryProgress: string;
  anyPrimaryFailed: boolean;
  totalObjectives: number;
  completedObjectives: number;
}

export class IntelCollector {
  private collectedIntel: Map<string, IntelItem> = new Map();
  private availableIntel: Map<string, IntelItem> = new Map();
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public registerIntel(intel: IntelItem): void {
    const item = { ...intel, unlocked: intel.unlocked ?? false };
    this.availableIntel.set(intel.id, item);
    this.emit('intel_registered', { intel: item });
  }

  public unlockIntel(id: string): boolean {
    const intel = this.availableIntel.get(id);
    if (intel && !intel.unlocked) {
      intel.unlocked = true;
      this.emit('intel_unlocked', { intel });
      return true;
    }
    return false;
  }

  public collectIntel(id: string): boolean {
    const intel = this.availableIntel.get(id);
    if (intel && intel.unlocked) {
      this.collectedIntel.set(id, intel);
      this.emit('intel_collected', { intel });
      return true;
    }
    return false;
  }

  public isCollected(id: string): boolean {
    return this.collectedIntel.has(id);
  }

  public isUnlocked(id: string): boolean {
    const intel = this.availableIntel.get(id);
    return intel ? intel.unlocked : false;
  }

  public getIntel(id: string): IntelItem | undefined {
    return this.availableIntel.get(id);
  }

  public getAllAvailableIntel(): IntelItem[] {
    return Array.from(this.availableIntel.values());
  }

  public getCollectedIntel(): IntelItem[] {
    return Array.from(this.collectedIntel.values());
  }

  public getUnlockedIntel(): IntelItem[] {
    return this.getAllAvailableIntel().filter(i => i.unlocked);
  }

  public getIntelByCategory(category: string): IntelItem[] {
    return this.getAllAvailableIntel().filter(i => i.category === category);
  }

  public getIntelByRarity(rarity: IntelRarity): IntelItem[] {
    return this.getAllAvailableIntel().filter(i => i.rarity === rarity);
  }

  public getCollectionProgress(): { collected: number; total: number; percentage: number } {
    const total = this.availableIntel.size;
    const collected = this.collectedIntel.size;
    return {
      collected,
      total,
      percentage: total > 0 ? (collected / total) * 100 : 0,
    };
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class CinematicRecorder {
  private recordedEvents: CinematicEvent[] = [];
  private isRecording: boolean = false;
  private recordingStartTime: number = 0;
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public startRecording(): void {
    this.recordedEvents = [];
    this.isRecording = true;
    this.recordingStartTime = performance.now();
    this.emit('recording_start');
  }

  public stopRecording(): CinematicEvent[] {
    this.isRecording = false;
    this.emit('recording_stop', { events: this.recordedEvents });
    return [...this.recordedEvents];
  }

  public recordEvent(event: Omit<CinematicEvent, 'timestamp'>): void {
    if (!this.isRecording) return;

    const timestamp = performance.now() - this.recordingStartTime;
    const fullEvent: CinematicEvent = {
      ...event,
      timestamp,
    };

    this.recordedEvents.push(fullEvent);
    this.emit('event_recorded', { event: fullEvent });
  }

  public recordCameraShot(shot: CameraShot): void {
    this.recordEvent({
      id: shot.id,
      type: CinematicEventType.CAMERA,
      duration: shot.duration,
      data: { cameraShot: shot },
    });
  }

  public recordDialogue(dialogue: DialogueData, duration: number): void {
    this.recordEvent({
      id: `dialogue_${Date.now()}`,
      type: CinematicEventType.DIALOGUE,
      duration,
      data: { dialogue },
    });
  }

  public recordEffect(effect: EffectData): void {
    this.recordEvent({
      id: `effect_${Date.now()}`,
      type: CinematicEventType.EFFECT,
      duration: effect.duration || 1,
      data: { effect },
    });
  }

  public recordTransition(transition: TransitionData): void {
    this.recordEvent({
      id: `transition_${Date.now()}`,
      type: CinematicEventType.TRANSITION,
      duration: transition.duration,
      data: { transition },
    });
  }

  public recordAction(actionType: string, data: Record<string, unknown>): void {
    this.recordEvent({
      id: `action_${Date.now()}`,
      type: CinematicEventType.ACTION,
      duration: 0,
      data: { action: actionType, ...data },
    });
  }

  public getRecordedEvents(): CinematicEvent[] {
    return [...this.recordedEvents];
  }

  public getIsRecording(): boolean {
    return this.isRecording;
  }

  public getRecordingDuration(): number {
    if (!this.isRecording) return 0;
    return performance.now() - this.recordingStartTime;
  }

  public clearRecording(): void {
    this.recordedEvents = [];
    this.emit('recording_cleared');
  }

  public exportRecording(): string {
    return JSON.stringify(this.recordedEvents, null, 2);
  }

  public importRecording(json: string): boolean {
    try {
      this.recordedEvents = JSON.parse(json);
      this.emit('recording_imported', { events: this.recordedEvents });
      return true;
    } catch {
      return false;
    }
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class TimelineEditor {
  private events: CinematicEvent[] = [];
  private selectedEventId: string | null = null;
  private clipboard: CinematicEvent[] = [];
  private undoStack: CinematicEvent[][] = [];
  private redoStack: CinematicEvent[][] = [];
  private maxUndoLevels: number = 50;
  private listeners: Map<string, Function[]> = new Map();

  constructor() {}

  public setEvents(events: CinematicEvent[]): void {
    this.saveUndoState();
    this.events = [...events].sort((a, b) => a.timestamp - b.timestamp);
    this.emit('events_changed', { events: this.events });
  }

  public addEvent(event: CinematicEvent): void {
    this.saveUndoState();
    this.events.push(event);
    this.sortEvents();
    this.emit('event_added', { event });
  }

  public removeEvent(eventId: string): void {
    this.saveUndoState();
    const index = this.events.findIndex(e => e.id === eventId);
    if (index > -1) {
      const removed = this.events.splice(index, 1)[0];
      this.emit('event_removed', { event: removed });
    }
  }

  public updateEvent(eventId: string, updates: Partial<CinematicEvent>): void {
    this.saveUndoState();
    const event = this.events.find(e => e.id === eventId);
    if (event) {
      Object.assign(event, updates);
      this.sortEvents();
      this.emit('event_updated', { event });
    }
  }

  public moveEvent(eventId: string, newTimestamp: number): void {
    const event = this.events.find(e => e.id === eventId);
    if (event) {
      this.saveUndoState();
      event.timestamp = Math.max(0, newTimestamp);
      this.sortEvents();
      this.emit('event_moved', { event, newTimestamp });
    }
  }

  public resizeEvent(eventId: string, newDuration: number): void {
    const event = this.events.find(e => e.id === eventId);
    if (event) {
      this.saveUndoState();
      event.duration = Math.max(0, newDuration);
      this.emit('event_resized', { event, newDuration });
    }
  }

  public selectEvent(eventId: string | null): void {
    this.selectedEventId = eventId;
    this.emit('selection_changed', { selectedId: eventId });
  }

  public duplicateEvent(eventId: string): CinematicEvent | null {
    const event = this.events.find(e => e.id === eventId);
    if (event) {
      this.saveUndoState();
      const duplicate: CinematicEvent = {
        ...JSON.parse(JSON.stringify(event)),
        id: `${event.id}_copy_${Date.now()}`,
        timestamp: event.timestamp + event.duration + 0.1,
      };
      this.events.push(duplicate);
      this.emit('event_duplicated', { original: event, duplicate });
      return duplicate;
    }
    return null;
  }

  public copySelected(): void {
    if (this.selectedEventId) {
      const event = this.events.find(e => e.id === this.selectedEventId);
      if (event) {
        this.clipboard = [JSON.parse(JSON.stringify(event))];
        this.emit('events_copied', { count: this.clipboard.length });
      }
    }
  }

  public cutSelected(): void {
    if (this.selectedEventId) {
      this.copySelected();
      this.removeEvent(this.selectedEventId);
    }
  }

  public paste(): void {
    if (this.clipboard.length > 0) {
      this.saveUndoState();
      const newEvents: CinematicEvent[] = [];
      for (const event of this.clipboard) {
        const pasted: CinematicEvent = {
          ...JSON.parse(JSON.stringify(event)),
          id: `${event.id}_pasted_${Date.now()}`,
          timestamp: this.getCurrentTime() + event.timestamp,
        };
        this.events.push(pasted);
        newEvents.push(pasted);
      }
      this.sortEvents();
      this.emit('events_pasted', { events: newEvents });
    }
  }

  public deleteSelected(): void {
    if (this.selectedEventId) {
      this.removeEvent(this.selectedEventId);
      this.selectedEventId = null;
    }
  }

  public selectAll(): void {
    const ids = this.events.map(e => e.id);
    this.emit('selection_changed', { selectedIds: ids, selectAll: true });
  }

  public selectRange(startTime: number, endTime: number): void {
    const ids = this.events
      .filter(e => e.timestamp >= startTime && e.timestamp <= endTime)
      .map(e => e.id);
    this.emit('selection_changed', { selectedIds: ids });
  }

  public undo(): void {
    if (this.undoStack.length > 0) {
      this.redoStack.push([...this.events]);
      this.events = this.undoStack.pop()!;
      this.emit('undo', { events: this.events });
    }
  }

  public redo(): void {
    if (this.redoStack.length > 0) {
      this.undoStack.push([...this.events]);
      this.events = this.redoStack.pop()!;
      this.emit('redo', { events: this.events });
    }
  }

  private saveUndoState(): void {
    this.undoStack.push([...this.events]);
    if (this.undoStack.length > this.maxUndoLevels) {
      this.undoStack.shift();
    }
    this.redoStack = [];
  }

  private sortEvents(): void {
    this.events.sort((a, b) => a.timestamp - b.timestamp);
  }

  private getCurrentTime(): number {
    if (this.events.length === 0) return 0;
    return Math.max(...this.events.map(e => e.timestamp));
  }

  public getEvents(): CinematicEvent[] {
    return [...this.events];
  }

  public getSelectedEvent(): CinematicEvent | null {
    if (this.selectedEventId) {
      return this.events.find(e => e.id === this.selectedEventId) || null;
    }
    return null;
  }

  public getEventsAtTime(time: number): CinematicEvent[] {
    return this.events.filter(e => 
      e.timestamp <= time && e.timestamp + e.duration >= time
    );
  }

  public getEventsByType(type: CinematicEventType): CinematicEvent[] {
    return this.events.filter(e => e.type === type);
  }

  public canUndo(): boolean {
    return this.undoStack.length > 0;
  }

  public canRedo(): boolean {
    return this.redoStack.length > 0;
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class ActionLibrary {
  private actions: Map<string, CutsceneAction> = new Map();
  private listeners: Map<string, Function[]> = new Map();

  constructor() {
    this.registerDefaultActions();
  }

  private registerDefaultActions(): void {
    this.registerAction({
      id: 'wait',
      name: 'Wait',
      execute: async (context) => {
        await new Promise(resolve => setTimeout(resolve, 1000));
      },
      canSkip: true,
      interruptible: true,
    });

    this.registerAction({
      id: 'fade_in',
      name: 'Fade In',
      execute: async (context) => {
        context.sceneTransition.fadeIn(1);
        await new Promise(resolve => setTimeout(resolve, 1000));
      },
      canSkip: true,
      interruptible: false,
    });

    this.registerAction({
      id: 'fade_out',
      name: 'Fade Out',
      execute: async (context) => {
        context.sceneTransition.fadeOut(1);
        await new Promise(resolve => setTimeout(resolve, 1000));
      },
      canSkip: true,
      interruptible: false,
    });

    this.registerAction({
      id: 'shake_camera',
      name: 'Shake Camera',
      execute: async (context) => {
        context.cameraDirector.setShake(0.5, 0.5);
        await new Promise(resolve => setTimeout(resolve, 500));
      },
      canSkip: false,
      interruptible: true,
    });

    this.registerAction({
      id: 'set_letterbox',
      name: 'Set Letterbox',
      execute: async (context) => {
        context.cameraDirector.setLetterbox({
          top: 0.1,
          bottom: 0.1,
          left: 0,
          right: 0,
          color: new Color(0, 0, 0),
          animated: true,
        });
      },
      canSkip: false,
      interruptible: true,
    });

    this.registerAction({
      id: 'remove_letterbox',
      name: 'Remove Letterbox',
      execute: async (context) => {
        context.cameraDirector.setLetterbox(null);
      },
      canSkip: false,
      interruptible: true,
    });

    this.registerAction({
      id: 'play_sound',
      name: 'Play Sound',
      execute: async (context) => {
        context.variables.set('sound_playing', true);
      },
      canSkip: false,
      interruptible: true,
    });
  }

  public registerAction(action: CutsceneAction): void {
    this.actions.set(action.id, action);
  }

  public unregisterAction(id: string): void {
    this.actions.delete(id);
  }

  public getAction(id: string): CutsceneAction | undefined {
    return this.actions.get(id);
  }

  public async executeAction(id: string, context: CutsceneContext): Promise<boolean> {
    const action = this.actions.get(id);
    if (action) {
      try {
        await action.execute(context);
        return true;
      } catch (error) {
        this.emit('action_error', { action, error });
        return false;
      }
    }
    return false;
  }

  public getAllActions(): CutsceneAction[] {
    return Array.from(this.actions.values());
  }

  public getActionNames(): string[] {
    return Array.from(this.actions.keys());
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class TriggerSystem {
  private triggers: Map<string, CinematicTrigger> = new Map();
  private activeTriggers: Set<string> = new Set();
  private listeners: Map<string, Function[]> = new Map();
  private conditionEvaluators: Map<string, (condition: CinematicCondition) => boolean> = new Map();

  constructor() {
    this.registerDefaultConditionEvaluators();
  }

  private registerDefaultConditionEvaluators(): void {
    this.registerConditionEvaluator('unit_exists', (condition) => {
      return condition.value !== undefined;
    });

    this.registerConditionEvaluator('objective_complete', (condition) => {
      return condition.value === true;
    });

    this.registerConditionEvaluator('time_elapsed', (condition) => {
      return typeof condition.value === 'number' && condition.value > 0;
    });

    this.registerConditionEvaluator('variable_equals', (condition) => {
      return true;
    });
  }

  public registerTrigger(id: string, trigger: CinematicTrigger): void {
    this.triggers.set(id, trigger);
    this.emit('trigger_registered', { id, trigger });
  }

  public unregisterTrigger(id: string): void {
    this.triggers.delete(id);
    this.activeTriggers.delete(id);
    this.emit('trigger_unregistered', { id });
  }

  public activateTrigger(id: string): void {
    const trigger = this.triggers.get(id);
    if (trigger) {
      this.activeTriggers.add(id);
      this.emit('trigger_activated', { id, trigger });
    }
  }

  public deactivateTrigger(id: string): void {
    this.activeTriggers.delete(id);
    this.emit('trigger_deactivated', { id });
  }

  public registerConditionEvaluator(type: string, evaluator: (condition: CinematicCondition) => boolean): void {
    this.conditionEvaluators.set(type, evaluator);
  }

  public evaluateCondition(condition: CinematicCondition): boolean {
    const evaluator = this.conditionEvaluators.get(condition.type);
    if (evaluator) {
      return evaluator(condition);
    }
    return false;
  }

  public evaluateConditions(conditions: CinematicCondition[]): boolean {
    for (const condition of conditions) {
      if (!this.evaluateCondition(condition)) {
        return false;
      }
    }
    return true;
  }

  public checkTriggers(timestamp: number, context: Map<string, unknown>): void {
    for (const [id, trigger] of this.triggers) {
      if (trigger.type === CinematicTriggerType.TIME && trigger.delay !== undefined) {
        const shouldFire = timestamp >= trigger.delay;
        if (shouldFire && this.activeTriggers.has(id)) {
          this.fireTrigger(id, context);
        }
      }
    }
  }

  public fireTrigger(id: string, context: Map<string, unknown>): void {
    const trigger = this.triggers.get(id);
    if (trigger && trigger.callback) {
      if (!trigger.condition || this.evaluateCondition(trigger.condition)) {
        trigger.callback();
        this.emit('trigger_fired', { id, trigger, context });
      }
    }
  }

  public fireCustomEvent(eventName: string, data?: unknown): void {
    this.emit('custom_event', { eventName, data });
  }

  public getActiveTriggers(): string[] {
    return Array.from(this.activeTriggers);
  }

  public getTrigger(id: string): CinematicTrigger | undefined {
    return this.triggers.get(id);
  }

  public getAllTriggers(): CinematicTrigger[] {
    return Array.from(this.triggers.values());
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data?: unknown): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      for (const listener of listeners) {
        listener(data);
      }
    }
  }
}

export class CinematicSystem extends EventEmitter {
  public player: CinematicPlayer;
  public cameraDirector: CameraDirector;
  public dialogueManager: DialogueManager;
  public effectSequencer: EffectSequencer;
  public sceneTransition: SceneTransition;
  public briefingRenderer: BriefingRenderer;
  public objectiveDisplay: ObjectiveDisplay;
  public intelCollector: IntelCollector;
  public recorder: CinematicRecorder;
  public timelineEditor: TimelineEditor;
  public actionLibrary: ActionLibrary;
  public triggerSystem: TriggerSystem;

  private isInitialized: boolean = false;
  private letterboxConfig: LetterboxConfig | null = null;
  private pillarboxConfig: PillarboxConfig | null = null;
  private audioSyncEnabled: boolean = true;
  private lastUpdateTime: number = 0;
  private variables: Map<string, unknown> = new Map();

  constructor() {
    super();
    this.player = new CinematicPlayer();
    this.cameraDirector = new CameraDirector();
    this.dialogueManager = new DialogueManager();
    this.effectSequencer = new EffectSequencer();
    this.sceneTransition = new SceneTransition();
    this.briefingRenderer = new BriefingRenderer();
    this.objectiveDisplay = new ObjectiveDisplay();
    this.intelCollector = new IntelCollector();
    this.recorder = new CinematicRecorder();
    this.timelineEditor = new TimelineEditor();
    this.actionLibrary = new ActionLibrary();
    this.triggerSystem = new TriggerSystem();

    this.setupEventListeners();
  }

  private setupEventListeners(): void {
    this.player.on('event_start', (data: { event: CinematicEvent }) => {
      this.handleEventStart(data.event);
    });

    this.player.on('event_complete', (data: { event: CinematicEvent }) => {
      this.handleEventComplete(data.event);
    });

    this.player.on('complete', () => {
      this.emit('cinematic_complete');
    });

    this.player.on('stop', () => {
      this.emit('cinematic_stopped');
    });

    this.sceneTransition.on('transition_complete', () => {
      this.emit('transition_complete');
    });
  }

  public initialize(): void {
    if (this.isInitialized) return;

    this.isInitialized = true;
    this.emit('initialized');
  }

  public update(deltaTime: number): void {
    if (!this.isInitialized) return;

    const scaledDelta = deltaTime;
    this.lastUpdateTime = performance.now();

    this.player.update(scaledDelta);
    
    this.cameraDirector.update(scaledDelta);
    this.dialogueManager.update(scaledDelta);
    this.effectSequencer.update(scaledDelta);
    this.sceneTransition.update(scaledDelta);

    this.triggerSystem.checkTriggers(this.player.getCurrentTime(), this.variables);
  }

  public loadCinematic(events: CinematicEvent[]): void {
    this.player.loadEvents(events);
    this.timelineEditor.setEvents(events);
    this.emit('cinematic_loaded', { eventCount: events.length });
  }

  public play(): void {
    this.player.play();
    this.cameraDirector.play();
    this.emit('play');
  }

  public pause(): void {
    this.player.pause();
    this.cameraDirector.pause();
    this.dialogueManager.pause();
    this.emit('pause');
  }

  public stop(): void {
    this.player.stop();
    this.cameraDirector.stop();
    this.dialogueManager.stop();
    this.effectSequencer.stopAllEffects();
    this.emit('stop');
  }

  public skip(): void {
    this.player.skip();
    this.emit('skip');
  }

  public restart(): void {
    this.player.restart();
    this.emit('restart');
  }

  public seek(time: number): void {
    this.player.seek(time);
  }

  public setPlaybackSpeed(speed: number): void {
    this.player.setPlaybackSpeed(speed);
  }

  public getCurrentTime(): number {
    return this.player.getCurrentTime();
  }

  public getTotalDuration(): number {
    return this.player.getTotalDuration();
  }

  public getIsPlaying(): boolean {
    return this.player.getIsPlaying();
  }

  private handleEventStart(event: CinematicEvent): void {
    switch (event.type) {
      case CinematicEventType.CAMERA:
        if (event.data.cameraShot) {
          this.cameraDirector.setShots([event.data.cameraShot]);
          this.cameraDirector.play();
        }
        break;
      case CinematicEventType.DIALOGUE:
        if (event.data.dialogue) {
          const dialogue = event.data.dialogue;
          this.dialogueManager.playDialogue(event.id);
          this.emit('dialogue_start', { dialogue });
        }
        break;
      case CinematicEventType.EFFECT:
        if (event.data.effect) {
          const effect = event.data.effect;
          this.effectSequencer.playEffect(effect.type, effect.position);
          this.emit('effect_start', { effect });
        }
        break;
      case CinematicEventType.TRANSITION:
        if (event.data.transition) {
          this.sceneTransition.startTransition(event.data.transition);
        }
        break;
      case CinematicEventType.OBJECTIVE:
        if (event.data.objective) {
          this.objectiveDisplay.addObjective(event.data.objective);
          this.emit('objective_added', { objective: event.data.objective });
        }
        break;
      case CinematicEventType.AUDIO:
        if (event.data.audio) {
          this.emit('audio_play', { audio: event.data.audio });
        }
        break;
      case CinematicEventType.TEXT:
        if (event.data.text) {
          this.emit('text_display', { text: event.data.text });
        }
        break;
      case CinematicEventType.ACTION:
        if (event.data.action) {
          this.executeAction(String(event.data.action));
        }
        break;
    }

    this.emit('event_start', { event });
  }

  private handleEventComplete(event: CinematicEvent): void {
    this.emit('event_complete', { event });
  }

  public createBriefing(config: Partial<BriefingConfig>): BriefingBuilder {
    const builder = new BriefingBuilder();
    if (config.id) builder.setId(config.id);
    if (config.missionId) builder.setMissionId(config.missionId);
    if (config.title) builder.setTitle(config.title);
    if (config.subtitle) builder.setSubtitle(config.subtitle);
    if (config.backgroundImage) builder.setBackgroundImage(config.backgroundImage);
    if (config.faction) builder.setFaction(config.faction);
    if (config.mapPreview) builder.setMapPreview(config.mapPreview);
    return builder;
  }

  public loadBriefing(briefing: BriefingConfig): void {
    this.briefingRenderer.loadBriefing(briefing);

    for (const objective of briefing.primaryObjectives) {
      this.objectiveDisplay.addObjective(objective);
    }
    for (const objective of briefing.secondaryObjectives) {
      this.objectiveDisplay.addObjective(objective);
    }

    for (const intel of briefing.intel) {
      this.intelCollector.registerIntel(intel);
    }

    if (briefing.cinematics.length > 0) {
      this.loadCinematic(briefing.cinematics);
    }

    this.emit('briefing_loaded', { briefing });
  }

  public showBriefing(): void {
    this.briefingRenderer.show();
    this.emit('briefing_show');
  }

  public hideBriefing(): void {
    this.briefingRenderer.hide();
    this.emit('briefing_hide');
  }

  public setLetterbox(config: LetterboxConfig | null): void {
    this.letterboxConfig = config;
    this.cameraDirector.setLetterbox(config);
    this.emit('letterbox_changed', { config });
  }

  public setPillarbox(config: PillarboxConfig | null): void {
    this.pillarboxConfig = config;
    this.emit('pillarbox_changed', { config });
  }

  public getLetterboxConfig(): LetterboxConfig | null {
    return this.letterboxConfig;
  }

  public getPillarboxConfig(): PillarboxConfig | null {
    return this.pillarboxConfig;
  }

  public enableAudioSync(enabled: boolean): void {
    this.audioSyncEnabled = enabled;
    this.emit('audio_sync_changed', { enabled });
  }

  public isAudioSyncEnabled(): boolean {
    return this.audioSyncEnabled;
  }

  public async executeAction(actionId: string): Promise<boolean> {
    const context: CutsceneContext = {
      cinematicSystem: this,
      cameraDirector: this.cameraDirector,
      dialogueManager: this.dialogueManager,
      effectSequencer: this.effectSequencer,
      sceneTransition: this.sceneTransition,
      timeline: this.getTimelineState(),
      variables: this.variables,
    };

    const result = await this.actionLibrary.executeAction(actionId, context);
    this.emit('action_executed', { actionId, success: result });
    return result;
  }

  public registerCustomAction(action: CutsceneAction): void {
    this.actionLibrary.registerAction(action);
    this.emit('action_registered', { action });
  }

  public setVariable(key: string, value: unknown): void {
    this.variables.set(key, value);
    this.emit('variable_changed', { key, value });
  }

  public getVariable<T>(key: string): T | undefined {
    return this.variables.get(key) as T | undefined;
  }

  public clearVariables(): void {
    this.variables.clear();
    this.emit('variables_cleared');
  }

  private getTimelineState(): TimelineState {
    return {
      currentTime: this.player.getCurrentTime(),
      totalDuration: this.player.getTotalDuration(),
      isPlaying: this.player.getIsPlaying(),
      isPaused: this.player.getIsPaused(),
      isSkipping: this.player.getIsSkipping(),
      playbackSpeed: this.player.getPlaybackSpeed(),
      markers: [],
    };
  }

  public startRecording(): void {
    this.recorder.startRecording();
    this.emit('recording_started');
  }

  public stopRecording(): CinematicEvent[] {
    const events = this.recorder.stopRecording();
    this.emit('recording_stopped', { eventCount: events.length });
    return events;
  }

  public playCameraSequence(sequence: CameraSequence): void {
    this.cameraDirector.setShots(sequence.shots);
    this.cameraDirector.play();
    
    if (sequence.loop) {
      this.cameraDirector.update = (() => {
        const originalUpdate = this.cameraDirector.update;
        return (deltaTime: number) => {
          const state = originalUpdate(deltaTime);
          if (this.cameraDirector.getTotalDuration() <= this.player.getCurrentTime()) {
            this.cameraDirector.setShots(sequence.shots);
          }
          return state;
        };
      })();
    }
  }

  public showCinematicText(text: CinematicText, duration: number = 3): void {
    this.emit('cinematic_text_show', { text, duration });
    
    setTimeout(() => {
      this.emit('cinematic_text_hide', { text });
    }, duration * 1000);
  }

  public createTransition(type: TransitionType, duration: number, options?: Partial<TransitionData>): void {
    const transition: TransitionData = {
      type,
      duration,
      color: options?.color,
      direction: options?.direction,
      easing: options?.easing,
    };
    this.sceneTransition.startTransition(transition);
  }

  public fadeToBlack(duration: number = 1): Promise<void> {
    return new Promise((resolve) => {
      this.sceneTransition.fadeIn(duration);
      setTimeout(() => {
        resolve();
      }, duration * 1000);
    });
  }

  public fadeFromBlack(duration: number = 1): Promise<void> {
    return new Promise((resolve) => {
      this.sceneTransition.fadeOut(duration);
      setTimeout(() => {
        resolve();
      }, duration * 1000);
    });
  }

  public shakeCamera(intensity: number, duration: number): void {
    this.cameraDirector.setShake(intensity, duration);
    this.emit('camera_shake', { intensity, duration });
  }

  public setCameraLookAt(target: Vector3): void {
    this.cameraDirector.setLookAtTarget(target);
  }

  public clearCameraLookAt(): void {
    this.cameraDirector.setLookAtTarget(null);
  }

  public playDialogue(dialogue: DialogueData): void {
    const id = `dialogue_${Date.now()}`;
    this.dialogueManager.addDialogue(id, dialogue);
    this.dialogueManager.playDialogue(id);
  }

  public stopDialogue(): void {
    this.dialogueManager.stop();
  }

  public playEffect(type: EffectType, position?: Vector3, intensity: number = 1): void {
    const effect: EffectData = {
      type,
      position,
      intensity,
    };
    this.effectSequencer.registerEffect(type, effect);
    this.effectSequencer.playEffect(type, position);
  }

  public setObjectiveStatus(objectiveId: string, status: ObjectiveStatus): void {
    switch (status) {
      case ObjectiveStatus.COMPLETED:
        this.objectiveDisplay.completeObjective(objectiveId);
        break;
      case ObjectiveStatus.FAILED:
        this.objectiveDisplay.failObjective(objectiveId);
        break;
      case ObjectiveStatus.ACTIVE:
      case ObjectiveStatus.UPDATED:
        this.objectiveDisplay.updateObjective(objectiveId, { status });
        break;
    }
  }

  public collectIntel(intelId: string): boolean {
    return this.intelCollector.collectIntel(intelId);
  }

  public unlockIntel(intelId: string): boolean {
    return this.intelCollector.unlockIntel(intelId);
  }

  public addTrigger(id: string, trigger: CinematicTrigger): void {
    this.triggerSystem.registerTrigger(id, trigger);
  }

  public fireTrigger(id: string): void {
    this.triggerSystem.fireTrigger(id, this.variables);
  }

  public getBriefing(): BriefingConfig | null {
    return this.briefingRenderer.getBriefing();
  }

  public getObjectives(): ObjectiveData[] {
    return this.objectiveDisplay.getAllObjectives();
  }

  public getActiveEffects(): ActiveEffect[] {
    return this.effectSequencer.getActiveEffects();
  }

  public getRecordedEvents(): CinematicEvent[] {
    return this.recorder.getRecordedEvents();
  }

  public exportCinematic(): string {
    return JSON.stringify(this.player.getEvents(), null, 2);
  }

  public importCinematic(json: string): boolean {
    try {
      const events = JSON.parse(json) as CinematicEvent[];
      this.loadCinematic(events);
      return true;
    } catch {
      return false;
    }
  }

  public dispose(): void {
    this.stop();
    this.letterboxConfig = null;
    this.pillarboxConfig = null;
    this.variables.clear();
    this.emit('disposed');
  }
}

export interface PillarboxConfig {
  left: number;
  right: number;
  color: Color;
  animated: boolean;
}

export default CinematicSystem;

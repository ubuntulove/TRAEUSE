export interface AnimationConfig {
  name: string;
  duration: number;
  loop: boolean;
  loopCount: number;
  speed: number;
  blendMode: BlendMode;
  easing: EasingFunction;
  events: AnimationEvent[];
  tracks: AnimationTrack[];
}

export enum BlendMode {
  REPLACE = 'replace',
  ADDITIVE = 'additive',
  MULTIPLY = 'multiply',
  SCREEN = 'screen',
  OVERLAY = 'overlay',
}

export type EasingFunction = (t: number) => number;

export interface AnimationEvent {
  name: string;
  time: number;
  data?: Record<string, unknown>;
  triggered: boolean;
}

export interface AnimationTrack {
  name: string;
  type: TrackType;
  target: string;
  keyframes: Keyframe[];
  additive: boolean;
  additiveReference?: string;
}

export enum TrackType {
  POSITION = 'position',
  ROTATION = 'rotation',
  SCALE = 'scale',
  COLOR = 'color',
  OPACITY = 'opacity',
  ATTRIBUTE = 'attribute',
  SPRITE = 'sprite',
  SOUND = 'sound',
  PARTICLE = 'particle',
  IK = 'ik',
  MORPH = 'morph',
}

export interface Keyframe {
  time: number;
  value: KeyframeValue;
  easing: EasingFunction;
  inTangent?: KeyframeValue;
  outTangent?: KeyframeValue;
  tangentMode: TangentMode;
}

export type KeyframeValue = number | Vec2 | Vec3 | Vec4 | Quaternion | Color | string | boolean;

export interface Vec2 {
  x: number;
  y: number;
}

export interface Vec3 {
  x: number;
  y: number;
  z: number;
}

export interface Vec4 {
  x: number;
  y: number;
  z: number;
  w: number;
}

export interface Quaternion {
  x: number;
  y: number;
  z: number;
  w: number;
}

export interface Color {
  r: number;
  g: number;
  b: number;
  a: number;
}

export enum TangentMode {
  LINEAR = 'linear',
  CONSTANT = 'constant',
  SMOOTH = 'smooth',
  FLAT = 'flat',
  BREAK = 'break',
  AUTO = 'auto',
  CLAMPED = 'clamped',
}

export class Easing {
  public static linear(t: number): number {
    return t;
  }

  public static easeInQuad(t: number): number {
    return t * t;
  }

  public static easeOutQuad(t: number): number {
    return t * (2 - t);
  }

  public static easeInOutQuad(t: number): number {
    return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
  }

  public static easeInCubic(t: number): number {
    return t * t * t;
  }

  public static easeOutCubic(t: number): number {
    return (--t) * t * t + 1;
  }

  public static easeInOutCubic(t: number): number {
    return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
  }

  public static easeInQuart(t: number): number {
    return t * t * t * t;
  }

  public static easeOutQuart(t: number): number {
    return 1 - (--t) * t * t * t;
  }

  public static easeInOutQuart(t: number): number {
    return t < 0.5 ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
  }

  public static easeInQuint(t: number): number {
    return t * t * t * t * t;
  }

  public static easeOutQuint(t: number): number {
    return 1 + (--t) * t * t * t * t;
  }

  public static easeInOutQuint(t: number): number {
    return t < 0.5 ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
  }

  public static easeInSine(t: number): number {
    return 1 - Math.cos(t * Math.PI / 2);
  }

  public static easeOutSine(t: number): number {
    return Math.sin(t * Math.PI / 2);
  }

  public static easeInOutSine(t: number): number {
    return -(Math.cos(Math.PI * t) - 1) / 2;
  }

  public static easeInExpo(t: number): number {
    return t === 0 ? 0 : Math.pow(2, 10 * t - 10);
  }

  public static easeOutExpo(t: number): number {
    return t === 1 ? 1 : 1 - Math.pow(2, -10 * t);
  }

  public static easeInOutExpo(t: number): number {
    return t === 0
      ? 0
      : t === 1
      ? 1
      : t < 0.5
      ? Math.pow(2, 20 * t - 10) / 2
      : (2 - Math.pow(2, -20 * t + 10)) / 2;
  }

  public static easeInCirc(t: number): number {
    return 1 - Math.sqrt(1 - Math.pow(t, 2));
  }

  public static easeOutCirc(t: number): number {
    return Math.sqrt(1 - Math.pow(t - 1, 2));
  }

  public static easeInOutCirc(t: number): number {
    return t < 0.5
      ? (1 - Math.sqrt(1 - Math.pow(2 * t, 2))) / 2
      : (Math.sqrt(1 - Math.pow(-2 * t + 2, 2)) + 1) / 2;
  }

  public static easeInBack(t: number): number {
    const c1 = 1.70158;
    const c3 = c1 + 1;
    return c3 * t * t * t - c1 * t * t;
  }

  public static easeOutBack(t: number): number {
    const c1 = 1.70158;
    const c3 = c1 + 1;
    return 1 + c3 * Math.pow(t - 1, 3) + c1 * Math.pow(t - 1, 2);
  }

  public static easeInOutBack(t: number): number {
    const c1 = 1.70158;
    const c2 = c1 * 1.525;
    return t < 0.5
      ? (Math.pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
      : (Math.pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
  }

  public static easeInElastic(t: number): number {
    const c4 = (2 * Math.PI) / 3;
    return t === 0
      ? 0
      : t === 1
      ? 1
      : -Math.pow(2, 10 * t - 10) * Math.sin((t * 10 - 10.75) * c4);
  }

  public static easeOutElastic(t: number): number {
    const c4 = (2 * Math.PI) / 3;
    return t === 0
      ? 0
      : t === 1
      ? 1
      : Math.pow(2, -10 * t) * Math.sin((t * 10 - 0.75) * c4) + 1;
  }

  public static easeInOutElastic(t: number): number {
    const c5 = (2 * Math.PI) / 4.5;
    return t === 0
      ? 0
      : t === 1
      ? 1
      : t < 0.5
      ? -(Math.pow(2, 20 * t - 10) * Math.sin((20 * t - 11.125) * c5)) / 2
      : (Math.pow(2, -20 * t + 10) * Math.sin((20 * t - 11.125) * c5)) / 2 + 1;
  }

  public static easeInBounce(t: number): number {
    return 1 - Easing.easeOutBounce(1 - t);
  }

  public static easeOutBounce(t: number): number {
    const n1 = 7.5625;
    const d1 = 2.75;
    if (t < 1 / d1) {
      return n1 * t * t;
    } else if (t < 2 / d1) {
      return n1 * (t -= 1.5 / d1) * t + 0.75;
    } else if (t < 2.5 / d1) {
      return n1 * (t -= 2.25 / d1) * t + 0.9375;
    } else {
      return n1 * (t -= 2.625 / d1) * t + 0.984375;
    }
  }

  public static easeInOutBounce(t: number): number {
    return t < 0.5
      ? (1 - Easing.easeOutBounce(1 - 2 * t)) / 2
      : (1 + Easing.easeOutBounce(2 * t - 1)) / 2;
  }

  public static bounce(t: number): number {
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
  }
}

export interface Bone {
  name: string;
  index: number;
  parentIndex: number;
  localPosition: Vec3;
  localRotation: Quaternion;
  localScale: Vec3;
  worldPosition: Vec3;
  worldRotation: Quaternion;
  worldScale: Vec3;
  length: number;
  children: number[];
}

export interface Skeleton {
  name: string;
  bones: Bone[];
  rootBoneIndex: number;
  hash: string;
}

export interface SkeletonAnimation extends AnimationConfig {
  skeleton: Skeleton;
  ikChains: IKChain[];
  socketList: Socket[];
  morphTargets: MorphTarget[];
}

export interface IKChain {
  name: string;
  bones: number[];
  targetBone: number;
  poleTarget?: number;
  iterations: number;
  tolerance: number;
  weight: number;
  enabled: boolean;
}

export interface Socket {
  name: string;
  boneIndex: number;
  localPosition: Vec3;
  localRotation: Quaternion;
}

export interface MorphTarget {
  name: string;
  vertices: Float32Array;
  normals: Float32Array;
  tangents: Float32Array;
  blendshapes: Blendshape[];
}

export interface Blendshape {
  name: string;
  weight: number;
  baseVertices?: Float32Array;
  deltaVertices?: Float32Array;
}

export interface SpriteAnimation {
  frames: SpriteFrame[];
  frameRate: number;
  loop: boolean;
  pingPong: boolean;
  direction: AnimationDirection;
}

export interface SpriteFrame {
  texture: string;
  rect: { x: number; y: number; width: number; height: number };
  pivot: Vec2;
  duration: number;
  offset?: Vec2;
  color?: Color;
  flipX?: boolean;
  flipY?: boolean;
  rotation?: number;
}

export enum AnimationDirection {
  FORWARD = 'forward',
  BACKWARD = 'backward',
  PING_PONG = 'ping_pong',
}

export class Animation {
  private config: AnimationConfig;
  private currentTime: number = 0;
  private lastTime: number = 0;
  private isPlaying: boolean = false;
  private isPaused: boolean = false;
  private speed: number = 1;
  private loopCount: number = 0;
  private weight: number = 1;
  private blendMode: BlendMode = BlendMode.REPLACE;
  private eventCallbacks: Map<string, Set<(event: AnimationEvent) => void>> = new Map();
  private trackValues: Map<string, KeyframeValue> = new Map();
  private triggeredEvents: Set<string> = new Set();

  constructor(config: AnimationConfig) {
    this.config = config;
  }

  public play(): void {
    this.isPlaying = true;
    this.isPaused = false;
    this.triggeredEvents.clear();
  }

  public pause(): void {
    this.isPaused = true;
  }

  public resume(): void {
    this.isPaused = false;
  }

  public stop(): void {
    this.isPlaying = false;
    this.isPaused = false;
    this.currentTime = 0;
    this.loopCount = 0;
    this.triggeredEvents.clear();
  }

  public restart(): void {
    this.stop();
    this.play();
  }

  public update(deltaTime: number): void {
    if (!this.isPlaying || this.isPaused) {
      return;
    }

    const previousTime = this.currentTime;
    this.currentTime += deltaTime * this.speed;

    this.checkEvents(previousTime, this.currentTime);

    if (this.currentTime >= this.config.duration) {
      if (this.config.loop) {
        this.loopCount++;
        this.currentTime = this.currentTime % this.config.duration;
      } else {
        this.currentTime = this.config.duration;
        this.isPlaying = false;
      }
    }
  }

  private checkEvents(previousTime: number, currentTime: number): void {
    for (const event of this.config.events) {
      if (!event.triggered) {
        if (previousTime < event.time && currentTime >= event.time) {
          event.triggered = true;
          this.triggerEvent(event);
        } else if (this.config.loop && previousTime > currentTime && currentTime >= event.time) {
          event.triggered = true;
          this.triggerEvent(event);
        }
      }
    }
  }

  private triggerEvent(event: AnimationEvent): void {
    const callbacks = this.eventCallbacks.get(event.name);
    if (callbacks) {
      callbacks.forEach(callback => callback(event));
    }
  }

  public on(eventName: string, callback: (event: AnimationEvent) => void): void {
    if (!this.eventCallbacks.has(eventName)) {
      this.eventCallbacks.set(eventName, new Set());
    }
    this.eventCallbacks.get(eventName)!.add(callback);
  }

  public off(eventName: string, callback: (event: AnimationEvent) => void): void {
    const callbacks = this.eventCallbacks.get(eventName);
    if (callbacks) {
      callbacks.delete(callback);
    }
  }

  public setSpeed(speed: number): void {
    this.speed = Math.max(0, speed);
  }

  public getSpeed(): number {
    return this.speed;
  }

  public setWeight(weight: number): void {
    this.weight = Math.max(0, Math.min(1, weight));
  }

  public getWeight(): number {
    return this.weight;
  }

  public setBlendMode(mode: BlendMode): void {
    this.blendMode = mode;
  }

  public getBlendMode(): BlendMode {
    return this.blendMode;
  }

  public seek(time: number): void {
    const previousTime = this.currentTime;
    this.currentTime = Math.max(0, Math.min(this.config.duration, time));
    if (previousTime !== this.currentTime) {
      this.checkEvents(previousTime, this.currentTime);
    }
  }

  public getCurrentTime(): number {
    return this.currentTime;
  }

  public getNormalizedTime(): number {
    return this.config.duration > 0 ? this.currentTime / this.config.duration : 0;
  }

  public isActive(): boolean {
    return this.isPlaying && !this.isPaused;
  }

  public getDuration(): number {
    return this.config.duration;
  }

  public getLoopCount(): number {
    return this.loopCount;
  }

  public getName(): string {
    return this.config.name;
  }

  public getTrackValue(trackName: string): KeyframeValue | undefined {
    return this.trackValues.get(trackName);
  }

  public evaluate(): Map<string, KeyframeValue> {
    const result = new Map<string, KeyframeValue>();

    for (const track of this.config.tracks) {
      const value = this.interpolateTrack(track);
      result.set(track.name, value);
    }

    return result;
  }

  private interpolateTrack(track: AnimationTrack): KeyframeValue {
    const keyframes = track.keyframes;

    if (keyframes.length === 0) {
      return 0;
    }

    if (keyframes.length === 1) {
      return keyframes[0].value;
    }

    const time = this.currentTime;

    let startIndex = 0;
    for (let i = 0; i < keyframes.length - 1; i++) {
      if (time >= keyframes[i].time && time <= keyframes[i + 1].time) {
        startIndex = i;
        break;
      }
      if (i === keyframes.length - 2) {
        startIndex = i;
      }
    }

    const startKeyframe = keyframes[startIndex];
    const endKeyframe = keyframes[startIndex + 1];

    const duration = endKeyframe.time - startKeyframe.time;
    const progress = duration > 0 ? (time - startKeyframe.time) / duration : 0;
    const easedProgress = startKeyframe.easing(progress);

    return this.interpolateValue(startKeyframe.value, endKeyframe.value, easedProgress, track.type);
  }

  private interpolateValue(
    start: KeyframeValue,
    end: KeyframeValue,
    progress: number,
    type: TrackType
  ): KeyframeValue {
    if (typeof start === 'number' && typeof end === 'number') {
      return this.interpolateNumber(start, end, progress);
    }

    if (this.isVec2(start) && this.isVec2(end)) {
      return this.interpolateVec2(start, end, progress);
    }

    if (this.isVec3(start) && this.isVec3(end)) {
      return this.interpolateVec3(start, end, progress);
    }

    if (this.isVec4(start) && this.isVec4(end)) {
      return this.interpolateVec4(start, end, progress);
    }

    if (this.isQuaternion(start) && this.isQuaternion(end)) {
      return this.interpolateQuaternion(start, end, progress);
    }

    if (this.isColor(start) && this.isColor(end)) {
      return this.interpolateColor(start, end, progress);
    }

    if (typeof start === 'boolean' && typeof end === 'boolean') {
      return progress < 0.5 ? start : end;
    }

    if (typeof start === 'string' && typeof end === 'string') {
      return progress < 0.5 ? start : end;
    }

    return end;
  }

  private interpolateNumber(start: number, end: number, progress: number): number {
    return start + (end - start) * progress;
  }

  private interpolateVec2(start: Vec2, end: Vec2, progress: number): Vec2 {
    return {
      x: start.x + (end.x - start.x) * progress,
      y: start.y + (end.y - start.y) * progress,
    };
  }

  private interpolateVec3(start: Vec3, end: Vec3, progress: number): Vec3 {
    return {
      x: start.x + (end.x - start.x) * progress,
      y: start.y + (end.y - start.y) * progress,
      z: start.z + (end.z - start.z) * progress,
    };
  }

  private interpolateVec4(start: Vec4, end: Vec4, progress: number): Vec4 {
    return {
      x: start.x + (end.x - start.x) * progress,
      y: start.y + (end.y - start.y) * progress,
      z: start.z + (end.z - start.z) * progress,
      w: start.w + (end.w - start.w) * progress,
    };
  }

  private interpolateQuaternion(start: Quaternion, end: Quaternion, progress: number): Quaternion {
    let dot = start.x * end.x + start.y * end.y + start.z * end.z + start.w * end.w;
    let qx: number, qy: number, qz: number, qw: number;

    if (dot < 0) {
      dot = -dot;
      qx = -end.x;
      qy = -end.y;
      qz = -end.z;
      qw = -end.w;
    } else {
      qx = end.x;
      qy = end.y;
      qz = end.z;
      qw = end.w;
    }

    const result = {
      x: start.x + (qx - start.x) * progress,
      y: start.y + (qy - start.y) * progress,
      z: start.z + (qz - start.z) * progress,
      w: start.w + (qw - start.w) * progress,
    };

    const length = Math.sqrt(result.x * result.x + result.y * result.y + result.z * result.z + result.w * result.w);
    return {
      x: result.x / length,
      y: result.y / length,
      z: result.z / length,
      w: result.w / length,
    };
  }

  private interpolateColor(start: Color, end: Color, progress: number): Color {
    return {
      r: start.r + (end.r - start.r) * progress,
      g: start.g + (end.g - start.g) * progress,
      b: start.b + (end.b - start.b) * progress,
      a: start.a + (end.a - start.a) * progress,
    };
  }

  private isVec2(value: KeyframeValue): value is Vec2 {
    return typeof value === 'object' && value !== null && 'x' in value && 'y' in value && !('z' in value);
  }

  private isVec3(value: KeyframeValue): value is Vec3 {
    return typeof value === 'object' && value !== null && 'x' in value && 'y' in value && 'z' in value;
  }

  private isVec4(value: KeyframeValue): value is Vec4 {
    return typeof value === 'object' && value !== null && 'x' in value && 'y' in value && 'z' in value && 'w' in value;
  }

  private isQuaternion(value: KeyframeValue): value is Quaternion {
    return typeof value === 'object' && value !== null && 'x' in value && 'y' in value && 'z' in value && 'w' in value;
  }

  private isColor(value: KeyframeValue): value is Color {
    return typeof value === 'object' && value !== null && 'r' in value && 'g' in value && 'b' in value && 'a' in value;
  }

  public clone(): Animation {
    const clonedConfig = JSON.parse(JSON.stringify(this.config));
    const cloned = new Animation(clonedConfig);
    cloned.setSpeed(this.speed);
    cloned.setWeight(this.weight);
    cloned.setBlendMode(this.blendMode);
    return cloned;
  }

  public destroy(): void {
    this.stop();
    this.eventCallbacks.clear();
    this.trackValues.clear();
    this.triggeredEvents.clear();
  }
}

export class AnimationBlender {
  private animations: Map<string, Animation> = new Map();
  private activeAnimations: Set<string> = new Set();
  private blendedValues: Map<string, KeyframeValue> = new Map();
  private blendTimes: Map<string, number> = new Map();
  private defaultBlendTime: number = 0.2;

  public addAnimation(name: string, animation: Animation): void {
    this.animations.set(name, animation);
  }

  public removeAnimation(name: string): void {
    this.animations.delete(name);
    this.activeAnimations.delete(name);
    this.blendedValues.delete(name);
    this.blendTimes.delete(name);
  }

  public play(animationName: string, fadeLength?: number): void {
    const animation = this.animations.get(animationName);
    if (!animation) {
      return;
    }

    if (fadeLength !== undefined && fadeLength > 0) {
      this.blendTimes.set(animationName, fadeLength);
    } else {
      this.blendTimes.set(animationName, this.defaultBlendTime);
    }

    this.activeAnimations.add(animationName);
    animation.play();
  }

  public crossFade(animationName: string, fadeLength?: number, freezeLastFrame: boolean = false): void {
    const previousAnimations = new Set(this.activeAnimations);
    this.play(animationName, fadeLength);

    previousAnimations.forEach(name => {
      if (name !== animationName) {
        this.stop(name, freezeLastFrame);
      }
    });
  }

  public blend(animationName: string, weight: number, fadeLength?: number): void {
    const animation = this.animations.get(animationName);
    if (!animation) {
      return;
    }

    if (fadeLength !== undefined && fadeLength > 0) {
      this.blendTimes.set(animationName, fadeLength);
    }

    this.activeAnimations.add(animationName);
    animation.setWeight(weight);
    if (!animation.isActive()) {
      animation.play();
    }
  }

  public stop(animationName: string, freezeLastFrame: boolean = false): void {
    const animation = this.animations.get(animationName);
    if (animation) {
      if (freezeLastFrame) {
        animation.pause();
      } else {
        animation.stop();
      }
      this.activeAnimations.delete(animationName);
    }
  }

  public stopAll(freezeLastFrame: boolean = false): void {
    this.activeAnimations.forEach(name => {
      this.stop(name, freezeLastFrame);
    });
    this.activeAnimations.clear();
  }

  public update(deltaTime: number): void {
    this.animations.forEach((animation, name) => {
      if (this.activeAnimations.has(name)) {
        animation.update(deltaTime);
      }
    });

    this.blendedValues.clear();

    for (const [name, animation] of this.animations) {
      if (this.activeAnimations.has(name)) {
        const values = animation.evaluate();
        const weight = animation.getWeight();
        const blendMode = animation.getBlendMode();

        values.forEach((value, trackName) => {
          const key = `${name}:${trackName}`;
          if (!this.blendedValues.has(key)) {
            this.blendedValues.set(key, value);
          } else {
            const current = this.blendedValues.get(key)!;
            this.blendedValues.set(key, this.blendValues(current, value, weight, blendMode));
          }
        });
      }
    }
  }

  private blendValues(
    base: KeyframeValue,
    add: KeyframeValue,
    weight: number,
    mode: BlendMode
  ): KeyframeValue {
    switch (mode) {
      case BlendMode.REPLACE:
        if (typeof base === 'number' && typeof add === 'number') {
          return base * (1 - weight) + add * weight;
        }
        if (this.isColor(base) && this.isColor(add)) {
          return this.blendColors(base, add, weight);
        }
        return weight >= 0.5 ? add : base;

      case BlendMode.ADDITIVE:
        if (typeof base === 'number' && typeof add === 'number') {
          return base + add * weight;
        }
        if (this.isVec3(base) && this.isVec3(add)) {
          return {
            x: base.x + add.x * weight,
            y: base.y + add.y * weight,
            z: base.z + add.z * weight,
          };
        }
        return add;

      case BlendMode.MULTIPLY:
        if (typeof base === 'number' && typeof add === 'number') {
          return base * (1 + (add - 1) * weight);
        }
        return add;

      case BlendMode.SCREEN:
        if (typeof base === 'number' && typeof add === 'number') {
          return 1 - (1 - base) * (1 - add * weight);
        }
        return add;

      case BlendMode.OVERLAY:
        return weight >= 0.5 ? add : base;

      default:
        return add;
    }
  }

  private blendColors(base: Color, add: Color, weight: number): Color {
    return {
      r: base.r * (1 - weight) + add.r * weight,
      g: base.g * (1 - weight) + add.g * weight,
      b: base.b * (1 - weight) + add.b * weight,
      a: base.a * (1 - weight) + add.a * weight,
    };
  }

  private isColor(value: KeyframeValue): value is Color {
    return typeof value === 'object' && value !== null && 'r' in value && 'g' in value && 'b' in value && 'a' in value;
  }

  private isVec3(value: KeyframeValue): value is Vec3 {
    return typeof value === 'object' && value !== null && 'x' in value && 'y' in value && 'z' in value;
  }

  public getAnimation(name: string): Animation | undefined {
    return this.animations.get(name);
  }

  public getBlendedValue(trackName: string): KeyframeValue | undefined {
    for (const [key, value] of this.blendedValues) {
      if (key.endsWith(`:${trackName}`)) {
        return value;
      }
    }
    return undefined;
  }

  public getAllBlendedValues(): Map<string, KeyframeValue> {
    return new Map(this.blendedValues);
  }

  public getActiveAnimations(): string[] {
    return Array.from(this.activeAnimations);
  }

  public hasAnimation(name: string): boolean {
    return this.animations.has(name);
  }

  public isPlaying(animationName: string): boolean {
    const animation = this.animations.get(animationName);
    return animation ? animation.isActive() : false;
  }

  public getAnimationTime(animationName: string): number {
    const animation = this.animations.get(animationName);
    return animation ? animation.getCurrentTime() : 0;
  }

  public seekAnimation(animationName: string, time: number): void {
    const animation = this.animations.get(animationName);
    if (animation) {
      animation.seek(time);
    }
  }

  public destroy(): void {
    this.animations.forEach(animation => animation.destroy());
    this.animations.clear();
    this.activeAnimations.clear();
    this.blendedValues.clear();
    this.blendTimes.clear();
  }
}

export class SpriteAnimator {
  private spriteAnimation: SpriteAnimation;
  private currentFrame: number = 0;
  private currentTime: number = 0;
  private isPlaying: boolean = false;
  private isPaused: boolean = false;
  private loopCount: number = 0;
  private direction: number = 1;
  private onFrameChange?: (frame: SpriteFrame) => void;
  private onAnimationEnd?: () => void;

  constructor(spriteAnimation: SpriteAnimation, onFrameChange?: (frame: SpriteFrame) => void) {
    this.spriteAnimation = spriteAnimation;
    this.onFrameChange = onFrameChange;
  }

  public play(): void {
    this.isPlaying = true;
    this.isPaused = false;
  }

  public pause(): void {
    this.isPaused = true;
  }

  public resume(): void {
    this.isPaused = false;
  }

  public stop(): void {
    this.isPlaying = false;
    this.isPaused = false;
    this.currentFrame = 0;
    this.currentTime = 0;
    this.loopCount = 0;
    this.direction = 1;
  }

  public restart(): void {
    this.stop();
    this.play();
  }

  public update(deltaTime: number): void {
    if (!this.isPlaying || this.isPaused) {
      return;
    }

    this.currentTime += deltaTime;

    const frame = this.getCurrentSpriteFrame();
    const frameDuration = frame.duration / 1000;

    if (this.currentTime >= frameDuration) {
      this.advanceFrame();
    }
  }

  private advanceFrame(): void {
    const frameCount = this.spriteAnimation.frames.length;
    this.currentTime = 0;

    if (this.direction > 0) {
      this.currentFrame++;
      if (this.currentFrame >= frameCount) {
        if (this.spriteAnimation.loop) {
          if (this.spriteAnimation.pingPong) {
            this.direction = -1;
            this.currentFrame = frameCount - 2;
          } else {
            this.currentFrame = 0;
          }
          this.loopCount++;
        } else {
          this.currentFrame = frameCount - 1;
          this.isPlaying = false;
          this.onAnimationEnd?.();
        }
      }
    } else {
      this.currentFrame--;
      if (this.currentFrame < 0) {
        if (this.spriteAnimation.loop) {
          if (this.spriteAnimation.pingPong) {
            this.direction = 1;
            this.currentFrame = 1;
          } else {
            this.currentFrame = frameCount - 1;
          }
          this.loopCount++;
        } else {
          this.currentFrame = 0;
          this.isPlaying = false;
          this.onAnimationEnd?.();
        }
      }
    }

    this.onFrameChange?.(this.getCurrentSpriteFrame());
  }

  public getCurrentSpriteFrame(): SpriteFrame {
    return this.spriteAnimation.frames[Math.max(0, Math.min(this.currentFrame, this.spriteAnimation.frames.length - 1))];
  }

  public setFrame(frameIndex: number): void {
    this.currentFrame = Math.max(0, Math.min(frameIndex, this.spriteAnimation.frames.length - 1));
    this.currentTime = 0;
    this.onFrameChange?.(this.getCurrentSpriteFrame());
  }

  public isActive(): boolean {
    return this.isPlaying && !this.isPaused;
  }

  public getCurrentFrameIndex(): number {
    return this.currentFrame;
  }

  public getLoopCount(): number {
    return this.loopCount;
  }

  public setDirection(direction: AnimationDirection): void {
    switch (direction) {
      case AnimationDirection.FORWARD:
        this.direction = 1;
        break;
      case AnimationDirection.BACKWARD:
        this.direction = -1;
        break;
      case AnimationDirection.PING_PONG:
        this.direction = 1;
        break;
    }
  }

  public onAnimationComplete(callback: () => void): void {
    this.onAnimationEnd = callback;
  }
}

export class SkeletonAnimator {
  private skeleton: Skeleton;
  private animations: Map<string, Animation> = new Map();
  private boneMatrices: Float32Array;
  private boneWorldMatrices: Map<number, Float32Array> = new Map();
  private socketMatrices: Map<string, Float32Array> = new Map();
  private activeAnimations: Map<string, Animation> = new Map();
  private currentPose: Map<string, KeyframeValue> = new Map();
  private lastUpdateTime: number = 0;

  constructor(skeleton: Skeleton) {
    this.skeleton = skeleton;
    this.boneMatrices = new Float32Array(skeleton.bones.length * 16);
    this.initializeWorldMatrices();
  }

  private initializeWorldMatrices(): void {
    for (const bone of this.skeleton.bones) {
      this.boneWorldMatrices.set(bone.index, new Float32Array(16));
    }
  }

  public addAnimation(name: string, animation: Animation): void {
    this.animations.set(name, animation);
  }

  public removeAnimation(name: string): void {
    this.animations.delete(name);
    this.activeAnimations.delete(name);
  }

  public play(animationName: string, layer: number = 0): void {
    const animation = this.animations.get(animationName);
    if (!animation) {
      return;
    }

    this.activeAnimations.set(animationName, animation);
    animation.play();
  }

  public crossFade(fromAnimation: string, toAnimation: string, fadeTime: number = 0.2): void {
    const from = this.animations.get(fromAnimation);
    const to = this.animations.get(toAnimation);

    if (!from || !to) {
      return;
    }

    this.activeAnimations.delete(fromAnimation);
    from.stop(false);

    this.activeAnimations.set(toAnimation, to);
    to.play();
  }

  public update(deltaTime: number): void {
    const currentTime = this.lastUpdateTime + deltaTime;

    this.currentPose.clear();

    this.activeAnimations.forEach(animation => {
      animation.update(deltaTime);
      const values = animation.evaluate();
      values.forEach((value, trackName) => {
        this.currentPose.set(trackName, value);
      });
    });

    this.updateBoneMatrices();
    this.updateSocketMatrices();

    this.lastUpdateTime = currentTime;
  }

  private updateBoneMatrices(): void {
    for (const bone of this.skeleton.bones) {
      this.calculateBoneMatrix(bone);
    }
  }

  private calculateBoneMatrix(bone: Bone): void {
    const position = this.getPoseValue(`${bone.name}.position`, bone.localPosition);
    const rotation = this.getPoseValue(`${bone.name}.rotation`, bone.localRotation);
    const scale = this.getPoseValue(`${bone.name}.scale`, bone.localScale);

    const localMatrix = this.createTransformMatrix(position as Vec3, rotation as Quaternion, scale as Vec3);
    const matrixIndex = bone.index * 16;

    if (bone.parentIndex >= 0) {
      const parentMatrix = this.boneWorldMatrices.get(bone.parentIndex)!;
      this.multiplyMatrices(parentMatrix, localMatrix, this.boneMatrices, matrixIndex);
    } else {
      this.copyMatrix(localMatrix, this.boneMatrices, matrixIndex);
    }

    const worldMatrix = this.boneWorldMatrices.get(bone.index)!;
    this.copyMatrix(this.boneMatrices, worldMatrix, 0);
  }

  private getPoseValue(name: string, defaultValue: KeyframeValue): KeyframeValue {
    if (this.currentPose.has(name)) {
      return this.currentPose.get(name)!;
    }
    return defaultValue;
  }

  private createTransformMatrix(position: Vec3, rotation: Quaternion, scale: Vec3): Float32Array {
    const matrix = new Float32Array(16);
    const cx = Math.cos(rotation.x);
    const sx = Math.sin(rotation.x);
    const cy = Math.cos(rotation.y);
    const sy = Math.sin(rotation.y);
    const cz = Math.cos(rotation.z);
    const sz = Math.sin(rotation.z);
    const cw = Math.cos(rotation.w);
    const sw = Math.sin(rotation.w);

    matrix[0] = (1 - 2 * (rotation.y * rotation.y + rotation.z * rotation.z)) * scale.x;
    matrix[1] = (rotation.x * rotation.y + rotation.z * rotation.w) * 2 * scale.x;
    matrix[2] = (rotation.z * rotation.x - rotation.x * rotation.w) * 2 * scale.x;
    matrix[3] = 0;

    matrix[4] = (rotation.x * rotation.y - rotation.z * rotation.w) * 2 * scale.y;
    matrix[5] = (1 - 2 * (rotation.z * rotation.z + rotation.x * rotation.x)) * scale.y;
    matrix[6] = (rotation.y * rotation.z + rotation.x * rotation.w) * 2 * scale.y;
    matrix[7] = 0;

    matrix[8] = (rotation.z * rotation.x + rotation.x * rotation.y * sw) * 2 * scale.z;
    matrix[9] = (rotation.y * rotation.z - rotation.x * rotation.x * sw) * 2 * scale.z;
    matrix[10] = (cw - 2 * (rotation.x * rotation.x + rotation.y * rotation.y)) * scale.z;
    matrix[11] = 0;

    matrix[12] = position.x;
    matrix[13] = position.y;
    matrix[14] = position.z;
    matrix[15] = 1;

    return matrix;
  }

  private multiplyMatrices(a: Float32Array, b: Float32Array, result: Float32Array, offset: number): void {
    for (let i = 0; i < 4; i++) {
      for (let j = 0; j < 4; j++) {
        let sum = 0;
        for (let k = 0; k < 4; k++) {
          sum += a[i + k * 4] * b[k + j * 4];
        }
        result[offset + i + j * 4] = sum;
      }
    }
  }

  private copyMatrix(source: Float32Array, dest: Float32Array, offset: number): void {
    for (let i = 0; i < 16; i++) {
      dest[offset + i] = source[i];
    }
  }

  private updateSocketMatrices(): void {
  }

  public getBoneMatrix(boneIndex: number): Float32Array | undefined {
    return this.boneWorldMatrices.get(boneIndex);
  }

  public getBoneWorldPosition(boneIndex: number): Vec3 {
    const matrix = this.boneWorldMatrices.get(boneIndex);
    if (matrix) {
      return { x: matrix[12], y: matrix[13], z: matrix[14] };
    }
    const bone = this.skeleton.bones.find(b => b.index === boneIndex);
    return bone ? bone.worldPosition : { x: 0, y: 0, z: 0 };
  }

  public getBoneWorldRotation(boneIndex: number): Quaternion {
    const bone = this.skeleton.bones.find(b => b.index === boneIndex);
    return bone ? bone.worldRotation : { x: 0, y: 0, z: 0, w: 1 };
  }

  public getSocketMatrix(socketName: string): Float32Array | undefined {
    return this.socketMatrices.get(socketName);
  }

  public getSkeleton(): Skeleton {
    return this.skeleton;
  }

  public destroy(): void {
    this.animations.forEach(animation => animation.destroy());
    this.animations.clear();
    this.activeAnimations.clear();
    this.currentPose.clear();
    this.boneWorldMatrices.clear();
    this.socketMatrices.clear();
  }
}

export class AnimationManager {
  private animators: Map<string, AnimationBlender | SpriteAnimator | SkeletonAnimator> = new Map();
  private globalTimeScale: number = 1;
  private isPaused: boolean = false;
  private totalAnimations: number = 0;

  public addAnimator(id: string, animator: AnimationBlender | SpriteAnimator | SkeletonAnimator): void {
    this.animators.set(id, animator);
    this.totalAnimations++;
  }

  public removeAnimator(id: string): void {
    const animator = this.animators.get(id);
    if (animator) {
      if ('destroy' in animator) {
        (animator as AnimationBlender).destroy();
      }
      this.animators.delete(id);
      this.totalAnimations--;
    }
  }

  public getAnimator(id: string): AnimationBlender | SpriteAnimator | SkeletonAnimator | undefined {
    return this.animators.get(id);
  }

  public update(deltaTime: number): void {
    if (this.isPaused) {
      return;
    }

    const scaledDelta = deltaTime * this.globalTimeScale;

    this.animators.forEach(animator => {
      animator.update(scaledDelta);
    });
  }

  public pause(): void {
    this.isPaused = true;
  }

  public resume(): void {
    this.isPaused = false;
  }

  public setGlobalTimeScale(scale: number): void {
    this.globalTimeScale = Math.max(0, scale);
  }

  public getGlobalTimeScale(): number {
    return this.globalTimeScale;
  }

  public stopAll(): void {
    this.animators.forEach(animator => {
      if ('stopAll' in animator) {
        (animator as AnimationBlender).stopAll(false);
      } else if ('stop' in animator) {
        (animator as SpriteAnimator).stop();
      }
    });
  }

  public getAnimatorCount(): number {
    return this.animators.size;
  }

  public getTotalAnimationCount(): number {
    return this.totalAnimations;
  }

  public destroy(): void {
    this.animators.forEach(animator => {
      if ('destroy' in animator) {
        (animator as AnimationBlender).destroy();
      }
    });
    this.animators.clear();
    this.totalAnimations = 0;
  }
}

export const createAnimationManager = (): AnimationManager => {
  return new AnimationManager();
};

export default Animation;

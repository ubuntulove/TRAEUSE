export interface VisualEffect {
  id: string;
  type: EffectType;
  name: string;
  description: string;
  position: Vector3;
  rotation: Vector3;
  scale: Vector3;
  color: Color;
  intensity: number;
  duration: number;
  elapsed: number;
  loop: boolean;
  speed: number;
  blendMode: BlendMode;
  sortingOrder: number;
  visible: boolean;
  active: boolean;
  children: VisualEffect[];
  parent?: string;
  properties: Record<string, unknown>;
  emitters: EffectEmitter[];
  shaders: string[];
  audio?: string;
  onComplete?: () => void;
}

export type EffectType =
  | 'particle'
  | 'beam'
  | 'ring'
  | 'sphere'
  | 'cylinder'
  | 'cone'
  | 'decal'
  | 'light'
  | 'trail'
  | 'shockwave'
  | 'vine'
  | 'shield'
  | 'explosion'
  | 'muzzle_flash'
  | 'smoke'
  | 'fire'
  | 'ice'
  | 'electricity'
  | 'portal'
  | 'summon'
  | 'teleport'
  | 'aurora'
  | 'nebula'
  | 'shatter'
  | 'melt'
  | 'freeze'
  | 'burn'
  | 'poison'
  | 'heal'
  | 'buff'
  | 'debuff'
  | 'custom';

export type BlendMode =
  | 'additive'
  | 'alpha'
  | 'multiply'
  | 'screen'
  | 'overlay'
  | 'transparent'
  | 'opaque';

export interface Vector3 {
  x: number;
  y: number;
  z: number;
}

export interface Color {
  r: number;
  g: number;
  b: number;
  a: number;
}

export interface EffectEmitter {
  id: string;
  type: EmitterType;
  position: Vector3;
  emissionRate: number;
  lifetime: number;
  lifetimeVariation: number;
  initialVelocity: Vector3;
  velocityVariation: Vector3;
  acceleration: Vector3;
  size: number;
  sizeVariation: number;
  sizeOverLifetime: AnimationCurve;
  colorOverLifetime: ColorGradient;
  alphaOverLifetime: AnimationCurve;
  rotationSpeed: number;
  rotationSpeedVariation: number;
  angularVelocity: Vector3;
  shape: EmitterShape;
  renderSettings: RenderSettings;
  particles: EffectParticle[];
  active: boolean;
  emitCount: number;
  maxParticles: number;
}

export type EmitterType =
  | 'continuous'
  | 'burst'
  | 'oneshot'
  | 'wave'
  | 'spiral'
  | 'cone'
  | 'sphere'
  | 'cylinder'
  | 'box';

export interface EmitterShape {
  type: 'point' | 'sphere' | 'hemisphere' | 'cone' | 'box' | 'cylinder' | 'ring';
  radius: number;
  radiusThickness: number;
  angle: number;
  length: number;
  boxSize: Vector3;
  arc: number;
  arcSpeed: number;
  thickness: number;
}

export interface RenderSettings {
  material: string;
  shader: string;
  texture: string;
  atlas: TextureAtlas;
  billboard: BillboardMode;
  sortingOrder: number;
  castShadows: boolean;
  receiveShadows: boolean;
  lightmapIndex: number;
  lightmapScaleOffset: Vector3;
}

export type BillboardMode =
  | 'none'
  | 'y'
  | 'full'
  | 'velocity'
  | 'custom';

export interface TextureAtlas {
  texture: string;
  columns: number;
  rows: number;
  frameCount: number;
  frameRate: number;
  loop: boolean;
  currentFrame: number;
}

export interface EffectParticle {
  id: string;
  position: Vector3;
  velocity: Vector3;
  acceleration: Vector3;
  rotation: Vector3;
  angularVelocity: Vector3;
  size: number;
  color: Color;
  alpha: number;
  lifetime: number;
  age: number;
  mass: number;
  drag: number;
  spin: number;
  stretch: number;
  sprite?: string;
  atlasFrame: number;
  mesh?: string;
  trail?: TrailData;
  physics?: ParticlePhysics;
}

export interface TrailData {
  width: number;
  widthOverTrail: AnimationCurve;
  colorOverTrail: ColorGradient;
  lifetime: number;
  minVertexDistance: number;
  generateDistance: number;
  vertices: TrailVertex[];
}

export interface TrailVertex {
  position: Vector3;
  time: number;
  color: Color;
  alpha: number;
  width: number;
}

export interface ParticlePhysics {
  gravity: number;
  wind: Vector3;
  turbulence: TurbulenceSettings;
  collision: CollisionSettings;
  forces: Force[];
}

export interface TurbulenceSettings {
  enabled: boolean;
  strength: number;
  frequency: number;
  octaves: number;
  persistence: number;
  scrollSpeed: number;
  offset: Vector3;
}

export interface CollisionSettings {
  enabled: boolean;
  type: 'plane' | 'sphere' | 'box' | 'mesh';
  planes: CollisionPlane[];
  spheres: CollisionSphere[];
  boxes: CollisionBox[];
  bounce: number;
  lifetimeLoss: number;
  sendCollisionMessage: boolean;
}

export interface CollisionPlane {
  normal: Vector3;
  distance: number;
  active: boolean;
}

export interface CollisionSphere {
  center: Vector3;
  radius: number;
  active: boolean;
}

export interface CollisionBox {
  center: Vector3;
  size: Vector3;
  active: boolean;
}

export interface Force {
  type: ForceType;
  direction: Vector3;
  strength: number;
  position: Vector3;
  radius: number;
  falloff: FalloffType;
}

export type ForceType =
  | 'constant'
  | 'attractor'
  | 'repulsor'
  | 'vortex'
  | 'wind'
  | 'gravity'
  | 'custom';

export type FalloffType =
  | 'none'
  | 'linear'
  | 'quadratic'
  | 'exponential';

export interface AnimationCurve {
  keys: CurveKey[];
  preWrap: WrapMode;
  postWrap: WrapMode;
  length: number;
}

export interface CurveKey {
  time: number;
  value: number;
  inTangent: number;
  outTangent: number;
  inWeight: number;
  outWeight: number;
  weightedMode: WeightedMode;
}

export type WeightedMode = 'none' | 'in' | 'out' | 'both';

export type WrapMode =
  | 'clamp'
  | 'loop'
  | 'ping_pong'
  | 'clamp_forever'
  | 'internal_default';

export interface ColorGradient {
  keys: GradientKey[];
  mode: GradientMode;
}

export interface GradientKey {
  time: number;
  color: Color;
  alpha: number;
  inTangent: Color;
  outTangent: Color;
}

export type GradientMode = 'blend' | 'fixed';

export interface EffectLayer {
  id: string;
  name: string;
  order: number;
  effects: VisualEffect[];
  enabled: boolean;
  visible: boolean;
  locked: boolean;
  color: Color;
  blendMode: BlendMode;
}

export interface EffectSequence {
  id: string;
  name: string;
  steps: EffectSequenceStep[];
  currentStep: number;
  loop: boolean;
  playing: boolean;
  duration: number;
  elapsed: number;
  onStepComplete?: (step: EffectSequenceStep) => void;
  onComplete?: () => void;
}

export interface EffectSequenceStep {
  time: number;
  effect: VisualEffect;
  action: 'play' | 'pause' | 'stop' | 'scale' | 'move' | 'rotate' | 'fade' | 'color';
  parameters: Record<string, unknown>;
  duration: number;
}

export class VisualEffectSystem {
  private effects: Map<string, VisualEffect> = new Map();
  private layers: Map<string, EffectLayer> = new Map();
  private emitters: Map<string, EffectEmitter> = new Map();
  private sequences: Map<string, EffectSequence> = new Map();
  private activeParticles: Map<string, EffectParticle[]> = new Map();
  private particlePool: EffectParticle[] = [];
  private maxPoolSize: number = 10000;
  private globalSpeed: number = 1.0;
  private globalIntensity: number = 1.0;
  private masterEnabled: boolean = true;
  private paused: boolean = false;
  private currentTime: number = 0;
  private lastUpdateTime: number = 0;
  private deltaTime: number = 0;
  private renderCallbacks: ((effects: VisualEffect[]) => void)[] = [];
  private audioCallbacks: Map<string, (effect: VisualEffect) => void> = new Map();
  private cache: EffectCache;
  private statistics: EffectStatistics;

  constructor() {
    this.cache = new EffectCache();
    this.statistics = {
      totalParticles: 0,
      activeEffects: 0,
      activeEmitters: 0,
      totalEmitted: 0,
      frameTime: 0,
      memoryUsage: 0
    };

    this.initializeDefaultLayers();
  }

  private initializeDefaultLayers(): void {
    this.createLayer({
      id: 'background',
      name: 'Background Effects',
      order: 0,
      effects: [],
      enabled: true,
      visible: true,
      locked: false,
      color: { r: 1, g: 1, b: 1, a: 1 },
      blendMode: 'additive'
    });

    this.createLayer({
      id: 'default',
      name: 'Default Effects',
      order: 1,
      effects: [],
      enabled: true,
      visible: true,
      locked: false,
      color: { r: 1, g: 1, b: 1, a: 1 },
      blendMode: 'additive'
    });

    this.createLayer({
      id: 'foreground',
      name: 'Foreground Effects',
      order: 2,
      effects: [],
      enabled: true,
      visible: true,
      locked: false,
      color: { r: 1, g: 1, b: 1, a: 1 },
      blendMode: 'additive'
    });

    this.createLayer({
      id: 'ui',
      name: 'UI Effects',
      order: 3,
      effects: [],
      enabled: true,
      visible: true,
      locked: false,
      color: { r: 1, g: 1, b: 1, a: 1 },
      blendMode: 'alpha'
    });
  }

  createLayer(config: Partial<EffectLayer> & { id: string; name: string }): EffectLayer {
    const layer: EffectLayer = {
      id: config.id,
      name: config.name,
      order: config.order ?? 0,
      effects: config.effects ?? [],
      enabled: config.enabled ?? true,
      visible: config.visible ?? true,
      locked: config.locked ?? false,
      color: config.color ?? { r: 1, g: 1, b: 1, a: 1 },
      blendMode: config.blendMode ?? 'additive'
    };

    this.layers.set(layer.id, layer);
    return layer;
  }

  getLayer(id: string): EffectLayer | undefined {
    return this.layers.get(id);
  }

  getAllLayers(): EffectLayer[] {
    return Array.from(this.layers.values()).sort((a, b) => a.order - b.order);
  }

  createEffect(config: Partial<VisualEffect> & { type: EffectType; name: string }): VisualEffect {
    const effect: VisualEffect = {
      id: config.id || `effect_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      type: config.type,
      name: config.name,
      description: config.description || '',
      position: config.position || { x: 0, y: 0, z: 0 },
      rotation: config.rotation || { x: 0, y: 0, z: 0 },
      scale: config.scale || { x: 1, y: 1, z: 1 },
      color: config.color || { r: 1, g: 1, b: 1, a: 1 },
      intensity: config.intensity ?? 1.0,
      duration: config.duration ?? 1000,
      elapsed: 0,
      loop: config.loop ?? false,
      speed: config.speed ?? 1.0,
      blendMode: config.blendMode ?? 'additive',
      sortingOrder: config.sortingOrder ?? 0,
      visible: config.visible ?? true,
      active: config.active ?? false,
      children: [],
      properties: config.properties || {},
      emitters: [],
      shaders: config.shaders || [],
      audio: config.audio,
      onComplete: config.onComplete
    };

    this.effects.set(effect.id, effect);

    if (config.emitters) {
      config.emitters.forEach(emitterConfig => {
        const emitter = this.createEmitter(emitterConfig);
        effect.emitters.push(emitter);
      });
    }

    this.cache.effects.set(effect.id, this.cloneEffect(effect));

    return effect;
  }

  createEmitter(config: Partial<EffectEmitter>): EffectEmitter {
    const emitter: EffectEmitter = {
      id: config.id || `emitter_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      type: config.type ?? 'continuous',
      position: config.position || { x: 0, y: 0, z: 0 },
      emissionRate: config.emissionRate ?? 10,
      lifetime: config.lifetime ?? 1000,
      lifetimeVariation: config.lifetimeVariation ?? 0,
      initialVelocity: config.initialVelocity || { x: 0, y: 0, z: 0 },
      velocityVariation: config.velocityVariation || { x: 0, y: 0, z: 0 },
      acceleration: config.acceleration || { x: 0, y: 0, z: 0 },
      size: config.size ?? 1.0,
      sizeVariation: config.sizeVariation ?? 0,
      sizeOverLifetime: config.sizeOverLifetime || this.createDefaultCurve(),
      colorOverLifetime: config.colorOverLifetime || this.createDefaultColorGradient(),
      alphaOverLifetime: config.alphaOverLifetime || this.createDefaultCurve(),
      rotationSpeed: config.rotationSpeed ?? 0,
      rotationSpeedVariation: config.rotationSpeedVariation ?? 0,
      angularVelocity: config.angularVelocity || { x: 0, y: 0, z: 0 },
      shape: config.shape || this.createDefaultShape(),
      renderSettings: config.renderSettings || this.createDefaultRenderSettings(),
      particles: [],
      active: true,
      emitCount: 0,
      maxParticles: config.maxParticles ?? 100
    };

    this.emitters.set(emitter.id, emitter);
    return emitter;
  }

  private createDefaultCurve(): AnimationCurve {
    return {
      keys: [{ time: 0, value: 1, inTangent: 0, outTangent: 0, inWeight: 0, outWeight: 0, weightedMode: 'none' }],
      preWrap: 'clamp',
      postWrap: 'clamp',
      length: 1
    };
  }

  private createDefaultColorGradient(): ColorGradient {
    return {
      keys: [
        { time: 0, color: { r: 1, g: 1, b: 1, a: 1 }, alpha: 0, inTangent: { r: 0, g: 0, b: 0, a: 0 }, outTangent: { r: 0, g: 0, b: 0, a: 0 } },
        { time: 1, color: { r: 1, g: 1, b: 1, a: 1 }, alpha: 1, inTangent: { r: 0, g: 0, b: 0, a: 0 }, outTangent: { r: 0, g: 0, b: 0, a: 0 } }
      ],
      mode: 'blend'
    };
  }

  private createDefaultShape(): EmitterShape {
    return {
      type: 'sphere',
      radius: 0.5,
      radiusThickness: 1,
      angle: 360,
      length: 5,
      boxSize: { x: 1, y: 1, z: 1 },
      arc: 360,
      arcSpeed: 0,
      thickness: 1
    };
  }

  private createDefaultRenderSettings(): RenderSettings {
    return {
      material: 'default_particle',
      shader: 'particle_shader',
      texture: 'default_particle',
      atlas: { texture: '', columns: 1, rows: 1, frameCount: 1, frameRate: 30, loop: true, currentFrame: 0 },
      billboard: 'full',
      sortingOrder: 0,
      castShadows: false,
      receiveShadows: false,
      lightmapIndex: -1,
      lightmapScaleOffset: { x: 1, y: 1, z: 1 }
    };
  }

  private cloneEffect(effect: VisualEffect): VisualEffect {
    return JSON.parse(JSON.stringify(effect));
  }

  play(effectId: string): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.active = true;
    effect.elapsed = 0;

    effect.emitters.forEach(emitter => {
      emitter.active = true;
      this.activeParticles.set(emitter.id, []);
    });

    if (effect.audio) {
      const audioCallback = this.audioCallbacks.get(effect.audio);
      if (audioCallback) {
        audioCallback(effect);
      }
    }

    return true;
  }

  pause(effectId: string): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.active = false;

    effect.emitters.forEach(emitter => {
      emitter.active = false;
    });

    return true;
  }

  stop(effectId: string): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.active = false;
    effect.elapsed = 0;

    effect.emitters.forEach(emitter => {
      emitter.active = false;
      this.activeParticles.set(emitter.id, []);
      emitter.emitCount = 0;
    });

    return true;
  }

  setPosition(effectId: string, position: Vector3): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.position = position;
    return true;
  }

  setScale(effectId: string, scale: Vector3): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.scale = scale;
    return true;
  }

  setColor(effectId: string, color: Color): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.color = color;
    return true;
  }

  setIntensity(effectId: string, intensity: number): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.intensity = intensity;
    return true;
  }

  setSpeed(effectId: string, speed: number): boolean {
    const effect = this.effects.get(effectId);
    if (!effect) return false;

    effect.speed = speed;
    return true;
  }

  update(deltaTime: number): void {
    if (this.paused || !this.masterEnabled) {
      return;
    }

    const startTime = performance.now();

    this.deltaTime = deltaTime * this.globalSpeed;
    this.currentTime += this.deltaTime;

    this.effects.forEach((effect, id) => {
      if (effect.active) {
        this.updateEffect(effect);
      }
    });

    this.emitters.forEach((emitter, id) => {
      if (emitter.active) {
        this.updateEmitter(emitter);
      }
    });

    this.sequences.forEach(sequence => {
      if (sequence.playing) {
        this.updateSequence(sequence);
      }
    });

    this.cleanup();

    const endTime = performance.now();
    this.statistics.frameTime = endTime - startTime;

    this.notifyRenderCallbacks();
  }

  private updateEffect(effect: VisualEffect): void {
    effect.elapsed += this.deltaTime;

    if (!effect.loop && effect.elapsed >= effect.duration) {
      effect.active = false;

      if (effect.onComplete) {
        effect.onComplete();
      }
    }

    effect.emitters.forEach(emitter => {
      if (emitter.active) {
        this.updateEmitterParticles(emitter);
      }
    });

    effect.children.forEach(child => {
      if (child.active) {
        this.updateEffect(child);
      }
    });
  }

  private updateEmitter(emitter: EffectEmitter): void {
    switch (emitter.type) {
      case 'continuous':
        this.emitContinuous(emitter);
        break;
      case 'burst':
        this.emitBurst(emitter);
        break;
      case 'oneshot':
        this.emitOneshot(emitter);
        break;
      case 'wave':
        this.emitWave(emitter);
        break;
      case 'spiral':
        this.emitSpiral(emitter);
        break;
    }
  }

  private emitContinuous(emitter: EffectEmitter): void {
    const emitInterval = 1000 / emitter.emissionRate;
    const particlesToEmit = Math.floor(this.deltaTime / emitInterval) + (Math.random() < (this.deltaTime % emitInterval) / emitInterval ? 1 : 0);

    for (let i = 0; i < particlesToEmit; i++) {
      if (emitter.particles.length < emitter.maxParticles) {
        this.emitParticle(emitter);
      }
    }
  }

  private emitBurst(emitter: EffectEmitter): void {
    if (emitter.emitCount === 0) {
      const burstCount = Math.floor(emitter.emissionRate);
      for (let i = 0; i < burstCount; i++) {
        if (emitter.particles.length < emitter.maxParticles) {
          this.emitParticle(emitter);
        }
      }
      emitter.emitCount = 1;
    }
  }

  private emitOneshot(emitter: EffectEmitter): void {
    if (emitter.emitCount === 0) {
      for (let i = 0; i < emitter.emissionRate; i++) {
        if (emitter.particles.length < emitter.maxParticles) {
          this.emitParticle(emitter);
        }
      }
      emitter.emitCount = 1;
    }
  }

  private emitWave(emitter: EffectEmitter): void {
    const wavePhase = Math.sin(this.currentTime / 1000 * 2);
    const waveEmission = Math.floor(emitter.emissionRate * (0.5 + wavePhase * 0.5));

    for (let i = 0; i < waveEmission; i++) {
      if (emitter.particles.length < emitter.maxParticles) {
        this.emitParticle(emitter);
      }
    }
  }

  private emitSpiral(emitter: EffectEmitter): void {
    const angle = this.currentTime / 1000 * 2;
    const spiralRadius = emitter.shape.radius * (1 + (this.currentTime / 10000));

    const particle = this.createParticle(emitter);
    particle.position.x = Math.cos(angle) * spiralRadius;
    particle.position.z = Math.sin(angle) * spiralRadius;
    particle.velocity.y = emitter.initialVelocity.y;

    emitter.particles.push(particle);
    emitter.emitCount++;
  }

  private emitParticle(emitter: EffectEmitter): void {
    const particle = this.createParticle(emitter);

    const spawnPosition = this.getShapePosition(emitter.shape, emitter.position);
    particle.position = { ...spawnPosition };

    const velocity = this.calculateVelocity(emitter, particle);
    particle.velocity = velocity;

    emitter.particles.push(particle);
    emitter.emitCount++;
    this.statistics.totalEmitted++;
  }

  private createParticle(emitter: EffectEmitter): EffectParticle {
    const lifetime = emitter.lifetime + (Math.random() - 0.5) * emitter.lifetimeVariation;

    return {
      id: `particle_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      position: { x: 0, y: 0, z: 0 },
      velocity: { x: 0, y: 0, z: 0 },
      acceleration: { ...emitter.acceleration },
      rotation: { x: 0, y: 0, z: 0 },
      angularVelocity: { ...emitter.angularVelocity },
      size: emitter.size + (Math.random() - 0.5) * emitter.sizeVariation,
      color: { ...emitter.colorOverLifetime.keys[0]?.color || { r: 1, g: 1, b: 1, a: 1 } },
      alpha: emitter.colorOverLifetime.keys[0]?.alpha || 1,
      lifetime,
      age: 0,
      mass: 1,
      drag: 1,
      spin: emitter.rotationSpeed + (Math.random() - 0.5) * emitter.rotationSpeedVariation,
      stretch: 1,
      atlasFrame: 0
    };
  }

  private getShapePosition(shape: EmitterShape, basePosition: Vector3): Vector3 {
    switch (shape.type) {
      case 'point':
        return { ...basePosition };

      case 'sphere':
        const theta = Math.random() * Math.PI * 2;
        const phi = Math.acos(2 * Math.random() - 1);
        const r = shape.radius * Math.pow(Math.random(), shape.radiusThickness);
        return {
          x: basePosition.x + r * Math.sin(phi) * Math.cos(theta),
          y: basePosition.y + r * Math.sin(phi) * Math.sin(theta),
          z: basePosition.z + r * Math.cos(phi)
        };

      case 'hemisphere':
        const hTheta = Math.random() * Math.PI * 2;
        const hPhi = Math.random() * Math.PI * 0.5;
        const hR = shape.radius * Math.pow(Math.random(), shape.radiusThickness);
        return {
          x: basePosition.x + hR * Math.sin(hPhi) * Math.cos(hTheta),
          y: basePosition.y + hR * Math.cos(hPhi),
          z: basePosition.z + hR * Math.sin(hPhi) * Math.sin(hTheta)
        };

      case 'cone':
        const cTheta = Math.random() * Math.PI * 2;
        const cR = shape.radius * Math.random();
        const cAngleRad = (shape.angle * Math.PI) / 180;
        return {
          x: basePosition.x + cR * Math.cos(cTheta),
          y: basePosition.y,
          z: basePosition.z + cR * Math.sin(cTheta)
        };

      case 'box':
        return {
          x: basePosition.x + (Math.random() - 0.5) * shape.boxSize.x,
          y: basePosition.y + (Math.random() - 0.5) * shape.boxSize.y,
          z: basePosition.z + (Math.random() - 0.5) * shape.boxSize.z
        };

      case 'cylinder':
        const cylTheta = Math.random() * Math.PI * 2;
        const cylR = shape.radius * shape.thickness + shape.radius * (1 - shape.thickness) * Math.random();
        return {
          x: basePosition.x + cylR * Math.cos(cylTheta),
          y: basePosition.y + (Math.random() - 0.5) * shape.length,
          z: basePosition.z + cylR * Math.sin(cylTheta)
        };

      case 'ring':
        const ringTheta = Math.random() * Math.PI * 2 * (shape.arc / 360);
        const ringR = shape.radius + (Math.random() - 0.5) * shape.radiusThickness * shape.radius;
        return {
          x: basePosition.x + ringR * Math.cos(ringTheta),
          y: basePosition.y,
          z: basePosition.z + ringR * Math.sin(ringTheta)
        };

      default:
        return { ...basePosition };
    }
  }

  private calculateVelocity(emitter: EffectEmitter, particle: EffectParticle): Vector3 {
    const velocity: Vector3 = { ...emitter.initialVelocity };

    velocity.x += (Math.random() - 0.5) * emitter.velocityVariation.x;
    velocity.y += (Math.random() - 0.5) * emitter.velocityVariation.y;
    velocity.z += (Math.random() - 0.5) * emitter.velocityVariation.z;

    return velocity;
  }

  private updateEmitterParticles(emitter: EffectEmitter): void {
    const particlesToRemove: string[] = [];

    emitter.particles.forEach(particle => {
      particle.age += this.deltaTime;

      if (particle.age >= particle.lifetime) {
        particlesToRemove.push(particle.id);
        return;
      }

      const normalizedAge = particle.age / particle.lifetime;

      const sizeCurve = this.evaluateCurve(emitter.sizeOverLifetime, normalizedAge);
      particle.size = emitter.size * sizeCurve;

      const colorData = this.evaluateGradient(emitter.colorOverLifetime, normalizedAge);
      particle.color = colorData.color;
      particle.alpha = colorData.alpha;

      particle.velocity.x += particle.acceleration.x * this.deltaTime / 1000;
      particle.velocity.y += particle.acceleration.y * this.deltaTime / 1000;
      particle.velocity.z += particle.acceleration.z * this.deltaTime / 1000;

      particle.position.x += particle.velocity.x * this.deltaTime / 1000;
      particle.position.y += particle.velocity.y * this.deltaTime / 1000;
      particle.position.z += particle.velocity.z * this.deltaTime / 1000;

      particle.rotation.x += particle.angularVelocity.x * this.deltaTime / 1000;
      particle.rotation.y += particle.angularVelocity.y * this.deltaTime / 1000;
      particle.rotation.z += particle.angularVelocity.z * this.deltaTime / 1000;

      particle.rotation.z += particle.spin * this.deltaTime / 1000;
    });

    particlesToRemove.forEach(id => {
      const index = emitter.particles.findIndex(p => p.id === id);
      if (index !== -1) {
        const particle = emitter.particles[index];
        if (particle.trail) {
          this.particlePool.push(particle);
        }
        emitter.particles.splice(index, 1);
      }
    });
  }

  private evaluateCurve(curve: AnimationCurve, time: number): number {
    if (curve.keys.length === 0) return 1;
    if (curve.keys.length === 1) return curve.keys[0].value;

    let key1 = curve.keys[0];
    let key2 = curve.keys[curve.keys.length - 1];

    for (let i = 0; i < curve.keys.length - 1; i++) {
      if (time >= curve.keys[i].time && time <= curve.keys[i + 1].time) {
        key1 = curve.keys[i];
        key2 = curve.keys[i + 1];
        break;
      }
    }

    const t = (time - key1.time) / (key2.time - key1.time);

    const smoothT = this.smoothstep(t);

    return key1.value + (key2.value - key1.value) * smoothT;
  }

  private smoothstep(t: number): number {
    return t * t * (3 - 2 * t);
  }

  private evaluateGradient(gradient: ColorGradient, time: number): { color: Color; alpha: number } {
    if (gradient.keys.length === 0) {
      return { color: { r: 1, g: 1, b: 1, a: 1 }, alpha: 1 };
    }

    let key1 = gradient.keys[0];
    let key2 = gradient.keys[gradient.keys.length - 1];

    for (let i = 0; i < gradient.keys.length - 1; i++) {
      if (time >= gradient.keys[i].time && time <= gradient.keys[i + 1].time) {
        key1 = gradient.keys[i];
        key2 = gradient.keys[i + 1];
        break;
      }
    }

    const t = (time - key1.time) / (key2.time - key1.time);

    return {
      color: {
        r: key1.color.r + (key2.color.r - key1.color.r) * t,
        g: key1.color.g + (key2.color.g - key1.color.g) * t,
        b: key1.color.b + (key2.color.b - key1.color.b) * t,
        a: key1.color.a + (key2.color.a - key1.color.a) * t
      },
      alpha: key1.alpha + (key2.alpha - key1.alpha) * t
    };
  }

  private updateSequence(sequence: EffectSequence): void {
    sequence.elapsed += this.deltaTime;

    while (sequence.currentStep < sequence.steps.length &&
           sequence.elapsed >= sequence.steps[sequence.currentStep].time) {
      const step = sequence.steps[sequence.currentStep];
      this.executeSequenceStep(step);

      if (step.onStepComplete) {
        step.onStepComplete(step);
      }

      sequence.currentStep++;
    }

    if (sequence.currentStep >= sequence.steps.length) {
      if (sequence.loop) {
        sequence.currentStep = 0;
        sequence.elapsed = 0;
      } else {
        sequence.playing = false;
        if (sequence.onComplete) {
          sequence.onComplete();
        }
      }
    }
  }

  private executeSequenceStep(step: EffectSequenceStep): void {
    switch (step.action) {
      case 'play':
        this.play(step.effect.id);
        break;
      case 'pause':
        this.pause(step.effect.id);
        break;
      case 'stop':
        this.stop(step.effect.id);
        break;
      case 'scale':
        this.setScale(step.effect.id, step.parameters.position as Vector3);
        break;
      case 'move':
        this.setPosition(step.effect.id, step.parameters.position as Vector3);
        break;
      case 'rotate':
        step.effect.rotation = step.parameters.rotation as Vector3;
        break;
      case 'fade':
        this.setIntensity(step.effect.id, step.parameters.intensity as number);
        break;
      case 'color':
        this.setColor(step.effect.id, step.parameters.color as Color);
        break;
    }
  }

  createSequence(config: Partial<EffectSequence> & { name: string }): EffectSequence {
    const sequence: EffectSequence = {
      id: config.id || `sequence_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      name: config.name,
      steps: config.steps || [],
      currentStep: 0,
      loop: config.loop ?? false,
      playing: false,
      duration: config.duration ?? 0,
      elapsed: 0,
      onStepComplete: config.onStepComplete,
      onComplete: config.onComplete
    };

    sequence.duration = Math.max(...sequence.steps.map(s => s.time + s.duration), 0);

    this.sequences.set(sequence.id, sequence);
    return sequence;
  }

  playSequence(sequenceId: string): boolean {
    const sequence = this.sequences.get(sequenceId);
    if (!sequence) return false;

    sequence.playing = true;
    sequence.currentStep = 0;
    sequence.elapsed = 0;

    return true;
  }

  pauseSequence(sequenceId: string): boolean {
    const sequence = this.sequences.get(sequenceId);
    if (!sequence) return false;

    sequence.playing = false;
    return true;
  }

  stopSequence(sequenceId: string): boolean {
    const sequence = this.sequences.get(sequenceId);
    if (!sequence) return false;

    sequence.playing = false;
    sequence.currentStep = 0;
    sequence.elapsed = 0;

    return true;
  }

  private cleanup(): void {
    this.effects.forEach((effect, id) => {
      if (!effect.active && effect.elapsed >= effect.duration && !effect.loop) {
      }
    });

    while (this.particlePool.length > this.maxPoolSize) {
      this.particlePool.pop();
    }
  }

  private notifyRenderCallbacks(): void {
    const visibleEffects = this.getVisibleEffects();
    this.renderCallbacks.forEach(callback => {
      callback(visibleEffects);
    });
  }

  getVisibleEffects(): VisualEffect[] {
    const visible: VisualEffect[] = [];

    this.effects.forEach(effect => {
      if (effect.visible) {
        visible.push(effect);
      }
    });

    return visible.sort((a, b) => a.sortingOrder - b.sortingOrder);
  }

  getEffect(id: string): VisualEffect | undefined {
    return this.effects.get(id);
  }

  getAllEffects(): VisualEffect[] {
    return Array.from(this.effects.values());
  }

  getEmitterParticles(emitterId: string): EffectParticle[] {
    return this.emitters.get(emitterId)?.particles || [];
  }

  getStatistics(): EffectStatistics {
    let totalParticles = 0;
    let activeEmitters = 0;

    this.emitters.forEach(emitter => {
      totalParticles += emitter.particles.length;
      if (emitter.active) activeEmitters++;
    });

    this.statistics.totalParticles = totalParticles;
    this.statistics.activeEffects = Array.from(this.effects.values()).filter(e => e.active).length;
    this.statistics.activeEmitters = activeEmitters;
    this.statistics.memoryUsage = this.estimateMemoryUsage();

    return { ...this.statistics };
  }

  private estimateMemoryUsage(): number {
    let bytes = 0;

    bytes += this.effects.size * 500;
    bytes += this.emitters.size * 1000;

    this.emitters.forEach(emitter => {
      bytes += emitter.particles.length * 300;
    });

    bytes += this.particlePool.length * 300;

    return bytes;
  }

  onRender(callback: (effects: VisualEffect[]) => void): void {
    this.renderCallbacks.push(callback);
  }

  offRender(callback: (effects: VisualEffect[]) => void): void {
    const index = this.renderCallbacks.indexOf(callback);
    if (index !== -1) {
      this.renderCallbacks.splice(index, 1);
    }
  }

  registerAudioCallback(audioId: string, callback: (effect: VisualEffect) => void): void {
    this.audioCallbacks.set(audioId, callback);
  }

  unregisterAudioCallback(audioId: string): void {
    this.audioCallbacks.delete(audioId);
  }

  setGlobalSpeed(speed: number): void {
    this.globalSpeed = Math.max(0.1, Math.min(10, speed));
  }

  setGlobalIntensity(intensity: number): void {
    this.globalIntensity = Math.max(0, Math.min(2, intensity));
  }

  setMasterEnabled(enabled: boolean): void {
    this.masterEnabled = enabled;
  }

  pause(): void {
    this.paused = true;
  }

  resume(): void {
    this.paused = false;
    this.lastUpdateTime = performance.now();
  }

  destroyEffect(id: string): boolean {
    const effect = this.effects.get(id);
    if (!effect) return false;

    effect.emitters.forEach(emitter => {
      this.emitters.delete(emitter.id);
      this.activeParticles.delete(emitter.id);
    });

    this.effects.delete(id);
    return true;
  }

  destroyLayer(id: string): boolean {
    const layer = this.layers.get(id);
    if (!layer) return false;

    layer.effects.forEach(effect => {
      this.destroyEffect(effect.id);
    });

    this.layers.delete(id);
    return true;
  }

  destroySequence(id: string): boolean {
    return this.sequences.delete(id);
  }

  clearAll(): void {
    this.effects.clear();
    this.emitters.clear();
    this.sequences.clear();
    this.activeParticles.clear();
    this.particlePool = [];

    this.layers.forEach(layer => {
      layer.effects = [];
    });
  }

  serialize(): VisualEffectSystemState {
    return {
      effects: Array.from(this.effects.values()).map(e => ({
        ...e,
        onComplete: undefined,
        children: []
      })),
      layers: Array.from(this.layers.values()).map(l => ({
        ...l,
        effects: l.effects.map(e => e.id)
      })),
      emitters: Array.from(this.emitters.values()).map(em => ({
        ...em,
        particles: []
      })),
      sequences: Array.from(this.sequences.values()).map(s => ({
        ...s,
        onStepComplete: undefined,
        onComplete: undefined
      })),
      globalSpeed: this.globalSpeed,
      globalIntensity: this.globalIntensity,
      masterEnabled: this.masterEnabled
    };
  }

  deserialize(state: VisualEffectSystemState): void {
    this.clearAll();

    state.layers.forEach(layerData => {
      this.createLayer(layerData);
    });

    state.effects.forEach(effectData => {
      this.createEffect(effectData);
    });

    state.emitters.forEach(emitterData => {
      this.createEmitter(emitterData);
    });

    state.sequences.forEach(sequenceData => {
      this.createSequence(sequenceData);
    });

    this.globalSpeed = state.globalSpeed;
    this.globalIntensity = state.globalIntensity;
    this.masterEnabled = state.masterEnabled;
  }
}

interface EffectCache {
  effects: Map<string, VisualEffect>;
  gradients: Map<string, ColorGradient>;
  curves: Map<string, AnimationCurve>;
}

class EffectCache {
  effects = new Map();
  gradients = new Map();
  curves = new Map();
}

interface EffectStatistics {
  totalParticles: number;
  activeEffects: number;
  activeEmitters: number;
  totalEmitted: number;
  frameTime: number;
  memoryUsage: number;
}

interface VisualEffectSystemState {
  effects: VisualEffect[];
  layers: { id: string; name: string; order: number; enabled: boolean; visible: boolean; locked: boolean; color: Color; blendMode: BlendMode; effects: string[] }[];
  emitters: EffectEmitter[];
  sequences: EffectSequence[];
  globalSpeed: number;
  globalIntensity: number;
  masterEnabled: boolean;
}

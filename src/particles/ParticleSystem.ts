import { Vector2D } from '../../entities/Entity';

export interface Particle {
  id: string;
  position: Vector2D;
  velocity: Vector2D;
  acceleration: Vector2D;
  life: number;
  maxLife: number;
  size: number;
  initialSize: number;
  color: string;
  alpha: number;
  initialAlpha: number;
  decay: number;
  gravity: number;
  friction: number;
  rotation: number;
  angularVelocity: number;
  texture?: string;
  blendMode: GlobalCompositeOperation;
  active: boolean;
  userData: any;
}

export interface ParticleEmitter {
  id: string;
  position: Vector2D;
  rate: number;
  maxParticles: number;
  emitCount: number;
  emitVariance: number;
  particleLife: number;
  particleLifeVariance: number;
  initialVelocity: Vector2D;
  velocityVariance: Vector2D;
  size: number;
  sizeVariance: number;
  color: string;
  colorVariance: string;
  alpha: number;
  alphaVariance: number;
  decay: number;
  gravity: number;
  friction: number;
  angularVelocity: number;
  angularVelocityVariance: number;
  spread: number;
  active: boolean;
  duration: number;
  elapsed: number;
  loop: boolean;
}

export interface ParticleSystemConfig {
  maxParticles: number;
  defaultParticleLife: number;
  defaultDecay: number;
  defaultGravity: number;
  defaultFriction: number;
}

export class ParticleSystem {
  private particles: Map<string, Particle> = new Map();
  private emitters: Map<string, ParticleEmitter> = new Map();
  private config: ParticleSystemConfig;
  private particlePool: Particle[] = [];
  private nextParticleId: number = 0;
  private totalEmitted: number = 0;
  private totalKilled: number = 0;

  constructor(config: Partial<ParticleSystemConfig> = {}) {
    this.config = {
      maxParticles: config.maxParticles || 10000,
      defaultParticleLife: config.defaultParticleLife || 2.0,
      defaultDecay: config.defaultDecay || 0.98,
      defaultGravity: config.defaultGravity || 0,
      defaultFriction: config.defaultFriction || 1.0,
    };

    for (let i = 0; i < 1000; i++) {
      this.particlePool.push(this.createEmptyParticle());
    }
  }

  private createEmptyParticle(): Particle {
    return {
      id: '',
      position: { x: 0, y: 0 },
      velocity: { x: 0, y: 0 },
      acceleration: { x: 0, y: 0 },
      life: 0,
      maxLife: 0,
      size: 0,
      initialSize: 0,
      color: '#ffffff',
      alpha: 1,
      initialAlpha: 1,
      decay: this.config.defaultDecay,
      gravity: this.config.defaultGravity,
      friction: this.config.defaultFriction,
      rotation: 0,
      angularVelocity: 0,
      blendMode: 'source-over',
      active: false,
      userData: null,
    };
  }

  private getParticle(): Particle {
    if (this.particlePool.length > 0) {
      return this.particlePool.pop()!;
    }

    if (this.particles.size < this.config.maxParticles) {
      return this.createEmptyParticle();
    }

    let oldest: Particle | null = null;
    for (const particle of this.particles.values()) {
      if (!oldest || particle.life < oldest.life) {
        oldest = particle;
      }
    }

    if (oldest) {
      this.particles.delete(oldest.id);
      oldest.active = false;
      return oldest;
    }

    return this.createEmptyParticle();
  }

  private releaseParticle(particle: Particle): void {
    this.particles.delete(particle.id);
    particle.active = false;

    if (this.particlePool.length < 2000) {
      this.particlePool.push(particle);
    }
  }

  public createEmitter(config: Partial<ParticleEmitter>): string {
    const id = config.id || `emitter_${Date.now()}_${Math.random()}`;

    const emitter: ParticleEmitter = {
      id,
      position: config.position || { x: 0, y: 0 },
      rate: config.rate || 10,
      maxParticles: config.maxParticles || 100,
      emitCount: config.emitCount || 1,
      emitVariance: config.emitVariance || 0,
      particleLife: config.particleLife || this.config.defaultParticleLife,
      particleLifeVariance: config.particleLifeVariance || 0,
      initialVelocity: config.initialVelocity || { x: 0, y: 0 },
      velocityVariance: config.velocityVariance || { x: 0, y: 0 },
      size: config.size || 10,
      sizeVariance: config.sizeVariance || 0,
      color: config.color || '#ffffff',
      colorVariance: config.colorVariance || '#000000',
      alpha: config.alpha || 1.0,
      alphaVariance: config.alphaVariance || 0,
      decay: config.decay || this.config.defaultDecay,
      gravity: config.gravity || this.config.defaultGravity,
      friction: config.friction || this.config.defaultFriction,
      angularVelocity: config.angularVelocity || 0,
      angularVelocityVariance: config.angularVelocityVariance || 0,
      spread: config.spread || Math.PI * 2,
      active: config.active !== undefined ? config.active : true,
      duration: config.duration || 0,
      elapsed: 0,
      loop: config.loop !== undefined ? config.loop : true,
    };

    this.emitters.set(id, emitter);
    return id;
  }

  public removeEmitter(id: string): void {
    this.emitters.delete(id);
  }

  public getEmitter(id: string): ParticleEmitter | undefined {
    return this.emitters.get(id);
  }

  public emit(
    position: Vector2D,
    velocity: Vector2D,
    config: {
      life?: number;
      size?: number;
      color?: string;
      alpha?: number;
      decay?: number;
      gravity?: number;
      friction?: number;
      angularVelocity?: number;
    } = {}
  ): string | null {
    if (this.particles.size >= this.config.maxParticles) {
      return null;
    }

    const particle = this.getParticle();
    const id = `particle_${this.nextParticleId++}`;

    particle.id = id;
    particle.position = { ...position };
    particle.velocity = { ...velocity };
    particle.acceleration = { x: 0, y: 0 };
    particle.life = config.life || this.config.defaultParticleLife;
    particle.maxLife = particle.life;
    particle.size = config.size || 10;
    particle.initialSize = particle.size;
    particle.color = config.color || '#ffffff';
    particle.alpha = config.alpha || 1.0;
    particle.initialAlpha = particle.alpha;
    particle.decay = config.decay || this.config.defaultDecay;
    particle.gravity = config.gravity || this.config.defaultGravity;
    particle.friction = config.friction || this.config.defaultFriction;
    particle.rotation = 0;
    particle.angularVelocity = config.angularVelocity || 0;
    particle.active = true;
    particle.userData = null;

    this.particles.set(id, particle);
    this.totalEmitted++;

    return id;
  }

  public burst(
    position: Vector2D,
    count: number,
    config: {
      life?: number;
      lifeVariance?: number;
      size?: number;
      sizeVariance?: number;
      speed?: number;
      speedVariance?: number;
      color?: string;
      colorVariance?: string;
      alpha?: number;
      alphaVariance?: number;
      decay?: number;
      gravity?: number;
      friction?: number;
      angularVelocity?: number;
      angularVelocityVariance?: number;
      spread?: number;
    } = {}
  ): string[] {
    const ids: string[] = [];
    const spread = config.spread || Math.PI * 2;
    const angleStep = spread / count;

    for (let i = 0; i < count; i++) {
      const angle = -spread / 2 + angleStep * i;
      const speed = (config.speed || 100) + (Math.random() - 0.5) * 2 * (config.speedVariance || 0);

      const velocity = {
        x: Math.cos(angle) * speed,
        y: Math.sin(angle) * speed,
      };

      const life = (config.life || 1) + (Math.random() - 0.5) * 2 * (config.lifeVariance || 0);
      const size = (config.size || 10) + (Math.random() - 0.5) * 2 * (config.sizeVariance || 0);
      const alpha = (config.alpha || 1) + (Math.random() - 0.5) * 2 * (config.alphaVariance || 0);
      const angularVelocity = (config.angularVelocity || 0) + (Math.random() - 0.5) * 2 * (config.angularVelocityVariance || 0);

      const particleId = this.emit(position, velocity, {
        life,
        size,
        color: config.color,
        alpha,
        decay: config.decay,
        gravity: config.gravity,
        friction: config.friction,
        angularVelocity,
      });

      if (particleId) {
        ids.push(particleId);
      }
    }

    return ids;
  }

  public trail(
    position: Vector2D,
    direction: Vector2D,
    config: {
      life?: number;
      size?: number;
      color?: string;
      speed?: number;
    } = {}
  ): string | null {
    const speed = config.speed || 10;
    const velocity = {
      x: direction.x * speed + (Math.random() - 0.5) * 20,
      y: direction.y * speed + (Math.random() - 0.5) * 20,
    };

    return this.emit(position, velocity, config);
  }

  public update(deltaTime: number): void {
    for (const emitter of this.emitters.values()) {
      if (!emitter.active) continue;

      emitter.elapsed += deltaTime;

      if (emitter.duration > 0 && emitter.elapsed >= emitter.duration) {
        if (!emitter.loop) {
          emitter.active = false;
        }
        emitter.elapsed = 0;
        continue;
      }

      const particlesToEmit = Math.floor(emitter.rate * deltaTime);
      const variance = Math.floor((Math.random() - 0.5) * 2 * emitter.emitVariance);
      const totalEmit = Math.max(0, particlesToEmit + variance);

      const currentParticles = Array.from(this.particles.values()).filter(
        p => this.getParticleEmitter(p.id) === emitter.id
      ).length;

      const canEmit = Math.min(totalEmit, emitter.maxParticles - currentParticles);

      for (let i = 0; i < canEmit; i++) {
        this.emitFromEmitter(emitter);
      }
    }

    for (const [id, particle] of this.particles) {
      if (!particle.active) continue;

      particle.life -= deltaTime;

      if (particle.life <= 0) {
        this.releaseParticle(particle);
        this.totalKilled++;
        continue;
      }

      particle.velocity.x += particle.acceleration.x * deltaTime;
      particle.velocity.y += particle.acceleration.y * deltaTime;

      particle.velocity.y += particle.gravity * deltaTime;

      particle.velocity.x *= particle.friction;
      particle.velocity.y *= particle.friction;

      particle.position.x += particle.velocity.x * deltaTime;
      particle.position.y += particle.velocity.y * deltaTime;

      particle.rotation += particle.angularVelocity * deltaTime;

      const lifeRatio = particle.life / particle.maxLife;
      particle.alpha = particle.initialAlpha * lifeRatio;
      particle.size = particle.initialSize * lifeRatio;
    }
  }

  private emitFromEmitter(emitter: ParticleEmitter): void {
    if (this.particles.size >= this.config.maxParticles) return;

    const angle = (Math.random() - 0.5) * emitter.spread;
    const cos = Math.cos(angle);
    const sin = Math.sin(angle);

    const speed = Math.sqrt(
      emitter.initialVelocity.x * emitter.initialVelocity.x +
      emitter.initialVelocity.y * emitter.initialVelocity.y
    );

    const speedVariance = Math.sqrt(
      emitter.velocityVariance.x * emitter.velocityVariance.x +
      emitter.velocityVariance.y * emitter.velocityVariance.y
    );

    const actualSpeed = speed + (Math.random() - 0.5) * 2 * speedVariance;

    const velocity = {
      x: cos * emitter.initialVelocity.x - sin * emitter.initialVelocity.y + (Math.random() - 0.5) * 2 * emitter.velocityVariance.x,
      y: sin * emitter.initialVelocity.x + cos * emitter.initialVelocity.y + (Math.random() - 0.5) * 2 * emitter.velocityVariance.y,
    };

    const actualSpeed2 = Math.sqrt(velocity.x * velocity.x + velocity.y * velocity.y);
    if (actualSpeed2 > 0) {
      velocity.x = (velocity.x / actualSpeed2) * actualSpeed;
      velocity.y = (velocity.y / actualSpeed2) * actualSpeed;
    }

    const particle = this.getParticle();
    const id = `particle_${this.nextParticleId++}`;

    particle.id = id;
    particle.position = {
      x: emitter.position.x + (Math.random() - 0.5) * 2 * 10,
      y: emitter.position.y + (Math.random() - 0.5) * 2 * 10,
    };
    particle.velocity = velocity;
    particle.acceleration = { x: 0, y: 0 };
    particle.life = emitter.particleLife + (Math.random() - 0.5) * 2 * emitter.particleLifeVariance;
    particle.maxLife = particle.life;
    particle.size = emitter.size + (Math.random() - 0.5) * 2 * emitter.sizeVariance;
    particle.initialSize = particle.size;
    particle.color = this.blendColors(emitter.color, emitter.colorVariance, Math.random());
    particle.alpha = emitter.alpha + (Math.random() - 0.5) * 2 * emitter.alphaVariance;
    particle.initialAlpha = particle.alpha;
    particle.decay = emitter.decay;
    particle.gravity = emitter.gravity;
    particle.friction = emitter.friction;
    particle.rotation = 0;
    particle.angularVelocity = emitter.angularVelocity + (Math.random() - 0.5) * 2 * emitter.angularVelocityVariance;
    particle.active = true;
    particle.userData = null;

    this.particles.set(id, particle);
    this.totalEmitted++;
  }

  private blendColors(color1: string, color2: string, ratio: number): string {
    const hex = (c: string) => parseInt(c.slice(1), 16);
    const r = (h: number) => (h >> 16) & 255;
    const g = (h: number) => (h >> 8) & 255;
    const b = (h: number) => h & 255;

    const h1 = hex(color1);
    const h2 = hex(color2);

    const rr = Math.round(r(h1) * (1 - ratio) + r(h2) * ratio);
    const gg = Math.round(g(h1) * (1 - ratio) + g(h2) * ratio);
    const bb = Math.round(b(h1) * (1 - ratio) + b(h2) * ratio);

    return `#${((rr << 16) | (gg << 8) | bb).toString(16).padStart(6, '0')}`;
  }

  private getParticleEmitter(particleId: string): string | null {
    for (const [emitterId, emitter] of this.emitters) {
      if (emitter.position.x === 0 && emitter.position.y === 0) {
        continue;
      }
    }
    return null;
  }

  public render(ctx: CanvasRenderingContext2D): void {
    for (const particle of this.particles.values()) {
      if (!particle.active) continue;
      if (particle.alpha <= 0 || particle.size <= 0) continue;

      ctx.save();
      ctx.globalAlpha = Math.max(0, Math.min(1, particle.alpha));
      ctx.fillStyle = particle.color;
      ctx.translate(particle.position.x, particle.position.y);
      ctx.rotate(particle.rotation);

      ctx.beginPath();
      ctx.arc(0, 0, particle.size, 0, Math.PI * 2);
      ctx.fill();

      ctx.restore();
    }
  }

  public clear(): void {
    for (const particle of this.particles.values()) {
      particle.active = false;
      this.particlePool.push(particle);
    }
    this.particles.clear();
  }

  public clearEmitters(): void {
    this.emitters.clear();
  }

  public getParticleCount(): number {
    return this.particles.size;
  }

  public getEmitterCount(): number {
    return this.emitters.size;
  }

  public getStats(): { total: number; active: number; emitted: number; killed: number; pooled: number } {
    return {
      total: this.config.maxParticles,
      active: this.particles.size,
      emitted: this.totalEmitted,
      killed: this.totalKilled,
      pooled: this.particlePool.length,
    };
  }
}

export class ParticlePresets {
  public static explosion(ps: ParticleSystem, position: Vector2D, size: number = 50): string[] {
    return ps.burst(position, 30, {
      life: 0.8,
      lifeVariance: 0.3,
      size: size / 5,
      sizeVariance: size / 10,
      speed: size * 3,
      speedVariance: size,
      color: '#ff6600',
      colorVariance: '#ff0000',
      gravity: 200,
      friction: 0.95,
      spread: Math.PI * 2,
    });
  }

  public static smoke(ps: ParticleSystem, position: Vector2D, count: number = 20): string[] {
    return ps.burst(position, count, {
      life: 2,
      lifeVariance: 1,
      size: 30,
      sizeVariance: 15,
      speed: 50,
      speedVariance: 30,
      color: '#555555',
      colorVariance: '#333333',
      alpha: 0.5,
      gravity: -20,
      friction: 0.98,
      spread: Math.PI,
    });
  }

  public static fire(ps: ParticleSystem, position: Vector2D, count: number = 10): string[] {
    return ps.burst(position, count, {
      life: 0.5,
      lifeVariance: 0.2,
      size: 20,
      sizeVariance: 10,
      speed: 80,
      speedVariance: 40,
      color: '#ff3300',
      colorVariance: '#ffaa00',
      gravity: -100,
      friction: 0.9,
      spread: Math.PI * 0.5,
    });
  }

  public static spark(ps: ParticleSystem, position: Vector2D, direction: Vector2D, count: number = 5): string[] {
    return ps.burst(position, count, {
      life: 0.3,
      lifeVariance: 0.1,
      size: 5,
      sizeVariance: 2,
      speed: 200,
      speedVariance: 100,
      color: '#ffff00',
      colorVariance: '#ffffff',
      gravity: 300,
      friction: 0.9,
    });
  }

  public static blood(ps: ParticleSystem, position: Vector2D, direction: Vector2D, count: number = 15): string[] {
    return ps.burst(position, count, {
      life: 0.6,
      lifeVariance: 0.2,
      size: 8,
      sizeVariance: 4,
      speed: 150,
      speedVariance: 50,
      color: '#cc0000',
      colorVariance: '#880000',
      gravity: 400,
      friction: 0.85,
    });
  }

  public static magic(ps: ParticleSystem, position: Vector2D, color: string = '#00ffff', count: number = 20): string[] {
    return ps.burst(position, count, {
      life: 1.5,
      lifeVariance: 0.5,
      size: 15,
      sizeVariance: 5,
      speed: 100,
      speedVariance: 50,
      color,
      colorVariance: '#ffffff',
      alpha: 0.8,
      gravity: -50,
      friction: 0.98,
      angularVelocity: 5,
      angularVelocityVariance: 3,
      spread: Math.PI * 2,
    });
  }

  public static snow(ps: ParticleSystem, position: Vector2D, count: number = 50): string[] {
    return ps.burst(position, count, {
      life: 3,
      lifeVariance: 1,
      size: 5,
      sizeVariance: 3,
      speed: 30,
      speedVariance: 15,
      color: '#ffffff',
      colorVariance: '#ccddff',
      alpha: 0.8,
      gravity: 10,
      friction: 0.99,
      angularVelocity: 2,
      angularVelocityVariance: 1,
      spread: Math.PI * 0.3,
    });
  }

  public static rain(ps: ParticleSystem, position: Vector2D, count: number = 100): string[] {
    return ps.burst(position, count, {
      life: 1,
      lifeVariance: 0.3,
      size: 3,
      sizeVariance: 1,
      speed: 400,
      speedVariance: 50,
      color: '#6699ff',
      alpha: 0.6,
      gravity: 500,
      friction: 1.0,
      spread: Math.PI * 0.1,
    });
  }

  public static dust(ps: ParticleSystem, position: Vector2D, count: number = 15): string[] {
    return ps.burst(position, count, {
      life: 1.2,
      lifeVariance: 0.4,
      size: 10,
      sizeVariance: 5,
      speed: 60,
      speedVariance: 30,
      color: '#aa8866',
      colorVariance: '#886644',
      alpha: 0.4,
      gravity: 5,
      friction: 0.95,
      spread: Math.PI,
    });
  }
}

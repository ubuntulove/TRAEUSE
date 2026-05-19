import { GAME_CONFIG } from './config.js';

export const PARTICLE_TYPE = {
    POINT: 0,
    SPRITE: 1,
    MODEL: 2,
    DECAL: 3,
    ROPE: 4,
    BEAM: 5,
    BOX: 6,
    SPHERE: 7,
    Cylinder: 8,
    TRAIL: 9,
    SMOKE: 10,
    FIRE: 11,
    SPARK: 12,
    EMBER: 13,
    BLOOD: 14,
    WATER: 15,
    DUST: 16,
    FOOTPRINT: 17,
    BULLET_IMPACT: 18,
    EXPLOSION: 19,
    MUZZLE_FLASH: 20,
    SHELL_CASING: 21,
    TRACER: 22,
    LASER: 23,
    LIGHT: 24,
    RICOCHET: 25,
    GLASS_SHARD: 26,
    METAL_SHARD: 27,
    DEBRIS: 28,
    CUSTOM: 999
};

export const PARTICLE_BLEND_MODE = {
    ADDITIVE: 0,
    ALPHA: 1,
    MULTIPLY: 2,
    SCREEN: 3,
    OVERLAY: 4,
    OPACITY_ADD: 5,
    MAX: 6
};

export const PARTICLE_RENDER_MODE = {
    NORMAL: 0,
    NEAREST: 1,
    BILLBOARD: 2,
    ORIENTED: 3,
    WORLD_ORIENTED: 4,
    VIEW_ORIENTED: 5,
    Axis_ALIGNED: 6,
    LOCAL_ALIGNED: 7
};

export const PARTICLE_UPDATE_FLAGS = {
    POSITION: 1 << 0,
    VELOCITY: 1 << 1,
    ACCELERATION: 1 << 2,
    SIZE: 1 << 3,
    COLOR: 1 << 4,
    ALPHA: 1 << 5,
    ROTATION: 1 << 6,
    ROTATION_RATE: 1 << 7,
    LIFETIME: 1 << 8,
    WORLD_TRANSFORM: 1 << 9,
    COLLISION: 1 << 10,
    TRAIL: 1 << 11,
    ORIENTATION: 1 << 12,
    SCALE: 1 << 13
};

export const PARTICLE_COLLISION_MODE = {
    NONE: 0,
    SPHERE: 1,
    AABB: 2,
    PLANE: 3,
    TRIGGER: 4,
    RAYCAST: 5
};

export const PARTICLE_SORT_MODE = {
    NONE: 0,
    BACK_TO_FRONT: 1,
    FRONT_TO_BACK: 2,
    BY_DISTANCE: 3,
    BY_LIFETIME: 4,
    BY_Z_DEPTH: 5
};

export class Particle {
    constructor(template) {
        this.id = Particle.nextId++;
        this.template = template;
        this.active = false;
        this.age = 0;
        this.lifetime = 0;
        this.normalizedAge = 0;
        
        this.position = { x: 0, y: 0, z: 0 };
        this.previousPosition = { x: 0, y: 0, z: 0 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.acceleration = { x: 0, y: 0, z: 0 };
        
        this.rotation = 0;
        this.rotationRate = 0;
        this.angularVelocity = { x: 0, y: 0, z: 0 };
        
        this.size = { x: 1, y: 1, z: 1 };
        this.sizeScale = 1;
        this.baseSize = { x: 1, y: 1, z: 1 };
        
        this.color = { r: 255, g: 255, b: 255, a: 255 };
        this.alpha = 1;
        this.baseColor = { r: 255, g: 255, b: 255, a: 255 };
        
        this.sequences = [];
        this.currentSequence = 0;
        this.sequenceTime = 0;
        
        this.trailLength = 0;
        this.trailData = [];
        
        this.collisionCount = 0;
        this.bounces = 0;
        this.friction = 0;
        this.bounceRestitution = 0.5;
        
        this.userData = {};
        this.controlPointIndices = [];
        this.controlPointPositions = [];
        
        this.initialValues = {};
        this.currentValues = {};
        
        this.orientation = { x: 0, y: 0, z: 0, w: 1 };
        this.forward = { x: 0, y: 0, z: 1 };
        this.up = { x: 0, y: 1, z: 0 };
        this.right = { x: 1, y: 0, z: 0 };
        
        this.sortDistance = 0;
        this.renderOrder = 0;
        
        this.material = null;
        this.texture = null;
        this.model = null;
        this.mesh = null;
        
        this.boundingBox = {
            min: { x: 0, y: 0, z: 0 },
            max: { x: 0, y: 0, z: 0 }
        };
        
        this.flags = 0;
        
        this.nextParticle = null;
        this.prevParticle = null;
    }
    
    static nextId = 0;
    
    initialize(template, controlPoints = []) {
        this.template = template;
        this.active = true;
        this.age = 0;
        this.lifetime = template.lifetime.getValue(Math.random());
        this.normalizedAge = 0;
        
        this.baseSize.x = template.sizeStart.x;
        this.baseSize.y = template.sizeStart.y;
        this.baseSize.z = template.sizeStart.z;
        this.size.x = this.baseSize.x;
        this.size.y = this.baseSize.y;
        this.size.z = this.baseSize.z;
        this.sizeScale = 1;
        
        this.baseColor.r = template.colorStart.r;
        this.baseColor.g = template.colorStart.g;
        this.baseColor.b = template.colorStart.b;
        this.baseColor.a = template.colorStart.a;
        this.color.r = this.baseColor.r;
        this.color.g = this.baseColor.g;
        this.color.b = this.baseColor.b;
        this.color.a = this.baseColor.a;
        this.alpha = this.baseColor.a / 255;
        
        this.rotation = template.rotationStart + (Math.random() - 0.5) * template.rotationRandom;
        this.rotationRate = template.rotationRateStart + (Math.random() - 0.5) * template.rotationRateRandom;
        
        this.velocity.x = template.velocityStart.x;
        this.velocity.y = template.velocityStart.y;
        this.velocity.z = template.velocityStart.z;
        
        this.acceleration.x = template.acceleration.x;
        this.acceleration.y = template.acceleration.y;
        this.acceleration.z = template.acceleration.z;
        
        this.friction = template.friction;
        this.bounceRestitution = template.bounceRestitution;
        
        this.trailLength = template.trailLength;
        this.trailData = [];
        
        this.sequences = [...template.sequences];
        this.currentSequence = 0;
        this.sequenceTime = 0;
        
        this.controlPointPositions = [];
        for (let i = 0; i < Math.min(controlPoints.length, 10); i++) {
            this.controlPointPositions.push({ ...controlPoints[i] });
        }
        
        this.userData = {};
        this.collisionCount = 0;
        this.bounces = 0;
        
        this.flags = template.flags;
        
        this.material = template.material;
        this.texture = template.texture;
        this.model = template.model;
        
        this.saveInitialValues();
    }
    
    saveInitialValues() {
        this.initialValues = {
            position: { ...this.position },
            velocity: { ...this.velocity },
            size: { ...this.size },
            color: { ...this.color },
            alpha: this.alpha,
            rotation: this.rotation
        };
    }
    
    update(deltaTime, controlPoints = []) {
        if (!this.active) return;
        
        this.previousPosition.x = this.position.x;
        this.previousPosition.y = this.position.y;
        this.previousPosition.z = this.position.z;
        
        this.age += deltaTime;
        
        if (this.lifetime > 0) {
            this.normalizedAge = Math.min(this.age / this.lifetime, 1.0);
        } else {
            this.normalizedAge = 1.0;
        }
        
        if (this.age >= this.lifetime) {
            this.active = false;
            return;
        }
        
        this.updateVelocity(deltaTime);
        this.updatePosition(deltaTime);
        this.updateSize();
        this.updateColor();
        this.updateRotation(deltaTime);
        this.updateSequence(deltaTime);
        this.updateTrail();
        this.updateOrientation();
        
        this.updateBoundingBox();
    }
    
    updateVelocity(deltaTime) {
        this.velocity.x += this.acceleration.x * deltaTime;
        this.velocity.y += this.acceleration.y * deltaTime;
        this.velocity.z += this.acceleration.z * deltaTime;
        
        const speed = Math.sqrt(
            this.velocity.x * this.velocity.x +
            this.velocity.y * this.velocity.y +
            this.velocity.z * this.velocity.z
        );
        
        if (this.friction > 0 && speed > 0.001) {
            const frictionForce = this.friction * deltaTime;
            const velocityScale = Math.max(0, 1 - frictionForce);
            
            this.velocity.x *= velocityScale;
            this.velocity.y *= velocityScale;
            this.velocity.z *= velocityScale;
        }
    }
    
    updatePosition(deltaTime) {
        this.position.x += this.velocity.x * deltaTime;
        this.position.y += this.velocity.y * deltaTime;
        this.position.z += this.velocity.z * deltaTime;
    }
    
    updateSize() {
        const t = this.normalizedAge;
        
        const sizeCurve = this.template.sizeCurve;
        if (sizeCurve) {
            const sizeValue = this.sampleCurve(sizeCurve, t);
            this.sizeScale = sizeValue;
        } else {
            const start = this.template.sizeStart;
            const end = this.template.sizeEnd;
            
            const sizeEndScale = this.template.sizeEndScale;
            this.sizeScale = 1.0 - t;
            if (sizeEndScale !== 1.0) {
                this.sizeScale = start.x + (end.x - start.x) * this.easeValue(t, this.template.sizeEase);
            }
        }
        
        this.size.x = this.baseSize.x * this.sizeScale;
        this.size.y = this.baseSize.y * this.sizeScale;
        this.size.z = this.baseSize.z * this.sizeScale;
    }
    
    updateColor() {
        const t = this.normalizedAge;
        
        const colorStart = this.template.colorStart;
        const colorEnd = this.template.colorEnd;
        
        const colorEase = this.template.colorEase || 'linear';
        
        const easedT = this.easeValue(t, colorEase);
        
        this.color.r = colorStart.r + (colorEnd.r - colorStart.r) * easedT;
        this.color.g = colorStart.g + (colorEnd.g - colorStart.g) * easedT;
        this.color.b = colorStart.b + (colorEnd.b - colorStart.b) * easedT;
        this.color.a = colorStart.a + (colorEnd.a - colorStart.a) * easedT;
        
        this.alpha = this.color.a / 255;
    }
    
    updateRotation(deltaTime) {
        this.rotation += this.rotationRate * deltaTime;
        
        this.rotationRate += this.template.rotationRateAccel * deltaTime;
    }
    
    updateSequence(deltaTime) {
        if (this.sequences.length === 0) return;
        
        this.sequenceTime += deltaTime;
        
        const currentSeq = this.sequences[this.currentSequence];
        if (currentSeq && this.sequenceTime >= currentSeq.duration) {
            this.sequenceTime = 0;
            this.currentSequence++;
            
            if (this.currentSequence >= this.sequences.length) {
                if (this.template.sequenceLoop) {
                    this.currentSequence = 0;
                } else {
                    this.currentSequence = this.sequences.length - 1;
                }
            }
        }
    }
    
    updateTrail() {
        if (this.trailLength <= 0) return;
        
        this.trailData.unshift({
            position: { ...this.position },
            alpha: this.alpha,
            size: { ...this.size },
            color: { ...this.color }
        });
        
        while (this.trailData.length > this.trailLength) {
            this.trailData.pop();
        }
    }
    
    updateOrientation() {
        if (this.template.type === PARTICLE_TYPE.SPRITE || 
            this.template.renderMode === PARTICLE_RENDER_MODE.BILLBOARD) {
            return;
        }
        
        if (this.template.type === PARTICLE_TYPE.BEAM) {
            return;
        }
        
        if (this.velocity.x !== 0 || this.velocity.y !== 0 || this.velocity.z !== 0) {
            const speed = Math.sqrt(
                this.velocity.x * this.velocity.x +
                this.velocity.y * this.velocity.y +
                this.velocity.z * this.velocity.z
            );
            
            if (speed > 0.001) {
                this.forward.x = this.velocity.x / speed;
                this.forward.y = this.velocity.y / speed;
                this.forward.z = this.velocity.z / speed;
            }
        }
        
        this.up = { x: 0, y: 1, z: 0 };
        
        const dot = this.forward.y;
        if (Math.abs(dot) < 0.999) {
            this.right.x = this.forward.z;
            this.right.y = 0;
            this.right.z = -this.forward.x;
            
            const len = Math.sqrt(this.right.x * this.right.x + this.right.z * this.right.z);
            this.right.x /= len;
            this.right.z /= len;
        } else {
            this.right = { x: 1, y: 0, z: 0 };
        }
        
        this.up.x = this.forward.y * this.right.z - this.forward.z * this.right.y;
        this.up.y = this.forward.z * this.right.x - this.forward.x * this.right.z;
        this.up.z = this.forward.x * this.right.y - this.forward.y * this.right.x;
    }
    
    updateBoundingBox() {
        const halfSize = {
            x: this.size.x * 0.5,
            y: this.size.y * 0.5,
            z: this.size.z * 0.5
        };
        
        this.boundingBox.min.x = this.position.x - halfSize.x;
        this.boundingBox.min.y = this.position.y - halfSize.y;
        this.boundingBox.min.z = this.position.z - halfSize.z;
        
        this.boundingBox.max.x = this.position.x + halfSize.x;
        this.boundingBox.max.y = this.position.y + halfSize.y;
        this.boundingBox.max.z = this.position.z + halfSize.z;
    }
    
    handleCollision(position, normal, restitution = 0.5) {
        const dot = this.velocity.x * normal.x + 
                    this.velocity.y * normal.y + 
                    this.velocity.z * normal.z;
        
        if (dot > 0) return;
        
        this.velocity.x -= (1 + restitution) * dot * normal.x;
        this.velocity.y -= (1 + restitution) * dot * normal.y;
        this.velocity.z -= (1 + restitution) * dot * normal.z;
        
        this.position.x = position.x + normal.x * 0.01;
        this.position.y = position.y + normal.y * 0.01;
        this.position.z = position.z + normal.z * 0.01;
        
        this.bounces++;
        this.collisionCount++;
        
        if (this.template.onCollision) {
            this.template.onCollision(this, position, normal);
        }
    }
    
    applyForce(force, deltaTime) {
        this.velocity.x += force.x * deltaTime;
        this.velocity.y += force.y * deltaTime;
        this.velocity.z += force.z * deltaTime;
    }
    
    applyImpulse(impulse) {
        this.velocity.x += impulse.x;
        this.velocity.y += impulse.y;
        this.velocity.z += impulse.z;
    }
    
    setPosition(x, y, z) {
        this.position.x = x;
        this.position.y = y;
        this.position.z = z;
    }
    
    setVelocity(x, y, z) {
        this.velocity.x = x;
        this.velocity.y = y;
        this.velocity.z = z;
    }
    
    setColor(r, g, b, a = 255) {
        this.color.r = r;
        this.color.g = g;
        this.color.b = b;
        this.color.a = a;
    }
    
    setSize(x, y, z = 1) {
        this.baseSize.x = x;
        this.baseSize.y = y;
        this.baseSize.z = z;
    }
    
    setAlpha(alpha) {
        this.alpha = Math.max(0, Math.min(1, alpha));
        this.color.a = alpha * 255;
    }
    
    setRotation(rotation) {
        this.rotation = rotation;
    }
    
    setLifetime(lifetime) {
        this.lifetime = lifetime;
    }
    
    reset() {
        this.active = false;
        this.age = 0;
        this.normalizedAge = 0;
        
        this.position = { x: 0, y: 0, z: 0 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.acceleration = { x: 0, y: 0, z: 0 };
        
        this.rotation = 0;
        this.rotationRate = 0;
        
        this.size = { x: 1, y: 1, z: 1 };
        this.sizeScale = 1;
        
        this.color = { r: 255, g: 255, b: 255, a: 255 };
        this.alpha = 1;
        
        this.trailData = [];
        this.collisionCount = 0;
        this.bounces = 0;
        
        this.userData = {};
    }
    
    sampleCurve(curve, t) {
        if (!curve || curve.length < 2) return 1.0;
        
        for (let i = 0; i < curve.length - 1; i++) {
            const p0 = curve[Math.max(0, i - 1)];
            const p1 = curve[i];
            const p2 = curve[i + 1];
            const p3 = curve[Math.min(curve.length - 1, i + 2)];
            
            if (t >= p1.t && t <= p2.t) {
                const localT = (t - p1.t) / (p2.t - p1.t);
                return this.catmullRom(p0.v, p1.v, p2.v, p3.v, localT);
            }
        }
        
        return curve[curve.length - 1].v;
    }
    
    catmullRom(p0, p1, p2, p3, t) {
        const t2 = t * t;
        const t3 = t2 * t;
        
        return 0.5 * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3
        );
    }
    
    easeValue(t, ease) {
        if (!ease || ease === 'linear') return t;
        
        switch (ease) {
            case 'easeIn':
                return t * t;
            case 'easeOut':
                return t * (2 - t);
            case 'easeInOut':
                return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
            case 'easeInSine':
                return 1 - Math.cos((t * Math.PI) / 2);
            case 'easeOutSine':
                return Math.sin((t * Math.PI) / 2);
            case 'easeInOutSine':
                return -(Math.cos(Math.PI * t) - 1) / 2;
            case 'easeInQuad':
                return t * t;
            case 'easeOutQuad':
                return t * (2 - t);
            case 'easeInCubic':
                return t * t * t;
            case 'easeOutCubic':
                return (--t) * t * t + 1;
            case 'easeInBounce':
                return 1 - this.easeOutBounce(1 - t);
            case 'easeOutBounce':
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
    
    getSpeed() {
        return Math.sqrt(
            this.velocity.x * this.velocity.x +
            this.velocity.y * this.velocity.y +
            this.velocity.z * this.velocity.z
        );
    }
    
    getDirection() {
        const speed = this.getSpeed();
        if (speed < 0.001) {
            return { x: 0, y: 0, z: 1 };
        }
        
        return {
            x: this.velocity.x / speed,
            y: this.velocity.y / speed,
            z: this.velocity.z / speed
        };
    }
    
    getNormalizedVelocity() {
        return this.getDirection();
    }
    
    getRemainingLifetime() {
        return Math.max(0, this.lifetime - this.age);
    }
    
    isAlive() {
        return this.active && this.age < this.lifetime;
    }
    
    clone() {
        const cloned = new Particle(this.template);
        
        cloned.active = false;
        cloned.age = 0;
        cloned.lifetime = this.lifetime;
        cloned.normalizedAge = 0;
        
        cloned.position = { ...this.position };
        cloned.previousPosition = { ...this.previousPosition };
        cloned.velocity = { ...this.velocity };
        cloned.acceleration = { ...this.acceleration };
        
        cloned.rotation = this.rotation;
        cloned.rotationRate = this.rotationRate;
        cloned.angularVelocity = { ...this.angularVelocity };
        
        cloned.size = { ...this.size };
        cloned.sizeScale = this.sizeScale;
        cloned.baseSize = { ...this.baseSize };
        
        cloned.color = { ...this.color };
        cloned.alpha = this.alpha;
        cloned.baseColor = { ...this.baseColor };
        
        cloned.sequences = [...this.sequences];
        cloned.currentSequence = this.currentSequence;
        cloned.sequenceTime = this.sequenceTime;
        
        cloned.trailLength = this.trailLength;
        cloned.trailData = this.trailData.map(d => ({ ...d }));
        
        cloned.friction = this.friction;
        cloned.bounceRestitution = this.bounceRestitution;
        
        cloned.userData = { ...this.userData };
        cloned.controlPointPositions = this.controlPointPositions.map(p => ({ ...p }));
        
        cloned.material = this.material;
        cloned.texture = this.texture;
        cloned.model = this.model;
        
        return cloned;
    }
}

export class ParticleTemplate {
    constructor(name) {
        this.name = name;
        this.type = PARTICLE_TYPE.SPRITE;
        
        this.flags = 0;
        
        this.lifetime = { min: 1.0, max: 1.0, constant: true };
        this.lifetimeRandom = 0;
        
        this.emissionRate = 100;
        this.emissionRateMin = 0;
        
        this.particleCount = { min: 1, max: 1 };
        
        this.bounds = {
            radius: 10,
            boxMin: { x: -10, y: -10, z: -10 },
            boxMax: { x: 10, y: 10, z: 10 }
        };
        
        this.material = null;
        this.texture = null;
        this.model = null;
        
        this.colorStart = { r: 255, g: 255, b: 255, a: 255 };
        this.colorEnd = { r: 255, g: 255, b: 255, a: 0 };
        this.colorEase = 'linear';
        
        this.sizeStart = { x: 10, y: 10, z: 1 };
        this.sizeEnd = { x: 0, y: 0, z: 1 };
        this.sizeEndScale = 0;
        this.sizeEase = 'linear';
        this.sizeCurve = null;
        
        this.rotationStart = 0;
        this.rotationRandom = 0;
        this.rotationEnd = 0;
        this.rotationRateStart = 0;
        this.rotationRateRandom = 0;
        this.rotationRateAccel = 0;
        
        this.velocityStart = { x: 0, y: 0, z: 0 };
        this.velocityEnd = { x: 0, y: 0, z: 0 };
        this.velocityRandom = { x: 0, y: 0, z: 0 };
        this.acceleration = { x: 0, y: 0, z: 0 };
        this.gravity = { x: 0, y: 0, z: 0 };
        
        this.spread = { x: 0, y: 0, z: 0 };
        this.spreadRandom = { x: 0, y: 0, z: 0 };
        
        this.friction = 0;
        this.bounceRestitution = 0.5;
        
        this.renderMode = PARTICLE_RENDER_MODE.BILLBOARD;
        this.blendMode = PARTICLE_BLEND_MODE.ALPHA;
        
        this.sortMode = PARTICLE_SORT_MODE.NONE;
        
        this.sequences = [];
        this.sequenceLoop = false;
        
        this.trailLength = 0;
        this.trailFade = 0;
        this.trailColorFade = false;
        
        this.collisionMode = PARTICLE_COLLISION_MODE.NONE;
        this.collisionRadius = 1;
        this.collisionRestitution = 0.5;
        this.collisionFade = true;
        
        this.noiseEnabled = false;
        this.noiseScale = 1;
        this.noiseStrength = { x: 0, y: 0, z: 0 };
        this.noiseFrequency = 1;
        this.noiseOctaves = 1;
        this.noisePersistence = 0.5;
        this.noiseSeed = 0;
        
        this.fieldColors = [];
        this.fieldForces = [];
        this.fieldSizes = [];
        
        this.children = [];
        this.childTemplates = [];
        
        this.materialParams = {};
        
        this.onCollision = null;
        this.onDeath = null;
        this.onSpawn = null;
    }
    
    setLifetime(min, max = min) {
        this.lifetime.min = min;
        this.lifetime.max = max;
        this.lifetime.constant = min === max;
    }
    
    setEmissionRate(rate, rateMin = 0) {
        this.emissionRate = rate;
        this.emissionRateMin = rateMin;
    }
    
    setParticleCount(min, max = min) {
        this.particleCount.min = min;
        this.particleCount.max = max;
    }
    
    setColor(r, g, b, a = 255, r2 = r, g2 = g, b2 = b, a2 = 0) {
        this.colorStart = { r, g, b, a };
        this.colorEnd = { r: r2, g: g2, b: b2, a: a2 };
    }
    
    setSize(width, height, depth = 1, widthEnd = 0, heightEnd = 0, depthEnd = 1) {
        this.sizeStart = { x: width, y: height, z: depth };
        this.sizeEnd = { x: widthEnd, y: heightEnd, z: depthEnd };
        this.sizeEndScale = 1;
    }
    
    setSizeCurve(curve) {
        this.sizeCurve = curve;
    }
    
    setVelocity(x, y, z, x2 = 0, y2 = 0, z2 = 0) {
        this.velocityStart = { x, y, z };
        this.velocityEnd = { x: x2, y: y2, z: z2 };
    }
    
    setVelocityRandom(x, y, z) {
        this.velocityRandom = { x, y, z };
    }
    
    setAcceleration(x, y, z) {
        this.acceleration = { x, y, z };
    }
    
    setGravity(x, y, z) {
        this.gravity = { x, y, z };
        this.acceleration.y -= y;
    }
    
    setSpread(x, y, z) {
        this.spread = { x, y, z };
    }
    
    setSpreadRandom(x, y, z) {
        this.spreadRandom = { x, y, z };
    }
    
    setRotation(start, random = 0, end = 0) {
        this.rotationStart = start;
        this.rotationRandom = random;
        this.rotationEnd = end;
    }
    
    setRotationRate(start, random = 0, accel = 0) {
        this.rotationRateStart = start;
        this.rotationRateRandom = random;
        this.rotationRateAccel = accel;
    }
    
    setFriction(friction) {
        this.friction = friction;
    }
    
    setBounceRestitution(restitution) {
        this.bounceRestitution = restitution;
    }
    
    setRenderMode(mode) {
        this.renderMode = mode;
    }
    
    setBlendMode(mode) {
        this.blendMode = mode;
    }
    
    setSortMode(mode) {
        this.sortMode = mode;
    }
    
    addSequence(duration, startFrame = 0, endFrame = -1, rate = 30) {
        this.sequences.push({
            duration,
            startFrame,
            endFrame,
            rate,
            frameCount: endFrame >= 0 ? endFrame - startFrame + 1 : 0
        });
    }
    
    setTrail(length, fade = 0, colorFade = false) {
        this.trailLength = length;
        this.trailFade = fade;
        this.trailColorFade = colorFade;
    }
    
    setCollision(mode, radius = 1, restitution = 0.5, fade = true) {
        this.collisionMode = mode;
        this.collisionRadius = radius;
        this.collisionRestitution = restitution;
        this.collisionFade = fade;
    }
    
    enableNoise(scale = 1, strength = { x: 0, y: 0, z: 0 }, frequency = 1) {
        this.noiseEnabled = true;
        this.noiseScale = scale;
        this.noiseStrength = strength;
        this.noiseFrequency = frequency;
    }
    
    addChild(template, attachToBone = null, copyRotation = true, copyPosition = false) {
        const child = {
            template,
            attachToBone,
            copyRotation,
            copyPosition,
            emissionRateScale: 1.0,
            offset: { x: 0, y: 0, z: 0 }
        };
        
        this.children.push(child);
        return child;
    }
    
    setMaterialParams(params) {
        this.materialParams = { ...this.materialParams, ...params };
    }
    
    getLifetime() {
        if (this.lifetime.constant) {
            return this.lifetime.min;
        }
        return this.lifetime.min + Math.random() * (this.lifetime.max - this.lifetime.min);
    }
    
    getVelocity(lifetime, normalizedAge) {
        const t = normalizedAge;
        
        return {
            x: this.velocityStart.x + (this.velocityEnd.x - this.velocityStart.x) * t,
            y: this.velocityStart.y + (this.velocityEnd.y - this.velocityStart.y) * t,
            z: this.velocityStart.z + (this.velocityEnd.z - this.velocityStart.z) * t
        };
    }
    
    getRandomSpread() {
        return {
            x: (Math.random() - 0.5) * 2 * (this.spread.x + this.spreadRandom.x * Math.random()),
            y: (Math.random() - 0.5) * 2 * (this.spread.y + this.spreadRandom.y * Math.random()),
            z: (Math.random() - 0.5) * 2 * (this.spread.z + this.spreadRandom.z * Math.random())
        };
    }
    
    clone() {
        const cloned = new ParticleTemplate(this.name);
        
        Object.assign(cloned, this);
        cloned.children = this.children.map(c => ({ ...c }));
        cloned.fieldColors = this.fieldColors.map(f => ({ ...f }));
        cloned.fieldForces = this.fieldForces.map(f => ({ ...f }));
        cloned.fieldSizes = this.fieldSizes.map(s => ({ ...s }));
        cloned.sequences = this.sequences.map(s => ({ ...s }));
        
        return cloned;
    }
}

export class ParticleEmitter {
    constructor(template, position = { x: 0, y: 0, z: 0 }, direction = { x: 0, y: 0, z: 1 }) {
        this.template = template;
        this.position = { ...position };
        this.direction = { ...direction };
        this.up = { x: 0, y: 1, z: 0 };
        
        this.active = true;
        this.visible = true;
        
        this.emissionAccumulator = 0;
        this.particleAccumulator = 0;
        
        this.emissionRate = template.emissionRate;
        this.emissionMultiplier = 1.0;
        
        this.particles = [];
        this.activeParticles = 0;
        this.maxParticles = 1000;
        
        this.controlPoints = new Array(10);
        for (let i = 0; i < 10; i++) {
            this.controlPoints[i] = { x: 0, y: 0, z: 0 };
        }
        
        this.startTime = 0;
        this.currentTime = 0;
        this.duration = -1;
        this.loop = true;
        
        this.owner = null;
        this.attachment = null;
        
        this.children = [];
        
        this.boundingBox = {
            min: { x: -100, y: -100, z: -100 },
            max: { x: 100, y: 100, z: 100 }
        };
    }
    
    setPosition(x, y, z) {
        this.position.x = x;
        this.position.y = y;
        this.position.z = z;
    }
    
    setDirection(x, y, z) {
        const len = Math.sqrt(x * x + y * y + z * z);
        if (len > 0.001) {
            this.direction.x = x / len;
            this.direction.y = y / len;
            this.direction.z = z / len;
        }
    }
    
    setControlPoint(index, x, y, z) {
        if (index >= 0 && index < 10) {
            this.controlPoints[index].x = x;
            this.controlPoints[index].y = y;
            this.controlPoints[index].z = z;
        }
    }
    
    setEmissionRate(rate) {
        this.emissionRate = rate;
    }
    
    setEmissionMultiplier(multiplier) {
        this.emissionMultiplier = multiplier;
    }
    
    setVisible(visible) {
        this.visible = visible;
    }
    
    start(duration = -1, loop = true) {
        this.active = true;
        this.startTime = this.currentTime;
        this.duration = duration;
        this.loop = loop;
        this.emissionAccumulator = 0;
        this.particleAccumulator = 0;
    }
    
    stop(immediate = false) {
        if (immediate) {
            this.active = false;
            this.particles = [];
            this.activeParticles = 0;
        } else {
            this.emissionRate = 0;
        }
    }
    
    pause() {
        this.active = false;
    }
    
    resume() {
        this.active = true;
    }
    
    emit(count = 1) {
        for (let i = 0; i < count; i++) {
            if (this.activeParticles >= this.maxParticles) {
                break;
            }
            
            const particle = this.createParticle();
            if (particle) {
                this.particles.push(particle);
                this.activeParticles++;
            }
        }
    }
    
    createParticle() {
        const particle = new Particle(this.template);
        
        particle.setPosition(this.position.x, this.position.y, this.position.z);
        
        const baseVelocity = this.template.getVelocity(particle.lifetime, 0);
        const spread = this.template.getRandomSpread();
        
        const forwardComponent = {
            x: this.direction.x * baseVelocity.z,
            y: this.direction.y * baseVelocity.z,
            z: this.direction.z * baseVelocity.z
        };
        
        const spreadComponent = {
            x: this.up.y * spread.y - this.up.z * spread.x,
            y: this.up.z * spread.x - this.up.x * spread.z,
            z: this.up.x * spread.y - this.up.y * spread.z
        };
        
        particle.setVelocity(
            forwardComponent.x + spreadComponent.x + baseVelocity.x,
            forwardComponent.y + spreadComponent.y + baseVelocity.y,
            forwardComponent.z + spreadComponent.z + baseVelocity.z
        );
        
        const rotation = this.template.rotationStart + 
                        (Math.random() - 0.5) * this.template.rotationRandom;
        particle.setRotation(rotation);
        
        particle.rotationRate = this.template.rotationRateStart + 
                                (Math.random() - 0.5) * this.template.rotationRateRandom;
        
        particle.setLifetime(this.template.getLifetime());
        
        if (this.template.onSpawn) {
            this.template.onSpawn(particle, this);
        }
        
        return particle;
    }
    
    update(deltaTime) {
        if (!this.active) {
            this.updateParticles(deltaTime);
            return;
        }
        
        this.currentTime += deltaTime;
        
        if (this.duration > 0 && this.currentTime - this.startTime >= this.duration) {
            if (this.loop) {
                this.startTime = this.currentTime;
                this.particleAccumulator = 0;
            } else {
                this.stop(true);
                return;
            }
        }
        
        const effectiveRate = this.emissionRate * this.emissionMultiplier;
        
        if (effectiveRate > 0) {
            this.emissionAccumulator += effectiveRate * deltaTime;
            
            const particlesToEmit = Math.floor(this.emissionAccumulator);
            this.emissionAccumulator -= particlesToEmit;
            
            if (particlesToEmit > 0) {
                this.emit(particlesToEmit);
            }
        }
        
        if (this.template.particleCount.min > 1 || this.template.particleCount.max > 1) {
            this.particleAccumulator += deltaTime;
            
            const countRange = this.template.particleCount.max - this.template.particleCount.min;
            const targetCount = this.template.particleCount.min + Math.floor(Math.random() * (countRange + 1));
            
            while (this.activeParticles < targetCount && this.activeParticles < this.maxParticles) {
                const particle = this.createParticle();
                if (particle) {
                    this.particles.push(particle);
                    this.activeParticles++;
                }
            }
        }
        
        this.updateParticles(deltaTime);
        
        this.updateChildren(deltaTime);
        
        this.updateBoundingBox();
    }
    
    updateParticles(deltaTime) {
        for (let i = this.particles.length - 1; i >= 0; i--) {
            const particle = this.particles[i];
            
            if (!particle.active) {
                this.particles.splice(i, 1);
                this.activeParticles--;
                continue;
            }
            
            if (this.template.gravity.y !== 0) {
                particle.applyForce({
                    x: this.template.gravity.x,
                    y: this.template.gravity.y,
                    z: this.template.gravity.z
                }, deltaTime);
            }
            
            particle.update(deltaTime, this.controlPoints);
            
            if (this.template.noiseEnabled) {
                this.applyNoise(particle, deltaTime);
            }
            
            if (!particle.isAlive()) {
                if (this.template.onDeath) {
                    this.template.onDeath(particle, this);
                }
                
                this.spawnChildren(particle);
                
                this.particles.splice(i, 1);
                this.activeParticles--;
            }
        }
    }
    
    applyNoise(particle, deltaTime) {
        const time = this.currentTime * this.template.noiseFrequency;
        
        const noiseX = this.simplexNoise3D(
            particle.position.x * this.template.noiseScale,
            particle.position.y * this.template.noiseScale,
            time
        );
        
        const noiseY = this.simplexNoise3D(
            time,
            particle.position.y * this.template.noiseScale,
            particle.position.z * this.template.noiseScale
        );
        
        const noiseZ = this.simplexNoise3D(
            particle.position.z * this.template.noiseScale,
            time,
            particle.position.x * this.template.noiseScale
        );
        
        particle.applyForce({
            x: noiseX * this.template.noiseStrength.x,
            y: noiseY * this.template.noiseStrength.y,
            z: noiseZ * this.template.noiseStrength.z
        }, deltaTime);
    }
    
    simplexNoise3D(x, y, z) {
        return Math.sin(x * 1.0) * Math.cos(y * 1.0) * Math.sin(z * 1.0) +
               Math.sin(x * 2.3) * Math.cos(y * 2.3) * Math.sin(z * 2.3) * 0.5 +
               Math.sin(x * 3.1) * Math.cos(y * 3.1) * Math.sin(z * 3.1) * 0.25;
    }
    
    updateBoundingBox() {
        if (this.particles.length === 0) {
            this.boundingBox.min = { x: -100, y: -100, z: -100 };
            this.boundingBox.max = { x: 100, y: 100, z: 100 };
            return;
        }
        
        let minX = Infinity, minY = Infinity, minZ = Infinity;
        let maxX = -Infinity, maxY = -Infinity, maxZ = -Infinity;
        
        for (const particle of this.particles) {
            if (!particle.active) continue;
            
            minX = Math.min(minX, particle.boundingBox.min.x);
            minY = Math.min(minY, particle.boundingBox.min.y);
            minZ = Math.min(minZ, particle.boundingBox.min.z);
            
            maxX = Math.max(maxX, particle.boundingBox.max.x);
            maxY = Math.max(maxY, particle.boundingBox.max.y);
            maxZ = Math.max(maxZ, particle.boundingBox.max.z);
        }
        
        this.boundingBox.min = { x: minX, y: minY, z: minZ };
        this.boundingBox.max = { x: maxX, y: maxY, z: maxZ };
    }
    
    spawnChildren(parentParticle) {
        for (const child of this.children) {
            if (child.emissionRateScale <= 0) continue;
            
            if (child.template.emissionRate > 0) {
                const childEmitter = new ParticleEmitter(
                    child.template,
                    parentParticle.position,
                    parentParticle.getDirection()
                );
                
                childEmitter.emissionMultiplier = child.emissionRateScale;
                childEmitter.setControlPoint(0, parentParticle.position.x, parentParticle.position.y, parentParticle.position.z);
                
                childEmitter.emit(1);
            }
        }
    }
    
    updateChildren(deltaTime) {
        for (const child of this.children) {
            if (child.emitter) {
                child.emitter.update(deltaTime);
            }
        }
    }
    
    addChild(template, emissionRateScale = 1.0) {
        const child = {
            template,
            emitter: new ParticleEmitter(template, this.position, this.direction),
            emissionRateScale
        };
        
        child.emitter.owner = this;
        this.children.push(child);
        
        return child;
    }
    
    removeChild(index) {
        if (index >= 0 && index < this.children.length) {
            this.children.splice(index, 1);
        }
    }
    
    clearParticles() {
        this.particles = [];
        this.activeParticles = 0;
        
        for (const child of this.children) {
            if (child.emitter) {
                child.emitter.clearParticles();
            }
        }
    }
    
    setMaxParticles(max) {
        this.maxParticles = max;
        
        while (this.particles.length > max) {
            this.particles.pop();
            this.activeParticles--;
        }
    }
    
    getActiveParticleCount() {
        return this.activeParticles;
    }
    
    isActive() {
        return this.active || this.activeParticles > 0;
    }
}

export class ParticleSystem {
    constructor(maxParticles = 10000) {
        this.maxParticles = maxParticles;
        this.activeParticles = 0;
        this.emitters = [];
        this.templates = new Map();
        this.particlesByType = new Map();
        
        this.active = true;
        this.visible = true;
        
        this.globalSpeedScale = 1.0;
        this.globalSizeScale = 1.0;
        this.globalAlphaScale = 1.0;
        
        this.sortMode = PARTICLE_SORT_MODE.NONE;
        this.sortDirty = false;
        
        this.renderQueue = [];
        this.culledEmitters = [];
        
        this.performanceStats = {
            totalParticles: 0,
            drawCalls: 0,
            updateTime: 0,
            renderTime: 0
        };
        
        this.lodDistances = [0, 100, 300, 500, 1000];
        this.currentLOD = 0;
        
        this.collisionWorld = null;
        
        this.boundingBox = {
            min: { x: -500, y: -500, z: -500 },
            max: { x: 500, y: 500, z: 500 }
        };
        
        this.initializeBuiltInTemplates();
    }
    
    initializeBuiltInTemplates() {
        this.createMuzzleFlashTemplate();
        this.createBloodTemplate();
        this.createBulletImpactTemplate();
        this.createExplosionTemplate();
        this.createSmokeTemplate();
        this.createFireTemplate();
        this.createSparkTemplate();
        this.createDebrisTemplate();
        this.createDustTemplate();
        this.createShellCasingTemplate();
        this.createTracerTemplate();
        this.createWaterSplashTemplate();
        this.createGlassShardTemplate();
        this.createFootprintTemplate();
        this.createGrenadeSmokeTemplate();
        this.createFlashbangEffectTemplate();
        this.createMolotovFireTemplate();
    }
    
    createMuzzleFlashTemplate() {
        const template = new ParticleTemplate('muzzle_flash');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.05, 0.1);
        template.setEmissionRate(0);
        template.setParticleCount(1, 3);
        template.setSize(15, 15, 1, 5, 5, 1);
        template.setColor(255, 220, 100, 255, 255, 150, 50, 0);
        template.setVelocity(0, 0, 200, 0, 0, 400);
        template.setSpread(0.2, 0.2, 0);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ADDITIVE);
        this.templates.set('muzzle_flash', template);
    }
    
    createBloodTemplate() {
        const template = new ParticleTemplate('blood');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.5, 1.5);
        template.setParticleCount(3, 8);
        template.setSize(3, 3, 1, 1, 1, 1);
        template.setColor(180, 20, 20, 255, 100, 10, 10, 0);
        template.setVelocity(50, 100, 150, 20, 200, 300);
        template.setGravity(0, 400, 0);
        template.setSpread(0.5, 0.3, 0.5);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        this.templates.set('blood', template);
    }
    
    createBulletImpactTemplate() {
        const template = new ParticleTemplate('bullet_impact');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.2, 0.5);
        template.setParticleCount(5, 12);
        template.setSize(2, 2, 1, 0.5, 0.5, 1);
        template.setColor(200, 180, 150, 255, 150, 130, 100, 0);
        template.setVelocity(100, 200, 250, 50, 150, 200);
        template.setGravity(0, 300, 0);
        template.setSpread(0.8, 0.5, 0.8);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        template.addChild(this.templates.get('blood') || this.createBloodTemplate());
        this.templates.set('bullet_impact', template);
    }
    
    createExplosionTemplate() {
        const template = new ParticleTemplate('explosion');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.3, 0.8);
        template.setParticleCount(20, 40);
        template.setSize(30, 30, 1, 5, 5, 1);
        template.setColor(255, 200, 100, 255, 255, 100, 50, 0);
        template.setVelocity(100, 300, 400, 50, 200, 300);
        template.setSpread(1, 0.5, 1);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ADDITIVE);
        template.setGravity(0, 100, 0);
        this.templates.set('explosion', template);
    }
    
    createSmokeTemplate() {
        const template = new ParticleTemplate('smoke');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(2, 4);
        template.setParticleCount(10, 20);
        template.setSize(20, 20, 1, 60, 60, 1);
        template.setColor(100, 100, 100, 100, 80, 80, 80, 0);
        template.setVelocity(20, 50, 30, 10, 30, 20);
        template.setSpread(0.3, 0.1, 0.3);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        template.setSizeCurve([
            { t: 0, v: 0.3 },
            { t: 0.3, v: 0.6 },
            { t: 0.7, v: 1.0 },
            { t: 1.0, v: 1.5 }
        ]);
        this.templates.set('smoke', template);
    }
    
    createFireTemplate() {
        const template = new ParticleTemplate('fire');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.3, 0.6);
        template.setParticleCount(15, 25);
        template.setSize(10, 10, 1, 5, 5, 1);
        template.setColor(255, 100, 20, 255, 200, 50, 10, 0);
        template.setVelocity(0, 80, 0, 0, 150, 0);
        template.setSpread(0.2, 0, 0.2);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ADDITIVE);
        this.templates.set('fire', template);
    }
    
    createSparkTemplate() {
        const template = new ParticleTemplate('spark');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.1, 0.4);
        template.setParticleCount(5, 15);
        template.setSize(1, 4, 1, 0.5, 1, 1);
        template.setColor(255, 255, 200, 255, 255, 200, 100, 0);
        template.setVelocity(200, 400, 500, 100, 300, 400);
        template.setGravity(0, 500, 0);
        template.setSpread(1, 0.5, 1);
        template.setRenderMode(PARTICLE_RENDER_MODE.ORIENTED);
        template.setBlendMode(PARTICLE_BLEND_MODE.ADDITIVE);
        this.templates.set('spark', template);
    }
    
    createDebrisTemplate() {
        const template = new ParticleTemplate('debris');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.5, 1.5);
        template.setParticleCount(5, 10);
        template.setSize(3, 5, 1, 1, 2, 1);
        template.setColor(150, 140, 130, 255, 100, 90, 80, 0);
        template.setVelocity(100, 250, 300, 50, 150, 200);
        template.setGravity(0, 600, 0);
        template.setSpread(0.8, 0.3, 0.8);
        template.setRotation(0, 360, 0);
        template.setRotationRate(0, 500, 0);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        template.setCollision(PARTICLE_COLLISION_MODE.SPHERE, 0.5, 0.3);
        this.templates.set('debris', template);
    }
    
    createDustTemplate() {
        const template = new ParticleTemplate('dust');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.5, 1.2);
        template.setParticleCount(8, 15);
        template.setSize(10, 15, 1, 25, 30, 1);
        template.setColor(180, 160, 140, 150, 150, 130, 120, 0);
        template.setVelocity(30, 60, 40, 10, 40, 30);
        template.setSpread(0.5, 0.2, 0.5);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        this.templates.set('dust', template);
    }
    
    createShellCasingTemplate() {
        const template = new ParticleTemplate('shell_casing');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.8, 1.2);
        template.setParticleCount(1, 1);
        template.setSize(2, 6, 1, 2, 6, 1);
        template.setColor(200, 180, 100, 255, 180, 160, 80, 0);
        template.setVelocity(100, 200, 150, 80, 150, 100);
        template.setGravity(0, 500, 0);
        template.setSpread(0.3, 0.2, 0.3);
        template.setRotation(0, 720, 0);
        template.setRotationRate(0, 1000, 0);
        template.setRenderMode(PARTICLE_RENDER_MODE.ORIENTED);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        template.setCollision(PARTICLE_COLLISION_MODE.SPHERE, 0.3, 0.4);
        this.templates.set('shell_casing', template);
    }
    
    createTracerTemplate() {
        const template = new ParticleTemplate('tracer');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.1, 0.2);
        template.setParticleCount(1, 1);
        template.setSize(2, 20, 1, 1, 10, 1);
        template.setColor(255, 255, 200, 255, 255, 200, 100, 200);
        template.setVelocity(0, 0, 2000, 0, 0, 2500);
        template.setRenderMode(PARTICLE_RENDER_MODE.ORIENTED);
        template.setBlendMode(PARTICLE_BLEND_MODE.ADDITIVE);
        this.templates.set('tracer', template);
    }
    
    createWaterSplashTemplate() {
        const template = new ParticleTemplate('water_splash');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.3, 0.6);
        template.setParticleCount(10, 20);
        template.setSize(2, 4, 1, 1, 2, 1);
        template.setColor(150, 180, 255, 200, 100, 150, 200, 0);
        template.setVelocity(50, 150, 100, 20, 100, 50);
        template.setGravity(0, 400, 0);
        template.setSpread(0.6, 0.3, 0.6);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        this.templates.set('water_splash', template);
    }
    
    createGlassShardTemplate() {
        const template = new ParticleTemplate('glass_shard');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(1.0, 2.0);
        template.setParticleCount(5, 15);
        template.setSize(2, 5, 1, 1, 2, 1);
        template.setColor(200, 220, 255, 150, 180, 200, 220, 0);
        template.setVelocity(80, 200, 250, 40, 150, 200);
        template.setGravity(0, 400, 0);
        template.setSpread(0.8, 0.3, 0.8);
        template.setRotation(0, 360, 0);
        template.setRotationRate(0, 300, 0);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        template.setCollision(PARTICLE_COLLISION_MODE.SPHERE, 0.2, 0.2);
        this.templates.set('glass_shard', template);
    }
    
    createFootprintTemplate() {
        const template = new ParticleTemplate('footprint');
        template.type = PARTICLE_TYPE.DECAL;
        template.setLifetime(10, 30);
        template.setParticleCount(1, 1);
        template.setSize(3, 8, 1, 3, 8, 1);
        template.setColor(80, 60, 50, 100, 60, 40, 30, 50);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.MULTIPLY);
        this.templates.set('footprint', template);
    }
    
    createGrenadeSmokeTemplate() {
        const template = new ParticleTemplate('grenade_smoke');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(8, 12);
        template.setParticleCount(2, 4);
        template.setSize(40, 40, 1, 80, 80, 1);
        template.setColor(150, 150, 150, 150, 120, 120, 120, 0);
        template.setVelocity(0, 30, 0, 0, 50, 0);
        template.setSpread(0.3, 0, 0.3);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ALPHA);
        template.setSizeCurve([
            { t: 0, v: 0.5 },
            { t: 0.2, v: 1.0 },
            { t: 0.8, v: 1.5 },
            { t: 1.0, v: 2.0 }
        ]);
        this.templates.set('grenade_smoke', template);
    }
    
    createFlashbangEffectTemplate() {
        const template = new ParticleTemplate('flashbang_effect');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(0.3, 0.8);
        template.setParticleCount(1, 1);
        template.setSize(200, 200, 1, 400, 400, 1);
        template.setColor(255, 255, 255, 255, 255, 255, 255, 0);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ADDITIVE);
        template.setAlpha(0.8);
        this.templates.set('flashbang_effect', template);
    }
    
    createMolotovFireTemplate() {
        const template = new ParticleTemplate('molotov_fire');
        template.type = PARTICLE_TYPE.SPRITE;
        template.setLifetime(1.5, 3.0);
        template.setParticleCount(5, 10);
        template.setSize(15, 20, 1, 30, 40, 1);
        template.setColor(255, 80, 20, 255, 200, 50, 10, 0);
        template.setVelocity(0, 40, 0, 0, 80, 0);
        template.setSpread(0.4, 0.2, 0.4);
        template.setRenderMode(PARTICLE_RENDER_MODE.BILLBOARD);
        template.setBlendMode(PARTICLE_BLEND_MODE.ADDITIVE);
        template.setSizeCurve([
            { t: 0, v: 0.3 },
            { t: 0.5, v: 1.0 },
            { t: 1.0, v: 1.5 }
        ]);
        this.templates.set('molotov_fire', template);
    }
    
    addTemplate(name, template) {
        this.templates.set(name, template);
        return template;
    }
    
    getTemplate(name) {
        return this.templates.get(name) || null;
    }
    
    createEmitter(templateName, position, direction) {
        const template = this.templates.get(templateName);
        if (!template) {
            console.warn(`Particle template '${templateName}' not found`);
            return null;
        }
        
        const emitter = new ParticleEmitter(template, position, direction);
        emitter.system = this;
        
        this.emitters.push(emitter);
        
        return emitter;
    }
    
    createEmitterFromTemplate(template, position, direction) {
        const emitter = new ParticleEmitter(template, position, direction);
        emitter.system = this;
        
        this.emitters.push(emitter);
        
        return emitter;
    }
    
    removeEmitter(emitter) {
        const index = this.emitters.indexOf(emitter);
        if (index !== -1) {
            this.emitters.splice(index, 1);
        }
    }
    
    emit(templateName, position, direction, count = 1) {
        const emitter = this.createEmitter(templateName, position, direction);
        if (emitter) {
            emitter.emit(count);
            emitter.stop(true);
            return emitter;
        }
        return null;
    }
    
    update(deltaTime) {
        const scaledDelta = deltaTime * this.globalSpeedScale;
        
        const startTime = performance.now();
        
        for (const emitter of this.emitters) {
            if (emitter.visible) {
                emitter.update(scaledDelta);
            }
        }
        
        this.activeParticles = 0;
        for (const emitter of this.emitters) {
            this.activeParticles += emitter.activeParticles;
        }
        
        if (this.sortDirty) {
            this.sortParticles();
            this.sortDirty = false;
        }
        
        this.performanceStats.updateTime = performance.now() - startTime;
    }
    
    render(renderer, camera) {
        if (!this.visible) return;
        
        const startTime = performance.now();
        
        this.renderQueue = [];
        
        for (const emitter of this.emitters) {
            if (!emitter.visible || !emitter.active) continue;
            
            if (this.shouldCull(emitter, camera)) continue;
            
            for (const particle of emitter.particles) {
                if (!particle.active) continue;
                
                particle.sortDistance = this.calculateDistance(particle.position, camera.position);
                particle.renderOrder = emitter.emitters.indexOf(emitter);
                
                this.renderQueue.push({ particle, emitter });
            }
        }
        
        switch (this.sortMode) {
            case PARTICLE_SORT_MODE.BACK_TO_FRONT:
                this.renderQueue.sort((a, b) => b.particle.sortDistance - a.particle.sortDistance);
                break;
            case PARTICLE_SORT_MODE.FRONT_TO_BACK:
                this.renderQueue.sort((a, b) => a.particle.sortDistance - b.particle.sortDistance);
                break;
            case PARTICLE_SORT_MODE.BY_DISTANCE:
                this.renderQueue.sort((a, b) => a.particle.sortDistance - b.particle.sortDistance);
                break;
        }
        
        for (const item of this.renderQueue) {
            this.renderParticle(renderer, item.particle, item.emitter, camera);
        }
        
        this.performanceStats.drawCalls = this.renderQueue.length;
        this.performanceStats.renderTime = performance.now() - startTime;
    }
    
    shouldCull(emitter, camera) {
        if (!camera) return false;
        
        const center = {
            x: (emitter.boundingBox.min.x + emitter.boundingBox.max.x) * 0.5,
            y: (emitter.boundingBox.min.y + emitter.boundingBox.max.y) * 0.5,
            z: (emitter.boundingBox.min.z + emitter.boundingBox.max.z) * 0.5
        };
        
        const extent = {
            x: (emitter.boundingBox.max.x - emitter.boundingBox.min.x) * 0.5,
            y: (emitter.boundingBox.max.y - emitter.boundingBox.min.y) * 0.5,
            z: (emitter.boundingBox.max.z - emitter.boundingBox.min.z) * 0.5
        };
        
        const distance = this.calculateDistance(center, camera.position);
        
        return distance > extent.x + extent.y + extent.z + 1000;
    }
    
    calculateDistance(pos1, pos2) {
        const dx = pos1.x - pos2.x;
        const dy = pos1.y - pos2.y;
        const dz = pos1.z - pos2.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    renderParticle(renderer, particle, emitter, camera) {
        if (!particle.active) return;
        
        const template = particle.template;
        const type = template.type;
        
        switch (type) {
            case PARTICLE_TYPE.SPRITE:
                this.renderSprite(renderer, particle, camera);
                break;
            case PARTICLE_TYPE.DECAL:
                this.renderDecal(renderer, particle, camera);
                break;
            case PARTICLE_TYPE.BEAM:
                this.renderBeam(renderer, particle, camera);
                break;
            case PARTICLE_TYPE.ROPE:
                this.renderRope(renderer, particle, camera);
                break;
            case PARTICLE_TYPE.TRAIL:
                this.renderTrail(renderer, particle, camera);
                break;
            default:
                this.renderSprite(renderer, particle, camera);
                break;
        }
    }
    
    renderSprite(renderer, particle, camera) {
        const template = particle.template;
        
        const sizeX = particle.size.x * this.globalSizeScale;
        const sizeY = particle.size.y * this.globalSizeScale;
        const alpha = particle.alpha * this.globalAlphaScale;
        
        if (alpha <= 0) return;
        
        let position = particle.position;
        
        if (template.renderMode === PARTICLE_RENDER_MODE.BILLBOARD) {
            if (camera) {
                const dx = particle.position.x - camera.position.x;
                const dy = particle.position.y - camera.position.y;
                const dz = particle.position.z - camera.position.z;
                
                const angle = Math.atan2(dx, dz);
                
                position = {
                    x: particle.position.x,
                    y: particle.position.y,
                    z: particle.position.z
                };
                
                particle.orientation = this.calculateBillboardOrientation(camera, position);
            }
        }
        
        const vertices = this.generateSpriteVertices(position, sizeX, sizeY, particle.orientation, particle.rotation);
        
        const color = {
            r: particle.color.r / 255,
            g: particle.color.g / 255,
            b: particle.color.b / 255,
            a: alpha
        };
        
        renderer.drawParticle(
            vertices,
            color,
            template.texture,
            template.blendMode,
            template.renderMode
        );
    }
    
    generateSpriteVertices(position, sizeX, sizeY, orientation, rotation) {
        const cos = Math.cos(rotation);
        const sin = Math.sin(rotation);
        
        const halfX = sizeX * 0.5;
        const halfY = sizeY * 0.5;
        
        const corners = [
            { x: -halfX, y: -halfY, u: 0, v: 0 },
            { x: halfX, y: -halfY, u: 1, v: 0 },
            { x: halfX, y: halfY, u: 1, v: 1 },
            { x: -halfX, y: halfY, u: 0, v: 1 }
        ];
        
        const rotated = corners.map(corner => {
            return {
                x: corner.x * cos - corner.y * sin,
                y: corner.x * sin + corner.y * cos,
                u: corner.u,
                v: corner.v
            };
        });
        
        return rotated.map(corner => ({
            x: position.x + corner.x,
            y: position.y + corner.y,
            z: position.z,
            u: corner.u,
            v: corner.v
        }));
    }
    
    calculateBillboardOrientation(camera, position) {
        const forward = {
            x: camera.position.x - position.x,
            y: camera.position.y - position.y,
            z: camera.position.z - position.z
        };
        
        const len = Math.sqrt(forward.x * forward.x + forward.y * forward.y + forward.z * forward.z);
        if (len > 0.001) {
            forward.x /= len;
            forward.y /= len;
            forward.z /= len;
        }
        
        return {
            forward,
            up: { x: 0, y: 1, z: 0 },
            right: { x: 1, y: 0, z: 0 }
        };
    }
    
    renderDecal(renderer, particle, camera) {
        const position = particle.position;
        const size = particle.size;
        const normal = particle.getDirection();
        
        const color = {
            r: particle.color.r / 255,
            g: particle.color.g / 255,
            b: particle.color.b / 255,
            a: particle.alpha * this.globalAlphaScale
        };
        
        renderer.drawDecal(position, size, normal, color, particle.template.texture);
    }
    
    renderBeam(renderer, particle, camera) {
        if (particle.trailData.length < 2) return;
        
        const points = particle.trailData.map(t => t.position);
        
        const color = {
            r: particle.color.r / 255,
            g: particle.color.g / 255,
            b: particle.color.b / 255,
            a: particle.alpha * this.globalAlphaScale
        };
        
        renderer.drawBeam(points, particle.size.x, color, particle.template.texture);
    }
    
    renderRope(renderer, particle, camera) {
        if (particle.trailData.length < 2) return;
        
        const points = particle.trailData.map(t => ({
            position: t.position,
            size: t.size.x,
            alpha: t.alpha
        }));
        
        renderer.drawRope(points, particle.template.texture);
    }
    
    renderTrail(renderer, particle, camera) {
        if (particle.trailData.length < 2) return;
        
        const vertices = [];
        
        for (let i = 0; i < particle.trailData.length - 1; i++) {
            const current = particle.trailData[i];
            const next = particle.trailData[i + 1];
            
            const t = i / (particle.trailData.length - 1);
            const alpha = current.alpha * t * this.globalAlphaScale;
            
            const color = {
                r: current.color.r / 255,
                g: current.color.g / 255,
                b: current.color.b / 255,
                a: alpha
            };
            
            vertices.push({
                position: current.position,
                size: current.size.x * (1 - t * 0.5),
                color,
                alpha
            });
        }
        
        renderer.drawTrail(vertices, particle.template.texture);
    }
    
    sortParticles() {
    }
    
    setSortMode(mode) {
        this.sortMode = mode;
        this.sortDirty = true;
    }
    
    setGlobalSpeedScale(scale) {
        this.globalSpeedScale = scale;
    }
    
    setGlobalSizeScale(scale) {
        this.globalSizeScale = scale;
    }
    
    setGlobalAlphaScale(scale) {
        this.globalAlphaScale = scale;
    }
    
    setVisible(visible) {
        this.visible = visible;
    }
    
    setActive(active) {
        this.active = active;
    }
    
    clear() {
        for (const emitter of this.emitters) {
            emitter.clearParticles();
        }
        
        this.emitters = [];
        this.activeParticles = 0;
        this.renderQueue = [];
    }
    
    setLOD(distance) {
        let lod = 0;
        for (let i = 0; i < this.lodDistances.length; i++) {
            if (distance >= this.lodDistances[i]) {
                lod = i;
            }
        }
        
        this.currentLOD = lod;
        
        const particleScale = [1.0, 0.8, 0.6, 0.4, 0.2][lod];
        this.setGlobalSizeScale(particleScale);
    }
    
    getPerformanceStats() {
        this.performanceStats.totalParticles = this.activeParticles;
        return { ...this.performanceStats };
    }
    
    prewarm(seconds) {
        const deltaTime = 0.016;
        const iterations = Math.floor(seconds / deltaTime);
        
        for (const emitter of this.emitters) {
            for (let i = 0; i < iterations; i++) {
                emitter.update(deltaTime);
            }
        }
    }
}

export class ParticleEffect {
    constructor(system) {
        this.system = system;
        this.emitters = [];
        this.active = true;
        this.userData = {};
    }
    
    createEmitter(templateName, position, direction) {
        const emitter = this.system.createEmitter(templateName, position, direction);
        if (emitter) {
            emitter.effect = this;
            this.emitters.push(emitter);
        }
        return emitter;
    }
    
    start() {
        this.active = true;
        for (const emitter of this.emitters) {
            emitter.start();
        }
    }
    
    stop(immediate = false) {
        for (const emitter of this.emitters) {
            emitter.stop(immediate);
        }
    }
    
    destroy() {
        for (const emitter of this.emitters) {
            this.system.removeEmitter(emitter);
        }
        this.emitters = [];
    }
    
    setPosition(x, y, z) {
        for (const emitter of this.emitters) {
            emitter.setPosition(x, y, z);
        }
    }
    
    setDirection(x, y, z) {
        for (const emitter of this.emitters) {
            emitter.setDirection(x, y, z);
        }
    }
    
    setControlPoint(index, x, y, z) {
        for (const emitter of this.emitters) {
            emitter.setControlPoint(index, x, y, z);
        }
    }
    
    isActive() {
        return this.emitters.some(e => e.isActive());
    }
    
    getActiveParticleCount() {
        let count = 0;
        for (const emitter of this.emitters) {
            count += emitter.getActiveParticleCount();
        }
        return count;
    }
}

export class ParticleBuilder {
    constructor() {
        this.template = new ParticleTemplate('custom');
    }
    
    name(name) {
        this.template.name = name;
        return this;
    }
    
    type(type) {
        this.template.type = type;
        return this;
    }
    
    lifetime(min, max = min) {
        this.template.setLifetime(min, max);
        return this;
    }
    
    emissionRate(rate, min = 0) {
        this.template.setEmissionRate(rate, min);
        return this;
    }
    
    particleCount(min, max = min) {
        this.template.setParticleCount(min, max);
        return this;
    }
    
    color(r, g, b, a = 255, r2 = r, g2 = g, b2 = b, a2 = 0) {
        this.template.setColor(r, g, b, a, r2, g2, b2, a2);
        return this;
    }
    
    size(width, height, depth = 1, widthEnd = 0, heightEnd = 0, depthEnd = 1) {
        this.template.setSize(width, height, depth, widthEnd, heightEnd, depthEnd);
        return this;
    }
    
    velocity(x, y, z, x2 = 0, y2 = 0, z2 = 0) {
        this.template.setVelocity(x, y, z, x2, y2, z2);
        return this;
    }
    
    velocityRandom(x, y, z) {
        this.template.setVelocityRandom(x, y, z);
        return this;
    }
    
    gravity(x, y, z) {
        this.template.setGravity(x, y, z);
        return this;
    }
    
    acceleration(x, y, z) {
        this.template.setAcceleration(x, y, z);
        return this;
    }
    
    spread(x, y, z, xr = 0, yr = 0, zr = 0) {
        this.template.setSpread(x, y, z);
        this.template.setSpreadRandom(xr, yr, zr);
        return this;
    }
    
    rotation(start, random = 0, end = 0) {
        this.template.setRotation(start, random, end);
        return this;
    }
    
    rotationRate(start, random = 0, accel = 0) {
        this.template.setRotationRate(start, random, accel);
        return this;
    }
    
    friction(friction) {
        this.template.setFriction(friction);
        return this;
    }
    
    bounce(restitution) {
        this.template.setBounceRestitution(restitution);
        return this;
    }
    
    renderMode(mode) {
        this.template.setRenderMode(mode);
        return this;
    }
    
    blendMode(mode) {
        this.template.setBlendMode(mode);
        return this;
    }
    
    trail(length, fade = 0, colorFade = false) {
        this.template.setTrail(length, fade, colorFade);
        return this;
    }
    
    collision(mode, radius = 1, restitution = 0.5) {
        this.template.setCollision(mode, radius, restitution);
        return this;
    }
    
    addSequence(duration, startFrame = 0, endFrame = -1, rate = 30) {
        this.template.addSequence(duration, startFrame, endFrame, rate);
        return this;
    }
    
    addChild(template) {
        this.template.addChild(template);
        return this;
    }
    
    build() {
        return this.template;
    }
}

export default {
    PARTICLE_TYPE,
    PARTICLE_BLEND_MODE,
    PARTICLE_RENDER_MODE,
    PARTICLE_UPDATE_FLAGS,
    PARTICLE_COLLISION_MODE,
    PARTICLE_SORT_MODE,
    Particle,
    ParticleTemplate,
    ParticleEmitter,
    ParticleSystem,
    ParticleEffect,
    ParticleBuilder
};

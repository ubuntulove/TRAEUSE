export class Entity {
    constructor(id = null) {
        this.id = id || Entity.nextId++;
        this.name = '';
        this.tag = '';
        this.layer = 0;
        this.active = true;
        this.visible = true;
        this.position = { x: 0, y: 0, z: 0 };
        this.rotation = { x: 0, y: 0, z: 0, w: 1 };
        this.scale = { x: 1, y: 1, z: 1 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.angularVelocity = { x: 0, y: 0, z: 0 };
        this.components = new Map();
        this.children = new Set();
        this.parent = null;
        this.localPosition = { x: 0, y: 0, z: 0 };
        this.localRotation = { x: 0, y: 0, z: 0, w: 1 };
        this.localScale = { x: 1, y: 1, z: 1 };
        this.dirtyFlags = 0;
        this.onUpdateCallbacks = [];
        this.onDestroyCallbacks = [];
    }

    addComponent(component) {
        component.entity = this;
        this.components.set(component.constructor.name, component);
        if (component.onAttach) {
            component.onAttach();
        }
        return component;
    }

    getComponent(componentClass) {
        return this.components.get(componentClass.name);
    }

    hasComponent(componentClass) {
        return this.components.has(componentClass.name);
    }

    removeComponent(componentClass) {
        const component = this.components.get(componentClass.name);
        if (component) {
            if (component.onDetach) {
                component.onDetach();
            }
            component.entity = null;
            this.components.delete(componentClass.name);
        }
    }

    getComponents(componentClass) {
        const result = [];
        for (const comp of this.components.values()) {
            if (comp instanceof componentClass) {
                result.push(comp);
            }
        }
        return result;
    }

    addChild(entity) {
        if (entity.parent) {
            entity.parent.removeChild(entity);
        }
        entity.parent = this;
        this.children.add(entity.id);
        entity.updateTransform();
    }

    removeChild(entity) {
        if (this.children.has(entity.id)) {
            this.children.delete(entity.id);
            entity.parent = null;
        }
    }

    getChild(id) {
        for (const childId of this.children) {
            if (childId === id) {
                return childId;
            }
        }
        return null;
    }

    setPosition(x, y, z) {
        if (typeof x === 'object') {
            this.position = { ...x };
        } else {
            this.position = { x, y, z };
        }
        this.dirtyFlags |= Entity.TRANSFORM_DIRTY;
    }

    setRotation(x, y, z, w = 1) {
        if (typeof x === 'object') {
            this.rotation = { ...x };
        } else {
            this.rotation = { x, y, z, w };
        }
        this.dirtyFlags |= Entity.TRANSFORM_DIRTY;
    }

    setScale(x, y, z) {
        if (typeof x === 'object') {
            this.scale = { ...x };
        } else {
            this.scale = { x, y, z };
        }
        this.dirtyFlags |= Entity.TRANSFORM_DIRTY;
    }

    translate(x, y, z) {
        this.position.x += x;
        this.position.y += y;
        this.position.z += z;
        this.dirtyFlags |= Entity.TRANSFORM_DIRTY;
    }

    rotate(x, y, z) {
        this.rotation.x += x;
        this.rotation.y += y;
        this.rotation.z += z;
        this.dirtyFlags |= Entity.TRANSFORM_DIRTY;
    }

    updateTransform() {
        if (!this.parent) {
            this.localPosition = { ...this.position };
            this.localRotation = { ...this.rotation };
            this.localScale = { ...this.scale };
            return;
        }

        this.localPosition = {
            x: this.position.x - this.parent.position.x,
            y: this.position.y - this.parent.position.y,
            z: this.position.z - this.parent.position.z
        };
    }

    getForwardVector() {
        const q = this.rotation;
        return {
            x: 2 * (q.x * q.z + q.y * q.w),
            y: 2 * (q.y * q.z - q.x * q.w),
            z: 1 - 2 * (q.x * q.x + q.y * q.y)
        };
    }

    getRightVector() {
        const q = this.rotation;
        return {
            x: 1 - 2 * (q.y * q.y + q.z * q.z),
            y: 2 * (q.x * q.y + q.z * q.w),
            z: 2 * (q.z * q.y - q.x * q.w)
        };
    }

    getUpVector() {
        const q = this.rotation;
        return {
            x: 2 * (q.x * q.y - q.z * q.w),
            y: 1 - 2 * (q.x * q.x + q.z * q.z),
            z: 2 * (q.y * q.z + q.x * q.w)
        };
    }

    distanceTo(other) {
        const dx = this.position.x - other.position.x;
        const dy = this.position.y - other.position.y;
        const dz = this.position.z - other.position.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    directionTo(other) {
        const dist = this.distanceTo(other);
        if (dist === 0) return { x: 0, y: 0, z: 0 };
        return {
            x: (other.position.x - this.position.x) / dist,
            y: (other.position.y - this.position.y) / dist,
            z: (other.position.z - this.position.z) / dist
        };
    }

    lookAt(target) {
        const dir = this.directionTo(target);
        const yaw = Math.atan2(dir.x, dir.y);
        const pitch = Math.asin(-dir.z);
        this.rotation = this.eulerToQuaternion(pitch, yaw, 0);
    }

    eulerToQuaternion(x, y, z) {
        const cx = Math.cos(x / 2);
        const sx = Math.sin(x / 2);
        const cy = Math.cos(y / 2);
        const sy = Math.sin(y / 2);
        const cz = Math.cos(z / 2);
        const sz = Math.sin(z / 2);
        return {
            x: sx * cy * cz - cx * sy * sz,
            y: cx * sy * cz + sx * cy * sz,
            z: cx * cy * sz - sx * sy * cz,
            w: cx * cy * cz + sx * sy * sz
        };
    }

    onUpdate(callback) {
        this.onUpdateCallbacks.push(callback);
    }

    onDestroy(callback) {
        this.onDestroyCallbacks.push(callback);
    }

    update(deltaTime) {
        if (!this.active) return;

        for (const callback of this.onUpdateCallbacks) {
            callback(this, deltaTime);
        }

        for (const component of this.components.values()) {
            if (component.active && component.update) {
                component.update(deltaTime);
            }
        }
    }

    destroy() {
        for (const callback of this.onDestroyCallbacks) {
            callback(this);
        }

        for (const component of this.components.values()) {
            if (component.onDestroy) {
                component.onDestroy();
            }
        }

        this.components.clear();
        this.children.clear();
        this.parent = null;
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            tag: this.tag,
            layer: this.layer,
            active: this.active,
            visible: this.visible,
            position: this.position,
            rotation: this.rotation,
            scale: this.scale,
            components: Array.from(this.components.values()).map(c => c.serialize())
        };
    }
}
Entity.nextId = 0;
Entity.TRANSFORM_DIRTY = 1;

export class Component {
    constructor() {
        this.entity = null;
        this.active = true;
        this.enabled = true;
    }

    getGameObject() {
        return this.entity;
    }

    getTransform() {
        return this.entity;
    }

    onAttach() {}
    onDetach() {}
    onDestroy() {}
    update(deltaTime) {}

    serialize() {
        return {
            type: this.constructor.name,
            active: this.active,
            enabled: this.enabled
        };
    }
}

export class TransformComponent extends Component {
    constructor() {
        super();
        this.position = { x: 0, y: 0, z: 0 };
        this.rotation = { x: 0, y: 0, z: 0, w: 1 };
        this.scale = { x: 1, y: 1, z: 1 };
        this.localPosition = { x: 0, y: 0, z: 0 };
        this.localRotation = { x: 0, y: 0, z: 0, w: 1 };
        this.localScale = { x: 1, y: 1, z: 1 };
    }

    setPosition(x, y, z) {
        this.position = { x, y, z };
    }

    setRotation(x, y, z, w = 1) {
        this.rotation = { x, y, z, w };
    }

    setScale(x, y, z) {
        this.scale = { x, y, z };
    }

    translate(x, y, z) {
        this.position.x += x;
        this.position.y += y;
        this.position.z += z;
    }

    getForward() {
        return this.entity.getForwardVector();
    }

    getRight() {
        return this.entity.getRightVector();
    }

    getUp() {
        return this.entity.getUpVector();
    }
}

export class RigidBodyComponent extends Component {
    constructor() {
        super();
        this.mass = 1.0;
        this.velocity = { x: 0, y: 0, z: 0 };
        this.acceleration = { x: 0, y: 0, z: 0 };
        this.angularVelocity = { x: 0, y: 0, z: 0 };
        this.drag = 0.0;
        this.angularDrag = 0.0;
        this.gravity = { x: 0, y: 0, z: -9.81 };
        this.useGravity = true;
        this.isKinematic = false;
        this.constraints = {
            freezePositionX: false,
            freezePositionY: false,
            freezePositionZ: false,
            freezeRotationX: false,
            freezeRotationY: false,
            freezeRotationZ: false
        };
    }

    applyForce(force) {
        if (this.isKinematic) return;
        this.acceleration.x += force.x / this.mass;
        this.acceleration.y += force.y / this.mass;
        this.acceleration.z += force.z / this.mass;
    }

    applyImpulse(impulse) {
        if (this.isKinematic) return;
        this.velocity.x += impulse.x / this.mass;
        this.velocity.y += impulse.y / this.mass;
        this.velocity.z += impulse.z / this.mass;
    }

    applyTorque(torque) {
        if (this.isKinematic) return;
        this.angularVelocity.x += torque.x / this.mass;
        this.angularVelocity.y += torque.y / this.mass;
        this.angularVelocity.z += torque.z / this.mass;
    }

    update(deltaTime) {
        if (this.isKinematic) return;

        if (this.useGravity) {
            this.acceleration.x += this.gravity.x;
            this.acceleration.y += this.gravity.y;
            this.acceleration.z += this.gravity.z;
        }

        this.velocity.x += this.acceleration.x * deltaTime;
        this.velocity.y += this.acceleration.y * deltaTime;
        this.velocity.z += this.acceleration.z * deltaTime;

        this.velocity.x *= (1 - this.drag * deltaTime);
        this.velocity.y *= (1 - this.drag * deltaTime);
        this.velocity.z *= (1 - this.drag * deltaTime);

        this.entity.position.x += this.velocity.x * deltaTime;
        this.entity.position.y += this.velocity.y * deltaTime;
        this.entity.position.z += this.velocity.z * deltaTime;

        this.angularVelocity.x *= (1 - this.angularDrag * deltaTime);
        this.angularVelocity.y *= (1 - this.angularDrag * deltaTime);
        this.angularVelocity.z *= (1 - this.angularDrag * deltaTime);

        this.acceleration = { x: 0, y: 0, z: 0 };
    }
}

export class ColliderComponent extends Component {
    constructor() {
        super();
        this.isTrigger = false;
        this.center = { x: 0, y: 0, z: 0 };
        this.material = null;
        this.contactOffset = 0.1;
        this.enabled = true;
    }

    onCollisionEnter(other) {}
    onCollisionStay(other) {}
    onCollisionExit(other) {}
}

export class BoxCollider extends ColliderComponent {
    constructor() {
        super();
        this.size = { x: 1, y: 1, z: 1 };
    }

    containsPoint(point) {
        const p = this.entity.position;
        const s = this.size;
        return point.x >= p.x - s.x / 2 && point.x <= p.x + s.x / 2 &&
               point.y >= p.y - s.y / 2 && point.y <= p.y + s.y / 2 &&
               point.z >= p.z - s.z / 2 && point.z <= p.z + s.z / 2;
    }

    intersectsBox(other) {
        const a = this.entity.position;
        const b = other.entity.position;
        const sa = this.size;
        const sb = other.size;
        return Math.abs(a.x - b.x) < (sa.x + sb.x) / 2 &&
               Math.abs(a.y - b.y) < (sa.y + sb.y) / 2 &&
               Math.abs(a.z - b.z) < (sa.z + sb.z) / 2;
    }
}

export class SphereCollider extends ColliderComponent {
    constructor() {
        super();
        this.radius = 0.5;
    }

    containsPoint(point) {
        const dx = point.x - this.entity.position.x;
        const dy = point.y - this.entity.position.y;
        const dz = point.z - this.entity.position.z;
        return dx * dx + dy * dy + dz * dz <= this.radius * this.radius;
    }

    intersectsSphere(other) {
        const dx = this.entity.position.x - other.entity.position.x;
        const dy = this.entity.position.y - other.entity.position.y;
        const dz = this.entity.position.z - other.entity.position.z;
        const dist = Math.sqrt(dx * dx + dy * dy + dz * dz);
        return dist < this.radius + other.radius;
    }
}

export class CapsuleCollider extends ColliderComponent {
    constructor() {
        super();
        this.radius = 0.5;
        this.height = 2.0;
        this.direction = 1;
    }
}

export class MeshRendererComponent extends Component {
    constructor() {
        super();
        this.mesh = null;
        this.materials = [];
        this.castShadows = true;
        this.receiveShadows = true;
        this.lightmapIndex = -1;
        this.lightmapScaleOffset = { x: 1, y: 1, z: 0, w: 0 };
    }

    setMesh(mesh) {
        this.mesh = mesh;
    }

    setMaterial(material) {
        this.materials = [material];
    }

    setMaterials(materials) {
        this.materials = materials;
    }
}

export class SkinnedMeshRendererComponent extends MeshRendererComponent {
    constructor() {
        super();
        this.bones = [];
        this.rootBone = null;
        this.localBounds = { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 } };
        this.quality = 2;
    }
}

export class LightComponent extends Component {
    constructor() {
        super();
        this.type = 'directional';
        this.color = { r: 1, g: 1, b: 1 };
        this.intensity = 1.0;
        this.range = 10;
        this.spotAngle = 30;
        this.innerSpotAngle = 20;
        this.shadowsEnabled = false;
        this.shadowStrength = 1.0;
        this.shadowResolution = 1024;
        this.shadowBias = 0.05;
        this.renderMode = 'auto';
    }
}

export class CameraComponent extends Component {
    constructor() {
        super();
        this.fov = 60;
        this.nearClip = 0.1;
        this.farClip = 1000;
        this.fieldOfView = 60;
        this.depth = 0;
        this.cullingMask = 0xFFFFFF;
        this.clearFlags = 'skybox';
        this.backgroundColor = { r: 0, g: 0, b: 0, a: 1 };
        this.rect = { x: 0, y: 0, width: 1, height: 1 };
        this.targetTexture = null;
        this.hdr = true;
        this.allowMSAA = true;
    }

    worldToScreenPoint(position) {
        return { x: 0, y: 0, z: 0 };
    }

    screenToWorldPoint(screenPos) {
        return { x: 0, y: 0, z: 0 };
    }

    raycast(screenPos) {
        return null;
    }

    frustumContainsPoint(point) {
        return true;
    }
}

export class AudioSourceComponent extends Component {
    constructor() {
        super();
        this.clip = null;
        this.volume = 1.0;
        this.pitch = 1.0;
        this.loop = false;
        this.playOnAwake = false;
        this.isPlaying = false;
        this.spatialBlend = 0;
        this.minDistance = 1;
        this.maxDistance = 500;
        this.dopplerLevel = 1;
    }

    play() {
        this.isPlaying = true;
    }

    pause() {
        this.isPlaying = false;
    }

    stop() {
        this.isPlaying = false;
    }
}

export class ParticleSystemComponent extends Component {
    constructor() {
        super();
        this.emissionRate = 10;
        this.startLifetime = 2.0;
        this.startSpeed = 5.0;
        this.startSize = 0.1;
        this.startColor = { r: 1, g: 1, b: 1, a: 1 };
        this.maxParticles = 100;
        this.playOnAwake = true;
        this.loop = true;
        this.duration = 5.0;
        this.shape = 'sphere';
        this.shapeRadius = 1.0;
        this.shapeAngle = 25;
        this.velocityOverLifetime = null;
        this.colorOverLifetime = null;
        this.sizeOverLifetime = null;
    }

    play() {}
    pause() {}
    stop() {}
    clear() {}
}

export class PlayerComponent extends Component {
    constructor() {
        super();
        this.playerId = 0;
        this.team = 'none';
        this.health = 100;
        this.maxHealth = 100;
        this.armor = 0;
        this.isAlive = true;
        this.isSpotted = false;
        this.lastSeenPosition = { x: 0, y: 0, z: 0 };
        this.spotters = new Set();
    }

    takeDamage(amount) {
        if (!this.isAlive) return;
        this.health = Math.max(0, this.health - amount);
        if (this.health <= 0) {
            this.die();
        }
    }

    heal(amount) {
        if (!this.isAlive) return;
        this.health = Math.min(this.maxHealth, this.health + amount);
    }

    die() {
        this.isAlive = false;
        this.health = 0;
    }

    respawn(position) {
        this.isAlive = true;
        this.health = this.maxHealth;
        this.entity.setPosition(position);
    }
}

export class WeaponComponent extends Component {
    constructor() {
        super();
        this.weaponId = '';
        this.weaponType = 'rifle';
        this.ammo = 30;
        this.maxAmmo = 30;
        this.reserveAmmo = 90;
        this.maxReserveAmmo = 120;
        this.fireRate = 0.1;
        this.damage = 30;
        this.accuracy = 0.9;
        this.recoil = 0.5;
        this.isReloading = false;
        this.reloadTime = 2.0;
        this.canUseScope = false;
        this.zoomLevels = [];
    }

    fire() {
        if (this.isReloading || this.ammo <= 0) return false;
        this.ammo--;
        return true;
    }

    reload() {
        if (this.isReloading || this.reserveAmmo <= 0 || this.ammo === this.maxAmmo) return false;
        this.isReloading = true;
        return true;
    }

    finishReload() {
        const needed = this.maxAmmo - this.ammo;
        const available = Math.min(needed, this.reserveAmmo);
        this.ammo += available;
        this.reserveAmmo -= available;
        this.isReloading = false;
    }

    canFire() {
        return !this.isReloading && this.ammo > 0;
    }

    hasAmmo() {
        return this.ammo > 0 || this.reserveAmmo > 0;
    }

    isFull() {
        return this.ammo === this.maxAmmo && this.reserveAmmo >= this.maxReserveAmmo;
    }
}

export class InventoryComponent extends Component {
    constructor() {
        super();
        this.weapons = new Map();
        this.currentWeapon = null;
        this.grenades = new Map();
        this.equipment = new Map();
        this.c4 = false;
        this.defuseKit = false;
        this.kevlarVest = false;
        this.helmet = false;
        this.maxPrimaryWeapons = 1;
        this.maxSecondaryWeapons = 2;
        this.maxGrenades = 4;
    }

    addWeapon(weapon) {
        if (this.weapons.has(weapon.weaponId)) return false;
        this.weapons.set(weapon.weaponId, weapon);
        if (!this.currentWeapon) {
            this.currentWeapon = weapon.weaponId;
        }
        return true;
    }

    removeWeapon(weaponId) {
        this.weapons.delete(weaponId);
        if (this.currentWeapon === weaponId) {
            this.currentWeapon = null;
        }
    }

    switchWeapon(weaponId) {
        if (this.weapons.has(weaponId)) {
            this.currentWeapon = weaponId;
            return true;
        }
        return false;
    }

    getCurrentWeapon() {
        return this.weapons.get(this.currentWeapon);
    }

    addGrenade(type, count = 1) {
        const current = this.grenades.get(type) || 0;
        this.grenades.set(type, current + count);
    }

    removeGrenade(type, count = 1) {
        const current = this.grenades.get(type) || 0;
        const newCount = Math.max(0, current - count);
        if (newCount === 0) {
            this.grenades.delete(type);
        } else {
            this.grenades.set(type, newCount);
        }
    }

    getGrenadeCount(type) {
        return this.grenades.get(type) || 0;
    }

    dropWeapon(weaponId) {
        const weapon = this.weapons.get(weaponId);
        if (!weapon) return null;
        this.removeWeapon(weaponId);
        return weapon;
    }
}

export class TeamComponent extends Component {
    constructor() {
        super();
        this.teamId = 'none';
        this.teamName = '';
        this.score = 0;
        this.alivePlayers = 0;
        this.totalPlayers = 0;
        this.hasBomb = false;
        this.hasHostage = false;
        this.canBuy = true;
        this.money = 0;
        this.timeoutsRemaining = 0;
    }

    addScore(points) {
        this.score += points;
    }

    resetScore() {
        this.score = 0;
    }

    updateAlivePlayers(alive) {
        this.alivePlayers = alive;
    }
}

export class EntityManager {
    constructor() {
        this.entities = new Map();
        this.entityPool = new Map();
        this.tagIndex = new Map();
        this.layerIndex = new Map();
        this.namedEntities = new Map();
        this.activeEntities = new Set();
        this.destroyQueue = [];
    }

    createEntity(name = '', tag = '', layer = 0) {
        const entity = new Entity();
        entity.name = name;
        entity.tag = tag;
        entity.layer = layer;

        this.entities.set(entity.id, entity);
        this.activeEntities.add(entity.id);

        if (tag) {
            if (!this.tagIndex.has(tag)) {
                this.tagIndex.set(tag, new Set());
            }
            this.tagIndex.get(tag).add(entity.id);
        }

        if (layer !== 0) {
            if (!this.layerIndex.has(layer)) {
                this.layerIndex.set(layer, new Set());
            }
            this.layerIndex.get(layer).add(entity.id);
        }

        if (name) {
            this.namedEntities.set(name, entity);
        }

        return entity;
    }

    destroyEntity(entityId, delay = 0) {
        if (delay > 0) {
            setTimeout(() => this.destroyEntityImmediate(entityId), delay);
        } else {
            this.destroyQueue.push(entityId);
        }
    }

    destroyEntityImmediate(entityId) {
        const entity = this.entities.get(entityId);
        if (!entity) return;

        entity.destroy();

        if (entity.tag) {
            this.tagIndex.get(entity.tag)?.delete(entityId);
        }
        if (entity.layer !== 0) {
            this.layerIndex.get(entity.layer)?.delete(entityId);
        }
        if (entity.name) {
            this.namedEntities.delete(entity.name);
        }

        this.activeEntities.delete(entityId);
        this.entities.delete(entityId);
    }

    getEntity(id) {
        return this.entities.get(id);
    }

    getEntityByName(name) {
        return this.namedEntities.get(name);
    }

    findEntitiesByTag(tag) {
        const ids = this.tagIndex.get(tag);
        if (!ids) return [];
        return Array.from(ids).map(id => this.entities.get(id)).filter(e => e);
    }

    findEntitiesByLayer(layer) {
        const ids = this.layerIndex.get(layer);
        if (!ids) return [];
        return Array.from(ids).map(id => this.entities.get(id)).filter(e => e);
    }

    findEntityWithComponent(componentClass) {
        for (const entity of this.entities.values()) {
            if (entity.hasComponent(componentClass)) {
                return entity;
            }
        }
        return null;
    }

    findEntitiesWithComponent(componentClass) {
        const result = [];
        for (const entity of this.entities.values()) {
            if (entity.hasComponent(componentClass)) {
                result.push(entity);
            }
        }
        return result;
    }

    processDestroyQueue() {
        for (const id of this.destroyQueue) {
            this.destroyEntityImmediate(id);
        }
        this.destroyQueue = [];
    }

    update(deltaTime) {
        this.processDestroyQueue();

        for (const id of this.activeEntities) {
            const entity = this.entities.get(id);
            if (entity) {
                entity.update(deltaTime);
            }
        }
    }

    clear() {
        for (const entity of this.entities.values()) {
            entity.destroy();
        }
        this.entities.clear();
        this.activeEntities.clear();
        this.tagIndex.clear();
        this.layerIndex.clear();
        this.namedEntities.clear();
        this.destroyQueue = [];
    }

    getEntityCount() {
        return this.entities.size;
    }

    getActiveEntityCount() {
        return this.activeEntities.size;
    }

    serialize() {
        const entityData = [];
        for (const entity of this.entities.values()) {
            entityData.push(entity.serialize());
        }
        return entityData;
    }
}

export class ComponentSystem {
    constructor() {
        this.systems = new Map();
        this.requiredComponents = new Map();
    }

    registerSystem(name, system, requiredComponents = []) {
        this.systems.set(name, system);
        this.requiredComponents.set(name, requiredComponents);
    }

    getSystem(name) {
        return this.systems.get(name);
    }

    update(deltaTime, entityManager) {
        for (const [name, system] of this.systems) {
            if (system.enabled !== false) {
                const entities = this.getEntitiesForSystem(name, entityManager);
                if (system.update) {
                    system.update(deltaTime, entities);
                }
            }
        }
    }

    getEntitiesForSystem(systemName, entityManager) {
        const required = this.requiredComponents.get(systemName);
        if (!required || required.length === 0) {
            return Array.from(entityManager.entities.values());
        }

        return Array.from(entityManager.entities.values()).filter(entity => {
            return required.every(comp => entity.hasComponent(comp));
        });
    }

    disableSystem(name) {
        const system = this.systems.get(name);
        if (system) {
            system.enabled = false;
        }
    }

    enableSystem(name) {
        const system = this.systems.get(name);
        if (system) {
            system.enabled = true;
        }
    }
}

export class PhysicsSystem {
    update(deltaTime, entities) {
        const rigidbodies = entities.filter(e => e.hasComponent(RigidBodyComponent));

        for (const entity of rigidbodies) {
            const rb = entity.getComponent(RigidBodyComponent);
            if (rb.isKinematic) continue;

            const transform = entity.getComponent(TransformComponent);
            if (transform) {
                transform.position.x += rb.velocity.x * deltaTime;
                transform.position.y += rb.velocity.y * deltaTime;
                transform.position.z += rb.velocity.z * deltaTime;
            }
        }

        const colliders = entities.filter(e => e.hasComponent(ColliderComponent));
        for (let i = 0; i < colliders.length; i++) {
            for (let j = i + 1; j < colliders.length; j++) {
                if (this.checkCollision(colliders[i], colliders[j])) {
                    const c1 = colliders[i].getComponent(ColliderComponent);
                    const c2 = colliders[j].getComponent(ColliderComponent);
                    if (!c1.isTrigger) c1.onCollisionEnter(colliders[j]);
                    if (!c2.isTrigger) c2.onCollisionEnter(colliders[i]);
                }
            }
        }
    }

    checkCollision(a, b) {
        const ca = a.getComponent(BoxCollider);
        const cb = b.getComponent(BoxCollider);
        if (ca && cb) {
            return ca.intersectsBox(cb);
        }
        return false;
    }
}

export class RenderSystem {
    update(deltaTime, entities) {
        const renderers = entities.filter(e => e.hasComponent(MeshRendererComponent));
        const cameras = entities.filter(e => e.hasComponent(CameraComponent));

        for (const camera of cameras) {
            const cam = camera.getComponent(CameraComponent);
            for (const entity of renderers) {
                if (cam.frustumContainsPoint(entity.position)) {
                }
            }
        }
    }
}

export class AudioSystem {
    update(deltaTime, entities) {
        const audioSources = entities.filter(e => e.hasComponent(AudioSourceComponent));

        for (const entity of audioSources) {
            const audio = entity.getComponent(AudioSourceComponent);
            if (audio.isPlaying && audio.clip) {
            }
        }
    }
}

export default EntityManager;

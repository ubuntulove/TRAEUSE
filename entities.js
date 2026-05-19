import { GAME_CONFIG } from './config.js';

export const ENTITY_CLASS = {
    BASE: 0,
    PLAYER: 1,
    WEAPON: 2,
    GRENADE: 3,
    PICKUP: 4,
    TRIGGER: 5,
    DOOR: 6,
    LADDER: 7,
   func_breakable: 8,
    func_button: 9,
   func_brush: 10,
    info_target: 11,
    info_spawn_point: 12,
    light: 13,
    func_door_rotating: 14,
    func_wall_toggle: 15,
    env_shooter: 16,
    env_physics_blocker: 17,
    trigger_multiple: 18,
    trigger_once: 19,
    trigger_hurt: 20,
    trigger_teleport: 21,
    CUSTOM: 999
};

export const ENTITY_STATE = {
    INACTIVE: 0,
    ACTIVE: 1,
    SLEEPING: 2,
    FROZEN: 3,
    DESTROYED: 4
};

export const ENTITY_TYPE = {
    WORLD: 0,
    ENTITY: 1,
    PLAYER: 2,
    NPC: 3,
    WEAPON: 4,
    PROJECTILE: 5,
    PICKUP: 6,
    TRIGGER: 7,
    LOGICAL: 8,
    DECORATOR: 9
};

export class Vector3 {
    constructor(x = 0, y = 0, z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    add(v) { return new Vector3(this.x + v.x, this.y + v.y, this.z + v.z); }
    subtract(v) { return new Vector3(this.x - v.x, this.y - v.y, this.z - v.z); }
    multiply(scalar) { return new Vector3(this.x * scalar, this.y * scalar, this.z * scalar); }
    divide(scalar) { return new Vector3(this.x / scalar, this.y / scalar, this.z / scalar); }
    dot(v) { return this.x * v.x + this.y * v.y + this.z * v.z; }
    cross(v) { return new Vector3(this.y * v.z - this.z * v.y, this.z * v.x - this.x * v.z, this.x * v.y - this.y * v.x); }
    length() { return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z); }
    lengthSquared() { return this.x * this.x + this.y * this.y + this.z * this.z; }
    normalize() { const len = this.length(); return len > 0 ? this.divide(len) : new Vector3(); }
    lerp(v, t) { return new Vector3(this.x + (v.x - this.x) * t, this.y + (v.y - this.y) * t, this.z + (v.z - this.z) * t); }
    clone() { return new Vector3(this.x, this.y, this.z); }
    equals(v, epsilon = 0.0001) { return Math.abs(this.x - v.x) < epsilon && Math.abs(this.y - v.y) < epsilon && Math.abs(this.z - v.z) < epsilon; }
}

export class Quaternion {
    constructor(x = 0, y = 0, z = 0, w = 1) {
        this.x = x; this.y = y; this.z = z; this.w = w;
    }

    multiply(q) {
        return new Quaternion(
            this.w * q.x + this.x * q.w + this.y * q.z - this.z * q.y,
            this.w * q.y - this.x * q.z + this.y * q.w + this.z * q.x,
            this.w * q.z + this.x * q.y - this.y * q.x + this.z * q.w,
            this.w * q.w - this.x * q.x - this.y * q.y - this.z * q.z
        );
    }

    normalize() {
        const len = Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w);
        if (len > 0) {
            this.x /= len; this.y /= len; this.z /= len; this.w /= len;
        }
        return this;
    }

    fromEuler(x, y, z) {
        const cx = Math.cos(x / 2), sx = Math.sin(x / 2);
        const cy = Math.cos(y / 2), sy = Math.sin(y / 2);
        const cz = Math.cos(z / 2), sz = Math.sin(z / 2);
        this.x = sx * cy * cz - cx * sy * sz;
        this.y = cx * sy * cz + sx * cy * sz;
        this.z = cx * cy * sz - sx * sy * cz;
        this.w = cx * cy * cz + sx * sy * sz;
        return this.normalize();
    }

    toEuler() {
        const sinr_cosp = 2 * (this.w * this.x + this.y * this.z);
        const cosr_cosp = 1 - 2 * (this.x * this.x + this.y * this.y);
        const roll = Math.atan2(sinr_cosp, cosr_cosp);

        const sinp = 2 * (this.w * this.y - this.z * this.x);
        const pitch = Math.abs(sinp) >= 1 ? Math.sign(sinp) * Math.PI / 2 : Math.asin(sinp);

        const siny_cosp = 2 * (this.w * this.z + this.x * this.y);
        const cosy_cosp = 1 - 2 * (this.y * this.y + this.z * this.z);
        const yaw = Math.atan2(siny_cosp, cosy_cosp);

        return new Vector3(roll, pitch, yaw);
    }

    clone() { return new Quaternion(this.x, this.y, this.z, this.w); }
    static identity() { return new Quaternion(0, 0, 0, 1); }
}

export class Transform {
    constructor() {
        this.position = new Vector3();
        this.rotation = Quaternion.identity();
        this.scale = new Vector3(1, 1, 1);
        this.localPosition = new Vector3();
        this.localRotation = Quaternion.identity();
        this.localScale = new Vector3(1, 1, 1);
        this.parent = null;
        this.children = [];
        this.dirty = true;
        this.worldMatrix = null;
    }

    setPosition(x, y, z) {
        this.position = new Vector3(x, y, z);
        this.dirty = true;
    }

    setRotation(x, y, z) {
        this.rotation.fromEuler(x, y, z);
        this.dirty = true;
    }

    setRotationQuat(q) {
        this.rotation = q.clone();
        this.dirty = true;
    }

    translate(x, y, z) {
        this.position = this.position.add(new Vector3(x, y, z));
        this.dirty = true;
    }

    rotate(x, y, z) {
        const q = new Quaternion().fromEuler(x, y, z);
        this.rotation = q.multiply(this.rotation).normalize();
        this.dirty = true;
    }

    lookAt(target, up = new Vector3(0, 1, 0)) {
        const forward = target.subtract(this.position).normalize();
        const dot = forward.dot(new Vector3(0, 0, 1));
        if (Math.abs(dot - (-1)) < 0.000001) {
            this.rotation = new Quaternion(1, 0, 0, 0);
            return;
        }
        if (Math.abs(dot - 1) < 0.000001) {
            this.rotation = Quaternion.identity();
            return;
        }
        const angle = Math.acos(dot);
        const axis = new Vector3(0, 0, 1).cross(forward).normalize();
        this.rotation = this.fromAxisAngle(axis, angle);
        this.dirty = true;
    }

    fromAxisAngle(axis, angle) {
        const halfAngle = angle / 2;
        const s = Math.sin(halfAngle);
        return new Quaternion(axis.x * s, axis.y * s, axis.z * s, Math.cos(halfAngle));
    }

    getForward() {
        return new Vector3(2 * (this.rotation.x * this.rotation.z + this.rotation.w * this.rotation.y),
                          2 * (this.rotation.y * this.rotation.z - this.rotation.w * this.rotation.x),
                          1 - 2 * (this.rotation.x * this.rotation.x + this.rotation.y * this.rotation.y)).normalize();
    }

    getRight() {
        return new Vector3(1 - 2 * (this.rotation.y * this.rotation.y + this.rotation.z * this.rotation.z),
                          2 * (this.rotation.x * this.rotation.y + this.rotation.w * this.rotation.z),
                          2 * (this.rotation.x * this.rotation.z - this.rotation.w * this.rotation.y)).normalize();
    }

    getUp() {
        return new Vector3(2 * (this.rotation.x * this.rotation.y - this.rotation.w * this.rotation.z),
                          1 - 2 * (this.rotation.x * this.rotation.x + this.rotation.z * this.rotation.z),
                          2 * (this.rotation.y * this.rotation.z + this.rotation.w * this.rotation.x)).normalize();
    }

    updateWorldMatrix() {
        if (!this.dirty && this.worldMatrix) return this.worldMatrix;

        let matrix = Matrix.identity();

        matrix = matrix.translate(this.position.x, this.position.y, this.position.z);

        const euler = this.rotation.toEuler();
        matrix = matrix.rotate(euler.x, euler.y, euler.z);

        matrix = matrix.scale(this.scale.x, this.scale.y, this.scale.z);

        if (this.parent) {
            const parentMatrix = this.parent.getWorldMatrix();
            matrix = parentMatrix.multiply(matrix);
        }

        this.worldMatrix = matrix;
        this.dirty = false;
        return matrix;
    }

    worldToLocal(point) {
        const invMatrix = this.updateWorldMatrix().inverse();
        return invMatrix.transformPoint(point);
    }

    localToWorld(point) {
        return this.updateWorldMatrix().transformPoint(point);
    }

    clone() {
        const t = new Transform();
        t.position = this.position.clone();
        t.rotation = this.rotation.clone();
        t.scale = this.scale.clone();
        return t;
    }
}

export class Matrix {
    constructor() {
        this.m = new Float32Array(16);
        this.identity();
    }

    static identity() {
        const m = new Matrix();
        m.m[0] = 1; m.m[1] = 0; m.m[2] = 0; m.m[3] = 0;
        m.m[4] = 0; m.m[5] = 1; m.m[6] = 0; m.m[7] = 0;
        m.m[8] = 0; m.m[9] = 0; m.m[10] = 1; m.m[11] = 0;
        m.m[12] = 0; m.m[13] = 0; m.m[14] = 0; m.m[15] = 1;
        return m;
    }

    translate(x, y, z) {
        const m = new Matrix();
        m.m[0] = 1; m.m[1] = 0; m.m[2] = 0; m.m[3] = x;
        m.m[4] = 0; m.m[5] = 1; m.m[6] = 0; m.m[7] = y;
        m.m[8] = 0; m.m[9] = 0; m.m[10] = 1; m.m[11] = z;
        m.m[12] = 0; m.m[13] = 0; m.m[14] = 0; m.m[15] = 1;
        return this.multiply(m);
    }

    rotate(x, y, z) {
        const cx = Math.cos(x), sx = Math.sin(x);
        const cy = Math.cos(y), sy = Math.sin(y);
        const cz = Math.cos(z), sz = Math.sin(z);

        const rotX = new Matrix();
        rotX.m[0] = 1; rotX.m[1] = 0; rotX.m[2] = 0; rotX.m[3] = 0;
        rotX.m[4] = 0; rotX.m[5] = cx; rotX.m[6] = -sx; rotX.m[7] = 0;
        rotX.m[8] = 0; rotX.m[9] = sx; rotX.m[10] = cx; rotX.m[11] = 0;
        rotX.m[12] = 0; rotX.m[13] = 0; rotX.m[14] = 0; rotX.m[15] = 1;

        const rotY = new Matrix();
        rotY.m[0] = cy; rotY.m[1] = 0; rotY.m[2] = sy; rotY.m[3] = 0;
        rotY.m[4] = 0; rotY.m[5] = 1; rotY.m[6] = 0; rotY.m[7] = 0;
        rotY.m[8] = -sy; rotY.m[9] = 0; rotY.m[10] = cy; rotY.m[11] = 0;
        rotY.m[12] = 0; rotY.m[13] = 0; rotY.m[14] = 0; rotY.m[15] = 1;

        const rotZ = new Matrix();
        rotZ.m[0] = cz; rotZ.m[1] = -sz; rotZ.m[2] = 0; rotZ.m[3] = 0;
        rotZ.m[4] = sz; rotZ.m[5] = cz; rotZ.m[6] = 0; rotZ.m[7] = 0;
        rotZ.m[8] = 0; rotZ.m[9] = 0; rotZ.m[10] = 1; rotZ.m[11] = 0;
        rotZ.m[12] = 0; rotZ.m[13] = 0; rotZ.m[14] = 0; rotZ.m[15] = 1;

        return this.multiply(rotX).multiply(rotY).multiply(rotZ);
    }

    scale(x, y, z) {
        const m = new Matrix();
        m.m[0] = x; m.m[1] = 0; m.m[2] = 0; m.m[3] = 0;
        m.m[4] = 0; m.m[5] = y; m.m[6] = 0; m.m[7] = 0;
        m.m[8] = 0; m.m[9] = 0; m.m[10] = z; m.m[11] = 0;
        m.m[12] = 0; m.m[13] = 0; m.m[14] = 0; m.m[15] = 1;
        return this.multiply(m);
    }

    multiply(other) {
        const result = new Matrix();
        for (let i = 0; i < 4; i++) {
            for (let j = 0; j < 4; j++) {
                let sum = 0;
                for (let k = 0; k < 4; k++) {
                    sum += this.m[i + k * 4] * other.m[k + j * 4];
                }
                result.m[i + j * 4] = sum;
            }
        }
        return result;
    }

    transformPoint(point) {
        const x = this.m[0] * point.x + this.m[4] * point.y + this.m[8] * point.z + this.m[12];
        const y = this.m[1] * point.x + this.m[5] * point.y + this.m[9] * point.z + this.m[13];
        const z = this.m[2] * point.x + this.m[6] * point.y + this.m[10] * point.z + this.m[14];
        return new Vector3(x, y, z);
    }

    inverse() {
        const m = this.m;
        const inv = new Matrix();

        inv.m[0] = m[5]*m[10]*m[15] - m[5]*m[11]*m[14] - m[9]*m[6]*m[15] + m[9]*m[7]*m[14] + m[13]*m[6]*m[11] - m[13]*m[7]*m[10];
        inv.m[4] = -m[4]*m[10]*m[15] + m[4]*m[11]*m[14] + m[8]*m[6]*m[15] - m[8]*m[7]*m[14] - m[12]*m[6]*m[11] + m[12]*m[7]*m[10];
        inv.m[8] = m[4]*m[9]*m[15] - m[4]*m[11]*m[13] - m[8]*m[5]*m[15] + m[8]*m[7]*m[13] + m[12]*m[5]*m[11] - m[12]*m[7]*m[9];
        inv.m[12] = -m[4]*m[9]*m[14] + m[4]*m[10]*m[13] + m[8]*m[5]*m[14] - m[8]*m[6]*m[13] - m[12]*m[5]*m[10] + m[12]*m[6]*m[9];
        inv.m[1] = -m[1]*m[10]*m[15] + m[1]*m[11]*m[14] + m[9]*m[2]*m[15] - m[9]*m[3]*m[14] - m[13]*m[2]*m[11] + m[13]*m[3]*m[10];
        inv.m[5] = m[0]*m[10]*m[15] - m[0]*m[11]*m[14] - m[8]*m[2]*m[15] + m[8]*m[3]*m[14] + m[12]*m[2]*m[11] - m[12]*m[3]*m[10];
        inv.m[9] = -m[0]*m[9]*m[15] + m[0]*m[11]*m[13] + m[8]*m[1]*m[15] - m[8]*m[3]*m[13] - m[12]*m[1]*m[11] + m[12]*m[3]*m[9];
        inv.m[13] = m[0]*m[9]*m[14] - m[0]*m[10]*m[13] - m[8]*m[1]*m[14] + m[8]*m[2]*m[13] + m[12]*m[1]*m[10] - m[12]*m[2]*m[9];
        inv.m[2] = m[1]*m[6]*m[15] - m[1]*m[7]*m[14] - m[5]*m[2]*m[15] + m[5]*m[3]*m[14] + m[13]*m[2]*m[7] - m[13]*m[3]*m[6];
        inv.m[6] = -m[0]*m[6]*m[15] + m[0]*m[7]*m[14] + m[4]*m[2]*m[15] - m[4]*m[3]*m[14] - m[12]*m[2]*m[7] + m[12]*m[3]*m[6];
        inv.m[10] = m[0]*m[5]*m[15] - m[0]*m[7]*m[13] - m[4]*m[1]*m[15] + m[4]*m[3]*m[13] + m[12]*m[1]*m[7] - m[12]*m[3]*m[5];
        inv.m[14] = -m[0]*m[5]*m[14] + m[0]*m[6]*m[13] + m[4]*m[1]*m[14] - m[4]*m[2]*m[13] - m[12]*m[1]*m[6] + m[12]*m[2]*m[5];
        inv.m[3] = -m[1]*m[6]*m[11] + m[1]*m[7]*m[10] + m[5]*m[2]*m[11] - m[5]*m[3]*m[10] - m[9]*m[2]*m[7] + m[9]*m[3]*m[6];
        inv.m[7] = m[0]*m[6]*m[11] - m[0]*m[7]*m[10] - m[4]*m[2]*m[11] + m[4]*m[3]*m[10] + m[8]*m[2]*m[7] - m[8]*m[3]*m[6];
        inv.m[11] = -m[0]*m[5]*m[11] + m[0]*m[7]*m[9] + m[4]*m[1]*m[11] - m[4]*m[3]*m[9] - m[8]*m[1]*m[7] + m[8]*m[3]*m[5];
        inv.m[15] = m[0]*m[5]*m[10] - m[0]*m[6]*m[9] - m[4]*m[1]*m[10] + m[4]*m[2]*m[9] + m[8]*m[1]*m[6] - m[8]*m[2]*m[5];

        let det = m[0]*inv.m[0] + m[1]*inv.m[4] + m[2]*inv.m[8] + m[3]*inv.m[12];
        if (det === 0) return Matrix.identity();

        det = 1.0 / det;
        for (let i = 0; i < 16; i++) inv.m[i] *= det;
        return inv;
    }

    clone() {
        const m = new Matrix();
        m.m.set(this.m);
        return m;
    }
}

export class BoundingBox {
    constructor(min = null, max = null) {
        this.min = min || new Vector3(-1, -1, -1);
        this.max = max || new Vector3(1, 1, 1);
    }

    getCenter() { return this.min.add(this.max).divide(2); }
    getSize() { return this.max.subtract(this.min); }
    getExtents() { return this.getSize().divide(2); }

    containsPoint(point) {
        return point.x >= this.min.x && point.x <= this.max.x &&
               point.y >= this.min.y && point.y <= this.max.y &&
               point.z >= this.min.z && point.z <= this.max.z;
    }

    intersects(box) {
        return !(box.max.x < this.min.x || box.min.x > this.max.x ||
                 box.max.y < this.min.y || box.min.y > this.max.y ||
                 box.max.z < this.min.z || box.min.z > this.max.z);
    }

    merge(box) {
        return new BoundingBox(
            new Vector3(Math.min(this.min.x, box.min.x), Math.min(this.min.y, box.min.y), Math.min(this.min.z, box.min.z)),
            new Vector3(Math.max(this.max.x, box.max.x), Math.max(this.max.y, box.max.y), Math.max(this.max.z, box.max.z))
        );
    }

    expand(amount) {
        const half = amount / 2;
        return new BoundingBox(
            this.min.subtract(new Vector3(half, half, half)),
            this.max.add(new Vector3(half, half, half))
        );
    }

    clone() { return new BoundingBox(this.min.clone(), this.max.clone()); }
}

export class Component {
    constructor() {
        this.enabled = true;
        this.entity = null;
        this.started = false;
    }

    onStart() {}
    onUpdate(deltaTime) {}
    onLateUpdate(deltaTime) {}
    onDestroy() {}
    onEnable() {}
    onDisable() {}

    getTransform() { return this.entity ? this.entity.transform : null; }
    getGameObject() { return this.entity; }

    setEnabled(enabled) {
        if (this.enabled === enabled) return;
        this.enabled = enabled;
        if (enabled) this.onEnable();
        else this.onDisable();
    }

    destroy() {
        if (this.entity) {
            this.entity.removeComponent(this);
        }
        this.onDestroy();
    }
}

export class Entity {
    constructor(name = 'Entity', classType = ENTITY_CLASS.BASE) {
        this.id = Entity.nextId++;
        this.name = name;
        this.classType = classType;
        this.type = ENTITY_TYPE.ENTITY;

        this.active = true;
        this.state = ENTITY_STATE.ACTIVE;
        this.visible = true;
        this.persistent = false;
        this.static = false;
        this.tags = new Set();
        this.layer = 0;

        this.transform = new Transform();
        this.boundingBox = new BoundingBox();
        this.components = [];
        this.children = [];
        this.parent = null;

        this.variables = new Map();
        this.networkId = null;
        this.ownerId = null;
        this.creatorId = null;

        this.createdAt = Date.now();
        this.destroyed = false;
        this.destroyTime = null;

        this.onUpdate = null;
        this.onLateUpdate = null;
        this.onDestroy = null;
        this.onCollisionEnter = null;
        this.onCollisionStay = null;
        this.onCollisionExit = null;
    }

    static nextId = 0;

    addComponent(component) {
        component.entity = this;
        this.components.push(component);
        if (this.active && component.enabled) {
            component.onStart();
            component.started = true;
        }
        return component;
    }

    removeComponent(component) {
        const index = this.components.indexOf(component);
        if (index !== -1) {
            component.onDestroy();
            this.components.splice(index, 1);
            component.entity = null;
            return true;
        }
        return false;
    }

    getComponent(type) {
        for (const comp of this.components) {
            if (comp instanceof type) return comp;
        }
        return null;
    }

    getComponents(type) {
        return this.components.filter(comp => comp instanceof type);
    }

    getComponentInChildren(type) {
        for (const child of this.children) {
            const comp = child.getComponent(type);
            if (comp) return comp;
        }
        return null;
    }

    addChild(entity) {
        if (entity.parent) {
            entity.parent.removeChild(entity);
        }
        entity.parent = this;
        this.children.push(entity);
        entity.transform.parent = this.transform;
        return entity;
    }

    removeChild(entity) {
        const index = this.children.indexOf(entity);
        if (index !== -1) {
            this.children.splice(index, 1);
            entity.parent = null;
            entity.transform.parent = null;
            return true;
        }
        return false;
    }

    setPosition(x, y, z) { this.transform.setPosition(x, y, z); return this; }
    setRotation(x, y, z) { this.transform.setRotation(x, y, z); return this; }
    setScale(x, y, z) { this.transform.scale = new Vector3(x, y, z); return this; }
    getPosition() { return this.transform.position; }
    getRotation() { return this.transform.rotation; }
    getForward() { return this.transform.getForward(); }
    getRight() { return this.transform.getRight(); }
    getUp() { return this.transform.getUp(); }

    translate(x, y, z) { this.transform.translate(x, y, z); return this; }
    rotate(x, y, z) { this.transform.rotate(x, y, z); return this; }
    lookAt(target) { this.transform.lookAt(target); return this; }

    setActive(active) {
        if (this.active === active) return;
        this.active = active;
        for (const comp of this.components) {
            comp.setEnabled(active && comp.enabled);
        }
        for (const child of this.children) {
            child.setActive(active);
        }
    }

    setVisible(visible) { this.visible = visible; return this; }
    setLayer(layer) { this.layer = layer; return this; }

    addTag(tag) { this.tags.add(tag); return this; }
    removeTag(tag) { this.tags.delete(tag); return this; }
    hasTag(tag) { return this.tags.has(tag); }
    hasTags(...tags) { return tags.every(tag => this.tags.has(tag)); }

    setVariable(name, value) { this.variables.set(name, value); return this; }
    getVariable(name) { return this.variables.get(name); }
    hasVariable(name) { return this.variables.has(name); }
    deleteVariable(name) { return this.variables.delete(name); }

    distanceTo(other) {
        return this.transform.position.subtract(other.transform.position).length();
    }

    directionTo(other) {
        return other.transform.position.subtract(this.transform.position).normalize();
    }

    update(deltaTime) {
        if (!this.active || this.state === ENTITY_STATE.FROZEN) return;

        for (const comp of this.components) {
            if (comp.enabled && comp.started) {
                comp.onUpdate(deltaTime);
            }
        }

        if (this.onUpdate) this.onUpdate(deltaTime);

        for (const child of this.children) {
            child.update(deltaTime);
        }
    }

    lateUpdate(deltaTime) {
        if (!this.active) return;

        for (const comp of this.components) {
            if (comp.enabled && comp.started) {
                comp.onLateUpdate(deltaTime);
            }
        }

        if (this.onLateUpdate) this.onLateUpdate(deltaTime);

        for (const child of this.children) {
            child.lateUpdate(deltaTime);
        }
    }

    destroy(delay = 0) {
        if (this.destroyed) return;
        if (delay > 0) {
            this.destroyTime = Date.now() + delay;
        } else {
            this.performDestroy();
        }
    }

    performDestroy() {
        if (this.destroyed) return;
        this.destroyed = true;

        for (const child of [...this.children]) {
            child.destroy();
        }

        for (const comp of [...this.components]) {
            comp.destroy();
        }

        if (this.parent) {
            this.parent.removeChild(this);
        }

        if (this.onDestroy) this.onDestroy();
    }

    clone() {
        const cloned = new Entity(this.name + '_clone', this.classType);
        cloned.type = this.type;
        cloned.active = this.active;
        cloned.visible = this.visible;
        cloned.tags = new Set(this.tags);
        cloned.layer = this.layer;

        cloned.transform = this.transform.clone();

        for (const [name, value] of this.variables) {
            cloned.variables.set(name, value);
        }

        for (const comp of this.components) {
            const compClone = Object.create(Object.getPrototypeOf(comp));
            Object.assign(compClone, comp);
            compClone.entity = cloned;
            cloned.components.push(compClone);
        }

        for (const child of this.children) {
            const childClone = child.clone();
            cloned.addChild(childClone);
        }

        return cloned;
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            classType: this.classType,
            type: this.type,
            active: this.active,
            visible: this.visible,
            tags: Array.from(this.tags),
            layer: this.layer,
            position: { x: this.transform.position.x, y: this.transform.position.y, z: this.transform.position.z },
            rotation: { x: this.transform.rotation.x, y: this.transform.rotation.y, z: this.transform.rotation.z, w: this.transform.rotation.w },
            scale: { x: this.transform.scale.x, y: this.transform.scale.y, z: this.transform.scale.z },
            variables: Object.fromEntries(this.variables),
            children: this.children.map(c => c.toJSON())
        };
    }

    static fromJSON(data) {
        const entity = new Entity(data.name, data.classType);
        entity.type = data.type;
        entity.active = data.active;
        entity.visible = data.visible;
        entity.tags = new Set(data.tags || []);
        entity.layer = data.layer || 0;

        entity.transform.position = new Vector3(data.position?.x || 0, data.position?.y || 0, data.position?.z || 0);
        entity.transform.rotation = new Quaternion(data.rotation?.x || 0, data.rotation?.y || 0, data.rotation?.z || 0, data.rotation?.w || 1);
        entity.transform.scale = new Vector3(data.scale?.x || 1, data.scale?.y || 1, data.scale?.z || 1);

        entity.variables = new Map(Object.entries(data.variables || {}));

        for (const childData of data.children || []) {
            const child = Entity.fromJSON(childData);
            entity.addChild(child);
        }

        return entity;
    }
}

export class PlayerEntity extends Entity {
    constructor(name = 'Player') {
        super(name, ENTITY_CLASS.PLAYER);
        this.type = ENTITY_TYPE.PLAYER;

        this.health = 100;
        this.maxHealth = 100;
        this.armor = 0;
        this.helmet = false;
        this.team = 'none';
        this.alive = true;
        this.observer = false;

        this.velocity = new Vector3();
        this.angularVelocity = new Vector3();
        this.moveDirection = new Vector3();
        this.viewDirection = new Vector3(0, 0, 1);

        this.speed = 250;
        this.jumpSpeed = 270;
        this.gravity = 800;

        this.grounded = false;
        this.ducking = false;
        this.walking = false;
        this.running = false;

        this.weapons = [];
        this.activeWeapon = null;
        this.lastDamageTime = 0;
        this.lastDamageAmount = 0;
        this.lastDamageOrigin = null;

        this.duckAmount = 0;
        this.duckSpeed = 8;

        this.ladder = false;
        this.ladderVelocity = new Vector3();

        this.flashDuration = 0;
        this.flashAmount = 0;

        this.burnTime = 0;
        this.frozen = false;
        this.frozenDuration = 0;

        this.interacting = false;
        this.interactTarget = null;
        this.interactDistance = 0;

        this.viewBob = 0;
        this.viewBobSpeed = 6;
        this.viewBobAmount = 0.02;
    }

    takeDamage(amount, attacker = null, damageType = 0, hitGroup = 0) {
        if (!this.alive) return;

        const previousHealth = this.health;
        this.health = Math.max(0, this.health - amount);
        this.lastDamageTime = Date.now();
        this.lastDamageAmount = amount;

        if (attacker) {
            this.lastDamageOrigin = attacker.getPosition();
        }

        if (this.health <= 0) {
            this.die(attacker, damageType, hitGroup);
        }

        return {
            previousHealth,
            currentHealth: this.health,
            damage: amount,
            lethal: this.health <= 0
        };
    }

    heal(amount) {
        if (!this.alive) return 0;
        const previousHealth = this.health;
        this.health = Math.min(this.maxHealth, this.health + amount);
        return this.health - previousHealth;
    }

    addArmor(amount) {
        this.armor = Math.min(100, this.armor + amount);
        return this.armor;
    }

    die(attacker = null, damageType = 0, hitGroup = 0) {
        this.alive = false;
        this.health = 0;
        this.velocity = new Vector3();
        this.activeWeapon = null;
    }

    respawn(position = null) {
        this.alive = true;
        this.health = this.maxHealth;
        this.armor = 0;
        this.helmet = false;
        this.velocity = new Vector3();
        this.grounded = false;
        this.ducking = false;

        if (position) {
            this.setPosition(position.x, position.y, position.z);
        }
    }

    flash(duration, amount = 1) {
        this.flashDuration = Math.max(this.flashDuration, duration);
        this.flashAmount = Math.max(this.flashAmount, amount);
    }

    burn(duration) {
        this.burnTime = Math.max(this.burnTime, duration);
    }

    freeze(duration) {
        this.frozen = true;
        this.frozenDuration = duration;
    }

    move(x, y, z) {
        if (this.frozen) return;
        this.moveDirection = new Vector3(x, y, z);
    }

    jump() {
        if (!this.grounded || this.frozen) return false;
        this.velocity.y = this.jumpSpeed;
        this.grounded = false;
        return true;
    }

    crouch() {
        this.ducking = true;
    }

    stand() {
        this.ducking = false;
    }

    update(deltaTime) {
        if (this.frozen) {
            this.frozenDuration -= deltaTime;
            if (this.frozenDuration <= 0) {
                this.frozen = false;
            }
            return;
        }

        if (this.flashDuration > 0) {
            this.flashDuration -= deltaTime;
            if (this.flashDuration <= 0) {
                this.flashAmount = 0;
            }
        }

        if (this.burnTime > 0) {
            this.burnTime -= deltaTime;
            this.takeDamage(deltaTime * 5);
        }

        if (this.ducking) {
            this.duckAmount = Math.min(1, this.duckAmount + this.duckSpeed * deltaTime);
        } else {
            this.duckAmount = Math.max(0, this.duckAmount - this.duckSpeed * deltaTime);
        }

        const currentDuckSpeed = this.speed * (1 - this.duckAmount * 0.4);
        const forward = this.getForward();
        forward.y = 0;
        forward.normalize();
        const right = this.getRight();
        right.y = 0;
        right.normalize();

        const moveVector = forward.multiply(this.moveDirection.z * currentDuckSpeed * deltaTime)
                                  .add(right.multiply(this.moveDirection.x * currentDuckSpeed * deltaTime));

        this.transform.position = this.transform.position.add(moveVector);

        if (this.grounded) {
            this.velocity.y = 0;
        } else {
            this.velocity.y -= this.gravity * deltaTime;
        }

        this.transform.position.y += this.velocity.y * deltaTime;

        if (this.walking) {
            this.viewBob += deltaTime * this.viewBobSpeed * 0.5;
        } else {
            this.viewBob += deltaTime * this.viewBobSpeed;
        }

        super.update(deltaTime);
    }
}

export class WeaponEntity extends Entity {
    constructor(weaponName = 'knife') {
        super(weaponName, ENTITY_CLASS.WEAPON);
        this.type = ENTITY_TYPE.WEAPON;

        this.weaponName = weaponName;
        this.weaponClass = 'knife';
        this.ammo = 0;
        this.ammoReserve = 0;
        this.maxAmmo = 0;
        this.maxAmmoReserve = 0;

        this.fireRate = 0;
        this.recoil = 0;
        this.recoilPattern = [];
        this.spread = 0;
        this.damage = 0;

        this.state = 'idle';
        this.lastFireTime = 0;
        this.fireCooldown = 0;

        this.reloading = false;
        this.reloadTime = 0;
        this.reloadProgress = 0;

        this.owner = null;
        this.viewModel = null;
        this.worldModel = null;

        this.muzzlePosition = new Vector3(0, 0, 0);
        this.ejectPosition = new Vector3(0, 0, 0);

        this.zoomLevel = 0;
        this.zoomLevels = [0];
        this.zoomTime = 0;

        this.accuracy = 1.0;
        this.stability = 1.0;

        this.silencer = false;
        this.fireMode = 'auto';
        this.supportedFireModes = ['auto'];

        this.penetration = 1;
        this.armorRatio = 1.0;
    }

    canFire() {
        if (this.reloading) return false;
        if (this.ammo <= 0) return false;
        if (Date.now() - this.lastFireTime < this.fireCooldown) return false;
        return true;
    }

    fire() {
        if (!this.canFire()) return null;

        this.ammo--;
        this.lastFireTime = Date.now();
        this.state = 'firing';

        this.stability = Math.max(0.1, this.stability - 0.05);

        return {
            position: this.getMuzzlePosition(),
            direction: this.owner ? this.owner.getForward() : new Vector3(0, 0, 1),
            spread: this.spread * (1 - this.stability),
            damage: this.damage,
            weapon: this.weaponName
        };
    }

    reload() {
        if (this.reloading) return false;
        if (this.ammo >= this.maxAmmo) return false;
        if (this.ammoReserve <= 0) return false;

        this.reloading = true;
        this.reloadProgress = 0;
        this.state = 'reloading';

        return true;
    }

    completeReload() {
        const needed = this.maxAmmo - this.ammo;
        const toLoad = Math.min(needed, this.ammoReserve);

        this.ammo += toLoad;
        this.ammoReserve -= toLoad;
        this.reloading = false;
        this.reloadProgress = 0;
        this.stability = 1.0;
        this.state = 'idle';
    }

    cancelReload() {
        this.reloading = false;
        this.reloadProgress = 0;
        this.state = 'idle';
    }

    zoom() {
        if (this.zoomLevel < this.zoomLevels.length - 1) {
            this.zoomLevel++;
            this.zoomTime = Date.now();
        }
    }

    unzoom() {
        if (this.zoomLevel > 0) {
            this.zoomLevel--;
            this.zoomTime = Date.now();
        }
    }

    getMuzzlePosition() {
        const worldPos = this.transform.localToWorld(this.muzzlePosition);
        return worldPos;
    }

    getEjectPosition() {
        const worldPos = this.transform.localToWorld(this.ejectPosition);
        return worldPos;
    }

    update(deltaTime) {
        if (this.reloading) {
            this.reloadProgress += deltaTime / this.reloadTime;
            if (this.reloadProgress >= 1) {
                this.completeReload();
            }
        }

        if (!this.reloading && this.stability < 1.0) {
            this.stability = Math.min(1.0, this.stability + deltaTime * 0.1);
        }

        super.update(deltaTime);
    }

    toJSON() {
        return {
            ...super.toJSON(),
            weaponName: this.weaponName,
            weaponClass: this.weaponClass,
            ammo: this.ammo,
            ammoReserve: this.ammoReserve,
            maxAmmo: this.maxAmmo,
            maxAmmoReserve: this.maxAmmoReserve,
            fireRate: this.fireRate,
            recoil: this.recoil,
            spread: this.spread,
            damage: this.damage,
            state: this.state,
            fireMode: this.fireMode,
            silencer: this.silencer,
            ownerId: this.owner?.id
        };
    }
}

export class GrenadeEntity extends Entity {
    constructor(type = 'frag') {
        super(type, ENTITY_CLASS.GRENADE);
        this.type = ENTITY_TYPE.PROJECTILE;

        this.grenadeType = type;
        this.damage = 100;
        this.radius = 150;
        this.fuseTime = 3.0;

        this.thrownBy = null;
        this.thrownAt = Date.now();
        this.exploded = false;

        this.velocity = new Vector3();
        this.angularVelocity = new Vector3();

        this.bouncing = false;
        this.bounceCount = 0;
        this.maxBounces = 3;
        this.bounceRestitution = 0.6;

        this.inAir = true;
        this.landTime = 0;

        this.active = false;
    }

    throw(position, direction, force = 800) {
        this.setPosition(position.x, position.y, position.z);
        this.velocity = direction.normalize().multiply(force);
        this.angularVelocity = new Vector3(
            (Math.random() - 0.5) * 20,
            (Math.random() - 0.5) * 20,
            (Math.random() - 0.5) * 20
        );
        this.active = true;
        this.exploded = false;
        this.inAir = true;
        this.thrownAt = Date.now();
        this.bounceCount = 0;
    }

    update(deltaTime) {
        if (!this.active || this.exploded) return;

        const elapsed = (Date.now() - this.thrownAt) / 1000;

        if (elapsed >= this.fuseTime) {
            this.explode();
            return;
        }

        if (this.inAir) {
            this.velocity.y -= 400 * deltaTime;
        }

        this.transform.position = this.transform.position.add(this.velocity.multiply(deltaTime));

        this.transform.rotation.x += this.angularVelocity.x * deltaTime;
        this.transform.rotation.y += this.angularVelocity.y * deltaTime;
        this.transform.rotation.z += this.angularVelocity.z * deltaTime;

        super.update(deltaTime);
    }

    explode() {
        if (this.exploded) return;
        this.exploded = true;
        this.active = false;
    }

    bounce(normal) {
        if (this.bounceCount >= this.maxBounces) {
            this.inAir = false;
            return;
        }

        const dot = this.velocity.dot(normal);
        this.velocity = this.velocity.subtract(normal.multiply(2 * dot)).multiply(this.bounceRestitution);

        this.angularVelocity = this.angularVelocity.multiply(0.8);

        this.bounceCount++;
    }
}

export class PickupEntity extends Entity {
    constructor(itemName = 'healthkit') {
        super(itemName, ENTITY_CLASS.PICKUP);
        this.type = ENTITY_TYPE.PICKUP;

        this.itemName = itemName;
        this.itemType = 'health';
        this.itemValue = 100;

        this.respawn = true;
        this.respawnTime = 15;
        this.respawnProgress = 0;
        this.available = true;

        this.pickedUpBy = null;
        this.pickupRadius = 50;

        this.pickupSound = 'pickup';
        this.useTime = 0;
    }

    canPickup(player) {
        if (!this.available) return false;
        if (player.distanceTo(this) > this.pickupRadius) return false;
        return true;
    }

    pickup(player) {
        if (!this.canPickup(player)) return false;

        this.available = false;
        this.pickedUpBy = player;
        this.pickupTime = Date.now();

        this.applyEffect(player);

        if (this.respawn) {
            this.scheduleRespawn();
        } else {
            this.destroy();
        }

        return true;
    }

    applyEffect(player) {
        switch (this.itemType) {
            case 'health':
                player.heal(this.itemValue);
                break;
            case 'armor':
                player.addArmor(this.itemValue);
                break;
            case 'ammo':
                if (player.activeWeapon) {
                    player.activeWeapon.ammoReserve = Math.min(
                        player.activeWeapon.maxAmmoReserve,
                        player.activeWeapon.ammoReserve + this.itemValue
                    );
                }
                break;
            case 'grenade':
                break;
            case 'weapon':
                break;
        }
    }

    scheduleRespawn() {
        this.respawnProgress = 0;
    }

    update(deltaTime) {
        if (!this.available && this.respawn) {
            this.respawnProgress += deltaTime / this.respawnTime;
            if (this.respawnProgress >= 1) {
                this.respawn();
            }
        }

        super.update(deltaTime);
    }

    respawn() {
        this.available = true;
        this.pickedUpBy = null;
        this.respawnProgress = 0;
        this.setActive(true);
    }
}

export class EntityManager {
    constructor() {
        this.entities = new Map();
        this.entitiesByClass = new Map();
        this.entitiesByTag = new Map();
        this.entitiesByType = new Map();
        this.rootEntities = [];

        this.nextEntityId = 0;
        this.destroyQueue = [];
        this.active = true;
    }

    createEntity(name = 'Entity', classType = ENTITY_CLASS.BASE) {
        const entity = new Entity(name, classType);
        this.registerEntity(entity);
        return entity;
    }

    registerEntity(entity) {
        entity.id = this.nextEntityId++;
        this.entities.set(entity.id, entity);

        if (!this.entitiesByClass.has(entity.classType)) {
            this.entitiesByClass.set(entity.classType, new Set());
        }
        this.entitiesByClass.get(entity.classType).add(entity);

        if (!this.entitiesByType.has(entity.type)) {
            this.entitiesByType.set(entity.type, new Set());
        }
        this.entitiesByType.get(entity.type).add(entity);

        for (const tag of entity.tags) {
            if (!this.entitiesByTag.has(tag)) {
                this.entitiesByTag.set(tag, new Set());
            }
            this.entitiesByTag.get(tag).add(entity);
        }

        if (!entity.parent) {
            this.rootEntities.push(entity);
        }

        return entity;
    }

    removeEntity(entity) {
        if (this.destroyQueue.includes(entity)) return false;

        this.destroyQueue.push(entity);
        return true;
    }

    destroyQueuedEntities() {
        for (const entity of this.destroyQueue) {
            this.performDestroy(entity);
        }
        this.destroyQueue = [];
    }

    performDestroy(entity) {
        entity.performDestroy();

        this.entities.delete(entity.id);

        const classSet = this.entitiesByClass.get(entity.classType);
        if (classSet) classSet.delete(entity);

        const typeSet = this.entitiesByType.get(entity.type);
        if (typeSet) typeSet.delete(entity);

        for (const tag of entity.tags) {
            const tagSet = this.entitiesByTag.get(tag);
            if (tagSet) tagSet.delete(entity);
        }

        const rootIndex = this.rootEntities.indexOf(entity);
        if (rootIndex !== -1) this.rootEntities.splice(rootIndex, 1);
    }

    findById(id) {
        return this.entities.get(id) || null;
    }

    findByName(name) {
        for (const entity of this.entities.values()) {
            if (entity.name === name) return entity;
        }
        return null;
    }

    findByClass(classType) {
        return Array.from(this.entitiesByClass.get(classType) || []);
    }

    findByType(type) {
        return Array.from(this.entitiesByType.get(type) || []);
    }

    findByTag(tag) {
        return Array.from(this.entitiesByTag.get(tag) || []);
    }

    findByTags(...tags) {
        let result = null;
        for (const tag of tags) {
            const set = this.entitiesByTag.get(tag);
            if (!set) return [];
            if (!result) result = new Set(set);
            else {
                for (const entity of result) {
                    if (!set.has(entity)) result.delete(entity);
                }
            }
        }
        return Array.from(result || []);
    }

    findAll(predicate) {
        const result = [];
        for (const entity of this.entities.values()) {
            if (predicate(entity)) result.push(entity);
        }
        return result;
    }

    findNearest(position, maxDistance = Infinity, type = null) {
        let nearest = null;
        let nearestDist = maxDistance;

        const entities = type ? this.findByType(type) : Array.from(this.entities.values());

        for (const entity of entities) {
            if (!entity.active) continue;
            const dist = position.subtract(entity.getPosition()).length();
            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = entity;
            }
        }

        return nearest;
    }

    findInRadius(position, radius, type = null) {
        const result = [];
        const entities = type ? this.findByType(type) : Array.from(this.entities.values());

        for (const entity of entities) {
            if (!entity.active) continue;
            const dist = position.subtract(entity.getPosition()).length();
            if (dist <= radius) {
                result.push({ entity, distance: dist });
            }
        }

        return result.sort((a, b) => a.distance - b.distance);
    }

    update(deltaTime) {
        if (!this.active) return;

        for (const entity of this.rootEntities) {
            entity.update(deltaTime);
        }

        for (const entity of this.rootEntities) {
            entity.lateUpdate(deltaTime);
        }

        this.destroyQueuedEntities();
    }

    clear() {
        for (const entity of this.entities.values()) {
            entity.performDestroy();
        }
        this.entities.clear();
        this.entitiesByClass.clear();
        this.entitiesByTag.clear();
        this.entitiesByType.clear();
        this.rootEntities = [];
        this.destroyQueue = [];
    }

    getEntityCount() {
        return this.entities.size;
    }

    getActiveEntityCount() {
        let count = 0;
        for (const entity of this.entities.values()) {
            if (entity.active) count++;
        }
        return count;
    }
}

export class EntitySerializer {
    static serialize(entity) {
        return JSON.stringify(entity.toJSON());
    }

    static deserialize(json) {
        const data = JSON.parse(json);
        return Entity.fromJSON(data);
    }

    static serializeMany(entities) {
        return JSON.stringify(entities.map(e => e.toJSON()));
    }

    static deserializeMany(json) {
        const data = JSON.parse(json);
        return data.map(d => Entity.fromJSON(d));
    }
}

export default {
    ENTITY_CLASS,
    ENTITY_STATE,
    ENTITY_TYPE,
    Vector3,
    Quaternion,
    Transform,
    Matrix,
    BoundingBox,
    Component,
    Entity,
    PlayerEntity,
    WeaponEntity,
    GrenadeEntity,
    PickupEntity,
    EntityManager,
    EntitySerializer
};

export class Vector3 {
    constructor(x = 0, y = 0, z = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    clone() {
        return new Vector3(this.x, this.y, this.z);
    }

    set(x, y, z) {
        this.x = x;
        this.y = y;
        this.z = z;
        return this;
    }

    copy(v) {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
        return this;
    }

    add(v) {
        return new Vector3(this.x + v.x, this.y + v.y, this.z + v.z);
    }

    addScaledVector(v, scale) {
        return new Vector3(
            this.x + v.x * scale,
            this.y + v.y * scale,
            this.z + v.z * scale
        );
    }

    sub(v) {
        return new Vector3(this.x - v.x, this.y - v.y, this.z - v.z);
    }

    subVectors(a, b) {
        this.x = a.x - b.x;
        this.y = a.y - b.y;
        this.z = a.z - b.z;
        return this;
    }

    multiply(scalar) {
        return new Vector3(this.x * scalar, this.y * scalar, this.z * scalar);
    }

    multiplyComponents(v) {
        return new Vector3(this.x * v.x, this.y * v.y, this.z * v.z);
    }

    divide(scalar) {
        if (scalar !== 0) {
            return new Vector3(this.x / scalar, this.y / scalar, this.z / scalar);
        }
        return this.clone();
    }

    dot(v) {
        return this.x * v.x + this.y * v.y + this.z * v.z;
    }

    cross(v) {
        return new Vector3(
            this.y * v.z - this.z * v.y,
            this.z * v.x - this.x * v.z,
            this.x * v.y - this.y * v.x
        );
    }

    length() {
        return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
    }

    lengthSq() {
        return this.x * this.x + this.y * this.y + this.z * this.z;
    }

    normalize() {
        const len = this.length();
        if (len > 0) {
            return this.divide(len);
        }
        return this.clone();
    }

    negate() {
        return new Vector3(-this.x, -this.y, -this.z);
    }

    distanceTo(v) {
        return this.sub(v).length();
    }

    distanceToSquared(v) {
        const dx = this.x - v.x;
        const dy = this.y - v.y;
        const dz = this.z - v.z;
        return dx * dx + dy * dy + dz * dz;
    }

    lerp(v, alpha) {
        return new Vector3(
            this.x + (v.x - this.x) * alpha,
            this.y + (v.y - this.y) * alpha,
            this.z + (v.z - this.z) * alpha
        );
    }

    angleTo(v) {
        const denominator = Math.sqrt(this.lengthSq() * v.lengthSq());
        if (denominator === 0) return Math.PI / 2;
        const theta = this.dot(v) / denominator;
        return Math.acos(Math.max(-1, Math.min(1, theta)));
    }

    reflect(normal) {
        const d = this.dot(normal) * 2;
        return new Vector3(
            this.x - normal.x * d,
            this.y - normal.y * d,
            this.z - normal.z * d
        );
    }

    projectOnVector(v) {
        const d = v.dot(v);
        if (d === 0) return this.clone();
        return v.multiply(this.dot(v) / d);
    }

    projectOnPlane(planeNormal) {
        const d = planeNormal.dot(planeNormal);
        if (d === 0) return this.clone();
        return this.sub(planeNormal.multiply(this.dot(planeNormal) / d));
    }

    rotateAround(axis, angle) {
        const q = Quaternion.fromAxisAngle(axis, angle);
        const result = q.multiplyVector3(this);
        return result;
    }

    clamp(min, max) {
        return new Vector3(
            Math.max(min.x, Math.min(max.x, this.x)),
            Math.max(min.y, Math.min(max.y, this.y)),
            Math.max(min.z, Math.min(max.z, this.z))
        );
    }

    floor() {
        return new Vector3(Math.floor(this.x), Math.floor(this.y), Math.floor(this.z));
    }

    ceil() {
        return new Vector3(Math.ceil(this.x), Math.ceil(this.y), Math.ceil(this.z));
    }

    round() {
        return new Vector3(Math.round(this.x), Math.round(this.y), Math.round(this.z));
    }

    isZero() {
        return this.x === 0 && this.y === 0 && this.z === 0;
    }

    equals(v, epsilon = 0.0001) {
        return (
            Math.abs(this.x - v.x) < epsilon &&
            Math.abs(this.y - v.y) < epsilon &&
            Math.abs(this.z - v.z) < epsilon
        );
    }

    fromArray(array, offset = 0) {
        this.x = array[offset];
        this.y = array[offset + 1];
        this.z = array[offset + 2];
        return this;
    }

    toArray() {
        return [this.x, this.y, this.z];
    }

    static zero() {
        return new Vector3(0, 0, 0);
    }

    static one() {
        return new Vector3(1, 1, 1);
    }

    static up() {
        return new Vector3(0, 1, 0);
    }

    static down() {
        return new Vector3(0, -1, 0);
    }

    static forward() {
        return new Vector3(0, 0, 1);
    }

    static back() {
        return new Vector3(0, 0, -1);
    }

    static left() {
        return new Vector3(-1, 0, 0);
    }

    static right() {
        return new Vector3(1, 0, 0);
    }
}

export class Quaternion {
    constructor(x = 0, y = 0, z = 0, w = 1) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    clone() {
        return new Quaternion(this.x, this.y, this.z, this.w);
    }

    copy(q) {
        this.x = q.x;
        this.y = q.y;
        this.z = q.z;
        this.w = q.w;
        return this;
    }

    set(x, y, z, w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        return this;
    }

    setFromEuler(euler) {
        const x = euler.x;
        const y = euler.y;
        const z = euler.z;
        const order = euler.order || 'XYZ';

        const c1 = Math.cos(x / 2);
        const c2 = Math.cos(y / 2);
        const c3 = Math.cos(z / 2);

        const s1 = Math.sin(x / 2);
        const s2 = Math.sin(y / 2);
        const s3 = Math.sin(z / 2);

        switch (order) {
            case 'XYZ':
                this.x = s1 * c2 * c3 + c1 * s2 * s3;
                this.y = c1 * s2 * c3 - s1 * c2 * s3;
                this.z = c1 * c2 * s3 + s1 * s2 * c3;
                this.w = c1 * c2 * c3 - s1 * s2 * s3;
                break;
            case 'YXZ':
                this.x = s1 * c2 * c3 + c1 * s2 * s3;
                this.y = c1 * s2 * c3 - s1 * c2 * s3;
                this.z = c1 * c2 * s3 - s1 * s2 * c3;
                this.w = c1 * c2 * c3 + s1 * s2 * s3;
                break;
            case 'ZXY':
                this.x = s1 * c2 * c3 - c1 * s2 * c3;
                this.y = c1 * s2 * c3 + s1 * c2 * s3;
                this.z = c1 * c2 * s3 + s1 * s2 * c3;
                this.w = c1 * c2 * c3 - s1 * s2 * s3;
                break;
            case 'ZYX':
                this.x = s1 * c2 * c3 - c1 * s2 * s3;
                this.y = c1 * s2 * c3 + s1 * c2 * s3;
                this.z = c1 * c2 * s3 - s1 * s2 * c3;
                this.w = c1 * c2 * c3 + s1 * s2 * s3;
                break;
            case 'YZX':
                this.x = s1 * c2 * c3 + c1 * s2 * s3;
                this.y = c1 * s2 * c3 + s1 * c2 * s3;
                this.z = c1 * c2 * s3 - s1 * s2 * c3;
                this.w = c1 * c2 * c3 - s1 * s2 * s3;
                break;
            case 'XZY':
                this.x = s1 * c2 * c3 - c1 * s2 * s3;
                this.y = c1 * s2 * c3 - s1 * c2 * s3;
                this.z = c1 * c2 * s3 + s1 * s2 * c3;
                this.w = c1 * c2 * c3 + s1 * s2 * s3;
                break;
        }

        return this;
    }

    setFromAxisAngle(axis, angle) {
        const halfAngle = angle / 2;
        const s = Math.sin(halfAngle);
        this.x = axis.x * s;
        this.y = axis.y * s;
        this.z = axis.z * s;
        this.w = Math.cos(halfAngle);
        return this;
    }

    setFromUnitVectors(from, to) {
        let r = from.dot(to) + 1;
        if (r < 0.000001) {
            if (Math.abs(from.x) > Math.abs(from.z)) {
                this.set(-from.y, from.x, 0, 0);
            } else {
                this.set(0, -from.z, from.y, 0);
            }
        } else {
            this.crossVectors(from, to);
            this.x += r;
            this.normalize();
        }
        return this;
    }

    angleTo(q) {
        return 2 * Math.acos(Math.abs(this.dot(q)));
    }

    rotateTowards(q, step) {
        const angle = this.angleTo(q);
        if (angle === 0) return this;
        const t = Math.min(step / angle, 1);
        this.slerp(q, t);
        return this;
    }

    slerp(q, t) {
        if (t === 0) return this;
        if (t === 1) return this.copy(q);

        const x = this.x;
        const y = this.y;
        const z = this.z;
        const w = this.w;

        let cosHalfTheta = w * q.w + x * q.x + y * q.y + z * q.z;
        if (cosHalfTheta < 0) {
            this.w = -q.w;
            this.x = -q.x;
            this.y = -q.y;
            this.z = -q.z;
            cosHalfTheta = -cosHalfTheta;
        } else {
            this.copy(q);
        }

        if (cosHalfTheta >= 1.0) {
            return this;
        }

        const sinHalfTheta = Math.sqrt(1.0 - cosHalfTheta * cosHalfTheta);
        if (Math.abs(sinHalfTheta) < 0.001) {
            this.w = 0.5 * (w + this.w);
            this.x = 0.5 * (x + this.x);
            this.y = 0.5 * (y + this.y);
            this.z = 0.5 * (z + this.z);
            return this;
        }

        const halfTheta = Math.acos(cosHalfTheta);
        const ratioA = Math.sin((1 - t) * halfTheta) / sinHalfTheta;
        const ratioB = Math.sin(t * halfTheta) / sinHalfTheta;

        this.w = w * ratioA + this.w * ratioB;
        this.x = x * ratioA + this.x * ratioB;
        this.y = y * ratioA + this.y * ratioB;
        this.z = z * ratioA + this.z * ratioB;

        return this;
    }

    multiply(q) {
        return this.multiplyQuaternions(this, q);
    }

    multiplyQuaternions(a, b) {
        const qax = a.x;
        const qay = a.y;
        const qaz = a.z;
        const qaw = a.w;
        const qbx = b.x;
        const qby = b.y;
        const qbz = b.z;
        const qbw = b.w;

        this.x = qax * qbw + qaw * qbx + qay * qbz - qaz * qby;
        this.y = qay * qbw + qaw * qby + qaz * qbx - qax * qbz;
        this.z = qaz * qbw + qaw * qbz + qax * qby - qay * qbx;
        this.w = qaw * qbw - qax * qbx - qay * qby - qaz * qbz;

        return this;
    }

    multiplyVector3(v) {
        const x = v.x;
        const y = v.y;
        const z = v.z;

        const qx = this.x;
        const qy = this.y;
        const qz = this.z;
        const qw = this.w;

        const ix = qw * x + qy * z - qz * y;
        const iy = qw * y + qz * x - qx * z;
        const iz = qw * z + qx * y - qy * x;
        const iw = -qx * x - qy * y - qz * z;

        return new Vector3(
            ix * qw + iw * -qx + iy * -qz - iz * -qy,
            iy * qw + iw * -qy + iz * -qx - ix * -qz,
            iz * qw + iw * -qz + ix * -qy - iy * -qx
        );
    }

    conjugate() {
        return new Quaternion(-this.x, -this.y, -this.z, this.w);
    }

    dot(v) {
        return this.x * v.x + this.y * v.y + this.z * v.z + this.w * v.w;
    }

    normalize() {
        let len = this.length();
        if (len === 0) {
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.w = 1;
        } else {
            len = 1 / len;
            this.x *= len;
            this.y *= len;
            this.z *= len;
            this.w *= len;
        }
        return this;
    }

    length() {
        return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w);
    }

    lengthSq() {
        return this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w;
    }

    inverse() {
        const len = this.lengthSq();
        if (len === 0) {
            return this.clone();
        }
        const invLen = 1 / len;
        return new Quaternion(
            -this.x * invLen,
            -this.y * invLen,
            -this.z * invLen,
            this.w * invLen
        );
    }

    equals(v, epsilon = 0.0001) {
        return (
            Math.abs(this.x - v.x) < epsilon &&
            Math.abs(this.y - v.y) < epsilon &&
            Math.abs(this.z - v.z) < epsilon &&
            Math.abs(this.w - v.w) < epsilon
        );
    }

    static identity() {
        return new Quaternion(0, 0, 0, 1);
    }

    static fromAxisAngle(axis, angle) {
        const halfAngle = angle / 2;
        const s = Math.sin(halfAngle);
        return new Quaternion(axis.x * s, axis.y * s, axis.z * s, Math.cos(halfAngle));
    }
}

export class Matrix4 {
    constructor() {
        this.elements = new Float32Array(16);
        this.identity();
    }

    clone() {
        return new Matrix4().copy(this);
    }

    copy(m) {
        for (let i = 0; i < 16; i++) {
            this.elements[i] = m.elements[i];
        }
        return this;
    }

    identity() {
        const e = this.elements;
        e[0] = 1; e[1] = 0; e[2] = 0; e[3] = 0;
        e[4] = 0; e[5] = 1; e[6] = 0; e[7] = 0;
        e[8] = 0; e[9] = 0; e[10] = 1; e[11] = 0;
        e[12] = 0; e[13] = 0; e[14] = 0; e[15] = 1;
        return this;
    }

    makeRotationFromQuaternion(q) {
        const e = this.elements;
        const x = q.x, y = q.y, z = q.z, w = q.w;
        const x2 = x + x, y2 = y + y, z2 = z + z;
        const xx = x * x2, xy = x * y2, xz = x * z2;
        const yy = y * y2, yz = y * z2, zz = z * z2;
        const wx = w * x2, wy = w * y2, wz = w * z2;

        e[0] = 1 - (yy + zz);
        e[4] = xy - wz;
        e[8] = xz + wy;
        e[12] = 0;

        e[1] = xy + wz;
        e[5] = 1 - (xx + zz);
        e[9] = yz - wx;
        e[13] = 0;

        e[2] = xz - wy;
        e[6] = yz + wx;
        e[10] = 1 - (xx + yy);
        e[14] = 0;

        e[3] = 0;
        e[7] = 0;
        e[11] = 0;
        e[15] = 1;

        return this;
    }

    makeTranslation(x, y, z) {
        this.identity();
        this.elements[12] = x;
        this.elements[13] = y;
        this.elements[14] = z;
        return this;
    }

    makeScale(x, y, z) {
        this.identity();
        this.elements[0] = x;
        this.elements[5] = y;
        this.elements[10] = z;
        return this;
    }

    multiply(m) {
        return this.multiplyMatrices(this, m);
    }

    multiplyMatrices(a, b) {
        const ae = a.elements;
        const be = b.elements;
        const e = this.elements;

        let a11 = ae[0], a12 = ae[4], a13 = ae[8], a14 = ae[12];
        let a21 = ae[1], a22 = ae[5], a23 = ae[9], a24 = ae[13];
        let a31 = ae[2], a32 = ae[6], a33 = ae[10], a34 = ae[14];
        let a41 = ae[3], a42 = ae[7], a43 = ae[11], a44 = ae[15];

        let b11 = be[0], b12 = be[4], b13 = be[8], b14 = be[12];
        let b21 = be[1], b22 = be[5], b23 = be[9], b24 = be[13];
        let b31 = be[2], b32 = be[6], b33 = be[10], b34 = be[14];
        let b41 = be[3], b42 = be[7], b43 = be[11], b44 = be[15];

        e[0] = a11 * b11 + a12 * b21 + a13 * b31 + a14 * b41;
        e[4] = a11 * b12 + a12 * b22 + a13 * b32 + a14 * b42;
        e[8] = a11 * b13 + a12 * b23 + a13 * b33 + a14 * b43;
        e[12] = a11 * b14 + a12 * b24 + a13 * b34 + a14 * b44;

        e[1] = a21 * b11 + a22 * b21 + a23 * b31 + a24 * b41;
        e[5] = a21 * b12 + a22 * b22 + a23 * b32 + a24 * b42;
        e[9] = a21 * b13 + a22 * b23 + a23 * b33 + a24 * b43;
        e[13] = a21 * b14 + a22 * b24 + a23 * b34 + a24 * b44;

        e[2] = a31 * b11 + a32 * b21 + a33 * b31 + a34 * b41;
        e[6] = a31 * b12 + a32 * b22 + a33 * b32 + a34 * b42;
        e[10] = a31 * b13 + a32 * b23 + a33 * b33 + a34 * b43;
        e[14] = a31 * b14 + a32 * b24 + a33 * b34 + a34 * b44;

        e[3] = a41 * b11 + a42 * b21 + a43 * b31 + a44 * b41;
        e[7] = a41 * b12 + a42 * b22 + a43 * b32 + a44 * b42;
        e[11] = a41 * b13 + a42 * b23 + a43 * b33 + a44 * b43;
        e[15] = a41 * b14 + a42 * b24 + a43 * b34 + a44 * b44;

        return this;
    }

    multiplyScalar(s) {
        const e = this.elements;
        e[0] *= s; e[4] *= s; e[8] *= s; e[12] *= s;
        e[1] *= s; e[5] *= s; e[9] *= s; e[13] *= s;
        e[2] *= s; e[6] *= s; e[10] *= s; e[14] *= s;
        e[3] *= s; e[7] *= s; e[11] *= s; e[15] *= s;
        return this;
    }

    applyToVector3Array(array, offset = 0, length = array.length) {
        for (let i = 0, j = offset; i < length; i += 3, j += 3) {
            const x = array[j];
            const y = array[j + 1];
            const z = array[j + 2];
            const e = this.elements;
            array[j] = e[0] * x + e[4] * y + e[8] * z + e[12];
            array[j + 1] = e[1] * x + e[5] * y + e[9] * z + e[13];
            array[j + 2] = e[2] * x + e[6] * y + e[10] * z + e[14];
        }
        return array;
    }

    getPosition() {
        return new Vector3(this.elements[12], this.elements[13], this.elements[14]);
    }

    setPosition(v) {
        this.elements[12] = v.x;
        this.elements[13] = v.y;
        this.elements[14] = v.z;
        return this;
    }

    getRotation() {
        const q = new Quaternion();
        q.setFromRotationMatrix(this);
        return q;
    }

    getScale() {
        const e = this.elements;
        return new Vector3(
            Math.sqrt(e[0] * e[0] + e[1] * e[1] + e[2] * e[2]),
            Math.sqrt(e[4] * e[4] + e[5] * e[5] + e[6] * e[6]),
            Math.sqrt(e[8] * e[8] + e[9] * e[9] + e[10] * e[10])
        );
    }

    determinant() {
        const e = this.elements;
        const a11 = e[0], a12 = e[4], a13 = e[8], a14 = e[12];
        const a21 = e[1], a22 = e[5], a23 = e[9], a24 = e[13];
        const a31 = e[2], a32 = e[6], a33 = e[10], a34 = e[14];
        const a41 = e[3], a42 = e[7], a43 = e[11], a44 = e[15];

        const b01 = a11 * a22 - a12 * a21;
        const b02 = a11 * a23 - a13 * a21;
        const b03 = a11 * a24 - a14 * a21;
        const b04 = a12 * a23 - a13 * a22;
        const b05 = a12 * a24 - a14 * a22;
        const b06 = a13 * a24 - a14 * a23;
        const b07 = a31 * a42 - a32 * a41;
        const b08 = a31 * a43 - a33 * a41;
        const b09 = a31 * a44 - a34 * a41;
        const b10 = a32 * a43 - a33 * a42;
        const b11 = a32 * a44 - a34 * a42;
        const b12 = a33 * a44 - a34 * a43;

        return b06 * b10 - b05 * b11 + b04 * b12 + b03 * b07 - b02 * b08 + b01 * b09;
    }

    transpose() {
        const e = this.elements;
        let tmp;

        tmp = e[1]; e[1] = e[4]; e[4] = tmp;
        tmp = e[2]; e[2] = e[8]; e[8] = tmp;
        tmp = e[6]; e[6] = e[9]; e[9] = tmp;

        tmp = e[3]; e[3] = e[12]; e[12] = tmp;
        tmp = e[7]; e[7] = e[13]; e[13] = tmp;
        tmp = e[11]; e[11] = e[14]; e[14] = tmp;

        return this;
    }

    inverse() {
        const e = this.elements;
        const a11 = e[0], a12 = e[4], a13 = e[8], a14 = e[12];
        const a21 = e[1], a22 = e[5], a23 = e[9], a24 = e[13];
        const a31 = e[2], a32 = e[6], a33 = e[10], a34 = e[14];
        const a41 = e[3], a42 = e[7], a43 = e[11], a44 = e[15];

        const b01 = a11 * a22 - a12 * a21;
        const b02 = a11 * a23 - a13 * a21;
        const b03 = a11 * a24 - a14 * a21;
        const b04 = a12 * a23 - a13 * a22;
        const b05 = a12 * a24 - a14 * a22;
        const b06 = a13 * a24 - a14 * a23;
        const b07 = a31 * a42 - a32 * a41;
        const b08 = a31 * a43 - a33 * a41;
        const b09 = a31 * a44 - a34 * a41;
        const b10 = a32 * a43 - a33 * a42;
        const b11 = a32 * a44 - a34 * a42;
        const b12 = a33 * a44 - a34 * a43;

        let det = b06 * b10 - b05 * b11 + b04 * b12 + b03 * b07 - b02 * b08 + b01 * b09;
        if (!det) return null;
        det = 1 / det;

        const result = new Matrix4();
        const r = result.elements;

        r[0] = (a22 * b12 - a23 * b11 + a24 * b10) * det;
        r[4] = (a13 * b11 - a12 * b12 - a14 * b10) * det;
        r[8] = (a42 * b06 - a43 * b05 + a44 * b04) * det;
        r[12] = (a33 * b05 - a32 * b06 - a34 * b04) * det;
        r[1] = (a23 * b09 - a21 * b12 - a24 * b08) * det;
        r[5] = (a11 * b12 - a13 * b09 + a14 * b08) * det;
        r[9] = (a43 * b03 - a41 * b06 - a44 * b02) * det;
        r[13] = (a31 * b06 - a33 * b03 + a34 * b02) * det;
        r[2] = (a21 * b11 - a22 * b09 + a24 * b07) * det;
        r[6] = (a12 * b09 - a11 * b11 - a14 * b07) * det;
        r[10] = (a41 * b05 - a42 * b03 + a44 * b01) * det;
        r[14] = (a32 * b03 - a31 * b05 - a34 * b01) * det;
        r[3] = (a22 * b08 - a21 * b10 - a23 * b07) * det;
        r[7] = (a11 * b10 - a12 * b08 + a13 * b07) * det;
        r[11] = (a42 * b02 - a41 * b04 - a43 * b01) * det;
        r[15] = (a31 * b04 - a32 * b02 + a33 * b01) * det;

        this.copy(result);
        return this;
    }

    compose(position, quaternion, scale) {
        this.makeRotationFromQuaternion(quaternion);
        const e = this.elements;
        e[0] *= scale.x; e[4] *= scale.y; e[8] *= scale.z;
        e[1] *= scale.x; e[5] *= scale.y; e[9] *= scale.z;
        e[2] *= scale.x; e[6] *= scale.y; e[10] *= scale.z;
        e[12] = position.x; e[13] = position.y; e[14] = position.z;
        return this;
    }

    decompose(position, quaternion, scale) {
        const e = this.elements;
        let sx = Math.sqrt(e[0] * e[0] + e[1] * e[1] + e[2] * e[2]);
        let sy = Math.sqrt(e[4] * e[4] + e[5] * e[5] + e[6] * e[6]);
        let sz = Math.sqrt(e[8] * e[8] + e[9] * e[9] + e[10] * e[10]);

        position.set(e[12], e[13], e[14]);

        const det = this.determinant();
        if (det < 0) sx = -sx;

        scale.set(sx, sy, sz);

        const invSX = 1 / sx;
        const invSY = 1 / sy;
        const invSZ = 1 / sz;

        const m = this.clone();
        const me = m.elements;
        me[0] *= invSX; me[4] *= invSY; me[8] *= invSZ;
        me[1] *= invSX; me[5] *= invSY; me[9] *= invSZ;
        me[2] *= invSX; me[6] *= invSY; me[10] *= invSZ;

        quaternion.setFromRotationMatrix(m);
        return this;
    }

    lookAt(eye, target, up) {
        const z = eye.sub(target).normalize();
        const x = up.cross(z).normalize();
        const y = z.cross(x);

        const e = this.elements;
        e[0] = x.x; e[4] = y.x; e[8] = z.x; e[12] = eye.x;
        e[1] = x.y; e[5] = y.y; e[9] = z.y; e[13] = eye.y;
        e[2] = x.z; e[6] = y.z; e[10] = z.z; e[14] = eye.z;
        e[3] = 0; e[7] = 0; e[11] = 0; e[15] = 1;

        return this;
    }

    extractBasis(xAxis, yAxis, zAxis) {
        const e = this.elements;
        xAxis.set(e[0], e[1], e[2]);
        yAxis.set(e[4], e[5], e[6]);
        zAxis.set(e[8], e[9], e[10]);
        return this;
    }
}

export class AABB {
    constructor(min = new Vector3(), max = new Vector3()) {
        this.min = min.clone();
        this.max = max.clone();
    }

    clone() {
        return new AABB(this.min.clone(), this.max.clone());
    }

    copy(aabb) {
        this.min.copy(aabb.min);
        this.max.copy(aabb.max);
        return this;
    }

    set(min, max) {
        this.min.copy(min);
        this.max.copy(max);
        return this;
    }

    setFromCenterAndSize(center, size) {
        const halfSize = size.multiply(0.5);
        this.min = center.sub(halfSize);
        this.max = center.add(halfSize);
        return this;
    }

    setFromPoints(points) {
        if (points.length === 0) {
            this.min.set(0, 0, 0);
            this.max.set(0, 0, 0);
            return this;
        }
        this.min.copy(points[0]);
        this.max.copy(points[0]);
        for (let i = 1; i < points.length; i++) {
            this.expandByPoint(points[i]);
        }
        return this;
    }

    expandByPoint(point) {
        this.min.x = Math.min(this.min.x, point.x);
        this.min.y = Math.min(this.min.y, point.y);
        this.min.z = Math.min(this.min.z, point.z);
        this.max.x = Math.max(this.max.x, point.x);
        this.max.y = Math.max(this.max.y, point.y);
        this.max.z = Math.max(this.max.z, point.z);
        return this;
    }

    expandByScalar(scalar) {
        this.min.sub(new Vector3(scalar, scalar, scalar));
        this.max.add(new Vector3(scalar, scalar, scalar));
        return this;
    }

    expandByVector(vector) {
        this.min.sub(vector);
        this.max.add(vector);
        return this;
    }

    containsPoint(point) {
        return (
            point.x >= this.min.x &&
            point.x <= this.max.x &&
            point.y >= this.min.y &&
            point.y <= this.max.y &&
            point.z >= this.min.z &&
            point.z <= this.max.z
        );
    }

    intersectsAABB(aabb) {
        return (
            this.min.x <= aabb.max.x &&
            this.max.x >= aabb.min.x &&
            this.min.y <= aabb.max.y &&
            this.max.y >= aabb.min.y &&
            this.min.z <= aabb.max.z &&
            this.max.z >= aabb.min.z
        );
    }

    intersectsSphere(sphere) {
        const closestPoint = new Vector3();
        closestPoint.x = Math.max(this.min.x, Math.min(sphere.center.x, this.max.x));
        closestPoint.y = Math.max(this.min.y, Math.min(sphere.center.y, this.max.y));
        closestPoint.z = Math.max(this.min.z, Math.min(sphere.center.z, this.max.z));
        return closestPoint.distanceToSquared(sphere.center) <= sphere.radius * sphere.radius;
    }

    intersectsRay(ray, target) {
        const invDirX = ray.direction.x !== 0 ? 1 / ray.direction.x : Infinity;
        const invDirY = ray.direction.y !== 0 ? 1 / ray.direction.y : Infinity;
        const invDirZ = ray.direction.z !== 0 ? 1 / ray.direction.z : Infinity;

        const t1 = (this.min.x - ray.origin.x) * invDirX;
        const t2 = (this.max.x - ray.origin.x) * invDirX;
        const t3 = (this.min.y - ray.origin.y) * invDirY;
        const t4 = (this.max.y - ray.origin.y) * invDirY;
        const t5 = (this.min.z - ray.origin.z) * invDirZ;
        const t6 = (this.max.z - ray.origin.z) * invDirZ;

        const tmin = Math.max(Math.max(Math.min(t1, t2), Math.min(t3, t4)), Math.min(t5, t6));
        const tmax = Math.min(Math.min(Math.max(t1, t2), Math.max(t3, t4)), Math.max(t5, t6));

        if (tmax < 0 || tmin > tmax) {
            return false;
        }

        if (tmin < 0) {
            if (target) {
                target.copy(ray.origin);
            }
            return true;
        }

        if (target) {
            target.copy(ray.origin.add(ray.direction.multiply(tmin)));
        }
        return true;
    }

    distanceToPoint(point) {
        const dx = Math.max(this.min.x - point.x, 0, point.x - this.max.x);
        const dy = Math.max(this.min.y - point.y, 0, point.y - this.max.y);
        const dz = Math.max(this.min.z - point.z, 0, point.z - this.max.z);
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    getCenter() {
        return this.min.add(this.max).multiply(0.5);
    }

    getSize() {
        return this.max.sub(this.min);
    }

    getBoundingSphere(target) {
        if (!target) {
            target = new Sphere();
        }
        target.center.copy(this.getCenter());
        target.radius = this.getSize().length() * 0.5;
        return target;
    }

    clampPoint(point, target) {
        if (!target) {
            target = new Vector3();
        }
        return target.copy(point).clamp(this.min, this.max);
    }

    union(aabb) {
        this.min.x = Math.min(this.min.x, aabb.min.x);
        this.min.y = Math.min(this.min.y, aabb.min.y);
        this.min.z = Math.min(this.min.z, aabb.min.z);
        this.max.x = Math.max(this.max.x, aabb.max.x);
        this.max.y = Math.max(this.max.y, aabb.max.y);
        this.max.z = Math.max(this.max.z, aabb.max.z);
        return this;
    }

    intersect(aabb) {
        this.min.x = Math.max(this.min.x, aabb.min.x);
        this.min.y = Math.max(this.min.y, aabb.min.y);
        this.min.z = Math.max(this.min.z, aabb.min.z);
        this.max.x = Math.min(this.max.x, aabb.max.x);
        this.max.y = Math.min(this.max.y, aabb.max.y);
        this.max.z = Math.min(this.max.z, aabb.max.z);
        return this;
    }

    isEmpty() {
        return this.max.x < this.min.x || this.max.y < this.min.y || this.max.z < this.min.z;
    }

    makeEmpty() {
        this.min.set(Infinity, Infinity, Infinity);
        this.max.set(-Infinity, -Infinity, -Infinity);
        return this;
    }

    translate(offset) {
        this.min.add(offset);
        this.max.add(offset);
        return this;
    }

    equals(aabb, epsilon = 0.0001) {
        return this.min.equals(aabb.min, epsilon) && this.max.equals(aabb.max, epsilon);
    }
}

export class Sphere {
    constructor(center = new Vector3(), radius = 0) {
        this.center = center.clone();
        this.radius = radius;
    }

    clone() {
        return new Sphere(this.center.clone(), this.radius);
    }

    copy(sphere) {
        this.center.copy(sphere.center);
        this.radius = sphere.radius;
        return this;
    }

    set(center, radius) {
        this.center.copy(center);
        this.radius = radius;
        return this;
    }

    setFromPoints(points, optionalCenter) {
        if (optionalCenter) {
            this.center.copy(optionalCenter);
        } else {
            const aabb = new AABB();
            aabb.setFromPoints(points);
            this.center.copy(aabb.getCenter());
        }

        let maxRadiusSq = 0;
        for (let i = 0; i < points.length; i++) {
            maxRadiusSq = Math.max(maxRadiusSq, this.center.distanceToSquared(points[i]));
        }
        this.radius = Math.sqrt(maxRadiusSq);

        return this;
    }

    containsPoint(point) {
        return this.center.distanceToSquared(point) <= this.radius * this.radius;
    }

    intersectsSphere(sphere) {
        const combinedRadius = this.radius + sphere.radius;
        return this.center.distanceToSquared(sphere.center) <= combinedRadius * combinedRadius;
    }

    intersectsAABB(aabb) {
        return aabb.intersectsSphere(this);
    }

    intersectsRay(ray, target) {
        const oc = ray.origin.sub(this.center);
        const b = oc.dot(ray.direction);
        const c = oc.dot(oc) - this.radius * this.radius;
        const discriminant = b * b - c;

        if (discriminant < 0) {
            return false;
        }

        const t = -b - Math.sqrt(discriminant);
        if (t < 0) {
            return false;
        }

        if (target) {
            target.copy(ray.origin.add(ray.direction.multiply(t)));
        }
        return true;
    }

    clampPoint(point, target) {
        const delta = point.sub(this.center);
        const len = delta.length();
        if (len <= this.radius) {
            return target ? target.copy(point) : point.clone();
        }
        return target ? target.copy(this.center.add(delta.normalize().multiply(this.radius))) : this.center.add(delta.normalize().multiply(this.radius));
    }

    distanceToPoint(point) {
        return Math.max(0, this.center.distanceTo(point) - this.radius);
    }

    translate(offset) {
        this.center.add(offset);
        return this;
    }

    equals(sphere, epsilon = 0.0001) {
        return this.center.equals(sphere.center, epsilon) && Math.abs(this.radius - sphere.radius) < epsilon;
    }
}

export class Ray {
    constructor(origin = new Vector3(), direction = new Vector3(0, 0, -1)) {
        this.origin = origin.clone();
        this.direction = direction.normalize();
    }

    clone() {
        return new Ray(this.origin.clone(), this.direction.clone());
    }

    copy(ray) {
        this.origin.copy(ray.origin);
        this.direction.copy(ray.direction);
        return this;
    }

    set(origin, direction) {
        this.origin.copy(origin);
        this.direction.copy(direction);
        return this;
    }

    at(t, target) {
        if (!target) {
            target = new Vector3();
        }
        return target.copy(this.direction).multiply(t).add(this.origin);
    }

    recast(t) {
        const result = new Vector3();
        this.at(t, result);
        return result;
    }

    closestPointToPoint(point, target) {
        if (!target) {
            target = new Vector3();
        }
        return target.copy(point).sub(this.origin).projectOnVector(this.direction).add(this.origin);
    }

    distanceToPoint(point) {
        const offset = this.closestPointToPoint(point);
        return offset.distanceTo(point);
    }

    distanceSquaredToPoint(point) {
        const offset = this.closestPointToPoint(point);
        return offset.distanceToSquared(point);
    }

    intersectsSphere(sphere) {
        const v = this.origin.sub(sphere.center);
        const b = v.dot(this.direction);
        const c = v.dot(v) - sphere.radius * sphere.radius;
        return b * b >= c;
    }

    intersectsAABB(aabb) {
        return aabb.intersectsRay(this);
    }

    intersectsPlane(plane, target) {
        const denominator = this.direction.dot(plane.normal);
        if (Math.abs(denominator) < 0.0001) {
            return false;
        }
        const t = -(this.origin.dot(plane.normal) + plane.constant) / denominator;
        if (t < 0) {
            return false;
        }
        if (target) {
            target.copy(this.at(t));
        }
        return true;
    }

    intersectsBox(box, target) {
        let tmin, tmax, tymin, tymax, tzmin, tzmax;

        const invDirX = 1 / this.direction.x;
        const invDirY = 1 / this.direction.y;
        const invDirZ = 1 / this.direction.z;

        if (invDirX >= 0) {
            tmin = (box.min.x - this.origin.x) * invDirX;
            tmax = (box.max.x - this.origin.x) * invDirX;
        } else {
            tmin = (box.max.x - this.origin.x) * invDirX;
            tmax = (box.min.x - this.origin.x) * invDirX;
        }

        if (invDirY >= 0) {
            tymin = (box.min.y - this.origin.y) * invDirY;
            tymax = (box.max.y - this.origin.y) * invDirY;
        } else {
            tymin = (box.max.y - this.origin.y) * invDirY;
            tymax = (box.min.y - this.origin.y) * invDirY;
        }

        if (tmin > tymax || tymin > tmax) {
            return false;
        }

        if (tymin > tmin) tmin = tymin;
        if (tymax < tmax) tmax = tymax;

        if (invDirZ >= 0) {
            tzmin = (box.min.z - this.origin.z) * invDirZ;
            tzmax = (box.max.z - this.origin.z) * invDirZ;
        } else {
            tzmin = (box.max.z - this.origin.z) * invDirZ;
            tzmax = (box.min.z - this.origin.z) * invDirZ;
        }

        if (tmin > tzmax || tzmin > tmax) {
            return false;
        }

        if (tzmin > tmin) tmin = tzmin;
        if (tzmax < tmax) tmax = tzmax;

        if (tmax < 0) {
            return false;
        }

        if (tmin < 0) {
            tmin = tmax;
        }

        if (target) {
            target.copy(this.at(tmin));
        }

        return true;
    }
}

export class Plane {
    constructor(normal = new Vector3(0, 1, 0), constant = 0) {
        this.normal = normal.clone();
        this.constant = constant;
    }

    clone() {
        return new Plane(this.normal.clone(), this.constant);
    }

    copy(plane) {
        this.normal.copy(plane.normal);
        this.constant = plane.constant;
        return this;
    }

    set(normal, constant) {
        this.normal.copy(normal);
        this.constant = constant;
        return this;
    }

    setFromComponents(x, y, z, w) {
        this.normal.set(x, y, z);
        this.constant = w;
        return this;
    }

    setFromNormalAndCoplanarPoint(normal, point) {
        this.normal.copy(normal);
        this.constant = -point.dot(normal);
        return this;
    }

    setFromTriangles(triangle) {
        const edge1 = triangle[1].sub(triangle[0]);
        const edge2 = triangle[2].sub(triangle[0]);
        this.normal = edge1.cross(edge2).normalize();
        this.constant = -triangle[0].dot(this.normal);
        return this;
    }

    normalize() {
        const len = this.normal.length();
        if (len === 0) {
            return this;
        }
        this.normal.divide(len);
        this.constant /= len;
        return this;
    }

    negate() {
        this.normal.negate();
        this.constant = -this.constant;
        return this;
    }

    distanceToPoint(point) {
        return this.normal.dot(point) + this.constant;
    }

    distanceToSphere(sphere) {
        return this.distanceToPoint(sphere.center) - Math.abs(sphere.radius);
    }

    projectPoint(point, target) {
        if (!target) {
            target = new Vector3();
        }
        return target.copy(this.normal).multiply(-this.distanceToPoint(point)).add(point);
    }

    intersectsPoint(point) {
        return Math.abs(this.distanceToPoint(point)) < 0.0001;
    }

    intersectsLine(line, target) {
        const denominator = this.normal.dot(line.direction);
        if (Math.abs(denominator) < 0.0001) {
            return false;
        }
        const t = -(this.normal.dot(line.origin) + this.constant) / denominator;
        if (t < 0 || t > 1) {
            return false;
        }
        if (target) {
            target.copy(line.at(t));
        }
        return true;
    }

    intersectsBox(box) {
        const corners = [
            new Vector3(box.min.x, box.min.y, box.min.z),
            new Vector3(box.max.x, box.min.y, box.min.z),
            new Vector3(box.min.x, box.max.y, box.min.z),
            new Vector3(box.max.x, box.max.y, box.min.z),
            new Vector3(box.min.x, box.min.y, box.max.z),
            new Vector3(box.max.x, box.min.y, box.max.z),
            new Vector3(box.min.x, box.max.y, box.max.z),
            new Vector3(box.max.x, box.max.y, box.max.z)
        ];

        let hasPositive = false;
        let hasNegative = false;

        for (const corner of corners) {
            const dist = this.distanceToPoint(corner);
            if (dist > 0) {
                hasPositive = true;
            } else {
                hasNegative = true;
            }
            if (hasPositive && hasNegative) {
                return true;
            }
        }

        return false;
    }

    intersectsSphere(sphere) {
        return Math.abs(this.distanceToPoint(sphere.center)) <= Math.abs(sphere.radius);
    }

    coplanarPoint(target) {
        if (!target) {
            target = new Vector3();
        }
        return target.copy(this.normal).multiply(-this.constant);
    }

    equals(plane, epsilon = 0.0001) {
        return this.normal.equals(plane.normal, epsilon) && Math.abs(this.constant - plane.constant) < epsilon;
    }

    applyMatrix4(matrix) {
        const referencePoint = this.coplanarPoint();
        referencePoint.applyMatrix4(matrix);
        this.normal.transformDirection(matrix);
        this.normal.normalize();
        this.constant = -referencePoint.dot(this.normal);
        return this;
    }
}

export class Capsule {
    constructor(start = new Vector3(0, 0, 0), end = new Vector3(0, 1, 0), radius = 1) {
        this.start = start.clone();
        this.end = end.clone();
        this.radius = radius;
    }

    clone() {
        return new Capsule(this.start.clone(), this.end.clone(), this.radius);
    }

    copy(capsule) {
        this.start.copy(capsule.start);
        this.end.copy(capsule.end);
        this.radius = capsule.radius;
        return this;
    }

    set(start, end, radius) {
        this.start.copy(start);
        this.end.copy(end);
        this.radius = radius;
        return this;
    }

    getCenter(target) {
        if (!target) {
            target = new Vector3();
        }
        return target.copy(this.end).add(this.start).multiply(0.5);
    }

    getLength() {
        return this.start.distanceTo(this.end);
    }

    getAxis() {
        return this.end.sub(this.start).normalize();
    }

    translate(offset) {
        this.start.add(offset);
        this.end.add(offset);
        return this;
    }

    containsPoint(point) {
        const d1 = this.start.sub(point);
        const d2 = this.end.sub(point);
        const dot1 = d1.dot(this.getAxis());
        const dot2 = d2.dot(this.getAxis());
        const lengthSq = this.getLength() * this.getLength();

        if (dot1 <= 0) {
            return d1.length() <= this.radius;
        } else if (dot2 >= 0) {
            return d2.length() <= this.radius;
        } else {
            const axis = this.getAxis();
            const proj = axis.multiply(dot1);
            const closest = this.start.add(proj);
            return closest.distanceTo(point) <= this.radius;
        }
    }

    intersectsSphere(sphere) {
        const closest = new Vector3();
        const axis = this.getAxis();
        const d1 = this.start.sub(sphere.center);
        const d2 = this.end.sub(sphere.center);
        const dot1 = d1.dot(axis);
        const dot2 = d2.dot(axis);

        if (dot1 <= 0) {
            closest.copy(this.start);
        } else if (dot2 >= 0) {
            closest.copy(this.end);
        } else {
            const proj = axis.multiply(dot1);
            closest.copy(this.start).add(proj);
        }

        return closest.distanceToSquared(sphere.center) <= this.radius * this.radius;
    }

    intersectsAABB(aabb) {
        const closest = new Vector3();
        closest.x = Math.max(aabb.min.x, Math.min(this.getCenter().x, aabb.max.x));
        closest.y = Math.max(aabb.min.y, Math.min(this.getCenter().y, aabb.max.y));
        closest.z = Math.max(aabb.min.z, Math.min(this.getCenter().z, aabb.max.z));

        const axis = this.getAxis();
        const d1 = this.start.sub(closest);
        const d2 = this.end.sub(closest);
        const dot1 = d1.dot(axis);
        const dot2 = d2.dot(axis);

        let validClosest;
        if (dot1 <= 0) {
            validClosest = this.start.clone();
        } else if (dot2 >= 0) {
            validClosest = this.end.clone();
        } else {
            const proj = axis.multiply(dot1);
            validClosest = this.start.add(proj);
        }

        return validClosest.distanceToSquared(closest) <= this.radius * this.radius;
    }

    intersectsRay(ray, target) {
        const axis = this.getAxis();
        const oc = ray.origin.sub(this.start);
        const ba = this.end.sub(this.start);

        const baba = ba.dot(ba);
        const bard = ba.dot(ray.direction);
        const baoc = ba.dot(oc);

        const k2 = baba - bard * bard;
        const k1 = baba * oc.dot(ray.direction) - baoc * bard;
        const k0 = baba * oc.dot(oc) - baoc * baoc - this.radius * this.radius * baba;

        let h = k1 * k1 - k2 * k0;
        if (h < 0) return false;
        h = Math.sqrt(h);

        let t = (-k1 - h) / k2;
        if (t < 0 || t > 1) return false;

        const y = baoc + t * bard;
        if (y > 0 && y < baba) {
            if (target) {
                target.copy(ray.at(t));
            }
            return true;
        }

        const ocNorm = oc.length();
        const b = ray.direction.dot(oc);
        const c = ocNorm * ocNorm - this.radius * this.radius;
        const discriminant = b * b - c;
        if (discriminant < 0) return false;

        t = -b - Math.sqrt(discriminant);
        if (t < 0) return false;

        const point = ray.at(t);
        if (point.distanceTo(this.start) <= this.radius || point.distanceTo(this.end) <= this.radius) {
            if (target) {
                target.copy(point);
            }
            return true;
        }

        return false;
    }
}

export class OBB {
    constructor(center = new Vector3(), halfExtents = new Vector3(1, 1, 1)) {
        this.center = center.clone();
        this.halfExtents = halfExtents.clone();
        this.axes = [
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1)
        ];
    }

    clone() {
        return new OBB(this.center.clone(), this.halfExtents.clone());
    }

    copy(obb) {
        this.center.copy(obb.center);
        this.halfExtents.copy(obb.halfExtents);
        this.axes[0].copy(obb.axes[0]);
        this.axes[1].copy(obb.axes[1]);
        this.axes[2].copy(obb.axes[2]);
        return this;
    }

    set(center, halfExtents, rotation) {
        this.center.copy(center);
        this.halfExtents.copy(halfExtents);
        if (rotation) {
            this.axes[0].setFromMatrixColumn(rotation, 0);
            this.axes[1].setFromMatrixColumn(rotation, 1);
            this.axes[2].setFromMatrixColumn(rotation, 2);
        }
        return this;
    }

    setFromMatrix4(matrix) {
        matrix.decompose(this.center, new Quaternion(), this.halfExtents);
        this.axes[0].setFromMatrixColumn(matrix, 0).normalize();
        this.axes[1].setFromMatrixColumn(matrix, 1).normalize();
        this.axes[2].setFromMatrixColumn(matrix, 2).normalize();
        return this;
    }

    containsPoint(point) {
        const diff = point.sub(this.center);
        for (let i = 0; i < 3; i++) {
            const projection = diff.dot(this.axes[i]);
            if (Math.abs(projection) > this.halfExtents.getComponent(i)) {
                return false;
            }
        }
        return true;
    }

    intersectsAABB(aabb) {
        const obbToAabb = this.getAABB();
        return obbToAabb.intersectsAABB(aabb);
    }

    intersectsSphere(sphere) {
        const localSphere = this.worldToLocal(sphere.center);
        const closest = localSphere.clamp(
            this.halfExtents.negate(),
            this.halfExtents
        );
        return closest.distanceTo(localSphere) <= sphere.radius;
    }

    intersectsRay(ray, target) {
        const t = this.intersectRay(ray);
        if (t !== null) {
            if (target) {
                target.copy(ray.at(t));
            }
            return true;
        }
        return false;
    }

    intersectRay(ray) {
        let tmin = -Infinity;
        let tmax = Infinity;

        const rayOrigin = ray.origin.sub(this.center);

        for (let i = 0; i < 3; i++) {
            const axis = this.axes[i];
            const halfExtent = this.halfExtents.getComponent(i);
            const originProjection = rayOrigin.dot(axis);
            const directionProjection = ray.direction.dot(axis);

            if (Math.abs(directionProjection) < 0.0001) {
                if (-originProjection - halfExtent > 0 || -originProjection + halfExtent < 0) {
                    return null;
                }
            } else {
                let t1 = (-originProjection - halfExtent) / directionProjection;
                let t2 = (-originProjection + halfExtent) / directionProjection;

                if (t1 > t2) {
                    const temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tmin = Math.max(tmin, t1);
                tmax = Math.min(tmax, t2);

                if (tmin > tmax) {
                    return null;
                }
            }
        }

        if (tmin < 0) {
            return tmax;
        }
        return tmin;
    }

    getAABB() {
        const aabb = new AABB();
        for (let i = 0; i < 8; i++) {
            const corner = new Vector3(
                (i & 1) ? this.halfExtents.x : -this.halfExtents.x,
                (i & 2) ? this.halfExtents.y : -this.halfExtents.y,
                (i & 4) ? this.halfExtents.z : -this.halfExtents.z
            );

            for (let j = 0; j < 3; j++) {
                corner.add(this.axes[j].multiply(corner.getComponent(j)));
            }

            corner.add(this.center);
            aabb.expandByPoint(corner);
        }
        return aabb;
    }

    worldToLocal(point) {
        const diff = point.sub(this.center);
        return new Vector3(
            diff.dot(this.axes[0]),
            diff.dot(this.axes[1]),
            diff.dot(this.axes[2])
        );
    }

    localToWorld(point) {
        return this.center
            .add(this.axes[0].multiply(point.x))
            .add(this.axes[1].multiply(point.y))
            .add(this.axes[2].multiply(point.z));
    }

    translate(offset) {
        this.center.add(offset);
        return this;
    }

    rotate(rotation) {
        const m = new Matrix4().makeRotationFromQuaternion(rotation);
        for (let i = 0; i < 3; i++) {
            this.axes[i].transformDirection(m);
        }
        return this;
    }

    scale(scale) {
        this.halfExtents.multiply(scale);
        return this;
    }
}

export class CollisionShape {
    static get SPHERE() { return 0; }
    static get BOX() { return 1; }
    static get CAPSULE() { return 2; }
    static get CYLINDER() { return 3; }
    static get CONE() { return 4; }
    static get MESH() { return 5; }
    static get TERRAIN() { return 6; }
}

export class PhysicsMaterial {
    constructor(config = {}) {
        this.id = PhysicsMaterial.nextId++;
        this.name = config.name || 'default';
        this.staticFriction = config.staticFriction || 0.5;
        this.dynamicFriction = config.dynamicFriction || 0.5;
        this.restitution = config.restitution || 0.3;
        this.density = config.density || 1.0;
        this.isConvex = config.isConvex || false;
    }

    getFriction() {
        return this.dynamicFriction;
    }

    combine(other) {
        const min = Math.min(this.staticFriction, other.staticFriction);
        const max = Math.max(this.staticFriction, other.staticFriction);
        return (min + max) * 0.5;
    }

    getRestitution(other) {
        return Math.max(this.restitution, other.restitution);
    }
}

PhysicsMaterial.nextId = 0;

PhysicsMaterial.STATIC = new PhysicsMaterial({ name: 'static', staticFriction: 0.8, dynamicFriction: 0.6, restitution: 0.1 });
PhysicsMaterial.DYNAMIC = new PhysicsMaterial({ name: 'dynamic', staticFriction: 0.5, dynamicFriction: 0.4, restitution: 0.2 });
PhysicsMaterial.METAL = new PhysicsMaterial({ name: 'metal', staticFriction: 0.4, dynamicFriction: 0.3, restitution: 0.4 });
PhysicsMaterial.WOOD = new PhysicsMaterial({ name: 'wood', staticFriction: 0.6, dynamicFriction: 0.5, restitution: 0.3 });
PhysicsMaterial.CONCRETE = new PhysicsMaterial({ name: 'concrete', staticFriction: 0.7, dynamicFriction: 0.6, restitution: 0.1 });
PhysicsMaterial.GLASS = new PhysicsMaterial({ name: 'glass', staticFriction: 0.2, dynamicFriction: 0.1, restitution: 0.5 });
PhysicsMaterial.FLESH = new PhysicsMaterial({ name: 'flesh', staticFriction: 0.8, dynamicFriction: 0.7, restitution: 0.1 });

export class CollisionResult {
    constructor() {
        this.point = new Vector3();
        this.normal = new Vector3();
        this.penetration = 0;
        this.distance = 0;
        this.bodyA = null;
        this.bodyB = null;
        this.shapeA = null;
        this.shapeB = null;
    }

    reset() {
        this.point.set(0, 0, 0);
        this.normal.set(0, 0, 0);
        this.penetration = 0;
        this.distance = 0;
        this.bodyA = null;
        this.bodyB = null;
        this.shapeA = null;
        this.shapeB = null;
        return this;
    }
}

export class CollisionDetector {
    static sphereVsSphere(a, b, result) {
        const diff = b.center.sub(a.center);
        const distance = diff.length();
        const combinedRadius = a.radius + b.radius;

        if (distance >= combinedRadius) {
            return false;
        }

        if (distance > 0) {
            const normal = diff.normalize();
            const contactPoint = a.center.add(normal.multiply(a.radius));
            result.normal.copy(normal);
            result.penetration = combinedRadius - distance;
        } else {
            result.normal.set(1, 0, 0);
            result.penetration = a.radius;
            result.point.copy(a.center);
        }

        result.point.copy(a.center.add(result.normal.multiply(a.radius)));
        return true;
    }

    static sphereVsAABB(sphere, aabb, result) {
        const closest = new Vector3();
        closest.x = Math.max(aabb.min.x, Math.min(sphere.center.x, aabb.max.x));
        closest.y = Math.max(aabb.min.y, Math.min(sphere.center.y, aabb.max.y));
        closest.z = Math.max(aabb.min.z, Math.min(sphere.center.z, aabb.max.z));

        const distanceSq = closest.distanceToSquared(sphere.center);
        if (distanceSq >= sphere.radius * sphere.radius) {
            return false;
        }

        const distance = Math.sqrt(distanceSq);
        if (distance > 0) {
            result.normal.copy(sphere.center.sub(closest)).normalize();
            result.penetration = sphere.radius - distance;
        } else {
            result.normal.set(0, 1, 0);
            result.penetration = sphere.radius;
        }

        result.point.copy(closest);
        return true;
    }

    static aabbVsAABB(a, b, result) {
        if (!a.intersectsAABB(b)) {
            return false;
        }

        const overlapX = Math.min(a.max.x, b.max.x) - Math.max(a.min.x, b.min.x);
        const overlapY = Math.min(a.max.y, b.max.y) - Math.max(a.min.y, b.min.y);
        const overlapZ = Math.min(a.max.z, b.max.z) - Math.max(a.min.z, b.min.z);

        if (overlapX < overlapY && overlapX < overlapZ) {
            result.penetration = overlapX;
            result.normal.set(a.center.x < b.center.x ? -1 : 1, 0, 0);
        } else if (overlapY < overlapZ) {
            result.penetration = overlapY;
            result.normal.set(0, a.center.y < b.center.y ? -1 : 1, 0);
        } else {
            result.penetration = overlapZ;
            result.normal.set(0, 0, a.center.z < b.center.z ? -1 : 1);
        }

        result.point.copy(a.getCenter().add(result.normal.multiply(a.getSize().multiply(0.5))));
        return true;
    }

    static rayVsSphere(ray, sphere, tmin, tmax) {
        const oc = ray.origin.sub(sphere.center);
        const a = ray.direction.dot(ray.direction);
        const b = 2 * oc.dot(ray.direction);
        const c = oc.dot(oc) - sphere.radius * sphere.radius;
        const discriminant = b * b - 4 * a * c;

        if (discriminant < 0) {
            return null;
        }

        const t1 = (-b - Math.sqrt(discriminant)) / (2 * a);
        const t2 = (-b + Math.sqrt(discriminant)) / (2 * a);

        if (t1 >= tmin && t1 <= tmax) {
            return t1;
        }
        if (t2 >= tmin && t2 <= tmax) {
            return t2;
        }

        return null;
    }

    static rayVsAABB(ray, aabb, tmin, tmax) {
        const invDirX = ray.direction.x !== 0 ? 1 / ray.direction.x : Infinity;
        const invDirY = ray.direction.y !== 0 ? 1 / ray.direction.y : Infinity;
        const invDirZ = ray.direction.z !== 0 ? 1 / ray.direction.z : Infinity;

        const t1 = (aabb.min.x - ray.origin.x) * invDirX;
        const t2 = (aabb.max.x - ray.origin.x) * invDirX;
        const t3 = (aabb.min.y - ray.origin.y) * invDirY;
        const t4 = (aabb.max.y - ray.origin.y) * invDirY;
        const t5 = (aabb.min.z - ray.origin.z) * invDirZ;
        const t6 = (aabb.max.z - ray.origin.z) * invDirZ;

        const tEnter = Math.max(Math.max(Math.min(t1, t2), Math.min(t3, t4)), Math.min(t5, t6));
        const tExit = Math.min(Math.min(Math.max(t1, t2), Math.max(t3, t4)), Math.max(t5, t6));

        if (tEnter > tExit || tExit < 0) {
            return null;
        }

        if (tEnter < 0) {
            return tExit;
        }

        return tEnter;
    }

    static rayVsPlane(ray, plane, tmin, tmax) {
        const denominator = ray.direction.dot(plane.normal);
        if (Math.abs(denominator) < 0.0001) {
            return null;
        }

        const t = -(ray.origin.dot(plane.normal) + plane.constant) / denominator;
        if (t >= tmin && t <= tmax) {
            return t;
        }

        return null;
    }

    static rayVsCapsule(ray, capsule, tmin, tmax) {
        const axis = capsule.getAxis();
        const oc = ray.origin.sub(capsule.start);
        const ba = capsule.end.sub(capsule.start);

        const baba = ba.dot(ba);
        const bard = ba.dot(ray.direction);
        const baoc = ba.dot(oc);

        const k2 = baba - bard * bard;
        const k1 = baba * oc.dot(ray.direction) - baoc * bard;
        const k0 = baba * oc.dot(oc) - baoc * baoc - capsule.radius * capsule.radius * baba;

        let discriminant = k1 * k1 - k2 * k0;
        if (discriminant < 0) {
            return null;
        }
        discriminant = Math.sqrt(discriminant);

        let t = (-k1 - discriminant) / k2;
        if (t < tmin || t > tmax) {
            t = (-k1 + discriminant) / k2;
            if (t < tmin || t > tmax) {
                return null;
            }
        }

        const y = baoc + t * bard;
        if (y > 0 && y < baba) {
            return t;
        }

        const t1 = -oc.dot(ba) / baba;
        const t2 = ray.direction.dot(ba);

        let closestT = null;
        let closestDist = Infinity;

        if (Math.abs(t2) > 0.0001) {
            const testT = (capsule.radius - oc.dot(axis)) / t2;
            if (testT >= tmin && testT <= tmax) {
                const point = ray.at(testT);
                const dist = point.distanceTo(capsule.start);
                if (dist < closestDist) {
                    closestDist = dist;
                    closestT = testT;
                }
            }

            const testT2 = (-capsule.radius - oc.dot(axis)) / t2;
            if (testT2 >= tmin && testT2 <= tmax) {
                const point = ray.at(testT2);
                const dist = point.distanceTo(capsule.end);
                if (dist < closestDist) {
                    closestDist = dist;
                    closestT = testT2;
                }
            }
        }

        return closestT;
    }

    static sphereVsPlane(sphere, plane, result) {
        const distance = plane.distanceToPoint(sphere.center);
        if (distance > sphere.radius) {
            return false;
        }

        result.normal.copy(plane.normal);
        result.penetration = sphere.radius - distance;
        result.point.copy(sphere.center.sub(plane.normal.multiply(sphere.radius)));
        return true;
    }

    static capsuleVsCapsule(a, b, result) {
        const axisA = a.getAxis();
        const axisB = b.getAxis();

        const d1 = a.start.sub(b.start);
        const d2 = a.end.sub(b.start);
        const d3 = a.start.sub(b.end);

        const c1 = axisA.dot(axisB);
        const c2 = axisA.dot(d1);
        const c3 = axisA.dot(d2);
        const c4 = axisB.dot(d1);
        const c5 = axisB.dot(d3);
        const c6 = d1.dot(d1);
        const c7 = d1.dot(d2);
        const c8 = d1.dot(d3);
        const c9 = d2.dot(d2);
        const c10 = d2.dot(d3);
        const c11 = d3.dot(d3);

        const k = c9 - 2 * c7 + c6 - (a.radius + b.radius) * (a.radius + b.radius);

        if (Math.abs(k) < 0.0001) {
            const t = (c2 - c4) / (2 * (c3 - c4 - c7 + c8 + c1 * (c4 - c2)));
            const s = c1 * t + c4;
            const clampedS = Math.max(0, Math.min(1, s));
            const clampedT = Math.max(0, Math.min(1, t));

            const pointA = a.start.add(axisA.multiply(clampedT));
            const pointB = b.start.add(axisB.multiply(clampedS));
            const diff = pointA.sub(pointB);
            const distance = diff.length();

            if (distance <= a.radius + b.radius) {
                result.normal.copy(diff.normalize());
                result.penetration = a.radius + b.radius - distance;
                result.point.copy(pointA.add(pointB).multiply(0.5));
                return true;
            }
            return false;
        }

        const discriminant = k * k - 4 * k * (c10 - c9 - c8 + c7 + c1 * (c4 - c2) + c3 * c3 - c6);
        if (discriminant < 0) {
            return false;
        }

        const sqrtDisc = Math.sqrt(discriminant);
        const t1 = (-k + sqrtDisc) / (2 * k);
        const t2 = (-k - sqrtDisc) / (2 * k);

        const s1 = c1 * t1 + c4;
        const s2 = c1 * t2 + c4;

        const clampedT1 = Math.max(0, Math.min(1, t1));
        const clampedS1 = Math.max(0, Math.min(1, s1));
        const clampedT2 = Math.max(0, Math.min(1, t2));
        const clampedS2 = Math.max(0, Math.min(1, s2));

        const pointA1 = a.start.add(axisA.multiply(clampedT1));
        const pointB1 = b.start.add(axisB.multiply(clampedS1));
        const diff1 = pointA1.sub(pointB1);
        const dist1 = diff1.length();

        const pointA2 = a.start.add(axisA.multiply(clampedT2));
        const pointB2 = b.start.add(axisB.multiply(clampedS2));
        const diff2 = pointA2.sub(pointB2);
        const dist2 = diff2.length();

        if (dist1 <= a.radius + b.radius && dist1 <= dist2) {
            result.normal.copy(diff1.normalize());
            result.penetration = a.radius + b.radius - dist1;
            result.point.copy(pointA1.add(pointB1).multiply(0.5));
            return true;
        }

        if (dist2 <= a.radius + b.radius) {
            result.normal.copy(diff2.normalize());
            result.penetration = a.radius + b.radius - dist2;
            result.point.copy(pointA2.add(pointB2).multiply(0.5));
            return true;
        }

        return false;
    }
}

export class CollisionFilter {
    constructor() {
        this.mask = 0xFFFFFFFF;
        this.group = 0x00000001;
    }

    setGroup(group) {
        this.group = group;
    }

    setMask(mask) {
        this.mask = mask;
    }

    canCollideWith(other) {
        return (this.group & other.mask) !== 0 && (other.group & this.mask) !== 0;
    }

    static get ALL() { return 0xFFFFFFFF; }
    static get NONE() { return 0x00000000; }
    static get STATIC() { return 0x00000001; }
    static get DYNAMIC() { return 0x00000002; }
    static get KINEMATIC() { return 0x00000004; }
    static get TRIGGER() { return 0x00000008; }
    static get CHARACTER() { return 0x00000010; }
    static get PROJECTILE() { return 0x00000020; }
    static get DEBRIS() { return 0x00000040; }
    static get RAGDOLL() { return 0x00000080; }
}

export class PhysicsWorld {
    constructor(config = {}) {
        this.gravity = config.gravity || new Vector3(0, -9.81, 0);
        this.iterations = config.iterations || 10;
        this.allowSleep = config.allowSleep !== false;
        this.bodies = [];
        this.constraints = [];
        this.collisionPairs = [];
        this.collisionResults = [];
        this.debugDraw = config.debugDraw || null;
        this.broadphase = config.broadphase || new SpatialHash(this);
        this.narrowphase = new Narrowphase();
        this.timeStep = 1 / 60;
        this.maxSubSteps = 4;
        this.fixedTimeStep = 1 / 60;
        this.accumulator = 0;
        this.isRunning = false;
    }

    addBody(body) {
        this.bodies.push(body);
        body.world = this;
        this.broadphase.addBody(body);
        return this;
    }

    removeBody(body) {
        const index = this.bodies.indexOf(body);
        if (index !== -1) {
            this.bodies.splice(index, 1);
            body.world = null;
            this.broadphase.removeBody(body);
        }
        return this;
    }

    addConstraint(constraint) {
        this.constraints.push(constraint);
        constraint.world = this;
        return this;
    }

    removeConstraint(constraint) {
        const index = this.constraints.indexOf(constraint);
        if (index !== -1) {
            this.constraints.splice(index, 1);
            constraint.world = null;
        }
        return this;
    }

    step(deltaTime) {
        this.accumulator += deltaTime;

        while (this.accumulator >= this.fixedTimeStep) {
            this.fixedUpdate(this.fixedTimeStep);
            this.accumulator -= this.fixedTimeStep;
        }

        const alpha = this.accumulator / this.fixedTimeStep;
        for (const body of this.bodies) {
            if (body.type === BodyType.DYNAMIC && body.interpolate) {
                body.interpolate(alpha);
            }
        }
    }

    fixedUpdate(dt) {
        for (const body of this.bodies) {
            if (body.type === BodyType.DYNAMIC) {
                body.integrateForce(dt);
            }
        }

        this.broadphase.update();
        this.broadphase.getCollisionPairs(this.collisionPairs);

        this.narrowphase.detectCollisions(this.collisionPairs, this.collisionResults);

        for (const result of this.collisionResults) {
            this.resolveCollision(result);
        }

        for (const body of this.bodies) {
            if (body.type === BodyType.DYNAMIC) {
                body.integrateVelocity(dt);
                body.updateBounds();
            }
        }

        for (const constraint of this.constraints) {
            constraint.solve();
        }

        if (this.allowSleep) {
            this.updateSleepState();
        }

        for (const body of this.bodies) {
            if (body.type === BodyType.DYNAMIC) {
                body.postUpdate();
            }
        }

        this.collisionPairs.length = 0;
        this.collisionResults.length = 0;
    }

    resolveCollision(result) {
        const bodyA = result.bodyA;
        const bodyB = result.bodyB;

        if (bodyA.isStatic && bodyB.isStatic) {
            return;
        }

        if (bodyA.isTrigger || bodyB.isTrigger) {
            if (bodyA.onTrigger) bodyA.onTrigger(bodyB, result);
            if (bodyB.onTrigger) bodyB.onTrigger(bodyA, result);
            return;
        }

        const relativeVelocity = bodyB.linearVelocity.sub(bodyA.linearVelocity);
        const normalVelocity = relativeVelocity.dot(result.normal);

        if (normalVelocity > 0) {
            return;
        }

        const restitution = Math.min(bodyA.material.restitution, bodyB.material.restitution);
        const j = -(1 + restitution) * normalVelocity;
        const invMassA = bodyA.isStatic ? 0 : 1 / bodyA.mass;
        const invMassB = bodyB.isStatic ? 0 : 1 / bodyB.mass;
        const totalInvMass = invMassA + invMassB;

        if (totalInvMass === 0) {
            return;
        }

        const impulseScalar = j / totalInvMass;
        const impulse = result.normal.multiply(impulseScalar);

        if (!bodyA.isStatic) {
            bodyA.linearVelocity.sub(impulse.multiply(invMassA));
            bodyA.angularVelocity.add(result.normal.cross(bodyA.linearVelocity).multiply(0.1));
        }

        if (!bodyB.isStatic) {
            bodyB.linearVelocity.add(impulse.multiply(invMassB));
            bodyB.angularVelocity.sub(result.normal.cross(bodyB.linearVelocity).multiply(0.1));
        }

        const penetration = result.penetration;
        const percent = 0.4;
        const slop = 0.01;

        const correctionMagnitude = Math.max(penetration - slop, 0) / totalInvMass * percent;
        const correction = result.normal.multiply(correctionMagnitude);

        if (!bodyA.isStatic) {
            bodyA.position.sub(correction.multiply(invMassA));
        }
        if (!bodyB.isStatic) {
            bodyB.position.add(correction.multiply(invMassB));
        }

        if (bodyA.onCollision) bodyA.onCollision(bodyB, result);
        if (bodyB.onCollision) bodyB.onCollision(bodyA, result);
    }

    updateSleepState() {
        for (const body of this.bodies) {
            if (body.type !== BodyType.DYNAMIC) continue;

            const speedSquared = body.linearVelocity.lengthSq();
            const angularSpeedSquared = body.angularVelocity.lengthSq();

            if (speedSquared < 0.01 && angularSpeedSquared < 0.01) {
                body.sleepTimer += this.fixedTimeStep;
                if (body.sleepTimer > body.sleepTimeLimit) {
                    body.sleeping = true;
                    body.linearVelocity.set(0, 0, 0);
                    body.angularVelocity.set(0, 0, 0);
                }
            } else {
                body.sleepTimer = 0;
                body.sleeping = false;
            }
        }
    }

    raycast(origin, direction, distance = 1000, filter = null) {
        const ray = new Ray(origin, direction.normalize());
        let closestHit = null;
        let closestDistance = distance;

        for (const body of this.bodies) {
            if (filter && !filter.canCollideWith(body.filter)) {
                continue;
            }

            const result = body.raycast(ray, 0, closestDistance);
            if (result && result.distance < closestDistance) {
                closestDistance = result.distance;
                closestHit = result;
                closestHit.body = body;
            }
        }

        return closestHit;
    }

    raycastAll(origin, direction, distance = 1000, filter = null) {
        const ray = new Ray(origin, direction.normalize());
        const hits = [];

        for (const body of this.bodies) {
            if (filter && !filter.canCollideWith(body.filter)) {
                continue;
            }

            const result = body.raycast(ray, 0, distance);
            if (result) {
                result.body = body;
                hits.push(result);
            }
        }

        hits.sort((a, b) => a.distance - b.distance);
        return hits;
    }

    sweepTest(shape, startPos, endPos, rotation, filter = null) {
        return [];
    }

    getBodyById(id) {
        return this.bodies.find(body => body.id === id);
    }

    clear() {
        this.bodies.length = 0;
        this.constraints.length = 0;
        this.collisionPairs.length = 0;
        this.collisionResults.length = 0;
    }
}

export class BodyType {
    static get STATIC() { return 0; }
    static get DYNAMIC() { return 1; }
    static get KINEMATIC() { return 2; }
}

export class Body {
    constructor(config = {}) {
        this.id = Body.nextId++;
        this.type = config.type || BodyType.DYNAMIC;
        this.position = config.position ? config.position.clone() : new Vector3();
        this.previousPosition = this.position.clone();
        this.linearVelocity = config.linearVelocity ? config.linearVelocity.clone() : new Vector3();
        this.angularVelocity = config.angularVelocity ? config.angularVelocity.clone() : new Vector3();
        this.quaternion = config.quaternion ? config.quaternion.clone() : Quaternion.identity();
        this.scale = config.scale ? config.scale.clone() : new Vector3(1, 1, 1);
        this.mass = config.mass || 1;
        this.invMass = this.type === BodyType.STATIC ? 0 : 1 / this.mass;
        this.inertia = config.inertia || new Vector3(1, 1, 1);
        this.invInertia = new Vector3(
            this.inertia.x !== 0 ? 1 / this.inertia.x : 0,
            this.inertia.y !== 0 ? 1 / this.inertia.y : 0,
            this.inertia.z !== 0 ? 1 / this.inertia.z : 0
        );
        this.material = config.material || PhysicsMaterial.DYNAMIC;
        this.shape = config.shape || new Sphere(new Vector3(), 1);
        this.filter = config.filter || new CollisionFilter();
        this.world = null;
        this.aabb = new AABB();
        this.friction = config.friction || 0.5;
        this.restitution = config.restitution || 0.3;
        this.linearDamping = config.linearDamping || 0.01;
        this.angularDamping = config.angularDamping || 0.01;
        this.sleeping = false;
        this.sleepTimer = 0;
        this.sleepTimeLimit = 0.5;
        this.allowSleep = config.allowSleep !== false;
        this.interpolate = config.interpolate !== false;
        this.force = new Vector3();
        this.torque = new Vector3();
        this.gravityScale = config.gravityScale !== undefined ? config.gravityScale : 1;
        this.collisionResponse = config.collisionResponse !== false;
        this.isTrigger = config.isTrigger || false;
        this.isStatic = this.type === BodyType.STATIC;
        this.onCollision = config.onCollision || null;
        this.onTrigger = config.onTrigger || null;
        this.userData = config.userData || {};
    }

    get isStatic() {
        return this.type === BodyType.STATIC;
    }

    applyForce(force, point = null) {
        this.force.add(force);
        if (point) {
            const r = point.sub(this.position);
            this.torque.add(r.cross(force));
        }
    }

    applyImpulse(impulse, point = null) {
        if (this.isStatic) return;
        this.linearVelocity.add(impulse.multiply(this.invMass));
        if (point) {
            const r = point.sub(this.position);
            this.angularVelocity.add(r.cross(impulse).multiply(this.invInertia));
        }
    }

    applyTorque(torque) {
        if (this.isStatic) return;
        this.torque.add(torque);
    }

    applyTorqueImpulse(impulse) {
        if (this.isStatic) return;
        this.angularVelocity.add(impulse.multiply(this.invInertia));
    }

    integrateForce(dt) {
        if (this.isStatic || this.sleeping) return;

        const gravity = this.world ? this.world.gravity.multiply(this.gravityScale) : new Vector3();
        this.linearVelocity.add(gravity.multiply(dt));
        this.linearVelocity.add(this.force.multiply(this.invMass * dt));

        const angularAcceleration = this.torque.multiply(this.invInertia);
        this.angularVelocity.add(angularAcceleration.multiply(dt));

        this.force.set(0, 0, 0);
        this.torque.set(0, 0, 0);
    }

    integrateVelocity(dt) {
        if (this.isStatic || this.sleeping) return;

        this.linearVelocity.multiply(1 - this.linearDamping);
        this.angularVelocity.multiply(1 - this.angularDamping);

        this.previousPosition.copy(this.position);
        this.position.add(this.linearVelocity.multiply(dt));

        const deltaRotation = this.angularVelocity.multiply(dt * 0.5);
        const newQuat = new Quaternion(
            deltaRotation.x,
            deltaRotation.y,
            deltaRotation.z,
            0
        ).multiply(this.quaternion);
        this.quaternion.x += newQuat.x;
        this.quaternion.y += newQuat.y;
        this.quaternion.z += newQuat.z;
        this.quaternion.w += newQuat.w;
        this.quaternion.normalize();
    }

    interpolate(alpha) {
        this.position.lerp(this.previousPosition, alpha);
    }

    postUpdate() {}

    updateBounds() {
        if (this.shape) {
            const localAABB = this.shape.getAABB();
            this.aabb.min = this.transformPoint(localAABB.min);
            this.aabb.max = this.transformPoint(localAABB.max);
        }
    }

    transformPoint(point) {
        const rotated = this.quaternion.multiplyVector3(point.multiply(this.scale));
        return rotated.add(this.position);
    }

    raycast(ray, tmin, tmax) {
        return null;
    }

    wakeUp() {
        this.sleeping = false;
        this.sleepTimer = 0;
    }

    putToSleep() {
        this.sleeping = true;
        this.linearVelocity.set(0, 0, 0);
        this.angularVelocity.set(0, 0, 0);
    }

    setPosition(position) {
        this.previousPosition.copy(this.position);
        this.position.copy(position);
        this.updateBounds();
    }

    setQuaternion(quaternion) {
        this.quaternion.copy(quaternion);
        this.updateBounds();
    }

    setLinearVelocity(velocity) {
        this.linearVelocity.copy(velocity);
        this.wakeUp();
    }

    setAngularVelocity(velocity) {
        this.angularVelocity.copy(velocity);
        this.wakeUp();
    }
}

Body.nextId = 0;

export class SphereBody extends Body {
    constructor(config = {}) {
        super(config);
        this.radius = config.radius || 1;
        this.shape = new Sphere(this.position, this.radius);
        this.mass = config.mass || (4 / 3 * Math.PI * Math.pow(this.radius, 3));
        this.invMass = this.type === BodyType.STATIC ? 0 : 1 / this.mass;
        this.inertia = new Vector3(
            0.4 * this.mass * this.radius * this.radius,
            0.4 * this.mass * this.radius * this.radius,
            0.4 * this.mass * this.radius * this.radius
        );
        this.invInertia = new Vector3(
            1 / this.inertia.x,
            1 / this.inertia.y,
            1 / this.inertia.z
        );
    }

    updateBounds() {
        this.aabb.min = this.position.clone().sub(new Vector3(this.radius, this.radius, this.radius));
        this.aabb.max = this.position.clone().add(new Vector3(this.radius, this.radius, this.radius));
        this.shape.center.copy(this.position);
    }

    raycast(ray, tmin, tmax) {
        return CollisionDetector.rayVsSphere(ray, this.shape, tmin, tmax);
    }
}

export class BoxBody extends Body {
    constructor(config = {}) {
        super(config);
        this.halfExtents = config.halfExtents ? config.halfExtents.clone() : new Vector3(0.5, 0.5, 0.5);
        this.shape = new AABB(
            this.halfExtents.negate(),
            this.halfExtents.clone()
        );
        this.mass = config.mass || (8 * this.halfExtents.x * this.halfExtents.y * this.halfExtents.z);
        this.invMass = this.type === BodyType.STATIC ? 0 : 1 / this.mass;
        this.inertia = new Vector3(
            (1 / 12) * this.mass * (4 * this.halfExtents.y * this.halfExtents.y + 4 * this.halfExtents.z * this.halfExtents.z),
            (1 / 12) * this.mass * (4 * this.halfExtents.x * this.halfExtents.x + 4 * this.halfExtents.z * this.halfExtents.z),
            (1 / 12) * this.mass * (4 * this.halfExtents.x * this.halfExtents.x + 4 * this.halfExtents.y * this.halfExtents.y)
        );
        this.invInertia = new Vector3(
            1 / this.inertia.x,
            1 / this.inertia.y,
            1 / this.inertia.z
        );
    }

    updateBounds() {
        const worldHalfExtents = this.quaternion.multiplyVector3(this.halfExtents.multiply(this.scale)).abs();
        this.aabb.min = this.position.sub(worldHalfExtents);
        this.aabb.max = this.position.add(worldHalfExtents);
    }

    raycast(ray, tmin, tmax) {
        return CollisionDetector.rayVsAABB(ray, this.aabb, tmin, tmax);
    }
}

export class CapsuleBody extends Body {
    constructor(config = {}) {
        super(config);
        this.radius = config.radius || 0.5;
        this.height = config.height || 2;
        this.shape = new Capsule(
            new Vector3(0, -this.height / 2, 0),
            new Vector3(0, this.height / 2, 0),
            this.radius
        );
        this.mass = config.mass || (Math.PI * this.radius * this.radius * this.height + 4 / 3 * Math.PI * Math.pow(this.radius, 3));
        this.invMass = this.type === BodyType.STATIC ? 0 : 1 / this.mass;
        const cylinderMass = Math.PI * this.radius * this.radius * this.height;
        const sphereMass = 4 / 3 * Math.PI * Math.pow(this.radius, 3);
        const totalMass = cylinderMass + sphereMass;
        this.inertia = new Vector3(
            (1 / 12) * totalMass * (3 * this.radius * this.radius + this.height * this.height),
            totalMass * this.radius * this.radius,
            (1 / 12) * totalMass * (3 * this.radius * this.radius + this.height * this.height)
        );
        this.invInertia = new Vector3(
            1 / this.inertia.x,
            1 / this.inertia.y,
            1 / this.inertia.z
        );
    }

    updateBounds() {
        const halfHeight = this.height / 2;
        const localMin = new Vector3(-this.radius, -halfHeight - this.radius, -this.radius);
        const localMax = new Vector3(this.radius, halfHeight + this.radius, this.radius);
        this.aabb.min = this.transformPoint(localMin);
        this.aabb.max = this.transformPoint(localMax);
    }

    raycast(ray, tmin, tmax) {
        const worldStart = this.transformPoint(this.shape.start);
        const worldEnd = this.transformPoint(this.shape.end);
        const worldShape = new Capsule(worldStart, worldEnd, this.radius);
        return CollisionDetector.rayVsCapsule(ray, worldShape, tmin, tmax);
    }
}

export class SpatialHash {
    constructor(world, cellSize = 2) {
        this.world = world;
        this.cellSize = cellSize;
        this.grid = new Map();
        this.bodyCells = new Map();
    }

    hash(position) {
        const x = Math.floor(position.x / this.cellSize);
        const y = Math.floor(position.y / this.cellSize);
        const z = Math.floor(position.z / this.cellSize);
        return `${x},${y},${z}`;
    }

    addBody(body) {
        const aabb = body.aabb;
        const minHash = this.hash(aabb.min);
        const maxHash = this.hash(aabb.max);

        const cells = new Set();

        const minX = Math.floor(aabb.min.x / this.cellSize);
        const maxX = Math.floor(aabb.max.x / this.cellSize);
        const minY = Math.floor(aabb.min.y / this.cellSize);
        const maxY = Math.floor(aabb.max.y / this.cellSize);
        const minZ = Math.floor(aabb.min.z / this.cellSize);
        const maxZ = Math.floor(aabb.max.z / this.cellSize);

        for (let x = minX; x <= maxX; x++) {
            for (let y = minY; y <= maxY; y++) {
                for (let z = minZ; z <= maxZ; z++) {
                    const hash = `${x},${y},${z}`;
                    cells.add(hash);
                    if (!this.grid.has(hash)) {
                        this.grid.set(hash, []);
                    }
                    this.grid.get(hash).push(body);
                }
            }
        }

        this.bodyCells.set(body.id, cells);
    }

    removeBody(body) {
        const cells = this.bodyCells.get(body.id);
        if (!cells) return;

        for (const hash of cells) {
            const cell = this.grid.get(hash);
            if (cell) {
                const index = cell.indexOf(body);
                if (index !== -1) {
                    cell.splice(index, 1);
                }
                if (cell.length === 0) {
                    this.grid.delete(hash);
                }
            }
        }

        this.bodyCells.delete(body.id);
    }

    updateBody(body) {
        this.removeBody(body);
        this.addBody(body);
    }

    update() {
        for (const body of this.world.bodies) {
            if (body.aabb) {
                this.updateBody(body);
            }
        }
    }

    getPotentialPairs(pairs) {
        const checked = new Set();

        for (const [hash, cell] of this.grid) {
            for (let i = 0; i < cell.length; i++) {
                for (let j = i + 1; j < cell.length; j++) {
                    const bodyA = cell[i];
                    const bodyB = cell[j];

                    if (bodyA.type === BodyType.STATIC && bodyB.type === BodyType.STATIC) {
                        continue;
                    }

                    const pairKey = bodyA.id < bodyB.id ? `${bodyA.id}-${bodyB.id}` : `${bodyB.id}-${bodyA.id}`;
                    if (checked.has(pairKey)) continue;
                    checked.add(pairKey);

                    if (bodyA.aabb.intersectsAABB(bodyB.aabb)) {
                        pairs.push({ bodyA, bodyB });
                    }
                }
            }
        }
    }

    getCollisionPairs(pairs) {
        this.getPotentialPairs(pairs);
    }
}

export class Narrowphase {
    constructor() {
        this.tempResult = new CollisionResult();
    }

    detectCollisions(pairs, results) {
        for (const pair of pairs) {
            const { bodyA, bodyB } = pair;

            if (!bodyA.collisionResponse || !bodyB.collisionResponse) {
                continue;
            }

            if (bodyA.filter && bodyB.filter && !bodyA.filter.canCollideWith(bodyB.filter)) {
                continue;
            }

            this.tempResult.reset();
            this.tempResult.bodyA = bodyA;
            this.tempResult.bodyB = bodyB;

            if (bodyA instanceof SphereBody && bodyB instanceof SphereBody) {
                if (CollisionDetector.sphereVsSphere(bodyA.shape, bodyB.shape, this.tempResult)) {
                    results.push(this.tempResult.clone());
                }
            } else if (bodyA instanceof BoxBody && bodyB instanceof BoxBody) {
                if (CollisionDetector.aabbVsAABB(bodyA.aabb, bodyB.aabb, this.tempResult)) {
                    results.push(this.tempResult.clone());
                }
            } else if (bodyA instanceof SphereBody && bodyB instanceof BoxBody) {
                if (CollisionDetector.sphereVsAABB(bodyA.shape, bodyB.aabb, this.tempResult)) {
                    results.push(this.tempResult.clone());
                }
            } else if (bodyA instanceof BoxBody && bodyB instanceof SphereBody) {
                if (CollisionDetector.sphereVsAABB(bodyB.shape, bodyA.aabb, this.tempResult)) {
                    this.tempResult.normal.negate();
                    results.push(this.tempResult.clone());
                }
            } else {
                if (bodyA.aabb.intersectsAABB(bodyB.aabb)) {
                    this.tempResult.penetration = bodyA.position.distanceTo(bodyB.position) * 0.1;
                    this.tempResult.normal.copy(bodyB.position.sub(bodyA.position)).normalize();
                    results.push(this.tempResult.clone());
                }
            }
        }
    }
}

export class CharacterController {
    constructor(config = {}) {
        this.position = config.position ? config.position.clone() : new Vector3();
        this.velocity = new Vector3();
        this.radius = config.radius || 0.5;
        this.height = config.height || 1.8;
        this.stepHeight = config.stepHeight || 0.3;
        this.maxSlopeAngle = config.maxSlopeAngle || 45 * Math.PI / 180;
        this.currentGroundBody = null;
        this.isGrounded = false;
        this.groundNormal = new Vector3(0, 1, 0);
        this.moveDirection = new Vector3();
        this.jumpForce = config.jumpForce || 8;
        this.speed = config.speed || 5;
        this.crouchSpeed = config.crouchSpeed || 2.5;
        this.sprintSpeed = config.sprintSpeed || 8;
        this.isCrouching = false;
        this.isSprinting = false;
        this.isJumping = false;
        this.canJump = true;
        this.jumpCooldown = 0;
        this.gravity = config.gravity || -20;
        this.world = null;
        this.filter = new CollisionFilter();
        this.filter.setGroup(CollisionFilter.CHARACTER);
        this.filter.setMask(CollisionFilter.STATIC | CollisionFilter.DYNAMIC | CollisionFilter.KINEMATIC);
    }

    setPosition(position) {
        this.position.copy(position);
    }

    move(direction, speed) {
        this.moveDirection.copy(direction);
        this.velocity.x = direction.x * speed;
        this.velocity.z = direction.z * speed;
    }

    jump() {
        if (this.isGrounded && this.canJump && this.jumpCooldown <= 0) {
            this.velocity.y = this.jumpForce;
            this.isGrounded = false;
            this.isJumping = true;
            this.canJump = false;
            this.jumpCooldown = 0.1;
        }
    }

    update(dt) {
        if (this.jumpCooldown > 0) {
            this.jumpCooldown -= dt;
        }

        if (this.isGrounded && !this.isJumping) {
            this.canJump = true;
        }

        this.velocity.y += this.gravity * dt;

        const newPosition = this.position.add(this.velocity.multiply(dt));

        if (this.world) {
            const capsule = new Capsule(
                newPosition.clone().add(new Vector3(0, this.height / 2 - this.radius, 0)),
                newPosition.clone().add(new Vector3(0, -this.height / 2 + this.radius, 0)),
                this.radius
            );

            const hits = this.world.collisionResults;

            if (this.velocity.y <= 0) {
                this.isGrounded = true;
            }

            if (hits.length > 0) {
                for (const hit of hits) {
                    if (hit.normal.y > 0) {
                        this.isGrounded = true;
                        this.groundNormal.copy(hit.normal);
                        this.currentGroundBody = hit.bodyB !== this ? hit.bodyB : hit.bodyA;
                        this.velocity.y = Math.max(0, this.velocity.y);
                        newPosition.add(hit.normal.multiply(hit.penetration));
                        break;
                    }
                }
            }

            this.position.copy(newPosition);
        } else {
            this.position.copy(newPosition);
        }

        if (this.isGrounded && this.isJumping) {
            this.isJumping = false;
        }
    }

    crouch() {
        this.isCrouching = true;
    }

    stand() {
        this.isCrouching = false;
    }

    sprint() {
        this.isSprinting = true;
    }

    stopSprint() {
        this.isSprinting = false;
    }

    getCapsule() {
        return new Capsule(
            this.position.clone().add(new Vector3(0, this.height / 2 - this.radius, 0)),
            this.position.clone().add(new Vector3(0, -this.height / 2 + this.radius, 0)),
            this.radius
        );
    }

    getCurrentSpeed() {
        if (this.isCrouching) {
            return this.crouchSpeed;
        } else if (this.isSprinting) {
            return this.sprintSpeed;
        }
        return this.speed;
    }
}

export class Ragdoll {
    constructor(position, config = {}) {
        this.position = position.clone();
        this.bodies = new Map();
        this.joints = [];
        this.isActive = false;
        this.isKinematic = true;
        this.createBodies(config);
        this.createJoints(config);
    }

    createBodies(config) {
        const headMass = config.headMass || 5;
        const torsoMass = config.torsoMass || 15;
        const pelvisMass = config.pelvisMass || 10;
        const armMass = config.armMass || 4;
        const legMass = config.legMass || 6;

        this.bodies.set('head', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(0, 1.7, 0)),
            radius: 0.12,
            height: 0.2,
            mass: headMass
        }));

        this.bodies.set('torso', new BoxBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(0, 1.3, 0)),
            halfExtents: new Vector3(0.2, 0.3, 0.12),
            mass: torsoMass
        }));

        this.bodies.set('pelvis', new BoxBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(0, 0.95, 0)),
            halfExtents: new Vector3(0.18, 0.12, 0.1),
            mass: pelvisMass
        }));

        this.bodies.set('leftUpperArm', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(-0.35, 1.35, 0)),
            radius: 0.05,
            height: 0.3,
            mass: armMass
        }));

        this.bodies.set('rightUpperArm', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(0.35, 1.35, 0)),
            radius: 0.05,
            height: 0.3,
            mass: armMass
        }));

        this.bodies.set('leftLowerArm', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(-0.35, 1.05, 0)),
            radius: 0.04,
            height: 0.3,
            mass: armMass * 0.8
        }));

        this.bodies.set('rightLowerArm', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(0.35, 1.05, 0)),
            radius: 0.04,
            height: 0.3,
            mass: armMass * 0.8
        }));

        this.bodies.set('leftUpperLeg', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(-0.12, 0.6, 0)),
            radius: 0.08,
            height: 0.4,
            mass: legMass
        }));

        this.bodies.set('rightUpperLeg', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(0.12, 0.6, 0)),
            radius: 0.08,
            height: 0.4,
            mass: legMass
        }));

        this.bodies.set('leftLowerLeg', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(-0.12, 0.2, 0)),
            radius: 0.06,
            height: 0.4,
            mass: legMass * 0.8
        }));

        this.bodies.set('rightLowerLeg', new CapsuleBody({
            type: BodyType.DYNAMIC,
            position: this.position.add(new Vector3(0.12, 0.2, 0)),
            radius: 0.06,
            height: 0.4,
            mass: legMass * 0.8
        }));
    }

    createJoints(config) {
        const stiffness = config.stiffness || 0.9;
        const damping = config.damping || 0.1;

        this.joints.push({
            bodyA: this.bodies.get('head'),
            bodyB: this.bodies.get('torso'),
            localAnchorA: new Vector3(0, -0.15, 0),
            localAnchorB: new Vector3(0, 0.3, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('torso'),
            bodyB: this.bodies.get('pelvis'),
            localAnchorA: new Vector3(0, -0.3, 0),
            localAnchorB: new Vector3(0, 0.12, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('torso'),
            bodyB: this.bodies.get('leftUpperArm'),
            localAnchorA: new Vector3(-0.2, 0.25, 0),
            localAnchorB: new Vector3(0, 0.15, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('torso'),
            bodyB: this.bodies.get('rightUpperArm'),
            localAnchorA: new Vector3(0.2, 0.25, 0),
            localAnchorB: new Vector3(0, 0.15, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('leftUpperArm'),
            bodyB: this.bodies.get('leftLowerArm'),
            localAnchorA: new Vector3(0, -0.15, 0),
            localAnchorB: new Vector3(0, 0.15, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('rightUpperArm'),
            bodyB: this.bodies.get('rightLowerArm'),
            localAnchorA: new Vector3(0, -0.15, 0),
            localAnchorB: new Vector3(0, 0.15, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('pelvis'),
            bodyB: this.bodies.get('leftUpperLeg'),
            localAnchorA: new Vector3(-0.1, -0.1, 0),
            localAnchorB: new Vector3(0, 0.2, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('pelvis'),
            bodyB: this.bodies.get('rightUpperLeg'),
            localAnchorA: new Vector3(0.1, -0.1, 0),
            localAnchorB: new Vector3(0, 0.2, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('leftUpperLeg'),
            bodyB: this.bodies.get('leftLowerLeg'),
            localAnchorA: new Vector3(0, -0.2, 0),
            localAnchorB: new Vector3(0, 0.2, 0),
            stiffness,
            damping
        });

        this.joints.push({
            bodyA: this.bodies.get('rightUpperLeg'),
            bodyB: this.bodies.get('rightLowerLeg'),
            localAnchorA: new Vector3(0, -0.2, 0),
            localAnchorB: new Vector3(0, 0.2, 0),
            stiffness,
            damping
        });
    }

    activate() {
        this.isActive = true;
        this.isKinematic = false;
        for (const body of this.bodies.values()) {
            body.type = BodyType.DYNAMIC;
        }
    }

    deactivate() {
        this.isActive = false;
        this.isKinematic = true;
        for (const body of this.bodies.values()) {
            body.type = BodyType.KINEMATIC;
        }
    }

    applyImpulse(impulse, point) {
        if (!this.isActive) return;

        for (const body of this.bodies.values()) {
            const r = point.sub(body.position);
            body.applyImpulse(impulse, point);
            body.applyTorqueImpulse(r.cross(impulse));
        }
    }

    update(dt) {
        if (!this.isActive) return;

        for (const joint of this.joints) {
            const worldAnchorA = joint.bodyA.position.add(joint.localAnchorA);
            const worldAnchorB = joint.bodyB.position.add(joint.localAnchorB);
            const delta = worldAnchorB.sub(worldAnchorA);
            const distance = delta.length();

            if (distance > 0.01) {
                const correction = delta.normalize().multiply(distance * joint.stiffness);
                joint.bodyA.position.add(correction.multiply(0.5));
                joint.bodyB.position.sub(correction.multiply(0.5));

                const relVel = joint.bodyB.linearVelocity.sub(joint.bodyA.linearVelocity);
                const dampingImpulse = relVel.multiply(joint.damping);
                joint.bodyA.linearVelocity.add(dampingImpulse.multiply(0.5));
                joint.bodyB.linearVelocity.sub(dampingImpulse.multiply(0.5));
            }
        }
    }

    getBodyPart(name) {
        return this.bodies.get(name);
    }

    getCenterOfMass() {
        let totalMass = 0;
        let centerOfMass = new Vector3();

        for (const [name, body] of this.bodies) {
            const mass = body.mass;
            centerOfMass.add(body.position.multiply(mass));
            totalMass += mass;
        }

        return centerOfMass.divide(totalMass);
    }

    getVelocity() {
        return this.bodies.get('torso').linearVelocity.clone();
    }

    getAngularVelocity() {
        return this.bodies.get('torso').angularVelocity.clone();
    }
}

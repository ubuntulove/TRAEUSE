export class Color {
    constructor(r = 1, g = 1, b = 1, a = 1) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    clone() {
        return new Color(this.r, this.g, this.b, this.a);
    }

    copy(c) {
        this.r = c.r;
        this.g = c.g;
        this.b = c.b;
        this.a = c.a;
        return this;
    }

    set(r, g, b, a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a !== undefined ? a : 1;
        return this;
    }

    setHex(hex) {
        const r = (hex >> 16 & 255) / 255;
        const g = (hex >> 8 & 255) / 255;
        const b = (hex & 255) / 255;
        this.set(r, g, b);
        return this;
    }

    lerp(c, alpha) {
        this.r += (c.r - this.r) * alpha;
        this.g += (c.g - this.g) * alpha;
        this.b += (c.b - this.b) * alpha;
        this.a += (c.a - this.a) * alpha;
        return this;
    }

    add(c) {
        this.r += c.r;
        this.g += c.g;
        this.b += c.b;
        this.a += c.a;
        return this;
    }

    multiply(c) {
        this.r *= c.r;
        this.g *= c.g;
        this.b *= c.b;
        this.a *= c.a;
        return this;
    }

    equals(c, epsilon = 0.0001) {
        return (
            Math.abs(this.r - c.r) < epsilon &&
            Math.abs(this.g - c.g) < epsilon &&
            Math.abs(this.b - c.b) < epsilon &&
            Math.abs(this.a - c.a) < epsilon
        );
    }

    toArray() {
        return [this.r, this.g, this.b, this.a];
    }

    toHex() {
        const r = Math.round(this.r * 255);
        const g = Math.round(this.g * 255);
        const b = Math.round(this.b * 255);
        return (r << 16) | (g << 8) | b;
    }

    toCSS() {
        const r = Math.round(this.r * 255);
        const g = Math.round(this.g * 255);
        const b = Math.round(this.b * 255);
        return `rgba(${r}, ${g}, ${b}, ${this.a})`;
    }

    static white() { return new Color(1, 1, 1, 1); }
    static black() { return new Color(0, 0, 0, 1); }
    static red() { return new Color(1, 0, 0, 1); }
    static green() { return new Color(0, 1, 0, 1); }
    static blue() { return new Color(0, 0, 1, 1); }
    static yellow() { return new Color(1, 1, 0, 1); }
    static cyan() { return new Color(0, 1, 1, 1); }
    static magenta() { return new Color(1, 0, 1, 1); }
    static transparent() { return new Color(0, 0, 0, 0); }
}

export class Camera {
    constructor(config = {}) {
        this.type = 'camera';
        this.position = config.position || new Vector3(0, 0, 0);
        this.rotation = config.rotation || new Quaternion();
        this.up = new Vector3(0, 1, 0);
        this.right = new Vector3(1, 0, 0);
        this.forward = new Vector3(0, 0, 1);
        this.fov = config.fov || 60;
        this.aspectRatio = config.aspectRatio || window.innerWidth / window.innerHeight;
        this.nearClip = config.nearClip || 0.1;
        this.farClip = config.farClip || 1000;
        this.orthographic = config.orthographic || false;
        this.orthographicSize = config.orthographicSize || 10;
        this.viewMatrix = new Matrix4();
        this.projectionMatrix = new Matrix4();
        this.viewProjectionMatrix = new Matrix4();
        this.frustum = new Frustum();
        this.target = config.target || new Vector3(0, 0, 0);
        this.dofEnabled = false;
        this.dofFocusDistance = 10;
        this.dofAperture = 0.025;
        this.dofMaxBlur = 0.01;
        this.bloomEnabled = false;
        this.bloomThreshold = 0.8;
        this.bloomStrength = 0.5;
        this.bloomRadius = 0.5;
        this.vignetteEnabled = false;
        this.vignetteIntensity = 0.3;
        this.vignetteSmoothness = 0.5;
        this.colorGradingEnabled = false;
        this.contrast = 1;
        this.saturation = 1;
        this.brightness = 0;
        this.temperature = 0;
        this.tint = new Color(1, 1, 1);
        this.gamma = 2.2;
        this.exposure = 1;
        this.updateMatrices();
    }

    lookAt(target) {
        this.target.copy(target);
        const forward = target.sub(this.position).normalize();
        const up = new Vector3(0, 1, 0);

        if (Math.abs(forward.y) > 0.999) {
            up.set(0, 0, -forward.y);
        }

        this.right = forward.cross(up).normalize();
        this.up = this.right.cross(forward).normalize();
        this.forward = forward.negate();

        const m = new Matrix4();
        m.set(
            new Vector4(this.right.x, this.up.x, -this.forward.x, 0),
            new Vector4(this.right.y, this.up.y, -this.forward.y, 0),
            new Vector4(this.right.z, this.up.z, -this.forward.z, 0),
            new Vector4(0, 0, 0, 1)
        );
        m.elements[12] = -this.position.dot(this.right);
        m.elements[13] = -this.position.dot(this.up);
        m.elements[14] = this.position.dot(this.forward);
        this.viewMatrix.copy(m);
    }

    updateMatrices() {
        this.lookAt(this.target);
        this.updateProjectionMatrix();
        this.viewProjectionMatrix.copy(this.projectionMatrix).multiply(this.viewMatrix);
        this.frustum.update(this.viewProjectionMatrix);
    }

    updateProjectionMatrix() {
        if (this.orthographic) {
            const halfSize = this.orthographicSize;
            const aspect = this.aspectRatio;
            const near = this.nearClip;
            const far = this.farClip;

            this.projectionMatrix.elements[0] = 1 / halfSize;
            this.projectionMatrix.elements[5] = 1 / (halfSize / aspect);
            this.projectionMatrix.elements[10] = -2 / (far - near);
            this.projectionMatrix.elements[11] = -(far + near) / (far - near);
            this.projectionMatrix.elements[14] = -1;
            this.projectionMatrix.elements[15] = 0;
        } else {
            const fov = this.fov * Math.PI / 180;
            const aspect = this.aspectRatio;
            const near = this.nearClip;
            const far = this.farClip;
            const tanHalfFov = Math.tan(fov / 2);

            this.projectionMatrix.elements[0] = 1 / (aspect * tanHalfFov);
            this.projectionMatrix.elements[5] = 1 / tanHalfFov;
            this.projectionMatrix.elements[10] = -(far + near) / (far - near);
            this.projectionMatrix.elements[11] = -1;
            this.projectionMatrix.elements[14] = -(2 * far * near) / (far - near);
            this.projectionMatrix.elements[15] = 0;
        }
    }

    setFov(fov) {
        this.fov = fov;
        this.updateProjectionMatrix();
    }

    setAspectRatio(aspect) {
        this.aspectRatio = aspect;
        this.updateProjectionMatrix();
    }

    setNearClip(near) {
        this.nearClip = near;
        this.updateProjectionMatrix();
    }

    setFarClip(far) {
        this.farClip = far;
        this.updateProjectionMatrix();
    }

    getForwardVector() {
        return this.forward.clone();
    }

    getRightVector() {
        return this.right.clone();
    }

    getUpVector() {
        return this.up.clone();
    }

    worldToScreen(point) {
        const clipSpace = this.viewProjectionMatrix.multiplyVector4(
            new Vector4(point.x, point.y, point.z, 1)
        );

        const ndc = new Vector3(
            clipSpace.x / clipSpace.w,
            clipSpace.y / clipSpace.w,
            clipSpace.z / clipSpace.w
        );

        return new Vector3(
            (ndc.x + 1) * 0.5 * this.aspectRatio * window.innerWidth,
            (1 - ndc.y) * 0.5 * window.innerHeight,
            ndc.z
        );
    }

    screenToWorld(screenX, screenY, depth = 0.5) {
        const ndc = new Vector3(
            (screenX / (this.aspectRatio * window.innerWidth)) * 2 - 1,
            1 - (screenY / window.innerHeight) * 2,
            depth * 2 - 1
        );

        const clipSpace = new Vector4(ndc.x, ndc.y, ndc.z, 1);
        const invViewProj = this.viewProjectionMatrix.inverse();

        if (!invViewProj) return null;

        const worldSpace = invViewProj.multiplyVector4(clipSpace);
        worldSpace.x /= worldSpace.w;
        worldSpace.y /= worldSpace.w;
        worldSpace.z /= worldSpace.w;

        return new Vector3(worldSpace.x, worldSpace.y, worldSpace.z);
    }

    screenToRay(screenX, screenY) {
        const near = this.screenToWorld(screenX, screenY, 0);
        const far = this.screenToWorld(screenX, screenY, 1);

        if (!near || !far) return null;

        const direction = far.sub(near).normalize();
        return new Ray(near, direction);
    }

    isPointInFrustum(point) {
        return this.frustum.containsPoint(point);
    }

    isSphereInFrustum(center, radius) {
        return this.frustum.intersectsSphere(new Sphere(center, radius));
    }

    isBoxInFrustum(min, max) {
        return this.frustum.intersectsAABB(new AABB(min, max));
    }

    clone() {
        return new Camera({
            position: this.position.clone(),
            rotation: this.rotation.clone(),
            fov: this.fov,
            aspectRatio: this.aspectRatio,
            nearClip: this.nearClip,
            farClip: this.farClip,
            orthographic: this.orthographic,
            orthographicSize: this.orthographicSize,
            target: this.target.clone()
        });
    }
}

export class Vector4 {
    constructor(x = 0, y = 0, z = 0, w = 1) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    clone() {
        return new Vector4(this.x, this.y, this.z, this.w);
    }

    copy(v) {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
        this.w = v.w;
        return this;
    }

    set(x, y, z, w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        return this;
    }

    add(v) {
        return new Vector4(this.x + v.x, this.y + v.y, this.z + v.z, this.w + v.w);
    }

    sub(v) {
        return new Vector4(this.x - v.x, this.y - v.y, this.z - v.z, this.w - v.w);
    }

    multiply(scalar) {
        return new Vector4(this.x * scalar, this.y * scalar, this.z * scalar, this.w * scalar);
    }

    divide(scalar) {
        return new Vector4(this.x / scalar, this.y / scalar, this.z / scalar, this.w / scalar);
    }

    dot(v) {
        return this.x * v.x + this.y * v.y + this.z * v.z + this.w * v.w;
    }

    length() {
        return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w);
    }

    normalize() {
        const len = this.length();
        if (len > 0) {
            return this.divide(len);
        }
        return this.clone();
    }

    lerp(v, alpha) {
        return new Vector4(
            this.x + (v.x - this.x) * alpha,
            this.y + (v.y - this.y) * alpha,
            this.z + (v.z - this.z) * alpha,
            this.w + (v.w - this.w) * alpha
        );
    }

    toArray() {
        return [this.x, this.y, this.z, this.w];
    }

    multiplyVector4(m) {
        const e = m.elements;
        return new Vector4(
            e[0] * this.x + e[4] * this.y + e[8] * this.z + e[12] * this.w,
            e[1] * this.x + e[5] * this.y + e[9] * this.z + e[13] * this.w,
            e[2] * this.x + e[6] * this.y + e[10] * this.z + e[14] * this.w,
            e[3] * this.x + e[7] * this.y + e[11] * this.z + e[15] * this.w
        );
    }
}

export class Frustum {
    constructor() {
        this.planes = [
            new Plane(),
            new Plane(),
            new Plane(),
            new Plane(),
            new Plane(),
            new Plane()
        ];
    }

    update(matrix) {
        const m = matrix.elements;

        this.planes[0].normal.set(
            m[3] - m[0], m[7] - m[4], m[11] - m[8]
        );
        this.planes[0].constant = -(m[15] - m[12]);

        this.planes[1].normal.set(
            m[3] + m[0], m[7] + m[4], m[11] + m[8]
        );
        this.planes[1].constant = -(m[15] + m[12]);

        this.planes[2].normal.set(
            m[3] + m[1], m[7] + m[5], m[11] + m[9]
        );
        this.planes[2].constant = -(m[15] + m[13]);

        this.planes[3].normal.set(
            m[3] - m[1], m[7] - m[5], m[11] - m[9]
        );
        this.planes[3].constant = -(m[15] - m[13]);

        this.planes[4].normal.set(
            m[3] - m[2], m[7] - m[6], m[11] - m[10]
        );
        this.planes[4].constant = -(m[15] - m[14]);

        this.planes[5].normal.set(
            m[3] + m[2], m[7] + m[6], m[11] + m[10]
        );
        this.planes[5].constant = -(m[15] + m[14]);

        for (const plane of this.planes) {
            plane.normalize();
        }
    }

    containsPoint(point) {
        for (const plane of this.planes) {
            if (plane.distanceToPoint(point) < 0) {
                return false;
            }
        }
        return true;
    }

    intersectsSphere(sphere) {
        for (const plane of this.planes) {
            const distance = plane.distanceToPoint(sphere.center);
            if (distance < -sphere.radius) {
                return false;
            }
        }
        return true;
    }

    intersectsAABB(aabb) {
        const center = aabb.getCenter();
        const extents = aabb.getSize().multiply(0.5);

        for (const plane of this.planes) {
            const n = plane.normal;
            const r = extents.x * Math.abs(n.x) + extents.y * Math.abs(n.y) + extents.z * Math.abs(n.z);
            const d = plane.normal.dot(center) + plane.constant;

            if (d < -r) {
                return false;
            }
        }
        return true;
    }
}

export class Light {
    static get DIRECTIONAL() { return 0; }
    static get POINT() { return 1; }
    static get SPOT() { return 2; }
    static get AMBIENT() { return 3; }
    static get HEMISPHERE() { return 4; }

    constructor(config = {}) {
        this.id = Light.nextId++;
        this.type = config.type || Light.DIRECTIONAL;
        this.color = config.color ? config.color.clone() : Color.white();
        this.intensity = config.intensity || 1;
        this.position = config.position ? config.position.clone() : new Vector3();
        this.direction = config.direction ? config.direction.clone() : new Vector3(0, -1, 0);
        this.rotation = config.rotation ? config.rotation.clone() : new Quaternion();
        this.range = config.range || 100;
        this.spotAngle = config.spotAngle || 45 * Math.PI / 180;
        this.spotExponent = config.spotExponent || 2;
        this.innerSpotAngle = config.innerSpotAngle || 30 * Math.PI / 180;
        this.castShadows = config.castShadows !== undefined ? config.castShadows : true;
        this.shadowMapSize = config.shadowMapSize || 1024;
        this.shadowBias = config.shadowBias || 0.001;
        this.shadowNormalBias = config.shadowNormalBias || 0.02;
        this.shadowRadius = config.shadowRadius || 2;
        this.shadowNearPlane = config.shadowNearPlane || 0.1;
        this.shadowFarPlane = config.shadowFarPlane || 500;
        this.shadowStrength = config.shadowStrength || 1;
        this.enabled = true;
        this.visible = true;
        this.layers = 0xFFFFFF;
        this.shadowMap = null;
        this.shadowCamera = null;
        this.lightCookie = null;
        this.lightCookieIntensity = 1;
        this.volumetricEnabled = false;
        this.volumetricIntensity = 0.1;
        this.volumetricSteps = 10;
        this.isVolumetricDirty = true;
        this.areaWidth = config.areaWidth || 10;
        this.areaHeight = config.areaHeight || 10;
        this.areaShape = config.areaShape || 'rectangle';
        this.attenuationEnabled = true;
        this.constantAttenuation = 1;
        this.linearAttenuation = 0.045;
        this.quadraticAttenuation = 0.0075;
        this.renderLayer = config.renderLayer || 0;
        this.priority = config.priority || 0;
    }

    updateShadowCamera(camera) {
        if (!this.castShadows) return;

        if (!this.shadowCamera) {
            this.shadowCamera = new Camera();
        }

        switch (this.type) {
            case Light.DIRECTIONAL:
                this.shadowCamera.position.copy(this.position);
                this.shadowCamera.position.add(this.direction.normalize().multiply(-50));
                this.shadowCamera.lookAt(this.position);
                this.shadowCamera.orthographic = true;
                this.shadowCamera.orthographicSize = 50;
                this.shadowCamera.nearClip = this.shadowNearPlane;
                this.shadowCamera.farClip = this.shadowFarPlane;
                break;

            case Light.POINT:
                this.shadowCamera.position.copy(this.position);
                this.shadowCamera.nearClip = 0.1;
                this.shadowCamera.farClip = this.range;
                this.shadowCamera.fov = 90;
                break;

            case Light.SPOT:
                this.shadowCamera.position.copy(this.position);
                this.shadowCamera.quaternion.copy(this.rotation);
                this.shadowCamera.nearClip = this.shadowNearPlane;
                this.shadowCamera.farClip = this.shadowFarPlane;
                this.shadowCamera.fov = this.spotAngle * 2 * 180 / Math.PI;
                break;
        }

        this.shadowCamera.updateMatrices();
    }

    getLightIntensity(point) {
        if (this.type === Light.DIRECTIONAL) {
            return this.intensity;
        }

        if (this.type === Light.POINT || this.type === Light.SPOT) {
            const distance = point.distanceTo(this.position);

            if (this.attenuationEnabled) {
                const attenuation = 1 / (
                    this.constantAttenuation +
                    this.linearAttenuation * distance +
                    this.quadraticAttenuation * distance * distance
                );
                return this.intensity * Math.max(0, attenuation);
            } else {
                return distance <= this.range ? this.intensity : 0;
            }
        }

        return this.intensity;
    }

    getSpotIntensity(point) {
        if (this.type !== Light.SPOT) return 1;

        const toPoint = point.sub(this.position).normalize();
        const spotDot = this.direction.normalize().dot(toPoint.negate());
        const spotCos = Math.cos(this.spotAngle);
        const innerCos = Math.cos(this.innerSpotAngle);

        if (spotDot < spotCos) return 0;

        return Math.pow((spotDot - spotCos) / (innerCos - spotCos), this.spotExponent);
    }

    getLightContribution(point) {
        const intensity = this.getLightIntensity(point);
        const spotFactor = this.type === Light.SPOT ? this.getSpotIntensity(point) : 1;

        return intensity * spotFactor * this.color.clone();
    }

    setEnabled(enabled) {
        this.enabled = enabled;
    }

    setIntensity(intensity) {
        this.intensity = intensity;
    }

    setColor(color) {
        this.color.copy(color);
    }

    clone() {
        return new Light({
            type: this.type,
            color: this.color.clone(),
            intensity: this.intensity,
            position: this.position.clone(),
            direction: this.direction.clone(),
            rotation: this.rotation.clone(),
            range: this.range,
            spotAngle: this.spotAngle,
            spotExponent: this.spotExponent,
            castShadows: this.castShadows,
            shadowMapSize: this.shadowMapSize,
            shadowBias: this.shadowBias
        });
    }
}

Light.nextId = 0;

export class Material {
    constructor(config = {}) {
        this.id = Material.nextId++;
        this.name = config.name || 'Material';
        this.shader = config.shader || null;
        this.uniforms = new Map();
        this.textures = new Map();
        this.renderQueue = config.renderQueue || 0;
        this.transparent = config.transparent || false;
        this.depthWrite = config.depthWrite !== undefined ? config.depthWrite : true;
        this.depthTest = config.depthTest !== undefined ? config.depthTest : true;
        this.blending = config.blending || 'normal';
        this.srcAlphaFactor = config.srcAlphaFactor || 'srcAlpha';
        this.dstAlphaFactor = config.dstAlphaFactor || 'oneMinusSrcAlpha';
        this.colorWrite = config.colorWrite !== undefined ? config.colorWrite : true;
        this.stencilWrite = config.stencilWrite || false;
        this.stencilRef = config.stencilRef || 0;
        this.stencilFunc = config.stencilFunc || 'always';
        this.stencilFail = config.stencilFail || 'keep';
        this.stencilZFail = config.stencilZFail || 'keep';
        this.stencilZPass = config.stencilZPass || 'keep';
        this.cullFace = config.cullFace !== undefined ? config.cullFace : true;
        this.cullFaceSide = config.cullFaceSide || 'back';
        this.flipFaces = config.flipFaces || false;
        this.polygonOffset = config.polygonOffset || false;
        this.polygonOffsetFactor = config.polygonOffsetFactor || 0;
        this.polygonOffsetUnits = config.polygonOffsetUnits || 0;
        this.alphaTest = config.alphaTest || 0;
        this.castShadows = config.castShadows !== undefined ? config.castShadows : true;
        this.receiveShadows = config.receiveShadows !== undefined ? config.receiveShadows : true;
        this.userData = {};
        this.properties = new Map();
    }

    setUniform(name, value) {
        this.uniforms.set(name, value);
    }

    getUniform(name) {
        return this.uniforms.get(name);
    }

    setTexture(name, texture) {
        this.textures.set(name, texture);
    }

    getTexture(name) {
        return this.textures.get(name);
    }

    setProperty(name, value) {
        this.properties.set(name, value);
    }

    getProperty(name) {
        return this.properties.get(name);
    }

    copy(source) {
        this.name = source.name + '_copy';
        this.shader = source.shader;
        this.uniforms = new Map(source.uniforms);
        this.textures = new Map(source.textures);
        this.renderQueue = source.renderQueue;
        this.transparent = source.transparent;
        this.depthWrite = source.depthWrite;
        this.depthTest = source.depthTest;
        this.blending = source.blending;
        this.colorWrite = source.colorWrite;
        this.cullFace = source.cullFace;
        this.cullFaceSide = source.cullFaceSide;
        this.castShadows = source.castShadows;
        this.receiveShadows = source.receiveShadows;
        this.alphaTest = source.alphaTest;
        return this;
    }

    clone() {
        const cloned = new Material();
        cloned.copy(this);
        return cloned;
    }

    dispose() {
        for (const texture of this.textures.values()) {
            if (texture && texture.dispose) {
                texture.dispose();
            }
        }
        this.textures.clear();
        this.uniforms.clear();
        this.properties.clear();
    }

    static createStandard(config = {}) {
        const material = new Material(config);
        material.setProperty('diffuse', config.diffuse || Color.white());
        material.setProperty('specular', config.specular || Color.white());
        material.setProperty('emissive', config.emissive || Color.black());
        material.setProperty('shininess', config.shininess || 30);
        material.setProperty('opacity', config.opacity || 1);
        material.setProperty('reflectivity', config.reflectivity || 0.5);
        material.setProperty('metalness', config.metalness || 0);
        material.setProperty('roughness', config.roughness || 0.5);
        material.setProperty('normalScale', config.normalScale || 1);
        material.setProperty('aoScale', config.aoScale || 1);
        material.setProperty('envMapIntensity', config.envMapIntensity || 1);
        material.transparent = config.opacity < 1;
        material.shader = 'standard';
        return material;
    }

    static createBasic(config = {}) {
        const material = new Material(config);
        material.setProperty('color', config.color || Color.white());
        material.setProperty('opacity', config.opacity || 1);
        material.setProperty('transparent', config.transparent || false);
        material.setProperty('wireframe', config.wireframe || false);
        material.transparent = config.transparent || config.opacity < 1;
        material.shader = 'basic';
        return material;
    }

    static createPhong(config = {}) {
        const material = new Material(config);
        material.setProperty('diffuse', config.diffuse || Color.white());
        material.setProperty('specular', config.specular || Color.white());
        material.setProperty('emissive', config.emissive || Color.black());
        material.setProperty('ambient', config.ambient || Color.gray());
        material.setProperty('shininess', config.shininess || 30);
        material.setProperty('opacity', config.opacity || 1);
        material.transparent = config.opacity < 1;
        material.shader = 'phong';
        return material;
    }

    static createToon(config = {}) {
        const material = new Material(config);
        material.setProperty('diffuse', config.diffuse || Color.white());
        material.setProperty('shadow', config.shadow || Color.gray());
        material.setProperty('highlight', config.highlight || Color.white());
        material.setProperty('steps', config.steps || 4);
        material.setProperty('opacity', config.opacity || 1);
        material.shader = 'toon';
        return material;
    }
}

Material.nextId = 0;

export class Texture {
    static get NEAREST() { return 'nearest'; }
    static get LINEAR() { return 'linear'; }
    static get NEAREST_MIPMAP_NEAREST() { return 'nearestMipmapNearest'; }
    static get LINEAR_MIPMAP_NEAREST() { return 'linearMipmapNearest'; }
    static get NEAREST_MIPMAP_LINEAR() { return 'nearestMipmapLinear'; }
    static get LINEAR_MIPMAP_LINEAR() { return 'linearMipmapLinear'; }

    static get CLAMP_TO_EDGE() { return 'clampToEdgeWrapping'; }
    static get REPEAT() { return 'repeatWrapping'; }
    static get MIRRORED_REPEAT() { return 'mirroredRepeatWrapping'; }

    constructor(config = {}) {
        this.id = Texture.nextId++;
        this.name = config.name || 'Texture';
        this.image = config.image || null;
        this.video = config.video || null;
        this.canvas = config.canvas || null;
        this.width = config.width || 0;
        this.height = config.height || 0;
        this.format = config.format || 'RGBAFormat';
        this.type = config.type || 'UnsignedByteType';
        this.magFilter = config.magFilter || Texture.LINEAR;
        this.minFilter = config.minFilter || Texture.LINEAR_MIPMAP_LINEAR;
        this.wrapS = config.wrapS || Texture.CLAMP_TO_EDGE;
        this.wrapT = config.wrapT || Texture.CLAMP_TO_EDGE;
        this.anisotropy = config.anisotropy || 1;
        this.generateMipmaps = config.generateMipmaps !== undefined ? config.generateMipmaps : true;
        this.flipY = config.flipY !== undefined ? config.flipY : true;
        this.premultiplyAlpha = config.premultiplyAlpha !== undefined ? config.premultiplyAlpha : true;
        this.unpackAlignment = config.unpackAlignment || 4;
        this.encoding = config.encoding || 'sRGBEncoding';
        this.needsUpdate = true;
        this.onUpdate = null;
        this.gpuTexture = null;
        this.cacheKey = '';
    }

    setImage(image) {
        this.image = image;
        if (image) {
            this.width = image.width || 0;
            this.height = image.height || 0;
        }
        this.needsUpdate = true;
    }

    setVideo(video) {
        this.video = video;
        if (video) {
            this.width = video.videoWidth || 0;
            this.height = video.videoHeight || 0;
        }
        this.needsUpdate = true;
    }

    setSize(width, height) {
        this.width = width;
        this.height = height;
        this.needsUpdate = true;
    }

    clone() {
        const cloned = new Texture({
            name: this.name + '_copy',
            width: this.width,
            height: this.height,
            format: this.format,
            type: this.type,
            magFilter: this.magFilter,
            minFilter: this.minFilter,
            wrapS: this.wrapS,
            wrapT: this.wrapT,
            anisotropy: this.anisotropy,
            generateMipmaps: this.generateMipmaps,
            flipY: this.flipY,
            encoding: this.encoding
        });

        if (this.image) cloned.image = this.image;
        if (this.video) cloned.video = this.video;
        if (this.canvas) cloned.canvas = this.canvas;

        return cloned;
    }

    dispose() {
        if (this.gpuTexture) {
            this.gpuTexture = null;
        }
        this.needsUpdate = false;
    }

    static createFromData(data, width, height, config = {}) {
        const texture = new Texture(config);
        texture.width = width;
        texture.height = height;
        texture.image = {
            data,
            width,
            height
        };
        return texture;
    }

    static createSolid(color, width = 1, height = 1) {
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const ctx = canvas.getContext('2d');
        ctx.fillStyle = typeof color === 'string' ? color : color.toCSS();
        ctx.fillRect(0, 0, width, height);

        const texture = new Texture({ name: 'Solid' });
        texture.canvas = canvas;
        return texture;
    }

    static createGradient(stops, width = 256, height = 256) {
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const ctx = canvas.getContext('2d');

        const gradient = ctx.createLinearGradient(0, 0, width, height);
        for (const [position, color] of stops) {
            gradient.addColorStop(position, color);
        }
        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, width, height);

        const texture = new Texture({ name: 'Gradient' });
        texture.canvas = canvas;
        return texture;
    }

    static createNoise(width = 256, height = 256, config = {}) {
        const data = new Uint8Array(width * height * 4);
        const scale = config.scale || 1;
        const octaves = config.octaves || 4;
        const persistence = config.persistence || 0.5;

        for (let y = 0; y < height; y++) {
            for (let x = 0; x < width; x++) {
                let noise = 0;
                let amplitude = 1;
                let frequency = scale;

                for (let o = 0; o < octaves; o++) {
                    noise += this.noise2D(x * frequency, y * frequency) * amplitude;
                    amplitude *= persistence;
                    frequency *= 2;
                }

                const value = Math.floor((noise + 1) * 0.5 * 255);
                const idx = (y * width + x) * 4;
                data[idx] = value;
                data[idx + 1] = value;
                data[idx + 2] = value;
                data[idx + 3] = 255;
            }
        }

        return Texture.createFromData(data, width, height, { name: 'Noise' });
    }

    static noise2D(x, y) {
        const n = Math.sin(x * 12.9898 + y * 78.233) * 43758.5453;
        return (n - Math.floor(n)) * 2 - 1;
    }
}

Texture.nextId = 0;

export class RenderTarget {
    constructor(config = {}) {
        this.id = RenderTarget.nextId++;
        this.width = config.width || window.innerWidth;
        this.height = config.height || window.innerHeight;
        this.minFilter = config.minFilter || Texture.LINEAR;
        this.magFilter = config.magFilter || Texture.LINEAR;
        this.format = config.format || 'RGBAFormat';
        this.type = config.type || 'UnsignedByteType';
        this.depthBuffer = config.depthBuffer !== undefined ? config.depthBuffer : true;
        this.stencilBuffer = config.stencilBuffer || false;
        this.depthTexture = null;
        this.texture = new Texture({ name: 'RenderTarget' });
        this.texture.width = this.width;
        this.texture.height = this.height;
        this.texture.format = this.format;
        this.texture.type = this.type;
        this.gpuTarget = null;
        this.needsUpdate = true;
    }

    setSize(width, height) {
        this.width = width;
        this.height = height;
        this.texture.width = width;
        this.texture.height = height;
        this.needsUpdate = true;
    }

    clone() {
        return new RenderTarget({
            width: this.width,
            height: this.height,
            minFilter: this.minFilter,
            magFilter: this.magFilter,
            format: this.format,
            type: this.type,
            depthBuffer: this.depthBuffer,
            stencilBuffer: this.stencilBuffer
        });
    }

    dispose() {
        if (this.gpuTarget) {
            this.gpuTarget = null;
        }
        this.texture.dispose();
        if (this.depthTexture) {
            this.depthTexture.dispose();
        }
    }
}

RenderTarget.nextId = 0;

export class Scene {
    constructor() {
        this.name = 'Scene';
        this.background = null;
        this.fog = null;
        this.fogColor = new Color(0.5, 0.5, 0.5);
        this.fogNear = 1;
        this.fogFar = 1000;
        this.overrideMaterial = null;
        this.autoUpdate = true;
        this.needsUpdate = false;
        this.backgroundColor = new Color();
        this.backgroundTexture = null;
        this.lights = [];
        this.objects = [];
        this.gameObjects = new Map();
        this.cameras = [];
        this.activeCamera = null;
        this.layers = new Set();
        this.clippingPlanes = [];
        this.localClippingEnabled = false;
        this.sortObjects = true;
        this.tree = new SceneGraph(this);
    }

    add(object) {
        this.objects.push(object);
        object.scene = this;

        if (object instanceof GameObject) {
            this.gameObjects.set(object.id, object);
        }

        if (object instanceof Light) {
            this.lights.push(object);
        }

        if (object instanceof Camera) {
            this.cameras.push(object);
            if (!this.activeCamera) {
                this.activeCamera = object;
            }
        }

        this.needsUpdate = true;
        return this;
    }

    remove(object) {
        const index = this.objects.indexOf(object);
        if (index !== -1) {
            this.objects.splice(index, 1);
        }

        object.scene = null;

        if (object instanceof GameObject) {
            this.gameObjects.delete(object.id);
        }

        if (object instanceof Light) {
            const lightIndex = this.lights.indexOf(object);
            if (lightIndex !== -1) {
                this.lights.splice(lightIndex, 1);
            }
        }

        if (object instanceof Camera) {
            const cameraIndex = this.cameras.indexOf(object);
            if (cameraIndex !== -1) {
                this.cameras.splice(cameraIndex, 1);
            }
            if (this.activeCamera === object) {
                this.activeCamera = this.cameras[0] || null;
            }
        }

        this.needsUpdate = true;
        return this;
    }

    traverse(callback) {
        callback(this);
        for (const object of this.objects) {
            object.traverse(callback);
        }
    }

    traverseVisible(callback) {
        if (!this.visible) return;
        callback(this);
        for (const object of this.objects) {
            if (object.visible) {
                object.traverse(callback);
            }
        }
    }

    update(camera) {
        if (this.autoUpdate) {
            this.updateMatrixWorld();
        }
    }

    updateMatrixWorld() {
        for (const object of this.objects) {
            object.updateMatrixWorld();
        }
    }

    getObjectById(id) {
        return this.gameObjects.get(id);
    }

    getObjectByName(name) {
        for (const object of this.objects) {
            if (object.name === name) {
                return object;
            }
        }
        return null;
    }

    getObjectsByType(type) {
        const results = [];
        for (const object of this.objects) {
            if (object instanceof type) {
                results.push(object);
            }
        }
        return results;
    }

    getLights() {
        return this.lights.filter(light => light.enabled);
    }

    dispose() {
        for (const object of this.objects) {
            if (object.dispose) {
                object.dispose();
            }
        }
        this.objects = [];
        this.gameObjects.clear();
        this.lights = [];
        this.cameras = [];
    }
}

export class GameObject {
    static nextId = 0;

    constructor(config = {}) {
        this.id = GameObject.nextId++;
        this.name = config.name || 'GameObject';
        this.tag = config.tag || 'Untagged';
        this.layer = config.layer || 0;
        this.active = config.active !== undefined ? config.active : true;
        this.visible = config.visible !== undefined ? config.visible : true;
        this.static = config.static || false;
        this.position = config.position ? config.position.clone() : new Vector3();
        this.rotation = config.rotation ? config.rotation.clone() : new Quaternion();
        this.scale = config.scale ? config.scale.clone() : new Vector3(1, 1, 1);
        this.localPosition = this.position.clone();
        this.localRotation = this.rotation.clone();
        this.localScale = this.scale.clone();
        this.matrix = new Matrix4();
        this.matrixWorld = new Matrix4();
        this.parent = null;
        this.children = [];
        this.components = new Map();
        this.scene = null;
        this.boundingBox = null;
        this.boundingSphere = null;
        this.frustumCulled = true;
        this.renderOrder = 0;
        this.userData = {};
        this.transform = new Transform(this);
        this.name = config.name || `GameObject_${this.id}`;
        this.updateMatrix();
    }

    addChild(child) {
        if (child.parent) {
            child.parent.removeChild(child);
        }
        child.parent = this;
        this.children.push(child);
        if (this.scene) {
            child.setScene(this.scene);
        }
        child.updateMatrixWorld();
        return this;
    }

    removeChild(child) {
        const index = this.children.indexOf(child);
        if (index !== -1) {
            this.children.splice(index, 1);
            child.parent = null;
        }
        return this;
    }

    setScene(scene) {
        this.scene = scene;
        for (const child of this.children) {
            child.setScene(scene);
        }
    }

    addComponent(component) {
        component.gameObject = this;
        this.components.set(component.constructor.name, component);
        if (component.awake) {
            component.awake();
        }
        return component;
    }

    getComponent(componentClass) {
        return this.components.get(componentClass.name);
    }

    getComponents(componentClass) {
        const results = [];
        for (const component of this.components.values()) {
            if (component instanceof componentClass) {
                results.push(component);
            }
        }
        return results;
    }

    removeComponent(component) {
        for (const [name, c] of this.components) {
            if (c === component) {
                if (component.onDestroy) {
                    component.onDestroy();
                }
                this.components.delete(name);
                return true;
            }
        }
        return false;
    }

    updateMatrix() {
        this.matrix.compose(this.localPosition, this.localRotation, this.localScale);
    }

    updateMatrixWorld(force = false) {
        this.updateMatrix();

        if (this.parent) {
            this.matrixWorld.copy(this.parent.matrixWorld).multiply(this.matrix);
        } else {
            this.matrixWorld.copy(this.matrix);
        }

        const worldPosition = new Vector3();
        const worldRotation = new Quaternion();
        const worldScale = new Vector3();
        this.matrixWorld.decompose(worldPosition, worldRotation, worldScale);
        this.position.copy(worldPosition);
        this.rotation.copy(worldRotation);
        this.scale.copy(worldScale);

        for (const child of this.children) {
            child.updateMatrixWorld(force);
        }
    }

    translate(x, y, z) {
        const v = new Vector3(x, y, z);
        v.applyQuaternion(this.localRotation);
        this.localPosition.add(v);
        return this;
    }

    translateX(x) {
        return this.translate(x, 0, 0);
    }

    translateY(y) {
        return this.translate(0, y, 0);
    }

    translateZ(z) {
        return this.translate(0, 0, z);
    }

    rotate(x, y, z, worldSpace = false) {
        const euler = new Vector3(x, y, z);
        const q = new Quaternion().setFromEuler({ x: euler.x, y: euler.y, z: euler.z });

        if (worldSpace || !this.parent) {
            this.localRotation = this.localRotation.multiply(q);
        } else {
            const parentWorldRotation = this.parent.rotation.clone();
            const worldQ = parentWorldRotation.multiply(q).multiply(parentWorldRotation.inverse());
            this.localRotation = this.parent.matrixWorld.inverse().multiply(worldQ);
        }

        return this;
    }

    lookAt(target) {
        const targetPos = target instanceof GameObject ? target.position.clone() : target.clone();
        const worldMatrix = this.parent ? this.parent.matrixWorld : new Matrix4();
        const localMatrix = worldMatrix.inverse();

        const localTarget = targetPos.applyMatrix4(localMatrix);
        const direction = localTarget.sub(new Vector3());

        const m = new Matrix4();
        m.lookAt(new Vector3(), direction, Vector3.up());
        const q = new Quaternion().setFromRotationMatrix(m);

        this.localRotation.copy(q);
        return this;
    }

    getWorldPosition() {
        return this.position.clone();
    }

    getWorldRotation() {
        return this.rotation.clone();
    }

    getWorldScale() {
        return this.scale.clone();
    }

    getWorldDirection() {
        const forward = new Vector3(0, 0, 1);
        return forward.applyQuaternion(this.rotation);
    }

    getForwardVector() {
        return this.getWorldDirection();
    }

    getRightVector() {
        const right = new Vector3(1, 0, 0);
        return right.applyQuaternion(this.rotation);
    }

    getUpVector() {
        const up = new Vector3(0, 1, 0);
        return up.applyQuaternion(this.rotation);
    }

    distanceTo(other) {
        return this.position.distanceTo(other.position);
    }

    clone(recursive = true) {
        const cloned = new GameObject({
            name: this.name + '_clone',
            tag: this.tag,
            layer: this.layer,
            active: this.active,
            visible: this.visible,
            static: this.static,
            position: this.localPosition.clone(),
            rotation: this.localRotation.clone(),
            scale: this.localScale.clone()
        });

        if (recursive) {
            for (const child of this.children) {
                const childClone = child.clone(true);
                cloned.addChild(childClone);
            }
        }

        for (const component of this.components.values()) {
            const componentClone = component.clone();
            cloned.addComponent(componentClone);
        }

        return cloned;
    }

    destroy() {
        for (const child of this.children) {
            child.destroy();
        }

        for (const component of this.components.values()) {
            if (component.onDestroy) {
                component.onDestroy();
            }
        }

        this.components.clear();

        if (this.parent) {
            this.parent.removeChild(this);
        }

        if (this.scene) {
            this.scene.remove(this);
        }
    }

    traverse(callback) {
        callback(this);
        for (const child of this.children) {
            child.traverse(callback);
        }
    }

    getBounds() {
        if (!this.boundingBox) {
            this.boundingBox = new AABB();
        }
        return this.boundingBox;
    }
}

export class Transform {
    constructor(gameObject) {
        this.gameObject = gameObject;
        this.localPosition = new Vector3();
        this.localRotation = new Quaternion();
        this.localScale = new Vector3(1, 1, 1);
        this.position = new Vector3();
        this.rotation = new Quaternion();
        this.scale = new Vector3(1, 1, 1);
        this.matrix = new Matrix4();
        this.matrixWorld = new Matrix4();
        this.hasChanged = false;
        this.dirty = true;
    }

    setLocalPosition(x, y, z) {
        this.localPosition.set(x, y, z);
        this.dirty = true;
        return this;
    }

    setLocalRotation(x, y, z, w) {
        this.localRotation.set(x, y, z, w);
        this.dirty = true;
        return this;
    }

    setLocalScale(x, y, z) {
        this.localScale.set(x, y, z);
        this.dirty = true;
        return this;
    }

    translate(x, y, z, space = 'local') {
        const v = new Vector3(x, y, z);
        if (space === 'world') {
            v.applyQuaternion(this.gameObject.rotation);
        }
        this.localPosition.add(v);
        this.dirty = true;
        return this;
    }

    rotate(x, y, z, space = 'local') {
        const euler = new Vector3(x, y, z);
        const q = new Quaternion().setFromEuler({ x: euler.x, y: euler.y, z: euler.z });

        if (space === 'world' || !this.gameObject.parent) {
            this.localRotation = this.localRotation.multiply(q);
        } else {
            const parentWorldRotation = this.gameObject.parent.rotation.clone();
            const worldQ = parentWorldRotation.multiply(q);
            this.localRotation = this.gameObject.parent.matrixWorld.inverse().multiply(worldQ);
        }

        this.dirty = true;
        return this;
    }

    rotateAround(point, axis, angle) {
        const worldPoint = point instanceof Vector3 ? point : point.clone();
        const q = Quaternion.fromAxisAngle(axis, angle);

        this.position.sub(worldPoint);
        this.position.applyQuaternion(q);
        this.position.add(worldPoint);
        this.position.applyQuaternion(q);

        this.rotation.premultiply(q);

        this.dirty = true;
        return this;
    }

    lookAt(target, up = new Vector3(0, 1, 0)) {
        const targetPos = target instanceof GameObject ? target.position : target;
        const direction = targetPos.sub(this.position).normalize();

        const m = new Matrix4();
        m.lookAt(new Vector3(), direction, up);
        const q = new Quaternion().setFromRotationMatrix(m);

        this.rotation.copy(q);
        this.dirty = true;
        return this;
    }

    getForward() {
        const forward = new Vector3(0, 0, 1);
        return forward.applyQuaternion(this.rotation);
    }

    getRight() {
        const right = new Vector3(1, 0, 0);
        return right.applyQuaternion(this.rotation);
    }

    getUp() {
        const up = new Vector3(0, 1, 0);
        return up.applyQuaternion(this.rotation);
    }

    transformDirection(direction) {
        return direction.applyQuaternion(this.rotation);
    }

    transformPoint(point) {
        return point.applyMatrix4(this.matrixWorld);
    }

    transformVector(vector) {
        return vector.applyQuaternion(this.rotation);
    }

    inverseTransformPoint(point) {
        const invMatrix = this.matrixWorld.inverse();
        if (!invMatrix) return point.clone();
        return point.applyMatrix4(invMatrix);
    }

    inverseTransformDirection(direction) {
        const invRotation = this.rotation.inverse();
        return direction.applyQuaternion(invRotation);
    }

    update() {
        if (!this.dirty) return;

        this.position.copy(this.localPosition);
        this.rotation.copy(this.localRotation);
        this.scale.copy(this.localScale);

        this.matrix.compose(this.localPosition, this.localRotation, this.localScale);

        if (this.gameObject.parent) {
            this.matrixWorld.copy(this.gameObject.parent.matrixWorld).multiply(this.matrix);
        } else {
            this.matrixWorld.copy(this.matrix);
        }

        this.dirty = false;
    }
}

export class SceneGraph {
    constructor(scene) {
        this.scene = scene;
        this.bounds = new AABB();
        this.autoUpdate = true;
        this.frustumCulled = true;
    }

    update() {
        if (this.autoUpdate) {
            this.scene.updateMatrixWorld();
            this.updateBounds();
        }
    }

    updateBounds() {
        this.bounds.makeEmpty();
        for (const object of this.scene.objects) {
            if (object.getBounds) {
                const objBounds = object.getBounds();
                if (objBounds) {
                    this.bounds.union(objBounds);
                }
            }
        }
    }

    raycast(ray, intersects) {
        intersects = intersects || [];

        for (const object of this.scene.objects) {
            if (object.raycast) {
                object.raycast(ray, intersects);
            }
        }

        intersects.sort((a, b) => a.distance - b.distance);
        return intersects;
    }

    traverse(callback) {
        this.scene.traverse(callback);
    }

    traverseVisible(callback) {
        this.scene.traverseVisible(callback);
    }
}

export class PostProcessing {
    constructor() {
        this.effects = [];
        this.enabled = true;
        this.renderPass = null;
        this.material = null;
        this.uniforms = new Map();
    }

    addEffect(effect) {
        this.effects.push(effect);
        effect.postProcessing = this;
        if (effect.initialize) {
            effect.initialize();
        }
        return this;
    }

    removeEffect(effect) {
        const index = this.effects.indexOf(effect);
        if (index !== -1) {
            this.effects.splice(index, 1);
            effect.postProcessing = null;
        }
        return this;
    }

    render(renderer, scene, camera, renderTarget) {
        if (!this.enabled || this.effects.length === 0) {
            return;
        }

        let currentTarget = renderTarget;

        for (const effect of this.effects) {
            if (!effect.enabled) continue;

            const nextTarget = effect === this.effects[this.effects.length - 1]
                ? null
                : this.getTempTarget();

            if (effect.render) {
                effect.render(renderer, scene, camera, currentTarget, nextTarget);
            }

            if (nextTarget) {
                currentTarget = nextTarget;
            }
        }
    }

    getTempTarget() {
        return new RenderTarget({
            width: window.innerWidth,
            height: window.innerHeight
        });
    }

    dispose() {
        for (const effect of this.effects) {
            if (effect.dispose) {
                effect.dispose();
            }
        }
        this.effects = [];
    }
}

export class BloomEffect {
    constructor(config = {}) {
        this.enabled = true;
        this.threshold = config.threshold || 0.8;
        this.strength = config.strength || 1;
        this.radius = config.radius || 1;
        this.resolution = config.resolution || { width: 512, height: 512 };
        this.renderTargets = [];
        this.separableBlurMaterial = null;
        this.compositeMaterial = null;
    }

    initialize() {
        this.renderTargets = [
            new RenderTarget({ width: this.resolution.width, height: this.resolution.height }),
            new RenderTarget({ width: this.resolution.width, height: this.resolution.height }),
            new RenderTarget({ width: this.resolution.width, height: this.resolution.height })
        ];
    }

    render(renderer, scene, camera, inputTarget, outputTarget) {
        console.log('Bloom effect rendering...');
    }

    dispose() {
        for (const target of this.renderTargets) {
            target.dispose();
        }
        this.renderTargets = [];
    }
}

export class SSAOEffect {
    constructor(config = {}) {
        this.enabled = true;
        this.radius = config.radius || 32;
        this.intensity = config.intensity || 1;
        this.bias = config.bias || 0.025;
        this.power = config.power || 2;
        this.samples = config.samples || 16;
        this.renderTargets = [];
    }

    initialize() {
        this.renderTargets = [
            new RenderTarget({ width: 512, height: 512 }),
            new RenderTarget({ width: 512, height: 512 })
        ];
    }

    render(renderer, scene, camera, inputTarget, outputTarget) {
        console.log('SSAO effect rendering...');
    }

    dispose() {
        for (const target of this.renderTargets) {
            target.dispose();
        }
        this.renderTargets = [];
    }
}

export class Renderer {
    constructor(config = {}) {
        this.canvas = config.canvas || document.createElement('canvas');
        this.context = this.canvas.getContext('webgl2') || this.canvas.getContext('webgl');
        this.width = config.width || window.innerWidth;
        this.height = config.height || window.innerHeight;
        this.pixelRatio = config.pixelRatio || Math.min(window.devicePixelRatio, 2);
        this.antialias = config.antialias !== undefined ? config.antialias : true;
        this.alpha = config.alpha !== undefined ? config.alpha : true;
        this.premultipliedAlpha = config.premultipliedAlpha !== undefined ? config.premultipliedAlpha : true;
        this.shadowMapEnabled = config.shadowMap !== undefined ? config.shadowMap : true;
        this.shadowMapType = config.shadowMapType || 'PCFSoftShadowMap';
        this.shadowMapSize = config.shadowMapSize || 2048;
        this.shadowMapBias = 0.0001;
        this.shadowMapRadius = 4;
        this.outputEncoding = 'sRGBEncoding';
        this.sortObjects = true;
        this.clearColor = new Color(0, 0, 0, 1);
        this.autoClear = true;
        this.autoClearColor = true;
        this.autoClearDepth = true;
        this.autoClearStencil = true;
        this.clippingPlanes = [];
        this.localClippingEnabled = false;
        this.maxLights = 8;
        this.currentRenderTarget = null;
        this.currentMaterial = null;
        this.currentGeometry = null;
        this.currentProgram = null;
        this.info = {
            render: { calls: 0, faces: 0, points: 0, lines: 0 },
            memory: { geometries: 0, textures: 0 },
            programs: []
        };
        this.time = 0;
        this.frameCount = 0;
        this.initialize();
    }

    initialize() {
        const gl = this.context;
        if (!gl) {
            console.error('WebGL not supported');
            return;
        }

        this.canvas.width = this.width * this.pixelRatio;
        this.canvas.height = this.height * this.pixelRatio;
        this.canvas.style.width = this.width + 'px';
        this.canvas.style.height = this.height + 'px';

        gl.enable(gl.DEPTH_TEST);
        gl.enable(gl.CULL_FACE);
        gl.cullFace(gl.BACK);

        gl.clearColor(
            this.clearColor.r,
            this.clearColor.g,
            this.clearColor.b,
            this.clearColor.a
        );

        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
        gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, this.premultipliedAlpha);
    }

    setSize(width, height) {
        this.width = width;
        this.height = height;
        this.canvas.width = width * this.pixelRatio;
        this.canvas.height = height * this.pixelRatio;
        this.canvas.style.width = width + 'px';
        this.canvas.style.height = height + 'px';
    }

    setClearColor(color) {
        this.clearColor.copy(color);
        this.context.clearColor(color.r, color.g, color.b, color.a);
    }

    clear(color = true, depth = true, stencil = true) {
        const gl = this.context;
        let bits = 0;
        if (color) bits |= gl.COLOR_BUFFER_BIT;
        if (depth) bits |= gl.DEPTH_BUFFER_BIT;
        if (stencil) bits |= gl.STENCIL_BUFFER_BIT;
        gl.clear(bits);
    }

    render(scene, camera, renderTarget) {
        if (!camera) {
            console.warn('Renderer.render: camera is required');
            return;
        }

        const gl = this.context;

        if (this.autoClear) {
            this.clear(this.autoClearColor, this.autoClearDepth, this.autoClearStencil);
        }

        scene.update(camera);

        const lights = scene.getLights();
        const objects = this.getObjectsToRender(scene);

        if (this.sortObjects) {
            objects.sort((a, b) => {
                if (a.renderOrder !== b.renderOrder) {
                    return a.renderOrder - b.renderOrder;
                }
                if (a.material && b.material) {
                    return (a.material.renderQueue || 0) - (b.material.renderQueue || 0);
                }
                return 0;
            });
        }

        this.info.render.calls = 0;
        this.info.render.faces = 0;
        this.info.render.points = 0;
        this.info.render.lines = 0;

        for (const object of objects) {
            if (!object.visible) continue;
            this.renderObject(object, scene, camera, lights);
        }

        this.frameCount++;
        this.time += 1 / 60;
    }

    getObjectsToRender(scene) {
        const objects = [];

        const processObject = (object) => {
            if (!object.visible) return;
            if (object.frustumCulled && !camera.isBoxInFrustum(object.boundingBox)) {
                return;
            }

            if (object instanceof GameObject) {
                if (object.getComponent && object.getComponent(Mesh)) {
                    objects.push(object);
                }

                for (const child of object.children) {
                    processObject(child);
                }
            }
        };

        const camera = scene.activeCamera;

        for (const object of scene.objects) {
            processObject(object);
        }

        return objects;
    }

    renderObject(object, scene, camera, lights) {
        const gl = this.context;
        const mesh = object.getComponent(Mesh);
        if (!mesh || !mesh.material) return;

        const geometry = mesh.geometry;
        const material = mesh.material;

        this.info.render.calls++;

        if (material.opacity < 1) {
            gl.enable(gl.BLEND);
            gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        }

        if (!material.depthWrite) {
            gl.depthMask(false);
        }

        if (material.cullFace !== undefined) {
            if (material.cullFace) {
                gl.enable(gl.CULL_FACE);
                gl.cullFace(material.cullFaceSide === 'front' ? gl.FRONT : gl.BACK);
            } else {
                gl.disable(gl.CULL_FACE);
            }
        }

        this.info.render.faces += geometry.indexCount / 3;
        this.info.render.points += geometry.positionCount;
    }

    dispose() {
        this.context.clear(this.context.COLOR_BUFFER_BIT | this.context.DEPTH_BUFFER_BIT);
        this.info.programs = [];
    }
}

export class Mesh {
    constructor(geometry, material) {
        this.geometry = geometry;
        this.material = material;
        this.frustumCulled = true;
        this.renderOrder = 0;
        this.castShadow = true;
        this.receiveShadow = true;
        this.boundingBox = null;
        this.boundingSphere = null;
        this.visible = true;
        this.layers = 0xFFFFFF;
    }

    raycast(ray, intersects) {
        if (!this.geometry || !this.geometry.boundingBox) {
            this.geometry.computeBoundingBox();
        }

        if (this.geometry.boundingBox) {
            const result = {};
            if (this.geometry.boundingBox.intersectsRay(ray, result)) {
                intersects.push({
                    object: this,
                    distance: result.distance,
                    point: result.point,
                    face: null,
                    uv: null
                });
            }
        }
    }

    clone() {
        const cloned = new Mesh(
            this.geometry ? this.geometry.clone() : null,
            this.material ? this.material.clone() : null
        );
        cloned.copy(this);
        return cloned;
    }

    copy(source) {
        this.geometry = source.geometry;
        this.material = source.material;
        this.frustumCulled = source.frustumCulled;
        this.renderOrder = source.renderOrder;
        this.castShadow = source.castShadow;
        this.receiveShadow = source.receiveShadow;
        this.visible = source.visible;
        this.layers = source.layers;
        return this;
    }

    dispose() {
        if (this.geometry) {
            this.geometry.dispose();
        }
        if (this.material) {
            this.material.dispose();
        }
    }
}

export class Geometry {
    constructor() {
        this.id = Geometry.nextId++;
        this.attributes = new Map();
        this.index = null;
        this.boundingBox = null;
        this.boundingSphere = null;
        this.positionCount = 0;
        this.indexCount = 0;
        this.drawRange = { start: 0, count: Infinity };
        this.groups = [];
        this.userData = {};
    }

    setAttribute(name, attribute) {
        this.attributes.set(name, attribute);
        if (name === 'position') {
            this.positionCount = attribute.count;
        }
        if (name === 'index') {
            this.indexCount = attribute.count;
        }
    }

    getAttribute(name) {
        return this.attributes.get(name);
    }

    removeAttribute(name) {
        this.attributes.delete(name);
    }

    computeBoundingBox() {
        const position = this.attributes.get('position');
        if (!position) {
            this.boundingBox = new AABB();
            return;
        }

        this.boundingBox = new AABB();
        const min = new Vector3(Infinity, Infinity, Infinity);
        const max = new Vector3(-Infinity, -Infinity, -Infinity);

        for (let i = 0; i < position.count; i++) {
            const x = position.getX(i);
            const y = position.getY(i);
            const z = position.getZ(i);

            min.x = Math.min(min.x, x);
            min.y = Math.min(min.y, y);
            min.z = Math.min(min.z, z);
            max.x = Math.max(max.x, x);
            max.y = Math.max(max.y, y);
            max.z = Math.max(max.z, z);
        }

        this.boundingBox.min.copy(min);
        this.boundingBox.max.copy(max);
    }

    computeBoundingSphere() {
        if (!this.boundingBox) {
            this.computeBoundingBox();
        }

        const center = this.boundingBox.getCenter();
        let maxRadius = 0;

        const position = this.attributes.get('position');
        if (position) {
            for (let i = 0; i < position.count; i++) {
                const x = position.getX(i);
                const y = position.getY(i);
                const z = position.getZ(i);
                const dist = Math.sqrt(
                    (x - center.x) ** 2 +
                    (y - center.y) ** 2 +
                    (z - center.z) ** 2
                );
                maxRadius = Math.max(maxRadius, dist);
            }
        }

        this.boundingSphere = new Sphere(center, maxRadius);
    }

    clone() {
        const cloned = new Geometry();
        for (const [name, attribute] of this.attributes) {
            cloned.setAttribute(name, attribute.clone());
        }
        if (this.index) {
            cloned.index = this.index.clone();
        }
        cloned.drawRange = { ...this.drawRange };
        cloned.groups = this.groups.map(g => ({ ...g }));
        return cloned;
    }

    dispose() {
        for (const attribute of this.attributes.values()) {
            if (attribute.array) {
                attribute.array = null;
            }
        }
        this.attributes.clear();
        if (this.index && this.index.array) {
            this.index.array = null;
        }
        this.index = null;
    }

    static createBox(width = 1, height = 1, depth = 1, widthSegments = 1, heightSegments = 1, depthSegments = 1) {
        const geometry = new Geometry();
        const vertices = [];
        const normals = [];
        const uvs = [];
        const indices = [];

        const buildPlane = (u, v, w, udir, vdir, width, height, depth, materialIndex) => {
            const wMid = width / 2;
            const hMid = height / 2;
            const gridX = widthSegments || 1;
            const gridY = heightSegments || 1;
            const segWidth = width / gridX;
            const segHeight = height / gridY;

            const normal = new Vector3();
            normal.setComponent(u, 1).normalize();

            for (let iy = 0; iy <= gridY; iy++) {
                for (let ix = 0; ix <= gridX; ix++) {
                    const p = new Vector3();
                    p.setComponent(u, (ix * segWidth - wMid) * udir);
                    p.setComponent(v, (iy * segHeight - hMid) * vdir);
                    p.setComponent(w, depth / 2);

                    vertices.push(p.x, p.y, p.z);
                    normals.push(normal.x, normal.y, normal.z);
                    uvs.push(ix / gridX, iy / gridY);
                }
            }

            for (let iy = 0; iy < gridY; iy++) {
                for (let ix = 0; ix < gridX; ix++) {
                    const a = materialIndex + ix + (gridX + 1) * iy;
                    const b = materialIndex + ix + (gridX + 1) * (iy + 1);
                    const c = materialIndex + ix + 1 + (gridX + 1) * (iy + 1);
                    const d = materialIndex + ix + 1 + (gridX + 1) * iy;

                    indices.push(a, b, d);
                    indices.push(b, c, d);
                }
            }
        };

        buildPlane(2, 1, 0, -1, -1, depth, height, depth, vertices.length / 3);
        buildPlane(2, 1, 0, 1, -1, depth, height, -depth, vertices.length / 3);
        buildPlane(0, 2, 1, 1, 1, width, depth, depth, vertices.length / 3);
        buildPlane(0, 2, 1, 1, -1, width, depth, -depth, vertices.length / 3);
        buildPlane(0, 1, 2, 1, -1, width, height, depth, vertices.length / 3);
        buildPlane(0, 1, 2, -1, -1, width, height, -depth, vertices.length / 3);

        geometry.setAttribute('position', { array: new Float32Array(vertices), itemSize: 3 });
        geometry.setAttribute('normal', { array: new Float32Array(normals), itemSize: 3 });
        geometry.setAttribute('uv', { array: new Float32Array(uvs), itemSize: 2 });
        geometry.setAttribute('index', { array: new Uint16Array(indices), itemSize: 1 });
        geometry.computeBoundingBox();
        geometry.computeBoundingSphere();

        return geometry;
    }

    static createPlane(width = 1, height = 1, widthSegments = 1, heightSegments = 1) {
        return Geometry.createBox(width, height, 0, widthSegments, heightSegments, 1);
    }

    static createSphere(radius = 1, widthSegments = 32, heightSegments = 16) {
        const geometry = new Geometry();
        const vertices = [];
        const normals = [];
        const uvs = [];
        const indices = [];

        for (let y = 0; y <= heightSegments; y++) {
            const v = y / heightSegments;
            const phi = v * Math.PI;

            for (let x = 0; x <= widthSegments; x++) {
                const u = x / widthSegments;
                const theta = u * Math.PI * 2;

                const px = -radius * Math.cos(theta) * Math.sin(phi);
                const py = radius * Math.cos(phi);
                const pz = radius * Math.sin(theta) * Math.sin(phi);

                vertices.push(px, py, pz);
                normals.push(px / radius, py / radius, pz / radius);
                uvs.push(u, v);
            }
        }

        for (let y = 0; y < heightSegments; y++) {
            for (let x = 0; x < widthSegments; x++) {
                const a = y * (widthSegments + 1) + x;
                const b = a + widthSegments + 1;
                const c = a + 1;
                const d = b + 1;

                if (y !== 0) indices.push(a, b, c);
                if (y !== heightSegments - 1) indices.push(b, d, c);
            }
        }

        geometry.setAttribute('position', { array: new Float32Array(vertices), itemSize: 3 });
        geometry.setAttribute('normal', { array: new Float32Array(normals), itemSize: 3 });
        geometry.setAttribute('uv', { array: new Float32Array(uvs), itemSize: 2 });
        geometry.setAttribute('index', { array: new Uint16Array(indices), itemSize: 1 });
        geometry.computeBoundingSphere();

        return geometry;
    }
}

Geometry.nextId = 0;

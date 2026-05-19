export class SoundClip {
    constructor(id, name, path, properties = {}) {
        this.id = id;
        this.name = name;
        this.path = path;
        this.duration = properties.duration || 0;
        this.format = properties.format || 'wav';
        this.sampleRate = properties.sampleRate || 44100;
        this.channels = properties.channels || 2;
        this.bitRate = properties.bitRate || 16;
        this.fileSize = properties.fileSize || 0;
        this.isCompressed = properties.isCompressed || false;
        this.compressionType = properties.compressionType || null;
        this.metadata = properties.metadata || {};
        this.tags = properties.tags || [];
        this.category = properties.category || 'misc';
        this.is3D = properties.is3D !== undefined ? properties.is3D : false;
        this.isStreaming = properties.isStreaming || false;
        this.isLooping = properties.isLooping || false;
        this.volume = properties.volume || 1.0;
        this.pitch = properties.pitch || 1.0;
        this.spatialBlend = properties.spatialBlend || 0;
        this.dopplerLevel = properties.dopplerLevel || 1.0;
        this.minDistance = properties.minDistance || 1;
        this.maxDistance = properties.maxDistance || 500;
        this.rolloffMode = properties.rolloffMode || 'inverse';
        this.rolloffFactor = properties.rolloffFactor || 1;
        this.panLevel = properties.panLevel || 0;
        this.spread = properties.spread || 0;
        this.loadState = 'unloaded';
        this.lastPlayed = 0;
        this.playCount = 0;
    }

    load() {
        if (this.loadState === 'loaded') return Promise.resolve();
        this.loadState = 'loading';
        return new Promise((resolve) => {
            setTimeout(() => {
                this.loadState = 'loaded';
                resolve();
            }, 10);
        });
    }

    unload() {
        this.loadState = 'unloaded';
    }

    isLoaded() {
        return this.loadState === 'loaded';
    }

    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
    }

    setPitch(pitch) {
        this.pitch = Math.max(0.5, Math.min(2.0, pitch));
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            path: this.path,
            duration: this.duration,
            format: this.format,
            category: this.category,
            is3D: this.is3D,
            volume: this.volume,
            pitch: this.pitch
        };
    }
}

export class SoundEmitter {
    constructor(position = { x: 0, y: 0, z: 0 }) {
        this.id = SoundEmitter.nextId++;
        this.position = { ...position };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.clip = null;
        this.isPlaying = false;
        this.isPaused = false;
        this.isLooping = false;
        this.volume = 1.0;
        this.pitch = 1.0;
        this.panStereo = 0;
        this.spatialBlend = 1.0;
        this.dopplerLevel = 1.0;
        this.minDistance = 1;
        this.maxDistance = 500;
        this.rolloffMode = 'inverse';
        this.rolloffFactor = 1;
        this.playbackTime = 0;
        this.startTime = 0;
        this.priority = 0;
        this.audibleDistance = 0;
        this.occlusion = 0;
        this.obstruction = 0;
        this.isVirtualized = false;
        this.fadeTarget = null;
        this.fadeDuration = 0;
        this.fadeStartTime = 0;
        this.onEndCallback = null;
        this.tags = [];
    }

    play(clip, loop = false) {
        this.clip = clip;
        this.isLooping = loop;
        this.isPlaying = true;
        this.isPaused = false;
        this.playbackTime = 0;
        this.startTime = Date.now();
        if (clip) {
            clip.lastPlayed = Date.now();
            clip.playCount++;
        }
    }

    pause() {
        if (this.isPlaying && !this.isPaused) {
            this.isPaused = true;
            this.playbackTime = this.getCurrentTime();
        }
    }

    resume() {
        if (this.isPlaying && this.isPaused) {
            this.isPaused = false;
            this.startTime = Date.now() - (this.playbackTime * 1000);
        }
    }

    stop() {
        this.isPlaying = false;
        this.isPaused = false;
        this.playbackTime = 0;
        if (this.onEndCallback) {
            this.onEndCallback(this);
        }
    }

    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
    }

    setPitch(pitch) {
        this.pitch = Math.max(0.5, Math.min(2.0, pitch));
    }

    setPosition(position) {
        this.position = { ...position };
    }

    setVelocity(velocity) {
        this.velocity = { ...velocity };
    }

    getCurrentTime() {
        if (!this.isPlaying) return this.playbackTime;
        if (this.isPaused) return this.playbackTime;
        const elapsed = (Date.now() - this.startTime) / 1000;
        return elapsed * this.pitch;
    }

    setPlaybackTime(time) {
        this.playbackTime = time;
        if (this.isPlaying) {
            this.startTime = Date.now() - (time * 1000);
        }
    }

    getEffectiveVolume(listenerPosition, listenerDirection) {
        if (this.spatialBlend === 0) {
            return this.volume * (this.fadeTarget !== null ? this.getFadeFactor() : 1);
        }

        const distance = this.calculateDistance(listenerPosition);
        const angle = this.calculateAngle(listenerDirection);
        const distanceFactor = this.calculateDistanceAttenuation(distance);
        const angleFactor = this.calculateAngleAttenuation(angle);

        let effectiveVolume = this.volume * distanceFactor * angleFactor;
        effectiveVolume *= (1 - this.occlusion) * (1 - this.obstruction);
        effectiveVolume *= (this.fadeTarget !== null ? this.getFadeFactor() : 1);

        return Math.max(0, Math.min(1, effectiveVolume));
    }

    calculateDistance(listenerPosition) {
        const dx = this.position.x - listenerPosition.x;
        const dy = this.position.y - listenerPosition.y;
        const dz = this.position.z - listenerPosition.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    calculateAngle(listenerDirection) {
        const dx = this.position.x - listenerDirection.x;
        const dy = this.position.y - listenerDirection.y;
        const angle = Math.atan2(dy, dx);
        return angle;
    }

    calculateDistanceAttenuation(distance) {
        if (distance <= this.minDistance) return 1.0;
        if (distance >= this.maxDistance) return 0.0;

        switch (this.rolloffMode) {
            case 'inverse':
                return this.minDistance / (this.minDistance + this.rolloffFactor * (distance - this.minDistance));
            case 'linear':
                return 1 - ((distance - this.minDistance) / (this.maxDistance - this.minDistance));
            case 'exponential':
                return Math.pow(distance / this.minDistance, -this.rolloffFactor);
            default:
                return this.minDistance / (this.minDistance + this.rolloffFactor * (distance - this.minDistance));
        }
    }

    calculateAngleAttenuation(angle) {
        if (this.spatialBlend <= 0) return 1.0;
        const panAngle = Math.atan2(this.position.x, this.position.y);
        const panValue = Math.sin(panAngle - angle);
        return 1.0 - (Math.abs(panValue) * this.spatialBlend * 0.5);
    }

    fadeTo(targetVolume, duration) {
        this.fadeTarget = targetVolume;
        this.fadeDuration = duration;
        this.fadeStartTime = Date.now();
    }

    getFadeFactor() {
        if (this.fadeTarget === null) return 1.0;
        const elapsed = (Date.now() - this.fadeStartTime) / 1000;
        const t = Math.min(1, elapsed / this.fadeDuration);
        const startVolume = this.volume;
        return startVolume + (this.fadeTarget - startVolume) * t;
    }

    onEnd(callback) {
        this.onEndCallback = callback;
    }

    update(deltaTime, listenerPosition, listenerDirection) {
        if (!this.isPlaying || this.isPaused) return;

        this.playbackTime += deltaTime * this.pitch;

        if (this.clip && this.playbackTime >= this.clip.duration && !this.isLooping) {
            this.stop();
        }

        this.audibleDistance = this.calculateDistance(listenerPosition);
        this.isVirtualized = this.audibleDistance > this.maxDistance;

        if (this.fadeTarget !== null && this.playbackTime >= this.fadeDuration) {
            this.volume = this.fadeTarget;
            this.fadeTarget = null;
        }
    }

    serialize() {
        return {
            id: this.id,
            position: this.position,
            clipId: this.clip ? this.clip.id : null,
            isPlaying: this.isPlaying,
            isLooping: this.isLooping,
            volume: this.volume,
            pitch: this.pitch,
            playbackTime: this.playbackTime
        };
    }
}
SoundEmitter.nextId = 0;

export class SoundCategory {
    constructor(name, parent = null) {
        this.name = name;
        this.parent = parent;
        this.volume = 1.0;
        this.isMuted = false;
        this.priority = 0;
        this.children = new Map();
        this.clips = new Map();
        if (parent) {
            parent.addChild(this);
        }
    }

    addChild(child) {
        this.children.set(child.name, child);
    }

    addClip(clip) {
        this.clips.set(clip.id, clip);
    }

    getClip(clipId) {
        return this.clips.get(clipId);
    }

    removeClip(clipId) {
        this.clips.delete(clipId);
    }

    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
    }

    mute() {
        this.isMuted = true;
    }

    unmute() {
        this.isMuted = false;
    }

    toggle() {
        this.isMuted = !this.isMuted;
    }

    getEffectiveVolume() {
        let volume = this.volume;
        if (this.isMuted) return 0;
        if (this.parent) {
            volume *= this.parent.getEffectiveVolume();
        }
        return volume;
    }

    getAllClips() {
        let clips = Array.from(this.clips.values());
        for (const child of this.children.values()) {
            clips = clips.concat(child.getAllClips());
        }
        return clips;
    }
}

export class AudioListener {
    constructor() {
        this.position = { x: 0, y: 0, z: 0 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.forward = { x: 0, y: 1, z: 0 };
        this.up = { x: 0, y: 0, z: 1 };
        this.innerAngle = 360;
        this.outerAngle = 360;
        this.rolloffFactor = 1.0;
        this.distanceNear = 1;
        this.distanceFar = 1000;
        this.isActive = true;
    }

    setPosition(position) {
        this.position = { ...position };
    }

    setOrientation(forward, up) {
        this.forward = { ...forward };
        this.up = { ...up };
    }

    setVelocity(velocity) {
        this.velocity = { ...velocity };
    }

    getDirection() {
        return { ...this.forward };
    }

    update(deltaTime) {}
}

export class SoundMixer {
    constructor() {
        this.masterVolume = 1.0;
        this.categories = new Map();
        this.effectsVolume = 1.0;
        this.musicVolume = 1.0;
        this.voiceVolume = 1.0;
        this.ambientVolume = 1.0;
        this.uiVolume = 1.0;
        this.isMuted = false;
        this.createDefaultCategories();
    }

    createDefaultCategories() {
        const master = new SoundCategory('Master', null);
        this.categories.set('Master', master);

        const effects = new SoundCategory('Effects', master);
        this.categories.set('Effects', effects);

        const music = new SoundCategory('Music', master);
        this.categories.set('Music', music);

        const voice = new SoundCategory('Voice', master);
        this.categories.set('Voice', voice);

        const ambient = new SoundCategory('Ambient', master);
        this.categories.set('Ambient', ambient);

        const ui = new SoundCategory('UI', master);
        this.categories.set('UI', ui);

        const weapons = new SoundCategory('Weapons', effects);
        this.categories.set('Weapons', weapons);

        const footsteps = new SoundCategory('Footsteps', effects);
        this.categories.set('Footsteps', footsteps);

        const explosions = new SoundCategory('Explosions', effects);
        this.categories.set('Explosions', explosions);
    }

    getCategory(name) {
        return this.categories.get(name);
    }

    setMasterVolume(volume) {
        this.masterVolume = Math.max(0, Math.min(1, volume));
    }

    getMasterVolume() {
        return this.masterVolume;
    }

    setCategoryVolume(categoryName, volume) {
        const category = this.categories.get(categoryName);
        if (category) {
            category.setVolume(volume);
        }
    }

    muteCategory(categoryName) {
        const category = this.categories.get(categoryName);
        if (category) {
            category.mute();
        }
    }

    unmuteCategory(categoryName) {
        const category = this.categories.get(categoryName);
        if (category) {
            category.unmute();
        }
    }

    mute() {
        this.isMuted = true;
    }

    unmute() {
        this.isMuted = false;
    }

    toggleMute() {
        this.isMuted = !this.isMuted;
    }

    getEffectiveVolume(categoryName) {
        if (this.isMuted) return 0;
        const category = this.categories.get(categoryName);
        if (!category) return this.masterVolume;
        return this.masterVolume * category.getEffectiveVolume();
    }

    serialize() {
        const categoryData = {};
        for (const [name, category] of this.categories) {
            categoryData[name] = {
                volume: category.volume,
                isMuted: category.isMuted
            };
        }
        return {
            masterVolume: this.masterVolume,
            isMuted: this.isMuted,
            categories: categoryData
        };
    }
}

export class SoundBank {
    constructor(id, name) {
        this.id = id;
        this.name = name;
        this.clips = new Map();
        this.events = new Map();
        this.rtpcs = new Map();
        this.switches = new Map();
        this.totalSize = 0;
        this.clipCount = 0;
        this.isLoaded = false;
    }

    addClip(clip) {
        this.clips.set(clip.id, clip);
        this.clipCount++;
        this.totalSize += clip.fileSize;
    }

    getClip(clipId) {
        return this.clips.get(clipId);
    }

    getClipsByCategory(category) {
        return Array.from(this.clips.values()).filter(c => c.category === category);
    }

    getClipsByTag(tag) {
        return Array.from(this.clips.values()).filter(c => c.tags.includes(tag));
    }

    removeClip(clipId) {
        const clip = this.clips.get(clipId);
        if (clip) {
            this.totalSize -= clip.fileSize;
            this.clipCount--;
        }
        this.clips.delete(clipId);
    }

    async load() {
        const loadPromises = Array.from(this.clips.values()).map(clip => clip.load());
        await Promise.all(loadPromises);
        this.isLoaded = true;
    }

    unload() {
        for (const clip of this.clips.values()) {
            clip.unload();
        }
        this.isLoaded = false;
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            clipCount: this.clipCount,
            totalSize: this.totalSize,
            isLoaded: this.isLoaded
        };
    }
}

export class RTPC {
    constructor(name, parameterType = 'float') {
        this.name = name;
        this.parameterType = parameterType;
        this.defaultValue = 0;
        this.minValue = 0;
        this.maxValue = 100;
        this.value = this.defaultValue;
        this.gameObjects = new Map();
    }

    setValue(value, gameObjectId = null) {
        this.value = Math.max(this.minValue, Math.min(this.maxValue, value));
        if (gameObjectId) {
            this.gameObjects.set(gameObjectId, this.value);
        }
    }

    getValue(gameObjectId = null) {
        if (gameObjectId && this.gameObjects.has(gameObjectId)) {
            return this.gameObjects.get(gameObjectId);
        }
        return this.value;
    }

    reset() {
        this.value = this.defaultValue;
        this.gameObjects.clear();
    }
}

export class SoundSwitch {
    constructor(name, defaultGroup) {
        this.name = name;
        this.currentGroup = defaultGroup;
        this.groups = new Map();
        this.defaultGroup = defaultGroup;
    }

    addGroup(groupName) {
        this.groups.set(groupName, new Set());
    }

    setGroup(groupName, gameObjectId = null) {
        if (!this.groups.has(groupName)) return;
        
        if (gameObjectId) {
            const oldGroup = this.getGroupForObject(gameObjectId);
            if (oldGroup) {
                this.groups.get(oldGroup).delete(gameObjectId);
            }
            this.groups.get(groupName).add(gameObjectId);
        }
        
        this.currentGroup = groupName;
    }

    getGroupForObject(gameObjectId) {
        for (const [groupName, objects] of this.groups) {
            if (objects.has(gameObjectId)) {
                return groupName;
            }
        }
        return this.defaultGroup;
    }
}

export class AudioBus {
    constructor(name) {
        this.name = name;
        this.parent = null;
        this.children = new Map();
        this.volume = 1.0;
        this.pitch = 1.0;
        this.pan = 0;
        this.isMuted = false;
        this.isSolo = false;
        this.rtpcs = new Map();
        this.switches = new Map();
    }

    addChild(bus) {
        this.children.set(bus.name, bus);
        bus.parent = this;
    }

    removeChild(busName) {
        const bus = this.children.get(busName);
        if (bus) {
            bus.parent = null;
            this.children.delete(busName);
        }
    }

    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
    }

    setPitch(pitch) {
        this.pitch = Math.max(0.5, Math.min(2.0, pitch));
    }

    setPan(pan) {
        this.pan = Math.max(-1, Math.min(1, pan));
    }

    mute() {
        this.isMuted = true;
    }

    unmute() {
        this.isMuted = false;
    }

    solo() {
        this.isSolo = true;
        if (this.parent) {
            this.parent.solo();
        }
    }

    unsolo() {
        this.isSolo = false;
        for (const child of this.children.values()) {
            child.unsolo();
        }
    }

    getEffectiveVolume() {
        if (this.isMuted) return 0;
        if (this.isSolo && this.parent && !this.parent.isSolo) {
            return this.volume;
        }
        let volume = this.volume;
        if (this.parent) {
            volume *= this.parent.getEffectiveVolume();
        }
        return volume;
    }

    getRTPC(name) {
        if (this.rtpcs.has(name)) {
            return this.rtpcs.get(name);
        }
        if (this.parent) {
            return this.parent.getRTPC(name);
        }
        return null;
    }

    setRTPC(name, value) {
        if (!this.rtpcs.has(name)) {
            this.rtpcs.set(name, new RTPC(name));
        }
        this.rtpcs.get(name).setValue(value);
    }

    getSwitch(name) {
        if (this.switches.has(name)) {
            return this.switches.get(name);
        }
        if (this.parent) {
            return this.parent.getSwitch(name);
        }
        return null;
    }

    setSwitch(name, groupName) {
        if (!this.switches.has(name)) {
            this.switches.set(name, new SoundSwitch(name, groupName));
        }
        this.switches.get(name).setGroup(groupName);
    }
}

export class SoundManager {
    constructor() {
        this.clips = new Map();
        this.emitters = new Map();
        this.categories = new Map();
        this.mixer = new SoundMixer();
        this.listener = new AudioListener();
        this.banks = new Map();
        this.audioBuses = new Map();
        this.activeEmitterCount = 0;
        this.maxEmitters = 64;
        this.isInitialized = false;
        this.isPaused = false;
        this.updateInterval = 1000 / 60;
        this.lastUpdateTime = 0;
        this.isVirtualizationEnabled = true;
        this.isOcclusionEnabled = true;
        this.dopplerEnabled = true;
        this.createDefaultBuses();
    }

    createDefaultBuses() {
        const masterBus = new AudioBus('Master');
        this.audioBuses.set('Master', masterBus);

        const sfxBus = new AudioBus('SFX');
        sfxBus.parent = masterBus;
        masterBus.children.set('SFX', sfxBus);
        this.audioBuses.set('SFX', sfxBus);

        const musicBus = new AudioBus('Music');
        musicBus.parent = masterBus;
        masterBus.children.set('Music', musicBus);
        this.audioBuses.set('Music', musicBus);

        const voiceBus = new AudioBus('Voice');
        voiceBus.parent = masterBus;
        masterBus.children.set('Voice', voiceBus);
        this.audioBuses.set('Voice', voiceBus);
    }

    initialize() {
        this.isInitialized = true;
        this.mixer = new SoundMixer();
    }

    registerClip(clip) {
        this.clips.set(clip.id, clip);
        const category = this.mixer.getCategory(clip.category);
        if (category) {
            category.addClip(clip);
        }
    }

    getClip(clipId) {
        return this.clips.get(clipId);
    }

    loadBank(bank) {
        return bank.load();
    }

    loadBankByName(bankName) {
        const bank = this.banks.get(bankName);
        if (bank) {
            return this.loadBank(bank);
        }
        return Promise.reject(new Error(`Bank not found: ${bankName}`));
    }

    createEmitter(position = { x: 0, y: 0, z: 0 }) {
        const emitter = new SoundEmitter(position);
        this.emitters.set(emitter.id, emitter);
        this.activeEmitterCount++;
        return emitter;
    }

    removeEmitter(emitterId) {
        if (this.emitters.has(emitterId)) {
            this.emitters.delete(emitterId);
            this.activeEmitterCount--;
        }
    }

    getEmitter(emitterId) {
        return this.emitters.get(emitterId);
    }

    play(clipId, position = null, loop = false) {
        const clip = this.clips.get(clipId);
        if (!clip || !clip.isLoaded()) return null;

        const emitter = this.createEmitter(position || { x: 0, y: 0, z: 0 });
        emitter.clip = clip;
        emitter.volume = clip.volume;
        emitter.pitch = clip.pitch;
        emitter.play(clip, loop);
        return emitter;
    }

    playOneShot(clipId, position, volumeScale = 1.0) {
        const clip = this.clips.get(clipId);
        if (!clip || !clip.isLoaded()) return null;

        const emitter = this.createEmitter(position);
        emitter.clip = clip;
        emitter.volume = clip.volume * volumeScale;
        emitter.pitch = clip.pitch;
        emitter.isLooping = false;
        emitter.play(clip, false);
        emitter.onEnd(() => this.removeEmitter(emitter.id));
        return emitter;
    }

    setListener(position, forward, up) {
        this.listener.setPosition(position);
        this.listener.setOrientation(forward, up);
    }

    update(deltaTime) {
        if (this.isPaused || !this.isInitialized) return;

        for (const emitter of this.emitters.values()) {
            if (emitter.isPlaying && !emitter.isPaused) {
                emitter.update(deltaTime, this.listener.position, this.listener.forward);

                const effectiveVolume = emitter.getEffectiveVolume(
                    this.listener.position,
                    this.listener.forward
                );

                if (this.isVirtualizationEnabled && emitter.isVirtualized) {
                    effectiveVolume *= 0.1;
                }
            }
        }
    }

    pause() {
        this.isPaused = true;
        for (const emitter of this.emitters.values()) {
            emitter.pause();
        }
    }

    resume() {
        this.isPaused = false;
        for (const emitter of this.emitters.values()) {
            emitter.resume();
        }
    }

    stopAll() {
        for (const emitter of this.emitters.values()) {
            emitter.stop();
        }
    }

    stopCategory(categoryName) {
        for (const emitter of this.emitters.values()) {
            if (emitter.clip && emitter.clip.category === categoryName) {
                emitter.stop();
            }
        }
    }

    setMasterVolume(volume) {
        this.mixer.setMasterVolume(volume);
    }

    getMasterVolume() {
        return this.mixer.getMasterVolume();
    }

    mute() {
        this.mixer.mute();
    }

    unmute() {
        this.mixer.unmute();
    }

    toggleMute() {
        this.mixer.toggleMute();
    }

    isMuted() {
        return this.mixer.isMuted;
    }

    setOcclusionEnabled(enabled) {
        this.isOcclusionEnabled = enabled;
    }

    setDopplerEnabled(enabled) {
        this.dopplerEnabled = enabled;
    }

    setVirtualizationEnabled(enabled) {
        this.isVirtualizationEnabled = enabled;
    }

    getActiveEmitterCount() {
        return this.activeEmitterCount;
    }

    getMaxEmitters() {
        return this.maxEmitters;
    }

    getClipsByCategory(category) {
        return Array.from(this.clips.values()).filter(c => c.category === category);
    }

    getClipsByTag(tag) {
        return Array.from(this.clips.values()).filter(c => c.tags.includes(tag));
    }

    serialize() {
        return {
            clips: Array.from(this.clips.values()).map(c => c.serialize()),
            mixer: this.mixer.serialize(),
            emitterCount: this.activeEmitterCount,
            isPaused: this.isPaused
        };
    }
}

export class FootstepSoundGenerator {
    constructor(soundManager) {
        this.soundManager = soundManager;
        this.lastFootstepTime = 0;
        this.footstepInterval = 0.4;
        this.materialSounds = new Map();
        this.createMaterialMappings();
    }

    createMaterialMappings() {
        this.materialSounds.set('concrete', { left: 'footstep_concrete_left', right: 'footstep_concrete_right' });
        this.materialSounds.set('metal', { left: 'footstep_metal_left', right: 'footstep_metal_right' });
        this.materialSounds.set('dirt', { left: 'footstep_dirt_left', right: 'footstep_dirt_right' });
        this.materialSounds.set('grass', { left: 'footstep_grass_left', right: 'footstep_grass_right' });
        this.materialSounds.set('wood', { left: 'footstep_wood_left', right: 'footstep_wood_right' });
        this.materialSounds.set('water', { left: 'footstep_water_left', right: 'footstep_water_right' });
        this.materialSounds.set('glass', { left: 'footstep_glass_left', right: 'footstep_glass_right' });
        this.materialSounds.set('tile', { left: 'footstep_tile_left', right: 'footstep_tile_right' });
        this.materialSounds.set('sand', { left: 'footstep_sand_left', right: 'footstep_sand_right' });
        this.materialSounds.set('snow', { left: 'footstep_snow_left', right: 'footstep_snow_right' });
    }

    playFootstep(position, material, isLeftFoot = true, speed = 1.0) {
        const now = Date.now();
        if (now - this.lastFootstepTime < this.footstepInterval / speed) return;

        const sounds = this.materialSounds.get(material);
        if (!sounds) return;

        const clipId = isLeftFoot ? sounds.left : sounds.right;
        this.soundManager.playOneShot(clipId, position, 0.5 * speed);
        this.lastFootstepTime = now;
    }

    setFootstepInterval(interval) {
        this.footstepInterval = Math.max(0.1, interval);
    }

    setMaterialSounds(material, leftClipId, rightClipId) {
        this.materialSounds.set(material, { left: leftClipId, right: rightClipId });
    }
}

export class WeaponSoundGenerator {
    constructor(soundManager) {
        this.soundManager = soundManager;
        this.weaponSounds = new Map();
        this.createDefaultWeaponSounds();
    }

    createDefaultWeaponSounds() {
        const weapons = [
            'ak47', 'm4a1', 'm4a4', 'awp', 'deagle', 'glock', 'usp', 'p250',
            'famas', 'galilar', 'aug', 'sg556', 'scar20', 'g3sg1', 'awp',
            'ak47', 'm4a1', 'ak47', 'm4a4'
        ];
        
        weapons.forEach(weapon => {
            this.weaponSounds.set(`${weapon}_fire`, `${weapon}_fire_single`);
            this.weaponSounds.set(`${weapon}_fire_last`, `${weapon}_fire_last`);
            this.weaponSounds.set(`${weapon}_reload`, `${weapon}_reload`);
            this.weaponSounds.set(`${weapon}_draw`, `${weapon}_draw`);
            this.weaponSounds.set(`${weapon}_empty`, `${weapon}_empty`);
        });
    }

    playFireSound(weaponId, position, isLastShot = false) {
        const clipId = isLastShot 
            ? this.weaponSounds.get(`${weaponId}_fire_last`)
            : this.weaponSounds.get(`${weaponId}_fire`);
        
        if (clipId) {
            this.soundManager.playOneShot(clipId, position, 0.8);
        }
    }

    playReloadSound(weaponId, position) {
        const clipId = this.weaponSounds.get(`${weaponId}_reload`);
        if (clipId) {
            this.soundManager.playOneShot(clipId, position, 0.7);
        }
    }

    playDrawSound(weaponId, position) {
        const clipId = this.weaponSounds.get(`${weaponId}_draw`);
        if (clipId) {
            this.soundManager.playOneShot(clipId, position, 0.5);
        }
    }

    playEmptySound(weaponId, position) {
        const clipId = this.weaponSounds.get(`${weaponId}_empty`);
        if (clipId) {
            this.soundManager.playOneShot(clipId, position, 0.3);
        }
    }

    setWeaponSound(weaponId, action, clipId) {
        this.weaponSounds.set(`${weaponId}_${action}`, clipId);
    }
}

export class AmbientSoundSystem {
    constructor(soundManager) {
        this.soundManager = soundManager;
        this.zones = new Map();
        this.activeZones = new Set();
        this.transitionTime = 2.0;
        this.ambientSounds = new Map();
        this.createDefaultAmbients();
    }

    createDefaultAmbients() {
        this.ambientSounds.set('wind', { loop: 'ambient_wind_loop', volume: 0.3 });
        this.ambientSounds.set('rain', { loop: 'ambient_rain_loop', volume: 0.4 });
        this.ambientSounds.set('city', { loop: 'ambient_city_loop', volume: 0.2 });
        this.ambientSounds.set('nature', { loop: 'ambient_nature_loop', volume: 0.2 });
        this.ambientSounds.set('dungeon', { loop: 'ambient_dungeon_loop', volume: 0.3 });
        this.ambientSounds.set('battle', { loop: 'ambient_battle_loop', volume: 0.5 });
    }

    createZone(id, bounds, ambientType, mixType = 'blend') {
        this.zones.set(id, {
            id,
            bounds,
            ambientType,
            mixType,
            emitters: new Set(),
            intensity: 1.0,
            isActive: false
        });
    }

    activateZone(zoneId) {
        const zone = this.zones.get(zoneId);
        if (!zone || zone.isActive) return;

        const ambient = this.ambientSounds.get(zone.ambientType);
        if (!ambient) return;

        zone.isActive = true;
        this.activeZones.add(zoneId);

        const emitter = this.soundManager.createEmitter(this.getZoneCenter(zone.bounds));
        emitter.clip = this.soundManager.getClip(ambient.loop);
        emitter.volume = 0;
        emitter.isLooping = true;
        emitter.play(emitter.clip, true);
        emitter.fadeTo(ambient.volume * zone.intensity, this.transitionTime);
        zone.emitters.add(emitter.id);
    }

    deactivateZone(zoneId) {
        const zone = this.zones.get(zoneId);
        if (!zone || !zone.isActive) return;

        for (const emitterId of zone.emitters) {
            const emitter = this.soundManager.getEmitter(emitterId);
            if (emitter) {
                emitter.fadeTo(0, this.transitionTime);
                setTimeout(() => {
                    emitter.stop();
                    this.soundManager.removeEmitter(emitterId);
                }, this.transitionTime * 1000);
            }
        }

        zone.emitters.clear();
        zone.isActive = false;
        this.activeZones.delete(zoneId);
    }

    setZoneIntensity(zoneId, intensity) {
        const zone = this.zones.get(zoneId);
        if (!zone) return;

        zone.intensity = Math.max(0, Math.min(1, intensity));

        for (const emitterId of zone.emitters) {
            const emitter = this.soundManager.getEmitter(emitterId);
            if (emitter) {
                const ambient = this.ambientSounds.get(zone.ambientType);
                emitter.fadeTo(ambient.volume * intensity, 0.5);
            }
        }
    }

    getZoneCenter(bounds) {
        return {
            x: (bounds.minX + bounds.maxX) / 2,
            y: (bounds.minY + bounds.maxY) / 2,
            z: (bounds.minZ + bounds.maxZ) / 2
        };
    }

    update(listenerPosition) {
        for (const [zoneId, zone] of this.zones) {
            if (this.isPositionInBounds(listenerPosition, zone.bounds)) {
                if (!zone.isActive) {
                    this.activateZone(zoneId);
                }
            } else {
                if (zone.isActive) {
                    this.deactivateZone(zoneId);
                }
            }
        }
    }

    isPositionInBounds(position, bounds) {
        return position.x >= bounds.minX && position.x <= bounds.maxX &&
               position.y >= bounds.minY && position.y <= bounds.maxY &&
               position.z >= bounds.minZ && position.z <= bounds.maxZ;
    }
}

export default SoundManager;

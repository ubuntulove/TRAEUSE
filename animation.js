export class AnimationState {
    constructor(name, properties = {}) {
        this.name = name;
        this.properties = {
            duration: properties.duration || 0,
            loop: properties.loop || false,
            speed: properties.speed || 1.0,
            blendIn: properties.blendIn || 0.2,
            blendOut: properties.blendOut || 0.2,
            priority: properties.priority || 0,
            interruptible: properties.interruptible !== false,
            rootMotion: properties.rootMotion || false,
            mirror: properties.mirror || false,
            layer: properties.layer || 0
        };
        this.events = [];
        this.transitions = [];
        this.onEnterCallbacks = [];
        this.onExitCallbacks = [];
        this.onUpdateCallbacks = [];
        this.isActive = false;
        this.elapsedTime = 0;
        this.progress = 0;
    }

    addTransition(toState, condition = null, duration = 0.3) {
        this.transitions.push({
            toState: toState instanceof AnimationState ? toState.name : toState,
            condition,
            duration,
            minDuration: duration
        });
    }

    addEvent(time, callback, data = {}) {
        this.events.push({ time, callback, data, fired: false });
    }

    onEnter(callback) {
        this.onEnterCallbacks.push(callback);
    }

    onExit(callback) {
        this.onExitCallbacks.push(callback);
    }

    onUpdate(callback) {
        this.onUpdateCallbacks.push(callback);
    }

    enter() {
        this.isActive = true;
        this.elapsedTime = 0;
        this.progress = 0;
        this.events.forEach(e => e.fired = false);
        this.onEnterCallbacks.forEach(cb => cb(this));
    }

    exit() {
        this.isActive = false;
        this.onExitCallbacks.forEach(cb => cb(this));
    }

    update(deltaTime, blendWeight = 1.0) {
        if (!this.isActive) return;

        this.elapsedTime += deltaTime * this.properties.speed;
        this.progress = this.properties.duration > 0 
            ? Math.min(1.0, this.elapsedTime / this.properties.duration) 
            : 1.0;

        for (const event of this.events) {
            if (!event.fired && this.progress >= event.time) {
                event.fired = true;
                event.callback(event.data);
            }
        }

        this.onUpdateCallbacks.forEach(cb => cb(this, deltaTime, blendWeight));
    }

    canTransitionTo(stateName) {
        if (!this.properties.interruptible && this.elapsedTime < this.properties.duration * 0.8) {
            return false;
        }
        return true;
    }

    shouldTransition() {
        if (!this.properties.loop && this.progress >= 1.0) {
            return { canTransition: true, reason: 'completed' };
        }
        return { canTransition: false };
    }

    serialize() {
        return {
            name: this.name,
            properties: this.properties,
            events: this.events.map(e => ({ time: e.time, data: e.data })),
            transitions: this.transitions.map(t => ({
                toState: t.toState,
                duration: t.duration
            }))
        };
    }
}

export class BlendTree {
    constructor(name, type = 'direct') {
        this.name = name;
        this.type = type;
        this.children = [];
        this.blendParameter = '';
        this.minThreshold = 0;
        this.maxThreshold = 1;
        this.syncIndex = -1;
        this.syncBitField = 0;
    }

    addChild(state, threshold = 0, timeScale = 1.0, blendWeight = 1.0) {
        this.children.push({
            state: state instanceof AnimationState ? state.name : state,
            threshold,
            timeScale,
            blendWeight,
            motion: state
        });
    }

    removeChild(stateName) {
        this.children = this.children.filter(c => c.state !== stateName);
    }

    getBlendedState(parameterValue) {
        if (this.children.length === 0) return null;
        if (this.children.length === 1) return this.children[0];

        let lowerChild = this.children[0];
        let upperChild = this.children[0];

        for (const child of this.children) {
            if (child.threshold <= parameterValue) {
                lowerChild = child;
            }
            if (child.threshold >= parameterValue && upperChild.threshold === this.children[0].threshold) {
                upperChild = child;
            }
        }

        if (lowerChild === upperChild) {
            return { state: lowerChild.state, weight: 1.0, timeScale: lowerChild.timeScale };
        }

        const range = upperChild.threshold - lowerChild.threshold;
        const t = range > 0 ? (parameterValue - lowerChild.threshold) / range : 0;
        const smoothT = this.smoothT(t);

        return {
            state: upperChild.state,
            lowerState: lowerChild.state,
            weight: smoothT,
            lowerWeight: 1 - smoothT,
            timeScale: lowerChild.timeScale + (upperChild.timeScale - lowerChild.timeScale) * smoothT
        };
    }

    smoothT(t) {
        return t * t * (3 - 2 * t);
    }

    update(deltaTime, parameterValue) {
        const blended = this.getBlendedState(parameterValue);
        if (!blended) return null;

        if (blended.state && blended.weight === 1.0) {
            return { state: blended.state, weight: 1.0, timeScale: blended.timeScale };
        }

        return blended;
    }

    getAllStates() {
        return this.children.map(c => c.state);
    }

    serialize() {
        return {
            name: this.name,
            type: this.type,
            children: this.children.map(c => ({
                state: c.state,
                threshold: c.threshold,
                timeScale: c.timeScale,
                blendWeight: c.blendWeight
            }))
        };
    }
}

export class AnimationLayer {
    constructor(name, weight = 1.0, blendingMode = 'additive') {
        this.name = name;
        this.weight = weight;
        this.blendingMode = blendingMode;
        this.states = [];
        this.blendTrees = [];
        this.currentState = null;
        this.previousState = null;
        this.blendFactor = 1.0;
        this.isAdditive = blendingMode === 'additive';
    }

    addState(state) {
        this.states.push(state);
    }

    addBlendTree(tree) {
        this.blendTrees.push(tree);
    }

    setState(stateName) {
        const state = this.states.find(s => s.name === stateName);
        if (!state) return false;

        if (this.currentState && this.currentState !== state) {
            this.currentState.exit();
            this.previousState = this.currentState;
            this.blendFactor = 0;
        }

        this.currentState = state;
        state.enter();
        return true;
    }

    transitionTo(stateName, blendDuration = 0.2) {
        const state = this.states.find(s => s.name === stateName);
        if (!state) return false;

        this.previousState = this.currentState;
        this.blendFactor = 0;
        this.blendDuration = blendDuration;
        this.targetState = state;
        return true;
    }

    update(deltaTime) {
        if (this.blendFactor < 1.0 && this.previousState) {
            this.blendFactor = Math.min(1.0, this.blendFactor + deltaTime / this.blendDuration);
            if (this.blendFactor >= 1.0) {
                if (this.previousState) {
                    this.previousState.exit();
                }
                this.previousState = null;
            }
        }

        if (this.currentState) {
            const weight = this.isAdditive ? this.weight : this.weight * this.blendFactor;
            this.currentState.update(deltaTime, weight);
        }

        for (const tree of this.blendTrees) {
            tree.update(deltaTime, 0);
        }
    }

    getCurrentStateName() {
        return this.currentState ? this.currentState.name : null;
    }

    getCurrentProgress() {
        return this.currentState ? this.currentState.progress : 0;
    }

    serialize() {
        return {
            name: this.name,
            weight: this.weight,
            blendingMode: this.blendingMode,
            states: this.states.map(s => s.serialize()),
            blendTrees: this.blendTrees.map(t => t.serialize()),
            currentState: this.getCurrentStateName()
        };
    }
}

export class AnimationController {
    constructor() {
        this.layers = new Map();
        this.defaultLayer = this.addLayer('Base', 1.0, 'override');
        this.parameters = new Map();
        this.triggers = new Map();
        this.rootMotionPosition = { x: 0, y: 0, z: 0 };
        this.rootMotionRotation = 0;
        this.velocity = { x: 0, y: 0, z: 0 };
        this.angularVelocity = 0;
        this.isRootMotionActive = false;
    }

    addLayer(name, weight = 1.0, blendingMode = 'override') {
        const layer = new AnimationLayer(name, weight, blendingMode);
        this.layers.set(name, layer);
        return layer;
    }

    getLayer(name) {
        return this.layers.get(name);
    }

    getLayerCount() {
        return this.layers.size;
    }

    addParameter(name, type = 'float', defaultValue = 0) {
        this.parameters.set(name, {
            type,
            value: defaultValue,
            defaultValue,
            min: type === 'float' ? -1 : 0,
            max: 1
        });
    }

    setFloat(name, value) {
        const param = this.parameters.get(name);
        if (param && param.type === 'float') {
            param.value = Math.max(param.min, Math.min(param.max, value));
        }
    }

    setBool(name, value) {
        const param = this.parameters.get(name);
        if (param && param.type === 'bool') {
            param.value = value;
        }
    }

    setInt(name, value) {
        const param = this.parameters.get(name);
        if (param && param.type === 'int') {
            param.value = value;
        }
    }

    trigger(name) {
        this.triggers.set(name, true);
    }

    resetTrigger(name) {
        this.triggers.set(name, false);
    }

    getFloat(name) {
        const param = this.parameters.get(name);
        return param ? param.value : 0;
    }

    getBool(name) {
        const param = this.parameters.get(name);
        return param ? param.value : false;
    }

    getInt(name) {
        const param = this.parameters.get(name);
        return param ? param.value : 0;
    }

    isTriggered(name) {
        return this.triggers.get(name) === true;
    }

    update(deltaTime) {
        for (const [, layer] of this.layers) {
            layer.update(deltaTime);
        }

        if (this.isRootMotionActive) {
            this.rootMotionPosition.x += this.velocity.x * deltaTime;
            this.rootMotionPosition.y += this.velocity.y * deltaTime;
            this.rootMotionPosition.z += this.velocity.z * deltaTime;
            this.rootMotionRotation += this.angularVelocity * deltaTime;
        }

        for (const [name] of this.triggers) {
            this.triggers.set(name, false);
        }
    }

    applyRootMotion(position, rotation) {
        this.isRootMotionActive = true;
        this.rootMotionPosition = position;
        this.rootMotionRotation = rotation;
    }

    disableRootMotion() {
        this.isRootMotionActive = false;
    }

    getLayerBlendedState(layerName = 'Base') {
        const layer = this.layers.get(layerName);
        if (!layer) return null;

        if (layer.currentState) {
            return {
                name: layer.currentState.name,
                progress: layer.currentState.progress,
                weight: layer.weight * layer.blendFactor
            };
        }

        return null;
    }

    getAllLayerStates() {
        const states = {};
        for (const [name, layer] of this.layers) {
            states[name] = layer.getCurrentStateName();
        }
        return states;
    }

    serialize() {
        const layerData = {};
        for (const [name, layer] of this.layers) {
            layerData[name] = layer.serialize();
        }

        const paramData = {};
        for (const [name, param] of this.parameters) {
            paramData[name] = { type: param.type, value: param.value };
        }

        return {
            layers: layerData,
            parameters: paramData,
            rootMotion: {
                position: this.rootMotionPosition,
                rotation: this.rootMotionRotation,
                active: this.isRootMotionActive
            }
        };
    }
}

export class AnimationClip {
    constructor(name, duration = 1.0) {
        this.name = name;
        this.duration = duration;
        this.tracks = [];
        this.events = [];
        this.wrapMode = 'clamp';
        this.legacy = false;
        this.locked = false;
        this.properties = {
            loopTime: true,
            loopBlend: false,
            loopBlendPositionY: false,
            loopBlendRotation: true,
            keepQuaternion: false,
            keepOthers: false,
            keepTransform: false,
            keepPosition: false
        };
    }

    addTrack(track) {
        this.tracks.push(track);
    }

    addEvent(time, functionName, data = {}) {
        this.events.push({ time, functionName, data });
    }

    getTrackCount() {
        return this.tracks.length;
    }

    getEventCount() {
        return this.events.length;
    }

    getDuration() {
        return this.duration;
    }

    setWrapMode(mode) {
        this.wrapMode = mode;
    }

    isLooping() {
        return this.properties.loopTime;
    }

    sample(time, entity) {
        const t = this.wrapMode === 'clamp' 
            ? Math.max(0, Math.min(time, this.duration))
            : time % this.duration;

        for (const track of this.tracks) {
            track.sample(t, entity);
        }

        for (const event of this.events) {
            if (t >= event.time) {
                this.onEvent(event.functionName, event.data);
            }
        }
    }

    onEvent(functionName, data) {
        console.log(`Animation event: ${functionName}`, data);
    }

    serialize() {
        return {
            name: this.name,
            duration: this.duration,
            tracks: this.tracks.map(t => t.serialize()),
            events: this.events,
            wrapMode: this.wrapMode,
            properties: this.properties
        };
    }
}

export class AnimationTrack {
    constructor(targetPath, propertyName = '') {
        this.targetPath = targetPath;
        this.propertyName = propertyName;
        this.keyframes = [];
        this.index = 0;
    }

    addKeyframe(time, value, tangentMode = 'linear') {
        this.keyframes.push({ time, value, tangentMode });
        this.keyframes.sort((a, b) => a.time - b.time);
    }

    sample(time, entity) {
        if (this.keyframes.length === 0) return;

        let keyframeIndex = 0;
        for (let i = 0; i < this.keyframes.length - 1; i++) {
            if (time >= this.keyframes[i].time && time < this.keyframes[i + 1].time) {
                keyframeIndex = i;
                break;
            }
        }

        const keyframe = this.keyframes[keyframeIndex];
        const nextKeyframe = this.keyframes[Math.min(keyframeIndex + 1, this.keyframes.length - 1)];

        let t = 0;
        if (nextKeyframe.time !== keyframe.time) {
            t = (time - keyframe.time) / (nextKeyframe.time - keyframe.time);
        }

        const value = this.interpolate(keyframe.value, nextKeyframe.value, t, keyframe.tangentMode);
        this.applyValue(entity, value);
    }

    interpolate(from, to, t, tangentMode) {
        switch (tangentMode) {
            case 'constant':
                return t < 1 ? from : to;
            case 'linear':
                return from + (to - from) * t;
            case 'smooth':
                const smoothT = t * t * (3 - 2 * t);
                return from + (to - from) * smoothT;
            default:
                return from + (to - from) * t;
        }
    }

    applyValue(entity, value) {
        const target = this.getTarget(entity, this.targetPath);
        if (target) {
            target[this.propertyName] = value;
        }
    }

    getTarget(entity, path) {
        const parts = path.split('.');
        let current = entity;
        for (const part of parts) {
            if (current[part] !== undefined) {
                current = current[part];
            } else {
                return null;
            }
        }
        return current;
    }

    serialize() {
        return {
            targetPath: this.targetPath,
            propertyName: this.propertyName,
            keyframes: this.keyframes
        };
    }
}

export class HumanoidAnimationController extends AnimationController {
    constructor() {
        super();
        this.createHumanoidLayers();
        this.createHumanoidParameters();
        this.createHumanoidStates();
    }

    createHumanoidLayers() {
        const baseLayer = this.getLayer('Base');
        baseLayer.blendingMode = 'override';

        const upperLayer = this.addLayer('UpperBody', 1.0, 'additive');
        upperLayer.weight = 1.0;

        const fullBodyLayer = this.addLayer('FullBody', 1.0, 'override');
        fullBodyLayer.weight = 1.0;

        const additiveLayer = this.addLayer('Additive', 0.5, 'additive');
        additiveLayer.weight = 0.5;
    }

    createHumanoidParameters() {
        this.addParameter('Speed', 'float', 0);
        this.addParameter('VerticalSpeed', 'float', 0);
        this.addParameter('HorizontalSpeed', 'float', 0);
        this.addParameter('LookUpDown', 'float', 0);
        this.addParameter('LookLeftRight', 'float', 0);
        this.addParameter('IsMoving', 'bool', false);
        this.addParameter('IsAirborne', 'bool', false);
        this.addParameter('IsCrouching', 'bool', false);
        this.addParameter('IsWalking', 'bool', false);
        this.addParameter('IsRunning', 'bool', false);
        this.addParameter('IsSwimming', 'bool', false);
        this.addParameter('IsGrounded', 'bool', true);
        this.addParameter('IsDead', 'bool', false);
        this.addParameter('WeaponType', 'int', 0);
        this.addParameter('MoveDirection', 'float', 0);
        this.addParameter('TurnDirection', 'float', 0);
    }

    createHumanoidStates() {
        const baseLayer = this.getLayer('Base');
        const upperLayer = this.getLayer('UpperBody');
        const fullBodyLayer = this.getLayer('FullBody');

        this.createMovementStates(fullBodyLayer);
        this.createCombatStates(upperLayer);
        this.createUtilityStates(upperLayer);
    }

    createMovementStates(layer) {
        const idle = new AnimationState('Idle', { duration: 4.0, loop: true });
        const walkForward = new AnimationState('WalkForward', { duration: 0.8, loop: true, speed: 0.8 });
        const walkBackward = new AnimationState('WalkBackward', { duration: 0.8, loop: true, speed: 0.8 });
        const runForward = new AnimationState('RunForward', { duration: 0.5, loop: true, speed: 1.2 });
        const runBackward = new AnimationState('RunBackward', { duration: 0.5, loop: true, speed: 1.2 });
        const crouchIdle = new AnimationState('CrouchIdle', { duration: 4.0, loop: true });
        const crouchWalk = new AnimationState('CrouchWalk', { duration: 0.6, loop: true });
        const jump = new AnimationState('Jump', { duration: 0.5, loop: false });
        const fall = new AnimationState('Fall', { duration: 2.0, loop: true });
        const land = new AnimationState('Land', { duration: 0.3, loop: false });
        const sprint = new AnimationState('Sprint', { duration: 0.4, loop: true, speed: 1.4 });
        const slide = new AnimationState('Slide', { duration: 0.6, loop: false });
        const climb = new AnimationState('Climb', { duration: 1.0, loop: false });

        idle.addEvent(0.5, () => this.onIdleBreath());
        idle.addEvent(3.5, () => this.onIdleLookAround());

        walkForward.addEvent(0.4, () => this.onWalkStep());
        walkForward.addEvent(0.8, () => this.onWalkStep());

        jump.addEvent(0.1, () => this.onJumpStart());
        land.addEvent(0.05, () => this.onLand());

        idle.addTransition('WalkForward', () => this.getBool('IsMoving') && this.getFloat('Speed') < 2);
        idle.addTransition('RunForward', () => this.getBool('IsRunning') && this.getFloat('Speed') > 3);
        idle.addTransition('CrouchIdle', () => this.getBool('IsCrouching'));
        idle.addTransition('Jump', () => !this.getBool('IsGrounded'));
        idle.addTransition('Fall', () => !this.getBool('IsGrounded'));

        walkForward.addTransition('Idle', () => !this.getBool('IsMoving'));
        walkForward.addTransition('RunForward', () => this.getBool('IsRunning'));
        walkForward.addTransition('CrouchWalk', () => this.getBool('IsCrouching'));
        walkForward.addTransition('Jump', () => !this.getBool('IsGrounded'));

        runForward.addTransition('Idle', () => !this.getBool('IsMoving'));
        runForward.addTransition('WalkForward', () => !this.getBool('IsRunning'));
        runForward.addTransition('Jump', () => !this.getBool('IsGrounded'));

        fall.addTransition('Land', () => this.getBool('IsGrounded'));

        [idle, walkForward, walkBackward, runForward, runBackward, crouchIdle, crouchWalk, jump, fall, land, sprint, slide, climb]
            .forEach(state => layer.addState(state));
    }

    createCombatStates(layer) {
        const idleAim = new AnimationState('IdleAim', { duration: 2.0, loop: true, priority: 5 });
        const fire = new AnimationState('Fire', { duration: 0.1, loop: true });
        const fireBurst = new AnimationState('FireBurst', { duration: 0.2, loop: false });
        const reload = new AnimationState('Reload', { duration: 2.0, loop: false, priority: 10 });
        const equipWeapon = new AnimationState('EquipWeapon', { duration: 0.5, loop: false });
        const meleeAttack = new AnimationState('MeleeAttack', { duration: 0.4, loop: false });
        const knifeAttack = new AnimationState('KnifeAttack', { duration: 0.3, loop: false });
        const grenadeThrow = new AnimationState('GrenadeThrow', { duration: 1.0, loop: false, priority: 15 });
        const plantDefuse = new AnimationState('PlantDefuse', { duration: 5.0, loop: false, priority: 15 });
        const hostagePickup = new AnimationState('HostagePickup', { duration: 0.5, loop: false });

        reload.addEvent(1.8, () => this.onReloadComplete());
        equipWeapon.addEvent(0.4, () => this.onWeaponEquipped());
        grenadeThrow.addEvent(0.7, () => this.onGrenadeRelease());
        plantDefuse.addEvent(4.5, () => this.onDefuseComplete());

        idleAim.addTransition('Fire', () => this.isTriggered('Fire'));
        idleAim.addTransition('Reload', () => this.isTriggered('Reload'));
        idleAim.addTransition('GrenadeThrow', () => this.isTriggered('Grenade'));
        idleAim.addTransition('PlantDefuse', () => this.isTriggered('Interact'));

        fire.addTransition('IdleAim', () => !this.isTriggered('Fire'));
        fire.addTransition('Reload', () => this.isTriggered('Reload'));

        [idleAim, fire, fireBurst, reload, equipWeapon, meleeAttack, knifeAttack, grenadeThrow, plantDefuse, hostagePickup]
            .forEach(state => layer.addState(state));
    }

    createUtilityStates(layer) {
        const useWorld = new AnimationState('UseWorld', { duration: 0.5, loop: false });
        const openDoor = new AnimationState('OpenDoor', { duration: 0.6, loop: false });
        const climbLadder = new AnimationState('ClimbLadder', { duration: 1.5, loop: true });
        const fallLadder = new AnimationState('FallLadder', { duration: 0.5, loop: false });
        const enterVehicle = new AnimationState('EnterVehicle', { duration: 0.8, loop: false });
        const exitVehicle = new AnimationState('ExitVehicle', { duration: 0.8, loop: false });
        const death = new AnimationState('Death', { duration: 3.0, loop: false, priority: 100 });
        const respawn = new AnimationState('Respawn', { duration: 1.0, loop: false });
        const hurt = new AnimationState('Hurt', { duration: 0.5, loop: false, priority: 20 });
        const blind = new AnimationState('Blind', { duration: 3.0, loop: true });

        death.addEvent(2.5, () => this.onDeathComplete());
        respawn.addEvent(0.9, () => this.onRespawnComplete());
        blind.addEvent(2.5, () => this.onBlindRecover());

        [useWorld, openDoor, climbLadder, fallLadder, enterVehicle, exitVehicle, death, respawn, hurt, blind]
            .forEach(state => layer.addState(state));
    }

    onIdleBreath() {}
    onIdleLookAround() {}
    onWalkStep() {}
    onJumpStart() {}
    onLand() {}
    onReloadComplete() {}
    onWeaponEquipped() {}
    onGrenadeRelease() {}
    onDefuseComplete() {}
    onDeathComplete() {}
    onRespawnComplete() {}
    onBlindRecover() {}

    updateMovement(speed, isGrounded, isCrouching, isRunning) {
        this.setFloat('Speed', speed);
        this.setBool('IsMoving', speed > 0.5);
        this.setBool('IsGrounded', isGrounded);
        this.setBool('IsCrouching', isCrouching);
        this.setBool('IsRunning', isRunning);

        if (!isGrounded) {
            this.setState('Fall');
        } else if (isCrouching) {
            if (speed > 0.5) {
                this.setState('CrouchWalk');
            } else {
                this.setState('CrouchIdle');
            }
        } else if (speed > 3 && isRunning) {
            this.setState('RunForward');
        } else if (speed > 0.5) {
            this.setState('WalkForward');
        } else {
            this.setState('Idle');
        }
    }

    setState(stateName) {
        const baseLayer = this.getLayer('Base');
        if (baseLayer) {
            baseLayer.setState(stateName);
        }
    }

    triggerFire() {
        this.trigger('Fire');
    }

    triggerReload() {
        this.trigger('Reload');
    }

    triggerGrenade() {
        this.trigger('Grenade');
    }

    triggerInteract() {
        this.trigger('Interact');
    }

    kill() {
        this.setBool('IsDead', true);
        this.setState('Death');
    }
}

export class AnimationBlender {
    constructor() {
        this.blendQueue = [];
        this.currentBlend = null;
        this.blendTime = 0;
        this.blendDuration = 0;
        this.isBlending = false;
    }

    blendTo(newState, duration = 0.2) {
        this.blendQueue.push({ state: newState, duration });
        if (!this.isBlending) {
            this.processNextBlend();
        }
    }

    processNextBlend() {
        if (this.blendQueue.length === 0) {
            this.isBlending = false;
            return;
        }

        const blend = this.blendQueue.shift();
        this.currentBlend = blend;
        this.blendTime = 0;
        this.blendDuration = blend.duration;
        this.isBlending = true;
    }

    update(deltaTime) {
        if (!this.isBlending) return;

        this.blendTime += deltaTime;
        if (this.blendTime >= this.blendDuration) {
            this.processNextBlend();
        }
    }

    getBlendFactor() {
        if (!this.isBlending) return 1.0;
        return Math.min(1.0, this.blendTime / this.blendDuration);
    }

    cancelBlend() {
        this.blendQueue = [];
        this.currentBlend = null;
        this.isBlending = false;
    }
}

export class AnimationManager {
    constructor() {
        this.controllers = new Map();
        this.clips = new Map();
        this.blenders = new Map();
        this.blendTrees = new Map();
        this.defaultController = null;
        this.updateQueue = [];
        this.playbackSpeed = 1.0;
        this.isPaused = false;
    }

    registerController(id, controller) {
        this.controllers.set(id, controller);
        this.blenders.set(id, new AnimationBlender());
        if (!this.defaultController) {
            this.defaultController = id;
        }
    }

    getController(id = null) {
        return this.controllers.get(id || this.defaultController);
    }

    registerClip(id, clip) {
        this.clips.set(id, clip);
    }

    getClip(id) {
        return this.clips.get(id);
    }

    createHumanoidController() {
        const controller = new HumanoidAnimationController();
        this.registerController('humanoid', controller);
        return controller;
    }

    play(controllerId, stateName, crossFadeDuration = 0.2) {
        const controller = this.getController(controllerId);
        if (!controller) return false;

        const layer = controller.getLayer('Base');
        if (layer) {
            layer.transitionTo(stateName, crossFadeDuration);
        }
        return true;
    }

    setFloat(controllerId, parameterName, value) {
        const controller = this.getController(controllerId);
        if (controller) {
            controller.setFloat(parameterName, value);
        }
    }

    setBool(controllerId, parameterName, value) {
        const controller = this.getController(controllerId);
        if (controller) {
            controller.setBool(parameterName, value);
        }
    }

    setTrigger(controllerId, triggerName) {
        const controller = this.getController(controllerId);
        if (controller) {
            controller.trigger(triggerName);
        }
    }

    update(deltaTime) {
        if (this.isPaused) return;

        const scaledDelta = deltaTime * this.playbackSpeed;

        for (const [id, controller] of this.controllers) {
            controller.update(scaledDelta);

            const blender = this.blenders.get(id);
            if (blender) {
                blender.update(scaledDelta);
            }
        }

        for (const clip of this.clips.values()) {
            clip.sample(clip.lastSampleTime || 0, null);
        }
    }

    pause() {
        this.isPaused = true;
    }

    resume() {
        this.isPaused = false;
    }

    setPlaybackSpeed(speed) {
        this.playbackSpeed = speed;
    }

    getPlaybackSpeed() {
        return this.playbackSpeed;
    }

    serializeAll() {
        const data = {};
        for (const [id, controller] of this.controllers) {
            data[id] = controller.serialize();
        }
        return data;
    }

    deserializeAll(data) {
        for (const [id, controllerData] of data) {
            const controller = this.controllers.get(id);
            if (controller) {
                Object.assign(controller, controllerData);
            }
        }
    }
}

export default AnimationManager;

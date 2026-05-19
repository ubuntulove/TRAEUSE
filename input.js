export class InputManager {
    constructor() {
        this.keys = new Map();
        this.mouseButtons = new Map();
        this.mousePosition = { x: 0, y: 0 };
        this.mouseDelta = { x: 0, y: 0 };
        this.mouseWheelDelta = 0;
        this.touches = new Map();
        this.gamepads = [];
        this.activeGamepad = null;
        this.keyBindings = new Map();
        this.actionBindings = new Map();
        this.defaultBindings();
        this.enabled = true;
        this.lockedPointer = false;
        this.eventListeners = {
            keydown: [],
            keyup: [],
            mousedown: [],
            mouseup: [],
            mousemove: [],
            mousewheel: [],
            touchstart: [],
            touchend: [],
            touchmove: [],
            gamepadconnected: [],
            gamepaddisconnected: []
        };
        this.inputContext = 'game';
        this.setupEventListeners();
    }

    defaultBindings() {
        this.bindKey('KeyW', 'forward');
        this.bindKey('KeyS', 'backward');
        this.bindKey('KeyA', 'left');
        this.bindKey('KeyD', 'right');
        this.bindKey('Space', 'jump');
        this.bindKey('ShiftLeft', 'sprint');
        this.bindKey('ControlLeft', 'crouch');
        this.bindKey('KeyE', 'interact');
        this.bindKey('KeyR', 'reload');
        this.bindKey('KeyQ', 'useAbility1');
        this.bindKey('KeyF', 'useAbility2');
        this.bindKey('KeyG', 'grenade');
        this.bindKey('KeyT', 'useItem');
        this.bindKey('Digit1', 'weapon1');
        this.bindKey('Digit2', 'weapon2');
        this.bindKey('Digit3', 'weapon3');
        this.bindKey('Digit4', 'weapon4');
        this.bindKey('Digit5', 'weapon5');
        this.bindKey('Tab', 'scoreboard');
        this.bindKey('Escape', 'menu');
        this.bindKey('KeyB', 'buyMenu');
        this.bindKey('KeyM', 'radioMessage');
        this.bindKey('KeyN', 'toggleNetGraph');
        this.bindKey('KeyC', 'voicePushToTalk');
        this.bindKey('KeyV', 'viewChange');
        this.bindKey('KeyH', 'holsterWeapon');
        this.bindKey('KeyK', 'killCam');
        this.bindKey('KeyP', 'pause');
        this.bindKey('Comma', 'prevWeapon');
        this.bindKey('Period', 'nextWeapon');

        this.bindMouseButton(0, 'attack');
        this.bindMouseButton(1, 'attackAlt');
        this.bindMouseButton(2, 'reload');
        this.bindMouseButton(3, 'use');
        this.bindMouseButton(4, 'grenade');

        this.bindGamepadButton(0, 'attack');
        this.bindGamepadButton(1, 'attackAlt');
        this.bindGamepadButton(2, 'jump');
        this.bindGamepadButton(3, 'reload');
        this.bindGamepadButton(4, 'prevWeapon');
        this.bindGamepadButton(5, 'nextWeapon');
        this.bindGamepadButton(6, 'crouch');
        this.bindGamepadButton(7, 'sprint');
        this.bindGamepadButton(8, 'interact');
        this.bindGamepadButton(9, 'menu');
        this.bindGamepadButton(10, 'useAbility1');

        this.bindGamepadAxis(0, 'moveX');
        this.bindGamepadAxis(1, 'moveY');
        this.bindGamepadAxis(2, 'lookX');
        this.bindGamepadAxis(3, 'lookY');
    }

    bindKey(code, action) {
        this.keyBindings.set(code, action);
    }

    bindMouseButton(button, action) {
        this.actionBindings.set(`mouse_${button}`, action);
    }

    bindGamepadButton(button, action) {
        this.actionBindings.set(`gamepad_${button}`, action);
    }

    bindGamepadAxis(axis, action) {
        this.actionBindings.set(`gamepad_axis_${axis}`, action);
    }

    setupEventListeners() {
        window.addEventListener('keydown', (e) => this.onKeyDown(e));
        window.addEventListener('keyup', (e) => this.onKeyUp(e));
        window.addEventListener('mousedown', (e) => this.onMouseDown(e));
        window.addEventListener('mouseup', (e) => this.onMouseUp(e));
        window.addEventListener('mousemove', (e) => this.onMouseMove(e));
        window.addEventListener('wheel', (e) => this.onMouseWheel(e));
        window.addEventListener('contextmenu', (e) => e.preventDefault());
        window.addEventListener('touchstart', (e) => this.onTouchStart(e));
        window.addEventListener('touchend', (e) => this.onTouchEnd(e));
        window.addEventListener('touchmove', (e) => this.onTouchMove(e));
        window.addEventListener('gamepadconnected', (e) => this.onGamepadConnected(e));
        window.addEventListener('gamepaddisconnected', (e) => this.onGamepadDisconnected(e));

        this.startGamepadPolling();
    }

    onKeyDown(e) {
        if (!this.enabled) return;

        const code = e.code;
        if (!this.keys.get(code)) {
            this.keys.set(code, true);
            this.emit('keydown', { code, key: e.key, action: this.keyBindings.get(code) });
            this.dispatchAction(this.keyBindings.get(code), true);
        }

        if (code === 'Escape' || code === 'Tab') {
            e.preventDefault();
        }
    }

    onKeyUp(e) {
        if (!this.enabled) return;

        const code = e.code;
        this.keys.set(code, false);
        this.emit('keyup', { code, key: e.key, action: this.keyBindings.get(code) });
        this.dispatchAction(this.keyBindings.get(code), false);
    }

    onMouseDown(e) {
        if (!this.enabled) return;

        const button = e.button;
        this.mouseButtons.set(button, true);
        this.emit('mousedown', { button, action: this.actionBindings.get(`mouse_${button}`) });
        this.dispatchAction(this.actionBindings.get(`mouse_${button}`), true);
    }

    onMouseUp(e) {
        if (!this.enabled) return;

        const button = e.button;
        this.mouseButtons.set(button, false);
        this.emit('mouseup', { button, action: this.actionBindings.get(`mouse_${button}`) });
        this.dispatchAction(this.actionBindings.get(`mouse_${button}`), false);
    }

    onMouseMove(e) {
        if (!this.enabled) return;

        const newX = e.clientX;
        const newY = e.clientY;

        this.mouseDelta.x = newX - this.mousePosition.x;
        this.mouseDelta.y = newY - this.mousePosition.y;

        this.mousePosition.x = newX;
        this.mousePosition.y = newY;

        this.emit('mousemove', {
            x: this.mousePosition.x,
            y: this.mousePosition.y,
            deltaX: this.mouseDelta.x,
            deltaY: this.mouseDelta.y
        });
    }

    onMouseWheel(e) {
        if (!this.enabled) return;

        this.mouseWheelDelta = e.deltaY;
        this.emit('mousewheel', { delta: this.mouseWheelDelta });
    }

    onTouchStart(e) {
        if (!this.enabled) return;

        for (const touch of e.changedTouches) {
            this.touches.set(touch.identifier, {
                id: touch.identifier,
                x: touch.clientX,
                y: touch.clientY,
                startX: touch.clientX,
                startY: touch.clientY
            });
        }
        this.emit('touchstart', { touches: Array.from(this.touches.values()) });
    }

    onTouchEnd(e) {
        if (!this.enabled) return;

        for (const touch of e.changedTouches) {
            this.touches.delete(touch.identifier);
        }
        this.emit('touchend', { touches: Array.from(this.touches.values()) });
    }

    onTouchMove(e) {
        if (!this.enabled) return;

        for (const touch of e.changedTouches) {
            const existing = this.touches.get(touch.identifier);
            if (existing) {
                existing.x = touch.clientX;
                existing.y = touch.clientY;
                existing.deltaX = touch.clientX - existing.startX;
                existing.deltaY = touch.clientY - existing.startY;
            }
        }
        this.emit('touchmove', { touches: Array.from(this.touches.values()) });
    }

    onGamepadConnected(e) {
        const gamepad = e.gamepad;
        this.gamepads.set(gamepad.index, gamepad);
        this.activeGamepad = gamepad.index;
        this.emit('gamepadconnected', { gamepad });
    }

    onGamepadDisconnected(e) {
        const gamepad = e.gamepad;
        this.gamepads.delete(gamepad.index);
        if (this.activeGamepad === gamepad.index) {
            this.activeGamepad = this.gamepads.size > 0 ? this.gamepads.keys().next().value : null;
        }
        this.emit('gamepaddisconnected', { gamepad });
    }

    startGamepadPolling() {
        const poll = () => {
            const gamepads = navigator.getGamepads();
            for (const gamepad of gamepads) {
                if (gamepad) {
                    this.gamepads.set(gamepad.index, gamepad);

                    for (let i = 0; i < gamepad.buttons.length; i++) {
                        const button = gamepad.buttons[i];
                        const action = this.actionBindings.get(`gamepad_${i}`);
                        if (button.pressed) {
                            this.dispatchAction(action, true);
                        } else if (button.value !== 0) {
                            this.dispatchAction(action, button.value);
                        }
                    }

                    for (let i = 0; i < gamepad.axes.length; i++) {
                        const axis = gamepad.axes[i];
                        const action = this.actionBindings.get(`gamepad_axis_${i}`);
                        if (Math.abs(axis) > 0.1) {
                            this.dispatchAction(action, axis);
                        }
                    }
                }
            }
            requestAnimationFrame(poll);
        };
        poll();
    }

    dispatchAction(action, state) {
        if (!action) return;
        const eventName = `action_${action}`;
        const callbacks = this.eventListeners[eventName];
        if (callbacks) {
            for (const callback of callbacks) {
                callback(state);
            }
        }
    }

    isKeyDown(code) {
        return this.keys.get(code) === true;
    }

    isKeyPressed(code) {
        if (this.keys.get(code)) {
            this.keys.set(code, false);
            return true;
        }
        return false;
    }

    isMouseButtonDown(button) {
        return this.mouseButtons.get(button) === true;
    }

    isMouseButtonPressed(button) {
        if (this.mouseButtons.get(button)) {
            this.mouseButtons.set(button, false);
            return true;
        }
        return false;
    }

    getActionState(action) {
        for (const [code, boundAction] of this.keyBindings) {
            if (boundAction === action) {
                return this.keys.get(code);
            }
        }
        for (const [button, boundAction] of this.actionBindings) {
            if (boundAction === action && button.startsWith('mouse_')) {
                const btnNum = parseInt(button.split('_')[1]);
                return this.mouseButtons.get(btnNum);
            }
        }
        return false;
    }

    getAxis(axisName) {
        let value = 0;

        switch (axisName) {
            case 'horizontal':
                if (this.isKeyDown('KeyA') || this.isKeyDown('ArrowLeft')) value -= 1;
                if (this.isKeyDown('KeyD') || this.isKeyDown('ArrowRight')) value += 1;
                break;
            case 'vertical':
                if (this.isKeyDown('KeyW') || this.isKeyDown('ArrowUp')) value -= 1;
                if (this.isKeyDown('KeyS') || this.isKeyDown('ArrowDown')) value += 1;
                break;
            case 'mouseX':
                value = this.mouseDelta.x;
                break;
            case 'mouseY':
                value = this.mouseDelta.y;
                break;
            case 'lookX':
                if (this.activeGamepad !== null) {
                    const gamepad = this.gamepads.get(this.activeGamepad);
                    if (gamepad) {
                        value = gamepad.axes[2] || 0;
                    }
                }
                break;
            case 'lookY':
                if (this.activeGamepad !== null) {
                    const gamepad = this.gamepads.get(this.activeGamepad);
                    if (gamepad) {
                        value = gamepad.axes[3] || 0;
                    }
                }
                break;
        }

        return Math.max(-1, Math.min(1, value));
    }

    getVector(axisX, axisY) {
        const x = this.getAxis(axisX);
        const y = this.getAxis(axisY);
        const length = Math.sqrt(x * x + y * y);
        if (length > 1) {
            return { x: x / length, y: y / length };
        }
        return { x, y };
    }

    on(event, callback) {
        if (this.eventListeners[event]) {
            this.eventListeners[event].push(callback);
        }
    }

    off(event, callback) {
        if (this.eventListeners[event]) {
            const index = this.eventListeners[event].indexOf(callback);
            if (index !== -1) {
                this.eventListeners[event].splice(index, 1);
            }
        }
    }

    emit(event, data) {
        if (this.eventListeners[event]) {
            for (const callback of this.eventListeners[event]) {
                callback(data);
            }
        }
    }

    enable() {
        this.enabled = true;
    }

    disable() {
        this.enabled = false;
        this.keys.clear();
        this.mouseButtons.clear();
    }

    lockPointer() {
        document.body.requestPointerLock();
        this.lockedPointer = true;
    }

    unlockPointer() {
        document.exitPointerLock();
        this.lockedPointer = false;
    }

    resetDelta() {
        this.mouseDelta.x = 0;
        this.mouseDelta.y = 0;
        this.mouseWheelDelta = 0;
    }

    saveBindings() {
        const bindings = {
            keys: Array.from(this.keyBindings.entries()),
            actions: Array.from(this.actionBindings.entries())
        };
        localStorage.setItem('inputBindings', JSON.stringify(bindings));
    }

    loadBindings() {
        const saved = localStorage.getItem('inputBindings');
        if (saved) {
            try {
                const bindings = JSON.parse(saved);
                this.keyBindings = new Map(bindings.keys);
                this.actionBindings = new Map(bindings.actions);
            } catch (e) {
                console.warn('Failed to load input bindings');
            }
        }
    }

    resetToDefaults() {
        this.keyBindings.clear();
        this.actionBindings.clear();
        this.defaultBindings();
        this.saveBindings();
    }
}

export class KeyBindings {
    constructor() {
        this.bindings = new Map();
        this.categories = {
            movement: ['forward', 'backward', 'left', 'right', 'jump', 'sprint', 'crouch'],
            combat: ['attack', 'attackAlt', 'reload', 'use', 'grenade'],
            interaction: ['interact', 'useItem', 'useAbility1', 'useAbility2'],
            inventory: ['weapon1', 'weapon2', 'weapon3', 'weapon4', 'weapon5', 'prevWeapon', 'nextWeapon', 'holsterWeapon'],
            communication: ['voicePushToTalk', 'radioMessage'],
            ui: ['menu', 'scoreboard', 'buyMenu', 'toggleNetGraph', 'viewChange'],
            system: ['pause', 'killCam']
        };
    }

    setBinding(action, keyCode, inputType = 'keyboard') {
        const binding = {
            action,
            keyCode,
            inputType,
            timestamp: Date.now()
        };
        this.bindings.set(action, binding);
    }

    getBinding(action) {
        return this.bindings.get(action);
    }

    removeBinding(action) {
        this.bindings.delete(action);
    }

    clearBindings() {
        this.bindings.clear();
    }

    getBindingsByCategory(category) {
        const actions = this.categories[category] || [];
        const bindings = [];
        for (const action of actions) {
            const binding = this.bindings.get(action);
            if (binding) {
                bindings.push(binding);
            }
        }
        return bindings;
    }

    exportBindings() {
        return JSON.stringify(Array.from(this.bindings.entries()));
    }

    importBindings(json) {
        try {
            const entries = JSON.parse(json);
            this.bindings = new Map(entries);
            return true;
        } catch (e) {
            return false;
        }
    }
}

export class InputAction {
    constructor(name, config = {}) {
        this.name = name;
        this.description = config.description || '';
        this.category = config.category || 'general';
        this.defaultKey = config.defaultKey || null;
        this.isAxis = config.isAxis || false;
        this.axisScale = config.axisScale || 1;
        this.invertAxis = config.invertAxis || false;
        this.sensitivity = config.sensitivity || 1;
        this.deadzone = config.deadzone || 0;
        this.state = false;
        this.value = 0;
        this.justPressed = false;
        this.justReleased = false;
        this.pressTime = 0;
        this.holdDuration = 0;
        this.toggleState = config.toggleable || false;
        this.isToggle = false;
    }

    press() {
        if (this.toggleState) {
            this.isToggle = !this.isToggle;
            this.state = this.isToggle;
        } else {
            if (!this.state) {
                this.justPressed = true;
                this.pressTime = Date.now();
            }
            this.state = true;
        }
    }

    release() {
        if (!this.toggleState) {
            if (this.state) {
                this.justReleased = true;
                this.holdDuration = Date.now() - this.pressTime;
            }
            this.state = false;
        }
    }

    setValue(value) {
        this.value = value * this.axisScale * (this.invertAxis ? -1 : 1);

        if (Math.abs(this.value) < this.deadzone) {
            this.value = 0;
        }
    }

    update() {
        this.justPressed = false;
        this.justReleased = false;
    }

    reset() {
        this.state = false;
        this.value = 0;
        this.justPressed = false;
        this.justReleased = false;
        this.isToggle = false;
    }
}

export class InputActionSet {
    constructor() {
        this.actions = new Map();
        this.groups = new Map();
    }

    addAction(name, config) {
        const action = new InputAction(name, config);
        this.actions.set(name, action);
        return action;
    }

    getAction(name) {
        return this.actions.get(name);
    }

    getState(actionName) {
        const action = this.actions.get(actionName);
        return action ? action.state : false;
    }

    getValue(actionName) {
        const action = this.actions.get(actionName);
        return action ? action.value : 0;
    }

    isPressed(actionName) {
        const action = this.actions.get(actionName);
        return action ? action.justPressed : false;
    }

    isReleased(actionName) {
        const action = this.actions.get(actionName);
        return action ? action.justReleased : false;
    }

    getHoldDuration(actionName) {
        const action = this.actions.get(actionName);
        return action ? action.holdDuration : 0;
    }

    pressAction(actionName) {
        const action = this.actions.get(actionName);
        if (action) {
            action.press();
        }
    }

    releaseAction(actionName) {
        const action = this.actions.get(actionName);
        if (action) {
            action.release();
        }
    }

    setActionValue(actionName, value) {
        const action = this.actions.get(actionName);
        if (action) {
            action.setValue(value);
        }
    }

    createGroup(name, actionNames) {
        this.groups.set(name, actionNames);
    }

    getGroupState(groupName) {
        const actions = this.groups.get(groupName);
        if (!actions) return false;

        for (const actionName of actions) {
            const action = this.actions.get(actionName);
            if (action && action.state) {
                return true;
            }
        }
        return false;
    }

    getGroupValue(groupName) {
        const actions = this.groups.get(groupName);
        if (!actions) return 0;

        let maxValue = 0;
        for (const actionName of actions) {
            const action = this.actions.get(actionName);
            if (action && Math.abs(action.value) > Math.abs(maxValue)) {
                maxValue = action.value;
            }
        }
        return maxValue;
    }

    update() {
        for (const action of this.actions.values()) {
            action.update();
        }
    }

    reset() {
        for (const action of this.actions.values()) {
            action.reset();
        }
    }

    clear() {
        this.actions.clear();
        this.groups.clear();
    }
}

export class TouchControls {
    constructor() {
        this.joysticks = new Map();
        this.buttons = new Map();
        this.dpad = null;
        this.actionAreas = new Map();
        this.enabled = true;
        this.visible = true;
        this.opacity = 0.7;
        this.touchScale = 1;
        this.joystickSize = 120;
        this.buttonSize = 60;
        this.setupDefaultControls();
    }

    setupDefaultControls() {
        this.createJoystick('move', {
            x: 100,
            y: window.innerHeight - 150,
            size: this.joystickSize
        });

        this.createJoystick('look', {
            x: window.innerWidth - 150,
            y: window.innerHeight - 150,
            size: this.joystickSize
        });

        this.createButton('jump', {
            x: window.innerWidth - 100,
            y: window.innerHeight - 300,
            size: this.buttonSize,
            label: 'JUMP'
        });

        this.createButton('fire', {
            x: window.innerWidth - 80,
            y: window.innerHeight - 100,
            size: this.buttonSize * 1.5,
            label: 'FIRE'
        });

        this.createButton('reload', {
            x: window.innerWidth - 180,
            y: window.innerHeight - 200,
            size: this.buttonSize,
            label: 'R'
        });

        this.createButton('grenade', {
            x: window.innerWidth - 260,
            y: window.innerHeight - 150,
            size: this.buttonSize,
            label: 'G'
        });

        this.createButton('interact', {
            x: 80,
            y: window.innerHeight - 300,
            size: this.buttonSize,
            label: 'E'
        });
    }

    createJoystick(name, config) {
        const joystick = {
            name,
            baseX: config.x,
            baseY: config.y,
            currentX: config.x,
            currentY: config.y,
            size: config.size || this.joystickSize,
            knobSize: config.knobSize || config.size * 0.4 || this.joystickSize * 0.4,
            active: false,
            touchId: null,
            valueX: 0,
            valueY: 0,
            deadzone: 0.1
        };
        this.joysticks.set(name, joystick);
        return joystick;
    }

    createButton(name, config) {
        const button = {
            name,
            x: config.x,
            y: config.y,
            size: config.size || this.buttonSize,
            label: config.label || name,
            active: false,
            touchId: null,
            pressed: false,
            alpha: this.opacity,
            color: config.color || 'rgba(255, 255, 255, 0.5)'
        };
        this.buttons.set(name, button);
        return button;
    }

    createDpad(config) {
        this.dpad = {
            x: config.x || 100,
            y: config.y || window.innerHeight - 200,
            size: config.size || 80,
            segments: [
                { name: 'up', x: 0, y: -1 },
                { name: 'down', x: 0, y: 1 },
                { name: 'left', x: -1, y: 0 },
                { name: 'right', x: 1, y: 0 }
            ],
            activeSegment: null,
            touchId: null
        };
        return this.dpad;
    }

    handleTouchStart(touches) {
        if (!this.enabled) return;

        for (const touch of touches) {
            for (const [name, joystick] of this.joysticks) {
                if (!joystick.active) {
                    const dist = Math.sqrt(
                        Math.pow(touch.clientX - joystick.baseX, 2) +
                        Math.pow(touch.clientY - joystick.baseY, 2)
                    );
                    if (dist < joystick.size) {
                        joystick.active = true;
                        joystick.touchId = touch.identifier;
                        joystick.currentX = touch.clientX;
                        joystick.currentY = touch.clientY;
                        break;
                    }
                }
            }

            for (const [name, button] of this.buttons) {
                if (!button.active) {
                    const dist = Math.sqrt(
                        Math.pow(touch.clientX - button.x, 2) +
                        Math.pow(touch.clientY - button.y, 2)
                    );
                    if (dist < button.size) {
                        button.active = true;
                        button.touchId = touch.identifier;
                        button.pressed = true;
                        button.alpha = 1;
                        break;
                    }
                }
            }

            if (this.dpad && !this.dpad.touchId) {
                const dx = touch.clientX - this.dpad.x;
                const dy = touch.clientY - this.dpad.y;
                const angle = Math.atan2(dy, dx);
                const dist = Math.sqrt(dx * dx + dy * dy);

                if (dist < this.dpad.size) {
                    this.dpad.touchId = touch.identifier;

                    for (const segment of this.dpad.segments) {
                        const segmentAngle = Math.atan2(segment.y, segment.x);
                        let angleDiff = Math.abs(angle - segmentAngle);
                        if (angleDiff > Math.PI) angleDiff = 2 * Math.PI - angleDiff;

                        if (angleDiff < Math.PI / 4 && dist > this.dpad.size * 0.3) {
                            this.dpad.activeSegment = segment;
                            break;
                        }
                    }
                }
            }
        }
    }

    handleTouchMove(touches) {
        if (!this.enabled) return;

        for (const touch of touches) {
            for (const [name, joystick] of this.joysticks) {
                if (joystick.touchId === touch.identifier) {
                    joystick.currentX = touch.clientX;
                    joystick.currentY = touch.clientY;

                    const dx = joystick.currentX - joystick.baseX;
                    const dy = joystick.currentY - joystick.baseY;
                    const dist = Math.sqrt(dx * dx + dy * dy);
                    const maxDist = joystick.size / 2;

                    if (dist > maxDist) {
                        joystick.currentX = joystick.baseX + (dx / dist) * maxDist;
                        joystick.currentY = joystick.baseY + (dy / dist) * maxDist;
                    }

                    joystick.valueX = dx / maxDist;
                    joystick.valueY = dy / maxDist;

                    if (Math.abs(joystick.valueX) < joystick.deadzone) joystick.valueX = 0;
                    if (Math.abs(joystick.valueY) < joystick.deadzone) joystick.valueY = 0;
                    break;
                }
            }
        }
    }

    handleTouchEnd(touches) {
        if (!this.enabled) return;

        for (const touch of touches) {
            for (const [name, joystick] of this.joysticks) {
                if (joystick.touchId === touch.identifier) {
                    joystick.active = false;
                    joystick.touchId = null;
                    joystick.currentX = joystick.baseX;
                    joystick.currentY = joystick.baseY;
                    joystick.valueX = 0;
                    joystick.valueY = 0;
                    break;
                }
            }

            for (const [name, button] of this.buttons) {
                if (button.touchId === touch.identifier) {
                    button.active = false;
                    button.touchId = null;
                    button.pressed = false;
                    button.alpha = this.opacity;
                    break;
                }
            }

            if (this.dpad && this.dpad.touchId === touch.identifier) {
                this.dpad.touchId = null;
                this.dpad.activeSegment = null;
            }
        }
    }

    getJoystickValue(name) {
        const joystick = this.joysticks.get(name);
        if (!joystick) return { x: 0, y: 0 };
        return { x: joystick.valueX, y: joystick.valueY };
    }

    isButtonPressed(name) {
        const button = this.buttons.get(name);
        if (!button) return false;
        if (button.pressed) {
            button.pressed = false;
            return true;
        }
        return false;
    }

    isButtonDown(name) {
        const button = this.buttons.get(name);
        return button ? button.active : false;
    }

    getDpadValue() {
        if (!this.dpad || !this.dpad.activeSegment) {
            return { x: 0, y: 0 };
        }
        return { x: this.dpad.activeSegment.x, y: this.dpad.activeSegment.y };
    }

    render(ctx) {
        if (!this.visible || !ctx) return;

        ctx.globalAlpha = this.opacity;

        for (const joystick of this.joysticks.values()) {
            ctx.beginPath();
            ctx.arc(joystick.baseX, joystick.baseY, joystick.size / 2, 0, Math.PI * 2);
            ctx.fillStyle = 'rgba(255, 255, 255, 0.2)';
            ctx.fill();
            ctx.strokeStyle = 'rgba(255, 255, 255, 0.5)';
            ctx.lineWidth = 2;
            ctx.stroke();

            ctx.beginPath();
            ctx.arc(joystick.currentX, joystick.currentY, joystick.knobSize / 2, 0, Math.PI * 2);
            ctx.fillStyle = joystick.active ? 'rgba(255, 255, 255, 0.8)' : 'rgba(255, 255, 255, 0.5)';
            ctx.fill();
        }

        for (const button of this.buttons.values()) {
            ctx.globalAlpha = button.alpha;
            ctx.beginPath();
            ctx.arc(button.x, button.y, button.size / 2, 0, Math.PI * 2);
            ctx.fillStyle = button.color;
            ctx.fill();
            ctx.strokeStyle = 'rgba(255, 255, 255, 0.8)';
            ctx.lineWidth = 2;
            ctx.stroke();

            ctx.fillStyle = 'white';
            ctx.font = 'bold 14px Arial';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(button.label, button.x, button.y);
        }

        if (this.dpad) {
            ctx.globalAlpha = this.opacity;
            const dp = this.dpad;

            ctx.fillStyle = 'rgba(255, 255, 255, 0.2)';
            for (const segment of dp.segments) {
                const x = dp.x + segment.x * dp.size * 0.6;
                const y = dp.y + segment.y * dp.size * 0.6;

                ctx.beginPath();
                ctx.arc(x, y, dp.size * 0.35, 0, Math.PI * 2);
                ctx.fill();
            }

            ctx.fillStyle = dp.activeSegment ? 'rgba(255, 255, 255, 0.8)' : 'rgba(255, 255, 255, 0.5)';
            if (dp.activeSegment) {
                const ax = dp.x + dp.activeSegment.x * dp.size * 0.6;
                const ay = dp.y + dp.activeSegment.y * dp.size * 0.6;
                ctx.beginPath();
                ctx.arc(ax, ay, dp.size * 0.35, 0, Math.PI * 2);
                ctx.fill();
            }
        }

        ctx.globalAlpha = 1;
    }

    setOpacity(opacity) {
        this.opacity = opacity;
    }

    setScale(scale) {
        this.touchScale = scale;
    }

    show() {
        this.visible = true;
    }

    hide() {
        this.visible = false;
    }

    enable() {
        this.enabled = true;
    }

    disable() {
        this.enabled = false;
    }

    resize() {
        for (const joystick of this.joysticks.values()) {
            joystick.baseX = joystick.baseX;
            joystick.baseY = joystick.baseY;
            joystick.currentX = joystick.baseX;
            joystick.currentY = joystick.baseY;
        }

        for (const button of this.buttons.values()) {
            button.x = button.x;
            button.y = button.y;
        }

        if (this.dpad) {
            this.dpad.x = this.dpad.x;
            this.dpad.y = this.dpad.y;
        }
    }
}

export class InputRecorder {
    constructor(inputManager) {
        this.inputManager = inputManager;
        this.recordings = new Map();
        this.currentRecording = null;
        this.isRecording = false;
        this.isPlaying = false;
        this.currentFrame = 0;
        this.playbackSpeed = 1;
        this.onPlaybackAction = null;
    }

    startRecording(name) {
        if (this.isRecording) {
            this.stopRecording();
        }

        this.currentRecording = {
            name,
            frames: [],
            startTime: Date.now(),
            duration: 0
        };
        this.isRecording = true;
        this.currentFrame = 0;
    }

    stopRecording() {
        if (!this.isRecording) return;

        this.isRecording = false;
        if (this.currentRecording) {
            this.currentRecording.duration = Date.now() - this.currentRecording.startTime;
            this.recordings.set(this.currentRecording.name, this.currentRecording);
        }
        this.currentRecording = null;
    }

    recordFrame() {
        if (!this.isRecording || !this.currentRecording) return;

        const frame = {
            timestamp: Date.now() - this.currentRecording.startTime,
            keys: new Map(this.inputManager.keys),
            mouseButtons: new Map(this.inputManager.mouseButtons),
            mousePosition: { ...this.inputManager.mousePosition },
            actions: {}
        };

        this.currentRecording.frames.push(frame);
    }

    playRecording(name, config = {}) {
        const recording = this.recordings.get(name);
        if (!recording) return false;

        this.isPlaying = true;
        this.currentFrame = 0;
        this.playbackSpeed = config.speed || 1;
        this.onPlaybackAction = config.onAction || null;

        const startTime = Date.now();
        const playbackLoop = () => {
            if (!this.isPlaying) return;

            const elapsed = (Date.now() - startTime) * this.playbackSpeed;
            const frameIndex = Math.floor(elapsed / (1000 / 60));

            if (frameIndex >= recording.frames.length) {
                this.isPlaying = false;
                if (config.loop) {
                    this.playRecording(name, config);
                }
                return;
            }

            const frame = recording.frames[frameIndex];
            if (frame) {
                this.applyFrame(frame);
            }

            this.currentFrame = frameIndex;
            requestAnimationFrame(playbackLoop);
        };

        playbackLoop();
        return true;
    }

    stopPlayback() {
        this.isPlaying = false;
        this.currentFrame = 0;
    }

    applyFrame(frame) {
        if (this.onPlaybackAction) {
            for (const [action, state] of frame.actions) {
                this.onPlaybackAction(action, state);
            }
        }
    }

    saveRecording(name) {
        const recording = this.recordings.get(name);
        if (!recording) return null;

        const serialized = {
            name: recording.name,
            duration: recording.duration,
            frames: recording.frames.map(f => ({
                timestamp: f.timestamp,
                keys: Array.from(f.keys.entries()),
                mouseButtons: Array.from(f.mouseButtons.entries()),
                mousePosition: f.mousePosition,
                actions: f.actions
            }))
        };

        return JSON.stringify(serialized);
    }

    loadRecording(json) {
        try {
            const data = JSON.parse(json);
            const recording = {
                name: data.name,
                frames: data.frames.map(f => ({
                    timestamp: f.timestamp,
                    keys: new Map(f.keys),
                    mouseButtons: new Map(f.mouseButtons),
                    mousePosition: f.mousePosition,
                    actions: f.actions
                })),
                startTime: Date.now(),
                duration: data.duration
            };
            this.recordings.set(data.name, recording);
            return true;
        } catch (e) {
            return false;
        }
    }

    deleteRecording(name) {
        return this.recordings.delete(name);
    }

    getRecording(name) {
        return this.recordings.get(name);
    }

    getAllRecordings() {
        return Array.from(this.recordings.values()).map(r => ({
            name: r.name,
            duration: r.duration,
            frameCount: r.frames.length
        }));
    }
}

export class CommandInput {
    constructor() {
        this.history = [];
        this.historyIndex = -1;
        this.maxHistory = 50;
        this.autocomplete = new Map();
        this.variables = new Map();
        this.defaultCommands();
    }

    defaultCommands() {
        this.registerCommand('help', () => 'Available commands: help, clear, echo, bind, unbind, set, exec, alias, unalias');
        this.registerCommand('clear', () => {
            this.history = [];
            return '';
        });
        this.registerCommand('echo', (args) => args.join(' '));
        this.registerCommand('version', () => 'CS:GO Clone v1.0.0');
        this.registerCommand('quit', () => {
            window.close();
            return 'Goodbye!';
        });
    }

    registerCommand(name, callback) {
        this.autocomplete.set(name, callback);
    }

    unregisterCommand(name) {
        this.autocomplete.delete(name);
    }

    registerVariable(name, defaultValue, config = {}) {
        this.variables.set(name, {
            value: defaultValue,
            default: defaultValue,
            type: typeof defaultValue,
            min: config.min,
            max: config.max,
            onChange: config.onChange
        });
    }

    setVariable(name, value) {
        const variable = this.variables.get(name);
        if (!variable) return false;

        if (variable.min !== undefined && value < variable.min) {
            value = variable.min;
        }
        if (variable.max !== undefined && value > variable.max) {
            value = variable.max;
        }

        variable.value = value;
        if (variable.onChange) {
            variable.onChange(value);
        }
        return true;
    }

    getVariable(name) {
        const variable = this.variables.get(name);
        return variable ? variable.value : undefined;
    }

    execute(input) {
        if (!input || input.trim() === '') return;

        this.history.push(input);
        if (this.history.length > this.maxHistory) {
            this.history.shift();
        }
        this.historyIndex = this.history.length;

        const parts = this.parseInput(input);
        const command = parts.command.toLowerCase();

        if (this.autocomplete.has(command)) {
            const callback = this.autocomplete.get(command);
            return callback(parts.args);
        }

        if (this.variables.has(command)) {
            if (parts.args.length > 0) {
                this.setVariable(command, parts.args[0]);
                return `${command} = ${this.variables.get(command).value}`;
            } else {
                const v = this.variables.get(command);
                return `${command} = ${v.value} (default: ${v.default})`;
            }
        }

        return `Unknown command: ${command}`;
    }

    parseInput(input) {
        const parts = input.match(/(?:[^\s"]+|"[^"]*")+/g) || [];
        const command = parts[0] || '';
        const args = parts.slice(1).map(arg => {
            if (arg.startsWith('"') && arg.endsWith('"')) {
                return arg.slice(1, -1);
            }
            return arg;
        });
        return { command, args, raw: input };
    }

    getSuggestions(partial) {
        const suggestions = [];
        const lower = partial.toLowerCase();

        for (const command of this.autocomplete.keys()) {
            if (command.toLowerCase().startsWith(lower)) {
                suggestions.push(command);
            }
        }

        for (const variable of this.variables.keys()) {
            if (variable.toLowerCase().startsWith(lower)) {
                suggestions.push(variable);
            }
        }

        return suggestions.sort();
    }

    previousCommand() {
        if (this.historyIndex > 0) {
            this.historyIndex--;
            return this.history[this.historyIndex];
        }
        return null;
    }

    nextCommand() {
        if (this.historyIndex < this.history.length - 1) {
            this.historyIndex++;
            return this.history[this.historyIndex];
        }
        this.historyIndex = this.history.length;
        return '';
    }

    clearHistory() {
        this.history = [];
        this.historyIndex = -1;
    }

    exportSettings() {
        return JSON.stringify({
            variables: Array.from(this.variables.entries()),
            history: this.history
        });
    }

    importSettings(json) {
        try {
            const data = JSON.parse(json);
            for (const [name, v] of data.variables) {
                if (this.variables.has(name)) {
                    this.variables.get(name).value = v.value;
                }
            }
            this.history = data.history || [];
            return true;
        } catch (e) {
            return false;
        }
    }
}

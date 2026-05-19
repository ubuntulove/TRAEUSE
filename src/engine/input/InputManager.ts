import { Vector2D } from '../../entities/Entity';

export interface KeyBinding {
  key: string;
  action: string;
  modifiers?: ('ctrl' | 'alt' | 'shift' | 'meta')[];
}

export interface MouseBinding {
  button: number;
  action: string;
  modifiers?: ('ctrl' | 'alt' | 'shift' | 'meta')[];
}

export interface InputAction {
  name: string;
  bindings: Array<KeyBinding | MouseBinding>;
  active: boolean;
  justPressed: boolean;
  justReleased: boolean;
  pressTime: number;
}

export type InputEventType = 'keydown' | 'keyup' | 'keypress' | 'mousedown' | 'mouseup' | 'mousemove' | 'wheel' | 'contextmenu';

export interface InputEvent {
  type: InputEventType;
  timestamp: number;
  key?: string;
  button?: number;
  x?: number;
  y?: number;
  deltaX?: number;
  deltaY?: number;
  modifiers: {
    ctrl: boolean;
    alt: boolean;
    shift: boolean;
    meta: boolean;
  };
}

export class InputManager {
  private keys: Map<string, boolean> = new Map();
  private previousKeys: Map<string, boolean> = new Map();
  private mouseButtons: Map<number, boolean> = new Map();
  private previousMouseButtons: Map<number, boolean> = new Map();
  private mousePosition: Vector2D = { x: 0, y: 0 };
  private previousMousePosition: Vector2D = { x: 0, y: 0 };
  private mouseDelta: Vector2D = { x: 0, y: 0 };
  private mouseWheelDelta: Vector2D = { x: 0, y: 0 };
  private actions: Map<string, InputAction> = new Map();
  private eventQueue: InputEvent[] = [];
  private listeners: Map<InputEventType, Array<(event: InputEvent) => void>> = new Map();
  private canvas: HTMLCanvasElement | null = null;
  private enabled: boolean = true;
  private touchPoints: Map<number, Vector2D> = new Map();

  constructor() {
    this.setupKeyboardListeners();
    this.setupMouseListeners();
    this.setupTouchListeners();
  }

  private setupKeyboardListeners(): void {
    window.addEventListener('keydown', (e) => {
      if (!this.enabled) return;
      const key = this.getKeyName(e);
      this.keys.set(key, true);
      this.enqueueEvent({
        type: 'keydown',
        timestamp: performance.now(),
        key,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('keydown', {
        type: 'keydown',
        timestamp: performance.now(),
        key,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    });

    window.addEventListener('keyup', (e) => {
      if (!this.enabled) return;
      const key = this.getKeyName(e);
      this.keys.set(key, false);
      this.enqueueEvent({
        type: 'keyup',
        timestamp: performance.now(),
        key,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('keyup', {
        type: 'keyup',
        timestamp: performance.now(),
        key,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    });

    window.addEventListener('keypress', (e) => {
      if (!this.enabled) return;
      this.enqueueEvent({
        type: 'keypress',
        timestamp: performance.now(),
        key: e.key,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('keypress', {
        type: 'keypress',
        timestamp: performance.now(),
        key: e.key,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    });
  }

  private setupMouseListeners(): void {
    window.addEventListener('mousedown', (e) => {
      if (!this.enabled) return;
      this.mouseButtons.set(e.button, true);
      this.enqueueEvent({
        type: 'mousedown',
        timestamp: performance.now(),
        button: e.button,
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('mousedown', {
        type: 'mousedown',
        timestamp: performance.now(),
        button: e.button,
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    });

    window.addEventListener('mouseup', (e) => {
      if (!this.enabled) return;
      this.mouseButtons.set(e.button, false);
      this.enqueueEvent({
        type: 'mouseup',
        timestamp: performance.now(),
        button: e.button,
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('mouseup', {
        type: 'mouseup',
        timestamp: performance.now(),
        button: e.button,
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    });

    window.addEventListener('mousemove', (e) => {
      if (!this.enabled) return;
      this.previousMousePosition = { ...this.mousePosition };
      this.mousePosition = { x: e.clientX, y: e.clientY };
      this.mouseDelta = {
        x: this.mousePosition.x - this.previousMousePosition.x,
        y: this.mousePosition.y - this.previousMousePosition.y,
      };
      this.enqueueEvent({
        type: 'mousemove',
        timestamp: performance.now(),
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('mousemove', {
        type: 'mousemove',
        timestamp: performance.now(),
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    });

    window.addEventListener('wheel', (e) => {
      if (!this.enabled) return;
      e.preventDefault();
      this.mouseWheelDelta = { x: e.deltaX, y: e.deltaY };
      this.enqueueEvent({
        type: 'wheel',
        timestamp: performance.now(),
        deltaX: e.deltaX,
        deltaY: e.deltaY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('wheel', {
        type: 'wheel',
        timestamp: performance.now(),
        deltaX: e.deltaX,
        deltaY: e.deltaY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    }, { passive: false });

    window.addEventListener('contextmenu', (e) => {
      if (!this.enabled) return;
      e.preventDefault();
      this.enqueueEvent({
        type: 'contextmenu',
        timestamp: performance.now(),
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
      this.notifyListeners('contextmenu', {
        type: 'contextmenu',
        timestamp: performance.now(),
        x: e.clientX,
        y: e.clientY,
        modifiers: {
          ctrl: e.ctrlKey,
          alt: e.altKey,
          shift: e.shiftKey,
          meta: e.metaKey,
        },
      });
    });
  }

  private setupTouchListeners(): void {
    window.addEventListener('touchstart', (e) => {
      if (!this.enabled) return;
      for (const touch of e.changedTouches) {
        this.touchPoints.set(touch.identifier, {
          x: touch.clientX,
          y: touch.clientY,
        });
      }
    });

    window.addEventListener('touchmove', (e) => {
      if (!this.enabled) return;
      for (const touch of e.changedTouches) {
        this.touchPoints.set(touch.identifier, {
          x: touch.clientX,
          y: touch.clientY,
        });
      }
    });

    window.addEventListener('touchend', (e) => {
      if (!this.enabled) return;
      for (const touch of e.changedTouches) {
        this.touchPoints.delete(touch.identifier);
      }
    });

    window.addEventListener('touchcancel', (e) => {
      if (!this.enabled) return;
      for (const touch of e.changedTouches) {
        this.touchPoints.delete(touch.identifier);
      }
    });
  }

  private getKeyName(e: KeyboardEvent): string {
    const parts: string[] = [];
    if (e.ctrlKey) parts.push('Ctrl');
    if (e.altKey) parts.push('Alt');
    if (e.shiftKey) parts.push('Shift');
    if (e.metaKey) parts.push('Meta');

    let key = e.key;
    if (key === ' ') key = 'Space';
    if (key === 'ArrowUp') key = 'Up';
    if (key === 'ArrowDown') key = 'Down';
    if (key === 'ArrowLeft') key = 'Left';
    if (key === 'ArrowRight') key = 'Right';

    parts.push(key.length === 1 ? key.toUpperCase() : key);

    return parts.join('+');
  }

  public setCanvas(canvas: HTMLCanvasElement): void {
    this.canvas = canvas;
  }

  public bindAction(action: InputAction): void {
    this.actions.set(action.name, {
      ...action,
      active: false,
      justPressed: false,
      justReleased: false,
      pressTime: 0,
    });
  }

  public bindKey(actionName: string, key: string, modifiers?: string[]): void {
    let action = this.actions.get(actionName);
    if (!action) {
      action = {
        name: actionName,
        bindings: [],
        active: false,
        justPressed: false,
        justReleased: false,
        pressTime: 0,
      };
      this.actions.set(actionName, action);
    }

    action.bindings.push({
      key,
      action: actionName,
      modifiers: modifiers as any,
    });
  }

  public bindMouseButton(actionName: string, button: number, modifiers?: string[]): void {
    let action = this.actions.get(actionName);
    if (!action) {
      action = {
        name: actionName,
        bindings: [],
        active: false,
        justPressed: false,
        justReleased: false,
        pressTime: 0,
      };
      this.actions.set(actionName, action);
    }

    action.bindings.push({
      button,
      action: actionName,
      modifiers: modifiers as any,
    });
  }

  public unbindAction(actionName: string): void {
    this.actions.delete(actionName);
  }

  public getAction(actionName: string): InputAction | undefined {
    return this.actions.get(actionName);
  }

  public isKeyPressed(key: string): boolean {
    return this.keys.get(key) || false;
  }

  public isKeyJustPressed(key: string): boolean {
    return (this.keys.get(key) || false) && !(this.previousKeys.get(key) || false);
  }

  public isKeyJustReleased(key: string): boolean {
    return !(this.keys.get(key) || false) && (this.previousKeys.get(key) || false);
  }

  public isMouseButtonPressed(button: number): boolean {
    return this.mouseButtons.get(button) || false;
  }

  public isMouseButtonJustPressed(button: number): boolean {
    return (this.mouseButtons.get(button) || false) && !(this.previousMouseButtons.get(button) || false);
  }

  public isMouseButtonJustReleased(button: number): boolean {
    return !(this.mouseButtons.get(button) || false) && (this.previousMouseButtons.get(button) || false);
  }

  public getMousePosition(): Vector2D {
    return { ...this.mousePosition };
  }

  public getPreviousMousePosition(): Vector2D {
    return { ...this.previousMousePosition };
  }

  public getMouseDelta(): Vector2D {
    return { ...this.mouseDelta };
  }

  public getMouseWheelDelta(): Vector2D {
    return { ...this.mouseWheelDelta };
  }

  public getMousePositionOnCanvas(): Vector2D {
    if (!this.canvas) return { x: 0, y: 0 };
    const rect = this.canvas.getBoundingClientRect();
    return {
      x: this.mousePosition.x - rect.left,
      y: this.mousePosition.y - rect.top,
    };
  }

  public getTouchPoints(): Map<number, Vector2D> {
    return new Map(this.touchPoints);
  }

  public getModifiers(): { ctrl: boolean; alt: boolean; shift: boolean; meta: boolean } {
    return {
      ctrl: this.keys.get('Ctrl') || false,
      alt: this.keys.get('Alt') || false,
      shift: this.keys.get('Shift') || false,
      meta: this.keys.get('Meta') || false,
    };
  }

  private enqueueEvent(event: InputEvent): void {
    this.eventQueue.push(event);
  }

  public getEventQueue(): InputEvent[] {
    return [...this.eventQueue];
  }

  public clearEventQueue(): void {
    this.eventQueue = [];
  }

  public on(eventType: InputEventType, callback: (event: InputEvent) => void): void {
    if (!this.listeners.has(eventType)) {
      this.listeners.set(eventType, []);
    }
    this.listeners.get(eventType)!.push(callback);
  }

  public off(eventType: InputEventType, callback: (event: InputEvent) => void): void {
    const callbacks = this.listeners.get(eventType);
    if (callbacks) {
      const index = callbacks.indexOf(callback);
      if (index !== -1) {
        callbacks.splice(index, 1);
      }
    }
  }

  private notifyListeners(eventType: InputEventType, event: InputEvent): void {
    const callbacks = this.listeners.get(eventType);
    if (callbacks) {
      for (const callback of callbacks) {
        callback(event);
      }
    }
  }

  public update(): void {
    this.previousKeys = new Map(this.keys);
    this.previousMouseButtons = new Map(this.mouseButtons);
    this.mouseWheelDelta = { x: 0, y: 0 };

    for (const action of this.actions.values()) {
      action.justPressed = false;
      action.justReleased = false;

      for (const binding of action.bindings) {
        if ('key' in binding) {
          const isPressed = this.isKeyPressed(binding.key);
          const wasPressed = this.previousKeys.get(binding.key) || false;

          if (isPressed && !wasPressed) {
            action.justPressed = true;
            action.pressTime = performance.now();
          } else if (!isPressed && wasPressed) {
            action.justReleased = true;
          }

          action.active = action.active || isPressed;
        } else if ('button' in binding) {
          const isPressed = this.isMouseButtonPressed(binding.button);
          const wasPressed = this.previousMouseButtons.get(binding.button) || false;

          if (isPressed && !wasPressed) {
            action.justPressed = true;
            action.pressTime = performance.now();
          } else if (!isPressed && wasPressed) {
            action.justReleased = true;
          }

          action.active = action.active || isPressed;
        }
      }
    }

    this.clearEventQueue();
  }

  public enable(): void {
    this.enabled = true;
  }

  public disable(): void {
    this.enabled = false;
  }

  public isEnabled(): boolean {
    return this.enabled;
  }

  public reset(): void {
    this.keys.clear();
    this.previousKeys.clear();
    this.mouseButtons.clear();
    this.previousMouseButtons.clear();
    this.mousePosition = { x: 0, y: 0 };
    this.previousMousePosition = { x: 0, y: 0 };
    this.mouseDelta = { x: 0, y: 0 };
    this.mouseWheelDelta = { x: 0, y: 0 };
    this.eventQueue = [];
    this.touchPoints.clear();

    for (const action of this.actions.values()) {
      action.active = false;
      action.justPressed = false;
      action.justReleased = false;
      action.pressTime = 0;
    }
  }
}

export class VirtualJoystick {
  private input: InputManager;
  private touchId: number | null = null;
  private basePosition: Vector2D = { x: 0, y: 0 };
  private stickPosition: Vector2D = { x: 0, y: 0 };
  private deadzone: number = 0.2;
  private maxDistance: number = 50;
  private active: boolean = false;

  constructor(input: InputManager) {
    this.input = input;
  }

  public update(): void {
    const touchPoints = this.input.getTouchPoints();
    
    if (touchPoints.size > 0 && this.touchId === null) {
      this.touchId = Array.from(touchPoints.keys())[0];
      this.basePosition = touchPoints.get(this.touchId)!;
      this.stickPosition = { ...this.basePosition };
      this.active = true;
    } else if (touchPoints.size === 0 && this.touchId !== null) {
      this.touchId = null;
      this.active = false;
      this.stickPosition = { ...this.basePosition };
    } else if (this.touchId !== null && touchPoints.has(this.touchId)) {
      this.stickPosition = touchPoints.get(this.touchId)!;
    }
  }

  public getDirection(): Vector2D {
    const dx = this.stickPosition.x - this.basePosition.x;
    const dy = this.stickPosition.y - this.basePosition.y;
    const distance = Math.sqrt(dx * dx + dy * dy);

    if (distance < this.deadzone * this.maxDistance) {
      return { x: 0, y: 0 };
    }

    const normalizedX = dx / this.maxDistance;
    const normalizedY = dy / this.maxDistance;

    return {
      x: Math.max(-1, Math.min(1, normalizedX)),
      y: Math.max(-1, Math.min(1, normalizedY)),
    };
  }

  public isActive(): boolean {
    return this.active;
  }

  public setDeadzone(deadzone: number): void {
    this.deadzone = deadzone;
  }

  public setMaxDistance(distance: number): void {
    this.maxDistance = distance;
  }
}

export class GestureRecognizer {
  private input: InputManager;
  private touchStartPoints: Map<number, Vector2D> = new Map();
  private touchPaths: Map<number, Vector2D[]> = new Map();
  private pinchStartDistance: number = 0;
  private rotationStartAngle: number = 0;
  private gestures: Map<string, {
    callback: (data: any) => void;
    minTouches: number;
    maxTouches: number;
  }> = new Map();

  constructor(input: InputManager) {
    this.input = input;
  }

  public registerGesture(
    name: string,
    minTouches: number,
    maxTouches: number,
    callback: (data: any) => void
  ): void {
    this.gestures.set(name, { callback, minTouches, maxTouches });
  }

  public update(): void {
    const touchPoints = this.input.getTouchPoints();
    const count = touchPoints.size;

    if (count === 1) {
      const id = Array.from(touchPoints.keys())[0];
      const pos = touchPoints.get(id)!;

      if (!this.touchStartPoints.has(id)) {
        this.touchStartPoints.set(id, { ...pos });
        this.touchPaths.set(id, [{ ...pos }]);
      } else {
        const path = this.touchPaths.get(id)!;
        path.push({ ...pos });
      }
    } else if (count === 2) {
      const ids = Array.from(touchPoints.keys());
      const p1 = touchPoints.get(ids[0])!;
      const p2 = touchPoints.get(ids[1])!;

      const distance = Math.sqrt(
        Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2)
      );

      if (this.pinchStartDistance === 0) {
        this.pinchStartDistance = distance;
      }

      const angle = Math.atan2(p2.y - p1.y, p2.x - p1.x);
      if (this.rotationStartAngle === 0) {
        this.rotationStartAngle = angle;
      }
    }
  }

  public getPinchScale(): number {
    if (this.pinchStartDistance === 0) return 1;
    const touchPoints = this.input.getTouchPoints();
    if (touchPoints.size !== 2) return 1;

    const ids = Array.from(touchPoints.keys());
    const p1 = touchPoints.get(ids[0])!;
    const p2 = touchPoints.get(ids[1])!;

    const distance = Math.sqrt(
      Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2)
    );

    return distance / this.pinchStartDistance;
  }

  public getRotationAngle(): number {
    if (this.rotationStartAngle === 0) return 0;
    const touchPoints = this.input.getTouchPoints();
    if (touchPoints.size !== 2) return 0;

    const ids = Array.from(touchPoints.keys());
    const p1 = touchPoints.get(ids[0])!;
    const p2 = touchPoints.get(ids[1])!;

    return Math.atan2(p2.y - p1.y, p2.x - p1.x) - this.rotationStartAngle;
  }

  public getSwipeDirection(): { direction: string; velocity: Vector2D } | null {
    for (const [id, startPos] of this.touchStartPoints) {
      const path = this.touchPaths.get(id);
      if (!path || path.length < 2) continue;

      const lastPos = path[path.length - 1];
      const dx = lastPos.x - startPos.x;
      const dy = lastPos.y - startPos.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance > 50) {
        let direction: string;
        if (Math.abs(dx) > Math.abs(dy)) {
          direction = dx > 0 ? 'right' : 'left';
        } else {
          direction = dy > 0 ? 'down' : 'up';
        }

        const lastTime = performance.now();
        const firstTime = performance.now();
        const dt = (lastTime - firstTime) / 1000;

        return {
          direction,
          velocity: {
            x: dx / dt,
            y: dy / dt,
          },
        };
      }
    }

    return null;
  }

  public reset(): void {
    this.touchStartPoints.clear();
    this.touchPaths.clear();
    this.pinchStartDistance = 0;
    this.rotationStartAngle = 0;
  }
}

import { Vector2D } from '../../entities/Entity';

export interface CameraConfig {
  x: number;
  y: number;
  zoom: number;
  rotation: number;
  width: number;
  height: number;
  minZoom: number;
  maxZoom: number;
  smoothing: number;
  bounds: { minX: number; minY: number; maxX: number; maxY: number } | null;
}

export class Camera {
  private config: CameraConfig;
  private targetX: number = 0;
  private targetY: number = 0;
  private targetZoom: number = 1;
  private targetRotation: number = 0;
  private followingEntity: string | null = null;
  private shakeIntensity: number = 0;
  private shakeDuration: number = 0;
  private shakeTimer: number = 0;

  constructor(config: Partial<CameraConfig> = {}) {
    this.config = {
      x: config.x || 0,
      y: config.y || 0,
      zoom: config.zoom || 1,
      rotation: config.rotation || 0,
      width: config.width || 1920,
      height: config.height || 1080,
      minZoom: config.minZoom || 0.1,
      maxZoom: config.maxZoom || 5,
      smoothing: config.smoothing || 0.1,
      bounds: config.bounds || null,
    };

    this.targetX = this.config.x;
    this.targetY = this.config.y;
    this.targetZoom = this.config.zoom;
    this.targetRotation = this.config.rotation;
  }

  public update(deltaTime: number): void {
    if (this.shakeTimer > 0) {
      this.shakeTimer -= deltaTime;
      const shakeX = (Math.random() - 0.5) * 2 * this.shakeIntensity;
      const shakeY = (Math.random() - 0.5) * 2 * this.shakeIntensity;
      this.config.x += shakeX;
      this.config.y += shakeY;
    }

    this.config.x += (this.targetX - this.config.x) * this.config.smoothing;
    this.config.y += (this.targetY - this.config.y) * this.config.smoothing;
    this.config.zoom += (this.targetZoom - this.config.zoom) * this.config.smoothing;
    this.config.rotation += (this.targetRotation - this.config.rotation) * this.config.smoothing;

    if (this.config.bounds) {
      this.config.x = Math.max(
        this.config.bounds.minX,
        Math.min(this.config.bounds.maxX - this.config.width / this.config.zoom, this.config.x)
      );
      this.config.y = Math.max(
        this.config.bounds.minY,
        Math.min(this.config.bounds.maxY - this.config.height / this.config.zoom, this.config.y)
      );
    }

    this.config.zoom = Math.max(this.config.minZoom, Math.min(this.config.maxZoom, this.config.zoom));
  }

  public setPosition(x: number, y: number): void {
    this.targetX = x;
    this.targetY = y;
  }

  public setPositionImmediate(x: number, y: number): void {
    this.targetX = x;
    this.targetY = y;
    this.config.x = x;
    this.config.y = y;
  }

  public getPosition(): Vector2D {
    return { x: this.config.x, y: this.config.y };
  }

  public setZoom(zoom: number): void {
    this.targetZoom = Math.max(this.config.minZoom, Math.min(this.config.maxZoom, zoom));
  }

  public setZoomImmediate(zoom: number): void {
    this.targetZoom = Math.max(this.config.minZoom, Math.min(this.config.maxZoom, zoom));
    this.config.zoom = this.targetZoom;
  }

  public getZoom(): number {
    return this.config.zoom;
  }

  public setRotation(rotation: number): void {
    this.targetRotation = rotation;
  }

  public getRotation(): number {
    return this.config.rotation;
  }

  public pan(dx: number, dy: number): void {
    this.targetX += dx / this.config.zoom;
    this.targetY += dy / this.config.zoom;
  }

  public zoomIn(amount: number = 0.1): void {
    this.setZoom(this.targetZoom + amount);
  }

  public zoomOut(amount: number = 0.1): void {
    this.setZoom(this.targetZoom - amount);
  }

  public rotate(degrees: number): void {
    this.targetRotation += degrees * (Math.PI / 180);
  }

  public follow(entityId: string): void {
    this.followingEntity = entityId;
  }

  public stopFollowing(): void {
    this.followingEntity = null;
  }

  public getFollowing(): string | null {
    return this.followingEntity;
  }

  public shake(intensity: number, duration: number): void {
    this.shakeIntensity = intensity;
    this.shakeDuration = duration;
    this.shakeTimer = duration;
  }

  public isShaking(): boolean {
    return this.shakeTimer > 0;
  }

  public worldToScreen(worldX: number, worldY: number): Vector2D {
    const screenX = (worldX - this.config.x) * this.config.zoom + this.config.width / 2;
    const screenY = (worldY - this.config.y) * this.config.zoom + this.config.height / 2;
    return { x: screenX, y: screenY };
  }

  public screenToWorld(screenX: number, screenY: number): Vector2D {
    const worldX = (screenX - this.config.width / 2) / this.config.zoom + this.config.x;
    const worldY = (screenY - this.config.height / 2) / this.config.zoom + this.config.y;
    return { x: worldX, y: worldY };
  }

  public getViewBounds(): { minX: number; minY: number; maxX: number; maxY: number } {
    const halfWidth = (this.config.width / 2) / this.config.zoom;
    const halfHeight = (this.config.height / 2) / this.config.zoom;

    return {
      minX: this.config.x - halfWidth,
      minY: this.config.y - halfHeight,
      maxX: this.config.x + halfWidth,
      maxY: this.config.y + halfHeight,
    };
  }

  public isInView(x: number, y: number, radius: number = 0): boolean {
    const bounds = this.getViewBounds();
    return (
      x + radius >= bounds.minX &&
      x - radius <= bounds.maxX &&
      y + radius >= bounds.minY &&
      y - radius <= bounds.maxY
    );
  }

  public setBounds(minX: number, minY: number, maxX: number, maxY: number): void {
    this.config.bounds = { minX, minY, maxX, maxY };
  }

  public clearBounds(): void {
    this.config.bounds = null;
  }

  public getBounds(): { minX: number; minY: number; maxX: number; maxY: number } | null {
    return this.config.bounds;
  }

  public setViewport(width: number, height: number): void {
    this.config.width = width;
    this.config.height = height;
  }

  public getViewport(): { width: number; height: number } {
    return { width: this.config.width, height: this.config.height };
  }

  public setSmoothing(smoothing: number): void {
    this.config.smoothing = Math.max(0.01, Math.min(1, smoothing));
  }

  public getSmoothing(): number {
    return this.config.smoothing;
  }

  public focusOn(x: number, y: number, zoom?: number): void {
    this.setPosition(x, y);
    if (zoom !== undefined) {
      this.setZoom(zoom);
    }
  }

  public focusOnRegion(minX: number, minY: number, maxX: number, maxY: number): void {
    const centerX = (minX + maxX) / 2;
    const centerY = (minY + maxY) / 2;
    this.setPosition(centerX, centerY);

    const regionWidth = maxX - minX;
    const regionHeight = maxY - minY;

    const zoomX = this.config.width / regionWidth;
    const zoomY = this.config.height / regionHeight;
    const zoom = Math.min(zoomX, zoomY, this.config.maxZoom);

    this.setZoom(zoom);
  }

  public getConfig(): CameraConfig {
    return { ...this.config };
  }

  public setConfig(config: Partial<CameraConfig>): void {
    Object.assign(this.config, config);
  }

  public reset(): void {
    this.targetX = 0;
    this.targetY = 0;
    this.targetZoom = 1;
    this.targetRotation = 0;
    this.followingEntity = null;
    this.shakeTimer = 0;
    this.config.x = 0;
    this.config.y = 0;
    this.config.zoom = 1;
    this.config.rotation = 0;
  }
}

export class CameraController {
  private camera: Camera;
  private inputManager: any;
  private moveSpeed: number = 500;
  private panEdges: boolean = true;
  private edgeMargin: number = 50;
  private keyboardControls: boolean = true;
  private mouseControls: boolean = true;

  constructor(camera: Camera, inputManager: any) {
    this.camera = camera;
    this.inputManager = inputManager;
  }

  public update(deltaTime: number): void {
    if (!this.camera) return;

    if (this.keyboardControls) {
      this.handleKeyboard(deltaTime);
    }

    if (this.mouseControls && this.panEdges) {
      this.handleEdgePanning();
    }
  }

  private handleKeyboard(deltaTime: number): void {
    const speed = this.moveSpeed * deltaTime;

    if (this.inputManager.isKeyPressed('W') || this.inputManager.isKeyPressed('Up')) {
      this.camera.pan(0, -speed);
    }
    if (this.inputManager.isKeyPressed('S') || this.inputManager.isKeyPressed('Down')) {
      this.camera.pan(0, speed);
    }
    if (this.inputManager.isKeyPressed('A') || this.inputManager.isKeyPressed('Left')) {
      this.camera.pan(-speed, 0);
    }
    if (this.inputManager.isKeyPressed('D') || this.inputManager.isKeyPressed('Right')) {
      this.camera.pan(speed, 0);
    }

    if (this.inputManager.isKeyPressed('Q')) {
      this.camera.zoomIn(0.05);
    }
    if (this.inputManager.isKeyPressed('E')) {
      this.camera.zoomOut(0.05);
    }

    if (this.inputManager.isKeyJustPressed('Space')) {
      this.camera.setZoom(1);
      this.camera.setRotation(0);
    }
  }

  private handleEdgePanning(): void {
    const mousePos = this.inputManager.getMousePosition();
    const viewport = this.camera.getViewport();
    const speed = this.moveSpeed * 0.1;

    if (mousePos.x < this.edgeMargin) {
      this.camera.pan(-speed, 0);
    }
    if (mousePos.x > viewport.width - this.edgeMargin) {
      this.camera.pan(speed, 0);
    }
    if (mousePos.y < this.edgeMargin) {
      this.camera.pan(0, -speed);
    }
    if (mousePos.y > viewport.height - this.edgeMargin) {
      this.camera.pan(0, speed);
    }
  }

  public setMoveSpeed(speed: number): void {
    this.moveSpeed = speed;
  }

  public getMoveSpeed(): number {
    return this.moveSpeed;
  }

  public setPanEdges(enabled: boolean): void {
    this.panEdges = enabled;
  }

  public isPanEdgesEnabled(): boolean {
    return this.panEdges;
  }

  public setEdgeMargin(margin: number): void {
    this.edgeMargin = margin;
  }

  public getEdgeMargin(): number {
    return this.edgeMargin;
  }

  public setKeyboardControls(enabled: boolean): void {
    this.keyboardControls = enabled;
  }

  public setMouseControls(enabled: boolean): void {
    this.mouseControls = enabled;
  }
}

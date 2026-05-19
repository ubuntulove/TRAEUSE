import { Vector2D, Position, Entity } from '../../entities/Entity';

export interface RenderLayer {
  name: string;
  order: number;
  visible: boolean;
  parallax: number;
}

export interface Sprite {
  image: HTMLImageElement | HTMLCanvasElement;
  width: number;
  height: number;
  frameWidth?: number;
  frameHeight?: number;
  frames?: number;
  frameRate?: number;
  offsetX?: number;
  offsetY?: number;
}

export interface Animation {
  name: string;
  frames: number[];
  frameRate: number;
  loop: boolean;
}

export interface TextStyle {
  font: string;
  size: number;
  color: string;
  align: 'left' | 'center' | 'right';
  baseline: 'top' | 'middle' | 'bottom';
  bold?: boolean;
  italic?: boolean;
  stroke?: { color: string; width: number };
  shadow?: { color: string; blur: number; offsetX: number; offsetY: number };
}

export interface Camera {
  x: number;
  y: number;
  zoom: number;
  rotation: number;
  width: number;
  height: number;
  target: Entity | null;
  smoothing: number;
  bounds: { minX: number; minY: number; maxX: number; maxY: number } | null;
}

export class RenderEngine {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private width: number;
  private height: number;
  private camera: Camera;
  private layers: Map<string, RenderLayer> = new Map();
  private sprites: Map<string, Sprite> = new Map();
  private animations: Map<string, Animation> = new Map();
  private activeAnimations: Map<string, { animation: Animation; frame: number; time: number }> = new Map();
  private renderQueue: Array<{ layer: string; render: () => void }> = [];
  private backgroundColor: string = '#000000';
  private debugMode: boolean = false;
  private gridSize: number = 0;
  private gridColor: string = 'rgba(255, 255, 255, 0.1)';

  constructor(canvas: HTMLCanvasElement, width: number, height: number) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
    this.width = width;
    this.height = height;
    this.camera = {
      x: 0,
      y: 0,
      zoom: 1,
      rotation: 0,
      width,
      height,
      target: null,
      smoothing: 0.1,
      bounds: null,
    };

    canvas.width = width;
    canvas.height = height;
  }

  public addLayer(name: string, order: number, visible: boolean = true, parallax: number = 1): void {
    this.layers.set(name, { name, order, visible, parallax });
  }

  public removeLayer(name: string): void {
    this.layers.delete(name);
  }

  public setLayerVisible(name: string, visible: boolean): void {
    const layer = this.layers.get(name);
    if (layer) {
      layer.visible = visible;
    }
  }

  public getLayer(name: string): RenderLayer | undefined {
    return this.layers.get(name);
  }

  public addSprite(id: string, imageSrc: string): Promise<void> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => {
        this.sprites.set(id, {
          image: img,
          width: img.width,
          height: img.height,
        });
        resolve();
      };
      img.onerror = reject;
      img.src = imageSrc;
    });
  }

  public addSpriteSheet(
    id: string,
    imageSrc: string,
    frameWidth: number,
    frameHeight: number,
    frames: number
  ): Promise<void> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => {
        this.sprites.set(id, {
          image: img,
          width: img.width,
          height: img.height,
          frameWidth,
          frameHeight,
          frames,
        });
        resolve();
      };
      img.onerror = reject;
      img.src = imageSrc;
    });
  }

  public getSprite(id: string): Sprite | undefined {
    return this.sprites.get(id);
  }

  public addAnimation(animation: Animation): void {
    this.animations.set(animation.name, animation);
  }

  public playAnimation(entityId: string, animationName: string): void {
    const animation = this.animations.get(animationName);
    if (animation) {
      this.activeAnimations.set(entityId, {
        animation,
        frame: 0,
        time: 0,
      });
    }
  }

  public stopAnimation(entityId: string): void {
    this.activeAnimations.delete(entityId);
  }

  public setBackgroundColor(color: string): void {
    this.backgroundColor = color;
  }

  public setGrid(size: number, color: string): void {
    this.gridSize = size;
    this.gridColor = color;
  }

  public setDebugMode(enabled: boolean): void {
    this.debugMode = enabled;
  }

  public setCameraTarget(target: Entity | null): void {
    this.camera.target = target;
  }

  public setCameraPosition(x: number, y: number): void {
    this.camera.x = x;
    this.camera.y = y;
  }

  public setCameraZoom(zoom: number): void {
    this.camera.zoom = Math.max(0.1, Math.min(10, zoom));
  }

  public setCameraRotation(rotation: number): void {
    this.camera.rotation = rotation;
  }

  public setCameraBounds(minX: number, minY: number, maxX: number, maxY: number): void {
    this.camera.bounds = { minX, minY, maxX, maxY };
  }

  public clearCameraBounds(): void {
    this.camera.bounds = null;
  }

  public updateCamera(deltaTime: number): void {
    if (this.camera.target) {
      const targetX = this.camera.target.position.x - this.camera.width / 2;
      const targetY = this.camera.target.position.y - this.camera.height / 2;

      this.camera.x += (targetX - this.camera.x) * this.camera.smoothing;
      this.camera.y += (targetY - this.camera.y) * this.camera.smoothing;
    }

    if (this.camera.bounds) {
      this.camera.x = Math.max(
        this.camera.bounds.minX,
        Math.min(this.camera.bounds.maxX - this.camera.width, this.camera.x)
      );
      this.camera.y = Math.max(
        this.camera.bounds.minY,
        Math.min(this.camera.bounds.maxY - this.camera.height, this.camera.y)
      );
    }
  }

  public worldToScreen(worldX: number, worldY: number): Vector2D {
    const screenX = (worldX - this.camera.x) * this.camera.zoom;
    const screenY = (worldY - this.camera.y) * this.camera.zoom;
    return { x: screenX, y: screenY };
  }

  public screenToWorld(screenX: number, screenY: number): Vector2D {
    const worldX = screenX / this.camera.zoom + this.camera.x;
    const worldY = screenY / this.camera.zoom + this.camera.y;
    return { x: worldX, y: worldY };
  }

  public clear(): void {
    this.ctx.fillStyle = this.backgroundColor;
    this.ctx.fillRect(0, 0, this.width, this.height);
  }

  public beginFrame(): void {
    this.renderQueue = [];
  }

  public endFrame(): void {
    const sortedLayers = Array.from(this.layers.values()).sort((a, b) => a.order - b.order);

    for (const layer of sortedLayers) {
      if (!layer.visible) continue;

      for (const item of this.renderQueue) {
        if (item.layer === layer.name) {
          item.render();
        }
      }
    }

    this.renderQueue = [];
  }

  public queueRender(layer: string, render: () => void): void {
    this.renderQueue.push({ layer, render });
  }

  public drawSprite(
    spriteId: string,
    x: number,
    y: number,
    options: {
      width?: number;
      height?: number;
      rotation?: number;
      alpha?: number;
      flipX?: boolean;
      flipY?: boolean;
      frame?: number;
    } = {}
  ): void {
    const sprite = this.sprites.get(spriteId);
    if (!sprite) return;

    const screenPos = this.worldToScreen(x, y);

    this.ctx.save();
    this.ctx.globalAlpha = options.alpha ?? 1;

    if (options.rotation) {
      this.ctx.translate(screenPos.x, screenPos.y);
      this.ctx.rotate(options.rotation);
      this.ctx.translate(-screenPos.x, -screenPos.y);
    }

    const drawWidth = options.width ?? sprite.frameWidth ?? sprite.width;
    const drawHeight = options.height ?? sprite.frameHeight ?? sprite.height;

    if (options.flipX || options.flipY) {
      this.ctx.translate(screenPos.x, screenPos.y);
      this.ctx.scale(options.flipX ? -1 : 1, options.flipY ? -1 : 1);
      this.ctx.translate(-screenPos.x, -screenPos.y);
    }

    if (sprite.frameWidth && sprite.frameHeight && options.frame !== undefined) {
      const framesPerRow = Math.floor(sprite.width / sprite.frameWidth);
      const frameX = (options.frame % framesPerRow) * sprite.frameWidth;
      const frameY = Math.floor(options.frame / framesPerRow) * sprite.frameHeight;

      this.ctx.drawImage(
        sprite.image,
        frameX,
        frameY,
        sprite.frameWidth,
        sprite.frameHeight,
        screenPos.x - drawWidth / 2,
        screenPos.y - drawHeight / 2,
        drawWidth,
        drawHeight
      );
    } else {
      this.ctx.drawImage(
        sprite.image,
        screenPos.x - drawWidth / 2,
        screenPos.y - drawHeight / 2,
        drawWidth,
        drawHeight
      );
    }

    this.ctx.restore();
  }

  public drawRectangle(
    x: number,
    y: number,
    width: number,
    height: number,
    fill: string,
    stroke?: string,
    strokeWidth?: number,
    rotation?: number,
    alpha?: number
  ): void {
    const screenPos = this.worldToScreen(x, y);

    this.ctx.save();
    this.ctx.globalAlpha = alpha ?? 1;

    if (rotation) {
      this.ctx.translate(screenPos.x, screenPos.y);
      this.ctx.rotate(rotation);
      this.ctx.translate(-screenPos.x, -screenPos.y);
    }

    if (fill) {
      this.ctx.fillStyle = fill;
      this.ctx.fillRect(screenPos.x - width / 2, screenPos.y - height / 2, width, height);
    }

    if (stroke) {
      this.ctx.strokeStyle = stroke;
      this.ctx.lineWidth = strokeWidth ?? 1;
      this.ctx.strokeRect(screenPos.x - width / 2, screenPos.y - height / 2, width, height);
    }

    this.ctx.restore();
  }

  public drawCircle(
    x: number,
    y: number,
    radius: number,
    fill: string,
    stroke?: string,
    strokeWidth?: number,
    alpha?: number
  ): void {
    const screenPos = this.worldToScreen(x, y);

    this.ctx.save();
    this.ctx.globalAlpha = alpha ?? 1;

    this.ctx.beginPath();
    this.ctx.arc(screenPos.x, screenPos.y, radius * this.camera.zoom, 0, Math.PI * 2);

    if (fill) {
      this.ctx.fillStyle = fill;
      this.ctx.fill();
    }

    if (stroke) {
      this.ctx.strokeStyle = stroke;
      this.ctx.lineWidth = strokeWidth ?? 1;
      this.ctx.stroke();
    }

    this.ctx.restore();
  }

  public drawLine(
    x1: number,
    y1: number,
    x2: number,
    y2: number,
    color: string,
    width?: number,
    alpha?: number
  ): void {
    const screenPos1 = this.worldToScreen(x1, y1);
    const screenPos2 = this.worldToScreen(x2, y2);

    this.ctx.save();
    this.ctx.globalAlpha = alpha ?? 1;
    this.ctx.strokeStyle = color;
    this.ctx.lineWidth = width ?? 1;
    this.ctx.beginPath();
    this.ctx.moveTo(screenPos1.x, screenPos1.y);
    this.ctx.lineTo(screenPos2.x, screenPos2.y);
    this.ctx.stroke();
    this.ctx.restore();
  }

  public drawPolygon(
    points: Vector2D[],
    fill: string,
    stroke?: string,
    strokeWidth?: number,
    alpha?: number
  ): void {
    if (points.length < 3) return;

    this.ctx.save();
    this.ctx.globalAlpha = alpha ?? 1;

    this.ctx.beginPath();
    const firstScreen = this.worldToScreen(points[0].x, points[0].y);
    this.ctx.moveTo(firstScreen.x, firstScreen.y);

    for (let i = 1; i < points.length; i++) {
      const screen = this.worldToScreen(points[i].x, points[i].y);
      this.ctx.lineTo(screen.x, screen.y);
    }

    this.ctx.closePath();

    if (fill) {
      this.ctx.fillStyle = fill;
      this.ctx.fill();
    }

    if (stroke) {
      this.ctx.strokeStyle = stroke;
      this.ctx.lineWidth = strokeWidth ?? 1;
      this.ctx.stroke();
    }

    this.ctx.restore();
  }

  public drawText(
    text: string,
    x: number,
    y: number,
    style: TextStyle,
    alpha?: number
  ): void {
    const screenPos = this.worldToScreen(x, y);

    this.ctx.save();
    this.ctx.globalAlpha = alpha ?? 1;

    let fontString = '';
    if (style.bold) fontString += 'bold ';
    if (style.italic) fontString += 'italic ';
    fontString += `${style.size}px ${style.font}`;
    this.ctx.font = fontString;

    this.ctx.textAlign = style.align;
    this.ctx.textBaseline = style.baseline;

    if (style.shadow) {
      this.ctx.shadowColor = style.shadow.color;
      this.ctx.shadowBlur = style.shadow.blur;
      this.ctx.shadowOffsetX = style.shadow.offsetX;
      this.ctx.shadowOffsetY = style.shadow.offsetY;
    }

    if (style.stroke) {
      this.ctx.strokeStyle = style.stroke.color;
      this.ctx.lineWidth = style.stroke.width;
      this.ctx.strokeText(text, screenPos.x, screenPos.y);
    }

    this.ctx.fillStyle = style.color;
    this.ctx.fillText(text, screenPos.x, screenPos.y);

    this.ctx.restore();
  }

  public drawHealthBar(
    x: number,
    y: number,
    width: number,
    height: number,
    health: number,
    maxHealth: number,
    backgroundColor?: string,
    foregroundColor?: string
  ): void {
    const screenPos = this.worldToScreen(x, y);
    const healthPercent = health / maxHealth;

    this.ctx.fillStyle = backgroundColor ?? '#333333';
    this.ctx.fillRect(screenPos.x - width / 2, screenPos.y - height / 2, width, height);

    let color = foregroundColor ?? '#00ff00';
    if (healthPercent < 0.5) color = '#ffff00';
    if (healthPercent < 0.25) color = '#ff0000';

    this.ctx.fillStyle = color;
    this.ctx.fillRect(
      screenPos.x - width / 2,
      screenPos.y - height / 2,
      width * healthPercent,
      height
    );
  }

  public drawGrid(): void {
    if (this.gridSize <= 0) return;

    const startX = Math.floor(this.camera.x / this.gridSize) * this.gridSize;
    const startY = Math.floor(this.camera.y / this.gridSize) * this.gridSize;
    const endX = startX + this.camera.width / this.camera.zoom + this.gridSize * 2;
    const endY = startY + this.camera.height / this.camera.zoom + this.gridSize * 2;

    this.ctx.strokeStyle = this.gridColor;
    this.ctx.lineWidth = 1;

    for (let x = startX; x < endX; x += this.gridSize) {
      const screen = this.worldToScreen(x, 0);
      this.ctx.beginPath();
      this.ctx.moveTo(screen.x, 0);
      this.ctx.lineTo(screen.x, this.height);
      this.ctx.stroke();
    }

    for (let y = startY; y < endY; y += this.gridSize) {
      const screen = this.worldToScreen(0, y);
      this.ctx.beginPath();
      this.ctx.moveTo(0, screen.y);
      this.ctx.lineTo(this.width, screen.y);
      this.ctx.stroke();
    }
  }

  public drawMinimap(
    minimapX: number,
    minimapY: number,
    minimapWidth: number,
    minimapHeight: number,
    worldWidth: number,
    worldHeight: number,
    entities: Entity[],
    backgroundColor?: string
  ): void {
    this.ctx.save();

    this.ctx.fillStyle = backgroundColor ?? '#1a1a2e';
    this.ctx.fillRect(minimapX, minimapY, minimapWidth, minimapHeight);

    this.ctx.strokeStyle = '#ffffff';
    this.ctx.lineWidth = 2;
    this.ctx.strokeRect(minimapX, minimapY, minimapWidth, minimapHeight);

    const scaleX = minimapWidth / worldWidth;
    const scaleY = minimapHeight / worldHeight;

    for (const entity of entities) {
      if (!entity.visible) continue;

      const dotX = minimapX + entity.position.x * scaleX;
      const dotY = minimapY + entity.position.y * scaleY;

      let color = '#888888';
      if (entity.type === 'building') color = '#4CAF50';
      if (entity.type === 'unit') color = '#2196F3';
      if (entity.type === 'enemy') color = '#f44336';

      this.ctx.fillStyle = color;
      this.ctx.fillRect(dotX - 1, dotY - 1, 3, 3);
    }

    const cameraRectX = minimapX + this.camera.x * scaleX;
    const cameraRectY = minimapY + this.camera.y * scaleY;
    const cameraRectW = (this.camera.width / this.camera.zoom) * scaleX;
    const cameraRectH = (this.camera.height / this.camera.zoom) * scaleY;

    this.ctx.strokeStyle = '#ffffff';
    this.ctx.lineWidth = 1;
    this.ctx.strokeRect(cameraRectX, cameraRectY, cameraRectW, cameraRectH);

    this.ctx.restore();
  }

  public drawPath(path: Vector2D[], color: string = '#00ff00'): void {
    if (path.length < 2) return;

    this.ctx.save();
    this.ctx.strokeStyle = color;
    this.ctx.lineWidth = 2;
    this.ctx.setLineDash([5, 5]);

    this.ctx.beginPath();
    const firstScreen = this.worldToScreen(path[0].x, path[0].y);
    this.ctx.moveTo(firstScreen.x, firstScreen.y);

    for (let i = 1; i < path.length; i++) {
      const screen = this.worldToScreen(path[i].x, path[i].y);
      this.ctx.lineTo(screen.x, screen.y);
    }

    this.ctx.stroke();
    this.ctx.restore();
  }

  public drawSelectionBox(
    startX: number,
    startY: number,
    endX: number,
    endY: number,
    color: string = '#00ff00'
  ): void {
    this.ctx.save();
    this.ctx.strokeStyle = color;
    this.ctx.lineWidth = 2;
    this.ctx.setLineDash([5, 5]);

    const width = Math.abs(endX - startX);
    const height = Math.abs(endY - startY);
    const x = Math.min(startX, endX);
    const y = Math.min(startY, endY);

    this.ctx.strokeRect(x, y, width, height);
    this.ctx.restore();
  }

  public getContext(): CanvasRenderingContext2D {
    return this.ctx;
  }

  public getCanvas(): HTMLCanvasElement {
    return this.canvas;
  }

  public resize(width: number, height: number): void {
    this.width = width;
    this.height = height;
    this.canvas.width = width;
    this.canvas.height = height;
    this.camera.width = width;
    this.camera.height = height;
  }

  public getCamera(): Camera {
    return { ...this.camera };
  }
}

export class ParticleRenderer {
  private particles: Array<{
    x: number;
    y: number;
    vx: number;
    vy: number;
    life: number;
    maxLife: number;
    size: number;
    color: string;
    alpha: number;
    decay: number;
  }> = [];

  public addParticle(
    x: number,
    y: number,
    vx: number,
    vy: number,
    life: number,
    size: number,
    color: string,
    alpha: number = 1,
    decay: number = 1
  ): void {
    this.particles.push({
      x,
      y,
      vx,
      vy,
      life,
      maxLife: life,
      size,
      color,
      alpha,
      decay,
    });
  }

  public update(deltaTime: number): void {
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const p = this.particles[i];
      p.x += p.vx * deltaTime;
      p.y += p.vy * deltaTime;
      p.life -= deltaTime;
      p.alpha *= Math.pow(p.decay, deltaTime);

      if (p.life <= 0 || p.alpha <= 0) {
        this.particles.splice(i, 1);
      }
    }
  }

  public render(ctx: CanvasRenderingContext2D, camera: Camera): void {
    for (const p of this.particles) {
      const screenX = (p.x - camera.x) * camera.zoom;
      const screenY = (p.y - camera.y) * camera.zoom;

      ctx.save();
      ctx.globalAlpha = p.alpha;
      ctx.fillStyle = p.color;
      ctx.beginPath();
      ctx.arc(screenX, screenY, p.size * camera.zoom, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    }
  }

  public clear(): void {
    this.particles = [];
  }

  public getCount(): number {
    return this.particles.length;
  }
}

export class Light2D {
  public x: number;
  public y: number;
  public radius: number;
  public color: string;
  public intensity: number;
  public angle: number;
  public spread: number;
  public enabled: boolean;

  constructor(
    x: number,
    y: number,
    radius: number,
    color: string = '#ffffff',
    intensity: number = 1
  ) {
    this.x = x;
    this.y = y;
    this.radius = radius;
    this.color = color;
    this.intensity = intensity;
    this.angle = 0;
    this.spread = Math.PI * 2;
    this.enabled = true;
  }

  public setPosition(x: number, y: number): void {
    this.x = x;
    this.y = y;
  }

  public setRadius(radius: number): void {
    this.radius = radius;
  }

  public setColor(color: string): void {
    this.color = color;
  }

  public setIntensity(intensity: number): void {
    this.intensity = intensity;
  }

  public setCone(angle: number, spread: number): void {
    this.angle = angle;
    this.spread = spread;
  }

  public enable(): void {
    this.enabled = true;
  }

  public disable(): void {
    this.enabled = false;
  }
}

export class LightRenderer {
  private lights: Light2D[] = [];
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;

  constructor(canvas: HTMLCanvasElement) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
  }

  public addLight(light: Light2D): void {
    this.lights.push(light);
  }

  public removeLight(light: Light2D): void {
    const index = this.lights.indexOf(light);
    if (index !== -1) {
      this.lights.splice(index, 1);
    }
  }

  public render(baseCanvas: HTMLCanvasElement, camera: Camera): void {
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

    this.ctx.globalCompositeOperation = 'lighter';

    for (const light of this.lights) {
      if (!light.enabled) continue;

      const screenX = (light.x - camera.x) * camera.zoom;
      const screenY = (light.y - camera.y) * camera.zoom;
      const screenRadius = light.radius * camera.zoom;

      const gradient = this.ctx.createRadialGradient(
        screenX,
        screenY,
        0,
        screenX,
        screenY,
        screenRadius
      );

      const color = this.hexToRgba(light.color, light.intensity);
      gradient.addColorStop(0, color);
      gradient.addColorStop(1, this.hexToRgba(light.color, 0));

      this.ctx.fillStyle = gradient;
      this.ctx.beginPath();
      this.ctx.arc(screenX, screenY, screenRadius, 0, Math.PI * 2);
      this.ctx.fill();
    }

    this.ctx.globalCompositeOperation = 'source-over';

    this.ctx.drawImage(baseCanvas, 0, 0);
  }

  private hexToRgba(hex: string, alpha: number): string {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  }

  public clear(): void {
    this.lights = [];
  }

  public getLights(): Light2D[] {
    return [...this.lights];
  }
}

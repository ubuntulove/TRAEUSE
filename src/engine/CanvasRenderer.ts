
import { SceneManager } from "./SceneManager";
import { PhysicsSystem } from "./PhysicsSystem";
import { InputHandler } from "./InputHandler";

export class CanvasRenderer {
  private canvas: HTMLCanvasElement;
  private context: CanvasRenderingContext2D;
  private sceneManager: SceneManager;
  private physicsSystem: PhysicsSystem;
  private inputHandler: InputHandler;
  private animationId: number | null = null;
  private lastTime: number = 0;
  public isRunning: boolean = false;
  public backgroundColor: string = "#0a0e27";

  constructor(
    canvas: HTMLCanvasElement,
    sceneManager: SceneManager,
    physicsSystem: PhysicsSystem,
    inputHandler: InputHandler
  ) {
    this.canvas = canvas;
    this.context = canvas.getContext("2d")!;
    this.sceneManager = sceneManager;
    this.physicsSystem = physicsSystem;
    this.inputHandler = inputHandler;
  }

  private gameLoop(timestamp: number): void {
    if (!this.isRunning) return;

    const deltaTime = (timestamp - this.lastTime) / 1000; // Convert to seconds
    this.lastTime = timestamp;

    this.update(deltaTime);
    this.render();

    this.animationId = requestAnimationFrame((t) => this.gameLoop(t));
  }

  private update(deltaTime: number): void {
    const objects = this.sceneManager.getAllObjects();
    this.physicsSystem.update(objects, deltaTime);
    this.sceneManager.update(deltaTime);
  }

  private render(): void {
    // Clear canvas
    this.context.fillStyle = this.backgroundColor;
    this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);

    // Render all objects
    this.sceneManager.render(this.context);
  }

  public start(): void {
    if (!this.isRunning) {
      this.isRunning = true;
      this.lastTime = performance.now();
      this.animationId = requestAnimationFrame((t) => this.gameLoop(t));
    }
  }

  public stop(): void {
    this.isRunning = false;
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
      this.animationId = null;
    }
  }

  public reset(): void {
    this.stop();
    this.sceneManager.clear();
    this.render();
  }
}

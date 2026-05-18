
export interface Vector2 {
  x: number;
  y: number;
}

export class GameObject {
  public id: string;
  public position: Vector2;
  public velocity: Vector2;
  public size: Vector2;
  public color: string;
  public active: boolean;

  constructor(
    id: string,
    position: Vector2,
    size: Vector2,
    color: string = "#00d4ff"
  ) {
    this.id = id;
    this.position = position;
    this.velocity = { x: 0, y: 0 };
    this.size = size;
    this.color = color;
    this.active = true;
  }

  public update(deltaTime: number): void {
    // Override this method in subclasses
  }

  public render(context: CanvasRenderingContext2D): void {
    context.fillStyle = this.color;
    context.fillRect(
      this.position.x,
      this.position.y,
      this.size.x,
      this.size.y
    );
  }
}

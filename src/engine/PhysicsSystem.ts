
import { GameObject, Vector2 } from "./GameObject";

export class PhysicsSystem {
  public gravity: Vector2;
  public friction: number;

  constructor(gravity: Vector2 = { x: 0, y: 500 }, friction: number = 0.98) {
    this.gravity = gravity;
    this.friction = friction;
  }

  public applyGravity(obj: GameObject, deltaTime: number): void {
    obj.velocity.x += this.gravity.x * deltaTime;
    obj.velocity.y += this.gravity.y * deltaTime;
  }

  public applyFriction(obj: GameObject): void {
    obj.velocity.x *= this.friction;
    obj.velocity.y *= this.friction;
  }

  public updatePosition(obj: GameObject, deltaTime: number): void {
    obj.position.x += obj.velocity.x * deltaTime;
    obj.position.y += obj.velocity.y * deltaTime;
  }

  public checkCollision(obj1: GameObject, obj2: GameObject): boolean {
    return (
      obj1.position.x < obj2.position.x + obj2.size.x &&
      obj1.position.x + obj1.size.x > obj2.position.x &&
      obj1.position.y < obj2.position.y + obj2.size.y &&
      obj1.position.y + obj1.size.y > obj2.position.y
    );
  }

  public update(objects: GameObject[], deltaTime: number): void {
    objects.forEach((obj) => {
      if (obj.active) {
        this.applyGravity(obj, deltaTime);
        this.applyFriction(obj);
        this.updatePosition(obj, deltaTime);
      }
    });
  }
}

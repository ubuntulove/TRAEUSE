
import { GameObject } from "./GameObject";

export class SceneManager {
  private gameObjects: Map<string, GameObject> = new Map();

  public addObject(object: GameObject): void {
    this.gameObjects.set(object.id, object);
  }

  public removeObject(id: string): void {
    this.gameObjects.delete(id);
  }

  public getObject(id: string): GameObject | undefined {
    return this.gameObjects.get(id);
  }

  public getAllObjects(): GameObject[] {
    return Array.from(this.gameObjects.values());
  }

  public update(deltaTime: number): void {
    this.gameObjects.forEach((obj) => {
      if (obj.active) {
        obj.update(deltaTime);
      }
    });
  }

  public render(context: CanvasRenderingContext2D): void {
    this.gameObjects.forEach((obj) => {
      if (obj.active) {
        obj.render(context);
      }
    });
  }

  public clear(): void {
    this.gameObjects.clear();
  }
}

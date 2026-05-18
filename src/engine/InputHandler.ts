
type KeyState = { [key: string]: boolean };

export class InputHandler {
  private keys: KeyState = {};
  private mouse: { x: number; y: number; pressed: boolean } = {
    x: 0,
    y: 0,
    pressed: false,
  };

  constructor() {
    this.setupEventListeners();
  }

  private setupEventListeners(): void {
    window.addEventListener("keydown", (e) => {
      this.keys[e.code] = true;
    });

    window.addEventListener("keyup", (e) => {
      this.keys[e.code] = false;
    });

    window.addEventListener("mousemove", (e) => {
      this.mouse.x = e.clientX;
      this.mouse.y = e.clientY;
    });

    window.addEventListener("mousedown", () => {
      this.mouse.pressed = true;
    });

    window.addEventListener("mouseup", () => {
      this.mouse.pressed = false;
    });
  }

  public isKeyDown(key: string): boolean {
    return this.keys[key] || false;
  }

  public getMousePosition(): { x: number; y: number } {
    return { x: this.mouse.x, y: this.mouse.y };
  }

  public isMouseDown(): boolean {
    return this.mouse.pressed;
  }
}

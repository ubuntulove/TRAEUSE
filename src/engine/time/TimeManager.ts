export interface GameTime {
  hours: number;
  minutes: number;
  seconds: number;
  totalSeconds: number;
  totalMilliseconds: number;
  day: number;
  week: number;
  month: number;
  year: number;
}

export interface TimeScaleConfig {
  min: number;
  max: number;
  default: number;
}

export class TimeManager {
  private currentTime: number = 0;
  private deltaTime: number = 0;
  private lastFrameTime: number = 0;
  private timeScale: number = 1.0;
  private paused: boolean = false;
  private fixedDeltaTime: number = 1 / 60;
  private accumulator: number = 0;
  private timeScaleConfig: TimeScaleConfig = { min: 0.1, max: 5.0, default: 1.0 };
  private frameCount: number = 0;
  private fps: number = 0;
  private fpsUpdateInterval: number = 0.5;
  private fpsAccumulator: number = 0;
  private fpsFrameCount: number = 0;
  private realTimeSinceStart: number = 0;
  private gameTime: GameTime = {
    hours: 8,
    minutes: 0,
    seconds: 0,
    totalSeconds: 0,
    totalMilliseconds: 0,
    day: 1,
    week: 1,
    month: 1,
    year: 1,
  };
  private listeners: Map<string, Array<(data: any) => void>> = new Map();
  private timers: Map<string, { duration: number; elapsed: number; repeat: boolean; callback: () => void }> = new Map();
  private scheduledEvents: Array<{ time: number; callback: () => void }> = [];

  constructor() {
    this.lastFrameTime = performance.now();
  }

  public update(currentTime: number): void {
    this.deltaTime = (currentTime - this.lastFrameTime) / 1000;
    this.lastFrameTime = currentTime;
    this.realTimeSinceStart += this.deltaTime;

    this.fpsAccumulator += this.deltaTime;
    this.fpsFrameCount++;

    if (this.fpsAccumulator >= this.fpsUpdateInterval) {
      this.fps = Math.round(this.fpsFrameCount / this.fpsAccumulator);
      this.fpsAccumulator = 0;
      this.fpsFrameCount = 0;
    }

    if (this.paused) {
      this.deltaTime = 0;
      return;
    }

    const scaledDelta = this.deltaTime * this.timeScale;
    this.currentTime += scaledDelta;
    this.updateGameTime(scaledDelta);

    this.accumulator += scaledDelta;
    while (this.accumulator >= this.fixedDeltaTime) {
      this.fixedUpdate(this.fixedDeltaTime);
      this.accumulator -= this.fixedDeltaTime;
    }

    this.frameCount++;
    this.updateTimers(scaledDelta);
    this.processScheduledEvents();
  }

  private updateGameTime(delta: number): void {
    this.gameTime.totalSeconds += delta;
    this.gameTime.totalMilliseconds += delta * 1000;

    this.gameTime.seconds = Math.floor(this.gameTime.totalSeconds) % 60;
    this.gameTime.minutes = Math.floor(this.gameTime.totalSeconds / 60) % 60;
    this.gameTime.hours = Math.floor(this.gameTime.totalSeconds / 3600) % 24;

    this.gameTime.day = Math.floor(this.gameTime.totalSeconds / 86400) + 1;
    this.gameTime.week = Math.floor((this.gameTime.day - 1) / 7) + 1;
    this.gameTime.month = Math.floor((this.gameTime.day - 1) / 30) + 1;
    this.gameTime.year = Math.floor((this.gameTime.day - 1) / 365) + 1;
  }

  private fixedUpdate(delta: number): void {
  }

  private updateTimers(delta: number): void {
    for (const [id, timer] of this.timers) {
      timer.elapsed += delta;

      if (timer.elapsed >= timer.duration) {
        timer.callback();

        if (timer.repeat) {
          timer.elapsed = 0;
        } else {
          this.timers.delete(id);
        }
      }
    }
  }

  private processScheduledEvents(): void {
    const currentTimeMs = this.gameTime.totalMilliseconds;

    for (let i = this.scheduledEvents.length - 1; i >= 0; i--) {
      const event = this.scheduledEvents[i];
      if (currentTimeMs >= event.time) {
        event.callback();
        this.scheduledEvents.splice(i, 1);
      }
    }
  }

  public setTimeScale(scale: number): void {
    this.timeScale = Math.max(this.timeScaleConfig.min, Math.min(this.timeScaleConfig.max, scale));
  }

  public getTimeScale(): number {
    return this.timeScale;
  }

  public pause(): void {
    this.paused = true;
  }

  public resume(): void {
    this.paused = false;
  }

  public isPaused(): boolean {
    return this.paused;
  }

  public togglePause(): void {
    this.paused = !this.paused;
  }

  public getDeltaTime(): number {
    return this.deltaTime;
  }

  public getScaledDeltaTime(): number {
    return this.deltaTime * this.timeScale;
  }

  public getCurrentTime(): number {
    return this.currentTime;
  }

  public getRealTimeSinceStart(): number {
    return this.realTimeSinceStart;
  }

  public getFrameCount(): number {
    return this.frameCount;
  }

  public getFPS(): number {
    return this.fps;
  }

  public getGameTime(): GameTime {
    return { ...this.gameTime };
  }

  public setGameTime(time: Partial<GameTime>): void {
    Object.assign(this.gameTime, time);
  }

  public addTimer(id: string, duration: number, callback: () => void, repeat: boolean = false): void {
    this.timers.set(id, {
      duration,
      elapsed: 0,
      repeat,
      callback,
    });
  }

  public removeTimer(id: string): void {
    this.timers.delete(id);
  }

  public hasTimer(id: string): boolean {
    return this.timers.has(id);
  }

  public getTimerRemaining(id: string): number {
    const timer = this.timers.get(id);
    if (!timer) return 0;
    return Math.max(0, timer.duration - timer.elapsed);
  }

  public scheduleEvent(time: number, callback: () => void): void {
    this.scheduledEvents.push({ time, callback });
    this.scheduledEvents.sort((a, b) => a.time - b.time);
  }

  public clearScheduledEvents(): void {
    this.scheduledEvents = [];
  }

  public on(event: string, callback: (data: any) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(callback);
  }

  public off(event: string, callback: (data: any) => void): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      const index = callbacks.indexOf(callback);
      if (index !== -1) {
        callbacks.splice(index, 1);
      }
    }
  }

  private emit(event: string, data: any): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      for (const callback of callbacks) {
        callback(data);
      }
    }
  }

  public reset(): void {
    this.currentTime = 0;
    this.deltaTime = 0;
    this.frameCount = 0;
    this.accumulator = 0;
    this.gameTime = {
      hours: 8,
      minutes: 0,
      seconds: 0,
      totalSeconds: 0,
      totalMilliseconds: 0,
      day: 1,
      week: 1,
      month: 1,
      year: 1,
    };
    this.timers.clear();
    this.scheduledEvents = [];
  }

  public getTimeScaleConfig(): TimeScaleConfig {
    return { ...this.timeScaleConfig };
  }

  public setTimeScaleConfig(config: Partial<TimeScaleConfig>): void {
    this.timeScaleConfig = { ...this.timeScaleConfig, ...config };
    this.timeScale = Math.max(this.timeScaleConfig.min, Math.min(this.timeScaleConfig.max, this.timeScale));
  }
}

export class Stopwatch {
  private startTime: number = 0;
  private endTime: number = 0;
  private running: boolean = false;
  private elapsed: number = 0;

  public start(): void {
    this.startTime = performance.now();
    this.running = true;
  }

  public stop(): void {
    if (!this.running) return;
    this.endTime = performance.now();
    this.elapsed += (this.endTime - this.startTime) / 1000;
    this.running = false;
  }

  public reset(): void {
    this.startTime = 0;
    this.endTime = 0;
    this.running = false;
    this.elapsed = 0;
  }

  public getElapsed(): number {
    if (this.running) {
      return this.elapsed + (performance.now() - this.startTime) / 1000;
    }
    return this.elapsed;
  }

  public isRunning(): boolean {
    return this.running;
  }

  public lap(): number {
    if (!this.running) return this.elapsed;
    return (performance.now() - this.startTime) / 1000;
  }
}

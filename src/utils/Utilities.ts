import { Vector2D } from '../entities/Entity';

export interface LoggerConfig {
  enabled: boolean;
  minLevel: LogLevel;
  prefixes: boolean;
  timestamps: boolean;
  colors: boolean;
  maxBufferSize: number;
}

export enum LogLevel {
  DEBUG = 0,
  INFO = 1,
  WARNING = 2,
  ERROR = 3,
  CRITICAL = 4,
}

export class Logger {
  private config: LoggerConfig;
  private buffer: LogEntry[] = [];
  private static instance: Logger;

  constructor(config: Partial<LoggerConfig> = {}) {
    this.config = {
      enabled: config.enabled !== undefined ? config.enabled : true,
      minLevel: config.minLevel || LogLevel.INFO,
      prefixes: config.prefixes !== undefined ? config.prefixes : true,
      timestamps: config.timestamps !== undefined ? config.timestamps : true,
      colors: config.colors !== undefined ? config.colors : true,
      maxBufferSize: config.maxBufferSize || 1000,
    };

    Logger.instance = this;
  }

  public static getInstance(): Logger {
    if (!Logger.instance) {
      Logger.instance = new Logger();
    }
    return Logger.instance;
  }

  private log(level: LogLevel, message: string, data?: any): void {
    if (!this.config.enabled || level < this.config.minLevel) {
      return;
    }

    const entry: LogEntry = {
      level,
      message,
      timestamp: Date.now(),
      data,
    };

    this.buffer.push(entry);

    if (this.buffer.length > this.config.maxBufferSize) {
      this.buffer.shift();
    }

    const prefix = this.getPrefix(level);
    const timestamp = this.getTimestamp();
    const formattedMessage = this.formatMessage(prefix, timestamp, message);

    this.output(formattedMessage, level, data);
  }

  private getPrefix(level: LogLevel): string {
    if (!this.config.prefixes) return '';

    const prefixes: Record<LogLevel, string> = {
      [LogLevel.DEBUG]: '[DEBUG]',
      [LogLevel.INFO]: '[INFO]',
      [LogLevel.WARNING]: '[WARN]',
      [LogLevel.ERROR]: '[ERROR]',
      [LogLevel.CRITICAL]: '[CRITICAL]',
    };

    return prefixes[level];
  }

  private getTimestamp(): string {
    if (!this.config.timestamps) return '';

    const now = new Date();
    return now.toISOString();
  }

  private formatMessage(prefix: string, timestamp: string, message: string): string {
    const parts: string[] = [];

    if (timestamp) parts.push(timestamp);
    if (prefix) parts.push(prefix);
    parts.push(message);

    return parts.join(' ');
  }

  private output(message: string, level: LogLevel, data?: any): void {
    const style = this.getStyle(level);

    if (this.config.colors && style) {
      console.log(`%c${message}`, style);
    } else {
      console.log(message);
    }

    if (data !== undefined) {
      console.log(data);
    }
  }

  private getStyle(level: LogLevel): string | undefined {
    if (!this.config.colors) return undefined;

    const styles: Record<LogLevel, string> = {
      [LogLevel.DEBUG]: 'color: gray',
      [LogLevel.INFO]: 'color: blue',
      [LogLevel.WARNING]: 'color: orange',
      [LogLevel.ERROR]: 'color: red',
      [LogLevel.CRITICAL]: 'color: red; font-weight: bold',
    };

    return styles[level];
  }

  public debug(message: string, data?: any): void {
    this.log(LogLevel.DEBUG, message, data);
  }

  public info(message: string, data?: any): void {
    this.log(LogLevel.INFO, message, data);
  }

  public warning(message: string, data?: any): void {
    this.log(LogLevel.WARNING, message, data);
  }

  public error(message: string, data?: any): void {
    this.log(LogLevel.ERROR, message, data);
  }

  public critical(message: string, data?: any): void {
    this.log(LogLevel.CRITICAL, message, data);
  }

  public getBuffer(): LogEntry[] {
    return [...this.buffer];
  }

  public clearBuffer(): void {
    this.buffer = [];
  }

  public setConfig(config: Partial<LoggerConfig>): void {
    Object.assign(this.config, config);
  }

  public getConfig(): LoggerConfig {
    return { ...this.config };
  }
}

export interface LogEntry {
  level: LogLevel;
  message: string;
  timestamp: number;
  data?: any;
}

export class MathUtils {
  public static clamp(value: number, min: number, max: number): number {
    return Math.max(min, Math.min(max, value));
  }

  public static lerp(a: number, b: number, t: number): number {
    return a + (b - a) * t;
  }

  public static inverseLerp(a: number, b: number, value: number): number {
    return (value - a) / (b - a);
  }

  public static remap(value: number, inMin: number, inMax: number, outMin: number, outMax: number): number {
    return this.lerp(outMin, outMax, this.inverseLerp(inMin, inMax, value));
  }

  public static distance(x1: number, y1: number, x2: number, y2: number): number {
    return Math.sqrt((x2 - x1) ** 2 + (y2 - y1) ** 2);
  }

  public static distance2D(a: Vector2D, b: Vector2D): number {
    return this.distance(a.x, a.y, b.x, b.y);
  }

  public static manhattanDistance(x1: number, y1: number, x2: number, y2: number): number {
    return Math.abs(x2 - x1) + Math.abs(y2 - y1);
  }

  public static normalize(x: number, y: number): Vector2D {
    const len = Math.sqrt(x * x + y * y);
    if (len === 0) return { x: 0, y: 0 };
    return { x: x / len, y: y / len };
  }

  public static dot(a: Vector2D, b: Vector2D): number {
    return a.x * b.x + a.y * b.y;
  }

  public static cross(a: Vector2D, b: Vector2D): number {
    return a.x * b.y - a.y * b.x;
  }

  public static angleBetween(a: Vector2D, b: Vector2D): number {
    return Math.atan2(b.y - a.y, b.x - a.x);
  }

  public static rotatePoint(point: Vector2D, center: Vector2D, angle: number): Vector2D {
    const cos = Math.cos(angle);
    const sin = Math.sin(angle);
    const dx = point.x - center.x;
    const dy = point.y - center.y;

    return {
      x: center.x + dx * cos - dy * sin,
      y: center.y + dx * sin + dy * cos,
    };
  }

  public static randomRange(min: number, max: number): number {
    return min + Math.random() * (max - min);
  }

  public static randomInt(min: number, max: number): number {
    return Math.floor(this.randomRange(min, max + 1));
  }

  public static randomBool(probability: number = 0.5): boolean {
    return Math.random() < probability;
  }

  public static sign(value: number): number {
    if (value < 0) return -1;
    if (value > 0) return 1;
    return 0;
  }

  public static smoothstep(edge0: number, edge1: number, x: number): number {
    const t = this.clamp((x - edge0) / (edge1 - edge0), 0, 1);
    return t * t * (3 - 2 * t);
  }

  public static smootherstep(edge0: number, edge1: number, x: number): number {
    const t = this.clamp((x - edge0) / (edge1 - edge0), 0, 1);
    return t * t * t * (t * (t * 6 - 15) + 10);
  }

  public static cubicBezier(p0: number, p1: number, p2: number, p3: number, t: number): number {
    const t2 = t * t;
    const t3 = t2 * t;
    const mt = 1 - t;
    const mt2 = mt * mt;
    const mt3 = mt2 * mt;

    return mt3 * p0 + 3 * mt2 * t * p1 + 3 * mt * t2 * p2 + t3 * p3;
  }

  public static degToRad(degrees: number): number {
    return degrees * (Math.PI / 180);
  }

  public static radToDeg(radians: number): number {
    return radians * (180 / Math.PI);
  }

  public static approach(current: number, target: number, maxDelta: number): number {
    if (target > current) {
      return Math.min(current + maxDelta, target);
    }
    return Math.max(current - maxDelta, target);
  }

  public static repeat(value: number, length: number): number {
    return value - Math.floor(value / length) * length;
  }

  public static pingPong(value: number, length: number): number {
    const halfLength = length / 2;
    return length - Math.abs(this.repeat(value, length) - halfLength);
  }

  public static isPowerOfTwo(value: number): boolean {
    return (value & (value - 1)) === 0 && value !== 0;
  }

  public static nextPowerOfTwo(value: number): number {
    if (this.isPowerOfTwo(value)) return value;

    let power = 1;
    while (power < value) {
      power *= 2;
    }
    return power;
  }
}

export class ColorUtils {
  public static hexToRgb(hex: string): { r: number; g: number; b: number } | null {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result
      ? {
          r: parseInt(result[1], 16),
          g: parseInt(result[2], 16),
          b: parseInt(result[3], 16),
        }
      : null;
  }

  public static rgbToHex(r: number, g: number, b: number): string {
    return '#' + [r, g, b].map(x => {
      const hex = Math.round(x).toString(16);
      return hex.length === 1 ? '0' + hex : hex;
    }).join('');
  }

  public static lerpColor(color1: string, color2: string, t: number): string {
    const rgb1 = this.hexToRgb(color1);
    const rgb2 = this.hexToRgb(color2);

    if (!rgb1 || !rgb2) return color1;

    return this.rgbToHex(
      MathUtils.lerp(rgb1.r, rgb2.r, t),
      MathUtils.lerp(rgb1.g, rgb2.g, t),
      MathUtils.lerp(rgb1.b, rgb2.b, t)
    );
  }

  public static adjustBrightness(color: string, amount: number): string {
    const rgb = this.hexToRgb(color);
    if (!rgb) return color;

    return this.rgbToHex(
      MathUtils.clamp(rgb.r + amount, 0, 255),
      MathUtils.clamp(rgb.g + amount, 0, 255),
      MathUtils.clamp(rgb.b + amount, 0, 255)
    );
  }

  public static adjustSaturation(color: string, amount: number): string {
    const rgb = this.hexToRgb(color);
    if (!rgb) return color;

    const gray = (rgb.r + rgb.g + rgb.b) / 3;

    return this.rgbToHex(
      MathUtils.lerp(gray, rgb.r, 1 + amount),
      MathUtils.lerp(gray, rgb.g, 1 + amount),
      MathUtils.lerp(gray, rgb.b, 1 + amount)
    );
  }

  public static toGrayscale(color: string): string {
    const rgb = this.hexToRgb(color);
    if (!rgb) return color;

    const gray = (rgb.r + rgb.g + rgb.b) / 3;
    return this.rgbToHex(gray, gray, gray);
  }

  public static blendColors(color1: string, color2: string, ratio: number): string {
    return this.lerpColor(color1, color2, ratio);
  }
}

export class StringUtils {
  public static capitalize(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }

  public static truncate(str: string, maxLength: number, suffix: string = '...'): string {
    if (str.length <= maxLength) return str;
    return str.substring(0, maxLength - suffix.length) + suffix;
  }

  public static padLeft(str: string, length: number, char: string = ' '): string {
    return char.repeat(Math.max(0, length - str.length)) + str;
  }

  public static padRight(str: string, length: number, char: string = ' '): string {
    return str + char.repeat(Math.max(0, length - str.length));
  }

  public static repeat(str: string, count: number): string {
    return str.repeat(count);
  }

  public static reverse(str: string): string {
    return str.split('').reverse().join('');
  }

  public static removeWhitespace(str: string): string {
    return str.replace(/\s+/g, '');
  }

  public static slugify(str: string): string {
    return str
      .toLowerCase()
      .replace(/[^\w\s-]/g, '')
      .replace(/[\s_-]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }

  public static camelCase(str: string): string {
    return str
      .replace(/(?:^\w|[A-Z]|\b\w)/g, (word, index) => {
        return index === 0 ? word.toLowerCase() : word.toUpperCase();
      })
      .replace(/\s+/g, '');
  }

  public static snakeCase(str: string): string {
    return str
      .replace(/\W+/g, ' ')
      .split(/ |\B(?=[A-Z])/)
      .map(word => word.toLowerCase())
      .join('_');
  }

  public static kebabCase(str: string): string {
    return str
      .replace(/\W+/g, ' ')
      .split(/ |\B(?=[A-Z])/)
      .map(word => word.toLowerCase())
      .join('-');
  }

  public static formatNumber(num: number): string {
    return num.toLocaleString();
  }

  public static formatBytes(bytes: number, decimals: number = 2): string {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }

  public static formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);

    if (hours > 0) {
      return `${hours}:${StringUtils.padLeft(minutes.toString(), 2, '0')}:${StringUtils.padLeft(secs.toString(), 2, '0')}`;
    }
    return `${minutes}:${StringUtils.padLeft(secs.toString(), 2, '0')}`;
  }

  public static interpolate(template: string, values: Record<string, string | number>): string {
    return template.replace(/\{(\w+)\}/g, (match, key) => {
      return values[key] !== undefined ? String(values[key]) : match;
    });
  }
}

export class ArrayUtils {
  public static shuffle<T>(array: T[]): T[] {
    const result = [...array];
    for (let i = result.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [result[i], result[j]] = [result[j], result[i]];
    }
    return result;
  }

  public static unique<T>(array: T[]): T[] {
    return [...new Set(array)];
  }

  public static groupBy<T>(array: T[], key: keyof T): Record<string, T[]> {
    return array.reduce((result, item) => {
      const groupKey = String(item[key]);
      if (!result[groupKey]) {
        result[groupKey] = [];
      }
      result[groupKey].push(item);
      return result;
    }, {} as Record<string, T[]>);
  }

  public static chunk<T>(array: T[], size: number): T[][] {
    const result: T[][] = [];
    for (let i = 0; i < array.length; i += size) {
      result.push(array.slice(i, i + size));
    }
    return result;
  }

  public static flatten<T>(array: any[]): T[] {
    return array.reduce((acc, val) => {
      return Array.isArray(val) ? acc.concat(this.flatten(val)) : acc.concat(val);
    }, []);
  }

  public static remove<T>(array: T[], predicate: (item: T) => boolean): T[] {
    const removed: T[] = [];
    const result: T[] = [];

    for (const item of array) {
      if (predicate(item)) {
        removed.push(item);
      } else {
        result.push(item);
      }
    }

    return result;
  }

  public static intersection<T>(a: T[], b: T[]): T[] {
    const setB = new Set(b);
    return a.filter(item => setB.has(item));
  }

  public static difference<T>(a: T[], b: T[]): T[] {
    const setB = new Set(b);
    return a.filter(item => !setB.has(item));
  }

  public static union<T>(a: T[], b: T[]): T[] {
    return this.unique([...a, ...b]);
  }
}

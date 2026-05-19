export interface LocalizationConfig {
  defaultLanguage: string;
  fallbackLanguage: string;
  supportedLanguages: string[];
  loadAsync: boolean;
  cacheTranslations: boolean;
  missingKeyHandler: MissingKeyHandler;
  pluralizationRules: PluralizationRules;
  interpolationSettings: InterpolationSettings;
}

export type MissingKeyHandler = (key: string, locale: string) => string;

export interface InterpolationSettings {
  prefix: string;
  suffix: string;
  precision?: number;
}

export interface PluralizationRules {
  [locale: string]: (count: number) => PluralCategory;
}

export enum PluralCategory {
  ZERO = 'zero',
  ONE = 'one',
  TWO = 'two',
  FEW = 'few',
  MANY = 'many',
  OTHER = 'other',
}

export interface LocaleData {
  language: string;
  name: string;
  nativeName: string;
  direction: TextDirection;
  translations: Map<string, TranslationValue>;
  plurals: Map<string, PluralRule>;
  datetime: DateTimeFormats;
  numbers: NumberFormats;
}

export enum TextDirection {
  LTR = 'ltr',
  RTL = 'rtl',
}

export type TranslationValue = string | TranslationObject | TranslationArray;

export interface TranslationObject {
  [key: string]: TranslationValue;
}

export type TranslationArray = TranslationValue[];

export interface TranslationParams {
  [key: string]: unknown;
}

export interface TranslationResult {
  value: string;
  missing: boolean;
  locale: string;
}

export interface PluralRule {
  (count: number): PluralCategory;
}

export interface DateTimeFormats {
  date: DateFormat;
  time: TimeFormat;
  datetime: DateTimeFormat;
  relative: RelativeTimeFormat;
  calendar: CalendarFormat;
}

export interface DateFormat {
  full: string;
  long: string;
  medium: string;
  short: string;
}

export interface TimeFormat {
  full: string;
  long: string;
  medium: string;
  short: string;
}

export interface DateTimeFormat {
  full: DateTimeComponents;
  long: DateTimeComponents;
  medium: DateTimeComponents;
  short: DateTimeComponents;
}

export interface DateTimeComponents {
  year: string;
  month: string;
  day: string;
  hour: string;
  minute: string;
  second: string;
}

export interface RelativeTimeFormat {
  second: RelativeTimeUnit;
  minute: RelativeTimeUnit;
  hour: RelativeTimeUnit;
  day: RelativeTimeUnit;
  week: RelativeTimeUnit;
  month: RelativeTimeUnit;
  quarter: RelativeTimeUnit;
  year: RelativeTimeUnit;
  now: string;
}

export interface RelativeTimeUnit {
  past: string;
  future: string;
}

export interface CalendarFormat {
  lastDay: string;
  lastWeek: string;
  lastDayOfWeek: string;
  sameDay: string;
  nextDay: string;
  nextWeek: string;
}

export interface NumberFormats {
  decimal: NumberFormat;
  percent: NumberFormat;
  currency: CurrencyFormat;
  accounting: CurrencyFormat;
}

export interface NumberFormat {
  pattern: string;
  precision: number;
  separator: SeparatorFormat;
}

export interface CurrencyFormat {
  pattern: string;
  symbol: string;
  code: string;
  precision: number;
  separator: SeparatorFormat;
}

export interface SeparatorFormat {
  decimal: string;
  grouping: number[];
}

export interface LanguageInfo {
  code: string;
  name: string;
  nativeName: string;
  direction: TextDirection;
  enabled: boolean;
  completeness: number;
  lastUpdated: number;
  translators: string[];
}

export class LocalizationManager {
  private config: LocalizationConfig;
  private locales: Map<string, LocaleData> = new Map();
  private currentLocale: string;
  private fallbackLocale: string;
  private missingKeys: Set<string> = new Set();
  private pluralRules: Map<string, PluralRule> = new Map();
  private cache: Map<string, string> = new Map();
  private cacheEnabled: boolean = true;

  constructor(config: Partial<LocalizationConfig> = {}) {
    this.config = {
      defaultLanguage: config.defaultLanguage || 'en',
      fallbackLanguage: config.fallbackLanguage || 'en',
      supportedLanguages: config.supportedLanguages || ['en', 'es', 'fr', 'de', 'zh', 'ja', 'ko', 'ru', 'pt', 'it'],
      loadAsync: config.loadAsync ?? true,
      cacheTranslations: config.cacheTranslations ?? true,
      missingKeyHandler: config.missingKeyHandler || this.defaultMissingKeyHandler.bind(this),
      pluralizationRules: config.pluralizationRules || this.getDefaultPluralRules(),
      interpolationSettings: config.interpolationSettings || { prefix: '{{', suffix: '}}' },
    };

    this.currentLocale = this.config.defaultLanguage;
    this.fallbackLocale = this.config.fallbackLanguage;
    this.cacheEnabled = this.config.cacheTranslations;

    this.initializePluralRules();
  }

  private initializePluralRules(): void {
    Object.entries(this.config.pluralizationRules).forEach(([locale, rule]) => {
      this.pluralRules.set(locale, rule);
    });
  }

  private defaultMissingKeyHandler(key: string, locale: string): string {
    this.missingKeys.add(`${locale}:${key}`);
    return key;
  }

  private getDefaultPluralRules(): PluralizationRules {
    return {
      en: (count: number) => (count === 1 ? PluralCategory.ONE : PluralCategory.OTHER),
      es: (count: number) => (count === 1 ? PluralCategory.ONE : PluralCategory.OTHER),
      fr: (count: number) => (count === 1 ? PluralCategory.ONE : PluralCategory.OTHER),
      de: (count: number) => (count === 1 ? PluralCategory.ONE : PluralCategory.OTHER),
      ru: (count: number) => {
        const mod10 = count % 10;
        const mod100 = count % 100;
        if (mod10 === 1 && mod100 !== 11) return PluralCategory.ONE;
        if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return PluralCategory.FEW;
        return PluralCategory.MANY;
      },
      zh: () => PluralCategory.OTHER,
      ja: () => PluralCategory.OTHER,
      ko: () => PluralCategory.OTHER,
      ar: (count: number) => {
        if (count === 0) return PluralCategory.ZERO;
        if (count === 1) return PluralCategory.ONE;
        if (count === 2) return PluralCategory.TWO;
        if (count >= 3 && count <= 10) return PluralCategory.FEW;
        return PluralCategory.MANY;
      },
    };
  }

  public addLocale(locale: LocaleData): void {
    this.locales.set(locale.language, locale);
  }

  public removeLocale(language: string): void {
    this.locales.delete(language);
  }

  public setLocale(language: string): boolean {
    if (!this.locales.has(language)) {
      console.warn(`Locale '${language}' not found`);
      return false;
    }

    this.currentLocale = language;
    this.clearCache();
    return true;
  }

  public getLocale(): string {
    return this.currentLocale;
  }

  public getCurrentLocaleData(): LocaleData | undefined {
    return this.locales.get(this.currentLocale);
  }

  public translate(key: string, params?: TranslationParams, count?: number): TranslationResult {
    const cacheKey = this.getCacheKey(key, params, count);

    if (this.cacheEnabled && this.cache.has(cacheKey)) {
      return { value: this.cache.get(cacheKey)!, missing: false, locale: this.currentLocale };
    }

    let value = this.getTranslation(key, params, count);

    if (value === null && this.currentLocale !== this.fallbackLocale) {
      value = this.getTranslation(key, params, count, this.fallbackLocale);
    }

    if (value === null) {
      value = this.config.missingKeyHandler(key, this.currentLocale);
    }

    if (this.cacheEnabled) {
      this.cache.set(cacheKey, value);
    }

    return { value, missing: value === key || value === null, locale: this.currentLocale };
  }

  private getTranslation(key: string, params?: TranslationParams, count?: number, localeOverride?: string): string | null {
    const locale = localeOverride || this.currentLocale;
    const localeData = this.locales.get(locale);

    if (!localeData) {
      return null;
    }

    const keys = key.split('.');
    let value: unknown = localeData.translations.get(keys[0]);

    for (let i = 1; i < keys.length && value !== undefined; i++) {
      if (typeof value === 'object' && value !== null) {
        value = (value as Record<string, unknown>)[keys[i]];
      } else {
        value = undefined;
      }
    }

    if (value === undefined || value === null) {
      return null;
    }

    if (typeof value !== 'string') {
      value = this.resolvePlural(value as TranslationArray | TranslationObject, count);
    }

    if (typeof value !== 'string') {
      return null;
    }

    if (params) {
      value = this.interpolate(value, params);
    }

    return value;
  }

  private resolvePlural(value: TranslationArray | TranslationObject, count?: number): string {
    if (!count !== undefined) {
      const pluralRule = this.pluralRules.get(this.currentLocale) || this.pluralRules.get('en')!;
      const category = pluralRule(count);

      if (Array.isArray(value)) {
        const index = Object.values(PluralCategory).indexOf(category);
        return value[index % value.length] as string;
      }

      if (typeof value === 'object' && value !== null) {
        return (value as Record<string, string>)[category] || Object.values(value)[0];
      }
    }

    if (Array.isArray(value)) {
      return value[0] as string;
    }

    if (typeof value === 'object') {
      return Object.values(value)[0];
    }

    return value as string;
  }

  private interpolate(template: string, params: TranslationParams): string {
    const { prefix, suffix, precision } = this.config.interpolationSettings;

    let result = template;

    Object.entries(params).forEach(([key, value]) => {
      const placeholder = `${prefix}${key}${suffix}`;

      let formattedValue: string;

      if (typeof value === 'number' && precision !== undefined) {
        formattedValue = value.toFixed(precision);
      } else if (value instanceof Date) {
        formattedValue = this.formatDate(value);
      } else if (typeof value === 'object') {
        formattedValue = JSON.stringify(value);
      } else {
        formattedValue = String(value);
      }

      result = result.replace(new RegExp(this.escapeRegex(placeholder), 'g'), formattedValue);
    });

    return result;
  }

  private escapeRegex(str: string): string {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }

  private formatDate(date: Date): string {
    const localeData = this.locales.get(this.currentLocale);
    if (!localeData) {
      return date.toLocaleDateString();
    }

    const format = localeData.datetime.date.short;
    return date.toLocaleDateString(this.currentLocale, { year: 'numeric', month: 'short', day: 'numeric' });
  }

  private getCacheKey(key: string, params?: TranslationParams, count?: number): string {
    return `${key}:${JSON.stringify(params)}:${count}`;
  }

  public clearCache(): void {
    this.cache.clear();
  }

  public getMissingKeys(): string[] {
    return Array.from(this.missingKeys);
  }

  public formatNumber(value: number, options?: Intl.NumberFormatOptions): string {
    return new Intl.NumberFormat(this.currentLocale, options).format(value);
  }

  public formatCurrency(value: number, currency?: string): string {
    const localeData = this.locales.get(this.currentLocale);
    const format = localeData?.numbers.currency || {
      pattern: '₡{{amount}}',
      symbol: '$',
      code: 'USD',
      precision: 2,
      separator: { decimal: '.', grouping: [3] },
    };

    const formatted = this.formatNumber(value, { minimumFractionDigits: format.precision, maximumFractionDigits: format.precision });
    return format.pattern.replace('{{amount}}', formatted).replace('{{symbol}}', format.symbol);
  }

  public formatPercent(value: number): string {
    return new Intl.NumberFormat(this.currentLocale, { style: 'percent' }).format(value);
  }

  public formatDateTime(date: Date, options?: Intl.DateTimeFormatOptions): string {
    return new Intl.DateTimeFormat(this.currentLocale, options).format(date);
  }

  public formatRelativeTime(date: Date, baseDate?: Date): string {
    const localeData = this.locales.get(this.currentLocale);
    const rtf = localeData?.datetime.relative;

    const now = baseDate || new Date();
    const diffMs = date.getTime() - now.getTime();
    const diffSec = Math.round(diffMs / 1000);
    const diffMin = Math.round(diffSec / 60);
    const diffHour = Math.round(diffMin / 60);
    const diffDay = Math.round(diffHour / 24);

    if (Math.abs(diffSec) < 60) {
      return rtf?.second?.past.replace('{{count}}', String(diffSec)) || `${diffSec} seconds ago`;
    }
    if (Math.abs(diffMin) < 60) {
      return rtf?.minute?.past.replace('{{count}}', String(diffMin)) || `${diffMin} minutes ago`;
    }
    if (Math.abs(diffHour) < 24) {
      return rtf?.hour?.past.replace('{{count}}', String(diffHour)) || `${diffHour} hours ago`;
    }
    if (Math.abs(diffDay) < 7) {
      return rtf?.day?.past.replace('{{count}}', String(diffDay)) || `${diffDay} days ago`;
    }

    return this.formatDateTime(date);
  }

  public formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    const localeData = this.locales.get(this.currentLocale);

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }
    return `${minutes}:${secs.toString().padStart(2, '0')}`;
  }

  public t(key: string, params?: TranslationParams, count?: number): string {
    return this.translate(key, params, count).value;
  }

  public getSupportedLanguages(): LanguageInfo[] {
    return this.config.supportedLanguages.map(code => ({
      code,
      name: this.getLanguageName(code),
      nativeName: this.getNativeLanguageName(code),
      direction: this.getDirection(code),
      enabled: this.locales.has(code),
      completeness: this.calculateCompleteness(code),
      lastUpdated: Date.now(),
      translators: [],
    }));
  }

  private getLanguageName(code: string): string {
    const names: Record<string, string> = {
      en: 'English', es: 'Spanish', fr: 'French', de: 'German', zh: 'Chinese',
      ja: 'Japanese', ko: 'Korean', ru: 'Russian', pt: 'Portuguese', it: 'Italian',
    };
    return names[code] || code;
  }

  private getNativeLanguageName(code: string): string {
    const names: Record<string, string> = {
      en: 'English', es: 'Español', fr: 'Français', de: 'Deutsch', zh: '中文',
      ja: '日本語', ko: '한국어', ru: 'Русский', pt: 'Português', it: 'Italiano',
    };
    return names[code] || code;
  }

  private getDirection(code: string): TextDirection {
    const rtlLanguages = ['ar', 'he', 'fa', 'ur'];
    return rtlLanguages.includes(code) ? TextDirection.RTL : TextDirection.LTR;
  }

  private calculateCompleteness(locale: string): number {
    const localeData = this.locales.get(locale);
    const englishLocale = this.locales.get('en');

    if (!localeData || !englishLocale) {
      return 0;
    }

    const totalKeys = englishLocale.translations.size;
    const translatedKeys = localeData.translations.size;

    return totalKeys > 0 ? (translatedKeys / totalKeys) * 100 : 0;
  }

  public loadTranslations(language: string, translations: Record<string, unknown>): void {
    const localeData = this.locales.get(language) || this.createDefaultLocale(language);
    const flatTranslations = this.flattenObject(translations);
    flatTranslations.forEach((value, key) => {
      localeData.translations.set(key, value);
    });
    this.locales.set(language, localeData);
    this.clearCache();
  }

  private createDefaultLocale(language: string): LocaleData {
    return {
      language,
      name: this.getLanguageName(language),
      nativeName: this.getNativeLanguageName(language),
      direction: this.getDirection(language),
      translations: new Map(),
      plurals: new Map(),
      datetime: {
        date: { full: 'EEEE, MMMM d, yyyy', long: 'MMMM d, yyyy', medium: 'MMM d, yyyy', short: 'M/d/yyyy' },
        time: { full: 'h:mm:ss a', long: 'h:mm:ss a', medium: 'h:mm a', short: 'h:mm a' },
        datetime: { full: { year: 'numeric', month: 'long', day: 'numeric', hour: 'numeric', minute: 'numeric' }, long: { year: 'numeric', month: 'long', day: 'numeric', hour: 'numeric', minute: 'numeric' }, medium: { year: 'numeric', month: 'short', day: 'numeric', hour: 'numeric', minute: 'numeric' }, short: { year: '2-digit', month: 'short', day: 'numeric', hour: 'numeric', minute: 'numeric' } },
        relative: { second: { past: '{{count}} seconds ago', future: 'in {{count}} seconds' }, minute: { past: '{{count}} minutes ago', future: 'in {{count}} minutes' }, hour: { past: '{{count}} hours ago', future: 'in {{count}} hours' }, day: { past: '{{count}} days ago', future: 'in {{count}} days' }, week: { past: '{{count}} weeks ago', future: 'in {{count}} weeks' }, month: { past: '{{count}} months ago', future: 'in {{count}} months' }, quarter: { past: '{{count}} quarters ago', future: 'in {{count}} quarters' }, year: { past: '{{count}} years ago', future: 'in {{count}} years' }, now: 'now' },
        calendar: { lastDay: '[Yesterday at] h:mm a', lastWeek: 'EEEE [at] h:mm a', lastDayOfWeek: '[Last] dddd [at] h:mm a', sameDay: '[Today at] h:mm a', nextDay: '[Tomorrow at] h:mm a', nextWeek: 'EEEE [at] h:mm a' },
      },
      numbers: {
        decimal: { pattern: '#,##0.###', precision: 3, separator: { decimal: '.', grouping: [3] } },
        percent: { pattern: '#,##0%', precision: 0, separator: { decimal: '.', grouping: [3] } },
        currency: { pattern: '₡{{amount}}', symbol: '$', code: 'USD', precision: 2, separator: { decimal: '.', grouping: [3] } },
        accounting: { pattern: '₡{{amount}}', symbol: '$', code: 'USD', precision: 2, separator: { decimal: '.', grouping: [3] } },
      },
    };
  }

  private flattenObject(obj: unknown, prefix: string = ''): Map<string, string> {
    const result = new Map<string, string>();

    if (typeof obj === 'string') {
      result.set(prefix, obj);
      return result;
    }

    if (typeof obj === 'object' && obj !== null) {
      Object.entries(obj).forEach(([key, value]) => {
        const newPrefix = prefix ? `${prefix}.${key}` : key;
        this.flattenObject(value, newPrefix).forEach((v, k) => result.set(k, v));
      });
    }

    return result;
  }

  public destroy(): void {
    this.locales.clear();
    this.cache.clear();
    this.missingKeys.clear();
    this.pluralRules.clear();
  }
}

export class CinematicEngine {
  private scenes: Map<string, CinematicScene> = new Map();
  private activeScene: CinematicScene | null = null;
  private sceneQueue: string[] = [];
  private callbacks: Map<CinematicEvent, Function[]> = new Map();
  private canvas: HTMLCanvasElement | null = null;
  private ctx: CanvasRenderingContext2D | null = null;
  private isPlaying: boolean = false;
  private currentTime: number = 0;
  private playbackSpeed: number = 1;
  private isSkipping: boolean = false;
  private volume: number = 1;

  constructor() {
    this.initializeCallbacks();
  }

  private initializeCallbacks(): void {
    Object.values(CinematicEvent).forEach(event => {
      this.callbacks.set(event, []);
    });
  }

  public createScene(config: SceneConfig): CinematicScene {
    const scene: CinematicScene = {
      id: config.id || `scene_${Date.now()}`,
      name: config.name,
      duration: config.duration,
      scenes: config.scenes || [],
      audio: config.audio || [],
      subtitles: config.subtitles || [],
      transitions: config.transitions || [],
      skippable: config.skippable ?? true,
      autoPlay: config.autoPlay ?? false,
      fadeIn: config.fadeIn ?? 0,
      fadeOut: config.fadeOut ?? 0,
      loop: config.loop ?? false,
      onComplete: config.onComplete,
      currentTime: 0,
      isPlaying: false,
    };

    this.scenes.set(scene.id, scene);
    return scene;
  }

  public playScene(sceneId: string): void {
    const scene = this.scenes.get(sceneId);
    if (!scene) {
      console.error(`Scene not found: ${sceneId}`);
      return;
    }

    this.activeScene = scene;
    this.activeScene.currentTime = 0;
    this.activeScene.isPlaying = true;
    this.isPlaying = true;
    this.isSkipping = false;

    this.emit(CinematicEvent.PLAY, { sceneId });
  }

  public stopScene(): void {
    if (!this.activeScene) return;

    this.activeScene.isPlaying = false;
    this.activeScene.currentTime = 0;
    this.isPlaying = false;

    this.emit(CinematicEvent.STOP, { sceneId: this.activeScene.id });
    this.activeScene = null;
  }

  public pauseScene(): void {
    if (!this.activeScene || !this.isPlaying) return;

    this.activeScene.isPlaying = false;
    this.isPlaying = false;

    this.emit(CinematicEvent.PAUSE, { sceneId: this.activeScene.id });
  }

  public resumeScene(): void {
    if (!this.activeScene || this.isPlaying) return;

    this.activeScene.isPlaying = true;
    this.isPlaying = true;

    this.emit(CinematicEvent.RESUME, { sceneId: this.activeScene.id });
  }

  public skipScene(): void {
    if (!this.activeScene || !this.activeScene.skippable) return;

    this.isSkipping = true;
    this.activeScene.currentTime = this.activeScene.duration;
  }

  public seekScene(time: number): void {
    if (!this.activeScene) return;

    this.activeScene.currentTime = Math.max(0, Math.min(time, this.activeScene.duration));
    this.emit(CinematicEvent.SEEK, { sceneId: this.activeScene.id, time });
  }

  public setPlaybackSpeed(speed: number): void {
    this.playbackSpeed = Math.max(0.25, Math.min(speed, 4));
  }

  public setVolume(volume: number): void {
    this.volume = Math.max(0, Math.min(volume, 1));
  }

  public update(deltaTime: number): void {
    if (!this.activeScene || !this.activeScene.isPlaying) return;

    const scaledDelta = deltaTime * this.playbackSpeed;
    this.currentTime += scaledDelta;
    this.activeScene.currentTime += scaledDelta;

    const currentScene = this.getCurrentSubScene();

    if (currentScene) {
      if (currentScene.onEnter && this.activeScene.currentTime >= currentScene.startTime && !currentScene.hasEntered) {
        currentScene.hasEntered = true;
        currentScene.onEnter();
        this.emit(CinematicEvent.SCENE_ENTER, { sceneId: this.activeScene!.id, subSceneId: currentScene.id });
      }

      if (currentScene.onExit && this.activeScene.currentTime >= currentScene.endTime && !currentScene.hasExited) {
        currentScene.hasExited = true;
        currentScene.onExit();
        this.emit(CinematicEvent.SCENE_EXIT, { sceneId: this.activeScene!.id, subSceneId: currentScene.id });
      }
    }

    if (this.activeScene.currentTime >= this.activeScene.duration) {
      if (this.activeScene.loop) {
        this.activeScene.currentTime = 0;
        this.resetSubScenes();
        this.emit(CinematicEvent.LOOP, { sceneId: this.activeScene.id });
      } else {
        this.completeScene();
      }
    }

    if (this.isSkipping) {
      this.emit(CinematicEvent.SKIP, { sceneId: this.activeScene.id });
    }
  }

  private getCurrentSubScene(): SubScene | null {
    if (!this.activeScene) return null;

    return this.activeScene.scenes.find(
      s => this.activeScene!.currentTime >= s.startTime && this.activeScene!.currentTime < s.endTime
    ) || null;
  }

  private resetSubScenes(): void {
    if (!this.activeScene) return;

    this.activeScene.scenes.forEach(scene => {
      scene.hasEntered = false;
      scene.hasExited = false;
    });
  }

  private completeScene(): void {
    if (!this.activeScene) return;

    this.activeScene.isPlaying = false;
    this.isPlaying = false;

    if (this.activeScene.fadeOut > 0) {
      this.emit(CinematicEvent.FADE_OUT_START, { sceneId: this.activeScene.id });
    }

    this.emit(CinematicEvent.COMPLETE, { sceneId: this.activeScene.id });

    if (this.activeScene.onComplete) {
      this.activeScene.onComplete();
    }

    this.activeScene = null;
  }

  public render(): void {
    if (!this.activeScene || !this.ctx) return;

    const progress = this.activeScene.currentTime / this.activeScene.duration;

    if (this.activeScene.fadeIn > 0 && this.activeScene.currentTime < this.activeScene.fadeIn) {
      const fadeProgress = this.activeScene.currentTime / this.activeScene.fadeIn;
      this.ctx.globalAlpha = fadeProgress;
    } else if (this.activeScene.fadeOut > 0 && this.activeScene.currentTime > this.activeScene.duration - this.activeScene.fadeOut) {
      const fadeProgress = (this.activeScene.duration - this.activeScene.currentTime) / this.activeScene.fadeOut;
      this.ctx.globalAlpha = fadeProgress;
    } else {
      this.ctx.globalAlpha = 1;
    }

    const currentScene = this.getCurrentSubScene();
    if (currentScene) {
      this.renderSubScene(currentScene);
    }

    this.renderSubtitles(progress);
    this.renderEffects(progress);
    this.renderTransitions(progress);

    this.ctx.globalAlpha = 1;
  }

  private renderSubScene(scene: SubScene): void {
    if (!this.ctx) return;

    this.ctx.save();

    if (scene.camera) {
      this.ctx.translate(scene.camera.x, scene.camera.y);
      this.ctx.scale(scene.camera.zoom, scene.camera.zoom);
      this.ctx.rotate(scene.camera.rotation * Math.PI / 180);
    }

    scene.elements.forEach(element => {
      this.renderElement(element);
    });

    this.ctx.restore();
  }

  private renderElement(element: SceneElement): void {
    if (!this.ctx) return;

    switch (element.type) {
      case ElementType.IMAGE:
        this.renderImage(element);
        break;
      case ElementType.TEXT:
        this.renderText(element);
        break;
      case ElementType.RECT:
        this.renderRect(element);
        break;
      case ElementType.CIRCLE:
        this.renderCircle(element);
        break;
      case ElementType.VIDEO:
        this.renderVideo(element);
        break;
      case ElementType.GROUP:
        this.renderGroup(element);
        break;
    }
  }

  private renderImage(element: SceneElement): void {
    // Image rendering logic
  }

  private renderText(element: SceneElement): void {
    if (!this.ctx || !element.text) return;

    const config = element.text;
    this.ctx.font = `${config.size}px ${config.font}`;
    this.ctx.fillStyle = config.color;
    this.ctx.textAlign = config.align as CanvasTextAlign;
    this.ctx.textBaseline = config.baseline as CanvasTextBaseline;
    this.ctx.fillText(config.content, config.x || 0, config.y || 0);
  }

  private renderRect(element: SceneElement): void {
    if (!this.ctx || !element.rect) return;

    const config = element.rect;
    if (config.fill) {
      this.ctx.fillStyle = config.fill;
      this.ctx.fillRect(config.x || 0, config.y || 0, config.width || 0, config.height || 0);
    }
    if (config.stroke) {
      this.ctx.strokeStyle = config.stroke;
      this.ctx.lineWidth = config.strokeWidth || 1;
      this.ctx.strokeRect(config.x || 0, config.y || 0, config.width || 0, config.height || 0);
    }
  }

  private renderCircle(element: SceneElement): void {
    if (!this.ctx || !element.circle) return;

    const config = element.circle;
    this.ctx.beginPath();
    this.ctx.arc(config.x || 0, config.y || 0, config.radius || 0, 0, Math.PI * 2);
    if (config.fill) {
      this.ctx.fillStyle = config.fill;
      this.ctx.fill();
    }
    if (config.stroke) {
      this.ctx.strokeStyle = config.stroke;
      this.ctx.lineWidth = config.strokeWidth || 1;
      this.ctx.stroke();
    }
  }

  private renderVideo(element: SceneElement): void {
    // Video rendering logic
  }

  private renderGroup(element: SceneElement): void {
    if (!element.children) return;

    this.ctx!.save();
    if (element.transform) {
      this.ctx!.translate(element.transform.x || 0, element.transform.y || 0);
      this.ctx!.rotate((element.transform.rotation || 0) * Math.PI / 180);
      this.ctx!.scale(element.transform.scaleX || 1, element.transform.scaleY || 1);
    }

    element.children.forEach(child => this.renderElement(child));
    this.ctx!.restore();
  }

  private renderSubtitles(_progress: number): void {
    // Subtitle rendering
  }

  private renderEffects(_progress: number): void {
    // Effects rendering
  }

  private renderTransitions(_progress: number): void {
    // Transition rendering
  }

  public on(event: CinematicEvent, callback: Function): void {
    const callbacks = this.callbacks.get(event);
    if (callbacks) {
      callbacks.push(callback);
    }
  }

  public off(event: CinematicEvent, callback: Function): void {
    const callbacks = this.callbacks.get(event);
    if (callbacks) {
      const index = callbacks.indexOf(callback);
      if (index > -1) {
        callbacks.splice(index, 1);
      }
    }
  }

  private emit(event: CinematicEvent, data: Record<string, unknown>): void {
    const callbacks = this.callbacks.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  public getActiveScene(): CinematicScene | null {
    return this.activeScene;
  }

  public getScene(sceneId: string): CinematicScene | undefined {
    return this.scenes.get(sceneId);
  }

  public getAllScenes(): CinematicScene[] {
    return Array.from(this.scenes.values());
  }

  public removeScene(sceneId: string): boolean {
    return this.scenes.delete(sceneId);
  }

  public isScenePlaying(): boolean {
    return this.isPlaying;
  }

  public getCurrentTime(): number {
    return this.currentTime;
  }

  public getProgress(): number {
    if (!this.activeScene) return 0;
    return this.activeScene.currentTime / this.activeScene.duration;
  }

  public setCanvas(canvas: HTMLCanvasElement): void {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d');
  }

  public destroy(): void {
    this.stopScene();
    this.scenes.clear();
    this.callbacks.forEach(callbacks => callbacks.length = 0);
    this.callbacks.clear();
    this.canvas = null;
    this.ctx = null;
  }
}

export enum CinematicEvent {
  PLAY = 'play',
  PAUSE = 'pause',
  RESUME = 'resume',
  STOP = 'stop',
  COMPLETE = 'complete',
  SKIP = 'skip',
  SEEK = 'seek',
  LOOP = 'loop',
  SCENE_ENTER = 'scene_enter',
  SCENE_EXIT = 'scene_exit',
  FADE_IN_START = 'fade_in_start',
  FADE_IN_COMPLETE = 'fade_in_complete',
  FADE_OUT_START = 'fade_out_start',
  FADE_OUT_COMPLETE = 'fade_out_complete',
}

export interface SceneConfig {
  id?: string;
  name: string;
  duration: number;
  scenes?: SubScene[];
  audio?: CinematicAudio[];
  subtitles?: CinematicSubtitle[];
  transitions?: CinematicTransition[];
  skippable?: boolean;
  autoPlay?: boolean;
  fadeIn?: number;
  fadeOut?: number;
  loop?: boolean;
  onComplete?: () => void;
}

export interface CinematicScene {
  id: string;
  name: string;
  duration: number;
  scenes: SubScene[];
  audio: CinematicAudio[];
  subtitles: CinematicSubtitle[];
  transitions: CinematicTransition[];
  skippable: boolean;
  autoPlay: boolean;
  fadeIn: number;
  fadeOut: number;
  loop: boolean;
  onComplete?: () => void;
  currentTime: number;
  isPlaying: boolean;
}

export interface SubScene {
  id: string;
  name: string;
  startTime: number;
  endTime: number;
  elements: SceneElement[];
  camera?: SceneCamera;
  onEnter?: () => void;
  onExit?: () => void;
  hasEntered: boolean;
  hasExited: boolean;
}

export interface SceneElement {
  id: string;
  type: ElementType;
  transform?: ElementTransform;
  opacity?: number;
  visible?: boolean;
  image?: ImageElement;
  text?: TextElement;
  rect?: RectElement;
  circle?: CircleElement;
  video?: VideoElement;
  group?: GroupElement;
  children?: SceneElement[];
}

export enum ElementType {
  IMAGE = 'image',
  TEXT = 'text',
  RECT = 'rect',
  CIRCLE = 'circle',
  VIDEO = 'video',
  GROUP = 'group',
}

export interface ElementTransform {
  x: number;
  y: number;
  rotation: number;
  scaleX: number;
  scaleY: number;
}

export interface ImageElement {
  src: string;
  width?: number;
  height?: number;
  crop?: { x: number; y: number; width: number; height: number };
}

export interface TextElement {
  content: string;
  x?: number;
  y?: number;
  font: string;
  size: number;
  color: string;
  align?: string;
  baseline?: string;
  maxWidth?: number;
  lineHeight?: number;
}

export interface RectElement {
  x?: number;
  y?: number;
  width?: number;
  height?: number;
  fill?: string;
  stroke?: string;
  strokeWidth?: number;
  cornerRadius?: number;
}

export interface CircleElement {
  x?: number;
  y?: number;
  radius?: number;
  fill?: string;
  stroke?: string;
  strokeWidth?: number;
}

export interface VideoElement {
  src: string;
  width?: number;
  height?: number;
  loop?: boolean;
  muted?: boolean;
}

export interface GroupElement {
  clip?: { x: number; y: number; width: number; height: number };
}

export interface SceneCamera {
  x: number;
  y: number;
  zoom: number;
  rotation: number;
  targetX?: number;
  targetY?: number;
  targetZoom?: number;
  interpolation?: CameraInterpolation;
}

export enum CameraInterpolation {
  LINEAR = 'linear',
  EASE_IN = 'ease_in',
  EASE_OUT = 'ease_out',
  EASE_IN_OUT = 'ease_in_out',
}

export interface CinematicAudio {
  src: string;
  startTime: number;
  endTime: number;
  volume: number;
  loop?: boolean;
}

export interface CinematicSubtitle {
  text: string;
  startTime: number;
  endTime: number;
  speaker?: string;
  position?: { x: number; y: number };
  style?: TextElement;
}

export interface CinematicTransition {
  type: TransitionType;
  startTime: number;
  endTime: number;
  color?: string;
  direction?: string;
}

export enum TransitionType {
  NONE = 'none',
  FADE = 'fade',
  DISSOLVE = 'dissolve',
  WIPE = 'wipe',
  SLIDE = 'slide',
  ZOOM = 'zoom',
  BLIND = 'blind',
  FLY = 'fly',
}

export const createLocalizationManager = (config?: Partial<LocalizationConfig>): LocalizationManager => {
  return new LocalizationManager(config);
};

export const createCinematicEngine = (): CinematicEngine => {
  return new CinematicEngine();
};

export default LocalizationManager;

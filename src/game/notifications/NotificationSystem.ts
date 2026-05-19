import { EventEmitter } from '../core/EventEmitter';
import { GameConfig } from '../core/GameConfig';

export enum NotificationType {
  Info = 'info',
  Warning = 'warning',
  Error = 'error',
  Success = 'success',
  Achievement = 'achievement',
  Event = 'event',
  Tutorial = 'tutorial',
  Combat = 'combat',
  Resource = 'resource',
  System = 'system',
}

export enum NotificationPriority {
  Low = 0,
  Normal = 1,
  High = 2,
  Critical = 3,
}

export enum NotificationCategory {
  General = 'general',
  Combat = 'combat',
  Economy = 'economy',
  Technology = 'technology',
  Diplomacy = 'diplomacy',
  Achievement = 'achievement',
  Tutorial = 'tutorial',
  System = 'system',
}

export enum NotificationDisplayStyle {
  Toast = 'toast',
  Modal = 'modal',
  Banner = 'banner',
  Alert = 'alert',
  Floating = 'floating',
  Chat = 'chat',
}

export enum SoundType {
  Info = 'notification_info',
  Warning = 'notification_warning',
  Error = 'notification_error',
  Success = 'notification_success',
  Achievement = 'notification_achievement',
  Combat = 'notification_combat',
  Custom = 'notification_custom',
}

export enum AnimationType {
  FadeIn = 'fadeIn',
  SlideIn = 'slideIn',
  Bounce = 'bounce',
  Pop = 'pop',
  Shake = 'shake',
  Glow = 'glow',
}

export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  priority: NotificationPriority;
  category: NotificationCategory;
  timestamp: number;
  duration: number;
  isRead: boolean;
  isDismissed: boolean;
  metadata?: Record<string, any>;
  source?: string;
  icon?: string;
  color?: string;
  displayStyle: NotificationDisplayStyle;
  soundType?: SoundType;
  animationType?: AnimationType;
  tags?: string[];
  groupId?: string;
  maxDisplayTime?: number;
  requiresInteraction?: boolean;
  actions?: NotificationAction[];
}

export interface NotificationAction {
  id: string;
  label: string;
  callback: () => void;
  icon?: string;
  isPrimary?: boolean;
  isDestructive?: boolean;
}

export interface NotificationConfig {
  maxNotifications: number;
  defaultDuration: number;
  maxQueueSize: number;
  enableSound: boolean;
  enableAnimations: boolean;
  enablePersistence: boolean;
  enableGrouping: boolean;
  groupSimilarNotifications: boolean;
  maxHistorySize: number;
  showTimestamps: boolean;
  autoHideDelay: number;
  position: NotificationPosition;
  layout: NotificationLayout;
  style: NotificationStyle;
}

export interface NotificationStyle {
  theme: NotificationTheme;
  fontFamily?: string;
  fontSize?: number;
  borderRadius?: number;
  shadow?: boolean;
  backdropBlur?: boolean;
  customColors?: Record<NotificationType, string>;
}

export enum NotificationTheme {
  Light = 'light',
  Dark = 'dark',
  Game = 'game',
  SciFi = 'scifi',
  Fantasy = 'fantasy',
}

export interface NotificationPosition {
  x: 'left' | 'center' | 'right';
  y: 'top' | 'center' | 'bottom';
  offsetX?: number;
  offsetY?: number;
}

export interface NotificationLayout {
  maxWidth: number;
  maxHeight: number;
  spacing: number;
  itemHeight: number;
}

export interface AlertThreshold {
  type: NotificationType;
  threshold: number;
  timeWindow: number;
  action: AlertAction;
}

export enum AlertAction {
  Queue = 'queue',
  Replace = 'replace',
  Merge = 'merge',
  Dismiss = 'dismiss',
}

export interface NotificationTemplate {
  id: string;
  name: string;
  type: NotificationType;
  category: NotificationCategory;
  titleTemplate: string;
  messageTemplate: string;
  priority: NotificationPriority;
  displayStyle: NotificationDisplayStyle;
  soundType?: SoundType;
  animationType?: AnimationType;
  variables?: TemplateVariable[];
}

export interface TemplateVariable {
  name: string;
  type: 'string' | 'number' | 'boolean';
  required: boolean;
  defaultValue?: any;
}

export interface NotificationGroup {
  id: string;
  name: string;
  type: NotificationType;
  notifications: Notification[];
  maxSize: number;
  collapseEnabled: boolean;
  isExpanded: boolean;
  lastUpdated: number;
  unreadCount: number;
}

export interface AchievementNotification extends Notification {
  achievementId: string;
  achievementName: string;
  achievementDescription: string;
  iconUrl?: string;
  reward?: AchievementReward;
  progress?: number;
  totalProgress?: number;
  tier?: AchievementTier;
  isSecret?: boolean;
}

export enum AchievementTier {
  Bronze = 'bronze',
  Silver = 'silver',
  Gold = 'gold',
  Platinum = 'platinum',
  Diamond = 'diamond',
}

export interface AchievementReward {
  type: 'currency' | 'experience' | 'item' | 'badge';
  amount?: number;
  itemId?: string;
  itemName?: string;
  description?: string;
}

export interface EventNotification extends Notification {
  eventId: string;
  eventName: string;
  eventType: EventType;
  startTime?: number;
  endTime?: number;
  location?: string;
  participants?: string[];
  rewards?: EventReward[];
  urgency?: EventUrgency;
}

export enum EventType {
  Battle = 'battle',
  Tournament = 'tournament',
  Quest = 'quest',
  Festival = 'festival',
  Raid = 'raid',
  Competition = 'competition',
}

export enum EventUrgency {
  Low = 'low',
  Medium = 'medium',
  High = 'high',
  Critical = 'critical',
}

export interface EventReward {
  type: string;
  amount?: number;
  itemId?: string;
  itemName?: string;
  rarity?: string;
}

export interface TutorialNotification extends Notification {
  tutorialId: string;
  stepNumber: number;
  totalSteps: number;
  targetElement?: string;
  arrowPosition?: ArrowPosition;
  spotlightEnabled: boolean;
  skipAvailable: boolean;
  progressEnabled: boolean;
  autoAdvance: boolean;
  advanceDelay: number;
}

export interface ArrowPosition {
  x: number;
  y: number;
  direction: 'up' | 'down' | 'left' | 'right';
}

export interface NotificationFilter {
  types?: NotificationType[];
  categories?: NotificationCategory[];
  priorities?: NotificationPriority[];
  searchQuery?: string;
  tags?: string[];
  source?: string;
  dateRange?: DateRange;
  isRead?: boolean;
  isDismissed?: boolean;
}

export interface DateRange {
  start: number;
  end: number;
}

export interface NotificationStatistics {
  totalNotifications: number;
  unreadCount: number;
  byType: Record<NotificationType, number>;
  byCategory: Record<NotificationCategory, number>;
  byPriority: Record<NotificationPriority, number>;
  averageResponseTime: number;
  mostFrequentType: NotificationType;
  mostFrequentCategory: NotificationCategory;
}

export class NotificationSound {
  private audioContext: AudioContext | null = null;
  private sounds: Map<SoundType, AudioBuffer> = new Map();
  private volume: number = 0.7;
  private enabled: boolean = true;
  private customSounds: Map<string, string> = new Map();

  constructor() {
    this.initializeAudioContext();
  }

  private initializeAudioContext(): void {
    try {
      this.audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
    } catch (error) {
      console.warn('Web Audio API not supported:', error);
    }
  }

  public async loadSound(type: SoundType, url: string): Promise<void> {
    if (!this.audioContext) return;

    try {
      const response = await fetch(url);
      const arrayBuffer = await response.arrayBuffer();
      const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
      this.sounds.set(type, audioBuffer);
    } catch (error) {
      console.error(`Failed to load sound ${type} from ${url}:`, error);
    }
  }

  public async loadCustomSound(id: string, url: string): Promise<void> {
    this.customSounds.set(id, url);
    await this.loadSound(SoundType.Custom, url);
  }

  public play(type: SoundType, volume?: number): void {
    if (!this.enabled || !this.audioContext) return;

    const audioBuffer = this.sounds.get(type);
    if (!audioBuffer) {
      this.playGeneratedSound(type, volume);
      return;
    }

    const source = this.audioContext.createBufferSource();
    source.buffer = audioBuffer;

    const gainNode = this.audioContext.createGain();
    gainNode.gain.value = volume ?? this.volume;

    source.connect(gainNode);
    gainNode.connect(this.audioContext.destination);
    source.start(0);
  }

  private playGeneratedSound(type: SoundType, volume?: number): void {
    if (!this.audioContext) return;

    const oscillator = this.audioContext.createOscillator();
    const gainNode = this.audioContext.createGain();

    oscillator.connect(gainNode);
    gainNode.connect(this.audioContext.destination);

    const vol = volume ?? this.volume;

    switch (type) {
      case SoundType.Info:
        oscillator.frequency.value = 440;
        oscillator.type = 'sine';
        gainNode.gain.value = vol * 0.3;
        break;
      case SoundType.Warning:
        oscillator.frequency.value = 587;
        oscillator.type = 'triangle';
        gainNode.gain.value = vol * 0.5;
        break;
      case SoundType.Error:
        oscillator.frequency.value = 220;
        oscillator.type = 'sawtooth';
        gainNode.gain.value = vol * 0.6;
        break;
      case SoundType.Success:
        oscillator.frequency.value = 523;
        oscillator.type = 'sine';
        gainNode.gain.value = vol * 0.4;
        break;
      case SoundType.Achievement:
        oscillator.frequency.value = 659;
        oscillator.type = 'sine';
        gainNode.gain.value = vol * 0.5;
        break;
      case SoundType.Combat:
        oscillator.frequency.value = 150;
        oscillator.type = 'square';
        gainNode.gain.value = vol * 0.7;
        break;
      default:
        oscillator.frequency.value = 440;
        oscillator.type = 'sine';
        gainNode.gain.value = vol * 0.3;
    }

    gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.5);

    oscillator.start(this.audioContext.currentTime);
    oscillator.stop(this.audioContext.currentTime + 0.5);
  }

  public setVolume(volume: number): void {
    this.volume = Math.max(0, Math.min(1, volume));
  }

  public getVolume(): number {
    return this.volume;
  }

  public setEnabled(enabled: boolean): void {
    this.enabled = enabled;
  }

  public isEnabled(): boolean {
    return this.enabled;
  }

  public dispose(): void {
    if (this.audioContext) {
      this.audioContext.close();
      this.audioContext = null;
    }
    this.sounds.clear();
    this.customSounds.clear();
  }
}

export class NotificationAnimation {
  private animations: Map<string, AnimationConfig> = new Map();
  private activeAnimations: Map<string, AnimationInstance> = new Map();

  constructor() {
    this.initializeDefaultAnimations();
  }

  private initializeDefaultAnimations(): void {
    this.registerAnimation(AnimationType.FadeIn, {
      duration: 300,
      easing: 'ease-out',
      keyframes: {
        opacity: [0, 1],
      },
    });

    this.registerAnimation(AnimationType.SlideIn, {
      duration: 400,
      easing: 'ease-out',
      keyframes: {
        transform: ['translateX(-100%)', 'translateX(0)'],
        opacity: [0, 1],
      },
    });

    this.registerAnimation(AnimationType.Bounce, {
      duration: 500,
      easing: 'ease-out',
      keyframes: {
        transform: ['scale(0)', 'scale(1.2)', 'scale(1)'],
        opacity: [0, 1, 1, 1],
      },
    });

    this.registerAnimation(AnimationType.Pop, {
      duration: 300,
      easing: 'ease-out',
      keyframes: {
        transform: ['scale(0)', 'scale(1.1)', 'scale(1)'],
        opacity: [0, 1, 1, 1],
      },
    });

    this.registerAnimation(AnimationType.Shake, {
      duration: 400,
      easing: 'ease-out',
      keyframes: {
        transform: [
          'translateX(0)',
          'translateX(-10px)',
          'translateX(10px)',
          'translateX(-10px)',
          'translateX(10px)',
          'translateX(0)',
        ],
      },
    });

    this.registerAnimation(AnimationType.Glow, {
      duration: 600,
      easing: 'ease-in-out',
      keyframes: {
        boxShadow: [
          '0 0 0 rgba(255, 255, 0, 0)',
          '0 0 20px rgba(255, 255, 0, 0.5)',
          '0 0 0 rgba(255, 255, 0, 0)',
        ],
      },
    });
  }

  public registerAnimation(type: AnimationType, config: AnimationConfig): void {
    this.animations.set(type, config);
  }

  public async play(
    element: HTMLElement,
    animationType: AnimationType,
    options?: AnimationOptions
  ): Promise<void> {
    const config = this.animations.get(animationType);
    if (!config) {
      console.warn(`Animation ${animationType} not found`);
      return;
    }

    const instance: AnimationInstance = {
      id: this.generateId(),
      element,
      type: animationType,
      startTime: performance.now(),
      duration: options?.duration ?? config.duration,
      easing: options?.easing ?? config.easing,
    };

    this.activeAnimations.set(instance.id, instance);

    return new Promise((resolve) => {
      const animate = (currentTime: number) => {
        const elapsed = currentTime - instance.startTime;
        const progress = Math.min(elapsed / instance.duration, 1);
        const easedProgress = this.applyEasing(progress, instance.easing);

        this.applyKeyframes(element, config.keyframes, easedProgress);

        if (progress < 1) {
          requestAnimationFrame(animate);
        } else {
          this.activeAnimations.delete(instance.id);
          if (options?.onComplete) {
            options.onComplete();
          }
          resolve();
        }
      };

      requestAnimationFrame(animate);
    });
  }

  private applyEasing(time: number, easing: string): number {
    switch (easing) {
      case 'ease-in':
        return time * time;
      case 'ease-out':
        return time * (2 - time);
      case 'ease-in-out':
        return time < 0.5 ? 2 * time * time : -1 + (4 - 2 * time) * time;
      case 'linear':
        return time;
      default:
        return time;
    }
  }

  private applyKeyframes(
    element: HTMLElement,
    keyframes: Record<string, any[]>,
    progress: number
  ): void {
    const keyframeCount = Object.values(keyframes)[0]?.length || 0;
    const keyframeIndex = progress * (keyframeCount - 1);
    const lowerIndex = Math.floor(keyframeIndex);
    const upperIndex = Math.ceil(keyframeIndex);
    const localProgress = keyframeIndex - lowerIndex;

    for (const [property, values] of Object.entries(keyframes)) {
      if (property === 'transform') {
        const lowerValue = this.parseTransform(values[lowerIndex] || values[0]);
        const upperValue = this.parseTransform(values[upperIndex] || values[lowerIndex]);
        const interpolated = this.interpolateTransform(lowerValue, upperValue, localProgress);
        element.style.transform = interpolated;
      } else if (property === 'opacity') {
        const lowerValue = values[lowerIndex] || 0;
        const upperValue = values[upperIndex] || lowerValue;
        element.style.opacity = String(this.lerp(lowerValue, upperValue, localProgress));
      } else if (property === 'boxShadow') {
        element.style.boxShadow = values[Math.floor(progress * (values.length - 1))] || '';
      }
    }
  }

  private parseTransform(value: string): Record<string, number> {
    const result: Record<string, number> = {};
    const translateMatch = value.match(/translate[X|Y|Z]\(([^)]+)\)/);
    const scaleMatch = value.match(/scale\(([^)]+)\)/);

    if (translateMatch) {
      result.translate = parseFloat(translateMatch[1]);
    }
    if (scaleMatch) {
      result.scale = parseFloat(scaleMatch[1]);
    }

    return result;
  }

  private interpolateTransform(
    lower: Record<string, number>,
    upper: Record<string, number>,
    progress: number
  ): string {
    const parts: string[] = [];

    if (lower.translate !== undefined || upper.translate !== undefined) {
      const value = this.lerp(lower.translate || 0, upper.translate || 0, progress);
      parts.push(`translateX(${value}px)`);
    }

    if (lower.scale !== undefined || upper.scale !== undefined) {
      const value = this.lerp(lower.scale || 1, upper.scale || 1, progress);
      parts.push(`scale(${value})`);
    }

    return parts.join(' ');
  }

  private lerp(a: number, b: number, t: number): number {
    return a + (b - a) * t;
  }

  private generateId(): string {
    return `anim_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  public cancelAnimation(id: string): void {
    this.activeAnimations.delete(id);
  }

  public cancelAllAnimations(): void {
    this.activeAnimations.clear();
  }

  public dispose(): void {
    this.cancelAllAnimations();
    this.animations.clear();
  }
}

export interface AnimationConfig {
  duration: number;
  easing: string;
  keyframes: Record<string, any[]>;
}

export interface AnimationInstance {
  id: string;
  element: HTMLElement;
  type: AnimationType;
  startTime: number;
  duration: number;
  easing: string;
}

export interface AnimationOptions {
  duration?: number;
  easing?: string;
  onComplete?: () => void;
}

export class NotificationHistory {
  private history: Notification[] = [];
  private maxSize: number;
  private listeners: Array<(notification: Notification) => void> = [];

  constructor(maxSize: number = 1000) {
    this.maxSize = maxSize;
    this.loadFromStorage();
  }

  public add(notification: Notification): void {
    this.history.unshift(notification);

    if (this.history.length > this.maxSize) {
      this.history = this.history.slice(0, this.maxSize);
    }

    this.saveToStorage();
    this.notifyListeners(notification);
  }

  public getAll(): Notification[] {
    return [...this.history];
  }

  public getRecent(count: number): Notification[] {
    return this.history.slice(0, count);
  }

  public getById(id: string): Notification | undefined {
    return this.history.find((n) => n.id === id);
  }

  public getByType(type: NotificationType): Notification[] {
    return this.history.filter((n) => n.type === type);
  }

  public getByCategory(category: NotificationCategory): Notification[] {
    return this.history.filter((n) => n.category === category);
  }

  public getByDateRange(start: number, end: number): Notification[] {
    return this.history.filter((n) => n.timestamp >= start && n.timestamp <= end);
  }

  public search(query: string): Notification[] {
    const lowerQuery = query.toLowerCase();
    return this.history.filter(
      (n) =>
        n.title.toLowerCase().includes(lowerQuery) ||
        n.message.toLowerCase().includes(lowerQuery)
    );
  }

  public clear(): void {
    this.history = [];
    this.saveToStorage();
  }

  public remove(id: string): boolean {
    const index = this.history.findIndex((n) => n.id === id);
    if (index !== -1) {
      this.history.splice(index, 1);
      this.saveToStorage();
      return true;
    }
    return false;
  }

  public getStatistics(): NotificationStatistics {
    const stats: NotificationStatistics = {
      totalNotifications: this.history.length,
      unreadCount: this.history.filter((n) => !n.isRead).length,
      byType: {} as Record<NotificationType, number>,
      byCategory: {} as Record<NotificationCategory, number>,
      byPriority: {} as Record<NotificationPriority, number>,
      averageResponseTime: 0,
      mostFrequentType: NotificationType.Info,
      mostFrequentCategory: NotificationCategory.General,
    };

    Object.values(NotificationType).forEach((type) => {
      stats.byType[type] = this.history.filter((n) => n.type === type).length;
    });

    Object.values(NotificationCategory).forEach((category) => {
      stats.byCategory[category] = this.history.filter((n) => n.category === category).length;
    });

    Object.values(NotificationPriority).forEach((priority) => {
      stats.byPriority[priority] = this.history.filter((n) => n.priority === priority).length;
    });

    const typeCounts = Object.entries(stats.byType).sort((a, b) => b[1] - a[1]);
    if (typeCounts.length > 0) {
      stats.mostFrequentType = typeCounts[0][0] as NotificationType;
    }

    const categoryCounts = Object.entries(stats.byCategory).sort((a, b) => b[1] - a[1]);
    if (categoryCounts.length > 0) {
      stats.mostFrequentCategory = categoryCounts[0][0] as NotificationCategory;
    }

    return stats;
  }

  public subscribe(listener: (notification: Notification) => void): () => void {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  private notifyListeners(notification: Notification): void {
    this.listeners.forEach((listener) => listener(notification));
  }

  private saveToStorage(): void {
    try {
      const data = JSON.stringify(this.history);
      localStorage.setItem('notification_history', data);
    } catch (error) {
      console.error('Failed to save notification history:', error);
    }
  }

  private loadFromStorage(): void {
    try {
      const data = localStorage.getItem('notification_history');
      if (data) {
        this.history = JSON.parse(data);
      }
    } catch (error) {
      console.error('Failed to load notification history:', error);
      this.history = [];
    }
  }

  public exportHistory(): string {
    return JSON.stringify(this.history, null, 2);
  }

  public importHistory(data: string): boolean {
    try {
      const imported = JSON.parse(data);
      if (Array.isArray(imported)) {
        this.history = imported;
        this.saveToStorage();
        return true;
      }
      return false;
    } catch (error) {
      console.error('Failed to import notification history:', error);
      return false;
    }
  }
}

export class NotificationFilterManager {
  private filters: Map<string, NotificationFilter> = new Map();
  private activeFilters: NotificationFilter = {};
  private defaultFilter: NotificationFilter = {};

  constructor() {
    this.loadFilters();
  }

  public createFilter(id: string, filter: NotificationFilter): void {
    this.filters.set(id, { ...filter });
    this.saveFilters();
  }

  public getFilter(id: string): NotificationFilter | undefined {
    return this.filters.get(id);
  }

  public deleteFilter(id: string): boolean {
    const result = this.filters.delete(id);
    if (result) {
      this.saveFilters();
    }
    return result;
  }

  public listFilters(): string[] {
    return Array.from(this.filters.keys());
  }

  public setActiveFilter(filter: NotificationFilter): void {
    this.activeFilters = { ...filter };
  }

  public getActiveFilter(): NotificationFilter {
    return { ...this.activeFilters };
  }

  public clearActiveFilter(): void {
    this.activeFilters = {};
  }

  public setDefaultFilter(filter: NotificationFilter): void {
    this.defaultFilter = { ...filter };
  }

  public getDefaultFilter(): NotificationFilter {
    return { ...this.defaultFilter };
  }

  public applyFilter(notifications: Notification[]): Notification[] {
    const filter = Object.keys(this.activeFilters).length > 0 ? this.activeFilters : this.defaultFilter;

    if (Object.keys(filter).length === 0) {
      return notifications;
    }

    return notifications.filter((notification) => {
      if (filter.types && filter.types.length > 0 && !filter.types.includes(notification.type)) {
        return false;
      }

      if (
        filter.categories &&
        filter.categories.length > 0 &&
        !filter.categories.includes(notification.category)
      ) {
        return false;
      }

      if (
        filter.priorities &&
        filter.priorities.length > 0 &&
        !filter.priorities.includes(notification.priority)
      ) {
        return false;
      }

      if (filter.searchQuery) {
        const query = filter.searchQuery.toLowerCase();
        const matches =
          notification.title.toLowerCase().includes(query) ||
          notification.message.toLowerCase().includes(query);
        if (!matches) return false;
      }

      if (filter.tags && filter.tags.length > 0) {
        const hasTag = notification.tags?.some((tag) => filter.tags!.includes(tag));
        if (!hasTag) return false;
      }

      if (filter.source && notification.source !== filter.source) {
        return false;
      }

      if (filter.dateRange) {
        if (
          notification.timestamp < filter.dateRange.start ||
          notification.timestamp > filter.dateRange.end
        ) {
          return false;
        }
      }

      if (filter.isRead !== undefined && notification.isRead !== filter.isRead) {
        return false;
      }

      if (filter.isDismissed !== undefined && notification.isDismissed !== filter.isDismissed) {
        return false;
      }

      return true;
    });
  }

  private saveFilters(): void {
    try {
      const data = JSON.stringify(Array.from(this.filters.entries()));
      localStorage.setItem('notification_filters', data);
    } catch (error) {
      console.error('Failed to save notification filters:', error);
    }
  }

  private loadFilters(): void {
    try {
      const data = localStorage.getItem('notification_filters');
      if (data) {
        const entries = JSON.parse(data);
        this.filters = new Map(entries);
      }
    } catch (error) {
      console.error('Failed to load notification filters:', error);
    }
  }
}

export class AlertQueue {
  private queue: Notification[] = [];
  private maxSize: number;
  private thresholds: AlertThreshold[] = [];
  private listeners: Array<(notification: Notification) => void> = [];

  constructor(maxSize: number = 100) {
    this.maxSize = maxSize;
  }

  public enqueue(notification: Notification): boolean {
    const threshold = this.getThreshold(notification.type);
    const recentCount = this.getRecentCount(notification.type, threshold?.timeWindow || 0);

    if (threshold && recentCount >= threshold.threshold) {
      switch (threshold.action) {
        case AlertAction.Dismiss:
          return false;
        case AlertAction.Replace:
          this.removeOldestOfType(notification.type);
          break;
        case AlertAction.Merge:
          this.mergeSimilar(notification);
          return true;
        case AlertAction.Queue:
        default:
          if (this.queue.length >= this.maxSize) {
            this.dequeue();
          }
          break;
      }
    }

    if (this.queue.length >= this.maxSize) {
      const criticalIndex = this.queue.findIndex((n) => n.priority === NotificationPriority.Critical);
      if (criticalIndex !== -1) {
        this.queue.splice(criticalIndex, 1);
      } else {
        this.dequeue();
      }
    }

    const insertIndex = this.findInsertPosition(notification);
    this.queue.splice(insertIndex, 0, notification);

    this.notifyListeners(notification);
    return true;
  }

  public dequeue(): Notification | undefined {
    const notification = this.queue.shift();
    return notification;
  }

  public peek(): Notification | undefined {
    return this.queue[0];
  }

  public clear(): void {
    this.queue = [];
  }

  public size(): number {
    return this.queue.length;
  }

  public isEmpty(): boolean {
    return this.queue.length === 0;
  }

  public isFull(): boolean {
    return this.queue.length >= this.maxSize;
  }

  public getAll(): Notification[] {
    return [...this.queue];
  }

  public getByType(type: NotificationType): Notification[] {
    return this.queue.filter((n) => n.type === type);
  }

  public remove(id: string): boolean {
    const index = this.queue.findIndex((n) => n.id === id);
    if (index !== -1) {
      this.queue.splice(index, 1);
      return true;
    }
    return false;
  }

  public addThreshold(threshold: AlertThreshold): void {
    this.thresholds.push(threshold);
  }

  public removeThreshold(type: NotificationType): void {
    this.thresholds = this.thresholds.filter((t) => t.type !== type);
  }

  public getThreshold(type: NotificationType): AlertThreshold | undefined {
    return this.thresholds.find((t) => t.type === type);
  }

  public clearThresholds(): void {
    this.thresholds = [];
  }

  private getRecentCount(type: NotificationType, timeWindow: number): number {
    const now = Date.now();
    return this.queue.filter(
      (n) => n.type === type && now - n.timestamp < timeWindow
    ).length;
  }

  private findInsertPosition(notification: Notification): number {
    for (let i = 0; i < this.queue.length; i++) {
      if (notification.priority > this.queue[i].priority) {
        return i;
      }
      if (notification.priority === this.queue[i].priority) {
        if (notification.timestamp < this.queue[i].timestamp) {
          return i;
        }
      }
    }
    return this.queue.length;
  }

  private removeOldestOfType(type: NotificationType): void {
    const index = this.queue.findIndex((n) => n.type === type);
    if (index !== -1) {
      this.queue.splice(index, 1);
    }
  }

  private mergeSimilar(notification: Notification): void {
    const similar = this.queue.find(
      (n) =>
        n.type === notification.type &&
        n.title === notification.title &&
        !n.metadata?.merged
    );

    if (similar) {
      similar.metadata = {
        ...similar.metadata,
        merged: true,
        mergeCount: (similar.metadata?.mergeCount || 1) + 1,
        lastMerged: Date.now(),
      };
      similar.message = `${similar.message} (+${similar.metadata.mergeCount})`;
      similar.timestamp = Date.now();
    }
  }

  public subscribe(listener: (notification: Notification) => void): () => void {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  private notifyListeners(notification: Notification): void {
    this.listeners.forEach((listener) => listener(notification));
  }

  public setMaxSize(size: number): void {
    this.maxSize = size;
    while (this.queue.length > this.maxSize) {
      this.dequeue();
    }
  }

  public getMaxSize(): number {
    return this.maxSize;
  }
}

export class NotificationGroupManager {
  private groups: Map<string, NotificationGroup> = new Map();
  private listeners: Array<(group: NotificationGroup) => void> = [];

  constructor() {
    this.loadGroups();
  }

  public createGroup(
    id: string,
    name: string,
    type: NotificationType,
    maxSize: number = 10
  ): NotificationGroup {
    const group: NotificationGroup = {
      id,
      name,
      type,
      notifications: [],
      maxSize,
      collapseEnabled: true,
      isExpanded: true,
      lastUpdated: Date.now(),
      unreadCount: 0,
    };

    this.groups.set(id, group);
    this.saveGroups();
    return group;
  }

  public getGroup(id: string): NotificationGroup | undefined {
    return this.groups.get(id);
  }

  public getAllGroups(): NotificationGroup[] {
    return Array.from(this.groups.values());
  }

  public deleteGroup(id: string): boolean {
    const result = this.groups.delete(id);
    if (result) {
      this.saveGroups();
    }
    return result;
  }

  public addToGroup(groupId: string, notification: Notification): boolean {
    const group = this.groups.get(groupId);
    if (!group) return false;

    notification.groupId = groupId;
    group.notifications.unshift(notification);

    if (group.notifications.length > group.maxSize) {
      group.notifications = group.notifications.slice(0, group.maxSize);
    }

    group.lastUpdated = Date.now();
    group.unreadCount = group.notifications.filter((n) => !n.isRead).length;

    this.saveGroups();
    this.notifyListeners(group);
    return true;
  }

  public removeFromGroup(groupId: string, notificationId: string): boolean {
    const group = this.groups.get(groupId);
    if (!group) return false;

    const index = group.notifications.findIndex((n) => n.id === notificationId);
    if (index === -1) return false;

    group.notifications.splice(index, 1);
    group.lastUpdated = Date.now();
    group.unreadCount = group.notifications.filter((n) => !n.isRead).length;

    this.saveGroups();
    this.notifyListeners(group);
    return true;
  }

  public clearGroup(groupId: string): boolean {
    const group = this.groups.get(groupId);
    if (!group) return false;

    group.notifications = [];
    group.lastUpdated = Date.now();
    group.unreadCount = 0;

    this.saveGroups();
    this.notifyListeners(group);
    return true;
  }

  public toggleGroupExpansion(groupId: string): boolean {
    const group = this.groups.get(groupId);
    if (!group) return false;

    group.isExpanded = !group.isExpanded;
    this.saveGroups();
    this.notifyListeners(group);
    return true;
  }

  public markGroupAsRead(groupId: string): boolean {
    const group = this.groups.get(groupId);
    if (!group) return false;

    group.notifications.forEach((n) => {
      n.isRead = true;
    });
    group.unreadCount = 0;

    this.saveGroups();
    this.notifyListeners(group);
    return true;
  }

  public getGroupNotifications(groupId: string): Notification[] {
    const group = this.groups.get(groupId);
    return group ? [...group.notifications] : [];
  }

  public getUnreadCount(groupId: string): number {
    const group = this.groups.get(groupId);
    return group?.unreadCount || 0;
  }

  public getTotalUnreadCount(): number {
    let total = 0;
    this.groups.forEach((group) => {
      total += group.unreadCount;
    });
    return total;
  }

  public subscribe(listener: (group: NotificationGroup) => void): () => void {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  private notifyListeners(group: NotificationGroup): void {
    this.listeners.forEach((listener) => listener(group));
  }

  private saveGroups(): void {
    try {
      const data = JSON.stringify(Array.from(this.groups.entries()));
      localStorage.setItem('notification_groups', data);
    } catch (error) {
      console.error('Failed to save notification groups:', error);
    }
  }

  private loadGroups(): void {
    try {
      const data = localStorage.getItem('notification_groups');
      if (data) {
        const entries = JSON.parse(data);
        this.groups = new Map(entries);
      }
    } catch (error) {
      console.error('Failed to load notification groups:', error);
    }
  }
}

export class ToastRenderer {
  private container: HTMLElement | null = null;
  private activeToasts: Map<string, HTMLElement> = new Map();
  private animationManager: NotificationAnimation;
  private config: ToastConfig;
  private listeners: Map<string, Array<(toast: ToastInstance) => void>> = new Map();

  constructor(animationManager: NotificationAnimation, config?: Partial<ToastConfig>) {
    this.animationManager = animationManager;
    this.config = { ...defaultToastConfig, ...config };
    this.initializeContainer();
  }

  private initializeContainer(): void {
    if (this.container) return;

    this.container = document.createElement('div');
    this.container.id = 'notification-toast-container';
    this.container.style.cssText = `
      position: fixed;
      z-index: ${this.config.zIndex};
      ${this.config.position}: ${this.config.offset}px;
      display: flex;
      flex-direction: ${this.config.direction};
      gap: ${this.config.spacing}px;
      pointer-events: none;
      font-family: inherit;
    `;

    document.body.appendChild(this.container);
  }

  public async render(notification: Notification): Promise<string> {
    if (!this.container) {
      this.initializeContainer();
    }

    const toast = this.createToastElement(notification);
    const toastId = notification.id;

    this.activeToasts.set(toastId, toast);
    this.container!.appendChild(toast);

    await this.animationManager.play(
      toast,
      notification.animationType || AnimationType.SlideIn
    );

    if (!notification.requiresInteraction && notification.duration > 0) {
      setTimeout(() => {
        this.dismiss(toastId);
      }, notification.duration);
    }

    this.setupToastEvents(toastId, notification);
    this.notifyListeners('render', { id: toastId, notification, element: toast });

    return toastId;
  }

  private createToastElement(notification: Notification): HTMLElement {
    const toast = document.createElement('div');
    toast.className = `notification-toast notification-toast-${notification.type}`;
    toast.style.cssText = this.getToastStyles(notification);

    const content = document.createElement('div');
    content.className = 'toast-content';

    if (this.config.showIcon && notification.icon) {
      const icon = document.createElement('img');
      icon.src = notification.icon;
      icon.className = 'toast-icon';
      icon.style.cssText = `
        width: ${this.config.iconSize}px;
        height: ${this.config.iconSize}px;
        margin-right: 8px;
      `;
      content.appendChild(icon);
    }

    const textContent = document.createElement('div');
    textContent.className = 'toast-text-content';

    const title = document.createElement('div');
    title.className = 'toast-title';
    title.textContent = notification.title;
    title.style.cssText = `
      font-weight: bold;
      margin-bottom: 4px;
      font-size: ${this.config.titleFontSize}px;
    `;
    textContent.appendChild(title);

    const message = document.createElement('div');
    message.className = 'toast-message';
    message.textContent = notification.message;
    message.style.cssText = `
      font-size: ${this.config.messageFontSize}px;
      opacity: 0.9;
    `;
    textContent.appendChild(message);

    content.appendChild(textContent);

    if (this.config.showTimestamp) {
      const timestamp = document.createElement('div');
      timestamp.className = 'toast-timestamp';
      timestamp.textContent = this.formatTimestamp(notification.timestamp);
      timestamp.style.cssText = `
        font-size: 10px;
        opacity: 0.6;
        margin-top: 4px;
      `;
      textContent.appendChild(timestamp);
    }

    if (notification.actions && notification.actions.length > 0) {
      const actions = document.createElement('div');
      actions.className = 'toast-actions';
      actions.style.cssText = `
        display: flex;
        gap: 8px;
        margin-top: 8px;
      `;

      notification.actions.forEach((action) => {
        const button = document.createElement('button');
        button.textContent = action.label;
        button.className = `toast-action-btn ${action.isPrimary ? 'primary' : ''} ${action.isDestructive ? 'destructive' : ''}`;
        button.style.cssText = `
          padding: 4px 12px;
          border: none;
          border-radius: 4px;
          cursor: pointer;
          font-size: 12px;
          pointer-events: auto;
          background: ${action.isPrimary ? '#4CAF50' : '#666'};
          color: white;
        `;
        button.onclick = () => {
          action.callback();
          this.dismiss(notification.id);
        };
        actions.appendChild(button);
      });

      content.appendChild(actions);
    }

    toast.appendChild(content);

    const closeButton = document.createElement('button');
    closeButton.className = 'toast-close';
    closeButton.innerHTML = '&times;';
    closeButton.style.cssText = `
      position: absolute;
      top: 4px;
      right: 4px;
      background: none;
      border: none;
      color: inherit;
      font-size: 20px;
      cursor: pointer;
      opacity: 0.6;
      pointer-events: auto;
    `;
    closeButton.onclick = () => this.dismiss(notification.id);
    toast.appendChild(closeButton);

    return toast;
  }

  private getToastStyles(notification: Notification): string {
    const typeColors = {
      [NotificationType.Info]: '#2196F3',
      [NotificationType.Warning]: '#FF9800',
      [NotificationType.Error]: '#F44336',
      [NotificationType.Success]: '#4CAF50',
      [NotificationType.Achievement]: '#FFD700',
      [NotificationType.Event]: '#9C27B0',
      [NotificationType.Tutorial]: '#00BCD4',
      [NotificationType.Combat]: '#FF5722',
      [NotificationType.Resource]: '#795548',
      [NotificationType.System]: '#607D8B',
    };

    const backgroundColor = notification.color || typeColors[notification.type] || '#333';
    const isDark = this.isColorDark(backgroundColor);
    const textColor = isDark ? '#fff' : '#000';

    return `
      position: relative;
      display: flex;
      align-items: flex-start;
      padding: ${this.config.padding}px;
      background: ${this.config.useGradient ? this.createGradient(backgroundColor) : backgroundColor};
      color: ${textColor};
      border-radius: ${this.config.borderRadius}px;
      box-shadow: ${this.config.shadow ? '0 4px 12px rgba(0,0,0,0.15)' : 'none'};
      max-width: ${this.config.maxWidth}px;
      min-width: ${this.config.minWidth}px;
      pointer-events: auto;
      transition: all 0.3s ease;
      overflow: hidden;
      font-family: inherit;
    `;
  }

  private createGradient(color: string): string {
    return `linear-gradient(135deg, ${color} 0%, ${this.adjustColor(color, -20)} 100%)`;
  }

  private adjustColor(color: string, amount: number): string {
    const hex = color.replace('#', '');
    const r = Math.max(0, Math.min(255, parseInt(hex.substr(0, 2), 16) + amount));
    const g = Math.max(0, Math.min(255, parseInt(hex.substr(2, 2), 16) + amount));
    const b = Math.max(0, Math.min(255, parseInt(hex.substr(4, 2), 16) + amount));
    return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
  }

  private isColorDark(color: string): boolean {
    const hex = color.replace('#', '');
    const r = parseInt(hex.substr(0, 2), 16);
    const g = parseInt(hex.substr(2, 2), 16);
    const b = parseInt(hex.substr(4, 2), 16);
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
    return luminance < 0.5;
  }

  private formatTimestamp(timestamp: number): string {
    const date = new Date(timestamp);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  private setupToastEvents(toastId: string, notification: Notification): void {
    const toast = this.activeToasts.get(toastId);
    if (!toast) return;

    toast.addEventListener('mouseenter', () => {
      toast.style.transform = 'scale(1.02)';
      toast.style.boxShadow = '0 6px 16px rgba(0,0,0,0.2)';
    });

    toast.addEventListener('mouseleave', () => {
      toast.style.transform = 'scale(1)';
      toast.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
    });
  }

  public async dismiss(toastId: string): Promise<void> {
    const toast = this.activeToasts.get(toastId);
    if (!toast) return;

    await this.animationManager.play(toast, AnimationType.FadeIn, {
      duration: 200,
      onComplete: () => {
        toast.remove();
        this.activeToasts.delete(toastId);
        this.notifyListeners('dismiss', { id: toastId });
      },
    });
  }

  public dismissAll(): void {
    const promises: Promise<void>[] = [];
    this.activeToasts.forEach((toast, id) => {
      promises.push(this.dismiss(id));
    });
    return Promise.all(promises) as any;
  }

  public getActiveCount(): number {
    return this.activeToasts.size;
  }

  public subscribe(event: string, listener: (toast: ToastInstance) => void): () => void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);

    return () => {
      const eventListeners = this.listeners.get(event);
      if (eventListeners) {
        const index = eventListeners.indexOf(listener);
        if (index !== -1) {
          eventListeners.splice(index, 1);
        }
      }
    };
  }

  private notifyListeners(event: string, toast: ToastInstance): void {
    const eventListeners = this.listeners.get(event);
    if (eventListeners) {
      eventListeners.forEach((listener) => listener(toast));
    }
  }

  public updateConfig(config: Partial<ToastConfig>): void {
    this.config = { ...this.config, ...config };
    if (this.container) {
      this.container.style.zIndex = String(this.config.zIndex);
      this.container.style.gap = `${this.config.spacing}px`;
    }
  }

  public getConfig(): ToastConfig {
    return { ...this.config };
  }

  public dispose(): void {
    this.activeToasts.forEach((toast) => toast.remove());
    this.activeToasts.clear();
    if (this.container) {
      this.container.remove();
      this.container = null;
    }
    this.listeners.clear();
  }
}

export interface ToastConfig {
  position: 'left' | 'right';
  offset: number;
  direction: 'column' | 'row';
  spacing: number;
  padding: number;
  borderRadius: number;
  maxWidth: number;
  minWidth: number;
  shadow: boolean;
  useGradient: boolean;
  zIndex: number;
  showIcon: boolean;
  iconSize: number;
  showTimestamp: boolean;
  titleFontSize: number;
  messageFontSize: number;
}

const defaultToastConfig: ToastConfig = {
  position: 'right',
  offset: 20,
  direction: 'column',
  spacing: 10,
  padding: 16,
  borderRadius: 8,
  maxWidth: 350,
  minWidth: 250,
  shadow: true,
  useGradient: true,
  zIndex: 10000,
  showIcon: true,
  iconSize: 24,
  showTimestamp: true,
  titleFontSize: 14,
  messageFontSize: 12,
};

export interface ToastInstance {
  id: string;
  notification?: Notification;
  element?: HTMLElement;
}

export class NotificationTemplateManager {
  private templates: Map<string, NotificationTemplate> = new Map();
  private listeners: Array<(template: NotificationTemplate) => void> = [];

  constructor() {
    this.initializeDefaultTemplates();
  }

  private initializeDefaultTemplates(): void {
    this.registerTemplate({
      id: 'combat_alert',
      name: 'Combat Alert',
      type: NotificationType.Combat,
      category: NotificationCategory.Combat,
      titleTemplate: 'Combat Alert',
      messageTemplate: 'Enemy ${enemyType} spotted at ${location}!',
      priority: NotificationPriority.High,
      displayStyle: NotificationDisplayStyle.Alert,
      soundType: SoundType.Combat,
      animationType: AnimationType.Shake,
      variables: [
        { name: 'enemyType', type: 'string', required: true },
        { name: 'location', type: 'string', required: true },
      ],
    });

    this.registerTemplate({
      id: 'resource_low',
      name: 'Low Resource Warning',
      type: NotificationType.Warning,
      category: NotificationCategory.Economy,
      titleTemplate: 'Low ${resourceType}',
      messageTemplate: 'Your ${resourceType} is running low (${current}/${max}).',
      priority: NotificationPriority.Normal,
      displayStyle: NotificationDisplayStyle.Toast,
      soundType: SoundType.Warning,
      animationType: AnimationType.Bounce,
      variables: [
        { name: 'resourceType', type: 'string', required: true },
        { name: 'current', type: 'number', required: true },
        { name: 'max', type: 'number', required: true },
      ],
    });

    this.registerTemplate({
      id: 'achievement_unlocked',
      name: 'Achievement Unlocked',
      type: NotificationType.Achievement,
      category: NotificationCategory.Achievement,
      titleTemplate: 'Achievement Unlocked!',
      messageTemplate: '${achievementName} - ${description}',
      priority: NotificationPriority.High,
      displayStyle: NotificationDisplayStyle.Modal,
      soundType: SoundType.Achievement,
      animationType: AnimationType.Pop,
      variables: [
        { name: 'achievementName', type: 'string', required: true },
        { name: 'description', type: 'string', required: false },
      ],
    });

    this.registerTemplate({
      id: 'unit_ready',
      name: 'Unit Ready',
      type: NotificationType.Info,
      category: NotificationCategory.Combat,
      titleTemplate: 'Unit Ready',
      messageTemplate: '${unitName} is ready for deployment.',
      priority: NotificationPriority.Low,
      displayStyle: NotificationDisplayStyle.Toast,
      soundType: SoundType.Info,
      animationType: AnimationType.FadeIn,
      variables: [{ name: 'unitName', type: 'string', required: true }],
    });

    this.registerTemplate({
      id: 'building_complete',
      name: 'Building Complete',
      type: NotificationType.Success,
      category: NotificationCategory.General,
      titleTemplate: 'Construction Complete',
      messageTemplate: '${buildingName} has been constructed.',
      priority: NotificationPriority.Normal,
      displayStyle: NotificationDisplayStyle.Toast,
      soundType: SoundType.Success,
      animationType: AnimationType.Bounce,
      variables: [{ name: 'buildingName', type: 'string', required: true }],
    });

    this.registerTemplate({
      id: 'research_complete',
      name: 'Research Complete',
      type: NotificationType.Success,
      category: NotificationCategory.Technology,
      titleTemplate: 'Research Complete',
      messageTemplate: '${researchName} research is complete.',
      priority: NotificationPriority.Normal,
      displayStyle: NotificationDisplayStyle.Toast,
      soundType: SoundType.Success,
      animationType: AnimationType.Bounce,
      variables: [{ name: 'researchName', type: 'string', required: true }],
    });

    this.registerTemplate({
      id: 'diplomatic_offer',
      name: 'Diplomatic Offer',
      type: NotificationType.Info,
      category: NotificationCategory.Diplomacy,
      titleTemplate: 'Diplomatic Offer',
      messageTemplate: '${factionName} offers: ${offerDetails}',
      priority: NotificationPriority.High,
      displayStyle: NotificationDisplayStyle.Modal,
      soundType: SoundType.Info,
      animationType: AnimationType.SlideIn,
      variables: [
        { name: 'factionName', type: 'string', required: true },
        { name: 'offerDetails', type: 'string', required: true },
      ],
    });

    this.registerTemplate({
      id: 'tutorial_hint',
      name: 'Tutorial Hint',
      type: NotificationType.Tutorial,
      category: NotificationCategory.Tutorial,
      titleTemplate: 'Tip',
      messageTemplate: '${hintText}',
      priority: NotificationPriority.Low,
      displayStyle: NotificationDisplayStyle.Banner,
      soundType: undefined,
      animationType: AnimationType.FadeIn,
      variables: [{ name: 'hintText', type: 'string', required: true }],
    });

    this.registerTemplate({
      id: 'event_starting',
      name: 'Event Starting',
      type: NotificationType.Event,
      category: NotificationCategory.General,
      titleTemplate: 'Event Starting Soon',
      messageTemplate: '${eventName} begins in ${timeRemaining}!',
      priority: NotificationPriority.High,
      displayStyle: NotificationDisplayStyle.Banner,
      soundType: SoundType.Info,
      animationType: AnimationType.Glow,
      variables: [
        { name: 'eventName', type: 'string', required: true },
        { name: 'timeRemaining', type: 'string', required: true },
      ],
    });

    this.registerTemplate({
      id: 'error_connection',
      name: 'Connection Error',
      type: NotificationType.Error,
      category: NotificationCategory.System,
      titleTemplate: 'Connection Error',
      messageTemplate: 'Failed to connect to ${server}. Please try again.',
      priority: NotificationPriority.Critical,
      displayStyle: NotificationDisplayStyle.Alert,
      soundType: SoundType.Error,
      animationType: AnimationType.Shake,
      variables: [{ name: 'server', type: 'string', required: false, defaultValue: 'server' }],
    });
  }

  public registerTemplate(template: NotificationTemplate): void {
    this.validateTemplate(template);
    this.templates.set(template.id, template);
    this.notifyListeners(template);
  }

  public getTemplate(id: string): NotificationTemplate | undefined {
    return this.templates.get(id);
  }

  public getAllTemplates(): NotificationTemplate[] {
    return Array.from(this.templates.values());
  }

  public getTemplatesByType(type: NotificationType): NotificationTemplate[] {
    return Array.from(this.templates.values()).filter((t) => t.type === type);
  }

  public getTemplatesByCategory(category: NotificationCategory): NotificationTemplate[] {
    return Array.from(this.templates.values()).filter((t) => t.category === category);
  }

  public deleteTemplate(id: string): boolean {
    return this.templates.delete(id);
  }

  public applyTemplate(id: string, variables: Record<string, any>): Notification | null {
    const template = this.templates.get(id);
    if (!template) return null;

    this.validateVariables(template, variables);

    const title = this.interpolate(template.titleTemplate, variables);
    const message = this.interpolate(template.messageTemplate, variables);

    return this.createNotificationFromTemplate(template, title, message, variables);
  }

  private createNotificationFromTemplate(
    template: NotificationTemplate,
    title: string,
    message: string,
    variables: Record<string, any>
  ): Notification {
    return {
      id: this.generateId(),
      type: template.type,
      title,
      message,
      priority: template.priority,
      category: template.category,
      timestamp: Date.now(),
      duration: 5000,
      isRead: false,
      isDismissed: false,
      metadata: variables,
      displayStyle: template.displayStyle,
      soundType: template.soundType,
      animationType: template.animationType,
    };
  }

  private interpolate(template: string, variables: Record<string, any>): string {
    return template.replace(/\$\{(\w+)\}/g, (match, key) => {
      return variables[key] !== undefined ? String(variables[key]) : match;
    });
  }

  private validateTemplate(template: NotificationTemplate): void {
    if (!template.id) throw new Error('Template ID is required');
    if (!template.titleTemplate) throw new Error('Title template is required');
    if (!template.messageTemplate) throw new Error('Message template is required');
  }

  private validateVariables(template: NotificationTemplate, variables: Record<string, any>): void {
    const requiredVars = template.variables?.filter((v) => v.required) || [];
    for (const required of requiredVars) {
      if (variables[required.name] === undefined) {
        throw new Error(`Missing required variable: ${required.name}`);
      }
    }

    for (const [key, value] of Object.entries(variables)) {
      const variable = template.variables?.find((v) => v.name === key);
      if (variable) {
        if (variable.type === 'number' && typeof value !== 'number') {
          throw new Error(`Variable ${key} must be a number`);
        }
        if (variable.type === 'boolean' && typeof value !== 'boolean') {
          throw new Error(`Variable ${key} must be a boolean`);
        }
      }
    }
  }

  public subscribe(listener: (template: NotificationTemplate) => void): () => void {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  private notifyListeners(template: NotificationTemplate): void {
    this.listeners.forEach((listener) => listener(template));
  }

  private generateId(): string {
    return `template_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  public exportTemplates(): string {
    return JSON.stringify(Array.from(this.templates.entries()), null, 2);
  }

  public importTemplates(data: string): boolean {
    try {
      const imported = JSON.parse(data);
      if (Array.isArray(imported)) {
        imported.forEach(([id, template]) => {
          this.templates.set(id, template);
        });
        return true;
      }
      return false;
    } catch (error) {
      console.error('Failed to import templates:', error);
      return false;
    }
  }
}

export class NotificationPersistence {
  private storageKey: string = 'notification_system_data';
  private saveDelay: number = 1000;
  private saveTimeout: ReturnType<typeof setTimeout> | null = null;
  private listeners: Array<(data: PersistedData) => void> = [];

  constructor() {}

  public save(
    notifications: Notification[],
    config: NotificationConfig,
    groups: NotificationGroup[]
  ): void {
    if (this.saveTimeout) {
      clearTimeout(this.saveTimeout);
    }

    this.saveTimeout = setTimeout(() => {
      this.performSave(notifications, config, groups);
    }, this.saveDelay);
  }

  private performSave(
    notifications: Notification[],
    config: NotificationConfig,
    groups: NotificationGroup[]
  ): void {
    try {
      const data: PersistedData = {
        notifications: notifications.slice(0, 100),
        config,
        groups: groups.slice(0, 50),
        savedAt: Date.now(),
        version: '1.0.0',
      };

      const serialized = JSON.stringify(data);
      localStorage.setItem(this.storageKey, serialized);

      this.notifyListeners(data);
    } catch (error) {
      console.error('Failed to persist notifications:', error);
      this.handleStorageError();
    }
  }

  public load(): PersistedData | null {
    try {
      const data = localStorage.getItem(this.storageKey);
      if (!data) return null;

      const parsed = JSON.parse(data) as PersistedData;

      if (!this.validateVersion(parsed.version)) {
        console.warn('Persisted data has incompatible version, clearing...');
        this.clear();
        return null;
      }

      return parsed;
    } catch (error) {
      console.error('Failed to load persisted notifications:', error);
      return null;
    }
  }

  public clear(): void {
    try {
      localStorage.removeItem(this.storageKey);
    } catch (error) {
      console.error('Failed to clear persisted notifications:', error);
    }
  }

  private handleStorageError(): void {
    try {
      const keys = Object.keys(localStorage);
      const notificationKeys = keys.filter((k) => k.startsWith('notification_'));

      if (notificationKeys.length > 10) {
        notificationKeys.slice(5).forEach((key) => {
          localStorage.removeItem(key);
        });
      }
    } catch (error) {
      console.error('Failed to handle storage error:', error);
    }
  }

  private validateVersion(version: string): boolean {
    const current = '1.0.0';
    const currentParts = current.split('.').map(Number);
    const versionParts = version.split('.').map(Number);

    return versionParts[0] === currentParts[0];
  }

  public subscribe(listener: (data: PersistedData) => void): () => void {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  private notifyListeners(data: PersistedData): void {
    this.listeners.forEach((listener) => listener(data));
  }

  public exportData(): string {
    const data = this.load();
    return data ? JSON.stringify(data, null, 2) : '{}';
  }

  public importData(data: string): boolean {
    try {
      const parsed = JSON.parse(data);
      if (this.validateVersion(parsed.version)) {
        localStorage.setItem(this.storageKey, data);
        return true;
      }
      return false;
    } catch (error) {
      console.error('Failed to import notification data:', error);
      return false;
    }
  }

  public getStorageUsage(): { used: number; available: number; percentage: number } {
    try {
      const data = localStorage.getItem(this.storageKey);
      const used = data ? new Blob([data]).size : 0;
      const available = 5 * 1024 * 1024;
      return {
        used,
        available,
        percentage: (used / available) * 100,
      };
    } catch (error) {
      return { used: 0, available: 0, percentage: 0 };
    }
  }

  public setSaveDelay(delay: number): void {
    this.saveDelay = Math.max(100, delay);
  }

  public forceSave(
    notifications: Notification[],
    config: NotificationConfig,
    groups: NotificationGroup[]
  ): void {
    if (this.saveTimeout) {
      clearTimeout(this.saveTimeout);
      this.saveTimeout = null;
    }
    this.performSave(notifications, config, groups);
  }
}

export interface PersistedData {
  notifications: Notification[];
  config: NotificationConfig;
  groups: NotificationGroup[];
  savedAt: number;
  version: string;
}

export class NotificationManager {
  private static instance: NotificationManager | null = null;

  private notifications: Map<string, Notification> = new Map();
  private config: NotificationConfig;
  private history: NotificationHistory;
  private queue: AlertQueue;
  private groupManager: NotificationGroupManager;
  private filterManager: NotificationFilterManager;
  private templateManager: NotificationTemplateManager;
  private toastRenderer: ToastRenderer;
  private soundManager: NotificationSound;
  private animationManager: NotificationAnimation;
  private persistence: NotificationPersistence;
  private eventEmitter: EventEmitter;

  private updateLoop: ReturnType<typeof setInterval> | null = null;
  private isRunning: boolean = false;

  private constructor() {
    this.config = this.getDefaultConfig();
    this.history = new NotificationHistory(this.config.maxHistorySize);
    this.queue = new AlertQueue(this.config.maxQueueSize);
    this.groupManager = new NotificationGroupManager();
    this.filterManager = new NotificationFilterManager();
    this.templateManager = new NotificationTemplateManager();
    this.animationManager = new NotificationAnimation();
    this.soundManager = new NotificationSound();
    this.toastRenderer = new ToastRenderer(this.animationManager);
    this.persistence = new NotificationPersistence();
    this.eventEmitter = new EventEmitter();

    this.setupEventHandlers();
    this.loadPersistedData();
  }

  public static getInstance(): NotificationManager {
    if (!NotificationManager.instance) {
      NotificationManager.instance = new NotificationManager();
    }
    return NotificationManager.instance;
  }

  private getDefaultConfig(): NotificationConfig {
    return {
      maxNotifications: 50,
      defaultDuration: 5000,
      maxQueueSize: 100,
      enableSound: true,
      enableAnimations: true,
      enablePersistence: true,
      enableGrouping: true,
      groupSimilarNotifications: true,
      maxHistorySize: 1000,
      showTimestamps: true,
      autoHideDelay: 3000,
      position: { x: 'right', y: 'top', offsetX: 20, offsetY: 20 },
      layout: { maxWidth: 400, maxHeight: 600, spacing: 10, itemHeight: 80 },
      style: {
        theme: NotificationTheme.Game,
        fontFamily: 'inherit',
        fontSize: 14,
        borderRadius: 8,
        shadow: true,
        backdropBlur: false,
      },
    };
  }

  private setupEventHandlers(): void {
    this.queue.subscribe((notification) => {
      this.processNotification(notification);
    });
  }

  private loadPersistedData(): void {
    if (!this.config.enablePersistence) return;

    const data = this.persistence.load();
    if (data) {
      data.notifications.forEach((n) => this.notifications.set(n.id, n));
    }
  }

  public show(notification: Notification): string {
    notification.id = notification.id || this.generateId();
    notification.timestamp = notification.timestamp || Date.now();
    notification.isRead = notification.isRead || false;
    notification.isDismissed = notification.isDismissed || false;
    notification.displayStyle = notification.displayStyle || NotificationDisplayStyle.Toast;
    notification.duration = notification.duration || this.config.defaultDuration;

    if (this.notifications.size >= this.config.maxNotifications) {
      const oldest = this.getOldestNotification();
      if (oldest) {
        this.dismiss(oldest.id);
      }
    }

    this.notifications.set(notification.id, notification);
    this.history.add(notification);

    if (notification.groupId) {
      this.groupManager.addToGroup(notification.groupId, notification);
    }

    if (this.config.enableSound && notification.soundType) {
      this.soundManager.play(notification.soundType);
    }

    if (this.shouldQueue(notification)) {
      this.queue.enqueue(notification);
    } else {
      this.displayNotification(notification);
    }

    if (this.config.enablePersistence) {
      this.saveCurrentState();
    }

    this.eventEmitter.emit('notification:shown', notification);
    return notification.id;
  }

  public showAchievement(
    achievementId: string,
    achievementName: string,
    achievementDescription: string,
    options?: Partial<AchievementNotification>
  ): string {
    const notification: AchievementNotification = {
      id: this.generateId(),
      type: NotificationType.Achievement,
      title: 'Achievement Unlocked!',
      message: achievementDescription,
      priority: NotificationPriority.High,
      category: NotificationCategory.Achievement,
      timestamp: Date.now(),
      duration: options?.duration || 8000,
      isRead: false,
      isDismissed: false,
      displayStyle: NotificationDisplayStyle.Modal,
      soundType: SoundType.Achievement,
      animationType: AnimationType.Pop,
      achievementId,
      achievementName,
      achievementDescription,
      iconUrl: options?.iconUrl,
      reward: options?.reward,
      progress: options?.progress,
      totalProgress: options?.totalProgress,
      tier: options?.tier || AchievementTier.Bronze,
      isSecret: options?.isSecret || false,
      ...options,
    };

    return this.show(notification);
  }

  public showEvent(
    eventId: string,
    eventName: string,
    eventType: EventType,
    options?: Partial<EventNotification>
  ): string {
    const notification: EventNotification = {
      id: this.generateId(),
      type: NotificationType.Event,
      title: eventName,
      message: options?.message || `Event: ${eventName}`,
      priority: options?.priority || NotificationPriority.Normal,
      category: NotificationCategory.General,
      timestamp: Date.now(),
      duration: options?.duration || 6000,
      isRead: false,
      isDismissed: false,
      displayStyle: NotificationDisplayStyle.Banner,
      soundType: SoundType.Info,
      animationType: AnimationType.Glow,
      eventId,
      eventName,
      eventType,
      startTime: options?.startTime,
      endTime: options?.endTime,
      location: options?.location,
      participants: options?.participants,
      rewards: options?.rewards,
      urgency: options?.urgency || EventUrgency.Medium,
      ...options,
    };

    return this.show(notification);
  }

  public showTutorial(
    tutorialId: string,
    hintText: string,
    stepNumber: number,
    totalSteps: number,
    options?: Partial<TutorialNotification>
  ): string {
    const notification: TutorialNotification = {
      id: this.generateId(),
      type: NotificationType.Tutorial,
      title: 'Tip',
      message: hintText,
      priority: NotificationPriority.Low,
      category: NotificationCategory.Tutorial,
      timestamp: Date.now(),
      duration: options?.duration || 10000,
      isRead: false,
      isDismissed: false,
      displayStyle: NotificationDisplayStyle.Banner,
      soundType: undefined,
      animationType: AnimationType.FadeIn,
      tutorialId,
      stepNumber,
      totalSteps,
      targetElement: options?.targetElement,
      arrowPosition: options?.arrowPosition,
      spotlightEnabled: options?.spotlightEnabled ?? true,
      skipAvailable: options?.skipAvailable ?? true,
      progressEnabled: options?.progressEnabled ?? true,
      autoAdvance: options?.autoAdvance ?? true,
      advanceDelay: options?.advanceDelay || 5000,
      ...options,
    };

    return this.show(notification);
  }

  public showFromTemplate(templateId: string, variables: Record<string, any>): string | null {
    const notification = this.templateManager.applyTemplate(templateId, variables);
    if (!notification) return null;
    return this.show(notification);
  }

  private processNotification(notification: Notification): void {
    this.displayNotification(notification);
  }

  private async displayNotification(notification: Notification): Promise<void> {
    switch (notification.displayStyle) {
      case NotificationDisplayStyle.Toast:
        await this.toastRenderer.render(notification);
        break;
      case NotificationDisplayStyle.Modal:
        await this.showModal(notification);
        break;
      case NotificationDisplayStyle.Banner:
        await this.showBanner(notification);
        break;
      case NotificationDisplayStyle.Alert:
        await this.showAlert(notification);
        break;
      case NotificationDisplayStyle.Floating:
        await this.showFloating(notification);
        break;
      case NotificationDisplayStyle.Chat:
        await this.showChat(notification);
        break;
    }
  }

  private async showModal(notification: Notification): Promise<void> {
    const modal = document.createElement('div');
    modal.className = 'notification-modal';
    modal.style.cssText = `
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.7);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10001;
      font-family: inherit;
    `;

    const content = document.createElement('div');
    content.style.cssText = `
      background: #2a2a2a;
      padding: 30px;
      border-radius: 12px;
      max-width: 400px;
      text-align: center;
      color: white;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5);
    `;

    if (notification.icon) {
      const icon = document.createElement('img');
      icon.src = notification.icon;
      icon.style.cssText = 'width: 64px; height: 64px; margin-bottom: 20px;';
      content.appendChild(icon);
    }

    const title = document.createElement('h2');
    title.textContent = notification.title;
    title.style.cssText = 'margin: 0 0 15px 0; font-size: 24px;';
    content.appendChild(title);

    const message = document.createElement('p');
    message.textContent = notification.message;
    message.style.cssText = 'margin: 0 0 25px 0; font-size: 16px; opacity: 0.9;';
    content.appendChild(message);

    if (notification.actions && notification.actions.length > 0) {
      const actions = document.createElement('div');
      actions.style.cssText = 'display: flex; gap: 10px; justify-content: center;';

      notification.actions.forEach((action) => {
        const button = document.createElement('button');
        button.textContent = action.label;
        button.style.cssText = `
          padding: 10px 20px;
          border: none;
          border-radius: 6px;
          cursor: pointer;
          font-size: 14px;
          font-weight: bold;
          background: ${action.isPrimary ? '#4CAF50' : '#666'};
          color: white;
          transition: transform 0.2s;
        `;
        button.onmouseenter = () => (button.style.transform = 'scale(1.05)');
        button.onmouseleave = () => (button.style.transform = 'scale(1)');
        button.onclick = () => {
          action.callback();
          this.closeModal(modal);
        };
        actions.appendChild(button);
      });

      content.appendChild(actions);
    } else {
      const closeButton = document.createElement('button');
      closeButton.textContent = 'Close';
      closeButton.style.cssText = `
        padding: 10px 30px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-size: 14px;
        background: #4CAF50;
        color: white;
      `;
      closeButton.onclick = () => this.closeModal(modal);
      content.appendChild(closeButton);
    }

    modal.appendChild(content);
    document.body.appendChild(modal);

    await this.animationManager.play(modal, AnimationType.FadeIn);
  }

  private closeModal(modal: HTMLElement): void {
    this.animationManager.play(modal, AnimationType.FadeIn, {
      duration: 200,
      onComplete: () => modal.remove(),
    });
  }

  private async showBanner(notification: Notification): Promise<void> {
    const banner = document.createElement('div');
    banner.className = 'notification-banner';
    banner.style.cssText = `
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      background: linear-gradient(90deg, #333 0%, #444 100%);
      color: white;
      padding: 15px 30px;
      text-align: center;
      z-index: 10000;
      font-family: inherit;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
    `;

    banner.textContent = `${notification.title}: ${notification.message}`;
    document.body.appendChild(banner);

    await this.animationManager.play(banner, AnimationType.SlideIn);

    setTimeout(() => {
      this.animationManager.play(banner, AnimationType.FadeIn, {
        duration: 300,
        onComplete: () => banner.remove(),
      });
    }, notification.duration);
  }

  private async showAlert(notification: Notification): Promise<void> {
    const alert = document.createElement('div');
    alert.className = 'notification-alert';
    alert.style.cssText = `
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: #F44336;
      color: white;
      padding: 40px;
      border-radius: 12px;
      text-align: center;
      z-index: 10001;
      font-family: inherit;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5);
      max-width: 500px;
    `;

    const title = document.createElement('h1');
    title.textContent = notification.title;
    title.style.cssText = 'margin: 0 0 20px 0; font-size: 32px;';
    alert.appendChild(title);

    const message = document.createElement('p');
    message.textContent = notification.message;
    message.style.cssText = 'margin: 0 0 30px 0; font-size: 18px;';
    alert.appendChild(message);

    const closeButton = document.createElement('button');
    closeButton.textContent = 'Dismiss';
    closeButton.style.cssText = `
      padding: 12px 40px;
      border: 2px solid white;
      border-radius: 8px;
      background: transparent;
      color: white;
      font-size: 16px;
      cursor: pointer;
      transition: all 0.2s;
    `;
    closeButton.onmouseenter = () => {
      closeButton.style.background = 'white';
      closeButton.style.color = '#F44336';
    };
    closeButton.onmouseleave = () => {
      closeButton.style.background = 'transparent';
      closeButton.style.color = 'white';
    };
    closeButton.onclick = () => {
      this.animationManager.play(alert, AnimationType.FadeIn, {
        duration: 200,
        onComplete: () => alert.remove(),
      });
    };
    alert.appendChild(closeButton);

    document.body.appendChild(alert);
    await this.animationManager.play(alert, AnimationType.Shake);
  }

  private async showFloating(notification: Notification): Promise<void> {
    const floating = document.createElement('div');
    floating.className = 'notification-floating';
    floating.style.cssText = `
      position: fixed;
      bottom: 100px;
      left: 50%;
      transform: translateX(-50%);
      background: #2196F3;
      color: white;
      padding: 20px 40px;
      border-radius: 30px;
      font-size: 16px;
      z-index: 10000;
      font-family: inherit;
      box-shadow: 0 5px 20px rgba(0, 0, 0, 0.3);
    `;

    floating.textContent = notification.message;
    document.body.appendChild(floating);

    await this.animationManager.play(floating, AnimationType.Bounce);

    setTimeout(() => {
      this.animationManager.play(floating, AnimationType.FadeIn, {
        duration: 300,
        onComplete: () => floating.remove(),
      });
    }, notification.duration);
  }

  private async showChat(notification: Notification): Promise<void> {
    const chat = document.createElement('div');
    chat.className = 'notification-chat';
    chat.style.cssText = `
      position: fixed;
      bottom: 20px;
      right: 20px;
      background: #fff;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 12px 16px;
      max-width: 300px;
      font-family: inherit;
      box-shadow: 0 3px 10px rgba(0, 0, 0, 0.1);
    `;

    const header = document.createElement('div');
    header.style.cssText = 'font-weight: bold; margin-bottom: 4px; font-size: 12px; color: #666;';
    header.textContent = notification.title;
    chat.appendChild(header);

    const message = document.createElement('div');
    message.style.cssText = 'font-size: 14px; color: #333;';
    message.textContent = notification.message;
    chat.appendChild(message);

    document.body.appendChild(chat);
    await this.animationManager.play(chat, AnimationType.SlideIn);

    setTimeout(() => {
      this.animationManager.play(chat, AnimationType.FadeIn, {
        duration: 200,
        onComplete: () => chat.remove(),
      });
    }, notification.duration);
  }

  private shouldQueue(notification: Notification): boolean {
    return (
      notification.priority < NotificationPriority.High ||
      this.toastRenderer.getActiveCount() >= 5
    );
  }

  public dismiss(id: string): boolean {
    const notification = this.notifications.get(id);
    if (!notification) return false;

    notification.isDismissed = true;
    this.toastRenderer.dismiss(id);
    this.eventEmitter.emit('notification:dismissed', notification);
    return true;
  }

  public dismissAll(): void {
    this.notifications.forEach((notification) => {
      notification.isDismissed = true;
    });
    this.toastRenderer.dismissAll();
    this.eventEmitter.emit('notification:dismissedAll');
  }

  public markAsRead(id: string): boolean {
    const notification = this.notifications.get(id);
    if (!notification) return false;

    notification.isRead = true;
    this.eventEmitter.emit('notification:read', notification);
    return true;
  }

  public markAllAsRead(): void {
    this.notifications.forEach((notification) => {
      notification.isRead = true;
    });
    this.eventEmitter.emit('notification:readAll');
  }

  public getNotification(id: string): Notification | undefined {
    return this.notifications.get(id);
  }

  public getAllNotifications(): Notification[] {
    return Array.from(this.notifications.values());
  }

  public getActiveNotifications(): Notification[] {
    return Array.from(this.notifications.values()).filter((n) => !n.isDismissed);
  }

  public getUnreadNotifications(): Notification[] {
    return Array.from(this.notifications.values()).filter((n) => !n.isRead && !n.isDismissed);
  }

  public getNotificationsByType(type: NotificationType): Notification[] {
    return Array.from(this.notifications.values()).filter((n) => n.type === type);
  }

  public getNotificationsByCategory(category: NotificationCategory): Notification[] {
    return Array.from(this.notifications.values()).filter((n) => n.category === category);
  }

  public getFilteredNotifications(filter: NotificationFilter): Notification[] {
    return this.filterManager.applyFilter(Array.from(this.notifications.values()));
  }

  public getQueueNotifications(): Notification[] {
    return this.queue.getAll();
  }

  public getHistory(): NotificationHistory {
    return this.history;
  }

  public getGroupManager(): NotificationGroupManager {
    return this.groupManager;
  }

  public getTemplateManager(): NotificationTemplateManager {
    return this.templateManager;
  }

  public getStatistics(): NotificationStatistics {
    return this.history.getStatistics();
  }

  public updateConfig(config: Partial<NotificationConfig>): void {
    this.config = { ...this.config, ...config };
    if (this.persistence) {
      this.saveCurrentState();
    }
  }

  public getConfig(): NotificationConfig {
    return { ...this.config };
  }

  private saveCurrentState(): void {
    const groups = this.groupManager.getAllGroups();
    this.persistence.save(
      Array.from(this.notifications.values()),
      this.config,
      groups
    );
  }

  public on(event: string, callback: (...args: any[]) => void): () => void {
    return this.eventEmitter.on(event, callback);
  }

  public off(event: string, callback: (...args: any[]) => void): void {
    this.eventEmitter.off(event, callback);
  }

  public startUpdateLoop(interval: number = 1000): void {
    if (this.isRunning) return;

    this.isRunning = true;
    this.updateLoop = setInterval(() => {
      this.update();
    }, interval);
  }

  public stopUpdateLoop(): void {
    if (this.updateLoop) {
      clearInterval(this.updateLoop);
      this.updateLoop = null;
    }
    this.isRunning = false;
  }

  private update(): void {
    const now = Date.now();
    this.notifications.forEach((notification) => {
      if (
        !notification.isDismissed &&
        notification.maxDisplayTime &&
        now - notification.timestamp > notification.maxDisplayTime
      ) {
        this.dismiss(notification.id);
      }
    });

    while (!this.queue.isEmpty() && this.toastRenderer.getActiveCount() < 3) {
      const next = this.queue.dequeue();
      if (next) {
        this.displayNotification(next);
      }
    }
  }

  private getOldestNotification(): Notification | undefined {
    let oldest: Notification | undefined;
    let oldestTime = Infinity;

    this.notifications.forEach((notification) => {
      if (!notification.isDismissed && notification.timestamp < oldestTime) {
        oldest = notification;
        oldestTime = notification.timestamp;
      }
    });

    return oldest;
  }

  private generateId(): string {
    return `notif_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  public dispose(): void {
    this.stopUpdateLoop();
    this.toastRenderer.dispose();
    this.soundManager.dispose();
    this.animationManager.dispose();
    this.notifications.clear();
    this.eventEmitter.removeAllListeners();
  }
}

export default NotificationManager;

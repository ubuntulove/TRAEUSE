import { EventEmitter } from 'events';
import { GameEvents } from '../events/GameEvents';
import { ResourceManager } from '../resources/ResourceManager';
import { UnitManager } from '../units/UnitManager';
import { BuildingManager } from '../buildings/BuildingManager';
import { TechTreeManager } from '../tech/TechTreeManager';
import { CombatManager } from '../combat/CombatManager';
import { EconomyManager } from '../economy/EconomyManager';
import { SaveManager } from '../save/SaveManager';
import { UIManager } from '../ui/UIManager';

export enum AchievementType {
  Combat = 'combat',
  Economy = 'economy',
  Exploration = 'exploration',
  Technology = 'technology',
  Construction = 'construction',
  Social = 'social',
  Special = 'special',
  Seasonal = 'seasonal',
  Hidden = 'hidden',
  Meta = 'meta'
}

export enum ProgressionTier {
  Bronze = 'bronze',
  Silver = 'silver',
  Gold = 'gold',
  Platinum = 'platinum',
  Diamond = 'diamond',
  Master = 'master',
  Grandmaster = 'grandmaster'
}

export enum UnlockConditionType {
  KillCount = 'kill_count',
  BuildCount = 'build_count',
  ResourceGathered = 'resource_gathered',
  BuildingDestroyed = 'building_destroyed',
  UnitTrained = 'unit_trained',
  TechResearched = 'tech_researched',
  MapExplored = 'map_explored',
  TimePlayed = 'time_played',
  WinStreak = 'win_streak',
  TotalWins = 'total_wins',
  TotalLosses = 'total_losses',
  DamageDealt = 'damage_dealt',
  DamageReceived = 'damage_received',
  UnitsHealed = 'units_healed',
  ResourcesTraded = 'resources_traded',
  AlliancesFormed = 'alliances_formed',
  AchievementsUnlocked = 'achievements_unlocked',
  MilestonesReached = 'milestones_reached',
  CustomCondition = 'custom_condition'
}

export interface UnlockCondition {
  type: UnlockConditionType;
  target: number;
  current: number;
  comparison: 'gte' | 'lte' | 'eq' | 'gt' | 'lt';
  requirements?: {
    unitType?: string;
    buildingType?: string;
    resourceType?: string;
    techType?: string;
    mapRegion?: string;
    playerId?: string;
    difficulty?: string;
  };
  secondaryCondition?: UnlockCondition;
}

export interface AchievementCriteria {
  conditions: UnlockCondition[];
  logic: 'all' | 'any' | 'custom';
  customLogic?: (conditions: UnlockCondition[]) => boolean;
}

export interface Achievement {
  id: string;
  name: string;
  description: string;
  type: AchievementType;
  tier: ProgressionTier;
  icon: string;
  criteria: AchievementCriteria;
  rewards: UnlockReward[];
  isSecret: boolean;
  isSeasonal: boolean;
  seasonalId?: string;
  categoryId: string;
  difficulty: number;
  points: number;
  unlockedAt?: Date;
  progress?: number;
  prerequisites: string[];
  relatedAchievements: string[];
  estimatedDifficulty: 'easy' | 'medium' | 'hard' | 'extreme';
  tags: string[];
  lastUpdated?: Date;
}

export interface AchievementDefinition {
  id: string;
  name: string;
  description: string;
  type: AchievementType;
  tier: ProgressionTier;
  icon: string;
  criteria: AchievementCriteria;
  rewards: UnlockReward[];
  isSecret: boolean;
  isSeasonal: boolean;
  seasonalId?: string;
  categoryId: string;
  difficulty: number;
  points: number;
  prerequisites: string[];
  relatedAchievements: string[];
  estimatedDifficulty: 'easy' | 'medium' | 'hard' | 'extreme';
  tags: string[];
}

export interface UnlockReward {
  type: 'experience' | 'currency' | 'unlock' | 'badge' | 'title' | 'cosmetic' | 'resource';
  value: number | string;
  description: string;
  isUnique: boolean;
  expiresAt?: Date;
  conditions?: {
    playerLevel?: number;
    achievementTier?: ProgressionTier;
    gameMode?: string;
  };
}

export interface ProgressUpdate {
  achievementId: string;
  conditionType: UnlockConditionType;
  previousValue: number;
  currentValue: number;
  targetValue: number;
  progress: number;
  isComplete: boolean;
  timestamp: Date;
  metadata?: Record<string, unknown>;
}

export interface AchievementProgress {
  achievementId: string;
  conditions: Map<UnlockConditionType, number>;
  isUnlocked: boolean;
  unlockedAt?: Date;
  lastProgressUpdate?: Date;
  completionPercentage: number;
  failedAttempts: number;
  resetCount: number;
}

export interface ProgressionMilestone {
  id: string;
  name: string;
  description: string;
  tier: ProgressionTier;
  requirements: {
    totalPoints: number;
    achievementsOfTier: Record<ProgressionTier, number>;
    specificAchievements?: string[];
  };
  rewards: UnlockReward[];
  isUnlocked: boolean;
  unlockedAt?: Date;
}

export interface AchievementCategory {
  id: string;
  name: string;
  description: string;
  icon: string;
  achievementIds: string[];
  sortOrder: number;
  isExpanded: boolean;
  completionPercentage: number;
}

export interface AchievementStats {
  totalAchievements: number;
  unlockedAchievements: number;
  lockedAchievements: number;
  completionPercentage: number;
  totalPoints: number;
  earnedPoints: number;
  achievementsByType: Map<AchievementType, number>;
  achievementsByTier: Map<ProgressionTier, number>;
  recentUnlocks: Achievement[];
  longestStreak: number;
  currentStreak: number;
  averageCompletionTime: number;
  mostDifficultUnlocked: ProgressionTier;
  categoryCompletion: Map<string, number>;
  seasonalProgress: Map<string, number>;
  hiddenAchievementsFound: number;
}

export interface SeasonalEvent {
  id: string;
  name: string;
  description: string;
  startDate: Date;
  endDate: Date;
  theme: string;
  achievements: string[];
  bonusRewards: UnlockReward[];
  isActive: boolean;
  participationCount: number;
  completionPercentage: number;
}

export interface AchievementComparison {
  playerId: string;
  playerName: string;
  achievementCount: number;
  totalPoints: number;
  rank: number;
  recentAchievements: string[];
  categoryProgress: Map<string, number>;
  tierDistribution: Map<ProgressionTier, number>;
  comparisonDate: Date;
}

export interface AchievementLeaderboard {
  id: string;
  name: string;
  entries: AchievementComparison[];
  sortBy: 'points' | 'count' | 'rank';
  updatedAt: Date;
  timeframe: 'all_time' | 'seasonal' | 'monthly' | 'weekly';
}

export interface AchievementNotification {
  id: string;
  achievementId: string;
  achievementName: string;
  achievementDescription: string;
  tier: ProgressionTier;
  icon: string;
  rewards: UnlockReward[];
  timestamp: Date;
  isRead: boolean;
  isDismissed: boolean;
  displayDuration: number;
  priority: 'low' | 'medium' | 'high' | 'critical';
}

export interface AchievementUIElements {
  panel: HTMLElement | null;
  achievementList: HTMLElement | null;
  categoryTabs: HTMLElement | null;
  progressBar: HTMLElement | null;
  notificationArea: HTMLElement | null;
  filterControls: HTMLElement | null;
  searchInput: HTMLElement | null;
  sortDropdown: HTMLElement | null;
  detailModal: HTMLElement | null;
  comparisonPanel: HTMLElement | null;
}

export class AchievementTracker extends EventEmitter {
  private trackingData: Map<string, Map<UnlockConditionType, number>> = new Map();
  private conditionThresholds: Map<string, Map<UnlockConditionType, number>> = new Map();
  private eventHandlers: Map<string, Set<Function>> = new Map();
  private lastUpdate: Map<string, Date> = new Map();
  private updateFrequency: number = 100;
  private maxHistorySize: number = 1000;
  private historyBuffer: ProgressUpdate[] = [];
  private batchUpdates: Map<string, ProgressUpdate> = new Map();
  private batchTimer: NodeJS.Timeout | null = null;
  private trackingEnabled: boolean = true;
  private persistenceCallback: ((data: unknown) => Promise<void>) | null = null;

  constructor() {
    super();
    this.initializeDefaultThresholds();
  }

  private initializeDefaultThresholds(): void {
    const defaultThresholds = new Map<UnlockConditionType, number>();
    defaultThresholds.set(UnlockConditionType.KillCount, 0);
    defaultThresholds.set(UnlockConditionType.BuildCount, 0);
    defaultThresholds.set(UnlockConditionType.ResourceGathered, 0);
    defaultThresholds.set(UnlockConditionType.BuildingDestroyed, 0);
    defaultThresholds.set(UnlockConditionType.UnitTrained, 0);
    defaultThresholds.set(UnlockConditionType.TechResearched, 0);
    defaultThresholds.set(UnlockConditionType.MapExplored, 0);
    defaultThresholds.set(UnlockConditionType.TimePlayed, 0);
    defaultThresholds.set(UnlockConditionType.WinStreak, 0);
    defaultThresholds.set(UnlockConditionType.TotalWins, 0);
    defaultThresholds.set(UnlockConditionType.TotalLosses, 0);
    defaultThresholds.set(UnlockConditionType.DamageDealt, 0);
    defaultThresholds.set(UnlockConditionType.DamageReceived, 0);
    defaultThresholds.set(UnlockConditionType.UnitsHealed, 0);
    defaultThresholds.set(UnlockConditionType.ResourcesTraded, 0);
    defaultThresholds.set(UnlockConditionType.AlliancesFormed, 0);
    defaultThresholds.set(UnlockConditionType.AchievementsUnlocked, 0);
    defaultThresholds.set(UnlockConditionType.MilestonesReached, 0);
  }

  public startTracking(playerId: string): void {
    if (!this.trackingData.has(playerId)) {
      this.trackingData.set(playerId, new Map());
      this.initializeTrackingData(playerId);
    }
    this.lastUpdate.set(playerId, new Date());
    this.emit('trackingStarted', { playerId, timestamp: new Date() });
  }

  private initializeTrackingData(playerId: string): void {
    const playerData = this.trackingData.get(playerId);
    if (!playerData) return;

    Object.values(UnlockConditionType).forEach(type => {
      playerData.set(type as UnlockConditionType, 0);
    });

    const thresholds = new Map<UnlockConditionType, number>();
    Object.values(UnlockConditionType).forEach(type => {
      thresholds.set(type as UnlockConditionType, 0);
    });
    this.conditionThresholds.set(playerId, thresholds);
  }

  public updateProgress(
    playerId: string,
    conditionType: UnlockConditionType,
    value: number,
    metadata?: Record<string, unknown>
  ): ProgressUpdate | null {
    if (!this.trackingEnabled) return null;

    if (!this.trackingData.has(playerId)) {
      this.startTracking(playerId);
    }

    const playerData = this.trackingData.get(playerId);
    if (!playerData) return null;

    const previousValue = playerData.get(conditionType) || 0;
    const newValue = previousValue + value;
    playerData.set(conditionType, newValue);

    const thresholds = this.conditionThresholds.get(playerId);
    const targetValue = thresholds?.get(conditionType) || 0;

    const progressUpdate: ProgressUpdate = {
      achievementId: '',
      conditionType,
      previousValue,
      currentValue: newValue,
      targetValue,
      progress: targetValue > 0 ? Math.min((newValue / targetValue) * 100, 100) : 0,
      isComplete: newValue >= targetValue,
      timestamp: new Date(),
      metadata
    };

    this.addToHistory(progressUpdate);
    this.lastUpdate.set(playerId, new Date());

    this.scheduleBatchUpdate(progressUpdate);
    this.emit('progressUpdated', progressUpdate);

    return progressUpdate;
  }

  public setProgress(
    playerId: string,
    conditionType: UnlockConditionType,
    value: number,
    metadata?: Record<string, unknown>
  ): ProgressUpdate | null {
    if (!this.trackingEnabled) return null;

    if (!this.trackingData.has(playerId)) {
      this.startTracking(playerId);
    }

    const playerData = this.trackingData.get(playerId);
    if (!playerData) return null;

    const previousValue = playerData.get(conditionType) || 0;
    playerData.set(conditionType, value);

    const thresholds = this.conditionThresholds.get(playerId);
    const targetValue = thresholds?.get(conditionType) || 0;

    const progressUpdate: ProgressUpdate = {
      achievementId: '',
      conditionType,
      previousValue,
      currentValue: value,
      targetValue,
      progress: targetValue > 0 ? Math.min((value / targetValue) * 100, 100) : 0,
      isComplete: value >= targetValue,
      timestamp: new Date(),
      metadata
    };

    this.addToHistory(progressUpdate);
    this.lastUpdate.set(playerId, new Date());

    this.scheduleBatchUpdate(progressUpdate);
    this.emit('progressUpdated', progressUpdate);

    return progressUpdate;
  }

  public getProgress(playerId: string, conditionType: UnlockConditionType): number {
    const playerData = this.trackingData.get(playerId);
    return playerData?.get(conditionType) || 0;
  }

  public getAllProgress(playerId: string): Map<UnlockConditionType, number> {
    return this.trackingData.get(playerId) || new Map();
  }

  public setThreshold(playerId: string, conditionType: UnlockConditionType, threshold: number): void {
    let thresholds = this.conditionThresholds.get(playerId);
    if (!thresholds) {
      thresholds = new Map();
      this.conditionThresholds.set(playerId, thresholds);
    }
    thresholds.set(conditionType, threshold);
  }

  public getThreshold(playerId: string, conditionType: UnlockConditionType): number {
    return this.conditionThresholds.get(playerId)?.get(conditionType) || 0;
  }

  private addToHistory(update: ProgressUpdate): void {
    this.historyBuffer.push(update);
    if (this.historyBuffer.length > this.maxHistorySize) {
      this.historyBuffer.shift();
    }
  }

  public getHistory(playerId?: string, limit?: number): ProgressUpdate[] {
    let filtered = this.historyBuffer;
    if (playerId) {
      filtered = filtered.filter(u => u.achievementId.includes(playerId));
    }
    return limit ? filtered.slice(-limit) : filtered;
  }

  private scheduleBatchUpdate(update: ProgressUpdate): void {
    this.batchUpdates.set(update.conditionType, update);

    if (this.batchTimer) {
      clearTimeout(this.batchTimer);
    }

    this.batchTimer = setTimeout(() => {
      this.flushBatchUpdates();
    }, this.updateFrequency);
  }

  private flushBatchUpdates(): void {
    const updates = Array.from(this.batchUpdates.values());
    this.batchUpdates.clear();

    if (updates.length > 0 && this.persistenceCallback) {
      this.persistenceCallback({
        trackingData: Object.fromEntries(this.trackingData),
        updates
      }).catch(err => console.error('Failed to persist tracking data:', err));
    }

    this.emit('batchUpdate', updates);
  }

  public setPersistenceCallback(callback: (data: unknown) => Promise<void>): void {
    this.persistenceCallback = callback;
  }

  public enableTracking(): void {
    this.trackingEnabled = true;
    this.emit('trackingEnabled');
  }

  public disableTracking(): void {
    this.trackingEnabled = false;
    this.emit('trackingDisabled');
  }

  public resetProgress(playerId: string): void {
    this.trackingData.delete(playerId);
    this.conditionThresholds.delete(playerId);
    this.lastUpdate.delete(playerId);
    this.emit('progressReset', { playerId, timestamp: new Date() });
  }

  public exportData(): unknown {
    return {
      trackingData: Object.fromEntries(this.trackingData),
      conditionThresholds: Object.fromEntries(this.conditionThresholds),
      historyBuffer: this.historyBuffer,
      lastUpdate: Object.fromEntries(this.lastUpdate)
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        trackingData: Record<string, Record<string, number>>;
        conditionThresholds: Record<string, Record<string, number>>;
        historyBuffer: ProgressUpdate[];
        lastUpdate: Record<string, string>;
      };

      this.trackingData.clear();
      Object.entries(typedData.trackingData).forEach(([playerId, values]) => {
        const playerData = new Map();
        Object.entries(values).forEach(([type, value]) => {
          playerData.set(type as UnlockConditionType, value);
        });
        this.trackingData.set(playerId, playerData);
      });

      this.conditionThresholds.clear();
      Object.entries(typedData.conditionThresholds).forEach(([playerId, values]) => {
        const thresholds = new Map();
        Object.entries(values).forEach(([type, value]) => {
          thresholds.set(type as UnlockConditionType, value);
        });
        this.conditionThresholds.set(playerId, thresholds);
      });

      this.historyBuffer = typedData.historyBuffer || [];

      this.lastUpdate.clear();
      Object.entries(typedData.lastUpdate).forEach(([playerId, timestamp]) => {
        this.lastUpdate.set(playerId, new Date(timestamp));
      });

      this.emit('dataImported', { timestamp: new Date() });
      return true;
    } catch (error) {
      console.error('Failed to import tracking data:', error);
      return false;
    }
  }
}

export class AchievementUnlocks {
  private pendingUnlocks: Map<string, string[]> = new Map();
  private unlockHistory: Map<string, Date> = new Map();
  private unlockAnimations: Map<string, boolean> = new Map();
  private prerequisites: Map<string, string[]> = new Map();
  private dependencies: Map<string, string[]> = new Map();
  private unlockQueue: string[] = [];
  private isProcessingQueue: boolean = false;

  constructor() {
    this.initializeDefaultPrerequisites();
  }

  private initializeDefaultPrerequisites(): void {
    this.prerequisites.set('bronze_commander', []);
    this.prerequisites.set('silver_commander', ['bronze_commander']);
    this.prerequisites.set('gold_commander', ['silver_commander']);
    this.prerequisites.set('first_blood', []);
    this.prerequisites.set('rampage', ['first_blood']);
    this.prerequisites.set('unstoppable', ['rampage']);
  }

  public checkUnlockConditions(
    achievement: Achievement,
    progress: Map<UnlockConditionType, number>
  ): boolean {
    const { criteria } = achievement;

    if (criteria.conditions.length === 0) return false;

    switch (criteria.logic) {
      case 'all':
        return criteria.conditions.every(condition => {
          const current = progress.get(condition.type) || 0;
          return this.evaluateCondition(condition, current);
        });

      case 'any':
        return criteria.conditions.some(condition => {
          const current = progress.get(condition.type) || 0;
          return this.evaluateCondition(condition, current);
        });

      case 'custom':
        return criteria.customLogic ? criteria.customLogic(criteria.conditions) : false;

      default:
        return false;
    }
  }

  private evaluateCondition(condition: UnlockCondition, current: number): boolean {
    const { comparison, target } = condition;

    switch (comparison) {
      case 'gte':
        return current >= target;
      case 'lte':
        return current <= target;
      case 'eq':
        return current === target;
      case 'gt':
        return current > target;
      case 'lt':
        return current < target;
      default:
        return false;
    }
  }

  public queueUnlock(achievementId: string, playerId: string): void {
    if (!this.pendingUnlocks.has(playerId)) {
      this.pendingUnlocks.set(playerId, []);
    }

    const pending = this.pendingUnlocks.get(playerId)!;
    if (!pending.includes(achievementId)) {
      pending.push(achievementId);
      this.unlockQueue.push(`${playerId}:${achievementId}`);
      this.processUnlockQueue();
    }
  }

  private async processUnlockQueue(): Promise<void> {
    if (this.isProcessingQueue) return;
    this.isProcessingQueue = true;

    while (this.unlockQueue.length > 0) {
      const item = this.unlockQueue.shift()!;
      const [playerId, achievementId] = item.split(':');

      await this.processUnlock(playerId, achievementId);
    }

    this.isProcessingQueue = false;
  }

  private async processUnlock(playerId: string, achievementId: string): Promise<void> {
    await this.delay(100);
    this.unlockHistory.set(`${playerId}:${achievementId}`, new Date());
    this.emit('unlockProcessed', { playerId, achievementId, timestamp: new Date() });
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  public isUnlocked(playerId: string, achievementId: string): boolean {
    return this.unlockHistory.has(`${playerId}:${achievementId}`);
  }

  public getUnlockTime(playerId: string, achievementId: string): Date | undefined {
    return this.unlockHistory.get(`${playerId}:${achievementId}`);
  }

  public getPendingUnlocks(playerId: string): string[] {
    return this.pendingUnlocks.get(playerId) || [];
  }

  public clearPendingUnlocks(playerId: string): void {
    this.pendingUnlocks.delete(playerId);
  }

  public setPrerequisite(achievementId: string, prerequisites: string[]): void {
    this.prerequisites.set(achievementId, prerequisites);
    this.updateDependencies();
  }

  private updateDependencies(): void {
    this.dependencies.clear();
    this.prerequisites.forEach((prereqs, achievementId) => {
      prereqs.forEach(prereqId => {
        if (!this.dependencies.has(prereqId)) {
          this.dependencies.set(prereqId, []);
        }
        this.dependencies.get(prereqId)!.push(achievementId);
      });
    });
  }

  public getPrerequisites(achievementId: string): string[] {
    return this.prerequisites.get(achievementId) || [];
  }

  public getDependencies(achievementId: string): string[] {
    return this.dependencies.get(achievementId) || [];
  }

  public checkPrerequisites(achievementId: string, unlockedAchievements: Set<string>): boolean {
    const prereqs = this.getPrerequisites(achievementId);
    return prereqs.every(prereqId => unlockedAchievements.has(prereqId));
  }

  public enableUnlockAnimation(achievementId: string): void {
    this.unlockAnimations.set(achievementId, true);
  }

  public disableUnlockAnimation(achievementId: string): void {
    this.unlockAnimations.set(achievementId, false);
  }

  public isAnimationEnabled(achievementId: string): boolean {
    return this.unlockAnimations.get(achievementId) || false;
  }

  private emit(event: string, data: unknown): void {
    process.emit(event, data);
  }

  public exportData(): unknown {
    return {
      pendingUnlocks: Object.fromEntries(this.pendingUnlocks),
      unlockHistory: Array.from(this.unlockHistory.entries()),
      unlockAnimations: Object.fromEntries(this.unlockAnimations),
      prerequisites: Object.fromEntries(this.prerequisites),
      dependencies: Object.fromEntries(this.dependencies)
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        pendingUnlocks: Record<string, string[]>;
        unlockHistory: [string, string][];
        unlockAnimations: Record<string, boolean>;
        prerequisites: Record<string, string[]>;
        dependencies: Record<string, string[]>;
      };

      this.pendingUnlocks = new Map(Object.entries(typedData.pendingUnlocks || {}));
      this.unlockHistory = new Map(typedData.unlockHistory || []);
      this.unlockAnimations = new Map(Object.entries(typedData.unlockAnimations || {}));
      this.prerequisites = new Map(Object.entries(typedData.prerequisites || {}));
      this.dependencies = new Map(Object.entries(typedData.dependencies || {}));
      this.updateDependencies();

      return true;
    } catch (error) {
      console.error('Failed to import unlock data:', error);
      return false;
    }
  }
}

export class AchievementRewards {
  private rewardHandlers: Map<string, (reward: UnlockReward) => Promise<boolean>> = new Map();
  private rewardHistory: Map<string, UnlockReward[]> = new Map();
  private pendingRewards: Map<string, UnlockReward[]> = new Map();
  private rewardLimits: Map<string, number> = new Map();
  private bonusMultipliers: Map<string, number> = new Map();
  private cooldownPeriods: Map<string, number> = new Map();
  private lastRewardTime: Map<string, Date> = new Map();

  constructor() {
    this.initializeDefaultHandlers();
  }

  private initializeDefaultHandlers(): void {
    this.registerRewardHandler('experience', async (reward) => {
      console.log(`Awarding ${reward.value} experience points`);
      return true;
    });

    this.registerRewardHandler('currency', async (reward) => {
      console.log(`Awarding ${reward.value} currency`);
      return true;
    });

    this.registerRewardHandler('unlock', async (reward) => {
      console.log(`Unlocking: ${reward.value}`);
      return true;
    });

    this.registerRewardHandler('badge', async (reward) => {
      console.log(`Awarding badge: ${reward.value}`);
      return true;
    });

    this.registerRewardHandler('title', async (reward) => {
      console.log(`Granting title: ${reward.value}`);
      return true;
    });

    this.registerRewardHandler('cosmetic', async (reward) => {
      console.log(`Granting cosmetic: ${reward.value}`);
      return true;
    });

    this.registerRewardHandler('resource', async (reward) => {
      console.log(`Awarding resource: ${reward.value}`);
      return true;
    });
  }

  public registerRewardHandler(type: string, handler: (reward: UnlockReward) => Promise<boolean>): void {
    this.rewardHandlers.set(type, handler);
  }

  public async processRewards(rewards: UnlockReward[], playerId: string): Promise<boolean[]> {
    const results: boolean[] = [];

    for (const reward of rewards) {
      const result = await this.grantReward(reward, playerId);
      results.push(result);
    }

    return results;
  }

  public async grantReward(reward: UnlockReward, playerId: string): Promise<boolean> {
    if (!this.checkRewardConditions(reward, playerId)) {
      return false;
    }

    if (this.isOnCooldown(reward, playerId)) {
      console.log(`Reward ${reward.type} is on cooldown for player ${playerId}`);
      return false;
    }

    if (reward.isUnique && this.hasReceivedUniqueReward(playerId, reward)) {
      console.log(`Player ${playerId} already received unique reward ${reward.type}`);
      return false;
    }

    const handler = this.rewardHandlers.get(reward.type);
    if (!handler) {
      console.error(`No handler found for reward type: ${reward.type}`);
      return false;
    }

    const adjustedReward = this.applyBonusMultipliers(reward, playerId);
    const success = await handler(adjustedReward);

    if (success) {
      this.recordReward(reward, playerId);
      this.updateLastRewardTime(reward, playerId);
    }

    return success;
  }

  private checkRewardConditions(reward: UnlockReward, playerId: string): boolean {
    if (!reward.conditions) return true;

    const { playerLevel, achievementTier, gameMode } = reward.conditions;

    if (playerLevel !== undefined) {
      const currentLevel = this.getPlayerLevel(playerId);
      if (currentLevel < playerLevel) {
        return false;
      }
    }

    if (achievementTier !== undefined) {
      console.log(`Checking tier condition: ${achievementTier}`);
    }

    if (gameMode !== undefined) {
      console.log(`Checking game mode condition: ${gameMode}`);
    }

    return true;
  }

  private getPlayerLevel(playerId: string): number {
    return 1;
  }

  private isOnCooldown(reward: UnlockReward, playerId: string): boolean {
    const key = `${playerId}:${reward.type}:${reward.value}`;
    const lastTime = this.lastRewardTime.get(key);

    if (!lastTime) return false;

    const cooldownKey = `${reward.type}:${reward.value}`;
    const cooldownPeriod = this.cooldownPeriods.get(cooldownKey) || 0;

    const elapsed = Date.now() - lastTime.getTime();
    return elapsed < cooldownPeriod;
  }

  private hasReceivedUniqueReward(playerId: string, reward: UnlockReward): boolean {
    const history = this.rewardHistory.get(playerId) || [];
    return history.some(r => r.isUnique && r.type === reward.type && r.value === reward.value);
  }

  private applyBonusMultipliers(reward: UnlockReward, playerId: string): UnlockReward {
    const multiplier = this.bonusMultipliers.get(playerId) || 1.0;

    if (typeof reward.value === 'number') {
      return {
        ...reward,
        value: Math.floor(reward.value * multiplier)
      };
    }

    return reward;
  }

  private recordReward(reward: UnlockReward, playerId: string): void {
    if (!this.rewardHistory.has(playerId)) {
      this.rewardHistory.set(playerId, []);
    }

    this.rewardHistory.get(playerId)!.push(reward);

    const key = `${playerId}:${reward.type}:${reward.value}`;
    const count = this.rewardLimits.get(key) || 0;
    this.rewardLimits.set(key, count + 1);
  }

  private updateLastRewardTime(reward: UnlockReward, playerId: string): void {
    const key = `${playerId}:${reward.type}:${reward.value}`;
    this.lastRewardTime.set(key, new Date());
  }

  public setBonusMultiplier(playerId: string, multiplier: number): void {
    this.bonusMultipliers.set(playerId, multiplier);
  }

  public getBonusMultiplier(playerId: string): number {
    return this.bonusMultipliers.get(playerId) || 1.0;
  }

  public setCooldownPeriod(rewardType: string, periodMs: number): void {
    this.cooldownPeriods.set(rewardType, periodMs);
  }

  public getRewardHistory(playerId: string): UnlockReward[] {
    return this.rewardHistory.get(playerId) || [];
  }

  public getTotalRewardValue(playerId: string, rewardType: string): number {
    const history = this.getRewardHistory(playerId);
    return history
      .filter(r => r.type === rewardType)
      .reduce((sum, r) => sum + (typeof r.value === 'number' ? r.value : 0), 0);
  }

  public queueReward(reward: UnlockReward, playerId: string): void {
    if (!this.pendingRewards.has(playerId)) {
      this.pendingRewards.set(playerId, []);
    }
    this.pendingRewards.get(playerId)!.push(reward);
  }

  public processPendingRewards(playerId: string): Promise<boolean[]> {
    const pending = this.pendingRewards.get(playerId) || [];
    this.pendingRewards.delete(playerId);
    return this.processRewards(pending, playerId);
  }

  public clearRewardHistory(playerId: string): void {
    this.rewardHistory.delete(playerId);
  }

  public exportData(): unknown {
    return {
      rewardHistory: Object.fromEntries(this.rewardHistory),
      pendingRewards: Object.fromEntries(this.pendingRewards),
      rewardLimits: Object.fromEntries(this.rewardLimits),
      bonusMultipliers: Object.fromEntries(this.bonusMultipliers),
      cooldownPeriods: Object.fromEntries(this.cooldownPeriods),
      lastRewardTime: Array.from(this.lastRewardTime.entries())
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        rewardHistory: Record<string, UnlockReward[]>;
        pendingRewards: Record<string, UnlockReward[]>;
        rewardLimits: Record<string, number>;
        bonusMultipliers: Record<string, number>;
        cooldownPeriods: Record<string, number>;
        lastRewardTime: [string, string][];
      };

      this.rewardHistory = new Map(Object.entries(typedData.rewardHistory || {}));
      this.pendingRewards = new Map(Object.entries(typedData.pendingRewards || {}));
      this.rewardLimits = new Map(Object.entries(typedData.rewardLimits || {}));
      this.bonusMultipliers = new Map(Object.entries(typedData.bonusMultipliers || {}));
      this.cooldownPeriods = new Map(Object.entries(typedData.cooldownPeriods || {}));
      this.lastRewardTime = new Map(typedData.lastRewardTime || []);

      return true;
    } catch (error) {
      console.error('Failed to import reward data:', error);
      return false;
    }
  }
}

export class AchievementNotifications {
  private notifications: AchievementNotification[] = [];
  private notificationQueue: AchievementNotification[] = [];
  private isProcessingQueue: boolean = false;
  private maxNotifications: number = 50;
  private displayDuration: number = 5000;
  private soundEnabled: boolean = true;
  private animationEnabled: boolean = true;
  private notificationHandlers: Map<string, Function> = new Map();
  private dismissedNotifications: Set<string> = new Map();
  private readNotifications: Set<string> = new Set();

  constructor() {
    this.initializeDefaultHandlers();
  }

  private initializeDefaultHandlers(): void {
    this.registerNotificationHandler('popup', (notification) => {
      console.log(`Displaying popup notification: ${notification.achievementName}`);
    });

    this.registerNotificationHandler('toast', (notification) => {
      console.log(`Displaying toast notification: ${notification.achievementName}`);
    });

    this.registerNotificationHandler('sound', (notification) => {
      if (this.soundEnabled) {
        console.log(`Playing achievement sound for: ${notification.achievementName}`);
      }
    });
  }

  public registerNotificationHandler(type: string, handler: Function): void {
    this.notificationHandlers.set(type, handler);
  }

  public showNotification(notification: AchievementNotification): void {
    notification.id = notification.id || this.generateNotificationId();
    notification.timestamp = notification.timestamp || new Date();
    notification.isRead = false;
    notification.isDismissed = false;
    notification.displayDuration = notification.displayDuration || this.displayDuration;

    this.notifications.unshift(notification);

    if (this.notifications.length > this.maxNotifications) {
      this.notifications.pop();
    }

    this.notificationQueue.push(notification);
    this.processNotificationQueue();

    this.notifyHandlers(notification);
  }

  private async processNotificationQueue(): Promise<void> {
    if (this.isProcessingQueue || this.notificationQueue.length === 0) return;
    this.isProcessingQueue = true;

    while (this.notificationQueue.length > 0) {
      const notification = this.notificationQueue.shift()!;
      await this.displayNotification(notification);
    }

    this.isProcessingQueue = false;
  }

  private async displayNotification(notification: AchievementNotification): Promise<void> {
    return new Promise(resolve => {
      setTimeout(() => {
        this.markAsRead(notification.id);
        resolve();
      }, notification.displayDuration);
    });
  }

  private notifyHandlers(notification: AchievementNotification): void {
    this.notificationHandlers.forEach((handler, type) => {
      try {
        handler(notification);
      } catch (error) {
        console.error(`Error in notification handler ${type}:`, error);
      }
    });
  }

  private generateNotificationId(): string {
    return `notif_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  public markAsRead(notificationId: string): void {
    this.readNotifications.add(notificationId);
    const notification = this.notifications.find(n => n.id === notificationId);
    if (notification) {
      notification.isRead = true;
    }
  }

  public dismissNotification(notificationId: string): void {
    this.dismissedNotifications.add(notificationId);
    const notification = this.notifications.find(n => n.id === notificationId);
    if (notification) {
      notification.isDismissed = true;
    }
  }

  public getUnreadNotifications(): AchievementNotification[] {
    return this.notifications.filter(n => !n.isRead && !n.isDismissed);
  }

  public getUnreadCount(): number {
    return this.getUnreadNotifications().length;
  }

  public getAllNotifications(): AchievementNotification[] {
    return this.notifications.filter(n => !n.isDismissed);
  }

  public getNotificationsByPriority(priority: AchievementNotification['priority']): AchievementNotification[] {
    return this.notifications.filter(n => n.priority === priority && !n.isDismissed);
  }

  public clearAllNotifications(): void {
    this.notifications = [];
    this.notificationQueue = [];
    this.dismissedNotifications.clear();
    this.readNotifications.clear();
  }

  public clearOldNotifications(maxAge: number): void {
    const cutoff = Date.now() - maxAge;
    this.notifications = this.notifications.filter(n => n.timestamp.getTime() > cutoff);
  }

  public enableSounds(): void {
    this.soundEnabled = true;
  }

  public disableSounds(): void {
    this.soundEnabled = false;
  }

  public enableAnimations(): void {
    this.animationEnabled = true;
  }

  public disableAnimations(): void {
    this.animationEnabled = false;
  }

  public setDisplayDuration(duration: number): void {
    this.displayDuration = duration;
  }

  public setMaxNotifications(max: number): void {
    this.maxNotifications = max;
  }

  public exportData(): unknown {
    return {
      notifications: this.notifications,
      dismissedNotifications: Array.from(this.dismissedNotifications),
      readNotifications: Array.from(this.readNotifications),
      soundEnabled: this.soundEnabled,
      animationEnabled: this.animationEnabled,
      displayDuration: this.displayDuration,
      maxNotifications: this.maxNotifications
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        notifications: AchievementNotification[];
        dismissedNotifications: string[];
        readNotifications: string[];
        soundEnabled: boolean;
        animationEnabled: boolean;
        displayDuration: number;
        maxNotifications: number;
      };

      this.notifications = typedData.notifications || [];
      this.dismissedNotifications = new Set(typedData.dismissedNotifications || []);
      this.readNotifications = new Set(typedData.readNotifications || []);
      this.soundEnabled = typedData.soundEnabled ?? true;
      this.animationEnabled = typedData.animationEnabled ?? true;
      this.displayDuration = typedData.displayDuration ?? 5000;
      this.maxNotifications = typedData.maxNotifications ?? 50;

      return true;
    } catch (error) {
      console.error('Failed to import notification data:', error);
      return false;
    }
  }
}

export class ProgressionMilestones {
  private milestones: Map<string, ProgressionMilestone> = new Map();
  private playerProgress: Map<string, Map<string, boolean>> = new Map();
  private tierBonuses: Map<ProgressionTier, number> = new Map();
  private milestoneDefinitions: ProgressionMilestone[] = [];

  constructor() {
    this.initializeDefaultMilestones();
    this.initializeTierBonuses();
  }

  private initializeDefaultMilestones(): void {
    this.milestoneDefinitions = [
      {
        id: 'bronze_initiate',
        name: 'Bronze Initiate',
        description: 'Begin your journey as a Bronze commander',
        tier: ProgressionTier.Bronze,
        requirements: {
          totalPoints: 0,
          achievementsOfTier: {
            [ProgressionTier.Bronze]: 1,
            [ProgressionTier.Silver]: 0,
            [ProgressionTier.Gold]: 0,
            [ProgressionTier.Platinum]: 0,
            [ProgressionTier.Diamond]: 0,
            [ProgressionTier.Master]: 0,
            [ProgressionTier.Grandmaster]: 0
          }
        },
        rewards: [
          { type: 'title', value: 'Initiate', description: 'Bronze Initiate Title', isUnique: true }
        ],
        isUnlocked: false
      },
      {
        id: 'silver_commander',
        name: 'Silver Commander',
        description: 'Achieve Silver rank in the commander hierarchy',
        tier: ProgressionTier.Silver,
        requirements: {
          totalPoints: 100,
          achievementsOfTier: {
            [ProgressionTier.Bronze]: 5,
            [ProgressionTier.Silver]: 1,
            [ProgressionTier.Gold]: 0,
            [ProgressionTier.Platinum]: 0,
            [ProgressionTier.Diamond]: 0,
            [ProgressionTier.Master]: 0,
            [ProgressionTier.Grandmaster]: 0
          }
        },
        rewards: [
          { type: 'title', value: 'Commander', description: 'Silver Commander Title', isUnique: true },
          { type: 'cosmetic', value: 'silver_badge', description: 'Silver Badge Cosmetic', isUnique: true }
        ],
        isUnlocked: false
      },
      {
        id: 'gold_warlord',
        name: 'Gold Warlord',
        description: 'Rise to Gold rank among the elite warlords',
        tier: ProgressionTier.Gold,
        requirements: {
          totalPoints: 500,
          achievementsOfTier: {
            [ProgressionTier.Bronze]: 10,
            [ProgressionTier.Silver]: 5,
            [ProgressionTier.Gold]: 1,
            [ProgressionTier.Platinum]: 0,
            [ProgressionTier.Diamond]: 0,
            [ProgressionTier.Master]: 0,
            [ProgressionTier.Grandmaster]: 0
          }
        },
        rewards: [
          { type: 'title', value: 'Warlord', description: 'Gold Warlord Title', isUnique: true },
          { type: 'cosmetic', value: 'gold_cape', description: 'Gold Cape Cosmetic', isUnique: true },
          { type: 'experience', value: 1000, description: '1000 XP Bonus', isUnique: false }
        ],
        isUnlocked: false
      },
      {
        id: 'platinum_general',
        name: 'Platinum General',
        description: 'Achieve the rank of Platinum General',
        tier: ProgressionTier.Platinum,
        requirements: {
          totalPoints: 1500,
          achievementsOfTier: {
            [ProgressionTier.Bronze]: 20,
            [ProgressionTier.Silver]: 15,
            [ProgressionTier.Gold]: 5,
            [ProgressionTier.Platinum]: 1,
            [ProgressionTier.Diamond]: 0,
            [ProgressionTier.Master]: 0,
            [ProgressionTier.Grandmaster]: 0
          }
        },
        rewards: [
          { type: 'title', value: 'General', description: 'Platinum General Title', isUnique: true },
          { type: 'cosmetic', value: 'platinum_aura', description: 'Platinum Aura Effect', isUnique: true },
          { type: 'experience', value: 2500, description: '2500 XP Bonus', isUnique: false }
        ],
        isUnlocked: false
      },
      {
        id: 'diamond_legend',
        name: 'Diamond Legend',
        description: 'Become a Diamond Legend in the RTS world',
        tier: ProgressionTier.Diamond,
        requirements: {
          totalPoints: 5000,
          achievementsOfTier: {
            [ProgressionTier.Bronze]: 30,
            [ProgressionTier.Silver]: 25,
            [ProgressionTier.Gold]: 15,
            [ProgressionTier.Platinum]: 5,
            [ProgressionTier.Diamond]: 1,
            [ProgressionTier.Master]: 0,
            [ProgressionTier.Grandmaster]: 0
          }
        },
        rewards: [
          { type: 'title', value: 'Legend', description: 'Diamond Legend Title', isUnique: true },
          { type: 'cosmetic', value: 'diamond_sparkle', description: 'Diamond Sparkle Effect', isUnique: true },
          { type: 'experience', value: 5000, description: '5000 XP Bonus', isUnique: false }
        ],
        isUnlocked: false
      },
      {
        id: 'master_conqueror',
        name: 'Master Conqueror',
        description: 'Reach the pinnacle of skill as a Master Conqueror',
        tier: ProgressionTier.Master,
        requirements: {
          totalPoints: 10000,
          achievementsOfTier: {
            [ProgressionTier.Bronze]: 40,
            [ProgressionTier.Silver]: 35,
            [ProgressionTier.Gold]: 25,
            [ProgressionTier.Platinum]: 15,
            [ProgressionTier.Diamond]: 5,
            [ProgressionTier.Master]: 1,
            [ProgressionTier.Grandmaster]: 0
          }
        },
        rewards: [
          { type: 'title', value: 'Conqueror', description: 'Master Conqueror Title', isUnique: true },
          { type: 'cosmetic', value: 'master_flame', description: 'Master Flame Effect', isUnique: true },
          { type: 'experience', value: 10000, description: '10000 XP Bonus', isUnique: false }
        ],
        isUnlocked: false
      },
      {
        id: 'grandmaster_immortal',
        name: 'Grandmaster Immortal',
        description: 'Achieve the ultimate rank of Grandmaster Immortal',
        tier: ProgressionTier.Grandmaster,
        requirements: {
          totalPoints: 25000,
          achievementsOfTier: {
            [ProgressionTier.Bronze]: 50,
            [ProgressionTier.Silver]: 50,
            [ProgressionTier.Gold]: 40,
            [ProgressionTier.Platinum]: 30,
            [ProgressionTier.Diamond]: 20,
            [ProgressionTier.Master]: 10,
            [ProgressionTier.Grandmaster]: 1
          }
        },
        rewards: [
          { type: 'title', value: 'Immortal', description: 'Grandmaster Immortal Title', isUnique: true },
          { type: 'cosmetic', value: 'immortal_aura', description: 'Immortal Aura Effect', isUnique: true },
          { type: 'experience', value: 25000, description: '25000 XP Bonus', isUnique: false }
        ],
        isUnlocked: false
      }
    ];

    this.milestoneDefinitions.forEach(milestone => {
      this.milestones.set(milestone.id, milestone);
    });
  }

  private initializeTierBonuses(): void {
    this.tierBonuses.set(ProgressionTier.Bronze, 1.0);
    this.tierBonuses.set(ProgressionTier.Silver, 1.25);
    this.tierBonuses.set(ProgressionTier.Gold, 1.5);
    this.tierBonuses.set(ProgressionTier.Platinum, 2.0);
    this.tierBonuses.set(ProgressionTier.Diamond, 2.5);
    this.tierBonuses.set(ProgressionTier.Master, 3.0);
    this.tierBonuses.set(ProgressionTier.Grandmaster, 4.0);
  }

  public checkMilestoneProgress(playerId: string, stats: AchievementStats): ProgressionMilestone | null {
    const playerMilestones = this.playerProgress.get(playerId);
    const unlockedIds = new Set<string>();
    playerMilestones?.forEach((unlocked, id) => {
      if (unlocked) unlockedIds.add(id);
    });

    for (const milestone of this.milestoneDefinitions) {
      if (unlockedIds.has(milestone.id)) continue;

      if (this.checkMilestoneRequirements(milestone, stats)) {
        this.unlockMilestone(playerId, milestone.id);
        return milestone;
      }
    }

    return null;
  }

  private checkMilestoneRequirements(milestone: ProgressionMilestone, stats: AchievementStats): boolean {
    if (stats.earnedPoints < milestone.requirements.totalPoints) {
      return false;
    }

    for (const [tier, required] of Object.entries(milestone.requirements.achievementsOfTier)) {
      const unlocked = stats.achievementsByTier.get(tier as ProgressionTier) || 0;
      if (unlocked < required) {
        return false;
      }
    }

    if (milestone.requirements.specificAchievements) {
      const allUnlocked = milestone.requirements.specificAchievements.every(id => {
        return stats.recentUnlocks.some(a => a.id === id);
      });
      if (!allUnlocked) {
        return false;
      }
    }

    return true;
  }

  public unlockMilestone(playerId: string, milestoneId: string): boolean {
    if (!this.milestones.has(milestoneId)) return false;

    if (!this.playerProgress.has(playerId)) {
      this.playerProgress.set(playerId, new Map());
    }

    const playerMilestones = this.playerProgress.get(playerId)!;
    playerMilestones.set(milestoneId, true);

    const milestone = this.milestones.get(milestoneId)!;
    milestone.isUnlocked = true;
    milestone.unlockedAt = new Date();

    return true;
  }

  public isMilestoneUnlocked(playerId: string, milestoneId: string): boolean {
    return this.playerProgress.get(playerId)?.get(milestoneId) || false;
  }

  public getUnlockedMilestones(playerId: string): ProgressionMilestone[] {
    const unlockedIds = this.playerProgress.get(playerId);
    if (!unlockedIds) return [];

    return this.milestoneDefinitions.filter(m => unlockedIds.get(m.id));
  }

  public getNextMilestone(playerId: string, currentTier?: ProgressionTier): ProgressionMilestone | null {
    const unlockedIds = this.playerProgress.get(playerId) || new Map();

    for (const milestone of this.milestoneDefinitions) {
      if (!unlockedIds.get(milestone.id)) {
        if (currentTier) {
          const currentIndex = Object.values(ProgressionTier).indexOf(currentTier);
          const milestoneIndex = Object.values(ProgressionTier).indexOf(milestone.tier);
          if (milestoneIndex <= currentIndex) continue;
        }
        return milestone;
      }
    }

    return null;
  }

  public getMilestoneProgress(playerId: string, stats: AchievementStats): {
    current: number;
    required: number;
    percentage: number;
  } {
    const nextMilestone = this.getNextMilestone(playerId);
    if (!nextMilestone) {
      return { current: stats.earnedPoints, required: stats.earnedPoints, percentage: 100 };
    }

    return {
      current: stats.earnedPoints,
      required: nextMilestone.requirements.totalPoints,
      percentage: Math.min((stats.earnedPoints / nextMilestone.requirements.totalPoints) * 100, 100)
    };
  }

  public getTierBonus(tier: ProgressionTier): number {
    return this.tierBonuses.get(tier) || 1.0;
  }

  public calculateRewardMultiplier(playerId: string): number {
    const unlockedMilestones = this.getUnlockedMilestones(playerId);
    if (unlockedMilestones.length === 0) return 1.0;

    const highestTier = unlockedMilestones.reduce((max, m) => {
      const maxIndex = Object.values(ProgressionTier).indexOf(max);
      const currentIndex = Object.values(ProgressionTier).indexOf(m.tier);
      return currentIndex > maxIndex ? m.tier : max;
    }, ProgressionTier.Bronze);

    return this.getTierBonus(highestTier);
  }

  public exportData(): unknown {
    return {
      milestones: Object.fromEntries(this.milestones),
      playerProgress: Object.fromEntries(
        Array.from(this.playerProgress.entries()).map(([playerId, progress]) => [
          playerId,
          Object.fromEntries(progress)
        ])
      ),
      tierBonuses: Object.fromEntries(this.tierBonuses)
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        milestones: Record<string, ProgressionMilestone>;
        playerProgress: Record<string, Record<string, boolean>>;
        tierBonuses: Record<string, number>;
      };

      this.milestones = new Map(Object.entries(typedData.milestones || {}));
      this.playerProgress = new Map(
        Object.entries(typedData.playerProgress || {}).map(([playerId, progress]) => [
          playerId,
          new Map(Object.entries(progress))
        ])
      );
      this.tierBonuses = new Map(Object.entries(typedData.tierBonuses || {}));

      return true;
    } catch (error) {
      console.error('Failed to import milestone data:', error);
      return false;
    }
  }
}

export class ProgressionPersistence {
  private saveManager: SaveManager | null = null;
  private autoSaveEnabled: boolean = true;
  private autoSaveInterval: number = 60000;
  private autoSaveTimer: NodeJS.Timeout | null = null;
  private lastSaveTime: Date | null = null;
  private saveSlots: number = 10;
  private currentSlot: number = 0;
  private encryptionEnabled: boolean = false;
  private compressionEnabled: boolean = true;

  constructor(saveManager?: SaveManager) {
    this.saveManager = saveManager || null;
  }

  public async save(playerId: string, data: unknown, slot?: number): Promise<boolean> {
    const saveSlot = slot ?? this.currentSlot;

    try {
      const serializedData = this.serializeData(data);

      const saveData = {
        playerId,
        timestamp: new Date().toISOString(),
        slot: saveSlot,
        data: serializedData,
        version: this.getVersion()
      };

      if (this.encryptionEnabled) {
        saveData.data = this.encryptData(serializedData);
      }

      if (this.compressionEnabled) {
        saveData.data = this.compressData(saveData.data);
      }

      if (this.saveManager) {
        await this.saveManager.save(`progression_${playerId}_${saveSlot}`, saveData);
      } else {
        localStorage.setItem(`progression_${playerId}_${saveSlot}`, JSON.stringify(saveData));
      }

      this.lastSaveTime = new Date();
      return true;
    } catch (error) {
      console.error('Failed to save progression data:', error);
      return false;
    }
  }

  public async load(playerId: string, slot?: number): Promise<unknown | null> {
    const loadSlot = slot ?? this.currentSlot;

    try {
      let saveData: unknown;

      if (this.saveManager) {
        saveData = await this.saveManager.load(`progression_${playerId}_${loadSlot}`);
      } else {
        const stored = localStorage.getItem(`progression_${playerId}_${loadSlot}`);
        saveData = stored ? JSON.parse(stored) : null;
      }

      if (!saveData) return null;

      const typedData = saveData as {
        data: unknown;
        version: string;
      };

      let data = typedData.data;

      if (this.compressionEnabled) {
        data = this.decompressData(data);
      }

      if (this.encryptionEnabled) {
        data = this.decryptData(data);
      }

      return this.deserializeData(data);
    } catch (error) {
      console.error('Failed to load progression data:', error);
      return null;
    }
  }

  public async delete(playerId: string, slot?: number): Promise<boolean> {
    const deleteSlot = slot ?? this.currentSlot;

    try {
      if (this.saveManager) {
        await this.saveManager.delete(`progression_${playerId}_${deleteSlot}`);
      } else {
        localStorage.removeItem(`progression_${playerId}_${deleteSlot}`);
      }

      return true;
    } catch (error) {
      console.error('Failed to delete progression data:', error);
      return false;
    }
  }

  public async listSaves(playerId: string): Promise<Array<{ slot: number; timestamp: Date | null }>> {
    const saves: Array<{ slot: number; timestamp: Date | null }> = [];

    for (let i = 0; i < this.saveSlots; i++) {
      try {
        let saveData: unknown;

        if (this.saveManager) {
          saveData = await this.saveManager.load(`progression_${playerId}_${i}`);
        } else {
          const stored = localStorage.getItem(`progression_${playerId}_${i}`);
          saveData = stored ? JSON.parse(stored) : null;
        }

        saves.push({
          slot: i,
          timestamp: saveData ? new Date((saveData as { timestamp: string }).timestamp) : null
        });
      } catch {
        saves.push({ slot: i, timestamp: null });
      }
    }

    return saves;
  }

  private serializeData(data: unknown): string {
    return JSON.stringify(data);
  }

  private deserializeData(data: unknown): unknown {
    return typeof data === 'string' ? JSON.parse(data) : data;
  }

  private encryptData(data: string): string {
    return Buffer.from(data).toString('base64');
  }

  private decryptData(data: unknown): string {
    return Buffer.from(data as string, 'base64').toString('utf8');
  }

  private compressData(data: unknown): unknown {
    return data;
  }

  private decompressData(data: unknown): unknown {
    return data;
  }

  private getVersion(): string {
    return '1.0.0';
  }

  public enableAutoSave(interval?: number): void {
    this.autoSaveEnabled = true;
    if (interval) {
      this.autoSaveInterval = interval;
    }

    if (this.autoSaveTimer) {
      clearInterval(this.autoSaveTimer);
    }

    this.autoSaveTimer = setInterval(() => {
      this.triggerAutoSave();
    }, this.autoSaveInterval);
  }

  public disableAutoSave(): void {
    this.autoSaveEnabled = false;
    if (this.autoSaveTimer) {
      clearInterval(this.autoSaveTimer);
      this.autoSaveTimer = null;
    }
  }

  private async triggerAutoSave(): Promise<void> {
    console.log('Auto-saving progression data...');
  }

  public getLastSaveTime(): Date | null {
    return this.lastSaveTime;
  }

  public setSaveSlots(slots: number): void {
    this.saveSlots = Math.max(1, Math.min(slots, 100));
  }

  public setCurrentSlot(slot: number): void {
    this.currentSlot = Math.max(0, Math.min(slot, this.saveSlots - 1));
  }

  public enableEncryption(): void {
    this.encryptionEnabled = true;
  }

  public disableEncryption(): void {
    this.encryptionEnabled = false;
  }

  public enableCompression(): void {
    this.compressionEnabled = true;
  }

  public disableCompression(): void {
    this.compressionEnabled = false;
  }
}

export class AchievementCategories {
  private categories: Map<string, AchievementCategory> = new Map();
  private categoryAssignments: Map<string, string> = new Map();
  private defaultCategories: AchievementCategory[] = [];

  constructor() {
    this.initializeDefaultCategories();
  }

  private initializeDefaultCategories(): void {
    this.defaultCategories = [
      {
        id: 'combat_mastery',
        name: 'Combat Mastery',
        description: 'Achievements related to combat and battle victories',
        icon: 'sword',
        achievementIds: [],
        sortOrder: 1,
        isExpanded: true,
        completionPercentage: 0
      },
      {
        id: 'economic_prowess',
        name: 'Economic Prowess',
        description: 'Achievements related to resource management and economy',
        icon: 'coins',
        achievementIds: [],
        sortOrder: 2,
        isExpanded: true,
        completionPercentage: 0
      },
      {
        id: 'exploration_discoveries',
        name: 'Exploration & Discoveries',
        description: 'Achievements related to map exploration and discoveries',
        icon: 'compass',
        achievementIds: [],
        sortOrder: 3,
        isExpanded: true,
        completionPercentage: 0
      },
      {
        id: 'technological_advancement',
        name: 'Technological Advancement',
        description: 'Achievements related to research and technology',
        icon: 'flask',
        achievementIds: [],
        sortOrder: 4,
        isExpanded: true,
        completionPercentage: 0
      },
      {
        id: 'construction_wonders',
        name: 'Construction & Wonders',
        description: 'Achievements related to building and construction',
        icon: 'hammer',
        achievementIds: [],
        sortOrder: 5,
        isExpanded: true,
        completionPercentage: 0
      },
      {
        id: 'social_interaction',
        name: 'Social Interaction',
        description: 'Achievements related to multiplayer and social features',
        icon: 'users',
        achievementIds: [],
        sortOrder: 6,
        isExpanded: true,
        completionPercentage: 0
      },
      {
        id: 'special_challenges',
        name: 'Special Challenges',
        description: 'Rare and special achievement challenges',
        icon: 'star',
        achievementIds: [],
        sortOrder: 7,
        isExpanded: false,
        completionPercentage: 0
      },
      {
        id: 'seasonal_events',
        name: 'Seasonal Events',
        description: 'Time-limited seasonal achievements',
        icon: 'calendar',
        achievementIds: [],
        sortOrder: 8,
        isExpanded: false,
        completionPercentage: 0
      },
      {
        id: 'hidden_secrets',
        name: 'Hidden Secrets',
        description: 'Secret achievements waiting to be discovered',
        icon: 'key',
        achievementIds: [],
        sortOrder: 9,
        isExpanded: false,
        completionPercentage: 0
      },
      {
        id: 'meta_progression',
        name: 'Meta Progression',
        description: 'Achievements related to overall game progression',
        icon: 'trophy',
        achievementIds: [],
        sortOrder: 10,
        isExpanded: true,
        completionPercentage: 0
      }
    ];

    this.defaultCategories.forEach(category => {
      this.categories.set(category.id, category);
    });
  }

  public createCategory(category: AchievementCategory): boolean {
    if (this.categories.has(category.id)) {
      return false;
    }

    this.categories.set(category.id, category);
    return true;
  }

  public getCategory(categoryId: string): AchievementCategory | undefined {
    return this.categories.get(categoryId);
  }

  public getAllCategories(): AchievementCategory[] {
    return Array.from(this.categories.values()).sort((a, b) => a.sortOrder - b.sortOrder);
  }

  public assignAchievement(achievementId: string, categoryId: string): boolean {
    if (!this.categories.has(categoryId)) {
      return false;
    }

    const previousCategoryId = this.categoryAssignments.get(achievementId);
    if (previousCategoryId) {
      const previousCategory = this.categories.get(previousCategoryId);
      if (previousCategory) {
        previousCategory.achievementIds = previousCategory.achievementIds.filter(id => id !== achievementId);
      }
    }

    this.categoryAssignments.set(achievementId, categoryId);

    const category = this.categories.get(categoryId)!;
    if (!category.achievementIds.includes(achievementId)) {
      category.achievementIds.push(achievementId);
    }

    return true;
  }

  public getAchievementCategory(achievementId: string): string | undefined {
    return this.categoryAssignments.get(achievementId);
  }

  public removeAchievement(achievementId: string): boolean {
    const categoryId = this.categoryAssignments.get(achievementId);
    if (!categoryId) return false;

    const category = this.categories.get(categoryId);
    if (category) {
      category.achievementIds = category.achievementIds.filter(id => id !== achievementId);
    }

    this.categoryAssignments.delete(achievementId);
    return true;
  }

  public updateCategory(categoryId: string, updates: Partial<AchievementCategory>): boolean {
    const category = this.categories.get(categoryId);
    if (!category) return false;

    Object.assign(category, updates);
    return true;
  }

  public deleteCategory(categoryId: string): boolean {
    const category = this.categories.get(categoryId);
    if (!category) return false;

    category.achievementIds.forEach(achievementId => {
      this.categoryAssignments.delete(achievementId);
    });

    this.categories.delete(categoryId);
    return true;
  }

  public getCategoryAchievements(categoryId: string): string[] {
    const category = this.categories.get(categoryId);
    return category?.achievementIds || [];
  }

  public getCategoryProgress(
    categoryId: string,
    unlockedAchievements: Set<string>
  ): { unlocked: number; total: number; percentage: number } {
    const category = this.categories.get(categoryId);
    if (!category) {
      return { unlocked: 0, total: 0, percentage: 0 };
    }

    const total = category.achievementIds.length;
    const unlocked = category.achievementIds.filter(id => unlockedAchievements.has(id)).length;
    const percentage = total > 0 ? (unlocked / total) * 100 : 0;

    return { unlocked, total, percentage };
  }

  public setCategoryExpanded(categoryId: string, expanded: boolean): boolean {
    const category = this.categories.get(categoryId);
    if (!category) return false;

    category.isExpanded = expanded;
    return true;
  }

  public reorderCategories(categoryIds: string[]): boolean {
    categoryIds.forEach((id, index) => {
      const category = this.categories.get(id);
      if (category) {
        category.sortOrder = index + 1;
      }
    });

    return true;
  }

  public searchCategories(query: string): AchievementCategory[] {
    const lowerQuery = query.toLowerCase();
    return this.getAllCategories().filter(category => {
      return (
        category.name.toLowerCase().includes(lowerQuery) ||
        category.description.toLowerCase().includes(lowerQuery) ||
        category.tags?.some(tag => tag.toLowerCase().includes(lowerQuery))
      );
    });
  }

  public exportData(): unknown {
    return {
      categories: Object.fromEntries(this.categories),
      categoryAssignments: Object.fromEntries(this.categoryAssignments)
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        categories: Record<string, AchievementCategory>;
        categoryAssignments: Record<string, string>;
      };

      this.categories = new Map(Object.entries(typedData.categories || {}));
      this.categoryAssignments = new Map(Object.entries(typedData.categoryAssignments || {}));

      return true;
    } catch (error) {
      console.error('Failed to import category data:', error);
      return false;
    }
  }
}

export class AchievementStats {
  private statsCache: Map<string, AchievementStats> = new Map();
  private calculationHistory: Array<{ playerId: string; timestamp: Date }> = [];
  private maxHistorySize: number = 100;

  public calculateStats(
    achievements: Achievement[],
    unlockedIds: Set<string>
  ): AchievementStats {
    const stats: AchievementStats = {
      totalAchievements: achievements.length,
      unlockedAchievements: unlockedIds.size,
      lockedAchievements: achievements.length - unlockedIds.size,
      completionPercentage: achievements.length > 0 ? (unlockedIds.size / achievements.length) * 100 : 0,
      totalPoints: achievements.reduce((sum, a) => sum + a.points, 0),
      earnedPoints: achievements
        .filter(a => unlockedIds.has(a.id))
        .reduce((sum, a) => sum + a.points, 0),
      achievementsByType: new Map(),
      achievementsByTier: new Map(),
      recentUnlocks: [],
      longestStreak: 0,
      currentStreak: 0,
      averageCompletionTime: 0,
      mostDifficultUnlocked: ProgressionTier.Bronze,
      categoryCompletion: new Map(),
      seasonalProgress: new Map(),
      hiddenAchievementsFound: 0
    };

    const typeCounts = new Map<AchievementType, number>();
    const tierCounts = new Map<ProgressionTier, number>();

    achievements.forEach(achievement => {
      const currentTypeCount = typeCounts.get(achievement.type) || 0;
      typeCounts.set(achievement.type, currentTypeCount + 1);

      const currentTierCount = tierCounts.get(achievement.tier) || 0;
      tierCounts.set(achievement.tier, currentTierCount + 1);
    });

    stats.achievementsByType = typeCounts;
    stats.achievementsByTier = tierCounts;

    const unlockedAchievements = achievements.filter(a => unlockedIds.has(a.id));
    stats.recentUnlocks = unlockedAchievements
      .filter(a => a.unlockedAt)
      .sort((a, b) => (b.unlockedAt?.getTime() || 0) - (a.unlockedAt?.getTime() || 0))
      .slice(0, 10);

    const hiddenAchievements = achievements.filter(a => a.isSecret && unlockedIds.has(a.id));
    stats.hiddenAchievementsFound = hiddenAchievements.length;

    const tierOrder = Object.values(ProgressionTier);
    const unlockedTiers = unlockedAchievements.map(a => a.tier);
    const maxTierIndex = Math.max(...unlockedTiers.map(t => tierOrder.indexOf(t)));
    stats.mostDifficultUnlocked = tierOrder[maxTierIndex];

    if (unlockedAchievements.length > 1) {
      const completionTimes = unlockedAchievements
        .filter(a => a.unlockedAt && a.lastUpdated)
        .map(a => (a.unlockedAt?.getTime() || 0) - (a.lastUpdated?.getTime() || 0));

      if (completionTimes.length > 0) {
        stats.averageCompletionTime = completionTimes.reduce((sum, time) => sum + time, 0) / completionTimes.length;
      }
    }

    return stats;
  }

  public cacheStats(playerId: string, stats: AchievementStats): void {
    this.statsCache.set(playerId, stats);
    this.calculationHistory.push({ playerId, timestamp: new Date() });

    if (this.calculationHistory.length > this.maxHistorySize) {
      this.calculationHistory.shift();
    }
  }

  public getCachedStats(playerId: string): AchievementStats | undefined {
    return this.statsCache.get(playerId);
  }

  public clearCache(playerId?: string): void {
    if (playerId) {
      this.statsCache.delete(playerId);
    } else {
      this.statsCache.clear();
    }
  }

  public getCalculationHistory(): Array<{ playerId: string; timestamp: Date }> {
    return [...this.calculationHistory];
  }

  public generateStatsReport(stats: AchievementStats): string {
    const lines: string[] = [];

    lines.push('=== Achievement Statistics Report ===');
    lines.push('');
    lines.push(`Total Achievements: ${stats.totalAchievements}`);
    lines.push(`Unlocked: ${stats.unlockedAchievements}`);
    lines.push(`Locked: ${stats.lockedAchievements}`);
    lines.push(`Completion: ${stats.completionPercentage.toFixed(2)}%`);
    lines.push('');
    lines.push(`Total Points: ${stats.totalPoints}`);
    lines.push(`Earned Points: ${stats.earnedPoints}`);
    lines.push('');
    lines.push('Achievements by Type:');
    stats.achievementsByType.forEach((count, type) => {
      lines.push(`  ${type}: ${count}`);
    });
    lines.push('');
    lines.push('Achievements by Tier:');
    stats.achievementsByTier.forEach((count, tier) => {
      lines.push(`  ${tier}: ${count}`);
    });
    lines.push('');
    lines.push(`Most Difficult Unlocked: ${stats.mostDifficultUnlocked}`);
    lines.push(`Hidden Achievements Found: ${stats.hiddenAchievementsFound}`);
    lines.push('');

    if (stats.recentUnlocks.length > 0) {
      lines.push('Recent Unlocks:');
      stats.recentUnlocks.slice(0, 5).forEach(a => {
        lines.push(`  - ${a.name} (${a.tier}) - ${a.unlockedAt?.toLocaleDateString()}`);
      });
    }

    return lines.join('\n');
  }

  public compareStats(stats1: AchievementStats, stats2: AchievementStats): {
    differences: Record<string, { stat1: number | string; stat2: number | string }>;
    winner: 'stat1' | 'stat2' | 'tie';
  } {
    const differences: Record<string, { stat1: number | string; stat2: number | string }> = {};

    if (stats1.unlockedAchievements !== stats2.unlockedAchievements) {
      differences.unlockedAchievements = {
        stat1: stats1.unlockedAchievements,
        stat2: stats2.unlockedAchievements
      };
    }

    if (stats1.earnedPoints !== stats2.earnedPoints) {
      differences.earnedPoints = {
        stat1: stats1.earnedPoints,
        stat2: stats2.earnedPoints
      };
    }

    if (stats1.completionPercentage !== stats2.completionPercentage) {
      differences.completionPercentage = {
        stat1: stats1.completionPercentage.toFixed(2),
        stat2: stats2.completionPercentage.toFixed(2)
      };
    }

    let winner: 'stat1' | 'stat2' | 'tie' = 'tie';
    if (stats1.earnedPoints > stats2.earnedPoints) {
      winner = 'stat1';
    } else if (stats2.earnedPoints > stats1.earnedPoints) {
      winner = 'stat2';
    }

    return { differences, winner };
  }

  public exportData(): unknown {
    return {
      statsCache: Object.fromEntries(this.statsCache),
      calculationHistory: this.calculationHistory
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        statsCache: Record<string, AchievementStats>;
        calculationHistory: Array<{ playerId: string; timestamp: string }>;
      };

      this.statsCache = new Map(Object.entries(typedData.statsCache || {}));
      this.calculationHistory = (typedData.calculationHistory || []).map(entry => ({
        ...entry,
        timestamp: new Date(entry.timestamp)
      }));

      return true;
    } catch (error) {
      console.error('Failed to import stats data:', error);
      return false;
    }
  }
}

export class SeasonalAchievements {
  private seasonalEvents: Map<string, SeasonalEvent> = new Map();
  private playerParticipation: Map<string, Map<string, boolean>> = new Map();
  private activeEventId: string | null = null;
  private eventHistory: Map<string, SeasonalEvent[]> = new Map();

  constructor() {
    this.initializeDefaultEvents();
  }

  private initializeDefaultEvents(): void {
    const currentYear = new Date().getFullYear();

    const springEvent: SeasonalEvent = {
      id: 'spring_rising',
      name: 'Spring Rising',
      description: 'Celebrate the season of renewal with spring-themed challenges',
      startDate: new Date(currentYear, 2, 20),
      endDate: new Date(currentYear, 4, 5),
      theme: 'spring',
      achievements: ['spring_champion', 'bloom_master', 'renewal_hero'],
      bonusRewards: [
        { type: 'cosmetic', value: 'spring_flower', description: 'Spring Flower Cosmetic', isUnique: true }
      ],
      isActive: false,
      participationCount: 0,
      completionPercentage: 0
    };

    const summerEvent: SeasonalEvent = {
      id: 'summer_conquest',
      name: 'Summer Conquest',
      description: 'Battle through the heat with summer challenges',
      startDate: new Date(currentYear, 5, 21),
      endDate: new Date(currentYear, 7, 31),
      theme: 'summer',
      achievements: ['summer_warrior', 'sun_champion', 'heat_master'],
      bonusRewards: [
        { type: 'cosmetic', value: 'summer_flame', description: 'Summer Flame Effect', isUnique: true }
      ],
      isActive: false,
      participationCount: 0,
      completionPercentage: 0
    };

    const autumnEvent: SeasonalEvent = {
      id: 'autumn_glory',
      name: 'Autumn Glory',
      description: 'Harvest rewards with autumn-themed achievements',
      startDate: new Date(currentYear, 8, 22),
      endDate: new Date(currentYear, 10, 30),
      theme: 'autumn',
      achievements: ['autumn_harvester', 'fall_champion', 'harvest_master'],
      bonusRewards: [
        { type: 'cosmetic', value: 'autumn_leaves', description: 'Autumn Leaves Effect', isUnique: true }
      ],
      isActive: false,
      participationCount: 0,
      completionPercentage: 0
    };

    const winterEvent: SeasonalEvent = {
      id: 'winter_wrath',
      name: 'Winter Wrath',
      description: 'Endure the cold with challenging winter achievements',
      startDate: new Date(currentYear, 11, 21),
      endDate: new Date(currentYear + 1, 1, 19),
      theme: 'winter',
      achievements: ['winter_survivor', 'ice_champion', 'frost_master'],
      bonusRewards: [
        { type: 'cosmetic', value: 'winter_frost', description: 'Winter Frost Effect', isUnique: true }
      ],
      isActive: false,
      participationCount: 0,
      completionPercentage: 0
    };

    this.seasonalEvents.set(springEvent.id, springEvent);
    this.seasonalEvents.set(summerEvent.id, summerEvent);
    this.seasonalEvents.set(autumnEvent.id, autumnEvent);
    this.seasonalEvents.set(winterEvent.id, winterEvent);

    this.updateActiveEvents();
  }

  public createSeasonalEvent(event: SeasonalEvent): boolean {
    if (this.seasonalEvents.has(event.id)) {
      return false;
    }

    this.seasonalEvents.set(event.id, event);
    this.updateActiveEvents();
    return true;
  }

  public getSeasonalEvent(eventId: string): SeasonalEvent | undefined {
    return this.seasonalEvents.get(eventId);
  }

  public getAllSeasonalEvents(): SeasonalEvent[] {
    return Array.from(this.seasonalEvents.values());
  }

  public getActiveEvents(): SeasonalEvent[] {
    return this.getAllSeasonalEvents().filter(e => e.isActive);
  }

  public getCurrentEvent(): SeasonalEvent | null {
    const activeEvents = this.getActiveEvents();
    return activeEvents.length > 0 ? activeEvents[0] : null;
  }

  public updateActiveEvents(): void {
    const now = new Date();

    this.seasonalEvents.forEach(event => {
      const wasActive = event.isActive;
      event.isActive = now >= event.startDate && now <= event.endDate;

      if (event.isActive && !wasActive) {
        this.activeEventId = event.id;
      } else if (!event.isActive && wasActive && this.activeEventId === event.id) {
        this.archiveEvent(event);
        this.activeEventId = null;
      }
    });
  }

  private archiveEvent(event: SeasonalEvent): void {
    if (!this.eventHistory.has(event.id)) {
      this.eventHistory.set(event.id, []);
    }
    this.eventHistory.get(event.id)!.push({ ...event });
  }

  public participateInEvent(playerId: string, eventId: string): boolean {
    if (!this.seasonalEvents.has(eventId)) {
      return false;
    }

    const event = this.seasonalEvents.get(eventId)!;
    if (!event.isActive) {
      return false;
    }

    if (!this.playerParticipation.has(playerId)) {
      this.playerParticipation.set(playerId, new Map());
    }

    const playerEvents = this.playerParticipation.get(playerId)!;
    playerEvents.set(eventId, true);

    event.participationCount++;

    return true;
  }

  public hasParticipated(playerId: string, eventId: string): boolean {
    return this.playerParticipation.get(playerId)?.get(eventId) || false;
  }

  public getEventProgress(
    playerId: string,
    eventId: string,
    unlockedAchievementIds: Set<string>
  ): { unlocked: number; total: number; percentage: number } {
    const event = this.seasonalEvents.get(eventId);
    if (!event) {
      return { unlocked: 0, total: 0, percentage: 0 };
    }

    const total = event.achievements.length;
    const unlocked = event.achievements.filter(id => unlockedAchievementIds.has(id)).length;
    const percentage = total > 0 ? (unlocked / total) * 100 : 0;

    event.completionPercentage = percentage;

    return { unlocked, total, percentage };
  }

  public getSeasonalBonusRewards(eventId: string): UnlockReward[] {
    const event = this.seasonalEvents.get(eventId);
    return event?.bonusRewards || [];
  }

  public getCompletedEvents(playerId: string): SeasonalEvent[] {
    const completed: SeasonalEvent[] = [];

    this.playerParticipation.get(playerId)?.forEach((participated, eventId) => {
      if (participated) {
        const event = this.seasonalEvents.get(eventId);
        if (event && !event.isActive) {
          completed.push(event);
        }
      }
    });

    return completed;
  }

  public getUpcomingEvents(): SeasonalEvent[] {
    const now = new Date();
    return this.getAllSeasonalEvents()
      .filter(e => e.startDate > now)
      .sort((a, b) => a.startDate.getTime() - b.startDate.getTime());
  }

  public getEventByTheme(theme: string): SeasonalEvent | undefined {
    return this.getAllSeasonalEvents().find(e => e.theme === theme);
  }

  public getEventByDate(date: Date): SeasonalEvent | undefined {
    return this.getAllSeasonalEvents().find(e => date >= e.startDate && date <= e.endDate);
  }

  public deleteSeasonalEvent(eventId: string): boolean {
    return this.seasonalEvents.delete(eventId);
  }

  public exportData(): unknown {
    return {
      seasonalEvents: Object.fromEntries(this.seasonalEvents),
      playerParticipation: Object.fromEntries(
        Array.from(this.playerParticipation.entries()).map(([playerId, events]) => [
          playerId,
          Object.fromEntries(events)
        ])
      ),
      activeEventId: this.activeEventId,
      eventHistory: Object.fromEntries(this.eventHistory)
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        seasonalEvents: Record<string, SeasonalEvent>;
        playerParticipation: Record<string, Record<string, boolean>>;
        activeEventId: string | null;
        eventHistory: Record<string, SeasonalEvent[]>;
      };

      this.seasonalEvents = new Map(Object.entries(typedData.seasonalEvents || {}));
      this.playerParticipation = new Map(
        Object.entries(typedData.playerParticipation || {}).map(([playerId, events]) => [
          playerId,
          new Map(Object.entries(events))
        ])
      );
      this.activeEventId = typedData.activeEventId || null;
      this.eventHistory = new Map(Object.entries(typedData.eventHistory || {}));

      return true;
    } catch (error) {
      console.error('Failed to import seasonal data:', error);
      return false;
    }
  }
}

export class AchievementUIIntegration {
  private uiElements: AchievementUIElements = {
    panel: null,
    achievementList: null,
    categoryTabs: null,
    progressBar: null,
    notificationArea: null,
    filterControls: null,
    searchInput: null,
    sortDropdown: null,
    detailModal: null,
    comparisonPanel: null
  };

  private isPanelVisible: boolean = false;
  private currentFilter: AchievementType | 'all' = 'all';
  private currentSort: 'name' | 'tier' | 'progress' | 'unlocked' = 'tier';
  private searchQuery: string = '';
  private isInitialized: boolean = false;

  public initialize(containerSelector: string): void {
    this.createUIElements(containerSelector);
    this.attachEventListeners();
    this.isInitialized = true;
  }

  private createUIElements(containerSelector: string): void {
    const container = document.querySelector(containerSelector);
    if (!container) return;

    const panel = document.createElement('div');
    panel.id = 'achievement-panel';
    panel.className = 'achievement-panel';
    panel.style.display = 'none';

    const header = document.createElement('div');
    header.className = 'achievement-header';
    header.innerHTML = `
      <h2>Achievements</h2>
      <button class="close-btn">&times;</button>
    `;

    const searchContainer = document.createElement('div');
    searchContainer.className = 'search-container';
    searchContainer.innerHTML = `
      <input type="text" class="search-input" placeholder="Search achievements..." />
    `;

    const filterContainer = document.createElement('div');
    filterContainer.className = 'filter-container';
    filterContainer.innerHTML = `
      <select class="filter-dropdown">
        <option value="all">All Types</option>
        <option value="combat">Combat</option>
        <option value="economy">Economy</option>
        <option value="exploration">Exploration</option>
        <option value="technology">Technology</option>
        <option value="construction">Construction</option>
        <option value="social">Social</option>
        <option value="special">Special</option>
        <option value="seasonal">Seasonal</option>
        <option value="hidden">Hidden</option>
        <option value="meta">Meta</option>
      </select>
      <select class="sort-dropdown">
        <option value="tier">Sort by Tier</option>
        <option value="name">Sort by Name</option>
        <option value="progress">Sort by Progress</option>
        <option value="unlocked">Sort by Unlocked</option>
      </select>
    `;

    const progressContainer = document.createElement('div');
    progressContainer.className = 'progress-container';
    progressContainer.innerHTML = `
      <div class="overall-progress">
        <span class="progress-label">Overall Progress</span>
        <div class="progress-bar">
          <div class="progress-fill"></div>
        </div>
        <span class="progress-text">0%</span>
      </div>
    `;

    const categoryTabs = document.createElement('div');
    categoryTabs.className = 'category-tabs';

    const achievementList = document.createElement('div');
    achievementList.className = 'achievement-list';

    const notificationArea = document.createElement('div');
    notificationArea.className = 'notification-area';

    panel.appendChild(header);
    panel.appendChild(searchContainer);
    panel.appendChild(filterContainer);
    panel.appendChild(progressContainer);
    panel.appendChild(categoryTabs);
    panel.appendChild(achievementList);
    panel.appendChild(notificationArea);

    container.appendChild(panel);

    this.uiElements = {
      panel,
      achievementList,
      categoryTabs,
      progressBar: progressContainer.querySelector('.progress-fill'),
      notificationArea,
      filterControls: filterContainer,
      searchInput: searchContainer.querySelector('.search-input'),
      sortDropdown: filterContainer.querySelector('.sort-dropdown'),
      detailModal: null,
      comparisonPanel: null
    };
  }

  private attachEventListeners(): void {
    const closeBtn = this.uiElements.panel?.querySelector('.close-btn');
    closeBtn?.addEventListener('click', () => this.hidePanel());

    const searchInput = this.uiElements.searchInput as HTMLInputElement;
    searchInput?.addEventListener('input', (e) => {
      this.searchQuery = (e.target as HTMLInputElement).value;
      this.applyFilters();
    });

    const filterDropdown = this.uiElements.filterControls?.querySelector('.filter-dropdown') as HTMLSelectElement;
    filterDropdown?.addEventListener('change', (e) => {
      this.currentFilter = (e.target as HTMLSelectElement).value as AchievementType | 'all';
      this.applyFilters();
    });

    const sortDropdown = this.uiElements.sortDropdown as HTMLSelectElement;
    sortDropdown?.addEventListener('change', (e) => {
      this.currentSort = (e.target as HTMLSelectElement).value as typeof this.currentSort;
      this.applyFilters();
    });
  }

  public showPanel(): void {
    if (!this.uiElements.panel) return;
    this.uiElements.panel.style.display = 'block';
    this.isPanelVisible = true;
  }

  public hidePanel(): void {
    if (!this.uiElements.panel) return;
    this.uiElements.panel.style.display = 'none';
    this.isPanelVisible = false;
  }

  public togglePanel(): void {
    if (this.isPanelVisible) {
      this.hidePanel();
    } else {
      this.showPanel();
    }
  }

  public renderAchievements(
    achievements: Achievement[],
    unlockedIds: Set<string>,
    categories: AchievementCategory[]
  ): void {
    if (!this.uiElements.achievementList) return;

    const filteredAchievements = this.filterAchievements(achievements);
    const sortedAchievements = this.sortAchievements(filteredAchievements);

    this.uiElements.achievementList.innerHTML = '';

    sortedAchievements.forEach(achievement => {
      const isUnlocked = unlockedIds.has(achievement.id);
      const element = this.createAchievementElement(achievement, isUnlocked);
      this.uiElements.achievementList?.appendChild(element);
    });

    this.updateCategoryTabs(categories, unlockedIds, achievements);
    this.updateOverallProgress(unlockedIds.size, achievements.length);
  }

  private filterAchievements(achievements: Achievement[]): Achievement[] {
    let filtered = achievements;

    if (this.currentFilter !== 'all') {
      filtered = filtered.filter(a => a.type === this.currentFilter);
    }

    if (this.searchQuery) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(a => {
        const nameMatch = a.name.toLowerCase().includes(query);
        const descMatch = a.description.toLowerCase().includes(query);
        const tagMatch = a.tags.some(tag => tag.toLowerCase().includes(query));
        return nameMatch || descMatch || tagMatch;
      });
    }

    return filtered;
  }

  private sortAchievements(achievements: Achievement[]): Achievement[] {
    const sorted = [...achievements];

    switch (this.currentSort) {
      case 'tier':
        const tierOrder = Object.values(ProgressionTier).reverse();
        sorted.sort((a, b) => {
          const aIndex = tierOrder.indexOf(a.tier);
          const bIndex = tierOrder.indexOf(b.tier);
          return aIndex - bIndex;
        });
        break;
      case 'name':
        sorted.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case 'progress':
        sorted.sort((a, b) => {
          const aProgress = a.progress || 0;
          const bProgress = b.progress || 0;
          return bProgress - aProgress;
        });
        break;
      case 'unlocked':
        sorted.sort((a, b) => {
          const aUnlocked = a.unlockedAt ? 1 : 0;
          const bUnlocked = b.unlockedAt ? 1 : 0;
          return bUnlocked - aUnlocked;
        });
        break;
    }

    return sorted;
  }

  private createAchievementElement(achievement: Achievement, isUnlocked: boolean): HTMLElement {
    const element = document.createElement('div');
    element.className = `achievement-item ${isUnlocked ? 'unlocked' : 'locked'}`;
    element.dataset.achievementId = achievement.id;

    const tierClass = achievement.tier.toLowerCase();
    const iconSrc = isUnlocked || !achievement.isSecret ? achievement.icon : 'secret_icon';

    element.innerHTML = `
      <div class="achievement-icon ${tierClass}">
        <img src="${iconSrc}" alt="${achievement.name}" />
        <span class="tier-badge ${tierClass}">${achievement.tier}</span>
      </div>
      <div class="achievement-info">
        <h3 class="achievement-name">${achievement.name}</h3>
        <p class="achievement-description">${isUnlocked || !achievement.isSecret ? achievement.description : '???'}</p>
        <div class="achievement-meta">
          <span class="achievement-points">${achievement.points} pts</span>
          <span class="achievement-type">${achievement.type}</span>
        </div>
        ${!isUnlocked && achievement.progress !== undefined ? `
          <div class="achievement-progress">
            <div class="progress-bar">
              <div class="progress-fill" style="width: ${achievement.progress}%"></div>
            </div>
            <span class="progress-text">${achievement.progress.toFixed(0)}%</span>
          </div>
        ` : ''}
      </div>
      ${isUnlocked ? '<div class="unlock-indicator">✓</div>' : ''}
    `;

    element.addEventListener('click', () => this.showAchievementDetails(achievement));

    return element;
  }

  private updateCategoryTabs(
    categories: AchievementCategory[],
    unlockedIds: Set<string>,
    achievements: Achievement[]
  ): void {
    if (!this.uiElements.categoryTabs) return;

    this.uiElements.categoryTabs.innerHTML = '';

    const allTab = document.createElement('button');
    allTab.className = `category-tab ${this.currentFilter === 'all' ? 'active' : ''}`;
    allTab.textContent = `All (${achievements.length})`;
    allTab.addEventListener('click', () => {
      this.currentFilter = 'all';
      this.applyFilters();
    });
    this.uiElements.categoryTabs.appendChild(allTab);

    categories.forEach(category => {
      const tab = document.createElement('button');
      const progress = this.calculateCategoryProgress(category.id, unlockedIds, achievements);
      tab.className = `category-tab ${this.currentFilter === category.id ? 'active' : ''}`;
      tab.textContent = `${category.name} (${progress.unlocked}/${progress.total})`;
      tab.dataset.categoryId = category.id;
      tab.addEventListener('click', () => {
        this.currentFilter = category.id as AchievementType;
        this.applyFilters();
      });
      this.uiElements.categoryTabs.appendChild(tab);
    });
  }

  private calculateCategoryProgress(
    categoryId: string,
    unlockedIds: Set<string>,
    achievements: Achievement[]
  ): { unlocked: number; total: number } {
    const categoryAchievements = achievements.filter(a => a.categoryId === categoryId);
    const unlocked = categoryAchievements.filter(a => unlockedIds.has(a.id)).length;
    return { unlocked, total: categoryAchievements.length };
  }

  private updateOverallProgress(unlocked: number, total: number): void {
    const percentage = total > 0 ? (unlocked / total) * 100 : 0;

    if (this.uiElements.progressBar) {
      (this.uiElements.progressBar as HTMLElement).style.width = `${percentage}%`;
    }

    const progressText = this.uiElements.progressBar?.parentElement?.querySelector('.progress-text');
    if (progressText) {
      progressText.textContent = `${percentage.toFixed(0)}% (${unlocked}/${total})`;
    }
  }

  private applyFilters(): void {
    document.dispatchEvent(new CustomEvent('achievementsFilterChanged', {
      detail: {
        filter: this.currentFilter,
        sort: this.currentSort,
        search: this.searchQuery
      }
    }));
  }

  public showAchievementDetails(achievement: Achievement): void {
    let modal = document.getElementById('achievement-detail-modal');

    if (!modal) {
      modal = document.createElement('div');
      modal.id = 'achievement-detail-modal';
      modal.className = 'modal achievement-detail-modal';
      modal.innerHTML = `
        <div class="modal-content">
          <div class="modal-header">
            <h2 class="achievement-title"></h2>
            <button class="close-modal">&times;</button>
          </div>
          <div class="modal-body">
            <div class="achievement-detail-icon"></div>
            <p class="achievement-detail-description"></p>
            <div class="achievement-detail-meta"></div>
            <div class="achievement-detail-rewards"></div>
            <div class="achievement-detail-prerequisites"></div>
          </div>
        </div>
      `;
      document.body.appendChild(modal);

      modal.querySelector('.close-modal')?.addEventListener('click', () => {
        modal!.style.display = 'none';
      });
    }

    const titleEl = modal.querySelector('.achievement-title');
    if (titleEl) titleEl.textContent = achievement.name;

    const descEl = modal.querySelector('.achievement-detail-description');
    if (descEl) descEl.textContent = achievement.description;

    const metaEl = modal.querySelector('.achievement-detail-meta');
    if (metaEl) {
      metaEl.innerHTML = `
        <span>Type: ${achievement.type}</span>
        <span>Tier: ${achievement.tier}</span>
        <span>Points: ${achievement.points}</span>
        ${achievement.unlockedAt ? `<span>Unlocked: ${achievement.unlockedAt.toLocaleDateString()}</span>` : ''}
      `;
    }

    const rewardsEl = modal.querySelector('.achievement-detail-rewards');
    if (rewardsEl) {
      rewardsEl.innerHTML = '<h4>Rewards:</h4>';
      achievement.rewards.forEach(reward => {
        const rewardItem = document.createElement('div');
        rewardItem.className = 'reward-item';
        rewardItem.innerHTML = `<span>${reward.description}</span>`;
        rewardsEl.appendChild(rewardItem);
      });
    }

    modal.style.display = 'block';
    this.uiElements.detailModal = modal;
  }

  public showNotification(notification: AchievementNotification): void {
    if (!this.uiElements.notificationArea) return;

    const notificationEl = document.createElement('div');
    notificationEl.className = `achievement-notification ${notification.priority}`;
    notificationEl.innerHTML = `
      <div class="notification-icon">
        <img src="${notification.icon}" alt="${notification.achievementName}" />
      </div>
      <div class="notification-content">
        <h4>${notification.achievementName}</h4>
        <p>${notification.achievementDescription}</p>
        <div class="notification-rewards">
          ${notification.rewards.map(r => `<span>${r.description}</span>`).join(', ')}
        </div>
      </div>
      <button class="dismiss-notification">&times;</button>
    `;

    notificationEl.querySelector('.dismiss-notification')?.addEventListener('click', () => {
      notificationEl.remove();
    });

    this.uiElements.notificationArea.appendChild(notificationEl);

    setTimeout(() => {
      notificationEl.classList.add('show');
    }, 100);

    setTimeout(() => {
      notificationEl.classList.remove('show');
      setTimeout(() => notificationEl.remove(), 300);
    }, notification.displayDuration);
  }

  public isInitializedStatus(): boolean {
    return this.isInitialized;
  }

  public destroy(): void {
    const modal = document.getElementById('achievement-detail-modal');
    if (modal) modal.remove();

    const panel = document.getElementById('achievement-panel');
    if (panel) panel.remove();

    this.uiElements = {
      panel: null,
      achievementList: null,
      categoryTabs: null,
      progressBar: null,
      notificationArea: null,
      filterControls: null,
      searchInput: null,
      sortDropdown: null,
      detailModal: null,
      comparisonPanel: null
    };

    this.isInitialized = false;
  }
}

export class AchievementComparison {
  private leaderboards: Map<string, AchievementLeaderboard> = new Map();
  private playerComparisons: Map<string, Map<string, AchievementComparison>> = new Map();
  private sortAlgorithms: Map<string, (a: AchievementComparison, b: AchievementComparison) => number> = new Map();
  private maxLeaderboardEntries: number = 100;

  constructor() {
    this.initializeSortAlgorithms();
  }

  private initializeSortAlgorithms(): void {
    this.sortAlgorithms.set('points', (a, b) => b.totalPoints - a.totalPoints);
    this.sortAlgorithms.set('count', (a, b) => b.achievementCount - a.achievementCount);
    this.sortAlgorithms.set('rank', (a, b) => a.rank - b.rank);
  }

  public createLeaderboard(
    id: string,
    name: string,
    timeframe: AchievementLeaderboard['timeframe'] = 'all_time'
  ): AchievementLeaderboard {
    const leaderboard: AchievementLeaderboard = {
      id,
      name,
      entries: [],
      sortBy: 'points',
      updatedAt: new Date(),
      timeframe
    };

    this.leaderboards.set(id, leaderboard);
    return leaderboard;
  }

  public getLeaderboard(leaderboardId: string): AchievementLeaderboard | undefined {
    return this.leaderboards.get(leaderboardId);
  }

  public getAllLeaderboards(): AchievementLeaderboard[] {
    return Array.from(this.leaderboards.values());
  }

  public addComparison(leaderboardId: string, comparison: AchievementComparison): void {
    const leaderboard = this.leaderboards.get(leaderboardId);
    if (!leaderboard) return;

    const existingIndex = leaderboard.entries.findIndex(e => e.playerId === comparison.playerId);
    if (existingIndex >= 0) {
      leaderboard.entries[existingIndex] = comparison;
    } else {
      leaderboard.entries.push(comparison);
    }

    this.recalculateRanks(leaderboard);
    leaderboard.updatedAt = new Date();
  }

  private recalculateRanks(leaderboard: AchievementLeaderboard): void {
    const sortAlgorithm = this.sortAlgorithms.get(leaderboard.sortBy) || this.sortAlgorithms.get('points')!;
    leaderboard.entries.sort(sortAlgorithm);

    leaderboard.entries.forEach((entry, index) => {
      entry.rank = index + 1;
    });

    if (leaderboard.entries.length > this.maxLeaderboardEntries) {
      leaderboard.entries = leaderboard.entries.slice(0, this.maxLeaderboardEntries);
    }
  }

  public removeComparison(leaderboardId: string, playerId: string): void {
    const leaderboard = this.leaderboards.get(leaderboardId);
    if (!leaderboard) return;

    leaderboard.entries = leaderboard.entries.filter(e => e.playerId !== playerId);
    this.recalculateRanks(leaderboard);
    leaderboard.updatedAt = new Date();
  }

  public getPlayerRank(leaderboardId: string, playerId: string): number | null {
    const leaderboard = this.leaderboards.get(leaderboardId);
    if (!leaderboard) return null;

    const entry = leaderboard.entries.find(e => e.playerId === playerId);
    return entry?.rank || null;
  }

  public getPlayerComparison(leaderboardId: string, playerId: string): AchievementComparison | undefined {
    const leaderboard = this.leaderboards.get(leaderboardId);
    return leaderboard?.entries.find(e => e.playerId === playerId);
  }

  public getTopPlayers(leaderboardId: string, count: number = 10): AchievementComparison[] {
    const leaderboard = this.leaderboards.get(leaderboardId);
    if (!leaderboard) return [];

    return leaderboard.entries.slice(0, count);
  }

  public compareTwoPlayers(
    leaderboardId: string,
    playerId1: string,
    playerId2: string
  ): {
    comparison: {
      player1: AchievementComparison | undefined;
      player2: AchievementComparison | undefined;
    };
    differences: Record<string, { player1: number | string; player2: number | string }>;
    winner: 'player1' | 'player2' | 'tie';
  } {
    const leaderboard = this.leaderboards.get(leaderboardId);
    if (!leaderboard) {
      return {
        comparison: { player1: undefined, player2: undefined },
        differences: {},
        winner: 'tie'
      };
    }

    const player1 = leaderboard.entries.find(e => e.playerId === playerId1);
    const player2 = leaderboard.entries.find(e => e.playerId === playerId2);

    const differences: Record<string, { player1: number | string; player2: number | string }> = {};

    if (player1 && player2) {
      if (player1.achievementCount !== player2.achievementCount) {
        differences.achievementCount = {
          player1: player1.achievementCount,
          player2: player2.achievementCount
        };
      }

      if (player1.totalPoints !== player2.totalPoints) {
        differences.totalPoints = {
          player1: player1.totalPoints,
          player2: player2.totalPoints
        };
      }

      if (player1.rank !== player2.rank) {
        differences.rank = {
          player1: player1.rank,
          player2: player2.rank
        };
      }
    }

    let winner: 'player1' | 'player2' | 'tie' = 'tie';
    const p1Points = player1?.totalPoints || 0;
    const p2Points = player2?.totalPoints || 0;

    if (p1Points > p2Points) {
      winner = 'player1';
    } else if (p2Points > p1Points) {
      winner = 'player2';
    }

    return {
      comparison: { player1, player2 },
      differences,
      winner
    };
  }

  public getPlayerPosition(
    leaderboardId: string,
    playerId: string
  ): { above: AchievementComparison[]; current: AchievementComparison | null; below: AchievementComparison[] } {
    const leaderboard = this.leaderboards.get(leaderboardId);
    if (!leaderboard) {
      return { above: [], current: null, below: [] };
    }

    const currentIndex = leaderboard.entries.findIndex(e => e.playerId === playerId);
    if (currentIndex < 0) {
      return { above: [], current: null, below: [] };
    }

    return {
      above: leaderboard.entries.slice(Math.max(0, currentIndex - 3), currentIndex),
      current: leaderboard.entries[currentIndex],
      below: leaderboard.entries.slice(currentIndex + 1, currentIndex + 4)
    };
  }

  public setLeaderboardSort(leaderboardId: string, sortBy: AchievementLeaderboard['sortBy']): void {
    const leaderboard = this.leaderboards.get(leaderboardId);
    if (!leaderboard) return;

    leaderboard.sortBy = sortBy;
    this.recalculateRanks(leaderboard);
  }

  public generateGlobalLeaderboard(
    playerComparisons: Map<string, AchievementComparison>
  ): AchievementComparison[] {
    const entries = Array.from(playerComparisons.values());
    entries.sort((a, b) => b.totalPoints - a.totalPoints);

    entries.forEach((entry, index) => {
      entry.rank = index + 1;
    });

    return entries;
  }

  public exportData(): unknown {
    return {
      leaderboards: Object.fromEntries(this.leaderboards),
      playerComparisons: Object.fromEntries(
        Array.from(this.playerComparisons.entries()).map(([playerId, comparisons]) => [
          playerId,
          Object.fromEntries(comparisons)
        ])
      )
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        leaderboards: Record<string, AchievementLeaderboard>;
        playerComparisons: Record<string, Record<string, AchievementComparison>>;
      };

      this.leaderboards = new Map(Object.entries(typedData.leaderboards || {}));
      this.playerComparisons = new Map(
        Object.entries(typedData.playerComparisons || {}).map(([playerId, comparisons]) => [
          playerId,
          new Map(Object.entries(comparisons))
        ])
      );

      return true;
    } catch (error) {
      console.error('Failed to import comparison data:', error);
      return false;
    }
  }
}

export class ProgressionManager extends EventEmitter {
  private achievements: Map<string, Achievement> = new Map();
  private playerProgress: Map<string, Map<string, AchievementProgress>> = new Map();
  private playerStats: Map<string, AchievementStats> = new Map();

  private tracker: AchievementTracker;
  private unlocks: AchievementUnlocks;
  private rewards: AchievementRewards;
  private notifications: AchievementNotifications;
  private milestones: ProgressionMilestones;
  private persistence: ProgressionPersistence;
  private categories: AchievementCategories;
  private stats: AchievementStats;
  private seasonal: SeasonalAchievements;
  private ui: AchievementUIIntegration;
  private comparison: AchievementComparison;

  private currentPlayerId: string | null = null;
  private initialized: boolean = false;
  private eventListeners: Map<string, Set<Function>> = new Map();

  constructor(
    resourceManager?: ResourceManager,
    unitManager?: UnitManager,
    buildingManager?: BuildingManager,
    techTreeManager?: TechTreeManager,
    combatManager?: CombatManager,
    economyManager?: EconomyManager,
    saveManager?: SaveManager,
    uiManager?: UIManager
  ) {
    super();

    this.tracker = new AchievementTracker();
    this.unlocks = new AchievementUnlocks();
    this.rewards = new AchievementRewards();
    this.notifications = new AchievementNotifications();
    this.milestones = new ProgressionMilestones();
    this.persistence = new ProgressionPersistence(saveManager);
    this.categories = new AchievementCategories();
    this.stats = new AchievementStats();
    this.seasonal = new SeasonalAchievements();
    this.ui = new AchievementUIIntegration();
    this.comparison = new AchievementComparison();

    this.initializeDefaultAchievements();
    this.setupEventListeners();
  }

  private initializeDefaultAchievements(): void {
    const defaultAchievements: AchievementDefinition[] = [
      {
        id: 'first_blood',
        name: 'First Blood',
        description: 'Win your first battle',
        type: AchievementType.Combat,
        tier: ProgressionTier.Bronze,
        icon: '/icons/achievements/first_blood.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.KillCount,
              target: 1,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 100, description: '100 XP', isUnique: false }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'combat_mastery',
        difficulty: 1,
        points: 10,
        prerequisites: [],
        relatedAchievements: ['rampage', 'unstoppable'],
        estimatedDifficulty: 'easy',
        tags: ['combat', 'beginner', 'first']
      },
      {
        id: 'rampage',
        name: 'Rampage',
        description: 'Defeat 50 enemy units in a single battle',
        type: AchievementType.Combat,
        tier: ProgressionTier.Silver,
        icon: '/icons/achievements/rampage.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.KillCount,
              target: 50,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 500, description: '500 XP', isUnique: false },
          { type: 'title', value: 'Rage Knight', description: 'Rage Knight Title', isUnique: true }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'combat_mastery',
        difficulty: 3,
        points: 50,
        prerequisites: ['first_blood'],
        relatedAchievements: ['first_blood', 'unstoppable'],
        estimatedDifficulty: 'medium',
        tags: ['combat', 'kills', 'battle']
      },
      {
        id: 'unstoppable',
        name: 'Unstoppable',
        description: 'Defeat 500 enemy units in total',
        type: AchievementType.Combat,
        tier: ProgressionTier.Gold,
        icon: '/icons/achievements/unstoppable.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.KillCount,
              target: 500,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 2000, description: '2000 XP', isUnique: false },
          { type: 'cosmetic', value: 'unstoppable_aura', description: 'Unstoppable Aura Effect', isUnique: true }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'combat_mastery',
        difficulty: 5,
        points: 200,
        prerequisites: ['rampage'],
        relatedAchievements: ['rampage', 'first_blood'],
        estimatedDifficulty: 'hard',
        tags: ['combat', 'kills', 'mastery']
      },
      {
        id: 'wealthy_commander',
        name: 'Wealthy Commander',
        description: 'Accumulate 10000 resources',
        type: AchievementType.Economy,
        tier: ProgressionTier.Bronze,
        icon: '/icons/achievements/wealthy.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.ResourceGathered,
              target: 10000,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 150, description: '150 XP', isUnique: false }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'economic_prowess',
        difficulty: 2,
        points: 15,
        prerequisites: [],
        relatedAchievements: ['economic_giant', 'master_trader'],
        estimatedDifficulty: 'easy',
        tags: ['economy', 'resources', 'wealth']
      },
      {
        id: 'master_builder',
        name: 'Master Builder',
        description: 'Construct 100 buildings',
        type: AchievementType.Construction,
        tier: ProgressionTier.Silver,
        icon: '/icons/achievements/master_builder.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.BuildCount,
              target: 100,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 600, description: '600 XP', isUnique: false },
          { type: 'cosmetic', value: 'builder_hammer', description: 'Golden Builder Hammer', isUnique: true }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'construction_wonders',
        difficulty: 3,
        points: 60,
        prerequisites: [],
        relatedAchievements: ['city_planner', 'wonder_builder'],
        estimatedDifficulty: 'medium',
        tags: ['construction', 'building', 'builder']
      },
      {
        id: 'explorer',
        name: 'Explorer',
        description: 'Reveal 75% of the map',
        type: AchievementType.Exploration,
        tier: ProgressionTier.Bronze,
        icon: '/icons/achievements/explorer.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.MapExplored,
              target: 75,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 200, description: '200 XP', isUnique: false }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'exploration_discoveries',
        difficulty: 2,
        points: 20,
        prerequisites: [],
        relatedAchievements: ['world_traveler', 'cartographer'],
        estimatedDifficulty: 'easy',
        tags: ['exploration', 'map', 'discovery']
      },
      {
        id: 'tech_master',
        name: 'Technology Master',
        description: 'Research 25 technologies',
        type: AchievementType.Technology,
        tier: ProgressionTier.Silver,
        icon: '/icons/achievements/tech_master.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.TechResearched,
              target: 25,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 700, description: '700 XP', isUnique: false },
          { type: 'unlock', value: 'advanced_tech_tree', description: 'Advanced Tech Tree Access', isUnique: true }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'technological_advancement',
        difficulty: 3,
        points: 70,
        prerequisites: [],
        relatedAchievements: ['innovator', 'scientist'],
        estimatedDifficulty: 'medium',
        tags: ['technology', 'research', 'innovation']
      },
      {
        id: 'champion',
        name: 'Champion',
        description: 'Win 50 battles',
        type: AchievementType.Combat,
        tier: ProgressionTier.Gold,
        icon: '/icons/achievements/champion.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.TotalWins,
              target: 50,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 3000, description: '3000 XP', isUnique: false },
          { type: 'title', value: 'Champion', description: 'Champion Title', isUnique: true }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'combat_mastery',
        difficulty: 6,
        points: 300,
        prerequisites: ['rampage'],
        relatedAchievements: ['legend', 'immortal'],
        estimatedDifficulty: 'hard',
        tags: ['combat', 'victory', 'champion']
      },
      {
        id: 'legend',
        name: 'Legend',
        description: 'Win 500 battles',
        type: AchievementType.Combat,
        tier: ProgressionTier.Platinum,
        icon: '/icons/achievements/legend.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.TotalWins,
              target: 500,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 10000, description: '10000 XP', isUnique: false },
          { type: 'cosmetic', value: 'legend_crown', description: 'Legend Crown', isUnique: true }
        ],
        isSecret: false,
        isSeasonal: false,
        categoryId: 'combat_mastery',
        difficulty: 8,
        points: 800,
        prerequisites: ['champion'],
        relatedAchievements: ['champion', 'immortal'],
        estimatedDifficulty: 'extreme',
        tags: ['combat', 'victory', 'legend']
      },
      {
        id: 'immortal',
        name: 'Immortal',
        description: 'Win 5000 battles',
        type: AchievementType.Combat,
        tier: ProgressionTier.Diamond,
        icon: '/icons/achievements/immortal.png',
        criteria: {
          conditions: [
            {
              type: UnlockConditionType.TotalWins,
              target: 5000,
              current: 0,
              comparison: 'gte'
            }
          ],
          logic: 'all'
        },
        rewards: [
          { type: 'experience', value: 50000, description: '50000 XP', isUnique: false },
          { type: 'cosmetic', value: 'immortal_wings', description: 'Immortal Wings', isUnique: true }
        ],
        isSecret: true,
        isSeasonal: false,
        categoryId: 'combat_mastery',
        difficulty: 10,
        points: 5000,
        prerequisites: ['legend'],
        relatedAchievements: ['legend', 'champion'],
        estimatedDifficulty: 'extreme',
        tags: ['combat', 'victory', 'immortal', 'secret']
      }
    ];

    defaultAchievements.forEach(definition => {
      const achievement: Achievement = {
        ...definition,
        progress: 0
      };
      this.achievements.set(achievement.id, achievement);
    });
  }

  private setupEventListeners(): void {
    this.tracker.on('progressUpdated', (update: ProgressUpdate) => {
      this.handleProgressUpdate(update);
    });

    this.tracker.setPersistenceCallback(async (data) => {
      if (this.currentPlayerId) {
        await this.persistence.save(this.currentPlayerId, data);
      }
    });
  }

  private handleProgressUpdate(update: ProgressUpdate): void {
    if (!this.currentPlayerId) return;

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    if (!playerProgress) return;

    this.achievements.forEach((achievement, achievementId) => {
      const progress = playerProgress.get(achievementId);
      if (progress?.isUnlocked) return;

      const conditionMet = achievement.criteria.conditions.some(c => c.type === update.conditionType);
      if (!conditionMet) return;

      this.checkAchievementUnlock(achievementId);
    });
  }

  private checkAchievementUnlock(achievementId: string): boolean {
    if (!this.currentPlayerId) return false;

    const achievement = this.achievements.get(achievementId);
    if (!achievement) return false;

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    if (!playerProgress) return false;

    const progress = playerProgress.get(achievementId);
    const allProgress = this.tracker.getAllProgress(this.currentPlayerId);

    const isUnlocked = this.unlocks.checkUnlockConditions(achievement, allProgress);

    if (isUnlocked && !(progress?.isUnlocked)) {
      this.unlockAchievement(achievementId);
      return true;
    }

    return false;
  }

  public async unlockAchievement(achievementId: string): Promise<boolean> {
    if (!this.currentPlayerId) return false;

    const achievement = this.achievements.get(achievementId);
    if (!achievement) return false;

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    if (!playerProgress) return false;

    const progress = playerProgress.get(achievementId);
    if (progress?.isUnlocked) return false;

    achievement.unlockedAt = new Date();
    achievement.progress = 100;

    if (!playerProgress.has(achievementId)) {
      playerProgress.set(achievementId, {
        achievementId,
        conditions: new Map(),
        isUnlocked: true,
        unlockedAt: new Date(),
        completionPercentage: 100
      });
    } else {
      const existingProgress = playerProgress.get(achievementId)!;
      existingProgress.isUnlocked = true;
      existingProgress.unlockedAt = new Date();
      existingProgress.completionPercentage = 100;
    }

    const notification: AchievementNotification = {
      id: `notif_${Date.now()}`,
      achievementId,
      achievementName: achievement.name,
      achievementDescription: achievement.description,
      tier: achievement.tier,
      icon: achievement.icon,
      rewards: achievement.rewards,
      timestamp: new Date(),
      isRead: false,
      isDismissed: false,
      displayDuration: 5000,
      priority: achievement.tier === ProgressionTier.Diamond ? 'critical' : 'high'
    };

    this.notifications.showNotification(notification);
    this.ui.showNotification(notification);

    await this.rewards.processRewards(achievement.rewards, this.currentPlayerId);

    this.unlocks.queueUnlock(achievementId, this.currentPlayerId);

    const stats = this.calculatePlayerStats();
    this.playerStats.set(this.currentPlayerId, stats);
    this.milestones.checkMilestoneProgress(this.currentPlayerId, stats);

    this.emit('achievementUnlocked', {
      playerId: this.currentPlayerId,
      achievement,
      notification,
      timestamp: new Date()
    });

    return true;
  }

  public async initializePlayer(playerId: string): Promise<void> {
    this.currentPlayerId = playerId;
    this.tracker.startTracking(playerId);

    if (!this.playerProgress.has(playerId)) {
      this.playerProgress.set(playerId, new Map());
    }

    const savedData = await this.persistence.load(playerId);
    if (savedData) {
      this.restoreProgress(playerId, savedData);
    }

    this.initialized = true;
    this.emit('playerInitialized', { playerId, timestamp: new Date() });
  }

  private restoreProgress(playerId: string, data: unknown): void {
    const typedData = data as {
      achievements?: Record<string, AchievementProgress>;
      tracker?: unknown;
      unlocks?: unknown;
      rewards?: unknown;
    };

    if (typedData.achievements) {
      Object.entries(typedData.achievements).forEach(([id, progress]) => {
        const playerProgress = this.playerProgress.get(playerId);
        if (playerProgress) {
          playerProgress.set(id, progress);
        }
      });
    }

    if (typedData.tracker) {
      this.tracker.importData(typedData.tracker);
    }

    if (typedData.unlocks) {
      this.unlocks.importData(typedData.unlocks);
    }

    if (typedData.rewards) {
      this.rewards.importData(typedData.rewards);
    }
  }

  public updateProgress(conditionType: UnlockConditionType, value: number): ProgressUpdate | null {
    if (!this.currentPlayerId) return null;
    return this.tracker.updateProgress(this.currentPlayerId, conditionType, value);
  }

  public setProgress(conditionType: UnlockConditionType, value: number): ProgressUpdate | null {
    if (!this.currentPlayerId) return null;
    return this.tracker.setProgress(this.currentPlayerId, conditionType, value);
  }

  public getAchievement(achievementId: string): Achievement | undefined {
    return this.achievements.get(achievementId);
  }

  public getAllAchievements(): Achievement[] {
    return Array.from(this.achievements.values());
  }

  public getUnlockedAchievements(): Achievement[] {
    if (!this.currentPlayerId) return [];

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    if (!playerProgress) return [];

    return Array.from(this.achievements.values()).filter(a => {
      const progress = playerProgress.get(a.id);
      return progress?.isUnlocked;
    });
  }

  public getLockedAchievements(): Achievement[] {
    if (!this.currentPlayerId) return [];

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    if (!playerProgress) return [];

    return Array.from(this.achievements.values()).filter(a => {
      const progress = playerProgress.get(a.id);
      return !progress?.isUnlocked;
    });
  }

  public getAchievementProgress(achievementId: string): AchievementProgress | null {
    if (!this.currentPlayerId) return null;

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    return playerProgress?.get(achievementId) || null;
  }

  public calculatePlayerStats(): AchievementStats {
    if (!this.currentPlayerId) {
      return this.stats.calculateStats([], new Set());
    }

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    const unlockedIds = new Set<string>();

    playerProgress?.forEach((progress, id) => {
      if (progress.isUnlocked) {
        unlockedIds.add(id);
      }
    });

    const stats = this.stats.calculateStats(Array.from(this.achievements.values()), unlockedIds);
    this.playerStats.set(this.currentPlayerId, stats);

    return stats;
  }

  public getPlayerStats(): AchievementStats | null {
    return this.playerStats.get(this.currentPlayerId || '') || null;
  }

  public getCategories(): AchievementCategory[] {
    return this.categories.getAllCategories();
  }

  public getMilestones(): ProgressionMilestone[] {
    if (!this.currentPlayerId) return [];

    return this.milestones.getUnlockedMilestones(this.currentPlayerId);
  }

  public getNextMilestone(): ProgressionMilestone | null {
    if (!this.currentPlayerId) return null;

    const stats = this.calculatePlayerStats();
    return this.milestones.checkMilestoneProgress(this.currentPlayerId, stats);
  }

  public getSeasonalEvents(): SeasonalEvent[] {
    return this.seasonal.getAllSeasonalEvents();
  }

  public getCurrentSeasonalEvent(): SeasonalEvent | null {
    return this.seasonal.getCurrentEvent();
  }

  public getLeaderboard(leaderboardId: string): AchievementLeaderboard | undefined {
    return this.comparison.getLeaderboard(leaderboardId);
  }

  public getTopPlayers(count: number = 10): AchievementComparison[] {
    const leaderboard = this.comparison.getLeaderboard('global');
    if (!leaderboard) return [];

    return this.comparison.getTopPlayers('global', count);
  }

  public compareWithPlayer(playerId: string): {
    comparison: {
      player1: AchievementComparison | undefined;
      player2: AchievementComparison | undefined;
    };
    differences: Record<string, { player1: number | string; player2: number | string }>;
    winner: 'player1' | 'player2' | 'tie';
  } | null {
    if (!this.currentPlayerId) return null;

    return this.comparison.compareTwoPlayers('global', this.currentPlayerId, playerId);
  }

  public async saveProgress(): Promise<boolean> {
    if (!this.currentPlayerId) return false;

    const data = {
      achievements: Object.fromEntries(this.playerProgress.get(this.currentPlayerId) || new Map()),
      tracker: this.tracker.exportData(),
      unlocks: this.unlocks.exportData(),
      rewards: this.rewards.exportData(),
      stats: this.playerStats.get(this.currentPlayerId)
    };

    return await this.persistence.save(this.currentPlayerId, data);
  }

  public showUIPanel(): void {
    this.ui.showPanel();
    this.refreshUI();
  }

  public hideUIPanel(): void {
    this.ui.hidePanel();
  }

  public toggleUIPanel(): void {
    this.ui.togglePanel();
    if (this.ui.isInitializedStatus()) {
      this.refreshUI();
    }
  }

  public refreshUI(): void {
    if (!this.currentPlayerId) return;

    const playerProgress = this.playerProgress.get(this.currentPlayerId);
    const unlockedIds = new Set<string>();

    playerProgress?.forEach((progress, id) => {
      if (progress.isUnlocked) {
        unlockedIds.add(id);
      }
    });

    this.achievements.forEach(achievement => {
      const progress = playerProgress?.get(achievement.id);
      achievement.progress = progress?.completionPercentage || 0;
      achievement.unlockedAt = progress?.unlockedAt;
    });

    this.ui.renderAchievements(
      Array.from(this.achievements.values()),
      unlockedIds,
      this.categories.getAllCategories()
    );
  }

  public exportData(): unknown {
    return {
      achievements: Array.from(this.achievements.entries()),
      playerProgress: Object.fromEntries(
        Array.from(this.playerProgress.entries()).map(([playerId, progress]) => [
          playerId,
          Object.fromEntries(progress)
        ])
      ),
      playerStats: Object.fromEntries(this.playerStats)
    };
  }

  public importData(data: unknown): boolean {
    try {
      const typedData = data as {
        achievements: [string, Achievement][];
        playerProgress: Record<string, Record<string, AchievementProgress>>;
        playerStats: Record<string, AchievementStats>;
      };

      this.achievements = new Map(typedData.achievements || []);
      this.playerProgress = new Map(
        Object.entries(typedData.playerProgress || {}).map(([playerId, progress]) => [
          playerId,
          new Map(Object.entries(progress))
        ])
      );
      this.playerStats = new Map(Object.entries(typedData.playerStats || {}));

      return true;
    } catch (error) {
      console.error('Failed to import progression data:', error);
      return false;
    }
  }
}

import { EventEmitter } from '../../../core/events/EventEmitter';
import { GameState } from '../../state/GameState';
import { Unit, UnitStats } from '../../entities/Unit';
import { ResourceManager, ResourceType } from '../../resources/ResourceManager';
import { TechTree } from '../../tech/TechTree';
import { Logger } from '../../../core/logger/Logger';

export enum UpgradeCategory {
  Weapon = 'weapon',
  Armor = 'armor',
  Ability = 'ability',
  Movement = 'movement',
  Special = 'special'
}

export enum UpgradeTier {
  Tier1 = 1,
  Tier2 = 2,
  Tier3 = 3,
  Tier4 = 4,
  Tier5 = 5
}

export enum UpgradeSource {
  Default = 'default',
  Campaign = 'campaign',
  Research = 'research',
  Achievement = 'achievement',
  Unlock = 'unlock',
  Bonus = 'bonus',
  Special = 'special'
}

export enum UnlockConditionType {
  ResourceRequired = 'resource_required',
  BuildingRequired = 'building_required',
  TechRequired = 'tech_required',
  UpgradeRequired = 'upgrade_required',
  LevelRequired = 'level_required',
  MissionRequired = 'mission_required',
  TimeRequired = 'time_required',
  CustomCondition = 'custom_condition'
}

export enum AutomationTrigger {
  OnUpgradeComplete = 'on_upgrade_complete',
  OnResourceThreshold = 'on_resource_threshold',
  OnUnitSpawn = 'on_unit_spawn',
  OnBuildingComplete = 'on_building_complete',
  OnResearchComplete = 'on_research_complete',
  Scheduled = 'scheduled',
  Manual = 'manual'
}

export interface UpgradeEffect {
  statName: string;
  modifier: number;
  operation: 'add' | 'multiply' | 'set' | 'percentage';
  duration?: number;
  stackable?: boolean;
  maxStacks?: number;
  condition?: UpgradeCondition;
}

export interface UpgradeCondition {
  unitType?: string;
  buildingType?: string;
  resourceType?: ResourceType;
  minResourceAmount?: number;
  upgradeId?: string;
  level?: number;
  customCheck?: (context: UpgradeContext) => boolean;
}

export interface UpgradeContext {
  unit?: Unit;
  playerId: string;
  gameState: GameState;
  timestamp: number;
}

export interface UpgradeType {
  id: string;
  name: string;
  description: string;
  category: UpgradeCategory;
  tier: UpgradeTier;
  icon?: string;
  effects: UpgradeEffect[];
  prerequisites: string[];
  cost: UpgradeCost;
  unlockCondition?: UnlockCondition;
  maxLevel: number;
  affectedUnits?: string[];
  affectedBuildings?: string[];
  source: UpgradeSource;
  exclusive?: boolean;
  exclusiveWith?: string[];
  hidden?: boolean;
  deprecated?: boolean;
}

export interface UpgradeLevel {
  level: number;
  effects: UpgradeEffect[];
  cost: UpgradeCost;
  timeRequired: number;
  experienceRequired?: number;
  unlockCondition?: UnlockCondition;
}

export interface UpgradeStats {
  totalUpgrades: number;
  upgradesByCategory: Map<UpgradeCategory, number>;
  upgradesByTier: Map<UpgradeTier, number>;
  totalSpent: Map<ResourceType, number>;
  totalTime: number;
  averageLevel: number;
  completionPercentage: number;
}

export interface UpgradeCost {
  resources: Map<ResourceType, number>;
  time: number;
  experience?: number;
  population?: number;
  prerequisiteUpgrades?: string[];
}

export interface UpgradeProgress {
  upgradeId: string;
  currentLevel: number;
  progressPercentage: number;
  startTime?: number;
  completionTime?: number;
  automationEnabled: boolean;
  autoUpgradePriority: number;
}

export interface UpgradeTreeNode {
  upgradeId: string;
  parents: string[];
  children: string[];
  position: { x: number; y: number };
  isUnlocked: boolean;
  isCompleted: boolean;
  isAvailable: boolean;
}

export interface UpgradeSynergy {
  id: string;
  name: string;
  description: string;
  requiredUpgrades: string[];
  bonusEffects: UpgradeEffect[];
  isActive: boolean;
  stackable: boolean;
  maxStacks?: number;
  currentStacks: number;
}

export interface UpgradeHistoryEntry {
  timestamp: number;
  upgradeId: string;
  level: number;
  previousLevel: number;
  cost: Map<ResourceType, number>;
  duration: number;
  source: UpgradeSource;
  autoTriggered: boolean;
}

export interface UpgradeUnlock {
  conditionType: UnlockConditionType;
  targetId?: string;
  requiredValue?: number;
  comparisonType?: 'equals' | 'greater' | 'less' | 'greaterEqual' | 'lessEqual';
  secondaryTarget?: string;
  secondaryValue?: number;
  customData?: Record<string, unknown>;
}

export class UpgradeCostCalculator {
  private costMultipliers: Map<UpgradeTier, number> = new Map([
    [UpgradeTier.Tier1, 1.0],
    [UpgradeTier.Tier2, 2.5],
    [UpgradeTier.Tier3, 5.0],
    [UpgradeTier.Tier4, 10.0],
    [UpgradeTier.Tier5, 25.0]
  ]);

  private scalingFactors: Map<UpgradeCategory, number> = new Map([
    [UpgradeCategory.Weapon, 1.2],
    [UpgradeCategory.Armor, 1.1],
    [UpgradeCategory.Ability, 1.5],
    [UpgradeCategory.Movement, 1.15],
    [UpgradeCategory.Special, 1.8]
  ]);

  calculateCost(
    baseCost: UpgradeCost,
    level: number,
    category: UpgradeCategory,
    tier: UpgradeTier
  ): UpgradeCost {
    const levelMultiplier = Math.pow(1.5, level - 1);
    const tierMultiplier = this.costMultipliers.get(tier) || 1.0;
    const categoryMultiplier = this.scalingFactors.get(category) || 1.0;
    const totalMultiplier = levelMultiplier * tierMultiplier * categoryMultiplier;

    const calculatedResources = new Map<ResourceType, number>();
    baseCost.resources.forEach((amount, resourceType) => {
      calculatedResources.set(resourceType, Math.floor(amount * totalMultiplier));
    });

    return {
      resources: calculatedResources,
      time: Math.floor(baseCost.time * totalMultiplier),
      experience: baseCost.experience
        ? Math.floor(baseCost.experience * totalMultiplier)
        : undefined,
      population: baseCost.population
        ? Math.floor(baseCost.population * totalMultiplier)
        : undefined,
      prerequisiteUpgrades: baseCost.prerequisiteUpgrades
    };
  }

  calculateDiscount(
    baseCost: UpgradeCost,
    playerUpgrades: string[],
    researchBonus: number = 0
  ): UpgradeCost {
    const discountMultiplier = 1 - (researchBonus * 0.1);
    const loyaltyDiscount = playerUpgrades.includes('loyalty_program') ? 0.9 : 1.0;
    const finalMultiplier = discountMultiplier * loyaltyDiscount;

    const discountedResources = new Map<ResourceType, number>();
    baseCost.resources.forEach((amount, resourceType) => {
      discountedResources.set(resourceType, Math.floor(amount * finalMultiplier));
    });

    return {
      resources: discountedResources,
      time: Math.floor(baseCost.time * finalMultiplier),
      experience: baseCost.experience
        ? Math.floor(baseCost.experience * finalMultiplier)
        : undefined,
      population: baseCost.population,
      prerequisiteUpgrades: baseCost.prerequisiteUpgrades
    };
  }

  calculateRefundPercentage(upgradeAge: number): number {
    const maxRefundAge = 30 * 24 * 60 * 60 * 1000;
    const ageRatio = Math.min(upgradeAge / maxRefundAge, 1);
    return 0.8 - (ageRatio * 0.5);
  }

  getCostBreakdown(cost: UpgradeCost): Map<ResourceType, { amount: number; percentage: number }> {
    const breakdown = new Map<ResourceType, { amount: number; percentage: number }>();
    let totalCost = 0;
    
    cost.resources.forEach((amount) => {
      totalCost += amount;
    });

    cost.resources.forEach((amount, resourceType) => {
      breakdown.set(resourceType, {
        amount,
        percentage: totalCost > 0 ? (amount / totalCost) * 100 : 0
      });
    });

    return breakdown;
  }
}

export class UpgradeProgressTracker {
  private progress: Map<string, UpgradeProgress> = new Map();
  private listeners: Array<(progress: UpgradeProgress) => void> = [];

  updateProgress(upgradeId: string, updates: Partial<UpgradeProgress>): void {
    const current = this.progress.get(upgradeId) || this.createInitialProgress(upgradeId);
    const updated = { ...current, ...updates };
    this.progress.set(upgradeId, updated);
    this.notifyListeners(updated);
  }

  getProgress(upgradeId: string): UpgradeProgress | undefined {
    return this.progress.get(upgradeId);
  }

  getAllProgress(): Map<string, UpgradeProgress> {
    return new Map(this.progress);
  }

  isInProgress(upgradeId: string): boolean {
    const progress = this.progress.get(upgradeId);
    return progress !== undefined && progress.currentLevel > 0 && progress.progressPercentage < 100;
  }

  completeUpgrade(upgradeId: string, level: number): void {
    this.updateProgress(upgradeId, {
      progressPercentage: 100,
      completionTime: Date.now(),
      currentLevel: level
    });
  }

  resetProgress(upgradeId: string): void {
    this.progress.delete(upgradeId);
  }

  getTotalProgress(): { completed: number; inProgress: number; available: number } {
    let completed = 0;
    let inProgress = 0;
    let available = 0;

    this.progress.forEach((p) => {
      if (p.progressPercentage === 100) {
        completed++;
      } else if (p.progressPercentage > 0) {
        inProgress++;
      } else {
        available++;
      }
    });

    return { completed, inProgress, available };
  }

  addListener(listener: (progress: UpgradeProgress) => void): void {
    this.listeners.push(listener);
  }

  removeListener(listener: (progress: UpgradeProgress) => void): void {
    const index = this.listeners.indexOf(listener);
    if (index > -1) {
      this.listeners.splice(index, 1);
    }
  }

  private createInitialProgress(upgradeId: string): UpgradeProgress {
    return {
      upgradeId,
      currentLevel: 0,
      progressPercentage: 0,
      automationEnabled: false,
      autoUpgradePriority: 0
    };
  }

  private notifyListeners(progress: UpgradeProgress): void {
    this.listeners.forEach((listener) => listener(progress));
  }
}

export class UpgradeTreeManager {
  private nodes: Map<string, UpgradeTreeNode> = new Map();
  private adjacencyList: Map<string, Set<string>> = new Map();

  addNode(upgrade: UpgradeType): void {
    const node: UpgradeTreeNode = {
      upgradeId: upgrade.id,
      parents: [...upgrade.prerequisites],
      children: [],
      position: this.calculateNodePosition(upgrade),
      isUnlocked: upgrade.prerequisites.length === 0,
      isCompleted: false,
      isAvailable: true
    };

    this.nodes.set(upgrade.id, node);

    upgrade.prerequisites.forEach((parentId) => {
      if (!this.adjacencyList.has(parentId)) {
        this.adjacencyList.set(parentId, new Set());
      }
      this.adjacencyList.get(parentId)!.add(upgrade.id);
    });
  }

  removeNode(upgradeId: string): void {
    const node = this.nodes.get(upgradeId);
    if (!node) return;

    node.parents.forEach((parentId) => {
      const children = this.adjacencyList.get(parentId);
      if (children) {
        children.delete(upgradeId);
      }
    });

    this.adjacencyList.forEach((children) => {
      children.delete(upgradeId);
    });

    this.nodes.delete(upgradeId);
  }

  updateNodeStatus(upgradeId: string, completed: boolean, unlocked: boolean): void {
    const node = this.nodes.get(upgradeId);
    if (!node) return;

    node.isCompleted = completed;
    node.isUnlocked = unlocked;

    if (completed) {
      this.unlockChildren(upgradeId);
    }
  }

  getNode(upgradeId: string): UpgradeTreeNode | undefined {
    return this.nodes.get(upgradeId);
  }

  getAllNodes(): Map<string, UpgradeTreeNode> {
    return new Map(this.nodes);
  }

  getAvailableUpgrades(): string[] {
    const available: string[] = [];
    this.nodes.forEach((node, id) => {
      if (this.isAvailable(id)) {
        available.push(id);
      }
    });
    return available;
  }

  isAvailable(upgradeId: string): boolean {
    const node = this.nodes.get(upgradeId);
    if (!node) return false;

    if (!node.isUnlocked) return false;
    if (node.isCompleted) return false;

    return node.parents.every((parentId) => {
      const parent = this.nodes.get(parentId);
      return parent && parent.isCompleted;
    });
  }

  getPath(upgradeId: string): string[] {
    const path: string[] = [upgradeId];
    const visited = new Set<string>();

    while (path.length > 0) {
      const current = path[path.length - 1];
      const node = this.nodes.get(current);

      if (!node || visited.has(current)) {
        path.pop();
        continue;
      }

      visited.add(current);

      const unvisitedParents = node.parents.filter((p) => !visited.has(p));
      if (unvisitedParents.length > 0) {
        path.push(unvisitedParents[0]);
      } else if (path.length > 1) {
        path.pop();
      } else {
        break;
      }
    }

    return path.reverse();
  }

  getCriticalPath(targetUpgradeId: string): string[] {
    const path: string[] = [];
    const visited = new Set<string>();

    const collectPath = (upgradeId: string): boolean => {
      if (visited.has(upgradeId)) return false;
      visited.add(upgradeId);

      const node = this.nodes.get(upgradeId);
      if (!node) return false;

      if (node.parents.length === 0) {
        path.push(upgradeId);
        return true;
      }

      for (const parentId of node.parents) {
        if (collectPath(parentId)) {
          path.push(upgradeId);
          return true;
        }
      }

      return false;
    };

    collectPath(targetUpgradeId);
    return path.reverse();
  }

  validateTree(): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    this.nodes.forEach((node, id) => {
      node.parents.forEach((parentId) => {
        if (!this.nodes.has(parentId)) {
          errors.push(`Upgrade ${id} has non-existent prerequisite ${parentId}`);
        }
      });
    });

    const hasCycle = this.detectCycles();
    if (hasCycle.length > 0) {
      errors.push(`Circular dependency detected: ${hasCycle.join(' -> ')}`);
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  private unlockChildren(upgradeId: string): void {
    const children = this.adjacencyList.get(upgradeId);
    if (!children) return;

    children.forEach((childId) => {
      const child = this.nodes.get(childId);
      if (child) {
        const allParentsCompleted = child.parents.every((parentId) => {
          const parent = this.nodes.get(parentId);
          return parent && parent.isCompleted;
        });

        if (allParentsCompleted) {
          child.isUnlocked = true;
        }
      }
    });
  }

  private calculateNodePosition(upgrade: UpgradeType): { x: number; y: number } {
    const tierOffset = (upgrade.tier - 1) * 200;
    const categoryOffset = Object.values(UpgradeCategory).indexOf(upgrade.category) * 100;

    return {
      x: 100 + tierOffset,
      y: 100 + categoryOffset
    };
  }

  private detectCycles(): string[] {
    const visited = new Set<string>();
    const recursionStack = new Set<string>();
    const cyclePath: string[] = [];

    const dfs = (nodeId: string): boolean => {
      visited.add(nodeId);
      recursionStack.add(nodeId);
      cyclePath.push(nodeId);

      const node = this.nodes.get(nodeId);
      if (!node) return false;

      for (const parentId of node.parents) {
        if (!visited.has(parentId)) {
          if (dfs(parentId)) return true;
        } else if (recursionStack.has(parentId)) {
          cyclePath.push(parentId);
          return true;
        }
      }

      recursionStack.delete(nodeId);
      cyclePath.pop();
      return false;
    };

    for (const nodeId of this.nodes.keys()) {
      if (!visited.has(nodeId)) {
        if (dfs(nodeId)) {
          const cycleStart = cyclePath[cyclePath.length - 1];
          const cycleIndex = cyclePath.indexOf(cycleStart);
          return cyclePath.slice(cycleIndex);
        }
      }
    }

    return [];
  }
}

export class UpgradeSynergyManager {
  private synergies: Map<string, UpgradeSynergy> = new Map();
  private activeSynergies: Set<string> = new Set();
  private synergyCache: Map<string, UpgradeEffect[]> = new Map();

  registerSynergy(synergy: UpgradeSynergy): void {
    this.synergies.set(synergy.id, { ...synergy, currentStacks: 0 });
    this.synergyCache.delete(synergy.id);
  }

  unregisterSynergy(synergyId: string): void {
    this.synergies.delete(synergyId);
    this.activeSynergies.delete(synergyId);
    this.synergyCache.delete(synergyId);
  }

  updateSynergyStatus(upgradeId: string, isActive: boolean): void {
    this.synergies.forEach((synergy) => {
      if (synergy.requiredUpgrades.includes(upgradeId)) {
        this.recalculateSynergy(synergy.id);
      }
    });
  }

  recalculateSynergy(synergyId: string): boolean {
    const synergy = this.synergies.get(synergyId);
    if (!synergy) return false;

    const completedUpgrades = new Set<string>();
    synergy.requiredUpgrades.forEach((upgradeId) => {
      completedUpgrades.add(upgradeId);
    });

    const allRequired = synergy.requiredUpgrades.every((id) => completedUpgrades.has(id));

    if (allRequired && !synergy.isActive) {
      synergy.isActive = true;
      this.activeSynergies.add(synergyId);
      if (synergy.stackable) {
        synergy.currentStacks = Math.min(
          synergy.currentStacks + 1,
          synergy.maxStacks || Infinity
        );
      }
      return true;
    } else if (!allRequired && synergy.isActive) {
      synergy.isActive = false;
      this.activeSynergies.delete(synergyId);
      if (synergy.stackable) {
        synergy.currentStacks = Math.max(synergy.currentStacks - 1, 0);
      }
      return true;
    }

    return false;
  }

  getActiveSynergies(): UpgradeSynergy[] {
    const active: UpgradeSynergy[] = [];
    this.activeSynergies.forEach((id) => {
      const synergy = this.synergies.get(id);
      if (synergy) {
        active.push(synergy);
      }
    });
    return active;
  }

  getSynergyBonus(synergyId: string): UpgradeEffect[] {
    if (this.synergyCache.has(synergyId)) {
      return this.synergyCache.get(synergyId)!;
    }

    const synergy = this.synergies.get(synergyId);
    if (!synergy || !synergy.isActive) {
      return [];
    }

    const bonuses = synergy.bonusEffects.map((effect) => ({
      ...effect,
      modifier: synergy.stackable
        ? effect.modifier * synergy.currentStacks
        : effect.modifier
    }));

    this.synergyCache.set(synergyId, bonuses);
    return bonuses;
  }

  getTotalBonusForStat(statName: string): number {
    let totalModifier = 0;

    this.activeSynergies.forEach((synergyId) => {
      const bonuses = this.getSynergyBonus(synergyId);
      bonuses.forEach((bonus) => {
        if (bonus.statName === statName) {
          totalModifier += bonus.modifier;
        }
      });
    });

    return totalModifier;
  }

  getSynergyProgress(synergyId: string): { completed: number; total: number } {
    const synergy = this.synergies.get(synergyId);
    if (!synergy) {
      return { completed: 0, total: 0 };
    }

    const completed = synergy.requiredUpgrades.filter((id) =>
      synergy.isActive || true
    ).length;

    return {
      completed,
      total: synergy.requiredUpgrades.length
    };
  }
}

export class UnlockConditionChecker {
  private customChecks: Map<string, (context: UpgradeContext) => boolean> = new Map();

  registerCustomCheck(id: string, check: (context: UpgradeContext) => boolean): void {
    this.customChecks.set(id, check);
  }

  unregisterCustomCheck(id: string): void {
    this.customChecks.delete(id);
  }

  checkCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    switch (condition.type) {
      case UnlockConditionType.ResourceRequired:
        return this.checkResourceCondition(condition, context);

      case UnlockConditionType.BuildingRequired:
        return this.checkBuildingCondition(condition, context);

      case UnlockConditionType.TechRequired:
        return this.checkTechCondition(condition, context);

      case UnlockConditionType.UpgradeRequired:
        return this.checkUpgradeCondition(condition, context);

      case UnlockConditionType.LevelRequired:
        return this.checkLevelCondition(condition, context);

      case UnlockConditionType.MissionRequired:
        return this.checkMissionCondition(condition, context);

      case UnlockConditionType.TimeRequired:
        return this.checkTimeCondition(condition, context);

      case UnlockConditionType.CustomCondition:
        return this.checkCustomCondition(condition, context);

      default:
        return { isUnlocked: true };
    }
  }

  checkAllConditions(
    conditions: UnlockCondition[],
    context: UpgradeContext
  ): { allUnlocked: boolean; failedConditions: string[] } {
    const failedConditions: string[] = [];

    for (const condition of conditions) {
      const result = this.checkCondition(condition, context);
      if (!result.isUnlocked && result.reason) {
        failedConditions.push(result.reason);
      }
    }

    return {
      allUnlocked: failedConditions.length === 0,
      failedConditions
    };
  }

  private checkResourceCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const resourceManager = ResourceManager.getInstance();
    const currentAmount = resourceManager.getResource(
      condition.targetId as ResourceType,
      context.playerId
    );
    const requiredAmount = condition.requiredValue || 0;

    if (currentAmount >= requiredAmount) {
      return { isUnlocked: true };
    }

    return {
      isUnlocked: false,
      reason: `Requires ${requiredAmount} ${condition.targetId}, currently have ${currentAmount}`
    };
  }

  private checkBuildingCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const buildings = context.gameState.getBuildings(context.playerId);
    const hasBuilding = buildings.some((b) => b.type === condition.targetId);

    if (hasBuilding) {
      return { isUnlocked: true };
    }

    return {
      isUnlocked: false,
      reason: `Requires ${condition.targetId} building`
    };
  }

  private checkTechCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const techTree = TechTree.getInstance();
    const hasResearched = techTree.isResearched(context.playerId, condition.targetId!);

    if (hasResearched) {
      return { isUnlocked: true };
    }

    return {
      isUnlocked: false,
      reason: `Requires ${condition.targetId} research`
    };
  }

  private checkUpgradeCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const upgradeManager = UpgradeManager.getInstance();
    const upgrade = upgradeManager.getUpgrade(condition.targetId!);
    const currentLevel = upgrade?.currentLevel || 0;
    const requiredLevel = condition.requiredValue || 1;

    if (currentLevel >= requiredLevel) {
      return { isUnlocked: true };
    }

    return {
      isUnlocked: false,
      reason: `Requires ${condition.targetId} at level ${requiredLevel}, currently at level ${currentLevel}`
    };
  }

  private checkLevelCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const playerLevel = context.gameState.getPlayerLevel(context.playerId);
    const requiredLevel = condition.requiredValue || 1;

    if (playerLevel >= requiredLevel) {
      return { isUnlocked: true };
    }

    return {
      isUnlocked: false,
      reason: `Requires player level ${requiredLevel}, currently at level ${playerLevel}`
    };
  }

  private checkMissionCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const completedMissions = context.gameState.getCompletedMissions(context.playerId);
    const missionCompleted = completedMissions.includes(condition.targetId!);

    if (missionCompleted) {
      return { isUnlocked: true };
    }

    return {
      isUnlocked: false,
      reason: `Requires completion of mission ${condition.targetId}`
    };
  }

  private checkTimeCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const gameTime = context.gameState.getGameTime();
    const requiredTime = condition.requiredValue || 0;

    if (gameTime >= requiredTime) {
      return { isUnlocked: true };
    }

    return {
      isUnlocked: false,
      reason: `Requires ${this.formatTime(requiredTime)} of game time`
    };
  }

  private checkCustomCondition(
    condition: UnlockCondition,
    context: UpgradeContext
  ): { isUnlocked: boolean; reason?: string } {
    const checkId = condition.customData?.checkId as string;
    const customCheck = this.customChecks.get(checkId);

    if (customCheck) {
      const result = customCheck(context);
      return {
        isUnlocked: result,
        reason: result ? undefined : `Custom condition "${checkId}" not met`
      };
    }

    return { isUnlocked: true };
  }

  private formatTime(milliseconds: number): string {
    const seconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);

    if (hours > 0) {
      return `${hours}h ${minutes % 60}m`;
    } else if (minutes > 0) {
      return `${minutes}m ${seconds % 60}s`;
    }
    return `${seconds}s`;
  }
}

export class UpgradeEffectsApplicator {
  private activeEffects: Map<string, Map<number, UpgradeEffect>> = new Map();
  private effectListeners: Array<(effect: UpgradeEffect, unit?: Unit) => void> = [];

  applyEffects(
    upgrade: UpgradeType,
    level: number,
    target: Unit | null,
    context: UpgradeContext
  ): void {
    const effects = this.getEffectsForLevel(upgrade, level);

    effects.forEach((effect) => {
      this.applyEffect(effect, target, context);
    });
  }

  removeEffects(
    upgrade: UpgradeType,
    level: number,
    target: Unit | null,
    context: UpgradeContext
  ): void {
    const effects = this.getEffectsForLevel(upgrade, level);

    effects.forEach((effect) => {
      this.removeEffect(effect, target, context);
    });
  }

  getEffectsForLevel(upgrade: UpgradeType, level: number): UpgradeEffect[] {
    const effects: UpgradeEffect[] = [];

    for (let l = 1; l <= level; l++) {
      const levelData = upgrade.levels?.find((lv) => lv.level === l);
      if (levelData) {
        effects.push(...levelData.effects);
      }
    }

    if (upgrade.effects.length > 0 && level > 0) {
      effects.push(...upgrade.effects);
    }

    return effects;
  }

  applyEffect(
    effect: UpgradeEffect,
    target: Unit | null,
    context: UpgradeContext
  ): void {
    if (effect.condition && !this.checkEffectCondition(effect.condition, context)) {
      return;
    }

    if (target) {
      this.applyToUnit(effect, target);
    }

    if (effect.duration) {
      this.scheduleEffectRemoval(effect, target, effect.duration);
    }

    this.notifyListeners(effect, target || undefined);
  }

  removeEffect(
    effect: UpgradeEffect,
    target: Unit | null,
    context: UpgradeContext
  ): void {
    if (target) {
      this.revertFromUnit(effect, target);
    }

    const unitId = target?.id || 'global';
    const unitEffects = this.activeEffects.get(unitId);
    if (unitEffects) {
      unitEffects.forEach((e, key) => {
        if (e === effect) {
          unitEffects.delete(key);
        }
      });
    }
  }

  applySynergyEffects(
    synergies: UpgradeSynergy[],
    target: Unit | null,
    context: UpgradeContext
  ): void {
    synergies.forEach((synergy) => {
      if (synergy.isActive) {
        synergy.bonusEffects.forEach((effect) => {
          const multipliedEffect = synergy.stackable
            ? { ...effect, modifier: effect.modifier * synergy.currentStacks }
            : effect;
          this.applyEffect(multipliedEffect, target, context);
        });
      }
    });
  }

  addListener(listener: (effect: UpgradeEffect, unit?: Unit) => void): void {
    this.effectListeners.push(listener);
  }

  removeListener(listener: (effect: UpgradeEffect, unit?: Unit) => void): void {
    const index = this.effectListeners.indexOf(listener);
    if (index > -1) {
      this.effectListeners.splice(index, 1);
    }
  }

  getActiveEffects(unitId: string): UpgradeEffect[] {
    const unitEffects = this.activeEffects.get(unitId);
    if (!unitEffects) return [];
    return Array.from(unitEffects.values());
  }

  private applyToUnit(effect: UpgradeEffect, unit: Unit): void {
    const stats = unit.stats;
    const currentValue = this.getStatValue(stats, effect.statName);

    let newValue: number;
    switch (effect.operation) {
      case 'add':
        newValue = currentValue + effect.modifier;
        break;
      case 'multiply':
        newValue = currentValue * effect.modifier;
        break;
      case 'set':
        newValue = effect.modifier;
        break;
      case 'percentage':
        newValue = currentValue * (1 + effect.modifier / 100);
        break;
      default:
        newValue = currentValue + effect.modifier;
    }

    this.setStatValue(stats, effect.statName, newValue);

    if (!this.activeEffects.has(unit.id)) {
      this.activeEffects.set(unit.id, new Map());
    }
    this.activeEffects.get(unit.id)!.set(this.getEffectKey(effect), effect);
  }

  private revertFromUnit(effect: UpgradeEffect, unit: Unit): void {
    const stats = unit.stats;
    const currentValue = this.getStatValue(stats, effect.statName);

    let newValue: number;
    switch (effect.operation) {
      case 'add':
        newValue = currentValue - effect.modifier;
        break;
      case 'multiply':
        newValue = currentValue / effect.modifier;
        break;
      case 'set':
        newValue = stats[effect.statName as keyof UnitStats] as number || 0;
        break;
      case 'percentage':
        newValue = currentValue / (1 + effect.modifier / 100);
        break;
      default:
        newValue = currentValue - effect.modifier;
    }

    this.setStatValue(stats, effect.statName, newValue);
  }

  private getStatValue(stats: UnitStats, statName: string): number {
    return (stats[statName as keyof UnitStats] as number) || 0;
  }

  private setStatValue(stats: UnitStats, statName: string, value: number): void {
    (stats as Record<string, number>)[statName] = Math.max(0, value);
  }

  private checkEffectCondition(
    condition: UpgradeCondition,
    context: UpgradeContext
  ): boolean {
    if (condition.unitType && context.unit) {
      if (context.unit.type !== condition.unitType) return false;
    }

    if (condition.minResourceAmount && condition.resourceType) {
      const resourceManager = ResourceManager.getInstance();
      const amount = resourceManager.getResource(condition.resourceType, context.playerId);
      if (amount < condition.minResourceAmount) return false;
    }

    if (condition.upgradeId && condition.level) {
      const upgradeManager = UpgradeManager.getInstance();
      const upgrade = upgradeManager.getUpgrade(condition.upgradeId);
      if (!upgrade || upgrade.currentLevel < condition.level) return false;
    }

    if (condition.customCheck) {
      return condition.customCheck(context);
    }

    return true;
  }

  private scheduleEffectRemoval(
    effect: UpgradeEffect,
    target: Unit | null,
    duration: number
  ): void {
    setTimeout(() => {
      if (target) {
        this.removeEffect(effect, target, {
          playerId: target.ownerId,
          gameState: GameState.getInstance(),
          timestamp: Date.now()
        });
      }
    }, duration);
  }

  private getEffectKey(effect: UpgradeEffect): number {
    return `${effect.statName}_${effect.operation}_${effect.modifier}`.split('').reduce((a, b) => {
      a = (a << 5) - a + b.charCodeAt(0);
      return a & a;
    }, 0);
  }

  private notifyListeners(effect: UpgradeEffect, unit?: Unit): void {
    this.effectListeners.forEach((listener) => listener(effect, unit));
  }
}

export class UpgradeHistoryTracker {
  private history: UpgradeHistoryEntry[] = [];
  private maxHistorySize: number = 1000;
  private listeners: Array<(entry: UpgradeHistoryEntry) => void> = [];

  recordUpgrade(
    upgradeId: string,
    level: number,
    previousLevel: number,
    cost: UpgradeCost,
    duration: number,
    source: UpgradeSource,
    autoTriggered: boolean = false
  ): void {
    const entry: UpgradeHistoryEntry = {
      timestamp: Date.now(),
      upgradeId,
      level,
      previousLevel,
      cost: new Map(cost.resources),
      duration,
      source,
      autoTriggered
    };

    this.history.unshift(entry);

    if (this.history.length > this.maxHistorySize) {
      this.history.pop();
    }

    this.notifyListeners(entry);
  }

  getHistory(
    options?: {
      upgradeId?: string;
      source?: UpgradeSource;
      fromTimestamp?: number;
      toTimestamp?: number;
      limit?: number;
    }
  ): UpgradeHistoryEntry[] {
    let filtered = [...this.history];

    if (options?.upgradeId) {
      filtered = filtered.filter((e) => e.upgradeId === options.upgradeId);
    }

    if (options?.source) {
      filtered = filtered.filter((e) => e.source === options.source);
    }

    if (options?.fromTimestamp) {
      filtered = filtered.filter((e) => e.timestamp >= options.fromTimestamp!);
    }

    if (options?.toTimestamp) {
      filtered = filtered.filter((e) => e.timestamp <= options.toTimestamp!);
    }

    if (options?.limit) {
      filtered = filtered.slice(0, options.limit);
    }

    return filtered;
  }

  getTotalSpentByResource(): Map<ResourceType, number> {
    const totals = new Map<ResourceType, number>();

    this.history.forEach((entry) => {
      entry.cost.forEach((amount, resourceType) => {
        const current = totals.get(resourceType) || 0;
        totals.set(resourceType, current + amount);
      });
    });

    return totals;
  }

  getUpgradeFrequency(): Map<string, number> {
    const frequency = new Map<string, number>();

    this.history.forEach((entry) => {
      const count = frequency.get(entry.upgradeId) || 0;
      frequency.set(entry.upgradeId, count + 1);
    });

    return frequency;
  }

  getAverageUpgradeTime(): number {
    if (this.history.length === 0) return 0;

    const totalTime = this.history.reduce((sum, entry) => sum + entry.duration, 0);
    return totalTime / this.history.length;
  }

  getMostUsedUpgrades(limit: number = 10): Array<{ upgradeId: string; count: number }> {
    const frequency = this.getUpgradeFrequency();
    const sorted = Array.from(frequency.entries())
      .sort((a, b) => b[1] - a[1])
      .slice(0, limit);

    return sorted.map(([upgradeId, count]) => ({ upgradeId, count }));
  }

  getRecentUpgrades(limit: number = 10): UpgradeHistoryEntry[] {
    return this.history.slice(0, limit);
  }

  exportHistory(): string {
    return JSON.stringify(this.history, null, 2);
  }

  importHistory(data: string): boolean {
    try {
      const imported = JSON.parse(data) as UpgradeHistoryEntry[];
      this.history = imported.slice(0, this.maxHistorySize);
      return true;
    } catch {
      return false;
    }
  }

  clearHistory(): void {
    this.history = [];
  }

  addListener(listener: (entry: UpgradeHistoryEntry) => void): void {
    this.listeners.push(listener);
  }

  removeListener(listener: (entry: UpgradeHistoryEntry) => void): void {
    const index = this.listeners.indexOf(listener);
    if (index > -1) {
      this.listeners.splice(index, 1);
    }
  }

  private notifyListeners(entry: UpgradeHistoryEntry): void {
    this.listeners.forEach((listener) => listener(entry));
  }
}

export class UpgradeAutomation {
  private automatedUpgrades: Map<string, AutomationTrigger> = new Map();
  private scheduledAutomations: Map<string, number> = new Map();
  private listeners: Array<(upgradeId: string) => void> = [];

  enableAutomation(upgradeId: string, trigger: AutomationTrigger): void {
    this.automatedUpgrades.set(upgradeId, trigger);
    this.setupTrigger(upgradeId, trigger);
  }

  disableAutomation(upgradeId: string): void {
    this.automatedUpgrades.delete(upgradeId);
    this.clearTrigger(upgradeId);
  }

  isAutomated(upgradeId: string): boolean {
    return this.automatedUpgrades.has(upgradeId);
  }

  getAutomationTrigger(upgradeId: string): AutomationTrigger | undefined {
    return this.automatedUpgrades.get(upgradeId);
  }

  getAllAutomatedUpgrades(): Map<string, AutomationTrigger> {
    return new Map(this.automatedUpgrades);
  }

  setScheduledTime(upgradeId: string, timestamp: number): void {
    this.scheduledAutomations.set(upgradeId, timestamp);
  }

  getScheduledTime(upgradeId: string): number | undefined {
    return this.scheduledAutomations.get(upgradeId);
  }

  triggerAutomation(upgradeId: string): void {
    this.notifyListeners(upgradeId);
  }

  handleResourceThreshold(
    resourceType: ResourceType,
    threshold: number,
    currentAmount: number
  ): void {
    if (currentAmount < threshold) return;

    this.automatedUpgrades.forEach((trigger, upgradeId) => {
      if (trigger === AutomationTrigger.OnResourceThreshold) {
        this.triggerAutomation(upgradeId);
      }
    });
  }

  handleUnitSpawn(unitType: string): void {
    this.automatedUpgrades.forEach((trigger, upgradeId) => {
      if (trigger === AutomationTrigger.OnUnitSpawn) {
        this.triggerAutomation(upgradeId);
      }
    });
  }

  handleBuildingComplete(buildingType: string): void {
    this.automatedUpgrades.forEach((trigger, upgradeId) => {
      if (trigger === AutomationTrigger.OnBuildingComplete) {
        this.triggerAutomation(upgradeId);
      }
    });
  }

  handleResearchComplete(researchId: string): void {
    this.automatedUpgrades.forEach((trigger, upgradeId) => {
      if (trigger === AutomationTrigger.OnResearchComplete) {
        this.triggerAutomation(upgradeId);
      }
    });
  }

  addListener(listener: (upgradeId: string) => void): void {
    this.listeners.push(listener);
  }

  removeListener(listener: (upgradeId: string) => void): void {
    const index = this.listeners.indexOf(listener);
    if (index > -1) {
      this.listeners.splice(index, 1);
    }
  }

  private setupTrigger(upgradeId: string, trigger: AutomationTrigger): void {
    if (trigger === AutomationTrigger.Scheduled) {
      const scheduledTime = this.scheduledAutomations.get(upgradeId);
      if (scheduledTime) {
        const delay = scheduledTime - Date.now();
        if (delay > 0) {
          setTimeout(() => this.triggerAutomation(upgradeId), delay);
        }
      }
    }
  }

  private clearTrigger(upgradeId: string): void {
    this.scheduledAutomations.delete(upgradeId);
  }

  private notifyListeners(upgradeId: string): void {
    this.listeners.forEach((listener) => listener(upgradeId));
  }
}

export class UpgradeUI {
  private visibleCategories: Set<UpgradeCategory> = new Set(Object.values(UpgradeCategory));
  private selectedUpgrade: string | null = null;
  private listeners: Map<string, Array<(data: unknown) => void>> = new Map();
  private tooltipData: Map<string, unknown> = new Map();

  selectUpgrade(upgradeId: string | null): void {
    this.selectedUpgrade = upgradeId;
    this.emit('upgradeSelected', upgradeId);
  }

  getSelectedUpgrade(): string | null {
    return this.selectedUpgrade;
  }

  setVisibleCategories(categories: UpgradeCategory[]): void {
    this.visibleCategories = new Set(categories);
    this.emit('categoriesChanged', Array.from(this.visibleCategories));
  }

  getVisibleCategories(): UpgradeCategory[] {
    return Array.from(this.visibleCategories);
  }

  showTooltip(upgradeId: string, data: unknown): void {
    this.tooltipData.set(upgradeId, data);
    this.emit('tooltipShow', { upgradeId, data });
  }

  hideTooltip(upgradeId: string): void {
    this.tooltipData.delete(upgradeId);
    this.emit('tooltipHide', upgradeId);
  }

  getTooltipData(upgradeId: string): unknown {
    return this.tooltipData.get(upgradeId);
  }

  renderUpgradeCard(upgrade: UpgradeType, progress: UpgradeProgress): unknown {
    return {
      id: upgrade.id,
      name: upgrade.name,
      description: upgrade.description,
      category: upgrade.category,
      tier: upgrade.tier,
      icon: upgrade.icon,
      level: progress.currentLevel,
      maxLevel: upgrade.maxLevel,
      progress: progress.progressPercentage,
      cost: this.formatCost(upgrade.cost),
      effects: this.formatEffects(upgrade.effects),
      isAvailable: progress.currentLevel < upgrade.maxLevel,
      isLocked: progress.currentLevel === 0 && upgrade.prerequisites.length > 0
    };
  }

  renderUpgradeTree(tree: UpgradeTreeManager): unknown {
    const nodes = tree.getAllNodes();
    const renderedNodes: unknown[] = [];

    nodes.forEach((node, id) => {
      renderedNodes.push({
        id,
        position: node.position,
        isUnlocked: node.isUnlocked,
        isCompleted: node.isCompleted,
        isAvailable: tree.isAvailable(id)
      });
    });

    return { nodes: renderedNodes };
  }

  formatCost(cost: UpgradeCost): unknown {
    const formatted: Record<string, number> = {};

    cost.resources.forEach((amount, type) => {
      formatted[type] = amount;
    });

    return {
      ...formatted,
      time: cost.time,
      experience: cost.experience,
      population: cost.population
    };
  }

  formatEffects(effects: UpgradeEffect[]): unknown[] {
    return effects.map((effect) => ({
      stat: effect.statName,
      modifier: effect.modifier,
      operation: effect.operation,
      formatted: this.formatEffectValue(effect)
    }));
  }

  private formatEffectValue(effect: UpgradeEffect): string {
    const sign = effect.modifier >= 0 ? '+' : '';
    switch (effect.operation) {
      case 'add':
        return `${sign}${effect.modifier} ${effect.statName}`;
      case 'multiply':
        return `${sign}${(effect.modifier - 1) * 100}% ${effect.statName}`;
      case 'set':
        return `${effect.modifier} ${effect.statName}`;
      case 'percentage':
        return `${sign}${effect.modifier}% ${effect.statName}`;
      default:
        return `${sign}${effect.modifier} ${effect.statName}`;
    }
  }

  on(event: string, listener: (data: unknown) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  off(event: string, listener: (data: unknown) => void): void {
    const eventListeners = this.listeners.get(event);
    if (eventListeners) {
      const index = eventListeners.indexOf(listener);
      if (index > -1) {
        eventListeners.splice(index, 1);
      }
    }
  }

  private emit(event: string, data: unknown): void {
    const eventListeners = this.listeners.get(event);
    if (eventListeners) {
      eventListeners.forEach((listener) => listener(data));
    }
  }
}

export class GlobalUpgrades {
  private globalUpgrades: Map<string, UpgradeType> = new Map();
  private playerUpgrades: Map<string, Map<string, number>> = new Map();

  registerGlobalUpgrade(upgrade: UpgradeType): void {
    this.globalUpgrades.set(upgrade.id, upgrade);
  }

  unregisterGlobalUpgrade(upgradeId: string): void {
    this.globalUpgrades.delete(upgradeId);
  }

  getGlobalUpgrade(upgradeId: string): UpgradeType | undefined {
    return this.globalUpgrades.get(upgradeId);
  }

  getAllGlobalUpgrades(): UpgradeType[] {
    return Array.from(this.globalUpgrades.values());
  }

  applyGlobalUpgrade(playerId: string, upgradeId: string): boolean {
    const upgrade = this.globalUpgrades.get(upgradeId);
    if (!upgrade) return false;

    if (!this.playerUpgrades.has(playerId)) {
      this.playerUpgrades.set(playerId, new Map());
    }

    const playerUpgradeMap = this.playerUpgrades.get(playerId)!;
    const currentLevel = playerUpgradeMap.get(upgradeId) || 0;

    if (currentLevel >= upgrade.maxLevel) {
      return false;
    }

    playerUpgradeMap.set(upgradeId, currentLevel + 1);

    const context: UpgradeContext = {
      playerId,
      gameState: GameState.getInstance(),
      timestamp: Date.now()
    };

    const effectsApplicator = new UpgradeEffectsApplicator();
    effectsApplicator.applyEffects(upgrade, currentLevel + 1, null, context);

    return true;
  }

  getPlayerUpgradeLevel(playerId: string, upgradeId: string): number {
    const playerUpgradeMap = this.playerUpgrades.get(playerId);
    return playerUpgradeMap?.get(upgradeId) || 0;
  }

  getPlayerGlobalUpgrades(playerId: string): Array<{ upgrade: UpgradeType; level: number }> {
    const playerUpgradeMap = this.playerUpgrades.get(playerId);
    if (!playerUpgradeMap) return [];

    const upgrades: Array<{ upgrade: UpgradeType; level: number }> = [];
    playerUpgradeMap.forEach((level, upgradeId) => {
      const upgrade = this.globalUpgrades.get(upgradeId);
      if (upgrade) {
        upgrades.push({ upgrade, level });
      }
    });

    return upgrades;
  }

  getGlobalUpgradesByCategory(category: UpgradeCategory): UpgradeType[] {
    return this.getAllGlobalUpgrades().filter((u) => u.category === category);
  }

  getGlobalUpgradesByTier(tier: UpgradeTier): UpgradeType[] {
    return this.getAllGlobalUpgrades().filter((u) => u.tier === tier);
  }

  resetPlayerUpgrades(playerId: string): void {
    this.playerUpgrades.delete(playerId);
  }

  getTotalGlobalBonus(playerId: string, statName: string): number {
    let totalBonus = 0;
    const playerUpgradeMap = this.playerUpgrades.get(playerId);

    if (!playerUpgradeMap) return 0;

    playerUpgradeMap.forEach((level, upgradeId) => {
      const upgrade = this.globalUpgrades.get(upgradeId);
      if (upgrade) {
        for (let l = 1; l <= level; l++) {
          upgrade.effects.forEach((effect) => {
            if (effect.statName === statName) {
              totalBonus += effect.modifier;
            }
          });
        }
      }
    });

    return totalBonus;
  }
}

export class UnitSpecificUpgrades {
  private unitUpgrades: Map<string, UpgradeType> = new Map();
  private unitUpgradeLevels: Map<string, Map<string, number>> = new Map();
  private unitTypeUpgrades: Map<string, Set<string>> = new Map();

  registerUnitUpgrade(upgrade: UpgradeType): void {
    this.unitUpgrades.set(upgrade.id, upgrade);

    if (upgrade.affectedUnits) {
      upgrade.affectedUnits.forEach((unitType) => {
        if (!this.unitTypeUpgrades.has(unitType)) {
          this.unitTypeUpgrades.set(unitType, new Set());
        }
        this.unitTypeUpgrades.get(unitType)!.add(upgrade.id);
      });
    }
  }

  unregisterUnitUpgrade(upgradeId: string): void {
    const upgrade = this.unitUpgrades.get(upgradeId);
    if (upgrade?.affectedUnits) {
      upgrade.affectedUnits.forEach((unitType) => {
        this.unitTypeUpgrades.get(unitType)?.delete(upgradeId);
      });
    }
    this.unitUpgrades.delete(upgradeId);
  }

  getUnitUpgrade(upgradeId: string): UpgradeType | undefined {
    return this.unitUpgrades.get(upgradeId);
  }

  getAllUnitUpgrades(): UpgradeType[] {
    return Array.from(this.unitUpgrades.values());
  }

  applyUnitUpgrade(unitId: string, unit: Unit, upgradeId: string): boolean {
    const upgrade = this.unitUpgrades.get(upgradeId);
    if (!upgrade) return false;

    if (upgrade.affectedUnits && !upgrade.affectedUnits.includes(unit.type)) {
      return false;
    }

    if (!this.unitUpgradeLevels.has(unitId)) {
      this.unitUpgradeLevels.set(unitId, new Map());
    }

    const unitUpgradeMap = this.unitUpgradeLevels.get(unitId)!;
    const currentLevel = unitUpgradeMap.get(upgradeId) || 0;

    if (currentLevel >= upgrade.maxLevel) {
      return false;
    }

    unitUpgradeMap.set(upgradeId, currentLevel + 1);

    const context: UpgradeContext = {
      unit,
      playerId: unit.ownerId,
      gameState: GameState.getInstance(),
      timestamp: Date.now()
    };

    const effectsApplicator = new UpgradeEffectsApplicator();
    effectsApplicator.applyEffects(upgrade, currentLevel + 1, unit, context);

    return true;
  }

  getUnitUpgradeLevel(unitId: string, upgradeId: string): number {
    const unitUpgradeMap = this.unitUpgradeLevels.get(unitId);
    return unitUpgradeMap?.get(upgradeId) || 0;
  }

  getUnitUpgrades(unitId: string): Array<{ upgrade: UpgradeType; level: number }> {
    const unitUpgradeMap = this.unitUpgradeLevels.get(unitId);
    if (!unitUpgradeMap) return [];

    const upgrades: Array<{ upgrade: UpgradeType; level: number }> = [];
    unitUpgradeMap.forEach((level, upgradeId) => {
      const upgrade = this.unitUpgrades.get(upgradeId);
      if (upgrade) {
        upgrades.push({ upgrade, level });
      }
    });

    return upgrades;
  }

  getUpgradesForUnitType(unitType: string): UpgradeType[] {
    const upgradeIds = this.unitTypeUpgrades.get(unitType);
    if (!upgradeIds) return [];

    return Array.from(upgradeIds)
      .map((id) => this.unitUpgrades.get(id))
      .filter((u): u is UpgradeType => u !== undefined);
  }

  getAvailableUpgradesForUnit(unit: Unit): UpgradeType[] {
    return this.getUpgradesForUnitType(unit.type).filter((upgrade) => {
      const currentLevel = this.getUnitUpgradeLevel(unit.id, upgrade.id);
      return currentLevel < upgrade.maxLevel;
    });
  }

  removeAllUpgradesFromUnit(unitId: string): void {
    this.unitUpgradeLevels.delete(unitId);
  }

  transferUpgrades(fromUnitId: string, toUnitId: string): void {
    const fromUpgrades = this.unitUpgradeLevels.get(fromUnitId);
    if (!fromUpgrades) return;

    if (!this.unitUpgradeLevels.has(toUnitId)) {
      this.unitUpgradeLevels.set(toUnitId, new Map());
    }

    const toUpgrades = this.unitUpgradeLevels.get(toUnitId)!;
    fromUpgrades.forEach((level, upgradeId) => {
      const currentToLevel = toUpgrades.get(upgradeId) || 0;
      toUpgrades.set(upgradeId, Math.max(currentToLevel, level));
    });

    this.unitUpgradeLevels.delete(fromUnitId);
  }

  getTotalUnitBonus(unit: Unit, statName: string): number {
    let totalBonus = 0;
    const unitUpgradeMap = this.unitUpgradeLevels.get(unit.id);

    if (!unitUpgradeMap) return 0;

    unitUpgradeMap.forEach((level, upgradeId) => {
      const upgrade = this.unitUpgrades.get(upgradeId);
      if (upgrade) {
        for (let l = 1; l <= level; l++) {
          upgrade.effects.forEach((effect) => {
            if (effect.statName === statName) {
              totalBonus += effect.modifier;
            }
          });
        }
      }
    });

    return totalBonus;
  }
}

export interface UnlockCondition {
  type: UnlockConditionType;
  targetId?: string;
  requiredValue?: number;
  secondaryTarget?: string;
  secondaryValue?: number;
  customData?: Record<string, unknown>;
}

export class UpgradeManager {
  private static instance: UpgradeManager;
  private upgrades: Map<string, UpgradeType> = new Map();
  private playerUpgrades: Map<string, Map<string, number>> = new Map();
  private progressTracker: UpgradeProgressTracker;
  private costCalculator: UpgradeCostCalculator;
  private treeManager: UpgradeTreeManager;
  private synergyManager: UpgradeSynergyManager;
  private unlockChecker: UnlockConditionChecker;
  private effectsApplicator: UpgradeEffectsApplicator;
  private historyTracker: UpgradeHistoryTracker;
  private automation: UpgradeAutomation;
  private globalUpgrades: GlobalUpgrades;
  private unitUpgrades: UnitSpecificUpgrades;
  private ui: UpgradeUI;
  private eventEmitter: EventEmitter;
  private logger: Logger;
  private isProcessing: boolean = false;
  private processingQueue: Array<{
    playerId: string;
    upgradeId: string;
    timestamp: number;
  }> = new Array();

  private constructor() {
    this.progressTracker = new UpgradeProgressTracker();
    this.costCalculator = new UpgradeCostCalculator();
    this.treeManager = new UpgradeTreeManager();
    this.synergyManager = new UpgradeSynergyManager();
    this.unlockChecker = new UnlockConditionChecker();
    this.effectsApplicator = new UpgradeEffectsApplicator();
    this.historyTracker = new UpgradeHistoryTracker();
    this.automation = new UpgradeAutomation();
    this.globalUpgrades = new GlobalUpgrades();
    this.unitUpgrades = new UnitSpecificUpgrades();
    this.ui = new UpgradeUI();
    this.eventEmitter = EventEmitter.getInstance();
    this.logger = Logger.getInstance();

    this.setupEventListeners();
  }

  static getInstance(): UpgradeManager {
    if (!UpgradeManager.instance) {
      UpgradeManager.instance = new UpgradeManager();
    }
    return UpgradeManager.instance;
  }

  registerUpgrade(upgrade: UpgradeType): void {
    if (this.upgrades.has(upgrade.id)) {
      this.logger.warn(`Upgrade ${upgrade.id} already registered, replacing...`);
    }

    this.upgrades.set(upgrade.id, upgrade);
    this.treeManager.addNode(upgrade);

    if (upgrade.source === UpgradeSource.Default) {
      this.globalUpgrades.registerGlobalUpgrade(upgrade);
    }

    this.eventEmitter.emit('upgradeRegistered', { upgradeId: upgrade.id });
    this.logger.info(`Registered upgrade: ${upgrade.name} (${upgrade.id})`);
  }

  unregisterUpgrade(upgradeId: string): void {
    const upgrade = this.upgrades.get(upgradeId);
    if (upgrade) {
      this.upgrades.delete(upgradeId);
      this.treeManager.removeNode(upgradeId);
      this.globalUpgrades.unregisterGlobalUpgrade(upgradeId);
      this.unitUpgrades.unregisterUnitUpgrade(upgradeId);
      this.eventEmitter.emit('upgradeUnregistered', { upgradeId });
    }
  }

  getUpgrade(upgradeId: string): UpgradeType | undefined {
    return this.upgrades.get(upgradeId);
  }

  getAllUpgrades(): UpgradeType[] {
    return Array.from(this.upgrades.values());
  }

  getUpgradesByCategory(category: UpgradeCategory): UpgradeType[] {
    return this.getAllUpgrades().filter((u) => u.category === category);
  }

  getUpgradesByTier(tier: UpgradeTier): UpgradeType[] {
    return this.getAllUpgrades().filter((u) => u.tier === tier);
  }

  getUpgradesBySource(source: UpgradeSource): UpgradeType[] {
    return this.getAllUpgrades().filter((u) => u.source === source);
  }

  startUpgrade(playerId: string, upgradeId: string): boolean {
    if (this.isProcessing) {
      this.processingQueue.push({ playerId, upgradeId, timestamp: Date.now() });
      return false;
    }

    this.isProcessing = true;

    const upgrade = this.upgrades.get(upgradeId);
    if (!upgrade) {
      this.logger.error(`Upgrade ${upgradeId} not found`);
      this.isProcessing = false;
      return false;
    }

    const currentLevel = this.getPlayerUpgradeLevel(playerId, upgradeId);
    if (currentLevel >= upgrade.maxLevel) {
      this.logger.warn(`Upgrade ${upgradeId} already at max level`);
      this.isProcessing = false;
      return false;
    }

    const context: UpgradeContext = {
      playerId,
      gameState: GameState.getInstance(),
      timestamp: Date.now()
    };

    if (upgrade.unlockCondition) {
      const unlockResult = this.unlockChecker.checkCondition(
        upgrade.unlockCondition,
        context
      );
      if (!unlockResult.isUnlocked) {
        this.logger.warn(`Unlock condition not met for ${upgradeId}: ${unlockResult.reason}`);
        this.isProcessing = false;
        return false;
      }
    }

    const cost = this.costCalculator.calculateCost(
      upgrade.cost,
      currentLevel + 1,
      upgrade.category,
      upgrade.tier
    );

    const canAfford = this.checkCanAfford(playerId, cost);
    if (!canAfford) {
      this.logger.warn(`Cannot afford upgrade ${upgradeId}`);
      this.isProcessing = false;
      return false;
    }

    this.deductCost(playerId, cost);

    this.progressTracker.updateProgress(upgradeId, {
      currentLevel: currentLevel + 1,
      progressPercentage: 0,
      startTime: Date.now()
    });

    this.eventEmitter.emit('upgradeStarted', {
      playerId,
      upgradeId,
      level: currentLevel + 1
    });

    setTimeout(() => {
      this.completeUpgrade(playerId, upgradeId, cost);
      this.isProcessing = false;
      this.processNextInQueue();
    }, cost.time);

    return true;
  }

  completeUpgrade(
    playerId: string,
    upgradeId: string,
    cost: UpgradeCost
  ): void {
    const upgrade = this.upgrades.get(upgradeId);
    if (!upgrade) return;

    const currentLevel = this.getPlayerUpgradeLevel(playerId, upgradeId);
    const previousLevel = currentLevel;

    if (!this.playerUpgrades.has(playerId)) {
      this.playerUpgrades.set(playerId, new Map());
    }

    this.playerUpgrades.get(playerId)!.set(upgradeId, currentLevel);

    this.progressTracker.completeUpgrade(upgradeId, currentLevel);
    this.treeManager.updateNodeStatus(upgradeId, true, true);

    const context: UpgradeContext = {
      playerId,
      gameState: GameState.getInstance(),
      timestamp: Date.now()
    };

    this.effectsApplicator.applyEffects(upgrade, currentLevel, null, context);

    const synergies = this.synergyManager.getActiveSynergies();
    this.effectsApplicator.applySynergyEffects(synergies, null, context);

    this.historyTracker.recordUpgrade(
      upgradeId,
      currentLevel,
      previousLevel,
      cost,
      cost.time,
      upgrade.source,
      this.automation.isAutomated(upgradeId)
    );

    this.eventEmitter.emit('upgradeCompleted', {
      playerId,
      upgradeId,
      level: currentLevel
    });

    if (this.automation.isAutomated(upgradeId)) {
      this.automation.triggerAutomation(upgradeId);
    }

    this.logger.info(`Completed upgrade ${upgradeId} for player ${playerId} at level ${currentLevel}`);
  }

  cancelUpgrade(playerId: string, upgradeId: string): boolean {
    const progress = this.progressTracker.getProgress(upgradeId);
    if (!progress || !progress.startTime) {
      return false;
    }

    const upgrade = this.upgrades.get(upgradeId);
    if (!upgrade) return false;

    const elapsedTime = Date.now() - progress.startTime;
    const refundPercentage = this.costCalculator.calculateRefundPercentage(elapsedTime);

    const currentLevel = this.getPlayerUpgradeLevel(playerId, upgradeId);
    const cost = this.costCalculator.calculateCost(
      upgrade.cost,
      currentLevel,
      upgrade.category,
      upgrade.tier
    );

    const refund = new Map<ResourceType, number>();
    cost.resources.forEach((amount, type) => {
      refund.set(type, Math.floor(amount * refundPercentage));
    });

    this.refundCost(playerId, refund);

    this.progressTracker.resetProgress(upgradeId);

    this.eventEmitter.emit('upgradeCancelled', {
      playerId,
      upgradeId,
      refund
    });

    return true;
  }

  getPlayerUpgradeLevel(playerId: string, upgradeId: string): number {
    const playerUpgradeMap = this.playerUpgrades.get(playerId);
    return playerUpgradeMap?.get(upgradeId) || 0;
  }

  getPlayerUpgrades(playerId: string): Array<{ upgrade: UpgradeType; level: number }> {
    const playerUpgradeMap = this.playerUpgrades.get(playerId);
    if (!playerUpgradeMap) return [];

    const upgrades: Array<{ upgrade: UpgradeType; level: number }> = [];
    playerUpgradeMap.forEach((level, upgradeId) => {
      const upgrade = this.upgrades.get(upgradeId);
      if (upgrade) {
        upgrades.push({ upgrade, level });
      }
    });

    return upgrades;
  }

  getAvailableUpgrades(playerId: string): UpgradeType[] {
    return this.getAllUpgrades().filter((upgrade) => {
      const currentLevel = this.getPlayerUpgradeLevel(playerId, upgrade.id);
      if (currentLevel >= upgrade.maxLevel) return false;

      const context: UpgradeContext = {
        playerId,
        gameState: GameState.getInstance(),
        timestamp: Date.now()
      };

      if (upgrade.unlockCondition) {
        const result = this.unlockChecker.checkCondition(upgrade.unlockCondition, context);
        if (!result.isUnlocked) return false;
      }

      return upgrade.prerequisites.every((prereqId) => {
        const prereqLevel = this.getPlayerUpgradeLevel(playerId, prereqId);
        return prereqLevel > 0;
      });
    });
  }

  getUpgradeStats(playerId: string): UpgradeStats {
    const playerUpgrades = this.getPlayerUpgrades(playerId);

    const upgradesByCategory = new Map<UpgradeCategory, number>();
    const upgradesByTier = new Map<UpgradeTier, number>();
    const totalSpent = this.historyTracker.getTotalSpentByResource();

    playerUpgrades.forEach(({ upgrade }) => {
      const categoryCount = upgradesByCategory.get(upgrade.category) || 0;
      upgradesByCategory.set(upgrade.category, categoryCount + 1);

      const tierCount = upgradesByTier.get(upgrade.tier) || 0;
      upgradesByTier.set(upgrade.tier, tierCount + 1);
    });

    let totalLevels = 0;
    let totalMaxLevels = 0;
    playerUpgrades.forEach(({ upgrade, level }) => {
      totalLevels += level;
      totalMaxLevels += upgrade.maxLevel;
    });

    return {
      totalUpgrades: playerUpgrades.length,
      upgradesByCategory,
      upgradesByTier,
      totalSpent,
      totalTime: this.historyTracker.getAverageUpgradeTime() * playerUpgrades.length,
      averageLevel: playerUpgrades.length > 0 ? totalLevels / playerUpgrades.length : 0,
      completionPercentage: totalMaxLevels > 0 ? (totalLevels / totalMaxLevels) * 100 : 0
    };
  }

  resetPlayerUpgrades(playerId: string): void {
    this.playerUpgrades.delete(playerId);
    this.globalUpgrades.resetPlayerUpgrades(playerId);
    this.eventEmitter.emit('playerUpgradesReset', { playerId });
  }

  applyUpgradeToUnit(unit: Unit, upgradeId: string): boolean {
    const upgrade = this.upgrades.get(upgradeId);
    if (!upgrade) return false;

    if (upgrade.affectedUnits && !upgrade.affectedUnits.includes(unit.type)) {
      return false;
    }

    return this.unitUpgrades.applyUnitUpgrade(unit.id, unit, upgradeId);
  }

  getUnitUpgrades(unit: Unit): Array<{ upgrade: UpgradeType; level: number }> {
    return this.unitUpgrades.getUnitUpgrades(unit.id);
  }

  getGlobalBonus(playerId: string, statName: string): number {
    return this.globalUpgrades.getTotalGlobalBonus(playerId, statName);
  }

  getUnitBonus(unit: Unit, statName: string): number {
    return this.unitUpgrades.getTotalUnitBonus(unit, statName);
  }

  addSynergy(synergy: UpgradeSynergy): void {
    this.synergyManager.registerSynergy(synergy);
  }

  removeSynergy(synergyId: string): void {
    this.synergyManager.unregisterSynergy(synergyId);
  }

  getActiveSynergies(): UpgradeSynergy[] {
    return this.synergyManager.getActiveSynergies();
  }

  getUpgradeTree(): UpgradeTreeManager {
    return this.treeManager;
  }

  getProgressTracker(): UpgradeProgressTracker {
    return this.progressTracker;
  }

  getHistoryTracker(): UpgradeHistoryTracker {
    return this.historyTracker;
  }

  getAutomation(): UpgradeAutomation {
    return this.automation;
  }

  getUI(): UpgradeUI {
    return this.ui;
  }

  exportUpgrades(playerId: string): string {
    const playerUpgradeMap = this.playerUpgrades.get(playerId);
    if (!playerUpgradeMap) return '{}';

    const exportData: Record<string, number> = {};
    playerUpgradeMap.forEach((level, upgradeId) => {
      exportData[upgradeId] = level;
    });

    return JSON.stringify(exportData);
  }

  importUpgrades(playerId: string, data: string): boolean {
    try {
      const imported = JSON.parse(data) as Record<string, number>;
      const playerUpgradeMap = new Map<string, number>();

      Object.entries(imported).forEach(([upgradeId, level]) => {
        if (this.upgrades.has(upgradeId)) {
          playerUpgradeMap.set(upgradeId, level);
        }
      });

      this.playerUpgrades.set(playerId, playerUpgradeMap);
      return true;
    } catch {
      this.logger.error('Failed to import upgrades');
      return false;
    }
  }

  private checkCanAfford(playerId: string, cost: UpgradeCost): boolean {
    const resourceManager = ResourceManager.getInstance();

    for (const [resourceType, amount] of cost.resources) {
      const currentAmount = resourceManager.getResource(resourceType, playerId);
      if (currentAmount < amount) {
        return false;
      }
    }

    return true;
  }

  private deductCost(playerId: string, cost: UpgradeCost): void {
    const resourceManager = ResourceManager.getInstance();

    cost.resources.forEach((amount, resourceType) => {
      resourceManager.deductResource(resourceType, amount, playerId);
    });
  }

  private refundCost(playerId: string, refund: Map<ResourceType, number>): void {
    const resourceManager = ResourceManager.getInstance();

    refund.forEach((amount, resourceType) => {
      resourceManager.addResource(resourceType, amount, playerId);
    });
  }

  private processNextInQueue(): void {
    if (this.processingQueue.length === 0) return;

    const next = this.processingQueue.shift();
    if (next) {
      this.startUpgrade(next.playerId, next.upgradeId);
    }
  }

  private setupEventListeners(): void {
    this.eventEmitter.on('resourceChanged', (data: { playerId: string; type: ResourceType; amount: number }) => {
      this.automation.handleResourceThreshold(data.type, data.amount, data.amount);
    });

    this.eventEmitter.on('unitSpawned', (data: { unit: Unit }) => {
      this.automation.handleUnitSpawn(data.unit.type);
    });

    this.eventEmitter.on('buildingCompleted', (data: { type: string }) => {
      this.automation.handleBuildingComplete(data.type);
    });

    this.eventEmitter.on('researchCompleted', (data: { researchId: string }) => {
      this.automation.handleResearchComplete(data.researchId);
    });

    this.automation.addListener((upgradeId: string) => {
      const upgrades = this.automation.getAllAutomatedUpgrades();
      upgrades.forEach((trigger, id) => {
        if (id === upgradeId) {
          this.eventEmitter.emit('autoUpgradeTriggered', { upgradeId: id, trigger });
        }
      });
    });
  }
}

export class UpgradeFactory {
  static createWeaponUpgrade(
    id: string,
    name: string,
    description: string,
    tier: UpgradeTier,
    effects: UpgradeEffect[],
    baseCost: Map<ResourceType, number>,
    baseTime: number = 60000
  ): UpgradeType {
    return {
      id,
      name,
      description,
      category: UpgradeCategory.Weapon,
      tier,
      effects,
      prerequisites: [],
      cost: {
        resources: new Map(baseCost),
        time: baseTime
      },
      maxLevel: 5,
      source: UpgradeSource.Default,
      levels: this.generateLevels(effects, baseCost, baseTime)
    };
  }

  static createArmorUpgrade(
    id: string,
    name: string,
    description: string,
    tier: UpgradeTier,
    effects: UpgradeEffect[],
    baseCost: Map<ResourceType, number>,
    baseTime: number = 60000
  ): UpgradeType {
    return {
      id,
      name,
      description,
      category: UpgradeCategory.Armor,
      tier,
      effects,
      prerequisites: [],
      cost: {
        resources: new Map(baseCost),
        time: baseTime
      },
      maxLevel: 5,
      source: UpgradeSource.Default,
      levels: this.generateLevels(effects, baseCost, baseTime)
    };
  }

  static createAbilityUpgrade(
    id: string,
    name: string,
    description: string,
    tier: UpgradeTier,
    effects: UpgradeEffect[],
    baseCost: Map<ResourceType, number>,
    baseTime: number = 90000
  ): UpgradeType {
    return {
      id,
      name,
      description,
      category: UpgradeCategory.Ability,
      tier,
      effects,
      prerequisites: [],
      cost: {
        resources: new Map(baseCost),
        time: baseTime
      },
      maxLevel: 3,
      source: UpgradeSource.Default,
      levels: this.generateLevels(effects, baseCost, baseTime)
    };
  }

  static createMovementUpgrade(
    id: string,
    name: string,
    description: string,
    tier: UpgradeTier,
    effects: UpgradeEffect[],
    baseCost: Map<ResourceType, number>,
    baseTime: number = 45000
  ): UpgradeType {
    return {
      id,
      name,
      description,
      category: UpgradeCategory.Movement,
      tier,
      effects,
      prerequisites: [],
      cost: {
        resources: new Map(baseCost),
        time: baseTime
      },
      maxLevel: 5,
      source: UpgradeSource.Default,
      levels: this.generateLevels(effects, baseCost, baseTime)
    };
  }

  static createSpecialUpgrade(
    id: string,
    name: string,
    description: string,
    tier: UpgradeTier,
    effects: UpgradeEffect[],
    baseCost: Map<ResourceType, number>,
    baseTime: number = 120000
  ): UpgradeType {
    return {
      id,
      name,
      description,
      category: UpgradeCategory.Special,
      tier,
      effects,
      prerequisites: [],
      cost: {
        resources: new Map(baseCost),
        time: baseTime
      },
      maxLevel: 3,
      source: UpgradeSource.Special,
      levels: this.generateLevels(effects, baseCost, baseTime)
    };
  }

  private static generateLevels(
    effects: UpgradeEffect[],
    baseCost: Map<ResourceType, number>,
    baseTime: number
  ): UpgradeLevel[] {
    const levels: UpgradeLevel[] = [];

    for (let i = 1; i <= 5; i++) {
      levels.push({
        level: i,
        effects: effects.map((e) => ({
          ...e,
          modifier: e.modifier * i
        })),
        cost: {
          resources: new Map(
            Array.from(baseCost.entries()).map(([type, amount]) => [
              type,
              Math.floor(amount * Math.pow(1.5, i - 1))
            ])
          ),
          time: Math.floor(baseTime * Math.pow(1.3, i - 1)),
          experience: 100 * i
        },
        timeRequired: Math.floor(baseTime * Math.pow(1.3, i - 1)),
        experienceRequired: 100 * i
      });
    }

    return levels;
  }

  static createSynergy(
    id: string,
    name: string,
    description: string,
    requiredUpgrades: string[],
    bonusEffects: UpgradeEffect[],
    stackable: boolean = false,
    maxStacks?: number
  ): UpgradeSynergy {
    return {
      id,
      name,
      description,
      requiredUpgrades,
      bonusEffects,
      isActive: false,
      stackable,
      maxStacks,
      currentStacks: 0
    };
  }
}

export { UpgradeManager };

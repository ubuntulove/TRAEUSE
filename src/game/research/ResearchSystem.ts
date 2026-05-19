import { EventEmitter } from '../../core/events/EventEmitter';
import { ResourceManager } from '../resources/ResourceManager';
import { UnitManager } from '../entities/units/UnitManager';
import { BuildingManager } from '../entities/buildings/BuildingManager';
import { AbilityManager } from '../abilities/AbilityManager';
import { Player, PlayerId } from '../players/Player';
import { Vector2 } from '../../core/math/Vector2';
import { Logger } from '../../core/logging/Logger';

export enum ResearchCategory {
    Unit = 'unit',
    Building = 'building',
    Ability = 'ability',
    Upgrade = 'upgrade'
}

export enum TechNodeState {
    Locked = 'locked',
    Available = 'available',
    Researching = 'researching',
    Completed = 'completed',
    Disabled = 'disabled'
}

export enum TechEra {
    Primitive = 'primitive',
    Ancient = 'ancient',
    Medieval = 'medieval',
    Renaissance = 'renaissance',
    Industrial = 'industrial',
    Modern = 'modern',
    Future = 'future'
}

export enum ResearchPriority {
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

export interface TechPrerequisite {
    techId: string;
    techName: string;
    required: boolean;
    researchLevel?: number;
    techEra?: TechEra;
}

export interface TechCost {
    gold: number;
    wood?: number;
    stone?: number;
    iron?: number;
    food?: number;
    energy?: number;
    researchPoints: number;
    customResources?: Record<string, number>;
}

export interface TechEffect {
    type: 'stat_modifier' | 'unlock_unit' | 'unlock_building' | 'unlock_ability' | 'unlock_upgrade' | 'research_speed' | 'resource_bonus';
    target?: string;
    value?: number;
    modifiers?: Record<string, number>;
    affectedCategories?: ResearchCategory[];
}

export interface UnlockableItem {
    id: string;
    name: string;
    type: ResearchCategory;
    description: string;
    icon: string;
    isUnlocked: boolean;
    unlockConditions: TechPrerequisite[];
    cost: TechCost;
    prerequisites: TechPrerequisite[];
}

export interface ResearchSlot {
    id: number;
    techId: string | null;
    startTime: number;
    progress: number;
    isActive: boolean;
    priority: ResearchPriority;
    estimatedCompletion: number;
}

export interface TechNodeData {
    id: string;
    name: string;
    description: string;
    category: ResearchCategory;
    era: TechEra;
    icon: string;
    cost: TechCost;
    researchTime: number;
    prerequisites: TechPrerequisite[];
    effects: TechEffect[];
    unlocks: UnlockableItem[];
    isRepeatable: boolean;
    maxRepeatCount?: number;
    currentRepeatCount: number;
    state: TechNodeState;
    progress: number;
    startResearchTime?: number;
    completionTime?: number;
    bonusResearchTime?: number;
    stateHistory: TechNodeState[];
}

export interface TechTree {
    id: string;
    name: string;
    description: string;
    version: string;
    rootNodes: string[];
    allNodes: Map<string, TechNodeData>;
    categories: Map<ResearchCategory, string[]>;
    eras: Map<TechEra, string[]>;
    dependencies: Map<string, string[]>;
}

export interface TechCompatibilityInfo {
    modId: string;
    modName: string;
    version: string;
    compatibleNodes: string[];
    incompatibleNodes: string[];
    conflicts: TechConflict[];
    dependencies: string[];
}

export interface TechConflict {
    techId1: string;
    techId2: string;
    conflictType: 'mutually_exclusive' | 'requires_other' | 'replaces';
    resolution: 'disable_first' | 'disable_second' | 'merge';
}

export interface TechBonus {
    id: string;
    name: string;
    description: string;
    multiplier: number;
    duration: number;
    startTime: number;
    affectedTechs: string[];
    stackable: boolean;
    currentStacks: number;
}

export interface ResearchProgressCallback {
    onProgress: (techId: string, progress: number, elapsed: number, remaining: number) => void;
    onComplete: (techId: string, tech: TechNodeData) => void;
    onFail: (techId: string, reason: string) => void;
    onCancel: (techId: string) => void;
    onPause: (techId: string) => void;
    onResume: (techId: string) => void;
}

export interface ResearchEvent {
    type: 'start' | 'progress' | 'complete' | 'fail' | 'cancel' | 'pause' | 'resume' | 'unlock';
    techId: string;
    timestamp: number;
    playerId: PlayerId;
    data?: Record<string, any>;
}

export interface ResearchQueueItem {
    techId: string;
    position: number;
    priority: ResearchPriority;
    queuedTime: number;
}

export interface OptimalPathResult {
    targetTechId: string;
    path: string[];
    totalCost: TechCost;
    totalTime: number;
    alternativePaths: OptimalPathResult[];
}

export interface TechTreeViewState {
    centerNodeId: string;
    zoom: number;
    visibleCategories: ResearchCategory[];
    visibleEras: TechEra[];
    showCompleted: boolean;
    showAvailable: boolean;
    showLocked: boolean;
    filterText: string;
}

export class TechCostCalculator {
    private baseCosts: Map<string, TechCost> = new Map();
    private costMultipliers: Map<string, number> = new Map();
    private resourceEfficiency: Map<string, number> = new Map();

    constructor() {
        this.initializeBaseCosts();
    }

    private initializeBaseCosts(): void {
        this.resourceEfficiency.set('gold', 1.0);
        this.resourceEfficiency.set('wood', 1.0);
        this.resourceEfficiency.set('stone', 1.2);
        this.resourceEfficiency.set('iron', 1.5);
        this.resourceEfficiency.set('food', 0.8);
        this.resourceEfficiency.set('energy', 1.3);
        this.resourceEfficiency.set('researchPoints', 1.0);
    }

    public calculateCost(techId: string, repeatCount: number = 0): TechCost {
        const baseCost = this.baseCosts.get(techId);
        if (!baseCost) {
            return this.getDefaultCost();
        }

        const multiplier = this.costMultipliers.get(techId) || 1.0;
        const repeatMultiplier = repeatCount > 0 ? 1 + (repeatCount * 0.2) : 1.0;
        const finalMultiplier = multiplier * repeatMultiplier;

        return {
            gold: Math.floor(baseCost.gold * finalMultiplier),
            wood: baseCost.wood ? Math.floor(baseCost.wood * finalMultiplier) : undefined,
            stone: baseCost.stone ? Math.floor(baseCost.stone * finalMultiplier) : undefined,
            iron: baseCost.iron ? Math.floor(baseCost.iron * finalMultiplier) : undefined,
            food: baseCost.food ? Math.floor(baseCost.food * finalMultiplier) : undefined,
            energy: baseCost.energy ? Math.floor(baseCost.energy * finalMultiplier) : undefined,
            researchPoints: Math.floor(baseCost.researchPoints * finalMultiplier),
            customResources: baseCost.customResources ? this.scaleCustomResources(baseCost.customResources, finalMultiplier) : undefined
        };
    }

    private scaleCustomResources(resources: Record<string, number>, multiplier: number): Record<string, number> {
        const scaled: Record<string, number> = {};
        for (const [key, value] of Object.entries(resources)) {
            scaled[key] = Math.floor(value * multiplier);
        }
        return scaled;
    }

    private getDefaultCost(): TechCost {
        return {
            gold: 100,
            researchPoints: 50
        };
    }

    public registerBaseCost(techId: string, cost: TechCost): void {
        this.baseCosts.set(techId, cost);
    }

    public setCostMultiplier(techId: string, multiplier: number): void {
        this.costMultipliers.set(techId, multiplier);
    }

    public getEffectiveCost(cost: TechCost, playerBonuses: Record<string, number>): TechCost {
        const efficiency = this.resourceEfficiency;
        return {
            gold: Math.floor(cost.gold * (playerBonuses['gold'] || 1)),
            wood: cost.wood ? Math.floor(cost.wood * (playerBonuses['wood'] || 1)) : undefined,
            stone: cost.stone ? Math.floor(cost.stone * (playerBonuses['stone'] || 1)) : undefined,
            iron: cost.iron ? Math.floor(cost.iron * (playerBonuses['iron'] || 1)) : undefined,
            food: cost.food ? Math.floor(cost.food * (playerBonuses['food'] || 1)) : undefined,
            energy: cost.energy ? Math.floor(cost.energy * (playerBonuses['energy'] || 1)) : undefined,
            researchPoints: Math.floor(cost.researchPoints * (playerBonuses['researchPoints'] || 1)),
            customResources: cost.customResources ? this.applyBonusesToCustomResources(cost.customResources, playerBonuses) : undefined
        };
    }

    private applyBonusesToCustomResources(resources: Record<string, number>, bonuses: Record<string, number>): Record<string, number> {
        const result: Record<string, number> = {};
        for (const [key, value] of Object.entries(resources)) {
            result[key] = Math.floor(value * (bonuses[key] || 1));
        }
        return result;
    }

    public calculateTotalCost(techIds: string[]): TechCost {
        const total: TechCost = {
            gold: 0,
            researchPoints: 0
        };

        for (const techId of techIds) {
            const cost = this.calculateCost(techId);
            total.gold += cost.gold;
            total.wood = (total.wood || 0) + (cost.wood || 0);
            total.stone = (total.stone || 0) + (cost.stone || 0);
            total.iron = (total.iron || 0) + (cost.iron || 0);
            total.food = (total.food || 0) + (cost.food || 0);
            total.energy = (total.energy || 0) + (cost.energy || 0);
            total.researchPoints += cost.researchPoints;
        }

        if (total.wood === 0) delete total.wood;
        if (total.stone === 0) delete total.stone;
        if (total.iron === 0) delete total.iron;
        if (total.food === 0) delete total.food;
        if (total.energy === 0) delete total.energy;

        return total;
    }

    public canAfford(cost: TechCost, availableResources: Record<string, number>): boolean {
        if (availableResources['gold'] < cost.gold) return false;
        if (availableResources['researchPoints'] < cost.researchPoints) return false;
        if (cost.wood && availableResources['wood'] < cost.wood) return false;
        if (cost.stone && availableResources['stone'] < cost.stone) return false;
        if (cost.iron && availableResources['iron'] < cost.iron) return false;
        if (cost.food && availableResources['food'] < cost.food) return false;
        if (cost.energy && availableResources['energy'] < cost.energy) return false;

        if (cost.customResources) {
            for (const [key, value] of Object.entries(cost.customResources)) {
                if ((availableResources[key] || 0) < value) return false;
            }
        }

        return true;
    }

    public getRefundAmount(techId: string, completionPercentage: number): TechCost {
        const fullCost = this.calculateCost(techId);
        const refundPercentage = Math.max(0, 1 - completionPercentage);

        return {
            gold: Math.floor(fullCost.gold * refundPercentage * 0.5),
            wood: fullCost.wood ? Math.floor(fullCost.wood * refundPercentage * 0.5) : undefined,
            stone: fullCost.stone ? Math.floor(fullCost.stone * refundPercentage * 0.5) : undefined,
            iron: fullCost.iron ? Math.floor(fullCost.iron * refundPercentage * 0.5) : undefined,
            food: fullCost.food ? Math.floor(fullCost.food * refundPercentage * 0.5) : undefined,
            energy: fullCost.energy ? Math.floor(fullCost.energy * refundPercentage * 0.5) : undefined,
            researchPoints: Math.floor(fullCost.researchPoints * refundPercentage * 0.5),
            customResources: fullCost.customResources ? this.calculateCustomResourceRefund(fullCost.customResources, refundPercentage) : undefined
        };
    }

    private calculateCustomResourceRefund(resources: Record<string, number>, percentage: number): Record<string, number> {
        const refund: Record<string, number> = {};
        for (const [key, value] of Object.entries(resources)) {
            refund[key] = Math.floor(value * percentage * 0.5);
        }
        return refund;
    }
}

export class PrerequisiteChecker {
    private playerProgress: Map<PlayerId, Set<string>> = new Map();
    private pendingTechs: Map<PlayerId, Set<string>> = new Map();

    constructor() {}

    public setPlayerProgress(playerId: PlayerId, completedTechs: string[]): void {
        this.playerProgress.set(playerId, new Set(completedTechs));
    }

    public addCompletedTech(playerId: PlayerId, techId: string): void {
        const progress = this.playerProgress.get(playerId) || new Set();
        progress.add(techId);
        this.playerProgress.set(playerId, progress);
    }

    public addPendingTech(playerId: PlayerId, techId: string): void {
        const pending = this.pendingTechs.get(playerId) || new Set();
        pending.add(techId);
        this.pendingTechs.set(playerId, pending);
    }

    public removePendingTech(playerId: PlayerId, techId: string): void {
        const pending = this.pendingTechs.get(playerId);
        if (pending) {
            pending.delete(techId);
        }
    }

    public checkPrerequisites(playerId: PlayerId, tech: TechNodeData, techTree: TechTree): PrerequisiteCheckResult {
        const result: PrerequisiteCheckResult = {
            isValid: true,
            missingPrerequisites: [],
            satisfiedPrerequisites: [],
            failedChecks: []
        };

        for (const prereq of tech.prerequisites) {
            if (prereq.required) {
                const isCompleted = this.isTechCompleted(playerId, prereq.techId);
                const isPending = this.isTechPending(playerId, prereq.techId);

                if (isCompleted) {
                    result.satisfiedPrerequisites.push(prereq);
                } else if (isPending) {
                    result.failedChecks.push({
                        type: 'pending',
                        techId: prereq.techId,
                        message: `${prereq.techName} is currently being researched`
                    });
                } else {
                    result.missingPrerequisites.push(prereq);
                    result.failedChecks.push({
                        type: 'missing',
                        techId: prereq.techId,
                        message: `${prereq.techName} has not been researched`
                    });
                    result.isValid = false;
                }

                if (prereq.researchLevel !== undefined) {
                    const currentLevel = this.getTechLevel(playerId, prereq.techId);
                    if (currentLevel < prereq.researchLevel) {
                        result.failedChecks.push({
                            type: 'insufficient_level',
                            techId: prereq.techId,
                            message: `Requires ${prereq.techName} level ${prereq.researchLevel}`
                        });
                        result.isValid = false;
                    }
                }

                if (prereq.techEra !== undefined) {
                    const node = techTree.allNodes.get(prereq.techId);
                    if (node && this.compareEras(node.era, prereq.techEra) < 0) {
                        result.failedChecks.push({
                            type: 'era_mismatch',
                            techId: prereq.techId,
                            message: `Requires ${prereq.techName} from ${prereq.techEra} era`
                        });
                        result.isValid = false;
                    }
                }
            }
        }

        return result;
    }

    private isTechCompleted(playerId: PlayerId, techId: string): boolean {
        const progress = this.playerProgress.get(playerId);
        return progress ? progress.has(techId) : false;
    }

    private isTechPending(playerId: PlayerId, techId: string): boolean {
        const pending = this.pendingTechs.get(playerId);
        return pending ? pending.has(techId) : false;
    }

    private getTechLevel(playerId: PlayerId, techId: string): number {
        return this.isTechCompleted(playerId, techId) ? 1 : 0;
    }

    private compareEras(era1: TechEra, era2: TechEra): number {
        const eraOrder = [
            TechEra.Primitive,
            TechEra.Ancient,
            TechEra.Medieval,
            TechEra.Renaissance,
            TechEra.Industrial,
            TechEra.Modern,
            TechEra.Future
        ];
        return eraOrder.indexOf(era1) - eraOrder.indexOf(era2);
    }

    public validateTechTree(techTree: TechTree): TechTreeValidationResult {
        const result: TechTreeValidationResult = {
            isValid: true,
            circularDependencies: [],
            orphanNodes: [],
            missingDependencies: []
        };

        const visited = new Set<string>();
        const recursionStack = new Set<string>();

        for (const nodeId of techTree.allNodes.keys()) {
            if (this.hasCircularDependency(nodeId, techTree, visited, recursionStack)) {
                result.circularDependencies.push(nodeId);
                result.isValid = false;
            }
        }

        for (const [nodeId, node] of techTree.allNodes) {
            if (node.prerequisites.length === 0 && !techTree.rootNodes.includes(nodeId)) {
                result.orphanNodes.push(nodeId);
            }

            for (const prereq of node.prerequisites) {
                if (!techTree.allNodes.has(prereq.techId)) {
                    result.missingDependencies.push({
                        nodeId,
                        missingTechId: prereq.techId
                    });
                    result.isValid = false;
                }
            }
        }

        return result;
    }

    private hasCircularDependency(
        nodeId: string,
        techTree: TechTree,
        visited: Set<string>,
        recursionStack: Set<string>
    ): boolean {
        if (recursionStack.has(nodeId)) return true;
        if (visited.has(nodeId)) return false;

        visited.add(nodeId);
        recursionStack.add(nodeId);

        const dependencies = techTree.dependencies.get(nodeId) || [];
        for (const depId of dependencies) {
            if (this.hasCircularDependency(depId, techTree, visited, recursionStack)) {
                return true;
            }
        }

        recursionStack.delete(nodeId);
        return false;
    }

    public getMissingDependencies(playerId: PlayerId, tech: TechNodeData): TechPrerequisite[] {
        const missing: TechPrerequisite[] = [];

        for (const prereq of tech.prerequisites) {
            if (prereq.required && !this.isTechCompleted(playerId, prereq.techId)) {
                missing.push(prereq);
            }
        }

        return missing;
    }
}

export interface PrerequisiteCheckResult {
    isValid: boolean;
    missingPrerequisites: TechPrerequisite[];
    satisfiedPrerequisites: TechPrerequisite[];
    failedChecks: TechCheckFailure[];
}

export interface TechCheckFailure {
    type: 'pending' | 'missing' | 'insufficient_level' | 'era_mismatch';
    techId: string;
    message: string;
}

export interface TechTreeValidationResult {
    isValid: boolean;
    circularDependencies: string[];
    orphanNodes: string[];
    missingDependencies: { nodeId: string; missingTechId: string }[];
}

export class TechUnlocking {
    private unlockedUnits: Map<PlayerId, Set<string>> = new Map();
    private unlockedBuildings: Map<PlayerId, Set<string>> = new Map();
    private unlockedAbilities: Map<PlayerId, Set<string>> = new Map();
    private unlockedUpgrades: Map<PlayerId, Set<string>> = new Map();
    private unitManager?: UnitManager;
    private buildingManager?: BuildingManager;
    private abilityManager?: AbilityManager;

    constructor() {}

    public setManagers(unitManager: UnitManager, buildingManager: BuildingManager, abilityManager: AbilityManager): void {
        this.unitManager = unitManager;
        this.buildingManager = buildingManager;
        this.abilityManager = abilityManager;
    }

    public unlockTech(playerId: PlayerId, tech: TechNodeData): UnlockResult {
        const result: UnlockResult = {
            success: true,
            unlockedUnits: [],
            unlockedBuildings: [],
            unlockedAbilities: [],
            unlockedUpgrades: [],
            failedUnlocks: []
        };

        for (const unlock of tech.unlocks) {
            const success = this.performUnlock(playerId, unlock);
            if (success) {
                switch (unlock.type) {
                    case ResearchCategory.Unit:
                        result.unlockedUnits.push(unlock);
                        break;
                    case ResearchCategory.Building:
                        result.unlockedBuildings.push(unlock);
                        break;
                    case ResearchCategory.Ability:
                        result.unlockedAbilities.push(unlock);
                        break;
                    case ResearchCategory.Upgrade:
                        result.unlockedUpgrades.push(unlock);
                        break;
                }
            } else {
                result.failedUnlocks.push({
                    item: unlock,
                    reason: 'Unlock failed - system not available'
                });
            }
        }

        return result;
    }

    private performUnlock(playerId: PlayerId, item: UnlockableItem): boolean {
        try {
            switch (item.type) {
                case ResearchCategory.Unit:
                    return this.unlockUnit(playerId, item.id);
                case ResearchCategory.Building:
                    return this.unlockBuilding(playerId, item.id);
                case ResearchCategory.Ability:
                    return this.unlockAbility(playerId, item.id);
                case ResearchCategory.Upgrade:
                    return this.unlockUpgrade(playerId, item.id);
                default:
                    return false;
            }
        } catch (error) {
            Logger.error(`Failed to unlock ${item.id}: ${error}`);
            return false;
        }
    }

    private unlockUnit(playerId: PlayerId, unitId: string): boolean {
        const units = this.unlockedUnits.get(playerId) || new Set();
        units.add(unitId);
        this.unlockedUnits.set(playerId, units);

        if (this.unitManager) {
            this.unitManager.unlockUnitType(playerId, unitId);
        }

        return true;
    }

    private unlockBuilding(playerId: PlayerId, buildingId: string): boolean {
        const buildings = this.unlockedBuildings.get(playerId) || new Set();
        buildings.add(buildingId);
        this.unlockedBuildings.set(playerId, buildings);

        if (this.buildingManager) {
            this.buildingManager.unlockBuildingType(playerId, buildingId);
        }

        return true;
    }

    private unlockAbility(playerId: PlayerId, abilityId: string): boolean {
        const abilities = this.unlockedAbilities.get(playerId) || new Set();
        abilities.add(abilityId);
        this.unlockedAbilities.set(playerId, abilities);

        if (this.abilityManager) {
            this.abilityManager.unlockAbility(playerId, abilityId);
        }

        return true;
    }

    private unlockUpgrade(playerId: PlayerId, upgradeId: string): boolean {
        const upgrades = this.unlockedUpgrades.get(playerId) || new Set();
        upgrades.add(upgradeId);
        this.unlockedUpgrades.set(playerId, upgrades);

        return true;
    }

    public isUnitUnlocked(playerId: PlayerId, unitId: string): boolean {
        const units = this.unlockedUnits.get(playerId);
        return units ? units.has(unitId) : false;
    }

    public isBuildingUnlocked(playerId: PlayerId, buildingId: string): boolean {
        const buildings = this.unlockedBuildings.get(playerId);
        return buildings ? buildings.has(buildingId) : false;
    }

    public isAbilityUnlocked(playerId: PlayerId, abilityId: string): boolean {
        const abilities = this.unlockedAbilities.get(playerId);
        return abilities ? abilities.has(abilityId) : false;
    }

    public isUpgradeUnlocked(playerId: PlayerId, upgradeId: string): boolean {
        const upgrades = this.unlockedUpgrades.get(playerId);
        return upgrades ? upgrades.has(upgradeId) : false;
    }

    public getUnlockedUnits(playerId: PlayerId): string[] {
        const units = this.unlockedUnits.get(playerId);
        return units ? Array.from(units) : [];
    }

    public getUnlockedBuildings(playerId: PlayerId): string[] {
        const buildings = this.unlockedBuildings.get(playerId);
        return buildings ? Array.from(buildings) : [];
    }

    public getUnlockedAbilities(playerId: PlayerId): string[] {
        const abilities = this.unlockedAbilities.get(playerId);
        return abilities ? Array.from(abilities) : [];
    }

    public getUnlockedUpgrades(playerId: PlayerId): string[] {
        const upgrades = this.unlockedUpgrades.get(playerId);
        return upgrades ? Array.from(upgrades) : [];
    }

    public applyTechEffects(playerId: PlayerId, tech: TechNodeData): void {
        for (const effect of tech.effects) {
            this.applyEffect(playerId, effect);
        }
    }

    private applyEffect(playerId: PlayerId, effect: TechEffect): void {
        switch (effect.type) {
            case 'stat_modifier':
                this.applyStatModifier(playerId, effect);
                break;
            case 'unlock_unit':
                if (effect.target) this.unlockUnit(playerId, effect.target);
                break;
            case 'unlock_building':
                if (effect.target) this.unlockBuilding(playerId, effect.target);
                break;
            case 'unlock_ability':
                if (effect.target) this.unlockAbility(playerId, effect.target);
                break;
            case 'unlock_upgrade':
                if (effect.target) this.unlockUpgrade(playerId, effect.target);
                break;
            case 'research_speed':
                break;
            case 'resource_bonus':
                break;
        }
    }

    private applyStatModifier(playerId: PlayerId, effect: TechEffect): void {
        if (effect.affectedCategories) {
            for (const category of effect.affectedCategories) {
            }
        }
    }

    public resetUnlocks(playerId: PlayerId): void {
        this.unlockedUnits.delete(playerId);
        this.unlockedBuildings.delete(playerId);
        this.unlockedAbilities.delete(playerId);
        this.unlockedUpgrades.delete(playerId);
    }

    public getLockedItems(playerId: PlayerId, allItems: UnlockableItem[]): UnlockableItem[] {
        return allItems.filter(item => {
            switch (item.type) {
                case ResearchCategory.Unit:
                    return !this.isUnitUnlocked(playerId, item.id);
                case ResearchCategory.Building:
                    return !this.isBuildingUnlocked(playerId, item.id);
                case ResearchCategory.Ability:
                    return !this.isAbilityUnlocked(playerId, item.id);
                case ResearchCategory.Upgrade:
                    return !this.isUpgradeUnlocked(playerId, item.id);
                default:
                    return true;
            }
        });
    }
}

export interface UnlockResult {
    success: boolean;
    unlockedUnits: UnlockableItem[];
    unlockedBuildings: UnlockableItem[];
    unlockedAbilities: UnlockableItem[];
    unlockedUpgrades: UnlockableItem[];
    failedUnlocks: { item: UnlockableItem; reason: string }[];
}

export class ResearchQueue {
    private queues: Map<PlayerId, ResearchQueueItem[]> = new Map();
    private maxQueueSize: number = 5;

    constructor(maxQueueSize: number = 5) {
        this.maxQueueSize = maxQueueSize;
    }

    public enqueue(playerId: PlayerId, techId: string, priority: ResearchPriority = ResearchPriority.Normal): boolean {
        const queue = this.getOrCreateQueue(playerId);

        if (queue.length >= this.maxQueueSize) {
            return false;
        }

        if (this.isInQueue(playerId, techId)) {
            return false;
        }

        const item: ResearchQueueItem = {
            techId,
            position: queue.length,
            priority,
            queuedTime: Date.now()
        };

        queue.push(item);
        this.sortQueue(playerId);

        return true;
    }

    public dequeue(playerId: PlayerId): ResearchQueueItem | null {
        const queue = this.getOrCreateQueue(playerId);
        return queue.shift() || null;
    }

    public remove(playerId: PlayerId, techId: string): boolean {
        const queue = this.queues.get(playerId);
        if (!queue) return false;

        const index = queue.findIndex(item => item.techId === techId);
        if (index === -1) return false;

        queue.splice(index, 1);
        this.reindexQueue(playerId);

        return true;
    }

    public getNext(playerId: PlayerId): ResearchQueueItem | null {
        const queue = this.queues.get(playerId);
        return queue && queue.length > 0 ? queue[0] : null;
    }

    public getQueue(playerId: PlayerId): ResearchQueueItem[] {
        return this.getOrCreateQueue(playerId);
    }

    public getQueuePosition(playerId: PlayerId, techId: string): number {
        const queue = this.queues.get(playerId);
        if (!queue) return -1;

        const index = queue.findIndex(item => item.techId === techId);
        return index;
    }

    public isInQueue(playerId: PlayerId, techId: string): boolean {
        const queue = this.queues.get(playerId);
        return queue ? queue.some(item => item.techId === techId) : false;
    }

    public clearQueue(playerId: PlayerId): void {
        this.queues.delete(playerId);
    }

    public updatePriority(playerId: PlayerId, techId: string, newPriority: ResearchPriority): boolean {
        const queue = this.queues.get(playerId);
        if (!queue) return false;

        const item = queue.find(i => i.techId === techId);
        if (!item) return false;

        item.priority = newPriority;
        this.sortQueue(playerId);

        return true;
    }

    private getOrCreateQueue(playerId: PlayerId): ResearchQueueItem[] {
        let queue = this.queues.get(playerId);
        if (!queue) {
            queue = [];
            this.queues.set(playerId, queue);
        }
        return queue;
    }

    private sortQueue(playerId: PlayerId): void {
        const queue = this.queues.get(playerId);
        if (!queue) return;

        queue.sort((a, b) => {
            if (a.priority !== b.priority) {
                return b.priority - a.priority;
            }
            return a.queuedTime - b.queuedTime;
        });

        this.reindexQueue(playerId);
    }

    private reindexQueue(playerId: PlayerId): void {
        const queue = this.queues.get(playerId);
        if (!queue) return;

        queue.forEach((item, index) => {
            item.position = index;
        });
    }

    public getQueueSize(playerId: PlayerId): number {
        const queue = this.queues.get(playerId);
        return queue ? queue.length : 0;
    }

    public isQueueFull(playerId: PlayerId): boolean {
        return this.getQueueSize(playerId) >= this.maxQueueSize;
    }

    public setMaxQueueSize(size: number): void {
        this.maxQueueSize = Math.max(1, size);
    }
}

export class ResearchProgress {
    private progressCallbacks: Map<string, ResearchProgressCallback[]> = new Map();
    private progressData: Map<string, { startTime: number; lastUpdate: number; progress: number }> = new Map();
    private updateInterval: number = 100;

    constructor() {}

    public startTracking(techId: string, startTime: number): void {
        this.progressData.set(techId, {
            startTime,
            lastUpdate: startTime,
            progress: 0
        });
    }

    public updateProgress(techId: string, progress: number, currentTime: number): void {
        const data = this.progressData.get(techId);
        if (!data) return;

        data.progress = Math.min(100, Math.max(0, progress));
        data.lastUpdate = currentTime;

        this.notifyProgressCallbacks(techId, data.progress);
    }

    public stopTracking(techId: string): void {
        this.progressData.delete(techId);
    }

    public registerCallback(techId: string, callback: ResearchProgressCallback): void {
        const callbacks = this.progressCallbacks.get(techId) || [];
        callbacks.push(callback);
        this.progressCallbacks.set(techId, callbacks);
    }

    public unregisterCallback(techId: string, callback: ResearchProgressCallback): void {
        const callbacks = this.progressCallbacks.get(techId);
        if (callbacks) {
            const index = callbacks.indexOf(callback);
            if (index !== -1) {
                callbacks.splice(index, 1);
            }
        }
    }

    private notifyProgressCallbacks(techId: string, progress: number): void {
        const callbacks = this.progressCallbacks.get(techId);
        if (!callbacks) return;

        const data = this.progressData.get(techId);
        if (!data) return;

        const elapsed = data.lastUpdate - data.startTime;
        const remaining = (elapsed / progress) * (100 - progress);

        for (const callback of callbacks) {
            if (callback.onProgress) {
                callback.onProgress(techId, progress, elapsed, remaining);
            }
        }
    }

    public notifyCompletion(techId: string, tech: TechNodeData): void {
        const callbacks = this.progressCallbacks.get(techId);
        if (!callbacks) return;

        for (const callback of callbacks) {
            if (callback.onComplete) {
                callback.onComplete(techId, tech);
            }
        }
    }

    public notifyFailure(techId: string, reason: string): void {
        const callbacks = this.progressCallbacks.get(techId);
        if (!callbacks) return;

        for (const callback of callbacks) {
            if (callback.onFail) {
                callback.onFail(techId, reason);
            }
        }
    }

    public notifyCancellation(techId: string): void {
        const callbacks = this.progressCallbacks.get(techId);
        if (!callbacks) return;

        for (const callback of callbacks) {
            if (callback.onCancel) {
                callback.onCancel(techId);
            }
        }
    }

    public notifyPause(techId: string): void {
        const callbacks = this.progressCallbacks.get(techId);
        if (!callbacks) return;

        for (const callback of callbacks) {
            if (callback.onPause) {
                callback.onPause(techId);
            }
        }
    }

    public notifyResume(techId: string): void {
        const callbacks = this.progressCallbacks.get(techId);
        if (!callbacks) return;

        for (const callback of callbacks) {
            if (callback.onResume) {
                callback.onResume(techId);
            }
        }
    }

    public getProgress(techId: string): number {
        const data = this.progressData.get(techId);
        return data ? data.progress : 0;
    }

    public calculateTimeRemaining(techId: string, totalTime: number): number {
        const data = this.progressData.get(techId);
        if (!data) return totalTime;

        if (data.progress === 0) return totalTime;

        const elapsed = data.lastUpdate - data.startTime;
        const progressRate = data.progress / elapsed;
        const remaining = (100 - data.progress) / progressRate;

        return remaining;
    }

    public setUpdateInterval(interval: number): void {
        this.updateInterval = Math.max(10, interval);
    }

    public getUpdateInterval(): number {
        return this.updateInterval;
    }
}

export class TechCompatibility {
    private registeredMods: Map<string, TechCompatibilityInfo> = new Map();
    private compatibilityCache: Map<string, boolean> = new Map();

    constructor() {}

    public registerMod(modInfo: TechCompatibilityInfo): void {
        this.registeredMods.set(modInfo.modId, modInfo);
        this.invalidateCache();
    }

    public unregisterMod(modId: string): void {
        this.registeredMods.delete(modId);
        this.invalidateCache();
    }

    public checkCompatibility(modId1: string, modId2: string): boolean {
        const cacheKey = `${modId1}:${modId2}`;
        const cached = this.compatibilityCache.get(cacheKey);
        if (cached !== undefined) return cached;

        const mod1 = this.registeredMods.get(modId1);
        const mod2 = this.registeredMods.get(modId2);

        if (!mod1 || !mod2) return true;

        const hasConflict = mod1.conflicts.some(c =>
            (c.techId1 === modId1 && c.techId2 === modId2) ||
            (c.techId1 === modId2 && c.techId2 === modId1)
        );

        const isCompatible = !hasConflict;
        this.compatibilityCache.set(cacheKey, isCompatible);

        return isCompatible;
    }

    public getConflicts(modId: string): TechConflict[] {
        const mod = this.registeredMods.get(modId);
        return mod ? mod.conflicts : [];
    }

    public resolveConflict(conflict: TechConflict): string {
        switch (conflict.resolution) {
            case 'disable_first':
                return conflict.techId1;
            case 'disable_second':
                return conflict.techId2;
            case 'merge':
                return `${conflict.techId1}+${conflict.techId2}`;
            default:
                return conflict.techId1;
        }
    }

    public getModDependencies(modId: string): string[] {
        const mod = this.registeredMods.get(modId);
        return mod ? mod.dependencies : [];
    }

    public checkDependencies(modId: string, activeMods: string[]): boolean {
        const dependencies = this.getModDependencies(modId);
        return dependencies.every(dep => activeMods.includes(dep));
    }

    public getCompatibleNodes(modId: string): string[] {
        const mod = this.registeredMods.get(modId);
        return mod ? mod.compatibleNodes : [];
    }

    public getIncompatibleNodes(modId: string): string[] {
        const mod = this.registeredMods.get(modId);
        return mod ? mod.incompatibleNodes : [];
    }

    public filterTechTreeByCompatibility(techIds: string[], activeMods: string[]): string[] {
        return techIds.filter(techId => {
            for (const modId of activeMods) {
                const mod = this.registeredMods.get(modId);
                if (mod && mod.incompatibleNodes.includes(techId)) {
                    return false;
                }
            }
            return true;
        });
    }

    private invalidateCache(): void {
        this.compatibilityCache.clear();
    }

    public getAllRegisteredMods(): TechCompatibilityInfo[] {
        return Array.from(this.registeredMods.values());
    }
}

export class TechCategory {
    private categories: Map<ResearchCategory, CategoryInfo> = new Map();
    private nodesByCategory: Map<ResearchCategory, Set<string>> = new Map();

    constructor() {
        this.initializeDefaultCategories();
    }

    private initializeDefaultCategories(): void {
        this.categories.set(ResearchCategory.Unit, {
            name: 'Units',
            description: 'Unit types and variations',
            icon: 'unit_icon',
            color: '#4A90D9'
        });

        this.categories.set(ResearchCategory.Building, {
            name: 'Buildings',
            description: 'Structures and facilities',
            icon: 'building_icon',
            color: '#D9944A'
        });

        this.categories.set(ResearchCategory.Ability, {
            name: 'Abilities',
            description: 'Special abilities and powers',
            icon: 'ability_icon',
            color: '#9B4ADE'
        });

        this.categories.set(ResearchCategory.Upgrade, {
            name: 'Upgrades',
            description: 'Enhancements and improvements',
            icon: 'upgrade_icon',
            color: '#4ADE9B'
        });
    }

    public getCategoryInfo(category: ResearchCategory): CategoryInfo | undefined {
        return this.categories.get(category);
    }

    public addNodeToCategory(category: ResearchCategory, nodeId: string): void {
        let nodes = this.nodesByCategory.get(category);
        if (!nodes) {
            nodes = new Set();
            this.nodesByCategory.set(category, nodes);
        }
        nodes.add(nodeId);
    }

    public removeNodeFromCategory(category: ResearchCategory, nodeId: string): void {
        const nodes = this.nodesByCategory.get(category);
        if (nodes) {
            nodes.delete(nodeId);
        }
    }

    public getNodesInCategory(category: ResearchCategory): string[] {
        const nodes = this.nodesByCategory.get(category);
        return nodes ? Array.from(nodes) : [];
    }

    public getAllCategories(): ResearchCategory[] {
        return Array.from(this.categories.keys());
    }

    public registerCategory(category: ResearchCategory, info: CategoryInfo): void {
        this.categories.set(category, info);
    }

    public getCategoryColor(category: ResearchCategory): string {
        const info = this.categories.get(category);
        return info ? info.color : '#FFFFFF';
    }

    public getCategoryIcon(category: ResearchCategory): string {
        const info = this.categories.get(category);
        return info ? info.icon : '';
    }
}

export interface CategoryInfo {
    name: string;
    description: string;
    icon: string;
    color: string;
}

export class TechEraSystem {
    private eraMultipliers: Map<TechEra, EraInfo> = new Map();

    constructor() {
        this.initializeEras();
    }

    private initializeEras(): void {
        this.eraMultipliers.set(TechEra.Primitive, {
            name: 'Primitive',
            researchSpeedMultiplier: 0.5,
            costMultiplier: 0.5,
            unlockLevel: 0
        });

        this.eraMultipliers.set(TechEra.Ancient, {
            name: 'Ancient',
            researchSpeedMultiplier: 0.7,
            costMultiplier: 0.7,
            unlockLevel: 1
        });

        this.eraMultipliers.set(TechEra.Medieval, {
            name: 'Medieval',
            researchSpeedMultiplier: 1.0,
            costMultiplier: 1.0,
            unlockLevel: 2
        });

        this.eraMultipliers.set(TechEra.Renaissance, {
            name: 'Renaissance',
            researchSpeedMultiplier: 1.2,
            costMultiplier: 1.2,
            unlockLevel: 3
        });

        this.eraMultipliers.set(TechEra.Industrial, {
            name: 'Industrial',
            researchSpeedMultiplier: 1.5,
            costMultiplier: 1.5,
            unlockLevel: 4
        });

        this.eraMultipliers.set(TechEra.Modern, {
            name: 'Modern',
            researchSpeedMultiplier: 2.0,
            costMultiplier: 2.0,
            unlockLevel: 5
        });

        this.eraMultipliers.set(TechEra.Future, {
            name: 'Future',
            researchSpeedMultiplier: 3.0,
            costMultiplier: 3.0,
            unlockLevel: 6
        });
    }

    public getEraInfo(era: TechEra): EraInfo | undefined {
        return this.eraMultipliers.get(era);
    }

    public calculateResearchTime(baseTime: number, era: TechEra): number {
        const info = this.eraMultipliers.get(era);
        return baseTime * (info?.researchSpeedMultiplier || 1.0);
    }

    public calculateCost(baseCost: TechCost, era: TechEra): TechCost {
        const info = this.eraMultipliers.get(era);
        const multiplier = info?.costMultiplier || 1.0;

        return {
            gold: Math.floor(baseCost.gold * multiplier),
            wood: baseCost.wood ? Math.floor(baseCost.wood * multiplier) : undefined,
            stone: baseCost.stone ? Math.floor(baseCost.stone * multiplier) : undefined,
            iron: baseCost.iron ? Math.floor(baseCost.iron * multiplier) : undefined,
            food: baseCost.food ? Math.floor(baseCost.food * multiplier) : undefined,
            energy: baseCost.energy ? Math.floor(baseCost.energy * multiplier) : undefined,
            researchPoints: Math.floor(baseCost.researchPoints * multiplier),
            customResources: baseCost.customResources
        };
    }

    public getUnlockedEras(currentLevel: number): TechEra[] {
        const unlocked: TechEra[] = [];
        for (const [era, info] of this.eraMultipliers) {
            if (info.unlockLevel <= currentLevel) {
                unlocked.push(era);
            }
        }
        return unlocked;
    }

    public getEraFromTechCount(techCount: number): TechEra {
        if (techCount < 5) return TechEra.Primitive;
        if (techCount < 15) return TechEra.Ancient;
        if (techCount < 30) return TechEra.Medieval;
        if (techCount < 50) return TechEra.Renaissance;
        if (techCount < 80) return TechEra.Industrial;
        if (techCount < 120) return TechEra.Modern;
        return TechEra.Future;
    }
}

export interface EraInfo {
    name: string;
    researchSpeedMultiplier: number;
    costMultiplier: number;
    unlockLevel: number;
}

export class ResearchEventHandler {
    private eventEmitter: EventEmitter;
    private eventHistory: Map<PlayerId, ResearchEvent[]> = new Map();
    private maxHistorySize: number = 100;

    constructor() {
        this.eventEmitter = new EventEmitter();
    }

    public onResearchStart(callback: (event: ResearchEvent) => void): void {
        this.eventEmitter.on('research:start', callback);
    }

    public onResearchProgress(callback: (event: ResearchEvent) => void): void {
        this.eventEmitter.on('research:progress', callback);
    }

    public onResearchComplete(callback: (event: ResearchEvent) => void): void {
        this.eventEmitter.on('research:complete', callback);
    }

    public onResearchFail(callback: (event: ResearchEvent) => void): void {
        this.eventEmitter.on('research:fail', callback);
    }

    public onResearchCancel(callback: (event: ResearchEvent) => void): void {
        this.eventEmitter.on('research:cancel', callback);
    }

    public onResearchUnlock(callback: (event: ResearchEvent) => void): void {
        this.eventEmitter.on('research:unlock', callback);
    }

    public emitEvent(event: ResearchEvent): void {
        this.recordEvent(event.playerId, event);
        this.eventEmitter.emit(`research:${event.type}`, event);
    }

    private recordEvent(playerId: PlayerId, event: ResearchEvent): void {
        let history = this.eventHistory.get(playerId);
        if (!history) {
            history = [];
            this.eventHistory.set(playerId, history);
        }

        history.push(event);

        if (history.length > this.maxHistorySize) {
            history.shift();
        }
    }

    public getEventHistory(playerId: PlayerId): ResearchEvent[] {
        return this.eventHistory.get(playerId) || [];
    }

    public clearHistory(playerId: PlayerId): void {
        this.eventHistory.delete(playerId);
    }

    public setMaxHistorySize(size: number): void {
        this.maxHistorySize = Math.max(10, size);
    }

    public getRecentEvents(playerId: PlayerId, count: number): ResearchEvent[] {
        const history = this.eventHistory.get(playerId) || [];
        return history.slice(-count);
    }

    public filterEvents(playerId: PlayerId, eventType: string): ResearchEvent[] {
        const history = this.eventHistory.get(playerId) || [];
        return history.filter(e => e.type === eventType);
    }
}

export class ResearchBonus {
    private activeBonuses: Map<PlayerId, TechBonus[]> = new Map();
    private bonusDefinitions: Map<string, BonusDefinition> = new Map();

    constructor() {
        this.initializeDefaultBonuses();
    }

    private initializeDefaultBonuses(): void {
        this.registerBonus({
            id: 'scholar_bonus',
            name: 'Scholar Bonus',
            description: 'Increased research speed',
            baseMultiplier: 1.25,
            duration: 60000
        });

        this.registerBonus({
            id: 'ancient_knowledge',
            name: 'Ancient Knowledge',
            description: 'Reduced research costs',
            baseMultiplier: 0.9,
            duration: 120000
        });

        this.registerBonus({
            id: 'focused_research',
            name: 'Focused Research',
            description: 'Single tech research speed boost',
            baseMultiplier: 1.5,
            duration: 30000
        });
    }

    public registerBonus(definition: BonusDefinition): void {
        this.bonusDefinitions.set(definition.id, definition);
    }

    public applyBonus(playerId: PlayerId, bonusId: string, affectedTechs?: string[]): boolean {
        const definition = this.bonusDefinitions.get(bonusId);
        if (!definition) return false;

        let bonuses = this.activeBonuses.get(playerId) || [];

        const newBonus: TechBonus = {
            id: `${bonusId}_${Date.now()}`,
            name: definition.name,
            description: definition.description,
            multiplier: definition.baseMultiplier,
            duration: definition.duration,
            startTime: Date.now(),
            affectedTechs: affectedTechs || [],
            stackable: definition.stackable,
            currentStacks: 1
        };

        if (!definition.stackable) {
            bonuses = bonuses.filter(b => b.name !== newBonus.name);
        } else {
            const existing = bonuses.find(b => b.name === newBonus.name);
            if (existing) {
                existing.currentStacks++;
                existing.multiplier = Math.min(
                    existing.multiplier * definition.stackingMultiplier,
                    definition.maxStacks * definition.baseMultiplier
                );
                return true;
            }
        }

        bonuses.push(newBonus);
        this.activeBonuses.set(playerId, bonuses);

        return true;
    }

    public removeBonus(playerId: PlayerId, bonusId: string): boolean {
        const bonuses = this.activeBonuses.get(playerId);
        if (!bonuses) return false;

        const index = bonuses.findIndex(b => b.id === bonusId);
        if (index === -1) return false;

        bonuses.splice(index, 1);
        return true;
    }

    public getActiveBonuses(playerId: PlayerId): TechBonus[] {
        return this.activeBonuses.get(playerId) || [];
    }

    public getResearchSpeedBonus(playerId: PlayerId, techId?: string): number {
        const bonuses = this.getActiveBonuses(playerId);
        let totalMultiplier = 1.0;

        for (const bonus of bonuses) {
            if (this.isBonusActive(bonus)) {
                if (bonus.affectedTechs.length === 0 || (techId && bonus.affectedTechs.includes(techId))) {
                    totalMultiplier *= bonus.multiplier;
                }
            }
        }

        return totalMultiplier;
    }

    public getCostBonus(playerId: PlayerId): number {
        const bonuses = this.getActiveBonuses(playerId);
        let totalMultiplier = 1.0;

        for (const bonus of bonuses) {
            if (this.isBonusActive(bonus) && bonus.description.includes('cost')) {
                totalMultiplier *= bonus.multiplier;
            }
        }

        return totalMultiplier;
    }

    private isBonusActive(bonus: TechBonus): boolean {
        const elapsed = Date.now() - bonus.startTime;
        return elapsed < bonus.duration;
    }

    public cleanupExpiredBonuses(playerId: PlayerId): void {
        const bonuses = this.activeBonuses.get(playerId);
        if (!bonuses) return;

        const active = bonuses.filter(b => this.isBonusActive(b));
        this.activeBonuses.set(playerId, active);
    }

    public clearAllBonuses(playerId: PlayerId): void {
        this.activeBonuses.delete(playerId);
    }
}

export interface BonusDefinition {
    id: string;
    name: string;
    description: string;
    baseMultiplier: number;
    duration: number;
    stackable: boolean;
    stackingMultiplier?: number;
    maxStacks?: number;
}

export class TechTreeCalculator {
    private techTree?: TechTree;

    constructor() {}

    public setTechTree(techTree: TechTree): void {
        this.techTree = techTree;
    }

    public findOptimalPath(targetTechId: string, completedTechs: Set<string>): OptimalPathResult | null {
        if (!this.techTree) return null;

        const targetNode = this.techTree.allNodes.get(targetTechId);
        if (!targetNode) return null;

        const path = this.buildPath(targetTechId, completedTechs);
        if (path.length === 0) return null;

        const totalCost = this.calculatePathCost(path);
        const totalTime = this.calculatePathTime(path);

        return {
            targetTechId,
            path,
            totalCost,
            totalTime,
            alternativePaths: this.findAlternativePaths(targetTechId, completedTechs)
        };
    }

    private buildPath(targetTechId: string, completedTechs: Set<string>): string[] {
        if (!this.techTree) return [];

        const path: string[] = [];
        const visited = new Set<string>();

        this.collectPrerequisites(targetTechId, completedTechs, path, visited);

        return path;
    }

    private collectPrerequisites(
        techId: string,
        completedTechs: Set<string>,
        path: string[],
        visited: Set<string>
    ): void {
        if (visited.has(techId)) return;
        visited.add(techId);

        const node = this.techTree!.allNodes.get(techId);
        if (!node) return;

        if (!completedTechs.has(techId)) {
            for (const prereq of node.prerequisites) {
                if (!completedTechs.has(prereq.techId)) {
                    this.collectPrerequisites(prereq.techId, completedTechs, path, visited);
                }
            }
            path.push(techId);
        }
    }

    private calculatePathCost(path: string[]): TechCost {
        const totalCost: TechCost = {
            gold: 0,
            researchPoints: 0
        };

        for (const techId of path) {
            const node = this.techTree!.allNodes.get(techId);
            if (node) {
                totalCost.gold += node.cost.gold;
                totalCost.wood = (totalCost.wood || 0) + (node.cost.wood || 0);
                totalCost.stone = (totalCost.stone || 0) + (node.cost.stone || 0);
                totalCost.iron = (totalCost.iron || 0) + (node.cost.iron || 0);
                totalCost.food = (totalCost.food || 0) + (node.cost.food || 0);
                totalCost.energy = (totalCost.energy || 0) + (node.cost.energy || 0);
                totalCost.researchPoints += node.cost.researchPoints;
            }
        }

        return totalCost;
    }

    private calculatePathTime(path: string[]): number {
        let totalTime = 0;

        for (const techId of path) {
            const node = this.techTree!.allNodes.get(techId);
            if (node) {
                totalTime += node.researchTime;
            }
        }

        return totalTime;
    }

    private findAlternativePaths(targetTechId: string, completedTechs: Set<string>): OptimalPathResult[] {
        return [];
    }

    public calculateAllAvailableTechs(
        completedTechs: Set<string>,
        availableResources: Record<string, number>
    ): string[] {
        if (!this.techTree) return [];

        const available: string[] = [];

        for (const [techId, node] of this.techTree.allNodes) {
            if (completedTechs.has(techId)) continue;

            const canResearch = this.canResearchTech(techId, completedTechs, availableResources);
            if (canResearch) {
                available.push(techId);
            }
        }

        return available;
    }

    private canResearchTech(
        techId: string,
        completedTechs: Set<string>,
        availableResources: Record<string, number>
    ): boolean {
        const node = this.techTree!.allNodes.get(techId);
        if (!node) return false;

        for (const prereq of node.prerequisites) {
            if (prereq.required && !completedTechs.has(prereq.techId)) {
                return false;
            }
        }

        if (availableResources['gold'] < node.cost.gold) return false;
        if (availableResources['researchPoints'] < node.cost.researchPoints) return false;

        return true;
    }

    public findCheapestPath(targetTechId: string, completedTechs: Set<string>): string[] {
        return this.buildPath(targetTechId, completedTechs);
    }

    public findFastestPath(targetTechId: string, completedTechs: Set<string>): string[] {
        return this.buildPath(targetTechId, completedTechs);
    }

    public calculateTechTreeProgress(completedTechs: Set<string>): ProgressReport {
        if (!this.techTree) {
            return {
                totalNodes: 0,
                completedNodes: 0,
                percentage: 0,
                byCategory: {},
                byEra: {}
            };
        }

        const totalNodes = this.techTree.allNodes.size;
        const completedNodes = completedTechs.size;

        const byCategory: Record<string, { total: number; completed: number }> = {};
        const byEra: Record<string, { total: number; completed: number }> = {};

        for (const [techId, node] of this.techTree.allNodes) {
            const catKey = node.category;
            const eraKey = node.era;

            if (!byCategory[catKey]) {
                byCategory[catKey] = { total: 0, completed: 0 };
            }
            byCategory[catKey].total++;

            if (!byEra[eraKey]) {
                byEra[eraKey] = { total: 0, completed: 0 };
            }
            byEra[eraKey].total++;

            if (completedTechs.has(techId)) {
                byCategory[catKey].completed++;
                byEra[eraKey].completed++;
            }
        }

        return {
            totalNodes,
            completedNodes,
            percentage: totalNodes > 0 ? (completedNodes / totalNodes) * 100 : 0,
            byCategory,
            byEra
        };
    }
}

export interface ProgressReport {
    totalNodes: number;
    completedNodes: number;
    percentage: number;
    byCategory: Record<string, { total: number; completed: number }>;
    byEra: Record<string, { total: number; completed: number }>;
}

export class TechTreeRenderer {
    private viewState: TechTreeViewState;
    private renderedNodes: Map<string, RenderedNode> = new Map();
    private connectionLines: RenderedConnection[] = [];

    constructor() {
        this.viewState = this.getDefaultViewState();
    }

    private getDefaultViewState(): TechTreeViewState {
        return {
            centerNodeId: '',
            zoom: 1.0,
            visibleCategories: Object.values(ResearchCategory),
            visibleEras: Object.values(TechEra),
            showCompleted: true,
            showAvailable: true,
            showLocked: true,
            filterText: ''
        };
    }

    public setViewState(state: Partial<TechTreeViewState>): void {
        this.viewState = { ...this.viewState, ...state };
    }

    public getViewState(): TechTreeViewState {
        return { ...this.viewState };
    }

    public renderTechTree(
        techTree: TechTree,
        completedTechs: Set<string>,
        playerId: PlayerId
    ): RenderResult {
        const visibleNodes: RenderedNode[] = [];
        const connections: RenderedConnection[] = [];

        for (const [techId, node] of techTree.allNodes) {
            if (this.shouldShowNode(node)) {
                const rendered = this.renderNode(node, completedTechs.has(techId));
                visibleNodes.push(rendered);
                this.renderedNodes.set(techId, rendered);
            }
        }

        for (const node of visibleNodes) {
            const nodeData = techTree.allNodes.get(node.id);
            if (nodeData) {
                for (const prereq of nodeData.prerequisites) {
                    if (this.shouldShowConnection(node.id, prereq.techId)) {
                        connections.push({
                            from: prereq.techId,
                            to: node.id,
                            isComplete: completedTechs.has(prereq.techId)
                        });
                    }
                }
            }
        }

        this.connectionLines = connections;

        return {
            nodes: visibleNodes,
            connections,
            viewport: this.calculateViewport(visibleNodes)
        };
    }

    private shouldShowNode(node: TechNodeData): boolean {
        if (!this.viewState.visibleCategories.includes(node.category)) return false;
        if (!this.viewState.visibleEras.includes(node.era)) return false;

        if (!this.viewState.showCompleted && node.state === TechNodeState.Completed) return false;
        if (!this.viewState.showAvailable && node.state === TechNodeState.Available) return false;
        if (!this.viewState.showLocked && node.state === TechNodeState.Locked) return false;

        if (this.viewState.filterText) {
            const filter = this.viewState.filterText.toLowerCase();
            return node.name.toLowerCase().includes(filter) ||
                   node.description.toLowerCase().includes(filter);
        }

        return true;
    }

    private shouldShowConnection(nodeId: string, prereqId: string): boolean {
        const fromNode = this.renderedNodes.get(prereqId);
        const toNode = this.renderedNodes.get(nodeId);
        return !!fromNode && !!toNode;
    }

    private renderNode(node: TechNodeData, isCompleted: boolean): RenderedNode {
        return {
            id: node.id,
            name: node.name,
            description: node.description,
            category: node.category,
            era: node.era,
            icon: node.icon,
            state: isCompleted ? TechNodeState.Completed : node.state,
            cost: node.cost,
            progress: node.progress,
            position: this.calculateNodePosition(node),
            unlocks: node.unlocks.length,
            prerequisites: node.prerequisites.length
        };
    }

    private calculateNodePosition(node: TechNodeData): Vector2 {
        return new Vector2(0, 0);
    }

    private calculateViewport(nodes: RenderedNode[]): Viewport {
        if (nodes.length === 0) {
            return { x: 0, y: 0, width: 800, height: 600, zoom: 1.0 };
        }

        let minX = Infinity, minY = Infinity;
        let maxX = -Infinity, maxY = -Infinity;

        for (const node of nodes) {
            minX = Math.min(minX, node.position.x);
            minY = Math.min(minY, node.position.y);
            maxX = Math.max(maxX, node.position.x + 100);
            maxY = Math.max(maxY, node.position.y + 100);
        }

        return {
            x: minX,
            y: minY,
            width: maxX - minX + 100,
            height: maxY - minY + 100,
            zoom: this.viewState.zoom
        };
    }

    public getNodeRenderData(techId: string): RenderedNode | undefined {
        return this.renderedNodes.get(techId);
    }

    public getConnectionLines(): RenderedConnection[] {
        return this.connectionLines;
    }

    public zoomIn(): void {
        this.viewState.zoom = Math.min(3.0, this.viewState.zoom + 0.1);
    }

    public zoomOut(): void {
        this.viewState.zoom = Math.max(0.3, this.viewState.zoom - 0.1);
    }

    public resetZoom(): void {
        this.viewState.zoom = 1.0;
    }

    public centerOnNode(nodeId: string): void {
        this.viewState.centerNodeId = nodeId;
    }

    public toggleCategory(category: ResearchCategory): void {
        const index = this.viewState.visibleCategories.indexOf(category);
        if (index >= 0) {
            this.viewState.visibleCategories.splice(index, 1);
        } else {
            this.viewState.visibleCategories.push(category);
        }
    }

    public toggleEra(era: TechEra): void {
        const index = this.viewState.visibleEras.indexOf(era);
        if (index >= 0) {
            this.viewState.visibleEras.splice(index, 1);
        } else {
            this.viewState.visibleEras.push(era);
        }
    }

    public setFilter(filterText: string): void {
        this.viewState.filterText = filterText;
    }

    public clearFilter(): void {
        this.viewState.filterText = '';
    }
}

export interface RenderedNode {
    id: string;
    name: string;
    description: string;
    category: ResearchCategory;
    era: TechEra;
    icon: string;
    state: TechNodeState;
    cost: TechCost;
    progress: number;
    position: Vector2;
    unlocks: number;
    prerequisites: number;
}

export interface RenderedConnection {
    from: string;
    to: string;
    isComplete: boolean;
}

export interface RenderResult {
    nodes: RenderedNode[];
    connections: RenderedConnection[];
    viewport: Viewport;
}

export interface Viewport {
    x: number;
    y: number;
    width: number;
    height: number;
    zoom: number;
}

export class TechTreeManager {
    private techTrees: Map<string, TechTree> = new Map();
    private currentTreeId: string = '';
    private playerProgress: Map<PlayerId, PlayerTechProgress> = new Map();

    constructor() {}

    public createTechTree(id: string, name: string, description: string): TechTree {
        const tree: TechTree = {
            id,
            name,
            description,
            version: '1.0.0',
            rootNodes: [],
            allNodes: new Map(),
            categories: new Map(),
            eras: new Map(),
            dependencies: new Map()
        };

        this.techTrees.set(id, tree);
        this.currentTreeId = id;

        return tree;
    }

    public addTechNode(treeId: string, node: TechNodeData): boolean {
        const tree = this.techTrees.get(treeId);
        if (!tree) return false;

        tree.allNodes.set(node.id, node);

        if (node.prerequisites.length === 0) {
            if (!tree.rootNodes.includes(node.id)) {
                tree.rootNodes.push(node.id);
            }
        }

        const deps = tree.dependencies.get(node.id) || [];
        for (const prereq of node.prerequisites) {
            if (!deps.includes(prereq.techId)) {
                deps.push(prereq.techId);
            }
        }
        tree.dependencies.set(node.id, deps);

        let categoryNodes = tree.categories.get(node.category);
        if (!categoryNodes) {
            categoryNodes = [];
            tree.categories.set(node.category, categoryNodes);
        }
        if (!categoryNodes.includes(node.id)) {
            categoryNodes.push(node.id);
        }

        let eraNodes = tree.eras.get(node.era);
        if (!eraNodes) {
            eraNodes = [];
            tree.eras.set(node.era, eraNodes);
        }
        if (!eraNodes.includes(node.id)) {
            eraNodes.push(node.id);
        }

        return true;
    }

    public getTechTree(id: string): TechTree | undefined {
        return this.techTrees.get(id);
    }

    public getCurrentTree(): TechTree | undefined {
        return this.techTrees.get(this.currentTreeId);
    }

    public setCurrentTree(id: string): boolean {
        if (this.techTrees.has(id)) {
            this.currentTreeId = id;
            return true;
        }
        return false;
    }

    public getTechNode(treeId: string, nodeId: string): TechNodeData | undefined {
        const tree = this.techTrees.get(treeId);
        return tree ? tree.allNodes.get(nodeId) : undefined;
    }

    public initializePlayerProgress(playerId: PlayerId, treeId: string): void {
        const tree = this.techTrees.get(treeId);
        if (!tree) return;

        const progress: PlayerTechProgress = {
            playerId,
            treeId,
            completedTechs: new Set(),
            currentResearch: null,
            unlockedItems: {
                units: [],
                buildings: [],
                abilities: [],
                upgrades: []
            }
        };

        this.playerProgress.set(playerId, progress);

        for (const [nodeId, node] of tree.allNodes) {
            if (node.prerequisites.length === 0) {
                this.updateNodeState(playerId, nodeId, TechNodeState.Available);
            } else {
                this.updateNodeState(playerId, nodeId, TechNodeState.Locked);
            }
        }
    }

    public getPlayerProgress(playerId: PlayerId): PlayerTechProgress | undefined {
        return this.playerProgress.get(playerId);
    }

    public completeTechNode(playerId: PlayerId, nodeId: string): boolean {
        const progress = this.playerProgress.get(playerId);
        if (!progress) return false;

        const tree = this.getCurrentTree();
        if (!tree) return false;

        const node = tree.allNodes.get(nodeId);
        if (!node) return false;

        progress.completedTechs.add(nodeId);
        this.updateNodeState(playerId, nodeId, TechNodeState.Completed);

        this.updateDependentNodes(playerId, nodeId);

        return true;
    }

    private updateDependentNodes(playerId: PlayerId, completedNodeId: string): void {
        const tree = this.getCurrentTree();
        if (!tree) return;

        for (const [nodeId, node] of tree.allNodes) {
            if (node.state === TechNodeState.Locked) {
                const hasAllPrereqs = node.prerequisites.every(prereq =>
                    progress.completedTechs.has(prereq.techId)
                );
                if (hasAllPrereqs) {
                    this.updateNodeState(playerId, nodeId, TechNodeState.Available);
                }
            }
        }
    }

    private updateNodeState(playerId: PlayerId, nodeId: string, state: TechNodeState): void {
        const tree = this.getCurrentTree();
        if (!tree) return;

        const node = tree.allNodes.get(nodeId);
        if (node) {
            node.stateHistory.push(state);
            if (node.stateHistory.length > 10) {
                node.stateHistory.shift();
            }
        }
    }

    public isTechCompleted(playerId: PlayerId, nodeId: string): boolean {
        const progress = this.playerProgress.get(playerId);
        return progress ? progress.completedTechs.has(nodeId) : false;
    }

    public getCompletedTechs(playerId: PlayerId): Set<string> {
        const progress = this.playerProgress.get(playerId);
        return progress ? new Set(progress.completedTechs) : new Set();
    }

    public getAvailableTechs(playerId: PlayerId): string[] {
        const progress = this.playerProgress.get(playerId);
        const tree = this.getCurrentTree();
        if (!progress || !tree) return [];

        const available: string[] = [];
        for (const [nodeId, node] of tree.allNodes) {
            if (node.state === TechNodeState.Available) {
                available.push(nodeId);
            }
        }

        return available;
    }

    public removeTechTree(id: string): boolean {
        return this.techTrees.delete(id);
    }
}

export interface PlayerTechProgress {
    playerId: PlayerId;
    treeId: string;
    completedTechs: Set<string>;
    currentResearch: string | null;
    unlockedItems: {
        units: string[];
        buildings: string[];
        abilities: string[];
        upgrades: string[];
    };
}

export class ResearchManager {
    private playerResearch: Map<PlayerId, PlayerResearchData> = new Map();
    private resourceManager?: ResourceManager;
    private researchSlots: Map<PlayerId, ResearchSlot[]> = new Map();
    private maxResearchSlots: number = 2;

    constructor(maxSlots: number = 2) {
        this.maxResearchSlots = maxSlots;
    }

    public setResourceManager(manager: ResourceManager): void {
        this.resourceManager = manager;
    }

    public initializePlayer(playerId: PlayerId): void {
        const slots: ResearchSlot[] = [];
        for (let i = 0; i < this.maxResearchSlots; i++) {
            slots.push(this.createEmptySlot(i));
        }
        this.researchSlots.set(playerId, slots);

        this.playerResearch.set(playerId, {
            playerId,
            activeResearch: new Map(),
            researchHistory: [],
            totalResearchTime: 0,
            completedResearch: 0
        });
    }

    private createEmptySlot(id: number): ResearchSlot {
        return {
            id,
            techId: null,
            startTime: 0,
            progress: 0,
            isActive: false,
            priority: ResearchPriority.Normal,
            estimatedCompletion: 0
        };
    }

    public startResearch(playerId: PlayerId, techId: string, tech: TechNodeData): boolean {
        const slots = this.researchSlots.get(playerId);
        if (!slots) return false;

        const emptySlot = slots.find(s => !s.techId);
        if (!emptySlot) return false;

        emptySlot.techId = techId;
        emptySlot.startTime = Date.now();
        emptySlot.progress = 0;
        emptySlot.isActive = true;
        emptySlot.estimatedCompletion = Date.now() + tech.researchTime;

        const research = this.playerResearch.get(playerId);
        if (research) {
            research.activeResearch.set(techId, {
                techId,
                slotId: emptySlot.id,
                startTime: Date.now(),
                lastUpdate: Date.now(),
                progress: 0,
                isPaused: false
            });
        }

        return true;
    }

    public pauseResearch(playerId: PlayerId, techId: string): boolean {
        const slots = this.researchSlots.get(playerId);
        if (!slots) return false;

        const slot = slots.find(s => s.techId === techId);
        if (!slot) return false;

        slot.isActive = false;
        return true;
    }

    public resumeResearch(playerId: PlayerId, techId: string): boolean {
        const slots = this.researchSlots.get(playerId);
        if (!slots) return false;

        const slot = slots.find(s => s.techId === techId);
        if (!slot) return false;

        slot.isActive = true;
        return true;
    }

    public cancelResearch(playerId: PlayerId, techId: string): boolean {
        const slots = this.researchSlots.get(playerId);
        if (!slots) return false;

        const slot = slots.find(s => s.techId === techId);
        if (!slot) return false;

        const resetSlot = this.createEmptySlot(slot.id);
        slot.id = resetSlot.id;
        slot.techId = resetSlot.techId;
        slot.startTime = resetSlot.startTime;
        slot.progress = resetSlot.progress;
        slot.isActive = resetSlot.isActive;
        slot.priority = resetSlot.priority;
        slot.estimatedCompletion = resetSlot.estimatedCompletion;

        const research = this.playerResearch.get(playerId);
        if (research) {
            research.activeResearch.delete(techId);
        }

        return true;
    }

    public updateResearch(playerId: PlayerId, deltaTime: number, bonusMultiplier: number = 1.0): void {
        const slots = this.researchSlots.get(playerId);
        if (!slots) return;

        for (const slot of slots) {
            if (slot.techId && slot.isActive) {
                const tree = this.getCurrentTechTree();
                const node = tree?.allNodes.get(slot.techId);
                if (node) {
                    const progressRate = (deltaTime / node.researchTime) * bonusMultiplier;
                    slot.progress = Math.min(100, slot.progress + progressRate * 100);
                }
            }
        }
    }

    public getResearchProgress(playerId: PlayerId, techId: string): number {
        const slots = this.researchSlots.get(playerId);
        if (!slots) return 0;

        const slot = slots.find(s => s.techId === techId);
        return slot ? slot.progress : 0;
    }

    public getActiveResearch(playerId: PlayerId): ResearchSlot[] {
        const slots = this.researchSlots.get(playerId);
        return slots ? slots.filter(s => s.techId !== null) : [];
    }

    public getAvailableSlot(playerId: PlayerId): ResearchSlot | null {
        const slots = this.researchSlots.get(playerId);
        return slots ? slots.find(s => !s.techId) || null : null;
    }

    public hasActiveResearch(playerId: PlayerId, techId: string): boolean {
        const slots = this.researchSlots.get(playerId);
        return slots ? slots.some(s => s.techId === techId) : false;
    }

    public getResearchSlot(playerId: PlayerId, slotId: number): ResearchSlot | null {
        const slots = this.researchSlots.get(playerId);
        return slots ? slots[slotId] || null : null;
    }

    private getCurrentTechTree(): TechTree | undefined {
        return undefined;
    }

    public setMaxResearchSlots(max: number): void {
        this.maxResearchSlots = Math.max(1, max);
    }

    public getMaxResearchSlots(): number {
        return this.maxResearchSlots;
    }
}

export interface PlayerResearchData {
    playerId: PlayerId;
    activeResearch: Map<string, ActiveResearchData>;
    researchHistory: ResearchHistoryEntry[];
    totalResearchTime: number;
    completedResearch: number;
}

export interface ActiveResearchData {
    techId: string;
    slotId: number;
    startTime: number;
    lastUpdate: number;
    progress: number;
    isPaused: boolean;
}

export interface ResearchHistoryEntry {
    techId: string;
    startTime: number;
    completionTime: number;
    duration: number;
}

export class CampaignTechUnlocks {
    private campaignTechUnlocks: Map<string, CampaignUnlock[]> = new Map();
    private missionProgress: Map<string, Set<string>> = new Map();

    constructor() {}

    public registerCampaign(campaignId: string, unlocks: CampaignUnlock[]): void {
        this.campaignTechUnlocks.set(campaignId, unlocks);
    }

    public unlockTechForMission(campaignId: string, missionId: string, techId: string): boolean {
        const unlocks = this.campaignTechUnlocks.get(campaignId);
        if (!unlocks) return false;

        const unlock = unlocks.find(u => u.missionId === missionId);
        if (!unlock) return false;

        let progress = this.missionProgress.get(campaignId);
        if (!progress) {
            progress = new Set();
            this.missionProgress.set(campaignId, progress);
        }

        progress.add(`${missionId}:${techId}`);
        return true;
    }

    public isTechUnlocked(campaignId: string, missionId: string, techId: string): boolean {
        const unlocks = this.campaignTechUnlocks.get(campaignId);
        if (!unlocks) return false;

        const unlock = unlocks.find(u => u.missionId === missionId);
        if (!unlock) return false;

        if (!unlock.requiredMission) return true;

        const progress = this.missionProgress.get(campaignId);
        return progress ? progress.has(`${unlock.requiredMission}:unlocked`) : false;
    }

    public getMissionUnlocks(campaignId: string, missionId: string): string[] {
        const unlocks = this.campaignTechUnlocks.get(campaignId);
        if (!unlocks) return [];

        const unlock = unlocks.find(u => u.missionId === missionId);
        return unlock ? unlock.techIds : [];
    }

    public getAllCampaignUnlocks(campaignId: string): CampaignUnlock[] {
        return this.campaignTechUnlocks.get(campaignId) || [];
    }

    public resetCampaignProgress(campaignId: string): void {
        this.missionProgress.delete(campaignId);
    }
}

export interface CampaignUnlock {
    missionId: string;
    techIds: string[];
    requiredMission?: string;
    isSecret?: boolean;
}

export class MultiplayerTechSync {
    private syncQueue: Map<PlayerId, SyncOperation[]> = new Map();
    private confirmedOperations: Set<string> = new Set();
    private syncInterval: number = 1000;
    private lastSyncTime: number = 0;

    constructor() {}

    public queueSyncOperation(playerId: PlayerId, operation: SyncOperation): void {
        let queue = this.syncQueue.get(playerId);
        if (!queue) {
            queue = [];
            this.syncQueue.set(playerId, queue);
        }

        queue.push({
            ...operation,
            timestamp: Date.now(),
            operationId: this.generateOperationId()
        });
    }

    private generateOperationId(): string {
        return `op_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    public getPendingOperations(playerId: PlayerId): SyncOperation[] {
        return this.syncQueue.get(playerId) || [];
    }

    public confirmOperation(operationId: string): void {
        this.confirmedOperations.add(operationId);
        this.cleanupConfirmedOperations();
    }

    public isOperationConfirmed(operationId: string): boolean {
        return this.confirmedOperations.has(operationId);
    }

    public createSyncPacket(playerId: PlayerId): SyncPacket {
        const pendingOps = this.getPendingOperations(playerId);
        const confirmedOps = pendingOps.filter(op =>
            this.confirmedOperations.has(op.operationId || '')
        );

        return {
            playerId,
            timestamp: Date.now(),
            operations: confirmedOps,
            checksum: this.calculateChecksum(confirmedOps)
        };
    }

    public applySyncPacket(packet: SyncPacket, techTreeManager: TechTreeManager): SyncResult {
        const result: SyncResult = {
            success: true,
            appliedOperations: [],
            failedOperations: []
        };

        for (const op of packet.operations) {
            if (!this.validateOperation(op)) {
                result.failedOperations.push(op);
                result.success = false;
                continue;
            }

            try {
                this.applyOperation(op, techTreeManager);
                result.appliedOperations.push(op);
                if (op.operationId) {
                    this.confirmOperation(op.operationId);
                }
            } catch (error) {
                result.failedOperations.push(op);
                result.success = false;
            }
        }

        return result;
    }

    private validateOperation(operation: SyncOperation): boolean {
        if (!operation.techId) return false;
        if (!operation.playerId) return false;
        return true;
    }

    private applyOperation(operation: SyncOperation, techTreeManager: TechTreeManager): void {
        switch (operation.type) {
            case 'complete':
                techTreeManager.completeTechNode(operation.playerId, operation.techId);
                break;
            case 'start':
                break;
            case 'cancel':
                break;
        }
    }

    private calculateChecksum(operations: SyncOperation[]): string {
        const data = JSON.stringify(operations);
        let hash = 0;
        for (let i = 0; i < data.length; i++) {
            const char = data.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash;
        }
        return hash.toString(16);
    }

    private cleanupConfirmedOperations(): void {
        for (const [playerId, queue] of this.syncQueue) {
            const filtered = queue.filter(op =>
                op.operationId && !this.confirmedOperations.has(op.operationId)
            );
            this.syncQueue.set(playerId, filtered);
        }
    }

    public setSyncInterval(interval: number): void {
        this.syncInterval = Math.max(100, interval);
    }

    public shouldSync(): boolean {
        const now = Date.now();
        if (now - this.lastSyncTime >= this.syncInterval) {
            this.lastSyncTime = now;
            return true;
        }
        return false;
    }

    public clearPendingOperations(playerId: PlayerId): void {
        this.syncQueue.delete(playerId);
    }
}

export interface SyncOperation {
    type: 'start' | 'complete' | 'cancel';
    playerId: PlayerId;
    techId: string;
    timestamp: number;
    operationId?: string;
    data?: Record<string, any>;
}

export interface SyncPacket {
    playerId: PlayerId;
    timestamp: number;
    operations: SyncOperation[];
    checksum: string;
}

export interface SyncResult {
    success: boolean;
    appliedOperations: SyncOperation[];
    failedOperations: SyncOperation[];
}

export class ResearchSystem {
    private techTreeManager: TechTreeManager;
    private researchManager: ResearchManager;
    private prerequisiteChecker: PrerequisiteChecker;
    private techUnlocking: TechUnlocking;
    private researchQueue: ResearchQueue;
    private researchProgress: ResearchProgress;
    private techCompatibility: TechCompatibility;
    private techCategory: TechCategory;
    private techCost: TechCostCalculator;
    private researchEventHandler: ResearchEventHandler;
    private techTreeRenderer: TechTreeRenderer;
    private techTreeCalculator: TechTreeCalculator;
    private researchBonus: ResearchBonus;
    private campaignTechUnlocks: CampaignTechUnlocks;
    private multiplayerSync: MultiplayerTechSync;
    private eraSystem: TechEraSystem;
    private resourceManager?: ResourceManager;
    private isRunning: boolean = false;
    private updateCallbacks: Array<(deltaTime: number) => void> = [];

    constructor() {
        this.techTreeManager = new TechTreeManager();
        this.researchManager = new ResearchManager(2);
        this.prerequisiteChecker = new PrerequisiteChecker();
        this.techUnlocking = new TechUnlocking();
        this.researchQueue = new ResearchQueue(5);
        this.researchProgress = new ResearchProgress();
        this.techCompatibility = new TechCompatibility();
        this.techCategory = new TechCategory();
        this.techCost = new TechCostCalculator();
        this.researchEventHandler = new ResearchEventHandler();
        this.techTreeRenderer = new TechTreeRenderer();
        this.techTreeCalculator = new TechTreeCalculator();
        this.researchBonus = new ResearchBonus();
        this.campaignTechUnlocks = new CampaignTechUnlocks();
        this.multiplayerSync = new MultiplayerTechSync();
        this.eraSystem = new TechEraSystem();
    }

    public initialize(config: ResearchSystemConfig): void {
        if (config.resourceManager) {
            this.resourceManager = config.resourceManager;
            this.researchManager.setResourceManager(config.resourceManager);
        }

        this.researchManager.setMaxResearchSlots(config.maxResearchSlots || 2);
        this.researchQueue.setMaxQueueSize(config.maxQueueSize || 5);

        if (config.techTree) {
            this.techTreeManager.setCurrentTree(config.techTree.id);
            this.techTreeCalculator.setTechTree(config.techTree);
        }

        this.setupEventHandlers();
    }

    private setupEventHandlers(): void {
        this.researchEventHandler.onResearchComplete((event) => {
            this.handleResearchComplete(event);
        });

        this.researchEventHandler.onResearchStart((event) => {
        });
    }

    private handleResearchComplete(event: ResearchEvent): void {
        const tree = this.techTreeManager.getCurrentTree();
        if (!tree) return;

        const tech = tree.allNodes.get(event.techId);
        if (!tech) return;

        this.techTreeManager.completeTechNode(event.playerId, event.techId);

        const unlockResult = this.techUnlocking.unlockTech(event.playerId, tech);

        this.techUnlocking.applyTechEffects(event.playerId, tech);

        this.prerequisiteChecker.addCompletedTech(event.playerId, event.techId);

        this.researchEventHandler.emitEvent({
            type: 'unlock',
            techId: event.techId,
            timestamp: Date.now(),
            playerId: event.playerId,
            data: { unlockResult }
        });
    }

    public createTechTree(id: string, name: string, description: string): TechTree {
        return this.techTreeManager.createTechTree(id, name, description);
    }

    public addTechNode(node: TechNodeData): boolean {
        const tree = this.techTreeManager.getCurrentTree();
        if (!tree) return false;

        return this.techTreeManager.addTechNode(tree.id, node);
    }

    public initializePlayer(playerId: PlayerId): void {
        const tree = this.techTreeManager.getCurrentTree();
        if (!tree) return;

        this.techTreeManager.initializePlayerProgress(playerId, tree.id);
        this.researchManager.initializePlayer(playerId);
    }

    public startResearch(playerId: PlayerId, techId: string): ResearchResult {
        const tree = this.techTreeManager.getCurrentTree();
        if (!tree) {
            return { success: false, error: 'No tech tree loaded' };
        }

        const tech = tree.allNodes.get(techId);
        if (!tech) {
            return { success: false, error: 'Tech not found' };
        }

        if (this.techTreeManager.isTechCompleted(playerId, techId)) {
            return { success: false, error: 'Tech already completed' };
        }

        if (this.researchManager.hasActiveResearch(playerId, techId)) {
            return { success: false, error: 'Tech already being researched' };
        }

        const prereqResult = this.prerequisiteChecker.checkPrerequisites(playerId, tech, tree);
        if (!prereqResult.isValid) {
            return {
                success: false,
                error: 'Prerequisites not met',
                missingPrerequisites: prereqResult.missingPrerequisites
            };
        }

        const slot = this.researchManager.getAvailableSlot(playerId);
        if (!slot) {
            return { success: false, error: 'No available research slots' };
        }

        if (this.resourceManager) {
            const player = this.resourceManager.getPlayer(playerId);
            if (player) {
                const effectiveCost = this.techCost.getEffectiveCost(tech.cost, player.getResourceBonuses());
                if (!this.techCost.canAfford(effectiveCost, player.getResources())) {
                    return { success: false, error: 'Insufficient resources' };
                }
                player.consumeResources(effectiveCost);
            }
        }

        const started = this.researchManager.startResearch(playerId, techId, tech);
        if (!started) {
            return { success: false, error: 'Failed to start research' };
        }

        this.prerequisiteChecker.addPendingTech(playerId, techId);

        this.researchEventHandler.emitEvent({
            type: 'start',
            techId,
            timestamp: Date.now(),
            playerId
        });

        return { success: true, slotId: slot.id };
    }

    public cancelResearch(playerId: PlayerId, techId: string): boolean {
        const cancelled = this.researchManager.cancelResearch(playerId, techId);
        if (cancelled) {
            this.prerequisiteChecker.removePendingTech(playerId, techId);

            this.researchEventHandler.emitEvent({
                type: 'cancel',
                techId,
                timestamp: Date.now(),
                playerId
            });
        }
        return cancelled;
    }

    public update(deltaTime: number): void {
        if (!this.isRunning) return;

        for (const callback of this.updateCallbacks) {
            callback(deltaTime);
        }

        for (const [playerId] of this.playerResearch) {
            const bonusMultiplier = this.researchBonus.getResearchSpeedBonus(playerId);
            this.researchManager.updateResearch(playerId, deltaTime, bonusMultiplier);

            this.checkResearchCompletion(playerId);
        }
    }

    private checkResearchCompletion(playerId: PlayerId): void {
        const activeResearch = this.researchManager.getActiveResearch(playerId);
        for (const slot of activeResearch) {
            if (slot.techId && slot.progress >= 100) {
                this.completeResearch(playerId, slot.techId);
            }
        }
    }

    private completeResearch(playerId: PlayerId, techId: string): void {
        this.researchManager.cancelResearch(playerId, techId);

        this.researchEventHandler.emitEvent({
            type: 'complete',
            techId,
            timestamp: Date.now(),
            playerId
        });

        const tree = this.techTreeManager.getCurrentTree();
        if (tree) {
            const tech = tree.allNodes.get(techId);
            if (tech) {
                this.researchProgress.notifyCompletion(techId, tech);
            }
        }
    }

    public start(): void {
        this.isRunning = true;
    }

    public stop(): void {
        this.isRunning = false;
    }

    public getTechTreeManager(): TechTreeManager {
        return this.techTreeManager;
    }

    public getResearchManager(): ResearchManager {
        return this.researchManager;
    }

    public getPrerequisiteChecker(): PrerequisiteChecker {
        return this.prerequisiteChecker;
    }

    public getTechUnlocking(): TechUnlocking {
        return this.techUnlocking;
    }

    public getResearchQueue(): ResearchQueue {
        return this.researchQueue;
    }

    public getResearchProgress(): ResearchProgress {
        return this.researchProgress;
    }

    public getTechCompatibility(): TechCompatibility {
        return this.techCompatibility;
    }

    public getTechCategory(): TechCategory {
        return this.techCategory;
    }

    public getTechCost(): TechCostCalculator {
        return this.techCost;
    }

    public getResearchEventHandler(): ResearchEventHandler {
        return this.researchEventHandler;
    }

    public getTechTreeRenderer(): TechTreeRenderer {
        return this.techTreeRenderer;
    }

    public getTechTreeCalculator(): TechTreeCalculator {
        return this.techTreeCalculator;
    }

    public getResearchBonus(): ResearchBonus {
        return this.researchBonus;
    }

    public getCampaignTechUnlocks(): CampaignTechUnlocks {
        return this.campaignTechUnlocks;
    }

    public getMultiplayerSync(): MultiplayerTechSync {
        return this.multiplayerSync;
    }

    public getEraSystem(): TechEraSystem {
        return this.eraSystem;
    }

    public registerUpdateCallback(callback: (deltaTime: number) => void): void {
        this.updateCallbacks.push(callback);
    }

    public unregisterUpdateCallback(callback: (deltaTime: number) => void): void {
        const index = this.updateCallbacks.indexOf(callback);
        if (index >= 0) {
            this.updateCallbacks.splice(index, 1);
        }
    }

    public isActive(): boolean {
        return this.isRunning;
    }

    public getSystemState(): ResearchSystemState {
        return {
            isRunning: this.isRunning,
            currentTreeId: this.techTreeManager.getCurrentTree()?.id || '',
            activePlayers: Array.from(this.playerResearch.keys())
        };
    }
}

export interface ResearchSystemConfig {
    resourceManager?: ResourceManager;
    maxResearchSlots?: number;
    maxQueueSize?: number;
    techTree?: TechTree;
}

export interface ResearchResult {
    success: boolean;
    error?: string;
    slotId?: number;
    missingPrerequisites?: TechPrerequisite[];
}

export interface ResearchSystemState {
    isRunning: boolean;
    currentTreeId: string;
    activePlayers: PlayerId[];
}

export default ResearchSystem;

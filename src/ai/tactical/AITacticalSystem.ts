import { Vector2D } from '../../entities/Entity';
import { Unit } from '../../game/units/UnitSystem';
import { Building } from '../../game/buildings/BuildingSystem';

export enum CombatPhase {
  NONE = 'none',
  APPROACH = 'approach',
  ENGAGEMENT = 'engagement',
  COMBAT = 'combat',
  ROUTING = 'routing',
  POST_BATTLE = 'post_battle'
}

export enum FormationType {
  LINE = 'line',
  WEDGE = 'wedge',
  CIRCLE = 'circle',
  SQUARE = 'square',
  FLANK = 'flank',
  DEFENSIVE = 'defensive',
  SKIRMISH = 'skirmish',
  PINCH = 'pinch'
}

export enum TacticalPriority {
  SURVIVAL = 'survival',
  DAMAGE = 'damage',
  OBJECTIVE = 'objective',
  TERRITORY = 'territory',
  ECONOMY = 'economy',
  DEFENSE = 'defense'
}

export enum TargetSelectionMode {
  THREAT = 'threat',
  VALUE = 'value',
  DISTANCE = 'distance',
  PRIORITY = 'priority',
  COMBINED = 'combined'
}

export enum RetreatReason {
  LOW_HEALTH = 'low_health',
  OVERWHELMED = 'overwhelmed',
  SURROUNDED = 'surrounded',
  NO_AMMO = 'no_ammo',
  LOST_ADVANTAGE = 'lost_advantage',
  SUPERIOR_FORCE = 'superior_force',
  STRATEGIC_RETREAT = 'strategic_retreat'
}

export enum AbilityType {
  INSTANT = 'instant',
  CHANNELED = 'channeled',
  TOGGLE = 'toggle',
  COOLDOWN = 'cooldown',
  CHARGE = 'charge'
}

export enum UnitRole {
  TANK = 'tank',
  DAMAGE_DEALER = 'damage_dealer',
  SUPPORT = 'support',
  SCOUT = 'scout',
  RANGED = 'ranged',
  MELEE = 'melee',
  SIEGE = 'siege',
  AIR = 'air'
}

export enum DamageType {
  BALLISTIC = 'ballistic',
  EXPLOSIVE = 'explosive',
  ENERGY = 'energy',
  FIRE = 'fire',
  POISON = 'poison',
  TRUE = 'true'
}

export enum CombatStyle {
  AGGRESSIVE = 'aggressive',
  DEFENSIVE = 'defensive',
  EVASIVE = 'evasive',
  BALANCED = 'balanced',
  SIEGE = 'siege',
  HARASS = 'harass'
}

export enum TerrainFeature {
  ELEVATION = 'elevation',
  COVER = 'cover',
  CHOKEPOINT = 'chokepoint',
  HIGH_GROUND = 'high_ground',
  FLANKING = 'flanking',
  RETREAT_PATH = 'retreat_path',
  DEFENSIBLE = 'defensible'
}

export enum GroupTactics {
  FOCUS_FIRE = 'focus_fire',
  SPLIT_ATTACK = 'split_attack',
  SURROUND = 'surround',
  PINCER = 'pincer',
  CHARGE = 'charge',
  DEFENSIVE_HOLD = 'defensive_hold',
  RETREAT_ORDERED = 'retreat_ordered'
}

export enum TacticalObjective {
  DESTROY_ENEMY = 'destroy_enemy',
  CAPTURE_POSITION = 'capture_position',
  DEFEND_POSITION = 'defend_position',
  PROTECT_UNIT = 'protect_unit',
  ELIMINATE_THREAT = 'eliminate_threat',
  SIEGE_STRUCTURE = 'siege_structure',
  HARASS = 'harass'
}

export interface TacticalSituation {
  id: string;
  position: Vector2D;
  enemyUnits: Unit[];
  friendlyUnits: Unit[];
  neutralEntities: (Unit | Building)[];
  combatPhase: CombatPhase;
  threatLevel: number;
  advantageLevel: number;
  terrainFeatures: Map<TerrainFeature, number>;
  nearbyStructures: Building[];
  objectives: TacticalObjective[];
  timeInCombat: number;
  estimatedEnemyReinforcements: number;
  estimatedFriendlyReinforcements: number;
  overallFirepower: { friendly: number; enemy: number };
  averageHealth: { friendly: number; enemy: number };
}

export interface CombatAction {
  id: string;
  type: 'attack' | 'move' | 'ability' | 'retreat' | 'reposition' | 'defend' | 'support';
  sourceUnit: string;
  targetEntity?: string;
  targetPosition?: Vector2D;
  abilityId?: string;
  priority: number;
  estimatedDamage?: number;
  estimatedRisk?: number;
  cooldown: number;
  energyCost: number;
  prerequisites: string[];
  expectedOutcome: CombatOutcome;
  executionTime: number;
  successProbability: number;
}

export interface FormationTemplate {
  type: FormationType;
  name: string;
  description: string;
  unitSlots: FormationSlot[];
  spacing: number;
  orientation: 'forward' | 'backward' | 'lateral';
  flexibility: number;
  recommendedUnits: UnitRole[];
  strengths: string[];
  weaknesses: string[];
  optimalTerrain: TerrainFeature[];
}

export interface FormationSlot {
  index: number;
  relativePosition: Vector2D;
  preferredRole: UnitRole;
  isFlexible: boolean;
  requiredUnitCount: number;
}

export interface TacticalScore {
  overall: number;
  damagePotential: number;
  survivalChance: number;
  positionalAdvantage: number;
  resourceEfficiency: number;
  objectiveProgress: number;
  riskAssessment: number;
  factors: Map<string, number>;
  timestamp: number;
}

export interface CombatMetrics {
  totalDamageDealt: number;
  totalDamageTaken: number;
  unitsLost: number;
  enemiesKilled: number;
  abilitiesUsed: number;
  abilitiesOnCooldown: Map<string, number>;
  averageHealthPercent: number;
  combatDuration: number;
  territoryGained: number;
  territoryLost: number;
  efficiency: number;
}

export interface AttackOrder {
  id: string;
  priority: number;
  targetEntity?: string;
  targetPosition?: Vector2D;
  assignedUnits: string[];
  formationType: FormationType;
  attackStyle: CombatStyle;
  supportUnits: string[];
  expectedOutcome: CombatOutcome;
  contingencies: ContingencyPlan[];
  isExecuting: boolean;
  progress: number;
}

export interface DefensePosition {
  id: string;
  position: Vector2D;
  defenseRadius: number;
  assignedUnits: string[];
  defenseStrength: number;
  fallbackPosition?: Vector2D;
  isActive: boolean;
  priority: TacticalPriority;
  fortifiedLevel: number;
  naturalCover: number;
  chokepoint: boolean;
}

export interface CombatOutcome {
  estimatedDamageToEnemy: number;
  estimatedDamageToSelf: number;
  victoryProbability: number;
  survivalProbability: number;
  resourceGain: number;
  resourceLoss: number;
  timeToResolution: number;
  risks: CombatRisk[];
}

export interface CombatRisk {
  type: string;
  severity: number;
  probability: number;
  mitigation?: string;
}

export interface ContingencyPlan {
  trigger: string;
  condition: string;
  alternativeAction: CombatAction;
  priority: number;
}

export interface TerrainAnalysis {
  elevation: number;
  coverAvailable: number;
  chokepointStrength: number;
  defensibility: number;
  flankingOpportunities: Vector2D[];
  retreatPaths: Vector2D[];
  optimalPositions: Vector2D[];
  threatVectors: Vector2D[];
}

export interface AbilityCooldown {
  abilityId: string;
  remainingTime: number;
  maxCooldown: number;
  isReady: boolean;
  lastUsed: number;
}

export interface GroupAssignment {
  groupId: string;
  units: string[];
  leaderId: string;
  objective: TacticalObjective;
  targetPosition?: Vector2D;
  targetEntity?: string;
  formation: FormationType;
  tactics: GroupTactics;
  isActive: boolean;
  morale: number;
}

export interface CombatSimulation {
  duration: number;
  friendlyLosses: number;
  enemyLosses: number;
  damageDealt: number;
  damageTaken: number;
  positionChanges: Vector2D[];
  abilitiesUsed: number;
  victoryProbability: number;
  recommendedActions: CombatAction[];
}

export interface AbilityUsePlan {
  abilityId: string;
  optimalTiming: number;
  targetPosition?: Vector2D;
  targetEntity?: string;
  expectedEffect: string;
  priority: number;
  isReady: boolean;
  lastUsedEffectiveness: number;
}

export class CombatAnalyzer {
  private situationCache: Map<string, TacticalSituation> = new Map();
  private cacheTimeout: number = 500;
  private lastAnalysis: number = 0;
  private historicalMetrics: CombatMetrics[] = [];
  private threatAssessmentCache: Map<string, number> = new Map();

  constructor() {
    this.initializeDefaultMetrics();
  }

  private initializeDefaultMetrics(): void {
    this.historicalMetrics.push({
      totalDamageDealt: 0,
      totalDamageTaken: 0,
      unitsLost: 0,
      enemiesKilled: 0,
      abilitiesUsed: 0,
      abilitiesOnCooldown: new Map(),
      averageHealthPercent: 100,
      combatDuration: 0,
      territoryGained: 0,
      territoryLost: 0,
      efficiency: 0
    });
  }

  public analyzeSituation(
    position: Vector2D,
    friendlyUnits: Unit[],
    enemyUnits: Unit[],
    structures: Building[]
  ): TacticalSituation {
    const cacheKey = this.generateCacheKey(position, friendlyUnits, enemyUnits);
    const cached = this.situationCache.get(cacheKey);
    
    if (cached && Date.now() - this.lastAnalysis < this.cacheTimeout) {
      return cached;
    }

    const situation: TacticalSituation = {
      id: `situation_${Date.now()}`,
      position,
      enemyUnits,
      friendlyUnits,
      neutralEntities: [],
      combatPhase: this.determineCombatPhase(friendlyUnits, enemyUnits),
      threatLevel: this.calculateThreatLevel(enemyUnits, friendlyUnits),
      advantageLevel: this.calculateAdvantageLevel(friendlyUnits, enemyUnits),
      terrainFeatures: this.analyzeTerrainFeatures(position, structures),
      nearbyStructures: structures.filter(s => this.isNearby(position, s.position, 500)),
      objectives: this.identifyObjectives(friendlyUnits, enemyUnits),
      timeInCombat: this.calculateTimeInCombat(friendlyUnits),
      estimatedEnemyReinforcements: this.estimateReinforcements(enemyUnits),
      estimatedFriendlyReinforcements: this.estimateReinforcements(friendlyUnits),
      overallFirepower: {
        friendly: this.calculateTotalFirepower(friendlyUnits),
        enemy: this.calculateTotalFirepower(enemyUnits)
      },
      averageHealth: {
        friendly: this.calculateAverageHealth(friendlyUnits),
        enemy: this.calculateAverageHealth(enemyUnits)
      }
    };

    this.situationCache.set(cacheKey, situation);
    this.lastAnalysis = Date.now();
    return situation;
  }

  private generateCacheKey(position: Vector2D, friendly: Unit[], enemy: Unit[]): string {
    return `${position.x}_${position.y}_${friendly.length}_${enemy.length}`;
  }

  private determineCombatPhase(friendly: Unit[], enemy: Unit[]): CombatPhase {
    if (friendly.length === 0 || enemy.length === 0) {
      return CombatPhase.NONE;
    }

    const avgFriendlyDist = this.calculateAverageDistance(friendly);
    const avgEnemyDist = this.calculateAverageDistance(enemy);
    const distBetweenForces = this.calculateDistanceBetweenForces(friendly, enemy);

    if (distBetweenForces > 400) {
      return CombatPhase.APPROACH;
    } else if (distBetweenForces > 150) {
      return CombatPhase.ENGAGEMENT;
    } else {
      const friendlyHealth = this.calculateAverageHealth(friendly);
      const enemyHealth = this.calculateAverageHealth(enemy);
      
      if (friendlyHealth < 30 || enemyHealth < 30) {
        return CombatPhase.ROUTING;
      }
      return CombatPhase.COMBAT;
    }
  }

  private calculateAverageDistance(units: Unit[]): number {
    if (units.length === 0) return 0;
    
    let totalDist = 0;
    let count = 0;
    
    for (let i = 0; i < units.length; i++) {
      for (let j = i + 1; j < units.length; j++) {
        totalDist += units[i].getDistanceTo(units[j]);
        count++;
      }
    }
    
    return count > 0 ? totalDist / count : 0;
  }

  private calculateDistanceBetweenForces(friendly: Unit[], enemy: Unit[]): number {
    if (friendly.length === 0 || enemy.length === 0) return Infinity;
    
    const friendlyCenter = this.calculateCentroid(friendly);
    const enemyCenter = this.calculateCentroid(enemy);
    
    const dx = enemyCenter.x - friendlyCenter.x;
    const dy = enemyCenter.y - friendlyCenter.y;
    return Math.sqrt(dx * dx + dy * dy);
  }

  private calculateCentroid(units: Unit[]): Vector2D {
    if (units.length === 0) return { x: 0, y: 0 };
    
    let x = 0, y = 0;
    for (const unit of units) {
      x += unit.position.x;
      y += unit.position.y;
    }
    
    return { x: x / units.length, y: y / units.length };
  }

  private calculateThreatLevel(enemies: Unit[], friendlies: Unit[]): number {
    if (enemies.length === 0) return 0;
    if (friendlies.length === 0) return 100;

    let threatScore = 0;
    
    for (const enemy of enemies) {
      const distanceFactor = this.getDistanceToClosestFriendly(enemy, friendlies);
      const healthFactor = enemy.health / enemy.maxHealth;
      const damageFactor = enemy.stats.attackDamage / 50;
      
      threatScore += (damageFactor * healthFactor) / Math.max(1, distanceFactor / 100);
    }
    
    const friendlyDisadvantage = friendlies.length / Math.max(1, enemies.length);
    threatScore *= friendlyDisadvantage;
    
    return Math.min(100, Math.max(0, threatScore));
  }

  private getDistanceToClosestFriendly(enemy: Unit, friendlies: Unit[]): number {
    let minDist = Infinity;
    for (const friendly of friendlies) {
      const dist = enemy.getDistanceTo(friendly);
      if (dist < minDist) minDist = dist;
    }
    return minDist;
  }

  private calculateAdvantageLevel(friendly: Unit[], enemy: Unit[]): number {
    if (friendly.length === 0) return -100;
    if (enemy.length === 0) return 100;

    const friendlyPower = this.calculateTotalFirepower(friendly);
    const enemyPower = this.calculateTotalFirepower(enemy);
    
    if (friendlyPower === 0 && enemyPower === 0) return 0;
    if (enemyPower === 0) return 100;
    if (friendlyPower === 0) return -100;
    
    const ratio = friendlyPower / enemyPower;
    return (ratio - 1) * 50;
  }

  private analyzeTerrainFeatures(position: Vector2D, structures: Building[]): Map<TerrainFeature, number> {
    const features = new Map<TerrainFeature, number>();
    
    features.set(TerrainFeature.ELEVATION, this.assessElevation(position));
    features.set(TerrainFeature.COVER, this.assessCover(position));
    features.set(TerrainFeature.CHOKEPOINT, this.assessChokepoint(position, structures));
    features.set(TerrainFeature.HIGH_GROUND, this.assessHighGround(position));
    features.set(TerrainFeature.FLANKING, this.assessFlanking(position, structures));
    features.set(TerrainFeature.RETREAT_PATH, this.assessRetreatPaths(position));
    features.set(TerrainFeature.DEFENSIBLE, this.assessDefensibility(position, structures));
    
    return features;
  }

  private assessElevation(position: Vector2D): number {
    return Math.random() * 0.3 + 0.5;
  }

  private assessCover(position: Vector2D): number {
    return Math.random() * 0.4 + 0.3;
  }

  private assessChokepoint(position: Vector2D, structures: Building[]): number {
    const nearbyStructures = structures.filter(s => this.isNearby(position, s.position, 200));
    return Math.min(1, nearbyStructures.length * 0.2);
  }

  private assessHighGround(position: Vector2D): number {
    const elevation = this.assessElevation(position);
    return elevation > 0.6 ? 0.8 : 0.2;
  }

  private assessFlanking(position: Vector2D, structures: Building[]): number {
    return Math.random() * 0.5 + 0.3;
  }

  private assessRetreatPaths(position: Vector2D): number {
    return Math.random() * 0.6 + 0.2;
  }

  private assessDefensibility(position: Vector2D, structures: Building[]): number {
    const cover = this.assessCover(position);
    const chokepoint = this.assessChokepoint(position, structures);
    return (cover * 0.4 + chokepoint * 0.6);
  }

  private identifyObjectives(friendly: Unit[], enemy: Unit[]): TacticalObjective[] {
    const objectives: TacticalObjective[] = [];
    
    if (enemy.length > friendly.length * 1.5) {
      objectives.push(TacticalObjective.ELIMINATE_THREAT);
    }
    
    if (this.hasHighValueTargets(enemy)) {
      objectives.push(TacticalObjective.DESTROY_ENEMY);
    }
    
    const hasSiegeUnits = friendly.some(u => u.unitType.includes('siege'));
    const hasStructures = enemy.some(e => e.type === 'building');
    
    if (hasSiegeUnits && hasStructures) {
      objectives.push(TacticalObjective.SIEGE_STRUCTURE);
    }
    
    return objectives;
  }

  private hasHighValueTargets(enemies: Unit[]): boolean {
    return enemies.some(e => e.stats.attackDamage > 50 || e.stats.health > 300);
  }

  private calculateTimeInCombat(units: Unit[]): number {
    return Math.random() * 30;
  }

  private estimateReinforcements(units: Unit[]): number {
    return Math.floor(Math.random() * 5);
  }

  private calculateTotalFirepower(units: Unit[]): number {
    return units.reduce((total, unit) => {
      const healthFactor = unit.health / unit.maxHealth;
      const damage = unit.stats.attackDamage * unit.stats.attackSpeed;
      return total + damage * healthFactor;
    }, 0);
  }

  private calculateAverageHealth(units: Unit[]): number {
    if (units.length === 0) return 0;
    const totalHealth = units.reduce((sum, u) => sum + (u.health / u.maxHealth), 0);
    return (totalHealth / units.length) * 100;
  }

  private isNearby(pos1: Vector2D, pos2: Vector2D, range: number): boolean {
    const dx = pos2.x - pos1.x;
    const dy = pos2.y - pos1.y;
    return Math.sqrt(dx * dx + dy * dy) <= range;
  }

  public calculateTacticalScore(situation: TacticalSituation): TacticalScore {
    const factors = new Map<string, number>();
    
    const damagePotential = this.evaluateDamagePotential(situation);
    factors.set('damage_potential', damagePotential);
    
    const survivalChance = this.evaluateSurvivalChance(situation);
    factors.set('survival_chance', survivalChance);
    
    const positionalAdvantage = this.evaluatePositionalAdvantage(situation);
    factors.set('positional_advantage', positionalAdvantage);
    
    const resourceEfficiency = this.evaluateResourceEfficiency(situation);
    factors.set('resource_efficiency', resourceEfficiency);
    
    const objectiveProgress = this.evaluateObjectiveProgress(situation);
    factors.set('objective_progress', objectiveProgress);
    
    const riskAssessment = this.evaluateRiskAssessment(situation);
    factors.set('risk_assessment', riskAssessment);
    
    const overall = (
      damagePotential * 0.25 +
      survivalChance * 0.20 +
      positionalAdvantage * 0.20 +
      resourceEfficiency * 0.15 +
      objectiveProgress * 0.10 +
      riskAssessment * 0.10
    );
    
    return {
      overall,
      damagePotential,
      survivalChance,
      positionalAdvantage,
      resourceEfficiency,
      objectiveProgress,
      riskAssessment,
      factors,
      timestamp: Date.now()
    };
  }

  private evaluateDamagePotential(situation: TacticalSituation): number {
    const friendlyPower = situation.overallFirepower.friendly;
    const enemyPower = situation.overallFirepower.enemy;
    
    if (friendlyPower === 0) return 0;
    
    const ratio = friendlyPower / Math.max(1, enemyPower);
    const healthBonus = situation.averageHealth.friendly / 100;
    
    return Math.min(100, ratio * 50 * healthBonus);
  }

  private evaluateSurvivalChance(situation: TacticalSituation): number {
    const healthFactor = situation.averageHealth.friendly;
    const threatFactor = 100 - situation.threatLevel;
    const advantageFactor = (situation.advantageLevel + 100) / 2;
    
    return (healthFactor * 0.4 + threatFactor * 0.3 + advantageFactor * 0.3);
  }

  private evaluatePositionalAdvantage(situation: TacticalSituation): number {
    const terrainScore = Array.from(situation.terrainFeatures.values())
      .reduce((sum, val) => sum + val, 0) / situation.terrainFeatures.size;
    
    const formationBonus = situation.friendlyUnits.length > situation.enemyUnits.length ? 10 : 0;
    
    return Math.min(100, terrainScore * 100 + formationBonus);
  }

  private evaluateResourceEfficiency(situation: TacticalSituation): number {
    const friendlyLosses = situation.enemyUnits.length > 0 ? 
      (situation.friendlyUnits.length / situation.enemyUnits.length) : 100;
    const damageRatio = situation.overallFirepower.friendly / 
      Math.max(1, situation.overallFirepower.friendly + situation.overallFirepower.enemy);
    
    return Math.min(100, friendlyLosses * 50 + damageRatio * 50);
  }

  private evaluateObjectiveProgress(situation: TacticalSituation): number {
    return Math.random() * 40 + 20;
  }

  private evaluateRiskAssessment(situation: TacticalSituation): number {
    const riskFactors = [
      situation.threatLevel,
      100 - situation.averageHealth.friendly,
      situation.estimatedEnemyReinforcements * 10
    ];
    
    const avgRisk = riskFactors.reduce((a, b) => a + b, 0) / riskFactors.length;
    return 100 - avgRisk;
  }

  public getCombatMetrics(): CombatMetrics {
    const current = this.historicalMetrics[this.historicalMetrics.length - 1];
    return { ...current };
  }

  public recordCombatMetrics(metrics: Partial<CombatMetrics>): void {
    const last = this.historicalMetrics[this.historicalMetrics.length - 1];
    const updated: CombatMetrics = {
      ...last,
      ...metrics,
      abilitiesOnCooldown: new Map([...last.abilitiesOnCooldown, ...(metrics.abilitiesOnCooldown || new Map())])
    };
    this.historicalMetrics.push(updated);
    
    if (this.historicalMetrics.length > 100) {
      this.historicalMetrics.shift();
    }
  }

  public clearCache(): void {
    this.situationCache.clear();
    this.threatAssessmentCache.clear();
  }
}

export class TargetSelector {
  private selectionMode: TargetSelectionMode = TargetSelectionMode.COMBINED;
  private priorityWeights: Map<string, number> = new Map();
  private preferredTargets: Set<string> = new Set();
  private avoidedTargets: Set<string> = new Set();
  private recentlyTargeted: Map<string, number> = new Map();
  private targetHistory: Map<string, TargetHistoryEntry> = new Map();

  constructor() {
    this.initializeDefaultWeights();
  }

  private initializeDefaultWeights(): void {
    this.priorityWeights.set('threat', 0.3);
    this.priorityWeights.set('value', 0.25);
    this.priorityWeights.set('distance', 0.25);
    this.priorityWeights.set('health', 0.2);
  }

  public selectTarget(
    attacker: Unit,
    potentialTargets: (Unit | Building)[],
    situation: TacticalSituation
  ): Unit | Building | null {
    if (potentialTargets.length === 0) return null;

    const availableTargets = potentialTargets.filter(t => this.isValidTarget(attacker, t));
    
    if (availableTargets.length === 0) return null;

    let bestTarget: Unit | Building | null = null;
    let bestScore = -Infinity;

    for (const target of availableTargets) {
      const score = this.calculateTargetScore(attacker, target, situation);
      
      if (score > bestScore) {
        bestScore = score;
        bestTarget = target;
      }
    }

    if (bestTarget) {
      this.recordTargetSelection(attacker.id, bestTarget.id);
    }

    return bestTarget;
  }

  private isValidTarget(attacker: Unit, target: Unit | Building): boolean {
    if (!target.isAlive()) return false;
    
    if (target instanceof Unit) {
      if (!attacker.stats.canAttack) return false;
    }
    
    const distance = attacker.getDistanceTo(target);
    if (distance > attacker.stats.attackRange * 1.2) return false;
    
    if (this.avoidedTargets.has(target.id)) return false;
    
    return true;
  }

  private calculateTargetScore(
    attacker: Unit,
    target: Unit | Building,
    situation: TacticalSituation
  ): number {
    const threatScore = this.calculateThreatScore(attacker, target);
    const valueScore = this.calculateValueScore(target);
    const distanceScore = this.calculateDistanceScore(attacker, target);
    const healthScore = this.calculateHealthScore(target);
    
    const threatWeight = this.priorityWeights.get('threat') || 0.3;
    const valueWeight = this.priorityWeights.get('value') || 0.25;
    const distanceWeight = this.priorityWeights.get('distance') || 0.25;
    const healthWeight = this.priorityWeights.get('health') || 0.2;
    
    let totalScore = 
      threatScore * threatWeight +
      valueScore * valueWeight +
      distanceScore * distanceWeight +
      healthScore * healthWeight;
    
    if (this.preferredTargets.has(target.id)) {
      totalScore *= 1.3;
    }
    
    const recentTargetPenalty = this.recentlyTargeted.get(target.id) || 0;
    if (Date.now() - recentTargetPenalty < 5000) {
      totalScore *= 0.8;
    }
    
    const historyEntry = this.targetHistory.get(target.id);
    if (historyEntry) {
      if (historyEntry.kills >= 3) {
        totalScore *= 1.2;
      }
      if (historyEntry.timesTargeted > 5) {
        totalScore *= 0.7;
      }
    }
    
    return totalScore;
  }

  private calculateThreatScore(attacker: Unit, target: Unit | Building): number {
    if (!(target instanceof Unit)) return 50;
    
    const damageThreat = target.stats.attackDamage / 100;
    const rangeThreat = target.stats.attackRange / attacker.stats.attackRange;
    const healthThreat = target.health / target.maxHealth;
    
    return Math.min(100, (damageThreat * 40 + rangeThreat * 30 + healthThreat * 30));
  }

  private calculateValueScore(target: Unit | Building): number {
    if (target instanceof Building) {
      return 80 + target.structureType.includes('core') ? 20 : 0;
    }
    
    const valueScore = (target.stats.attackDamage * 0.3 + 
                       target.stats.health * 0.2 + 
                       target.stats.cost.credits * 0.5);
    
    return Math.min(100, valueScore);
  }

  private calculateDistanceScore(attacker: Unit, target: Unit | Building): number {
    const distance = attacker.getDistanceTo(target);
    const optimalRange = attacker.stats.attackRange * 0.8;
    
    const distanceFactor = 1 - Math.abs(distance - optimalRange) / attacker.stats.attackRange;
    return Math.max(0, Math.min(100, distanceFactor * 100));
  }

  private calculateHealthScore(target: Unit | Building): number {
    const healthPercent = target.health / target.maxHealth;
    
    if (healthPercent <= 0.3) return 90;
    if (healthPercent <= 0.5) return 70;
    if (healthPercent <= 0.7) return 50;
    return 30;
  }

  private recordTargetSelection(attackerId: string, targetId: string): void {
    this.recentlyTargeted.set(targetId, Date.now());
    
    let history = this.targetHistory.get(targetId);
    if (!history) {
      history = {
        targetId,
        timesTargeted: 0,
        kills: 0,
        damageDealt: 0,
        lastTargeted: 0
      };
      this.targetHistory.set(targetId, history);
    }
    
    history.timesTargeted++;
    history.lastTargeted = Date.now();
  }

  public recordKill(targetId: string): void {
    const history = this.targetHistory.get(targetId);
    if (history) {
      history.kills++;
    }
  }

  public setSelectionMode(mode: TargetSelectionMode): void {
    this.selectionMode = mode;
    
    switch (mode) {
      case TargetSelectionMode.THREAT:
        this.priorityWeights.set('threat', 0.5);
        this.priorityWeights.set('value', 0.15);
        this.priorityWeights.set('distance', 0.2);
        this.priorityWeights.set('health', 0.15);
        break;
      case TargetSelectionMode.VALUE:
        this.priorityWeights.set('threat', 0.2);
        this.priorityWeights.set('value', 0.4);
        this.priorityWeights.set('distance', 0.2);
        this.priorityWeights.set('health', 0.2);
        break;
      case TargetSelectionMode.DISTANCE:
        this.priorityWeights.set('threat', 0.2);
        this.priorityWeights.set('value', 0.2);
        this.priorityWeights.set('distance', 0.4);
        this.priorityWeights.set('health', 0.2);
        break;
      case TargetSelectionMode.PRIORITY:
        this.priorityWeights.set('threat', 0.25);
        this.priorityWeights.set('value', 0.25);
        this.priorityWeights.set('distance', 0.25);
        this.priorityWeights.set('health', 0.25);
        break;
      case TargetSelectionMode.COMBINED:
        this.initializeDefaultWeights();
        break;
    }
  }

  public addPreferredTarget(targetId: string): void {
    this.preferredTargets.add(targetId);
  }

  public removePreferredTarget(targetId: string): void {
    this.preferredTargets.delete(targetId);
  }

  public addAvoidedTarget(targetId: string): void {
    this.avoidedTargets.add(targetId);
  }

  public removeAvoidedTarget(targetId: string): void {
    this.avoidedTargets.delete(targetId);
  }

  public getTargetHistory(targetId: string): TargetHistoryEntry | undefined {
    return this.targetHistory.get(targetId);
  }

  public clearRecentTargets(): void {
    this.recentlyTargeted.clear();
  }

  public clearHistory(): void {
    this.targetHistory.clear();
    this.recentlyTargeted.clear();
  }
}

interface TargetHistoryEntry {
  targetId: string;
  timesTargeted: number;
  kills: number;
  damageDealt: number;
  lastTargeted: number;
}

export class FormationManager {
  private formations: Map<FormationType, FormationTemplate> = new Map();
  private activeFormations: Map<string, FormationAssignment> = new Map();
  private formationHistory: FormationAssignment[] = [];

  constructor() {
    this.initializeDefaultFormations();
  }

  private initializeDefaultFormations(): void {
    this.formations.set(FormationType.LINE, {
      type: FormationType.LINE,
      name: 'Line Formation',
      description: 'Units spread in a horizontal line, optimal for ranged units',
      unitSlots: this.generateLineSlots(8),
      spacing: 30,
      orientation: 'forward',
      flexibility: 0.3,
      recommendedUnits: [UnitRole.RANGED, UnitRole.DAMAGE_DEALER],
      strengths: ['Ranged superiority', 'Wide engagement front'],
      weaknesses: ['Vulnerable to flanking', 'Weak in melee'],
      optimalTerrain: [TerrainFeature.HIGH_GROUND, TerrainFeature.DEFENSIBLE]
    });

    this.formations.set(FormationType.WEDGE, {
      type: FormationType.WEDGE,
      name: 'Wedge Formation',
      description: 'Arrow-shaped formation for breaking enemy lines',
      unitSlots: this.generateWedgeSlots(10),
      spacing: 25,
      orientation: 'forward',
      flexibility: 0.5,
      recommendedUnits: [UnitRole.MELEE, UnitRole.TANK, UnitRole.DAMAGE_DEALER],
      strengths: ['High damage concentration', 'Good for charging'],
      weaknesses: ['Vulnerable sides', 'Requires good positioning'],
      optimalTerrain: [TerrainFeature.FLANKING, TerrainFeature.ELEVATION]
    });

    this.formations.set(FormationType.CIRCLE, {
      type: FormationType.CIRCLE,
      name: 'Circle Formation',
      description: 'Defensive formation with all units facing outward',
      unitSlots: this.generateCircleSlots(8),
      spacing: 20,
      orientation: 'lateral',
      flexibility: 0.2,
      recommendedUnits: [UnitRole.TANK, UnitRole.SUPPORT],
      strengths: ['All-around defense', 'Good for holding position'],
      weaknesses: ['Limited offensive capability', 'Stationary'],
      optimalTerrain: [TerrainFeature.CHOKEPOINT, TerrainFeature.DEFENSIBLE]
    });

    this.formations.set(FormationType.SQUARE, {
      type: FormationType.SQUARE,
      name: 'Square Formation',
      description: 'Rigid defensive formation for protecting center',
      unitSlots: this.generateSquareSlots(12),
      spacing: 25,
      orientation: 'lateral',
      flexibility: 0.1,
      recommendedUnits: [UnitRole.TANK, UnitRole.MELEE],
      strengths: ['Maximum defense', 'Protects ranged units inside'],
      weaknesses: ['Very inflexible', 'Slow movement'],
      optimalTerrain: [TerrainFeature.CHOKEPOINT, TerrainFeature.COVER]
    });

    this.formations.set(FormationType.FLANK, {
      type: FormationType.FLANK,
      name: 'Flanking Formation',
      description: 'Split formation for simultaneous attacks from multiple angles',
      unitSlots: this.generateFlankSlots(10),
      spacing: 35,
      orientation: 'lateral',
      flexibility: 0.7,
      recommendedUnits: [UnitRole.DAMAGE_DEALER, UnitRole.SCOUT, UnitRole.AIR],
      strengths: ['Surprise attacks', 'Multiple threat vectors'],
      weaknesses: ['Coordination required', 'Split focus'],
      optimalTerrain: [TerrainFeature.FLANKING, TerrainFeature.COVER]
    });

    this.formations.set(FormationType.DEFENSIVE, {
      type: FormationType.DEFENSIVE,
      name: 'Defensive Formation',
      description: 'Spread formation optimized for defense',
      unitSlots: this.generateDefensiveSlots(8),
      spacing: 40,
      orientation: 'backward',
      flexibility: 0.4,
      recommendedUnits: [UnitRole.TANK, UnitRole.SUPPORT, UnitRole.RANGED],
      strengths: ['Strong defense', 'Good for counterattacks'],
      weaknesses: ['Slow damage output', 'Requires good position'],
      optimalTerrain: [TerrainFeature.DEFENSIBLE, TerrainFeature.COVER]
    });
  }

  private generateLineSlots(count: number): FormationSlot[] {
    const slots: FormationSlot[] = [];
    const halfCount = Math.floor(count / 2);
    
    for (let i = 0; i < count; i++) {
      slots.push({
        index: i,
        relativePosition: {
          x: (i - halfCount) * 30,
          y: 0
        },
        preferredRole: UnitRole.RANGED,
        isFlexible: true,
        requiredUnitCount: 1
      });
    }
    return slots;
  }

  private generateWedgeSlots(count: number): FormationSlot[] {
    const slots: FormationSlot[] = [];
    const rows = Math.ceil(Math.sqrt(count));
    let slotIndex = 0;
    
    for (let row = 0; row < rows && slotIndex < count; row++) {
      const unitsInRow = row + 1;
      for (let col = 0; col < unitsInRow && slotIndex < count; col++) {
        slots.push({
          index: slotIndex++,
          relativePosition: {
            x: (col - unitsInRow / 2) * 25,
            y: row * 25
          },
          preferredRole: row === 0 ? UnitRole.MELEE : UnitRole.TANK,
          isFlexible: row > 0,
          requiredUnitCount: 1
        });
      }
    }
    return slots;
  }

  private generateCircleSlots(count: number): FormationSlot[] {
    const slots: FormationSlot[] = [];
    
    for (let i = 0; i < count; i++) {
      const angle = (i / count) * Math.PI * 2;
      slots.push({
        index: i,
        relativePosition: {
          x: Math.cos(angle) * 50,
          y: Math.sin(angle) * 50
        },
        preferredRole: UnitRole.TANK,
        isFlexible: false,
        requiredUnitCount: 1
      });
    }
    return slots;
  }

  private generateSquareSlots(count: number): FormationSlot[] {
    const slots: FormationSlot[] = [];
    const sideLength = Math.ceil(Math.sqrt(count));
    const halfSide = Math.floor(sideLength / 2);
    let slotIndex = 0;
    
    for (let row = 0; row < sideLength && slotIndex < count; row++) {
      for (let col = 0; col < sideLength && slotIndex < count; col++) {
        const isEdge = row === 0 || row === sideLength - 1 || col === 0 || col === sideLength - 1;
        if (isEdge) {
          slots.push({
            index: slotIndex++,
            relativePosition: {
              x: (col - halfSide) * 25,
              y: (row - halfSide) * 25
            },
            preferredRole: UnitRole.MELEE,
            isFlexible: false,
            requiredUnitCount: 1
          });
        }
      }
    }
    return slots;
  }

  private generateFlankSlots(count: number): FormationSlot[] {
    const slots: FormationSlot[] = [];
    const halfCount = Math.floor(count / 2);
    
    for (let i = 0; i < count; i++) {
      const side = i < halfCount ? -1 : 1;
      const rowIndex = i % halfCount;
      slots.push({
        index: i,
        relativePosition: {
          x: side * 60,
          y: (rowIndex - halfCount / 2) * 30
        },
        preferredRole: UnitRole.DAMAGE_DEALER,
        isFlexible: true,
        requiredUnitCount: 1
      });
    }
    return slots;
  }

  private generateDefensiveSlots(count: number): FormationSlot[] {
    const slots: FormationSlot[] = [];
    
    for (let i = 0; i < count; i++) {
      const angle = (i / count) * Math.PI * 2 + Math.PI;
      const distance = 40 + Math.random() * 20;
      slots.push({
        index: i,
        relativePosition: {
          x: Math.cos(angle) * distance,
          y: Math.sin(angle) * distance
        },
        preferredRole: UnitRole.SUPPORT,
        isFlexible: true,
        requiredUnitCount: 1
      });
    }
    return slots;
  }

  public createFormation(
    type: FormationType,
    center: Vector2D,
    units: Unit[],
    facingDirection: Vector2D
  ): FormationAssignment {
    const template = this.formations.get(type);
    if (!template) {
      throw new Error(`Formation type ${type} not found`);
    }

    const sortedUnits = this.assignUnitsToSlots(units, template.unitSlots);
    const positions = this.calculateFormationPositions(template, center, facingDirection);
    
    const assignment: FormationAssignment = {
      id: `formation_${Date.now()}`,
      type,
      center,
      units: sortedUnits.map(u => u.id),
      positions,
      facingDirection,
      flexibility: template.flexibility,
      isActive: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    };

    this.activeFormations.set(assignment.id, assignment);
    return assignment;
  }

  private assignUnitsToSlots(units: Unit[], slots: FormationSlot[]): Unit[] {
    const sortedUnits = [...units].sort((a, b) => {
      const aDamage = a.stats.attackDamage;
      const bDamage = b.stats.attackDamage;
      return bDamage - aDamage;
    });

    const assignments: Unit[] = [];
    const usedSlots = new Set<number>();

    for (const unit of sortedUnits) {
      let bestSlot = -1;
      let bestScore = -Infinity;

      for (let i = 0; i < slots.length; i++) {
        if (usedSlots.has(i)) continue;

        const slot = slots[i];
        const roleMatch = this.getRoleMatchScore(unit, slot.preferredRole);
        
        if (roleMatch > bestScore) {
          bestScore = roleMatch;
          bestSlot = i;
        }
      }

      if (bestSlot !== -1) {
        usedSlots.add(bestSlot);
        assignments.push(unit);
      }
    }

    return assignments;
  }

  private getRoleMatchScore(unit: Unit, preferredRole: UnitRole): number {
    const unitRole = this.inferUnitRole(unit);
    
    if (unitRole === preferredRole) return 100;
    
    const roleCompatibility: Record<UnitRole, UnitRole[]> = {
      [UnitRole.TANK]: [UnitRole.MELEE, UnitRole.SUPPORT],
      [UnitRole.DAMAGE_DEALER]: [UnitRole.RANGED, UnitRole.MELEE],
      [UnitRole.SUPPORT]: [UnitRole.TANK],
      [UnitRole.SCOUT]: [UnitRole.AIR],
      [UnitRole.RANGED]: [UnitRole.DAMAGE_DEALER],
      [UnitRole.MELEE]: [UnitRole.DAMAGE_DEALER, UnitRole.TANK],
      [UnitRole.SIEGE]: [UnitRole.RANGED],
      [UnitRole.AIR]: [UnitRole.SCOUT]
    };

    const compatible = roleCompatibility[unitRole] || [];
    return compatible.includes(preferredRole) ? 70 : 30;
  }

  private inferUnitRole(unit: Unit): UnitRole {
    if (unit.stats.isAir) return UnitRole.AIR;
    if (unit.unitType.includes('tank') || unit.stats.armor > 5) return UnitRole.TANK;
    if (unit.stats.attackRange > 200) return UnitRole.SIEGE;
    if (unit.stats.attackRange > 100) return UnitRole.RANGED;
    if (unit.stats.attackDamage > 30) return UnitRole.DAMAGE_DEALER;
    if (unit.stats.moveSpeed > 60) return UnitRole.SCOUT;
    if (unit.stats.attackDamage === 0) return UnitRole.SUPPORT;
    return UnitRole.MELEE;
  }

  private calculateFormationPositions(
    template: FormationTemplate,
    center: Vector2D,
    facing: Vector2D
  ): Vector2D[] {
    const positions: Vector2D[] = [];
    const angle = Math.atan2(facing.y, facing.x);
    
    for (const slot of template.unitSlots) {
      const rotated = this.rotateVector(slot.relativePosition, angle);
      positions.push({
        x: center.x + rotated.x,
        y: center.y + rotated.y
      });
    }
    
    return positions;
  }

  private rotateVector(vec: Vector2D, angle: number): Vector2D {
    const cos = Math.cos(angle);
    const sin = Math.sin(angle);
    return {
      x: vec.x * cos - vec.y * sin,
      y: vec.x * sin + vec.y * cos
    };
  }

  public maintainFormation(
    formationId: string,
    units: Unit[],
    newCenter: Vector2D,
    newFacing: Vector2D
  ): FormationAssignment | null {
    const formation = this.activeFormations.get(formationId);
    if (!formation) return null;

    formation.center = newCenter;
    formation.facingDirection = newFacing;
    formation.positions = this.calculateFormationPositions(
      this.formations.get(formation.type)!,
      newCenter,
      newFacing
    );
    formation.lastUpdated = Date.now();

    return formation;
  }

  public breakFormation(formationId: string): void {
    this.activeFormations.delete(formationId);
  }

  public getFormation(formationId: string): FormationAssignment | undefined {
    return this.activeFormations.get(formationId);
  }

  public getActiveFormations(): FormationAssignment[] {
    return Array.from(this.activeFormations.values());
  }

  public getFormationTemplate(type: FormationType): FormationTemplate | undefined {
    return this.formations.get(type);
  }

  public registerCustomFormation(template: FormationTemplate): void {
    this.formations.set(template.type, template);
  }

  public evaluateFormationEffectiveness(
    formationId: string,
    situation: TacticalSituation
  ): number {
    const formation = this.activeFormations.get(formationId);
    if (!formation) return 0;

    const template = this.formations.get(formation.type);
    if (!template) return 0;

    let effectiveness = 50;

    const terrainMatch = this.countTerrainMatches(situation, template.optimalTerrain);
    effectiveness += terrainMatch * 10;

    const unitRoleDistribution = this.evaluateUnitRoles(
      formation.units.map(id => situation.friendlyUnits.find(u => u.id === id)).filter(u => u),
      template.recommendedUnits
    );
    effectiveness += unitRoleDistribution * 30;

    const coverage = this.evaluateCoverage(formation, situation);
    effectiveness += coverage * 20;

    return Math.min(100, Math.max(0, effectiveness));
  }

  private countTerrainMatches(situation: TacticalSituation, optimal: TerrainFeature[]): number {
    let matches = 0;
    for (const terrain of optimal) {
      const value = situation.terrainFeatures.get(terrain) || 0;
      if (value > 0.5) matches++;
    }
    return matches;
  }

  private evaluateUnitRoles(units: Unit[], recommended: UnitRole[]): number {
    if (units.length === 0) return 0;
    
    let matches = 0;
    for (const unit of units) {
      const role = this.inferUnitRole(unit);
      if (recommended.includes(role)) matches++;
    }
    
    return matches / units.length;
  }

  private evaluateCoverage(formation: FormationAssignment, situation: TacticalSituation): number {
    const minX = Math.min(...formation.positions.map(p => p.x));
    const maxX = Math.max(...formation.positions.map(p => p.x));
    const minY = Math.min(...formation.positions.map(p => p.y));
    const maxY = Math.max(...formation.positions.map(p => p.y));
    
    const coverage = (maxX - minX) * (maxY - minY);
    const enemyArea = situation.enemyUnits.length * 5000;
    
    return Math.min(1, coverage / enemyArea);
  }
}

interface FormationAssignment {
  id: string;
  type: FormationType;
  center: Vector2D;
  units: string[];
  positions: Vector2D[];
  facingDirection: Vector2D;
  flexibility: number;
  isActive: boolean;
  createdAt: number;
  lastUpdated: number;
}

export class FlankingManager {
  private flankingHistory: Map<string, FlankingAttempt[]> = new Map();
  private currentManeuvers: Map<string, FlankingManeuver> = new Map();
  private preferredAngles: Map<string, number> = new Map();

  constructor() {
    this.initializeDefaultAngles();
  }

  private initializeDefaultAngles(): void {
    this.preferredAngles.set('aggressive', 45);
    this.preferredAngles.set('balanced', 90);
    this.preferredAngles.set('cautious', 135);
  }

  public planFlankingManeuver(
    units: Unit[],
    enemyCenter: Vector2D,
    friendlyCenter: Vector2D,
    terrainFeatures: Map<TerrainFeature, number>
  ): FlankingManeuver | null {
    if (units.length < 2) return null;

    const flankPositions = this.calculateFlankPositions(
      units,
      enemyCenter,
      friendlyCenter,
      terrainFeatures
    );

    if (flankPositions.length === 0) return null;

    const bestFlank = this.selectBestFlankPosition(flankPositions, terrainFeatures);
    
    const maneuver: FlankingManeuver = {
      id: `flank_${Date.now()}`,
      flankPositions: bestFlank.positions,
      approachVector: bestFlank.approachVector,
      targetPoint: bestFlank.targetPoint,
      assignedUnits: units.map(u => u.id),
      status: 'planning',
      successProbability: this.calculateSuccessProbability(bestFlank, units, terrainFeatures),
      expectedEngagementTime: this.estimateEngagementTime(units, bestFlank),
      risks: this.identifyFlankingRisks(bestFlank, units),
      contingencies: this.createFlankingContingencies(bestFlank)
    };

    this.currentManeuvers.set(maneuver.id, maneuver);
    return maneuver;
  }

  private calculateFlankPositions(
    units: Unit[],
    enemyCenter: Vector2D,
    friendlyCenter: Vector2D,
    terrainFeatures: Map<TerrainFeature, number>
  ): FlankingPosition[] {
    const positions: FlankingPosition[] = [];
    const baseAngle = Math.atan2(
      enemyCenter.y - friendlyCenter.y,
      enemyCenter.x - friendlyCenter.x
    );

    const flankingScores = terrainFeatures.get(TerrainFeature.FLANKING) || 0.5;
    const angles = [-90, -45, 45, 90];

    for (const angleOffset of angles) {
      const angle = baseAngle + (angleOffset * Math.PI / 180);
      const distance = 150 + Math.random() * 50;
      
      const flankCenter = {
        x: enemyCenter.x + Math.cos(angle) * distance,
        y: enemyCenter.y + Math.sin(angle) * distance
      };

      const targetPoint = this.calculateAttackPoint(enemyCenter, flankCenter, units.length);

      const approachVector = this.normalizeVector({
        x: flankCenter.x - friendlyCenter.x,
        y: flankCenter.y - friendlyCenter.y
      });

      positions.push({
        center: flankCenter,
        approachVector,
        targetPoint,
        angleOffset,
        coverQuality: this.assessCoverAlongPath(friendlyCenter, flankCenter, terrainFeatures),
        exposureRisk: this.assessExposureRisk(flankCenter, enemyCenter)
      });
    }

    return positions.sort((a, b) => {
      const scoreA = a.coverQuality * 0.6 - a.exposureRisk * 0.4;
      const scoreB = b.coverQuality * 0.6 - b.exposureRisk * 0.4;
      return scoreB - scoreA;
    });
  }

  private calculateAttackPoint(
    enemyCenter: Vector2D,
    flankCenter: Vector2D,
    unitCount: number
  ): Vector2D {
    const spreadFactor = Math.min(unitCount * 10, 80);
    return {
      x: enemyCenter.x + (flankCenter.x - enemyCenter.x) * 0.3 + (Math.random() - 0.5) * spreadFactor,
      y: enemyCenter.y + (flankCenter.y - enemyCenter.y) * 0.3 + (Math.random() - 0.5) * spreadFactor
    };
  }

  private normalizeVector(vec: Vector2D): Vector2D {
    const length = Math.sqrt(vec.x * vec.x + vec.y * vec.y);
    if (length === 0) return { x: 0, y: 0 };
    return { x: vec.x / length, y: vec.y / length };
  }

  private assessCoverAlongPath(
    start: Vector2D,
    end: Vector2D,
    terrainFeatures: Map<TerrainFeature, number>
  ): number {
    const coverQuality = terrainFeatures.get(TerrainFeature.COVER) || 0.5;
    const distance = Math.sqrt(
      Math.pow(end.x - start.x, 2) + Math.pow(end.y - start.y, 2)
    );
    
    const coverMultiplier = 1 + coverQuality * (distance / 300);
    return Math.min(1, coverMultiplier);
  }

  private assessExposureRisk(flankPosition: Vector2D, enemyCenter: Vector2D): number {
    const distance = Math.sqrt(
      Math.pow(flankPosition.x - enemyCenter.x, 2) +
      Math.pow(flankPosition.y - enemyCenter.y, 2)
    );
    
    if (distance < 100) return 0.8;
    if (distance < 200) return 0.5;
    return 0.2;
  }

  private selectBestFlankPosition(
    positions: FlankingPosition[],
    terrainFeatures: Map<TerrainFeature, number>
  ): FlankingPosition {
    let bestPosition = positions[0];
    let bestScore = -Infinity;

    for (const pos of positions) {
      const terrainBonus = (terrainFeatures.get(TerrainFeature.COVER) || 0) * 30;
      const exposurePenalty = pos.exposureRisk * 40;
      const coverBonus = pos.coverQuality * 30;
      
      const score = terrainBonus + coverBonus - exposurePenalty;
      
      if (score > bestScore) {
        bestScore = score;
        bestPosition = pos;
      }
    }

    return bestPosition;
  }

  private calculateSuccessProbability(
    position: FlankingPosition,
    units: Unit[],
    terrainFeatures: Map<TerrainFeature, number>
  ): number {
    let probability = 50;

    probability += position.coverQuality * 30;
    probability -= position.exposureRisk * 25;

    const unitSpeed = units.reduce((sum, u) => sum + u.stats.moveSpeed, 0) / units.length;
    if (unitSpeed > 50) probability += 10;
    else probability -= 10;

    const flankingBonus = terrainFeatures.get(TerrainFeature.FLANKING) || 0;
    probability += flankingBonus * 15;

    return Math.min(95, Math.max(5, probability));
  }

  private estimateEngagementTime(units: Unit[], position: FlankingPosition): number {
    const avgMoveSpeed = units.reduce((sum, u) => sum + u.stats.moveSpeed, 0) / units.length;
    const distance = Math.sqrt(
      Math.pow(position.targetPoint.x - position.center.x, 2) +
      Math.pow(position.targetPoint.y - position.center.y, 2)
    );
    
    const moveTime = distance / avgMoveSpeed;
    const formationTime = units.length * 0.5;
    
    return moveTime + formationTime;
  }

  private identifyFlankingRisks(position: FlankingPosition, units: Unit[]): FlankingRisk[] {
    const risks: FlankingRisk[] = [];

    if (position.exposureRisk > 0.5) {
      risks.push({
        type: 'exposure',
        severity: position.exposureRisk,
        description: 'High exposure to enemy fire during approach',
        mitigation: 'Use cover or wait for distraction'
      });
    }

    if (units.length < 3) {
      risks.push({
        type: 'insufficient_force',
        severity: 0.4,
        description: 'Limited units for effective flanking',
        mitigation: 'Request reinforcements'
      });
    }

    risks.push({
      type: 'timing',
      severity: 0.3,
      description: 'Attack timing must coordinate with main force',
      mitigation: 'Establish communication protocol'
    });

    return risks;
  }

  private createFlankingContingencies(position: FlankingPosition): FlankingContingency[] {
    return [
      {
        trigger: 'enemy_detected_approach',
        action: 'abort_and_reposition',
        priority: 1,
        alternativePositions: this.findAlternativePositions(position)
      },
      {
        trigger: 'main_force_delayed',
        action: 'hold_position',
        priority: 2,
        holdTime: 10
      },
      {
        trigger: 'heavy_enemy_fire',
        action: 'spread_formation',
        priority: 1,
        spreadFactor: 1.5
      }
    ];
  }

  private findAlternativePositions(current: FlankingPosition): Vector2D[] {
    return [
      {
        x: current.center.x + (Math.random() - 0.5) * 100,
        y: current.center.y + (Math.random() - 0.5) * 100
      },
      {
        x: current.center.x + (Math.random() - 0.5) * 80,
        y: current.center.y - 50
      }
    ];
  }

  public executeManeuver(maneuverId: string): boolean {
    const maneuver = this.currentManeuvers.get(maneuverId);
    if (!maneuver) return false;

    maneuver.status = 'executing';
    maneuver.startTime = Date.now();
    return true;
  }

  public updateManeuver(
    maneuverId: string,
    units: Unit[],
    enemyPositions: Vector2D[]
  ): ManeuverUpdate {
    const maneuver = this.currentManeuvers.get(maneuverId);
    if (!maneuver) {
      return { status: 'unknown', progress: 0, recommendations: [] };
    }

    const elapsed = Date.now() - (maneuver.startTime || 0);
    const expectedDuration = maneuver.expectedEngagementTime * 1000;
    const progress = Math.min(1, elapsed / expectedDuration);

    const recommendations: string[] = [];

    if (progress > 0.5 && maneuver.status === 'executing') {
      maneuver.status = 'engaging';
    }

    if (progress >= 1 && maneuver.status !== 'completed') {
      maneuver.status = 'completed';
      this.recordManeuverResult(maneuver, true);
    }

    if (this.checkForRisks(maneuver, units, enemyPositions)) {
      recommendations.push('Consider aborting maneuver due to elevated risk');
    }

    return {
      status: maneuver.status,
      progress,
      recommendations,
      currentPositions: units.map(u => u.position)
    };
  }

  private checkForRisks(
    maneuver: FlankingManeuver,
    units: Unit[],
    enemyPositions: Vector2D[]
  ): boolean {
    const unitCenter = this.calculateCenter(units.map(u => u.position));
    
    for (const enemyPos of enemyPositions) {
      const dist = Math.sqrt(
        Math.pow(enemyPos.x - unitCenter.x, 2) +
        Math.pow(enemyPos.y - unitCenter.y, 2)
      );
      
      if (dist < 100) return true;
    }
    
    return false;
  }

  private calculateCenter(positions: Vector2D[]): Vector2D {
    if (positions.length === 0) return { x: 0, y: 0 };
    
    let x = 0, y = 0;
    for (const pos of positions) {
      x += pos.x;
      y += pos.y;
    }
    
    return { x: x / positions.length, y: y / positions.length };
  }

  public abortManeuver(maneuverId: string): void {
    const maneuver = this.currentManeuvers.get(maneuverId);
    if (maneuver) {
      maneuver.status = 'aborted';
      this.recordManeuverResult(maneuver, false);
    }
  }

  private recordManeuverResult(maneuver: FlankingManeuver, success: boolean): void {
    let history = this.flankingHistory.get(maneuver.id);
    if (!history) {
      history = [];
      this.flankingHistory.set(maneuver.id, history);
    }
    
    history.push({
      maneuverId: maneuver.id,
      success,
      timestamp: Date.now(),
      positions: maneuver.flankPositions,
      assignedUnits: maneuver.assignedUnits
    });
  }

  public getActiveManeuvers(): FlankingManeuver[] {
    return Array.from(this.currentManeuvers.values())
      .filter(m => m.status === 'planning' || m.status === 'executing');
  }

  public setPreferredAngle(style: 'aggressive' | 'balanced' | 'cautious'): void {
    for (const [key, angle] of this.preferredAngles) {
      if (key === style) {
        this.currentPreferredAngle = angle;
        break;
      }
    }
  }

  private currentPreferredAngle: number = 90;

  public getFlankingStatistics(): FlankingStatistics {
    const allAttempts = Array.from(this.flankingHistory.values()).flat();
    
    const successful = allAttempts.filter(a => a.success).length;
    const total = allAttempts.length;
    
    return {
      totalAttempts: total,
      successfulAttempts: successful,
      successRate: total > 0 ? successful / total : 0,
      averageEngagementTime: this.calculateAverageEngagementTime(),
      preferredAngles: new Map(this.preferredAngles)
    };
  }

  private calculateAverageEngagementTime(): number {
    const allAttempts = Array.from(this.flankingHistory.values()).flat();
    if (allAttempts.length === 0) return 0;
    
    const totalTime = allAttempts.reduce((sum, attempt) => {
      const maneuver = this.currentManeuvers.get(attempt.maneuverId);
      return sum + (maneuver?.expectedEngagementTime || 0);
    }, 0);
    
    return totalTime / allAttempts.length;
  }
}

interface FlankingPosition {
  center: Vector2D;
  approachVector: Vector2D;
  targetPoint: Vector2D;
  angleOffset: number;
  coverQuality: number;
  exposureRisk: number;
  positions: Vector2D[];
}

interface FlankingManeuver {
  id: string;
  flankPositions: Vector2D[];
  approachVector: Vector2D;
  targetPoint: Vector2D;
  assignedUnits: string[];
  status: 'planning' | 'executing' | 'engaging' | 'completed' | 'aborted';
  successProbability: number;
  expectedEngagementTime: number;
  risks: FlankingRisk[];
  contingencies: FlankingContingency[];
  startTime?: number;
}

interface FlankingRisk {
  type: string;
  severity: number;
  description: string;
  mitigation?: string;
}

interface FlankingContingency {
  trigger: string;
  action: string;
  priority: number;
  alternativePositions?: Vector2D[];
  holdTime?: number;
  spreadFactor?: number;
}

interface ManeuverUpdate {
  status: string;
  progress: number;
  recommendations: string[];
  currentPositions: Vector2D[];
}

interface FlankingAttempt {
  maneuverId: string;
  success: boolean;
  timestamp: number;
  positions: Vector2D[];
  assignedUnits: string[];
}

interface FlankingStatistics {
  totalAttempts: number;
  successfulAttempts: number;
  successRate: number;
  averageEngagementTime: number;
  preferredAngles: Map<string, number>;
}

export class RetreatAnalyzer {
  private retreatHistory: RetreatDecision[] = [];
  private safeZones: SafeZone[] = [];
  private healthThresholds: Map<string, number> = new Map();
  private threatMemory: Map<string, number> = new Map();

  constructor() {
    this.initializeDefaultThresholds();
  }

  private initializeDefaultThresholds(): void {
    this.healthThresholds.set('critical', 15);
    this.healthThresholds.set('low', 30);
    this.healthThresholds.set('moderate', 50);
    this.healthThresholds.set('safe', 75);
  }

  public shouldRetreat(
    unit: Unit,
    situation: TacticalSituation,
    nearbyEnemies: Unit[]
  ): RetreatDecision {
    const reasons: RetreatReason[] = [];
    let severity = 0;

    const healthCheck = this.checkHealth(unit);
    if (healthCheck.shouldRetreat) {
      reasons.push(healthCheck.reason!);
      severity = Math.max(severity, healthCheck.severity);
    }

    const overwhelmedCheck = this.checkIfOverwhelmed(unit, situation, nearbyEnemies);
    if (overwhelmedCheck.shouldRetreat) {
      reasons.push(overwhelmedCheck.reason!);
      severity = Math.max(severity, overwhelmedCheck.severity);
    }

    const surroundedCheck = this.checkIfSurrounded(unit, nearbyEnemies);
    if (surroundedCheck.shouldRetreat) {
      reasons.push(surroundedCheck.reason!);
      severity = Math.max(severity, surroundedCheck.severity);
    }

    const advantageCheck = this.checkLostAdvantage(unit, situation);
    if (advantageCheck.shouldRetreat) {
      reasons.push(advantageCheck.reason!);
      severity = Math.max(severity, advantageCheck.severity);
    }

    const survivalOdds = this.calculateSurvivalOdds(unit, situation, nearbyEnemies);
    const retreatPath = this.findBestRetreatPath(unit, situation);

    const decision: RetreatDecision = {
      shouldRetreat: severity >= 0.5 || survivalOdds < 0.4,
      reasons,
      severity,
      survivalOddsIfStay: survivalOdds,
      survivalOddsIfRetreat: this.calculateRetreatSurvivalOdds(unit, retreatPath),
      recommendedPath: retreatPath,
      urgency: this.calculateRetreatUrgency(unit, severity, reasons),
      alternativeActions: this.suggestAlternativeActions(unit, situation, nearbyEnemies)
    };

    this.recordRetreatDecision(decision, unit.id);
    return decision;
  }

  private checkHealth(unit: Unit): { shouldRetreat: boolean; reason?: RetreatReason; severity: number } {
    const healthPercent = (unit.health / unit.maxHealth) * 100;
    const criticalThreshold = this.healthThresholds.get('critical') || 15;
    const lowThreshold = this.healthThresholds.get('low') || 30;

    if (healthPercent <= criticalThreshold) {
      return { shouldRetreat: true, reason: RetreatReason.LOW_HEALTH, severity: 0.9 };
    }

    if (healthPercent <= lowThreshold) {
      return { shouldRetreat: true, reason: RetreatReason.LOW_HEALTH, severity: 0.6 };
    }

    return { shouldRetreat: false, severity: 0 };
  }

  private checkIfOverwhelmed(
    unit: Unit,
    situation: TacticalSituation,
    enemies: Unit[]
  ): { shouldRetreat: boolean; reason?: RetreatReason; severity: number } {
    const friendlyCount = situation.friendlyUnits.length;
    const enemyCount = enemies.length + situation.estimatedEnemyReinforcements;
    
    if (enemyCount > friendlyCount * 2) {
      return { shouldRetreat: true, reason: RetreatReason.OVERWHELMED, severity: 0.8 };
    }
    
    if (enemyCount > friendlyCount * 1.5) {
      return { shouldRetreat: true, reason: RetreatReason.OVERWHELMED, severity: 0.5 };
    }

    return { shouldRetreat: false, severity: 0 };
  }

  private checkIfSurrounded(unit: Unit, enemies: Unit[]): { shouldRetreat: boolean; reason?: RetreatReason; severity: number } {
    if (enemies.length < 3) {
      return { shouldRetreat: false, severity: 0 };
    }

    const angles = this.analyzeEnemyAngles(unit, enemies);
    const coverage = angles.reduce((sum, angle) => sum + angle, 0) / (Math.PI * 2);
    
    if (coverage > 0.8) {
      return { shouldRetreat: true, reason: RetreatReason.SURROUNDED, severity: 0.85 };
    }
    
    if (coverage > 0.6) {
      return { shouldRetreat: true, reason: RetreatReason.SURROUNDED, severity: 0.5 };
    }

    return { shouldRetreat: false, severity: 0 };
  }

  private analyzeEnemyAngles(unit: Unit, enemies: Unit[]): number[] {
    const angles: number[] = [];
    
    for (const enemy of enemies) {
      const dx = enemy.position.x - unit.position.x;
      const dy = enemy.position.y - unit.position.y;
      const angle = Math.atan2(dy, dx);
      angles.push(angle);
    }
    
    angles.sort((a, b) => a - b);
    
    const arcCoverages: number[] = [];
    for (let i = 0; i < angles.length; i++) {
      const nextIndex = (i + 1) % angles.length;
      let arc = angles[nextIndex] - angles[i];
      if (arc < 0) arc += Math.PI * 2;
      arcCoverages.push(arc);
    }
    
    return arcCoverages;
  }

  private checkLostAdvantage(
    unit: Unit,
    situation: TacticalSituation
  ): { shouldRetreat: boolean; reason?: RetreatReason; severity: number } {
    const terrainScore = Array.from(situation.terrainFeatures.values())
      .reduce((sum, v) => sum + v, 0) / situation.terrainFeatures.size;

    if (terrainScore < 0.3 && situation.advantageLevel < -20) {
      return { shouldRetreat: true, reason: RetreatReason.LOST_ADVANTAGE, severity: 0.6 };
    }

    return { shouldRetreat: false, severity: 0 };
  }

  private calculateSurvivalOdds(
    unit: Unit,
    situation: TacticalSituation,
    enemies: Unit[]
  ): number {
    let survivalOdds = 0.5;

    const healthFactor = unit.health / unit.maxHealth;
    survivalOdds += healthFactor * 0.2;

    const friendlySupport = situation.friendlyUnits.filter(
      u => u.id !== unit.id && unit.getDistanceTo(u) < 100
    ).length;
    survivalOdds += friendlySupport * 0.1;

    const enemyThreat = enemies.reduce((sum, e) => {
      return sum + (e.stats.attackDamage * (e.health / e.maxHealth));
    }, 0);
    survivalOdds -= enemyThreat / 500;

    const terrainScore = Array.from(situation.terrainFeatures.values())
      .reduce((sum, v) => sum + v, 0) / situation.terrainFeatures.size;
    survivalOdds += terrainScore * 0.1;

    return Math.max(0.05, Math.min(0.95, survivalOdds));
  }

  private calculateRetreatSurvivalOdds(unit: Unit, path: RetreatPath): number {
    if (path.waypoints.length === 0) return 0.1;

    let survivalOdds = 0.7;

    const coverQuality = this.assessRetreatCover(path);
    survivalOdds += coverQuality * 0.15;

    const pathLength = this.calculatePathLength(path.waypoints);
    const estimatedRetreatTime = pathLength / unit.stats.moveSpeed;
    const enemyInterceptionChance = this.estimateInterceptionChance(path, unit);
    
    survivalOdds -= enemyInterceptionChance * 0.2;

    const safeZone = this.findNearestSafeZone(path.waypoints[path.waypoints.length - 1]);
    if (safeZone) {
      survivalOdds += 0.1;
    }

    return Math.max(0.1, Math.min(0.99, survivalOdds));
  }

  private assessRetreatCover(path: RetreatPath): number {
    return 0.5 + Math.random() * 0.3;
  }

  private calculatePathLength(waypoints: Vector2D[]): number {
    let length = 0;
    for (let i = 1; i < waypoints.length; i++) {
      const dx = waypoints[i].x - waypoints[i - 1].x;
      const dy = waypoints[i].y - waypoints[i - 1].y;
      length += Math.sqrt(dx * dx + dy * dy);
    }
    return length;
  }

  private estimateInterceptionChance(path: RetreatPath, unit: Unit): number {
    return 0.2 + Math.random() * 0.3;
  }

  private findBestRetreatPath(unit: Unit, situation: TacticalSituation): RetreatPath {
    const path: RetreatPath = {
      waypoints: [],
      totalDistance: 0,
      estimatedTime: 0,
      coverPoints: [],
      dangerPoints: []
    };

    const baseDirection = this.getRetreatDirection(unit, situation);
    
    for (let i = 0; i < 5; i++) {
      const distance = 50 + i * 30;
      const waypoint: Vector2D = {
        x: unit.position.x + baseDirection.x * distance,
        y: unit.position.y + baseDirection.y * distance
      };
      path.waypoints.push(waypoint);
    }

    path.totalDistance = this.calculatePathLength(path.waypoints);
    path.estimatedTime = path.totalDistance / unit.stats.moveSpeed;

    return path;
  }

  private getRetreatDirection(unit: Unit, situation: TacticalSituation): Vector2D {
    const friendlyCenter = this.calculateCenter(
      situation.friendlyUnits.filter(u => u.id !== unit.id).map(u => u.position)
    );

    const direction = {
      x: friendlyCenter.x - unit.position.x,
      y: friendlyCenter.y - unit.position.y
    };

    const length = Math.sqrt(direction.x * direction.x + direction.y * direction.y);
    if (length === 0) {
      return { x: -1, y: 0 };
    }

    return {
      x: direction.x / length,
      y: direction.y / length
    };
  }

  private calculateCenter(positions: Vector2D[]): Vector2D {
    if (positions.length === 0) return { x: 0, y: 0 };
    
    let x = 0, y = 0;
    for (const pos of positions) {
      x += pos.x;
      y += pos.y;
    }
    
    return { x: x / positions.length, y: y / positions.length };
  }

  private findNearestSafeZone(position: Vector2D): SafeZone | null {
    let nearest: SafeZone | null = null;
    let minDist = Infinity;

    for (const zone of this.safeZones) {
      const dist = Math.sqrt(
        Math.pow(position.x - zone.position.x, 2) +
        Math.pow(position.y - zone.position.y, 2)
      );
      if (dist < zone.radius && dist < minDist) {
        minDist = dist;
        nearest = zone;
      }
    }

    return nearest;
  }

  private calculateRetreatUrgency(unit: Unit, severity: number, reasons: RetreatReason[]): 'critical' | 'high' | 'moderate' | 'low' {
    if (severity >= 0.8 || reasons.includes(RetreatReason.LOW_HEALTH)) {
      return 'critical';
    }
    if (severity >= 0.6 || reasons.includes(RetreatReason.SURROUNDED)) {
      return 'high';
    }
    if (severity >= 0.4) {
      return 'moderate';
    }
    return 'low';
  }

  private suggestAlternativeActions(
    unit: Unit,
    situation: TacticalSituation,
    enemies: Unit[]
  ): string[] {
    const alternatives: string[] = [];

    const nearbyFriendly = situation.friendlyUnits.find(
      u => u.id !== unit.id && unit.getDistanceTo(u) < 80
    );
    if (nearbyFriendly) {
      alternatives.push('Request support from nearby ally');
    }

    if (enemies.length < situation.enemyUnits.length) {
      alternatives.push('Wait for enemy forces to thin');
    }

    const hasAbility = unit.stats.attackDamage > 0;
    if (hasAbility) {
      alternatives.push('Consider making a final attack before retreating');
    }

    return alternatives;
  }

  private recordRetreatDecision(decision: RetreatDecision, unitId: string): void {
    this.retreatHistory.push({
      ...decision,
      unitId,
      timestamp: Date.now()
    });

    if (this.retreatHistory.length > 100) {
      this.retreatHistory.shift();
    }
  }

  public addSafeZone(zone: SafeZone): void {
    this.safeZones.push(zone);
  }

  public removeSafeZone(zoneId: string): void {
    this.safeZones = this.safeZones.filter(z => z.id !== zoneId);
  }

  public getRetreatHistory(): RetreatDecision[] {
    return [...this.retreatHistory];
  }

  public getRetreatStatistics(): RetreatStatistics {
    const total = this.retreatHistory.length;
    const successful = this.retreatHistory.filter(d => d.shouldRetreat).length;
    
    return {
      totalRetreatDecisions: total,
      retreatInitiated: successful,
      averageSeverity: total > 0 ? 
        this.retreatHistory.reduce((sum, d) => sum + d.severity, 0) / total : 0,
      mostCommonReasons: this.getMostCommonReasons()
    };
  }

  private getMostCommonReasons(): Map<RetreatReason, number> {
    const reasons = new Map<RetreatReason, number>();
    
    for (const decision of this.retreatHistory) {
      for (const reason of decision.reasons) {
        reasons.set(reason, (reasons.get(reason) || 0) + 1);
      }
    }
    
    return reasons;
  }
}

interface RetreatDecision {
  shouldRetreat: boolean;
  reasons: RetreatReason[];
  severity: number;
  survivalOddsIfStay: number;
  survivalOddsIfRetreat: number;
  recommendedPath: RetreatPath;
  urgency: 'critical' | 'high' | 'moderate' | 'low';
  alternativeActions: string[];
  unitId?: string;
  timestamp?: number;
}

interface RetreatPath {
  waypoints: Vector2D[];
  totalDistance: number;
  estimatedTime: number;
  coverPoints: Vector2D[];
  dangerPoints: Vector2D[];
}

interface SafeZone {
  id: string;
  position: Vector2D;
  radius: number;
  safety: number;
}

interface RetreatStatistics {
  totalRetreatDecisions: number;
  retreatInitiated: number;
  averageSeverity: number;
  mostCommonReasons: Map<RetreatReason, number>;
}

export class AbilityCoordinator {
  private abilityPlans: Map<string, AbilityUsePlan> = new Map();
  private cooldowns: Map<string, AbilityCooldown> = new Map();
  private abilityEffects: Map<string, AbilityEffectiveness> = new Map();
  private combatEnhancers: Map<string, CombatEnhancement> = new Map();
  private syncWindows: Map<string, SyncWindow> = new Map();

  constructor() {
    this.initializeDefaultAbilities();
  }

  private initializeDefaultAbilities(): void {
    this.combatEnhancers.set('damage_boost', {
      abilityId: 'damage_boost',
      effectType: 'damage',
      multiplier: 1.25,
      duration: 5,
      cooldown: 30,
      stackLimit: 1
    });

    this.combatEnhancers.set('speed_boost', {
      abilityId: 'speed_boost',
      effectType: 'speed',
      multiplier: 1.5,
      duration: 3,
      cooldown: 20,
      stackLimit: 1
    });

    this.syncWindows.set('focus_fire', {
      id: 'focus_fire',
      windowDuration: 2,
      requiredUnits: 3,
      optimalSpacing: 0.5,
      effectivenessBonus: 1.5
    });
  }

  public planAbilityUsage(
    abilityId: string,
    unit: Unit,
    situation: TacticalSituation,
    targetPosition?: Vector2D,
    targetEntity?: string
  ): AbilityUsePlan | null {
    const cooldown = this.cooldowns.get(abilityId);
    if (cooldown && !cooldown.isReady) {
      return null;
    }

    const optimalTiming = this.calculateOptimalTiming(abilityId, situation);
    const expectedEffect = this.predictAbilityEffect(abilityId, unit, situation, targetEntity);
    const priority = this.calculateAbilityPriority(abilityId, unit, situation, expectedEffect);

    const plan: AbilityUsePlan = {
      abilityId,
      optimalTiming,
      targetPosition,
      targetEntity,
      expectedEffect,
      priority,
      isReady: true,
      lastUsedEffectiveness: this.abilityEffects.get(abilityId)?.effectiveness || 0
    };

    this.abilityPlans.set(`${unit.id}_${abilityId}`, plan);
    return plan;
  }

  private calculateOptimalTiming(abilityId: string, situation: TacticalSituation): number {
    const now = Date.now();

    const enemyHealthAvg = situation.averageHealth.enemy;
    if (enemyHealthAvg < 50 && this.abilityEffects.get(abilityId)?.effectType === 'damage') {
      return now + 500;
    }

    const friendlyHealthAvg = situation.averageHealth.friendly;
    if (friendlyHealthAvg < 40 && this.abilityEffects.get(abilityId)?.effectType === 'heal') {
      return now + 1000;
    }

    if (situation.combatPhase === CombatPhase.ENGAGEMENT && this.isInitiationAbility(abilityId)) {
      return now + 200;
    }

    return now + 2000;
  }

  private isInitiationAbility(abilityId: string): boolean {
    const initiationAbilities = ['stun', 'slow', 'charge', 'teleport'];
    return initiationAbilities.some(id => abilityId.toLowerCase().includes(id));
  }

  private predictAbilityEffect(
    abilityId: string,
    unit: Unit,
    situation: TacticalSituation,
    targetEntityId?: string
  ): string {
    const enhancement = this.combatEnhancers.get(abilityId);
    if (enhancement) {
      return `${enhancement.effectType} ${enhancement.multiplier}x for ${enhancement.duration}s`;
    }

    const abilityScale = this.getAbilityScale(abilityId);
    const targetCount = targetEntityId ? 1 : situation.enemyUnits.length;
    
    return `Affects ${targetCount} targets at ${abilityScale} power`;
  }

  private getAbilityScale(abilityId: string): 'low' | 'medium' | 'high' | 'massive' {
    if (abilityId.includes('ultimate') || abilityId.includes('nuke')) return 'massive';
    if (abilityId.includes('heavy') || abilityId.includes('aoe')) return 'high';
    if (abilityId.includes('standard') || abilityId.includes('basic')) return 'medium';
    return 'low';
  }

  private calculateAbilityPriority(
    abilityId: string,
    unit: Unit,
    situation: TacticalSituation,
    expectedEffect: string
  ): number {
    let priority = 50;

    const enhancement = this.combatEnhancers.get(abilityId);
    if (enhancement) {
      switch (enhancement.effectType) {
        case 'damage':
          priority += situation.threatLevel * 0.3;
          break;
        case 'heal':
          priority += (100 - situation.averageHealth.friendly) * 0.4;
          break;
        case 'speed':
          priority += situation.advantageLevel < 0 ? 20 : 10;
          break;
      }
    }

    const recentEffectiveness = this.abilityEffects.get(abilityId)?.effectiveness || 0.5;
    priority *= (0.5 + recentEffectiveness);

    if (situation.combatPhase === CombatPhase.ROUTING) {
      priority *= 1.5;
    }

    return Math.min(100, Math.max(1, priority));
  }

  public executeAbility(
    unitId: string,
    abilityId: string,
    targetPosition?: Vector2D,
    targetEntityId?: string
  ): AbilityExecution {
    const plan = this.abilityPlans.get(`${unitId}_${abilityId}`);
    
    if (!plan) {
      return {
        success: false,
        abilityId,
        reason: 'No plan found for ability',
        actualEffect: '',
        timestamp: Date.now()
      };
    }

    if (!plan.isReady) {
      return {
        success: false,
        abilityId,
        reason: 'Ability not ready',
        actualEffect: '',
        timestamp: Date.now()
      };
    }

    const cooldownTime = this.getAbilityCooldown(abilityId);
    this.cooldowns.set(abilityId, {
      abilityId,
      remainingTime: cooldownTime,
      maxCooldown: cooldownTime,
      isReady: false,
      lastUsed: Date.now()
    });

    const actualEffect = this.applyAbilityEffect(abilityId, targetPosition, targetEntityId);
    
    this.recordAbilityUse(abilityId, actualEffect);

    return {
      success: true,
      abilityId,
      reason: 'Ability executed successfully',
      actualEffect,
      timestamp: Date.now()
    };
  }

  private getAbilityCooldown(abilityId: string): number {
    const enhancement = this.combatEnhancers.get(abilityId);
    if (enhancement) {
      return enhancement.cooldown * 1000;
    }
    return 10000;
  }

  private applyAbilityEffect(
    abilityId: string,
    position?: Vector2D,
    entityId?: string
  ): string {
    const enhancement = this.combatEnhancers.get(abilityId);
    if (enhancement) {
      return `Applied ${enhancement.effectType} effect at ${enhancement.multiplier}x power`;
    }
    return 'Applied standard ability effect';
  }

  private recordAbilityUse(abilityId: string, effect: string): void {
    const effectiveness = this.calculateEffectiveness(abilityId, effect);
    
    let record = this.abilityEffects.get(abilityId);
    if (!record) {
      record = {
        abilityId,
        useCount: 0,
        totalEffectiveness: 0,
        effectiveness: 0,
        lastUsed: 0,
        averageTiming: 0
      };
      this.abilityEffects.set(abilityId, record);
    }

    record.useCount++;
    record.totalEffectiveness += effectiveness;
    record.effectiveness = record.totalEffectiveness / record.useCount;
    record.lastUsed = Date.now();
  }

  private calculateEffectiveness(abilityId: string, effect: string): number {
    const enhancement = this.combatEnhancers.get(abilityId);
    if (enhancement) {
      return enhancement.multiplier;
    }
    return effect.includes('massive') ? 1.5 : effect.includes('high') ? 1.2 : 1.0;
  }

  public updateCooldowns(deltaTime: number): void {
    for (const [abilityId, cooldown] of this.cooldowns) {
      cooldown.remainingTime -= deltaTime * 1000;
      if (cooldown.remainingTime <= 0) {
        cooldown.remainingTime = 0;
        cooldown.isReady = true;
      }
    }
  }

  public isAbilityReady(abilityId: string): boolean {
    const cooldown = this.cooldowns.get(abilityId);
    return !cooldown || cooldown.isReady;
  }

  public getRemainingCooldown(abilityId: string): number {
    const cooldown = this.cooldowns.get(abilityId);
    return cooldown ? cooldown.remainingTime / 1000 : 0;
  }

  public findSyncOpportunities(
    units: Unit[],
    situation: TacticalSituation,
    syncType: string
  ): SyncOpportunity | null {
    const window = this.syncWindows.get(syncType);
    if (!window) return null;

    const eligibleUnits = units.filter(u => this.isAbilityReady(u.id));
    
    if (eligibleUnits.length < window.requiredUnits) {
      return null;
    }

    const positions = this.calculateSyncPositions(eligibleUnits, window);
    const timing = this.calculateSyncTiming(situation, window);

    return {
      windowId: syncType,
      participatingUnits: eligibleUnits.map(u => u.id),
      optimalPosition: positions.center,
      optimalTiming: timing,
      effectiveness: window.effectivenessBonus * (eligibleUnits.length / window.requiredUnits),
      requirements: window
    };
  }

  private calculateSyncPositions(
    units: Unit[],
    window: SyncWindow
  ): { center: Vector2D; radius: number } {
    let x = 0, y = 0;
    for (const unit of units) {
      x += unit.position.x;
      y += unit.position.y;
    }
    
    return {
      center: { x: x / units.length, y: y / units.length },
      radius: units.length * window.optimalSpacing * 20
    };
  }

  private calculateSyncTiming(
    situation: TacticalSituation,
    window: SyncWindow
  ): number {
    const now = Date.now();
    
    if (situation.combatPhase === CombatPhase.ENGAGEMENT) {
      return now + window.windowDuration * 1000 * 0.5;
    }
    
    if (situation.combatPhase === CombatPhase.COMBAT) {
      return now + window.windowDuration * 1000;
    }
    
    return now + window.windowDuration * 1000 * 2;
  }

  private isAbilityReady(unitId: string): boolean {
    return Math.random() > 0.3;
  }

  public getAbilityStatistics(): AbilityStatistics {
    const abilities = Array.from(this.abilityEffects.values());
    
    return {
      totalAbilitiesUsed: abilities.reduce((sum, a) => sum + a.useCount, 0),
      averageEffectiveness: abilities.length > 0 ?
        abilities.reduce((sum, a) => sum + a.effectiveness, 0) / abilities.length : 0,
      mostUsedAbility: this.findMostUsedAbility(),
      mostEffectiveAbility: this.findMostEffectiveAbility(),
      abilityCooldowns: new Map(this.cooldowns)
    };
  }

  private findMostUsedAbility(): string {
    let maxUse = 0;
    let mostUsed = '';
    
    for (const [id, effect] of this.abilityEffects) {
      if (effect.useCount > maxUse) {
        maxUse = effect.useCount;
        mostUsed = id;
      }
    }
    
    return mostUsed;
  }

  private findMostEffectiveAbility(): string {
    let maxEffectiveness = 0;
    let mostEffective = '';
    
    for (const [id, effect] of this.abilityEffects) {
      if (effect.effectiveness > maxEffectiveness) {
        maxEffectiveness = effect.effectiveness;
        mostEffective = id;
      }
    }
    
    return mostEffective;
  }

  public clearAbilityPlans(): void {
    this.abilityPlans.clear();
  }
}

interface AbilityEffectiveness {
  abilityId: string;
  useCount: number;
  totalEffectiveness: number;
  effectiveness: number;
  lastUsed: number;
  averageTiming: number;
}

interface CombatEnhancement {
  abilityId: string;
  effectType: string;
  multiplier: number;
  duration: number;
  cooldown: number;
  stackLimit: number;
}

interface SyncWindow {
  id: string;
  windowDuration: number;
  requiredUnits: number;
  optimalSpacing: number;
  effectivenessBonus: number;
}

interface AbilityExecution {
  success: boolean;
  abilityId: string;
  reason: string;
  actualEffect: string;
  timestamp: number;
}

interface SyncOpportunity {
  windowId: string;
  participatingUnits: string[];
  optimalPosition: Vector2D;
  optimalTiming: number;
  effectiveness: number;
  requirements: SyncWindow;
}

interface AbilityStatistics {
  totalAbilitiesUsed: number;
  averageEffectiveness: number;
  mostUsedAbility: string;
  mostEffectiveAbility: string;
  abilityCooldowns: Map<string, AbilityCooldown>;
}

export class GroupCoordinator {
  private groups: Map<string, GroupAssignment> = new Map();
  private groupHistory: GroupAssignment[] = [];
  private pendingAssignments: PendingAssignment[] = [];
  private coordinationProtocols: Map<string, CoordinationProtocol> = new Map();

  constructor() {
    this.initializeDefaultProtocols();
  }

  private initializeDefaultProtocols(): void {
    this.coordinationProtocols.set('focus_fire', {
      name: 'Focus Fire Protocol',
      description: 'All units target the same enemy',
      communicationInterval: 1000,
      targetSelection: 'priority',
      formationAdjustment: 'tight'
    });

    this.coordinationProtocols.set('flanking', {
      name: 'Flanking Protocol',
      description: 'Multiple groups attack from different angles',
      communicationInterval: 2000,
      targetSelection: 'distributed',
      formationAdjustment: 'spread'
    });

    this.coordinationProtocols.set('defensive', {
      name: 'Defensive Protocol',
      description: 'Units protect key positions',
      communicationInterval: 500,
      targetSelection: 'nearest',
      formationAdjustment: 'defensive'
    });
  }

  public createGroup(
    groupId: string,
    units: Unit[],
    leaderId: string,
    objective: TacticalObjective,
    tactics: GroupTactics
  ): GroupAssignment {
    const group: GroupAssignment = {
      groupId,
      units: units.map(u => u.id),
      leaderId,
      objective,
      tactics,
      isActive: true,
      morale: 100
    };

    this.groups.set(groupId, group);
    return group;
  }

  public assignUnitsToGroup(
    groupId: string,
    unitIds: string[]
  ): GroupAssignment | null {
    const group = this.groups.get(groupId);
    if (!group) return null;

    for (const unitId of unitIds) {
      if (!group.units.includes(unitId)) {
        group.units.push(unitId);
      }
    }

    return group;
  }

  public removeUnitsFromGroup(
    groupId: string,
    unitIds: string[]
  ): GroupAssignment | null {
    const group = this.groups.get(groupId);
    if (!group) return null;

    group.units = group.units.filter(id => !unitIds.includes(id));
    
    return group;
  }

  public coordinateGroup(
    groupId: string,
    situation: TacticalSituation
  ): GroupCoordinationResult {
    const group = this.groups.get(groupId);
    if (!group) {
      return { success: false, orders: [], reason: 'Group not found' };
    }

    const units = situation.friendlyUnits.filter(u => group.units.includes(u.id));
    if (units.length === 0) {
      return { success: false, orders: [], reason: 'No units in group' };
    }

    const orders: GroupOrder[] = [];

    switch (group.tactics) {
      case GroupTactics.FOCUS_FIRE:
        orders.push(...this.generateFocusFireOrders(group, units, situation));
        break;
      case GroupTactics.SPLIT_ATTACK:
        orders.push(...this.generateSplitAttackOrders(group, units, situation));
        break;
      case GroupTactics.SURROUND:
        orders.push(...this.generateSurroundOrders(group, units, situation));
        break;
      case GroupTactics.DEFENSIVE_HOLD:
        orders.push(...this.generateDefensiveOrders(group, units, situation));
        break;
      case GroupTactics.RETREAT_ORDERED:
        orders.push(...this.generateRetreatOrders(group, units, situation));
        break;
    }

    return {
      success: true,
      orders,
      groupStatus: this.getGroupStatus(group),
      coordinationQuality: this.evaluateCoordination(group, orders)
    };
  }

  private generateFocusFireOrders(
    group: GroupAssignment,
    units: Unit[],
    situation: TacticalSituation
  ): GroupOrder[] {
    const orders: GroupOrder[] = [];

    const priorityTarget = this.selectPriorityTarget(situation.enemyUnits);
    if (!priorityTarget) return orders;

    for (const unit of units) {
      orders.push({
        unitId: unit.id,
        type: 'attack',
        targetId: priorityTarget.id,
        priority: 1,
        formation: 'compact'
      });
    }

    return orders;
  }

  private selectPriorityTarget(enemies: Unit[]): Unit | null {
    if (enemies.length === 0) return null;

    let bestTarget: Unit | null = null;
    let bestScore = -Infinity;

    for (const enemy of enemies) {
      const healthScore = (1 - enemy.health / enemy.maxHealth) * 50;
      const damageScore = enemy.stats.attackDamage * 0.5;
      const score = healthScore + damageScore;

      if (score > bestScore) {
        bestScore = score;
        bestTarget = enemy;
      }
    }

    return bestTarget;
  }

  private generateSplitAttackOrders(
    group: GroupAssignment,
    units: Unit[],
    situation: TacticalSituation
  ): GroupOrder[] {
    const orders: GroupOrder[] = [];
    const splitSize = Math.floor(units.length / Math.min(2, units.length));
    
    let splitIndex = 0;
    for (let i = 0; i < units.length; i += splitSize) {
      const splitUnits = units.slice(i, i + splitSize);
      const targetIndex = splitIndex % situation.enemyUnits.length;
      const target = situation.enemyUnits[targetIndex];

      for (const unit of splitUnits) {
        orders.push({
          unitId: unit.id,
          type: 'attack',
          targetId: target?.id,
          priority: splitIndex === 0 ? 1 : 0.8,
          formation: 'spread'
        });
      }
      splitIndex++;
    }

    return orders;
  }

  private generateSurroundOrders(
    group: GroupAssignment,
    units: Unit[],
    situation: TacticalSituation
  ): GroupOrder[] {
    const orders: GroupOrder[] = [];
    
    if (situation.enemyUnits.length === 0) return orders;

    const target = situation.enemyUnits[0];
    const angles = this.distributeUnitsAroundTarget(units.length);

    for (let i = 0; i < units.length; i++) {
      const unit = units[i];
      const angle = angles[i];
      const distance = 80 + unit.stats.attackRange * 0.3;

      const targetPosition = {
        x: target.position.x + Math.cos(angle) * distance,
        y: target.position.y + Math.sin(angle) * distance
      };

      orders.push({
        unitId: unit.id,
        type: 'move',
        targetPosition,
        priority: 1,
        formation: 'surround'
      });
    }

    return orders;
  }

  private distributeUnitsAroundTarget(count: number): number[] {
    const angles: number[] = [];
    for (let i = 0; i < count; i++) {
      angles.push((i / count) * Math.PI * 2);
    }
    return angles;
  }

  private generateDefensiveOrders(
    group: GroupAssignment,
    units: Unit[],
    situation: TacticalSituation
  ): GroupOrder[] {
    const orders: GroupOrder[] = [];
    const centerPosition = this.calculateCenter(units.map(u => u.position));

    for (const unit of units) {
      orders.push({
        unitId: unit.id,
        type: 'defend',
        targetPosition: centerPosition,
        priority: 1,
        formation: 'defensive'
      });
    }

    return orders;
  }

  private generateRetreatOrders(
    group: GroupAssignment,
    units: Unit[],
    situation: TacticalSituation
  ): GroupOrder[] {
    const orders: GroupOrder[] = [];
    const friendlyCenter = this.calculateCenter(
      situation.friendlyUnits.filter(u => !group.units.includes(u.id)).map(u => u.position)
    );

    for (const unit of units) {
      orders.push({
        unitId: unit.id,
        type: 'move',
        targetPosition: {
          x: friendlyCenter.x + (Math.random() - 0.5) * 50,
          y: friendlyCenter.y + (Math.random() - 0.5) * 50
        },
        priority: 1,
        formation: 'compact'
      });
    }

    return orders;
  }

  private calculateCenter(positions: Vector2D[]): Vector2D {
    if (positions.length === 0) return { x: 0, y: 0 };
    
    let x = 0, y = 0;
    for (const pos of positions) {
      x += pos.x;
      y += pos.y;
    }
    
    return { x: x / positions.length, y: y / positions.length };
  }

  private getGroupStatus(group: GroupAssignment): GroupStatus {
    return {
      unitCount: group.units.length,
      morale: group.morale,
      isActive: group.isActive
    };
  }

  private evaluateCoordination(
    group: GroupAssignment,
    orders: GroupOrder[]
  ): number {
    if (orders.length === 0) return 0;

    let coordinationScore = 70;

    const targetDistribution = this.analyzeTargetDistribution(orders);
    coordinationScore += targetDistribution * 20;

    const timingConsistency = this.analyzeTimingConsistency(orders);
    coordinationScore += timingConsistency * 10;

    return Math.min(100, Math.max(0, coordinationScore));
  }

  private analyzeTargetDistribution(orders: GroupOrder[]): number {
    const targets = new Set(orders.map(o => o.targetId).filter(id => id));
    return Math.min(1, targets.size / orders.length);
  }

  private analyzeTimingConsistency(orders: GroupOrder[]): number {
    return 0.8;
  }

  public dissolveGroup(groupId: string): GroupAssignment | null {
    const group = this.groups.get(groupId);
    if (!group) return null;

    this.groups.delete(groupId);
    this.groupHistory.push(group);

    return group;
  }

  public getGroup(groupId: string): GroupAssignment | undefined {
    return this.groups.get(groupId);
  }

  public getAllGroups(): GroupAssignment[] {
    return Array.from(this.groups.values());
  }

  public getActiveGroups(): GroupAssignment[] {
    return Array.from(this.groups.values()).filter(g => g.isActive);
  }

  public getGroupStatistics(): GroupStatistics {
    const groups = Array.from(this.groups.values());
    
    return {
      totalGroups: groups.length,
      activeGroups: groups.filter(g => g.isActive).length,
      averageMorale: groups.length > 0 ?
        groups.reduce((sum, g) => sum + g.morale, 0) / groups.length : 0,
      totalUnitsUnderCommand: groups.reduce((sum, g) => sum + g.units.length, 0)
    };
  }
}

interface PendingAssignment {
  unitId: string;
  groupId: string;
  priority: number;
  requestedAt: number;
}

interface CoordinationProtocol {
  name: string;
  description: string;
  communicationInterval: number;
  targetSelection: string;
  formationAdjustment: string;
}

interface GroupOrder {
  unitId: string;
  type: string;
  targetId?: string;
  targetPosition?: Vector2D;
  priority: number;
  formation: string;
}

interface GroupStatus {
  unitCount: number;
  morale: number;
  isActive: boolean;
}

interface GroupCoordinationResult {
  success: boolean;
  orders: GroupOrder[];
  reason?: string;
  groupStatus?: GroupStatus;
  coordinationQuality?: number;
}

interface GroupStatistics {
  totalGroups: number;
  activeGroups: number;
  averageMorale: number;
  totalUnitsUnderCommand: number;
}

export class MeleeTactician {
  private chargePatterns: Map<string, ChargePattern> = new Map();
  private combatTechniques: CombatTechnique[] = [];

  constructor() {
    this.initializeDefaultTechniques();
  }

  private initializeDefaultTechniques(): void {
    this.combatTechniques.push({
      name: 'Aggressive Push',
      description: 'Constant forward pressure',
      damageBonus: 1.2,
      defensePenalty: 0.8,
      staminaCost: 1.5
    });

    this.combatTechniques.push({
      name: 'Defensive Stance',
      description: 'Parry and counter',
      damageBonus: 0.8,
      defenseBonus: 1.3,
      staminaCost: 1.0
    });

    this.combatTechniques.push({
      name: 'Flanking Strike',
      description: 'Attack from the side',
      damageBonus: 1.5,
      defensePenalty: 0.7,
      staminaCost: 1.2,
      requiresFlanking: true
    });
  }

  public developMeleeStrategy(
    meleeUnits: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): MeleeStrategy {
    const strategy: MeleeStrategy = {
      id: `melee_${Date.now()}`,
      recommendedTechnique: this.selectBestTechnique(meleeUnits, enemies, situation),
      targetAssignments: this.assignTargets(meleeUnits, enemies),
      positioning: this.calculateOptimalPositions(meleeUnits, enemies, situation),
      engagementTiming: this.determineEngagementTiming(situation),
      expectedDamage: this.estimateDamage(meleeUnits, enemies),
      risks: this.identifyMeleeRisks(meleeUnits, enemies)
    };

    return strategy;
  }

  private selectBestTechnique(
    units: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): CombatTechnique {
    let bestTechnique = this.combatTechniques[0];
    let bestScore = -Infinity;

    for (const technique of this.combatTechniques) {
      let score = technique.damageBonus * 50;

      if (technique.requiresFlanking && !this.canFlank(units, enemies)) {
        continue;
      }

      if (situation.advantageLevel > 20) {
        score += technique.damageBonus * 20;
      } else if (situation.advantageLevel < -20) {
        score += (technique.defenseBonus || 1) * 20;
      }

      if (technique.staminaCost > 1) {
        score -= technique.staminaCost * 10;
      }

      if (score > bestScore) {
        bestScore = score;
        bestTechnique = technique;
      }
    }

    return bestTechnique;
  }

  private canFlank(units: Unit[], enemies: Unit[]): boolean {
    if (units.length < 2 || enemies.length === 0) return false;
    
    const friendlyCenter = this.calculateCenter(units.map(u => u.position));
    const enemyCenter = this.calculateCenter(enemies.map(u => u.position));
    
    for (const unit of units) {
      const toUnit = {
        x: unit.position.x - friendlyCenter.x,
        y: unit.position.y - friendlyCenter.y
      };
      const toEnemy = {
        x: enemyCenter.x - unit.position.x,
        y: enemyCenter.y - unit.position.y
      };
      
      const dot = toUnit.x * toEnemy.x + toUnit.y * toEnemy.y;
      if (dot < 0) return true;
    }
    
    return false;
  }

  private calculateCenter(positions: Vector2D[]): Vector2D {
    if (positions.length === 0) return { x: 0, y: 0 };
    
    let x = 0, y = 0;
    for (const pos of positions) {
      x += pos.x;
      y += pos.y;
    }
    
    return { x: x / positions.length, y: y / positions.length };
  }

  private assignTargets(
    units: Unit[],
    enemies: Unit[]
  ): Map<string, string> {
    const assignments = new Map<string, string>();
    
    const sortedUnits = [...units].sort((a, b) => 
      b.stats.attackDamage - a.stats.attackDamage
    );
    
    const sortedEnemies = [...enemies].sort((a, b) => 
      a.health - b.health
    );

    for (let i = 0; i < sortedUnits.length; i++) {
      const targetIndex = i % sortedEnemies.length;
      assignments.set(sortedUnits[i].id, sortedEnemies[targetIndex].id);
    }

    return assignments;
  }

  private calculateOptimalPositions(
    units: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): Vector2D[] {
    const positions: Vector2D[] = [];
    const enemyCenter = this.calculateCenter(enemies.map(u => u.position));
    
    for (const unit of units) {
      const angle = Math.atan2(
        unit.position.y - enemyCenter.y,
        unit.position.x - enemyCenter.x
      );
      
      const offsetAngle = angle + (Math.random() - 0.5) * 0.5;
      const distance = 30 + unit.stats.attackRange * 0.3;
      
      positions.push({
        x: enemyCenter.x + Math.cos(offsetAngle) * distance,
        y: enemyCenter.y + Math.sin(offsetAngle) * distance
      });
    }

    return positions;
  }

  private determineEngagementTiming(situation: TacticalSituation): number {
    const now = Date.now();

    if (situation.combatPhase === CombatPhase.APPROACH) {
      return now + 2000;
    }

    if (situation.combatPhase === CombatPhase.ENGAGEMENT) {
      return now + 500;
    }

    return now + 100;
  }

  private estimateDamage(
    units: Unit[],
    enemies: Unit[]
  ): { friendly: number; enemy: number } {
    let friendlyDamage = 0;
    let enemyDamage = 0;

    for (const unit of units) {
      const nearbyEnemies = enemies.filter(e => unit.getDistanceTo(e) < unit.stats.attackRange);
      for (const enemy of nearbyEnemies) {
        friendlyDamage += unit.stats.attackDamage * unit.stats.attackSpeed;
      }
    }

    for (const enemy of enemies) {
      const nearbyUnits = units.filter(u => enemy.getDistanceTo(u) < enemy.stats.attackRange);
      for (const unit of nearbyUnits) {
        enemyDamage += enemy.stats.attackDamage * enemy.stats.attackSpeed;
      }
    }

    return { friendly: friendlyDamage, enemy: enemyDamage };
  }

  private identifyMeleeRisks(
    units: Unit[],
    enemies: Unit[]
  ): MeleeRisk[] {
    const risks: MeleeRisk[] = [];

    const avgUnitHealth = units.reduce((sum, u) => sum + u.health / u.maxHealth, 0) / units.length;
    if (avgUnitHealth < 0.5) {
      risks.push({
        type: 'low_health',
        severity: 0.7,
        description: 'Units have low health'
      });
    }

    if (enemies.length > units.length * 1.5) {
      risks.push({
        type: 'outnumbered',
        severity: 0.8,
        description: 'Facing superior numbers'
      });
    }

    return risks;
  }
}

interface CombatTechnique {
  name: string;
  description: string;
  damageBonus: number;
  defenseBonus?: number;
  defensePenalty?: number;
  staminaCost: number;
  requiresFlanking?: boolean;
}

interface MeleeStrategy {
  id: string;
  recommendedTechnique: CombatTechnique;
  targetAssignments: Map<string, string>;
  positioning: Vector2D[];
  engagementTiming: number;
  expectedDamage: { friendly: number; enemy: number };
  risks: MeleeRisk[];
}

interface MeleeRisk {
  type: string;
  severity: number;
  description: string;
}

interface ChargePattern {
  id: string;
  unitCount: number;
  targetType: string;
  successRate: number;
  averageDamage: number;
  timestamp: number;
}

export class RangedTactician {
  private rangeBands: Map<string, RangeBand> = new Map();
  private firingPatterns: Map<string, FiringPattern> = new Map();

  constructor() {
    this.initializeDefaultRanges();
    this.initializeDefaultPatterns();
  }

  private initializeDefaultRanges(): void {
    this.rangeBands.set('optimal', { min: 0.7, max: 1.0, damageMultiplier: 1.0 });
    this.rangeBands.set('medium', { min: 0.4, max: 0.7, damageMultiplier: 0.8 });
    this.rangeBands.set('long', { min: 0.0, max: 0.4, damageMultiplier: 0.6 });
    this.rangeBands.set('danger', { min: -0.2, max: 0.0, damageMultiplier: 0.4 });
  }

  private initializeDefaultPatterns(): void {
    this.firingPatterns.set('volley', {
      name: 'Volley Fire',
      description: 'All units fire simultaneously',
      coordinationRequired: true,
      damageBurst: true
    });

    this.firingPatterns.set('rotation', {
      name: 'Rotating Fire',
      description: 'Units fire in sequence for continuous damage',
      coordinationRequired: true,
      damageBurst: false
    });

    this.firingPatterns.set('focused', {
      name: 'Focused Fire',
      description: 'Concentrated fire on single target',
      coordinationRequired: true,
      damageBurst: true
    });
  }

  public developRangedStrategy(
    rangedUnits: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): RangedStrategy {
    const firingPattern = this.selectBestFiringPattern(rangedUnits, enemies, situation);
    const positioning = this.calculateRangedPositions(rangedUnits, enemies, situation);
    const kitingPlan = this.evaluateKitingOpportunity(rangedUnits, enemies, situation);

    return {
      id: `ranged_${Date.now()}`,
      firingPattern,
      optimalPositions: positioning.positions,
      engagementRange: positioning.optimalRange,
      kitingRequired: kitingPlan.shouldKite,
      expectedDamage: this.estimateRangedDamage(rangedUnits, enemies, firingPattern),
      risks: this.identifyRangedRisks(rangedUnits, enemies)
    };
  }

  private selectBestFiringPattern(
    units: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): FiringPattern {
    if (situation.combatPhase === CombatPhase.ENGAGEMENT) {
      return this.firingPatterns.get('volley')!;
    }

    if (situation.advantageLevel > 20) {
      return this.firingPatterns.get('focused')!;
    }

    if (enemies.length > units.length) {
      return this.firingPatterns.get('rotation')!;
    }

    return this.firingPatterns.get('volley')!;
  }

  private calculateRangedPositions(
    units: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): { positions: Vector2D[]; optimalRange: number } {
    const positions: Vector2D[] = [];
    const enemyCenter = this.calculateCenter(enemies.map(u => u.position));

    const avgRange = units.reduce((sum, u) => sum + u.stats.attackRange, 0) / units.length;
    const optimalRange = avgRange * 0.8;

    for (let i = 0; i < units.length; i++) {
      const angle = (i / units.length) * Math.PI * 2;
      const distance = optimalRange;

      positions.push({
        x: enemyCenter.x + Math.cos(angle) * distance,
        y: enemyCenter.y + Math.sin(angle) * distance
      });
    }

    return { positions, optimalRange };
  }

  private calculateCenter(positions: Vector2D[]): Vector2D {
    if (positions.length === 0) return { x: 0, y: 0 };
    
    let x = 0, y = 0;
    for (const pos of positions) {
      x += pos.x;
      y += pos.y;
    }
    
    return { x: x / positions.length, y: y / positions.length };
  }

  private evaluateKitingOpportunity(
    units: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): { shouldKite: boolean } {
    if (enemies.length === 0) {
      return { shouldKite: false };
    }

    const meleeEnemies = enemies.filter(e => e.stats.attackRange < 50);
    if (meleeEnemies.length === 0) {
      return { shouldKite: false };
    }

    const avgUnitSpeed = units.reduce((sum, u) => sum + u.stats.moveSpeed, 0) / units.length;
    const avgEnemySpeed = enemies.reduce((sum, e) => sum + e.stats.moveSpeed, 0) / enemies.length;

    if (avgUnitSpeed > avgEnemySpeed && units[0].stats.attackRange > 100) {
      return { shouldKite: true };
    }

    return { shouldKite: false };
  }

  private estimateRangedDamage(
    units: Unit[],
    enemies: Unit[],
    pattern: FiringPattern
  ): number {
    let totalDamage = 0;

    for (const unit of units) {
      const damagePerAttack = unit.stats.attackDamage * unit.stats.attackSpeed;
      totalDamage += damagePerAttack;
    }

    if (pattern.damageBurst) {
      totalDamage *= 1.3;
    }

    return totalDamage;
  }

  private identifyRangedRisks(units: Unit[], enemies: Unit[]): RangedRisk[] {
    const risks: RangedRisk[] = [];

    const meleeEnemies = enemies.filter(e => e.stats.attackRange < 50);
    if (meleeEnemies.length > units.length) {
      risks.push({
        type: 'melee_pressure',
        severity: 0.7,
        description: 'Too many melee enemies approaching'
      });
    }

    return risks;
  }
}

interface RangeBand {
  min: number;
  max: number;
  damageMultiplier: number;
}

interface FiringPattern {
  name: string;
  description: string;
  coordinationRequired: boolean;
  damageBurst: boolean;
}

interface RangedStrategy {
  id: string;
  firingPattern: FiringPattern;
  optimalPositions: Vector2D[];
  engagementRange: number;
  kitingRequired: boolean;
  expectedDamage: number;
  risks: RangedRisk[];
}

interface RangedRisk {
  type: string;
  severity: number;
  description: string;
}

export class AirTactician {
  private altitudeBands: Map<string, AltitudeBand> = new Map();
  private flightPatterns: Map<string, FlightPattern> = new Map();

  constructor() {
    this.initializeDefaultAltitude();
    this.initializeDefaultPatterns();
  }

  private initializeDefaultAltitude(): void {
    this.altitudeBands.set('low', { minHeight: 0, maxHeight: 100, visibility: 1.0, threat: 0.3 });
    this.altitudeBands.set('medium', { minHeight: 100, maxHeight: 300, visibility: 0.8, threat: 0.5 });
    this.altitudeBands.set('high', { minHeight: 300, maxHeight: 500, visibility: 0.5, threat: 0.8 });
  }

  private initializeDefaultPatterns(): void {
    this.flightPatterns.set('hover', {
      name: 'Hover',
      description: 'Maintain position in air',
      speedMultiplier: 0,
      maneuverability: 1.0
    });

    this.flightPatterns.set('patrol', {
      name: 'Patrol',
      description: 'Systematic area coverage',
      speedMultiplier: 0.5,
      maneuverability: 0.7
    });

    this.flightPatterns.set('dive', {
      name: 'Dive Attack',
      description: 'High speed attack run',
      speedMultiplier: 1.5,
      maneuverability: 0.5
    });

    this.flightPatterns.set('evasion', {
      name: 'Evasion',
      description: 'Dodge enemy fire',
      speedMultiplier: 0.8,
      maneuverability: 1.2
    });
  }

  public developAirStrategy(
    airUnits: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): AirStrategy {
    const altitudePlan = this.determineOptimalAltitude(airUnits, enemies, situation);
    const flightPattern = this.selectFlightPattern(airUnits, enemies, situation);
    const targetAssignments = this.assignAirTargets(airUnits, enemies);

    return {
      id: `air_${Date.now()}`,
      altitudePlan,
      flightPattern,
      targetAssignments,
      formation: this.calculateAirFormation(airUnits),
      expectedDamage: this.estimateAirDamage(airUnits, enemies),
      risks: this.identifyAirRisks(airUnits, enemies)
    };
  }

  private determineOptimalAltitude(
    units: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): AltitudePlan {
    const antiAirThreats = enemies.filter(e => e.stats.attackRange > 150);
    
    let recommendedAltitude: 'low' | 'medium' | 'high' = 'medium';

    if (antiAirThreats.length > units.length) {
      recommendedAltitude = 'low';
    } else if (antiAirThreats.length === 0) {
      recommendedAltitude = 'high';
    }

    const altitudeBand = this.altitudeBands.get(recommendedAltitude)!;

    return {
      recommendedAltitude,
      minHeight: altitudeBand.minHeight,
      maxHeight: altitudeBand.maxHeight,
      visibility: altitudeBand.visibility,
      threat: altitudeBand.threat
    };
  }

  private selectFlightPattern(
    units: Unit[],
    enemies: Unit[],
    situation: TacticalSituation
  ): FlightPattern {
    const antiAir = enemies.filter(e => e.stats.attackRange > 150);
    
    if (antiAir.length > 0) {
      return this.flightPatterns.get('evasion')!;
    }

    if (situation.combatPhase === CombatPhase.ENGAGEMENT) {
      return this.flightPatterns.get('dive')!;
    }

    if (situation.combatPhase === CombatPhase.APPROACH) {
      return this.flightPatterns.get('patrol')!;
    }

    return this.flightPatterns.get('hover')!;
  }

  private assignAirTargets(
    airUnits: Unit[],
    enemies: Unit[]
  ): Map<string, string> {
    const assignments = new Map<string, string>();
    
    const groundTargets = enemies.filter(e => !e.stats.isAir);

    for (const unit of airUnits) {
      let bestTarget: Unit | null = null;
      let bestScore = -Infinity;

      for (const target of groundTargets) {
        const distance = unit.getDistanceTo(target);
        if (distance > unit.stats.attackRange) continue;

        const healthScore = (1 - target.health / target.maxHealth) * 30;
        const damageScore = target.stats.attackDamage * 0.5;
        const distanceScore = (1 - distance / unit.stats.attackRange) * 20;
        
        const score = healthScore + damageScore + distanceScore;

        if (score > bestScore) {
          bestScore = score;
          bestTarget = target;
        }
      }

      if (bestTarget) {
        assignments.set(unit.id, bestTarget.id);
      }
    }

    return assignments;
  }

  private calculateAirFormation(units: Unit[]): AirFormation {
    const spacing = 40;
    
    return {
      type: 'loose',
      spacing,
      positions: units.map((unit, i) => ({
        id: unit.id,
        offset: {
          x: Math.cos((i / units.length) * Math.PI * 2) * spacing,
          y: Math.sin((i / units.length) * Math.PI * 2) * spacing
        }
      }))
    };
  }

  private estimateAirDamage(airUnits: Unit[], enemies: Unit[]): number {
    let totalDamage = 0;

    for (const unit of airUnits) {
      const nearbyEnemies = enemies.filter(e => unit.getDistanceTo(e) < unit.stats.attackRange);
      for (const enemy of nearbyEnemies) {
        totalDamage += unit.stats.attackDamage;
      }
    }

    return totalDamage;
  }

  private identifyAirRisks(airUnits: Unit[], enemies: Unit[]): AirRisk[] {
    const risks: AirRisk[] = [];

    const antiAir = enemies.filter(e => e.stats.attackRange > 150);
    if (antiAir.length > airUnits.length) {
      risks.push({
        type: 'anti_air_threat',
        severity: 0.8,
        description: 'Heavy anti-air fire present'
      });
    }

    return risks;
  }
}

interface AltitudeBand {
  minHeight: number;
  maxHeight: number;
  visibility: number;
  threat: number;
}

interface FlightPattern {
  name: string;
  description: string;
  speedMultiplier: number;
  maneuverability: number;
}

interface AltitudePlan {
  recommendedAltitude: 'low' | 'medium' | 'high';
  minHeight: number;
  maxHeight: number;
  visibility: number;
  threat: number;
}

interface AirFormation {
  type: string;
  spacing: number;
  positions: { id: string; offset: Vector2D }[];
}

interface AirStrategy {
  id: string;
  altitudePlan: AltitudePlan;
  flightPattern: FlightPattern;
  targetAssignments: Map<string, string>;
  formation: AirFormation;
  expectedDamage: number;
  risks: AirRisk[];
}

interface AirRisk {
  type: string;
  severity: number;
  description: string;
}

export class SiegeTactician {
  private siegeRanges: Map<string, SiegeRange> = new Map();
  private targetPriorities: Map<string, number> = new Map();

  constructor() {
    this.initializeDefaultRanges();
    this.initializeTargetPriorities();
  }

  private initializeDefaultRanges(): void {
    this.siegeRanges.set('optimal', { min: 0.8, max: 1.0, damageMultiplier: 1.0 });
    this.siegeRanges.set('medium', { min: 0.5, max: 0.8, damageMultiplier: 0.7 });
    this.siegeRanges.set('long', { min: 0.3, max: 0.5, damageMultiplier: 0.4 });
  }

  private initializeTargetPriorities(): void {
    this.targetPriorities.set('core', 100);
    this.targetPriorities.set('production', 80);
    this.targetPriorities.set('defense', 70);
    this.targetPriorities.set('resource', 50);
    this.targetPriorities.set('wall', 60);
  }

  public developSiegeStrategy(
    siegeUnits: Unit[],
    targetStructures: Building[],
    situation: TacticalSituation
  ): SiegeStrategy {
    const optimalPositions = this.calculateOptimalPositions(siegeUnits, targetStructures);
    const targetAssignments = this.assignTargets(siegeUnits, targetStructures);
    const approachPlan = this.planApproach(siegeUnits, targetStructures);

    return {
      id: `siege_${Date.now()}`,
      targetAssignments,
      optimalPositions,
      approachPlan,
      expectedDamage: this.estimateSiegeDamage(siegeUnits, targetStructures),
      estimatedTime: this.estimateSiegeTime(siegeUnits, targetStructures),
      risks: this.identifySiegeRisks(siegeUnits, targetStructures)
    };
  }

  private calculateOptimalPositions(
    siegeUnits: Unit[],
    structures: Building[]
  ): Vector2D[] {
    const positions: Vector2D[] = [];

    for (const structure of structures) {
      const range = siegeUnits[0]?.stats.attackRange || 200;
      const optimalDistance = range * 0.9;

      for (const unit of siegeUnits) {
        const angle = Math.random() * Math.PI * 2;
        positions.push({
          x: structure.position.x + Math.cos(angle) * optimalDistance,
          y: structure.position.y + Math.sin(angle) * optimalDistance
        });
      }
    }

    return positions;
  }

  private assignTargets(
    siegeUnits: Unit[],
    structures: Building[]
  ): Map<string, string> {
    const assignments = new Map<string, string>();
    
    const sortedStructures = [...structures].sort((a, b) => {
      const priorityA = this.targetPriorities.get(a.structureType) || 50;
      const priorityB = this.targetPriorities.get(b.structureType) || 50;
      return priorityB - priorityA;
    });

    for (let i = 0; i < siegeUnits.length; i++) {
      const targetIndex = i % sortedStructures.length;
      assignments.set(siegeUnits[i].id, sortedStructures[targetIndex].id);
    }

    return assignments;
  }

  private planApproach(
    siegeUnits: Unit[],
    structures: Building[]
  ): ApproachPlan {
    const target = structures[0];
    if (!target) {
      return { waypoints: [], formation: 'compact', estimatedTime: 0 };
    }

    const waypoints: Vector2D[] = [];
    const unitCenter = this.calculateCenter(siegeUnits.map(u => u.position));
    
    const midPoint = {
      x: (unitCenter.x + target.position.x) / 2,
      y: (unitCenter.y + target.position.y) / 2 - 50
    };
    waypoints.push(midPoint);

    const finalPosition = {
      x: target.position.x + (Math.random() - 0.5) * 100,
      y: target.position.y + (Math.random() - 0.5) * 100
    };
    waypoints.push(finalPosition);

    const distance = this.calculateDistance(unitCenter, target.position);
    const avgSpeed = siegeUnits.reduce((sum, u) => sum + u.stats.moveSpeed, 0) / siegeUnits.length;

    return {
      waypoints,
      formation: 'spread',
      estimatedTime: distance / avgSpeed
    };
  }

  private calculateCenter(positions: Vector2D[]): Vector2D {
    if (positions.length === 0) return { x: 0, y: 0 };
    
    let x = 0, y = 0;
    for (const pos of positions) {
      x += pos.x;
      y += pos.y;
    }
    
    return { x: x / positions.length, y: y / positions.length };
  }

  private calculateDistance(a: Vector2D, b: Vector2D): number {
    return Math.sqrt(Math.pow(b.x - a.x, 2) + Math.pow(b.y - a.y, 2));
  }

  private estimateSiegeDamage(
    siegeUnits: Unit[],
    structures: Building[]
  ): number {
    let totalDamage = 0;

    for (const unit of siegeUnits) {
      const damagePerSecond = unit.stats.attackDamage * unit.stats.attackSpeed;
      totalDamage += damagePerSecond;
    }

    return totalDamage;
  }

  private estimateSiegeTime(
    siegeUnits: Unit[],
    structures: Building[]
  ): number {
    if (structures.length === 0) return 0;

    const totalHealth = structures.reduce((sum, s) => sum + s.health, 0);
    const damagePerSecond = this.estimateSiegeDamage(siegeUnits, structures);

    return damagePerSecond > 0 ? totalHealth / damagePerSecond : Infinity;
  }

  private identifySiegeRisks(
    siegeUnits: Unit[],
    structures: Building[]
  ): SiegeRisk[] {
    const risks: SiegeRisk[] = [];

    const hasEscort = siegeUnits.some(u => u.stats.attackRange < 100);
    if (!hasEscort && siegeUnits.length > 0) {
      risks.push({
        type: 'no_escort',
        severity: 0.6,
        description: 'Siege units lack melee escort'
      });
    }

    if (structures.some(s => s.structureType.includes('turret'))) {
      risks.push({
        type: 'defensive_turrets',
        severity: 0.7,
        description: 'Target has defensive turrets'
      });
    }

    return risks;
  }
}

interface SiegeRange {
  min: number;
  max: number;
  damageMultiplier: number;
}

interface SiegeStrategy {
  id: string;
  targetAssignments: Map<string, string>;
  optimalPositions: Vector2D[];
  approachPlan: ApproachPlan;
  expectedDamage: number;
  estimatedTime: number;
  risks: SiegeRisk[];
}

interface ApproachPlan {
  waypoints: Vector2D[];
  formation: string;
  estimatedTime: number;
}

interface SiegeRisk {
  type: string;
  severity: number;
  description: string;
}

export class CombatSimulator {
  private simulationCache: Map<string, CombatSimulation> = new Map();

  constructor() {}

  public simulateCombat(
    friendlyUnits: Unit[],
    enemyUnits: Unit[],
    iterations: number = 100
  ): CombatSimulation {
    const cacheKey = this.generateCacheKey(friendlyUnits, enemyUnits);
    const cached = this.simulationCache.get(cacheKey);
    
    if (cached) {
      return cached;
    }

    let totalFriendlyLosses = 0;
    let totalEnemyLosses = 0;
    let totalDamageDealt = 0;
    let totalDamageTaken = 0;
    let victories = 0;

    for (let i = 0; i < iterations; i++) {
      const result = this.runSingleSimulation(friendlyUnits, enemyUnits);
      totalFriendlyLosses += result.friendlyLosses;
      totalEnemyLosses += result.enemyLosses;
      totalDamageDealt += result.damageDealt;
      totalDamageTaken += result.damageTaken;
      if (result.victory) victories++;
    }

    const simulation: CombatSimulation = {
      duration: totalFriendlyLosses + totalEnemyLosses,
      friendlyLosses: totalFriendlyLosses / iterations,
      enemyLosses: totalEnemyLosses / iterations,
      damageDealt: totalDamageDealt / iterations,
      damageTaken: totalDamageTaken / iterations,
      positionChanges: [],
      abilitiesUsed: 0,
      victoryProbability: victories / iterations,
      recommendedActions: []
    };

    this.simulationCache.set(cacheKey, simulation);
    return simulation;
  }

  private runSingleSimulation(
    friendlyUnits: Unit[],
    enemyUnits: Unit[]
  ): {
    friendlyLosses: number;
    enemyLosses: number;
    damageDealt: number;
    damageTaken: number;
    victory: boolean;
  } {
    const friendlyCopy = friendlyUnits.map(u => ({ ...u }));
    const enemyCopy = enemyUnits.map(u => ({ ...u }));

    let damageDealt = 0;
    let damageTaken = 0;
    let rounds = 0;
    const maxRounds = 50;

    while (friendlyCopy.length > 0 && enemyCopy.length > 0 && rounds < maxRounds) {
      for (const friendly of friendlyCopy) {
        if (enemyCopy.length === 0) break;
        const target = enemyCopy[Math.floor(Math.random() * enemyCopy.length)];
        const damage = friendly.stats.attackDamage * (0.8 + Math.random() * 0.4);
        target.health -= damage;
        damageDealt += damage;

        if (target.health <= 0) {
          const index = enemyCopy.indexOf(target);
          if (index > -1) enemyCopy.splice(index, 1);
        }
      }

      for (const enemy of enemyCopy) {
        if (friendlyCopy.length === 0) break;
        const target = friendlyCopy[Math.floor(Math.random() * friendlyCopy.length)];
        const damage = enemy.stats.attackDamage * (0.8 + Math.random() * 0.4);
        target.health -= damage;
        damageTaken += damage;

        if (target.health <= 0) {
          const index = friendlyCopy.indexOf(target);
          if (index > -1) friendlyCopy.splice(index, 1);
        }
      }

      rounds++;
    }

    return {
      friendlyLosses: friendlyUnits.length - friendlyCopy.length,
      enemyLosses: enemyUnits.length - enemyCopy.length,
      damageDealt,
      damageTaken,
      victory: enemyCopy.length === 0
    };
  }

  private generateCacheKey(friendly: Unit[], enemy: Unit[]): string {
    return `${friendly.length}_${enemy.length}_${Date.now()}`;
  }

  public clearCache(): void {
    this.simulationCache.clear();
  }
}

export class AITacticalSystem {
  private combatAnalyzer: CombatAnalyzer;
  private targetSelector: TargetSelector;
  private formationManager: FormationManager;
  private flankingManager: FlankingManager;
  private retreatAnalyzer: RetreatAnalyzer;
  private abilityCoordinator: AbilityCoordinator;
  private groupCoordinator: GroupCoordinator;
  private meleeTactician: MeleeTactician;
  private rangedTactician: RangedTactician;
  private airTactician: AirTactician;
  private siegeTactician: SiegeTactician;
  private combatSimulator: CombatSimulator;
  
  private currentSituation: TacticalSituation | null = null;
  private lastUpdate: number = 0;
  private updateInterval: number = 500;
  private tacticalMemory: Map<string, unknown> = new Map();

  constructor() {
    this.combatAnalyzer = new CombatAnalyzer();
    this.targetSelector = new TargetSelector();
    this.formationManager = new FormationManager();
    this.flankingManager = new FlankingManager();
    this.retreatAnalyzer = new RetreatAnalyzer();
    this.abilityCoordinator = new AbilityCoordinator();
    this.groupCoordinator = new GroupCoordinator();
    this.meleeTactician = new MeleeTactician();
    this.rangedTactician = new RangedTactician();
    this.airTactician = new AirTactician();
    this.siegeTactician = new SiegeTactician();
    this.combatSimulator = new CombatSimulator();
  }

  public update(
    position: Vector2D,
    friendlyUnits: Unit[],
    enemyUnits: Unit[],
    structures: Building[],
    deltaTime: number
  ): TacticalDecision {
    this.abilityCoordinator.updateCooldowns(deltaTime);

    const now = Date.now();
    if (now - this.lastUpdate >= this.updateInterval) {
      this.currentSituation = this.combatAnalyzer.analyzeSituation(
        position,
        friendlyUnits,
        enemyUnits,
        structures
      );
      this.lastUpdate = now;
    }

    if (!this.currentSituation) {
      return { action: 'idle', priority: 0, reason: 'No situation data' };
    }

    const tacticalScore = this.combatAnalyzer.calculateTacticalScore(this.currentSituation);
    
    const retreatDecision = this.evaluateRetreat(friendlyUnits);
    if (retreatDecision.shouldRetreat) {
      return {
        action: 'retreat',
        priority: retreatDecision.urgency === 'critical' ? 100 : 80,
        reason: `Retreat: ${retreatDecision.reasons.join(', ')}`,
        details: retreatDecision
      };
    }

    const attackDecision = this.evaluateAttack(friendlyUnits, enemyUnits);
    if (attackDecision.shouldAttack) {
      return {
        action: 'attack',
        priority: 70,
        reason: attackDecision.reason,
        details: attackDecision
      };
    }

    const defendDecision = this.evaluateDefense(friendlyUnits, structures);
    if (defendDecision.shouldDefend) {
      return {
        action: 'defend',
        priority: 60,
        reason: defendDecision.reason,
        details: defendDecision
      };
    }

    return {
      action: 'hold',
      priority: 30,
      reason: 'Maintain current position',
      details: { tacticalScore }
    };
  }

  private evaluateRetreat(units: Unit[]): RetreatDecision {
    if (!this.currentSituation || units.length === 0) {
      return {
        shouldRetreat: false,
        reasons: [],
        severity: 0,
        survivalOddsIfStay: 1,
        survivalOddsIfRetreat: 1,
        recommendedPath: { waypoints: [], totalDistance: 0, estimatedTime: 0, coverPoints: [], dangerPoints: [] },
        urgency: 'low',
        alternativeActions: []
      };
    }

    const criticalUnit = units.find(u => u.health / u.maxHealth < 0.2);

    if (criticalUnit) {
      return this.retreatAnalyzer.shouldRetreat(
        criticalUnit,
        this.currentSituation,
        this.currentSituation.enemyUnits
      );
    }

    return {
      shouldRetreat: false,
      reasons: [],
      severity: 0,
      survivalOddsIfStay: 1,
      survivalOddsIfRetreat: 1,
      recommendedPath: { waypoints: [], totalDistance: 0, estimatedTime: 0, coverPoints: [], dangerPoints: [] },
      urgency: 'low',
      alternativeActions: []
    };
  }

  private evaluateAttack(
    friendlyUnits: Unit[],
    enemyUnits: Unit[]
  ): { shouldAttack: boolean; reason: string; details?: unknown } {
    if (!this.currentSituation) {
      return { shouldAttack: false, reason: 'No situation data' };
    }

    const advantage = this.currentSituation.advantageLevel;
    const firepowerRatio = this.currentSituation.overallFirepower.friendly / 
      Math.max(1, this.currentSituation.overallFirepower.enemy);

    if (advantage > 30 && firepowerRatio > 1.2) {
      const meleeUnits = friendlyUnits.filter(u => u.stats.attackRange < 80);
      const rangedUnits = friendlyUnits.filter(u => u.stats.attackRange >= 80);
      const airUnits = friendlyUnits.filter(u => u.stats.isAir);

      if (meleeUnits.length > 0) {
        const meleeStrategy = this.meleeTactician.developMeleeStrategy(
          meleeUnits,
          enemyUnits,
          this.currentSituation
        );
        return { shouldAttack: true, reason: 'Melee advantage', details: meleeStrategy };
      }

      if (rangedUnits.length > 0) {
        const rangedStrategy = this.rangedTactician.developRangedStrategy(
          rangedUnits,
          enemyUnits,
          this.currentSituation
        );
        return { shouldAttack: true, reason: 'Ranged advantage', details: rangedStrategy };
      }

      if (airUnits.length > 0) {
        const airStrategy = this.airTactician.developAirStrategy(
          airUnits,
          enemyUnits,
          this.currentSituation
        );
        return { shouldAttack: true, reason: 'Air advantage', details: airStrategy };
      }
    }

    return { shouldAttack: false, reason: 'Not advantageous to attack' };
  }

  private evaluateDefense(
    friendlyUnits: Unit[],
    structures: Building[]
  ): { shouldDefend: boolean; reason: string; details?: DefensePlan } {
    if (!this.currentSituation) {
      return { shouldDefend: false, reason: 'No situation data' };
    }

    const threatLevel = this.currentSituation.threatLevel;
    const terrainDefensibility = this.currentSituation.terrainFeatures.get(TerrainFeature.DEFENSIBLE) || 0;

    if (threatLevel > 60 && terrainDefensibility > 0.5) {
      const defensePlan: DefensePlan = {
        position: this.currentSituation.position,
        assignedUnits: friendlyUnits.map(u => u.id),
        formation: this.formationManager.createFormation(
          FormationType.DEFENSIVE,
          this.currentSituation.position,
          friendlyUnits,
          this.getDefaultFacing()
        )!,
        fallbackPosition: this.findFallbackPosition(friendlyUnits),
        fortifiedLevel: terrainDefensibility
      };

      return { shouldDefend: true, reason: 'Defensive advantage', details: defensePlan };
    }

    return { shouldDefend: false, reason: 'Not advantageous to defend' };
  }

  public selectTarget(
    attacker: Unit,
    potentialTargets: (Unit | Building)[]
  ): Unit | Building | null {
    if (!this.currentSituation) return null;
    return this.targetSelector.selectTarget(attacker, potentialTargets, this.currentSituation);
  }

  public createFormation(
    type: FormationType,
    center: Vector2D,
    units: Unit[]
  ): FormationAssignment | null {
    if (units.length === 0) return null;
    return this.formationManager.createFormation(
      type,
      center,
      units,
      this.getDefaultFacing()
    );
  }

  public maintainFormation(
    formationId: string,
    units: Unit[],
    newCenter: Vector2D
  ): FormationAssignment | null {
    return this.formationManager.maintainFormation(
      formationId,
      units,
      newCenter,
      this.getDefaultFacing()
    );
  }

  public planFlankingManeuver(
    units: Unit[],
    enemyCenter: Vector2D,
    friendlyCenter: Vector2D
  ): FlankingManeuver | null {
    if (!this.currentSituation) return null;
    return this.flankingManager.planFlankingManeuver(
      units,
      enemyCenter,
      friendlyCenter,
      this.currentSituation.terrainFeatures
    );
  }

  public executeFlankingManeuver(maneuverId: string): boolean {
    return this.flankingManager.executeManeuver(maneuverId);
  }

  public updateFlankingManeuver(
    maneuverId: string,
    units: Unit[],
    enemyPositions: Vector2D[]
  ): ManeuverUpdate {
    return this.flankingManager.updateManeuver(maneuverId, units, enemyPositions);
  }

  public abortFlankingManeuver(maneuverId: string): void {
    this.flankingManager.abortManeuver(maneuverId);
  }

  public shouldRetreat(
    unit: Unit,
    nearbyEnemies: Unit[]
  ): RetreatDecision {
    if (!this.currentSituation) {
      return {
        shouldRetreat: false,
        reasons: [],
        severity: 0,
        survivalOddsIfStay: 1,
        survivalOddsIfRetreat: 1,
        recommendedPath: { waypoints: [], totalDistance: 0, estimatedTime: 0, coverPoints: [], dangerPoints: [] },
        urgency: 'low' as const,
        alternativeActions: []
      };
    }
    return this.retreatAnalyzer.shouldRetreat(unit, this.currentSituation, nearbyEnemies);
  }

  public planAbilityUse(
    abilityId: string,
    unit: Unit,
    targetPosition?: Vector2D,
    targetEntity?: string
  ): AbilityUsePlan | null {
    if (!this.currentSituation) return null;
    return this.abilityCoordinator.planAbilityUsage(
      abilityId,
      unit,
      this.currentSituation,
      targetPosition,
      targetEntity
    );
  }

  public executeAbility(
    unitId: string,
    abilityId: string,
    targetPosition?: Vector2D,
    targetEntityId?: string
  ): AbilityExecution {
    return this.abilityCoordinator.executeAbility(unitId, abilityId, targetPosition, targetEntityId);
  }

  public findSyncOpportunities(
    units: Unit[],
    syncType: string
  ): SyncOpportunity | null {
    if (!this.currentSituation) return null;
    return this.abilityCoordinator.findSyncOpportunities(units, this.currentSituation, syncType);
  }

  public createGroup(
    groupId: string,
    units: Unit[],
    leaderId: string,
    objective: TacticalObjective,
    tactics: GroupTactics
  ): GroupAssignment {
    return this.groupCoordinator.createGroup(groupId, units, leaderId, objective, tactics);
  }

  public assignUnitsToGroup(groupId: string, unitIds: string[]): GroupAssignment | null {
    return this.groupCoordinator.assignUnitsToGroup(groupId, unitIds);
  }

  public coordinateGroup(groupId: string): GroupCoordinationResult {
    if (!this.currentSituation) {
      return { success: false, orders: [], reason: 'No situation data' };
    }
    return this.groupCoordinator.coordinateGroup(groupId, this.currentSituation);
  }

  public dissolveGroup(groupId: string): GroupAssignment | null {
    return this.groupCoordinator.dissolveGroup(groupId);
  }

  public developMeleeStrategy(
    meleeUnits: Unit[],
    enemies: Unit[]
  ): MeleeStrategy | null {
    if (!this.currentSituation || meleeUnits.length === 0) return null;
    return this.meleeTactician.developMeleeStrategy(meleeUnits, enemies, this.currentSituation);
  }

  public developRangedStrategy(
    rangedUnits: Unit[],
    enemies: Unit[]
  ): RangedStrategy | null {
    if (!this.currentSituation || rangedUnits.length === 0) return null;
    return this.rangedTactician.developRangedStrategy(rangedUnits, enemies, this.currentSituation);
  }

  public developAirStrategy(
    airUnits: Unit[],
    enemies: Unit[]
  ): AirStrategy | null {
    if (!this.currentSituation || airUnits.length === 0) return null;
    return this.airTactician.developAirStrategy(airUnits, enemies, this.currentSituation);
  }

  public developSiegeStrategy(
    siegeUnits: Unit[],
    targetStructures: Building[]
  ): SiegeStrategy | null {
    if (!this.currentSituation || siegeUnits.length === 0 || targetStructures.length === 0) return null;
    return this.siegeTactician.developSiegeStrategy(siegeUnits, targetStructures, this.currentSituation);
  }

  public simulateCombat(
    friendlyUnits: Unit[],
    enemyUnits: Unit[],
    iterations: number = 100
  ): CombatSimulation {
    return this.combatSimulator.simulateCombat(friendlyUnits, enemyUnits, iterations);
  }

  public getTacticalScore(): TacticalScore | null {
    if (!this.currentSituation) return null;
    return this.combatAnalyzer.calculateTacticalScore(this.currentSituation);
  }

  public getCurrentSituation(): TacticalSituation | null {
    return this.currentSituation;
  }

  public getCombatMetrics(): CombatMetrics {
    return this.combatAnalyzer.getCombatMetrics();
  }

  public recordCombatMetrics(metrics: Partial<CombatMetrics>): void {
    this.combatAnalyzer.recordCombatMetrics(metrics);
  }

  public recordKill(targetId: string): void {
    this.targetSelector.recordKill(targetId);
  }

  public getFormationTemplate(type: FormationType): FormationTemplate | undefined {
    return this.formationManager.getFormationTemplate(type);
  }

  public getActiveFormations(): FormationAssignment[] {
    return this.formationManager.getActiveFormations();
  }

  public getFlankingStatistics(): FlankingStatistics {
    return this.flankingManager.getFlankingStatistics();
  }

  public getRetreatStatistics(): RetreatStatistics {
    return this.retreatAnalyzer.getRetreatStatistics();
  }

  public addSafeZone(zone: SafeZone): void {
    this.retreatAnalyzer.addSafeZone(zone);
  }

  public removeSafeZone(zoneId: string): void {
    this.retreatAnalyzer.removeSafeZone(zoneId);
  }

  public getAbilityStatistics(): AbilityStatistics {
    return this.abilityCoordinator.getAbilityStatistics();
  }

  public isAbilityReady(abilityId: string): boolean {
    return this.abilityCoordinator.isAbilityReady(abilityId);
  }

  public getRemainingCooldown(abilityId: string): number {
    return this.abilityCoordinator.getRemainingCooldown(abilityId);
  }

  public getGroupStatistics(): GroupStatistics {
    return this.groupCoordinator.getGroupStatistics();
  }

  public getAllGroups(): GroupAssignment[] {
    return this.groupCoordinator.getAllGroups();
  }

  public getActiveGroups(): GroupAssignment[] {
    return this.groupCoordinator.getActiveGroups();
  }

  public clearCaches(): void {
    this.combatAnalyzer.clearCache();
    this.combatSimulator.clearCache();
    this.abilityCoordinator.clearAbilityPlans();
  }

  private getDefaultFacing(): Vector2D {
    return { x: 1, y: 0 };
  }

  private findFallbackPosition(units: Unit[]): Vector2D | null {
    if (!this.currentSituation || units.length === 0) return null;

    const retreatScore = this.currentSituation.terrainFeatures.get(TerrainFeature.RETREAT_PATH) || 0;

    if (retreatScore > 0.3) {
      const direction = this.getDefaultFacing();
      return {
        x: units[0].position.x - direction.x * 150,
        y: units[0].position.y - direction.y * 150
      };
    }

    return units[0].position;
  }

  public evaluateTerrain(position: Vector2D, structures: Building[]): TerrainAnalysis {
    return {
      elevation: this.assessElevation(position),
      coverAvailable: this.assessCover(position),
      chokepointStrength: this.assessChokepoint(position, structures),
      defensibility: this.assessDefensibility(position, structures),
      flankingOpportunities: this.findFlankingOpportunities(position),
      retreatPaths: this.findRetreatPaths(position),
      optimalPositions: this.findOptimalPositions(position),
      threatVectors: this.calculateThreatVectors(position)
    };
  }

  private assessElevation(position: Vector2D): number {
    return Math.random() * 0.3 + 0.5;
  }

  private assessCover(position: Vector2D): number {
    return Math.random() * 0.4 + 0.3;
  }

  private assessChokepoint(position: Vector2D, structures: Building[]): number {
    return 0.3;
  }

  private assessDefensibility(position: Vector2D, structures: Building[]): number {
    return 0.5;
  }

  private findFlankingOpportunities(position: Vector2D): Vector2D[] {
    const opportunities: Vector2D[] = [];
    for (let i = 0; i < 3; i++) {
      const angle = (i / 3) * Math.PI * 2;
      opportunities.push({
        x: position.x + Math.cos(angle) * 100,
        y: position.y + Math.sin(angle) * 100
      });
    }
    return opportunities;
  }

  private findRetreatPaths(position: Vector2D): Vector2D[] {
    const paths: Vector2D[] = [];
    for (let i = 0; i < 2; i++) {
      paths.push({
        x: position.x - 100 * (i + 1),
        y: position.y + (Math.random() - 0.5) * 50
      });
    }
    return paths;
  }

  private findOptimalPositions(position: Vector2D): Vector2D[] {
    return [
      { x: position.x, y: position.y },
      { x: position.x + 50, y: position.y + 50 },
      { x: position.x - 50, y: position.y + 50 }
    ];
  }

  private calculateThreatVectors(position: Vector2D): Vector2D[] {
    return [
      { x: 1, y: 0 },
      { x: 0.707, y: 0.707 }
    ];
  }

  public predictCombatOutcome(
    friendlyUnits: Unit[],
    enemyUnits: Unit[]
  ): CombatOutcome {
    const simulation = this.combatSimulator.simulateCombat(friendlyUnits, enemyUnits, 50);
    
    const friendlyPower = friendlyUnits.reduce((sum, u) => 
      sum + u.stats.attackDamage * u.stats.attackSpeed * (u.health / u.maxHealth), 0);
    const enemyPower = enemyUnits.reduce((sum, u) => 
      sum + u.stats.attackDamage * u.stats.attackSpeed * (u.health / u.maxHealth), 0);

    const estimatedDamageToEnemy = simulation.enemyLosses * 100;
    const estimatedDamageToSelf = simulation.friendlyLosses * 100;

    return {
      estimatedDamageToEnemy,
      estimatedDamageToSelf,
      victoryProbability: simulation.victoryProbability,
      survivalProbability: 1 - (simulation.friendlyLosses / Math.max(1, friendlyUnits.length)),
      resourceGain: enemyUnits.length * 50 * simulation.victoryProbability,
      resourceLoss: friendlyUnits.length * 50 * (1 - simulation.victoryProbability),
      timeToResolution: simulation.duration,
      risks: this.identifyCombatRisks(friendlyUnits, enemyUnits, simulation)
    };
  }

  private identifyCombatRisks(
    friendlyUnits: Unit[],
    enemyUnits: Unit[],
    simulation: CombatSimulation
  ): CombatRisk[] {
    const risks: CombatRisk[] = [];

    if (simulation.friendlyLosses > friendlyUnits.length * 0.3) {
      risks.push({
        type: 'high_friendly_losses',
        severity: 0.8,
        probability: simulation.friendlyLosses / friendlyUnits.length,
        mitigation: 'Consider retreat or reinforcement'
      });
    }

    if (enemyUnits.length > friendlyUnits.length * 1.5) {
      risks.push({
        type: 'numerical_disadvantage',
        severity: 0.7,
        probability: 0.9,
        mitigation: 'Wait for reinforcements or use terrain advantage'
      });
    }

    return risks;
  }

  public getMovementOrders(
    unit: Unit,
    targetPosition: Vector2D,
    formationId?: string
  ): MovementOrder {
    const distance = this.calculateDistance(unit.position, targetPosition);
    const estimatedArrival = distance / unit.stats.moveSpeed;

    return {
      unitId: unit.id,
      targetPosition,
      formationId,
      pathType: this.determinePathType(distance),
      speed: unit.stats.moveSpeed,
      estimatedArrival,
      waypoints: this.generateWaypoints(unit.position, targetPosition)
    };
  }

  private determinePathType(distance: number): 'direct' | 'flanking' | 'cautious' {
    if (distance < 100) return 'direct';
    if (distance < 300) return 'cautious';
    return 'flanking';
  }

  private generateWaypoints(start: Vector2D, end: Vector2D): Vector2D[] {
    const waypoints: Vector2D[] = [start];
    const distance = this.calculateDistance(start, end);
    
    if (distance > 200) {
      const midX = (start.x + end.x) / 2;
      const midY = (start.y + end.y) / 2 - 30;
      waypoints.push({ x: midX, y: midY });
    }
    
    waypoints.push(end);
    return waypoints;
  }

  private calculateDistance(a: Vector2D, b: Vector2D): number {
    return Math.sqrt(Math.pow(b.x - a.x, 2) + Math.pow(b.y - a.y, 2));
  }

  public setTargetSelectionMode(mode: TargetSelectionMode): void {
    this.targetSelector.setSelectionMode(mode);
  }

  public addPreferredTarget(targetId: string): void {
    this.targetSelector.addPreferredTarget(targetId);
  }

  public removePreferredTarget(targetId: string): void {
    this.targetSelector.removePreferredTarget(targetId);
  }

  public addAvoidedTarget(targetId: string): void {
    this.targetSelector.addAvoidedTarget(targetId);
  }

  public clearRecentTargets(): void {
    this.targetSelector.clearRecentTargets();
  }

  public setPreferredFlankingAngle(style: 'aggressive' | 'balanced' | 'cautious'): void {
    this.flankingManager.setPreferredAngle(style);
  }

  public registerCustomFormation(template: FormationTemplate): void {
    this.formationManager.registerCustomFormation(template);
  }

  public addTacticalMemory(key: string, value: unknown): void {
    this.tacticalMemory.set(key, value);
  }

  public getTacticalMemory(key: string): unknown | undefined {
    return this.tacticalMemory.get(key);
  }

  public clearTacticalMemory(): void {
    this.tacticalMemory.clear();
  }

  public getSystemStatus(): SystemStatus {
    return {
      isActive: this.currentSituation !== null,
      lastUpdate: this.lastUpdate,
      currentPhase: this.currentSituation?.combatPhase || CombatPhase.NONE,
      threatLevel: this.currentSituation?.threatLevel || 0,
      activeFormations: this.formationManager.getActiveFormations().length,
      activeGroups: this.groupCoordinator.getActiveGroups().length,
      pendingManeuvers: this.flankingManager.getActiveManeuvers().length
    };
  }
}

interface DefensePlan {
  position: Vector2D;
  assignedUnits: string[];
  formation: FormationAssignment;
  fallbackPosition: Vector2D | null;
  fortifiedLevel: number;
}

interface MovementOrder {
  unitId: string;
  targetPosition: Vector2D;
  formationId?: string;
  pathType: 'direct' | 'flanking' | 'cautious';
  speed: number;
  estimatedArrival: number;
  waypoints: Vector2D[];
}

interface SystemStatus {
  isActive: boolean;
  lastUpdate: number;
  currentPhase: CombatPhase;
  threatLevel: number;
  activeFormations: number;
  activeGroups: number;
  pendingManeuvers: number;
}

export interface TacticalDecision {
  action: 'attack' | 'defend' | 'retreat' | 'hold' | 'idle';
  priority: number;
  reason: string;
  details?: unknown;
}
import { Vector2 } from '../../core/Vector2';
import { PathFinder } from '../../navigation/PathFinder';
import { UnitManager } from '../../units/UnitManager';
import { GameState, Unit, Building, Resource } from '../../core/GameState';
import { EventEmitter } from '../../core/EventEmitter';

export enum GoalType {
  EXPAND = 'expand',
  ATTACK = 'attack',
  DEFEND = 'defend',
  ECONOMY = 'economy',
  TECH = 'tech',
  HARASS = 'harass',
  SCOUT = 'scout',
  DENY = 'deny',
  OVERWHELM = 'overwhelm',
  RETREAT = 'retreat',
  GATHER = 'gather',
  BUILD = 'build',
  UPGRADE = 'upgrade',
  NUKE = 'nuke',
  SIEGE = 'siege'
}

export enum PlanStatus {
  ACTIVE = 'active',
  PAUSED = 'paused',
  COMPLETED = 'completed',
  FAILED = 'failed',
  CANCELLED = 'cancelled',
  PENDING = 'pending'
}

export enum StrategyType {
  AGGRESSIVE = 'aggressive',
  DEFENSIVE = 'defensive',
  BALANCED = 'balanced',
  ECONOMIC = 'economic',
  MILITARY = 'military',
  TECH_RUSH = 'tech_rush',
  ALL_IN = 'all_in',
  TURTLE = 'turtle',
  HARASS = 'harass',
  EXPAND_FAST = 'expand_fast',
  MAP_CONTROL = 'map_control'
}

export enum ResourceType {
  MINERALS = 'minerals',
  GAS = 'gas',
  ENERGY = 'energy',
  FOOD = 'food',
  WOOD = 'wood',
  GOLD = 'gold'
}

export enum ThreatLevel {
  NONE = 'none',
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high',
  CRITICAL = 'critical'
}

export enum TerrainType {
  LAND = 'land',
  WATER = 'water',
  AIR = 'air',
  IMPASSABLE = 'impassable',
  ELEVATED = 'elevated',
  CHOKEPOint = 'chokepoint',
  RESOURCE_RICH = 'resource_rich'
}

export enum GamePhase {
  EARLY_GAME = 'early_game',
  MID_GAME = 'mid_game',
  LATE_GAME = 'late_game',
  END_GAME = 'end_game'
}

export enum GoalStatus {
  ACTIVE = 'active',
  COMPLETED = 'completed',
  FAILED = 'failed',
  PAUSED = 'paused'
}

export enum PlanType {
  EXPANSION = 'expansion',
  ATTACK = 'attack',
  DEFENSE = 'defense',
  ECONOMIC = 'economic',
  TECH = 'tech',
  SCOUT = 'scout',
  HARASS = 'harass',
  COMPOUND = 'compound'
}

export enum StepStatus {
  PENDING = 'pending',
  IN_PROGRESS = 'in_progress',
  COMPLETED = 'completed',
  FAILED = 'failed',
  SKIPPED = 'skipped'
}

export enum ActionType {
  MOVE_UNIT = 'move_unit',
  ATTACK_UNIT = 'attack_unit',
  ATTACK_BUILDING = 'attack_building',
  BUILD_UNIT = 'build_unit',
  BUILD_BUILDING = 'build_building',
  GATHER_RESOURCE = 'gather_resource',
  RESEARCH_TECH = 'research_tech',
  EXPAND_BASE = 'expand_base',
  DEFEND_POSITION = 'defend_position',
  RETREAT = 'retreat',
  SCOUT_AREA = 'scout_area'
}

export enum PriorityLevel {
  CRITICAL = 0,
  HIGH = 1,
  MEDIUM = 2,
  LOW = 3
}

export enum ZoneType {
  STARTING = 'starting',
  EXPANSION = 'expansion',
  CONTESTED = 'contested',
  ENEMY_TERRITORY = 'enemy_territory',
  STRATEGIC = 'strategic',
  RESOURCE = 'resource',
  CHOKEPOint = 'chokepoint',
  DEFENSIBLE = 'defensible'
}

export interface StrategicGoal {
  id: string;
  type: GoalType;
  priority: PriorityLevel;
  status: GoalStatus;
  targetPosition?: Vector2;
  targetId?: string;
  deadline?: number;
  requirements: GoalRequirement[];
  progress: number;
  createdAt: number;
  completedAt?: number;
  dependencies: string[];
  constraints: GoalConstraint[];
  successCriteria: SuccessCriteria[];
}

export interface GoalRequirement {
  resourceType: ResourceType;
  amount: number;
  satisfied: boolean;
}

export interface GoalConstraint {
  type: string;
  parameter: string;
  value: any;
}

export interface SuccessCriteria {
  description: string;
  checkFunction: (state: GameState) => boolean;
  weight: number;
}

export interface StrategicPlan {
  id: string;
  name: string;
  type: PlanType;
  goalId: string;
  status: PlanStatus;
  steps: PlanStep[];
  estimatedDuration: number;
  actualDuration?: number;
  startTime?: number;
  completionTime?: number;
  score?: number;
  riskAssessment: RiskAssessment;
  cost: PlanCost;
  preconditions: Precondition[];
  postconditions: Postcondition[];
  rollbackPlan?: PlanStep[];
  parallelSteps: string[];
  criticalPath: string[];
}

export interface PlanStep {
  id: string;
  name: string;
  description: string;
  status: StepStatus;
  actions: PlanAction[];
  estimatedDuration: number;
  actualDuration?: number;
  startTime?: number;
  completionTime?: number;
  dependencies: string[];
  prerequisites: string[];
  effects: StepEffect[];
  rollbackActions?: PlanAction[];
  retryCount: number;
  maxRetries: number;
  skipConditions: SkipCondition[];
}

export interface PlanAction {
  id: string;
  type: ActionType;
  targetId?: string;
  targetPosition?: Vector2;
  parameters: ActionParameters;
  cost: PlanCost;
  duration: number;
  conditions: ActionCondition[];
  effects: ActionEffect[];
  rollbackAction?: PlanAction;
  resourceReservation?: ResourceReservation;
}

export interface ActionParameters {
  unitId?: string;
  buildingId?: string;
  techId?: string;
  position?: Vector2;
  targetPosition?: Vector2;
  unitType?: string;
  buildingType?: string;
  techType?: string;
  quantity?: number;
  priority?: number;
  [key: string]: any;
}

export interface ActionCondition {
  type: string;
  check: (state: GameState) => boolean;
  description: string;
  critical: boolean;
}

export interface ActionEffect {
  type: string;
  value: any;
  duration?: number;
  applicationTime: 'immediate' | 'delayed' | 'continuous';
}

export interface PlanCost {
  minerals: number;
  gas: number;
  energy: number;
  food: number;
  wood: number;
  gold: number;
  time: number;
  units: UnitCost[];
  opportunityCost?: number;
  risk?: number;
}

export interface UnitCost {
  unitType: string;
  quantity: number;
  currentCount: number;
}

export interface ResourceReservation {
  resourceType: ResourceType;
  amount: number;
  reservedAt: number;
  releaseAt?: number;
}

export interface Precondition {
  type: string;
  description: string;
  check: (state: GameState) => boolean;
}

export interface Postcondition {
  type: string;
  description: string;
  verify: (state: GameState) => boolean;
}

export interface StepEffect {
  type: string;
  target: string;
  value: any;
  applicationTime: 'immediate' | 'delayed' | 'continuous';
  duration?: number;
}

export interface SkipCondition {
  description: string;
  check: (state: GameState) => boolean;
  alternativeStepId?: string;
}

export interface PlanEvaluation {
  planId: string;
  overallScore: number;
  tacticalScore: number;
  strategicScore: number;
  riskScore: number;
  costEfficiency: number;
  timeEfficiency: number;
  successProbability: number;
  resourceAvailability: ResourceAvailability;
  riskAssessment: RiskAssessment;
  comparisonRank: number;
  strengths: string[];
  weaknesses: string[];
  recommendations: string[];
  timestamp: number;
}

export interface ResourceAvailability {
  minerals: AvailabilityScore;
  gas: AvailabilityScore;
  energy: AvailabilityScore;
  food: AvailabilityScore;
}

export interface AvailabilityScore {
  current: number;
  required: number;
  sufficient: boolean;
  shortageRisk: number;
}

export interface RiskAssessment {
  overallRisk: ThreatLevel;
  riskFactors: RiskFactor[];
  totalRiskScore: number;
  mitigationStrategies: MitigationStrategy[];
  contingencyPlans: ContingencyPlan[];
  fallbackPlan?: StrategicPlan;
}

export interface RiskFactor {
  type: string;
  probability: number;
  impact: number;
  severity: ThreatLevel;
  description: string;
  affectedAreas: string[];
  warningSigns: string[];
}

export interface MitigationStrategy {
  riskType: string;
  strategy: string;
  implementation: string;
  effectiveness: number;
  resourceCost: PlanCost;
  triggerConditions: string[];
}

export interface ContingencyPlan {
  triggerCondition: string;
  plan: StrategicPlan;
  activationTime: number;
  priority: PriorityLevel;
}

export interface StrategicZone {
  id: string;
  type: ZoneType;
  center: Vector2;
  radius: number;
  boundary: Vector2[];
  importance: number;
  controlLevel: number;
  controlledBy: 'player' | 'enemy' | 'neutral';
  resources: Resource[];
  strategicValue: number;
  defensibility: number;
  visibility: number;
  accessPoints: Vector2[];
  chokepoints: Chokepoint[];
  adjacentZones: string[];
}

export interface EnemyProfile {
  id: string;
  lastKnownPosition?: Vector2;
  basePosition?: Vector2;
  unitCount: number;
  buildingCount: number;
  resourceCount: number;
  knownUnits: string[];
  knownBuildings: string[];
  technologyLevel: number;
  estimatedArmySize: number;
  estimatedArmyComposition: UnitComposition;
  threatLevel: ThreatLevel;
  aggressionLevel: number;
  expansionRate: number;
  resourceGatheringRate: number;
  behaviorPatterns: BehaviorPattern[];
  currentIntent: string;
  predictedIntent: string;
  confidenceLevel: number;
  lastUpdate: number;
  behaviorHistory: BehaviorSnapshot[];
}

export interface BehaviorPattern {
  patternType: string;
  frequency: number;
  triggerConditions: string[];
  averageDuration: number;
  successRate: number;
  lastOccurrence?: number;
  predictedNextOccurrence?: number;
}

export interface BehaviorSnapshot {
  timestamp: number;
  action: string;
  target: string;
  position: Vector2;
  outcome: string;
  context: Record<string, any>;
}

export interface ResourceAnalysis {
  currentResources: ResourceState;
  incomeRate: IncomeRate;
  expenditureRate: ExpenditureRate;
  projectedResources: ProjectedResource[];
  gatheringEfficiency: GatheringEfficiency;
  resourceDistribution: ResourceDistribution;
  bottlenecks: ResourceBottleneck[];
  optimizationSuggestions: OptimizationSuggestion[];
}

export interface ResourceState {
  minerals: number;
  gas: number;
  energy: number;
  food: number;
  wood: number;
  gold: number;
  storageCapacity: ResourceCapacity;
  reserves: ResourceReserves;
}

export interface ResourceCapacity {
  minerals: { current: number; max: number };
  gas: { current: number; max: number };
  energy: { current: number; max: number };
  food: { current: number; max: number };
}

export interface ResourceReserves {
  minerals: number;
  gas: number;
  energy: number;
  food: number;
}

export interface IncomeRate {
  mineralsPerMinute: number;
  gasPerMinute: number;
  energyPerMinute: number;
  foodPerMinute: number;
  sources: IncomeSource[];
}

export interface IncomeSource {
  type: ResourceType;
  source: string;
  amount: number;
  efficiency: number;
}

export interface ExpenditureRate {
  mineralsPerMinute: number;
  gasPerMinute: number;
  energyPerMinute: number;
  foodPerMinute: number;
  categories: ExpenditureCategory[];
}

export interface ExpenditureCategory {
  category: string;
  resourceType: ResourceType;
  amount: number;
  percentage: number;
  isEssential: boolean;
}

export interface ProjectedResource {
  time: number;
  minerals: number;
  gas: number;
  energy: number;
  food: number;
  potentialDeficit: ResourceDeficit[];
}

export interface ResourceDeficit {
  resourceType: ResourceType;
  deficitAmount: number;
  timeUntilDeficit: number;
  severity: ThreatLevel;
}

export interface GatheringEfficiency {
  overallEfficiency: number;
  workerEfficiency: WorkerEfficiency[];
  resourceNodeUtilization: ResourceNodeUtilization[];
  travelTimeOptimization: TravelTimeOptimization;
}

export interface WorkerEfficiency {
  workerId: string;
  resourceType: ResourceType;
  resourcesPerMinute: number;
  utilizationRate: number;
  optimalAssignment: boolean;
}

export interface ResourceNodeUtilization {
  nodeId: string;
  resourceType: ResourceType;
  position: Vector2;
  assignedWorkers: number;
  extractionRate: number;
  capacityUtilization: number;
  distanceFromBase: number;
}

export interface TravelTimeOptimization {
  averageTravelTime: number;
  optimalWorkerCount: number;
  suggestedAssignments: SuggestedAssignment[];
  routeEfficiency: number;
}

export interface SuggestedAssignment {
  workerId: string;
  resourceNodeId: string;
  expectedGain: number;
  distance: number;
}

export interface ResourceDistribution {
  minerals: SpatialDistribution;
  gas: SpatialDistribution;
  energy: SpatialDistribution;
  food: SpatialDistribution;
  balance: number;
}

export interface SpatialDistribution {
  averageDistance: number;
  variance: number;
  nearestSource: Vector2;
  farthestSource: Vector2;
  density: number;
}

export interface ResourceBottleneck {
  resourceType: ResourceType;
  severity: ThreatLevel;
  cause: string;
  impact: string;
  solution: string;
  estimatedFixTime: number;
}

export interface OptimizationSuggestion {
  type: string;
  description: string;
  expectedGain: number;
  implementationCost: PlanCost;
  priority: PriorityLevel;
  riskLevel: ThreatLevel;
}

export interface MapAnalysis {
  terrain: TerrainMap;
  strategicZones: StrategicZone[];
  chokepoints: Chokepoint[];
  expansionSites: ExpansionSite[];
  defensivePositions: DefensivePosition[];
  attackRoutes: AttackRoute[];
  resourceLocations: ResourceLocation[];
  visibilityMap: VisibilityMap;
  pathfindingAnalysis: PathfindingAnalysis;
}

export interface TerrainMap {
  width: number;
  height: number;
  terrainTypes: TerrainType[][];
  elevation: number[][];
  walkability: boolean[][];
  buildability: boolean[][];
}

export interface Chokepoint {
  id: string;
  position: Vector2;
  width: number;
  type: 'narrow' | 'wide' | 'elevated';
  importance: StrategicImportance;
  controllingEntity?: string;
  controlLevel: number;
  adjacentZones: string[];
  bypassOptions: Vector2[];
  defensiveBonus: number;
  attackAdvantage: number;
}

export interface StrategicImportance {
  level: number;
  reason: string;
  controllingAdvantage: number;
}

export interface ExpansionSite {
  id: string;
  position: Vector2;
  resources: Resource[];
  resourceValue: number;
  defensibility: number;
  accessibility: number;
  proximityToBase: number;
  proximityToEnemy: number;
  buildingSlots: number;
  recommendedBuildings: string[];
  constructionTime: number;
  riskLevel: ThreatLevel;
  strategicValue: number;
}

export interface DefensivePosition {
  id: string;
  position: Vector2;
  type: 'bunker' | 'chokepoint' | 'elevated' | 'ambush';
  coverage: number;
  sightLines: SightLine[];
  optimalDefenders: number;
  fallbackPosition?: Vector2;
  chokepointId?: string;
  naturalBarriers: Vector2[];
  recommendedUnits: string[];
  constructionRequirements: PlanCost;
  effectiveness: number;
}

export interface SightLine {
  start: Vector2;
  end: Vector2;
  blocked: boolean;
  blockedBy?: Vector2;
}

export interface AttackRoute {
  id: string;
  startPosition: Vector2;
  endPosition: Vector2;
  waypoints: Vector2[];
  distance: number;
  estimatedTime: number;
  riskLevel: ThreatLevel;
  terrainTypes: TerrainType[];
  chokepointsCrossed: string[];
  enemyPresence: number;
  coverQuality: number;
  recommendedUnits: string[];
  alternativeRoutes: string[];
}

export interface ResourceLocation {
  id: string;
  position: Vector2;
  type: ResourceType;
  amount: number;
  quality: number;
  accessibility: number;
  contested: boolean;
  contestedBy?: string[];
  optimalGatherers: string[];
  gatheringDistance: number;
}

export interface VisibilityMap {
  playerVision: boolean[][];
  enemyVision: boolean[][];
  contestedAreas: Vector2[];
  unexploredAreas: Vector2[];
  recentlyExplored: RecentlyExplored[];
  enemyBaseLocation?: Vector2;
  confidenceLevel: number;
}

export interface RecentlyExplored {
  position: Vector2;
  lastSeen: number;
  enemyPresence: boolean;
}

export interface PathfindingAnalysis {
  averagePathLength: number;
  blockedPaths: BlockedPath[];
  optimalRoutes: OptimalRoute[];
  congestionPoints: CongestionPoint[];
  recommendedPaths: RecommendedPosition[];
}

export interface BlockedPath {
  start: Vector2;
  end: Vector2;
  blockedBy: Vector2[];
  alternativePathExists: boolean;
}

export interface OptimalRoute {
  from: Vector2;
  to: Vector2;
  waypoints: Vector2[];
  distance: number;
  safetyScore: number;
}

export interface CongestionPoint {
  position: Vector2;
  averageUnits: number;
  peakUnits: number;
  timeOfPeak: number;
  solution: string;
}

export interface RecommendedPosition {
  position: Vector2;
  type: 'defensive' | 'offensive' | 'scout' | 'gather';
  purpose: string;
  effectiveness: number;
  requirements: PlanCost;
}

export interface GoalGenerator {
  generateGoals(gameState: GameState): StrategicGoal[];
  prioritizeGoals(goals: StrategicGoal[]): StrategicGoal[];
  updateGoalProgress(goalId: string, progress: number): void;
  assessGoalCompletion(goalId: string, gameState: GameState): boolean;
  generateSubGoals(parentGoalId: string): StrategicGoal[];
}

export interface PlanGenerator {
  generatePlan(goal: StrategicGoal, gameState: GameState): StrategicPlan;
  refinePlan(plan: StrategicPlan, gameState: GameState): StrategicPlan;
  validatePlan(plan: StrategicPlan, gameState: GameState): boolean;
  estimatePlanDuration(plan: StrategicPlan, gameState: GameState): number;
  generateAlternativePlans(goal: StrategicGoal, gameState: GameState): StrategicPlan[];
}

export interface PlanEvaluator {
  evaluatePlan(plan: StrategicPlan, gameState: GameState): PlanEvaluation;
  comparePlans(plans: StrategicPlan[], gameState: GameState): PlanComparison[];
  rankPlans(plans: StrategicPlan[], gameState: GameState): StrategicPlan[];
  assessPlanRisk(plan: StrategicPlan, gameState: GameState): RiskAssessment;
  estimateSuccessProbability(plan: StrategicPlan, gameState: GameState): number;
}

export interface PlanComparison {
  planA: StrategicPlan;
  planB: StrategicPlan;
  comparisonScores: ComparisonScore[];
  recommendation: string;
  preferredPlan: string;
}

export interface ComparisonScore {
  criteria: string;
  scoreA: number;
  scoreB: number;
  winner: string;
  weight: number;
}

export interface StrategySelector {
  selectStrategy(gameState: GameState, availablePlans: StrategicPlan[]): StrategyType;
  evaluateStrategy(strategy: StrategyType, gameState: GameState): StrategyEvaluation;
  transitionStrategy(from: StrategyType, to: StrategyType, gameState: GameState): StrategyTransition;
  getStrategyParameters(strategy: StrategyType): StrategyParameters;
  adaptStrategy(strategy: StrategyType, gameState: GameState): StrategyType;
}

export interface StrategyEvaluation {
  strategy: StrategyType;
  overallScore: number;
  strengths: string[];
  weaknesses: string[];
  fitScore: number;
  riskLevel: ThreatLevel;
  expectedOutcome: string;
}

export interface StrategyTransition {
  from: StrategyType;
  to: StrategyType;
  transitionPlan: StrategicPlan;
  estimatedTransitionTime: number;
  risks: string[];
  benefits: string[];
}

export interface StrategicMemory {
  recordDecision(decision: StrategicDecision): void;
  getHistoricalDecisions(filter?: DecisionFilter): StrategicDecision[];
  analyzeDecisionPatterns(): DecisionPattern[];
  learnFromOutcome(outcome: PlanOutcome): void;
  getStrategicInsights(): StrategicInsight[];
  adjustParameters(insights: StrategicInsight[]): void;
}

export interface StrategicDecision {
  id: string;
  timestamp: number;
  decisionType: string;
  context: DecisionContext;
  chosenOption: string;
  alternativeOptions: string[];
  reasoning: string;
  expectedOutcome: string;
  actualOutcome?: string;
  success: boolean;
  gameState: GameState;
}

export interface DecisionContext {
  gamePhase: GamePhase;
  resourceState: ResourceState;
  threatLevel: ThreatLevel;
  mapControl: number;
  armySize: number;
  enemyStrength: number;
}

export interface DecisionFilter {
  startTime?: number;
  endTime?: number;
  decisionType?: string;
  successOnly?: boolean;
}

export interface DecisionPattern {
  patternType: string;
  frequency: number;
  averageOutcome: number;
  conditions: string[];
  recommendations: string[];
}

export interface PlanOutcome {
  planId: string;
  success: boolean;
  completionTime?: number;
  actualCost: PlanCost;
  unexpectedEffects: string[];
  lessons: string[];
  performanceAnalysis: PerformanceAnalysis;
}

export interface PerformanceAnalysis {
  efficiency: number;
  accuracy: number;
  deviationFromPlan: number;
  resourceWaste: PlanCost;
  timeOverrun: number;
  qualityOfExecution: number;
}

export interface StrategicInsight {
  type: string;
  description: string;
  confidence: number;
  evidence: string[];
  recommendations: string[];
  timestamp: number;
}

export interface StrategicDecisionOutcome {
  decisionId: string;
  outcome: 'success' | 'partial_success' | 'failure';
  metrics: OutcomeMetrics;
  lessons: string[];
  timestamp: number;
}

export interface OutcomeMetrics {
  objectiveCompletion: number;
  resourceEfficiency: number;
  timeEfficiency: number;
  riskMitigation: number;
  strategicAdvancement: number;
}

export interface ExpansionPlanner {
  identifyExpansionSites(gameState: GameState): ExpansionSite[];
  evaluateExpansionSite(site: ExpansionSite, gameState: GameState): ExpansionEvaluation;
  generateExpansionPlan(site: ExpansionSite, gameState: GameState): StrategicPlan;
  optimizeExpansionTiming(gameState: GameState): ExpansionTiming;
  assessExpansionRisk(site: ExpansionSite, gameState: GameState): RiskAssessment;
  prioritizeExpansions(sites: ExpansionSite[], gameState: GameState): ExpansionSite[];
}

export interface ExpansionEvaluation {
  site: ExpansionSite;
  overallScore: number;
  resourceScore: number;
  defensiveScore: number;
  economicScore: number;
  strategicScore: number;
  timingScore: number;
  riskScore: number;
  recommendation: 'highly_recommended' | 'recommended' | 'neutral' | 'not_recommended';
}

export interface ExpansionTiming {
  optimalTime: number;
  earliestViableTime: number;
  latestViableTime: number;
  resourceRequirements: PlanCost;
  riskFactors: string[];
  indicators: TimingIndicator[];
}

export interface TimingIndicator {
  type: string;
  value: number;
  threshold: number;
  current: boolean;
  description: string;
}

export interface TechPlanner {
  analyzeTechTree(gameState: GameState): TechAnalysis;
  identifyTechOpportunities(gameState: GameState): TechOpportunity[];
  prioritizeTechs(techs: TechOpportunity[], gameState: GameState): TechOpportunity[];
  generateTechPlan(techs: TechOpportunity[], gameState: GameState): StrategicPlan;
  assessTechAdvantages(enemies: EnemyProfile[]): TechAdvantage[];
  optimizeTechTiming(gameState: GameState): TechTiming;
}

export interface TechAnalysis {
  currentLevel: number;
  availableTechs: TechNode[];
  inProgressTechs: TechNode[];
  completedTechs: TechNode[];
  techDependencies: TechDependency[];
  recommendedTechPath: string[];
  estimatedCompletionTime: number;
  resourceRequirements: PlanCost;
}

export interface TechNode {
  id: string;
  name: string;
  cost: PlanCost;
  researchTime: number;
  prerequisites: string[];
  unlocks: string[];
  benefits: TechBenefit[];
  dependencies: TechDependency[];
}

export interface TechBenefit {
  type: string;
  value: any;
  description: string;
}

export interface TechDependency {
  techId: string;
  type: 'requires' | 'enhances' | 'conflicts';
}

export interface TechOpportunity {
  tech: TechNode;
  strategicValue: number;
  timing: number;
  dependencies: string[];
  risks: string[];
  benefits: TechBenefit[];
  alternativePaths: string[];
}

export interface TechAdvantage {
  techId: string;
  advantageType: string;
  advantageLevel: number;
  comparedTo: string[];
  estimatedImpact: number;
  counterOptions: string[];
}

export interface TechTiming {
  optimalStartTime: number;
  researchDuration: number;
  resourceAvailability: PlanCost;
  windowOfOpportunity: TimeWindow;
  risks: string[];
}

export interface TimeWindow {
  start: number;
  end: number;
  duration: number;
  conditions: string[];
}

export interface EconomyPlanner {
  analyzeEconomy(gameState: GameState): EconomyAnalysis;
  identifyOptimizationOpportunities(gameState: GameState): GatheringOptimization[];
  optimizeResourceDistribution(gameState: GameState): void;
  projectResourceNeeds(futureTime: number, gameState: GameState): ProjectedResource[];
  balanceEconomy(gameState: GameState): EconomicPlan;
  manageIncomeExpenditure(gameState: GameState): IncomeExpenditurePlan;
}

export interface EconomyAnalysis {
  currentState: EconomicState;
  growthRate: GrowthRate;
  efficiency: EconomicEfficiency;
  projections: EconomicProjection[];
  bottlenecks: EconomicBottleneck[];
  opportunities: EconomicOpportunity[];
  recommendations: EconomicRecommendation[];
}

export interface EconomicState {
  income: IncomeRate;
  expenses: ExpenditureRate;
  reserves: ResourceReserves;
  workerAllocation: WorkerAllocation;
  resourceManagement: ResourceManagement;
}

export interface WorkerAllocation {
  minerals: number;
  gas: number;
  energy: number;
  food: number;
  military: number;
  building: number;
  idle: number;
  optimal: boolean;
}

export interface ResourceManagement {
  gatheringPriority: ResourceType[];
  storagePriority: ResourceType[];
  reservePolicy: string;
  investmentStrategy: string;
}

export interface GrowthRate {
  minerals: number;
  gas: number;
  energy: number;
  food: number;
  armyValue: number;
  economicValue: number;
}

export interface EconomicEfficiency {
  overall: number;
  gathering: number;
  spending: number;
  saving: number;
  investment: number;
}

export interface EconomicProjection {
  time: number;
  projectedIncome: IncomeRate;
  projectedExpenses: ExpenditureRate;
  projectedBalance: ResourceReserves;
  confidence: number;
}

export interface EconomicBottleneck {
  type: string;
  severity: ThreatLevel;
  cause: string;
  solution: string;
  estimatedCost: PlanCost;
}

export interface EconomicOpportunity {
  type: string;
  description: string;
  potentialGain: number;
  implementationCost: PlanCost;
  riskLevel: ThreatLevel;
  timeSensitivity: number;
}

export interface EconomicRecommendation {
  priority: PriorityLevel;
  type: string;
  description: string;
  expectedOutcome: string;
  implementationSteps: string[];
}

export interface GatheringOptimization {
  workerId: string;
  currentAssignment: ResourceType;
  optimalAssignment: ResourceType;
  expectedGain: number;
  implementationCost: PlanCost;
  riskLevel: ThreatLevel;
}

export interface EconomicPlan {
  actions: EconomicAction[];
  resourceAllocation: ResourceAllocation[];
  timeline: EconomicTimeline;
  riskMitigation: string[];
}

export interface EconomicAction {
  type: string;
  target: string;
  parameters: any;
  expectedOutcome: string;
}

export interface ResourceAllocation {
  resourceType: ResourceType;
  amount: number;
  purpose: string;
  priority: PriorityLevel;
}

export interface EconomicTimeline {
  phases: EconomicPhase[];
  milestones: EconomicMilestone[];
}

export interface EconomicPhase {
  duration: number;
  focus: string;
  actions: EconomicAction[];
}

export interface EconomicMilestone {
  time: number;
  achievement: string;
  successCriteria: string[];
}

export interface IncomeExpenditurePlan {
  projectedIncome: IncomeRate;
  projectedExpenses: ExpenditureRate;
  balanceProjection: ResourceReserves[];
  adjustments: BudgetAdjustment[];
  contingencies: ContingencyBudget[];
}

export interface BudgetAdjustment {
  category: string;
  currentAmount: number;
  adjustedAmount: number;
  reason: string;
}

export interface ContingencyBudget {
  triggerCondition: string;
  resourceType: ResourceType;
  amount: number;
  priority: PriorityLevel;
}

export interface MilitaryPlanner {
  analyzeMilitary(gameState: GameState): ArmyAnalysis;
  recommendUnitComposition(enemyComposition: UnitComposition, gameState: GameState): UnitComposition;
  planArmyMovement(plan: StrategicPlan, gameState: GameState): MovementPlan;
  optimizeArmyPositioning(gameState: GameState): PositionOptimization;
  coordinateMultiArmy(armies: Army[], gameState: GameState): CoordinatedAttack;
  assessBattleOutcome(attackPlan: StrategicPlan, gameState: GameState): BattleAssessment;
}

export interface ArmyAnalysis {
  totalUnits: number;
  unitComposition: UnitComposition;
  armyValue: number;
  effectiveness: number;
  strengths: string[];
  weaknesses: string[];
  recommendedImprovements: string[];
  comparison: ArmyComparison;
}

export interface UnitComposition {
  units: UnitTypeCount[];
  totalValue: number;
  strengths: string[];
  weaknesses: string[];
  recommendedCount: RecommendedCount[];
  counters: CounterUnit[];
}

export interface UnitTypeCount {
  type: string;
  count: number;
  value: number;
  health: number;
  damage: number;
  speed: number;
}

export interface RecommendedCount {
  unitType: string;
  current: number;
  recommended: number;
  priority: PriorityLevel;
}

export interface CounterUnit {
  threatUnit: string;
  counterUnits: CounterUnitInfo[];
  tactic: string;
}

export interface CounterUnitInfo {
  unitType: string;
  effectiveness: number;
  ratio: number;
}

export interface ArmyComparison {
  ourArmy: ArmyAnalysis;
  enemyArmy: ArmyAnalysis;
  advantage: number;
  recommendation: string;
}

export interface Army {
  id: string;
  name: string;
  units: Unit[];
  commander?: string;
  position: Vector2;
  status: 'idle' | 'moving' | 'attacking' | 'defending';
  morale: number;
  strength: number;
}

export interface MovementPlan {
  armyId: string;
  waypoints: Vector2[];
  formation: Formation;
  speed: number;
  estimatedArrival: number;
  risks: MovementRisk[];
}

export interface Formation {
  type: 'line' | 'circle' | 'wedge' | 'flank' | 'scattered';
  spacing: number;
  orientation: Vector2;
}

export interface MovementRisk {
  type: string;
  location: Vector2;
  severity: ThreatLevel;
  mitigation: string;
}

export interface PositionOptimization {
  positions: RecommendedPosition[];
  coverage: number;
  defensiveStrength: number;
  offensiveCapability: number;
  resourceProtection: number;
}

export interface CoordinatedAttack {
  armies: Army[];
  target: Vector2;
  coordinationPlan: CoordinationStep[];
  timing: number;
  contingencyPlans: ContingencyPlan[];
}

export interface CoordinationStep {
  armyId: string;
  action: string;
  position: Vector2;
  timing: number;
  triggerConditions: string[];
}

export interface BattleAssessment {
  ourStrength: number;
  enemyStrength: number;
  victoryProbability: number;
  expectedCasualties: CasualtyEstimate;
  terrainAdvantage: number;
  recommendations: string[];
  risks: BattleRisk[];
}

export interface CasualtyEstimate {
  ourCasualties: number;
  enemyCasualties: number;
  acceptableThreshold: number;
  resourceValue: number;
}

export interface BattleRisk {
  type: string;
  probability: number;
  impact: number;
  mitigation: string;
}

export interface StrategyParameters {
  aggressionLevel: number;
  expansionRate: number;
  techInvestment: number;
  defenseLevel: number;
  resourceReserve: number;
  riskTolerance: number;
  preferredUnits: string[];
  preferredStrategy: StrategyType;
}

export interface GoalPriority {
  goalType: GoalType;
  basePriority: number;
  currentPriority: number;
  factors: PriorityFactor[];
  lastCalculated: number;
}

export interface PriorityFactor {
  factor: string;
  weight: number;
  value: number;
  impact: number;
}

export interface GoalGeneratorConfig {
  maxConcurrentGoals: number;
  goalEvaluationInterval: number;
  adaptivePriorities: boolean;
  considerEnemyActions: boolean;
  timeHorizon: number;
}

export interface PlanGeneratorConfig {
  maxPlanSteps: number;
  planEvaluationDepth: number;
  allowParallelExecution: boolean;
  riskThreshold: number;
  costOptimization: boolean;
}

export interface StrategySelectorConfig {
  evaluationInterval: number;
  transitionThreshold: number;
  considerHistoricalPerformance: boolean;
  riskAdjustmentFactor: number;
  flexibilityLevel: number;
}

export class AIStrategicPlanner {
  private gameState: GameState;
  private pathFinder: PathFinder;
  private unitManager: UnitManager;
  private eventEmitter: EventEmitter;
  
  private resourceAnalysis: ResourceAnalysisImpl;
  private mapAnalysis: MapAnalysisImpl;
  private enemyAnalysis: EnemyAnalysisImpl;
  private goalGenerator: GoalGeneratorImpl;
  private planGenerator: PlanGeneratorImpl;
  private planEvaluator: PlanEvaluatorImpl;
  private strategySelector: StrategySelectorImpl;
  private strategicMemory: StrategicMemoryImpl;
  private expansionPlanner: ExpansionPlannerImpl;
  private techPlanner: TechPlannerImpl;
  private economyPlanner: EconomyPlannerImpl;
  private militaryPlanner: MilitaryPlannerImpl;
  
  private activeGoals: Map<string, StrategicGoal> = new Map();
  private activePlans: Map<string, StrategicPlan> = new Map();
  private currentStrategy: StrategyType = StrategyType.BALANCED;
  private lastUpdateTime: number = 0;
  private updateInterval: number = 1000;
  private isPaused: boolean = false;

  constructor(
    gameState: GameState,
    pathFinder: PathFinder,
    unitManager: UnitManager,
    eventEmitter?: EventEmitter
  ) {
    this.gameState = gameState;
    this.pathFinder = pathFinder;
    this.unitManager = unitManager;
    this.eventEmitter = eventEmitter || new EventEmitter();
    
    this.initializeComponents();
    this.setupEventListeners();
  }

  private initializeComponents(): void {
    this.resourceAnalysis = new ResourceAnalysisImpl(this.gameState);
    this.mapAnalysis = new MapAnalysisImpl(this.gameState, this.pathFinder);
    this.enemyAnalysis = new EnemyAnalysisImpl(this.gameState);
    
    this.goalGenerator = new GoalGeneratorImpl(
      this.gameState,
      this.resourceAnalysis,
      this.mapAnalysis,
      this.enemyAnalysis
    );
    
    this.planGenerator = new PlanGeneratorImpl(
      this.gameState,
      this.pathFinder,
      this.unitManager
    );
    
    this.planEvaluator = new PlanEvaluatorImpl(
      this.gameState,
      this.resourceAnalysis,
      this.mapAnalysis
    );
    
    this.strategySelector = new StrategySelectorImpl(
      this.gameState,
      this.resourceAnalysis,
      this.enemyAnalysis
    );
    
    this.strategicMemory = new StrategicMemoryImpl();
    
    this.expansionPlanner = new ExpansionPlannerImpl(
      this.gameState,
      this.mapAnalysis,
      this.resourceAnalysis
    );
    
    this.techPlanner = new TechPlannerImpl(
      this.gameState,
      this.resourceAnalysis
    );
    
    this.economyPlanner = new EconomyPlannerImpl(
      this.gameState,
      this.resourceAnalysis
    );
    
    this.militaryPlanner = new MilitaryPlannerImpl(
      this.gameState,
      this.unitManager,
      this.enemyAnalysis,
      this.mapAnalysis
    );
  }

  private setupEventListeners(): void {
    this.eventEmitter.on('goal_completed', (goal: StrategicGoal) => {
      this.handleGoalCompletion(goal);
    });
    
    this.eventEmitter.on('plan_failed', (plan: StrategicPlan) => {
      this.handlePlanFailure(plan);
    });
    
    this.eventEmitter.on('strategy_transition', (transition: StrategyTransition) => {
      this.handleStrategyTransition(transition);
    });
  }

  public update(deltaTime: number): void {
    if (this.isPaused) return;
    
    const currentTime = Date.now();
    if (currentTime - this.lastUpdateTime < this.updateInterval) return;
    
    this.lastUpdateTime = currentTime;
    
    this.updateEnemyAnalysis();
    this.updateGoalProgress();
    this.evaluateActivePlans();
    this.selectOptimalStrategy();
    this.adaptToGameState();
  }

  private updateEnemyAnalysis(): void {
    this.enemyAnalysis.analyzeAllEnemies();
  }

  private updateGoalProgress(): void {
    this.activeGoals.forEach((goal, goalId) => {
      if (goal.status === GoalStatus.ACTIVE) {
        const progress = this.calculateGoalProgress(goal);
        this.goalGenerator.updateGoalProgress(goalId, progress);
        
        if (progress >= 100) {
          goal.status = GoalStatus.COMPLETED;
          goal.completedAt = Date.now();
          this.eventEmitter.emit('goal_completed', goal);
        }
      }
    });
  }

  private calculateGoalProgress(goal: StrategicGoal): number {
    let progress = goal.progress;
    
    switch (goal.type) {
      case GoalType.EXPAND:
        progress = this.calculateExpansionProgress(goal);
        break;
      case GoalType.ATTACK:
        progress = this.calculateAttackProgress(goal);
        break;
      case GoalType.DEFEND:
        progress = this.calculateDefenseProgress(goal);
        break;
      case GoalType.ECONOMY:
        progress = this.calculateEconomyProgress(goal);
        break;
      case GoalType.TECH:
        progress = this.calculateTechProgress(goal);
        break;
    }
    
    return progress;
  }

  private calculateExpansionProgress(goal: StrategicGoal): number {
    if (!goal.targetPosition) return 0;
    
    const buildings = this.gameState.getBuildings();
    const expanded = buildings.some(b => 
      Vector2.distance(b.position, goal.targetPosition!) < 150
    );
    
    return expanded ? 100 : 0;
  }

  private calculateAttackProgress(goal: StrategicGoal): number {
    if (!goal.targetPosition) return 0;
    
    const enemyBuildings = this.gameState.getEnemyBuildings();
    const destroyed = !enemyBuildings.some(b => 
      b.type === 'command_center' || b.type === 'nexus' || b.type === 'hatchery'
    );
    
    return destroyed ? 100 : 50;
  }

  private calculateDefenseProgress(goal: StrategicGoal): number {
    const combatUnits = this.gameState.getPlayerUnits().filter(u => u.isCombatUnit).length;
    const defensiveBuildings = this.gameState.getBuildings().filter(b => 
      b.type === 'turret' || b.type === 'bunker'
    ).length;
    
    const defenseScore = Math.min(100, (combatUnits / 10) * 50 + (defensiveBuildings / 5) * 50);
    return defenseScore;
  }

  private calculateEconomyProgress(goal: StrategicGoal): number {
    const income = this.resourceAnalysis.calculateIncomeRate();
    const targetIncome = 300;
    
    return Math.min(100, (income.mineralsPerMinute / targetIncome) * 100);
  }

  private calculateTechProgress(goal: StrategicGoal): number {
    const buildings = this.gameState.getBuildings();
    const researchBuilding = buildings.some(b => 
      b.type === 'tech_lab' || b.type === 'research_center'
    );
    
    return researchBuilding ? 50 : 0;
  }

  private evaluateActivePlans(): void {
    this.activePlans.forEach((plan, planId) => {
      if (plan.status === PlanStatus.ACTIVE) {
        const evaluation = this.planEvaluator.evaluatePlan(plan);
        
        if (evaluation.successProbability < 0.3) {
          plan.status = PlanStatus.PAUSED;
          this.eventEmitter.emit('plan_failed', plan);
        }
        
        if (this.checkPlanCompletion(plan)) {
          plan.status = PlanStatus.COMPLETED;
          plan.completionTime = Date.now();
        }
      }
    });
  }

  private checkPlanCompletion(plan: StrategicPlan): boolean {
    return plan.steps.every(step => step.status === StepStatus.COMPLETED);
  }

  private selectOptimalStrategy(): void {
    const goals = Array.from(this.activeGoals.values());
    const plans = Array.from(this.activePlans.values());
    
    const newStrategy = this.strategySelector.selectStrategy(this.gameState, plans);
    
    if (newStrategy !== this.currentStrategy) {
      const transition = this.strategySelector.transitionStrategy(
        this.currentStrategy,
        newStrategy,
        this.gameState
      );
      
      this.handleStrategyTransition(transition);
      this.currentStrategy = newStrategy;
    }
  }

  private handleStrategyTransition(transition: StrategyTransition): void {
    this.eventEmitter.emit('strategy_transition', transition);
    
    const decision: StrategicDecision = {
      id: `decision_${Date.now()}`,
      timestamp: Date.now(),
      decisionType: 'strategy_transition',
      context: this.createDecisionContext(),
      chosenOption: transition.to,
      alternativeOptions: [transition.from],
      reasoning: `Transitioning from ${transition.from} to ${transition.to}`,
      expectedOutcome: 'Improved strategic positioning',
      success: true,
      gameState: this.gameState
    };
    
    this.strategicMemory.recordDecision(decision);
  }

  private createDecisionContext(): DecisionContext {
    const resourceState = this.resourceAnalysis.analyzeCurrentResources();
    const threatLevel = this.enemyAnalysis.getEnemyThreatAssessment();
    
    return {
      gamePhase: this.determineGamePhase(),
      resourceState,
      threatLevel,
      mapControl: this.calculateMapControl(),
      armySize: this.gameState.getPlayerUnits().filter(u => u.isCombatUnit).length,
      enemyStrength: this.enemyAnalysis.getEnemyThreatAssessment()
    };
  }

  private determineGamePhase(): GamePhase {
    const gameTime = this.gameState.getGameTime();
    
    if (gameTime < 180) return GamePhase.EARLY_GAME;
    if (gameTime < 600) return GamePhase.MID_GAME;
    if (gameTime < 1200) return GamePhase.LATE_GAME;
    return GamePhase.END_GAME;
  }

  private calculateMapControl(): number {
    const zones = this.mapAnalysis.strategicZones;
    const playerZones = zones.filter(z => z.controlledBy === 'player');
    
    return zones.length > 0 ? playerZones.length / zones.length : 0;
  }

  private adaptToGameState(): void {
    const threatLevel = this.enemyAnalysis.getEnemyThreatAssessment();
    
    if (threatLevel === ThreatLevel.CRITICAL) {
      this.adjustForCriticalThreat();
    } else if (threatLevel === ThreatLevel.HIGH) {
      this.adjustForHighThreat();
    }
    
    this.optimizeResourceAllocation();
    this.balanceMilitaryEconomy();
  }

  private adjustForCriticalThreat(): void {
    const defenseGoal = this.createEmergencyDefenseGoal();
    this.activeGoals.set(defenseGoal.id, defenseGoal);
    
    const defensePlan = this.planGenerator.generatePlan(defenseGoal);
    this.activePlans.set(defensePlan.id, defensePlan);
  }

  private createEmergencyDefenseGoal(): StrategicGoal {
    return {
      id: `emergency_defense_${Date.now()}`,
      type: GoalType.DEFEND,
      priority: PriorityLevel.CRITICAL,
      status: GoalStatus.ACTIVE,
      requirements: [],
      progress: 0,
      createdAt: Date.now(),
      dependencies: [],
      constraints: [],
      successCriteria: [{
        description: 'Neutralize immediate threat',
        checkFunction: (state) => this.enemyAnalysis.getEnemyThreatAssessment() !== ThreatLevel.CRITICAL,
        weight: 1.0
      }]
    };
  }

  private adjustForHighThreat(): void {
    const currentDefenseGoals = Array.from(this.activeGoals.values())
      .filter(g => g.type === GoalType.DEFEND);
    
    if (currentDefenseGoals.length === 0) {
      const defenseGoal = this.createDefenseGoal();
      this.activeGoals.set(defenseGoal.id, defenseGoal);
      
      const defensePlan = this.planGenerator.generatePlan(defenseGoal);
      this.activePlans.set(defensePlan.id, defensePlan);
    }
  }

  private createDefenseGoal(): StrategicGoal {
    return {
      id: `defense_${Date.now()}`,
      type: GoalType.DEFEND,
      priority: PriorityLevel.HIGH,
      status: GoalStatus.ACTIVE,
      requirements: [],
      progress: 0,
      createdAt: Date.now(),
      dependencies: [],
      constraints: [],
      successCriteria: [{
        description: 'Establish defensive position',
        checkFunction: (state) => {
          const buildings = state.getBuildings();
          return buildings.filter(b => b.type === 'turret').length >= 3;
        },
        weight: 1.0
      }]
    };
  }

  private optimizeResourceAllocation(): void {
    const resourceState = this.resourceAnalysis.analyzeCurrentResources();
    
    if (resourceState.minerals < 200) {
      const gatherGoal = this.createGatherGoal();
      this.activeGoals.set(gatherGoal.id, gatherGoal);
      
      const gatherPlan = this.planGenerator.generatePlan(gatherGoal);
      this.activePlans.set(gatherPlan.id, gatherPlan);
    }
  }

  private createGatherGoal(): StrategicGoal {
    return {
      id: `gather_${Date.now()}`,
      type: GoalType.GATHER,
      priority: PriorityLevel.HIGH,
      status: GoalStatus.ACTIVE,
      requirements: [],
      progress: 0,
      createdAt: Date.now(),
      dependencies: [],
      constraints: [],
      successCriteria: [{
        description: 'Accumulate sufficient resources',
        checkFunction: (state) => state.getMinerals() >= 500,
        weight: 1.0
      }]
    };
  }

  private balanceMilitaryEconomy(): void {
    const resourceState = this.resourceAnalysis.analyzeCurrentResources();
    const threatLevel = this.enemyAnalysis.getEnemyThreatAssessment();
    
    if (resourceState.minerals > 1000 && threatLevel < ThreatLevel.HIGH) {
      const economyGoal = this.createEconomyGoal();
      const expansionGoal = this.createExpansionGoal();
      
      this.activeGoals.set(economyGoal.id, economyGoal);
      this.activeGoals.set(expansionGoal.id, expansionGoal);
      
      const economyPlan = this.planGenerator.generatePlan(economyGoal);
      const expansionPlan = this.planGenerator.generatePlan(expansionGoal);
      
      this.activePlans.set(economyPlan.id, economyPlan);
      this.activePlans.set(expansionPlan.id, expansionPlan);
    }
  }

  private createEconomyGoal(): StrategicGoal {
    return {
      id: `economy_${Date.now()}`,
      type: GoalType.ECONOMY,
      priority: PriorityLevel.MEDIUM,
      status: GoalStatus.ACTIVE,
      requirements: [],
      progress: 0,
      createdAt: Date.now(),
      dependencies: [],
      constraints: [],
      successCriteria: [{
        description: 'Increase resource income',
        checkFunction: (state) => {
          const income = this.resourceAnalysis.calculateIncomeRate();
          return income.mineralsPerMinute >= 400;
        },
        weight: 1.0
      }]
    };
  }

  private createExpansionGoal(): StrategicGoal {
    const expansionSites = this.expansionPlanner.identifyExpansionSites(this.gameState);
    
    if (expansionSites.length === 0) {
      return {
        id: `expand_${Date.now()}`,
        type: GoalType.EXPAND,
        priority: PriorityLevel.MEDIUM,
        status: GoalStatus.ACTIVE,
        requirements: [],
        progress: 0,
        createdAt: Date.now(),
        dependencies: [],
        constraints: [],
        successCriteria: []
      };
    }
    
    const bestSite = expansionSites[0];
    
    return {
      id: `expand_${Date.now()}`,
      type: GoalType.EXPAND,
      priority: PriorityLevel.MEDIUM,
      status: GoalStatus.ACTIVE,
      targetPosition: bestSite.position,
      requirements: [],
      progress: 0,
      createdAt: Date.now(),
      dependencies: [],
      constraints: [],
      successCriteria: [{
        description: 'Establish expansion base',
        checkFunction: (state) => {
          const buildings = state.getBuildings();
          return buildings.some(b => 
            b.type === 'command_center' && 
            Vector2.distance(b.position, bestSite.position) < 100
          );
        },
        weight: 1.0
      }]
    };
  }

  public generateStrategicGoals(): StrategicGoal[] {
    const goals = this.goalGenerator.generateGoals();
    
    goals.forEach(goal => {
      this.activeGoals.set(goal.id, goal);
    });
    
    return goals;
  }

  public generatePlanForGoal(goalId: string): StrategicPlan | null {
    const goal = this.activeGoals.get(goalId);
    if (!goal) return null;
    
    const plan = this.planGenerator.generatePlan(goal);
    this.activePlans.set(plan.id, plan);
    
    return plan;
  }

  public evaluatePlan(planId: string): PlanEvaluation | null {
    const plan = this.activePlans.get(planId);
    if (!plan) return null;
    
    return this.planEvaluator.evaluatePlan(plan);
  }

  public comparePlans(planIds: string[]): PlanComparison[] {
    const plans = planIds
      .map(id => this.activePlans.get(id))
      .filter(plan => plan !== undefined) as StrategicPlan[];
    
    return this.planEvaluator.comparePlans(plans);
  }

  public selectBestPlan(goalId: string): StrategicPlan | null {
    const goal = this.activeGoals.get(goalId);
    if (!goal) return null;
    
    const alternatives = this.planGenerator.generateAlternativePlans(goal);
    
    alternatives.forEach(plan => {
      this.activePlans.set(plan.id, plan);
    });
    
    const rankedPlans = this.planEvaluator.rankPlans(alternatives);
    
    return rankedPlans.length > 0 ? rankedPlans[0] : null;
  }

  private handleGoalCompletion(goal: StrategicGoal): void {
    this.activeGoals.delete(goal.id);
    
    const relatedPlans = Array.from(this.activePlans.values())
      .filter(plan => plan.goalId === goal.id);
    
    relatedPlans.forEach(plan => {
      plan.status = PlanStatus.COMPLETED;
    });
    
    this.generateStrategicGoals();
  }

  private handlePlanFailure(plan: StrategicPlan): void {
    if (plan.riskAssessment.fallbackPlan) {
      this.activePlans.set(plan.riskAssessment.fallbackPlan.id, plan.riskAssessment.fallbackPlan);
    }
    
    this.generateStrategicGoals();
  }

  public executeStep(planId: string, stepId: string): boolean {
    const plan = this.activePlans.get(planId);
    if (!plan) return false;
    
    const step = plan.steps.find(s => s.id === stepId);
    if (!step) return false;
    
    if (step.status !== StepStatus.PENDING) return false;
    
    const preconditionsMet = this.checkPreconditions(step);
    if (!preconditionsMet) return false;
    
    step.status = StepStatus.IN_PROGRESS;
    step.startTime = Date.now();
    
    this.executeStepActions(step);
    
    return true;
  }

  private checkPreconditions(step: PlanStep): boolean {
    for (const prereq of step.prerequisites) {
      const prereqStep = step.dependencies.includes(prereq);
      if (!prereqStep) return false;
    }
    
    return true;
  }

  private executeStepActions(step: PlanStep): void {
    step.actions.forEach(action => {
      this.executeAction(action);
    });
    
    step.status = StepStatus.COMPLETED;
    step.completionTime = Date.now();
    step.actualDuration = step.completionTime - (step.startTime || step.completionTime);
  }

  private executeAction(action: PlanAction): void {
    switch (action.type) {
      case ActionType.BUILD_UNIT:
        this.executeBuildUnit(action);
        break;
      case ActionType.BUILD_BUILDING:
        this.executeBuildBuilding(action);
        break;
      case ActionType.MOVE_UNIT:
        this.executeMoveUnit(action);
        break;
      case ActionType.ATTACK_UNIT:
        this.executeAttackUnit(action);
        break;
      case ActionType.GATHER_RESOURCE:
        this.executeGatherResource(action);
        break;
      case ActionType.RESEARCH_TECH:
        this.executeResearchTech(action);
        break;
      case ActionType.SCOUT_AREA:
        this.executeScoutArea(action);
        break;
      case ActionType.DEFEND_POSITION:
        this.executeDefendPosition(action);
        break;
    }
  }

  private executeBuildUnit(action: PlanAction): void {
    const unitType = action.parameters.unitType;
    const quantity = action.parameters.quantity || 1;
    
    for (let i = 0; i < quantity; i++) {
      this.unitManager.trainUnit(unitType);
    }
  }

  private executeBuildBuilding(action: PlanAction): void {
    const buildingType = action.parameters.buildingType;
    const position = action.targetPosition || new Vector2(150, 150);
    
    this.gameState.constructBuilding(buildingType, position);
  }

  private executeMoveUnit(action: PlanAction): void {
    const targetPosition = action.targetPosition;
    if (!targetPosition) return;
    
    const units = this.gameState.getPlayerUnits();
    units.forEach(unit => {
      if (unit.isCombatUnit) {
        this.unitManager.moveUnit(unit.id, targetPosition);
      }
    });
  }

  private executeAttackUnit(action: PlanAction): void {
    const targetPosition = action.targetPosition;
    if (!targetPosition) return;
    
    const units = this.gameState.getPlayerUnits();
    const combatUnits = units.filter(u => u.isCombatUnit);
    
    combatUnits.forEach(unit => {
      this.unitManager.attackPosition(unit.id, targetPosition);
    });
  }

  private executeGatherResource(action: PlanAction): void {
    const units = this.gameState.getUnits();
    const workers = units.filter(u => u.type === 'worker');
    
    const resources = this.gameState.getResources();
    if (resources.length > 0) {
      workers.forEach(worker => {
        this.unitManager.assignGathering(worker.id, resources[0].id);
      });
    }
  }

  private executeResearchTech(action: PlanAction): void {
    const techId = action.parameters.techId;
    this.gameState.startResearch(techId);
  }

  private executeScoutArea(action: PlanAction): void {
    const units = this.gameState.getUnits();
    const scouts = units.filter(u => u.type === 'scout');
    
    const targetPosition = action.targetPosition;
    if (targetPosition) {
      scouts.forEach(scout => {
        this.unitManager.moveUnit(scout.id, targetPosition);
      });
    }
  }

  private executeDefendPosition(action: PlanAction): void {
    const units = this.gameState.getPlayerUnits();
    const combatUnits = units.filter(u => u.isCombatUnit);
    
    const targetPosition = action.targetPosition || this.gameState.getStartingPositions()[0];
    
    combatUnits.forEach(unit => {
      this.unitManager.defendPosition(unit.id, targetPosition);
    });
  }

  public pause(): void {
    this.isPaused = true;
  }

  public resume(): void {
    this.isPaused = false;
  }

  public reset(): void {
    this.activeGoals.clear();
    this.activePlans.clear();
    this.currentStrategy = StrategyType.BALANCED;
    this.lastUpdateTime = 0;
  }

  public getResourceAnalysis(): ResourceAnalysis {
    return this.resourceAnalysis;
  }

  public getMapAnalysis(): MapAnalysis {
    return this.mapAnalysis;
  }

  public getEnemyProfiles(): EnemyProfile[] {
    return this.enemyAnalysis.analyzeAllEnemies();
  }

  public getActiveGoals(): StrategicGoal[] {
    return Array.from(this.activeGoals.values());
  }

  public getActivePlans(): StrategicPlan[] {
    return Array.from(this.activePlans.values());
  }

  public getCurrentStrategy(): StrategyType {
    return this.currentStrategy;
  }

  public getStrategicInsights(): StrategicInsight[] {
    return this.strategicMemory.getStrategicInsights();
  }

  public getExpansionRecommendations(): ExpansionSite[] {
    return this.expansionPlanner.prioritizeExpansions(
      this.expansionPlanner.identifyExpansionSites(this.gameState),
      this.gameState
    );
  }

  public getTechRecommendations(): TechOpportunity[] {
    return this.techPlanner.prioritizeTechs(
      this.techPlanner.identifyTechOpportunities(this.gameState),
      this.gameState
    );
  }

  public getMilitaryRecommendations(): UnitComposition {
    const enemyProfiles = this.enemyAnalysis.analyzeAllEnemies();
    if (enemyProfiles.length > 0) {
      return this.militaryPlanner.recommendUnitComposition(
        enemyProfiles[0].estimatedArmyComposition,
        this.gameState
      );
    }
    return {
      units: [],
      totalValue: 0,
      strengths: [],
      weaknesses: [],
      recommendedCount: [],
      counters: []
    };
  }

  public getEconomicRecommendations(): EconomicRecommendation[] {
    const analysis = this.economyPlanner.analyzeEconomy(this.gameState);
    return analysis.recommendations;
  }

  public analyzeBattleOutcome(targetPosition: Vector2): BattleAssessment {
    const attackPlan: StrategicPlan = {
      id: 'temp_attack_plan',
      name: 'Temporary Attack Assessment',
      type: PlanType.ATTACK,
      goalId: 'battle_assessment',
      status: PlanStatus.PENDING,
      steps: [{
        id: 'assess_step',
        name: 'Assess battle',
        description: 'Evaluate battle outcome',
        status: StepStatus.PENDING,
        actions: [{
          id: 'assess_action',
          type: ActionType.ATTACK_UNIT,
          targetPosition,
          parameters: {},
          cost: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] },
          duration: 0,
          conditions: [],
          effects: []
        }],
        estimatedDuration: 0,
        dependencies: [],
        prerequisites: [],
        effects: [],
        retryCount: 0,
        maxRetries: 0,
        skipConditions: []
      }],
      estimatedDuration: 0,
      riskAssessment: {
        overallRisk: ThreatLevel.MEDIUM,
        riskFactors: [],
        totalRiskScore: 0.5,
        mitigationStrategies: [],
        contingencyPlans: []
      },
      cost: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] },
      preconditions: [],
      postconditions: [],
      parallelSteps: [],
      criticalPath: []
    };
    
    return this.militaryPlanner.assessBattleOutcome(attackPlan, this.gameState);
  }

  public getStrategicReport(): StrategicReport {
    return {
      currentStrategy: this.currentStrategy,
      activeGoals: this.getActiveGoals(),
      activePlans: this.getActivePlans(),
      resourceAnalysis: this.resourceAnalysis.analyzeCurrentResources(),
      incomeRate: this.resourceAnalysis.calculateIncomeRate(),
      enemyThreat: this.enemyAnalysis.getEnemyThreatAssessment(),
      mapControl: this.calculateMapControl(),
      expansionSites: this.getExpansionRecommendations(),
      techRecommendations: this.getTechRecommendations(),
      militaryRecommendations: this.getMilitaryRecommendations(),
      economicRecommendations: this.getEconomicRecommendations(),
      strategicInsights: this.getStrategicInsights(),
      gamePhase: this.determineGamePhase(),
      timestamp: Date.now()
    };
  }
}

export interface StrategicReport {
  currentStrategy: StrategyType;
  activeGoals: StrategicGoal[];
  activePlans: StrategicPlan[];
  resourceAnalysis: ResourceState;
  incomeRate: IncomeRate;
  enemyThreat: ThreatLevel;
  mapControl: number;
  expansionSites: ExpansionSite[];
  techRecommendations: TechOpportunity[];
  militaryRecommendations: UnitComposition;
  economicRecommendations: EconomicRecommendation[];
  strategicInsights: StrategicInsight[];
  gamePhase: GamePhase;
  timestamp: number;
}

export class StrategicMemoryImpl implements StrategicMemory {
  private decisions: StrategicDecision[] = [];
  private insights: StrategicInsight[] = [];

  public recordDecision(decision: StrategicDecision): void {
    this.decisions.push(decision);
    this.analyzeDecisionOutcomes();
  }

  public getHistoricalDecisions(filter?: DecisionFilter): StrategicDecision[] {
    let filtered = this.decisions;

    if (filter) {
      if (filter.startTime) {
        filtered = filtered.filter(d => d.timestamp >= filter.startTime!);
      }
      if (filter.endTime) {
        filtered = filtered.filter(d => d.timestamp <= filter.endTime!);
      }
      if (filter.decisionType) {
        filtered = filtered.filter(d => d.decisionType === filter.decisionType);
      }
      if (filter.successOnly) {
        filtered = filtered.filter(d => d.success);
      }
    }

    return filtered;
  }

  public analyzeDecisionPatterns(): DecisionPattern[] {
    const patterns: Map<string, DecisionPattern> = new Map();

    this.decisions.forEach(decision => {
      const contextKey = this.getContextKey(decision.context);
      const existing = patterns.get(contextKey);

      if (existing) {
        existing.frequency++;
        if (decision.success) {
          existing.averageOutcome = (existing.averageOutcome * (existing.frequency - 1) + 1) / existing.frequency;
        }
      } else {
        patterns.set(contextKey, {
          patternType: contextKey,
          frequency: 1,
          averageOutcome: decision.success ? 1 : 0,
          conditions: [decision.decisionType],
          recommendations: []
        });
      }
    });

    return Array.from(patterns.values());
  }

  private getContextKey(context: DecisionContext): string {
    return `${context.gamePhase}_${context.threatLevel}_${context.armySize}`;
  }

  public learnFromOutcome(outcome: PlanOutcome): void {
    const decision = this.decisions.find(d => d.id === outcome.planId);
    if (decision) {
      decision.actualOutcome = outcome.success ? 'success' : 'failure';
      decision.success = outcome.success;

      outcome.lessons.forEach(lesson => {
        this.addInsight({
          type: 'lesson_learned',
          description: lesson,
          confidence: 0.7,
          evidence: [outcome.planId],
          recommendations: [],
          timestamp: Date.now()
        });
      });
    }
  }

  public getStrategicInsights(): StrategicInsight[] {
    return this.insights;
  }

  public adjustParameters(insights: StrategicInsight[]): void {
    insights.forEach(insight => {
      this.addInsight(insight);
    });
  }

  private addInsight(insight: StrategicInsight): void {
    const existing = this.insights.find(i => 
      i.type === insight.type && i.description === insight.description
    );

    if (existing) {
      existing.confidence = Math.min(1, (existing.confidence + insight.confidence) / 2);
      existing.evidence.push(...insight.evidence);
    } else {
      this.insights.push(insight);
    }
  }

  private analyzeDecisionOutcomes(): void {
    const recentDecisions = this.decisions.slice(-10);
    const successRate = recentDecisions.filter(d => d.success).length / recentDecisions.length;

    if (successRate < 0.5) {
      this.addInsight({
        type: 'performance_warning',
        description: `Recent decision success rate is ${successRate * 100}%`,
        confidence: 0.8,
        evidence: recentDecisions.map(d => d.id),
        recommendations: ['Review recent decisions', 'Consider more conservative approach'],
        timestamp: Date.now()
      });
    }
  }
}

export class ResourceAnalysisImpl implements ResourceAnalysis {
  private gameState: GameState;

  constructor(gameState: GameState) {
    this.gameState = gameState;
  }

  public analyzeCurrentResources(): ResourceState {
    return {
      minerals: this.gameState.getMinerals(),
      gas: this.gameState.getGas(),
      energy: this.gameState.getEnergy(),
      food: this.gameState.getFood(),
      wood: this.gameState.getWood(),
      gold: this.gameState.getGold(),
      storageCapacity: this.getStorageCapacity(),
      reserves: this.getReserves()
    };
  }

  private getStorageCapacity(): ResourceCapacity {
    const buildings = this.gameState.getBuildings();
    let mineralCapacity = 0;
    let gasCapacity = 0;
    let energyCapacity = 0;
    let foodCapacity = 0;

    buildings.forEach(building => {
      if (building.type === 'depot') {
        mineralCapacity += building.storageCapacity || 1000;
      }
      if (building.type === 'refinery') {
        gasCapacity += building.storageCapacity || 500;
      }
      if (building.type === 'powerplant') {
        energyCapacity += building.storageCapacity || 500;
      }
      if (building.type === 'farm') {
        foodCapacity += building.storageCapacity || 500;
      }
    });

    return {
      minerals: { current: this.gameState.getMinerals(), max: mineralCapacity || 10000 },
      gas: { current: this.gameState.getGas(), max: gasCapacity || 5000 },
      energy: { current: this.gameState.getEnergy(), max: energyCapacity || 5000 },
      food: { current: this.gameState.getFood(), max: foodCapacity || 5000 }
    };
  }

  private getReserves(): ResourceReserves {
    return {
      minerals: Math.floor(this.gameState.getMinerals() * 0.2),
      gas: Math.floor(this.gameState.getGas() * 0.2),
      energy: Math.floor(this.gameState.getEnergy() * 0.2),
      food: Math.floor(this.gameState.getFood() * 0.2)
    };
  }

  public calculateIncomeRate(): IncomeRate {
    const sources: IncomeSource[] = [];
    const units = this.gameState.getUnits();
    
    let mineralIncome = 0;
    let gasIncome = 0;
    let energyIncome = 0;
    let foodIncome = 0;

    units.forEach(unit => {
      if (unit.isGathering && unit.gatheringTarget) {
        if (unit.gatheringTarget.type === 'mineral') {
          mineralIncome += unit.gatheringRate || 10;
          sources.push({
            type: ResourceType.MINERALS,
            source: unit.id,
            amount: unit.gatheringRate || 10,
            efficiency: 0.9
          });
        } else if (unit.gatheringTarget.type === 'gas') {
          gasIncome += unit.gatheringRate || 8;
          sources.push({
            type: ResourceType.GAS,
            source: unit.id,
            amount: unit.gatheringRate || 8,
            efficiency: 0.85
          });
        } else if (unit.gatheringTarget.type === 'energy') {
          energyIncome += unit.gatheringRate || 5;
          sources.push({
            type: ResourceType.ENERGY,
            source: unit.id,
            amount: unit.gatheringRate || 5,
            efficiency: 0.95
          });
        } else if (unit.gatheringTarget.type === 'food') {
          foodIncome += unit.gatheringRate || 3;
          sources.push({
            type: ResourceType.FOOD,
            source: unit.id,
            amount: unit.gatheringRate || 3,
            efficiency: 0.9
          });
        }
      }
    });

    return {
      mineralsPerMinute: mineralIncome * 60,
      gasPerMinute: gasIncome * 60,
      energyPerMinute: energyIncome * 60,
      foodPerMinute: foodIncome * 60,
      sources
    };
  }

  public calculateExpenditureRate(): ExpenditureRate {
    const categories: ExpenditureCategory[] = [];
    const units = this.gameState.getUnits();
    const buildings = this.gameState.getBuildings();

    let mineralExpense = 0;
    let gasExpense = 0;
    let energyExpense = 0;
    let foodExpense = 0;

    units.forEach(unit => {
      if (unit.isTraining) {
        const trainingCost = unit.trainingCost || { minerals: 50, gas: 25 };
        mineralExpense += trainingCost.minerals;
        gasExpense += trainingCost.gas || 0;
      }
    });

    buildings.forEach(building => {
      if (building.isConstructing) {
        const constructionCost = building.constructionCost || { minerals: 100 };
        mineralExpense += constructionCost.minerals;
        gasExpense += constructionCost.gas || 0;
      }
      if (building.isResearching) {
        const researchCost = building.researchCost || { minerals: 100, gas: 50 };
        mineralExpense += researchCost.minerals;
        gasExpense += researchCost.gas || 0;
        energyExpense += researchCost.energy || 0;
      }
    });

    return {
      mineralsPerMinute: mineralExpense,
      gasPerMinute: gasExpense,
      energyPerMinute: energyExpense,
      foodPerMinute: foodExpense,
      categories
    };
  }

  public projectResources(futureTime: number): ProjectedResource[] {
    const projections: ProjectedResource[] = [];
    const currentState = this.analyzeCurrentResources();
    const income = this.calculateIncomeRate();
    const expenditure = this.calculateExpenditureRate();

    const intervals = Math.min(10, Math.floor(futureTime / 60));
    for (let i = 1; i <= intervals; i++) {
      const time = i * 60;
      const deficit: ResourceDeficit[] = [];

      const projectedMinerals = currentState.minerals + 
        (income.mineralsPerMinute - expenditure.mineralsPerMinute) * (time / 60);

      if (projectedMinerals < 0) {
        deficit.push({
          resourceType: ResourceType.MINERALS,
          deficitAmount: -projectedMinerals,
          timeUntilDeficit: time,
          severity: ThreatLevel.HIGH
        });
      }

      projections.push({
        time,
        minerals: Math.max(0, projectedMinerals),
        gas: Math.max(0, currentState.gas + (income.gasPerMinute - expenditure.gasPerMinute) * (time / 60)),
        energy: Math.max(0, currentState.energy + (income.energyPerMinute - expenditure.energyPerMinute) * (time / 60)),
        food: Math.max(0, currentState.food + (income.foodPerMinute - expenditure.foodPerMinute) * (time / 60)),
        potentialDeficit: deficit
      });
    }

    return projections;
  }

  public analyzeGatheringEfficiency(): GatheringEfficiency {
    const units = this.gameState.getUnits();
    const workerEfficiency: WorkerEfficiency[] = [];

    let totalEfficiency = 0;
    let workerCount = 0;

    units.forEach(unit => {
      if (unit.isGathering && unit.gatheringTarget) {
        const efficiency: WorkerEfficiency = {
          workerId: unit.id,
          resourceType: this.getResourceTypeFromTarget(unit.gatheringTarget),
          resourcesPerMinute: (unit.gatheringRate || 10) * 60,
          utilizationRate: unit.isIdle ? 0 : 1,
          optimalAssignment: this.isOptimalAssignment(unit)
        };
        workerEfficiency.push(efficiency);
        totalEfficiency += efficiency.resourcesPerMinute * efficiency.utilizationRate;
        workerCount++;
      }
    });

    return {
      overallEfficiency: workerCount > 0 ? totalEfficiency / workerCount : 0,
      workerEfficiency,
      resourceNodeUtilization: [],
      travelTimeOptimization: {
        averageTravelTime: 5,
        optimalWorkerCount: 12,
        suggestedAssignments: [],
        routeEfficiency: 0.85
      }
    };
  }

  private getResourceTypeFromTarget(target: any): ResourceType {
    if (!target || !target.type) return ResourceType.MINERALS;
    switch (target.type) {
      case 'mineral': return ResourceType.MINERALS;
      case 'gas': return ResourceType.GAS;
      case 'energy': return ResourceType.ENERGY;
      case 'food': return ResourceType.FOOD;
      default: return ResourceType.MINERALS;
    }
  }

  private isOptimalAssignment(unit: Unit): boolean {
    if (!unit.gatheringTarget) return false;
    return (unit.type === 'worker');
  }

  public identifyBottlenecks(): ResourceBottleneck[] {
    const bottlenecks: ResourceBottleneck[] = [];
    const income = this.calculateIncomeRate();
    const expenditure = this.calculateExpenditureRate();

    const netMinerals = income.mineralsPerMinute - expenditure.mineralsPerMinute;
    if (netMinerals < 0) {
      bottlenecks.push({
        resourceType: ResourceType.MINERALS,
        severity: ThreatLevel.HIGH,
        cause: 'Expenditure exceeds income',
        impact: 'Inability to fund operations',
        solution: 'Increase worker count or reduce spending',
        estimatedFixTime: 60
      });
    }

    return bottlenecks;
  }

  public getOptimizationSuggestions(): OptimizationSuggestion[] {
    const suggestions: OptimizationSuggestion[] = [];
    const efficiency = this.analyzeGatheringEfficiency();

    if (efficiency.overallEfficiency < 50) {
      suggestions.push({
        type: 'worker_reassignment',
        description: 'Optimize worker assignments for better resource gathering',
        expectedGain: 20,
        implementationCost: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 5, units: [] },
        priority: PriorityLevel.HIGH,
        riskLevel: ThreatLevel.LOW
      });
    }

    return suggestions;
  }

  public get currentResources(): ResourceState {
    return this.analyzeCurrentResources();
  }

  public get incomeRate(): IncomeRate {
    return this.calculateIncomeRate();
  }

  public get expenditureRate(): ExpenditureRate {
    return this.calculateExpenditureRate();
  }

  public get projectedResources(): ProjectedResource[] {
    return this.projectResources(300);
  }

  public get gatheringEfficiency(): GatheringEfficiency {
    return this.analyzeGatheringEfficiency();
  }

  public get resourceDistribution(): ResourceDistribution {
    return {
      minerals: { averageDistance: 100, variance: 20, nearestSource: new Vector2(50, 50), farthestSource: new Vector2(200, 200), density: 0.8 },
      gas: { averageDistance: 150, variance: 30, nearestSource: new Vector2(80, 80), farthestSource: new Vector2(250, 250), density: 0.6 },
      energy: { averageDistance: 120, variance: 25, nearestSource: new Vector2(60, 60), farthestSource: new Vector2(220, 220), density: 0.7 },
      food: { averageDistance: 90, variance: 15, nearestSource: new Vector2(40, 40), farthestSource: new Vector2(180, 180), density: 0.9 },
      balance: 0.75
    };
  }

  public get bottlenecks(): ResourceBottleneck[] {
    return this.identifyBottlenecks();
  }

  public get optimizationSuggestions(): OptimizationSuggestion[] {
    return this.getOptimizationSuggestions();
  }
}

export class MapAnalysisImpl implements MapAnalysis {
  private gameState: GameState;
  private pathFinder: PathFinder;
  private terrainMap: TerrainMap | null = null;

  constructor(gameState: GameState, pathFinder: PathFinder) {
    this.gameState = gameState;
    this.pathFinder = pathFinder;
  }

  public analyzeTerrain(): TerrainMap {
    if (this.terrainMap) {
      return this.terrainMap;
    }

    const mapSize = this.gameState.getMapSize();
    const terrainTypes: TerrainType[][] = [];
    const elevation: number[][] = [];
    const walkability: boolean[][] = [];
    const buildability: boolean[][] = [];

    for (let x = 0; x < mapSize.x; x++) {
      terrainTypes[x] = [];
      elevation[x] = [];
      walkability[x] = [];
      buildability[x] = [];

      for (let y = 0; y < mapSize.y; y++) {
        const terrain = this.gameState.getTerrainAt(new Vector2(x, y));
        terrainTypes[x][y] = this.convertTerrainType(terrain);
        elevation[x][y] = terrain.elevation || 0;
        walkability[x][y] = terrain.walkable !== false;
        buildability[x][y] = terrain.buildable !== false;
      }
    }

    this.terrainMap = {
      width: mapSize.x,
      height: mapSize.y,
      terrainTypes,
      elevation,
      walkability,
      buildability
    };

    return this.terrainMap;
  }

  private convertTerrainType(terrain: any): TerrainType {
    if (!terrain.walkable) return TerrainType.IMPASSABLE;
    if (terrain.type === 'water') return TerrainType.WATER;
    if (terrain.type === 'elevated') return TerrainType.ELEVATED;
    if (terrain.resource) return TerrainType.RESOURCE_RICH;
    return TerrainType.LAND;
  }

  public identifyStrategicZones(): StrategicZone[] {
    const zones: StrategicZone[] = [];
    const startingPositions = this.gameState.getStartingPositions();
    const resources = this.gameState.getResources();

    startingPositions.forEach((pos, index) => {
      zones.push({
        id: `zone_starting_${index}`,
        type: ZoneType.STARTING,
        center: pos,
        radius: 200,
        boundary: this.calculateZoneBoundary(pos, 200),
        importance: 10,
        controlLevel: index === 0 ? 1 : 0,
        controlledBy: index === 0 ? 'player' : 'neutral',
        resources: resources.filter(r => Vector2.distance(r.position, pos) < 200),
        strategicValue: 10,
        defensibility: 8,
        visibility: 10,
        accessPoints: this.identifyAccessPoints(pos, 200),
        chokepoints: [],
        adjacentZones: []
      });
    });

    return zones;
  }

  private calculateZoneBoundary(center: Vector2, radius: number): Vector2[] {
    const boundary: Vector2[] = [];
    const segments = 8;
    for (let i = 0; i < segments; i++) {
      const angle = (i / segments) * Math.PI * 2;
      boundary.push(new Vector2(
        center.x + Math.cos(angle) * radius,
        center.y + Math.sin(angle) * radius
      ));
    }
    return boundary;
  }

  private identifyAccessPoints(center: Vector2, radius: number): Vector2[] {
    const accessPoints: Vector2[] = [];
    const terrain = this.analyzeTerrain();
    const segments = 12;

    for (let i = 0; i < segments; i++) {
      const angle = (i / segments) * Math.PI * 2;
      const checkDistance = radius * 0.8;

      for (let d = 0; d < checkDistance; d += 10) {
        const x = Math.floor(center.x + Math.cos(angle) * d);
        const y = Math.floor(center.y + Math.sin(angle) * d);

        if (x >= 0 && x < terrain.width && y >= 0 && y < terrain.height) {
          if (terrain.walkability[x][y]) {
            accessPoints.push(new Vector2(x, y));
            break;
          }
        }
      }
    }

    return accessPoints;
  }

  public identifyChokepoints(): Chokepoint[] {
    const chokepoints: Chokepoint[] = [];
    return chokepoints;
  }

  public identifyExpansionSites(): ExpansionSite[] {
    const sites: ExpansionSite[] = [];
    const resources = this.gameState.getResources();
    const startingPositions = this.gameState.getStartingPositions();
    const basePosition = startingPositions[0] || new Vector2(100, 100);

    resources.forEach(resource => {
      const distanceFromBase = Vector2.distance(resource.position, basePosition);
      const distanceFromEnemy = this.estimateEnemyDistance(resource.position);

      if (distanceFromBase > 200 && distanceFromBase < 800 && distanceFromEnemy > 300) {
        sites.push({
          id: `expansion_${resource.id}`,
          position: resource.position,
          resources: [resource],
          resourceValue: resource.amount || 1000,
          defensibility: 7,
          accessibility: 8,
          proximityToBase: distanceFromBase,
          proximityToEnemy: distanceFromEnemy,
          buildingSlots: 10,
          recommendedBuildings: ['depot', 'turret'],
          constructionTime: 60,
          riskLevel: ThreatLevel.LOW,
          strategicValue: 8
        });
      }
    });

    return sites.sort((a, b) => b.strategicValue - a.strategicValue);
  }

  private estimateEnemyDistance(position: Vector2): number {
    const enemyBase = this.gameState.getEnemyBasePosition();
    if (enemyBase) {
      return Vector2.distance(position, enemyBase);
    }
    return 500;
  }

  public identifyDefensivePositions(): DefensivePosition[] {
    const positions: DefensivePosition[] = [];
    return positions;
  }

  public identifyAttackRoutes(): AttackRoute[] {
    const routes: AttackRoute[] = [];
    return routes;
  }

  public analyzeVisibility(): VisibilityMap {
    const mapSize = this.gameState.getMapSize();
    const playerVision: boolean[][] = [];
    const enemyVision: boolean[][] = [];

    for (let x = 0; x < mapSize.x; x++) {
      playerVision[x] = [];
      enemyVision[x] = [];

      for (let y = 0; y < mapSize.y; y++) {
        playerVision[x][y] = false;
        enemyVision[x][y] = false;
      }
    }

    return {
      playerVision,
      enemyVision,
      contestedAreas: [],
      unexploredAreas: [],
      recentlyExplored: [],
      enemyBaseLocation: this.gameState.getEnemyBasePosition(),
      confidenceLevel: 0.5
    };
  }

  public analyzePathfinding(): PathfindingAnalysis {
    return {
      averagePathLength: 0,
      blockedPaths: [],
      optimalRoutes: [],
      congestionPoints: [],
      recommendedPaths: []
    };
  }

  public get terrain(): TerrainMap {
    return this.analyzeTerrain();
  }

  public get strategicZones(): StrategicZone[] {
    return this.identifyStrategicZones();
  }

  public get chokepoints(): Chokepoint[] {
    return this.identifyChokepoints();
  }

  public get expansionSites(): ExpansionSite[] {
    return this.identifyExpansionSites();
  }

  public get defensivePositions(): DefensivePosition[] {
    return this.identifyDefensivePositions();
  }

  public get attackRoutes(): AttackRoute[] {
    return this.identifyAttackRoutes();
  }

  public get resourceLocations(): ResourceLocation[] {
    return this.gameState.getResources().map(r => ({
      id: r.id,
      position: r.position,
      type: this.getResourceTypeFromString(r.type),
      amount: r.amount,
      quality: 1.0,
      accessibility: 0.8,
      contested: false,
      optimalGatherers: ['worker'],
      gatheringDistance: Vector2.distance(r.position, this.gameState.getStartingPositions()[0] || new Vector2(0, 0))
    }));
  }

  private getResourceTypeFromString(type: string): ResourceType {
    switch (type) {
      case 'mineral': return ResourceType.MINERALS;
      case 'gas': return ResourceType.GAS;
      case 'energy': return ResourceType.ENERGY;
      case 'food': return ResourceType.FOOD;
      default: return ResourceType.MINERALS;
    }
  }

  public get visibilityMap(): VisibilityMap {
    return this.analyzeVisibility();
  }

  public get pathfindingAnalysis(): PathfindingAnalysis {
    return this.analyzePathfinding();
  }
}

export class EnemyAnalysisImpl {
  private gameState: GameState;
  private enemyProfiles: Map<string, EnemyProfile> = new Map();

  constructor(gameState: GameState) {
    this.gameState = gameState;
  }

  public analyzeEnemy(enemyId: string): EnemyProfile {
    const existingProfile = this.enemyProfiles.get(enemyId);
    if (existingProfile) {
      return this.updateProfile(existingProfile);
    }

    return this.createNewProfile(enemyId);
  }

  private createNewProfile(enemyId: string): EnemyProfile {
    const enemyUnits = this.gameState.getEnemyUnits();
    const enemyBuildings = this.gameState.getEnemyBuildings();
    const enemyBase = this.gameState.getEnemyBasePosition();

    const profile: EnemyProfile = {
      id: enemyId,
      lastKnownPosition: enemyBase,
      basePosition: enemyBase,
      unitCount: enemyUnits.length,
      buildingCount: enemyBuildings.length,
      resourceCount: this.estimateEnemyResources(),
      knownUnits: enemyUnits.map(u => u.id),
      knownBuildings: enemyBuildings.map(b => b.id),
      technologyLevel: this.estimateTechnologyLevel(enemyBuildings),
      estimatedArmySize: this.estimateArmySize(enemyUnits),
      estimatedArmyComposition: this.analyzeUnitComposition(enemyUnits),
      threatLevel: this.calculateThreatLevel(enemyUnits),
      aggressionLevel: 0.5,
      expansionRate: 0,
      resourceGatheringRate: this.estimateGatheringRate(),
      behaviorPatterns: [],
      currentIntent: 'unknown',
      predictedIntent: 'unknown',
      confidenceLevel: 0.3,
      lastUpdate: Date.now(),
      behaviorHistory: []
    };

    this.enemyProfiles.set(enemyId, profile);
    return profile;
  }

  private updateProfile(profile: EnemyProfile): EnemyProfile {
    const enemyUnits = this.gameState.getEnemyUnits();
    const enemyBuildings = this.gameState.getEnemyBuildings();

    profile.unitCount = enemyUnits.length;
    profile.buildingCount = enemyBuildings.length;
    profile.knownUnits = enemyUnits.map(u => u.id);
    profile.knownBuildings = enemyBuildings.map(b => b.id);
    profile.estimatedArmySize = this.estimateArmySize(enemyUnits);
    profile.threatLevel = this.calculateThreatLevel(enemyUnits);
    profile.lastUpdate = Date.now();

    return profile;
  }

  private estimateEnemyResources(): number {
    return 1000;
  }

  private estimateTechnologyLevel(buildings: Building[]): number {
    let level = 1;
    buildings.forEach(building => {
      if (building.type === 'tech_lab') level += 1;
      if (building.type === 'research_center') level += 2;
    });
    return level;
  }

  private estimateArmySize(units: Unit[]): number {
    return units.filter(u => u.isCombatUnit).length;
  }

  private analyzeUnitComposition(units: Unit[]): UnitComposition {
    const typeCounts: Map<string, number> = new Map();
    let totalValue = 0;

    units.forEach(unit => {
      const count = typeCounts.get(unit.type) || 0;
      typeCounts.set(unit.type, count + 1);
      totalValue += unit.value || 10;
    });

    return {
      units: Array.from(typeCounts.entries()).map(([type, count]) => ({
        type,
        count,
        value: totalValue / units.length,
        health: 100,
        damage: 10,
        speed: 5
      })),
      totalValue,
      strengths: this.identifyCompositionStrengths(typeCounts),
      weaknesses: this.identifyCompositionWeaknesses(typeCounts),
      recommendedCount: [],
      counters: []
    };
  }

  private identifyCompositionStrengths(composition: Map<string, number>): string[] {
    const strengths: string[] = [];
    if (composition.get('tank') && composition.get('tank')! > 5) {
      strengths.push('Strong armored forces');
    }
    return strengths;
  }

  private identifyCompositionWeaknesses(composition: Map<string, number>): string[] {
    const weaknesses: string[] = [];
    if (!composition.get('anti_air') || composition.get('anti_air')! < 2) {
      weaknesses.push('Vulnerable to air attacks');
    }
    return weaknesses;
  }

  private calculateThreatLevel(units: Unit[]): ThreatLevel {
    const combatUnits = units.filter(u => u.isCombatUnit).length;
    if (combatUnits >= 50) return ThreatLevel.CRITICAL;
    if (combatUnits >= 30) return ThreatLevel.HIGH;
    if (combatUnits >= 15) return ThreatLevel.MEDIUM;
    if (combatUnits >= 5) return ThreatLevel.LOW;
    return ThreatLevel.NONE;
  }

  private estimateGatheringRate(): number {
    return 500;
  }

  public predictEnemyBehavior(enemyId: string): string {
    const profile = this.enemyProfiles.get(enemyId);
    if (!profile) return 'unknown';

    if (profile.estimatedArmySize > 30 && profile.aggressionLevel > 0.7) {
      return 'aggressive_expansion';
    }
    if (profile.technologyLevel > 5) {
      return 'tech_rush';
    }
    return 'defensive_posture';
  }

  public analyzeAllEnemies(): EnemyProfile[] {
    const profiles: EnemyProfile[] = [];
    this.enemyProfiles.forEach((profile, id) => {
      profiles.push(this.analyzeEnemy(id));
    });
    return profiles;
  }

  public getEnemyThreatAssessment(): ThreatLevel {
    let totalThreat = 0;
    this.enemyProfiles.forEach(profile => {
      switch (profile.threatLevel) {
        case ThreatLevel.CRITICAL: totalThreat += 10; break;
        case ThreatLevel.HIGH: totalThreat += 7; break;
        case ThreatLevel.MEDIUM: totalThreat += 5; break;
        case ThreatLevel.LOW: totalThreat += 2; break;
      }
    });

    if (totalThreat >= 20) return ThreatLevel.CRITICAL;
    if (totalThreat >= 15) return ThreatLevel.HIGH;
    if (totalThreat >= 10) return ThreatLevel.MEDIUM;
    if (totalThreat >= 5) return ThreatLevel.LOW;
    return ThreatLevel.NONE;
  }
}

export class GoalGeneratorImpl implements GoalGenerator {
  private gameState: GameState;
  private resourceAnalysis: ResourceAnalysisImpl;
  private mapAnalysis: MapAnalysisImpl;
  private enemyAnalysis: EnemyAnalysisImpl;
  private goalIdCounter: number = 0;

  constructor(
    gameState: GameState,
    resourceAnalysis: ResourceAnalysisImpl,
    mapAnalysis: MapAnalysisImpl,
    enemyAnalysis: EnemyAnalysisImpl
  ) {
    this.gameState = gameState;
    this.resourceAnalysis = resourceAnalysis;
    this.mapAnalysis = mapAnalysis;
    this.enemyAnalysis = enemyAnalysis;
  }

  public generateGoals(): StrategicGoal[] {
    const goals: StrategicGoal[] = [];

    goals.push(...this.generateExpansionGoals());
    goals.push(...this.generateAttackGoals());
    goals.push(...this.generateDefenseGoals());
    goals.push(...this.generateEconomyGoals());

    return goals;
  }

  private generateExpansionGoals(): StrategicGoal[] {
    const goals: StrategicGoal[] = [];
    const expansionSites = this.mapAnalysis.expansionSites;

    if (expansionSites.length > 0) {
      const bestSite = expansionSites[0];
      goals.push({
        id: `goal_expand_${++this.goalIdCounter}`,
        type: GoalType.EXPAND,
        priority: PriorityLevel.MEDIUM,
        status: GoalStatus.ACTIVE,
        targetPosition: bestSite.position,
        requirements: [],
        progress: 0,
        createdAt: Date.now(),
        dependencies: [],
        constraints: [],
        successCriteria: [{
          description: 'Expand base to target location',
          checkFunction: (state) => this.checkExpansionComplete(state, bestSite.position),
          weight: 1.0
        }]
      });
    }

    return goals;
  }

  private checkExpansionComplete(state: GameState, position: Vector2): boolean {
    const buildings = state.getBuildings();
    return buildings.some(b => Vector2.distance(b.position, position) < 100);
  }

  private generateAttackGoals(): StrategicGoal[] {
    const goals: StrategicGoal[] = [];
    const threatLevel = this.enemyAnalysis.getEnemyThreatAssessment();

    if (threatLevel >= ThreatLevel.MEDIUM) {
      const enemyBase = this.gameState.getEnemyBasePosition();
      if (enemyBase) {
        goals.push({
          id: `goal_attack_${++this.goalIdCounter}`,
          type: GoalType.ATTACK,
          priority: PriorityLevel.HIGH,
          status: GoalStatus.ACTIVE,
          targetPosition: enemyBase,
          requirements: [],
          progress: 0,
          createdAt: Date.now(),
          dependencies: [],
          constraints: [],
          successCriteria: [{
            description: 'Destroy enemy base',
            checkFunction: (state) => this.checkEnemyBaseDestroyed(state),
            weight: 1.0
          }]
        });
      }
    }

    return goals;
  }

  private checkEnemyBaseDestroyed(state: GameState): boolean {
    const enemyBuildings = state.getEnemyBuildings();
    return !enemyBuildings.some(b => b.type === 'command_center' || b.type === 'nexus');
  }

  private generateDefenseGoals(): StrategicGoal[] {
    const goals: StrategicGoal[] = [];
    const threatLevel = this.enemyAnalysis.getEnemyThreatAssessment();

    if (threatLevel >= ThreatLevel.HIGH) {
      goals.push({
        id: `goal_defend_${++this.goalIdCounter}`,
        type: GoalType.DEFEND,
        priority: PriorityLevel.HIGH,
        status: GoalStatus.ACTIVE,
        requirements: [],
        progress: 0,
        createdAt: Date.now(),
        dependencies: [],
        constraints: [],
        successCriteria: [{
          description: 'Defend against enemy attack',
          checkFunction: (state) => this.checkDefenseSuccessful(state),
          weight: 1.0
        }]
      });
    }

    return goals;
  }

  private checkDefenseSuccessful(state: GameState): boolean {
    return state.getPlayerUnits().filter(u => u.isCombatUnit).length >= 10;
  }

  private generateEconomyGoals(): StrategicGoal[] {
    const goals: StrategicGoal[] = [];
    const income = this.resourceAnalysis.calculateIncomeRate();

    if (income.mineralsPerMinute < 300) {
      goals.push({
        id: `goal_economy_${++this.goalIdCounter}`,
        type: GoalType.ECONOMY,
        priority: PriorityLevel.HIGH,
        status: GoalStatus.ACTIVE,
        requirements: [],
        progress: 0,
        createdAt: Date.now(),
        dependencies: [],
        constraints: [],
        successCriteria: [{
          description: 'Increase mineral income',
          checkFunction: (state) => true,
          weight: 1.0
        }]
      });
    }

    return goals;
  }

  public prioritizeGoals(goals: StrategicGoal[]): StrategicGoal[] {
    return goals.sort((a, b) => a.priority - b.priority);
  }

  public updateGoalProgress(goalId: string, progress: number): void {
  }

  public assessGoalCompletion(goalId: string): boolean {
    return false;
  }

  public generateSubGoals(parentGoalId: string): StrategicGoal[] {
    return [];
  }
}

export class PlanGeneratorImpl implements PlanGenerator {
  private gameState: GameState;
  private pathFinder: PathFinder;
  private unitManager: UnitManager;
  private planIdCounter: number = 0;
  private stepIdCounter: number = 0;

  constructor(
    gameState: GameState,
    pathFinder: PathFinder,
    unitManager: UnitManager
  ) {
    this.gameState = gameState;
    this.pathFinder = pathFinder;
    this.unitManager = unitManager;
  }

  public generatePlan(goal: StrategicGoal): StrategicPlan {
    const plan: StrategicPlan = {
      id: `plan_${++this.planIdCounter}`,
      name: `Plan for ${goal.type}`,
      type: this.goalTypeToPlanType(goal.type),
      goalId: goal.id,
      status: PlanStatus.PENDING,
      steps: this.generateStepsForGoal(goal),
      estimatedDuration: 0,
      riskAssessment: this.createInitialRiskAssessment(),
      cost: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] },
      preconditions: [],
      postconditions: [],
      parallelSteps: [],
      criticalPath: []
    };

    plan.estimatedDuration = plan.steps.reduce((sum, step) => sum + step.estimatedDuration, 0);
    return plan;
  }

  private goalTypeToPlanType(goalType: GoalType): PlanType {
    switch (goalType) {
      case GoalType.EXPAND: return PlanType.EXPANSION;
      case GoalType.ATTACK: return PlanType.ATTACK;
      case GoalType.DEFEND: return PlanType.DEFENSE;
      case GoalType.ECONOMY: return PlanType.ECONOMIC;
      default: return PlanType.COMPOUND;
    }
  }

  private generateStepsForGoal(goal: StrategicGoal): PlanStep[] {
    return [
      {
        id: `step_${++this.stepIdCounter}`,
        name: 'Execute goal',
        description: `Execute ${goal.type} goal`,
        status: StepStatus.PENDING,
        actions: [],
        estimatedDuration: 60,
        dependencies: [],
        prerequisites: [],
        effects: [],
        retryCount: 0,
        maxRetries: 3,
        skipConditions: []
      }
    ];
  }

  private createInitialRiskAssessment(): RiskAssessment {
    return {
      overallRisk: ThreatLevel.MEDIUM,
      riskFactors: [],
      totalRiskScore: 0.5,
      mitigationStrategies: [],
      contingencyPlans: []
    };
  }

  public refinePlan(plan: StrategicPlan): StrategicPlan {
    return plan;
  }

  public validatePlan(plan: StrategicPlan): boolean {
    return true;
  }

  public estimatePlanDuration(plan: StrategicPlan): number {
    return plan.estimatedDuration;
  }

  public generateAlternativePlans(goal: StrategicGoal): StrategicPlan[] {
    return [this.generatePlan(goal)];
  }
}

export class PlanEvaluatorImpl implements PlanEvaluator {
  private gameState: GameState;
  private resourceAnalysis: ResourceAnalysisImpl;
  private mapAnalysis: MapAnalysisImpl;

  constructor(
    gameState: GameState,
    resourceAnalysis: ResourceAnalysisImpl,
    mapAnalysis: MapAnalysisImpl
  ) {
    this.gameState = gameState;
    this.resourceAnalysis = resourceAnalysis;
    this.mapAnalysis = mapAnalysis;
  }

  public evaluatePlan(plan: StrategicPlan): PlanEvaluation {
    return {
      planId: plan.id,
      overallScore: 75,
      tacticalScore: 70,
      strategicScore: 75,
      riskScore: 30,
      costEfficiency: 80,
      timeEfficiency: 75,
      successProbability: 0.8,
      resourceAvailability: {
        minerals: { current: 500, required: 100, sufficient: true, shortageRisk: 0 },
        gas: { current: 200, required: 50, sufficient: true, shortageRisk: 0 },
        energy: { current: 100, required: 0, sufficient: true, shortageRisk: 0 },
        food: { current: 100, required: 0, sufficient: true, shortageRisk: 0 }
      },
      riskAssessment: plan.riskAssessment,
      comparisonRank: 1,
      strengths: ['Well structured plan'],
      weaknesses: [],
      recommendations: [],
      timestamp: Date.now()
    };
  }

  public comparePlans(plans: StrategicPlan[]): PlanComparison[] {
    return [];
  }

  public rankPlans(plans: StrategicPlan[]): StrategicPlan[] {
    return plans;
  }

  public assessPlanRisk(plan: StrategicPlan): RiskAssessment {
    return plan.riskAssessment;
  }

  public estimateSuccessProbability(plan: StrategicPlan): number {
    return 0.8;
  }
}

export class StrategySelectorImpl implements StrategySelector {
  private gameState: GameState;
  private resourceAnalysis: ResourceAnalysisImpl;
  private enemyAnalysis: EnemyAnalysisImpl;

  constructor(
    gameState: GameState,
    resourceAnalysis: ResourceAnalysisImpl,
    enemyAnalysis: EnemyAnalysisImpl
  ) {
    this.gameState = gameState;
    this.resourceAnalysis = resourceAnalysis;
    this.enemyAnalysis = enemyAnalysis;
  }

  public selectStrategy(gameState: GameState, availablePlans: StrategicPlan[]): StrategyType {
    const threatLevel = this.enemyAnalysis.getEnemyThreatAssessment();

    if (threatLevel >= ThreatLevel.HIGH) {
      return StrategyType.DEFENSIVE;
    }

    return StrategyType.BALANCED;
  }

  public evaluateStrategy(strategy: StrategyType, gameState: GameState): StrategyEvaluation {
    return {
      strategy,
      overallScore: 75,
      strengths: [],
      weaknesses: [],
      fitScore: 0.7,
      riskLevel: ThreatLevel.MEDIUM,
      expectedOutcome: 'Balanced approach'
    };
  }

  public transitionStrategy(from: StrategyType, to: StrategyType, gameState: GameState): StrategyTransition {
    return {
      from,
      to,
      transitionPlan: this.createTransitionPlan(from, to),
      estimatedTransitionTime: 60,
      risks: [],
      benefits: []
    };
  }

  private createTransitionPlan(from: StrategyType, to: StrategyType): StrategicPlan {
    return {
      id: `transition_${from}_to_${to}`,
      name: `Transition from ${from} to ${to}`,
      type: PlanType.COMPOUND,
      goalId: 'strategy_transition',
      status: PlanStatus.PENDING,
      steps: [],
      estimatedDuration: 60,
      riskAssessment: {
        overallRisk: ThreatLevel.LOW,
        riskFactors: [],
        totalRiskScore: 0.2,
        mitigationStrategies: [],
        contingencyPlans: []
      },
      cost: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] },
      preconditions: [],
      postconditions: [],
      parallelSteps: [],
      criticalPath: []
    };
  }

  public getStrategyParameters(strategy: StrategyType): StrategyParameters {
    return {
      aggressionLevel: 0.5,
      expansionRate: 0.5,
      techInvestment: 0.5,
      defenseLevel: 0.5,
      resourceReserve: 0.2,
      riskTolerance: 0.5,
      preferredUnits: [],
      preferredStrategy: strategy
    };
  }

  public adaptStrategy(strategy: StrategyType, gameState: GameState): StrategyType {
    return strategy;
  }
}

export class ExpansionPlannerImpl implements ExpansionPlanner {
  private gameState: GameState;
  private mapAnalysis: MapAnalysisImpl;
  private resourceAnalysis: ResourceAnalysisImpl;

  constructor(
    gameState: GameState,
    mapAnalysis: MapAnalysisImpl,
    resourceAnalysis: ResourceAnalysisImpl
  ) {
    this.gameState = gameState;
    this.mapAnalysis = mapAnalysis;
    this.resourceAnalysis = resourceAnalysis;
  }

  public identifyExpansionSites(gameState: GameState): ExpansionSite[] {
    return this.mapAnalysis.expansionSites;
  }

  public evaluateExpansionSite(site: ExpansionSite, gameState: GameState): ExpansionEvaluation {
    return {
      site,
      overallScore: 75,
      resourceScore: 80,
      defensiveScore: 70,
      economicScore: 75,
      strategicScore: 80,
      timingScore: 70,
      riskScore: 30,
      recommendation: 'recommended'
    };
  }

  public generateExpansionPlan(site: ExpansionSite, gameState: GameState): StrategicPlan {
    return {
      id: `expansion_plan_${site.id}`,
      name: `Expand to ${site.id}`,
      type: PlanType.EXPANSION,
      goalId: `expand_${site.id}`,
      status: PlanStatus.PENDING,
      steps: [],
      estimatedDuration: 120,
      riskAssessment: {
        overallRisk: ThreatLevel.MEDIUM,
        riskFactors: [],
        totalRiskScore: 0.4,
        mitigationStrategies: [],
        contingencyPlans: []
      },
      cost: { minerals: 400, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 120, units: [] },
      preconditions: [],
      postconditions: [],
      parallelSteps: [],
      criticalPath: []
    };
  }

  public optimizeExpansionTiming(gameState: GameState): ExpansionTiming {
    return {
      optimalTime: Date.now() + 120000,
      earliestViableTime: Date.now() + 60000,
      latestViableTime: Date.now() + 300000,
      resourceRequirements: { minerals: 400, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] },
      riskFactors: [],
      indicators: []
    };
  }

  public assessExpansionRisk(site: ExpansionSite, gameState: GameState): RiskAssessment {
    return {
      overallRisk: ThreatLevel.MEDIUM,
      riskFactors: [],
      totalRiskScore: 0.4,
      mitigationStrategies: [],
      contingencyPlans: []
    };
  }

  public prioritizeExpansions(sites: ExpansionSite[], gameState: GameState): ExpansionSite[] {
    return sites.sort((a, b) => b.strategicValue - a.strategicValue);
  }
}

export class TechPlannerImpl implements TechPlanner {
  private gameState: GameState;
  private resourceAnalysis: ResourceAnalysisImpl;

  constructor(gameState: GameState, resourceAnalysis: ResourceAnalysisImpl) {
    this.gameState = gameState;
    this.resourceAnalysis = resourceAnalysis;
  }

  public analyzeTechTree(gameState: GameState): TechAnalysis {
    return {
      currentLevel: 1,
      availableTechs: [],
      inProgressTechs: [],
      completedTechs: [],
      techDependencies: [],
      recommendedTechPath: [],
      estimatedCompletionTime: 0,
      resourceRequirements: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] }
    };
  }

  public identifyTechOpportunities(gameState: GameState): TechOpportunity[] {
    return [];
  }

  public prioritizeTechs(techs: TechOpportunity[], gameState: GameState): TechOpportunity[] {
    return techs;
  }

  public generateTechPlan(techs: TechOpportunity[], gameState: GameState): StrategicPlan {
    return {
      id: 'tech_plan',
      name: 'Technology Research Plan',
      type: PlanType.TECH,
      goalId: 'tech_research',
      status: PlanStatus.PENDING,
      steps: [],
      estimatedDuration: 0,
      riskAssessment: {
        overallRisk: ThreatLevel.LOW,
        riskFactors: [],
        totalRiskScore: 0.2,
        mitigationStrategies: [],
        contingencyPlans: []
      },
      cost: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] },
      preconditions: [],
      postconditions: [],
      parallelSteps: [],
      criticalPath: []
    };
  }

  public assessTechAdvantages(enemies: EnemyProfile[]): TechAdvantage[] {
    return [];
  }

  public optimizeTechTiming(gameState: GameState): TechTiming {
    return {
      optimalStartTime: Date.now(),
      researchDuration: 120,
      resourceAvailability: { minerals: 0, gas: 0, energy: 0, food: 0, wood: 0, gold: 0, time: 0, units: [] },
      windowOfOpportunity: { start: Date.now(), end: Date.now() + 300000, duration: 300000, conditions: [] },
      risks: []
    };
  }
}

export class EconomyPlannerImpl implements EconomyPlanner {
  private gameState: GameState;
  private resourceAnalysis: ResourceAnalysisImpl;

  constructor(gameState: GameState, resourceAnalysis: ResourceAnalysisImpl) {
    this.gameState = gameState;
    this.resourceAnalysis = resourceAnalysis;
  }

  public analyzeEconomy(gameState: GameState): EconomyAnalysis {
    return {
      currentState: {
        income: this.resourceAnalysis.calculateIncomeRate(),
        expenses: this.resourceAnalysis.calculateExpenditureRate(),
        reserves: this.resourceAnalysis.analyzeCurrentResources().reserves,
        workerAllocation: { minerals: 0, gas: 0, energy: 0, food: 0, military: 0, building: 0, idle: 0, optimal: true },
        resourceManagement: {
          gatheringPriority: [ResourceType.MINERALS, ResourceType.GAS],
          storagePriority: [ResourceType.MINERALS, ResourceType.GAS],
          reservePolicy: 'maintain_20_percent',
          investmentStrategy: 'balanced'
        }
      },
      growthRate: { minerals: 0, gas: 0, energy: 0, food: 0, armyValue: 0, economicValue: 0 },
      efficiency: { overall: 0.75, gathering: 0.8, spending: 0.7, saving: 0.6, investment: 0.65 },
      projections: [],
      bottlenecks: [],
      opportunities: [],
      recommendations: []
    };
  }

  public identifyOptimizationOpportunities(gameState: GameState): GatheringOptimization[] {
    return [];
  }

  public optimizeResourceDistribution(gameState: GameState): void {
  }

  public projectResourceNeeds(futureTime: number, gameState: GameState): ProjectedResource[] {
    return this.resourceAnalysis.projectResources(futureTime);
  }

  public balanceEconomy(gameState: GameState): EconomicPlan {
    return {
      actions: [],
      resourceAllocation: [],
      timeline: { phases: [], milestones: [] },
      riskMitigation: []
    };
  }

  public manageIncomeExpenditure(gameState: GameState): IncomeExpenditurePlan {
    return {
      projectedIncome: this.resourceAnalysis.calculateIncomeRate(),
      projectedExpenses: this.resourceAnalysis.calculateExpenditureRate(),
      balanceProjection: [],
      adjustments: [],
      contingencies: []
    };
  }
}

export class MilitaryPlannerImpl implements MilitaryPlanner {
  private gameState: GameState;
  private unitManager: UnitManager;
  private enemyAnalysis: EnemyAnalysisImpl;
  private mapAnalysis: MapAnalysisImpl;

  constructor(
    gameState: GameState,
    unitManager: UnitManager,
    enemyAnalysis: EnemyAnalysisImpl,
    mapAnalysis: MapAnalysisImpl
  ) {
    this.gameState = gameState;
    this.unitManager = unitManager;
    this.enemyAnalysis = enemyAnalysis;
    this.mapAnalysis = mapAnalysis;
  }

  public analyzeMilitary(gameState: GameState): ArmyAnalysis {
    const units = gameState.getPlayerUnits();
    const combatUnits = units.filter(u => u.isCombatUnit);

    return {
      totalUnits: combatUnits.length,
      unitComposition: {
        units: [],
        totalValue: combatUnits.reduce((sum, u) => sum + (u.value || 10), 0),
        strengths: [],
        weaknesses: [],
        recommendedCount: [],
        counters: []
      },
      armyValue: combatUnits.reduce((sum, u) => sum + (u.value || 10), 0),
      effectiveness: 75,
      strengths: [],
      weaknesses: [],
      recommendedImprovements: [],
      comparison: {
        ourArmy: this.createEmptyArmyAnalysis(),
        enemyArmy: this.createEmptyArmyAnalysis(),
        advantage: 0,
        recommendation: ''
      }
    };
  }

  private createEmptyArmyAnalysis(): ArmyAnalysis {
    return {
      totalUnits: 0,
      unitComposition: { units: [], totalValue: 0, strengths: [], weaknesses: [], recommendedCount: [], counters: [] },
      armyValue: 0,
      effectiveness: 0,
      strengths: [],
      weaknesses: [],
      recommendedImprovements: [],
      comparison: { ourArmy: this.createEmptyArmyAnalysis(), enemyArmy: this.createEmptyArmyAnalysis(), advantage: 0, recommendation: '' }
    };
  }

  public recommendUnitComposition(enemyComposition: UnitComposition, gameState: GameState): UnitComposition {
    return {
      units: [],
      totalValue: 0,
      strengths: [],
      weaknesses: [],
      recommendedCount: [],
      counters: []
    };
  }

  public planArmyMovement(plan: StrategicPlan, gameState: GameState): MovementPlan {
    return {
      armyId: 'main_army',
      waypoints: [],
      formation: { type: 'line', spacing: 5, orientation: new Vector2(1, 0) },
      speed: 100,
      estimatedArrival: Date.now() + 60000,
      risks: []
    };
  }

  public optimizeArmyPositioning(gameState: GameState): PositionOptimization {
    return {
      positions: [],
      coverage: 70,
      defensiveStrength: 80,
      offensiveCapability: 40,
      resourceProtection: 60
    };
  }

  public coordinateMultiArmy(armies: Army[], gameState: GameState): CoordinatedAttack {
    return {
      armies,
      target: new Vector2(500, 500),
      coordinationPlan: [],
      timing: Date.now() + 30000,
      contingencyPlans: []
    };
  }

  public assessBattleOutcome(attackPlan: StrategicPlan, gameState: GameState): BattleAssessment {
    const ourAnalysis = this.analyzeMilitary(gameState);
    const enemyAnalysis = this.enemyAnalysis.getEnemyThreatAssessment();

    const ourStrength = ourAnalysis.armyValue;
    let enemyStrength = 50;

    switch (enemyAnalysis) {
      case ThreatLevel.CRITICAL: enemyStrength = 150; break;
      case ThreatLevel.HIGH: enemyStrength = 100; break;
      case ThreatLevel.MEDIUM: enemyStrength = 75; break;
      case ThreatLevel.LOW: enemyStrength = 30; break;
    }

    const victoryProbability = ourStrength / (ourStrength + enemyStrength);

    return {
      ourStrength,
      enemyStrength,
      victoryProbability,
      expectedCasualties: {
        ourCasualties: Math.floor(ourAnalysis.totalUnits * (1 - victoryProbability)),
        enemyCasualties: Math.floor(enemyStrength * victoryProbability),
        acceptableThreshold: ourAnalysis.totalUnits * 0.3,
        resourceValue: 500
      },
      terrainAdvantage: 0.5,
      recommendations: victoryProbability > 0.6 ? ['Proceed with attack'] : ['Consider defensive strategy'],
      risks: []
    };
  }
}

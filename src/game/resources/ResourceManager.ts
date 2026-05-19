import { EventEmitter } from './EventEmitter';

export enum ResourceType {
  Minerals = 'minerals',
  Gas = 'gas',
  Energy = 'energy',
  Credits = 'credits',
  Supply = 'supply'
}

export enum ResourceNodeType {
  MineralPatch = 'mineral_patch',
  GasExtractor = 'gas_extractor',
  SolarPlant = 'solar_plant',
  CreditVault = 'credit_vault',
  SupplyDepot = 'supply_depot'
}

export enum NodeState {
  Available = 'available',
  Depleted = 'depleted',
  BeingHarvested = 'being_harvested',
  Contested = 'contested'
}

export enum TradeRouteStatus {
  Active = 'active',
  Paused = 'paused',
  Disrupted = 'disrupted',
  Completed = 'completed'
}

export enum SpendingCategory {
  UnitProduction = 'unit_production',
  BuildingConstruction = 'building_construction',
  Research = 'research',
  Upgrades = 'upgrades',
  Maintenance = 'maintenance',
  Emergency = 'emergency',
  Trade = 'trade'
}

export enum WarningLevel {
  None = 'none',
  Low = 'low',
  Critical = 'critical',
  Emergency = 'emergency'
}

export interface ResourceAmount {
  minerals: number;
  gas: number;
  energy: number;
  credits: number;
  supply: number;
}

export interface ResourceCost {
  minerals?: number;
  gas?: number;
  energy?: number;
  credits?: number;
  supply?: number;
  time?: number;
}

export interface IncomeSource {
  id: string;
  name: string;
  type: ResourceType;
  amount: number;
  frequency: number;
  timestamp: number;
  isActive: boolean;
  sourceType: 'passive' | 'active' | 'trade' | 'ally';
}

export interface SpendingRecord {
  id: string;
  category: SpendingCategory;
  description: string;
  cost: ResourceCost;
  timestamp: number;
  playerId: string;
  status: 'pending' | 'completed' | 'cancelled';
}

export interface ResourceNode {
  id: string;
  type: ResourceNodeType;
  position: { x: number; y: number };
  state: NodeState;
  currentAmount: number;
  maxAmount: number;
  harvestRate: number;
  harvesterIds: string[];
  ownerId: string | null;
  contestedBy: string[];
  lastHarvestTime: number;
  depletionTime: number | null;
  spawnTime: number;
  respawnTime: number | null;
  connectedToBase: boolean;
  pathLength: number;
}

export interface ResourceDeposit {
  id: string;
  nodes: ResourceNode[];
  center: { x: number; y: number };
  ownerId: string | null;
  isContested: boolean;
  totalAmount: number;
  maxTotalAmount: number;
  spawnInterval: number;
  lastSpawnTime: number;
  isRegenerating: boolean;
}

export interface ResourceHarvester {
  id: string;
  unitId: string;
  targetNodeId: string | null;
  currentResource: ResourceType;
  carryingAmount: number;
  maxCarryAmount: number;
  harvestRate: number;
  returnRate: number;
  state: 'idle' | 'harvesting' | 'returning' | 'depositing';
  path: { x: number; y: number }[];
  currentPathIndex: number;
  efficiency: number;
  assignedDepositId: string | null;
  totalHarvested: ResourceAmount;
  lastHarvestTime: number;
}

export interface ResourceAllocator {
  playerId: string;
  allocations: Map<SpendingCategory, ResourceCost>;
  priorities: Map<SpendingCategory, number>;
  reservedResources: ResourceAmount;
  emergencyReserve: number;
  isAllocating: boolean;
}

export interface MarketListing {
  id: string;
  resource: ResourceType;
  amount: number;
  pricePerUnit: number;
  sellerId: string;
  buyerId: string | null;
  status: 'active' | 'sold' | 'cancelled';
  createdAt: number;
  expiresAt: number;
}

export interface ResourceMarket {
  listings: MarketListing[];
  priceHistory: Map<ResourceType, number[]>;
  currentPrices: Map<ResourceType, number>;
  volatility: Map<ResourceType, number>;
  totalVolume: Map<ResourceType, number>;
}

export interface StorageFacility {
  id: string;
  type: ResourceType;
  capacity: number;
  currentAmount: number;
  position: { x: number; y: number };
  isActive: boolean;
  efficiency: number;
}

export interface ResourceStorage {
  facilities: StorageFacility[];
  totalCapacity: ResourceAmount;
  currentStorage: ResourceAmount;
  overflowPenalty: number;
}

export interface PassiveIncome {
  sourceId: string;
  resourceType: ResourceType;
  baseAmount: number;
  multiplier: number;
  isActive: boolean;
  nextTickTime: number;
  interval: number;
}

export interface ResourceIncome {
  sources: IncomeSource[];
  passiveIncomes: PassiveIncome[];
  totalIncome: ResourceAmount;
  incomeRate: ResourceAmount;
  lastCalculationTime: number;
}

export interface ResourceSpending {
  records: SpendingRecord[];
  categoryTotals: Map<SpendingCategory, ResourceAmount>;
  pendingSpending: SpendingRecord[];
  completedSpending: SpendingRecord[];
}

export interface ResourceWarning {
  level: WarningLevel;
  resourceType: ResourceType;
  currentAmount: number;
  threshold: number;
  message: string;
  timestamp: number;
  acknowledged: boolean;
}

export interface TradeRoute {
  id: string;
  initiatorId: string;
  recipientId: string;
  resourceType: ResourceType;
  amountPerTrip: number;
  tripsPerMinute: number;
  currentTrip: number;
  totalTrips: number;
  status: TradeRouteStatus;
  startPosition: { x: number; y: number };
  endPosition: { x: number; y: number };
  distance: number;
  incomeGenerated: number;
  isProfitable: boolean;
}

export interface ResourceBudget {
  id: string;
  name: string;
  allocations: Map<SpendingCategory, { percentage: number; amount: ResourceAmount }>;
  totalBudget: ResourceAmount;
  remainingBudget: ResourceAmount;
  period: number;
  startTime: number;
  endTime: number;
  isActive: boolean;
}

export interface ResourceStatistics {
  totalHarvested: ResourceAmount;
  totalSpent: ResourceAmount;
  totalIncome: ResourceAmount;
  totalTraded: ResourceAmount;
  averageIncomeRate: ResourceAmount;
  averageSpendingRate: ResourceAmount;
  peakResources: ResourceAmount;
  lowResourceEvents: number;
  efficiency: number;
  utilizationRate: number;
}

export interface HistoryEntry {
  timestamp: number;
  resources: ResourceAmount;
  income: ResourceAmount;
  spending: ResourceAmount;
  harvesterCount: number;
  activeNodes: number;
  storageUtilization: number;
}

export interface RushIndicator {
  type: 'early_aggression' | 'economic_boom' | 'defensive_posture' | 'tech_rush';
  confidence: number;
  timestamp: number;
  evidence: string[];
  recommendedResponse: string;
}

export class ResourceManager extends EventEmitter {
  private resources: ResourceAmount = {
    minerals: 0,
    gas: 0,
    energy: 0,
    credits: 0,
    supply: 0
  };

  private resourceNodes: Map<string, ResourceNode> = new Map();
  private resourceDeposits: Map<string, ResourceDeposit> = new Map();
  private harvesters: Map<string, ResourceHarvester> = new Map();
  private storage: ResourceStorage;
  private income: ResourceIncome;
  private spending: ResourceSpending;
  private market: ResourceMarket;
  private warnings: Map<ResourceType, ResourceWarning> = new Map();
  private tradeRoutes: Map<string, TradeRoute> = new Map();
  private budgets: Map<string, ResourceBudget> = new Map();
  private statistics: ResourceStatistics;
  private history: HistoryEntry[] = [];
  private rushIndicators: RushIndicator[] = [];

  private allocator: ResourceAllocator;
  private warningThresholds: Map<ResourceType, { low: number; critical: number; emergency: number }> = new Map();
  private maxHistoryEntries: number = 1000;
  private tickRate: number = 1000;
  private lastTickTime: number = 0;
  private gameSpeed: number = 1;
  private playerId: string;
  private allyIds: Set<string> = new Set();
  private sharedResources: Map<string, ResourceAmount> = new Map();
  private resourceLockTimeouts: Map<string, number> = new Map();

  constructor(playerId: string, initialResources?: Partial<ResourceAmount>) {
    super();
    this.playerId = playerId;
    
    this.resources = {
      minerals: initialResources?.minerals ?? 0,
      gas: initialResources?.gas ?? 0,
      energy: initialResources?.energy ?? 0,
      credits: initialResources?.credits ?? 0,
      supply: initialResources?.supply ?? 0
    };

    this.storage = this.initializeStorage();
    this.income = this.initializeIncome();
    this.spending = this.initializeSpending();
    this.market = this.initializeMarket();
    this.statistics = this.initializeStatistics();
    this.allocator = this.initializeAllocator();
    
    this.initializeWarningThresholds();
    this.startResourceTick();
  }

  private initializeStorage(): ResourceStorage {
    return {
      facilities: [],
      totalCapacity: {
        minerals: 10000,
        gas: 5000,
        energy: 10000,
        credits: 10000,
        supply: 200
      },
      currentStorage: {
        minerals: 0,
        gas: 0,
        energy: 0,
        credits: 0,
        supply: 0
      },
      overflowPenalty: 0.1
    };
  }

  private initializeIncome(): ResourceIncome {
    return {
      sources: [],
      passiveIncomes: [],
      totalIncome: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      incomeRate: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      lastCalculationTime: Date.now()
    };
  }

  private initializeSpending(): ResourceSpending {
    return {
      records: [],
      categoryTotals: new Map(),
      pendingSpending: [],
      completedSpending: []
    };
  }

  private initializeMarket(): ResourceMarket {
    const prices = new Map<ResourceType, number>();
    prices.set(ResourceType.Minerals, 10);
    prices.set(ResourceType.Gas, 20);
    prices.set(ResourceType.Energy, 5);
    prices.set(ResourceType.Credits, 15);
    prices.set(ResourceType.Supply, 25);

    return {
      listings: [],
      priceHistory: new Map(),
      currentPrices: prices,
      volatility: new Map(),
      totalVolume: new Map()
    };
  }

  private initializeStatistics(): ResourceStatistics {
    return {
      totalHarvested: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      totalSpent: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      totalIncome: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      totalTraded: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      averageIncomeRate: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      averageSpendingRate: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      peakResources: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      lowResourceEvents: 0,
      efficiency: 0,
      utilizationRate: 0
    };
  }

  private initializeAllocator(): ResourceAllocator {
    const allocations = new Map<SpendingCategory, ResourceCost>();
    Object.values(SpendingCategory).forEach(category => {
      allocations.set(category, {});
    });

    const priorities = new Map<SpendingCategory, number>();
    priorities.set(SpendingCategory.Emergency, 100);
    priorities.set(SpendingCategory.UnitProduction, 80);
    priorities.set(SpendingCategory.BuildingConstruction, 70);
    priorities.set(SpendingCategory.Research, 60);
    priorities.set(SpendingCategory.Upgrades, 50);
    priorities.set(SpendingCategory.Maintenance, 40);
    priorities.set(SpendingCategory.Trade, 30);

    return {
      playerId: this.playerId,
      allocations,
      priorities,
      reservedResources: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      emergencyReserve: 100,
      isAllocating: false
    };
  }

  private initializeWarningThresholds(): void {
    this.warningThresholds.set(ResourceType.Minerals, { low: 100, critical: 50, emergency: 20 });
    this.warningThresholds.set(ResourceType.Gas, { low: 50, critical: 25, emergency: 10 });
    this.warningThresholds.set(ResourceType.Energy, { low: 100, critical: 50, emergency: 20 });
    this.warningThresholds.set(ResourceType.Credits, { low: 75, critical: 30, emergency: 10 });
    this.warningThresholds.set(ResourceType.Supply, { low: 20, critical: 10, emergency: 5 });
  }

  private startResourceTick(): void {
    setInterval(() => this.resourceTick(), this.tickRate);
  }

  private resourceTick(): void {
    const currentTime = Date.now();
    const deltaTime = (currentTime - this.lastTickTime) / 1000;
    this.lastTickTime = currentTime;

    this.calculatePassiveIncome(deltaTime);
    this.updateHarvesterStates(deltaTime);
    this.processTradeRoutes(deltaTime);
    this.updateResourceWarnings();
    this.updateNodeDepletion();
    this.checkRushIndicators();
    this.recordHistory();
    this.cleanupExpiredData();

    this.emit('tick', { deltaTime, resources: this.resources });
  }

  public spawnResourceNode(
    type: ResourceNodeType,
    position: { x: number; y: number },
    amount: number
  ): ResourceNode {
    const id = `node_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    let resourceType: ResourceType;
    let harvestRate = 10;
    
    switch (type) {
      case ResourceNodeType.MineralPatch:
        resourceType = ResourceType.Minerals;
        harvestRate = 10;
        break;
      case ResourceNodeType.GasExtractor:
        resourceType = ResourceType.Gas;
        harvestRate = 8;
        break;
      case ResourceNodeType.SolarPlant:
        resourceType = ResourceType.Energy;
        harvestRate = 15;
        break;
      case ResourceNodeType.CreditVault:
        resourceType = ResourceType.Credits;
        harvestRate = 12;
        break;
      case ResourceNodeType.SupplyDepot:
        resourceType = ResourceType.Supply;
        harvestRate = 5;
        break;
    }

    const node: ResourceNode = {
      id,
      type,
      position,
      state: NodeState.Available,
      currentAmount: amount,
      maxAmount: amount,
      harvestRate,
      harvesterIds: [],
      ownerId: null,
      contestedBy: [],
      lastHarvestTime: 0,
      depletionTime: null,
      spawnTime: Date.now(),
      respawnTime: null,
      connectedToBase: false,
      pathLength: 0
    };

    this.resourceNodes.set(id, node);
    this.emit('nodeSpawned', node);
    
    return node;
  }

  public spawnResourceDeposit(
    center: { x: number; y: number },
    nodeCount: number,
    nodeType: ResourceNodeType,
    totalAmount: number
  ): ResourceDeposit {
    const id = `deposit_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    const nodes: ResourceNode[] = [];
    const amountPerNode = totalAmount / nodeCount;

    for (let i = 0; i < nodeCount; i++) {
      const offsetX = (Math.random() - 0.5) * 100;
      const offsetY = (Math.random() - 0.5) * 100;
      const position = { x: center.x + offsetX, y: center.y + offsetY };
      
      const node = this.spawnResourceNode(nodeType, position, amountPerNode);
      nodes.push(node);
    }

    const deposit: ResourceDeposit = {
      id,
      nodes,
      center,
      ownerId: null,
      isContested: false,
      totalAmount,
      maxTotalAmount: totalAmount,
      spawnInterval: 60000,
      lastSpawnTime: Date.now(),
      isRegenerating: true
    };

    this.resourceDeposits.set(id, deposit);
    this.emit('depositSpawned', deposit);
    
    return deposit;
  }

  public registerHarvester(
    unitId: string,
    maxCarryAmount: number = 20,
    harvestRate: number = 10
  ): ResourceHarvester {
    const id = `harvester_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    const harvester: ResourceHarvester = {
      id,
      unitId,
      targetNodeId: null,
      currentResource: ResourceType.Minerals,
      carryingAmount: 0,
      maxCarryAmount,
      harvestRate,
      returnRate: 15,
      state: 'idle',
      path: [],
      currentPathIndex: 0,
      efficiency: 1.0,
      assignedDepositId: null,
      totalHarvested: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      lastHarvestTime: 0
    };

    this.harvesters.set(id, harvester);
    this.emit('harvesterRegistered', harvester);
    
    return harvester;
  }

  public assignHarvesterToNode(harvesterId: string, nodeId: string): boolean {
    const harvester = this.harvesters.get(harvesterId);
    const node = this.resourceNodes.get(nodeId);

    if (!harvester || !node) {
      return false;
    }

    if (node.state === NodeState.Depleted) {
      return false;
    }

    harvester.targetNodeId = nodeId;
    harvester.state = 'harvesting';
    node.state = NodeState.BeingHarvested;
    node.harvesterIds.push(harvesterId);
    node.ownerId = this.playerId;

    this.emit('harvesterAssigned', { harvester, node });
    
    return true;
  }

  public optimalHarvesterAssignment(): Map<string, string[]> {
    const optimalAssignments = new Map<string, string[]>();
    const availableNodes = Array.from(this.resourceNodes.values())
      .filter(node => node.state === NodeState.Available || node.state === NodeState.BeingHarvested);

    const idleHarvesters = Array.from(this.harvesters.values())
      .filter(h => h.state === 'idle');

    for (const node of availableNodes) {
      optimalAssignments.set(node.id, []);
    }

    for (const harvester of idleHarvesters) {
      let bestNode: ResourceNode | null = null;
      let bestScore = -Infinity;

      for (const node of availableNodes) {
        if (node.harvesterIds.length >= 3) continue;

        const pathLength = this.calculatePathLength(harvester, node);
        const remainingAmount = node.currentAmount / node.maxAmount;
        const harvestScore = remainingAmount * node.harvestRate;
        const distancePenalty = pathLength * 0.1;
        const currentLoad = node.harvesterIds.length * 0.5;
        
        const score = harvestScore - distancePenalty - currentLoad;

        if (score > bestScore) {
          bestScore = score;
          bestNode = node;
        }
      }

      if (bestNode) {
        this.assignHarvesterToNode(harvester.id, bestNode.id);
        const assignments = optimalAssignments.get(bestNode.id) || [];
        assignments.push(harvester.id);
        optimalAssignments.set(bestNode.id, assignments);
      }
    }

    return optimalAssignments;
  }

  private calculatePathLength(harvester: ResourceHarvester, node: ResourceNode): number {
    return Math.sqrt(
      Math.pow(node.position.x - 0, 2) + Math.pow(node.position.y - 0, 2)
    );
  }

  private updateHarvesterStates(deltaTime: number): void {
    for (const harvester of this.harvesters.values()) {
      if (harvester.state === 'harvesting' && harvester.targetNodeId) {
        this.performHarvesting(harvester, deltaTime);
      } else if (harvester.state === 'returning') {
        this.performReturning(harvester, deltaTime);
      }
    }
  }

  private performHarvesting(harvester: ResourceHarvester, deltaTime: number): void {
    const node = this.resourceNodes.get(harvester.targetNodeId!);
    if (!node) {
      harvester.state = 'idle';
      harvester.targetNodeId = null;
      return;
    }

    if (node.currentAmount <= 0) {
      this.depleteNode(node.id);
      harvester.state = 'idle';
      harvester.targetNodeId = null;
      return;
    }

    const harvestAmount = harvester.harvestRate * harvester.efficiency * deltaTime;
    const actualHarvest = Math.min(harvestAmount, node.currentAmount, harvester.maxCarryAmount - harvester.carryingAmount);

    node.currentAmount -= actualHarvest;
    harvester.carryingAmount += actualHarvest;
    harvester.lastHarvestTime = Date.now();

    const resourceKey = this.getResourceTypeFromNodeType(node.type);
    if (resourceKey) {
      (harvester.totalHarvested as any)[resourceKey] += actualHarvest;
    }

    if (harvester.carryingAmount >= harvester.maxCarryAmount) {
      harvester.state = 'returning';
      this.emit('harvesterFull', harvester);
    }

    this.emit('harvested', { harvester, amount: actualHarvest, node });
  }

  private performReturning(harvester: ResourceHarvester, deltaTime: number): void {
    harvester.carryingAmount -= harvester.returnRate * deltaTime;

    if (harvester.carryingAmount <= 0) {
      harvester.carryingAmount = 0;
      
      const resourceType = harvester.currentResource;
      const depositedAmount = harvester.maxCarryAmount - harvester.carryingAmount;
      
      this.depositResource(resourceType, depositedAmount);
      
      harvester.state = harvester.targetNodeId ? 'harvesting' : 'idle';
      
      this.emit('resourcesDeposited', { harvester, amount: depositedAmount, resourceType });
    }
  }

  private getResourceTypeFromNodeType(nodeType: ResourceNodeType): ResourceType | null {
    switch (nodeType) {
      case ResourceNodeType.MineralPatch: return ResourceType.Minerals;
      case ResourceNodeType.GasExtractor: return ResourceType.Gas;
      case ResourceNodeType.SolarPlant: return ResourceType.Energy;
      case ResourceNodeType.CreditVault: return ResourceType.Credits;
      case ResourceNodeType.SupplyDepot: return ResourceType.Supply;
      default: return null;
    }
  }

  private depleteNode(nodeId: string): void {
    const node = this.resourceNodes.get(nodeId);
    if (!node) return;

    node.state = NodeState.Depleted;
    node.depletionTime = Date.now();
    node.currentAmount = 0;
    node.harvesterIds.forEach(harvesterId => {
      const harvester = this.harvesters.get(harvesterId);
      if (harvester) {
        harvester.state = 'idle';
        harvester.targetNodeId = null;
      }
    });
    node.harvesterIds = [];

    const respawnDelay = 120000 + Math.random() * 60000;
    node.respawnTime = Date.now() + respawnDelay;

    this.emit('nodeDepleted', node);
    
    this.scheduleNodeRespawn(nodeId, respawnDelay);
  }

  private scheduleNodeRespawn(nodeId: string, delay: number): void {
    setTimeout(() => {
      this.respawnNode(nodeId);
    }, delay);
  }

  private respawnNode(nodeId: string): void {
    const node = this.resourceNodes.get(nodeId);
    if (!node) return;

    node.state = NodeState.Available;
    node.currentAmount = node.maxAmount;
    node.depletionTime = null;
    node.respawnTime = null;
    node.lastHarvestTime = 0;

    this.emit('nodeRespawned', node);
  }

  private updateNodeDepletion(): void {
    for (const deposit of this.resourceDeposits.values()) {
      if (!deposit.isRegenerating) continue;

      const totalRemaining = deposit.nodes.reduce((sum, node) => sum + node.currentAmount, 0);
      deposit.totalAmount = totalRemaining;

      if (totalRemaining < deposit.maxTotalAmount * 0.2) {
        const timeSinceLastSpawn = Date.now() - deposit.lastSpawnTime;
        
        if (timeSinceLastSpawn >= deposit.spawnInterval) {
          this.spawnNewNodeForDeposit(deposit);
        }
      }
    }
  }

  private spawnNewNodeForDeposit(deposit: ResourceDeposit): void {
    const depletedNodes = deposit.nodes.filter(n => n.state === NodeState.Depleted);
    
    if (depletedNodes.length > 0) {
      const nodeToRespawn = depletedNodes[0];
      this.respawnNode(nodeToRespawn.id);
      deposit.lastSpawnTime = Date.now();
    }
  }

  public depositResource(type: ResourceType, amount: number): boolean {
    if (amount <= 0) return false;

    const currentAmount = (this.resources as any)[type];
    const storageCapacity = (this.storage.totalCapacity as any)[type];

    if (currentAmount + amount > storageCapacity) {
      const overflow = (currentAmount + amount) - storageCapacity;
      const actualDeposit = amount - overflow;
      
      if (actualDeposit > 0) {
        (this.resources as any)[type] += actualDeposit;
        (this.storage.currentStorage as any)[type] = currentAmount + actualDeposit;
        
        this.emit('resourceOverflow', { type, overflow, capacity: storageCapacity });
        return true;
      }
      
      return false;
    }

    (this.resources as any)[type] += amount;
    (this.storage.currentStorage as any)[type] = currentAmount + amount;
    
    this.emit('resourceDeposited', { type, amount, total: this.resources[type as keyof ResourceAmount] });
    
    return true;
  }

  public spendResources(cost: ResourceCost): boolean {
    if (!this.canAfford(cost)) {
      this.emit('insufficientResources', cost);
      return false;
    }

    const record: SpendingRecord = {
      id: `spending_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      category: this.determineSpendingCategory(cost),
      description: 'Resource expenditure',
      cost,
      timestamp: Date.now(),
      playerId: this.playerId,
      status: 'completed'
    };

    if (cost.minerals) this.resources.minerals -= cost.minerals;
    if (cost.gas) this.resources.gas -= cost.gas;
    if (cost.energy) this.resources.energy -= cost.energy;
    if (cost.credits) this.resources.credits -= cost.credits;
    if (cost.supply) this.resources.supply -= cost.supply;

    this.spending.records.push(record);
    this.spending.completedSpending.push(record);
    
    this.updateSpendingTotals(record);
    this.updateStatistics('spending', cost);

    this.emit('resourcesSpent', record);
    
    return true;
  }

  private canAfford(cost: ResourceCost): boolean {
    if (cost.minerals && this.resources.minerals < cost.minerals) return false;
    if (cost.gas && this.resources.gas < cost.gas) return false;
    if (cost.energy && this.resources.energy < cost.energy) return false;
    if (cost.credits && this.resources.credits < cost.credits) return false;
    if (cost.supply && this.resources.supply < cost.supply) return false;
    
    const reservedMinerals = this.allocator.reservedResources.minerals;
    const reservedGas = this.allocator.reservedResources.gas;
    
    if (cost.minerals && (this.resources.minerals - reservedMinerals) < cost.minerals) return false;
    if (cost.gas && (this.resources.gas - reservedGas) < cost.gas) return false;
    
    return true;
  }

  private determineSpendingCategory(cost: ResourceCost): SpendingCategory {
    if (cost.supply && cost.supply > 50) return SpendingCategory.UnitProduction;
    if (cost.energy && cost.energy > 100) return SpendingCategory.BuildingConstruction;
    if (cost.gas && cost.gas > 50) return SpendingCategory.Research;
    
    return SpendingCategory.Maintenance;
  }

  private updateSpendingTotals(record: SpendingRecord): void {
    const category = record.category;
    const current = this.spending.categoryTotals.get(category) || { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 };
    
    if (record.cost.minerals) current.minerals += record.cost.minerals;
    if (record.cost.gas) current.gas += record.cost.gas;
    if (record.cost.energy) current.energy += record.cost.energy;
    if (record.cost.credits) current.credits += record.cost.credits;
    if (record.cost.supply) current.supply += record.cost.supply;
    
    this.spending.categoryTotals.set(category, current);
  }

  public addPassiveIncome(
    sourceId: string,
    name: string,
    resourceType: ResourceType,
    amount: number,
    interval: number
  ): void {
    const source: IncomeSource = {
      id: sourceId,
      name,
      type: resourceType,
      amount,
      frequency: interval,
      timestamp: Date.now(),
      isActive: true,
      sourceType: 'passive'
    };

    const passiveIncome: PassiveIncome = {
      sourceId,
      resourceType,
      baseAmount: amount,
      multiplier: 1.0,
      isActive: true,
      nextTickTime: Date.now() + interval,
      interval
    };

    this.income.sources.push(source);
    this.income.passiveIncomes.push(passiveIncome);
    
    this.emit('passiveIncomeAdded', source);
  }

  private calculatePassiveIncome(deltaTime: number): void {
    const currentTime = Date.now();
    
    for (const passive of this.income.passiveIncomes) {
      if (!passive.isActive) continue;

      if (currentTime >= passive.nextTickTime) {
        const amount = passive.baseAmount * passive.multiplier;
        this.depositResource(passive.resourceType, amount);
        
        (this.income.totalIncome as any)[passive.resourceType] += amount;
        this.income.sources.find(s => s.id === passive.sourceId);
        
        passive.nextTickTime = currentTime + passive.interval;
        
        this.updateStatistics('income', { [passive.resourceType]: amount } as any);
      }
    }

    this.calculateIncomeRate(deltaTime);
  }

  private calculateIncomeRate(deltaTime: number): void {
    const rates: ResourceAmount = { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 };
    
    for (const passive of this.income.passiveIncomes) {
      if (passive.isActive) {
        const rate = (passive.baseAmount * passive.multiplier) / (passive.interval / 1000);
        (rates as any)[passive.resourceType] += rate;
      }
    }

    for (const source of this.income.sources) {
      if (source.isActive && source.sourceType === 'ally') {
        const rate = source.amount / (source.frequency / 1000);
        (rates as any)[source.type] += rate;
      }
    }

    this.income.incomeRate = rates;
    this.income.lastCalculationTime = Date.now();
  }

  public addStorageFacility(
    type: ResourceType,
    capacity: number,
    position: { x: number; y: number }
  ): StorageFacility {
    const id = `storage_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    const facility: StorageFacility = {
      id,
      type,
      capacity,
      currentAmount: 0,
      position,
      isActive: true,
      efficiency: 1.0
    };

    this.storage.facilities.push(facility);
    (this.storage.totalCapacity as any)[type] += capacity;
    
    this.emit('storageFacilityAdded', facility);
    
    return facility;
  }

  public getStorageUtilization(): ResourceAmount {
    const utilization: ResourceAmount = { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 };
    
    for (const type of Object.values(ResourceType)) {
      const capacity = (this.storage.totalCapacity as any)[type];
      const current = (this.resources as any)[type];
      (utilization as any)[type] = capacity > 0 ? current / capacity : 0;
    }

    return utilization;
  }

  public createMarketListing(
    resource: ResourceType,
    amount: number,
    pricePerUnit: number,
    expiresIn: number = 300000
  ): MarketListing {
    const id = `listing_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    const listing: MarketListing = {
      id,
      resource,
      amount,
      pricePerUnit,
      sellerId: this.playerId,
      buyerId: null,
      status: 'active',
      createdAt: Date.now(),
      expiresAt: Date.now() + expiresIn
    };

    this.market.listings.push(listing);
    this.updateMarketPrices(resource);
    
    this.emit('marketListingCreated', listing);
    
    return listing;
  }

  public purchaseFromMarket(listingId: string, buyerId: string): boolean {
    const listing = this.market.listings.find(l => l.id === listingId);
    
    if (!listing || listing.status !== 'active') return false;
    if (Date.now() > listing.expiresAt) {
      listing.status = 'cancelled';
      return false;
    }

    const totalCost = listing.amount * listing.pricePerUnit;
    
    if (this.playerId === listing.sellerId) {
      return false;
    }

    const cost: ResourceCost = { credits: totalCost };
    if (!this.canAfford(cost)) {
      this.emit('marketPurchaseFailed', { listing, reason: 'insufficient_credits' });
      return false;
    }

    this.spendResources(cost);
    this.depositResource(listing.resource, listing.amount);
    
    listing.buyerId = buyerId;
    listing.status = 'sold';
    
    const volume = (this.market.totalVolume.get(listing.resource) || 0) + listing.amount;
    this.market.totalVolume.set(listing.resource, volume);
    
    this.emit('marketPurchaseCompleted', listing);
    
    return true;
  }

  private updateMarketPrices(resource: ResourceType): void {
    const relevantListings = this.market.listings.filter(
      l => l.resource === resource && l.status === 'sold'
    );

    if (relevantListings.length === 0) return;

    const totalPrice = relevantListings.reduce((sum, l) => sum + l.pricePerUnit * l.amount, 0);
    const totalAmount = relevantListings.reduce((sum, l) => sum + l.amount, 0);
    const averagePrice = totalPrice / totalAmount;

    const currentPrice = this.market.currentPrices.get(resource) || averagePrice;
    const priceChange = Math.abs(averagePrice - currentPrice) / currentPrice;
    this.market.volatility.set(resource, priceChange);

    this.market.currentPrices.set(resource, averagePrice);

    const history = this.market.priceHistory.get(resource) || [];
    history.push(averagePrice);
    if (history.length > 100) {
      history.shift();
    }
    this.market.priceHistory.set(resource, history);
  }

  public createTradeRoute(
    recipientId: string,
    resourceType: ResourceType,
    amountPerTrip: number,
    tripsPerMinute: number,
    startPosition: { x: number; y: number },
    endPosition: { x: number; y: number }
  ): TradeRoute {
    const id = `trade_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    const distance = Math.sqrt(
      Math.pow(endPosition.x - startPosition.x, 2) + 
      Math.pow(endPosition.y - startPosition.y, 2)
    );

    const profitPerTrip = amountPerTrip * (this.getMarketPrice(resourceType) * 0.1);
    const travelTime = distance / 100;
    const actualTripsPerMinute = 60 / travelTime;
    const isProfitable = profitPerTrip > distance * 0.01;

    const route: TradeRoute = {
      id,
      initiatorId: this.playerId,
      recipientId,
      resourceType,
      amountPerTrip,
      tripsPerMinute: Math.min(tripsPerMinute, actualTripsPerMinute),
      currentTrip: 0,
      totalTrips: 0,
      status: TradeRouteStatus.Active,
      startPosition,
      endPosition,
      distance,
      incomeGenerated: 0,
      isProfitable
    };

    this.tradeRoutes.set(id, route);
    this.emit('tradeRouteCreated', route);
    
    return route;
  }

  private processTradeRoutes(deltaTime: number): void {
    for (const route of this.tradeRoutes.values()) {
      if (route.status !== TradeRouteStatus.Active) continue;

      const tripDuration = 60 / route.tripsPerMinute;
      route.currentTrip += deltaTime / tripDuration;

      if (route.currentTrip >= 1) {
        route.currentTrip = 0;
        route.totalTrips++;
        
        this.executeTradeTrip(route);
      }
    }
  }

  private executeTradeTrip(route: TradeRoute): void {
    const profit = route.amountPerTrip * (this.getMarketPrice(route.resourceType) * 0.1);
    
    route.incomeGenerated += profit;
    this.depositResource(ResourceType.Credits, profit);
    
    this.emit('tradeTripCompleted', { route, profit });
  }

  public getMarketPrice(resource: ResourceType): number {
    return this.market.currentPrices.get(resource) || 10;
  }

  public setResourceWarning(type: ResourceType, threshold: number, level: WarningLevel): void {
    const warning: ResourceWarning = {
      level,
      resourceType: type,
      currentAmount: (this.resources as any)[type],
      threshold,
      message: this.generateWarningMessage(type, level, threshold),
      timestamp: Date.now(),
      acknowledged: false
    };

    this.warnings.set(type, warning);
    
    if (level !== WarningLevel.None) {
      this.emit('resourceWarning', warning);
    }
  }

  private updateResourceWarnings(): void {
    for (const type of Object.values(ResourceType)) {
      const current = (this.resources as any)[type];
      const thresholds = this.warningThresholds.get(type);
      
      if (!thresholds) continue;

      let level = WarningLevel.None;
      
      if (current <= thresholds.emergency) {
        level = WarningLevel.Emergency;
      } else if (current <= thresholds.critical) {
        level = WarningLevel.Critical;
      } else if (current <= thresholds.low) {
        level = WarningLevel.Low;
      }

      const warning: ResourceWarning = {
        level,
        resourceType: type,
        currentAmount: current,
        threshold: thresholds[level.toLowerCase() as keyof typeof thresholds] || 0,
        message: this.generateWarningMessage(type, level, current),
        timestamp: Date.now(),
        acknowledged: false
      };

      this.warnings.set(type, warning);

      if (level === WarningLevel.Critical || level === WarningLevel.Emergency) {
        this.emit('criticalResourceWarning', warning);
        
        if (level === WarningLevel.Emergency) {
          this.statistics.lowResourceEvents++;
        }
      }
    }
  }

  private generateWarningMessage(type: ResourceType, level: WarningLevel, value: number): string {
    if (level === WarningLevel.None) return '';
    
    const resourceName = type.charAt(0).toUpperCase() + type.slice(1);
    
    switch (level) {
      case WarningLevel.Low:
        return `${resourceName} is running low (${value.toFixed(0)} remaining)`;
      case WarningLevel.Critical:
        return `CRITICAL: ${resourceName} is very low (${value.toFixed(0)} remaining)`;
      case WarningLevel.Emergency:
        return `EMERGENCY: ${resourceName} depleted! (${value.toFixed(0)} remaining)`;
      default:
        return '';
    }
  }

  public acknowledgeWarning(type: ResourceType): void {
    const warning = this.warnings.get(type);
    if (warning) {
      warning.acknowledged = true;
      this.emit('warningAcknowledged', warning);
    }
  }

  public createBudget(
    name: string,
    period: number,
    allocations: Map<SpendingCategory, number>
  ): ResourceBudget {
    const id = `budget_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    const budgetAllocations = new Map<SpendingCategory, { percentage: number; amount: ResourceAmount }>();
    
    for (const [category, percentage] of allocations) {
      budgetAllocations.set(category, {
        percentage,
        amount: {
          minerals: this.resources.minerals * (percentage / 100),
          gas: this.resources.gas * (percentage / 100),
          energy: this.resources.energy * (percentage / 100),
          credits: this.resources.credits * (percentage / 100),
          supply: this.resources.supply * (percentage / 100)
        }
      });
    }

    const budget: ResourceBudget = {
      id,
      name,
      allocations: budgetAllocations,
      totalBudget: { ...this.resources },
      remainingBudget: { ...this.resources },
      period,
      startTime: Date.now(),
      endTime: Date.now() + period,
      isActive: true
    };

    this.budgets.set(id, budget);
    this.emit('budgetCreated', budget);
    
    return budget;
  }

  public allocateFromBudget(budgetId: string, category: SpendingCategory, cost: ResourceCost): boolean {
    const budget = this.budgets.get(budgetId);
    if (!budget || !budget.isActive) return false;

    const allocation = budget.allocations.get(category);
    if (!allocation) return false;

    if (Date.now() > budget.endTime) {
      budget.isActive = false;
      return false;
    }

    if (!this.canAfford(cost)) return false;

    if (cost.minerals && allocation.amount.minerals < cost.minerals) return false;
    if (cost.gas && allocation.amount.gas < cost.gas) return false;
    if (cost.energy && allocation.amount.energy < cost.energy) return false;
    if (cost.credits && allocation.amount.credits < cost.credits) return false;
    if (cost.supply && allocation.amount.supply < cost.supply) return false;

    this.spendResources(cost);

    if (cost.minerals) allocation.amount.minerals -= cost.minerals;
    if (cost.gas) allocation.amount.gas -= cost.gas;
    if (cost.energy) allocation.amount.energy -= cost.energy;
    if (cost.credits) allocation.amount.credits -= cost.credits;
    if (cost.supply) allocation.amount.supply -= cost.supply;

    if (cost.minerals) budget.remainingBudget.minerals -= cost.minerals;
    if (cost.gas) budget.remainingBudget.gas -= cost.gas;
    if (cost.energy) budget.remainingBudget.energy -= cost.energy;
    if (cost.credits) budget.remainingBudget.credits -= cost.credits;
    if (cost.supply) budget.remainingBudget.supply -= cost.supply;

    this.emit('budgetAllocation', { budget, category, cost });
    
    return true;
  }

  public reserveResources(amount: ResourceAmount, reason: string): void {
    this.allocator.reservedResources.minerals += amount.minerals;
    this.allocator.reservedResources.gas += amount.gas;
    this.allocator.reservedResources.energy += amount.energy;
    this.allocator.reservedResources.credits += amount.credits;
    this.allocator.reservedResources.supply += amount.supply;

    this.emit('resourcesReserved', { amount, reason });
  }

  public releaseReservedResources(amount: ResourceAmount): void {
    this.allocator.reservedResources.minerals = Math.max(0, this.allocator.reservedResources.minerals - amount.minerals);
    this.allocator.reservedResources.gas = Math.max(0, this.allocator.reservedResources.gas - amount.gas);
    this.allocator.reservedResources.energy = Math.max(0, this.allocator.reservedResources.energy - amount.energy);
    this.allocator.reservedResources.credits = Math.max(0, this.allocator.reservedResources.credits - amount.credits);
    this.allocator.reservedResources.supply = Math.max(0, this.allocator.reservedResources.supply - amount.supply);
  }

  public shareResourcesWithAlly(allyId: string, amount: ResourceAmount): void {
    if (!this.allyIds.has(allyId)) {
      this.allyIds.add(allyId);
    }

    const shared = this.getAvailableResources();
    
    if (amount.minerals > shared.minerals) amount.minerals = shared.minerals;
    if (amount.gas > shared.gas) amount.gas = shared.gas;
    if (amount.energy > shared.energy) amount.energy = shared.energy;
    if (amount.credits > shared.credits) amount.credits = shared.credits;
    if (amount.supply > shared.supply) amount.supply = shared.supply;

    this.resources.minerals -= amount.minerals;
    this.resources.gas -= amount.gas;
    this.resources.energy -= amount.energy;
    this.resources.credits -= amount.credits;
    this.resources.supply -= amount.supply;

    const currentShared = this.sharedResources.get(allyId) || { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 };
    currentShared.minerals += amount.minerals;
    currentShared.gas += amount.gas;
    currentShared.energy += amount.energy;
    currentShared.credits += amount.credits;
    currentShared.supply += amount.supply;
    
    this.sharedResources.set(allyId, currentShared);

    const source: IncomeSource = {
      id: `ally_share_${allyId}`,
      name: `Alliance with ${allyId}`,
      type: ResourceType.Minerals,
      amount: amount.minerals,
      frequency: 60000,
      timestamp: Date.now(),
      isActive: true,
      sourceType: 'ally'
    };
    this.income.sources.push(source);

    this.emit('resourcesShared', { allyId, amount });
  }

  public requestResourcesFromAlly(allyId: string, amount: ResourceAmount): void {
    this.emit('resourceRequest', { allyId, amount, requesterId: this.playerId });
  }

  public setEmergencyReserve(amount: number): void {
    this.allocator.emergencyReserve = amount;
    this.emit('emergencyReserveChanged', amount);
  }

  private updateStatistics(type: 'income' | 'spending' | 'harvest', data: any): void {
    if (type === 'income') {
      for (const key of Object.keys(data)) {
        (this.statistics.totalIncome as any)[key] += (data as any)[key];
      }
    } else if (type === 'spending') {
      for (const key of Object.keys(data)) {
        (this.statistics.totalSpent as any)[key] += (data as any)[key];
      }
    } else if (type === 'harvest') {
      for (const key of Object.keys(data)) {
        (this.statistics.totalHarvested as any)[key] += (data as any)[key];
      }
    }

    for (const key of Object.keys(this.resources)) {
      const current = (this.resources as any)[key];
      const peak = (this.statistics.peakResources as any)[key];
      if (current > peak) {
        (this.statistics.peakResources as any)[key] = current;
      }
    }

    this.calculateEfficiency();
  }

  private calculateEfficiency(): void {
    let totalIncome = 0;
    let totalSpent = 0;
    let totalHarvested = 0;

    for (const key of Object.keys(this.statistics.totalIncome)) {
      totalIncome += (this.statistics.totalIncome as any)[key];
      totalSpent += (this.statistics.totalSpent as any)[key];
      totalHarvested += (this.statistics.totalHarvested as any)[key];
    }

    this.statistics.efficiency = totalHarvested > 0 ? totalSpent / totalHarvested : 0;

    const activeHarvesters = Array.from(this.harvesters.values()).filter(h => h.state !== 'idle');
    const totalHarvesters = this.harvesters.size;
    this.statistics.utilizationRate = totalHarvesters > 0 ? activeHarvesters.length / totalHarvesters : 0;
  }

  private recordHistory(): void {
    const entry: HistoryEntry = {
      timestamp: Date.now(),
      resources: { ...this.resources },
      income: { ...this.income.totalIncome },
      spending: { ...this.income.totalIncome },
      harvesterCount: this.harvesters.size,
      activeNodes: Array.from(this.resourceNodes.values()).filter(n => n.state !== NodeState.Depleted).length,
      storageUtilization: this.getAverageStorageUtilization()
    };

    this.history.push(entry);

    if (this.history.length > this.maxHistoryEntries) {
      this.history.shift();
    }
  }

  private getAverageStorageUtilization(): number {
    let totalUtil = 0;
    let count = 0;

    for (const type of Object.values(ResourceType)) {
      const capacity = (this.storage.totalCapacity as any)[type];
      const current = (this.resources as any)[type];
      if (capacity > 0) {
        totalUtil += current / capacity;
        count++;
      }
    }

    return count > 0 ? totalUtil / count : 0;
  }

  private cleanupExpiredData(): void {
    const currentTime = Date.now();
    const maxAge = 3600000;

    this.market.listings = this.market.listings.filter(
      listing => listing.status === 'active' && listing.expiresAt > currentTime
    );

    this.spending.records = this.spending.records.filter(
      record => currentTime - record.timestamp < maxAge
    );
  }

  private checkRushIndicators(): void {
    if (this.history.length < 10) return;

    const recentHistory = this.history.slice(-10);
    const resourceDepletionRate = this.calculateResourceDepletionRate(recentHistory);
    const harvesterActivityRate = this.calculateHarvesterActivityRate(recentHistory);
    const spendingPattern = this.analyzeSpendingPattern();
    
    const indicators: RushIndicator[] = [];

    if (resourceDepletionRate > 2.0 && harvesterActivityRate > 0.8) {
      indicators.push({
        type: 'early_aggression',
        confidence: 0.7,
        timestamp: Date.now(),
        evidence: ['High resource depletion rate', 'Active harvester force'],
        recommendedResponse: 'Consider defensive structures and unit production'
      });
    }

    if (this.income.sources.length > 5 && resourceDepletionRate < 0.5) {
      indicators.push({
        type: 'economic_boom',
        confidence: 0.8,
        timestamp: Date.now(),
        evidence: ['Multiple income sources', 'Low resource depletion'],
        recommendedResponse: 'Continue economic expansion'
      });
    }

    const unitSpending = this.spending.categoryTotals.get(SpendingCategory.UnitProduction);
    if (unitSpending && unitSpending.minerals > 500 && resourceDepletionRate > 1.5) {
      indicators.push({
        type: 'early_aggression',
        confidence: 0.85,
        timestamp: Date.now(),
        evidence: ['Heavy unit production', 'High resource consumption'],
        recommendedResponse: 'Rush defense recommended'
      });
    }

    if (this.history.length >= 20) {
      const olderHistory = this.history.slice(-20, -10);
      const currentRate = this.calculateIncomeRate(this.history.slice(-5));
      const pastRate = this.calculateIncomeRate(olderHistory);

      if (currentRate < pastRate * 0.5) {
        indicators.push({
          type: 'defensive_posture',
          confidence: 0.6,
          timestamp: Date.now(),
          evidence: ['Significant income reduction', 'Possible harassment or pressure'],
          recommendedResponse: 'Increase harvester protection'
        });
      }
    }

    this.rushIndicators = indicators;
    
    if (indicators.length > 0) {
      this.emit('rushIndicatorsDetected', indicators);
    }
  }

  private calculateResourceDepletionRate(history: HistoryEntry[]): number {
    if (history.length < 2) return 0;

    let totalDepletion = 0;
    
    for (let i = 1; i < history.length; i++) {
      const prev = history[i - 1];
      const curr = history[i];
      
      for (const key of Object.keys(curr.resources)) {
        const diff = (prev.resources as any)[key] - (curr.resources as any)[key];
        if (diff > 0) {
          totalDepletion += diff;
        }
      }
    }

    return totalDepletion / (history.length - 1);
  }

  private calculateHarvesterActivityRate(history: HistoryEntry[]): number {
    if (history.length === 0) return 0;

    const totalActivity = history.reduce((sum, entry) => sum + entry.harvesterCount, 0);
    const maxHarvesters = Math.max(...history.map(h => h.harvesterCount));
    
    return maxHarvesters > 0 ? totalActivity / (history.length * maxHarvesters) : 0;
  }

  private calculateIncomeRate(history: HistoryEntry[]): number {
    if (history.length < 2) return 0;

    let totalIncome = 0;
    
    for (const entry of history) {
      for (const key of Object.keys(entry.income)) {
        totalIncome += (entry.income as any)[key];
      }
    }

    return totalIncome / history.length;
  }

  private analyzeSpendingPattern(): Map<SpendingCategory, number> {
    const pattern = new Map<SpendingCategory, number>();
    let totalSpending = 0;

    for (const [category, totals] of this.spending.categoryTotals) {
      let categoryTotal = 0;
      for (const key of Object.keys(totals)) {
        categoryTotal += (totals as any)[key];
      }
      pattern.set(category, categoryTotal);
      totalSpending += categoryTotal;
    }

    for (const [category, amount] of pattern) {
      pattern.set(category, totalSpending > 0 ? amount / totalSpending : 0);
    }

    return pattern;
  }

  public detectRampStrategy(): { isRamping: boolean; intensity: number; predictedThreat: boolean } {
    if (this.history.length < 30) {
      return { isRamping: false, intensity: 0, predictedThreat: false };
    }

    const recentHistory = this.history.slice(-30);
    const incomeTrend = this.calculateIncomeTrend(recentHistory);
    const spendingTrend = this.calculateSpendingTrend(recentHistory);
    const storageTrend = this.calculateStorageTrend(recentHistory);

    const isRamping = incomeTrend > 0.1 && spendingTrend > 0.05;
    const intensity = Math.min(1, (incomeTrend + spendingTrend) / 2);
    const predictedThreat = isRamping && intensity > 0.7;

    return { isRamping, intensity, predictedThreat };
  }

  private calculateIncomeTrend(history: HistoryEntry[]): number {
    if (history.length < 5) return 0;

    const recent = history.slice(-5);
    const older = history.slice(-10, -5);

    const recentAvg = this.getAverageIncome(recent);
    const olderAvg = this.getAverageIncome(older);

    return olderAvg > 0 ? (recentAvg - olderAvg) / olderAvg : 0;
  }

  private calculateSpendingTrend(history: HistoryEntry[]): number {
    if (history.length < 5) return 0;

    const recent = history.slice(-5);
    const older = history.slice(-10, -5);

    const recentAvg = this.getAverageSpending(recent);
    const olderAvg = this.getAverageSpending(older);

    return olderAvg > 0 ? (recentAvg - olderAvg) / olderAvg : 0;
  }

  private calculateStorageTrend(history: HistoryEntry[]): number {
    if (history.length < 5) return 0;

    const recent = history.slice(-5);
    const older = history.slice(-10, -5);

    const recentAvg = this.getAverageStorage(recent);
    const olderAvg = this.getAverageStorage(older);

    return recentAvg > 0 ? (olderAvg - recentAvg) / recentAvg : 0;
  }

  private getAverageIncome(history: HistoryEntry[]): number {
    return history.reduce((sum, entry) => {
      let total = 0;
      for (const key of Object.keys(entry.income)) {
        total += (entry.income as any)[key];
      }
      return sum + total;
    }, 0) / history.length;
  }

  private getAverageSpending(history: HistoryEntry[]): number {
    return history.reduce((sum, entry) => {
      let total = 0;
      for (const key of Object.keys(entry.spending)) {
        total += (entry.spending as any)[key];
      }
      return sum + total;
    }, 0) / history.length;
  }

  private getAverageStorage(history: HistoryEntry[]): number {
    return history.reduce((sum, entry) => sum + entry.storageUtilization, 0) / history.length;
  }

  public getResourceAmount(type: ResourceType): number {
    return (this.resources as any)[type];
  }

  public getAllResources(): ResourceAmount {
    return { ...this.resources };
  }

  public getAvailableResources(): ResourceAmount {
    return {
      minerals: Math.max(0, this.resources.minerals - this.allocator.reservedResources.minerals),
      gas: Math.max(0, this.resources.gas - this.allocator.reservedResources.gas),
      energy: Math.max(0, this.resources.energy - this.allocator.reservedResources.energy),
      credits: Math.max(0, this.resources.credits - this.allocator.reservedResources.credits),
      supply: Math.max(0, this.resources.supply - this.allocator.reservedResources.supply)
    };
  }

  public getResourceNodes(): ResourceNode[] {
    return Array.from(this.resourceNodes.values());
  }

  public getResourceDeposits(): ResourceDeposit[] {
    return Array.from(this.resourceDeposits.values());
  }

  public getHarvesters(): ResourceHarvester[] {
    return Array.from(this.harvesters.values());
  }

  public getWarnings(): Map<ResourceType, ResourceWarning> {
    return new Map(this.warnings);
  }

  public getTradeRoutes(): TradeRoute[] {
    return Array.from(this.tradeRoutes.values());
  }

  public getBudgets(): ResourceBudget[] {
    return Array.from(this.budgets.values());
  }

  public getStatistics(): ResourceStatistics {
    return { ...this.statistics };
  }

  public getHistory(limit?: number): HistoryEntry[] {
    if (limit) {
      return this.history.slice(-limit);
    }
    return [...this.history];
  }

  public getRushIndicators(): RushIndicator[] {
    return [...this.rushIndicators];
  }

  public setResources(amount: ResourceAmount): void {
    this.resources = { ...amount };
    this.emit('resourcesSet', this.resources);
  }

  public setGameSpeed(speed: number): void {
    this.gameSpeed = Math.max(0.1, Math.min(10, speed));
    this.tickRate = 1000 / this.gameSpeed;
    this.emit('gameSpeedChanged', this.gameSpeed);
  }

  public addAlly(allyId: string): void {
    this.allyIds.add(allyId);
    this.emit('allyAdded', allyId);
  }

  public removeAlly(allyId: string): void {
    this.allyIds.delete(allyId);
    this.sharedResources.delete(allyId);
    this.emit('allyRemoved', allyId);
  }

  public getAllyIds(): string[] {
    return Array.from(this.allyIds);
  }

  public isAlly(playerId: string): boolean {
    return this.allyIds.has(playerId);
  }

  public pauseTradeRoute(routeId: string): void {
    const route = this.tradeRoutes.get(routeId);
    if (route) {
      route.status = TradeRouteStatus.Paused;
      this.emit('tradeRoutePaused', route);
    }
  }

  public resumeTradeRoute(routeId: string): void {
    const route = this.tradeRoutes.get(routeId);
    if (route) {
      route.status = TradeRouteStatus.Active;
      this.emit('tradeRouteResumed', route);
    }
  }

  public cancelTradeRoute(routeId: string): void {
    const route = this.tradeRoutes.get(routeId);
    if (route) {
      route.status = TradeRouteStatus.Completed;
      this.tradeRoutes.delete(routeId);
      this.emit('tradeRouteCancelled', route);
    }
  }

  public lockResource(type: ResourceType, amount: number, timeout: number): string {
    const lockId = `lock_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    const available = this.getAvailableResources();
    if ((available as any)[type] < amount) {
      return '';
    }

    this.resourceLockTimeouts.set(lockId, Date.now() + timeout);
    this.reserveResources({ minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 }, lockId);
    (this.allocator.reservedResources as any)[type] += amount;

    setTimeout(() => {
      this.unlockResource(lockId, type, amount);
    }, timeout);

    this.emit('resourceLocked', { lockId, type, amount });
    
    return lockId;
  }

  public unlockResource(lockId: string, type: ResourceType, amount: number): void {
    this.resourceLockTimeouts.delete(lockId);
    (this.allocator.reservedResources as any)[type] = Math.max(0, (this.allocator.reservedResources as any)[type] - amount);
    this.emit('resourceUnlocked', { lockId, type, amount });
  }

  public getOptimalHarvesterCount(nodeId: string): number {
    const node = this.resourceNodes.get(nodeId);
    if (!node) return 0;

    const pathLength = this.calculatePathLength({ id: '', unitId: '', targetNodeId: null, currentResource: ResourceType.Minerals, carryingAmount: 0, maxCarryAmount: 20, harvestRate: 10, returnRate: 15, state: 'idle', path: [], currentPathIndex: 0, efficiency: 1.0, assignedDepositId: null, totalHarvested: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 }, lastHarvestTime: 0 }, node);
    
    const baseHarvesters = 2;
    const pathBonus = Math.floor(pathLength / 50);
    const maxHarvesters = 3;

    return Math.min(maxHarvesters, baseHarvesters + pathBonus);
  }

  public recalculateOptimalAssignments(): void {
    for (const harvester of this.harvesters.values()) {
      if (harvester.targetNodeId) {
        const node = this.resourceNodes.get(harvester.targetNodeId);
        if (node && node.state === NodeState.Depleted) {
          harvester.state = 'idle';
          harvester.targetNodeId = null;
        }
      }
    }

    this.optimalHarvesterAssignment();
    this.emit('assignmentsRecalculated');
  }

  public getContestedNodes(): ResourceNode[] {
    return Array.from(this.resourceNodes.values())
      .filter(node => node.state === NodeState.Contested);
  }

  public markNodeAsContested(nodeId: string, byPlayerId: string): void {
    const node = this.resourceNodes.get(nodeId);
    if (node) {
      node.state = NodeState.Contested;
      if (!node.contestedBy.includes(byPlayerId)) {
        node.contestedBy.push(byPlayerId);
      }
      
      const deposit = Array.from(this.resourceDeposits.values())
        .find(d => d.nodes.some(n => n.id === nodeId));
      
      if (deposit) {
        deposit.isContested = true;
      }

      this.emit('nodeContested', { node, byPlayerId });
    }
  }

  public clearNodeContestation(nodeId: string, byPlayerId?: string): void {
    const node = this.resourceNodes.get(nodeId);
    if (node) {
      if (byPlayerId) {
        node.contestedBy = node.contestedBy.filter(id => id !== byPlayerId);
      } else {
        node.contestedBy = [];
      }

      if (node.contestedBy.length === 0) {
        node.state = node.currentAmount > 0 ? NodeState.Available : NodeState.Depleted;
        
        const deposit = Array.from(this.resourceDeposits.values())
          .find(d => d.nodes.some(n => n.id === nodeId));
        
        if (deposit) {
          deposit.isContested = deposit.nodes.some(n => n.state === NodeState.Contested);
        }
      }

      this.emit('nodeContestationCleared', { node, byPlayerId });
    }
  }

  public getTotalIncomeRate(): ResourceAmount {
    return { ...this.income.incomeRate };
  }

  public getSpendingByCategory(category: SpendingCategory): ResourceAmount {
    return this.spending.categoryTotals.get(category) || { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 };
  }

  public getMarketStats(): { prices: Map<ResourceType, number>; volatility: Map<ResourceType, number>; volume: Map<ResourceType, number> } {
    return {
      prices: new Map(this.market.currentPrices),
      volatility: new Map(this.market.volatility),
      volume: new Map(this.market.totalVolume)
    };
  }

  public exportState(): any {
    return {
      resources: this.getAllResources(),
      nodes: this.getResourceNodes(),
      deposits: this.getResourceDeposits(),
      harvesters: this.getHarvesters(),
      warnings: Array.from(this.warnings.entries()),
      tradeRoutes: this.getTradeRoutes(),
      budgets: this.getBudgets(),
      statistics: this.getStatistics(),
      historyLength: this.history.length,
      income: this.income,
      storage: this.storage
    };
  }

  public importState(state: any): void {
    if (state.resources) {
      this.resources = state.resources;
    }
    
    if (state.nodes) {
      this.resourceNodes.clear();
      state.nodes.forEach((node: ResourceNode) => {
        this.resourceNodes.set(node.id, node);
      });
    }
    
    if (state.deposits) {
      this.resourceDeposits.clear();
      state.deposits.forEach((deposit: ResourceDeposit) => {
        this.resourceDeposits.set(deposit.id, deposit);
      });
    }
    
    if (state.harvesters) {
      this.harvesters.clear();
      state.harvesters.forEach((harvester: ResourceHarvester) => {
        this.harvesters.set(harvester.id, harvester);
      });
    }

    this.emit('stateImported', state);
  }

  public reset(): void {
    this.resources = { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 };
    this.resourceNodes.clear();
    this.resourceDeposits.clear();
    this.harvesters.clear();
    this.warnings.clear();
    this.tradeRoutes.clear();
    this.budgets.clear();
    this.history = [];
    this.rushIndicators = [];
    this.income = this.initializeIncome();
    this.spending = this.initializeSpending();
    this.allocator = this.initializeAllocator();
    
    this.emit('reset');
  }
}

export class EventEmitter {
  private events: Map<string, Function[]> = new Map();

  public on(event: string, listener: Function): void {
    if (!this.events.has(event)) {
      this.events.set(event, []);
    }
    this.events.get(event)!.push(listener);
  }

  public off(event: string, listener: Function): void {
    const listeners = this.events.get(event);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  public emit(event: string, ...args: any[]): void {
    const listeners = this.events.get(event);
    if (listeners) {
      listeners.forEach(listener => {
        try {
          listener(...args);
        } catch (error) {
          console.error(`Error in event listener for ${event}:`, error);
        }
      });
    }
  }

  public once(event: string, listener: Function): void {
    const onceWrapper = (...args: any[]) => {
      this.off(event, onceWrapper);
      listener(...args);
    };
    this.on(event, onceWrapper);
  }

  public removeAllListeners(event?: string): void {
    if (event) {
      this.events.delete(event);
    } else {
      this.events.clear();
    }
  }
}

export interface ResourcePrediction {
  timestamp: number;
  resources: ResourceAmount;
  confidence: number;
  factors: string[];
}

export interface ResourceConstraint {
  type: ResourceType;
  minAmount: number;
  maxAmount: number;
  priority: number;
  reason: string;
}

export interface ResourceSchedulingItem {
  id: string;
  resourceType: ResourceType;
  amount: number;
  scheduledTime: number;
  isIncome: boolean;
  source?: string;
  status: 'pending' | 'completed' | 'failed';
}

export interface ResourceTransaction {
  id: string;
  timestamp: number;
  type: 'income' | 'expense' | 'transfer' | 'trade';
  resourceType: ResourceType;
  amount: number;
  balance: number;
  description: string;
  relatedEntity?: string;
}

export interface ResourceAlert {
  id: string;
  type: 'warning' | 'critical' | 'opportunity';
  resourceType: ResourceType;
  currentValue: number;
  threshold: number;
  message: string;
  recommendedAction: string;
  timestamp: number;
  acknowledged: boolean;
}

export interface AllianceResourceAgreement {
  id: string;
  allyId: string;
  resourceType: ResourceType;
  amountPerMinute: number;
  duration: number;
  startTime: number;
  endTime: number;
  status: 'active' | 'paused' | 'terminated';
  totalTransferred: number;
}

export interface ResourceForecast {
  timeHorizon: number;
  predictions: ResourcePrediction[];
  confidence: number;
  trend: 'increasing' | 'decreasing' | 'stable';
  seasonalFactors: Map<string, number>;
}

export interface OptimizationResult {
  improvements: { area: string; currentValue: number; projectedValue: number; priority: number }[];
  totalProjectedGain: ResourceAmount;
  implementationCost: ResourceCost;
  timeToImplement: number;
  risk: 'low' | 'medium' | 'high';
}

export interface ResourceHeatmap {
  timestamp: number;
  resourceDensities: Map<string, Map<ResourceType, number>>;
  harvestEfficiency: Map<string, number>;
  contestedZones: { position: { x: number; y: number }; intensity: number }[];
}

export interface MarketTrend {
  resourceType: ResourceType;
  currentPrice: number;
  priceChange24h: number;
  volume24h: number;
  trend: 'bullish' | 'bearish' | 'neutral';
  support: number;
  resistance: number;
  prediction: number;
}

export interface ResourceBalancingAction {
  id: string;
  fromResource: ResourceType;
  toResource: ResourceType;
  amount: number;
  conversionRate: number;
  efficiency: number;
  estimatedTime: number;
}

export class AdvancedResourceAnalytics {
  private resourceManager: ResourceManager;
  private analysisWindow: number = 300000;
  private predictionModels: Map<string, any> = new Map();
  private seasonalPatterns: Map<string, number[]> = new Map();

  constructor(resourceManager: ResourceManager) {
    this.resourceManager = resourceManager;
    this.initializeModels();
  }

  private initializeModels(): void {
    Object.values(ResourceType).forEach(type => {
      this.predictionModels.set(type, {
        coefficients: [1, 0.5, 0.25, 0.125],
        bias: 0,
        lastUpdate: Date.now()
      });
      this.seasonalPatterns.set(type, []);
    });
  }

  public predictResources(timeAhead: number): ResourcePrediction[] {
    const predictions: ResourcePrediction[] = [];
    const currentResources = this.resourceManager.getAllResources();
    const incomeRate = this.resourceManager.getTotalIncomeRate();
    const currentTime = Date.now();

    for (let t = 0; t <= timeAhead; t += 10000) {
      const timestamp = currentTime + t;
      const predictedResources = this.calculatePredictedResources(
        currentResources,
        incomeRate,
        t
      );
      const confidence = this.calculatePredictionConfidence(t);

      predictions.push({
        timestamp,
        resources: predictedResources,
        confidence,
        factors: this.identifyResourceFactors(t)
      });
    }

    return predictions;
  }

  private calculatePredictedResources(
    current: ResourceAmount,
    incomeRate: ResourceAmount,
    timeDelta: number
  ): ResourceAmount {
    const seconds = timeDelta / 1000;
    
    return {
      minerals: current.minerals + incomeRate.minerals * seconds,
      gas: current.gas + incomeRate.gas * seconds,
      energy: current.energy + incomeRate.energy * seconds,
      credits: current.credits + incomeRate.credits * seconds,
      supply: current.supply + incomeRate.supply * seconds
    };
  }

  private calculatePredictionConfidence(timeDelta: number): number {
    const maxConfidence = 1.0;
    const decayRate = 0.00001;
    return Math.max(0.1, maxConfidence * Math.exp(-decayRate * timeDelta));
  }

  private identifyResourceFactors(timeDelta: number): string[] {
    const factors: string[] = [];
    
    if (timeDelta < 30000) {
      factors.push('Short-term harvester activity');
    } else if (timeDelta < 120000) {
      factors.push('Passive income sources');
    } else {
      factors.push('Market trends');
      factors.push('Alliance contributions');
    }

    const history = this.resourceManager.getHistory(10);
    if (history.length > 5) {
      factors.push('Historical patterns');
    }

    return factors;
  }

  public analyzeResourceTrends(): Map<ResourceType, { trend: number; volatility: number; pattern: string }> {
    const trends = new Map<ResourceType, { trend: number; volatility: number; pattern: string }>();
    const history = this.resourceManager.getHistory(30);

    for (const type of Object.values(ResourceType)) {
      const values = history.map(h => (h.resources as any)[type]);
      const trend = this.calculateTrend(values);
      const volatility = this.calculateVolatility(values);
      const pattern = this.identifyPattern(values);

      trends.set(type, { trend, volatility, pattern });
    }

    return trends;
  }

  private calculateTrend(values: number[]): number {
    if (values.length < 2) return 0;

    let sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
    
    for (let i = 0; i < values.length; i++) {
      sumX += i;
      sumY += values[i];
      sumXY += i * values[i];
      sumX2 += i * i;
    }

    const n = values.length;
    const slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
    
    return slope;
  }

  private calculateVolatility(values: number[]): number {
    if (values.length < 2) return 0;

    const mean = values.reduce((a, b) => a + b, 0) / values.length;
    const squaredDiffs = values.map(v => Math.pow(v - mean, 2));
    const variance = squaredDiffs.reduce((a, b) => a + b, 0) / values.length;
    
    return Math.sqrt(variance);
  }

  private identifyPattern(values: number[]): string {
    if (values.length < 5) return 'insufficient_data';

    const trend = this.calculateTrend(values);
    const volatility = this.calculateVolatility(values);
    const mean = values.reduce((a, b) => a + b, 0) / values.length;

    if (volatility / mean < 0.05) return 'stable';
    if (trend > 0.1) return 'growing';
    if (trend < -0.1) return 'declining';
    
    return 'fluctuating';
  }

  public findResourceBottlenecks(): ResourceConstraint[] {
    const constraints: ResourceConstraint[] = [];
    const resources = this.resourceManager.getAllResources();
    const warnings = this.resourceManager.getWarnings();

    for (const type of Object.values(ResourceType)) {
      const current = (resources as any)[type];
      const warning = warnings.get(type);
      
      if (warning && warning.level !== WarningLevel.None) {
        constraints.push({
          type,
          minAmount: current,
          maxAmount: (this.resourceManager as any).storage.totalCapacity[type],
          priority: warning.level === WarningLevel.Emergency ? 100 : 
                     warning.level === WarningLevel.Critical ? 75 : 50,
          reason: warning.message
        });
      }
    }

    return constraints.sort((a, b) => b.priority - a.priority);
  }

  public scheduleResourceTransaction(
    resourceType: ResourceType,
    amount: number,
    scheduledTime: number,
    isIncome: boolean,
    source?: string
  ): ResourceSchedulingItem {
    const id = `schedule_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    const item: ResourceSchedulingItem = {
      id,
      resourceType,
      amount,
      scheduledTime,
      isIncome,
      source,
      status: 'pending'
    };

    return item;
  }

  public optimizeResourceAllocation(): OptimizationResult {
    const improvements: { area: string; currentValue: number; projectedValue: number; priority: number }[] = [];
    const harvesters = this.resourceManager.getHarvesters();
    
    let optimalAssignmentGain = 0;
    for (const harvester of harvesters) {
      if (harvester.efficiency < 0.9) {
        optimalAssignmentGain += (1 - harvester.efficiency) * 10;
      }
    }

    if (optimalAssignmentGain > 0) {
      improvements.push({
        area: 'Harvester Efficiency',
        currentValue: 0,
        projectedValue: optimalAssignmentGain,
        priority: 80
      });
    }

    const warnings = Array.from(this.resourceManager.getWarnings().values());
    const criticalWarnings = warnings.filter(w => w.level === WarningLevel.Critical || w.level === WarningLevel.Emergency);
    
    if (criticalWarnings.length > 0) {
      improvements.push({
        area: 'Critical Resource Management',
        currentValue: criticalWarnings.length,
        projectedValue: 0,
        priority: 100
      });
    }

    const totalProjectedGain: ResourceAmount = {
      minerals: improvements.reduce((sum, i) => sum + (i.area.includes('Mineral') ? i.projectedValue : 0), 0),
      gas: improvements.reduce((sum, i) => sum + (i.area.includes('Gas') ? i.projectedValue : 0), 0),
      energy: improvements.reduce((sum, i) => sum + (i.area.includes('Energy') ? i.projectedValue : 0), 0),
      credits: improvements.reduce((sum, i) => sum + (i.area.includes('Credit') ? i.projectedValue : 0), 0),
      supply: improvements.reduce((sum, i) => sum + (i.area.includes('Supply') ? i.projectedValue : 0), 0)
    };

    return {
      improvements: improvements.sort((a, b) => b.priority - a.priority),
      totalProjectedGain,
      implementationCost: { minerals: 50, time: 10000 },
      timeToImplement: 30000,
      risk: improvements.length > 3 ? 'high' : improvements.length > 1 ? 'medium' : 'low'
    };
  }

  public generateResourceHeatmap(): ResourceHeatmap {
    const nodes = this.resourceManager.getResourceNodes();
    const resourceDensities = new Map<string, Map<ResourceType, number>>();
    const harvestEfficiency = new Map<string, number>();
    const contestedZones: { position: { x: number; y: number }; intensity: number }[] = [];

    const gridSize = 50;
    for (let x = 0; x < 1000; x += gridSize) {
      for (let y = 0; y < 1000; y += gridSize) {
        const gridId = `${x}_${y}`;
        const densities = new Map<ResourceType, number>();
        
        for (const type of Object.values(ResourceType)) {
          densities.set(type, 0);
        }

        for (const node of nodes) {
          const distance = Math.sqrt(
            Math.pow(node.position.x - x, 2) + Math.pow(node.position.y - y, 2)
          );
          
          if (distance < gridSize * 2) {
            const resourceType = this.getResourceTypeFromNodeType(node.type);
            const current = densities.get(resourceType) || 0;
            const density = node.currentAmount / Math.max(1, distance);
            densities.set(resourceType, current + density);
          }
        }

        resourceDensities.set(gridId, densities);
      }
    }

    const harvesters = this.resourceManager.getHarvesters();
    for (const harvester of harvesters) {
      harvestEfficiency.set(harvester.id, harvester.efficiency);
    }

    const contestedNodes = this.resourceManager.getContestedNodes();
    for (const node of contestedNodes) {
      contestedZones.push({
        position: node.position,
        intensity: node.contestedBy.length / 3
      });
    }

    return {
      timestamp: Date.now(),
      resourceDensities,
      harvestEfficiency,
      contestedZones
    };
  }

  private getResourceTypeFromNodeType(nodeType: ResourceNodeType): ResourceType {
    switch (nodeType) {
      case ResourceNodeType.MineralPatch: return ResourceType.Minerals;
      case ResourceNodeType.GasExtractor: return ResourceType.Gas;
      case ResourceNodeType.SolarPlant: return ResourceType.Energy;
      case ResourceNodeType.CreditVault: return ResourceType.Credits;
      case ResourceNodeType.SupplyDepot: return ResourceType.Supply;
      default: return ResourceType.Minerals;
    }
  }

  public analyzeMarketTrends(): MarketTrend[] {
    const marketStats = this.resourceManager.getMarketStats();
    const trends: MarketTrend[] = [];

    for (const type of Object.values(ResourceType)) {
      const currentPrice = marketStats.prices.get(type) || 10;
      const priceHistory = this.getPriceHistory(type);
      const volume = marketStats.volume.get(type) || 0;
      
      const priceChange24h = this.calculatePriceChange(priceHistory);
      const trend = this.determineTrend(priceChange24h);
      const support = this.calculateSupport(priceHistory);
      const resistance = this.calculateResistance(priceHistory);
      const prediction = this.predictPrice(type, priceHistory);

      trends.push({
        resourceType: type,
        currentPrice,
        priceChange24h,
        volume24h: volume,
        trend,
        support,
        resistance,
        prediction
      });
    }

    return trends;
  }

  private getPriceHistory(type: ResourceType): number[] {
    return [];
  }

  private calculatePriceChange(history: number[]): number {
    if (history.length < 2) return 0;
    
    const recent = history[history.length - 1];
    const older = history[0];
    
    return older > 0 ? ((recent - older) / older) * 100 : 0;
  }

  private determineTrend(change: number): 'bullish' | 'bearish' | 'neutral' {
    if (change > 5) return 'bullish';
    if (change < -5) return 'bearish';
    return 'neutral';
  }

  private calculateSupport(history: number[]): number {
    if (history.length === 0) return 0;
    
    const sorted = [...history].sort((a, b) => a - b);
    return sorted[Math.floor(sorted.length * 0.1)];
  }

  private calculateResistance(history: number[]): number {
    if (history.length === 0) return 0;
    
    const sorted = [...history].sort((a, b) => b - a);
    return sorted[Math.floor(sorted.length * 0.1)];
  }

  private predictPrice(type: ResourceType, history: number[]): number {
    if (history.length < 5) {
      return this.resourceManager.getMarketPrice(type);
    }

    const model = this.predictionModels.get(type);
    if (!model) return history[history.length - 1];

    let prediction = model.bias;
    for (let i = 0; i < Math.min(history.length, model.coefficients.length); i++) {
      prediction += model.coefficients[i] * history[history.length - 1 - i];
    }

    return Math.max(0, prediction);
  }

  public suggestBalancingActions(): ResourceBalancingAction[] {
    const actions: ResourceBalancingAction[] = [];
    const resources = this.resourceManager.getAllResources();
    const warnings = this.resourceManager.getWarnings();

    for (const lowType of Object.values(ResourceType)) {
      const lowWarning = warnings.get(lowType);
      if (lowWarning && lowWarning.level !== WarningLevel.None) {
        for (const highType of Object.values(ResourceType)) {
          if (lowType === highType) continue;
          
          const highAmount = (resources as any)[highType];
          const highCapacity = (this.resourceManager as any).storage.totalCapacity[highType];
          
          if (highAmount > highCapacity * 0.8) {
            const conversionRate = this.getConversionRate(highType, lowType);
            const maxConvert = Math.min(
              highAmount - highCapacity * 0.5,
              (highAmount * 0.5) / conversionRate
            );

            if (maxConvert > 0) {
              actions.push({
                id: `balance_${highType}_${lowType}`,
                fromResource: highType,
                toResource: lowType,
                amount: maxConvert,
                conversionRate,
                efficiency: 0.9,
                estimatedTime: 5000
              });
            }
          }
        }
      }
    }

    return actions.sort((a, b) => b.amount - a.amount);
  }

  private getConversionRate(from: ResourceType, to: ResourceType): number {
    const rates: Record<string, number> = {
      'minerals_credits': 0.8,
      'gas_energy': 0.7,
      'credits_minerals': 1.2,
      'energy_gas': 1.3
    };

    const key = `${from}_${to}`;
    return rates[key] || 1.0;
  }

  public calculateResourceHealth(): {
    score: number;
    factors: { name: string; score: number; weight: number }[];
    recommendations: string[]
  } {
    const factors: { name: string; score: number; weight: number }[] = [];
    let weightedSum = 0;
    let totalWeight = 0;

    const resources = this.resourceManager.getAllResources();
    const incomeRate = this.resourceManager.getTotalIncomeRate();
    const warnings = this.resourceManager.getWarnings();
    const storage = this.resourceManager.getStatistics();

    const resourceFactor = this.calculateResourceFactor(resources);
    factors.push({ name: 'Resource Balance', score: resourceFactor, weight: 0.3 });
    weightedSum += resourceFactor * 0.3;
    totalWeight += 0.3;

    const incomeFactor = this.calculateIncomeFactor(incomeRate);
    factors.push({ name: 'Income Stability', score: incomeFactor, weight: 0.25 });
    weightedSum += incomeFactor * 0.25;
    totalWeight += 0.25;

    const warningFactor = this.calculateWarningFactor(warnings);
    factors.push({ name: 'Warning Status', score: warningFactor, weight: 0.2 });
    weightedSum += warningFactor * 0.2;
    totalWeight += 0.2;

    const efficiencyFactor = storage.efficiency;
    factors.push({ name: 'Harvester Efficiency', score: efficiencyFactor * 100, weight: 0.15 });
    weightedSum += efficiencyFactor * 100 * 0.15;
    totalWeight += 0.15;

    const utilizationFactor = storage.utilizationRate * 100;
    factors.push({ name: 'Resource Utilization', score: utilizationFactor, weight: 0.1 });
    weightedSum += utilizationFactor * 0.1;
    totalWeight += 0.1;

    const score = totalWeight > 0 ? weightedSum / totalWeight : 0;

    const recommendations: string[] = [];
    if (warningFactor < 50) {
      recommendations.push('Address critical resource warnings immediately');
    }
    if (efficiencyFactor < 0.7) {
      recommendations.push('Optimize harvester assignments');
    }
    if (utilizationFactor < 50) {
      recommendations.push('Increase resource node utilization');
    }

    return { score, factors, recommendations };
  }

  private calculateResourceFactor(resources: ResourceAmount): number {
    const capacities = (this.resourceManager as any).storage.totalCapacity;
    
    let totalFactor = 0;
    let count = 0;

    for (const type of Object.values(ResourceType)) {
      const current = (resources as any)[type];
      const capacity = (capacities as any)[type];
      const ratio = capacity > 0 ? current / capacity : 0;
      
      if (ratio < 0.2) {
        return 0;
      }
      
      totalFactor += Math.min(100, ratio * 100);
      count++;
    }

    return count > 0 ? totalFactor / count : 0;
  }

  private calculateIncomeFactor(incomeRate: ResourceAmount): number {
    let totalIncome = 0;
    let count = 0;

    for (const type of Object.values(ResourceType)) {
      const rate = (incomeRate as any)[type];
      if (rate > 0) {
        totalIncome += Math.min(100, rate * 10);
        count++;
      }
    }

    return count > 0 ? totalIncome / count : 0;
  }

  private calculateWarningFactor(warnings: Map<ResourceType, ResourceWarning>): number {
    if (warnings.size === 0) return 100;

    let totalPenalty = 0;
    
    for (const warning of warnings.values()) {
      switch (warning.level) {
        case WarningLevel.Emergency:
          totalPenalty += 50;
          break;
        case WarningLevel.Critical:
          totalPenalty += 25;
          break;
        case WarningLevel.Low:
          totalPenalty += 10;
          break;
      }
    }

    return Math.max(0, 100 - totalPenalty);
  }

  public forecastResourceNeeds(duration: number): ResourceAmount {
    const pendingSpending = (this.resourceManager as any).spending.pendingSpending;
    const budgets = this.resourceManager.getBudgets();
    
    const estimatedNeeds: ResourceAmount = {
      minerals: 0,
      gas: 0,
      energy: 0,
      credits: 0,
      supply: 0
    };

    for (const record of pendingSpending) {
      if (record.cost.minerals) estimatedNeeds.minerals += record.cost.minerals;
      if (record.cost.gas) estimatedNeeds.gas += record.cost.gas;
      if (record.cost.energy) estimatedNeeds.energy += record.cost.energy;
      if (record.cost.credits) estimatedNeeds.credits += record.cost.credits;
      if (record.cost.supply) estimatedNeeds.supply += record.cost.supply;
    }

    for (const budget of budgets) {
      if (!budget.isActive) continue;

      const remaining = budget.remainingBudget;
      estimatedNeeds.minerals += remaining.minerals;
      estimatedNeeds.gas += remaining.gas;
      estimatedNeeds.energy += remaining.energy;
      estimatedNeeds.credits += remaining.credits;
      estimatedNeeds.supply += remaining.supply;
    }

    const incomeRate = this.resourceManager.getTotalIncomeRate();
    const durationSeconds = duration / 1000;

    const projectedIncome: ResourceAmount = {
      minerals: incomeRate.minerals * durationSeconds,
      gas: incomeRate.gas * durationSeconds,
      energy: incomeRate.energy * durationSeconds,
      credits: incomeRate.credits * durationSeconds,
      supply: incomeRate.supply * durationSeconds
    };

    return {
      minerals: Math.max(0, estimatedNeeds.minerals - projectedIncome.minerals),
      gas: Math.max(0, estimatedNeeds.gas - projectedIncome.gas),
      energy: Math.max(0, estimatedNeeds.energy - projectedIncome.energy),
      credits: Math.max(0, estimatedNeeds.credits - projectedIncome.credits),
      supply: Math.max(0, estimatedNeeds.supply - projectedIncome.supply)
    };
  }

  public identifyOpportunities(): {
    type: string;
    description: string;
    potentialGain: ResourceAmount;
    effort: 'low' | 'medium' | 'high';
    timeEstimate: number;
  }[] {
    const opportunities: {
      type: string;
      description: string;
      potentialGain: ResourceAmount;
      effort: 'low' | 'medium' | 'high';
      timeEstimate: number;
    }[] = [];

    const marketTrends = this.analyzeMarketTrends();
    for (const trend of marketTrends) {
      if (trend.trend === 'bullish' && trend.volume24h > 100) {
        opportunities.push({
          type: 'Market Trading',
          description: `Increase ${trend.resourceType} production due to rising prices`,
          potentialGain: { minerals: 0, gas: 0, energy: 0, credits: trend.currentPrice * 50, supply: 0 },
          effort: 'medium',
          timeEstimate: 30000
        });
      }
    }

    const balancingActions = this.suggestBalancingActions();
    for (const action of balancingActions.slice(0, 2)) {
      opportunities.push({
        type: 'Resource Conversion',
        description: `Convert excess ${action.fromResource} to ${action.toResource}`,
        potentialGain: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
        effort: 'low',
        timeEstimate: action.estimatedTime
      });
    }

    const harvesters = this.resourceManager.getHarvesters();
    const idleHarvesters = harvesters.filter(h => h.state === 'idle');
    
    if (idleHarvesters.length > 0) {
      opportunities.push({
        type: 'Deploy Idle Harvesters',
        description: `Assign ${idleHarvesters.length} idle harvesters to resource nodes`,
        potentialGain: { minerals: idleHarvesters.length * 100, gas: 0, energy: 0, credits: 0, supply: 0 },
        effort: 'low',
        timeEstimate: 5000
      });
    }

    const efficiency = this.resourceManager.getStatistics().efficiency;
    if (efficiency < 0.8) {
      opportunities.push({
        type: 'Optimize Assignments',
        description: 'Reoptimize harvester assignments for better efficiency',
        potentialGain: { minerals: 50, gas: 20, energy: 0, credits: 0, supply: 0 },
        effort: 'low',
        timeEstimate: 2000
      });
    }

    return opportunities;
  }
}

export class ResourceTransactionManager {
  private transactions: ResourceTransaction[] = [];
  private maxTransactions: number = 5000;
  private listeners: Map<string, Function[]> = new Map();

  public recordTransaction(
    type: 'income' | 'expense' | 'transfer' | 'trade',
    resourceType: ResourceType,
    amount: number,
    balance: number,
    description: string,
    relatedEntity?: string
  ): ResourceTransaction {
    const transaction: ResourceTransaction = {
      id: `txn_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      timestamp: Date.now(),
      type,
      resourceType,
      amount,
      balance,
      description,
      relatedEntity
    };

    this.transactions.push(transaction);

    if (this.transactions.length > this.maxTransactions) {
      this.transactions.shift();
    }

    this.emit('transactionRecorded', transaction);

    return transaction;
  }

  public getTransactions(filter?: {
    type?: 'income' | 'expense' | 'transfer' | 'trade';
    resourceType?: ResourceType;
    startTime?: number;
    endTime?: number;
    limit?: number;
  }): ResourceTransaction[] {
    let result = [...this.transactions];

    if (filter) {
      if (filter.type) {
        result = result.filter(t => t.type === filter.type);
      }
      if (filter.resourceType) {
        result = result.filter(t => t.resourceType === filter.resourceType);
      }
      if (filter.startTime) {
        result = result.filter(t => t.timestamp >= filter.startTime!);
      }
      if (filter.endTime) {
        result = result.filter(t => t.timestamp <= filter.endTime!);
      }
    }

    if (filter?.limit) {
      result = result.slice(-filter.limit);
    }

    return result;
  }

  public getTransactionStats(): {
    totalIncome: ResourceAmount;
    totalExpenses: ResourceAmount;
    netFlow: ResourceAmount;
    transactionCount: number;
    averageTransactionSize: number;
  } {
    const stats = {
      totalIncome: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      totalExpenses: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      netFlow: { minerals: 0, gas: 0, energy: 0, credits: 0, supply: 0 },
      transactionCount: this.transactions.length,
      averageTransactionSize: 0
    };

    for (const txn of this.transactions) {
      const amount = txn.amount;
      
      if (txn.type === 'income' || txn.type === 'trade') {
        (stats.totalIncome as any)[txn.resourceType] += amount;
      } else if (txn.type === 'expense') {
        (stats.totalExpenses as any)[txn.resourceType] += amount;
      }
    }

    for (const type of Object.values(ResourceType)) {
      (stats.netFlow as any)[type] = (stats.totalIncome as any)[type] - (stats.totalExpenses as any)[type];
    }

    const totalAmount = this.transactions.reduce((sum, t) => sum + t.amount, 0);
    stats.averageTransactionSize = this.transactions.length > 0 ? totalAmount / this.transactions.length : 0;

    return stats;
  }

  public on(event: string, listener: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(listener);
  }

  private emit(event: string, data: any): void {
    const listeners = this.listeners.get(event);
    if (listeners) {
      listeners.forEach(listener => listener(data));
    }
  }
}

export class ResourceAlertSystem {
  private alerts: ResourceAlert[] = [];
  private thresholds: Map<ResourceType, {
    opportunity?: number;
    warning?: number;
    critical?: number;
  }> = new Map();

  constructor() {
    this.initializeThresholds();
  }

  private initializeThresholds(): void {
    this.thresholds.set(ResourceType.Minerals, {
      opportunity: 8000,
      warning: 200,
      critical: 50
    });
    this.thresholds.set(ResourceType.Gas, {
      opportunity: 4000,
      warning: 100,
      critical: 25
    });
    this.thresholds.set(ResourceType.Energy, {
      opportunity: 8000,
      warning: 200,
      critical: 50
    });
    this.thresholds.set(ResourceType.Credits, {
      opportunity: 8000,
      warning: 150,
      critical: 30
    });
    this.thresholds.set(ResourceType.Supply, {
      opportunity: 180,
      warning: 30,
      critical: 10
    });
  }

  public checkResources(current: ResourceAmount): ResourceAlert[] {
    const newAlerts: ResourceAlert[] = [];

    for (const type of Object.values(ResourceType)) {
      const amount = (current as any)[type];
      const threshold = this.thresholds.get(type);
      
      if (!threshold) continue;

      if (threshold.critical && amount <= threshold.critical) {
        newAlerts.push(this.createAlert(type, amount, threshold.critical, 'critical'));
      } else if (threshold.warning && amount <= threshold.warning) {
        newAlerts.push(this.createAlert(type, amount, threshold.warning, 'warning'));
      } else if (threshold.opportunity && amount >= threshold.opportunity) {
        newAlerts.push(this.createAlert(type, amount, threshold.opportunity, 'opportunity'));
      }
    }

    this.alerts = this.alerts.filter(a => !a.acknowledged || Date.now() - a.timestamp < 60000);
    this.alerts.push(...newAlerts);

    return newAlerts;
  }

  private createAlert(
    type: ResourceType,
    currentValue: number,
    threshold: number,
    alertType: 'warning' | 'critical' | 'opportunity'
  ): ResourceAlert {
    const messages: Record<string, string> = {
      critical: `CRITICAL: ${type} resources are dangerously low`,
      warning: `Warning: ${type} resources are running low`,
      opportunity: `Opportunity: ${type} resources are abundant`
    };

    const actions: Record<string, string> = {
      critical: 'Immediately allocate resources to increase production',
      warning: 'Consider expanding resource gathering operations',
      opportunity: 'Good time to invest in expensive upgrades or units'
    };

    return {
      id: `alert_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      type: alertType,
      resourceType: type,
      currentValue,
      threshold,
      message: messages[alertType],
      recommendedAction: actions[alertType],
      timestamp: Date.now(),
      acknowledged: false
    };
  }

  public acknowledgeAlert(alertId: string): void {
    const alert = this.alerts.find(a => a.id === alertId);
    if (alert) {
      alert.acknowledged = true;
    }
  }

  public getActiveAlerts(): ResourceAlert[] {
    return this.alerts.filter(a => !a.acknowledged);
  }

  public setThreshold(type: ResourceType, level: 'opportunity' | 'warning' | 'critical', value: number): void {
    const current = this.thresholds.get(type) || {};
    current[level] = value;
    this.thresholds.set(type, current);
  }
}

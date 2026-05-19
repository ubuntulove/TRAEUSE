export interface ResourceType {
  name: string;
  icon: string;
  color: string;
  maxCapacity: number;
  startingAmount: number;
  regenerationRate: number;
  description: string;
}

export interface ResourceNode {
  id: string;
  type: string;
  position: { x: number; y: number };
  amount: number;
  maxAmount: number;
  depletionRate: number;
  isDepleted: boolean;
  respawnTime: number;
  respawnTimer: number;
}

export interface PlayerResources {
  credits: number;
  power: number;
  ore: number;
  gems: number;
  energy: number;
}

export class ResourceManager {
  private resources: Map<string, number> = new Map();
  private maxResources: Map<string, number> = new Map();
  private resourceTypes: Map<string, ResourceType> = new Map();
  private resourceNodes: Map<string, ResourceNode> = new Map();
  private incomeRates: Map<string, number> = new Map();
  private consumptionRates: Map<string, number> = new Map();
  private listeners: Map<string, Array<(amount: number) => void>> = new Map();

  constructor() {
    this.initializeResourceTypes();
  }

  private initializeResourceTypes(): void {
    this.registerResourceType({
      name: 'credits',
      icon: '💰',
      color: '#ffd700',
      maxCapacity: 100000,
      startingAmount: 1000,
      regenerationRate: 0,
      description: 'Standard currency for purchasing units and buildings',
    });

    this.registerResourceType({
      name: 'power',
      icon: '⚡',
      color: '#00bfff',
      maxCapacity: 10000,
      startingAmount: 100,
      regenerationRate: 0,
      description: 'Required to operate buildings and units',
    });

    this.registerResourceType({
      name: 'ore',
      icon: '💎',
      color: '#888888',
      maxCapacity: 50000,
      startingAmount: 500,
      regenerationRate: 0,
      description: 'Raw materials for advanced construction',
    });

    this.registerResourceType({
      name: 'gems',
      icon: '🔮',
      color: '#ff00ff',
      maxCapacity: 10000,
      startingAmount: 0,
      regenerationRate: 0,
      description: 'Rare crystals used for advanced technologies',
    });

    this.registerResourceType({
      name: 'energy',
      icon: '✨',
      color: '#ffff00',
      maxCapacity: 5000,
      startingAmount: 0,
      regenerationRate: 0,
      description: 'Special energy for experimental weapons',
    });
  }

  public registerResourceType(type: ResourceType): void {
    this.resourceTypes.set(type.name, type);
    this.resources.set(type.name, type.startingAmount);
    this.maxResources.set(type.name, type.maxCapacity);
  }

  public addResource(type: string, amount: number): boolean {
    const current = this.resources.get(type) || 0;
    const max = this.maxResources.get(type) || Infinity;

    if (amount > 0 && current >= max) {
      return false;
    }

    const newAmount = Math.max(0, Math.min(max, current + amount));
    this.resources.set(type, newAmount);

    this.notifyListeners(type, newAmount);

    return true;
  }

  public removeResource(type: string, amount: number): boolean {
    const current = this.resources.get(type) || 0;

    if (amount > current) {
      return false;
    }

    const newAmount = current - amount;
    this.resources.set(type, newAmount);

    this.notifyListeners(type, newAmount);

    return true;
  }

  public getResource(type: string): number {
    return this.resources.get(type) || 0;
  }

  public getMaxResource(type: string): number {
    return this.maxResources.get(type) || 0;
  }

  public getAllResources(): PlayerResources {
    return {
      credits: this.getResource('credits'),
      power: this.getResource('power'),
      ore: this.getResource('ore'),
      gems: this.getResource('gems'),
      energy: this.getResource('energy'),
    };
  }

  public setResource(type: string, amount: number): void {
    const max = this.maxResources.get(type) || Infinity;
    this.resources.set(type, Math.max(0, Math.min(max, amount)));
    this.notifyListeners(type, this.resources.get(type)!);
  }

  public setMaxResource(type: string, max: number): void {
    this.maxResources.set(type, max);
    const current = this.resources.get(type) || 0;
    if (current > max) {
      this.resources.set(type, max);
    }
  }

  public canAfford(cost: Partial<PlayerResources>): boolean {
    if (cost.credits !== undefined && this.getResource('credits') < cost.credits) {
      return false;
    }
    if (cost.power !== undefined && this.getResource('power') < cost.power) {
      return false;
    }
    if (cost.ore !== undefined && this.getResource('ore') < cost.ore) {
      return false;
    }
    if (cost.gems !== undefined && this.getResource('gems') < cost.gems) {
      return false;
    }
    if (cost.energy !== undefined && this.getResource('energy') < cost.energy) {
      return false;
    }
    return true;
  }

  public spend(cost: Partial<PlayerResources>): boolean {
    if (!this.canAfford(cost)) {
      return false;
    }

    if (cost.credits !== undefined) {
      this.removeResource('credits', cost.credits);
    }
    if (cost.power !== undefined) {
      this.removeResource('power', cost.power);
    }
    if (cost.ore !== undefined) {
      this.removeResource('ore', cost.ore);
    }
    if (cost.gems !== undefined) {
      this.removeResource('gems', cost.gems);
    }
    if (cost.energy !== undefined) {
      this.removeResource('energy', cost.energy);
    }

    return true;
  }

  public refund(percentage: number, cost: Partial<PlayerResources>): void {
    if (cost.credits !== undefined) {
      this.addResource('credits', Math.floor(cost.credits * percentage));
    }
    if (cost.power !== undefined) {
      this.addResource('power', Math.floor(cost.power * percentage));
    }
    if (cost.ore !== undefined) {
      this.addResource('ore', Math.floor(cost.ore * percentage));
    }
    if (cost.gems !== undefined) {
      this.addResource('gems', Math.floor(cost.gems * percentage));
    }
    if (cost.energy !== undefined) {
      this.addResource('energy', Math.floor(cost.energy * percentage));
    }
  }

  public setIncomeRate(type: string, rate: number): void {
    this.incomeRates.set(type, rate);
  }

  public setConsumptionRate(type: string, rate: number): void {
    this.consumptionRates.set(type, rate);
  }

  public getNetRate(type: string): number {
    const income = this.incomeRates.get(type) || 0;
    const consumption = this.consumptionRates.get(type) || 0;
    return income - consumption;
  }

  public update(deltaTime: number): void {
    for (const [type, rate] of this.incomeRates) {
      if (rate > 0) {
        this.addResource(type, rate * deltaTime);
      }
    }

    for (const [type, rate] of this.consumptionRates) {
      if (rate > 0) {
        this.removeResource(type, rate * deltaTime);
      }
    }

    for (const node of this.resourceNodes.values()) {
      if (!node.isDepleted) {
        node.amount -= node.depletionRate * deltaTime;

        if (node.amount <= 0) {
          node.isDepleted = true;
          node.amount = 0;
          node.respawnTimer = node.respawnTime;
        }
      } else {
        node.respawnTimer -= deltaTime;

        if (node.respawnTimer <= 0) {
          node.isDepleted = false;
          node.amount = node.maxAmount;
        }
      }
    }
  }

  public addResourceNode(node: ResourceNode): void {
    this.resourceNodes.set(node.id, node);
  }

  public removeResourceNode(id: string): void {
    this.resourceNodes.delete(id);
  }

  public getResourceNode(id: string): ResourceNode | undefined {
    return this.resourceNodes.get(id);
  }

  public getAllResourceNodes(): ResourceNode[] {
    return Array.from(this.resourceNodes.values());
  }

  public harvest(nodeId: string, amount: number): number {
    const node = this.resourceNodes.get(nodeId);
    if (!node || node.isDepleted) {
      return 0;
    }

    const harvested = Math.min(node.amount, amount);
    node.amount -= harvested;

    return harvested;
  }

  public on(type: string, callback: (amount: number) => void): void {
    if (!this.listeners.has(type)) {
      this.listeners.set(type, []);
    }
    this.listeners.get(type)!.push(callback);
  }

  public off(type: string, callback: (amount: number) => void): void {
    const callbacks = this.listeners.get(type);
    if (callbacks) {
      const index = callbacks.indexOf(callback);
      if (index !== -1) {
        callbacks.splice(index, 1);
      }
    }
  }

  private notifyListeners(type: string, amount: number): void {
    const callbacks = this.listeners.get(type);
    if (callbacks) {
      for (const callback of callbacks) {
        callback(amount);
      }
    }
  }

  public clear(): void {
    this.resources.clear();
    this.incomeRates.clear();
    this.consumptionRates.clear();
    this.resourceNodes.clear();
  }

  public reset(): void {
    for (const type of this.resourceTypes.values()) {
      this.resources.set(type.name, type.startingAmount);
    }
    this.incomeRates.clear();
    this.consumptionRates.clear();
  }

  public toJSON(): object {
    return {
      resources: Object.fromEntries(this.resources),
      maxResources: Object.fromEntries(this.maxResources),
      incomeRates: Object.fromEntries(this.incomeRates),
      consumptionRates: Object.fromEntries(this.consumptionRates),
    };
  }

  public fromJSON(data: any): void {
    if (data.resources) {
      for (const [type, amount] of Object.entries(data.resources)) {
        this.resources.set(type, amount as number);
      }
    }

    if (data.maxResources) {
      for (const [type, max] of Object.entries(data.maxResources)) {
        this.maxResources.set(type, max as number);
      }
    }

    if (data.incomeRates) {
      for (const [type, rate] of Object.entries(data.incomeRates)) {
        this.incomeRates.set(type, rate as number);
      }
    }

    if (data.consumptionRates) {
      for (const [type, rate] of Object.entries(data.consumptionRates)) {
        this.consumptionRates.set(type, rate as number);
      }
    }
  }
}

export class EconomySimulator {
  private resourceManager: ResourceManager;
  private buildings: Map<string, { powerProduction: number; powerConsumption: number; resourceProduction: number }> = new Map();
  private units: Map<string, { powerConsumption: number }> = new Map();

  constructor(resourceManager: ResourceManager) {
    this.resourceManager = resourceManager;
  }

  public addBuilding(id: string, data: { powerProduction: number; powerConsumption: number; resourceProduction: number }): void {
    this.buildings.set(id, data);
    this.updateRates();
  }

  public removeBuilding(id: string): void {
    this.buildings.delete(id);
    this.updateRates();
  }

  public addUnit(id: string, data: { powerConsumption: number }): void {
    this.units.set(id, data);
    this.updateRates();
  }

  public removeUnit(id: string): void {
    this.units.delete(id);
    this.updateRates();
  }

  private updateRates(): void {
    let totalPowerProduction = 0;
    let totalPowerConsumption = 0;
    let totalResourceProduction = 0;

    for (const building of this.buildings.values()) {
      totalPowerProduction += building.powerProduction;
      totalPowerConsumption += building.powerConsumption;
      totalResourceProduction += building.resourceProduction;
    }

    for (const unit of this.units.values()) {
      totalPowerConsumption += unit.powerConsumption;
    }

    this.resourceManager.setIncomeRate('power', totalPowerProduction);
    this.resourceManager.setConsumptionRate('power', totalPowerConsumption);
    this.resourceManager.setIncomeRate('ore', totalResourceProduction);
  }

  public getPowerBalance(): number {
    let production = 0;
    let consumption = 0;

    for (const building of this.buildings.values()) {
      production += building.powerProduction;
      consumption += building.powerConsumption;
    }

    for (const unit of this.units.values()) {
      consumption += unit.powerConsumption;
    }

    return production - consumption;
  }

  public isPowered(): boolean {
    return this.getPowerBalance() >= 0;
  }
}

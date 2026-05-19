import { Entity, Vector2D } from '../../entities/Entity';

export interface BuildingStats {
  health: number;
  maxHealth: number;
  armor: number;
  attackDamage: number;
  attackRange: number;
  attackSpeed: number;
  sightRange: number;
  cost: { credits: number; power: number; ore: number };
  buildTime: number;
  size: { width: number; height: number };
  powerProduction: number;
  powerConsumption: number;
  resourceProduction: number;
}

export interface BuildingUpgrade {
  id: string;
  name: string;
  description: string;
  cost: { credits: number; power: number; ore: number };
  time: number;
  statBonuses: Partial<BuildingStats>;
  requirements: string[];
}

export class Building extends Entity {
  public buildingType: string;
  public stats: BuildingStats;
  public isConstructed: boolean;
  public constructionProgress: number;
  public isPowered: boolean;
  public isActive: boolean;
  public productionQueue: any[];
  public productionProgress: number;
  public upgrades: Map<string, BuildingUpgrade>;
  public assignedWorkers: string[];
  public rallyPoint: Vector2D | null;
  public mode: 'idle' | 'producing' | 'attacking' | 'disabled';

  constructor(config: {
    id: string;
    position: Vector2D;
    type: string;
    team: number;
    stats: BuildingStats;
  }) {
    super({
      id: config.id,
      position: config.position,
      health: config.stats.health,
      maxHealth: config.stats.maxHealth,
      damage: config.stats.attackDamage,
      armor: config.stats.armor,
      team: config.team,
      type: 'building',
    });

    this.buildingType = config.type;
    this.stats = config.stats;
    this.isConstructed = config.stats.buildTime === 0;
    this.constructionProgress = this.isConstructed ? 1 : 0;
    this.isPowered = true;
    this.isActive = true;
    this.productionQueue = [];
    this.productionProgress = 0;
    this.upgrades = new Map();
    this.assignedWorkers = [];
    this.rallyPoint = null;
    this.mode = 'idle';
  }

  public update(deltaTime: number): void {
    super.update(deltaTime);

    if (!this.isConstructed) {
      this.constructionProgress += deltaTime / this.stats.buildTime;
      if (this.constructionProgress >= 1) {
        this.constructionProgress = 1;
        this.isConstructed = true;
      }
    }

    if (this.productionQueue.length > 0) {
      const currentProduction = this.productionQueue[0];
      this.productionProgress += deltaTime / currentProduction.time;

      if (this.productionProgress >= 1) {
        this.completeProduction();
      }
    }

    this.updatePower();
    this.updateMode();
  }

  private updatePower(): void {
    this.isPowered = this.stats.powerProduction >= this.stats.powerConsumption;
  }

  private updateMode(): void {
    if (!this.isConstructed || !this.isPowered) {
      this.mode = 'disabled';
    } else if (this.productionQueue.length > 0) {
      this.mode = 'producing';
    } else {
      this.mode = 'idle';
    }
  }

  public queueProduction(item: { type: string; time: number; cost: any }): boolean {
    if (this.productionQueue.length >= 5) {
      return false;
    }

    this.productionQueue.push(item);
    return true;
  }

  public cancelProduction(index: number): void {
    if (index >= 0 && index < this.productionQueue.length) {
      this.productionQueue.splice(index, 1);
      this.productionProgress = 0;
    }
  }

  public completeProduction(): any {
    if (this.productionQueue.length === 0) return null;

    const item = this.productionQueue.shift();
    this.productionProgress = 0;

    return item;
  }

  public addUpgrade(upgrade: BuildingUpgrade): void {
    this.upgrades.set(upgrade.id, upgrade);

    if (upgrade.statBonuses.health) {
      const bonus = upgrade.statBonuses.health - this.stats.maxHealth;
      this.stats.maxHealth = upgrade.statBonuses.health;
      this.health += bonus;
    }

    if (upgrade.statBonuses.attackDamage) {
      this.stats.attackDamage = upgrade.statBonuses.attackDamage;
    }

    if (upgrade.statBonuses.armor) {
      this.stats.armor = upgrade.statBonuses.armor;
    }
  }

  public hasUpgrade(upgradeId: string): boolean {
    return this.upgrades.has(upgradeId);
  }

  public getUpgrade(upgradeId: string): BuildingUpgrade | undefined {
    return this.upgrades.get(upgradeId);
  }

  public setRallyPoint(point: Vector2D | null): void {
    this.rallyPoint = point;
  }

  public assignWorker(workerId: string): void {
    if (!this.assignedWorkers.includes(workerId)) {
      this.assignedWorkers.push(workerId);
    }
  }

  public unassignWorker(workerId: string): void {
    const index = this.assignedWorkers.indexOf(workerId);
    if (index !== -1) {
      this.assignedWorkers.splice(index, 1);
    }
  }

  public canAttack(): boolean {
    return this.stats.attackDamage > 0 && this.isConstructed && this.isPowered;
  }

  public getAttackCooldown(): number {
    return 1 / this.stats.attackSpeed;
  }

  public getProductionCost(): { credits: number; power: number; ore: number } {
    if (this.productionQueue.length === 0) {
      return { credits: 0, power: 0, ore: 0 };
    }

    return this.productionQueue[0].cost;
  }

  public repair(amount: number): void {
    this.health = Math.min(this.maxHealth, this.health + amount);
  }

  public sell(percentage: number = 0.75): { credits: number; power: number; ore: number } {
    return {
      credits: Math.floor(this.stats.cost.credits * percentage),
      power: 0,
      ore: Math.floor(this.stats.cost.ore * percentage),
    };
  }
}

export class BuildingManager {
  private buildings: Map<string, Building> = new Map();
  private buildingsByType: Map<string, Set<string>> = new Map();
  private buildingsByTeam: Map<number, Set<string>> = new Map();

  public add(building: Building): void {
    this.buildings.set(building.id, building);

    if (!this.buildingsByType.has(building.buildingType)) {
      this.buildingsByType.set(building.buildingType, new Set());
    }
    this.buildingsByType.get(building.buildingType)!.add(building.id);

    if (!this.buildingsByTeam.has(building.team)) {
      this.buildingsByTeam.set(building.team, new Set());
    }
    this.buildingsByTeam.get(building.team)!.add(building.id);
  }

  public remove(id: string): void {
    const building = this.buildings.get(id);
    if (!building) return;

    this.buildingsByType.get(building.buildingType)?.delete(id);
    this.buildingsByTeam.get(building.team)?.delete(id);
    this.buildings.delete(id);
  }

  public get(id: string): Building | undefined {
    return this.buildings.get(id);
  }

  public getAll(): Building[] {
    return Array.from(this.buildings.values());
  }

  public getByType(type: string): Building[] {
    const ids = this.buildingsByType.get(type);
    if (!ids) return [];
    return Array.from(ids).map(id => this.buildings.get(id)).filter((b): b is Building => b !== undefined);
  }

  public getByTeam(team: number): Building[] {
    const ids = this.buildingsByTeam.get(team);
    if (!ids) return [];
    return Array.from(ids).map(id => this.buildings.get(id)).filter((b): b is Building => b !== undefined);
  }

  public getConstructed(): Building[] {
    return this.getAll().filter(b => b.isConstructed);
  }

  public getUnconstructed(): Building[] {
    return this.getAll().filter(b => !b.isConstructed);
  }

  public getPowered(): Building[] {
    return this.getAll().filter(b => b.isPowered);
  }

  public getUnpowered(): Building[] {
    return this.getAll().filter(b => !b.isPowered);
  }

  public update(deltaTime: number): void {
    for (const building of this.buildings.values()) {
      if (building.active) {
        building.update(deltaTime);
      }
    }
  }

  public getTotalPowerProduction(team: number): number {
    return this.getByTeam(team)
      .filter(b => b.isConstructed && b.isPowered)
      .reduce((sum, b) => sum + b.stats.powerProduction, 0);
  }

  public getTotalPowerConsumption(team: number): number {
    return this.getByTeam(team)
      .filter(b => b.isConstructed)
      .reduce((sum, b) => sum + b.stats.powerConsumption, 0);
  }

  public clear(): void {
    this.buildings.clear();
    this.buildingsByType.clear();
    this.buildingsByTeam.clear();
  }

  public size(): number {
    return this.buildings.size;
  }
}

export class BuildingFactory {
  private static buildingTemplates: Map<string, Partial<BuildingStats>> = new Map();

  static {
    this.buildingTemplates.set('command-center', {
      health: 2000,
      maxHealth: 2000,
      armor: 20,
      attackDamage: 0,
      attackRange: 0,
      attackSpeed: 0,
      sightRange: 500,
      cost: { credits: 0, power: 0, ore: 0 },
      buildTime: 0,
      size: { width: 160, height: 160 },
      powerProduction: 100,
      powerConsumption: 0,
      resourceProduction: 0,
    });

    this.buildingTemplates.set('barracks', {
      health: 1500,
      maxHealth: 1500,
      armor: 15,
      attackDamage: 0,
      attackRange: 0,
      attackSpeed: 0,
      sightRange: 300,
      cost: { credits: 400, power: 0, ore: 0 },
      buildTime: 20,
      size: { width: 120, height: 120 },
      powerProduction: 0,
      powerConsumption: 10,
      resourceProduction: 0,
    });

    this.buildingTemplates.set('factory', {
      health: 2000,
      maxHealth: 2000,
      armor: 18,
      attackDamage: 0,
      attackRange: 0,
      attackSpeed: 0,
      sightRange: 300,
      cost: { credits: 800, power: 0, ore: 0 },
      buildTime: 30,
      size: { width: 140, height: 140 },
      powerProduction: 0,
      powerConsumption: 20,
      resourceProduction: 0,
    });

    this.buildingTemplates.set('power-plant', {
      health: 800,
      maxHealth: 800,
      armor: 10,
      attackDamage: 0,
      attackRange: 0,
      attackSpeed: 0,
      sightRange: 150,
      cost: { credits: 300, power: 0, ore: 0 },
      buildTime: 15,
      size: { width: 80, height: 80 },
      powerProduction: 200,
      powerConsumption: 0,
      resourceProduction: 0,
    });

    this.buildingTemplates.set('refinery', {
      health: 1200,
      maxHealth: 1200,
      armor: 12,
      attackDamage: 0,
      attackRange: 0,
      attackSpeed: 0,
      sightRange: 200,
      cost: { credits: 500, power: 0, ore: 0 },
      buildTime: 25,
      size: { width: 100, height: 100 },
      powerProduction: 0,
      powerConsumption: 15,
      resourceProduction: 50,
    });

    this.buildingTemplates.set('turret', {
      health: 600,
      maxHealth: 600,
      armor: 15,
      attackDamage: 50,
      attackRange: 200,
      attackSpeed: 2,
      sightRange: 250,
      cost: { credits: 200, power: 0, ore: 0 },
      buildTime: 10,
      size: { width: 60, height: 60 },
      powerProduction: 0,
      powerConsumption: 5,
      resourceProduction: 0,
    });
  }

  public static create(type: string, id: string, position: Vector2D, team: number): Building | null {
    const template = this.buildingTemplates.get(type);
    if (!template) return null;

    const stats: BuildingStats = {
      health: template.health!,
      maxHealth: template.maxHealth!,
      armor: template.armor!,
      attackDamage: template.attackDamage!,
      attackRange: template.attackRange!,
      attackSpeed: template.attackSpeed!,
      sightRange: template.sightRange!,
      cost: { ...template.cost! },
      buildTime: template.buildTime!,
      size: { ...template.size! },
      powerProduction: template.powerProduction!,
      powerConsumption: template.powerConsumption!,
      resourceProduction: template.resourceProduction!,
    };

    return new Building({
      id,
      position,
      type,
      team,
      stats,
    });
  }

  public static registerTemplate(type: string, stats: Partial<BuildingStats>): void {
    this.buildingTemplates.set(type, stats);
  }

  public static getTemplate(type: string): Partial<BuildingStats> | undefined {
    return this.buildingTemplates.get(type);
  }

  public static getAllTypes(): string[] {
    return Array.from(this.buildingTemplates.keys());
  }
}

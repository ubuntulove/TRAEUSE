import { Entity, Vector2D } from '../../entities/Entity';

export interface UnitStats {
  health: number;
  maxHealth: number;
  armor: number;
  attackDamage: number;
  attackRange: number;
  attackSpeed: number;
  moveSpeed: number;
  sightRange: number;
  cost: { credits: number; power: number; ore: number };
  buildTime: number;
  size: number;
  canAttack: boolean;
  canMove: boolean;
  canCapture: boolean;
  isAir: boolean;
  isGround: boolean;
  isNaval: boolean;
  cargoCapacity: number;
  fuel: number;
  fuelConsumption: number;
}

export interface UnitOrder {
  type: 'move' | 'attack' | 'patrol' | 'guard' | 'build' | 'capture' | 'harvest' | 'repair';
  targetPosition?: Vector2D;
  targetEntity?: Entity;
  queued: boolean;
}

export class Unit extends Entity {
  public unitType: string;
  public stats: UnitStats;
  public isConstructed: boolean;
  public constructionProgress: number;
  public orders: UnitOrder[];
  public currentOrder: UnitOrder | null;
  public attackTarget: Entity | null;
  public moveTarget: Vector2D | null;
  public path: Vector2D[];
  public pathIndex: number;
  public carryCapacity: { credits: number; ore: number };
  public cargo: any[];
  public garrison: string[];
  public statusEffects: Map<string, { duration: number; intensity: number }>;
  public experience: number;
  public level: number;
  public kills: number;
  public isSelected: boolean;
  public rallyPoint: Vector2D | null;

  constructor(config: {
    id: string;
    position: Vector2D;
    type: string;
    team: number;
    stats: UnitStats;
  }) {
    super({
      id: config.id,
      position: config.position,
      health: config.stats.health,
      maxHealth: config.stats.maxHealth,
      damage: config.stats.attackDamage,
      armor: config.stats.armor,
      team: config.team,
      type: 'unit',
      maxSpeed: config.stats.moveSpeed,
    });

    this.unitType = config.type;
    this.stats = config.stats;
    this.isConstructed = config.stats.buildTime === 0;
    this.constructionProgress = this.isConstructed ? 1 : 0;
    this.orders = [];
    this.currentOrder = null;
    this.attackTarget = null;
    this.moveTarget = null;
    this.path = [];
    this.pathIndex = 0;
    this.carryCapacity = { credits: 0, ore: 0 };
    this.cargo = [];
    this.garrison = [];
    this.statusEffects = new Map();
    this.experience = 0;
    this.level = 1;
    this.kills = 0;
    this.isSelected = false;
    this.rallyPoint = null;
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

    this.updateStatusEffects(deltaTime);

    if (this.currentOrder) {
      this.executeOrder(deltaTime);
    }

    if (this.statusEffects.has('speed-boost')) {
      const boost = this.statusEffects.get('speed-boost')!;
      this.maxSpeed = this.stats.moveSpeed * 1.5;
    } else {
      this.maxSpeed = this.stats.moveSpeed;
    }

    if (this.statusEffects.has('damage-boost')) {
      const boost = this.statusEffects.get('damage-boost')!;
      this.damage = this.stats.attackDamage * 1.25;
    } else {
      this.damage = this.stats.attackDamage;
    }
  }

  private updateStatusEffects(deltaTime: number): void {
    for (const [name, effect] of this.statusEffects) {
      effect.duration -= deltaTime;

      if (effect.duration <= 0) {
        this.statusEffects.delete(name);
      }
    }
  }

  public giveOrder(order: UnitOrder): void {
    this.orders.push(order);
    if (!this.currentOrder) {
      this.currentOrder = this.orders.shift() || null;
    }
  }

  public clearOrders(): void {
    this.orders = [];
    this.currentOrder = null;
    this.attackTarget = null;
    this.moveTarget = null;
    this.path = [];
    this.pathIndex = 0;
    this.stop();
  }

  public cancelCurrentOrder(): void {
    this.currentOrder = this.orders.shift() || null;
    this.attackTarget = null;
    this.moveTarget = null;
    this.path = [];
    this.pathIndex = 0;
    this.stop();
  }

  private executeOrder(deltaTime: number): void {
    switch (this.currentOrder!.type) {
      case 'move':
        this.executeMoveOrder(deltaTime);
        break;
      case 'attack':
        this.executeAttackOrder(deltaTime);
        break;
      case 'patrol':
        this.executePatrolOrder(deltaTime);
        break;
      case 'guard':
        this.executeGuardOrder(deltaTime);
        break;
      case 'harvest':
        this.executeHarvestOrder(deltaTime);
        break;
      case 'repair':
        this.executeRepairOrder(deltaTime);
        break;
    }
  }

  private executeMoveOrder(deltaTime: number): void {
    if (!this.moveTarget) return;

    const dx = this.moveTarget.x - this.position.x;
    const dy = this.moveTarget.y - this.position.y;
    const distance = Math.sqrt(dx * dx + dy * dy);

    if (distance < 10) {
      this.currentOrder = this.orders.shift() || null;
      this.moveTarget = null;
      this.path = [];
      this.pathIndex = 0;
      this.stop();
      return;
    }

    if (this.path.length > 0 && this.pathIndex < this.path.length) {
      const nextPoint = this.path[this.pathIndex];
      const pdx = nextPoint.x - this.position.x;
      const pdy = nextPoint.y - this.position.y;
      const pdist = Math.sqrt(pdx * pdx + pdy * pdy);

      if (pdist < 10) {
        this.pathIndex++;
      } else {
        const direction = { x: pdx / pdist, y: pdy / pdist };
        this.velocity.x = direction.x * this.maxSpeed;
        this.velocity.y = direction.y * this.maxSpeed;
      }
    } else {
      const direction = { x: dx / distance, y: dy / distance };
      this.velocity.x = direction.x * this.maxSpeed;
      this.velocity.y = direction.y * this.maxSpeed;
    }
  }

  private executeAttackOrder(deltaTime: number): void {
    if (!this.attackTarget || !this.attackTarget.isAlive()) {
      this.attackTarget = null;
      this.currentOrder = this.orders.shift() || null;
      return;
    }

    const distance = this.getDistanceTo(this.attackTarget);

    if (distance > this.stats.attackRange) {
      this.moveTarget = this.attackTarget.position;
      this.executeMoveOrder(deltaTime);
    } else {
      this.stop();
      this.lookAt(this.attackTarget.position);
    }
  }

  private executePatrolOrder(deltaTime: number): void {
    if (!this.moveTarget) {
      if (this.currentOrder!.targetPosition) {
        this.moveTarget = this.currentOrder!.targetPosition;
        this.executeMoveOrder(deltaTime);
      }
    } else {
      const dx = this.moveTarget.x - this.position.x;
      const dy = this.moveTarget.y - this.position.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance < 10) {
        this.moveTarget = this.currentOrder!.targetEntity?.position || this.position;
      }

      this.executeMoveOrder(deltaTime);
    }
  }

  private executeGuardOrder(deltaTime: number): void {
    if (!this.currentOrder!.targetEntity) {
      this.currentOrder = this.orders.shift() || null;
      return;
    }

    const guardTarget = this.currentOrder!.targetEntity;
    const distance = this.getDistanceTo(guardTarget);

    if (distance > this.stats.sightRange) {
      this.moveTarget = guardTarget.position;
      this.executeMoveOrder(deltaTime);
    } else {
      const nearbyEnemies = this.statusEffects.get('nearby-enemies') as any;
      if (nearbyEnemies) {
        this.attackTarget = nearbyEnemies[0];
        this.executeAttackOrder(deltaTime);
      } else {
        this.stop();
      }
    }
  }

  private executeHarvestOrder(deltaTime: number): void {
  }

  private executeRepairOrder(deltaTime: number): void {
  }

  public setPath(path: Vector2D[]): void {
    this.path = path;
    this.pathIndex = 0;
  }

  public clearPath(): void {
    this.path = [];
    this.pathIndex = 0;
  }

  public addExperience(amount: number): void {
    this.experience += amount;
    const expForNextLevel = this.calculateExpForLevel(this.level + 1);

    while (this.experience >= expForNextLevel) {
      this.experience -= expForNextLevel;
      this.level++;
      this.applyLevelUp();
    }
  }

  private calculateExpForLevel(level: number): number {
    return 100 * level * level;
  }

  private applyLevelUp(): void {
    this.maxHealth += 10;
    this.health = Math.min(this.health + 10, this.maxHealth);
    this.damage += 2;
  }

  public addStatusEffect(name: string, duration: number, intensity: number = 1): void {
    this.statusEffects.set(name, { duration, intensity });
  }

  public removeStatusEffect(name: string): void {
    this.statusEffects.delete(name);
  }

  public hasStatusEffect(name: string): boolean {
    return this.statusEffects.has(name);
  }

  public getStatusEffect(name: string): { duration: number; intensity: number } | undefined {
    return this.statusEffects.get(name);
  }

  public addKill(): void {
    this.kills++;
    this.addExperience(25);
  }

  public garrisonUnit(unitId: string): boolean {
    if (this.garrison.length >= this.stats.cargoCapacity) {
      return false;
    }
    this.garrison.push(unitId);
    return true;
  }

  public ungarrisonUnit(unitId: string): boolean {
    const index = this.garrison.indexOf(unitId);
    if (index === -1) {
      return false;
    }
    this.garrison.splice(index, 1);
    return true;
  }

  public canGarrison(): boolean {
    return this.garrison.length < this.stats.cargoCapacity;
  }

  public isGarrisoned(): boolean {
    return this.garrison.length > 0;
  }

  public harvest(resource: { credits?: number; ore?: number }): void {
    if (resource.credits) {
      this.carryCapacity.credits = Math.min(100, this.carryCapacity.credits + resource.credits);
    }
    if (resource.ore) {
      this.carryCapacity.ore = Math.min(100, this.carryCapacity.ore + resource.ore);
    }
  }

  public isCarrying(): boolean {
    return this.carryCapacity.credits > 0 || this.carryCapacity.ore > 0;
  }

  public isFull(): boolean {
    return this.carryCapacity.credits >= 100 && this.carryCapacity.ore >= 100;
  }

  public unload(): { credits: number; ore: number } {
    const resources = { ...this.carryCapacity };
    this.carryCapacity = { credits: 0, ore: 0 };
    return resources;
  }
}

export class UnitManager {
  private units: Map<string, Unit> = new Map();
  private unitsByType: Map<string, Set<string>> = new Map();
  private unitsByTeam: Map<number, Set<string>> = new Map();

  public add(unit: Unit): void {
    this.units.set(unit.id, unit);

    if (!this.unitsByType.has(unit.unitType)) {
      this.unitsByType.set(unit.unitType, new Set());
    }
    this.unitsByType.get(unit.unitType)!.add(unit.id);

    if (!this.unitsByTeam.has(unit.team)) {
      this.unitsByTeam.set(unit.team, new Set());
    }
    this.unitsByTeam.get(unit.team)!.add(unit.id);
  }

  public remove(id: string): void {
    const unit = this.units.get(id);
    if (!unit) return;

    this.unitsByType.get(unit.unitType)?.delete(id);
    this.unitsByTeam.get(unit.team)?.delete(id);
    this.units.delete(id);
  }

  public get(id: string): Unit | undefined {
    return this.units.get(id);
  }

  public getAll(): Unit[] {
    return Array.from(this.units.values());
  }

  public getByType(type: string): Unit[] {
    const ids = this.unitsByType.get(type);
    if (!ids) return [];
    return Array.from(ids).map(id => this.units.get(id)).filter((u): u is Unit => u !== undefined);
  }

  public getByTeam(team: number): Unit[] {
    const ids = this.unitsByTeam.get(team);
    if (!ids) return [];
    return Array.from(ids).map(id => this.units.get(id)).filter((u): u is Unit => u !== undefined);
  }

  public getConstructed(): Unit[] {
    return this.getAll().filter(u => u.isConstructed);
  }

  public getUnconstructed(): Unit[] {
    return this.getAll().filter(u => !u.isConstructed);
  }

  public getSelected(): Unit[] {
    return this.getAll().filter(u => u.isSelected);
  }

  public select(id: string, multiSelect: boolean = false): void {
    const unit = this.units.get(id);
    if (!unit) return;

    if (!multiSelect) {
      this.deselectAll();
    }

    unit.isSelected = true;
  }

  public deselect(id: string): void {
    const unit = this.units.get(id);
    if (unit) {
      unit.isSelected = false;
    }
  }

  public deselectAll(): void {
    for (const unit of this.units.values()) {
      unit.isSelected = false;
    }
  }

  public update(deltaTime: number): void {
    for (const unit of this.units.values()) {
      if (unit.active) {
        unit.update(deltaTime);
      }
    }

    for (const [id, unit] of this.units) {
      if (!unit.isAlive()) {
        this.remove(id);
      }
    }
  }

  public clear(): void {
    this.units.clear();
    this.unitsByType.clear();
    this.unitsByTeam.clear();
  }

  public size(): number {
    return this.units.size;
  }
}

export class UnitFactory {
  private static unitTemplates: Map<string, Partial<UnitStats>> = new Map();

  static {
    this.unitTemplates.set('soldier', {
      health: 100,
      maxHealth: 100,
      armor: 2,
      attackDamage: 15,
      attackRange: 150,
      attackSpeed: 1,
      moveSpeed: 40,
      sightRange: 200,
      cost: { credits: 100, power: 0, ore: 0 },
      buildTime: 8,
      size: 10,
      canAttack: true,
      canMove: true,
      canCapture: true,
      isAir: false,
      isGround: true,
      isNaval: false,
      cargoCapacity: 0,
      fuel: 0,
      fuelConsumption: 0,
    });

    this.unitTemplates.set('tank', {
      health: 400,
      maxHealth: 400,
      armor: 8,
      attackDamage: 40,
      attackRange: 200,
      attackSpeed: 0.5,
      moveSpeed: 30,
      sightRange: 250,
      cost: { credits: 600, power: 0, ore: 0 },
      buildTime: 20,
      size: 20,
      canAttack: true,
      canMove: true,
      canCapture: false,
      isAir: false,
      isGround: true,
      isNaval: false,
      cargoCapacity: 0,
      fuel: 0,
      fuelConsumption: 0,
    });

    this.unitTemplates.set('miner', {
      health: 80,
      maxHealth: 80,
      armor: 1,
      attackDamage: 0,
      attackRange: 0,
      attackSpeed: 0,
      moveSpeed: 35,
      sightRange: 150,
      cost: { credits: 300, power: 0, ore: 0 },
      buildTime: 12,
      size: 12,
      canAttack: false,
      canMove: true,
      canCapture: false,
      isAir: false,
      isGround: true,
      isNaval: false,
      cargoCapacity: 100,
      fuel: 0,
      fuelConsumption: 0,
    });

    this.unitTemplates.set('scout', {
      health: 50,
      maxHealth: 50,
      armor: 1,
      attackDamage: 5,
      attackRange: 100,
      attackSpeed: 1,
      moveSpeed: 80,
      sightRange: 400,
      cost: { credits: 150, power: 0, ore: 0 },
      buildTime: 6,
      size: 8,
      canAttack: true,
      canMove: true,
      canCapture: false,
      isAir: false,
      isGround: true,
      isNaval: false,
      cargoCapacity: 0,
      fuel: 0,
      fuelConsumption: 0,
    });

    this.unitTemplates.set('apc', {
      health: 200,
      maxHealth: 200,
      armor: 5,
      attackDamage: 0,
      attackRange: 0,
      attackSpeed: 0,
      moveSpeed: 45,
      sightRange: 180,
      cost: { credits: 400, power: 0, ore: 0 },
      buildTime: 15,
      size: 15,
      canAttack: false,
      canMove: true,
      canCapture: false,
      isAir: false,
      isGround: true,
      isNaval: false,
      cargoCapacity: 5,
      fuel: 0,
      fuelConsumption: 0,
    });

    this.unitTemplates.set('helicopter', {
      health: 120,
      maxHealth: 120,
      armor: 2,
      attackDamage: 25,
      attackRange: 180,
      attackSpeed: 1.5,
      moveSpeed: 70,
      sightRange: 300,
      cost: { credits: 500, power: 0, ore: 0 },
      buildTime: 18,
      size: 15,
      canAttack: true,
      canMove: true,
      canCapture: false,
      isAir: true,
      isGround: false,
      isNaval: false,
      cargoCapacity: 0,
      fuel: 100,
      fuelConsumption: 0.5,
    });
  }

  public static create(type: string, id: string, position: Vector2D, team: number): Unit | null {
    const template = this.unitTemplates.get(type);
    if (!template) return null;

    const stats: UnitStats = {
      health: template.health!,
      maxHealth: template.maxHealth!,
      armor: template.armor!,
      attackDamage: template.attackDamage!,
      attackRange: template.attackRange!,
      attackSpeed: template.attackSpeed!,
      moveSpeed: template.moveSpeed!,
      sightRange: template.sightRange!,
      cost: { ...template.cost! },
      buildTime: template.buildTime!,
      size: template.size!,
      canAttack: template.canAttack!,
      canMove: template.canMove!,
      canCapture: template.canCapture!,
      isAir: template.isAir!,
      isGround: template.isGround!,
      isNaval: template.isNaval!,
      cargoCapacity: template.cargoCapacity!,
      fuel: template.fuel!,
      fuelConsumption: template.fuelConsumption!,
    };

    return new Unit({
      id,
      position,
      type,
      team,
      stats,
    });
  }

  public static registerTemplate(type: string, stats: Partial<UnitStats>): void {
    this.unitTemplates.set(type, stats);
  }

  public static getTemplate(type: string): Partial<UnitStats> | undefined {
    return this.unitTemplates.get(type);
  }

  public static getAllTypes(): string[] {
    return Array.from(this.unitTemplates.keys());
  }
}

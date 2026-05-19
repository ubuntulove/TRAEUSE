import { Entity, Vector2D } from '../../entities/Entity';
import { Unit } from '../units/UnitSystem';
import { Building } from '../buildings/BuildingSystem';

export interface CombatConfig {
  damageFalloff: boolean;
  armorReduction: boolean;
  criticalHits: boolean;
  splashDamage: boolean;
  friendlyFire: boolean;
  overwatch: boolean;
  suppressFire: boolean;
}

export interface DamageType {
  name: string;
  multiplier: number;
  vsArmor: number;
  vsInfantry: number;
  vsVehicle: number;
  vsAircraft: number;
  vsBuilding: number;
}

export interface CombatResult {
  attacker: Entity;
  defender: Entity;
  damageDealt: number;
  damageType: string;
  isCritical: boolean;
  isSplash: boolean;
  splashTargets: Entity[];
  isKillingBlow: boolean;
  defenderDied: boolean;
}

export interface Projectile {
  id: string;
  position: Vector2D;
  velocity: Vector2D;
  target: Entity;
  damage: number;
  damageType: string;
  speed: number;
  owner: Entity;
  size: number;
  active: boolean;
}

export class CombatManager {
  private config: CombatConfig;
  private damageTypes: Map<string, DamageType> = new Map();
  private projectiles: Map<string, Projectile> = new Map();
  private attackCooldowns: Map<string, number> = new Map();
  private damageEvents: CombatResult[] = [];
  private criticalChance: number = 0.1;
  private criticalMultiplier: number = 2.0;
  private splashRadii: Map<string, number> = new Map();

  constructor(config: Partial<CombatConfig> = {}) {
    this.config = {
      damageFalloff: config.damageFalloff ?? true,
      armorReduction: config.armorReduction ?? true,
      criticalHits: config.criticalHits ?? true,
      splashDamage: config.splashDamage ?? true,
      friendlyFire: config.friendlyFire ?? false,
      overwatch: config.overwatch ?? false,
      suppressFire: config.suppressFire ?? false,
    };

    this.initializeDamageTypes();
  }

  private initializeDamageTypes(): void {
    this.damageTypes.set('ballistic', {
      name: 'Ballistic',
      multiplier: 1.0,
      vsArmor: 0.8,
      vsInfantry: 1.2,
      vsVehicle: 1.0,
      vsAircraft: 0.5,
      vsBuilding: 0.7,
    });

    this.damageTypes.set('explosive', {
      name: 'Explosive',
      multiplier: 1.5,
      vsArmor: 1.2,
      vsInfantry: 0.8,
      vsVehicle: 1.5,
      vsAircraft: 1.0,
      vsBuilding: 1.2,
    });

    this.damageTypes.set('energy', {
      name: 'Energy',
      multiplier: 1.2,
      vsArmor: 1.0,
      vsInfantry: 1.0,
      vsVehicle: 1.0,
      vsAircraft: 1.5,
      vsBuilding: 0.8,
    });

    this.damageTypes.set('fire', {
      name: 'Fire',
      multiplier: 0.8,
      vsArmor: 0.5,
      vsInfantry: 1.5,
      vsVehicle: 0.7,
      vsAircraft: 0.6,
      vsBuilding: 1.0,
    });

    this.splashRadii.set('explosive', 50);
    this.splashRadii.set('fire', 30);
  }

  public calculateDamage(
    attacker: Entity,
    defender: Entity,
    damageType: string = 'ballistic'
  ): number {
    let baseDamage = attacker.damage;

    if (this.config.armorReduction) {
      baseDamage = Math.max(1, baseDamage - defender.armor * 0.5);
    }

    const damageTypeData = this.damageTypes.get(damageType);
    if (damageTypeData) {
      baseDamage *= damageTypeData.multiplier;

      if (defender.type === 'infantry') {
        baseDamage *= damageTypeData.vsInfantry;
      } else if (defender.type === 'vehicle') {
        baseDamage *= damageTypeData.vsVehicle;
      } else if (defender.type === 'aircraft') {
        baseDamage *= damageTypeData.vsAircraft;
      } else if (defender.type === 'building') {
        baseDamage *= damageTypeData.vsBuilding;
      }
    }

    if (this.config.criticalHits && Math.random() < this.criticalChance) {
      baseDamage *= this.criticalMultiplier;
    }

    if (this.config.damageFalloff) {
      const distance = attacker.getDistanceTo(defender);
      const falloffStart = 100;
      const falloffEnd = 500;

      if (distance > falloffStart) {
        const falloff = Math.min(1, (distance - falloffStart) / (falloffEnd - falloffStart));
        baseDamage *= (1 - falloff * 0.5);
      }
    }

    return Math.round(baseDamage);
  }

  public attack(attacker: Entity, defender: Entity, damageType: string = 'ballistic'): CombatResult {
    const cooldownKey = `${attacker.id}-${defender.id}`;
    const lastAttack = this.attackCooldowns.get(cooldownKey) || 0;
    const now = Date.now();

    const cooldownTime = attacker.type === 'unit' ? 1000 : 2000;

    if (now - lastAttack < cooldownTime) {
      return {
        attacker,
        defender,
        damageDealt: 0,
        damageType,
        isCritical: false,
        isSplash: false,
        splashTargets: [],
        isKillingBlow: false,
        defenderDied: false,
      };
    }

    this.attackCooldowns.set(cooldownKey, now);

    const damage = this.calculateDamage(attacker, defender, damageType);
    defender.takeDamage(damage);

    const isCritical = damage > attacker.damage * this.criticalMultiplier * 0.9;
    const isSplash = this.config.splashDamage && this.splashRadii.has(damageType);
    const splashTargets: Entity[] = [];

    if (isSplash && defender.isAlive()) {
      const splashRadius = this.splashRadii.get(damageType)!;
      const splashDamage = Math.round(damage * 0.5);

      this.damageSplashArea(defender.position, splashRadius, splashDamage, attacker, damageType, splashTargets);
    }

    const result: CombatResult = {
      attacker,
      defender,
      damageDealt: damage,
      damageType,
      isCritical,
      isSplash,
      splashTargets,
      isKillingBlow: damage >= defender.health,
      defenderDied: !defender.isAlive(),
    };

    this.damageEvents.push(result);

    if (result.defenderDied && attacker.type === 'unit') {
      (attacker as Unit).addKill();
    }

    return result;
  }

  private damageSplashArea(
    center: Vector2D,
    radius: number,
    damage: number,
    attacker: Entity,
    damageType: string,
    splashTargets: Entity[]
  ): void {
  }

  public createProjectile(
    id: string,
    position: Vector2D,
    target: Entity,
    damage: number,
    damageType: string,
    speed: number,
    owner: Entity
  ): Projectile {
    const projectile: Projectile = {
      id,
      position: { ...position },
      velocity: { x: 0, y: 0 },
      target,
      damage,
      damageType,
      speed,
      owner,
      size: 5,
      active: true,
    };

    this.projectiles.set(id, projectile);
    return projectile;
  }

  public updateProjectiles(deltaTime: number, entities: Entity[]): void {
    for (const [id, projectile] of this.projectiles) {
      if (!projectile.active) continue;

      if (!projectile.target.isAlive()) {
        projectile.active = false;
        continue;
      }

      const dx = projectile.target.position.x - projectile.position.x;
      const dy = projectile.target.position.y - projectile.position.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance < 10) {
        this.attack(projectile.owner, projectile.target, projectile.damageType);
        projectile.active = false;
        continue;
      }

      const direction = { x: dx / distance, y: dy / distance };
      projectile.velocity.x = direction.x * projectile.speed;
      projectile.velocity.y = direction.y * projectile.speed;

      projectile.position.x += projectile.velocity.x * deltaTime;
      projectile.position.y += projectile.velocity.y * deltaTime;
    }

    for (const [id, projectile] of this.projectiles) {
      if (!projectile.active) {
        this.projectiles.delete(id);
      }
    }
  }

  public getProjectile(id: string): Projectile | undefined {
    return this.projectiles.get(id);
  }

  public getAllProjectiles(): Projectile[] {
    return Array.from(this.projectiles.values()).filter(p => p.active);
  }

  public getDamageEvents(): CombatResult[] {
    return [...this.damageEvents];
  }

  public clearDamageEvents(): void {
    this.damageEvents = [];
  }

  public registerDamageType(type: string, data: DamageType): void {
    this.damageTypes.set(type, data);
  }

  public setSplashRadius(damageType: string, radius: number): void {
    this.splashRadii.set(damageType, radius);
  }

  public setCriticalChance(chance: number): void {
    this.criticalChance = Math.max(0, Math.min(1, chance));
  }

  public setCriticalMultiplier(multiplier: number): void {
    this.criticalMultiplier = Math.max(1, multiplier);
  }

  public getConfig(): CombatConfig {
    return { ...this.config };
  }

  public setConfig(config: Partial<CombatConfig>): void {
    this.config = { ...this.config, ...config };
  }

  public canAttack(attacker: Entity, defender: Entity): boolean {
    if (!attacker.isAlive() || !defender.isAlive()) {
      return false;
    }

    if (!this.config.friendlyFire && attacker.team === defender.team) {
      return false;
    }

    return true;
  }

  public getAttackCooldown(attackerId: string, defenderId: string): number {
    const key = `${attackerId}-${defenderId}`;
    return this.attackCooldowns.get(key) || 0;
  }

  public clearCooldowns(): void {
    this.attackCooldowns.clear();
  }

  public clear(): void {
    this.projectiles.clear();
    this.attackCooldowns.clear();
    this.damageEvents = [];
  }
}

export class CombatAI {
  private combatManager: CombatManager;
  private preferredTargetType: string | null = null;
  private attackRangeMultiplier: number = 1.0;
  private retreatThreshold: number = 0.25;
  private aggressiveMode: boolean = true;

  constructor(combatManager: CombatManager) {
    this.combatManager = combatManager;
  }

  public evaluateThreat(unit: Unit, nearbyEnemies: Entity[]): number {
    let threat = 0;

    for (const enemy of nearbyEnemies) {
      const distance = unit.getDistanceTo(enemy);
      const inRange = distance <= unit.stats.attackRange * this.attackRangeMultiplier;

      if (inRange) {
        threat += enemy.damage * (1 - unit.health / unit.maxHealth);
      }
    }

    return threat;
  }

  public shouldRetreat(unit: Unit): boolean {
    if (!this.aggressiveMode) return false;

    const healthPercent = unit.health / unit.maxHealth;
    return healthPercent < this.retreatThreshold;
  }

  public findBestTarget(unit: Unit, enemies: Entity[]): Entity | null {
    if (enemies.length === 0) return null;

    let bestTarget: Entity | null = null;
    let bestScore = -Infinity;

    for (const enemy of enemies) {
      if (!enemy.isAlive()) continue;

      const distance = unit.getDistanceTo(enemy);
      const inRange = distance <= unit.stats.attackRange;

      if (!inRange) continue;

      let score = enemy.damage * 0.5;

      if (enemy.health < unit.damage) {
        score += 100;
      }

      if (enemy.type === this.preferredTargetType) {
        score += 50;
      }

      score -= distance * 0.1;

      if (score > bestScore) {
        bestScore = score;
        bestTarget = enemy;
      }
    }

    return bestTarget;
  }

  public setPreferredTarget(type: string | null): void {
    this.preferredTargetType = type;
  }

  public setAttackRangeMultiplier(multiplier: number): void {
    this.attackRangeMultiplier = multiplier;
  }

  public setRetreatThreshold(threshold: number): void {
    this.retreatThreshold = Math.max(0, Math.min(1, threshold));
  }

  public setAggressiveMode(aggressive: boolean): void {
    this.aggressiveMode = aggressive;
  }
}

export class LineOfSight {
  private obstacles: Set<string> = new Set();

  public addObstacle(x: number, y: number): void {
    this.obstacles.add(`${x},${y}`);
  }

  public removeObstacle(x: number, y: number): void {
    this.obstacles.delete(`${x},${y}`);
  }

  public hasObstacle(x: number, y: number): boolean {
    return this.obstacles.has(`${x},${y}`);
  }

  public checkLineOfSight(from: Vector2D, to: Vector2D): boolean {
    const dx = to.x - from.x;
    const dy = to.y - from.y;
    const distance = Math.sqrt(dx * dx + dy * dy);
    const steps = Math.ceil(distance / 10);

    for (let i = 0; i <= steps; i++) {
      const t = i / steps;
      const x = Math.round(from.x + dx * t);
      const y = Math.round(from.y + dy * t);

      if (this.hasObstacle(x, y)) {
        return false;
      }
    }

    return true;
  }

  public clear(): void {
    this.obstacles.clear();
  }
}

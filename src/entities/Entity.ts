
export interface Vector2D {
  x: number;
  y: number;
}

export interface Position {
  x: number;
  y: number;
}

export interface AABB {
  minX: number;
  minY: number;
  maxX: number;
  maxY: number;
}

export interface Circle {
  center: Position;
  radius: number;
}

export interface Rectangle {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface Polygon {
  vertices: Position[];
}

export interface Transform {
  position: Vector2D;
  rotation: number;
  scale: Vector2D;
}

export interface EntityConfig {
  id: string;
  position: Vector2D;
  rotation?: number;
  scale?: Vector2D;
  velocity?: Vector2D;
  acceleration?: Vector2D;
  maxSpeed?: number;
  health?: number;
  maxHealth?: number;
  damage?: number;
  armor?: number;
  team?: number;
  type?: string;
  tag?: string;
  active?: boolean;
  visible?: boolean;
}

export class Entity {
  public id: string;
  public position: Vector2D;
  public rotation: number;
  public scale: Vector2D;
  public velocity: Vector2D;
  public acceleration: Vector2D;
  public maxSpeed: number;
  public health: number;
  public maxHealth: number;
  public damage: number;
  public armor: number;
  public team: number;
  public type: string;
  public tag: string;
  public active: boolean;
  public visible: boolean;
  public age: number;
  public lifetime: number;

  constructor(config: EntityConfig) {
    this.id = config.id;
    this.position = { ...config.position };
    this.rotation = config.rotation || 0;
    this.scale = config.scale || { x: 1, y: 1 };
    this.velocity = config.velocity || { x: 0, y: 0 };
    this.acceleration = config.acceleration || { x: 0, y: 0 };
    this.maxSpeed = config.maxSpeed || 100;
    this.health = config.health || 100;
    this.maxHealth = config.maxHealth || 100;
    this.damage = config.damage || 10;
    this.armor = config.armor || 0;
    this.team = config.team || 0;
    this.type = config.type || 'generic';
    this.tag = config.tag || '';
    this.active = config.active !== undefined ? config.active : true;
    this.visible = config.visible !== undefined ? config.visible : true;
    this.age = 0;
    this.lifetime = 0;
  }

  public update(deltaTime: number): void {
    this.age += deltaTime;
    this.velocity.x += this.acceleration.x * deltaTime;
    this.velocity.y += this.acceleration.y * deltaTime;

    const speed = Math.sqrt(this.velocity.x * this.velocity.x + this.velocity.y * this.velocity.y);
    if (speed > this.maxSpeed) {
      const scale = this.maxSpeed / speed;
      this.velocity.x *= scale;
      this.velocity.y *= scale;
    }

    this.position.x += this.velocity.x * deltaTime;
    this.position.y += this.velocity.y * deltaTime;

    if (this.lifetime > 0 && this.age >= this.lifetime) {
      this.active = false;
    }
  }

  public applyForce(force: Vector2D): void {
    this.acceleration.x += force.x;
    this.acceleration.y += force.y;
  }

  public takeDamage(amount: number): void {
    const actualDamage = Math.max(0, amount - this.armor);
    this.health = Math.max(0, this.health - actualDamage);

    if (this.health <= 0) {
      this.active = false;
      this.onDeath();
    }
  }

  public heal(amount: number): void {
    this.health = Math.min(this.maxHealth, this.health + amount);
  }

  public getDistanceTo(other: Entity): number {
    const dx = other.position.x - this.position.x;
    const dy = other.position.y - this.position.y;
    return Math.sqrt(dx * dx + dy * dy);
  }

  public getDirectionTo(other: Entity): Vector2D {
    const dx = other.position.x - this.position.x;
    const dy = other.position.y - this.position.y;
    const distance = Math.sqrt(dx * dx + dy * dy);
    if (distance === 0) return { x: 0, y: 0 };
    return { x: dx / distance, y: dy / distance };
  }

  public lookAt(target: Vector2D): void {
    const dx = target.x - this.position.x;
    const dy = target.y - this.position.y;
    this.rotation = Math.atan2(dy, dx);
  }

  public moveTo(target: Vector2D, speed: number): void {
    const direction = this.getDirectionTo({ position: target } as Entity);
    this.velocity.x = direction.x * speed;
    this.velocity.y = direction.y * speed;
  }

  public stop(): void {
    this.velocity.x = 0;
    this.velocity.y = 0;
    this.acceleration.x = 0;
    this.acceleration.y = 0;
  }

  public isAlive(): boolean {
    return this.health > 0 && this.active;
  }

  public getAABB(): AABB {
    return {
      minX: this.position.x - this.scale.x / 2,
      minY: this.position.y - this.scale.y / 2,
      maxX: this.position.x + this.scale.x / 2,
      maxY: this.position.y + this.scale.y / 2,
    };
  }

  protected onDeath(): void {
  }

  public clone(): Entity {
    return new Entity({
      id: this.id + '_clone',
      position: { ...this.position },
      rotation: this.rotation,
      scale: { ...this.scale },
      velocity: { ...this.velocity },
      acceleration: { ...this.acceleration },
      maxSpeed: this.maxSpeed,
      health: this.health,
      maxHealth: this.maxHealth,
      damage: this.damage,
      armor: this.armor,
      team: this.team,
      type: this.type,
      tag: this.tag,
      active: this.active,
      visible: this.visible,
    });
  }

  public toJSON(): object {
    return {
      id: this.id,
      position: this.position,
      rotation: this.rotation,
      scale: this.scale,
      velocity: this.velocity,
      acceleration: this.acceleration,
      maxSpeed: this.maxSpeed,
      health: this.health,
      maxHealth: this.maxHealth,
      damage: this.damage,
      armor: this.armor,
      team: this.team,
      type: this.type,
      tag: this.tag,
      active: this.active,
      visible: this.visible,
      age: this.age,
      lifetime: this.lifetime,
    };
  }

  public fromJSON(data: any): void {
    this.position = data.position;
    this.rotation = data.rotation;
    this.scale = data.scale;
    this.velocity = data.velocity;
    this.acceleration = data.acceleration;
    this.maxSpeed = data.maxSpeed;
    this.health = data.health;
    this.maxHealth = data.maxHealth;
    this.damage = data.damage;
    this.armor = data.armor;
    this.team = data.team;
    this.type = data.type;
    this.tag = data.tag;
    this.active = data.active;
    this.visible = data.visible;
    this.age = data.age;
    this.lifetime = data.lifetime;
  }
}

export class EntityManager {
  private entities: Map<string, Entity> = new Map();
  private entityTags: Map<string, Set<string>> = new Map();
  private entityTypes: Map<string, Set<string>> = new Map();
  private entityTeams: Map<number, Set<string>> = new Map();

  public add(entity: Entity): void {
    this.entities.set(entity.id, entity);

    if (entity.tag) {
      if (!this.entityTags.has(entity.tag)) {
        this.entityTags.set(entity.tag, new Set());
      }
      this.entityTags.get(entity.tag)!.add(entity.id);
    }

    if (entity.type) {
      if (!this.entityTypes.has(entity.type)) {
        this.entityTypes.set(entity.type, new Set());
      }
      this.entityTypes.get(entity.type)!.add(entity.id);
    }

    if (!this.entityTeams.has(entity.team)) {
      this.entityTeams.set(entity.team, new Set());
    }
    this.entityTeams.get(entity.team)!.add(entity.id);
  }

  public remove(id: string): void {
    const entity = this.entities.get(id);
    if (!entity) return;

    if (entity.tag) {
      this.entityTags.get(entity.tag)?.delete(id);
    }
    if (entity.type) {
      this.entityTypes.get(entity.type)?.delete(id);
    }
    this.entityTeams.get(entity.team)?.delete(id);

    this.entities.delete(id);
  }

  public get(id: string): Entity | undefined {
    return this.entities.get(id);
  }

  public getAll(): Entity[] {
    return Array.from(this.entities.values());
  }

  public getByTag(tag: string): Entity[] {
    const ids = this.entityTags.get(tag);
    if (!ids) return [];
    return Array.from(ids).map(id => this.entities.get(id)).filter((e): e is Entity => e !== undefined);
  }

  public getByType(type: string): Entity[] {
    const ids = this.entityTypes.get(type);
    if (!ids) return [];
    return Array.from(ids).map(id => this.entities.get(id)).filter((e): e is Entity => e !== undefined);
  }

  public getByTeam(team: number): Entity[] {
    const ids = this.entityTeams.get(team);
    if (!ids) return [];
    return Array.from(ids).map(id => this.entities.get(id)).filter((e): e is Entity => e !== undefined);
  }

  public getInRadius(position: Vector2D, radius: number, includeDead: boolean = false): Entity[] {
    const result: Entity[] = [];
    const radiusSq = radius * radius;

    for (const entity of this.entities.values()) {
      if (!includeDead && !entity.isAlive()) continue;

      const dx = entity.position.x - position.x;
      const dy = entity.position.y - position.y;
      if (dx * dx + dy * dy <= radiusSq) {
        result.push(entity);
      }
    }

    return result;
  }

  public getInAABB(aabb: AABB, includeDead: boolean = false): Entity[] {
    const result: Entity[] = [];

    for (const entity of this.entities.values()) {
      if (!includeDead && !entity.isAlive()) continue;

      const entityAABB = entity.getAABB();
      if (
        entityAABB.minX <= aabb.maxX &&
        entityAABB.maxX >= aabb.minX &&
        entityAABB.minY <= aabb.maxY &&
        entityAABB.maxY >= aabb.minY
      ) {
        result.push(entity);
      }
    }

    return result;
  }

  public update(deltaTime: number): void {
    for (const entity of this.entities.values()) {
      if (entity.active) {
        entity.update(deltaTime);
      }
    }
  }

  public clear(): void {
    this.entities.clear();
    this.entityTags.clear();
    this.entityTypes.clear();
    this.entityTeams.clear();
  }

  public size(): number {
    return this.entities.size;
  }

  public has(id: string): boolean {
    return this.entities.has(id);
  }

  public getNearest(position: Vector2D, filter?: (entity: Entity) => boolean): Entity | null {
    let nearest: Entity | null = null;
    let minDistance = Infinity;

    for (const entity of this.entities.values()) {
      if (!entity.isAlive()) continue;
      if (filter && !filter(entity)) continue;

      const dx = entity.position.x - position.x;
      const dy = entity.position.y - position.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance < minDistance) {
        minDistance = distance;
        nearest = entity;
      }
    }

    return nearest;
  }

  public getFarthest(position: Vector2D, filter?: (entity: Entity) => boolean): Entity | null {
    let farthest: Entity | null = null;
    let maxDistance = -Infinity;

    for (const entity of this.entities.values()) {
      if (!entity.isAlive()) continue;
      if (filter && !filter(entity)) continue;

      const dx = entity.position.x - position.x;
      const dy = entity.position.y - position.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance > maxDistance) {
        maxDistance = distance;
        farthest = entity;
      }
    }

    return farthest;
  }

  public forEach(callback: (entity: Entity) => void): void {
    for (const entity of this.entities.values()) {
      callback(entity);
    }
  }

  public filter(predicate: (entity: Entity) => boolean): Entity[] {
    const result: Entity[] = [];
    for (const entity of this.entities.values()) {
      if (predicate(entity)) {
        result.push(entity);
      }
    }
    return result;
  }

  public find(predicate: (entity: Entity) => boolean): Entity | undefined {
    for (const entity of this.entities.values()) {
      if (predicate(entity)) {
        return entity;
      }
    }
    return undefined;
  }
}

export class SpatialHash {
  private cellSize: number;
  private cells: Map<string, Set<string>> = new Map();
  private entityPositions: Map<string, Vector2D> = new Map();

  constructor(cellSize: number = 100) {
    this.cellSize = cellSize;
  }

  private getCellKey(x: number, y: number): string {
    const cellX = Math.floor(x / this.cellSize);
    const cellY = Math.floor(y / this.cellSize);
    return `${cellX},${cellY}`;
  }

  public insert(entityId: string, position: Vector2D): void {
    const key = this.getCellKey(position.x, position.y);
    
    if (!this.cells.has(key)) {
      this.cells.set(key, new Set());
    }
    
    const oldKey = this.entityPositions.get(entityId);
    if (oldKey) {
      const oldCell = this.cells.get(this.getCellKey(oldKey.x, oldKey.y));
      oldCell?.delete(entityId);
    }
    
    this.cells.get(key)!.add(entityId);
    this.entityPositions.set(entityId, position);
  }

  public remove(entityId: string): void {
    const position = this.entityPositions.get(entityId);
    if (position) {
      const key = this.getCellKey(position.x, position.y);
      this.cells.get(key)?.delete(entityId);
      this.entityPositions.delete(entityId);
    }
  }

  public update(entityId: string, newPosition: Vector2D): void {
    this.insert(entityId, newPosition);
  }

  public queryRadius(position: Vector2D, radius: number): string[] {
    const results: string[] = [];
    const radiusSq = radius * radius;

    const minX = Math.floor((position.x - radius) / this.cellSize);
    const maxX = Math.floor((position.x + radius) / this.cellSize);
    const minY = Math.floor((position.y - radius) / this.cellSize);
    const maxY = Math.floor((position.y + radius) / this.cellSize);

    for (let cx = minX; cx <= maxX; cx++) {
      for (let cy = minY; cy <= maxY; cy++) {
        const key = `${cx},${cy}`;
        const cell = this.cells.get(key);
        if (cell) {
          for (const entityId of cell) {
            const entityPos = this.entityPositions.get(entityId);
            if (entityPos) {
              const dx = entityPos.x - position.x;
              const dy = entityPos.y - position.y;
              if (dx * dx + dy * dy <= radiusSq) {
                results.push(entityId);
              }
            }
          }
        }
      }
    }

    return results;
  }

  public queryAABB(minX: number, minY: number, maxX: number, maxY: number): string[] {
    const results: string[] = [];

    const cellMinX = Math.floor(minX / this.cellSize);
    const cellMaxX = Math.floor(maxX / this.cellSize);
    const cellMinY = Math.floor(minY / this.cellSize);
    const cellMaxY = Math.floor(maxY / this.cellSize);

    for (let cx = cellMinX; cx <= cellMaxX; cx++) {
      for (let cy = cellMinY; cy <= cellMaxY; cy++) {
        const key = `${cx},${cy}`;
        const cell = this.cells.get(key);
        if (cell) {
          for (const entityId of cell) {
            const entityPos = this.entityPositions.get(entityId);
            if (entityPos) {
              if (
                entityPos.x >= minX &&
                entityPos.x <= maxX &&
                entityPos.y >= minY &&
                entityPos.y <= maxY
              ) {
                results.push(entityId);
              }
            }
          }
        }
      }
    }

    return results;
  }

  public clear(): void {
    this.cells.clear();
    this.entityPositions.clear();
  }
}

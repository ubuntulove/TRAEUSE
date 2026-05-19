
import { Entity, Vector2D } from '../../entities/Entity';
import { EntityManager } from '../../entities/Entity';

export interface AIState {
  name: string;
  enter(): void;
  update(deltaTime: number): void;
  exit(): void;
  canTransitionTo(stateName: string): boolean;
}

export interface Transition {
  from: string;
  to: string;
  condition: () => boolean;
}

export class StateMachine {
  private states: Map<string, AIState> = new Map();
  private currentState: AIState | null = null;
  private transitions: Transition[] = [];

  public addState(state: AIState): void {
    this.states.set(state.name, state);
  }

  public addTransition(transition: Transition): void {
    this.transitions.push(transition);
  }

  public setState(stateName: string): void {
    const newState = this.states.get(stateName);
    if (!newState) {
      console.warn(`State ${stateName} not found`);
      return;
    }

    if (this.currentState) {
      this.currentState.exit();
    }

    this.currentState = newState;
    this.currentState.enter();
  }

  public update(deltaTime: number): void {
    if (!this.currentState) return;

    this.currentState.update(deltaTime);

    for (const transition of this.transitions) {
      if (transition.from === this.currentState!.name && transition.condition()) {
        this.setState(transition.to);
        break;
      }
    }
  }

  public getCurrentState(): AIState | null {
    return this.currentState;
  }

  public hasState(stateName: string): boolean {
    return this.states.has(stateName);
  }
}

export abstract class BaseAIBehavior {
  protected entity: Entity;
  protected target: Entity | null = null;
  protected stateMachine: StateMachine;
  protected decisionCooldown: number = 0;
  protected perceptionRange: number = 500;
  protected attackRange: number = 100;
  protected moveSpeed: number = 50;
  protected entityManager: EntityManager;

  constructor(entity: Entity, entityManager: EntityManager) {
    this.entity = entity;
    this.entityManager = entityManager;
    this.stateMachine = new StateMachine();
  }

  public abstract initialize(): void;
  public abstract update(deltaTime: number): void;

  protected findTarget(): Entity | null {
    const nearbyEntities = this.entityManager.getInRadius(
      this.entity.position,
      this.perceptionRange
    );

    let closestEnemy: Entity | null = null;
    let closestDistance = Infinity;

    for (const other of nearbyEntities) {
      if (other.team === this.entity.team) continue;
      if (!other.isAlive()) continue;

      const distance = this.entity.getDistanceTo(other);
      if (distance < closestDistance) {
        closestDistance = distance;
        closestEnemy = other;
      }
    }

    return closestEnemy;
  }

  protected moveToTarget(): void {
    if (!this.target) return;

    const direction = this.entity.getDirectionTo(this.target);
    this.entity.velocity.x = direction.x * this.moveSpeed;
    this.entity.velocity.y = direction.y * this.moveSpeed;
  }

  protected attackTarget(): void {
    if (!this.target) return;

    const distance = this.entity.getDistanceTo(this.target);
    if (distance <= this.attackRange) {
      this.target.takeDamage(this.entity.damage);
    }
  }

  protected stopMovement(): void {
    this.entity.velocity.x = 0;
    this.entity.velocity.y = 0;
  }

  public setPerceptionRange(range: number): void {
    this.perceptionRange = range;
  }

  public setAttackRange(range: number): void {
    this.attackRange = range;
  }

  public setMoveSpeed(speed: number): void {
    this.moveSpeed = speed;
  }
}

export class IdleState implements AIState {
  public name: string = 'idle';
  private behavior: BaseAIBehavior;

  constructor(behavior: BaseAIBehavior) {
    this.behavior = behavior;
  }

  public enter(): void {
  }

  public update(deltaTime: number): void {
    const target = this.behavior['findTarget']();
    if (target) {
      this.behavior['target'] = target;
    }
  }

  public exit(): void {
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'patrol' || stateName === 'chase' || stateName === 'attack';
  }
}

export class PatrolState implements AIState {
  public name: string = 'patrol';
  private behavior: BaseAIBehavior;
  private patrolPoints: Vector2D[] = [];
  private currentPatrolIndex: number = 0;
  private patrolWaitTime: number = 0;
  private maxPatrolWait: number = 2;

  constructor(behavior: BaseAIBehavior, patrolPoints: Vector2D[] = []) {
    this.behavior = behavior;
    this.patrolPoints = patrolPoints;
  }

  public enter(): void {
    this.patrolWaitTime = 0;
  }

  public update(deltaTime: number): void {
    const target = this.behavior['findTarget']();
    if (target) {
      this.behavior['target'] = target;
      return;
    }

    if (this.patrolPoints.length === 0) return;

    if (this.patrolWaitTime > 0) {
      this.patrolWaitTime -= deltaTime;
      this.behavior['stopMovement']();
      return;
    }

    const targetPoint = this.patrolPoints[this.currentPatrolIndex];
    const distance = Math.sqrt(
      Math.pow(targetPoint.x - this.behavior['entity'].position.x, 2) +
      Math.pow(targetPoint.y - this.behavior['entity'].position.y, 2)
    );

    if (distance < 10) {
      this.currentPatrolIndex = (this.currentPatrolIndex + 1) % this.patrolPoints.length;
      this.patrolWaitTime = this.maxPatrolWait;
      return;
    }

    const direction = {
      x: (targetPoint.x - this.behavior['entity'].position.x) / distance,
      y: (targetPoint.y - this.behavior['entity'].position.y) / distance,
    };

    this.behavior['entity'].velocity.x = direction.x * this.behavior['moveSpeed'];
    this.behavior['entity'].velocity.y = direction.y * this.behavior['moveSpeed'];
  }

  public exit(): void {
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'idle' || stateName === 'chase' || stateName === 'attack';
  }

  public setPatrolPoints(points: Vector2D[]): void {
    this.patrolPoints = points;
  }

  public addPatrolPoint(point: Vector2D): void {
    this.patrolPoints.push(point);
  }
}

export class ChaseState implements AIState {
  public name: string = 'chase';
  private behavior: BaseAIBehavior;
  private chaseTimer: number = 0;
  private maxChaseTime: number = 10;

  constructor(behavior: BaseAIBehavior) {
    this.behavior = behavior;
  }

  public enter(): void {
    this.chaseTimer = 0;
  }

  public update(deltaTime: number): void {
    const target = this.behavior['target'];
    if (!target || !target.isAlive()) {
      this.behavior['target'] = null;
      return;
    }

    this.chaseTimer += deltaTime;
    if (this.chaseTimer > this.maxChaseTime) {
      this.behavior['target'] = null;
      this.chaseTimer = 0;
      return;
    }

    const distance = this.behavior['entity'].getDistanceTo(target);

    if (distance <= this.behavior['attackRange']) {
      return;
    }

    this.behavior['moveToTarget']();
  }

  public exit(): void {
    this.chaseTimer = 0;
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'attack' || stateName === 'idle' || stateName === 'patrol';
  }
}

export class AttackState implements AIState {
  public name: string = 'attack';
  private behavior: BaseAIBehavior;
  private attackCooldown: number = 0;
  private attackRate: number = 1;

  constructor(behavior: BaseAIBehavior) {
    this.behavior = behavior;
  }

  public enter(): void {
    this.attackCooldown = 0;
  }

  public update(deltaTime: number): void {
    const target = this.behavior['target'];
    if (!target || !target.isAlive()) {
      this.behavior['target'] = null;
      return;
    }

    const distance = this.behavior['entity'].getDistanceTo(target);

    if (distance > this.behavior['attackRange'] * 1.5) {
      this.behavior['moveToTarget']();
      return;
    }

    if (distance > this.behavior['attackRange']) {
      this.behavior['moveToTarget']();
      return;
    }

    this.behavior['stopMovement']();

    if (this.attackCooldown <= 0) {
      this.behavior['attackTarget']();
      this.attackCooldown = this.attackRate;
    } else {
      this.attackCooldown -= deltaTime;
    }

    this.behavior['entity'].lookAt(target.position);
  }

  public exit(): void {
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'chase' || stateName === 'idle' || stateName === 'patrol';
  }

  public setAttackRate(rate: number): void {
    this.attackRate = rate;
  }
}

export class FleeState implements AIState {
  public name: string = 'flee';
  private behavior: BaseAIBehavior;
  private fleeTarget: Vector2D = { x: 0, y: 0 };

  constructor(behavior: BaseAIBehavior) {
    this.behavior = behavior;
  }

  public enter(): void {
  }

  public update(deltaTime: number): void {
    const target = this.behavior['target'];
    if (!target) {
      this.behavior['target'] = null;
      return;
    }

    const entity = this.behavior['entity'];
    const direction = entity.getDirectionTo(target);
    
    this.fleeTarget.x = entity.position.x - direction.x * 200;
    this.fleeTarget.y = entity.position.y - direction.y * 200;

    const dx = this.fleeTarget.x - entity.position.x;
    const dy = this.fleeTarget.y - entity.position.y;
    const distance = Math.sqrt(dx * dx + dy * dy);

    if (distance > 10) {
      entity.velocity.x = (dx / distance) * this.behavior['moveSpeed'] * 1.5;
      entity.velocity.y = (dy / distance) * this.behavior['moveSpeed'] * 1.5;
    } else {
      this.behavior['target'] = null;
    }
  }

  public exit(): void {
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'idle' || stateName === 'patrol';
  }
}

export class GuardState implements AIState {
  public name: string = 'guard';
  private behavior: BaseAIBehavior;
  private guardPosition: Vector2D;
  private guardRadius: number = 50;

  constructor(behavior: BaseAIBehavior, guardPosition: Vector2D) {
    this.behavior = behavior;
    this.guardPosition = guardPosition;
  }

  public enter(): void {
  }

  public update(deltaTime: number): void {
    const target = this.behavior['findTarget']();
    if (target) {
      this.behavior['target'] = target;
      return;
    }

    const entity = this.behavior['entity'];
    const dx = this.guardPosition.x - entity.position.x;
    const dy = this.guardPosition.y - entity.position.y;
    const distance = Math.sqrt(dx * dx + dy * dy);

    if (distance > this.guardRadius) {
      const direction = { x: dx / distance, y: dy / distance };
      entity.velocity.x = direction.x * this.behavior['moveSpeed'] * 0.5;
      entity.velocity.y = direction.y * this.behavior['moveSpeed'] * 0.5;
    } else {
      this.behavior['stopMovement']();
    }
  }

  public exit(): void {
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'chase' || stateName === 'attack' || stateName === 'idle';
  }

  public setGuardPosition(position: Vector2D): void {
    this.guardPosition = position;
  }

  public setGuardRadius(radius: number): void {
    this.guardRadius = radius;
  }
}

export class WanderState implements AIState {
  public name: string = 'wander';
  private behavior: BaseAIBehavior;
  private wanderTarget: Vector2D = { x: 0, y: 0 };
  private wanderTimer: number = 0;
  private wanderInterval: number = 3;
  private wanderRadius: number = 100;

  constructor(behavior: BaseAIBehavior) {
    this.behavior = behavior;
    this.pickNewWanderTarget();
  }

  public enter(): void {
    this.wanderTimer = 0;
  }

  public update(deltaTime: number): void {
    const target = this.behavior['findTarget']();
    if (target) {
      this.behavior['target'] = target;
      return;
    }

    this.wanderTimer += deltaTime;
    if (this.wanderTimer > this.wanderInterval) {
      this.pickNewWanderTarget();
      this.wanderTimer = 0;
    }

    const entity = this.behavior['entity'];
    const dx = this.wanderTarget.x - entity.position.x;
    const dy = this.wanderTarget.y - entity.position.y;
    const distance = Math.sqrt(dx * dx + dy * dy);

    if (distance > 10) {
      const direction = { x: dx / distance, y: dy / distance };
      entity.velocity.x = direction.x * this.behavior['moveSpeed'] * 0.3;
      entity.velocity.y = direction.y * this.behavior['moveSpeed'] * 0.3;
    } else {
      this.pickNewWanderTarget();
    }
  }

  private pickNewWanderTarget(): void {
    const entity = this.behavior['entity'];
    const angle = Math.random() * Math.PI * 2;
    const distance = Math.random() * this.wanderRadius;

    this.wanderTarget.x = entity.position.x + Math.cos(angle) * distance;
    this.wanderTarget.y = entity.position.y + Math.sin(angle) * distance;
  }

  public exit(): void {
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'idle' || stateName === 'patrol' || stateName === 'chase' || stateName === 'attack';
  }
}

export class SupportState implements AIState {
  public name: string = 'support';
  private behavior: BaseAIBehavior;
  private supportTarget: Entity | null = null;
  private supportRadius: number = 100;

  constructor(behavior: BaseAIBehavior) {
    this.behavior = behavior;
  }

  public enter(): void {
  }

  public update(deltaTime: number): void {
    if (!this.supportTarget || !this.supportTarget.isAlive()) {
      this.supportTarget = this.findAllyToSupport();
    }

    if (!this.supportTarget) {
      return;
    }

    const distance = this.behavior['entity'].getDistanceTo(this.supportTarget);

    if (distance > this.supportRadius * 1.5) {
      this.behavior['moveToTarget']();
    } else if (distance < this.supportRadius * 0.5) {
      const entity = this.behavior['entity'];
      const direction = entity.getDirectionTo(this.supportTarget);
      entity.velocity.x = -direction.x * this.behavior['moveSpeed'] * 0.5;
      entity.velocity.y = -direction.y * this.behavior['moveSpeed'] * 0.5;
    } else {
      this.behavior['stopMovement']();
    }

    const nearbyEnemies = this.behavior['entityManager'].getInRadius(
      this.supportTarget.position,
      this.behavior['attackRange']
    ).filter(e => e.team !== this.behavior['entity'].team && e.isAlive());

    if (nearbyEnemies.length > 0) {
      this.behavior['target'] = nearbyEnemies[0];
    }
  }

  private findAllyToSupport(): Entity | null {
    const allies = this.behavior['entityManager'].getInRadius(
      this.behavior['entity'].position,
      this.behavior['perceptionRange']
    ).filter(e => e.team === this.behavior['entity'].team && e.isAlive() && e.id !== this.behavior['entity'].id);

    let mostDamaged: Entity | null = null;
    let lowestHealth = Infinity;

    for (const ally of allies) {
      const healthPercent = ally.health / ally.maxHealth;
      if (healthPercent < lowestHealth) {
        lowestHealth = healthPercent;
        mostDamaged = ally;
      }
    }

    return mostDamaged;
  }

  public exit(): void {
    this.supportTarget = null;
  }

  public canTransitionTo(stateName: string): boolean {
    return stateName === 'attack' || stateName === 'chase' || stateName === 'idle';
  }
}

export class SquadAI {
  private members: Entity[] = [];
  private squadLeader: Entity | null = null;
  private formation: Formation = Formation.VEE;
  private formationSpacing: number = 30;
  private entityManager: EntityManager;

  constructor(entityManager: EntityManager) {
    this.entityManager = entityManager;
  }

  public addMember(entity: Entity): void {
    if (!this.squadLeader) {
      this.squadLeader = entity;
    }
    this.members.push(entity);
  }

  public removeMember(entityId: string): void {
    this.members = this.members.filter(m => m.id !== entityId);
    if (this.squadLeader?.id === entityId) {
      this.squadLeader = this.members[0] || null;
    }
  }

  public setFormation(formation: Formation): void {
    this.formation = formation;
  }

  public update(deltaTime: number): void {
    if (!this.squadLeader) return;

    const positions = this.calculateFormation();

    for (let i = 0; i < this.members.length; i++) {
      const member = this.members[i];
      if (member.id === this.squadLeader?.id) continue;

      const targetPos = positions[i];
      if (!targetPos) continue;

      const dx = targetPos.x - member.position.x;
      const dy = targetPos.y - member.position.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance > 10) {
        const direction = { x: dx / distance, y: dy / distance };
        member.velocity.x = direction.x * 50;
        member.velocity.y = direction.y * 50;
      } else {
        member.velocity.x = 0;
        member.velocity.y = 0;
      }
    }
  }

  private calculateFormation(): Vector2D[] {
    if (!this.squadLeader) return [];

    const positions: Vector2D[] = [];
    const leaderPos = this.squadLeader.position;

    switch (this.formation) {
      case Formation.LINE:
        for (let i = 1; i < this.members.length; i++) {
          positions.push({
            x: leaderPos.x - i * this.formationSpacing,
            y: leaderPos.y,
          });
        }
        break;

      case Formation.VEE:
        for (let i = 1; i < this.members.length; i++) {
          const side = i % 2 === 0 ? 1 : -1;
          const row = Math.ceil(i / 2);
          positions.push({
            x: leaderPos.x - row * this.formationSpacing,
            y: leaderPos.y + side * row * this.formationSpacing,
          });
        }
        break;

      case Formation.CIRCLE:
        for (let i = 1; i < this.members.length; i++) {
          const angle = (2 * Math.PI * i) / this.members.length;
          positions.push({
            x: leaderPos.x + Math.cos(angle) * this.formationSpacing,
            y: leaderPos.y + Math.sin(angle) * this.formationSpacing,
          });
        }
        break;

      case Formation.COLUMN:
        for (let i = 1; i < this.members.length; i++) {
          positions.push({
            x: leaderPos.x,
            y: leaderPos.y - i * this.formationSpacing,
          });
        }
        break;
    }

    return positions;
  }

  public getMembers(): Entity[] {
    return [...this.members];
  }

  public getLeader(): Entity | null {
    return this.squadLeader;
  }
}

export enum Formation {
  LINE,
  VEE,
  CIRCLE,
  COLUMN,
}

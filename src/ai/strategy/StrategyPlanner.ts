import { Entity, Vector2D } from '../../entities/Entity';
import { Unit } from '../units/UnitSystem';
import { Building } from '../buildings/BuildingSystem';

export interface StrategyGoal {
  id: string;
  type: 'attack' | 'defend' | 'economy' | 'tech' | 'scout';
  priority: number;
  target?: Entity;
  targetPosition?: Vector2D;
  status: 'active' | 'completed' | 'failed';
  createdAt: number;
  completedAt?: number;
}

export interface StrategyPlan {
  id: string;
  name: string;
  goals: StrategyGoal[];
  currentGoalIndex: number;
  status: 'planning' | 'executing' | 'completed' | 'aborted';
  startTime: number;
}

export interface AIController {
  team: number;
  personality: AIPersonality;
  resources: { credits: number; power: number; ore: number };
  militaryStrength: number;
  economicStrength: number;
  techLevel: number;
  aggressionLevel: number;
  defensiveLevel: number;
}

export interface AIPersonality {
  name: string;
  aggression: number;
  defensiveness: number;
  economicFocus: number;
  riskTolerance: number;
  responseSpeed: number;
}

export class StrategyPlanner {
  private controller: AIController;
  private plans: StrategyPlan[] = [];
  private activePlan: StrategyPlan | null = null;
  private goalQueue: StrategyGoal[] = [];

  constructor(controller: AIController) {
    this.controller = controller;
  }

  public createPlan(name: string): StrategyPlan {
    const plan: StrategyPlan = {
      id: `plan_${Date.now()}`,
      name,
      goals: [],
      currentGoalIndex: -1,
      status: 'planning',
      startTime: Date.now(),
    };

    this.plans.push(plan);
    return plan;
  }

  public addGoal(planId: string, goal: Omit<StrategyGoal, 'id' | 'status' | 'createdAt'>): void {
    const plan = this.plans.find(p => p.id === planId);
    if (!plan) return;

    const newGoal: StrategyGoal = {
      ...goal,
      id: `goal_${Date.now()}_${Math.random()}`,
      status: 'active',
      createdAt: Date.now(),
    };

    plan.goals.push(newGoal);
    this.sortGoalsByPriority(plan);
  }

  private sortGoalsByPriority(plan: StrategyPlan): void {
    plan.goals.sort((a, b) => b.priority - a.priority);
  }

  public activatePlan(planId: string): boolean {
    const plan = this.plans.find(p => p.id === planId);
    if (!plan || plan.status === 'completed' || plan.status === 'aborted') {
      return false;
    }

    this.activePlan = plan;
    plan.status = 'executing';
    plan.currentGoalIndex = 0;

    return true;
  }

  public update(): StrategyGoal | null {
    if (!this.activePlan) return null;

    if (this.activePlan.currentGoalIndex >= this.activePlan.goals.length) {
      this.activePlan.status = 'completed';
      this.activePlan = null;
      return null;
    }

    const currentGoal = this.activePlan.goals[this.activePlan.currentGoalIndex];

    if (currentGoal.status === 'completed') {
      currentGoal.completedAt = Date.now();
      this.activePlan.currentGoalIndex++;
      return null;
    }

    if (currentGoal.status === 'active') {
      return currentGoal;
    }

    return null;
  }

  public completeGoal(goalId: string): void {
    const goal = this.findGoal(goalId);
    if (goal) {
      goal.status = 'completed';
      goal.completedAt = Date.now();
    }
  }

  public failGoal(goalId: string): void {
    const goal = this.findGoal(goalId);
    if (goal) {
      goal.status = 'failed';
    }
  }

  private findGoal(goalId: string): StrategyGoal | undefined {
    for (const plan of this.plans) {
      const goal = plan.goals.find(g => g.id === goalId);
      if (goal) return goal;
    }
    return undefined;
  }

  public getActiveGoal(): StrategyGoal | null {
    if (!this.activePlan) return null;
    if (this.activePlan.currentGoalIndex < 0) return null;
    if (this.activePlan.currentGoalIndex >= this.activePlan.goals.length) return null;
    return this.activePlan.goals[this.activePlan.currentGoalIndex];
  }

  public getPlans(): StrategyPlan[] {
    return [...this.plans];
  }

  public getActivePlan(): StrategyPlan | null {
    return this.activePlan;
  }

  public clearPlans(): void {
    this.plans = [];
    this.activePlan = null;
  }
}

export class EconomicAI {
  private controller: AIController;
  private resourceManager: any;
  private buildQueue: Array<{ type: string; priority: number }> = [];

  constructor(controller: AIController, resourceManager: any) {
    this.controller = controller;
    this.resourceManager = resourceManager;
  }

  public update(deltaTime: number): void {
    this.evaluateBuildQueue();
    this.balancePower();
    this.manageResources();
  }

  private evaluateBuildQueue(): void {
    if (this.controller.resources.credits < 300) {
      this.addToQueue('refinery', 10);
    }

    if (this.controller.resources.power < 0) {
      this.addToQueue('power-plant', 15);
    }

    this.buildQueue.sort((a, b) => b.priority - a.priority);
  }

  private addToQueue(type: string, priority: number): void {
    if (!this.buildQueue.some(item => item.type === type)) {
      this.buildQueue.push({ type, priority });
    }
  }

  private balancePower(): void {
    if (this.controller.resources.power < 0) {
      console.log(`${this.controller.team}: Power deficit detected!`);
    }
  }

  private manageResources(): void {
    if (this.controller.resources.credits > 2000) {
      this.addToQueue('refinery', 5);
    }
  }

  public getNextBuild(): string | null {
    return this.buildQueue.shift()?.type || null;
  }

  public getBuildQueue(): Array<{ type: string; priority: number }> {
    return [...this.buildQueue];
  }
}

export class MilitaryAI {
  private controller: AIController;
  private attackWaveSize: number = 5;
  private attackCooldown: number = 0;
  private minAttackStrength: number = 1000;

  constructor(controller: AIController) {
    this.controller = controller;
  }

  public update(deltaTime: number): void {
    this.attackCooldown -= deltaTime;

    if (this.attackCooldown <= 0) {
      this.evaluateAttack();
    }
  }

  private evaluateAttack(): void {
    if (this.controller.militaryStrength < this.minAttackStrength) {
      return;
    }

    const aggression = this.controller.personality.aggression;

    if (aggression > 0.7 && this.controller.militaryStrength > this.minAttackStrength * 1.5) {
      this.launchAttack();
    }
  }

  public launchAttack(): void {
    console.log(`${this.controller.team}: Launching attack!`);
    this.attackCooldown = 120;
    this.controller.militaryStrength *= 0.6;
  }

  public setAttackCooldown(cooldown: number): void {
    this.attackCooldown = cooldown;
  }

  public getAttackCooldown(): number {
    return this.attackCooldown;
  }
}

export class ScoutAI {
  private controller: AIController;
  private scoutUnits: Unit[] = [];
  private exploredAreas: Set<string> = new Set();
  private lastScoutUpdate: number = 0;

  constructor(controller: AIController) {
    this.controller = controller;
  }

  public update(deltaTime: number): void {
    this.lastScoutUpdate += deltaTime;

    if (this.lastScoutUpdate > 5) {
      this.assignScoutMissions();
      this.lastScoutUpdate = 0;
    }
  }

  public addScout(unit: Unit): void {
    if (!this.scoutUnits.includes(unit)) {
      this.scoutUnits.push(unit);
    }
  }

  public removeScout(unitId: string): void {
    this.scoutUnits = this.scoutUnits.filter(s => s.id !== unitId);
  }

  private assignScoutMissions(): void {
    for (const scout of this.scoutUnits) {
      if (!scout.currentOrder) {
        const unexploredArea = this.findUnexploredArea();
        if (unexploredArea) {
          scout.giveOrder({
            type: 'move',
            targetPosition: unexploredArea,
            queued: false,
          });
        }
      }

      const key = `${Math.floor(scout.position.x / 100)},${Math.floor(scout.position.y / 100)}`;
      this.exploredAreas.add(key);
    }
  }

  private findUnexploredArea(): Vector2D | null {
    const mapSize = { width: 100, height: 100 };
    const gridSize = 10;

    for (let gx = 0; gx < mapSize.width / gridSize; gx++) {
      for (let gy = 0; gy < mapSize.height / gridSize; gy++) {
        const key = `${gx},${gy}`;
        if (!this.exploredAreas.has(key)) {
          return {
            x: gx * gridSize + gridSize / 2,
            y: gy * gridSize + gridSize / 2,
          };
        }
      }
    }

    return null;
  }

  public getExploredPercentage(): number {
    const mapSize = { width: 100, height: 100 };
    const gridSize = 10;
    const totalGrids = (mapSize.width / gridSize) * (mapSize.height / gridSize);
    return (this.exploredAreas.size / totalGrids) * 100;
  }
}

export class AIControllerFactory {
  public static create(
    team: number,
    personalityType: 'aggressive' | 'defensive' | 'economic' | 'balanced'
  ): AIController {
    const personalities: Record<string, AIPersonality> = {
      aggressive: {
        name: 'Aggressive',
        aggression: 0.9,
        defensiveness: 0.3,
        economicFocus: 0.4,
        riskTolerance: 0.8,
        responseSpeed: 0.9,
      },
      defensive: {
        name: 'Defensive',
        aggression: 0.3,
        defensiveness: 0.9,
        economicFocus: 0.5,
        riskTolerance: 0.3,
        responseSpeed: 0.7,
      },
      economic: {
        name: 'Economic',
        aggression: 0.4,
        defensiveness: 0.5,
        economicFocus: 0.9,
        riskTolerance: 0.5,
        responseSpeed: 0.6,
      },
      balanced: {
        name: 'Balanced',
        aggression: 0.6,
        defensiveness: 0.6,
        economicFocus: 0.6,
        riskTolerance: 0.6,
        responseSpeed: 0.7,
      },
    };

    return {
      team,
      personality: personalities[personalityType],
      resources: { credits: 1000, power: 100, ore: 500 },
      militaryStrength: 0,
      economicStrength: 0,
      techLevel: 1,
      aggressionLevel: personalities[personalityType].aggression,
      defensiveLevel: personalities[personalityType].defensiveness,
    };
  }
}

export class AIDifficultyScaler {
  public static scaleDifficulty(
    baseController: AIController,
    difficulty: 'easy' | 'normal' | 'hard' | 'expert'
  ): AIController {
    const multipliers: Record<string, number> = {
      easy: 0.5,
      normal: 1.0,
      hard: 1.5,
      expert: 2.0,
    };

    const mult = multipliers[difficulty];

    const scaled: AIController = {
      ...baseController,
      aggressionLevel: baseController.aggressionLevel * mult,
      defensiveLevel: baseController.defensiveLevel * mult,
    };

    return scaled;
  }
}

export class MultiAI {
  private controllers: Map<number, AIController> = new Map();
  private strategyPlanners: Map<number, StrategyPlanner> = new Map();
  private economicAIs: Map<number, EconomicAI> = new Map();
  private militaryAIs: Map<number, MilitaryAI> = new Map();
  private scoutAIs: Map<number, ScoutAI> = new Map();
  private resourceManagers: Map<number, any> = new Map();

  public addAI(team: number, personality: 'aggressive' | 'defensive' | 'economic' | 'balanced'): void {
    const controller = AIControllerFactory.create(team, personality);
    this.controllers.set(team, controller);

    this.strategyPlanners.set(team, new StrategyPlanner(controller));
    this.economicAIs.set(team, new EconomicAI(controller, this.resourceManagers.get(team)));
    this.militaryAIs.set(team, new MilitaryAI(controller));
    this.scoutAIs.set(team, new ScoutAI(controller));
  }

  public removeAI(team: number): void {
    this.controllers.delete(team);
    this.strategyPlanners.delete(team);
    this.economicAIs.delete(team);
    this.militaryAIs.delete(team);
    this.scoutAIs.delete(team);
  }

  public getController(team: number): AIController | undefined {
    return this.controllers.get(team);
  }

  public getAllControllers(): AIController[] {
    return Array.from(this.controllers.values());
  }

  public update(deltaTime: number): void {
    for (const [team, controller] of this.controllers) {
      const strategy = this.strategyPlanners.get(team);
      const economic = this.economicAIs.get(team);
      const military = this.militaryAIs.get(team);
      const scout = this.scoutAIs.get(team);

      strategy?.update();
      economic?.update(deltaTime);
      military?.update(deltaTime);
      scout?.update(deltaTime);
    }
  }

  public setResourceManager(team: number, manager: any): void {
    this.resourceManagers.set(team, manager);
    this.economicAIs.set(team, new EconomicAI(this.controllers.get(team)!, manager));
  }
}

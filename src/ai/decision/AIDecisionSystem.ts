export interface AIGoal {
  id: string;
  name: string;
  description: string;
  priority: number;
  utility: number;
  satisfaction: number;
  decayRate: number;
  subGoals: AIGoal[];
  preconditions: AICondition[];
  effects: AIEffect[];
  targetEntity?: string;
  targetPosition?: { x: number; y: number; z: number };
  timeConstraint?: number;
  progress: number;
  active: boolean;
  createdAt: number;
  lastUpdated: number;
}

export interface AICondition {
  type: 'entity' | 'resource' | 'territory' | 'combat' | 'time' | 'custom';
  target: string;
  comparison: 'equals' | 'not_equals' | 'greater' | 'less' | 'greater_equal' | 'less_equal';
  value: number | string | boolean;
  weight: number;
  callback?: () => boolean;
}

export interface AIEffect {
  type: 'increase' | 'decrease' | 'set' | 'toggle' | 'spawn' | 'destroy';
  target: string;
  value: number | string | boolean;
  duration?: number;
  probability: number;
}

export interface AIAction {
  id: string;
  name: string;
  description: string;
  utility: number;
  cost: number;
  duration: number;
  preconditions: AICondition[];
  effects: AIEffect[];
  targetTypes: ('entity' | 'position' | 'area' | 'none')[];
  animation?: string;
  sound?: string;
  requiresTarget: boolean;
  interruptible: boolean;
  repeatable: boolean;
  cooldown: number;
  lastExecuted: number;
  executedCount: number;
}

export interface BehaviorTreeNode {
  id: string;
  type: 'selector' | 'sequence' | 'parallel' | 'decorator' | 'action' | 'condition';
  name: string;
  children: BehaviorTreeNode[];
  parent?: string;
  status: 'idle' | 'running' | 'success' | 'failure';
  properties: Record<string, unknown>;
  decorators: BehaviorTreeDecorator[];
}

export interface BehaviorTreeDecorator {
  type: 'repeat' | 'inverter' | 'succeeder' | 'failer' | 'rate_limiter' | 'cooldown' | 'max_time';
  properties: Record<string, unknown>;
}

export interface DecisionTreeNode {
  id: string;
  type: 'decision' | 'action' | 'chance';
  condition?: AICondition;
  trueChild?: string;
  falseChild?: string;
  action?: AIAction;
  probability?: number;
  weight: number;
}

export class AIDecisionSystem {
  private goals: Map<string, AIGoal> = new Map();
  private actions: Map<string, AIAction> = new Map();
  private activeGoals: Set<string> = new Set();
  private goalHistory: AIGoal[] = [];
  private actionHistory: AIAction[] = [];
  private decisionTree: Map<string, DecisionTreeNode> = new Map();
  private utilities: Map<string, number> = new Map();
  private considerations: Map<string, number> = new Map();
  private weights: Map<string, number> = new Map();
  private tacticalMemory: Map<string, unknown> = new Map();
  private strategicPlans: StrategicPlan[] = [];
  private currentPlan?: StrategicPlan;
  private decisionCycle: number = 0;
  private maxGoals: number = 10;
  private goalUpdateInterval: number = 1000;
  private lastGoalUpdate: number = 0;
  private context: AIContext;

  constructor(context: AIContext) {
    this.context = context;
    this.initializeDefaultGoals();
    this.initializeDefaultActions();
  }

  private initializeDefaultGoals(): void {
    this.addGoal({
      id: 'survive',
      name: 'Survive',
      description: 'Maintain unit/building integrity',
      priority: 100,
      utility: 0,
      satisfaction: 100,
      decayRate: 0.1,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 100,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'defend',
      name: 'Defend Territory',
      description: 'Protect owned structures and units',
      priority: 90,
      utility: 0,
      satisfaction: 80,
      decayRate: 0.2,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'expand',
      name: 'Expand Territory',
      description: 'Build new structures and capture territory',
      priority: 70,
      utility: 0,
      satisfaction: 50,
      decayRate: 0.15,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'gather_resources',
      name: 'Gather Resources',
      description: 'Collect credits and ore for economy',
      priority: 80,
      utility: 0,
      satisfaction: 60,
      decayRate: 0.25,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'attack',
      name: 'Launch Attack',
      description: 'Engage enemy forces and structures',
      priority: 75,
      utility: 0,
      satisfaction: 40,
      decayRate: 0.1,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'build',
      name: 'Construct Buildings',
      description: 'Erect new structures for production and defense',
      priority: 65,
      utility: 0,
      satisfaction: 50,
      decayRate: 0.2,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'research',
      name: 'Research Technology',
      description: 'Advance tech tree for new units and abilities',
      priority: 60,
      utility: 0,
      satisfaction: 50,
      decayRate: 0.1,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'scout',
      name: 'Explore Map',
      description: 'Reveal fog of war and find enemy positions',
      priority: 55,
      utility: 0,
      satisfaction: 50,
      decayRate: 0.15,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'economy',
      name: 'Maintain Economy',
      description: 'Balance income and expenses',
      priority: 85,
      utility: 0,
      satisfaction: 70,
      decayRate: 0.2,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });

    this.addGoal({
      id: 'counter',
      name: 'Counter Enemy',
      description: 'Respond to enemy unit compositions',
      priority: 70,
      utility: 0,
      satisfaction: 50,
      decayRate: 0.15,
      subGoals: [],
      preconditions: [],
      effects: [],
      progress: 0,
      active: true,
      createdAt: Date.now(),
      lastUpdated: Date.now()
    });
  }

  private initializeDefaultActions(): void {
    this.addAction({
      id: 'move_to',
      name: 'Move To',
      description: 'Move unit to target position',
      utility: 0,
      cost: 10,
      duration: 1000,
      preconditions: [],
      effects: [],
      targetTypes: ['position'],
      requiresTarget: true,
      interruptible: true,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'attack_entity',
      name: 'Attack Entity',
      description: 'Attack target entity',
      utility: 0,
      cost: 20,
      duration: 500,
      preconditions: [],
      effects: [],
      targetTypes: ['entity'],
      requiresTarget: true,
      interruptible: false,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'build_structure',
      name: 'Build Structure',
      description: 'Construct a building',
      utility: 0,
      cost: 50,
      duration: 5000,
      preconditions: [],
      effects: [],
      targetTypes: ['position'],
      requiresTarget: true,
      interruptible: false,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'harvest',
      name: 'Harvest Resources',
      description: 'Gather resources from ore nodes',
      utility: 0,
      cost: 5,
      duration: 3000,
      preconditions: [],
      effects: [],
      targetTypes: ['entity'],
      requiresTarget: true,
      interruptible: true,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'patrol',
      name: 'Patrol Area',
      description: 'Patrol between waypoints',
      utility: 0,
      cost: 15,
      duration: 10000,
      preconditions: [],
      effects: [],
      targetTypes: ['area'],
      requiresTarget: true,
      interruptible: true,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'defend_position',
      name: 'Defend Position',
      description: 'Hold position and attack enemies',
      utility: 0,
      cost: 10,
      duration: 5000,
      preconditions: [],
      effects: [],
      targetTypes: ['position'],
      requiresTarget: true,
      interruptible: true,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'retreat',
      name: 'Retreat',
      description: 'Move to safe location',
      utility: 0,
      cost: 5,
      duration: 3000,
      preconditions: [],
      effects: [],
      targetTypes: ['position'],
      requiresTarget: false,
      interruptible: false,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'scout_area',
      name: 'Scout Area',
      description: 'Explore and reveal area',
      utility: 0,
      cost: 10,
      duration: 5000,
      preconditions: [],
      effects: [],
      targetTypes: ['position'],
      requiresTarget: true,
      interruptible: true,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'heal',
      name: 'Heal',
      description: 'Repair damaged units',
      utility: 0,
      cost: 30,
      duration: 2000,
      preconditions: [],
      effects: [],
      targetTypes: ['entity'],
      requiresTarget: true,
      interruptible: true,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });

    this.addAction({
      id: 'produce_unit',
      name: 'Produce Unit',
      description: 'Train new unit from structure',
      utility: 0,
      cost: 40,
      duration: 3000,
      preconditions: [],
      effects: [],
      targetTypes: ['none'],
      requiresTarget: false,
      interruptible: false,
      repeatable: true,
      cooldown: 0,
      lastExecuted: 0,
      executedCount: 0
    });
  }

  addGoal(goal: AIGoal): void {
    this.goals.set(goal.id, goal);
    this.utilities.set(goal.id, goal.utility);
    this.considerations.set(goal.id, 1.0);
    this.weights.set(goal.id, goal.priority / 100);
  }

  addAction(action: AIAction): void {
    this.actions.set(action.id, action);
  }

  getGoal(id: string): AIGoal | undefined {
    return this.goals.get(id);
  }

  getAction(id: string): AIAction | undefined {
    return this.actions.get(id);
  }

  getAllGoals(): AIGoal[] {
    return Array.from(this.goals.values());
  }

  getAllActions(): AIAction[] {
    return Array.from(this.actions.values());
  }

  calculateUtility(goalId: string): number {
    const goal = this.goals.get(goalId);
    if (!goal) return 0;

    let utility = 0;
    let totalWeight = 0;

    const urgencyFactor = 1 - (goal.satisfaction / 100);
    utility += goal.priority * urgencyFactor * this.weights.get(goalId)!;

    const timeSinceUpdate = Date.now() - goal.lastUpdated;
    const decayFactor = Math.exp(-goal.decayRate * timeSinceUpdate / 1000);
    utility *= (1 + decayFactor);

    goal.subGoals.forEach(subGoal => {
      const subUtility = this.calculateUtility(subGoal.id);
      utility += subUtility * 0.5;
    });

    goal.preconditions.forEach(condition => {
      const conditionMet = this.evaluateCondition(condition);
      if (conditionMet) {
        utility *= (1 + condition.weight);
      } else {
        utility *= (1 - condition.weight * 0.5);
      }
    });

    utility *= this.considerations.get(goalId)!;

    return Math.max(0, Math.min(100, utility));
  }

  evaluateCondition(condition: AICondition): boolean {
    const targetValue = this.getConditionTargetValue(condition);

    switch (condition.comparison) {
      case 'equals':
        return targetValue === condition.value;
      case 'not_equals':
        return targetValue !== condition.value;
      case 'greater':
        return typeof targetValue === 'number' && typeof condition.value === 'number' && targetValue > condition.value;
      case 'less':
        return typeof targetValue === 'number' && typeof condition.value === 'number' && targetValue < condition.value;
      case 'greater_equal':
        return typeof targetValue === 'number' && typeof condition.value === 'number' && targetValue >= condition.value;
      case 'less_equal':
        return typeof targetValue === 'number' && typeof condition.value === 'number' && targetValue <= condition.value;
      default:
        return false;
    }
  }

  private getConditionTargetValue(condition: AICondition): number | string | boolean {
    if (condition.callback) {
      return condition.callback();
    }

    switch (condition.type) {
      case 'entity':
        return this.context.entities.get(condition.target)?.health || 0;
      case 'resource':
        return this.context.resources.get(condition.target) || 0;
      case 'territory':
        return this.context.territory.get(condition.target) || 0;
      case 'combat':
        return this.context.combatMetrics.get(condition.target) || 0;
      case 'time':
        return Date.now() - this.context.gameStartTime;
      default:
        return this.tacticalMemory.get(condition.target) as number | string | boolean || 0;
    }
  }

  updateGoals(): void {
    const now = Date.now();
    if (now - this.lastGoalUpdate < this.goalUpdateInterval) {
      return;
    }

    this.lastGoalUpdate = now;
    this.decisionCycle++;

    this.goals.forEach((goal, id) => {
      const oldUtility = goal.utility;
      goal.utility = this.calculateUtility(id);
      goal.lastUpdated = now;

      if (Math.abs(goal.utility - oldUtility) > 10) {
        this.recordGoalChange(id, oldUtility, goal.utility);
      }
    });

    this.normalizeUtilities();

    const sortedGoals = this.getAllGoals().sort((a, b) => b.utility - a.utility);

    this.activeGoals.clear();
    let count = 0;
    for (const goal of sortedGoals) {
      if (count >= this.maxGoals) break;
      if (goal.utility > 20 && goal.active) {
        this.activeGoals.add(goal.id);
        count++;
      }
    }

    this.updateStrategicPlans();
  }

  private normalizeUtilities(): void {
    let totalUtility = 0;
    this.goals.forEach(goal => {
      totalUtility += goal.utility;
    });

    if (totalUtility > 0) {
      this.goals.forEach((goal, id) => {
        goal.utility = (goal.utility / totalUtility) * 100;
        this.utilities.set(id, goal.utility);
      });
    }
  }

  private recordGoalChange(goalId: string, oldUtility: number, newUtility: number): void {
    this.tacticalMemory.set(`goal_change_${goalId}_${this.decisionCycle}`, {
      oldValue: oldUtility,
      newValue: newUtility,
      timestamp: Date.now()
    });
  }

  private updateStrategicPlans(): void {
    const topGoals = Array.from(this.activeGoals)
      .map(id => this.goals.get(id)!)
      .filter(g => g)
      .sort((a, b) => b.utility - a.utility)
      .slice(0, 3);

    const currentPlanUtility = this.currentPlan ?
      this.evaluatePlanUtility(this.currentPlan) : 0;

    const potentialPlans = this.generatePotentialPlans(topGoals);

    let bestPlan = this.currentPlan;
    let bestUtility = currentPlanUtility;

    for (const plan of potentialPlans) {
      const utility = this.evaluatePlanUtility(plan);
      if (utility > bestUtility) {
        bestPlan = plan;
        bestUtility = utility;
      }
    }

    if (bestPlan && bestPlan !== this.currentPlan) {
      this.strategicPlans.push(this.currentPlan!);
      this.currentPlan = bestPlan;
    }
  }

  private generatePotentialPlans(goals: AIGoal[]): StrategicPlan[] {
    const plans: StrategicPlan[] = [];

    goals.forEach(goal => {
      const applicableActions = this.getApplicableActions(goal);

      applicableActions.forEach(action => {
        const plan: StrategicPlan = {
          id: `plan_${Date.now()}_${Math.random()}`,
          name: `${goal.name} via ${action.name}`,
          goals: [goal.id],
          actions: [action.id],
          utility: 0,
          feasibility: 0,
          expectedOutcome: '',
          timeline: [],
          contingencies: []
        };

        this.assessPlan(plan);
        plans.push(plan);
      });
    });

    return plans;
  }

  private getApplicableActions(goal: AIGoal): AIAction[] {
    const applicable: AIAction[] = [];

    this.actions.forEach(action => {
      let allPreconditionsMet = true;

      for (const precondition of action.preconditions) {
        if (!this.evaluateCondition(precondition)) {
          allPreconditionsMet = false;
          break;
        }
      }

      if (allPreconditionsMet) {
        action.utility = this.calculateActionUtility(action, goal);
        applicable.push(action);
      }
    });

    return applicable.sort((a, b) => b.utility - a.utility);
  }

  private calculateActionUtility(action: AIAction, goal: AIGoal): number {
    let utility = 100 - action.cost;

    action.effects.forEach(effect => {
      const relevance = this.getEffectRelevance(effect, goal);
      utility += relevance * effect.probability * 50;
    });

    const cooldownFactor = Math.max(0, 1 - (Date.now() - action.lastExecuted) / (action.cooldown + 1000));
    utility *= cooldownFactor;

    return utility;
  }

  private getEffectRelevance(effect: AIEffect, goal: AIGoal): number {
    for (const goalEffect of goal.effects) {
      if (goalEffect.target === effect.target && goalEffect.type === effect.type) {
        return 1.0;
      }
    }
    return 0.3;
  }

  private evaluatePlanUtility(plan: StrategicPlan): number {
    let utility = 0;

    plan.goals.forEach(goalId => {
      const goal = this.goals.get(goalId);
      if (goal) {
        utility += goal.utility * 0.7;
      }
    });

    plan.actions.forEach(actionId => {
      const action = this.actions.get(actionId);
      if (action) {
        utility += action.utility * 0.3;
      }
    });

    utility *= plan.feasibility;

    return utility;
  }

  private assessPlan(plan: StrategicPlan): void {
    plan.feasibility = this.calculateFeasibility(plan);
    plan.utility = this.evaluatePlanUtility(plan);
    plan.expectedOutcome = this.generateExpectedOutcome(plan);

    plan.timeline = this.generateTimeline(plan);
    plan.contingencies = this.generateContingencies(plan);
  }

  private calculateFeasibility(plan: StrategicPlan): number {
    let feasibility = 1.0;

    plan.actions.forEach(actionId => {
      const action = this.actions.get(actionId);
      if (action) {
        const available = this.context.availableResources >= action.cost;
        if (!available) {
          feasibility *= 0.5;
        }

        const cooldownElapsed = Date.now() - action.lastExecuted >= action.cooldown;
        if (!cooldownElapsed) {
          feasibility *= 0.7;
        }
      }
    });

    return feasibility;
  }

  private generateExpectedOutcome(plan: StrategicPlan): string {
    const goalNames = plan.goals.map(id => this.goals.get(id)?.name || 'Unknown').join(', ');
    const actionNames = plan.actions.map(id => this.actions.get(id)?.name || 'Unknown').join(' -> ');

    return `Achieve ${goalNames} through ${actionNames}`;
  }

  private generateTimeline(plan: StrategicPlan): TimelineEvent[] {
    const timeline: TimelineEvent[] = [];
    let currentTime = Date.now();

    plan.actions.forEach(actionId => {
      const action = this.actions.get(actionId);
      if (action) {
        timeline.push({
          time: currentTime,
          action: actionId,
          description: action.name,
          duration: action.duration,
          expectedResult: 'Action completed'
        });
        currentTime += action.duration;
      }
    });

    return timeline;
  }

  private generateContingencies(plan: StrategicPlan): Contingency[] {
    const contingencies: Contingency[] = [];

    contingencies.push({
      trigger: 'action_failed',
      alternativePlan: this.generateAlternativePlan(plan),
      priority: 1
    });

    contingencies.push({
      trigger: 'goal_achieved_early',
      alternativePlan: this.generateNextGoalPlan(plan),
      priority: 2
    });

    contingencies.push({
      trigger: 'resources_insufficient',
      alternativePlan: this.generateResourcePlan(plan),
      priority: 3
    });

    return contingencies;
  }

  private generateAlternativePlan(plan: StrategicPlan): StrategicPlan {
    return {
      id: `alt_${plan.id}`,
      name: `Alternative to ${plan.name}`,
      goals: plan.goals,
      actions: plan.actions.slice(1),
      utility: plan.utility * 0.8,
      feasibility: plan.feasibility * 0.9,
      expectedOutcome: 'Fallback approach',
      timeline: [],
      contingencies: []
    };
  }

  private generateNextGoalPlan(plan: StrategicPlan): StrategicPlan {
    const nextGoal = Array.from(this.activeGoals)
      .map(id => this.goals.get(id)!)
      .filter(g => !plan.goals.includes(g.id))
      .sort((a, b) => b.utility - a.utility)[0];

    return {
      id: `next_${plan.id}`,
      name: `Continue to ${nextGoal?.name || 'next objective'}`,
      goals: nextGoal ? [nextGoal.id] : [],
      actions: [],
      utility: nextGoal?.utility || 0,
      feasibility: 0.5,
      expectedOutcome: 'Advance to next goal',
      timeline: [],
      contingencies: []
    };
  }

  private generateResourcePlan(plan: StrategicPlan): StrategicPlan {
    return {
      id: `res_${plan.id}`,
      name: `Resource-efficient ${plan.name}`,
      goals: plan.goals,
      actions: plan.actions.map(actionId => {
        const action = this.actions.get(actionId);
        if (action) {
          return { ...action, cost: Math.floor(action.cost * 0.7) } as AIAction;
        }
        return actionId;
      }) as string[],
      utility: plan.utility * 0.6,
      feasibility: plan.feasibility * 0.5,
      expectedOutcome: 'Reduced resource usage',
      timeline: [],
      contingencies: []
    };
  }

  selectBestAction(goalId: string): AIAction | undefined {
    const goal = this.goals.get(goalId);
    if (!goal) return undefined;

    const applicableActions = this.getApplicableActions(goal);
    if (applicableActions.length === 0) return undefined;

    return applicableActions[0];
  }

  executeDecisionCycle(): DecisionResult {
    this.updateGoals();

    const activeGoalsList = Array.from(this.activeGoals)
      .map(id => this.goals.get(id)!)
      .filter(g => g)
      .sort((a, b) => b.utility - a.utility);

    if (activeGoalsList.length === 0) {
      return { success: false, action: undefined, reason: 'No active goals' };
    }

    const topGoal = activeGoalsList[0];
    const action = this.selectBestAction(topGoal.id);

    if (!action) {
      return { success: false, action: undefined, reason: 'No applicable action' };
    }

    if (action.cooldown > 0 && Date.now() - action.lastExecuted < action.cooldown) {
      return { success: false, action: undefined, reason: 'Action on cooldown' };
    }

    if (this.context.availableResources < action.cost) {
      return { success: false, action: undefined, reason: 'Insufficient resources' };
    }

    action.lastExecuted = Date.now();
    action.executedCount++;
    this.actionHistory.push(action);

    topGoal.progress += action.utility / 10;
    topGoal.satisfaction = Math.min(100, topGoal.satisfaction + 10);

    return {
      success: true,
      action: action,
      goal: topGoal,
      utility: action.utility,
      reason: `Executing ${action.name} for ${topGoal.name}`
    };
  }

  getDecisionTree(): Map<string, DecisionTreeNode> {
    return this.decisionTree;
  }

  addDecisionNode(node: DecisionTreeNode): void {
    this.decisionTree.set(node.id, node);
  }

  buildDecisionTree(): void {
    this.decisionTree.clear();

    let nodeId = 0;

    const rootNode: DecisionTreeNode = {
      id: `decision_${nodeId++}`,
      type: 'decision',
      condition: {
        type: 'resource',
        target: 'credits',
        comparison: 'greater',
        value: 100,
        weight: 1.0
      },
      trueChild: `decision_${nodeId}`,
      falseChild: `action_${this.actions.get('harvest')?.id || 'harvest'}`,
      weight: 1.0
    };

    this.decisionTree.set(rootNode.id, rootNode);

    const resourceNode: DecisionTreeNode = {
      id: `decision_${nodeId}`,
      type: 'decision',
      condition: {
        type: 'combat',
        target: 'enemy_threat',
        comparison: 'greater',
        value: 50,
        weight: 1.0
      },
      trueChild: `action_${this.actions.get('defend_position')?.id || 'defend_position'}`,
      falseChild: `decision_${nodeId + 1}`,
      weight: 0.8
    };

    this.decisionTree.set(resourceNode.id, resourceNode);

    const expansionNode: DecisionTreeNode = {
      id: `decision_${nodeId + 1}`,
      type: 'decision',
      condition: {
        type: 'territory',
        target: 'controlled',
        comparison: 'less',
        value: 30,
        weight: 1.0
      },
      trueChild: `action_${this.actions.get('expand')?.id || 'expand'}`,
      falseChild: `action_${this.actions.get('attack_entity')?.id || 'attack_entity'}`,
      weight: 0.6
    };

    this.decisionTree.set(expansionNode.id, expansionNode);
  }

  traverseDecisionTree(startId: string): AIAction | undefined {
    const node = this.decisionTree.get(startId);
    if (!node) return undefined;

    if (node.type === 'action' && node.action) {
      return node.action;
    }

    if (node.type === 'decision' && node.condition) {
      const conditionMet = this.evaluateCondition(node.condition);
      const nextId = conditionMet ? node.trueChild : node.falseChild;
      if (nextId) {
        return this.traverseDecisionTree(nextId);
      }
    }

    if (node.type === 'chance' && node.probability) {
      if (Math.random() < node.probability && node.trueChild) {
        return this.traverseDecisionTree(node.trueChild);
      } else if (node.falseChild) {
        return this.traverseDecisionTree(node.falseChild);
      }
    }

    return undefined;
  }

  getUtilityBreakdown(goalId: string): UtilityBreakdown {
    const goal = this.goals.get(goalId);
    if (!goal) {
      return { factors: [], total: 0 };
    }

    const factors: UtilityFactor[] = [];

    const urgencyFactor: UtilityFactor = {
      name: 'Urgency',
      value: (1 - goal.satisfaction / 100) * goal.priority,
      weight: 1.0,
      contribution: 0
    };
    factors.push(urgencyFactor);

    const timeSinceUpdate = Date.now() - goal.lastUpdated;
    const decayFactor: UtilityFactor = {
      name: 'Decay',
      value: Math.exp(-goal.decayRate * timeSinceUpdate / 1000) * 50,
      weight: 0.5,
      contribution: 0
    };
    factors.push(decayFactor);

    goal.preconditions.forEach((condition, index) => {
      const conditionMet = this.evaluateCondition(condition);
      const factor: UtilityFactor = {
        name: `Condition ${index + 1}`,
        value: conditionMet ? condition.weight * 30 : -condition.weight * 15,
        weight: condition.weight,
        contribution: 0
      };
      factors.push(factor);
    });

    let total = 0;
    factors.forEach(factor => {
      factor.contribution = factor.value * factor.weight;
      total += factor.contribution;
    });

    return { factors, total };
  }

  getStrategicAnalysis(): StrategicAnalysis {
    return {
      goals: this.getAllGoals().sort((a, b) => b.utility - a.utility),
      currentPlan: this.currentPlan,
      recentDecisions: this.actionHistory.slice(-10),
      goalHistory: this.goalHistory.slice(-10),
      utilityTrends: this.calculateUtilityTrends(),
      recommendations: this.generateRecommendations()
    };
  }

  private calculateUtilityTrends(): Map<string, number[]> {
    const trends = new Map<string, number[]>();

    this.goals.forEach((goal, id) => {
      const recentUtilities: number[] = [];
      for (let i = 0; i < 10; i++) {
        const key = `goal_change_${id}_${this.decisionCycle - i}`;
        const change = this.tacticalMemory.get(key) as { newValue: number } | undefined;
        if (change) {
          recentUtilities.push(change.newValue);
        }
      }
      if (recentUtilities.length > 0) {
        trends.set(id, recentUtilities);
      }
    });

    return trends;
  }

  private generateRecommendations(): AIRecommendation[] {
    const recommendations: AIRecommendation[] = [];

    const resourceGoal = this.goals.get('economy');
    if (resourceGoal && resourceGoal.utility > 70) {
      recommendations.push({
        priority: 'high',
        category: 'economy',
        suggestion: 'Increase resource gathering operations',
        rationale: 'Economy goal utility is high',
        expectedImpact: 0.8
      });
    }

    const attackGoal = this.goals.get('attack');
    if (attackGoal && attackGoal.utility > 80) {
      recommendations.push({
        priority: 'high',
        category: 'combat',
        suggestion: 'Launch offensive attack',
        rationale: 'Attack goal utility exceeds threshold',
        expectedImpact: 0.9
      });
    }

    const defendGoal = this.goals.get('defend');
    if (defendGoal && defendGoal.utility > 75) {
      recommendations.push({
        priority: 'medium',
        category: 'defense',
        suggestion: 'Reinforce defensive structures',
        rationale: 'Defensive needs are elevated',
        expectedImpact: 0.7
      });
    }

    return recommendations;
  }

  setGoalPriority(goalId: string, priority: number): void {
    const goal = this.goals.get(goalId);
    if (goal) {
      goal.priority = Math.max(0, Math.min(100, priority));
      this.weights.set(goalId, goal.priority / 100);
    }
  }

  setConsideration(goalId: string, consideration: number): void {
    this.considerations.set(goalId, Math.max(0.1, Math.min(2.0, consideration)));
  }

  addSubGoal(parentGoalId: string, subGoal: AIGoal): boolean {
    const parentGoal = this.goals.get(parentGoalId);
    if (!parentGoal) return false;

    this.addGoal(subGoal);
    parentGoal.subGoals.push(subGoal);
    return true;
  }

  removeGoal(goalId: string): boolean {
    const goal = this.goals.get(goalId);
    if (!goal) return false;

    this.goals.forEach(g => {
      g.subGoals = g.subGoals.filter(sg => sg.id !== goalId);
    });

    this.goals.delete(goalId);
    this.activeGoals.delete(goalId);
    this.goalHistory.push(goal);

    return true;
  }

  serialize(): AIDecisionSystemState {
    return {
      goals: Array.from(this.goals.values()),
      activeGoals: Array.from(this.activeGoals),
      goalHistory: this.goalHistory.slice(-100),
      actionHistory: this.actionHistory.slice(-100),
      strategicPlans: this.strategicPlans.slice(-10),
      currentPlan: this.currentPlan,
      decisionCycle: this.decisionCycle,
      tacticalMemory: Array.from(this.tacticalMemory.entries())
    };
  }

  deserialize(state: AIDecisionSystemState): void {
    this.goals.clear();
    state.goals.forEach(goal => {
      this.goals.set(goal.id, goal);
      this.utilities.set(goal.id, goal.utility);
    });

    this.activeGoals = new Set(state.activeGoals);
    this.goalHistory = state.goalHistory;
    this.actionHistory = state.actionHistory;
    this.strategicPlans = state.strategicPlans;
    this.currentPlan = state.currentPlan;
    this.decisionCycle = state.decisionCycle;

    this.tacticalMemory.clear();
    state.tacticalMemory.forEach(([key, value]) => {
      this.tacticalMemory.set(key, value);
    });
  }

  reset(): void {
    this.goals.clear();
    this.actions.clear();
    this.activeGoals.clear();
    this.goalHistory = [];
    this.actionHistory = [];
    this.decisionTree.clear();
    this.utilities.clear();
    this.considerations.clear();
    this.weights.clear();
    this.tacticalMemory.clear();
    this.strategicPlans = [];
    this.currentPlan = undefined;
    this.decisionCycle = 0;

    this.initializeDefaultGoals();
    this.initializeDefaultActions();
  }
}

export interface AIContext {
  entities: Map<string, { health: number; position: { x: number; y: number; z: number } }>;
  resources: Map<string, number>;
  territory: Map<string, number>;
  combatMetrics: Map<string, number>;
  gameStartTime: number;
  availableResources: number;
}

export interface StrategicPlan {
  id: string;
  name: string;
  goals: string[];
  actions: string[];
  utility: number;
  feasibility: number;
  expectedOutcome: string;
  timeline: TimelineEvent[];
  contingencies: Contingency[];
}

export interface TimelineEvent {
  time: number;
  action: string;
  description: string;
  duration: number;
  expectedResult: string;
}

export interface Contingency {
  trigger: string;
  alternativePlan: StrategicPlan;
  priority: number;
}

export interface DecisionResult {
  success: boolean;
  action?: AIAction;
  goal?: AIGoal;
  utility?: number;
  reason: string;
}

export interface UtilityBreakdown {
  factors: UtilityFactor[];
  total: number;
}

export interface UtilityFactor {
  name: string;
  value: number;
  weight: number;
  contribution: number;
}

export interface StrategicAnalysis {
  goals: AIGoal[];
  currentPlan?: StrategicPlan;
  recentDecisions: AIAction[];
  goalHistory: AIGoal[];
  utilityTrends: Map<string, number[]>;
  recommendations: AIRecommendation[];
}

export interface AIRecommendation {
  priority: 'high' | 'medium' | 'low';
  category: string;
  suggestion: string;
  rationale: string;
  expectedImpact: number;
}

export interface AIDecisionSystemState {
  goals: AIGoal[];
  activeGoals: string[];
  goalHistory: AIGoal[];
  actionHistory: AIAction[];
  strategicPlans: StrategicPlan[];
  currentPlan?: StrategicPlan;
  decisionCycle: number;
  tacticalMemory: [string, unknown][];
}

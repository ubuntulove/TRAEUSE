import { Vector2, Vector3 } from '../../core/Math';

export interface CommandTarget {
  readonly id: string;
  readonly type: 'unit' | 'building' | 'resource' | 'location' | 'group';
  readonly position?: Vector3;
  readonly entity?: string;
}

export interface CommandQueue {
  readonly commands: Command[];
  readonly maxSize: number;
  enqueue(command: Command): boolean;
  dequeue(): Command | null;
  peek(): Command | null;
  clear(): void;
  insert(index: number, command: Command): boolean;
  remove(index: number): Command | null;
  reorder(fromIndex: number, toIndex: number): boolean;
}

export interface Command {
  readonly id: string;
  readonly type: CommandType;
  readonly target: CommandTarget;
  readonly position?: Vector3;
  readonly waypoints?: Vector3[];
  readonly priority: CommandPriority;
  readonly cost: CommandCost;
  readonly context: CommandContext;
  readonly timestamp: number;
  readonly source: 'player' | 'ai' | 'script' | 'ability';
  execute(executor: CommandExecutor): CommandResult;
  canExecute(context: CommandContext): boolean;
  validate(context: CommandContext): ValidationResult;
  toJSON(): object;
}

export enum CommandType {
  Move = 'move',
  Attack = 'attack',
  AttackMove = 'attack_move',
  Patrol = 'patrol',
  Hold = 'hold',
  Garrison = 'garrison',
  Ungarrison = 'ungarrison',
  Repair = 'repair',
  Build = 'build',
  Train = 'train',
  Research = 'research',
  Rally = 'rally',
  SetGatherPoint = 'set_gather_point',
  Cancel = 'cancel',
  Stop = 'stop',
  Ability = 'ability',
  GroupAssign = 'group_assign',
  GroupRecall = 'group_recall',
  formations = 'formations',
  Follow = 'follow',
  Guard = 'guard',
  Scout = 'scout',
  Harvest = 'harvest',
  ReturnResources = 'return_resources',
  Reinforce = 'reinforce',
  Morph = 'morph',
  Upgrade = 'upgrade',
  Sell = 'sell',
  Buy = 'buy',
  Trade = 'trade',
  Diplomacy = 'diplomacy',
  Gift = 'gift',
  Ping = 'ping',
  AttackGround = 'attack_ground',
  PlaceBuilding = 'place_building',
}

export enum CommandPriority {
  Immediate = 0,
  Critical = 1,
  High = 2,
  Normal = 3,
  Low = 4,
  Queued = 5,
}

export interface CommandCost {
  readonly food?: number;
  readonly wood?: number;
  readonly gold?: number;
  readonly stone?: number;
  readonly population?: number;
  readonly time?: number;
}

export interface CommandContext {
  readonly playerId: string;
  readonly selectedUnits: string[];
  readonly selectedBuildings: string[];
  readonly gameTime: number;
  readonly mapBounds: { min: Vector3; max: Vector3 };
  readonly visibility: Map<string, boolean>;
  readonly resources: ResourceState;
  readonly technology: TechnologyState;
  readonly gameMode: 'singleplayer' | 'multiplayer' | 'campaign';
}

export interface ResourceState {
  food: number;
  wood: number;
  gold: number;
  stone: number;
  population: number;
  populationCap: number;
}

export interface TechnologyState {
  researched: Set<string>;
  inProgress: Map<string, number>;
}

export interface CommandResult {
  success: boolean;
  message?: string;
  executedCommands?: Command[];
  failedReason?: string;
  resourceCost?: CommandCost;
  timeCost?: number;
  sideEffects?: SideEffect[];
}

export interface SideEffect {
  type: string;
  targetId: string;
  data: Record<string, unknown>;
}

export interface ValidationResult {
  valid: boolean;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

export interface ValidationError {
  code: string;
  message: string;
  field?: string;
}

export interface ValidationWarning {
  code: string;
  message: string;
  field?: string;
}

export class BaseCommand implements Command {
  public readonly id: string;
  public readonly type: CommandType;
  public readonly target: CommandTarget;
  public readonly position?: Vector3;
  public readonly waypoints?: Vector3[];
  public readonly priority: CommandPriority;
  public readonly cost: CommandCost;
  public readonly context: CommandContext;
  public readonly timestamp: number;
  public readonly source: 'player' | 'ai' | 'script' | 'ability';

  constructor(
    type: CommandType,
    target: CommandTarget,
    options: {
      position?: Vector3;
      waypoints?: Vector3[];
      priority?: CommandPriority;
      cost?: CommandCost;
      context: CommandContext;
      source?: 'player' | 'ai' | 'script' | 'ability';
    }
  ) {
    this.id = this.generateId();
    this.type = type;
    this.target = target;
    this.position = options.position;
    this.waypoints = options.waypoints;
    this.priority = options.priority ?? CommandPriority.Normal;
    this.cost = options.cost ?? {};
    this.context = options.context;
    this.timestamp = Date.now();
    this.source = options.source ?? 'player';
  }

  private generateId(): string {
    return `cmd_${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;
  }

  execute(executor: CommandExecutor): CommandResult {
    throw new Error('Execute method must be implemented by subclass');
  }

  canExecute(context: CommandContext): boolean {
    return true;
  }

  validate(context: CommandContext): ValidationResult {
    return { valid: true, errors: [], warnings: [] };
  }

  toJSON(): object {
    return {
      id: this.id,
      type: this.type,
      target: this.target,
      position: this.position,
      waypoints: this.waypoints,
      priority: this.priority,
      cost: this.cost,
      timestamp: this.timestamp,
      source: this.source,
    };
  }
}

export class MoveCommand extends BaseCommand {
  public readonly type = CommandType.Move;

  constructor(
    target: CommandTarget,
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority; waypoints?: Vector3[] }
  ) {
    super(CommandType.Move, target, {
      position,
      context,
      priority: options?.priority,
      waypoints: options?.waypoints,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return {
        success: false,
        failedReason: 'Cannot execute move command',
      };
    }

    const path = executor.calculatePath(this.context.selectedUnits, this.position!);
    
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'path_requested',
          targetId: this.context.selectedUnits.join(','),
          data: { path, destination: this.position },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return context.selectedUnits.length > 0 && this.position !== undefined;
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];
    const warnings: ValidationWarning[] = [];

    if (context.selectedUnits.length === 0) {
      errors.push({
        code: 'NO_UNITS_SELECTED',
        message: 'No units selected for move command',
      });
    }

    if (!this.position) {
      errors.push({
        code: 'NO_DESTINATION',
        message: 'No destination specified for move command',
      });
    }

    if (this.position && !this.isWithinBounds(this.position, context.mapBounds)) {
      errors.push({
        code: 'OUT_OF_BOUNDS',
        message: 'Destination is outside map boundaries',
        field: 'position',
      });
    }

    return { valid: errors.length === 0, errors, warnings };
  }

  private isWithinBounds(position: Vector3, bounds: { min: Vector3; max: Vector3 }): boolean {
    return (
      position.x >= bounds.min.x &&
      position.x <= bounds.max.x &&
      position.z >= bounds.min.z &&
      position.z <= bounds.max.z
    );
  }
}

export class AttackCommand extends BaseCommand {
  public readonly type = CommandType.Attack;

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    super(CommandType.Attack, target, {
      context,
      priority: options?.priority,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return {
        success: false,
        failedReason: 'Cannot execute attack command',
      };
    }

    if (!this.context.visibility.get(this.target.id)) {
      return {
        success: false,
        failedReason: 'Target is not visible',
      };
    }

    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'attack_initiated',
          targetId: this.target.id,
          data: { target: this.target },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    const hasCombatUnits = context.selectedUnits.length > 0;
    const hasTarget = this.target.id !== '';
    return hasCombatUnits && hasTarget;
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];
    const warnings: ValidationWarning[] = [];

    if (context.selectedUnits.length === 0) {
      errors.push({
        code: 'NO_UNITS_SELECTED',
        message: 'No units selected for attack command',
      });
    }

    if (!this.target.id) {
      errors.push({
        code: 'NO_TARGET',
        message: 'No target specified for attack command',
      });
    }

    const targetVisible = context.visibility.get(this.target.id);
    if (this.target.id && targetVisible === false) {
      errors.push({
        code: 'TARGET_NOT_VISIBLE',
        message: 'Target is not visible to your units',
        field: 'target',
      });
    }

    return { valid: errors.length === 0, errors, warnings };
  }
}

export class AttackMoveCommand extends BaseCommand {
  private readonly searchRadius: number;

  constructor(
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority; searchRadius?: number }
  ) {
    const target: CommandTarget = {
      id: '',
      type: 'location',
      position,
    };
    
    super(CommandType.AttackMove, target, {
      position,
      context,
      priority: options?.priority,
    });
    
    this.searchRadius = options?.searchRadius ?? 50;
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return {
        success: false,
        failedReason: 'Cannot execute attack-move command',
      };
    }

    const path = executor.calculatePath(this.context.selectedUnits, this.position!);
    
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'attack_move_initiated',
          targetId: this.context.selectedUnits.join(','),
          data: {
            path,
            destination: this.position,
            searchRadius: this.searchRadius,
          },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return context.selectedUnits.length > 0 && this.position !== undefined;
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];
    const warnings: ValidationWarning[] = [];

    if (context.selectedUnits.length === 0) {
      errors.push({
        code: 'NO_UNITS_SELECTED',
        message: 'No combat units selected for attack-move command',
      });
    }

    if (!this.position) {
      errors.push({
        code: 'NO_DESTINATION',
        message: 'No destination specified for attack-move command',
      });
    }

    return { valid: errors.length === 0, errors, warnings };
  }
}

export class PatrolCommand extends BaseCommand {
  public readonly type = CommandType.Patrol;

  constructor(
    waypoints: Vector3[],
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = {
      id: '',
      type: 'location',
      position: waypoints[0],
    };

    super(CommandType.Patrol, target, {
      position: waypoints[0],
      waypoints,
      context,
      priority: options?.priority,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return {
        success: false,
        failedReason: 'Cannot execute patrol command',
      };
    }

    if (this.waypoints!.length < 2) {
      return {
        success: false,
        failedReason: 'Patrol command requires at least 2 waypoints',
      };
    }

    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'patrol_initiated',
          targetId: this.context.selectedUnits.join(','),
          data: { waypoints: this.waypoints },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return (
      context.selectedUnits.length > 0 &&
      this.waypoints !== undefined &&
      this.waypoints.length >= 2
    );
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];
    const warnings: ValidationWarning[] = [];

    if (context.selectedUnits.length === 0) {
      errors.push({
        code: 'NO_UNITS_SELECTED',
        message: 'No units selected for patrol command',
      });
    }

    if (!this.waypoints || this.waypoints.length < 2) {
      errors.push({
        code: 'INSUFFICIENT_WAYPOINTS',
        message: 'Patrol command requires at least 2 waypoints',
        field: 'waypoints',
      });
    }

    return { valid: errors.length === 0, errors, warnings };
  }
}

export class HoldCommand extends BaseCommand {
  public readonly type = CommandType.Hold;

  constructor(context: CommandContext) {
    const target: CommandTarget = { id: '', type: 'location' };
    super(CommandType.Hold, target, { context });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'hold_position',
          targetId: this.context.selectedUnits.join(','),
          data: {},
        },
      ],
    };
  }
}

export class StopCommand extends BaseCommand {
  public readonly type = CommandType.Stop;

  constructor(context: CommandContext) {
    const target: CommandTarget = { id: '', type: 'location' };
    super(CommandType.Stop, target, { context });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'stop_all',
          targetId: this.context.selectedUnits.join(','),
          data: {},
        },
      ],
    };
  }
}

export class GarrisonCommand extends BaseCommand {
  public readonly type = CommandType.Garrison;

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    super(CommandType.Garrison, target, {
      context,
      priority: options?.priority,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return { success: false, failedReason: 'Cannot garrison' };
    }

    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'garrison_started',
          targetId: this.context.selectedUnits.join(','),
          data: { building: this.target.id },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return (
      context.selectedUnits.length > 0 &&
      this.target.id !== '' &&
      this.target.type === 'building'
    );
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];

    if (context.selectedUnits.length === 0) {
      errors.push({
        code: 'NO_UNITS_SELECTED',
        message: 'No units selected for garrison command',
      });
    }

    if (!this.target.id) {
      errors.push({
        code: 'NO_TARGET',
        message: 'No building target specified for garrison',
      });
    }

    return { valid: errors.length === 0, errors, warnings: [] };
  }
}

export class UngarrisonCommand extends BaseCommand {
  public readonly type = CommandType.Ungarrison;

  constructor(
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: '', type: 'location', position };
    super(CommandType.Ungarrison, target, {
      position,
      context,
      priority: options?.priority,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'ungarrison_started',
          targetId: this.context.selectedUnits.join(','),
          data: { position: this.position },
        },
      ],
    };
  }
}

export class RepairCommand extends BaseCommand {
  public readonly type = CommandType.Repair;

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    super(CommandType.Repair, target, { context, priority: options?.priority });
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return { success: false, failedReason: 'Cannot repair' };
    }

    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'repair_started',
          targetId: this.target.id,
          data: { repairedBy: this.context.selectedUnits },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return context.selectedUnits.length > 0 && this.target.id !== '';
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];

    if (context.selectedUnits.length === 0) {
      errors.push({
        code: 'NO_UNITS_SELECTED',
        message: 'No worker units selected for repair',
      });
    }

    if (!this.target.id) {
      errors.push({
        code: 'NO_TARGET',
        message: 'No target specified for repair',
      });
    }

    return { valid: errors.length === 0, errors, warnings: [] };
  }
}

export class RallyCommand extends BaseCommand {
  public readonly type = CommandType.Rally;

  constructor(
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: '', type: 'location', position };
    super(CommandType.Rally, target, {
      position,
      context,
      priority: options?.priority,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'rally_point_set',
          targetId: this.context.selectedBuildings[0] || '',
          data: { position: this.position },
        },
      ],
    };
  }
}

export class GroupCommand extends BaseCommand {
  public readonly type = CommandType.GroupAssign;
  private readonly groupId: number;

  constructor(
    groupId: number,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: '', type: 'group' };
    super(CommandType.GroupAssign, target, { context, priority: options?.priority });
    this.groupId = groupId;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'group_assigned',
          targetId: this.context.selectedUnits.join(','),
          data: { groupId: this.groupId },
        },
      ],
    };
  }
}

export class GroupRecallCommand extends BaseCommand {
  public readonly type = CommandType.GroupRecall;
  private readonly groupId: number;

  constructor(
    groupId: number,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: '', type: 'group' };
    super(CommandType.GroupRecall, target, { context, priority: options?.priority });
    this.groupId = groupId;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'group_recalled',
          targetId: String(this.groupId),
          data: { groupId: this.groupId },
        },
      ],
    };
  }
}

export class ProductionCommand extends BaseCommand {
  public readonly type = CommandType.Train;
  private readonly unitType: string;
  private readonly quantity: number;

  constructor(
    unitType: string,
    context: CommandContext,
    options?: { priority?: CommandPriority; quantity?: number }
  ) {
    const target: CommandTarget = {
      id: context.selectedBuildings[0] || '',
      type: 'building',
    };
    super(CommandType.Train, target, { context, priority: options?.priority });
    this.unitType = unitType;
    this.quantity = options?.quantity ?? 1;
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return { success: false, failedReason: 'Cannot produce unit' };
    }

    const unitCost = executor.getUnitCost(this.unitType);

    if (!this.hasResources(unitCost)) {
      return {
        success: false,
        failedReason: 'Insufficient resources',
        resourceCost: unitCost,
      };
    }

    return {
      success: true,
      executedCommands: [this],
      resourceCost: unitCost,
      timeCost: unitCost.time,
      sideEffects: [
        {
          type: 'production_started',
          targetId: this.target.id,
          data: {
            unitType: this.unitType,
            quantity: this.quantity,
            time: unitCost.time,
          },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return context.selectedBuildings.length > 0;
  }

  private hasResources(cost: CommandCost): boolean {
    const resources = this.context.resources;
    return (
      (cost.food === undefined || resources.food >= cost.food) &&
      (cost.wood === undefined || resources.wood >= cost.wood) &&
      (cost.gold === undefined || resources.gold >= cost.gold) &&
      (cost.stone === undefined || resources.stone >= cost.stone)
    );
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];

    if (context.selectedBuildings.length === 0) {
      errors.push({
        code: 'NO_BUILDING_SELECTED',
        message: 'No production building selected',
      });
    }

    if (!this.unitType) {
      errors.push({
        code: 'NO_UNIT_TYPE',
        message: 'No unit type specified for production',
      });
    }

    return { valid: errors.length === 0, errors, warnings: [] };
  }
}

export class ResearchCommand extends BaseCommand {
  public readonly type = CommandType.Research;
  private readonly technologyId: string;

  constructor(
    technologyId: string,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = {
      id: context.selectedBuildings[0] || '',
      type: 'building',
    };
    super(CommandType.Research, target, { context, priority: options?.priority });
    this.technologyId = technologyId;
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return { success: false, failedReason: 'Cannot research technology' };
    }

    if (this.context.technology.researched.has(this.technologyId)) {
      return { success: false, failedReason: 'Technology already researched' };
    }

    if (this.context.technology.inProgress.has(this.technologyId)) {
      return { success: false, failedReason: 'Technology already in progress' };
    }

    const techCost = executor.getTechnologyCost(this.technologyId);

    if (!this.hasResources(techCost)) {
      return {
        success: false,
        failedReason: 'Insufficient resources',
        resourceCost: techCost,
      };
    }

    return {
      success: true,
      executedCommands: [this],
      resourceCost: techCost,
      timeCost: techCost.time,
      sideEffects: [
        {
          type: 'research_started',
          targetId: this.target.id,
          data: { technologyId: this.technologyId, time: techCost.time },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return context.selectedBuildings.length > 0 && this.technologyId !== '';
  }

  private hasResources(cost: CommandCost): boolean {
    const resources = this.context.resources;
    return (
      (cost.food === undefined || resources.food >= cost.food) &&
      (cost.wood === undefined || resources.wood >= cost.wood) &&
      (cost.gold === undefined || resources.gold >= cost.gold) &&
      (cost.stone === undefined || resources.stone >= cost.stone)
    );
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];

    if (context.selectedBuildings.length === 0) {
      errors.push({
        code: 'NO_BUILDING_SELECTED',
        message: 'No building selected for research',
      });
    }

    if (!this.technologyId) {
      errors.push({
        code: 'NO_TECHNOLOGY',
        message: 'No technology specified for research',
      });
    }

    return { valid: errors.length === 0, errors, warnings: [] };
  }
}

export class BuildingCommand extends BaseCommand {
  public readonly type = CommandType.PlaceBuilding;
  private readonly buildingType: string;
  private readonly rotation: number;

  constructor(
    buildingType: string,
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority; rotation?: number }
  ) {
    const target: CommandTarget = { id: '', type: 'location', position };
    super(CommandType.PlaceBuilding, target, {
      position,
      context,
      priority: options?.priority,
    });
    this.buildingType = buildingType;
    this.rotation = options?.rotation ?? 0;
  }

  execute(executor: CommandExecutor): CommandResult {
    if (!this.canExecute(this.context)) {
      return { success: false, failedReason: 'Cannot place building' };
    }

    const buildingCost = executor.getBuildingCost(this.buildingType);

    if (!this.hasResources(buildingCost)) {
      return {
        success: false,
        failedReason: 'Insufficient resources',
        resourceCost: buildingCost,
      };
    }

    const placementValid = executor.validatePlacement(
      this.buildingType,
      this.position!,
      this.rotation
    );

    if (!placementValid.valid) {
      return {
        success: false,
        failedReason: placementValid.error || 'Invalid placement',
      };
    }

    return {
      success: true,
      executedCommands: [this],
      resourceCost: buildingCost,
      sideEffects: [
        {
          type: 'building_placed',
          targetId: '',
          data: {
            buildingType: this.buildingType,
            position: this.position,
            rotation: this.rotation,
          },
        },
      ],
    };
  }

  canExecute(context: CommandContext): boolean {
    return context.selectedUnits.length > 0 && this.buildingType !== '';
  }

  private hasResources(cost: CommandCost): boolean {
    const resources = this.context.resources;
    return (
      (cost.food === undefined || resources.food >= cost.food) &&
      (cost.wood === undefined || resources.wood >= cost.wood) &&
      (cost.gold === undefined || resources.gold >= cost.gold) &&
      (cost.stone === undefined || resources.stone >= cost.stone)
    );
  }

  validate(context: CommandContext): ValidationResult {
    const errors: ValidationError[] = [];
    const warnings: ValidationWarning[] = [];

    if (context.selectedUnits.length === 0) {
      errors.push({
        code: 'NO_WORKERS_SELECTED',
        message: 'No worker units selected for building placement',
      });
    }

    if (!this.buildingType) {
      errors.push({
        code: 'NO_BUILDING_TYPE',
        message: 'No building type specified',
      });
    }

    if (!this.position) {
      errors.push({
        code: 'NO_POSITION',
        message: 'No position specified for building placement',
      });
    }

    return { valid: errors.length === 0, errors, warnings };
  }
}

export class FollowCommand extends BaseCommand {
  public readonly type = CommandType.Follow;

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    super(CommandType.Follow, target, { context, priority: options?.priority });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'follow_started',
          targetId: this.context.selectedUnits.join(','),
          data: { target: this.target },
        },
      ],
    };
  }
}

export class GuardCommand extends BaseCommand {
  public readonly type = CommandType.Guard;

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    super(CommandType.Guard, target, { context, priority: options?.priority });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'guard_started',
          targetId: this.context.selectedUnits.join(','),
          data: { target: this.target },
        },
      ],
    };
  }
}

export class HarvestCommand extends BaseCommand {
  public readonly type = CommandType.Harvest;
  private readonly resourceType: string;

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority; resourceType?: string }
  ) {
    super(CommandType.Harvest, target, { context, priority: options?.priority });
    this.resourceType = options?.resourceType ?? 'auto';
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'harvest_started',
          targetId: this.context.selectedUnits.join(','),
          data: { target: this.target, resourceType: this.resourceType },
        },
      ],
    };
  }
}

export class ReturnResourcesCommand extends BaseCommand {
  public readonly type = CommandType.ReturnResources;

  constructor(context: CommandContext) {
    const target: CommandTarget = { id: '', type: 'location' };
    super(CommandType.ReturnResources, target, { context });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'return_resources',
          targetId: this.context.selectedUnits.join(','),
          data: {},
        },
      ],
    };
  }
}

export class CancelCommand extends BaseCommand {
  public readonly type = CommandType.Cancel;
  private readonly cancelType: 'unit' | 'building' | 'research' | 'all';

  constructor(
    context: CommandContext,
    options?: { priority?: CommandPriority; cancelType?: 'unit' | 'building' | 'research' | 'all' }
  ) {
    const target: CommandTarget = {
      id: context.selectedBuildings[0] || '',
      type: 'building',
    };
    super(CommandType.Cancel, target, { context, priority: options?.priority });
    this.cancelType = options?.cancelType ?? 'all';
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'canceled',
          targetId: this.target.id,
          data: { cancelType: this.cancelType },
        },
      ],
    };
  }
}

export class CommandExecutor {
  private unitGroups: Map<number, Set<string>> = new Map();
  private commandCache: Map<string, CommandResult> = new Map();

  execute(command: Command): CommandResult {
    const cacheKey = `${command.id}_${command.timestamp}`;
    
    if (this.commandCache.has(cacheKey)) {
      return this.commandCache.get(cacheKey)!;
    }

    const validation = command.validate(command.context);
    if (!validation.valid) {
      const result: CommandResult = {
        success: false,
        failedReason: validation.errors.map(e => e.message).join(', '),
      };
      this.commandCache.set(cacheKey, result);
      return result;
    }

    const result = command.execute(this);
    this.commandCache.set(cacheKey, result);
    
    return result;
  }

  calculatePath(unitIds: string[], destination: Vector3): Vector3[] {
    return [destination];
  }

  getUnitCost(unitType: string): CommandCost {
    return { food: 0, wood: 0, gold: 0, stone: 0, time: 10 };
  }

  getBuildingCost(buildingType: string): CommandCost {
    return { wood: 100, time: 60 };
  }

  getTechnologyCost(technologyId: string): CommandCost {
    return { food: 0, wood: 0, gold: 0, stone: 0, time: 60 };
  }

  validatePlacement(
    buildingType: string,
    position: Vector3,
    rotation: number
  ): { valid: boolean; error?: string } {
    return { valid: true };
  }

  assignUnitToGroup(unitId: string, groupId: number): void {
    if (!this.unitGroups.has(groupId)) {
      this.unitGroups.set(groupId, new Set());
    }
    this.unitGroups.get(groupId)!.add(unitId);
  }

  removeUnitFromGroup(unitId: string, groupId: number): void {
    this.unitGroups.get(groupId)?.delete(unitId);
  }

  getGroupUnits(groupId: number): string[] {
    return Array.from(this.unitGroups.get(groupId) || []);
  }

  recallGroup(groupId: number, position: Vector3): void {
    const units = this.getGroupUnits(groupId);
  }

  clearCache(): void {
    this.commandCache.clear();
  }
}

export class CommandQueueManager implements CommandQueue {
  private _commands: Command[] = [];
  public readonly maxSize: number;

  constructor(maxSize: number = 50) {
    this.maxSize = maxSize;
  }

  get commands(): Command[] {
    return [...this._commands];
  }

  enqueue(command: Command): boolean {
    if (this._commands.length >= this.maxSize) {
      return false;
    }
    this._commands.push(command);
    return true;
  }

  dequeue(): Command | null {
    return this._commands.shift() || null;
  }

  peek(): Command | null {
    return this._commands[0] || null;
  }

  clear(): void {
    this._commands = [];
  }

  insert(index: number, command: Command): boolean {
    if (this._commands.length >= this.maxSize) {
      return false;
    }
    if (index < 0 || index > this._commands.length) {
      return false;
    }
    this._commands.splice(index, 0, command);
    return true;
  }

  remove(index: number): Command | null {
    if (index < 0 || index >= this._commands.length) {
      return null;
    }
    return this._commands.splice(index, 1)[0];
  }

  reorder(fromIndex: number, toIndex: number): boolean {
    if (
      fromIndex < 0 ||
      fromIndex >= this._commands.length ||
      toIndex < 0 ||
      toIndex >= this._commands.length
    ) {
      return false;
    }
    const [command] = this._commands.splice(fromIndex, 1);
    this._commands.splice(toIndex, 0, command);
    return true;
  }

  executeAll(executor: CommandExecutor): CommandResult[] {
    const results: CommandResult[] = [];
    while (this._commands.length > 0) {
      const command = this.dequeue();
      if (command) {
        results.push(executor.execute(command));
      }
    }
    return results;
  }
}

export class CommandHistory {
  private history: Command[] = [];
  private redoStack: Command[] = [];
  private maxHistorySize: number;

  constructor(maxHistorySize: number = 100) {
    this.maxHistorySize = maxHistorySize;
  }

  push(command: Command): void {
    this.history.push(command);
    this.redoStack = [];
    
    if (this.history.length > this.maxHistorySize) {
      this.history.shift();
    }
  }

  undo(): Command | null {
    const command = this.history.pop();
    if (command) {
      this.redoStack.push(command);
      return command;
    }
    return null;
  }

  redo(): Command | null {
    const command = this.redoStack.pop();
    if (command) {
      this.history.push(command);
      return command;
    }
    return null;
  }

  canUndo(): boolean {
    return this.history.length > 0;
  }

  canRedo(): boolean {
    return this.redoStack.length > 0;
  }

  getHistory(): Command[] {
    return [...this.history];
  }

  clear(): void {
    this.history = [];
    this.redoStack = [];
  }

  getLastCommand(): Command | null {
    return this.history[this.history.length - 1] || null;
  }
}

export class CommandValidator {
  private rules: Map<CommandType, ValidationRule[]> = new Map();

  addRule(type: CommandType, rule: ValidationRule): void {
    if (!this.rules.has(type)) {
      this.rules.set(type, []);
    }
    this.rules.get(type)!.push(rule);
  }

  validate(command: Command): ValidationResult {
    const errors: ValidationError[] = [];
    const warnings: ValidationWarning[] = [];

    const typeRules = this.rules.get(command.type);
    if (typeRules) {
      for (const rule of typeRules) {
        const result = rule.validate(command, command.context);
        if (result.error) {
          errors.push({ code: rule.code, message: result.error });
        }
        if (result.warning) {
          warnings.push({ code: rule.code, message: result.warning });
        }
      }
    }

    return {
      valid: errors.length === 0,
      errors,
      warnings,
    };
  }

  validateAll(commands: Command[]): ValidationResult[] {
    return commands.map(cmd => this.validate(cmd));
  }
}

interface ValidationRule {
  code: string;
  validate(command: Command, context: CommandContext): { error?: string; warning?: string };
}

export class WaypointManager {
  private waypoints: Map<string, Vector3[]> = new Map();
  private currentWaypointIndex: Map<string, number> = new Map();

  addWaypoint(commandId: string, waypoint: Vector3): void {
    if (!this.waypoints.has(commandId)) {
      this.waypoints.set(commandId, []);
    }
    this.waypoints.get(commandId)!.push(waypoint);
  }

  getWaypoints(commandId: string): Vector3[] {
    return this.waypoints.get(commandId) || [];
  }

  getCurrentWaypoint(commandId: string): Vector3 | null {
    const waypoints = this.waypoints.get(commandId);
    const index = this.currentWaypointIndex.get(commandId) || 0;
    return waypoints?.[index] || null;
  }

  advanceWaypoint(commandId: string): boolean {
    const waypoints = this.waypoints.get(commandId);
    if (!waypoints) return false;
    
    const currentIndex = this.currentWaypointIndex.get(commandId) || 0;
    if (currentIndex < waypoints.length - 1) {
      this.currentWaypointIndex.set(commandId, currentIndex + 1);
      return true;
    }
    return false;
  }

  resetWaypoints(commandId: string): void {
    this.currentWaypointIndex.set(commandId, 0);
  }

  removeWaypoints(commandId: string): void {
    this.waypoints.delete(commandId);
    this.currentWaypointIndex.delete(commandId);
  }

  clear(): void {
    this.waypoints.clear();
    this.currentWaypointIndex.clear();
  }

  isLooping(commandId: string): boolean {
    return this.waypoints.has(commandId);
  }
}

export class CommandMacro {
  public readonly id: string;
  public readonly name: string;
  public readonly commands: Command[];
  public readonly hotkey?: string;

  constructor(
    id: string,
    name: string,
    commands: Command[],
    options?: { hotkey?: string }
  ) {
    this.id = id;
    this.name = name;
    this.commands = commands;
    this.hotkey = options?.hotkey;
  }

  execute(executor: CommandExecutor): CommandResult[] {
    return this.commands.map(cmd => executor.execute(cmd));
  }

  addCommand(command: Command): void {
    this.commands.push(command);
  }

  removeCommand(index: number): Command | null {
    if (index >= 0 && index < this.commands.length) {
      return this.commands.splice(index, 1)[0];
    }
    return null;
  }

  toJSON(): object {
    return {
      id: this.id,
      name: this.name,
      commands: this.commands.map(cmd => cmd.toJSON()),
      hotkey: this.hotkey,
    };
  }
}

export class HotkeyManager {
  private hotkeys: Map<string, HotkeyBinding> = new Map();
  private commandMacros: Map<string, CommandMacro> = new Map();

  bindHotkey(key: string, binding: HotkeyBinding): void {
    this.hotkeys.set(key, binding);
  }

  unbindHotkey(key: string): void {
    this.hotkeys.delete(key);
  }

  getHotkey(key: string): HotkeyBinding | undefined {
    return this.hotkeys.get(key);
  }

  getAllHotkeys(): Map<string, HotkeyBinding> {
    return new Map(this.hotkeys);
  }

  registerMacro(macro: CommandMacro): void {
    this.commandMacros.set(macro.id, macro);
    if (macro.hotkey) {
      this.bindHotkey(macro.hotkey, {
        type: 'macro',
        targetId: macro.id,
      });
    }
  }

  getMacro(id: string): CommandMacro | undefined {
    return this.commandMacros.get(id);
  }

  executeMacro(id: string, executor: CommandExecutor): CommandResult[] {
    const macro = this.commandMacros.get(id);
    if (macro) {
      return macro.execute(executor);
    }
    return [];
  }

  clear(): void {
    this.hotkeys.clear();
    this.commandMacros.clear();
  }
}

export interface HotkeyBinding {
  type: 'command' | 'macro' | 'ability' | 'ui';
  targetId: string;
  modifiers?: ('ctrl' | 'alt' | 'shift')[];
}

export class ContextMenuAction {
  public readonly id: string;
  public readonly label: string;
  public readonly icon?: string;
  public readonly commandType: CommandType;
  public readonly requiresTarget: boolean;
  public readonly enabled: (context: CommandContext) => boolean;

  constructor(
    id: string,
    label: string,
    commandType: CommandType,
    options?: {
      icon?: string;
      requiresTarget?: boolean;
      enabled?: (context: CommandContext) => boolean;
    }
  ) {
    this.id = id;
    this.label = label;
    this.commandType = commandType;
    this.icon = options?.icon;
    this.requiresTarget = options?.requiresTarget ?? false;
    this.enabled = options?.enabled ?? (() => true);
  }
}

export class ContextMenuManager {
  private menus: Map<string, ContextMenuAction[]> = new Map();

  registerMenu(contextType: string, actions: ContextMenuAction[]): void {
    this.menus.set(contextType, actions);
  }

  getMenu(contextType: string): ContextMenuAction[] {
    return this.menus.get(contextType) || [];
  }

  getAvailableActions(context: CommandContext): ContextMenuAction[] {
    const allActions: ContextMenuAction[] = [];
    
    for (const actions of this.menus.values()) {
      for (const action of actions) {
        if (action.enabled(context)) {
          allActions.push(action);
        }
      }
    }
    
    return allActions;
  }

  createCommandFromAction(
    action: ContextMenuAction,
    target: CommandTarget,
    context: CommandContext
  ): Command {
    return new AttackCommand(target, context);
  }
}

export class CommandCostCalculator {
  calculateCost(command: Command): CommandCost {
    return { ...command.cost };
  }

  sumCosts(costs: CommandCost[]): CommandCost {
    const total: CommandCost = {};
    
    for (const cost of costs) {
      if (cost.food) total.food = (total.food || 0) + cost.food;
      if (cost.wood) total.wood = (total.wood || 0) + cost.wood;
      if (cost.gold) total.gold = (total.gold || 0) + cost.gold;
      if (cost.stone) total.stone = (total.stone || 0) + cost.stone;
      if (cost.population) total.population = (total.population || 0) + cost.population;
      if (cost.time) total.time = (total.time || 0) + cost.time;
    }
    
    return total;
  }

  canAfford(cost: CommandCost, resources: ResourceState): boolean {
    return (
      (cost.food === undefined || resources.food >= cost.food) &&
      (cost.wood === undefined || resources.wood >= cost.wood) &&
      (cost.gold === undefined || resources.gold >= cost.gold) &&
      (cost.stone === undefined || resources.stone >= cost.stone) &&
      (cost.population === undefined || resources.population + cost.population <= resources.populationCap)
    );
  }

  getAffordableQuantity(
    unitType: string,
    costPerUnit: CommandCost,
    resources: ResourceState
  ): number {
    if (!costPerUnit.food && !costPerUnit.wood && !costPerUnit.gold && !costPerUnit.stone) {
      return resources.populationCap - resources.population;
    }

    let quantity = resources.populationCap - resources.population;

    if (costPerUnit.food) {
      quantity = Math.min(quantity, Math.floor(resources.food / costPerUnit.food));
    }
    if (costPerUnit.wood) {
      quantity = Math.min(quantity, Math.floor(resources.wood / costPerUnit.wood));
    }
    if (costPerUnit.gold) {
      quantity = Math.min(quantity, Math.floor(resources.gold / costPerUnit.gold));
    }
    if (costPerUnit.stone) {
      quantity = Math.min(quantity, Math.floor(resources.stone / costPerUnit.stone));
    }

    return quantity;
  }
}

export class CommandPriorityManager {
  private priorities: Map<string, CommandPriority> = new Map();

  setPriority(commandId: string, priority: CommandPriority): void {
    this.priorities.set(commandId, priority);
  }

  getPriority(commandId: string): CommandPriority {
    return this.priorities.get(commandId) || CommandPriority.Normal;
  }

  getHighestPriority(commands: Command[]): Command {
    return commands.reduce((highest, cmd) => 
      cmd.priority < highest.priority ? cmd : highest
    , commands[0]);
  }

  sortByPriority(commands: Command[]): Command[] {
    return [...commands].sort((a, b) => a.priority - b.priority);
  }

  clear(): void {
    this.priorities.clear();
  }
}

export class CommandEventEmitter {
  private listeners: Map<string, Set<(event: CommandEvent) => void>> = new Map();

  on(eventType: string, listener: (event: CommandEvent) => void): void {
    if (!this.listeners.has(eventType)) {
      this.listeners.set(eventType, new Set());
    }
    this.listeners.get(eventType)!.add(listener);
  }

  off(eventType: string, listener: (event: CommandEvent) => void): void {
    this.listeners.get(eventType)?.delete(listener);
  }

  emit(event: CommandEvent): void {
    const listeners = this.listeners.get(event.type);
    if (listeners) {
      for (const listener of listeners) {
        listener(event);
      }
    }
  }

  clear(): void {
    this.listeners.clear();
  }
}

export interface CommandEvent {
  type: string;
  command?: Command;
  result?: CommandResult;
  timestamp: number;
}

export class ScoutCommand extends BaseCommand {
  public readonly type = CommandType.Scout;
  private readonly scoutMode: 'area' | 'path' | 'target';

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority; scoutMode?: 'area' | 'path' | 'target' }
  ) {
    super(CommandType.Scout, target, { context, priority: options?.priority });
    this.scoutMode = options?.scoutMode ?? 'area';
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'scout_initiated',
          targetId: this.context.selectedUnits.join(','),
          data: { mode: this.scoutMode, target: this.target },
        },
      ],
    };
  }
}

export class AttackGroundCommand extends BaseCommand {
  public readonly type = CommandType.AttackGround;

  constructor(
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: '', type: 'location', position };
    super(CommandType.AttackGround, target, {
      position,
      context,
      priority: options?.priority,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'attack_ground_initiated',
          targetId: this.context.selectedUnits.join(','),
          data: { position: this.position },
        },
      ],
    };
  }
}

export class ReinforceCommand extends BaseCommand {
  public readonly type = CommandType.Reinforce;

  constructor(
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    super(CommandType.Reinforce, target, { context, priority: options?.priority });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'reinforce_initiated',
          targetId: this.target.id,
          data: {},
        },
      ],
    };
  }
}

export class TradeCommand extends BaseCommand {
  public readonly type = CommandType.Trade;
  private readonly tradePartner: string;

  constructor(
    tradePartner: string,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: tradePartner, type: 'building' };
    super(CommandType.Trade, target, { context, priority: options?.priority });
    this.tradePartner = tradePartner;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'trade_initiated',
          targetId: this.context.selectedUnits.join(','),
          data: { partner: this.tradePartner },
        },
      ],
    };
  }
}

export class DiplomacyCommand extends BaseCommand {
  public readonly type = CommandType.Diplomacy;
  private readonly action: 'ally' | 'neutral' | 'enemy';
  private readonly targetPlayer: string;

  constructor(
    action: 'ally' | 'neutral' | 'enemy',
    targetPlayer: string,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: targetPlayer, type: 'location' };
    super(CommandType.Diplomacy, target, { context, priority: options?.priority });
    this.action = action;
    this.targetPlayer = targetPlayer;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'diplomacy_changed',
          targetId: this.targetPlayer,
          data: { action: this.action },
        },
      ],
    };
  }
}

export class GiftCommand extends BaseCommand {
  public readonly type = CommandType.Gift;
  private readonly resources: { food?: number; wood?: number; gold?: number; stone?: number };
  private readonly targetPlayer: string;

  constructor(
    targetPlayer: string,
    resources: { food?: number; wood?: number; gold?: number; stone?: number },
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: targetPlayer, type: 'location' };
    super(CommandType.Gift, target, { context, priority: options?.priority });
    this.resources = resources;
    this.targetPlayer = targetPlayer;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      resourceCost: this.resources,
      sideEffects: [
        {
          type: 'resources_gifted',
          targetId: this.targetPlayer,
          data: { resources: this.resources },
        },
      ],
    };
  }
}

export class PingCommand extends BaseCommand {
  public readonly type = CommandType.Ping;
  private readonly pingType: 'attack' | 'defend' | 'alert' | 'generic';

  constructor(
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority; pingType?: 'attack' | 'defend' | 'alert' | 'generic' }
  ) {
    const target: CommandTarget = { id: '', type: 'location', position };
    super(CommandType.Ping, target, {
      position,
      context,
      priority: CommandPriority.Immediate,
    });
    this.pingType = options?.pingType ?? 'generic';
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'ping_created',
          targetId: context.playerId,
          data: { position: this.position, pingType: this.pingType },
        },
      ],
    };
  }
}

export class MorphCommand extends BaseCommand {
  public readonly type = CommandType.Morph;
  private readonly targetUnitType: string;

  constructor(
    targetUnitType: string,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = {
      id: context.selectedUnits[0] || '',
      type: 'unit',
    };
    super(CommandType.Morph, target, { context, priority: options?.priority });
    this.targetUnitType = targetUnitType;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'morph_started',
          targetId: this.target.id,
          data: { targetType: this.targetUnitType },
        },
      ],
    };
  }
}

export class SellCommand extends BaseCommand {
  public readonly type = CommandType.Sell;
  private readonly resource: 'food' | 'wood' | 'gold' | 'stone';
  private readonly quantity: number;

  constructor(
    resource: 'food' | 'wood' | 'gold' | 'stone',
    quantity: number,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = {
      id: context.selectedBuildings[0] || '',
      type: 'building',
    };
    super(CommandType.Sell, target, { context, priority: options?.priority });
    this.resource = resource;
    this.quantity = quantity;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'sell_initiated',
          targetId: this.target.id,
          data: { resource: this.resource, quantity: this.quantity },
        },
      ],
    };
  }
}

export class BuyCommand extends BaseCommand {
  public readonly type = CommandType.Buy;
  private readonly resource: 'food' | 'wood' | 'gold' | 'stone';
  private readonly quantity: number;

  constructor(
    resource: 'food' | 'wood' | 'gold' | 'stone',
    quantity: number,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = {
      id: context.selectedBuildings[0] || '',
      type: 'building',
    };
    super(CommandType.Buy, target, { context, priority: options?.priority });
    this.resource = resource;
    this.quantity = quantity;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'buy_initiated',
          targetId: this.target.id,
          data: { resource: this.resource, quantity: this.quantity },
        },
      ],
    };
  }
}

export class UpgradeCommand extends BaseCommand {
  public readonly type = CommandType.Upgrade;
  private readonly upgradeId: string;

  constructor(
    upgradeId: string,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = {
      id: context.selectedBuildings[0] || '',
      type: 'building',
    };
    super(CommandType.Upgrade, target, { context, priority: options?.priority });
    this.upgradeId = upgradeId;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'upgrade_started',
          targetId: this.target.id,
          data: { upgradeId: this.upgradeId },
        },
      ],
    };
  }
}

export class SetGatherPointCommand extends BaseCommand {
  public readonly type = CommandType.SetGatherPoint;

  constructor(
    position: Vector3,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    const target: CommandTarget = { id: '', type: 'location', position };
    super(CommandType.SetGatherPoint, target, {
      position,
      context,
      priority: options?.priority,
    });
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'gather_point_set',
          targetId: this.context.selectedBuildings[0] || '',
          data: { position: this.position },
        },
      ],
    };
  }
}

export class AbilityCommand extends BaseCommand {
  public readonly type = CommandType.Ability;
  private readonly abilityId: string;

  constructor(
    abilityId: string,
    target: CommandTarget,
    context: CommandContext,
    options?: { priority?: CommandPriority }
  ) {
    super(CommandType.Ability, target, { context, priority: options?.priority });
    this.abilityId = abilityId;
  }

  execute(executor: CommandExecutor): CommandResult {
    return {
      success: true,
      executedCommands: [this],
      sideEffects: [
        {
          type: 'ability_used',
          targetId: this.context.selectedUnits[0] || '',
          data: { abilityId: this.abilityId, target: this.target },
        },
      ],
    };
  }
}

export class CommandSystem {
  public readonly executor: CommandExecutor;
  public readonly queueManager: CommandQueueManager;
  public readonly history: CommandHistory;
  public readonly validator: CommandValidator;
  public readonly waypointManager: WaypointManager;
  public readonly hotkeyManager: HotkeyManager;
  public readonly contextMenuManager: ContextMenuManager;
  public readonly costCalculator: CommandCostCalculator;
  public readonly priorityManager: CommandPriorityManager;
  public readonly eventEmitter: CommandEventEmitter;

  private shiftQueue: Command[] = [];
  private commandBuffer: Command[] = [];
  private isProcessing: boolean = false;

  constructor() {
    this.executor = new CommandExecutor();
    this.queueManager = new CommandQueueManager(50);
    this.history = new CommandHistory(100);
    this.validator = new CommandValidator();
    this.waypointManager = new WaypointManager();
    this.hotkeyManager = new HotkeyManager();
    this.contextMenuManager = new ContextMenuManager();
    this.costCalculator = new CommandCostCalculator();
    this.priorityManager = new CommandPriorityManager();
    this.eventEmitter = new CommandEventEmitter();

    this.initializeDefaultHotkeys();
    this.initializeDefaultContextMenus();
  }

  private initializeDefaultHotkeys(): void {
    this.hotkeyManager.bindHotkey('M', {
      type: 'command',
      targetId: 'move',
    });
    this.hotkeyManager.bindHotkey('A', {
      type: 'command',
      targetId: 'attack',
    });
    this.hotkeyManager.bindHotkey('H', {
      type: 'command',
      targetId: 'hold',
    });
    this.hotkeyManager.bindHotkey('S', {
      type: 'command',
      targetId: 'stop',
    });
    this.hotkeyManager.bindHotkey('G', {
      type: 'command',
      targetId: 'garrison',
    });
    this.hotkeyManager.bindHotkey('P', {
      type: 'command',
      targetId: 'patrol',
    });
    this.hotkeyManager.bindHotkey('R', {
      type: 'command',
      targetId: 'repair',
    });

    for (let i = 1; i <= 9; i++) {
      this.hotkeyManager.bindHotkey(String(i), {
        type: 'command',
        targetId: `group_${i}`,
      });
    }

    this.hotkeyManager.bindHotkey('ctrl+1', {
      type: 'command',
      targetId: 'assign_group_1',
      modifiers: ['ctrl'],
    });
    this.hotkeyManager.bindHotkey('ctrl+2', {
      type: 'command',
      targetId: 'assign_group_2',
      modifiers: ['ctrl'],
    });
    this.hotkeyManager.bindHotkey('ctrl+3', {
      type: 'command',
      targetId: 'assign_group_3',
      modifiers: ['ctrl'],
    });
    this.hotkeyManager.bindHotkey('ctrl+4', {
      type: 'command',
      targetId: 'assign_group_4',
      modifiers: ['ctrl'],
    });
    this.hotkeyManager.bindHotkey('ctrl+5', {
      type: 'command',
      targetId: 'assign_group_5',
      modifiers: ['ctrl'],
    });
  }

  private initializeDefaultContextMenus(): void {
    const unitActions: ContextMenuAction[] = [
      new ContextMenuAction('move', 'Move', CommandType.Move, {
        icon: 'move',
        requiresTarget: true,
      }),
      new ContextMenuAction('attack', 'Attack', CommandType.Attack, {
        icon: 'attack',
        requiresTarget: true,
      }),
      new ContextMenuAction('attack_move', 'Attack-Move', CommandType.AttackMove, {
        icon: 'attack_move',
        requiresTarget: true,
      }),
      new ContextMenuAction('patrol', 'Patrol', CommandType.Patrol, {
        icon: 'patrol',
        requiresTarget: true,
      }),
      new ContextMenuAction('hold', 'Hold Position', CommandType.Hold, {
        icon: 'hold',
      }),
      new ContextMenuAction('stop', 'Stop', CommandType.Stop, {
        icon: 'stop',
      }),
      new ContextMenuAction('follow', 'Follow', CommandType.Follow, {
        icon: 'follow',
        requiresTarget: true,
      }),
      new ContextMenuAction('guard', 'Guard', CommandType.Guard, {
        icon: 'guard',
        requiresTarget: true,
      }),
      new ContextMenuAction('scout', 'Scout', CommandType.Scout, {
        icon: 'scout',
        requiresTarget: true,
      }),
    ];

    const workerActions: ContextMenuAction[] = [
      ...unitActions,
      new ContextMenuAction('repair', 'Repair', CommandType.Repair, {
        icon: 'repair',
        requiresTarget: true,
      }),
      new ContextMenuAction('build', 'Build', CommandType.PlaceBuilding, {
        icon: 'build',
      }),
      new ContextMenuAction('harvest', 'Harvest', CommandType.Harvest, {
        icon: 'harvest',
        requiresTarget: true,
      }),
      new ContextMenuAction('return', 'Return Resources', CommandType.ReturnResources, {
        icon: 'return',
      }),
    ];

    const buildingActions: ContextMenuAction[] = [
      new ContextMenuAction('rally', 'Set Rally Point', CommandType.Rally, {
        icon: 'rally',
        requiresTarget: true,
      }),
      new ContextMenuAction('gather_point', 'Set Gather Point', CommandType.SetGatherPoint, {
        icon: 'gather',
        requiresTarget: true,
      }),
      new ContextMenuAction('cancel', 'Cancel', CommandType.Cancel, {
        icon: 'cancel',
      }),
    ];

    const productionBuildingActions: ContextMenuAction[] = [
      ...buildingActions,
      new ContextMenuAction('train', 'Train Unit', CommandType.Train, {
        icon: 'train',
      }),
      new ContextMenuAction('research', 'Research', CommandType.Research, {
        icon: 'research',
      }),
      new ContextMenuAction('upgrade', 'Upgrade', CommandType.Upgrade, {
        icon: 'upgrade',
      }),
    ];

    const garrisonActions: ContextMenuAction[] = [
      new ContextMenuAction('ungarrison', 'Ungarrison', CommandType.Ungarrison, {
        icon: 'ungarrison',
        requiresTarget: true,
      }),
    ];

    const marketActions: ContextMenuAction[] = [
      ...buildingActions,
      new ContextMenuAction('sell', 'Sell', CommandType.Sell, {
        icon: 'sell',
      }),
      new ContextMenuAction('buy', 'Buy', CommandType.Buy, {
        icon: 'buy',
      }),
      new ContextMenuAction('trade', 'Trade', CommandType.Trade, {
        icon: 'trade',
        requiresTarget: true,
      }),
    ];

    this.contextMenuManager.registerMenu('unit', unitActions);
    this.contextMenuManager.registerMenu('worker', workerActions);
    this.contextMenuManager.registerMenu('building', buildingActions);
    this.contextMenuManager.registerMenu('production_building', productionBuildingActions);
    this.contextMenuManager.registerMenu('garrison_unit', garrisonActions);
    this.contextMenuManager.registerMenu('market', marketActions);
  }

  executeCommand(command: Command, useShiftQueue: boolean = false): CommandResult {
    if (useShiftQueue) {
      this.shiftQueue.push(command);
      return {
        success: true,
        message: 'Command queued (Shift held)',
      };
    }

    const validation = command.validate(command.context);
    if (!validation.valid) {
      this.eventEmitter.emit({
        type: 'command_validation_failed',
        command,
        timestamp: Date.now(),
      });
      return {
        success: false,
        failedReason: validation.errors.map(e => e.message).join(', '),
      };
    }

    const result = this.executor.execute(command);

    if (result.success) {
      this.history.push(command);
      this.executeShiftQueue();
      
      this.eventEmitter.emit({
        type: 'command_executed',
        command,
        result,
        timestamp: Date.now(),
      });
    } else {
      this.eventEmitter.emit({
        type: 'command_failed',
        command,
        result,
        timestamp: Date.now(),
      });
    }

    return result;
  }

  private executeShiftQueue(): void {
    if (this.shiftQueue.length === 0) return;

    const sortedCommands = this.priorityManager.sortByPriority(this.shiftQueue);
    
    for (const command of sortedCommands) {
      this.queueManager.enqueue(command);
    }

    this.shiftQueue = [];
  }

  queueCommand(command: Command): boolean {
    return this.queueManager.enqueue(command);
  }

  queueShiftCommand(command: Command): void {
    this.shiftQueue.push(command);
  }

  executeNextQueuedCommand(): CommandResult | null {
    const command = this.queueManager.dequeue();
    if (command) {
      const result = this.executor.execute(command);
      this.eventEmitter.emit({
        type: 'queued_command_executed',
        command,
        result,
        timestamp: Date.now(),
      });
      return result;
    }
    return null;
  }

  undo(): CommandResult {
    const command = this.history.undo();
    if (command) {
      this.eventEmitter.emit({
        type: 'command_undone',
        command,
        timestamp: Date.now(),
      });
      return { success: true, executedCommands: [command] };
    }
    return { success: false, failedReason: 'Nothing to undo' };
  }

  redo(): CommandResult {
    const command = this.history.undo();
    if (command) {
      const result = this.executor.execute(command);
      this.eventEmitter.emit({
        type: 'command_redone',
        command,
        result,
        timestamp: Date.now(),
      });
      return result;
    }
    return { success: false, failedReason: 'Nothing to redo' };
  }

  handleRightClick(
    position: Vector3,
    target: CommandTarget | null,
    context: CommandContext,
    modifiers: { shift: boolean; ctrl: boolean }
  ): CommandResult {
    if (target && target.type === 'resource') {
      const command = new HarvestCommand(target, context);
      return this.executeCommand(command, modifiers.shift);
    }

    if (target && (target.type === 'unit' || target.type === 'building')) {
      if (this.isEnemy(target.id, context)) {
        const command = new AttackCommand(target, context);
        return this.executeCommand(command, modifiers.shift);
      }
      
      if (target.type === 'building' && this.canGarrison(target.id, context)) {
        const command = new GarrisonCommand(target, context);
        return this.executeCommand(command, modifiers.shift);
      }

      const command = new FollowCommand(target, context);
      return this.executeCommand(command, modifiers.shift);
    }

    if (target && target.type === 'group') {
      const command = new GroupRecallCommand(parseInt(target.id), context);
      return this.executeCommand(command, modifiers.shift);
    }

    if (position) {
      const command = new MoveCommand(
        { id: '', type: 'location', position },
        position,
        context
      );
      return this.executeCommand(command, modifiers.shift);
    }

    return { success: false, failedReason: 'No valid target' };
  }

  private isEnemy(targetId: string, context: CommandContext): boolean {
    return false;
  }

  private canGarrison(targetId: string, context: CommandContext): boolean {
    return true;
  }

  createMoveCommand(
    position: Vector3,
    context: CommandContext,
    options?: { waypoints?: Vector3[] }
  ): MoveCommand {
    const target: CommandTarget = { id: '', type: 'location', position };
    return new MoveCommand(target, position, context, options);
  }

  createAttackCommand(
    target: CommandTarget,
    context: CommandContext
  ): AttackCommand {
    return new AttackCommand(target, context);
  }

  createAttackMoveCommand(
    position: Vector3,
    context: CommandContext
  ): AttackMoveCommand {
    return new AttackMoveCommand(position, context);
  }

  createPatrolCommand(
    waypoints: Vector3[],
    context: CommandContext
  ): PatrolCommand {
    return new PatrolCommand(waypoints, context);
  }

  createProductionCommand(
    unitType: string,
    context: CommandContext,
    quantity?: number
  ): ProductionCommand {
    return new ProductionCommand(unitType, context, { quantity });
  }

  createResearchCommand(
    technologyId: string,
    context: CommandContext
  ): ResearchCommand {
    return new ResearchCommand(technologyId, context);
  }

  createBuildingCommand(
    buildingType: string,
    position: Vector3,
    context: CommandContext
  ): BuildingCommand {
    return new BuildingCommand(buildingType, position, context);
  }

  createGroupCommand(
    groupId: number,
    context: CommandContext
  ): GroupCommand {
    return new GroupCommand(groupId, context);
  }

  createGroupRecallCommand(
    groupId: number,
    context: CommandContext
  ): GroupRecallCommand {
    return new GroupRecallCommand(groupId, context);
  }

  createMacro(
    name: string,
    commands: Command[],
    hotkey?: string
  ): CommandMacro {
    const id = `macro_${Date.now()}`;
    const macro = new CommandMacro(id, name, commands, { hotkey });
    this.hotkeyManager.registerMacro(macro);
    return macro;
  }

  getCommandCost(command: Command): CommandCost {
    return this.costCalculator.calculateCost(command);
  }

  getTotalQueueCost(): CommandCost {
    return this.costCalculator.sumCosts(
      this.queueManager.commands.map(cmd => cmd.cost)
    );
  }

  canAffordCommand(command: Command): boolean {
    return this.costCalculator.canAfford(command.cost, command.context.resources);
  }

  getQueuedCommandCount(): number {
    return this.queueManager.commands.length;
  }

  clearQueue(): void {
    this.queueManager.clear();
    this.shiftQueue = [];
  }

  clearHistory(): void {
    this.history.clear();
  }

  getCommandHistory(): Command[] {
    return this.history.getHistory();
  }

  onCommandExecuted(listener: (event: CommandEvent) => void): void {
    this.eventEmitter.on('command_executed', listener);
  }

  onCommandFailed(listener: (event: CommandEvent) => void): void {
    this.eventEmitter.on('command_failed', listener);
  }

  onCommandUndo(listener: (event: CommandEvent) => void): void {
    this.eventEmitter.on('command_undone', listener);
  }

  removeCommandListener(eventType: string, listener: (event: CommandEvent) => void): void {
    this.eventEmitter.off(eventType, listener);
  }

  processInput(
    input: CommandInput,
    context: CommandContext
  ): CommandResult {
    switch (input.type) {
      case 'move':
        return this.executeCommand(
          this.createMoveCommand(input.position!, context, { waypoints: input.waypoints }),
          input.shift
        );

      case 'attack':
        return this.executeCommand(
          this.createAttackCommand(input.target!, context),
          input.shift
        );

      case 'attack_move':
        return this.executeCommand(
          this.createAttackMoveCommand(input.position!, context),
          input.shift
        );

      case 'patrol':
        return this.executeCommand(
          this.createPatrolCommand(input.waypoints!, context),
          input.shift
        );

      case 'right_click':
        return this.handleRightClick(
          input.position!,
          input.target,
          context,
          { shift: input.shift || false, ctrl: input.ctrl || false }
        );

      case 'hotkey':
        return this.executeHotkey(input.hotkey!, context);

      case 'queue':
        if (input.command) {
          this.queueCommand(input.command);
          return { success: true };
        }
        return { success: false, failedReason: 'No command to queue' };

      default:
        return { success: false, failedReason: 'Unknown input type' };
    }
  }

  private executeHotkey(hotkey: string, context: CommandContext): CommandResult {
    const binding = this.hotkeyManager.getHotkey(hotkey);
    if (!binding) {
      return { success: false, failedReason: 'Hotkey not bound' };
    }

    switch (binding.type) {
      case 'command':
        return this.executeBoundCommand(binding.targetId, context);
      case 'macro':
        return {
          success: true,
          executedCommands: this.hotkeyManager.executeMacro(binding.targetId, this.executor),
        };
      case 'ability':
        return { success: true };
      default:
        return { success: false, failedReason: 'Unknown binding type' };
    }
  }

  private executeBoundCommand(targetId: string, context: CommandContext): CommandResult {
    switch (targetId) {
      case 'move':
        return { success: true };
      case 'attack':
        return { success: true };
      case 'hold':
        return this.executeCommand(new HoldCommand(context));
      case 'stop':
        return this.executeCommand(new StopCommand(context));
      case 'patrol':
        return { success: true };
      case 'garrison':
        return { success: true };
      case 'repair':
        return { success: true };
      default:
        if (targetId.startsWith('group_')) {
          const groupId = parseInt(targetId.split('_')[1]);
          return this.executeCommand(new GroupRecallCommand(groupId, context));
        }
        if (targetId.startsWith('assign_group_')) {
          const groupId = parseInt(targetId.split('_')[2]);
          return this.executeCommand(new GroupCommand(groupId, context));
        }
        return { success: false, failedReason: 'Unknown command' };
    }
  }

  serialize(): object {
    return {
      queue: this.queueManager.commands.map(cmd => cmd.toJSON()),
      history: this.history.getHistory().map(cmd => cmd.toJSON()),
      shiftQueue: this.shiftQueue.map(cmd => cmd.toJSON()),
    };
  }

  deserialize(data: { queue?: object[]; history?: object[]; shiftQueue?: object[] }): void {
    this.queueManager.clear();
    this.history.clear();
    this.shiftQueue = [];
  }
}

export interface CommandInput {
  type: 'move' | 'attack' | 'attack_move' | 'patrol' | 'right_click' | 'hotkey' | 'queue';
  position?: Vector3;
  target?: CommandTarget;
  waypoints?: Vector3[];
  shift?: boolean;
  ctrl?: boolean;
  hotkey?: string;
  command?: Command;
}

export default CommandSystem;

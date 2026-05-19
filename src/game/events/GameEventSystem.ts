export interface GameEvent {
  id: string;
  type: EventType;
  category: EventCategory;
  name: string;
  description: string;
  timestamp: number;
  gameTime: number;
  source: EventSource;
  target: EventTarget;
  data: EventData;
  effects: EventEffect[];
  conditions: EventCondition[];
  triggers: EventTrigger[];
  metadata: EventMetadata;
  state: EventState;
  history: EventHistoryEntry[];
}

export type EventType =
  | 'combat'
  | 'economy'
  | 'production'
  | 'research'
  | 'diplomacy'
  | 'narrative'
  | 'environmental'
  | 'triggered'
  | 'automated'
  | 'player_action'
  | 'ai_action'
  | 'system';

export type EventCategory =
  | 'battle'
  | 'strategy'
  | 'story'
  | 'tutorial'
  | 'achievement'
  | 'milestone'
  | 'discovery'
  | 'mystery';

export interface EventSource {
  type: SourceType;
  id: string;
  name: string;
  faction?: string;
  player?: string;
  position?: Position;
  level?: number;
}

export type SourceType =
  | 'unit'
  | 'building'
  | 'player'
  | 'ai'
  | 'system'
  | 'map'
  | 'narrative'
  | 'trigger'
  | 'script';

export interface EventTarget {
  type: TargetType;
  id: string;
  name: string;
  faction?: string;
  player?: string;
  position?: Position;
  health?: number;
  armor?: number;
}

export type TargetType =
  | 'unit'
  | 'building'
  | 'player'
  | 'faction'
  | 'territory'
  | 'resource'
  | 'technology'
  | 'global';

export interface Position {
  x: number;
  y: number;
  z?: number;
}

export interface EventData {
  unit?: UnitEventData;
  building?: BuildingEventData;
  combat?: CombatEventData;
  economy?: EconomyEventData;
  research?: ResearchEventData;
  diplomacy?: DiplomacyEventData;
  narrative?: NarrativeEventData;
  environmental?: EnvironmentalEventData;
  custom?: Record<string, unknown>;
}

export interface UnitEventData {
  unitType: string;
  unitClass: string;
  health: number;
  maxHealth: number;
  experience: number;
  level: number;
  status: UnitStatus;
  speed: number;
  damage: number;
  range: number;
  armor: number;
  abilities: string[];
  owner: string;
}

export interface UnitStatus {
  moving: boolean;
  attacking: boolean;
  harvesting: boolean;
  constructing: boolean;
  idle: boolean;
  stunned: boolean;
  cloaked: boolean;
  loaded: boolean;
  garrisoned: boolean;
}

export interface BuildingEventData {
  buildingType: string;
  health: number;
  maxHealth: number;
  production?: string;
  progress: number;
  powered: boolean;
  active: boolean;
  owner: string;
  constructing: boolean;
}

export interface CombatEventData {
  attacker: CombatParticipant;
  defender: CombatParticipant;
  damage: number;
  damageType: DamageType;
  attackType: AttackType;
  result: CombatResult;
  terrain?: string;
  weather?: string;
  timeOfDay?: string;
}

export type DamageType =
  | 'ballistic'
  | 'explosive'
  | 'energy'
  | 'chemical'
  | 'EMP'
  | 'psychic'
  | 'fire'
  | 'cold'
  | 'acid';

export type AttackType =
  | 'melee'
  | 'ranged'
  | 'siege'
  | 'air'
  | 'submarine'
  | 'swarm'
  | 'special';

export interface CombatParticipant {
  id: string;
  type: string;
  health: number;
  armor: number;
  position: Position;
}

export type CombatResult =
  | 'hit'
  | 'miss'
  | 'critical_hit'
  | 'blocked'
  | 'evaded'
  | 'destroyed'
  | 'captured';

export interface EconomyEventData {
  resourceType: ResourceType;
  amount: number;
  balance: number;
  income: number;
  expenses: number;
  tradeValue?: number;
  taxRate?: number;
}

export type ResourceType =
  | 'credits'
  | 'ore'
  | 'Tiberium'
  | 'alloy'
  | 'energy'
  | 'power'
  | 'influence'
  | 'research_points'
  | 'population';

export interface ResearchEventData {
  technology: TechnologyData;
  progress: number;
  researcher?: string;
  laboratory?: string;
  timeRemaining: number;
  cost: number;
}

export interface TechnologyData {
  id: string;
  name: string;
  tier: number;
  category: string;
  prerequisites: string[];
  unlocks: string[];
}

export interface DiplomacyEventData {
  action: DiplomacyAction;
  targetFaction: string;
  relationship: number;
  previousRelationship: number;
  treaty?: string;
  tradeAgreement?: TradeAgreement;
  alliance?: AllianceInfo;
}

export type DiplomacyAction =
  | 'declare_war'
  | 'sign_peace'
  | 'form_alliance'
  | 'break_alliance'
  | 'trade_agreement'
  | 'non_aggression'
  | 'embargo'
  | 'sanctions'
  | 'gift'
  | 'demand';

export interface TradeAgreement {
  id: string;
  terms: TradeTerm[];
  duration: number;
  ratified: boolean;
}

export interface TradeTerm {
  resource: ResourceType;
  amount: number;
  frequency: 'once' | 'daily' | 'weekly' | 'monthly';
}

export interface AllianceInfo {
  id: string;
  name: string;
  leader: string;
  members: string[];
  sharedVision: boolean;
  sharedUnits: boolean;
  jointOperations: boolean;
}

export interface NarrativeEventData {
  chapter: number;
  episode: number;
  scene?: number;
  plotPoint: PlotPoint;
  dialogue?: DialogueLine[];
  choices?: NarrativeChoice[];
  consequences?: string[];
  setting?: SceneSetting;
}

export interface PlotPoint {
  id: string;
  type: PlotPointType;
  title: string;
  description: string;
  importance: 'minor' | 'major' | 'critical';
  relatedEvents: string[];
}

export type PlotPointType =
  | 'introduction'
  | 'rising_action'
  | 'climax'
  | 'falling_action'
  | 'resolution'
  | 'twist'
  | 'revelation'
  | 'betrayal'
  | 'sacrifice'
  | 'victory'
  | 'defeat';

export interface DialogueLine {
  id: string;
  speaker: string;
  speakerPortrait?: string;
  text: string;
  emotion: Emotion;
  audio?: AudioCue;
  subtitle?: string;
  duration: number;
}

export type Emotion =
  | 'neutral'
  | 'happy'
  | 'sad'
  | 'angry'
  | 'fear'
  | 'surprise'
  | 'disgust'
  | 'contempt'
  | 'determined'
  | 'confused';

export interface AudioCue {
  file: string;
  volume: number;
  startTime?: number;
  endTime?: number;
}

export interface NarrativeChoice {
  id: string;
  text: string;
  description?: string;
  consequences: string[];
  requirements?: ChoiceRequirement[];
  outcomes: ChoiceOutcome[];
}

export interface ChoiceRequirement {
  type: 'resource' | 'unit' | 'building' | 'technology' | 'faction' | 'variable';
  condition: string;
  value: unknown;
}

export interface ChoiceOutcome {
  probability: number;
  effects: EventEffect[];
  narrative: string;
  nextPlotPoint?: string;
}

export interface SceneSetting {
  location: string;
  timeOfDay: string;
  weather: string;
  atmosphere: string;
  background: string;
  music: string;
  ambientSound?: string;
}

export interface EnvironmentalEventData {
  type: EnvironmentalEventType;
  severity: number;
  duration: number;
  affectedArea: Area;
  effects: EnvironmentalEffect[];
}

export type EnvironmentalEventType =
  | 'earthquake'
  | 'flood'
  | 'fire'
  | 'storm'
  | 'fog'
  | 'dust_storm'
  | 'radiation'
  | 'meteor'
  | 'eclipse'
  | 'tsunami';

export interface Area {
  center: Position;
  radius: number;
  shape: 'circle' | 'rectangle' | 'polygon';
  vertices?: Position[];
}

export interface EnvironmentalEffect {
  type: EffectType;
  target: string;
  value: number;
  duration: number;
}

export interface EventEffect {
  id: string;
  type: EffectType;
  target: EffectTarget;
  value: unknown;
  duration?: number;
  probability?: number;
  conditions?: EventCondition[];
  delayed?: boolean;
  delayTime?: number;
  stacks?: boolean;
  maxStacks?: number;
}

export type EffectType =
  | 'damage'
  | 'heal'
  | 'buff'
  | 'debuff'
  | 'teleport'
  | 'spawn'
  | 'destroy'
  | 'disable'
  | 'enable'
  | 'transform'
  | 'capture'
  | 'release'
  | 'reveal'
  | 'hide'
  | 'unlock'
  | 'lock'
  | 'apply_status'
  | 'remove_status'
  | 'modify_stat'
  | 'grant_ability'
  | 'revoke_ability'
  | 'change_faction'
  | 'trigger_event'
  | 'play_animation'
  | 'play_sound'
  | 'play_cutscene'
  | 'show_dialogue'
  | 'give_choice'
  | 'set_variable'
  | 'modify_variable'
  | 'create_unit'
  | 'remove_unit'
  | 'modify_resources'
  | 'modify_relationships'
  | 'reveal_territory'
  | 'conquer_territory'
  | 'research_technology'
  | 'build_structure'
  | 'complete_objective';

export type EffectTarget =
  | 'self'
  | 'caster'
  | 'target'
  | 'area'
  | 'allies'
  | 'enemies'
  | 'neutral'
  | 'all_units'
  | 'all_buildings'
  | 'all_players'
  | 'faction'
  | 'global'
  | 'variable';

export interface EventCondition {
  id: string;
  type: ConditionType;
  target: string;
  comparison: ComparisonOperator;
  value: unknown;
  logicalOperator?: 'and' | 'or' | 'not';
  nested?: EventCondition[];
}

export type ConditionType =
  | 'has_unit'
  | 'has_building'
  | 'has_resource'
  | 'has_technology'
  | 'has_status'
  | 'has_variable'
  | 'health_above'
  | 'health_below'
  | 'position_in_area'
  | 'time_elapsed'
  | 'turn_number'
  | 'player_count'
  | 'faction_relation'
  | 'unit_count'
  | 'building_count'
  | 'tech_level'
  | 'difficulty_level'
  | 'game_mode'
  | 'map_type'
  | 'random_chance';

export type ComparisonOperator =
  | 'equals'
  | 'not_equals'
  | 'greater_than'
  | 'less_than'
  | 'greater_or_equal'
  | 'less_or_equal'
  | 'contains'
  | 'not_contains'
  | 'in_range'
  | 'not_in_range';

export interface EventTrigger {
  id: string;
  type: TriggerType;
  source: string;
  conditions?: EventCondition[];
  cooldown?: number;
  lastTriggered?: number;
  priority?: number;
}

export type TriggerType =
  | 'on_spawn'
  | 'on_death'
  | 'on_damage'
  | 'on_heal'
  | 'on_capture'
  | 'on_discover'
  | 'on_research_complete'
  | 'on_building_complete'
  | 'on_resource_change'
  | 'on_area_enter'
  | 'on_area_exit'
  | 'on_keyword'
  | 'on_time'
  | 'on_turn'
  | 'on_player_action'
  | 'on_ai_action'
  | 'on_dialogue_choice'
  | 'on_cutscene_end'
  | 'on_mission_start'
  | 'on_mission_complete'
  | 'on_objective_complete'
  | 'on_defeat'
  | 'on_victory';

export interface EventMetadata {
  createdAt: number;
  modifiedAt: number;
  author?: string;
  version: string;
  tags: string[];
  category?: string;
  difficulty?: DifficultyLevel;
  replayable: boolean;
  skippable: boolean;
}

export type DifficultyLevel = 'easy' | 'normal' | 'hard' | 'brutal';

export interface EventState {
  triggered: boolean;
  active: boolean;
  completed: boolean;
  failed: boolean;
  progress: number;
  currentPhase?: number;
  timer?: number;
  effectsApplied: AppliedEffect[];
  variables: Map<string, unknown>;
}

export interface AppliedEffect {
  id: string;
  effectId: string;
  target: string;
  startTime: number;
  endTime?: number;
  value: unknown;
  stacks: number;
}

export interface EventHistoryEntry {
  timestamp: number;
  action: string;
  details: Record<string, unknown>;
}

export interface StoryManager {
  id: string;
  name: string;
  campaign: Campaign;
  chapters: Chapter[];
  currentChapterIndex: number;
  currentEpisodeIndex: number;
  currentSceneIndex: number;
  progress: StoryProgress;
  variables: StoryVariables;
  choices: StoryChoices;
  unlocks: StoryUnlocks;
  state: StoryState;
  settings: StorySettings;
}

export interface Campaign {
  id: string;
  name: string;
  description: string;
  author: string;
  version: string;
  difficulty: DifficultyLevel;
  estimatedDuration: number;
  prerequisites: string[];
  recommendedLevel: number;
  lore: string;
  introduction?: DialogueLine[];
  poster?: string;
  trailer?: string;
}

export interface Chapter {
  id: string;
  number: number;
  name: string;
  description: string;
  episodes: Episode[];
  objectives: ChapterObjective[];
  rewards: ChapterReward[];
  unlocks: string[];
  requirements: ChapterRequirement[];
}

export interface Episode {
  id: string;
  number: number;
  title: string;
  description: string;
  scenes: Scene[];
  objectives: EpisodeObjective[];
  rewards: EpisodeReward[];
  dialogue?: DialogueLine[];
  choices?: NarrativeChoice[];
}

export interface Scene {
  id: string;
  number: number;
  type: SceneType;
  title?: string;
  setting: SceneSetting;
  events: GameEvent[];
  dialogue?: DialogueLine[];
  actions?: SceneAction[];
  choices?: NarrativeChoice[];
  objectives?: SceneObjective[];
  transitions: SceneTransition[];
  completionConditions: EventCondition[];
}

export type SceneType =
  | 'cinematic'
  | 'gameplay'
  | 'dialogue'
  | 'choice'
  | 'combat'
  | 'stealth'
  | 'exploration'
  | 'puzzle'
  | 'tutorial'
  | 'boss';

export interface SceneAction {
  id: string;
  type: ActionType;
  target: string;
  parameters: Record<string, unknown>;
  delay?: number;
  conditions?: EventCondition[];
}

export interface SceneTransition {
  id: string;
  type: TransitionType;
  targetScene?: string;
  conditions?: EventCondition[];
  effects?: EventEffect[];
  delay?: number;
  automatic?: boolean;
}

export type TransitionType =
  | 'immediate'
  | 'delayed'
  | 'on_choice'
  | 'on_completion'
  | 'on_failure'
  | 'conditional';

export interface SceneObjective {
  id: string;
  type: ObjectiveType;
  description: string;
  target?: string;
  requiredValue?: number;
  currentValue?: number;
  completed?: boolean;
  bonus?: boolean;
  hidden?: boolean;
}

export type ObjectiveType =
  | 'primary'
  | 'secondary'
  | 'bonus'
  | 'hidden'
  | 'optional';

export interface ChapterObjective {
  id: string;
  type: ObjectiveType;
  description: string;
  episodes: string[];
  completed: boolean;
}

export interface EpisodeObjective {
  id: string;
  type: ObjectiveType;
  description: string;
  scenes: string[];
  completed: boolean;
}

export interface ChapterReward {
  type: RewardType;
  value: unknown;
  conditions?: EventCondition[];
}

export type RewardType =
  | 'experience'
  | 'credits'
  | 'units'
  | 'technology'
  | 'unlock'
  | 'achievement'
  | 'skin'
  | 'badge'
  | 'title';

export interface EpisodeReward {
  type: RewardType;
  value: unknown;
}

export interface ChapterRequirement {
  type: RequirementType;
  value: unknown;
  met: boolean;
}

export type RequirementType =
  | 'previous_chapter_complete'
  | 'level_reached'
  | 'units_owned'
  | 'technology_researched'
  | 'mission_completed'
  | 'achievement_earned';

export interface StoryProgress {
  chapter: number;
  episode: number;
  scene: number;
  totalScenes: number;
  completedScenes: number;
  currentObjectives: string[];
  completedObjectives: string[];
  bonusObjectivesCompleted: number;
  totalBonusObjectives: number;
  timeElapsed: number;
  retries: number;
  bestTime?: number;
  score?: number;
}

export interface StoryVariables {
  global: Record<string, unknown>;
  chapter: Record<string, unknown>;
  episode: Record<string, unknown>;
  scene: Record<string, unknown>;
  mission: Record<string, unknown>;
  player: Record<string, unknown>;
}

export interface StoryChoices {
  made: string[];
  outcomes: Record<string, ChoiceOutcome>;
  consequences: string[];
  flags: Record<string, boolean>;
}

export interface StoryUnlocks {
  chapters: string[];
  episodes: string[];
  missions: string[];
  units: string[];
  technologies: string[];
  cosmetics: string[];
  achievements: string[];
  endings: string[];
  bonuses: string[];
}

export interface StoryState {
  currentScene?: Scene;
  dialogueIndex: number;
  choiceIndex: number;
  objectiveProgress: Record<string, number>;
  activeEffects: AppliedEffect[];
  triggeredEvents: string[];
  completedTransitions: string[];
  variables: StoryVariables;
}

export interface StorySettings {
  autoAdvance: boolean;
  autoAdvanceDelay: number;
  showObjectives: boolean;
  showDialogue: boolean;
  showSubtitles: boolean;
  skipEnabled: boolean;
  difficultyLocked: boolean;
  allowReplay: boolean;
  saveProgress: boolean;
  showTutorialHints: boolean;
  voiceEnabled: boolean;
  musicVolume: number;
  effectsVolume: number;
}

export class GameEventSystem {
  private events: Map<string, GameEvent> = new Map();
  private eventQueue: GameEvent[] = [];
  private activeEvents: Set<string> = new Map();
  private listeners: Map<string, Set<Function>> = new Map();
  private eventHistory: GameEvent[] = [];
  private maxHistorySize: number = 1000;
  private triggers: Map<TriggerType, Set<string>> = new Map();
  private conditions: ConditionEvaluator;
  private effects: EffectProcessor;
  private state: EventSystemState;
  private debounceTimers: Map<string, NodeJS.Timeout> = new Map();
  private cooldownTracker: Map<string, number> = new Map();
  private statistics: EventStatistics;

  constructor() {
    this.conditions = new ConditionEvaluator();
    this.effects = new EffectProcessor();
    this.state = {
      paused: false,
      currentTime: 0,
      gameTime: 0,
      processingQueue: false
    };
    this.statistics = {
      totalEventsTriggered: 0,
      eventsByType: new Map(),
      eventsByCategory: new Map(),
      averageProcessingTime: 0,
      failedEvents: 0
    };

    this.initializeTriggerMap();
  }

  private initializeTriggerMap(): void {
    const triggerTypes: TriggerType[] = [
      'on_spawn', 'on_death', 'on_damage', 'on_heal', 'on_capture',
      'on_discover', 'on_research_complete', 'on_building_complete',
      'on_resource_change', 'on_area_enter', 'on_area_exit',
      'on_keyword', 'on_time', 'on_turn', 'on_player_action',
      'on_ai_action', 'on_dialogue_choice', 'on_cutscene_end',
      'on_mission_start', 'on_mission_complete', 'on_objective_complete',
      'on_defeat', 'on_victory'
    ];

    triggerTypes.forEach(type => {
      this.triggers.set(type, new Set());
    });
  }

  createEvent(config: EventConfig): GameEvent {
    const event: GameEvent = {
      id: config.id || this.generateEventId(),
      type: config.type,
      category: config.category,
      name: config.name,
      description: config.description,
      timestamp: Date.now(),
      gameTime: this.state.gameTime,
      source: config.source,
      target: config.target,
      data: config.data || {},
      effects: config.effects || [],
      conditions: config.conditions || [],
      triggers: config.triggers || [],
      metadata: {
        createdAt: Date.now(),
        modifiedAt: Date.now(),
        version: config.metadata?.version || '1.0.0',
        tags: config.metadata?.tags || [],
        replayable: config.metadata?.replayable ?? true,
        skippable: config.metadata?.skippable ?? true
      },
      state: {
        triggered: false,
        active: false,
        completed: false,
        failed: false,
        progress: 0,
        effectsApplied: [],
        variables: new Map()
      },
      history: []
    };

    this.events.set(event.id, event);

    event.triggers.forEach(trigger => {
      const triggerSet = this.triggers.get(trigger.type);
      if (triggerSet) {
        triggerSet.add(event.id);
      }
    });

    this.emit('event_created', { event });
    return event;
  }

  private generateEventId(): string {
    return `evt_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  triggerEvent(eventId: string, context?: EventContext): boolean {
    const event = this.events.get(eventId);
    if (!event) return false;

    if (event.state.triggered && !event.metadata.replayable) return false;

    if (this.state.paused) return false;

    const cooldownKey = `${eventId}_${context?.playerId || 'global'}`;
    const lastTriggered = this.cooldownTracker.get(cooldownKey);
    if (lastTriggered) {
      const cooldown = event.triggers[0]?.cooldown || 0;
      if (Date.now() - lastTriggered < cooldown) {
        return false;
      }
    }

    if (!this.evaluateConditions(event.conditions, context)) {
      return false;
    }

    event.state.triggered = true;
    event.state.active = true;
    event.state.completed = false;
    event.state.failed = false;
    event.state.progress = 0;

    this.cooldownTracker.set(cooldownKey, Date.now());

    this.applyEffects(event.effects, context);

    this.eventHistory.push(event);
    this.trimHistory();

    this.updateStatistics(event);

    this.emit('event_triggered', { event, context });

    this.statistics.totalEventsTriggered++;

    return true;
  }

  private evaluateConditions(conditions: EventCondition[], context?: EventContext): boolean {
    if (conditions.length === 0) return true;

    let result = true;
    let currentOperator: 'and' | 'or' = 'and';

    for (const condition of conditions) {
      const conditionResult = this.conditions.evaluate(condition, context);

      if (condition.logicalOperator) {
        currentOperator = condition.logicalOperator;
      }

      if (currentOperator === 'and') {
        result = result && conditionResult;
      } else if (currentOperator === 'or') {
        result = result || conditionResult;
      } else if (currentOperator === 'not') {
        result = !conditionResult;
      }
    }

    return result;
  }

  private applyEffects(effects: EventEffect[], context?: EventContext): void {
    effects.forEach(effect => {
      if (effect.probability !== undefined && Math.random() > effect.probability) {
        return;
      }

      if (effect.conditions && effect.conditions.length > 0) {
        if (!this.evaluateConditions(effect.conditions, context)) {
          return;
        }
      }

      if (effect.delayed && effect.delayTime) {
        this.scheduleEffect(effect, context, effect.delayTime);
      } else {
        this.effects.apply(effect, context);
      }
    });
  }

  private scheduleEffect(effect: EventEffect, context: EventContext | undefined, delay: number): void {
    setTimeout(() => {
      this.effects.apply(effect, context);
    }, delay);
  }

  queueEvent(event: GameEvent): void {
    this.eventQueue.push(event);
    this.processQueue();
  }

  private processQueue(): void {
    if (this.state.processingQueue || this.eventQueue.length === 0) return;

    this.state.processingQueue = true;

    const processNext = () => {
      if (this.eventQueue.length === 0 || this.state.paused) {
        this.state.processingQueue = false;
        return;
      }

      const event = this.eventQueue.shift()!;
      this.triggerEvent(event.id);
      processNext();
    };

    processNext();
  }

  handleTrigger(triggerType: TriggerType, context: EventContext): void {
    const triggerSet = this.triggers.get(triggerType);
    if (!triggerSet) return;

    triggerSet.forEach(eventId => {
      const event = this.events.get(eventId);
      if (!event) return;

      const trigger = event.triggers.find(t => t.type === triggerType);
      if (!trigger) return;

      if (trigger.conditions && !this.evaluateConditions(trigger.conditions, context)) {
        return;
      }

      this.triggerEvent(eventId, context);
    });
  }

  update(deltaTime: number): void {
    if (this.state.paused) return;

    this.state.currentTime += deltaTime;
    this.state.gameTime += deltaTime;

    this.events.forEach(event => {
      if (!event.state.active) return;

      if (event.state.progress < 100) {
        event.state.progress += deltaTime / 10;
      }

      if (event.state.progress >= 100 && !event.state.completed) {
        this.completeEvent(event.id);
      }
    });

    this.checkTimedTriggers();
  }

  private checkTimedTriggers(): void {
    this.events.forEach(event => {
      event.triggers.forEach(trigger => {
        if (trigger.type === 'on_time' && trigger.conditions) {
          const timeCondition = trigger.conditions.find(c => c.type === 'time_elapsed');
          if (timeCondition && typeof timeCondition.value === 'number') {
            if (this.state.gameTime >= timeCondition.value * 1000) {
              this.triggerEvent(event.id);
            }
          }
        }
      });
    });
  }

  completeEvent(eventId: string): void {
    const event = this.events.get(eventId);
    if (!event) return;

    event.state.active = false;
    event.state.completed = true;

    event.effects.forEach(effect => {
      if (effect.type === 'trigger_event') {
        const targetEventId = effect.value as string;
        this.triggerEvent(targetEventId);
      }
    });

    this.emit('event_completed', { event });
  }

  failEvent(eventId: string, reason?: string): void {
    const event = this.events.get(eventId);
    if (!event) return;

    event.state.active = false;
    event.state.failed = true;

    this.emit('event_failed', { event, reason });
  }

  resetEvent(eventId: string): void {
    const event = this.events.get(eventId);
    if (!event) return;

    event.state.triggered = false;
    event.state.active = false;
    event.state.completed = false;
    event.state.failed = false;
    event.state.progress = 0;
    event.state.effectsApplied = [];

    this.emit('event_reset', { event });
  }

  getEvent(eventId: string): GameEvent | undefined {
    return this.events.get(eventId);
  }

  getAllEvents(): GameEvent[] {
    return Array.from(this.events.values());
  }

  getEventsByType(type: EventType): GameEvent[] {
    return this.getAllEvents().filter(e => e.type === type);
  }

  getEventsByCategory(category: EventCategory): GameEvent[] {
    return this.getAllEvents().filter(e => e.category === category);
  }

  getActiveEvents(): GameEvent[] {
    return this.getAllEvents().filter(e => e.state.active);
  }

  getTriggeredEvents(): GameEvent[] {
    return this.getAllEvents().filter(e => e.state.triggered);
  }

  getEventHistory(): GameEvent[] {
    return [...this.eventHistory];
  }

  pause(): void {
    this.state.paused = true;
    this.emit('system_paused', {});
  }

  resume(): void {
    this.state.paused = false;
    this.emit('system_resumed', {});
  }

  setGameTime(time: number): void {
    this.state.gameTime = time;
  }

  getGameTime(): number {
    return this.state.gameTime;
  }

  clearHistory(): void {
    this.eventHistory = [];
  }

  private trimHistory(): void {
    while (this.eventHistory.length > this.maxHistorySize) {
      this.eventHistory.shift();
    }
  }

  private updateStatistics(event: GameEvent): void {
    const typeCount = this.statistics.eventsByType.get(event.type) || 0;
    this.statistics.eventsByType.set(event.type, typeCount + 1);

    const categoryCount = this.statistics.eventsByCategory.get(event.category) || 0;
    this.statistics.eventsByCategory.set(event.category, categoryCount + 1);
  }

  getStatistics(): EventStatistics {
    return {
      ...this.statistics,
      eventsByType: new Map(this.statistics.eventsByType),
      eventsByCategory: new Map(this.statistics.eventsByCategory)
    };
  }

  exportEvents(): string {
    return JSON.stringify(Array.from(this.events.values()));
  }

  importEvents(jsonData: string): boolean {
    try {
      const events = JSON.parse(jsonData);
      if (!Array.isArray(events)) return false;

      events.forEach((eventData: GameEvent) => {
        const event: GameEvent = {
          ...eventData,
          state: {
            ...eventData.state,
            variables: new Map(eventData.state.variables as [string, unknown][])
          }
        };
        this.events.set(event.id, event);
      });

      return true;
    } catch (error) {
      console.error('Failed to import events:', error);
      return false;
    }
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(cb => cb(data));
    }
  }
}

export interface EventConfig {
  id?: string;
  type: EventType;
  category: EventCategory;
  name: string;
  description: string;
  source: EventSource;
  target?: EventTarget;
  data?: EventData;
  effects?: EventEffect[];
  conditions?: EventCondition[];
  triggers?: EventTrigger[];
  metadata?: Partial<EventMetadata>;
}

export interface EventContext {
  playerId?: string;
  factionId?: string;
  unitId?: string;
  buildingId?: string;
  position?: Position;
  gameState?: Record<string, unknown>;
}

export interface EventSystemState {
  paused: boolean;
  currentTime: number;
  gameTime: number;
  processingQueue: boolean;
}

export interface EventStatistics {
  totalEventsTriggered: number;
  eventsByType: Map<EventType, number>;
  eventsByCategory: Map<EventCategory, number>;
  averageProcessingTime: number;
  failedEvents: number;
}

class ConditionEvaluator {
  evaluate(condition: EventCondition, context?: EventContext): boolean {
    switch (condition.type) {
      case 'has_unit':
        return this.checkHasUnit(condition, context);
      case 'has_building':
        return this.checkHasBuilding(condition, context);
      case 'has_resource':
        return this.checkHasResource(condition, context);
      case 'has_technology':
        return this.checkHasTechnology(condition, context);
      case 'has_status':
        return this.checkHasStatus(condition, context);
      case 'has_variable':
        return this.checkHasVariable(condition, context);
      case 'health_above':
        return this.checkHealthAbove(condition, context);
      case 'health_below':
        return this.checkHealthBelow(condition, context);
      case 'position_in_area':
        return this.checkPositionInArea(condition, context);
      case 'time_elapsed':
        return this.checkTimeElapsed(condition, context);
      case 'random_chance':
        return this.checkRandomChance(condition);
      default:
        return true;
    }
  }

  private checkHasUnit(condition: EventCondition, context?: EventContext): boolean {
    const value = condition.value as string;
    return context?.gameState?.[`has_${value}`] === true;
  }

  private checkHasBuilding(condition: EventCondition, context?: EventContext): boolean {
    const value = condition.value as string;
    return context?.gameState?.[`has_building_${value}`] === true;
  }

  private checkHasResource(condition: EventCondition, context?: EventContext): boolean {
    const value = condition.value as number;
    const resource = context?.gameState?.[condition.target] as number || 0;
    return this.compareValues(resource, value, condition.comparison);
  }

  private checkHasTechnology(condition: EventCondition, context?: EventContext): boolean {
    const value = condition.value as string;
    return context?.gameState?.[`tech_${value}`] === true;
  }

  private checkHasStatus(condition: EventCondition, context?: EventContext): boolean {
    const value = condition.value as string;
    return context?.gameState?.[`status_${value}`] === true;
  }

  private checkHasVariable(condition: EventCondition, context?: EventContext): boolean {
    const variableName = condition.target;
    const value = context?.gameState?.[variableName];
    return value !== undefined;
  }

  private checkHealthAbove(condition: EventCondition, context?: EventContext): boolean {
    const threshold = condition.value as number;
    const health = context?.gameState?.['health'] as number || 0;
    return health > threshold;
  }

  private checkHealthBelow(condition: EventCondition, context?: EventContext): boolean {
    const threshold = condition.value as number;
    const health = context?.gameState?.['health'] as number || 0;
    return health < threshold;
  }

  private checkPositionInArea(condition: EventCondition, context?: EventContext): boolean {
    if (!context?.position) return false;
    const value = condition.value as { center: Position; radius: number };
    const pos = context.position;
    const distance = Math.sqrt(
      Math.pow(pos.x - value.center.x, 2) +
      Math.pow(pos.y - value.center.y, 2)
    );
    return distance <= value.radius;
  }

  private checkTimeElapsed(condition: EventCondition, context?: EventContext): boolean {
    const elapsed = context?.gameState?.['elapsed'] as number || 0;
    const required = condition.value as number;
    return this.compareValues(elapsed, required, condition.comparison);
  }

  private checkRandomChance(condition: EventCondition): boolean {
    const chance = condition.value as number;
    return Math.random() * 100 < chance;
  }

  private compareValues(actual: unknown, expected: unknown, comparison: ComparisonOperator): boolean {
    const a = actual as number;
    const e = expected as number;

    switch (comparison) {
      case 'equals':
        return a === e;
      case 'not_equals':
        return a !== e;
      case 'greater_than':
        return a > e;
      case 'less_than':
        return a < e;
      case 'greater_or_equal':
        return a >= e;
      case 'less_or_equal':
        return a <= e;
      default:
        return false;
    }
  }
}

class EffectProcessor {
  apply(effect: EventEffect, context?: EventContext): void {
    switch (effect.type) {
      case 'damage':
        this.applyDamage(effect, context);
        break;
      case 'heal':
        this.applyHeal(effect, context);
        break;
      case 'buff':
        this.applyBuff(effect, context);
        break;
      case 'debuff':
        this.applyDebuff(effect, context);
        break;
      case 'spawn':
        this.applySpawn(effect, context);
        break;
      case 'destroy':
        this.applyDestroy(effect, context);
        break;
      case 'modify_resources':
        this.applyResourceModify(effect, context);
        break;
      case 'modify_stat':
        this.applyStatModify(effect, context);
        break;
      case 'set_variable':
        this.applyVariableSet(effect, context);
        break;
      case 'modify_variable':
        this.applyVariableModify(effect, context);
        break;
      case 'play_sound':
        this.applyPlaySound(effect, context);
        break;
      case 'show_dialogue':
        this.applyShowDialogue(effect, context);
        break;
      case 'trigger_event':
        this.applyTriggerEvent(effect, context);
        break;
      default:
        console.log(`Effect type ${effect.type} not implemented`);
    }
  }

  private applyDamage(effect: EventEffect, context?: EventContext): void {
    const value = effect.value as number;
    const target = effect.target;
    console.log(`Applying ${value} damage to ${target}`);
  }

  private applyHeal(effect: EventEffect, context?: EventContext): void {
    const value = effect.value as number;
    const target = effect.target;
    console.log(`Applying ${value} healing to ${target}`);
  }

  private applyBuff(effect: EventEffect, context?: EventContext): void {
    const buffId = effect.value as string;
    const target = effect.target;
    console.log(`Applying buff ${buffId} to ${target}`);
  }

  private applyDebuff(effect: EventEffect, context?: EventContext): void {
    const debuffId = effect.value as string;
    const target = effect.target;
    console.log(`Applying debuff ${debuffId} to ${target}`);
  }

  private applySpawn(effect: EventEffect, context?: EventContext): void {
    const unitType = effect.value as string;
    const target = effect.target;
    console.log(`Spawning ${unitType} at ${target}`);
  }

  private applyDestroy(effect: EventEffect, context?: EventContext): void {
    const target = effect.target;
    console.log(`Destroying ${target}`);
  }

  private applyResourceModify(effect: EventEffect, context?: EventContext): void {
    const { resource, amount } = effect.value as { resource: string; amount: number };
    console.log(`Modifying ${resource} by ${amount}`);
  }

  private applyStatModify(effect: EventEffect, context?: EventContext): void {
    const { stat, modifier } = effect.value as { stat: string; modifier: number };
    const target = effect.target;
    console.log(`Modifying ${stat} of ${target} by ${modifier}`);
  }

  private applyVariableSet(effect: EventEffect, context?: EventContext): void {
    const { name, value } = effect.value as { name: string; value: unknown };
    console.log(`Setting variable ${name} to ${value}`);
  }

  private applyVariableModify(effect: EventEffect, context?: EventContext): void {
    const { name, operation, value } = effect.value as { name: string; operation: string; value: unknown };
    console.log(`Modifying variable ${name} with ${operation} by ${value}`);
  }

  private applyPlaySound(effect: EventEffect, context?: EventContext): void {
    const soundId = effect.value as string;
    console.log(`Playing sound: ${soundId}`);
  }

  private applyShowDialogue(effect: EventEffect, context?: EventContext): void {
    const dialogue = effect.value as DialogueLine[];
    console.log(`Showing dialogue:`, dialogue);
  }

  private applyTriggerEvent(effect: EventEffect, context?: EventContext): void {
    const eventId = effect.value as string;
    console.log(`Triggering event: ${eventId}`);
  }
}

export class StorySystem {
  private stories: Map<string, StoryManager> = new Map();
  private activeStory?: StoryManager;
  private listeners: Map<string, Set<Function>> = new Map();
  private eventSystem: GameEventSystem;

  constructor(eventSystem: GameEventSystem) {
    this.eventSystem = eventSystem;
  }

  createStory(config: StoryConfig): StoryManager {
    const story: StoryManager = {
      id: config.id || this.generateStoryId(),
      name: config.name,
      campaign: config.campaign,
      chapters: config.chapters,
      currentChapterIndex: 0,
      currentEpisodeIndex: 0,
      currentSceneIndex: 0,
      progress: {
        chapter: 0,
        episode: 0,
        scene: 0,
        totalScenes: this.countScenes(config.chapters),
        completedScenes: 0,
        currentObjectives: [],
        completedObjectives: [],
        bonusObjectivesCompleted: 0,
        totalBonusObjectives: 0,
        timeElapsed: 0,
        retries: 0
      },
      variables: {
        global: {},
        chapter: {},
        episode: {},
        scene: {},
        mission: {},
        player: {}
      },
      choices: {
        made: [],
        outcomes: {},
        consequences: [],
        flags: {}
      },
      unlocks: {
        chapters: [],
        episodes: [],
        missions: [],
        units: [],
        technologies: [],
        cosmetics: [],
        achievements: [],
        endings: [],
        bonuses: []
      },
      state: {
        currentScene: undefined,
        dialogueIndex: 0,
        choiceIndex: 0,
        objectiveProgress: {},
        activeEffects: [],
        triggeredEvents: [],
        completedTransitions: [],
        variables: {
          global: {},
          chapter: {},
          episode: {},
          scene: {},
          mission: {},
          player: {}
        }
      },
      settings: config.settings || {
        autoAdvance: true,
        autoAdvanceDelay: 3000,
        showObjectives: true,
        showDialogue: true,
        showSubtitles: true,
        skipEnabled: true,
        difficultyLocked: false,
        allowReplay: true,
        saveProgress: true,
        showTutorialHints: true,
        voiceEnabled: true,
        musicVolume: 0.7,
        effectsVolume: 1.0
      }
    };

    this.stories.set(story.id, story);

    if (!this.activeStory) {
      this.activeStory = story;
    }

    return story;
  }

  private generateStoryId(): string {
    return `story_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private countScenes(chapters: Chapter[]): number {
    let count = 0;
    chapters.forEach(chapter => {
      chapter.episodes.forEach(episode => {
        episode.scenes.forEach(scene => {
          count++;
        });
      });
    });
    return count;
  }

  startStory(storyId: string): boolean {
    const story = this.stories.get(storyId);
    if (!story) return false;

    this.activeStory = story;
    this.loadScene(0, 0, 0);

    this.emit('story_started', { story });
    return true;
  }

  loadScene(chapterIndex: number, episodeIndex: number, sceneIndex: number): Scene | null {
    if (!this.activeStory) return null;

    const chapter = this.activeStory.chapters[chapterIndex];
    if (!chapter) return null;

    const episode = chapter.episodes[episodeIndex];
    if (!episode) return null;

    const scene = episode.scenes[sceneIndex];
    if (!scene) return null;

    this.activeStory.currentChapterIndex = chapterIndex;
    this.activeStory.currentEpisodeIndex = episodeIndex;
    this.activeStory.currentSceneIndex = sceneIndex;

    this.activeStory.state.currentScene = scene;
    this.activeStory.state.dialogueIndex = 0;
    this.activeStory.state.choiceIndex = 0;

    this.activeStory.progress = {
      ...this.activeStory.progress,
      chapter: chapterIndex,
      episode: episodeIndex,
      scene: sceneIndex
    };

    scene.events.forEach(event => {
      this.eventSystem.createEvent({
        type: 'narrative',
        category: 'story',
        name: event.name,
        description: event.description,
        source: { type: 'narrative', id: event.id, name: event.name },
        data: { narrative: event.data?.narrative },
        effects: event.effects,
        conditions: event.conditions,
        triggers: event.triggers
      });
    });

    this.emit('scene_loaded', {
      story: this.activeStory,
      scene,
      chapterIndex,
      episodeIndex,
      sceneIndex
    });

    return scene;
  }

  nextDialogue(): DialogueLine | null {
    if (!this.activeStory?.state.currentScene) return null;

    const scene = this.activeStory.state.currentScene;
    if (!scene.dialogue || scene.dialogue.length === 0) return null;

    const index = this.activeStory.state.dialogueIndex;
    if (index >= scene.dialogue.length) return null;

    const line = scene.dialogue[index];
    this.activeStory.state.dialogueIndex++;

    this.emit('dialogue_advance', { line, index });

    return line;
  }

  makeChoice(choiceId: string): ChoiceOutcome | null {
    if (!this.activeStory?.state.currentScene) return null;

    const scene = this.activeStory.state.currentScene;
    const choice = scene.choices?.find(c => c.id === choiceId);
    if (!choice) return null;

    this.activeStory.choices.made.push(choiceId);
    this.activeStory.state.choiceIndex++;

    let selectedOutcome: ChoiceOutcome | null = null;

    for (const outcome of choice.outcomes) {
      if (Math.random() * 100 < outcome.probability) {
        selectedOutcome = outcome;
        break;
      }
    }

    if (!selectedOutcome) {
      selectedOutcome = choice.outcomes[0];
    }

    this.activeStory.choices.outcomes[choiceId] = selectedOutcome;

    selectedOutcome.effects.forEach(effect => {
      this.eventSystem.createEvent({
        type: 'triggered',
        category: 'story',
        name: `Choice Effect: ${choice.text}`,
        description: selectedOutcome?.narrative || '',
        source: { type: 'trigger', id: choiceId, name: choice.text },
        effects: selectedOutcome.effects
      });
    });

    this.emit('choice_made', { choice, outcome: selectedOutcome });

    return selectedOutcome;
  }

  completeObjective(objectiveId: string): void {
    if (!this.activeStory) return;

    const scene = this.activeStory.state.currentScene;
    const objective = scene?.objectives?.find(o => o.id === objectiveId);

    if (objective) {
      objective.completed = true;
      this.activeStory.progress.completedObjectives.push(objectiveId);

      if (objective.bonus) {
        this.activeStory.progress.bonusObjectivesCompleted++;
      }

      this.emit('objective_completed', { objective });
    }

    this.checkSceneCompletion();
  }

  private checkSceneCompletion(): void {
    if (!this.activeStory?.state.currentScene) return;

    const scene = this.activeStory.state.currentScene;
    const objectives = scene.objectives || [];

    const allPrimaryComplete = objectives
      .filter(o => o.type === 'primary' && !o.hidden)
      .every(o => o.completed);

    if (allPrimaryComplete) {
      this.completeScene();
    }
  }

  completeScene(): void {
    if (!this.activeStory) return;

    this.activeStory.progress.completedScenes++;

    const transition = this.selectNextTransition();

    if (transition) {
      this.executeTransition(transition);
    }

    this.emit('scene_completed', { scene: this.activeStory.state.currentScene });
  }

  private selectNextTransition(): SceneTransition | null {
    if (!this.activeStory?.state.currentScene) return null;

    const scene = this.activeStory.state.currentScene;
    const availableTransitions = scene.transitions.filter(t => {
      if (!t.conditions || t.conditions.length === 0) return true;
      return this.eventSystem.getEventsByType('triggered').some(e =>
        this.eventSystem.getEvent(e.id)?.state.completed
      );
    });

    return availableTransitions[0] || null;
  }

  private executeTransition(transition: SceneTransition): void {
    if (!this.activeStory) return;

    if (transition.effects) {
      transition.effects.forEach(effect => {
        this.eventSystem.createEvent({
          type: 'triggered',
          category: 'story',
          name: 'Transition Effect',
          description: '',
          source: { type: 'trigger', id: transition.id, name: transition.type },
          effects: [effect]
        });
      });
    }

    if (transition.targetScene) {
      const [chapter, episode, scene] = transition.targetScene.split('_').map(Number);
      setTimeout(() => {
        this.loadScene(chapter, episode, scene);
      }, transition.delay || 0);
    }

    this.emit('transition_executed', { transition });
  }

  skipDialogue(): void {
    if (!this.activeStory) return;

    const scene = this.activeStory.state.currentScene;
    if (!scene) return;

    this.activeStory.state.dialogueIndex = scene.dialogue?.length || 0;

    this.emit('dialogue_skipped', {});
  }

  skipScene(): void {
    if (!this.activeStory?.settings.skipEnabled) return;

    this.completeScene();
    this.emit('scene_skipped', {});
  }

  restartScene(): void {
    if (!this.activeStory) return;

    const { chapter, episode, scene } = this.activeStory.progress;
    this.activeStory.progress.retries++;

    this.loadScene(chapter, episode, scene);
    this.emit('scene_restarted', { chapter, episode, scene });
  }

  setVariable(scope: keyof StoryVariables, name: string, value: unknown): void {
    if (!this.activeStory) return;

    const variables = this.activeStory.variables[scope];
    if (variables) {
      variables[name] = value;
      this.emit('variable_changed', { scope, name, value });
    }
  }

  getVariable(scope: keyof StoryVariables, name: string): unknown {
    return this.activeStory?.variables[scope]?.[name];
  }

  setChoiceFlag(flag: string, value: boolean): void {
    if (!this.activeStory) return;

    this.activeStory.choices.flags[flag] = value;
    this.emit('flag_changed', { flag, value });
  }

  getChoiceFlag(flag: string): boolean {
    return this.activeStory?.choices.flags[flag] || false;
  }

  unlockContent(type: keyof StoryUnlocks, id: string): void {
    if (!this.activeStory) return;

    const unlocks = this.activeStory.unlocks[type];
    if (unlocks && !unlocks.includes(id)) {
      unlocks.push(id);
      this.emit('content_unlocked', { type, id });
    }
  }

  isContentUnlocked(type: keyof StoryUnlocks, id: string): boolean {
    const unlocks = this.activeStory?.unlocks[type];
    return unlocks ? unlocks.includes(id) : false;
  }

  getCurrentProgress(): StoryProgress | null {
    return this.activeStory?.progress || null;
  }

  getCurrentScene(): Scene | null {
    return this.activeStory?.state.currentScene || null;
  }

  getActiveStory(): StoryManager | undefined {
    return this.activeStory;
  }

  getAllStories(): StoryManager[] {
    return Array.from(this.stories.values());
  }

  saveProgress(): string | null {
    if (!this.activeStory) return null;

    const saveData = {
      storyId: this.activeStory.id,
      progress: this.activeStory.progress,
      variables: this.activeStory.variables,
      choices: this.activeStory.choices,
      unlocks: this.activeStory.unlocks,
      currentChapter: this.activeStory.currentChapterIndex,
      currentEpisode: this.activeStory.currentEpisodeIndex,
      currentScene: this.activeStory.currentSceneIndex
    };

    return JSON.stringify(saveData);
  }

  loadProgress(saveData: string): boolean {
    try {
      const data = JSON.parse(saveData);
      const story = this.stories.get(data.storyId);
      if (!story) return false;

      story.progress = data.progress;
      story.variables = data.variables;
      story.choices = data.choices;
      story.unlocks = data.unlocks;

      this.activeStory = story;
      this.loadScene(data.currentChapter, data.currentEpisode, data.currentScene);

      this.emit('progress_loaded', { story });
      return true;
    } catch (error) {
      console.error('Failed to load progress:', error);
      return false;
    }
  }

  resetProgress(): void {
    if (!this.activeStory) return;

    this.activeStory.progress = {
      chapter: 0,
      episode: 0,
      scene: 0,
      totalScenes: this.activeStory.progress.totalScenes,
      completedScenes: 0,
      currentObjectives: [],
      completedObjectives: [],
      bonusObjectivesCompleted: 0,
      totalBonusObjectives: 0,
      timeElapsed: 0,
      retries: 0
    };

    this.activeStory.variables = {
      global: {},
      chapter: {},
      episode: {},
      scene: {},
      mission: {},
      player: {}
    };

    this.activeStory.choices = {
      made: [],
      outcomes: {},
      consequences: [],
      flags: {}
    };

    this.activeStory.state = {
      currentScene: undefined,
      dialogueIndex: 0,
      choiceIndex: 0,
      objectiveProgress: {},
      activeEffects: [],
      triggeredEvents: [],
      completedTransitions: [],
      variables: {
        global: {},
        chapter: {},
        episode: {},
        scene: {},
        mission: {},
        player: {}
      }
    };

    this.emit('progress_reset', { storyId: this.activeStory.id });
  }

  endStory(): void {
    if (!this.activeStory) return;

    const story = this.activeStory;
    this.activeStory = undefined;

    this.emit('story_ended', { story });
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(cb => cb(data));
    }
  }
}

export interface StoryConfig {
  id?: string;
  name: string;
  campaign: Campaign;
  chapters: Chapter[];
  settings?: Partial<StorySettings>;
}

export const gameEventSystem = new GameEventSystem();
export const storySystem = new StorySystem(gameEventSystem);

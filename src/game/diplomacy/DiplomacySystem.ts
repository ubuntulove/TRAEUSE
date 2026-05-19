export enum RelationLevel {
  Allied = 5,
  Friendly = 4,
  Neutral = 3,
  Unfriendly = 2,
  Hostile = 1,
  War = 0,
}

export enum DiplomaticActionType {
  DeclareWar = 'declare_war',
  ProposeAlliance = 'propose_alliance',
  BreakAlliance = 'break_alliance',
  ProposeTradeAgreement = 'propose_trade_agreement',
  BreakTradeAgreement = 'break_trade_agreement',
  ProposeNonAggressionPact = 'propose_non_aggression_pact',
  BreakNAP = 'break_nap',
  Embargo = 'embargo',
  EstablishEmbassy = 'establish_embassy',
  RecallEmbassy = 'recall_embassy',
  VoteProposal = 'vote_proposal',
  SupportAlly = 'support_ally',
  BetrayAlly = 'betray_ally',
  PeaceOffer = 'peace_offer',
  DemandSurrender = 'demand_surrender',
  GiftResources = 'gift_resources',
  Threaten = 'threaten',
  Apologize = 'apologize',
}

export enum VoteResult {
  Pending = 'pending',
  Approved = 'approved',
  Rejected = 'rejected',
  Expired = 'expired',
  Tied = 'tied',
}

export enum EventSeverity {
  Info = 'info',
  Warning = 'warning',
  Critical = 'critical',
  Victory = 'victory',
  Defeat = 'defeat',
}

export enum VictoryType {
  Domination = 'domination',
  Diplomatic = 'diplomatic',
  Economic = 'economic',
  Alliance = 'alliance',
  Elimination = 'elimination',
}

export enum AIAggressionLevel {
  Passive = 'passive',
  Cautious = 'cautious',
  Balanced = 'balanced',
  Aggressive = 'aggressive',
  Paranoid = 'paranoid',
}

export enum AITrustLevel {
  Untrustworthy = 0,
  Low = 1,
  Moderate = 2,
  High = 3,
  Absolute = 4,
}

export interface Player {
  id: string;
  name: string;
  isAI: boolean;
  isAlive: boolean;
  teamId?: string;
  resources?: Record<string, number>;
}

export interface DiplomaticRelation {
  playerId: string;
  targetPlayerId: string;
  relationLevel: RelationLevel;
  relationScore: number;
  modifiers: RelationModifier[];
  activeAgreements: DiplomaticAgreement[];
  lastInteraction: number;
  trustLevel: AITrustLevel;
  hasEmbassy: boolean;
  embassyLevel: number;
  sharedBorder: boolean;
  tradeVolume: number;
  militaryThreat: number;
}

export interface RelationModifier {
  id: string;
  type: string;
  value: number;
  reason: string;
  source: string;
  timestamp: number;
  expiresAt?: number;
  stackable: boolean;
}

export interface DiplomaticAction {
  id: string;
  type: DiplomaticActionType;
  sourcePlayerId: string;
  targetPlayerId: string;
  timestamp: number;
  resolved: boolean;
  success: boolean;
  metadata?: Record<string, any>;
  effects?: DiplomaticEffects;
}

export interface DiplomaticEffects {
  relationChange: number;
  trustChange: number;
  modifier?: Partial<RelationModifier>;
  rewards?: Record<string, number>;
  penalties?: Record<string, number>;
  additionalEffects?: DiplomaticEffect[];
}

export interface DiplomaticEffect {
  type: string;
  target: string;
  value: number;
  duration?: number;
}

export interface Alliance {
  id: string;
  name: string;
  leaderId: string;
  memberIds: string[];
  createdAt: number;
  votes: Map<string, boolean>;
  totalVotes: number;
  requiredVotes: number;
  sharedVision: boolean;
  sharedTrade: boolean;
  sharedTechnology: boolean;
  jointMilitary: boolean;
  unifiedDiplomacy: boolean;
  diplomaticVotes: DiplomaticVotes[];
  sharedResources: Record<string, number>;
  alliedVictoryPoints: number;
  diplomaticProposals: DiplomaticProposal[];
  diplomaticHistory: DiplomaticAction[];
  alliedCommands: AllyCommand[];
  supportNetwork: SupportNetwork;
  resourceSharingRules?: ResourceSharingRules;
  militaryPactLevel: MilitaryPactLevel;
}

export enum MilitaryPactLevel {
  None = 0,
  Defensive = 1,
  Offensive = 2,
  Full = 3,
}

export interface ResourceSharingRules {
  enabled: boolean;
  percentageToShare: number;
  resourceTypes: string[];
  automaticDistribution: boolean;
}

export interface TradeAgreement {
  id: string;
  parties: string[];
  proposedBy: string;
  createdAt: number;
  expiresAt?: number;
  terms: TradeTerms;
  active: boolean;
  tradeVolume: number;
  benefits: TradeBenefits;
  penalties: TradePenalties;
  violations: TradeViolation[];
}

export interface TradeTerms {
  resources: Record<string, TradeResourceTerm>;
  technology: string[];
  unitTypes: string[];
  duration?: number;
  renewalTerms?: Partial<TradeTerms>;
}

export interface TradeResourceTerm {
  resourceType: string;
  amount: number;
  frequency: TradeFrequency;
  direction: TradeDirection;
  priceAdjustment?: number;
}

export enum TradeFrequency {
  OneTime = 'one_time',
  Hourly = 'hourly',
  Daily = 'daily',
  Weekly = 'weekly',
}

export enum TradeDirection {
  Unilateral = 'unilateral',
  Bilateral = 'bilateral',
  Multilateral = 'multilateral',
}

export interface TradeBenefits {
  resourceBonus: Record<string, number>;
  technologyBonus: string[];
  tradeRouteIncome: number;
  diplomaticBonus: number;
}

export interface TradePenalties {
  violationPenalty: number;
  terminationPenalty: number;
  embargoMultiplier: number;
}

export interface TradeViolation {
  id: string;
  violatingParty: string;
  type: string;
  timestamp: number;
  severity: number;
  description: string;
  penaltyApplied: boolean;
}

export interface NonAggressionPact {
  id: string;
  parties: string[];
  proposedBy: string;
  createdAt: number;
  expiresAt?: number;
  terms: NAPTerms;
  active: boolean;
  violations: NAPViolation[];
  extensions: NAPExtension[];
}

export interface NAPTerms {
  duration?: number;
  mutualDefense: boolean;
  guaranteedBorders: string[];
  noHostileActions: string[];
  resolutionMechanism?: string;
}

export interface NAPViolation {
  id: string;
  violatingParty: string;
  type: string;
  timestamp: number;
  evidence?: string[];
  penaltyApplied: boolean;
  forgiven: boolean;
}

export interface NAPExtension {
  id: string;
  proposedBy: string;
  duration: number;
  accepted: boolean;
  timestamp: number;
}

export interface DiplomaticVotes {
  id: string;
  allianceId: string;
  proposerId: string;
  type: VoteType;
  title: string;
  description: string;
  options: VoteOption[];
  deadline: number;
  results: Map<string, VoteResult>;
  totalVotesFor: number;
  totalVotesAgainst: number;
  totalVoters: number;
  status: VoteResult;
  executed: boolean;
  executionResult?: any;
}

export enum VoteType {
  AdmitMember = 'admit_member',
  RemoveMember = 'remove_member',
  DeclareWar = 'declare_war',
  ProposePeace = 'propose_peace',
  ChangeLeader = 'change_leader',
  ModifyAlliancePolicy = 'modify_alliance_policy',
  ShareResources = 'share_resources',
  JointMilitaryAction = 'joint_military_action',
  EmbargoTarget = 'embargo_target',
  SanctionMember = 'sanction_member',
  AllianceVote = 'alliance_vote',
}

export interface VoteOption {
  optionId: string;
  text: string;
  value: any;
}

export interface DiplomaticProposal {
  id: string;
  type: ProposalType;
  proposerId: string;
  targetAllianceId?: string;
  targetPlayerIds: string[];
  title: string;
  description: string;
  terms: any;
  status: ProposalStatus;
  votes: Map<string, boolean>;
  createdAt: number;
  expiresAt: number;
  acceptedAt?: number;
  rejectedAt?: number;
  negotiationRounds: NegotiationRound[];
}

export enum ProposalType {
  AllianceMembership = 'alliance_membership',
  TradeDeal = 'trade_deal',
  NonAggressionPact = 'non_aggression_pact',
  MilitaryAlliance = 'military_alliance',
  JointWarDeclaration = 'joint_war_declaration',
  TechnologySharing = 'technology_sharing',
  ResourceSharing = 'resource_sharing',
  PeaceTreaty = 'peace_treaty',
  SurrenderTerms = 'surrender_terms',
  TerritoryExchange = 'territory_exchange',
  Ceasefire = 'ceasefire',
}

export enum ProposalStatus {
  Proposed = 'proposed',
  Negotiating = 'negotiating',
  Accepted = 'accepted',
  Rejected = 'rejected',
  Expired = 'expired',
  Cancelled = 'cancelled',
}

export interface NegotiationRound {
  round: number;
  negotiatorId: string;
  counterproposal?: Partial<DiplomaticProposal>;
  comments?: string;
  timestamp: number;
}

export interface DiplomaticEvent {
  id: string;
  type: DiplomaticEventType;
  severity: EventSeverity;
  sourcePlayerId?: string;
  targetPlayerId?: string;
  allianceId?: string;
  timestamp: number;
  title: string;
  message: string;
  data?: Record<string, any>;
  read: boolean;
  dismissed: boolean;
  priority: number;
  relatedActionId?: string;
}

export enum DiplomaticEventType {
  RelationChanged = 'relation_changed',
  AllianceCreated = 'alliance_created',
  AllianceDissolved = 'alliance_dissolved',
  AllianceJoined = 'alliance_joined',
  AllianceLeft = 'alliance_left',
  AllianceKicked = 'alliance_kicked',
  TradeAgreementSigned = 'trade_agreement_signed',
  TradeAgreementBroken = 'trade_agreement_broken',
  NonAggressionPactSigned = 'non_aggression_pact_signed',
  NonAggressionPactBroken = 'non_aggression_pact_broken',
  WarDeclared = 'war_declared',
  PeaceSigned = 'peace_signed',
  VoteProposed = 'vote_proposed',
  VoteResult = 'vote_result',
  EmbassyEstablished = 'embassy_established',
  EmbassyRecalled = 'embassy_recalled',
  BetrayalDetected = 'betrayal_detected',
  BetrayalPunishment = 'betrayal_punishment',
  AIProposal = 'ai_proposal',
  VictoryAchieved = 'victory_achieved',
  DefeatSuffered = 'defeat_suffered',
  UltimatumReceived = 'ultimatum_received',
  UltimatumExpired = 'ultimatum_expired',
  AllyUnderAttack = 'ally_under_attack',
  AlliedVictory = 'allied_victory',
}

export interface AIPersonality {
  playerId: string;
  baseAggression: number;
  currentAggression: number;
  trustBaseline: number;
  currentTrust: Map<string, AITrustLevel>;
  loyaltyThreshold: number;
  betrayalTendency: number;
  alliancePreference: number;
  honorLevel: number;
  memorySpan: number;
  relationshipMemories: AIDiplomaticMemory[];
  negotiationStyle: AINegotiationStyle;
  communicationStyle: AICommunicationStyle;
  threatAssessment: AIT hreatAssessment;
  economicStrategy: AIEconomicStrategy;
  aggressionHistory: AIBetrayalRecord[];
}

export interface AIDiplomaticMemory {
  eventType: DiplomaticEventType;
  participantId: string;
  timestamp: number;
  outcome: string;
  sentiment: number;
  weight: number;
}

export enum AINegotiationStyle {
  Aggressive = 'aggressive',
  Diplomatic = 'diplomatic',
  Deceptive = 'deceptive',
  Honest = 'honest',
  Adaptive = 'adaptive',
}

export enum AICommunicationStyle {
  Formal = 'formal',
  Casual = 'casual',
  Threatening = 'threatening',
  Friendly = 'friendly',
}

export interface AIT hreatAssessment {
  currentThreats: Map<string, ThreatLevel>;
  threatMemory: ThreatEvent[];
  responseStrategy: ThreatResponseStrategy;
}

export enum ThreatLevel {
  None = 0,
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4,
}

export enum ThreatResponseStrategy {
  Avoid = 'avoid',
  Defend = 'defend',
  Counterattack = 'counterattack',
  SeekAlliance = 'seek_alliance',
}

export enum AIEconomicStrategy {
  Expansionist = 'expansionist',
  Conservative = 'conservative',
  Balanced = 'balanced',
  Mercantile = 'mercantile',
}

export interface AIBetrayalRecord {
  targetId: string;
  betrayalType: DiplomaticActionType;
  timestamp: number;
  detectionDifficulty: number;
  reputationPenalty: number;
  forgiveable: boolean;
}

export interface Embassy {
  id: string;
  ownerId: string;
  hostPlayerId: string;
  level: number;
  establishedAt: number;
  intelligenceLevel: number;
  influenceLevel: number;
  tradeBonus: number;
  espionageCapability: number;
  upgradeCost: Record<string, number>;
  missions: EmbassyMission[];
  staff: EmbassyStaff[];
  security: EmbassySecurity;
  covertOperations: CovertOperation[];
}

export interface EmbassyMission {
  id: string;
  type: EmbassyMissionType;
  status: MissionStatus;
  startTime: number;
  completionTime?: number;
  progress: number;
  cost: Record<string, number>;
  rewards: EmbassyMissionReward;
  risk: MissionRisk;
  discovered: boolean;
}

export enum EmbassyMissionType {
  IntelligenceGathering = 'intelligence_gathering',
  InfluenceCampaign = 'influence_campaign',
  TradePromotion = 'trade_promotion',
  Sabotage = 'sabotage',
  Assassination = 'assassination',
  Propaganda = 'propaganda',
  DiplomaticLiaison = 'diplomatic_liaison',
  TechnologyTheft = 'technology_theft',
  ResourceSurvey = 'resource_survey',
  DefectionInducement = 'defection_inducement',
}

export enum MissionStatus {
  Available = 'available',
  InProgress = 'in_progress',
  Completed = 'completed',
  Failed = 'failed',
  Discovered = 'discovered',
  Cancelled = 'cancelled',
}

export interface MissionRisk {
  baseRisk: number;
  hostRelationsModifier: number;
  securityLevelModifier: number;
  missionTypeModifier: number;
}

export interface EmbassyMissionReward {
  intelligence?: number;
  influence?: number;
  gold?: number;
  technology?: string[];
  information?: string;
}

export interface EmbassyStaff {
  id: string;
  name: string;
  role: EmbassyRole;
  level: number;
  efficiency: number;
  loyalty: number;
  specializations: string[];
}

export enum EmbassyRole {
  Ambassador = 'ambassador',
  Spy = 'spy',
  Merchant = 'merchant',
  Scholar = 'scholar',
  Culturist = 'culturist',
  Agent = 'agent',
}

export interface EmbassySecurity {
  level: number;
  detectionChance: number;
  protectionBonus: number;
  countermeasures: string[];
  compromised: boolean;
  lastAudit: number;
}

export interface CovertOperation {
  id: string;
  type: string;
  targetPlayerId: string;
  executorId: string;
  status: MissionStatus;
  risk: number;
  reward: any;
  discoveredBy: string[];
  evidence: string[];
  coverStory?: string;
  successProbability: number;
}

export interface AllyCommand {
  id: string;
  allianceId: string;
  commanderId: string;
  type: AllyCommandType;
  targetPlayerId?: string;
  targetLocation?: string;
  priority: number;
  status: CommandStatus;
  createdAt: number;
  acceptedBy: string[];
  rejectedBy: string[];
  deadline?: number;
  executionTime?: number;
  results?: CommandResult[];
}

export enum AllyCommandType {
  Attack = 'attack',
  Defend = 'defend',
  Reinforce = 'reinforce',
  Scout = 'scout',
  Support = 'support',
  Withdraw = 'withdraw',
  HoldPosition = 'hold_position',
  Coordinate = 'coordinate',
  ShareIntelligence = 'share_intelligence',
  Distract = 'distract',
}

export enum CommandStatus {
  Proposed = 'proposed',
  Accepted = 'accepted',
  Rejected = 'rejected',
  Executing = 'executing',
  Completed = 'completed',
  Failed = 'failed',
  Cancelled = 'cancelled',
  Expired = 'expired',
}

export interface CommandResult {
  playerId: string;
  success: boolean;
  outcome: string;
  unitsDeployed?: string[];
  casualties?: string[];
  resourcesSpent?: Record<string, number>;
  territoryGained?: string[];
  intelligenceGathered?: any;
}

export interface SupportNetwork {
  activeSupports: ActiveSupport[];
  supportRequests: SupportRequest[];
  resourceSharing: ResourceSharing[];
  militaryAid: MilitaryAid[];
  intelligenceSharing: IntelligenceSharing[];
}

export interface ActiveSupport {
  id: string;
  supportedPlayerId: string;
  supportingPlayerId: string;
  type: SupportType;
  amount: number;
  duration: number;
  startTime: number;
  active: boolean;
}

export enum SupportType {
  Economic = 'economic',
  Military = 'military',
  Diplomatic = 'diplomatic',
  Intelligence = 'intelligence',
  Technological = 'technological',
}

export interface SupportRequest {
  id: string;
  requestingPlayerId: string;
  supportType: SupportType;
  amount: number;
  urgency: number;
  justification: string;
  status: RequestStatus;
  responses: Map<string, SupportResponse>;
  deadline: number;
}

export enum RequestStatus {
  Pending = 'pending',
  Approved = 'approved',
  PartiallyApproved = 'partially_approved',
  Rejected = 'rejected',
  Fulfilled = 'fulfilled',
  Expired = 'expired',
}

export interface SupportResponse {
  playerId: string;
  approved: boolean;
  amount?: number;
  reason?: string;
  timestamp: number;
}

export interface ResourceSharing {
  id: string;
  fromPlayerId: string;
  toPlayerId: string;
  resourceType: string;
  amount: number;
  frequency: TradeFrequency;
  startTime: number;
  endTime?: number;
  active: boolean;
  emergencyPause: boolean;
}

export interface MilitaryAid {
  id: string;
  fromPlayerId: string;
  toPlayerId: string;
  unitTypes: string[];
  unitCounts: Record<string, number>;
  deploymentLocation?: string;
  commandAuthority: CommandAuthority;
  duration?: number;
  returnCondition?: string;
  active: boolean;
}

export enum CommandAuthority {
  FullControl = 'full_control',
  Coordinated = 'coordinated',
  Advisory = 'advisory',
}

export interface IntelligenceSharing {
  id: string;
  fromPlayerId: string;
  toPlayerId: string;
  intelligenceType: IntelligenceType;
  priority: IntelligencePriority;
  sharingLevel: IntelligenceSharingLevel;
  automatic: boolean;
  filters?: string[];
  lastShared: number;
}

export enum IntelligenceType {
  Military = 'military',
  Economic = 'economic',
  Diplomatic = 'diplomatic',
  Technological = 'technological',
  All = 'all',
}

export enum IntelligencePriority {
  Critical = 3,
  High = 2,
  Normal = 1,
  Low = 0,
}

export enum IntelligenceSharingLevel {
  None = 0,
  Basic = 1,
  Standard = 2,
  Full = 3,
}

export interface DiplomaticVictory {
  type: VictoryType;
  achievedBy: string;
  allies: string[];
  conditions: VictoryCondition[];
  completedAt: number;
  proof: VictoryProof;
}

export interface VictoryCondition {
  type: string;
  description: string;
  target: number;
  current: number;
  completed: boolean;
  progress: number;
  bonusEffects?: any[];
}

export interface VictoryProof {
  allianceStrength: number;
  diplomaticAgreements: number;
  alliesCount: number;
  treatiesSigned: number;
  betrayalCount: number;
  betrayedCount: number;
  warVictories: number;
  peaceTreaties: number;
}

export class DiplomacySystem {
  private relations: Map<string, DiplomaticRelation>;
  private alliances: Map<string, Alliance>;
  private tradeAgreements: Map<string, TradeAgreement>;
  private nonAggressionPacts: Map<string, NonAggressionPact>;
  private diplomaticEvents: DiplomaticEvent[];
  private aiPersonalities: Map<string, AIPersonality>;
  private embassies: Map<string, Embassy>;
  private diplomaticVotes: Map<string, DiplomaticVotes>;
  private diplomaticProposals: Map<string, DiplomaticProposal>;
  private betrayalRecords: Map<string, BetrayalRecord[]>;
  private victoryTracking: Map<string, DiplomaticVictory>;
  private players: Map<string, Player>;
  private eventListeners: Map<string, Function[]>;
  private config: DiplomacyConfig;
  private tickCounter: number;
  private globalRelations: Map<string, number>;
  private activeWars: Set<string>;

  constructor(config?: Partial<DiplomacyConfig>) {
    this.relations = new Map();
    this.alliances = new Map();
    this.tradeAgreements = new Map();
    this.nonAggressionPacts = new Map();
    this.diplomaticEvents = [];
    this.aiPersonalities = new Map();
    this.embassies = new Map();
    this.diplomaticVotes = new Map();
    this.diplomaticProposals = new Map();
    this.betrayalRecords = new Map();
    this.victoryTracking = new Map();
    this.players = new Map();
    this.eventListeners = new Map();
    this.tickCounter = 0;
    this.globalRelations = new Map();
    this.activeWars = new Set();

    this.config = {
      maxRelationScore: 1000,
      minRelationScore: -1000,
      relationChangeRate: 0.1,
      betrayalPenaltyMultiplier: 2.0,
      allianceSizeLimit: 10,
      embassyMaxLevel: 5,
      voteDuration: 300000,
      proposalExpirationTime: 600000,
      trustDecayRate: 0.01,
      minTrustForAlliance: AITrustLevel.Moderate,
      ...config,
    };

    this.initializeGlobalRelations();
  }

  private initializeGlobalRelations(): void {
    const relationKeys = [
      'global_hostility',
      'global_cooperation',
      'trade_activity',
      'military_tension',
    ];
    relationKeys.forEach((key) => this.globalRelations.set(key, 0));
  }

  public registerPlayer(player: Player): void {
    this.players.set(player.id, player);

    if (player.isAI) {
      this.initializeAIPersonality(player.id);
    }

    this.initializeRelationsForNewPlayer(player.id);
  }

  private initializeRelationsForNewPlayer(playerId: string): void {
    this.players.forEach((player) => {
      if (player.id !== playerId) {
        const relation = this.createDefaultRelation(playerId, player.id);
        this.relations.set(this.getRelationKey(playerId, player.id), relation);
      }
    });
  }

  private createDefaultRelation(playerId: string, targetPlayerId: string): DiplomaticRelation {
    return {
      playerId,
      targetPlayerId,
      relationLevel: RelationLevel.Neutral,
      relationScore: 0,
      modifiers: [],
      activeAgreements: [],
      lastInteraction: Date.now(),
      trustLevel: AITrustLevel.Low,
      hasEmbassy: false,
      embassyLevel: 0,
      sharedBorder: false,
      tradeVolume: 0,
      militaryThreat: 0,
    };
  }

  private initializeAIPersonality(playerId: string): void {
    const personality: AIPersonality = {
      playerId,
      baseAggression: this.randomInRange(20, 80),
      currentAggression: 0,
      trustBaseline: this.randomInRange(30, 70),
      currentTrust: new Map(),
      loyaltyThreshold: this.randomInRange(40, 70),
      betrayalTendency: this.randomInRange(10, 60),
      alliancePreference: this.randomInRange(30, 90),
      honorLevel: this.randomInRange(20, 80),
      memorySpan: this.randomInRange(5000, 20000),
      relationshipMemories: [],
      negotiationStyle: this.randomNegotiationStyle(),
      communicationStyle: this.randomCommunicationStyle(),
      threatAssessment: {
        currentThreats: new Map(),
        threatMemory: [],
        responseStrategy: ThreatResponseStrategy.Balanced,
      },
      economicStrategy: AIEconomicStrategy.Balanced,
      aggressionHistory: [],
    };

    personality.currentAggression = personality.baseAggression;
    this.players.forEach((player) => {
      if (player.id !== playerId) {
        personality.currentTrust.set(player.id, AITrustLevel.Moderate);
      }
    });

    this.aiPersonalities.set(playerId, personality);
  }

  private randomNegotiationStyle(): AINegotiationStyle {
    const styles = Object.values(AINegotiationStyle);
    return styles[Math.floor(Math.random() * styles.length)];
  }

  private randomCommunicationStyle(): AICommunicationStyle {
    const styles = Object.values(AICommunicationStyle);
    return styles[Math.floor(Math.random() * styles.length)];
  }

  private randomInRange(min: number, max: number): number {
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }

  private getRelationKey(playerId: string, targetPlayerId: string): string {
    return `${playerId}_${targetPlayerId}`;
  }

  public getRelation(playerId: string, targetPlayerId: string): DiplomaticRelation | null {
    return this.relations.get(this.getRelationKey(playerId, targetPlayerId)) || null;
  }

  public modifyRelation(
    playerId: string,
    targetPlayerId: string,
    modifier: Partial<RelationModifier>
  ): DiplomaticRelation | null {
    const relation = this.getRelation(playerId, targetPlayerId);
    if (!relation) return null;

    const newModifier: RelationModifier = {
      id: this.generateId(),
      type: modifier.type || 'general',
      value: modifier.value || 0,
      reason: modifier.reason || 'Unspecified',
      source: modifier.source || playerId,
      timestamp: Date.now(),
      expiresAt: modifier.expiresAt,
      stackable: modifier.stackable !== false,
    };

    relation.modifiers.push(newModifier);
    relation.lastInteraction = Date.now();

    this.recalculateRelationScore(relation);
    this.updateRelationLevel(relation);

    if (this.isPlayerAI(playerId)) {
      this.updateAITrust(playerId, targetPlayerId, newModifier.value);
    }

    this.emitEvent({
      type: DiplomaticEventType.RelationChanged,
      severity: EventSeverity.Info,
      sourcePlayerId: playerId,
      targetPlayerId,
      title: 'Relation Modified',
      message: `Relation between ${playerId} and ${targetPlayerId} modified by ${newModifier.value}`,
      data: { modifier: newModifier },
    });

    return relation;
  }

  private recalculateRelationScore(relation: DiplomaticRelation): void {
    let totalModifier = 0;
    const now = Date.now();

    relation.modifiers = relation.modifiers.filter((mod) => {
      if (mod.expiresAt && mod.expiresAt < now) {
        return false;
      }
      totalModifier += mod.stackable ? mod.value : Math.max(0, mod.value);
      return true;
    });

    relation.relationScore = Math.max(
      this.config.minRelationScore,
      Math.min(this.config.maxRelationScore, totalModifier)
    );
  }

  private updateRelationLevel(relation: DiplomaticRelation): void {
    const score = relation.relationScore;

    if (score >= 800) {
      relation.relationLevel = RelationLevel.Allied;
    } else if (score >= 500) {
      relation.relationLevel = RelationLevel.Friendly;
    } else if (score >= 200) {
      relation.relationLevel = RelationLevel.Neutral;
    } else if (score >= -200) {
      relation.relationLevel = RelationLevel.Unfriendly;
    } else if (score >= -500) {
      relation.relationLevel = RelationLevel.Hostile;
    } else {
      relation.relationLevel = RelationLevel.War;
    }
  }

  private updateAITrust(playerId: string, targetPlayerId: string, change: number): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    const currentTrust = personality.currentTrust.get(targetPlayerId) || AITrustLevel.Low;
    let newTrust = currentTrust;

    if (change > 50) {
      newTrust = Math.min(AITrustLevel.Absolute, currentTrust + 1);
    } else if (change > 20) {
      newTrust = Math.min(AITrustLevel.Absolute, currentTrust + 1);
    } else if (change < -50) {
      newTrust = Math.max(AITrustLevel.Untrustworthy, currentTrust - 2);
    } else if (change < -20) {
      newTrust = Math.max(AITrustLevel.Untrustworthy, currentTrust - 1);
    }

    personality.currentTrust.set(targetPlayerId, newTrust);

    const memory: AIDiplomaticMemory = {
      eventType: DiplomaticEventType.RelationChanged,
      participantId: targetPlayerId,
      timestamp: Date.now(),
      outcome: change > 0 ? 'positive' : 'negative',
      sentiment: change,
      weight: Math.abs(change) / 100,
    };

    personality.relationshipMemories.push(memory);
    this.pruneAIMemories(personality);
  }

  private pruneAIMemories(personality: AIPersonality): void {
    const cutoffTime = Date.now() - personality.memorySpan;
    personality.relationshipMemories = personality.relationshipMemories.filter(
      (mem) => mem.timestamp > cutoffTime
    );
  }

  public declareWar(playerId: string, targetPlayerId: string): DiplomaticAction | null {
    const relation = this.getRelation(playerId, targetPlayerId);
    if (!relation) return null;

    if (this.hasActiveNAP(playerId, targetPlayerId)) {
      this.recordNAPViolation(playerId, targetPlayerId);
    }

    this.removeAllAgreements(playerId, targetPlayerId);

    const action = this.createDiplomaticAction(DiplomaticActionType.DeclareWar, playerId, targetPlayerId, {
      relationChange: -500,
      trustChange: -4,
      penalties: {
        reputation: 100,
      },
    });

    relation.relationLevel = RelationLevel.War;
    relation.relationScore = this.config.minRelationScore;

    this.activeWars.add(this.getRelationKey(playerId, targetPlayerId));

    this.emitEvent({
      type: DiplomaticEventType.WarDeclared,
      severity: EventSeverity.Critical,
      sourcePlayerId: playerId,
      targetPlayerId,
      title: 'War Declared',
      message: `${playerId} has declared war on ${targetPlayerId}!`,
      data: { action },
    });

    this.handleWarDeclarationEffects(playerId, targetPlayerId);

    return action;
  }

  private hasActiveNAP(playerId: string, targetPlayerId: string): boolean {
    let hasActiveNAP = false;
    this.nonAggressionPacts.forEach((nap) => {
      if (nap.active && nap.parties.includes(playerId) && nap.parties.includes(targetPlayerId)) {
        hasActiveNAP = true;
      }
    });
    return hasActiveNAP;
  }

  private removeAllAgreements(playerId: string, targetPlayerId: string): void {
    this.tradeAgreements.forEach((agreement) => {
      if (agreement.active && agreement.parties.includes(playerId) && agreement.parties.includes(targetPlayerId)) {
        agreement.active = false;
        this.emitEvent({
          type: DiplomaticEventType.TradeAgreementBroken,
          severity: EventSeverity.Warning,
          sourcePlayerId: playerId,
          targetPlayerId,
          title: 'Trade Agreement Broken',
          message: `Trade agreement between ${playerId} and ${targetPlayerId} has been terminated due to war.`,
        });
      }
    });

    const alliance1 = this.findAllianceContaining(playerId);
    const alliance2 = this.findAllianceContaining(targetPlayerId);

    if (alliance1 && alliance2 && alliance1.id === alliance2.id) {
      this.handleAllianceWar(alliance1.id, playerId, targetPlayerId);
    }
  }

  private handleWarDeclarationEffects(playerId: string, targetPlayerId: string): void {
    const alliance = this.findAllianceContaining(playerId);
    if (alliance) {
      alliance.alliedVictoryPoints = Math.max(0, alliance.alliedVictoryPoints - 50);

      this.alliances.forEach((ally) => {
        if (ally.id !== alliance.id && this.areAlliancesAllied(ally.id, alliance.id)) {
          this.breakAllianceRelations(ally.id, alliance.id);
        }
      });
    }

    this.modifyRelation(playerId, targetPlayerId, {
      type: 'war_declaration',
      value: -500,
      reason: 'War declaration',
      source: playerId,
    });

    this.players.forEach((player) => {
      if (player.id !== playerId && player.id !== targetPlayerId) {
        const relation = this.getRelation(playerId, player.id);
        if (relation && relation.relationLevel === RelationLevel.Allied) {
          this.modifyRelation(playerId, player.id, {
            type: 'ally_declared_war',
            value: -30,
            reason: `Ally ${playerId} declared war on ${targetPlayerId}`,
            source: playerId,
          });
        }
      }
    });
  }

  public proposePeace(playerId: string, targetPlayerId: string): DiplomaticProposal | null {
    const relation = this.getRelation(playerId, targetPlayerId);
    if (!relation || relation.relationLevel !== RelationLevel.War) {
      return null;
    }

    const proposal: DiplomaticProposal = {
      id: this.generateId(),
      type: ProposalType.PeaceTreaty,
      proposerId: playerId,
      targetPlayerIds: [targetPlayerId],
      title: 'Peace Treaty Proposal',
      description: `${playerId} proposes peace to ${targetPlayerId}`,
      terms: {
        war reparations: 0,
        territory_exchange: [],
        diplomatic_normalization: true,
      },
      status: ProposalStatus.Proposed,
      votes: new Map(),
      createdAt: Date.now(),
      expiresAt: Date.now() + this.config.proposalExpirationTime,
      negotiationRounds: [],
    };

    this.diplomaticProposals.set(proposal.id, proposal);

    this.emitEvent({
      type: DiplomaticEventType.VoteProposed,
      severity: EventSeverity.Info,
      sourcePlayerId: playerId,
      targetPlayerId,
      title: 'Peace Proposal',
      message: `${playerId} has proposed a peace treaty to ${targetPlayerId}`,
      data: { proposal },
    });

    return proposal;
  }

  public acceptPeace(proposalId: string, acceptorId: string): boolean {
    const proposal = this.diplomaticProposals.get(proposalId);
    if (!proposal || proposal.status !== ProposalStatus.Proposed) {
      return false;
    }

    if (!proposal.targetPlayerIds.includes(acceptorId)) {
      return false;
    }

    proposal.status = ProposalStatus.Accepted;
    proposal.acceptedAt = Date.now();

    const warKey = this.getRelationKey(proposal.proposerId, acceptorId);
    this.activeWars.delete(warKey);

    this.modifyRelation(proposal.proposerId, acceptorId, {
      type: 'peace_agreement',
      value: 100,
      reason: 'Peace treaty signed',
      source: proposal.proposerId,
    });

    this.emitEvent({
      type: DiplomaticEventType.PeaceSigned,
      severity: EventSeverity.Info,
      sourcePlayerId: proposal.proposerId,
      targetPlayerId: acceptorId,
      title: 'Peace Signed',
      message: `Peace treaty signed between ${proposal.proposerId} and ${acceptorId}`,
      data: { proposal },
    });

    return true;
  }

  public createAlliance(leaderId: string, name: string): Alliance | null {
    const leader = this.players.get(leaderId);
    if (!leader) return null;

    const existingAlliance = this.findAllianceContaining(leaderId);
    if (existingAlliance) return null;

    const alliance: Alliance = {
      id: this.generateId(),
      name,
      leaderId,
      memberIds: [leaderId],
      createdAt: Date.now(),
      votes: new Map(),
      totalVotes: 0,
      requiredVotes: 1,
      sharedVision: true,
      sharedTrade: false,
      sharedTechnology: false,
      jointMilitary: false,
      unifiedDiplomacy: false,
      diplomaticVotes: [],
      sharedResources: {},
      alliedVictoryPoints: 100,
      diplomaticProposals: [],
      diplomaticHistory: [],
      alliedCommands: [],
      supportNetwork: {
        activeSupports: [],
        supportRequests: [],
        resourceSharing: [],
        militaryAid: [],
        intelligenceSharing: [],
      },
      resourceSharingRules: {
        enabled: false,
        percentageToShare: 10,
        resourceTypes: ['gold'],
        automaticDistribution: false,
      },
      militaryPactLevel: MilitaryPactLevel.None,
    };

    this.alliances.set(alliance.id, alliance);

    this.emitEvent({
      type: DiplomaticEventType.AllianceCreated,
      severity: EventSeverity.Info,
      sourcePlayerId: leaderId,
      title: 'Alliance Created',
      message: `${leader.name || leaderId} has created the alliance "${name}"`,
      data: { alliance },
    });

    return alliance;
  }

  public inviteToAlliance(allianceId: string, inviterId: string, targetPlayerId: string): DiplomaticProposal | null {
    const alliance = this.alliances.get(allianceId);
    const inviter = this.players.get(inviterId);

    if (!alliance || !inviter) return null;

    if (!alliance.memberIds.includes(inviterId)) return null;

    if (alliance.memberIds.length >= this.config.allianceSizeLimit) return null;

    const targetPlayer = this.players.get(targetPlayerId);
    if (!targetPlayer) return null;

    const existingAlliance = this.findAllianceContaining(targetPlayerId);
    if (existingAlliance) return null;

    const proposal: DiplomaticProposal = {
      id: this.generateId(),
      type: ProposalType.AllianceMembership,
      proposerId: inviterId,
      targetAllianceId: allianceId,
      targetPlayerIds: [targetPlayerId],
      title: `Alliance Invitation: ${alliance.name}`,
      description: `${inviter.name || inviterId} invites you to join ${alliance.name}`,
      terms: {
        allianceId,
        allianceName: alliance.name,
        sharedVision: alliance.sharedVision,
        sharedTrade: alliance.sharedTrade,
        sharedTechnology: alliance.sharedTechnology,
      },
      status: ProposalStatus.Proposed,
      votes: new Map(),
      createdAt: Date.now(),
      expiresAt: Date.now() + this.config.proposalExpirationTime,
      negotiationRounds: [],
    };

    this.diplomaticProposals.set(proposal.id, proposal);

    this.emitEvent({
      type: DiplomaticEventType.VoteProposed,
      severity: EventSeverity.Info,
      sourcePlayerId: inviterId,
      targetPlayerId,
      title: 'Alliance Invitation',
      message: `${inviter.name || inviterId} has invited you to join ${alliance.name}`,
      data: { proposal },
    });

    return proposal;
  }

  public respondToAllianceInvite(proposalId: string, playerId: string, accept: boolean): boolean {
    const proposal = this.diplomaticProposals.get(proposalId);
    if (!proposal || proposal.type !== ProposalType.AllianceMembership) return false;

    if (!proposal.targetPlayerIds.includes(playerId)) return false;

    if (accept) {
      return this.acceptAllianceInvite(proposal);
    } else {
      proposal.status = ProposalStatus.Rejected;
      proposal.rejectedAt = Date.now();
      return true;
    }
  }

  private acceptAllianceInvite(proposal: DiplomaticProposal): boolean {
    const alliance = this.alliances.get(proposal.targetAllianceId!);
    if (!alliance) return false;

    const newMemberId = proposal.targetPlayerIds[0];

    alliance.memberIds.push(newMemberId);
    alliance.requiredVotes = Math.ceil(alliance.memberIds.length / 2);

    proposal.status = ProposalStatus.Accepted;
    proposal.acceptedAt = Date.now();

    alliance.memberIds.forEach((memberId) => {
      if (memberId !== newMemberId) {
        this.modifyRelation(newMemberId, memberId, {
          type: 'alliance_joined',
          value: 50,
          reason: `Joined alliance ${alliance.name}`,
          source: newMemberId,
        });

        this.modifyRelation(memberId, newMemberId, {
          type: 'alliance_joined',
          value: 50,
          reason: `Joined alliance ${alliance.name}`,
          source: memberId,
        });
      }
    });

    this.initializeAllianceIntelligenceSharing(alliance.id, newMemberId);

    this.emitEvent({
      type: DiplomaticEventType.AllianceJoined,
      severity: EventSeverity.Info,
      sourcePlayerId: newMemberId,
      allianceId: alliance.id,
      title: 'Alliance Joined',
      message: `${this.players.get(newMemberId)?.name || newMemberId} has joined ${alliance.name}`,
      data: { alliance },
    });

    this.updateVictoryConditions(newMemberId);

    return true;
  }

  private initializeAllianceIntelligenceSharing(allianceId: string, newMemberId: string): void {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return;

    alliance.memberIds.forEach((memberId) => {
      if (memberId !== newMemberId) {
        const sharing: IntelligenceSharing = {
          id: this.generateId(),
          fromPlayerId: newMemberId,
          toPlayerId: memberId,
          intelligenceType: IntelligenceType.All,
          priority: IntelligencePriority.Normal,
          sharingLevel: IntelligenceSharingLevel.Standard,
          automatic: true,
          lastShared: Date.now(),
        };

        alliance.supportNetwork.intelligenceSharing.push(sharing);
      }
    });
  }

  public leaveAlliance(playerId: string): boolean {
    const alliance = this.findAllianceContaining(playerId);
    if (!alliance) return false;

    if (alliance.leaderId === playerId) {
      if (alliance.memberIds.length > 1) {
        const newLeader = alliance.memberIds.find((id) => id !== playerId);
        if (newLeader) {
          alliance.leaderId = newLeader;
        }
      } else {
        this.dissolveAlliance(alliance.id);
        return true;
      }
    }

    alliance.memberIds = alliance.memberIds.filter((id) => id !== playerId);
    alliance.requiredVotes = Math.ceil(alliance.memberIds.length / 2);

    this.cleanupAllianceIntelligenceSharing(alliance.id, playerId);

    alliance.memberIds.forEach((memberId) => {
      this.modifyRelation(playerId, memberId, {
        type: 'left_alliance',
        value: -20,
        reason: `Left alliance ${alliance.name}`,
        source: playerId,
      });
    });

    this.emitEvent({
      type: DiplomaticEventType.AllianceLeft,
      severity: EventSeverity.Info,
      sourcePlayerId: playerId,
      allianceId: alliance.id,
      title: 'Alliance Left',
      message: `${this.players.get(playerId)?.name || playerId} has left ${alliance.name}`,
      data: { alliance },
    });

    return true;
  }

  private cleanupAllianceIntelligenceSharing(allianceId: string, playerId: string): void {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return;

    alliance.supportNetwork.intelligenceSharing = alliance.supportNetwork.intelligenceSharing.filter(
      (sharing) => sharing.fromPlayerId !== playerId && sharing.toPlayerId !== playerId
    );
  }

  public dissolveAlliance(allianceId: string): boolean {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return false;

    alliance.memberIds.forEach((memberId) => {
      this.players.forEach((player) => {
        if (player.id !== memberId && this.isAllianceMember(player.id, allianceId)) {
          this.modifyRelation(memberId, player.id, {
            type: 'alliance_dissolved',
            value: -30,
            reason: `Alliance ${alliance.name} was dissolved`,
            source: memberId,
          });
        }
      });
    });

    this.emitEvent({
      type: DiplomaticEventType.AllianceDissolved,
      severity: EventSeverity.Warning,
      allianceId,
      title: 'Alliance Dissolved',
      message: `The alliance "${alliance.name}" has been dissolved`,
      data: { alliance },
    });

    this.alliances.delete(allianceId);
    return true;
  }

  public kickFromAlliance(allianceId: string, kickerId: string, targetPlayerId: string): boolean {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return false;

    if (alliance.leaderId !== kickerId) return false;

    if (!alliance.memberIds.includes(targetPlayerId)) return false;

    alliance.memberIds = alliance.memberIds.filter((id) => id !== targetPlayerId);
    alliance.requiredVotes = Math.ceil(alliance.memberIds.length / 2);

    this.cleanupAllianceIntelligenceSharing(allianceId, targetPlayerId);

    alliance.memberIds.forEach((memberId) => {
      this.modifyRelation(targetPlayerId, memberId, {
        type: 'kicked_from_alliance',
        value: -50,
        reason: `Kicked from alliance ${alliance.name}`,
        source: kickerId,
      });

      this.modifyRelation(memberId, targetPlayerId, {
        type: 'kicked_from_alliance',
        value: -20,
        reason: `Kicked from alliance ${alliance.name}`,
        source: kickerId,
      });
    });

    this.emitEvent({
      type: DiplomaticEventType.AllianceKicked,
      severity: EventSeverity.Warning,
      sourcePlayerId: kickerId,
      targetPlayerId,
      allianceId,
      title: 'Alliance Member Kicked',
      message: `${this.players.get(targetPlayerId)?.name || targetPlayerId} has been kicked from ${alliance.name}`,
      data: { alliance },
    });

    return true;
  }

  private findAllianceContaining(playerId: string): Alliance | null {
    let foundAlliance: Alliance | null = null;
    this.alliances.forEach((alliance) => {
      if (alliance.memberIds.includes(playerId)) {
        foundAlliance = alliance;
      }
    });
    return foundAlliance;
  }

  public isAllianceMember(playerId: string, allianceId: string): boolean {
    const alliance = this.alliances.get(allianceId);
    return alliance ? alliance.memberIds.includes(playerId) : false;
  }

  public areAlliancesAllied(allianceId1: string, allianceId2: string): boolean {
    const alliance1 = this.alliances.get(allianceId1);
    const alliance2 = this.alliances.get(allianceId2);

    if (!alliance1 || !alliance2) return false;

    if (alliance1.id === alliance2.id) return true;

    const members1 = new Set(alliance1.memberIds);
    const members2 = new Set(alliance2.memberIds);

    for (const member of members1) {
      const memberRelation = this.getRelation(member, alliance2.leaderId);
      if (memberRelation && memberRelation.relationLevel === RelationLevel.Allied) {
        return true;
      }
    }

    return false;
  }

  public createTradeAgreement(
    proposerId: string,
    targetPlayerIds: string[],
    terms: TradeTerms
  ): DiplomaticProposal | null {
    const proposer = this.players.get(proposerId);
    if (!proposer) return null;

    const parties = [proposerId, ...targetPlayerIds];

    const existingAgreement = this.findExistingTradeAgreement(parties);
    if (existingAgreement) return null;

    const proposal: DiplomaticProposal = {
      id: this.generateId(),
      type: ProposalType.TradeDeal,
      proposerId,
      targetPlayerIds,
      title: 'Trade Agreement Proposal',
      description: `${proposer.name || proposerId} proposes a trade agreement`,
      terms,
      status: ProposalStatus.Proposed,
      votes: new Map(),
      createdAt: Date.now(),
      expiresAt: Date.now() + this.config.proposalExpirationTime,
      negotiationRounds: [],
    };

    this.diplomaticProposals.set(proposal.id, proposal);

    return proposal;
  }

  private findExistingTradeAgreement(parties: string[]): TradeAgreement | null {
    let found: TradeAgreement | null = null;
    this.tradeAgreements.forEach((agreement) => {
      if (agreement.active && parties.every((p) => agreement.parties.includes(p))) {
        found = agreement;
      }
    });
    return found;
  }

  public acceptTradeAgreement(proposalId: string, acceptorId: string): TradeAgreement | null {
    const proposal = this.diplomaticProposals.get(proposalId);
    if (!proposal || proposal.type !== ProposalType.TradeDeal) return null;

    if (!proposal.targetPlayerIds.includes(acceptorId)) return null;

    const terms = proposal.terms as TradeTerms;
    const agreement: TradeAgreement = {
      id: this.generateId(),
      parties: [proposal.proposerId, ...proposal.targetPlayerIds],
      proposedBy: proposal.proposerId,
      createdAt: Date.now(),
      expiresAt: terms.duration ? Date.now() + terms.duration : undefined,
      terms,
      active: true,
      tradeVolume: 0,
      benefits: {
        resourceBonus: {},
        technologyBonus: [],
        tradeRouteIncome: 10,
        diplomaticBonus: 5,
      },
      penalties: {
        violationPenalty: 50,
        terminationPenalty: 30,
        embargoMultiplier: 2.0,
      },
      violations: [],
    };

    this.tradeAgreements.set(agreement.id, agreement);
    proposal.status = ProposalStatus.Accepted;
    proposal.acceptedAt = Date.now();

    agreement.parties.forEach((party1) => {
      agreement.parties.forEach((party2) => {
        if (party1 !== party2) {
          this.modifyRelation(party1, party2, {
            type: 'trade_agreement',
            value: 20,
            reason: 'Trade agreement signed',
            source: agreement.id,
          });
        }
      });
    });

    this.emitEvent({
      type: DiplomaticEventType.TradeAgreementSigned,
      severity: EventSeverity.Info,
      sourcePlayerId: proposal.proposerId,
      targetPlayerId: acceptorId,
      title: 'Trade Agreement Signed',
      message: `A trade agreement has been signed between ${agreement.parties.join(', ')}`,
      data: { agreement },
    });

    return agreement;
  }

  public breakTradeAgreement(agreementId: string, breakerId: string): boolean {
    const agreement = this.tradeAgreements.get(agreementId);
    if (!agreement || !agreement.active) return false;

    if (!agreement.parties.includes(breakerId)) return false;

    agreement.active = false;

    const otherParties = agreement.parties.filter((p) => p !== breakerId);
    otherParties.forEach((party) => {
      this.modifyRelation(breakerId, party, {
        type: 'trade_agreement_broken',
        value: -this.config.betrayalPenaltyMultiplier * agreement.penalties.terminationPenalty,
        reason: 'Trade agreement broken',
        source: breakerId,
      });

      if (this.isPlayerAI(party)) {
        this.recordBetrayal(breakerId, party, DiplomaticActionType.BreakTradeAgreement);
      }
    });

    this.emitEvent({
      type: DiplomaticEventType.TradeAgreementBroken,
      severity: EventSeverity.Warning,
      sourcePlayerId: breakerId,
      title: 'Trade Agreement Broken',
      message: `${this.players.get(breakerId)?.name || breakerId} has broken the trade agreement`,
      data: { agreement },
    });

    return true;
  }

  public createNonAggressionPact(
    proposerId: string,
    targetPlayerId: string,
    terms: NAPTerms
  ): DiplomaticProposal | null {
    const proposer = this.players.get(proposerId);
    const target = this.players.get(targetPlayerId);

    if (!proposer || !target) return null;

    const existingNAP = this.findExistingNAP([proposerId, targetPlayerId]);
    if (existingNAP) return null;

    const proposal: DiplomaticProposal = {
      id: this.generateId(),
      type: ProposalType.NonAggressionPact,
      proposerId,
      targetPlayerIds: [targetPlayerId],
      title: 'Non-Aggression Pact Proposal',
      description: `${proposer.name || proposerId} proposes a non-aggression pact with ${target.name || targetPlayerId}`,
      terms,
      status: ProposalStatus.Proposed,
      votes: new Map(),
      createdAt: Date.now(),
      expiresAt: Date.now() + this.config.proposalExpirationTime,
      negotiationRounds: [],
    };

    this.diplomaticProposals.set(proposal.id, proposal);

    return proposal;
  }

  private findExistingNAP(parties: string[]): NonAggressionPact | null {
    let found: NonAggressionPact | null = null;
    this.nonAggressionPacts.forEach((nap) => {
      if (nap.active && parties.every((p) => nap.parties.includes(p))) {
        found = nap;
      }
    });
    return found;
  }

  public acceptNonAggressionPact(proposalId: string, acceptorId: string): NonAggressionPact | null {
    const proposal = this.diplomaticProposals.get(proposalId);
    if (!proposal || proposal.type !== ProposalType.NonAggressionPact) return null;

    if (!proposal.targetPlayerIds.includes(acceptorId)) return null;

    const terms = proposal.terms as NAPTerms;
    const nap: NonAggressionPact = {
      id: this.generateId(),
      parties: [proposal.proposerId, ...proposal.targetPlayerIds],
      proposedBy: proposal.proposerId,
      createdAt: Date.now(),
      expiresAt: terms.duration ? Date.now() + terms.duration : undefined,
      terms,
      active: true,
      violations: [],
      extensions: [],
    };

    this.nonAggressionPacts.set(nap.id, nap);
    proposal.status = ProposalStatus.Accepted;
    proposal.acceptedAt = Date.now();

    this.modifyRelation(proposal.proposerId, acceptorId, {
      type: 'nap_signed',
      value: 30,
      reason: 'Non-aggression pact signed',
      source: nap.id,
    });

    this.emitEvent({
      type: DiplomaticEventType.NonAggressionPactSigned,
      severity: EventSeverity.Info,
      sourcePlayerId: proposal.proposerId,
      targetPlayerId: acceptorId,
      title: 'Non-Aggression Pact Signed',
      message: `A non-aggression pact has been signed between ${nap.parties.join(' and ')}`,
      data: { nap },
    });

    return nap;
  }

  public breakNAP(napId: string, breakerId: string): boolean {
    const nap = this.nonAggressionPacts.get(napId);
    if (!nap || !nap.active) return false;

    if (!nap.parties.includes(breakerId)) return false;

    this.recordNAPViolation(breakerId, nap.parties.find((p) => p !== breakerId)!);

    nap.active = false;

    this.emitEvent({
      type: DiplomaticEventType.NonAggressionPactBroken,
      severity: EventSeverity.Critical,
      sourcePlayerId: breakerId,
      title: 'Non-Aggression Pact Broken',
      message: `${this.players.get(breakerId)?.name || breakerId} has broken the non-aggression pact!`,
      data: { nap },
    });

    return true;
  }

  private recordNAPViolation(breakerId: string, victimId: string): void {
    this.modifyRelation(breakerId, victimId, {
      type: 'nap_violation',
      value: -100 * this.config.betrayalPenaltyMultiplier,
      reason: 'Non-aggression pact violation',
      source: breakerId,
    });

    if (this.isPlayerAI(breakerId)) {
      this.recordBetrayal(breakerId, victimId, DiplomaticActionType.BreakNAP);
    }

    const nap = this.findExistingNAP([breakerId, victimId]);
    if (nap) {
      const violation: NAPViolation = {
        id: this.generateId(),
        violatingParty: breakerId,
        type: 'hostile_action',
        timestamp: Date.now(),
        evidence: [],
        penaltyApplied: true,
        forgiven: false,
      };
      nap.violations.push(violation);
    }
  }

  private recordBetrayal(betrayerId: string, victimId: string, type: DiplomaticActionType): void {
    let records = this.betrayalRecords.get(betrayerId) || [];
    records.push({
      targetId: victimId,
      betrayalType: type,
      timestamp: Date.now(),
      severity: this.calculateBetrayalSeverity(type),
      trustImpact: this.calculateTrustImpact(type),
      forgiving: false,
      forgiveTimestamp: 0,
    });
    this.betrayalRecords.set(betrayerId, records);

    this.emitEvent({
      type: DiplomaticEventType.BetrayalDetected,
      severity: EventSeverity.Critical,
      sourcePlayerId: betrayerId,
      targetPlayerId: victimId,
      title: 'Betrayal Detected',
      message: `${this.players.get(betrayerId)?.name || betrayerId} has betrayed ${this.players.get(victimId)?.name || victimId}!`,
      data: { type },
    });

    this.applyBetrayalPunishments(betrayerId, victimId, type);
  }

  private calculateBetrayalSeverity(type: DiplomaticActionType): number {
    switch (type) {
      case DiplomaticActionType.BetrayAlly:
        return 100;
      case DiplomaticActionType.BreakAlliance:
        return 80;
      case DiplomaticActionType.BreakNAP:
        return 60;
      case DiplomaticActionType.BreakTradeAgreement:
        return 40;
      case DiplomaticActionType.EstablishEmbassy:
        return 20;
      default:
        return 50;
    }
  }

  private calculateTrustImpact(type: DiplomaticActionType): number {
    switch (type) {
      case DiplomaticActionType.BetrayAlly:
        return -100;
      case DiplomaticActionType.BreakAlliance:
        return -80;
      case DiplomaticActionType.BreakNAP:
        return -60;
      case DiplomaticActionType.BreakTradeAgreement:
        return -40;
      default:
        return -50;
    }
  }

  private applyBetrayalPunishments(
    betrayerId: string,
    victimId: string,
    type: DiplomaticActionType
  ): void {
    const severity = this.calculateBetrayalSeverity(type);

    this.players.forEach((player) => {
      if (player.id !== betrayerId && player.id !== victimId) {
        const relation = this.getRelation(betrayerId, player.id);
        if (relation && relation.relationLevel !== RelationLevel.War) {
          const penalty = severity * 0.5;
          this.modifyRelation(betrayerId, player.id, {
            type: 'betrayal_witnessed',
            value: -penalty,
            reason: `Witnessed betrayal of ${victimId}`,
            source: victimId,
            stackable: true,
          });
        }
      }
    });

    const betrayerPersonality = this.aiPersonalities.get(betrayerId);
    if (betrayerPersonality) {
      betrayerPersonality.honorLevel = Math.max(0, betrayerPersonality.honorLevel - severity * 0.2);
      betrayerPersonality.aggressionHistory.push({
        targetId: victimId,
        betrayalType: type,
        timestamp: Date.now(),
        detectionDifficulty: Math.random() * 50,
        reputationPenalty: severity,
        forgiveable: false,
      });
    }

    this.emitEvent({
      type: DiplomaticEventType.BetrayalPunishment,
      severity: EventSeverity.Info,
      sourcePlayerId: betrayerId,
      targetPlayerId: victimId,
      title: 'Betrayal Consequences',
      message: `The betrayal by ${this.players.get(betrayerId)?.name || betrayerId} has damaged their reputation`,
      data: { severity },
    });
  }

  public establishEmbassy(ownerId: string, hostPlayerId: string): Embassy | null {
    const owner = this.players.get(ownerId);
    const host = this.players.get(hostPlayerId);

    if (!owner || !host) return null;

    if (ownerId === hostPlayerId) return null;

    const existingEmbassy = this.getEmbassy(ownerId, hostPlayerId);
    if (existingEmbassy) return null;

    const embassy: Embassy = {
      id: this.generateId(),
      ownerId,
      hostPlayerId,
      level: 1,
      establishedAt: Date.now(),
      intelligenceLevel: 1,
      influenceLevel: 1,
      tradeBonus: 10,
      espionageCapability: 5,
      upgradeCost: {
        gold: 1000,
        stone: 500,
      },
      missions: [],
      staff: [
        {
          id: this.generateId(),
          name: 'Ambassador',
          role: EmbassyRole.Ambassador,
          level: 1,
          efficiency: 0.8,
          loyalty: 1.0,
          specializations: ['diplomacy'],
        },
      ],
      security: {
        level: 1,
        detectionChance: 0.2,
        protectionBonus: 0.1,
        countermeasures: ['encryption'],
        compromised: false,
        lastAudit: Date.now(),
      },
      covertOperations: [],
    };

    this.embassies.set(embassy.id, embassy);

    const relation = this.getRelation(ownerId, hostPlayerId);
    if (relation) {
      relation.hasEmbassy = true;
      relation.embassyLevel = 1;
    }

    this.modifyRelation(ownerId, hostPlayerId, {
      type: 'embassy_established',
      value: 30,
      reason: 'Embassy established',
      source: ownerId,
    });

    this.emitEvent({
      type: DiplomaticEventType.EmbassyEstablished,
      severity: EventSeverity.Info,
      sourcePlayerId: ownerId,
      targetPlayerId: hostPlayerId,
      title: 'Embassy Established',
      message: `${owner.name || ownerId} has established an embassy in ${host.name || hostPlayerId}`,
      data: { embassy },
    });

    return embassy;
  }

  public getEmbassy(ownerId: string, hostPlayerId: string): Embassy | null {
    let found: Embassy | null = null;
    this.embassies.forEach((embassy) => {
      if (embassy.ownerId === ownerId && embassy.hostPlayerId === hostPlayerId) {
        found = embassy;
      }
    });
    return found;
  }

  public upgradeEmbassy(ownerId: string, hostPlayerId: string): boolean {
    const embassy = this.getEmbassy(ownerId, hostPlayerId);
    if (!embassy) return false;

    if (embassy.level >= this.config.embassyMaxLevel) return false;

    const upgradeCost = embassy.upgradeCost;
    const owner = this.players.get(ownerId);

    if (owner && owner.resources) {
      for (const [resource, amount] of Object.entries(upgradeCost)) {
        if ((owner.resources[resource] || 0) < amount) {
          return false;
        }
      }

      for (const [resource, amount] of Object.entries(upgradeCost)) {
        owner.resources[resource] -= amount;
      }
    }

    embassy.level++;
    embassy.intelligenceLevel = Math.min(5, embassy.intelligenceLevel + 1);
    embassy.influenceLevel = Math.min(5, embassy.influenceLevel + 1);
    embassy.tradeBonus += 5;
    embassy.espionageCapability += 5;

    embassy.upgradeCost = {
      gold: Math.floor(embassy.upgradeCost.gold * 1.5),
      stone: Math.floor(embassy.upgradeCost.stone * 1.5),
    };

    const relation = this.getRelation(ownerId, hostPlayerId);
    if (relation) {
      relation.embassyLevel = embassy.level;
    }

    return true;
  }

  public recallEmbassy(ownerId: string, hostPlayerId: string): boolean {
    const embassy = this.getEmbassy(ownerId, hostPlayerId);
    if (!embassy) return false;

    this.embassies.delete(embassy.id);

    const relation = this.getRelation(ownerId, hostPlayerId);
    if (relation) {
      relation.hasEmbassy = false;
      relation.embassyLevel = 0;
    }

    this.modifyRelation(ownerId, hostPlayerId, {
      type: 'embassy_recalled',
      value: -10,
      reason: 'Embassy recalled',
      source: ownerId,
    });

    this.emitEvent({
      type: DiplomaticEventType.EmbassyRecalled,
      severity: EventSeverity.Info,
      sourcePlayerId: ownerId,
      targetPlayerId: hostPlayerId,
      title: 'Embassy Recalled',
      message: `${this.players.get(ownerId)?.name || ownerId} has recalled their embassy from ${this.players.get(hostPlayerId)?.name || hostPlayerId}`,
    });

    return true;
  }

  public startEmbassyMission(embassyId: string, missionType: EmbassyMissionType): EmbassyMission | null {
    const embassy = this.embassies.get(embassyId);
    if (!embassy) return null;

    const missionExists = embassy.missions.some((m) => m.type === missionType && m.status === MissionStatus.InProgress);
    if (missionExists) return null;

    const mission: EmbassyMission = {
      id: this.generateId(),
      type: missionType,
      status: MissionStatus.InProgress,
      startTime: Date.now(),
      progress: 0,
      cost: this.calculateMissionCost(missionType, embassy.level),
      rewards: this.calculateMissionRewards(missionType),
      risk: this.calculateMissionRisk(missionType, embassy),
      discovered: false,
    };

    embassy.missions.push(mission);
    return mission;
  }

  private calculateMissionCost(type: EmbassyMissionType, embassyLevel: number): Record<string, number> {
    const baseCosts: Record<EmbassyMissionType, Record<string, number>> = {
      [EmbassyMissionType.IntelligenceGathering]: { gold: 500, influence: 100 },
      [EmbassyMissionType.InfluenceCampaign]: { gold: 300, influence: 200 },
      [EmbassyMissionType.TradePromotion]: { gold: 400 },
      [EmbassyMissionType.Sabotage]: { gold: 600, influence: 150 },
      [EmbassyMissionType.Assassination]: { gold: 1000, influence: 500 },
      [EmbassyMissionType.Propaganda]: { gold: 200, influence: 300 },
      [EmbassyMissionType.DiplomaticLiaison]: { gold: 100 },
      [EmbassyMissionType.TechnologyTheft]: { gold: 700, influence: 200 },
      [EmbassyMissionType.ResourceSurvey]: { gold: 300 },
      [EmbassyMissionType.DefectionInducement]: { gold: 800, influence: 400 },
    };

    const base = baseCosts[type] || { gold: 300 };
    const multiplier = 1 + (embassyLevel - 1) * 0.1;

    const result: Record<string, number> = {};
    for (const [resource, amount] of Object.entries(base)) {
      result[resource] = Math.floor(amount * multiplier);
    }
    return result;
  }

  private calculateMissionRewards(type: EmbassyMissionType): EmbassyMissionReward {
    const rewards: Partial<Record<EmbassyMissionType, EmbassyMissionReward>> = {
      [EmbassyMissionType.IntelligenceGathering]: { intelligence: 50 },
      [EmbassyMissionType.InfluenceCampaign]: { influence: 40 },
      [EmbassyMissionType.TradePromotion]: { tradeBonus: 20 },
      [EmbassyMissionType.Sabotage]: {},
      [EmbassyMissionType.Assassination]: {},
      [EmbassyMissionType.Propaganda]: { influence: 30 },
      [EmbassyMissionType.DiplomaticLiaison]: { diplomaticBonus: 15 },
      [EmbassyMissionType.TechnologyTheft]: { technology: ['advanced_unit'] },
      [EmbassyMissionType.ResourceSurvey]: { information: 'resource_locations' },
      [EmbassyMissionType.DefectionInducement]: {},
    };

    return rewards[type] || {};
  }

  private calculateMissionRisk(type: EmbassyMissionType, embassy: Embassy): MissionRisk {
    const baseRisks: Partial<Record<EmbassyMissionType, number>> = {
      [EmbassyMissionType.IntelligenceGathering]: 0.3,
      [EmbassyMissionType.InfluenceCampaign]: 0.2,
      [EmbassyMissionType.TradePromotion]: 0.1,
      [EmbassyMissionType.Sabotage]: 0.6,
      [EmbassyMissionType.Assassination]: 0.8,
      [EmbassyMissionType.Propaganda]: 0.25,
      [EmbassyMissionType.DiplomaticLiaison]: 0.05,
      [EmbassyMissionType.TechnologyTheft]: 0.5,
      [EmbassyMissionType.ResourceSurvey]: 0.15,
      [EmbassyMissionType.DefectionInducement]: 0.55,
    };

    const baseRisk = baseRisks[type] || 0.3;

    return {
      baseRisk,
      hostRelationsModifier: embassy.level > 2 ? 0.1 : 0.2,
      securityLevelModifier: embassy.security.level > 2 ? -0.1 : 0.1,
      missionTypeModifier: 0,
    };
  }

  public proposeVote(allianceId: string, proposerId: string, vote: Partial<DiplomaticVotes>): DiplomaticVotes | null {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return null;

    if (!alliance.memberIds.includes(proposerId)) return null;

    const fullVote: DiplomaticVotes = {
      id: this.generateId(),
      allianceId,
      proposerId,
      type: vote.type || VoteType.AllianceVote,
      title: vote.title || 'Vote Proposed',
      description: vote.description || '',
      options: vote.options || [{ optionId: 'approve', text: 'Approve', value: true }],
      deadline: Date.now() + this.config.voteDuration,
      results: new Map(),
      totalVotesFor: 0,
      totalVotesAgainst: 0,
      totalVoters: 0,
      status: VoteResult.Pending,
      executed: false,
    };

    alliance.diplomaticVotes.push(fullVote);
    this.diplomaticVotes.set(fullVote.id, fullVote);

    this.emitEvent({
      type: DiplomaticEventType.VoteProposed,
      severity: EventSeverity.Info,
      sourcePlayerId: proposerId,
      allianceId,
      title: 'Alliance Vote Proposed',
      message: `A vote has been proposed in ${alliance.name}: ${fullVote.title}`,
      data: { vote: fullVote },
    });

    return fullVote;
  }

  public castVote(voteId: string, playerId: string, approve: boolean): boolean {
    const vote = this.diplomaticVotes.get(voteId);
    if (!vote) return false;

    if (vote.status !== VoteResult.Pending) return false;

    const alliance = this.alliances.get(vote.allianceId);
    if (!alliance || !alliance.memberIds.includes(playerId)) return false;

    if (vote.deadline < Date.now()) {
      this.closeVote(voteId);
      return false;
    }

    vote.votes.set(playerId, approve);
    vote.totalVoters++;

    if (approve) {
      vote.totalVotesFor++;
    } else {
      vote.totalVotesAgainst++;
    }

    if (vote.totalVoters >= alliance.memberIds.length) {
      this.closeVote(voteId);
    }

    return true;
  }

  private closeVote(voteId: string): void {
    const vote = this.diplomaticVotes.get(voteId);
    if (!vote) return;

    const alliance = this.alliances.get(vote.allianceId);
    if (!alliance) return;

    const requiredVotes = Math.ceil(alliance.memberIds.length / 2);

    if (vote.totalVotesFor >= requiredVotes) {
      vote.status = VoteResult.Approved;
      this.executeVote(vote);
    } else if (vote.totalVotesAgainst >= requiredVotes) {
      vote.status = VoteResult.Rejected;
    } else if (vote.deadline < Date.now()) {
      vote.status = VoteResult.Expired;
    } else {
      vote.status = VoteResult.Tied;
    }

    this.emitEvent({
      type: DiplomaticEventType.VoteResult,
      severity: EventSeverity.Info,
      allianceId: vote.allianceId,
      title: 'Vote Results',
      message: `Vote "${vote.title}" has been ${vote.status}`,
      data: { vote },
    });
  }

  private executeVote(vote: DiplomaticVotes): void {
    if (vote.executed) return;

    switch (vote.type) {
      case VoteType.DeclareWar:
        this.executeWarVote(vote);
        break;
      case VoteType.ProposePeace:
        this.executePeaceVote(vote);
        break;
      case VoteType.JointMilitaryAction:
        this.executeJointMilitaryVote(vote);
        break;
      case VoteType.ShareResources:
        this.executeResourceSharingVote(vote);
        break;
    }

    vote.executed = true;
  }

  private executeWarVote(vote: DiplomaticVotes): void {
    const targetPlayerId = vote.options[0]?.value;
    if (!targetPlayerId) return;

    const alliance = this.alliances.get(vote.allianceId);
    if (!alliance) return;

    alliance.memberIds.forEach((memberId) => {
      this.declareWar(memberId, targetPlayerId);
    });
  }

  private executePeaceVote(vote: DiplomaticVotes): void {
    const targetPlayerId = vote.options[0]?.value;
    if (!targetPlayerId) return;

    this.proposePeace(vote.proposerId, targetPlayerId);
  }

  private executeJointMilitaryVote(vote: DiplomaticVotes): void {
    const alliance = this.alliances.get(vote.allianceId);
    if (!alliance) return;

    alliance.jointMilitary = true;
    alliance.militaryPactLevel = MilitaryPactLevel.Offensive;
  }

  private executeResourceSharingVote(vote: DiplomaticVotes): void {
    const alliance = this.alliances.get(vote.allianceId);
    if (!alliance) return;

    alliance.resourceSharingRules = {
      enabled: true,
      percentageToShare: 10,
      resourceTypes: ['gold', 'food', 'wood'],
      automaticDistribution: true,
    };
  }

  public proposeAllyCommand(allianceId: string, commanderId: string, command: Partial<AllyCommand>): AllyCommand | null {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return null;

    if (!alliance.memberIds.includes(commanderId)) return null;

    const fullCommand: AllyCommand = {
      id: this.generateId(),
      allianceId,
      commanderId,
      type: command.type || AllyCommandType.Coordinate,
      targetPlayerId: command.targetPlayerId,
      targetLocation: command.targetLocation,
      priority: command.priority || 1,
      status: CommandStatus.Proposed,
      createdAt: Date.now(),
      acceptedBy: [commanderId],
      rejectedBy: [],
      deadline: command.deadline,
    };

    alliance.alliedCommands.push(fullCommand);

    alliance.memberIds.forEach((memberId) => {
      if (memberId !== commanderId) {
        this.emitEvent({
          type: DiplomaticEventType.VoteProposed,
          severity: EventSeverity.Info,
          sourcePlayerId: commanderId,
          targetPlayerId: memberId,
          allianceId,
          title: 'Ally Command Proposed',
          message: `${this.players.get(commanderId)?.name || commanderId} has proposed a ${command.type} command`,
          data: { command: fullCommand },
        });
      }
    });

    return fullCommand;
  }

  public respondToAllyCommand(commandId: string, playerId: string, accept: boolean): boolean {
    let foundCommand: AllyCommand | null = null;
    let foundAlliance: Alliance | null = null;

    this.alliances.forEach((alliance) => {
      const command = alliance.alliedCommands.find((c) => c.id === commandId);
      if (command) {
        foundCommand = command;
        foundAlliance = alliance;
      }
    });

    if (!foundCommand || !foundAlliance) return false;

    if (!foundAlliance.memberIds.includes(playerId)) return false;

    if (accept) {
      foundCommand.acceptedBy.push(playerId);

      if (foundCommand.acceptedBy.length >= foundAlliance.requiredVotes) {
        foundCommand.status = CommandStatus.Accepted;
      }
    } else {
      foundCommand.rejectedBy.push(playerId);

      if (foundCommand.rejectedBy.length >= foundAlliance.requiredVotes) {
        foundCommand.status = CommandStatus.Rejected;
      }
    }

    return true;
  }

  public requestSupport(
    requesterId: string,
    supportType: SupportType,
    amount: number,
    justification: string
  ): SupportRequest | null {
    const alliance = this.findAllianceContaining(requesterId);
    if (!alliance) return null;

    const request: SupportRequest = {
      id: this.generateId(),
      requestingPlayerId: requesterId,
      supportType,
      amount,
      urgency: 1,
      justification,
      status: RequestStatus.Pending,
      responses: new Map(),
      deadline: Date.now() + 300000,
    };

    alliance.supportNetwork.supportRequests.push(request);

    alliance.memberIds.forEach((memberId) => {
      if (memberId !== requesterId) {
        this.emitEvent({
          type: DiplomaticEventType.VoteProposed,
          severity: EventSeverity.Info,
          sourcePlayerId: requesterId,
          targetPlayerId: memberId,
          title: 'Support Request',
          message: `${this.players.get(requesterId)?.name || requesterId} is requesting ${supportType} support`,
          data: { request },
        });
      }
    });

    return request;
  }

  public respondToSupportRequest(
    requestId: string,
    responderId: string,
    approved: boolean,
    amount?: number,
    reason?: string
  ): boolean {
    let foundRequest: SupportRequest | null = null;

    this.alliances.forEach((alliance) => {
      const request = alliance.supportNetwork.supportRequests.find((r) => r.id === requestId);
      if (request) {
        foundRequest = request;
      }
    });

    if (!foundRequest) return false;

    if (foundRequest.status !== RequestStatus.Pending) return false;

    const response: SupportResponse = {
      playerId: responderId,
      approved,
      amount,
      reason,
      timestamp: Date.now(),
    };

    foundRequest.responses.set(responderId, response);

    let totalApproved = 0;
    foundRequest.responses.forEach((resp) => {
      if (resp.approved && resp.amount) {
        totalApproved += resp.amount;
      }
    });

    if (totalApproved >= foundRequest.amount) {
      foundRequest.status = RequestStatus.Fulfilled;
      this.fulfillSupportRequest(foundRequest);
    } else if (foundRequest.deadline < Date.now()) {
      foundRequest.status = RequestStatus.PartiallyApproved;
    }

    return true;
  }

  private fulfillSupportRequest(request: SupportRequest): void {
    const alliance = this.findAllianceContaining(request.requestingPlayerId);
    if (!alliance) return;

    let totalProvided = 0;

    request.responses.forEach((response) => {
      if (response.approved && response.amount) {
        const support: ActiveSupport = {
          id: this.generateId(),
          supportedPlayerId: request.requestingPlayerId,
          supportingPlayerId: response.playerId,
          type: request.supportType,
          amount: response.amount,
          duration: 300000,
          startTime: Date.now(),
          active: true,
        };

        alliance.supportNetwork.activeSupports.push(support);
        totalProvided += response.amount;
      }
    });

    request.status = RequestStatus.Approved;
  }

  public addIntelligenceSharing(
    fromPlayerId: string,
    toPlayerId: string,
    type: IntelligenceType,
    level: IntelligenceSharingLevel
  ): IntelligenceSharing | null {
    const fromAlliance = this.findAllianceContaining(fromPlayerId);
    const toAlliance = this.findAllianceContaining(toPlayerId);

    if (fromAlliance) {
      const sharing: IntelligenceSharing = {
        id: this.generateId(),
        fromPlayerId,
        toPlayerId,
        intelligenceType: type,
        priority: IntelligencePriority.Normal,
        sharingLevel: level,
        automatic: true,
        lastShared: Date.now(),
      };

      fromAlliance.supportNetwork.intelligenceSharing.push(sharing);
      return sharing;
    }

    return null;
  }

  public getSharedIntelligence(playerId: string): any {
    const alliance = this.findAllianceContaining(playerId);
    if (!alliance) return {};

    const sharedIntelligence: Record<string, any> = {};

    alliance.supportNetwork.intelligenceSharing.forEach((sharing) => {
      if (sharing.toPlayerId === playerId) {
        const sourceIntelligence = this.gatherPlayerIntelligence(sharing.fromPlayerId, sharing.intelligenceType);
        Object.assign(sharedIntelligence, sourceIntelligence);
      }
    });

    return sharedIntelligence;
  }

  private gatherPlayerIntelligence(playerId: string, type: IntelligenceType): any {
    const intelligence: any = {};

    if (type === IntelligenceType.Military || type === IntelligenceType.All) {
      intelligence.military = {
        armySize: 0,
        unitComposition: {},
        fortifications: [],
        deploymentLocations: [],
      };
    }

    if (type === IntelligenceType.Economic || type === IntelligenceType.All) {
      intelligence.economic = {
        resources: {},
        income: 0,
        tradePartners: [],
      };
    }

    if (type === IntelligenceType.Diplomatic || type === IntelligenceType.All) {
      intelligence.diplomatic = {
        relations: {},
        alliances: [],
        agreements: [],
      };

      this.relations.forEach((relation, key) => {
        if (relation.playerId === playerId) {
          intelligence.diplomatic.relations[relation.targetPlayerId] = {
            level: relation.relationLevel,
            score: relation.relationScore,
          };
        }
      });

      this.alliances.forEach((alliance) => {
        if (alliance.memberIds.includes(playerId)) {
          intelligence.diplomatic.alliances.push({
            id: alliance.id,
            name: alliance.name,
            memberCount: alliance.memberIds.length,
          });
        }
      });
    }

    return intelligence;
  }

  public triggerAllyUnderAttack(playerId: string, attackerId: string): void {
    const alliance = this.findAllianceContaining(playerId);
    if (!alliance) return;

    alliance.memberIds.forEach((memberId) => {
      if (memberId !== playerId) {
        this.modifyRelation(memberId, attackerId, {
          type: 'ally_under_attack',
          value: -20,
          reason: `Ally ${playerId} is under attack`,
          source: playerId,
        });

        this.emitEvent({
          type: DiplomaticEventType.AllyUnderAttack,
          severity: EventSeverity.Critical,
          sourcePlayerId: playerId,
          targetPlayerId: memberId,
          title: 'Ally Under Attack!',
          message: `${this.players.get(playerId)?.name || playerId} is being attacked by ${this.players.get(attackerId)?.name || attackerId}!`,
          data: { attackerId, defenderId: playerId },
        });
      }
    });

    if (alliance.jointMilitary) {
      this.initiateJointDefense(alliance.id, playerId, attackerId);
    }
  }

  private initiateJointDefense(allianceId: string, defenderId: string, attackerId: string): void {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return;

    alliance.memberIds.forEach((memberId) => {
      if (memberId !== defenderId) {
        const relation = this.getRelation(memberId, defenderId);
        if (relation && relation.relationLevel === RelationLevel.Allied) {
          this.proposeAllyCommand(allianceId, defenderId, {
            type: AllyCommandType.Defend,
            targetPlayerId: defenderId,
            priority: 5,
          });
        }
      }
    });
  }

  public checkDiplomaticVictory(playerId: string): DiplomaticVictory | null {
    const alliance = this.findAllianceContaining(playerId);
    if (!alliance) return null;

    const conditions = this.evaluateVictoryConditions(alliance, playerId);
    const completedConditions = conditions.filter((c) => c.completed);

    if (completedConditions.length < 3) return null;

    const victory: DiplomaticVictory = {
      type: VictoryType.Diplomatic,
      achievedBy: playerId,
      allies: alliance.memberIds,
      conditions,
      completedAt: Date.now(),
      proof: {
        allianceStrength: this.calculateAllianceStrength(alliance.id),
        diplomaticAgreements: this.countDiplomaticAgreements(playerId),
        alliesCount: alliance.memberIds.length,
        treatiesSigned: this.countTreaties(playerId),
        betrayalCount: this.getBetrayalCount(playerId),
        betrayedCount: this.getBetrayedCount(playerId),
        warVictories: 0,
        peaceTreaties: this.countPeaceTreaties(playerId),
      },
    };

    this.victoryTracking.set(playerId, victory);

    this.emitEvent({
      type: DiplomaticEventType.VictoryAchieved,
      severity: EventSeverity.Victory,
      sourcePlayerId: playerId,
      allianceId: alliance.id,
      title: 'Diplomatic Victory!',
      message: `${this.players.get(playerId)?.name || playerId} has achieved a diplomatic victory!`,
      data: { victory },
    });

    return victory;
  }

  private evaluateVictoryConditions(alliance: Alliance, playerId: string): VictoryCondition[] {
    const conditions: VictoryCondition[] = [];

    conditions.push({
      type: 'alliance_size',
      description: 'Build a large alliance',
      target: 5,
      current: alliance.memberIds.length,
      completed: alliance.memberIds.length >= 5,
      progress: Math.min(1, alliance.memberIds.length / 5),
    });

    conditions.push({
      type: 'alliance_strength',
      description: 'Achieve high alliance strength',
      target: 1000,
      current: this.calculateAllianceStrength(alliance.id),
      completed: this.calculateAllianceStrength(alliance.id) >= 1000,
      progress: Math.min(1, this.calculateAllianceStrength(alliance.id) / 1000),
    });

    conditions.push({
      type: 'trade_network',
      description: 'Establish extensive trade network',
      target: 10,
      current: this.countActiveTradeAgreements(alliance.id),
      completed: this.countActiveTradeAgreements(alliance.id) >= 10,
      progress: Math.min(1, this.countActiveTradeAgreements(alliance.id) / 10),
    });

    conditions.push({
      type: 'diplomatic_agreements',
      description: 'Sign multiple diplomatic agreements',
      target: 15,
      current: this.countDiplomaticAgreements(playerId),
      completed: this.countDiplomaticAgreements(playerId) >= 15,
      progress: Math.min(1, this.countDiplomaticAgreements(playerId) / 15),
    });

    conditions.push({
      type: 'peaceful_diplomacy',
      description: 'Maintain diplomatic relations without betrayals',
      target: 0,
      current: this.getBetrayalCount(playerId),
      completed: this.getBetrayalCount(playerId) === 0,
      progress: this.getBetrayalCount(playerId) === 0 ? 1 : 0,
    });

    return conditions;
  }

  private calculateAllianceStrength(allianceId: string): number {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return 0;

    let strength = 0;

    alliance.memberIds.forEach((memberId) => {
      const relation = this.getRelation(memberId, alliance.leaderId);
      if (relation) {
        strength += relation.relationScore;
      }
    });

    strength += alliance.alliedVictoryPoints;
    strength += alliance.memberIds.length * 50;

    return strength;
  }

  private countDiplomaticAgreements(playerId: string): number {
    let count = 0;

    this.tradeAgreements.forEach((agreement) => {
      if (agreement.active && agreement.parties.includes(playerId)) {
        count++;
      }
    });

    this.nonAggressionPacts.forEach((nap) => {
      if (nap.active && nap.parties.includes(playerId)) {
        count++;
      }
    });

    return count;
  }

  private countTreaties(playerId: string): number {
    return this.countDiplomaticAgreements(playerId);
  }

  private countPeaceTreaties(playerId: string): number {
    let count = 0;

    this.diplomaticProposals.forEach((proposal) => {
      if (
        proposal.type === ProposalType.PeaceTreaty &&
        proposal.status === ProposalStatus.Accepted &&
        proposal.targetPlayerIds.includes(playerId)
      ) {
        count++;
      }
    });

    return count;
  }

  private countActiveTradeAgreements(allianceId: string): number {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return 0;

    let count = 0;
    const allianceMembers = new Set(alliance.memberIds);

    this.tradeAgreements.forEach((agreement) => {
      if (agreement.active) {
        const partySet = new Set(agreement.parties);
        const intersection = [...allianceMembers].filter((x) => partySet.has(x));
        if (intersection.length >= 2) {
          count++;
        }
      }
    });

    return count;
  }

  private getBetrayalCount(playerId: string): number {
    const records = this.betrayalRecords.get(playerId) || [];
    return records.length;
  }

  private getBetrayedCount(playerId: string): number {
    let count = 0;
    this.betrayalRecords.forEach((records) => {
      count += records.filter((r) => r.targetId === playerId).length;
    });
    return count;
  }

  private updateVictoryConditions(playerId: string): void {
    const alliance = this.findAllianceContaining(playerId);
    if (!alliance) return;

    alliance.alliedVictoryPoints += 10;
  }

  public updateAIBehavior(playerId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    this.updateAIAggression(playerId);
    this.updateAIThreats(playerId);
    this.evaluateAIAlliances(playerId);
    this.generateAIProposals(playerId);
    this.respondToAISituations(playerId);
  }

  private updateAIAggression(playerId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    let aggressionModifier = 0;

    const warCount = this.countWars(playerId);
    aggressionModifier -= warCount * 5;

    const resources = this.players.get(playerId)?.resources || {};
    const resourceShortage = Object.values(resources).some((v) => v < 100);
    if (resourceShortage) {
      aggressionModifier += 10;
    }

    const recentBetrayals = personality.aggressionHistory.filter(
      (h) => Date.now() - h.timestamp < 60000
    ).length;
    if (recentBetrayals > 0) {
      aggressionModifier -= recentBetrayals * 5;
    }

    personality.currentAggression = Math.max(
      0,
      Math.min(100, personality.baseAggression + aggressionModifier)
    );
  }

  private countWars(playerId: string): number {
    let count = 0;
    this.activeWars.forEach((key) => {
      if (key.includes(playerId)) {
        count++;
      }
    });
    return count;
  }

  private updateAIThreats(playerId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    this.players.forEach((player) => {
      if (player.id === playerId) return;

      const relation = this.getRelation(playerId, player.id);
      if (!relation) return;

      const threatLevel = this.assessThreatLevel(playerId, player.id);

      personality.threatAssessment.currentThreats.set(player.id, threatLevel);

      if (threatLevel >= ThreatLevel.High) {
        this.respondToAIThreat(playerId, player.id, threatLevel);
      }
    });
  }

  private assessThreatLevel(playerId: string, targetPlayerId: string): ThreatLevel {
    const relation = this.getRelation(playerId, targetPlayerId);
    if (!relation) return ThreatLevel.None;

    if (relation.relationLevel === RelationLevel.War) {
      return ThreatLevel.Critical;
    }

    if (relation.militaryThreat > 80) {
      return ThreatLevel.High;
    }

    if (relation.militaryThreat > 50) {
      return ThreatLevel.Medium;
    }

    if (relation.militaryThreat > 20) {
      return ThreatLevel.Low;
    }

    return ThreatLevel.None;
  }

  private respondToAIThreat(playerId: string, threatPlayerId: string, level: ThreatLevel): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    switch (personality.threatAssessment.responseStrategy) {
      case ThreatResponseStrategy.SeekAlliance:
        this.seekAllianceAgainstThreat(playerId, threatPlayerId);
        break;
      case ThreatResponseStrategy.Defend:
        this.requestDefensiveSupport(playerId, threatPlayerId);
        break;
      case ThreatResponseStrategy.Counterattack:
        if (level >= ThreatLevel.High) {
          this.initiateCounterattack(playerId, threatPlayerId);
        }
        break;
    }
  }

  private seekAllianceAgainstThreat(playerId: string, threatPlayerId: string): void {
    this.players.forEach((player) => {
      if (player.id === playerId || player.id === threatPlayerId) return;

      const relation = this.getRelation(playerId, player.id);
      if (relation && relation.relationLevel >= RelationLevel.Friendly) {
        const proposal = this.createAlliance(playerId, player.id, 'Defense Pact');
        if (proposal) {
          this.diplomaticProposals.set(proposal.id, proposal);

          this.emitEvent({
            type: DiplomaticEventType.AIProposal,
            severity: EventSeverity.Info,
            sourcePlayerId: playerId,
            targetPlayerId: player.id,
            title: 'Defense Pact Proposal',
            message: `${this.players.get(playerId)?.name || playerId} proposes a defense pact against common threats`,
          });
        }
      }
    });
  }

  private createAlliance(playerId: string, targetPlayerId: string, type: string): DiplomaticProposal | null {
    return this.inviteToAlliance(this.findAllianceContaining(playerId)?.id || '', playerId, targetPlayerId);
  }

  private requestDefensiveSupport(playerId: string, threatPlayerId: string): void {
    this.requestSupport(playerId, SupportType.Military, 100, 'Defensive support requested');
  }

  private initiateCounterattack(playerId: string, threatPlayerId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    if (personality.currentAggression >= 70) {
      this.declareWar(playerId, threatPlayerId);
    }
  }

  private evaluateAIAlliances(playerId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    const alliance = this.findAllianceContaining(playerId);
    if (!alliance) return;

    alliance.memberIds.forEach((memberId) => {
      if (memberId === playerId) return;

      const relation = this.getRelation(playerId, memberId);
      if (!relation) return;

      const trust = personality.currentTrust.get(memberId) || AITrustLevel.Low;

      if (trust <= AITrustLevel.Low && relation.relationScore < -100) {
        if (this.shouldAILeaveAlliance(playerId, alliance.id)) {
          this.leaveAlliance(playerId);
        }
      }

      if (trust >= AITrustLevel.High && relation.relationScore > 200) {
        this.strengthenAllianceBond(playerId, memberId);
      }
    });
  }

  private shouldAILeaveAlliance(playerId: string, allianceId: string): boolean {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return false;

    return personality.loyaltyThreshold < 30;
  }

  private strengthenAllianceBond(playerId: string, allyId: string): void {
    this.modifyRelation(playerId, allyId, {
      type: 'alliance_strengthened',
      value: 20,
      reason: 'Strengthening alliance bond',
      source: playerId,
    });
  }

  private generateAIProposals(playerId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    const alliance = this.findAllianceContaining(playerId);
    if (!alliance) return;

    const randomChance = Math.random();

    if (randomChance < 0.1 && personality.alliancePreference > 50) {
      this.proposeResourceSharing(alliance.id, playerId);
    }

    if (randomChance < 0.05) {
      this.proposeJointMilitaryAction(alliance.id, playerId);
    }
  }

  private proposeResourceSharing(allianceId: string, playerId: string): void {
    this.proposeVote(allianceId, playerId, {
      type: VoteType.ShareResources,
      title: 'Resource Sharing Proposal',
      description: 'Propose sharing resources among alliance members',
    });
  }

  private proposeJointMilitaryAction(allianceId: string, playerId: string): void {
    this.proposeVote(allianceId, playerId, {
      type: VoteType.JointMilitaryAction,
      title: 'Joint Military Action',
      description: 'Establish joint military command',
    });
  }

  private respondToAISituations(playerId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    this.processAIProposalResponses(playerId);
    this.processAIEmbassyOperations(playerId);
    this.processAIWarResponses(playerId);
  }

  private processAIProposalResponses(playerId: string): void {
    this.diplomaticProposals.forEach((proposal) => {
      if (proposal.status !== ProposalStatus.Proposed) return;
      if (!proposal.targetPlayerIds.includes(playerId)) return;
      if (proposal.proposerId === playerId) return;

      const accept = this.evaluateAIProposalAcceptance(playerId, proposal);
      if (accept) {
        this.respondToAIProposal(playerId, proposal, true);
      } else {
        this.respondToAIProposal(playerId, proposal, false);
      }
    });
  }

  private evaluateAIProposalAcceptance(playerId: string, proposal: DiplomaticProposal): boolean {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return false;

    const proposerRelation = this.getRelation(proposal.proposerId, playerId);
    if (!proposerRelation) return false;

    let acceptanceChance = 0.5;

    const trust = personality.currentTrust.get(proposal.proposerId) || AITrustLevel.Low;
    acceptanceChance += trust * 0.1;

    acceptanceChance += proposerRelation.relationScore / 2000;

    if (proposal.type === ProposalType.AllianceMembership) {
      acceptanceChance *= personality.alliancePreference / 100;
    }

    if (proposal.type === ProposalType.JointWarDeclaration) {
      acceptanceChance *= personality.currentAggression / 100;
    }

    return Math.random() < acceptanceChance;
  }

  private respondToAIProposal(playerId: string, proposal: DiplomaticProposal, accept: boolean): void {
    switch (proposal.type) {
      case ProposalType.AllianceMembership:
        this.respondToAllianceInvite(proposal.id, playerId, accept);
        break;
      case ProposalType.TradeDeal:
        if (accept) {
          this.acceptTradeAgreement(proposal.id, playerId);
        } else {
          proposal.status = ProposalStatus.Rejected;
        }
        break;
      case ProposalType.NonAggressionPact:
        if (accept) {
          this.acceptNonAggressionPact(proposal.id, playerId);
        } else {
          proposal.status = ProposalStatus.Rejected;
        }
        break;
    }
  }

  private processAIEmbassyOperations(playerId: string): void {
    this.embassies.forEach((embassy) => {
      if (embassy.ownerId !== playerId) return;

      if (embassy.missions.length < 3) {
        const missionType = this.selectEmbassyMission(embassy);
        if (missionType) {
          this.startEmbassyMission(embassy.id, missionType);
        }
      }
    });
  }

  private selectEmbassyMission(embassy: Embassy): EmbassyMissionType | null {
    const personality = this.aiPersonalities.get(embassy.ownerId);
    if (!personality) return null;

    const availableMissions = Object.values(EmbassyMissionType);

    const weights: Record<EmbassyMissionType, number> = {
      [EmbassyMissionType.IntelligenceGathering]: personality.threatAssessment.currentThreats.size > 0 ? 0.4 : 0.2,
      [EmbassyMissionType.InfluenceCampaign]: 0.2,
      [EmbassyMissionType.TradePromotion]: 0.3,
      [EmbassyMissionType.Sabotage]: personality.currentAggression > 70 ? 0.2 : 0.05,
      [EmbassyMissionType.Assassination]: personality.currentAggression > 90 ? 0.1 : 0.0,
      [EmbassyMissionType.Propaganda]: 0.15,
      [EmbassyMissionType.DiplomaticLiaison]: 0.25,
      [EmbassyMissionType.TechnologyTheft]: 0.15,
      [EmbassyMissionType.ResourceSurvey]: 0.2,
      [EmbassyMissionType.DefectionInducement]: personality.currentAggression > 60 ? 0.1 : 0.05,
    };

    const totalWeight = Object.values(weights).reduce((sum, w) => sum + w, 0);
    let random = Math.random() * totalWeight;

    for (const missionType of availableMissions) {
      random -= weights[missionType] || 0;
      if (random <= 0) {
        return missionType;
      }
    }

    return EmbassyMissionType.IntelligenceGathering;
  }

  private processAIWarResponses(playerId: string): void {
    this.relations.forEach((relation, key) => {
      if (relation.playerId !== playerId) return;
      if (relation.relationLevel !== RelationLevel.War) return;

      const personality = this.aiPersonalities.get(playerId);
      if (!personality) return;

      if (personality.currentAggression > 50 && relation.relationScore < -500) {
        this.evaluateWarTermination(playerId, relation.targetPlayerId);
      }
    });
  }

  private evaluateWarTermination(playerId: string, enemyId: string): void {
    const personality = this.aiPersonalities.get(playerId);
    if (!personality) return;

    const warDuration = Date.now() - (this.getWarStartTime(playerId, enemyId) || Date.now());
    const longWar = warDuration > 300000;

    if (longWar && personality.honorLevel > 50) {
      if (Math.random() < 0.1) {
        this.proposePeace(playerId, enemyId);
      }
    }

    if (longWar) {
      const proposal = this.proposePeace(playerId, enemyId);
      if (proposal) {
        this.emitEvent({
          type: DiplomaticEventType.AIProposal,
          severity: EventSeverity.Info,
          sourcePlayerId: playerId,
          targetPlayerId: enemyId,
          title: 'Peace Proposal',
          message: `${this.players.get(playerId)?.name || playerId} offers peace`,
        });
      }
    }
  }

  private getWarStartTime(playerId: string, enemyId: string): number | null {
    return Date.now();
  }

  public update(): void {
    this.tickCounter++;

    if (this.tickCounter % 100 === 0) {
      this.processDiplomaticEvents();
      this.updateActiveAgreements();
      this.processAllianceVotes();
      this.processEmbassyMissions();
      this.checkVictoryConditions();
    }

    if (this.tickCounter % 50 === 0) {
      this.updateAIPersonalities();
      this.decayRelations();
      this.updateGlobalRelations();
    }

    if (this.tickCounter % 200 === 0) {
      this.cleanupExpiredData();
    }
  }

  private processDiplomaticEvents(): void {
    this.diplomaticEvents = this.diplomaticEvents.filter((event) => {
      if (event.dismissed) return false;

      if (event.severity === EventSeverity.Critical) {
        return true;
      }

      const age = Date.now() - event.timestamp;
      if (age > 86400000) {
        return false;
      }

      return true;
    });
  }

  private updateActiveAgreements(): void {
    const now = Date.now();

    this.tradeAgreements.forEach((agreement) => {
      if (agreement.active && agreement.expiresAt && agreement.expiresAt < now) {
        agreement.active = false;

        this.emitEvent({
          type: DiplomaticEventType.TradeAgreementBroken,
          severity: EventSeverity.Info,
          title: 'Trade Agreement Expired',
          message: `Trade agreement has expired`,
          data: { agreement },
        });
      }
    });

    this.nonAggressionPacts.forEach((nap) => {
      if (nap.active && nap.expiresAt && nap.expiresAt < now) {
        nap.active = false;

        this.emitEvent({
          type: DiplomaticEventType.NonAggressionPactBroken,
          severity: EventSeverity.Info,
          title: 'Non-Aggression Pact Expired',
          message: `Non-aggression pact has expired`,
          data: { nap },
        });
      }
    });
  }

  private processAllianceVotes(): void {
    this.diplomaticVotes.forEach((vote) => {
      if (vote.status === VoteResult.Pending && vote.deadline < Date.now()) {
        this.closeVote(vote.id);
      }
    });
  }

  private processEmbassyMissions(): void {
    this.embassies.forEach((embassy) => {
      embassy.missions = embassy.missions.filter((mission) => {
        if (mission.status === MissionStatus.InProgress) {
          mission.progress += 5;

          if (mission.progress >= 100) {
            return this.completeEmbassyMission(embassy, mission);
          }
        }

        return mission.status !== MissionStatus.Completed;
      });
    });
  }

  private completeEmbassyMission(embassy: Embassy, mission: EmbassyMission): boolean {
    const successChance = 1 - mission.risk.baseRisk;

    if (Math.random() < successChance) {
      mission.status = MissionStatus.Completed;
      mission.completionTime = Date.now();

      embassy.ownerId;

      this.emitEvent({
        type: DiplomaticEventType.EmbassyEstablished,
        severity: EventSeverity.Info,
        sourcePlayerId: embassy.ownerId,
        targetPlayerId: embassy.hostPlayerId,
        title: 'Embassy Mission Completed',
        message: `A ${mission.type} mission has been completed`,
        data: { mission },
      });

      return true;
    } else {
      mission.status = MissionStatus.Failed;

      if (Math.random() < mission.risk.baseRisk) {
        mission.discovered = true;
        embassy.security.compromised = true;

        this.emitEvent({
          type: DiplomaticEventType.EmbassyRecalled,
          severity: EventSeverity.Warning,
          sourcePlayerId: embassy.ownerId,
          targetPlayerId: embassy.hostPlayerId,
          title: 'Embassy Mission Discovered',
          message: `A ${mission.type} mission has been discovered!`,
          data: { mission },
        });
      }

      return false;
    }
  }

  private checkVictoryConditions(): void {
    this.players.forEach((player) => {
      if (!player.isAlive) return;

      const victory = this.checkDiplomaticVictory(player.id);
      if (victory) {
        player.isAlive = false;
      }
    });
  }

  private updateAIPersonalities(): void {
    this.players.forEach((player) => {
      if (player.isAI) {
        this.updateAIBehavior(player.id);
      }
    });
  }

  private decayRelations(): void {
    const now = Date.now();

    this.relations.forEach((relation) => {
      const timeSinceInteraction = now - relation.lastInteraction;

      if (timeSinceInteraction > 60000) {
        const decayAmount = this.config.trustDecayRate * Math.floor(timeSinceInteraction / 60000);

        relation.relationScore = Math.max(
          this.config.minRelationScore,
          relation.relationScore - decayAmount
        );

        this.updateRelationLevel(relation);
      }
    });
  }

  private updateGlobalRelations(): void {
    let totalHostility = 0;
    let totalCooperation = 0;
    let warCount = 0;

    this.relations.forEach((relation) => {
      if (relation.relationLevel === RelationLevel.War) {
        totalHostility += 1;
        warCount++;
      } else if (relation.relationLevel === RelationLevel.Allied || relation.relationLevel === RelationLevel.Friendly) {
        totalCooperation += 1;
      }
    });

    this.globalRelations.set('global_hostility', totalHostility / Math.max(1, this.relations.size));
    this.globalRelations.set('global_cooperation', totalCooperation / Math.max(1, this.relations.size));
    this.globalRelations.set('military_tension', (warCount / Math.max(1, this.relations.size)) * 100);
  }

  private cleanupExpiredData(): void {
    const now = Date.now();
    const maxAge = 604800000;

    this.diplomaticEvents = this.diplomaticEvents.filter((event) => {
      return now - event.timestamp < maxAge;
    });

    this.diplomaticProposals.forEach((proposal, id) => {
      if (proposal.status === ProposalStatus.Proposed && proposal.expiresAt < now) {
        proposal.status = ProposalStatus.Expired;
      }
    });
  }

  public getDiplomaticStatus(playerId: string): any {
    const player = this.players.get(playerId);
    if (!player) return null;

    const alliance = this.findAllianceContaining(playerId);
    const relations: any[] = [];

    this.relations.forEach((relation) => {
      if (relation.playerId === playerId) {
        relations.push({
          targetPlayerId: relation.targetPlayerId,
          level: relation.relationLevel,
          score: relation.relationScore,
          hasEmbassy: relation.hasEmbassy,
        });
      }
    });

    return {
      playerId,
      alliance: alliance
        ? {
            id: alliance.id,
            name: alliance.name,
            memberCount: alliance.memberIds.length,
            isLeader: alliance.leaderId === playerId,
          }
        : null,
      relations,
      activeWars: Array.from(this.activeWars).filter((key) => key.includes(playerId)),
      tradeAgreements: this.getPlayerTradeAgreements(playerId),
      nonAggressionPacts: this.getPlayerNAPs(playerId),
      embassies: this.getPlayerEmbassies(playerId),
      globalRelations: Object.fromEntries(this.globalRelations),
    };
  }

  private getPlayerTradeAgreements(playerId: string): TradeAgreement[] {
    const agreements: TradeAgreement[] = [];
    this.tradeAgreements.forEach((agreement) => {
      if (agreement.active && agreement.parties.includes(playerId)) {
        agreements.push(agreement);
      }
    });
    return agreements;
  }

  private getPlayerNAPs(playerId: string): NonAggressionPact[] {
    const pacts: NonAggressionPact[] = [];
    this.nonAggressionPacts.forEach((nap) => {
      if (nap.active && nap.parties.includes(playerId)) {
        pacts.push(nap);
      }
    });
    return pacts;
  }

  private getPlayerEmbassies(playerId: string): Embassy[] {
    const embassies: Embassy[] = [];
    this.embassies.forEach((embassy) => {
      if (embassy.ownerId === playerId || embassy.hostPlayerId === playerId) {
        embassies.push(embassy);
      }
    });
    return embassies;
  }

  public createDiplomaticAction(
    type: DiplomaticActionType,
    sourcePlayerId: string,
    targetPlayerId: string,
    effects?: DiplomaticEffects
  ): DiplomaticAction {
    return {
      id: this.generateId(),
      type,
      sourcePlayerId,
      targetPlayerId,
      timestamp: Date.now(),
      resolved: false,
      success: false,
      effects,
    };
  }

  private emitEvent(event: Partial<DiplomaticEvent>): void {
    const fullEvent: DiplomaticEvent = {
      id: this.generateId(),
      type: event.type || DiplomaticEventType.RelationChanged,
      severity: event.severity || EventSeverity.Info,
      sourcePlayerId: event.sourcePlayerId,
      targetPlayerId: event.targetPlayerId,
      allianceId: event.allianceId,
      timestamp: Date.now(),
      title: event.title || 'Event',
      message: event.message || '',
      data: event.data,
      read: false,
      dismissed: false,
      priority: event.severity === EventSeverity.Critical ? 1 : 0,
      relatedActionId: event.data?.action?.id,
    };

    this.diplomaticEvents.push(fullEvent);

    this.eventListeners.forEach((listeners, eventType) => {
      if (eventType === fullEvent.type || eventType === '*') {
        listeners.forEach((listener) => {
          try {
            listener(fullEvent);
          } catch (error) {
            console.error('Error in diplomatic event listener:', error);
          }
        });
      }
    });
  }

  public on(eventType: string, listener: Function): void {
    if (!this.eventListeners.has(eventType)) {
      this.eventListeners.set(eventType, []);
    }
    this.eventListeners.get(eventType)!.push(listener);
  }

  public off(eventType: string, listener: Function): void {
    const listeners = this.eventListeners.get(eventType);
    if (listeners) {
      const index = listeners.indexOf(listener);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private generateId(): string {
    return `${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private isPlayerAI(playerId: string): boolean {
    const player = this.players.get(playerId);
    return player?.isAI || false;
  }

  private breakAllianceRelations(allianceId1: string, allianceId2: string): void {
    const alliance1 = this.alliances.get(allianceId1);
    const alliance2 = this.alliances.get(allianceId2);

    if (!alliance1 || !alliance2) return;

    alliance1.memberIds.forEach((member1) => {
      alliance2.memberIds.forEach((member2) => {
        this.modifyRelation(member1, member2, {
          type: 'alliance_relations_broken',
          value: -100,
          reason: 'Allied alliance relations terminated',
          source: 'system',
        });
      });
    });
  }

  private handleAllianceWar(allianceId: string, player1Id: string, player2Id: string): void {
    const alliance = this.alliances.get(allianceId);
    if (!alliance) return;

    const warMembers = [player1Id, player2Id];

    alliance.memberIds.forEach((memberId) => {
      if (!warMembers.includes(memberId)) {
        warMembers.forEach((warMemberId) => {
          this.modifyRelation(memberId, warMemberId, {
            type: 'alliance_war_splash',
            value: -30,
            reason: `Alliance member ${warMemberId} is at war`,
            source: 'system',
          });
        });
      }
    });
  }

  public getAlliances(): Map<string, Alliance> {
    return this.alliances;
  }

  public getAlliance(allianceId: string): Alliance | null {
    return this.alliances.get(allianceId) || null;
  }

  public getTradeAgreements(): Map<string, TradeAgreement> {
    return this.tradeAgreements;
  }

  public getNonAggressionPacts(): Map<string, NonAggressionPact> {
    return this.nonAggressionPacts;
  }

  public getEmbassies(): Map<string, Embassy> {
    return this.embassies;
  }

  public getDiplomaticEvents(playerId?: string): DiplomaticEvent[] {
    if (!playerId) {
      return this.diplomaticEvents;
    }

    return this.diplomaticEvents.filter(
      (event) =>
        event.sourcePlayerId === playerId ||
        event.targetPlayerId === playerId ||
        event.allianceId
    );
  }

  public getAIPersonality(playerId: string): AIPersonality | null {
    return this.aiPersonalities.get(playerId) || null;
  }

  public getVictoryTracking(playerId: string): DiplomaticVictory | null {
    return this.victoryTracking.get(playerId) || null;
  }

  public setConfig(config: Partial<DiplomacyConfig>): void {
    this.config = { ...this.config, ...config };
  }

  public getConfig(): DiplomacyConfig {
    return { ...this.config };
  }
}

export interface DiplomacyConfig {
  maxRelationScore: number;
  minRelationScore: number;
  relationChangeRate: number;
  betrayalPenaltyMultiplier: number;
  allianceSizeLimit: number;
  embassyMaxLevel: number;
  voteDuration: number;
  proposalExpirationTime: number;
  trustDecayRate: number;
  minTrustForAlliance: AITrustLevel;
}

export interface BetrayalRecord {
  targetId: string;
  betrayalType: DiplomaticActionType;
  timestamp: number;
  severity: number;
  trustImpact: number;
  forgiving: boolean;
  forgiveTimestamp: number;
}

export interface DiplomaticAgreement {
  id: string;
  type: string;
  parties: string[];
  createdAt: number;
  expiresAt?: number;
  active: boolean;
}

export { DiplomacySystem as default };

import { EventEmitter } from '../../core/events/EventEmitter';
import { GameSaveManager, Saveable } from '../../core/save/SaveSystem';
import { Logger } from '../../core/logger/Logger';
import { Random } from '../../core/util/Random';
import { MathUtils } from '../../core/util/MathUtils';
import { TimeManager } from '../../core/time/TimeManager';
import { ResourceManager } from '../../core/resources/ResourceManager';
import { UnitFactory, UnitType } from '../../units/UnitFactory';
import { MapGenerator, MapSize } from '../../maps/MapGenerator';

export enum MissionType {
    Assault = 'assault',
    Defense = 'defense',
    Escort = 'escort',
    Rescue = 'rescue',
    Sabotage = 'sabotage',
    Recon = 'recon',
    Boss = 'boss'
}

export enum MissionPhase {
    Locked = 'locked',
    Available = 'available',
    InProgress = 'in_progress',
    Completed = 'completed',
    Failed = 'failed',
    Skipped = 'skipped'
}

export enum ObjectiveType {
    Primary = 'primary',
    Secondary = 'secondary',
    Bonus = 'bonus',
    Hidden = 'hidden'
}

export enum ObjectiveStatus {
    Pending = 'pending',
    Active = 'active',
    Completed = 'completed',
    Failed = 'failed',
    Skipped = 'skipped'
}

export enum RewardType {
    Credits = 'credits',
    Experience = 'experience',
    Units = 'units',
    Technology = 'technology',
    Resources = 'resources',
    Unlock = 'unlock',
    Achievement = 'achievement',
    Reputation = 'reputation',
    Special = 'special'
}

export enum DifficultyLevel {
    Easy = 'easy',
    Normal = 'normal',
    Hard = 'hard',
    Expert = 'expert',
    Nightmare = 'nightmare'
}

export enum MissionTrigger {
    OnStart = 'on_start',
    OnObjectiveComplete = 'on_objective_complete',
    OnObjectiveFail = 'on_objective_fail',
    OnUnitLost = 'on_unit_lost',
    OnUnitBuilt = 'on_unit_built',
    OnResourceGathered = 'on_resource_gathered',
    OnAreaReached = 'on_area_reached',
    OnTimeElapsed = 'on_time_elapsed',
    OnEnemyDefeated = 'on_enemy_defeated',
    OnBuildingDestroyed = 'on_building_destroyed',
    OnAllObjectivesComplete = 'on_all_objectives_complete',
    OnPlayerHealthBelow = 'on_player_health_below',
    OnUnitReachesHealth = 'on_unit_reaches_health'
}

export enum StarRating {
    OneStar = 1,
    TwoStars = 2,
    ThreeStars = 3
}

export interface MissionObjective {
    id: string;
    name: string;
    description: string;
    type: ObjectiveType;
    targetId?: string;
    targetType?: string;
    targetCount: number;
    currentCount: number;
    position?: { x: number; y: number };
    radius?: number;
    timeLimit?: number;
    optional: boolean;
    rewardMultiplier?: number;
    dependencies?: string[];
    triggers?: MissionTrigger[];
    conditions?: ObjectiveCondition[];
    hiddenUntilActive?: boolean;
    repeatable?: boolean;
}

export interface ObjectiveCondition {
    type: string;
    value: number;
    operator?: 'equal' | 'greater' | 'less' | 'greater_equal' | 'less_equal';
}

export interface MissionReward {
    type: RewardType;
    amount: number;
    itemId?: string;
    unitType?: UnitType;
    unlockId?: string;
    experienceMultiplier?: number;
    reputationPoints?: number;
    probability?: number;
}

export interface MissionNarrative {
    briefingTitle: string;
    briefingText: string;
    debriefingTitle: string;
    debriefingText: string;
    missionStartDialog?: DialogLine[];
    missionEndDialog?: DialogLine[];
    midMissionEvents?: NarrativeEvent[];
    characterNames?: Record<string, string>;
    voiceOverPaths?: Record<string, string>;
}

export interface DialogLine {
    speakerId: string;
    speakerName: string;
    text: string;
    emotion?: string;
    duration?: number;
}

export interface NarrativeEvent {
    trigger: MissionTrigger;
    dialog?: DialogLine[];
    action?: NarrativeAction;
    narrativeText?: string;
}

export interface NarrativeAction {
    type: string;
    target?: string;
    delay?: number;
}

export interface MissionConfig {
    id: string;
    name: string;
    description: string;
    type: MissionType;
    difficulty: DifficultyLevel;
    mapId: string;
    mapSize: MapSize;
    timeLimit?: number;
    recommendedLevel: number;
    prerequisites: string[];
    rewards: MissionReward[];
    bonusRewards: MissionReward[];
    objectives: MissionObjective[];
    narrative: MissionNarrative;
    enemyForces: ForceConfig[];
    alliedForces: ForceConfig[];
    resources?: ResourceConfig;
    specialRules?: SpecialRule[];
    unlocks?: string[];
    tags: string[];
}

export interface ForceConfig {
    factionId: string;
    unitComposition: UnitComposition[];
    aiProfile?: string;
    spawnPositions: SpawnPosition[];
    behaviorOverrides?: Record<string, any>;
}

export interface UnitComposition {
    type: UnitType;
    count: number;
    level?: number;
    equipment?: string[];
}

export interface SpawnPosition {
    x: number;
    y: number;
    facing?: number;
}

export interface ResourceConfig {
    startingCredits: number;
    startingResources: Record<string, number>;
    incomeRate?: Record<string, number>;
}

export interface SpecialRule {
    id: string;
    name: string;
    description: string;
    effect: Record<string, any>;
    activeFromStart: boolean;
}

export interface ObjectiveProgress {
    objectiveId: string;
    currentCount: number;
    status: ObjectiveStatus;
    startTime?: number;
    completionTime?: number;
    attempts: number;
}

export interface MissionState {
    missionId: string;
    phase: MissionPhase;
    startTime?: number;
    endTime?: number;
    elapsedTime: number;
    timeRemaining?: number;
    currentObjectives: ObjectiveProgress[];
    completedObjectives: string[];
    failedObjectives: string[];
    bonusObjectivesCompleted: string[];
    starRating: StarRating;
    score: number;
    scoreBreakdown: Record<string, number>;
    casualties: UnitLoss[];
    kills: UnitKill[];
    resourcesGathered: Record<string, number>;
    resourcesSpent: Record<string, number>;
    events: MissionEventLog[];
    difficulty: DifficultyLevel;
    isCustom: boolean;
    modifiers: MissionModifier[];
}

export interface MissionModifier {
    id: string;
    name: string;
    type: string;
    value: number;
    stackable: boolean;
    source: string;
}

export interface UnitLoss {
    unitId: string;
    unitType: UnitType;
    position: { x: number; y: number };
    time: number;
    cause: string;
}

export interface UnitKill {
    unitId: string;
    unitType: UnitType;
    position: { x: number; y: number };
    time: number;
    killerId: string;
}

export interface MissionEventLog {
    timestamp: number;
    type: string;
    data: Record<string, any>;
}

export interface CampaignProgress {
    campaignId: string;
    currentMissionId: string;
    completedMissions: string[];
    failedMissions: string[];
    totalPlayTime: number;
    bestScores: Record<string, number>;
    bestStarRatings: Record<string, StarRating>;
    unlockedMissions: string[];
    unlockedTech: string[];
    unlockedUnits: string[];
    achievements: string[];
    statistics: CampaignStatistics;
    lastPlayed: number;
    settings: CampaignSettings;
}

export interface CampaignStatistics {
    totalMissionsCompleted: number;
    totalMissionsFailed: number;
    totalUnitsLost: number;
    totalUnitsBuilt: number;
    totalEnemiesDefeated: number;
    totalResourcesGathered: Record<string, number>;
    totalPlayTime: number;
    fastestCompletion?: Record<string, number>;
    highestScore?: Record<string, number>;
}

export interface CampaignSettings {
    difficultyPreference: DifficultyLevel;
    autoSave: boolean;
    showTutorials: boolean;
    enableMods: boolean;
}

export interface MissionSaveData {
    version: string;
    timestamp: number;
    missionState: MissionState;
    campaignProgress: CampaignProgress;
    customMissions: CustomMission[];
}

export interface CustomMission {
    id: string;
    name: string;
    description: string;
    author: string;
    createdAt: number;
    modifiedAt: number;
    config: MissionConfig;
    isValid: boolean;
    validationErrors: string[];
}

export interface StarRatingConfig {
    oneStar: number;
    twoStars: number;
    threeStars: number;
}

export interface BonusObjectiveConfig {
    id: string;
    name: string;
    description: string;
    reward: MissionReward;
    conditions: ObjectiveCondition[];
    timeLimit?: number;
    penalty?: number;
}

export class MissionLoader {
    private static instance: MissionLoader;
    private missions: Map<string, MissionConfig> = new Map();
    private missionBundles: Map<string, MissionBundle> = new Map();
    private loadingPromise: Promise<void> | null = null;

    private constructor() {
        this.initializeDefaultMissions();
    }

    public static getInstance(): MissionLoader {
        if (!MissionLoader.instance) {
            MissionLoader.instance = new MissionLoader();
        }
        return MissionLoader.instance;
    }

    public async loadMissions(bundlePaths: string[]): Promise<void> {
        if (this.loadingPromise) {
            return this.loadingPromise;
        }

        this.loadingPromise = this.performLoad(bundlePaths);
        return this.loadingPromise;
    }

    private async performLoad(bundlePaths: string[]): Promise<void> {
        for (const path of bundlePaths) {
            try {
                const bundle = await this.loadBundle(path);
                this.missionBundles.set(path, bundle);
                for (const mission of bundle.missions) {
                    this.missions.set(mission.id, mission);
                }
            } catch (error) {
                Logger.warn(`Failed to load mission bundle: ${path}`, error);
            }
        }
        this.loadingPromise = null;
    }

    private async loadBundle(path: string): Promise<MissionBundle> {
        return {
            id: path,
            name: path.split('/').pop() || path,
            missions: [],
            version: '1.0.0'
        };
    }

    private initializeDefaultMissions(): void {
        const defaultMissions = this.createDefaultMissionSet();
        for (const mission of defaultMissions) {
            this.missions.set(mission.id, mission);
        }
    }

    private createDefaultMissionSet(): MissionConfig[] {
        return [
            this.createAssaultMission(),
            this.createDefenseMission(),
            this.createEscortMission(),
            this.createRescueMission(),
            this.createSabotageMission(),
            this.createReconMission(),
            this.createBossMission()
        ];
    }

    private createAssaultMission(): MissionConfig {
        return {
            id: 'mission_assault_01',
            name: 'First Strike',
            description: 'Eliminate the enemy base camp and secure the area.',
            type: MissionType.Assault,
            difficulty: DifficultyLevel.Easy,
            mapId: 'map_desert_outpost',
            mapSize: MapSize.Small,
            timeLimit: 900,
            recommendedLevel: 1,
            prerequisites: [],
            rewards: [
                { type: RewardType.Credits, amount: 1000 },
                { type: RewardType.Experience, amount: 500 },
                { type: RewardType.Units, amount: 1, unitType: UnitType.Infantry }
            ],
            bonusRewards: [
                { type: RewardType.Credits, amount: 500, probability: 0.5 }
            ],
            objectives: [
                {
                    id: 'obj_destroy_hq',
                    name: 'Destroy Enemy HQ',
                    description: 'Locate and destroy the enemy headquarters.',
                    type: ObjectiveType.Primary,
                    targetType: 'building',
                    targetCount: 1,
                    currentCount: 0,
                    optional: false
                },
                {
                    id: 'obj_eliminate_forces',
                    name: 'Eliminate Enemy Forces',
                    description: 'Defeat at least 50% of enemy units.',
                    type: ObjectiveType.Primary,
                    targetType: 'unit',
                    targetCount: 25,
                    currentCount: 0,
                    optional: false
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing: First Strike',
                briefingText: 'Commander, our intelligence has identified an enemy base camp in the desert region. Your objective is to launch a surprise assault and eliminate their headquarters before they can reinforce.',
                debriefingTitle: 'Mission Complete',
                debriefingText: 'Excellent work, Commander. The enemy base has been neutralized. Our forces can now advance deeper into enemy territory.'
            },
            enemyForces: [
                {
                    factionId: 'enemy_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 20 },
                        { type: UnitType.Vehicle, count: 10 }
                    ],
                    aiProfile: 'aggressive',
                    spawnPositions: [
                        { x: 800, y: 400 }
                    ]
                }
            ],
            alliedForces: [
                {
                    factionId: 'player_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 15 },
                        { type: UnitType.Vehicle, count: 5 }
                    ],
                    spawnPositions: [
                        { x: 100, y: 100 }
                    ]
                }
            ],
            resources: {
                startingCredits: 2000,
                startingResources: { metal: 500, energy: 300 }
            },
            tags: ['tutorial', 'assault', 'beginner']
        };
    }

    private createDefenseMission(): MissionConfig {
        return {
            id: 'mission_defense_01',
            name: 'Hold the Line',
            description: 'Defend your base against waves of enemy attacks.',
            type: MissionType.Defense,
            difficulty: DifficultyLevel.Normal,
            mapId: 'map_fortress',
            mapSize: MapSize.Medium,
            timeLimit: 1200,
            recommendedLevel: 3,
            prerequisites: ['mission_assault_01'],
            rewards: [
                { type: RewardType.Credits, amount: 1500 },
                { type: RewardType.Experience, amount: 750 }
            ],
            bonusRewards: [
                { type: RewardType.Technology, amount: 1, unlockId: 'turret_upgrade' }
            ],
            objectives: [
                {
                    id: 'obj_survive_waves',
                    name: 'Survive Enemy Waves',
                    description: 'Hold your position for 5 waves of enemy attacks.',
                    type: ObjectiveType.Primary,
                    targetCount: 5,
                    currentCount: 0,
                    optional: false
                },
                {
                    id: 'obj_protect_hq',
                    name: 'Protect Headquarters',
                    description: 'Ensure your HQ survives until reinforcements arrive.',
                    type: ObjectiveType.Primary,
                    targetType: 'hq',
                    targetCount: 1,
                    currentCount: 1,
                    optional: false
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing: Hold the Line',
                briefingText: 'Commander, enemy forces are mobilizing for a major assault on our position. We have limited reinforcements. You must defend the base at all costs until relief forces arrive.',
                debriefingTitle: 'Defense Successful',
                debriefingText: 'Your defensive tactics were exemplary, Commander. The base has been secured and enemy forces have retreated.'
            },
            enemyForces: [
                {
                    factionId: 'enemy_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 30 },
                        { type: UnitType.Vehicle, count: 15 }
                    ],
                    aiProfile: 'aggressive',
                    spawnPositions: [
                        { x: 900, y: 300 },
                        { x: 900, y: 500 }
                    ]
                }
            ],
            alliedForces: [
                {
                    factionId: 'player_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 10 },
                        { type: UnitType.Vehicle, count: 3 }
                    ],
                    spawnPositions: [
                        { x: 200, y: 400 }
                    ]
                }
            ],
            tags: ['defense', 'intermediate']
        };
    }

    private createEscortMission(): MissionConfig {
        return {
            id: 'mission_escort_01',
            name: 'Safe Passage',
            description: 'Escort the VIP convoy to the extraction point.',
            type: MissionType.Escort,
            difficulty: DifficultyLevel.Normal,
            mapId: 'map_convoy_route',
            mapSize: MapSize.Large,
            recommendedLevel: 4,
            prerequisites: ['mission_defense_01'],
            rewards: [
                { type: RewardType.Credits, amount: 2000 },
                { type: RewardType.Experience, amount: 1000 }
            ],
            bonusRewards: [
                { type: RewardType.Credits, amount: 750 }
            ],
            objectives: [
                {
                    id: 'obj_escort_convoy',
                    name: 'Escort VIP Convoy',
                    description: 'Guide the VIP convoy safely to the extraction point.',
                    type: ObjectiveType.Primary,
                    targetType: 'convoy',
                    targetCount: 1,
                    currentCount: 0,
                    position: { x: 900, y: 450 },
                    optional: false
                },
                {
                    id: 'obj_no_convoy_losses',
                    name: 'Zero Casualties',
                    description: 'Complete the mission without losing any convoy units.',
                    type: ObjectiveType.Bonus,
                    targetCount: 1,
                    currentCount: 1,
                    optional: true,
                    rewardMultiplier: 1.5
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing: Safe Passage',
                briefingText: 'Commander, we have a high-value convoy that needs safe passage through hostile territory. The VIP aboard is crucial to our operations. Ensure their safety at all costs.',
                debriefingTitle: 'Escort Complete',
                debriefingText: 'Mission accomplished, Commander. The VIP has been delivered safely. Your careful planning ensured the convoy\'s protection throughout the journey.'
            },
            enemyForces: [
                {
                    factionId: 'enemy_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 25 },
                        { type: UnitType.Vehicle, count: 12 }
                    ],
                    aiProfile: 'ambush',
                    spawnPositions: [
                        { x: 500, y: 200 },
                        { x: 500, y: 600 },
                        { x: 700, y: 400 }
                    ]
                }
            ],
            alliedForces: [
                {
                    factionId: 'player_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 20 },
                        { type: UnitType.Vehicle, count: 8 }
                    ],
                    spawnPositions: [
                        { x: 100, y: 400 }
                    ]
                }
            ],
            tags: ['escort', 'intermediate']
        };
    }

    private createRescueMission(): MissionConfig {
        return {
            id: 'mission_rescue_01',
            name: 'Extraction Point',
            description: 'Rescue the captured operatives from the enemy compound.',
            type: MissionType.Rescue,
            difficulty: DifficultyLevel.Hard,
            mapId: 'map_enemy_compound',
            mapSize: MapSize.Medium,
            timeLimit: 600,
            recommendedLevel: 5,
            prerequisites: ['mission_escort_01'],
            rewards: [
                { type: RewardType.Credits, amount: 2500 },
                { type: RewardType.Experience, amount: 1250 },
                { type: RewardType.Units, amount: 2, unitType: UnitType.Infantry }
            ],
            bonusRewards: [
                { type: RewardType.Resources, amount: 1000 }
            ],
            objectives: [
                {
                    id: 'obj_rescue_operatives',
                    name: 'Rescue Operatives',
                    description: 'Extract all captured operatives from the detention facility.',
                    type: ObjectiveType.Primary,
                    targetType: 'prisoner',
                    targetCount: 5,
                    currentCount: 0,
                    position: { x: 750, y: 400 },
                    optional: false,
                    timeLimit: 600
                },
                {
                    id: 'obj_secure_area',
                    name: 'Secure Extraction Area',
                    description: 'Clear the area around the extraction point.',
                    type: ObjectiveType.Secondary,
                    targetType: 'enemy',
                    targetCount: 20,
                    currentCount: 0,
                    optional: false
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing: Extraction Point',
                briefingText: 'Commander, our operatives have been captured and are being held in an enemy compound. Time is critical. Infiltrate the facility, free the prisoners, and extract them safely.',
                debriefingTitle: 'Rescue Successful',
                debriefingText: 'All operatives have been rescued, Commander. Your swift action saved their lives. They have valuable intelligence to share with us.'
            },
            enemyForces: [
                {
                    factionId: 'enemy_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 40 },
                        { type: UnitType.Vehicle, count: 15 }
                    ],
                    aiProfile: 'defensive',
                    spawnPositions: [
                        { x: 750, y: 400 }
                    ]
                }
            ],
            alliedForces: [
                {
                    factionId: 'player_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 15 },
                        { type: UnitType.Vehicle, count: 5 }
                    ],
                    spawnPositions: [
                        { x: 100, y: 400 }
                    ]
                }
            ],
            tags: ['rescue', 'infiltration', 'advanced']
        };
    }

    private createSabotageMission(): MissionConfig {
        return {
            id: 'mission_sabotage_01',
            name: 'Operation Blackout',
            description: 'Destroy enemy power generators and disrupt their operations.',
            type: MissionType.Sabotage,
            difficulty: DifficultyLevel.Hard,
            mapId: 'map_power_plant',
            mapSize: MapSize.Medium,
            timeLimit: 480,
            recommendedLevel: 6,
            prerequisites: ['mission_rescue_01'],
            rewards: [
                { type: RewardType.Credits, amount: 3000 },
                { type: RewardType.Experience, amount: 1500 },
                { type: RewardType.Technology, amount: 1, unlockId: 'stealth_tech' }
            ],
            bonusRewards: [
                { type: RewardType.Special, amount: 1, unlockId: 'saboteur_class' }
            ],
            objectives: [
                {
                    id: 'obj_destroy_generators',
                    name: 'Destroy Power Generators',
                    description: 'Locate and destroy all power generators in the facility.',
                    type: ObjectiveType.Primary,
                    targetType: 'generator',
                    targetCount: 4,
                    currentCount: 0,
                    optional: false
                },
                {
                    id: 'obj_avoid_detection',
                    name: 'Stealth Approach',
                    description: 'Complete objectives without triggering general alarm.',
                    type: ObjectiveType.Bonus,
                    targetCount: 1,
                    currentCount: 1,
                    optional: true,
                    rewardMultiplier: 2.0
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing: Operation Blackout',
                briefingText: 'Commander, the enemy\'s power grid is their lifeline. Your mission is to infiltrate their power plant and destroy all generators. This will cripple their operations and give our main forces the advantage.',
                debriefingTitle: 'Sabotage Complete',
                debriefingText: 'The power grid has been successfully neutralized, Commander. Enemy operations are in chaos. Our main assault can proceed as planned.'
            },
            enemyForces: [
                {
                    factionId: 'enemy_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 35 },
                        { type: UnitType.Vehicle, count: 10 }
                    ],
                    aiProfile: 'patrol',
                    spawnPositions: [
                        { x: 600, y: 300 },
                        { x: 600, y: 500 }
                    ]
                }
            ],
            alliedForces: [
                {
                    factionId: 'player_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 12 },
                        { type: UnitType.Vehicle, count: 3 }
                    ],
                    spawnPositions: [
                        { x: 100, y: 400 }
                    ]
                }
            ],
            tags: ['sabotage', 'stealth', 'advanced']
        };
    }

    private createReconMission(): MissionConfig {
        return {
            id: 'mission_recon_01',
            name: 'Eyes in the Sky',
            description: 'Survey the enemy territory and mark targets for strike missions.',
            type: MissionType.Recon,
            difficulty: DifficultyLevel.Normal,
            mapId: 'map_enemy_territory',
            mapSize: MapSize.Large,
            timeLimit: 900,
            recommendedLevel: 4,
            prerequisites: ['mission_defense_01'],
            rewards: [
                { type: RewardType.Credits, amount: 1800 },
                { type: RewardType.Experience, amount: 900 }
            ],
            bonusRewards: [
                { type: RewardType.Credits, amount: 600 }
            ],
            objectives: [
                {
                    id: 'obj_survey_areas',
                    name: 'Survey Designated Areas',
                    description: 'Mark all reconnaissance points on the map.',
                    type: ObjectiveType.Primary,
                    targetType: 'recon_point',
                    targetCount: 6,
                    currentCount: 0,
                    optional: false
                },
                {
                    id: 'obj_photograph_targets',
                    name: 'Photograph Enemy Assets',
                    description: 'Capture images of enemy units and structures.',
                    type: ObjectiveType.Secondary,
                    targetType: 'target',
                    targetCount: 10,
                    currentCount: 0,
                    optional: true
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing: Eyes in the Sky',
                briefingText: 'Commander, we need detailed intelligence on enemy positions before our major offensive. Your team will survey the territory and mark targets for upcoming strike missions.',
                debriefingTitle: 'Recon Complete',
                debriefingText: 'Excellent reconnaissance work, Commander. The intelligence gathered will be invaluable for planning our next operations.'
            },
            enemyForces: [
                {
                    factionId: 'enemy_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 20 },
                        { type: UnitType.Vehicle, count: 8 }
                    ],
                    aiProfile: 'patrol',
                    spawnPositions: [
                        { x: 700, y: 400 }
                    ]
                }
            ],
            alliedForces: [
                {
                    factionId: 'player_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 8 },
                        { type: UnitType.Vehicle, count: 4 }
                    ],
                    spawnPositions: [
                        { x: 100, y: 400 }
                    ]
                }
            ],
            tags: ['recon', 'intelligence', 'intermediate']
        };
    }

    private createBossMission(): MissionConfig {
        return {
            id: 'mission_boss_01',
            name: 'Final Confrontation',
            description: 'Defeat the enemy commander and end the war.',
            type: MissionType.Boss,
            difficulty: DifficultyLevel.Expert,
            mapId: 'map_final_battle',
            mapSize: MapSize.Large,
            timeLimit: 1200,
            recommendedLevel: 10,
            prerequisites: ['mission_sabotage_01', 'mission_rescue_01'],
            rewards: [
                { type: RewardType.Credits, amount: 5000 },
                { type: RewardType.Experience, amount: 2500 },
                { type: RewardType.Special, amount: 1, unlockId: 'victory_achievement' }
            ],
            bonusRewards: [
                { type: RewardType.Special, amount: 1, unlockId: 'master_commander_title' }
            ],
            objectives: [
                {
                    id: 'obj_defeat_commander',
                    name: 'Defeat Enemy Commander',
                    description: 'Locate and defeat the enemy commander.',
                    type: ObjectiveType.Primary,
                    targetType: 'commander',
                    targetCount: 1,
                    currentCount: 0,
                    optional: false
                },
                {
                    id: 'obj_destroy_command_center',
                    name: 'Destroy Command Center',
                    description: 'Eliminate the enemy\'s main command structure.',
                    type: ObjectiveType.Primary,
                    targetType: 'command_center',
                    targetCount: 1,
                    currentCount: 0,
                    optional: false
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing: Final Confrontation',
                briefingText: 'Commander, this is it. The enemy commander has been located. End this war once and for all. Destroy the command center and eliminate their leadership.',
                debriefingTitle: 'Victory',
                debriefingText: 'The war is over, Commander. Your leadership has brought us to victory. The enemy has been defeated and peace can finally return to our lands.'
            },
            enemyForces: [
                {
                    factionId: 'enemy_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 50 },
                        { type: UnitType.Vehicle, count: 25 }
                    ],
                    aiProfile: 'boss',
                    spawnPositions: [
                        { x: 800, y: 400 }
                    ]
                }
            ],
            alliedForces: [
                {
                    factionId: 'player_faction',
                    unitComposition: [
                        { type: UnitType.Infantry, count: 30 },
                        { type: UnitType.Vehicle, count: 15 }
                    ],
                    spawnPositions: [
                        { x: 100, y: 400 }
                    ]
                }
            ],
            tags: ['boss', 'final', 'expert']
        };
    }

    public getMission(missionId: string): MissionConfig | undefined {
        return this.missions.get(missionId);
    }

    public getAllMissions(): MissionConfig[] {
        return Array.from(this.missions.values());
    }

    public getMissionsByType(type: MissionType): MissionConfig[] {
        return this.getAllMissions().filter(m => m.type === type);
    }

    public getMissionsByDifficulty(difficulty: DifficultyLevel): MissionConfig[] {
        return this.getAllMissions().filter(m => m.difficulty === difficulty);
    }

    public getAvailableMissions(prerequisites: string[]): MissionConfig[] {
        return this.getAllMissions().filter(mission => {
            return mission.prerequisites.every(prereq => prerequisites.includes(prereq));
        });
    }

    public getMissionBundle(path: string): MissionBundle | undefined {
        return this.missionBundles.get(path);
    }
}

export interface MissionBundle {
    id: string;
    name: string;
    missions: MissionConfig[];
    version: string;
}

export class MissionState implements Saveable {
    private missionConfig: MissionConfig;
    private phase: MissionPhase;
    private startTime: number = 0;
    private endTime: number = 0;
    private elapsedTime: number = 0;
    private timeRemaining: number = 0;
    private objectiveProgress: Map<string, ObjectiveProgress> = new Map();
    private completedObjectives: Set<string> = new Set();
    private failedObjectives: Set<string> = new Set();
    private bonusObjectivesCompleted: Set<string> = new Set();
    private starRating: StarRating = StarRating.OneStar;
    private score: number = 0;
    private scoreBreakdown: Map<string, number> = new Map();
    private casualties: UnitLoss[] = [];
    private kills: UnitKill[] = [];
    private resourcesGathered: Map<string, number> = new Map();
    private resourcesSpent: Map<string, number> = new Map();
    private events: MissionEventLog[] = [];
    private modifiers: MissionModifier[] = [];
    private paused: boolean = false;
    private pauseStartTime: number = 0;
    private totalPauseTime: number = 0;

    constructor(missionConfig: MissionConfig) {
        this.missionConfig = missionConfig;
        this.phase = MissionPhase.Locked;
        this.initializeObjectives();
        this.timeRemaining = missionConfig.timeLimit || 0;
    }

    private initializeObjectives(): void {
        for (const objective of this.missionConfig.objectives) {
            this.objectiveProgress.set(objective.id, {
                objectiveId: objective.id,
                currentCount: 0,
                status: ObjectiveStatus.Pending,
                attempts: 0
            });
        }
    }

    public start(): void {
        if (this.phase === MissionPhase.Available) {
            this.phase = MissionPhase.InProgress;
            this.startTime = Date.now();
            this.elapsedTime = 0;
            this.timeRemaining = this.missionConfig.timeLimit || 0;
            this.logEvent('mission_start', { missionId: this.missionConfig.id });
            this.activateObjectives();
        }
    }

    private activateObjectives(): void {
        for (const objective of this.missionConfig.objectives) {
            if (!objective.dependencies || objective.dependencies.length === 0) {
                this.setObjectiveStatus(objective.id, ObjectiveStatus.Active);
            }
        }
    }

    public update(deltaTime: number): void {
        if (this.phase !== MissionPhase.InProgress || this.paused) {
            return;
        }

        this.elapsedTime += deltaTime;
        if (this.missionConfig.timeLimit) {
            this.timeRemaining = this.missionConfig.timeLimit - this.elapsedTime;
            if (this.timeRemaining <= 0) {
                this.failMission('Time expired');
            }
        }

        this.updateObjectiveTimers(deltaTime);
    }

    private updateObjectiveTimers(deltaTime: number): void {
        for (const objective of this.missionConfig.objectives) {
            if (objective.timeLimit) {
                const progress = this.objectiveProgress.get(objective.id);
                if (progress && progress.status === ObjectiveStatus.Active) {
                    const timeRemaining = objective.timeLimit - this.elapsedTime;
                    if (timeRemaining <= 0 && !objective.optional) {
                        this.failObjective(objective.id);
                    }
                }
            }
        }
    }

    public updateObjectiveProgress(objectiveId: string, increment: number): void {
        const progress = this.objectiveProgress.get(objectiveId);
        if (!progress || progress.status !== ObjectiveStatus.Active) {
            return;
        }

        progress.currentCount += increment;
        
        const objective = this.missionConfig.objectives.find(o => o.id === objectiveId);
        if (objective && progress.currentCount >= objective.targetCount) {
            this.completeObjective(objectiveId);
        }

        this.logEvent('objective_progress', {
            objectiveId,
            currentCount: progress.currentCount
        });
    }

    public setObjectiveStatus(objectiveId: string, status: ObjectiveStatus): void {
        const progress = this.objectiveProgress.get(objectiveId);
        if (!progress) {
            return;
        }

        const previousStatus = progress.status;
        progress.status = status;

        if (status === ObjectiveStatus.Completed) {
            progress.completionTime = this.elapsedTime;
            this.completedObjectives.add(objectiveId);
            this.checkForDependentObjectives(objectiveId);
        } else if (status === ObjectiveStatus.Failed) {
            this.failedObjectives.add(objectiveId);
            if (!this.isObjectiveOptional(objectiveId)) {
                this.checkMissionFailure();
            }
        }

        this.logEvent('objective_status_change', {
            objectiveId,
            previousStatus,
            newStatus: status
        });
    }

    private isObjectiveOptional(objectiveId: string): boolean {
        const objective = this.missionConfig.objectives.find(o => o.id === objectiveId);
        return objective?.optional || false;
    }

    private completeObjective(objectiveId: string): void {
        this.setObjectiveStatus(objectiveId, ObjectiveStatus.Completed);
        
        const objective = this.missionConfig.objectives.find(o => o.id === objectiveId);
        if (objective?.type === ObjectiveType.Bonus) {
            this.bonusObjectivesCompleted.add(objectiveId);
        }

        this.addScore('objective_complete', 100);
        this.checkAllPrimaryObjectivesComplete();
    }

    private failObjective(objectiveId: string): void {
        this.setObjectiveStatus(objectiveId, ObjectiveStatus.Failed);
        this.addScore('objective_fail', -50);
    }

    private checkForDependentObjectives(completedId: string): void {
        for (const objective of this.missionConfig.objectives) {
            if (objective.dependencies?.includes(completedId)) {
                const allDependenciesMet = objective.dependencies.every(depId => 
                    this.completedObjectives.has(depId)
                );
                if (allDependenciesMet) {
                    this.setObjectiveStatus(objective.id, ObjectiveStatus.Active);
                }
            }
        }
    }

    private checkAllPrimaryObjectivesComplete(): void {
        const primaryObjectives = this.missionConfig.objectives.filter(
            o => o.type === ObjectiveType.Primary
        );
        
        const allComplete = primaryObjectives.every(
            o => this.completedObjectives.has(o.id)
        );

        if (allComplete) {
            this.completeMission();
        }
    }

    private checkMissionFailure(): void {
        const failedPrimary = this.missionConfig.objectives.some(
            o => o.type === ObjectiveType.Primary && this.failedObjectives.has(o.id)
        );

        if (failedPrimary) {
            this.failMission('Primary objective failed');
        }
    }

    public completeMission(): void {
        if (this.phase === MissionPhase.InProgress) {
            this.phase = MissionPhase.Completed;
            this.endTime = Date.now();
            this.calculateStarRating();
            this.calculateFinalScore();
            this.logEvent('mission_complete', {
                duration: this.elapsedTime,
                starRating: this.starRating,
                score: this.score
            });
        }
    }

    public failMission(reason: string): void {
        if (this.phase === MissionPhase.InProgress) {
            this.phase = MissionPhase.Failed;
            this.endTime = Date.now();
            this.starRating = StarRating.OneStar;
            this.logEvent('mission_fail', {
                reason,
                duration: this.elapsedTime
            });
        }
    }

    private calculateStarRating(): void {
        const starConfig = this.getStarRatingThresholds();
        const timeBonus = this.calculateTimeBonus();
        const objectiveBonus = this.calculateObjectiveBonus();
        const casualtyPenalty = this.calculateCasualtyPenalty();

        const totalScore = timeBonus + objectiveBonus - casualtyPenalty;

        if (totalScore >= starConfig.threeStars) {
            this.starRating = StarRating.ThreeStars;
        } else if (totalScore >= starConfig.twoStars) {
            this.starRating = StarRating.TwoStars;
        } else {
            this.starRating = StarRating.OneStar;
        }

        this.score = Math.max(0, totalScore);
    }

    private getStarRatingThresholds(): { oneStar: number; twoStars: number; threeStars: number } {
        return {
            oneStar: 0,
            twoStars: 500,
            threeStars: 1000
        };
    }

    private calculateTimeBonus(): number {
        if (!this.missionConfig.timeLimit) {
            return 500;
        }
        const timeRemaining = this.timeRemaining;
        const timeLimit = this.missionConfig.timeLimit;
        return Math.floor((timeRemaining / timeLimit) * 500);
    }

    private calculateObjectiveBonus(): number {
        let bonus = 0;
        
        for (const objective of this.missionConfig.objectives) {
            if (this.completedObjectives.has(objective.id)) {
                if (objective.type === ObjectiveType.Primary) {
                    bonus += 100;
                } else if (objective.type === ObjectiveType.Secondary) {
                    bonus += 50;
                } else if (objective.type === ObjectiveType.Bonus) {
                    bonus += 25;
                }
            }
        }

        return bonus;
    }

    private calculateCasualtyPenalty(): number {
        return this.casualties.length * 10;
    }

    private calculateFinalScore(): void {
        this.scoreBreakdown.set('time_bonus', this.calculateTimeBonus());
        this.scoreBreakdown.set('objective_bonus', this.calculateObjectiveBonus());
        this.scoreBreakdown.set('casualty_penalty', this.calculateCasualtyPenalty());
    }

    public addScore(category: string, amount: number): void {
        const current = this.scoreBreakdown.get(category) || 0;
        this.scoreBreakdown.set(category, current + amount);
        this.score += amount;
    }

    public recordUnitLoss(loss: UnitLoss): void {
        this.casualties.push(loss);
        this.logEvent('unit_lost', loss);
    }

    public recordUnitKill(kill: UnitKill): void {
        this.kills.push(kill);
        this.logEvent('unit_killed', kill);
    }

    public addResourcesGathered(type: string, amount: number): void {
        const current = this.resourcesGathered.get(type) || 0;
        this.resourcesGathered.set(type, current + amount);
    }

    public addResourcesSpent(type: string, amount: number): void {
        const current = this.resourcesSpent.get(type) || 0;
        this.resourcesSpent.set(type, current + amount);
    }

    private logEvent(type: string, data: Record<string, any>): void {
        this.events.push({
            timestamp: this.elapsedTime,
            type,
            data
        });
    }

    public pause(): void {
        if (this.phase === MissionPhase.InProgress && !this.paused) {
            this.paused = true;
            this.pauseStartTime = Date.now();
        }
    }

    public resume(): void {
        if (this.paused) {
            this.totalPauseTime += Date.now() - this.pauseStartTime;
            this.paused = false;
        }
    }

    public isPaused(): boolean {
        return this.paused;
    }

    public getPhase(): MissionPhase {
        return this.phase;
    }

    public setPhase(phase: MissionPhase): void {
        this.phase = phase;
    }

    public getMissionConfig(): MissionConfig {
        return this.missionConfig;
    }

    public getElapsedTime(): number {
        return this.elapsedTime;
    }

    public getTimeRemaining(): number {
        return this.timeRemaining;
    }

    public getScore(): number {
        return this.score;
    }

    public getStarRating(): StarRating {
        return this.starRating;
    }

    public getCompletedObjectives(): string[] {
        return Array.from(this.completedObjectives);
    }

    public getFailedObjectives(): string[] {
        return Array.from(this.failedObjectives);
    }

    public getObjectiveProgress(): ObjectiveProgress[] {
        return Array.from(this.objectiveProgress.values());
    }

    public getModifiers(): MissionModifier[] {
        return this.modifiers;
    }

    public addModifier(modifier: MissionModifier): void {
        this.modifiers.push(modifier);
    }

    public removeModifier(modifierId: string): void {
        this.modifiers = this.modifiers.filter(m => m.id !== modifierId);
    }

    public getState(): MissionStateData {
        return {
            missionId: this.missionConfig.id,
            phase: this.phase,
            startTime: this.startTime,
            endTime: this.endTime,
            elapsedTime: this.elapsedTime,
            timeRemaining: this.timeRemaining,
            objectiveProgress: Array.from(this.objectiveProgress.values()),
            completedObjectives: Array.from(this.completedObjectives),
            failedObjectives: Array.from(this.failedObjectives),
            bonusObjectivesCompleted: Array.from(this.bonusObjectivesCompleted),
            starRating: this.starRating,
            score: this.score,
            scoreBreakdown: Object.fromEntries(this.scoreBreakdown),
            casualties: this.casualties,
            kills: this.kills,
            resourcesGathered: Object.fromEntries(this.resourcesGathered),
            resourcesSpent: Object.fromEntries(this.resourcesSpent),
            events: this.events,
            difficulty: this.missionConfig.difficulty,
            isCustom: false,
            modifiers: this.modifiers
        };
    }

    public loadState(state: MissionStateData): void {
        this.phase = state.phase;
        this.startTime = state.startTime;
        this.endTime = state.endTime;
        this.elapsedTime = state.elapsedTime;
        this.timeRemaining = state.timeRemaining;
        this.starRating = state.starRating;
        this.score = state.score;
        this.casualties = state.casualties;
        this.kills = state.kills;
        this.events = state.events;
        this.modifiers = state.modifiers;

        this.objectiveProgress.clear();
        for (const progress of state.objectiveProgress) {
            this.objectiveProgress.set(progress.objectiveId, progress);
        }

        this.completedObjectives.clear();
        for (const id of state.completedObjectives) {
            this.completedObjectives.add(id);
        }

        this.failedObjectives.clear();
        for (const id of state.failedObjectives) {
            this.failedObjectives.add(id);
        }

        this.bonusObjectivesCompleted.clear();
        for (const id of state.bonusObjectivesCompleted) {
            this.bonusObjectivesCompleted.add(id);
        }

        this.scoreBreakdown.clear();
        for (const [key, value] of Object.entries(state.scoreBreakdown)) {
            this.scoreBreakdown.set(key, value);
        }

        this.resourcesGathered.clear();
        for (const [key, value] of Object.entries(state.resourcesGathered)) {
            this.resourcesGathered.set(key, value);
        }

        this.resourcesSpent.clear();
        for (const [key, value] of Object.entries(state.resourcesSpent)) {
            this.resourcesSpent.set(key, value);
        }
    }

    public getSaveData(): MissionStateData {
        return this.getState();
    }
}

interface MissionStateData {
    missionId: string;
    phase: MissionPhase;
    startTime: number;
    endTime: number;
    elapsedTime: number;
    timeRemaining: number;
    objectiveProgress: ObjectiveProgress[];
    completedObjectives: string[];
    failedObjectives: string[];
    bonusObjectivesCompleted: string[];
    starRating: StarRating;
    score: number;
    scoreBreakdown: Record<string, number>;
    casualties: UnitLoss[];
    kills: UnitKill[];
    resourcesGathered: Record<string, number>;
    resourcesSpent: Record<string, number>;
    events: MissionEventLog[];
    difficulty: DifficultyLevel;
    isCustom: boolean;
    modifiers: MissionModifier[];
}

export class ObjectiveTracker {
    private tracker: Map<string, ObjectiveTrackingData> = new Map();
    private listeners: Map<string, ObjectiveTrackerListener[]> = new Map();

    constructor() {
        this.initialize();
    }

    private initialize(): void {
    }

    public createObjectiveTracking(objective: MissionObjective): ObjectiveTrackingData {
        const data: ObjectiveTrackingData = {
            objectiveId: objective.id,
            type: objective.type,
            targetCount: objective.targetCount,
            currentCount: 0,
            status: ObjectiveStatus.Pending,
            progress: 0,
            timeStarted: 0,
            timeCompleted: 0,
            attempts: 0,
            hintsRevealed: 0,
            lastUpdateTime: 0
        };
        
        this.tracker.set(objective.id, data);
        return data;
    }

    public updateProgress(objectiveId: string, increment: number): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        const previousCount = data.currentCount;
        data.currentCount = Math.min(data.targetCount, data.currentCount + increment);
        data.progress = data.targetCount > 0 ? data.currentCount / data.targetCount : 0;
        data.lastUpdateTime = Date.now();

        if (data.currentCount >= data.targetCount && previousCount < data.targetCount) {
            data.status = ObjectiveStatus.Completed;
            data.timeCompleted = Date.now();
            this.notifyCompletion(objectiveId);
        }

        this.notifyProgress(objectiveId, data);
    }

    public setProgress(objectiveId: string, count: number): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        data.currentCount = Math.min(data.targetCount, count);
        data.progress = data.targetCount > 0 ? data.currentCount / data.targetCount : 0;
        data.lastUpdateTime = Date.now();

        this.notifyProgress(objectiveId, data);
    }

    public activateObjective(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        data.status = ObjectiveStatus.Active;
        data.timeStarted = Date.now();
        this.notifyActivation(objectiveId);
    }

    public completeObjective(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        data.status = ObjectiveStatus.Completed;
        data.currentCount = data.targetCount;
        data.progress = 1;
        data.timeCompleted = Date.now();
        this.notifyCompletion(objectiveId);
    }

    public failObjective(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        data.status = ObjectiveStatus.Failed;
        data.attempts++;
        this.notifyFailure(objectiveId);
    }

    public skipObjective(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        data.status = ObjectiveStatus.Skipped;
        this.notifySkip(objectiveId);
    }

    public resetObjective(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        data.currentCount = 0;
        data.progress = 0;
        data.status = ObjectiveStatus.Pending;
        data.attempts++;
        this.notifyReset(objectiveId);
    }

    public revealHint(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        data.hintsRevealed++;
        this.notifyHint(objectiveId, data.hintsRevealed);
    }

    public getProgress(objectiveId: string): ObjectiveTrackingData | undefined {
        return this.tracker.get(objectiveId);
    }

    public getAllProgress(): ObjectiveTrackingData[] {
        return Array.from(this.tracker.values());
    }

    public getActiveObjectives(): ObjectiveTrackingData[] {
        return Array.from(this.tracker.values()).filter(d => d.status === ObjectiveStatus.Active);
    }

    public getCompletedObjectives(): ObjectiveTrackingData[] {
        return Array.from(this.tracker.values()).filter(d => d.status === ObjectiveStatus.Completed);
    }

    public getFailedObjectives(): ObjectiveTrackingData[] {
        return Array.from(this.tracker.values()).filter(d => d.status === ObjectiveStatus.Failed);
    }

    public getPendingObjectives(): ObjectiveTrackingData[] {
        return Array.from(this.tracker.values()).filter(d => d.status === ObjectiveStatus.Pending);
    }

    public addListener(objectiveId: string, listener: ObjectiveTrackerListener): void {
        if (!this.listeners.has(objectiveId)) {
            this.listeners.set(objectiveId, []);
        }
        this.listeners.get(objectiveId)!.push(listener);
    }

    public removeListener(objectiveId: string, listener: ObjectiveTrackerListener): void {
        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            const index = listeners.indexOf(listener);
            if (index > -1) {
                listeners.splice(index, 1);
            }
        }
    }

    private notifyProgress(objectiveId: string, data: ObjectiveTrackingData): void {
        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            for (const listener of listeners) {
                listener.onProgressUpdate(data);
            }
        }
    }

    private notifyCompletion(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            for (const listener of listeners) {
                listener.onObjectiveComplete(data);
            }
        }
    }

    private notifyFailure(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            for (const listener of listeners) {
                listener.onObjectiveFail(data);
            }
        }
    }

    private notifyActivation(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            for (const listener of listeners) {
                listener.onObjectiveActivate(data);
            }
        }
    }

    private notifySkip(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            for (const listener of listeners) {
                listener.onObjectiveSkip(data);
            }
        }
    }

    private notifyReset(objectiveId: string): void {
        const data = this.tracker.get(objectiveId);
        if (!data) {
            return;
        }

        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            for (const listener of listeners) {
                listener.onObjectiveReset(data);
            }
        }
    }

    private notifyHint(objectiveId: string, hintLevel: number): void {
        const listeners = this.listeners.get(objectiveId);
        if (listeners) {
            for (const listener of listeners) {
                listener.onHintRevealed(objectiveId, hintLevel);
            }
        }
    }

    public getCompletionPercentage(): number {
        const all = this.getAllProgress();
        if (all.length === 0) {
            return 0;
        }

        const completed = all.filter(d => d.status === ObjectiveStatus.Completed).length;
        return (completed / all.length) * 100;
    }

    public getPrimaryCompletionPercentage(): number {
        const primary = this.getAllProgress().filter(d => d.type === ObjectiveType.Primary);
        if (primary.length === 0) {
            return 0;
        }

        const completed = primary.filter(d => d.status === ObjectiveStatus.Completed).length;
        return (completed / primary.length) * 100;
    }

    public clear(): void {
        this.tracker.clear();
        this.listeners.clear();
    }

    public clearListeners(): void {
        this.listeners.clear();
    }
}

export interface ObjectiveTrackingData {
    objectiveId: string;
    type: ObjectiveType;
    targetCount: number;
    currentCount: number;
    status: ObjectiveStatus;
    progress: number;
    timeStarted: number;
    timeCompleted: number;
    attempts: number;
    hintsRevealed: number;
    lastUpdateTime: number;
}

export interface ObjectiveTrackerListener {
    onProgressUpdate(data: ObjectiveTrackingData): void;
    onObjectiveComplete(data: ObjectiveTrackingData): void;
    onObjectiveFail(data: ObjectiveTrackingData): void;
    onObjectiveActivate(data: ObjectiveTrackingData): void;
    onObjectiveSkip(data: ObjectiveTrackingData): void;
    onObjectiveReset(data: ObjectiveTrackingData): void;
    onHintRevealed(objectiveId: string, hintLevel: number): void;
}

export class MissionEvents extends EventEmitter {
    private static instance: MissionEvents;
    private eventHistory: MissionEvent[] = [];
    private activeTriggers: Map<MissionTrigger, MissionTriggerConfig> = new Map();

    private constructor() {
        super();
        this.initializeDefaultTriggers();
    }

    public static getInstance(): MissionEvents {
        if (!MissionEvents.instance) {
            MissionEvents.instance = new MissionEvents();
        }
        return MissionEvents.instance;
    }

    private initializeDefaultTriggers(): void {
        const defaultTriggers: MissionTrigger[] = [
            MissionTrigger.OnStart,
            MissionTrigger.OnObjectiveComplete,
            MissionTrigger.OnObjectiveFail,
            MissionTrigger.OnUnitLost,
            MissionTrigger.OnUnitBuilt,
            MissionTrigger.OnResourceGathered,
            MissionTrigger.OnAreaReached,
            MissionTrigger.OnTimeElapsed,
            MissionTrigger.OnEnemyDefeated,
            MissionTrigger.OnBuildingDestroyed,
            MissionTrigger.OnAllObjectivesComplete,
            MissionTrigger.OnPlayerHealthBelow,
            MissionTrigger.OnUnitReachesHealth
        ];

        for (const trigger of defaultTriggers) {
            this.activeTriggers.set(trigger, {
                trigger,
                enabled: true,
                conditions: [],
                actions: []
            });
        }
    }

    public registerTrigger(
        trigger: MissionTrigger,
        condition: TriggerCondition | null,
        action: TriggerAction
    ): void {
        const config = this.activeTriggers.get(trigger);
        if (config) {
            if (condition) {
                config.conditions.push(condition);
            }
            config.actions.push(action);
        }
    }

    public unregisterTrigger(trigger: MissionTrigger, actionId: string): void {
        const config = this.activeTriggers.get(trigger);
        if (config) {
            config.actions = config.actions.filter(a => a.id !== actionId);
        }
    }

    public enableTrigger(trigger: MissionTrigger): void {
        const config = this.activeTriggers.get(trigger);
        if (config) {
            config.enabled = true;
        }
    }

    public disableTrigger(trigger: MissionTrigger): void {
        const config = this.activeTriggers.get(trigger);
        if (config) {
            config.enabled = false;
        }
    }

    public triggerEvent(
        trigger: MissionTrigger,
        data: Record<string, any>
    ): void {
        const config = this.activeTriggers.get(trigger);
        if (!config || !config.enabled) {
            return;
        }

        const conditionsMet = config.conditions.every(condition => 
            this.evaluateCondition(condition, data)
        );

        if (!conditionsMet) {
            return;
        }

        const event: MissionEvent = {
            trigger,
            data,
            timestamp: Date.now()
        };

        this.eventHistory.push(event);
        this.emit(trigger, event);

        for (const action of config.actions) {
            this.executeAction(action, data);
        }
    }

    private evaluateCondition(condition: TriggerCondition, data: Record<string, any>): boolean {
        const value = data[condition.property];
        
        switch (condition.operator) {
            case 'equal':
                return value === condition.value;
            case 'greater':
                return value > condition.value;
            case 'less':
                return value < condition.value;
            case 'greater_equal':
                return value >= condition.value;
            case 'less_equal':
                return value <= condition.value;
            case 'contains':
                return Array.isArray(value) && value.includes(condition.value);
            default:
                return false;
        }
    }

    private executeAction(action: TriggerAction, data: Record<string, any>): void {
        switch (action.type) {
            case 'spawn_unit':
                this.handleSpawnUnit(action, data);
                break;
            case 'show_dialog':
                this.handleShowDialog(action, data);
                break;
            case 'play_sound':
                this.handlePlaySound(action, data);
                break;
            case 'activate_objective':
                this.handleActivateObjective(action, data);
                break;
            case 'deactivate_objective':
                this.handleDeactivateObjective(action, data);
                break;
            case 'modify_resources':
                this.handleModifyResources(action, data);
                break;
            case 'trigger_cutscene':
                this.handleTriggerCutscene(action, data);
                break;
            case 'camera_focus':
                this.handleCameraFocus(action, data);
                break;
            case 'show_notification':
                this.handleShowNotification(action, data);
                break;
            case 'set_flag':
                this.handleSetFlag(action, data);
                break;
        }
    }

    private handleSpawnUnit(action: TriggerAction, data: Record<string, any>): void {
        Logger.debug(`Spawn unit: ${action.params.unitType} at position`);
    }

    private handleShowDialog(action: TriggerAction, data: Record<string, any>): void {
        this.emit('show_dialog', action.params);
    }

    private handlePlaySound(action: TriggerAction, data: Record<string, any>): void {
        this.emit('play_sound', action.params);
    }

    private handleActivateObjective(action: TriggerAction, data: Record<string, any>): void {
        this.emit('activate_objective', action.params);
    }

    private handleDeactivateObjective(action: TriggerAction, data: Record<string, any>): void {
        this.emit('deactivate_objective', action.params);
    }

    private handleModifyResources(action: TriggerAction, data: Record<string, any>): void {
        this.emit('modify_resources', action.params);
    }

    private handleTriggerCutscene(action: TriggerAction, data: Record<string, any>): void {
        this.emit('trigger_cutscene', action.params);
    }

    private handleCameraFocus(action: TriggerAction, data: Record<string, any>): void {
        this.emit('camera_focus', action.params);
    }

    private handleShowNotification(action: TriggerAction, data: Record<string, any>): void {
        this.emit('show_notification', action.params);
    }

    private handleSetFlag(action: TriggerAction, data: Record<string, any>): void {
        this.emit('set_flag', action.params);
    }

    public getEventHistory(): MissionEvent[] {
        return [...this.eventHistory];
    }

    public getEventsByTrigger(trigger: MissionTrigger): MissionEvent[] {
        return this.eventHistory.filter(e => e.trigger === trigger);
    }

    public getRecentEvents(count: number): MissionEvent[] {
        return this.eventHistory.slice(-count);
    }

    public clearHistory(): void {
        this.eventHistory = [];
    }
}

export interface MissionEvent {
    trigger: MissionTrigger;
    data: Record<string, any>;
    timestamp: number;
}

export interface MissionTriggerConfig {
    trigger: MissionTrigger;
    enabled: boolean;
    conditions: TriggerCondition[];
    actions: TriggerAction[];
}

export interface TriggerCondition {
    property: string;
    operator: 'equal' | 'greater' | 'less' | 'greater_equal' | 'less_equal' | 'contains';
    value: any;
}

export interface TriggerAction {
    id: string;
    type: string;
    params: Record<string, any>;
    delay?: number;
    once?: boolean;
    executed?: boolean;
}

export class DifficultyScaler {
    private static instance: DifficultyScaler;
    private difficultyMultipliers: Map<DifficultyLevel, DifficultyMultiplier> = new Map();

    private constructor() {
        this.initializeMultipliers();
    }

    public static getInstance(): DifficultyScaler {
        if (!DifficultyScaler.instance) {
            DifficultyScaler.instance = new DifficultyScaler();
        }
        return DifficultyScaler.instance;
    }

    private initializeMultipliers(): void {
        this.difficultyMultipliers.set(DifficultyLevel.Easy, {
            enemyHealthMultiplier: 0.75,
            enemyDamageMultiplier: 0.75,
            enemyCountMultiplier: 0.75,
            timeLimitMultiplier: 1.5,
            rewardMultiplier: 1.0,
            resourceMultiplier: 1.25
        });

        this.difficultyMultipliers.set(DifficultyLevel.Normal, {
            enemyHealthMultiplier: 1.0,
            enemyDamageMultiplier: 1.0,
            enemyCountMultiplier: 1.0,
            timeLimitMultiplier: 1.0,
            rewardMultiplier: 1.0,
            resourceMultiplier: 1.0
        });

        this.difficultyMultipliers.set(DifficultyLevel.Hard, {
            enemyHealthMultiplier: 1.25,
            enemyDamageMultiplier: 1.25,
            enemyCountMultiplier: 1.25,
            timeLimitMultiplier: 0.9,
            rewardMultiplier: 1.25,
            resourceMultiplier: 0.9
        });

        this.difficultyMultipliers.set(DifficultyLevel.Expert, {
            enemyHealthMultiplier: 1.5,
            enemyDamageMultiplier: 1.5,
            enemyCountMultiplier: 1.5,
            timeLimitMultiplier: 0.8,
            rewardMultiplier: 1.5,
            resourceMultiplier: 0.8
        });

        this.difficultyMultipliers.set(DifficultyLevel.Nightmare, {
            enemyHealthMultiplier: 2.0,
            enemyDamageMultiplier: 2.0,
            enemyCountMultiplier: 2.0,
            timeLimitMultiplier: 0.7,
            rewardMultiplier: 2.0,
            resourceMultiplier: 0.7
        });
    }

    public getMultipliers(difficulty: DifficultyLevel): DifficultyMultiplier {
        return this.difficultyMultipliers.get(difficulty) || this.difficultyMultipliers.get(DifficultyLevel.Normal)!;
    }

    public scaleEnemyHealth(baseHealth: number, difficulty: DifficultyLevel): number {
        const multiplier = this.getMultipliers(difficulty).enemyHealthMultiplier;
        return Math.floor(baseHealth * multiplier);
    }

    public scaleEnemyDamage(baseDamage: number, difficulty: DifficultyLevel): number {
        const multiplier = this.getMultipliers(difficulty).enemyDamageMultiplier;
        return Math.floor(baseDamage * multiplier);
    }

    public scaleEnemyCount(baseCount: number, difficulty: DifficultyLevel): number {
        const multiplier = this.getMultipliers(difficulty).enemyCountMultiplier;
        return Math.floor(baseCount * multiplier);
    }

    public scaleTimeLimit(baseTimeLimit: number, difficulty: DifficultyLevel): number {
        const multiplier = this.getMultipliers(difficulty).timeLimitMultiplier;
        return Math.floor(baseTimeLimit * multiplier);
    }

    public scaleReward(baseReward: number, difficulty: DifficultyLevel): number {
        const multiplier = this.getMultipliers(difficulty).rewardMultiplier;
        return Math.floor(baseReward * multiplier);
    }

    public scaleResources(baseResources: number, difficulty: DifficultyLevel): number {
        const multiplier = this.getMultipliers(difficulty).resourceMultiplier;
        return Math.floor(baseResources * multiplier);
    }

    public scaleMissionConfig(config: MissionConfig, difficulty: DifficultyLevel): MissionConfig {
        const multipliers = this.getMultipliers(difficulty);
        
        const scaledConfig: MissionConfig = {
            ...config,
            difficulty,
            timeLimit: config.timeLimit 
                ? this.scaleTimeLimit(config.timeLimit, difficulty)
                : undefined,
            enemyForces: config.enemyForces.map(force => ({
                ...force,
                unitComposition: force.unitComposition.map(unit => ({
                    ...unit,
                    count: this.scaleEnemyCount(unit.count, difficulty)
                }))
            })),
            rewards: config.rewards.map(reward => ({
                ...reward,
                amount: this.scaleReward(reward.amount, difficulty)
            })),
            bonusRewards: config.bonusRewards.map(reward => ({
                ...reward,
                amount: this.scaleReward(reward.amount, difficulty)
            }))
        };

        return scaledConfig;
    }

    public calculateDifficultyRating(
        playerLevel: number,
        missionRecommendedLevel: number,
        completedMissions: number,
        failedMissions: number
    ): DifficultyLevel {
        const levelDiff = missionRecommendedLevel - playerLevel;
        const winRate = completedMissions / Math.max(1, completedMissions + failedMissions);
        
        let rating: DifficultyLevel;

        if (levelDiff <= -2 && winRate > 0.8) {
            rating = DifficultyLevel.Nightmare;
        } else if (levelDiff <= -1 && winRate > 0.7) {
            rating = DifficultyLevel.Expert;
        } else if (levelDiff <= 0 && winRate > 0.5) {
            rating = DifficultyLevel.Hard;
        } else if (levelDiff <= 2 && winRate > 0.3) {
            rating = DifficultyLevel.Normal;
        } else {
            rating = DifficultyLevel.Easy;
        }

        return rating;
    }

    public getDifficultyName(difficulty: DifficultyLevel): string {
        const names: Record<DifficultyLevel, string> = {
            [DifficultyLevel.Easy]: 'Easy',
            [DifficultyLevel.Normal]: 'Normal',
            [DifficultyLevel.Hard]: 'Hard',
            [DifficultyLevel.Expert]: 'Expert',
            [DifficultyLevel.Nightmare]: 'Nightmare'
        };
        return names[difficulty];
    }

    public getDifficultyDescription(difficulty: DifficultyLevel): string {
        const descriptions: Record<DifficultyLevel, string> = {
            [DifficultyLevel.Easy]: 'Relaxed gameplay with reduced challenges',
            [DifficultyLevel.Normal]: 'Balanced challenge for most players',
            [DifficultyLevel.Hard]: 'Increased difficulty with better rewards',
            [DifficultyLevel.Expert]: 'Challenging gameplay for experienced players',
            [DifficultyLevel.Nightmare]: 'Extreme difficulty for master strategists'
        };
        return descriptions[difficulty];
    }

    public getAllDifficulties(): DifficultyLevel[] {
        return [
            DifficultyLevel.Easy,
            DifficultyLevel.Normal,
            DifficultyLevel.Hard,
            DifficultyLevel.Expert,
            DifficultyLevel.Nightmare
        ];
    }
}

export interface DifficultyMultiplier {
    enemyHealthMultiplier: number;
    enemyDamageMultiplier: number;
    enemyCountMultiplier: number;
    timeLimitMultiplier: number;
    rewardMultiplier: number;
    resourceMultiplier: number;
}

export class MissionRewards {
    private static instance: MissionRewards;
    private rewardHandlers: Map<RewardType, RewardHandler> = new Map();
    private pendingRewards: MissionReward[] = [];
    private totalRewardsGranted: Map<RewardType, number> = new Map();

    private constructor() {
        this.registerDefaultHandlers();
    }

    public static getInstance(): MissionRewards {
        if (!MissionRewards.instance) {
            MissionRewards.instance = new MissionRewards();
        }
        return MissionRewards.instance;
    }

    private registerDefaultHandlers(): void {
        this.registerHandler(RewardType.Credits, {
            type: RewardType.Credits,
            apply: (reward, state) => {
                ResourceManager.getInstance().addCredits(reward.amount);
                return reward.amount;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Experience, {
            type: RewardType.Experience,
            apply: (reward, state) => {
                return reward.amount;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Resources, {
            type: RewardType.Resources,
            apply: (reward, state) => {
                return reward.amount;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Units, {
            type: RewardType.Units,
            apply: (reward, state) => {
                return reward.amount;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Technology, {
            type: RewardType.Technology,
            apply: (reward, state) => {
                return 1;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Unlock, {
            type: RewardType.Unlock,
            apply: (reward, state) => {
                return 1;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Achievement, {
            type: RewardType.Achievement,
            apply: (reward, state) => {
                return 1;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Reputation, {
            type: RewardType.Reputation,
            apply: (reward, state) => {
                return reward.reputationPoints || 0;
            },
            canApply: () => true
        });

        this.registerHandler(RewardType.Special, {
            type: RewardType.Special,
            apply: (reward, state) => {
                return 1;
            },
            canApply: () => true
        });
    }

    public registerHandler(type: RewardType, handler: RewardHandler): void {
        this.rewardHandlers.set(type, handler);
    }

    public grantRewards(
        rewards: MissionReward[],
        missionState: MissionState,
        starRating: StarRating,
        bonusMultiplier: number = 1.0
    ): GrantedRewards {
        const granted: GrantedRewards = {
            credits: 0,
            experience: 0,
            resources: {},
            units: [],
            unlocks: [],
            achievements: [],
            totalValue: 0
        };

        for (const reward of rewards) {
            if (reward.probability !== undefined && Math.random() > reward.probability) {
                continue;
            }

            const handler = this.rewardHandlers.get(reward.type);
            if (!handler || !handler.canApply()) {
                continue;
            }

            const scaledAmount = this.scaleRewardByStarRating(reward.amount, starRating, bonusMultiplier);
            const actualAmount = handler.apply({ ...reward, amount: scaledAmount }, missionState);

            this.recordReward(reward.type, actualAmount);

            switch (reward.type) {
                case RewardType.Credits:
                    granted.credits += actualAmount;
                    break;
                case RewardType.Experience:
                    granted.experience += actualAmount;
                    break;
                case RewardType.Resources:
                    granted.resources[reward.itemId || 'generic'] = actualAmount;
                    break;
                case RewardType.Units:
                    granted.units.push(reward.unitType!);
                    break;
                case RewardType.Unlock:
                    granted.unlocks.push(reward.unlockId!);
                    break;
                case RewardType.Achievement:
                    granted.achievements.push(reward.unlockId!);
                    break;
            }

            granted.totalValue += this.getRewardValue(reward);
        }

        return granted;
    }

    private scaleRewardByStarRating(amount: number, starRating: StarRating, bonusMultiplier: number): number {
        let multiplier = 1.0;
        
        switch (starRating) {
            case StarRating.OneStar:
                multiplier = 1.0;
                break;
            case StarRating.TwoStars:
                multiplier = 1.25;
                break;
            case StarRating.ThreeStars:
                multiplier = 1.5;
                break;
        }

        return Math.floor(amount * multiplier * bonusMultiplier);
    }

    private getRewardValue(reward: MissionReward): number {
        switch (reward.type) {
            case RewardType.Credits:
                return reward.amount;
            case RewardType.Experience:
                return reward.amount;
            case RewardType.Resources:
                return reward.amount * 10;
            case RewardType.Units:
                return reward.amount * 100;
            case RewardType.Technology:
                return 500;
            case RewardType.Unlock:
                return 250;
            case RewardType.Achievement:
                return 100;
            case RewardType.Reputation:
                return reward.reputationPoints || 0;
            default:
                return 50;
        }
    }

    private recordReward(type: RewardType, amount: number): void {
        const current = this.totalRewardsGranted.get(type) || 0;
        this.totalRewardsGranted.set(type, current + amount);
    }

    public queueReward(reward: MissionReward): void {
        this.pendingRewards.push(reward);
    }

    public processPendingRewards(missionState: MissionState, starRating: StarRating): GrantedRewards {
        const rewards = [...this.pendingRewards];
        this.pendingRewards = [];
        return this.grantRewards(rewards, missionState, starRating);
    }

    public clearPendingRewards(): void {
        this.pendingRewards = [];
    }

    public getTotalRewardsGranted(): Map<RewardType, number> {
        return new Map(this.totalRewardsGranted);
    }

    public getRewardsByType(type: RewardType): MissionReward[] {
        const allRewards = MissionLoader.getInstance().getAllMissions()
            .flatMap(m => [...m.rewards, ...m.bonusRewards]);
        return allRewards.filter(r => r.type === type);
    }

    public calculateMissionValue(missionId: string, difficulty: DifficultyLevel): number {
        const mission = MissionLoader.getInstance().getMission(missionId);
        if (!mission) {
            return 0;
        }

        let totalValue = 0;

        for (const reward of mission.rewards) {
            totalValue += this.getRewardValue(reward);
        }

        const scaler = DifficultyScaler.getInstance();
        const multiplier = scaler.getMultipliers(difficulty).rewardMultiplier;
        totalValue *= multiplier;

        return Math.floor(totalValue);
    }
}

export interface RewardHandler {
    type: RewardType;
    apply(reward: MissionReward, state: MissionState): number;
    canApply(): boolean;
}

export interface GrantedRewards {
    credits: number;
    experience: number;
    resources: Record<string, number>;
    units: UnitType[];
    unlocks: string[];
    achievements: string[];
    totalValue: number;
}

export class MissionNarrative {
    private static instance: MissionNarrative;
    private narrativeCache: Map<string, MissionNarrativeData> = new Map();
    private currentNarrative: MissionNarrativeData | null = null;
    private eventQueue: NarrativeEvent[] = [];
    private isPlaying: boolean = false;

    private constructor() {
    }

    public static getInstance(): MissionNarrative {
        if (!MissionNarrative.instance) {
            MissionNarrative.instance = new MissionNarrative();
        }
        return MissionNarrative.instance;
    }

    public loadNarrative(missionConfig: MissionConfig): MissionNarrativeData {
        const narrative: MissionNarrativeData = {
            missionId: missionConfig.id,
            briefing: {
                title: missionConfig.narrative.briefingTitle,
                text: missionConfig.narrative.briefingText,
                audioPath: missionConfig.narrative.voiceOverPaths?.briefing
            },
            debriefing: {
                title: missionConfig.narrative.debriefingTitle,
                text: missionConfig.narrative.debriefingText,
                audioPath: missionConfig.narrative.voiceOverPaths?.debriefing
            },
            dialogEvents: [
                ...(missionConfig.narrative.missionStartDialog || []),
                ...(missionConfig.narrative.missionEndDialog || [])
            ],
            midMissionEvents: missionConfig.narrative.midMissionEvents || []
        };

        this.narrativeCache.set(missionConfig.id, narrative);
        return narrative;
    }

    public startNarrative(missionId: string): void {
        const narrative = this.narrativeCache.get(missionId);
        if (!narrative) {
            return;
        }

        this.currentNarrative = narrative;
        this.isPlaying = true;
    }

    public endNarrative(): void {
        this.currentNarrative = null;
        this.eventQueue = [];
        this.isPlaying = false;
    }

    public showBriefing(): DialogLine[] | null {
        if (!this.currentNarrative) {
            return null;
        }

        return [{
            speakerId: 'commander',
            speakerName: 'Mission Control',
            text: this.currentNarrative.briefing.text
        }];
    }

    public showDebriefing(missionState: MissionState): DialogLine[] | null {
        if (!this.currentNarrative) {
            return null;
        }

        const phase = missionState.getPhase();
        let text = this.currentNarrative.debriefing.text;

        if (phase === MissionPhase.Completed) {
            const stars = missionState.getStarRating();
            text += ` You earned ${stars} star${stars > 1 ? 's' : ''}!`;
        } else if (phase === MissionPhase.Failed) {
            text += ' Better luck next time, Commander.';
        }

        return [{
            speakerId: 'commander',
            speakerName: 'Mission Control',
            text
        }];
    }

    public queueEvent(event: NarrativeEvent): void {
        this.eventQueue.push(event);
    }

    public processEventQueue(): NarrativeEvent | null {
        if (this.eventQueue.length === 0) {
            return null;
        }

        const event = this.eventQueue.shift()!;
        return event;
    }

    public getCurrentNarrative(): MissionNarrativeData | null {
        return this.currentNarrative;
    }

    public isNarrativeActive(): boolean {
        return this.isPlaying;
    }

    public clearCache(): void {
        this.narrativeCache.clear();
    }
}

export interface MissionNarrativeData {
    missionId: string;
    briefing: {
        title: string;
        text: string;
        audioPath?: string;
    };
    debriefing: {
        title: string;
        text: string;
        audioPath?: string;
    };
    dialogEvents: DialogLine[];
    midMissionEvents: NarrativeEvent[];
}

export class StarRatingCalculator {
    private static instance: StarRatingCalculator;
    private ratingConfigs: Map<string, StarRatingConfig> = new Map();

    private constructor() {
        this.initializeDefaultConfigs();
    }

    public static getInstance(): StarRatingCalculator {
        if (!StarRatingCalculator.instance) {
            StarRatingCalculator.instance = new StarRatingCalculator();
        }
        return StarRatingCalculator.instance;
    }

    private initializeDefaultConfigs(): void {
        this.ratingConfigs.set('default', {
            oneStar: 0,
            twoStars: 500,
            threeStars: 1000
        });

        this.ratingConfigs.set('time_based', {
            oneStar: 0,
            twoStars: 750,
            threeStars: 1500
        });

        this.ratingConfigs.set('score_based', {
            oneStar: 0,
            twoStars: 1000,
            threeStars: 2000
        });
    }

    public setRatingConfig(missionId: string, config: StarRatingConfig): void {
        this.ratingConfigs.set(missionId, config);
    }

    public calculateRating(
        missionId: string,
        score: number,
        timeBonus: number,
        objectiveBonus: number,
        casualtyPenalty: number
    ): StarRating {
        const config = this.ratingConfigs.get(missionId) || this.ratingConfigs.get('default')!;
        
        const totalScore = score + timeBonus + objectiveBonus - casualtyPenalty;

        if (totalScore >= config.threeStars) {
            return StarRating.ThreeStars;
        } else if (totalScore >= config.twoStars) {
            return StarRating.TwoStars;
        } else {
            return StarRating.OneStar;
        }
    }

    public getScoreThresholds(missionId: string): StarRatingConfig {
        return this.ratingConfigs.get(missionId) || this.ratingConfigs.get('default')!;
    }

    public getStarDescription(rating: StarRating): string {
        switch (rating) {
            case StarRating.OneStar:
                return 'Bronze - Mission completed';
            case StarRating.TwoStars:
                return 'Silver - Exceeded expectations';
            case StarRating.ThreeStars:
                return 'Gold - Outstanding performance';
            default:
                return 'Unrated';
        }
    }

    public calculateScoreBreakdown(
        missionState: MissionState,
        config: StarRatingConfig
    ): ScoreBreakdown {
        const baseScore = missionState.getScore();
        const completedObjectives = missionState.getCompletedObjectives().length;
        const failedObjectives = missionState.getFailedObjectives().length;
        const casualties = missionState.casualties.length;
        const timeRemaining = missionState.getTimeRemaining();

        const timeBonus = this.calculateTimeBonus(timeRemaining, missionState.getMissionConfig().timeLimit);
        const objectiveBonus = completedObjectives * 100 - failedObjectives * 50;
        const casualtyPenalty = casualties * 25;

        const totalScore = baseScore + timeBonus + objectiveBonus - casualtyPenalty;

        let rating = StarRating.OneStar;
        if (totalScore >= config.threeStars) {
            rating = StarRating.ThreeStars;
        } else if (totalScore >= config.twoStars) {
            rating = StarRating.TwoStars;
        }

        return {
            baseScore,
            timeBonus,
            objectiveBonus,
            casualtyPenalty,
            totalScore,
            starRating: rating,
            thresholds: config
        };
    }

    private calculateTimeBonus(timeRemaining: number, totalTime: number | undefined): number {
        if (!totalTime || totalTime === 0) {
            return 0;
        }

        const percentageRemaining = timeRemaining / totalTime;
        return Math.floor(percentageRemaining * 500);
    }
}

export interface StarRatingConfig {
    oneStar: number;
    twoStars: number;
    threeStars: number;
}

export interface ScoreBreakdown {
    baseScore: number;
    timeBonus: number;
    objectiveBonus: number;
    casualtyPenalty: number;
    totalScore: number;
    starRating: StarRating;
    thresholds: StarRatingConfig;
}

export class BonusObjectiveManager {
    private static instance: BonusObjectiveManager;
    private bonusObjectives: Map<string, BonusObjectiveConfig> = new Map();
    private completedBonus: Set<string> = new Set();
    private failedBonus: Set<string> = new Set();

    private constructor() {
        this.initializeDefaultBonuses();
    }

    public static getInstance(): BonusObjectiveManager {
        if (!BonusObjectiveManager.instance) {
            BonusObjectiveManager.instance = new BonusObjectiveManager();
        }
        return BonusObjectiveManager.instance;
    }

    private initializeDefaultBonuses(): void {
        this.registerBonus({
            id: 'speed_demon',
            name: 'Speed Demon',
            description: 'Complete the mission in under 50% of the time limit',
            reward: {
                type: RewardType.Credits,
                amount: 500
            },
            conditions: [
                { type: 'time_percent', value: 50, operator: 'less_equal' }
            ]
        });

        this.registerBonus({
            id: 'perfect_defense',
            name: 'Perfect Defense',
            description: 'Complete a defense mission without losing any structures',
            reward: {
                type: RewardType.Credits,
                amount: 750
            },
            conditions: [
                { type: 'structures_lost', value: 0, operator: 'equal' }
            ]
        });

        this.registerBonus({
            id: 'no_casualties',
            name: 'No Casualties',
            description: 'Complete the mission without losing any units',
            reward: {
                type: RewardType.Credits,
                amount: 1000
            },
            conditions: [
                { type: 'units_lost', value: 0, operator: 'equal' }
            ]
        });

        this.registerBonus({
            id: 'collector',
            name: 'Resource Collector',
            description: 'Gather at least 2000 resources during the mission',
            reward: {
                type: RewardType.Resources,
                amount: 500
            },
            conditions: [
                { type: 'resources_gathered', value: 2000, operator: 'greater_equal' }
            ]
        });

        this.registerBonus({
            id: 'exterminator',
            name: 'Exterminator',
            description: 'Defeat 100 enemy units',
            reward: {
                type: RewardType.Credits,
                amount: 600
            },
            conditions: [
                { type: 'enemies_killed', value: 100, operator: 'greater_equal' }
            ]
        });
    }

    public registerBonus(bonus: BonusObjectiveConfig): void {
        this.bonusObjectives.set(bonus.id, bonus);
    }

    public unregisterBonus(bonusId: string): void {
        this.bonusObjectives.delete(bonusId);
    }

    public evaluateBonus(
        bonusId: string,
        missionState: MissionState
    ): { completed: boolean; reason?: string } {
        const bonus = this.bonusObjectives.get(bonusId);
        if (!bonus) {
            return { completed: false, reason: 'Bonus not found' };
        }

        if (this.completedBonus.has(bonusId) || this.failedBonus.has(bonusId)) {
            return { completed: this.completedBonus.has(bonusId) };
        }

        for (const condition of bonus.conditions) {
            const actualValue = this.getConditionValue(condition.type, missionState);
            if (!this.evaluateCondition(actualValue, condition.operator, condition.value)) {
                return { completed: false, reason: `Condition not met: ${condition.type}` };
            }
        }

        return { completed: true };
    }

    private getConditionValue(type: string, missionState: MissionState): number {
        switch (type) {
            case 'time_percent':
                const timeLimit = missionState.getMissionConfig().timeLimit;
                if (!timeLimit) return 100;
                return (missionState.getTimeRemaining() / timeLimit) * 100;
            case 'units_lost':
                return missionState.casualties.length;
            case 'structures_lost':
                return 0;
            case 'resources_gathered':
                const resources = missionState.resourcesGathered;
                return Object.values(resources).reduce((a, b) => a + b, 0);
            case 'enemies_killed':
                return missionState.kills.length;
            default:
                return 0;
        }
    }

    private evaluateCondition(
        actual: number,
        operator: string,
        expected: number
    ): boolean {
        switch (operator) {
            case 'equal':
                return actual === expected;
            case 'greater':
                return actual > expected;
            case 'less':
                return actual < expected;
            case 'greater_equal':
                return actual >= expected;
            case 'less_equal':
                return actual <= expected;
            default:
                return false;
        }
    }

    public markBonusCompleted(bonusId: string): void {
        this.completedBonus.add(bonusId);
    }

    public markBonusFailed(bonusId: string): void {
        this.failedBonus.add(bonusId);
    }

    public getCompletedBonuses(): string[] {
        return Array.from(this.completedBonus);
    }

    public getFailedBonuses(): string[] {
        return Array.from(this.failedBonus);
    }

    public getAvailableBonuses(): BonusObjectiveConfig[] {
        return Array.from(this.bonusObjectives.values()).filter(
            b => !this.completedBonus.has(b.id) && !this.failedBonus.has(b.id)
        );
    }

    public reset(): void {
        this.completedBonus.clear();
        this.failedBonus.clear();
    }
}

export class MissionLock {
    private static instance: MissionLock;
    private lockedMissions: Set<string> = new Set();
    private missionDependencies: Map<string, string[]> = new Map();
    private completedMissions: Set<string> = new Set();
    private unlockedMissions: Set<string> = new Set();

    private constructor() {
    }

    public static getInstance(): MissionLock {
        if (!MissionLock.instance) {
            MissionLock.instance = new MissionLock();
        }
        return MissionLock.instance;
    }

    public initializeLocks(missions: MissionConfig[]): void {
        for (const mission of missions) {
            if (mission.prerequisites.length > 0) {
                this.missionDependencies.set(mission.id, mission.prerequisites);
                this.lockedMissions.add(mission.id);
            } else {
                this.unlockedMissions.add(mission.id);
            }
        }
    }

    public isMissionLocked(missionId: string): boolean {
        return this.lockedMissions.has(missionId);
    }

    public isMissionUnlocked(missionId: string): boolean {
        return this.unlockedMissions.has(missionId);
    }

    public arePrerequisitesMet(missionId: string): boolean {
        const prerequisites = this.missionDependencies.get(missionId);
        if (!prerequisites || prerequisites.length === 0) {
            return true;
        }

        return prerequisites.every(prereq => this.completedMissions.has(prereq));
    }

    public completeMission(missionId: string): void {
        this.completedMissions.add(missionId);
        this.unlockDependentMissions(missionId);
    }

    public unlockDependentMissions(completedMissionId: string): void {
        for (const [missionId, dependencies] of this.missionDependencies.entries()) {
            if (dependencies.includes(completedMissionId)) {
                if (this.arePrerequisitesMet(missionId)) {
                    this.unlockedMissions.add(missionId);
                    this.lockedMissions.delete(missionId);
                }
            }
        }
    }

    public lockMission(missionId: string, prerequisites: string[]): void {
        this.lockedMissions.add(missionId);
        this.missionDependencies.set(missionId, prerequisites);
        this.unlockedMissions.delete(missionId);
    }

    public forceUnlockMission(missionId: string): void {
        this.unlockedMissions.add(missionId);
        this.lockedMissions.delete(missionId);
    }

    public forceLockMission(missionId: string): void {
        this.lockedMissions.add(missionId);
        this.unlockedMissions.delete(missionId);
    }

    public getPrerequisites(missionId: string): string[] {
        return this.missionDependencies.get(missionId) || [];
    }

    public getDependentMissions(missionId: string): string[] {
        const dependents: string[] = [];
        for (const [mission, deps] of this.missionDependencies.entries()) {
            if (deps.includes(missionId)) {
                dependents.push(mission);
            }
        }
        return dependents;
    }

    public resetProgress(): void {
        this.completedMissions.clear();
        this.initializeLocks(Array.from(this.missionDependencies.keys()).map(id => {
            return {
                id,
                prerequisites: this.missionDependencies.get(id) || []
            } as any;
        }));
    }

    public getCompletedMissions(): string[] {
        return Array.from(this.completedMissions);
    }

    public getUnlockedMissions(): string[] {
        return Array.from(this.unlockedMissions);
    }

    public getLockedMissions(): string[] {
        return Array.from(this.lockedMissions);
    }

    public loadProgress(completed: string[], unlocked: string[]): void {
        this.completedMissions = new Set(completed);
        this.unlockedMissions = new Set(unlocked);
        
        for (const missionId of this.lockedMissions) {
            if (this.arePrerequisitesMet(missionId)) {
                this.unlockedMissions.add(missionId);
                this.lockedMissions.delete(missionId);
            }
        }
    }

    public getSaveData(): { completed: string[]; unlocked: string[] } {
        return {
            completed: Array.from(this.completedMissions),
            unlocked: Array.from(this.unlockedMissions)
        };
    }
}

export class MissionEditor {
    private static instance: MissionEditor;
    private customMissions: Map<string, CustomMission> = new Map();
    private currentDraft: CustomMission | null = null;

    private constructor() {
    }

    public static getInstance(): MissionEditor {
        if (!MissionEditor.instance) {
            MissionEditor.instance = new MissionEditor();
        }
        return MissionEditor.instance;
    }

    public createNewMission(): CustomMission {
        const id = `custom_${Date.now()}`;
        const mission: CustomMission = {
            id,
            name: 'New Custom Mission',
            description: 'A custom mission created in the editor',
            author: 'Player',
            createdAt: Date.now(),
            modifiedAt: Date.now(),
            config: this.createDefaultConfig(id),
            isValid: false,
            validationErrors: []
        };

        this.currentDraft = mission;
        return mission;
    }

    public editMission(missionId: string): CustomMission | null {
        return this.customMissions.get(missionId) || null;
    }

    private createDefaultConfig(id: string): MissionConfig {
        return {
            id,
            name: 'New Custom Mission',
            description: 'A custom mission',
            type: MissionType.Assault,
            difficulty: DifficultyLevel.Normal,
            mapId: 'map_default',
            mapSize: MapSize.Medium,
            recommendedLevel: 1,
            prerequisites: [],
            rewards: [
                { type: RewardType.Credits, amount: 1000 }
            ],
            bonusRewards: [],
            objectives: [
                {
                    id: `${id}_obj_1`,
                    name: 'Primary Objective',
                    description: 'Complete the main objective',
                    type: ObjectiveType.Primary,
                    targetCount: 1,
                    currentCount: 0,
                    optional: false
                }
            ],
            narrative: {
                briefingTitle: 'Mission Briefing',
                briefingText: 'Mission briefing text',
                debriefingTitle: 'Mission Complete',
                debriefingText: 'Debriefing text'
            },
            enemyForces: [],
            alliedForces: [],
            tags: ['custom']
        };
    }

    public updateDraft(updates: Partial<MissionConfig>): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.config = { ...this.currentDraft.config, ...updates };
        this.currentDraft.modifiedAt = Date.now();
        this.validateDraft();
    }

    public updateName(name: string): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.name = name;
        this.currentDraft.config.name = name;
        this.currentDraft.modifiedAt = Date.now();
    }

    public updateDescription(description: string): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.description = description;
        this.currentDraft.config.description = description;
        this.currentDraft.modifiedAt = Date.now();
    }

    public addObjective(objective: MissionObjective): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.config.objectives.push(objective);
        this.currentDraft.modifiedAt = Date.now();
        this.validateDraft();
    }

    public removeObjective(objectiveId: string): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.config.objectives = this.currentDraft.config.objectives.filter(
            o => o.id !== objectiveId
        );
        this.currentDraft.modifiedAt = Date.now();
        this.validateDraft();
    }

    public updateObjective(objectiveId: string, updates: Partial<MissionObjective>): void {
        if (!this.currentDraft) {
            return;
        }

        const index = this.currentDraft.config.objectives.findIndex(o => o.id === objectiveId);
        if (index !== -1) {
            this.currentDraft.config.objectives[index] = {
                ...this.currentDraft.config.objectives[index],
                ...updates
            };
            this.currentDraft.modifiedAt = Date.now();
            this.validateDraft();
        }
    }

    public addReward(reward: MissionReward): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.config.rewards.push(reward);
        this.currentDraft.modifiedAt = Date.now();
    }

    public removeReward(index: number): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.config.rewards.splice(index, 1);
        this.currentDraft.modifiedAt = Date.now();
    }

    public addEnemyForce(force: ForceConfig): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.config.enemyForces.push(force);
        this.currentDraft.modifiedAt = Date.now();
    }

    public removeEnemyForce(index: number): void {
        if (!this.currentDraft) {
            return;
        }

        this.currentDraft.config.enemyForces.splice(index, 1);
        this.currentDraft.modifiedAt = Date.now();
    }

    private validateDraft(): void {
        if (!this.currentDraft) {
            return;
        }

        const errors: string[] = [];

        if (!this.currentDraft.name || this.currentDraft.name.trim() === '') {
            errors.push('Mission name is required');
        }

        if (!this.currentDraft.config.mapId) {
            errors.push('Map must be selected');
        }

        const primaryObjectives = this.currentDraft.config.objectives.filter(
            o => o.type === ObjectiveType.Primary
        );
        if (primaryObjectives.length === 0) {
            errors.push('At least one primary objective is required');
        }

        if (this.currentDraft.config.rewards.length === 0) {
            errors.push('At least one reward is required');
        }

        this.currentDraft.validationErrors = errors;
        this.currentDraft.isValid = errors.length === 0;
    }

    public saveDraft(): CustomMission | null {
        if (!this.currentDraft || !this.currentDraft.isValid) {
            return null;
        }

        this.validateDraft();
        if (!this.currentDraft.isValid) {
            return null;
        }

        this.customMissions.set(this.currentDraft.id, { ...this.currentDraft });
        const saved = this.currentDraft;
        this.currentDraft = null;
        return saved;
    }

    public discardDraft(): void {
        this.currentDraft = null;
    }

    public getDraft(): CustomMission | null {
        return this.currentDraft;
    }

    public getCustomMission(missionId: string): CustomMission | undefined {
        return this.customMissions.get(missionId);
    }

    public getAllCustomMissions(): CustomMission[] {
        return Array.from(this.customMissions.values());
    }

    public deleteCustomMission(missionId: string): boolean {
        return this.customMissions.delete(missionId);
    }

    public exportMission(missionId: string): string | null {
        const mission = this.customMissions.get(missionId);
        if (!mission) {
            return null;
        }

        return JSON.stringify(mission, null, 2);
    }

    public importMission(jsonString: string): CustomMission | null {
        try {
            const mission = JSON.parse(jsonString) as CustomMission;
            mission.id = `custom_${Date.now()}`;
            mission.createdAt = Date.now();
            mission.modifiedAt = Date.now();
            
            this.validateMission(mission);
            
            if (mission.isValid) {
                this.customMissions.set(mission.id, mission);
                return mission;
            }
        } catch (error) {
            Logger.error('Failed to import mission', error);
        }
        return null;
    }

    private validateMission(mission: CustomMission): void {
        const errors: string[] = [];

        if (!mission.name) {
            errors.push('Mission name is required');
        }

        if (!mission.config) {
            errors.push('Mission configuration is required');
        }

        mission.validationErrors = errors;
        mission.isValid = errors.length === 0;
    }
}

export class MissionSaveManager {
    private static instance: MissionSaveManager;
    private saveManager: GameSaveManager;
    private autoSaveEnabled: boolean = true;
    private autoSaveSlot: string = 'autosave_mission';
    private maxAutoSaves: number = 3;
    private saveHistory: SaveRecord[] = [];

    private constructor() {
        this.saveManager = GameSaveManager.getInstance();
    }

    public static getInstance(): MissionSaveManager {
        if (!MissionSaveManager.instance) {
            MissionSaveManager.instance = new MissionSaveManager();
        }
        return MissionSaveManager.instance;
    }

    public saveMissionState(state: MissionState, slotName?: string): boolean {
        const slot = slotName || `mission_save_${Date.now()}`;
        
        try {
            const saveData: MissionSaveData = {
                version: '1.0.0',
                timestamp: Date.now(),
                missionState: state.getState(),
                campaignProgress: this.getCurrentCampaignProgress(),
                customMissions: MissionEditor.getInstance().getAllCustomMissions()
            };

            this.saveManager.save(slot, saveData);
            this.recordSave(slot, state.getMissionConfig().id);
            
            if (this.autoSaveEnabled && !slotName) {
                this.performAutoSave();
            }

            return true;
        } catch (error) {
            Logger.error('Failed to save mission state', error);
            return false;
        }
    }

    public loadMissionState(slotName: string): MissionState | null {
        try {
            const saveData = this.saveManager.load(slotName) as MissionSaveData | null;
            if (!saveData) {
                return null;
            }

            const missionConfig = MissionLoader.getInstance().getMission(saveData.missionState.missionId);
            if (!missionConfig) {
                Logger.warn(`Mission config not found: ${saveData.missionState.missionId}`);
                return null;
            }

            const state = new MissionState(missionConfig);
            state.loadState(saveData.missionState);
            
            return state;
        } catch (error) {
            Logger.error('Failed to load mission state', error);
            return null;
        }
    }

    public deleteSave(slotName: string): boolean {
        try {
            this.saveManager.delete(slotName);
            this.removeFromHistory(slotName);
            return true;
        } catch (error) {
            Logger.error('Failed to delete save', error);
            return false;
        }
    }

    public getSaveSlots(): SaveRecord[] {
        return this.saveHistory.filter(save => save.slotName.startsWith('mission_save_'));
    }

    public getAutoSaves(): SaveRecord[] {
        return this.saveHistory.filter(save => save.slotName.startsWith('autosave_'));
    }

    private performAutoSave(): void {
        const autoSaves = this.getAutoSaves();
        
        if (autoSaves.length >= this.maxAutoSaves) {
            const oldest = autoSaves.sort((a, b) => a.timestamp - b.timestamp)[0];
            this.deleteSave(oldest.slotName);
        }

        this.saveManager.save(this.autoSaveSlot, {
            version: '1.0.0',
            timestamp: Date.now(),
            isAutoSave: true
        });
    }

    private recordSave(slotName: string, missionId: string): void {
        this.saveHistory.push({
            slotName,
            missionId,
            timestamp: Date.now(),
            playTime: 0
        });
    }

    private removeFromHistory(slotName: string): void {
        this.saveHistory = this.saveHistory.filter(save => save.slotName !== slotName);
    }

    private getCurrentCampaignProgress(): CampaignProgress {
        return {
            campaignId: 'default',
            currentMissionId: '',
            completedMissions: [],
            failedMissions: [],
            totalPlayTime: 0,
            bestScores: {},
            bestStarRatings: {},
            unlockedMissions: [],
            unlockedTech: [],
            unlockedUnits: [],
            achievements: [],
            statistics: {
                totalMissionsCompleted: 0,
                totalMissionsFailed: 0,
                totalUnitsLost: 0,
                totalUnitsBuilt: 0,
                totalEnemiesDefeated: 0,
                totalResourcesGathered: {},
                totalPlayTime: 0
            },
            lastPlayed: Date.now(),
            settings: {
                difficultyPreference: DifficultyLevel.Normal,
                autoSave: true,
                showTutorials: true,
                enableMods: false
            }
        };
    }

    public setAutoSaveEnabled(enabled: boolean): void {
        this.autoSaveEnabled = enabled;
    }

    public setMaxAutoSaves(max: number): void {
        this.maxAutoSaves = Math.max(1, max);
    }
}

export interface SaveRecord {
    slotName: string;
    missionId: string;
    timestamp: number;
    playTime: number;
}

export class CampaignProgressManager {
    private static instance: CampaignProgressManager;
    private progress: CampaignProgress;
    private progressListeners: CampaignProgressListener[] = [];

    private constructor() {
        this.progress = this.createDefaultProgress();
    }

    public static getInstance(): CampaignProgressManager {
        if (!CampaignProgressManager.instance) {
            CampaignProgressManager.instance = new CampaignProgressManager();
        }
        return CampaignProgressManager.instance;
    }

    private createDefaultProgress(): CampaignProgress {
        const missions = MissionLoader.getInstance().getAllMissions();
        const availableMissions = missions.filter(m => m.prerequisites.length === 0);

        return {
            campaignId: 'default',
            currentMissionId: '',
            completedMissions: [],
            failedMissions: [],
            totalPlayTime: 0,
            bestScores: {},
            bestStarRatings: {},
            unlockedMissions: availableMissions.map(m => m.id),
            unlockedTech: [],
            unlockedUnits: [],
            achievements: [],
            statistics: {
                totalMissionsCompleted: 0,
                totalMissionsFailed: 0,
                totalUnitsLost: 0,
                totalUnitsBuilt: 0,
                totalEnemiesDefeated: 0,
                totalResourcesGathered: {},
                totalPlayTime: 0
            },
            lastPlayed: Date.now(),
            settings: {
                difficultyPreference: DifficultyLevel.Normal,
                autoSave: true,
                showTutorials: true,
                enableMods: false
            }
        };
    }

    public initialize(campaignId: string): void {
        this.progress.campaignId = campaignId;
        MissionLock.getInstance().initializeLocks(MissionLoader.getInstance().getAllMissions());
    }

    public startMission(missionId: string): void {
        this.progress.currentMissionId = missionId;
        this.progress.lastPlayed = Date.now();
        this.notifyListeners('mission_started', missionId);
    }

    public completeMission(missionId: string, state: MissionState): void {
        const mission = MissionLoader.getInstance().getMission(missionId);
        if (!mission) {
            return;
        }

        if (!this.progress.completedMissions.includes(missionId)) {
            this.progress.completedMissions.push(missionId);
            this.progress.statistics.totalMissionsCompleted++;
        }

        const currentBest = this.progress.bestScores[missionId] || 0;
        if (state.getScore() > currentBest) {
            this.progress.bestScores[missionId] = state.getScore();
        }

        const currentStars = this.progress.bestStarRatings[missionId] || StarRating.OneStar;
        if (state.getStarRating() > currentStars) {
            this.progress.bestStarRatings[missionId] = state.getStarRating();
        }

        MissionLock.getInstance().completeMission(missionId);
        this.updateUnlockedMissions();

        for (const reward of mission.rewards) {
            if (reward.type === RewardType.Technology && reward.unlockId) {
                this.unlockTechnology(reward.unlockId);
            } else if (reward.type === RewardType.Unlock && reward.unlockId) {
                this.unlockFeature(reward.unlockId);
            }
        }

        this.notifyListeners('mission_completed', missionId);
    }

    public failMission(missionId: string): void {
        if (!this.progress.failedMissions.includes(missionId)) {
            this.progress.failedMissions.push(missionId);
            this.progress.statistics.totalMissionsFailed++;
        }
        this.notifyListeners('mission_failed', missionId);
    }

    public addPlayTime(seconds: number): void {
        this.progress.totalPlayTime += seconds;
        this.progress.statistics.totalPlayTime += seconds;
    }

    public recordUnitLost(): void {
        this.progress.statistics.totalUnitsLost++;
    }

    public recordUnitBuilt(): void {
        this.progress.statistics.totalUnitsBuilt++;
    }

    public recordEnemyDefeated(): void {
        this.progress.statistics.totalEnemiesDefeated++;
    }

    public recordResourcesGathered(type: string, amount: number): void {
        const current = this.progress.statistics.totalResourcesGathered[type] || 0;
        this.progress.statistics.totalResourcesGathered[type] = current + amount;
    }

    public unlockTechnology(techId: string): void {
        if (!this.progress.unlockedTech.includes(techId)) {
            this.progress.unlockedTech.push(techId);
            this.notifyListeners('tech_unlocked', techId);
        }
    }

    public unlockUnit(unitType: UnitType): void {
        const unitId = unitType.toString();
        if (!this.progress.unlockedUnits.includes(unitId)) {
            this.progress.unlockedUnits.push(unitId);
            this.notifyListeners('unit_unlocked', unitId);
        }
    }

    public unlockFeature(featureId: string): void {
        if (!this.progress.unlockedMissions.includes(featureId)) {
            this.progress.unlockedMissions.push(featureId);
            this.notifyListeners('feature_unlocked', featureId);
        }
    }

    public unlockAchievement(achievementId: string): void {
        if (!this.progress.achievements.includes(achievementId)) {
            this.progress.achievements.push(achievementId);
            this.notifyListeners('achievement_unlocked', achievementId);
        }
    }

    private updateUnlockedMissions(): void {
        const allMissions = MissionLoader.getInstance().getAllMissions();
        for (const mission of allMissions) {
            if (MissionLock.getInstance().isMissionUnlocked(mission.id)) {
                if (!this.progress.unlockedMissions.includes(mission.id)) {
                    this.progress.unlockedMissions.push(mission.id);
                }
            }
        }
    }

    public getProgress(): CampaignProgress {
        return { ...this.progress };
    }

    public loadProgress(progress: CampaignProgress): void {
        this.progress = { ...progress };
        MissionLock.getInstance().loadProgress(
            progress.completedMissions,
            progress.unlockedMissions
        );
    }

    public resetProgress(): void {
        this.progress = this.createDefaultProgress();
        MissionLock.getInstance().resetProgress();
        this.notifyListeners('progress_reset', null);
    }

    public isMissionUnlocked(missionId: string): boolean {
        return this.progress.unlockedMissions.includes(missionId);
    }

    public isMissionCompleted(missionId: string): boolean {
        return this.progress.completedMissions.includes(missionId);
    }

    public getBestScore(missionId: string): number {
        return this.progress.bestScores[missionId] || 0;
    }

    public getBestStars(missionId: string): StarRating {
        return this.progress.bestStarRatings[missionId] || StarRating.OneStar;
    }

    public getCompletedCount(): number {
        return this.progress.completedMissions.length;
    }

    public getTotalMissionCount(): number {
        return MissionLoader.getInstance().getAllMissions().length;
    }

    public getCompletionPercentage(): number {
        const total = this.getTotalMissionCount();
        if (total === 0) {
            return 0;
        }
        return (this.getCompletedCount() / total) * 100;
    }

    public addListener(listener: CampaignProgressListener): void {
        this.progressListeners.push(listener);
    }

    public removeListener(listener: CampaignProgressListener): void {
        const index = this.progressListeners.indexOf(listener);
        if (index > -1) {
            this.progressListeners.splice(index, 1);
        }
    }

    private notifyListeners(event: string, data: any): void {
        for (const listener of this.progressListeners) {
            listener.onProgressUpdate(event, data, this.progress);
        }
    }

    public setDifficultyPreference(difficulty: DifficultyLevel): void {
        this.progress.settings.difficultyPreference = difficulty;
    }

    public getDifficultyPreference(): DifficultyLevel {
        return this.progress.settings.difficultyPreference;
    }

    public saveToStorage(): void {
        const data = JSON.stringify(this.progress);
        localStorage.setItem('campaign_progress', data);
    }

    public loadFromStorage(): boolean {
        const data = localStorage.getItem('campaign_progress');
        if (!data) {
            return false;
        }

        try {
            const progress = JSON.parse(data) as CampaignProgress;
            this.loadProgress(progress);
            return true;
        } catch (error) {
            Logger.error('Failed to load campaign progress', error);
            return false;
        }
    }
}

export interface CampaignProgressListener {
    onProgressUpdate(event: string, data: any, progress: CampaignProgress): void;
}

export class MissionSystem {
    private static instance: MissionSystem;
    private missionLoader: MissionLoader;
    private currentState: MissionState | null = null;
    private objectiveTracker: ObjectiveTracker;
    private missionEvents: MissionEvents;
    private difficultyScaler: DifficultyScaler;
    private missionRewards: MissionRewards;
    private missionNarrative: MissionNarrative;
    private bonusObjectiveManager: BonusObjectiveManager;
    private missionLock: MissionLock;
    private missionEditor: MissionEditor;
    private saveManager: MissionSaveManager;
    private progressManager: CampaignProgressManager;
    private isRunning: boolean = false;
    private updateInterval: number = 1000 / 60;
    private lastUpdateTime: number = 0;

    private constructor() {
        this.missionLoader = MissionLoader.getInstance();
        this.objectiveTracker = new ObjectiveTracker();
        this.missionEvents = MissionEvents.getInstance();
        this.difficultyScaler = DifficultyScaler.getInstance();
        this.missionRewards = MissionRewards.getInstance();
        this.missionNarrative = MissionNarrative.getInstance();
        this.bonusObjectiveManager = BonusObjectiveManager.getInstance();
        this.missionLock = MissionLock.getInstance();
        this.missionEditor = MissionEditor.getInstance();
        this.saveManager = MissionSaveManager.getInstance();
        this.progressManager = CampaignProgressManager.getInstance();
    }

    public static getInstance(): MissionSystem {
        if (!MissionSystem.instance) {
            MissionSystem.instance = new MissionSystem();
        }
        return MissionSystem.instance;
    }

    public initialize(): void {
        this.setupEventListeners();
        this.progressManager.initialize('default');
        Logger.info('MissionSystem initialized');
    }

    private setupEventListeners(): void {
        this.missionEvents.registerTrigger(
            MissionTrigger.OnObjectiveComplete,
            null,
            {
                id: 'objective_complete_handler',
                type: 'handle_objective_complete',
                params: {}
            }
        );

        this.missionEvents.registerTrigger(
            MissionTrigger.OnStart,
            null,
            {
                id: 'mission_start_handler',
                type: 'handle_mission_start',
                params: {}
            }
        );
    }

    public async loadMissions(bundlePaths?: string[]): Promise<void> {
        const paths = bundlePaths || ['data/missions/campaign'];
        await this.missionLoader.loadMissions(paths);
    }

    public startMission(missionId: string, difficulty?: DifficultyLevel): boolean {
        const missionConfig = this.missionLoader.getMission(missionId);
        if (!missionConfig) {
            Logger.error(`Mission not found: ${missionId}`);
            return false;
        }

        if (!this.progressManager.isMissionUnlocked(missionId)) {
            Logger.warn(`Mission is locked: ${missionId}`);
            return false;
        }

        let config = missionConfig;
        if (difficulty) {
            config = this.difficultyScaler.scaleMissionConfig(missionConfig, difficulty);
        }

        this.currentState = new MissionState(config);
        this.currentState.setPhase(MissionPhase.Available);
        
        for (const objective of config.objectives) {
            this.objectiveTracker.createObjectiveTracking(objective);
        }

        this.missionNarrative.loadNarrative(config);
        this.missionNarrative.startNarrative(missionId);
        this.progressManager.startMission(missionId);
        
        MissionSaveManager.getInstance().saveMissionState(this.currentState);

        return true;
    }

    public beginMission(): void {
        if (!this.currentState) {
            return;
        }

        this.currentState.start();
        this.isRunning = true;
        this.lastUpdateTime = Date.now();
    }

    public update(): void {
        if (!this.isRunning || !this.currentState) {
            return;
        }

        const currentTime = Date.now();
        const deltaTime = currentTime - this.lastUpdateTime;
        this.lastUpdateTime = currentTime;

        this.currentState.update(deltaTime);
        this.progressManager.addPlayTime(deltaTime / 1000);
    }

    public updateObjective(objectiveId: string, increment: number = 1): void {
        if (!this.currentState) {
            return;
        }

        this.currentState.updateObjectiveProgress(objectiveId, increment);
        this.objectiveTracker.updateProgress(objectiveId, increment);
    }

    public completeMission(): void {
        if (!this.currentState) {
            return;
        }

        this.currentState.completeMission();
        this.missionNarrative.endNarrative();
        this.progressManager.completeMission(this.currentState.getMissionConfig().id, this.currentState);
        
        const rewards = this.grantRewards();
        Logger.info('Mission completed', {
            missionId: this.currentState.getMissionConfig().id,
            starRating: this.currentState.getStarRating(),
            score: this.currentState.getScore(),
            rewards
        });

        this.isRunning = false;
        MissionSaveManager.getInstance().saveMissionState(this.currentState);
    }

    public failMission(reason: string): void {
        if (!this.currentState) {
            return;
        }

        this.currentState.failMission(reason);
        this.missionNarrative.endNarrative();
        this.progressManager.failMission(this.currentState.getMissionConfig().id);
        
        this.isRunning = false;
        Logger.warn('Mission failed', {
            missionId: this.currentState.getMissionConfig().id,
            reason
        });

        MissionSaveManager.getInstance().saveMissionState(this.currentState);
    }

    public pauseMission(): void {
        if (!this.currentState) {
            return;
        }

        this.currentState.pause();
        this.isRunning = false;
    }

    public resumeMission(): void {
        if (!this.currentState) {
            return;
        }

        this.currentState.resume();
        this.isRunning = true;
        this.lastUpdateTime = Date.now();
    }

    public quitMission(): void {
        this.currentState = null;
        this.isRunning = false;
        this.missionNarrative.endNarrative();
        this.objectiveTracker.clear();
    }

    private grantRewards(): GrantedRewards | null {
        if (!this.currentState) {
            return null;
        }

        const config = this.currentState.getMissionConfig();
        const starRating = this.currentState.getStarRating();
        
        const bonusObjectives = this.bonusObjectiveManager.getCompletedBonuses();
        const bonusMultiplier = 1.0 + (bonusObjectives.length * 0.1);

        const baseRewards = this.missionRewards.grantRewards(
            config.rewards,
            this.currentState,
            starRating,
            bonusMultiplier
        );

        const bonusRewards = this.missionRewards.grantRewards(
            config.bonusRewards,
            this.currentState,
            starRating,
            1.0
        );

        return {
            ...baseRewards,
            credits: baseRewards.credits + bonusRewards.credits,
            experience: baseRewards.experience + bonusRewards.experience
        };
    }

    public getCurrentState(): MissionState | null {
        return this.currentState;
    }

    public getMission(missionId: string): MissionConfig | undefined {
        return this.missionLoader.getMission(missionId);
    }

    public getAllMissions(): MissionConfig[] {
        return this.missionLoader.getAllMissions();
    }

    public getAvailableMissions(): MissionConfig[] {
        return this.getAllMissions().filter(m => 
            this.progressManager.isMissionUnlocked(m.id)
        );
    }

    public getCompletedMissions(): MissionConfig[] {
        return this.getAllMissions().filter(m =>
            this.progressManager.isMissionCompleted(m.id)
        );
    }

    public getCurrentProgress(): CampaignProgress {
        return this.progressManager.getProgress();
    }

    public getObjectiveTracker(): ObjectiveTracker {
        return this.objectiveTracker;
    }

    public getMissionEvents(): MissionEvents {
        return this.missionEvents;
    }

    public getDifficultyScaler(): DifficultyScaler {
        return this.difficultyScaler;
    }

    public getMissionRewards(): MissionRewards {
        return this.missionRewards;
    }

    public getMissionNarrative(): MissionNarrative {
        return this.missionNarrative;
    }

    public getBonusObjectiveManager(): BonusObjectiveManager {
        return this.bonusObjectiveManager;
    }

    public getMissionLock(): MissionLock {
        return this.missionLock;
    }

    public getMissionEditor(): MissionEditor {
        return this.missionEditor;
    }

    public getSaveManager(): MissionSaveManager {
        return this.saveManager;
    }

    public getProgressManager(): CampaignProgressManager {
        return this.progressManager;
    }

    public saveGame(slotName?: string): boolean {
        if (!this.currentState) {
            return false;
        }
        return this.saveManager.saveMissionState(this.currentState, slotName);
    }

    public loadGame(slotName: string): boolean {
        const state = this.saveManager.loadMissionState(slotName);
        if (!state) {
            return false;
        }

        this.currentState = state;
        this.missionNarrative.loadNarrative(state.getMissionConfig());
        this.missionNarrative.startNarrative(state.getMissionConfig().id);
        
        for (const objective of state.getMissionConfig().objectives) {
            this.objectiveTracker.createObjectiveTracking(objective);
        }

        if (state.getPhase() === MissionPhase.InProgress) {
            this.beginMission();
        }

        return true;
    }

    public resetCampaign(): void {
        this.progressManager.resetProgress();
        this.missionLock.resetProgress();
        this.currentState = null;
        this.isRunning = false;
        this.missionNarrative.endNarrative();
        this.objectiveTracker.clear();
    }
}

export class Operation {
    constructor(config = {}) {
        this.id = config.id || Operation.nextId++;
        this.name = config.name || 'Operation';
        this.description = config.description || '';
        this.imageUrl = config.imageUrl || '';
        this.startDate = config.startDate || new Date();
        this.endDate = config.endDate || new Date(Date.now() + 30 * 24 * 60 * 60 * 1000);
        this.status = this.calculateStatus();
        this.tiers = config.tiers || this.generateDefaultTiers();
        this.currentTier = 0;
        this.missions = config.missions || [];
        this.rewards = config.rewards || [];
        this.requiredLevel = config.requiredLevel || 1;
        this.cost = config.cost || 0;
        this.type = config.type || 'standard';
        this.maps = config.maps || [];
        this.bonuses = config.bonuses || [];
        this.statRequirements = config.statRequirements || {};
        this.isPremium = config.isPremium || false;
        this.purchaseCount = config.purchaseCount || 0;
        this.participants = config.participants || 0;
        this.completedCount = config.completedCount || 0;
    }

    static get nextId() {
        return Operation._nextId++;
    }

    calculateStatus() {
        const now = new Date();
        if (now < this.startDate) return 'upcoming';
        if (now > this.endDate) return 'ended';
        return 'active';
    }

    generateDefaultTiers() {
        return [
            { tier: 1, xpRequired: 0, rewards: [] },
            { tier: 2, xpRequired: 1000, rewards: [] },
            { tier: 3, xpRequired: 2500, rewards: [] },
            { tier: 4, xpRequired: 5000, rewards: [] },
            { tier: 5, xpRequired: 7500, rewards: [] },
            { tier: 6, xpRequired: 10000, rewards: [] }
        ];
    }

    addMission(mission) {
        this.missions.push(mission);
        return this;
    }

    addReward(reward) {
        this.rewards.push(reward);
        return this;
    }

    getProgress(xpEarned) {
        let currentTierXP = 0;
        let nextTierXP = this.tiers[this.currentTier + 1]?.xpRequired || this.tiers[this.currentTier].xpRequired;

        for (let i = 0; i <= this.currentTier; i++) {
            if (this.tiers[i].xpRequired <= xpEarned) {
                currentTierXP = this.tiers[i].xpRequired;
            }
        }

        for (let i = this.currentTier + 1; i < this.tiers.length; i++) {
            if (this.tiers[i].xpRequired <= xpEarned) {
                nextTierXP = this.tiers[i].xpRequired;
                this.currentTier = i;
            }
        }

        const progress = (xpEarned - currentTierXP) / (nextTierXP - currentTierXP);

        return {
            currentTier: this.currentTier,
            totalTiers: this.tiers.length,
            xpEarned,
            xpToNextTier: nextTierXP - xpEarned,
            progress: Math.min(1, Math.max(0, progress)),
            isComplete: this.currentTier >= this.tiers.length - 1
        };
    }

    getTimeRemaining() {
        const now = new Date();
        const diff = this.endDate - now;

        if (diff <= 0) {
            return { expired: true, days: 0, hours: 0, minutes: 0 };
        }

        const days = Math.floor(diff / (24 * 60 * 60 * 1000));
        const hours = Math.floor((diff % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000));
        const minutes = Math.floor((diff % (60 * 60 * 1000)) / (60 * 1000));

        return {
            expired: false,
            days,
            hours,
            minutes,
            totalMs: diff
        };
    }

    canParticipate(playerLevel, isPremium = false) {
        if (playerLevel < this.requiredLevel) {
            return { allowed: false, reason: 'level_required' };
        }
        if (this.isPremium && !isPremium) {
            return { allowed: false, reason: 'premium_required' };
        }
        if (this.status !== 'active') {
            return { allowed: false, reason: 'operation_not_active' };
        }
        return { allowed: true };
    }

    claimReward(tier) {
        const tierData = this.tiers[tier];
        if (!tierData) {
            return { success: false, reason: 'invalid_tier' };
        }
        return { success: true, rewards: tierData.rewards };
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            description: this.description,
            startDate: this.startDate,
            endDate: this.endDate,
            status: this.status,
            tiers: this.tiers,
            currentTier: this.currentTier,
            missions: this.missions,
            rewards: this.rewards,
            requiredLevel: this.requiredLevel,
            cost: this.cost,
            type: this.type,
            maps: this.maps,
            bonuses: this.bonuses,
            isPremium: this.isPremium,
            purchaseCount: this.purchaseCount,
            participants: this.participants,
            completedCount: this.completedCount
        };
    }

    static fromJSON(json) {
        const operation = new Operation(json);
        return operation;
    }
}

Operation._nextId = 0;

export class Mission {
    constructor(config = {}) {
        this.id = config.id || Mission.nextId++;
        this.name = config.name || 'Mission';
        this.description = config.description || '';
        this.category = config.category || 'combat';
        this.type = config.type || 'default';
        this.difficulty = config.difficulty || 'normal';
        this.rewards = config.rewards || [];
        this.requirements = config.requirements || {};
        this.map = config.map || null;
        this.gameMode = config.gameMode || 'competitive';
        this.isPrimary = config.isPrimary || false;
        this.isSideMission = config.isSideMission || false;
        this.expiresAt = config.expiresAt || null;
        this.isExpired = false;
        this.isComplete = false;
        this.isNew = true;
        this.timesCompleted = config.timesCompleted || 0;
        this.bestTime = config.bestTime || null;
        this.statTracking = config.statTracking || {};
        this.bonuses = config.bonuses || [];
        this.variables = config.variables || {};
        this.progress = 0;
        this.maxProgress = 100;
        this.conditions = config.conditions || [];
        this.successConditions = config.successConditions || [];
        this.failureConditions = config.failureConditions || [];
        this.hints = config.hints || [];
    }

    static get nextId() {
        return Mission._nextId++;
    }

    static get CATEGORIES() {
        return [
            'combat', 'objective', 'support', 'utility', 'survival',
            'speed', 'precision', 'teamwork', 'stealth', 'gadget'
        ];
    }

    static get TYPES() {
        return [
            'elimination', 'objective', 'survival', 'speedrun',
            'precision', 'utility', 'team', 'guardian', 'co-op', 'commendation'
        ];
    }

    static get DIFFICULTIES() {
        return {
            easy: { multiplier: 1, xp: 100 },
            normal: { multiplier: 1.5, xp: 200 },
            hard: { multiplier: 2, xp: 350 },
            expert: { multiplier: 3, xp: 500 },
            master: { multiplier: 5, xp: 1000 }
        };
    }

    checkConditions(matchData) {
        for (const condition of this.conditions) {
            if (!this.evaluateCondition(condition, matchData)) {
                return { success: false, failedCondition: condition };
            }
        }
        return { success: true };
    }

    evaluateCondition(condition, matchData) {
        switch (condition.type) {
            case 'kills':
                return (matchData.kills || 0) >= condition.value;
            case 'headshots':
                return (matchData.headshots || 0) >= condition.value;
            case 'assists':
                return (matchData.assists || 0) >= condition.value;
            case 'damage':
                return (matchData.damage || 0) >= condition.value;
            case 'win':
                return matchData.win === true;
            case 'map':
                return matchData.map === condition.value;
            case 'gameMode':
                return matchData.gameMode === condition.value;
            case 'roundWins':
                return (matchData.roundWins || 0) >= condition.value;
            case 'clutch':
                return (matchData.clutches || 0) >= condition.value;
            case 'mvp':
                return (matchData.mvps || 0) >= condition.value;
            case 'noDeathRounds':
                return (matchData.noDeathRounds || 0) >= condition.value;
            case 'awpKills':
                return (matchData.awpKills || 0) >= condition.value;
            case 'knifeKills':
                return (matchData.knifeKills || 0) >= condition.value;
            case 'grenadeKills':
                return (matchData.grenadeKills || 0) >= condition.value;
            case 'tradeKills':
                return (matchData.tradeKills || 0) >= condition.value;
            default:
                return true;
        }
    }

    updateProgress(matchData) {
        if (this.isComplete || this.isExpired) return this.progress;

        this.checkExpiration();

        if (this.isExpired) {
            return 0;
        }

        const progressUpdate = this.calculateProgress(matchData);
        this.progress = Math.min(this.maxProgress, this.progress + progressUpdate);

        if (this.progress >= this.maxProgress) {
            this.complete();
        }

        return this.progress;
    }

    calculateProgress(matchData) {
        const baseProgress = 10;
        const difficultyData = Mission.DIFFICULTIES[this.difficulty];

        let multiplier = difficultyData.multiplier;

        if (matchData.win) multiplier *= 1.5;
        if (matchData.mvp) multiplier *= 1.25;
        if (matchData.clutch) multiplier *= 1.5;

        return Math.floor(baseProgress * multiplier);
    }

    complete() {
        this.isComplete = true;
        this.completedAt = Date.now();
        return {
            success: true,
            rewards: this.rewards,
            timeSpent: this.calculateTimeSpent()
        };
    }

    calculateTimeSpent() {
        if (!this.startedAt) return 0;
        return Date.now() - this.startedAt;
    }

    checkExpiration() {
        if (this.expiresAt && Date.now() > this.expiresAt) {
            this.isExpired = true;
        }
    }

    start() {
        this.startedAt = Date.now();
        this.isNew = false;
    }

    abandon() {
        this.progress = 0;
        this.startedAt = null;
        this.isComplete = false;
    }

    getTimeRemaining() {
        if (!this.expiresAt) return null;

        const diff = this.expiresAt - Date.now();
        if (diff <= 0) {
            this.isExpired = true;
            return { expired: true, days: 0, hours: 0, minutes: 0 };
        }

        const days = Math.floor(diff / (24 * 60 * 60 * 1000));
        const hours = Math.floor((diff % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000));
        const minutes = Math.floor((diff % (60 * 60 * 1000)) / (60 * 1000));

        return { expired: false, days, hours, minutes, totalMs: diff };
    }

    getRewardSummary() {
        return this.rewards.map(reward => ({
            type: reward.type,
            name: reward.name,
            value: reward.value || reward.amount || 1
        }));
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            description: this.description,
            category: this.category,
            type: this.type,
            difficulty: this.difficulty,
            rewards: this.rewards,
            requirements: this.requirements,
            map: this.map,
            gameMode: this.gameMode,
            isPrimary: this.isPrimary,
            isSideMission: this.isSideMission,
            expiresAt: this.expiresAt,
            isExpired: this.isExpired,
            isComplete: this.isComplete,
            isNew: this.isNew,
            timesCompleted: this.timesCompleted,
            bestTime: this.bestTime,
            statTracking: this.statTracking,
            progress: this.progress,
            maxProgress: this.maxProgress,
            startedAt: this.startedAt
        };
    }

    static fromJSON(json) {
        const mission = new Mission(json);
        return mission;
    }
}

Mission._nextId = 0;

export class MissionGenerator {
    constructor(config = {}) {
        this.difficulty = config.difficulty || 'normal';
        this.playerLevel = config.playerLevel || 1;
        this.maps = config.maps || ['dust2', 'mirage', 'inferno', 'nuke', 'overpass', 'train', 'cache', 'cobblestone'];
        this.gameModes = config.gameModes || ['competitive', 'casual', 'deathmatch', 'armsRace'];
        this.categoryPool = Mission.CATEGORIES;
        this.typePool = Mission.TYPES;
        this.difficultyPool = Object.keys(Mission.DIFFICULTIES);
        this.generatedMissions = [];
        this.history = [];
    }

    generateDailyMissions(count = 3) {
        const missions = [];

        for (let i = 0; i < count; i++) {
            const mission = this.generateMission({
                difficulty: this.getDailyDifficulty(),
                category: this.getRandomCategory(),
                isPrimary: i === 0
            });
            missions.push(mission);
        }

        this.generatedMissions = missions;
        return missions;
    }

    generateWeeklyMissions(count = 7) {
        const missions = [];

        for (let i = 0; i < count; i++) {
            const mission = this.generateMission({
                difficulty: this.getWeeklyDifficulty(),
                category: this.categoryPool[i % this.categoryPool.length],
                expiresIn: 7 * 24 * 60 * 60 * 1000
            });
            missions.push(mission);
        }

        return missions;
    }

    generateOperationMissions(operationConfig) {
        const missions = [];
        const tiers = operationConfig.tiers || 5;
        const missionsPerTier = 5;

        for (let tier = 1; tier <= tiers; tier++) {
            for (let i = 0; i < missionsPerTier; i++) {
                const mission = this.generateMission({
                    difficulty: this.getDifficultyForTier(tier),
                    tier,
                    category: this.categoryPool[i % this.categoryPool.length],
                    isOperation: true
                });
                missions.push(mission);
            }
        }

        return missions;
    }

    generateMission(config = {}) {
        const difficulty = config.difficulty || this.getRandomDifficulty();
        const category = config.category || this.getRandomCategory();
        const type = config.type || this.getRandomType(category);

        const missionData = this.getMissionTemplate(category, type, difficulty);

        const mission = new Mission({
            name: missionData.name,
            description: missionData.description,
            category,
            type,
            difficulty,
            rewards: this.calculateRewards(difficulty),
            requirements: missionData.requirements,
            map: config.map || this.getRandomMap(),
            gameMode: config.gameMode || this.getRandomGameMode(),
            isPrimary: config.isPrimary || false,
            isSideMission: config.isSideMission || false,
            expiresAt: config.expiresIn ? Date.now() + config.expiresIn : null,
            tier: config.tier,
            conditions: missionData.conditions,
            bonuses: missionData.bonuses,
            variables: missionData.variables
        });

        this.history.push(mission);
        return mission;
    }

    getMissionTemplate(category, type, difficulty) {
        const templates = {
            combat: {
                elimination: {
                    name: 'Elimination Specialist',
                    description: 'Eliminate {value} enemies in a match',
                    requirements: { kills: 10 },
                    conditions: [
                        { type: 'kills', value: 10 }
                    ],
                    bonuses: [
                        { type: 'headshot', bonus: 25 },
                        { type: 'noDeath', bonus: 50 }
                    ]
                },
                headshot: {
                    name: 'Precision Hunter',
                    description: 'Get {value} headshot eliminations',
                    requirements: { headshots: 5 },
                    conditions: [
                        { type: 'headshots', value: 5 }
                    ],
                    bonuses: [
                        { type: 'pistolOnly', bonus: 50 }
                    ]
                }
            },
            objective: {
                bomb: {
                    name: 'Bomb Specialist',
                    description: 'Plant or defuse the bomb {value} times',
                    requirements: { bombActions: 3 },
                    conditions: [
                        { type: 'bombPlants', value: 2 },
                        { type: 'bombDefuses', value: 1 }
                    ]
                },
                hostage: {
                    name: 'Hostage Rescuer',
                    description: 'Rescue {value} hostages',
                    requirements: { rescues: 2 },
                    conditions: [
                        { type: 'rescues', value: 2 }
                    ]
                }
            },
            utility: {
                grenade: {
                    name: 'Grenade Expert',
                    description: 'Eliminate {value} enemies with grenades',
                    requirements: { grenadeKills: 3 },
                    conditions: [
                        { type: 'grenadeKills', value: 3 }
                    ]
                },
                flashbang: {
                    name: 'Flash Master',
                    description: 'Flash {value} enemies for your team',
                    requirements: { flashes: 10 },
                    conditions: [
                        { type: 'flashAssists', value: 10 }
                    ]
                }
            },
            precision: {
                awp: {
                    name: 'AWP Pro',
                    description: 'Get {value} AWP eliminations',
                    requirements: { awpKills: 5 },
                    conditions: [
                        { type: 'awpKills', value: 5 }
                    ],
                    variables: { weapon: 'awp' }
                },
                knife: {
                    name: 'Silent Killer',
                    description: 'Get {value} knife eliminations',
                    requirements: { knifeKills: 2 },
                    conditions: [
                        { type: 'knifeKills', value: 2 }
                    ]
                }
            }
        };

        return templates[category]?.[type] || {
            name: `${category} ${type}`,
            description: `Complete ${category} ${type} objective`,
            conditions: [{ type: 'kills', value: 10 }]
        };
    }

    calculateRewards(difficulty) {
        const baseRewards = {
            easy: { xp: 100, coins: 10 },
            normal: { xp: 200, coins: 20 },
            hard: { xp: 350, coins: 35 },
            expert: { xp: 500, coins: 50 },
            master: { xp: 1000, coins: 100 }
        };

        const base = baseRewards[difficulty];
        const scale = 1 + (this.playerLevel - 1) * 0.01;

        return [
            { type: 'xp', name: 'Experience', value: Math.floor(base.xp * scale) },
            { type: 'coins', name: 'Operation Coins', value: base.coins }
        ];
    }

    getRandomCategory() {
        return this.categoryPool[Math.floor(Math.random() * this.categoryPool.length)];
    }

    getRandomType(category) {
        return this.typePool[Math.floor(Math.random() * this.typePool.length)];
    }

    getRandomDifficulty() {
        const weights = {
            easy: 0.2,
            normal: 0.4,
            hard: 0.25,
            expert: 0.1,
            master: 0.05
        };

        return this.weightedRandom(weights);
    }

    getDailyDifficulty() {
        return Math.random() < 0.7 ? 'normal' : 'hard';
    }

    getWeeklyDifficulty() {
        return Math.random() < 0.5 ? 'hard' : 'expert';
    }

    getDifficultyForTier(tier) {
        const difficulties = ['easy', 'normal', 'normal', 'hard', 'hard', 'expert'];
        return difficulties[tier - 1] || 'normal';
    }

    getRandomMap() {
        return this.maps[Math.floor(Math.random() * this.maps.length)];
    }

    getRandomGameMode() {
        return this.gameModes[Math.floor(Math.random() * this.gameModes.length)];
    }

    weightedRandom(weights) {
        const total = Object.values(weights).reduce((sum, w) => sum + w, 0);
        let random = Math.random() * total;

        for (const [key, weight] of Object.entries(weights)) {
            random -= weight;
            if (random <= 0) {
                return key;
            }
        }

        return Object.keys(weights)[0];
    }
}

export class Tournament {
    constructor(config = {}) {
        this.id = config.id || Tournament.nextId++;
        this.name = config.name || 'Tournament';
        this.organizer = config.organizer || '';
        this.description = config.description || '';
        this.type = config.type || 'single_elimination';
        this.format = config.format || 'bo1';
        this.region = config.region || 'global';
        this.prizePool = config.prizePool || 0;
        this.prizeDistribution = config.prizeDistribution || this.defaultPrizeDistribution();
        this.teams = [];
        this.brackets = [];
        this.currentRound = 0;
        this.maxRounds = 0;
        this.registrationOpen = config.registrationOpen !== undefined ? config.registrationOpen : true;
        this.registrationDeadline = config.registrationDeadline || null;
        this.startDate = config.startDate || new Date();
        this.endDate = config.endDate || null;
        this.status = 'planned';
        this.capacity = config.capacity || 16;
        this.minTeamSize = config.minTeamSize || 5;
        this.maxTeamSize = config.maxTeamSize || 5;
        this.allowSubstitutes = config.allowSubstitutes || false;
        this.maxSubstitutes = config.maxSubstitutes || 2;
        this.inviteOnly = config.inviteOnly || false;
        this.invites = config.invites || [];
        this.spectatorSettings = config.spectatorSettings || {
            allowSpectators: true,
            maxSpectators: 100,
            password: null
        };
        this.broadcastSettings = config.broadcastSettings || {
            streamUrl: null,
            twitchChannel: null,
            youtubeChannel: null
        };
        this.rules = config.rules || this.defaultRules();
        this.maps = config.maps || ['dust2', 'mirage', 'inferno'];
        this.vetoSettings = config.vetoSettings || {
            enabled: true,
            type: 'standard',
            bansPerTeam: 2,
            picksPerTeam: 1
        };
        this.metadata = config.metadata || {};
    }

    static get nextId() {
        return Tournament._nextId++;
    }

    static get TYPES() {
        return ['single_elimination', 'double_elimination', 'round_robin', 'swiss'];
    }

    static get FORMATS() {
        return {
            'bo1': { name: 'Best of 1', games: 1 },
            'bo3': { name: 'Best of 3', games: 3 },
            'bo5': { name: 'Best of 5', games: 5 },
            'bo7': { name: 'Best of 7', games: 7 }
        };
    }

    defaultPrizeDistribution() {
        return [
            { place: 1, percentage: 50 },
            { place: 2, percentage: 25 },
            { place: 3, percentage: 15 },
            { place: 4, percentage: 10 }
        ];
    }

    defaultRules() {
        return {
            maxRoundTime: 115,
            overtimeTime: 40,
            overtimeEnabled: true,
            maxOvertimes: 6,
            mapPool: ['dust2', 'mirage', 'inferno', 'nuke', 'overpass', 'train', 'cache', 'cobblestone'],
            vetoEnabled: true,
            knifeRound: true,
            pauseAllowed: true,
            maxPauses: 4,
            techTimeoutAllowed: true
        };
    }

    registerTeam(team) {
        if (this.teams.length >= this.capacity) {
            return { success: false, reason: 'tournament_full' };
        }

        if (team.players.length < this.minTeamSize) {
            return { success: false, reason: 'team_too_small' };
        }

        if (team.players.length > this.maxTeamSize + this.maxSubstitutes) {
            return { success: false, reason: 'team_too_large' };
        }

        this.teams.push(team);
        return { success: true, team };
    }

    unregisterTeam(teamId) {
        const index = this.teams.findIndex(t => t.id === teamId);
        if (index === -1) {
            return { success: false, reason: 'team_not_found' };
        }

        this.teams.splice(index, 1);
        return { success: true };
    }

    generateBrackets() {
        this.teams.sort(() => Math.random() - 0.5);

        switch (this.type) {
            case 'single_elimination':
                this.generateSingleEliminationBracket();
                break;
            case 'double_elimination':
                this.generateDoubleEliminationBracket();
                break;
            case 'round_robin':
                this.generateRoundRobinBracket();
                break;
            case 'swiss':
                this.generateSwissBracket();
                break;
        }
    }

    generateSingleEliminationBracket() {
        const numTeams = this.teams.length;
        const numRounds = Math.ceil(Math.log2(numTeams));
        this.maxRounds = numRounds;
        this.currentRound = 0;

        const numSlots = Math.pow(2, numRounds);
        const byes = numSlots - numTeams;

        this.brackets = [];

        for (let round = 1; round <= numRounds; round++) {
            const matchesInRound = numSlots / Math.pow(2, round);
            const roundMatches = [];

            for (let i = 0; i < matchesInRound; i++) {
                roundMatches.push({
                    round,
                    matchNumber: i + 1,
                    team1: null,
                    team2: null,
                    winner: null,
                    loser: null,
                    score: { team1: 0, team2: 0 },
                    maps: [],
                    scheduledTime: null,
                    status: 'pending'
                });
            }

            this.brackets.push(roundMatches);
        }

        this.seedTeams(this.brackets[0], byes);
    }

    generateDoubleEliminationBracket() {
        this.generateSingleEliminationBracket();

        const losersRounds = this.maxRounds - 1;
        const losersBracket = [];

        for (let round = 1; round <= losersRounds; round++) {
            const matchesInRound = Math.pow(2, losersRounds - round);
            const roundMatches = [];

            for (let i = 0; i < matchesInRound; i++) {
                roundMatches.push({
                    round,
                    bracket: 'losers',
                    matchNumber: i + 1,
                    team1: null,
                    team2: null,
                    winner: null,
                    loser: null,
                    status: 'pending'
                });
            }

            losersBracket.push(roundMatches);
        }

        this.brackets.push(...losersBracket);
    }

    generateRoundRobinBracket() {
        const numRounds = this.teams.length - 1;
        const matchesPerRound = Math.floor(this.teams.length / 2);
        this.maxRounds = numRounds;

        this.brackets = [];

        for (let round = 1; round <= numRounds; round++) {
            const roundMatches = [];

            for (let i = 0; i < matchesPerRound; i++) {
                roundMatches.push({
                    round,
                    matchNumber: i + 1,
                    team1: null,
                    team2: null,
                    winner: null,
                    status: 'pending'
                });
            }

            this.brackets.push(roundMatches);
        }

        this.seedRoundRobin(this.brackets);
    }

    generateSwissBracket() {
        const numRounds = Math.ceil(Math.log2(this.teams.length)) + 2;
        this.maxRounds = numRounds;

        this.brackets = [];

        for (let round = 1; round <= numRounds; round++) {
            const roundMatches = [];

            for (let i = 0; i < Math.floor(this.teams.length / 2); i++) {
                roundMatches.push({
                    round,
                    matchNumber: i + 1,
                    team1: null,
                    team2: null,
                    winner: null,
                    status: 'pending',
                    pairings: 'swiss'
                });
            }

            this.brackets.push(roundMatches);
        }
    }

    seedTeams(firstRound, byes) {
        let teamIndex = 0;
        for (let i = 0; i < firstRound.length; i++) {
            if (i >= byes && teamIndex < this.teams.length) {
                firstRound[i].team1 = this.teams[teamIndex++];
            }
            if (i + byes < firstRound.length && teamIndex < this.teams.length) {
                if (firstRound[i].team1 === null) {
                    firstRound[i].team1 = this.teams[teamIndex++];
                } else {
                    firstRound[i].team2 = this.teams[teamIndex++];
                }
            }
        }
    }

    seedRoundRobin(brackets) {
        const teams = [...this.teams];
        const n = teams.length;

        for (let round = 0; round < n - 1; round++) {
            for (let match = 0; match < n / 2; match++) {
                if (match === 0) {
                    brackets[round][match].team1 = teams[0];
                    brackets[round][match].team2 = teams[n - 1];
                } else {
                    brackets[round][match].team1 = teams[match];
                    brackets[round][match].team2 = teams[n - 1 - match];
                }
            }

            teams.splice(1, 0, teams.pop());
        }
    }

    getMatch(round, matchNumber) {
        const bracketRound = this.brackets[round - 1];
        if (!bracketRound) return null;
        return bracketRound.find(m => m.matchNumber === matchNumber);
    }

    setMatchResult(round, matchNumber, winner, score) {
        const match = this.getMatch(round, matchNumber);
        if (!match) return { success: false, reason: 'match_not_found' };

        match.winner = winner;
        match.loser = match.team1 === winner ? match.team2 : match.team1;
        match.score = score;
        match.status = 'completed';

        const nextRound = this.brackets[round];
        if (nextRound) {
            const nextMatchIndex = Math.floor((matchNumber - 1) / 2);
            const nextMatch = nextRound[nextMatchIndex];

            if (nextMatch) {
                if (matchNumber % 2 === 1) {
                    nextMatch.team1 = winner;
                } else {
                    nextMatch.team2 = winner;
                }
            }
        }

        return { success: true, match };
    }

    getWinner() {
        if (this.status !== 'completed') return null;
        const final = this.brackets[this.brackets.length - 1];
        if (!final) return null;

        const finalMatch = final.find(m => m.bracket !== 'losers');
        return finalMatch?.winner;
    }

    getPrizeForPlace(place) {
        const distribution = this.prizeDistribution.find(p => p.place === place);
        if (!distribution) return 0;
        return Math.floor(this.prizePool * distribution.percentage / 100);
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            organizer: this.organizer,
            type: this.type,
            format: this.format,
            region: this.region,
            prizePool: this.prizePool,
            prizeDistribution: this.prizeDistribution,
            teams: this.teams,
            brackets: this.brackets,
            currentRound: this.currentRound,
            status: this.status,
            capacity: this.capacity,
            startDate: this.startDate,
            endDate: this.endDate,
            rules: this.rules,
            maps: this.maps,
            vetoSettings: this.vetoSettings
        };
    }

    static fromJSON(json) {
        const tournament = new Tournament(json);
        return tournament;
    }
}

Tournament._nextId = 0;

export class MissionService {
    constructor() {
        this.missions = new Map();
        this.dailyMissions = [];
        this.weeklyMissions = [];
        this.operationMissions = new Map();
        this.completedMissions = new Map();
        this.playerProgress = new Map();
        this.generators = new Map();
        this.onMissionComplete = null;
        this.onProgressUpdate = null;
        this.initialize();
    }

    initialize() {
        const dailyGenerator = new MissionGenerator();
        const weeklyGenerator = new MissionGenerator();

        this.generators.set('daily', dailyGenerator);
        this.generators.set('weekly', weeklyGenerator);

        this.refreshDailyMissions();
        this.refreshWeeklyMissions();
    }

    refreshDailyMissions() {
        const generator = this.generators.get('daily');
        this.dailyMissions = generator.generateDailyMissions(3);
        this.saveMissionsToStorage('daily');
    }

    refreshWeeklyMissions() {
        const generator = this.generators.get('weekly');
        this.weeklyMissions = generator.generateWeeklyMissions(7);
        this.saveMissionsToStorage('weekly');
    }

    generateOperationMissions(operation) {
        const generator = new MissionGenerator({
            playerLevel: operation.requiredLevel
        });

        const missions = generator.generateOperationMissions({
            tiers: operation.tiers.length
        });

        this.operationMissions.set(operation.id, missions);
        return missions;
    }

    addMission(mission) {
        this.missions.set(mission.id, mission);
        return mission;
    }

    getMission(missionId) {
        return this.missions.get(missionId);
    }

    getActiveMissions() {
        return Array.from(this.missions.values()).filter(m => !m.isComplete && !m.isExpired);
    }

    getDailyMissions() {
        this.checkMissionExpiration(this.dailyMissions);
        return this.dailyMissions.filter(m => !m.isExpired);
    }

    getWeeklyMissions() {
        this.checkMissionExpiration(this.weeklyMissions);
        return this.weeklyMissions.filter(m => !m.isExpired);
    }

    getOperationMissions(operationId) {
        const missions = this.operationMissions.get(operationId);
        if (!missions) return [];
        this.checkMissionExpiration(missions);
        return missions.filter(m => !m.isExpired);
    }

    checkMissionExpiration(missions) {
        for (const mission of missions) {
            mission.checkExpiration();
        }
    }

    updateMissionProgress(missionId, matchData) {
        const mission = this.missions.get(missionId);
        if (!mission) return null;

        const oldProgress = mission.progress;
        mission.updateProgress(matchData);

        const result = {
            mission,
            progressChanged: mission.progress !== oldProgress,
            newProgress: mission.progress,
            isComplete: mission.isComplete
        };

        if (mission.isComplete) {
            this.completeMission(mission);
        }

        this.saveProgressToStorage(missionId);

        if (this.onProgressUpdate) {
            this.onProgressUpdate(result);
        }

        return result;
    }

    completeMission(mission) {
        mission.complete();
        this.completedMissions.set(mission.id, {
            ...mission.toJSON(),
            completedAt: Date.now()
        });

        const rewards = mission.rewards;

        if (this.onMissionComplete) {
            this.onMissionComplete({
                mission: mission.toJSON(),
                rewards
            });
        }

        return rewards;
    }

    claimRewards(missionId) {
        const mission = this.missions.get(missionId);
        if (!mission) return { success: false, reason: 'mission_not_found' };
        if (!mission.isComplete) return { success: false, reason: 'mission_not_complete' };
        if (mission.rewardsClaimed) return { success: false, reason: 'rewards_already_claimed' };

        mission.rewardsClaimed = true;
        return { success: true, rewards: mission.rewards };
    }

    updatePlayerProgress(playerId, matchData) {
        let progress = this.playerProgress.get(playerId);
        if (!progress) {
            progress = {
                playerId,
                totalMissionsCompleted: 0,
                totalRewardsClaimed: 0,
                streakDays: 0,
                lastPlayedDate: null
            };
        }

        const today = new Date().toDateString();
        if (progress.lastPlayedDate !== today) {
            const yesterday = new Date(Date.now() - 24 * 60 * 60 * 1000).toDateString();
            if (progress.lastPlayedDate === yesterday) {
                progress.streakDays++;
            } else {
                progress.streakDays = 1;
            }
            progress.lastPlayedDate = today;
        }

        this.updateMissionProgress

for (const mission of this.getActiveMissions()) {
            this.updateMissionProgress(mission.id, matchData);
        }

        this.playerProgress.set(playerId, progress);
        return progress;
    }

    getPlayerProgress(playerId) {
        return this.playerProgress.get(playerId);
    }

    saveMissionsToStorage(type) {
        try {
            const missions = type === 'daily' ? this.dailyMissions : this.weeklyMissions;
            const json = missions.map(m => m.toJSON());
            localStorage.setItem(`missions_${type}`, JSON.stringify(json));
        } catch (e) {
            console.error('Failed to save missions:', e);
        }
    }

    loadMissionsFromStorage(type) {
        try {
            const json = localStorage.getItem(`missions_${type}`);
            if (json) {
                const data = JSON.parse(json);
                return data.map(m => Mission.fromJSON(m));
            }
        } catch (e) {
            console.error('Failed to load missions:', e);
        }
        return [];
    }

    saveProgressToStorage(missionId) {
        try {
            const mission = this.missions.get(missionId);
            if (mission) {
                localStorage.setItem(`mission_progress_${missionId}`, JSON.stringify(mission.toJSON()));
            }
        } catch (e) {
            console.error('Failed to save progress:', e);
        }
    }

    loadProgressFromStorage(missionId) {
        try {
            const json = localStorage.getItem(`mission_progress_${missionId}`);
            if (json) {
                return Mission.fromJSON(JSON.parse(json));
            }
        } catch (e) {
            console.error('Failed to load progress:', e);
        }
        return null;
    }

    getMissionStats() {
        const total = this.missions.size;
        const completed = Array.from(this.missions.values()).filter(m => m.isComplete).length;
        const expired = Array.from(this.missions.values()).filter(m => m.isExpired).length;
        const active = total - completed - expired;

        return {
            total,
            completed,
            expired,
            active,
            dailyCompleted: this.dailyMissions.filter(m => m.isComplete).length,
            weeklyCompleted: this.weeklyMissions.filter(m => m.isComplete).length
        };
    }

    resetDailyMissions() {
        const now = new Date();
        const lastReset = this.lastDailyReset;

        if (!lastReset || now.getDate() !== lastReset.getDate()) {
            this.dailyMissions = [];
            this.refreshDailyMissions();
            this.lastDailyReset = now;
        }
    }

    resetWeeklyMissions() {
        const now = new Date();
        const lastReset = this.lastWeeklyReset;

        if (!lastReset || now.getWeek() !== lastReset.getWeek()) {
            this.weeklyMissions = [];
            this.refreshWeeklyMissions();
            this.lastWeeklyReset = now;
        }
    }
}

export class GuardianMission extends Mission {
    constructor(config = {}) {
        super(config);
        this.guardianType = config.guardianType || 'standard';
        this.botDifficulty = config.botDifficulty || 'normal';
        this.objectiveZone = config.objectiveZone || null;
        this.timeLimit = config.timeLimit || 300;
        this.respawns = config.respawns || 0;
        this.currentRespawns = this.respawns;
        this.eliminations = config.eliminations || 0;
        this.requiredEliminations = config.requiredEliminations || 20;
        this.botsRemaining = config.botsRemaining || 0;
        this.objectiveProgress = config.objectiveProgress || 0;
        this.requiredObjectiveProgress = config.requiredObjectiveProgress || 100;
    }

    static get GUARDIAN_TYPES() {
        return [
            'standard', 'assault', 'sniper', 'pistol', 'knife',
            'noRadar', 'restrictedBuy', 'headshotOnly', 'heavyWeapons'
        ];
    }

    updateProgress(matchData) {
        this.eliminations += matchData.kills || 0;
        this.objectiveProgress += matchData.objectiveTime || 0;
        this.botsRemaining = Math.max(0, this.botsRemaining - (matchData.kills || 0));

        if (this.respawns > 0 && matchData.deaths > 0) {
            this.currentRespawns = Math.max(0, this.currentRespawns - matchData.deaths);
        }

        if (this.respawns === 0 && (matchData.deaths > 0 || this.eliminations < this.requiredEliminations)) {
            this.isFailed = true;
        }

        const eliminationProgress = Math.min(100, (this.eliminations / this.requiredEliminations) * 100);
        const objectiveProgress = Math.min(100, (this.objectiveProgress / this.requiredObjectiveProgress) * 100);

        this.progress = Math.max(eliminationProgress, objectiveProgress);

        if (this.eliminations >= this.requiredEliminations || this.objectiveProgress >= this.requiredObjectiveProgress) {
            this.complete();
        }

        return this.progress;
    }

    complete() {
        if (this.isFailed) {
            return { success: false, reason: 'mission_failed' };
        }
        return super.complete();
    }
}

export class CoOpMission extends Mission {
    constructor(config = {}) {
        super(config);
        this.coopType = config.coopType || 'standard';
        this.difficulty = config.difficulty || 'normal';
        this.maxPlayers = config.maxPlayers || 5;
        this.currentPlayers = config.currentPlayers || 0;
        this.botTeammates = config.botTeammates || false;
        this.objectives = config.objectives || [];
        this.currentObjectiveIndex = 0;
        this.checkpoints = config.checkpoints || [];
        this.currentCheckpoint = config.currentCheckpoint || 0;
        this.deaths = config.deaths || 0;
        this.maxDeaths = config.maxDeaths || 3;
        this.timeElapsed = config.timeElapsed || 0;
    }

    static get COOP_TYPES() {
        return [
            'weaponMaster', 'guardian', 'mission', 'wingman',
            'training', 'demo', 'survival'
        ];
    }

    updateProgress(matchData) {
        this.timeElapsed += matchData.roundTime || 0;
        this.deaths += matchData.deaths || 0;
        this.currentPlayers = matchData.players || this.currentPlayers;

        if (this.deaths >= this.maxDeaths) {
            this.isFailed = true;
            return this.progress;
        }

        const currentObjective = this.objectives[this.currentObjectiveIndex];
        if (currentObjective) {
            if (this.evaluateObjective(currentObjective, matchData)) {
                this.currentObjectiveIndex++;
                this.progress = Math.floor((this.currentObjectiveIndex / this.objectives.length) * 100);
            }
        }

        if (this.currentObjectiveIndex >= this.objectives.length) {
            this.complete();
        }

        return this.progress;
    }

    evaluateObjective(objective, matchData) {
        switch (objective.type) {
            case 'eliminate':
                return (matchData.kills || 0) >= objective.value;
            case 'collect':
                return (matchData.collected || 0) >= objective.value;
            case 'reach':
                return matchData.reachedLocation === objective.location;
            case 'protect':
                return (matchData.timeProtected || 0) >= objective.value;
            case 'survive':
                return matchData.survivedTime >= objective.value;
            default:
                return false;
        }
    }

    restart() {
        this.currentObjectiveIndex = 0;
        this.currentCheckpoint = 0;
        this.deaths = 0;
        this.timeElapsed = 0;
        this.isFailed = false;
        this.progress = 0;
    }
}

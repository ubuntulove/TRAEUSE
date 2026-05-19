import { GameState } from './config.js';

export const GameModeType = {
    COMPETITIVE: 'competitive',
    CASUAL: 'casual',
    DEATHMATCH: 'deathmatch',
    ARMS_RACE: 'arms_race',
    DEMOLITION: 'demolition',
    FLYING_SCOUTSMAN: 'flying_scoutman',
    WINGMAN: 'wingman',
    GUARDIAN: 'guardian',
    COOP_STRIKE: 'coop_strike',
    DANGER_ZONE: 'danger_zone',
    CUSTOM: 'custom'
};

export const MatchResult = {
    WIN: 'win',
    LOSS: 'loss',
    DRAW: 'draw',
    NONE: 'none'
};

export const GameModeConfig = {
    [GameModeType.COMPETITIVE]: {
        name: 'Competitive',
        shortName: 'Comp',
        maxPlayers: 10,
        teams: 2,
        roundsPerHalf: 15,
        roundTime: 115,
        freezeTime: 5,
        buyTime: 20,
        overtimeRounds: 3,
        overtimeDuration: 60,
        winCondition: (score) => score.tScore >= 16 || score.ctScore >= 16,
        overtimeEnabled: true,
        friendlyFire: false,
        allowPistolRound: true,
        rankingEnabled: true,
        vacEnabled: true,
        overtimeMultiplier: 1.5
    },
    [GameModeType.CASUAL]: {
        name: 'Casual',
        shortName: 'Cas',
        maxPlayers: 16,
        teams: 2,
        roundsPerHalf: 15,
        roundTime: 90,
        freezeTime: 3,
        buyTime: 15,
        overtimeRounds: 0,
        winCondition: (score) => score.tScore >= 15 || score.ctScore >= 15,
        overtimeEnabled: false,
        friendlyFire: false,
        allowPistolRound: true,
        rankingEnabled: false,
        vacEnabled: true,
        overtimeMultiplier: 1
    },
    [GameModeType.DEATHMATCH]: {
        name: 'Deathmatch',
        shortName: 'DM',
        maxPlayers: 16,
        teams: 0,
        roundsPerHalf: 0,
        roundTime: 600,
        respawnTime: 3,
        killLimit: 30,
        scorePerKill: 100,
        scorePerHeadshot: 50,
        scorePerKnifeKill: 200,
        noFriendlyFire: true,
        allowKillstreaks: false,
        rankingEnabled: false,
        vacEnabled: true,
        mapRotation: true
    },
    [GameModeType.ARMS_RACE]: {
        name: 'Arms Race',
        shortName: 'AR',
        maxPlayers: 16,
        teams: 0,
        roundsPerHalf: 0,
        roundTime: 120,
        winCondition: (score) => score.score >= 26,
        progressionEnabled: true,
        noFriendlyFire: true,
        weaponProgression: true,
        knifeProgression: true,
        meleeFinal: true,
        respawnEnabled: true,
        respawnTime: 2,
        rankingEnabled: false,
        vacEnabled: true,
        mapRotation: true,
        weapons: [
            { name: 'Glock-18', type: 'pistol', index: 0 },
            { name: 'P250', type: 'pistol', index: 1 },
            { name: 'Nova', type: 'shotgun', index: 2 },
            { name: 'MP9', type: 'smg', index: 3 },
            { name: 'AK-47', type: 'rifle', index: 4 },
            { name: 'M4A4', type: 'rifle', index: 5 },
            { name: 'AWP', type: 'sniper', index: 6 },
            { name: 'M249', type: 'heavy', index: 7 },
            { name: 'AUG', type: 'rifle', index: 8 },
            { name: 'SG 553', type: 'rifle', index: 9 },
            { name: 'AWP', type: 'sniper', index: 10 },
            { name: 'Glock-18', type: 'pistol', index: 11 },
            { name: 'Desert Eagle', type: 'pistol', index: 12 },
            { name: 'M4A1-S', type: 'rifle', index: 13 },
            { name: 'AK-47', type: 'rifle', index: 14 },
            { name: 'Five-SeveN', type: 'pistol', index: 15 },
            { name: 'AWP', type: 'sniper', index: 16 },
            { name: 'M4A4', type: 'rifle', index: 17 },
            { name: 'AK-47', type: 'rifle', index: 18 },
            { name: 'Glock-18', type: 'pistol', index: 19 },
            { name: 'Desert Eagle', type: 'pistol', index: 20 },
            { name: 'AK-47', type: 'rifle', index: 21 },
            { name: 'M4A4', type: 'rifle', index: 22 },
            { name: 'AWP', type: 'sniper', index: 23 },
            { name: 'Knife', type: 'melee', index: 24 },
            { name: 'Golden Knife', type: 'melee', index: 25 }
        ]
    },
    [GameModeType.DEMOLITION]: {
        name: 'Demolition',
        shortName: 'Dem',
        maxPlayers: 16,
        teams: 2,
        roundsPerHalf: 10,
        roundTime: 90,
        winCondition: (score) => score.tScore >= 5 || score.ctScore >= 5,
        progressionEnabled: true,
        weaponProgression: true,
        noBuyMenu: true,
        respawnEnabled: true,
        respawnTime: 2,
        scorePerKill: 100,
        scorePerPlant: 100,
        scorePerHeadshot: 50,
        scorePerWin: 200,
        rankingEnabled: false,
        vacEnabled: true,
        mapRotation: true,
        weapons: [
            { name: 'SSG 08', type: 'sniper', index: 0 },
            { name: 'Galil AR', type: 'rifle', index: 1 },
            { name: 'SG 553', type: 'rifle', index: 2 },
            { name: 'AUG', type: 'rifle', index: 3 },
            { name: 'M4A4', type: 'rifle', index: 4 },
            { name: 'AK-47', type: 'rifle', index: 5 },
            { name: 'AWP', type: 'sniper', index: 6 },
            { name: 'Glock-18', type: 'pistol', index: 7 },
            { name: 'M4A1-S', type: 'rifle', index: 8 },
            { name: 'Five-SeveN', type: 'pistol', index: 9 }
        ]
    },
    [GameModeType.FLYING_SCOUTSMAN]: {
        name: 'Flying Scoutsman',
        shortName: 'FS',
        maxPlayers: 16,
        teams: 2,
        roundsPerHalf: 15,
        roundTime: 120,
        winCondition: (score) => score.tScore >= 8 || score.ctScore >= 8,
        progressionEnabled: true,
        weaponProgression: true,
        noBuyMenu: true,
        respawnEnabled: false,
        gravityMultiplier: 0.5,
        movementSpeedMultiplier: 1.5,
        allowScoutOnly: true,
        scoutZoomLevels: [4, 8],
        scoutFireRateMultiplier: 1.5,
        scoutDamageMultiplier: 1.75,
        scorePerKill: 100,
        scorePerHeadshot: 50,
        scorePerWin: 200,
        rankingEnabled: false,
        vacEnabled: true,
        weapons: [
            { name: 'SSG 08', type: 'sniper', index: 0 },
            { name: 'SCAR-20', type: 'auto', index: 1 },
            { name: 'SSG 08', type: 'sniper', index: 2 },
            { name: 'SCAR-20', type: 'auto', index: 3 },
            { name: 'SSG 08', type: 'sniper', index: 4 },
            { name: 'SCAR-20', type: 'auto', index: 5 },
            { name: 'SSG 08', type: 'sniper', index: 6 },
            { name: 'SCAR-20', type: 'auto', index: 7 },
            { name: 'SSG 08', type: 'sniper', index: 8 },
            { name: 'SSG 08', type: 'sniper', index: 9 }
        ]
    },
    [GameModeType.WINGMAN]: {
        name: 'Wingman',
        shortName: 'Wing',
        maxPlayers: 4,
        teams: 2,
        roundsPerHalf: 8,
        roundTime: 90,
        freezeTime: 5,
        buyTime: 15,
        overtimeRounds: 2,
        winCondition: (score) => score.tScore >= 8 || score.ctScore >= 8,
        overtimeEnabled: true,
        friendlyFire: false,
        allowPistolRound: true,
        rankingEnabled: true,
        vacEnabled: true,
        maps: ['de_mirage', 'de_overpass', 'de_nuke', 'de_inferno', 'de_dust2', 'de_vertigo', 'de_ancient']
    },
    [GameModeType.GUARDIAN]: {
        name: 'Guardian',
        shortName: 'Guard',
        maxPlayers: 6,
        teams: 2,
        playerCount: { terrorists: 1, counterTerrorists: 5 },
        roundsPerHalf: 12,
        roundTime: 120,
        winCondition: (score) => score.ctScore >= 10 || score.tScore >= 10,
        friendlyFire: false,
        botDifficulty: 'hard',
        co-opMode: true,
        rankingEnabled: false,
        vacEnabled: true
    },
    [GameModeType.COOP_STRIKE]: {
        name: 'Co-op Strike',
        shortName: 'Coop',
        maxPlayers: 6,
        teams: 2,
        playerCount: { terrorists: 1, counterTerrorists: 5 },
        roundsPerHalf: 12,
        roundTime: 120,
        winCondition: (score) => score.ctScore >= 10 || score.tScore >= 10,
        friendlyFire: false,
        botDifficulty: 'hard',
        co-opMode: true,
        rankingEnabled: false,
        vacEnabled: true,
        missionBased: true
    },
    [GameModeType.DANGER_ZONE]: {
        name: 'Danger Zone',
        shortName: 'DZ',
        maxPlayers: 18,
        teams: 0,
        individualMode: true,
        battleRoyale: true,
        lootEnabled: true,
        buyMenuEnabled: true,
        noTeams: true,
        startMoney: 0,
        zoneShrinkTime: 45,
        maxZoneShrinks: 5,
        startZoneSize: 100,
        lootTiers: ['common', 'uncommon', 'rare', 'epic', 'legendary'],
        progressionEnabled: false,
        rankingEnabled: false,
        vacEnabled: true,
        maps: ['dz_sirocco', 'dz_junghitz', 'dz_arid', 'dz_frostbite']
    }
};

export class ArmsRaceProgress {
    constructor(playerId) {
        this.playerId = playerId;
        this.currentWeaponIndex = 0;
        this.kills = 0;
        this.assists = 0;
        this.deaths = 0;
        this.headshots = 0;
        this.currentWeaponKills = 0;
        this.score = 0;
        this.levelUps = 0;
        this.levelDowns = 0;
        this.meleeKills = 0;
        this.knifeKills = 0;
        this.grenadeKills = 0;
        this.weaponHistory = [];
    }

    addKill(isHeadshot, weaponType) {
        this.kills++;
        this.currentWeaponKills++;

        if (isHeadshot) {
            this.headshots++;
            this.score += 50;
        } else {
            this.score += 100;
        }

        if (weaponType === 'knife') {
            this.knifeKills++;
            this.score += 200;
        }

        this.weaponHistory.push({
            weaponIndex: this.currentWeaponIndex,
            tick: Date.now(),
            type: 'kill'
        });

        if (this.currentWeaponKills >= 1 && this.currentWeaponIndex < 25) {
            this.levelUp();
        }
    }

    levelUp() {
        this.levelUps++;
        this.currentWeaponIndex++;
        this.currentWeaponKills = 0;
        this.score += 100;
    }

    levelDown() {
        if (this.currentWeaponIndex > 0) {
            this.levelDowns++;
            this.currentWeaponIndex--;
            this.currentWeaponKills = 0;
        }
    }

    addDeath() {
        this.deaths++;
        this.currentWeaponKills = 0;
        this.weaponHistory.push({
            weaponIndex: this.currentWeaponIndex,
            tick: Date.now(),
            type: 'death'
        });

        if (this.currentWeaponIndex > 0) {
            this.levelDown();
        }
    }

    addAssist() {
        this.assists++;
        this.score += 50;
    }

    getCurrentWeapon() {
        const weapons = GameModeConfig[GameModeType.ARMS_RACE].weapons;
        return weapons[Math.min(this.currentWeaponIndex, weapons.length - 1)];
    }

    getProgress() {
        return {
            currentWeapon: this.getCurrentWeapon(),
            weaponIndex: this.currentWeaponIndex,
            kills: this.kills,
            deaths: this.deaths,
            assists: this.assists,
            headshots: this.headshots,
            score: this.score,
            kdr: this.deaths > 0 ? (this.kills / this.deaths).toFixed(2) : this.kills.toFixed(2),
            hsp: this.kills > 0 ? ((this.headshots / this.kills) * 100).toFixed(1) : '0.0'
        };
    }
}

export class DemolitionProgress {
    constructor(playerId) {
        this.playerId = playerId;
        this.currentWeaponIndex = 0;
        this.kills = 0;
        this.deaths = 0;
        this.assists = 0;
        this.headshots = 0;
        this.plants = 0;
        this.defuses = 0;
        this.wins = 0;
        this.currentWeaponKills = 0;
        this.score = 0;
        this.weaponKills = {};
        this.roundWins = 0;
    }

    addKill(isHeadshot) {
        this.kills++;
        this.currentWeaponKills++;
        this.weaponKills[this.currentWeaponIndex] = (this.weaponKills[this.currentWeaponIndex] || 0) + 1;

        if (isHeadshot) {
            this.headshots++;
            this.score += 150;
        } else {
            this.score += 100;
        }
    }

    levelUp() {
        if (this.currentWeaponIndex < 9) {
            this.currentWeaponIndex++;
            this.currentWeaponKills = 0;
        }
    }

    addDeath() {
        this.deaths++;
        this.currentWeaponKills = 0;
    }

    addAssist() {
        this.assists++;
        this.score += 50;
    }

    addPlant() {
        this.plants++;
        this.score += 100;
    }

    addDefuse() {
        this.defuses++;
        this.score += 100;
    }

    addRoundWin() {
        this.wins++;
        this.roundWins++;
        this.score += 200;
    }

    getCurrentWeapon() {
        const weapons = GameModeConfig[GameModeType.DEMOLITION].weapons;
        return weapons[Math.min(this.currentWeaponIndex, weapons.length - 1)];
    }

    getProgress() {
        return {
            currentWeapon: this.getCurrentWeapon(),
            weaponIndex: this.currentWeaponIndex,
            kills: this.kills,
            deaths: this.deaths,
            assists: this.assists,
            headshots: this.headshots,
            plants: this.plants,
            defuses: this.defuses,
            wins: this.wins,
            score: this.score,
            weaponKills: this.weaponKills
        };
    }
}

export class DeathmatchSession {
    constructor(mode) {
        this.mode = mode;
        this.kills = 0;
        this.deaths = 0;
        this.headshots = 0;
        this.knifeKills = 0;
        this.suicides = 0;
        this.score = 0;
        this.killStreak = 0;
        this.bestKillStreak = 0;
        this.recentKills = [];
        this.spawnLocations = [];
        this.respawnTimer = 0;
        this.isAlive = true;
    }

    addKill(isHeadshot, isKnife) {
        this.kills++;
        this.killStreak++;

        if (isKnife) {
            this.knifeKills++;
            this.score += 200;
        } else if (isHeadshot) {
            this.headshots++;
            this.score += 150;
        } else {
            this.score += 100;
        }

        if (this.killStreak > this.bestKillStreak) {
            this.bestKillStreak = this.killStreak;
        }

        this.recentKills.push({
            time: Date.now(),
            isHeadshot,
            isKnife
        });

        if (this.recentKills.length > 5) {
            this.recentKills.shift();
        }
    }

    addDeath() {
        this.deaths++;
        this.killStreak = 0;
        this.isAlive = false;
    }

    addSuicide() {
        this.suicides++;
        this.deaths++;
        this.killStreak = 0;
    }

    respawn() {
        this.isAlive = true;
        this.respawnTimer = 3;
    }

    getScore() {
        return this.score;
    }

    getKDRatio() {
        return this.deaths > 0 ? (this.kills / this.deaths).toFixed(2) : this.kills.toFixed(2);
    }

    getStats() {
        return {
            kills: this.kills,
            deaths: this.deaths,
            headshots: this.headshots,
            knifeKills: this.knifeKills,
            suicides: this.suicides,
            score: this.score,
            killStreak: this.killStreak,
            bestKillStreak: this.bestKillStreak,
            kdr: this.getKDRatio(),
            recentKills: this.recentKills
        };
    }
}

export class DangerZoneMatch {
    constructor(matchId) {
        this.matchId = id;
        this.players = new Map();
        this.alivePlayers = new Set();
        this.deadPlayers = new Set();
        this.lootItems = new Map();
        this.zones = [];
        this.currentZone = 0;
        this.matchStartTime = new Date();
        this.matchEndTime = null;
        this.winner = null;
        this.playerPositions = new Map();
        this.buyables = new Map();
        this.upgrades = new Map();
        this.drones = new Map();
        this.turrets = new Map();
        this.medkits = new Map();
    }

    addPlayer(playerId) {
        this.players.set(playerId, {
            id: playerId,
            health: 100,
            armor: 0,
            cash: 0,
            inventory: [],
            equipment: [],
            kills: 0,
            deaths: 0,
            assists: 0,
            position: null,
            lastPosition: null,
            lastSeenAlive: null,
            isAlive: true,
            isConnected: true,
            zone: 0,
            hasTablet: false,
            hasDrones: 0,
            droneCooldowns: [],
            upgrades: []
        });
        this.alivePlayers.add(playerId);
    }

    removePlayer(playerId) {
        const player = this.players.get(playerId);
        if (player) {
            player.isConnected = false;
            player.lastSeenAlive = new Date();
        }
        this.alivePlayers.delete(playerId);
        this.deadPlayers.add(playerId);
    }

    eliminatePlayer(playerId, killerId) {
        const player = this.players.get(playerId);
        const killer = killerId ? this.players.get(killerId) : null;

        if (player) {
            player.isAlive = false;
            player.deaths++;
            this.alivePlayers.delete(playerId);
            this.deadPlayers.add(playerId);
        }

        if (killer) {
            killer.kills++;
        }

        if (this.alivePlayers.size === 1) {
            const winnerId = Array.from(this.alivePlayers)[0];
            this.endMatch(winnerId);
        }
    }

    spawnLoot(position, tier) {
        const lootId = `loot_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
        this.lootItems.set(lootId, {
            id: lootId,
            position,
            tier,
            type: this.getRandomLootType(tier),
            value: this.getRandomLootValue(tier),
            owner: null,
            spawnTime: new Date()
        });
        return lootId;
    }

    getRandomLootType(tier) {
        const lootTypes = {
            common: ['pistol_ammo', 'smg_ammo', 'revolver', 'p250', 'taser', 'healthshot', 'flashlight'],
            uncommon: ['mp9', 'mac10', 'nova', 'ump45', 'xm1014', 'tec9', 'fiveseven', 'healthshot'],
            rare: ['ak47', 'm4a1s', 'm4a4', 'galilar', 'famas', 'sg553', 'aug', 'awp', 'ssg08', 'mp5sd'],
            epic: ['awp', 'ak47', 'm4a4', 'm4a1s', 'deagle', 'negev', 'nova', 'xm1014', 'sawedoff'],
            legendary: ['goldenknife', 'zeus_x27', 'max_cash', 'drone', 'exojamma']
        };
        const types = lootTypes[tier] || lootTypes.common;
        return types[Math.floor(Math.random() * types.length)];
    }

    getRandomLootValue(tier) {
        const values = {
            common: { min: 100, max: 300 },
            uncommon: { min: 300, max: 600 },
            rare: { min: 600, max: 1200 },
            epic: { min: 1200, max: 2000 },
            legendary: { min: 2000, max: 5000 }
        };
        const range = values[tier] || values.common;
        return Math.floor(Math.random() * (range.max - range.min)) + range.min;
    }

    shrinkZone() {
        this.currentZone++;
        if (this.currentZone >= 5) {
            this.endMatch(null);
        }
    }

    endMatch(winnerId) {
        this.winner = winnerId;
        this.matchEndTime = new Date();
    }

    addUpgrade(playerId, upgrade) {
        const player = this.players.get(playerId);
        if (player && player.isAlive) {
            if (!player.upgrades.includes(upgrade)) {
                player.upgrades.push(upgrade);
            }
        }
    }

    deployDrone(playerId) {
        const player = this.players.get(playerId);
        if (player && player.hasDrones > 0 && player.isAlive) {
            player.hasDrones--;
            const droneId = `drone_${Date.now()}`;
            this.drones.set(droneId, {
                id: droneId,
                owner: playerId,
                position: { ...player.position },
                battery: 100,
                deployed: true
            });
            return droneId;
        }
        return null;
    }

    getStats() {
        return {
            totalPlayers: this.players.size,
            alivePlayers: this.alivePlayers.size,
            deadPlayers: this.deadPlayers.size,
            lootItems: this.lootItems.size,
            currentZone: this.currentZone,
            matchDuration: this.matchEndTime
                ? this.matchEndTime - this.matchStartTime
                : Date.now() - this.matchStartTime,
            winner: this.winner
        };
    }
}

export class WingmanMatch {
    constructor(matchId) {
        this.matchId = matchId;
        this.teams = {
            team1: { players: [], kills: 0, score: 0 },
            team2: { players: [], kills: 0, score: 0 }
        };
        this.currentRound = 0;
        this.map = '';
        this.score = { team1: 0, team2: 0 };
        this.isOvertime = false;
        this.matchStartTime = new Date();
        this.matchEndTime = null;
        this.winner = null;
    }

    setTeams(team1Players, team2Players) {
        this.teams.team1.players = team1Players;
        this.teams.team2.players = team2Players;
    }

    addKill(team, killerId) {
        this.teams[team].kills++;
    }

    setRoundWinner(team) {
        this.score[team]++;
        this.currentRound++;

        if (this.score.team1 >= 8 || this.score.team2 >= 8) {
            this.endMatch(this.score.team1 > this.score.team2 ? 'team1' : 'team2');
        } else if (this.score.team1 + this.score.team2 >= 16) {
            this.startOvertime();
        }
    }

    startOvertime() {
        this.isOvertime = true;
    }

    endMatch(winner) {
        this.winner = winner;
        this.matchEndTime = new Date();
    }

    getMatchInfo() {
        return {
            matchId: this.matchId,
            map: this.map,
            score: this.score,
            currentRound: this.currentRound,
            isOvertime: this.isOvertime,
            duration: this.matchEndTime
                ? this.matchEndTime - this.matchStartTime
                : Date.now() - this.matchStartTime,
            winner: this.winner
        };
    }
}

export class GuardianMission {
    constructor(map) {
        this.map = map;
        this.objectives = [];
        this.currentObjective = 0;
        this.players = [];
        this.bots = [];
        this.score = 0;
        this.wins = 0;
        this.losses = 0;
        this.roundsCompleted = 0;
        this.startTime = new Date();
    }

    addObjective(description, target, count) {
        this.objectives.push({
            description,
            target,
            count,
            current: 0,
            completed: false
        });
    }

    updateObjective(index, progress) {
        if (this.objectives[index]) {
            this.objectives[index].current = progress;
            if (progress >= this.objectives[index].count) {
                this.objectives[index].completed = true;
            }
        }
    }

    completeObjective(index) {
        if (this.objectives[index] && !this.objectives[index].completed) {
            this.objectives[index].completed = true;
            this.objectives[index].current = this.objectives[index].count;
            this.score += 100;
            return true;
        }
        return false;
    }

    getCurrentObjective() {
        return this.objectives[this.currentObjective];
    }

    nextObjective() {
        this.currentObjective++;
        this.roundsCompleted++;
        return this.currentObjective < this.objectives.length;
    }

    isComplete() {
        return this.objectives.every(o => o.completed);
    }
}

export class GameModeManager {
    static instance = null;

    constructor() {
        this.modes = new Map();
        this.activeMode = null;
        this.customModes = new Map();
        this.listeners = new Map();
        this.stats = {
            gamesPlayed: new Map(),
            gamesWon: new Map(),
            totalKills: new Map(),
            totalDeaths: new Map()
        };
        this.initDefaultModes();
    }

    static getInstance() {
        if (!GameModeManager.instance) {
            GameModeManager.instance = new GameModeManager();
        }
        return GameModeManager.instance;
    }

    initDefaultModes() {
        for (const [modeType, config] of Object.entries(GameModeConfig)) {
            this.modes.set(modeType, {
                type: modeType,
                config,
                enabled: true,
                maps: config.maps || [],
                isFeatured: modeType === GameModeType.COMPETITIVE,
                playerCount: { min: 2, max: config.maxPlayers }
            });
        }
    }

    getMode(type) {
        return this.modes.get(type);
    }

    getAllModes() {
        return Array.from(this.modes.values());
    }

    getFeaturedModes() {
        return Array.from(this.modes.values()).filter(m => m.isFeatured);
    }

    getEnabledModes() {
        return Array.from(this.modes.values()).filter(m => m.enabled);
    }

    enableMode(type) {
        const mode = this.modes.get(type);
        if (mode) {
            mode.enabled = true;
            this.emit('modeEnabled', mode);
        }
    }

    disableMode(type) {
        const mode = this.modes.get(type);
        if (mode) {
            mode.enabled = false;
            this.emit('modeDisabled', mode);
        }
    }

    setActiveMode(type) {
        const mode = this.modes.get(type);
        if (mode && mode.enabled) {
            this.activeMode = mode;
            this.emit('activeModeChanged', mode);
        }
    }

    getActiveMode() {
        return this.activeMode;
    }

    createCustomMode(config) {
        const id = `custom_${Date.now()}`;
        const customMode = {
            type: GameModeType.CUSTOM,
            id,
            name: config.name,
            shortName: config.shortName || 'Custom',
            config: {
                ...GameModeConfig[GameModeType.COMPETITIVE],
                ...config
            },
            enabled: true,
            createdAt: new Date()
        };
        this.customModes.set(id, customMode);
        this.emit('customModeCreated', customMode);
        return customMode;
    }

    deleteCustomMode(id) {
        const deleted = this.customModes.delete(id);
        if (deleted) {
            this.emit('customModeDeleted', id);
        }
        return deleted;
    }

    getCustomModes() {
        return Array.from(this.customModes.values());
    }

    getModeStats(type) {
        return {
            gamesPlayed: this.stats.gamesPlayed.get(type) || 0,
            gamesWon: this.stats.gamesWon.get(type) || 0,
            totalKills: this.stats.totalKills.get(type) || 0,
            totalDeaths: this.stats.totalDeaths.get(type) || 0,
            winRate: this.getWinRate(type),
            kdRatio: this.getKDRatio(type)
        };
    }

    recordGamePlayed(type, won = false, kills = 0, deaths = 0) {
        const played = (this.stats.gamesPlayed.get(type) || 0) + 1;
        this.stats.gamesPlayed.set(type, played);

        if (won) {
            const won_count = (this.stats.gamesWon.get(type) || 0) + 1;
            this.stats.gamesWon.set(type, won_count);
        }

        const totalKills = (this.stats.totalKills.get(type) || 0) + kills;
        this.stats.totalKills.set(type, totalKills);

        const totalDeaths = (this.stats.totalDeaths.get(type) || 0) + deaths;
        this.stats.totalDeaths.set(type, totalDeaths);

        this.emit('gameRecorded', { type, won, kills, deaths });
    }

    getWinRate(type) {
        const played = this.stats.gamesPlayed.get(type) || 0;
        const won = this.stats.gamesWon.get(type) || 0;
        return played > 0 ? ((won / played) * 100).toFixed(1) : '0.0';
    }

    getKDRatio(type) {
        const deaths = this.stats.totalDeaths.get(type) || 0;
        const kills = this.stats.totalKills.get(type) || 0;
        return deaths > 0 ? (kills / deaths).toFixed(2) : kills.toFixed(2);
    }

    getRecommendedMode(playerSkill, playerStats) {
        const modes = this.getEnabledModes();

        if (playerStats.competitiveWins < 10) {
            return this.getMode(GameModeType.CASUAL);
        }

        if (playerStats.friends > 1) {
            return this.getMode(GameModeType.WINGMAN);
        }

        if (playerSkill > 2000) {
            return this.getMode(GameModeType.COMPETITIVE);
        }

        return this.getMode(GameModeType.CASUAL);
    }

    exportStats() {
        const stats = {};
        for (const [type] of this.modes) {
            stats[type] = this.getModeStats(type);
        }
        return stats;
    }

    importStats(data) {
        for (const [type, modeStats] of Object.entries(data)) {
            this.stats.gamesPlayed.set(type, modeStats.gamesPlayed || 0);
            this.stats.gamesWon.set(type, modeStats.gamesWon || 0);
            this.stats.totalKills.set(type, modeStats.totalKills || 0);
            this.stats.totalDeaths.set(type, modeStats.totalDeaths || 0);
        }
    }

    on(event, callback) {
        if (!this.listeners.has(event)) {
            this.listeners.set(event, []);
        }
        this.listeners.get(event).push(callback);
    }

    off(event, callback) {
        if (!this.listeners.has(event)) return;
        const callbacks = this.listeners.get(event);
        const index = callbacks.indexOf(callback);
        if (index > -1) {
            callbacks.splice(index, 1);
        }
    }

    emit(event, data) {
        if (!this.listeners.has(event)) return;
        for (const callback of this.listeners.get(event)) {
            try {
                callback(data);
            } catch (e) {
                console.error(`Error in game mode listener: ${e}`);
            }
        }
    }
}

export const GameModes = GameModeManager.getInstance();

import { GameState } from './config.js';

export const CombatEventType = {
    SHOT_FIRED: 'shot_fired',
    SHOT_HIT: 'shot_hit',
    SHOT_MISSED: 'shot_missed',
    KILL: 'kill',
    DEATH: 'death',
    DAMAGE_DEALT: 'damage_dealt',
    DAMAGE_TAKEN: 'damage_taken',
    HEADSHOT: 'headshot',
    TRADE_KILL: 'trade_kill',
    CLUTCH: 'clutch',
    FLASH_BLIND: 'flash_blind',
    SMOKE_BLOCK: 'smoke_block',
    MOLOTOV_DAMAGE: 'molotov_damage',
    GRENADE_DAMAGE: 'grenade_damage'
};

export const WeaponCategory = {
    PISTOL: 'pistol',
    RIFLE: 'rifle',
    SMG: 'smg',
    SHOTGUN: 'shotgun',
    SNIPER: 'sniper',
    HEAVY: 'heavy',
    MELEE: 'melee',
    GRENADE: 'grenade',
    EQUIPMENT: 'equipment'
};

export const HitZone = {
    HEAD: 'head',
    CHEST: 'chest',
    STOMACH: 'stomach',
    LEFT_ARM: 'left_arm',
    RIGHT_ARM: 'right_arm',
    LEFT_LEG: 'left_leg',
    RIGHT_LEG: 'right_leg'
};

export class CombatEvent {
    constructor(type, data) {
        this.id = `event_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
        this.type = type;
        this.data = data;
        this.timestamp = Date.now();
        this.tick = 0;
        this.roundNumber = 0;
        this.position = data.position || null;
        this.attacker = data.attacker || null;
        this.victim = data.victim || null;
        this.weapon = data.weapon || null;
        this.isHeadshot = data.isHeadshot || false;
        this.damage = data.damage || 0;
        this.hitZone = data.hitZone || null;
    }

    export() {
        return {
            id: this.id,
            type: this.type,
            data: this.data,
            timestamp: this.timestamp,
            tick: this.tick,
            round_number: this.roundNumber,
            position: this.position,
            attacker: this.attacker,
            victim: this.victim,
            weapon: this.weapon,
            is_headshot: this.isHeadshot,
            damage: this.damage,
            hit_zone: this.hitZone
        };
    }

    static import(data) {
        const event = new CombatEvent(data.type, data.data);
        event.id = data.id;
        event.timestamp = data.timestamp;
        event.tick = data.tick;
        event.roundNumber = data.round_number;
        event.position = data.position;
        event.attacker = data.attacker;
        event.victim = data.victim;
        event.weapon = data.weapon;
        event.isHeadshot = data.is_headshot;
        event.damage = data.damage;
        event.hitZone = data.hit_zone;
        return event;
    }
}

export class ShotAnalysis {
    constructor(weapon) {
        this.weapon = weapon;
        this.category = this.getWeaponCategory(weapon);
        this.shotsFired = 0;
        this.shotsHit = 0;
        this.shotsMissed = 0;
        this.damageDealt = 0;
        this.headshots = 0;
        this.bodyshots = 0;
        this.legshots = 0;
        this.kills = 0;
        this.shotsByDistance = new Map();
        this.hitPositions = [];
        this.missPositions = [];
        this.spreadData = [];
        this.recoilPattern = [];
        this.firstShotAccuracy = 0;
        this.firstShotHits = 0;
        this.firstShotTotal = 0;
        this.sprayAccuracy = 0;
        this.sprayHits = 0;
        this.sprayShots = 0;
        this.tapFireAccuracy = 0;
        this.burstAccuracy = 0;
    }

    getWeaponCategory(weapon) {
        if (weapon.includes('pistol') || weapon.includes('deagle') || weapon.includes('revolver')) {
            return WeaponCategory.PISTOL;
        }
        if (weapon.includes('ak47') || weapon.includes('m4') || weapon.includes('galil') || weapon.includes('famas')) {
            return WeaponCategory.RIFLE;
        }
        if (weapon.includes('smg') || weapon.includes('mp')) {
            return WeaponCategory.SMG;
        }
        if (weapon.includes('shotgun') || weapon.includes('nova') || weapon.includes('mag7')) {
            return WeaponCategory.SHOTGUN;
        }
        if (weapon.includes('awp') || weapon.includes('ssg') || weapon.includes('scout') || weapon.includes('scar')) {
            return WeaponCategory.SNIPER;
        }
        if (weapon.includes('machinegun') || weapon.includes('negev') || weapon.includes('m249')) {
            return WeaponCategory.HEAVY;
        }
        if (weapon.includes('knife')) {
            return WeaponCategory.MELEE;
        }
        return WeaponCategory.EQUIPMENT;
    }

    recordShot(isHit, distance, position, damage = 0, hitZone = null) {
        this.shotsFired++;

        if (isHit) {
            this.shotsHit++;
            this.damageDealt += damage;
            this.hitPositions.push({ position, distance, hitZone });

            if (hitZone === HitZone.HEAD) {
                this.headshots++;
            } else if (hitZone === HitZone.CHEST || hitZone === HitZone.STOMACH) {
                this.bodyshots++;
            } else {
                this.legshots++;
            }

            if (this.shotsFired === 1) {
                this.firstShotTotal++;
                this.firstShotHits++;
            } else {
                this.sprayShots++;
                this.sprayHits++;
            }
        } else {
            this.shotsMissed++;
            this.missPositions.push({ position, distance });

            if (this.shotsFired === 1) {
                this.firstShotTotal++;
            } else {
                this.sprayShots++;
            }
        }

        this.updateAccuracy();
        this.updateDistanceStats(distance, isHit);
    }

    recordKill() {
        this.kills++;
    }

    updateAccuracy() {
        if (this.shotsFired > 0) {
            this.shotsHit / this.shotsFired * 100;
        }
        if (this.firstShotTotal > 0) {
            this.firstShotAccuracy = (this.firstShotHits / this.firstShotTotal) * 100;
        }
        if (this.sprayShots > 0) {
            this.sprayAccuracy = (this.sprayHits / this.sprayShots) * 100;
        }
    }

    updateDistanceStats(distance, isHit) {
        const distanceBucket = Math.floor(distance / 100) * 100;
        if (!this.shotsByDistance.has(distanceBucket)) {
            this.shotsByDistance.set(distanceBucket, { shots: 0, hits: 0 });
        }
        const stats = this.shotsByDistance.get(distanceBucket);
        stats.shots++;
        if (isHit) stats.hits++;
    }

    getAccuracy() {
        return this.shotsFired > 0 ? (this.shotsHit / this.shotsFired) * 100 : 0;
    }

    getHeadshotPercentage() {
        return this.shotsHit > 0 ? (this.headshots / this.shotsHit) * 100 : 0;
    }

    getDamagePerShot() {
        return this.shotsHit > 0 ? this.damageDealt / this.shotsHit : 0;
    }

    getDistanceAccuracy(distance) {
        const distanceBucket = Math.floor(distance / 100) * 100;
        const stats = this.shotsByDistance.get(distanceBucket);
        return stats ? (stats.shots > 0 ? (stats.hits / stats.shots) * 100 : 0) : 0;
    }

    export() {
        return {
            weapon: this.weapon,
            category: this.category,
            shots_fired: this.shotsFired,
            shots_hit: this.shotsHit,
            shots_missed: this.shotsMissed,
            accuracy: this.getAccuracy(),
            damage_dealt: this.damageDealt,
            headshots: this.headshots,
            bodyshots: this.bodyshots,
            legshots: this.legshots,
            headshot_percentage: this.getHeadshotPercentage(),
            kills: this.kills,
            damage_per_shot: this.getDamagePerShot(),
            first_shot_accuracy: this.firstShotAccuracy,
            spray_accuracy: this.sprayAccuracy,
            distance_stats: Object.fromEntries(this.shotsByDistance)
        };
    }
}

export class CombatStatistics {
    constructor(playerId) {
        this.playerId = playerId;
        this.kills = 0;
        this.deaths = 0;
        this.assists = 0;
        this.headshots = 0;
        this.mvps = 0;
        this.aces = 0;
        this.fbKills = 0;
        this.lbKills = 0;
        this.tradeKills = 0;
        this.tradeDeaths = 0;
        this.clutchAttempts = 0;
        this.clutchWins = 0;
        this.oneV1Wins = 0;
        this.oneV2Wins = 0;
        this.oneV3Wins = 0;
        this.oneV4Wins = 0;
        this.oneV5Wins = 0;
        this.totalDamage = 0;
        this.totalAssumedDamage = 0;
        this.damagePerRound = [];
        this.damageByWeapon = new Map();
        this.damageByHitZone = new Map();
        this.damageByDistance = new Map();
        this.killsByWeapon = new Map();
        this.deathsByWeapon = new Map();
        this.killsByDistance = new Map();
        this.deathsByDistance = new Map();
        this.killStreaks = [];
        this.currentKillStreak = 0;
        this.bestKillStreak = 0;
        this.deathStreaks = [];
        this.currentDeathStreak = 0;
        this.bestDeathStreak = 0;
        this.shots = new Map();
        this.recoilCompensation = 0;
        this.recoilCompensationAttempts = 0;
        this.utilityThrown = { smoke: 0, flash: 0, he: 0, molotov: 0, decoy: 0 };
        this.utilityHits = { smoke: 0, flash: 0, he: 0, molotov: 0, decoy: 0 };
        this.utilityDamage = { smoke: 0, flash: 0, he: 0, molotov: 0, decoy: 0 };
        this.flashAssists = 0;
        this.enemiesFlashed = 0;
        this.teammatesFlashed = 0;
        this.smokeBlocks = 0;
        this.positionData = [];
        this.timingData = [];
    }

    addKill(victim, weapon, distance, position, isHeadshot, isTrade, isFB) {
        this.kills++;
        this.currentKillStreak++;
        this.currentDeathStreak = 0;

        if (this.currentKillStreak > this.bestKillStreak) {
            this.bestKillStreak = this.currentKillStreak;
        }

        this.killStreaks.push({
            kills: this.currentKillStreak,
            timestamp: Date.now()
        });

        if (isHeadshot) this.headshots++;
        if (isTrade) this.tradeKills++;
        if (isFB) this.fbKills++;

        this.recordWeaponKill(weapon);
        this.recordDistanceKill(distance);
        this.recordPosition(position);
    }

    addDeath(killer, weapon, distance, position, isTrade) {
        this.deaths++;
        this.currentDeathStreak++;
        this.currentKillStreak = 0;

        if (this.currentDeathStreak > this.bestDeathStreak) {
            this.bestDeathStreak = this.currentDeathStreak;
        }

        this.deathStreaks.push({
            deaths: this.currentDeathStreak,
            timestamp: Date.now()
        });

        if (isTrade) this.tradeDeaths++;

        this.recordWeaponDeath(weapon);
        this.recordDistanceDeath(distance);
    }

    addAssist() {
        this.assists++;
    }

    addMVP() {
        this.mvps++;
    }

    recordAce() {
        this.aces++;
    }

    recordDamage(damage, weapon, hitZone, distance, isAssumed = false) {
        this.totalDamage += damage;

        if (isAssumed) {
            this.totalAssumedDamage += damage;
        }

        const weaponStats = this.damageByWeapon.get(weapon) || 0;
        this.damageByWeapon.set(weapon, weaponStats + damage);

        const zoneStats = this.damageByHitZone.get(hitZone) || 0;
        this.damageByHitZone.set(hitZone, zoneStats + damage);

        const distanceBucket = Math.floor(distance / 100) * 100;
        const distanceStats = this.damageByDistance.get(distanceBucket) || 0;
        this.damageByDistance.set(distanceBucket, distanceStats + damage);
    }

    recordWeaponKill(weapon) {
        const count = this.killsByWeapon.get(weapon) || 0;
        this.killsByWeapon.set(weapon, count + 1);
    }

    recordWeaponDeath(weapon) {
        const count = this.deathsByWeapon.get(weapon) || 0;
        this.deathsByWeapon.set(weapon, count + 1);
    }

    recordDistanceKill(distance) {
        const bucket = Math.floor(distance / 100) * 100;
        const count = this.killsByDistance.get(bucket) || 0;
        this.killsByDistance.set(bucket, count + 1);
    }

    recordDistanceDeath(distance) {
        const bucket = Math.floor(distance / 100) * 100;
        const count = this.deathsByDistance.get(bucket) || 0;
        this.deathsByDistance.set(bucket, count + 1);
    }

    recordPosition(position) {
        this.positionData.push({
            position,
            timestamp: Date.now()
        });

        if (this.positionData.length > 1000) {
            this.positionData.shift();
        }
    }

    recordShot(weapon, isHit, distance, position, damage, hitZone) {
        if (!this.shots.has(weapon)) {
            this.shots.set(weapon, new ShotAnalysis(weapon));
        }

        const analysis = this.shots.get(weapon);
        analysis.recordShot(isHit, distance, position, damage, hitZone);
    }

    recordUtility(type) {
        if (this.utilityThrown[type] !== undefined) {
            this.utilityThrown[type]++;
        }
    }

    recordUtilityHit(type) {
        if (this.utilityHits[type] !== undefined) {
            this.utilityHits[type]++;
        }
    }

    recordUtilityDamage(type, damage) {
        if (this.utilityDamage[type] !== undefined) {
            this.utilityDamage[type] += damage;
        }
    }

    recordFlashAssist() {
        this.flashAssists++;
    }

    recordEnemyFlashed() {
        this.enemiesFlashed++;
    }

    recordTeammateFlashed() {
        this.teammatesFlashed++;
    }

    recordSmokeBlock() {
        this.smokeBlocks++;
    }

    recordClutchAttempt() {
        this.clutchAttempts++;
    }

    recordClutchWin(enemiesRemaining) {
        this.clutchWins++;

        if (enemiesRemaining === 1) this.oneV1Wins++;
        else if (enemiesRemaining === 2) this.oneV2Wins++;
        else if (enemiesRemaining === 3) this.oneV3Wins++;
        else if (enemiesRemaining === 4) this.oneV4Wins++;
        else if (enemiesRemaining === 5) this.oneV5Wins++;
    }

    recordTiming(type, duration) {
        this.timingData.push({
            type,
            duration,
            timestamp: Date.now()
        });
    }

    getKDRatio() {
        return this.deaths > 0 ? this.kills / this.deaths : this.kills;
    }

    getHeadshotPercentage() {
        return this.kills > 0 ? (this.headshots / this.kills) * 100 : 0;
    }

    getAverageDamage() {
        const total = this.damagePerRound.reduce((a, b) => a + b, 0);
        return this.damagePerRound.length > 0 ? total / this.damagePerRound.length : 0;
    }

    getADR() {
        return Math.round(this.getAverageDamage());
    }

    getWinRate(wins, losses) {
        const total = wins + losses;
        return total > 0 ? (wins / total) * 100 : 0;
    }

    getClutchRate() {
        return this.clutchAttempts > 0 ? (this.clutchWins / this.clutchAttempts) * 100 : 0;
    }

    getBestWeapon() {
        let best = null;
        let bestKills = 0;

        for (const [weapon, kills] of this.killsByWeapon) {
            if (kills > bestKills) {
                bestKills = kills;
                best = weapon;
            }
        }

        return best;
    }

    getWeaponStats(weapon) {
        return this.shots.get(weapon)?.export() || null;
    }

    export() {
        return {
            player_id: this.playerId,
            kills: this.kills,
            deaths: this.deaths,
            assists: this.assists,
            headshots: this.headshots,
            mvps: this.mvps,
            aces: this.aces,
            fb_kills: this.fbKills,
            lb_kills: this.lbKills,
            trade_kills: this.tradeKills,
            trade_deaths: this.tradeDeaths,
            clutch_attempts: this.clutchAttempts,
            clutch_wins: this.clutchWins,
            one_v_one_wins: this.oneV1Wins,
            one_v_two_wins: this.oneV2Wins,
            one_v_three_wins: this.oneV3Wins,
            one_v_four_wins: this.oneV4Wins,
            one_v_five_wins: this.oneV5Wins,
            total_damage: this.totalDamage,
            total_assumed_damage: this.totalAssumedDamage,
            adr: this.getADR(),
            kd_ratio: this.getKDRatio(),
            headshot_percentage: this.getHeadshotPercentage(),
            best_weapon: this.getBestWeapon(),
            best_kill_streak: this.bestKillStreak,
            clutch_rate: this.getClutchRate(),
            utility_thrown: this.utilityThrown,
            utility_hits: this.utilityHits,
            flash_assists: this.flashAssists,
            enemies_flashed: this.enemiesFlashed,
            teammates_flashed: this.teammatesFlashed,
            smoke_blocks: this.smokeBlocks,
            weapon_kills: Object.fromEntries(this.killsByWeapon),
            weapon_deaths: Object.fromEntries(this.deathsByWeapon),
            damage_by_weapon: Object.fromEntries(this.damageByWeapon),
            damage_by_hit_zone: Object.fromEntries(this.damageByHitZone),
            shots: Object.fromEntries(Array.from(this.shots.entries()).map(([k, v]) => [k, v.export()]))
        };
    }

    import(data) {
        Object.assign(this, data);
        this.shots = new Map(Object.entries(data.shots || {}).map(([k, v]) => {
            const shot = new ShotAnalysis(v.weapon);
            Object.assign(shot, v);
            return [k, shot];
        }));
    }
}

export class CombatAnalyzer {
    constructor() {
        this.events = [];
        this.playerStats = new Map();
        this.roundStats = new Map();
        this.matchStats = {
            totalKills: 0,
            totalDeaths: 0,
            totalAssists: 0,
            totalDamage: 0,
            totalHeadshots: 0,
            totalUtility: 0,
            totalFlashes: 0,
            duration: 0,
            rounds: 0
        };
        this.heatmaps = new Map();
        this.killMap = new Map();
        this.damageMap = new Map();
        this.listeners = new Map();
        this.isRecording = false;
        this.recordingStartTime = 0;
    }

    startRecording() {
        this.isRecording = true;
        this.recordingStartTime = Date.now();
        this.events = [];
    }

    stopRecording() {
        this.isRecording = false;
        this.matchStats.duration = Date.now() - this.recordingStartTime;
    }

    recordEvent(type, data) {
        if (!this.isRecording) return;

        const event = new CombatEvent(type, data);
        this.events.push(event);
        this.emit('eventRecorded', event);
    }

    recordKill(killerId, victimId, weapon, distance, position, data) {
        this.recordEvent(CombatEventType.KILL, {
            attacker: killerId,
            victim: victimId,
            weapon,
            distance,
            position,
            ...data
        });

        const killerStats = this.getPlayerStats(killerId);
        killerStats.addKill(victimId, weapon, distance, position, data.isHeadshot, data.isTrade, data.isFB);

        const victimStats = this.getPlayerStats(victimId);
        victimStats.addDeath(killerId, weapon, distance, position, data.isTrade);

        const killKey = `${killerId}_${victimId}`;
        this.killMap.set(killKey, (this.killMap.get(killKey) || 0) + 1);

        this.matchStats.totalKills++;
    }

    recordDamage(attackerId, victimId, weapon, damage, hitZone, distance, isAssumed = false) {
        this.recordEvent(CombatEventType.DAMAGE_DEALT, {
            attacker: attackerId,
            victim: victimId,
            weapon,
            damage,
            hitZone,
            distance
        });

        const attackerStats = this.getPlayerStats(attackerId);
        attackerStats.recordDamage(damage, weapon, hitZone, distance, isAssumed);
        attackerStats.recordShot(weapon, true, distance, null, damage, hitZone);

        const victimStats = this.getPlayerStats(victimId);

        this.matchStats.totalDamage += damage;
    }

    recordShot(playerId, weapon, isHit, distance, position, damage, hitZone) {
        this.recordEvent(isHit ? CombatEventType.SHOT_HIT : CombatEventType.SHOT_FIRED, {
            player: playerId,
            weapon,
            isHit,
            distance,
            position,
            damage,
            hitZone
        });

        const stats = this.getPlayerStats(playerId);
        stats.recordShot(weapon, isHit, distance, position, damage, hitZone);
    }

    recordUtility(playerId, type, position) {
        this.recordEvent(CombatEventType.GRENADE_DAMAGE, {
            player: playerId,
            utilityType: type,
            position
        });

        const stats = this.getPlayerStats(playerId);
        stats.recordUtility(type);
    }

    recordClutch(playerId, won, enemiesRemaining) {
        const stats = this.getPlayerStats(playerId);
        stats.recordClutchAttempt();
        if (won) {
            stats.recordClutchWin(enemiesRemaining);
        }
    }

    getPlayerStats(playerId) {
        if (!this.playerStats.has(playerId)) {
            this.playerStats.set(playerId, new CombatStatistics(playerId));
        }
        return this.playerStats.get(playerId);
    }

    getMatchStats() {
        return { ...this.matchStats };
    }

    getEvents(type = null, playerId = null) {
        let filtered = [...this.events];

        if (type) {
            filtered = filtered.filter(e => e.type === type);
        }

        if (playerId) {
            filtered = filtered.filter(e => e.attacker === playerId || e.victim === playerId);
        }

        return filtered;
    }

    generateKillMatrix() {
        const matrix = {};

        for (const [key, count] of this.killMap) {
            const [killer, victim] = key.split('_');
            if (!matrix[killer]) matrix[killer] = {};
            matrix[killer][victim] = count;
        }

        return matrix;
    }

    generateDamageHeatmap(playerId) {
        const stats = this.getPlayerStats(playerId);
        const heatmap = new Map();

        for (const pos of stats.positionData) {
            const key = `${Math.floor(pos.position.x / 50)}_${Math.floor(pos.position.y / 50)}`;
            heatmap.set(key, (heatmap.get(key) || 0) + 1);
        }

        return heatmap;
    }

    generatePerformanceReport(playerId) {
        const stats = this.getPlayerStats(playerId);

        return {
            overview: {
                kills: stats.kills,
                deaths: stats.deaths,
                assists: stats.assists,
                kd: stats.getKDRatio().toFixed(2),
                hs_percentage: stats.getHeadshotPercentage().toFixed(1),
                adr: stats.getADR()
            },
            combat: {
                total_damage: stats.totalDamage,
                damage_per_shot: (stats.totalDamage / (stats.kills || 1)).toFixed(1),
                best_streak: stats.bestKillStreak,
                clutch_rate: stats.getClutchRate().toFixed(1)
            },
            weapons: {
                best: stats.getBestWeapon(),
                kills_by_weapon: Object.fromEntries(stats.killsByWeapon)
            },
            utility: {
                thrown: stats.utilityThrown,
                hits: stats.utilityHits,
                efficiency: this.calculateUtilityEfficiency(stats)
            }
        };
    }

    calculateUtilityEfficiency(stats) {
        const efficiency = {};

        for (const type of Object.keys(stats.utilityThrown)) {
            const thrown = stats.utilityThrown[type] || 0;
            const hits = stats.utilityHits[type] || 0;
            efficiency[type] = thrown > 0 ? (hits / thrown * 100).toFixed(1) : 0;
        }

        return efficiency;
    }

    export() {
        return {
            events: this.events.map(e => e.export()),
            player_stats: Object.fromEntries(
                Array.from(this.playerStats.entries()).map(([k, v]) => [k, v.export()])
            ),
            match_stats: this.matchStats,
            duration: this.matchStats.duration
        };
    }

    import(data) {
        this.events = data.events.map(e => CombatEvent.import(e));
        this.playerStats = new Map(Object.entries(data.player_stats).map(([k, v]) => {
            const stats = new CombatStatistics(k);
            stats.import(v);
            return [k, stats];
        }));
        this.matchStats = data.match_stats;
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
                console.error(`Error in combat analyzer listener: ${e}`);
            }
        }
    }
}

export class CombatSystem {
    static instance = null;

    constructor() {
        this.analyzer = new CombatAnalyzer();
        this.isActive = false;
        this.listeners = new Map();
    }

    static getInstance() {
        if (!CombatSystem.instance) {
            CombatSystem.instance = new CombatSystem();
        }
        return CombatSystem.instance;
    }

    start() {
        this.isActive = true;
        this.analyzer.startRecording();
    }

    stop() {
        this.isActive = false;
        this.analyzer.stopRecording();
    }

    recordKill(killerId, victimId, weapon, distance, position, data) {
        if (!this.isActive) return;
        this.analyzer.recordKill(killerId, victimId, weapon, distance, position, data);
    }

    recordDamage(attackerId, victimId, weapon, damage, hitZone, distance, isAssumed = false) {
        if (!this.isActive) return;
        this.analyzer.recordDamage(attackerId, victimId, weapon, damage, hitZone, distance, isAssumed);
    }

    recordShot(playerId, weapon, isHit, distance, position, damage, hitZone) {
        if (!this.isActive) return;
        this.analyzer.recordShot(playerId, weapon, isHit, distance, position, damage, hitZone);
    }

    recordUtility(playerId, type, position) {
        if (!this.isActive) return;
        this.analyzer.recordUtility(playerId, type, position);
    }

    recordClutch(playerId, won, enemiesRemaining) {
        if (!this.isActive) return;
        this.analyzer.recordClutch(playerId, won, enemiesRemaining);
    }

    getPlayerStats(playerId) {
        return this.analyzer.getPlayerStats(playerId);
    }

    getMatchStats() {
        return this.analyzer.getMatchStats();
    }

    getPerformanceReport(playerId) {
        return this.analyzer.generatePerformanceReport(playerId);
    }

    export() {
        return this.analyzer.export();
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
                console.error(`Error in combat system listener: ${e}`);
            }
        }
    }
}

export const Combat = CombatSystem.getInstance();

export class StatisticsCollector {
    constructor() {
        this.playerStats = new Map();
        this.matchStats = new Map();
        this.weaponStats = new Map();
        this.mapStats = new Map();
        this.roundStats = new Map();
        this.teamStats = new Map();
        this.aggregatedData = new AggregatedData();
        this.realTimeMetrics = new RealTimeMetrics();
        this.historicalData = new Map();
        this.retentionPeriod = 365 * 24 * 60 * 60 * 1000;
        this.collectionEnabled = true;
        this.samplingRate = 1.0;
        this.batchSize = 100;
        this.pendingUpdates = [];
    }

    initialize() {
        this.startCollection();
        this.startAggregation();
        this.startCleanup();
    }

    startCollection() {
        setInterval(() => {
            this.processPendingUpdates();
        }, 5000);

        setInterval(() => {
            this.calculateRealTimeMetrics();
        }, 1000);
    }

    startAggregation() {
        setInterval(() => {
            this.aggregateHourlyData();
        }, 60 * 60 * 1000);

        setInterval(() => {
            this.aggregateDailyData();
        }, 24 * 60 * 60 * 1000);
    }

    startCleanup() {
        setInterval(() => {
            this.cleanupOldData();
        }, 24 * 60 * 60 * 1000);
    }

    recordPlayerAction(playerId, action, data = {}) {
        if (!this.collectionEnabled) return;
        if (Math.random() > this.samplingRate) return;

        const playerStats = this.getOrCreatePlayerStats(playerId);
        
        if (!playerStats.actions) {
            playerStats.actions = [];
        }
        
        playerStats.actions.push({
            action,
            data,
            timestamp: Date.now()
        });

        this.processAction(action, playerStats, data);

        this.pendingUpdates.push({
            type: 'player_action',
            playerId,
            action,
            data,
            timestamp: Date.now()
        });
    }

    recordMatchStart(matchId, data) {
        const matchStats = {
            id: matchId,
            startTime: Date.now(),
            players: data.players || [],
            teams: data.teams || [],
            map: data.map,
            gameMode: data.gameMode,
            rounds: [],
            endTime: null,
            winner: null,
            score: { team1: 0, team2: 0 }
        };

        this.matchStats.set(matchId, matchStats);

        for (const playerId of data.players) {
            this.recordPlayerAction(playerId, 'match_start', {
                matchId,
                map: data.map,
                gameMode: data.gameMode
            });
        }

        return matchStats;
    }

    recordMatchEnd(matchId, winner, score) {
        const matchStats = this.matchStats.get(matchId);
        if (!matchStats) return;

        matchStats.endTime = Date.now();
        matchStats.winner = winner;
        matchStats.score = score;
        matchStats.duration = matchStats.endTime - matchStats.startTime;

        for (const playerId of matchStats.players) {
            const playerStats = this.getOrCreatePlayerStats(playerId);
            playerStats.totalMatches++;
            
            if (matchStats.teams[0].includes(playerId)) {
                const teamScore = score.team1;
                const enemyScore = score.team2;
                if ((winner === 'team1' && matchStats.teams[0].includes(playerId)) ||
                    (winner === 'team2' && matchStats.teams[1].includes(playerId))) {
                    playerStats.wins++;
                } else {
                    playerStats.losses++;
                }
            }

            this.recordPlayerAction(playerId, 'match_end', {
                matchId,
                winner,
                score,
                duration: matchStats.duration
            });
        }

        this.aggregatedData.recordMatch(matchStats);
    }

    recordRoundStart(matchId, roundNumber, data = {}) {
        const matchStats = this.matchStats.get(matchId);
        if (!matchStats) return;

        const roundStats = {
            matchId,
            roundNumber,
            startTime: Date.now(),
            endTime: null,
            winner: null,
            endCondition: null,
            bombPlanted: false,
            bombDefused: false,
            bombExploded: false,
            players: [],
            events: []
        };

        matchStats.rounds.push(roundStats);
        this.roundStats.set(`${matchId}_${roundNumber}`, roundStats);

        return roundStats;
    }

    recordRoundEnd(matchId, roundNumber, winner, endCondition, data = {}) {
        const roundStats = this.roundStats.get(`${matchId}_${roundNumber}`);
        if (!roundStats) return;

        roundStats.endTime = Date.now();
        roundStats.winner = winner;
        roundStats.endCondition = endCondition;
        roundStats.duration = roundStats.endTime - roundStats.startTime;
        roundStats.extraData = data;

        const matchStats = this.matchStats.get(matchId);
        if (matchStats) {
            if (winner === 'team1') {
                matchStats.score.team1++;
            } else if (winner === 'team2') {
                matchStats.score.team2++;
            }
        }

        for (const event of roundStats.events) {
            this.processRoundEvent(event, roundStats);
        }
    }

    recordKill(matchId, roundNumber, killer, victim, weapon, data = {}) {
        const roundStats = this.roundStats.get(`${matchId}_${roundNumber}`);
        if (!roundStats) return;

        const killEvent = {
            type: 'kill',
            timestamp: Date.now(),
            killer,
            victim,
            weapon,
            headshot: data.headshot || false,
            throughWall: data.throughWall || false,
            distance: data.distance || 0,
            position: data.position
        };

        roundStats.events.push(killEvent);

        const killerStats = this.getOrCreatePlayerStats(killer);
        killerStats.kills++;
        if (killEvent.headshot) killerStats.headshots++;

        this.updateWeaponStats(weapon, 'kills', 1);
        this.updateWeaponStats(weapon, 'headshots', killEvent.headshot ? 1 : 0);

        const victimStats = this.getOrCreatePlayerStats(victim);
        victimStats.deaths++;

        this.recordPlayerAction(killer, 'kill', {
            matchId,
            roundNumber,
            victim,
            weapon,
            headshot: killEvent.headshot,
            throughWall: killEvent.throughWall
        });

        return killEvent;
    }

    recordDamage(matchId, roundNumber, attacker, victim, damage, weapon, data = {}) {
        const roundStats = this.roundStats.get(`${matchId}_${roundNumber}`);
        if (!roundStats) return;

        const damageEvent = {
            type: 'damage',
            timestamp: Date.now(),
            attacker,
            victim,
            damage,
            weapon,
            hitgroup: data.hitgroup || 'body',
            friendly: data.friendly || false
        };

        roundStats.events.push(damageEvent);

        const attackerStats = this.getOrCreatePlayerStats(attacker);
        attackerStats.damageDealt += damage;

        this.updateWeaponStats(weapon, 'damage', damage);
        this.updateWeaponStats(weapon, 'hits', 1);

        const victimStats = this.getOrCreatePlayerStats(victim);
        victimStats.damageTaken += damage;

        return damageEvent;
    }

    recordAssist(matchId, roundNumber, playerId, data = {}) {
        const roundStats = this.roundStats.get(`${matchId}_${roundNumber}`);
        if (!roundStats) return;

        const assistEvent = {
            type: 'assist',
            timestamp: Date.now(),
            playerId,
            assistedKill: data.assistedKill
        };

        roundStats.events.push(assistEvent);

        const playerStats = this.getOrCreatePlayerStats(playerId);
        playerStats.assists++;

        return assistEvent;
    }

    recordObjective(matchId, roundNumber, playerId, objectiveType, data = {}) {
        const roundStats = this.roundStats.get(`${matchId}_${roundNumber}`);
        if (!roundStats) return;

        const objectiveEvent = {
            type: 'objective',
            timestamp: Date.now(),
            playerId,
            objectiveType,
            data
        };

        roundStats.events.push(objectiveEvent);

        const playerStats = this.getOrCreatePlayerStats(playerId);
        
        switch (objectiveType) {
            case 'bomb_planted':
                playerStats.plants++;
                roundStats.bombPlanted = true;
                break;
            case 'bomb_defused':
                playerStats.defusals++;
                roundStats.bombDefused = true;
                break;
            case 'bomb_exploded':
                roundStats.bombExploded = true;
                break;
            case 'hostage_pickup':
                playerStats.hostageRescues++;
                break;
            case 'hostage_rescue':
                playerStats.hostageRescues++;
                break;
        }

        return objectiveEvent;
    }

    processAction(action, playerStats, data) {
        switch (action) {
            case 'flashThrown':
                playerStats.flashbangsThrown++;
                break;
            case 'smokeThrown':
                playerStats.smokeGrenadesThrown++;
                break;
            case 'heGrenadeThrown':
                playerStats.grenadesThrown++;
                break;
            case 'fireBombThrown':
                playerStats.fireBombThrown++;
                break;
            case 'decoyThrown':
                playerStats.decoyThrown++;
                break;
            case 'entryKill':
                playerStats.entryKills++;
                break;
            case 'clutchWon':
                playerStats.clutchesWon++;
                break;
            case 'tradeKill':
                playerStats.tradeKills++;
                break;
            case 'knifeKill':
                playerStats.knifeKills++;
                break;
        }
    }

    processRoundEvent(event, roundStats) {
        if (event.type === 'kill') {
            const playerStats = this.getOrCreatePlayerStats(event.killer);
            playerStats.shotsHit++;
            
            if (event.headshot) {
                playerStats.headshotShots++;
            }
        }
    }

    processPendingUpdates() {
        if (this.pendingUpdates.length === 0) return;

        const batch = this.pendingUpdates.splice(0, this.batchSize);
        
        for (const update of batch) {
            this.processUpdate(update);
        }
    }

    processUpdate(update) {
        switch (update.type) {
            case 'player_action':
                break;
        }
    }

    calculateRealTimeMetrics() {
        const now = Date.now();
        
        this.realTimeMetrics.concurrentPlayers = this.countActivePlayers(now);
        this.realTimeMetrics.matchesInProgress = this.matchStats.size;
        this.realTimeMetrics.averageMatchDuration = this.calculateAverageMatchDuration();
        this.realTimeMetrics.killsPerMinute = this.calculateKillsPerMinute();
    }

    countActivePlayers(timestamp) {
        let count = 0;
        for (const [matchId, stats] of this.matchStats) {
            if (stats.startTime <= timestamp && (!stats.endTime || stats.endTime > timestamp)) {
                count += stats.players.length;
            }
        }
        return count;
    }

    calculateAverageMatchDuration() {
        if (this.matchStats.size === 0) return 0;
        
        let totalDuration = 0;
        let count = 0;
        
        for (const stats of this.matchStats.values()) {
            if (stats.duration) {
                totalDuration += stats.duration;
                count++;
            }
        }
        
        return count > 0 ? totalDuration / count : 0;
    }

    calculateKillsPerMinute() {
        const lastMinute = Date.now() - 60000;
        let kills = 0;
        
        for (const [matchId, stats] of this.matchStats) {
            for (const round of stats.rounds) {
                for (const event of round.events) {
                    if (event.type === 'kill' && event.timestamp > lastMinute) {
                        kills++;
                    }
                }
            }
        }
        
        return kills;
    }

    getOrCreatePlayerStats(playerId) {
        if (!this.playerStats.has(playerId)) {
            this.playerStats.set(playerId, this.createDefaultPlayerStats(playerId));
        }
        return this.playerStats.get(playerId);
    }

    createDefaultPlayerStats(playerId) {
        return {
            playerId,
            kills: 0,
            deaths: 0,
            assists: 0,
            headshots: 0,
            damageDealt: 0,
            damageTaken: 0,
            totalMatches: 0,
            wins: 0,
            losses: 0,
            draws: 0,
            plants: 0,
            defusals: 0,
            hostageRescues: 0,
            flashbangsThrown: 0,
            smokeGrenadesThrown: 0,
            grenadesThrown: 0,
            fireBombThrown: 0,
            decoyThrown: 0,
            entryKills: 0,
            clutchesWon: 0,
            tradeKills: 0,
            knifeKills: 0,
            shotsHit: 0,
            shotsFired: 0,
            totalPlaytime: 0,
            totalRoundsPlayed: 0,
            actions: [],
            weaponUsage: new Map(),
            mapPerformance: new Map(),
            recentMatches: [],
            rankHistory: [],
            createdAt: Date.now(),
            lastMatchAt: null
        };
    }

    updateWeaponStats(weapon, stat, value) {
        if (!this.weaponStats.has(weapon)) {
            this.weaponStats.set(weapon, {
                weapon,
                kills: 0,
                headshots: 0,
                damage: 0,
                hits: 0,
                shotsFired: 0,
                timesUsed: 0,
                winRate: 0
            });
        }

        const stats = this.weaponStats.get(weapon);
        if (stat in stats) {
            stats[stat] += value;
        }
    }

    aggregateHourlyData() {
        const now = Date.now();
        const hourStart = now - 60 * 60 * 1000;
        const hourKey = new Date(hourStart).toISOString();

        const hourlyData = {
            timestamp: hourStart,
            totalMatches: 0,
            totalKills: 0,
            totalPlayers: new Set(),
            uniqueMaps: new Set(),
            averageDuration: 0,
            peakConcurrentPlayers: 0
        };

        for (const [matchId, stats] of this.matchStats) {
            if (stats.startTime >= hourStart && stats.startTime < now) {
                hourlyData.totalMatches++;
                hourlyData.totalKills += stats.rounds.reduce((sum, r) => 
                    sum + r.events.filter(e => e.type === 'kill').length, 0);
                
                for (const playerId of stats.players) {
                    hourlyData.totalPlayers.add(playerId);
                }
                hourlyData.uniqueMaps.add(stats.map);
            }
        }

        this.aggregatedData.hourlyData.set(hourKey, {
            ...hourlyData,
            totalPlayers: hourlyData.totalPlayers.size,
            uniqueMaps: Array.from(hourlyData.uniqueMaps)
        });
    }

    aggregateDailyData() {
        const now = Date.now();
        const dayStart = now - 24 * 60 * 60 * 1000;
        const dayKey = new Date(dayStart).toDateString();

        const dailyData = {
            timestamp: dayStart,
            totalMatches: 0,
            totalKills: 0,
            uniquePlayers: 0,
            peakConcurrentPlayers: 0,
            averageMatchDuration: 0,
            mostPlayedMap: null,
            mostUsedWeapon: null,
            topPlayers: []
        };

        for (const [matchId, stats] of this.matchStats) {
            if (stats.startTime >= dayStart) {
                dailyData.totalMatches++;
                dailyData.totalKills += stats.rounds.reduce((sum, r) => 
                    sum + r.events.filter(e => e.type === 'kill').length, 0);
            }
        }

        const playerCounts = new Map();
        for (const [playerId, stats] of this.playerStats) {
            playerCounts.set(playerId, stats.totalMatches);
        }

        dailyData.topPlayers = Array.from(playerCounts.entries())
            .sort((a, b) => b[1] - a[1])
            .slice(0, 10)
            .map(([playerId, matches]) => ({ playerId, matches }));

        this.aggregatedData.dailyData.set(dayKey, dailyData);
    }

    cleanupOldData() {
        const cutoff = Date.now() - this.retentionPeriod;

        for (const [key, stats] of this.matchStats) {
            if (stats.endTime && stats.endTime < cutoff) {
                this.matchStats.delete(key);
            }
        }

        for (const [playerId, stats] of this.playerStats) {
            if (stats.lastMatchAt && stats.lastMatchAt < cutoff) {
                stats.actions = stats.actions.filter(a => a.timestamp > cutoff);
            }
        }
    }

    getPlayerStats(playerId) {
        return this.playerStats.get(playerId);
    }

    getPlayerDetailedStats(playerId) {
        const stats = this.getOrCreatePlayerStats(playerId);
        
        const kd = stats.deaths > 0 ? stats.kills / stats.deaths : stats.kills;
        const hsRate = stats.kills > 0 ? (stats.headshots / stats.kills) * 100 : 0;
        const winRate = stats.totalMatches > 0 ? (stats.wins / stats.totalMatches) * 100 : 0;
        const accuracy = stats.shotsFired > 0 ? (stats.shotsHit / stats.shotsFired) * 100 : 0;

        return {
            ...stats,
            kd,
            headshotRate: hsRate,
            winRate,
            accuracy,
            averageKillsPerMatch: stats.totalMatches > 0 ? stats.kills / stats.totalMatches : 0,
            averageDamagePerRound: stats.totalRoundsPlayed > 0 ? stats.damageDealt / stats.totalRoundsPlayed : 0,
            favoriteWeapon: this.getMostUsedWeapon(playerId),
            bestMap: this.getBestMap(playerId),
            recentPerformance: this.getRecentPerformance(playerId)
        };
    }

    getMostUsedWeapon(playerId) {
        const stats = this.getOrCreatePlayerStats(playerId);
        if (!stats.weaponUsage || stats.weaponUsage.size === 0) return null;

        let maxWeapon = null;
        let maxKills = 0;

        for (const [weapon, kills] of stats.weaponUsage) {
            if (kills > maxKills) {
                maxKills = kills;
                maxWeapon = weapon;
            }
        }

        return maxWeapon;
    }

    getBestMap(playerId) {
        const stats = this.getOrCreatePlayerStats(playerId);
        if (!stats.mapPerformance || stats.mapPerformance.size === 0) return null;

        let bestMap = null;
        let bestWinRate = 0;

        for (const [map, performance] of stats.mapPerformance) {
            if (performance.wins + performance.losses >= 5) {
                const winRate = performance.wins / (performance.wins + performance.losses);
                if (winRate > bestWinRate) {
                    bestWinRate = winRate;
                    bestMap = map;
                }
            }
        }

        return bestMap;
    }

    getRecentPerformance(playerId, lastN = 10) {
        const stats = this.getOrCreatePlayerStats(playerId);
        return stats.recentMatches.slice(-lastN).map(matchId => {
            return this.matchStats.get(matchId);
        }).filter(Boolean);
    }

    getGlobalStats() {
        return {
            totalPlayers: this.playerStats.size,
            totalMatches: this.matchStats.size,
            totalKills: this.aggregatedData.totalKills,
            totalRounds: this.aggregatedData.totalRounds,
            averageKD: this.calculateGlobalAverageKD(),
            averageWinRate: this.calculateGlobalAverageWinRate(),
            mostPopularMap: this.getMostPopularMap(),
            mostPopularWeapon: this.getMostPopularWeapon(),
            peakPlayersToday: this.realTimeMetrics.peakConcurrentPlayers
        };
    }

    calculateGlobalAverageKD() {
        let totalKills = 0;
        let totalDeaths = 0;

        for (const stats of this.playerStats.values()) {
            totalKills += stats.kills;
            totalDeaths += stats.deaths;
        }

        return totalDeaths > 0 ? totalKills / totalDeaths : 0;
    }

    calculateGlobalAverageWinRate() {
        let totalWins = 0;
        let totalMatches = 0;

        for (const stats of this.playerStats.values()) {
            totalWins += stats.wins;
            totalMatches += stats.totalMatches;
        }

        return totalMatches > 0 ? totalWins / totalMatches : 0;
    }

    getMostPopularMap() {
        const mapCounts = new Map();

        for (const stats of this.matchStats.values()) {
            const count = mapCounts.get(stats.map) || 0;
            mapCounts.set(stats.map, count + 1);
        }

        let maxMap = null;
        let maxCount = 0;

        for (const [map, count] of mapCounts) {
            if (count > maxCount) {
                maxCount = count;
                maxMap = map;
            }
        }

        return maxMap;
    }

    getMostPopularWeapon() {
        let maxWeapon = null;
        let maxKills = 0;

        for (const [weapon, stats] of this.weaponStats) {
            if (stats.kills > maxKills) {
                maxKills = stats.kills;
                maxWeapon = weapon;
            }
        }

        return maxWeapon;
    }

    serialize() {
        return {
            playerStats: Array.from(this.playerStats.entries()),
            matchStats: Array.from(this.matchStats.entries()),
            weaponStats: Array.from(this.weaponStats.entries()),
            aggregatedData: this.aggregatedData.serialize(),
            realTimeMetrics: this.realTimeMetrics.serialize(),
            collectionEnabled: this.collectionEnabled,
            samplingRate: this.samplingRate
        };
    }
}

class AggregatedData {
    constructor() {
        this.totalKills = 0;
        this.totalDeaths = 0;
        this.totalRounds = 0;
        this.totalMatches = 0;
        this.hourlyData = new Map();
        this.dailyData = new Map();
        this.weeklyData = new Map();
        this.monthlyData = new Map();
    }

    recordMatch(matchStats) {
        this.totalMatches++;
        this.totalRounds += matchStats.rounds.length;
        
        for (const round of matchStats.rounds) {
            const kills = round.events.filter(e => e.type === 'kill').length;
            this.totalKills += kills;
        }
    }

    serialize() {
        return {
            totalKills: this.totalKills,
            totalDeaths: this.totalDeaths,
            totalRounds: this.totalRounds,
            totalMatches: this.totalMatches,
            hourlyData: Array.from(this.hourlyData.entries()),
            dailyData: Array.from(this.dailyData.entries())
        };
    }
}

class RealTimeMetrics {
    constructor() {
        this.concurrentPlayers = 0;
        this.matchesInProgress = 0;
        this.averageMatchDuration = 0;
        this.killsPerMinute = 0;
        this.peakConcurrentPlayers = 0;
        this.peakTime = null;
        this.lastUpdate = Date.now();
    }

    updateMetric(name, value) {
        this[name] = value;
        this.lastUpdate = Date.now();

        if (name === 'concurrentPlayers' && value > this.peakConcurrentPlayers) {
            this.peakConcurrentPlayers = value;
            this.peakTime = Date.now();
        }
    }

    serialize() {
        return {
            concurrentPlayers: this.concurrentPlayers,
            matchesInProgress: this.matchesInProgress,
            averageMatchDuration: this.averageMatchDuration,
            killsPerMinute: this.killsPerMinute,
            peakConcurrentPlayers: this.peakConcurrentPlayers,
            peakTime: this.peakTime,
            lastUpdate: this.lastUpdate
        };
    }
}

export class AnalyticsDashboard {
    constructor(statisticsCollector) {
        this.collector = statisticsCollector;
        this.widgets = new Map();
        this.filters = new AnalyticsFilters();
        this.timeRange = '7d';
        this.comparisonEnabled = false;
        this.refreshInterval = 60000;
        this.exportFormats = ['json', 'csv', 'pdf'];
    }

    initialize() {
        this.createDefaultWidgets();
        this.startAutoRefresh();
    }

    createDefaultWidgets() {
        this.widgets.set('player_count', new LineChartWidget('Active Players', 'time'));
        this.widgets.set('match_count', new BarChartWidget('Matches Played', 'hour'));
        this.widgets.set('kill_distribution', new PieChartWidget('Kill Distribution', 'weapon'));
        this.widgets.set('map_popularity', new BarChartWidget('Map Popularity', 'map'));
        this.widgets.set('win_rate', new LineChartWidget('Win Rate', 'time'));
        this.widgets.set('kd_ratio', new LineChartWidget('K/D Ratio', 'time'));
        this.widgets.set('weapon_usage', new HorizontalBarWidget('Weapon Usage', 'weapon'));
        this.widgets.set('peak_hours', new HeatmapWidget('Peak Hours', 'hour'));
        this.widgets.set('geographic', new MapWidget('Geographic Distribution', 'region'));
        this.widgets.set('rank_distribution', new BarChartWidget('Rank Distribution', 'rank'));
    }

    startAutoRefresh() {
        setInterval(() => {
            this.refreshAllWidgets();
        }, this.refreshInterval);
    }

    refreshAllWidgets() {
        for (const [name, widget] of this.widgets) {
            const data = this.collectDataForWidget(name);
            widget.updateData(data);
        }
    }

    collectDataForWidget(widgetName) {
        const globalStats = this.collector.getGlobalStats();
        
        switch (widgetName) {
            case 'player_count':
                return this.getPlayerCountTimeline();
            case 'match_count':
                return this.getMatchCountByPeriod();
            case 'kill_distribution':
                return this.getKillDistributionByWeapon();
            case 'map_popularity':
                return this.getMapPopularity();
            case 'win_rate':
                return this.getWinRateTimeline();
            case 'kd_ratio':
                return this.getKDRatioTimeline();
            case 'weapon_usage':
                return this.getWeaponUsageData();
            case 'peak_hours':
                return this.getPeakHoursData();
            case 'geographic':
                return this.getGeographicData();
            case 'rank_distribution':
                return this.getRankDistribution();
            default:
                return {};
        }
    }

    getPlayerCountTimeline() {
        const data = [];
        const now = Date.now();
        
        for (let i = 24; i >= 0; i--) {
            const timestamp = now - i * 60 * 60 * 1000;
            data.push({
                timestamp,
                value: Math.floor(Math.random() * 1000) + 500
            });
        }
        
        return data;
    }

    getMatchCountByPeriod() {
        const data = [];
        const now = Date.now();
        
        for (let i = 23; i >= 0; i--) {
            const hourStart = now - i * 60 * 60 * 1000;
            const hourEnd = hourStart + 60 * 60 * 1000;
            let count = 0;
            
            for (const [matchId, stats] of this.collector.matchStats) {
                if (stats.startTime >= hourStart && stats.startTime < hourEnd) {
                    count++;
                }
            }
            
            data.push({
                label: new Date(hourStart).getHours() + ':00',
                value: count
            });
        }
        
        return data;
    }

    getKillDistributionByWeapon() {
        const distribution = [];
        
        for (const [weapon, stats] of this.collector.weaponStats) {
            distribution.push({
                label: weapon,
                value: stats.kills,
                percentage: 0
            });
        }
        
        const total = distribution.reduce((sum, d) => sum + d.value, 0);
        for (const d of distribution) {
            d.percentage = total > 0 ? (d.value / total) * 100 : 0;
        }
        
        return distribution.sort((a, b) => b.value - a.value);
    }

    getMapPopularity() {
        const mapCounts = new Map();
        
        for (const [matchId, stats] of this.collector.matchStats) {
            const count = mapCounts.get(stats.map) || 0;
            mapCounts.set(stats.map, count + 1);
        }
        
        const data = [];
        for (const [map, count] of mapCounts) {
            data.push({
                label: map,
                value: count
            });
        }
        
        return data.sort((a, b) => b.value - a.value);
    }

    getWinRateTimeline() {
        const data = [];
        const now = Date.now();
        
        for (let i = 30; i >= 0; i--) {
            const timestamp = now - i * 24 * 60 * 60 * 1000;
            data.push({
                timestamp,
                value: Math.random() * 0.4 + 0.3
            });
        }
        
        return data;
    }

    getKDRatioTimeline() {
        const data = [];
        const now = Date.now();
        
        for (let i = 30; i >= 0; i--) {
            const timestamp = now - i * 24 * 60 * 60 * 1000;
            data.push({
                timestamp,
                value: Math.random() * 0.8 + 0.6
            });
        }
        
        return data;
    }

    getWeaponUsageData() {
        const data = [];
        
        for (const [weapon, stats] of this.collector.weaponStats) {
            data.push({
                label: weapon,
                value: stats.timesUsed
            });
        }
        
        return data.sort((a, b) => b.value - a.value);
    }

    getPeakHoursData() {
        const data = [];
        
        for (let hour = 0; hour < 24; hour++) {
            data.push({
                hour,
                monday: Math.floor(Math.random() * 100),
                tuesday: Math.floor(Math.random() * 100),
                wednesday: Math.floor(Math.random() * 100),
                thursday: Math.floor(Math.random() * 100),
                friday: Math.floor(Math.random() * 100),
                saturday: Math.floor(Math.random() * 100),
                sunday: Math.floor(Math.random() * 100)
            });
        }
        
        return data;
    }

    getGeographicData() {
        return [
            { region: 'North America', players: 25000, percentage: 35 },
            { region: 'Europe', players: 20000, percentage: 28 },
            { region: 'Asia', players: 15000, percentage: 21 },
            { region: 'South America', players: 6000, percentage: 8 },
            { region: 'Oceania', players: 3000, percentage: 4 },
            { region: 'Other', players: 3000, percentage: 4 }
        ];
    }

    getRankDistribution() {
        return [
            { rank: 'Silver I', players: 5000 },
            { rank: 'Silver II', players: 6000 },
            { rank: 'Silver III', players: 5500 },
            { rank: 'Gold I', players: 8000 },
            { rank: 'Gold II', players: 7000 },
            { rank: 'Gold III', players: 6500 },
            { rank: 'Master', players: 4000 },
            { rank: 'Diamond', players: 3000 },
            { rank: 'Elite', players: 1500 },
            { rank: 'Champion', players: 500 }
        ];
    }

    exportData(format = 'json') {
        const data = {
            timestamp: Date.now(),
            timeRange: this.timeRange,
            filters: this.filters.serialize(),
            globalStats: this.collector.getGlobalStats(),
            widgets: {}
        };

        for (const [name, widget] of this.widgets) {
            data.widgets[name] = this.collectDataForWidget(name);
        }

        switch (format) {
            case 'json':
                return JSON.stringify(data, null, 2);
            case 'csv':
                return this.convertToCSV(data);
            case 'pdf':
                return this.generatePDFData(data);
            default:
                return data;
        }
    }

    convertToCSV(data) {
        let csv = '';
        
        for (const [widgetName, widgetData] of Object.entries(data.widgets)) {
            csv += `\n# ${widgetName}\n`;
            
            if (Array.isArray(widgetData) && widgetData.length > 0) {
                const headers = Object.keys(widgetData[0]);
                csv += headers.join(',') + '\n';
                
                for (const row of widgetData) {
                    csv += headers.map(h => row[h]).join(',') + '\n';
                }
            }
        }
        
        return csv;
    }

    generatePDFData(data) {
        return {
            title: 'Analytics Report',
            generatedAt: new Date().toISOString(),
            data: data
        };
    }
}

class AnalyticsFilters {
    constructor() {
        this.ranks = [];
        this.regions = [];
        this.maps = [];
        this.gameModes = [];
        this.dateRange = {
            start: Date.now() - 7 * 24 * 60 * 60 * 1000,
            end: Date.now()
        };
        this.playerTypes = [];
        this.teamSizes = [];
    }

    setRankFilter(ranks) {
        this.ranks = ranks;
    }

    setRegionFilter(regions) {
        this.regions = regions;
    }

    setMapFilter(maps) {
        this.maps = maps;
    }

    setDateRange(start, end) {
        this.dateRange = { start, end };
    }

    clearFilters() {
        this.ranks = [];
        this.regions = [];
        this.maps = [];
        this.gameModes = [];
        this.playerTypes = [];
        this.teamSizes = [];
    }

    serialize() {
        return {
            ranks: this.ranks,
            regions: this.regions,
            maps: this.maps,
            gameModes: this.gameModes,
            dateRange: this.dateRange,
            playerTypes: this.playerTypes,
            teamSizes: this.teamSizes
        };
    }
}

class LineChartWidget {
    constructor(title, dataType) {
        this.title = title;
        this.dataType = dataType;
        this.data = [];
        this.options = {
            showLegend: true,
            showGrid: true,
            smooth: true,
            fillArea: false
        };
    }

    updateData(data) {
        this.data = data;
    }
}

class BarChartWidget {
    constructor(title, dataType) {
        this.title = title;
        this.dataType = dataType;
        this.data = [];
        this.options = {
            horizontal: false,
            showValues: true,
            barSpacing: 2
        };
    }

    updateData(data) {
        this.data = data;
    }
}

class PieChartWidget {
    constructor(title, dataType) {
        this.title = title;
        this.dataType = dataType;
        this.data = [];
        this.options = {
            showLegend: true,
            showPercentages: true,
            donut: false
        };
    }

    updateData(data) {
        this.data = data;
    }
}

class HorizontalBarWidget {
    constructor(title, dataType) {
        this.title = title;
        this.dataType = dataType;
        this.data = [];
        this.options = {
            showValues: true,
            showPercentages: true
        };
    }

    updateData(data) {
        this.data = data;
    }
}

class HeatmapWidget {
    constructor(title, dataType) {
        this.title = title;
        this.dataType = dataType;
        this.data = [];
        this.options = {
            colorScale: 'default',
            showLegend: true
        };
    }

    updateData(data) {
        this.data = data;
    }
}

class MapWidget {
    constructor(title, dataType) {
        this.title = title;
        this.dataType = dataType;
        this.data = [];
        this.options = {
            projection: 'mercator',
            showMarkers: true
        };
    }

    updateData(data) {
        this.data = data;
    }
}

export default {
    StatisticsCollector,
    AnalyticsDashboard,
    AggregatedData,
    RealTimeMetrics
};

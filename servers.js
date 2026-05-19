export class ServerBrowser {
    constructor() {
        this.servers = new Map();
        this.filters = new ServerFilters();
        this.favorites = new Set();
        this.history = [];
        this.pingCache = new Map();
        this.refreshInterval = 30000;
        this.autoRefresh = true;
        this.maxServers = 10000;
    }

    initialize() {
        this.startAutoRefresh();
    }

    addServer(serverData) {
        const server = new GameServer(serverData);
        this.servers.set(server.id, server);
        return server;
    }

    removeServer(serverId) {
        return this.servers.delete(serverId);
    }

    getServer(serverId) {
        return this.servers.get(serverId);
    }

    searchServers(query, options = {}) {
        const results = [];
        const queryLower = query.toLowerCase();

        for (const server of this.servers.values()) {
            if (this.matchesServer(server, queryLower, options)) {
                results.push(server);
            }
        }

        results.sort((a, b) => this.compareServers(a, b, options.sortBy || 'ping'));

        const page = options.page || 1;
        const pageSize = options.pageSize || 50;
        const start = (page - 1) * pageSize;

        return {
            servers: results.slice(start, start + pageSize),
            total: results.length,
            page,
            pageSize,
            totalPages: Math.ceil(results.length / pageSize)
        };
    }

    matchesServer(server, query, options) {
        if (options.gameMode && server.gameMode !== options.gameMode) return false;
        if (options.map && server.map !== options.map) return false;
        if (options.region && server.region !== options.region) return false;
        if (options.maxPing && server.ping > options.maxPing) return false;
        if (options.minPlayers && server.playerCount < options.minPlayers) return false;
        if (options.maxPlayers && server.playerCount > options.maxPlayers) return false;
        if (options.hasVac && !server.hasVAC) return false;
        if (options.hasPassword && !server.hasPassword) return false;
        if (options.hasBots && server.botCount > 0) return false;
        if (options.full && server.playerCount >= server.maxPlayers) return false;
        if (options.empty && server.playerCount > 0) return false;
        if (options.gameType && server.gameType !== options.gameType) return false;

        if (query) {
            const nameMatch = server.name.toLowerCase().includes(query);
            const mapMatch = server.map.toLowerCase().includes(query);
            const addressMatch = server.address.includes(query);
            return nameMatch || mapMatch || addressMatch;
        }

        return true;
    }

    compareServers(a, b, sortBy) {
        switch (sortBy) {
            case 'ping':
                return a.ping - b.ping;
            case 'name':
                return a.name.localeCompare(b.name);
            case 'players':
                return b.playerCount - a.playerCount;
            case 'map':
                return a.map.localeCompare(b.map);
            case 'gameMode':
                return a.gameMode.localeCompare(b.gameMode);
            default:
                return a.ping - b.ping;
        }
    }

    getServersByRegion(region) {
        return Array.from(this.servers.values())
            .filter(s => s.region === region && s.playerCount < s.maxPlayers);
    }

    getServersByMap(mapName) {
        return Array.from(this.servers.values())
            .filter(s => s.map === mapName);
    }

    getServersByGameMode(gameMode) {
        return Array.from(this.servers.values())
            .filter(s => s.gameMode === gameMode);
    }

    getLANServers() {
        return Array.from(this.servers.values())
            .filter(s => s.isLAN);
    }

    getInternetServers() {
        return Array.from(this.servers.values())
            .filter(s => !s.isLAN);
    }

    addToFavorites(serverId) {
        this.favorites.add(serverId);
        const server = this.servers.get(serverId);
        if (server) {
            server.isFavorite = true;
        }
    }

    removeFromFavorites(serverId) {
        this.favorites.delete(serverId);
        const server = this.servers.get(serverId);
        if (server) {
            server.isFavorite = false;
        }
    }

    getFavorites() {
        return Array.from(this.favorites)
            .map(id => this.servers.get(id))
            .filter(Boolean);
    }

    refreshServer(serverId) {
        const server = this.servers.get(serverId);
        if (server) {
            server.lastRefresh = Date.now();
        }
    }

    refreshAllServers() {
        for (const server of this.servers.values()) {
            server.lastRefresh = Date.now();
        }
    }

    startAutoRefresh() {
        if (this.autoRefresh) {
            setInterval(() => {
                this.refreshAllServers();
            }, this.refreshInterval);
        }
    }

    stopAutoRefresh() {
        this.autoRefresh = false;
    }

    updatePing(serverId, ping) {
        this.pingCache.set(serverId, {
            ping,
            timestamp: Date.now()
        });

        const server = this.servers.get(serverId);
        if (server) {
            server.ping = ping;
        }
    }

    getRecentServers(limit = 10) {
        return this.history.slice(-limit);
    }

    serialize() {
        return {
            servers: Array.from(this.servers.values()).map(s => s.serialize()),
            filters: this.filters.serialize(),
            favorites: Array.from(this.favorites),
            history: this.history,
            autoRefresh: this.autoRefresh
        };
    }
}

export class GameServer {
    constructor(data) {
        this.id = data.id || 'server_' + Date.now();
        this.name = data.name;
        this.address = data.address;
        this.port = data.port || 27015;
        this.region = data.region;
        this.country = data.country;
        this.gameMode = data.gameMode;
        this.map = data.map;
        this.gameType = data.gameType;
        this.playerCount = data.playerCount || 0;
        this.maxPlayers = data.maxPlayers || 10;
        this.botCount = data.botCount || 0;
        this.ping = data.ping || 999;
        this.hasPassword = data.hasPassword || false;
        this.hasVAC = data.hasVAC ?? true;
        this.isLAN = data.isLAN || false;
        this.isFavorite = false;
        this.lastRefresh = Date.now();
        this.lastPlayed = null;
        this.tags = data.tags || [];
        this.version = data.version;
        this.vars = data.vars || {};
        this.players = [];
        this.ranked = data.ranked ?? false;
        this.tournamentMode = data.tournamentMode ?? false;
        this.allowCheats = data.allowCheats ?? false;
        this.tickrate = data.tickrate || 64;
        this.os = data.os || 'windows';
    }

    addPlayer(playerData) {
        this.players.push(playerData);
        this.playerCount = this.players.length;
    }

    removePlayer(playerId) {
        this.players = this.players.filter(p => p.id !== playerId);
        this.playerCount = this.players.length;
    }

    getAvailableSlots() {
        return this.maxPlayers - this.playerCount;
    }

    isEmpty() {
        return this.playerCount === 0;
    }

    isFull() {
        return this.playerCount >= this.maxPlayers;
    }

    hasSpace(minimum = 1) {
        return this.getAvailableSlots() >= minimum;
    }

    getUtilization() {
        return this.maxPlayers > 0 ? (this.playerCount / this.maxPlayers) * 100 : 0;
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            address: this.address,
            port: this.port,
            region: this.region,
            country: this.country,
            gameMode: this.gameMode,
            map: this.map,
            playerCount: this.playerCount,
            maxPlayers: this.maxPlayers,
            botCount: this.botCount,
            ping: this.ping,
            hasPassword: this.hasPassword,
            hasVAC: this.hasVAC,
            isLAN: this.isLAN,
            isFavorite: this.isFavorite,
            lastRefresh: this.lastRefresh,
            tags: this.tags,
            ranked: this.ranked,
            tickrate: this.tickrate
        };
    }
}

export class ServerFilters {
    constructor() {
        this.gameMode = null;
        this.map = null;
        this.region = null;
        this.maxPing = null;
        this.minPlayers = 0;
        this.maxPlayers = null;
        this.hasVac = true;
        this.hasPassword = false;
        this.hasBots = false;
        this.full = false;
        this.empty = false;
        this.rankedOnly = false;
        this.customTags = [];
        this.sortBy = 'ping';
    }

    reset() {
        this.gameMode = null;
        this.map = null;
        this.region = null;
        this.maxPing = null;
        this.minPlayers = 0;
        this.maxPlayers = null;
        this.hasVac = true;
        this.hasPassword = false;
        this.hasBots = false;
        this.full = false;
        this.empty = false;
        this.rankedOnly = false;
        this.customTags = [];
    }

    serialize() {
        return { ...this };
    }
}

export class MatchmakingSystem {
    constructor() {
        this.queues = new Map();
        this.players = new Map();
        this.activeMatches = new Map();
        this.matchmakingPools = new Map();
        this.eligibilityCriteria = new Map();
        this.waitTimeEstimates = new Map();
    }

    createQueue(queueData) {
        const queue = {
            id: queueData.id || 'queue_' + Date.now(),
            name: queueData.name,
            gameMode: queueData.gameMode,
            region: queueData.region,
            minPlayers: queueData.minPlayers || 2,
            maxPlayers: queueData.maxPlayers || 10,
            teamSize: queueData.teamSize || 5,
            rankRestriction: queueData.rankRestriction || null,
            skillRange: queueData.skillRange || null,
            waitTimeLimit: queueData.waitTimeLimit || 300000,
            createdAt: Date.now(),
            players: [],
            matches: [],
            statistics: {
                totalPlayers: 0,
                averageWaitTime: 0,
                matchesPerHour: 0
            }
        };

        this.queues.set(queue.id, queue);
        return queue;
    }

    joinQueue(queueId, playerData) {
        const queue = this.queues.get(queueId);
        if (!queue) return { success: false, reason: 'Queue not found' };

        if (this.isPlayerInQueue(queueId, playerData.id)) {
            return { success: false, reason: 'Already in queue' };
        }

        const player = {
            ...playerData,
            joinedAt: Date.now(),
            preferredTeam: playerData.preferredTeam || null,
            canPlayAnyTeam: playerData.canPlayAnyTeam ?? true,
            skill: playerData.skill || 0,
            rank: playerData.rank || 1
        };

        queue.players.push(player);
        this.players.set(playerData.id, { queueId, player });

        queue.statistics.totalPlayers = queue.players.length;

        return { success: true, queue, position: queue.players.length };
    }

    leaveQueue(queueId, playerId) {
        const queue = this.queues.get(queueId);
        if (!queue) return { success: false };

        const playerIndex = queue.players.findIndex(p => p.id === playerId);
        if (playerIndex === -1) {
            return { success: false, reason: 'Player not in queue' };
        }

        queue.players.splice(playerIndex, 1);
        this.players.delete(playerId);

        return { success: true };
    }

    isPlayerInQueue(queueId, playerId) {
        const queue = this.queues.get(queueId);
        if (!queue) return false;
        return queue.players.some(p => p.id === playerId);
    }

    findMatch(queueId) {
        const queue = this.queues.get(queueId);
        if (!queue) return null;

        if (queue.players.length < queue.minPlayers) {
            return null;
        }

        const teams = this.balanceTeams(queue.players, queue.teamSize);
        
        if (!teams) {
            return null;
        }

        const match = {
            id: 'match_' + Date.now(),
            queueId,
            teams,
            status: 'created',
            createdAt: Date.now(),
            estimatedStartTime: Date.now() + 5000,
            skill: this.calculateAverageSkill(teams)
        };

        this.activeMatches.set(match.id, match);
        queue.matches.push(match);

        return match;
    }

    balanceTeams(players, teamSize) {
        const team1 = [];
        const team2 = [];
        const remaining = [...players];

        remaining.sort((a, b) => b.skill - a.skill);

        for (let i = 0; i < teamSize && remaining.length > 0; i++) {
            if (i % 2 === 0) {
                team1.push(remaining.shift());
            } else {
                team2.push(remaining.shift());
            }
        }

        if (team1.length !== team2.length) {
            return null;
        }

        return { team1, team2 };
    }

    calculateAverageSkill(teams) {
        const allPlayers = [...teams.team1, ...teams.team2];
        const totalSkill = allPlayers.reduce((sum, p) => sum + p.skill, 0);
        return totalSkill / allPlayers.length;
    }

    getQueueStatus(queueId) {
        const queue = this.queues.get(queueId);
        if (!queue) return null;

        const averageWaitTime = queue.players.length > 0
            ? (Date.now() - queue.players.reduce((sum, p) => sum + p.joinedAt, 0) / queue.players.length)
            : 0;

        return {
            id: queue.id,
            name: queue.name,
            playerCount: queue.players.length,
            maxPlayers: queue.maxPlayers,
            averageWaitTime,
            matchCount: queue.matches.length
        };
    }

    getEstimatedWaitTime(queueId, playerSkill) {
        const estimate = this.waitTimeEstimates.get(queueId);
        if (!estimate) return null;

        const skillRange = Math.abs(playerSkill - estimate.averageSkill);
        const waitMultiplier = skillRange / 100;
        const baseWait = estimate.averageWait;

        return baseWait * (1 + waitMultiplier);
    }
}

export class RankingSystem {
    constructor() {
        this.ranks = [];
        this.divisions = [];
        this.leaderboards = new Map();
        this.seasonData = new Map();
        this.playerRanks = new Map();
        this.rankChanges = new Map();
    }

    initialize() {
        this.setupRanks();
        this.setupDivisions();
    }

    setupRanks() {
        this.ranks = [
            { level: 1, name: 'Silver I', minMMR: 0, maxMMR: 999, icon: 'silver_1', color: '#C0C0C0' },
            { level: 2, name: 'Silver II', minMMR: 1000, maxMMR: 1099, icon: 'silver_2', color: '#C0C0C0' },
            { level: 3, name: 'Silver III', minMMR: 1100, maxMMR: 1199, icon: 'silver_3', color: '#C0C0C0' },
            { level: 4, name: 'Gold I', minMMR: 1200, maxMMR: 1399, icon: 'gold_1', color: '#FFD700' },
            { level: 5, name: 'Gold II', minMMR: 1400, maxMMR: 1599, icon: 'gold_2', color: '#FFD700' },
            { level: 6, name: 'Gold III', minMMR: 1600, maxMMR: 1799, icon: 'gold_3', color: '#FFD700' },
            { level: 7, name: 'Master', minMMR: 1800, maxMMR: 1999, icon: 'master', color: '#9400D3' },
            { level: 8, name: 'Diamond', minMMR: 2000, maxMMR: 2199, icon: 'diamond', color: '#00CED1' },
            { level: 9, name: 'Elite', minMMR: 2200, maxMMR: 2399, icon: 'elite', color: '#FF4500' },
            { level: 10, name: 'Champion', minMMR: 2400, maxMMR: 9999, icon: 'champion', color: '#FF69B4' }
        ];
    }

    setupDivisions() {
        this.divisions = [
            { id: 'bronze', name: 'Bronze', ranks: [1], color: '#CD7F32' },
            { id: 'silver', name: 'Silver', ranks: [2, 3], color: '#C0C0C0' },
            { id: 'gold', name: 'Gold', ranks: [4, 5, 6], color: '#FFD700' },
            { id: 'platinum', name: 'Platinum', ranks: [7], color: '#E5E4E2' },
            { id: 'diamond', name: 'Diamond', ranks: [8], color: '#00CED1' },
            { id: 'elite', name: 'Elite', ranks: [9], color: '#FF4500' },
            { id: 'champion', name: 'Champion', ranks: [10], color: '#FF69B4' }
        ];
    }

    getRankForMMR(mmr) {
        return this.ranks.find(r => mmr >= r.minMMR && mmr <= r.maxMMR) || this.ranks[0];
    }

    calculateNewMMR(currentMMR, result, options = {}) {
        const kFactor = options.rankBased ? this.getKFactor(currentMMR) : 32;
        const expectedScore = this.calculateExpectedScore(currentMMR, options.opponentMMR || currentMMR);
        const actualScore = result === 'win' ? 1 : result === 'loss' ? 0 : 0.5;
        const mmrChange = Math.round(kFactor * (actualScore - expectedScore));

        return {
            newMMR: currentMMR + mmrChange,
            change: mmrChange,
            newRank: this.getRankForMMR(currentMMR + mmrChange)
        };
    }

    calculateExpectedScore(playerMMR, opponentMMR) {
        return 1 / (1 + Math.pow(10, (opponentMMR - playerMMR) / 400));
    }

    getKFactor(mmr) {
        if (mmr < 1000) return 40;
        if (mmr < 1500) return 32;
        if (mmr < 2000) return 24;
        return 16;
    }

    getRankProgress(mmr) {
        const rank = this.getRankForMMR(mmr);
        const range = rank.maxMMR - rank.minMMR;
        const progress = mmr - rank.minMMR;
        const percentage = (progress / range) * 100;

        return {
            rank,
            percentage: Math.min(100, Math.max(0, percentage)),
            currentMMR: mmr,
            nextRankMMR: rank.maxMMR + 1,
            progressToNextRank: rank.maxMMR - mmr
        };
    }

    getDivisionForRank(rankLevel) {
        return this.divisions.find(d => d.ranks.includes(rankLevel));
    }

    updateLeaderboard(seasonId, playerData) {
        if (!this.leaderboards.has(seasonId)) {
            this.leaderboards.set(seasonId, new Map());
        }

        const leaderboard = this.leaderboards.get(seasonId);
        leaderboard.set(playerData.playerId, {
            playerId: playerData.playerId,
            playerName: playerData.playerName,
            rank: playerData.rank,
            mmr: playerData.mmr,
            wins: playerData.wins || 0,
            losses: playerData.losses || 0,
            winRate: playerData.winRate || 0,
            gamesPlayed: playerData.gamesPlayed || 0,
            lastUpdated: Date.now()
        });

        return this.getLeaderboard(seasonId);
    }

    getLeaderboard(seasonId, options = {}) {
        const leaderboard = this.leaderboards.get(seasonId);
        if (!leaderboard) return [];

        let entries = Array.from(leaderboard.values());

        if (options.sortBy === 'rank') {
            entries.sort((a, b) => a.rank - b.rank);
        } else {
            entries.sort((a, b) => b.mmr - a.mmr);
        }

        if (options.limit) {
            entries = entries.slice(0, options.limit);
        }

        if (options.offset) {
            entries = entries.slice(options.offset);
        }

        return entries;
    }

    getPlayerRank(playerId, seasonId) {
        const leaderboard = this.leaderboards.get(seasonId);
        if (!leaderboard) return null;

        return leaderboard.get(playerId);
    }

    getRankDistribution(seasonId) {
        const leaderboard = this.leaderboards.get(seasonId);
        if (!leaderboard) return {};

        const distribution = {};
        for (const entry of leaderboard.values()) {
            const rankName = this.getRankForMMR(entry.mmr).name;
            distribution[rankName] = (distribution[rankName] || 0) + 1;
        }

        return distribution;
    }
}

export default {
    ServerBrowser,
    GameServer,
    ServerFilters,
    MatchmakingSystem,
    RankingSystem
};

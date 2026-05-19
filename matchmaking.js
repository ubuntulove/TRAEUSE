export class PlayerSkill {
    constructor() {
        this.overall = 0;
        this.aim = 0;
        this.positioning = 0;
        this.communication = 0;
        this.teamwork = 0;
        this.gameSense = 0;
        this.consistency = 0;
        this clutchAbility = 0;
        this.entryAbility = 0;
        this.supportAbility = 0;
        this.lurkerAbility = 0;
        this.iglingAbility = 0;
        this.ratingHistory = [];
        this.lastUpdated = Date.now();
    }

    getAverage() {
        return (this.aim + this.positioning + this.communication + this.gameSense) / 4;
    }

    getWinProbability(ownSkill, enemySkill) {
        return 1 / (1 + Math.pow(10, (enemySkill - ownSkill) / 400));
    }

    getMatchmakingRating() {
        return Math.round(this.overall * 0.7 + this.getAverage() * 0.3);
    }

    update(newRating) {
        this.ratingHistory.push({
            rating: newRating,
            timestamp: Date.now()
        });
        if (this.ratingHistory.length > 100) {
            this.ratingHistory.shift();
        }
        this.recalculate();
    }

    recalculate() {
        if (this.ratingHistory.length > 0) {
            const recent = this.ratingHistory.slice(-20);
            const avg = recent.reduce((sum, r) => sum + r.rating, 0) / recent.length;
            this.overall = Math.round(avg);
        }
    }

    serialize() {
        return {
            overall: this.overall,
            aim: this.aim,
            positioning: this.positioning,
            communication: this.communication,
            teamwork: this.teamwork,
            gameSense: this.gameSense,
            consistency: this.consistency,
            ratingHistory: this.ratingHistory
        };
    }
}

export class PlayerQueueInfo {
    constructor(playerId) {
        this.playerId = playerId;
        this.queueStartTime = Date.now();
        this.gameMode = 'competitive';
        this.region = 'na';
        this.rank = 0;
        this.winRate = 0.5;
        this.avgDamage = 100;
        this.skill = new PlayerSkill();
        this.preferredMap = null;
        this.avoidMaps = [];
        this.partySize = 1;
        this.isPrime = false;
        this.trustFactor = 1.0;
        this.languages = ['en'];
        this.microphoneEnabled = true;
        this.searchRadius = 'any';
        this.escalation = false;
    }

    getQueueTime() {
        return (Date.now() - this.queueStartTime) / 1000;
    }

    setGameMode(mode) {
        this.gameMode = mode;
    }

    setRegion(region) {
        this.region = region;
    }

    setParty(party) {
        this.partySize = party.length;
        this.skill = this.calculatePartySkill(party);
    }

    calculatePartySkill(party) {
        let totalSkill = 0;
        for (const member of party) {
            totalSkill += member.skill?.overall || 1500;
        }
        const avgSkill = totalSkill / party.length;
        const skill = new PlayerSkill();
        skill.overall = Math.round(avgSkill);
        return skill;
    }

    isInExpandedSearch() {
        const queueTime = this.getQueueTime();
        return queueTime > 60;
    }

    getSearchExpansion() {
        const queueTime = this.getQueueTime();
        if (queueTime < 60) return 0;
        if (queueTime < 120) return 0.1;
        if (queueTime < 300) return 0.2;
        return 0.3;
    }
}

export class MatchmakingConfig {
    constructor() {
        this.updateInterval = 500;
        this.maxSearchTime = 420;
        this.minRankDelta = 500;
        this.maxRankDelta = 2000;
        this.expansionRate = 0.05;
        this.expansionInterval = 30;
        this.partySkillTolerance = 0.15;
        this.trustWeight = 0.1;
        this.latencyWeight = 0.15;
        this.pingLimit = 150;
        this.pingLimitReduced = 100;
        this.rankWeights = {
            skillUncertainty: 0.3,
            recentMatches: 0.2,
            winStreak: 0.1,
            loseStreak: 0.15
        };
        this.dataCenterWeights = {};
        this.matchmakingRatingBounds = {
            min: 1,
            max: 5000
        };
        this.playerCountBounds = {
            min: 1,
            max: 5
        };
    }

    validateConfig() {
        return this.minRankDelta <= this.maxRankDelta &&
               this.pingLimit >= this.pingLimitReduced &&
               this.updateInterval > 0;
    }
}

export class Lobby {
    constructor(id, gameMode) {
        this.id = id;
        this.gameMode = gameMode;
        this.state = 'waiting';
        this.ownerId = null;
        this.members = [];
        this.maxMembers = 5;
        this.slots = {
            counterTerrorist: [],
            terrorist: []
        };
        this.maxTeamSize = 5;
        this.map = null;
        this.region = 'na';
        this.hasPassword = false;
        this.passwordHash = '';
        this.inviteOnly = false;
        this.spectators = [];
        this.chat = [];
        this.votes = new Map();
        this.settings = {};
        this.createdAt = Date.now();
        this.readyStatus = new Map();
    }

    addMember(playerId, team = null) {
        if (this.members.includes(playerId)) return false;
        if (this.members.length >= this.maxMembers) return false;

        this.members.push(playerId);
        this.readyStatus.set(playerId, false);

        if (team && this.canJoinTeam(team)) {
            this.slots[team].push(playerId);
        } else if (this.slots.counterTerrorist.length <= this.slots.terrorist.length) {
            this.slots.counterTerrorist.push(playerId);
        } else {
            this.slots.terrorist.push(playerId);
        }

        return true;
    }

    removeMember(playerId) {
        const index = this.members.indexOf(playerId);
        if (index === -1) return false;

        this.members.splice(index, 1);
        this.readyStatus.delete(playerId);

        const ctIndex = this.slots.counterTerrorist.indexOf(playerId);
        if (ctIndex !== -1) {
            this.slots.counterTerrorist.splice(ctIndex, 1);
        }

        const tIndex = this.slots.terrorist.indexOf(playerId);
        if (tIndex !== -1) {
            this.slots.terrorist.splice(tIndex, 1);
        }

        const spectatorIndex = this.spectators.indexOf(playerId);
        if (spectatorIndex !== -1) {
            this.spectators.splice(spectatorIndex, 1);
        }

        if (this.ownerId === playerId) {
            this.ownerId = this.members.length > 0 ? this.members[0] : null;
        }

        return true;
    }

    canJoinTeam(team) {
        const teamSize = this.slots[team].length;
        const otherTeam = team === 'counterTerrorist' ? 'terrorist' : 'counterTerrorist';
        const otherTeamSize = this.slots[otherTeam].length;
        return teamSize < otherTeamSize + 1;
    }

    switchTeam(playerId) {
        const currentTeam = this.getPlayerTeam(playerId);
        if (!currentTeam) return false;

        const otherTeam = currentTeam === 'counterTerrorist' ? 'terrorist' : 'counterTerrorist';
        if (!this.canJoinTeam(otherTeam)) return false;

        const index = this.slots[currentTeam].indexOf(playerId);
        if (index !== -1) {
            this.slots[currentTeam].splice(index, 1);
        }

        this.slots[otherTeam].push(playerId);
        return true;
    }

    getPlayerTeam(playerId) {
        if (this.slots.counterTerrorist.includes(playerId)) {
            return 'counterTerrorist';
        }
        if (this.slots.terrorist.includes(playerId)) {
            return 'terrorist';
        }
        return null;
    }

    setReady(playerId, ready) {
        if (this.members.includes(playerId)) {
            this.readyStatus.set(playerId, ready);
        }
    }

    areAllReady() {
        for (const playerId of this.members) {
            if (!this.readyStatus.get(playerId)) {
                return false;
            }
        }
        return this.members.length >= 2;
    }

    startVote(map) {
        this.votes.clear();
        this.votes.set(this.ownerId, map);
        this.currentVote = { map, endTime: Date.now() + 60000 };
    }

    castVote(playerId, map) {
        if (this.state !== 'voting') return;
        this.votes.set(playerId, map);
    }

    getVoteResults() {
        const results = new Map();
        for (const [, vote] of this.votes) {
            results.set(vote, (results.get(vote) || 0) + 1);
        }
        return results;
    }

    isFull() {
        return this.members.length >= this.maxMembers;
    }

    getBalance() {
        const ctSize = this.slots.counterTerrorist.length;
        const tSize = this.slots.terrorist.length;
        return ctSize - tSize;
    }

    serialize() {
        return {
            id: this.id,
            gameMode: this.gameMode,
            state: this.state,
            ownerId: this.ownerId,
            members: this.members,
            slots: this.slots,
            maxMembers: this.maxMembers,
            map: this.map,
            region: this.region
        };
    }
}

export class MatchmakingQueue {
    constructor(config) {
        this.config = config || new MatchmakingConfig();
        this.queue = new Map();
        this.skillRanges = this.initSkillRanges();
        this.dataCenters = new Map();
        this.matchHistory = [];
        this.activeMatches = new Map();
        this.waitingPlayers = [];
        this.lastUpdate = Date.now();
        this.isRunning = false;
    }

    initSkillRanges() {
        const ranges = [];
        for (let i = 1; i <= 18; i++) {
            ranges.push({
                rank: i,
                minMMR: (i - 1) * 500 + 1,
                maxMMR: i * 500,
                name: this.getRankName(i)
            });
        }
        return ranges;
    }

    getRankName(rank) {
        const names = [
            'Silver I', 'Silver II', 'Silver III', 'Silver IV', 'Silver Elite',
            'Silver Elite Master', 'Gold Nova I', 'Gold Nova II', 'Gold Nova III',
            'Gold Nova Master', 'Master Guardian I', 'Master Guardian II',
            'Master Guardian Elite', 'Distinguished Master Guardian',
            'Legendary Eagle', 'Legendary Eagle Master', 'Supreme Master First Class',
            'Global Elite'
        ];
        return names[rank - 1] || 'Unranked';
    }

    enqueue(player) {
        const queueInfo = new PlayerQueueInfo(player.id);
        queueInfo.gameMode = player.gameMode;
        queueInfo.region = player.region;
        queueInfo.rank = player.rank || 1500;
        queueInfo.isPrime = player.isPrime || false;
        queueInfo.trustFactor = player.trustFactor || 1.0;

        this.queue.set(player.id, queueInfo);
        this.waitingPlayers.push(player.id);
    }

    dequeue(playerId) {
        const queueInfo = this.queue.get(playerId);
        if (queueInfo) {
            this.queue.delete(playerId);
            const index = this.waitingPlayers.indexOf(playerId);
            if (index !== -1) {
                this.waitingPlayers.splice(index, 1);
            }
            return queueInfo;
        }
        return null;
    }

    isInQueue(playerId) {
        return this.queue.has(playerId);
    }

    getQueuePosition(playerId) {
        return this.waitingPlayers.indexOf(playerId);
    }

    getQueueTime(playerId) {
        const queueInfo = this.queue.get(playerId);
        return queueInfo ? queueInfo.getQueueTime() : 0;
    }

    update() {
        const now = Date.now();
        if (now - this.lastUpdate < this.config.updateInterval) {
            return;
        }
        this.lastUpdate = now;

        for (const [playerId, queueInfo] of this.queue) {
            const expansion = queueInfo.getSearchExpansion();
            if (expansion > 0) {
                this.expandSearch(playerId, expansion);
            }
        }

        this.tryMatchPlayers();
    }

    expandSearch(playerId, expansion) {
        const queueInfo = this.queue.get(playerId);
        if (queueInfo) {
            queueInfo.skill.expansion = expansion;
        }
    }

    tryMatchPlayers() {
        const sortedPlayers = Array.from(this.queue.values())
            .sort((a, b) => a.skill.overall - b.skill.overall);

        const matched = new Set();
        const teams = [];

        for (let i = 0; i < sortedPlayers.length; i++) {
            if (matched.has(sortedPlayers[i].playerId)) continue;

            const team1 = [sortedPlayers[i]];
            const team1Skill = sortedPlayers[i].skill.overall;
            matched.add(sortedPlayers[i].playerId);

            for (let j = i + 1; j < sortedPlayers.length; j++) {
                if (matched.has(sortedPlayers[j].playerId)) continue;
                if (team1.length >= 5) break;

                const skillDiff = Math.abs(sortedPlayers[j].skill.overall - team1Skill / team1.length);
                if (skillDiff < this.config.maxRankDelta) {
                    team1.push(sortedPlayers[j]);
                    matched.add(sortedPlayers[j].playerId);
                }
            }

            const enemyTeam = this.findEnemyTeam(team1, sortedPlayers, matched);
            if (enemyTeam) {
                teams.push({ team1, team2: enemyTeam });
                for (const p of enemyTeam) {
                    matched.add(p.playerId);
                }
            }
        }

        for (const match of teams) {
            this.createMatch(match.team1, match.team2);
        }
    }

    findEnemyTeam(team1, sortedPlayers, matched) {
        const avgSkill = team1.reduce((sum, p) => sum + p.skill.overall, 0) / team1.length;
        const team = [];
        const needed = 5 - team1.length;

        for (const player of sortedPlayers) {
            if (matched.has(player.playerId)) continue;
            if (team.length >= needed) break;

            const skillDiff = Math.abs(player.skill.overall - avgSkill);
            if (skillDiff < this.config.maxRankDelta * 1.5) {
                team.push(player);
                matched.add(player.playerId);
            }
        }

        return team.length >= needed - 1 ? team : null;
    }

    createMatch(team1, team2) {
        for (const player of team1) {
            this.dequeue(player.playerId);
        }
        for (const player of team2) {
            this.dequeue(player.playerId);
        }

        const match = {
            id: MatchmakingQueue.nextMatchId++,
            teams: [team1.map(p => p.playerId), team2.map(p => p.playerId)],
            createdAt: Date.now(),
            state: 'created'
        };

        this.activeMatches.set(match.id, match);
        return match;
    }

    cancelMatch(matchId) {
        const match = this.activeMatches.get(matchId);
        if (match && match.state === 'created') {
            for (const playerId of match.teams.flat()) {
                this.enqueue({ id: playerId });
            }
            this.activeMatches.delete(matchId);
            return true;
        }
        return false;
    }

    getQueueSize() {
        return this.queue.size;
    }

    getEstimatedWaitTime(playerId) {
        const queueInfo = this.queue.get(playerId);
        if (!queueInfo) return 0;

        const skillTier = Math.floor(queueInfo.skill.overall / 500);
        const baseWait = 30 + skillTier * 10;
        const queueBonus = this.queue.size * 2;

        return Math.min(baseWait + queueBonus, 300);
    }

    serialize() {
        return {
            queueSize: this.queue.size,
            waitingPlayers: this.waitingPlayers,
            activeMatches: this.activeMatches.size,
            skillRanges: this.skillRanges
        };
    }
}
MatchmakingQueue.nextMatchId = 1;

export class RankSystem {
    constructor() {
        this.ranks = this.initRanks();
        this.primeRank = 21;
        this.maxRank = 18;
    }

    initRanks() {
        return [
            { level: 1, name: 'Silver I', minMMR: 1, maxMMR: 500, icon: 'silver1' },
            { level: 2, name: 'Silver II', minMMR: 501, maxMMR: 1000, icon: 'silver2' },
            { level: 3, name: 'Silver III', minMMR: 1001, maxMMR: 1500, icon: 'silver3' },
            { level: 4, name: 'Silver IV', minMMR: 1501, maxMMR: 2000, icon: 'silver4' },
            { level: 5, name: 'Silver Elite', minMMR: 2001, maxMMR: 2500, icon: 'silver_elite' },
            { level: 6, name: 'Silver Elite Master', minMMR: 2501, maxMMR: 3000, icon: 'silver_elite_master' },
            { level: 7, name: 'Gold Nova I', minMMR: 3001, maxMMR: 3500, icon: 'gold_nova1' },
            { level: 8, name: 'Gold Nova II', minMMR: 3501, maxMMR: 4000, icon: 'gold_nova2' },
            { level: 9, name: 'Gold Nova III', minMMR: 4001, maxMMR: 4500, icon: 'gold_nova3' },
            { level: 10, name: 'Gold Nova Master', minMMR: 4501, maxMMR: 5000, icon: 'gold_nova_master' },
            { level: 11, name: 'Master Guardian I', minMMR: 5001, maxMMR: 5500, icon: 'master_guardian1' },
            { level: 12, name: 'Master Guardian II', minMMR: 5501, maxMMR: 6000, icon: 'master_guardian2' },
            { level: 13, name: 'Master Guardian Elite', minMMR: 6001, maxMMR: 6500, icon: 'master_guardian_elite' },
            { level: 14, name: 'Distinguished Master Guardian', minMMR: 6501, maxMMR: 7000, icon: 'dmg' },
            { level: 15, name: 'Legendary Eagle', minMMR: 7001, maxMMR: 7500, icon: 'legendary_eagle' },
            { level: 16, name: 'Legendary Eagle Master', minMMR: 7501, maxMMR: 8000, icon: 'legendary_eagle_master' },
            { level: 17, name: 'Supreme Master First Class', minMMR: 8001, maxMMR: 8500, icon: 'supreme' },
            { level: 18, name: 'Global Elite', minMMR: 8501, maxMMR: 10000, icon: 'global_elite' }
        ];
    }

    getRankForMMR(mmr) {
        for (let i = this.ranks.length - 1; i >= 0; i--) {
            if (mmr >= this.ranks[i].minMMR) {
                return this.ranks[i];
            }
        }
        return this.ranks[0];
    }

    getRankLevel(mmr) {
        const rank = this.getRankForMMR(mmr);
        return rank ? rank.level : 1;
    }

    getMMRForRank(level) {
        const rank = this.ranks.find(r => r.level === level);
        return rank ? rank.minMMR : 1;
    }

    getWinBonus(winnerMMR, loserMMR) {
        const expected = 1 / (1 + Math.pow(10, (loserMMR - winnerMMR) / 400));
        const K = 32;
        return Math.round(K * (1 - expected));
    }

    getLosePenalty(winnerMMR, loserMMR) {
        const expected = 1 / (1 + Math.pow(10, (loserMMR - winnerMMR) / 400));
        const K = 32;
        return Math.round(K * (0 - expected));
    }

    calculateNewRatings(team1MMR, team2MMR, team1Won) {
        const avg1 = team1MMR.reduce((a, b) => a + b, 0) / team1MMR.length;
        const avg2 = team2MMR.reduce((a, b) => a + b, 0) / team2MMR.length;

        const updates = [];
        for (const mmr of team1MMR) {
            const change = team1Won ?
                this.getWinBonus(mmr, avg2) :
                this.getLosePenalty(avg2, mmr);
            updates.push({ mmr, change, newMMR: mmr + change });
        }

        for (const mmr of team2MMR) {
            const change = !team1Won ?
                this.getWinBonus(mmr, avg1) :
                this.getLosePenalty(avg1, mmr);
            updates.push({ mmr, change, newMMR: mmr + change });
        }

        return updates;
    }
}

export class MatchmakingManager {
    constructor() {
        this.config = new MatchmakingConfig();
        this.queue = new MatchmakingQueue(this.config);
        this.lobbies = new Map();
        this.activeMatches = new Map();
        this.rankSystem = new RankSystem();
        this.playerData = new Map();
        this.pendingInvites = new Map();
        this.isEnabled = true;
        this.status = 'online';
        this.lastStatusUpdate = Date.now();
    }

    initialize() {
        this.queue.isRunning = true;
    }

    shutdown() {
        this.queue.isRunning = false;
        this.status = 'offline';
    }

    getStatus() {
        return {
            status: this.status,
            playersOnline: this.playerData.size,
            playersInQueue: this.queue.getQueueSize(),
            activeMatches: this.activeMatches.size,
            lastUpdate: this.lastStatusUpdate
        };
    }

    registerPlayer(playerId, data) {
        this.playerData.set(playerId, {
            id: playerId,
            rank: data.rank || 1500,
            winCount: data.winCount || 0,
            lossCount: data.lossCount || 0,
            lastMatch: data.lastMatch || 0,
            preferredRegion: data.preferredRegion || 'na',
            isPrime: data.isPrime || false,
            trustFactor: data.trustFactor || 1.0,
            accountLevel: data.accountLevel || 1
        });
    }

    unregisterPlayer(playerId) {
        this.playerData.delete(playerId);
        this.queue.dequeue(playerId);
        this.leaveLobby(playerId);
    }

    findMatch(playerId, gameMode = 'competitive') {
        const player = this.playerData.get(playerId);
        if (!player) return null;

        if (this.queue.isInQueue(playerId)) {
            return { status: 'already_in_queue', position: this.queue.getQueuePosition(playerId) };
        }

        this.queue.enqueue({
            id: playerId,
            gameMode,
            region: player.preferredRegion,
            rank: player.rank,
            isPrime: player.isPrime,
            trustFactor: player.trustFactor
        });

        return { status: 'queued', position: this.queue.getQueuePosition(playerId) };
    }

    cancelSearch(playerId) {
        const queueInfo = this.queue.dequeue(playerId);
        if (queueInfo) {
            return { status: 'cancelled', queueTime: queueInfo.getQueueTime() };
        }
        return { status: 'not_in_queue' };
    }

    getQueueStatus(playerId) {
        if (!this.queue.isInQueue(playerId)) {
            return null;
        }
        return {
            position: this.queue.getQueuePosition(playerId),
            queueTime: this.queue.getQueueTime(playerId),
            estimatedWait: this.queue.getEstimatedWaitTime(playerId)
        };
    }

    createLobby(playerId, gameMode, settings = {}) {
        const lobby = new Lobby(Lobby.nextId++, gameMode);
        lobby.ownerId = playerId;
        lobby.addMember(playerId);
        lobby.settings = settings;

        this.lobbies.set(lobby.id, lobby);
        return lobby;
    }

    joinLobby(lobbyId, playerId, password = null) {
        const lobby = this.lobbies.get(lobbyId);
        if (!lobby) return { success: false, error: 'Lobby not found' };

        if (lobby.hasPassword && lobby.passwordHash !== this.hashPassword(password)) {
            return { success: false, error: 'Invalid password' };
        }

        if (lobby.isFull()) {
            return { success: false, error: 'Lobby is full' };
        }

        if (lobby.inviteOnly && !lobby.invites?.includes(playerId)) {
            return { success: false, error: 'Invite only' };
        }

        lobby.addMember(playerId);
        return { success: true, lobby };
    }

    leaveLobby(playerId) {
        for (const [lobbyId, lobby] of this.lobbies) {
            if (lobby.members.includes(playerId)) {
                lobby.removeMember(playerId);
                if (lobby.members.length === 0) {
                    this.lobbies.delete(lobbyId);
                }
                return true;
            }
        }
        return false;
    }

    inviteToLobby(lobbyId, hostId, targetId) {
        const lobby = this.lobbies.get(lobbyId);
        if (!lobby || lobby.ownerId !== hostId) {
            return { success: false, error: 'Not authorized' };
        }

        if (!lobby.invites) {
            lobby.invites = [];
        }
        lobby.invites.push(targetId);

        this.pendingInvites.set(`${lobbyId}_${targetId}`, {
            lobbyId,
            from: hostId,
            to: targetId,
            createdAt: Date.now()
        });

        return { success: true };
    }

    acceptInvite(lobbyId, playerId) {
        const key = `${lobbyId}_${playerId}`;
        const invite = this.pendingInvites.get(key);
        if (!invite) {
            return { success: false, error: 'Invite not found' };
        }

        this.pendingInvites.delete(key);
        return this.joinLobby(lobbyId, playerId);
    }

    startMatch(lobbyId) {
        const lobby = this.lobbies.get(lobbyId);
        if (!lobby) return null;

        if (lobby.state !== 'waiting') {
            return { success: false, error: 'Lobby not in waiting state' };
        }

        if (!lobby.areAllReady()) {
            return { success: false, error: 'Not all players ready' };
        }

        const team1 = lobby.slots.counterTerrorist;
        const team2 = lobby.slots.terrorist;

        if (team1.length !== team2.length) {
            return { success: false, error: 'Teams not balanced' };
        }

        lobby.state = 'playing';
        const match = {
            id: MatchmakingManager.nextMatchId++,
            lobbyId,
            teams: [team1, team2],
            map: lobby.map || this.selectMap(),
            gameMode: lobby.gameMode,
            createdAt: Date.now(),
            state: 'loading'
        };

        this.activeMatches.set(match.id, match);
        return { success: true, match };
    }

    selectMap() {
        const maps = ['de_dust2', 'de_mirage', 'de_inferno', 'de_nuke', 'de_overpass', 'de_train'];
        return maps[Math.floor(Math.random() * maps.length)];
    }

    update() {
        if (!this.isEnabled) return;

        this.queue.update();

        for (const [matchId, match] of this.activeMatches) {
            if (match.state === 'created') {
                match.state = 'loading';
            }
        }
    }

    completeMatch(matchId, winnerTeam) {
        const match = this.activeMatches.get(matchId);
        if (!match) return null;

        match.state = 'completed';
        match.winnerTeam = winnerTeam;
        match.completedAt = Date.now();

        const team1MMR = [];
        const team2MMR = [];

        for (const playerId of match.teams[0]) {
            const data = this.playerData.get(playerId);
            if (data) {
                team1MMR.push(data.rank);
            }
        }

        for (const playerId of match.teams[1]) {
            const data = this.playerData.get(playerId);
            if (data) {
                team2MMR.push(data.rank);
            }
        }

        const updates = this.rankSystem.calculateNewRatings(team1MMR, team2MMR, winnerTeam === 0);

        const result = {
            matchId,
            teams: match.teams,
            winnerTeam,
            updates
        };

        return result;
    }

    hashPassword(password) {
        let hash = 0;
        for (let i = 0; i < password.length; i++) {
            const char = password.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash;
        }
        return hash.toString(16);
    }

    getPlayerStats(playerId) {
        return this.playerData.get(playerId);
    }

    getRankInfo(mmr) {
        return this.rankSystem.getRankForMMR(mmr);
    }

    serialize() {
        return {
            status: this.status,
            queueSize: this.queue.getQueueSize(),
            activeMatches: this.activeMatches.size,
            lobbies: Array.from(this.lobbies.values()).map(l => l.serialize())
        };
    }
}
MatchmakingManager.nextMatchId = 1;

export default MatchmakingManager;

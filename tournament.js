export const TournamentStatus = {
    REGISTRATION: 'registration',
    CHECK_IN: 'check_in',
    IN_PROGRESS: 'in_progress',
    COMPLETED: 'completed',
    CANCELLED: 'cancelled',
    POSTPONED: 'postponed'
};

export const TournamentFormat = {
    SINGLE_ELIMINATION: 'single_elimination',
    DOUBLE_ELIMINATION: 'double_elimination',
    ROUND_ROBIN: 'round_robin',
    SWISS: 'swiss',
    GSL: 'gsl',
    LEAGUE: 'league'
};

export const TournamentType = {
    MAJOR: 'major',
    MINOR: 'minor',
    QUALIFIER: 'qualifier',
    INVITATIONAL: 'invitational',
    SHOWMATCH: 'showmatch',
    CHARITY: 'charity'
};

export const MatchStatus = {
    SCHEDULED: 'scheduled',
    IN_PROGRESS: 'in_progress',
    COMPLETED: 'completed',
    CANCELLED: 'cancelled',
    POSTPONED: 'postponed'
};

export class Tournament {
    constructor(data) {
        this.id = 'tournament_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        this.name = data.name;
        this.shortName = data.shortName || data.name.substring(0, 3).toUpperCase();
        this.type = data.type || TournamentType.MINOR;
        this.format = data.format || TournamentFormat.SINGLE_ELIMINATION;
        this.status = TournamentStatus.REGISTRATION;
        this.organizer = data.organizer;
        this.sponsor = data.sponsor;
        this.prizePool = data.prizePool || 0;
        this.currency = data.currency || 'USD';
        this.location = data.location;
        this.region = data.region;
        this.maxTeams = data.maxTeams || 8;
        this.minTeams = data.minTeams || 2;
        this.registeredTeams = [];
        this.qualifiedTeams = [];
        this.invitedTeams = [];
        this.schedule = new TournamentSchedule();
        this.bracket = null;
        this.matches = [];
        this.teams = new Map();
        this.players = new Map();
        this.stats = new TournamentStats();
        this.rules = new TournamentRules(data.rules);
        this.stream = data.stream;
        this.broadcastSettings = new BroadcastSettings();
        this.registrationStart = data.registrationStart || Date.now();
        this.registrationEnd = data.registrationEnd || Date.now() + 7 * 24 * 60 * 60 * 1000;
        this.checkInStart = data.checkInStart;
        this.checkInEnd = data.checkInEnd;
        this.startDate = data.startDate;
        this.endDate = data.endDate;
        this.createdAt = Date.now();
        this.updatedAt = Date.now();
        this.metadata = data.metadata || {};
        this.tags = data.tags || [];
        this.visibility = data.visibility || 'public';
        this.allowSpectators = data.allowSpectators ?? true;
        this.chatEnabled = data.chatEnabled ?? true;
        this.castEnabled = data.castEnabled ?? true;
    }

    registerTeam(team) {
        if (this.registeredTeams.length >= this.maxTeams) {
            return { success: false, reason: 'Tournament is full' };
        }

        if (this.getTeamById(team.id)) {
            return { success: false, reason: 'Team already registered' };
        }

        const teamData = {
            id: team.id,
            name: team.name,
            tag: team.tag,
            logo: team.logo,
            roster: team.roster.map(p => p.id),
            registeredAt: Date.now(),
            checkInStatus: 'pending',
            seed: null,
            status: 'registered'
        };

        this.registeredTeams.push(teamData);
        this.teams.set(team.id, teamData);
        
        for (const player of team.roster) {
            if (!this.players.has(player.id)) {
                this.players.set(player.id, {
                    id: player.id,
                    name: player.name,
                    teamId: team.id,
                    stats: new PlayerTournamentStats()
                });
            }
        }

        this.updatedAt = Date.now();
        return { success: true, team: teamData };
    }

    unregisterTeam(teamId) {
        const teamIndex = this.registeredTeams.findIndex(t => t.id === teamId);
        if (teamIndex === -1) {
            return { success: false, reason: 'Team not found' };
        }

        const team = this.registeredTeams[teamIndex];
        this.registeredTeams.splice(teamIndex, 1);
        this.teams.delete(teamId);

        for (const playerId of team.roster) {
            this.players.delete(playerId);
        }

        this.updatedAt = Date.now();
        return { success: true };
    }

    inviteTeam(team) {
        this.invitedTeams.push({
            id: team.id,
            name: team.name,
            invitedAt: Date.now(),
            status: 'pending',
            accepted: false
        });
        this.updatedAt = Date.now();
    }

    acceptInvitation(teamId) {
        const invite = this.invitedTeams.find(t => t.id === teamId);
        if (!invite) return { success: false, reason: 'Invitation not found' };

        invite.status = 'accepted';
        invite.accepted = true;
        return this.registerTeam({ ...invite, roster: [] });
    }

    checkInTeam(teamId) {
        const team = this.getTeamById(teamId);
        if (!team) {
            return { success: false, reason: 'Team not found' };
        }

        const now = Date.now();
        if (this.checkInStart && now < this.checkInStart) {
            return { success: false, reason: 'Check-in has not started' };
        }
        if (this.checkInEnd && now > this.checkInEnd) {
            return { success: false, reason: 'Check-in has ended' };
        }

        team.checkInStatus = 'checked_in';
        team.checkedInAt = now;
        this.updatedAt = Date.now();
        return { success: true };
    }

    generateBracket() {
        if (this.registeredTeams.length < this.minTeams) {
            return { success: false, reason: 'Not enough teams' };
        }

        this.qualifiedTeams = this.registeredTeams.filter(t => t.checkInStatus === 'checked_in');
        
        if (this.qualifiedTeams.length < this.minTeams) {
            return { success: false, reason: 'Not enough checked-in teams' };
        }

        this.seedTeams();
        
        switch (this.format) {
            case TournamentFormat.SINGLE_ELIMINATION:
                this.bracket = new SingleEliminationBracket(this.qualifiedTeams);
                break;
            case TournamentFormat.DOUBLE_ELIMINATION:
                this.bracket = new DoubleEliminationBracket(this.qualifiedTeams);
                break;
            case TournamentFormat.ROUND_ROBIN:
                this.bracket = new RoundRobinBracket(this.qualifiedTeams);
                break;
            case TournamentFormat.SWISS:
                this.bracket = new SwissBracket(this.qualifiedTeams);
                break;
            default:
                this.bracket = new SingleEliminationBracket(this.qualifiedTeams);
        }

        this.matches = this.bracket.generateMatches();
        this.status = TournamentStatus.IN_PROGRESS;
        this.updatedAt = Date.now();

        return { success: true, bracket: this.bracket };
    }

    seedTeams() {
        const qualified = this.qualifiedTeams;
        
        qualified.sort((a, b) => {
            if ((a.seed || 999) !== (b.seed || 999)) {
                return (a.seed || 999) - (b.seed || 999);
            }
            return a.registeredAt - b.registeredAt;
        });

        const seedOrder = this.calculateSeedOrder(qualified.length);
        qualified.forEach((team, index) => {
            team.seed = seedOrder[index];
        });
    }

    calculateSeedOrder(teamCount) {
        const order = [];
        
        if (teamCount === 2) {
            return [1, 2];
        }
        
        if (teamCount === 4) {
            return [1, 4, 2, 3];
        }
        
        if (teamCount === 8) {
            return [1, 8, 4, 5, 2, 7, 3, 6];
        }
        
        if (teamCount === 16) {
            return [1, 16, 8, 9, 4, 13, 5, 12, 2, 15, 7, 10, 3, 14, 6, 11];
        }

        for (let i = 0; i < teamCount; i++) {
            order.push(i + 1);
        }
        return order;
    }

    recordMatchResult(matchId, result) {
        const match = this.matches.find(m => m.id === matchId);
        if (!match) {
            return { success: false, reason: 'Match not found' };
        }

        match.result = result;
        match.status = MatchStatus.COMPLETED;
        match.completedAt = Date.now();

        if (result.winner) {
            const winner = this.getTeamById(result.winner);
            if (winner) {
                winner.wins = (winner.wins || 0) + 1;
            }
        }

        if (result.loser) {
            const loser = this.getTeamById(result.loser);
            if (loser) {
                loser.losses = (loser.losses || 0) + 1;
            }
        }

        this.updateBracketAfterMatch(match, result);
        this.updateStats(result);

        if (this.isTournamentComplete()) {
            this.completeTournament();
        }

        this.updatedAt = Date.now();
        return { success: true, match };
    }

    updateBracketAfterMatch(match, result) {
        if (!this.bracket) return;

        const nextMatch = this.bracket.getNextMatch(match);
        if (nextMatch) {
            const position = this.bracket.getNextPosition(match);
            if (position === 'slot1') {
                nextMatch.team1Id = result.winner;
            } else {
                nextMatch.team2Id = result.winner;
            }
        }

        if (this.format === TournamentFormat.DOUBLE_ELIMINATION) {
            this.bracket.moveToLosersBracket(match, result);
        }
    }

    updateStats(result) {
        this.stats.matchesPlayed++;
        
        if (result.map) {
            this.stats.mapsPlayed.push(result.map);
        }
        
        if (result.score) {
            this.stats.totalRounds += result.score.team1 + result.score.team2;
        }

        for (const playerId of (result.playerStats?.keys() || [])) {
            const player = this.players.get(playerId);
            if (player) {
                const stats = result.playerStats.get(playerId);
                player.stats.addMatch(stats);
            }
        }
    }

    isTournamentComplete() {
        if (this.format === TournamentFormat.SINGLE_ELIMINATION) {
            const completedMatches = this.matches.filter(m => m.status === MatchStatus.COMPLETED);
            const finalMatches = this.matches.filter(m => m.round === this.bracket.totalRounds);
            return finalMatches.length > 0 && finalMatches.every(m => m.status === MatchStatus.COMPLETED);
        }

        if (this.format === TournamentFormat.DOUBLE_ELIMINATION) {
            return this.bracket?.grandFinalsCompleted;
        }

        if (this.format === TournamentFormat.ROUND_ROBIN) {
            const completedMatches = this.matches.filter(m => m.status === MatchStatus.COMPLETED);
            const expectedMatches = (this.qualifiedTeams.length * (this.qualifiedTeams.length - 1)) / 2;
            return completedMatches.length >= expectedMatches;
        }

        return false;
    }

    completeTournament() {
        this.status = TournamentStatus.COMPLETED;
        
        const winner = this.determineWinner();
        if (winner) {
            this.stats.winner = winner;
            this.stats.champion = this.getTeamById(winner);
        }

        this.calculatePrizeDistribution();
        this.updatedAt = Date.now();
    }

    determineWinner() {
        if (this.format === TournamentFormat.SINGLE_ELIMINATION) {
            const finalMatch = this.matches.find(m => m.round === this.bracket.totalRounds && m.status === MatchStatus.COMPLETED);
            return finalMatch?.result?.winner;
        }

        if (this.format === TournamentFormat.DOUBLE_ELIMINATION) {
            return this.bracket.getChampion();
        }

        if (this.format === TournamentFormat.ROUND_ROBIN) {
            return this.bracket.getLeader();
        }

        return null;
    }

    calculatePrizeDistribution() {
        const distribution = [
            { place: 1, percentage: 0.5 },
            { place: 2, percentage: 0.25 },
            { place: 3, percentage: 0.125 },
            { place: 4, percentage: 0.0625 }
        ];

        for (const prize of distribution) {
            const team = this.getTeamAtPlace(prize.place);
            if (team) {
                prize.teamId = team.id;
                prize.amount = Math.floor(this.prizePool * prize.percentage);
            }
        }

        this.stats.prizeDistribution = distribution;
        return distribution;
    }

    getTeamAtPlace(place) {
        if (this.format === TournamentFormat.ROUND_ROBIN) {
            return this.bracket?.getTeamAtRank(place);
        }

        const finalMatch = this.matches.find(m => m.round === this.bracket.totalRounds);
        if (place === 1) {
            return this.getTeamById(finalMatch?.result?.winner);
        }
        if (place === 2) {
            return this.getTeamById(finalMatch?.result?.loser);
        }

        return null;
    }

    getTeamById(teamId) {
        return this.teams.get(teamId);
    }

    getMatchById(matchId) {
        return this.matches.find(m => m.id === matchId);
    }

    getMatchesForTeam(teamId) {
        return this.matches.filter(m => m.team1Id === teamId || m.team2Id === teamId);
    }

    getMatchesForRound(round) {
        return this.matches.filter(m => m.round === round);
    }

    getUpcomingMatches() {
        return this.matches
            .filter(m => m.status === MatchStatus.SCHEDULED || m.status === MatchStatus.IN_PROGRESS)
            .sort((a, b) => (a.scheduledTime || 0) - (b.scheduledTime || 0));
    }

    getLeaderboard() {
        const teams = Array.from(this.teams.values());
        
        return teams.sort((a, b) => {
            if ((a.wins || 0) !== (b.wins || 0)) {
                return (b.wins || 0) - (a.wins || 0);
            }
            if ((a.seed || 999) !== (b.seed || 999)) {
                return (a.seed || 999) - (b.seed || 999);
            }
            return 0;
        });
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            shortName: this.shortName,
            type: this.type,
            format: this.format,
            status: this.status,
            organizer: this.organizer,
            sponsor: this.sponsor,
            prizePool: this.prizePool,
            currency: this.currency,
            location: this.location,
            region: this.region,
            maxTeams: this.maxTeams,
            minTeams: this.minTeams,
            registeredTeams: this.registeredTeams,
            qualifiedTeams: this.qualifiedTeams,
            invitedTeams: this.invitedTeams,
            matches: this.matches,
            stats: this.stats.serialize(),
            rules: this.rules.serialize(),
            registrationStart: this.registrationStart,
            registrationEnd: this.registrationEnd,
            startDate: this.startDate,
            endDate: this.endDate,
            createdAt: this.createdAt,
            updatedAt: this.updatedAt,
            metadata: this.metadata,
            tags: this.tags,
            visibility: this.visibility
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.teams = new Map(this.registeredTeams.map(t => [t.id, t]));
    }
}

export class TournamentSchedule {
    constructor() {
        this.events = [];
        this.timezones = new Map();
        this.currentTimezone = 'UTC';
    }

    addEvent(event) {
        this.events.push({
            ...event,
            id: 'event_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            createdAt: Date.now()
        });
        this.sortEvents();
    }

    removeEvent(eventId) {
        this.events = this.events.filter(e => e.id !== eventId);
    }

    updateEvent(eventId, updates) {
        const event = this.events.find(e => e.id === eventId);
        if (event) {
            Object.assign(event, updates);
            this.sortEvents();
        }
    }

    sortEvents() {
        this.events.sort((a, b) => (a.startTime || 0) - (b.startTime || 0));
    }

    getEventsInRange(start, end) {
        return this.events.filter(e => 
            (e.startTime || 0) >= start && (e.startTime || 0) <= end
        );
    }

    serialize() {
        return {
            events: this.events,
            currentTimezone: this.currentTimezone
        };
    }
}

export class TournamentStats {
    constructor() {
        this.matchesPlayed = 0;
        this.mapsPlayed = [];
        this.totalRounds = 0;
        this.totalViewers = 0;
        this.peakViewers = 0;
        this.averageMatchDuration = 0;
        this.winner = null;
        this.champion = null;
        this.prizeDistribution = [];
        this.playerStats = new Map();
        this.teamStats = new Map();
        this.mapStats = new Map();
        this.recordedAt = Date.now();
    }

    addMatch(stats) {
        this.matchesPlayed++;
        if (stats.map) {
            this.mapsPlayed.push(stats.map);
        }
        if (stats.rounds) {
            this.totalRounds += stats.rounds;
        }
    }

    recordViewership(viewers) {
        this.totalViewers += viewers;
        this.peakViewers = Math.max(this.peakViewers, viewers);
    }

    serialize() {
        return {
            matchesPlayed: this.matchesPlayed,
            mapsPlayed: this.mapsPlayed,
            totalRounds: this.totalRounds,
            totalViewers: this.totalViewers,
            peakViewers: this.peakViewers,
            winner: this.winner,
            prizeDistribution: this.prizeDistribution,
            recordedAt: this.recordedAt
        };
    }
}

export class TournamentRules {
    constructor(data = {}) {
        this.bestOf = data.bestOf || 3;
        this.maxGamesPerMatch = data.maxGamesPerMatch || this.bestOf;
        this.teamSize = data.teamSize || 5;
        this.reservePlayers = data.reservePlayers || 2;
        this.allowSubstitutes = data.allowSubstitutes ?? true;
        this.substitutionDeadline = data.substitutionDeadline;
        this.matchTimeout = data.matchTimeout || 15 * 60 * 1000;
        this.pauseAllowed = data.pauseAllowed ?? true;
        this.maxPauseTime = data.maxPauseTime || 2 * 60 * 1000;
        this.overtimeRules = new OvertimeRules(data.overtime);
        this.mapPool = data.mapPool || [];
        this.mapVetoProcess = data.mapVetoProcess || 'standard';
        this.allowPause = data.allowPause ?? true;
        this.forfeitTimeout = data.forfeitTimeout || 10 * 60 * 1000;
        this.disconnectRules = data.disconnectRules || 'default';
        this.coachingRules = data.coachingRules || 'disabled';
        this.customRules = data.customRules || [];
    }

    canPause(playerId) {
        if (!this.allowPause) return false;
        return true;
    }

    canSubstitute(playerId) {
        if (!this.allowSubstitutes) return false;
        return true;
    }

    getMatchDuration(match) {
        const start = match.startTime;
        const end = match.endTime || Date.now();
        return end - start;
    }

    serialize() {
        return {
            bestOf: this.bestOf,
            teamSize: this.teamSize,
            reservePlayers: this.reservePlayers,
            allowSubstitutes: this.allowSubstitutes,
            matchTimeout: this.matchTimeout,
            pauseAllowed: this.pauseAllowed,
            overtimeRules: this.overtimeRules.serialize(),
            mapPool: this.mapPool,
            mapVetoProcess: this.mapVetoProcess,
            forfeitTimeout: this.forfeitTimeout
        };
    }
}

export class OvertimeRules {
    constructor(data = {}) {
        this.enabled = data.enabled ?? true;
        this.rules = data.rules || 'standard';
        this.maxOvertimes = data.maxOvertimes || 6;
        this.overtimeLength = data.overtimeLength || 3;
        this.startMoney = data.startMoney || 10000;
        this.winCondition = data.winCondition || '2_rounds_ahead';
        this.tieBreaker = data.tieBreaker || 'next_round';
    }

    serialize() {
        return {
            enabled: this.enabled,
            rules: this.rules,
            maxOvertimes: this.maxOvertimes,
            overtimeLength: this.overtimeLength,
            startMoney: this.startMoney,
            winCondition: this.winCondition
        };
    }
}

export class BroadcastSettings {
    constructor() {
        this.enabled = true;
        this.streamUrl = '';
        this.embedCode = '';
        this.autoRecord = true;
        this.quality = '1080p';
        this.delays = [0, 30, 60, 120];
        this.currentDelay = 0;
        this.multiView = true;
        this.casterAudio = true;
        this.commentaryEnabled = true;
    }

    serialize() {
        return {
            enabled: this.enabled,
            streamUrl: this.streamUrl,
            quality: this.quality,
            currentDelay: this.currentDelay,
            multiView: this.multiView
        };
    }
}

export class TournamentBracket {
    constructor(teams) {
        this.teams = teams;
        this.rounds = [];
        this.totalRounds = 0;
        this.matches = [];
    }

    generateMatches() {
        return [];
    }

    getNextMatch(match) {
        return null;
    }

    getNextPosition(match) {
        return null;
    }
}

export class SingleEliminationBracket extends TournamentBracket {
    constructor(teams) {
        super(teams);
        this.totalRounds = Math.ceil(Math.log2(teams.length));
        this.generateRounds();
    }

    generateRounds() {
        let matchId = 1;
        
        for (let round = 1; round <= this.totalRounds; round++) {
            const matchesInRound = Math.pow(2, this.totalRounds - round);
            const roundMatches = [];

            for (let i = 0; i < matchesInRound; i++) {
                const match = {
                    id: 'match_' + matchId++,
                    round: round,
                    team1Id: null,
                    team2Id: null,
                    status: MatchStatus.SCHEDULED,
                    result: null,
                    scheduledTime: null,
                    bracketPosition: i
                };

                if (round === 1) {
                    const seed1 = this.getSeedForPosition(round, i, 1);
                    const seed2 = this.getSeedForPosition(round, i, 2);
                    
                    const team1 = this.teams.find(t => t.seed === seed1);
                    const team2 = this.teams.find(t => t.seed === seed2);
                    
                    match.team1Id = team1?.id;
                    match.team2Id = team2?.id;
                }

                roundMatches.push(match);
                this.matches.push(match);
            }

            this.rounds.push({
                round: round,
                matches: roundMatches,
                name: this.getRoundName(round)
            });
        }
    }

    getSeedForPosition(round, matchIndex, slot) {
        const matchesInRound = Math.pow(2, this.totalRounds - round);
        const byeAdjustment = (slot - 1) * matchesInRound;
        return (matchIndex * 2 + slot) - byeAdjustment;
    }

    getRoundName(round) {
        if (round === this.totalRounds) return 'Grand Final';
        if (round === this.totalRounds - 1) return 'Semi Finals';
        if (round === this.totalRounds - 2) return 'Quarter Finals';
        return `Round of ${Math.pow(2, this.totalRounds - round + 1)}`;
    }

    getNextMatch(match) {
        if (match.round >= this.totalRounds) return null;

        const nextRound = this.rounds[match.round];
        const nextMatchIndex = Math.floor(match.bracketPosition / 2);
        
        return nextRound?.matches[nextMatchIndex];
    }

    getNextPosition(match) {
        return match.bracketPosition % 2 === 0 ? 'slot1' : 'slot2';
    }

    generateMatches() {
        return this.matches;
    }
}

export class DoubleEliminationBracket extends TournamentBracket {
    constructor(teams) {
        super(teams);
        this.winnersBracket = [];
        this.losersBracket = [];
        this.grandFinals = null;
        this.grandFinalsCompleted = false;
        this.totalRounds = Math.ceil(Math.log2(teams.length)) * 2 - 1;
        this.generateBrackets();
    }

    generateBrackets() {
        this.winnersBracket = new SingleEliminationBracket(this.teams);
        this.matches = [...this.winnersBracket.matches];
        
        const losersRounds = (this.winnersBracket.totalRounds - 1) * 2;
        
        for (let round = 1; round <= losersRounds; round++) {
            const matchesInRound = Math.pow(2, Math.ceil((losersRounds - round + 1) / 2));
            const roundMatches = [];

            for (let i = 0; i < matchesInRound; i++) {
                const match = {
                    id: 'loser_match_' + round + '_' + i,
                    round: round,
                    bracket: 'losers',
                    team1Id: null,
                    team2Id: null,
                    status: MatchStatus.SCHEDULED,
                    result: null
                };
                roundMatches.push(match);
                this.matches.push(match);
                this.losersBracket.push(match);
            }
        }

        this.grandFinals = {
            id: 'grand_finals',
            round: this.totalRounds,
            team1Id: null,
            team2Id: null,
            status: MatchStatus.SCHEDULED,
            result: null,
            bracket: 'finals',
            isReset: false
        };
        this.matches.push(this.grandFinals);
    }

    moveToLosersBracket(match, result) {
        if (match.bracket === 'winners') {
            const loserMatch = this.losersBracket.find(m => m.status === MatchStatus.SCHEDULED);
            if (loserMatch) {
                loserMatch.team1Id = result.loser;
            }
        }
    }

    getChampion() {
        if (!this.grandFinals?.result) return null;
        return this.grandFinals.result.winner;
    }

    generateMatches() {
        return this.matches;
    }
}

export class RoundRobinBracket extends TournamentBracket {
    constructor(teams) {
        super(teams);
        this.standings = new Map();
        this.matches = [];
        this.totalRounds = teams.length - 1;
        this.generateMatches();
        this.initializeStandings();
    }

    generateMatches() {
        let matchId = 1;
        const teams = [...this.teams];

        if (teams.length % 2 !== 0) {
            teams.push({ id: 'bye', name: 'BYE', isBye: true });
        }

        const numTeams = teams.length;
        const rounds = (numTeams - 1) * 2;

        for (let round = 0; round < rounds; round++) {
            const roundMatches = [];

            for (let i = 0; i < numTeams / 2; i++) {
                const team1 = teams[i];
                const team2 = teams[numTeams - 1 - i];

                if (!team1.isBye && !team2.isBye) {
                    const match = {
                        id: 'rr_match_' + matchId++,
                        round: round + 1,
                        team1Id: team1.id,
                        team2Id: team2.id,
                        status: MatchStatus.SCHEDULED,
                        result: null,
                        bracketPosition: i
                    };
                    roundMatches.push(match);
                    this.matches.push(match);
                }
            }

            this.rounds.push({
                round: round + 1,
                matches: roundMatches
            });

            const lastTeam = teams.pop();
            teams.splice(1, 0, lastTeam);
        }
    }

    initializeStandings() {
        for (const team of this.teams) {
            this.standings.set(team.id, {
                teamId: team.id,
                wins: 0,
                losses: 0,
                draws: 0,
                points: 0,
                matchesPlayed: 0,
                roundsWon: 0,
                roundsLost: 0
            });
        }
    }

    getLeader() {
        const sorted = Array.from(this.standings.values())
            .sort((a, b) => {
                if (b.points !== a.points) return b.points - a.points;
                if ((b.wins - b.losses) !== (a.wins - a.losses)) {
                    return (b.wins - b.losses) - (a.wins - a.losses);
                }
                return (b.roundsWon - b.roundsLost) - (a.roundsWon - a.roundsLost);
            });
        
        return sorted[0]?.teamId;
    }

    getTeamAtRank(rank) {
        const sorted = Array.from(this.standings.values())
            .sort((a, b) => {
                if (b.points !== a.points) return b.points - a.points;
                return (b.roundsWon - b.roundsLost) - (a.roundsWon - a.roundsLost);
            });
        
        return sorted[rank - 1]?.teamId;
    }

    generateMatches() {
        return this.matches;
    }
}

export class SwissBracket extends TournamentBracket {
    constructor(teams) {
        super(teams);
        this.currentRound = 0;
        this.pairings = [];
        this.matchHistory = [];
        this.generateMatches();
    }

    generateMatches() {
        this.currentRound = 1;
        const rounds = Math.ceil(Math.log2(teams.length));
        
        for (let i = 0; i < rounds; i++) {
            const roundMatches = [];
            
            for (let j = 0; j < Math.floor(teams.length / 2); j++) {
                const match = {
                    id: 'swiss_match_' + (i + 1) + '_' + j,
                    round: i + 1,
                    team1Id: null,
                    team2Id: null,
                    status: MatchStatus.SCHEDULED,
                    result: null
                };
                roundMatches.push(match);
                this.matches.push(match);
            }

            this.rounds.push({
                round: i + 1,
                matches: roundMatches
            });
        }
    }

    generateMatches() {
        return this.matches;
    }
}

export class PlayerTournamentStats {
    constructor() {
        this.kills = 0;
        this.deaths = 0;
        this.assists = 0;
        this.headshots = 0;
        this.damage = 0;
        this.matchesPlayed = 0;
        this.matchesWon = 0;
        this.roundsPlayed = 0;
        this.clutchAttempts = 0;
        this.clutchesWon = 0;
        this.mvps = 0;
        this.score = 0;
    }

    addMatch(stats) {
        this.kills += stats.kills || 0;
        this.deaths += stats.deaths || 0;
        this.assists += stats.assists || 0;
        this.headshots += stats.headshots || 0;
        this.damage += stats.damage || 0;
        this.matchesPlayed++;
        if (stats.won) this.matchesWon++;
        this.roundsPlayed += stats.roundsPlayed || 0;
        if (stats.clutchWon) this.clutchesWon++;
        this.mvps += stats.mvp || 0;
        this.score += stats.score || 0;
    }

    getKD() {
        return this.deaths > 0 ? (this.kills / this.deaths).toFixed(2) : this.kills.toFixed(2);
    }

    getHSR() {
        return this.kills > 0 ? ((this.headshots / this.kills) * 100).toFixed(1) + '%' : '0%';
    }

    getADR() {
        return this.roundsPlayed > 0 ? (this.damage / this.roundsPlayed).toFixed(1) : '0';
    }
}

export class TournamentManager {
    constructor() {
        this.tournaments = new Map();
        this.activeTournaments = new Map();
        this.upcomingTournaments = new Map();
        this.completedTournaments = new Map();
        this.organizers = new Map();
        this.subscribers = new Set();
    }

    createTournament(data) {
        const tournament = new Tournament(data);
        this.tournaments.set(tournament.id, tournament);
        this.categorizeTournament(tournament);
        return tournament;
    }

    getTournament(id) {
        return this.tournaments.get(id);
    }

    getTournamentsByStatus(status) {
        return Array.from(this.tournaments.values())
            .filter(t => t.status === status);
    }

    getTournamentsByType(type) {
        return Array.from(this.tournaments.values())
            .filter(t => t.type === type);
    }

    getTournamentsByRegion(region) {
        return Array.from(this.tournaments.values())
            .filter(t => t.region === region);
    }

    getUpcomingTournaments(limit = 10) {
        const now = Date.now();
        return Array.from(this.tournaments.values())
            .filter(t => t.startDate > now)
            .sort((a, b) => a.startDate - b.startDate)
            .slice(0, limit);
    }

    getOngoingTournaments() {
        return Array.from(this.tournaments.values())
            .filter(t => t.status === TournamentStatus.IN_PROGRESS);
    }

    categorizeTournament(tournament) {
        switch (tournament.status) {
            case TournamentStatus.REGISTRATION:
            case TournamentStatus.CHECK_IN:
                this.upcomingTournaments.set(tournament.id, tournament);
                break;
            case TournamentStatus.IN_PROGRESS:
                this.activeTournaments.set(tournament.id, tournament);
                break;
            case TournamentStatus.COMPLETED:
                this.completedTournaments.set(tournament.id, tournament);
                break;
        }
    }

    updateTournamentStatus(tournamentId) {
        const tournament = this.tournaments.get(tournamentId);
        if (!tournament) return;

        const now = Date.now();

        if (tournament.registrationEnd && now >= tournament.registrationEnd && 
            tournament.status === TournamentStatus.REGISTRATION) {
            tournament.status = TournamentStatus.CHECK_IN;
        }

        if (tournament.checkInEnd && now >= tournament.checkInEnd && 
            tournament.status === TournamentStatus.CHECK_IN) {
            this.categorizeTournament(tournament);
        }

        if (tournament.status === TournamentStatus.IN_PROGRESS && tournament.isTournamentComplete()) {
            tournament.completeTournament();
            this.categorizeTournament(tournament);
        }
    }

    subscribe(callback) {
        this.subscribers.add(callback);
    }

    unsubscribe(callback) {
        this.subscribers.delete(callback);
    }

    notifySubscribers(event) {
        for (const callback of this.subscribers) {
            callback(event);
        }
    }
}

export default {
    TournamentStatus,
    TournamentFormat,
    TournamentType,
    MatchStatus,
    Tournament,
    TournamentSchedule,
    TournamentStats,
    TournamentRules,
    OvertimeRules,
    BroadcastSettings,
    TournamentBracket,
    SingleEliminationBracket,
    DoubleEliminationBracket,
    RoundRobinBracket,
    SwissBracket,
    PlayerTournamentStats,
    TournamentManager
};

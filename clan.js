export const TeamSide = {
    TERRORIST: 'terrorist',
    COUNTER_TERRORIST: 'counter_terrorist',
    SPECTATOR: 'spectator',
    NONE: 'none'
};

export const MatchQueueType = {
    COMPETITIVE: 'competitive',
    CASUAL: 'casual',
    SCrimmage: 'scrimmage',
    WINGMAN: 'wingman',
    CUSTOM: 'custom'
};

export const ClanRank = {
    OWNER: 'owner',
    ADMIN: 'admin',
    MODERATOR: 'moderator',
    MEMBER: 'member',
    RECRUIT: 'recruit',
    GUEST: 'guest'
};

export const TeamRole = {
    LEADER: 'leader',
    CAPTAIN: 'captain',
    LURKER: 'lurker',
    SUPPORT: 'support',
    ENTRY: 'entry',
    AWP: 'awp',
    MEMBER: 'member'
};

export const ClanPermission = {
    MANAGE_SETTINGS: 'manage_settings',
    MANAGE_ROLES: 'manage_roles',
    MANAGE_MEMBERS: 'manage_members',
    INVITE_MEMBERS: 'invite_members',
    KICK_MEMBERS: 'kick_members',
    BAN_MEMBERS: 'ban_members',
    MANAGE_WARS: 'manage_wars',
    CREATE_WARS: 'create_wars',
    MANAGE_FINANCES: 'manage_finances',
    MANAGE_ANNouncements: 'manage_announcements',
    MANAGE_EVENTS: 'manage_events',
    VIEW_STATS: 'view_stats',
    EDIT_BIO: 'edit_bio',
    MANAGE_INVENTORY: 'manage_inventory',
    WITHDRAW_FUNDS: 'withdraw_funds'
};

export class Clan {
    constructor(id, name) {
        this.id = id;
        this.name = name;
        this.tag = '';
        this.description = '';
        this.avatar = null;
        this.banner = null;
        this.createdAt = Date.now();
        this.updatedAt = Date.now();
        this.ownerId = null;
        this.memberCount = 0;
        this.maxMembers = 50;
        this.level = 1;
        this.experience = 0;
        this.reputation = 0;
        this.wins = 0;
        this.losses = 0;
        this.draws = 0;
        this.rank = 0;
        this.region = 'global';
        this.language = 'en';
        this.isRecruiting = true;
        this.minSkillLevel = 0;
        this.requiredJoinCode = null;
        this.motd = '';
        this.rules = [];
        this.socialLinks = {};
        this.contactEmail = '';
        this.website = '';
        this.discord = '';
        this.twitter = '';
        this.twitch = '';
        this.youtube = '';
        this.totalMatches = 0;
        this.tournamentWins = 0;
        this.currentStreak = 0;
        this.longestStreak = 0;
        this.members = new Map();
        this.pendingInvites = [];
        this.bans = [];
        this.wars = [];
        this.tournaments = [];
        this.events = [];
        this.financials = new ClanFinances();
        this.settings = new ClanSettings();
        this.inventory = new ClanInventory();
        this.battlePass = null;
        this.achievements = [];
        this.history = [];
        this.alliances = new Map();
        this.rivals = new Set();
        this.stats = new ClanStats();
    }

    addXP(amount) {
        this.experience += amount;
        while (this.experience >= this.getXPForNextLevel()) {
            this.experience -= this.getXPForNextLevel();
            this.level++;
        }
    }

    getXPForNextLevel() {
        return Math.floor(10000 * Math.pow(1.5, this.level - 1));
    }

    getProgressToNextLevel() {
        return (this.experience / this.getXPForNextLevel()) * 100;
    }

    getWinRate() {
        const total = this.wins + this.losses;
        if (total === 0) return 0;
        return (this.wins / total) * 100;
    }

    addMember(playerId, rank = ClanRank.RECRUIT) {
        if (this.members.size >= this.maxMembers) {
            return false;
        }
        const member = new ClanMember(playerId, this.id);
        member.rank = rank;
        member.joinedAt = Date.now();
        this.members.set(playerId, member);
        this.memberCount = this.members.size;
        return true;
    }

    removeMember(playerId) {
        if (!this.members.has(playerId)) {
            return false;
        }
        this.members.delete(playerId);
        this.memberCount = this.members.size;
        return true;
    }

    promoteMember(playerId) {
        const member = this.members.get(playerId);
        if (!member) return false;

        const rankOrder = [ClanRank.GUEST, ClanRank.RECRUIT, ClanRank.MEMBER, ClanRank.MODERATOR, ClanRank.ADMIN, ClanRank.OWNER];
        const currentIndex = rankOrder.indexOf(member.rank);
        if (currentIndex < rankOrder.length - 1) {
            member.rank = rankOrder[currentIndex + 1];
            member.promotedAt = Date.now();
            this.addHistory(`Member promoted: ${playerId} to ${member.rank}`);
            return true;
        }
        return false;
    }

    demoteMember(playerId) {
        const member = this.members.get(playerId);
        if (!member) return false;

        const rankOrder = [ClanRank.GUEST, ClanRank.RECRUIT, ClanRank.MEMBER, ClanRank.MODERATOR, ClanRank.ADMIN, ClanRank.OWNER];
        const currentIndex = rankOrder.indexOf(member.rank);
        if (currentIndex > 0) {
            member.rank = rankOrder[currentIndex - 1];
            member.demotedAt = Date.now();
            this.addHistory(`Member demoted: ${playerId} to ${member.rank}`);
            return true;
        }
        return false;
    }

    recordMatch(won, isWar = false) {
        this.totalMatches++;
        if (won) {
            this.wins++;
            this.currentStreak++;
            if (this.currentStreak > this.longestStreak) {
                this.longestStreak = this.currentStreak;
            }
        } else {
            this.losses++;
            this.currentStreak = 0;
        }
        if (isWar) {
            this.addXP(500);
        } else {
            this.addXP(100);
        }
    }

    invitePlayer(playerId, invitedBy) {
        if (this.pendingInvites.some(inv => inv.playerId === playerId)) {
            return false;
        }
        const invite = {
            playerId,
            invitedBy,
            invitedAt: Date.now(),
            expiresAt: Date.now() + (7 * 24 * 60 * 60 * 1000)
        };
        this.pendingInvites.push(invite);
        this.addHistory(`Player invited: ${playerId}`);
        return true;
    }

    cancelInvite(playerId) {
        this.pendingInvites = this.pendingInvites.filter(inv => inv.playerId !== playerId);
    }

    acceptInvite(playerId) {
        const index = this.pendingInvites.findIndex(inv => inv.playerId === playerId);
        if (index >= 0) {
            this.pendingInvites.splice(index, 1);
            return this.addMember(playerId);
        }
        return false;
    }

    banMember(playerId, reason, bannedBy) {
        const member = this.members.get(playerId);
        if (!member) return false;

        const ban = {
            playerId,
            reason,
            bannedBy,
            bannedAt: Date.now(),
            expiresAt: null
        };
        this.bans.push(ban);
        this.removeMember(playerId);
        this.addHistory(`Member banned: ${playerId} - ${reason}`);
        return true;
    }

    unbanMember(playerId) {
        this.bans = this.bans.filter(ban => ban.playerId !== playerId);
        this.addHistory(`Member unbanned: ${playerId}`);
    }

    isBanned(playerId) {
        return this.bans.some(ban => ban.playerId === playerId);
    }

    hasPermission(playerId, permission) {
        const member = this.members.get(playerId);
        if (!member) return false;
        return this.settings.getRankPermissions(member.rank).includes(permission);
    }

    addHistory(action) {
        this.history.push({
            action,
            timestamp: Date.now()
        });
        if (this.history.length > 100) {
            this.history.shift();
        }
    }

    getMemberList() {
        return Array.from(this.members.values()).sort((a, b) => {
            const rankOrder = [ClanRank.OWNER, ClanRank.ADMIN, ClanRank.MODERATOR, ClanRank.MEMBER, ClanRank.RECRUIT, ClanRank.GUEST];
            return rankOrder.indexOf(a.rank) - rankOrder.indexOf(b.rank);
        });
    }

    getOnlineMembers() {
        return Array.from(this.members.values()).filter(m => m.isOnline);
    }

    getTopMembers(limit = 10) {
        return Array.from(this.members.values())
            .sort((a, b) => b.contribution - a.contribution)
            .slice(0, limit);
    }

    addAlliance(clanId, level = 1) {
        this.alliances.set(clanId, { level, createdAt: Date.now() });
    }

    removeAlliance(clanId) {
        this.alliances.delete(clanId);
    }

    addRival(clanId) {
        this.rivals.add(clanId);
    }

    removeRival(clanId) {
        this.rivals.delete(clanId);
    }

    isRival(clanId) {
        return this.rivals.has(clanId);
    }

    update() {
        this.updatedAt = Date.now();
        for (const member of this.members.values()) {
            member.update();
        }
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            tag: this.tag,
            description: this.description,
            avatar: this.avatar,
            banner: this.banner,
            createdAt: this.createdAt,
            updatedAt: this.updatedAt,
            ownerId: this.ownerId,
            memberCount: this.memberCount,
            maxMembers: this.maxMembers,
            level: this.level,
            experience: this.experience,
            reputation: this.reputation,
            wins: this.wins,
            losses: this.losses,
            draws: this.draws,
            rank: this.rank,
            region: this.region,
            language: this.language,
            isRecruiting: this.isRecruiting,
            minSkillLevel: this.minSkillLevel,
            requiredJoinCode: this.requiredJoinCode,
            motd: this.motd,
            rules: this.rules,
            socialLinks: this.socialLinks,
            contactEmail: this.contactEmail,
            website: this.website,
            discord: this.discord,
            twitter: this.twitter,
            twitch: this.twitch,
            youtube: this.youtube,
            totalMatches: this.totalMatches,
            tournamentWins: this.tournamentWins,
            currentStreak: this.currentStreak,
            longestStreak: this.longestStreak,
            members: Array.from(this.members.entries()),
            pendingInvites: this.pendingInvites,
            bans: this.bans,
            wars: this.wars,
            tournaments: this.tournaments,
            events: this.events,
            financials: this.financials.serialize(),
            settings: this.settings.serialize(),
            inventory: this.inventory.serialize(),
            battlePass: this.battlePass,
            achievements: this.achievements,
            history: this.history,
            alliances: Array.from(this.alliances.entries()),
            rivals: Array.from(this.rivals)
        };
    }

    deserialize(data) {
        this.id = data.id;
        this.name = data.name;
        this.tag = data.tag;
        this.description = data.description;
        this.avatar = data.avatar;
        this.banner = data.banner;
        this.createdAt = data.createdAt;
        this.updatedAt = data.updatedAt;
        this.ownerId = data.ownerId;
        this.memberCount = data.memberCount;
        this.maxMembers = data.maxMembers;
        this.level = data.level;
        this.experience = data.experience;
        this.reputation = data.reputation;
        this.wins = data.wins;
        this.losses = data.losses;
        this.draws = data.draws;
        this.rank = data.rank;
        this.region = data.region;
        this.language = data.language;
        this.isRecruiting = data.isRecruiting;
        this.minSkillLevel = data.minSkillLevel;
        this.requiredJoinCode = data.requiredJoinCode;
        this.motd = data.motd;
        this.rules = data.rules || [];
        this.socialLinks = data.socialLinks || {};
        this.contactEmail = data.contactEmail;
        this.website = data.website;
        this.discord = data.discord;
        this.twitter = data.twitter;
        this.twitch = data.twitch;
        this.youtube = data.youtube;
        this.totalMatches = data.totalMatches;
        this.tournamentWins = data.tournamentWins;
        this.currentStreak = data.currentStreak;
        this.longestStreak = data.longestStreak;
        this.members = new Map(data.members || []);
        this.pendingInvites = data.pendingInvites || [];
        this.bans = data.bans || [];
        this.wars = data.wars || [];
        this.tournaments = data.tournaments || [];
        this.events = data.events || [];
        this.financials = new ClanFinances();
        this.financials.deserialize(data.financials || {});
        this.settings = new ClanSettings();
        this.settings.deserialize(data.settings || {});
        this.inventory = new ClanInventory();
        this.inventory.deserialize(data.inventory || {});
        this.battlePass = data.battlePass;
        this.achievements = data.achievements || [];
        this.history = data.history || [];
        this.alliances = new Map(data.alliances || []);
        this.rivals = new Set(data.rivals || []);
    }
}

export class ClanMember {
    constructor(playerId, clanId) {
        this.playerId = playerId;
        this.clanId = clanId;
        this.rank = ClanRank.RECRUIT;
        this.role = TeamRole.MEMBER;
        this.joinedAt = Date.now();
        this.lastActive = Date.now();
        this.isOnline = false;
        this.status = 'active';
        this.contribution = 0;
        this.totalContribution = 0;
        this.rankPoints = 0;
        this.level = 1;
        this.experience = 0;
        this.nickname = '';
        this.avatar = null;
        this.stats = new MemberStats();
        this.permissions = [];
        this.bans = [];
        this.warnings = [];
        this.promotedAt = null;
        this.demotedAt = null;
        this.leaveCount = 0;
        this.kickCount = 0;
        this.preferredMaps = [];
        this.preferredWeapons = [];
        this.schedule = {};
        this.notes = '';
        this.tags = [];
        this.badges = [];
        this.donations = 0;
        this.attendance = 0;
        this.participationRate = 100;
    }

    update() {
        this.lastActive = Date.now();
    }

    goOnline() {
        this.isOnline = true;
    }

    goOffline() {
        this.isOnline = false;
    }

    addContribution(amount) {
        this.contribution += amount;
        this.totalContribution += amount;
        this.addXP(Math.floor(amount / 10));
    }

    removeContribution(amount) {
        this.contribution = Math.max(0, this.contribution - amount);
    }

    addXP(amount) {
        this.experience += amount;
        while (this.experience >= this.getXPForNextLevel()) {
            this.experience -= this.getXPForNextLevel();
            this.level++;
        }
    }

    getXPForNextLevel() {
        return Math.floor(500 * Math.pow(1.3, this.level - 1));
    }

    getProgressToNextLevel() {
        return (this.experience / this.getXPForNextLevel()) * 100;
    }

    addWarning(warning) {
        this.warnings.push({
            ...warning,
            issuedAt: Date.now()
        });
    }

    removeWarning(index) {
        this.warnings.splice(index, 1);
    }

    clearWarnings() {
        this.warnings = [];
    }

    getActiveWarnings() {
        return this.warnings.filter(w => !w.resolved);
    }

    ban(reason, expiresAt = null) {
        const ban = {
            reason,
            issuedAt: Date.now(),
            expiresAt
        };
        this.bans.push(ban);
    }

    isBanned() {
        return this.bans.some(ban => !ban.expiresAt || ban.expiresAt > Date.now());
    }

    updateStats(matchResult) {
        this.stats.recordMatch(matchResult);
    }

    updateAttendance(participated) {
        this.attendance++;
        if (participated) {
            this.participationRate = (this.participationRate + 100) / 2;
        } else {
            this.participationRate = this.participationRate / 2;
        }
    }

    serialize() {
        return {
            playerId: this.playerId,
            clanId: this.clanId,
            rank: this.rank,
            role: this.role,
            joinedAt: this.joinedAt,
            lastActive: this.lastActive,
            isOnline: this.isOnline,
            status: this.status,
            contribution: this.contribution,
            totalContribution: this.totalContribution,
            rankPoints: this.rankPoints,
            level: this.level,
            experience: this.experience,
            nickname: this.nickname,
            avatar: this.avatar,
            stats: this.stats.serialize(),
            permissions: this.permissions,
            bans: this.bans,
            warnings: this.warnings,
            promotedAt: this.promotedAt,
            demotedAt: this.demotedAt,
            leaveCount: this.leaveCount,
            kickCount: this.kickCount,
            preferredMaps: this.preferredMaps,
            preferredWeapons: this.preferredWeapons,
            schedule: this.schedule,
            notes: this.notes,
            tags: this.tags,
            badges: this.badges,
            donations: this.donations,
            attendance: this.attendance,
            participationRate: this.participationRate
        };
    }

    deserialize(data) {
        this.playerId = data.playerId;
        this.clanId = data.clanId;
        this.rank = data.rank;
        this.role = data.role;
        this.joinedAt = data.joinedAt;
        this.lastActive = data.lastActive;
        this.isOnline = data.isOnline;
        this.status = data.status;
        this.contribution = data.contribution;
        this.totalContribution = data.totalContribution;
        this.rankPoints = data.rankPoints;
        this.level = data.level;
        this.experience = data.experience;
        this.nickname = data.nickname;
        this.avatar = data.avatar;
        this.stats = new MemberStats();
        this.stats.deserialize(data.stats || {});
        this.permissions = data.permissions || [];
        this.bans = data.bans || [];
        this.warnings = data.warnings || [];
        this.promotedAt = data.promotedAt;
        this.demotedAt = data.demotedAt;
        this.leaveCount = data.leaveCount || 0;
        this.kickCount = data.kickCount || 0;
        this.preferredMaps = data.preferredMaps || [];
        this.preferredWeapons = data.preferredWeapons || [];
        this.schedule = data.schedule || {};
        this.notes = data.notes || '';
        this.tags = data.tags || [];
        this.badges = data.badges || [];
        this.donations = data.donations || 0;
        this.attendance = data.attendance || 0;
        this.participationRate = data.participationRate || 100;
    }
}

export class MemberStats {
    constructor() {
        this.matchesPlayed = 0;
        this.matchesWon = 0;
        this.matchesLost = 0;
        this.roundsPlayed = 0;
        this.roundsWon = 0;
        this.kills = 0;
        this.deaths = 0;
        this.assists = 0;
        this.headshots = 0;
        this.mvps = 0;
        this.damage = 0;
        this.aces = 0;
        this.clutches = 0;
        this.clutchesWon = 0;
        this.entries = 0;
        this.firstKills = 0;
        this.firstDeaths = 0;
        this.trades = 0;
        this.flashAssists = 0;
        this.bombsPlanted = 0;
        this.bombsDefused = 0;
        this.hostagesRescued = 0;
    }

    recordMatch(result) {
        this.matchesPlayed++;
        if (result === 'won') this.matchesWon++;
        else if (result === 'lost') this.matchesLost++;
    }

    recordRound(won) {
        this.roundsPlayed++;
        if (won) this.roundsWon++;
    }

    getKDRatio() {
        if (this.deaths === 0) return this.kills;
        return this.kills / this.deaths;
    }

    getWinRate() {
        if (this.matchesPlayed === 0) return 0;
        return (this.matchesWon / this.matchesPlayed) * 100;
    }

    getHeadshotRate() {
        if (this.kills === 0) return 0;
        return (this.headshots / this.kills) * 100;
    }

    getAverageKills() {
        if (this.matchesPlayed === 0) return 0;
        return this.kills / this.matchesPlayed;
    }

    getAverageDamage() {
        if (this.matchesPlayed === 0) return 0;
        return this.damage / this.matchesPlayed;
    }

    serialize() {
        return {
            matchesPlayed: this.matchesPlayed,
            matchesWon: this.matchesWon,
            matchesLost: this.matchesLost,
            roundsPlayed: this.roundsPlayed,
            roundsWon: this.roundsWon,
            kills: this.kills,
            deaths: this.deaths,
            assists: this.assists,
            headshots: this.headshots,
            mvps: this.mvps,
            damage: this.damage,
            aces: this.aces,
            clutches: this.clutches,
            clutchesWon: this.clutchesWon,
            entries: this.entries,
            firstKills: this.firstKills,
            firstDeaths: this.firstDeaths,
            trades: this.trades,
            flashAssists: this.flashAssists,
            bombsPlanted: this.bombsPlanted,
            bombsDefused: this.bombsDefused,
            hostagesRescued: this.hostagesRescued
        };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class ClanWar {
    constructor(id, clanId, opponentId) {
        this.id = id;
        this.clanId = clanId;
        this.opponentId = opponentId;
        this.status = 'pending';
        this.type = 'competitive';
        this.format = 'best_of_1';
        this.maps = [];
        this.currentMap = null;
        this.currentRound = 0;
        this.scores = { home: 0, away: 0 };
        this.schedule = null;
        this.startedAt = null;
        this.endedAt = null;
        this.duration = 0;
        this.rounds = [];
        this.players = new Map();
        this.captains = { home: null, away: null };
        this.mapsBanned = [];
        this.mapsPicked = [];
        this.prizes = [];
        this.skillBracket = null;
        this.region = 'global';
        this.bestOf = 1;
        this.homeRounds = 0;
        this.awayRounds = 0;
        this.winner = null;
        this.surrenderedBy = null;
    }

    start() {
        this.status = 'active';
        this.startedAt = Date.now();
    }

    end(winner, surrendered = false) {
        this.status = 'finished';
        this.endedAt = Date.now();
        this.duration = this.endedAt - this.startedAt;
        this.winner = winner;
        if (surrendered) {
            this.surrenderedBy = winner === this.clanId ? 'away' : 'home';
        }
    }

    recordRound(roundNumber, winner, condition) {
        this.rounds.push({
            roundNumber,
            winner,
            condition,
            timestamp: Date.now()
        });
        if (winner === 'home') {
            this.homeRounds++;
        } else {
            this.awayRounds++;
        }
    }

    checkWinner() {
        const roundsNeeded = Math.ceil(this.bestOf / 2);
        if (this.homeRounds >= roundsNeeded) {
            return this.clanId;
        }
        if (this.awayRounds >= roundsNeeded) {
            return this.opponentId;
        }
        return null;
    }

    isFull() {
        return this.homeRounds + this.awayRounds >= this.bestOf;
    }

    serialize() {
        return {
            id: this.id,
            clanId: this.clanId,
            opponentId: this.opponentId,
            status: this.status,
            type: this.type,
            format: this.format,
            maps: this.maps,
            currentMap: this.currentMap,
            currentRound: this.currentRound,
            scores: this.scores,
            schedule: this.schedule,
            startedAt: this.startedAt,
            endedAt: this.endedAt,
            duration: this.duration,
            rounds: this.rounds,
            players: Array.from(this.players.entries()),
            captains: this.captains,
            mapsBanned: this.mapsBanned,
            mapsPicked: this.mapsPicked,
            prizes: this.prizes,
            skillBracket: this.skillBracket,
            region: this.region,
            bestOf: this.bestOf,
            homeRounds: this.homeRounds,
            awayRounds: this.awayRounds,
            winner: this.winner,
            surrenderedBy: this.surrenderedBy
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.players = new Map(data.players || []);
    }
}

export class ClanSettings {
    constructor() {
        this.rankPermissions = {
            [ClanRank.OWNER]: Object.values(ClanPermission),
            [ClanRank.ADMIN]: [
                ClanPermission.MANAGE_MEMBERS,
                ClanPermission.INVITE_MEMBERS,
                ClanPermission.KICK_MEMBERS,
                ClanPermission.VIEW_STATS,
                ClanPermission.EDIT_BIO,
                ClanPermission.MANAGE_WARS
            ],
            [ClanRank.MODERATOR]: [
                ClanPermission.INVITE_MEMBERS,
                ClanPermission.VIEW_STATS,
                ClanPermission.EDIT_BIO
            ],
            [ClanRank.MEMBER]: [
                ClanPermission.VIEW_STATS,
                ClanPermission.EDIT_BIO
            ],
            [ClanRank.RECRUIT]: [
                ClanPermission.VIEW_STATS
            ],
            [ClanRank.GUEST]: []
        };
        this.autoKick = {
            enabled: false,
            inactiveDays: 30,
            lowContribution: false,
            minContribution: 1000
        };
        this.warSettings = {
            defaultFormat: 'best_of_1',
            defaultSkillBracket: null,
            autoAcceptWars: false,
            maxWarsPerWeek: 3,
            requireFullTeam: true
        };
        this.privacy = {
            isPublic: true,
            showStats: true,
            showMembers: true,
            showWars: true,
            showAchievements: true,
            allowFriendInvites: true
        };
        this.notifications = {
            newMember: true,
            memberLeft: true,
            warScheduled: true,
            warStarted: true,
            warEnded: true,
            tournamentUpdate: true,
            announcement: true
        };
        this.ui = {
            theme: 'default',
            colorScheme: 'blue',
            language: 'en'
        };
    }

    getRankPermissions(rank) {
        return this.rankPermissions[rank] || [];
    }

    setRankPermissions(rank, permissions) {
        this.rankPermissions[rank] = permissions;
    }

    clone() {
        const cloned = new ClanSettings();
        cloned.rankPermissions = { ...this.rankPermissions };
        cloned.autoKick = { ...this.autoKick };
        cloned.warSettings = { ...this.warSettings };
        cloned.privacy = { ...this.privacy };
        cloned.notifications = { ...this.notifications };
        cloned.ui = { ...this.ui };
        return cloned;
    }

    serialize() {
        return {
            rankPermissions: this.rankPermissions,
            autoKick: this.autoKick,
            warSettings: this.warSettings,
            privacy: this.privacy,
            notifications: this.notifications,
            ui: this.ui
        };
    }

    deserialize(data) {
        if (data.rankPermissions) this.rankPermissions = data.rankPermissions;
        if (data.autoKick) this.autoKick = data.autoKick;
        if (data.warSettings) this.warSettings = data.warSettings;
        if (data.privacy) this.privacy = data.privacy;
        if (data.notifications) this.notifications = data.notifications;
        if (data.ui) this.ui = data.ui;
    }
}

export class ClanFinances {
    constructor() {
        this.currency = 'coins';
        this.balance = 0;
        this.totalEarned = 0;
        this.totalSpent = 0;
        this.transactions = [];
        this.donations = new Map();
        this.bounties = [];
        this.investments = [];
        this.taxRate = 0;
        this.donationEnabled = true;
        this.minDonation = 0;
        this.maxDonation = 10000;
        this.donationRewards = {};
        this.salary = {
            enabled: false,
            interval: 'weekly',
            amounts: {}
        };
    }

    deposit(amount, source = 'match') {
        this.balance += amount;
        this.totalEarned += amount;
        this.addTransaction('deposit', amount, source);
    }

    withdraw(amount, destination = 'prize') {
        if (amount > this.balance) return false;
        this.balance -= amount;
        this.totalSpent += amount;
        this.addTransaction('withdrawal', -amount, destination);
        return true;
    }

    addTransaction(type, amount, description) {
        this.transactions.push({
            type,
            amount,
            description,
            timestamp: Date.now()
        });
        if (this.transactions.length > 500) {
            this.transactions.shift();
        }
    }

    receiveDonation(playerId, amount) {
        this.balance += amount;
        this.totalEarned += amount;
        const current = this.donations.get(playerId) || 0;
        this.donations.set(playerId, current + amount);
        this.addTransaction('donation', amount, `Player ${playerId} donation`);
    }

    getDonationLeaderboard(limit = 10) {
        return Array.from(this.donations.entries())
            .sort((a, b) => b[1] - a[1])
            .slice(0, limit);
    }

    paySalary() {
        if (!this.salary.enabled) return;
        for (const [playerId, amount] of Object.entries(this.salary.amounts)) {
            if (this.balance >= amount) {
                this.withdraw(amount, `Salary for ${playerId}`);
            }
        }
    }

    serialize() {
        return {
            currency: this.currency,
            balance: this.balance,
            totalEarned: this.totalEarned,
            totalSpent: this.totalSpent,
            transactions: this.transactions,
            donations: Array.from(this.donations.entries()),
            bounties: this.bounties,
            investments: this.investments,
            taxRate: this.taxRate,
            donationEnabled: this.donationEnabled,
            minDonation: this.minDonation,
            maxDonation: this.maxDonation,
            donationRewards: this.donationRewards,
            salary: this.salary
        };
    }

    deserialize(data) {
        this.currency = data.currency || 'coins';
        this.balance = data.balance || 0;
        this.totalEarned = data.totalEarned || 0;
        this.totalSpent = data.totalSpent || 0;
        this.transactions = data.transactions || [];
        this.donations = new Map(data.donations || []);
        this.bounties = data.bounties || [];
        this.investments = data.investments || [];
        this.taxRate = data.taxRate || 0;
        this.donationEnabled = data.donationEnabled ?? true;
        this.minDonation = data.minDonation || 0;
        this.maxDonation = data.maxDonation || 10000;
        this.donationRewards = data.donationRewards || {};
        this.salary = data.salary || { enabled: false, interval: 'weekly', amounts: {} };
    }
}

export class ClanInventory {
    constructor() {
        this.items = new Map();
        this.cosmetics = {
            avatars: [],
            banners: [],
            badges: [],
            borders: [],
            themes: []
        };
        this.upgrades = {
            clanVault: 0,
            trainingFacility: 0,
            recruitmentBonus: 0,
            warBonus: 0,
            trophyRoom: 0
        };
        this.achievements = [];
        this.trophies = [];
    }

    addItem(itemId, quantity = 1) {
        const current = this.items.get(itemId) || 0;
        this.items.set(itemId, current + quantity);
    }

    removeItem(itemId, quantity = 1) {
        const current = this.items.get(itemId) || 0;
        const newAmount = current - quantity;
        if (newAmount <= 0) {
            this.items.delete(itemId);
        } else {
            this.items.set(itemId, newAmount);
        }
    }

    hasItem(itemId) {
        return this.items.has(itemId) && this.items.get(itemId) > 0;
    }

    serialize() {
        return {
            items: Array.from(this.items.entries()),
            cosmetics: this.cosmetics,
            upgrades: this.upgrades,
            achievements: this.achievements,
            trophies: this.trophies
        };
    }

    deserialize(data) {
        this.items = new Map(data.items || []);
        this.cosmetics = data.cosmetics || {
            avatars: [],
            banners: [],
            badges: [],
            borders: [],
            themes: []
        };
        this.upgrades = data.upgrades || {
            clanVault: 0,
            trainingFacility: 0,
            recruitmentBonus: 0,
            warBonus: 0,
            trophyRoom: 0
        };
        this.achievements = data.achievements || [];
        this.trophies = data.trophies || [];
    }
}

export class ClanStats {
    constructor() {
        this.totalMatches = 0;
        this.wins = 0;
        this.losses = 0;
        this.draws = 0;
        this.warWins = 0;
        this.warLosses = 0;
        this.tournamentWins = 0;
        this.totalKills = 0;
        this.totalDeaths = 0;
        this.totalAssists = 0;
        this.totalDamage = 0;
        this.headshots = 0;
        this.aces = 0;
        this.clutches = 0;
        this.mapsPlayed = new Map();
        this.weaponsUsed = new Map();
        this.periodStats = {
            daily: new Map(),
            weekly: new Map(),
            monthly: new Map()
        };
    }

    recordMatch(result, stats) {
        this.totalMatches++;
        if (result === 'won') this.wins++;
        else if (result === 'lost') this.losses++;
        else this.draws++;

        if (stats) {
            this.totalKills += stats.kills || 0;
            this.totalDeaths += stats.deaths || 0;
            this.totalAssists += stats.assists || 0;
            this.totalDamage += stats.damage || 0;
            this.headshots += stats.headshots || 0;
            this.aces += stats.aces || 0;
            this.clutches += stats.clutches || 0;

            if (stats.map) {
                const current = this.mapsPlayed.get(stats.map) || 0;
                this.mapsPlayed.set(stats.map, current + 1);
            }

            if (stats.weapon) {
                const current = this.weaponsUsed.get(stats.weapon) || 0;
                this.weaponsUsed.set(stats.weapon, current + 1);
            }
        }
    }

    getKDRatio() {
        if (this.totalDeaths === 0) return this.totalKills;
        return this.totalKills / this.totalDeaths;
    }

    getWinRate() {
        const total = this.wins + this.losses;
        if (total === 0) return 0;
        return (this.wins / total) * 100;
    }

    getTopMap() {
        let topMap = null;
        let maxPlays = 0;
        for (const [map, plays] of this.mapsPlayed) {
            if (plays > maxPlays) {
                maxPlays = plays;
                topMap = map;
            }
        }
        return topMap;
    }

    getTopWeapon() {
        let topWeapon = null;
        let maxUses = 0;
        for (const [weapon, uses] of this.weaponsUsed) {
            if (uses > maxUses) {
                maxUses = uses;
                topWeapon = weapon;
            }
        }
        return topWeapon;
    }

    serialize() {
        return {
            totalMatches: this.totalMatches,
            wins: this.wins,
            losses: this.losses,
            draws: this.draws,
            warWins: this.warWins,
            warLosses: this.warLosses,
            tournamentWins: this.tournamentWins,
            totalKills: this.totalKills,
            totalDeaths: this.totalDeaths,
            totalAssists: this.totalAssists,
            totalDamage: this.totalDamage,
            headshots: this.headshots,
            aces: this.aces,
            clutches: this.clutches,
            mapsPlayed: Array.from(this.mapsPlayed.entries()),
            weaponsUsed: Array.from(this.weaponsUsed.entries())
        };
    }

    deserialize(data) {
        this.totalMatches = data.totalMatches || 0;
        this.wins = data.wins || 0;
        this.losses = data.losses || 0;
        this.draws = data.draws || 0;
        this.warWins = data.warWins || 0;
        this.warLosses = data.warLosses || 0;
        this.tournamentWins = data.tournamentWins || 0;
        this.totalKills = data.totalKills || 0;
        this.totalDeaths = data.totalDeaths || 0;
        this.totalAssists = data.totalAssists || 0;
        this.totalDamage = data.totalDamage || 0;
        this.headshots = data.headshots || 0;
        this.aces = data.aces || 0;
        this.clutches = data.clutches || 0;
        this.mapsPlayed = new Map(data.mapsPlayed || []);
        this.weaponsUsed = new Map(data.weaponsUsed || []);
    }
}

export class Team {
    constructor(id, clanId = null) {
        this.id = id;
        this.clanId = clanId;
        this.name = '';
        this.tag = '';
        this.members = [];
        this.leaderId = null;
        this.captainId = null;
        this.sortedPlayers = [];
        this.roleAssignments = new Map();
        this.stats = new TeamStats();
        this.wins = 0;
        this.losses = 0;
        this.draws = 0;
        this.rating = 1000;
        this.rank = 0;
        this.form = [];
        this.currentStreak = 0;
        this.longestWinStreak = 0;
        this.longestLossStreak = 0;
        this.preferredMaps = [];
        this.bannedMaps = [];
        this.settings = new TeamSettings();
        this.readyStatus = new Map();
        this.isReady = false;
        this.isFull = false;
        this.isLocked = false;
    }

    addMember(playerId, role = TeamRole.MEMBER) {
        if (this.members.length >= 5) {
            return false;
        }
        if (this.members.includes(playerId)) {
            return false;
        }
        this.members.push(playerId);
        this.roleAssignments.set(playerId, role);
        this.updateReadyStatus(playerId, false);
        if (this.members.length === 5) {
            this.isFull = true;
        }
        return true;
    }

    removeMember(playerId) {
        const index = this.members.indexOf(playerId);
        if (index >= 0) {
            this.members.splice(index, 1);
            this.roleAssignments.delete(playerId);
            this.readyStatus.delete(playerId);
            this.isFull = false;
            return true;
        }
        return false;
    }

    setLeader(playerId) {
        if (this.members.includes(playerId)) {
            this.leaderId = playerId;
            this.roleAssignments.set(playerId, TeamRole.LEADER);
            return true;
        }
        return false;
    }

    setCaptain(playerId) {
        if (this.members.includes(playerId)) {
            this.captainId = playerId;
            this.roleAssignments.set(playerId, TeamRole.CAPTAIN);
            return true;
        }
        return false;
    }

    assignRole(playerId, role) {
        if (this.members.includes(playerId)) {
            this.roleAssignments.set(playerId, role);
            return true;
        }
        return false;
    }

    updateReadyStatus(playerId, ready) {
        this.readyStatus.set(playerId, ready);
        this.updateIsReady();
    }

    updateIsReady() {
        this.isReady = this.members.length === 5 && Array.from(this.readyStatus.values()).every(r => r);
    }

    lock() {
        this.isLocked = true;
    }

    unlock() {
        this.isLocked = false;
    }

    recordMatch(result, stats = null) {
        if (result === 'won') {
            this.wins++;
            this.currentStreak++;
            if (this.currentStreak > this.longestWinStreak) {
                this.longestWinStreak = this.currentStreak;
            }
        } else if (result === 'lost') {
            this.losses++;
            this.currentStreak = Math.min(0, this.currentStreak - 1);
            if (-this.currentStreak > this.longestLossStreak) {
                this.longestLossStreak = -this.currentStreak;
            }
        } else {
            this.draws++;
        }

        this.form.push(result.charAt(0).toUpperCase());
        if (this.form.length > 10) {
            this.form.shift();
        }

        if (stats) {
            this.stats.recordMatch(result, stats);
        }

        this.updateRating(result);
    }

    updateRating(result) {
        const K = 32;
        let expected = 0.5;
        if (result === 'won') expected = 0.8;
        else if (result === 'lost') expected = 0.2;

        const change = Math.round(K * (1 - expected));
        this.rating = Math.max(0, this.rating + change);
    }

    pickMap(mapId) {
        if (!this.preferredMaps.includes(mapId)) {
            this.preferredMaps.push(mapId);
        }
    }

    banMap(mapId) {
        if (!this.bannedMaps.includes(mapId)) {
            this.bannedMaps.push(mapId);
        }
    }

    getForm() {
        return this.form.join('');
    }

    serialize() {
        return {
            id: this.id,
            clanId: this.clanId,
            name: this.name,
            tag: this.tag,
            members: this.members,
            leaderId: this.leaderId,
            captainId: this.captainId,
            sortedPlayers: this.sortedPlayers,
            roleAssignments: Array.from(this.roleAssignments.entries()),
            stats: this.stats.serialize(),
            wins: this.wins,
            losses: this.losses,
            draws: this.draws,
            rating: this.rating,
            rank: this.rank,
            form: this.form,
            currentStreak: this.currentStreak,
            longestWinStreak: this.longestWinStreak,
            longestLossStreak: this.longestLossStreak,
            preferredMaps: this.preferredMaps,
            bannedMaps: this.bannedMaps,
            settings: this.settings.serialize(),
            readyStatus: Array.from(this.readyStatus.entries()),
            isReady: this.isReady,
            isFull: this.isFull,
            isLocked: this.isLocked
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.roleAssignments = new Map(data.roleAssignments || []);
        this.stats = new TeamStats();
        this.stats.deserialize(data.stats || {});
        this.readyStatus = new Map(data.readyStatus || []);
    }
}

export class TeamSettings {
    constructor() {
        this.voiceChatEnabled = true;
        this.pushToTalk = true;
        this.crossTeamChat = false;
        this.autoBalance = true;
        this.fillEmptySlots = true;
        this.maxPing = 150;
        this.preferredRegion = 'auto';
        this.skillRange = 500;
        this.allowSubstitutes = true;
        this.substituteTimeout = 300;
        this.readyCheckTimeout = 60;
        this.autoReady = false;
        this.rolesLocked = false;
        this.mapsLocked = false;
        this.formation = 'default';
        this.communicationStyle = 'balanced';
    }

    clone() {
        const cloned = new TeamSettings();
        Object.assign(cloned, this);
        return cloned;
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class TeamStats {
    constructor() {
        this.matchesPlayed = 0;
        this.wins = 0;
        this.losses = 0;
        this.draws = 0;
        this.roundsPlayed = 0;
        this.roundsWon = 0;
        this.totalKills = 0;
        this.totalDeaths = 0;
        this.totalAssists = 0;
        this.totalDamage = 0;
        this.headshots = 0;
        this.clutches = 0;
        this.clutchesWon = 0;
        this.aces = 0;
        this.flashAssists = 0;
        this.trades = 0;
    }

    recordMatch(result, stats = null) {
        this.matchesPlayed++;
        if (result === 'won') this.wins++;
        else if (result === 'lost') this.losses++;
        else this.draws++;

        if (stats) {
            this.totalKills += stats.kills || 0;
            this.totalDeaths += stats.deaths || 0;
            this.totalAssists += stats.assists || 0;
            this.totalDamage += stats.damage || 0;
            this.headshots += stats.headshots || 0;
            this.clutches += stats.clutches || 0;
            this.clutchesWon += stats.clutchesWon || 0;
            this.aces += stats.aces || 0;
            this.flashAssists += stats.flashAssists || 0;
            this.trades += stats.trades || 0;
        }
    }

    serialize() {
        return {
            matchesPlayed: this.matchesPlayed,
            wins: this.wins,
            losses: this.losses,
            draws: this.draws,
            roundsPlayed: this.roundsPlayed,
            roundsWon: this.roundsWon,
            totalKills: this.totalKills,
            totalDeaths: this.totalDeaths,
            totalAssists: this.totalAssists,
            totalDamage: this.totalDamage,
            headshots: this.headshots,
            clutches: this.clutches,
            clutchesWon: this.clutchesWon,
            aces: this.aces,
            flashAssists: this.flashAssists,
            trades: this.trades
        };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class ClanManager {
    constructor() {
        this.clans = new Map();
        this.teams = new Map();
        this.playerClans = new Map();
        this.playerTeams = new Map();
        this.leaderboards = new Map();
        this.pendingWars = [];
        this.tournaments = [];
        this.matches = [];
        this.eventListeners = new Map();
    }

    createClan(name, ownerId) {
        const clanId = 'clan_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        const clan = new Clan(clanId, name);
        clan.ownerId = ownerId;
        clan.addMember(ownerId, ClanRank.OWNER);
        this.clans.set(clanId, clan);
        this.playerClans.set(ownerId, clanId);
        this.emit('clanCreated', { clan });
        return clan;
    }

    deleteClan(clanId) {
        const clan = this.clans.get(clanId);
        if (!clan) return false;

        for (const memberId of clan.members.keys()) {
            this.playerClans.delete(memberId);
        }
        this.clans.delete(clanId);
        this.emit('clanDeleted', { clanId });
        return true;
    }

    getClan(clanId) {
        return this.clans.get(clanId);
    }

    getClanByTag(tag) {
        for (const clan of this.clans.values()) {
            if (clan.tag.toLowerCase() === tag.toLowerCase()) {
                return clan;
            }
        }
        return null;
    }

    getPlayerClan(playerId) {
        const clanId = this.playerClans.get(playerId);
        return clanId ? this.clans.get(clanId) : null;
    }

    createTeam(clanId = null) {
        const teamId = 'team_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        const team = new Team(teamId, clanId);
        this.teams.set(teamId, team);
        return team;
    }

    getTeam(teamId) {
        return this.teams.get(teamId);
    }

    searchClans(criteria) {
        const results = [];
        for (const clan of this.clans.values()) {
            if (criteria.name && !clan.name.toLowerCase().includes(criteria.name.toLowerCase())) continue;
            if (criteria.tag && !clan.tag.toLowerCase().includes(criteria.tag.toLowerCase())) continue;
            if (criteria.minMembers && clan.memberCount < criteria.minMembers) continue;
            if (criteria.maxMembers && clan.memberCount > criteria.maxMembers) continue;
            if (criteria.minLevel && clan.level < criteria.minLevel) continue;
            if (criteria.recruitingOnly && !clan.isRecruiting) continue;
            if (criteria.region && clan.region !== criteria.region) continue;
            results.push(clan);
        }
        return results;
    }

    inviteToClan(clanId, playerId, invitedBy) {
        const clan = this.clans.get(clanId);
        if (!clan) return false;
        if (clan.isBanned(playerId)) return false;
        if (clan.memberCount >= clan.maxMembers) return false;
        return clan.invitePlayer(playerId, invitedBy);
    }

    joinClan(clanId, playerId, joinCode = null) {
        const clan = this.clans.get(clanId);
        if (!clan) return false;
        if (!clan.isRecruiting) return false;
        if (clan.memberCount >= clan.maxMembers) return false;
        if (clan.isBanned(playerId)) return false;
        if (clan.requiredJoinCode && clan.requiredJoinCode !== joinCode) return false;
        return clan.addMember(playerId);
    }

    leaveClan(clanId, playerId) {
        const clan = this.clans.get(clanId);
        if (!clan) return false;
        if (clan.ownerId === playerId) return false;
        this.playerClans.delete(playerId);
        return clan.removeMember(playerId);
    }

    challengeClan(clanId, opponentId, settings = {}) {
        const warId = 'war_' + Date.now();
        const war = new ClanWar(warId, clanId, opponentId);
        Object.assign(war, settings);
        this.pendingWars.push(war);
        return war;
    }

    acceptWar(warId) {
        const war = this.pendingWars.find(w => w.id === warId);
        if (!war) return false;
        war.status = 'accepted';
        return true;
    }

    declineWar(warId) {
        const index = this.pendingWars.findIndex(w => w.id === warId);
        if (index >= 0) {
            this.pendingWars.splice(index, 1);
            return true;
        }
        return false;
    }

    updateLeaderboards() {
        const sortedClans = Array.from(this.clans.values())
            .sort((a, b) => b.reputation - a.reputation);

        this.leaderboards.set('reputation', sortedClans);
        this.leaderboards.set('wins', sortedClans.sort((a, b) => b.wins - a.wins));
        this.leaderboards.set('winRate', sortedClans.sort((a, b) => b.getWinRate() - a.getWinRate()));
        this.leaderboards.set('level', sortedClans.sort((a, b) => b.level - a.level));
        this.leaderboards.set('activity', sortedClans.sort((a, b) => b.memberCount - a.memberCount));
    }

    getLeaderboard(category = 'reputation', limit = 100) {
        const board = this.leaderboards.get(category) || [];
        return board.slice(0, limit);
    }

    getClanRank(clanId, category = 'reputation') {
        const board = this.leaderboards.get(category) || [];
        const index = board.findIndex(c => c.id === clanId);
        return index >= 0 ? index + 1 : null;
    }

    on(event, callback) {
        if (!this.eventListeners.has(event)) {
            this.eventListeners.set(event, []);
        }
        this.eventListeners.get(event).push(callback);
    }

    off(event, callback) {
        if (this.eventListeners.has(event)) {
            const listeners = this.eventListeners.get(event);
            const index = listeners.indexOf(callback);
            if (index >= 0) {
                listeners.splice(index, 1);
            }
        }
    }

    emit(event, data) {
        if (this.eventListeners.has(event)) {
            for (const callback of this.eventListeners.get(event)) {
                callback(data);
            }
        }
    }

    saveState() {
        return {
            clans: Array.from(this.clans.entries()).map(([id, clan]) => [id, clan.serialize()]),
            teams: Array.from(this.teams.entries()).map(([id, team]) => [id, team.serialize()]),
            playerClans: Array.from(this.playerClans.entries()),
            playerTeams: Array.from(this.playerTeams.entries()),
            pendingWars: this.pendingWars,
            tournaments: this.tournaments,
            matches: this.matches
        };
    }

    loadState(state) {
        this.clans = new Map(state.clans?.map(([id, clanData]) => {
            const clan = new Clan(id);
            clan.deserialize(clanData);
            return [id, clan];
        }) || []);
        this.teams = new Map(state.teams?.map(([id, teamData]) => {
            const team = new Team(id);
            team.deserialize(teamData);
            return [id, team];
        }) || []);
        this.playerClans = new Map(state.playerClans || []);
        this.playerTeams = new Map(state.playerTeams || []);
        this.pendingWars = state.pendingWars || [];
        this.tournaments = state.tournaments || [];
        this.matches = state.matches || [];
    }
}

export default {
    TeamSide,
    MatchQueueType,
    ClanRank,
    TeamRole,
    ClanPermission,
    Clan,
    ClanMember,
    MemberStats,
    ClanWar,
    ClanSettings,
    ClanFinances,
    ClanInventory,
    ClanStats,
    Team,
    TeamSettings,
    TeamStats,
    ClanManager
};

import { GameState } from './config.js';

export const RankTier = {
    SILVER: 'silver',
    GOLD: 'gold',
    PLATINUM: 'platinum',
    DIAMOND: 'diamond',
    MASTER: 'master',
    ELITE: 'elite',
    GLOBAL: 'global'
};

export const RankLevel = {
    SILVER_1: { tier: RankTier.SILVER, level: 1, name: 'Silver I', icon: 'silver1' },
    SILVER_2: { tier: RankTier.SILVER, level: 2, name: 'Silver II', icon: 'silver2' },
    SILVER_3: { tier: RankTier.SILVER, level: 3, name: 'Silver III', icon: 'silver3' },
    SILVER_4: { tier: RankTier.SILVER, level: 4, name: 'Silver IV', icon: 'silver4' },
    SILVER_5: { tier: RankTier.SILVER, level: 5, name: 'Silver Elite', icon: 'silver5' },
    SILVER_6: { tier: RankTier.SILVER, level: 6, name: 'Silver Elite Master', icon: 'silver6' },
    GOLD_1: { tier: RankTier.GOLD, level: 7, name: 'Gold Nova I', icon: 'gold1' },
    GOLD_2: { tier: RankTier.GOLD, level: 8, name: 'Gold Nova II', icon: 'gold2' },
    GOLD_3: { tier: RankTier.GOLD, level: 9, name: 'Gold Nova III', icon: 'gold3' },
    GOLD_4: { tier: RankTier.GOLD, level: 10, name: 'Gold Nova Master', icon: 'gold4' },
    PLATINUM_1: { tier: RankTier.PLATINUM, level: 11, name: 'Master Guardian I', icon: 'platinum1' },
    PLATINUM_2: { tier: RankTier.PLATINUM, level: 12, name: 'Master Guardian II', icon: 'platinum2' },
    PLATINUM_3: { tier: RankTier.PLATINUM, level: 13, name: 'Master Guardian Elite', icon: 'platinum3' },
    PLATINUM_4: { tier: RankTier.PLATINUM, level: 14, name: 'Distinguished Master Guardian', icon: 'platinum4' },
    DIAMOND_1: { tier: RankTier.DIAMOND, level: 15, name: 'Legendary Eagle', icon: 'diamond1' },
    DIAMOND_2: { tier: RankTier.DIAMOND, level: 16, name: 'Legendary Eagle Master', icon: 'diamond2' },
    DIAMOND_3: { tier: RankTier.DIAMOND, level: 17, name: 'Supreme Master First Class', icon: 'diamond3' },
    DIAMOND_4: { tier: RankTier.DIAMOND, level: 18, name: 'The Global Elite', icon: 'diamond4' },
    MASTER_1: { tier: RankTier.MASTER, level: 19, name: 'Master I', icon: 'master1' },
    MASTER_2: { tier: RankTier.MASTER, level: 20, name: 'Master II', icon: 'master2' },
    MASTER_3: { tier: RankTier.MASTER, level: 21, name: 'Master III', icon: 'master3' },
    MASTER_4: { tier: RankTier.MASTER, level: 22, name: 'Master IV', icon: 'master4' },
    ELITE_1: { tier: RankTier.ELITE, level: 23, name: 'Elite I', icon: 'elite1' },
    ELITE_2: { tier: RankTier.ELITE, level: 24, name: 'Elite II', icon: 'elite2' },
    ELITE_3: { tier: RankTier.ELITE, level: 25, name: 'Elite III', icon: 'elite3' },
    ELITE_4: { tier: RankTier.ELITE, level: 26, name: 'Elite IV', icon: 'elite4' },
    GLOBAL_1: { tier: RankTier.GLOBAL, level: 27, name: 'Global I', icon: 'global1' },
    GLOBAL_2: { tier: RankTier.GLOBAL, level: 28, name: 'Global II', icon: 'global2' },
    GLOBAL_3: { tier: RankTier.GLOBAL, level: 29, name: 'Global III', icon: 'global3' },
    GLOBAL_4: { tier: RankTier.GLOBAL, level: 30, name: 'Global Elite', icon: 'global4' }
};

export const LeaderboardType = {
    GLOBAL: 'global',
    REGIONAL: 'regional',
    FRIENDS: 'friends',
    TEAM: 'team',
    WEEKLY: 'weekly',
    MONTHLY: 'monthly',
    SEASONAL: 'seasonal',
    COMPETITIVE: 'competitive',
    CASUAL: 'casual',
    DEATHMATCH: 'deathmatch',
    ARMS_RACE: 'arms_race',
    DEMOLITION: 'demolition'
};

export const LeaderboardMetric = {
    RANK: 'rank',
    WINS: 'wins',
    KILLS: 'kills',
    KILL_RATIO: 'kill_ratio',
    HEADSHOT_PERCENTAGE: 'headshot_percentage',
    MVPS: 'mvps',
    SCORE: 'score',
    TIME_PLAYED: 'time_played',
    ACCURACY: 'accuracy',
    DAMAGE_PER_ROUND: 'damage_per_round',
    WIN_RATE: 'win_rate'
};

export const SeasonStatus = {
    UPCOMING: 'upcoming',
    ACTIVE: 'active',
    ENDING: 'ending',
    COMPLETED: 'completed'
};

export const MedalType = {
    BRONZE: 'bronze',
    SILVER: 'silver',
    GOLD: 'gold',
    PLATINUM: 'platinum'
};

export class RankInfo {
    constructor(rankKey, wins, winRate, skillLevel) {
        this.rankKey = rankKey;
        this.rankData = RankLevel[rankKey] || RankLevel.SILVER_1;
        this.wins = wins;
        this.winRate = winRate;
        this.skillLevel = skillLevel;
        this.matchCount = 0;
        this.rankProgress = 0;
        this.previousRank = null;
        this.rankChange = 0;
    }

    getLevel() {
        return this.rankData.level;
    }

    getName() {
        return this.rankData.name;
    }

    getTier() {
        return this.rankData.tier;
    }

    getIcon() {
        return this.rankData.icon;
    }

    getProgress() {
        return this.rankProgress;
    }

    setProgress(progress) {
        this.rankProgress = Math.max(0, Math.min(100, progress));
    }

    canRankUp() {
        return this.rankProgress >= 100 && this.rankKey !== 'GLOBAL_4';
    }

    canRankDown() {
        return this.rankProgress <= 0 && this.rankKey !== 'SILVER_1';
    }

    rankUp() {
        if (this.canRankUp()) {
            const levels = Object.keys(RankLevel);
            const currentIndex = levels.indexOf(this.rankKey);
            if (currentIndex < levels.length - 1) {
                this.previousRank = this.rankKey;
                this.rankKey = levels[currentIndex + 1];
                this.rankData = RankLevel[this.rankKey];
                this.rankChange = 1;
                return true;
            }
        }
        return false;
    }

    rankDown() {
        if (this.canRankDown()) {
            const levels = Object.keys(RankLevel);
            const currentIndex = levels.indexOf(this.rankKey);
            if (currentIndex > 0) {
                this.previousRank = this.rankKey;
                this.rankKey = levels[currentIndex - 1];
                this.rankData = RankLevel[this.rankKey];
                this.rankChange = -1;
                return true;
            }
        }
        return false;
    }

    update(wins, winRate, skillLevel, matchCount) {
        this.wins = wins;
        this.winRate = winRate;
        this.skillLevel = skillLevel;
        this.matchCount = matchCount;
        this.recalculateRank();
    }

    recalculateRank() {
        let newRankKey = 'SILVER_1';

        if (this.wins >= 1000 && this.skillLevel >= 3000) {
            newRankKey = 'GLOBAL_4';
        } else if (this.wins >= 800 && this.skillLevel >= 2500) {
            newRankKey = 'GLOBAL_3';
        } else if (this.wins >= 600 && this.skillLevel >= 2000) {
            newRankKey = 'GLOBAL_2';
        } else if (this.wins >= 400 && this.skillLevel >= 1500) {
            newRankKey = 'GLOBAL_1';
        } else if (this.wins >= 300 && this.skillLevel >= 1200) {
            newRankKey = 'ELITE_4';
        } else if (this.wins >= 250 && this.skillLevel >= 1000) {
            newRankKey = 'ELITE_3';
        } else if (this.wins >= 200 && this.skillLevel >= 850) {
            newRankKey = 'ELITE_2';
        } else if (this.wins >= 150 && this.skillLevel >= 700) {
            newRankKey = 'ELITE_1';
        } else if (this.wins >= 120 && this.skillLevel >= 600) {
            newRankKey = 'MASTER_4';
        } else if (this.wins >= 100 && this.skillLevel >= 500) {
            newRankKey = 'MASTER_3';
        } else if (this.wins >= 80 && this.skillLevel >= 400) {
            newRankKey = 'MASTER_2';
        } else if (this.wins >= 60 && this.skillLevel >= 300) {
            newRankKey = 'MASTER_1';
        } else if (this.wins >= 50 && this.skillLevel >= 250) {
            newRankKey = 'DIAMOND_4';
        } else if (this.wins >= 40 && this.skillLevel >= 200) {
            newRankKey = 'DIAMOND_3';
        } else if (this.wins >= 30 && this.skillLevel >= 150) {
            newRankKey = 'DIAMOND_2';
        } else if (this.wins >= 20 && this.skillLevel >= 100) {
            newRankKey = 'DIAMOND_1';
        } else if (this.wins >= 15 && this.skillLevel >= 80) {
            newRankKey = 'PLATINUM_4';
        } else if (this.wins >= 12 && this.skillLevel >= 60) {
            newRankKey = 'PLATINUM_3';
        } else if (this.wins >= 10 && this.skillLevel >= 45) {
            newRankKey = 'PLATINUM_2';
        } else if (this.wins >= 8 && this.skillLevel >= 30) {
            newRankKey = 'PLATINUM_1';
        } else if (this.wins >= 6 && this.skillLevel >= 20) {
            newRankKey = 'GOLD_4';
        } else if (this.wins >= 5 && this.skillLevel >= 15) {
            newRankKey = 'GOLD_3';
        } else if (this.wins >= 4 && this.skillLevel >= 10) {
            newRankKey = 'GOLD_2';
        } else if (this.wins >= 3 && this.skillLevel >= 5) {
            newRankKey = 'GOLD_1';
        } else if (this.wins >= 2) {
            newRankKey = 'SILVER_6';
        } else if (this.wins >= 1) {
            newRankKey = 'SILVER_5';
        }

        if (newRankKey !== this.rankKey) {
            this.previousRank = this.rankKey;
            this.rankKey = newRankKey;
            this.rankData = RankLevel[this.rankKey];
        }
    }

    export() {
        return {
            rank_key: this.rankKey,
            wins: this.wins,
            win_rate: this.winRate,
            skill_level: this.skillLevel,
            match_count: this.matchCount,
            rank_progress: this.rankProgress,
            previous_rank: this.previousRank,
            rank_change: this.rankChange
        };
    }

    static import(data) {
        const rank = new RankInfo(data.rank_key, data.wins, data.win_rate, data.skill_level);
        rank.matchCount = data.match_count || 0;
        rank.rankProgress = data.rank_progress || 0;
        rank.previousRank = data.previous_rank || null;
        rank.rankChange = data.rank_change || 0;
        return rank;
    }
}

export class PlayerRank {
    constructor(playerId, seasonId) {
        this.playerId = playerId;
        this.seasonId = seasonId;
        this.rankInfo = new RankInfo('SILVER_1', 0, 0, 0);
        this.wins = 0;
        this.losses = 0;
        this.draws = 0;
        this.rankPoints = 0;
        this.currentWinStreak = 0;
        this.bestWinStreak = 0;
        this.currentLoseStreak = 0;
        this.bestLoseStreak = 0;
        this.matchesPlayed = 0;
        this.lastRankUpdate = new Date();
    }

    getWinRate() {
        const totalGames = this.wins + this.losses + this.draws;
        return totalGames > 0 ? (this.wins / totalGames) * 100 : 0;
    }

    addWin() {
        this.wins++;
        this.matchesPlayed++;
        this.currentWinStreak++;
        this.currentLoseStreak = 0;
        this.bestWinStreak = Math.max(this.bestWinStreak, this.currentWinStreak);
        this.updateRank();
    }

    addLoss() {
        this.losses++;
        this.matchesPlayed++;
        this.currentLoseStreak++;
        this.currentWinStreak = 0;
        this.bestLoseStreak = Math.max(this.bestLoseStreak, this.currentLoseStreak);
        this.updateRank();
    }

    addDraw() {
        this.draws++;
        this.matchesPlayed++;
        this.currentWinStreak = 0;
        this.currentLoseStreak = 0;
        this.updateRank();
    }

    updateRank() {
        this.rankInfo.update(this.wins, this.getWinRate(), this.rankPoints, this.matchesPlayed);
        this.lastRankUpdate = new Date();
    }

    export() {
        return {
            player_id: this.playerId,
            season_id: this.seasonId,
            rank: this.rankInfo.export(),
            wins: this.wins,
            losses: this.losses,
            draws: this.draws,
            rank_points: this.rankPoints,
            current_win_streak: this.currentWinStreak,
            best_win_streak: this.bestWinStreak,
            current_lose_streak: this.currentLoseStreak,
            best_lose_streak: this.bestLoseStreak,
            matches_played: this.matchesPlayed,
            last_rank_update: this.lastRankUpdate.toISOString()
        };
    }

    static import(data) {
        const rank = new PlayerRank(data.player_id, data.season_id);
        rank.rankInfo = RankInfo.import(data.rank);
        rank.wins = data.wins || 0;
        rank.losses = data.losses || 0;
        rank.draws = data.draws || 0;
        rank.rankPoints = data.rank_points || 0;
        rank.currentWinStreak = data.current_win_streak || 0;
        rank.bestWinStreak = data.best_win_streak || 0;
        rank.currentLoseStreak = data.current_lose_streak || 0;
        rank.bestLoseStreak = data.best_lose_streak || 0;
        rank.matchesPlayed = data.matches_played || 0;
        rank.lastRankUpdate = data.last_rank_update ? new Date(data.last_rank_update) : new Date();
        return rank;
    }
}

export class LeaderboardEntry {
    constructor(rank, playerId, playerName, value, metadata = {}) {
        this.rank = rank;
        this.playerId = playerId;
        this.playerName = playerName;
        this.value = value;
        this.metadata = metadata;
        this.previousRank = null;
        this.rankChange = 0;
        this.avatar = '';
        this.country = '';
        this.clanTag = '';
        this.player = null;
    }

    getRankChange() {
        if (this.previousRank === null) return 0;
        return this.previousRank - this.rank;
    }

    isRankUp() {
        return this.getRankChange() > 0;
    }

    isRankDown() {
        return this.getRankChange() < 0;
    }

    export() {
        return {
            rank: this.rank,
            player_id: this.playerId,
            player_name: this.playerName,
            value: this.value,
            metadata: this.metadata,
            previous_rank: this.previousRank,
            rank_change: this.getRankChange(),
            avatar: this.avatar,
            country: this.country,
            clan_tag: this.clanTag
        };
    }

    static import(data) {
        const entry = new LeaderboardEntry(
            data.rank,
            data.player_id,
            data.player_name,
            data.value,
            data.metadata || {}
        );
        entry.previousRank = data.previous_rank || null;
        entry.avatar = data.avatar || '';
        entry.country = data.country || '';
        entry.clanTag = data.clan_tag || '';
        return entry;
    }
}

export class Leaderboard {
    constructor(type, metric, seasonId = null) {
        this.type = type;
        this.metric = metric;
        this.seasonId = seasonId;
        this.entries = [];
        this.totalEntries = 0;
        this.lastUpdated = new Date();
        this.filters = {};
        this.cacheExpiry = 0;
        this.isCached = false;
    }

    setEntries(entries) {
        this.entries = entries;
        this.totalEntries = entries.length;
        this.lastUpdated = new Date();
    }

    getTop(n = 100) {
        return this.entries.slice(0, n);
    }

    getPlayerEntry(playerId) {
        return this.entries.find(e => e.playerId === playerId);
    }

    getPlayerRank(playerId) {
        const entry = this.getPlayerEntry(playerId);
        return entry ? entry.rank : null;
    }

    getEntriesInRange(start, end) {
        return this.entries.slice(start, end);
    }

    addEntry(entry) {
        const existingIndex = this.entries.findIndex(e => e.playerId === entry.playerId);
        if (existingIndex >= 0) {
            this.entries[existingIndex] = entry;
        } else {
            this.entries.push(entry);
            this.recalculateRanks();
        }
        this.totalEntries = this.entries.length;
        this.lastUpdated = new Date();
    }

    removeEntry(playerId) {
        const index = this.entries.findIndex(e => e.playerId === playerId);
        if (index >= 0) {
            this.entries.splice(index, 1);
            this.recalculateRanks();
            this.totalEntries = this.entries.length;
        }
    }

    recalculateRanks() {
        this.entries.sort((a, b) => {
            if (this.metric === LeaderboardMetric.RANK || this.metric === LeaderboardMetric.KILL_RATIO) {
                return b.value - a.value;
            }
            return b.value - a.value;
        });

        this.entries.forEach((entry, index) => {
            entry.rank = index + 1;
        });
    }

    filterEntries(filters) {
        let filtered = [...this.entries];

        if (filters.region) {
            filtered = filtered.filter(e => e.country === filters.region);
        }

        if (filters.minValue !== undefined) {
            filtered = filtered.filter(e => e.value >= filters.minValue);
        }

        if (filters.maxValue !== undefined) {
            filtered = filtered.filter(e => e.value <= filters.maxValue);
        }

        if (filters.search) {
            const searchLower = filters.search.toLowerCase();
            filtered = filtered.filter(e =>
                e.playerName.toLowerCase().includes(searchLower)
            );
        }

        return filtered;
    }

    isExpired() {
        return Date.now() > this.cacheExpiry;
    }

    markExpired() {
        this.cacheExpiry = 0;
        this.isCached = false;
    }

    export() {
        return {
            type: this.type,
            metric: this.metric,
            season_id: this.seasonId,
            entries: this.entries.map(e => e.export()),
            total_entries: this.totalEntries,
            last_updated: this.lastUpdated.toISOString()
        };
    }

    static import(data) {
        const leaderboard = new Leaderboard(data.type, data.metric, data.season_id);
        leaderboard.entries = data.entries.map(e => LeaderboardEntry.import(e));
        leaderboard.totalEntries = data.total_entries || 0;
        leaderboard.lastUpdated = data.last_updated ? new Date(data.last_updated) : new Date();
        return leaderboard;
    }
}

export class Season {
    constructor(id, name, startDate, endDate) {
        this.id = id;
        this.name = name;
        this.startDate = startDate;
        this.endDate = endDate;
        this.status = SeasonStatus.UPCOMING;
        this.rewards = [];
        this.statistics = {
            totalMatches: 0,
            totalPlayers: 0,
            averageSkillLevel: 0
        };
    }

    isActive() {
        const now = new Date();
        return now >= this.startDate && now <= this.endDate;
    }

    isUpcoming() {
        return new Date() < this.startDate;
    }

    isCompleted() {
        return new Date() > this.endDate;
    }

    getDaysRemaining() {
        const now = new Date();
        const end = new Date(this.endDate);
        const diff = end - now;
        return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
    }

    getDaysUntilStart() {
        const now = new Date();
        const start = new Date(this.startDate);
        const diff = start - now;
        return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
    }

    addReward(reward) {
        this.rewards.push(reward);
    }

    updateStatus() {
        const now = new Date();
        if (now < this.startDate) {
            this.status = SeasonStatus.UPCOMING;
        } else if (now > this.endDate) {
            this.status = SeasonStatus.COMPLETED;
        } else {
            const daysRemaining = this.getDaysRemaining();
            if (daysRemaining <= 7) {
                this.status = SeasonStatus.ENDING;
            } else {
                this.status = SeasonStatus.ACTIVE;
            }
        }
    }
}

export class Medal {
    constructor(id, name, type, requirement, icon, description) {
        this.id = id;
        this.name = name;
        this.type = type;
        this.requirement = requirement;
        this.icon = icon;
        this.description = description;
        this.progress = 0;
        this.unlocked = false;
        this.unlockedDate = null;
    }

    updateProgress(value) {
        this.progress = Math.min(value, this.requirement);
        if (this.progress >= this.requirement && !this.unlocked) {
            this.unlocked = true;
            this.unlockedDate = new Date();
        }
    }

    getProgressPercentage() {
        return (this.progress / this.requirement) * 100;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            type: this.type,
            requirement: this.requirement,
            icon: this.icon,
            description: this.description,
            progress: this.progress,
            unlocked: this.unlocked,
            unlocked_date: this.unlockedDate ? this.unlockedDate.toISOString() : null
        };
    }
}

export class PlayerMedals {
    constructor(playerId) {
        this.playerId = playerId;
        this.medals = [];
        this.totalMedals = 0;
        this.goldMedals = 0;
        this.silverMedals = 0;
        this.bronzeMedals = 0;
        this.platinumMedals = 0;
    }

    addMedal(medal) {
        const existing = this.medals.find(m => m.id === medal.id);
        if (existing) {
            Object.assign(existing, medal);
        } else {
            this.medals.push(medal);
        }
        this.recalculateCounts();
    }

    recalculateCounts() {
        this.totalMedals = this.medals.filter(m => m.unlocked).length;
        this.goldMedals = this.medals.filter(m => m.unlocked && m.type === MedalType.GOLD).length;
        this.silverMedals = this.medals.filter(m => m.unlocked && m.type === MedalType.SILVER).length;
        this.bronzeMedals = this.medals.filter(m => m.unlocked && m.type === MedalType.BRONZE).length;
        this.platinumMedals = this.medals.filter(m => m.unlocked && m.type === MedalType.PLATINUM).length;
    }

    getMedal(id) {
        return this.medals.find(m => m.id === id);
    }

    export() {
        return {
            player_id: this.playerId,
            medals: this.medals.map(m => m.export()),
            total_medals: this.totalMedals,
            gold_medals: this.goldMedals,
            silver_medals: this.silverMedals,
            bronze_medals: this.bronzeMedals,
            platinum_medals: this.platinumMedals
        };
    }
}

export class PlayerProfile {
    constructor(playerId, playerName) {
        this.playerId = playerId;
        this.playerName = playerName;
        this.rank = new PlayerRank(playerId, null);
        this.medals = new PlayerMedals(playerId);
        this.statistics = {
            totalKills: 0,
            totalDeaths: 0,
            totalAssists: 0,
            totalHeadshots: 0,
            totalMvps: 0,
            totalScore: 0,
            totalTimePlayed: 0,
            totalDamage: 0,
            totalRoundsPlayed: 0
        };
        this.achievements = [];
        this.recentMatches = [];
        this.favoriteMaps = [];
        this.favoriteWeapons = [];
        this.country = '';
        this.avatar = '';
        this.clanTag = '';
        this.clanId = null;
    }

    getKDRatio() {
        return this.statistics.totalDeaths > 0
            ? (this.statistics.totalKills / this.statistics.totalDeaths).toFixed(2)
            : '0.00';
    }

    getHeadshotPercentage() {
        return this.statistics.totalKills > 0
            ? ((this.statistics.totalHeadshots / this.statistics.totalKills) * 100).toFixed(1)
            : '0.0';
    }

    getAverageScore() {
        const matchCount = this.recentMatches.length;
        return matchCount > 0
            ? Math.round(this.statistics.totalScore / matchCount)
            : 0;
    }

    getAverageDamage() {
        return this.statistics.totalRoundsPlayed > 0
            ? Math.round(this.statistics.totalDamage / this.statistics.totalRoundsPlayed)
            : 0;
    }

    updateFromMatch(matchData) {
        this.statistics.totalKills += matchData.kills;
        this.statistics.totalDeaths += matchData.deaths;
        this.statistics.totalAssists += matchData.assists;
        this.statistics.totalHeadshots += matchData.headshots;
        this.statistics.totalMvps += matchData.mvps;
        this.statistics.totalScore += matchData.score;
        this.statistics.totalTimePlayed += matchData.duration;
        this.statistics.totalDamage += matchData.damage;
        this.statistics.totalRoundsPlayed += matchData.rounds;

        this.recentMatches.unshift(matchData);
        if (this.recentMatches.length > 20) {
            this.recentMatches.pop();
        }
    }

    export() {
        return {
            player_id: this.playerId,
            player_name: this.playerName,
            rank: this.rank.export(),
            medals: this.medals.export(),
            statistics: this.statistics,
            achievements: this.achievements,
            recent_matches: this.recentMatches,
            favorite_maps: this.favoriteMaps,
            favorite_weapons: this.favoriteWeapons,
            country: this.country,
            avatar: this.avatar,
            clan_tag: this.clanTag,
            clan_id: this.clanId
        };
    }
}

export class LeaderboardManager {
    static instance = null;

    constructor() {
        this.leaderboards = new Map();
        this.seasons = [];
        this.currentSeason = null;
        this.playerProfiles = new Map();
        this.cache = new Map();
        this.cacheTimeout = 60000;
        this.refreshInterval = 300000;
        this.isRefreshing = false;
        this.listeners = new Map();
    }

    static getInstance() {
        if (!LeaderboardManager.instance) {
            LeaderboardManager.instance = new LeaderboardManager();
        }
        return LeaderboardManager.instance;
    }

    createLeaderboard(type, metric, seasonId = null) {
        const key = `${type}_${metric}_${seasonId || 'current'}`;
        if (!this.leaderboards.has(key)) {
            this.leaderboards.set(key, new Leaderboard(type, metric, seasonId));
        }
        return this.leaderboards.get(key);
    }

    getLeaderboard(type, metric, seasonId = null) {
        const key = `${type}_${metric}_${seasonId || 'current'}`;
        return this.leaderboards.get(key);
    }

    getAllLeaderboards() {
        return Array.from(this.leaderboards.values());
    }

    addSeason(season) {
        this.seasons.push(season);
        if (season.isActive()) {
            this.currentSeason = season;
        }
    }

    getCurrentSeason() {
        if (!this.currentSeason) {
            this.currentSeason = this.seasons.find(s => s.isActive());
        }
        return this.currentSeason;
    }

    getSeason(id) {
        return this.seasons.find(s => s.id === id);
    }

    getAllSeasons() {
        return [...this.seasons];
    }

    getPlayerProfile(playerId) {
        if (!this.playerProfiles.has(playerId)) {
            this.playerProfiles.set(playerId, new PlayerProfile(playerId, `Player${playerId}`));
        }
        return this.playerProfiles.get(playerId);
    }

    async refreshLeaderboard(type, metric, seasonId = null) {
        const leaderboard = this.getLeaderboard(type, metric, seasonId) ||
            this.createLeaderboard(type, metric, seasonId);

        const cacheKey = `${type}_${metric}_${seasonId || 'current'}`;
        const cached = this.cache.get(cacheKey);

        if (cached && !cached.isExpired()) {
            return cached.data;
        }

        this.isRefreshing = true;
        this.emit('refreshStarted', { type, metric });

        try {
            const entries = await this.fetchLeaderboardData(type, metric, seasonId);
            leaderboard.setEntries(entries);

            this.cache.set(cacheKey, {
                data: entries,
                timestamp: Date.now(),
                expiry: Date.now() + this.cacheTimeout
            });

            this.emit('refreshCompleted', { type, metric, count: entries.length });
            return entries;
        } catch (error) {
            this.emit('refreshFailed', { type, metric, error });
            throw error;
        } finally {
            this.isRefreshing = false;
        }
    }

    async fetchLeaderboardData(type, metric, seasonId) {
        return new Promise((resolve) => {
            setTimeout(() => {
                const entries = [];
                const names = ['ProPlayer', 'EliteSniper', 'CTMaster', 'TRookie', 'TopFragger', 'ClutchKing', 'AwpPro', 'RushB', 'CampMasta', 'NoobPlayer'];
                
                for (let i = 0; i < 100; i++) {
                    const entry = new LeaderboardEntry(
                        i + 1,
                        `player_${i}`,
                        names[i % names.length] + `_${i}`,
                        10000 - (i * 50) + Math.floor(Math.random() * 100)
                    );
                    entry.avatar = `avatar_${i % 10}`;
                    entry.country = ['US', 'EU', 'RU', 'BR', 'KR'][i % 5];
                    entries.push(entry);
                }
                
                resolve(entries);
            }, 500);
        });
    }

    getLeaderboardPosition(playerId, type, metric, seasonId = null) {
        const leaderboard = this.getLeaderboard(type, metric, seasonId);
        if (!leaderboard) return null;
        return leaderboard.getPlayerRank(playerId);
    }

    getNearbyPlayers(playerId, type, metric, count = 10, seasonId = null) {
        const leaderboard = this.getLeaderboard(type, metric, seasonId);
        if (!leaderboard) return [];

        const playerRank = leaderboard.getPlayerRank(playerId);
        if (!playerRank) return [];

        const start = Math.max(0, playerRank - count - 1);
        const end = playerRank + count;
        return leaderboard.getEntriesInRange(start, end);
    }

    clearCache() {
        this.cache.clear();
    }

    startAutoRefresh() {
        setInterval(() => {
            if (!this.isRefreshing) {
                for (const [key, leaderboard] of this.leaderboards) {
                    leaderboard.markExpired();
                }
            }
        }, this.refreshInterval);
    }

    exportData() {
        return {
            leaderboards: Array.from(this.leaderboards.entries()),
            seasons: this.seasons.map(s => ({
                id: s.id,
                name: s.name,
                start_date: s.startDate.toISOString(),
                end_date: s.endDate.toISOString(),
                status: s.status
            })),
            current_season: this.currentSeason ? this.currentSeason.id : null,
            cache_timestamp: Date.now()
        };
    }

    importData(data) {
        for (const [key, leaderboard] of data.leaderboards) {
            this.leaderboards.set(key, leaderboard);
        }

        this.seasons = data.seasons.map(s => {
            const season = new Season(s.id, s.name, new Date(s.start_date), new Date(s.end_date));
            season.status = s.status;
            return season;
        });

        if (data.current_season) {
            this.currentSeason = this.getSeason(data.current_season);
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
                console.error(`Error in leaderboard listener: ${e}`);
            }
        }
    }
}

export const RankSystem = LeaderboardManager.getInstance();

export class AchievementSystem {
    constructor() {
        this.achievements = new Map();
        this.playerAchievements = new Map();
        this.categories = new Map();
        this.badges = new Map();
        this.challenges = new Map();
        this.seasonalEvents = new Map();
        this.progress = new Map();
        this.rewards = new Map();
    }

    initialize() {
        this.setupCategories();
        this.setupDefaultAchievements();
    }

    setupCategories() {
        this.categories.set('combat', {
            id: 'combat',
            name: 'Combat',
            description: 'Achievements related to combat performance',
            icon: '⚔️',
            achievements: []
        });

        this.categories.set('objective', {
            id: 'objective',
            name: 'Objective',
            description: 'Achievements related to completing objectives',
            icon: '🎯',
            achievements: []
        });

        this.categories.set('teamwork', {
            id: 'teamwork',
            name: 'Teamwork',
            description: 'Achievements related to teamwork',
            icon: '🤝',
            achievements: []
        });

        this.categories.set('exploration', {
            id: 'exploration',
            name: 'Exploration',
            description: 'Achievements related to exploring the game',
            icon: '🗺️',
            achievements: []
        });

        this.categories.set('mastery', {
            id: 'mastery',
            name: 'Mastery',
            description: 'Achievements related to mastering weapons',
            icon: '🎓',
            achievements: []
        });

        this.categories.set('social', {
            id: 'social',
            name: 'Social',
            description: 'Achievements related to social interactions',
            icon: '💬',
            achievements: []
        });

        this.categories.set('rare', {
            id: 'rare',
            name: 'Rare',
            description: 'Rare achievements',
            icon: '💎',
            achievements: []
        });
    }

    setupDefaultAchievements() {
        const achievements = [
            {
                id: 'first_blood',
                name: 'First Blood',
                description: 'Get the first kill of the match',
                category: 'combat',
                rarity: 'common',
                icon: 'first_blood',
                points: 10,
                unlockCondition: { type: 'first_kill', value: 1 }
            },
            {
                id: 'rampage',
                name: 'Rampage',
                description: 'Kill 4 enemies in one round',
                category: 'combat',
                rarity: 'uncommon',
                icon: 'rampage',
                points: 25,
                unlockCondition: { type: 'kills_in_round', value: 4 }
            },
            {
                id: 'unstoppable',
                name: 'Unstoppable',
                description: 'Kill 5 enemies in one round',
                category: 'combat',
                rarity: 'rare',
                icon: 'unstoppable',
                points: 50,
                unlockCondition: { type: 'kills_in_round', value: 5 }
            },
            {
                id: 'headhunter',
                name: 'Headhunter',
                description: 'Get 100 headshot kills',
                category: 'combat',
                rarity: 'uncommon',
                icon: 'headhunter',
                points: 30,
                unlockCondition: { type: 'total_headshots', value: 100 }
            },
            {
                id: 'sniper_elite',
                name: 'Sniper Elite',
                description: 'Kill an enemy from across the map',
                category: 'combat',
                rarity: 'common',
                icon: 'sniper_elite',
                points: 15,
                unlockCondition: { type: 'long_range_kill', value: 1 }
            },
            {
                id: 'clutch_master',
                name: 'Clutch Master',
                description: 'Win a 1v2 clutch',
                category: 'combat',
                rarity: 'rare',
                icon: 'clutch_master',
                points: 45,
                unlockCondition: { type: 'clutch_win', value: 1, situation: '1v2' }
            },
            {
                id: 'bomb_planted',
                name: 'Demolition Expert',
                description: 'Plant the bomb on the bomb site',
                category: 'objective',
                rarity: 'common',
                icon: 'bomb_planted',
                points: 10,
                unlockCondition: { type: 'bomb_plant', value: 1 }
            },
            {
                id: 'bomb_defused',
                name: 'Bomb Squad',
                description: 'Defuse the bomb with less than 5 seconds',
                category: 'objective',
                rarity: 'uncommon',
                icon: 'bomb_defused',
                points: 25,
                unlockCondition: { type: 'bomb_defuse', value: 1, timeLimit: 5 }
            },
            {
                id: 'hostage_rescuer',
                name: 'Hero',
                description: 'Rescue all hostages in a round',
                category: 'objective',
                rarity: 'common',
                icon: 'hostage_rescuer',
                points: 15,
                unlockCondition: { type: 'hostage_rescue', value: 1 }
            },
            {
                id: 'entry_frag',
                name: 'Point Man',
                description: 'Get the first kill in a round',
                category: 'teamwork',
                rarity: 'common',
                icon: 'entry_frag',
                points: 10,
                unlockCondition: { type: 'entry_kill', value: 1 }
            },
            {
                id: 'trade_kill',
                name: 'Payback',
                description: 'Kill the enemy who just killed your teammate',
                category: 'teamwork',
                rarity: 'common',
                icon: 'trade_kill',
                points: 10,
                unlockCondition: { type: 'trade_kill', value: 1 }
            },
            {
                id: 'assistant',
                name: 'Assistant',
                description: 'Get 50 assists',
                category: 'teamwork',
                rarity: 'uncommon',
                icon: 'assistant',
                points: 20,
                unlockCondition: { type: 'assists', value: 50 }
            },
            {
                id: 'map_master_de_dust2',
                name: 'Dust II Expert',
                description: 'Play 100 rounds on Dust II',
                category: 'exploration',
                rarity: 'common',
                icon: 'map_master',
                points: 25,
                unlockCondition: { type: 'map_playtime', map: 'de_dust2', value: 100 }
            },
            {
                id: 'world_traveler',
                name: 'World Traveler',
                description: 'Play on every competitive map',
                category: 'exploration',
                rarity: 'rare',
                icon: 'world_traveler',
                points: 50,
                unlockCondition: { type: 'unique_maps', value: 10 }
            },
            {
                id: 'ak47_master',
                name: 'AK-47 Master',
                description: 'Get 1000 kills with the AK-47',
                category: 'mastery',
                rarity: 'epic',
                icon: 'ak47_master',
                points: 100,
                unlockCondition: { type: 'weapon_kills', weapon: 'ak47', value: 1000 }
            },
            {
                id: 'awper',
                name: 'AWP Master',
                description: 'Get 500 kills with the AWP',
                category: 'mastery',
                rarity: 'epic',
                icon: 'awper',
                points: 100,
                unlockCondition: { type: 'weapon_kills', weapon: 'awp', value: 500 }
            },
            {
                id: 'knife_fighter',
                name: 'Knife Fighter',
                description: 'Get 10 knife kills',
                category: 'mastery',
                rarity: 'rare',
                icon: 'knife_fighter',
                points: 40,
                unlockCondition: { type: 'weapon_kills', weapon: 'knife', value: 10 }
            },
            {
                id: 'no_scope',
                name: 'No Scope',
                description: 'Kill an enemy with the AWP without zooming',
                category: 'combat',
                rarity: 'rare',
                icon: 'no_scope',
                points: 35,
                unlockCondition: { type: 'no_scope_kill', value: 1 }
            },
            {
                id: 'flawless_round',
                name: 'Flawless Victory',
                description: 'Win a round without taking any damage',
                category: 'combat',
                rarity: 'uncommon',
                icon: 'flawless_round',
                points: 30,
                unlockCondition: { type: 'flawless_round', value: 1 }
            },
            {
                id: 'speed_demon',
                name: 'Speed Demon',
                description: 'Get a kill within 10 seconds of the round starting',
                category: 'combat',
                rarity: 'common',
                icon: 'speed_demon',
                points: 15,
                unlockCondition: { type: 'fast_kill', value: 1, maxTime: 10 }
            },
            {
                id: 'grenadier',
                name: 'Grenadier',
                description: 'Kill 3 enemies with a single grenade',
                category: 'combat',
                rarity: 'rare',
                icon: 'grenadier',
                points: 40,
                unlockCondition: { type: 'nade_kill_multikill', value: 3 }
            },
            {
                id: 'first_match',
                name: 'Welcome to the Battlefield',
                description: 'Complete your first match',
                category: 'social',
                rarity: 'common',
                icon: 'first_match',
                points: 5,
                unlockCondition: { type: 'matches_played', value: 1 }
            },
            {
                id: 'veteran',
                name: 'Veteran',
                description: 'Play 100 matches',
                category: 'social',
                rarity: 'uncommon',
                icon: 'veteran',
                points: 50,
                unlockCondition: { type: 'matches_played', value: 100 }
            },
            {
                id: 'champion',
                name: 'Champion',
                description: 'Win a championship match',
                category: 'objective',
                rarity: 'epic',
                icon: 'champion',
                points: 150,
                unlockCondition: { type: 'championship_win', value: 1 }
            },
            {
                id: 'competitive_king',
                name: 'Competitive King',
                description: 'Win 50 competitive matches',
                category: 'objective',
                rarity: 'rare',
                icon: 'competitive_king',
                points: 75,
                unlockCondition: { type: 'competitive_wins', value: 50 }
            }
        ];

        for (const achievement of achievements) {
            this.createAchievement(achievement);
        }
    }

    createAchievement(data) {
        const achievement = {
            id: data.id,
            name: data.name,
            description: data.description,
            category: data.category,
            rarity: data.rarity || 'common',
            icon: data.icon,
            points: data.points || 10,
            unlockCondition: data.unlockCondition,
            secret: data.secret || false,
            tier: data.tier || 1,
            requirements: data.requirements || [],
            rewards: data.rewards || [],
            prerequisites: data.prerequisites || [],
            createdAt: Date.now(),
            unlockCount: 0,
            globalPercentage: 0
        };

        this.achievements.set(achievement.id, achievement);

        const category = this.categories.get(achievement.category);
        if (category) {
            category.achievements.push(achievement.id);
        }

        return achievement;
    }

    getAchievement(achievementId) {
        return this.achievements.get(achievementId);
    }

    getAchievementsByCategory(categoryId) {
        const category = this.categories.get(categoryId);
        if (!category) return [];

        return category.achievements
            .map(id => this.achievements.get(id))
            .filter(Boolean);
    }

    getAllAchievements() {
        return Array.from(this.achievements.values());
    }

    unlockAchievement(playerId, achievementId) {
        const achievement = this.achievements.get(achievementId);
        if (!achievement) return { success: false, reason: 'Achievement not found' };

        const playerData = this.getOrCreatePlayerData(playerId);

        if (playerData.unlocked.has(achievementId)) {
            return { success: false, reason: 'Already unlocked' };
        }

        if (!this.checkPrerequisites(playerId, achievement)) {
            return { success: false, reason: 'Prerequisites not met' };
        }

        const unlocked = {
            achievementId,
            unlockedAt: Date.now(),
            pointsEarned: achievement.points,
            rewards: achievement.rewards
        };

        playerData.unlocked.set(achievementId, unlocked);
        playerData.totalPoints += achievement.points;

        achievement.unlockCount++;
        achievement.globalPercentage = (achievement.unlockCount / this.getTotalPlayers()) * 100;

        return {
            success: true,
            achievement,
            unlocked,
            totalPoints: playerData.totalPoints
        };
    }

    checkPrerequisites(playerId, achievement) {
        if (!achievement.prerequisites || achievement.prerequisites.length === 0) {
            return true;
        }

        const playerData = this.getOrCreatePlayerData(playerId);

        for (const prereqId of achievement.prerequisites) {
            if (!playerData.unlocked.has(prereqId)) {
                return false;
            }
        }

        return true;
    }

    getPlayerAchievements(playerId) {
        const playerData = this.getOrCreatePlayerData(playerId);
        return Array.from(playerData.unlocked.values());
    }

    getPlayerProgress(playerId) {
        const playerData = this.getOrCreatePlayerData(playerId);
        const totalAchievements = this.achievements.size;
        const unlocked = playerData.unlocked.size;
        const percentage = (unlocked / totalAchievements) * 100;

        return {
            playerId,
            totalAchievements,
            unlocked,
            locked: totalAchievements - unlocked,
            percentage,
            totalPoints: playerData.totalPoints,
            maxPoints: Array.from(this.achievements.values()).reduce((sum, a) => sum + a.points, 0)
        };
    }

    checkProgress(playerId, event) {
        const playerData = this.getOrCreatePlayerData(playerId);
        const newlyUnlocked = [];

        for (const [id, achievement] of this.achievements) {
            if (playerData.unlocked.has(id)) continue;

            if (!this.checkPrerequisites(playerId, achievement)) continue;

            const progress = this.getProgress(playerId, achievement);

            if (this.checkCondition(progress, achievement.unlockCondition)) {
                const result = this.unlockAchievement(playerId, id);
                if (result.success) {
                    newlyUnlocked.push(result.achievement);
                }
            }

            playerData.progress.set(id, progress);
        }

        return newlyUnlocked;
    }

    getProgress(playerId, achievement) {
        const condition = achievement.unlockCondition;
        const playerData = this.getOrCreatePlayerData(playerId);

        const progress = {
            current: 0,
            target: condition.value || 1,
            percentage: 0
        };

        switch (condition.type) {
            case 'total_headshots':
                progress.current = playerData.stats.headshots || 0;
                break;
            case 'kills_in_round':
                progress.current = playerData.maxKillsInRound || 0;
                break;
            case 'bomb_plant':
                progress.current = playerData.stats.plants || 0;
                break;
            case 'bomb_defuse':
                progress.current = playerData.stats.defusals || 0;
                break;
            case 'matches_played':
                progress.current = playerData.stats.matchesPlayed || 0;
                break;
            case 'competitive_wins':
                progress.current = playerData.stats.competitiveWins || 0;
                break;
            case 'weapon_kills':
                progress.current = playerData.weaponKills[condition.weapon] || 0;
                break;
            default:
                progress.current = 0;
        }

        progress.percentage = Math.min(100, (progress.current / progress.target) * 100);

        return progress;
    }

    checkCondition(progress, condition) {
        if (condition.type === 'clutch_win' || condition.type === 'bomb_defuse') {
            return progress.current >= 1;
        }
        return progress.current >= condition.value;
    }

    getOrCreatePlayerData(playerId) {
        if (!this.playerAchievements.has(playerId)) {
            this.playerAchievements.set(playerId, {
                playerId,
                unlocked: new Map(),
                progress: new Map(),
                totalPoints: 0,
                stats: {
                    matchesPlayed: 0,
                    totalKills: 0,
                    headshots: 0,
                    plants: 0,
                    defusals: 0,
                    assists: 0,
                    competitiveWins: 0
                },
                weaponKills: {},
                mapPlaytime: {},
                maxKillsInRound: 0,
                achievementsByCategory: new Map(),
                recentUnlocks: [],
                firstUnlockTime: null,
                lastUnlockTime: null,
                createdAt: Date.now(),
                updatedAt: Date.now()
            });
        }

        return this.playerAchievements.get(playerId);
    }

    getTotalPlayers() {
        return this.playerAchievements.size;
    }

    createBadge(data) {
        const badge = {
            id: data.id || 'badge_' + Date.now(),
            name: data.name,
            description: data.description,
            icon: data.icon,
            rarity: data.rarity || 'common',
            achievements: data.achievements || [],
            requirement: data.requirement,
            exclusive: data.exclusive || false,
            limited: data.limited || false,
            limitedCount: data.limitedCount || 0,
            obtainedCount: 0,
            createdAt: Date.now()
        };

        this.badges.set(badge.id, badge);
        return badge;
    }

    getBadgesForPlayer(playerId) {
        const playerData = this.getOrCreatePlayerData(playerId);
        const badges = [];

        for (const [id, badge] of this.badges) {
            const unlockedAchievements = badge.achievements.filter(
                aId => playerData.unlocked.has(aId)
            );

            badges.push({
                ...badge,
                progress: unlockedAchievements.length,
                total: badge.achievements.length,
                unlocked: unlockedAchievements.length === badge.achievements.length
            });
        }

        return badges;
    }

    createChallenge(challengeData) {
        const challenge = {
            id: challengeData.id || 'challenge_' + Date.now(),
            name: challengeData.name,
            description: challengeData.description,
            category: challengeData.category,
            difficulty: challengeData.difficulty || 'medium',
            requirement: challengeData.requirement,
            reward: challengeData.reward || {},
            timeLimit: challengeData.timeLimit || null,
            startDate: challengeData.startDate || Date.now(),
            endDate: challengeData.endDate || null,
            repeatable: challengeData.repeatable || false,
            maxCompletions: challengeData.maxCompletions || 1,
            completedCount: 0,
            participantCount: 0,
            successRate: 0,
            createdAt: Date.now()
        };

        this.challenges.set(challenge.id, challenge);
        return challenge;
    }

    getActiveChallenges() {
        const now = Date.now();
        return Array.from(this.challenges.values())
            .filter(c => !c.endDate || c.endDate > now);
    }

    getChallengeProgress(playerId, challengeId) {
        const challenge = this.challenges.get(challengeId);
        if (!challenge) return null;

        const playerData = this.getOrCreatePlayerData(playerId);

        return {
            challengeId,
            current: 0,
            target: challenge.requirement.value,
            completed: false,
            timeRemaining: challenge.endDate ? challenge.endDate - Date.now() : null
        };
    }

    completeChallenge(playerId, challengeId) {
        const challenge = this.challenges.get(challengeId);
        if (!challenge) return { success: false, reason: 'Challenge not found' };

        const playerData = this.getOrCreatePlayerData(playerId);

        if (!playerData.challenges) {
            playerData.challenges = new Map();
        }

        const playerChallenge = playerData.challenges.get(challengeId) || {
            completions: 0,
            startedAt: Date.now(),
            lastCompleted: null
        };

        if (playerChallenge.completions >= challenge.maxCompletions && !challenge.repeatable) {
            return { success: false, reason: 'Already completed' };
        }

        playerChallenge.completions++;
        playerChallenge.lastCompleted = Date.now();
        playerData.challenges.set(challengeId, playerChallenge);

        challenge.completedCount++;

        return {
            success: true,
            reward: challenge.reward,
            completions: playerChallenge.completions
        };
    }

    getSeasonalEvents() {
        const now = Date.now();
        return Array.from(this.seasonalEvents.values())
            .filter(e => e.startDate <= now && e.endDate >= now);
    }

    createSeasonalEvent(data) {
        const event = {
            id: data.id || 'event_' + Date.now(),
            name: data.name,
            description: data.description,
            type: data.type,
            startDate: data.startDate,
            endDate: data.endDate,
            rewards: data.rewards || [],
            challenges: data.challenges || [],
            achievements: data.achievements || [],
            bonuses: data.bonuses || [],
            createdAt: Date.now()
        };

        this.seasonalEvents.set(event.id, event);
        return event;
    }

    serialize() {
        return {
            achievements: Array.from(this.achievements.entries()),
            categories: Array.from(this.categories.entries()),
            badges: Array.from(this.badges.entries()),
            challenges: Array.from(this.challenges.entries()),
            seasonalEvents: Array.from(this.seasonalEvents.entries())
        };
    }
}

export default {
    AchievementSystem
};

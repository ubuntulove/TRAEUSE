export class PlayerProgression {
    constructor(config = {}) {
        this.playerId = config.playerId || null;
        this.levels = [];
        this.currentLevel = 1;
        this.currentXP = 0;
        this.totalXP = 0;
        this.lifetimeXP = 0;
        this.rank = PlayerRank.SILVER_I;
        this.xpToNextLevel = this.calculateXPForLevel(this.currentLevel + 1);
        this.bonusXP = 0;
        this.xpMultiplier = 1;
        this.weeklyXP = 0;
        this.weeklyXPReset = this.getNextWeeklyReset();
        this.seasonXP = 0;
        this.achievements = [];
        this.unlockedItems = new Set();
        this.badges = [];
        this.title = null;
        this.avatars = [];
        this.activeAvatar = null;
        this.backgrounds = [];
        this.activeBackground = null;
        this.crosshairCodes = [];
        this.activeCrosshairCode = null;
        this.statRanks = new Map();
        this.initialize();
    }

    initialize() {
        this.levels = this.generateLevelTable();
        this.initializeStatRanks();
        this.initializeDefaultUnlocks();
    }

    generateLevelTable() {
        const levels = [];
        let xpRequired = 0;
        let baseXP = 100;

        for (let level = 1; level <= 1000; level++) {
            levels.push({
                level,
                xpRequired,
                totalXPRequired: xpRequired,
                title: this.getTitleForLevel(level),
                rewards: this.getRewardsForLevel(level)
            });

            if (level < 10) {
                baseXP = 100;
            } else if (level < 20) {
                baseXP = 150;
            } else if (level < 50) {
                baseXP = 200;
            } else if (level < 100) {
                baseXP = 300;
            } else if (level < 250) {
                baseXP = 500;
            } else if (level < 500) {
                baseXP = 750;
            } else {
                baseXP = 1000;
            }

            const growthFactor = 1.0 + (level - 1) * 0.05;
            xpRequired += Math.floor(baseXP * growthFactor);
        }

        return levels;
    }

    getTitleForLevel(level) {
        if (level < 2) return 'Newcomer';
        if (level < 5) return 'Recruit';
        if (level < 10) return 'Private';
        if (level < 15) return 'Corporal';
        if (level < 20) return 'Sergeant';
        if (level < 30) return 'Lieutenant';
        if (level < 40) return 'Captain';
        if (level < 50) return 'Major';
        if (level < 60) return 'Colonel';
        if (level < 75) return 'Brigadier';
        if (level < 90) return 'General';
        if (level < 100) return 'Commander';
        if (level < 150) return 'Elite';
        if (level < 200) return 'Veteran';
        if (level < 300) return 'Expert';
        if (level < 400) return 'Master';
        if (level < 500) return 'Grandmaster';
        if (level < 750) return 'Legendary';
        return 'Supreme';
    }

    getRewardsForLevel(level) {
        const rewards = [];

        if (level === 2) {
            rewards.push({ type: 'item', id: 'spray_default' });
            rewards.push({ type: 'xp_boost', amount: 100, duration: 24 });
        }

        if (level === 5) {
            rewards.push({ type: 'item', id: 'spray_team' });
            rewards.push({ type: 'xp_boost', amount: 200, duration: 48 });
        }

        if (level === 10) {
            rewards.push({ type: 'item', id: 'weapon_skin_1' });
            rewards.push({ type: 'xp_boost', amount: 500, duration: 72 });
        }

        if (level % 10 === 0 && level > 0) {
            rewards.push({ type: 'xp_boost', amount: 100 * (level / 10), duration: 24 * (level / 10) });
        }

        if (level % 25 === 0 && level > 0) {
            rewards.push({ type: 'item', id: `weapon_skin_${level / 25}` });
            rewards.push({ type: 'medal', id: `medal_${level}` });
        }

        return rewards;
    }

    initializeStatRanks() {
        const statNames = [
            'headshots', 'kills', 'deaths', 'assists', 'wins',
            'damage', 'mvps', 'clutches', 'aWPkills', 'knifeKills',
            'grenadeKills', 'dominations', 'rescues', 'hoursPlayed'
        ];

        for (const stat of statNames) {
            this.statRanks.set(stat, {
                value: 0,
                rank: this.calculateStatRank(0, stat),
                percentile: 0
            });
        }
    }

    calculateStatRank(value, statName) {
        if (value < 10) return 'Iron';
        if (value < 100) return 'Bronze';
        if (value < 500) return 'Silver';
        if (value < 1000) return 'Gold';
        if (value < 5000) return 'Platinum';
        if (value < 10000) return 'Diamond';
        if (value < 50000) return 'Master';
        return 'Grandmaster';
    }

    initializeDefaultUnlocks() {
        this.unlockedItems.add('spray_default');
        this.unlockedItems.add('crosshair_default');
        this.unlockedItems.add('avatar_default');
        this.unlockedItems.add('background_default');
        this.unlockedItems.add('melee_default');
    }

    addXP(amount, source = 'match') {
        const adjustedAmount = Math.floor(amount * this.xpMultiplier) + this.bonusXP;

        this.currentXP += adjustedAmount;
        this.totalXP += adjustedAmount;
        this.lifetimeXP += adjustedAmount;
        this.weeklyXP += adjustedAmount;
        this.seasonXP += adjustedAmount;

        const events = [];

        while (this.currentXP >= this.xpToNextLevel && this.currentLevel < 1000) {
            const levelUp = this.levelUp();
            events.push(levelUp);
        }

        return {
            amount: adjustedAmount,
            newTotalXP: this.totalXP,
            newLevel: this.currentLevel,
            currentXP: this.currentXP,
            xpToNextLevel: this.xpToNextLevel,
            events
        };
    }

    levelUp() {
        const oldLevel = this.currentLevel;
        this.currentLevel++;
        this.currentXP -= this.xpToNextLevel;
        this.xpToNextLevel = this.calculateXPForLevel(this.currentLevel + 1);

        const rewards = this.getRewardsForLevel(this.currentLevel);
        for (const reward of rewards) {
            this.claimReward(reward);
        }

        return {
            type: 'levelUp',
            oldLevel,
            newLevel: this.currentLevel,
            title: this.getTitleForLevel(this.currentLevel),
            rewards
        };
    }

    claimReward(reward) {
        switch (reward.type) {
            case 'item':
                this.unlockedItems.add(reward.id);
                break;
            case 'xp_boost':
                this.bonusXP += reward.amount;
                setTimeout(() => {
                    this.bonusXP -= reward.amount;
                }, reward.duration * 60 * 60 * 1000);
                break;
            case 'medal':
                this.badges.push({
                    id: reward.id,
                    unlockedAt: Date.now(),
                    level: this.currentLevel
                });
                break;
        }
    }

    calculateXPForLevel(level) {
        if (level <= 1) return 0;
        if (level > this.levels.length) {
            return this.levels[this.levels.length - 1].xpRequired;
        }
        return this.levels[level - 1].xpRequired - this.levels[level - 2].xpRequired;
    }

    getNextWeeklyReset() {
        const now = new Date();
        const nextWeek = new Date(now);
        nextWeek.setDate(now.getDate() + (7 - now.getDay()));
        nextWeek.setHours(0, 0, 0, 0);
        return nextWeek.getTime();
    }

    updateStat(statName, value) {
        if (!this.statRanks.has(statName)) {
            this.statRanks.set(statName, { value: 0, rank: 'Iron', percentile: 0 });
        }

        const stat = this.statRanks.get(statName);
        const oldValue = stat.value;
        stat.value = value;
        stat.rank = this.calculateStatRank(value, statName);
        stat.percentile = this.calculatePercentile(statName, value);

        return {
            stat: statName,
            oldValue,
            newValue: value,
            rank: stat.rank,
            percentile: stat.percentile,
            rankChanged: stat.rank !== this.calculateStatRank(oldValue, statName)
        };
    }

    calculatePercentile(statName, value) {
        const maxExpected = {
            'headshots': 10000,
            'kills': 100000,
            'damage': 10000000,
            'hoursPlayed': 10000
        };

        const max = maxExpected[statName] || 100000;
        return Math.min(99.9, (value / max) * 100);
    }

    setTitle(title) {
        this.title = title;
    }

    unlockItem(itemId) {
        this.unlockedItems.add(itemId);
    }

    isItemUnlocked(itemId) {
        return this.unlockedItems.has(itemId);
    }

    getUnlockedItemsByCategory(category) {
        const items = [];
        for (const itemId of this.unlockedItems) {
            if (itemId.startsWith(category)) {
                items.push(itemId);
            }
        }
        return items;
    }

    addBadge(badge) {
        this.badges.push({
            ...badge,
            unlockedAt: Date.now()
        });
    }

    getRecentBadges(count = 5) {
        return this.badges
            .sort((a, b) => b.unlockedAt - a.unlockedAt)
            .slice(0, count);
    }

    getXPProgress() {
        const progress = this.currentXP / this.xpToNextLevel;
        return {
            currentXP: this.currentXP,
            xpToNextLevel: this.xpToNextLevel,
            progress: Math.min(1, Math.max(0, progress)),
            percentage: Math.floor(progress * 100),
            currentLevel: this.currentLevel,
            title: this.getTitleForLevel(this.currentLevel),
            nextTitle: this.getTitleForLevel(this.currentLevel + 1)
        };
    }

    getWeeklyProgress() {
        const weekStart = new Date();
        weekStart.setDate(weekStart.getDate() - weekStart.getDay());
        weekStart.setHours(0, 0, 0, 0);

        const xpPerDay = 5000;
        const daysPassed = Math.floor((Date.now() - weekStart.getTime()) / (24 * 60 * 60 * 1000));
        const expectedXP = daysPassed * xpPerDay;
        const bonusXP = Math.min(0.2, this.weeklyXP / xpPerDay / 7);

        return {
            currentXP: this.weeklyXP,
            expectedXP,
            bonusPercent: bonusXP * 100,
            daysRemaining: 7 - daysPassed,
            resetTime: this.weeklyXPReset
        };
    }

    getSeasonProgress(seasonId) {
        return {
            seasonXP: this.seasonXP,
            seasonId,
            percentComplete: Math.min(100, (this.seasonXP / 50000) * 100)
        };
    }

    toJSON() {
        return {
            playerId: this.playerId,
            currentLevel: this.currentLevel,
            currentXP: this.currentXP,
            totalXP: this.totalXP,
            lifetimeXP: this.lifetimeXP,
            rank: this.rank,
            title: this.title,
            unlockedItems: Array.from(this.unlockedItems),
            badges: this.badges,
            statRanks: Array.from(this.statRanks.entries()),
            weeklyXP: this.weeklyXP,
            seasonXP: this.seasonXP,
            crosshairCodes: this.crosshairCodes,
            activeCrosshairCode: this.activeCrosshairCode,
            avatars: this.avatars,
            activeAvatar: this.activeAvatar,
            backgrounds: this.backgrounds,
            activeBackground: this.activeBackground
        };
    }

    fromJSON(json) {
        this.playerId = json.playerId;
        this.currentLevel = json.currentLevel;
        this.currentXP = json.currentXP;
        this.totalXP = json.totalXP;
        this.lifetimeXP = json.lifetimeXP;
        this.rank = json.rank;
        this.title = json.title;
        this.unlockedItems = new Set(json.unlockedItems || []);
        this.badges = json.badges || [];
        this.statRanks = new Map(json.statRanks || []);
        this.weeklyXP = json.weeklyXP || 0;
        this.seasonXP = json.seasonXP || 0;
        this.crosshairCodes = json.crosshairCodes || [];
        this.activeCrosshairCode = json.activeCrosshairCode;
        this.avatars = json.avatars || [];
        this.activeAvatar = json.activeAvatar;
        this.backgrounds = json.backgrounds || [];
        this.activeBackground = json.activeBackground;
        this.xpToNextLevel = this.calculateXPForLevel(this.currentLevel + 1);
    }
}

export class PlayerRank {
    static get SILVER_I() { return { name: 'Silver I', tier: 1, division: 1 }; }
    static get SILVER_II() { return { name: 'Silver II', tier: 1, division: 2 }; }
    static get SILVER_III() { return { name: 'Silver III', tier: 1, division: 3 }; }
    static get SILVER_IV() { return { name: 'Silver IV', tier: 1, division: 4 }; }
    static get SILVER_ELITE() { return { name: 'Silver Elite', tier: 1, division: 5 }; }
    static get SILVER_ELITE_MASTER() { return { name: 'Silver Elite Master', tier: 1, division: 6 }; }

    static get GOLD_NOVA_I() { return { name: 'Gold Nova I', tier: 2, division: 1 }; }
    static get GOLD_NOVA_II() { return { name: 'Gold Nova II', tier: 2, division: 2 }; }
    static get GOLD_NOVA_III() { return { name: 'Gold Nova III', tier: 2, division: 3 }; }
    static get GOLD_NOVA_MASTER() { return { name: 'Gold Nova Master', tier: 2, division: 4 }; }

    static get MASTER_GUARDIAN_I() { return { name: 'Master Guardian I', tier: 3, division: 1 }; }
    static get MASTER_GUARDIAN_II() { return { name: 'Master Guardian II', tier: 3, division: 2 }; }
    static get MASTER_GUARDIAN_ELITE() { return { name: 'Master Guardian Elite', tier: 3, division: 3 }; }
    static get DISTINGUISHED_MASTER_GUARDIAN() { return { name: 'Distinguished Master Guardian', tier: 3, division: 4 }; }

    static get LEGENDARY_EAGLE() { return { name: 'Legendary Eagle', tier: 4, division: 1 }; }
    static get LEGENDARY_EAGLE_MASTER() { return { name: 'Legendary Eagle Master', tier: 4, division: 2 }; }

    static get SUPREME_MASTER_FIRST_CLASS() { return { name: 'Supreme Master First Class', tier: 5, division: 1 }; }

    static get GLOBAL_ELITE() { return { name: 'Global Elite', tier: 6, division: 1 }; }

    static getAll() {
        return [
            'Silver I', 'Silver II', 'Silver III', 'Silver IV', 'Silver Elite', 'Silver Elite Master',
            'Gold Nova I', 'Gold Nova II', 'Gold Nova III', 'Gold Nova Master',
            'Master Guardian I', 'Master Guardian II', 'Master Guardian Elite', 'Distinguished Master Guardian',
            'Legendary Eagle', 'Legendary Eagle Master',
            'Supreme Master First Class',
            'Global Elite'
        ];
    }

    static getByName(name) {
        const ranks = this.getAll();
        const index = ranks.indexOf(name);
        if (index === -1) return null;

        return {
            name,
            tier: Math.floor(index / 6) + 1,
            division: (index % 6) + 1
        };
    }

    static getByWinCount(wins) {
        if (wins < 10) return this.SILVER_I;
        if (wins < 50) return this.SILVER_II;
        if (wins < 100) return this.SILVER_III;
        if (wins < 200) return this.SILVER_IV;
        if (wins < 300) return this.SILVER_ELITE;
        if (wins < 400) return this.SILVER_ELITE_MASTER;
        if (wins < 500) return this.GOLD_NOVA_I;
        if (wins < 600) return this.GOLD_NOVA_II;
        if (wins < 750) return this.GOLD_NOVA_III;
        if (wins < 900) return this.GOLD_NOVA_MASTER;
        if (wins < 1100) return this.MASTER_GUARDIAN_I;
        if (wins < 1300) return this.MASTER_GUARDIAN_II;
        if (wins < 1500) return this.MASTER_GUARDIAN_ELITE;
        if (wins < 1750) return this.DISTINGUISHED_MASTER_GUARDIAN;
        if (wins < 2000) return this.LEGENDARY_EAGLE;
        if (wins < 2300) return this.LEGENDARY_EAGLE_MASTER;
        if (wins < 2600) return this.SUPREME_MASTER_FIRST_CLASS;
        return this.GLOBAL_ELITE;
    }

    static getRankDifference(rank1, rank2) {
        const ranks = this.getAll();
        const index1 = ranks.indexOf(rank1.name);
        const index2 = ranks.indexOf(rank2.name);
        return index2 - index1;
    }
}

export class SkillRating {
    constructor(config = {}) {
        this.rank = config.rank || PlayerRank.SILVER_I;
        this.elo = config.elo || 1000;
        this.winCount = config.winCount || 0;
        this.lossCount = config.lossCount || 0;
        this.drawCount = config.drawCount || 0;
        this.killCount = config.killCount || 0;
        this.deathCount = config.deathCount || 0;
        this.headshotCount = config.headshotCount || 0;
        this.damageCount = config.damageCount || 0;
        this.mvpCount = config.mvpCount || 0;
        this.hltvRating = config.hltvRating || 1.0;
        this.acelRating = config.acelRating || 0;
        this.form = [];
        this.maxFormLength = 20;
        this.calibrationMatches = config.calibrationMatches || 0;
        this.calibrationComplete = this.calibrationMatches >= 10;
    }

    getWinRate() {
        const total = this.winCount + this.lossCount;
        if (total === 0) return 0;
        return this.winCount / total;
    }

    getKDRatio() {
        if (this.deathCount === 0) return this.killCount;
        return this.killCount / this.deathCount;
    }

    getHeadshotPercent() {
        if (this.killCount === 0) return 0;
        return (this.headshotCount / this.killCount) * 100;
    }

    getAverageDamage() {
        const totalRounds = (this.winCount + this.lossCount) * 30;
        if (totalRounds === 0) return 0;
        return this.damageCount / totalRounds;
    }

    addMatchResult(result) {
        this.winCount += result.win ? 1 : 0;
        this.lossCount += result.loss ? 1 : 0;
        this.drawCount += result.draw ? 1 : 0;
        this.killCount += result.kills;
        this.deathCount += result.deaths;
        this.headshotCount += result.headshots;
        this.damageCount += result.damage;
        this.mvpCount += result.mvp ? 1 : 0;
        this.hltvRating = this.calculateHLTVRating();
        this.acelRating = this.calculateACELRating();

        if (result.eloChange) {
            this.elo += result.eloChange;
        }

        this.updateForm(result);
        this.updateRank();
    }

    updateForm(result) {
        const outcome = result.win ? 1 : result.draw ? 0.5 : 0;
        this.form.push(outcome);

        if (this.form.length > this.maxFormLength) {
            this.form.shift();
        }
    }

    getRecentForm(count = 5) {
        return this.form.slice(-count);
    }

    getFormRating() {
        if (this.form.length === 0) return 1.0;

        const recent = this.form.slice(-this.maxFormLength);
        const wins = recent.filter(f => f === 1).length;
        const draws = recent.filter(f => f === 0.5).length;
        const total = wins + draws + recent.filter(f => f === 0).length;

        return (wins + draws * 0.5) / total;
    }

    calculateHLTVRating() {
        const kills = this.killCount;
        const deaths = Math.max(1, this.deathCount);
        const headshots = this.headshotCount;
        const rounds = (this.winCount + this.lossCount) * 30;
        const impact = (kills / rounds) * 2 + (headshots / rounds) * 0.5;

        const rating = 1 + impact * 0.5;
        return Math.min(2.0, Math.max(0.5, rating));
    }

    calculateACELRating() {
        const kills = this.killCount;
        const deaths = Math.max(1, this.deathCount);
        const assists = this.assists || 0;
        const mvps = this.mvpCount;

        return (kills / deaths) * 0.5 + assists * 0.1 + mvps * 0.2;
    }

    updateRank() {
        if (!this.calibrationComplete) {
            this.calibrationMatches++;
            if (this.calibrationMatches >= 10) {
                this.calibrationComplete = true;
                this.rank = PlayerRank.getByWinCount(this.winCount);
            }
        } else {
            const oldRank = this.rank;
            this.rank = PlayerRank.getByWinCount(this.winCount);

            if (this.rank.name !== oldRank.name) {
                return {
                    type: 'rankUp',
                    oldRank: oldRank,
                    newRank: this.rank
                };
            }
        }
        return null;
    }

    getSkillDistribution() {
        return {
            overall: this.getOverallSkill(),
            aim: this.getAimSkill(),
            utility: this.getUtilitySkill(),
            clutch: this.getClutchSkill(),
            communication: this.getCommunicationSkill()
        };
    }

    getOverallSkill() {
        const kd = this.getKDRatio();
        const hs = this.getHeadshotPercent();
        const form = this.getFormRating();

        return Math.min(100, Math.floor((kd * 30 + hs * 0.3 + form * 40)));
    }

    getAimSkill() {
        const hs = this.getHeadshotPercent();
        const kd = this.getKDRatio();

        return Math.min(100, Math.floor(hs * 0.6 + kd * 20));
    }

    getUtilitySkill() {
        const rounds = (this.winCount + this.lossCount) * 30;
        const avgDamage = rounds > 0 ? this.damageCount / rounds : 0;

        return Math.min(100, Math.floor(avgDamage * 0.5));
    }

    getClutchSkill() {
        const clutchWins = this.clutchWins || 0;
        const clutchAttempts = this.clutchAttempts || 1;

        return Math.min(100, Math.floor((clutchWins / clutchAttempts) * 100));
    }

    getCommunicationSkill() {
        const assists = this.assists || 0;
        const rounds = (this.winCount + this.lossCount) * 30;

        return Math.min(100, Math.floor((assists / rounds) * 1000));
    }

    calculateEloChange(opponentElo, won) {
        const kFactor = this.calibrationComplete ? 32 : 64;
        const expectedScore = 1 / (1 + Math.pow(10, (opponentElo - this.elo) / 400));
        const actualScore = won ? 1 : 0;
        const change = Math.round(kFactor * (actualScore - expectedScore));

        return change;
    }

    toJSON() {
        return {
            rank: this.rank,
            elo: this.elo,
            winCount: this.winCount,
            lossCount: this.lossCount,
            drawCount: this.drawCount,
            killCount: this.killCount,
            deathCount: this.deathCount,
            headshotCount: this.headshotCount,
            damageCount: this.damageCount,
            mvpCount: this.mvpCount,
            hltvRating: this.hltvRating,
            acelRating: this.acelRating,
            form: this.form,
            calibrationMatches: this.calibrationMatches,
            calibrationComplete: this.calibrationComplete
        };
    }

    fromJSON(json) {
        Object.assign(this, json);
    }
}

export class ServiceMedal {
    constructor(config = {}) {
        this.type = config.type || 'bronze';
        this.year = config.year || new Date().getFullYear();
        this.unlocked = false;
        this.unlockedAt = null;
        this.progress = 0;
        this.requiredWins = this.getRequiredWins();
    }

    getRequiredWins() {
        const requirements = {
            bronze: 10,
            silver: 25,
            gold: 50,
            platinum: 100,
            diamond: 250,
            diamond_plus: 500
        };
        return requirements[this.type] || 50;
    }

    addWin() {
        if (this.unlocked) return false;

        this.progress++;
        if (this.progress >= this.requiredWins) {
            this.unlocked = true;
            this.unlockedAt = Date.now();
            return true;
        }
        return false;
    }

    getProgress() {
        return {
            type: this.type,
            year: this.year,
            progress: this.progress,
            required: this.requiredWins,
            percent: Math.floor((this.progress / this.requiredWins) * 100),
            unlocked: this.unlocked,
            daysRemaining: this.getDaysRemaining()
        };
    }

    getDaysRemaining() {
        const yearEnd = new Date(this.year, 11, 31);
        const now = new Date();
        const diff = yearEnd - now;
        return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
    }

    canClaim() {
        return this.unlocked && !this.claimed;
    }

    claim() {
        if (this.canClaim()) {
            this.claimed = true;
            return true;
        }
        return false;
    }
}

export class PlayerCard {
    constructor(config = {}) {
        this.id = config.id || PlayerCard.nextId++;
        this.name = config.name || 'Player Card';
        this.description = config.description || '';
        this.imageUrl = config.imageUrl || '';
        this.rarity = config.rarity || 'common';
        this.statBoost = config.statBoost || null;
        this.specialAbility = config.specialAbility || null;
        this.unlocked = config.unlocked || false;
        this.unlockedAt = config.unlockedAt || null;
        this.equipped = config.equipped || false;
        this.overlayColor = config.overlayColor || null;
        this.badgePosition = config.badgePosition || 'bottom-right';
        this.animation = config.animation || null;
    }

    static get nextId() {
        return PlayerCard._nextId++;
    }

    equip() {
        this.equipped = true;
    }

    unequip() {
        this.equipped = false;
    }

    getStatBoost() {
        if (!this.statBoost) return null;
        return {
            type: this.statBoost.type,
            value: this.statBoost.value,
            description: `+${this.statBoost.value}% ${this.statBoost.type}`
        };
    }
}

PlayerCard._nextId = 0;

export class Graffiti {
    constructor(config = {}) {
        this.id = config.id || Graffiti.nextId++;
        this.name = config.name || 'Graffiti';
        this.description = config.description || '';
        this.imageUrl = config.imageUrl || '';
        this.rarity = config.rarity || 'common';
        this.uses = config.uses || 10;
        this.maxUses = config.maxUses || 10;
        this.unlocked = config.unlocked || false;
        this.unlockedAt = config.unlockedAt || null;
        this.owned = config.owned || 0;
        this.isActive = config.isActive || false;
    }

    use() {
        if (this.uses > 0) {
            this.uses--;
            if (this.uses === 0) {
                this.isActive = false;
            }
            return true;
        }
        return false;
    }

    refill(uses) {
        this.uses = Math.min(this.maxUses, this.uses + uses);
    }

    getRemaining() {
        return {
            current: this.uses,
            max: this.maxUses,
            percent: Math.floor((this.uses / this.maxUses) * 100)
        };
    }
}

Graffiti.nextId = 0;

export class AgentSkin {
    constructor(config = {}) {
        this.id = config.id || AgentSkin.nextId++;
        this.name = config.name || 'Agent';
        this.collection = config.collection || '';
        this.rarity = config.rarity || 'common';
        this.imageUrl = config.imageUrl || '';
        this.modelUrl = config.modelUrl || '';
        this.unlocked = config.unlocked || false;
        this.unlockedAt = config.unlockedAt || null;
        this.isEquipped = config.isEquipped || false;
        this.team = config.team || 'both';
        this.armSlot = config.armSlot || 'default';
    }

    static get nextId() {
        return AgentSkin._nextId++;
    }

    equip() {
        this.isEquipped = true;
    }

    unequip() {
        this.isEquipped = false;
    }
}

AgentSkin._nextId = 0;

export class MusicKit {
    constructor(config = {}) {
        this.id = config.id || MusicKit.nextId++;
        this.name = config.name || 'Music Kit';
        this.artist = config.artist || 'Unknown Artist';
        this.description = config.description || '';
        this.rarity = config.rarity || 'common';
        this.previewUrl = config.previewUrl || '';
        this.fullUrl = config.fullUrl || '';
        this.unlocked = config.unlocked || false;
        this.unlockedAt = config.unlockedAt || null;
        this.isEquipped = config.isEquipped || false;
        this.statEvent = config.statEvent || 'none';
        this.statValue = config.statValue || 0;
    }

    static get nextId() {
        return MusicKit._nextId++;
    }

    equip() {
        this.isEquipped = true;
    }

    unequip() {
        this.isEquipped = false;
    }

    getStatBonus() {
        if (this.statEvent === 'none') return null;
        return {
            event: this.statEvent,
            value: this.statValue,
            description: `+${this.statValue}% ${this.statEvent}`
        };
    }
}

MusicKit._nextId = 0;

export class Inventory {
    constructor(config = {}) {
        this.playerId = config.playerId || null;
        this.items = new Map();
        this.equippedItems = new Map();
        this.maxSlots = config.maxSlots || 1000;
        this.weapons = [];
        this.skins = [];
        this.agents = [];
        this.musicKits = [];
        this.graffiti = [];
        this.sprays = [];
        this.cases = [];
        this.keys = [];
        this.stickers = [];
        this.patches = [];
        this.charmSlots = [];
        this.initialize();
    }

    initialize() {
        this.addDefaultItems();
    }

    addDefaultItems() {
        this.addItem({
            type: 'weapon',
            id: 'weapon_knife_default',
            name: 'Knife',
            equipped: true,
            slot: 'melee'
        });

        this.addItem({
            type: 'spray',
            id: 'spray_default',
            name: 'Default Spray',
            uses: -1,
            equipped: true
        });

        this.addItem({
            type: 'graffiti',
            id: 'graffiti_default',
            name: 'Default Graffiti',
            uses: 100,
            equipped: true
        });
    }

    addItem(item) {
        const itemId = item.id || `${item.type}_${item.name.toLowerCase().replace(/\s+/g, '_')}`;

        if (this.items.size >= this.maxSlots && !this.items.has(itemId)) {
            return { success: false, reason: 'inventory_full' };
        }

        const existingItem = this.items.get(itemId);
        if (existingItem) {
            if (existingItem.uses !== undefined) {
                existingItem.uses += item.uses || 1;
            }
            existingItem.quantity = (existingItem.quantity || 1) + (item.quantity || 1);
            return { success: true, item: existingItem, stacked: true };
        }

        const newItem = {
            ...item,
            itemId,
            addedAt: Date.now(),
            quantity: item.quantity || 1
        };

        this.items.set(itemId, newItem);
        this.categorizeItem(newItem);

        return { success: true, item: newItem };
    }

    categorizeItem(item) {
        switch (item.type) {
            case 'weapon':
                this.weapons.push(item);
                break;
            case 'skin':
            case 'weapon_skin':
                this.skins.push(item);
                break;
            case 'agent':
                this.agents.push(item);
                break;
            case 'music':
            case 'music_kit':
                this.musicKits.push(item);
                break;
            case 'graffiti':
                this.graffiti.push(item);
                break;
            case 'spray':
                this.sprays.push(item);
                break;
            case 'case':
            case 'container':
                this.cases.push(item);
                break;
            case 'key':
                this.keys.push(item);
                break;
            case 'sticker':
                this.stickers.push(item);
                break;
            case 'patch':
                this.patches.push(item);
                break;
            case 'charm':
                this.charmSlots.push(item);
                break;
        }
    }

    removeItem(itemId, quantity = 1) {
        const item = this.items.get(itemId);
        if (!item) {
            return { success: false, reason: 'item_not_found' };
        }

        if (item.quantity !== undefined && item.quantity < quantity) {
            return { success: false, reason: 'insufficient_quantity' };
        }

        if (item.quantity !== undefined) {
            item.quantity -= quantity;
            if (item.quantity <= 0) {
                this.items.delete(itemId);
            }
        } else if (item.uses !== undefined) {
            item.uses--;
            if (item.uses <= 0) {
                this.items.delete(itemId);
            }
        } else {
            this.items.delete(itemId);
        }

        return { success: true };
    }

    equipItem(itemId, slot) {
        const item = this.items.get(itemId);
        if (!item) {
            return { success: false, reason: 'item_not_found' };
        }

        if (item.equipped && item.slot === slot) {
            return { success: false, reason: 'already_equipped' };
        }

        const previousEquipped = this.equippedItems.get(slot);
        if (previousEquipped) {
            previousEquipped.equipped = false;
        }

        item.equipped = true;
        item.slot = slot;
        this.equippedItems.set(slot, item);

        return { success: true, previousEquipped };
    }

    unequipItem(slot) {
        const item = this.equippedItems.get(slot);
        if (!item) {
            return { success: false, reason: 'slot_empty' };
        }

        item.equipped = false;
        item.slot = null;
        this.equippedItems.delete(slot);

        return { success: true, item };
    }

    getEquippedItems() {
        return Array.from(this.equippedItems.values());
    }

    getItemById(itemId) {
        return this.items.get(itemId);
    }

    getItemsByType(type) {
        const results = [];
        for (const item of this.items.values()) {
            if (item.type === type || item.type === type + 's') {
                results.push(item);
            }
        }
        return results;
    }

    getItemsByRarity(rarity) {
        const results = [];
        for (const item of this.items.values()) {
            if (item.rarity === rarity) {
                results.push(item);
            }
        }
        return results;
    }

    getInventoryValue() {
        let totalValue = 0;
        for (const item of this.items.values()) {
            totalValue += (item.value || 0) * (item.quantity || 1);
        }
        return totalValue;
    }

    getInventoryStats() {
        return {
            totalItems: this.items.size,
            maxSlots: this.maxSlots,
            usedSlots: this.items.size,
            availableSlots: this.maxSlots - this.items.size,
            byType: {
                weapons: this.weapons.length,
                skins: this.skins.length,
                agents: this.agents.length,
                musicKits: this.musicKits.length,
                graffiti: this.graffiti.length,
                sprays: this.sprays.length,
                cases: this.cases.length,
                keys: this.keys.length
            },
            byRarity: this.getRarityCounts()
        };
    }

    getRarityCounts() {
        const counts = {
            common: 0,
            uncommon: 0,
            rare: 0,
            mythical: 0,
            legendary: 0,
            ancient: 0,
            contraband: 0
        };

        for (const item of this.items.values()) {
            if (counts[item.rarity] !== undefined) {
                counts[item.rarity]++;
            }
        }

        return counts;
    }

    sortItems(criteria, order = 'desc') {
        const items = Array.from(this.items.values());

        items.sort((a, b) => {
            let comparison = 0;

            switch (criteria) {
                case 'name':
                    comparison = (a.name || '').localeCompare(b.name || '');
                    break;
                case 'rarity':
                    const rarityOrder = ['common', 'uncommon', 'rare', 'mythical', 'legendary', 'ancient', 'contraband'];
                    comparison = rarityOrder.indexOf(a.rarity) - rarityOrder.indexOf(b.rarity);
                    break;
                case 'value':
                    comparison = (a.value || 0) - (b.value || 0);
                    break;
                case 'added':
                    comparison = (a.addedAt || 0) - (b.addedAt || 0);
                    break;
                case 'type':
                    comparison = (a.type || '').localeCompare(b.type || '');
                    break;
                default:
                    comparison = 0;
            }

            return order === 'desc' ? -comparison : comparison;
        });

        return items;
    }

    filterItems(filters) {
        let items = Array.from(this.items.values());

        if (filters.type) {
            items = items.filter(item => item.type === filters.type);
        }

        if (filters.rarity) {
            items = items.filter(item => item.rarity === filters.rarity);
        }

        if (filters.equipped !== undefined) {
            items = items.filter(item => item.equipped === filters.equipped);
        }

        if (filters.search) {
            const search = filters.search.toLowerCase();
            items = items.filter(item =>
                (item.name && item.name.toLowerCase().includes(search)) ||
                (item.description && item.description.toLowerCase().includes(search))
            );
        }

        if (filters.minValue !== undefined) {
            items = items.filter(item => (item.value || 0) >= filters.minValue);
        }

        if (filters.maxValue !== undefined) {
            items = items.filter(item => (item.value || 0) <= filters.maxValue);
        }

        return items;
    }

    toJSON() {
        return {
            playerId: this.playerId,
            items: Array.from(this.items.entries()),
            equippedItems: Array.from(this.equippedItems.entries()),
            maxSlots: this.maxSlots
        };
    }

    fromJSON(json) {
        this.playerId = json.playerId;
        this.items = new Map(json.items);
        this.equippedItems = new Map(json.equippedItems);
        this.maxSlots = json.maxSlots;
    }
}

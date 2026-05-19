import { GameState } from './config.js';

export const EconomyEventType = {
    ROUND_START: 'round_start',
    ROUND_END: 'round_end',
    KILL: 'kill',
    ASSIST: 'assist',
    PLANT: 'plant',
    DEFUSE: 'defuse',
    HOSTAGE_RESCUE: 'hostage_rescue',
    HOSTAGE_DEATH: 'hostage_death',
    PURCHASE: 'purchase',
    REFUND: 'refund',
    LOSS_BONUS: 'loss_bonus',
    WIN_BONUS: 'win_bonus',
    ITEM_DROP: 'item_drop',
    TIMEOUT: 'timeout'
};

export const BuyCategory = {
    PISTOL: 'pistol',
    SHOTGUN: 'shotgun',
    SMG: 'smg',
    RIFLE: 'rifle',
    SNIPER: 'sniper',
    HEAVY: 'heavy',
    ARMOR: 'armor',
    GRENADE: 'grenade',
    EQUIPMENT: 'equipment',
    DEFUSE_KIT: 'defuse_kit',
    RESCUE_PLAYER: 'rescue_player'
};

export const EconomyConfig = {
    lossBonus: {
        0: 1400,
        1: 1900,
        2: 2400,
        3: 2900,
        4: 3400
    },
    winBonus: {
        0: 3250,
        1: 3250,
        2: 3250,
        3: 3250,
        4: 3250
    },
    pistolRoundWin: 800,
    plantReward: 300,
    defuseReward: 300,
    hostageReward: 150,
    assistReward: 150,
    killReward: 150,
    surviveReward: 300,
    overtimeMultiplier: 1.5
};

export class BuyMenu {
    constructor() {
        this.items = this.initItems();
        this.restrictions = this.initRestrictions();
    }

    initItems() {
        return {
            [BuyCategory.PISTOL]: [
                { id: 'hkp2000', name: 'P2000', price: 200, ammo: 40 },
                { id: 'glock', name: 'Glock-18', price: 200, ammo: 40 },
                { id: 'p250', name: 'P250', price: 300, ammo: 52 },
                { id: 'deagle', name: 'Desert Eagle', price: 700, ammo: 35 },
                { id: 'tec9', name: 'Tec-9', price: 500, ammo: 60 },
                { id: 'fiveseven', name: 'Five-SeveN', price: 500, ammo: 40 },
                { id: 'revolver', name: 'R8 Revolver', price: 600, ammo: 8 }
            ],
            [BuyCategory.RIFLE]: [
                { id: 'ak47', name: 'AK-47', price: 2700, ammo: 90 },
                { id: 'm4a4', name: 'M4A4', price: 3100, ammo: 90 },
                { id: 'm4a1', name: 'M4A1-S', price: 2900, ammo: 75 },
                { id: 'galilar', name: 'Galil AR', price: 2000, ammo: 90 },
                { id: 'famas', name: 'FAMAS', price: 2250, ammo: 90 },
                { id: 'aug', name: 'AUG', price: 3300, ammo: 90 },
                { id: 'sg553', name: 'SG 553', price: 3000, ammo: 90 },
                { id: 'scar20', name: 'SCAR-20', price: 5000, ammo: 40 },
                { id: 'g3sg1', name: 'G3SG1', price: 5000, ammo: 40 }
            ],
            [BuyCategory.SMG]: [
                { id: 'mac10', name: 'MAC-10', price: 1050, ammo: 120 },
                { id: 'mp9', name: 'MP9', price: 1250, ammo: 120 },
                { id: 'ump45', name: 'UMP-45', price: 1200, ammo: 100 },
                { id: 'bizon', name: 'PP-Bizon', price: 1400, ammo: 120 },
                { id: 'mp7', name: 'MP7', price: 1500, ammo: 120 },
                { id: 'mp5sd', name: 'MP5-SD', price: 1700, ammo: 120 }
            ],
            [BuyCategory.SHOTGUN]: [
                { id: 'nova', name: 'Nova', price: 1200, ammo: 32 },
                { id: 'xm1014', name: 'XM1014', price: 2000, ammo: 32 },
                { id: 'mag7', name: 'MAG-7', price: 1300, ammo: 28 },
                { id: 'sawedoff', name: 'Sawed-Off', price: 1100, ammo: 28 }
            ],
            [BuyCategory.SNIPER]: [
                { id: 'awp', name: 'AWP', price: 4750, ammo: 30 },
                { id: 'ssg08', name: 'SSG 08', price: 2700, ammo: 30 },
                { id: 'scout', name: 'SSG 08', price: 2700, ammo: 30 }
            ],
            [BuyCategory.HEAVY]: [
                { id: 'negev', name: 'Negev', price: 5700, ammo: 200 },
                { id: 'm249', name: 'M249', price: 5200, ammo: 200 },
                { id: 'nova', name: 'Nova', price: 1200, ammo: 32 }
            ],
            [BuyCategory.ARMOR]: [
                { id: 'kevlar', name: 'Kevlar Vest', price: 650, protection: 0.75 },
                { id: 'kevlar_helmet', name: 'Kevlar + Helmet', price: 1000, protection: 0.8 }
            ],
            [BuyCategory.GRENADE]: [
                { id: 'flash', name: 'Flashbang', price: 200, type: 'flash' },
                { id: 'he', name: 'HE Grenade', price: 300, type: 'he' },
                { id: 'smoke', name: 'Smoke Grenade', price: 200, type: 'smoke' },
                { id: 'molotov', name: 'Molotov', price: 400, type: 'molotov' },
                { id: 'incendiary', name: 'Incendiary', price: 400, type: 'incendiary' },
                { id: 'decoy', name: 'Decoy Grenade', price: 50, type: 'decoy' }
            ],
            [BuyCategory.EQUIPMENT]: [
                { id: 'zeus', name: 'Zeus x27', price: 200 },
                { id: 'c4', name: 'C4', price: 0 },
                { id: 'defuse_kit', name: 'Defuse Kit', price: 400 }
            ]
        };
    }

    initRestrictions() {
        return {
            maxGrenades: 4,
            maxHeavyWeapons: 1,
            maxRifles: 1,
            maxPistols: 1,
            maxArmor: 1
        };
    }

    getItems(category) {
        return this.items[category] || [];
    }

    getAllItems() {
        return Object.values(this.items).flat();
    }

    getItem(itemId) {
        for (const category of Object.values(this.items)) {
            const item = category.find(i => i.id === itemId);
            if (item) return item;
        }
        return null;
    }

    getCategoryForItem(itemId) {
        for (const [category, items] of Object.entries(this.items)) {
            if (items.some(i => i.id === itemId)) {
                return category;
            }
        }
        return null;
    }

    canAfford(playerMoney, itemPrice) {
        return playerMoney >= itemPrice;
    }

    getBestRifle(money) {
        if (money >= 4750) {
            return this.items[BuyCategory.SNIPER].find(i => i.id === 'awp');
        }
        if (money >= 3100) {
            return this.items[BuyCategory.RIFLE].find(i => i.id === 'm4a4');
        }
        if (money >= 2700) {
            return this.items[BuyCategory.RIFLE].find(i => i.id === 'ak47');
        }
        if (money >= 2000) {
            return this.items[BuyCategory.RIFLE].find(i => i.id === 'galilar');
        }
        return null;
    }

    getBestPistol(money) {
        if (money >= 700) {
            return this.items[BuyCategory.PISTOL].find(i => i.id === 'deagle');
        }
        if (money >= 500) {
            return this.items[BuyCategory.PISTOL].find(i => i.id === 'tec9');
        }
        if (money >= 300) {
            return this.items[BuyCategory.PISTOL].find(i => i.id === 'p250');
        }
        return this.items[BuyCategory.PISTOL].find(i => i.id === 'glock');
    }

    getBestSmg(money) {
        if (money >= 1700) {
            return this.items[BuyCategory.SMG].find(i => i.id === 'mp5sd');
        }
        if (money >= 1500) {
            return this.items[BuyCategory.SMG].find(i => i.id === 'mp7');
        }
        return null;
    }
}

export class EconomyEvent {
    constructor(type, playerId, data) {
        this.id = `econ_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
        this.type = type;
        this.playerId = playerId;
        this.data = data;
        this.timestamp = Date.now();
        this.amount = data.amount || 0;
        this.item = data.item || null;
        this.reason = data.reason || null;
    }

    export() {
        return {
            id: this.id,
            type: this.type,
            player_id: this.playerId,
            data: this.data,
            timestamp: this.timestamp,
            amount: this.amount,
            item: this.item,
            reason: this.reason
        };
    }
}

export class PlayerEconomy {
    constructor(playerId) {
        this.playerId = playerId;
        this.money = 800;
        this.startMoney = 800;
        this.totalEarned = 0;
        this.totalSpent = 0;
        this.purchases = [];
        this.refunds = [];
        this.events = [];
        this.inventory = [];
        this.currentWeapon = null;
        this.armor = 0;
        this.helmet = false;
        this.hasDefuseKit = false;
        this.hasRescueKit = false;
        this.grenades = {
            flash: 0,
            he: 0,
            smoke: 0,
            molotov: 0,
            decoy: 0
        };
        this.equipment = {
            zeus: false,
            c4: false
        };
        this.buyStyle = 'normal';
        this.ecoCount = 0;
        this.lossStreak = 0;
        this.lastBuy = null;
        this.suggestedBuy = null;
    }

    addMoney(amount, reason) {
        this.money += amount;
        this.totalEarned += amount;

        const event = new EconomyEvent(EconomyEventType.ITEM_DROP, this.playerId, {
            amount,
            reason
        });
        this.events.push(event);

        return event;
    }

    spendMoney(amount, reason) {
        if (this.money < amount) {
            return false;
        }

        this.money -= amount;
        this.totalSpent += amount;

        return true;
    }

    purchase(item, price) {
        if (!this.spendMoney(price, `Purchase ${item.name}`)) {
            return false;
        }

        const purchase = {
            item: item.id,
            name: item.name,
            price,
            timestamp: Date.now()
        };

        this.purchases.push(purchase);
        this.lastBuy = purchase;
        this.inventory.push(item);

        const event = new EconomyEvent(EconomyEventType.PURCHASE, this.playerId, {
            item: item.id,
            price
        });
        this.events.push(event);

        this.updateInventory(item);

        return true;
    }

    updateInventory(item) {
        const category = BuyMenu.prototype.getCategoryForItem(item.id);

        switch (category) {
            case BuyCategory.ARMOR:
                if (item.id.includes('helmet')) {
                    this.helmet = true;
                }
                this.armor = item.protection * 100;
                break;
            case BuyCategory.DEFUSE_KIT:
                this.hasDefuseKit = true;
                break;
            case BuyCategory.GRENADE:
                const grenadeType = item.type || item.id;
                if (this.grenades[grenadeType] !== undefined) {
                    this.grenades[grenadeType]++;
                }
                break;
            case BuyCategory.EQUIPMENT:
                if (item.id === 'zeus') {
                    this.equipment.zeus = true;
                }
                break;
            default:
                if (this.currentWeapon) {
                }
                this.currentWeapon = item.id;
        }
    }

    refund(item, price) {
        this.money += price;

        const refund = {
            item: item.id,
            name: item.name,
            price,
            timestamp: Date.now()
        };

        this.refunds.push(refund);

        const event = new EconomyEvent(EconomyEventType.REFUND, this.playerId, {
            item: item.id,
            price
        });
        this.events.push(event);
    }

    resetForNewRound() {
        this.currentWeapon = null;
        this.armor = 0;
        this.helmet = false;
        this.hasDefuseKit = false;
        this.hasRescueKit = false;
        this.grenades = {
            flash: 0,
            he: 0,
            smoke: 0,
            molotov: 0,
            decoy: 0
        };
        this.equipment = {
            zeus: false,
            c4: false
        };
        this.inventory = [];
    }

    setLossStreak(streak) {
        this.lossStreak = streak;
    }

    setBuyStyle(style) {
        this.buyStyle = style;
    }

    getLossBonus() {
        const streak = Math.min(this.lossStreak, 4);
        return EconomyConfig.lossBonus[streak];
    }

    getWinBonus() {
        return EconomyConfig.winBonus[0];
    }

    canAfford(price) {
        return this.money >= price;
    }

    getTotalLoadoutValue() {
        let value = this.money;

        for (const item of this.inventory) {
            value += item.price;
        }

        return value;
    }

    getBuyRecommendation(opponentEconomy = null) {
        const recommendations = {
            force: [],
            eco: [],
            full: []
        };

        if (this.money < 1500) {
            return { type: 'eco', items: [] };
        }

        if (this.money >= 4500) {
            recommendations.full.push({ type: 'rifle', item: this.getBestRifle(this.money) });
            recommendations.full.push({ type: 'armor', item: { id: 'kevlar_helmet', name: 'Kevlar + Helmet', price: 1000 } });
        } else if (this.money >= 2500) {
            recommendations.force.push({ type: 'smg', item: this.getBestSmg(this.money) });
        }

        if (this.money >= 200) {
            recommendations.full.push({ type: 'smoke', item: { id: 'smoke', name: 'Smoke', price: 200 } });
        }

        if (this.money >= 200) {
            recommendations.full.push({ type: 'flash', item: { id: 'flash', name: 'Flashbang', price: 200 } });
        }

        if (this.money >= 300) {
            recommendations.full.push({ type: 'he', item: { id: 'he', name: 'HE Grenade', price: 300 } });
        }

        if (opponentEconomy && opponentEconomy.money > 5000) {
            if (this.money >= 400) {
                recommendations.full.push({ type: 'defuse_kit', item: { id: 'defuse_kit', name: 'Defuse Kit', price: 400 } });
            }
        }

        return recommendations;
    }

    export() {
        return {
            player_id: this.playerId,
            money: this.money,
            start_money: this.startMoney,
            total_earned: this.totalEarned,
            total_spent: this.totalSpent,
            purchases: this.purchases,
            refunds: this.refunds,
            events: this.events.map(e => e.export()),
            inventory: this.inventory,
            current_weapon: this.currentWeapon,
            armor: this.armor,
            helmet: this.helmet,
            has_defuse_kit: this.hasDefuseKit,
            has_rescue_kit: this.hasRescueKit,
            grenades: { ...this.grenades },
            equipment: { ...this.equipment },
            buy_style: this.buyStyle,
            eco_count: this.ecoCount,
            loss_streak: this.lossStreak,
            last_buy: this.lastBuy,
            suggested_buy: this.suggestedBuy
        };
    }

    static import(data) {
        const economy = new PlayerEconomy(data.player_id);
        economy.money = data.money;
        economy.startMoney = data.start_money;
        economy.totalEarned = data.total_earned;
        economy.totalSpent = data.total_spent;
        economy.purchases = data.purchases || [];
        economy.refunds = data.refunds || [];
        economy.inventory = data.inventory || [];
        economy.currentWeapon = data.current_weapon;
        economy.armor = data.armor;
        economy.helmet = data.helmet;
        economy.hasDefuseKit = data.has_defuse_kit;
        economy.hasRescueKit = data.has_rescue_kit;
        economy.grenades = data.grenades;
        economy.equipment = data.equipment;
        economy.buyStyle = data.buy_style;
        economy.ecoCount = data.eco_count;
        economy.lossStreak = data.loss_streak;
        economy.lastBuy = data.last_buy;
        economy.suggestedBuy = data.suggested_buy;
        return economy;
    }
}

export class TeamEconomy {
    constructor(teamId) {
        this.teamId = teamId;
        this.players = new Map();
        this.teamMoney = 0;
        this.averageMoney = 0;
        this.totalEquipmentValue = 0;
        this.alivePlayers = 0;
        this.buyStyle = 'normal';
        this.buySuccess = 0;
        this.ecoWins = 0;
        this.ecoLosses = 0;
        this.forceWins = 0;
        this.forceLosses = 0;
        this.fullBuyWins = 0;
        this.fullBuyLosses = 0;
        this.roundsWon = 0;
        this.roundsLost = 0;
        this.events = [];
    }

    addPlayer(playerId) {
        const economy = new PlayerEconomy(playerId);
        this.players.set(playerId, economy);
        return economy;
    }

    removePlayer(playerId) {
        return this.players.delete(playerId);
    }

    getPlayer(playerId) {
        return this.players.get(playerId);
    }

    getAllPlayers() {
        return Array.from(this.players.values());
    }

    updateTeamMoney() {
        this.teamMoney = 0;
        for (const player of this.players.values()) {
            this.teamMoney += player.money;
        }
        this.averageMoney = this.players.size > 0 ? this.teamMoney / this.players.size : 0;
    }

    updateEquipmentValue() {
        this.totalEquipmentValue = 0;
        for (const player of this.players.values()) {
            this.totalEquipmentValue += player.getTotalLoadoutValue();
        }
    }

    recordRoundWin(winType, bonus = 0) {
        this.roundsWon++;

        for (const player of this.players.values()) {
            player.addMoney(winType === 'eco' ? 1500 : 3250, 'Round win bonus');

            if (bonus > 0) {
                player.addMoney(bonus, 'Plant/Defuse bonus');
            }
        }

        for (const player of this.players.values()) {
            player.setLossStreak(0);
        }

        if (winType === 'eco') {
            this.ecoWins++;
        } else if (winType === 'force') {
            this.forceWins++;
        } else {
            this.fullBuyWins++;
        }

        const event = new EconomyEvent(EconomyEventType.ROUND_END, this.teamId, {
            result: 'win',
            type: winType,
            bonus
        });
        this.events.push(event);
    }

    recordRoundLoss(lossType, lossBonus = 0) {
        this.roundsLost++;

        for (const player of this.players.values()) {
            player.lossStreak++;
            const bonus = player.getLossBonus();
            player.addMoney(bonus, 'Loss bonus');

            if (player.lossStreak >= 3) {
                player.setBuyStyle('eco');
            }
        }

        if (lossType === 'eco') {
            this.ecoLosses++;
        } else if (lossType === 'force') {
            this.forceLosses++;
        } else {
            this.fullBuyLosses++;
        }

        const event = new EconomyEvent(EconomyEventType.ROUND_END, this.teamId, {
            result: 'loss',
            type: lossType,
            lossBonus
        });
        this.events.push(event);
    }

    resetForNewRound() {
        for (const player of this.players.values()) {
            player.resetForNewRound();
        }
    }

    getEconomyStatus() {
        this.updateTeamMoney();

        let status = 'full_buy';
        if (this.averageMoney < 1500) {
            status = 'eco';
        } else if (this.averageMoney < 3500) {
            status = 'force';
        }

        return {
            teamMoney: this.teamMoney,
            averageMoney: this.averageMoney,
            totalEquipmentValue: this.totalEquipmentValue,
            playerCount: this.players.size,
            status
        };
    }

    getBuyRecommendation() {
        const recommendations = [];

        for (const player of this.players.values()) {
            const playerRecommendation = player.getBuyRecommendation();
            recommendations.push({
                playerId: player.playerId,
                recommendation: playerRecommendation
            });
        }

        return recommendations;
    }

    export() {
        return {
            team_id: this.teamId,
            team_money: this.teamMoney,
            average_money: this.averageMoney,
            total_equipment_value: this.totalEquipmentValue,
            player_count: this.players.size,
            buy_style: this.buyStyle,
            buy_success: this.buySuccess,
            eco_wins: this.ecoWins,
            eco_losses: this.ecoLosses,
            force_wins: this.forceWins,
            force_losses: this.forceLosses,
            full_buy_wins: this.fullBuyWins,
            full_buy_losses: this.fullBuyLosses,
            rounds_won: this.roundsWon,
            rounds_lost: this.roundsLost,
            events: this.events.map(e => e.export())
        };
    }

    static import(data) {
        const team = new TeamEconomy(data.team_id);
        team.teamMoney = data.team_money;
        team.averageMoney = data.average_money;
        team.totalEquipmentValue = data.total_equipment_value;
        team.buyStyle = data.buy_style;
        team.buySuccess = data.buy_success;
        team.ecoWins = data.eco_wins;
        team.ecoLosses = data.eco_losses;
        team.forceWins = data.force_wins;
        team.forceLosses = data.force_losses;
        team.fullBuyWins = data.full_buy_wins;
        team.fullBuyLosses = data.full_buy_losses;
        team.roundsWon = data.rounds_won;
        team.roundsLost = data.rounds_lost;
        return team;
    }
}

export class EconomyManager {
    static instance = null;

    constructor() {
        this.buyMenu = new BuyMenu();
        this.teamEconomies = new Map();
        this.matchEconomy = {
            totalPistolRoundWins: 0,
            totalEcoWins: 0,
            totalForceWins: 0,
            totalFullBuyWins: 0,
            totalMoneyTransferred: 0,
            bestEcoStreak: 0,
            currentEcoStreak: 0
        };
        this.listeners = new Map();
    }

    static getInstance() {
        if (!EconomyManager.instance) {
            EconomyManager.instance = new EconomyManager();
        }
        return EconomyManager.instance;
    }

    createTeamEconomy(teamId) {
        const economy = new TeamEconomy(teamId);
        this.teamEconomies.set(teamId, economy);
        return economy;
    }

    getTeamEconomy(teamId) {
        return this.teamEconomies.get(teamId);
    }

    processKill(attackerId, victimId, isHeadshot, isFirstKill, isTrade) {
        const attackerEconomy = this.getPlayerEconomy(attackerId);
        const victimEconomy = this.getPlayerEconomy(victimId);

        const killReward = EconomyConfig.killReward;
        attackerEconomy.addMoney(killReward, 'Kill reward');

        if (isHeadshot) {
        }

        if (victimEconomy) {
            victimEconomy.currentWeapon = null;
        }

        return killReward;
    }

    processAssist(playerId, victimId) {
        const economy = this.getPlayerEconomy(playerId);
        const assistReward = EconomyConfig.assistReward;
        economy.addMoney(assistReward, 'Assist reward');
        return assistReward;
    }

    processPlant(playerId, isRetake) {
        const economy = this.getPlayerEconomy(playerId);
        const plantReward = isRetake ? 600 : EconomyConfig.plantReward;
        economy.addMoney(plantReward, 'Bomb plant bonus');
        return plantReward;
    }

    processDefuse(playerId, hasKit) {
        const economy = this.getPlayerEconomy(playerId);
        const defuseReward = hasKit ? 600 : EconomyConfig.defuseReward;
        economy.addMoney(defuseReward, 'Bomb defuse bonus');
        return defuseReward;
    }

    processHostageRescue(playerId) {
        const economy = this.getPlayerEconomy(playerId);
        economy.addMoney(EconomyConfig.hostageReward, 'Hostage rescue bonus');
        return EconomyConfig.hostageReward;
    }

    processRoundEnd(winnerTeamId, endReason, tScore, ctScore) {
        const teamEconomy = this.getTeamEconomy(winnerTeamId);

        let winType = 'full';
        if (teamEconomy.averageMoney < 1500) {
            winType = 'eco';
            this.matchEconomy.totalEcoWins++;
            this.matchEconomy.currentEcoStreak++;
            this.matchEconomy.bestEcoStreak = Math.max(
                this.matchEconomy.bestEcoStreak,
                this.matchEconomy.currentEcoStreak
            );
        } else if (teamEconomy.averageMoney < 3500) {
            winType = 'force';
            this.matchEconomy.totalForceWins++;
            this.matchEconomy.currentEcoStreak = 0;
        } else {
            this.matchEconomy.totalFullBuyWins++;
            this.matchEconomy.currentEcoStreak = 0;
        }

        teamEconomy.recordRoundWin(winType);

        return {
            winType,
            bonuses: teamEconomy.getAllPlayers().map(p => ({
                playerId: p.playerId,
                bonus: 3250
            }))
        };
    }

    getPlayerEconomy(playerId) {
        for (const team of this.teamEconomies.values()) {
            const player = team.getPlayer(playerId);
            if (player) return player;
        }
        return null;
    }

    getMatchEconomyStats() {
        return { ...this.matchEconomy };
    }

    export() {
        return {
            match_economy: this.matchEconomy,
            team_economies: Object.fromEntries(
                Array.from(this.teamEconomies.entries()).map(([k, v]) => [k, v.export()])
            )
        };
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
                console.error(`Error in economy manager listener: ${e}`);
            }
        }
    }
}

export const Economy = EconomyManager.getInstance();

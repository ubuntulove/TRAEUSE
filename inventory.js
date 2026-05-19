export class InventorySystem {
    constructor() {
        this.items = new Map();
        this.categories = new Map();
        this.itemDefinitions = new Map();
        this.loadouts = new Map();
        this.skins = new Map();
        this.stickers = new Map();
        this.patches = new Map();
        this.cases = new Map();
        this.keys = new Map();
        this.capsules = new Map();
        this.souvenirs = new Map();
        this.containers = new Map();
        this.quality = new QualitySystem();
        this.rarity = new RaritySystem();
        this.trading = new TradingSystem();
        this.marketplace = new MarketplaceIntegration();
    }

    initialize() {
        this.setupCategories();
        this.loadItemDefinitions();
    }

    setupCategories() {
        this.categories.set('weapons', {
            id: 'weapons',
            name: 'Weapons',
            description: 'All weapons and weapon skins',
            icon: '🔫',
            itemTypes: ['rifle', 'pistol', 'smg', 'shotgun', 'sniper', 'machinegun', 'knife', 'gloves']
        });

        this.categories.set('containers', {
            id: 'containers',
            name: 'Containers',
            description: 'Cases, boxes, and containers',
            icon: '📦',
            itemTypes: ['case', 'key', 'sticker_capsule', 'souvenir', 'package']
        });

        this.categories.set('collectibles', {
            id: 'collectibles',
            name: 'Collectibles',
            description: 'Collectible items',
            icon: '🏆',
            itemTypes: ['trophy', 'medal', 'coin', 'pin', 'patch']
        });

        this.categories.set('cosmetics', {
            id: 'cosmetics',
            name: 'Cosmetics',
            description: 'Cosmetic items',
            icon: '👔',
            itemTypes: ['agent', 'graffiti', 'spray', 'music', 'background', 'cursor']
        });
    }

    loadItemDefinitions() {
        this.defineWeapon('ak47', {
            name: 'AK-47',
            type: 'rifle',
            price: 2700,
            ammo: { magazine: 30, reserve: 90 },
            damage: 36,
            fireRate: 600,
            recoil: 0.8,
            accuracy: 0.7
        });

        this.defineWeapon('m4a4', {
            name: 'M4A4',
            type: 'rifle',
            price: 3100,
            ammo: { magazine: 30, reserve: 90 },
            damage: 33,
            fireRate: 600,
            recoil: 0.7,
            accuracy: 0.75
        });

        this.defineWeapon('awp', {
            name: 'AWP',
            type: 'sniper',
            price: 4750,
            ammo: { magazine: 10, reserve: 30 },
            damage: 115,
            fireRate: 41,
            recoil: 1.2,
            accuracy: 0.95
        });
    }

    defineWeapon(weaponId, definition) {
        const itemDef = {
            id: weaponId,
            ...definition,
            category: 'weapons',
            isUnique: false,
            tradable: true,
            marketable: true
        };

        this.itemDefinitions.set(weaponId, itemDef);
        return itemDef;
    }

    createItem(itemData) {
        const item = {
            id: 'item_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            defId: itemData.defId,
            name: itemData.name,
            type: itemData.type,
            quality: itemData.quality || 'consumer',
            rarity: itemData.rarity || 'common',
            wear: itemData.wear || null,
            float: itemData.float || null,
            stickers: itemData.stickers || [],
            origin: itemData.origin || 'purchased',
            purchaseDate: Date.now(),
            tradable: itemData.tradable ?? true,
            marketable: itemData.marketable ?? true,
            tradeHold: itemData.tradeHold || null,
            price: itemData.price || 0,
            customName: itemData.customName || null,
            customDescription: itemData.customDescription || null,
            paintSeed: itemData.paintSeed || null,
            paintKit: itemData.paintKit || null,
            statTrack: itemData.statTrack || false,
            killstreak: itemData.killstreak || 0,
            properties: {}
        };

        this.items.set(item.id, item);
        return item;
    }

    addItemToInventory(playerId, item) {
        const inventory = this.getOrCreateInventory(playerId);
        inventory.items.push(item.id);
        inventory.lastUpdated = Date.now();
        return item;
    }

    removeItemFromInventory(playerId, itemId) {
        const inventory = this.getOrCreateInventory(playerId);
        const index = inventory.items.indexOf(itemId);
        if (index !== -1) {
            inventory.items.splice(index, 1);
            inventory.lastUpdated = Date.now();
            return true;
        }
        return false;
    }

    getInventory(playerId) {
        const inventory = this.getOrCreateInventory(playerId);
        return {
            ...inventory,
            items: inventory.items.map(id => this.items.get(id)).filter(Boolean)
        };
    }

    getOrCreateInventory(playerId) {
        if (!this.inventories) {
            this.inventories = new Map();
        }

        if (!this.inventories.has(playerId)) {
            this.inventories.set(playerId, {
                playerId,
                items: [],
                slots: 1000,
                lastUpdated: Date.now()
            });
        }

        return this.inventories.get(playerId);
    }

    equipItem(playerId, itemId, slot) {
        const inventory = this.getOrCreateInventory(playerId);
        const item = this.items.get(itemId);

        if (!item) return { success: false, reason: 'Item not found' };
        if (!inventory.items.includes(itemId)) {
            return { success: false, reason: 'Item not in inventory' };
        }

        const loadout = this.getOrCreateLoadout(playerId);

        if (!loadout.equipped) {
            loadout.equipped = {};
        }

        loadout.equipped[slot] = itemId;
        loadout.lastUpdated = Date.now();

        return { success: true, loadout };
    }

    unequipItem(playerId, slot) {
        const loadout = this.getOrCreateLoadout(playerId);

        if (loadout.equipped && loadout.equipped[slot]) {
            delete loadout.equipped[slot];
            loadout.lastUpdated = Date.now();
            return { success: true };
        }

        return { success: false, reason: 'Nothing equipped in this slot' };
    }

    getLoadout(playerId) {
        const loadout = this.getOrCreateLoadout(playerId);
        const equipped = {};

        if (loadout.equipped) {
            for (const [slot, itemId] of Object.entries(loadout.equipped)) {
                equipped[slot] = this.items.get(itemId);
            }
        }

        return {
            ...loadout,
            equipped
        };
    }

    getOrCreateLoadout(playerId) {
        if (!this.loadouts) {
            this.loadouts = new Map();
        }

        if (!this.loadouts.has(playerId)) {
            this.loadouts.set(playerId, {
                playerId,
                equipped: {},
                presets: [],
                lastUpdated: Date.now()
            });
        }

        return this.loadouts.get(playerId);
    }

    saveLoadout(playerId, loadoutData) {
        const loadout = this.getOrCreateLoadout(playerId);
        const preset = {
            id: 'preset_' + Date.now(),
            name: loadoutData.name,
            equipped: { ...loadout.equipped },
            createdAt: Date.now()
        };

        if (!loadout.presets) {
            loadout.presets = [];
        }

        loadout.presets.push(preset);
        return preset;
    }

    applyLoadout(playerId, presetId) {
        const loadout = this.getOrCreateLoadout(playerId);
        const preset = loadout.presets?.find(p => p.id === presetId);

        if (!preset) {
            return { success: false, reason: 'Preset not found' };
        }

        loadout.equipped = { ...preset.equipped };
        loadout.lastUpdated = Date.now();

        return { success: true, loadout };
    }

    openCase(playerId, caseId, keys = []) {
        const item = this.items.get(caseId);
        if (!item) return { success: false, reason: 'Case not found' };

        const inventory = this.getOrCreateInventory(playerId);
        const caseIndex = inventory.items.indexOf(caseId);
        if (caseIndex === -1) {
            return { success: false, reason: 'Case not in inventory' };
        }

        const result = this.quality.rollQuality();
        const rarity = this.rarity.rollRarity();
        const possibleItems = this.getPossibleItemsFromCase(caseId, rarity);

        if (possibleItems.length === 0) {
            return { success: false, reason: 'No items available' };
        }

        const wonItem = possibleItems[Math.floor(Math.random() * possibleItems.length)];
        const newItem = this.createItem({
            defId: wonItem.defId,
            name: wonItem.name,
            type: wonItem.type,
            quality: result.quality,
            rarity: result.rarity,
            wear: result.wear,
            float: result.float
        });

        inventory.items.splice(caseIndex, 1);
        inventory.items.push(newItem.id);
        inventory.lastUpdated = Date.now();

        return {
            success: true,
            wonItem: newItem,
            caseId
        };
    }

    getPossibleItemsFromCase(caseId, rarity) {
        return Array.from(this.itemDefinitions.values())
            .filter(item => item.category === 'weapons');
    }

    tradeItems(playerId, tradeId, offeredItems, requestedItems) {
        const trade = this.trading.getTrade(tradeId);
        if (!trade) return { success: false, reason: 'Trade not found' };

        const offered = offeredItems.map(id => this.items.get(id)).filter(Boolean);
        const requested = requestedItems.map(id => this.items.get(id)).filter(Boolean);

        const validation = this.trading.validateTrade({
            offered,
            requested,
            playerId
        });

        if (!validation.valid) {
            return { success: false, reason: validation.reason };
        }

        for (const itemId of offeredItems) {
            this.removeItemFromInventory(playerId, itemId);
            this.trading.transferItemToPartner(tradeId, itemId, playerId);
        }

        for (const itemId of requestedItems) {
            const item = this.items.get(itemId);
            this.removeItemFromInventory(trade.partnerId, itemId);
            this.addItemToInventory(playerId, item);
        }

        return { success: true };
    }

    listOnMarketplace(itemId, price) {
        const item = this.items.get(itemId);
        if (!item) return { success: false, reason: 'Item not found' };

        if (!item.marketable) {
            return { success: false, reason: 'Item is not marketable' };
        }

        if (item.tradeHold && item.tradeHold > Date.now()) {
            return { success: false, reason: 'Item is in trade hold' };
        }

        const listing = this.marketplace.createListing({
            itemId,
            price,
            sellerId: item.ownerId,
            createdAt: Date.now()
        });

        item.listed = true;
        item.listingId = listing.id;

        return { success: true, listing };
    }

    removeFromMarketplace(itemId) {
        const item = this.items.get(itemId);
        if (!item) return { success: false, reason: 'Item not found' };

        this.marketplace.removeListing(item.listingId);
        item.listed = false;
        item.listingId = null;

        return { success: true };
    }

    getItemValue(itemId) {
        const item = this.items.get(itemId);
        if (!item) return null;

        return {
            item,
            suggestedPrice: this.calculateItemValue(item),
            lowestPrice: this.marketplace.getLowestPrice(item.defId),
            highestPrice: this.marketplace.getHighestPrice(item.defId),
            recentSales: this.marketplace.getRecentSales(item.defId)
        };
    }

    calculateItemValue(item) {
        let basePrice = item.price || 100;

        const rarityMultiplier = {
            'common': 1,
            'uncommon': 2,
            'rare': 5,
            'epic': 15,
            'legendary': 50
        };

        basePrice *= rarityMultiplier[item.rarity] || 1;

        const qualityMultiplier = {
            'consumer': 0.5,
            'industrial': 0.75,
            'mil-spec': 1,
            'restricted': 2,
            'classified': 5,
            'covert': 15,
            'contraband': 100
        };

        basePrice *= qualityMultiplier[item.quality] || 1;

        if (item.wear) {
            const wearMultipliers = {
                'factory_new': 1.5,
                'minimal_wear': 1.2,
                'field_tested': 1,
                'well_worn': 0.8,
                'battle_scarred': 0.6
            };
            basePrice *= wearMultipliers[item.wear] || 1;
        }

        if (item.float !== null) {
            if (item.float < 0.07) basePrice *= 1.5;
            else if (item.float > 0.8) basePrice *= 0.5;
        }

        if (item.stickers && item.stickers.length > 0) {
            basePrice *= 1 + (item.stickers.length * 0.1);
        }

        if (item.statTrack) {
            basePrice *= 1.3;
        }

        if (item.killstreak > 0) {
            basePrice *= 1 + (item.killstreak * 0.2);
        }

        return Math.round(basePrice);
    }

    getItemsByType(type) {
        return Array.from(this.items.values())
            .filter(item => item.type === type);
    }

    getItemsByRarity(rarity) {
        return Array.from(this.items.values())
            .filter(item => item.rarity === rarity);
    }

    getItemsByQuality(quality) {
        return Array.from(this.items.values())
            .filter(item => item.quality === quality);
    }

    getTotalInventoryValue(playerId) {
        const inventory = this.getInventory(playerId);
        let total = 0;

        for (const item of inventory.items) {
            total += this.calculateItemValue(item);
        }

        return total;
    }

    serialize() {
        return {
            items: Array.from(this.items.entries()),
            inventories: this.inventories ? Array.from(this.inventories.entries()) : [],
            loadouts: Array.from(this.loadouts.entries()),
            categories: Array.from(this.categories.entries()),
            itemDefinitions: Array.from(this.itemDefinitions.entries())
        };
    }
}

export class QualitySystem {
    constructor() {
        this.qualities = [
            { name: 'consumer', weight: 79.92, color: '#b0c3d9' },
            { name: 'industrial', weight: 15.98, color: '#5e98d9' },
            { name: 'mil-spec', weight: 3.20, color: '#4b69ff' },
            { name: 'restricted', weight: 0.64, color: '#8847ff' },
            { name: 'classified', weight: 0.16, color: '#d32ce6' },
            { name: 'covert', weight: 0.08, color: '#eb4b4b' },
            { name: 'contraband', weight: 0.02, color: '#e4ae39' }
        ];
    }

    rollQuality() {
        const roll = Math.random() * 100;
        let cumulative = 0;

        for (const quality of this.qualities) {
            cumulative += quality.weight;
            if (roll <= cumulative) {
                return {
                    quality: quality.name,
                    color: quality.color
                };
            }
        }

        return this.qualities[0];
    }
}

export class RaritySystem {
    constructor() {
        this.rarities = [
            { name: 'common', weight: 60, color: '#b0c3d9' },
            { name: 'uncommon', weight: 25, color: '#5e98d9' },
            { name: 'rare', weight: 10, color: '#8847ff' },
            { name: 'epic', weight: 4, color: '#d32ce6' },
            { name: 'legendary', weight: 1, color: '#e4ae39' }
        ];
    }

    rollRarity() {
        const roll = Math.random() * 100;
        let cumulative = 0;

        for (const rarity of this.rarities) {
            cumulative += rarity.weight;
            if (roll <= cumulative) {
                return {
                    rarity: rarity.name,
                    color: rarity.color
                };
            }
        }

        return this.rarities[0];
    }
}

export class WearSystem {
    constructor() {
        this.wears = [
            { name: 'factory_new', min: 0.00, max: 0.07, abbreviation: 'FN' },
            { name: 'minimal_wear', min: 0.07, max: 0.15, abbreviation: 'MW' },
            { name: 'field_tested', min: 0.15, max: 0.38, abbreviation: 'FT' },
            { name: 'well_worn', min: 0.38, max: 0.45, abbreviation: 'WW' },
            { name: 'battle_scarred', min: 0.45, max: 1.00, abbreviation: 'BS' }
        ];
    }

    rollWear() {
        const wear = Math.random();
        for (const w of this.wears) {
            if (wear >= w.min && wear < w.max) {
                return {
                    wear: w.name,
                    float: wear,
                    abbreviation: w.abbreviation
                };
            }
        }
        return { wear: 'factory_new', float: 0, abbreviation: 'FN' };
    }
}

export class TradingSystem {
    constructor() {
        this.trades = new Map();
        this.pendingTrades = new Map();
        this.tradeHistory = [];
        this.completedTrades = 0;
    }

    createTrade(senderId, receiverId, offeredItems, requestedItems) {
        const trade = {
            id: 'trade_' + Date.now(),
            senderId,
            receiverId,
            offeredItems,
            requestedItems,
            status: 'pending',
            createdAt: Date.now(),
            expiresAt: Date.now() + 5 * 60 * 1000,
            messages: [],
            lastUpdated: Date.now()
        };

        this.trades.set(trade.id, trade);
        this.pendingTrades.set(trade.id, trade);

        return trade;
    }

    acceptTrade(tradeId, acceptorId) {
        const trade = this.trades.get(tradeId);
        if (!trade) return { success: false, reason: 'Trade not found' };

        if (trade.status !== 'pending') {
            return { success: false, reason: 'Trade is not pending' };
        }

        if (trade.receiverId !== acceptorId) {
            return { success: false, reason: 'Not authorized to accept' };
        }

        trade.status = 'accepted';
        trade.acceptedAt = Date.now();
        this.pendingTrades.delete(tradeId);

        return { success: true, trade };
    }

    declineTrade(tradeId, declinerId) {
        const trade = this.trades.get(tradeId);
        if (!trade) return { success: false };

        trade.status = 'declined';
        trade.declinedAt = Date.now();
        this.pendingTrades.delete(tradeId);

        return { success: true };
    }

    cancelTrade(tradeId, cancellerId) {
        const trade = this.trades.get(tradeId);
        if (!trade) return { success: false };

        if (trade.senderId !== cancellerId) {
            return { success: false, reason: 'Not authorized to cancel' };
        }

        trade.status = 'cancelled';
        trade.cancelledAt = Date.now();
        this.pendingTrades.delete(tradeId);

        return { success: true };
    }

    validateTrade(tradeData) {
        const { offered, requested, playerId } = tradeData;

        if (offered.length === 0) {
            return { valid: false, reason: 'Must offer at least one item' };
        }

        for (const item of offered) {
            if (!item.tradable) {
                return { valid: false, reason: `${item.name} is not tradable` };
            }
            if (item.tradeHold && item.tradeHold > Date.now()) {
                return { valid: false, reason: `${item.name} is in trade hold` };
            }
        }

        for (const item of requested) {
            if (!item.tradable) {
                return { valid: false, reason: `${item.name} is not tradable` };
            }
        }

        return { valid: true };
    }

    transferItemToPartner(tradeId, itemId, senderId) {
        const trade = this.trades.get(tradeId);
        if (!trade) return false;

        return true;
    }

    getTrade(tradeId) {
        return this.trades.get(tradeId);
    }

    getPendingTrades(userId) {
        return Array.from(this.pendingTrades.values())
            .filter(t => t.receiverId === userId || t.senderId === userId);
    }

    getTradeHistory(userId) {
        return this.tradeHistory.filter(t => 
            t.senderId === userId || t.receiverId === userId
        );
    }
}

export class MarketplaceIntegration {
    constructor() {
        this.listings = new Map();
        this.priceHistory = new Map();
        this.volumeHistory = new Map();
        this.fees = {
            sell: 0.15,
            buy: 0.05
        };
    }

    createListing(listingData) {
        const listing = {
            id: 'listing_' + Date.now(),
            itemId: listingData.itemId,
            price: listingData.price,
            sellerId: listingData.sellerId,
            status: 'active',
            createdAt: Date.now(),
            views: 0,
            watchers: []
        };

        this.listings.set(listing.id, listing);
        return listing;
    }

    removeListing(listingId) {
        const listing = this.listings.get(listingId);
        if (listing) {
            listing.status = 'removed';
            return true;
        }
        return false;
    }

    purchaseListing(listingId, buyerId) {
        const listing = this.listings.get(listingId);
        if (!listing || listing.status !== 'active') {
            return { success: false, reason: 'Listing not available' };
        }

        listing.status = 'sold';
        listing.soldTo = buyerId;
        listing.soldAt = Date.now();

        const sale = {
            listingId,
            itemId: listing.itemId,
            price: listing.price,
            sellerId: listing.sellerId,
            buyerId,
            soldAt: Date.now()
        };

        this.recordSale(sale);

        return { success: true, sale };
    }

    recordSale(sale) {
        const key = sale.itemId;

        if (!this.priceHistory.has(key)) {
            this.priceHistory.set(key, []);
        }

        this.priceHistory.get(key).push({
            price: sale.price,
            timestamp: sale.soldAt
        });

        if (!this.volumeHistory.has(key)) {
            this.volumeHistory.set(key, new Map());
        }

        const volume = this.volumeHistory.get(key);
        const day = new Date(sale.soldAt).toDateString();
        volume.set(day, (volume.get(day) || 0) + 1);
    }

    getLowestPrice(defId) {
        const listings = Array.from(this.listings.values())
            .filter(l => l.itemId === defId && l.status === 'active');

        if (listings.length === 0) return null;

        return Math.min(...listings.map(l => l.price));
    }

    getHighestPrice(defId) {
        const listings = Array.from(this.listings.values())
            .filter(l => l.itemId === defId && l.status === 'active');

        if (listings.length === 0) return null;

        return Math.max(...listings.map(l => l.price));
    }

    getRecentSales(defId, limit = 10) {
        const history = this.priceHistory.get(defId) || [];
        return history.slice(-limit);
    }

    getAveragePrice(defId, days = 7) {
        const history = this.priceHistory.get(defId) || [];
        const cutoff = Date.now() - days * 24 * 60 * 60 * 1000;
        const recent = history.filter(h => h.timestamp > cutoff);

        if (recent.length === 0) return null;

        const sum = recent.reduce((acc, h) => acc + h.price, 0);
        return sum / recent.length;
    }

    calculateFees(price, type = 'sell') {
        const feeRate = this.fees[type];
        const fee = price * feeRate;

        return {
            price,
            fee,
            net: price - fee
        };
    }
}

export default {
    InventorySystem,
    QualitySystem,
    RaritySystem,
    WearSystem,
    TradingSystem,
    MarketplaceIntegration
};

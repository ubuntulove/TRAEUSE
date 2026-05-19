export class Marketplace {
    constructor(config = {}) {
        this.id = config.id || Marketplace.nextId++;
        this.name = config.name || 'Marketplace';
        this.items = new Map();
        this.listings = new Map();
        this.transactions = [];
        this.salesHistory = [];
        this.categories = this.initializeCategories();
        this.pricingEngine = new PricingEngine();
        this.listingFee = config.listingFee || 0.05;
        this.transactionFee = config.transactionFee || 0.10;
        this.minPrice = config.minPrice || 0.50;
        this.maxPrice = config.maxPrice || 100000;
        this.currency = config.currency || 'USD';
        this.taxRate = config.taxRate || 0;
        this.isEnabled = config.enabled !== undefined ? config.enabled : true;
        this.maintenanceMode = false;
        this.supportedCurrencies = ['USD', 'EUR', 'GBP', 'JPY', 'CNY', 'KRW', 'RUB', 'BRL'];
        this.exchangeRates = new Map();
        this.priceHistory = new Map();
        this.onSale = null;
        this.onPurchase = null;
        this.onListingCreated = null;
        this.onListingRemoved = null;
        this.loadItems();
    }

    static get nextId() {
        return Marketplace._nextId++;
    }

    initializeCategories() {
        return {
            weapons: {
                name: 'Weapons',
                icon: 'weapon_icon',
                subcategories: {
                    pistols: { name: 'Pistols' },
                    smgs: { name: 'SMGs' },
                    rifles: { name: 'Rifles' },
                    snipers: { name: 'Snipers' },
                    shotguns: { name: 'Shotguns' },
                    machineguns: { name: 'Machine Guns' },
                    knives: { name: 'Knives' },
                    gloves: { name: 'Gloves' }
                }
            },
            skins: {
                name: 'Weapon Skins',
                icon: 'skin_icon',
                subcategories: {
                    common: { name: 'Consumer Grade' },
                    industrial: { name: 'Industrial Grade' },
                    milspec: { name: 'Mil-Spec' },
                    restricted: { name: 'Restricted' },
                    classified: { name: 'Classified' },
                    covert: { name: 'Covert' },
                    contraband: { name: 'Contraband' }
                }
            },
            cases: {
                name: 'Cases',
                icon: 'case_icon',
                subcategories: {
                    standard: { name: 'Standard Cases' },
                    rare: { name: 'Rare Special Items' },
                    souvenirs: { name: 'Souvenir Packages' }
                }
            },
            stickers: {
                name: 'Stickers',
                icon: 'sticker_icon',
                subcategories: {
                    regular: { name: 'Regular Stickers' },
                    holo: { name: 'Holo-Foil' },
                    foil: { name: 'Foil' },
                    gold: { name: 'Gold' }
                }
            },
            music: {
                name: 'Music Kits',
                icon: 'music_icon'
            },
            patches: {
                name: 'Patches',
                icon: 'patch_icon'
            },
            collectibles: {
                name: 'Collectibles',
                icon: 'collectible_icon',
                subcategories: {
                    medals: { name: 'Medals' },
                    trophies: { name: 'Trophies' },
                    coins: { name: 'Coins' }
                }
            }
        };
    }

    loadItems() {
        const defaultItems = this.getDefaultItems();
        for (const item of defaultItems) {
            this.addItem(item);
        }
    }

    getDefaultItems() {
        return [
            {
                id: 'ak47_redline',
                name: 'AK-47 | Redline',
                type: 'skin',
                rarity: 'classified',
                weapon: 'ak47',
                wear: 'field-tested',
                price: 15.50,
                float: 0.15,
                imageUrl: '/images/skins/ak47_redline.png'
            },
            {
                id: 'awp_dragon_lore',
                name: 'AWP | Dragon Lore',
                type: 'skin',
                rarity: 'covert',
                weapon: 'awp',
                wear: 'factory_new',
                price: 1500.00,
                float: 0.02,
                imageUrl: '/images/skins/awp_dragon_lore.png'
            },
            {
                id: 'knife_fade',
                name: 'Butterfly Knife | Fade',
                type: 'skin',
                rarity: 'contraband',
                weapon: 'knife',
                wear: 'factory_new',
                price: 2500.00,
                float: 0.01,
                imageUrl: '/images/skins/knife_fade.png'
            },
            {
                id: 'glove_tiger_phantom',
                name: 'Sport Gloves | Tiger Phantom',
                type: 'skin',
                rarity: 'covert',
                weapon: 'gloves',
                wear: 'field-tested',
                price: 800.00,
                float: 0.18,
                imageUrl: '/images/skins/glove_tiger_phantom.png'
            }
        ];
    }

    addItem(item) {
        const itemData = {
            id: item.id,
            name: item.name,
            type: item.type,
            rarity: item.rarity,
            weapon: item.weapon || null,
            collection: item.collection || null,
            exterior: item.wear || 'any',
            price: item.price,
            float: item.float || null,
            minFloat: item.minFloat || 0,
            maxFloat: item.maxFloat || 1,
            imageUrl: item.imageUrl || '',
            inspectUrl: item.inspectUrl || '',
            description: item.description || '',
            stickers: item.stickers || [],
            stickersFloat: item.stickersFloat || 0,
            marketHashName: item.marketHashName || item.name,
            tradable: item.tradable !== undefined ? item.tradable : true,
            craftable: item.craftable !== undefined ? item.craftable : true,
            sellable: item.sellable !== undefined ? item.sellable : true,
            searchable: true,
            tags: item.tags || [],
            createdAt: item.createdAt || Date.now(),
            updatedAt: Date.now()
        };

        this.items.set(item.id, itemData);
        return itemData;
    }

    createListing(config) {
        const item = this.items.get(config.itemId);
        if (!item) {
            return { success: false, reason: 'item_not_found' };
        }

        if (item.sellable === false) {
            return { success: false, reason: 'item_not_sellable' };
        }

        const price = config.price;
        if (price < this.minPrice || price > this.maxPrice) {
            return { success: false, reason: 'invalid_price' };
        }

        const listing = {
            id: `listing_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
            itemId: config.itemId,
            sellerId: config.sellerId,
            price,
            currency: config.currency || this.currency,
            fees: this.calculateFees(price),
            netPrice: price - this.calculateFees(price),
            createdAt: Date.now(),
            expiresAt: config.expiresAt || Date.now() + 30 * 24 * 60 * 60 * 1000,
            status: 'active',
            views: 0,
            favorites: 0,
            offers: [],
            notes: config.notes || '',
            hideFromListings: config.hideFromListings || false,
            autoAcceptOffers: config.autoAcceptOffers || false,
            minOfferPrice: config.minOfferPrice || null
        };

        this.listings.set(listing.id, listing);

        if (this.onListingCreated) {
            this.onListingCreated(listing);
        }

        return { success: true, listing };
    }

    calculateFees(price) {
        const listingFee = price * this.listingFee;
        const transactionFee = price * this.transactionFee;
        return {
            listingFee,
            transactionFee,
            total: listingFee + transactionFee
        };
    }

    removeListing(listingId, sellerId) {
        const listing = this.listings.get(listingId);
        if (!listing) {
            return { success: false, reason: 'listing_not_found' };
        }

        if (listing.sellerId !== sellerId) {
            return { success: false, reason: 'not_owner' };
        }

        if (listing.status !== 'active') {
            return { success: false, reason: 'listing_not_active' };
        }

        listing.status = 'removed';
        listing.removedAt = Date.now();

        if (this.onListingRemoved) {
            this.onListingRemoved(listing);
        }

        return { success: true };
    }

    purchaseListing(listingId, buyerId, paymentMethod = 'wallet') {
        const listing = this.listings.get(listingId);
        if (!listing) {
            return { success: false, reason: 'listing_not_found' };
        }

        if (listing.status !== 'active') {
            return { success: false, reason: 'listing_not_available' };
        }

        if (listing.sellerId === buyerId) {
            return { success: false, reason: 'cannot_buy_own_listing' };
        }

        if (Date.now() > listing.expiresAt) {
            listing.status = 'expired';
            return { success: false, reason: 'listing_expired' };
        }

        const item = this.items.get(listing.itemId);
        const fees = listing.fees;
        const sellerPayout = listing.price - fees.total;

        const transaction = {
            id: `txn_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
            type: 'sale',
            listingId: listing.id,
            itemId: listing.itemId,
            buyerId,
            sellerId: listing.sellerId,
            price: listing.price,
            fees,
            sellerPayout,
            paymentMethod,
            currency: listing.currency,
            status: 'pending',
            createdAt: Date.now(),
            completedAt: null,
            steamTransactionId: null
        };

        listing.status = 'sold';
        listing.soldAt = Date.now();
        listing.buyerId = buyerId;

        this.transactions.push(transaction);
        this.salesHistory.push(transaction);
        this.updatePriceHistory(listing.itemId, listing.price);

        if (this.onPurchase) {
            this.onPurchase(transaction);
        }

        return { success: true, transaction };
    }

    createOffer(listingId, buyerId, price) {
        const listing = this.listings.get(listingId);
        if (!listing) {
            return { success: false, reason: 'listing_not_found' };
        }

        if (listing.sellerId === buyerId) {
            return { success: false, reason: 'cannot_offer_own' };
        }

        if (listing.minOfferPrice && price < listing.minOfferPrice) {
            return { success: false, reason: 'below_minimum_offer' };
        }

        const offer = {
            id: `offer_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
            listingId,
            buyerId,
            price,
            status: 'pending',
            createdAt: Date.now(),
            expiresAt: Date.now() + 24 * 60 * 60 * 1000,
            message: '',
            isAutoBid: false
        };

        listing.offers.push(offer);

        if (listing.autoAcceptOffers && price >= listing.minOfferPrice) {
            return this.acceptOffer(offer.id, listing.sellerId);
        }

        return { success: true, offer };
    }

    acceptOffer(offerId, sellerId) {
        let targetOffer = null;
        let targetListing = null;

        for (const [listingId, listing] of this.listings) {
            const offer = listing.offers.find(o => o.id === offerId);
            if (offer) {
                targetOffer = offer;
                targetListing = listing;
                break;
            }
        }

        if (!targetOffer) {
            return { success: false, reason: 'offer_not_found' };
        }

        if (targetListing.sellerId !== sellerId) {
            return { success: false, reason: 'not_owner' };
        }

        targetOffer.status = 'accepted';
        targetOffer.acceptedAt = Date.now();

        for (const offer of targetListing.offers) {
            if (offer.id !== offerId) {
                offer.status = 'rejected';
            }
        }

        return this.purchaseListing(targetListing.id, targetOffer.buyerId);
    }

    rejectOffer(offerId, sellerId) {
        for (const [listingId, listing] of this.listings) {
            const offer = listing.offers.find(o => o.id === offerId);
            if (offer) {
                if (listing.sellerId !== sellerId) {
                    return { success: false, reason: 'not_owner' };
                }

                offer.status = 'rejected';
                offer.rejectedAt = Date.now();
                return { success: true };
            }
        }

        return { success: false, reason: 'offer_not_found' };
    }

    searchItems(query, filters = {}) {
        const results = [];

        for (const [id, item] of this.items) {
            if (!item.searchable) continue;

            let score = 0;
            const queryLower = query.toLowerCase();

            if (item.name.toLowerCase().includes(queryLower)) {
                score += 100;
            }
            if (item.marketHashName.toLowerCase().includes(queryLower)) {
                score += 80;
            }
            if (item.description?.toLowerCase().includes(queryLower)) {
                score += 30;
            }

            for (const tag of item.tags) {
                if (tag.toLowerCase().includes(queryLower)) {
                    score += 20;
                }
            }

            if (score === 0) continue;

            if (filters.type && item.type !== filters.type) continue;
            if (filters.rarity && item.rarity !== filters.rarity) continue;
            if (filters.weapon && item.weapon !== filters.weapon) continue;
            if (filters.exterior && item.exterior !== filters.exterior) continue;

            results.push({ item, score });
        }

        results.sort((a, b) => b.score - a.score);

        return results.map(r => r.item);
    }

    getListingsByItem(itemId, filters = {}) {
        const listings = [];

        for (const listing of this.listings.values()) {
            if (listing.itemId !== itemId) continue;
            if (listing.status !== 'active') continue;

            listings.push(listing);
        }

        if (filters.sortBy) {
            listings.sort((a, b) => {
                switch (filters.sortBy) {
                    case 'price_asc':
                        return a.price - b.price;
                    case 'price_desc':
                        return b.price - a.price;
                    case 'newest':
                        return b.createdAt - a.createdAt;
                    case 'oldest':
                        return a.createdAt - b.createdAt;
                    default:
                        return 0;
                }
            });
        }

        return listings;
    }

    getPriceHistory(itemId, days = 30) {
        const cutoff = Date.now() - days * 24 * 60 * 60 * 1000;

        const sales = this.salesHistory.filter(
            s => s.itemId === itemId && s.createdAt >= cutoff
        );

        const pricePoints = sales.map(s => ({
            price: s.price,
            timestamp: s.createdAt
        }));

        return {
            itemId,
            days,
            dataPoints: pricePoints,
            average: this.calculateAverage(pricePoints.map(p => p.price)),
            min: Math.min(...pricePoints.map(p => p.price)),
            max: Math.max(...pricePoints.map(p => p.price)),
            volume: sales.length
        };
    }

    updatePriceHistory(itemId, price) {
        if (!this.priceHistory.has(itemId)) {
            this.priceHistory.set(itemId, []);
        }

        const history = this.priceHistory.get(itemId);
        history.push({
            price,
            timestamp: Date.now()
        });

        if (history.length > 1000) {
            history.shift();
        }
    }

    calculateAverage(prices) {
        if (prices.length === 0) return 0;
        return prices.reduce((sum, price) => sum + price, 0) / prices.length;
    }

    getRecommendedPrice(itemId) {
        const item = this.items.get(itemId);
        if (!item) return null;

        const listings = this.getListingsByItem(itemId);
        if (listings.length === 0) {
            return this.pricingEngine.estimatePrice(item);
        }

        const prices = listings.map(l => l.price);
        return {
            suggested: this.calculateAverage(prices),
            lowest: Math.min(...prices),
            highest: Math.max(...prices),
            median: this.getMedian(prices),
            volume: listings.length
        };
    }

    getMedian(values) {
        const sorted = [...values].sort((a, b) => a - b);
        const mid = Math.floor(sorted.length / 2);
        return sorted.length % 2 !== 0
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2;
    }

    getPopularItems(limit = 20, timeRange = '7d') {
        const cutoff = this.getTimeRangeCutoff(timeRange);

        const itemPopularity = new Map();

        for (const sale of this.salesHistory) {
            if (sale.createdAt < cutoff) continue;

            const count = itemPopularity.get(sale.itemId) || { count: 0, volume: 0, revenue: 0 };
            count.count++;
            count.volume++;
            count.revenue += sale.price;
            itemPopularity.set(sale.itemId, count);
        }

        for (const listing of this.listings.values()) {
            if (listing.status !== 'active') continue;

            const count = itemPopularity.get(listing.itemId) || { count: 0, volume: 0, revenue: 0 };
            count.count++;
            itemPopularity.set(listing.itemId, count);
        }

        const sorted = Array.from(itemPopularity.entries())
            .sort((a, b) => b[1].count - a[1].count)
            .slice(0, limit);

        return sorted.map(([itemId, stats]) => ({
            item: this.items.get(itemId),
            ...stats
        }));
    }

    getTimeRangeCutoff(range) {
        const now = Date.now();
        switch (range) {
            case '24h': return now - 24 * 60 * 60 * 1000;
            case '7d': return now - 7 * 24 * 60 * 60 * 1000;
            case '30d': return now - 30 * 24 * 60 * 60 * 1000;
            case 'all': return 0;
            default: return now - 7 * 24 * 60 * 60 * 1000;
        }
    }

    getTrendingItems(limit = 20) {
        const recentCutoff = Date.now() - 24 * 60 * 60 * 1000;
        const olderCutoff = Date.now() - 7 * 24 * 60 * 60 * 1000;

        const recentSales = new Map();
        const olderSales = new Map();

        for (const sale of this.salesHistory) {
            if (sale.createdAt >= recentCutoff) {
                recentSales.set(sale.itemId, (recentSales.get(sale.itemId) || 0) + 1);
            } else if (sale.createdAt >= olderCutoff) {
                olderSales.set(sale.itemId, (olderSales.get(sale.itemId) || 0) + 1);
            }
        }

        const trending = [];

        for (const [itemId, recentCount] of recentSales) {
            const olderCount = olderSales.get(itemId) || 1;
            const trendScore = recentCount / olderCount;

            trending.push({
                item: this.items.get(itemId),
                trendScore,
                recentVolume: recentCount,
                olderVolume: olderCount
            });
        }

        return trending
            .sort((a, b) => b.trendScore - a.trendScore)
            .slice(0, limit);
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            listings: Array.from(this.listings.entries()),
            transactions: this.transactions.slice(-100),
            salesHistory: this.salesHistory.slice(-1000),
            pricingEngine: this.pricingEngine.toJSON()
        };
    }
}

Marketplace._nextId = 0;

export class PricingEngine {
    constructor(config = {}) {
        this.basePrices = new Map();
        this.rarityMultipliers = {
            common: 1,
            industrial: 1.5,
            milspec: 3,
            restricted: 5,
            classified: 10,
            covert: 20,
            contraband: 100
        };
        this.wearMultipliers = {
            factory_new: 2.0,
            minimal_wear: 1.5,
            field_tested: 1.0,
            well_worn: 0.75,
            battle_scarred: 0.5
        };
        this.stickerMultiplier = 1.1;
        this.collectionDemand = new Map();
        this.updateInterval = config.updateInterval || 60 * 60 * 1000;
        this.lastUpdate = Date.now();
        this.algorithm = config.algorithm || 'weighted_average';
    }

    estimatePrice(item) {
        const basePrice = this.basePrices.get(item.weapon || item.type) || 1;
        const rarityMult = this.rarityMultipliers[item.rarity] || 1;
        const wearMult = this.wearMultipliers[item.wear] || 1;

        let price = basePrice * rarityMult * wearMult;

        if (item.stickers && item.stickers.length > 0) {
            price *= Math.pow(this.stickerMultiplier, item.stickers.length);
        }

        if (item.float !== null) {
            const floatAdjustment = this.calculateFloatAdjustment(item.float, item.rarity);
            price *= floatAdjustment;
        }

        if (item.collection && this.collectionDemand.has(item.collection)) {
            const demandBoost = this.collectionDemand.get(item.collection);
            price *= (1 + demandBoost * 0.5);
        }

        return Math.round(price * 100) / 100;
    }

    calculateFloatAdjustment(float, rarity) {
        let adjustment = 1;

        if (float < 0.07) {
            adjustment = 1 + (0.07 - float) * 5;
        } else if (float > 0.37) {
            adjustment = 1 - (float - 0.37) * 1.5;
        }

        if (rarity === 'contraband') {
            adjustment *= 2;
        }

        return Math.max(0.1, adjustment);
    }

    updatePrices(marketplace) {
        for (const [itemId, item] of marketplace.items) {
            const listings = marketplace.getListingsByItem(itemId);

            if (listings.length > 0) {
                const prices = listings.map(l => l.price);
                const avgPrice = prices.reduce((a, b) => a + b, 0) / prices.length;

                const history = marketplace.getPriceHistory(itemId, 7);
                if (history.dataPoints.length > 0) {
                    const historicalAvg = history.average;
                    const combinedPrice = this.combinePrices(avgPrice, historicalAvg);
                    this.basePrices.set(item.weapon || item.type, combinedPrice);
                } else {
                    this.basePrices.set(item.weapon || item.type, avgPrice);
                }
            }
        }

        this.updateCollectionDemand(marketplace);
        this.lastUpdate = Date.now();
    }

    combinePrices(current, historical) {
        switch (this.algorithm) {
            case 'simple_average':
                return (current + historical) / 2;
            case 'weighted_average':
                return current * 0.7 + historical * 0.3;
            case 'recent_only':
                return current;
            case 'stable':
                return historical * 0.9 + current * 0.1;
            default:
                return (current + historical) / 2;
        }
    }

    updateCollectionDemand(marketplace) {
        const collectionStats = new Map();

        for (const [itemId, item] of marketplace.items) {
            if (!item.collection) continue;

            const stats = collectionStats.get(item.collection) || { count: 0, totalPrice: 0 };
            const listings = marketplace.getListingsByItem(itemId);
            const avgPrice = listings.length > 0
                ? listings.reduce((a, b) => a + b.price, 0) / listings.length
                : 0;

            stats.count++;
            stats.totalPrice += avgPrice;
            collectionStats.set(item.collection, stats);
        }

        const avgAllPrices = Array.from(collectionStats.values())
            .map(s => s.totalPrice / s.count)
            .reduce((a, b) => a + b, 0) / collectionStats.size;

        for (const [collection, stats] of collectionStats) {
            const collectionAvg = stats.totalPrice / stats.count;
            const demand = Math.log10(collectionAvg / avgAllPrices + 1);
            this.collectionDemand.set(collection, demand);
        }
    }

    predictPrice(itemId, daysAhead = 7) {
        const item = this.items?.get(itemId);
        if (!item) return null;

        const history = this.priceHistory?.get(itemId);
        if (!history || history.length < 2) {
            return this.estimatePrice(item);
        }

        const recentPrices = history.slice(-30);
        const trend = this.calculateTrend(recentPrices);

        const currentPrice = recentPrices[recentPrices.length - 1].price;
        const predictedPrice = currentPrice * Math.pow(1 + trend, daysAhead / 7);

        return {
            current: currentPrice,
            predicted: Math.max(0, predictedPrice),
            trend: trend * 100,
            confidence: this.calculateConfidence(recentPrices)
        };
    }

    calculateTrend(prices) {
        if (prices.length < 2) return 0;

        const xSum = 0;
        const ySum = prices.reduce((a, b) => a + b.price, 0);
        const xySum = prices.reduce((a, b, i) => a + i * b.price, 0);
        const xxSum = prices.reduce((a, _, i) => a + i * i, 0);
        const n = prices.length;

        const slope = (n * xySum - xSum * ySum) / (n * xxSum - xSum * xSum);
        const avgPrice = ySum / n;

        return slope / avgPrice;
    }

    calculateConfidence(prices) {
        if (prices.length < 5) return 0.3;
        if (prices.length < 10) return 0.5;
        if (prices.length < 30) return 0.7;
        return 0.9;
    }

    setBasePrice(weapon, price) {
        this.basePrices.set(weapon, price);
    }

    setRarityMultiplier(rarity, multiplier) {
        this.rarityMultipliers[rarity] = multiplier;
    }

    setWearMultiplier(wear, multiplier) {
        this.wearMultipliers[wear] = multiplier;
    }

    toJSON() {
        return {
            basePrices: Array.from(this.basePrices.entries()),
            rarityMultipliers: this.rarityMultipliers,
            wearMultipliers: this.wearMultipliers,
            collectionDemand: Array.from(this.collectionDemand.entries()),
            algorithm: this.algorithm
        };
    }

    fromJSON(json) {
        this.basePrices = new Map(json.basePrices || []);
        this.rarityMultipliers = json.rarityMultipliers || this.rarityMultipliers;
        this.wearMultipliers = json.wearMultipliers || this.wearMultipliers;
        this.collectionDemand = new Map(json.collectionDemand || []);
        this.algorithm = json.algorithm || 'weighted_average';
    }
}

export class CaseOpening {
    constructor(config = {}) {
        this.id = config.id || CaseOpening.nextId++;
        this.caseId = config.caseId;
        this.caseName = config.caseName || 'Case';
        this.items = config.items || [];
        this.isOpen = false;
        this.openedAt = null;
        this.result = null;
        this.rolls = [];
        this.animationPhase = 'closed';
        this.totalValue = 0;
        this.calculateTotalValue();
    }

    static get nextId() {
        return CaseOpening._nextId++;
    }

    calculateTotalValue() {
        this.totalValue = this.items.reduce((sum, item) => sum + item.price, 0);
    }

    getItems() {
        return this.items.map(item => ({
            ...item,
            weight: this.calculateWeight(item)
        }));
    }

    calculateWeight(item) {
        const rarityWeights = {
            common: 0.7992,
            industrial: 0.1598,
            milspec: 0.0320,
            restricted: 0.0064,
            classified: 0.0020,
            covert: 0.0006,
            contraband: 0.00001
        };

        const baseWeight = rarityWeights[item.rarity] || 0.1;
        const exteriorWeight = item.exterior === 'factory_new' ? 1.5 : 1;
        const stickerBonus = (item.stickers?.length || 0) * 0.05;

        return baseWeight * exteriorWeight * (1 + stickerBonus);
    }

    open() {
        if (this.isOpen) {
            return { success: false, reason: 'already_opened' };
        }

        this.isOpen = true;
        this.openedAt = Date.now();
        this.animationPhase = 'rolling';

        const result = this.rollForItem();
        this.result = result.item;
        this.rolls = this.performRolls();

        return {
            success: true,
            result: this.result,
            rolls: this.rolls,
            isNew: result.isNew,
            isDuplicate: result.isDuplicate
        };
    }

    rollForItem() {
        const items = this.getItems();
        const totalWeight = items.reduce((sum, item) => sum + item.weight, 0);

        let random = Math.random() * totalWeight;
        let selectedItem = items[0];

        for (const item of items) {
            random -= item.weight;
            if (random <= 0) {
                selectedItem = item;
                break;
            }
        }

        return {
            item: selectedItem,
            isNew: Math.random() < 0.1,
            isDuplicate: Math.random() < 0.5
        };
    }

    performRolls() {
        const rolls = [];
        const numRolls = 20 + Math.floor(Math.random() * 10);

        const sortedByRarity = [...this.items].sort((a, b) =>
            (this.calculateWeight(a) - this.calculateWeight(b))
        );

        for (let i = 0; i < numRolls; i++) {
            const progress = i / numRolls;
            const speed = 1 - Math.pow(progress, 2);
            const itemIndex = Math.floor(Math.random() * sortedByRarity.length * speed);
            rolls.push(sortedByRarity[Math.min(itemIndex, sortedByRarity.length - 1)]);
        }

        rolls.push(this.result);
        return rolls;
    }

    completeAnimation() {
        this.animationPhase = 'completed';
        return this.result;
    }

    toJSON() {
        return {
            id: this.id,
            caseId: this.caseId,
            caseName: this.caseName,
            isOpen: this.isOpen,
            openedAt: this.openedAt,
            result: this.result,
            totalValue: this.totalValue
        };
    }
}

CaseOpening._nextId = 0;

export class Case {
    constructor(config = {}) {
        this.id = config.id || Case.nextId++;
        this.name = config.name || 'Case';
        this.description = config.description || '';
        this.imageUrl = config.imageUrl || '';
        this.keyRequired = config.keyRequired || false;
        this.keyId = config.keyId || null;
        this.keyPrice = config.keyPrice || 2.50;
        this.items = config.items || [];
        this.collection = config.collection || null;
        this.releaseDate = config.releaseDate || Date.now();
        this.expirationDate = config.expirationDate || null;
        this.isAvailable = true;
        this.isSpecial = config.isSpecial || false;
        this.specialType = config.specialType || null;
        this.questAvailable = config.questAvailable || false;
        this.rareItem = config.rareItem || null;
        this.dropRateBonus = config.dropRateBonus || 0;
        this.totalDrops = config.totalDrops || 0;
        this.calculateDropRates();
    }

    static get nextId() {
        return Case._nextId++;
    }

    static get TYPES() {
        return {
            STANDARD: 'standard',
            CSGO: 'csgo',
            SPECIAL: 'special',
            souvenir: 'souvenir',
            tournament: 'tournament',
            event: 'event',
            collection: 'collection'
        };
    }

    calculateDropRates() {
        const rarityTotals = { common: 0, industrial: 0, milspec: 0, restricted: 0, classified: 0, covert: 0, contraband: 0 };

        for (const item of this.items) {
            rarityTotals[item.rarity] += 1;
        }

        const total = this.items.length;
        const boost = this.dropRateBonus;

        this.dropRates = {
            common: Math.max(0, (rarityTotals.common / total) - boost / 2) / total,
            industrial: Math.max(0, (rarityTotals.industrial / total) - boost / 4) / total,
            milspec: Math.max(0, (rarityTotals.milspec / total) - boost / 8) / total,
            restricted: Math.max(0, (rarityTotals.restricted / total) - boost / 16) / total,
            classified: Math.max(0, (rarityTotals.classified / total) + boost / 32) / total,
            covert: Math.max(0, (rarityTotals.covert / total) + boost / 64) / total,
            contraband: (this.rareItem ? 1 : 0) + boost / 128
        };

        const sum = Object.values(this.dropRates).reduce((a, b) => a + b, 0);
        for (const rarity in this.dropRates) {
            this.dropRates[rarity] /= sum;
        }
    }

    addItem(item) {
        this.items.push(item);
        this.calculateDropRates();
    }

    removeItem(itemId) {
        const index = this.items.findIndex(i => i.id === itemId);
        if (index !== -1) {
            this.items.splice(index, 1);
            this.calculateDropRates();
            return true;
        }
        return false;
    }

    getRandomItem() {
        const random = Math.random();
        let cumulative = 0;

        for (const item of this.items) {
            cumulative += this.dropRates[item.rarity] || 0;
            if (random <= cumulative) {
                return item;
            }
        }

        return this.items[this.items.length - 1];
    }

    getItemsByRarity(rarity) {
        return this.items.filter(item => item.rarity === rarity);
    }

    getAverageValue() {
        return this.items.reduce((sum, item) => sum + (item.price || 0), 0) / this.items.length;
    }

    isExpired() {
        if (!this.expirationDate) return false;
        return Date.now() > this.expirationDate;
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            description: this.description,
            imageUrl: this.imageUrl,
            keyRequired: this.keyRequired,
            keyPrice: this.keyPrice,
            items: this.items,
            collection: this.collection,
            releaseDate: this.releaseDate,
            expirationDate: this.expirationDate,
            isAvailable: this.isAvailable,
            isSpecial: this.isSpecial,
            specialType: this.specialType,
            dropRates: this.dropRates,
            totalDrops: this.totalDrops
        };
    }
}

Case._nextId = 0;

export class Key {
    constructor(config = {}) {
        this.id = config.id || Key.nextId++;
        this.name = config.name || 'Case Key';
        this.description = config.description || '';
        this.forCaseId = config.forCaseId || null;
        this.price = config.price || 2.50;
        this.imageUrl = config.imageUrl || '';
        this.rarity = config.rarity || 'milspec';
        this.collection = config.collection || null;
        this.isAvailable = true;
        this.tradable = config.tradable !== undefined ? config.tradable : true;
        this.marketable = config.marketable !== undefined ? config.marketable : true;
    }

    static get nextId() {
        return Key._nextId++;
    }

    canOpen(caseItem) {
        if (!this.forCaseId && !caseItem.collection) return true;
        if (this.forCaseId === caseItem.id) return true;
        if (this.collection === caseItem.collection) return true;
        return false;
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            description: this.description,
            forCaseId: this.forCaseId,
            price: this.price,
            imageUrl: this.imageUrl,
            rarity: this.rarity,
            collection: this.collection,
            isAvailable: this.isAvailable,
            tradable: this.tradable,
            marketable: this.marketable
        };
    }
}

Key._nextId = 0;

export class Capsule {
    constructor(config = {}) {
        this.id = config.id || Capsule.nextId++;
        this.name = config.name || 'Capsule';
        this.description = config.description || '';
        this.imageUrl = config.imageUrl || '';
        this.items = config.items || [];
        this.rarityDistribution = config.rarityDistribution || this.defaultDistribution();
        this.price = config.price || 0;
        this.isAvailable = true;
        this.expirationDate = config.expirationDate || null;
        this.collection = config.collection || null;
        this.type = config.type || 'standard';
    }

    static get nextId() {
        return Capsule._nextId++;
    }

    defaultDistribution() {
        return {
            rare: 0.70,
            mythic: 0.20,
            legendary: 0.08,
            ancient: 0.02
        };
    }

    getRandomItem() {
        const random = Math.random();
        let cumulative = 0;

        for (const [rarity, probability] of Object.entries(this.rarityDistribution)) {
            cumulative += probability;
            if (random <= cumulative) {
                const itemsOfRarity = this.items.filter(i => i.rarity === rarity);
                if (itemsOfRarity.length > 0) {
                    return itemsOfRarity[Math.floor(Math.random() * itemsOfRarity.length)];
                }
            }
        }

        return this.items[Math.floor(Math.random() * this.items.length)];
    }

    open() {
        const result = this.getRandomItem();
        return {
            success: true,
            item: result,
            capsuleName: this.name
        };
    }

    isExpired() {
        if (!this.expirationDate) return false;
        return Date.now() > this.expirationDate;
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            description: this.description,
            imageUrl: this.imageUrl,
            items: this.items,
            rarityDistribution: this.rarityDistribution,
            price: this.price,
            isAvailable: this.isAvailable,
            expirationDate: this.expirationDate,
            collection: this.collection,
            type: this.type
        };
    }
}

Capsule._nextId = 0;

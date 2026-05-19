export class WorkshopBrowser {
    constructor() {
        this.items = new Map();
        this.categories = new Map();
        this.tags = new Map();
        this.collections = new Map();
        this.reviews = new Map();
        this.favorites = new Set();
        this.subscriptions = new Set();
        this.downloads = new Map();
        this.uploads = new Map();
        this.searchHistory = [];
        this.filters = new WorkshopFilters();
        this.sortBy = 'popular';
        this.pageSize = 20;
        this.currentPage = 1;
    }

    initialize() {
        this.setupCategories();
        this.setupTags();
    }

    setupCategories() {
        this.categories.set('maps', {
            id: 'maps',
            name: 'Maps',
            description: 'Community created maps',
            icon: '🗺️',
            itemCount: 0,
            subcategories: {
                hostage: { name: 'Hostage', count: 0 },
                bomb: { name: 'Bomb Defusal', count: 0 },
                armsrace: { name: 'Arms Race', count: 0 },
                deathmatch: { name: 'Deathmatch', count: 0 },
                competitive: { name: 'Competitive', count: 0 }
            }
        });

        this.categories.set('skins', {
            id: 'skins',
            name: 'Weapon Skins',
            description: 'Custom weapon skins and finishes',
            icon: '🔫',
            itemCount: 0,
            subcategories: {
                pistol: { name: 'Pistols', count: 0 },
                rifle: { name: 'Rifles', count: 0 },
                smg: { name: 'SMGs', count: 0 },
                shotgun: { name: 'Shotguns', count: 0 },
                sniper: { name: 'Sniper Rifles', count: 0 },
                knife: { name: 'Knives', count: 0 }
            }
        });

        this.categories.set('models', {
            id: 'models',
            name: 'Player Models',
            description: 'Custom player models and agents',
            icon: '👤',
            itemCount: 0,
            subcategories: {
                terrorist: { name: 'Terrorists', count: 0 },
                counterterrorist: { name: 'Counter-Terrorists', count: 0 },
                spectator: { name: 'Spectator', count: 0 }
            }
        });

        this.categories.set('ui', {
            id: 'ui',
            name: 'UI Mods',
            description: 'User interface modifications',
            icon: '🖥️',
            itemCount: 0,
            subcategories: {
                hud: { name: 'HUDs', count: 0 },
                menus: { name: 'Menus', count: 0 },
                fonts: { name: 'Fonts', count: 0 },
                icons: { name: 'Icons', count: 0 }
            }
        });

        this.categories.set('gameplay', {
            id: 'gameplay',
            name: 'Gameplay Mods',
            description: 'Gameplay modifications',
            icon: '🎮',
            itemCount: 0,
            subcategories: {
                bots: { name: 'Bot Mods', count: 0 },
                weapons: { name: 'Weapon Mods', count: 0 },
                grenades: { name: 'Grenade Mods', count: 0 },
                movement: { name: 'Movement Mods', count: 0 }
            }
        });

        this.categories.set('sounds', {
            id: 'sounds',
            name: 'Sounds',
            description: 'Audio modifications',
            icon: '🔊',
            itemCount: 0,
            subcategories: {
                music: { name: 'Music Kits', count: 0 },
                weapons: { name: 'Weapon Sounds', count: 0 },
                ambient: { name: 'Ambient Sounds', count: 0 },
                voice: { name: 'Voice Lines', count: 0 }
            }
        });

        this.categories.set('sprays', {
            id: 'sprays',
            name: 'Sprays',
            description: 'Custom spray logos',
            icon: '🎨',
            itemCount: 0,
            subcategories: {
                logos: { name: 'Logos', count: 0 },
                anime: { name: 'Anime', count: 0 },
                memes: { name: 'Memes', count: 0 },
                flags: { name: 'Flags', count: 0 }
            }
        });

        this.categories.set('packages', {
            id: 'packages',
            name: 'Game Packages',
            description: 'Complete game modifications',
            icon: '📦',
            itemCount: 0,
            subcategories: {}
        });
    }

    setupTags() {
        const popularTags = [
            'realistic', 'competitive', 'fun', 'cinematic', 'minimalist',
            'dark', 'bright', 'colorful', 'retro', 'modern', 'military',
            'scifi', 'fantasy', 'anime', 'tactical', 'casual', 'night',
            'day', 'urban', 'desert', 'snow', 'industrial', 'nature'
        ];

        for (const tag of popularTags) {
            this.tags.set(tag, {
                name: tag,
                count: 0,
                trending: false
            });
        }
    }

    createItem(itemData) {
        const item = new WorkshopItem(itemData);
        this.items.set(item.id, item);
        this.updateCategoryCount(item.category, 1);
        
        for (const tag of item.tags) {
            this.updateTagCount(tag, 1);
        }

        return item;
    }

    getItem(itemId) {
        return this.items.get(itemId);
    }

    search(query, options = {}) {
        const results = [];
        const queryLower = query.toLowerCase();

        for (const item of this.items.values()) {
            if (this.matchesQuery(item, queryLower, options)) {
                results.push(item);
            }
        }

        results.sort((a, b) => this.compareItems(a, b, options.sortBy || this.sortBy));

        const page = options.page || this.currentPage;
        const size = options.pageSize || this.pageSize;
        const start = (page - 1) * size;

        return {
            items: results.slice(start, start + size),
            total: results.length,
            page,
            pageSize: size,
            totalPages: Math.ceil(results.length / size)
        };
    }

    matchesQuery(item, query, options) {
        if (options.category && item.category !== options.category) return false;
        if (options.subcategory && item.subcategory !== options.subcategory) return false;
        if (options.tags && options.tags.length > 0) {
            if (!options.tags.some(tag => item.tags.includes(tag))) return false;
        }
        if (options.author) {
            if (item.authorId !== options.author && !item.coAuthors.includes(options.author)) return false;
        }
        if (options.premium !== undefined && item.isPremium !== options.premium) return false;
        if (options.free !== undefined && item.price > 0) return false;

        if (options.rating) {
            if (item.rating < options.rating) return false;
        }

        if (options.downloads) {
            if (item.downloadCount < options.downloads) return false;
        }

        if (query) {
            const titleMatch = item.title.toLowerCase().includes(query);
            const descMatch = item.description.toLowerCase().includes(query);
            const tagMatch = item.tags.some(tag => tag.includes(query));
            return titleMatch || descMatch || tagMatch;
        }

        return true;
    }

    compareItems(a, b, sortBy) {
        switch (sortBy) {
            case 'popular':
                return b.downloadCount - a.downloadCount;
            case 'rating':
                return b.rating - a.rating;
            case 'recent':
                return b.createdAt - a.createdAt;
            case 'updated':
                return b.updatedAt - a.updatedAt;
            case 'trending':
                const aTrending = a.downloadCount / Math.max(1, (Date.now() - a.createdAt) / (60 * 60 * 24 * 1000));
                const bTrending = b.downloadCount / Math.max(1, (Date.now() - b.createdAt) / (60 * 60 * 24 * 1000));
                return bTrending - aTrending;
            case 'alphabetical':
                return a.title.localeCompare(b.title);
            default:
                return b.downloadCount - a.downloadCount;
        }
    }

    getPopularItems(limit = 20, category = null) {
        const items = Array.from(this.items.values())
            .filter(item => {
                if (!item.published) return false;
                if (category && item.category !== category) return false;
                return true;
            })
            .sort((a, b) => b.downloadCount - a.downloadCount)
            .slice(0, limit);

        return items;
    }

    getRecentItems(limit = 20, category = null) {
        const items = Array.from(this.items.values())
            .filter(item => {
                if (!item.published) return false;
                if (category && item.category !== category) return false;
                return true;
            })
            .sort((a, b) => b.createdAt - a.createdAt)
            .slice(0, limit);

        return items;
    }

    getTopRatedItems(limit = 20, category = null) {
        const items = Array.from(this.items.values())
            .filter(item => {
                if (!item.published) return false;
                if (item.ratingCount < 10) return false;
                if (category && item.category !== category) return false;
                return true;
            })
            .sort((a, b) => b.rating - a.rating)
            .slice(0, limit);

        return items;
    }

    getItemsByCategory(categoryId, options = {}) {
        const category = this.categories.get(categoryId);
        if (!category) return [];

        let items = Array.from(this.items.values())
            .filter(item => item.category === categoryId && item.published);

        if (options.subcategory) {
            items = items.filter(item => item.subcategory === options.subcategory);
        }

        items.sort((a, b) => this.compareItems(a, b, options.sortBy || this.sortBy));

        return items;
    }

    updateCategoryCount(categoryId, delta) {
        const category = this.categories.get(categoryId);
        if (category) {
            category.itemCount += delta;
        }
    }

    updateTagCount(tagName, delta) {
        const tag = this.tags.get(tagName);
        if (tag) {
            tag.count += delta;
        }
    }

    addToFavorites(itemId, userId) {
        const key = `${itemId}_${userId}`;
        this.favorites.add(key);

        const item = this.items.get(itemId);
        if (item) {
            item.favoriteCount++;
        }
    }

    removeFromFavorites(itemId, userId) {
        const key = `${itemId}_${userId}`;
        this.favorites.delete(key);

        const item = this.items.get(itemId);
        if (item) {
            item.favoriteCount = Math.max(0, item.favoriteCount - 1);
        }
    }

    isFavorite(itemId, userId) {
        return this.favorites.has(`${itemId}_${userId}`);
    }

    subscribe(itemId, userId) {
        this.subscriptions.add(`${itemId}_${userId}`);

        const item = this.items.get(itemId);
        if (item) {
            item.subscriberCount++;
        }
    }

    unsubscribe(itemId, userId) {
        this.subscriptions.delete(`${itemId}_${userId}`);

        const item = this.items.get(itemId);
        if (item) {
            item.subscriberCount = Math.max(0, item.subscriberCount - 1);
        }
    }

    isSubscribed(itemId, userId) {
        return this.subscriptions.has(`${itemId}_${userId}`);
    }

    recordDownload(itemId, userId) {
        const key = `${itemId}_${userId}`;
        const count = this.downloads.get(key) || 0;
        this.downloads.set(key, count + 1);

        const item = this.items.get(itemId);
        if (item) {
            item.downloadCount++;
        }
    }

    addReview(reviewData) {
        const review = {
            id: 'review_' + Date.now(),
            itemId: reviewData.itemId,
            userId: reviewData.userId,
            userName: reviewData.userName,
            rating: reviewData.rating,
            title: reviewData.title,
            content: reviewData.content,
            createdAt: Date.now(),
            updatedAt: Date.now(),
            helpful: 0,
            notHelpful: 0,
            voters: { helpful: [], notHelpful: [] },
            images: reviewData.images || [],
            verified: reviewData.verified || false,
            highlighted: false,
            responses: []
        };

        this.reviews.set(review.id, review);

        const item = this.items.get(reviewData.itemId);
        if (item) {
            item.reviewCount++;
            item.totalRating += reviewData.rating;
            item.rating = item.totalRating / item.reviewCount;
        }

        return review;
    }

    voteOnReview(reviewId, userId, voteType) {
        const review = this.reviews.get(reviewId);
        if (!review) return;

        if (voteType === 'helpful') {
            if (!review.voters.helpful.includes(userId)) {
                review.helpful++;
                review.voters.helpful.push(userId);
                
                const notHelpfulIndex = review.voters.notHelpful.indexOf(userId);
                if (notHelpfulIndex !== -1) {
                    review.notHelpful--;
                    review.voters.notHelpful.splice(notHelpfulIndex, 1);
                }
            }
        } else if (voteType === 'not_helpful') {
            if (!review.voters.notHelpful.includes(userId)) {
                review.notHelpful++;
                review.voters.notHelpful.push(userId);
                
                const helpfulIndex = review.voters.helpful.indexOf(userId);
                if (helpfulIndex !== -1) {
                    review.helpful--;
                    review.voters.helpful.splice(helpfulIndex, 1);
                }
            }
        }
    }

    getReviewsForItem(itemId, options = {}) {
        let reviews = Array.from(this.reviews.values())
            .filter(r => r.itemId === itemId);

        if (options.rating) {
            reviews = reviews.filter(r => r.rating === options.rating);
        }

        if (options.highlighted) {
            reviews = reviews.filter(r => r.highlighted);
        }

        if (options.verified) {
            reviews = reviews.filter(r => r.verified);
        }

        reviews.sort((a, b) => {
            if (a.highlighted !== b.highlighted) return a.highlighted ? -1 : 1;
            
            const aHelpfulness = a.helpful - a.notHelpful;
            const bHelpfulness = b.helpful - b.notHelpful;
            return bHelpfulness - aHelpfulness;
        });

        return reviews;
    }

    createCollection(collectionData) {
        const collection = {
            id: 'collection_' + Date.now(),
            ownerId: collectionData.ownerId,
            ownerName: collectionData.ownerName,
            name: collectionData.name,
            description: collectionData.description,
            items: [],
            visibility: collectionData.visibility || 'public',
            featured: false,
            createdAt: Date.now(),
            updatedAt: Date.now(),
            followerCount: 0,
            viewCount: 0,
            tags: collectionData.tags || []
        };

        this.collections.set(collection.id, collection);
        return collection;
    }

    addToCollection(collectionId, itemId) {
        const collection = this.collections.get(collectionId);
        if (!collection) return false;

        if (!collection.items.includes(itemId)) {
            collection.items.push(itemId);
            collection.updatedAt = Date.now();
        }

        return true;
    }

    removeFromCollection(collectionId, itemId) {
        const collection = this.collections.get(collectionId);
        if (!collection) return false;

        collection.items = collection.items.filter(id => id !== itemId);
        collection.updatedAt = Date.now();

        return true;
    }

    getCollectionsByUser(userId) {
        return Array.from(this.collections.values())
            .filter(c => c.ownerId === userId)
            .sort((a, b) => b.updatedAt - a.updatedAt);
    }

    serialize() {
        return {
            items: Array.from(this.items.entries()),
            categories: Array.from(this.categories.entries()),
            tags: Array.from(this.tags.entries()),
            collections: Array.from(this.collections.entries()),
            reviews: Array.from(this.reviews.entries()),
            favorites: Array.from(this.favorites),
            subscriptions: Array.from(this.subscriptions),
            searchHistory: this.searchHistory,
            filters: this.filters.serialize()
        };
    }
}

export class WorkshopItem {
    constructor(data) {
        this.id = data.id || 'workshop_' + Date.now();
        this.title = data.title;
        this.description = data.description;
        this.category = data.category;
        this.subcategory = data.subcategory || null;
        this.authorId = data.authorId;
        this.authorName = data.authorName;
        this.coAuthors = data.coAuthors || [];
        this.thumbnail = data.thumbnail;
        this.previewImages = data.previewImages || [];
        this.videoUrl = data.videoUrl || null;
        this.files = data.files || [];
        this.size = data.size || 0;
        this.tags = data.tags || [];
        this.createdAt = Date.now();
        this.updatedAt = Date.now();
        this.published = false;
        this.publishedAt = null;
        this.approved = false;
        this.approvedAt = null;
        this.isPremium = data.isPremium || false;
        this.price = data.price || 0;
        this.currency = data.currency || 'USD';
        this.downloadCount = 0;
        this.subscriberCount = 0;
        this.favoriteCount = 0;
        this.rating = 0;
        this.ratingCount = 0;
        this.totalRating = 0;
        this.reviewCount = 0;
        this.viewCount = 0;
        this.trending = false;
        this.featured = false;
        this.banned = false;
        this.banReason = null;
        this.violations = [];
        this.version = '1.0.0';
        this.minGameVersion = data.minGameVersion || '1.0.0';
        this.maxGameVersion = data.maxGameVersion || null;
        this.dependencies = data.dependencies || [];
        this.conflicts = data.conflicts || [];
        this.platforms = data.platforms || ['windows', 'mac', 'linux'];
        this.supportedLanguages = data.supportedLanguages || ['en'];
        this.ratings = { positive: 0, negative: 0, mixed: 0 };
        this.statistics = {
            dailyDownloads: new Map(),
            weeklyDownloads: new Map(),
            monthlyDownloads: new Map()
        };
    }

    publish() {
        this.published = true;
        this.publishedAt = Date.now();
    }

    unpublish() {
        this.published = false;
        this.publishedAt = null;
    }

    update(data) {
        Object.assign(this, data);
        this.updatedAt = Date.now();
    }

    addFile(fileData) {
        const file = {
            id: 'file_' + Date.now(),
            name: fileData.name,
            url: fileData.url,
            size: fileData.size,
            hash: fileData.hash,
            version: fileData.version || '1.0.0',
            type: fileData.type,
            required: fileData.required ?? true,
            createdAt: Date.now()
        };

        this.files.push(file);
        return file;
    }

    rate(userId, rating) {
        this.ratings[rating]++;
        this.ratingCount++;
        this.totalRating += rating === 'positive' ? 5 : rating === 'mixed' ? 3 : 1;
        this.rating = (this.totalRating / this.ratingCount).toFixed(1);
    }

    getRatingBreakdown() {
        const total = this.ratings.positive + this.ratings.negative + this.ratings.mixed;
        return {
            positive: total > 0 ? (this.ratings.positive / total * 100).toFixed(1) : 0,
            negative: total > 0 ? (this.ratings.negative / total * 100).toFixed(1) : 0,
            mixed: total > 0 ? (this.ratings.mixed / total * 100).toFixed(1) : 0
        };
    }

    getCompatibilityStatus() {
        return {
            compatible: this.files.every(f => f.verified),
            issues: this.files.filter(f => !f.verified).map(f => f.issue)
        };
    }
}

export class WorkshopFilters {
    constructor() {
        this.categories = [];
        this.tags = [];
        this.rating = null;
        this.price = null;
        this.sortBy = 'popular';
        this.timeRange = null;
        this.author = null;
        this.searchQuery = '';
        this.language = null;
        this.platform = null;
        this.showOnlySubscribed = false;
        this.showOnlyFavorites = false;
    }

    setCategory(category) {
        this.categories = category ? [category] : [];
    }

    addTag(tag) {
        if (!this.tags.includes(tag)) {
            this.tags.push(tag);
        }
    }

    removeTag(tag) {
        this.tags = this.tags.filter(t => t !== tag);
    }

    clear() {
        this.categories = [];
        this.tags = [];
        this.rating = null;
        this.price = null;
        this.author = null;
        this.searchQuery = '';
        this.timeRange = null;
    }

    serialize() {
        return {
            categories: this.categories,
            tags: this.tags,
            rating: this.rating,
            price: this.price,
            sortBy: this.sortBy,
            timeRange: this.timeRange,
            author: this.author,
            language: this.language,
            platform: this.platform
        };
    }
}

export default {
    WorkshopBrowser,
    WorkshopItem,
    WorkshopFilters
};

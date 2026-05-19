import { GameState } from './config.js';

export const AvatarFrame = {
    DEFAULT: { id: 'default', name: 'Default', price: 0, rarity: 'common' },
    BRONZE: { id: 'bronze', name: 'Bronze Frame', price: 500, rarity: 'common' },
    SILVER: { id: 'silver', name: 'Silver Frame', price: 1000, rarity: 'uncommon' },
    GOLD: { id: 'gold', name: 'Gold Frame', price: 2500, rarity: 'rare' },
    PLATINUM: { id: 'platinum', name: 'Platinum Frame', price: 5000, rarity: 'epic' },
    DIAMOND: { id: 'diamond', name: 'Diamond Frame', price: 10000, rarity: 'legendary' },
    GLOBAL: { id: 'global', name: 'Global Elite', price: 15000, rarity: 'legendary' },
    MASTERS: { id: 'masters', name: 'Masters', price: 20000, rarity: 'legendary' }
};

export const ProfileBanner = {
    DEFAULT: { id: 'default', name: 'Default Banner', price: 0, rarity: 'common' },
    DUST2: { id: 'dust2', name: 'Dust II', price: 300, rarity: 'common' },
    MIRAGE: { id: 'mirage', name: 'Mirage', price: 300, rarity: 'common' },
    INFERNO: { id: 'inferno', name: 'Inferno', price: 300, rarity: 'common' },
    NUKE: { id: 'nuke', name: 'Nuke', price: 500, rarity: 'uncommon' },
    OVERPASS: { id: 'overpass', name: 'Overpass', price: 500, rarity: 'uncommon' },
    TRAIN: { id: 'train', name: 'Train', price: 500, rarity: 'uncommon' },
    VERTIGO: { id: 'vertigo', name: 'Vertigo', price: 750, rarity: 'rare' },
    ANCIENT: { id: 'ancient', name: 'Ancient', price: 750, rarity: 'rare' },
    COMMUNITY: { id: 'community', name: 'Community', price: 1000, rarity: 'rare' },
    ESPORTS: { id: 'esports', name: 'Esports', price: 2000, rarity: 'epic' },
    MAJOR: { id: 'major', name: 'Major Championship', price: 5000, rarity: 'legendary' }
};

export const PlayerTitle = {
    NEWCOMER: { id: 'newcomer', name: 'Newcomer', requirement: null, color: '#AAAAAA' },
    CADET: { id: 'cadet', name: 'Cadet', requirement: { level: 5 }, color: '#4A90D9' },
    CORPORAL: { id: 'corporal', name: 'Corporal', requirement: { level: 10 }, color: '#5D8AA8' },
    SERGEANT: { id: 'sergeant', name: 'Sergeant', requirement: { level: 15 }, color: '#6B8E23' },
    LIEUTENANT: { id: 'lieutenant', name: 'Lieutenant', requirement: { level: 20 }, color: '#9B59B6' },
    CAPTAIN: { id: 'captain', name: 'Captain', requirement: { level: 25 }, color: '#E74C3C' },
    MAJOR: { id: 'major', name: 'Major', requirement: { level: 30 }, color: '#F39C12' },
    COLONEL: { id: 'colonel', name: 'Colonel', requirement: { level: 40 }, color: '#E67E22' },
    GENERAL: { id: 'general', name: 'General', requirement: { level: 50 }, color: '#C0392B' },
    CHAMPION: { id: 'champion', name: 'Champion', requirement: { wins: 100 }, color: '#F1C40F' },
    LEGEND: { id: 'legend', name: 'Legend', requirement: { wins: 500 }, color: '#9B59B6' },
    MASTER: { id: 'master', name: 'Master', requirement: { rank: 'GLOBAL' }, color: '#E91E63' }
};

export const BackgroundEffect = {
    NONE: { id: 'none', name: 'None', price: 0 },
    PARTICLES: { id: 'particles', name: 'Particles', price: 500 },
    GRADIENT: { id: 'gradient', name: 'Gradient', price: 750 },
    ANIMATED: { id: 'animated', name: 'Animated', price: 1000 },
    STARS: { id: 'stars', name: 'Stars', price: 1500 },
    FLAMES: { id: 'flames', name: 'Flames', price: 2000 }
};

export const BadgeType = {
    ACHIEVEMENT: 'achievement',
    EVENT: 'event',
    COMPETITIVE: 'competitive',
    SOCIAL: 'social',
    SPECIAL: 'special'
};

export class Avatar {
    constructor(id, image, frame = AvatarFrame.DEFAULT) {
        this.id = id;
        this.image = image;
        this.frame = frame;
        this.level = 1;
        this.xp = 0;
        this.isDefault = true;
    }

    setFrame(frame) {
        this.frame = frame;
    }

    addXP(amount) {
        this.xp += amount;
        while (this.xp >= this.getXPForNextLevel()) {
            this.xp -= this.getXPForNextLevel();
            this.level++;
        }
    }

    getXPForNextLevel() {
        return 1000 * this.level;
    }

    export() {
        return {
            id: this.id,
            image: this.image,
            frame: this.frame,
            level: this.level,
            xp: this.xp,
            is_default: this.isDefault
        };
    }
}

export class PlayerBadge {
    constructor(id, type, name, icon, rarity, description, unlockedDate = null) {
        this.id = id;
        this.type = type;
        this.name = name;
        this.icon = icon;
        this.rarity = rarity;
        this.description = description;
        this.unlockedDate = unlockedDate;
        this.isUnlocked = unlockedDate !== null;
        this.displayOrder = 0;
        this.isEquipped = false;
    }

    unlock() {
        if (!this.isUnlocked) {
            this.isUnlocked = true;
            this.unlockedDate = new Date();
        }
    }

    equip() {
        this.isEquipped = true;
    }

    unequip() {
        this.isEquipped = false;
    }

    getRarityColor() {
        const colors = {
            common: '#AAAAAA',
            uncommon: '#4A90D9',
            rare: '#9B59B6',
            epic: '#E74C3C',
            legendary: '#F1C40F'
        };
        return colors[this.rarity] || colors.common;
    }

    export() {
        return {
            id: this.id,
            type: this.type,
            name: this.name,
            icon: this.icon,
            rarity: this.rarity,
            description: this.description,
            unlocked_date: this.unlockedDate ? this.unlockedDate.toISOString() : null,
            is_unlocked: this.isUnlocked,
            display_order: this.displayOrder,
            is_equipped: this.isEquipped
        };
    }

    static import(data) {
        const badge = new PlayerBadge(
            data.id,
            data.type,
            data.name,
            data.icon,
            data.rarity,
            data.description,
            data.unlocked_date ? new Date(data.unlocked_date) : null
        );
        badge.displayOrder = data.display_order || 0;
        badge.isEquipped = data.is_equipped || false;
        return badge;
    }
}

export class PlayerSticker {
    constructor(id, name, image, rarity, price, wear = 'factory_new') {
        this.id = id;
        this.name = name;
        this.image = image;
        this.rarity = rarity;
        this.price = price;
        this.wear = wear;
        this.isApplied = false;
        this.position = null;
        this.slot = null;
    }

    apply(slot, position) {
        this.isApplied = true;
        this.slot = slot;
        this.position = position;
    }

    remove() {
        this.isApplied = false;
        this.slot = null;
        this.position = null;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            image: this.image,
            rarity: this.rarity,
            price: this.price,
            wear: this.wear,
            is_applied: this.isApplied,
            slot: this.slot,
            position: this.position
        };
    }
}

export class PlayerGraffiti {
    constructor(id, name, image, rarity, price) {
        this.id = id;
        this.name = name;
        this.image = image;
        this.rarity = rarity;
        this.price = price;
        this.uses = 0;
        this.maxUses = 100;
    }

    use() {
        this.uses++;
        if (this.uses >= this.maxUses) {
            return false;
        }
        return true;
    }

    refill() {
        this.uses = 0;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            image: this.image,
            rarity: this.rarity,
            price: this.price,
            uses: this.uses,
            max_uses: this.maxUses
        };
    }
}

export class PlayerSeal {
    constructor(id, name, rarity, price) {
        this.id = id;
        this.name = name;
        this.rarity = rarity;
        this.price = price;
        this.isApplied = false;
    }

    apply() {
        this.isApplied = true;
    }

    remove() {
        this.isApplied = false;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            rarity: this.rarity,
            price: this.price,
            is_applied: this.isApplied
        };
    }
}

export class PlayerPin {
    constructor(id, name, rarity, price, event = null) {
        this.id = id;
        this.name = name;
        this.rarity = rarity;
        this.price = price;
        this.event = event;
        this.isEquipped = false;
    }

    equip() {
        this.isEquipped = true;
    }

    unequip() {
        this.isEquipped = false;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            rarity: this.rarity,
            price: this.price,
            event: this.event,
            is_equipped: this.isEquipped
        };
    }
}

export class ProfileTheme {
    constructor(id, name, primaryColor, secondaryColor, accentColor, price) {
        this.id = id;
        this.name = name;
        this.primaryColor = primaryColor;
        this.secondaryColor = secondaryColor;
        this.accentColor = accentColor;
        this.price = price;
        this.isOwned = false;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            primary_color: this.primaryColor,
            secondary_color: this.secondaryColor,
            accent_color: this.accentColor,
            price: this.price,
            is_owned: this.isOwned
        };
    }
}

export class ProfileMusic {
    constructor(id, name, artist, duration, price, preview = null) {
        this.id = id;
        this.name = name;
        this.artist = artist;
        this.duration = duration;
        this.price = price;
        this.preview = preview;
        this.isOwned = false;
        this.isEquipped = false;
    }

    equip() {
        this.isEquipped = true;
    }

    unequip() {
        this.isEquipped = false;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            artist: this.artist,
            duration: this.duration,
            price: this.price,
            preview: this.preview,
            is_owned: this.isOwned,
            is_equipped: this.isEquipped
        };
    }
}

export class PlayerCard {
    constructor(id, name, image, rarity, price, category) {
        this.id = id;
        this.name = name;
        this.image = image;
        this.rarity = rarity;
        this.price = price;
        this.category = category;
        this.isOwned = false;
        this.isEquipped = false;
    }

    equip() {
        this.isEquipped = true;
    }

    unequip() {
        this.isEquipped = false;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            image: this.image,
            rarity: this.rarity,
            price: this.price,
            category: this.category,
            is_owned: this.isOwned,
            is_equipped: this.isEquipped
        };
    }
}

export class Friend {
    constructor(steamId, name, avatar, status) {
        this.steamId = steamId;
        this.name = name;
        this.avatar = avatar;
        this.status = status;
        this.lastSeen = null;
        this.isFavorite = false;
        this.note = '';
        this.inviteDate = new Date();
    }

    setStatus(status) {
        this.status = status;
        if (status !== 'online') {
            this.lastSeen = new Date();
        }
    }

    toggleFavorite() {
        this.isFavorite = !this.isFavorite;
    }

    setNote(note) {
        this.note = note;
    }

    export() {
        return {
            steam_id: this.steamId,
            name: this.name,
            avatar: this.avatar,
            status: this.status,
            last_seen: this.lastSeen ? this.lastSeen.toISOString() : null,
            is_favorite: this.isFavorite,
            note: this.note,
            invite_date: this.inviteDate.toISOString()
        };
    }
}

export class PlayerProfile {
    constructor(steamId, personaName) {
        this.steamId = steamId;
        this.personaName = personaName;
        this.avatar = new Avatar(steamId, 'default_avatar');
        this.banner = ProfileBanner.DEFAULT;
        this.title = PlayerTitle.NEWCOMER;
        this.backgroundEffect = BackgroundEffect.NONE;
        this.theme = new ProfileTheme('default', 'Default', '#000000', '#FFFFFF', '#4A90D9', 0);
        this.badges = [];
        this.stickers = [];
        this.graffitis = [];
        this.seals = [];
        this.pins = [];
        this.music = [];
        this.cards = [];
        this.friends = [];
        this.clanTag = '';
        this.bio = '';
        this.realName = '';
        this.location = '';
        this.website = '';
        this.birthday = null;
        this.country = '';
        this.languages = [];
        this.is VACBanned = false;
        this.isTradeBanned = false;
        this.tradeBanExpires = null;
        this.level = 1;
        this.xp = 0;
        this.totalXP = 0;
        this.playtime = {
            total: 0,
            twoWeeks: 0,
            lastWeek: 0
        };
        this.createdAt = new Date();
        this.lastUpdated = new Date();
        this.stats = {
            kills: 0,
            deaths: 0,
            wins: 0,
            losses: 0,
            matches: 0,
            headshots: 0,
            mvps: 0,
            aces: 0,
            hours: 0
        };
        this.privacySettings = {
            profile: 'public',
            games: 'public',
            inventory: 'friends',
            stats: 'public'
        };
        this.displaySettings = {
            showVAC: true,
            showBan: true,
            showCountry: true,
            showLevel: true,
            showRank: true
        };
    }

    setAvatar(avatar) {
        this.avatar = avatar;
        this.lastUpdated = new Date();
    }

    setBanner(banner) {
        this.banner = banner;
        this.lastUpdated = new Date();
    }

    setTitle(title) {
        this.title = title;
        this.lastUpdated = new Date();
    }

    setBio(bio) {
        this.bio = bio.substring(0, 500);
        this.lastUpdated = new Date();
    }

    setRealName(realName) {
        this.realName = realName.substring(0, 100);
        this.lastUpdated = new Date();
    }

    setLocation(location) {
        this.location = location;
        this.lastUpdated = new Date();
    }

    setWebsite(website) {
        this.website = website.substring(0, 200);
        this.lastUpdated = new Date();
    }

    setCountry(country) {
        this.country = country;
        this.lastUpdated = new Date();
    }

    setLanguages(languages) {
        this.languages = languages.slice(0, 10);
        this.lastUpdated = new Date();
    }

    addXP(amount) {
        this.xp += amount;
        this.totalXP += amount;
        while (this.xp >= this.getXPForNextLevel()) {
            this.xp -= this.getXPForNextLevel();
            this.level++;
        }
        this.lastUpdated = new Date();
    }

    getXPForNextLevel() {
        return 1000 * this.level;
    }

    getLevelProgress() {
        return {
            current: this.xp,
            required: this.getXPForNextLevel(),
            percentage: (this.xp / this.getXPForNextLevel()) * 100
        };
    }

    addBadge(badge) {
        const existing = this.badges.find(b => b.id === badge.id);
        if (!existing) {
            badge.unlock();
            this.badges.push(badge);
        }
    }

    equipBadge(badgeId) {
        for (const badge of this.badges) {
            badge.isEquipped = badge.id === badgeId;
        }
    }

    addSticker(sticker) {
        this.stickers.push(sticker);
    }

    removeSticker(stickerId) {
        const index = this.stickers.findIndex(s => s.id === stickerId);
        if (index > -1) {
            this.stickers.splice(index, 1);
        }
    }

    addGraffiti(graffiti) {
        this.graffitis.push(graffiti);
    }

    removeGraffiti(graffitiId) {
        const index = this.graffitis.findIndex(g => g.id === graffitiId);
        if (index > -1) {
            this.graffitis.splice(index, 1);
        }
    }

    addFriend(friend) {
        if (!this.friends.find(f => f.steamId === friend.steamId)) {
            this.friends.push(friend);
        }
    }

    removeFriend(steamId) {
        const index = this.friends.findIndex(f => f.steamId === steamId);
        if (index > -1) {
            this.friends.splice(index, 1);
        }
    }

    getFriends(status = null) {
        if (status) {
            return this.friends.filter(f => f.status === status);
        }
        return this.friends;
    }

    setPrivacySetting(category, level) {
        this.privacySettings[category] = level;
        this.lastUpdated = new Date();
    }

    setDisplaySetting(setting, value) {
        this.displaySettings[setting] = value;
        this.lastUpdated = new Date();
    }

    updateStats(stats) {
        this.stats = { ...this.stats, ...stats };
        this.lastUpdated = new Date();
    }

    getWinRate() {
        const total = this.stats.wins + this.stats.losses;
        return total > 0 ? ((this.stats.wins / total) * 100).toFixed(1) : '0.0';
    }

    getKDRatio() {
        return this.stats.deaths > 0 
            ? (this.stats.kills / this.stats.deaths).toFixed(2) 
            : this.stats.kills.toFixed(2);
    }

    getHeadshotPercentage() {
        return this.stats.kills > 0 
            ? ((this.stats.headshots / this.stats.kills) * 100).toFixed(1) 
            : '0.0';
    }

    getAverageScore() {
        return this.stats.matches > 0 
            ? Math.round((this.stats.kills + this.stats.assists) / this.stats.matches) 
            : 0;
    }

    export() {
        return {
            steam_id: this.steamId,
            persona_name: this.personaName,
            avatar: this.avatar.export(),
            banner: this.banner,
            title: this.title,
            background_effect: this.backgroundEffect,
            theme: this.theme.export(),
            badges: this.badges.map(b => b.export()),
            stickers: this.stickers.map(s => s.export()),
            graffitis: this.graffitis.map(g => g.export()),
            seals: this.seals.map(s => s.export()),
            pins: this.pins.map(p => p.export()),
            music: this.music.map(m => m.export()),
            cards: this.cards.map(c => c.export()),
            friends: this.friends.map(f => f.export()),
            clan_tag: this.clanTag,
            bio: this.bio,
            real_name: this.realName,
            location: this.location,
            website: this.website,
            birthday: this.birthday ? this.birthday.toISOString() : null,
            country: this.country,
            languages: this.languages,
            is_vac_banned: this.is VACBanned,
            is_trade_banned: this.isTradeBanned,
            trade_ban_expires: this.tradeBanExpires ? this.tradeBanExpires.toISOString() : null,
            level: this.level,
            xp: this.xp,
            total_xp: this.totalXP,
            playtime: this.playtime,
            created_at: this.createdAt.toISOString(),
            last_updated: this.lastUpdated.toISOString(),
            stats: this.stats,
            privacy_settings: this.privacySettings,
            display_settings: this.displaySettings
        };
    }

    static import(data) {
        const profile = new PlayerProfile(data.steam_id, data.persona_name);
        profile.avatar = new Avatar(data.avatar.id, data.avatar.image);
        profile.banner = data.banner;
        profile.title = data.title;
        profile.backgroundEffect = data.background_effect;
        profile.theme = ProfileTheme.import(data.theme);
        profile.badges = (data.badges || []).map(b => PlayerBadge.import(b));
        profile.stickers = (data.stickers || []).map(s => new PlayerSticker(s.id, s.name, s.image, s.rarity, s.price));
        profile.graffitis = (data.graffitis || []).map(g => new PlayerGraffiti(g.id, g.name, g.image, g.rarity, g.price));
        profile.seals = (data.seals || []).map(s => new PlayerSeal(s.id, s.name, s.rarity, s.price));
        profile.pins = (data.pins || []).map(p => new PlayerPin(p.id, p.name, p.rarity, p.price, p.event));
        profile.music = (data.music || []).map(m => new ProfileMusic(m.id, m.name, m.artist, m.duration, m.price));
        profile.cards = (data.cards || []).map(c => new PlayerCard(c.id, c.name, c.image, c.rarity, c.price, c.category));
        profile.friends = (data.friends || []).map(f => new Friend(f.steam_id, f.name, f.avatar, f.status));
        profile.clanTag = data.clan_tag || '';
        profile.bio = data.bio || '';
        profile.realName = data.real_name || '';
        profile.location = data.location || '';
        profile.website = data.website || '';
        profile.birthday = data.birthday ? new Date(data.birthday) : null;
        profile.country = data.country || '';
        profile.languages = data.languages || [];
        profile.is VACBanned = data.is_vac_banned || false;
        profile.isTradeBanned = data.is_trade_banned || false;
        profile.tradeBanExpires = data.trade_ban_expires ? new Date(data.trade_ban_expires) : null;
        profile.level = data.level || 1;
        profile.xp = data.xp || 0;
        profile.totalXP = data.total_xp || 0;
        profile.playtime = data.playtime || { total: 0, twoWeeks: 0, lastWeek: 0 };
        profile.createdAt = data.created_at ? new Date(data.created_at) : new Date();
        profile.lastUpdated = data.last_updated ? new Date(data.last_updated) : new Date();
        profile.stats = data.stats || {};
        profile.privacySettings = data.privacy_settings || {};
        profile.displaySettings = data.display_settings || {};
        return profile;
    }
}

export class ProfileManager {
    static instance = null;

    constructor() {
        this.profiles = new Map();
        this.currentProfile = null;
        this.customizationItems = {
            avatars: new Map(),
            banners: new Map(),
            titles: new Map(),
            themes: new Map(),
            badges: new Map(),
            stickers: new Map(),
            graffitis: new Map(),
            seals: new Map(),
            pins: new Map(),
            music: new Map(),
            cards: new Map()
        };
        this.listeners = new Map();
        this.initDefaultItems();
    }

    static getInstance() {
        if (!ProfileManager.instance) {
            ProfileManager.instance = new ProfileManager();
        }
        return ProfileManager.instance;
    }

    initDefaultItems() {
        for (const [id, frame] of Object.entries(AvatarFrame)) {
            this.customizationItems.avatars.set(id, frame);
        }
        for (const [id, banner] of Object.entries(ProfileBanner)) {
            this.customizationItems.banners.set(id, banner);
        }
        for (const [id, title] of Object.entries(PlayerTitle)) {
            this.customizationItems.titles.set(id, title);
        }
        this.customizationItems.themes.set('default', new ProfileTheme('default', 'Default', '#000000', '#FFFFFF', '#4A90D9', 0));
    }

    createProfile(steamId, personaName) {
        const profile = new PlayerProfile(steamId, personaName);
        this.profiles.set(steamId, profile);
        this.emit('profileCreated', profile);
        return profile;
    }

    getProfile(steamId) {
        return this.profiles.get(steamId);
    }

    setCurrentProfile(steamId) {
        const profile = this.profiles.get(steamId);
        if (profile) {
            this.currentProfile = profile;
            this.emit('currentProfileChanged', profile);
        }
    }

    getCurrentProfile() {
        return this.currentProfile;
    }

    updateProfile(steamId, updates) {
        const profile = this.profiles.get(steamId);
        if (profile) {
            Object.assign(profile, updates);
            profile.lastUpdated = new Date();
            this.emit('profileUpdated', profile);
        }
    }

    deleteProfile(steamId) {
        const deleted = this.profiles.delete(steamId);
        if (deleted) {
            this.emit('profileDeleted', steamId);
        }
        return deleted;
    }

    addCustomizationItem(category, item) {
        const collection = this.customizationItems[category];
        if (collection) {
            collection.set(item.id, item);
            this.emit('itemAdded', { category, item });
        }
    }

    getCustomizationItems(category) {
        return Array.from((this.customizationItems[category] || new Map()).values());
    }

    purchaseItem(category, itemId, profileId) {
        const item = this.customizationItems[category]?.get(itemId);
        const profile = this.profiles.get(profileId);

        if (item && profile && item.price > 0) {
            const owned = this.isItemOwned(profileId, category, itemId);
            if (!owned) {
                profile.addXP(Math.floor(item.price / 10));
                this.emit('itemPurchased', { category, item, profile });
                return true;
            }
        }
        return false;
    }

    isItemOwned(profileId, category, itemId) {
        const profile = this.profiles.get(profileId);
        if (!profile) return false;

        switch (category) {
            case 'stickers':
                return profile.stickers.some(s => s.id === itemId);
            case 'graffitis':
                return profile.graffitis.some(g => g.id === itemId);
            case 'music':
                return profile.music.some(m => m.id === itemId);
            case 'cards':
                return profile.cards.some(c => c.id === itemId);
            default:
                return true;
        }
    }

    exportProfile(steamId) {
        const profile = this.profiles.get(steamId);
        return profile ? profile.export() : null;
    }

    importProfile(data) {
        const profile = PlayerProfile.import(data);
        this.profiles.set(profile.steamId, profile);
        return profile;
    }

    exportAllProfiles() {
        return Array.from(this.profiles.values()).map(p => p.export());
    }

    importAllProfiles(data) {
        for (const profileData of data) {
            this.importProfile(profileData);
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
                console.error(`Error in profile listener: ${e}`);
            }
        }
    }
}

export const Profiles = ProfileManager.getInstance();

export const SaveSlot = {
    AUTO: 'auto',
    MANUAL_1: 'manual_1',
    MANUAL_2: 'manual_2',
    MANUAL_3: 'manual_3',
    MANUAL_4: 'manual_4',
    MANUAL_5: 'manual_5'
};

export const SaveType = {
    FULL: 'full',
    QUICK: 'quick',
    CHECKPOINT: 'checkpoint',
    AUTO: 'auto',
    SETTINGS: 'settings',
    PROFILE: 'profile'
};

export const LoadPriority = {
    HIGH: 'high',
    NORMAL: 'normal',
    LOW: 'low'
};

export class SaveMetadata {
    constructor() {
        this.slotId = null;
        this.type = SaveType.FULL;
        this.timestamp = Date.now();
        this.playtime = 0;
        this.level = 0;
        this.matchId = null;
        this.mapId = null;
        this.description = '';
        this.thumbnail = null;
        this.version = '1.0.0';
        this.size = 0;
        this.checksum = null;
        this.valid = true;
        this.corrupted = false;
    }

    static fromSaveData(data) {
        const metadata = new SaveMetadata();
        metadata.slotId = data.slotId;
        metadata.type = data.type;
        metadata.timestamp = data.timestamp;
        metadata.playtime = data.playtime;
        metadata.level = data.level;
        metadata.matchId = data.matchId;
        metadata.mapId = data.mapId;
        metadata.description = data.description || '';
        metadata.thumbnail = data.thumbnail || null;
        metadata.version = data.version;
        metadata.size = data.size;
        metadata.checksum = data.checksum;
        metadata.valid = data.valid !== false;
        metadata.corrupted = data.corrupted === true;
        return metadata;
    }

    getFormattedDate() {
        const date = new Date(this.timestamp);
        return date.toLocaleString();
    }

    getFormattedPlaytime() {
        const hours = Math.floor(this.playtime / 3600);
        const minutes = Math.floor((this.playtime % 3600) / 60);
        if (hours > 0) {
            return `${hours}h ${minutes}m`;
        }
        return `${minutes}m`;
    }

    isValid() {
        return this.valid && !this.corrupted;
    }

    serialize() {
        return {
            slotId: this.slotId,
            type: this.type,
            timestamp: this.timestamp,
            playtime: this.playtime,
            level: this.level,
            matchId: this.matchId,
            mapId: this.mapId,
            description: this.description,
            thumbnail: this.thumbnail,
            version: this.version,
            size: this.size,
            checksum: this.checksum,
            valid: this.valid,
            corrupted: this.corrupted
        };
    }
}

export class ProfileManager {
    constructor() {
        this.profiles = new Map();
        this.currentProfile = null;
        this.profileOrder = [];
        this.maxProfiles = 5;
    }

    createProfile(name, settings = {}) {
        if (this.profiles.size >= this.maxProfiles) {
            return null;
        }
        const profileId = this.generateProfileId();
        const profile = new PlayerProfile(profileId, name);
        profile.settings = { ...profile.settings, ...settings };
        this.profiles.set(profileId, profile);
        this.profileOrder.push(profileId);
        return profile;
    }

    deleteProfile(profileId) {
        if (!this.profiles.has(profileId)) {
            return false;
        }
        this.profiles.delete(profileId);
        this.profileOrder = this.profileOrder.filter(id => id !== profileId);
        if (this.currentProfile?.id === profileId) {
            this.currentProfile = null;
        }
        return true;
    }

    loadProfile(profileId) {
        const profile = this.profiles.get(profileId);
        if (profile) {
            this.currentProfile = profile;
            return true;
        }
        return false;
    }

    getProfile(profileId) {
        return this.profiles.get(profileId);
    }

    getCurrentProfile() {
        return this.currentProfile;
    }

    getAllProfiles() {
        return Array.from(this.profiles.values());
    }

    renameProfile(profileId, newName) {
        const profile = this.profiles.get(profileId);
        if (profile) {
            profile.name = newName;
            return true;
        }
        return false;
    }

    duplicateProfile(profileId) {
        const original = this.profiles.get(profileId);
        if (!original || this.profiles.size >= this.maxProfiles) {
            return null;
        }
        const newProfile = original.clone();
        newProfile.id = this.generateProfileId();
        newProfile.name = original.name + ' (Copy)';
        this.profiles.set(newProfile.id, newProfile);
        this.profileOrder.push(newProfile.id);
        return newProfile;
    }

    generateProfileId() {
        return 'profile_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    serialize() {
        return {
            profiles: Array.from(this.profiles.entries()).map(([id, profile]) => [id, profile.serialize()]),
            currentProfileId: this.currentProfile?.id,
            profileOrder: this.profileOrder
        };
    }

    deserialize(data) {
        this.profiles = new Map(data.profiles?.map(([id, profileData]) => {
            const profile = new PlayerProfile(id);
            profile.deserialize(profileData);
            return [id, profile];
        }) || []);
        this.currentProfile = data.currentProfileId ? this.profiles.get(data.currentProfileId) : null;
        this.profileOrder = data.profileOrder || [];
    }
}

export class PlayerProfile {
    constructor(id, name = 'Player') {
        this.id = id;
        this.name = name;
        this.createdAt = Date.now();
        this.lastPlayed = Date.now();
        this.totalPlaytime = 0;
        this.level = 1;
        this.xp = 0;
        this.settings = new ProfileSettings();
        this.keyBindings = new KeyBindings();
        this.videoSettings = new VideoSettings();
        this.audioSettings = new AudioSettings();
        this.controlSettings = new ControlSettings();
        this.cosmetics = new CosmeticsInventory();
        this.inventory = new Inventory();
        this.contracts = [];
        this.missions = [];
        this.friends = [];
        this.statistics = null;
        this.achievements = [];
        this.unlockedItems = [];
        this.crateKeys = 0;
        this.currency = {
            coins: 0,
            points: 0,
            scrap: 0
        };
        this.preferences = new PlayerPreferences();
    }

    addXP(amount) {
        this.xp += amount;
        while (this.xp >= this.getXPForNextLevel()) {
            this.xp -= this.getXPForNextLevel();
            this.level++;
        }
    }

    getXPForNextLevel() {
        return Math.floor(1000 * Math.pow(1.2, this.level - 1));
    }

    getProgressToNextLevel() {
        return (this.xp / this.getXPForNextLevel()) * 100;
    }

    updatePlaytime(seconds) {
        this.totalPlaytime += seconds;
        this.lastPlayed = Date.now();
    }

    clone() {
        const cloned = new PlayerProfile(this.id, this.name);
        cloned.createdAt = this.createdAt;
        cloned.lastPlayed = this.lastPlayed;
        cloned.totalPlaytime = this.totalPlaytime;
        cloned.level = this.level;
        cloned.xp = this.xp;
        cloned.settings = this.settings.clone();
        cloned.keyBindings = this.keyBindings.clone();
        cloned.videoSettings = this.videoSettings.clone();
        cloned.audioSettings = this.audioSettings.clone();
        cloned.controlSettings = this.controlSettings.clone();
        cloned.cosmetics = this.cosmetics.clone();
        cloned.inventory = this.inventory.clone();
        cloned.contracts = [...this.contracts];
        cloned.missions = [...this.missions];
        cloned.friends = [...this.friends];
        cloned.statistics = this.statistics;
        cloned.achievements = [...this.achievements];
        cloned.unlockedItems = [...this.unlockedItems];
        cloned.crateKeys = this.crateKeys;
        cloned.currency = { ...this.currency };
        cloned.preferences = this.preferences.clone();
        return cloned;
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            createdAt: this.createdAt,
            lastPlayed: this.lastPlayed,
            totalPlaytime: this.totalPlaytime,
            level: this.level,
            xp: this.xp,
            settings: this.settings.serialize(),
            keyBindings: this.keyBindings.serialize(),
            videoSettings: this.videoSettings.serialize(),
            audioSettings: this.audioSettings.serialize(),
            controlSettings: this.controlSettings.serialize(),
            cosmetics: this.cosmetics.serialize(),
            inventory: this.inventory.serialize(),
            contracts: this.contracts,
            missions: this.missions,
            friends: this.friends,
            statistics: this.statistics,
            achievements: this.achievements,
            unlockedItems: this.unlockedItems,
            crateKeys: this.crateKeys,
            currency: this.currency,
            preferences: this.preferences.serialize()
        };
    }

    deserialize(data) {
        this.id = data.id;
        this.name = data.name;
        this.createdAt = data.createdAt;
        this.lastPlayed = data.lastPlayed;
        this.totalPlaytime = data.totalPlaytime;
        this.level = data.level;
        this.xp = data.xp;
        this.settings = new ProfileSettings();
        this.settings.deserialize(data.settings || {});
        this.keyBindings = new KeyBindings();
        this.keyBindings.deserialize(data.keyBindings || {});
        this.videoSettings = new VideoSettings();
        this.videoSettings.deserialize(data.videoSettings || {});
        this.audioSettings = new AudioSettings();
        this.audioSettings.deserialize(data.audioSettings || {});
        this.controlSettings = new ControlSettings();
        this.controlSettings.deserialize(data.controlSettings || {});
        this.cosmetics = new CosmeticsInventory();
        this.cosmetics.deserialize(data.cosmetics || {});
        this.inventory = new Inventory();
        this.inventory.deserialize(data.inventory || {});
        this.contracts = data.contracts || [];
        this.missions = data.missions || [];
        this.friends = data.friends || [];
        this.statistics = data.statistics;
        this.achievements = data.achievements || [];
        this.unlockedItems = data.unlockedItems || [];
        this.crateKeys = data.crateKeys || 0;
        this.currency = data.currency || { coins: 0, points: 0, scrap: 0 };
        this.preferences = new PlayerPreferences();
        this.preferences.deserialize(data.preferences || {});
    }
}

export class ProfileSettings {
    constructor() {
        this.language = 'en';
        this.region = 'US';
        this.difficulty = 'normal';
        this.mouseSensitivity = 2.0;
        this.zoomSensitivity = 1.0;
        this.invertMouse = false;
        this.invertScroll = false;
        this.rawInput = true;
        this.windowsPointerSpeed = false;
        this.autoJump = false;
        autoReload = true;
        this.alwaysShowAvatars = true;
        this.showFPS = true;
        this.showPing = true;
        this.radarZoom = 1.0;
        this.bombTimer = true;
        this.grenadeTrajectories = false;
        this.buyMenuAlerts = true;
        this.muteEnemies = false;
        this.autoAcceptMatch = false;
        this.spectateEnemies = true;
        this.crosshairStyle = 'default';
        this.crosshairColor = '#00FF00';
        this.crosshairSize = 'medium';
        this.dynamicCrosshair = true;
        this.sniperScopeCrosshair = true;
        this.netGraph = false;
        this.fogOfWar = true;
        this.tutorialHints = true;
        this.chatWheelEnabled = true;
        this.voiceChatEnabled = true;
        this.pushToTalk = true;
        this.voiceVolume = 80;
        this.subtitles = false;
        this.minimapStyle = 'standard';
        this.damageIndicator = true;
        this.hitMarkers = true;
        this.killFeed = true;
        this.deathCam = true;
        this.spectatorList = true;
        this.autoScreenshot = false;
        this.autoRecordDemo = false;
        this.streamerMode = false;
    }

    clone() {
        const cloned = new ProfileSettings();
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

export class KeyBindings {
    constructor() {
        this.primary = {
            forward: 'W',
            backward: 'S',
            left: 'A',
            right: 'D',
            jump: 'SPACE',
            crouch: 'CTRL',
            walk: 'SHIFT',
            sprint: 'ALT',
            reload: 'R',
            use: 'E',
            drop: 'G',
            lastWeapon: 'Q',
            nextWeapon: 'MWHEELDOWN',
            previousWeapon: 'MWHEELUP',
            inspect: 'F',
            taunt: 'Z',
            dropWeapon: 'H'
        };
        this.secondary = {
            slot1: '1',
            slot2: '2',
            slot3: '3',
            slot4: '4',
            slot5: '5',
            slot6: '6',
            slot7: '7',
            slot8: '8',
            slot9: '9',
            slot10: '0'
        };
        this.grenades = {
            flashbang: 'F',
            smoke: 'C',
            he: '3',
            molotov: '4',
            decoy: '5'
        };
        this.utilities = {
            buyMenu: 'B',
            scoreboard: 'TAB',
            showMenu: 'ESC',
            teamMenu: 'K',
            chat: 'T',
            teamChat: 'Y',
            voiceChat: 'V',
            callVote: 'U',
            pause: 'PAUSE'
        };
        this.misc = {
            screenshot: 'F12',
            console: '`',
            demoRecord: 'SHIFT+F10'
        };
    }

    clone() {
        const cloned = new KeyBindings();
        cloned.primary = { ...this.primary };
        cloned.secondary = { ...this.secondary };
        cloned.grenades = { ...this.grenades };
        cloned.utilities = { ...this.utilities };
        cloned.misc = { ...this.misc };
        return cloned;
    }

    serialize() {
        return {
            primary: this.primary,
            secondary: this.secondary,
            grenades: this.grenades,
            utilities: this.utilities,
            misc: this.misc
        };
    }

    deserialize(data) {
        if (data.primary) this.primary = { ...data.primary };
        if (data.secondary) this.secondary = { ...data.secondary };
        if (data.grenades) this.grenades = { ...data.grenades };
        if (data.utilities) this.utilities = { ...data.utilities };
        if (data.misc) this.misc = { ...data.misc };
    }
}

export class VideoSettings {
    constructor() {
        this.resolutionWidth = 1920;
        this.resolutionHeight = 1080;
        this.displayMode = 'fullscreen';
        this.aspectRatio = '16:9';
        this.refreshRate = 144;
        this.vSync = false;
        this.nvidiaGSync = false;
        this.amdFreeSync = false;
        this.qualityPreset = 'medium';
        this.shadowQuality = 'medium';
        this.textureQuality = 'high';
        this.effectQuality = 'medium';
        this.shaderQuality = 'medium';
        this.antiAliasing = 'FXAA';
        this.antiAliasingQuality = 'medium';
        this.anisotropicFiltering = 16;
        this.ambientOcclusion = true;
        this.bloom = true;
        this.motionBlur = false;
        this.depthOfField = false;
        this.hdr = false;
        this.filmGrain = 0;
        this.vignette = true;
        this.colorBlindnessCorrection = false;
        this.colorBlindnessType = 'none';
        this.fpsLimit = 0;
        this.gpuUsageLimit = 100;
        this.multiCoreRendering = true;
        this.textureStreaming = true;
        this.lapixaFiltering = true;
        this.tiledBackgrounds = 'high';
    }

    clone() {
        const cloned = new VideoSettings();
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

export class AudioSettings {
    constructor() {
        this.masterVolume = 80;
        this.effectsVolume = 80;
        this.musicVolume = 50;
        this.voiceChatVolume = 80;
        this.uiVolume = 70;
        this.footstepsVolume = 90;
        this.weaponsVolume = 100;
        this.ambientVolume = 60;
        this.headphonesMode = 'headphones';
        this.surroundSound = true;
        this.hrtfMode = 'hrtf';
        this.voiceChatDevice = 'default';
        this.microphoneVolume = 80;
        this.microphoneDevice = 'default';
        this.pushToTalkKey = 'CAPITAL';
        this.voiceProcessing = true;
        this.noiseGate = true;
        this.compressor = true;
        this.ducking = true;
        this.duckingAmount = 50;
        this.hitSound = true;
        this.killSound = true;
        this.bombBeepVolume = 100;
        this.footstepSounds = true;
        this.weaponSounds = true;
        this.ui sounds = true;
        this.musicEnabled = true;
        this.ambientSoundsEnabled = true;
        this.voResponse = true;
        this.directionalMix = 100;
    }

    clone() {
        const cloned = new AudioSettings();
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

export class ControlSettings {
    constructor() {
        this.mouseAcceleration = false;
        this.mouseFilter = false;
        this.joystickEnabled = false;
        this.joystickLookSpeed = 50;
        this.joystickMoveSpeed = 50;
        this.joystickDeadzone = 10;
        this.touchControlsEnabled = false;
        this.touchLookSensitivity = 50;
        this.touchMoveSensitivity = 50;
        this.gyroEnabled = false;
        this.gyroSensitivity = 50;
        this.triggerThreshold = 50;
        this.adaptiveTrigger = false;
        this.aimAssist = true;
        this.autoAim = false;
        this.autoWallBang = false;
        this.autoFire = false;
        this.bunnyHop = false;
        this.quickReload = true;
        this.fastSwitch = true;
        this.nadeThrow = 'quick';
        this.buyWheelPreset = 'default';
    }

    clone() {
        const cloned = new ControlSettings();
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

export class PlayerPreferences {
    constructor() {
        this.autoJoinVoice = false;
        this.showFriendOnline = true;
        this.showFriendInGame = true;
        this.allowFriendInvites = true;
        this.allowFriendMessages = true;
        this.partyPrivacy = 'public';
        this.matchMakingRegion = 'auto';
        this.pingLimit = 100;
        this.maxAcceptableLatency = 150;
        this.preferLocalServer = false;
        this.allowSpectating = true;
        this.autoDemoRecord = false;
        this.hltvReplay = true;
        this.twitchStreamerMode = false;
        this.twitchStreamerKey = '';
        this.autoUpdateProfile = true;
        this.shareReplayToTwitch = false;
        this.shareReplayToYouTube = false;
        this.shareStatsPublicly = true;
        this.showNametag = true;
        this.showPlayerLevel = true;
        this.showRank = true;
        this.enableNotifications = true;
        this.notificationSound = true;
        this.emailNotifications = false;
    }

    clone() {
        const cloned = new PlayerPreferences();
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

export class Inventory {
    constructor() {
        this.items = new Map();
        this.weapons = [];
        this.cases = [];
        this.keys = [];
        this.stickers = [];
        this.patches = [];
        this.graphics = [];
        this.sprays = [];
        this.musickits = [];
        this.coins = [];
        this.tickets = [];
        this.vanityItems = [];
        this.quality = {
            consumer: [],
            industrial: [],
            milspec: [],
            restricted: [],
            classified: [],
            covert: []
        };
    }

    addItem(item, quantity = 1) {
        if (this.items.has(item.id)) {
            const existing = this.items.get(item.id);
            existing.quantity += quantity;
        } else {
            this.items.set(item.id, { ...item, quantity });
        }
    }

    removeItem(itemId, quantity = 1) {
        if (this.items.has(itemId)) {
            const item = this.items.get(itemId);
            item.quantity -= quantity;
            if (item.quantity <= 0) {
                this.items.delete(itemId);
            }
            return true;
        }
        return false;
    }

    hasItem(itemId) {
        return this.items.has(itemId);
    }

    getItemCount(itemId) {
        const item = this.items.get(itemId);
        return item ? item.quantity : 0;
    }

    getAllItems() {
        return Array.from(this.items.values());
    }

    clone() {
        const cloned = new Inventory();
        cloned.items = new Map(this.items);
        cloned.weapons = [...this.weapons];
        cloned.cases = [...this.cases];
        cloned.keys = [...this.keys];
        cloned.stickers = [...this.stickers];
        cloned.patches = [...this.patches];
        cloned.graphics = [...this.graphics];
        cloned.sprays = [...this.sprays];
        cloned.musickits = [...this.musickits];
        cloned.coins = [...this.coins];
        cloned.tickets = [...this.tickets];
        cloned.vanityItems = [...this.vanityItems];
        return cloned;
    }

    serialize() {
        return {
            items: Array.from(this.items.entries()),
            weapons: this.weapons,
            cases: this.cases,
            keys: this.keys,
            stickers: this.stickers,
            patches: this.patches,
            graphics: this.graphics,
            sprays: this.sprays,
            musickits: this.musickits,
            coins: this.coins,
            tickets: this.tickets,
            vanityItems: this.vanityItems,
            quality: this.quality
        };
    }

    deserialize(data) {
        if (data.items) this.items = new Map(data.items);
        this.weapons = data.weapons || [];
        this.cases = data.cases || [];
        this.keys = data.keys || [];
        this.stickers = data.stickers || [];
        this.patches = data.patches || [];
        this.graphics = data.graphics || [];
        this.sprays = data.sprays || [];
        this.musickits = data.musickits || [];
        this.coins = data.coins || [];
        this.tickets = data.tickets || [];
        this.vanityItems = data.vanityItems || [];
        this.quality = data.quality || {
            consumer: [],
            industrial: [],
            milspec: [],
            restricted: [],
            classified: [],
            covert: []
        };
    }
}

export class CosmeticsInventory {
    constructor() {
        this.agents = [];
        this.weaponSkins = new Map();
        this.gloves = [];
        this.stickers = [];
        this.patches = [];
        this.graffiti = [];
        this.musickits = [];
        this.spraySeeds = [];
        this.crosshairPresets = [];
        this.viewModelSkins = [];
        this.ambientPacks = [];
        this.playerCards = [];
        this.borders = [];
        this.favoriteWeaponskins = [];
        this.equippedLoadouts = new Map();
    }

    equipSkin(weaponId, skinId) {
        this.weaponSkins.set(weaponId, skinId);
    }

    unequipSkin(weaponId) {
        this.weaponSkins.delete(weaponId);
    }

    getEquippedSkin(weaponId) {
        return this.weaponSkins.get(weaponId);
    }

    addLoadout(name, loadout) {
        this.equippedLoadouts.set(name, loadout);
    }

    clone() {
        const cloned = new CosmeticsInventory();
        cloned.agents = [...this.agents];
        cloned.weaponSkins = new Map(this.weaponSkins);
        cloned.gloves = [...this.gloves];
        cloned.stickers = [...this.stickers];
        cloned.patches = [...this.patches];
        cloned.graffiti = [...this.graffiti];
        cloned.musickits = [...this.musickits];
        cloned.spraySeeds = [...this.spraySeeds];
        cloned.crosshairPresets = [...this.crosshairPresets];
        cloned.viewModelSkins = [...this.viewModelSkins];
        cloned.ambientPacks = [...this.ambientPacks];
        cloned.playerCards = [...this.playerCards];
        cloned.borders = [...this.borders];
        cloned.favoriteWeaponskins = [...this.favoriteWeaponskins];
        cloned.equippedLoadouts = new Map(this.equippedLoadouts);
        return cloned;
    }

    serialize() {
        return {
            agents: this.agents,
            weaponSkins: Array.from(this.weaponSkins.entries()),
            gloves: this.gloves,
            stickers: this.stickers,
            patches: this.patches,
            graffiti: this.graffiti,
            musickits: this.musickits,
            spraySeeds: this.spraySeeds,
            crosshairPresets: this.crosshairPresets,
            viewModelSkins: this.viewModelSkins,
            ambientPacks: this.ambientPacks,
            playerCards: this.playerCards,
            borders: this.borders,
            favoriteWeaponskins: this.favoriteWeaponskins,
            equippedLoadouts: Array.from(this.equippedLoadouts.entries())
        };
    }

    deserialize(data) {
        this.agents = data.agents || [];
        this.weaponSkins = new Map(data.weaponSkins || []);
        this.gloves = data.gloves || [];
        this.stickers = data.stickers || [];
        this.patches = data.patches || [];
        this.graffiti = data.graffiti || [];
        this.musickits = data.musickits || [];
        this.spraySeeds = data.spraySeeds || [];
        this.crosshairPresets = data.crosshairPresets || [];
        this.viewModelSkins = data.viewModelSkins || [];
        this.ambientPacks = data.ambientPacks || [];
        this.playerCards = data.playerCards || [];
        this.borders = data.borders || [];
        this.favoriteWeaponskins = data.favoriteWeaponskins || [];
        this.equippedLoadouts = new Map(data.equippedLoadouts || []);
    }
}

export class SaveManager {
    constructor(options = {}) {
        this.storage = options.storage || localStorage;
        this.compressionEnabled = options.compressionEnabled ?? true;
        this.encryptionEnabled = options.encryptionEnabled ?? false;
        this.autoSaveEnabled = options.autoSaveEnabled ?? true;
        this.autoSaveInterval = options.autoSaveInterval || 60000;
        this.maxSlots = options.maxSlots || 10;
        this.backupEnabled = options.backupEnabled ?? true;
        this.maxBackups = options.maxBackups || 5;
        this.version = options.version || '1.0.0';
        this.slots = new Map();
        this.autoSaveTimer = null;
        this.cloudSyncEnabled = false;
        this.cloudProvider = null;
        this.lastSave = null;
        this.lastLoad = null;
        this.saveQueue = [];
        this.loadQueue = [];
        this.isProcessing = false;
        this.eventListeners = new Map();
    }

    async initialize() {
        await this.loadSlotList();
        if (this.autoSaveEnabled) {
            this.startAutoSave();
        }
        return this;
    }

    async save(slotId, data, metadata = {}, options = {}) {
        const saveData = {
            version: this.version,
            timestamp: Date.now(),
            slotId,
            metadata,
            data,
            checksum: null
        };

        if (this.compressionEnabled) {
            saveData.data = await this.compress(JSON.stringify(data));
        }

        if (options.encrypt || this.encryptionEnabled) {
            saveData.data = await this.encrypt(saveData.data);
            saveData.encrypted = true;
        }

        saveData.checksum = await this.calculateChecksum(saveData.data);

        const slotData = {
            metadata: {
                ...metadata,
                slotId,
                type: metadata.type || SaveType.FULL,
                timestamp: saveData.timestamp,
                version: this.version,
                size: JSON.stringify(saveData.data).length,
                playtime: metadata.playtime || 0,
                level: metadata.level || 1,
                mapId: metadata.mapId || null,
                matchId: metadata.matchId || null
            },
            data: saveData.data,
            checksum: saveData.checksum,
            encrypted: saveData.encrypted || false
        };

        try {
            this.storage.setItem(`save_${slotId}`, JSON.stringify(slotData));
            this.slots.set(slotId, new SaveMetadata().serialize());
            await this.saveSlotList();

            if (this.backupEnabled) {
                await this.createBackup(slotId);
            }

            this.lastSave = Date.now();
            this.emit('save', { slotId, timestamp: this.lastSave });
            return true;
        } catch (error) {
            this.emit('error', { type: 'save', slotId, error });
            return false;
        }
    }

    async load(slotId, options = {}) {
        const priority = options.priority || LoadPriority.NORMAL;

        if (priority === LoadPriority.LOW) {
            return this.queueLoad(slotId, options);
        }

        try {
            const slotData = this.storage.getItem(`save_${slotId}`);
            if (!slotData) {
                throw new Error(`Save slot ${slotId} not found`);
            }

            const parsed = JSON.parse(slotData);
            let data = parsed.data;

            if (parsed.encrypted || options.decrypt) {
                data = await this.decrypt(data);
            }

            if (this.compressionEnabled) {
                const decompressed = await this.decompress(data);
                data = JSON.parse(decompressed);
            }

            const checksum = await this.calculateChecksum(parsed.data);
            if (checksum !== parsed.checksum) {
                throw new Error('Save file checksum mismatch - file may be corrupted');
            }

            this.lastLoad = Date.now();
            this.emit('load', { slotId, timestamp: this.lastLoad });

            return {
                data,
                metadata: parsed.metadata
            };
        } catch (error) {
            this.emit('error', { type: 'load', slotId, error });
            throw error;
        }
    }

    async quickSave(data, metadata = {}) {
        return this.save(SaveSlot.AUTO, data, {
            ...metadata,
            type: SaveType.QUICK
        });
    }

    async quickLoad(options = {}) {
        return this.load(SaveSlot.AUTO, options);
    }

    async createCheckpoint(data, metadata = {}) {
        const checkpointSlot = `checkpoint_${Date.now()}`;
        return this.save(checkpointSlot, data, {
            ...metadata,
            type: SaveType.CHECKPOINT
        });
    }

    async delete(slotId) {
        this.storage.removeItem(`save_${slotId}`);
        this.slots.delete(slotId);
        await this.saveSlotList();
        this.emit('delete', { slotId });
    }

    async copySlot(sourceId, destId) {
        const sourceData = this.storage.getItem(`save_${sourceId}`);
        if (!sourceData) {
            throw new Error(`Source slot ${sourceId} not found`);
        }
        this.storage.setItem(`save_${destId}`, sourceData);
        const metadata = await this.getSlotMetadata(destId);
        this.slots.set(destId, metadata);
        await this.saveSlotList();
    }

    async renameSlot(slotId, newName) {
        const slotData = this.storage.getItem(`save_${slotId}`);
        if (!slotData) {
            throw new Error(`Slot ${slotId} not found`);
        }
        const parsed = JSON.parse(slotData);
        parsed.metadata.description = newName;
        this.storage.setItem(`save_${slotId}`, JSON.stringify(parsed));
        const metadata = await this.getSlotMetadata(slotId);
        this.slots.set(slotId, metadata);
        await this.saveSlotList();
    }

    async getSlotMetadata(slotId) {
        const slotData = this.storage.getItem(`save_${slotId}`);
        if (!slotData) {
            return null;
        }
        const parsed = JSON.parse(slotData);
        return parsed.metadata;
    }

    async getAllSlotMetadata() {
        const metadataList = [];
        for (const slotId of this.slots.keys()) {
            const metadata = await this.getSlotMetadata(slotId);
            if (metadata) {
                metadataList.push(metadata);
            }
        }
        return metadataList.sort((a, b) => b.timestamp - a.timestamp);
    }

    async loadSlotList() {
        const listData = this.storage.getItem('save_slots');
        if (listData) {
            const list = JSON.parse(listData);
            this.slots = new Map(list);
        }
    }

    async saveSlotList() {
        this.storage.setItem('save_slots', JSON.stringify(Array.from(this.slots.entries())));
    }

    async createBackup(slotId) {
        const slotData = this.storage.getItem(`save_${slotId}`);
        if (!slotData) return;

        const backupId = `backup_${slotId}_${Date.now()}`;
        this.storage.setItem(backupId, slotData);

        const backups = await this.getBackups(slotId);
        if (backups.length > this.maxBackups) {
            const toDelete = backups.slice(this.maxBackups);
            for (const backup of toDelete) {
                this.storage.removeItem(backup.id);
            }
        }
    }

    async getBackups(slotId) {
        const backups = [];
        for (let i = 0; i < this.storage.length; i++) {
            const key = this.storage.key(i);
            if (key.startsWith(`backup_${slotId}_`)) {
                const data = this.storage.getItem(key);
                if (data) {
                    const parsed = JSON.parse(data);
                    backups.push({
                        id: key,
                        timestamp: parsed.metadata.timestamp,
                        size: data.length
                    });
                }
            }
        }
        return backups.sort((a, b) => b.timestamp - a.timestamp);
    }

    async restoreBackup(backupId, slotId) {
        const backupData = this.storage.getItem(backupId);
        if (!backupData) {
            throw new Error(`Backup ${backupId} not found`);
        }
        this.storage.setItem(`save_${slotId}`, backupData);
        const metadata = await this.getSlotMetadata(slotId);
        this.slots.set(slotId, metadata);
        await this.saveSlotList();
    }

    async deleteBackup(backupId) {
        this.storage.removeItem(backupId);
    }

    queueSave(slotId, data, metadata, options) {
        this.saveQueue.push({ slotId, data, metadata, options });
        this.processSaveQueue();
    }

    queueLoad(slotId, options) {
        return new Promise((resolve, reject) => {
            this.loadQueue.push({ slotId, options, resolve, reject });
            this.processLoadQueue();
        });
    }

    async processSaveQueue() {
        if (this.isProcessing || this.saveQueue.length === 0) return;
        this.isProcessing = true;

        const item = this.saveQueue.shift();
        await this.save(item.slotId, item.data, item.metadata, item.options);
        this.isProcessing = false;

        if (this.saveQueue.length > 0) {
            this.processSaveQueue();
        }
    }

    async processLoadQueue() {
        if (this.isProcessing || this.loadQueue.length === 0) return;
        this.isProcessing = true;

        const item = this.loadQueue.shift();
        try {
            const result = await this.load(item.slotId, item.options);
            item.resolve(result);
        } catch (error) {
            item.reject(error);
        }
        this.isProcessing = false;

        if (this.loadQueue.length > 0) {
            this.processLoadQueue();
        }
    }

    async compress(data) {
        const encoder = new TextEncoder();
        const encoded = encoder.encode(data);
        const compressed = await this.pako.deflate(encoded);
        return this.arrayBufferToBase64(compressed);
    }

    async decompress(data) {
        const compressed = this.base64ToArrayBuffer(data);
        const decompressed = await this.pako.inflate(compressed, { to: 'string' });
        const decoder = new TextDecoder();
        return decoder.decode(decompressed);
    }

    async encrypt(data) {
        const encoder = new TextEncoder();
        const encoded = encoder.encode(data);
        const key = await this.getEncryptionKey();
        const encrypted = await this.crypto.subtle.encrypt(
            { name: 'AES-GCM' },
            key,
            encoded
        );
        return this.arrayBufferToBase64(encrypted);
    }

    async decrypt(data) {
        const encrypted = this.base64ToArrayBuffer(data);
        const key = await this.getEncryptionKey();
        const decrypted = await this.crypto.subtle.decrypt(
            { name: 'AES-GCM' },
            key,
            encrypted
        );
        const decoder = new TextDecoder();
        return decoder.decode(decrypted);
    }

    async getEncryptionKey() {
        const storedKey = sessionStorage.getItem('encryption_key');
        if (storedKey) {
            return this.base64ToArrayBuffer(storedKey);
        }
        const key = await this.crypto.subtle.generateKey(
            { name: 'AES-GCM', length: 256 },
            true,
            ['encrypt', 'decrypt']
        );
        const exported = await this.crypto.subtle.exportKey('raw', key);
        sessionStorage.setItem('encryption_key', this.arrayBufferToBase64(exported));
        return exported;
    }

    async calculateChecksum(data) {
        const encoder = new TextEncoder();
        const dataBuffer = encoder.encode(data);
        const hashBuffer = await this.crypto.subtle.digest('SHA-256', dataBuffer);
        return this.arrayBufferToBase64(hashBuffer);
    }

    arrayBufferToBase64(buffer) {
        const bytes = new Uint8Array(buffer);
        let binary = '';
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return btoa(binary);
    }

    base64ToArrayBuffer(base64) {
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes.buffer;
    }

    startAutoSave() {
        if (this.autoSaveTimer) {
            clearInterval(this.autoSaveTimer);
        }
        this.autoSaveTimer = setInterval(() => {
            this.emit('autosave', { timestamp: Date.now() });
        }, this.autoSaveInterval);
    }

    stopAutoSave() {
        if (this.autoSaveTimer) {
            clearInterval(this.autoSaveTimer);
            this.autoSaveTimer = null;
        }
    }

    exportToFile(slotId) {
        const slotData = this.storage.getItem(`save_${slotId}`);
        if (!slotData) {
            throw new Error(`Slot ${slotId} not found`);
        }
        const blob = new Blob([slotData], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `save_${slotId}_${Date.now()}.json`;
        a.click();
        URL.revokeObjectURL(url);
    }

    async importFromFile(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = async (e) => {
                try {
                    const data = JSON.parse(e.target.result);
                    const slotId = data.metadata.slotId || `imported_${Date.now()}`;
                    this.storage.setItem(`save_${slotId}`, JSON.stringify(data));
                    const metadata = await this.getSlotMetadata(slotId);
                    this.slots.set(slotId, metadata);
                    await this.saveSlotList();
                    resolve({ slotId, metadata });
                } catch (error) {
                    reject(error);
                }
            };
            reader.onerror = reject;
            reader.readAsText(file);
        });
    }

    async syncToCloud(provider, credentials) {
        this.cloudSyncEnabled = true;
        this.cloudProvider = provider;
        const cloudData = {
            slots: Array.from(this.slots.entries()),
            saves: {}
        };
        for (const slotId of this.slots.keys()) {
            cloudData.saves[slotId] = this.storage.getItem(`save_${slotId}`);
        }
        await provider.upload('game_save', cloudData);
        this.emit('cloudSync', { success: true });
    }

    async syncFromCloud(provider) {
        this.cloudSyncEnabled = true;
        this.cloudProvider = provider;
        const cloudData = await provider.download('game_save');
        if (cloudData) {
            this.slots = new Map(cloudData.slots);
            for (const [slotId, data] of Object.entries(cloudData.saves)) {
                this.storage.setItem(`save_${slotId}`, data);
            }
            await this.saveSlotList();
            this.emit('cloudSync', { success: true, loaded: true });
        }
    }

    on(event, callback) {
        if (!this.eventListeners.has(event)) {
            this.eventListeners.set(event, []);
        }
        this.eventListeners.get(event).push(callback);
    }

    off(event, callback) {
        if (this.eventListeners.has(event)) {
            const callbacks = this.eventListeners.get(event);
            const index = callbacks.indexOf(callback);
            if (index >= 0) {
                callbacks.splice(index, 1);
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

    clearAllData() {
        const keysToRemove = [];
        for (let i = 0; i < this.storage.length; i++) {
            const key = this.storage.key(i);
            if (key.startsWith('save_') || key.startsWith('backup_')) {
                keysToRemove.push(key);
            }
        }
        for (const key of keysToRemove) {
            this.storage.removeItem(key);
        }
        this.slots.clear();
        this.saveSlotList();
    }

    getStorageUsage() {
        let totalSize = 0;
        for (let i = 0; i < this.storage.length; i++) {
            const key = this.storage.key(i);
            if (key.startsWith('save_') || key.startsWith('backup_')) {
                const value = this.storage.getItem(key);
                totalSize += key.length + value.length;
            }
        }
        return {
            totalBytes: totalSize,
            totalKB: (totalSize / 1024).toFixed(2),
            totalMB: (totalSize / (1024 * 1024)).toFixed(2)
        };
    }

    destroy() {
        this.stopAutoSave();
        this.saveQueue = [];
        this.loadQueue = [];
        this.eventListeners.clear();
    }
}

export class GameStateSerializer {
    constructor() {
        this.serializers = new Map();
        this.typeHandlers = new Map();
        this.registerDefaultHandlers();
    }

    registerDefaultHandlers() {
        this.registerHandler('Vector3', {
            serialize: (v) => ({ x: v.x, y: v.y, z: v.z }),
            deserialize: (data) => ({ x: data.x, y: data.y, z: data.z })
        });

        this.registerHandler('Quaternion', {
            serialize: (q) => ({ x: q.x, y: q.y, z: q.z, w: q.w }),
            deserialize: (data) => ({ x: data.x, y: data.y, z: data.z, w: data.w })
        });

        this.registerHandler('Matrix4', {
            serialize: (m) => ({ elements: Array.from(m.elements) }),
            deserialize: (data) => ({ elements: new Float32Array(data.elements) })
        });
    }

    registerHandler(type, handler) {
        this.typeHandlers.set(type, handler);
    }

    registerSerializer(componentType, serializer) {
        this.serializers.set(componentType, serializer);
    }

    canSerialize(obj) {
        if (obj === null || obj === undefined) return true;
        if (typeof obj === 'boolean' || typeof obj === 'number' || typeof obj === 'string') return true;
        if (Array.isArray(obj)) return true;
        if (typeof obj === 'object') {
            if (this.typeHandlers.has(obj.constructor.name)) return true;
            if (this.serializers.has(obj.constructor.name)) return true;
            return true;
        }
        return false;
    }

    serialize(obj, context = {}) {
        if (obj === null) return null;
        if (obj === undefined) return undefined;

        if (typeof obj === 'boolean' || typeof obj === 'number' || typeof obj === 'string') {
            return obj;
        }

        if (Array.isArray(obj)) {
            return obj.map(item => this.serialize(item, context));
        }

        if (obj instanceof Date) {
            return { __type: 'Date', value: obj.getTime() };
        }

        if (obj instanceof Map) {
            return {
                __type: 'Map',
                value: Array.from(obj.entries()).map(([k, v]) => [k, this.serialize(v, context)])
            };
        }

        if (obj instanceof Set) {
            return {
                __type: 'Set',
                value: Array.from(obj).map(item => this.serialize(item, context))
            };
        }

        if (obj instanceof Error) {
            return {
                __type: 'Error',
                message: obj.message,
                name: obj.name,
                stack: obj.stack
            };
        }

        if (obj.constructor && this.typeHandlers.has(obj.constructor.name)) {
            return {
                __type: obj.constructor.name,
                value: this.typeHandlers.get(obj.constructor.name).serialize(obj)
            };
        }

        if (obj.constructor && this.serializers.has(obj.constructor.name)) {
            return {
                __type: obj.constructor.name,
                value: this.serializers.get(obj.constructor.name).serialize(obj, context)
            };
        }

        const serialized = {};
        for (const [key, value] of Object.entries(obj)) {
            if (value !== undefined && value !== null) {
                if (key.startsWith('_') && !context.includePrivate) continue;
                if (typeof value === 'function') continue;
                serialized[key] = this.serialize(value, context);
            }
        }

        return serialized;
    }

    deserialize(data, context = {}) {
        if (data === null || data === undefined) return data;

        if (typeof data === 'boolean' || typeof data === 'number' || typeof data === 'string') {
            return data;
        }

        if (Array.isArray(data)) {
            return data.map(item => this.deserialize(item, context));
        }

        if (data.__type) {
            switch (data.__type) {
                case 'Date':
                    return new Date(data.value);
                case 'Map':
                    return new Map(data.value.map(([k, v]) => [k, this.deserialize(v, context)]));
                case 'Set':
                    return new Set(data.value.map(item => this.deserialize(item, context)));
                case 'Error':
                    const error = new Error(data.message);
                    error.name = data.name;
                    error.stack = data.stack;
                    return error;
            }

            if (this.typeHandlers.has(data.__type)) {
                return this.typeHandlers.get(data.__type).deserialize(data.value);
            }

            if (this.serializers.has(data.__type)) {
                return this.serializers.get(data.__type).deserialize(data.value, context);
            }
        }

        const deserialized = {};
        for (const [key, value] of Object.entries(data)) {
            deserialized[key] = this.deserialize(value, context);
        }

        return deserialized;
    }

    serializeGameState(state) {
        return this.serialize(state, { includePrivate: false });
    }

    deserializeGameState(data) {
        return this.deserialize(data);
    }
}

export class SettingsManager {
    constructor() {
        this.settings = new Map();
        this.defaults = new Map();
        this.listeners = new Map();
    }

    registerSetting(key, defaultValue, options = {}) {
        this.defaults.set(key, defaultValue);
        if (!this.settings.has(key)) {
            this.settings.set(key, defaultValue);
        }
        if (options.validator) {
            this.settings.get(key) = options.validator(this.settings.get(key));
        }
    }

    get(key, defaultValue = null) {
        if (this.settings.has(key)) {
            return this.settings.get(key);
        }
        if (this.defaults.has(key)) {
            return this.defaults.get(key);
        }
        return defaultValue;
    }

    set(key, value) {
        const oldValue = this.settings.get(key);
        this.settings.set(key, value);
        this.emit('change', { key, oldValue, newValue: value });
        if (this.listeners.has(key)) {
            for (const listener of this.listeners.get(key)) {
                listener(value, oldValue);
            }
        }
    }

    reset(key) {
        if (this.defaults.has(key)) {
            this.set(key, this.defaults.get(key));
        }
    }

    resetAll() {
        for (const [key, defaultValue] of this.defaults) {
            this.settings.set(key, defaultValue);
        }
        this.emit('reset');
    }

    has(key) {
        return this.settings.has(key) || this.defaults.has(key);
    }

    delete(key) {
        this.settings.delete(key);
    }

    on(key, listener) {
        if (!this.listeners.has(key)) {
            this.listeners.set(key, []);
        }
        this.listeners.get(key).push(listener);
    }

    off(key, listener) {
        if (this.listeners.has(key)) {
            const listeners = this.listeners.get(key);
            const index = listeners.indexOf(listener);
            if (index >= 0) {
                listeners.splice(index, 1);
            }
        }
    }

    emit(event, data) {
        if (this.listeners.has(event)) {
            for (const listener of this.listeners.get(event)) {
                listener(data);
            }
        }
    }

    export() {
        const exported = {};
        for (const [key, value] of this.settings) {
            exported[key] = value;
        }
        return exported;
    }

    import(data) {
        for (const [key, value] of Object.entries(data)) {
            if (this.defaults.has(key)) {
                this.set(key, value);
            }
        }
    }
}

export default {
    SaveSlot,
    SaveType,
    LoadPriority,
    SaveMetadata,
    ProfileManager,
    PlayerProfile,
    ProfileSettings,
    KeyBindings,
    VideoSettings,
    AudioSettings,
    ControlSettings,
    PlayerPreferences,
    Inventory,
    CosmeticsInventory,
    SaveManager,
    GameStateSerializer,
    SettingsManager
};

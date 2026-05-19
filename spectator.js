export const SpectatorCamera = {
    FREE: 'free',
    FOLLOW: 'follow',
    OVERVIEW: 'overview',
    FIRST_PERSON: 'first_person',
    THIRD_PERSON: 'third_person',
    FREEZE: 'freeze',
    POINT_OF_INTEREST: 'point_of_interest'
};

export const BroadcastQuality = {
    AUTO: 'auto',
    LOW: '360p',
    MEDIUM: '480p',
    HIGH: '720p',
    ULTRA: '1080p',
    MAX: '1440p',
    SOURCE: '4k'
};

export const StreamPlatform = {
    TWITCH: 'twitch',
    YOUTUBE: 'youtube',
    MIXER: 'mixer',
    FACEBOOK: 'facebook',
    CUSTOM: 'custom'
};

export class BroadcastSession {
    constructor(id, broadcasterId) {
        this.id = id;
        this.broadcasterId = broadcasterId;
        this.title = '';
        this.description = '';
        this.game = 'Counter-Strike';
        this.tags = [];
        this.category = 'FPS';
        this.thumbnail = '';
        this.previewUrl = '';
        this.streamUrl = '';
        this.ingestUrl = '';
        this.quality = BroadcastQuality.AUTO;
        this.bitrate = 4500;
        this.frameRate = 60;
        this.resolution = { width: 1920, height: 1080 };
        this.keyframeInterval = 2;
        this.audioCodec = 'aac';
        this.videoCodec = 'h264';
        this.audioBitrate = 128;
        this.state = 'idle';
        this.startedAt = null;
        this.endedAt = null;
        this.duration = 0;
        this.viewers = new Set();
        this.peakViewers = 0;
        this.totalViews = 0;
        this.averageViewers = 0;
        this.followers = 0;
        this.subscribers = 0;
        this.maxViewers = 10000;
        this.delay = 0;
        this.anticheatDelay = 60;
        this.chatEnabled = true;
        this.slowMode = false;
        this.slowModeDelay = 0;
        this.subMode = false;
        this.r9k = false;
        this.emotesOnly = false;
        this.followersOnly = false;
        this.followersOnlyDuration = 0;
        this.subscribersOnly = false;
        this.rerun = false;
        this.commercialEnabled = false;
        this.adSchedule = [];
        this.moderators = new Set();
        this.bannedWords = [];
        this.allowedWords = [];
        this.hostTarget = null;
        this.raidTarget = null;
        this.raiders = [];
        this.sponsors = [];
        this.overlays = new BroadcastOverlays();
        this.alerts = new AlertSystem();
        this.donations = [];
        this.subscriptions = [];
        this.bits = 0;
        this.bitsByUser = new Map();
        this.marquee = null;
        this.polls = [];
        this.predictions = [];
        this.goals = [];
        this.clips = [];
        this.vods = [];
        this.highlights = [];
        this.statistics = new BroadcastStatistics();
        this.analytics = new BroadcastAnalytics();
        this.monitization = new BroadcastMonitization();
        this.settings = new BroadcastSettings();
        this.cameras = [];
        this.activeCamera = 0;
        this.multiviewEnabled = false;
        this.multiviewLayout = '1x1';
        this.replayEnabled = false;
        this.replayBuffer = 60;
        this.scene = null;
        this.scenes = [];
        this.activeScene = 0;
        this.transitions = [];
        this.streamKey = '';
        this.streamId = null;
        this.channelId = null;
        this.platform = StreamPlatform.TWITCH;
        this.embedUrl = '';
        this.chatUrl = '';
        this.panels = [];
        this.playerCards = [];
        this.badges = [];
        this.emoteSets = [];
        this.commands = [];
        this.sounds = [];
        this.notifications = new NotificationSettings();
        this.locations = [];
        this.activeLocation = 0;
        this.bounties = [];
        this.activeBounty = null;
        this.isPremium = false;
        this.partnered = false;
        this.verified = false;
    }

    start() {
        this.state = 'live';
        this.startedAt = Date.now();
        this.statistics.recordStart();
    }

    stop() {
        this.state = 'offline';
        this.endedAt = Date.now();
        this.duration = this.endedAt - this.startedAt;
        this.calculateStatistics();
        this.statistics.recordEnd();
    }

    pause() {
        if (this.state === 'live') {
            this.state = 'paused';
            this.statistics.recordPause();
        }
    }

    resume() {
        if (this.state === 'paused') {
            this.state = 'live';
            this.statistics.recordResume();
        }
    }

    addViewer(viewerId) {
        this.viewers.add(viewerId);
        this.totalViews++;
        if (this.viewers.size > this.peakViewers) {
            this.peakViewers = this.viewers.size;
        }
        this.statistics.recordViewerJoin();
    }

    removeViewer(viewerId) {
        if (this.viewers.has(viewerId)) {
            this.viewers.delete(viewerId);
            this.statistics.recordViewerLeave();
            return true;
        }
        return false;
    }

    getViewerCount() {
        return this.viewers.size;
    }

    calculateStatistics() {
        if (this.duration > 0) {
            this.averageViewers = this.totalViews / (this.duration / 60000);
        }
    }

    setTitle(title) {
        this.title = title;
        this.statistics.recordTitleChange();
    }

    addTag(tag) {
        if (!this.tags.includes(tag) && this.tags.length < 5) {
            this.tags.push(tag);
        }
    }

    removeTag(tag) {
        const index = this.tags.indexOf(tag);
        if (index >= 0) {
            this.tags.splice(index, 1);
        }
    }

    setQuality(quality) {
        this.quality = quality;
        const qualitySettings = this.getQualitySettings(quality);
        this.bitrate = qualitySettings.bitrate;
        this.frameRate = qualitySettings.frameRate;
        this.resolution = qualitySettings.resolution;
    }

    getQualitySettings(quality) {
        const settings = {
            [BroadcastQuality.AUTO]: { bitrate: 6000, frameRate: 60, resolution: { width: 1920, height: 1080 } },
            [BroadcastQuality.LOW]: { bitrate: 1500, frameRate: 30, resolution: { width: 640, height: 360 } },
            [BroadcastQuality.MEDIUM]: { bitrate: 3000, frameRate: 30, resolution: { width: 854, height: 480 } },
            [BroadcastQuality.HIGH]: { bitrate: 4500, frameRate: 60, resolution: { width: 1280, height: 720 } },
            [BroadcastQuality.ULTRA]: { bitrate: 6000, frameRate: 60, resolution: { width: 1920, height: 1080 } },
            [BroadcastQuality.MAX]: { bitrate: 9000, frameRate: 60, resolution: { width: 2560, height: 1440 } },
            [BroadcastQuality.SOURCE]: { bitrate: 20000, frameRate: 60, resolution: { width: 3840, height: 2160 } }
        };
        return settings[quality] || settings[BroadcastQuality.AUTO];
    }

    addModerator(userId) {
        this.moderators.add(userId);
    }

    removeModerator(userId) {
        this.moderators.delete(userId);
    }

    isModerator(userId) {
        return this.moderators.has(userId);
    }

    addBannedWord(word) {
        if (!this.bannedWords.includes(word)) {
            this.bannedWords.push(word);
        }
    }

    removeBannedWord(word) {
        const index = this.bannedWords.indexOf(word);
        if (index >= 0) {
            this.bannedWords.splice(index, 1);
        }
    }

    enableSlowMode(delay) {
        this.slowMode = true;
        this.slowModeDelay = delay;
    }

    disableSlowMode() {
        this.slowMode = false;
        this.slowModeDelay = 0;
    }

    enableSubscribersOnly() {
        this.subscribersOnly = true;
    }

    disableSubscribersOnly() {
        this.subscribersOnly = false;
    }

    enableFollowersOnly(duration) {
        this.followersOnly = true;
        this.followersOnlyDuration = duration;
    }

    disableFollowersOnly() {
        this.followersOnly = false;
        this.followersOnlyDuration = 0;
    }

    hostChannel(channelId) {
        this.hostTarget = channelId;
        this.state = 'hosting';
    }

    unhost() {
        this.hostTarget = null;
        this.state = 'live';
    }

    raidChannel(channelId, viewerCount) {
        this.raidTarget = channelId;
        this.raiders = Array.from(this.viewers);
        this.viewers.clear();
        this.state = 'raiding';
    }

    startCommercial(duration) {
        this.state = 'commercial';
        this.commercialEndTime = Date.now() + (duration * 1000);
        this.statistics.recordCommercial(duration);
    }

    addPoll(poll) {
        this.polls.push({
            id: 'poll_' + Date.now(),
            ...poll,
            votes: new Map(),
            totalVotes: 0,
            startedAt: Date.now(),
            endedAt: null,
            status: 'active'
        });
    }

    endPoll(pollId) {
        const poll = this.polls.find(p => p.id === pollId);
        if (poll) {
            poll.status = 'ended';
            poll.endedAt = Date.now();
            poll.winner = this.getPollWinner(poll);
        }
    }

    getPollWinner(poll) {
        let maxVotes = 0;
        let winner = null;
        for (const [option, votes] of poll.votes) {
            if (votes > maxVotes) {
                maxVotes = votes;
                winner = option;
            }
        }
        return winner;
    }

    addPrediction(prediction) {
        this.predictions.push({
            id: 'prediction_' + Date.now(),
            ...prediction,
            points: new Map(),
            totalPoints: 0,
            participants: new Set(),
            startedAt: Date.now(),
            lockedAt: null,
            endedAt: null,
            status: 'active'
        });
    }

    addGoal(goal) {
        this.goals.push({
            id: 'goal_' + Date.now(),
            ...goal,
            current: 0,
            startedAt: Date.now(),
            completedAt: null,
            notifications: []
        });
    }

    updateGoal(goalId, current) {
        const goal = this.goals.find(g => g.id === goalId);
        if (goal) {
            const wasCompleted = goal.current >= goal.target;
            goal.current = current;
            if (!wasCompleted && current >= goal.target) {
                goal.completedAt = Date.now();
                this.alerts.triggerAlert('goal', goal);
            }
        }
    }

    receiveDonation(donation) {
        this.donations.push({
            id: 'donation_' + Date.now(),
            ...donation,
            timestamp: Date.now()
        });
        this.monitization.recordDonation(donation);
        this.statistics.recordDonation(donation);
        this.alerts.triggerAlert('donation', donation);
    }

    receiveSubscription(subscription) {
        this.subscriptions.push({
            id: 'sub_' + Date.now(),
            ...subscription,
            timestamp: Date.now()
        });
        this.monitization.recordSubscription(subscription);
        this.statistics.recordSubscription(subscription);
        this.alerts.triggerAlert('subscription', subscription);
    }

    receiveBits(bits, userId) {
        this.bits += bits;
        const current = this.bitsByUser.get(userId) || 0;
        this.bitsByUser.set(userId, current + bits);
        this.monitization.recordBits(bits);
        this.statistics.recordBits(bits);
    }

    createClip(data) {
        const clip = {
            id: 'clip_' + Date.now(),
            ...data,
            views: 0,
            duration: 0,
            createdAt: Date.now()
        };
        this.clips.push(clip);
        this.statistics.recordClipCreated();
        return clip;
    }

    createHighlight(data) {
        const highlight = {
            id: 'highlight_' + Date.now(),
            ...data,
            views: 0,
            createdAt: Date.now()
        };
        this.highlights.push(highlight);
        return highlight;
    }

    setScene(sceneId) {
        this.scene = sceneId;
    }

    addScene(scene) {
        this.scenes.push(scene);
    }

    switchScene(sceneId) {
        const index = this.scenes.findIndex(s => s.id === sceneId);
        if (index >= 0) {
            this.activeScene = index;
            this.scene = sceneId;
        }
    }

    addCamera(camera) {
        this.cameras.push(camera);
    }

    switchCamera(index) {
        if (index >= 0 && index < this.cameras.length) {
            this.activeCamera = index;
        }
    }

    serialize() {
        return {
            id: this.id,
            broadcasterId: this.broadcasterId,
            title: this.title,
            description: this.description,
            game: this.game,
            tags: this.tags,
            category: this.category,
            thumbnail: this.thumbnail,
            previewUrl: this.previewUrl,
            streamUrl: this.streamUrl,
            ingestUrl: this.ingestUrl,
            quality: this.quality,
            bitrate: this.bitrate,
            frameRate: this.frameRate,
            resolution: this.resolution,
            keyframeInterval: this.keyframeInterval,
            audioCodec: this.audioCodec,
            videoCodec: this.videoCodec,
            audioBitrate: this.audioBitrate,
            state: this.state,
            startedAt: this.startedAt,
            endedAt: this.endedAt,
            duration: this.duration,
            viewers: Array.from(this.viewers),
            peakViewers: this.peakViewers,
            totalViews: this.totalViews,
            averageViewers: this.averageViewers,
            followers: this.followers,
            subscribers: this.subscribers,
            maxViewers: this.maxViewers,
            delay: this.delay,
            anticheatDelay: this.anticheatDelay,
            chatEnabled: this.chatEnabled,
            slowMode: this.slowMode,
            slowModeDelay: this.slowModeDelay,
            subMode: this.subMode,
            r9k: this.r9k,
            emotesOnly: this.emotesOnly,
            followersOnly: this.followersOnly,
            followersOnlyDuration: this.followersOnlyDuration,
            subscribersOnly: this.subscribersOnly,
            rerun: this.rerun,
            commercialEnabled: this.commercialEnabled,
            adSchedule: this.adSchedule,
            moderators: Array.from(this.moderators),
            bannedWords: this.bannedWords,
            allowedWords: this.allowedWords,
            hostTarget: this.hostTarget,
            raidTarget: this.raidTarget,
            raiders: this.raiders,
            sponsors: this.sponsors,
            overlays: this.overlays.serialize(),
            alerts: this.alerts.serialize(),
            donations: this.donations,
            subscriptions: this.subscriptions,
            bits: this.bits,
            bitsByUser: Array.from(this.bitsByUser.entries()),
            marquee: this.marquee,
            polls: this.polls,
            predictions: this.predictions,
            goals: this.goals,
            clips: this.clips,
            vods: this.vods,
            highlights: this.highlights,
            statistics: this.statistics.serialize(),
            analytics: this.analytics.serialize(),
            monetization: this.monitization.serialize(),
            settings: this.settings.serialize(),
            cameras: this.cameras,
            activeCamera: this.activeCamera,
            multiviewEnabled: this.multiviewEnabled,
            multiviewLayout: this.multiviewLayout,
            replayEnabled: this.replayEnabled,
            replayBuffer: this.replayBuffer,
            scene: this.scene,
            scenes: this.scenes,
            activeScene: this.activeScene,
            transitions: this.transitions,
            streamKey: this.streamKey,
            streamId: this.streamId,
            channelId: this.channelId,
            platform: this.platform,
            embedUrl: this.embedUrl,
            chatUrl: this.chatUrl,
            panels: this.panels,
            playerCards: this.playerCards,
            badges: this.badges,
            emoteSets: this.emoteSets,
            commands: this.commands,
            sounds: this.sounds,
            notifications: this.notifications.serialize(),
            locations: this.locations,
            activeLocation: this.activeLocation,
            bounties: this.bounties,
            activeBounty: this.activeBounty,
            isPremium: this.isPremium,
            partnered: this.partnered,
            verified: this.verified
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.viewers = new Set(data.viewers || []);
        this.moderators = new Set(data.moderators || []);
        this.bitsByUser = new Map(data.bitsByUser || []);
        this.overlays = new BroadcastOverlays();
        this.overlays.deserialize(data.overlays || {});
        this.alerts = new AlertSystem();
        this.alerts.deserialize(data.alerts || {});
        this.statistics = new BroadcastStatistics();
        this.statistics.deserialize(data.statistics || {});
        this.analytics = new BroadcastAnalytics();
        this.analytics.deserialize(data.analytics || {});
        this.monitization = new BroadcastMonitization();
        this.monitization.deserialize(data.monetization || {});
        this.settings = new BroadcastSettings();
        this.settings.deserialize(data.settings || {});
        this.notifications = new NotificationSettings();
        this.notifications.deserialize(data.notifications || {});
    }
}

export class BroadcastSettings {
    constructor() {
        this.automated = false;
        this.autostartOnLaunch = false;
        this.autostopOnExit = false;
        this.recordWhileLive = true;
        this.recordOnly = false;
        this.enableLowLatency = false;
        this.enableDynBitrate = false;
        this.enableBframes = false;
        this.enableSceneSwitching = true;
        this.enableMicrophone = true;
        this.enableCamera = false;
        this.enableDesktopAudio = true;
        this.enableApplicationAudio = true;
        this.enableNoiseSuppression = true;
        this.enableNoiseGate = true;
        this.enableNoiseSuppressor = true;
        this.enableEchoCancellation = true;
        this.enableAutomaticGain = true;
        this.enablePushToMute = false;
        this.pushToMuteKey = null;
        this.enablePushToTalk = false;
        this.pushToTalkKey = null;
        this.stingerTransition = false;
        this.stingerFile = null;
        this.stingerTransitionDuration = 300;
        this.outputQuality = 'high';
        this.outputFormat = 'flv';
        this.filePath = '';
        this.maxFileSize = 2048;
        this.maxFileDuration = 0;
        this.enableAutoSplits = false;
        this.autoSplitInterval = 0;
        this.replayBufferSeconds = 30;
        this.enableReplayBuffer = false;
        this.replayBufferEvents = [];
    }

    clone() {
        const cloned = new BroadcastSettings();
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

export class BroadcastOverlays {
    constructor() {
        this.enabled = true;
        this.webcam = new WebcamOverlay();
        this.chat = new ChatOverlay();
        this.alerts = new AlertOverlay();
        this.stats = new StatsOverlay();
        this.clock = new ClockOverlay();
        this.chatBox = new ChatBoxOverlay();
        this.imageFiles = [];
        this.textSources = [];
        this.mediaSources = [];
        this.browserSources = [];
        this.colorSources = [];
        this.slideshowSources = [];
        this.gameCapture = null;
        this.windowCapture = null;
        this.displayCapture = null;
        this.cropSettings = new Map();
        this.colorCorrection = new ColorCorrectionSettings();
        this.transitions = new TransitionSettings();
    }

    clone() {
        const cloned = new BroadcastOverlays();
        Object.assign(cloned, this);
        return cloned;
    }

    serialize() {
        return {
            enabled: this.enabled,
            webcam: this.webcam.serialize(),
            chat: this.chat.serialize(),
            alerts: this.alerts.serialize(),
            stats: this.stats.serialize(),
            clock: this.clock.serialize(),
            chatBox: this.chatBox.serialize(),
            imageFiles: this.imageFiles,
            textSources: this.textSources,
            mediaSources: this.mediaSources,
            browserSources: this.browserSources,
            colorSources: this.colorSources,
            slideshowSources: this.slideshowSources,
            gameCapture: this.gameCapture,
            windowCapture: this.windowCapture,
            displayCapture: this.displayCapture,
            cropSettings: Array.from(this.cropSettings.entries()),
            colorCorrection: this.colorCorrection.serialize(),
            transitions: this.transitions.serialize()
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.webcam = new WebcamOverlay();
        this.webcam.deserialize(data.webcam || {});
        this.chat = new ChatOverlay();
        this.chat.deserialize(data.chat || {});
        this.alerts = new AlertOverlay();
        this.alerts.deserialize(data.alerts || {});
        this.stats = new StatsOverlay();
        this.stats.deserialize(data.stats || {});
        this.clock = new ClockOverlay();
        this.clock.deserialize(data.clock || {});
        this.chatBox = new ChatBoxOverlay();
        this.chatBox.deserialize(data.chatBox || {});
        this.colorCorrection = new ColorCorrectionSettings();
        this.colorCorrection.deserialize(data.colorCorrection || {});
        this.transitions = new TransitionSettings();
        this.transitions.deserialize(data.transitions || {});
        this.cropSettings = new Map(data.cropSettings || []);
    }
}

export class WebcamOverlay {
    constructor() {
        this.enabled = false;
        this.source = null;
        this.x = 0;
        this.y = 0;
        this.width = 320;
        this.height = 240;
        this.position = 'bottom-left';
        this.flipH = false;
        this.flipV = false;
        this.rotation = 0;
        this.crop = { top: 0, bottom: 0, left: 0, right: 0 };
        this.border = { enabled: false, color: '#ffffff', width: 2 };
        this.cornerRadius = 0;
        this.shadow = { enabled: false, x: 2, y: 2, size: 4, color: '#000000' };
        this.opacity = 100;
        this.blur = 0;
        this.grayscale = false;
        this.colorFilter = null;
        this.badge = { enabled: false, image: null, position: 'top-right' };
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class ChatOverlay {
    constructor() {
        this.enabled = true;
        this.position = 'bottom-right';
        this.width = 350;
        this.height = 400;
        this.fontSize = 14;
        this.fontFamily = 'Arial';
        this.fontColor = '#ffffff';
        this.backgroundColor = 'rgba(0, 0, 0, 0.6)';
        this.backgroundImage = null;
        this.borderRadius = 4;
        this.padding = 10;
        this.scrollbar = true;
        this.showTimestamps = false;
        this.showBadges = true;
        this.showEmotes = true;
        this.showBits = true;
        this.showSubscriptionBadge = true;
        this.showModeratorBadge = true;
        this.showVipBadge = true;
        this.maxMessages = 100;
        this.fadeOutTime = 0;
        this.animation = 'slide';
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class AlertOverlay {
    constructor() {
        this.enabled = true;
        this.position = 'center';
        this.width = 600;
        this.height = 200;
        this.fontSize = 24;
        this.fontFamily = 'Arial';
        this.fontColor = '#ffffff';
        this.backgroundColor = 'rgba(0, 0, 0, 0.8)';
        this.borderRadius = 8;
        this.border = { enabled: true, color: '#6441a5', width: 4 };
        this.animation = 'bounce';
        this.soundEnabled = true;
        this.soundVolume = 100;
        this.duration = 5000;
        this.queue = [];
        this.maxQueue = 10;
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class StatsOverlay {
    constructor() {
        this.enabled = false;
        this.position = 'top-right';
        this.fontSize = 16;
        this.fontFamily = 'Arial';
        this.fontColor = '#ffffff';
        this.backgroundColor = 'rgba(0, 0, 0, 0.6)';
        this.borderRadius = 4;
        this.showViewers = true;
        this.showFollowers = true;
        this.showSubscribers = true;
        this.showBits = true;
        this.showDonations = true;
        this.showUptime = true;
        this.showFPS = false;
        this.showCPU = false;
        this.showDroppedFrames = false;
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class ClockOverlay {
    constructor() {
        this.enabled = false;
        this.position = 'top-left';
        this.fontSize = 24;
        this.fontFamily = 'Arial';
        this.fontColor = '#ffffff';
        this.format = 'HH:MM:SS';
        this.showDate = false;
        this.showTimezone = false;
        this.timezone = 'local';
        this.animation = 'none';
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class ChatBoxOverlay {
    constructor() {
        this.enabled = false;
        this.position = 'bottom-center';
        this.width = 600;
        this.fontSize = 18;
        this.fontFamily = 'Arial';
        this.fontColor = '#ffffff';
        this.backgroundColor = 'rgba(0, 0, 0, 0.8)';
        this.borderRadius = 8;
        this.padding = 15;
        this.maxMessages = 5;
        this.scrollAnimation = 'slide';
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class ColorCorrectionSettings {
    constructor() {
        this.brightness = 0;
        this.contrast = 0;
        this.saturation = 0;
        this.gamma = 0;
        this.exposure = 0;
        this.highlight = 0;
        this.shadow = 0;
        this.temperature = 0;
        this.tint = 0;
    }

    clone() {
        const cloned = new ColorCorrectionSettings();
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

export class TransitionSettings {
    constructor() {
        this.type = 'cut';
        this.duration = 300;
        this.easing = 'easeInOut';
        this.direction = 'left';
    }

    clone() {
        const cloned = new TransitionSettings();
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

export class AlertSystem {
    constructor() {
        this.sounds = new Map();
        this.templates = new Map();
        this.queue = [];
        this.maxQueueSize = 10;
        this.globalVolume = 100;
        this.globalEnabled = true;
        this.types = {
            follow: true,
            subscription: true,
            donation: true,
            bits: true,
            host: true,
            raid: true,
            goal: true,
            poll: true,
            prediction: true
        };
    }

    addSound(type, soundUrl, volume = 100) {
        this.sounds.set(type, { url: soundUrl, volume });
    }

    removeSound(type) {
        this.sounds.delete(type);
    }

    addTemplate(type, template) {
        this.templates.set(type, template);
    }

    triggerAlert(type, data) {
        if (!this.globalEnabled || !this.types[type]) {
            return null;
        }

        const alert = {
            id: 'alert_' + Date.now(),
            type,
            data,
            template: this.templates.get(type),
            sound: this.sounds.get(type),
            triggeredAt: Date.now(),
            shown: false,
            duration: 5000
        };

        if (this.queue.length >= this.maxQueueSize) {
            this.queue.shift();
        }

        this.queue.push(alert);
        return alert;
    }

    getNextAlert() {
        return this.queue.shift();
    }

    serialize() {
        return {
            sounds: Array.from(this.sounds.entries()),
            templates: Array.from(this.templates.entries()),
            queue: this.queue,
            maxQueueSize: this.maxQueueSize,
            globalVolume: this.globalVolume,
            globalEnabled: this.globalEnabled,
            types: this.types
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.sounds = new Map(data.sounds || []);
        this.templates = new Map(data.templates || []);
    }
}

export class BroadcastStatistics {
    constructor() {
        this.totalStreamTime = 0;
        this.totalViewers = 0;
        this.totalFollowers = 0;
        this.totalSubscribers = 0;
        this.totalBits = 0;
        this.totalDonations = 0;
        this.totalChatMessages = 0;
        this.totalClipsCreated = 0;
        this.totalRaidsReceived = 0;
        this.totalHostsReceived = 0;
        this.peakViewers = 0;
        this.averageViewers = 0;
        this.averageViewersPercent = 0;
        this.viewsByDay = new Map();
        this.viewersByDay = new Map();
        this.followersByDay = new Map();
        this.subscribersByDay = new Map();
        this.bitsByDay = new Map();
        this.donationsByDay = new Map();
        this.adBreakTime = 0;
        this.adBreaks = 0;
        this.activityLog = [];
    }

    recordStart() {
        this.activityLog.push({ type: 'start', timestamp: Date.now() });
    }

    recordEnd() {
        this.activityLog.push({ type: 'end', timestamp: Date.now() });
    }

    recordPause() {
        this.activityLog.push({ type: 'pause', timestamp: Date.now() });
    }

    recordResume() {
        this.activityLog.push({ type: 'resume', timestamp: Date.now() });
    }

    recordViewerJoin() {
        this.totalViewers++;
    }

    recordViewerLeave() {
    }

    recordDonation(donation) {
        this.totalDonations += donation.amount;
        const today = new Date().toDateString();
        const count = this.donationsByDay.get(today) || 0;
        this.donationsByDay.set(today, count + donation.amount);
    }

    recordSubscription(subscription) {
        this.totalSubscribers++;
        const today = new Date().toDateString();
        const count = this.subscribersByDay.get(today) || 0;
        this.subscribersByDay.set(today, count + 1);
    }

    recordBits(bits) {
        this.totalBits += bits;
        const today = new Date().toDateString();
        const count = this.bitsByDay.get(today) || 0;
        this.bitsByDay.set(today, count + bits);
    }

    recordCommercial(duration) {
        this.adBreaks++;
        this.adBreakTime += duration;
    }

    recordClipCreated() {
        this.totalClipsCreated++;
    }

    serialize() {
        return {
            totalStreamTime: this.totalStreamTime,
            totalViewers: this.totalViewers,
            totalFollowers: this.totalFollowers,
            totalSubscribers: this.totalSubscribers,
            totalBits: this.totalBits,
            totalDonations: this.totalDonations,
            totalChatMessages: this.totalChatMessages,
            totalClipsCreated: this.totalClipsCreated,
            totalRaidsReceived: this.totalRaidsReceived,
            totalHostsReceived: this.totalHostsReceived,
            peakViewers: this.peakViewers,
            averageViewers: this.averageViewers,
            averageViewersPercent: this.averageViewersPercent,
            viewsByDay: Array.from(this.viewsByDay.entries()),
            viewersByDay: Array.from(this.viewersByDay.entries()),
            followersByDay: Array.from(this.followersByDay.entries()),
            subscribersByDay: Array.from(this.subscribersByDay.entries()),
            bitsByDay: Array.from(this.bitsByDay.entries()),
            donationsByDay: Array.from(this.donationsByDay.entries()),
            adBreakTime: this.adBreakTime,
            adBreaks: this.adBreaks
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.viewsByDay = new Map(data.viewsByDay || []);
        this.viewersByDay = new Map(data.viewersByDay || []);
        this.followersByDay = new Map(data.followersByDay || []);
        this.subscribersByDay = new Map(data.subscribersByDay || []);
        this.bitsByDay = new Map(data.bitsByDay || []);
        this.donationsByDay = new Map(data.donationsByDay || []);
    }
}

export class BroadcastAnalytics {
    constructor() {
        this.engagementRate = 0;
        this.chatRate = 0;
        this.raidRate = 0;
        this.hostRate = 0;
        this.subscriptionRate = 0;
        this.donationConversion = 0;
        this.viewerRetention = 0;
        this.averageWatchTime = 0;
        this.peakChatters = 0;
        this.uniqueChatters = 0;
        this.emoteUsage = new Map();
        this.viewerDemographics = new Map();
        this.viewerLocations = new Map();
        this.viewerDevices = new Map();
        this.viewerPlatforms = new Map();
        this.referrers = new Map();
        this.trafficSources = new Map();
        this.subscribersByTier = new Map();
        this.giftedSubs = 0;
        this.primeSubs = 0;
        this.bitTop = [];
        this.donationTop = [];
        this.hostTop = [];
        this.raidTop = [];
    }

    serialize() {
        return {
            engagementRate: this.engagementRate,
            chatRate: this.chatRate,
            raidRate: this.raidRate,
            hostRate: this.hostRate,
            subscriptionRate: this.subscriptionRate,
            donationConversion: this.donationConversion,
            viewerRetention: this.viewerRetention,
            averageWatchTime: this.averageWatchTime,
            peakChatters: this.peakChatters,
            uniqueChatters: this.uniqueChatters,
            emoteUsage: Array.from(this.emoteUsage.entries()),
            viewerDemographics: Array.from(this.viewerDemographics.entries()),
            viewerLocations: Array.from(this.viewerLocations.entries()),
            viewerDevices: Array.from(this.viewerDevices.entries()),
            viewerPlatforms: Array.from(this.viewerPlatforms.entries()),
            referrers: Array.from(this.referrers.entries()),
            trafficSources: Array.from(this.trafficSources.entries()),
            subscribersByTier: Array.from(this.subscribersByTier.entries()),
            giftedSubs: this.giftedSubs,
            primeSubs: this.primeSubs,
            bitTop: this.bitTop,
            donationTop: this.donationTop,
            hostTop: this.hostTop,
            raidTop: this.raidTop
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.emoteUsage = new Map(data.emoteUsage || []);
        this.viewerDemographics = new Map(data.viewerDemographics || []);
        this.viewerLocations = new Map(data.viewerLocations || []);
        this.viewerDevices = new Map(data.viewerDevices || []);
        this.viewerPlatforms = new Map(data.viewerPlatforms || []);
        this.referrers = new Map(data.referrers || []);
        this.trafficSources = new Map(data.trafficSources || []);
        this.subscribersByTier = new Map(data.subscribersByTier || []);
    }
}

export class BroadcastMonitization {
    constructor() {
        this.totalRevenue = 0;
        this.subscriptionRevenue = 0;
        this.bitsRevenue = 0;
        this.donationRevenue = 0;
        this.adRevenue = 0;
        this.sponsorshipRevenue = 0;
        this.otherRevenue = 0;
        this.pendingPayout = 0;
        this.totalPayout = 0;
        this.payoutHistory = [];
        this.taxInfo = null;
        this.paymentMethod = null;
        this.revenueByDay = new Map();
        this.subscribersByTier = new Map();
        this.commission = 0.5;
    }

    recordSubscription(subscription) {
        this.subscribersByTier.set(subscription.tier, (this.subscribersByTier.get(subscription.tier) || 0) + 1);
        const amount = this.calculateSubscriptionRevenue(subscription);
        this.subscriptionRevenue += amount;
        this.totalRevenue += amount;
        this.recordRevenue(amount, 'subscription');
    }

    recordBits(bits) {
        const amount = bits * 0.01 * this.commission;
        this.bitsRevenue += amount;
        this.totalRevenue += amount;
        this.recordRevenue(amount, 'bits');
    }

    recordDonation(donation) {
        this.donationRevenue += donation.amount;
        this.totalRevenue += donation.amount;
        this.recordRevenue(donation.amount, 'donation');
    }

    recordAdRevenue(amount) {
        this.adRevenue += amount;
        this.totalRevenue += amount;
        this.recordRevenue(amount, 'ad');
    }

    calculateSubscriptionRevenue(subscription) {
        const tierPrices = { 1: 4.99, 2: 9.99, 3: 24.99 };
        const price = tierPrices[subscription.tier] || 4.99;
        return price * this.commission;
    }

    recordRevenue(amount, type) {
        const today = new Date().toDateString();
        if (!this.revenueByDay.has(today)) {
            this.revenueByDay.set(today, {});
        }
        const dayRevenue = this.revenueByDay.get(today);
        dayRevenue[type] = (dayRevenue[type] || 0) + amount;
    }

    requestPayout(amount) {
        if (amount > this.pendingPayout) {
            return false;
        }
        this.pendingPayout -= amount;
        this.totalPayout += amount;
        this.payoutHistory.push({
            amount,
            requestedAt: Date.now(),
            completedAt: null,
            status: 'pending'
        });
        return true;
    }

    serialize() {
        return {
            totalRevenue: this.totalRevenue,
            subscriptionRevenue: this.subscriptionRevenue,
            bitsRevenue: this.bitsRevenue,
            donationRevenue: this.donationRevenue,
            adRevenue: this.adRevenue,
            sponsorshipRevenue: this.sponsorshipRevenue,
            otherRevenue: this.otherRevenue,
            pendingPayout: this.pendingPayout,
            totalPayout: this.totalPayout,
            payoutHistory: this.payoutHistory,
            taxInfo: this.taxInfo,
            paymentMethod: this.paymentMethod,
            revenueByDay: Array.from(this.revenueByDay.entries()),
            subscribersByTier: Array.from(this.subscribersByTier.entries()),
            commission: this.commission
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.revenueByDay = new Map(data.revenueByDay || []);
        this.subscribersByTier = new Map(data.subscribersByTier || []);
    }
}

export class NotificationSettings {
    constructor() {
        this.sounds = {
            follow: true,
            subscription: true,
            donation: true,
            bits: true,
            host: true,
            raid: true,
            goal: true,
            poll: true,
            prediction: true,
            clip: true
        };
        this.desktop = {
            enabled: true,
            duration: 5000,
            sounds: true
        };
        this.email = {
            enabled: false,
            digest: 'daily',
            threshold: 100
        };
        this.push = {
            enabled: false,
            follow: true,
            subscription: true,
            donation: true
        };
        this.mobile = {
            enabled: false,
            follow: true,
            subscription: true,
            donation: true
        };
    }

    clone() {
        const cloned = new NotificationSettings();
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

export class SpectatorView {
    constructor(sessionId, playerId, matchId) {
        this.sessionId = sessionId;
        this.playerId = playerId;
        this.matchId = matchId;
        this.camera = SpectatorCamera.FREE;
        this.targetPlayerId = null;
        this.targetTeam = null;
        this.position = { x: 0, y: 0, z: 0 };
        this.rotation = { pitch: 0, yaw: 0 };
        this.zoom = 1;
        this.isGotoEnabled = true;
        this.isFollowEnabled = true;
        this.isFreeCameraEnabled = true;
        this.isOverviewEnabled = true;
        this.isPovEnabled = true;
        this.showRadar = true;
        this.showMinimap = true;
        this.showHUD = true;
        this.showPlayerNames = true;
        this.showHealthBars = true;
        this.showWeapons = true;
        this.showGrenades = true;
        this.showBombStatus = true;
        this.showMoney = true;
        this.showArmor = true;
        this.showAmmo = true;
        this.showSpectatorCount = true;
        this.showKillFeed = true;
        this.showDamageIndicator = true;
        this.showHitMarkers = true;
        this.filters = new SpectatorFilters();
        this.overlays = new SpectatorOverlays();
        this.sound = new SpectatorSoundSettings();
        this.delay = 0;
        this.isRecording = false;
        this.recordingSettings = null;
        this.markers = [];
        this.annotations = [];
        this.notes = [];
        this.bookmarks = [];
        this.highlights = [];
        this.replays = [];
        this.joinedAt = Date.now();
        this.lastUpdate = Date.now();
        this.isActive = true;
    }

    setCamera(camera, targetId = null) {
        this.camera = camera;
        if (camera === SpectatorCamera.FOLLOW && targetId) {
            this.targetPlayerId = targetId;
        }
        this.lastUpdate = Date.now();
    }

    followPlayer(playerId) {
        this.targetPlayerId = playerId;
        this.targetTeam = null;
        this.camera = SpectatorCamera.FOLLOW;
        this.lastUpdate = Date.now();
    }

    followTeam(team) {
        this.targetTeam = team;
        this.targetPlayerId = null;
        this.camera = SpectatorCamera.OVERVIEW;
        this.lastUpdate = Date.now();
    }

    gotoPosition(x, y, z) {
        this.position = { x, y, z };
        this.camera = SpectatorCamera.FREE;
        this.lastUpdate = Date.now();
    }

    addMarker(label, position, color = '#ffffff') {
        this.markers.push({
            id: 'marker_' + Date.now(),
            label,
            position,
            color,
            timestamp: Date.now()
        });
    }

    addAnnotation(annotation) {
        this.annotations.push({
            id: 'annotation_' + Date.now(),
            ...annotation,
            timestamp: Date.now()
        });
    }

    addNote(note) {
        this.notes.push({
            id: 'note_' + Date.now(),
            note,
            timestamp: Date.now()
        });
    }

    addBookmark(position) {
        this.bookmarks.push({
            id: 'bookmark_' + Date.now(),
            position,
            timestamp: Date.now()
        });
    }

    addHighlight(fragment) {
        this.highlights.push({
            id: 'highlight_' + Date.now(),
            ...fragment,
            timestamp: Date.now()
        });
    }

    startRecording(settings) {
        this.isRecording = true;
        this.recordingSettings = settings;
    }

    stopRecording() {
        this.isRecording = false;
        const replay = {
            id: 'replay_' + Date.now(),
            settings: this.recordingSettings,
            fragments: [...this.highlights],
            createdAt: Date.now()
        };
        this.replays.push(replay);
        this.recordingSettings = null;
        return replay;
    }

    leave() {
        this.isActive = false;
    }

    serialize() {
        return {
            sessionId: this.sessionId,
            playerId: this.playerId,
            matchId: this.matchId,
            camera: this.camera,
            targetPlayerId: this.targetPlayerId,
            targetTeam: this.targetTeam,
            position: this.position,
            rotation: this.rotation,
            zoom: this.zoom,
            isGotoEnabled: this.isGotoEnabled,
            isFollowEnabled: this.isFollowEnabled,
            isFreeCameraEnabled: this.isFreeCameraEnabled,
            isOverviewEnabled: this.isOverviewEnabled,
            isPovEnabled: this.isPovEnabled,
            showRadar: this.showRadar,
            showMinimap: this.showMinimap,
            showHUD: this.showHUD,
            showPlayerNames: this.showPlayerNames,
            showHealthBars: this.showHealthBars,
            showWeapons: this.showWeapons,
            showGrenades: this.showGrenades,
            showBombStatus: this.showBombStatus,
            showMoney: this.showMoney,
            showArmor: this.showArmor,
            showAmmo: this.showAmmo,
            showSpectatorCount: this.showSpectatorCount,
            showKillFeed: this.showKillFeed,
            showDamageIndicator: this.showDamageIndicator,
            showHitMarkers: this.showHitMarkers,
            filters: this.filters.serialize(),
            overlays: this.overlays.serialize(),
            sound: this.sound.serialize(),
            delay: this.delay,
            isRecording: this.isRecording,
            recordingSettings: this.recordingSettings,
            markers: this.markers,
            annotations: this.annotations,
            notes: this.notes,
            bookmarks: this.bookmarks,
            highlights: this.highlights,
            replays: this.replays,
            joinedAt: this.joinedAt,
            lastUpdate: this.lastUpdate,
            isActive: this.isActive
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.filters = new SpectatorFilters();
        this.filters.deserialize(data.filters || {});
        this.overlays = new SpectatorOverlays();
        this.overlays.deserialize(data.overlays || {});
        this.sound = new SpectatorSoundSettings();
        this.sound.deserialize(data.sound || {});
    }
}

export class SpectatorFilters {
    constructor() {
        this.showTeams = { t: true, ct: true };
        this.showDead = true;
        this.showAlive = true;
        this.showSpectators = true;
        this.playerIds = [];
        this.weapons = [];
        this.grenades = true;
        this.projectiles = true;
        this.particles = true;
        this.tracers = true;
        this.blood = true;
        this.smoke = true;
        this.fire = true;
        this.spectators = true;
    }

    clone() {
        const cloned = new SpectatorFilters();
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

export class SpectatorOverlays {
    constructor() {
        this.scoreboard = true;
        this.timeRemaining = true;
        this.teamScores = true;
        this.roundNumber = true;
        this.matchInfo = true;
        this.playerStats = true;
        this.killFeed = true;
        this.bombStatus = true;
        this.hostageStatus = true;
        this.minimap = true;
        this.radar = true;
        this.crosshair = true;
        this.spectatorList = true;
        this.chat = false;
        this.voiceIndicator = true;
        this.tournamentInfo = true;
        this.branding = true;
    }

    clone() {
        const cloned = new SpectatorOverlays();
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

export class SpectatorSoundSettings {
    constructor() {
        this.masterVolume = 100;
        this.gameSounds = 100;
        this.voiceChat = 100;
        this.commentary = 100;
        this.music = 50;
        this.uiSounds = 50;
        this.broadcastAudio = true;
        this.playerAudio = true;
        this.ambientAudio = true;
        this.spatialAudio = true;
        this.binauralAudio = false;
    }

    clone() {
        const cloned = new SpectatorSoundSettings();
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

export class BroadcastManager {
    constructor() {
        this.sessions = new Map();
        this.spectatorViews = new Map();
        this.channels = new Map();
        this.activeStreams = [];
        this.statistics = new BroadcastManagerStatistics();
        this.settings = new BroadcastManagerSettings();
    }

    createSession(broadcasterId) {
        const sessionId = 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        const session = new BroadcastSession(sessionId, broadcasterId);
        this.sessions.set(sessionId, session);
        return session;
    }

    getSession(sessionId) {
        return this.sessions.get(sessionId);
    }

    getActiveSessions() {
        return Array.from(this.sessions.values())
            .filter(s => s.state === 'live');
    }

    createSpectatorView(sessionId, playerId, matchId) {
        const viewId = 'view_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        const view = new SpectatorView(viewId, playerId, matchId);
        this.spectatorViews.set(viewId, view);
        return view;
    }

    getSpectatorView(viewId) {
        return this.spectatorViews.get(viewId);
    }

    getMatchSpectators(matchId) {
        return Array.from(this.spectatorViews.values())
            .filter(v => v.matchId === matchId && v.isActive);
    }

    serialize() {
        return {
            sessions: Array.from(this.sessions.entries()).map(([id, s]) => [id, s.serialize()]),
            spectatorViews: Array.from(this.spectatorViews.entries()).map(([id, v]) => [id, v.serialize()]),
            channels: Array.from(this.channels.entries()),
            activeStreams: this.activeStreams,
            statistics: this.statistics.serialize()
        };
    }

    loadState(state) {
        this.sessions = new Map(state.sessions?.map(([id, data]) => {
            const session = new BroadcastSession(id);
            session.deserialize(data);
            return [id, session];
        }) || []);

        this.spectatorViews = new Map(state.spectatorViews?.map(([id, data]) => {
            const view = new SpectatorView();
            view.deserialize(data);
            return [id, view];
        }) || []);

        this.channels = new Map(state.channels || []);
        this.activeStreams = state.activeStreams || [];
        this.statistics = new BroadcastManagerStatistics();
        this.statistics.deserialize(state.statistics || {});
    }
}

export class BroadcastManagerStatistics {
    constructor() {
        this.totalStreams = 0;
        this.activeStreams = 0;
        this.totalViewers = 0;
        this.totalWatchTime = 0;
        this.totalRevenue = 0;
        this.statisticsByBroadcaster = new Map();
    }

    serialize() {
        return {
            totalStreams: this.totalStreams,
            activeStreams: this.activeStreams,
            totalViewers: this.totalViewers,
            totalWatchTime: this.totalWatchTime,
            totalRevenue: this.totalRevenue,
            statisticsByBroadcaster: Array.from(this.statisticsByBroadcaster.entries())
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.statisticsByBroadcaster = new Map(data.statisticsByBroadcaster || []);
    }
}

export class BroadcastManagerSettings {
    constructor() {
        this.autoRecording = false;
        this.recordFormat = 'mp4';
        this.qualityPreset = 'high';
        this.maxConcurrentStreams = 100;
        this.maxSpectatorsPerStream = 10000;
        this.defaultDelay = 0;
        this.anticheatDelay = 60;
        this.enableRTMP = true;
        this.enableWebRTC = false;
        this.cdnEnabled = true;
        this.adjustBitrateAutomatically = true;
    }

    serialize() {
        return { ...this };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export default {
    SpectatorCamera,
    BroadcastQuality,
    StreamPlatform,
    BroadcastSession,
    BroadcastSettings,
    BroadcastOverlays,
    WebcamOverlay,
    ChatOverlay,
    AlertOverlay,
    StatsOverlay,
    ClockOverlay,
    ChatBoxOverlay,
    ColorCorrectionSettings,
    TransitionSettings,
    AlertSystem,
    BroadcastStatistics,
    BroadcastAnalytics,
    BroadcastMonitization,
    NotificationSettings,
    SpectatorView,
    SpectatorFilters,
    SpectatorOverlays,
    SpectatorSoundSettings,
    BroadcastManager,
    BroadcastManagerStatistics,
    BroadcastManagerSettings
};

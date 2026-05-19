import { GameState } from './config.js';

export const NotificationType = {
    INFO: 'info',
    SUCCESS: 'success',
    WARNING: 'warning',
    ERROR: 'error',
    ACHIEVEMENT: 'achievement',
    RANK_UP: 'rank_up',
    MATCHMAKING: 'matchmaking',
    FRIEND: 'friend',
    SYSTEM: 'system',
    TRADE: 'trade',
    UPDATE: 'update',
    TIP: 'tip'
};

export const NotificationPriority = {
    LOW: 'low',
    NORMAL: 'normal',
    HIGH: 'high',
    CRITICAL: 'critical'
};

export const NotificationPosition = {
    TOP_LEFT: 'top-left',
    TOP_CENTER: 'top-center',
    TOP_RIGHT: 'top-right',
    BOTTOM_LEFT: 'bottom-left',
    BOTTOM_CENTER: 'bottom-center',
    BOTTOM_RIGHT: 'bottom-right',
    CENTER: 'center'
};

export const NotificationDuration = {
    SHORT: 3000,
    MEDIUM: 5000,
    LONG: 8000,
    PERSISTENT: 0
};

export const MessageType = {
    TEXT: 'text',
    SYSTEM: 'system',
    TEAM: 'team',
    GLOBAL: 'global',
    WHISPER: 'whisper',
    CLAN: 'clan',
    PARTY: 'party'
};

export const MessageFilter = {
    ALL: 'all',
    TEAM: 'team',
    GLOBAL: 'global',
    PRIVATE: 'private',
    SYSTEM: 'system'
};

export class Notification {
    constructor(id, type, title, message, options = {}) {
        this.id = id;
        this.type = type;
        this.title = title;
        this.message = message;
        this.options = {
            duration: options.duration || NotificationDuration.MEDIUM,
            priority: options.priority || NotificationPriority.NORMAL,
            position: options.position || NotificationPosition.TOP_RIGHT,
            icon: options.icon || null,
            sound: options.sound !== undefined ? options.sound : true,
            dismissible: options.dismissible !== undefined ? options.dismissible : true,
            progress: options.progress || false,
            actions: options.actions || [],
            metadata: options.metadata || {},
            category: options.category || 'general',
            tag: options.tag || null,
            image: options.image || null,
            link: options.link || null,
            autoHide: options.autoHide !== undefined ? options.autoHide : true,
            maxWidth: options.maxWidth || 350,
            transition: options.transition || 'slide',
            ...options
        };
        this.createdAt = new Date();
        this.expiresAt = options.duration ? new Date(Date.now() + options.duration) : null;
        this.read = false;
        this.dismissed = false;
        this.progressValue = 0;
    }

    dismiss() {
        this.dismissed = true;
    }

    markAsRead() {
        this.read = true;
    }

    setProgress(value) {
        this.progressValue = Math.max(0, Math.min(100, value));
    }

    isExpired() {
        if (!this.expiresAt) return false;
        return new Date() > this.expiresAt;
    }

    canAutoHide() {
        return this.options.autoHide && this.options.dismissible && this.options.duration > 0;
    }

    getRemainingTime() {
        if (!this.expiresAt) return Infinity;
        return Math.max(0, this.expiresAt - new Date());
    }

    toJSON() {
        return {
            id: this.id,
            type: this.type,
            title: this.title,
            message: this.message,
            options: this.options,
            created_at: this.createdAt.toISOString(),
            expires_at: this.expiresAt ? this.expiresAt.toISOString() : null,
            read: this.read,
            dismissed: this.dismissed,
            progress: this.progressValue
        };
    }

    static fromJSON(data) {
        const notification = new Notification(
            data.id,
            data.type,
            data.title,
            data.message,
            data.options
        );
        notification.createdAt = new Date(data.created_at);
        notification.expiresAt = data.expires_at ? new Date(data.expires_at) : null;
        notification.read = data.read;
        notification.dismissed = data.dismissed;
        notification.progressValue = data.progress || 0;
        return notification;
    }
}

export class NotificationAction {
    constructor(id, label, callback, options = {}) {
        this.id = id;
        this.label = label;
        this.callback = callback;
        this.options = {
            primary: options.primary || false,
            disabled: options.disabled || false,
            icon: options.icon || null,
            ...options
        };
    }

    execute() {
        if (!this.options.disabled && this.callback) {
            this.callback();
            return true;
        }
        return false;
    }
}

export class NotificationCategory {
    constructor(id, name, icon, options = {}) {
        this.id = id;
        this.name = name;
        this.icon = icon;
        this.options = {
            enabled: options.enabled !== undefined ? options.enabled : true,
            sound: options.sound !== undefined ? options.sound : true,
            countBadge: options.countBadge !== undefined ? options.countBadge : true,
            ...options
        };
        this.notificationCount = 0;
        this.unreadCount = 0;
    }

    incrementCount() {
        this.notificationCount++;
        this.unreadCount++;
    }

    markAllRead() {
        this.unreadCount = 0;
    }

    reset() {
        this.notificationCount = 0;
        this.unreadCount = 0;
    }
}

export class ChatMessage {
    constructor(id, senderId, senderName, content, type, options = {}) {
        this.id = id;
        this.senderId = senderId;
        this.senderName = senderName;
        this.content = content;
        this.type = type;
        this.options = {
            timestamp: options.timestamp || new Date(),
            channel: options.channel || null,
            recipientId: options.recipientId || null,
            clanTag: options.clanTag || null,
            avatar: options.avatar || null,
            rank: options.rank || null,
            color: options.color || null,
            metadata: options.metadata || {},
            isCommand: options.isCommand || false,
            commandName: options.commandName || null,
            attachments: options.attachments || [],
            reactions: options.reactions || [],
            replyTo: options.replyTo || null,
            ...options
        };
        this.read = false;
        this.delivered = true;
        this.edited = false;
        this.deleted = false;
        this.reactionCount = 0;
    }

    edit(newContent) {
        if (!this.deleted) {
            this.content = newContent;
            this.edited = true;
            return true;
        }
        return false;
    }

    delete() {
        this.deleted = true;
        this.content = '[Message deleted]';
    }

    addReaction(reaction) {
        if (!this.options.reactions.includes(reaction)) {
            this.options.reactions.push(reaction);
            this.reactionCount++;
        }
    }

    removeReaction(reaction) {
        const index = this.options.reactions.indexOf(reaction);
        if (index > -1) {
            this.options.reactions.splice(index, 1);
            this.reactionCount--;
        }
    }

    markAsRead() {
        this.read = true;
    }

    getFormattedTime() {
        const date = this.options.timestamp;
        const now = new Date();
        const isToday = date.toDateString() === now.toDateString();

        if (isToday) {
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        return date.toLocaleDateString([], { month: 'short', day: 'numeric' }) + 
               ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    toJSON() {
        return {
            id: this.id,
            sender_id: this.senderId,
            sender_name: this.senderName,
            content: this.content,
            type: this.type,
            options: this.options,
            read: this.read,
            delivered: this.delivered,
            edited: this.edited,
            deleted: this.deleted,
            reaction_count: this.reactionCount
        };
    }

    static fromJSON(data) {
        const message = new ChatMessage(
            data.id,
            data.sender_id,
            data.sender_name,
            data.content,
            data.type,
            data.options
        );
        message.read = data.read;
        message.delivered = data.delivered;
        message.edited = data.edited;
        message.deleted = data.deleted;
        message.reactionCount = data.reaction_count || 0;
        return message;
    }
}

export class ChatChannel {
    constructor(id, name, type) {
        this.id = id;
        this.name = name;
        this.type = type;
        this.messages = [];
        this.participants = new Set();
        this.unreadCount = 0;
        this.lastActivity = new Date();
        this.isMuted = false;
        this.isPinned = false;
        this.isArchived = false;
        this.settings = {
            notifications: true,
            sound: true,
            preview: true,
            compactMode: false
        };
    }

    addMessage(message) {
        this.messages.push(message);
        this.lastActivity = new Date();
        if (!message.read) {
            this.unreadCount++;
        }
        if (this.messages.length > 1000) {
            this.messages.shift();
        }
    }

    getRecentMessages(count = 50) {
        return this.messages.slice(-count);
    }

    markAllRead() {
        this.unreadCount = 0;
        for (const message of this.messages) {
            message.markAsRead();
        }
    }

    addParticipant(playerId) {
        this.participants.add(playerId);
    }

    removeParticipant(playerId) {
        this.participants.delete(playerId);
    }

    clearMessages() {
        this.messages = [];
        this.unreadCount = 0;
    }

    toggleMute() {
        this.isMuted = !this.isMuted;
    }

    togglePin() {
        this.isPinned = !this.isPinned;
    }

    archive() {
        this.isArchived = true;
    }

    export() {
        return {
            id: this.id,
            name: this.name,
            type: this.type,
            messages: this.messages.map(m => m.toJSON()),
            participants: Array.from(this.participants),
            unread_count: this.unreadCount,
            last_activity: this.lastActivity.toISOString(),
            is_muted: this.isMuted,
            is_pinned: this.isPinned,
            is_archived: this.isArchived,
            settings: this.settings
        };
    }

    static import(data) {
        const channel = new ChatChannel(data.id, data.name, data.type);
        channel.messages = (data.messages || []).map(m => ChatMessage.fromJSON(m));
        channel.participants = new Set(data.participants || []);
        channel.unreadCount = data.unread_count || 0;
        channel.lastActivity = data.last_activity ? new Date(data.last_activity) : new Date();
        channel.isMuted = data.is_muted || false;
        channel.isPinned = data.is_pinned || false;
        channel.isArchived = data.is_archived || false;
        channel.settings = { ...channel.settings, ...data.settings };
        return channel;
    }
}

export class ChatManager {
    constructor() {
        this.channels = new Map();
        this.activeChannel = null;
        this.blockedUsers = new Set();
        this.mutedUsers = new Set();
        this.chatHistory = [];
        this.maxHistorySize = 5000;
        this.listeners = new Map();
    }

    createChannel(id, name, type) {
        if (!this.channels.has(id)) {
            const channel = new ChatChannel(id, name, type);
            this.channels.set(id, channel);
            return channel;
        }
        return this.channels.get(id);
    }

    getChannel(id) {
        return this.channels.get(id);
    }

    deleteChannel(id) {
        return this.channels.delete(id);
    }

    setActiveChannel(id) {
        const channel = this.channels.get(id);
        if (channel) {
            this.activeChannel = id;
            channel.markAllRead();
            this.emit('activeChannelChanged', channel);
        }
    }

    getActiveChannel() {
        return this.activeChannel ? this.channels.get(this.activeChannel) : null;
    }

    getAllChannels() {
        return Array.from(this.channels.values());
    }

    sendMessage(channelId, senderId, senderName, content, type, options = {}) {
        const channel = this.channels.get(channelId);
        if (!channel || channel.isMuted) return null;

        const message = new ChatMessage(
            this.generateId(),
            senderId,
            senderName,
            content,
            type,
            options
        );

        channel.addMessage(message);
        this.addToHistory(message);
        this.emit('messageReceived', { channel, message });
        return message;
    }

    blockUser(playerId) {
        this.blockedUsers.add(playerId);
        this.emit('userBlocked', playerId);
    }

    unblockUser(playerId) {
        this.blockedUsers.delete(playerId);
        this.emit('userUnblocked', playerId);
    }

    isBlocked(playerId) {
        return this.blockedUsers.has(playerId);
    }

    muteUser(playerId) {
        this.mutedUsers.add(playerId);
        this.emit('userMuted', playerId);
    }

    unmuteUser(playerId) {
        this.mutedUsers.delete(playerId);
        this.emit('userUnmuted', playerId);
    }

    isMuted(playerId) {
        return this.mutedUsers.has(playerId);
    }

    addToHistory(message) {
        this.chatHistory.push(message);
        if (this.chatHistory.length > this.maxHistorySize) {
            this.chatHistory.shift();
        }
    }

    getHistory(filter = MessageFilter.ALL) {
        if (filter === MessageFilter.ALL) {
            return [...this.chatHistory];
        }
        return this.chatHistory.filter(m => m.type === filter);
    }

    searchHistory(query, options = {}) {
        const { caseSensitive = false, maxResults = 100 } = options;
        const searchTerm = caseSensitive ? query : query.toLowerCase();

        return this.chatHistory
            .filter(m => {
                const content = caseSensitive ? m.content : m.content.toLowerCase();
                return content.includes(searchTerm);
            })
            .slice(0, maxResults);
    }

    generateId() {
        return `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
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
                console.error(`Error in chat listener: ${e}`);
            }
        }
    }
}

export class NotificationManager {
    static instance = null;

    constructor() {
        this.notifications = [];
        this.categories = new Map();
        this.maxNotifications = 100;
        this.listeners = new Map();
        this.dismissTimers = new Map();
        this.isPaused = false;
        this.soundEnabled = true;
        this.initDefaultCategories();
    }

    static getInstance() {
        if (!NotificationManager.instance) {
            NotificationManager.instance = new NotificationManager();
        }
        return NotificationManager.instance;
    }

    initDefaultCategories() {
        const categories = [
            new NotificationCategory('general', 'General', 'bell'),
            new NotificationCategory('matchmaking', 'Matchmaking', 'users'),
            new NotificationCategory('friends', 'Friends', 'user-plus'),
            new NotificationCategory('achievements', 'Achievements', 'trophy'),
            new NotificationCategory('trades', 'Trades', 'exchange'),
            new NotificationCategory('system', 'System', 'settings'),
            new NotificationCategory('updates', 'Updates', 'download'),
            new NotificationCategory('social', 'Social', 'message-circle')
        ];

        for (const category of categories) {
            this.categories.set(category.id, category);
        }
    }

    createCategory(id, name, icon, options) {
        if (!this.categories.has(id)) {
            const category = new NotificationCategory(id, name, icon, options);
            this.categories.set(id, category);
            return category;
        }
        return this.categories.get(id);
    }

    getCategory(id) {
        return this.categories.get(id);
    }

    add(type, title, message, options = {}) {
        if (this.isPaused && options.priority !== NotificationPriority.CRITICAL) {
            return null;
        }

        const id = this.generateId();
        const notification = new Notification(id, type, title, message, options);

        if (notification.options.category) {
            const category = this.categories.get(notification.options.category);
            if (category) {
                category.incrementCount();
            }
        }

        this.notifications.unshift(notification);
        if (this.notifications.length > this.maxNotifications) {
            this.notifications.pop();
        }

        this.emit('notificationAdded', notification);

        if (notification.canAutoHide()) {
            this.startDismissTimer(notification);
        }

        return notification;
    }

    remove(id) {
        const index = this.notifications.findIndex(n => n.id === id);
        if (index > -1) {
            const notification = this.notifications[index];
            notification.dismiss();
            this.notifications.splice(index, 1);
            this.dismissTimers.delete(id);
            this.emit('notificationRemoved', notification);
            return true;
        }
        return false;
    }

    dismiss(id) {
        return this.remove(id);
    }

    dismissAll() {
        const dismissed = [...this.notifications];
        this.notifications = [];
        for (const [id] of this.dismissTimers) {
            clearTimeout(this.dismissTimers.get(id));
        }
        this.dismissTimers.clear();
        this.emit('allNotificationsDismissed', dismissed);
    }

    dismissByCategory(categoryId) {
        const toRemove = this.notifications.filter(n => n.options.category === categoryId);
        for (const notification of toRemove) {
            this.remove(notification.id);
        }
        return toRemove.length;
    }

    get(id) {
        return this.notifications.find(n => n.id === id);
    }

    getAll() {
        return [...this.notifications];
    }

    getByCategory(categoryId) {
        return this.notifications.filter(n => n.options.category === categoryId);
    }

    getByType(type) {
        return this.notifications.filter(n => n.type === type);
    }

    getUnread() {
        return this.notifications.filter(n => !n.read);
    }

    getUnreadCount() {
        return this.notifications.filter(n => !n.read).length;
    }

    getCategoryUnreadCount(categoryId) {
        const category = this.categories.get(categoryId);
        return category ? category.unreadCount : 0;
    }

    markAsRead(id) {
        const notification = this.get(id);
        if (notification) {
            notification.markAsRead();
            this.emit('notificationRead', notification);
        }
    }

    markAllAsRead() {
        for (const notification of this.notifications) {
            notification.markAsRead();
        }
        for (const category of this.categories.values()) {
            category.markAllRead();
        }
        this.emit('allNotificationsRead');
    }

    markCategoryAsRead(categoryId) {
        const category = this.categories.get(categoryId);
        if (category) {
            category.markAllRead();
            for (const notification of this.getByCategory(categoryId)) {
                notification.markAsRead();
            }
        }
    }

    startDismissTimer(notification) {
        if (this.dismissTimers.has(notification.id)) {
            clearTimeout(this.dismissTimers.get(notification.id));
        }

        const timerId = setTimeout(() => {
            this.remove(notification.id);
        }, notification.options.duration);

        this.dismissTimers.set(notification.id, timerId);
    }

    pause() {
        this.isPaused = true;
        for (const [id, timerId] of this.dismissTimers) {
            clearTimeout(timerId);
        }
        this.dismissTimers.clear();
        this.emit('notificationsPaused');
    }

    resume() {
        this.isPaused = false;
        for (const notification of this.notifications) {
            if (notification.canAutoHide() && !notification.dismissed) {
                this.startDismissTimer(notification);
            }
        }
        this.emit('notificationsResumed');
    }

    setSoundEnabled(enabled) {
        this.soundEnabled = enabled;
        this.emit('soundChanged', enabled);
    }

    isSoundEnabled() {
        return this.soundEnabled;
    }

    setMaxNotifications(max) {
        this.maxNotifications = max;
        while (this.notifications.length > this.maxNotifications) {
            this.notifications.pop();
        }
    }

    generateId() {
        return `notif_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    showAchievementUnlocked(achievement) {
        return this.add(
            NotificationType.ACHIEVEMENT,
            'Achievement Unlocked!',
            achievement.name,
            {
                duration: NotificationDuration.LONG,
                priority: NotificationPriority.HIGH,
                icon: 'trophy',
                sound: true,
                metadata: achievement
            }
        );
    }

    showRankUp(rankInfo) {
        return this.add(
            NotificationType.RANK_UP,
            'Rank Up!',
            `You are now ${rankInfo.getName()}`,
            {
                duration: NotificationDuration.LONG,
                priority: NotificationPriority.HIGH,
                icon: 'star',
                sound: true,
                metadata: rankInfo.export()
            }
        );
    }

    showMatchFound(queueType, estimatedWait) {
        return this.add(
            NotificationType.MATCHMAKING,
            'Match Found!',
            `Accept to join ${queueType} match`,
            {
                duration: NotificationDuration.PERSISTENT,
                priority: NotificationPriority.CRITICAL,
                icon: 'users',
                dismissible: false,
                sound: true,
                actions: [
                    new NotificationAction('accept', 'Accept'),
                    new NotificationAction('decline', 'Decline')
                ]
            }
        );
    }

    showFriendOnline(friendName) {
        return this.add(
            NotificationType.FRIEND,
            'Friend Online',
            `${friendName} is now online`,
            {
                duration: NotificationDuration.MEDIUM,
                priority: NotificationPriority.NORMAL,
                icon: 'user-plus',
                sound: true
            }
        );
    }

    showSystemMessage(message, options = {}) {
        return this.add(
            NotificationType.SYSTEM,
            options.title || 'System',
            message,
            {
                duration: options.duration || NotificationDuration.SHORT,
                priority: options.priority || NotificationPriority.LOW,
                icon: options.icon || 'info',
                sound: false
            }
        );
    }

    showError(message, options = {}) {
        return this.add(
            NotificationType.ERROR,
            options.title || 'Error',
            message,
            {
                duration: options.duration || NotificationDuration.LONG,
                priority: NotificationPriority.HIGH,
                icon: 'alert-circle',
                sound: true,
                ...options
            }
        );
    }

    showSuccess(message, options = {}) {
        return this.add(
            NotificationType.SUCCESS,
            options.title || 'Success',
            message,
            {
                duration: options.duration || NotificationDuration.SHORT,
                priority: NotificationPriority.NORMAL,
                icon: 'check-circle',
                sound: false,
                ...options
            }
        );
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
                console.error(`Error in notification listener: ${e}`);
            }
        }
    }
}

export const Notifications = NotificationManager.getInstance();
export const Chat = new ChatManager();

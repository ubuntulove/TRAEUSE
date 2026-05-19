export class CommunityHub {
    constructor() {
        this.id = 'hub_' + Date.now();
        this.name = 'Community Hub';
        this.description = '';
        this.owner = null;
        this.admins = new Set();
        this.members = new Map();
        this.invitedMembers = [];
        this.settings = new HubSettings();
        this.channels = [];
        this.threads = [];
        this.events = [];
        this.announcements = [];
        this.polls = [];
        this.resources = new Map();
        this.statistics = new HubStatistics();
        this.createdAt = Date.now();
        this.updatedAt = Date.now();
        this.isPublic = true;
        this.isVerified = false;
        this.tags = [];
        this.region = null;
        this.language = 'en';
        this.theme = 'default';
        this.avatar = null;
        this.banner = null;
    }

    setOwner(userId) {
        this.owner = userId;
        this.updatedAt = Date.now();
    }

    addAdmin(userId) {
        if (this.owner === userId) return false;
        this.admins.add(userId);
        this.updatedAt = Date.now();
        return true;
    }

    removeAdmin(userId) {
        if (this.owner === userId) return false;
        this.admins.delete(userId);
        this.updatedAt = Date.now();
        return true;
    }

    isAdmin(userId) {
        return this.owner === userId || this.admins.has(userId);
    }

    inviteMember(userId, inviterId) {
        if (this.members.has(userId)) {
            return { success: false, reason: 'Already a member' };
        }

        if (this.invitedMembers.find(i => i.userId === userId)) {
            return { success: false, reason: 'Already invited' };
        }

        this.invitedMembers.push({
            userId,
            invitedBy: inviterId,
            invitedAt: Date.now(),
            expiresAt: Date.now() + 7 * 24 * 60 * 60 * 1000
        });

        this.updatedAt = Date.now();
        return { success: true };
    }

    acceptInvite(userId) {
        const inviteIndex = this.invitedMembers.findIndex(i => i.userId === userId);
        if (inviteIndex === -1) {
            return { success: false, reason: 'No invitation found' };
        }

        const invite = this.invitedMembers[inviteIndex];
        if (invite.expiresAt < Date.now()) {
            this.invitedMembers.splice(inviteIndex, 1);
            return { success: false, reason: 'Invitation expired' };
        }

        this.invitedMembers.splice(inviteIndex, 1);
        return this.addMember(userId, { invited: true });
    }

    addMember(userId, options = {}) {
        if (this.members.has(userId)) {
            return { success: false, reason: 'Already a member' };
        }

        if (!this.isPublic && this.invitedMembers.find(i => i.userId === userId) === -1) {
            return { success: false, reason: 'Must be invited to private hub' };
        }

        const memberData = {
            userId,
            joinedAt: Date.now(),
            role: 'member',
            nickname: options.nickname || null,
            status: 'active',
            lastActive: Date.now(),
            contributionScore: 0,
            postsCount: 0,
            threadsCount: 0,
            reactionsGiven: 0,
            reactionsReceived: 0,
            warnings: 0,
            bans: [],
            achievements: [],
            customTitle: null,
            flair: null
        };

        this.members.set(userId, memberData);
        this.statistics.totalMembers++;
        this.updatedAt = Date.now();

        return { success: true, member: memberData };
    }

    removeMember(userId) {
        if (!this.members.has(userId)) {
            return { success: false, reason: 'Not a member' };
        }

        if (this.owner === userId) {
            return { success: false, reason: 'Cannot remove owner' };
        }

        this.members.delete(userId);
        this.statistics.totalMembers--;
        this.updatedAt = Date.now();

        return { success: true };
    }

    updateMemberStatus(userId, status) {
        const member = this.members.get(userId);
        if (!member) {
            return { success: false, reason: 'Not a member' };
        }

        member.status = status;
        member.lastActive = Date.now();
        this.updatedAt = Date.now();

        return { success: true };
    }

    getMember(userId) {
        return this.members.get(userId);
    }

    getMemberList() {
        return Array.from(this.members.values())
            .sort((a, b) => {
                if (a.role === 'owner') return -1;
                if (b.role === 'owner') return 1;
                if (a.role === 'admin') return -1;
                if (b.role === 'admin') return 1;
                return b.contributionScore - a.contributionScore;
            });
    }

    createChannel(channelData) {
        const channel = {
            id: 'channel_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            name: channelData.name,
            description: channelData.description || '',
            type: channelData.type || 'text',
            category: channelData.category || null,
            position: this.channels.length,
            isPrivate: channelData.isPrivate || false,
            allowedRoles: channelData.allowedRoles || ['member', 'admin', 'owner'],
            allowedUsers: channelData.allowedUsers || [],
            createdBy: channelData.createdBy,
            createdAt: Date.now(),
            messages: [],
            messageCount: 0,
            lastMessage: null,
            settings: {
                slowMode: false,
                slowModeDelay: 0,
                nsfw: false,
                archived: false
            }
        };

        this.channels.push(channel);
        this.updatedAt = Date.now();

        return channel;
    }

    deleteChannel(channelId) {
        const channelIndex = this.channels.findIndex(c => c.id === channelId);
        if (channelIndex === -1) {
            return { success: false, reason: 'Channel not found' };
        }

        const channel = this.channels[channelIndex];
        if (channel.isSystem) {
            return { success: false, reason: 'Cannot delete system channel' };
        }

        this.channels.splice(channelIndex, 1);
        this.updatedAt = Date.now();

        return { success: true };
    }

    getChannel(channelId) {
        return this.channels.find(c => c.id === channelId);
    }

    createThread(threadData) {
        const thread = {
            id: 'thread_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            channelId: threadData.channelId,
            title: threadData.title,
            content: threadData.content,
            authorId: threadData.authorId,
            authorName: threadData.authorName,
            createdAt: Date.now(),
            updatedAt: Date.now(),
            isPinned: false,
            isLocked: false,
            isArchived: false,
            views: 0,
            replies: [],
            replyCount: 0,
            reactions: new Map(),
            tags: threadData.tags || [],
            category: threadData.category || null,
            poll: null,
            solution: null,
            editHistory: []
        };

        this.threads.push(thread);
        
        const author = this.members.get(threadData.authorId);
        if (author) {
            author.threadsCount++;
            author.contributionScore += 10;
        }

        this.statistics.totalThreads++;
        this.updatedAt = Date.now();

        return thread;
    }

    getThread(threadId) {
        return this.threads.find(t => t.id === threadId);
    }

    getThreadsByChannel(channelId, options = {}) {
        let threads = this.threads.filter(t => t.channelId === channelId);

        if (options.category) {
            threads = threads.filter(t => t.category === options.category);
        }

        if (options.tags && options.tags.length > 0) {
            threads = threads.filter(t => 
                options.tags.some(tag => t.tags.includes(tag))
            );
        }

        if (options.authorId) {
            threads = threads.filter(t => t.authorId === options.authorId);
        }

        threads.sort((a, b) => {
            if (a.isPinned !== b.isPinned) return a.isPinned ? -1 : 1;
            if (options.sortBy === 'popular') {
                const aReactions = Array.from(a.reactions.values()).reduce((sum, r) => sum + r.count, 0);
                const bReactions = Array.from(b.reactions.values()).reduce((sum, r) => sum + r.count, 0);
                return bReactions - aReactions;
            }
            return b.updatedAt - a.updatedAt;
        });

        if (options.limit) {
            threads = threads.slice(options.offset || 0, options.offset + options.limit);
        }

        return threads;
    }

    addReply(threadId, replyData) {
        const thread = this.getThread(threadId);
        if (!thread) {
            return { success: false, reason: 'Thread not found' };
        }

        if (thread.isLocked) {
            return { success: false, reason: 'Thread is locked' };
        }

        const reply = {
            id: 'reply_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            threadId,
            content: replyData.content,
            authorId: replyData.authorId,
            authorName: replyData.authorName,
            createdAt: Date.now(),
            updatedAt: Date.now(),
            isEdited: false,
            reactions: new Map(),
            isSolution: false,
            quotedReply: replyData.quotedReply || null,
            editHistory: []
        };

        thread.replies.push(reply);
        thread.replyCount++;
        thread.updatedAt = Date.now();

        const author = this.members.get(replyData.authorId);
        if (author) {
            author.postsCount++;
            author.contributionScore += 2;
        }

        this.statistics.totalReplies++;
        this.updatedAt = Date.now();

        return { success: true, reply };
    }

    reactToThread(threadId, userId, emoji) {
        const thread = this.getThread(threadId);
        if (!thread) {
            return { success: false, reason: 'Thread not found' };
        }

        const reactions = thread.reactions;
        if (!reactions.has(emoji)) {
            reactions.set(emoji, { count: 0, users: new Set() });
        }

        const reaction = reactions.get(emoji);
        if (reaction.users.has(userId)) {
            reaction.users.delete(userId);
            reaction.count--;
            
            const author = this.members.get(thread.authorId);
            if (author) author.reactionsReceived--;
            
            const user = this.members.get(userId);
            if (user) user.reactionsGiven--;
        } else {
            reaction.users.add(userId);
            reaction.count++;
            
            const author = this.members.get(thread.authorId);
            if (author) author.reactionsReceived++;
            
            const user = this.members.get(userId);
            if (user) user.reactionsGiven++;
        }

        thread.updatedAt = Date.now();
        this.updatedAt = Date.now();

        return { success: true, reaction: reaction.count };
    }

    createPoll(pollData) {
        const poll = {
            id: 'poll_' + Date.now(),
            question: pollData.question,
            options: pollData.options.map((text, index) => ({
                id: index,
                text,
                votes: [],
                voteCount: 0
            })),
            createdBy: pollData.createdBy,
            createdAt: Date.now(),
            endsAt: pollData.endsAt || Date.now() + 7 * 24 * 60 * 60 * 1000,
            isAnonymous: pollData.isAnonymous ?? false,
            allowMultipleVotes: pollData.allowMultipleVotes ?? false,
            totalVotes: 0,
            votedUsers: new Set(),
            status: 'active'
        };

        this.polls.push(poll);
        return poll;
    }

    voteOnPoll(pollId, optionId, userId) {
        const poll = this.polls.find(p => p.id === pollId);
        if (!poll) {
            return { success: false, reason: 'Poll not found' };
        }

        if (poll.status !== 'active') {
            return { success: false, reason: 'Poll is not active' };
        }

        if (poll.endsAt < Date.now()) {
            poll.status = 'ended';
            return { success: false, reason: 'Poll has ended' };
        }

        if (poll.votedUsers.has(userId)) {
            return { success: false, reason: 'Already voted' };
        }

        const option = poll.options.find(o => o.id === optionId);
        if (!option) {
            return { success: false, reason: 'Invalid option' };
        }

        option.votes.push(userId);
        option.voteCount++;
        poll.totalVotes++;
        poll.votedUsers.add(userId);

        this.updatedAt = Date.now();
        return { success: true };
    }

    createEvent(eventData) {
        const event = {
            id: 'event_' + Date.now(),
            title: eventData.title,
            description: eventData.description,
            startTime: eventData.startTime,
            endTime: eventData.endTime,
            organizer: eventData.organizer,
            maxParticipants: eventData.maxParticipants,
            participants: [],
            createdAt: Date.now(),
            status: 'upcoming',
            type: eventData.type || 'general',
            tags: eventData.tags || [],
            rsvps: new Map()
        };

        this.events.push(event);
        return event;
    }

    rsvpToEvent(eventId, userId, status) {
        const event = this.events.find(e => e.id === eventId);
        if (!event) {
            return { success: false, reason: 'Event not found' };
        }

        if (status === 'attending' && event.maxParticipants) {
            const currentParticipants = event.rsvps.get('attending')?.size || 0;
            if (currentParticipants >= event.maxParticipants) {
                return { success: false, reason: 'Event is full' };
            }
        }

        event.rsvps.set(status, event.rsvps.get(status) || new Set());
        event.rsvps.get(status).add(userId);

        this.updatedAt = Date.now();
        return { success: true };
    }

    createAnnouncement(announcementData) {
        const announcement = {
            id: 'announce_' + Date.now(),
            title: announcementData.title,
            content: announcementData.content,
            authorId: announcementData.authorId,
            authorName: announcementData.authorName,
            createdAt: Date.now(),
            isPinned: announcementData.isPinned || false,
            isImportant: announcementData.isImportant || false,
            views: 0,
            reactions: new Map()
        };

        this.announcements.push(announcement);
        return announcement;
    }

    getLeaderboard(type = 'contribution') {
        const members = Array.from(this.members.values());

        switch (type) {
            case 'contribution':
                return members.sort((a, b) => b.contributionScore - a.contributionScore);
            case 'posts':
                return members.sort((a, b) => b.postsCount - a.postsCount);
            case 'threads':
                return members.sort((a, b) => b.threadsCount - a.threadsCount);
            case 'reactions':
                return members.sort((a, b) => b.reactionsReceived - a.reactionsReceived);
            default:
                return members.sort((a, b) => b.contributionScore - a.contributionScore);
        }
    }

    search(query, options = {}) {
        const results = {
            threads: [],
            members: [],
            channels: [],
            events: []
        };

        const queryLower = query.toLowerCase();

        if (options.threads !== false) {
            results.threads = this.threads.filter(t => 
                t.title.toLowerCase().includes(queryLower) ||
                t.content.toLowerCase().includes(queryLower) ||
                t.tags.some(tag => tag.toLowerCase().includes(queryLower))
            );
        }

        if (options.members !== false) {
            results.members = Array.from(this.members.values()).filter(m =>
                m.nickname?.toLowerCase().includes(queryLower) ||
                m.userId.toLowerCase().includes(queryLower) ||
                m.customTitle?.toLowerCase().includes(queryLower)
            );
        }

        if (options.channels !== false) {
            results.channels = this.channels.filter(c =>
                c.name.toLowerCase().includes(queryLower) ||
                c.description.toLowerCase().includes(queryLower)
            );
        }

        if (options.events !== false) {
            results.events = this.events.filter(e =>
                e.title.toLowerCase().includes(queryLower) ||
                e.description.toLowerCase().includes(queryLower)
            );
        }

        return results;
    }

    getStatistics() {
        return {
            totalMembers: this.members.size,
            onlineMembers: Array.from(this.members.values()).filter(m => m.status === 'online').length,
            totalThreads: this.threads.length,
            totalReplies: this.statistics.totalReplies,
            totalEvents: this.events.length,
            upcomingEvents: this.events.filter(e => e.status === 'upcoming').length,
            totalChannels: this.channels.length,
            totalPolls: this.polls.length,
            createdAt: this.createdAt
        };
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            description: this.description,
            owner: this.owner,
            admins: Array.from(this.admins),
            members: Array.from(this.members.entries()),
            invitedMembers: this.invitedMembers,
            settings: this.settings.serialize(),
            channels: this.channels,
            threads: this.threads,
            events: this.events,
            announcements: this.announcements,
            polls: this.polls,
            statistics: this.statistics.serialize(),
            createdAt: this.createdAt,
            updatedAt: this.updatedAt,
            isPublic: this.isPublic,
            isVerified: this.isVerified,
            tags: this.tags
        };
    }
}

export class HubSettings {
    constructor() {
        this.visibility = 'public';
        this.approvalRequired = false;
        this.inviteOnly = false;
        this.maxMembers = 10000;
        this.minAccountAge = 0;
        this.requireVerifiedEmail = false;
        this.allowCustomNicknames = true;
        this.allowCustomAvatars = true;
        this.enableRichPresences = true;
        this.enableTypingIndicators = true;
        this.enableReadReceipts = false;
        this.messageRetention = 365;
        this.autoModeration = true;
        this.censorProfanity = true;
        this.blockExternalLinks = false;
        this.enableThreads = true;
        this.enablePolls = true;
        this.enableEvents = true;
        this.enableAnnouncements = true;
        this.enableReactions = true;
        this.maxReactionsPerMessage = 5;
        this.defaultChannel = null;
        this.welcomeMessage = null;
        this.rules = [];
        this.roles = [];
    }

    serialize() {
        return { ...this };
    }
}

export class HubStatistics {
    constructor() {
        this.totalMembers = 0;
        this.totalThreads = 0;
        this.totalReplies = 0;
        this.totalMessages = 0;
        this.totalEvents = 0;
        this.totalPolls = 0;
        this.dailyActiveUsers = new Map();
        this.monthlyActiveUsers = new Map();
    }

    recordActivity(type) {
        const today = new Date().toDateString();
        const count = this.dailyActiveUsers.get(today) || { threads: 0, replies: 0, messages: 0 };
        count[type]++;
        this.dailyActiveUsers.set(today, count);
    }

    serialize() {
        return {
            totalMembers: this.totalMembers,
            totalThreads: this.totalThreads,
            totalReplies: this.totalReplies,
            totalMessages: this.totalMessages,
            totalEvents: this.totalEvents,
            totalPolls: this.totalPolls
        };
    }
}

export class CommunityManager {
    constructor() {
        this.hubs = new Map();
        this.globalChannels = [];
        this.trendingTopics = new Map();
        this.featuredHubs = [];
        this.categories = new Map();
        this.announcements = [];
        this.notifications = new Map();
        this.reports = [];
    }

    createHub(data) {
        const hub = new CommunityHub();
        hub.name = data.name;
        hub.description = data.description;
        hub.setOwner(data.ownerId);
        hub.isPublic = data.isPublic ?? true;
        hub.tags = data.tags || [];
        hub.region = data.region;
        hub.language = data.language || 'en';

        this.hubs.set(hub.id, hub);

        if (data.createDefaultChannels) {
            this.createDefaultChannels(hub.id);
        }

        return hub;
    }

    createDefaultChannels(hubId) {
        const hub = this.hubs.get(hubId);
        if (!hub) return;

        hub.createChannel({
            name: 'general',
            description: 'General discussion',
            type: 'text',
            createdBy: hub.owner
        });

        hub.createChannel({
            name: 'announcements',
            description: 'Hub announcements',
            type: 'text',
            isSystem: true,
            createdBy: hub.owner
        });

        hub.createChannel({
            name: 'rules',
            description: 'Hub rules and guidelines',
            type: 'text',
            isSystem: true,
            createdBy: hub.owner
        });
    }

    getHub(hubId) {
        return this.hubs.get(hubId);
    }

    getHubsByTag(tag) {
        return Array.from(this.hubs.values()).filter(h => h.tags.includes(tag));
    }

    getHubsByRegion(region) {
        return Array.from(this.hubs.values()).filter(h => h.region === region);
    }

    getPublicHubs(limit = 20, offset = 0) {
        return Array.from(this.hubs.values())
            .filter(h => h.isPublic)
            .sort((a, b) => b.members.size - a.members.size)
            .slice(offset, offset + limit);
    }

    getFeaturedHubs() {
        return this.featuredHubs
            .map(id => this.hubs.get(id))
            .filter(Boolean);
    }

    getTrendingTopics(limit = 10) {
        return Array.from(this.trendingTopics.entries())
            .sort((a, b) => b[1] - a[1])
            .slice(0, limit)
            .map(([topic, count]) => ({ topic, count }));
    }

    recordTopicMention(topic) {
        const count = this.trendingTopics.get(topic) || 0;
        this.trendingTopics.set(topic, count + 1);
    }

    searchHubs(query) {
        const queryLower = query.toLowerCase();
        return Array.from(this.hubs.values()).filter(h =>
            h.name.toLowerCase().includes(queryLower) ||
            h.description.toLowerCase().includes(queryLower) ||
            h.tags.some(tag => tag.toLowerCase().includes(queryLower))
        );
    }

    getCategories() {
        return Array.from(this.categories.entries()).map(([id, category]) => ({
            id,
            ...category
        }));
    }

    createCategory(categoryData) {
        const category = {
            id: 'cat_' + Date.now(),
            name: categoryData.name,
            description: categoryData.description,
            icon: categoryData.icon,
            color: categoryData.color,
            hubs: []
        };

        this.categories.set(category.id, category);
        return category;
    }

    getGlobalFeed(options = {}) {
        const feed = [];
        
        for (const hub of this.hubs.values()) {
            if (!hub.isPublic) continue;

            for (const thread of hub.threads) {
                feed.push({
                    ...thread,
                    hubId: hub.id,
                    hubName: hub.name
                });
            }
        }

        feed.sort((a, b) => b.updatedAt - a.updatedAt);

        if (options.limit) {
            return feed.slice(options.offset || 0, (options.offset || 0) + options.limit);
        }

        return feed;
    }

    getNotifications(userId) {
        return this.notifications.get(userId) || [];
    }

    sendNotification(userId, notification) {
        if (!this.notifications.has(userId)) {
            this.notifications.set(userId, []);
        }

        this.notifications.get(userId).push({
            ...notification,
            id: 'notif_' + Date.now(),
            timestamp: Date.now(),
            read: false
        });
    }

    reportContent(reportData) {
        const report = {
            id: 'report_' + Date.now(),
            type: reportData.type,
            contentId: reportData.contentId,
            hubId: reportData.hubId,
            reporterId: reportData.reporterId,
            reason: reportData.reason,
            details: reportData.details,
            evidence: reportData.evidence || [],
            status: 'pending',
            createdAt: Date.now(),
            resolvedAt: null,
            resolvedBy: null
        };

        this.reports.push(report);
        return report;
    }

    resolveReport(reportId, resolution, resolvedBy) {
        const report = this.reports.find(r => r.id === reportId);
        if (!report) return { success: false };

        report.status = 'resolved';
        report.resolution = resolution;
        report.resolvedBy = resolvedBy;
        report.resolvedAt = Date.now();

        return { success: true };
    }

    serialize() {
        return {
            hubs: Array.from(this.hubs.entries()),
            categories: Array.from(this.categories.entries()),
            trendingTopics: Array.from(this.trendingTopics.entries()),
            featuredHubs: this.featuredHubs,
            reports: this.reports
        };
    }
}

export class SocialFeed {
    constructor() {
        this.posts = [];
        this.userFeeds = new Map();
        this.timelines = new Map();
    }

    createPost(postData) {
        const post = {
            id: 'post_' + Date.now(),
            authorId: postData.authorId,
            authorName: postData.authorName,
            authorAvatar: postData.authorAvatar,
            content: postData.content,
            media: postData.media || [],
            createdAt: Date.now(),
            updatedAt: Date.now(),
            likes: new Map(),
            comments: [],
            shares: 0,
            views: 0,
            isEdited: false,
            isPinned: false,
            isDeleted: false,
            visibility: postData.visibility || 'public',
            tags: postData.tags || [],
            mentions: postData.mentions || [],
            linkedContent: postData.linkedContent || null,
            poll: postData.poll || null,
            location: postData.location || null
        };

        this.posts.push(post);
        return post;
    }

    likePost(postId, userId) {
        const post = this.posts.find(p => p.id === postId);
        if (!post) return { success: false };

        if (post.likes.has(userId)) {
            post.likes.delete(userId);
        } else {
            post.likes.set(userId, Date.now());
        }

        return { success: true, likeCount: post.likes.size };
    }

    commentOnPost(postId, commentData) {
        const post = this.posts.find(p => p.id === postId);
        if (!post) return { success: false };

        const comment = {
            id: 'comment_' + Date.now(),
            postId,
            authorId: commentData.authorId,
            authorName: commentData.authorName,
            content: commentData.content,
            createdAt: Date.now(),
            likes: new Map(),
            replies: []
        };

        post.comments.push(comment);
        return { success: true, comment };
    }

    sharePost(postId, userId) {
        const post = this.posts.find(p => p.id === postId);
        if (!post) return { success: false };

        post.shares++;
        return { success: true, shareCount: post.shares };
    }

    getFeed(userId, options = {}) {
        let feed = this.posts.filter(p => 
            !p.isDeleted &&
            (p.visibility === 'public' || p.authorId === userId)
        );

        if (options.tags && options.tags.length > 0) {
            feed = feed.filter(p => 
                options.tags.some(tag => p.tags.includes(tag))
            );
        }

        if (options.authorId) {
            feed = feed.filter(p => p.authorId === options.authorId);
        }

        if (options.since) {
            feed = feed.filter(p => p.createdAt > options.since);
        }

        feed.sort((a, b) => b.createdAt - a.createdAt);

        if (options.limit) {
            return feed.slice(options.offset || 0, options.offset + options.limit);
        }

        return feed;
    }

    getTrendingPosts(limit = 20) {
        return this.posts
            .filter(p => !p.isDeleted && p.visibility === 'public')
            .map(p => ({
                ...p,
                engagement: p.likes.size + p.comments.length + p.shares,
                trendingScore: (p.likes.size + p.comments.length + p.shares) / 
                    Math.max(1, (Date.now() - p.createdAt) / (60 * 60 * 1000))
            }))
            .sort((a, b) => b.trendingScore - a.trendingScore)
            .slice(0, limit);
    }
}

export default {
    CommunityHub,
    HubSettings,
    HubStatistics,
    CommunityManager,
    SocialFeed
};

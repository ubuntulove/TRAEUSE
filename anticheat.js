export const ViolationType = {
    AIMBOT: 'aimbot',
    TRIGGERBOT: 'triggerbot',
    SPINBOT: 'spinbot',
    SPEEDHACK: 'speedhack',
    TELEPORT: 'teleport',
    WALLHACK: 'wallhack',
    RADAR_HACK: 'radar_hack',
    BUNNY_HOP: 'bunny_hop',
    AUTO_PISTOL: 'auto_pistol',
    NO_RECOIL: 'no_recoil',
    NO_SPREAD: 'no_spread',
    NO_FLASH: 'no_flash',
    NO_SMOKE: 'no_smoke',
    FAKE_LAG: 'fake_lag',
    NAME_ABUSE: 'name_abuse',
    CHAT_SPAM: 'chat_spam',
    VOICE_SPAM: 'voice_spam',
    TEAM_KILL: 'team_kill',
    HOSTAGE_KILL: 'hostage_kill',
    FRIENDLY_FIRE: 'friendly_fire',
    GRIEFING: 'griefing',
    ABUSIVE_BEHAVIOR: 'abusive_behavior',
    EXPLOIT: 'exploit',
    SCRIPT_KIDDIE: 'script_kiddie',
    MACRO: 'macro',
    LOOKUP_TABLE: 'lookup_table',
    MEMORY_MODIFICATION: 'memory_modification',
    ENGINE Manipulation: 'engine_manipulation',
    NETCODE_EXPLOIT: 'netcode_exploit',
    VAC_BYPASS: 'vac_bypass'
};

export const BanDuration = {
    TEMPORARY: 'temporary',
    PERMANENT: 'permanent',
    OVERWATCH: 'overwatch',
    ACCOUNT: 'account',
    HARDWARE: 'hardware',
    IP: 'ip'
};

export const ReportReason = {
    AIMBOT: 'aimbot',
    WALLHACK: 'wallhack',
    SPEEDHACK: 'speedhack',
    TEXT_ABUSE: 'text_abuse',
    VOICE_ABUSE: 'voice_abuse',
    GRIEFING: 'griefing',
    MATCH_STEALING: 'match_stealing',
    IDLE: 'idle',
    SMURFING: 'smurfing',
    BOOSTING: 'boosting',
    MATCH_MACE: 'match_mace',
    EXPLOITING: 'exploiting',
    INAPPROPRIATE_NAME: 'inappropriate_name',
    IMPERSONATION: 'impersonation',
    REAL_LIFE_THREAT: 'real_life_threat',
    CHEATING: 'cheating'
};

export class AntiCheatEvent {
    constructor(type, playerId, severity = 'low') {
        this.id = 'event_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        this.type = type;
        this.playerId = playerId;
        this.severity = severity;
        this.confidence = 0;
        this.weight = 0;
        this.detectedAt = Date.now();
        this.matchId = null;
        this.mapId = null;
        this.gameMode = null;
        this.roundNumber = 0;
        this.data = {};
        this.screenshots = [];
        this.demos = [];
        this.evidence = [];
        this.verdict = 'pending';
        this.verifiedBy = null;
        this.verifiedAt = null;
        this.confirmed = false;
        this.falsePositive = false;
        this.metadata = {};
    }

    addEvidence(evidence) {
        this.evidence.push({
            ...evidence,
            timestamp: Date.now()
        });
        this.recalculateConfidence();
    }

    addScreenshot(url) {
        this.screenshots.push({
            url,
            timestamp: Date.now()
        });
    }

    addDemo(demoId) {
        this.demos.push(demoId);
    }

    recalculateConfidence() {
        let totalWeight = 0;
        let weightedSum = 0;

        for (const evidence of this.evidence) {
            const weight = evidence.weight || 1.0;
            weightedSum += (evidence.confidence || 0) * weight;
            totalWeight += weight;
        }

        this.confidence = totalWeight > 0 ? weightedSum / totalWeight : 0;
        this.weight = totalWeight;
    }

    setVerdict(verdict, verifiedBy = null) {
        this.verdict = verdict;
        this.verifiedBy = verifiedBy;
        this.verifiedAt = Date.now();
        this.confirmed = verdict === 'confirmed';
        this.falsePositive = verdict === 'false_positive';
    }

    isSuspicious() {
        return this.confidence >= 0.5;
    }

    isHighPriority() {
        return this.severity === 'high' || this.confidence >= 0.8;
    }

    serialize() {
        return {
            id: this.id,
            type: this.type,
            playerId: this.playerId,
            severity: this.severity,
            confidence: this.confidence,
            weight: this.weight,
            detectedAt: this.detectedAt,
            matchId: this.matchId,
            mapId: this.mapId,
            gameMode: this.gameMode,
            roundNumber: this.roundNumber,
            data: this.data,
            screenshots: this.screenshots,
            demos: this.demos,
            evidence: this.evidence,
            verdict: this.verdict,
            verifiedBy: this.verifiedBy,
            verifiedAt: this.verifiedAt,
            confirmed: this.confirmed,
            falsePositive: this.falsePositive,
            metadata: this.metadata
        };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class AntiCheatManager {
    constructor() {
        this.events = new Map();
        this.playerFlags = new Map();
        this.playerBans = new Map();
        this.trustedPlayers = new Set();
        this.trustScores = new Map();
        this.enabledChecks = new Set();
        this.checkIntervals = new Map();
        this.suspiciousThreshold = 0.7;
        this.confirmationThreshold = 0.9;
        this.lastCheck = Date.now();
        this.statistics = new AntiCheatStatistics();
        this.observers = [];
        this.autoBanEnabled = false;
        this.manualReviewRequired = true;
        this.overwatchEnabled = true;
        this.vacEnabled = true;
    }

    initialize() {
        this.enableAllChecks();
        this.loadTrustedPlayers();
        this.startMonitoring();
    }

    enableAllChecks() {
        for (const type of Object.values(ViolationType)) {
            this.enabledChecks.add(type);
        }
    }

    disableCheck(type) {
        this.enabledChecks.delete(type);
    }

    enableCheck(type) {
        this.enabledChecks.add(type);
    }

    isCheckEnabled(type) {
        return this.enabledChecks.has(type);
    }

    startMonitoring() {
        setInterval(() => {
            this.runAllChecks();
        }, 5000);

        setInterval(() => {
            this.processPendingEvents();
        }, 60000);
    }

    loadTrustedPlayers() {
    }

    recordEvent(type, playerId, data = {}, severity = 'medium') {
        if (!this.isCheckEnabled(type)) return null;

        const event = new AntiCheatEvent(type, playerId, severity);
        event.data = data;
        event.matchId = data.matchId;
        event.mapId = data.mapId;
        event.gameMode = data.gameMode;
        event.roundNumber = data.roundNumber;

        this.events.set(event.id, event);

        if (!this.playerFlags.has(playerId)) {
            this.playerFlags.set(playerId, {
                events: [],
                totalWeight: 0,
                lastEvent: null,
                banWeight: 0
            });
        }

        const flags = this.playerFlags.get(playerId);
        flags.events.push(event.id);
        flags.totalWeight += event.weight;
        flags.lastEvent = event;

        this.updateBanWeight(playerId);
        this.statistics.recordEvent(event);

        return event;
    }

    updateBanWeight(playerId) {
        const flags = this.playerFlags.get(playerId);
        if (!flags) return;

        let banWeight = 0;
        const recentEvents = this.getRecentEvents(playerId, 30 * 24 * 60 * 60 * 1000);

        for (const event of recentEvents) {
            const timeDecay = Math.exp(-(Date.now() - event.detectedAt) / (90 * 24 * 60 * 60 * 1000));
            banWeight += event.confidence * event.weight * timeDecay;
        }

        flags.banWeight = banWeight;

        if (this.autoBanEnabled && banWeight >= this.confirmationThreshold) {
            this.banPlayer(playerId, 'permanent', 'anticheat_auto');
        }
    }

    getRecentEvents(playerId, maxAge) {
        const flags = this.playerFlags.get(playerId);
        if (!flags) return [];

        const cutoff = Date.now() - maxAge;
        return flags.events
            .map(id => this.events.get(id))
            .filter(e => e && e.detectedAt > cutoff);
    }

    getPlayerFlags(playerId) {
        return this.playerFlags.get(playerId);
    }

    getPlayerSuspicionLevel(playerId) {
        const flags = this.playerFlags.get(playerId);
        if (!flags) return 0;

        const recentEvents = this.getRecentEvents(playerId, 30 * 24 * 60 * 60 * 1000);
        if (recentEvents.length === 0) return 0;

        const totalConfidence = recentEvents.reduce((sum, e) => sum + e.confidence, 0);
        const avgConfidence = totalConfidence / recentEvents.length;

        return Math.min(1, avgConfidence * (1 + Math.log10(recentEvents.length + 1)));
    }

    processPendingEvents() {
        const pending = Array.from(this.events.values())
            .filter(e => e.verdict === 'pending')
            .filter(e => e.confidence >= this.suspiciousThreshold)
            .sort((a, b) => b.confidence - a.confidence);

        for (const event of pending) {
            if (this.manualReviewRequired) {
                this.queueForReview(event);
            }
        }
    }

    queueForReview(event) {
        this.statistics.pendingReviews++;

        for (const observer of this.observers) {
            observer.notifyPendingEvent(event);
        }
    }

    runAllChecks() {
        this.checkAimbot();
        this.checkSpeedhack();
        this.checkWallhack();
        this.checkTeleport();
        this.checkNoRecoil();
        this.checkBhop();
        this.checkAutoPistol();
    }

    checkAimbot() {
    }

    checkSpeedhack() {
    }

    checkWallhack() {
    }

    checkTeleport() {
    }

    checkNoRecoil() {
    }

    checkBhop() {
    }

    checkAutoPistol() {
    }

    banPlayer(playerId, duration, reason, details = {}) {
        const ban = {
            playerId,
            duration,
            reason,
            startDate: Date.now(),
            endDate: duration === BanDuration.PERMANENT ? null : Date.now() + this.parseDuration(duration),
            details,
            issuedBy: 'system',
            appeals: [],
            active: true
        };

        this.playerBans.set(playerId, ban);
        this.statistics.recordBan(reason);
        return ban;
    }

    unbanPlayer(playerId) {
        const ban = this.playerBans.get(playerId);
        if (ban) {
            ban.active = false;
            ban.endDate = Date.now();
            return true;
        }
        return false;
    }

    isPlayerBanned(playerId) {
        const ban = this.playerBans.get(playerId);
        if (!ban) return false;
        if (!ban.active) return false;
        if (ban.endDate && Date.now() > ban.endDate) {
            ban.active = false;
            return false;
        }
        return true;
    }

    getBanInfo(playerId) {
        return this.playerBans.get(playerId);
    }

    appealBan(playerId, reason) {
        const ban = this.playerBans.get(playerId);
        if (!ban) return false;

        ban.appeals.push({
            reason,
            submittedAt: Date.now(),
            status: 'pending'
        });

        return true;
    }

    addTrust(playerId, amount) {
        const current = this.trustScores.get(playerId) || 0;
        this.trustScores.set(playerId, Math.min(1, current + amount));

        if (this.trustScores.get(playerId) >= 0.9) {
            this.trustedPlayers.add(playerId);
        }
    }

    removeTrust(playerId, amount) {
        const current = this.trustScores.get(playerId) || 0;
        this.trustScores.set(playerId, Math.max(0, current - amount));

        if (this.trustScores.get(playerId) < 0.9) {
            this.trustedPlayers.delete(playerId);
        }
    }

    isTrusted(playerId) {
        return this.trustedPlayers.has(playerId);
    }

    getTrustScore(playerId) {
        return this.trustScores.get(playerId) || 0;
    }

    addObserver(observer) {
        this.observers.push(observer);
    }

    removeObserver(observer) {
        const index = this.observers.indexOf(observer);
        if (index >= 0) {
            this.observers.splice(index, 1);
        }
    }

    serialize() {
        return {
            events: Array.from(this.events.entries()).map(([id, e]) => [id, e.serialize()]),
            playerFlags: Array.from(this.playerFlags.entries()),
            playerBans: Array.from(this.playerBans.entries()),
            trustedPlayers: Array.from(this.trustedPlayers),
            trustScores: Array.from(this.trustScores.entries()),
            enabledChecks: Array.from(this.enabledChecks),
            suspiciousThreshold: this.suspiciousThreshold,
            confirmationThreshold: this.confirmationThreshold,
            lastCheck: this.lastCheck,
            statistics: this.statistics.serialize(),
            autoBanEnabled: this.autoBanEnabled,
            manualReviewRequired: this.manualReviewRequired,
            overwatchEnabled: this.overwatchEnabled,
            vacEnabled: this.vacEnabled
        };
    }

    loadState(state) {
        this.events = new Map(state.events?.map(([id, data]) => {
            const event = new AntiCheatEvent();
            event.deserialize(data);
            return [id, event];
        }) || []);

        this.playerFlags = new Map(state.playerFlags || []);
        this.playerBans = new Map(state.playerBans || []);
        this.trustedPlayers = new Set(state.trustedPlayers || []);
        this.trustScores = new Map(state.trustScores || []);
        this.enabledChecks = new Set(state.enabledChecks || []);
        this.suspiciousThreshold = state.suspiciousThreshold || 0.7;
        this.confirmationThreshold = state.confirmationThreshold || 0.9;
        this.autoBanEnabled = state.autoBanEnabled || false;
        this.manualReviewRequired = state.manualReviewRequired ?? true;
        this.overwatchEnabled = state.overwatchEnabled ?? true;
        this.vacEnabled = state.vacEnabled ?? true;

        this.statistics = new AntiCheatStatistics();
        this.statistics.deserialize(state.statistics || {});
    }

    parseDuration(duration) {
        const match = duration.match(/^(\d+)([dhms])$/);
        if (!match) return 0;

        const value = parseInt(match[1]);
        const unit = match[2];

        const multipliers = {
            's': 1000,
            'm': 60 * 1000,
            'h': 60 * 60 * 1000,
            'd': 24 * 60 * 60 * 1000
        };

        return value * (multipliers[unit] || 0);
    }
}

export class AntiCheatStatistics {
    constructor() {
        this.totalEvents = 0;
        this.totalBans = 0;
        this.confirmedCheaters = 0;
        this.falsePositives = 0;
        this.pendingReviews = 0;
        this.autoBans = 0;
        this.manualBans = 0;
        this.appealsFiled = 0;
        this.appealsApproved = 0;
        this.appealsRejected = 0;
        this.eventsByType = new Map();
        this.bansByType = new Map();
        this.bansByDuration = new Map();
        this.cheatersDetectedByDay = new Map();
        this.averageDetectionTime = 0;
        this.averageBanWeight = 0;
        this.trustedPlayers = 0;
        this.suspiciousPlayers = 0;
        this.highPriorityEvents = 0;
        this.recentActivity = [];
    }

    recordEvent(event) {
        this.totalEvents++;

        const count = this.eventsByType.get(event.type) || 0;
        this.eventsByType.set(event.type, count + 1);

        if (event.isHighPriority()) {
            this.highPriorityEvents++;
        }

        this.recentActivity.push({
            type: 'event',
            timestamp: Date.now()
        });

        if (this.recentActivity.length > 1000) {
            this.recentActivity.shift();
        }
    }

    recordBan(reason) {
        this.totalBans++;

        const count = this.bansByType.get(reason) || 0;
        this.bansByType.set(reason, count + 1);

        const today = new Date().toDateString();
        const dayCount = this.cheatersDetectedByDay.get(today) || 0;
        this.cheatersDetectedByDay.set(today, dayCount + 1);
    }

    recordConfirmation() {
        this.confirmedCheaters++;
    }

    recordFalsePositive() {
        this.falsePositives++;
    }

    recordAppeal(approved = false) {
        this.appealsFiled++;
        if (approved) {
            this.appealsApproved++;
        } else {
            this.appealsRejected++;
        }
    }

    recordAutoBan() {
        this.autoBans++;
    }

    recordManualBan() {
        this.manualBans++;
    }

    updateDetectionTime(duration) {
        if (this.confirmedCheaters === 0) {
            this.averageDetectionTime = duration;
        } else {
            this.averageDetectionTime = (this.averageDetectionTime * this.confirmedCheaters + duration) / (this.confirmedCheaters + 1);
        }
    }

    getTopViolationTypes(limit = 10) {
        return Array.from(this.eventsByType.entries())
            .sort((a, b) => b[1] - a[1])
            .slice(0, limit);
    }

    getTopBanReasons(limit = 10) {
        return Array.from(this.bansByType.entries())
            .sort((a, b) => b[1] - a[1])
            .slice(0, limit);
    }

    serialize() {
        return {
            totalEvents: this.totalEvents,
            totalBans: this.totalBans,
            confirmedCheaters: this.confirmedCheaters,
            falsePositives: this.falsePositives,
            pendingReviews: this.pendingReviews,
            autoBans: this.autoBans,
            manualBans: this.manualBans,
            appealsFiled: this.appealsFiled,
            appealsApproved: this.appealsApproved,
            appealsRejected: this.appealsRejected,
            eventsByType: Array.from(this.eventsByType.entries()),
            bansByType: Array.from(this.bansByType.entries()),
            bansByDuration: Array.from(this.bansByDuration.entries()),
            cheatersDetectedByDay: Array.from(this.cheatersDetectedByDay.entries()),
            averageDetectionTime: this.averageDetectionTime,
            averageBanWeight: this.averageBanWeight,
            trustedPlayers: this.trustedPlayers,
            suspiciousPlayers: this.suspiciousPlayers,
            highPriorityEvents: this.highPriorityEvents,
            recentActivity: this.recentActivity
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.eventsByType = new Map(data.eventsByType || []);
        this.bansByType = new Map(data.bansByType || []);
        this.bansByDuration = new Map(data.bansByDuration || []);
        this.cheatersDetectedByDay = new Map(data.cheatersDetectedByDay || []);
    }
}

export class Report {
    constructor(reporterId, reportedId, reason) {
        this.id = 'report_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
        this.reporterId = reporterId;
        this.reportedId = reportedId;
        this.reason = reason;
        this.matchId = null;
        this.matchTimestamp = null;
        this.description = '';
        this.evidence = [];
        this.screenshots = [];
        this.demoUrl = null;
        this.state = 'pending';
        this.priority = 'normal';
        this.assignedTo = null;
        this.assignedAt = null;
        this.resolvedAt = null;
        this.resolution = null;
        this.createdAt = Date.now();
        this.updatedAt = Date.now();
        this.verificationScore = 0;
        this.communityScore = 0;
        this.totalVotes = 0;
        this.upvotes = 0;
        this.downvotes = 0;
        this.category = this.categorizeReason(reason);
        this.subcategory = null;
        this.tags = [];
        this.history = [];
        this.comments = [];
        this.relatedReports = [];
        this.notes = [];
    }

    categorizeReason(reason) {
        const categories = {
            hacking: [ReportReason.AIMBOT, ReportReason.WALLHACK, ReportReason.SPEEDHACK, ReportReason.CHEATING, ReportReason.EXPLOITING],
            harassment: [ReportReason.TEXT_ABUSE, ReportReason.VOICE_ABUSE, ReportReason.ABUSIVE_BEHAVIOR],
            griefing: [ReportReason.GRIEFING, ReportReason.MATCH_STEALING, ReportReason.IDLE],
            integrity: [ReportReason.SMURFING, ReportReason.BOOSTING, ReportReason.MATCH_MACE],
            content: [ReportReason.INAPPROPRIATE_NAME, ReportReason.IMPERSONATION, ReportReason.REAL_LIFE_THREAT]
        };

        for (const [category, reasons] of Object.entries(categories)) {
            if (reasons.includes(reason)) {
                return category;
            }
        }
        return 'other';
    }

    addEvidence(evidence) {
        this.evidence.push({
            ...evidence,
            timestamp: Date.now()
        });
        this.updatedAt = Date.now();
    }

    addScreenshot(url, description = '') {
        this.screenshots.push({
            url,
            description,
            timestamp: Date.now()
        });
        this.updatedAt = Date.now();
    }

    setMatch(matchId, timestamp) {
        this.matchId = matchId;
        this.matchTimestamp = timestamp;
        this.updatedAt = Date.now();
    }

    vote(voterId, isUpvote) {
        if (isUpvote) {
            this.upvotes++;
        } else {
            this.downvotes++;
        }
        this.totalVotes++;
        this.communityScore = (this.upvotes - this.downvotes) / this.totalVotes;
        this.updatedAt = Date.now();
    }

    updateVerificationScore(score) {
        this.verificationScore = score;
        this.updatedAt = Date.now();
    }

    assign(assignedTo) {
        this.assignedTo = assignedTo;
        this.assignedAt = Date.now();
        this.state = 'in_review';
        this.addHistory('assigned', { assignedTo });
        this.updatedAt = Date.now();
    }

    resolve(resolution, notes = '') {
        this.state = 'resolved';
        this.resolution = resolution;
        this.resolvedAt = Date.now();
        if (notes) {
            this.addHistory('resolved', { resolution, notes });
        }
        this.updatedAt = Date.now();
    }

    addComment(commenterId, comment) {
        this.comments.push({
            commenterId,
            comment,
            timestamp: Date.now()
        });
        this.updatedAt = Date.now();
    }

    addNote(note) {
        this.notes.push({
            note,
            timestamp: Date.now()
        });
        this.updatedAt = Date.now();
    }

    addHistory(action, data = {}) {
        this.history.push({
            action,
            data,
            timestamp: Date.now()
        });
        if (this.history.length > 100) {
            this.history.shift();
        }
    }

    addRelatedReport(reportId) {
        if (!this.relatedReports.includes(reportId)) {
            this.relatedReports.push(reportId);
        }
    }

    getPriority() {
        if (this.communityScore > 0.8) return 'high';
        if (this.verificationScore > 0.7) return 'high';
        if (this.screenshots.length >= 3) return 'high';
        if (this.category === 'hacking') return 'high';
        return 'normal';
    }

    isValid() {
        if (this.reporterId === this.reportedId) return false;
        if (!Object.values(ReportReason).includes(this.reason)) return false;
        return true;
    }

    serialize() {
        return {
            id: this.id,
            reporterId: this.reporterId,
            reportedId: this.reportedId,
            reason: this.reason,
            matchId: this.matchId,
            matchTimestamp: this.matchTimestamp,
            description: this.description,
            evidence: this.evidence,
            screenshots: this.screenshots,
            demoUrl: this.demoUrl,
            state: this.state,
            priority: this.priority,
            assignedTo: this.assignedTo,
            assignedAt: this.assignedAt,
            resolvedAt: this.resolvedAt,
            resolution: this.resolution,
            createdAt: this.createdAt,
            updatedAt: this.updatedAt,
            verificationScore: this.verificationScore,
            communityScore: this.communityScore,
            totalVotes: this.totalVotes,
            upvotes: this.upvotes,
            downvotes: this.downvotes,
            category: this.category,
            subcategory: this.subcategory,
            tags: this.tags,
            history: this.history,
            comments: this.comments,
            relatedReports: this.relatedReports,
            notes: this.notes
        };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class ReportManager {
    constructor() {
        this.reports = new Map();
        this.playerReportCounts = new Map();
        this.reporterStats = new Map();
        this.statistics = new ReportStatistics();
        this.pendingReports = [];
        this.assignedReports = new Map();
        this.resolvedReports = [];
        this.categories = ['hacking', 'harassment', 'griefing', 'integrity', 'content', 'other'];
        this.autoAssignEnabled = true;
        this.autoAssignThreshold = 3;
        this.reviewers = new Set();
        this.escalationThreshold = 5;
    }

    createReport(reporterId, reportedId, reason, data = {}) {
        const report = new Report(reporterId, reportedId, reason);

        if (data.description) report.description = data.description;
        if (data.matchId) report.setMatch(data.matchId, data.matchTimestamp);
        if (data.evidence) {
            for (const evidence of data.evidence) {
                report.addEvidence(evidence);
            }
        }
        if (data.screenshots) {
            for (const screenshot of data.screenshots) {
                report.addScreenshot(screenshot.url, screenshot.description);
            }
        }

        if (!report.isValid()) {
            return null;
        }

        this.reports.set(report.id, report);
        this.updatePlayerReportCount(reportedId);
        this.updateReporterStats(reporterId);
        this.statistics.recordReport(report);

        if (this.autoAssignEnabled && this.getPlayerReportCount(reportedId) >= this.autoAssignThreshold) {
            this.queueForAutoAssign(report);
        }

        return report;
    }

    getReport(reportId) {
        return this.reports.get(reportId);
    }

    getReportsByPlayer(playerId) {
        return Array.from(this.reports.values())
            .filter(r => r.reportedId === playerId)
            .sort((a, b) => b.createdAt - a.createdAt);
    }

    getReportsByReporter(reporterId) {
        return Array.from(this.reports.values())
            .filter(r => r.reporterId === reporterId)
            .sort((a, b) => b.createdAt - a.createdAt);
    }

    getReportsByState(state) {
        return Array.from(this.reports.values())
            .filter(r => r.state === state)
            .sort((a, b) => {
                const priorityOrder = { high: 0, normal: 1, low: 2 };
                return (priorityOrder[a.priority] || 1) - (priorityOrder[b.priority] || 1);
            });
    }

    getReportsByCategory(category) {
        return Array.from(this.reports.values())
            .filter(r => r.category === category);
    }

    getPendingReports() {
        return this.getReportsByState('pending');
    }

    getInReviewReports() {
        return this.getReportsByState('in_review');
    }

    getResolvedReports() {
        return this.getReportsByState('resolved');
    }

    updatePlayerReportCount(playerId) {
        const count = this.playerReportCounts.get(playerId) || 0;
        this.playerReportCounts.set(playerId, count + 1);
    }

    getPlayerReportCount(playerId) {
        return this.playerReportCounts.get(playerId) || 0;
    }

    updateReporterStats(reporterId) {
        if (!this.reporterStats.has(reporterId)) {
            this.reporterStats.set(reporterId, {
                reportsSubmitted: 0,
                reportsActioned: 0,
                accuracy: 0,
                lastReportAt: null
            });
        }

        const stats = this.reporterStats.get(reporterId);
        stats.reportsSubmitted++;
        stats.lastReportAt = Date.now();
    }

    queueForAutoAssign(report) {
        this.pendingReports.push(report);
    }

    assignReport(reportId, assignedTo) {
        const report = this.reports.get(reportId);
        if (!report) return false;

        report.assign(assignedTo);
        this.assignedReports.set(assignedTo, reportId);
        this.statistics.recordAssignment();
        return true;
    }

    resolveReport(reportId, resolution, notes = '') {
        const report = this.reports.get(reportId);
        if (!report) return false;

        report.resolve(resolution, notes);

        if (this.assignedReports.has(report.assignedTo)) {
            this.assignedReports.delete(report.assignedTo);
        }

        this.resolvedReports.push(report);
        this.statistics.recordResolution(resolution);

        if (resolution === 'confirmed') {
            this.statistics.recordConfirmation();
        } else if (resolution === 'false_report') {
            this.statistics.recordFalseReport();
        }

        return true;
    }

    voteOnReport(reportId, voterId, isUpvote) {
        const report = this.reports.get(reportId);
        if (!report) return false;

        report.vote(voterId, isUpvote);
        return true;
    }

    getTopReporters(limit = 10) {
        return Array.from(this.reporterStats.entries())
            .map(([id, stats]) => ({ playerId: id, ...stats }))
            .sort((a, b) => b.reportsSubmitted - a.reportsSubmitted)
            .slice(0, limit);
    }

    getMostReportedPlayers(limit = 10) {
        return Array.from(this.playerReportCounts.entries())
            .map(([id, count]) => ({ playerId: id, count }))
            .sort((a, b) => b.count - a.count)
            .slice(0, limit);
    }

    addReviewer(playerId) {
        this.reviewers.add(playerId);
    }

    removeReviewer(playerId) {
        this.reviewers.delete(playerId);
    }

    getAvailableReviewers() {
        return Array.from(this.reviewers);
    }

    queueForOverwatch(report) {
        this.statistics.queueForOverwatch();
    }

    serialize() {
        return {
            reports: Array.from(this.reports.entries()).map(([id, r]) => [id, r.serialize()]),
            playerReportCounts: Array.from(this.playerReportCounts.entries()),
            reporterStats: Array.from(this.reporterStats.entries()),
            statistics: this.statistics.serialize(),
            pendingReports: this.pendingReports.map(r => r.id),
            assignedReports: Array.from(this.assignedReports.entries()),
            resolvedReports: this.resolvedReports.map(r => r.id),
            categories: this.categories,
            autoAssignEnabled: this.autoAssignEnabled,
            autoAssignThreshold: this.autoAssignThreshold,
            reviewers: Array.from(this.reviewers),
            escalationThreshold: this.escalationThreshold
        };
    }

    loadState(state) {
        this.reports = new Map(state.reports?.map(([id, data]) => {
            const report = new Report();
            report.deserialize(data);
            return [id, report];
        }) || []);

        this.playerReportCounts = new Map(state.playerReportCounts || []);
        this.reporterStats = new Map(state.reporterStats || []);

        this.statistics = new ReportStatistics();
        this.statistics.deserialize(state.statistics || {});

        this.pendingReports = state.pendingReports?.map(id => this.reports.get(id)).filter(r => r) || [];
        this.assignedReports = new Map(state.assignedReports || []);
        this.resolvedReports = state.resolvedReports?.map(id => this.reports.get(id)).filter(r => r) || [];
        this.reviewers = new Set(state.reviewers || []);
        this.autoAssignEnabled = state.autoAssignEnabled ?? true;
        this.autoAssignThreshold = state.autoAssignThreshold || 3;
        this.escalationThreshold = state.escalationThreshold || 5;
    }
}

export class ReportStatistics {
    constructor() {
        this.totalReports = 0;
        this.reportsByCategory = new Map();
        this.reportsByReason = new Map();
        this.reportsByState = new Map();
        this.resolutionStats = new Map();
        this.confirmationRate = 0;
        this.falseReportRate = 0;
        this.averageReviewTime = 0;
        this.totalReviewed = 0;
        this.overwatchQueue = 0;
        this.recentActivity = [];
    }

    recordReport(report) {
        this.totalReports++;

        const categoryCount = this.reportsByCategory.get(report.category) || 0;
        this.reportsByCategory.set(report.category, categoryCount + 1);

        const reasonCount = this.reportsByReason.get(report.reason) || 0;
        this.reportsByReason.set(report.reason, reasonCount + 1);

        const stateCount = this.reportsByState.get(report.state) || 0;
        this.reportsByState.set(report.state, stateCount + 1);

        this.recentActivity.push({
            type: 'report',
            category: report.category,
            timestamp: Date.now()
        });

        if (this.recentActivity.length > 1000) {
            this.recentActivity.shift();
        }
    }

    recordResolution(resolution) {
        const count = this.resolutionStats.get(resolution) || 0;
        this.resolutionStats.set(resolution, count + 1);
        this.totalReviewed++;

        this.calculateRates();
    }

    recordConfirmation() {
        this.confirmationRate = (this.confirmationRate * (this.totalReviewed - 1) + 1) / this.totalReviewed;
    }

    recordFalseReport() {
        this.falseReportRate = (this.falseReportRate * (this.totalReviewed - 1) + 1) / this.totalReviewed;
    }

    calculateRates() {
        if (this.totalReviewed > 0) {
            const confirmed = this.resolutionStats.get('confirmed') || 0;
            const falseReports = this.resolutionStats.get('false_report') || 0;
            this.confirmationRate = confirmed / this.totalReviewed;
            this.falseReportRate = falseReports / this.totalReviewed;
        }
    }

    queueForOverwatch() {
        this.overwatchQueue++;
    }

    recordOverwatchReview() {
        this.overwatchQueue--;
    }

    serialize() {
        return {
            totalReports: this.totalReports,
            reportsByCategory: Array.from(this.reportsByCategory.entries()),
            reportsByReason: Array.from(this.reportsByReason.entries()),
            reportsByState: Array.from(this.reportsByState.entries()),
            resolutionStats: Array.from(this.resolutionStats.entries()),
            confirmationRate: this.confirmationRate,
            falseReportRate: this.falseReportRate,
            averageReviewTime: this.averageReviewTime,
            totalReviewed: this.totalReviewed,
            overwatchQueue: this.overwatchQueue,
            recentActivity: this.recentActivity
        };
    }

    deserialize(data) {
        Object.assign(this, data);
        this.reportsByCategory = new Map(data.reportsByCategory || []);
        this.reportsByReason = new Map(data.reportsByReason || []);
        this.reportsByState = new Map(data.reportsByState || []);
        this.resolutionStats = new Map(data.resolutionStats || []);
    }
}

export class TrustFactor {
    constructor(playerId) {
        this.playerId = playerId;
        this.score = 0.5;
        this.previousScores = [];
        this.factors = {
            accountAge: 0,
            phoneVerified: 0,
            emailVerified: 0,
           VACBans: 0,
            gameBans: 0,
            reportRate: 0,
            commendationRate: 0,
            playTime: 0,
            competitiveWinRate: 0,
            recentActivity: 0,
            priveLevel: 0,
            communityBanned: 0,
            tradeBan: 0,
            overwatchScore: 0,
            matchMakingBans: 0,
            sameIPRate: 0,
            paymentVerified: 0
        };
        this.bonusFactors = {
            primeStatus: false,
            bassetLevel: 0,
            workshopContributor: false,
            tournamentParticipant: false,
            veteranPlayer: false,
            communityContributor: false
        };
        this.penalties = [];
        this.lastUpdated = Date.now();
        this.history = [];
        this.decayRate = 0.001;
        this.boostRate = 0.005;
        this.minimumThreshold = 0.1;
        this.maximumThreshold = 0.99;
    }

    updateScore() {
        this.previousScores.push(this.score);
        if (this.previousScores.length > 100) {
            this.previousScores.shift();
        }

        let baseScore = 0.5;
        
        if (this.factors.accountAge > 365 * 24 * 60 * 60 * 1000) {
            baseScore += 0.05;
        }
        if (this.factors.phoneVerified) {
            baseScore += 0.1;
        }
        if (this.factors.emailVerified) {
            baseScore += 0.02;
        }
        if (this.factors.VACBans > 0) {
            baseScore -= 0.1 * this.factors.VACBans;
        }
        if (this.factors.gameBans > 0) {
            baseScore -= 0.15 * this.factors.gameBans;
        }
        if (this.factors.reportRate > 0.1) {
            baseScore -= 0.2 * Math.min(1, this.factors.reportRate);
        }
        if (this.factors.commendationRate > 0.5) {
            baseScore += 0.1;
        }
        if (this.factors.playTime > 1000 * 60 * 60) {
            baseScore += 0.05;
        }
        if (this.factors.competitiveWinRate > 0.5) {
            baseScore += 0.05;
        }
        if (this.factors.priveLevel >= 21) {
            baseScore += 0.05;
        }
        if (this.factors.communityBanned > 0) {
            baseScore -= 0.3;
        }
        if (this.factors.tradeBan > 0) {
            baseScore -= 0.2;
        }
        if (this.factors.overwatchScore > 0.7) {
            baseScore += 0.1;
        }
        if (this.factors.overwatchScore < 0.3) {
            baseScore -= 0.2;
        }
        if (this.factors.matchMakingBans > 0) {
            baseScore -= 0.15 * this.factors.matchMakingBans;
        }

        if (this.bonusFactors.primeStatus) baseScore += 0.1;
        if (this.bonusFactors.bassetLevel >= 10) baseScore += 0.05;
        if (this.bonusFactors.workshopContributor) baseScore += 0.05;
        if (this.bonusFactors.tournamentParticipant) baseScore += 0.05;
        if (this.bonusFactors.veteranPlayer) baseScore += 0.05;
        if (this.bonusFactors.communityContributor) baseScore += 0.05;

        for (const penalty of this.penalties) {
            if (penalty.expiresAt > Date.now()) {
                baseScore -= penalty.severity;
            }
        }

        const timeSinceUpdate = Date.now() - this.lastUpdated;
        const decayDays = timeSinceUpdate / (24 * 60 * 60 * 1000);
        baseScore -= this.decayRate * decayDays;

        this.score = Math.max(this.minimumThreshold, Math.min(this.maximumThreshold, baseScore));
        this.lastUpdated = Date.now();

        this.history.push({
            score: this.score,
            timestamp: Date.now()
        });

        if (this.history.length > 365) {
            this.history.shift();
        }

        return this.score;
    }

    addPenalty(severity, duration, reason) {
        this.penalties.push({
            severity,
            duration,
            reason,
            appliedAt: Date.now(),
            expiresAt: Date.now() + duration
        });
        this.updateScore();
    }

    removePenalty(reason) {
        this.penalties = this.penalties.filter(p => p.reason !== reason);
        this.updateScore();
    }

    getTrustLevel() {
        if (this.score >= 0.9) return 'trusted';
        if (this.score >= 0.7) return 'reliable';
        if (this.score >= 0.5) return 'neutral';
        if (this.score >= 0.3) return 'caution';
        return 'restricted';
    }

    shouldMatchWith(trustFactor, tolerance = 0.3) {
        return Math.abs(this.score - trustFactor.score) <= tolerance;
    }

    getMatchmakingPool() {
        if (this.score >= 0.8) return 'high_trust';
        if (this.score >= 0.6) return 'medium_trust';
        if (this.score >= 0.4) return 'low_trust';
        return 'very_low_trust';
    }

    calculateReportWeight(report) {
        let weight = 1.0;
        
        if (this.getTrustLevel() === 'trusted') weight *= 0.5;
        if (this.getTrustLevel() === 'restricted') weight *= 2.0;
        
        const recentReports = this.penalties.filter(p => 
            p.appliedAt > Date.now() - 7 * 24 * 60 * 60 * 1000
        ).length;
        weight *= (1 + recentReports * 0.1);
        
        return weight;
    }

    getDetailedBreakdown() {
        return {
            score: this.score,
            trustLevel: this.getTrustLevel(),
            matchmakingPool: this.getMatchmakingPool(),
            factors: { ...this.factors },
            bonusFactors: { ...this.bonusFactors },
            activePenalties: this.penalties.filter(p => p.expiresAt > Date.now()),
            scoreHistory: this.history.slice(-30),
            lastUpdated: this.lastUpdated
        };
    }

    serialize() {
        return {
            playerId: this.playerId,
            score: this.score,
            previousScores: this.previousScores,
            factors: this.factors,
            bonusFactors: this.bonusFactors,
            penalties: this.penalties,
            lastUpdated: this.lastUpdated,
            history: this.history
        };
    }

    deserialize(data) {
        Object.assign(this, data);
    }
}

export class VACSystem {
    constructor() {
        this.enabled = true;
        this.modules = new Map();
        this.signatures = new Map();
        this.heuristics = new Map();
        this.hardwareBans = new Set();
        this.vacBannedAccounts = new Set();
        this.vacBannedSerials = new Set();
        this.lastScan = null;
        this.scanInterval = 60000;
        this.moduleObservers = [];
        this.kernelModeProtection = false;
        this.userModeProtection = true;
        this.cloudIntegration = true;
    }

    initialize() {
        this.initializeModules();
        this.loadSignatures();
        this.initializeHeuristics();
        this.startProtection();
    }

    initializeModules() {
        this.modules.set('memory_scanner', new MemoryScannerModule());
        this.modules.set('file_integrity', new FileIntegrityModule());
        this.modules.set('behavior_monitor', new BehaviorMonitorModule());
        this.modules.set('network_inspector', new NetworkInspectorModule());
        this.modules.set('driver_checker', new DriverCheckerModule());
        this.modules.set('registry_watcher', new RegistryWatcherModule());
        this.modules.set('process_monitor', new ProcessMonitorModule());
        this.modules.set('hook_detector', new HookDetectorModule());
    }

    loadSignatures() {
        this.signatures.set('aimbot_common', {
            hash: 'sha256:abc123...',
            type: 'memory_pattern',
            severity: 'high',
            description: 'Common aimbot pattern'
        });
        this.signatures.set('wallhack_common', {
            hash: 'sha256:def456...',
            type: 'memory_pattern',
            severity: 'high',
            description: 'Common wallhack pattern'
        });
        this.signatures.set('speedhack_common', {
            hash: 'sha256:ghi789...',
            type: 'timing_anomaly',
            severity: 'medium',
            description: 'Common speedhack pattern'
        });
    }

    initializeHeuristics() {
        this.heuristics.set('unusual_accuracy', {
            check: (playerData) => {
                if (playerData.headshotPercentage > 0.8 && playerData.averageKillsPerRound > 2) {
                    return { detected: true, confidence: 0.7 };
                }
                return { detected: false, confidence: 0 };
            },
            weight: 1.5
        });

        this.heuristics.set('impossible_reactions', {
            check: (playerData) => {
                if (playerData.averageReactionTime < 100) {
                    return { detected: true, confidence: 0.6 };
                }
                return { detected: false, confidence: 0 };
            },
            weight: 1.2
        });

        this.heuristics.set('perfect_tracking', {
            check: (playerData) => {
                if (playerData.aimStability > 0.95) {
                    return { detected: true, confidence: 0.5 };
                }
                return { detected: false, confidence: 0 };
            },
            weight: 1.0
        });

        this.heuristics.set('consistent_spread', {
            check: (playerData) => {
                if (playerData.spreadVariance < 0.01) {
                    return { detected: true, confidence: 0.8 };
                }
                return { detected: false, confidence: 0 };
            },
            weight: 1.8
        });
    }

    startProtection() {
        if (this.enabled) {
            setInterval(() => this.runFullScan(), this.scanInterval);
            this.registerCallbacks();
        }
    }

    registerCallbacks() {
        for (const [name, module] of this.modules) {
            if (typeof module.registerCallback === 'function') {
                module.registerCallback((event) => this.handleModuleEvent(name, event));
            }
        }
    }

    handleModuleEvent(moduleName, event) {
        if (event.severity === 'critical') {
            this.flagForVACBan(event);
        }
    }

    runFullScan() {
        this.lastScan = Date.now();
        const results = {
            timestamp: this.lastScan,
            modules: {},
            signatures: {},
            heuristics: {},
            verdict: 'clean'
        };

        for (const [name, module] of this.modules) {
            results.modules[name] = module.scan();
        }

        const signatureResults = this.checkSignatures();
        if (signatureResults.detected) {
            results.signatures = signatureResults;
            results.verdict = 'detected';
        }

        const heuristicResults = this.runHeuristics();
        if (heuristicResults.violations > 0) {
            results.heuristics = heuristicResults;
            if (heuristicResults.confidence > 0.8) {
                results.verdict = 'detected';
            }
        }

        return results;
    }

    checkSignatures() {
        for (const [name, signature] of this.signatures) {
            if (this.scanForSignature(signature)) {
                return {
                    detected: true,
                    signature: name,
                    signatureData: signature,
                    confidence: 1.0
                };
            }
        }
        return { detected: false };
    }

    scanForSignature(signature) {
        return false;
    }

    runHeuristics() {
        let totalWeight = 0;
        let violations = 0;
        const results = [];

        for (const [name, heuristic] of this.heuristics) {
            const playerData = this.getPlayerDataForHeuristic();
            const result = heuristic.check(playerData);
            
            if (result.detected) {
                violations++;
                totalWeight += heuristic.weight * result.confidence;
                results.push({
                    heuristic: name,
                    confidence: result.confidence,
                    weight: heuristic.weight
                });
            }
        }

        return {
            violations,
            totalWeight,
            confidence: Math.min(1, totalWeight),
            results
        };
    }

    getPlayerDataForHeuristic() {
        return {
            headshotPercentage: 0,
            averageKillsPerRound: 0,
            averageReactionTime: 0,
            aimStability: 0,
            spreadVariance: 0
        };
    }

    flagForVACBan(event) {
        this.vacBannedAccounts.add(event.playerId);
        
        if (event.hardwareId) {
            this.hardwareBans.add(event.hardwareId);
        }
    }

    isVACBanned(playerId) {
        return this.vacBannedAccounts.has(playerId);
    }

    isHardwareBanned(hardwareId) {
        return this.hardwareBans.has(hardwareId);
    }

    getBanStatus(playerId) {
        if (this.isVACBanned(playerId)) {
            return {
                banned: true,
                type: 'vac',
                permanent: true,
                detectionMethod: 'automated'
            };
        }
        return { banned: false };
    }

    serialize() {
        return {
            enabled: this.enabled,
            modules: Array.from(this.modules.keys()),
            signatures: Array.from(this.signatures.entries()),
            hardwareBans: Array.from(this.hardwareBans),
            vacBannedAccounts: Array.from(this.vacBannedAccounts),
            vacBannedSerials: Array.from(this.vacBannedSerials),
            lastScan: this.lastScan,
            scanInterval: this.scanInterval,
            kernelModeProtection: this.kernelModeProtection,
            userModeProtection: this.userModeProtection,
            cloudIntegration: this.cloudIntegration
        };
    }
}

class MemoryScannerModule {
    constructor() {
        this.enabled = true;
        this.scanDepth = 'deep';
        this.heapOnly = false;
        this.modulePatterns = [];
    }

    scan() {
        const results = {
            module: 'memory_scanner',
            status: 'completed',
            threats: [],
            scanTime: Date.now(),
            regions: 0,
            violations: 0
        };

        return results;
    }
}

class FileIntegrityModule {
    constructor() {
        this.enabled = true;
        this.checkedFiles = new Map();
        this.knownHashes = new Map();
    }

    scan() {
        return {
            module: 'file_integrity',
            status: 'completed',
            verified: true,
            violations: 0
        };
    }
}

class BehaviorMonitorModule {
    constructor() {
        this.enabled = true;
        this.behaviorLog = [];
        this.anomalyThreshold = 0.7;
    }

    scan() {
        return {
            module: 'behavior_monitor',
            status: 'completed',
            anomalies: [],
            violations: 0
        };
    }
}

class NetworkInspectorModule {
    constructor() {
        this.enabled = true;
        this.packetLog = [];
        this.suspiciousPatterns = [];
    }

    scan() {
        return {
            module: 'network_inspector',
            status: 'completed',
            packetsAnalyzed: 0,
            violations: 0
        };
    }
}

class DriverCheckerModule {
    constructor() {
        this.enabled = true;
        this.trustedDrivers = new Set();
        this.suspiciousDrivers = new Set();
    }

    scan() {
        return {
            module: 'driver_checker',
            status: 'completed',
            driversChecked: 0,
            violations: 0
        };
    }
}

class RegistryWatcherModule {
    constructor() {
        this.enabled = true;
        this.watchedKeys = [];
        this.modifications = [];
    }

    scan() {
        return {
            module: 'registry_watcher',
            status: 'completed',
            keysMonitored: 0,
            violations: 0
        };
    }
}

class ProcessMonitorModule {
    constructor() {
        this.enabled = true;
        this.processes = new Map();
        this.injections = [];
    }

    scan() {
        return {
            module: 'process_monitor',
            status: 'completed',
            processesChecked: 0,
            violations: 0
        };
    }
}

class HookDetectorModule {
    constructor() {
        this.enabled = true;
        this.hooks = [];
        this.inlineHooks = [];
    }

    scan() {
        return {
            module: 'hook_detector',
            status: 'completed',
            hooksFound: 0,
            violations: 0
        };
    }
}

export class OverwatchSystem {
    constructor() {
        this.enabled = true;
        this.investigators = new Map();
        this.cases = new Map();
        this.caseQueue = [];
        this.completedCases = [];
        this.statistics = new OverwatchStatistics();
        this.priorityWeights = {
            aimbot: 1.5,
            wallhack: 1.5,
            speedhack: 1.3,
            griefing: 1.0,
            text_abuse: 0.8,
            voice_abuse: 0.8
        };
        this.rewards = {
            perCase: 100,
            highQualityReview: 50,
            streakBonus: 25
        };
        this.caseTimeout = 7 * 24 * 60 * 60 * 1000;
        this.minInvestigationScore = 80;
        this.consensusThreshold = 0.6;
        this.caseReplayEndpoint = '';
    }

    initialize() {
        this.loadInvestigators();
        this.processCaseQueue();
    }

    loadInvestigators() {
        const eligiblePlayers = this.getEligibleInvestigators();
        for (const playerId of eligiblePlayers) {
            this.investigators.set(playerId, new Investigator(playerId));
        }
    }

    getEligibleInvestigators() {
        return [];
    }

    createCase(evidence) {
        const caseData = {
            id: 'ow_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            suspectId: evidence.suspectId,
            evidenceType: evidence.type,
            priority: this.priorityWeights[evidence.type] || 1.0,
            matchId: evidence.matchId,
            roundData: evidence.rounds || [],
            demos: evidence.demos || [],
            screenshots: evidence.screenshots || [],
            communityFlags: evidence.communityFlags || 0,
            aiScore: evidence.aiScore || 0,
            reportCount: evidence.reportCount || 0,
            caseWeight: this.calculateCaseWeight(evidence),
            createdAt: Date.now(),
            expiresAt: Date.now() + this.caseTimeout,
            assignedInvestigators: [],
            verdicts: [],
            finalVerdict: null,
            status: 'pending',
            rewardPool: 0
        };

        this.cases.set(caseData.id, caseData);
        this.caseQueue.push(caseData.id);
        this.sortCaseQueue();
        this.statistics.recordCaseCreated(caseData);

        return caseData.id;
    }

    calculateCaseWeight(evidence) {
        let weight = 0;

        weight += (evidence.aiScore || 0.5) * 0.4;
        weight += Math.min(1, (evidence.reportCount || 0) / 10) * 0.3;
        weight += (evidence.communityFlags || 0) / 100 * 0.2;

        if (evidence.demos?.length > 0) weight += 0.1;

        return weight;
    }

    sortCaseQueue() {
        this.caseQueue.sort((a, b) => {
            const caseA = this.cases.get(a);
            const caseB = this.cases.get(b);
            return caseB.caseWeight - caseA.caseWeight;
        });
    }

    assignCase(caseId, investigatorId) {
        const caseData = this.cases.get(caseId);
        const investigator = this.investigators.get(investigatorId);

        if (!caseData || !investigator) return false;
        if (caseData.status !== 'pending') return false;
        if (investigator.activeCases >= investigator.maxActiveCases) return false;

        caseData.assignedInvestigators.push(investigatorId);
        caseData.status = 'in_progress';
        investigator.activeCases++;
        investigator.casesAssigned.push(caseId);

        return true;
    }

    submitVerdict(caseId, investigatorId, verdict) {
        const caseData = this.cases.get(caseId);
        const investigator = this.investigators.get(investigatorId);

        if (!caseData || !investigator) return false;

        const verdictData = {
            investigatorId,
            verdict,
            confidence: verdict.confidence,
            submittedAt: Date.now(),
            rewardEarned: 0
        };

        caseData.verdicts.push(verdictData);
        investigator.activeCases--;
        investigator.casesCompleted.push(caseId);
        investigator.totalCases++;

        if (caseData.verdicts.length >= this.getRequiredInvestigators(caseData)) {
            this.finalizeCase(caseId);
        }

        return true;
    }

    getRequiredInvestigators(caseData) {
        if (caseData.caseWeight > 0.8) return 5;
        if (caseData.caseWeight > 0.6) return 4;
        return 3;
    }

    finalizeCase(caseId) {
        const caseData = this.cases.get(caseId);
        if (!caseData) return;

        const verdictCounts = {};
        for (const verdict of caseData.verdicts) {
            const v = verdict.verdict;
            verdictCounts[v] = (verdictCounts[v] || 0) + 1;
        }

        const total = caseData.verdicts.length;
        for (const [verdict, count] of Object.entries(verdictCounts)) {
            if (count / total >= this.consensusThreshold) {
                caseData.finalVerdict = verdict;
                break;
            }
        }

        caseData.status = caseData.finalVerdict ? 'completed' : 'inconclusive';

        for (const verdict of caseData.verdicts) {
            const investigator = this.investigators.get(verdict.investigatorId);
            if (investigator && verdict.verdict === caseData.finalVerdict) {
                verdict.rewardEarned = this.calculateReward(investigator);
                investigator.reputation += verdict.rewardEarned;
            }
        }

        this.completedCases.push(caseId);
        this.statistics.recordCaseCompleted(caseData);
    }

    calculateReward(investigator) {
        let reward = this.rewards.perCase;

        if (investigator.streak >= 5) {
            reward += this.rewards.streakBonus;
        }

        if (investigator.accuracy > 0.9) {
            reward += this.rewards.highQualityReview;
        }

        return reward;
    }

    processCaseQueue() {
        setInterval(() => {
            const unassignedCases = this.caseQueue.filter(id => {
                const caseData = this.cases.get(id);
                return caseData?.status === 'pending';
            });

            for (const caseId of unassignedCases) {
                const availableInvestigators = this.getAvailableInvestigators();
                for (const investigatorId of availableInvestigators) {
                    this.assignCase(caseId, investigatorId);
                }
            }
        }, 60000);
    }

    getAvailableInvestigators() {
        const available = [];
        for (const [id, investigator] of this.investigators) {
            if (investigator.activeCases < investigator.maxActiveCases) {
                available.push(id);
            }
        }
        return available;
    }

    getCaseForReview(investigatorId, caseId) {
        const caseData = this.cases.get(caseId);
        if (!caseData) return null;
        if (!caseData.assignedInvestigators.includes(investigatorId)) return null;

        return {
            id: caseData.id,
            suspectId: caseData.suspectId,
            evidenceType: caseData.evidenceType,
            matchId: caseData.matchId,
            rounds: caseData.roundData,
            demos: caseData.demos,
            screenshots: caseData.screenshots,
            verdictOptions: this.getVerdictOptions(caseData.evidenceType)
        };
    }

    getVerdictOptions(evidenceType) {
        if (evidenceType === 'griefing' || evidenceType === 'text_abuse' || evidenceType === 'voice_abuse') {
            return [
                { id: 'confirmed', label: 'Confirmed', severity: 'low' },
                { id: 'insufficient_evidence', label: 'Insufficient Evidence', severity: 'none' },
                { id: 'false_report', label: 'False Report', severity: 'none' }
            ];
        }

        return [
            { id: 'aimbot', label: 'Aimbot (Evidence)', severity: 'high' },
            { id: 'wallhack', label: 'Wallhack (Evidence)', severity: 'high' },
            { id: 'speedhack', label: 'Speedhack (Evidence)', severity: 'high' },
            { id: 'other', label: 'Other Cheats (Evidence)', severity: 'medium' },
            { id: 'insufficient_evidence', label: 'Insufficient Evidence', severity: 'none' },
            { id: 'false_report', label: 'False Report', severity: 'none' }
        ];
    }

    serialize() {
        return {
            enabled: this.enabled,
            cases: Array.from(this.cases.entries()),
            caseQueue: this.caseQueue,
            completedCases: this.completedCases,
            investigators: Array.from(this.investigators.entries()),
            statistics: this.statistics.serialize(),
            priorityWeights: this.priorityWeights,
            rewards: this.rewards,
            caseTimeout: this.caseTimeout,
            consensusThreshold: this.consensusThreshold
        };
    }

    loadState(state) {
        if (state.cases) {
            this.cases = new Map(state.cases);
        }
        this.caseQueue = state.caseQueue || [];
        this.completedCases = state.completedCases || [];
        this.priorityWeights = state.priorityWeights || this.priorityWeights;
        this.rewards = state.rewards || this.rewards;
    }
}

class Investigator {
    constructor(playerId) {
        this.playerId = playerId;
        this.reputation = 0;
        this.accuracy = 0;
        this.totalCases = 0;
        this.correctVerdicts = 0;
        this.streak = 0;
        this.longestStreak = 0;
        this.activeCases = 0;
        this.maxActiveCases = 3;
        this.casesAssigned = [];
        this.casesCompleted = [];
        this.specializations = [];
        this.ranking = 'novice';
    }

    updateAccuracy(finalVerdict) {
        const lastVerdict = this.casesCompleted[this.casesCompleted.length - 1];
        if (lastVerdict === finalVerdict) {
            this.correctVerdicts++;
            this.streak++;
            this.longestStreak = Math.max(this.longestStreak, this.streak);
        } else {
            this.streak = 0;
        }

        this.accuracy = this.correctVerdicts / this.totalCases;
        this.updateRanking();
    }

    updateRanking() {
        if (this.reputation >= 10000 && this.accuracy >= 0.95) {
            this.ranking = 'legendary';
        } else if (this.reputation >= 5000 && this.accuracy >= 0.9) {
            this.ranking = 'expert';
        } else if (this.reputation >= 2000 && this.accuracy >= 0.85) {
            this.ranking = 'skilled';
        } else if (this.reputation >= 500) {
            this.ranking = 'competent';
        } else {
            this.ranking = 'novice';
        }
    }
}

class OverwatchStatistics {
    constructor() {
        this.totalCasesCreated = 0;
        this.totalCasesCompleted = 0;
        this.casesByVerdict = new Map();
        this.casesByEvidenceType = new Map();
        this.averageCompletionTime = 0;
        this.accuracyRate = 0;
        this.appealSuccessRate = 0;
        this.investigatorParticipation = new Map();
    }

    recordCaseCreated(caseData) {
        this.totalCasesCreated++;
        const count = this.casesByEvidenceType.get(caseData.evidenceType) || 0;
        this.casesByEvidenceType.set(caseData.evidenceType, count + 1);
    }

    recordCaseCompleted(caseData) {
        this.totalCasesCompleted++;
        const count = this.casesByVerdict.get(caseData.finalVerdict) || 0;
        this.casesByVerdict.set(caseData.finalVerdict, count + 1);

        if (caseData.verdicts.length > 0) {
            const totalTime = caseData.verdicts.reduce((sum, v) => sum + v.submittedAt, 0) / caseData.verdicts.length;
            const caseDuration = totalTime - caseData.createdAt;
            this.averageCompletionTime = (this.averageCompletionTime * (this.totalCasesCompleted - 1) + caseDuration) / this.totalCasesCompleted;
        }
    }

    serialize() {
        return {
            totalCasesCreated: this.totalCasesCreated,
            totalCasesCompleted: this.totalCasesCompleted,
            casesByVerdict: Array.from(this.casesByVerdict.entries()),
            casesByEvidenceType: Array.from(this.casesByEvidenceType.entries()),
            averageCompletionTime: this.averageCompletionTime,
            accuracyRate: this.accuracyRate,
            appealSuccessRate: this.appealSuccessRate
        };
    }
}

export default {
    ViolationType,
    BanDuration,
    ReportReason,
    AntiCheatEvent,
    AntiCheatManager,
    AntiCheatStatistics,
    Report,
    ReportManager,
    ReportStatistics,
    TrustFactor,
    VACSystem,
    OverwatchSystem
};

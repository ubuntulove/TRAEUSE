export class ReplayEvent {
    constructor(type, data = {}) {
        this.id = ReplayEvent.nextId++;
        this.type = type;
        this.data = data;
        this.timestamp = Date.now();
        this.frameNumber = 0;
        this.tick = 0;
        this.sourceId = null;
        this.targetId = null;
    }

    setFrame(frameNumber) {
        this.frameNumber = frameNumber;
    }

    setTick(tick) {
        this.tick = tick;
    }

    setSource(entityId) {
        this.sourceId = entityId;
    }

    setTarget(entityId) {
        this.targetId = entityId;
    }

    serialize() {
        return {
            id: this.id,
            type: this.type,
            data: this.data,
            timestamp: this.timestamp,
            frameNumber: this.frameNumber,
            tick: this.tick,
            sourceId: this.sourceId,
            targetId: this.targetId
        };
    }
}
ReplayEvent.nextId = 0;

export class ReplayFrame {
    constructor(frameNumber = 0, tick = 0) {
        this.frameNumber = frameNumber;
        this.tick = tick;
        this.entities = new Map();
        this.events = [];
        this.projectiles = new Map();
        this.grenades = new Map();
        this.bomb = null;
        this.hostages = new Map();
        this.roundState = null;
        this.time = Date.now();
    }

    addEntity(entityId, data) {
        this.entities.set(entityId, data);
    }

    removeEntity(entityId) {
        this.entities.delete(entityId);
    }

    addEvent(event) {
        this.events.push(event);
    }

    setBomb(bombState) {
        this.bomb = bombState;
    }

    addProjectile(projectileId, data) {
        this.projectiles.set(projectileId, data);
    }

    removeProjectile(projectileId) {
        this.projectiles.delete(projectileId);
    }

    addGrenade(grenadeId, data) {
        this.grenades.set(grenadeId, data);
    }

    removeGrenade(grenadeId) {
        this.grenades.delete(grenadeId);
    }

    addHostage(hostageId, data) {
        this.hostages.set(hostageId, data);
    }

    removeHostage(hostageId) {
        this.hostages.delete(hostageId);
    }

    setRoundState(state) {
        this.roundState = state;
    }

    getEntity(entityId) {
        return this.entities.get(entityId);
    }

    getProjectile(projectileId) {
        return this.projectiles.get(projectileId);
    }

    serialize() {
        return {
            frameNumber: this.frameNumber,
            tick: this.tick,
            time: this.time,
            entities: Array.from(this.entities.entries()),
            events: this.events.map(e => e.serialize()),
            projectiles: Array.from(this.projectiles.entries()),
            grenades: Array.from(this.grenades.entries()),
            bomb: this.bomb,
            hostages: Array.from(this.hostages.entries()),
            roundState: this.roundState
        };
    }
}

export class ReplayRecording {
    constructor(matchId, config = {}) {
        this.id = ReplayRecording.nextId++;
        this.matchId = matchId;
        this.config = {
            tickRate: config.tickRate || 64,
            frameSkip: config.frameSkip || 3,
            compress: config.compress !== false,
            recordEntities: config.recordEntities !== false,
            recordSounds: config.recordSounds !== false,
            maxDuration: config.maxDuration || 7200,
            ...config
        };
        this.frames = new Map();
        this.events = [];
        this.metadata = {};
        this.startTime = 0;
        this.endTime = 0;
        this.currentFrame = 0;
        this.currentTick = 0;
        this.isRecording = false;
        this.isPaused = false;
        this.fileSize = 0;
        this.header = this.createHeader();
    }

    createHeader() {
        return {
            version: '1.0',
            engine: 'CS2',
            protocol: 135,
            tickRate: this.config.tickRate,
            startFrame: 0,
            playbackFrames: 0,
            snapshotLength: 0,
            lastCompressedFrame: 0
        };
    }

    start() {
        this.isRecording = true;
        this.startTime = Date.now();
        this.currentFrame = 0;
        this.currentTick = 0;
    }

    stop() {
        this.isRecording = false;
        this.endTime = Date.now();
        this.finalize();
    }

    pause() {
        this.isPaused = true;
    }

    resume() {
        this.isPaused = false;
    }

    recordFrame(frameData) {
        if (!this.isRecording || this.isPaused) return;

        const frame = new ReplayFrame(this.currentFrame, this.currentTick);

        if (this.config.recordEntities && frameData.entities) {
            for (const [entityId, entityData] of frameData.entities) {
                frame.addEntity(entityId, entityData);
            }
        }

        if (frameData.events) {
            for (const event of frameData.events) {
                const replayEvent = new ReplayEvent(event.type, event.data);
                replayEvent.setFrame(this.currentFrame);
                replayEvent.setTick(this.currentTick);
                if (event.sourceId) replayEvent.setSource(event.sourceId);
                if (event.targetId) replayEvent.setTarget(event.targetId);
                frame.addEvent(replayEvent);
                this.events.push(replayEvent);
            }
        }

        if (frameData.bomb) {
            frame.setBomb(frameData.bomb);
        }

        if (frameData.projectiles) {
            for (const [projId, projData] of frameData.projectiles) {
                frame.addProjectile(projId, projData);
            }
        }

        if (frameData.grenades) {
            for (const [grenadeId, grenadeData] of frameData.grenades) {
                frame.addGrenade(grenadeId, grenadeData);
            }
        }

        if (frameData.roundState) {
            frame.setRoundState(frameData.roundState);
        }

        if (this.currentFrame % this.config.frameSkip === 0) {
            this.frames.set(this.currentFrame, frame);
        }

        this.currentFrame++;
        this.currentTick = this.currentFrame * this.config.tickRate;

        return frame;
    }

    addEvent(type, data = {}, sourceId = null, targetId = null) {
        const event = new ReplayEvent(type, data);
        event.setFrame(this.currentFrame);
        event.setTick(this.currentTick);
        if (sourceId) event.setSource(sourceId);
        if (targetId) event.setTarget(targetId);
        this.events.push(event);
        return event;
    }

    setMetadata(key, value) {
        this.metadata[key] = value;
    }

    getMetadata(key) {
        return this.metadata[key];
    }

    finalize() {
        this.header.playbackFrames = this.currentFrame;
        this.fileSize = this.calculateFileSize();
    }

    calculateFileSize() {
        let size = 0;

        size += JSON.stringify(this.header).length;

        for (const frame of this.frames.values()) {
            size += JSON.stringify(frame.serialize()).length;
        }

        size += JSON.stringify(this.events).length;
        size += JSON.stringify(this.metadata).length;

        return size;
    }

    getDuration() {
        if (this.endTime === 0) {
            return (Date.now() - this.startTime) / 1000;
        }
        return (this.endTime - this.startTime) / 1000;
    }

    getFrameCount() {
        return this.currentFrame;
    }

    getEventCount() {
        return this.events.length;
    }

    getFrame(frameNumber) {
        if (this.config.frameSkip > 1) {
            const actualFrame = frameNumber * this.config.frameSkip;
            return this.frames.get(actualFrame);
        }
        return this.frames.get(frameNumber);
    }

    getEventsByType(type) {
        return this.events.filter(e => e.type === type);
    }

    getEventsBySource(sourceId) {
        return this.events.filter(e => e.sourceId === sourceId);
    }

    getEventsByTarget(targetId) {
        return this.events.filter(e => e.targetId === targetId);
    }

    getEventsInRange(startFrame, endFrame) {
        return this.events.filter(e => e.frameNumber >= startFrame && e.frameNumber <= endFrame);
    }

    serialize() {
        return {
            id: this.id,
            matchId: this.matchId,
            config: this.config,
            header: this.header,
            metadata: this.metadata,
            frameCount: this.frames.size,
            eventCount: this.events.length,
            duration: this.getDuration(),
            fileSize: this.fileSize,
            startTime: this.startTime,
            endTime: this.endTime
        };
    }

    getFullData() {
        return {
            header: this.header,
            metadata: this.metadata,
            frames: Array.from(this.frames.values()).map(f => f.serialize()),
            events: this.events.map(e => e.serialize())
        };
    }
}
ReplayRecording.nextId = 0;

export class ReplayPlayback {
    constructor(recording) {
        this.recording = recording;
        this.currentFrame = 0;
        this.currentTick = 0;
        this.isPlaying = false;
        this.isPaused = false;
        this.playbackSpeed = 1.0;
        this.direction = 1;
        this.loop = false;
        this.startFrame = 0;
        this.endFrame = 0;
        this.frameCache = new Map();
        this.eventCallbacks = new Map();
        this.frameCallbacks = [];
        this.lastUpdateTime = 0;
    }

    play() {
        this.isPlaying = true;
        this.isPaused = false;
        this.lastUpdateTime = Date.now();
    }

    pause() {
        this.isPlaying = false;
        this.isPaused = true;
    }

    stop() {
        this.isPlaying = false;
        this.isPaused = false;
        this.currentFrame = this.startFrame;
        this.currentTick = this.startFrame * this.recording.config.tickRate;
    }

    seekToFrame(frame) {
        this.currentFrame = Math.max(this.startFrame, Math.min(this.endFrame, frame));
        this.currentTick = this.currentFrame * this.recording.config.tickRate;
    }

    seekToTime(seconds) {
        const frame = Math.floor(seconds * this.recording.config.tickRate);
        this.seekToFrame(frame);
    }

    setPlaybackSpeed(speed) {
        this.playbackSpeed = Math.max(0.1, Math.min(10, speed));
    }

    setDirection(direction) {
        this.direction = direction === 'forward' ? 1 : -1;
    }

    setLoop(loop) {
        this.loop = loop;
    }

    setRange(startFrame, endFrame) {
        this.startFrame = startFrame;
        this.endFrame = endFrame || this.recording.getFrameCount();
    }

    onEvent(type, callback) {
        if (!this.eventCallbacks.has(type)) {
            this.eventCallbacks.set(type, []);
        }
        this.eventCallbacks.get(type).push(callback);
    }

    onFrame(callback) {
        this.frameCallbacks.push(callback);
    }

    update() {
        if (!this.isPlaying || this.isPaused) return;

        const now = Date.now();
        const deltaTime = (now - this.lastUpdateTime) * this.playbackSpeed;
        this.lastUpdateTime = now;

        const framesToAdvance = Math.floor(deltaTime / (1000 / this.recording.config.tickRate));
        this.currentFrame += framesToAdvance * this.direction;

        if (this.currentFrame >= this.endFrame) {
            if (this.loop) {
                this.currentFrame = this.startFrame;
            } else {
                this.currentFrame = this.endFrame;
                this.isPlaying = false;
            }
        }

        if (this.currentFrame < this.startFrame) {
            if (this.loop) {
                this.currentFrame = this.endFrame;
            } else {
                this.currentFrame = this.startFrame;
                this.isPlaying = false;
            }
        }

        this.currentTick = this.currentFrame * this.recording.config.tickRate;

        const frame = this.getCurrentFrame();
        if (frame) {
            for (const callback of this.frameCallbacks) {
                callback(frame, this.currentFrame);
            }

            for (const event of frame.events) {
                const callbacks = this.eventCallbacks.get(event.type);
                if (callbacks) {
                    for (const callback of callbacks) {
                        callback(event, frame);
                    }
                }
            }
        }
    }

    getCurrentFrame() {
        return this.recording.getFrame(this.currentFrame);
    }

    getCurrentTime() {
        return this.currentFrame / this.recording.config.tickRate;
    }

    getDuration() {
        return this.recording.getDuration();
    }

    getProgress() {
        const total = this.endFrame - this.startFrame;
        const current = this.currentFrame - this.startFrame;
        return total > 0 ? current / total : 0;
    }
}

export class ReplayAnalyzer {
    constructor(recording) {
        this.recording = recording;
        this.analysis = null;
        this.cache = new Map();
    }

    analyze() {
        this.analysis = {
            duration: this.recording.getDuration(),
            frameCount: this.recording.getFrameCount(),
            eventCount: this.recording.getEventCount(),
            kills: this.analyzeKills(),
            damage: this.analyzeDamage(),
            economy: this.analyzeEconomy(),
            utility: this.analyzeUtility(),
            movement: this.analyzeMovement(),
            teamStats: this.analyzeTeamStats(),
            roundAnalysis: this.analyzeRounds(),
            highlights: this.findHighlights()
        };

        return this.analysis;
    }

    analyzeKills() {
        const kills = this.recording.getEventsByType('kill');
        const killerStats = new Map();
        const victimStats = new Map();

        for (const kill of kills) {
            const killer = kill.sourceId;
            const victim = kill.targetId;

            if (!killerStats.has(killer)) {
                killerStats.set(killer, { kills: 0, headshots: 0, deaths: 0, kdr: 0 });
            }
            const killerStat = killerStats.get(killer);
            killerStat.kills++;
            if (kill.data.headshot) killerStat.headshots++;

            if (!victimStats.has(victim)) {
                victimStats.set(victim, { deaths: 0, kills: 0 });
            }
            victimStats.get(victim).deaths++;

            killerStat.kdr = killerStat.kills / Math.max(1, killerStat.deaths);
        }

        return {
            total: kills.length,
            headshotPercentage: kills.filter(k => k.data.headshot).length / Math.max(1, kills.length),
            topKiller: this.getTopStat(killerStats, 'kills'),
            mostDied: this.getTopStat(victimStats, 'deaths')
        };
    }

    analyzeDamage() {
        const damageEvents = this.recording.getEventsByType('damage');
        const playerDamage = new Map();

        for (const dmg of damageEvents) {
            const attacker = dmg.sourceId;
            const victim = dmg.targetId;

            if (!playerDamage.has(attacker)) {
                playerDamage.set(attacker, { damage: 0, hits: 0, avgDamage: 0 });
            }
            const stat = playerDamage.get(attacker);
            stat.damage += dmg.data.amount || 0;
            stat.hits++;
            stat.avgDamage = stat.damage / Math.max(1, stat.hits);
        }

        return {
            total: damageEvents.length,
            topDamager: this.getTopStat(playerDamage, 'damage'),
            avgDamagePerRound: damageEvents.length / Math.max(1, this.getRoundCount())
        };
    }

    analyzeEconomy() {
        const buyEvents = this.recording.getEventsByType('buy');
        const teamEconomy = { terrorist: 0, counterTerrorist: 0 };

        for (const buy of buyEvents) {
            const team = buy.data.team;
            if (teamEconomy[team] !== undefined) {
                teamEconomy[team] += buy.data.spent || 0;
            }
        }

        return {
            totalSpend: buyEvents.reduce((sum, b) => sum + (b.data.spent || 0), 0),
            teamSpend: teamEconomy
        };
    }

    analyzeUtility() {
        const grenadeEvents = this.recording.getEventsByType('grenade_thrown');
        const grenadeTypes = new Map();

        for (const grenade of grenadeEvents) {
            const type = grenade.data.type || 'unknown';
            grenadeTypes.set(type, (grenadeTypes.get(type) || 0) + 1);
        }

        return {
            total: grenadeEvents.length,
            byType: Object.fromEntries(grenadeTypes)
        };
    }

    analyzeMovement() {
        const frames = Array.from(this.recording.frames.values());
        const movementStats = new Map();

        for (const frame of frames) {
            for (const [entityId, entity] of frame.entities) {
                if (!movementStats.has(entityId)) {
                    movementStats.set(entityId, { distance: 0, speed: 0, avgSpeed: 0 });
                }
                const stat = movementStats.get(entityId);
                if (entity.velocity) {
                    const speed = Math.sqrt(
                        entity.velocity.x ** 2 +
                        entity.velocity.y ** 2 +
                        entity.velocity.z ** 2
                    );
                    stat.speed = Math.max(stat.speed, speed);
                    stat.avgSpeed = (stat.avgSpeed + speed) / 2;
                }
            }
        }

        return {
            fastestPlayer: this.getTopStat(movementStats, 'speed'),
            avgSpeed: this.getAverageStat(movementStats, 'avgSpeed')
        };
    }

    analyzeTeamStats() {
        const roundWins = this.recording.getEventsByType('round_win');
        const teamWins = { terrorist: 0, counterTerrorist: 0 };

        for (const win of roundWins) {
            const team = win.data.team;
            if (teamWins[team] !== undefined) {
                teamWins[team]++;
            }
        }

        return {
            terroristWins: teamWins.terrorist,
            counterTerroristWins: teamWins.counterTerrorist,
            totalRounds: this.getRoundCount()
        };
    }

    analyzeRounds() {
        const rounds = [];
        const roundStarts = this.recording.getEventsByType('round_start');

        for (const start of roundStarts) {
            const endFrame = start.frameNumber + this.recording.config.tickRate * 115;
            const endEvents = this.recording.getEventsInRange(start.frameNumber, endFrame);
            const roundEnd = endEvents.find(e =>
                e.type === 'round_end' && e.frameNumber >= start.frameNumber
            );

            rounds.push({
                roundNumber: rounds.length + 1,
                startFrame: start.frameNumber,
                endFrame: roundEnd ? roundEnd.frameNumber : endFrame,
                winner: roundEnd ? roundEnd.data.winner : null,
                endCondition: roundEnd ? roundEnd.data.condition : 'timeout'
            });
        }

        return rounds;
    }

    findHighlights() {
        const highlights = [];
        const kills = this.recording.getEventsByType('kill');

        for (const kill of kills) {
            if (kill.data.headshot && kill.data.flashAssist) {
                highlights.push({
                    type: 'ace',
                    frame: kill.frameNumber,
                    killer: kill.sourceId,
                    description: 'Ace with flash assist'
                });
            }

            if (kill.data.noScope || kill.data.throughSmoke) {
                highlights.push({
                    type: 'skillshot',
                    frame: kill.frameNumber,
                    killer: kill.sourceId,
                    description: kill.data.noScope ? 'No scope kill' : 'Through smoke'
                });
            }
        }

        const clutches = this.recording.getEventsByType('clutch');
        for (const clutch of clutches) {
            highlights.push({
                type: 'clutch',
                frame: clutch.frameNumber,
                player: clutch.sourceId,
                description: `Clutch by player`
            });
        }

        return highlights.sort((a, b) => b.frame - a.frame);
    }

    getTopStat(stats, key) {
        let top = null;
        let topValue = -Infinity;

        for (const [id, stat] of stats) {
            if (stat[key] > topValue) {
                topValue = stat[key];
                top = { id, ...stat };
            }
        }

        return top;
    }

    getAverageStat(stats, key) {
        if (stats.size === 0) return 0;
        let total = 0;
        for (const stat of stats.values()) {
            total += stat[key] || 0;
        }
        return total / stats.size;
    }

    getRoundCount() {
        return this.recording.getEventsByType('round_start').length;
    }

    getAnalysis() {
        return this.analysis;
    }
}

export class ReplayManager {
    constructor() {
        this.recordings = new Map();
        this.activeRecording = null;
        this.activePlayback = null;
        this.replayBuffer = new Map();
        this.maxBufferSize = 100;
        this.autoSave = true;
        this.autoSaveInterval = 30000;
        this.lastAutoSave = 0;
    }

    startRecording(matchId, config = {}) {
        if (this.activeRecording) {
            this.stopRecording();
        }

        this.activeRecording = new ReplayRecording(matchId, config);
        this.activeRecording.start();
        return this.activeRecording;
    }

    stopRecording() {
        if (this.activeRecording) {
            this.activeRecording.stop();
            this.recordings.set(this.activeRecording.id, this.activeRecording);
            this.activeRecording = null;
            return true;
        }
        return false;
    }

    pauseRecording() {
        if (this.activeRecording) {
            this.activeRecording.pause();
        }
    }

    resumeRecording() {
        if (this.activeRecording) {
            this.activeRecording.resume();
        }
    }

    getActiveRecording() {
        return this.activeRecording;
    }

    recordFrame(frameData) {
        if (this.activeRecording) {
            return this.activeRecording.recordFrame(frameData);
        }
        return null;
    }

    recordEvent(type, data = {}, sourceId = null, targetId = null) {
        if (this.activeRecording) {
            return this.activeRecording.addEvent(type, data, sourceId, targetId);
        }
        return null;
    }

    loadRecording(recordingId) {
        return this.recordings.get(recordingId);
    }

    loadRecordingFromData(data) {
        const recording = new ReplayRecording(data.matchId, data.config);
        Object.assign(recording.header, data.header);
        Object.assign(recording.metadata, data.metadata);
        return recording;
    }

    startPlayback(recordingId) {
        const recording = this.recordings.get(recordingId);
        if (!recording) return null;

        this.activePlayback = new ReplayPlayback(recording);
        return this.activePlayback;
    }

    stopPlayback() {
        if (this.activePlayback) {
            this.activePlayback.stop();
            this.activePlayback = null;
        }
    }

    getActivePlayback() {
        return this.activePlayback;
    }

    getRecording(id) {
        return this.recordings.get(id);
    }

    getAllRecordings() {
        return Array.from(this.recordings.values());
    }

    deleteRecording(id) {
        return this.recordings.delete(id);
    }

    analyzeRecording(recordingId) {
        const recording = this.recordings.get(recordingId);
        if (!recording) return null;

        const analyzer = new ReplayAnalyzer(recording);
        return analyzer.analyze();
    }

    exportRecording(recordingId, format = 'json') {
        const recording = this.recordings.get(recordingId);
        if (!recording) return null;

        switch (format) {
            case 'json':
                return JSON.stringify(recording.getFullData(), null, 2);
            case 'binary':
                return this.exportAsBinary(recording);
            default:
                return JSON.stringify(recording.getFullData());
        }
    }

    exportAsBinary(recording) {
        return new Uint8Array([]);
    }

    importRecording(data, format = 'json') {
        try {
            if (format === 'json') {
                const parsed = JSON.parse(data);
                const recording = this.loadRecordingFromData(parsed);
                this.recordings.set(recording.id, recording);
                return recording;
            }
        } catch (e) {
            console.error('Failed to import recording:', e);
        }
        return null;
    }

    update() {
        if (this.activePlayback) {
            this.activePlayback.update();
        }

        if (this.autoSave && this.activeRecording) {
            const now = Date.now();
            if (now - this.lastAutoSave >= this.autoSaveInterval) {
                this.autoSaveRecording();
                this.lastAutoSave = now;
            }
        }
    }

    autoSaveRecording() {
        if (this.activeRecording) {
            this.recordings.set(this.activeRecording.id, this.activeRecording);
        }
    }

    getStorageInfo() {
        let totalSize = 0;
        const recordings = [];

        for (const recording of this.recordings.values()) {
            totalSize += recording.fileSize;
            recordings.push({
                id: recording.id,
                matchId: recording.matchId,
                duration: recording.getDuration(),
                fileSize: recording.fileSize
            });
        }

        return {
            totalSize,
            recordingCount: this.recordings.size,
            recordings
        };
    }

    clearOldRecordings(maxAge = 604800000) {
        const cutoff = Date.now() - maxAge;
        let deleted = 0;

        for (const [id, recording] of this.recordings) {
            if (recording.endTime > 0 && recording.endTime < cutoff) {
                this.recordings.delete(id);
                deleted++;
            }
        }

        return deleted;
    }
}

export default ReplayManager;

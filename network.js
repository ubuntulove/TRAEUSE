export class NetworkManager {
    constructor() {
        this.connections = new Map();
        this.servers = new Map();
        this.matches = new Map();
        this.lobbies = new Map();
        this.packets = new Map();
        this.lagCompensation = new LagCompensation();
        this.interpolation = new Interpolation();
        this.prediction = new Prediction();
        this.reconciliation = new Reconciliation();
        this.pingManager = new PingManager();
        this.bandwidthManager = new BandwidthManager();
        this.connectionQuality = new ConnectionQuality();
        this.networkSimulator = new NetworkSimulator();
        this.reliability = new ReliabilitySystem();
    }

    initialize() {
        this.startPingUpdates();
        this.startBandwidthMonitoring();
    }

    connect(serverAddress, options = {}) {
        const connection = new NetworkConnection({
            id: 'conn_' + Date.now(),
            serverAddress,
            protocol: options.protocol || 'udp',
            timeout: options.timeout || 30000,
            retryAttempts: options.retryAttempts || 3,
            compression: options.compression || true,
            encryption: options.encryption || false,
            createdAt: Date.now()
        });

        connection.onConnect(() => {
            this.connections.set(connection.id, connection);
            this.connectionQuality.initialize(connection);
        });

        return connection;
    }

    disconnect(connectionId) {
        const connection = this.connections.get(connectionId);
        if (connection) {
            connection.close();
            this.connections.delete(connectionId);
        }
    }

    sendPacket(connectionId, packetData, options = {}) {
        const connection = this.connections.get(connectionId);
        if (!connection) return null;

        const packet = {
            id: 'packet_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            connectionId,
            type: packetData.type || 'data',
            payload: packetData.payload,
            priority: options.priority || 'medium',
            channel: options.channel || 0,
            reliable: options.reliable ?? true,
            sequenced: options.sequenced ?? true,
            compressed: options.compressed ?? connection.compression,
            createdAt: Date.now(),
            sentAt: null,
            acknowledged: false
        };

        if (packet.reliable) {
            this.reliability.trackPacket(connectionId, packet.id);
        }

        this.packets.set(packet.id, packet);

        if (this.networkSimulator.enabled) {
            this.networkSimulator.simulatePacket(packet);
        } else {
            this.transmitPacket(packet);
        }

        return packet;
    }

    transmitPacket(packet) {
        packet.sentAt = Date.now();
        
        setTimeout(() => {
            this.receiveAck(packet.id);
        }, packet.acknowledged ? 0 : 100);
    }

    receiveAck(packetId) {
        const packet = this.packets.get(packetId);
        if (packet) {
            packet.acknowledged = true;
            this.reliability.onAck(packet.connectionId, packetId);
        }
    }

    createServer(serverData) {
        const server = {
            id: 'server_' + Date.now(),
            name: serverData.name,
            address: serverData.address,
            port: serverData.port,
            maxPlayers: serverData.maxPlayers || 64,
            tickRate: serverData.tickRate || 64,
            map: serverData.map,
            gameMode: serverData.gameMode,
            password: serverData.password,
            region: serverData.region,
            vacSecure: serverData.vacSecure ?? true,
            status: 'online',
            players: [],
            currentTick: 0,
            uptime: 0,
            startedAt: Date.now(),
            bandwidth: {
                in: 0,
                out: 0
            },
            performance: {
                cpu: 0,
                memory: 0,
                networkIn: 0,
                networkOut: 0
            }
        };

        this.servers.set(server.id, server);
        return server;
    }

    createMatch(matchData) {
        const match = {
            id: 'match_' + Date.now(),
            serverId: matchData.serverId,
            teams: {
                team1: [],
                team2: []
            },
            status: 'created',
            currentTick: 0,
            currentRound: 0,
            maxRounds: matchData.maxRounds || 30,
            score: {
                team1: 0,
                team2: 0
            },
            map: matchData.map,
            gameMode: matchData.gameMode,
            createdAt: Date.now(),
            startedAt: null,
            endedAt: null,
            pauseState: {
                paused: false,
                team: null,
                duration: 0,
                startedAt: null
            }
        };

        this.matches.set(match.id, match);
        return match;
    }

    createLobby(lobbyData) {
        const lobby = {
            id: 'lobby_' + Date.now(),
            ownerId: lobbyData.ownerId,
            name: lobbyData.name,
            maxPlayers: lobbyData.maxPlayers || 5,
            gameMode: lobbyData.gameMode,
            map: lobbyData.map,
            password: lobbyData.password,
            private: lobbyData.private ?? false,
            region: lobbyData.region,
            status: 'open',
            players: [],
            invites: [],
            settings: {
                allowCheats: false,
                friendlyFire: false,
                kickPlayersOnDeath: false,
                respawnOnDeath: false,
                spectateTeams: 'none',
                autoBalance: true
            },
            chat: [],
            createdAt: Date.now()
        };

        this.lobbies.set(lobby.id, lobby);
        return lobby;
    }

    joinLobby(lobbyId, playerId, password = null) {
        const lobby = this.lobbies.get(lobbyId);
        if (!lobby) {
            return { success: false, reason: 'Lobby not found' };
        }

        if (lobby.status !== 'open') {
            return { success: false, reason: 'Lobby is not accepting players' };
        }

        if (lobby.players.length >= lobby.maxPlayers) {
            return { success: false, reason: 'Lobby is full' };
        }

        if (lobby.private && password !== lobby.password) {
            return { success: false, reason: 'Invalid password' };
        }

        lobby.players.push({
            id: playerId,
            role: 'member',
            ready: false,
            isHost: false,
            joinedAt: Date.now()
        });

        return { success: true, lobby };
    }

    leaveLobby(lobbyId, playerId) {
        const lobby = this.lobbies.get(lobbyId);
        if (!lobby) return { success: false };

        const playerIndex = lobby.players.findIndex(p => p.id === playerId);
        if (playerIndex === -1) {
            return { success: false, reason: 'Not in lobby' };
        }

        const player = lobby.players[playerIndex];
        lobby.players.splice(playerIndex, 1);

        if (player.role === 'owner' && lobby.players.length > 0) {
            lobby.players[0].role = 'owner';
            lobby.ownerId = lobby.players[0].id;
        }

        return { success: true };
    }

    startPingUpdates() {
        setInterval(() => {
            for (const [id, connection] of this.connections) {
                this.pingManager.updatePing(connection);
                this.connectionQuality.evaluate(connection);
            }
        }, 1000);
    }

    startBandwidthMonitoring() {
        setInterval(() => {
            for (const [id, connection] of this.connections) {
                this.bandwidthManager.update(connection);
            }
        }, 5000);
    }

    applyLagCompensation(playerId, serverTick, entityState) {
        return this.lagCompensation.compensate(playerId, serverTick, entityState);
    }

    interpolateEntity(entityId, targetTick, settings) {
        return this.interpolation.interpolate(entityId, targetTick, settings);
    }

    predictPlayerMovement(playerId, input, serverTick) {
        return this.prediction.predict(playerId, input, serverTick);
    }

    reconcile(clientTick, serverTick, acknowledgedState) {
        return this.reconciliation.reconcile(clientTick, serverTick, acknowledgedState);
    }

    getConnectionQuality(connectionId) {
        return this.connectionQuality.getQuality();
    }

    getNetworkStats(connectionId) {
        const connection = this.connections.get(connectionId);
        if (!connection) return null;

        return {
            ping: this.pingManager.getPing(connectionId),
            jitter: this.pingManager.getJitter(connectionId),
            packetLoss: this.reliability.getPacketLoss(connectionId),
            bandwidth: this.bandwidthManager.getBandwidth(connectionId),
            queueSize: this.reliability.getReliableQueueSize(connectionId)
        };
    }

    serialize() {
        return {
            connections: Array.from(this.connections.values()).map(c => c.serialize()),
            servers: Array.from(this.servers.values()),
            matches: Array.from(this.matches.values()),
            lobbies: Array.from(this.lobbies.values())
        };
    }
}

export class NetworkConnection {
    constructor(data) {
        this.id = data.id;
        this.serverAddress = data.serverAddress;
        this.protocol = data.protocol;
        this.status = 'connecting';
        this.createdAt = Date.now();
        this.connectedAt = null;
        this.lastActivity = Date.now();
        this.timeout = data.timeout;
        this.retryAttempts = data.retryAttempts;
        this.compression = data.compression;
        this.encryption = data.encryption;
        this.state = new ConnectionState();
        this.statistics = {
            packetsSent: 0,
            packetsReceived: 0,
            bytesSent: 0,
            bytesReceived: 0,
            lastPing: 0,
            averagePing: 0,
            packetLoss: 0
        };
        this.callbacks = {
            onConnect: [],
            onDisconnect: [],
            onError: [],
            onPacket: []
        };
    }

    onConnect(callback) {
        this.callbacks.onConnect.push(callback);
    }

    onDisconnect(callback) {
        this.callbacks.onDisconnect.push(callback);
    }

    onError(callback) {
        this.callbacks.onError.push(callback);
    }

    onPacket(callback) {
        this.callbacks.onPacket.push(callback);
    }

    connect() {
        setTimeout(() => {
            this.status = 'connected';
            this.connectedAt = Date.now();
            
            for (const callback of this.callbacks.onConnect) {
                callback(this);
            }
        }, 100);
    }

    disconnect(reason = 'normal') {
        this.status = 'disconnected';
        
        for (const callback of this.callbacks.onDisconnect) {
            callback(this, reason);
        }
    }

    close() {
        this.disconnect('closed');
    }

    send(data, options = {}) {
        if (this.status !== 'connected') return false;
        
        this.statistics.packetsSent++;
        this.statistics.bytesSent += JSON.stringify(data).length;
        this.lastActivity = Date.now();
        
        return true;
    }

    receive(data) {
        this.statistics.packetsReceived++;
        this.statistics.bytesReceived += JSON.stringify(data).length;
        this.lastActivity = Date.now();
        
        for (const callback of this.callbacks.onPacket) {
            callback(this, data);
        }
    }

    serialize() {
        return {
            id: this.id,
            serverAddress: this.serverAddress,
            protocol: this.protocol,
            status: this.status,
            createdAt: this.createdAt,
            connectedAt: this.connectedAt,
            statistics: this.statistics
        };
    }
}

export class ConnectionState {
    constructor() {
        this.authenticated = false;
        this.ready = false;
        this.loading = false;
        this.inGame = false;
        this.spectating = false;
    }
}

export class LagCompensation {
    constructor() {
        this.enabled = true;
        this.maxTime = 1000;
        this.bufferSize = 128;
        this.playerStates = new Map();
    }

    recordState(playerId, tick, state) {
        if (!this.playerStates.has(playerId)) {
            this.playerStates.set(playerId, []);
        }

        const states = this.playerStates.get(playerId);
        states.push({ tick, state, timestamp: Date.now() });

        if (states.length > this.bufferSize) {
            states.shift();
        }
    }

    compensate(playerId, serverTick, entityState) {
        if (!this.enabled) return entityState;

        const targetTick = serverTick - Math.floor(this.maxTime / 16.67);
        const states = this.playerStates.get(playerId);

        if (!states || states.length === 0) {
            return entityState;
        }

        let bestState = states[0];
        for (const state of states) {
            if (state.tick <= targetTick) {
                if (!bestState || state.tick > bestState.tick) {
                    bestState = state;
                }
            }
        }

        return bestState ? bestState.state : entityState;
    }
}

export class Interpolation {
    constructor() {
        this.enabled = true;
        this.interval = 100;
        this.smoothing = 0.1;
        this.entityStates = new Map();
    }

    interpolate(entityId, targetTick, settings) {
        if (!this.enabled) {
            return this.entityStates.get(entityId);
        }

        const currentState = this.entityStates.get(entityId);
        const targetState = settings.targetState;

        if (!currentState) {
            return targetState;
        }

        return this.smoothState(currentState, targetState, this.smoothing);
    }

    smoothState(from, to, factor) {
        return {
            position: {
                x: from.position.x + (to.position.x - from.position.x) * factor,
                y: from.position.y + (to.position.y - from.position.y) * factor,
                z: from.position.z + (to.position.z - from.position.z) * factor
            },
            rotation: {
                pitch: from.rotation.pitch + (to.rotation.pitch - from.rotation.pitch) * factor,
                yaw: from.rotation.yaw + (to.rotation.yaw - from.rotation.yaw) * factor
            },
            velocity: {
                x: from.velocity.x + (to.velocity.x - from.velocity.x) * factor,
                y: from.velocity.y + (to.velocity.y - from.velocity.y) * factor,
                z: from.velocity.z + (to.velocity.z - from.velocity.z) * factor
            }
        };
    }
}

export class Prediction {
    constructor() {
        this.enabled = true;
        this.maxIterations = 5;
        this.accumulator = 0;
        this.playerInputs = new Map();
    }

    predict(playerId, input, serverTick) {
        if (!this.enabled) return null;

        const lastInput = this.playerInputs.get(playerId);
        const newState = this.simulateStep(playerId, input, lastInput);

        this.playerInputs.set(playerId, input);

        return newState;
    }

    simulateStep(playerId, input, lastInput) {
        return {
            position: { x: 0, y: 0, z: 0 },
            velocity: { x: 0, y: 0, z: 0 },
            input
        };
    }
}

export class Reconciliation {
    constructor() {
        this.enabled = true;
        this.reconciliationQueue = new Map();
    }

    reconcile(clientTick, serverTick, acknowledgedState) {
        if (!this.enabled) return null;

        const key = `${clientTick}_${serverTick}`;
        
        if (this.reconciliationQueue.has(key)) {
            const result = this.reconciliationQueue.get(key);
            this.reconciliationQueue.delete(key);
            return result;
        }

        return {
            clientState: null,
            serverState: acknowledgedState,
            corrections: []
        };
    }

    recordState(clientTick, serverTick, state) {
        const key = `${clientTick}_${serverTick}`;
        this.reconciliationQueue.set(key, state);
    }
}

export class PingManager {
    constructor() {
        this.pings = new Map();
        this.pingHistory = new Map();
        this.jitterHistory = new Map();
        this.lastPingTime = new Map();
    }

    updatePing(connection) {
        const now = Date.now();
        const lastTime = this.lastPingTime.get(connection.id) || now;
        const elapsed = now - lastTime;

        const estimatedPing = Math.random() * 50 + 10;

        this.pings.set(connection.id, estimatedPing);

        if (!this.pingHistory.has(connection.id)) {
            this.pingHistory.set(connection.id, []);
        }

        const history = this.pingHistory.get(connection.id);
        history.push(estimatedPing);

        if (history.length > 60) {
            history.shift();
        }

        this.lastPingTime.set(connection.id, now);

        this.updateJitter(connection.id);
    }

    updateJitter(connectionId) {
        const history = this.pingHistory.get(connectionId);
        if (!history || history.length < 2) return 0;

        let totalJitter = 0;
        for (let i = 1; i < history.length; i++) {
            totalJitter += Math.abs(history[i] - history[i - 1]);
        }

        const averageJitter = totalJitter / (history.length - 1);
        this.jitterHistory.set(connectionId, averageJitter);

        return averageJitter;
    }

    getPing(connectionId) {
        return this.pings.get(connectionId) || 0;
    }

    getJitter(connectionId) {
        return this.jitterHistory.get(connectionId) || 0;
    }

    getAveragePing(connectionId) {
        const history = this.pingHistory.get(connectionId);
        if (!history || history.length === 0) return 0;

        const sum = history.reduce((a, b) => a + b, 0);
        return sum / history.length;
    }
}

export class BandwidthManager {
    constructor() {
        this.bandwidth = new Map();
        this.dataHistory = new Map();
    }

    update(connection) {
        if (!this.dataHistory.has(connection.id)) {
            this.dataHistory.set(connection.id, {
                sent: [],
                received: []
            });
        }

        const history = this.dataHistory.get(connection.id);
        const now = Date.now();

        history.sent.push({
            timestamp: now,
            bytes: connection.statistics.bytesSent
        });

        history.received.push({
            timestamp: now,
            bytes: connection.statistics.bytesReceived
        });

        if (history.sent.length > 60) history.sent.shift();
        if (history.received.length > 60) history.received.shift();

        const bandwidth = this.calculateBandwidth(history, 60000);
        this.bandwidth.set(connection.id, bandwidth);
    }

    calculateBandwidth(history, windowMs) {
        const now = Date.now();
        const recent = history.filter(h => now - h.timestamp <= windowMs);

        if (recent.length < 2) return { in: 0, out: 0 };

        const inBytes = recent[recent.length - 1].bytes - recent[0].bytes;
        const outBytes = recent[recent.length - 1].bytes - recent[0].bytes;

        return {
            in: inBytes / (windowMs / 1000),
            out: outBytes / (windowMs / 1000)
        };
    }

    getBandwidth(connectionId) {
        return this.bandwidth.get(connectionId) || { in: 0, out: 0 };
    }
}

export class ConnectionQuality {
    constructor() {
        this.quality = 'good';
        this.metrics = {
            latency: 0,
            jitter: 0,
            packetLoss: 0,
            choke: 0
        };
    }

    initialize(connection) {
        this.metrics = {
            latency: 0,
            jitter: 0,
            packetLoss: 0,
            choke: 0
        };
    }

    evaluate(connection) {
        const ping = connection.statistics.averagePing;
        const packetLoss = connection.statistics.packetLoss;

        if (ping < 50 && packetLoss < 1) {
            this.quality = 'good';
        } else if (ping < 100 && packetLoss < 5) {
            this.quality = 'fair';
        } else if (ping < 150 && packetLoss < 10) {
            this.quality = 'poor';
        } else {
            this.quality = 'bad';
        }

        return this.quality;
    }

    getQuality() {
        return {
            quality: this.quality,
            metrics: this.metrics
        };
    }
}

export class NetworkSimulator {
    constructor() {
        this.enabled = false;
        this.latency = 0;
        this.jitter = 0;
        this.packetLoss = 0;
        this.duplicates = 0;
        this.packetBuffer = [];
    }

    enable(settings = {}) {
        this.enabled = true;
        this.latency = settings.latency || 0;
        this.jitter = settings.jitter || 0;
        this.packetLoss = settings.packetLoss || 0;
        this.duplicates = settings.duplicates || 0;
    }

    disable() {
        this.enabled = false;
    }

    simulatePacket(packet) {
        if (Math.random() < this.packetLoss / 100) {
            return;
        }

        const latency = this.latency + (Math.random() * this.jitter * 2 - this.jitter);

        setTimeout(() => {
            this.transmitPacket(packet);
        }, latency);

        if (this.duplicates > 0 && Math.random() < this.duplicates / 100) {
            setTimeout(() => {
                this.transmitPacket(packet);
            }, latency);
        }
    }

    transmitPacket(packet) {
        packet.sentAt = Date.now();
    }
}

export class ReliabilitySystem {
    constructor() {
        this.sentPackets = new Map();
        this.receivedAcks = new Map();
        this.reliableQueue = new Map();
    }

    trackPacket(connectionId, packetId) {
        if (!this.sentPackets.has(connectionId)) {
            this.sentPackets.set(connectionId, new Map());
        }

        const sent = this.sentPackets.get(connectionId);
        sent.set(packetId, {
            sentAt: Date.now(),
            acknowledged: false,
            retries: 0
        });
    }

    onAck(connectionId, packetId) {
        const sent = this.sentPackets.get(connectionId);
        if (sent && sent.has(packetId)) {
            const packet = sent.get(packetId);
            packet.acknowledged = true;

            if (!this.receivedAcks.has(connectionId)) {
                this.receivedAcks.set(connectionId, []);
            }

            this.receivedAcks.get(connectionId).push({
                packetId,
                timestamp: Date.now()
            });
        }
    }

    getPacketLoss(connectionId) {
        const sent = this.sentPackets.get(connectionId);
        if (!sent || sent.size === 0) return 0;

        let lost = 0;
        const now = Date.now();

        for (const [id, packet] of sent) {
            if (!packet.acknowledged && now - packet.sentAt > 5000) {
                lost++;
            }
        }

        return (lost / sent.size) * 100;
    }

    getReliableQueueSize(connectionId) {
        const queue = this.reliableQueue.get(connectionId);
        return queue ? queue.length : 0;
    }
}

export default {
    NetworkManager,
    NetworkConnection,
    ConnectionState,
    LagCompensation,
    Interpolation,
    Prediction,
    Reconciliation,
    PingManager,
    BandwidthManager,
    ConnectionQuality,
    NetworkSimulator,
    ReliabilitySystem
};

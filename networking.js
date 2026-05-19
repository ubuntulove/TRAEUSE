import { NETWORK_CONFIG, GAME_CONFIG } from './config.js';

export class NetworkManager {
    constructor() {
        this.connected = false;
        this.connecting = false;
        this.connectionId = null;
        this.serverAddress = null;
        this.serverPort = 0;
        
        this.socket = null;
        this.webSocket = null;
        
        this.packets = {
            sent: 0,
            received: 0,
            lost: 0,
            corrupted: 0,
            queued: []
        };
        
        this.latency = {
            current: 0,
            average: 0,
            min: 0,
            max: 0,
            history: []
        };
        
        this.bandwidth = {
            in: 0,
            out: 0,
            inPeak: 0,
            outPeak: 0
        };
        
        this.tickRate = NETWORK_CONFIG.TICK_RATE;
        this.tickInterval = 1000 / this.tickRate;
        this.lastTick = 0;
        this.tickTimeAccumulator = 0;
        
        this.clientTick = 0;
        this.serverTick = 0;
        this.currentSnapshot = null;
        this.snapshots = new Map();
        this.snapshotHistory = new Map();
        
        this.entityState = new Map();
        this.projectileState = new Map();
        this.grenadeState = new Map();
        
        this.inputSequence = 0;
        this.lastProcessedInput = 0;
        
        this.commands = [];
        this.commandAck = 0;
        
        this.messageHandlers = new Map();
        this.registerMessageHandlers();
        
        this.reliableCommands = [];
        this.unreliableCommands = [];
        
        this.compressionEnabled = true;
        this.encryptionEnabled = false;
        
        this.packetQueue = [];
        this.outgoingData = [];
        
        this.lastUpdate = Date.now();
        this.updateInterval = 1000 / 60;
    }
    
    registerMessageHandlers() {
        this.messageHandlers.set(NET_MESSAGE.SV_BASELINE, this.handleBaseline.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_SNAPSHOT, this.handleSnapshot.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_ENTITY_UPDATE, this.handleEntityUpdate.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_PLAYER_UPDATE, this.handlePlayerUpdate.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_PROJECTILE, this.handleProjectile.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_EXPLOSION, this.handleExplosion.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_ROUND_START, this.handleRoundStart.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_ROUND_END, this.handleRoundEnd.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_PLAYER_DEATH, this.handlePlayerDeath.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_PLAYER_HURT, this.handlePlayerHurt.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_BOMB_PLANT, this.handleBombPlant.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_BOMB_DEFUSE, this.handleBombDefuse.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_BOMB_EXPLODE, this.handleBombExplode.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_HOSTAGE_RESCUE, this.handleHostageRescue.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_VOTE_START, this.handleVoteStart.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_VOTE_END, this.handleVoteEnd.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_TEAM_SCORE, this.handleTeamScore.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_GAME_PHASE, this.handleGamePhase.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_SERVER_INFO, this.handleServerInfo.bind(this));
        this.messageHandlers.set(NET_MESSAGE.SV_CHAT, this.handleChat.bind(this));
    }
    
    connect(address, port = 27015) {
        if (this.connected || this.connecting) return false;
        
        this.serverAddress = address;
        this.serverPort = port;
        this.connecting = true;
        
        const url = `ws://${address}:${port}`;
        
        try {
            this.webSocket = new WebSocket(url);
            
            this.webSocket.onopen = () => {
                this.onConnect();
            };
            
            this.webSocket.onmessage = (event) => {
                this.onMessage(event.data);
            };
            
            this.webSocket.onerror = (error) => {
                this.onError(error);
            };
            
            this.webSocket.onclose = (event) => {
                this.onDisconnect(event);
            };
            
            return true;
        } catch (error) {
            console.error('Failed to create WebSocket:', error);
            this.connecting = false;
            return false;
        }
    }
    
    disconnect() {
        if (this.webSocket) {
            this.webSocket.close();
            this.webSocket = null;
        }
        
        this.connected = false;
        this.connecting = false;
        this.connectionId = null;
        
        this.reset();
    }
    
    onConnect() {
        console.log('Connected to server');
        this.connected = true;
        this.connecting = false;
        this.connectionId = this.generateConnectionId();
        
        this.sendConnectPacket();
    }
    
    onMessage(data) {
        const packet = this.deserializePacket(data);
        
        if (!packet) {
            this.packets.corrupted++;
            return;
        }
        
        this.packets.received++;
        
        this.processPacket(packet);
        
        this.updateLatency(packet.timestamp);
    }
    
    onError(error) {
        console.error('Network error:', error);
        this.connecting = false;
    }
    
    onDisconnect(event) {
        console.log('Disconnected from server', event.code, event.reason);
        this.connected = false;
        this.connecting = false;
        this.connectionId = null;
        
        this.reset();
    }
    
    sendConnectPacket() {
        const packet = {
            type: NET_MESSAGE.CL_CONNECT,
            protocol: NETWORK_CONFIG.PROTOCOL_VERSION,
            version: GAME_CONFIG.VERSION,
            playerName: this.getPlayerName(),
            playerId: this.getPlayerId()
        };
        
        this.send(packet);
    }
    
    send(packet) {
        if (!this.connected && !this.connecting) return;
        
        const data = this.serializePacket(packet);
        
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.send(data);
            this.packets.sent++;
        }
    }
    
    sendReliable(packet) {
        const reliablePacket = {
            ...packet,
            sequence: this.inputSequence++,
            reliable: true
        };
        
        this.reliableCommands.push(reliablePacket);
        this.send(reliablePacket);
    }
    
    sendUnreliable(packet) {
        const unreliablePacket = {
            ...packet,
            sequence: this.inputSequence++,
            reliable: false
        };
        
        this.unreliableCommands.push(unreliablePacket);
        this.send(unreliablePacket);
    }
    
    serializePacket(packet) {
        try {
            const json = JSON.stringify(packet);
            
            if (this.compressionEnabled) {
                return this.compress(json);
            }
            
            return json;
        } catch (error) {
            console.error('Failed to serialize packet:', error);
            return null;
        }
    }
    
    deserializePacket(data) {
        try {
            let json = data;
            
            if (this.compressionEnabled) {
                json = this.decompress(data);
            }
            
            return JSON.parse(json);
        } catch (error) {
            console.error('Failed to deserialize packet:', error);
            return null;
        }
    }
    
    compress(data) {
        return data;
    }
    
    decompress(data) {
        return data;
    }
    
    processPacket(packet) {
        const handler = this.messageHandlers.get(packet.type);
        if (handler) {
            handler(packet);
        }
    }
    
    update(deltaTime) {
        const now = Date.now();
        this.updateInterval = now - this.lastUpdate;
        this.lastUpdate = now;
        
        this.processInput();
        
        this.processPacketQueue();
        
        this.sendPendingData();
        
        this.cleanupOldData();
    }
    
    processInput() {
        if (!this.connected) return;
        
        const inputState = this.collectInputState();
        
        const command = {
            type: NET_MESSAGE.CL_INPUT,
            tick: this.clientTick,
            sequence: this.inputSequence,
            input: inputState
        };
        
        this.sendUnreliable(command);
    }
    
    collectInputState() {
        return {
            forward: 0,
            backward: 0,
            left: 0,
            right: 0,
            jump: false,
            duck: false,
            attack: false,
            attack2: false,
            use: false,
            reload: false,
            lookPitch: 0,
            lookYaw: 0,
            mouseX: 0,
            mouseY: 0,
            weaponSlot: 0
        };
    }
    
    processPacketQueue() {
        while (this.packetQueue.length > 0) {
            const packet = this.packetQueue.shift();
            this.processPacket(packet);
        }
    }
    
    sendPendingData() {
        for (const command of this.reliableCommands) {
            if (!command.acked) {
                this.send(command);
            }
        }
        
        this.reliableCommands = this.reliableCommands.filter(c => !c.acked);
    }
    
    cleanupOldData() {
        const cutoffTime = Date.now() - 10000;
        
        for (const [tick, snapshot] of this.snapshotHistory.entries()) {
            if (snapshot.time < cutoffTime) {
                this.snapshotHistory.delete(tick);
            }
        }
        
        const maxSnapshots = 128;
        if (this.snapshotHistory.size > maxSnapshots) {
            const sortedTicks = Array.from(this.snapshotHistory.keys()).sort();
            const toRemove = sortedTicks.slice(0, this.snapshotHistory.size - maxSnapshots);
            for (const tick of toRemove) {
                this.snapshotHistory.delete(tick);
            }
        }
    }
    
    updateLatency(timestamp) {
        const now = Date.now();
        const latency = now - timestamp;
        
        this.latency.current = latency;
        this.latency.history.push(latency);
        
        if (this.latency.history.length > 60) {
            this.latency.history.shift();
        }
        
        this.latency.average = this.latency.history.reduce((a, b) => a + b, 0) / this.latency.history.length;
        this.latency.min = Math.min(...this.latency.history);
        this.latency.max = Math.max(...this.latency.history);
    }
    
    interpolateState(targetTick, interpolationDelay) {
        const currentTick = targetTick - interpolationDelay;
        
        let previousSnapshot = null;
        let nextSnapshot = null;
        
        for (const [tick, snapshot] of this.snapshotHistory) {
            if (tick <= currentTick) {
                previousSnapshot = snapshot;
            } else if (!nextSnapshot && tick > currentTick) {
                nextSnapshot = snapshot;
                break;
            }
        }
        
        if (!previousSnapshot || !nextSnapshot) {
            return previousSnapshot || nextSnapshot || null;
        }
        
        const t = (currentTick - previousSnapshot.tick) / (nextSnapshot.tick - previousSnapshot.tick);
        
        const interpolated = this.interpolateSnapshots(previousSnapshot, nextSnapshot, t);
        
        return interpolated;
    }
    
    interpolateSnapshots(prev, next, t) {
        const result = {
            tick: prev.tick,
            players: [],
            entities: []
        };
        
        for (let i = 0; i < prev.players.length; i++) {
            const prevPlayer = prev.players[i];
            const nextPlayer = next.players.find(p => p.id === prevPlayer.id) || prevPlayer;
            
            result.players.push({
                id: prevPlayer.id,
                position: this.lerpVector(prevPlayer.position, nextPlayer.position, t),
                rotation: this.lerpVector(prevPlayer.rotation, nextPlayer.rotation, t),
                velocity: this.lerpVector(prevPlayer.velocity, nextPlayer.velocity, t),
                health: Math.round(this.lerp(prevPlayer.health, nextPlayer.health, t)),
                armor: Math.round(this.lerp(prevPlayer.armor, nextPlayer.armor, t)),
                alive: t < 0.5 ? prevPlayer.alive : nextPlayer.alive
            });
        }
        
        return result;
    }
    
    lerp(a, b, t) {
        return a + (b - a) * t;
    }
    
    lerpVector(a, b, t) {
        return {
            x: this.lerp(a.x, b.x, t),
            y: this.lerp(a.y, b.y, t),
            z: this.lerp(a.z, b.z, t)
        };
    }
    
    handleBaseline(packet) {
        this.currentSnapshot = packet.snapshot;
        this.serverTick = packet.tick;
    }
    
    handleSnapshot(packet) {
        this.snapshotHistory.set(packet.tick, {
            tick: packet.tick,
            time: Date.now(),
            players: packet.players,
            entities: packet.entities
        });
        
        this.snapshots.set(packet.tick, packet);
    }
    
    handleEntityUpdate(packet) {
        this.entityState.set(packet.entityId, {
            ...packet.state,
            lastUpdate: Date.now()
        });
    }
    
    handlePlayerUpdate(packet) {
        this.entityState.set(packet.playerId, {
            ...packet.state,
            lastUpdate: Date.now()
        });
    }
    
    handleProjectile(packet) {
        this.projectileState.set(packet.projectileId, {
            ...packet.state,
            lastUpdate: Date.now()
        });
    }
    
    handleExplosion(packet) {
    }
    
    handleRoundStart(packet) {
        this.clientTick = 0;
        this.commandAck = 0;
    }
    
    handleRoundEnd(packet) {
    }
    
    handlePlayerDeath(packet) {
    }
    
    handlePlayerHurt(packet) {
    }
    
    handleBombPlant(packet) {
    }
    
    handleBombDefuse(packet) {
    }
    
    handleBombExplode(packet) {
    }
    
    handleHostageRescue(packet) {
    }
    
    handleVoteStart(packet) {
    }
    
    handleVoteEnd(packet) {
    }
    
    handleTeamScore(packet) {
    }
    
    handleGamePhase(packet) {
    }
    
    handleServerInfo(packet) {
        this.serverTick = packet.tickRate;
        this.tickRate = packet.tickRate;
    }
    
    handleChat(packet) {
    }
    
    generateConnectionId() {
        return Math.random().toString(36).substring(2, 15);
    }
    
    getPlayerName() {
        return 'Player';
    }
    
    getPlayerId() {
        return this.connectionId;
    }
    
    reset() {
        this.packets = {
            sent: 0,
            received: 0,
            lost: 0,
            corrupted: 0,
            queued: []
        };
        
        this.latency = {
            current: 0,
            average: 0,
            min: 0,
            max: 0,
            history: []
        };
        
        this.clientTick = 0;
        this.serverTick = 0;
        this.currentSnapshot = null;
        this.snapshots.clear();
        this.snapshotHistory.clear();
        this.entityState.clear();
        this.projectileState.clear();
        this.grenadeState.clear();
        this.inputSequence = 0;
        this.lastProcessedInput = 0;
        this.commands = [];
        this.commandAck = 0;
        this.packetQueue = [];
        this.outgoingData = [];
    }
    
    getStatistics() {
        return {
            connected: this.connected,
            latency: this.latency,
            packets: this.packets,
            bandwidth: this.bandwidth,
            clientTick: this.clientTick,
            serverTick: this.serverTick
        };
    }
}

export class NetworkPredictor {
    constructor() {
        this.predictedStates = new Map();
        this.corrections = [];
        this.maxPredictions = 64;
    }
    
    predict(entityId, input, currentState) {
        const predictedState = {
            ...currentState,
            position: { ...currentState.position },
            velocity: { ...currentState.velocity },
            rotation: { ...currentState.rotation }
        };
        
        this.applyInput(predictedState, input);
        
        this.predictedStates.set(entityId, predictedState);
        
        return predictedState;
    }
    
    applyInput(state, input) {
        if (input.forward) {
            state.velocity.x += Math.sin(state.rotation.yaw) * 5;
            state.velocity.y += Math.cos(state.rotation.yaw) * 5;
        }
        
        if (input.backward) {
            state.velocity.x -= Math.sin(state.rotation.yaw) * 5;
            state.velocity.y -= Math.cos(state.rotation.yaw) * 5;
        }
        
        if (input.left) {
            state.velocity.x -= Math.cos(state.rotation.yaw) * 5;
            state.velocity.y += Math.sin(state.rotation.yaw) * 5;
        }
        
        if (input.right) {
            state.velocity.x += Math.cos(state.rotation.yaw) * 5;
            state.velocity.y -= Math.sin(state.rotation.yaw) * 5;
        }
        
        if (input.jump && state.grounded) {
            state.velocity.z = 250;
            state.grounded = false;
        }
        
        state.position.x += state.velocity.x * 0.015;
        state.position.y += state.velocity.y * 0.015;
        state.position.z += state.velocity.z * 0.015;
        
        state.rotation.yaw += input.lookYaw * 0.002;
        state.rotation.pitch += input.lookPitch * 0.002;
    }
    
    compareAndCorrect(entityId, serverState) {
        const predictedState = this.predictedStates.get(entityId);
        if (!predictedState) return serverState;
        
        const positionError = this.getDistance(predictedState.position, serverState.position);
        const velocityError = this.getDistance(predictedState.velocity, serverState.velocity);
        
        const positionThreshold = 10;
        const velocityThreshold = 5;
        
        if (positionError > positionThreshold || velocityError > velocityThreshold) {
            this.corrections.push({
                entityId: entityId,
                serverState: serverState,
                predictedState: predictedState,
                error: positionError,
                time: Date.now()
            });
            
            this.predictedStates.delete(entityId);
            
            return serverState;
        }
        
        return predictedState;
    }
    
    getDistance(a, b) {
        const dx = a.x - b.x;
        const dy = a.y - b.y;
        const dz = (a.z || 0) - (b.z || 0);
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    cleanup() {
        const cutoff = Date.now() - 5000;
        this.corrections = this.corrections.filter(c => c.time > cutoff);
        
        if (this.predictedStates.size > this.maxPredictions) {
            const toRemove = Array.from(this.predictedStates.keys()).slice(0, this.predictedStates.size - this.maxPredictions);
            for (const key of toRemove) {
                this.predictedStates.delete(key);
            }
        }
    }
}

export class PacketBuilder {
    constructor() {
        this.buffer = [];
        this.position = 0;
    }
    
    writeUint8(value) {
        this.buffer.push(value & 0xFF);
    }
    
    writeUint16(value) {
        this.buffer.push((value >> 8) & 0xFF);
        this.buffer.push(value & 0xFF);
    }
    
    writeUint32(value) {
        this.buffer.push((value >> 24) & 0xFF);
        this.buffer.push((value >> 16) & 0xFF);
        this.buffer.push((value >> 8) & 0xFF);
        this.buffer.push(value & 0xFF);
    }
    
    writeFloat32(value) {
        const buffer = new ArrayBuffer(4);
        const view = new DataView(buffer);
        view.setFloat32(0, value, true);
        
        for (let i = 0; i < 4; i++) {
            this.buffer.push(view.getUint8(i));
        }
    }
    
    writeString(value) {
        const bytes = new TextEncoder().encode(value);
        this.writeUint16(bytes.length);
        
        for (const byte of bytes) {
            this.buffer.push(byte);
        }
    }
    
    writeBoolean(value) {
        this.writeUint8(value ? 1 : 0);
    }
    
    writeVector3(vector) {
        this.writeFloat32(vector.x);
        this.writeFloat32(vector.y);
        this.writeFloat32(vector.z);
    }
    
    writeVector2(vector) {
        this.writeFloat32(vector.x);
        this.writeFloat32(vector.y);
    }
    
    writeBytes(bytes) {
        for (const byte of bytes) {
            this.buffer.push(byte);
        }
    }
    
    getBuffer() {
        return new Uint8Array(this.buffer);
    }
    
    getArrayBuffer() {
        return this.getBuffer().buffer;
    }
    
    reset() {
        this.buffer = [];
        this.position = 0;
    }
}

export class PacketReader {
    constructor(buffer) {
        this.buffer = buffer instanceof Uint8Array ? buffer : new Uint8Array(buffer);
        this.position = 0;
    }
    
    readUint8() {
        return this.buffer[this.position++] & 0xFF;
    }
    
    readUint16() {
        const value = (this.buffer[this.position] << 8) | this.buffer[this.position + 1];
        this.position += 2;
        return value;
    }
    
    readUint32() {
        const value = (this.buffer[this.position] << 24) |
                     (this.buffer[this.position + 1] << 16) |
                     (this.buffer[this.position + 2] << 8) |
                     this.buffer[this.position + 3];
        this.position += 4;
        return value >>> 0;
    }
    
    readFloat32() {
        const buffer = new ArrayBuffer(4);
        const view = new DataView(buffer);
        
        for (let i = 0; i < 4; i++) {
            view.setUint8(i, this.buffer[this.position++]);
        }
        
        return view.getFloat32(0, true);
    }
    
    readString() {
        const length = this.readUint16();
        const bytes = this.buffer.slice(this.position, this.position + length);
        this.position += length;
        
        return new TextDecoder().decode(bytes);
    }
    
    readBoolean() {
        return this.readUint8() === 1;
    }
    
    readVector3() {
        return {
            x: this.readFloat32(),
            y: this.readFloat32(),
            z: this.readFloat32()
        };
    }
    
    readVector2() {
        return {
            x: this.readFloat32(),
            y: this.readFloat32()
        };
    }
    
    readBytes(length) {
        const bytes = this.buffer.slice(this.position, this.position + length);
        this.position += length;
        return bytes;
    }
    
    remaining() {
        return this.buffer.length - this.position;
    }
    
    reset() {
        this.position = 0;
    }
}

export class DeltaCompression {
    constructor() {
        this.baseline = null;
        this.fieldDefinitions = new Map();
    }
    
    setBaseline(data) {
        this.baseline = data;
    }
    
    encode(current, baseline = this.baseline) {
        const encoded = {};
        
        for (const [key, value] of Object.entries(current)) {
            if (baseline && baseline[key] !== undefined) {
                if (JSON.stringify(value) !== JSON.stringify(baseline[key])) {
                    encoded[key] = value;
                }
            } else {
                encoded[key] = value;
            }
        }
        
        return encoded;
    }
    
    decode(encoded, baseline = this.baseline) {
        if (!baseline) return encoded;
        
        const decoded = { ...baseline };
        
        for (const [key, value] of Object.entries(encoded)) {
            decoded[key] = value;
        }
        
        return decoded;
    }
    
    addFieldDefinition(fieldId, name, type, bitCount) {
        this.fieldDefinitions.set(fieldId, {
            name: name,
            type: type,
            bitCount: bitCount
        });
    }
}

export class ReliableUDP {
    constructor() {
        this.sentPackets = new Map();
        this.receivedPackets = new Map();
        this.acks = new Set();
        this.sentSequence = 0;
        this.receivedSequence = 0;
        this.maxPacketAge = 5000;
    }
    
    send(packet) {
        const sequence = this.sentSequence++;
        const timestamp = Date.now();
        
        const packetData = {
            sequence: sequence,
            timestamp: timestamp,
            data: packet
        };
        
        this.sentPackets.set(sequence, packetData);
        
        return packetData;
    }
    
    receive(packetData) {
        const sequence = packetData.sequence;
        
        if (sequence <= this.receivedSequence) {
            return null;
        }
        
        this.receivedPackets.set(sequence, packetData);
        this.receivedSequence = sequence;
        
        this.acks.add(sequence);
        
        return packetData;
    }
    
    processAck(sequence) {
        this.sentPackets.delete(sequence);
    }
    
    getUnackedPackets() {
        const now = Date.now();
        const unacked = [];
        
        for (const [sequence, packet] of this.sentPackets) {
            if (now - packet.timestamp > this.maxPacketAge) {
                unacked.push(packet);
                this.sentPackets.delete(sequence);
            }
        }
        
        return unacked;
    }
    
    cleanup() {
        const cutoff = Date.now() - this.maxPacketAge * 2;
        
        for (const [sequence, packet] of this.receivedPackets) {
            if (packet.timestamp < cutoff) {
                this.receivedPackets.delete(sequence);
            }
        }
    }
}

export class BandwidthLimiter {
    constructor(maxBytesPerSecond) {
        this.maxBytesPerSecond = maxBytesPerSecond;
        this.currentBytes = 0;
        this.lastReset = Date.now();
        this.packetQueue = [];
    }
    
    canSend(size) {
        this.checkReset();
        return this.currentBytes + size <= this.maxBytesPerSecond;
    }
    
    send(packet, size) {
        if (this.canSend(size)) {
            this.currentBytes += size;
            return true;
        }
        
        this.packetQueue.push({ packet, size, time: Date.now() });
        return false;
    }
    
    checkReset() {
        const now = Date.now();
        if (now - this.lastReset >= 1000) {
            this.currentBytes = 0;
            this.lastReset = now;
            
            while (this.packetQueue.length > 0 && this.canSend(this.packetQueue[0].size)) {
                const queued = this.packetQueue.shift();
                this.currentBytes += queued.size;
            }
        }
    }
}

import { GameState } from './config.js';

export const ServerStatus = {
    ONLINE: 'online',
    OFFLINE: 'offline',
    FULL: 'full',
    PASSWORD_PROTECTED: 'password',
    VAC_PROTECTED: 'vac',
    IN_PROGRESS: 'in_progress',
    BOTS_ONLY: 'bots_only'
};

export const ServerType = {
    OFFICIAL: 'official',
    COMMUNITY: 'community',
    PRIVATE: 'private',
    RENTED: 'rented',
    DEDICATED: 'dedicated',
    LISTEN: 'listen',
    SOURCE_TV: 'sourcetv'
};

export const ServerRegion = {
    NORTH_AMERICA: 'na',
    SOUTH_AMERICA: 'sa',
    EUROPE: 'eu',
    ASIA: 'asia',
    OCEANIA: 'oc',
    AFRICA: 'af',
    MIDDLE_EAST: 'me',
    GLOBAL: 'global'
};

export const ServerSortBy = {
    PING: 'ping',
    NAME: 'name',
    PLAYERS: 'players',
    MAP: 'map',
    VAC: 'vac',
    LAST_PLAYED: 'last_played',
    DATA_CENTER: 'data_center'
};

export const ServerFilter = {
    ALL: 'all',
    OFFICIAL: 'official',
    COMMUNITY: 'community',
    NO_PASSWORD: 'no_password',
    NOT_FULL: 'not_full',
    VAC_SECURE: 'vac_secure',
    HAS_PLAYERS: 'has_players',
    MAP_PREFIX: 'map_prefix'
};

export const PingStatus = {
    EXCELLENT: 'excellent',
    GOOD: 'good',
    FAIR: 'fair',
    POOR: 'poor',
    UNPLAYABLE: 'unplayable'
};

export const DataCenterStatus = {
    AVAILABLE: 'available',
    HIGH_LATENCY: 'high_latency',
    OVERLOADED: 'overloaded',
    MAINTENANCE: 'maintenance',
    UNAVAILABLE: 'unavailable'
};

export class ServerInfo {
    constructor(address, port = 27015) {
        this.address = address;
        this.port = port;
        this.name = '';
        this.map = '';
        this.gameDir = 'csgo';
        this.gameType = 0;
        this.maxPlayers = 10;
        this.players = 0;
        this.bots = 0;
        this.serverType = ServerType.DEDICATED;
        this.serverOS = 'windows';
        this.password = false;
        this.vac = true;
        this.version = '';
        this.ping = 0;
        this.pingTimestamp = 0;
        this.status = ServerStatus.ONLINE;
        this.region = ServerRegion.GLOBAL;
        this.dataCenter = '';
        this.tickRate = 128;
        this托管 = false;
        this.demoAvailable = false;
        this.stvEnabled = false;
        this.stvAddress = '';
        this.lastHeartbeat = 0;
        this.metadata = {};
        this.hostname = '';
        this.os = 'windows';
        this.dedicated = true;
        this.secure = true;
        this.mod = '';
        this.mode = '';
        this.tags = [];
        this.game = 'Counter-Strike: Global Offensive';
    }

    getId() {
        return `${this.address}:${this.port}`;
    }

    getDisplayName() {
        return this.hostname || this.name || this.getId();
    }

    getPingStatus() {
        if (this.ping < 30) return PingStatus.EXCELLENT;
        if (this.ping < 60) return PingStatus.GOOD;
        if (this.ping < 100) return PingStatus.FAIR;
        if (this.ping < 150) return PingStatus.POOR;
        return PingStatus.UNPLAYABLE;
    }

    getPlayerSlots() {
        return {
            used: this.players,
            total: this.maxPlayers,
            available: this.maxPlayers - this.players
        };
    }

    isFull() {
        return this.players >= this.maxPlayers;
    }

    isEmpty() {
        return this.players === 0;
    }

    hasVac() {
        return this.vac;
    }

    requiresPassword() {
        return this.password;
    }

    getJoinable() {
        return !this.isFull() && !this.requiresPassword() && this.status === ServerStatus.ONLINE;
    }

    updateFromMasterResponse(data) {
        if (data.address) this.address = data.address;
        if (data.port) this.port = data.port;
        if (data.name) this.name = data.name;
        if (data.map) this.map = data.map;
        if (data.players !== undefined) this.players = data.players;
        if (data.max_players !== undefined) this.maxPlayers = data.max_players;
        if (data.ping !== undefined) this.ping = data.ping;
    }

    export() {
        return {
            address: this.address,
            port: this.port,
            name: this.name,
            hostname: this.hostname,
            map: this.map,
            game: this.game,
            game_dir: this.gameDir,
            game_type: this.gameType,
            players: this.players,
            max_players: this.maxPlayers,
            bots: this.bots,
            server_type: this.serverType,
            os: this.os,
            password: this.password,
            secure: this.secure,
            version: this.version,
            ping: this.ping,
            region: this.region,
            data_center: this.dataCenter,
            tick_rate: this.tickRate,
            demo_available: this.demoAvailable,
            stv_enabled: this.stvEnabled,
            tags: this.tags,
            metadata: this.metadata
        };
    }

    static import(data) {
        const server = new ServerInfo(data.address, data.port);
        server.name = data.name || '';
        server.hostname = data.hostname || '';
        server.map = data.map || '';
        server.gameDir = data.game_dir || 'csgo';
        server.gameType = data.game_type || 0;
        server.players = data.players || 0;
        server.maxPlayers = data.max_players || 10;
        server.bots = data.bots || 0;
        server.serverType = data.server_type || ServerType.DEDICATED;
        server.os = data.os || 'windows';
        server.password = data.password || false;
        server.vac = data.secure !== false;
        server.version = data.version || '';
        server.ping = data.ping || 0;
        server.region = data.region || ServerRegion.GLOBAL;
        server.dataCenter = data.data_center || '';
        server.tickRate = data.tick_rate || 128;
        server.demoAvailable = data.demo_available || false;
        server.stvEnabled = data.stv_enabled || false;
        server.tags = data.tags || [];
        server.metadata = data.metadata || {};
        return server;
    }
}

export class DataCenter {
    constructor(id, name, region, location) {
        this.id = id;
        this.name = name;
        this.region = region;
        this.location = location;
        this.servers = 0;
        this.players = 0;
        this.averagePing = 0;
        this.status = DataCenterStatus.AVAILABLE;
        this.lastUpdated = new Date();
        this.suggested = false;
    }

    updateStats(servers, players, averagePing) {
        this.servers = servers;
        this.players = players;
        this.averagePing = averagePing;
        this.lastUpdated = new Date();
    }

    setStatus(status) {
        this.status = status;
    }

    isAvailable() {
        return this.status === DataCenterStatus.AVAILABLE;
    }

    getPingStatus() {
        if (this.averagePing < 30) return PingStatus.EXCELLENT;
        if (this.averagePing < 60) return PingStatus.GOOD;
        if (this.averagePing < 100) return PingStatus.FAIR;
        if (this.averagePing < 150) return PingStatus.POOR;
        return PingStatus.UNPLAYABLE;
    }
}

export class DataCenterManager {
    constructor() {
        this.dataCenters = new Map();
        this.suggestedDataCenter = null;
        this.initDefaultDataCenters();
    }

    initDefaultDataCenters() {
        const centers = [
            new DataCenter('us_east', 'US East', ServerRegion.NORTH_AMERICA, 'Virginia'),
            new DataCenter('us_west', 'US West', ServerRegion.NORTH_AMERICA, 'California'),
            new DataCenter('us_central', 'US Central', ServerRegion.NORTH_AMERICA, 'Texas'),
            new DataCenter('eu_west', 'EU West', ServerRegion.EUROPE, 'London'),
            new DataCenter('eu_east', 'EU East', ServerRegion.EUROPE, 'Frankfurt'),
            new DataCenter('eu_north', 'EU North', ServerRegion.EUROPE, 'Stockholm'),
            new DataCenter('asia_east', 'Asia East', ServerRegion.ASIA, 'Tokyo'),
            new DataCenter('asia_southeast', 'Asia Southeast', ServerRegion.ASIA, 'Singapore'),
            new DataCenter('asia_india', 'Asia India', ServerRegion.ASIA, 'Mumbai'),
            new DataCenter('australia', 'Australia', ServerRegion.OCEANIA, 'Sydney'),
            new DataCenter('south_america', 'South America', ServerRegion.SOUTH_AMERICA, 'São Paulo'),
            new DataCenter('middle_east', 'Middle East', ServerRegion.MIDDLE_EAST, 'Dubai'),
            new DataCenter('africa', 'Africa', ServerRegion.AFRICA, 'Johannesburg')
        ];

        for (const center of centers) {
            this.dataCenters.set(center.id, center);
        }
    }

    getAllDataCenters() {
        return Array.from(this.dataCenters.values());
    }

    getDataCenter(id) {
        return this.dataCenters.get(id);
    }

    getDataCentersByRegion(region) {
        return Array.from(this.dataCenters.values()).filter(dc => dc.region === region);
    }

    getAvailableDataCenters() {
        return Array.from(this.dataCenters.values()).filter(dc => dc.isAvailable());
    }

    suggestDataCenter(pingData) {
        let best = null;
        let bestPing = Infinity;

        for (const [id, ping] of Object.entries(pingData)) {
            if (ping < bestPing) {
                bestPing = ping;
                best = this.dataCenters.get(id);
            }
        }

        if (best) {
            best.suggested = true;
            this.suggestedDataCenter = best;
        }

        return best;
    }

    updatePingData(serverId, ping) {
        const center = this.findServerDataCenter(serverId);
        if (center) {
            center.averagePing = ping;
        }
    }

    findServerDataCenter(serverId) {
        const prefix = serverId.split('_')[0];
        const centerMap = {
            'use': 'us_east',
            'usw': 'us_west',
            'usc': 'us_central',
            'euw': 'eu_west',
            'eue': 'eu_east',
            'eun': 'eu_north',
            'ase': 'asia_east',
            'ass': 'asia_southeast',
            'asi': 'asia_india',
            'aus': 'australia',
            'sam': 'south_america',
            'me': 'middle_east',
            'afr': 'africa'
        };
        const dcId = centerMap[prefix] || 'us_east';
        return this.dataCenters.get(dcId);
    }
}

export class ServerQuery {
    constructor(address, port = 27015) {
        this.address = address;
        this.port = port;
        this.socket = null;
        this.timeout = 5000;
        this.retries = 3;
        this.challenge = '';
    }

    async getInfo() {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                const server = new ServerInfo(this.address, this.port);
                server.name = `Server ${this.address}:${this.port}`;
                server.map = 'de_dust2';
                server.players = Math.floor(Math.random() * 32);
                server.maxPlayers = 64;
                server.ping = Math.floor(Math.random() * 100);
                resolve(server);
            }, 100);
        });
    }

    async getPlayers() {
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve([]);
            }, 100);
        });
    }

    async getRules() {
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve({});
            }, 100);
        });
    }

    async ping() {
        return new Promise((resolve, reject) => {
            const start = Date.now();
            setTimeout(() => {
                const ping = Date.now() - start;
                resolve(ping);
            }, Math.random() * 50 + 10);
        });
    }
}

export class PingMeasurement {
    constructor(serverId, address, port) {
        this.serverId = serverId;
        this.address = address;
        this.port = port;
        this.pings = [];
        this.lastPing = 0;
        this.averagePing = 0;
        this.minPing = Infinity;
        this.maxPing = 0;
        this.loss = 0;
        this.samples = 0;
        this.lastUpdated = new Date();
    }

    addPing(ping) {
        if (ping > 0) {
            this.pings.push(ping);
            if (this.pings.length > 10) {
                this.pings.shift();
            }
            this.lastPing = ping;
            this.minPing = Math.min(this.minPing, ping);
            this.maxPing = Math.max(this.maxPing, ping);
            this.samples++;
            this.recalculateAverage();
        } else {
            this.loss++;
        }
        this.lastUpdated = new Date();
    }

    recalculateAverage() {
        if (this.pings.length > 0) {
            const sum = this.pings.reduce((a, b) => a + b, 0);
            this.averagePing = Math.round(sum / this.pings.length);
        }
    }

    getPingStatus() {
        if (this.averagePing < 30) return PingStatus.EXCELLENT;
        if (this.averagePing < 60) return PingStatus.GOOD;
        if (this.averagePing < 100) return PingStatus.FAIR;
        if (this.averagePing < 150) return PingStatus.POOR;
        return PingStatus.UNPLAYABLE;
    }

    getLossPercentage() {
        const total = this.samples + this.loss;
        return total > 0 ? (this.loss / total) * 100 : 0;
    }

    reset() {
        this.pings = [];
        this.lastPing = 0;
        this.averagePing = 0;
        this.minPing = Infinity;
        this.maxPing = 0;
        this.loss = 0;
        this.samples = 0;
    }
}

export class PingManager {
    constructor() {
        this.measurements = new Map();
        this.pingQueue = [];
        this.isPinging = false;
        this.pingConcurrency = 10;
        this.pingInterval = 30000;
        this.intervalId = null;
    }

    startPinging() {
        if (this.intervalId) return;
        this.intervalId = setInterval(() => this.pingAll(), this.pingInterval);
    }

    stopPinging() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
    }

    addServer(server) {
        const id = server.getId();
        if (!this.measurements.has(id)) {
            this.measurements.set(id, new PingMeasurement(id, server.address, server.port));
        }
        if (!this.pingQueue.includes(id)) {
            this.pingQueue.push(id);
        }
    }

    removeServer(serverId) {
        this.measurements.delete(serverId);
        const index = this.pingQueue.indexOf(serverId);
        if (index > -1) {
            this.pingQueue.splice(index, 1);
        }
    }

    getPing(serverId) {
        const measurement = this.measurements.get(serverId);
        return measurement ? measurement.averagePing : 0;
    }

    pingServer(serverId) {
        return new Promise((resolve) => {
            setTimeout(() => {
                const ping = Math.floor(Math.random() * 100) + 10;
                const measurement = this.measurements.get(serverId);
                if (measurement) {
                    measurement.addPing(ping);
                }
                resolve(ping);
            }, Math.random() * 200);
        });
    }

    async pingAll() {
        if (this.isPinging) return;
        this.isPinging = true;

        const servers = [...this.pingQueue];
        const batches = [];

        for (let i = 0; i < servers.length; i += this.pingConcurrency) {
            batches.push(servers.slice(i, i + this.pingConcurrency));
        }

        for (const batch of batches) {
            await Promise.all(batch.map(id => this.pingServer(id)));
        }

        this.isPinging = false;
    }

    getAllPings() {
        const pings = {};
        for (const [id, measurement] of this.measurements) {
            pings[id] = measurement.averagePing;
        }
        return pings;
    }

    getDataCenterPings() {
        const dcPings = {};
        for (const [id, measurement] of this.measurements) {
            const dcId = id.split(':')[0];
            if (!dcPings[dcId]) {
                dcPings[dcId] = [];
            }
            dcPings[dcId].push(measurement.averagePing);
        }

        const averages = {};
        for (const [dcId, pings] of Object.entries(dcPings)) {
            if (pings.length > 0) {
                averages[dcId] = Math.round(pings.reduce((a, b) => a + b, 0) / pings.length);
            }
        }
        return averages;
    }
}

export class ServerFilterOptions {
    constructor() {
        this.region = null;
        this.gameMode = null;
        this.map = null;
        this.mapPrefix = null;
        this.maxPing = null;
        this.minPlayers = 0;
        this.maxPlayers = 64;
        this.hasPassword = null;
        this.vacOnly = false;
        this.tickRate = null;
        this.tags = [];
        this.excludeTags = [];
        this.search = '';
        this.dataCenter = null;
        this.officialOnly = false;
        this.communityOnly = false;
    }

    matches(server) {
        if (this.region && server.region !== this.region) return false;
        if (this.gameMode !== null && server.gameType !== this.gameMode) return false;
        if (this.map && server.map !== this.map) return false;
        if (this.mapPrefix && !server.map.toLowerCase().startsWith(this.mapPrefix.toLowerCase())) return false;
        if (this.maxPing && server.ping > this.maxPing) return false;
        if (server.players < this.minPlayers) return false;
        if (server.players > this.maxPlayers) return false;
        if (this.hasPassword !== null && server.password !== this.hasPassword) return false;
        if (this.vacOnly && !server.vac) return false;
        if (this.tickRate && server.tickRate !== this.tickRate) return false;
        if (this.tags.length > 0) {
            const hasAllTags = this.tags.every(tag => server.tags.includes(tag));
            if (!hasAllTags) return false;
        }
        if (this.excludeTags.length > 0) {
            const hasAnyExclude = this.excludeTags.some(tag => server.tags.includes(tag));
            if (hasAnyExclude) return false;
        }
        if (this.search) {
            const searchLower = this.search.toLowerCase();
            const nameMatch = server.name.toLowerCase().includes(searchLower);
            const mapMatch = server.map.toLowerCase().includes(searchLower);
            const addressMatch = server.address.toLowerCase().includes(searchLower);
            if (!nameMatch && !mapMatch && !addressMatch) return false;
        }
        if (this.dataCenter && server.dataCenter !== this.dataCenter) return false;
        if (this.officialOnly && server.serverType !== ServerType.OFFICIAL) return false;
        if (this.communityOnly && server.serverType === ServerType.OFFICIAL) return false;
        return true;
    }

    clone() {
        const filter = new ServerFilterOptions();
        filter.region = this.region;
        filter.gameMode = this.gameMode;
        filter.map = this.map;
        filter.mapPrefix = this.mapPrefix;
        filter.maxPing = this.maxPing;
        filter.minPlayers = this.minPlayers;
        filter.maxPlayers = this.maxPlayers;
        filter.hasPassword = this.hasPassword;
        filter.vacOnly = this.vacOnly;
        filter.tickRate = this.tickRate;
        filter.tags = [...this.tags];
        filter.excludeTags = [...this.excludeTags];
        filter.search = this.search;
        filter.dataCenter = this.dataCenter;
        filter.officialOnly = this.officialOnly;
        filter.communityOnly = this.communityOnly;
        return filter;
    }

    toQuery() {
        const query = {};
        if (this.region) query.region = this.region;
        if (this.gameMode !== null) query.gamemode = this.gameMode;
        if (this.map) query.map = this.map;
        if (this.mapPrefix) query.map_prefix = this.mapPrefix;
        if (this.maxPing) query.max_ping = this.maxPing;
        if (this.minPlayers > 0) query.min_players = this.minPlayers;
        if (this.maxPlayers < 64) query.max_players = this.maxPlayers;
        if (this.hasPassword !== null) query.has_password = this.hasPassword;
        if (this.vacOnly) query.vac_only = true;
        if (this.tickRate) query.tickrate = this.tickRate;
        if (this.tags.length > 0) query.tags = this.tags.join(',');
        if (this.search) query.search = this.search;
        if (this.dataCenter) query.datacenter = this.dataCenter;
        if (this.officialOnly) query.official = true;
        if (this.communityOnly) query.community = true;
        return query;
    }

    static fromQuery(query) {
        const filter = new ServerFilterOptions();
        if (query.region) filter.region = query.region;
        if (query.gamemode !== undefined) filter.gameMode = query.gamemode;
        if (query.map) filter.map = query.map;
        if (query.map_prefix) filter.mapPrefix = query.map_prefix;
        if (query.max_ping) filter.maxPing = query.max_ping;
        if (query.min_players) filter.minPlayers = query.min_players;
        if (query.max_players) filter.maxPlayers = query.max_players;
        if (query.has_password !== undefined) filter.hasPassword = query.has_password;
        if (query.vac_only) filter.vacOnly = true;
        if (query.tickrate) filter.tickRate = query.tickrate;
        if (query.tags) filter.tags = query.tags.split(',');
        if (query.search) filter.search = query.search;
        if (query.datacenter) filter.dataCenter = query.datacenter;
        if (query.official) filter.officialOnly = true;
        if (query.community) filter.communityOnly = true;
        return filter;
    }
}

export class ServerBrowser {
    constructor() {
        this.servers = new Map();
        this.favorites = new Set();
        this.history = [];
        this.filter = new ServerFilterOptions();
        this.sortBy = ServerSortBy.PING;
        this.sortAscending = true;
        this.listeners = new Map();
        this.isLoading = false;
        this.lastRefresh = null;
        this.autoRefresh = false;
        this.refreshInterval = 30000;
        this.refreshTimer = null;
        this.masterServers = [];
        this.currentMasterIndex = 0;
    }

    addServer(server) {
        this.servers.set(server.getId(), server);
    }

    removeServer(serverId) {
        this.servers.delete(serverId);
    }

    getServer(serverId) {
        return this.servers.get(serverId);
    }

    getAllServers() {
        return Array.from(this.servers.values());
    }

    getFilteredServers() {
        let servers = this.getAllServers();
        servers = servers.filter(s => this.filter.matches(s));
        return this.sortServers(servers);
    }

    sortServers(servers) {
        const sortFunctions = {
            [ServerSortBy.PING]: (a, b) => a.ping - b.ping,
            [ServerSortBy.NAME]: (a, b) => a.name.localeCompare(b.name),
            [ServerSortBy.PLAYERS]: (a, b) => a.players - b.players,
            [ServerSortBy.MAP]: (a, b) => a.map.localeCompare(b.map),
            [ServerSortBy.VAC]: (a, b) => (b.vac ? 1 : 0) - (a.vac ? 1 : 0),
            [ServerSortBy.LAST_PLAYED]: (a, b) => (b.lastHeartbeat || 0) - (a.lastHeartbeat || 0),
            [ServerSortBy.DATA_CENTER]: (a, b) => (a.dataCenter || '').localeCompare(b.dataCenter || '')
        };

        const sortFn = sortFunctions[this.sortBy] || sortFunctions[ServerSortBy.PING];
        servers.sort(sortFn);

        if (!this.sortAscending) {
            servers.reverse();
        }

        return servers;
    }

    setFilter(filter) {
        this.filter = filter.clone();
        this.emit('filterChanged', this.filter);
    }

    getFilter() {
        return this.filter;
    }

    setSort(sortBy, ascending = true) {
        this.sortBy = sortBy;
        this.sortAscending = ascending;
        this.emit('sortChanged', { sortBy, ascending });
    }

    addFavorite(serverId) {
        this.favorites.add(serverId);
        this.emit('favoriteAdded', serverId);
    }

    removeFavorite(serverId) {
        this.favorites.delete(serverId);
        this.emit('favoriteRemoved', serverId);
    }

    isFavorite(serverId) {
        return this.favorites.has(serverId);
    }

    getFavorites() {
        return Array.from(this.favorites).map(id => this.servers.get(id)).filter(Boolean);
    }

    addToHistory(serverId, serverName) {
        this.history.unshift({
            serverId,
            serverName,
            timestamp: new Date()
        });
        if (this.history.length > 50) {
            this.history.pop();
        }
        this.emit('historyUpdated', this.history);
    }

    getHistory() {
        return [...this.history];
    }

    clearHistory() {
        this.history = [];
        this.emit('historyCleared');
    }

    setMasterServers(servers) {
        this.masterServers = servers;
        this.currentMasterIndex = 0;
    }

    getCurrentMaster() {
        return this.masterServers[this.currentMasterIndex] || 'master.steampowered.com';
    }

    async refresh() {
        this.isLoading = true;
        this.emit('refreshStarted');
        this.lastRefresh = new Date();

        try {
            await this.queryMasterServer();
            this.emit('refreshCompleted', this.getFilteredServers().length);
        } catch (error) {
            this.emit('refreshFailed', error);
        }

        this.isLoading = false;
    }

    async queryMasterServer() {
        return new Promise((resolve) => {
            setTimeout(() => {
                const sampleServers = this.generateSampleServers();
                for (const server of sampleServers) {
                    this.addServer(server);
                }
                resolve();
            }, 500);
        });
    }

    generateSampleServers() {
        const servers = [];
        const maps = ['de_dust2', 'de_mirage', 'de_inferno', 'de_nuke', 'de_overpass', 'de_train', 'de_vertigo', 'de_ancient'];
        const regions = [ServerRegion.NORTH_AMERICA, ServerRegion.EUROPE, ServerRegion.ASIA, ServerRegion.OCEANIA];
        const tickRates = [64, 128, tick_rate = 128];

        for (let i = 0; i < 100; i++) {
            const server = new ServerInfo(`192.168.1.${i % 255}`, 27015);
            server.name = `CS:GO Server #${i + 1}`;
            server.hostname = `CS:GO Community Server ${i + 1}`;
            server.map = maps[Math.floor(Math.random() * maps.length)];
            server.players = Math.floor(Math.random() * 32);
            server.maxPlayers = 64;
            server.region = regions[Math.floor(Math.random() * regions.length)];
            server.tickRate = tickRates[Math.floor(Math.random() * tickRates.length)];
            server.vac = Math.random() > 0.1;
            server.ping = Math.floor(Math.random() * 150) + 10;
            servers.push(server);
        }

        return servers;
    }

    startAutoRefresh() {
        if (this.refreshTimer) return;
        this.refreshTimer = setInterval(() => this.refresh(), this.refreshInterval);
        this.emit('autoRefreshStarted');
    }

    stopAutoRefresh() {
        if (this.refreshTimer) {
            clearInterval(this.refreshTimer);
            this.refreshTimer = null;
            this.emit('autoRefreshStopped');
        }
    }

    getStats() {
        const servers = this.getAllServers();
        let totalPlayers = 0;
        let totalCapacity = 0;

        for (const server of servers) {
            totalPlayers += server.players;
            totalCapacity += server.maxPlayers;
        }

        return {
            serverCount: servers.length,
            totalPlayers,
            totalCapacity,
            averagePing: servers.length > 0
                ? Math.round(servers.reduce((sum, s) => sum + s.ping, 0) / servers.length)
                : 0,
            favoriteCount: this.favorites.size,
            historyCount: this.history.length
        };
    }

    exportFavorites() {
        return Array.from(this.favorites);
    }

    importFavorites(favorites) {
        this.favorites = new Set(favorites);
    }

    exportHistory() {
        return this.history.map(h => ({
            serverId: h.serverId,
            serverName: h.serverName,
            timestamp: h.timestamp.toISOString()
        }));
    }

    importHistory(history) {
        this.history = history.map(h => ({
            serverId: h.serverId,
            serverName: h.serverName,
            timestamp: new Date(h.timestamp)
        }));
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
                console.error(`Error in server browser listener: ${e}`);
            }
        }
    }
}

export class ServerConnection {
    constructor(serverInfo) {
        this.server = serverInfo;
        this.state = 'disconnected';
        this.socket = null;
        this.retryCount = 0;
        this.maxRetries = 3;
        this.retryDelay = 5000;
        this.listeners = new Map();
        this.timeout = 30000;
        this.password = '';
    }

    setPassword(password) {
        this.password = password;
    }

    async connect() {
        if (this.state !== 'disconnected') {
            return false;
        }

        this.state = 'connecting';
        this.emit('connecting', this.server);

        try {
            await this.establishConnection();
            this.state = 'connected';
            this.emit('connected', this.server);
            return true;
        } catch (error) {
            this.state = 'failed';
            this.emit('connectionFailed', { server: this.server, error });
            
            if (this.retryCount < this.maxRetries) {
                this.retryCount++;
                this.emit('retrying', { attempt: this.retryCount, max: this.maxRetries });
                await this.delay(this.retryDelay);
                return this.connect();
            }
            
            return false;
        }
    }

    async establishConnection() {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                if (Math.random() > 0.1) {
                    resolve();
                } else {
                    reject(new Error('Connection refused'));
                }
            }, 500);
        });
    }

    async disconnect() {
        if (this.state === 'disconnected') {
            return true;
        }

        this.state = 'disconnecting';
        this.emit('disconnecting', this.server);

        await this.closeConnection();

        this.state = 'disconnected';
        this.emit('disconnected', this.server);
        return true;
    }

    async closeConnection() {
        return new Promise((resolve) => {
            setTimeout(resolve, 100);
        });
    }

    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    send(data) {
        if (this.state !== 'connected') {
            return false;
        }
        this.emit('dataSent', data);
        return true;
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
                console.error(`Error in server connection listener: ${e}`);
            }
        }
    }
}

export class ServerBrowserManager {
    static instance = null;

    constructor() {
        this.browser = new ServerBrowser();
        this.pingManager = new PingManager();
        this.dataCenterManager = new DataCenterManager();
        this.activeConnection = null;
        this.connectionHistory = [];
        this.listeners = new Map();
    }

    static getInstance() {
        if (!ServerBrowserManager.instance) {
            ServerBrowserManager.instance = new ServerBrowserManager();
        }
        return ServerBrowserManager.instance;
    }

    getBrowser() {
        return this.browser;
    }

    getPingManager() {
        return this.pingManager;
    }

    getDataCenterManager() {
        return this.dataCenterManager;
    }

    async connectToServer(server, password = '') {
        if (this.activeConnection) {
            await this.activeConnection.disconnect();
        }

        const connection = new ServerConnection(server);
        if (password) {
            connection.setPassword(password);
        }

        this.activeConnection = connection;
        this.browser.addToHistory(server.getId(), server.getDisplayName());

        const success = await connection.connect();
        if (success) {
            this.connectionHistory.unshift({
                server: server,
                timestamp: new Date()
            });
            if (this.connectionHistory.length > 10) {
                this.connectionHistory.pop();
            }
        }

        return connection;
    }

    disconnect() {
        if (this.activeConnection) {
            this.activeConnection.disconnect();
            this.activeConnection = null;
        }
    }

    getActiveConnection() {
        return this.activeConnection;
    }

    getConnectionHistory() {
        return [...this.connectionHistory];
    }

    getSuggestedDataCenter() {
        const pings = this.pingManager.getDataCenterPings();
        return this.dataCenterManager.suggestDataCenter(pings);
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
                console.error(`Error in server browser manager listener: ${e}`);
            }
        }
    }
}

export const ServerBrowser = ServerBrowserManager.getInstance();

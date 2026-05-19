export class MapDefinition {
    constructor(id, name, displayName, maxPlayers, bombSites, spawnPoints, zones) {
        this.id = id;
        this.name = name;
        this.displayName = displayName;
        this.maxPlayers = maxPlayers;
        this.minPlayers = 2;
        this.bombSites = bombSites || [];
        this.spawnPoints = spawnPoints || { counterTerrorist: [], terrorist: [] };
        this.zones = zones || [];
        this.navigationMesh = null;
        this.visibilityGraph = null;
        this.waypoints = [];
        this.hidingSpots = [];
        this.coverPositions = [];
        this.grenadeSpots = [];
        this.sniperPositions = [];
        this.rescueZones = [];
        this.hostageZones = [];
        this.dangerZones = [];
        this.bounds = { minX: 0, maxX: 0, minY: 0, maxY: 0, minZ: 0, maxZ: 0 };
        this.gridSize = 64;
        this.heightMap = null;
        this.materialMap = null;
        this.lighting = null;
        this.weather = null;
        this.radarImage = null;
        this.minimapScale = 1.0;
        this.difficulty = 'standard';
        this.tags = [];
        this.releaseDate = null;
        this.fileSize = 0;
        this.author = '';
        this.version = '1.0';
        this.customData = {};
    }

    getBombSiteCount() {
        return this.bombSites.length;
    }

    hasRescueZone() {
        return this.rescueZones.length > 0;
    }

    hasHostageZone() {
        return this.hostageZones.length > 0;
    }

    getSpawnPoint(team, index = 0) {
        const spawns = this.spawnPoints[team];
        if (!spawns || spawns.length === 0) return null;
        return spawns[index % spawns.length];
    }

    getRandomSpawnPoint(team) {
        const spawns = this.spawnPoints[team];
        if (!spawns || spawns.length === 0) return null;
        return spawns[Math.floor(Math.random() * spawns.length)];
    }

    isInBounds(x, y, z) {
        return x >= this.bounds.minX && x <= this.bounds.maxX &&
               y >= this.bounds.minY && y <= this.bounds.maxY &&
               z >= this.bounds.minZ && z <= this.bounds.maxZ;
    }

    getGridCell(x, y) {
        const cellX = Math.floor(x / this.gridSize);
        const cellY = Math.floor(y / this.gridSize);
        return { x: cellX, y: cellY };
    }

    getTerrainHeight(x, y) {
        if (!this.heightMap) return 0;
        const cell = this.getGridCell(x, y);
        const key = `${cell.x},${cell.y}`;
        return this.heightMap.get(key) || 0;
    }

    getMaterialAt(x, y) {
        if (!this.materialMap) return 'concrete';
        const cell = this.getGridCell(x, y);
        const key = `${cell.x},${cell.y}`;
        return this.materialMap.get(key) || 'concrete';
    }

    findNearestHidingSpot(position, maxDistance = 1000) {
        let nearest = null;
        let nearestDist = maxDistance;
        for (const spot of this.hidingSpots) {
            const dist = Math.sqrt(
                Math.pow(spot.x - position.x, 2) +
                Math.pow(spot.y - position.y, 2) +
                Math.pow(spot.z - position.z, 2)
            );
            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = spot;
            }
        }
        return nearest;
    }

    findNearestCover(position, maxDistance = 1000) {
        let nearest = null;
        let nearestDist = maxDistance;
        for (const cover of this.coverPositions) {
            const dist = Math.sqrt(
                Math.pow(cover.x - position.x, 2) +
                Math.pow(cover.y - position.y, 2) +
                Math.pow(cover.z - position.z, 2)
            );
            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = cover;
            }
        }
        return nearest;
    }

    isVisible(from, to) {
        if (!this.navigationMesh) return true;
        return this.navigationMesh.lineOfSight(from, to);
    }

    getNearestWaypoint(position, maxDistance = 500) {
        let nearest = null;
        let nearestDist = maxDistance;
        for (const waypoint of this.waypoints) {
            const dist = Math.sqrt(
                Math.pow(waypoint.x - position.x, 2) +
                Math.pow(waypoint.y - position.y, 2)
            );
            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = waypoint;
            }
        }
        return nearest;
    }

    getBombSiteById(id) {
        return this.bombSites.find(site => site.id === id);
    }

    getZoneAtPosition(position) {
        for (const zone of this.zones) {
            if (zone.contains(position)) {
                return zone;
            }
        }
        return null;
    }

    isInBuyZone(position, team) {
        for (const zone of this.zones) {
            if (zone.type === 'buy' && zone.team === team && zone.contains(position)) {
                return true;
            }
        }
        return false;
    }

    isInSpawnZone(position, team) {
        for (const zone of this.zones) {
            if (zone.type === 'spawn' && zone.team === team && zone.contains(position)) {
                return true;
            }
        }
        return false;
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            displayName: this.displayName,
            maxPlayers: this.maxPlayers,
            bombSites: this.bombSites.map(site => site.serialize()),
            spawnPoints: this.spawnPoints,
            bounds: this.bounds,
            gridSize: this.gridSize,
            difficulty: this.difficulty,
            tags: this.tags,
            version: this.version
        };
    }

    static deserialize(data) {
        const map = new MapDefinition(data.id, data.name, data.displayName);
        map.maxPlayers = data.maxPlayers;
        map.bounds = data.bounds;
        map.gridSize = data.gridSize;
        map.difficulty = data.difficulty;
        map.tags = data.tags;
        map.version = data.version;
        return map;
    }
}

export class BombSite {
    constructor(id, name, position, radius) {
        this.id = id;
        this.name = name;
        this.position = position;
        this.radius = radius;
        this.isArmed = false;
        this.isDefused = false;
        this.isExploded = false;
        this.plantTime = 0;
        this.defuseTime = 0;
        this.bombTimer = 40;
        this.canDefuseWithKit = true;
        this.defuseTimeWithKit = 5;
        this.defuseTimeWithoutKit = 10;
        this.spots = [];
        this.coverPositions = [];
        this.approachAngles = [];
    }

    contains(point) {
        const dx = point.x - this.position.x;
        const dy = point.y - this.position.y;
        const dz = point.z - this.position.z;
        const distance = Math.sqrt(dx * dx + dy * dy + dz * dz);
        return distance <= this.radius;
    }

    plant(team) {
        if (this.isExploded || this.isDefused) return false;
        this.isArmed = true;
        this.plantTime = Date.now();
        return true;
    }

    defuse(canUseKit) {
        if (!this.isArmed) return false;
        this.defuseTime = canUseKit ? this.defuseTimeWithKit : this.defuseTimeWithoutKit;
        return true;
    }

    update(deltaTime) {
        if (!this.isArmed || this.isDefused) return;
        const elapsed = (Date.now() - this.plantTime) / 1000;
        if (elapsed >= this.bombTimer) {
            this.explode();
        }
    }

    explode() {
        if (this.isExploded) return;
        this.isExploded = true;
        this.isArmed = false;
    }

    reset() {
        this.isArmed = false;
        this.isDefused = false;
        this.isExploded = false;
        this.plantTime = 0;
        this.defuseTime = 0;
    }

    serialize() {
        return {
            id: this.id,
            name: this.name,
            position: this.position,
            radius: this.radius,
            isArmed: this.isArmed,
            isDefused: this.isDefused,
            isExploded: this.isExploded
        };
    }
}

export class Zone {
    constructor(type, bounds, team = null, metadata = {}) {
        this.type = type;
        this.bounds = bounds;
        this.team = team;
        this.metadata = metadata;
        this.isActive = true;
        this.occupants = [];
        this.enterTime = new Map();
    }

    contains(point) {
        return point.x >= this.bounds.minX && point.x <= this.bounds.maxX &&
               point.y >= this.bounds.minY && point.y <= this.bounds.maxY &&
               point.z >= this.bounds.minZ && point.z <= this.bounds.maxZ;
    }

    onEnter(entity) {
        if (!this.occupants.includes(entity.id)) {
            this.occupants.push(entity.id);
            this.enterTime.set(entity.id, Date.now());
        }
    }

    onExit(entity) {
        const index = this.occupants.indexOf(entity.id);
        if (index !== -1) {
            this.occupants.splice(index, 1);
            this.enterTime.delete(entity.id);
        }
    }

    getOccupationTime(entityId) {
        const enterTime = this.enterTime.get(entityId);
        if (!enterTime) return 0;
        return (Date.now() - enterTime) / 1000;
    }

    getEntityCount() {
        return this.occupants.length;
    }

    isControlledByTeam(team) {
        if (this.metadata.requiredCount) {
            const teamCount = this.occupants.filter(id => 
                this.metadata.entityTeams && this.metadata.entityTeams[id] === team
            ).length;
            return teamCount >= this.metadata.requiredCount;
        }
        return false;
    }

    serialize() {
        return {
            type: this.type,
            bounds: this.bounds,
            team: this.team,
            metadata: this.metadata,
            isActive: this.isActive
        };
    }
}

export class NavigationMesh {
    constructor(map) {
        this.map = map;
        this.nodes = [];
        this.edges = [];
        this.triangles = [];
        this.portals = [];
        this.areas = [];
    }

    addNode(position, areaId = 0, flags = 0) {
        const node = {
            id: this.nodes.length,
            position: { ...position },
            areaId,
            flags,
            connections: [],
            cost: 0
        };
        this.nodes.push(node);
        return node;
    }

    addEdge(fromNodeId, toNodeId, cost = 1) {
        const edge = {
            from: fromNodeId,
            to: toNodeId,
            cost,
            bidirectional: true
        };
        this.edges.push(edge);
        this.nodes[fromNodeId].connections.push(toNodeId);
        if (edge.bidirectional) {
            this.nodes[toNodeId].connections.push(fromNodeId);
        }
        return edge;
    }

    findPath(start, end, maxIterations = 1000) {
        const startNode = this.findNearestNode(start);
        const endNode = this.findNearestNode(end);
        if (!startNode || !endNode) return [];

        const openSet = new Map();
        const closedSet = new Set();
        const cameFrom = new Map();
        const gScore = new Map();
        const fScore = new Map();

        const startId = startNode.id;
        openSet.set(startId, startNode);
        gScore.set(startId, 0);
        fScore.set(startId, this.heuristic(startNode.position, endNode.position));

        let iterations = 0;
        while (openSet.size > 0 && iterations < maxIterations) {
            iterations++;
            let current = null;
            let lowestF = Infinity;
            for (const [nodeId] of openSet) {
                const f = fScore.get(nodeId) || Infinity;
                if (f < lowestF) {
                    lowestF = f;
                    current = nodeId;
                }
            }

            if (current === endId) {
                return this.reconstructPath(cameFrom, current);
            }

            openSet.delete(current);
            closedSet.add(current);
            const currentNode = this.nodes[current];

            for (const neighborId of currentNode.connections) {
                if (closedSet.has(neighborId)) continue;
                const neighbor = this.nodes[neighborId];
                const edge = this.edges.find(e => 
                    (e.from === current && e.to === neighborId) ||
                    (e.to === current && e.from === neighborId)
                );
                const tentativeG = (gScore.get(current) || 0) + (edge?.cost || 1);

                if (!openSet.has(neighborId)) {
                    openSet.set(neighborId, neighbor);
                } else if (tentativeG >= (gScore.get(neighborId) || Infinity)) {
                    continue;
                }

                cameFrom.set(neighborId, current);
                gScore.set(neighborId, tentativeG);
                fScore.set(neighborId, tentativeG + this.heuristic(neighbor.position, endNode.position));
            }
        }

        return [];
    }

    findNearestNode(position) {
        let nearest = null;
        let nearestDist = Infinity;
        for (const node of this.nodes) {
            const dist = this.distance(position, node.position);
            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = node;
            }
        }
        return nearest;
    }

    heuristic(a, b) {
        return Math.sqrt(
            Math.pow(b.x - a.x, 2) +
            Math.pow(b.y - a.y, 2) +
            Math.pow(b.z - a.z, 2)
        );
    }

    distance(a, b) {
        return this.heuristic(a, b);
    }

    reconstructPath(cameFrom, current) {
        const path = [current];
        while (cameFrom.has(current)) {
            current = cameFrom.get(current);
            path.unshift(current);
        }
        return path.map(id => this.nodes[id].position);
    }

    lineOfSight(from, to) {
        const steps = 10;
        const dx = (to.x - from.x) / steps;
        const dy = (to.y - from.y) / steps;
        const dz = (to.z - from.z) / steps;

        for (let i = 0; i <= steps; i++) {
            const point = {
                x: from.x + dx * i,
                y: from.y + dy * i,
                z: from.z + dz * i
            };
            const node = this.findNearestNode(point);
            if (node && node.flags & 0x01) {
                return false;
            }
        }
        return true;
    }

    getNodesInRadius(position, radius) {
        return this.nodes.filter(node => 
            this.distance(position, node.position) <= radius
        );
    }

    getNodesByArea(areaId) {
        return this.nodes.filter(node => node.areaId === areaId);
    }

    serialize() {
        return {
            nodes: this.nodes.map(n => ({
                id: n.id,
                position: n.position,
                areaId: n.areaId,
                flags: n.flags
            })),
            edges: this.edges
        };
    }
}

export class Waypoint {
    constructor(position, type = 'normal') {
        this.id = Waypoint.nextId++;
        this.position = position;
        this.type = type;
        this.connections = [];
        this.properties = {
            isJumpSpot: false,
            isCrouchSpot: false,
            isLadderSpot: false,
            isCoverSpot: false,
            dangerLevel: 0,
            visibility: 1.0,
            height: position.z
        };
        this.tags = [];
        this.associatedGrenades = [];
        this.visibilityTo = new Map();
    }

    addConnection(waypointId, cost = 1, properties = {}) {
        this.connections.push({
            waypointId,
            cost,
            ...properties
        });
    }

    removeConnection(waypointId) {
        const index = this.connections.findIndex(c => c.waypointId === waypointId);
        if (index !== -1) {
            this.connections.splice(index, 1);
        }
    }

    hasLineOfSightTo(position) {
        return this.visibilityTo.get(`${position.x},${position.y},${position.z}`) === 1.0;
    }

    serialize() {
        return {
            id: this.id,
            position: this.position,
            type: this.type,
            connections: this.connections,
            properties: this.properties,
            tags: this.tags
        };
    }
}
Waypoint.nextId = 0;

export class HidingSpot {
    constructor(position, type, quality) {
        this.id = HidingSpot.nextId++;
        this.position = position;
        this.type = type;
        this.quality = quality;
        this.properties = {
            isCrouched: false,
            isStanding: false,
            coverHeight: 0,
            dangerLevel: 0
        };
        this.nearbyWaypoints = [];
        this.associatedGrenades = [];
    }

    getEffectiveness(playerHeight, threatDirection) {
        let effectiveness = this.quality;
        if (this.properties.isCrouched && playerHeight < 60) {
            effectiveness *= 1.2;
        }
        if (threatDirection) {
            const dotProduct = Math.cos(this.properties.coverHeight - threatDirection);
            effectiveness *= Math.max(0.1, dotProduct);
        }
        return Math.min(1.0, effectiveness);
    }

    serialize() {
        return {
            id: this.id,
            position: this.position,
            type: this.type,
            quality: this.quality,
            properties: this.properties
        };
    }
}
HidingSpot.nextId = 0;

export class CoverPosition {
    constructor(position, coverType, height) {
        this.id = CoverPosition.nextId++;
        this.position = position;
        this.coverType = coverType;
        this.height = height;
        this.leanOptions = ['left', 'right', 'none'];
        this.currentLean = 'none';
        this.visibility = 1.0;
        this.flankRoutes = [];
        this.associatedGrenades = [];
    }

    canLean(leanDirection) {
        return this.leanOptions.includes(leanDirection);
    }

    getBestLean(threatDirection) {
        if (threatDirection === undefined) return 'none';
        let bestLean = 'none';
        let bestScore = -Infinity;
        for (const lean of this.leanOptions) {
            let score = 0;
            if (lean === 'left') score = -90;
            else if (lean === 'right') score = 90;
            const diff = Math.abs(threatDirection - score);
            score -= diff * 0.5;
            if (score > bestScore) {
                bestScore = score;
                bestLean = lean;
            }
        }
        return bestLean;
    }

    serialize() {
        return {
            id: this.id,
            position: this.position,
            coverType: this.coverType,
            height: this.height,
            leanOptions: this.leanOptions,
            visibility: this.visibility,
            flankRoutes: this.flankRoutes
        };
    }
}
CoverPosition.nextId = 0;

export class GrenadeSpot {
    constructor(position, grenadeType, effect) {
        this.id = GrenadeSpot.nextId++;
        this.position = position;
        this.grenadeType = grenadeType;
        this.effect = effect;
        this.targetPosition = null;
        this.difficulty = 'medium';
        this.successRate = 0.5;
        this.associatedWaypoints = [];
        this.alternativeThrows = [];
        this.description = '';
        this.createdBy = null;
        this.votes = 0;
        this.tags = [];
    }

    setTarget(position) {
        this.targetPosition = { ...position };
        this.calculateDifficulty();
    }

    calculateDifficulty() {
        if (!this.targetPosition) return;
        const dx = this.targetPosition.x - this.position.x;
        const dy = this.targetPosition.y - this.position.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        const dz = this.targetPosition.z - this.position.z;
        const heightDiff = Math.abs(dz);

        if (distance > 2000 || heightDiff > 500) {
            this.difficulty = 'hard';
            this.successRate = 0.2;
        } else if (distance > 1000 || heightDiff > 200) {
            this.difficulty = 'medium';
            this.successRate = 0.5;
        } else {
            this.difficulty = 'easy';
            this.successRate = 0.8;
        }
    }

    canBeUsedWith(grenadeType) {
        return this.grenadeType === grenadeType;
    }

    serialize() {
        return {
            id: this.id,
            position: this.position,
            grenadeType: this.grenadeType,
            effect: this.effect,
            targetPosition: this.targetPosition,
            difficulty: this.difficulty,
            successRate: this.successRate,
            description: this.description,
            tags: this.tags
        };
    }
}
GrenadeSpot.nextId = 0;

export class MapLoader {
    constructor() {
        this.maps = new Map();
        this.activeMap = null;
        this.loadingProgress = 0;
        this.loadedAssets = new Set();
        this.assetManifests = new Map();
    }

    registerMap(map) {
        this.maps.set(map.id, map);
    }

    unregisterMap(mapId) {
        this.maps.delete(mapId);
    }

    getMap(mapId) {
        return this.maps.get(mapId);
    }

    getAllMaps() {
        return Array.from(this.maps.values());
    }

    getMapsByTag(tag) {
        return this.getAllMaps().filter(map => map.tags.includes(tag));
    }

    getMapsByDifficulty(difficulty) {
        return this.getAllMaps().filter(map => map.difficulty === difficulty);
    }

    async loadMap(mapId, onProgress = null) {
        const map = this.maps.get(mapId);
        if (!map) {
            throw new Error(`Map not found: ${mapId}`);
        }

        this.loadingProgress = 0;
        const totalSteps = 10;

        await this.loadMapDefinition(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadNavigationMesh(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadTextures(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadSounds(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadEntities(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadWaypoints(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadSpawnPoints(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadZones(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.loadLighting(map, onProgress, totalSteps);
        this.loadingProgress += 100 / totalSteps;
        if (onProgress) onProgress(this.loadingProgress);

        await this.finalizeLoading(map, onProgress, totalSteps);
        this.loadingProgress = 100;
        if (onProgress) onProgress(this.loadingProgress);

        this.activeMap = map;
        return map;
    }

    async loadMapDefinition(map, onProgress, totalSteps) {
        await this.simulateLoad(50);
    }

    async loadNavigationMesh(map, onProgress, totalSteps) {
        if (!map.navigationMesh) {
            map.navigationMesh = new NavigationMesh(map);
        }
        await this.simulateLoad(100);
    }

    async loadTextures(map, onProgress, totalSteps) {
        await this.simulateLoad(150);
    }

    async loadSounds(map, onProgress, totalSteps) {
        await this.simulateLoad(80);
    }

    async loadEntities(map, onProgress, totalSteps) {
        await this.simulateLoad(60);
    }

    async loadWaypoints(map, onProgress, totalSteps) {
        await this.simulateLoad(40);
    }

    async loadSpawnPoints(map, onProgress, totalSteps) {
        await this.simulateLoad(30);
    }

    async loadZones(map, onProgress, totalSteps) {
        await this.simulateLoad(50);
    }

    async loadLighting(map, onProgress, totalSteps) {
        await this.simulateLoad(40);
    }

    async finalizeLoading(map, onProgress, totalSteps) {
        await this.simulateLoad(20);
    }

    async simulateLoad(duration) {
        return new Promise(resolve => setTimeout(resolve, duration));
    }

    unloadMap() {
        if (this.activeMap) {
            this.activeMap = null;
            this.loadedAssets.clear();
        }
    }

    getLoadingProgress() {
        return this.loadingProgress;
    }

    isMapLoaded(mapId) {
        return this.activeMap && this.activeMap.id === mapId;
    }

    getLoadedAssets() {
        return Array.from(this.loadedAssets);
    }

    generateStandardMaps() {
        const maps = [
            this.createDeDust2(),
            this.createDeMirage(),
            this.createDeInferno(),
            this.createDeNuke(),
            this.createDeOverpass(),
            this.createDeTrain(),
            this.createDeVertigo(),
            this.createDeAncient()
        ];
        maps.forEach(map => this.registerMap(map));
        return maps;
    }

    createDeDust2() {
        const map = new MapDefinition('de_dust2', 'Dust II', 'Dust II', 10);
        map.bounds = { minX: -4096, maxX: 4096, minY: -4096, maxY: 4096, minZ: -1024, maxZ: 2048 };
        map.difficulty = 'standard';
        map.tags = ['competitive', 'desert', 'classic'];
        map.minimapScale = 0.8;
        
        const siteA = new BombSite('A', 'Bomb Site A', { x: 1500, y: 2500, z: 0 }, 400);
        siteA.spots = [
            { x: 1600, y: 2600, z: 0 },
            { x: 1400, y: 2400, z: 0 }
        ];
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: -1500, y: -2500, z: 0 }, 400);
        siteB.spots = [
            { x: -1600, y: -2600, z: 0 },
            { x: -1400, y: -2400, z: 0 }
        ];
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 2500, y: -2500, z: 0 },
            { x: 2700, y: -2300, z: 0 },
            { x: 2300, y: -2700, z: 0 },
            { x: 2900, y: -2100, z: 0 },
            { x: 2100, y: -2900, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -2500, y: 2500, z: 0 },
            { x: -2700, y: 2300, z: 0 },
            { x: -2300, y: 2700, z: 0 },
            { x: -2900, y: 2100, z: 0 },
            { x: -2100, y: 2900, z: 0 }
        ];

        map.zones.push(new Zone('buy', { 
            minX: 2400, maxX: 2800, minY: -2800, maxY: -2200, minZ: -100, maxZ: 300 
        }, 'counterTerrorist'));

        map.zones.push(new Zone('buy', { 
            minX: -2800, maxX: -2400, minY: 2200, maxY: 2800, minZ: -100, maxZ: 300 
        }, 'terrorist'));

        map.zones.push(new Zone('spawn', { 
            minX: 2200, maxX: 3000, minY: -3000, maxY: -2000, minZ: -100, maxZ: 300 
        }, 'counterTerrorist'));

        map.zones.push(new Zone('spawn', { 
            minX: -3000, maxX: -2200, minY: 2000, maxY: 3000, minZ: -100, maxZ: 300 
        }, 'terrorist'));

        return map;
    }

    createDeMirage() {
        const map = new MapDefinition('de_mirage', 'Mirage', 'Mirage', 10);
        map.bounds = { minX: -3500, maxX: 3500, minY: -3500, maxY: 3500, minZ: -1024, maxZ: 2048 };
        map.difficulty = 'standard';
        map.tags = ['competitive', 'urban', 'classic'];
        map.minimapScale = 0.75;

        const siteA = new BombSite('A', 'Bomb Site A', { x: 1000, y: 2000, z: 200 }, 350);
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: -2000, y: -1500, z: 0 }, 350);
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 1500, y: -2500, z: 0 },
            { x: 1700, y: -2300, z: 0 },
            { x: 1300, y: -2700, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -1500, y: 2500, z: 0 },
            { x: -1700, y: 2300, z: 0 },
            { x: -1300, y: 2700, z: 0 }
        ];

        return map;
    }

    createDeInferno() {
        const map = new MapDefinition('de_inferno', 'Inferno', 'Inferno', 10);
        map.bounds = { minX: -4000, maxX: 4000, minY: -4000, maxY: 4000, minZ: -1024, maxZ: 2048 };
        map.difficulty = 'standard';
        map.tags = ['competitive', 'village', 'classic'];
        map.minimapScale = 0.85;

        const siteA = new BombSite('A', 'Bomb Site A', { x: 2000, y: 1500, z: 0 }, 400);
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: -1500, y: -2000, z: 0 }, 400);
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 2500, y: -2000, z: 0 },
            { x: 2700, y: -1800, z: 0 },
            { x: 2300, y: -2200, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -2500, y: 2000, z: 0 },
            { x: -2700, y: 1800, z: 0 },
            { x: -2300, y: 2200, z: 0 }
        ];

        map.rescueZones.push({ x: 3000, y: 0, z: 0, radius: 200 });

        return map;
    }

    createDeNuke() {
        const map = new MapDefinition('de_nuke', 'Nuke', 'Nuke', 10);
        map.bounds = { minX: -3500, maxX: 3500, minY: -3500, maxY: 3500, minZ: -2048, maxZ: 4096 };
        map.difficulty = 'hard';
        map.tags = ['competitive', 'industrial', 'vertical'];
        map.minimapScale = 0.9;

        const siteA = new BombSite('A', 'Bomb Site A', { x: 0, y: 2000, z: 1500 }, 300);
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: 0, y: -2000, z: 0 }, 400);
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 2000, y: -2500, z: 0 },
            { x: 2200, y: -2300, z: 0 },
            { x: 1800, y: -2700, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -2000, y: 2500, z: 0 },
            { x: -2200, y: 2300, z: 0 },
            { x: -1800, y: 2700, z: 0 }
        ];

        return map;
    }

    createDeOverpass() {
        const map = new MapDefinition('de_overpass', 'Overpass', 'Overpass', 10);
        map.bounds = { minX: -4000, maxX: 4000, minY: -4000, maxY: 4000, minZ: -1024, maxZ: 4096 };
        map.difficulty = 'hard';
        map.tags = ['competitive', 'construction', 'vertical'];
        map.minimapScale = 0.8;

        const siteA = new BombSite('A', 'Bomb Site A', { x: 2000, y: 2000, z: 500 }, 350);
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: -2000, y: -2000, z: 500 }, 350);
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 2500, y: -2500, z: 0 },
            { x: 2700, y: -2300, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -2500, y: 2500, z: 0 },
            { x: -2700, y: 2300, z: 0 }
        ];

        return map;
    }

    createDeTrain() {
        const map = new MapDefinition('de_train', 'Train', 'Train', 10);
        map.bounds = { minX: -4096, maxX: 4096, minY: -4096, maxY: 4096, minZ: -1024, maxZ: 2048 };
        map.difficulty = 'standard';
        map.tags = ['competitive', 'train', 'industrial'];
        map.minimapScale = 0.85;

        const siteA = new BombSite('A', 'Bomb Site A', { x: 1500, y: 2000, z: 0 }, 400);
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: -1500, y: -2000, z: 0 }, 400);
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 2500, y: 0, z: 0 },
            { x: 2700, y: 200, z: 0 },
            { x: 2300, y: -200, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -2500, y: 0, z: 0 },
            { x: -2700, y: 200, z: 0 },
            { x: -2300, y: -200, z: 0 }
        ];

        return map;
    }

    createDeVertigo() {
        const map = new MapDefinition('de_vertigo', 'Vertigo', 'Vertigo', 10);
        map.bounds = { minX: -3500, maxX: 3500, minY: -3500, maxY: 3500, minZ: -1024, maxZ: 8192 };
        map.difficulty = 'hard';
        map.tags = ['competitive', 'skyscraper', 'vertical'];
        map.minimapScale = 0.75;

        const siteA = new BombSite('A', 'Bomb Site A', { x: 0, y: 1500, z: 4000 }, 350);
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: 0, y: -1500, z: 0 }, 350);
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 1500, y: -2000, z: 0 },
            { x: 1700, y: -1800, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -1500, y: 2000, z: 0 },
            { x: -1700, y: 1800, z: 0 }
        ];

        return map;
    }

    createDeAncient() {
        const map = new MapDefinition('de_ancient', 'Ancient', 'Ancient', 10);
        map.bounds = { minX: -4000, maxX: 4000, minY: -4000, maxY: 4000, minZ: -1024, maxZ: 2048 };
        map.difficulty = 'standard';
        map.tags = ['competitive', 'jungle', 'modern'];
        map.minimapScale = 0.8;

        const siteA = new BombSite('A', 'Bomb Site A', { x: 1500, y: 2000, z: 0 }, 400);
        map.bombSites.push(siteA);

        const siteB = new BombSite('B', 'Bomb Site B', { x: -1500, y: -2000, z: 0 }, 400);
        map.bombSites.push(siteB);

        map.spawnPoints.counterTerrorist = [
            { x: 2000, y: -2500, z: 0 },
            { x: 2200, y: -2300, z: 0 }
        ];
        map.spawnPoints.terrorist = [
            { x: -2000, y: 2500, z: 0 },
            { x: -2200, y: 2300, z: 0 }
        ];

        return map;
    }
}

export class MapManager {
    constructor() {
        this.loader = new MapLoader();
        this.activeMap = null;
        this.mapInstances = new Map();
        this.mapState = new Map();
    }

    initialize() {
        this.loader.generateStandardMaps();
    }

    async loadMap(mapId, onProgress = null) {
        const map = await this.loader.loadMap(mapId, onProgress);
        this.activeMap = map;
        return map;
    }

    unloadMap() {
        if (this.activeMap) {
            this.mapInstances.delete(this.activeMap.id);
            this.mapState.delete(this.activeMap.id);
            this.loader.unloadMap();
            this.activeMap = null;
        }
    }

    getActiveMap() {
        return this.activeMap;
    }

    getMap(mapId) {
        return this.loader.getMap(mapId);
    }

    getAllMaps() {
        return this.loader.getAllMaps();
    }

    getMapsByTag(tag) {
        return this.loader.getMapsByTag(tag);
    }

    getCompetitiveMaps() {
        return this.getMapsByTag('competitive');
    }

    getMapState(mapId) {
        if (!this.mapState.has(mapId)) {
            this.mapState.set(mapId, this.createInitialMapState(mapId));
        }
        return this.mapState.get(mapId);
    }

    createInitialMapState(mapId) {
        const map = this.getMap(mapId);
        if (!map) return null;

        return {
            mapId,
            bombSites: map.bombSites.map(site => ({
                id: site.id,
                isArmed: false,
                isDefused: false,
                isExploded: false
            })),
            zones: map.zones.map(zone => ({
                type: zone.type,
                team: zone.team,
                isActive: true
            })),
            roundTime: 115,
            freezeTime: 15,
            currentPhase: 'warmup'
        };
    }

    updateMapState(mapId, deltaTime) {
        const state = this.getMapState(mapId);
        if (!state) return;

        for (const site of state.bombSites) {
            if (site.isArmed && !site.isDefused) {
                site.timer -= deltaTime;
                if (site.timer <= 0) {
                    site.isExploded = true;
                    site.isArmed = false;
                }
            }
        }
    }

    armBomb(siteId) {
        if (!this.activeMap) return false;
        const site = this.activeMap.getBombSiteById(siteId);
        if (!site) return false;
        return site.plant('terrorist');
    }

    defuseBomb(siteId, hasKit) {
        if (!this.activeMap) return false;
        const site = this.activeMap.getBombSiteById(siteId);
        if (!site) return false;
        return site.defuse(hasKit);
    }

    isInBuyZone(position, team) {
        if (!this.activeMap) return false;
        return this.activeMap.isInBuyZone(position, team);
    }

    isInSpawnZone(position, team) {
        if (!this.activeMap) return false;
        return this.activeMap.isInSpawnZone(position, team);
    }

    getSpawnPoint(team, index = 0) {
        if (!this.activeMap) return null;
        return this.activeMap.getSpawnPoint(team, index);
    }

    findPath(start, end) {
        if (!this.activeMap || !this.activeMap.navigationMesh) return [];
        return this.activeMap.navigationMesh.findPath(start, end);
    }

    findNearestNode(position) {
        if (!this.activeMap || !this.activeMap.navigationMesh) return null;
        return this.activeMap.navigationMesh.findNearestNode(position);
    }

    getNearestCover(position, maxDistance = 1000) {
        if (!this.activeMap) return null;
        return this.activeMap.findNearestCover(position, maxDistance);
    }

    getNearestHidingSpot(position, maxDistance = 1000) {
        if (!this.activeMap) return null;
        return this.activeMap.findNearestHidingSpot(position, maxDistance);
    }
}

export default MapManager;

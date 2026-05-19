import { GAME_CONFIG, MAP_TYPE } from './config.js';
import { Vector3, AABB } from './physics.js';

export class Map {
    constructor(mapId, data = {}) {
        this.id = mapId;
        this.name = data.name || 'Unknown Map';
        this.type = data.type || MAP_TYPE.BOMB_DEFUSAL;
        
        this.bounds = {
            min: data.bounds?.min || { x: -1000, y: -1000, z: 0 },
            max: data.bounds?.max || { x: 1000, y: 1000, z: 200 }
        };
        
        this.spawns = {
            terrorist: data.spawns?.terrorist || [],
            counterTerrorist: data.spawns?.counterTerrorist || []
        };
        
        this.bombSites = data.bombSites || [];
        this.hostageZones = data.hostageZones || [];
        this.rescueZones = data.rescueZones || [];
        this.buyZones = data.buyZones || [];
        
        this.waypoints = data.waypoints || [];
        this.coverSpots = data.coverSpots || [];
        this.grenadeSpots = data.grenadeSpots || [];
        
        this.geometry = data.geometry || [];
        this.lights = data.lights || [];
        this.props = data.props || [];
        this.decals = data.decals || [];
        
        this.fog = data.fog || {
            enabled: false,
            color: { r: 128, g: 128, b: 128 },
            near: 100,
            far: 1000
        };
        
        this.ambientColor = data.ambientColor || { r: 50, g: 50, b: 50 };
        this.skyColor = data.skyColor || { r: 100, g: 150, b: 200 };
        
        this.navigationMesh = data.navigationMesh || null;
        this.collisionMesh = data.collisionMesh || null;
        
        this.metadata = data.metadata || {};
        
        this.loaded = false;
    }
    
    load() {
        console.log(`Loading map: ${this.name}`);
        this.loaded = true;
    }
    
    unload() {
        this.loaded = false;
    }
    
    isLoaded() {
        return this.loaded;
    }
    
    getSpawnPosition(team) {
        const spawns = team === 'terrorist' ? this.spawns.terrorist : this.spawns.counterTerrorist;
        if (spawns.length === 0) return { x: 0, y: 0, z: 0 };
        
        const index = Math.floor(Math.random() * spawns.length);
        return { ...spawns[index] };
    }
    
    getBombSite(siteId) {
        return this.bombSites.find(site => site.id === siteId);
    }
    
    getHostageZone(zoneId) {
        return this.hostageZones.find(zone => zone.id === zoneId);
    }
    
    isInBombZone(position) {
        for (const site of this.bombSites) {
            if (this.isPointInAABB(position, site.bounds)) {
                return site;
            }
        }
        return null;
    }
    
    isInBuyZone(position, team) {
        for (const zone of this.buyZones) {
            if (zone.team === team && this.isPointInAABB(position, zone.bounds)) {
                return true;
            }
        }
        return false;
    }
    
    isInHostageZone(position) {
        for (const zone of this.hostageZones) {
            if (this.isPointInAABB(position, zone.bounds)) {
                return zone;
            }
        }
        return null;
    }
    
    isInRescueZone(position) {
        for (const zone of this.rescueZones) {
            if (this.isPointInAABB(position, zone.bounds)) {
                return true;
            }
        }
        return false;
    }
    
    isPointInAABB(point, aabb) {
        return point.x >= aabb.min.x && point.x <= aabb.max.x &&
               point.y >= aabb.min.y && point.y <= aabb.max.y &&
               point.z >= aabb.min.z && point.z <= aabb.max.z;
    }
    
    isLineOfSight(from, to) {
        return true;
    }
    
    findPath(from, to) {
        return [from, to];
    }
    
    getNearestWaypoint(position) {
        let nearest = null;
        let nearestDistance = Infinity;
        
        for (const waypoint of this.waypoints) {
            const distance = this.getDistance(position, waypoint.position);
            if (distance < nearestDistance) {
                nearestDistance = distance;
                nearest = waypoint;
            }
        }
        
        return nearest;
    }
    
    getCoverSpotNear(position, distance = 100) {
        return this.coverSpots.find(spot => {
            return this.getDistance(position, spot.position) <= distance;
        });
    }
    
    getDistance(a, b) {
        const dx = a.x - b.x;
        const dy = a.y - b.y;
        const dz = (a.z || 0) - (b.z || 0);
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }
}

export class MapManager {
    constructor() {
        this.maps = new Map();
        this.currentMap = null;
        this.loadingCallbacks = [];
    }
    
    registerMap(map) {
        this.maps.set(map.id, map);
    }
    
    loadMap(mapId) {
        const map = this.maps.get(mapId);
        if (!map) {
            console.error(`Map not found: ${mapId}`);
            return Promise.reject(`Map not found: ${mapId}`);
        }
        
        if (this.currentMap) {
            this.currentMap.unload();
        }
        
        return new Promise((resolve, reject) => {
            map.load();
            
            setTimeout(() => {
                this.currentMap = map;
                this.onMapLoaded(map);
                resolve(map);
            }, 500);
        });
    }
    
    onMapLoaded(map) {
        for (const callback of this.loadingCallbacks) {
            callback(map);
        }
    }
    
    onMapLoaded(callback) {
        this.loadingCallbacks.push(callback);
    }
    
    getMap(mapId) {
        return this.maps.get(mapId);
    }
    
    getCurrentMap() {
        return this.currentMap;
    }
    
    unloadCurrentMap() {
        if (this.currentMap) {
            this.currentMap.unload();
            this.currentMap = null;
        }
    }
    
    getAllMaps() {
        return Array.from(this.maps.values());
    }
    
    initialize() {
        this.createDefaultMaps();
    }
    
    createDefaultMaps() {
        const dust2 = this.createDust2();
        this.registerMap(dust2);
        
        const mirage = this.createMirage();
        this.registerMap(mirage);
        
        const inferno = this.createInferno();
        this.registerMap(inferno);
        
        const nuke = this.createNuke();
        this.registerMap(nuke);
        
        const overpass = this.createOverpass();
        this.registerMap(overpass);
        
        const ancient = this.createAncient();
        this.registerMap(ancient);
        
        const vertigo = this.createVertigo();
        this.registerMap(vertigo);
        
        const office = this.createOffice();
        this.registerMap(office);
        
        const Italy = this.createItaly();
        this.registerMap(Italy);
        
        const militia = this.createMilitia();
        this.registerMap(militia);
    }
    
    createDust2() {
        const map = new Map('de_dust2', {
            name: 'Dust II',
            type: MAP_TYPE.BOMB_DEFUSAL,
            bounds: {
                min: { x: -2500, y: -2500, z: 0 },
                max: { x: 2500, y: 2500, z: 300 }
            },
            spawns: {
                terrorist: [
                    { x: -1500, y: -500, z: 0 },
                    { x: -1400, y: -600, z: 0 },
                    { x: -1300, y: -500, z: 0 },
                    { x: -1400, y: -400, z: 0 },
                    { x: -1500, y: -600, z: 0 }
                ],
                counterTerrorist: [
                    { x: 1500, y: 500, z: 0 },
                    { x: 1400, y: 600, z: 0 },
                    { x: 1300, y: 500, z: 0 },
                    { x: 1400, y: 400, z: 0 },
                    { x: 1500, y: 600, z: 0 }
                ]
            },
            bombSites: [
                {
                    id: 'A',
                    name: 'Bomb Site A',
                    bounds: {
                        min: { x: -500, y: -500, z: 0 },
                        max: { x: -200, y: -200, z: 100 }
                    },
                    position: { x: -350, y: -350, z: 0 }
                },
                {
                    id: 'B',
                    name: 'Bomb Site B',
                    bounds: {
                        min: { x: 200, y: -500, z: 0 },
                        max: { x: 500, y: -200, z: 100 }
                    },
                    position: { x: 350, y: -350, z: 0 }
                }
            ],
            hostageZones: [],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: -2000, y: -800, z: 0 },
                        max: { x: -1000, y: -200, z: 100 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: 1000, y: 200, z: 0 },
                        max: { x: 2000, y: 800, z: 100 }
                    }
                }
            ]
        });
        
        map.waypoints = this.generateWaypoints(map);
        map.coverSpots = this.generateCoverSpots(map);
        
        return map;
    }
    
    createMirage() {
        return new Map('de_mirage', {
            name: 'Mirage',
            type: MAP_TYPE.BOMB_DEFUSAL,
            bounds: {
                min: { x: -2000, y: -2000, z: 0 },
                max: { x: 2000, y: 2000, z: 400 }
            },
            spawns: {
                terrorist: [
                    { x: -1000, y: 0, z: 0 },
                    { x: -900, y: 100, z: 0 },
                    { x: -800, y: 0, z: 0 },
                    { x: -900, y: -100, z: 0 },
                    { x: -1000, y: 100, z: 0 }
                ],
                counterTerrorist: [
                    { x: 1000, y: 0, z: 0 },
                    { x: 900, y: 100, z: 0 },
                    { x: 800, y: 0, z: 0 },
                    { x: 900, y: -100, z: 0 },
                    { x: 1000, y: 100, z: 0 }
                ]
            },
            bombSites: [
                {
                    id: 'A',
                    name: 'Bomb Site A',
                    bounds: {
                        min: { x: -300, y: -400, z: 100 },
                        max: { x: 0, y: -100, z: 200 }
                    },
                    position: { x: -150, y: -250, z: 150 }
                },
                {
                    id: 'B',
                    name: 'Bomb Site B',
                    bounds: {
                        min: { x: 200, y: 100, z: 0 },
                        max: { x: 400, y: 300, z: 100 }
                    },
                    position: { x: 300, y: 200, z: 50 }
                }
            ],
            hostageZones: [],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: -1500, y: -300, z: 0 },
                        max: { x: -800, y: 300, z: 150 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: 800, y: -300, z: 0 },
                        max: { x: 1500, y: 300, z: 150 }
                    }
                }
            ]
        });
    }
    
    createInferno() {
        return new Map('de_inferno', {
            name: 'Inferno',
            type: MAP_TYPE.BOMB_DEFUSAL,
            bounds: {
                min: { x: -3000, y: -2000, z: 0 },
                max: { x: 0, y: 1000, z: 350 }
            },
            spawns: {
                terrorist: [
                    { x: -1500, y: 0, z: 0 },
                    { x: -1400, y: 100, z: 0 },
                    { x: -1300, y: 0, z: 0 },
                    { x: -1400, y: -100, z: 0 },
                    { x: -1500, y: 100, z: 0 }
                ],
                counterTerrorist: [
                    { x: -500, y: 0, z: 0 },
                    { x: -400, y: 100, z: 0 },
                    { x: -300, y: 0, z: 0 },
                    { x: -400, y: -100, z: 0 },
                    { x: -500, y: 100, z: 0 }
                ]
            },
            bombSites: [
                {
                    id: 'A',
                    name: 'Bomb Site A',
                    bounds: {
                        min: { x: -800, y: -500, z: 0 },
                        max: { x: -500, y: -200, z: 150 }
                    },
                    position: { x: -650, y: -350, z: 50 }
                },
                {
                    id: 'B',
                    name: 'Bomb Site B',
                    bounds: {
                        min: { x: -200, y: 100, z: 0 },
                        max: { x: 100, y: 400, z: 100 }
                    },
                    position: { x: -50, y: 250, z: 50 }
                }
            ],
            hostageZones: [
                {
                    id: 'hostage_1',
                    position: { x: -100, y: 200, z: 0 },
                    bounds: {
                        min: { x: -150, y: 150, z: 0 },
                        max: { x: -50, y: 250, z: 100 }
                    }
                }
            ],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: -2000, y: -500, z: 0 },
                        max: { x: -1200, y: 500, z: 150 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: -800, y: -500, z: 0 },
                        max: { x: 0, y: 500, z: 150 }
                    }
                }
            ]
        });
    }
    
    createNuke() {
        return new Map('de_nuke', {
            name: 'Nuke',
            type: MAP_TYPE.BOMB_DEFUSAL,
            bounds: {
                min: { x: -2500, y: -2500, z: -100 },
                max: { x: 1500, y: 1500, z: 600 }
            },
            spawns: {
                terrorist: [
                    { x: -500, y: -500, z: 0 },
                    { x: -400, y: -400, z: 0 },
                    { x: -300, y: -500, z: 0 },
                    { x: -400, y: -600, z: 0 },
                    { x: -500, y: -400, z: 0 }
                ],
                counterTerrorist: [
                    { x: 500, y: 500, z: 300 },
                    { x: 400, y: 400, z: 300 },
                    { x: 300, y: 500, z: 300 },
                    { x: 400, y: 600, z: 300 },
                    { x: 500, y: 400, z: 300 }
                ]
            },
            bombSites: [
                {
                    id: 'A',
                    name: 'Bomb Site A (Outside)',
                    bounds: {
                        min: { x: -1000, y: -800, z: 0 },
                        max: { x: -500, y: -300, z: 200 }
                    },
                    position: { x: -750, y: -550, z: 100 }
                },
                {
                    id: 'B',
                    name: 'Bomb Site B (Inside)',
                    bounds: {
                        min: { x: -100, y: -100, z: 200 },
                        max: { x: 200, y: 200, z: 400 }
                    },
                    position: { x: 50, y: 50, z: 300 }
                }
            ],
            hostageZones: [],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: -1500, y: -1000, z: 0 },
                        max: { x: -500, y: 0, z: 200 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: 500, y: 0, z: 200 },
                        max: { x: 1500, y: 1000, z: 400 }
                    }
                }
            ]
        });
    }
    
    createOverpass() {
        return new Map('de_overpass', {
            name: 'Overpass',
            type: MAP_TYPE.BOMB_DEFUSAL,
            bounds: {
                min: { x: -2000, y: -1500, z: 0 },
                max: { x: 1000, y: 1500, z: 500 }
            },
            spawns: {
                terrorist: [
                    { x: -800, y: 0, z: 0 },
                    { x: -700, y: 100, z: 0 },
                    { x: -600, y: 0, z: 0 },
                    { x: -700, y: -100, z: 0 },
                    { x: -800, y: 100, z: 0 }
                ],
                counterTerrorist: [
                    { x: 400, y: 0, z: 0 },
                    { x: 300, y: 100, z: 0 },
                    { x: 200, y: 0, z: 0 },
                    { x: 300, y: -100, z: 0 },
                    { x: 400, y: 100, z: 0 }
                ]
            },
            bombSites: [
                {
                    id: 'A',
                    name: 'Bomb Site A',
                    bounds: {
                        min: { x: -300, y: -300, z: 200 },
                        max: { x: 0, y: 0, z: 350 }
                    },
                    position: { x: -150, y: -150, z: 275 }
                },
                {
                    id: 'B',
                    name: 'Bomb Site B',
                    bounds: {
                        min: { x: 200, y: 300, z: 0 },
                        max: { x: 500, y: 600, z: 150 }
                    },
                    position: { x: 350, y: 450, z: 75 }
                }
            ],
            hostageZones: [],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: -1200, y: -500, z: 0 },
                        max: { x: -600, y: 500, z: 150 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: 600, y: -500, z: 0 },
                        max: { x: 1200, y: 500, z: 150 }
                    }
                }
            ]
        });
    }
    
    createAncient() {
        return new Map('de_ancient', {
            name: 'Ancient',
            type: MAP_TYPE.BOMB_DEFUSAL,
            bounds: {
                min: { x: -2000, y: -2000, z: 0 },
                max: { x: 2000, y: 2000, z: 400 }
            },
            spawns: {
                terrorist: [
                    { x: -1000, y: 0, z: 0 },
                    { x: -900, y: 100, z: 0 },
                    { x: -800, y: 0, z: 0 },
                    { x: -900, y: -100, z: 0 },
                    { x: -1000, y: 100, z: 0 }
                ],
                counterTerrorist: [
                    { x: 1000, y: 0, z: 0 },
                    { x: 900, y: 100, z: 0 },
                    { x: 800, y: 0, z: 0 },
                    { x: 900, y: -100, z: 0 },
                    { x: 1000, y: 100, z: 0 }
                ]
            },
            bombSites: [
                {
                    id: 'A',
                    name: 'Bomb Site A',
                    bounds: {
                        min: { x: -400, y: -400, z: 0 },
                        max: { x: -100, y: -100, z: 150 }
                    },
                    position: { x: -250, y: -250, z: 75 }
                },
                {
                    id: 'B',
                    name: 'Bomb Site B',
                    bounds: {
                        min: { x: 100, y: 100, z: 0 },
                        max: { x: 400, y: 400, z: 150 }
                    },
                    position: { x: 250, y: 250, z: 75 }
                }
            ],
            hostageZones: [],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: -1500, y: -500, z: 0 },
                        max: { x: -800, y: 500, z: 150 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: 800, y: -500, z: 0 },
                        max: { x: 1500, y: 500, z: 150 }
                    }
                }
            ]
        });
    }
    
    createVertigo() {
        return new Map('de_vertigo', {
            name: 'Vertigo',
            type: MAP_TYPE.BOMB_DEFUSAL,
            bounds: {
                min: { x: -1500, y: -1500, z: 0 },
                max: { x: 1500, y: 1500, z: 800 }
            },
            spawns: {
                terrorist: [
                    { x: -500, y: 0, z: 0 },
                    { x: -400, y: 100, z: 0 },
                    { x: -300, y: 0, z: 0 },
                    { x: -400, y: -100, z: 0 },
                    { x: -500, y: 100, z: 0 }
                ],
                counterTerrorist: [
                    { x: 500, y: 0, z: 400 },
                    { x: 400, y: 100, z: 400 },
                    { x: 300, y: 0, z: 400 },
                    { x: 400, y: -100, z: 400 },
                    { x: 500, y: 100, z: 400 }
                ]
            },
            bombSites: [
                {
                    id: 'A',
                    name: 'Bomb Site A (Low)',
                    bounds: {
                        min: { x: -400, y: -400, z: 0 },
                        max: { x: -100, y: -100, z: 100 }
                    },
                    position: { x: -250, y: -250, z: 50 }
                },
                {
                    id: 'B',
                    name: 'Bomb Site B (High)',
                    bounds: {
                        min: { x: 100, y: 100, z: 500 },
                        max: { x: 400, y: 400, z: 600 }
                    },
                    position: { x: 250, y: 250, z: 550 }
                }
            ],
            hostageZones: [],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: -1000, y: -500, z: 0 },
                        max: { x: -300, y: 500, z: 200 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: 300, y: -500, z: 300 },
                        max: { x: 1000, y: 500, z: 500 }
                    }
                }
            ]
        });
    }
    
    createOffice() {
        return new Map('cs_office', {
            name: 'Office',
            type: MAP_TYPE.HOSTAGE_RESCUE,
            bounds: {
                min: { x: -1000, y: -1000, z: 0 },
                max: { x: 1000, y: 1000, z: 300 }
            },
            spawns: {
                terrorist: [
                    { x: 200, y: 200, z: 0 },
                    { x: 300, y: 200, z: 0 },
                    { x: 200, y: 300, z: 0 },
                    { x: 300, y: 300, z: 0 },
                    { x: 250, y: 250, z: 0 }
                ],
                counterTerrorist: [
                    { x: -200, y: -200, z: 0 },
                    { x: -300, y: -200, z: 0 },
                    { x: -200, y: -300, z: 0 },
                    { x: -300, y: -300, z: 0 },
                    { x: -250, y: -250, z: 0 }
                ]
            },
            bombSites: [],
            hostageZones: [
                {
                    id: 'hostage_1',
                    position: { x: 100, y: 100, z: 0 },
                    bounds: {
                        min: { x: 50, y: 50, z: 0 },
                        max: { x: 150, y: 150, z: 100 }
                    }
                }
            ],
            rescueZones: [
                {
                    id: 'rescue_1',
                    bounds: {
                        min: { x: -150, y: -150, z: 0 },
                        max: { x: -50, y: -50, z: 100 }
                    }
                }
            ],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: 0, y: 0, z: 0 },
                        max: { x: 500, y: 500, z: 150 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: -500, y: -500, z: 0 },
                        max: { x: 0, y: 0, z: 150 }
                    }
                }
            ]
        });
    }
    
    createItaly() {
        return new Map('cs_italy', {
            name: 'Italy',
            type: MAP_TYPE.HOSTAGE_RESCUE,
            bounds: {
                min: { x: -2000, y: -2000, z: 0 },
                max: { x: 2000, y: 2000, z: 300 }
            },
            spawns: {
                terrorist: [
                    { x: 500, y: 0, z: 0 },
                    { x: 600, y: 100, z: 0 },
                    { x: 400, y: 0, z: 0 },
                    { x: 600, y: -100, z: 0 },
                    { x: 500, y: 100, z: 0 }
                ],
                counterTerrorist: [
                    { x: -500, y: 0, z: 0 },
                    { x: -600, y: 100, z: 0 },
                    { x: -400, y: 0, z: 0 },
                    { x: -600, y: -100, z: 0 },
                    { x: -500, y: 100, z: 0 }
                ]
            },
            bombSites: [],
            hostageZones: [
                {
                    id: 'hostage_1',
                    position: { x: 0, y: 0, z: 0 },
                    bounds: {
                        min: { x: -50, y: -50, z: 0 },
                        max: { x: 50, y: 50, z: 100 }
                    }
                }
            ],
            rescueZones: [
                {
                    id: 'rescue_1',
                    bounds: {
                        min: { x: -550, y: -50, z: 0 },
                        max: { x: -450, y: 50, z: 100 }
                    }
                }
            ],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: 200, y: -200, z: 0 },
                        max: { x: 800, y: 200, z: 150 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: -800, y: -200, z: 0 },
                        max: { x: -200, y: 200, z: 150 }
                    }
                }
            ]
        });
    }
    
    createMilitia() {
        return new Map('cs_militia', {
            name: 'Militia',
            type: MAP_TYPE.HOSTAGE_RESCUE,
            bounds: {
                min: { x: -1500, y: -1500, z: 0 },
                max: { x: 1500, y: 1500, z: 300 }
            },
            spawns: {
                terrorist: [
                    { x: 300, y: 0, z: 0 },
                    { x: 400, y: 100, z: 0 },
                    { x: 200, y: 0, z: 0 },
                    { x: 400, y: -100, z: 0 },
                    { x: 300, y: 100, z: 0 }
                ],
                counterTerrorist: [
                    { x: -300, y: 0, z: 0 },
                    { x: -400, y: 100, z: 0 },
                    { x: -200, y: 0, z: 0 },
                    { x: -400, y: -100, z: 0 },
                    { x: -300, y: 100, z: 0 }
                ]
            },
            bombSites: [],
            hostageZones: [
                {
                    id: 'hostage_1',
                    position: { x: 0, y: 0, z: 0 },
                    bounds: {
                        min: { x: -50, y: -50, z: 0 },
                        max: { x: 50, y: 50, z: 100 }
                    }
                }
            ],
            rescueZones: [
                {
                    id: 'rescue_1',
                    bounds: {
                        min: { x: -350, y: -50, z: 0 },
                        max: { x: -250, y: 50, z: 100 }
                    }
                }
            ],
            buyZones: [
                {
                    id: 'terrorist_buy',
                    team: 'terrorist',
                    bounds: {
                        min: { x: 0, y: -300, z: 0 },
                        max: { x: 600, y: 300, z: 150 }
                    }
                },
                {
                    id: 'ct_buy',
                    team: 'counterTerrorist',
                    bounds: {
                        min: { x: -600, y: -300, z: 0 },
                        max: { x: 0, y: 300, z: 150 }
                    }
                }
            ]
        });
    }
    
    generateWaypoints(map) {
        const waypoints = [];
        
        const gridSize = 200;
        
        for (let x = map.bounds.min.x; x <= map.bounds.max.x; x += gridSize) {
            for (let y = map.bounds.min.y; y <= map.bounds.max.y; y += gridSize) {
                waypoints.push({
                    id: `wp_${x}_${y}`,
                    position: { x, y, z: map.bounds.min.z },
                    connections: [],
                    type: 'walk',
                    cost: 1
                });
            }
        }
        
        return waypoints;
    }
    
    generateCoverSpots(map) {
        const coverSpots = [];
        
        const corners = [
            { x: map.bounds.min.x, y: map.bounds.min.y },
            { x: map.bounds.max.x, y: map.bounds.min.y },
            { x: map.bounds.min.x, y: map.bounds.max.y },
            { x: map.bounds.max.x, y: map.bounds.max.y }
        ];
        
        for (const corner of corners) {
            coverSpots.push({
                id: `cover_${coverSpots.length}`,
                position: { x: corner.x, y: corner.y, z: map.bounds.min.z },
                type: 'corner',
                effectiveness: 0.8
            });
        }
        
        return coverSpots;
    }
}

export class Waypoint {
    constructor(id, position, connections = []) {
        this.id = id;
        this.position = position;
        this.connections = connections;
        this.type = 'walk';
        this.cost = 1;
        this.height = 0;
        this.jumpRequired = false;
    }
    
    connectTo(waypoint, bidirectional = true) {
        if (!this.connections.includes(waypoint.id)) {
            this.connections.push(waypoint.id);
        }
        
        if (bidirectional) {
            waypoint.connectTo(this, false);
        }
    }
    
    disconnectFrom(waypointId) {
        const index = this.connections.indexOf(waypointId);
        if (index > -1) {
            this.connections.splice(index, 1);
        }
    }
}

export class NavigationMesh {
    constructor() {
        this.nodes = [];
        this.edges = [];
        this.polygons = [];
    }
    
    addNode(node) {
        this.nodes.push(node);
    }
    
    addEdge(from, to) {
        this.edges.push({ from, to });
    }
    
    findPath(start, end) {
        return [start, end];
    }
    
    getNearestNode(position) {
        let nearest = null;
        let nearestDistance = Infinity;
        
        for (const node of this.nodes) {
            const dx = position.x - node.position.x;
            const dy = position.y - node.position.y;
            const dz = (position.z || 0) - (node.position.z || 0);
            const distance = Math.sqrt(dx * dx + dy * dy + dz * dz);
            
            if (distance < nearestDistance) {
                nearestDistance = distance;
                nearest = node;
            }
        }
        
        return nearest;
    }
    
    isWalkable(position) {
        return true;
    }
}

export class CollisionMesh {
    constructor() {
        this.triangles = [];
        this.boxes = [];
        this.spheres = [];
    }
    
    addTriangle(p1, p2, p3, material = 'concrete') {
        this.triangles.push({
            p1, p2, p3,
            material,
            normal: this.calculateNormal(p1, p2, p3)
        });
    }
    
    addBox(min, max, material = 'concrete') {
        this.boxes.push({ min, max, material });
    }
    
    addSphere(center, radius, material = 'concrete') {
        this.spheres.push({ center, radius, material });
    }
    
    calculateNormal(p1, p2, p3) {
        const u = { x: p2.x - p1.x, y: p2.y - p1.y, z: p2.z - p1.z };
        const v = { x: p3.x - p1.x, y: p3.y - p1.y, z: p3.z - p1.z };
        
        return {
            x: u.y * v.z - u.z * v.y,
            y: u.z * v.x - u.x * v.z,
            z: u.x * v.y - u.y * v.x
        };
    }
    
    raycast(origin, direction, maxDistance = 1000) {
        let closest = null;
        let closestDistance = maxDistance;
        
        for (const triangle of this.triangles) {
            const hit = this.raycastTriangle(origin, direction, triangle);
            if (hit && hit.distance < closestDistance) {
                closestDistance = hit.distance;
                closest = hit;
            }
        }
        
        for (const box of this.boxes) {
            const hit = this.raycastBox(origin, direction, box);
            if (hit && hit.distance < closestDistance) {
                closestDistance = hit.distance;
                closest = hit;
            }
        }
        
        return closest;
    }
    
    raycastTriangle(origin, direction, triangle) {
        return null;
    }
    
    raycastBox(origin, direction, box) {
        return null;
    }
}

export class MapGeometry {
    constructor() {
        this.walls = [];
        this.floors = [];
        this.ceilings = [];
        this.doors = [];
        this.ladders = [];
        this.elevators = [];
        this.platforms = [];
    }
    
    addWall(start, end, height, material = 'concrete') {
        this.walls.push({
            start: { ...start },
            end: { ...end },
            height: height,
            material: material
        });
    }
    
    addFloor(position, width, depth, material = 'concrete') {
        this.floors.push({
            position: { ...position },
            width: width,
            depth: depth,
            material: material
        });
    }
    
    addCeiling(position, width, depth, material = 'concrete') {
        this.ceilings.push({
            position: { ...position },
            width: width,
            depth: depth,
            height: 50,
            material: material
        });
    }
    
    addDoor(position, width, height, open = false) {
        this.doors.push({
            position: { ...position },
            width: width,
            height: height,
            open: open,
            openAngle: 0
        });
    }
    
    addLadder(position, height, width = 50) {
        this.ladders.push({
            position: { ...position },
            height: height,
            width: width
        });
    }
    
    addElevator(position, minHeight, maxHeight) {
        this.elevators.push({
            position: { ...position },
            minHeight: minHeight,
            maxHeight: maxHeight,
            currentHeight: minHeight,
            moving: false
        });
    }
}

export class MapProp {
    constructor(id, type, position, rotation, scale, model) {
        this.id = id;
        this.type = type;
        this.position = position;
        this.rotation = rotation || { x: 0, y: 0, z: 0 };
        this.scale = scale || { x: 1, y: 1, z: 1 };
        this.model = model;
        this.visible = true;
        this.solid = false;
        this.destructible = false;
        this.health = 100;
    }
    
    damage(amount) {
        if (!this.destructible) return;
        
        this.health -= amount;
        
        if (this.health <= 0) {
            this.destroy();
        }
    }
    
    destroy() {
        this.visible = false;
        this.solid = false;
    }
}

export class MapLighting {
    constructor() {
        this.ambientLight = { r: 50, g: 50, b: 50 };
        this.directionalLight = {
            direction: { x: 0.5, y: 0.5, z: -1 },
            color: { r: 255, g: 255, b: 255 },
            intensity: 1.0
        };
        this.pointLights = [];
        this.spotLights = [];
        this.shadows = true;
        this.shadowMapSize = 2048;
    }
    
    addPointLight(position, color, intensity, radius) {
        this.pointLights.push({
            position: position,
            color: color,
            intensity: intensity,
            radius: radius
        });
    }
    
    addSpotLight(position, direction, color, intensity, angle, radius) {
        this.spotLights.push({
            position: position,
            direction: direction,
            color: color,
            intensity: intensity,
            angle: angle,
            radius: radius
        });
    }
    
    removeLight(lightId) {
        const pointIndex = this.pointLights.findIndex(l => l.id === lightId);
        if (pointIndex > -1) {
            this.pointLights.splice(pointIndex, 1);
            return true;
        }
        
        const spotIndex = this.spotLights.findIndex(l => l.id === lightId);
        if (spotIndex > -1) {
            this.spotLights.splice(spotIndex, 1);
            return true;
        }
        
        return false;
    }
}

export class MapAudio {
    constructor() {
        this.ambientSounds = [];
        this.triggerSounds = [];
        this.areaSounds = [];
    }
    
    addAmbientSound(soundId, position, radius) {
        this.ambientSounds.push({
            soundId: soundId,
            position: position,
            radius: radius
        });
    }
    
    addTriggerSound(soundId, triggerBounds) {
        this.triggerSounds.push({
            soundId: soundId,
            bounds: triggerBounds
        });
    }
    
    addAreaSound(soundId, areaBounds, probability = 1.0) {
        this.areaSounds.push({
            soundId: soundId,
            bounds: areaBounds,
            probability: probability
        });
    }
}

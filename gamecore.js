export class EventSystem {
    constructor() {
        this.events = new Map();
        this.listeners = new Map();
        this.eventQueue = [];
        this.maxQueueSize = 1000;
        this.isProcessing = false;
        this.eventHistory = new Map();
        this.maxHistorySize = 10000;
    }

    registerEvent(eventName, schema = null) {
        this.events.set(eventName, {
            name: eventName,
            schema,
            registeredAt: Date.now(),
            emitCount: 0
        });
    }

    on(eventName, callback, context = null) {
        if (!this.listeners.has(eventName)) {
            this.listeners.set(eventName, []);
        }

        const listener = {
            id: EventSystem.nextListenerId++,
            callback,
            context,
            createdAt: Date.now(),
            once: false
        };

        this.listeners.get(eventName).push(listener);
        return listener.id;
    }

    once(eventName, callback, context = null) {
        const id = this.on(eventName, callback, context);
        const listener = this.getListener(eventName, id);
        if (listener) {
            listener.once = true;
        }
        return id;
    }

    off(eventName, listenerId = null) {
        if (listenerId === null) {
            this.listeners.delete(eventName);
            return true;
        }

        const listeners = this.listeners.get(eventName);
        if (!listeners) return false;

        const index = listeners.findIndex(l => l.id === listenerId);
        if (index !== -1) {
            listeners.splice(index, 1);
            return true;
        }
        return false;
    }

    getListener(eventName, listenerId) {
        const listeners = this.listeners.get(eventName);
        if (!listeners) return null;
        return listeners.find(l => l.id === listenerId);
    }

    emit(eventName, data = null) {
        const event = this.events.get(eventName);
        if (event) {
            event.emitCount++;
        }

        const listeners = this.listeners.get(eventName);
        if (!listeners || listeners.length === 0) return false;

        const eventData = {
            name: eventName,
            data,
            timestamp: Date.now(),
            id: EventSystem.nextEventId++
        };

        this.eventHistory.set(eventData.id, eventData);
        if (this.eventHistory.size > this.maxHistorySize) {
            const firstKey = this.eventHistory.keys().next().value;
            this.eventHistory.delete(firstKey);
        }

        const results = [];
        for (const listener of listeners) {
            try {
                if (listener.context) {
                    results.push(listener.callback.call(listener.context, data));
                } else {
                    results.push(listener.callback(data));
                }

                if (listener.once) {
                    this.off(eventName, listener.id);
                }
            } catch (error) {
                console.error(`Error in event listener for ${eventName}:`, error);
                results.push({ error });
            }
        }

        return results;
    }

    emitAsync(eventName, data = null) {
        return new Promise((resolve, reject) => {
            const eventData = {
                name: eventName,
                data,
                timestamp: Date.now(),
                id: EventSystem.nextEventId++
            };

            this.eventHistory.set(eventData.id, eventData);
            const listeners = this.listeners.get(eventName);

            if (!listeners || listeners.length === 0) {
                resolve([]);
                return;
            }

            const results = [];
            let completed = 0;

            for (const listener of listeners) {
                try {
                    const result = listener.context
                        ? listener.callback.call(listener.context, data)
                        : listener.callback(data);

                    if (result instanceof Promise) {
                        result
                            .then(r => {
                                results.push(r);
                                completed++;
                                if (listener.once) {
                                    this.off(eventName, listener.id);
                                }
                                if (completed === listeners.length) {
                                    resolve(results);
                                }
                            })
                            .catch(err => {
                                results.push({ error: err });
                                completed++;
                                if (completed === listeners.length) {
                                    resolve(results);
                                }
                            });
                    } else {
                        results.push(result);
                        completed++;
                        if (listener.once) {
                            this.off(eventName, listener.id);
                        }
                    }
                } catch (error) {
                    results.push({ error });
                    completed++;
                }
            }

            if (completed === listeners.length) {
                resolve(results);
            }
        });
    }

    queue(eventName, data = null) {
        if (this.eventQueue.length >= this.maxQueueSize) {
            this.eventQueue.shift();
        }

        this.eventQueue.push({
            name: eventName,
            data,
            queuedAt: Date.now()
        });
    }

    processQueue() {
        if (this.isProcessing) return;
        this.isProcessing = true;

        while (this.eventQueue.length > 0) {
            const item = this.eventQueue.shift();
            this.emit(item.name, item.data);
        }

        this.isProcessing = false;
    }

    clearQueue() {
        this.eventQueue = [];
    }

    getListenerCount(eventName) {
        const listeners = this.listeners.get(eventName);
        return listeners ? listeners.length : 0;
    }

    getEventHistory(eventName = null, limit = 100) {
        const history = Array.from(this.eventHistory.values());

        if (eventName) {
            return history
                .filter(e => e.name === eventName)
                .slice(-limit);
        }

        return history.slice(-limit);
    }

    getEventStats(eventName = null) {
        if (eventName) {
            const event = this.events.get(eventName);
            return {
                name: eventName,
                emitCount: event?.emitCount || 0,
                listenerCount: this.getListenerCount(eventName)
            };
        }

        const stats = [];
        for (const [name, event] of this.events) {
            stats.push({
                name,
                emitCount: event.emitCount,
                listenerCount: this.getListenerCount(name)
            });
        }

        return stats;
    }

    removeAllListeners() {
        this.listeners.clear();
    }

    serialize() {
        return {
            registeredEvents: this.events.size,
            listenerCount: Array.from(this.listeners.values()).reduce((sum, arr) => sum + arr.length, 0),
            queueSize: this.eventQueue.length,
            historySize: this.eventHistory.size
        };
    }
}
EventSystem.nextListenerId = 0;
EventSystem.nextEventId = 0;

export class GameEventEmitter extends EventSystem {
    constructor() {
        super();
        this.registerGameEvents();
    }

    registerGameEvents() {
        this.registerEvent('player.spawn');
        this.registerEvent('player.death');
        this.registerEvent('player.damage');
        this.registerEvent('player.footstep');
        this.registerEvent('player.jump');
        this.registerEvent('player.land');
        this.registerEvent('player.chat');
        this.registerEvent('player.connect');
        this.registerEvent('player.disconnect');
        this.registerEvent('player.team_change');
        this.registerEvent('player.buy');
        this.registerEvent('player.use');

        this.registerEvent('weapon.fire');
        this.registerEvent('weapon.reload');
        this.registerEvent('weapon.switch');
        this.registerEvent('weapon.drop');
        this.registerEvent('weapon.pickup');

        this.registerEvent('grenade.thrown');
        this.registerEvent('grenade.explode');
        this.registerEvent('grenade.detonate');
        this.registerEvent('flashbang.explode');
        this.registerEvent('smoke.start');
        this.registerEvent('smoke.end');
        this.registerEvent('molotov.explode');

        this.registerEvent('bomb.plant');
        this.registerEvent('bomb.defuse_start');
        this.registerEvent('bomb.defuse_stop');
        this.registerEvent('bomb.defused');
        this.registerEvent('bomb.explode');

        this.registerEvent('round.start');
        this.registerEvent('round.end');
        this.registerEvent('round.money');
        this.registerEvent('round.buytime_end');
        this.registerEvent('round.freeze_end');

        this.registerEvent('match.start');
        this.registerEvent('match.end');
        this.registerEvent('match.pause');
        this.registerEvent('match.resume');
        this.registerEvent('match.interrupt');

        this.registerEvent('vote.start');
        this.registerEvent('vote.cast');
        this.registerEvent('vote.end');

        this.registerEvent('hostage.pickup');
        this.registerEvent('hostage.drop');
        this.registerEvent('hostage.rescue');

        this.registerEvent('entity.create');
        this.registerEvent('entity.remove');
        this.registerEvent('entity.update');

        this.registerEvent('server.start');
        this.registerEvent('server.shutdown');
        this.registerEvent('server.tick');
    }

    emitPlayerSpawn(playerId, team, position) {
        return this.emit('player.spawn', { playerId, team, position });
    }

    emitPlayerDeath(playerId, killerId, weapon, headshot) {
        return this.emit('player.death', { playerId, killerId, weapon, headshot });
    }

    emitPlayerDamage(playerId, attackerId, damage, weapon, hitgroup) {
        return this.emit('player.damage', { playerId, attackerId, damage, weapon, hitgroup });
    }

    emitWeaponFire(playerId, weaponId, position, spread) {
        return this.emit('weapon.fire', { playerId, weaponId, position, spread });
    }

    emitBombPlant(playerId, site) {
        return this.emit('bomb.plant', { playerId, site });
    }

    emitBombDefuse(playerId, hasKit) {
        return this.emit('bomb.defuse_start', { playerId, hasKit });
    }

    emitBombDefused(playerId, hasKit) {
        return this.emit('bomb.defused', { playerId, hasKit });
    }

    emitBombExplode(playerId, site) {
        return this.emit('bomb.explode', { playerId, site });
    }

    emitRoundStart(roundNumber, timeLimit) {
        return this.emit('round.start', { roundNumber, timeLimit });
    }

    emitRoundEnd(winner, reason, scores) {
        return this.emit('round.end', { winner, reason, scores });
    }

    emitMatchEnd(winner, scores, duration) {
        return this.emit('match.end', { winner, scores, duration });
    }

    emitGrenadeThrow(playerId, grenadeType, position, velocity) {
        return this.emit('grenade.thrown', { playerId, grenadeType, position, velocity });
    }

    emitGrenadeExplode(playerId, grenadeType, position, radius) {
        return this.emit('grenade.explode', { playerId, grenadeType, position, radius });
    }
}

export class ConfigManager {
    constructor() {
        this.configs = new Map();
        this.activeConfig = null;
        this.defaultConfig = this.createDefaultConfig();
        this.loadOrder = [];
    }

    createDefaultConfig() {
        return {
            name: 'default',
            version: '1.0',
            game: {
                hostname: 'CS2 Server',
                maxplayers: 10,
                map: 'de_dust2',
                gamemode: 'competitive',
                tickrate: 128,
                rcon_password: '',
                password: '',
                sv_tags: []
            },
            server: {
                host: '0.0.0.0',
                port: 27015,
                tv_port: 27020,
                tv_enabled: false,
                tv_delay: 30,
                tv_maxclients: 100
            },
            network: {
                sv_maxrate: 0,
                sv_minrate: 0,
                sv_uploadrate: 0,
                net_chunked: 1,
                net_compresspackets: 1,
                net_compressvoice: 1
            },
            gameplay: {
                mp_friendlyfire: 0,
                mp_teammates_are_enemies: 0,
                mp_roundtime: 115,
                mp_roundtime_defuse: 115,
                mp_roundtime_hostage: 115,
                mp_maxrounds: 30,
                mp_timelimit: 0,
                mp_freezetime: 15,
                mp_startmoney: 800,
                mp_buytime: 20,
                mp_buy_anywhere: 0,
                mp_buy_during_plant: 0,
                mp_respawn_on_death: 0,
                mp_respawn_allow_observer: 1,
                mp_overtime_enable: 1,
                mp_overtime_maxrounds: 6,
                mp_overtime_timefactor: 0,
                mp_hostagepenalty: 10,
                mp_plant_c4_anywhere: 0,
                mp_defuser_allocation: 0,
                mp_solid_teammates: 1,
                sv_holiday_mode: 0
            },
            weapons: {
                weapon_recoil_scale: 1.0,
                weapon_accuracy_npc: 0,
                weapon_air_control: 1,
                weapon_muzzleflash_enabled: 1,
                sv_ignore_weapon_pickup_rule: 0,
                ammo_grenade_limit_total: 4,
                ammo_grenade_limit_flash: 2,
                ammo_grenade_limit_smoke: 2,
                ammo_grenade_limit_molotov: 1,
                ammo_grenade_limit_decoy: 2
            },
            bots: {
                bot_quota: 0,
                bot_quota_mode: 'normal',
                bot_difficulty: 1,
                bot_defer_to_pathfinding: 1,
                bot_allow_rogues: 1,
                bot_allow_pistols_smg_only: 0,
                bot_chatter: 'normal',
                bot_deathmatch: 0,
                bot_join_after_player: 1,
                bot_join_team: 'any',
                bot_prefix': '',
                bot_name_prefix: '',
                bot_eco_threshold: 2500
            },
            vote: {
                sv_allow_votes: 1,
                sv_vote_allow_scenarios: 1,
                sv_vote_allow_music_kits: 1,
                vote_kick': 1,
                vote_ban: 1,
                vote_changelevel: 1,
                vote_scenarios_weights_granularity: 5,
                vote_scenarios_keep_presets: 1
            },
            spectators: {
                allow_spectators: 1,
                spec_freeze_time': 3,
                spec_freeze_panel_x: -1,
                spec_freeze_panel_y: 0.5,
                spec_freeze_target_x: 0,
                spec_freeze_target_y: 0,
                spec_labels_allow_on_spectate': 0
            },
            moderation: {
                sv_rcon: 1,
                sv_rcon_banpenalty: 14400,
                sv_rcon_maxfailures: 10,
                sv_rcon_minfailuretime: 30,
                sv_rcon_minfailures_to_ban: 5,
                sv_pausable: 0,
                sv_auto_crouch': 1,
                sv_show_radio_icons: 1,
                sv_deadtalk: 0,
                sv_alltalk: 0,
                sv_voiceenable: 1,
                sv_full_alltalk: 0,
                sv_talk_enemies_living: 1,
                sv_talk_enemies_dead: 0,
                sv_deadtalk_team: 0,
                sv_ignore_nav: 0,
                sv_footsteps: 1,
                sv_clip_penetration_entities': 1
            },
            physics: {
                sv_gravity: 750,
                sv_hl2mp_item_respawn_time: 30,
                sv_accelerate: 5.5,
                sv_airaccelerate': 12,
                sv_wateraccelerate: 10,
                sv_friction: 5.2,
                sv_stopspeed': 80,
                sv_maxvelocity': 3500,
                sv_bounce: 0.9,
                sv_min_jump_lethal_damage: 10,
                sv_jump_spawn_danger_speed': 500,
                sv_jump_spawn_look_time': 0.7,
                sv_jump_spawn_look_at_position': '0 0 0'
            },
            maps: {
                maps_in_vote: 'de_dust2', 'de_mirage', 'de_inferno', 'de_nuke', 'de_overpass', 'de_train', 'de_vertigo', 'de_ancient',
                nextlevel: '',
                levelname: '',
                sky_name: 'sky_cs15_daytime01'
            },
            logs: {
                log on: '',
                sv_log_onefile': 0,
                sv_logfile: 1,
                sv_logflush: 0,
                sv_logsdir: 'logs'
            }
        };
    }

    createConfig(name, baseConfig = null) {
        const config = baseConfig
            ? JSON.parse(JSON.stringify(baseConfig))
            : JSON.parse(JSON.stringify(this.defaultConfig));

        config.name = name;
        this.configs.set(name, config);
        return config;
    }

    getConfig(name) {
        return this.configs.get(name) || this.defaultConfig;
    }

    setActiveConfig(name) {
        if (this.configs.has(name) || name === 'default') {
            this.activeConfig = name;
            return true;
        }
        return false;
    }

    getActiveConfig() {
        if (this.activeConfig) {
            return this.getConfig(this.activeConfig);
        }
        return this.defaultConfig;
    }

    setValue(path, value) {
        const config = this.getActiveConfig();
        const keys = path.split('.');
        let current = config;

        for (let i = 0; i < keys.length - 1; i++) {
            if (!(keys[i] in current)) {
                current[keys[i]] = {};
            }
            current = current[keys[i]];
        }

        current[keys[keys.length - 1]] = value;
    }

    getValue(path, defaultValue = null) {
        const config = this.getActiveConfig();
        const keys = path.split('.');
        let current = config;

        for (const key of keys) {
            if (!(key in current)) {
                return defaultValue;
            }
            current = current[key];
        }

        return current;
    }

    loadFromFile(filename) {
        return true;
    }

    saveToFile(filename) {
        return true;
    }

    resetToDefault() {
        this.activeConfig = 'default';
        return this.defaultConfig;
    }

    mergeConfigs(target, source) {
        for (const key of Object.keys(source)) {
            if (typeof source[key] === 'object' && !Array.isArray(source[key])) {
                if (!target[key]) {
                    target[key] = {};
                }
                this.mergeConfigs(target[key], source[key]);
            } else {
                target[key] = source[key];
            }
        }
    }

    exportConfig() {
        return JSON.stringify(this.getActiveConfig(), null, 2);
    }

    importConfig(json) {
        try {
            const config = JSON.parse(json);
            if (config.name) {
                this.configs.set(config.name, config);
                return config;
            }
        } catch (e) {
            console.error('Failed to import config:', e);
        }
        return null;
    }
}

export class PerformanceMonitor {
    constructor() {
        this.metrics = new Map();
        this.frameTimes = [];
        this.maxFrameTimeHistory = 600;
        this.tickRates = [];
        this.maxTickRateHistory = 100;
        this.memoryUsage = [];
        this.lastGC = 0;
        this.startTime = Date.now();
        this.frameCount = 0;
        this.tickCount = 0;
        this.fps = 0;
        this.mspf = 0;
        this.uptime = 0;
    }

    beginFrame() {
        this.frameStart = performance.now();
    }

    endFrame() {
        const frameTime = performance.now() - this.frameStart;
        this.frameTimes.push(frameTime);

        if (this.frameTimes.length > this.maxFrameTimeHistory) {
            this.frameTimes.shift();
        }

        this.frameCount++;
        this.uptime = Date.now() - this.startTime;

        const recentFrames = this.frameTimes.slice(-60);
        const avgFrameTime = recentFrames.reduce((a, b) => a + b, 0) / recentFrames.length;
        this.fps = 1000 / avgFrameTime;
        this.mspf = avgFrameTime;

        return frameTime;
    }

    recordTick(duration) {
        this.tickRates.push(duration);
        if (this.tickRates.length > this.maxTickRateHistory) {
            this.tickRates.shift();
        }
        this.tickCount++;
    }

    recordMetric(name, value) {
        if (!this.metrics.has(name)) {
            this.metrics.set(name, []);
        }
        const values = this.metrics.get(name);
        values.push({ value, timestamp: Date.now() });

        if (values.length > 100) {
            values.shift();
        }
    }

    getMetric(name) {
        return this.metrics.get(name) || [];
    }

    getAverageMetric(name) {
        const values = this.metrics.get(name);
        if (!values || values.length === 0) return 0;
        return values.reduce((a, b) => a + b.value, 0) / values.length;
    }

    getFPS() {
        return Math.round(this.fps);
    }

    getMSPF() {
        return Math.round(this.mspf * 100) / 100;
    }

    getUptime() {
        return this.uptime;
    }

    getTickRate() {
        const recent = this.tickRates.slice(-10);
        if (recent.length === 0) return 0;
        return 1000 / (recent.reduce((a, b) => a + b, 0) / recent.length);
    }

    getFrameTime() {
        return this.frameTimes[this.frameTimes.length - 1] || 0;
    }

    getAverageFrameTime() {
        if (this.frameTimes.length === 0) return 0;
        return this.frameTimes.reduce((a, b) => a + b, 0) / this.frameTimes.length;
    }

    getMinFrameTime() {
        if (this.frameTimes.length === 0) return 0;
        return Math.min(...this.frameTimes);
    }

    getMaxFrameTime() {
        if (this.frameTimes.length === 0) return 0;
        return Math.max(...this.frameTimes);
    }

    get1PercentLow() {
        if (this.frameTimes.length === 0) return 0;
        const sorted = [...this.frameTimes].sort((a, b) => a - b);
        const index = Math.floor(sorted.length * 0.01);
        return sorted[index];
    }

    get99PercentHigh() {
        if (this.frameTimes.length === 0) return 0;
        const sorted = [...this.frameTimes].sort((a, b) => a - b);
        const index = Math.floor(sorted.length * 0.99);
        return sorted[index];
    }

    checkMemory() {
        if (performance.memory) {
            this.memoryUsage.push({
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit,
                timestamp: Date.now()
            });

            if (this.memoryUsage.length > 60) {
                this.memoryUsage.shift();
            }
        }
    }

    getMemoryUsage() {
        if (performance.memory) {
            return {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit,
                percentage: (performance.memory.usedJSHeapSize / performance.memory.jsHeapSizeLimit) * 100
            };
        }
        return null;
    }

    getNetworkStats() {
        return {
            packetsIn: 0,
            packetsOut: 0,
            bytesIn: 0,
            bytesOut: 0,
            packetsLost: 0,
            latency: 0
        };
    }

    getReport() {
        return {
            fps: this.getFPS(),
            mspf: this.getMSPF(),
            frameTime: this.getFrameTime(),
            avgFrameTime: this.getAverageFrameTime(),
            minFrameTime: this.getMinFrameTime(),
            maxFrameTime: this.getMaxFrameTime(),
            percentile1Low: this.get1PercentLow(),
            percentile99High: this.get99PercentHigh(),
            tickRate: this.getTickRate(),
            uptime: this.uptime,
            frameCount: this.frameCount,
            tickCount: this.tickCount,
            memory: this.getMemoryUsage(),
            network: this.getNetworkStats()
        };
    }

    reset() {
        this.frameTimes = [];
        this.tickRates = [];
        this.metrics.clear();
        this.memoryUsage = [];
        this.frameCount = 0;
        this.tickCount = 0;
        this.startTime = Date.now();
    }

    serialize() {
        return {
            fps: this.getFPS(),
            mspf: this.getMSPF(),
            frameTime: this.getFrameTime(),
            uptime: this.uptime,
            frameCount: this.frameCount,
            tickCount: this.tickCount,
            memory: this.getMemoryUsage()
        };
    }
}

export class SecurityManager {
    constructor() {
        this.bannedUsers = new Map();
        this.mutedUsers = new Map();
        this.ipBans = new Set();
        this.hardwareBans = new Set();
        this.trustedUsers = new Set();
        this.reportHistory = new Map();
        this.maxReportsPerUser = 100;
        this.autobanThreshold = 10;
    }

    banUser(userId, adminId, reason, duration = 0) {
        const ban = {
            userId,
            adminId,
            reason,
            createdAt: Date.now(),
            expiresAt: duration > 0 ? Date.now() + duration : 0,
            active: true,
            type: 'user'
        };
        this.bannedUsers.set(userId, ban);
        return ban;
    }

    unbanUser(userId, adminId, reason = '') {
        const ban = this.bannedUsers.get(userId);
        if (ban) {
            ban.active = false;
            ban.unbannedBy = adminId;
            ban.unbannedReason = reason;
            ban.unbannedAt = Date.now();
            return true;
        }
        return false;
    }

    isBanned(userId) {
        const ban = this.bannedUsers.get(userId);
        if (!ban || !ban.active) return false;

        if (ban.expiresAt > 0 && Date.now() > ban.expiresAt) {
            ban.active = false;
            return false;
        }

        return true;
    }

    getBanInfo(userId) {
        return this.bannedUsers.get(userId);
    }

    muteUser(userId, adminId, reason, duration = 0) {
        const mute = {
            userId,
            adminId,
            reason,
            createdAt: Date.now(),
            expiresAt: duration > 0 ? Date.now() + duration : 0,
            active: true
        };
        this.mutedUsers.set(userId, mute);
        return mute;
    }

    unmuteUser(userId) {
        const mute = this.mutedUsers.get(userId);
        if (mute) {
            mute.active = false;
            return true;
        }
        return false;
    }

    isMuted(userId) {
        const mute = this.mutedUsers.get(userId);
        if (!mute || !mute.active) return false;

        if (mute.expiresAt > 0 && Date.now() > mute.expiresAt) {
            mute.active = false;
            return false;
        }

        return true;
    }

    banIP(ip, adminId, reason, duration = 0) {
        const ban = {
            ip,
            adminId,
            reason,
            createdAt: Date.now(),
            expiresAt: duration > 0 ? Date.now() + duration : 0,
            active: true,
            type: 'ip'
        };
        this.ipBans.set(ip, ban);
        return ban;
    }

    unbanIP(ip) {
        return this.ipBans.delete(ip);
    }

    isIPBanned(ip) {
        return this.ipBans.has(ip);
    }

    banHardware(hardwareId, adminId, reason) {
        const ban = {
            hardwareId,
            adminId,
            reason,
            createdAt: Date.now(),
            active: true,
            type: 'hardware'
        };
        this.hardwareBans.set(hardwareId, ban);
        return ban;
    }

    isHardwareBanned(hardwareId) {
        return this.hardwareBans.has(hardwareId);
    }

    trustUser(userId) {
        this.trustedUsers.add(userId);
    }

    untrustUser(userId) {
        return this.trustedUsers.delete(userId);
    }

    isTrusted(userId) {
        return this.trustedUsers.has(userId);
    }

    recordReport(reporterId, targetId, reason, details) {
        if (!this.reportHistory.has(reporterId)) {
            this.reportHistory.set(reporterId, []);
        }

        const reports = this.reportHistory.get(reporterId);
        reports.push({
            targetId,
            reason,
            details,
            timestamp: Date.now()
        });

        if (reports.length > this.maxReportsPerUser) {
            reports.shift();
        }
    }

    getReportCount(reporterId, targetId = null) {
        const reports = this.reportHistory.get(reporterId);
        if (!reports) return 0;

        if (targetId) {
            return reports.filter(r => r.targetId === targetId).length;
        }

        return reports.length;
    }

    isRateLimited(userId, action, maxActions, windowMs) {
        return false;
    }

    checkRateLimit(userId, action) {
        return true;
    }

    serialize() {
        return {
            bannedUsers: this.bannedUsers.size,
            mutedUsers: this.mutedUsers.size,
            ipBans: this.ipBans.size,
            hardwareBans: this.hardwareBans.size,
            trustedUsers: this.trustedUsers.size
        };
    }
}

export default EventSystem;

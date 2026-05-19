import { GAME_CONFIG } from './config.js';

export const SCRIPT_TYPE = {
    SERVER: 0,
    CLIENT: 1,
    SHARED: 2,
    ENTITY: 3,
    GAMEMODE: 4,
    WEAPON: 5,
    BOT: 6
};

export const VScriptReturnType = {
    VOID: 0,
    BOOLEAN: 1,
    INTEGER: 2,
    FLOAT: 3,
    STRING: 4,
    ENTITY: 5,
    TABLE: 6,
    HANDLE: 7,
    VECTOR: 8
};

export class VScriptVM {
    constructor() {
        this.contexts = new Map();
        this.globals = new Map();
        this.functions = new Map();
        this.entities = new Map();
        this.convars = new Map();
        this.timers = [];
        this.nextTimerId = 0;
        this.running = false;
        this.errorHandler = null;
        this.outputCallbacks = new Map();
    }

    initialize() {
        this.setupGlobalFunctions();
        this.setupGlobalConstants();
    }

    setupGlobalFunctions() {
        this.registerGlobal('print', (...args) => {
            console.log('[VScript]', ...args);
            return null;
        });

        this.registerGlobal('printl', (...args) => {
            console.log('[VScript]', ...args.map(a => String(a) + '\n').join(''));
            return null;
        });

        this.registerGlobal('warning', (msg) => {
            console.warn('[VScript Warning]', msg);
            return null;
        });

        this.registerGlobal('error', (msg) => {
            console.error('[VScript Error]', msg);
            if (this.errorHandler) this.errorHandler(msg);
            return null;
        });

        this.registerGlobal('DoEntFire', (target, input, param, delay, activator, caller) => {
            this.handleEntFire(target, input, param, delay, activator, caller);
            return null;
        });

        this.registerGlobal('DoEntFiref', (target, input, param, delay, activator, caller) => {
            this.handleEntFire(target, input, param, delay, activator, caller);
            return null;
        });

        this.registerGlobal('IncludeScript', (scriptName, context = null) => {
            return this.includeScript(scriptName, context);
        });

        this.registerGlobal('ExecScript', (code, context = null) => {
            return this.executeCode(code, context);
        });

        this.registerGlobal('AppendToEntityTable', (entity, tableName, tableData) => {
            const ent = this.getEntity(entity);
            if (ent) {
                if (!ent.scriptData) ent.scriptData = {};
                ent.scriptData[tableName] = tableData;
            }
            return null;
        });

        this.registerGlobal('TableToSave', (table) => {
            return JSON.stringify(table);
        });

        this.registerGlobal('SaveTableToScript', (table, filename, replace = false) => {
            return this.saveTable(table, filename, replace);
        });

        this.registerGlobal('LoadTableFromScript', (filename) => {
            return this.loadTable(filename);
        });

        this.registerGlobal('ActivateSkill', (skillName) => {
            return this.activateSkill(skillName);
        });

        this.registerGlobal('GetActiveSkills', () => {
            return this.getActiveSkills();
        });

        this.registerGlobal('GetSkillLevel', (skillName) => {
            return this.getSkillLevel(skillName);
        });

        this.registerGlobal('AddSkillPoints', (points) => {
            return this.addSkillPoints(points);
        });

        this.registerGlobal('GetSkillPoints', () => {
            return this.getSkillPoints();
        });

        this.registerGlobal('SetSkillPoints', (points) => {
            return this.setSkillPoints(points);
        });

        this.registerGlobal('GetMaxSkillLevel', (skillName) => {
            return this.getMaxSkillLevel(skillName);
        });

        this.registerGlobal('IsValidEntity', (entity) => {
            return this.isValidEntity(entity);
        });

        this.registerGlobal('Entities', () => {
            return Array.from(this.entities.values());
        });

        this.registerGlobal('EntitiesByClass', (classname) => {
            return Array.from(this.entities.values()).filter(e => e.classname === classname);
        });

        this.registerGlobal('EntitiesByName', (name) => {
            return Array.from(this.entities.values()).filter(e => e.targetname === name);
        });

        this.registerGlobal('EntitiesByTarget', (target) => {
            return Array.from(this.entities.values()).filter(e => e.target === target);
        });

        this.registerGlobal('EntitiesByModel', (model) => {
            return Array.from(this.entities.values()).filter(e => e.model === model);
        });

        this.registerGlobal('EntitiesInSphere', (origin, radius) => {
            const center = this.parseVector(origin);
            const r = parseFloat(radius);
            return Array.from(this.entities.values()).filter(e => {
                const pos = this.parseVector(e.origin || { x: 0, y: 0, z: 0 });
                const dx = pos.x - center.x, dy = pos.y - center.y, dz = pos.z - center.z;
                return Math.sqrt(dx * dx + dy * dy + dz * dz) <= r;
            });
        });

        this.registerGlobal('CreateEntity', (classname) => {
            return this.createEntity(classname);
        });

        this.registerGlobal('SpawnEntity', (classname) => {
            return this.spawnEntity(classname);
        });

        this.registerGlobal('DispatchSpawn', (entity) => {
            return this.dispatchSpawn(entity);
        });

        this.registerGlobal('RemoveEntity', (entity) => {
            return this.removeEntity(entity);
        });

        this.registerGlobal('EmitSound', (entity, soundName) => {
            this.emitSound(entity, soundName);
            return null;
        });

        this.registerGlobal('EmitSoundOn', (entity, soundName, target) => {
            this.emitSoundOn(entity, soundName, target);
            return null;
        });

        this.registerGlobal('StopSound', (entity, soundName) => {
            this.stopSound(entity, soundName);
            return null;
        });

        this.registerGlobal('StopAllSounds', (entity) => {
            this.stopAllSounds(entity);
            return null;
        });

        this.registerGlobal('SetGravity', (scale) => {
            return this.setGravity(scale);
        });

        this.registerGlobal('GetGravity', () => {
            return this.getGravity();
        });

        this.registerGlobal('SetPhysVars', (gravity, air) => {
            return this.setPhysVars(gravity, air);
        });

        this.registerGlobal('Time', () => {
            return Date.now() / 1000;
        });

        this.registerGlobal('FrameTime', () => {
            return 1 / 60;
        });

        this.registerGlobal('TickCount', () => {
            return Math.floor(Date.now() / (1000 / 60));
        });

        this.registerGlobal('RandomFloat', (min, max) => {
            return min + Math.random() * (max - min);
        });

        this.registerGlobal('RandomInt', (min, max) => {
            return Math.floor(min + Math.random() * (max - min + 1));
        });

        this.registerGlobal('RandomVector', (min, max) => {
            return {
                x: min + Math.random() * (max - min),
                y: min + Math.random() * (max - min),
                z: min + Math.random() * (max - min)
            };
        });

        this.registerGlobal('Shuffle', (array) => {
            for (let i = array.length - 1; i > 0; i--) {
                const j = Math.floor(Math.random() * (i + 1));
                [array[i], array[j]] = [array[j], array[i]];
            }
            return array;
        });

        this.registerGlobal('Vector', (x = 0, y = 0, z = 0) => {
            return { x: parseFloat(x), y: parseFloat(y), z: parseFloat(z) };
        });

        this.registerGlobal('VectorToString', (vec) => {
            return `${vec.x},${vec.y},${vec.z}`;
        });

        this.registerGlobal('StringToVector', (str) => {
            const parts = str.split(',');
            return { x: parseFloat(parts[0]), y: parseFloat(parts[1]), z: parseFloat(parts[2]) };
        });

        this.registerGlobal('VectorToAngles', (vec) => {
            const yaw = Math.atan2(vec.y, vec.x) * 180 / Math.PI;
            const pitch = Math.atan2(vec.z, Math.sqrt(vec.x * vec.x + vec.y * vec.y)) * 180 / Math.PI;
            return { x: pitch, y: yaw, z: 0 };
        });

        this.registerGlobal('AnglesToVector', (angles) => {
            const pitch = angles.x * Math.PI / 180;
            const yaw = angles.y * Math.PI / 180;
            return {
                x: Math.cos(pitch) * Math.cos(yaw),
                y: Math.cos(pitch) * Math.sin(yaw),
                z: -Math.sin(pitch)
            };
        });

        this.registerGlobal('VecToVec', (v1, v2) => {
            return Math.sqrt(
                Math.pow(v1.x - v2.x, 2) +
                Math.pow(v1.y - v2.y, 2) +
                Math.pow(v1.z - v2.z, 2)
            );
        });

        this.registerGlobal('VecToYaw', (vec) => {
            return Math.atan2(vec.y, vec.x) * 180 / Math.PI;
        });

        this.registerGlobal('YawToVec', (yaw) => {
            const y = parseFloat(yaw) * Math.PI / 180;
            return { x: Math.cos(y), y: Math.sin(y), z: 0 };
        });

        this.registerGlobal('DotProduct', (v1, v2) => {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        });

        this.registerGlobal('CrossProduct', (v1, v2) => {
            return {
                x: v1.y * v2.z - v1.z * v2.y,
                y: v1.z * v2.x - v1.x * v2.z,
                z: v1.x * v2.y - v1.y * v2.x
            };
        });

        this.registerGlobal('NormalizeVector', (vec) => {
            const len = Math.sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
            if (len === 0) return { x: 0, y: 0, z: 0 };
            return { x: vec.x / len, y: vec.y / len, z: vec.z / len };
        });

        this.registerGlobal('Length', (vec) => {
            return Math.sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
        });

        this.registerGlobal('Length2D', (vec) => {
            return Math.sqrt(vec.x * vec.x + vec.y * vec.y);
        });

        this.registerGlobal('Add', (v1, v2) => {
            return { x: v1.x + v2.x, y: v1.y + v2.y, z: v1.z + v2.z };
        });

        this.registerGlobal('Multiply', (v, s) => {
            return { x: v.x * s, y: v.y * s, z: v.z * s };
        });

        this.registerGlobal('AddVectors', (...vecs) => {
            const result = { x: 0, y: 0, z: 0 };
            for (const v of vecs) {
                result.x += v.x;
                result.y += v.y;
                result.z += v.z;
            }
            return result;
        });

        this.registerGlobal('PlayerInstanceFromUserid', (userid) => {
            return this.getPlayerFromUserId(userid);
        });

        this.registerGlobal('GetListenServerHost', () => {
            return this.getListenServerHost();
        });

        this.registerGlobal('GetPlayerForUserid', (userid) => {
            return this.getPlayerFromUserId(userid);
        });

        this.registerGlobal('DoInclude', (scriptName) => {
            return this.includeScript(scriptName);
        });

        this.registerGlobal('Include', (scriptName) => {
            return this.includeScript(scriptName);
        });

        this.registerGlobal('ReloadMOTD', () => {
            this.reloadMOTD();
            return null;
        });

        this.registerGlobal('ShowMessage', (message) => {
            this.showMessage(message);
            return null;
        });

        this.registerGlobal('ClientPrint', (player, type, message) => {
            this.clientPrint(player, type, message);
            return null;
        });

        this.registerGlobal('ServerCmd', (command) => {
            this.serverCommand(command);
            return null;
        });

        this.registerGlobal('ServerCmdGetStr', (command) => {
            return this.serverCommandGetResult(command);
        });

        this.registerGlobal('GetMapName', () => {
            return this.getMapName();
        });

        this.registerGlobal('GetMapVersion', () => {
            return '1.0';
        });

        this.registerGlobal('GetEngineVersion', () => {
            return 'Source Engine 2';
        });

        this.registerGlobal('GetGameDirectory', () => {
            return 'csgo';
        });

        this.registerGlobal('GetGameFolder', () => {
            return 'csgo';
        });

        this.registerGlobal('GetGameType', () => {
            return 0;
        });

        this.registerGlobal('GetGameMode', () => {
            return 0;
        });

        this.registerGlobal('GetCurrentMap', () => {
            return this.getCurrentMap();
        });

        this.registerGlobal('IsDedicatedServer', () => {
            return false;
        });

        this.registerGlobal('IsLan', () => {
            return true;
        });

        this.registerGlobal('IsMultiplayer', () => {
            return true;
        });

        this.registerGlobal('IsPasswordRequired', () => {
            return false;
        });

        this.registerGlobal('GetHostName', () => {
            return 'Counter-Strike Server';
        });

        this.registerGlobal('SetHostName', (name) => {
            return this.setHostName(name);
        });

        this.registerGlobal('GetNameForMapping', (mapping) => {
            return mapping;
        });

        this.registerGlobal('UpdateMapName', (name) => {
            return this.updateMapName(name);
        });

        this.registerGlobal('SetVisible', (entity, visible) => {
            return this.setEntityVisible(entity, visible);
        });

        this.registerGlobal('SetDisabled', (entity, disabled) => {
            return this.setEntityDisabled(entity, disabled);
        });

        this.registerGlobal('SetKeyValue', (entity, key, value) => {
            return this.setEntityKeyValue(entity, key, value);
        });

        this.registerGlobal('GetKeyValue', (entity, key) => {
            return this.getEntityKeyValue(entity, key);
        });

        this.registerGlobal('Connect', (name, filter, desc = '') => {
            return this.connectOutput(name, filter, desc);
        });

        this.registerGlobal('ConnectOutput', (output, functionName) => {
            return this.connectOutput(output, functionName, '');
        });

        this.registerGlobal('DisconnectOutput', (output, functionName) => {
            return this.disconnectOutput(output, functionName);
        });

        this.registerGlobal('DestroyEntity', (entity) => {
            return this.destroyEntity(entity);
        });

        this.registerGlobal('SetName', (entity, name) => {
            return this.setEntityName(entity, name);
        });

        this.registerGlobal('PrecacheModel', (modelName) => {
            return this.precacheModel(modelName);
        });

        this.registerGlobal('PrecacheSound', (soundName) => {
            return this.precacheSound(soundName);
        });

        this.registerGlobal('PrecacheParticle', (particleName) => {
            return this.precacheParticle(particleName);
        });

        this.registerGlobal('PrecacheMaterial', (materialName) => {
            return this.precacheMaterial(materialName);
        });

        this.registerGlobal('PrecacheScript', (scriptName) => {
            return this.precacheScript(scriptName);
        });

        this.registerGlobal('PrecacheEntity', (entityName) => {
            return this.precacheEntity(entityName);
        });

        this.registerGlobal('PrecacheEntityFromTable', (table) => {
            return this.precacheEntityFromTable(table);
        });

        this.registerGlobal('GetPlayerCount', () => {
            return this.getPlayerCount();
        });

        this.registerGlobal('GetAlivePlayerCount', () => {
            return this.getAlivePlayerCount();
        });

        this.registerGlobal('GetPlayerCount', () => {
            return this.getPlayerCount();
        });

        this.registerGlobal('GetTotalPlayerCount', () => {
            return this.getTotalPlayerCount();
        });

        this.registerGlobal('GetFakeClientCount', () => {
            return this.getFakeClientCount();
        });

        this.registerGlobal('CreateFakeClient', (name) => {
            return this.createFakeClient(name);
        });

        this.registerGlobal('RunPlayerBindThink', (player) => {
            return this.runPlayerBindThink(player);
        });

        this.registerGlobal('SetClientListeningFlags', (client, flags) => {
            return this.setClientListeningFlags(client, flags);
        });

        this.registerGlobal('GetClientListeningFlags', (client) => {
            return this.getClientListeningFlags(client);
        });

        this.registerGlobal('SetClientListening', (receiver, sender, listen) => {
            return this.setClientListening(receiver, sender, listen);
        });

        this.registerGlobal('GetClientListening', (receiver, sender) => {
            return this.getClientListening(receiver, sender);
        });

        this.registerGlobal('ConVarExists', (name) => {
            return this.convarExists(name);
        });

        this.registerGlobal('CreateConVar', (name, defaultValue, flags = 0, description = '') => {
            return this.createConVar(name, defaultValue, flags, description);
        });

        this.registerGlobal('GetConVar', (name) => {
            return this.getConVar(name);
        });

        this.registerGlobal('GetNewestSVCommands', () => {
            return [];
        });

        this.registerGlobal('GetNewestCLCommands', () => {
            return [];
        });

        this.registerGlobal('SetConvars', (table) => {
            for (const [name, value] of Object.entries(table)) {
                this.setConvar(name, value);
            }
            return null;
        });

        this.registerGlobal('GetAllConvars', () => {
            return Object.fromEntries(this.convars);
        });

        this.registerGlobal('CreateTimer', (interval, callback, data = null) => {
            return this.createTimer(interval, callback, data);
        });

        this.registerGlobal('DestroyTable', (timer) => {
            return this.destroyTimer(timer);
        });

        this.registerGlobal('IsValid', (handle) => {
            return handle !== null && handle !== undefined;
        });

        this.registerGlobal('IsTable', (value) => {
            return typeof value === 'object' && value !== null && !Array.isArray(value);
        });

        this.registerGlobal('IsArray', (value) => {
            return Array.isArray(value);
        });

        this.registerGlobal('IsString', (value) => {
            return typeof value === 'string';
        });

        this.registerGlobal('IsNumber', (value) => {
            return typeof value === 'number';
        });

        this.registerGlobal('IsBool', (value) => {
            return typeof value === 'boolean';
        });

        this.registerGlobal('IsFunction', (value) => {
            return typeof value === 'function';
        });

        this.registerGlobal('IsEntity', (value) => {
            return typeof value === 'object' && value !== null && value.entindex !== undefined;
        });

        this.registerGlobal('IsValidEntity', (entityIndex) => {
            return this.isValidEntity(entityIndex);
        });

        this.registerGlobal('EntIndexToHScript', (entityIndex) => {
            return this.entIndexToHScript(entityIndex);
        });

        this.registerGlobal('HScriptToEntIndex', (hscript) => {
            return hscript.entindex ? hscript.entindex() : -1;
        });

        this.registerGlobal('ScriptGetHudElement', (panel, element) => {
            return this.getHudElement(panel, element);
        });

        this.registerGlobal('ScriptSetHudElement', (panel, element, value) => {
            return this.setHudElement(panel, element, value);
        });

        this.registerGlobal('ShowHudHint', (player, text, duration = 5) => {
            return this.showHudHint(player, text, duration);
        });

        this.registerGlobal('ShowCenterText', (player, text, duration = 5) => {
            return this.showCenterText(player, text, duration);
        });

        this.registerGlobal('AddCommand', (command, callback, flags = 0) => {
            return this.addCommand(command, callback, flags);
        });

        this.registerGlobal('AddConsoleCommand', (command, callback, flags = 0) => {
            return this.addConsoleCommand(command, callback, flags);
        });

        this.registerGlobal('GetCommandLine', () => {
            return '';
        });

        this.registerGlobal('GetCommandLineValue', (key) => {
            return null;
        });

        this.registerGlobal('GetCurrentCommand', () => {
            return null;
        });

        this.registerGlobal('GetCurrentEntity', () => {
            return null;
        });

        this.registerGlobal('GetCmdArg', (argNum) => {
            return '';
        });

        this.registerGlobal('GetCmdFunction', () => {
            return null;
        });

        this.registerGlobal('Awake', (scriptName) => {
            return this.awake(scriptName);
        });

        this.registerGlobal('Activate', () => {
            return this.activate();
        });

        this.registerGlobal('DebugDrawLine', (start, end, r, g, b, ztest = true, duration = 0.1) => {
            return this.debugDrawLine(start, end, r, g, b, ztest, duration);
        });

        this.registerGlobal('DebugDrawCircle', (pos, r, g, b, a, ztest = true, duration = 0.1) => {
            return this.debugDrawCircle(pos, r, g, b, a, ztest, duration);
        });

        this.registerGlobal('DebugDrawBox', (pos, mins, maxs, r, g, b, a, ztest = true, duration = 0.1) => {
            return this.debugDrawBox(pos, mins, maxs, r, g, b, a, ztest, duration);
        });

        this.registerGlobal('DebugDrawText', (pos, text, viewCheck = true, duration = 0.1) => {
            return this.debugDrawText(pos, text, viewCheck, duration);
        });

        this.registerGlobal('DebugDrawScreenText', (x, y, text, r, g, b, a, duration = 0.1) => {
            return this.debugDrawScreenText(x, y, text, r, g, b, a, duration);
        });

        this.registerGlobal('DebugDrawClear', () => {
            return this.debugDrawClear();
        });
    }

    setupGlobalConstants() {
        this.globals.set('GROUP_EVERYTHING', -1);
        this.globals.set('GROUP_NON_DESTROYABLE', -2);
        this.globals.set('GROUP_DESTROYABLE', -3);
        this.globals.set('GROUP_NOT_PLAYER_DESTROYABLE', -4);
        this.globals.set('GROUP_PLAYER_DESTROYABLE', -5);
        this.globals.set('GROUP_NPC', -6);
        this.globals.set('GROUP_PLAYER', -7);
        this.globals.set('GROUP_VEHICLE', -8);
        this.globals.set('GROUP_OBJECT', -9);
        this.globals.set('GROUP_ENTRY', -10);
        this.globals.set('GROUP_HIDDEN', -11);

        this.globals.set('HUD_PRINTNOTIFY', 1);
        this.globals.set('HUD_PRINTCONSOLE', 2);
        this.globals.set('HUD_PRINTTALK', 3);
        this.globals.set('HUD_PRINTCENTER', 4);
        this.globals.set('HUD_PRINTALERT', 5);

        this.globals.set('SF_GAMERULES_SYSTEMUI', 1);
        this.globals.set('SF_GAMERULES_DONT_SPAWN_SEQUENCE', 256);
        this.globals.set('SF_GAMERULES_IGNORE_SPAWN_WAVES', 512);
        this.globals.set('SF_GAMERULES_NO_FALL_DAMAGE', 1024);
        this.globals.set('SF_GAMERULES_NO_BULLET_CPU', 2048);
        this.globals.set('SF_GAMERULES_FOG_observer', 4096);

        this.globals.set('EF_BONEMERGE', 0x001);
        this.globals.set('EF_BRIGHTLIGHT', 0x002);
        this.globals.set('EF_DIMLIGHT', 0x004);
        this.globals.set('EF_NOINTERP', 0x008);
        this.globals.set('EF_NOSHADOW', 0x010);
        this.globals.set('EF_NODRAW', 0x020);
        this.globals.set('EF_NORECEIVESHADOW', 0x040);
        this.globals.set('EF_BONEMERGE_FASTCULL', 0x080);
        this.globals.set('EF_ITEM_BLEND', 0x100);

        this.globals.set('FL_FLY', 0x001);
        this.globals.set('FL_CLIENT', 0x002);
        this.globals.set('FL_NPC', 0x004);
        this.globals.set('FL_INWATER', 0x008);
        this.globals.set('FL_FAKECLIENT', 0x010);
        this.globals.set('FL_TRANSRAGDOLL', 0x020);
        this.globals.set('FL_UNBLOCKABLE_BY_PLAYER', 0x040);

        this.globals.set('USE_OFF', 0);
        this.globals.set('USE_ON', 1);
        this.globals.set('USE_SET', 2);
        this.globals.set('USE_TOGGLE', 3);

        this.globals.set('DOD_BOMB_A', 0);
        this.globals.set('DOD_BOMB_B', 1);

        this.globals.set('MODE_DEATHMATCH', 0);
        this.globals.set('MODE_TEAMPLAY', 1);
        this.globals.set('MODE_CASUAL', 2);
        this.globals.set('MODE_COMPETITIVE', 3);

        this.globals.set('TEAM_UNASSIGNED', 0);
        this.globals.set('TEAM_SPECTATOR', 1);
        this.globals.set('TEAM_TERRORIST', 2);
        this.globals.set('TEAM_COUNTER_TERRORIST', 3);
    }

    registerGlobal(name, fn) {
        this.globals.set(name, fn);
    }

    registerFunction(name, fn, returnType = VScriptReturnType.VOID) {
        this.functions.set(name, { fn, returnType });
    }

    registerEntity(entity) {
        this.entities.set(entity.entindex(), entity);
    }

    unregisterEntity(entity) {
        this.entities.delete(entity.entindex());
    }

    getEntity(handle) {
        if (typeof handle === 'number') {
            return this.entities.get(handle);
        }
        if (typeof handle === 'object' && handle.entindex) {
            return this.entities.get(handle.entindex());
        }
        return null;
    }

    parseVector(vec) {
        if (typeof vec === 'string') {
            const parts = vec.split(',').map(Number);
            return { x: parts[0] || 0, y: parts[1] || 0, z: parts[2] || 0 };
        }
        if (vec && typeof vec.x !== 'undefined') {
            return { x: parseFloat(vec.x), y: parseFloat(vec.y), z: parseFloat(vec.z) };
        }
        return { x: 0, y: 0, z: 0 };
    }

    handleEntFire(target, input, param, delay, activator, caller) {
        const targetEntity = this.getEntity(target);
        if (targetEntity && targetEntity[`Input${input}`]) {
            setTimeout(() => {
                targetEntity[`Input${input}`](param);
            }, (delay || 0) * 1000);
        }
    }

    includeScript(scriptName, context = null) {
        const script = this.loadScript(scriptName);
        if (script) {
            return this.executeCode(script, context);
        }
        return false;
    }

    executeCode(code, context = null) {
        try {
            const fn = new Function(...this.globals.keys(), `"use strict"; ${code}`);
            return fn(...this.globals.values());
        } catch (error) {
            console.error('[VScript]', error);
            if (this.errorHandler) this.errorHandler(error);
            return null;
        }
    }

    loadScript(scriptName) {
        return null;
    }

    createTimer(interval, callback, data = null) {
        const timer = {
            id: this.nextTimerId++,
            interval,
            callback,
            data,
            lastRun: Date.now(),
            destroyed: false,
            deleteWhenDone: true,
            repeats: true
        };
        this.timers.push(timer);
        return timer;
    }

    destroyTimer(timer) {
        if (timer) timer.destroyed = true;
    }

    updateTimers(deltaTime) {
        const now = Date.now();
        for (let i = this.timers.length - 1; i >= 0; i--) {
            const timer = this.timers[i];
            if (timer.destroyed) {
                this.timers.splice(i, 1);
                continue;
            }

            const elapsed = (now - timer.lastRun) / 1000;
            if (elapsed >= timer.interval) {
                try {
                    timer.callback(timer.data);
                    timer.lastRun = now;

                    if (!timer.repeats) {
                        timer.destroyed = true;
                    }
                } catch (error) {
                    console.error('[VScript Timer Error]', error);
                    timer.destroyed = true;
                }
            }
        }
    }

    emitSound(entity, soundName) {
        if (entity) {
            const ent = this.getEntity(entity);
            if (ent && ent.EmitSound) {
                ent.EmitSound(soundName);
            }
        }
    }

    emitSoundOn(entity, soundName, target) {
        const targetEnt = this.getEntity(target);
        if (targetEnt && targetEnt.EmitSound) {
            targetEnt.EmitSound(soundName);
        }
    }

    stopSound(entity, soundName) {
    }

    stopAllSounds(entity) {
    }

    setGravity(scale) {
        return scale;
    }

    getGravity() {
        return 1.0;
    }

    setPhysVars(gravity, air) {
    }

    getPlayerFromUserId(userid) {
        return null;
    }

    getListenServerHost() {
        return null;
    }

    reloadMOTD() {
    }

    showMessage(message) {
        console.log('[Message]', message);
    }

    clientPrint(player, type, message) {
    }

    serverCommand(command) {
        console.log('[Server Command]', command);
    }

    serverCommandGetResult(command) {
        return '';
    }

    getMapName() {
        return 'de_dust2';
    }

    getCurrentMap() {
        return this.getMapName();
    }

    setHostName(name) {
    }

    updateMapName(name) {
    }

    setEntityVisible(entity, visible) {
        const ent = this.getEntity(entity);
        if (ent) ent.visible = visible;
    }

    setEntityDisabled(entity, disabled) {
        const ent = this.getEntity(entity);
        if (ent) ent.disabled = disabled;
    }

    setEntityKeyValue(entity, key, value) {
        const ent = this.getEntity(entity);
        if (ent) ent[key] = value;
    }

    getEntityKeyValue(entity, key) {
        const ent = this.getEntity(entity);
        return ent ? ent[key] : null;
    }

    connectOutput(output, functionName, description) {
        if (!this.outputCallbacks.has(output)) {
            this.outputCallbacks.set(output, []);
        }
        this.outputCallbacks.get(output).push({ functionName, description });
        return true;
    }

    disconnectOutput(output, functionName) {
        const callbacks = this.outputCallbacks.get(output);
        if (callbacks) {
            const index = callbacks.findIndex(cb => cb.functionName === functionName);
            if (index !== -1) callbacks.splice(index, 1);
        }
    }

    fireOutput(output, activator, caller, data) {
        const callbacks = this.outputCallbacks.get(output);
        if (callbacks) {
            for (const callback of callbacks) {
                try {
                    if (typeof callback.functionName === 'function') {
                        callback.functionName(activator, caller, data);
                    }
                } catch (error) {
                    console.error('[VScript Output Error]', error);
                }
            }
        }
    }

    destroyEntity(entity) {
        const ent = this.getEntity(entity);
        if (ent && ent.Destroy) ent.Destroy();
    }

    setEntityName(entity, name) {
        const ent = this.getEntity(entity);
        if (ent) ent.targetname = name;
    }

    precacheModel(modelName) {
        return true;
    }

    precacheSound(soundName) {
        return true;
    }

    precacheParticle(particleName) {
        return true;
    }

    precacheMaterial(materialName) {
        return true;
    }

    precacheScript(scriptName) {
        return true;
    }

    precacheEntity(entityName) {
        return true;
    }

    precacheEntityFromTable(table) {
        return true;
    }

    getPlayerCount() {
        return 0;
    }

    getAlivePlayerCount() {
        return 0;
    }

    getTotalPlayerCount() {
        return this.getPlayerCount();
    }

    getFakeClientCount() {
        return 0;
    }

    createFakeClient(name) {
        return null;
    }

    runPlayerBindThink(player) {
    }

    setClientListeningFlags(client, flags) {
    }

    getClientListeningFlags(client) {
        return 0;
    }

    setClientListening(receiver, sender, listen) {
    }

    getClientListening(receiver, sender) {
        return false;
    }

    convarExists(name) {
        return this.convars.has(name);
    }

    createConVar(name, defaultValue, flags, description) {
        const convar = { name, defaultValue, value: defaultValue, flags, description };
        this.convars.set(name, convar);
        return convar;
    }

    getConVar(name) {
        return this.convars.get(name);
    }

    setConvar(name, value) {
        const convar = this.convars.get(name);
        if (convar) {
            convar.value = value;
        }
    }

    isValidEntity(entityIndex) {
        if (typeof entityIndex === 'number') {
            return this.entities.has(entityIndex);
        }
        if (typeof entityIndex === 'object' && entityIndex.entindex) {
            return this.entities.has(entityIndex.entindex());
        }
        return false;
    }

    entIndexToHScript(entityIndex) {
        return this.entities.get(entityIndex);
    }

    getHudElement(panel, element) {
        return null;
    }

    setHudElement(panel, element, value) {
    }

    showHudHint(player, text, duration) {
    }

    showCenterText(player, text, duration) {
    }

    addCommand(command, callback, flags) {
        this.registerFunction(command, callback);
    }

    addConsoleCommand(command, callback, flags) {
        this.registerFunction(command, callback);
    }

    awake(scriptName) {
        return this.includeScript(scriptName);
    }

    activate() {
    }

    debugDrawLine(start, end, r, g, b, ztest, duration) {
    }

    debugDrawCircle(pos, radius, r, g, b, a, ztest, duration) {
    }

    debugDrawBox(pos, mins, maxs, r, g, b, a, ztest, duration) {
    }

    debugDrawText(pos, text, viewCheck, duration) {
    }

    debugDrawScreenText(x, y, text, r, g, b, a, duration) {
    }

    debugDrawClear() {
    }

    activateSkill(skillName) {
        return true;
    }

    getActiveSkills() {
        return [];
    }

    getSkillLevel(skillName) {
        return 0;
    }

    addSkillPoints(points) {
        return 0;
    }

    getSkillPoints() {
        return 0;
    }

    setSkillPoints(points) {
    }

    getMaxSkillLevel(skillName) {
        return 0;
    }

    saveTable(table, filename, replace) {
        return true;
    }

    loadTable(filename) {
        return null;
    }

    update(deltaTime) {
        this.updateTimers(deltaTime);
    }

    start() {
        this.running = true;
    }

    stop() {
        this.running = false;
    }

    reset() {
        this.timers = [];
        this.errorHandler = null;
        this.outputCallbacks.clear();
    }

    toJSON() {
        return {
            timers: this.timers.length,
            entities: this.entities.size,
            convars: Object.keys(Object.fromEntries(this.convars)).length
        };
    }
}

export class EntityScript {
    constructor(entity, scriptScope = null) {
        this.entity = entity;
        this.scriptScope = scriptScope || {};
        this.scriptVM = null;
        this.initialized = false;
        this.destroyed = false;
    }

    initialize(scriptVM) {
        this.scriptVM = scriptVM;
        this.setupEntityMethods();
        this.setupEntityCallbacks();
        this.initialized = true;
    }

    setupEntityMethods() {
        const entity = this.entity;

        this.scriptScope.GetOrigin = () => entity.origin || { x: 0, y: 0, z: 0 };
        this.scriptScope.SetOrigin = (pos) => { entity.origin = pos; };

        this.scriptScope.GetForward = () => entity.forward || { x: 0, y: 0, z: 1 };
        this.scriptScope.GetRight = () => entity.right || { x: 1, y: 0, z: 0 };
        this.scriptScope.GetUp = () => entity.up || { x: 0, y: 1, z: 0 };

        this.scriptScope.GetAngles = () => entity.angles || { x: 0, y: 0, z: 0 };
        this.scriptScope.SetAngles = (angles) => { entity.angles = angles; };

        this.scriptScope.GetClassname = () => entity.classname || '';
        this.scriptScope.GetName = () => entity.targetname || '';
        this.scriptScope.SetName = (name) => { entity.targetname = name; };

        this.scriptScope.GetVelocity = () => entity.velocity || { x: 0, y: 0, z: 0 };
        this.scriptScope.SetVelocity = (vel) => { entity.velocity = vel; };

        this.scriptScope.GetLocalAngularVelocity = () => entity.angularVelocity || { x: 0, y: 0, z: 0 };
        this.scriptScope.SetLocalAngularVelocity = (vel) => { entity.angularVelocity = vel; };

        this.scriptScope.GetModel = () => entity.model || '';
        this.scriptScope.SetModel = (model) => { entity.model = model; };

        this.scriptScope.GetTeam = () => entity.team || 0;
        this.scriptScope.SetTeam = (team) => { entity.team = team; };

        this.scriptScope.GetOwner = () => entity.owner;
        this.scriptScope.SetOwner = (owner) => { entity.owner = owner; };

        this.scriptScope.GetOwnerEntity = () => entity.owner;

        this.scriptScope.GetHealth = () => entity.health || 0;
        this.scriptScope.SetHealth = (health) => { entity.health = health; };
        this.scriptScope.GetMaxHealth = () => entity.maxHealth || 100;
        this.scriptScope.SetMaxHealth = (maxHealth) => { entity.maxHealth = maxHealth; };

        this.scriptScope.IsAlive = () => entity.alive !== false;

        this.scriptScope.Kill = () => {
            if (entity.Kill) entity.Kill();
        };

        this.scriptScope.ValidateScriptScope = () => true;

        this.scriptScope.entindex = () => entity.entindex ? entity.entindex() : -1;

        this.scriptScope.teleport = (pos, ang, vel) => {
            if (pos) entity.origin = pos;
            if (ang) entity.angles = ang;
            if (vel) entity.velocity = vel;
        };

        this.scriptScope.GetOrigin = () => entity.origin || { x: 0, y: 0, z: 0 };
        this.scriptScope.SetOrigin = (pos) => { entity.origin = pos; };

        this.scriptScope.GetForward = () => entity.forward || { x: 0, y: 0, z: 1 };
        this.scriptScope.GetRight = () => entity.right || { x: 1, y: 0, z: 0 };
        this.scriptScope.GetUp = () => entity.up || { x: 0, y: 1, z: 0 };

        this.scriptScope.EmitSound = (soundName) => {
            if (entity.EmitSound) entity.EmitSound(soundName);
        };

        this.scriptScope.EmitSoundOn = (soundName, target) => {
            if (target && target.EmitSound) target.EmitSound(soundName);
        };

        this.scriptScope.StopSound = (soundName) => {
            if (entity.StopSound) entity.StopSound(soundName);
        };

        this.scriptScope.GetActiveWeapon = () => {
            if (entity.GetActiveWeapon) return entity.GetActiveWeapon();
            return null;
        };

        this.scriptScope.GetWeapon = (weaponName) => {
            return null;
        };

        this.scriptScope.GiveWeapon = (weaponName) => {
            return null;
        };

        this.scriptScope.AddOutput = (outputDef) => {
            if (entity.AddOutput) entity.AddOutput(outputDef);
        };

        this.scriptScope.ConnectOutput = (output, functionName) => {
            if (entity.ConnectOutput) entity.ConnectOutput(output, functionName);
        };

        this.scriptScope.DisconnectOutput = (output, functionName) => {
            if (entity.DisconnectOutput) entity.DisconnectOutput(output, functionName);
        };

        this.scriptScope.FireOutput = (output, activator, caller, data) => {
            if (entity.FireOutput) entity.FireOutput(output, activator, caller, data);
        };

        this.scriptScope.SetThink = (functionName) => {
            if (entity.SetThink) entity.SetThink(functionName);
        };

        this.scriptScope.CallFunction = (functionName) => {
            if (this.scriptScope[functionName]) {
                return this.scriptScope[functionName]();
            }
            return null;
        };
    }

    setupEntityCallbacks() {
        this.scriptScope.__DestroyScript = () => {
            this.destroy();
        };
    }

    callFunction(functionName, ...args) {
        if (this.scriptScope[functionName]) {
            try {
                return this.scriptScope[functionName](...args);
            } catch (error) {
                console.error(`[EntityScript] Error calling ${functionName}:`, error);
                return null;
            }
        }
        return null;
    }

    setFunction(functionName, fn) {
        this.scriptScope[functionName] = fn;
    }

    getFunction(functionName) {
        return this.scriptScope[functionName];
    }

    hasFunction(functionName) {
        return typeof this.scriptScope[functionName] === 'function';
    }

    destroy() {
        if (this.destroyed) return;
        this.destroyed = true;

        if (this.scriptScope.__DestroyScript) {
            try {
                this.scriptScope.__DestroyScript();
            } catch (error) {
                console.error('[EntityScript] Error in __DestroyScript:', error);
            }
        }

        this.scriptScope = {};
        this.entity = null;
        this.scriptVM = null;
    }
}

export class ScriptManager {
    constructor() {
        this.vm = new VScriptVM();
        this.entityScripts = new Map();
        this.loadedScripts = new Set();
        this.scriptPaths = new Map();
        this.scriptsByType = {
            [SCRIPT_TYPE.SERVER]: new Set(),
            [SCRIPT_TYPE.CLIENT]: new Set(),
            [SCRIPT_TYPE.SHARED]: new Set()
        };
    }

    initialize() {
        this.vm.initialize();
    }

    registerScriptPath(type, path) {
        this.scriptPaths.set(type, path);
    }

    loadScript(scriptName, type = SCRIPT_TYPE.SHARED) {
        if (this.loadedScripts.has(scriptName)) {
            return true;
        }

        const path = this.scriptPaths.get(type);
        const fullPath = path ? `${path}/${scriptName}` : scriptName;

        const script = this.vm.loadScript(fullPath);
        if (script) {
            this.loadedScripts.add(scriptName);
            this.scriptsByType[type].add(scriptName);
            return true;
        }

        return false;
    }

    executeScript(scriptName, context = null, type = SCRIPT_TYPE.SHARED) {
        if (!this.loadScript(scriptName, type)) {
            return false;
        }

        return this.vm.includeScript(scriptName, context);
    }

    registerEntityScript(entity, scriptName = null) {
        const scriptScope = {};
        const entityScript = new EntityScript(entity, scriptScope);

        if (scriptName) {
            this.executeScript(scriptName, scriptScope);
        }

        entityScript.initialize(this.vm);
        this.entityScripts.set(entity.entindex(), entityScript);

        return entityScript;
    }

    unregisterEntityScript(entity) {
        const entIndex = typeof entity === 'number' ? entity : entity.entindex();
        const script = this.entityScripts.get(entIndex);

        if (script) {
            script.destroy();
            this.entityScripts.delete(entIndex);
        }
    }

    getEntityScript(entity) {
        const entIndex = typeof entity === 'number' ? entity : entity.entindex();
        return this.entityScripts.get(entIndex);
    }

    callEntityFunction(entity, functionName, ...args) {
        const script = this.getEntityScript(entity);
        if (script) {
            return script.callFunction(functionName, ...args);
        }
        return null;
    }

    createGlobalFunction(name, fn) {
        this.vm.registerGlobal(name, fn);
    }

    createEntityFunction(entityType, name, fn) {
    }

    getVM() {
        return this.vm;
    }

    update(deltaTime) {
        this.vm.update(deltaTime);
    }

    start() {
        this.vm.start();
    }

    stop() {
        this.vm.stop();
    }

    clear() {
        for (const script of this.entityScripts.values()) {
            script.destroy();
        }
        this.entityScripts.clear();
        this.loadedScripts.clear();
        this.vm.reset();
    }
}

export default {
    SCRIPT_TYPE,
    VScriptReturnType,
    VScriptVM,
    EntityScript,
    ScriptManager
};

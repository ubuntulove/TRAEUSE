import { GameEngine } from './engine.js';
import { Renderer, Camera } from './renderer.js';
import { InputManager } from './input.js';
import { Player, PlayerManager } from './player.js';
import { WeaponFactory, WeaponManager, WeaponSkin } from './weapons.js';
import { PhysicsWorld, Vector3 } from './physics.js';
import { NetworkManager, ReliableUDP } from './networking.js';
import { UIManager } from './ui.js';
import { SoundManager, MusicManager, VoiceOverManager } from './sound.js';
import { MapManager } from './maps.js';
import { EconomySystem, BuyMenuSystem } from './economy.js';
import { RoundManager, MatchRecorder, PauseManager } from './round.js';
import { BotManager } from './bot.js';
import { GAME_CONFIG, UI_CONFIG, TEAM, GAME_MODE, GAME_STATE } from './config.js';

export class Game {
    constructor() {
        this.name = GAME_CONFIG.TITLE;
        this.version = GAME_CONFIG.VERSION;
        
        this.state = GAME_STATE.IDLE;
        this.mode = GAME_MODE.COMPETITIVE;
        
        this.canvas = null;
        this.ctx = null;
        
        this.engine = null;
        this.renderer = null;
        this.camera = null;
        this.input = null;
        this.ui = null;
        this.sound = null;
        this.network = null;
        this.physics = null;
        this.map = null;
        this.economy = null;
        this.round = null;
        this.bots = null;
        
        this.playerManager = null;
        this.weaponManager = null;
        this.buyMenu = null;
        this.matchRecorder = null;
        this.pauseManager = null;
        this.musicManager = null;
        this.voiceManager = null;
        
        this.localPlayer = null;
        this.players = new Map();
        this.bots = new Map();
        
        this.lastFrameTime = 0;
        this.deltaTime = 0;
        this.frameCount = 0;
        this.fps = 0;
        this.fpsUpdateTime = 0;
        this.fpsUpdateInterval = 500;
        this.frameTimeAccumulator = 0;
        
        this.running = false;
        this.paused = false;
        this.focused = true;
        
        this.showDebug = false;
        this.showFPS = true;
        
        this.systems = [];
        this.managers = [];
        
        this.gameTime = 0;
        this.gameDate = new Date();
    }
    
    async initialize(canvasId) {
        console.log(`Initializing ${this.name} v${this.version}`);
        
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) {
            console.error('Canvas not found');
            return false;
        }
        
        this.ctx = this.canvas.getContext('2d');
        
        this.canvas.width = GAME_CONFIG.CANVAS_WIDTH;
        this.canvas.height = GAME_CONFIG.CANVAS_HEIGHT;
        
        this.engine = new GameEngine();
        this.engine.initialize();
        
        this.camera = new Camera();
        this.renderer = new Renderer(this.canvas);
        
        this.input = new InputManager();
        
        this.physics = new PhysicsWorld();
        this.physics.initialize();
        
        this.ui = new UIManager();
        
        this.sound = new SoundManager();
        await this.sound.initialize();
        
        this.musicManager = new MusicManager(this.sound);
        this.voiceManager = new VoiceOverManager(this.sound);
        
        this.network = new NetworkManager();
        
        this.map = new MapManager();
        this.map.initialize();
        
        this.economy = new EconomySystem();
        this.buyMenu = new BuyMenuSystem();
        
        this.round = new RoundManager();
        this.matchRecorder = new MatchRecorder();
        this.pauseManager = new PauseManager();
        
        this.playerManager = new PlayerManager();
        this.weaponManager = new WeaponManager();
        this.botManager = new BotManager();
        
        this.setupEventListeners();
        
        this.state = GAME_STATE.READY;
        
        console.log('Game initialized successfully');
        
        return true;
    }
    
    setupEventListeners() {
        window.addEventListener('focus', () => {
            this.focused = true;
        });
        
        window.addEventListener('blur', () => {
            this.focused = false;
        });
        
        window.addEventListener('resize', () => {
            this.handleResize();
        });
        
        window.addEventListener('keydown', (e) => {
            this.handleKeyDown(e);
        });
        
        window.addEventListener('keyup', (e) => {
            this.handleKeyUp(e);
        });
        
        window.addEventListener('mousedown', (e) => {
            this.handleMouseDown(e);
        });
        
        window.addEventListener('mouseup', (e) => {
            this.handleMouseUp(e);
        });
        
        window.addEventListener('mousemove', (e) => {
            this.handleMouseMove(e);
        });
        
        this.input.addEventListener('keydown', (data) => {
            this.onKeyDown(data);
        });
        
        this.input.addEventListener('keyup', (data) => {
            this.onKeyUp(data);
        });
        
        this.round.onEvent('onRoundStart', (data) => {
            this.onRoundStart(data);
        });
        
        this.round.onEvent('onRoundEnd', (data) => {
            this.onRoundEnd(data);
        });
        
        this.round.onEvent('onMatchEnd', (data) => {
            this.onMatchEnd(data);
        });
        
        this.ui.addEventListener('play', () => {
            this.startGame();
        });
        
        this.ui.addEventListener('options', () => {
            this.showOptions();
        });
        
        this.ui.addEventListener('quit', () => {
            this.quitGame();
        });
    }
    
    handleResize() {
        const width = window.innerWidth;
        const height = window.innerHeight;
        
        this.canvas.width = width;
        this.canvas.height = height;
        
        this.renderer.resize(width, height);
    }
    
    handleKeyDown(e) {
        if (e.key === 'Escape') {
            if (this.state === GAME_STATE.PLAYING) {
                this.togglePause();
            }
        } else if (e.key === 'Tab' && !e.shiftKey) {
            if (this.state === GAME_STATE.PLAYING) {
                this.ui.showScoreboard();
            }
        } else if (e.key === 'b') {
            if (this.state === GAME_STATE.PLAYING) {
                this.ui.showBuyMenu();
            }
        } else if (e.key === 'i') {
            if (this.state === GAME_STATE.PLAYING) {
                this.ui.toggleInventory();
            }
        }
    }
    
    handleKeyUp(e) {
    }
    
    handleMouseDown(e) {
        if (e.button === 0) {
            if (this.state === GAME_STATE.PLAYING) {
                this.localPlayer?.shoot();
            }
        } else if (e.button === 1) {
            this.localPlayer?.useScope();
        } else if (e.button === 2) {
        }
    }
    
    handleMouseUp(e) {
    }
    
    handleMouseMove(e) {
        if (this.input.isMouseLocked()) {
            const sensitivity = this.input.getMouseSensitivity();
            const delta = this.input.getAccumulatedMouseDelta();
            
            this.camera.rotate(delta.x * sensitivity * 0.002, delta.y * sensitivity * 0.002);
            
            if (this.localPlayer) {
                this.localPlayer.rotation.yaw = this.camera.rotation.yaw;
                this.localPlayer.rotation.pitch = this.camera.rotation.pitch;
            }
        }
    }
    
    onKeyDown(data) {
        if (data.key === 'w' || data.key === 'W') {
            if (this.localPlayer) this.localPlayer.inputState.forward = 1;
        } else if (data.key === 's' || data.key === 'S') {
            if (this.localPlayer) this.localPlayer.inputState.backward = 1;
        } else if (data.key === 'a' || data.key === 'A') {
            if (this.localPlayer) this.localPlayer.inputState.left = 1;
        } else if (data.key === 'd' || data.key === 'D') {
            if (this.localPlayer) this.localPlayer.inputState.right = 1;
        } else if (data.key === ' ') {
            if (this.localPlayer) this.localPlayer.inputState.jump = true;
        } else if (data.key === 'Control') {
            if (this.localPlayer) this.localPlayer.inputState.duck = true;
        } else if (data.key === 'Shift') {
            if (this.localPlayer) this.localPlayer.inputState.walk = true;
        }
    }
    
    onKeyUp(data) {
        if (data.key === 'w' || data.key === 'W') {
            if (this.localPlayer) this.localPlayer.inputState.forward = 0;
        } else if (data.key === 's' || data.key === 'S') {
            if (this.localPlayer) this.localPlayer.inputState.backward = 0;
        } else if (data.key === 'a' || data.key === 'A') {
            if (this.localPlayer) this.localPlayer.inputState.left = 0;
        } else if (data.key === 'd' || data.key === 'D') {
            if (this.localPlayer) this.localPlayer.inputState.right = 0;
        } else if (data.key === ' ') {
            if (this.localPlayer) this.localPlayer.inputState.jump = false;
        } else if (data.key === 'Control') {
            if (this.localPlayer) this.localPlayer.inputState.duck = false;
        } else if (data.key === 'Shift') {
            if (this.localPlayer) this.localPlayer.inputState.walk = false;
        }
    }
    
    async startGame() {
        console.log('Starting game...');
        
        this.matchRecorder.startRecording();
        
        this.state = GAME_STATE.LOADING;
        
        await this.loadMap('de_dust2');
        
        await this.setupPlayers();
        
        this.round.initialize({
            [TEAM.TERRORIST]: this.players.filter(p => p.team === TEAM.TERRORIST),
            [TEAM.COUNTER_TERRORIST]: this.players.filter(p => p.team === TEAM.COUNTER_TERRORIST)
        });
        
        this.state = GAME_STATE.PLAYING;
        
        this.sound.playMusic('ambient_action');
        
        this.matchRecorder.setMatchData({
            map: 'de_dust2',
            mode: this.mode,
            players: this.players.length
        });
        
        console.log('Game started');
    }
    
    async loadMap(mapId) {
        console.log(`Loading map: ${mapId}`);
        
        await this.map.loadMap(mapId);
        
        const map = this.map.getCurrentMap();
        
        this.physics.clear();
        
        for (const geometry of map.geometry || []) {
            this.physics.addStaticGeometry(geometry);
        }
        
        return true;
    }
    
    async setupPlayers() {
        const localPlayer = new Player(1, 'Player', TEAM.COUNTER_TERRORIST, false);
        localPlayer.position = this.map.getCurrentMap().getSpawnPosition(TEAM.COUNTER_TERRORIST);
        localPlayer.money = 800;
        
        this.players.set(localPlayer.id, localPlayer);
        this.playerManager.addPlayer(localPlayer);
        this.playerManager.setLocalPlayer(localPlayer);
        this.localPlayer = localPlayer;
        
        const teamSize = GAME_CONFIG.TEAM_SIZE;
        
        const terroristCount = teamSize - 1;
        const ctCount = teamSize - 1;
        
        for (let i = 0; i < terroristCount; i++) {
            const bot = this.botManager.addBot(`Bot_T_${i}`, TEAM.TERRORIST);
            bot.position = this.map.getCurrentMap().getSpawnPosition(TEAM.TERRORIST);
            this.players.set(bot.id, bot);
        }
        
        for (let i = 0; i < ctCount; i++) {
            const bot = this.botManager.addBot(`Bot_CT_${i}`, TEAM.COUNTER_TERRORIST);
            bot.position = this.map.getCurrentMap().getSpawnPosition(TEAM.COUNTER_TERRORIST);
            this.players.set(bot.id, bot);
        }
        
        this.round.players = {
            [TEAM.TERRORIST]: this.players.filter(p => p.team === TEAM.TERRORIST),
            [TEAM.COUNTER_TERRORIST]: this.players.filter(p => p.team === TEAM.COUNTER_TERRORIST)
        };
    }
    
    startBotMatch() {
        const terroristBots = this.botManager.getTeamBots(TEAM.TERRORIST);
        const ctBots = this.botManager.getTeamBots(TEAM.COUNTER_TERRORIST);
        
        if (terroristBots.length < GAME_CONFIG.TEAM_SIZE) {
            this.botManager.fillServer(TEAM.TERRORIST, GAME_CONFIG.TEAM_SIZE);
        }
        
        if (ctBots.length < GAME_CONFIG.TEAM_SIZE) {
            this.botManager.fillServer(TEAM.COUNTER_TERRORIST, GAME_CONFIG.TEAM_SIZE);
        }
    }
    
    togglePause() {
        if (this.paused) {
            this.resume();
        } else {
            this.pause();
        }
    }
    
    pause() {
        this.paused = true;
        this.pauseManager.pause();
        this.ui.showPauseMenu();
    }
    
    resume() {
        this.paused = false;
        this.pauseManager.resume();
        this.ui.hidePauseMenu();
    }
    
    onRoundStart(data) {
        console.log(`Round ${data.round} started`);
        
        this.sound.playRoundStart();
        
        for (const player of this.players.values()) {
            player.resetForNextRound();
        }
    }
    
    onRoundEnd(data) {
        console.log(`Round ${data.round} ended: ${data.reason} - Winner: ${data.winner}`);
        
        this.matchRecorder.recordEvent('round_end', data);
        
        if (data.winner === TEAM.TERRORIST) {
            this.sound.play('round_end_terrorist');
        } else if (data.winner === TEAM.COUNTER_TERRORIST) {
            this.sound.play('round_end_counterterrorist');
        }
    }
    
    onMatchEnd(data) {
        console.log(`Match ended: ${data.reason} - Winner: ${data.winner}`);
        
        this.matchRecorder.recordEvent('match_end', data);
        
        this.matchRecorder.stopRecording();
        
        this.state = GAME_STATE.MATCH_END;
        
        const matchData = this.matchRecorder.getMatchData();
        console.log('Match data:', matchData);
        
        this.ui.showMatchEndScreen(data);
    }
    
    update(deltaTime) {
        if (!this.focused || this.paused) {
            this.input.update();
            return;
        }
        
        this.gameTime += deltaTime;
        
        if (this.state === GAME_STATE.PLAYING) {
            this.round.update(deltaTime);
            
            this.updatePlayers(deltaTime);
            
            this.updateBots(deltaTime);
            
            this.updatePhysics(deltaTime);
            
            this.updateCamera(deltaTime);
        }
        
        this.updateInput(deltaTime);
        
        this.updateUI(deltaTime);
        
        this.updateStats(deltaTime);
        
        this.input.update();
    }
    
    updatePlayers(deltaTime) {
        if (this.localPlayer && this.localPlayer.state.alive) {
            this.localPlayer.update(deltaTime);
        }
        
        for (const player of this.players.values()) {
            if (player !== this.localPlayer && player.state.alive) {
                player.update(deltaTime);
            }
        }
    }
    
    updateBots(deltaTime) {
        this.botManager.update(deltaTime);
    }
    
    updatePhysics(deltaTime) {
        this.physics.update(deltaTime);
    }
    
    updateCamera(deltaTime) {
        if (this.localPlayer) {
            this.camera.position = {
                x: this.localPlayer.position.x,
                y: this.localPlayer.position.y,
                z: this.localPlayer.position.z + this.localPlayer.viewOffset.z
            };
            
            this.camera.rotation = {
                pitch: this.localPlayer.rotation.pitch,
                yaw: this.localPlayer.rotation.yaw,
                roll: this.localPlayer.rotation.roll
            };
        }
        
        this.camera.update(deltaTime);
        this.renderer.setCamera(this.camera.position, this.camera.rotation);
    }
    
    updateInput(deltaTime) {
    }
    
    updateUI(deltaTime) {
        this.ui.update(deltaTime);
        
        if (this.localPlayer) {
            const healthBar = this.ui.getPanel('health-bar');
            if (healthBar) {
                healthBar.value = this.localPlayer.health;
            }
            
            const armorBar = this.ui.getPanel('armor-bar');
            if (armorBar) {
                armorBar.value = this.localPlayer.armor;
            }
            
            const ammoCounter = this.ui.getPanel('ammo-counter');
            if (ammoCounter) {
                ammoCounter.currentAmmo = this.localPlayer.ammo.primary.current;
                ammoCounter.reserveAmmo = this.localPlayer.ammo.primary.reserve;
            }
            
            const moneyDisplay = this.ui.getPanel('money-display');
            if (moneyDisplay) {
                moneyDisplay.money = this.localPlayer.money;
            }
            
            const roundTimer = this.ui.getPanel('round-timer');
            if (roundTimer) {
                roundTimer.time = this.round.currentRoundTime;
            }
            
            const scoreBoard = this.ui.getPanel('score-board');
            if (scoreBoard) {
                scoreBoard.team1Score = this.round.teamScores[TEAM.TERRORIST];
                scoreBoard.team2Score = this.round.teamScores[TEAM.COUNTER_TERRORIST];
            }
        }
    }
    
    updateStats(deltaTime) {
        this.frameCount++;
        this.frameTimeAccumulator += deltaTime;
        
        if (this.frameTimeAccumulator >= this.fpsUpdateInterval) {
            this.fps = Math.round(this.frameCount / (this.frameTimeAccumulator / 1000));
            this.frameCount = 0;
            this.frameTimeAccumulator = 0;
        }
    }
    
    render() {
        this.renderer.clear();
        
        if (this.state === GAME_STATE.PLAYING || this.state === GAME_STATE.PAUSED) {
            this.renderWorld();
            this.renderPlayers();
            this.renderEffects();
        }
        
        this.renderUI();
        
        if (this.showDebug) {
            this.renderDebug();
        }
        
        if (this.showFPS) {
            this.renderFPS();
        }
    }
    
    renderWorld() {
        const map = this.map.getCurrentMap();
        if (!map) return;
        
        for (const geometry of map.geometry || []) {
            this.renderGeometry(geometry);
        }
        
        for (const prop of map.props || []) {
            this.renderProp(prop);
        }
    }
    
    renderGeometry(geometry) {
    }
    
    renderProp(prop) {
    }
    
    renderPlayers() {
        for (const player of this.players.values()) {
            if (!player.state.alive) continue;
            
            const screenPos = this.renderer.worldToScreen(player.position);
            if (!screenPos) continue;
            
            if (player === this.localPlayer) {
                this.renderFirstPerson();
            } else {
                this.renderThirdPerson(player, screenPos);
            }
        }
    }
    
    renderFirstPerson() {
    }
    
    renderThirdPerson(player, screenPos) {
    }
    
    renderEffects() {
        this.renderer.updateParticles(this.deltaTime);
        this.renderer.updateProjectiles(this.deltaTime);
        this.renderer.updateTracers(this.deltaTime);
    }
    
    renderUI() {
        this.ui.render(this.ctx);
    }
    
    renderDebug() {
        if (!this.localPlayer) return;
        
        const text = `Position: (${this.localPlayer.position.x.toFixed(2)}, ${this.localPlayer.position.y.toFixed(2)}, ${this.localPlayer.position.z.toFixed(2)})`;
        
        this.ctx.fillStyle = '#00ff00';
        this.ctx.font = '14px monospace';
        this.ctx.fillText(text, 10, 50);
    }
    
    renderFPS() {
        const text = `FPS: ${this.fps}`;
        
        this.ctx.fillStyle = '#00ff00';
        this.ctx.font = '16px Arial';
        this.ctx.fillText(text, 10, 10);
    }
    
    showOptions() {
        this.ui.showPanel('settings');
    }
    
    quitGame() {
        console.log('Quitting game...');
        
        this.matchRecorder.stopRecording();
        
        this.state = GAME_STATE.IDLE;
        
        this.cleanup();
    }
    
    cleanup() {
        this.running = false;
        
        if (this.network) {
            this.network.disconnect();
        }
        
        if (this.sound) {
            this.sound.dispose();
        }
        
        if (this.physics) {
            this.physics.clear();
        }
        
        this.players.clear();
        this.bots.clear();
        
        this.playerManager.clear();
        this.botManager.clear();
        this.weaponManager.clear();
    }
    
    gameLoop(currentTime) {
        if (!this.lastFrameTime) {
            this.lastFrameTime = currentTime;
        }
        
        this.deltaTime = (currentTime - this.lastFrameTime) / 1000;
        this.lastFrameTime = currentTime;
        
        this.deltaTime = Math.min(this.deltaTime, 0.1);
        
        this.update(this.deltaTime);
        
        this.render();
        
        if (this.running) {
            requestAnimationFrame((time) => this.gameLoop(time));
        }
    }
    
    start() {
        console.log('Starting game loop');
        this.running = true;
        this.lastFrameTime = 0;
        requestAnimationFrame((time) => this.gameLoop(time));
    }
    
    stop() {
        console.log('Stopping game loop');
        this.running = false;
    }
}

export class GameFactory {
    static createGame(gameType = 'standard') {
        const game = new Game();
        
        switch (gameType) {
            case 'competitive':
                game.mode = GAME_MODE.COMPETITIVE;
                break;
            case 'casual':
                game.mode = GAME_MODE.CASUAL;
                break;
            case 'deathmatch':
                game.mode = GAME_MODE.DEATHMATCH;
                break;
            case 'custom':
                game.mode = GAME_MODE.CUSTOM;
                break;
            default:
                game.mode = GAME_MODE.STANDARD;
        }
        
        return game;
    }
}

export class GameSettings {
    constructor() {
        this.graphics = {
            resolution: { width: 1920, height: 1080 },
            fullscreen: false,
            vsync: true,
            antiAliasing: true,
            shadowQuality: 'high',
            textureQuality: 'high',
            particleQuality: 'high',
            viewDistance: 10000,
            ambientOcclusion: true,
            bloom: true,
            motionBlur: false,
            filmGrain: false
        };
        
        this.audio = {
            masterVolume: 1.0,
            musicVolume: 0.5,
            effectsVolume: 0.8,
            voiceVolume: 1.0,
            footstepVolume: 0.6,
            ambientVolume: 0.3,
            spatialAudio: true,
            voiceChat: true,
            notifications: true
        };
        
        this.controls = {
            mouseSensitivity: 2.0,
            mouseAcceleration: false,
            invertedMouse: false,
            rawInput: true,
            keyBindings: {},
            gamepadEnabled: true,
            gamepadSensitivity: 1.0
        };
        
        this.gameplay = {
            showDamage: true,
            showCrosshair: true,
            showRadar: true,
            showSpectatorCount: true,
            showKillFeed: true,
            autoReload: true,
            autoSwitchWeapon: false,
            grenadeTrajectory: true,
            demoRecording: false
        };
        
        this.network = {
            clUpdaterate: 128,
            clInterpRatio: 2,
            clInterp: 0.1,
            rate: 786432,
            maxAcceptableLatency: 100,
            packetCompression: true
        };
    }
    
    load() {
        const saved = localStorage.getItem('gameSettings');
        if (saved) {
            try {
                const data = JSON.parse(saved);
                this.merge(data);
            } catch (e) {
                console.error('Failed to load settings:', e);
            }
        }
    }
    
    save() {
        try {
            localStorage.setItem('gameSettings', JSON.stringify(this));
        } catch (e) {
            console.error('Failed to save settings:', e);
        }
    }
    
    merge(data) {
        if (data.graphics) Object.assign(this.graphics, data.graphics);
        if (data.audio) Object.assign(this.audio, data.audio);
        if (data.controls) Object.assign(this.controls, data.controls);
        if (data.gameplay) Object.assign(this.gameplay, data.gameplay);
        if (data.network) Object.assign(this.network, data.network);
    }
    
    reset() {
        this.graphics = {
            resolution: { width: 1920, height: 1080 },
            fullscreen: false,
            vsync: true,
            antiAliasing: true,
            shadowQuality: 'high',
            textureQuality: 'high',
            particleQuality: 'high',
            viewDistance: 10000,
            ambientOcclusion: true,
            bloom: true,
            motionBlur: false,
            filmGrain: false
        };
        
        this.audio = {
            masterVolume: 1.0,
            musicVolume: 0.5,
            effectsVolume: 0.8,
            voiceVolume: 1.0,
            footstepVolume: 0.6,
            ambientVolume: 0.3,
            spatialAudio: true,
            voiceChat: true,
            notifications: true
        };
        
        this.controls = {
            mouseSensitivity: 2.0,
            mouseAcceleration: false,
            invertedMouse: false,
            rawInput: true,
            keyBindings: {},
            gamepadEnabled: true,
            gamepadSensitivity: 1.0
        };
        
        this.gameplay = {
            showDamage: true,
            showCrosshair: true,
            showRadar: true,
            showSpectatorCount: true,
            showKillFeed: true,
            autoReload: true,
            autoSwitchWeapon: false,
            grenadeTrajectory: true,
            demoRecording: false
        };
        
        this.network = {
            clUpdaterate: 128,
            clInterpRatio: 2,
            clInterp: 0.1,
            rate: 786432,
            maxAcceptableLatency: 100,
            packetCompression: true
        };
    }
    
    export() {
        return JSON.stringify(this, null, 2);
    }
    
    import(json) {
        try {
            const data = JSON.parse(json);
            this.merge(data);
            return true;
        } catch (e) {
            console.error('Failed to import settings:', e);
            return false;
        }
    }
}

export class ProfileManager {
    constructor() {
        this.profiles = new Map();
        this.currentProfile = null;
    }
    
    createProfile(name, settings = null) {
        const profile = {
            id: Math.random().toString(36).substring(2, 15),
            name: name,
            settings: settings || new GameSettings(),
            stats: this.createDefaultStats(),
            inventory: [],
            achievements: [],
            unlockedItems: [],
            createdAt: Date.now(),
            lastPlayed: Date.now(),
            playTime: 0
        };
        
        this.profiles.set(profile.id, profile);
        
        return profile;
    }
    
    createDefaultStats() {
        return {
            totalKills: 0,
            totalDeaths: 0,
            totalAssists: 0,
            totalHeadshots: 0,
            totalMVPs: 0,
            totalRoundsPlayed: 0,
            totalMatchesPlayed: 0,
            matchesWon: 0,
            matchesLost: 0,
            favoriteMap: null,
            favoriteWeapon: null,
            totalDamage: 0,
            totalPlayTime: 0,
            rank: 1,
            xp: 0,
            level: 1
        };
    }
    
    selectProfile(profileId) {
        const profile = this.profiles.get(profileId);
        if (profile) {
            this.currentProfile = profile;
            profile.lastPlayed = Date.now();
            return true;
        }
        return false;
    }
    
    updateStats(stats) {
        if (!this.currentProfile) return;
        
        Object.assign(this.currentProfile.stats, stats);
    }
    
    addAchievement(achievementId) {
        if (!this.currentProfile) return;
        
        if (!this.currentProfile.achievements.includes(achievementId)) {
            this.currentProfile.achievements.push(achievementId);
        }
    }
    
    addXP(amount) {
        if (!this.currentProfile) return;
        
        this.currentProfile.stats.xp += amount;
        
        const xpForLevel = this.calculateXPForLevel(this.currentProfile.stats.level + 1);
        while (this.currentProfile.stats.xp >= xpForLevel) {
            this.currentProfile.stats.xp -= xpForLevel;
            this.currentProfile.stats.level++;
            xpForLevel = this.calculateXPForLevel(this.currentProfile.stats.level + 1);
        }
    }
    
    calculateXPForLevel(level) {
        return 1000 * level * level;
    }
    
    getRank() {
        if (!this.currentProfile) return null;
        
        const level = this.currentProfile.stats.level;
        
        const ranks = [
            { name: 'Silver I', level: 1 },
            { name: 'Silver II', level: 5 },
            { name: 'Silver III', level: 10 },
            { name: 'Silver IV', level: 15 },
            { name: 'Gold Nova I', level: 20 },
            { name: 'Gold Nova II', level: 25 },
            { name: 'Gold Nova III', level: 30 },
            { name: 'Gold Nova IV', level: 35 },
            { name: 'Master Guardian I', level: 40 },
            { name: 'Master Guardian II', level: 45 },
            { name: 'Master Guardian Elite', level: 50 },
            { name: 'Distinguished Master Guardian', level: 60 },
            { name: 'Legendary Eagle', level: 70 },
            { name: 'Legendary Eagle Master', level: 80 },
            { name: 'Supreme Master First Class', level: 90 },
            { name: 'The Global Elite', level: 100 }
        ];
        
        for (let i = ranks.length - 1; i >= 0; i--) {
            if (level >= ranks[i].level) {
                return ranks[i];
            }
        }
        
        return ranks[0];
    }
    
    save() {
        const data = {
            profiles: Array.from(this.profiles.entries()),
            currentProfile: this.currentProfile?.id
        };
        
        try {
            localStorage.setItem('gameProfiles', JSON.stringify(data));
        } catch (e) {
            console.error('Failed to save profiles:', e);
        }
    }
    
    load() {
        try {
            const saved = localStorage.getItem('gameProfiles');
            if (saved) {
                const data = JSON.parse(saved);
                
                this.profiles = new Map(data.profiles);
                
                if (data.currentProfile) {
                    this.currentProfile = this.profiles.get(data.currentProfile);
                }
            }
        } catch (e) {
            console.error('Failed to load profiles:', e);
        }
    }
}

export class DemoRecorder {
    constructor() {
        this.recording = false;
        this.frames = [];
        this.startTime = 0;
        this.endTime = 0;
        this.tickRate = 128;
        this.tick = 0;
    }
    
    startRecording() {
        this.recording = true;
        this.frames = [];
        this.startTime = Date.now();
        this.tick = 0;
    }
    
    stopRecording() {
        this.recording = false;
        this.endTime = Date.now();
    }
    
    recordFrame(gameState) {
        if (!this.recording) return;
        
        this.frames.push({
            tick: this.tick,
            time: Date.now() - this.startTime,
            state: this.serializeState(gameState)
        });
        
        this.tick++;
    }
    
    serializeState(state) {
        return {
            players: state.players,
            round: state.round,
            scores: state.teamScores,
            bomb: state.bomb
        };
    }
    
    getDemoData() {
        return {
            header: {
                version: 6,
                game: 'Counter-Strike',
                map: 'de_dust2',
                tickRate: this.tickRate,
                tickCount: this.tick,
                duration: this.endTime - this.startTime,
                frames: this.frames.length
            },
            frames: this.frames
        };
    }
    
    export() {
        return JSON.stringify(this.getDemoData());
    }
    
    import(data) {
        try {
            const demo = JSON.parse(data);
            return demo;
        } catch (e) {
            console.error('Failed to import demo:', e);
            return null;
        }
    }
}

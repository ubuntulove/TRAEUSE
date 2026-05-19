import { GAME_CONFIG, PLAYER_CONFIG, TEAM, GAME_STATE, WEAPON_TYPE, DAMAGE_TYPE } from './config.js';

export class Player {
    constructor(id, name, team, isBot = false) {
        this.id = id;
        this.name = name;
        this.team = team;
        this.isBot = isBot;
        
        this.position = { x: 0, y: 0, z: 0 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.rotation = { pitch: 0, yaw: 0, roll: 0 };
        this.viewOffset = { x: 0, y: 0, z: PLAYER_CONFIG.HEIGHT };
        
        this.health = PLAYER_CONFIG.MAX_HEALTH;
        this.armor = PLAYER_CONFIG.MAX_ARMOR;
        this.helmet = false;
        this.hasKit = false;
        
        this.weapons = {
            primary: null,
            secondary: null,
            knife: null,
            grenade: [],
            bomb: null,
            defuseKit: false
        };
        
        this.activeWeapon = null;
        this.previousWeapon = null;
        
        this.ammo = {
            primary: { current: 0, max: 0, reserve: 0 },
            secondary: { current: 0, max: 0, reserve: 0 }
        };
        
        this.money = PLAYER_CONFIG.STARTING_MONEY;
        this.score = {
            kills: 0,
            deaths: 0,
            assists: 0,
            headshots: 0,
            mvps: 0
        };
        
        this.state = {
            alive: true,
            inBombZone: false,
            inRescueZone: false,
            inBuyZone: true,
            blind: 0,
            burning: 0,
            frozen: 0,
            jumping: false,
            crouching: false,
            walking: false,
            scoped: false,
            reload: false,
            reloading: false,
            planting: false,
            defusing: false,
            hostageInteraction: false,
            inAir: false,
            grounded: true,
            vaulting: false
        };
        
        this.movement = {
            speed: PLAYER_CONFIG.MOVE_SPEED,
            direction: { x: 0, y: 0 },
            moveType: 0,
            groundEntity: -1
        };
        
        this.viewModel = {
            bobCycle: 0,
            bobOffset: { x: 0, y: 0, z: 0 },
            weaponKick: { x: 0, y: 0, z: 0 }
        };
        
        this.inventory = {
            slots: new Array(5).fill(null),
            grenadeSlots: new Array(4).fill(null)
        };
        
        this.buyZoneHistory = [];
        this.lastKillTime = 0;
        this.lastDamageGiven = [];
        this.lastDamageTaken = [];
        
        this.networkData = {
            tick: 0,
            latency: 0,
            packetLoss: 0,
            choke: 0
        };
        
        this.animation = {
            state: 'idle',
            sequence: 0,
            frame: 0,
            blend: 0,
            playbackRate: 1.0
        };
        
        this.audio = {
            footstepType: 0,
            lastFootstep: 0,
            voiceEnabled: true,
            muted: false
        };
        
        this.flashEffects = {
            flashAlpha: 0,
            flashDuration: 0,
            flashStartTime: 0
        };
        
        this.smokeEffects = {
            smokeAlpha: 0,
            smokeDuration: 0,
            smokeStartTime: 0
        };
        
        this.molotovEffects = {
            burning: false,
            burnDamage: 0,
            burnStartTime: 0
        };
        
        this.ducking = {
            ducking: false,
            duckAmount: 0,
            inDuckAnimation: false,
            timeOfDuckStart: 0
        };
        
        this.spectating = {
            isSpectating: false,
            spectateTarget: null,
            spectateMode: 'firstperson'
        };
        
        this.matchStats = {
            killsThisRound: 0,
            deathsThisRound: 0,
            damageGivenThisRound: 0,
            damageTakenThisRound: 0,
            shotsFired: 0,
            shotsHit: 0,
            accuracy: 0
        };
        
        this.hitboxes = this.initializeHitboxes();
        this.bones = this.initializeBones();
        
        this.activeWeaponSlot = 0;
        this.cycleTimer = 0;
        this.weaponSelectTimer = 0;
        
        this.lastAimPunch = { x: 0, y: 0 };
        self.lastVelocityPunch = { x: 0, y: 0, z: 0 };
        
        this.inputState = {
            forward: 0,
            backward: 0,
            left: 0,
            right: 0,
            jump: false,
            duck: false,
            walk: false,
            sprint: false
        };
        
        this.mouseState = {
            buttons: 0,
            lastX: 0,
            lastY: 0,
            deltaX: 0,
            deltaY: 0,
            sensitivity: 2.0,
            accelEnabled: true
        };
        
        this.weaponSway = {
            offset: { x: 0, y: 0, z: 0 },
            angles: { x: 0, y: 0, z: 0 },
            time: 0
        };
        
        this.recoil = {
            angle: { x: 0, y: 0 },
            pattern: [],
            index: 0
        };
        
        this.grenadeTrajectory = [];
        this.lastGrenadeTime = 0;
        
        this.voteKick = {
            initiated: false,
            initiatorId: null,
            votes: [],
            timeRemaining: 0
        };
        
        this.chatMute = {
            muted: false,
            mutedBy: []
        };
        
        this.clanTag = '';
        this.avatarUrl = '';
        this.rank = 0;
        this.commendation = { friendly: 0, teaching: 0, leader: 0 };
        
        this.gameStats = {
            timePlayed: 0,
            roundsPlayed: 0,
            lastMatchTime: 0,
            favoriteWeapon: null,
            totalDamage: 0,
            totalHeadshots: 0,
            winRate: 0,
            kdr: 0
        };
        
        this.inventoryVisual = {
            show: false,
            selectedSlot: 0,
            cursorPosition: { x: 0, y: 0 },
            dragItem: null
        };
        
        this.buyMenu = {
            show: false,
            category: 'assault',
            scrollPosition: 0,
            selectedItem: null,
            itemGrid: []
        };
        
        this.radar = {
            enabled: true,
            zoom: 1.0,
            centerOnPlayer: true,
            showNames: true,
            showHealth: true,
            showWeapons: false,
            icons: []
        };
        
        this.killFeed = {
            entries: [],
            maxEntries: 5,
            fadeTime: 5000
        };
        
        this.damageIndicator = {
            show: false,
            direction: { x: 0, y: 0 },
            damage: 0,
            color: '#ff0000',
            duration: 500
        };
        
        this.statusEffects = {
            poisoned: false,
            poisonedDamage: 0,
            poisonedDuration: 0,
            shocked: false,
            shockedStunDuration: 0,
            burned: false,
            burnDamage: 0,
            burnDuration: 0,
            bleeding: false,
            bleedDamage: 0,
            bleedDuration: 0
        };
        
        this.respawn = {
            needed: false,
            time: 0,
            ghostPosition: { x: 0, y: 0, z: 0 },
            ghostRotation: { pitch: 0, yaw: 0, roll: 0 }
        };
        
        this.shooting = {
            canShoot: true,
            lastShotTime: 0,
            shotsThisBurst: 0,
            burstMode: false
        };
        
        this.reloading = {
            reloading: false,
            reloadStartTime: 0,
            reloadDuration: 0,
            bulletsToLoad: 0
        };
        
        this.switchingWeapon = {
            switching: false,
            switchStartTime: 0,
            switchDuration: 0,
            fromWeapon: null,
            toWeapon: null
        };
        
        this.planting = {
            planting: false,
            plantStartTime: 0,
            plantDuration: PLAYER_CONFIG.PLANT_TIME,
            plantProgress: 0,
            canCancel: true
        };
        
        this.defusing = {
            defusing: false,
            defuseStartTime: 0,
            defuseDuration: PLAYER_CONFIG.DEFUSE_TIME_REGULAR,
            defuseProgress: 0,
            hasKit: false,
            canCancel: true
        };
        
        this.hostageInteraction = {
            interacting: false,
            interactStartTime: 0,
            interactDuration: 500,
            interactProgress: 0,
            currentHostage: null
        };
        
        this.velocityModifier = {
            speed: 1.0,
            acceleration: 1.0,
            gravity: 1.0,
            jumpVelocity: 1.0,
            friction: 1.0
        };
        
        this.footstepSound = {
            lastFootstepTime: 0,
            footstepInterval: PLAYER_CONFIG.FOOTSTEP_INTERVAL,
            volume: 1.0,
            surface: 'concrete'
        };
        
        this.weaponProficiency = {};
        
        this.inventoryManagement = {
            droppedWeapons: [],
            pickedUpWeapons: [],
            droppedGrenades: []
        };
        
        this.chat = {
            messages: [],
            maxMessages: 50,
            cooldowns: {
                global: 0,
                team: 0,
                voice: 0
            }
        };
        
        this.spray = {
            pattern: [],
            angle: 0,
            showing: false,
            cooldown: 0
        };
        
        this.achievementTracker = {
            unlocked: [],
            progress: {},
            recentUnlocks: []
        };
        
        this.tutorial = {
            hints: [],
            completedObjectives: [],
            currentObjective: null,
            shownTips: []
        };
        
        this.customization = {
            weaponSkin: null,
            playerModel: 'default',
            gloveModel: 'default',
            agentSkin: null,
            sprayTexture: null,
            musicKit: null,
            rankEmblem: null
        };
        
        this.movementHistory = [];
        this.positionHistory = [];
        this.velocityHistory = [];
        this.rotationHistory = [];
        
        this.spectatorCount = 0;
        this.connected = true;
        this.steamId = '';
        this.ipAddress = '';
        this.country = '';
        this.ping = 0;
        
        this.networkQuality = {
            goodPackets: 0,
            chokedPackets: 0,
            lostPackets: 0,
            lastUpdateTime: 0
        };
        
        this.inputPrediction = {
            enabled: true,
            corrections: 0,
            errors: []
        };
        
        this.interpolation = {
            enabled: true,
            delay: GAME_CONFIG.NETWORK_INTERPOLATION_TIME,
            bufferSize: GAME_CONFIG.NETWORK_INTERPOLATION_BUFFER
        };
    }
    
    initializeHitboxes() {
        return {
            head: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 8 },
            neck: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 5 },
            chest: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 15 },
            stomach: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 12 },
            leftArm: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 6 },
            rightArm: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 6 },
            leftLeg: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 7 },
            rightLeg: { min: { x: 0, y: 0, z: 0 }, max: { x: 0, y: 0, z: 0 }, radius: 7 }
        };
    }
    
    initializeBones() {
        return {
            root: { x: 0, y: 0, z: 0 },
            pelvis: { x: 0, y: 0, z: 50 },
            spine: { x: 0, y: 0, z: 60 },
            chest: { x: 0, y: 0, z: 70 },
            neck: { x: 0, y: 0, z: 80 },
            head: { x: 0, y: 0, z: 90 },
            leftShoulder: { x: -15, y: 0, z: 75 },
            rightShoulder: { x: 15, y: 0, z: 75 },
            leftElbow: { x: -25, y: 0, z: 60 },
            rightElbow: { x: 25, y: 0, z: 60 },
            leftHand: { x: -30, y: 0, z: 45 },
            rightHand: { x: 30, y: 0, z: 45 },
            leftHip: { x: -8, y: 0, z: 50 },
            rightHip: { x: 8, y: 0, z: 50 },
            leftKnee: { x: -8, y: 0, z: 30 },
            rightKnee: { x: 8, y: 0, z: 30 },
            leftFoot: { x: -8, y: 0, z: 0 },
            rightFoot: { x: 8, y: 0, z: 0 }
        };
    }
    
    update(deltaTime) {
        if (!this.state.alive) {
            this.updateSpectator(deltaTime);
            return;
        }
        
        this.updateMovement(deltaTime);
        this.updatePhysics(deltaTime);
        this.updateViewModel(deltaTime);
        this.updateWeapon(deltaTime);
        this.updateStatusEffects(deltaTime);
        this.updateAnimation(deltaTime);
        this.updateNetworkData(deltaTime);
        this.updateInventory(deltaTime);
        
        this.recordHistory();
    }
    
    updateMovement(deltaTime) {
        if (this.state.jumping) {
            this.handleJump();
        }
        
        if (this.state.crouching) {
            this.handleCrouch();
        }
        
        this.calculateMovementDirection();
        this.applyMovementSpeed();
        this.updateGroundState();
        this.handleFootsteps(deltaTime);
    }
    
    handleJump() {
        if (this.state.grounded && this.inputState.jump) {
            this.velocity.z = PLAYER_CONFIG.JUMP_VELOCITY * this.velocityModifier.jumpVelocity;
            this.state.grounded = false;
            this.state.jumping = true;
            this.state.inAir = true;
        }
        
        if (!this.state.grounded) {
            this.velocity.z -= GAME_CONFIG.GRAVITY * deltaTime * this.velocityModifier.gravity;
        }
    }
    
    handleCrouch() {
        const crouchSpeed = PLAYER_CONFIG.MOVE_SPEED * PLAYER_CONFIG.CROUCH_SPEED_MULTIPLIER;
        
        if (this.state.crouching && !this.ducking.ducking) {
            this.ducking.ducking = true;
            this.ducking.duckAmount = 1.0;
            this.ducking.timeOfDuckStart = Date.now();
        }
        
        if (this.state.crouching) {
            this.viewOffset.z = PLAYER_CONFIG.CROUCH_HEIGHT;
            this.movement.speed = crouchSpeed;
        } else {
            this.viewOffset.z = PLAYER_CONFIG.HEIGHT;
            this.movement.speed = PLAYER_CONFIG.MOVE_SPEED;
        }
        
        if (this.ducking.ducking) {
            const duckProgress = Math.min((Date.now() - this.ducking.timeOfDuckStart) / 200, 1);
            this.ducking.duckAmount = this.state.crouching ? 1.0 - duckProgress : duckProgress;
        }
    }
    
    calculateMovementDirection() {
        let moveX = 0;
        let moveY = 0;
        
        if (this.inputState.forward) moveY += 1;
        if (this.inputState.backward) moveY -= 1;
        if (this.inputState.left) moveX -= 1;
        if (this.inputState.right) moveX += 1;
        
        const length = Math.sqrt(moveX * moveX + moveY * moveY);
        if (length > 0) {
            moveX /= length;
            moveY /= length;
        }
        
        const sin = Math.sin(this.rotation.yaw);
        const cos = Math.cos(this.rotation.yaw);
        
        this.movement.direction.x = moveX * cos - moveY * sin;
        this.movement.direction.y = moveX * sin + moveY * cos;
    }
    
    applyMovementSpeed() {
        let speedMultiplier = 1.0;
        
        if (this.state.crouching) speedMultiplier *= PLAYER_CONFIG.CROUCH_SPEED_MULTIPLIER;
        if (this.state.walking) speedMultiplier *= PLAYER_CONFIG.WALK_SPEED_MULTIPLIER;
        if (this.state.scoped) speedMultiplier *= PLAYER_CONFIG.SCOPED_SPEED_MULTIPLIER;
        
        speedMultiplier *= this.velocityModifier.speed;
        
        const targetVelocityX = this.movement.direction.x * this.movement.speed * speedMultiplier;
        const targetVelocityY = this.movement.direction.y * this.movement.speed * speedMultiplier;
        
        this.velocity.x = this.velocity.x * (1 - this.velocityModifier.friction * 0.1) + targetVelocityX * 0.1;
        this.velocity.y = this.velocity.y * (1 - this.velocityModifier.friction * 0.1) + targetVelocityY * 0.1;
    }
    
    updateGroundState() {
        if (this.position.z <= GAME_CONFIG.GROUND_LEVEL) {
            this.position.z = GAME_CONFIG.GROUND_LEVEL;
            this.velocity.z = 0;
            this.state.grounded = true;
            this.state.inAir = false;
            this.state.jumping = false;
        }
    }
    
    handleFootsteps(deltaTime) {
        if (!this.state.grounded || this.state.crouching) return;
        
        const isMoving = Math.abs(this.velocity.x) > 0.1 || Math.abs(this.velocity.y) > 0.1;
        
        if (isMoving) {
            const currentTime = Date.now();
            const interval = this.footstepSound.footstepInterval / (this.state.walking ? 1.5 : 1.0);
            
            if (currentTime - this.footstepSound.lastFootstepTime > interval) {
                this.playFootstepSound();
                this.footstepSound.lastFootstepTime = currentTime;
            }
        }
    }
    
    playFootstepSound() {
        const volume = this.state.walking ? 0.5 : 1.0;
    }
    
    updatePhysics(deltaTime) {
        this.position.x += this.velocity.x * deltaTime;
        this.position.y += this.velocity.y * deltaTime;
        this.position.z += this.velocity.z * deltaTime;
        
        if (this.position.z > GAME_CONFIG.GROUND_LEVEL) {
            this.velocity.z -= GAME_CONFIG.GRAVITY * deltaTime;
        }
        
        this.handleCollisions();
        this.handleBoundaries();
    }
    
    handleCollisions() {
        if (this.position.z < GAME_CONFIG.GROUND_LEVEL) {
            this.position.z = GAME_CONFIG.GROUND_LEVEL;
            this.velocity.z = 0;
        }
    }
    
    handleBoundaries() {
        const mapBounds = {
            minX: -1000,
            maxX: 1000,
            minY: -1000,
            maxY: 1000
        };
        
        this.position.x = Math.max(mapBounds.minX, Math.min(mapBounds.maxX, this.position.x));
        this.position.y = Math.max(mapBounds.minY, Math.min(mapBounds.maxY, this.position.y));
    }
    
    updateViewModel(deltaTime) {
        this.updateWeaponSway(deltaTime);
        this.updateRecoil(deltaTime);
        this.updateBob(deltaTime);
        this.updateAimPunch(deltaTime);
    }
    
    updateWeaponSway(deltaTime) {
        this.weaponSway.time += deltaTime;
        
        const isMoving = Math.abs(this.velocity.x) > 0.1 || Math.abs(this.velocity.y) > 0.1;
        const swayIntensity = isMoving ? 1.5 : 0.5;
        
        this.weaponSway.offset.x = Math.sin(this.weaponSway.time * 2) * swayIntensity;
        this.weaponSway.offset.y = Math.cos(this.weaponSway.time * 3) * swayIntensity * 0.5;
        
        const pitchVelocity = this.mouseState.deltaY * 0.1;
        const yawVelocity = this.mouseState.deltaX * 0.1;
        
        this.weaponSway.angles.x -= pitchVelocity * 0.1;
        this.weaponSway.angles.y -= yawVelocity * 0.1;
        
        this.weaponSway.angles.x *= 0.9;
        this.weaponSway.angles.y *= 0.9;
    }
    
    updateRecoil(deltaTime) {
        this.recoil.angle.x *= PLAYER_CONFIG.RECOIL_RECOVERY_RATE;
        this.recoil.angle.y *= PLAYER_CONFIG.RECOIL_RECOVERY_RATE;
        
        if (Math.abs(this.recoil.angle.x) < 0.01) this.recoil.angle.x = 0;
        if (Math.abs(this.recoil.angle.y) < 0.01) this.recoil.angle.y = 0;
    }
    
    updateBob(deltaTime) {
        if (!this.state.grounded) {
            this.viewModel.bobCycle = 0;
            return;
        }
        
        const speed = Math.sqrt(this.velocity.x * this.velocity.x + this.velocity.y * this.velocity.y);
        this.viewModel.bobCycle += speed * deltaTime * PLAYER_CONFIG.BOB_CYCLE_SPEED;
        
        this.viewModel.bobOffset.x = Math.sin(this.viewModel.bobCycle) * PLAYER_CONFIG.BOB_AMOUNT_X * (speed / PLAYER_CONFIG.MOVE_SPEED);
        this.viewModel.bobOffset.y = Math.cos(this.viewModel.bobCycle * 2) * PLAYER_CONFIG.BOB_AMOUNT_Y * (speed / PLAYER_CONFIG.MOVE_SPEED);
    }
    
    updateAimPunch(deltaTime) {
        this.lastAimPunch.x *= 0.95;
        this.lastAimPunch.y *= 0.95;
    }
    
    updateWeapon(deltaTime) {
        if (this.reloading.reloading) {
            this.updateReload(deltaTime);
        }
        
        if (this.switchingWeapon.switching) {
            this.updateWeaponSwitch(deltaTime);
        }
        
        if (this.planting.planting) {
            this.updatePlanting(deltaTime);
        }
        
        if (this.defusing.defusing) {
            this.updateDefusing(deltaTime);
        }
    }
    
    updateReload(deltaTime) {
        const elapsed = Date.now() - this.reloading.reloadStartTime;
        const progress = Math.min(elapsed / this.reloading.reloadDuration, 1);
        
        if (progress >= 1) {
            this.completeReload();
        }
    }
    
    completeReload() {
        this.reloading.reloading = false;
        this.state.reloading = false;
        
        if (this.activeWeapon && this.reloading.bulletsToLoad > 0) {
            this.ammo.primary.current = this.reloading.bulletsToLoad;
        }
    }
    
    updateWeaponSwitch(deltaTime) {
        const elapsed = Date.now() - this.switchingWeapon.switchStartTime;
        const progress = Math.min(elapsed / this.switchingWeapon.switchDuration, 1);
        
        if (progress >= 1) {
            this.completeWeaponSwitch();
        }
    }
    
    completeWeaponSwitch() {
        this.switchingWeapon.switching = false;
        this.activeWeapon = this.switchingWeapon.toWeapon;
        this.state.reload = false;
    }
    
    updatePlanting(deltaTime) {
        if (!this.state.alive) {
            this.cancelPlanting();
            return;
        }
        
        const elapsed = Date.now() - this.planting.plantStartTime;
        this.planting.plantProgress = Math.min(elapsed / this.planting.plantDuration, 1);
        
        if (this.planting.plantProgress >= 1) {
            this.completePlanting();
        }
    }
    
    completePlanting() {
        this.planting.planting = false;
        this.state.planting = false;
    }
    
    cancelPlanting() {
        this.planting.planting = false;
        this.state.planting = false;
        this.planting.plantProgress = 0;
    }
    
    updateDefusing(deltaTime) {
        if (!this.state.alive) {
            this.cancelDefusing();
            return;
        }
        
        const elapsed = Date.now() - this.defusing.defuseStartTime;
        this.defusing.defuseProgress = Math.min(elapsed / this.defusing.defuseDuration, 1);
        
        if (this.defusing.defuseProgress >= 1) {
            this.completeDefusing();
        }
    }
    
    completeDefusing() {
        this.defusing.defusing = false;
        this.state.defusing = false;
    }
    
    cancelDefusing() {
        this.defusing.defusing = false;
        this.state.defusing = false;
        this.defusing.defuseProgress = 0;
    }
    
    updateStatusEffects(deltaTime) {
        if (this.state.blind > 0) {
            this.state.blind -= deltaTime;
            if (this.state.blind < 0) this.state.blind = 0;
        }
        
        if (this.flashEffects.flashAlpha > 0) {
            const elapsed = Date.now() - this.flashEffects.flashStartTime;
            this.flashEffects.flashAlpha = Math.max(0, 1 - elapsed / this.flashEffects.flashDuration);
        }
        
        if (this.smokeEffects.smokeAlpha > 0) {
            const elapsed = Date.now() - this.smokeEffects.smokeStartTime;
            this.smokeEffects.smokeAlpha = Math.max(0, 1 - elapsed / this.smokeEffects.smokeDuration);
        }
        
        if (this.molotovEffects.burning) {
            const elapsed = Date.now() - this.molotovEffects.burnStartTime;
            if (elapsed < this.molotovEffects.burnDuration) {
                this.takeDamage(this.molotovEffects.burnDamage * deltaTime, DAMAGE_TYPE.BURN);
            } else {
                this.molotovEffects.burning = false;
            }
        }
    }
    
    updateAnimation(deltaTime) {
        if (!this.state.alive) return;
        
        this.animation.frame += this.animation.playbackRate * deltaTime;
        
        if (this.animation.frame >= this.getAnimationLength(this.animation.sequence)) {
            this.animation.frame = 0;
        }
    }
    
    getAnimationLength(sequence) {
        const animationLengths = {
            idle: 2.0,
            walk: 0.5,
            run: 0.3,
            jump: 0.5,
            crouch: 0.5,
            shoot: 0.1,
            reload: 2.0,
            knife_slash: 0.5,
            plant: 3.0,
            defuse: 5.0
        };
        
        return animationLengths[this.animation.state] || 1.0;
    }
    
    updateNetworkData(deltaTime) {
        this.networkData.tick += 1;
        
        this.networkQuality.lastUpdateTime += deltaTime;
        if (this.networkQuality.lastUpdateTime > 1.0) {
            this.networkQuality.lastUpdateTime = 0;
        }
    }
    
    updateInventory(deltaTime) {
        if (this.state.planting && this.weapons.bomb) {
            this.grenadeTrajectory.push({
                position: { ...this.position },
                time: Date.now()
            });
        }
    }
    
    recordHistory() {
        this.positionHistory.push({
            position: { ...this.position },
            time: Date.now()
        });
        
        this.velocityHistory.push({
            velocity: { ...this.velocity },
            time: Date.now()
        });
        
        this.rotationHistory.push({
            rotation: { ...this.rotation },
            time: Date.now()
        });
        
        const maxHistory = 128;
        if (this.positionHistory.length > maxHistory) {
            this.positionHistory.shift();
        }
        if (this.velocityHistory.length > maxHistory) {
            this.velocityHistory.shift();
        }
        if (this.rotationHistory.length > maxHistory) {
            this.rotationHistory.shift();
        }
    }
    
    updateSpectator(deltaTime) {
        if (!this.spectating.isSpectating) return;
    }
    
    takeDamage(amount, damageType, attackerId = null) {
        if (!this.state.alive) return;
        
        const actualDamage = this.calculateDamage(amount, damageType);
        
        this.health -= actualDamage;
        
        this.lastDamageTaken.push({
            damage: actualDamage,
            type: damageType,
            attacker: attackerId,
            time: Date.now()
        });
        
        if (this.lastDamageTaken.length > 10) {
            this.lastDamageTaken.shift();
        }
        
        if (this.health <= 0) {
            this.die(damageType, attackerId);
        }
    }
    
    calculateDamage(baseDamage, damageType) {
        let damage = baseDamage;
        
        switch (damageType) {
            case DAMAGE_TYPE.BULLET:
                damage *= (1 - this.armor * 0.01);
                break;
            case DAMAGE_TYPE.BURN:
                damage *= 1.0;
                break;
            case DAMAGE_TYPE.EXPLOSION:
                damage *= 1.0;
                break;
        }
        
        return Math.max(1, Math.floor(damage));
    }
    
    heal(amount) {
        this.health = Math.min(PLAYER_CONFIG.MAX_HEALTH, this.health + amount);
    }
    
    addArmor(amount) {
        this.armor = Math.min(PLAYER_CONFIG.MAX_ARMOR, this.armor + amount);
    }
    
    die(damageType, attackerId) {
        this.state.alive = false;
        this.position.z = GAME_CONFIG.GROUND_LEVEL;
        this.velocity = { x: 0, y: 0, z: 0 };
        
        this.respawn.needed = true;
        this.respawn.time = Date.now();
        this.respawn.ghostPosition = { ...this.position };
        this.respawn.ghostRotation = { ...this.rotation };
    }
    
    respawnAt(position, rotation) {
        this.state.alive = true;
        this.health = PLAYER_CONFIG.MAX_HEALTH;
        this.armor = PLAYER_CONFIG.STARTING_ARMOR;
        
        this.position = { ...position };
        this.rotation = { ...rotation };
        this.velocity = { x: 0, y: 0, z: 0 };
        
        this.respawn.needed = false;
        this.state.jumping = false;
        this.state.crouching = false;
        this.state.scoped = false;
        
        this.matchStats.deathsThisRound++;
        this.matchStats.deathsThisRound++;
        this.score.deaths++;
    }
    
    shoot() {
        if (!this.state.alive || !this.activeWeapon) return null;
        
        if (this.reloading.reloading || this.switchingWeapon.switching) {
            return null;
        }
        
        if (this.ammo.primary.current <= 0) {
            this.reload();
            return null;
        }
        
        const recoilX = (Math.random() - 0.5) * PLAYER_CONFIG.RECOIL_SPREAD;
        const recoilY = PLAYER_CONFIG.RECOIL_MAGNITUDE + Math.random() * PLAYER_CONFIG.RECOIL_SPREAD;
        
        this.recoil.angle.x += recoilX;
        this.recoil.angle.y += recoilY;
        
        this.lastAimPunch.x += recoilX * 0.5;
        this.lastAimPunch.y += recoilY * 0.5;
        
        this.ammo.primary.current--;
        
        this.matchStats.shotsFired++;
        
        return {
            position: { ...this.position },
            direction: this.getViewDirection(),
            recoil: { x: recoilX, y: recoilY },
            time: Date.now()
        };
    }
    
    reload() {
        if (!this.activeWeapon || this.reloading.reloading) return;
        
        const weapon = this.activeWeapon;
        const reserve = this.ammo.primary.reserve;
        const needed = weapon.magazineSize - this.ammo.primary.current;
        
        if (needed <= 0 || reserve <= 0) return;
        
        this.reloading.reloading = true;
        this.reloading.reloadStartTime = Date.now();
        this.reloading.reloadDuration = weapon.reloadTime * 1000;
        this.reloading.bulletsToLoad = Math.min(needed, reserve);
        
        this.state.reloading = true;
        this.animation.state = 'reload';
    }
    
    switchWeapon(slot) {
        if (this.switchingWeapon.switching) return;
        
        const newWeapon = this.inventory.slots[slot];
        if (!newWeapon) return;
        
        this.switchingWeapon.switching = true;
        this.switchingWeapon.switchStartTime = Date.now();
        this.switchingWeapon.switchDuration = PLAYER_CONFIG.WEAPON_SWITCH_TIME;
        this.switchingWeapon.fromWeapon = this.activeWeapon;
        this.switchingWeapon.toWeapon = newWeapon;
        
        this.activeWeaponSlot = slot;
    }
    
    equipWeapon(weapon, slot) {
        this.inventory.slots[slot] = weapon;
        
        if (slot === 0) {
            this.weapons.primary = weapon;
        } else if (slot === 1) {
            this.weapons.secondary = weapon;
        } else if (slot === 2) {
            this.weapons.knife = weapon;
        }
        
        if (this.activeWeaponSlot === slot && !this.switchingWeapon.switching) {
            this.activeWeapon = weapon;
        }
    }
    
    dropWeapon(slot) {
        const weapon = this.inventory.slots[slot];
        if (!weapon) return null;
        
        this.inventory.slots[slot] = null;
        
        if (slot === 0) {
            this.weapons.primary = null;
        } else if (slot === 1) {
            this.weapons.secondary = null;
        }
        
        if (this.activeWeaponSlot === slot) {
            this.activeWeapon = null;
        }
        
        this.inventoryManagement.droppedWeapons.push({
            weapon: { ...weapon },
            position: { ...this.position },
            time: Date.now()
        });
        
        return weapon;
    }
    
    pickupWeapon(weapon) {
        for (let i = 0; i < this.inventory.slots.length; i++) {
            if (!this.inventory.slots[i]) {
                this.equipWeapon(weapon, i);
                
                this.inventoryManagement.pickedUpWeapons.push({
                    weapon: { ...weapon },
                    position: { ...this.position },
                    time: Date.now()
                });
                
                return true;
            }
        }
        
        return false;
    }
    
    throwGrenade(grenadeType) {
        if (!this.state.alive) return;
        
        const grenade = this.weapons.grenade.find(g => g && g.type === grenadeType);
        if (!grenade) return;
        
        const index = this.weapons.grenade.indexOf(grenade);
        this.weapons.grenade.splice(index, 1);
        
        this.grenadeTrajectory.push({
            type: grenadeType,
            position: { ...this.position },
            velocity: { ...this.velocity },
            throwTime: Date.now()
        });
        
        this.lastGrenadeTime = Date.now();
    }
    
    plantBomb() {
        if (!this.state.alive || !this.weapons.bomb) return false;
        
        if (!this.state.inBombZone) return false;
        
        this.planting.planting = true;
        this.planting.plantStartTime = Date.now();
        this.planting.plantProgress = 0;
        this.state.planting = true;
        
        return true;
    }
    
    startDefusing(hasKit = false) {
        if (!this.state.alive) return false;
        
        this.defusing.defusing = true;
        this.defusing.defuseStartTime = Date.now();
        this.defusing.defuseProgress = 0;
        this.defusing.hasKit = hasKit;
        this.defusing.defuseDuration = hasKit ? PLAYER_CONFIG.DEFUSE_TIME_KIT : PLAYER_CONFIG.DEFUSE_TIME_REGULAR;
        this.state.defusing = true;
        
        return true;
    }
    
    interactWithHostage(hostage) {
        if (!this.state.alive) return;
        
        this.hostageInteraction.interacting = true;
        this.hostageInteraction.currentHostage = hostage;
        this.hostageInteraction.interactStartTime = Date.now();
        this.hostageInteraction.interactProgress = 0;
        this.state.hostageInteraction = true;
    }
    
    cancelInteraction() {
        this.hostageInteraction.interacting = false;
        this.hostageInteraction.currentHostage = null;
        this.state.hostageInteraction = false;
    }
    
    flashbangEffect(duration, intensity) {
        this.flashEffects.flashAlpha = intensity;
        this.flashEffects.flashDuration = duration;
        this.flashEffects.flashStartTime = Date.now();
        this.state.blind = duration;
    }
    
    smokeEffect(duration, intensity) {
        this.smokeEffects.smokeAlpha = intensity;
        this.smokeEffects.smokeDuration = duration;
        this.smokeEffects.smokeStartTime = Date.now();
    }
    
    molotovEffect(duration, damage) {
        this.molotovEffects.burning = true;
        this.molotovEffects.burnDuration = duration;
        this.molotovEffects.burnDamage = damage;
        this.molotovEffects.burnStartTime = Date.now();
    }
    
    buyWeapon(weaponId, price) {
        if (this.money < price) return false;
        if (!this.state.inBuyZone) return false;
        
        this.money -= price;
        
        return true;
    }
    
    buyItem(itemId, price) {
        if (this.money < price) return false;
        if (!this.state.inBuyZone) return false;
        
        switch (itemId) {
            case 'kevlar':
                this.armor = PLAYER_CONFIG.MAX_ARMOR;
                break;
            case 'helmet':
                this.helmet = true;
                break;
            case 'defuse':
                this.hasKit = true;
                break;
            case 'flash':
                this.weapons.grenade.push({ type: 'flash', count: 1 });
                break;
            case 'smoke':
                this.weapons.grenade.push({ type: 'smoke', count: 1 });
                break;
            case 'he':
                this.weapons.grenade.push({ type: 'he', count: 1 });
                break;
            case 'molotov':
                this.weapons.grenade.push({ type: 'molotov', count: 1 });
                break;
        }
        
        this.money -= price;
        
        this.buyZoneHistory.push({
            item: itemId,
            price: price,
            time: Date.now()
        });
        
        return true;
    }
    
    giveMoney(amount) {
        this.money += amount;
    }
    
    getViewDirection() {
        const pitch = this.rotation.pitch;
        const yaw = this.rotation.yaw;
        
        const x = Math.cos(pitch) * Math.sin(yaw);
        const y = Math.cos(pitch) * Math.cos(yaw);
        const z = Math.sin(pitch);
        
        return { x, y, z };
    }
    
    lookAt(pitch, yaw) {
        this.rotation.pitch = Math.max(-Math.PI / 2, Math.min(Math.PI / 2, pitch));
        this.rotation.yaw = yaw;
    }
    
    addKill(victimId, weaponId, isHeadshot) {
        this.score.kills++;
        this.matchStats.killsThisRound++;
        
        if (isHeadshot) {
            this.score.headshots++;
        }
        
        this.lastKillTime = Date.now();
        
        this.gameStats.totalDamage += this.matchStats.damageGivenThisRound;
        if (isHeadshot) {
            this.gameStats.totalHeadshots++;
        }
    }
    
    addAssist(playerId) {
        this.score.assists++;
    }
    
    addMVP() {
        this.score.mvps++;
    }
    
    calculateKDR() {
        if (this.score.deaths === 0) return this.score.kills;
        return (this.score.kills / this.score.deaths).toFixed(2);
    }
    
    calculateWinRate() {
        if (this.gameStats.roundsPlayed === 0) return 0;
        return ((this.gameStats.roundsPlayed / this.gameStats.timePlayed) * 100).toFixed(1);
    }
    
    getNetworkState() {
        return {
            id: this.id,
            position: { ...this.position },
            rotation: { ...this.rotation },
            velocity: { ...this.velocity },
            health: this.health,
            armor: this.armor,
            alive: this.state.alive,
            weapon: this.activeWeapon ? this.activeWeapon.id : null,
            team: this.team
        };
    }
    
    applyNetworkState(state) {
        if (state.position) this.position = { ...state.position };
        if (state.rotation) this.rotation = { ...state.rotation };
        if (state.velocity) this.velocity = { ...state.velocity };
        if (state.health !== undefined) this.health = state.health;
        if (state.armor !== undefined) this.armor = state.armor;
        if (state.alive !== undefined) this.state.alive = state.alive;
    }
    
    interpolateState(targetState, interpolationTime) {
        const lerp = (start, end, t) => start + (end - start) * t;
        
        const targetTime = Date.now() - interpolationTime;
        
        const prevState = this.positionHistory.find(h => h.time <= targetTime);
        const nextState = this.positionHistory.find(h => h.time > targetTime);
        
        if (!prevState || !nextState) {
            this.applyNetworkState(targetState);
            return;
        }
        
        const t = (targetTime - prevState.time) / (nextState.time - prevState.time);
        
        this.position.x = lerp(prevState.position.x, nextState.position.x, t);
        this.position.y = lerp(prevState.position.y, nextState.position.y, t);
        this.position.z = lerp(prevState.position.z, nextState.position.z, t);
        
        this.rotation.pitch = lerp(prevState.rotation.pitch, nextState.rotation.pitch, t);
        this.rotation.yaw = lerp(prevState.rotation.yaw, nextState.rotation.yaw, t);
    }
    
    getHitboxPosition(hitbox) {
        const offset = this.hitboxes[hitbox];
        return {
            x: this.position.x + offset.min.x + (offset.max.x - offset.min.x) / 2,
            y: this.position.y + offset.min.y + (offset.max.y - offset.min.y) / 2,
            z: this.position.z + offset.min.z + (offset.max.z - offset.min.z) / 2
        };
    }
    
    checkLineOfSight(targetPosition) {
        const direction = {
            x: targetPosition.x - this.position.x,
            y: targetPosition.y - this.position.y,
            z: targetPosition.z - this.position.z
        };
        
        const length = Math.sqrt(
            direction.x * direction.x +
            direction.y * direction.y +
            direction.z * direction.z
        );
        
        if (length === 0) return true;
        
        direction.x /= length;
        direction.y /= length;
        direction.z /= length;
        
        return {
            origin: { ...this.position },
            direction: { ...direction },
            distance: length
        };
    }
    
    getEyePosition() {
        return {
            x: this.position.x,
            y: this.position.y,
            z: this.position.z + this.viewOffset.z
        };
    }
    
    isInBuyZone() {
        return this.state.inBuyZone;
    }
    
    isInBombZone() {
        return this.state.inBombZone;
    }
    
    isInRescueZone() {
        return this.state.inRescueZone;
    }
    
    getCollisionRadius() {
        return this.state.crouching ? PLAYER_CONFIG.CROUCH_SIZE.radius : PLAYER_CONFIG.STAND_SIZE.radius;
    }
    
    getCollisionHeight() {
        return this.state.crouching ? PLAYER_CONFIG.CROUCH_SIZE.height : PLAYER_CONFIG.STAND_SIZE.height;
    }
    
    getMovementSpeed() {
        return Math.sqrt(
            this.velocity.x * this.velocity.x +
            this.velocity.y * this.velocity.y
        );
    }
    
    isMoving() {
        return this.getMovementSpeed() > 0.1;
    }
    
    canAfford(cost) {
        return this.money >= cost;
    }
    
    getAccuracy() {
        if (this.matchStats.shotsFired === 0) return 0;
        return (this.matchStats.shotsHit / this.matchStats.shotsFired * 100).toFixed(1);
    }
    
    resetForNextRound() {
        this.health = PLAYER_CONFIG.MAX_HEALTH;
        this.armor = 0;
        this.helmet = false;
        this.hasKit = false;
        
        this.weapons.primary = null;
        this.weapons.secondary = null;
        this.weapons.bomb = null;
        this.weapons.grenade = [];
        
        this.activeWeapon = null;
        
        this.ammo.primary = { current: 0, max: 0, reserve: 0 };
        this.ammo.secondary = { current: 0, max: 0, reserve: 0 };
        
        this.state.alive = true;
        this.state.inBombZone = false;
        this.state.inRescueZone = false;
        this.state.blind = 0;
        this.state.burning = 0;
        this.state.frozen = 0;
        this.state.scoped = false;
        this.state.planting = false;
        this.state.defusing = false;
        this.state.jumping = false;
        this.state.crouching = false;
        
        this.matchStats.killsThisRound = 0;
        this.matchStats.deathsThisRound = 0;
        this.matchStats.damageGivenThisRound = 0;
        this.matchStats.damageTakenThisRound = 0;
        this.matchStats.shotsFired = 0;
        this.matchStats.shotsHit = 0;
        this.matchStats.accuracy = 0;
        
        this.flashEffects.flashAlpha = 0;
        this.smokeEffects.smokeAlpha = 0;
        this.molotovEffects.burning = false;
        
        this.ducking.ducking = false;
        this.ducking.duckAmount = 0;
        
        this.velocity = { x: 0, y: 0, z: 0 };
        
        this.recoil.angle = { x: 0, y: 0 };
        this.recoil.pattern = [];
        this.recoil.index = 0;
        
        this.lastAimPunch = { x: 0, y: 0 };
        
        this.inventory.slots = new Array(5).fill(null);
        
        this.grenadeTrajectory = [];
        
        this.inventoryManagement.droppedWeapons = [];
        this.inventoryManagement.pickedUpWeapons = [];
        
        this.viewOffset.z = PLAYER_CONFIG.HEIGHT;
    }
    
    resetForNextMatch() {
        this.money = PLAYER_CONFIG.STARTING_MONEY;
        
        this.score = {
            kills: 0,
            deaths: 0,
            assists: 0,
            headshots: 0,
            mvps: 0
        };
        
        this.gameStats = {
            timePlayed: 0,
            roundsPlayed: 0,
            lastMatchTime: 0,
            favoriteWeapon: null,
            totalDamage: 0,
            totalHeadshots: 0,
            winRate: 0,
            kdr: 0
        };
        
        this.achievementTracker = {
            unlocked: [],
            progress: {},
            recentUnlocks: []
        };
        
        this.customization = {
            weaponSkin: null,
            playerModel: 'default',
            gloveModel: 'default',
            agentSkin: null,
            sprayTexture: null,
            musicKit: null,
            rankEmblem: null
        };
        
        this.resetForNextRound();
    }
    
    disconnect() {
        this.connected = false;
    }
    
    reconnect() {
        this.connected = true;
    }
    
    isConnected() {
        return this.connected;
    }
    
    getPing() {
        return this.ping;
    }
    
    setPing(ping) {
        this.ping = ping;
    }
    
    getSteamId() {
        return this.steamId;
    }
    
    setSteamId(steamId) {
        this.steamId = steamId;
    }
    
    getCountry() {
        return this.country;
    }
    
    setCountry(country) {
        this.country = country;
    }
    
    getClanTag() {
        return this.clanTag;
    }
    
    setClanTag(tag) {
        this.clanTag = tag;
    }
    
    getRank() {
        return this.rank;
    }
    
    setRank(rank) {
        this.rank = rank;
    }
    
    getCommendation() {
        return { ...this.commendation };
    }
    
    addCommendation(type, amount) {
        if (this.commendation[type] !== undefined) {
            this.commendation[type] += amount;
        }
    }
    
    getStats() {
        return {
            kills: this.score.kills,
            deaths: this.score.deaths,
            assists: this.score.assists,
            headshots: this.score.headshots,
            mvps: this.score.mvps,
            kdr: this.calculateKDR(),
            accuracy: this.getAccuracy(),
            money: this.money,
            health: this.health,
            armor: this.armor
        };
    }
}

export class PlayerManager {
    constructor() {
        this.players = new Map();
        this.localPlayer = null;
        this.spectatingPlayer = null;
        this.playerOrder = [];
    }
    
    addPlayer(player) {
        this.players.set(player.id, player);
        this.playerOrder.push(player.id);
    }
    
    removePlayer(playerId) {
        this.players.delete(playerId);
        const index = this.playerOrder.indexOf(playerId);
        if (index > -1) {
            this.playerOrder.splice(index, 1);
        }
    }
    
    getPlayer(playerId) {
        return this.players.get(playerId);
    }
    
    getAllPlayers() {
        return Array.from(this.players.values());
    }
    
    getTeamPlayers(team) {
        return Array.from(this.players.values()).filter(p => p.team === team);
    }
    
    getEnemyPlayers(team) {
        return Array.from(this.players.values()).filter(p => p.team !== team);
    }
    
    getAlivePlayers(team = null) {
        let players = Array.from(this.players.values()).filter(p => p.state.alive);
        if (team !== null) {
            players = players.filter(p => p.team === team);
        }
        return players;
    }
    
    getDeadPlayers(team = null) {
        let players = Array.from(this.players.values()).filter(p => !p.state.alive);
        if (team !== null) {
            players = players.filter(p => p.team === team);
        }
        return players;
    }
    
    getLocalPlayer() {
        return this.localPlayer;
    }
    
    setLocalPlayer(player) {
        this.localPlayer = player;
    }
    
    getSpectatingPlayer() {
        return this.spectatingPlayer;
    }
    
    setSpectatingPlayer(player) {
        this.spectatingPlayer = player;
    }
    
    getPlayerCount() {
        return this.players.size;
    }
    
    getAliveCount(team = null) {
        return this.getAlivePlayers(team).length;
    }
    
    getPlayerByName(name) {
        return Array.from(this.players.values()).find(p => p.name === name);
    }
    
    getPlayersInRadius(position, radius) {
        return Array.from(this.players.values()).filter(player => {
            const dx = player.position.x - position.x;
            const dy = player.position.y - position.y;
            const dz = player.position.z - position.z;
            const distance = Math.sqrt(dx * dx + dy * dy + dz * dz);
            return distance <= radius;
        });
    }
    
    getPlayersInView(player, fov, maxDistance) {
        return Array.from(this.players.values()).filter(target => {
            if (target.id === player.id) return false;
            if (!target.state.alive) return false;
            
            const direction = player.getViewDirection();
            const toTarget = {
                x: target.position.x - player.position.x,
                y: target.position.y - player.position.y,
                z: target.position.z - player.position.z
            };
            
            const distance = Math.sqrt(
                toTarget.x * toTarget.x +
                toTarget.y * toTarget.y +
                toTarget.z * toTarget.z
            );
            
            if (distance > maxDistance) return false;
            
            const dot = direction.x * toTarget.x + direction.y * toTarget.y + direction.z * toTarget.z;
            const angle = Math.acos(dot / distance);
            
            return angle <= fov / 2;
        });
    }
    
    update(deltaTime) {
        for (const player of this.players.values()) {
            player.update(deltaTime);
        }
    }
    
    resetAllPlayers() {
        for (const player of this.players.values()) {
            player.resetForNextRound();
        }
    }
    
    resetAllPlayersForMatch() {
        for (const player of this.players.values()) {
            player.resetForNextMatch();
        }
    }
    
    clear() {
        this.players.clear();
        this.playerOrder = [];
        this.localPlayer = null;
        this.spectatingPlayer = null;
    }
    
    serialize() {
        const data = {};
        for (const [id, player] of this.players) {
            data[id] = player.getNetworkState();
        }
        return data;
    }
    
    deserialize(data) {
        this.clear();
        for (const [id, state] of Object.entries(data)) {
            const player = new Player(id, state.name || `Player ${id}`, state.team);
            player.applyNetworkState(state);
            this.players.set(id, player);
        }
    }
}

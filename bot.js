import { BOT_CONFIG, BOT_DIFFICULTY, GAME_CONFIG, TEAM, PLAYER_CONFIG } from './config.js';
import { Vector3 } from './physics.js';

export class Bot {
    constructor(botId, name, team, difficulty = BOT_DIFFICULTY.NORMAL) {
        this.id = botId;
        this.name = name;
        this.team = team;
        this.difficulty = difficulty;
        
        this.position = { x: 0, y: 0, z: 0 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.rotation = { pitch: 0, yaw: 0, roll: 0 };
        
        this.health = PLAYER_CONFIG.MAX_HEALTH;
        this.armor = 0;
        this.alive = true;
        
        this.state = {
            currentState: BOT_STATE.IDLE,
            previousState: BOT_STATE.IDLE,
            stateTime: 0,
            transitionTime: 0
        };
        
        this.ai = {
            personality: this.createPersonality(),
            skills: this.createSkills(),
            preferences: this.createPreferences(),
            memory: {
                enemies: [],
                allies: [],
                grenades: [],
                lastSeenEnemies: new Map(),
                lastHeardSounds: [],
                investigateSpots: [],
                dangerZones: []
            }
        };
        
        this.target = {
            enemy: null,
            position: null,
            lastPosition: null,
            lastSeenTime: 0,
            visible: false
        };
        
        this.movement = {
            path: [],
            currentWaypoint: 0,
            destination: null,
            moveState: MOVE_STATE.IDLE,
            jumpState: JUMP_STATE.NONE,
            crouchState: CROUCH_STATE.NONE,
            isSprinting: false,
            isStrafing: false,
            strafeDirection: 0,
            strafeTime: 0
        };
        
        this.combat = {
            primaryWeapon: null,
            sidearm: null,
            grenadeType: null,
            attackState: ATTACK_STATE.NONE,
            lastAttackTime: 0,
            burstCount: 0,
            recoilControl: 0,
            targetPriority: TARGET_PRIORITY.NONE,
            suppressionActive: false
        };
        
        this.equipment = {
            hasKevlar: false,
            hasHelmet: false,
            hasDefuseKit: false,
            grenades: [],
            hasC4: team === TEAM.TERRORIST,
            budget: 0
        };
        
        this.communication = {
            radioActive: false,
            radioMessage: null,
            radioCooldown: 0,
            teamMessage: null,
            enemySpotted: false,
            needBackup: false,
            following: null,
            followingDistance: 0
        };
        
        this.waypoints = [];
        this.coverSpots = [];
        this.grenadeSpots = [];
        
        this.lookAngles = { pitch: 0, yaw: 0 };
        this.aimPunch = { x: 0, y: 0 };
        
        this.lastUpdateTime = 0;
        this.reactionTime = this.calculateReactionTime();
        this.precision = this.calculatePrecision();
        
        this.navMesh = null;
        this.currentMap = null;
        
        this.bots = null;
        this.players = null;
        this.entities = null;
    }
    
    createPersonality() {
        return {
            aggression: this.randomRange(0.2, 0.8),
            cowardice: this.randomRange(0.0, 0.4),
            reactionTime: this.randomRange(0.1, 0.5),
            accuracy: this.randomRange(0.4, 0.9),
            teamwork: this.randomRange(0.3, 0.9),
            camping: this.randomRange(0.0, 0.6),
            flanking: this.randomRange(0.2, 0.8),
            rush: this.randomRange(0.1, 0.7),
            campingTime: this.randomRange(5.0, 30.0),
            campSpots: []
        };
    }
    
    createSkills() {
        const baseSkills = {
            reactionTime: 0.25,
            accuracy: 0.5,
            recoilControl: 0.5,
            sprayControl: 0.5,
            movement: 0.5,
            awareness: 0.5,
            GrenadeUsage: 0.5,
            strategy: 0.5
        };
        
        switch (this.difficulty) {
            case BOT_DIFFICULTY.EASY:
                return this.scaleSkills(baseSkills, 0.3);
            case BOT_DIFFICULTY.NORMAL:
                return this.scaleSkills(baseSkills, 0.5);
            case BOT_DIFFICULTY.HARD:
                return this.scaleSkills(baseSkills, 0.75);
            case BOT_DIFFICULTY.EXPERT:
                return this.scaleSkills(baseSkills, 0.95);
            default:
                return baseSkills;
        }
    }
    
    scaleSkills(skills, factor) {
        const scaled = {};
        for (const [key, value] of Object.entries(skills)) {
            scaled[key] = 0.1 + (value * factor);
        }
        return scaled;
    }
    
    createPreferences() {
        return {
            preferredDistance: this.randomRange(100, 500),
            sniperDistance: this.randomRange(400, 800),
            campSpots: [],
            rushSpots: [],
            grenadeSpots: [],
            favoriteWeapon: null,
            favoritePosition: null
        };
    }
    
    calculateReactionTime() {
        return Math.max(0.1, this.ai.skills.reactionTime - (this.difficulty * 0.05));
    }
    
    calculatePrecision() {
        return this.ai.skills.accuracy * this.ai.personality.accuracy;
    }
    
    randomRange(min, max) {
        return min + Math.random() * (max - min);
    }
    
    update(deltaTime) {
        if (!this.alive) {
            this.updateSpectator(deltaTime);
            return;
        }
        
        this.state.stateTime += deltaTime;
        this.state.transitionTime += deltaTime;
        
        this.updateSenses(deltaTime);
        this.updateStateMachine(deltaTime);
        this.updateTargeting(deltaTime);
        this.updateMovement(deltaTime);
        this.updateCombat(deltaTime);
        this.updateCommunication(deltaTime);
        
        this.lastUpdateTime = Date.now();
    }
    
    updateSenses(deltaTime) {
        this.detectEnemies();
        this.detectSounds();
        this.updateDangerMap(deltaTime);
        this.updateMemory(deltaTime);
    }
    
    detectEnemies() {
        if (!this.players) return;
        
        this.target.enemy = null;
        this.target.visible = false;
        this.target.lastSeenTime = 0;
        
        let closestDistance = Infinity;
        let highestPriority = -1;
        
        for (const player of this.players) {
            if (player.team === this.team || !player.alive) continue;
            
            const distance = this.getDistanceTo(player.position);
            
            if (distance > GAME_CONFIG.MAX_BOT_VISION_RANGE) continue;
            
            const visible = this.hasLineOfSight(this.position, player.position);
            
            if (visible) {
                const priority = this.calculateTargetPriority(player, distance);
                
                if (priority > highestPriority) {
                    highestPriority = priority;
                    this.target.enemy = player;
                    this.target.visible = true;
                    this.target.lastSeenTime = Date.now();
                    this.target.lastPosition = { ...player.position };
                    closestDistance = distance;
                }
            } else {
                const lastSeen = this.ai.memory.lastSeenEnemies.get(player.id);
                if (lastSeen && Date.now() - lastSeen.time < 5000) {
                    if (lastSeen.distance < closestDistance) {
                        closestDistance = lastSeen.distance;
                        this.target.lastPosition = lastSeen.position;
                    }
                }
            }
            
            this.ai.memory.lastSeenEnemies.set(player.id, {
                position: { ...player.position },
                time: Date.now(),
                visible: visible,
                distance: distance
            });
        }
        
        if (!this.target.enemy && this.target.lastPosition) {
            const lastSeenAge = Date.now() - this.target.lastSeenTime;
            if (lastSeenAge > 3000) {
                this.target.lastPosition = null;
            }
        }
    }
    
    calculateTargetPriority(player, distance) {
        let priority = 0;
        
        priority += player.health < 50 ? 20 : (player.health < 100 ? 10 : 0);
        priority += player.hasC4 ? 30 : 0;
        
        if (distance < 200) priority += 30;
        else if (distance < 500) priority += 20;
        else if (distance < 1000) priority += 10;
        
        if (player.combat.isInCombat) priority += 15;
        
        priority += player.score?.kills || 0;
        
        return priority;
    }
    
    detectSounds() {
        if (!this.entities) return;
        
        for (const sound of this.entities.sounds) {
            const distance = this.getDistanceTo(sound.position);
            
            if (distance < sound.range) {
                const existingSound = this.ai.memory.lastHeardSounds.find(
                    s => s.position.x === sound.position.x &&
                         s.position.y === sound.position.y &&
                         Date.now() - s.time < 1000
                );
                
                if (!existingSound) {
                    this.ai.memory.lastHeardSounds.push({
                        position: { ...sound.position },
                        type: sound.type,
                        time: Date.now(),
                        distance: distance
                    });
                    
                    this.onSoundHeard(sound);
                }
            }
        }
    }
    
    onSoundHeard(sound) {
        switch (sound.type) {
            case SOUND_TYPE.FOOTSTEP:
                if (Math.random() < this.ai.personality.aggression) {
                    this.state.currentState = BOT_STATE.INVESTIGATE;
                    this.setDestination(sound.position);
                }
                break;
                
            case SOUND_TYPE.GUNSHOT:
                if (Math.random() < this.ai.personality.aggression * 0.5) {
                    this.state.currentState = BOT_STATE.HUNTING;
                    this.setDestination(sound.position);
                }
                break;
                
            case SOUND_TYPE.GRENADE:
                this.addDangerZone(sound.position, 300, 5.0);
                break;
                
            case SOUND_TYPE.EXPLOSION:
                this.addDangerZone(sound.position, 500, 3.0);
                this.state.currentState = BOT_STATE.EVASIVE;
                break;
        }
    }
    
    updateDangerMap(deltaTime) {
        const currentTime = Date.now();
        
        for (let i = this.ai.memory.dangerZones.length - 1; i >= 0; i--) {
            const zone = this.ai.memory.dangerZones[i];
            if (currentTime - zone.startTime > zone.duration * 1000) {
                this.ai.memory.dangerZones.splice(i, 1);
            } else {
                const remaining = 1 - (currentTime - zone.startTime) / (zone.duration * 1000);
                zone.intensity = zone.baseIntensity * remaining;
            }
        }
    }
    
    addDangerZone(position, radius, duration, intensity = 1.0) {
        this.ai.memory.dangerZones.push({
            position: { ...position },
            radius: radius,
            duration: duration,
            startTime: Date.now(),
            intensity: intensity,
            baseIntensity: intensity
        });
    }
    
    updateMemory(deltaTime) {
        const currentTime = Date.now();
        
        for (const [id, data] of this.ai.memory.lastSeenEnemies) {
            if (currentTime - data.time > 10000) {
                this.ai.memory.lastSeenEnemies.delete(id);
            }
        }
        
        for (let i = this.ai.memory.lastHeardSounds.length - 1; i >= 0; i--) {
            if (currentTime - this.ai.memory.lastHeardSounds[i].time > 3000) {
                this.ai.memory.lastHeardSounds.splice(i, 1);
            }
        }
        
        for (let i = this.ai.memory.investigateSpots.length - 1; i >= 0; i--) {
            if (currentTime - this.ai.memory.investigateSpots[i].time > 5000) {
                this.ai.memory.investigateSpots.splice(i, 1);
            }
        }
    }
    
    updateStateMachine(deltaTime) {
        const previousState = this.state.currentState;
        
        switch (this.state.currentState) {
            case BOT_STATE.IDLE:
                this.updateIdleState(deltaTime);
                break;
            case BOT_STATE.MOVING:
                this.updateMovingState(deltaTime);
                break;
            case BOT_STATE.ATTACKING:
                this.updateAttackingState(deltaTime);
                break;
            case BOT_STATE.RETREATING:
                this.updateRetreatingState(deltaTime);
                break;
            case BOT_STATE.INVESTIGATE:
                this.updateInvestigateState(deltaTime);
                break;
            case BOT_STATE.HUNTING:
                this.updateHuntingState(deltaTime);
                break;
            case BOT_STATE.EVASIVE:
                this.updateEvasiveState(deltaTime);
                break;
            case BOT_STATE.CAMPING:
                this.updateCampingState(deltaTime);
                break;
            case BOT_STATE.FOLLOWING:
                this.updateFollowingState(deltaTime);
                break;
            case BOT_STATE.BUYING:
                this.updateBuyingState(deltaTime);
                break;
            case BOT_STATE.PLANTING:
                this.updatePlantingState(deltaTime);
                break;
            case BOT_STATE.DEFUSING:
                this.updateDefusingState(deltaTime);
                break;
        }
        
        if (previousState !== this.state.currentState) {
            this.onStateExit(previousState);
            this.onStateEnter(this.state.currentState);
            this.state.previousState = previousState;
            this.state.stateTime = 0;
        }
    }
    
    updateIdleState(deltaTime) {
        if (this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.ATTACKING);
            return;
        }
        
        if (this.shouldInvestigateSound()) {
            this.transitionTo(BOT_STATE.INVESTIGATE);
            return;
        }
        
        if (this.shouldRush()) {
            this.transitionTo(BOT_STATE.MOVING);
            this.setNextWaypoint();
        } else if (this.shouldCamp()) {
            this.transitionTo(BOT_STATE.CAMPING);
        }
    }
    
    updateMovingState(deltaTime) {
        if (this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.ATTACKING);
            return;
        }
        
        this.followPath(deltaTime);
        
        if (this.movement.path.length > 0 && this.movement.currentWaypoint >= this.movement.path.length) {
            this.transitionTo(BOT_STATE.IDLE);
        }
        
        if (this.isInDangerZone()) {
            this.transitionTo(BOT_STATE.EVASIVE);
        }
    }
    
    updateAttackingState(deltaTime) {
        if (!this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.HUNTING);
            return;
        }
        
        this.updateAim(deltaTime);
        this.updateShooting(deltaTime);
        
        if (this.shouldRetreat()) {
            this.transitionTo(BOT_STATE.RETREATING);
        }
        
        if (this.isLowHealth() && Math.random() < this.ai.personality.cowardice) {
            this.transitionTo(BOT_STATE.RETREATING);
        }
    }
    
    updateRetreatingState(deltaTime) {
        this.retreat(deltaTime);
        
        if (this.hasEnemiesInSight()) {
            this.updateAim(deltaTime);
            this.updateShooting(deltaTime);
        }
        
        if (this.isSafe() || !this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.IDLE);
        }
    }
    
    updateInvestigateState(deltaTime) {
        if (this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.ATTACKING);
            return;
        }
        
        if (this.reachedDestination()) {
            this.state.stateTime = 0;
        }
        
        if (this.state.stateTime > 3.0) {
            this.transitionTo(BOT_STATE.IDLE);
        }
    }
    
    updateHuntingState(deltaTime) {
        if (this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.ATTACKING);
            return;
        }
        
        if (this.target.lastPosition) {
            this.setDestination(this.target.lastPosition);
        }
        
        this.followPath(deltaTime);
        
        if (this.state.stateTime > 10.0) {
            this.transitionTo(BOT_STATE.IDLE);
        }
    }
    
    updateEvasiveState(deltaTime) {
        if (!this.isInDangerZone()) {
            this.transitionTo(BOT_STATE.IDLE);
            return;
        }
        
        this.moveToSafety(deltaTime);
        
        if (this.hasEnemiesInSight()) {
            this.updateAim(deltaTime);
            this.updateShooting(deltaTime);
        }
    }
    
    updateCampingState(deltaTime) {
        if (this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.ATTACKING);
            return;
        }
        
        this.updateCampLook(deltaTime);
        
        if (this.state.stateTime > this.ai.personality.campingTime) {
            this.transitionTo(BOT_STATE.MOVING);
            this.setNextWaypoint();
        }
    }
    
    updateFollowingState(deltaTime) {
        const leader = this.communication.following;
        if (!leader || !leader.alive) {
            this.transitionTo(BOT_STATE.IDLE);
            return;
        }
        
        const distance = this.getDistanceTo(leader.position);
        
        if (distance > this.communication.followingDistance + 100) {
            this.setDestination(leader.position);
            this.followPath(deltaTime);
        }
        
        if (this.hasEnemiesInSight()) {
            this.transitionTo(BOT_STATE.ATTACKING);
        }
    }
    
    updateBuyingState(deltaTime) {
        if (this.state.stateTime > 5.0) {
            this.transitionTo(BOT_STATE.MOVING);
            this.setNextWaypoint();
        }
    }
    
    updatePlantingState(deltaTime) {
        if (!this.isInBombZone()) {
            this.transitionTo(BOT_STATE.MOVING);
            return;
        }
        
        this.plantBomb(deltaTime);
    }
    
    updateDefusingState(deltaTime) {
        if (!this.isNearBomb()) {
            this.transitionTo(BOT_STATE.MOVING);
            return;
        }
        
        this.defuseBomb(deltaTime);
    }
    
    onStateEnter(state) {
        switch (state) {
            case BOT_STATE.MOVING:
                this.movement.moveState = MOVE_STATE.WALKING;
                break;
            case BOT_STATE.ATTACKING:
                this.combat.attackState = ATTACK_STATE.ENGAGING;
                break;
            case BOT_STATE.CAMPING:
                this.findCampSpot();
                this.movement.moveState = MOVE_STATE.STATIONARY;
                break;
            case BOT_STATE.BUYING:
                this.executeBuyPhase();
                break;
        }
    }
    
    onStateExit(state) {
        switch (state) {
            case BOT_STATE.CAMPING:
                this.movement.crouchState = CROUCH_STATE.NONE;
                break;
            case BOT_STATE.RETREATING:
                this.movement.isSprinting = false;
                break;
        }
    }
    
    transitionTo(newState) {
        this.state.currentState = newState;
        this.state.stateTime = 0;
        this.state.transitionTime = 0;
    }
    
    updateTargeting(deltaTime) {
        if (!this.target.enemy) return;
        
        const distance = this.getDistanceTo(this.target.enemy.position);
        
        if (this.prefersRangedCombat() && distance < 200) {
            this.movement.moveState = MOVE_STATE.RETREATING;
        } else if (this.prefersMeleeCombat() && distance > 100) {
            this.movement.moveState = MOVE_STATE.RUSHING;
        }
    }
    
    updateAim(deltaTime) {
        if (!this.target.enemy) return;
        
        const targetPos = this.target.enemy.position;
        const aimPosition = this.getAimPosition(targetPos);
        
        const desiredYaw = Math.atan2(aimPosition.x - this.position.x, aimPosition.y - this.position.y);
        const desiredPitch = Math.atan2(-aimPosition.z, this.getDistanceTo2D(targetPos));
        
        const aimSpeed = this.ai.skills.recoilControl * 5.0 * deltaTime;
        
        this.lookAngles.yaw = this.lerpAngle(this.lookAngles.yaw, desiredYaw, aimSpeed);
        this.lookAngles.pitch = this.lerp(this.lookAngles.pitch, desiredPitch, aimSpeed);
        
        const punchReduction = this.ai.skills.recoilControl * 0.5;
        this.aimPunch.x *= (1 - punchReduction);
        this.aimPunch.y *= (1 - punchReduction);
    }
    
    getAimPosition(targetPos) {
        let aimPos = { ...targetPos };
        aimPos.z += 50;
        
        const predictionTime = this.reactionTime * 0.5;
        if (this.target.enemy.velocity) {
            aimPos.x += this.target.enemy.velocity.x * predictionTime;
            aimPos.y += this.target.enemy.velocity.y * predictionTime;
            aimPos.z += this.target.enemy.velocity.z * predictionTime;
        }
        
        const randomOffset = (1 - this.precision) * 20;
        aimPos.x += (Math.random() - 0.5) * randomOffset;
        aimPos.y += (Math.random() - 0.5) * randomOffset;
        aimPos.z += (Math.random() - 0.5) * randomOffset;
        
        return aimPos;
    }
    
    updateShooting(deltaTime) {
        if (!this.hasEnemiesInSight()) return;
        
        const distance = this.getDistanceTo(this.target.enemy.position);
        
        if (distance > this.getWeaponRange()) {
            this.useScope();
        } else {
            this.usesScope(false);
        }
        
        const fireRate = this.getWeaponFireRate();
        const timeSinceLastShot = Date.now() - this.combat.lastAttackTime;
        
        if (timeSinceLastShot > fireRate) {
            if (this.canFire()) {
                this.fireWeapon();
            }
        }
        
        if (this.shouldGrenade()) {
            this.throwGrenade();
        }
    }
    
    updateMovement(deltaTime) {
        if (this.movement.moveState === MOVE_STATE.IDLE) return;
        
        switch (this.movement.moveState) {
            case MOVE_STATE.WALKING:
                this.performWalking(deltaTime);
                break;
            case MOVE_STATE.RUSHING:
                this.performRushing(deltaTime);
                break;
            case MOVE_STATE.RETREATING:
                this.performRetreating(deltaTime);
                break;
            case MOVE_STATE.STRAFING:
                this.performStrafing(deltaTime);
                break;
        }
        
        this.updateStrafing(deltaTime);
        this.updateJumping(deltaTime);
        this.updateCrouching(deltaTime);
    }
    
    followPath(deltaTime) {
        if (this.movement.path.length === 0) return;
        
        const waypoint = this.movement.path[this.movement.currentWaypoint];
        if (!waypoint) return;
        
        const distance = this.getDistanceTo2D(waypoint.position);
        
        if (distance < 50) {
            this.movement.currentWaypoint++;
            if (this.movement.currentWaypoint >= this.movement.path.length) {
                this.movement.moveState = MOVE_STATE.IDLE;
            }
            return;
        }
        
        this.moveToward(waypoint.position, deltaTime);
    }
    
    moveToward(targetPos, deltaTime) {
        const dx = targetPos.x - this.position.x;
        const dy = targetPos.y - this.position.y;
        const dz = targetPos.z - this.position.z;
        
        const distance2D = Math.sqrt(dx * dx + dy * dy);
        
        if (distance2D > 0) {
            const moveSpeed = this.getMovementSpeed();
            const moveX = (dx / distance2D) * moveSpeed * deltaTime;
            const moveY = (dy / distance2D) * moveSpeed * deltaTime;
            
            this.position.x += moveX;
            this.position.y += moveY;
        }
        
        if (Math.abs(dz) > 0.1) {
            this.position.z += Math.sign(dz) * PLAYER_CONFIG.MOVE_SPEED * deltaTime;
        }
        
        this.lookAngles.yaw = Math.atan2(dx, dy);
    }
    
    performWalking(deltaTime) {
        this.movement.isSprinting = false;
        this.moveToward(this.movement.destination || this.getNextWaypointPosition(), deltaTime);
    }
    
    performRushing(deltaTime) {
        this.movement.isSprinting = true;
        const rushSpeed = PLAYER_CONFIG.MOVE_SPEED * 1.3;
        
        if (this.target.enemy) {
            this.moveToward(this.target.enemy.position, deltaTime);
        }
    }
    
    performRetreating(deltaTime) {
        this.movement.isSprinting = true;
        
        const retreatSpot = this.findRetreatSpot();
        if (retreatSpot) {
            this.moveToward(retreatSpot, deltaTime);
        }
    }
    
    performStrafing(deltaTime) {
        if (!this.movement.isStrafing) {
            this.movement.isStrafing = true;
            this.movement.strafeDirection = Math.random() > 0.5 ? 1 : -1;
            this.movement.strafeTime = this.randomRange(1.0, 3.0);
        }
        
        const strafeSpeed = PLAYER_CONFIG.MOVE_SPEED * 0.8;
        const dx = Math.cos(this.lookAngles.yaw + Math.PI / 2 * this.movement.strafeDirection) * strafeSpeed;
        const dy = Math.sin(this.lookAngles.yaw + Math.PI / 2 * this.movement.strafeDirection) * strafeSpeed;
        
        this.position.x += dx * deltaTime;
        this.position.y += dy * deltaTime;
        
        if (this.movement.strafeTime > 0) {
            this.movement.strafeTime -= deltaTime;
        } else {
            this.movement.isStrafing = false;
        }
    }
    
    updateStrafing(deltaTime) {
        if (this.state.currentState === BOT_STATE.ATTACKING && Math.random() < 0.3) {
            this.performStrafing(deltaTime);
        }
    }
    
    updateJumping(deltaTime) {
        const shouldJump = this.shouldJump();
        if (shouldJump) {
            this.jump();
        }
    }
    
    shouldJump() {
        if (!this.isInAir()) return false;
        
        if (this.isApproachingObstacle()) return true;
        
        return false;
    }
    
    isApproachingObstacle() {
        const lookAhead = 100;
        const dx = Math.sin(this.lookAngles.yaw) * lookAhead;
        const dy = Math.cos(this.lookAngles.yaw) * lookAhead;
        
        const checkPos = {
            x: this.position.x + dx,
            y: this.position.y + dy,
            z: this.position.z
        };
        
        return this.isBlocked(checkPos);
    }
    
    updateCrouching(deltaTime) {
        const shouldCrouch = this.shouldCrouch();
        
        if (shouldCrouch && this.movement.crouchState !== CROUCH_STATE.CROUCHING) {
            this.movement.crouchState = CROUCH_STATE.CROUCHING;
        } else if (!shouldCrouch && this.movement.crouchState === CROUCH_STATE.CROUCHING) {
            this.movement.crouchState = CROUCH_STATE.NONE;
        }
    }
    
    shouldCrouch() {
        if (this.state.currentState === BOT_STATE.CAMPING) return true;
        
        if (this.hasEnemiesInSight()) {
            const distance = this.getDistanceTo(this.target.enemy.position);
            if (distance < 300 && Math.random() < 0.4) return true;
        }
        
        return false;
    }
    
    updateCombat(deltaTime) {
        this.updateWeaponChoice(deltaTime);
        this.updateSuppression(deltaTime);
    }
    
    updateWeaponChoice(deltaTime) {
        if (!this.target.enemy) return;
        
        const distance = this.getDistanceTo(this.target.enemy.position);
        
        if (distance > 800) {
            if (this.hasWeapon('awp') || this.hasWeapon('ssg08')) {
                this.equipBestWeapon();
            }
        } else if (distance > 300) {
            if (this.hasWeapon('ak47') || this.hasWeapon('m4a4')) {
                this.equipBestWeapon();
            }
        } else {
            if (this.hasWeapon('mp5') || this.hasWeapon('ump45')) {
                this.equipBestWeapon();
            }
        }
    }
    
    updateSuppression(deltaTime) {
        if (this.state.currentState !== BOT_STATE.ATTACKING) {
            this.combat.suppressionActive = false;
            return;
        }
        
        if (this.hasEnemiesInSight()) {
            const distance = this.getDistanceTo(this.target.enemy.position);
            
            if (distance < 200 && Math.random() < 0.2) {
                this.combat.suppressionActive = true;
                this.performSuppressionFire();
            } else {
                this.combat.suppressionActive = false;
            }
        }
    }
    
    performSuppressionFire() {
        const randomOffset = 0.2;
        const aimX = this.target.enemy.position.x + (Math.random() - 0.5) * randomOffset * 100;
        const aimY = this.target.enemy.position.y + (Math.random() - 0.5) * randomOffset * 100;
        
        this.setDestination({ x: aimX, y: aimY, z: this.target.enemy.position.z });
        this.fireWeapon();
    }
    
    updateCommunication(deltaTime) {
        if (this.communication.radioCooldown > 0) {
            this.communication.radioCooldown -= deltaTime;
        }
        
        if (this.communication.enemySpotted && this.communication.radioCooldown <= 0) {
            this.radioMessage('enemy_spotted');
            this.communication.enemySpotted = false;
            this.communication.radioCooldown = 3.0;
        }
        
        if (this.communication.needBackup && this.communication.radioCooldown <= 0) {
            this.radioMessage('need_backup');
            this.communication.needBackup = false;
            this.communication.radioCooldown = 5.0;
        }
    }
    
    radioMessage(message) {
        this.communication.radioMessage = message;
        this.communication.radioActive = true;
        
        setTimeout(() => {
            this.communication.radioActive = false;
            this.communication.radioMessage = null;
        }, 1000);
    }
    
    setDestination(position) {
        this.movement.destination = { ...position };
    }
    
    setNextWaypoint() {
        if (this.waypoints.length === 0) return;
        
        const nextIndex = (this.movement.currentWaypoint + 1) % this.waypoints.length;
        this.movement.path = [this.waypoints[nextIndex]];
        this.movement.currentWaypoint = 0;
    }
    
    getNextWaypointPosition() {
        if (this.waypoints.length === 0) return null;
        
        const nextIndex = (this.movement.currentWaypoint + 1) % this.waypoints.length;
        return this.waypoints[nextIndex].position;
    }
    
    reachedDestination() {
        if (!this.movement.destination) return true;
        
        const distance = this.getDistanceTo(this.movement.destination);
        return distance < 50;
    }
    
    findRetreatSpot() {
        const retreatSpots = [];
        
        for (const cover of this.coverSpots) {
            const distanceToEnemy = this.target.enemy ? 
                this.getDistanceTo2D(this.target.enemy.position) - this.getDistanceTo2D(cover.position) :
                this.getDistanceTo2D(cover.position);
            
            if (distanceToEnemy > 0) {
                retreatSpots.push({
                    position: cover.position,
                    safety: distanceToEnemy
                });
            }
        }
        
        retreatSpots.sort((a, b) => b.safety - a.safety);
        
        return retreatSpots.length > 0 ? retreatSpots[0].position : null;
    }
    
    findCampSpot() {
        const validSpots = [];
        
        for (const spot of this.ai.personality.campSpots) {
            const hasGoodSight = this.hasGoodSightLines(spot);
            const isHidden = !this.isSpotVisible(spot);
            
            if (hasGoodSight && isHidden) {
                validSpots.push(spot);
            }
        }
        
        if (validSpots.length > 0) {
            const spot = validSpots[Math.floor(Math.random() * validSpots.length)];
            this.setDestination(spot.position);
            this.ai.personality.campingTime = this.randomRange(10.0, 45.0);
        }
    }
    
    hasGoodSightLines(spot) {
        let goodLines = 0;
        const testAngles = [0, Math.PI / 4, Math.PI / 2, 3 * Math.PI / 4, Math.PI];
        
        for (const angle of testAngles) {
            const testPos = {
                x: spot.position.x + Math.cos(angle) * 500,
                y: spot.position.y + Math.sin(angle) * 500,
                z: spot.position.z
            };
            
            if (this.hasLineOfSight(spot.position, testPos)) {
                goodLines++;
            }
        }
        
        return goodLines >= 3;
    }
    
    isSpotVisible(spot) {
        for (const player of this.players || []) {
            if (player.team === this.team) continue;
            
            const distance = this.getDistanceTo(spot.position);
            if (distance > 500) continue;
            
            if (this.hasLineOfSight(player.position, spot.position)) {
                return true;
            }
        }
        
        return false;
    }
    
    updateCampLook(deltaTime) {
        if (this.state.stateTime < 1.0) return;
        
        const lookSpeed = 0.5;
        const maxOffset = Math.PI / 6;
        
        const offset = Math.sin(this.state.stateTime * lookSpeed) * maxOffset;
        const desiredYaw = this.getCampYaw() + offset;
        
        this.lookAngles.yaw = this.lerpAngle(this.lookAngles.yaw, desiredYaw, 0.1);
    }
    
    getCampYaw() {
        const enemyDir = this.getAverageEnemyDirection();
        return Math.atan2(enemyDir.x, enemyDir.y);
    }
    
    getAverageEnemyDirection() {
        let totalX = 0, totalY = 0, count = 0;
        
        for (const player of this.players || []) {
            if (player.team === this.team) continue;
            
            totalX += player.position.x - this.position.x;
            totalY += player.position.y - this.position.y;
            count++;
        }
        
        if (count === 0) return { x: 0, y: 1 };
        
        return {
            x: totalX / count,
            y: totalY / count
        };
    }
    
    moveToSafety(deltaTime) {
        const dangerDirection = this.getAverageDangerDirection();
        
        const safePos = {
            x: this.position.x - dangerDirection.x * 200,
            y: this.position.y - dangerDirection.y * 200,
            z: this.position.z
        };
        
        this.moveToward(safePos, deltaTime);
        this.movement.isSprinting = true;
    }
    
    getAverageDangerDirection() {
        let totalX = 0, totalY = 0, totalWeight = 0;
        
        for (const zone of this.ai.memory.dangerZones) {
            const dx = zone.position.x - this.position.x;
            const dy = zone.position.y - this.position.y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            
            if (distance > 0) {
                const weight = zone.intensity / distance;
                totalX += (dx / distance) * weight;
                totalY += (dy / distance) * weight;
                totalWeight += weight;
            }
        }
        
        if (totalWeight === 0) return { x: 0, y: 0 };
        
        return {
            x: totalX / totalWeight,
            y: totalY / totalWeight
        };
    }
    
    hasEnemiesInSight() {
        return this.target.visible && this.target.enemy !== null;
    }
    
    shouldInvestigateSound() {
        return this.ai.memory.lastHeardSounds.length > 0;
    }
    
    shouldRush() {
        return Math.random() < this.ai.personality.rush;
    }
    
    shouldCamp() {
        return Math.random() < this.ai.personality.camping;
    }
    
    shouldRetreat() {
        if (this.isLowHealth()) return true;
        
        const distance = this.getDistanceTo(this.target.enemy.position);
        if (distance < 150 && Math.random() < this.ai.personality.cowardice) return true;
        
        if (this.isFlanked()) return true;
        
        return false;
    }
    
    shouldGrenade() {
        if (!this.hasGrenades()) return false;
        
        const distance = this.getDistanceTo(this.target.enemy.position);
        if (distance > 200 && distance < 500) {
            return Math.random() < this.ai.skills.GrenadeUsage * 0.3;
        }
        
        return false;
    }
    
    isLowHealth() {
        return this.health < 30;
    }
    
    isFlanked() {
        for (const player of this.players || []) {
            if (player.team === this.team) continue;
            
            const behindAngle = Math.PI / 2;
            const dx = player.position.x - this.position.x;
            const dy = player.position.y - this.position.y;
            const angleToPlayer = Math.atan2(dy, dx);
            const angleDiff = Math.abs(this.normalizeAngle(angleToPlayer - this.lookAngles.yaw));
            
            if (angleDiff < behindAngle && this.getDistanceTo(player.position) < 300) {
                return true;
            }
        }
        
        return false;
    }
    
    isInDangerZone() {
        for (const zone of this.ai.memory.dangerZones) {
            const distance = this.getDistanceTo(zone.position);
            if (distance < zone.radius) {
                return true;
            }
        }
        return false;
    }
    
    isSafe() {
        return !this.isInDangerZone() && !this.hasEnemiesInSight();
    }
    
    isInAir() {
        return this.position.z > GAME_CONFIG.GROUND_LEVEL + 10;
    }
    
    isBlocked(position) {
        return false;
    }
    
    isInBombZone() {
        return false;
    }
    
    isNearBomb() {
        return false;
    }
    
    hasLineOfSight(from, to) {
        return true;
    }
    
    getDistanceTo(position) {
        const dx = position.x - this.position.x;
        const dy = position.y - this.position.y;
        const dz = position.z - this.position.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    getDistanceTo2D(position) {
        const dx = position.x - this.position.x;
        const dy = position.y - this.position.y;
        return Math.sqrt(dx * dx + dy * dy);
    }
    
    prefersRangedCombat() {
        return this.hasWeapon('awp') || this.hasWeapon('ssg08');
    }
    
    prefersMeleeCombat() {
        return this.hasWeapon('knife');
    }
    
    hasWeapon(weaponName) {
        return false;
    }
    
    getWeaponRange() {
        return 500;
    }
    
    getWeaponFireRate() {
        return 100;
    }
    
    equipBestWeapon() {
    }
    
    canFire() {
        return true;
    }
    
    fireWeapon() {
        this.combat.lastAttackTime = Date.now();
    }
    
    useScope() {
    }
    
    usesScope(using) {
    }
    
    throwGrenade() {
    }
    
    plantBomb(deltaTime) {
    }
    
    defuseBomb(deltaTime) {
    }
    
    jump() {
        this.movement.jumpState = JUMP_STATE.JUMPING;
    }
    
    retreat(deltaTime) {
        this.performRetreating(deltaTime);
    }
    
    getMovementSpeed() {
        let speed = PLAYER_CONFIG.MOVE_SPEED;
        
        if (this.movement.isSprinting) {
            speed *= 1.3;
        }
        
        if (this.movement.crouchState === CROUCH_STATE.CROUCHING) {
            speed *= PLAYER_CONFIG.CROUCH_SPEED_MULTIPLIER;
        }
        
        if (this.isInAir()) {
            speed *= 0.8;
        }
        
        return speed;
    }
    
    executeBuyPhase() {
    }
    
    hasGrenades() {
        return this.equipment.grenades.length > 0;
    }
    
    updateSpectator(deltaTime) {
    }
    
    lerp(a, b, t) {
        return a + (b - a) * t;
    }
    
    lerpAngle(a, b, t) {
        let diff = b - a;
        while (diff < -Math.PI) diff += Math.PI * 2;
        while (diff > Math.PI) diff -= Math.PI * 2;
        return a + diff * t;
    }
    
    normalizeAngle(angle) {
        while (angle < -Math.PI) angle += Math.PI * 2;
        while (angle > Math.PI) angle -= Math.PI * 2;
        return angle;
    }
}

export const BOT_STATE = {
    IDLE: 'idle',
    MOVING: 'moving',
    ATTACKING: 'attacking',
    RETREATING: 'retreating',
    INVESTIGATE: 'investigate',
    HUNTING: 'hunting',
    EVASIVE: 'evasive',
    CAMPING: 'camping',
    FOLLOWING: 'following',
    BUYING: 'buying',
    PLANTING: 'planting',
    DEFUSING: 'defusing'
};

export const MOVE_STATE = {
    IDLE: 'idle',
    WALKING: 'walking',
    RUNNING: 'running',
    RUSHING: 'rushing',
    RETREATING: 'retreating',
    STRAFING: 'strafing',
    STATIONARY: 'stationary'
};

export const JUMP_STATE = {
    NONE: 'none',
    JUMPING: 'jumping',
    FALLING: 'falling'
};

export const CROUCH_STATE = {
    NONE: 'none',
    CROUCHING: 'crouching',
    UNCROUCHING: 'uncrouching'
};

export const ATTACK_STATE = {
    NONE: 'none',
    ENGAGING: 'engaging',
    SUPPRESSING: 'suppressing',
    SPRAYING: 'spraying',
    TAP_FIRING: 'tap_firing'
};

export const TARGET_PRIORITY = {
    NONE: 0,
    LOW: 1,
    MEDIUM: 2,
    HIGH: 3,
    CRITICAL: 4
};

export const SOUND_TYPE = {
    FOOTSTEP: 'footstep',
    GUNSHOT: 'gunshot',
    GRENADE: 'grenade',
    EXPLOSION: 'explosion',
    VOICE: 'voice',
    RADIO: 'radio'
};

export class BotManager {
    constructor() {
        this.bots = new Map();
        this.nextBotId = 1;
        
        this.teamBots = {
            [TEAM.TERRORIST]: [],
            [TEAM.COUNTER_TERRORIST]: []
        };
        
        this.globalDifficulty = BOT_DIFFICULTY.NORMAL;
    }
    
    addBot(name, team, difficulty = null) {
        const botId = this.nextBotId++;
        const botDifficulty = difficulty || this.globalDifficulty;
        
        const bot = new Bot(botId, name, team, botDifficulty);
        
        this.bots.set(botId, bot);
        this.teamBots[team].push(bot);
        
        bot.bots = this;
        
        return bot;
    }
    
    removeBot(botId) {
        const bot = this.bots.get(botId);
        if (!bot) return;
        
        const teamList = this.teamBots[bot.team];
        const index = teamList.indexOf(bot);
        if (index > -1) {
            teamList.splice(index, 1);
        }
        
        this.bots.delete(botId);
    }
    
    getBot(botId) {
        return this.bots.get(botId);
    }
    
    getAllBots() {
        return Array.from(this.bots.values());
    }
    
    getTeamBots(team) {
        return this.teamBots[team] || [];
    }
    
    getAliveBots(team = null) {
        let bots = Array.from(this.bots.values()).filter(b => b.alive);
        
        if (team !== null) {
            bots = bots.filter(b => b.team === team);
        }
        
        return bots;
    }
    
    update(deltaTime) {
        for (const bot of this.bots.values()) {
            bot.update(deltaTime);
        }
    }
    
    setGlobalDifficulty(difficulty) {
        this.globalDifficulty = difficulty;
        
        for (const bot of this.bots.values()) {
            bot.difficulty = difficulty;
            bot.ai.skills = bot.createSkills();
            bot.reactionTime = bot.calculateReactionTime();
            bot.precision = bot.calculatePrecision();
        }
    }
    
    fillServer(team, count) {
        const currentCount = this.getTeamBots(team).length;
        const botsToAdd = Math.max(0, count - currentCount);
        
        const names = this.getBotNames();
        
        for (let i = 0; i < botsToAdd; i++) {
            const name = names[Math.floor(Math.random() * names.length)];
            this.addBot(name, team);
        }
    }
    
    getBotNames() {
        return [
            'Faker', 'S1mple', 'ZywOo', 'Shroud', 'ScreaM',
            'Get_Right', ' Friberg', 'KRIMZ', '_flusha', 'REZ',
            'device', 'dupreeh', 'gla1ve', 'Xyp9x', 'Magisk',
            'NiKo', 'rain', 'broky', 'karrigan', 'ropz'
        ];
    }
    
    kickAllBots() {
        for (const botId of Array.from(this.bots.keys())) {
            this.removeBot(botId);
        }
    }
    
    setWaypoints(botId, waypoints) {
        const bot = this.bots.get(botId);
        if (bot) {
            bot.waypoints = waypoints;
        }
    }
    
    setCoverSpots(botId, coverSpots) {
        const bot = this.bots.get(botId);
        if (bot) {
            bot.coverSpots = coverSpots;
        }
    }
    
    giveCommand(botId, command, params = {}) {
        const bot = this.bots.get(botId);
        if (!bot) return;
        
        switch (command) {
            case 'attack':
                bot.transitionTo(BOT_STATE.ATTACKING);
                break;
            case 'move':
                if (params.position) {
                    bot.setDestination(params.position);
                    bot.transitionTo(BOT_STATE.MOVING);
                }
                break;
            case 'follow':
                if (params.target) {
                    bot.communication.following = params.target;
                    bot.transitionTo(BOT_STATE.FOLLOWING);
                }
                break;
            case 'stop':
                bot.transitionTo(BOT_STATE.IDLE);
                bot.movement.moveState = MOVE_STATE.IDLE;
                break;
            case 'camp':
                bot.transitionTo(BOT_STATE.CAMPING);
                break;
            case 'rush':
                bot.transitionTo(BOT_STATE.MOVING);
                bot.movement.moveState = MOVE_STATE.RUSHING;
                break;
        }
    }
    
    giveTeamCommand(team, command, params = {}) {
        const bots = this.getTeamBots(team);
        
        for (const bot of bots) {
            this.giveCommand(bot.id, command, params);
        }
    }
}

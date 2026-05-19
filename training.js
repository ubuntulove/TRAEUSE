import { GAME_CONFIG } from './config.js';

export const TRAINING_MODE = {
    NONE: 0,
    AIM_TRAINING: 1,
    RECOIL_TRAINING: 2,
    MOVEMENT_TRAINING: 3,
    GRENADE_TRAINING: 4,
    BOMB_TRAINING: 5,
    DEFUSE_TRAINING: 6,
    WALLBANG_TRAINING: 7,
    CROSSHAIR_PLACEMENT: 8,
    PREFIRE_TRAINING: 9,
    SPRAY_CONTROL: 10,
    BURST_FIRE: 11,
    FULL_AUTO: 12,
    PISTOL_ONLY: 13,
    KNIFE_TRAINING: 14,
    CUSTOM: 99
};

export const TRAINING_DIFFICULTY = {
    BEGINNER: 0,
    EASY: 1,
    NORMAL: 2,
    HARD: 3,
    EXPERT: 4,
    MASTER: 5
};

export const AIM_BOT_TYPE = {
    STATIC: 0,
    MOVING: 1,
    CIRCLING: 2,
    RANDOM: 3,
    ZIGZAG: 4,
    FLEEING: 5,
    AGGRESSIVE: 6,
    DEFENSIVE: 7,
    ERRATIC: 8
};

export const GRENADE_TYPE = {
    FRAG: 0,
    SMOKE: 1,
    FLASHBANG: 2,
    MOLOTOV: 3,
    DECOY: 4,
    INCENDIARY: 5,
    FLASH: 6
};

export const TRAINING_SESSION_STATUS = {
    NOT_STARTED: 0,
    IN_PROGRESS: 1,
    PAUSED: 2,
    COMPLETED: 3,
    FAILED: 4
};

export const TRAINING_RESULT = {
    PASSED: 0,
    FAILED: 1,
    INCOMPLETE: 2,
    EXCELLENT: 3,
    PERFECT: 4
};

export class TrainingScenario {
    constructor(id, name) {
        this.id = id;
        this.name = name;
        this.description = '';
        this.category = TRAINING_MODE.NONE;
        this.difficulty = TRAINING_DIFFICULTY.NORMAL;
        this.duration = 60;
        this.objectives = [];
        this.prerequisites = [];
        this.rewards = {};
        this.settings = {};
        this.statistics = null;
        this.bestScore = null;
        this.averageScore = 0;
        this.completedCount = 0;
        this.totalTimeSpent = 0;
        this.createdAt = Date.now();
        this.modifiedAt = Date.now();
        this.isCustom = false;
        this.creator = null;
        this.version = '1.0';
        this.enabled = true;
    }

    addObjective(id, name, description, target, type = 'count') {
        this.objectives.push({
            id,
            name,
            description,
            target,
            type,
            current: 0,
            completed: false,
            bonus: false,
            timeLimit: null
        });
        return this;
    }

    setDifficulty(difficulty) {
        this.difficulty = difficulty;
        return this;
    }

    setDuration(seconds) {
        this.duration = seconds;
        return this;
    }

    addPrerequisite(scenarioId) {
        this.prerequisites.push(scenarioId);
        return this;
    }

    setReward(type, amount) {
        this.rewards[type] = amount;
        return this;
    }

    addSetting(key, value) {
        this.settings[key] = value;
        return this;
    }

    isUnlocked(completedScenarios) {
        for (const prereq of this.prerequisites) {
            if (!completedScenarios.includes(prereq)) {
                return false;
            }
        }
        return true;
    }

    resetProgress() {
        for (const objective of this.objectives) {
            objective.current = 0;
            objective.completed = false;
        }
    }

    updateObjective(objectiveId, value) {
        const objective = this.objectives.find(o => o.id === objectiveId);
        if (objective) {
            objective.current = value;
            if (objective.current >= objective.target) {
                objective.completed = true;
            }
        }
    }

    incrementObjective(objectiveId, amount = 1) {
        const objective = this.objectives.find(o => o.id === objectiveId);
        if (objective) {
            this.updateObjective(objectiveId, objective.current + amount);
        }
    }

    getProgress() {
        const total = this.objectives.length;
        const completed = this.objectives.filter(o => o.completed).length;
        return total > 0 ? completed / total : 0;
    }

    isComplete() {
        return this.objectives.every(o => o.completed);
    }

    getTimeProgress(elapsedTime) {
        return Math.min(1, elapsedTime / this.duration);
    }

    clone() {
        const cloned = new TrainingScenario(this.id + '_copy', this.name + ' (Copy)');
        cloned.description = this.description;
        cloned.category = this.category;
        cloned.difficulty = this.difficulty;
        cloned.duration = this.duration;
        cloned.objectives = this.objectives.map(o => ({ ...o }));
        cloned.prerequisites = [...this.prerequisites];
        cloned.rewards = { ...this.rewards };
        cloned.settings = { ...this.settings };
        cloned.isCustom = true;
        return cloned;
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            description: this.description,
            category: this.category,
            difficulty: this.difficulty,
            duration: this.duration,
            objectives: this.objectives,
            prerequisites: this.prerequisites,
            rewards: this.rewards,
            settings: this.settings,
            isCustom: this.isCustom,
            version: this.version
        };
    }

    static fromJSON(data) {
        const scenario = new TrainingScenario(data.id, data.name);
        Object.assign(scenario, data);
        return scenario;
    }
}

export class AimBot {
    constructor(type = AIM_BOT_TYPE.STATIC) {
        this.type = type;
        this.active = false;
        this.target = null;
        this.position = { x: 0, y: 0, z: 0 };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.rotation = 0;
        this.rotationSpeed = 0;
        this.health = 100;
        this.size = 10;
        this.color = { r: 255, g: 0, b: 0 };
        this.headshotMultiplier = 2;
        this.movementBounds = {
            minX: -500,
            maxX: 500,
            minY: -500,
            maxY: 500,
            minZ: 0,
            maxZ: 200
        };
        this.spawnPosition = { ...this.position };
        this.lastHitTime = 0;
        this.totalHits = 0;
        this.headshots = 0;
        this.missCount = 0;
        this.movementPattern = [];
        this.currentPatternIndex = 0;
        this.patternTimer = 0;
        this.randomDirectionChangeTime = 1;
        this.lastDirectionChange = 0;
        this.currentDirection = { x: 1, y: 0, z: 0 };
        this.speed = 100;
        this.acceleration = 0;
        this.deceleration = 0;
        this.teleportEnabled = false;
        this.teleportInterval = 5;
        this.lastTeleportTime = 0;
    }

    spawn(position) {
        this.position = { ...position };
        this.spawnPosition = { ...position };
        this.velocity = { x: 0, y: 0, z: 0 };
        this.health = 100;
        this.active = true;
        this.totalHits = 0;
        this.headshots = 0;
        this.missCount = 0;
        this.lastHitTime = 0;
        this.currentDirection = { x: 1, y: 0, z: 0 };
    }

    update(deltaTime) {
        if (!this.active) return;

        switch (this.type) {
            case AIM_BOT_TYPE.STATIC:
                break;
            case AIM_BOT_TYPE.MOVING:
                this.updateMoving(deltaTime);
                break;
            case AIM_BOT_TYPE.CIRCLING:
                this.updateCircling(deltaTime);
                break;
            case AIM_BOT_TYPE.RANDOM:
                this.updateRandom(deltaTime);
                break;
            case AIM_BOT_TYPE.ZIGZAG:
                this.updateZigzag(deltaTime);
                break;
            case AIM_BOT_TYPE.FLEEING:
                this.updateFleeing(deltaTime);
                break;
            case AIM_BOT_TYPE.AGGRESSIVE:
                this.updateAggressive(deltaTime);
                break;
        }

        if (this.teleportEnabled) {
            const now = Date.now();
            if (now - this.lastTeleportTime > this.teleportInterval * 1000) {
                this.teleport();
                this.lastTeleportTime = now;
            }
        }

        this.enforceBounds();
    }

    updateMoving(deltaTime) {
        this.position.x += this.currentDirection.x * this.speed * deltaTime;
        this.position.y += this.currentDirection.y * this.speed * deltaTime;
        this.position.z += this.currentDirection.z * this.speed * deltaTime;

        const now = Date.now();
        if (now - this.lastDirectionChange > this.randomDirectionChangeTime * 1000) {
            this.randomizeDirection();
            this.lastDirectionChange = now;
        }
    }

    updateCircling(deltaTime) {
        this.rotation += this.rotationSpeed * deltaTime;

        const centerX = this.spawnPosition.x;
        const centerY = this.spawnPosition.y;
        const radius = 200;

        this.position.x = centerX + Math.cos(this.rotation) * radius;
        this.position.y = centerY + Math.sin(this.rotation) * radius;
    }

    updateRandom(deltaTime) {
        this.position.x += this.currentDirection.x * this.speed * deltaTime;
        this.position.y += this.currentDirection.y * this.speed * deltaTime;
        this.position.z += this.currentDirection.z * this.speed * deltaTime;

        const now = Date.now();
        if (now - this.lastDirectionChange > this.randomDirectionChangeTime * 1000) {
            const magnitude = Math.sqrt(
                this.currentDirection.x * this.currentDirection.x +
                this.currentDirection.y * this.currentDirection.y
            );

            if (magnitude > 0.1) {
                this.currentDirection.x = -this.currentDirection.y / magnitude + (Math.random() - 0.5) * 0.5;
                this.currentDirection.y = this.currentDirection.x / magnitude + (Math.random() - 0.5) * 0.5;
            } else {
                this.currentDirection.x = Math.random() - 0.5;
                this.currentDirection.y = Math.random() - 0.5;
            }

            const len = Math.sqrt(this.currentDirection.x * this.currentDirection.x + this.currentDirection.y * this.currentDirection.y);
            this.currentDirection.x /= len;
            this.currentDirection.y /= len;

            this.lastDirectionChange = now;
        }
    }

    updateZigzag(deltaTime) {
        this.position.x += this.currentDirection.x * this.speed * deltaTime;
        this.position.y += this.currentDirection.y * this.speed * deltaTime;
        this.position.z += this.currentDirection.z * this.speed * deltaTime;

        this.patternTimer += deltaTime;

        if (this.patternTimer > 1) {
            this.currentDirection.x = -this.currentDirection.x;
            this.patternTimer = 0;
        }
    }

    updateFleeing(deltaTime) {
        if (this.target) {
            const dx = this.position.x - this.target.position.x;
            const dy = this.position.y - this.target.position.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist > 0) {
                this.currentDirection.x = dx / dist;
                this.currentDirection.y = dy / dist;
            }
        }

        this.position.x += this.currentDirection.x * this.speed * deltaTime;
        this.position.y += this.currentDirection.y * this.speed * deltaTime;
    }

    updateAggressive(deltaTime) {
        if (this.target) {
            const dx = this.target.position.x - this.position.x;
            const dy = this.target.position.y - this.position.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist > 100) {
                this.currentDirection.x = dx / dist;
                this.currentDirection.y = dy / dist;
            } else {
                this.currentDirection.x = (Math.random() - 0.5) * 2;
                this.currentDirection.y = (Math.random() - 0.5) * 2;
            }
        }

        this.position.x += this.currentDirection.x * this.speed * deltaTime;
        this.position.y += this.currentDirection.y * this.speed * deltaTime;
    }

    enforceBounds() {
        this.position.x = Math.max(this.movementBounds.minX, Math.min(this.movementBounds.maxX, this.position.x));
        this.position.y = Math.max(this.movementBounds.minY, Math.min(this.movementBounds.maxY, this.position.y));
        this.position.z = Math.max(this.movementBounds.minZ, Math.min(this.movementBounds.maxZ, this.position.z));

        if (this.position.x <= this.movementBounds.minX || this.position.x >= this.movementBounds.maxX) {
            this.currentDirection.x = -this.currentDirection.x;
        }
        if (this.position.y <= this.movementBounds.minY || this.position.y >= this.movementBounds.maxY) {
            this.currentDirection.y = -this.currentDirection.y;
        }
    }

    randomizeDirection() {
        const angle = Math.random() * Math.PI * 2;
        this.currentDirection.x = Math.cos(angle);
        this.currentDirection.y = Math.sin(angle);
    }

    teleport() {
        this.position.x = this.movementBounds.minX + Math.random() * (this.movementBounds.maxX - this.movementBounds.minX);
        this.position.y = this.movementBounds.minY + Math.random() * (this.movementBounds.maxY - this.movementBounds.minY);
        this.position.z = this.movementBounds.minZ + Math.random() * (this.movementBounds.maxZ - this.movementBounds.minZ);
    }

    takeDamage(amount, headshot = false) {
        this.health -= amount;
        this.totalHits++;
        if (headshot) {
            this.headshots++;
        }
        this.lastHitTime = Date.now();

        if (this.health <= 0) {
            this.respawn();
            return true;
        }
        return false;
    }

    respawn() {
        this.position = { ...this.spawnPosition };
        this.health = 100;
        this.velocity = { x: 0, y: 0, z: 0 };
    }

    setTarget(target) {
        this.target = target;
    }

    setSpeed(speed) {
        this.speed = speed;
    }

    setMovementBounds(bounds) {
        Object.assign(this.movementBounds, bounds);
    }

    setMovementPattern(pattern) {
        this.movementPattern = pattern;
        this.currentPatternIndex = 0;
    }

    setRotationSpeed(speed) {
        this.rotationSpeed = speed;
    }

    getHitAccuracy(totalShots) {
        return totalShots > 0 ? this.totalHits / totalShots : 0;
    }

    getHeadshotAccuracy() {
        return this.totalHits > 0 ? this.headshots / this.totalHits : 0;
    }

    getMovementSpeed() {
        return Math.sqrt(
            this.velocity.x * this.velocity.x +
            this.velocity.y * this.velocity.y +
            this.velocity.z * this.velocity.z
        );
    }

    getDistanceFromSpawn() {
        const dx = this.position.x - this.spawnPosition.x;
        const dy = this.position.y - this.spawnPosition.y;
        const dz = this.position.z - this.spawnPosition.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    getState() {
        return {
            position: { ...this.position },
            velocity: { ...this.velocity },
            health: this.health,
            active: this.active,
            totalHits: this.totalHits,
            headshots: this.headshots,
            missCount: this.missCount,
            timeSinceHit: Date.now() - this.lastHitTime
        };
    }

    reset() {
        this.spawn(this.spawnPosition);
        this.totalHits = 0;
        this.headshots = 0;
        this.missCount = 0;
    }

    destroy() {
        this.active = false;
        this.target = null;
    }
}

export class AimTrainingSession {
    constructor(scenario) {
        this.scenario = scenario;
        this.status = TRAINING_SESSION_STATUS.NOT_STARTED;
        this.startTime = 0;
        this.endTime = 0;
        this.elapsedTime = 0;
        this.pausedTime = 0;

        this.bots = [];
        this.botCount = 5;
        this.maxBotCount = 20;

        this.shotsFired = 0;
        this.shotsHit = 0;
        this.damageDealt = 0;
        this.headshots = 0;
        this.bodyshots = 0;

        this.currentScore = 0;
        this.highScore = 0;
        this.combo = 0;
        this.maxCombo = 0;
        this.lastHitTime = 0;

        this.hitStreak = 0;
        this.maxHitStreak = 0;
        this.missStreak = 0;

        this.averageHitTime = 0;
        this.totalHitTimes = [];
        this.lastSpawnTime = 0;
        this.spawnInterval = 3;

        this.accuracy = 0;
        this.hitsPerMinute = 0;
        this.damagePerMinute = 0;

        this.hitPositions = [];
        this.missPositions = [];

        this.achievementsUnlocked = [];
        this.objectiveProgress = new Map();

        this.settings = {
            infiniteAmmo: true,
            infiniteHealth: false,
            showCrosshair: true,
            showHitMarker: true,
            showDamageNumbers: true,
            showComboCounter: true,
            botHealth: 100,
            botSpeed: 100,
            autoRespawn: true,
            respawnDelay: 1
        };

        Object.assign(this.settings, scenario.settings);
    }

    initialize() {
        for (const objective of this.scenario.objectives) {
            this.objectiveProgress.set(objective.id, 0);
        }
    }

    start() {
        this.status = TRAINING_SESSION_STATUS.IN_PROGRESS;
        this.startTime = Date.now();
        this.elapsedTime = 0;
        this.resetStats();

        this.spawnBots();
    }

    pause() {
        if (this.status === TRAINING_SESSION_STATUS.IN_PROGRESS) {
            this.status = TRAINING_SESSION_STATUS.PAUSED;
            this.pauseTime = Date.now();
        }
    }

    resume() {
        if (this.status === TRAINING_SESSION_STATUS.PAUSED) {
            this.pausedTime += Date.now() - this.pauseTime;
            this.status = TRAINING_SESSION_STATUS.IN_PROGRESS;
        }
    }

    stop() {
        this.status = TRAINING_SESSION_STATUS.COMPLETED;
        this.endTime = Date.now();
        this.finalizeSession();
    }

    resetStats() {
        this.shotsFired = 0;
        this.shotsHit = 0;
        this.damageDealt = 0;
        this.headshots = 0;
        this.bodyshots = 0;
        this.currentScore = 0;
        this.combo = 0;
        this.maxCombo = 0;
        this.hitStreak = 0;
        this.maxHitStreak = 0;
        this.missStreak = 0;
        this.totalHitTimes = [];
        this.hitPositions = [];
        this.missPositions = [];
        this.lastSpawnTime = Date.now();

        for (const [id, _] of this.objectiveProgress) {
            this.objectiveProgress.set(id, 0);
        }
    }

    spawnBots() {
        for (let i = 0; i < this.botCount; i++) {
            const bot = new AimBot(AIM_BOT_TYPE.RANDOM);
            bot.spawn({
                x: (Math.random() - 0.5) * 1000,
                y: (Math.random() - 0.5) * 1000,
                z: 50 + Math.random() * 100
            });
            bot.setSpeed(this.settings.botSpeed);
            this.bots.push(bot);
        }
    }

    addBot(type = AIM_BOT_TYPE.RANDOM) {
        if (this.bots.length >= this.maxBotCount) return null;

        const bot = new AimBot(type);
        bot.spawn({
            x: (Math.random() - 0.5) * 1000,
            y: (Math.random() - 0.5) * 1000,
            z: 50 + Math.random() * 100
        });
        bot.setSpeed(this.settings.botSpeed);
        this.bots.push(bot);
        return bot;
    }

    removeBot(index) {
        if (index >= 0 && index < this.bots.length) {
            this.bots[index].destroy();
            this.bots.splice(index, 1);
        }
    }

    update(deltaTime) {
        if (this.status !== TRAINING_SESSION_STATUS.IN_PROGRESS) return;

        this.elapsedTime = Date.now() - this.startTime - this.pausedTime;

        for (const bot of this.bots) {
            bot.update(deltaTime);
        }

        if (this.settings.autoRespawn) {
            const now = Date.now();
            if (now - this.lastSpawnTime > this.spawnInterval * 1000) {
                const inactiveBot = this.bots.find(b => !b.active);
                if (inactiveBot) {
                    inactiveBot.spawn({
                        x: (Math.random() - 0.5) * 1000,
                        y: (Math.random() - 0.5) * 1000,
                        z: 50 + Math.random() * 100
                    });
                }
                this.lastSpawnTime = now;
            }
        }

        this.updateStatistics();
        this.checkObjectives();
        this.updateCombo(deltaTime);

        if (this.elapsedTime >= this.scenario.duration * 1000) {
            this.stop();
        }
    }

    updateStatistics() {
        this.accuracy = this.shotsFired > 0 ? this.shotsHit / this.shotsFired : 0;

        const minutes = this.elapsedTime / 60000;
        this.hitsPerMinute = minutes > 0 ? this.shotsHit / minutes : 0;
        this.damagePerMinute = minutes > 0 ? this.damageDealt / minutes : 0;
    }

    updateCombo(deltaTime) {
        const comboTimeout = 2000;
        const now = Date.now();

        if (now - this.lastHitTime > comboTimeout && this.combo > 0) {
            this.combo = 0;
        }
    }

    registerShot(position) {
        this.shotsFired++;
    }

    registerHit(botIndex, position, damage, headshot = false) {
        if (botIndex < 0 || botIndex >= this.bots.length) return;

        const bot = this.bots[botIndex];
        const killed = bot.takeDamage(damage, headshot);

        this.shotsHit++;
        this.damageDealt += damage;
        this.hitStreak++;
        this.missStreak = 0;

        if (this.hitStreak > this.maxHitStreak) {
            this.maxHitStreak = this.hitStreak;
        }

        if (headshot) {
            this.headshots++;
            this.currentScore += 100 * this.comboMultiplier;
        } else {
            this.bodyshots++;
            this.currentScore += 50 * this.comboMultiplier;
        }

        this.combo++;
        if (this.combo > this.maxCombo) {
            this.maxCombo = this.combo;
        }

        this.lastHitTime = Date.now();
        this.hitPositions.push({
            position: { ...position },
            time: this.elapsedTime,
            headshot
        });

        this.updateObjectiveProgress('hits', this.shotsHit);
        if (headshot) {
            this.updateObjectiveProgress('headshots', this.headshots);
        }
        this.updateObjectiveProgress('kills', this.bots.filter(b => !b.active).length);

        return killed;
    }

    registerMiss(position) {
        this.missCount++;
        this.hitStreak = 0;
        this.missStreak++;
        this.combo = 0;

        this.missPositions.push({
            position: { ...position },
            time: this.elapsedTime
        });
    }

    updateObjectiveProgress(objectiveId, value) {
        const progress = this.objectiveProgress.get(objectiveId);
        if (progress !== undefined) {
            this.objectiveProgress.set(objectiveId, value);
        }
    }

    checkObjectives() {
        for (const objective of this.scenario.objectives) {
            const progress = this.objectiveProgress.get(objective.id) || 0;

            if (objective.type === 'accuracy' && this.shotsFired > 0) {
                const accuracy = this.shotsHit / this.shotsFired;
                if (accuracy >= objective.target) {
                    objective.completed = true;
                }
            } else if (objective.type === 'count' && progress >= objective.target) {
                objective.completed = true;
            }
        }
    }

    getComboMultiplier() {
        return 1 + Math.min(this.combo, 10) * 0.1;
    }

    calculateScore() {
        let score = this.currentScore;

        const accuracyBonus = this.accuracy * 1000;
        score += accuracyBonus;

        const headshotBonus = this.headshots * 50;
        score += headshotBonus;

        const comboBonus = this.maxCombo * 10;
        score += comboBonus;

        const timeBonus = Math.max(0, this.scenario.duration - this.elapsedTime / 1000) * 10;
        score += timeBonus;

        const objectiveBonus = this.scenario.objectives.filter(o => o.completed).length * 100;
        score += objectiveBonus;

        return Math.floor(score);
    }

    getResult() {
        const completedObjectives = this.scenario.objectives.filter(o => o.completed).length;
        const totalObjectives = this.scenario.objectives.length;
        const completionRate = totalObjectives > 0 ? completedObjectives / totalObjectives : 0;

        let result = TRAINING_RESULT.FAILED;

        if (completionRate >= 1) {
            if (this.accuracy >= 0.8 && this.headshots / this.shotsHit >= 0.5) {
                result = TRAINING_RESULT.PERFECT;
            } else if (this.accuracy >= 0.6) {
                result = TRAINING_RESULT.EXCELLENT;
            } else {
                result = TRAINING_RESULT.PASSED;
            }
        } else if (completionRate >= 0.5) {
            result = TRAINING_RESULT.INCOMPLETE;
        }

        return {
            result,
            score: this.calculateScore(),
            accuracy: this.accuracy,
            hitsPerMinute: this.hitsPerMinute,
            damagePerMinute: this.damagePerMinute,
            headshotRate: this.shotsHit > 0 ? this.headshots / this.shotsHit : 0,
            maxCombo: this.maxCombo,
            maxHitStreak: this.maxHitStreak,
            completedObjectives,
            totalObjectives,
            timeElapsed: this.elapsedTime / 1000,
            stats: {
                shotsFired: this.shotsFired,
                shotsHit: this.shotsHit,
                damageDealt: this.damageDealt,
                headshots: this.headshots,
                bodyshots: this.bodyshots
            }
        };
    }

    finalizeSession() {
        const result = this.getResult();

        this.scenario.completedCount++;
        this.scenario.totalTimeSpent += this.elapsedTime;

        if (result.score > (this.scenario.bestScore || 0)) {
            this.scenario.bestScore = result.score;
        }

        this.scenario.averageScore =
            (this.scenario.averageScore * (this.scenario.completedCount - 1) + result.score) /
            this.scenario.completedCount;

        return result;
    }

    getProgress() {
        const objectiveProgress = {};
        for (const [id, value] of this.objectiveProgress) {
            objectiveProgress[id] = value;
        }

        return {
            status: this.status,
            elapsedTime: this.elapsedTime,
            duration: this.scenario.duration,
            progress: this.elapsedTime / (this.scenario.duration * 1000),
            currentScore: this.currentScore,
            accuracy: this.accuracy,
            combo: this.combo,
            hitsPerMinute: this.hitsPerMinute,
            botCount: this.bots.length,
            objectiveProgress,
            objectives: this.scenario.objectives
        };
    }

    reset() {
        this.status = TRAINING_SESSION_STATUS.NOT_STARTED;
        this.startTime = 0;
        this.endTime = 0;
        this.elapsedTime = 0;
        this.pausedTime = 0;
        this.resetStats();

        for (const bot of this.bots) {
            bot.reset();
        }

        for (const objective of this.scenario.objectives) {
            objective.current = 0;
            objective.completed = false;
        }
    }

    destroy() {
        for (const bot of this.bots) {
            bot.destroy();
        }
        this.bots = [];
        this.objectiveProgress.clear();
    }
}

export class RecoilPattern {
    constructor(name) {
        this.name = name;
        this.points = [];
        this.duration = 1.0;
        this.spreadMultiplier = 1.0;
        this.recoveryTime = 0.5;
        this.recoveryRate = 1.0;
    }

    addPoint(time, x, y, spread = 0) {
        this.points.push({ time, x, y, spread });
        return this;
    }

    getPointAt(time) {
        if (this.points.length === 0) return { x: 0, y: 0, spread: 0 };
        if (this.points.length === 1) return this.points[0];

        for (let i = 0; i < this.points.length - 1; i++) {
            const p1 = this.points[i];
            const p2 = this.points[i + 1];

            if (time >= p1.time && time <= p2.time) {
                const t = (time - p1.time) / (p2.time - p1.time);
                return {
                    x: p1.x + (p2.x - p1.x) * t,
                    y: p1.y + (p2.y - p1.y) * t,
                    spread: p1.spread + (p2.spread - p1.spread) * t
                };
            }
        }

        return this.points[this.points.length - 1];
    }

    static createAK47() {
        const pattern = new RecoilPattern('AK-47');
        pattern.addPoint(0, 0, 0, 0)
               .addPoint(0.05, 0, -2, 1)
               .addPoint(0.1, 1, -4, 2)
               .addPoint(0.15, 2, -6, 2)
               .addPoint(0.2, 2, -8, 3)
               .addPoint(0.25, 1, -10, 3)
               .addPoint(0.3, 0, -12, 3)
               .addPoint(0.35, -1, -13, 3)
               .addPoint(0.4, -1, -14, 3)
               .addPoint(0.45, -1, -15, 3)
               .addPoint(0.5, -1, -15, 2)
               .addPoint(0.55, 0, -15, 2)
               .addPoint(0.6, 0, -15, 2)
               .addPoint(0.7, 0, -15, 1)
               .addPoint(0.8, 0, -15, 1)
               .addPoint(0.9, 0, -15, 0)
               .addPoint(1.0, 0, 0, 0);
        pattern.duration = 1.0;
        return pattern;
    }

    static createM4A4() {
        const pattern = new RecoilPattern('M4A4');
        pattern.addPoint(0, 0, 0, 0)
               .addPoint(0.05, 0, -1.5, 1)
               .addPoint(0.1, 1, -3, 1)
               .addPoint(0.15, 1.5, -4.5, 2)
               .addPoint(0.2, 1.5, -6, 2)
               .addPoint(0.25, 1.5, -7, 2)
               .addPoint(0.3, 1, -8, 2)
               .addPoint(0.35, 0, -9, 2)
               .addPoint(0.4, -0.5, -10, 2)
               .addPoint(0.5, -0.5, -11, 2)
               .addPoint(0.6, 0, -11, 1)
               .addPoint(0.7, 0, -11, 1)
               .addPoint(0.8, 0, -11, 0)
               .addPoint(0.9, 0, -11, 0)
               .addPoint(1.0, 0, 0, 0);
        pattern.duration = 1.0;
        return pattern;
    }

    static createM4A1() {
        const pattern = new RecoilPattern('M4A1-S');
        pattern.addPoint(0, 0, 0, 0)
               .addPoint(0.05, 0, -1, 1)
               .addPoint(0.1, 0.5, -2, 1)
               .addPoint(0.15, 0.5, -3, 1)
               .addPoint(0.2, 0.5, -4, 1)
               .addPoint(0.25, 0.5, -4.5, 2)
               .addPoint(0.3, 0, -5, 2)
               .addPoint(0.35, 0, -5.5, 2)
               .addPoint(0.4, 0, -6, 2)
               .addPoint(0.5, 0, -6, 1)
               .addPoint(0.6, 0, -6, 1)
               .addPoint(0.7, 0, -6, 0)
               .addPoint(0.8, 0, -6, 0)
               .addPoint(1.0, 0, 0, 0);
        pattern.duration = 1.0;
        return pattern;
    }

    static createAWP() {
        const pattern = new RecoilPattern('AWP');
        pattern.addPoint(0, 0, 0, 0)
               .addPoint(0.1, 0, -4, 1)
               .addPoint(0.2, 0, -8, 2)
               .addPoint(0.3, 0, -12, 2)
               .addPoint(0.4, 0, -16, 2)
               .addPoint(0.5, 0, -20, 2)
               .addPoint(0.6, 0, -24, 1)
               .addPoint(0.7, 0, -28, 1)
               .addPoint(0.8, 0, -30, 1)
               .addPoint(1.0, 0, 0, 0);
        pattern.duration = 1.2;
        return pattern;
    }

    static createGalil() {
        const pattern = new RecoilPattern('Galil');
        pattern.addPoint(0, 0, 0, 0)
               .addPoint(0.05, 0, -2, 1)
               .addPoint(0.1, -1, -4, 2)
               .addPoint(0.15, -2, -6, 2)
               .addPoint(0.2, -3, -8, 3)
               .addPoint(0.25, -4, -10, 3)
               .addPoint(0.3, -4, -12, 3)
               .addPoint(0.35, -4, -14, 3)
               .addPoint(0.4, -3, -16, 3)
               .addPoint(0.5, -2, -18, 2)
               .addPoint(0.6, -1, -20, 2)
               .addPoint(0.7, 0, -22, 2)
               .addPoint(0.8, 0, -24, 1)
               .addPoint(1.0, 0, 0, 0);
        pattern.duration = 1.0;
        return pattern;
    }

    static createFamas() {
        const pattern = new RecoilPattern('Famas');
        pattern.addPoint(0, 0, 0, 0)
               .addPoint(0.05, 2, -2, 1)
               .addPoint(0.1, 4, -4, 2)
               .addPoint(0.15, 6, -6, 2)
               .addPoint(0.2, 8, -7, 3)
               .addPoint(0.25, 10, -8, 3)
               .addPoint(0.3, 10, -9, 3)
               .addPoint(0.35, 10, -10, 3)
               .addPoint(0.4, 8, -11, 2)
               .addPoint(0.5, 6, -12, 2)
               .addPoint(0.6, 4, -13, 2)
               .addPoint(0.7, 2, -14, 1)
               .addPoint(0.8, 0, -15, 1)
               .addPoint(1.0, 0, 0, 0);
        pattern.duration = 1.0;
        return pattern;
    }
}

export class SprayTrainingSession {
    constructor(scenario) {
        this.scenario = scenario;
        this.status = TRAINING_SESSION_STATUS.NOT_STARTED;
        this.startTime = 0;
        this.endTime = 0;
        this.elapsedTime = 0;

        this.pattern = RecoilPattern.createAK47();
        this.weapons = ['ak47', 'm4a4', 'm4a1', 'awp', 'galil', 'famas'];
        this.currentWeaponIndex = 0;

        this.shotsFired = 0;
        this.shotsHit = 0;
        this.patternAccuracy = 0;
        this.averageDeviation = 0;
        this.totalDeviation = 0;
        this.bestAccuracy = 0;

        this.currentShot = 0;
        this.patternStartPosition = { x: 0, y: 0 };
        this.lastShotTime = 0;
        this.shotInterval = 0.1;

        this.recoilData = [];
        this.trainingShots = 50;

        this.settings = {
            showPattern: true,
            showDeviation: true,
            autoCompensate: false,
            measureAccuracy: true
        };

        Object.assign(this.settings, scenario.settings);
    }

    start() {
        this.status = TRAINING_SESSION_STATUS.IN_PROGRESS;
        this.startTime = Date.now();
        this.elapsedTime = 0;
        this.resetStats();
    }

    resetStats() {
        this.shotsFired = 0;
        this.shotsHit = 0;
        this.patternAccuracy = 0;
        this.averageDeviation = 0;
        this.totalDeviation = 0;
        this.bestAccuracy = 0;
        this.currentShot = 0;
        this.recoilData = [];
        this.lastShotTime = 0;
    }

    setWeapon(weaponName) {
        const index = this.weapons.indexOf(weaponName.toLowerCase());
        if (index >= 0) {
            this.currentWeaponIndex = index;
            this.loadPatternForWeapon(weaponName);
        }
    }

    loadPatternForWeapon(weaponName) {
        switch (weaponName.toLowerCase()) {
            case 'ak47':
                this.pattern = RecoilPattern.createAK47();
                break;
            case 'm4a4':
                this.pattern = RecoilPattern.createM4A4();
                break;
            case 'm4a1':
            case 'm4a1-s':
                this.pattern = RecoilPattern.createM4A1();
                break;
            case 'awp':
                this.pattern = RecoilPattern.createAWP();
                break;
            case 'galil':
                this.pattern = RecoilPattern.createGalil();
                break;
            case 'famas':
                this.pattern = RecoilPattern.createFamas();
                break;
            default:
                this.pattern = RecoilPattern.createAK47();
        }
    }

    update(deltaTime) {
        if (this.status !== TRAINING_SESSION_STATUS.IN_PROGRESS) return;

        this.elapsedTime = Date.now() - this.startTime;

        if (this.settings.autoCompensate) {
            const compensation = this.pattern.getPointAt(this.elapsedTime / 1000);
        }

        if (this.shotsFired >= this.trainingShots) {
            this.stop();
        }
    }

    registerShot(position, timeSinceStart) {
        this.shotsFired++;
        this.lastShotTime = Date.now();

        const expectedPoint = this.pattern.getPointAt(timeSinceStart);

        const deviation = Math.sqrt(
            Math.pow(position.x - expectedPoint.x, 2) +
            Math.pow(position.y - expectedPoint.y, 2)
        );

        this.recoilData.push({
            shotNumber: this.shotsFired,
            time: timeSinceStart,
            expected: expectedPoint,
            actual: position,
            deviation,
            spread: expectedPoint.spread * this.pattern.spreadMultiplier
        });

        this.totalDeviation += deviation;
        this.averageDeviation = this.totalDeviation / this.shotsFired;

        this.patternAccuracy = 1 - (this.averageDeviation / 50);

        if (this.patternAccuracy > this.bestAccuracy) {
            this.bestAccuracy = this.patternAccuracy;
        }
    }

    calculateScore() {
        let score = 0;

        score += this.patternAccuracy * 5000;

        score += (1 - this.averageDeviation / 100) * 3000;

        const perfectShots = this.recoilData.filter(d => d.deviation < 5).length;
        score += perfectShots * 20;

        const goodShots = this.recoilData.filter(d => d.deviation < 15).length;
        score += goodShots * 10;

        return Math.floor(score);
    }

    stop() {
        this.status = TRAINING_SESSION_STATUS.COMPLETED;
        this.endTime = Date.now();
    }

    getProgress() {
        return {
            status: this.status,
            elapsedTime: this.elapsedTime,
            shotsFired: this.shotsFired,
            totalShots: this.trainingShots,
            patternAccuracy: this.patternAccuracy,
            averageDeviation: this.averageDeviation,
            bestAccuracy: this.bestAccuracy,
            currentWeapon: this.weapons[this.currentWeaponIndex],
            pattern: this.pattern
        };
    }

    getResult() {
        return {
            score: this.calculateScore(),
            patternAccuracy: this.patternAccuracy,
            averageDeviation: this.averageDeviation,
            bestAccuracy: this.bestAccuracy,
            totalShots: this.shotsFired,
            recoilData: this.recoilData,
            weapon: this.weapons[this.currentWeaponIndex],
            timeElapsed: this.elapsedTime / 1000
        };
    }

    reset() {
        this.status = TRAINING_SESSION_STATUS.NOT_STARTED;
        this.startTime = 0;
        this.endTime = 0;
        this.elapsedTime = 0;
        this.resetStats();
    }
}

export class TrainingManager {
    constructor() {
        this.scenarios = new Map();
        this.customScenarios = new Map();
        this.currentScenario = null;
        this.currentSession = null;
        this.completedScenarios = new Set();
        this.totalTimeSpent = 0;
        this.totalSessionsCompleted = 0;

        this.settings = {
            showTutorialHints: true,
            autoAim: false,
            unlimitedAmmo: true,
            instantRespawn: true,
            difficultyScaling: true
        };

        this.statistics = {
            totalShotsFired: 0,
            totalShotsHit: 0,
            totalAccuracy: 0,
            totalHeadshots: 0,
            totalDamageDealt: 0,
            totalBotsKilled: 0,
            bestScores: new Map(),
            averageScores: new Map()
        };

        this.initializeDefaultScenarios();
    }

    initializeDefaultScenarios() {
        this.createAimTrainingScenarios();
        this.createRecoilTrainingScenarios();
        this.createMovementScenarios();
        this.createGrenadeScenarios();
    }

    createAimTrainingScenarios() {
        const beginnerAim = new TrainingScenario('aim_beginner', 'Beginner Aim Training');
        beginnerAim.category = TRAINING_MODE.AIM_TRAINING;
        beginnerAim.difficulty = TRAINING_DIFFICULTY.BEGINNER;
        beginnerAim.duration = 60;
        beginnerAim.description = 'Learn the basics of aiming at stationary targets';
        beginnerAim.addObjective('hits', 'Total Hits', 'Hit targets', 30, 'count')
                   .addObjective('accuracy', 'Accuracy', 'Maintain accuracy', 0.4, 'accuracy')
                   .addObjective('kills', 'Targets Eliminated', 'Eliminate targets', 15, 'count');
        beginnerAim.addSetting('botSpeed', 50);
        beginnerAim.addSetting('botCount', 3);
        beginnerAim.addSetting('botType', AIM_BOT_TYPE.STATIC);
        this.scenarios.set(beginnerAim.id, beginnerAim);

        const intermediateAim = new TrainingScenario('aim_intermediate', 'Intermediate Aim Training');
        intermediateAim.category = TRAINING_MODE.AIM_TRAINING;
        intermediateAim.difficulty = TRAINING_DIFFICULTY.NORMAL;
        intermediateAim.duration = 90;
        intermediateAim.description = 'Practice against moving targets';
        intermediateAim.addObjective('hits', 'Total Hits', 'Hit targets', 60, 'count')
                      .addObjective('headshots', 'Headshots', 'Land headshots', 20, 'count')
                      .addObjective('accuracy', 'Accuracy', 'Maintain accuracy', 0.5, 'accuracy')
                      .addObjective('kills', 'Targets Eliminated', 'Eliminate targets', 30, 'count');
        intermediateAim.addPrerequisite('aim_beginner');
        intermediateAim.addSetting('botSpeed', 100);
        intermediateAim.addSetting('botCount', 5);
        intermediateAim.addSetting('botType', AIM_BOT_TYPE.MOVING);
        this.scenarios.set(intermediateAim.id, intermediateAim);

        const advancedAim = new TrainingScenario('aim_advanced', 'Advanced Aim Training');
        advancedAim.category = TRAINING_MODE.AIM_TRAINING;
        advancedAim.difficulty = TRAINING_DIFFICULTY.HARD;
        advancedAim.duration = 120;
        advancedAim.description = 'Challenge yourself with fast and erratic targets';
        advancedAim.addObjective('hits', 'Total Hits', 'Hit targets', 100, 'count')
                  .addObjective('headshots', 'Headshots', 'Land headshots', 40, 'count')
                  .addObjective('accuracy', 'Accuracy', 'Maintain accuracy', 0.6, 'accuracy')
                  .addObjective('streak', 'Hit Streak', 'Achieve hit streak', 15, 'count');
        advancedAim.addPrerequisite('aim_intermediate');
        advancedAim.addSetting('botSpeed', 200);
        advancedAim.addSetting('botCount', 8);
        advancedAim.addSetting('botType', AIM_BOT_TYPE.ERRATIC);
        advancedAim.addSetting('teleportEnabled', true);
        this.scenarios.set(advancedAim.id, advancedAim);
    }

    createRecoilTrainingScenarios() {
        const ak47Recoil = new TrainingScenario('recoil_ak47', 'AK-47 Spray Control');
        ak47Recoil.category = TRAINING_MODE.RECOIL_TRAINING;
        ak47Recoil.difficulty = TRAINING_DIFFICULTY.NORMAL;
        ak47Recoil.duration = 120;
        ak47Recoil.description = 'Master the AK-47 recoil pattern';
        ak47Recoil.addObjective('shots', 'Shots Fired', 'Fire at targets', 100, 'count')
                  .addObjective('accuracy', 'Accuracy', 'Maintain accuracy', 0.4, 'accuracy')
                  .addObjective('pattern_control', 'Pattern Control', 'Control recoil', 0.6, 'count');
        ak47Recoil.addSetting('weapon', 'ak47');
        this.scenarios.set(ak47Recoil.id, ak47Recoil);

        const m4Recoil = new TrainingScenario('recoil_m4', 'M4 Spray Control');
        m4Recoil.category = TRAINING_MODE.RECOIL_TRAINING;
        m4Recoil.difficulty = TRAINING_DIFFICULTY.NORMAL;
        m4Recoil.duration = 120;
        m4Recoil.description = 'Master the M4 recoil patterns';
        m4Recoil.addObjective('shots', 'Shots Fired', 'Fire at targets', 100, 'count')
                .addObjective('accuracy', 'Accuracy', 'Maintain accuracy', 0.45, 'accuracy')
                .addObjective('pattern_control', 'Pattern Control', 'Control recoil', 0.6, 'count');
        m4Recoil.addPrerequisite('recoil_ak47');
        m4Recoil.addSetting('weapon', 'm4a4');
        this.scenarios.set(m4Recoil.id, m4Recoil);
    }

    createMovementScenarios() {
        const movementBasic = new TrainingScenario('movement_basic', 'Basic Movement');
        movementBasic.category = TRAINING_MODE.MOVEMENT_TRAINING;
        movementBasic.difficulty = TRAINING_DIFFICULTY.BEGINNER;
        movementBasic.duration = 60;
        movementBasic.description = 'Learn basic movement mechanics';
        movementBasic.addObjective('distance', 'Distance Traveled', 'Cover distance', 500, 'count')
                    .addObjective('strafes', 'Direction Changes', 'Change direction', 20, 'count');
        this.scenarios.set(movementBasic.id, movementBasic);
    }

    createGrenadeScenarios() {
        const smokeTraining = new TrainingScenario('nade_smoke', 'Smoke Grenade Training');
        smokeTraining.category = TRAINING_MODE.GRENADE_TRAINING;
        smokeTraining.difficulty = TRAINING_DIFFICULTY.NORMAL;
        smokeTraining.duration = 120;
        smokeTraining.description = 'Learn smoke grenade lineups';
        smokeTraining.addObjective('smokes_landed', 'Smokes Landed', 'Land smoke grenades', 10, 'count')
                    .addObjective('max_range', 'Max Range', 'Land smoke at max range', 3, 'count');
        this.scenarios.set(smokeTraining.id, smokeTraining);

        const molotovTraining = new TrainingScenario('nade_molotov', 'Molotov Training');
        molotovTraining.category = TRAINING_MODE.GRENADE_TRAINING;
        molotovTraining.difficulty = TRAINING_DIFFICULTY.HARD;
        molotovTraining.duration = 120;
        molotovTraining.description = 'Learn molotov lineups and usage';
        molotovTraining.addObjective('molotovs_landed', 'Molotovs Landed', 'Land molotov grenades', 10, 'count')
                       .addObjective('damage_dealt', 'Damage Dealt', 'Deal damage with molotovs', 500, 'count');
        molotovTraining.addPrerequisite('nade_smoke');
        this.scenarios.set(molotovTraining.id, molotovTraining);
    }

    getScenario(id) {
        return this.scenarios.get(id) || this.customScenarios.get(id);
    }

    getScenariosByCategory(category) {
        const results = [];
        for (const scenario of this.scenarios.values()) {
            if (scenario.category === category && scenario.enabled) {
                results.push(scenario);
            }
        }
        return results;
    }

    getUnlockedScenarios() {
        const unlocked = [];
        for (const scenario of this.scenarios.values()) {
            if (scenario.isUnlocked(Array.from(this.completedScenarios))) {
                unlocked.push(scenario);
            }
        }
        return unlocked;
    }

    getRecommendedScenario() {
        const unlocked = this.getUnlockedScenarios();
        const notCompleted = unlocked.filter(s => !this.completedScenarios.has(s.id));

        if (notCompleted.length > 0) {
            return notCompleted.sort((a, b) => {
                if (a.difficulty !== b.difficulty) {
                    return a.difficulty - b.difficulty;
                }
                return a.createdAt - b.createdAt;
            })[0];
        }

        return unlocked[0] || null;
    }

    startScenario(scenarioId) {
        const scenario = this.getScenario(scenarioId);
        if (!scenario) return null;

        if (!scenario.isUnlocked(Array.from(this.completedScenarios))) {
            return null;
        }

        if (scenario.category === TRAINING_MODE.AIM_TRAINING) {
            this.currentSession = new AimTrainingSession(scenario);
        } else if (scenario.category === TRAINING_MODE.RECOIL_TRAINING) {
            this.currentSession = new SprayTrainingSession(scenario);
        } else {
            this.currentSession = new AimTrainingSession(scenario);
        }

        this.currentScenario = scenario;
        this.currentSession.initialize();
        this.currentSession.start();

        return this.currentSession;
    }

    update(deltaTime) {
        if (this.currentSession) {
            this.currentSession.update(deltaTime);

            if (this.currentSession.status === TRAINING_SESSION_STATUS.COMPLETED) {
                this.handleSessionComplete();
            }
        }
    }

    handleSessionComplete() {
        const result = this.currentSession.getResult();

        if (result.result === TRAINING_RESULT.PASSED ||
            result.result === TRAINING_RESULT.EXCELLENT ||
            result.result === TRAINING_RESULT.PERFECT) {
            this.completedScenarios.add(this.currentScenario.id);
        }

        this.totalSessionsCompleted++;
        this.totalTimeSpent += this.currentSession.elapsedTime;

        this.updateStatistics(result);
    }

    updateStatistics(result) {
        this.statistics.totalShotsFired += result.stats?.shotsFired || 0;
        this.statistics.totalShotsHit += result.stats?.shotsHit || 0;
        this.statistics.totalHeadshots += result.stats?.headshots || 0;
        this.statistics.totalDamageDealt += result.stats?.damageDealt || 0;

        if (result.stats?.shotsFired > 0) {
            this.statistics.totalAccuracy =
                this.statistics.totalShotsHit / this.statistics.totalShotsFired;
        }

        const currentBest = this.statistics.bestScores.get(this.currentScenario.id) || 0;
        if (result.score > currentBest) {
            this.statistics.bestScores.set(this.currentScenario.id, result.score);
        }

        const currentAverage = this.statistics.averageScores.get(this.currentScenario.id) || { sum: 0, count: 0 };
        currentAverage.sum += result.score;
        currentAverage.count++;
        this.statistics.averageScores.set(this.currentScenario.id, currentAverage);
    }

    pauseSession() {
        if (this.currentSession) {
            this.currentSession.pause();
        }
    }

    resumeSession() {
        if (this.currentSession) {
            this.currentSession.resume();
        }
    }

    stopSession() {
        if (this.currentSession) {
            this.currentSession.stop();
            this.handleSessionComplete();
        }
    }

    resetCurrentScenario() {
        if (this.currentSession) {
            this.currentSession.reset();
        }
    }

    createCustomScenario(data) {
        const scenario = new TrainingScenario(
            `custom_${Date.now()}`,
            data.name || 'Custom Scenario'
        );
        scenario.description = data.description || '';
        scenario.category = data.category || TRAINING_MODE.CUSTOM;
        scenario.difficulty = data.difficulty || TRAINING_DIFFICULTY.NORMAL;
        scenario.duration = data.duration || 60;
        scenario.objectives = data.objectives || [];
        scenario.settings = data.settings || {};
        scenario.isCustom = true;
        scenario.creator = data.creator || null;

        this.customScenarios.set(scenario.id, scenario);
        return scenario;
    }

    deleteCustomScenario(id) {
        return this.customScenarios.delete(id);
    }

    getProgress() {
        const unlocked = this.getUnlockedScenarios();
        const completed = unlocked.filter(s => this.completedScenarios.has(s.id));

        return {
            totalScenarios: this.scenarios.size + this.customScenarios.size,
            unlockedScenarios: unlocked.length,
            completedScenarios: completed.length,
            totalTimeSpent: this.totalTimeSpent,
            totalSessionsCompleted: this.totalSessionsCompleted,
            overallProgress: unlocked.length > 0 ?
                (completed.length / unlocked.length) * 100 : 0
        };
    }

    getStatistics() {
        return {
            ...this.statistics,
            bestScores: Object.fromEntries(this.statistics.bestScores),
            averageScores: Object.fromEntries(
                Array.from(this.statistics.averageScores.entries()).map(([id, data]) => [
                    id,
                    data.count > 0 ? data.sum / data.count : 0
                ])
            )
        };
    }

    saveProgress() {
        return {
            completedScenarios: Array.from(this.completedScenarios),
            totalTimeSpent: this.totalTimeSpent,
            totalSessionsCompleted: this.totalSessionsCompleted,
            statistics: this.statistics,
            settings: this.settings,
            customScenarios: Array.from(this.customScenarios.values()).map(s => s.toJSON())
        };
    }

    loadProgress(data) {
        if (data.completedScenarios) {
            this.completedScenarios = new Set(data.completedScenarios);
        }
        if (data.totalTimeSpent) {
            this.totalTimeSpent = data.totalTimeSpent;
        }
        if (data.totalSessionsCompleted) {
            this.totalSessionsCompleted = data.totalSessionsCompleted;
        }
        if (data.statistics) {
            Object.assign(this.statistics, data.statistics);
        }
        if (data.settings) {
            Object.assign(this.settings, data.settings);
        }
        if (data.customScenarios) {
            for (const scenarioData of data.customScenarios) {
                const scenario = TrainingScenario.fromJSON(scenarioData);
                this.customScenarios.set(scenario.id, scenario);
            }
        }
    }
}

export default {
    TRAINING_MODE,
    TRAINING_DIFFICULTY,
    AIM_BOT_TYPE,
    GRENADE_TYPE,
    TRAINING_SESSION_STATUS,
    TRAINING_RESULT,
    TrainingScenario,
    AimBot,
    AimTrainingSession,
    RecoilPattern,
    SprayTrainingSession,
    TrainingManager
};

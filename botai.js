export class BotAI {
    constructor(config = {}) {
        this.id = config.id || BotAI.nextId++;
        this.name = config.name || 'Bot';
        this.personality = config.personality || this.createDefaultPersonality();
        this.behaviorTree = config.behaviorTree || this.createDefaultBehaviorTree();
        this.blackboard = new BotBlackboard();
        this.state = 'idle';
        this.previousState = 'idle';
        this.stateTime = 0;
        this.lastUpdate = Date.now();
        this.isActive = true;
        this.isAlive = true;
        this.target = null;
        this.waypoints = [];
        this.currentWaypointIndex = 0;
        this.lastKnownPosition = null;
        this.lastSeenTime = 0;
        this.hearingRange = config.hearingRange || 500;
        this.visionRange = config.visionRange || 800;
        this.visionAngle = config.visionAngle || 90;
        this.reactionTime = config.reactionTime || 0.2;
        this.accuracy = config.accuracy || 0.7;
        this.preferredDistance = config.preferredDistance || 400;
        this.aggression = config.aggression || 0.5;
        this.team = config.team || 'terrorists';
        this.role = config.role || 'soldier';
        this.skillLevel = config.skillLevel || 'normal';
        this.grenadeUsage = config.grenadeUsage || 0.5;
        this.communicationLevel = config.communicationLevel || 0.5;
        this.isVIP = config.isVIP || false;
        this.isHostageCarrier = config.isHostageCarrier || false;
        this.hasDefuser = config.hasDefuser || false;
        this.utility = {
            grenades: [],
            flashbangs: [],
            smokes: [],
            molotovs: []
        };
        this.loadout = config.loadout || this.getDefaultLoadout();
        this.decisionMaker = new BotDecisionMaker(this);
        this.pathPlanner = new BotPathPlanner();
        this.combatAI = new BotCombatAI(this);
        this.movementAI = new BotMovementAI(this);
        this.listeners = {
            onStateChange: [],
            onTargetAcquired: [],
            onTargetLost: [],
            onDeath: [],
            onKill: []
        };
    }

    static get nextId() {
        return BotAI._nextId++;
    }

    static get SKILL_LEVELS() {
        return {
            easy: {
                accuracy: 0.4,
                reactionTime: 0.5,
                awareness: 0.3,
                grenadeUsage: 0.2,
                preferredDistance: 600
            },
            normal: {
                accuracy: 0.6,
                reactionTime: 0.3,
                awareness: 0.5,
                grenadeUsage: 0.5,
                preferredDistance: 400
            },
            hard: {
                accuracy: 0.8,
                reactionTime: 0.15,
                awareness: 0.8,
                grenadeUsage: 0.8,
                preferredDistance: 300
            },
            expert: {
                accuracy: 0.95,
                reactionTime: 0.1,
                awareness: 0.95,
                grenadeUsage: 0.95,
                preferredDistance: 250
            }
        };
    }

    static get ROLES() {
        return {
            entry: { aggression: 0.9, preferredDistance: 200 },
            support: { aggression: 0.5, preferredDistance: 400 },
            awper: { aggression: 0.6, preferredDistance: 600 },
            lurker: { aggression: 0.7, preferredDistance: 800 },
            leader: { aggression: 0.6, preferredDistance: 400, communicationLevel: 0.9 },
            follower: { aggression: 0.4, preferredDistance: 350 },
            camper: { aggression: 0.3, preferredDistance: 100 },
            retaker: { aggression: 0.8, preferredDistance: 350 }
        };
    }

    static get PERSONALITY_TYPES() {
        return {
            aggressive: { aggression: 0.9, riskTaking: 0.8, patience: 0.3 },
            defensive: { aggression: 0.3, riskTaking: 0.2, patience: 0.8 },
            passive: { aggression: 0.4, riskTaking: 0.3, patience: 0.6 },
            reckless: { aggression: 1.0, riskTaking: 1.0, patience: 0.2 },
            cautious: { aggression: 0.5, riskTaking: 0.3, patience: 0.9 }
        };
    }

    createDefaultPersonality() {
        return {
            aggression: 0.5,
            riskTaking: 0.5,
            patience: 0.5,
            teamwork: 0.5,
            communication: 0.5,
            learningRate: 0.1
        };
    }

    createDefaultBehaviorTree() {
        return new BehaviorTree(this, [
            new SelectorNode([
                new SequenceNode([
                    new ConditionNode('isDead'),
                    new ActionNode('handleDeath')
                ]),
                new SequenceNode([
                    new ConditionNode('hasTarget'),
                    new SelectorNode([
                        new SequenceNode([
                            new ConditionNode('canEngage'),
                            new ActionNode('engageTarget')
                        ]),
                        new SequenceNode([
                            new ConditionNode('hasLastKnownPosition'),
                            new ActionNode('searchLastKnownPosition')
                        ]),
                        new ActionNode('loseTarget')
                    ])
                ]),
                new SequenceNode([
                    new ConditionNode('hasObjective'),
                    new SelectorNode([
                        new SequenceNode([
                            new ConditionNode('isObjectiveReachable'),
                            new ActionNode('moveToObjective')
                        ]),
                        new SequenceNode([
                            new ConditionNode('hasAlternativeRoute'),
                            new ActionNode('findAlternativeRoute')
                        ]),
                        new ActionNode('waitForOpening')
                    ])
                ]),
                new SequenceNode([
                    new ConditionNode('hasWaypoints'),
                    new ActionNode('followWaypoints')
                ]),
                new ActionNode('idle')
            ])
        ]);
    }

    getDefaultLoadout() {
        return {
            primary: 'ak47',
            secondary: 'deagle',
            grenades: [
                { type: 'he', count: 1 },
                { type: 'flash', count: 2 },
                { type: 'smoke', count: 1 }
            ],
            kit: false,
            armor: true
        };
    }

    update(dt) {
        if (!this.isActive) return;

        const now = Date.now();
        const deltaTime = (now - this.lastUpdate) / 1000;
        this.lastUpdate = now;
        this.stateTime += deltaTime;

        this.updatePerception();
        this.updateBlackboard();
        this.behaviorTree.tick(this.blackboard);
        this.updateState();

        if (this.target) {
            this.combatAI.update(dt);
        }

        this.movementAI.update(dt);
    }

    updatePerception() {
        const gameState = this.getGameState();

        this.blackboard.set('gameTime', gameState.time);
        this.blackboard.set('roundPhase', gameState.phase);
        this.blackboard.set('bombState', gameState.bombState);
        this.blackboard.set('aliveEnemies', gameState.aliveEnemies);
        this.blackboard.set('aliveTeammates', gameState.aliveTeammates);

        this.updateHearing(gameState);
        this.updateVision(gameState);
        this.updateMemory(gameState);
    }

    updateHearing(gameState) {
        const hearingEvents = [];

        for (const event of gameState.soundEvents) {
            const distance = this.position.distanceTo(event.position);
            if (distance <= this.hearingRange * (1 + this.personality.awareness * 0.5)) {
                hearingEvents.push({
                    ...event,
                    urgency: 1 - (distance / this.hearingRange),
                    distance
                });
            }
        }

        this.blackboard.set('hearingEvents', hearingEvents);
        this.blackboard.set('heardEnemyRecently', hearingEvents.some(e => e.type === 'footsteps'));
    }

    updateVision(gameState) {
        const visibleEnemies = [];

        for (const enemy of gameState.enemies) {
            if (this.canSee(enemy)) {
                visibleEnemies.push({
                    ...enemy,
                    distance: this.position.distanceTo(enemy.position),
                    angle: this.getAngleToTarget(enemy.position),
                    visible: true,
                    lastSeen: Date.now()
                });
            }
        }

        if (visibleEnemies.length > 0) {
            const closest = visibleEnemies.reduce((a, b) => a.distance < b.distance ? a : b);

            if (!this.target || this.target.id !== closest.id) {
                this.acquireTarget(closest);
            }

            this.lastSeenTime = Date.now();
            this.lastKnownPosition = closest.position.clone();
        }

        this.blackboard.set('visibleEnemies', visibleEnemies);
        this.blackboard.set('hasVisibleEnemies', visibleEnemies.length > 0);
    }

    updateMemory(gameState) {
        const memoryDecay = 0.95;
        const memory = this.blackboard.get('enemyMemory') || [];

        for (const entry of memory) {
            entry.confidence *= memoryDecay;
            entry.timeSinceSeen += 0.1;

            if (entry.timeSinceSeen > 10) {
                entry.confidence *= 0.5;
            }
        }

        this.blackboard.set('enemyMemory', memory.filter(m => m.confidence > 0.1));
    }

    canSee(enemy) {
        const distance = this.position.distanceTo(enemy.position);
        if (distance > this.visionRange) return false;

        const angle = this.getAngleToTarget(enemy.position);
        if (Math.abs(angle) > this.visionAngle / 2) return false;

        if (this.isLineOfSightBlocked(enemy.position)) return false;

        return true;
    }

    getAngleToTarget(targetPos) {
        const toTarget = targetPos.sub(this.position).normalize();
        const forward = this.getForwardVector();
        return Math.acos(forward.dot(toTarget)) * (180 / Math.PI);
    }

    isLineOfSightBlocked(targetPos) {
        return false;
    }

    acquireTarget(enemy) {
        const previousTarget = this.target;
        this.target = enemy;
        this.blackboard.set('target', enemy);
        this.blackboard.set('targetPosition', enemy.position.clone());
        this.blackboard.set('targetLastSeen', Date.now());

        if (this.listeners.onTargetAcquired.length > 0) {
            this.listeners.onTargetAcquired.forEach(cb => cb(enemy, previousTarget));
        }

        if (this.communicationLevel > 0.5 && previousTarget?.id !== enemy.id) {
            this.reportEnemyPosition(enemy);
        }
    }

    loseTarget() {
        if (this.target) {
            const targetId = this.target.id;
            this.addToMemory(targetId, this.lastKnownPosition, 0.8);

            if (this.listeners.onTargetLost.length > 0) {
                this.listeners.onTargetLost(this.target);
            }
        }

        this.target = null;
        this.blackboard.set('target', null);
        this.blackboard.set('hasTarget', false);
    }

    addToMemory(enemyId, position, confidence) {
        const memory = this.blackboard.get('enemyMemory') || [];
        const existing = memory.find(m => m.enemyId === enemyId);

        if (existing) {
            existing.position = position.clone();
            existing.confidence = Math.max(existing.confidence, confidence);
            existing.timeSinceSeen = 0;
        } else {
            memory.push({
                enemyId,
                position: position.clone(),
                confidence,
                timeSinceSeen: 0
            });
        }

        this.blackboard.set('enemyMemory', memory);
    }

    reportEnemyPosition(enemy) {
        console.log(`${this.name} reports: Enemy at ${enemy.position.x}, ${enemy.position.y}, ${enemy.position.z}`);
    }

    updateBlackboard() {
        this.blackboard.set('self', {
            id: this.id,
            position: this.position.clone(),
            health: this.health,
            armor: this.armor,
            hasPrimaryWeapon: this.hasPrimaryWeapon(),
            hasUtility: this.hasUtility(),
            isCrouching: this.isCrouching,
            isMoving: this.isMoving,
            isReloading: this.isReloading,
            isScoped: this.isScoped
        });

        this.blackboard.set('hasTarget', this.target !== null);
        this.blackboard.set('hasLastKnownPosition', this.lastKnownPosition !== null);
    }

    updateState() {
        if (this.state !== this.previousState) {
            this.previousState = this.state;
            this.stateTime = 0;

            if (this.listeners.onStateChange.length > 0) {
                this.listeners.onStateChange.forEach(cb => cb(this.state, this.previousState));
            }
        }
    }

    changeState(newState) {
        this.state = newState;
    }

    getGameState() {
        return {
            time: Date.now(),
            phase: 'active',
            bombState: 'planted',
            aliveEnemies: [],
            aliveTeammates: [],
            soundEvents: []
        };
    }

    hasPrimaryWeapon() {
        return true;
    }

    hasUtility() {
        return this.utility.grenades.length > 0 || this.utility.flashbangs.length > 0;
    }

    setPersonality(personalityType) {
        const personality = BotAI.PERSONALITY_TYPES[personalityType];
        if (personality) {
            Object.assign(this.personality, personality);
        }
    }

    setSkillLevel(level) {
        const skills = BotAI.SKILL_LEVELS[level];
        if (skills) {
            Object.assign(this, skills);
        }
        this.skillLevel = level;
    }

    setRole(role) {
        const roleData = BotAI.ROLES[role];
        if (roleData) {
            Object.assign(this.personality, roleData);
        }
        this.role = role;
    }

    on(event, callback) {
        if (this.listeners[event]) {
            this.listeners[event].push(callback);
        }
    }

    off(event, callback) {
        if (this.listeners[event]) {
            const index = this.listeners[event].indexOf(callback);
            if (index !== -1) {
                this.listeners[event].splice(index, 1);
            }
        }
    }

    toJSON() {
        return {
            id: this.id,
            name: this.name,
            personality: this.personality,
            state: this.state,
            skillLevel: this.skillLevel,
            role: this.role,
            loadout: this.loadout,
            utility: this.utility,
            accuracy: this.accuracy,
            aggression: this.aggression
        };
    }

    static fromJSON(json) {
        const bot = new BotAI(json);
        return bot;
    }
}

BotAI._nextId = 0;

export class BotBlackboard {
    constructor() {
        this.data = new Map();
        this.parent = null;
    }

    set(key, value) {
        this.data.set(key, value);
    }

    get(key) {
        if (this.data.has(key)) {
            return this.data.get(key);
        }
        if (this.parent) {
            return this.parent.get(key);
        }
        return null;
    }

    has(key) {
        return this.data.has(key) || (this.parent && this.parent.has(key));
    }

    delete(key) {
        this.data.delete(key);
    }

    clear() {
        this.data.clear();
    }

    getAll() {
        return Object.fromEntries(this.data);
    }
}

export class BehaviorTree {
    constructor(root, blackboard) {
        this.root = root;
        this.blackboard = blackboard || new BotBlackboard();
        this.lastTick = 0;
    }

    tick(blackboard) {
        this.blackboard = blackboard;
        this.root.execute(this.blackboard);
        this.lastTick = Date.now();
    }
}

export class BehaviorNode {
    constructor(name) {
        this.name = name;
        this.children = [];
    }

    addChild(child) {
        this.children.push(child);
        return this;
    }

    execute(blackboard) {
        return 'success';
    }
}

export class CompositeNode extends BehaviorNode {
    constructor(name, children = []) {
        super(name);
        this.children = children;
    }
}

export class SelectorNode extends CompositeNode {
    constructor(children = []) {
        super('Selector', children);
    }

    execute(blackboard) {
        for (const child of this.children) {
            const result = child.execute(blackboard);
            if (result === 'success') {
                return 'success';
            }
            if (result === 'running') {
                return 'running';
            }
        }
        return 'failure';
    }
}

export class SequenceNode extends CompositeNode {
    constructor(children = []) {
        super('Sequence', children);
    }

    execute(blackboard) {
        for (const child of this.children) {
            const result = child.execute(blackboard);
            if (result === 'failure') {
                return 'failure';
            }
            if (result === 'running') {
                return 'running';
            }
        }
        return 'success';
    }
}

export class ParallelNode extends CompositeNode {
    constructor(children = [], successPolicy = 'all', failurePolicy = 'all') {
        super('Parallel', children);
        this.successPolicy = successPolicy;
        this.failurePolicy = failurePolicy;
    }

    execute(blackboard) {
        let successCount = 0;
        let failureCount = 0;

        for (const child of this.children) {
            const result = child.execute(blackboard);
            if (result === 'success') {
                successCount++;
            } else if (result === 'failure') {
                failureCount++;
            }
        }

        if (this.successPolicy === 'all' && successCount === this.children.length) {
            return 'success';
        }
        if (this.successPolicy === 'one' && successCount >= 1) {
            return 'success';
        }
        if (this.failurePolicy === 'all' && failureCount === this.children.length) {
            return 'failure';
        }
        if (this.failurePolicy === 'one' && failureCount >= 1) {
            return 'failure';
        }

        return 'running';
    }
}

export class DecoratorNode extends BehaviorNode {
    constructor(name, child) {
        super(name);
        this.child = child;
    }

    addChild(child) {
        this.child = child;
        return this;
    }
}

export class InverterNode extends DecoratorNode {
    constructor(child) {
        super('Inverter', child);
    }

    execute(blackboard) {
        const result = this.child.execute(blackboard);
        if (result === 'success') return 'failure';
        if (result === 'failure') return 'success';
        return result;
    }
}

export class ConditionNode extends BehaviorNode {
    constructor(conditionName, conditionFn) {
        super(`Condition: ${conditionName}`);
        this.conditionName = conditionName;
        this.conditionFn = conditionFn;
    }

    execute(blackboard) {
        if (this.conditionFn) {
            return this.conditionFn(blackboard) ? 'success' : 'failure';
        }

        const value = blackboard.get(this.conditionName);
        return value ? 'success' : 'failure';
    }
}

export class ActionNode extends BehaviorNode {
    constructor(actionName, actionFn) {
        super(`Action: ${actionName}`);
        this.actionName = actionName;
        this.actionFn = actionFn;
        this.status = 'ready';
    }

    execute(blackboard) {
        if (this.actionFn) {
            const result = this.actionFn(blackboard);
            if (result === 'running') {
                this.status = 'running';
                return 'running';
            }
            this.status = 'ready';
            return result ? 'success' : 'failure';
        }

        switch (this.actionName) {
            case 'engageTarget':
                return this.engageTarget(blackboard);
            case 'searchLastKnownPosition':
                return this.searchLastKnownPosition(blackboard);
            case 'moveToObjective':
                return this.moveToObjective(blackboard);
            case 'followWaypoints':
                return this.followWaypoints(blackboard);
            case 'idle':
                return this.idle(blackboard);
            default:
                return 'success';
        }
    }

    engageTarget(blackboard) {
        const target = blackboard.get('target');
        if (!target) return 'failure';

        blackboard.set('isAttacking', true);
        return 'success';
    }

    searchLastKnownPosition(blackboard) {
        const position = blackboard.get('targetLastKnownPosition');
        if (!position) return 'failure';

        return 'running';
    }

    moveToObjective(blackboard) {
        return 'running';
    }

    followWaypoints(blackboard) {
        return 'running';
    }

    idle(blackboard) {
        return 'success';
    }
}

export class BotDecisionMaker {
    constructor(bot) {
        this.bot = bot;
        this.decisionHistory = [];
        this.weightFactors = {
            distance: 0.3,
            health: 0.2,
            cover: 0.25,
            ammo: 0.15,
            utility: 0.1
        };
    }

    decideAction(context) {
        const decisions = [];

        if (context.hasVisibleEnemies) {
            decisions.push({
                action: 'engage',
                priority: this.calculateEngagePriority(context),
                target: context.visibleEnemies[0]
            });
        }

        if (context.shouldRetreat) {
            decisions.push({
                action: 'retreat',
                priority: this.calculateRetreatPriority(context),
                position: this.findRetreatPosition(context)
            });
        }

        if (context.shouldUseUtility) {
            decisions.push({
                action: 'useUtility',
                priority: this.calculateUtilityPriority(context),
                type: context.utilityType
            });
        }

        if (context.hasObjective) {
            decisions.push({
                action: 'moveToObjective',
                priority: this.calculateObjectivePriority(context),
                objective: context.objective
            });
        }

        decisions.sort((a, b) => b.priority - a.priority);
        return decisions[0] || { action: 'idle' };
    }

    calculateEngagePriority(context) {
        const target = context.visibleEnemies[0];
        const distanceFactor = 1 - (target.distance / this.bot.visionRange);
        const healthFactor = this.bot.health / 100;
        const ammoFactor = this.bot.currentAmmo / this.bot.maxAmmo;

        return (
            distanceFactor * this.weightFactors.distance +
            healthFactor * this.weightFactors.health +
            ammoFactor * this.weightFactors.ammo +
            this.bot.aggression * 0.2
        );
    }

    calculateRetreatPriority(context) {
        const healthFactor = 1 - (this.bot.health / 100);
        const enemyCount = context.visibleEnemies.length;
        const lowAmmo = this.bot.currentAmmo < this.bot.maxAmmo * 0.2;

        return healthFactor * 0.5 + (enemyCount > 1 ? 0.3 : 0) + (lowAmmo ? 0.2 : 0);
    }

    calculateUtilityPriority(context) {
        const hasEnemies = context.visibleEnemies.length > 0;
        const distance = context.visibleEnemies[0]?.distance || Infinity;

        if (context.utilityType === 'smoke' && distance > 300) {
            return 0.7;
        }
        if (context.utilityType === 'flash' && distance < 400) {
            return 0.8;
        }
        if (context.utilityType === 'molotov') {
            return 0.6;
        }

        return 0;
    }

    calculateObjectivePriority(context) {
        const isTerrorist = this.bot.team === 'terrorists';
        const timeRemaining = context.roundTime;

        if (isTerrorist && context.bombPlanted) {
            return timeRemaining < 30 ? 0.9 : 0.5;
        }

        if (!isTerrorist && context.bombPlanted) {
            return 0.95;
        }

        if (context.hasHostage && !isTerrorist) {
            return 0.8;
        }

        return 0.4;
    }

    findRetreatPosition(context) {
        return this.bot.position.clone();
    }

    recordDecision(decision) {
        this.decisionHistory.push({
            ...decision,
            timestamp: Date.now()
        });

        if (this.decisionHistory.length > 1000) {
            this.decisionHistory.shift();
        }
    }

    learn() {
        for (let i = 1; i < this.decisionHistory.length; i++) {
            const prev = this.decisionHistory[i - 1];
            const curr = this.decisionHistory[i];

            if (prev.outcome === 'success') {
                this.weightFactors.distance *= 1 + this.bot.personality.learningRate * 0.1;
            } else if (prev.outcome === 'failure') {
                this.weightFactors.distance *= 1 - this.bot.personality.learningRate * 0.1;
            }
        }

        const total = Object.values(this.weightFactors).reduce((a, b) => a + b, 0);
        for (const key in this.weightFactors) {
            this.weightFactors[key] /= total;
        }
    }
}

export class BotPathPlanner {
    constructor() {
        this.navMesh = null;
        this.currentPath = [];
        this.pathIndex = 0;
        this.waypoints = [];
        this.lastPathUpdate = 0;
        this.pathUpdateInterval = 1000;
        this.smoothing = true;
        this.waypointSpacing = 50;
    }

    setNavMesh(navMesh) {
        this.navMesh = navMesh;
    }

    findPath(start, end) {
        if (!this.navMesh) {
            return this.createDirectPath(start, end);
        }

        const nodes = this.navMesh.findPath(start, end);
        if (nodes.length === 0) {
            return this.createDirectPath(start, end);
        }

        this.currentPath = this.smoothing ? this.smoothPath(nodes) : nodes;
        this.pathIndex = 0;

        return this.currentPath;
    }

    createDirectPath(start, end) {
        const direction = end.sub(start);
        const distance = direction.length();
        const steps = Math.ceil(distance / this.waypointSpacing);

        const path = [];
        for (let i = 0; i <= steps; i++) {
            const t = i / steps;
            path.push(start.clone().lerp(end, t));
        }

        return path;
    }

    smoothPath(nodes) {
        if (nodes.length < 3) return nodes;

        const smoothed = [nodes[0]];

        for (let i = 1; i < nodes.length - 1; i++) {
            const prev = nodes[i - 1];
            const curr = nodes[i];
            const next = nodes[i + 1];

            const v1 = curr.sub(prev).normalize();
            const v2 = next.sub(curr).normalize();
            const dot = v1.dot(v2);

            if (dot < 0.9) {
                smoothed.push(curr);
            }
        }

        smoothed.push(nodes[nodes.length - 1]);
        return smoothed;
    }

    getNextWaypoint() {
        if (this.pathIndex >= this.currentPath.length) {
            return null;
        }

        return this.currentPath[this.pathIndex];
    }

    advancePath() {
        if (this.pathIndex < this.currentPath.length - 1) {
            this.pathIndex++;
            return true;
        }
        return false;
    }

    hasPath() {
        return this.currentPath.length > 0;
    }

    getRemainingDistance() {
        if (this.pathIndex >= this.currentPath.length) {
            return 0;
        }

        let distance = 0;
        for (let i = this.pathIndex; i < this.currentPath.length - 1; i++) {
            distance += this.currentPath[i].distanceTo(this.currentPath[i + 1]);
        }

        return distance;
    }

    clearPath() {
        this.currentPath = [];
        this.pathIndex = 0;
    }

    addWaypoint(waypoint) {
        this.waypoints.push(waypoint);
    }

    clearWaypoints() {
        this.waypoints = [];
    }

    getWaypoints() {
        return this.waypoints;
    }
}

export class BotCombatAI {
    constructor(bot) {
        this.bot = bot;
        this.aimAdjustment = new Vector3();
        this.lastFireTime = 0;
        this.fireMode = 'automatic';
        this.preferredTargetPart = 'body';
        this.strafeDirection = 1;
        this.strafeChangeTime = 0;
        this.burstCount = 0;
        this.headshotRate = 0.3;
        this.lastHit = null;
        this.hitTracking = [];
    }

    update(dt) {
        if (!this.bot.target) return;

        this.updateAim(dt);
        this.updateFiring(dt);
        this.updateMovement(dt);
        this.updateGrenades(dt);
    }

    updateAim(dt) {
        const target = this.bot.target;
        const targetPosition = target.position.clone();
        targetPosition.y += target.height * 0.7;

        const aimPart = Math.random() < this.headshotRate ? 'head' : 'body';
        if (aimPart === 'head') {
            targetPosition.y += target.height * 0.3;
        }

        const spread = this.calculateSpread();
        this.aimAdjustment.set(
            (Math.random() - 0.5) * spread,
            (Math.random() - 0.5) * spread,
            0
        );

        targetPosition.add(this.aimAdjustment);

        const direction = targetPosition.sub(this.bot.position).normalize();

        if (this.bot.isScoped && this.bot.hasScope()) {
            this.applyScopeSpreadReduction();
        }

        this.bot.aimDirection = direction;
    }

    calculateSpread() {
        const baseSpread = 0.05;
        const movingSpread = this.bot.isMoving ? 0.1 : 0;
        const jumpingSpread = this.bot.isJumping ? 0.2 : 0;
        const crouchedSpread = this.bot.isCrouching ? -0.02 : 0;

        const totalSpread = baseSpread + movingSpread + jumpingSpread + crouchedSpread;
        return totalSpread * (1 - this.bot.accuracy * 0.5);
    }

    applyScopeSpreadReduction() {
        this.aimAdjustment.multiply(0.2);
    }

    updateFiring(dt) {
        if (!this.bot.target) return;

        const target = this.bot.target;
        const distance = this.bot.position.distanceTo(target.position);

        if (distance < this.bot.weapon.range) {
            this.bot.isFiring = true;

            if (this.bot.targetVisible) {
                this.fire(distance);
            }
        } else {
            this.bot.isFiring = false;
        }
    }

    fire(distance) {
        const fireRate = this.bot.weapon.fireRate;

        if (Date.now() - this.lastFireTime < fireRate * 1000) {
            return;
        }

        this.lastFireTime = Date.now();

        if (this.fireMode === 'burst') {
            this.burstCount = 3;
        }

        const hitChance = this.calculateHitChance(distance);

        if (Math.random() < hitChance) {
            this.bot.hitMarker = true;
            this.recordHit(this.bot.target);
        }
    }

    calculateHitChance(distance) {
        let chance = this.bot.accuracy;

        if (distance > 500) {
            chance *= 0.7;
        }
        if (distance > 800) {
            chance *= 0.5;
        }
        if (this.bot.isMoving) {
            chance *= 0.6;
        }
        if (this.bot.isJumping) {
            chance *= 0.3;
        }
        if (this.bot.isScoped) {
            chance *= 1.2;
        }

        return Math.max(0.1, Math.min(0.95, chance));
    }

    recordHit(target) {
        const hit = {
            targetId: target.id,
            position: target.position.clone(),
            time: Date.now()
        };

        this.hitTracking.push(hit);
        this.lastHit = hit;

        if (this.hitTracking.length > 10) {
            this.hitTracking.shift();
        }
    }

    updateMovement(dt) {
        if (!this.bot.target) return;

        const target = this.bot.target;
        const distance = this.bot.position.distanceTo(target.position);

        if (distance > this.bot.preferredDistance) {
            this.bot.movementInput.forward = 1;
            this.bot.movementInput.backward = 0;
        } else if (distance < this.bot.preferredDistance * 0.5) {
            this.bot.movementInput.forward = 0;
            this.bot.movementInput.backward = 1;
        } else {
            this.bot.movementInput.forward = 0;
            this.bot.movementInput.backward = 0;
        }

        if (Date.now() - this.strafeChangeTime > 1000 + Math.random() * 1000) {
            this.strafeDirection *= -1;
            this.strafeChangeTime = Date.now();
        }

        this.bot.movementInput.right = this.strafeDirection * 0.5;

        if (Math.random() < 0.1) {
            this.bot.isCrouching = !this.bot.isCrouching;
        }
    }

    updateGrenades(dt) {
        if (!this.bot.target || this.bot.grenadeUsage < 0.3) return;

        const target = this.bot.target;
        const distance = this.bot.position.distanceTo(target.position);

        if (distance < 200 && this.bot.utility.flashbangs?.length > 0) {
            this.throwFlashbang(target.position);
        }

        if (distance < 400 && this.bot.utility.smokes?.length > 0) {
            this.throwSmoke(target.position);
        }

        if (distance < 300 && this.bot.utility.molotovs?.length > 0) {
            this.throwMolotov(target.position);
        }
    }

    throwFlashbang(targetPosition) {
        console.log(`${this.bot.name} throws flashbang at ${targetPosition}`);
    }

    throwSmoke(targetPosition) {
        console.log(`${this.bot.name} throws smoke at ${targetPosition}`);
    }

    throwMolotov(targetPosition) {
        console.log(`${this.bot.name} throws molotov at ${targetPosition}`);
    }
}

export class BotMovementAI {
    constructor(bot) {
        this.bot = bot;
        this.grounded = true;
        this.jumpCooldown = 0;
        this.lastObstacleCheck = 0;
        this.coverPosition = null;
        this.isTakingCover = false;
    }

    update(dt) {
        this.updateGroundState();
        this.updateMovement(dt);
        this.updateObstacleAvoidance(dt);
        this.updateCover(dt);
    }

    updateGroundState() {
        this.grounded = this.bot.position.y <= this.bot.groundLevel + 0.1;
    }

    updateMovement(dt) {
        if (this.bot.movementInput.forward !== 0) {
            this.bot.isMoving = true;
        } else {
            this.bot.isMoving = false;
        }

        if (this.jumpCooldown > 0) {
            this.jumpCooldown -= dt;
        }

        if (this.shouldJump()) {
            this.bot.jump();
            this.jumpCooldown = 0.5;
        }
    }

    shouldJump() {
        if (!this.grounded || this.jumpCooldown > 0) {
            return false;
        }

        if (this.bot.movementInput.forward > 0 && this.bot.isApproachingEdge()) {
            return true;
        }

        if (this.bot.isStuck()) {
            return true;
        }

        return false;
    }

    updateObstacleAvoidance(dt) {
        if (Date.now() - this.lastObstacleCheck < 100) {
            return;
        }

        this.lastObstacleCheck = Date.now();

        const obstacles = this.detectObstacles();

        for (const obstacle of obstacles) {
            if (obstacle.type === 'wall') {
                this.navigateAroundWall(obstacle);
            } else if (obstacle.type === 'gap') {
                this.navigateOverGap(obstacle);
            }
        }
    }

    detectObstacles() {
        return [];
    }

    navigateAroundWall(wall) {
    }

    navigateOverGap(gap) {
    }

    updateCover(dt) {
        if (this.isTakingCover && !this.isValidCoverPosition(this.coverPosition)) {
            this.leaveCover();
        }
    }

    findCover(fromPosition) {
        const coverSpots = [];

        for (const potentialCover of this.getPotentialCoverSpots()) {
            const coverScore = this.evaluateCoverSpot(potentialCover, fromPosition);
            coverSpots.push({
                position: potentialCover,
                score: coverScore
            });
        }

        coverSpots.sort((a, b) => b.score - a.score);

        return coverSpots[0]?.position || null;
    }

    getPotentialCoverSpots() {
        return [];
    }

    evaluateCoverSpot(spot, enemyPosition) {
        let score = 0;

        const distanceToEnemy = spot.distanceTo(enemyPosition);
        if (distanceToEnemy > 200 && distanceToEnemy < 600) {
            score += 50;
        }

        if (this.isBehindHardCover(spot)) {
            score += 30;
        }

        if (this.hasEscapeRoute(spot)) {
            score += 20;
        }

        return score;
    }

    isBehindHardCover(position) {
        return true;
    }

    hasEscapeRoute(position) {
        return true;
    }

    isValidCoverPosition(position) {
        return true;
    }

    takeCover(position) {
        this.coverPosition = position;
        this.isTakingCover = true;
        this.bot.setState('takingCover');
    }

    leaveCover() {
        this.coverPosition = null;
        this.isTakingCover = false;
        this.bot.setState('combat');
    }
}

export class BotTeamAI {
    constructor(team) {
        this.team = team;
        this.bots = [];
        this.leader = null;
        this.formation = new Formation();
        this.objectives = [];
        this.currentObjective = null;
        this.communicationChannel = new CommunicationChannel();
    }

    addBot(bot) {
        this.bots.push(bot);
        bot.team = this.team;

        if (this.bots.length === 1 || bot.personality.leadership > 0.7) {
            this.leader = bot;
        }

        this.assignRole(bot);
    }

    removeBot(botId) {
        const index = this.bots.findIndex(b => b.id === botId);
        if (index !== -1) {
            this.bots.splice(index, 1);
        }

        if (this.leader?.id === botId) {
            this.leader = this.bots[0] || null;
        }
    }

    assignRole(bot) {
        const aliveBots = this.bots.filter(b => b.isAlive);

        if (aliveBots.length <= 2) {
            bot.setRole('entry');
        } else if (aliveBots.length <= 4) {
            const roles = ['entry', 'support', 'awper', 'lurker'];
            bot.setRole(roles[aliveBots.length % roles.length]);
        } else {
            const roles = ['entry', 'entry', 'support', 'awper', 'lurker', 'leader'];
            bot.setRole(roles[aliveBots.length % roles.length]);
        }
    }

    update(dt) {
        this.updateObjectives();
        this.updateFormation();
        this.communicate();
        this.coordinateAttacks();
    }

    updateObjectives() {
        for (const bot of this.bots) {
            if (!bot.isAlive) continue;

            const objective = this.getBotObjective(bot);
            bot.objective = objective;
        }
    }

    getBotObjective(bot) {
        if (this.team === 'terrorists') {
            if (this.currentObjective?.type === 'plant') {
                return this.currentObjective;
            }
            return { type: 'moveTo', position: this.getBombSite() };
        } else {
            if (this.currentObjective?.type === 'defuse') {
                return this.currentObjective;
            }
            if (this.currentObjective?.type === 'retake') {
                return this.currentObjective;
            }
            return { type: 'guard', position: this.getDefendPosition() };
        }
    }

    getBombSite() {
        return new Vector3(0, 0, 0);
    }

    getDefendPosition() {
        return new Vector3(0, 0, 0);
    }

    updateFormation() {
        if (!this.leader) return;

        for (const bot of this.bots) {
            if (bot === this.leader) continue;
            if (!bot.isAlive) continue;

            const formationPosition = this.formation.getPosition(bot, this.leader);
            bot.waypoints = [formationPosition];
        }
    }

    communicate() {
        for (const bot of this.bots) {
            if (bot.communicationLevel < 0.5) continue;

            if (bot.target && bot.targetVisible) {
                this.communicationChannel.broadcast(bot, 'enemySpotted', {
                    position: bot.target.position,
                    count: bot.blackboard.get('visibleEnemies')?.length || 1
                });
            }

            if (bot.health < 30) {
                this.communicationChannel.broadcast(bot, 'needHealth');
            }

            if (bot.hasDefuser && !this.botHasDefuserOtherThan(bot)) {
                this.communicationChannel.broadcast(bot, 'hasDefuser');
            }
        }
    }

    botHasDefuserOtherThan(bot) {
        return this.bots.some(b => b !== bot && b.hasDefuser && b.isAlive);
    }

    coordinateAttacks() {
        if (!this.leader) return;

        const enemies = this.getEnemyPositions();
        if (enemies.length === 0) return;

        for (const bot of this.bots) {
            if (!bot.isAlive) continue;
            if (bot.role !== 'entry') continue;

            const target = this.findBestEntryTarget(bot, enemies);
            if (target) {
                this.communicationChannel.broadcast(bot, 'entryTarget', { target });
            }
        }
    }

    getEnemyPositions() {
        return [];
    }

    findBestEntryTarget(bot, enemies) {
        let bestTarget = null;
        let bestScore = -Infinity;

        for (const enemy of enemies) {
            let score = 0;

            const distance = bot.position.distanceTo(enemy.position);
            score += (1000 - distance) * 0.1;

            if (enemy.health < 50) {
                score += 30;
            }

            if (!enemy.hasCover) {
                score += 20;
            }

            if (score > bestScore) {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    setObjective(objective) {
        this.currentObjective = objective;

        this.communicationChannel.broadcast(null, 'newObjective', objective);

        for (const bot of this.bots) {
            if (!bot.isAlive) continue;
            bot.objective = objective;
        }
    }
}

export class Formation {
    constructor(type = 'spread') {
        this.type = type;
        this.spacing = 200;
    }

    setType(type) {
        this.type = type;
    }

    getPosition(bot, leader) {
        const offset = this.getOffset(bot);

        if (this.type === 'spread') {
            return leader.position.clone().add(offset);
        } else if (this.type === 'column') {
            return leader.position.clone().add(offset);
        } else if (this.type === 'wedge') {
            return leader.position.clone().add(offset);
        }

        return leader.position.clone();
    }

    getOffset(bot) {
        const index = bot.formationIndex || 0;

        switch (this.type) {
            case 'spread':
                return new Vector3(
                    Math.cos(index * Math.PI / 3) * this.spacing,
                    0,
                    Math.sin(index * Math.PI / 3) * this.spacing
                );
            case 'column':
                return new Vector3(0, 0, index * this.spacing);
            case 'wedge':
                return new Vector3(
                    (index % 2 === 0 ? -1 : 1) * this.spacing * 0.5,
                    0,
                    index * this.spacing * 0.5
                );
            default:
                return new Vector3(0, 0, 0);
        }
    }
}

export class CommunicationChannel {
    constructor() {
        this.messages = [];
        this.maxMessages = 100;
    }

    broadcast(sender, type, data = {}) {
        const message = {
            sender: sender?.id,
            type,
            data,
            timestamp: Date.now()
        };

        this.messages.push(message);

        if (this.messages.length > this.maxMessages) {
            this.messages.shift();
        }
    }

    getMessages(since = 0) {
        return this.messages.filter(m => m.timestamp > since);
    }

    clear() {
        this.messages = [];
    }
}

export class BotFactory {
    static createBot(config = {}) {
        const personalityType = config.personalityType || 'normal';
        const skillLevel = config.skillLevel || 'normal';
        const role = config.role || 'soldier';

        const personality = {
            ...BotAI.PERSONALITY_TYPES[personalityType],
            leadership: config.leadership || Math.random() * 0.5
        };

        const bot = new BotAI({
            name: config.name || `Bot_${Date.now()}`,
            personality,
            skillLevel,
            role,
            team: config.team || 'terrorists',
            position: config.position?.clone()
        });

        bot.position = config.position || new Vector3();
        bot.health = config.health || 100;
        bot.armor = config.armor || 100;
        bot.weapon = config.weapon || { type: 'ak47', ammo: 30 };

        return bot;
    }

    static createTeam(team, count = 5, config = {}) {
        const teamAI = new BotTeamAI(team);

        const names = {
            terrorists: ['Phoenix', 'Anubis', 'Viking', 'Reaper', 'Ghost', 'Shadow', 'Havoc', 'Specter'],
            counterterrorists: ['GSG9', 'SAS', 'GIGN', 'SWAT', 'Alpha', 'Bravo', 'Delta', 'Echo']
        };

        const teamNames = names[team] || names.terrorists;

        for (let i = 0; i < count; i++) {
            const bot = this.createBot({
                name: teamNames[i % teamNames.length],
                team,
                role: this.getRoleForIndex(i, count),
                skillLevel: config.skillLevel || (Math.random() < 0.2 ? 'hard' : 'normal'),
                position: config.startPosition?.clone()
            });

            teamAI.addBot(bot);
        }

        return teamAI;
    }

    static getRoleForIndex(index, total) {
        if (total <= 2) return 'entry';
        if (total <= 4) {
            const roles = ['entry', 'support', 'awper', 'lurker'];
            return roles[index % roles.length];
        }
        if (total <= 6) {
            const roles = ['entry', 'entry', 'support', 'awper', 'lurker', 'leader'];
            return roles[index % roles.length];
        }
        return 'support';
    }
}

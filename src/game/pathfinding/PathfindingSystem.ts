import { IUpdatable } from '../../core/interfaces/IUpdatable';

export enum PathfindingAlgorithm {
    A_STAR = 'a_star',
    DIJKSTRA = 'dijkstra',
    BFS = 'bfs',
    JUMP_POINT_SEARCH = 'jump_point_search',
    FLOW_FIELD = 'flow_field'
}

export enum MovementState {
    IDLE = 'idle',
    MOVING = 'moving',
    PATHING = 'pathing',
    BLOCKED = 'blocked',
    ARRIVED = 'arrived',
    OBSTACLE_AVOIDANCE = 'obstacle_avoidance'
}

export enum PathStatus {
    SUCCESS = 'success',
    FAILURE = 'failure',
    IN_PROGRESS = 'in_progress',
    PARTIAL = 'partial',
    CANCELLED = 'cancelled'
}

export enum HeuristicType {
    MANHATTAN = 'manhattan',
    EUCLIDEAN = 'euclidean',
    OCTILE = 'octile',
    CHEBYSHEV = 'chebyshev',
    ZERO = 'zero'
}

export enum SteeringBehavior {
    SEEK = 'seek',
    FLEE = 'flee',
    ARRIVE = 'arrive',
    WANDER = 'wander',
    PURSUIT = 'pursuit',
    EVADE = 'evade',
    SEPARATION = 'separation',
    COHESION = 'cohesion',
    ALIGNMENT = 'alignment',
    FLOCKING = 'flocking',
    OBSTACLE_AVOIDANCE = 'obstacle_avoidance'
}

export enum FormationType {
    LINE = 'line',
    VEE = 'vee',
    SQUARE = 'square',
    CIRCLE = 'circle',
    WEDGE = 'wedge',
    ECHELON = 'echelon',
    COLUMN = 'column'
}

export enum DestinationType {
    POINT = 'point',
    AREA = 'area',
    UNIT = 'unit',
    BUILDING = 'building'
}

export enum AvoidanceQuality {
    FAST = 'fast',
    BALANCED = 'balanced',
    THOROUGH = 'thorough'
}

export enum GoalTargetType {
    ATTACK = 'attack',
    DEFEND = 'defend',
    EXPLORE = 'explore',
    GATHER = 'gather',
    PATROL = 'patrol',
    ESCAPE = 'escape'
}

export interface Vector3Like {
    x: number;
    y: number;
    z: number;
}

export class Vector3 {
    public x: number;
    public y: number;
    public z: number;

    constructor(x: number = 0, y: number = 0, z: number = 0) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static get ZERO(): Vector3 {
        return new Vector3(0, 0, 0);
    }

    public static get ONE(): Vector3 {
        return new Vector3(1, 1, 1);
    }

    public static get UP(): Vector3 {
        return new Vector3(0, 1, 0);
    }

    public static get DOWN(): Vector3 {
        return new Vector3(0, -1, 0);
    }

    public static get LEFT(): Vector3 {
        return new Vector3(-1, 0, 0);
    }

    public static get RIGHT(): Vector3 {
        return new Vector3(1, 0, 0);
    }

    public static get FORWARD(): Vector3 {
        return new Vector3(0, 0, 1);
    }

    public static get BACKWARD(): Vector3 {
        return new Vector3(0, 0, -1);
    }

    public static fromArray(arr: number[]): Vector3 {
        return new Vector3(arr[0] || 0, arr[1] || 0, arr[2] || 0);
    }

    public static fromObject(obj: Vector3Like): Vector3 {
        return new Vector3(obj.x, obj.y, obj.z);
    }

    public static distance(a: Vector3, b: Vector3): number {
        const dx = b.x - a.x;
        const dy = b.y - a.y;
        const dz = b.z - a.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static distanceSquared(a: Vector3, b: Vector3): number {
        const dx = b.x - a.x;
        const dy = b.y - a.y;
        const dz = b.z - a.z;
        return dx * dx + dy * dy + dz * dz;
    }

    public static manhattanDistance(a: Vector3, b: Vector3): number {
        return Math.abs(b.x - a.x) + Math.abs(b.y - a.y) + Math.abs(b.z - a.z);
    }

    public static dot(a: Vector3, b: Vector3): number {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static cross(a: Vector3, b: Vector3): Vector3 {
        return new Vector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }

    public static lerp(a: Vector3, b: Vector3, t: number): Vector3 {
        return new Vector3(
            a.x + (b.x - a.x) * t,
            a.y + (b.y - a.y) * t,
            a.z + (b.z - a.z) * t
        );
    }

    public static angle(a: Vector3, b: Vector3): number {
        const denom = this.magnitude(a) * this.magnitude(b);
        if (denom < 0.000001) return 0;
        const cosAngle = Math.max(-1, Math.min(1, this.dot(a, b) / denom));
        return Math.acos(cosAngle);
    }

    public static signedAngle(a: Vector3, b: Vector3, normal: Vector3): number {
        const angle = this.angle(a, b);
        const sign = this.dot(this.cross(a, b), normal) < 0 ? -1 : 1;
        return angle * sign;
    }

    public static reflect(vector: Vector3, normal: Vector3): Vector3 {
        const dotProduct = this.dot(vector, normal) * 2;
        return new Vector3(
            vector.x - normal.x * dotProduct,
            vector.y - normal.y * dotProduct,
            vector.z - normal.z * dotProduct
        );
    }

    public static project(vector: Vector3, normal: Vector3): Vector3 {
        const dotProduct = this.dot(vector, normal);
        const magSquared = this.magnitudeSquared(normal);
        if (magSquared < 0.000001) return this.ZERO;
        const scalar = dotProduct / magSquared;
        return new Vector3(
            normal.x * scalar,
            normal.y * scalar,
            normal.z * scalar
        );
    }

    public static projectOnPlane(vector: Vector3, planeNormal: Vector3): Vector3 {
        const dotProduct = this.dot(vector, planeNormal);
        const magSquared = this.magnitudeSquared(planeNormal);
        if (magSquared < 0.000001) return vector.clone();
        const scalar = dotProduct / magSquared;
        return new Vector3(
            vector.x - planeNormal.x * scalar,
            vector.y - planeNormal.y * scalar,
            vector.z - planeNormal.z * scalar
        );
    }

    public static magnitude(v: Vector3): number {
        return Math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }

    public static magnitudeSquared(v: Vector3): number {
        return v.x * v.x + v.y * v.y + v.z * v.z;
    }

    public static normalize(v: Vector3): Vector3 {
        const mag = this.magnitude(v);
        if (mag < 0.000001) return this.ZERO;
        return new Vector3(v.x / mag, v.y / mag, v.z / mag);
    }

    public static isZero(v: Vector3): boolean {
        return Math.abs(v.x) < 0.000001 && Math.abs(v.y) < 0.000001 && Math.abs(v.z) < 0.000001;
    }

    public static add(a: Vector3, b: Vector3): Vector3 {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static subtract(a: Vector3, b: Vector3): Vector3 {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static multiply(v: Vector3, scalar: number): Vector3 {
        return new Vector3(v.x * scalar, v.y * scalar, v.z * scalar);
    }

    public static divide(v: Vector3, scalar: number): Vector3 {
        if (Math.abs(scalar) < 0.000001) return this.ZERO;
        return new Vector3(v.x / scalar, v.y / scalar, v.z / scalar);
    }

    public static min(a: Vector3, b: Vector3): Vector3 {
        return new Vector3(Math.min(a.x, b.x), Math.min(a.y, b.y), Math.min(a.z, b.z));
    }

    public static max(a: Vector3, b: Vector3): Vector3 {
        return new Vector3(Math.max(a.x, b.x), Math.max(a.y, b.y), Math.max(a.z, b.z));
    }

    public static clamp(value: Vector3, min: Vector3, max: Vector3): Vector3 {
        return new Vector3(
            Math.max(min.x, Math.min(max.x, value.x)),
            Math.max(min.y, Math.min(max.y, value.y)),
            Math.max(min.z, Math.min(max.z, value.z))
        );
    }

    public static moveTowards(current: Vector3, target: Vector3, maxDistanceDelta: number): Vector3 {
        const toTarget = this.subtract(target, current);
        const distance = this.magnitude(toTarget);
        if (distance <= maxDistanceDelta || distance < 0.000001) {
            return target.clone();
        }
        return this.add(current, this.multiply(this.normalize(toTarget), maxDistanceDelta));
    }

    public static rotateTowards(current: Vector3, target: Vector3, maxRadians: number, maxMagnitude: number): Vector3 {
        const currentAngle = this.angle(current, target);
        const maxAngle = Math.min(maxRadians, currentAngle);
        const angle = currentAngle > 0.000001 ? maxAngle / currentAngle : 0;
        const direction = this.normalize(this.subtract(target, current));
        const sinAngle = Math.sin(angle);
        const cosAngle = Math.cos(angle);
        const x = current.x * cosAngle - current.z * sinAngle;
        const z = current.x * sinAngle + current.z * cosAngle;
        const result = new Vector3(x, 0, z);
        return this.multiply(this.normalize(result), Math.min(this.magnitude(current) + maxMagnitude * angle, this.magnitude(target)));
    }

    public static smoothDamp(current: Vector3, target: Vector3, velocity: Vector3, smoothTime: number, deltaTime: number, maxSpeed: number = Infinity): { position: Vector3; velocity: Vector3 } {
        const omega = 2 / smoothTime;
        const x = omega * deltaTime;
        const exp = 1 / (1 + x + 0.48 * x * x + 0.235 * x * x * x);
        let change = current.clone().subtract(target);
        const maxChange = maxSpeed * smoothTime;
        const maxChangeSquared = maxChange * maxChange;
        const sqDist = change.magnitudeSquared();
        if (sqDist > maxChangeSquared) {
            change = change.normalize().multiply(maxChange);
        }
        const temp = velocity.add(change.clone().multiply(omega)).multiply(deltaTime);
        const newPos = target.add(change.add(temp).multiply(-exp));
        const newVel = current.subtract(target).subtract(change).multiply(omega).add(temp).multiply(exp);
        return { position: newPos, velocity: newVel };
    }

    public clone(): Vector3 {
        return new Vector3(this.x, this.y, this.z);
    }

    public copy(v: Vector3): this {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
        return this;
    }

    public set(x: number, y: number, z: number): this {
        this.x = x;
        this.y = y;
        this.z = z;
        return this;
    }

    public add(v: Vector3): this {
        this.x += v.x;
        this.y += v.y;
        this.z += v.z;
        return this;
    }

    public subtract(v: Vector3): this {
        this.x -= v.x;
        this.y -= v.y;
        this.z -= v.z;
        return this;
    }

    public multiplyScalar(scalar: number): this {
        this.x *= scalar;
        this.y *= scalar;
        this.z *= scalar;
        return this;
    }

    public divideScalar(scalar: number): this {
        if (Math.abs(scalar) > 0.000001) {
            this.x /= scalar;
            this.y /= scalar;
            this.z /= scalar;
        }
        return this;
    }

    public magnitude(): number {
        return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
    }

    public magnitudeSquared(): number {
        return this.x * this.x + this.y * this.y + this.z * this.z;
    }

    public normalize(): this {
        const mag = this.magnitude();
        if (mag > 0.000001) {
            this.divideScalar(mag);
        }
        return this;
    }

    public dot(v: Vector3): number {
        return this.x * v.x + this.y * v.y + this.z * v.z;
    }

    public cross(v: Vector3): Vector3 {
        return new Vector3(
            this.y * v.z - this.z * v.y,
            this.z * v.x - this.x * v.z,
            this.x * v.y - this.y * v.x
        );
    }

    public distanceTo(v: Vector3): number {
        return Vector3.distance(this, v);
    }

    public distanceToSquared(v: Vector3): number {
        return Vector3.distanceSquared(this, v);
    }

    public manhattanDistanceTo(v: Vector3): number {
        return Math.abs(v.x - this.x) + Math.abs(v.y - this.y) + Math.abs(v.z - this.z);
    }

    public lerp(v: Vector3, t: number): this {
        this.x += (v.x - this.x) * t;
        this.y += (v.y - this.y) * t;
        this.z += (v.z - this.z) * t;
        return this;
    }

    public rotateY(angle: number): this {
        const cos = Math.cos(angle);
        const sin = Math.sin(angle);
        const x = this.x * cos - this.z * sin;
        const z = this.x * sin + this.z * cos;
        this.x = x;
        this.z = z;
        return this;
    }

    public reflect(normal: Vector3): this {
        const dotProduct = this.dot(normal) * 2;
        this.x -= normal.x * dotProduct;
        this.y -= normal.y * dotProduct;
        this.z -= normal.z * dotProduct;
        return this;
    }

    public projectOnVector(v: Vector3): this {
        const dotProduct = this.dot(v);
        const magSquared = v.magnitudeSquared();
        if (magSquared < 0.000001) {
            this.set(0, 0, 0);
            return this;
        }
        const scalar = dotProduct / magSquared;
        return this.copy(v.clone().multiplyScalar(scalar));
    }

    public projectOnPlane(planeNormal: Vector3): this {
        const dotProduct = this.dot(planeNormal);
        const magSquared = planeNormal.magnitudeSquared();
        if (magSquared < 0.000001) return this;
        const scalar = dotProduct / magSquared;
        this.x -= planeNormal.x * scalar;
        this.y -= planeNormal.y * scalar;
        this.z -= planeNormal.z * scalar;
        return this;
    }

    public isZero(): boolean {
        return Math.abs(this.x) < 0.000001 && Math.abs(this.y) < 0.000001 && Math.abs(this.z) < 0.000001;
    }

    public equals(v: Vector3, epsilon: number = 0.000001): boolean {
        return Math.abs(this.x - v.x) < epsilon && Math.abs(this.y - v.y) < epsilon && Math.abs(this.z - v.z) < epsilon;
    }

    public notEquals(v: Vector3, epsilon: number = 0.000001): boolean {
        return !this.equals(v, epsilon);
    }

    public toArray(): number[] {
        return [this.x, this.y, this.z];
    }

    public toObject(): Vector3Like {
        return { x: this.x, y: this.y, z: this.z };
    }

    public toString(): string {
        return `(${this.x.toFixed(2)}, ${this.y.toFixed(2)}, ${this.z.toFixed(2)})`;
    }

    public floor(): this {
        this.x = Math.floor(this.x);
        this.y = Math.floor(this.y);
        this.z = Math.floor(this.z);
        return this;
    }

    public ceil(): this {
        this.x = Math.ceil(this.x);
        this.y = Math.ceil(this.y);
        this.z = Math.ceil(this.z);
        return this;
    }

    public round(): this {
        this.x = Math.round(this.x);
        this.y = Math.round(this.y);
        this.z = Math.round(this.z);
        return this;
    }

    public abs(): this {
        this.x = Math.abs(this.x);
        this.y = Math.abs(this.y);
        this.z = Math.abs(this.z);
        return this;
    }

    public negate(): this {
        this.x = -this.x;
        this.y = -this.y;
        this.z = -this.z;
        return this;
    }
}

export interface PathNode {
    position: Vector3;
    gridX: number;
    gridZ: number;
    g: number;
    h: number;
    f: number;
    walkable: boolean;
    cost: number;
    parent: PathNode | null;
    inOpenSet: boolean;
    inClosedSet: boolean;
    forced: boolean;
    jumpPoint: boolean;
}

export interface PathFinder {
    findPath(start: Vector3, end: Vector3, request?: PathRequest): PathResult;
    setGrid(grid: boolean[][]): void;
    setTerrainCosts(costs: number[][]): void;
    updateObstacles(obstacles: Vector3[]): void;
    getAlgorithm(): PathfindingAlgorithm;
}

export interface MovementTarget {
    position: Vector3;
    reached: boolean;
    targetUnit?: any;
    targetBuilding?: any;
}

export interface PathRequest {
    id: string;
    start: Vector3;
    end: Vector3;
    unitId: string;
    algorithm: PathfindingAlgorithm;
    allowPartialPath: boolean;
    priority: number;
    callback: (result: PathResult) => void;
    timeout?: number;
    maxIterations?: number;
    heuristicType?: HeuristicType;
    constraints?: PathConstraints;
}

export interface PathConstraints {
    minTurnRadius?: number;
    maxSlope?: number;
    canSwim?: boolean;
    canFly?: boolean;
    requiredCapabilities?: string[];
    avoidAreas?: Vector3[][];
    preferRoads?: boolean;
}

export interface PathResult {
    requestId: string;
    status: PathStatus;
    path: Vector3[];
    smoothedPath?: Vector3[];
    cost: number;
    length: number;
    iterations: number;
    timeMs: number;
    partial?: boolean;
    remainingDistance?: number;
    message?: string;
}

export interface UnitMovementData {
    unitId: string;
    position: Vector3;
    velocity: Vector3;
    targetPosition: Vector3;
    currentPath: Vector3[];
    currentWaypointIndex: number;
    state: MovementState;
    speed: number;
    maxSpeed: number;
    acceleration: number;
    steeringBehaviors: SteeringBehavior[];
    formationPosition?: Vector3;
    pathfindingRequestId?: string;
    lastPathUpdate: number;
    targetLock?: string;
}

export interface DynamicObstacle {
    id: string;
    position: Vector3;
    radius: number;
    velocity: Vector3;
    predictedPosition: Vector3;
    priority: number;
    updateInterval: number;
    lastUpdate: number;
}

export interface PathfindingStats {
    totalRequests: number;
    successfulRequests: number;
    failedRequests: number;
    averageIterations: number;
    averageTimeMs: number;
    cacheHitRate: number;
    memoryUsageBytes: number;
    activeUnits: number;
    activePathfinders: number;
}

export class PathNodePool {
    private pool: PathNode[];
    private activeNodes: Set<PathNode>;
    private gridSize: Vector3;
    private worldSize: Vector3;

    constructor(gridSize: Vector3, worldSize: Vector3, initialCapacity: number = 1000) {
        this.pool = [];
        this.activeNodes = new Set();
        this.gridSize = gridSize;
        this.worldSize = worldSize;
        for (let i = 0; i < initialCapacity; i++) {
            this.pool.push(this.createNode());
        }
    }

    private createNode(): PathNode {
        return {
            position: new Vector3(),
            gridX: 0,
            gridZ: 0,
            g: Infinity,
            h: 0,
            f: Infinity,
            walkable: true,
            cost: 1,
            parent: null,
            inOpenSet: false,
            inClosedSet: false,
            forced: false,
            jumpPoint: false
        };
    }

    public acquire(): PathNode {
        let node: PathNode;
        if (this.pool.length > 0) {
            node = this.pool.pop()!;
        } else {
            node = this.createNode();
        }
        node.g = Infinity;
        node.h = 0;
        node.f = Infinity;
        node.parent = null;
        node.inOpenSet = false;
        node.inClosedSet = false;
        node.forced = false;
        node.jumpPoint = false;
        node.walkable = true;
        node.cost = 1;
        this.activeNodes.add(node);
        return node;
    }

    public release(node: PathNode): void {
        if (this.activeNodes.has(node)) {
            this.activeNodes.delete(node);
            this.pool.push(node);
        }
    }

    public releaseAll(): void {
        for (const node of this.activeNodes) {
            this.pool.push(node);
        }
        this.activeNodes.clear();
    }

    public getNode(gridX: number, gridZ: number): PathNode | null {
        for (const node of this.activeNodes) {
            if (node.gridX === gridX && node.gridZ === gridZ) {
                return node;
            }
        }
        return null;
    }

    public getActiveNodeCount(): number {
        return this.activeNodes.size;
    }

    public getPooledNodeCount(): number {
        return this.pool.length;
    }
}

export class MovementCost {
    private defaultCosts: Map<string, number>;
    private terrainCosts: Map<number, number>;
    private heightPenalties: Map<number, number>;
    private slopePenalties: Map<number, number>;
    private customModifiers: Map<string, (baseCost: number, position: Vector3) => number>;

    constructor() {
        this.defaultCosts = new Map();
        this.terrainCosts = new Map();
        this.heightPenalties = new Map();
        this.slopePenalties = new Map();
        this.customModifiers = new Map();

        this.initializeDefaults();
    }

    private initializeDefaults(): void {
        this.defaultCosts.set('grass', 1.0);
        this.defaultCosts.set('dirt', 1.2);
        this.defaultCosts.set('road', 0.5);
        this.defaultCosts.set('water', 10.0);
        this.defaultCosts.set('forest', 2.0);
        this.defaultCosts.set('mountain', 5.0);
        this.defaultCosts.set('building', Infinity);
        this.defaultCosts.set('obstacle', Infinity);
        this.defaultCosts.set('swamp', 3.0);
        this.defaultCosts.set('sand', 1.5);
    }

    public setDefaultCost(terrainType: string, cost: number): void {
        this.defaultCosts.set(terrainType, cost);
    }

    public setTerrainCost(terrainId: number, cost: number): void {
        this.terrainCosts.set(terrainId, cost);
    }

    public setHeightPenalty(heightDifference: number, penalty: number): void {
        this.heightPenalties.set(heightDifference, penalty);
    }

    public setSlopePenalty(slopeAngle: number, penalty: number): void {
        this.slopePenalties.set(slopeAngle, penalty);
    }

    public addCustomModifier(name: string, modifier: (baseCost: number, position: Vector3) => number): void {
        this.customModifiers.set(name, modifier);
    }

    public removeCustomModifier(name: string): void {
        this.customModifiers.delete(name);
    }

    public getCost(terrainType: string, position?: Vector3, fromPosition?: Vector3): number {
        let cost = this.defaultCosts.get(terrainType) || 1.0;

        if (position && fromPosition) {
            const heightDiff = Math.abs(position.y - fromPosition.y);
            cost *= this.getHeightPenalty(heightDiff);
            cost *= this.getSlopePenaltyFromDirection(position, fromPosition);
        }

        for (const modifier of this.customModifiers.values()) {
            cost = modifier(cost, position || Vector3.ZERO);
        }

        return cost;
    }

    public getTerrainCost(terrainId: number): number {
        return this.terrainCosts.get(terrainId) || 1.0;
    }

    private getHeightPenalty(heightDiff: number): number {
        let penalty = 1.0;
        for (const [threshold, pen] of this.heightPenalties.entries()) {
            if (heightDiff >= threshold) {
                penalty = Math.max(penalty, pen);
            }
        }
        return penalty;
    }

    private getSlopePenaltyFromDirection(to: Vector3, from: Vector3): number {
        const dx = to.x - from.x;
        const dz = to.z - from.z;
        const distance = Math.sqrt(dx * dx + dz * dz);
        if (distance < 0.001) return 1.0;

        const slopeAngle = Math.atan2(to.y - from.y, distance);
        const slopeDegrees = slopeAngle * (180 / Math.PI);

        let penalty = 1.0;
        for (const [angle, pen] of this.slopePenalties.entries()) {
            if (slopeDegrees >= angle) {
                penalty = Math.max(penalty, pen);
            }
        }
        return penalty;
    }

    public calculatePathCost(path: Vector3[], terrainTypes: string[]): number {
        let totalCost = 0;
        for (let i = 0; i < path.length - 1; i++) {
            const from = path[i];
            const to = path[i + 1];
            const terrainType = terrainTypes[i] || 'grass';
            totalCost += this.getCost(terrainType, to, from);
        }
        return totalCost;
    }

    public getMovementMultiplier(speed: number, cost: number): number {
        return speed / cost;
    }

    public isPassable(terrainType: string): boolean {
        const cost = this.defaultCosts.get(terrainType);
        return cost !== undefined && cost < Infinity;
    }

    public reset(): void {
        this.terrainCosts.clear();
        this.heightPenalties.clear();
        this.slopePenalties.clear();
        this.customModifiers.clear();
        this.initializeDefaults();
    }
}

export class PathCache {
    private cache: Map<string, PathResult>;
    private accessOrder: string[];
    private maxSize: number;
    private maxMemoryBytes: number;
    private currentMemoryUsage: number;
    private hitCount: number;
    private missCount: number;
    private cacheKeyGenerator: (start: Vector3, end: Vector3, gridSize: number) => string;

    constructor(maxSize: number = 1000, maxMemoryBytes: number = 10 * 1024 * 1024) {
        this.cache = new Map();
        this.accessOrder = [];
        this.maxSize = maxSize;
        this.maxMemoryBytes = maxMemoryBytes;
        this.currentMemoryUsage = 0;
        this.hitCount = 0;
        this.missCount = 0;
        this.cacheKeyGenerator = this.defaultKeyGenerator;
    }

    public setKeyGenerator(generator: (start: Vector3, end: Vector3, gridSize: number) => string): void {
        this.cacheKeyGenerator = generator;
    }

    private defaultKeyGenerator(start: Vector3, end: Vector3, gridSize: number): string {
        const gridX1 = Math.floor(start.x / gridSize);
        const gridZ1 = Math.floor(start.z / gridSize);
        const gridX2 = Math.floor(end.x / gridSize);
        const gridZ2 = Math.floor(end.z / gridSize);
        return `${gridX1},${gridZ1}-${gridX2},${gridZ2}`;
    }

    public get(start: Vector3, end: Vector3, gridSize: number): PathResult | null {
        const key = this.cacheKeyGenerator(start, end, gridSize);
        const cached = this.cache.get(key);
        if (cached) {
            this.hitCount++;
            this.updateAccessOrder(key);
            return cached;
        }
        this.missCount++;
        return null;
    }

    public set(start: Vector3, end: Vector3, gridSize: number, result: PathResult): void {
        const key = this.cacheKeyGenerator(start, end, gridSize);
        const size = this.estimateSize(result);

        while (this.cache.size >= this.maxSize || this.currentMemoryUsage + size > this.maxMemoryBytes) {
            this.evictOldest();
        }

        if (this.cache.has(key)) {
            this.currentMemoryUsage -= this.estimateSize(this.cache.get(key)!);
        }

        this.cache.set(key, result);
        this.accessOrder.push(key);
        this.currentMemoryUsage += size;
    }

    private estimateSize(result: PathResult): number {
        const pathSize = result.path.length * 24;
        const smoothedPathSize = result.smoothedPath ? result.smoothedPath.length * 24 : 0;
        return pathSize + smoothedPathSize + 200;
    }

    private evictOldest(): void {
        if (this.accessOrder.length === 0) return;
        const oldest = this.accessOrder.shift()!;
        const removed = this.cache.get(oldest);
        if (removed) {
            this.currentMemoryUsage -= this.estimateSize(removed);
            this.cache.delete(oldest);
        }
    }

    private updateAccessOrder(key: string): void {
        const index = this.accessOrder.indexOf(key);
        if (index > -1) {
            this.accessOrder.splice(index, 1);
        }
        this.accessOrder.push(key);
    }

    public invalidate(start: Vector3, radius: number, gridSize: number): void {
        const gridRadius = Math.ceil(radius / gridSize);
        const centerX = Math.floor(start.x / gridSize);
        const centerZ = Math.floor(start.z / gridSize);
        const keysToRemove: string[] = [];

        for (const key of this.cache.keys()) {
            const [startPart, endPart] = key.split('-');
            const [x1, z1] = startPart.split(',').map(Number);
            const [x2, z2] = endPart.split(',').map(Number);

            const nearStart = Math.abs(x1 - centerX) <= gridRadius && Math.abs(z1 - centerZ) <= gridRadius;
            const nearEnd = Math.abs(x2 - centerX) <= gridRadius && Math.abs(z2 - centerZ) <= gridRadius;

            if (nearStart || nearEnd) {
                keysToRemove.push(key);
            }
        }

        for (const key of keysToRemove) {
            const removed = this.cache.get(key);
            if (removed) {
                this.currentMemoryUsage -= this.estimateSize(removed);
            }
            this.cache.delete(key);
            const index = this.accessOrder.indexOf(key);
            if (index > -1) {
                this.accessOrder.splice(index, 1);
            }
        }
    }

    public clear(): void {
        this.cache.clear();
        this.accessOrder = [];
        this.currentMemoryUsage = 0;
    }

    public getHitRate(): number {
        const total = this.hitCount + this.missCount;
        return total > 0 ? this.hitCount / total : 0;
    }

    public getStats(): { size: number; maxSize: number; memoryUsage: number; maxMemory: number; hitRate: number } {
        return {
            size: this.cache.size,
            maxSize: this.maxSize,
            memoryUsage: this.currentMemoryUsage,
            maxMemory: this.maxMemoryBytes,
            hitRate: this.getHitRate()
        };
    }

    public resize(maxSize: number, maxMemoryBytes: number): void {
        this.maxSize = maxSize;
        this.maxMemoryBytes = maxMemoryBytes;

        while (this.cache.size > this.maxSize) {
            this.evictOldest();
        }

        while (this.currentMemoryUsage > this.maxMemoryBytes) {
            this.evictOldest();
        }
    }
}

export class PathSmoother {
    private minTurnAngle: number;
    private simplificationTolerance: number;
    private cornerSmoothingFactor: number;

    constructor(
        minTurnAngle: number = 30,
        simplificationTolerance: number = 0.5,
        cornerSmoothingFactor: number = 0.3
    ) {
        this.minTurnAngle = minTurnAngle * (Math.PI / 180);
        this.simplificationTolerance = simplificationTolerance;
        this.cornerSmoothingFactor = cornerSmoothingFactor;
    }

    public smooth(path: Vector3[], obstacles?: Vector3[]): Vector3[] {
        if (path.length < 3) return path;

        let smoothed = this.douglasPeuckerSimplify([...path], this.simplificationTolerance);
        smoothed = this.removeUnnecessaryWaypoints(smoothed);
        smoothed = this.smoothCorners(smoothed);

        if (obstacles && obstacles.length > 0) {
            smoothed = this.avoidObstacles(smoothed, obstacles);
        }

        return smoothed;
    }

    private douglasPeuckerSimplify(points: Vector3[], tolerance: number): Vector3[] {
        if (points.length < 3) return points;

        let maxDistance = 0;
        let maxIndex = 0;
        const first = points[0];
        const last = points[points.length - 1];

        for (let i = 1; i < points.length - 1; i++) {
            const distance = this.perpendicularDistance(points[i], first, last);
            if (distance > maxDistance) {
                maxDistance = distance;
                maxIndex = i;
            }
        }

        if (maxDistance > tolerance) {
            const left = this.douglasPeuckerSimplify(points.slice(0, maxIndex + 1), tolerance);
            const right = this.douglasPeuckerSimplify(points.slice(maxIndex), tolerance);
            return [...left.slice(0, -1), ...right];
        }

        return [first, last];
    }

    private perpendicularDistance(point: Vector3, lineStart: Vector3, lineEnd: Vector3): number {
        const dx = lineEnd.x - lineStart.x;
        const dz = lineEnd.z - lineStart.z;
        const lengthSquared = dx * dx + dz * dz;

        if (lengthSquared < 0.000001) {
            return point.distanceTo(lineStart);
        }

        const t = Math.max(0, Math.min(1,
            ((point.x - lineStart.x) * dx + (point.z - lineStart.z) * dz) / lengthSquared
        ));

        const projection = new Vector3(
            lineStart.x + t * dx,
            0,
            lineStart.z + t * dz
        );

        return point.distanceTo(projection);
    }

    private removeUnnecessaryWaypoints(path: Vector3[]): Vector3[] {
        if (path.length < 3) return path;

        const result: Vector3[] = [path[0]];

        for (let i = 1; i < path.length - 1; i++) {
            const prev = result[result.length - 1];
            const current = path[i];
            const next = path[i + 1];

            const prevDir = Vector3.subtract(current, prev).normalize();
            const nextDir = Vector3.subtract(next, current).normalize();

            const dotProduct = Vector3.dot(prevDir, nextDir);
            const angle = Math.acos(Math.max(-1, Math.min(1, dotProduct)));

            if (angle > this.minTurnAngle) {
                result.push(current);
            }
        }

        result.push(path[path.length - 1]);
        return result;
    }

    private smoothCorners(path: Vector3[]): Vector3[] {
        if (path.length < 3) return path;

        const result: Vector3[] = [path[0]];

        for (let i = 1; i < path.length - 1; i++) {
            const prev = path[i - 1];
            const current = path[i];
            const next = path[i + 1];

            const toPrev = Vector3.subtract(prev, current);
            const toNext = Vector3.subtract(next, current);

            const distance = Math.min(toPrev.magnitude(), toNext.magnitude()) * this.cornerSmoothingFactor;

            if (distance > 0.1) {
                const smoothPrev = Vector3.add(current, toPrev.normalize().multiplyScalar(distance));
                const smoothNext = Vector3.add(current, toNext.normalize().multiplyScalar(distance));

                result.push(smoothPrev);
                result.push(current);
                result.push(smoothNext);
            } else {
                result.push(current);
            }
        }

        result.push(path[path.length - 1]);
        return result;
    }

    private avoidObstacles(path: Vector3[], obstacles: Vector3[]): Vector3[] {
        if (path.length < 2 || obstacles.length === 0) return path;

        const result: Vector3[] = [];
        for (let i = 0; i < path.length; i++) {
            result.push(path[i]);

            if (i < path.length - 1) {
                const segmentStart = path[i];
                const segmentEnd = path[i + 1];
                const closestObstacle = this.findClosestObstacle(segmentStart, segmentEnd, obstacles);

                if (closestObstacle) {
                    const detourPoints = this.createDetour(segmentStart, segmentEnd, closestObstacle);
                    result.push(...detourPoints);
                }
            }
        }

        return result;
    }

    private findClosestObstacle(start: Vector3, end: Vector3, obstacles: Vector3[]): Vector3 | null {
        let closest: Vector3 | null = null;
        let minDist = Infinity;

        for (const obstacle of obstacles) {
            const dist = this.pointToSegmentDistance(obstacle, start, end);
            if (dist < minDist) {
                minDist = dist;
                closest = obstacle;
            }
        }

        return closest;
    }

    private pointToSegmentDistance(point: Vector3, lineStart: Vector3, lineEnd: Vector3): number {
        const dx = lineEnd.x - lineStart.x;
        const dz = lineEnd.z - lineStart.z;
        const lengthSquared = dx * dx + dz * dz;

        if (lengthSquared < 0.000001) {
            return point.distanceTo(lineStart);
        }

        const t = Math.max(0, Math.min(1,
            ((point.x - lineStart.x) * dx + (point.z - lineStart.z) * dz) / lengthSquared
        ));

        const closest = new Vector3(
            lineStart.x + t * dx,
            0,
            lineStart.z + t * dz
        );

        return point.distanceTo(closest);
    }

    private createDetour(start: Vector3, end: Vector3, obstacle: Vector3): Vector3[] {
        const midPoint = Vector3.lerp(start, end, 0.5);
        const direction = Vector3.subtract(end, start).normalize();
        const perpendicular = new Vector3(-direction.z, 0, direction.x);

        const offset = obstacle.y + 1;
        const detourLeft = Vector3.add(midPoint, perpendicular.clone().multiplyScalar(offset));
        const detourRight = Vector3.subtract(midPoint, perpendicular.clone().multiplyScalar(offset));

        return [detourLeft, detourRight];
    }

    public setMinTurnAngle(angleDegrees: number): void {
        this.minTurnAngle = angleDegrees * (Math.PI / 180);
    }

    public setSimplificationTolerance(tolerance: number): void {
        this.simplificationTolerance = tolerance;
    }

    public setCornerSmoothingFactor(factor: number): void {
        this.cornerSmoothingFactor = Math.max(0, Math.min(1, factor));
    }
}

export class Heuristic {
    public static manhattan(a: Vector3, b: Vector3): number {
        return Math.abs(a.x - b.x) + Math.abs(a.z - b.z);
    }

    public static euclidean(a: Vector3, b: Vector3): number {
        const dx = a.x - b.x;
        const dz = a.z - b.z;
        return Math.sqrt(dx * dx + dz * dz);
    }

    public static octile(a: Vector3, b: Vector3): number {
        const dx = Math.abs(a.x - b.x);
        const dz = Math.abs(a.z - b.z);
        const D = 1;
        const D2 = Math.SQRT2;
        return D * (dx + dz) + (D2 - 2 * D) * Math.min(dx, dz);
    }

    public static chebyshev(a: Vector3, b: Vector3): number {
        const dx = Math.abs(a.x - b.x);
        const dz = Math.abs(a.z - b.z);
        return Math.max(dx, dz);
    }

    public static zero(a: Vector3, b: Vector3): number {
        return 0;
    }

    public static diagonal(a: Vector3, b: Vector3, D: number = 1, D2: number = Math.SQRT2): number {
        const dx = Math.abs(a.x - b.x);
        const dz = Math.abs(a.z - b.z);
        return D * (dx + dz) + (D2 - 2 * D) * Math.min(dx, dz);
    }

    public static calculate(type: HeuristicType, a: Vector3, b: Vector3): number {
        switch (type) {
            case HeuristicType.MANHATTAN:
                return this.manhattan(a, b);
            case HeuristicType.EUCLIDEAN:
                return this.euclidean(a, b);
            case HeuristicType.OCTILE:
                return this.octile(a, b);
            case HeuristicType.CHEBYSHEV:
                return this.chebyshev(a, b);
            case HeuristicType.ZERO:
                return this.zero(a, b);
            default:
                return this.euclidean(a, b);
        }
    }

    public static getWeight(heuristicType: HeuristicType): number {
        switch (heuristicType) {
            case HeuristicType.MANHATTAN:
                return 1.0;
            case HeuristicType.EUCLIDEAN:
                return 1.0;
            case HeuristicType.OCTILE:
                return 1.0;
            case HeuristicType.CHEBYSHEV:
                return 1.0;
            case HeuristicType.ZERO:
                return 0;
            default:
                return 1.0;
        }
    }

    public static isConsistent(heuristicType: HeuristicType): boolean {
        switch (heuristicType) {
            case HeuristicType.EUCLIDEAN:
            case HeuristicType.MANHATTAN:
            case HeuristicType.OCTILE:
            case HeuristicType.CHEBYSHEV:
                return true;
            case HeuristicType.ZERO:
                return false;
            default:
                return true;
        }
    }
}

export class PathFinderAStar implements PathFinder {
    protected grid: boolean[][];
    protected terrainCosts: number[][];
    protected worldSize: Vector3;
    protected gridSize: Vector3;
    protected nodePool: PathNodePool;
    protected openList: PathNode[];
    protected heuristicType: HeuristicType;
    protected diagonalMovement: boolean;
    protected allowDiagonal: boolean;

    constructor(gridSize: Vector3, worldSize: Vector3, heuristicType: HeuristicType = HeuristicType.EUCLIDEAN) {
        this.gridSize = gridSize;
        this.worldSize = worldSize;
        this.heuristicType = heuristicType;
        this.diagonalMovement = true;
        this.allowDiagonal = true;
        this.grid = [];
        this.terrainCosts = [];
        this.nodePool = new PathNodePool(gridSize, worldSize);
        this.openList = [];
        this.initializeGrid();
    }

    protected initializeGrid(): void {
        const gridWidth = Math.ceil(worldSize.x / this.gridSize.x);
        const gridHeight = Math.ceil(worldSize.z / this.gridSize.z);

        this.grid = [];
        this.terrainCosts = [];

        for (let x = 0; x < gridWidth; x++) {
            this.grid[x] = [];
            this.terrainCosts[x] = [];
            for (let z = 0; z < gridHeight; z++) {
                this.grid[x][z] = true;
                this.terrainCosts[x][z] = 1.0;
            }
        }
    }

    public getAlgorithm(): PathfindingAlgorithm {
        return PathfindingAlgorithm.A_STAR;
    }

    public setGrid(grid: boolean[][]): void {
        this.grid = grid;
    }

    public setTerrainCosts(costs: number[][]): void {
        this.terrainCosts = costs;
    }

    public updateObstacles(obstacles: Vector3[]): void {
        for (const obstacle of obstacles) {
            const gridX = Math.floor(obstacle.x / this.gridSize.x);
            const gridZ = Math.floor(obstacle.z / this.gridSize.z);

            if (gridX >= 0 && gridX < this.grid.length && gridZ >= 0 && gridZ < this.grid[0].length) {
                const radius = Math.ceil(obstacle.y / this.gridSize.y) + 1;
                for (let dx = -radius; dx <= radius; dx++) {
                    for (let dz = -radius; dz <= radius; dz++) {
                        const nx = gridX + dx;
                        const nz = gridZ + dz;
                        if (nx >= 0 && nx < this.grid.length && nz >= 0 && nz < this.grid[0].length) {
                            const distance = Math.sqrt(dx * dx + dz * dz);
                            if (distance <= radius) {
                                this.grid[nx][nz] = false;
                            }
                        }
                    }
                }
            }
        }
    }

    public setHeuristicType(type: HeuristicType): void {
        this.heuristicType = type;
    }

    public setDiagonalMovement(enabled: boolean): void {
        this.diagonalMovement = enabled;
        this.allowDiagonal = enabled;
    }

    public findPath(start: Vector3, end: Vector3, request?: PathRequest): PathResult {
        const startTime = performance.now();
        const maxIterations = request?.maxIterations || 10000;
        const allowPartial = request?.allowPartialPath || false;

        const gridStartX = Math.floor(start.x / this.gridSize.x);
        const gridStartZ = Math.floor(start.z / this.gridSize.z);
        const gridEndX = Math.floor(end.x / this.gridSize.x);
        const gridEndZ = Math.floor(end.z / this.gridSize.z);

        if (!this.isInBounds(gridStartX, gridStartZ) || !this.isInBounds(gridEndX, gridEndZ)) {
            return this.createFailureResult(request?.id || '', startTime, 'Start or end position out of bounds');
        }

        if (!this.grid[gridEndX] || !this.grid[gridEndX][gridEndZ] || !this.grid[gridEndX][gridEndZ]) {
            return this.createFailureResult(request?.id || '', startTime, 'End position is not walkable');
        }

        this.nodePool.releaseAll();
        this.openList = [];

        const startNode = this.nodePool.acquire();
        startNode.gridX = gridStartX;
        startNode.gridZ = gridStartZ;
        startNode.position = start.clone();
        startNode.g = 0;
        startNode.h = Heuristic.calculate(this.heuristicType, start, end);
        startNode.f = startNode.g + startNode.h;
        startNode.parent = null;
        startNode.inOpenSet = true;
        startNode.inClosedSet = false;

        this.openList.push(startNode);
        let iterations = 0;

        while (this.openList.length > 0 && iterations < maxIterations) {
            iterations++;
            const current = this.getLowestFScoreNode();

            if (current.gridX === gridEndX && current.gridZ === gridEndZ) {
                return this.createSuccessResult(request?.id || '', current, startTime, iterations);
            }

            current.inOpenSet = false;
            current.inClosedSet = true;

            const neighbors = this.getNeighbors(current, gridEndX, gridEndZ);

            for (const neighbor of neighbors) {
                if (neighbor.inClosedSet) continue;

                const tentativeG = current.g + this.getMovementCost(neighbor, current);

                if (tentativeG < neighbor.g) {
                    neighbor.parent = current;
                    neighbor.g = tentativeG;
                    neighbor.h = Heuristic.calculate(this.heuristicType, neighbor.position, end);
                    neighbor.f = neighbor.g + neighbor.h;

                    if (!neighbor.inOpenSet) {
                        neighbor.inOpenSet = true;
                        this.openList.push(neighbor);
                    }
                }
            }
        }

        if (this.openList.length === 0 || iterations >= maxIterations) {
            if (allowPartial) {
                const lastNode = this.openList.length > 0 ? this.getLowestFScoreNode() : this.getLastReachableNode(gridEndX, gridEndZ);
                if (lastNode) {
                    return this.createPartialResult(request?.id || '', lastNode, startTime, iterations);
                }
            }
            return this.createFailureResult(request?.id || '', startTime, 'No path found', iterations);
        }

        return this.createFailureResult(request?.id || '', startTime, 'Pathfinding terminated unexpectedly', iterations);
    }

    protected getLowestFScoreNode(): PathNode {
        let lowest = this.openList[0];
        let lowestIndex = 0;

        for (let i = 1; i < this.openList.length; i++) {
            if (this.openList[i].f < lowest.f || (this.openList[i].f === lowest.f && this.openList[i].h < lowest.h)) {
                lowest = this.openList[i];
                lowestIndex = i;
            }
        }

        this.openList.splice(lowestIndex, 1);
        return lowest;
    }

    protected getNeighbors(node: PathNode, targetX: number, targetZ: number): PathNode[] {
        const neighbors: PathNode[] = [];
        const directions = this.getDirections();

        for (const dir of directions) {
            const nx = node.gridX + dir.x;
            const nz = node.gridZ + dir.z;

            if (!this.isInBounds(nx, nz)) continue;
            if (!this.grid[nx] || !this.grid[nx][nz] || !this.grid[nx][nz]) continue;

            if (dir.diagonal) {
                if (!this.canMoveDiagonally(node.gridX, node.gridZ, nx, nz)) continue;
            }

            const neighbor = this.nodePool.acquire();
            neighbor.gridX = nx;
            neighbor.gridZ = nz;
            neighbor.position = new Vector3(
                nx * this.gridSize.x + this.gridSize.x / 2,
                0,
                nz * this.gridSize.z + this.gridSize.z / 2
            );
            neighbor.cost = this.terrainCosts[nx]?.[nz] || 1.0;
            neighbors.push(neighbor);
        }

        return neighbors;
    }

    protected getDirections(): Array<{ x: number; z: number; diagonal: boolean }> {
        if (!this.diagonalMovement) {
            return [
                { x: 0, z: -1, diagonal: false },
                { x: 1, z: 0, diagonal: false },
                { x: 0, z: 1, diagonal: false },
                { x: -1, z: 0, diagonal: false }
            ];
        }

        return [
            { x: 0, z: -1, diagonal: false },
            { x: 1, z: 0, diagonal: false },
            { x: 0, z: 1, diagonal: false },
            { x: -1, z: 0, diagonal: false },
            { x: 1, z: -1, diagonal: true },
            { x: 1, z: 1, diagonal: true },
            { x: -1, z: 1, diagonal: true },
            { x: -1, z: -1, diagonal: true }
        ];
    }

    protected canMoveDiagonally(fromX: number, fromZ: number, toX: number, toZ: number): boolean {
        const dx = toX - fromX;
        const dz = toZ - fromZ;

        if (this.grid[fromX + dx]?.[fromZ] === false || this.grid[fromX]?.[fromZ + dz] === false) {
            return false;
        }

        return true;
    }

    protected getMovementCost(node: PathNode, parent: PathNode): number {
        const baseCost = this.terrainCosts[node.gridX]?.[node.gridZ] || 1.0;
        const dx = Math.abs(node.gridX - parent.gridX);
        const dz = Math.abs(node.gridZ - parent.gridZ);

        if (dx === 1 && dz === 1) {
            return Math.SQRT2 * baseCost;
        }

        return baseCost;
    }

    protected isInBounds(x: number, z: number): boolean {
        return x >= 0 && x < this.grid.length && z >= 0 && z < (this.grid[0]?.length || 0);
    }

    protected getLastReachableNode(targetX: number, targetZ: number): PathNode | null {
        let closest: PathNode | null = null;
        let closestDistance = Infinity;

        for (const node of this.openList) {
            const dx = node.gridX - targetX;
            const dz = node.gridZ - targetZ;
            const distance = Math.sqrt(dx * dx + dz * dz);

            if (distance < closestDistance) {
                closestDistance = distance;
                closest = node;
            }
        }

        return closest;
    }

    protected retracePath(startNode: PathNode, endNode: PathNode): Vector3[] {
        const path: Vector3[] = [];
        let current: PathNode | null = endNode;

        while (current !== null) {
            path.unshift(current.position.clone());
            current = current.parent;
        }

        return path;
    }

    protected createSuccessResult(requestId: string, endNode: PathNode, startTime: number, iterations: number): PathResult {
        const path = this.retracePath(endNode.parent, endNode);
        const cost = endNode.g;
        const length = path.length;

        return {
            requestId,
            status: PathStatus.SUCCESS,
            path,
            cost,
            length,
            iterations,
            timeMs: performance.now() - startTime
        };
    }

    protected createPartialResult(requestId: string, endNode: PathNode, startTime: number, iterations: number): PathResult {
        const path = this.retracePath(endNode.parent, endNode);
        const cost = endNode.g;

        return {
            requestId,
            status: PathStatus.PARTIAL,
            path,
            cost,
            length: path.length,
            iterations,
            timeMs: performance.now() - startTime,
            partial: true
        };
    }

    protected createFailureResult(requestId: string, startTime: number, message: string, iterations: number = 0): PathResult {
        return {
            requestId,
            status: PathStatus.FAILURE,
            path: [],
            cost: Infinity,
            length: 0,
            iterations,
            timeMs: performance.now() - startTime,
            message
        };
    }

    public getGridDimensions(): { width: number; height: number } {
        return {
            width: this.grid.length,
            height: this.grid[0]?.length || 0
        };
    }

    public isWalkable(x: number, z: number): boolean {
        return this.grid[x]?.[z] ?? false;
    }

    public getTerrainCost(x: number, z: number): number {
        return this.terrainCosts[x]?.[z] ?? 1.0;
    }

    public reset(): void {
        this.nodePool.releaseAll();
        this.openList = [];
    }
}

export class PathFinderJumpPointSearch extends PathFinderAStar {
    private jumpPointCache: Map<string, PathNode>;

    constructor(gridSize: Vector3, worldSize: Vector3, heuristicType: HeuristicType = HeuristicType.OCTILE) {
        super(gridSize, worldSize, heuristicType);
        this.jumpPointCache = new Map();
    }

    public getAlgorithm(): PathfindingAlgorithm {
        return PathfindingAlgorithm.JUMP_POINT_SEARCH;
    }

    public findPath(start: Vector3, end: Vector3, request?: PathRequest): PathResult {
        return super.findPath(start, end, request);
    }

    protected getNeighbors(node: PathNode, targetX: number, targetZ: number): PathNode[] {
        const neighbors: PathNode[] = [];
        const parent = node.parent;

        if (!parent) {
            return super.getNeighbors(node, targetX, targetZ);
        }

        const dx = node.gridX - parent.gridX;
        const dz = node.gridZ - parent.gridZ;

        if (dx !== 0 && dz !== 0) {
            if (this.grid[node.gridX + dx]?.[node.gridZ] ?? false) {
                neighbors.push(this.jump(node.gridX + dx, node.gridZ, dx, 0, targetX, targetZ));
            }
            if (this.grid[node.gridX]?.[node.gridZ + dz] ?? false) {
                neighbors.push(this.jump(node.gridX, node.gridZ + dz, 0, dz, targetX, targetZ));
            }
            if (this.grid[node.gridX + dx]?.[node.gridZ + dz] ?? false) {
                neighbors.push(this.jump(node.gridX + dx, node.gridZ + dz, dx, dz, targetX, targetZ));
            }

            const neighborL = this.grid[node.gridX - dx]?.[node.gridZ] ?? false;
            const neighborDL = this.grid[node.gridX - dx]?.[node.gridZ + dz] ?? false;
            if (!neighborL && neighborDL) {
                neighbors.push(this.jump(node.gridX - dx, node.gridZ + dz, -dx, dz, targetX, targetZ));
            }

            const neighborR = this.grid[node.gridX + dx]?.[node.gridZ] ?? false;
            const neighborDR = this.grid[node.gridX + dx]?.[node.gridZ + dz] ?? false;
            if (!neighborR && neighborDR) {
                neighbors.push(this.jump(node.gridX + dx, node.gridZ + dz, dx, dz, targetX, targetZ));
            }
        } else {
            if (dx !== 0) {
                if (this.grid[node.gridX + dx]?.[node.gridZ] ?? false) {
                    neighbors.push(this.jump(node.gridX + dx, node.gridZ, dx, 0, targetX, targetZ));
                }

                const neighborF = this.grid[node.gridX]?.[node.gridZ + 1] ?? false;
                const neighborFF = this.grid[node.gridX + dx]?.[node.gridZ + 1] ?? false;
                if (!neighborF && neighborFF) {
                    neighbors.push(this.jump(node.gridX + dx, node.gridZ + 1, dx, 1, targetX, targetZ));
                }

                const neighborB = this.grid[node.gridX]?.[node.gridZ - 1] ?? false;
                const neighborBB = this.grid[node.gridX + dx]?.[node.gridZ - 1] ?? false;
                if (!neighborB && neighborBB) {
                    neighbors.push(this.jump(node.gridX + dx, node.gridZ - 1, dx, -1, targetX, targetZ));
                }
            } else {
                if (this.grid[node.gridX]?.[node.gridZ + dz] ?? false) {
                    neighbors.push(this.jump(node.gridX, node.gridZ + dz, 0, dz, targetX, targetZ));
                }

                const neighborF = this.grid[node.gridX + 1]?.[node.gridZ] ?? false;
                const neighborFF = this.grid[node.gridX + 1]?.[node.gridZ + dz] ?? false;
                if (!neighborF && neighborFF) {
                    neighbors.push(this.jump(node.gridX + 1, node.gridZ + dz, 1, dz, targetX, targetZ));
                }

                const neighborB = this.grid[node.gridX - 1]?.[node.gridZ] ?? false;
                const neighborBB = this.grid[node.gridX - 1]?.[node.gridZ + dz] ?? false;
                if (!neighborB && neighborBB) {
                    neighbors.push(this.jump(node.gridX - 1, node.gridZ + dz, -1, dz, targetX, targetZ));
                }
            }
        }

        return neighbors.filter(n => n !== null);
    }

    private jump(x: number, z: number, dx: number, dz: number, targetX: number, targetZ: number): PathNode | null {
        const cacheKey = `${x},${z},${dx},${dz}`;

        if (x < 0 || z < 0 || x >= this.grid.length || z >= (this.grid[0]?.length || 0)) {
            return null;
        }

        if (!(this.grid[x]?.[z] ?? false)) {
            return null;
        }

        const jumpPoint = this.nodePool.acquire();
        jumpPoint.gridX = x;
        jumpPoint.gridZ = z;
        jumpPoint.position = new Vector3(
            x * this.gridSize.x + this.gridSize.x / 2,
            0,
            z * this.gridSize.z + this.gridSize.z / 2
        );
        jumpPoint.jumpPoint = true;
        jumpPoint.cost = this.terrainCosts[x]?.[z] || 1.0;

        if (x === targetX && z === targetZ) {
            return jumpPoint;
        }

        const prevX = x - dx;
        const prevZ = z - dz;

        if (dx !== 0 && dz !== 0) {
            const cornerLeft = !(this.grid[prevX]?.[z] ?? true);
            const cornerDown = !(this.grid[x]?.[prevZ] ?? true);
            if (cornerLeft || cornerDown) {
                return jumpPoint;
            }
        }

        if (dx !== 0) {
            const cornerF = !(this.grid[x]?.[z + 1] ?? true);
            const jumpF = this.jump(x, z + 1, dx, 1, targetX, targetZ);
            if (cornerF || jumpF) {
                return jumpPoint;
            }

            const cornerB = !(this.grid[x]?.[z - 1] ?? true);
            const jumpB = this.jump(x, z - 1, dx, -1, targetX, targetZ);
            if (cornerB || jumpB) {
                return jumpPoint;
            }
        } else if (dz !== 0) {
            const cornerF = !(this.grid[x + 1]?.[z] ?? true);
            const jumpF = this.jump(x + 1, z, 1, dz, targetX, targetZ);
            if (cornerF || jumpF) {
                return jumpPoint;
            }

            const cornerB = !(this.grid[x - 1]?.[z] ?? true);
            const jumpB = this.jump(x - 1, z, -1, dz, targetX, targetZ);
            if (cornerB || jumpB) {
                return jumpPoint;
            }
        }

        return this.jump(x + dx, z + dz, dx, dz, targetX, targetZ);
    }

    public clearCache(): void {
        this.jumpPointCache.clear();
    }
}

export class PathFinderFlowField implements PathFinder {
    private grid: boolean[][];
    private integrationField: number[][];
    private flowField: Vector3[][];
    private worldSize: Vector3;
    private gridSize: Vector3;
    private targetPosition: Vector3 | null;
    private valid: boolean;

    constructor(gridSize: Vector3, worldSize: Vector3) {
        this.gridSize = gridSize;
        this.worldSize = worldSize;
        this.grid = [];
        this.integrationField = [];
        this.flowField = [];
        this.targetPosition = null;
        this.valid = false;
        this.initializeGrids();
    }

    private initializeGrids(): void {
        const width = Math.ceil(this.worldSize.x / this.gridSize.x);
        const height = Math.ceil(this.worldSize.z / this.gridSize.z);

        this.grid = [];
        this.integrationField = [];
        this.flowField = [];

        for (let x = 0; x < width; x++) {
            this.grid[x] = [];
            this.integrationField[x] = [];
            this.flowField[x] = [];
            for (let z = 0; z < height; z++) {
                this.grid[x][z] = true;
                this.integrationField[x][z] = Infinity;
                this.flowField[x][z] = Vector3.ZERO;
            }
        }
    }

    public getAlgorithm(): PathfindingAlgorithm {
        return PathfindingAlgorithm.FLOW_FIELD;
    }

    public setGrid(grid: boolean[][]): void {
        this.grid = grid;
        this.valid = false;
    }

    public setTerrainCosts(costs: number[][]): void {
    }

    public updateObstacles(obstacles: Vector3[]): void {
        const radius = 1;

        for (const obstacle of obstacles) {
            const gridX = Math.floor(obstacle.x / this.gridSize.x);
            const gridZ = Math.floor(obstacle.z / this.gridSize.z);

            for (let dx = -radius; dx <= radius; dx++) {
                for (let dz = -radius; dz <= radius; dz++) {
                    const nx = gridX + dx;
                    const nz = gridZ + dz;
                    if (nx >= 0 && nx < this.grid.length && nz >= 0 && nz < (this.grid[0]?.length || 0)) {
                        const distance = Math.sqrt(dx * dx + dz * dz);
                        if (distance <= radius) {
                            this.grid[nx][nz] = false;
                        }
                    }
                }
            }
        }

        this.valid = false;
    }

    public findPath(start: Vector3, end: Vector3, request?: PathRequest): PathResult {
        const startTime = performance.now();

        const gridEndX = Math.floor(end.x / this.gridSize.x);
        const gridEndZ = Math.floor(end.z / this.gridSize.z);

        if (!this.isInBounds(gridEndX, gridEndZ) || !this.grid[gridEndX]?.[gridEndZ]) {
            return {
                requestId: request?.id || '',
                status: PathStatus.FAILURE,
                path: [],
                cost: Infinity,
                length: 0,
                iterations: 0,
                timeMs: performance.now() - startTime,
                message: 'Target position is not walkable'
            };
        }

        if (!this.targetPosition || 
            Math.abs(this.targetPosition.x - end.x) > 0.1 || 
            Math.abs(this.targetPosition.z - end.z) > 0.1) {
            this.computeIntegrationField(gridEndX, gridEndZ);
            this.computeFlowField();
            this.targetPosition = end.clone();
            this.valid = true;
        }

        const path = this.extractPath(start);
        const cost = this.integrationField[gridEndX]?.[gridEndZ] || 0;

        return {
            requestId: request?.id || '',
            status: path.length > 0 ? PathStatus.SUCCESS : PathStatus.FAILURE,
            path,
            cost,
            length: path.length,
            iterations: 1,
            timeMs: performance.now() - startTime
        };
    }

    private computeIntegrationField(targetX: number, targetZ: number): void {
        const width = this.grid.length;
        const height = this.grid[0]?.length || 0;

        for (let x = 0; x < width; x++) {
            for (let z = 0; z < height; z++) {
                this.integrationField[x][z] = Infinity;
            }
        }

        const queue: Array<{ x: number; z: number; cost: number }> = [];
        this.integrationField[targetX][targetZ] = 0;
        queue.push({ x: targetX, z: targetZ, cost: 0 });

        while (queue.length > 0) {
            const current = queue.shift()!;

            const neighbors = this.getCardinalNeighbors(current.x, current.z);
            for (const neighbor of neighbors) {
                const cost = current.cost + (this.grid[neighbor.x]?.[neighbor.z] ? 1 : 100);
                if (cost < this.integrationField[neighbor.x][neighbor.z]) {
                    this.integrationField[neighbor.x][neighbor.z] = cost;
                    queue.push({ x: neighbor.x, z: neighbor.z, cost });
                }
            }
        }
    }

    private computeFlowField(): void {
        const width = this.grid.length;
        const height = this.grid[0]?.length || 0;

        for (let x = 0; x < width; x++) {
            for (let z = 0; z < height; z++) {
                if (!this.grid[x]?.[z]) {
                    this.flowField[x][z] = Vector3.ZERO;
                    continue;
                }

                const currentCost = this.integrationField[x][z];
                let bestDirection = Vector3.ZERO;
                let lowestCost = Infinity;

                const neighbors = this.getAllNeighbors(x, z);
                for (const neighbor of neighbors) {
                    if (!this.grid[neighbor.x]?.[neighbor.z]) continue;

                    const neighborCost = this.integrationField[neighbor.x][neighbor.z];
                    if (neighborCost < lowestCost) {
                        lowestCost = neighborCost;
                        const dirX = neighbor.x - x;
                        const dirZ = neighbor.z - z;
                        const length = Math.sqrt(dirX * dirX + dirZ * dirZ);
                        bestDirection = new Vector3(dirX / length, 0, dirZ / length);
                    }
                }

                this.flowField[x][z] = bestDirection;
            }
        }
    }

    private getCardinalNeighbors(x: number, z: number): Array<{ x: number; z: number }> {
        const neighbors: Array<{ x: number; z: number }> = [];

        if (x > 0 && this.grid[x - 1]?.[z]) neighbors.push({ x: x - 1, z });
        if (x < this.grid.length - 1 && this.grid[x + 1]?.[z]) neighbors.push({ x: x + 1, z });
        if (z > 0 && this.grid[x]?.[z - 1]) neighbors.push({ x: x, z: z - 1 });
        if (z < (this.grid[0]?.length || 0) - 1 && this.grid[x]?.[z + 1]) neighbors.push({ x: x, z: z + 1 });

        return neighbors;
    }

    private getAllNeighbors(x: number, z: number): Array<{ x: number; z: number }> {
        const neighbors: Array<{ x: number; z: number }> = [];

        for (let dx = -1; dx <= 1; dx++) {
            for (let dz = -1; dz <= 1; dz++) {
                if (dx === 0 && dz === 0) continue;
                const nx = x + dx;
                const nz = z + dz;
                if (nx >= 0 && nx < this.grid.length && nz >= 0 && nz < (this.grid[0]?.length || 0)) {
                    neighbors.push({ x: nx, z: nz });
                }
            }
        }

        return neighbors;
    }

    private extractPath(start: Vector3): Vector3[] {
        const path: Vector3[] = [];
        let currentX = Math.floor(start.x / this.gridSize.x);
        let currentZ = Math.floor(start.z / this.gridSize.z);

        if (!this.isInBounds(currentX, currentZ)) {
            return path;
        }

        path.push(new Vector3(
            currentX * this.gridSize.x + this.gridSize.x / 2,
            0,
            currentZ * this.gridSize.z + this.gridSize.z / 2
        ));

        const maxIterations = 1000;
        let iterations = 0;

        while (iterations < maxIterations) {
            iterations++;

            if (!this.grid[currentX]?.[currentZ]) {
                break;
            }

            const flow = this.flowField[currentX][currentZ];

            if (flow.isZero()) {
                break;
            }

            currentX = Math.round(currentX + flow.x);
            currentZ = Math.round(currentZ + flow.z);

            if (!this.isInBounds(currentX, currentZ)) {
                break;
            }

            if (!this.grid[currentX]?.[currentZ]) {
                break;
            }

            path.push(new Vector3(
                currentX * this.gridSize.x + this.gridSize.x / 2,
                0,
                currentZ * this.gridSize.z + this.gridSize.z / 2
            ));

            if (this.integrationField[currentX]?.[currentZ] === 0) {
                break;
            }
        }

        return path;
    }

    private isInBounds(x: number, z: number): boolean {
        return x >= 0 && x < this.grid.length && z >= 0 && z < (this.grid[0]?.length || 0);
    }

    public getFlowDirection(x: number, z: number): Vector3 {
        if (!this.isInBounds(x, z)) return Vector3.ZERO;
        return this.flowField[x][z].clone();
    }

    public getIntegrationCost(x: number, z: number): number {
        if (!this.isInBounds(x, z)) return Infinity;
        return this.integrationField[x][z];
    }

    public invalidate(): void {
        this.valid = false;
    }

    public isValid(): boolean {
        return this.valid;
    }
}

export class PathFinderNavMesh implements PathFinder {
    private mesh: NavMeshTriangle[];
    private spatialIndex: SpatialHash;
    private worldSize: Vector3;

    constructor(worldSize: Vector3) {
        this.worldSize = worldSize;
        this.mesh = [];
        this.spatialIndex = new SpatialHash(worldSize, 10);
    }

    public getAlgorithm(): PathfindingAlgorithm {
        return PathfindingAlgorithm.A_STAR;
    }

    public setGrid(grid: boolean[][]): void {
    }

    public setTerrainCosts(costs: number[][]): void {
    }

    public updateObstacles(obstacles: Vector3[]): void {
        for (const obstacle of obstacles) {
            this.spatialIndex.remove(obstacle);
        }
    }

    public addTriangle(triangle: NavMeshTriangle): void {
        this.mesh.push(triangle);
        this.spatialIndex.insert(triangle.centroid, triangle);
    }

    public buildMesh(vertices: Vector3[], triangles: number[]): void {
        this.mesh = [];

        for (let i = 0; i < triangles.length; i += 3) {
            const a = vertices[triangles[i]];
            const b = vertices[triangles[i + 1]];
            const c = vertices[triangles[i + 2]];

            const centroid = Vector3.add(Vector3.add(a, b), c).multiplyScalar(1 / 3);

            const triangle: NavMeshTriangle = {
                vertices: [a, b, c],
                centroid,
                neighbors: [],
                walkable: true,
                cost: 1.0
            };

            this.mesh.push(triangle);
        }

        this.buildNeighborConnections();
        this.rebuildSpatialIndex();
    }

    private buildNeighborConnections(): void {
        for (let i = 0; i < this.mesh.length; i++) {
            for (let j = i + 1; j < this.mesh.length; j++) {
                if (this.trianglesShareEdge(this.mesh[i], this.mesh[j])) {
                    this.mesh[i].neighbors.push(j);
                    this.mesh[j].neighbors.push(i);
                }
            }
        }
    }

    private trianglesShareEdge(t1: NavMeshTriangle, t2: NavMeshTriangle): boolean {
        let sharedVertices = 0;

        for (const v1 of t1.vertices) {
            for (const v2 of t2.vertices) {
                if (v1.distanceToSquared(v2) < 0.01) {
                    sharedVertices++;
                }
            }
        }

        return sharedVertices === 2;
    }

    private rebuildSpatialIndex(): void {
        this.spatialIndex = new SpatialHash(this.worldSize, 10);
        for (let i = 0; i < this.mesh.length; i++) {
            this.spatialIndex.insert(this.mesh[i].centroid, this.mesh[i]);
        }
    }

    public findPath(start: Vector3, end: Vector3, request?: PathRequest): PathResult {
        const startTime = performance.now();
        const maxIterations = request?.maxIterations || 5000;

        const startTriangle = this.findTriangleContaining(start);
        const endTriangle = this.findTriangleContaining(end);

        if (!startTriangle || !endTriangle) {
            return {
                requestId: request?.id || '',
                status: PathStatus.FAILURE,
                path: [],
                cost: Infinity,
                length: 0,
                iterations: 0,
                timeMs: performance.now() - startTime,
                message: 'Start or end position not in navigable area'
            };
        }

        if (startTriangle === endTriangle) {
            return {
                requestId: request?.id || '',
                status: PathStatus.SUCCESS,
                path: [start, end],
                cost: start.distanceTo(end),
                length: 2,
                iterations: 1,
                timeMs: performance.now() - startTime
            };
        }

        const path = this.aStarNavMesh(start, end, startTriangle, endTriangle, maxIterations);

        if (path.length > 0) {
            const smoothedPath = this.smoothPath(path);
            const cost = this.calculatePathCost(path);

            return {
                requestId: request?.id || '',
                status: PathStatus.SUCCESS,
                path: smoothedPath,
                smoothedPath,
                cost,
                length: smoothedPath.length,
                iterations: path.length,
                timeMs: performance.now() - startTime
            };
        }

        return {
            requestId: request?.id || '',
            status: PathStatus.FAILURE,
            path: [],
            cost: Infinity,
            length: 0,
            iterations: 0,
            timeMs: performance.now() - startTime,
            message: 'No path found in navmesh'
        };
    }

    private aStarNavMesh(start: Vector3, end: Vector3, startTri: number, endTri: number, maxIterations: number): number[] {
        const openSet: NavMeshNode[] = [];
        const closedSet = new Set<number>();
        const nodeMap = new Map<number, NavMeshNode>();

        const startNode: NavMeshNode = {
            triangleIndex: startTri,
            g: 0,
            h: this.estimateCost(this.mesh[startTri].centroid, end),
            f: 0,
            parent: null
        };
        startNode.f = startNode.g + startNode.h;

        openSet.push(startNode);
        nodeMap.set(startTri, startNode);
        let iterations = 0;

        while (openSet.length > 0 && iterations < maxIterations) {
            iterations++;

            openSet.sort((a, b) => a.f - b.f);
            const current = openSet.shift()!;
            closedSet.add(current.triangleIndex);

            if (current.triangleIndex === endTri) {
                return this.reconstructNavMeshPath(current);
            }

            const triangle = this.mesh[current.triangleIndex];

            for (const neighborIndex of triangle.neighbors) {
                if (closedSet.has(neighborIndex)) continue;

                const neighbor = this.mesh[neighborIndex];
                if (!neighbor.walkable) continue;

                const edgeCrossingCost = this.calculateEdgeCrossingCost(triangle, neighbor, current.triangleIndex === startTri ? start : this.mesh[current.triangleIndex].centroid);
                const tentativeG = current.g + edgeCrossingCost;

                let neighborNode = nodeMap.get(neighborIndex);

                if (!neighborNode) {
                    neighborNode = {
                        triangleIndex: neighborIndex,
                        g: Infinity,
                        h: this.estimateCost(neighbor.centroid, end),
                        f: Infinity,
                        parent: null
                    };
                    nodeMap.set(neighborIndex, neighborNode);
                    openSet.push(neighborNode);
                }

                if (tentativeG < neighborNode.g) {
                    neighborNode.parent = current;
                    neighborNode.g = tentativeG;
                    neighborNode.f = neighborNode.g + neighborNode.h;
                }
            }
        }

        return [];
    }

    private estimateCost(from: Vector3, to: Vector3): number {
        return from.distanceTo(to);
    }

    private calculateEdgeCrossingCost(from: NavMeshTriangle, to: NavMeshTriangle, fromPoint: Vector3): number {
        const toPoint = to.centroid;
        const distance = fromPoint.distanceTo(toPoint);
        return distance * to.cost;
    }

    private reconstructNavMeshPath(endNode: NavMeshNode): number[] {
        const path: number[] = [];
        let current: NavMeshNode | null = endNode;

        while (current !== null) {
            path.unshift(current.triangleIndex);
            current = current.parent;
        }

        return path;
    }

    private smoothPath(path: number[]): Vector3[] {
        if (path.length <= 2) {
            return path.map(i => this.mesh[i].centroid.clone());
        }

        const smoothed: Vector3[] = [this.mesh[path[0]].centroid.clone()];

        for (let i = 0; i < path.length - 1; i++) {
            const currentTri = path[i];
            const nextTri = path[i + 1];

            if (!this.canStraightLineConnect(currentTri, nextTri)) {
                smoothed.push(this.mesh[currentTri].centroid.clone());
            }
        }

        smoothed.push(this.mesh[path[path.length - 1]].centroid.clone());

        return smoothed;
    }

    private canStraightLineConnect(tri1Index: number, tri2Index: number): boolean {
        const tri1 = this.mesh[tri1Index];
        const tri2 = this.mesh[tri2Index];

        for (const neighbor of tri1.neighbors) {
            if (neighbor === tri2Index) {
                return this.trianglesShareEdge(tri1, tri2);
            }
        }

        return false;
    }

    private calculatePathCost(path: number[]): number {
        let cost = 0;

        for (let i = 0; i < path.length; i++) {
            cost += this.mesh[path[i]].cost;
        }

        return cost;
    }

    public findTriangleContaining(point: Vector3): number | null {
        const nearby = this.spatialIndex.getNearby(point, 1);

        for (const tri of nearby) {
            if (this.pointInTriangle(point, tri)) {
                return this.mesh.indexOf(tri);
            }
        }

        let closestIndex = -1;
        let closestDist = Infinity;

        for (let i = 0; i < this.mesh.length; i++) {
            const dist = point.distanceToSquared(this.mesh[i].centroid);
            if (dist < closestDist) {
                closestDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex >= 0 ? closestIndex : null;
    }

    private pointInTriangle(point: Vector3, triangle: NavMeshTriangle): boolean {
        const [a, b, c] = triangle.vertices;

        const v0 = Vector3.subtract(c, a);
        const v1 = Vector3.subtract(b, a);
        const v2 = Vector3.subtract(point, a);

        const dot00 = Vector3.dot(v0, v0);
        const dot01 = Vector3.dot(v0, v1);
        const dot02 = Vector3.dot(v0, v2);
        const dot11 = Vector3.dot(v1, v1);
        const dot12 = Vector3.dot(v1, v2);

        const invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        const u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        const v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return u >= 0 && v >= 0 && u + v <= 1;
    }

    public getTriangleAt(index: number): NavMeshTriangle | null {
        return index >= 0 && index < this.mesh.length ? this.mesh[index] : null;
    }

    public getTriangleCount(): number {
        return this.mesh.length;
    }
}

export interface NavMeshTriangle {
    vertices: Vector3[];
    centroid: Vector3;
    neighbors: number[];
    walkable: boolean;
    cost: number;
}

interface NavMeshNode {
    triangleIndex: number;
    g: number;
    h: number;
    f: number;
    parent: NavMeshNode | null;
}

class SpatialHash {
    private worldSize: Vector3;
    private cellSize: number;
    private cells: Map<string, any[]>;

    constructor(worldSize: Vector3, cellSize: number) {
        this.worldSize = worldSize;
        this.cellSize = cellSize;
        this.cells = new Map();
    }

    public insert(position: Vector3, data: any): void {
        const key = this.getKey(position);
        if (!this.cells.has(key)) {
            this.cells.set(key, []);
        }
        this.cells.get(key)!.push(data);
    }

    public remove(position: Vector3): void {
        const key = this.getKey(position);
        this.cells.delete(key);
    }

    public getNearby(position: Vector3, radius: number = 1): any[] {
        const results: any[] = [];
        const cellX = Math.floor(position.x / this.cellSize);
        const cellZ = Math.floor(position.z / this.cellSize);

        for (let dx = -radius; dx <= radius; dx++) {
            for (let dz = -radius; dz <= radius; dz++) {
                const key = this.getCellKey(cellX + dx, cellZ + dz);
                const cell = this.cells.get(key);
                if (cell) {
                    results.push(...cell);
                }
            }
        }

        return results;
    }

    private getKey(position: Vector3): string {
        const cellX = Math.floor(position.x / this.cellSize);
        const cellZ = Math.floor(position.z / this.cellSize);
        return this.getCellKey(cellX, cellZ);
    }

    private getCellKey(x: number, z: number): string {
        return `${x},${z}`;
    }
}

export class PathFollower {
    private unitData: Map<string, UnitMovementData>;
    private obstacleAvoidance: ObstacleAvoidance;
    private formationMovement: FormationMovement;
    private worldBounds: { min: Vector3; max: Vector3 };
    private arrivalThreshold: number;

    constructor(
        gridSize: Vector3,
        worldSize: Vector3,
        obstacleAvoidance: ObstacleAvoidance,
        formationMovement: FormationMovement
    ) {
        this.unitData = new Map();
        this.obstacleAvoidance = obstacleAvoidance;
        this.formationMovement = formationMovement;
        this.worldBounds = {
            min: new Vector3(0, 0, 0),
            max: worldSize.clone()
        };
        this.arrivalThreshold = gridSize.x * 0.5;
    }

    public registerUnit(
        unitId: string,
        position: Vector3,
        speed: number,
        steeringBehaviors: SteeringBehavior[] = []
    ): void {
        const data: UnitMovementData = {
            unitId,
            position: position.clone(),
            velocity: Vector3.ZERO.clone(),
            targetPosition: Vector3.ZERO.clone(),
            currentPath: [],
            currentWaypointIndex: 0,
            state: MovementState.IDLE,
            speed,
            maxSpeed: speed,
            acceleration: speed * 5,
            steeringBehaviors,
            lastPathUpdate: 0
        };
        this.unitData.set(unitId, data);
    }

    public unregisterUnit(unitId: string): void {
        this.unitData.delete(unitId);
    }

    public setPath(unitId: string, path: Vector3[]): boolean {
        const data = this.unitData.get(unitId);
        if (!data) return false;

        data.currentPath = path;
        data.currentWaypointIndex = 0;
        data.targetPosition = path.length > 0 ? path[0].clone() : Vector3.ZERO.clone();
        data.state = path.length > 0 ? MovementState.MOVING : MovementState.IDLE;

        return true;
    }

    public update(unitId: string, deltaTime: number, nearbyUnits: Map<string, Vector3>): Vector3 {
        const data = this.unitData.get(unitId);
        if (!data) return Vector3.ZERO.clone();

        if (data.state === MovementState.IDLE || data.currentPath.length === 0) {
            data.velocity = Vector3.ZERO.clone();
            return data.position.clone();
        }

        const target = this.getCurrentWaypoint(data);
        const toTarget = Vector3.subtract(target, data.position);
        const distanceToTarget = toTarget.magnitude();

        if (distanceToTarget < this.arrivalThreshold) {
            data.currentWaypointIndex++;
            if (data.currentWaypointIndex >= data.currentPath.length) {
                data.state = MovementState.ARRIVED;
                data.velocity = Vector3.ZERO.clone();
                return data.position.clone();
            }
        }

        let steeringForce = Vector3.ZERO.clone();

        for (const behavior of data.steeringBehaviors) {
            steeringForce.add(this.calculateSteering(data, behavior, nearbyUnits));
        }

        if (steeringForce.isZero()) {
            steeringForce = this.seek(data, target);
        }

        const acceleration = steeringForce.multiplyScalar(deltaTime);
        data.velocity.add(acceleration);

        const speed = data.velocity.magnitude();
        if (speed > data.maxSpeed) {
            data.velocity.normalize().multiplyScalar(data.maxSpeed);
        }

        const avoidanceForce = this.obstacleAvoidance.calculate(
            data.position,
            data.velocity,
            nearbyUnits
        );

        data.velocity.add(avoidanceForce.multiplyScalar(deltaTime));

        data.position.add(data.velocity.clone().multiplyScalar(deltaTime));
        data.position = Vector3.clamp(data.position, this.worldBounds.min, this.worldBounds.max);

        return data.position.clone();
    }

    private getCurrentWaypoint(data: UnitMovementData): Vector3 {
        if (data.currentWaypointIndex < data.currentPath.length) {
            return data.currentPath[data.currentWaypointIndex];
        }
        return data.currentPath[data.currentPath.length - 1] || Vector3.ZERO.clone();
    }

    private seek(data: UnitMovementData, target: Vector3): Vector3 {
        const desired = Vector3.subtract(target, data.position);
        desired.normalize().multiplyScalar(data.maxSpeed);
        return Vector3.subtract(desired, data.velocity);
    }

    private flee(data: UnitMovementData, target: Vector3): Vector3 {
        const desired = Vector3.subtract(data.position, target);
        desired.normalize().multiplyScalar(data.maxSpeed);
        return Vector3.subtract(desired, data.velocity);
    }

    private arrive(data: UnitMovementData, target: Vector3, slowRadius: number = 5): Vector3 {
        const toTarget = Vector3.subtract(target, data.position);
        const distance = toTarget.magnitude();

        if (distance < slowRadius) {
            const scaledSpeed = data.maxSpeed * (distance / slowRadius);
            toTarget.normalize().multiplyScalar(scaledSpeed);
        } else {
            toTarget.normalize().multiplyScalar(data.maxSpeed);
        }

        return Vector3.subtract(toTarget, data.velocity);
    }

    private wander(data: UnitMovementData): Vector3 {
        const wanderAngle = Math.random() * Math.PI * 2;
        const wanderRadius = 1;

        const circleCenter = data.velocity.clone().normalize().multiplyScalar(wanderRadius);
        const displacement = new Vector3(
            Math.cos(wanderAngle) * wanderRadius * 0.5,
            0,
            Math.sin(wanderAngle) * wanderRadius * 0.5
        );

        return circleCenter.add(displacement);
    }

    private separation(data: UnitMovementData, nearbyUnits: Map<string, Vector3>): Vector3 {
        const separationForce = Vector3.ZERO.clone();
        const separationRadius = 3;
        let count = 0;

        for (const [unitId, position] of nearbyUnits) {
            if (unitId === data.unitId) continue;

            const distance = data.position.distanceTo(position);
            if (distance < separationRadius && distance > 0) {
                const diff = Vector3.subtract(data.position, position);
                diff.normalize().divideScalar(distance);
                separationForce.add(diff);
                count++;
            }
        }

        if (count > 0) {
            separationForce.divideScalar(count);
        }

        return separationForce;
    }

    private calculateSteering(
        data: UnitMovementData,
        behavior: SteeringBehavior,
        nearbyUnits: Map<string, Vector3>
    ): Vector3 {
        switch (behavior) {
            case SteeringBehavior.SEEK:
                return this.seek(data, data.targetPosition);
            case SteeringBehavior.FLEE:
                return this.flee(data, data.targetPosition);
            case SteeringBehavior.ARRIVE:
                return this.arrive(data, data.targetPosition);
            case SteeringBehavior.WANDER:
                return this.wander(data);
            case SteeringBehavior.SEPARATION:
                return this.separation(data, nearbyUnits);
            case SteeringBehavior.COHESION:
                return this.cohesion(data, nearbyUnits);
            case SteeringBehavior.ALIGNMENT:
                return this.alignment(data, nearbyUnits);
            case SteeringBehavior.OBSTACLE_AVOIDANCE:
                return this.obstacleAvoidance.calculate(data.position, data.velocity, nearbyUnits);
            default:
                return Vector3.ZERO.clone();
        }
    }

    private cohesion(data: UnitMovementData, nearbyUnits: Map<string, Vector3>): Vector3 {
        const centerOfMass = Vector3.ZERO.clone();
        let count = 0;

        for (const [unitId, position] of nearbyUnits) {
            if (unitId === data.unitId) continue;
            centerOfMass.add(position);
            count++;
        }

        if (count > 0) {
            centerOfMass.divideScalar(count);
            return this.seek(data, centerOfMass);
        }

        return Vector3.ZERO.clone();
    }

    private alignment(data: UnitMovementData, nearbyUnits: Map<string, Vector3>): Vector3 {
        const averageVelocity = Vector3.ZERO.clone();
        let count = 0;

        for (const [unitId] of nearbyUnits) {
            if (unitId === data.unitId) continue;
            const otherData = this.unitData.get(unitId);
            if (otherData) {
                averageVelocity.add(otherData.velocity);
                count++;
            }
        }

        if (count > 0) {
            averageVelocity.divideScalar(count);
            averageVelocity.normalize().multiplyScalar(data.maxSpeed);
            return Vector3.subtract(averageVelocity, data.velocity);
        }

        return Vector3.ZERO.clone();
    }

    public setSteeringBehaviors(unitId: string, behaviors: SteeringBehavior[]): void {
        const data = this.unitData.get(unitId);
        if (data) {
            data.steeringBehaviors = behaviors;
        }
    }

    public setMaxSpeed(unitId: string, speed: number): void {
        const data = this.unitData.get(unitId);
        if (data) {
            data.maxSpeed = speed;
        }
    }

    public getMovementState(unitId: string): MovementState {
        return this.unitData.get(unitId)?.state || MovementState.IDLE;
    }

    public getCurrentPath(unitId: string): Vector3[] {
        return this.unitData.get(unitId)?.currentPath || [];
    }

    public hasReachedDestination(unitId: string): boolean {
        const data = this.unitData.get(unitId);
        return data?.state === MovementState.ARRIVED;
    }

    public forceStop(unitId: string): void {
        const data = this.unitData.get(unitId);
        if (data) {
            data.velocity = Vector3.ZERO.clone();
            data.state = MovementState.IDLE;
        }
    }

    public getRemainingPathLength(unitId: string): number {
        const data = this.unitData.get(unitId);
        if (!data || !data.currentPath) return 0;

        let length = 0;
        const currentPos = data.position;

        if (data.currentWaypointIndex < data.currentPath.length) {
            length += currentPos.distanceTo(data.currentPath[data.currentWaypointIndex]);
        }

        for (let i = data.currentWaypointIndex; i < data.currentPath.length - 1; i++) {
            length += data.currentPath[i].distanceTo(data.currentPath[i + 1]);
        }

        return length;
    }
}

export class ObstacleAvoidance {
    private dynamicObstacles: Map<string, DynamicObstacle>;
    private avoidanceQuality: AvoidanceQuality;
    private forwardRayLength: number;
    private sideRayLength: number;
    private detectionRadius: number;
    private minTimeToCollision: number;

    constructor(
        quality: AvoidanceQuality = AvoidanceQuality.BALANCED,
        forwardRayLength: number = 10,
        detectionRadius: number = 5
    ) {
        this.dynamicObstacles = new Map();
        this.avoidanceQuality = quality;
        this.forwardRayLength = forwardRayLength;
        this.sideRayLength = forwardRayLength * 0.5;
        this.detectionRadius = detectionRadius;
        this.minTimeToCollision = 1.0;

        this.applyQualitySettings();
    }

    private applyQualitySettings(): void {
        switch (this.avoidanceQuality) {
            case AvoidanceQuality.FAST:
                this.forwardRayLength *= 0.5;
                this.sideRayLength *= 0.5;
                this.detectionRadius *= 0.5;
                this.minTimeToCollision *= 0.5;
                break;
            case AvoidanceQuality.THOROUGH:
                this.forwardRayLength *= 1.5;
                this.sideRayLength *= 1.5;
                this.detectionRadius *= 1.5;
                this.minTimeToCollision *= 1.5;
                break;
        }
    }

    public addObstacle(obstacle: DynamicObstacle): void {
        this.dynamicObstacles.set(obstacle.id, obstacle);
    }

    public removeObstacle(obstacleId: string): void {
        this.dynamicObstacles.delete(obstacleId);
    }

    public updateObstacle(obstacleId: string, position: Vector3, velocity: Vector3): void {
        const obstacle = this.dynamicObstacles.get(obstacleId);
        if (obstacle) {
            obstacle.position = position.clone();
            obstacle.velocity = velocity.clone();
            obstacle.lastUpdate = performance.now();
        }
    }

    public calculate(
        agentPosition: Vector3,
        agentVelocity: Vector3,
        nearbyUnits: Map<string, Vector3>
    ): Vector3 {
        const avoidanceForce = Vector3.ZERO.clone();

        for (const obstacle of this.dynamicObstacles.values()) {
            const force = this.calculateObstacleAvoidance(agentPosition, agentVelocity, obstacle);
            avoidanceForce.add(force);
        }

        for (const [, position] of nearbyUnits) {
            const unitObstacle: DynamicObstacle = {
                id: 'unit',
                position: position.clone(),
                velocity: Vector3.ZERO.clone(),
                predictedPosition: position.clone(),
                priority: 1,
                updateInterval: 0,
                lastUpdate: 0
            };
            const force = this.calculateObstacleAvoidance(agentPosition, agentVelocity, unitObstacle);
            avoidanceForce.add(force);
        }

        return avoidanceForce;
    }

    private calculateObstacleAvoidance(
        agentPosition: Vector3,
        agentVelocity: Vector3,
        obstacle: DynamicObstacle
    ): Vector3 {
        const toObstacle = Vector3.subtract(obstacle.position, agentPosition);
        const distance = toObstacle.magnitude();

        if (distance > this.forwardRayLength + obstacle.radius + this.detectionRadius) {
            return Vector3.ZERO.clone();
        }

        const timeToCollision = this.predictTimeToCollision(agentPosition, agentVelocity, obstacle);

        if (timeToCollision > this.minTimeToCollision || timeToCollision < 0) {
            return Vector3.ZERO.clone();
        }

        const avoidanceStrength = 1 - (timeToCollision / this.minTimeToCollision);
        const avoidanceDirection = toObstacle.normalize();

        return avoidanceDirection.multiplyScalar(avoidanceStrength);
    }

    private predictTimeToCollision(
        agentPosition: Vector3,
        agentVelocity: Vector3,
        obstacle: DynamicObstacle
    ): number {
        const toObstacle = Vector3.subtract(obstacle.position, agentPosition);
        const relativeVelocity = Vector3.subtract(agentVelocity, obstacle.velocity);

        const distance = toObstacle.magnitude();
        const combinedRadius = obstacle.radius + this.detectionRadius;

        const dotProduct = Vector3.dot(relativeVelocity, toObstacle.normalize());

        if (dotProduct >= 0) {
            return -1;
        }

        const time = distance / Math.abs(dotProduct);

        return time;
    }

    public setAvoidanceQuality(quality: AvoidanceQuality): void {
        this.avoidanceQuality = quality;
        this.applyQualitySettings();
    }

    public setDetectionParameters(forwardRay: number, sideRay: number, detectionRadius: number): void {
        this.forwardRayLength = forwardRay;
        this.sideRayLength = sideRay;
        this.detectionRadius = detectionRadius;
    }

    public getObstacleCount(): number {
        return this.dynamicObstacles.size;
    }

    public clearObstacles(): void {
        this.dynamicObstacles.clear();
    }
}

export class CollisionDetector {
    public raycast(origin: Vector3, direction: Vector3, maxDistance: number): RaycastResult {
        const normalizedDir = direction.normalize();
        const steps = Math.ceil(maxDistance / 0.1);
        const stepSize = maxDistance / steps;

        for (let i = 0; i < steps; i++) {
            const point = origin.clone().add(normalizedDir.clone().multiplyScalar(stepSize * i));
            for (const obstacle of this.obstacles) {
                if (this.pointInSphere(point, obstacle.position, obstacle.radius)) {
                    return {
                        hit: true,
                        point: point.clone(),
                        normal: Vector3.ZERO.clone(),
                        distance: point.distanceTo(origin),
                        obstacleId: obstacle.id
                    };
                }
            }
        }

        return { hit: false, point: Vector3.ZERO.clone(), normal: Vector3.ZERO.clone(), distance: 0 };
    }

    public raycastSphere(
        origin: Vector3,
        direction: Vector3,
        sphereCenter: Vector3,
        sphereRadius: number
    ): RaycastResult {
        const oc = Vector3.subtract(origin, sphereCenter);
        const a = Vector3.dot(direction, direction);
        const b = 2.0 * Vector3.dot(oc, direction);
        const c = Vector3.dot(oc, oc) - sphereRadius * sphereRadius;
        const discriminant = b * b - 4 * a * c;

        if (discriminant < 0) {
            return { hit: false, point: Vector3.ZERO.clone(), normal: Vector3.ZERO.clone(), distance: 0 };
        }

        const t = (-b - Math.sqrt(discriminant)) / (2 * a);
        if (t < 0) {
            return { hit: false, point: Vector3.ZERO.clone(), normal: Vector3.ZERO.clone(), distance: 0 };
        }

        const point = origin.clone().add(direction.clone().multiplyScalar(t));
        const normal = Vector3.subtract(point, sphereCenter).normalize();

        return {
            hit: true,
            point,
            normal,
            distance: t,
            obstacleId: undefined
        };
    }

    public raycastBox(
        origin: Vector3,
        direction: Vector3,
        boxMin: Vector3,
        boxMax: Vector3
    ): RaycastResult {
        let tmin = -Infinity;
        let tmax = Infinity;

        const axes = [
            { origin: origin.x, dir: direction.x, min: boxMin.x, max: boxMax.x },
            { origin: origin.y, dir: direction.y, min: boxMin.y, max: boxMax.y },
            { origin: origin.z, dir: direction.z, min: boxMin.z, max: boxMax.z }
        ];

        for (const axis of axes) {
            if (Math.abs(axis.dir) < 0.0001) {
                if (axis.origin < axis.min || axis.origin > axis.max) {
                    return { hit: false, point: Vector3.ZERO.clone(), normal: Vector3.ZERO.clone(), distance: 0 };
                }
            } else {
                let t1 = (axis.min - axis.origin) / axis.dir;
                let t2 = (axis.max - axis.origin) / axis.dir;
                if (t1 > t2) [t1, t2] = [t2, t1];

                tmin = Math.max(tmin, t1);
                tmax = Math.min(tmax, t2);

                if (tmin > tmax) {
                    return { hit: false, point: Vector3.ZERO.clone(), normal: Vector3.ZERO.clone(), distance: 0 };
                }
            }
        }

        if (tmin < 0) {
            return { hit: false, point: Vector3.ZERO.clone(), normal: Vector3.ZERO.clone(), distance: 0 };
        }

        const point = origin.clone().add(direction.clone().multiplyScalar(tmin));
        let normal = Vector3.ZERO.clone();

        const epsilon = 0.001;
        if (Math.abs(point.x - boxMin.x) < epsilon) normal = Vector3.LEFT.clone();
        else if (Math.abs(point.x - boxMax.x) < epsilon) normal = Vector3.RIGHT.clone();
        else if (Math.abs(point.z - boxMin.z) < epsilon) normal = Vector3.BACKWARD.clone();
        else if (Math.abs(point.z - boxMax.z) < epsilon) normal = Vector3.FORWARD.clone();
        else if (Math.abs(point.y - boxMin.y) < epsilon) normal = Vector3.DOWN.clone();
        else if (Math.abs(point.y - boxMax.y) < epsilon) normal = Vector3.UP.clone();

        return {
            hit: true,
            point,
            normal,
            distance: tmin,
            obstacleId: undefined
        };
    }

    public sphereSphere(sphere1: Vector3, radius1: number, sphere2: Vector3, radius2: number): boolean {
        const distance = sphere1.distanceTo(sphere2);
        return distance < radius1 + radius2;
    }

    public sphereBox(sphereCenter: Vector3, sphereRadius: number, boxMin: Vector3, boxMax: Vector3): boolean {
        const closestPoint = Vector3.clamp(sphereCenter, boxMin, boxMax);
        const distance = sphereCenter.distanceTo(closestPoint);
        return distance < sphereRadius;
    }

    private pointInSphere(point: Vector3, sphereCenter: Vector3, radius: number): boolean {
        return point.distanceToSquared(sphereCenter) < radius * radius;
    }

    private obstacles: Array<{ id: string; position: Vector3; radius: number }> = [];

    public addObstacle(id: string, position: Vector3, radius: number): void {
        this.obstacles.push({ id, position, radius });
    }

    public removeObstacle(id: string): void {
        this.obstacles = this.obstacles.filter(o => o.id !== id);
    }

    public updateObstacle(id: string, position: Vector3): void {
        const obstacle = this.obstacles.find(o => o.id === id);
        if (obstacle) {
            obstacle.position = position;
        }
    }
}

export interface RaycastResult {
    hit: boolean;
    point: Vector3;
    normal: Vector3;
    distance: number;
    obstacleId?: string;
}

export class FormationMovement {
    private unitPositions: Map<string, Vector3>;
    private formationType: FormationType;
    private formationAnchor: Vector3;
    private formationSpacing: number;
    private unitAssignments: Map<string, number>;

    constructor(spacing: number = 2) {
        this.unitPositions = new Map();
        this.formationType = FormationType.LINE;
        this.formationAnchor = Vector3.ZERO.clone();
        this.formationSpacing = spacing;
        this.unitAssignments = new Map();
    }

    public setFormationType(type: FormationType): void {
        this.formationType = type;
    }

    public setAnchor(position: Vector3): void {
        this.formationAnchor = position.clone();
    }

    public setSpacing(spacing: number): void {
        this.formationSpacing = spacing;
    }

    public calculateFormation(units: Array<{ id: string; position: Vector3 }>, target: Vector3): Map<string, Vector3> {
        const sortedUnits = [...units].sort((a, b) => a.position.x - b.position.x);
        const positions = new Map<string, Vector3>();

        for (let i = 0; i < sortedUnits.length; i++) {
            const unitId = sortedUnits[i].id;
            const formationPos = this.getFormationPosition(i, sortedUnits.length, target);
            positions.set(unitId, formationPos);
            this.unitAssignments.set(unitId, i);
        }

        return positions;
    }

    private getFormationPosition(index: number, totalUnits: number, target: Vector3): Vector3 {
        switch (this.formationType) {
            case FormationType.LINE:
                return this.getLinePosition(index, totalUnits, target);
            case FormationType.VEE:
                return this.getVeePosition(index, totalUnits, target);
            case FormationType.SQUARE:
                return this.getSquarePosition(index, totalUnits, target);
            case FormationType.CIRCLE:
                return this.getCirclePosition(index, totalUnits, target);
            case FormationType.WEDGE:
                return this.getWedgePosition(index, totalUnits, target);
            case FormationType.ECHELON:
                return this.getEchelonPosition(index, totalUnits, target);
            case FormationType.COLUMN:
                return this.getColumnPosition(index, totalUnits, target);
            default:
                return this.getLinePosition(index, totalUnits, target);
        }
    }

    private getLinePosition(index: number, total: number, anchor: Vector3): Vector3 {
        const offset = (index - (total - 1) / 2) * this.formationSpacing;
        return new Vector3(anchor.x + offset, anchor.y, anchor.z);
    }

    private getVeePosition(index: number, total: number, anchor: Vector3): Vector3 {
        const rows = Math.ceil(Math.sqrt(total * 2));
        let currentRow = 0;
        let remainingInRow = 0;

        for (let r = 1; r <= rows; r++) {
            if (index < r) {
                currentRow = r;
                remainingInRow = r;
                break;
            }
            index -= r;
        }

        const rowIndex = index;
        const rowOffset = currentRow * this.formationSpacing;
        const posInRow = (rowIndex - (remainingInRow - 1) / 2) * this.formationSpacing;

        return new Vector3(anchor.x + posInRow, anchor.y, anchor.z + rowOffset);
    }

    private getSquarePosition(index: number, total: number, anchor: Vector3): Vector3 {
        const side = Math.ceil(Math.sqrt(total));
        const row = Math.floor(index / side);
        const col = index % side;
        const offsetX = (col - (side - 1) / 2) * this.formationSpacing;
        const offsetZ = (row - (side - 1) / 2) * this.formationSpacing;

        return new Vector3(anchor.x + offsetX, anchor.y, anchor.z + offsetZ);
    }

    private getCirclePosition(index: number, total: number, anchor: Vector3): Vector3 {
        if (total === 1) return anchor.clone();

        const angle = (2 * Math.PI * index) / total;
        const radius = this.formationSpacing * Math.max(1, total / (2 * Math.PI));

        return new Vector3(
            anchor.x + Math.cos(angle) * radius,
            anchor.y,
            anchor.z + Math.sin(angle) * radius
        );
    }

    private getWedgePosition(index: number, total: number, anchor: Vector3): Vector3 {
        let row = 0;
        let remaining = index;

        while (remaining >= row + 1) {
            remaining -= row + 1;
            row++;
        }

        const posInRow = remaining - row / 2;
        const offsetX = posInRow * this.formationSpacing;
        const offsetZ = row * this.formationSpacing;

        return new Vector3(anchor.x + offsetX, anchor.y, anchor.z + offsetZ);
    }

    private getEchelonPosition(index: number, total: number, anchor: Vector3): Vector3 {
        const row = Math.floor(index / 2);
        const col = index % 2;
        const offsetX = col * this.formationSpacing * 1.5;
        const offsetZ = row * this.formationSpacing;

        return new Vector3(anchor.x + offsetX, anchor.y, anchor.z + offsetZ);
    }

    private getColumnPosition(index: number, total: number, anchor: Vector3): Vector3 {
        const offset = index * this.formationSpacing;

        return new Vector3(anchor.x, anchor.y, anchor.z + offset);
    }

    public getFormationPositionForUnit(unitId: string): Vector3 | null {
        const index = this.unitAssignments.get(unitId);
        if (index === undefined) return null;

        return this.getFormationPosition(index, this.unitAssignments.size, this.formationAnchor);
    }

    public isUnitInFormation(unitId: string): boolean {
        return this.unitAssignments.has(unitId);
    }

    public getFormationCenter(): Vector3 {
        if (this.unitAssignments.size === 0) return this.formationAnchor.clone();

        let center = Vector3.ZERO.clone();
        for (const [unitId] of this.unitAssignments) {
            const pos = this.unitPositions.get(unitId);
            if (pos) center.add(pos);
        }

        return center.divideScalar(this.unitAssignments.size);
    }

    public maintainFormation(
        units: Array<{ id: string; position: Vector3 }>,
        leaderPosition: Vector3,
        leaderDirection: Vector3
    ): Map<string, Vector3> {
        const positions = new Map<string, Vector3>();
        const sortedUnits = [...units].sort((a, b) => {
            const distA = a.position.distanceToSquared(leaderPosition);
            const distB = b.position.distanceToSquared(leaderPosition);
            return distA - distB;
        });

        for (let i = 0; i < sortedUnits.length; i++) {
            const unit = sortedUnits[i];
            const targetFormationPos = this.getFormationPosition(i, sortedUnits.length, leaderPosition);

            const toTarget = Vector3.subtract(targetFormationPos, leaderPosition);
            const rotatedPos = new Vector3(
                toTarget.x * leaderDirection.z - toTarget.z * leaderDirection.x,
                0,
                toTarget.x * (-leaderDirection.x) + toTarget.z * leaderDirection.z
            );

            const finalPos = Vector3.add(leaderPosition, rotatedPos);
            positions.set(unit.id, finalPos);
        }

        return positions;
    }

    public updateUnitPosition(unitId: string, position: Vector3): void {
        this.unitPositions.set(unitId, position.clone());
    }

    public removeUnit(unitId: string): void {
        this.unitPositions.delete(unitId);
        this.unitAssignments.delete(unitId);
    }
}

export class GoalEvaluator {
    private goals: Map<string, Goal>;
    private scores: Map<string, number>;
    private lastEvaluation: number;
    private evaluationInterval: number;

    constructor(evaluationIntervalMs: number = 500) {
        this.goals = new Map();
        this.scores = new Map();
        this.lastEvaluation = 0;
        this.evaluationInterval = evaluationIntervalMs;
    }

    public addGoal(goal: Goal): void {
        this.goals.set(goal.id, goal);
    }

    public removeGoal(goalId: string): void {
        this.goals.delete(goalId);
        this.scores.delete(goalId);
    }

    public updateGoal(goalId: string, updates: Partial<Goal>): void {
        const goal = this.goals.get(goalId);
        if (goal) {
            Object.assign(goal, updates);
        }
    }

    public evaluate(targets: GoalTarget[]): GoalTarget | null {
        const now = performance.now();
        if (now - this.lastEvaluation < this.evaluationInterval) {
            return this.getHighestScoredTarget();
        }

        this.lastEvaluation = now;
        this.scores.clear();

        for (const goal of this.goals.values()) {
            for (const target of targets) {
                const score = this.calculateScore(goal, target);
                const key = `${goal.id}_${target.id}`;

                if (!this.scores.has(key) || this.scores.get(key)! < score) {
                    this.scores.set(key, score);
                }
            }
        }

        return this.getHighestScoredTarget();
    }

    private calculateScore(goal: Goal, target: GoalTarget): number {
        let score = 0;

        const distanceFactor = this.calculateDistanceFactor(goal, target);
        score += distanceFactor * goal.distanceWeight;

        const priorityFactor = this.calculatePriorityFactor(goal, target);
        score += priorityFactor * goal.priorityWeight;

        const resourceFactor = this.calculateResourceFactor(goal, target);
        score += resourceFactor * goal.resourceWeight;

        const threatFactor = this.calculateThreatFactor(goal, target);
        score += threatFactor * goal.threatWeight;

        if (goal.terrainBonus) {
            const terrainFactor = this.calculateTerrainFactor(goal, target);
            score += terrainFactor * goal.terrainBonus;
        }

        const randomFactor = Math.random() * goal.randomness;
        score += randomFactor;

        return score;
    }

    private calculateDistanceFactor(goal: Goal, target: GoalTarget): number {
        if (!goal.currentPosition || !target.position) return 0;

        const distance = goal.currentPosition.distanceTo(target.position);
        const maxDistance = 100;
        const normalized = Math.max(0, 1 - distance / maxDistance);

        return normalized;
    }

    private calculatePriorityFactor(goal: Goal, target: GoalTarget): number {
        return target.priority / 100;
    }

    private calculateResourceFactor(goal: Goal, target: GoalTarget): number {
        if (goal.type === GoalTargetType.GATHER) {
            return (target.resources || 0) / 100;
        }
        return 0;
    }

    private calculateThreatFactor(goal: Goal, target: GoalTarget): number {
        const threat = target.threatLevel || 0;
        const maxThreat = 100;

        switch (goal.type) {
            case GoalTargetType.ATTACK:
                return 1 - Math.min(1, threat / maxThreat);
            case GoalTargetType.ESCAPE:
                return Math.min(1, threat / maxThreat);
            case GoalTargetType.DEFEND:
                return threat / maxThreat;
            default:
                return 0;
        }
    }

    private calculateTerrainFactor(goal: Goal, target: GoalTarget): number {
        if (!target.terrainType) return 0;

        const favorableTerrain = ['road', 'grass', 'flat'];
        const penaltyTerrain = ['water', 'mountain', 'forest'];

        if (favorableTerrain.includes(target.terrainType)) return 1;
        if (penaltyTerrain.includes(target.terrainType)) return 0.5;

        return 0.75;
    }

    private getHighestScoredTarget(): GoalTarget | null {
        let highestScore = -Infinity;
        let highestTarget: GoalTarget | null = null;

        for (const [key, score] of this.scores.entries()) {
            if (score > highestScore) {
                highestScore = score;
                const parts = key.split('_');
                const targetId = parts.slice(1).join('_');
                highestTarget = this.findTargetById(targetId);
            }
        }

        return highestTarget;
    }

    private findTargetById(targetId: string): GoalTarget | null {
        for (const goal of this.goals.values()) {
            for (const target of goal.potentialTargets) {
                if (target.id === targetId) return target;
            }
        }
        return null;
    }

    public getGoal(goalId: string): Goal | undefined {
        return this.goals.get(goalId);
    }

    public getAllGoals(): Goal[] {
        return Array.from(this.goals.values());
    }

    public clearGoals(): void {
        this.goals.clear();
        this.scores.clear();
    }

    public setEvaluationInterval(intervalMs: number): void {
        this.evaluationInterval = intervalMs;
    }

    public getDebugInfo(): Map<string, any> {
        const info = new Map<string, any>();

        info.set('goalCount', this.goals.size);
        info.set('lastEvaluation', this.lastEvaluation);
        info.set('evaluationInterval', this.evaluationInterval);

        const scoreSummary = new Map<string, number>();
        for (const [key, score] of this.scores.entries()) {
            const parts = key.split('_');
            const goalId = parts[0];
            const currentMax = scoreSummary.get(goalId) || 0;
            if (score > currentMax) {
                scoreSummary.set(goalId, score);
            }
        }

        info.set('goalScores', scoreSummary);

        return info;
    }
}

export interface Goal {
    id: string;
    type: GoalTargetType;
    potentialTargets: GoalTarget[];
    currentPosition: Vector3 | null;
    distanceWeight: number;
    priorityWeight: number;
    resourceWeight: number;
    threatWeight: number;
    terrainBonus: number;
    randomness: number;
}

export interface GoalTarget {
    id: string;
    type: GoalTargetType;
    position: Vector3;
    priority: number;
    resources?: number;
    threatLevel?: number;
    terrainType?: string;
    unitCount?: number;
}

export class PathfindingSystem implements IUpdatable {
    private pathfinders: Map<PathfindingAlgorithm, PathFinder>;
    private currentAlgorithm: PathfindingAlgorithm;
    private gridSize: Vector3;
    private worldSize: Vector3;
    private pathCache: PathCache;
    private pathSmoother: PathSmoother;
    private movementCost: MovementCost;
    private obstacleAvoidance: ObstacleAvoidance;
    private pathFollowers: Map<string, PathFollower>;
    private formationMovement: FormationMovement;
    private goalEvaluator: GoalEvaluator;
    private collisionDetector: CollisionDetector;

    private pathRequestQueue: PathRequest[];
    private activeRequests: Map<string, PathRequest>;
    private completedRequests: PathResult[];

    private dynamicObstacles: Map<string, DynamicObstacle>;
    private unitMovementData: Map<string, UnitMovementData>;

    private stats: PathfindingStats;
    private updateOrder: number = 100;
    private enabled: boolean = true;
    private paused: boolean = false;

    private lastUpdateTime: number;
    private fixedDeltaTime: number = 0.1;

    constructor(gridSize: Vector3, worldSize: Vector3) {
        this.gridSize = gridSize;
        this.worldSize = worldSize;

        this.pathfinders = new Map();
        this.currentAlgorithm = PathfindingAlgorithm.A_STAR;

        this.pathCache = new PathCache(1000, 10 * 1024 * 1024);
        this.pathSmoother = new PathSmoother();
        this.movementCost = new MovementCost();
        this.obstacleAvoidance = new ObstacleAvoidance();
        this.formationMovement = new FormationMovement(2);
        this.goalEvaluator = new GoalEvaluator();
        this.collisionDetector = new CollisionDetector();

        this.pathRequestQueue = [];
        this.activeRequests = new Map();
        this.completedRequests = [];

        this.dynamicObstacles = new Map();
        this.unitMovementData = new Map();
        this.pathFollowers = new Map();

        this.stats = {
            totalRequests: 0,
            successfulRequests: 0,
            failedRequests: 0,
            averageIterations: 0,
            averageTimeMs: 0,
            cacheHitRate: 0,
            memoryUsageBytes: 0,
            activeUnits: 0,
            activePathfinders: this.pathfinders.size
        };

        this.lastUpdateTime = performance.now();

        this.initializePathfinders();
    }

    private initializePathfinders(): void {
        const aStar = new PathFinderAStar(this.gridSize, this.worldSize);
        this.pathfinders.set(PathfindingAlgorithm.A_STAR, aStar);

        const jumpPoint = new PathFinderJumpPointSearch(this.gridSize, this.worldSize);
        this.pathfinders.set(PathfindingAlgorithm.JUMP_POINT_SEARCH, jumpPoint);

        const flowField = new PathFinderFlowField(this.gridSize, this.worldSize);
        this.pathfinders.set(PathfindingAlgorithm.FLOW_FIELD, flowField);

        const navMesh = new PathFinderNavMesh(this.worldSize);
        this.pathfinders.set(PathfindingAlgorithm.A_STAR, navMesh);

        this.stats.activePathfinders = this.pathfinders.size;
    }

    public update(deltaTime: number): void {
        if (!this.enabled || this.paused) return;

        this.processPathRequests();
        this.updateDynamicObstacles();
        this.updateGoalEvaluation();
        this.cleanupCompletedRequests();

        const now = performance.now();
        const elapsed = (now - this.lastUpdateTime) / 1000;
        if (elapsed >= this.fixedDeltaTime) {
            this.fixedUpdate(elapsed);
            this.lastUpdateTime = now;
        }

        this.updateStats();
    }

    public fixedUpdate(deltaTime: number): void {
        this.recalculatePathsIfNeeded(deltaTime);
        this.updatePathFollowers(deltaTime);
    }

    private processPathRequests(): void {
        while (this.pathRequestQueue.length > 0) {
            const request = this.pathRequestQueue.shift();
            if (!request) break;

            const cachedResult = this.pathCache.get(request.start, request.end, this.gridSize.x);
            if (cachedResult && !request.constraints) {
                request.callback(cachedResult);
                this.stats.successfulRequests++;
                continue;
            }

            this.activeRequests.set(request.id, request);
            this.stats.totalRequests++;

            const pathfinder = this.pathfinders.get(request.algorithm) || this.pathfinders.get(this.currentAlgorithm);
            if (!pathfinder) {
                const failureResult: PathResult = {
                    requestId: request.id,
                    status: PathStatus.FAILURE,
                    path: [],
                    cost: Infinity,
                    length: 0,
                    iterations: 0,
                    timeMs: 0,
                    message: 'No pathfinder available'
                };
                this.completeRequest(request, failureResult);
                continue;
            }

            const result = pathfinder.findPath(request.start, request.end, request);

            if (result.status === PathStatus.SUCCESS && result.path.length > 0) {
                const smoothedPath = this.pathSmoother.smooth(result.path);
                result.smoothedPath = smoothedPath;

                this.pathCache.set(request.start, request.end, this.gridSize.x, result);
            }

            this.completeRequest(request, result);
        }
    }

    private completeRequest(request: PathRequest, result: PathResult): void {
        this.activeRequests.delete(request.id);
        this.completedRequests.push(result);

        if (result.status === PathStatus.SUCCESS) {
            this.stats.successfulRequests++;
        } else {
            this.stats.failedRequests++;
        }

        request.callback(result);

        if (this.completedRequests.length > 1000) {
            this.completedRequests = this.completedRequests.slice(-500);
        }
    }

    public requestPath(request: PathRequest): void {
        if (request.priority < 0) {
            this.pathRequestQueue.unshift(request);
        } else {
            const insertIndex = this.pathRequestQueue.findIndex(r => r.priority < request.priority);
            if (insertIndex === -1) {
                this.pathRequestQueue.push(request);
            } else {
                this.pathRequestQueue.splice(insertIndex, 0, request);
            }
        }
    }

    public findPathImmediate(start: Vector3, end: Vector3, algorithm?: PathfindingAlgorithm): PathResult {
        const pathfinder = this.pathfinders.get(algorithm || this.currentAlgorithm);
        if (!pathfinder) {
            return {
                requestId: '',
                status: PathStatus.FAILURE,
                path: [],
                cost: Infinity,
                length: 0,
                iterations: 0,
                timeMs: 0,
                message: 'No pathfinder available'
            };
        }

        return pathfinder.findPath(start, end);
    }

    public registerUnit(unitId: string, position: Vector3, speed: number): void {
        if (this.unitMovementData.has(unitId)) return;

        const data: UnitMovementData = {
            unitId,
            position: position.clone(),
            velocity: Vector3.ZERO.clone(),
            targetPosition: Vector3.ZERO.clone(),
            currentPath: [],
            currentWaypointIndex: 0,
            state: MovementState.IDLE,
            speed,
            maxSpeed: speed,
            acceleration: speed * 5,
            steeringBehaviors: [SteeringBehavior.SEEK],
            lastPathUpdate: 0
        };

        this.unitMovementData.set(unitId, data);

        const follower = new PathFollower(
            this.gridSize,
            this.worldSize,
            this.obstacleAvoidance,
            this.formationMovement
        );
        follower.registerUnit(unitId, position, speed);
        this.pathFollowers.set(unitId, follower);

        this.stats.activeUnits = this.unitMovementData.size;
    }

    public unregisterUnit(unitId: string): void {
        this.unitMovementData.delete(unitId);
        this.pathFollowers.delete(unitId);
        this.stats.activeUnits = this.unitMovementData.size;
    }

    public setUnitPath(unitId: string, path: Vector3[]): boolean {
        const data = this.unitMovementData.get(unitId);
        if (!data) return false;

        data.currentPath = path;
        data.currentWaypointIndex = 0;
        data.state = path.length > 0 ? MovementState.MOVING : MovementState.IDLE;
        data.lastPathUpdate = performance.now();

        const follower = this.pathFollowers.get(unitId);
        if (follower) {
            return follower.setPath(unitId, path);
        }

        return false;
    }

    public updateUnit(unitId: string, deltaTime: number): Vector3 | null {
        const data = this.unitMovementData.get(unitId);
        if (!data) return null;

        const nearbyUnits = new Map<string, Vector3>();
        for (const [otherId, otherData] of this.unitMovementData) {
            if (otherId !== unitId) {
                nearbyUnits.set(otherId, otherData.position);
            }
        }

        const follower = this.pathFollowers.get(unitId);
        if (!follower) return null;

        const newPosition = follower.update(unitId, deltaTime, nearbyUnits);
        data.position = newPosition.clone();

        if (follower.hasReachedDestination(unitId)) {
            data.state = MovementState.ARRIVED;
        }

        return newPosition;
    }

    private updatePathFollowers(deltaTime: number): void {
        for (const [unitId, follower] of this.pathFollowers) {
            const nearbyUnits = new Map<string, Vector3>();
            for (const [otherId, otherData] of this.unitMovementData) {
                if (otherId !== unitId) {
                    nearbyUnits.set(otherId, otherData.position);
                }
            }

            follower.update(unitId, deltaTime, nearbyUnits);
        }
    }

    private updateDynamicObstacles(): void {
        const now = performance.now();

        for (const [id, obstacle] of this.dynamicObstacles) {
            if (now - obstacle.lastUpdate > obstacle.updateInterval) {
                obstacle.predictedPosition = Vector3.add(
                    obstacle.position,
                    obstacle.velocity.clone().multiplyScalar(obstacle.updateInterval)
                );
                obstacle.lastUpdate = now;
            }
        }
    }

    private recalculatePathsIfNeeded(deltaTime: number): void {
        const now = performance.now();
        const pathRecalculateInterval = 1000;

        for (const [unitId, data] of this.unitMovementData) {
            if (data.state !== MovementState.MOVING) continue;

            if (now - data.lastPathUpdate > pathRecalculateInterval) {
                let blocked = false;
                const currentWaypoint = data.currentPath[data.currentWaypointIndex];

                if (currentWaypoint) {
                    for (const obstacle of this.dynamicObstacles.values()) {
                        if (obstacle.position.distanceTo(currentWaypoint) < obstacle.radius + 2) {
                            blocked = true;
                            break;
                        }
                    }
                }

                if (blocked) {
                    data.state = MovementState.PATHING;
                    this.requestPath({
                        id: `${unitId}_recalc_${now}`,
                        start: data.position.clone(),
                        end: data.currentPath[data.currentPath.length - 1] || data.targetPosition.clone(),
                        unitId,
                        algorithm: this.currentAlgorithm,
                        allowPartialPath: true,
                        priority: 1,
                        callback: (result) => {
                            if (result.status === PathStatus.SUCCESS || result.status === PathStatus.PARTIAL) {
                                this.setUnitPath(unitId, result.path);
                                data.state = MovementState.MOVING;
                            } else {
                                data.state = MovementState.BLOCKED;
                            }
                        }
                    });
                }

                data.lastPathUpdate = now;
            }
        }
    }

    private updateGoalEvaluation(): void {
        const targets: GoalTarget[] = [];

        for (const [unitId, data] of this.unitMovementData) {
            const goalTarget: GoalTarget = {
                id: unitId,
                type: GoalTargetType.PATROL,
                position: data.position.clone(),
                priority: 50
            };
            targets.push(goalTarget);
        }

        for (const obstacle of this.dynamicObstacles.values()) {
            const goalTarget: GoalTarget = {
                id: obstacle.id,
                type: GoalTargetType.ATTACK,
                position: obstacle.position.clone(),
                priority: obstacle.priority,
                threatLevel: 30
            };
            targets.push(goalTarget);
        }

        this.goalEvaluator.evaluate(targets);
    }

    private cleanupCompletedRequests(): void {
        const maxAge = 60000;
        const now = performance.now();

        this.completedRequests = this.completedRequests.filter(
            result => now - (result.timeMs || 0) < maxAge
        );
    }

    private updateStats(): void {
        if (this.completedRequests.length > 0) {
            let totalIterations = 0;
            let totalTime = 0;

            for (const result of this.completedRequests) {
                totalIterations += result.iterations;
                totalTime += result.timeMs;
            }

            this.stats.averageIterations = totalIterations / this.completedRequests.length;
            this.stats.averageTimeMs = totalTime / this.completedRequests.length;
        }

        this.stats.cacheHitRate = this.pathCache.getHitRate();
        this.stats.memoryUsageBytes = this.pathCache.getStats().memoryUsage;
        this.stats.activeUnits = this.unitMovementData.size;
        this.stats.activePathfinders = this.pathfinders.size;
    }

    public addDynamicObstacle(obstacle: DynamicObstacle): void {
        this.dynamicObstacles.set(obstacle.id, obstacle);

        for (const pathfinder of this.pathfinders.values()) {
            pathfinder.updateObstacles([obstacle.position]);
        }

        this.pathCache.invalidate(obstacle.position, obstacle.radius * 3, this.gridSize.x);
    }

    public removeDynamicObstacle(obstacleId: string): void {
        this.dynamicObstacles.delete(obstacleId);
    }

    public updateDynamicObstacle(obstacleId: string, position: Vector3, velocity: Vector3): void {
        const obstacle = this.dynamicObstacles.get(obstacleId);
        if (obstacle) {
            const oldPosition = obstacle.position.clone();
            obstacle.position = position.clone();
            obstacle.velocity = velocity.clone();

            this.pathCache.invalidate(oldPosition, obstacle.radius * 3, this.gridSize.x);
            this.pathCache.invalidate(position, obstacle.radius * 3, this.gridSize.x);
        }
    }

    public setGrid(x: number, z: number, walkable: boolean): void {
        for (const pathfinder of this.pathfinders.values()) {
            if (pathfinder instanceof PathFinderAStar || pathfinder instanceof PathFinderJumpPointSearch) {
                pathfinder.setGrid(this.createGridColumn(x, z, walkable));
            }
        }
    }

    private createGridColumn(x: number, z: number, walkable: boolean): boolean[][] {
        const grid: boolean[][] = [];
        for (let i = 0; i <= x; i++) {
            grid[i] = [];
            for (let j = 0; j <= z; j++) {
                grid[i][j] = !(i === x && j === z) || walkable;
            }
        }
        return grid;
    }

    public setCurrentAlgorithm(algorithm: PathfindingAlgorithm): void {
        this.currentAlgorithm = algorithm;
    }

    public getCurrentAlgorithm(): PathfindingAlgorithm {
        return this.currentAlgorithm;
    }

    public getPathfinder(algorithm: PathfindingAlgorithm): PathFinder | undefined {
        return this.pathfinders.get(algorithm);
    }

    public getStats(): PathfindingStats {
        return { ...this.stats };
    }

    public getPathCacheStats(): { size: number; maxSize: number; memoryUsage: number; maxMemory: number; hitRate: number } {
        return this.pathCache.getStats();
    }

    public setPathSmoothing(tolerance: number, cornerFactor: number): void {
        this.pathSmoother.setSimplificationTolerance(tolerance);
        this.pathSmoother.setCornerSmoothingFactor(cornerFactor);
    }

    public setMovementCost(terrainType: string, cost: number): void {
        this.movementCost.setDefaultCost(terrainType, cost);
    }

    public setAvoidanceQuality(quality: AvoidanceQuality): void {
        this.obstacleAvoidance.setAvoidanceQuality(quality);
    }

    public setFormationType(type: FormationType): void {
        this.formationMovement.setFormationType(type);
    }

    public calculateFormation(
        units: Array<{ id: string; position: Vector3 }>,
        target: Vector3
    ): Map<string, Vector3> {
        return this.formationMovement.calculate(units, target);
    }

    public addGoal(goal: Goal): void {
        this.goalEvaluator.addGoal(goal);
    }

    public removeGoal(goalId: string): void {
        this.goalEvaluator.removeGoal(goalId);
    }

    public evaluateGoals(targets: GoalTarget[]): GoalTarget | null {
        return this.goalEvaluator.evaluate(targets);
    }

    public clearCache(): void {
        this.pathCache.clear();
    }

    public invalidateCache(position: Vector3, radius: number): void {
        this.pathCache.invalidate(position, radius, this.gridSize.x);
    }

    public reset(): void {
        this.pathRequestQueue = [];
        this.activeRequests.clear();
        this.completedRequests = [];
        this.unitMovementData.clear();
        this.pathFollowers.clear();
        this.dynamicObstacles.clear();
        this.pathCache.clear();

        this.stats = {
            totalRequests: 0,
            successfulRequests: 0,
            failedRequests: 0,
            averageIterations: 0,
            averageTimeMs: 0,
            cacheHitRate: 0,
            memoryUsageBytes: 0,
            activeUnits: 0,
            activePathfinders: this.pathfinders.size
        };
    }

    public pause(): void {
        this.paused = true;
    }

    public resume(): void {
        this.paused = false;
    }

    public isPaused(): boolean {
        return this.paused;
    }

    public enable(): void {
        this.enabled = true;
    }

    public disable(): void {
        this.enabled = false;
    }

    public isEnabled(): boolean {
        return this.enabled;
    }

    public getUpdateOrder(): number {
        return this.updateOrder;
    }

    public setUpdateOrder(order: number): void {
        this.updateOrder = order;
    }

    public getUnitMovementData(unitId: string): UnitMovementData | undefined {
        return this.unitMovementData.get(unitId);
    }

    public getAllUnitMovementData(): Map<string, UnitMovementData> {
        return new Map(this.unitMovementData);
    }

    public getUnitCount(): number {
        return this.unitMovementData.size;
    }

    public getObstacleCount(): number {
        return this.dynamicObstacles.size;
    }

    public getPendingRequestCount(): number {
        return this.pathRequestQueue.length + this.activeRequests.size;
    }

    public getDebugInfo(): {
        stats: PathfindingStats;
        cacheStats: any;
        unitCount: number;
        obstacleCount: number;
        pendingRequests: number;
        algorithm: PathfindingAlgorithm;
    } {
        return {
            stats: this.getStats(),
            cacheStats: this.getPathCacheStats(),
            unitCount: this.getUnitCount(),
            obstacleCount: this.getObstacleCount(),
            pendingRequests: this.getPendingRequestCount(),
            algorithm: this.currentAlgorithm
        };
    }

    public raycast(origin: Vector3, direction: Vector3, maxDistance: number): RaycastResult {
        return this.collisionDetector.raycast(origin, direction, maxDistance);
    }

    public addCollisionObstacle(id: string, position: Vector3, radius: number): void {
        this.collisionDetector.addObstacle(id, position, radius);
    }

    public removeCollisionObstacle(id: string): void {
        this.collisionDetector.removeObstacle(id);
    }
}

export default PathfindingSystem;

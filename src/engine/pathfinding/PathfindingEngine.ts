
import { Entity, Position } from '../../entities/Entity';

export interface PathNode {
  x: number;
  y: number;
  g: number;
  h: number;
  f: number;
  parent: PathNode | null;
  walkable: boolean;
  cost: number;
}

export interface PathResult {
  path: Position[];
  success: boolean;
  length: number;
  time: number;
}

export interface PathfindingOptions {
  allowDiagonal: boolean;
  maxIterations: number;
  heuristic: 'manhattan' | 'euclidean' | 'octile' | 'chebyshev';
  weight: number;
  tieBreaker: boolean;
}

const DEFAULT_OPTIONS: PathfindingOptions = {
  allowDiagonal: true,
  maxIterations: 10000,
  heuristic: 'octile',
  weight: 1.0,
  tieBreaker: true,
};

export class PathfindingEngine {
  private grid: PathNode[][] = [];
  private width: number = 0;
  private height: number = 0;
  private openList: PathNode[] = [];
  private closedSet: Set<string> = new Set();
  private nodeMap: Map<string, PathNode> = new Map();
  private iterations: number = 0;
  private options: PathfindingOptions;

  constructor(options: Partial<PathfindingOptions> = {}) {
    this.options = { ...DEFAULT_OPTIONS, ...options };
  }

  public initializeGrid(width: number, height: number, walkableCallback: (x: number, y: number) => boolean): void {
    this.width = width;
    this.height = height;
    this.grid = [];
    this.nodeMap.clear();

    for (let x = 0; x < width; x++) {
      this.grid[x] = [];
      for (let y = 0; y < height; y++) {
        const node: PathNode = {
          x,
          y,
          g: Infinity,
          h: 0,
          f: Infinity,
          parent: null,
          walkable: walkableCallback(x, y),
          cost: walkableCallback(x, y) ? 1.0 : 0.0,
        };
        this.grid[x][y] = node;
        this.nodeMap.set(`${x},${y}`, node);
      }
    }
  }

  public updateNode(x: number, y: number, walkable: boolean, cost: number = 1.0): void {
    if (x >= 0 && x < this.width && y >= 0 && y < this.height) {
      const node = this.grid[x][y];
      node.walkable = walkable;
      node.cost = walkable ? cost : 0.0;
    }
  }

  public findPath(start: Position, end: Position): PathResult {
    const startTime = performance.now();
    this.iterations = 0;
    this.openList = [];
    this.closedSet.clear();

    const startNode = this.getNode(start.x, start.y);
    const endNode = this.getNode(end.x, end.y);

    if (!startNode || !endNode) {
      return { path: [], success: false, length: 0, time: 0 };
    }

    if (!startNode.walkable || !endNode.walkable) {
      return { path: [], success: false, length: 0, time: 0 };
    }

    startNode.g = 0;
    startNode.h = this.calculateHeuristic(startNode, endNode);
    startNode.f = startNode.g + startNode.h * this.options.weight;
    startNode.parent = null;

    this.openList.push(startNode);

    while (this.openList.length > 0 && this.iterations < this.options.maxIterations) {
      this.iterations++;

      const currentNode = this.getLowestFNode();
      if (!currentNode) break;

      if (currentNode.x === endNode.x && currentNode.y === endNode.y) {
        const path = this.reconstructPath(currentNode);
        return {
          path,
          success: true,
          length: path.length,
          time: performance.now() - startTime,
        };
      }

      this.openList = this.openList.filter(n => n !== currentNode);
      this.closedSet.add(`${currentNode.x},${currentNode.y}`);

      const neighbors = this.getNeighbors(currentNode);
      for (const neighbor of neighbors) {
        if (this.closedSet.has(`${neighbor.x},${neighbor.y}`) || !neighbor.walkable) {
          continue;
        }

        const tentativeG = currentNode.g + this.getMoveCost(currentNode, neighbor);

        const inOpenList = this.openList.includes(neighbor);
        if (!inOpenList || tentativeG < neighbor.g) {
          neighbor.parent = currentNode;
          neighbor.g = tentativeG;
          neighbor.h = this.calculateHeuristic(neighbor, endNode);
          neighbor.f = neighbor.g + neighbor.h * this.options.weight;

          if (!inOpenList) {
            this.openList.push(neighbor);
          }
        }
      }
    }

    return {
      path: [],
      success: false,
      length: 0,
      time: performance.now() - startTime,
    };
  }

  private getNode(x: number, y: number): PathNode | null {
    if (x >= 0 && x < this.width && y >= 0 && y < this.height) {
      return this.grid[x][y];
    }
    return null;
  }

  private getLowestFNode(): PathNode | null {
    if (this.openList.length === 0) return null;
    
    let lowest = this.openList[0];
    for (const node of this.openList) {
      if (node.f < lowest.f) {
        lowest = node;
      }
    }
    return lowest;
  }

  private calculateHeuristic(node: PathNode, endNode: PathNode): number {
    const dx = Math.abs(node.x - endNode.x);
    const dy = Math.abs(node.y - endNode.y);

    switch (this.options.heuristic) {
      case 'manhattan':
        return dx + dy;
      case 'euclidean':
        return Math.sqrt(dx * dx + dy * dy);
      case 'octile':
        return dx + dy - 0.586 * Math.min(dx, dy);
      case 'chebyshev':
        return Math.max(dx, dy);
      default:
        return dx + dy;
    }
  }

  private getNeighbors(node: PathNode): PathNode[] {
    const neighbors: PathNode[] = [];
    const directions = this.options.allowDiagonal
      ? [
          [-1, -1], [0, -1], [1, -1],
          [-1, 0],          [1, 0],
          [-1, 1],  [0, 1],  [1, 1],
        ]
      : [
          [0, -1],
          [-1, 0], [1, 0],
          [0, 1],
        ];

    for (const [dx, dy] of directions) {
      const nx = node.x + dx;
      const ny = node.y + dy;
      const neighbor = this.getNode(nx, ny);
      if (neighbor) {
        neighbors.push(neighbor);
      }
    }

    return neighbors;
  }

  private getMoveCost(from: PathNode, to: PathNode): number {
    const dx = Math.abs(from.x - to.x);
    const dy = Math.abs(from.y - to.y);

    if (dx + dy > 1) {
      return 1.414 * to.cost;
    }
    return to.cost;
  }

  private reconstructPath(node: PathNode): Position[] {
    const path: Position[] = [];
    let current: PathNode | null = node;

    while (current) {
      path.unshift({ x: current.x, y: current.y });
      current = current.parent;
    }

    return path;
  }

  public getIterations(): number {
    return this.iterations;
  }

  public setOption<K extends keyof PathfindingOptions>(key: K, value: PathfindingOptions[K]): void {
    this.options[key] = value;
  }

  public getOption<K extends keyof PathfindingOptions>(key: K): PathfindingOptions[K] {
    return this.options[key];
  }
}

export class FlowField {
  private grid: Map<string, Position> = new Map();
  private width: number;
  private height: number;

  constructor(width: number, height: number) {
    this.width = width;
    this.height = height;
  }

  public generate(target: Position, walkableCallback: (x: number, y: number) => boolean): void {
    this.grid.clear();
    const queue: Position[] = [target];
    const visited: Set<string> = new Set();
    visited.add(`${target.x},${target.y}`);
    this.grid.set(`${target.x},${target.y}`, target);

    const directions = [
      [-1, 0], [1, 0], [0, -1], [0, 1],
      [-1, -1], [1, -1], [-1, 1], [1, 1],
    ];

    while (queue.length > 0) {
      const current = queue.shift()!;

      for (const [dx, dy] of directions) {
        const nx = current.x + dx;
        const ny = current.y + dy;
        const key = `${nx},${ny}`;

        if (nx < 0 || nx >= this.width || ny < 0 || ny >= this.height) continue;
        if (visited.has(key)) continue;
        if (!walkableCallback(nx, ny)) continue;

        visited.add(key);
        this.grid.set(key, { x: dx, y: dy });
        queue.push({ x: nx, y: ny });
      }
    }
  }

  public getDirection(x: number, y: number): Position | null {
    return this.grid.get(`${x},${y}`) || null;
  }

  public hasDirection(x: number, y: number): boolean {
    return this.grid.has(`${x},${y}`);
  }
}

export class HierarchicalPathfinder {
  private clusterSize: number = 8;
  private clusters: Map<string, PathfindingEngine> = new Map();
  private clusterGraph: Map<string, Map<string, { path: Position[]; cost: number }>> = new Map();
  private mainEngine: PathfindingEngine;

  constructor() {
    this.mainEngine = new PathfindingEngine();
  }

  public initialize(
    width: number,
    height: number,
    walkableCallback: (x: number, y: number) => boolean
  ): void {
    this.mainEngine.initializeGrid(width, height, walkableCallback);
    this.buildClusterGraph(width, height, walkableCallback);
  }

  private buildClusterGraph(
    width: number,
    height: number,
    walkableCallback: (x: number, y: number) => boolean
  ): void {
    const clusterWidth = Math.ceil(width / this.clusterSize);
    const clusterHeight = Math.ceil(height / this.clusterSize);

    for (let cx = 0; cx < clusterWidth; cx++) {
      for (let cy = 0; cy < clusterHeight; cy++) {
        const clusterKey = `${cx},${cy}`;
        const cluster = new PathfindingEngine();
        cluster.initializeGrid(
          this.clusterSize,
          this.clusterSize,
          (x, y) => {
            const worldX = cx * this.clusterSize + x;
            const worldY = cy * this.clusterSize + y;
            return walkableCallback(worldX, worldY);
          }
        );
        this.clusters.set(clusterKey, cluster);
      }
    }
  }

  public findPath(start: Position, end: Position): PathResult {
    return this.mainEngine.findPath(start, end);
  }

  public smoothPath(path: Position[]): Position[] {
    if (path.length <= 2) return path;

    const smoothed: Position[] = [path[0]];
    let current = 0;

    while (current < path.length - 1) {
      let furthest = current + 1;
      
      for (let i = path.length - 1; i > current + 1; i--) {
        if (this.hasLineOfSight(path[current], path[i])) {
          furthest = i;
          break;
        }
      }

      smoothed.push(path[furthest]);
      current = furthest;
    }

    return smoothed;
  }

  private hasLineOfSight(from: Position, to: Position): boolean {
    const dx = to.x - from.x;
    const dy = to.y - from.y;
    const steps = Math.max(Math.abs(dx), Math.abs(dy));

    for (let i = 0; i <= steps; i++) {
      const t = i / steps;
      const x = Math.round(from.x + dx * t);
      const y = Math.round(from.y + dy * t);
      
      const node = this.mainEngine['getNode'](x, y);
      if (node && !node.walkable) {
        return false;
      }
    }

    return true;
  }
}

export class JumpPointSearch {
  private grid: PathNode[][] = [];
  private width: number = 0;
  private height: number = 0;

  public initializeGrid(width: number, height: number, walkableCallback: (x: number, y: number) => boolean): void {
    this.width = width;
    this.height = height;
    this.grid = [];

    for (let x = 0; x < width; x++) {
      this.grid[x] = [];
      for (let y = 0; y < height; y++) {
        this.grid[x][y] = {
          x,
          y,
          g: Infinity,
          h: 0,
          f: Infinity,
          parent: null,
          walkable: walkableCallback(x, y),
          cost: walkableCallback(x, y) ? 1.0 : 0.0,
        };
      }
    }
  }

  public findPath(start: Position, end: Position): PathResult {
    const startTime = performance.now();
    
    const startNode = this.grid[start.x]?.[start.y];
    const endNode = this.grid[end.x]?.[end.y];

    if (!startNode || !endNode || !startNode.walkable || !endNode.walkable) {
      return { path: [], success: false, length: 0, time: 0 };
    }

    const openList: PathNode[] = [startNode];
    const closedSet: Set<string> = new Set();

    startNode.g = 0;
    startNode.h = this.heuristic(startNode, endNode);
    startNode.f = startNode.g + startNode.h;
    startNode.parent = null;

    while (openList.length > 0) {
      openList.sort((a, b) => a.f - b.f);
      const current = openList.shift()!;

      if (current.x === endNode.x && current.y === endNode.y) {
        const path = this.reconstructPath(current);
        return {
          path,
          success: true,
          length: path.length,
          time: performance.now() - startTime,
        };
      }

      closedSet.add(`${current.x},${current.y}`);

      const neighbors = this.getJumpPoints(current, end);
      for (const neighbor of neighbors) {
        if (closedSet.has(`${neighbor.x},${neighbor.y}`)) continue;

        const tentativeG = current.g + this.distance(current, neighbor);

        if (tentativeG < neighbor.g) {
          neighbor.parent = current;
          neighbor.g = tentativeG;
          neighbor.h = this.heuristic(neighbor, endNode);
          neighbor.f = neighbor.g + neighbor.h;

          if (!openList.includes(neighbor)) {
            openList.push(neighbor);
          }
        }
      }
    }

    return { path: [], success: false, length: 0, time: performance.now() - startTime };
  }

  private getJumpPoints(node: PathNode, goal: Position): PathNode[] {
    const points: PathNode[] = [];
    const directions = [
      [-1, 0], [1, 0], [0, -1], [0, 1],
      [-1, -1], [1, -1], [-1, 1], [1, 1],
    ];

    for (const [dx, dy] of directions) {
      const jump = this.jump(node.x, node.y, dx, dy, goal);
      if (jump) {
        points.push(jump);
      }
    }

    return points;
  }

  private jump(x: number, y: number, dx: number, dy: number, goal: Position): PathNode | null {
    const nx = x + dx;
    const ny = y + dy;

    if (nx < 0 || nx >= this.width || ny < 0 || ny >= this.height) {
      return null;
    }

    const node = this.grid[nx][ny];
    if (!node.walkable) {
      return null;
    }

    if (nx === goal.x && ny === goal.y) {
      return node;
    }

    if (dx !== 0 && dy !== 0) {
      if (!this.grid[x + dx]?.[y]?.walkable || !this.grid[x]?.[y + dy]?.walkable) {
        return node;
      }
    }

    return this.jump(nx, ny, dx, dy, goal);
  }

  private heuristic(a: PathNode, b: PathNode): number {
    return Math.abs(a.x - b.x) + Math.abs(a.y - b.y);
  }

  private distance(a: PathNode, b: PathNode): number {
    const dx = Math.abs(a.x - b.x);
    const dy = Math.abs(a.y - b.y);
    return dx + dy === 1 ? 1 : 1.414;
  }

  private reconstructPath(node: PathNode): Position[] {
    const path: Position[] = [];
    let current: PathNode | null = node;

    while (current) {
      path.unshift({ x: current.x, y: current.y });
      current = current.parent;
    }

    return path;
  }
}

export class NavMesh {
  private polygons: Polygon[] = [];
  private graph: Map<number, Set<number>> = new Map();

  public addPolygon(vertices: Position[]): number {
    const polygon: Polygon = {
      id: this.polygons.length,
      vertices,
      center: this.calculateCenter(vertices),
    };
    this.polygons.push(polygon);
    return polygon.id;
  }

  public buildGraph(): void {
    this.graph.clear();

    for (let i = 0; i < this.polygons.length; i++) {
      this.graph.set(i, new Set());
    }

    for (let i = 0; i < this.polygons.length; i++) {
      for (let j = i + 1; j < this.polygons.length; j++) {
        if (this.polygonsShareEdge(this.polygons[i], this.polygons[j])) {
          this.graph.get(i)!.add(j);
          this.graph.get(j)!.add(i);
        }
      }
    }
  }

  private polygonsShareEdge(a: Polygon, b: Polygon): boolean {
    for (let i = 0; i < a.vertices.length; i++) {
      const a1 = a.vertices[i];
      const a2 = a.vertices[(i + 1) % a.vertices.length];

      for (let j = 0; j < b.vertices.length; j++) {
        const b1 = b.vertices[j];
        const b2 = b.vertices[(j + 1) % b.vertices.length];

        if (this.segmentsEqual(a1, a2, b1, b2)) {
          return true;
        }
      }
    }
    return false;
  }

  private segmentsEqual(a1: Position, a2: Position, b1: Position, b2: Position): boolean {
    return (
      (a1.x === b1.x && a1.y === b1.y && a2.x === b2.x && a2.y === b2.y) ||
      (a1.x === b2.x && a1.y === b2.y && a2.x === b1.x && a2.y === b1.y)
    );
  }

  private calculateCenter(vertices: Position[]): Position {
    let x = 0;
    let y = 0;
    for (const v of vertices) {
      x += v.x;
      y += v.y;
    }
    return { x: x / vertices.length, y: y / vertices.length };
  }

  public findPath(start: Position, end: Position): Position[] {
    const startPolygon = this.findPolygonContaining(start);
    const endPolygon = this.findPolygonContaining(end);

    if (!startPolygon || !endPolygon) {
      return [start, end];
    }

    if (startPolygon === endPolygon) {
      return [start, end];
    }

    const path = this.aStarPolygons(startPolygon, endPolygon);
    const fullPath: Position[] = [start];

    for (const polygonId of path) {
      fullPath.push(this.polygons[polygonId].center);
    }

    fullPath.push(end);
    return fullPath;
  }

  private findPolygonContaining(point: Position): number | null {
    for (const polygon of this.polygons) {
      if (this.pointInPolygon(point, polygon.vertices)) {
        return polygon.id;
      }
    }
    return null;
  }

  private pointInPolygon(point: Position, vertices: Position[]): boolean {
    let inside = false;
    for (let i = 0, j = vertices.length - 1; i < vertices.length; j = i++) {
      const xi = vertices[i].x;
      const yi = vertices[i].y;
      const xj = vertices[j].x;
      const yj = vertices[j].y;

      if (
        yi > point.y !== yj > point.y &&
        point.x < ((xj - xi) * (point.y - yi)) / (yj - yi) + xi
      ) {
        inside = !inside;
      }
    }
    return inside;
  }

  private aStarPolygons(start: number, end: number): number[] {
    const openList: { id: number; f: number }[] = [{ id: start, f: 0 }];
    const closedSet: Set<number> = new Set();
    const gScore: Map<number, number> = new Map();
    const cameFrom: Map<number, number> = new Map();

    gScore.set(start, 0);

    while (openList.length > 0) {
      openList.sort((a, b) => a.f - b.f);
      const current = openList.shift()!.id;

      if (current === end) {
        return this.reconstructPath(cameFrom, current);
      }

      closedSet.add(current);

      for (const neighbor of this.graph.get(current) || []) {
        if (closedSet.has(neighbor)) continue;

        const tentativeG = (gScore.get(current) || 0) + this.polygonDistance(this.polygons[current], this.polygons[neighbor]);

        if (!gScore.has(neighbor) || tentativeG < gScore.get(neighbor)!) {
          cameFrom.set(neighbor, current);
          gScore.set(neighbor, tentativeG);
          const f = tentativeG + this.polygonDistance(this.polygons[neighbor], this.polygons[end]);
          openList.push({ id: neighbor, f });
        }
      }
    }

    return [];
  }

  private polygonDistance(a: Polygon, b: Polygon): number {
    const dx = a.center.x - b.center.x;
    const dy = a.center.y - b.center.y;
    return Math.sqrt(dx * dx + dy * dy);
  }

  private reconstructPath(cameFrom: Map<number, number>, current: number): number[] {
    const path: number[] = [current];
    while (cameFrom.has(current)) {
      current = cameFrom.get(current)!;
      path.unshift(current);
    }
    return path;
  }
}

interface Polygon {
  id: number;
  vertices: Position[];
  center: Position;
}

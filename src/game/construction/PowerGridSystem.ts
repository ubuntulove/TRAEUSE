import { Vector3, Vector2 } from 'three';

export enum PowerState {
  Off = 'off',
  Online = 'online',
  Brownout = 'brownout',
  Blackout = 'blackout',
  Surge = 'surge',
  Damaged = 'damaged',
  Constructing = 'constructing',
}

export enum ConnectionType {
  Direct = 'direct',
  Wireless = 'wireless',
  Relay = 'relay',
  Cable = 'cable',
  Grid = 'grid',
}

export enum GridSector {
  Residential = 'residential',
  Industrial = 'industrial',
  Commercial = 'commercial',
  Military = 'military',
  Infrastructure = 'infrastructure',
}

export enum BuildingCategory {
  PowerPlant = 'powerPlant',
  Consumer = 'consumer',
  Storage = 'storage',
  Defense = 'defense',
  Production = 'production',
  Research = 'research',
  Utility = 'utility',
}

export enum ConstructionStatus {
  Pending = 'pending',
  InProgress = 'inProgress',
  Completed = 'completed',
  Cancelled = 'cancelled',
  Failed = 'failed',
}

export enum RepairPriority {
  Critical = 'critical',
  High = 'high',
  Medium = 'medium',
  Low = 'low',
}

export interface PowerNodeData {
  id: string;
  position: Vector3;
  sector: GridSector;
  capacity: number;
  currentLoad: number;
  efficiency: number;
  connections: string[];
  state: PowerState;
}

export interface PowerConnectionData {
  id: string;
  sourceId: string;
  targetId: string;
  type: ConnectionType;
  maxCapacity: number;
  currentFlow: number;
  distance: number;
  lossRate: number;
  active: boolean;
}

export interface PowerNetworkData {
  id: string;
  nodes: Map<string, PowerNodeData>;
  connections: Map<string, PowerConnectionData>;
  totalCapacity: number;
  totalLoad: number;
  surplus: number;
  efficiency: number;
}

export interface PowerProducerData {
  buildingId: string;
  type: string;
  baseOutput: number;
  currentOutput: number;
  maxOutput: number;
  fuelType: string;
  fuelEfficiency: number;
  cooldownTime: number;
  lastUpdate: number;
}

export interface PowerConsumerData {
  buildingId: string;
  type: string;
  baseConsumption: number;
  currentConsumption: number;
  priority: number;
  critical: boolean;
  canBrownout: boolean;
}

export interface PowerStorageData {
  buildingId: string;
  capacity: number;
  currentCharge: number;
  chargeRate: number;
  dischargeRate: number;
  efficiency: number;
  type: string;
}

export interface GhostBuildingData {
  buildingId: string;
  type: string;
  position: Vector3;
  rotation: number;
  valid: boolean;
  errors: string[];
  cost: number;
  powerRequirement: number;
}

export interface BuildQueueItem {
  id: string;
  buildingType: string;
  position: Vector3;
  rotation: number;
  priority: number;
  cost: number;
  progress: number;
  status: ConstructionStatus;
  startTime: number;
  estimatedCompletion: number;
  workerCount: number;
  requiredResources: Map<string, number>;
}

export interface RepairTask {
  id: string;
  buildingId: string;
  damage: number;
  repairCost: number;
  priority: RepairPriority;
  estimatedTime: number;
  assignedWorkers: number;
  status: ConstructionStatus;
}

export interface PowerSurgeData {
  id: string;
  sourceId: string;
  targetIds: string[];
  powerAmount: number;
  duration: number;
  startTime: number;
  active: boolean;
}

export interface ConstructionCost {
  minerals: number;
  energy: number;
  time: number;
  workers: number;
}

export interface PlacementRules {
  minDistance: number;
  maxSlope: number;
  requiredTerrain: string[];
  forbiddenTerrain: string[];
  collisionRadius: number;
  alignmentRequired: boolean;
  gridSnapping: boolean;
}

export interface PowerGridSystemConfig {
  maxNetworkSize: number;
  maxConnectionsPerNode: number;
  powerLossPerDistance: number;
  brownoutThreshold: number;
  blackoutThreshold: number;
  surgeDuration: number;
  defaultRepairRate: number;
  gridUpdateInterval: number;
  visualizationEnabled: boolean;
  debugMode: boolean;
}

export class PowerNode {
  public id: string;
  public position: Vector3;
  public sector: GridSector;
  public capacity: number;
  public currentLoad: number;
  public efficiency: number;
  public connections: Set<string>;
  public state: PowerState;
  public lastUpdate: number;

  constructor(data: Partial<PowerNodeData>) {
    this.id = data.id || crypto.randomUUID();
    this.position = data.position || new Vector3();
    this.sector = data.sector || GridSector.Infrastructure;
    this.capacity = data.capacity || 0;
    this.currentLoad = data.currentLoad || 0;
    this.efficiency = data.efficiency || 1.0;
    this.connections = new Set(data.connections || []);
    this.state = data.state || PowerState.Offline;
    this.lastUpdate = Date.now();
  }

  public addConnection(connectionId: string): void {
    this.connections.add(connectionId);
    this.lastUpdate = Date.now();
  }

  public removeConnection(connectionId: string): void {
    this.connections.delete(connectionId);
    this.lastUpdate = Date.now();
  }

  public getLoadPercentage(): number {
    if (this.capacity === 0) return 0;
    return (this.currentLoad / this.capacity) * 100;
  }

  public isOverloaded(): boolean {
    return this.currentLoad > this.capacity;
  }

  public updateLoad(amount: number): void {
    this.currentLoad = Math.max(0, this.currentLoad + amount);
    this.lastUpdate = Date.now();
  }

  public setState(state: PowerState): void {
    this.state = state;
    this.lastUpdate = Date.now();
  }

  public toData(): PowerNodeData {
    return {
      id: this.id,
      position: this.position,
      sector: this.sector,
      capacity: this.capacity,
      currentLoad: this.currentLoad,
      efficiency: this.efficiency,
      connections: Array.from(this.connections),
      state: this.state,
    };
  }
}

export class PowerConnection {
  public id: string;
  public sourceId: string;
  public targetId: string;
  public type: ConnectionType;
  public maxCapacity: number;
  public currentFlow: number;
  public distance: number;
  public lossRate: number;
  public active: boolean;

  constructor(data: Partial<PowerConnectionData>) {
    this.id = data.id || crypto.randomUUID();
    this.sourceId = data.sourceId || '';
    this.targetId = data.targetId || '';
    this.type = data.type || ConnectionType.Direct;
    this.maxCapacity = data.maxCapacity || 100;
    this.currentFlow = data.currentFlow || 0;
    this.distance = data.distance || 0;
    this.lossRate = data.lossRate || 0.01;
    this.active = data.active !== undefined ? data.active : true;
  }

  public getEffectiveCapacity(): number {
    const distanceLoss = 1 - (this.distance * this.lossRate);
    const typeMultiplier = this.getTypeMultiplier();
    return this.maxCapacity * distanceLoss * typeMultiplier * (this.active ? 1 : 0);
  }

  private getTypeMultiplier(): number {
    switch (this.type) {
      case ConnectionType.Wireless: return 0.7;
      case ConnectionType.Relay: return 0.85;
      case ConnectionType.Cable: return 0.95;
      case ConnectionType.Grid: return 0.9;
      default: return 1.0;
    }
  }

  public transferPower(amount: number): number {
    const effectiveCapacity = this.getEffectiveCapacity();
    const transferAmount = Math.min(amount, effectiveCapacity);
    const loss = transferAmount * this.lossRate * this.distance;
    this.currentFlow = transferAmount - loss;
    return this.currentFlow;
  }

  public setActive(active: boolean): void {
    this.active = active;
  }

  public toData(): PowerConnectionData {
    return {
      id: this.id,
      sourceId: this.sourceId,
      targetId: this.targetId,
      type: this.type,
      maxCapacity: this.maxCapacity,
      currentFlow: this.currentFlow,
      distance: this.distance,
      lossRate: this.lossRate,
      active: this.active,
    };
  }
}

export class PowerNetwork {
  public id: string;
  public nodes: Map<string, PowerNode>;
  public connections: Map<string, PowerConnection>;
  public totalCapacity: number;
  public totalLoad: number;

  constructor(id: string) {
    this.id = id;
    this.nodes = new Map();
    this.connections = new Map();
    this.totalCapacity = 0;
    this.totalLoad = 0;
  }

  public addNode(node: PowerNode): void {
    this.nodes.set(node.id, node);
    this.recalculateMetrics();
  }

  public removeNode(nodeId: string): void {
    this.nodes.delete(nodeId);
    const connectionsToRemove: string[] = [];
    this.connections.forEach((conn, connId) => {
      if (conn.sourceId === nodeId || conn.targetId === nodeId) {
        connectionsToRemove.push(connId);
      }
    });
    connectionsToRemove.forEach(connId => this.connections.delete(connId));
    this.recalculateMetrics();
  }

  public addConnection(connection: PowerConnection): void {
    this.connections.set(connection.id, connection);
    const sourceNode = this.nodes.get(connection.sourceId);
    const targetNode = this.nodes.get(connection.targetId);
    if (sourceNode) sourceNode.addConnection(connection.id);
    if (targetNode) targetNode.addConnection(connection.id);
    this.recalculateMetrics();
  }

  public removeConnection(connectionId: string): void {
    const connection = this.connections.get(connectionId);
    if (connection) {
      const sourceNode = this.nodes.get(connection.sourceId);
      const targetNode = this.nodes.get(connection.targetId);
      if (sourceNode) sourceNode.removeConnection(connectionId);
      if (targetNode) targetNode.removeConnection(connectionId);
    }
    this.connections.delete(connectionId);
    this.recalculateMetrics();
  }

  public recalculateMetrics(): void {
    this.totalCapacity = 0;
    this.totalLoad = 0;
    this.nodes.forEach(node => {
      this.totalCapacity += node.capacity * node.efficiency;
      this.totalLoad += node.currentLoad;
    });
  }

  public getSurplus(): number {
    return this.totalCapacity - this.totalLoad;
  }

  public getEfficiency(): number {
    if (this.totalCapacity === 0) return 0;
    return Math.max(0, Math.min(1, this.getSurplus() / this.totalCapacity));
  }

  public getNodesInSector(sector: GridSector): PowerNode[] {
    const result: PowerNode[] = [];
    this.nodes.forEach(node => {
      if (node.sector === sector) {
        result.push(node);
      }
    });
    return result;
  }

  public getConnectionPath(sourceId: string, targetId: string): string[] | null {
    const visited = new Set<string>();
    const path: string[] = [];
    
    const dfs = (currentId: string, target: string): boolean => {
      if (currentId === target) {
        path.push(currentId);
        return true;
      }
      
      visited.add(currentId);
      path.push(currentId);
      
      const node = this.nodes.get(currentId);
      if (!node) return false;
      
      for (const connId of node.connections) {
        const conn = this.connections.get(connId);
        if (!conn || !conn.active) continue;
        
        const nextId = conn.sourceId === currentId ? conn.targetId : conn.sourceId;
        if (!visited.has(nextId)) {
          if (dfs(nextId, target)) {
            return true;
          }
        }
      }
      
      path.pop();
      return false;
    };
    
    if (dfs(sourceId, targetId)) {
      return path;
    }
    return null;
  }

  public toData(): PowerNetworkData {
    const nodeDataMap = new Map<string, PowerNodeData>();
    const connectionDataMap = new Map<string, PowerConnectionData>();
    
    this.nodes.forEach((node, id) => nodeDataMap.set(id, node.toData()));
    this.connections.forEach((conn, id) => connectionDataMap.set(id, conn.toData()));
    
    return {
      id: this.id,
      nodes: nodeDataMap,
      connections: connectionDataMap,
      totalCapacity: this.totalCapacity,
      totalLoad: this.totalLoad,
      surplus: this.getSurplus(),
      efficiency: this.getEfficiency(),
    };
  }
}

export class PowerGrid {
  public networks: Map<string, PowerNetwork>;
  public gridSectors: Map<GridSector, Set<string>>;
  public lastUpdateTime: number;

  constructor() {
    this.networks = new Map();
    this.gridSectors = new Map();
    this.lastUpdateTime = Date.now();
  }

  public createNetwork(id: string): PowerNetwork {
    const network = new PowerNetwork(id);
    this.networks.set(id, network);
    return network;
  }

  public getNetwork(id: string): PowerNetwork | undefined {
    return this.networks.get(id);
  }

  public mergeNetworks(networkId1: string, networkId2: string): PowerNetwork | null {
    const network1 = this.networks.get(networkId1);
    const network2 = this.networks.get(networkId2);
    
    if (!network1 || !network2) return null;
    
    network2.nodes.forEach((node, nodeId) => {
      network1.addNode(node);
    });
    
    network2.connections.forEach((conn) => {
      network1.addConnection(conn);
    });
    
    this.networks.delete(networkId2);
    return network1;
  }

  public splitNetwork(networkId: string, nodeIds: string[]): PowerNetwork | null {
    const originalNetwork = this.networks.get(networkId);
    if (!originalNetwork) return null;
    
    const newNetwork = this.createNetwork(crypto.randomUUID());
    
    const connectionsToCheck = new Set<string>();
    nodeIds.forEach(nodeId => {
      const node = originalNetwork.nodes.get(nodeId);
      if (node) {
        node.connections.forEach(connId => connectionsToCheck.add(connId));
        originalNetwork.removeNode(nodeId);
        newNetwork.addNode(node);
      }
    });
    
    const connectionsToKeep = new Set<string>();
    connectionsToCheck.forEach(connId => {
      const conn = originalNetwork.connections.get(connId);
      if (conn) {
        const sourceInNew = nodeIds.includes(conn.sourceId);
        const targetInNew = nodeIds.includes(conn.targetId);
        
        if (sourceInNew && targetInNew) {
          originalNetwork.removeConnection(connId);
          newNetwork.addConnection(conn);
        } else if (sourceInNew || targetInNew) {
          connectionsToKeep.add(connId);
        }
      }
    });
    
    return newNetwork;
  }

  public getTotalGridCapacity(): number {
    let total = 0;
    this.networks.forEach(network => {
      total += network.totalCapacity;
    });
    return total;
  }

  public getTotalGridLoad(): number {
    let total = 0;
    this.networks.forEach(network => {
      total += network.totalLoad;
    });
    return total;
  }

  public getTotalGridSurplus(): number {
    return this.getTotalGridCapacity() - this.getTotalGridLoad();
  }

  public getNetworkForNode(nodeId: string): PowerNetwork | undefined {
    for (const network of this.networks.values()) {
      if (network.nodes.has(nodeId)) {
        return network;
      }
    }
    return undefined;
  }

  public updateGrid(deltaTime: number): void {
    this.lastUpdateTime = Date.now();
    
    this.networks.forEach(network => {
      network.recalculateMetrics();
    });
  }

  public optimizeConnections(): void {
    this.networks.forEach(network => {
      const nodesArray = Array.from(network.nodes.values());
      const connectionsToRemove: string[] = [];
      
      network.connections.forEach((conn, connId) => {
        const path = network.getConnectionPath(conn.sourceId, conn.targetId);
        if (path && path.length > 2) {
          const alternativePath = network.getConnectionPath(conn.sourceId, conn.targetId);
          if (alternativePath && alternativePath.length <= path.length) {
            connectionsToRemove.push(connId);
          }
        }
      });
      
      connectionsToRemove.forEach(connId => {
        network.removeConnection(connId);
      });
    });
  }
}

export class PowerProducer {
  public data: PowerProducerData;
  public buildingId: string;
  public currentOutput: number;
  public isOnline: boolean;
  public efficiencyModifiers: Map<string, number>;

  constructor(buildingId: string, data: Partial<PowerProducerData>) {
    this.buildingId = buildingId;
    this.data = {
      buildingId,
      type: data.type || 'generic',
      baseOutput: data.baseOutput || 100,
      currentOutput: data.currentOutput || 0,
      maxOutput: data.maxOutput || 100,
      fuelType: data.fuelType || 'none',
      fuelEfficiency: data.fuelEfficiency || 1.0,
      cooldownTime: data.cooldownTime || 0,
      lastUpdate: data.lastUpdate || Date.now(),
      ...data,
    };
    this.currentOutput = this.data.currentOutput;
    this.isOnline = false;
    this.efficiencyModifiers = new Map();
  }

  public start(): void {
    this.isOnline = true;
    this.data.lastUpdate = Date.now();
  }

  public stop(): void {
    this.isOnline = false;
    this.currentOutput = 0;
  }

  public producePower(deltaTime: number): number {
    if (!this.isOnline) return 0;
    
    let production = this.data.baseOutput * this.data.fuelEfficiency;
    
    this.efficiencyModifiers.forEach(modifier => {
      production *= modifier;
    });
    
    production = Math.min(production, this.data.maxOutput);
    this.currentOutput = production;
    this.data.currentOutput = production;
    this.data.lastUpdate = Date.now();
    
    return production;
  }

  public setEfficiencyModifier(key: string, value: number): void {
    this.efficiencyModifiers.set(key, value);
  }

  public removeEfficiencyModifier(key: string): void {
    this.efficiencyModifiers.delete(key);
  }

  public getEffectiveOutput(): number {
    if (!this.isOnline) return 0;
    
    let output = this.data.baseOutput;
    this.efficiencyModifiers.forEach(modifier => {
      output *= modifier;
    });
    
    return Math.min(output, this.data.maxOutput);
  }

  public canProduce(): boolean {
    return this.isOnline && this.currentOutput < this.data.maxOutput;
  }

  public getEfficiency(): number {
    if (this.data.baseOutput === 0) return 0;
    return this.currentOutput / this.data.baseOutput;
  }

  public toData(): PowerProducerData {
    return { ...this.data };
  }
}

export class PowerConsumer {
  public data: PowerConsumerData;
  public buildingId: string;
  public currentConsumption: number;
  public isPowered: boolean;
  public powerHistory: number[];

  constructor(buildingId: string, data: Partial<PowerConsumerData>) {
    this.buildingId = buildingId;
    this.data = {
      buildingId,
      type: data.type || 'generic',
      baseConsumption: data.baseConsumption || 50,
      currentConsumption: data.currentConsumption || 0,
      priority: data.priority || 5,
      critical: data.critical || false,
      canBrownout: data.canBrownout !== undefined ? data.canBrownout : true,
      ...data,
    };
    this.currentConsumption = this.data.currentConsumption;
    this.isPowered = false;
    this.powerHistory = [];
  }

  public consumePower(amount: number): number {
    const consumed = Math.min(amount, this.data.currentConsumption || this.data.baseConsumption);
    this.currentConsumption = consumed;
    this.isPowered = consumed >= this.data.baseConsumption * 0.5;
    this.powerHistory.push(consumed);
    if (this.powerHistory.length > 100) {
      this.powerHistory.shift();
    }
    return consumed;
  }

  public setConsumption(amount: number): void {
    this.data.currentConsumption = Math.max(0, amount);
  }

  public increasePriority(amount: number): void {
    this.data.priority = Math.min(10, this.data.priority + amount);
  }

  public decreasePriority(amount: number): void {
    this.data.priority = Math.max(1, this.data.priority - amount);
  }

  public setCritical(critical: boolean): void {
    this.data.critical = critical;
  }

  public getAverageConsumption(): number {
    if (this.powerHistory.length === 0) return 0;
    const sum = this.powerHistory.reduce((a, b) => a + b, 0);
    return sum / this.powerHistory.length;
  }

  public getConsumptionVariance(): number {
    if (this.powerHistory.length < 2) return 0;
    const avg = this.getAverageConsumption();
    const squaredDiffs = this.powerHistory.map(val => Math.pow(val - avg, 2));
    return squaredDiffs.reduce((a, b) => a + b, 0) / this.powerHistory.length;
  }

  public isActive(): boolean {
    return this.currentConsumption > 0;
  }

  public toData(): PowerConsumerData {
    return { ...this.data };
  }
}

export class PowerStorage {
  public data: PowerStorageData;
  public buildingId: string;
  public chargeLevel: number;
  public isCharging: boolean;
  public isDischarging: boolean;
  public chargeHistory: number[];

  constructor(buildingId: string, data: Partial<PowerStorageData>) {
    this.buildingId = buildingId;
    this.data = {
      buildingId,
      capacity: data.capacity || 500,
      currentCharge: data.currentCharge || 0,
      chargeRate: data.chargeRate || 50,
      dischargeRate: data.dischargeRate || 50,
      efficiency: data.efficiency || 0.9,
      type: data.type || 'battery',
      ...data,
    };
    this.chargeLevel = this.data.currentCharge;
    this.isCharging = false;
    this.isDischarging = false;
    this.chargeHistory = [];
  }

  public charge(amount: number): number {
    const availableCapacity = this.data.capacity - this.chargeLevel;
    const actualCharge = Math.min(amount, availableCapacity);
    const energyUsed = actualCharge / this.data.efficiency;
    
    this.chargeLevel += actualCharge;
    this.data.currentCharge = this.chargeLevel;
    this.isCharging = actualCharge > 0;
    this.isDischarging = false;
    this.chargeHistory.push(this.chargeLevel);
    
    if (this.chargeHistory.length > 100) {
      this.chargeHistory.shift();
    }
    
    return energyUsed;
  }

  public discharge(amount: number): number {
    const availableDischarge = Math.min(this.chargeLevel, amount);
    const actualDischarge = availableDischarge * this.data.efficiency;
    
    this.chargeLevel -= availableDischarge;
    this.data.currentCharge = this.chargeLevel;
    this.isDischarging = actualDischarge > 0;
    this.isCharging = false;
    this.chargeHistory.push(this.chargeLevel);
    
    if (this.chargeHistory.length > 100) {
      this.chargeHistory.shift();
    }
    
    return actualDischarge;
  }

  public getChargePercentage(): number {
    return (this.chargeLevel / this.data.capacity) * 100;
  }

  public isFull(): boolean {
    return this.chargeLevel >= this.data.capacity;
  }

  public isEmpty(): boolean {
    return this.chargeLevel <= 0;
  }

  public getAvailablePower(): number {
    return this.chargeLevel * this.data.efficiency;
  }

  public canDischarge(amount: number): boolean {
    return this.chargeLevel >= amount / this.data.efficiency;
  }

  public getChargeRate(): number {
    return this.isCharging ? this.data.chargeRate : 0;
  }

  public getDischargeRate(): number {
    return this.isDischarging ? this.data.dischargeRate : 0;
  }

  public reset(): void {
    this.chargeLevel = 0;
    this.data.currentCharge = 0;
    this.isCharging = false;
    this.isDischarging = false;
  }

  public toData(): PowerStorageData {
    return { ...this.data };
  }
}

export class GridConnectionManager {
  private grid: PowerGrid;
  private pendingConnections: Map<string, PowerConnection>;
  private connectionTemplates: Map<ConnectionType, Partial<PowerConnectionData>>;

  constructor(grid: PowerGrid) {
    this.grid = grid;
    this.pendingConnections = new Map();
    this.connectionTemplates = new Map();
    this.initializeTemplates();
  }

  private initializeTemplates(): void {
    this.connectionTemplates.set(ConnectionType.Direct, {
      maxCapacity: 200,
      lossRate: 0.01,
    });
    this.connectionTemplates.set(ConnectionType.Wireless, {
      maxCapacity: 100,
      lossRate: 0.05,
    });
    this.connectionTemplates.set(ConnectionType.Relay, {
      maxCapacity: 150,
      lossRate: 0.02,
    });
    this.connectionTemplates.set(ConnectionType.Cable, {
      maxCapacity: 300,
      lossRate: 0.005,
    });
    this.connectionTemplates.set(ConnectionType.Grid, {
      maxCapacity: 250,
      lossRate: 0.01,
    });
  }

  public createConnection(
    sourceId: string,
    targetId: string,
    type: ConnectionType = ConnectionType.Direct
  ): PowerConnection | null {
    const sourceNetwork = this.grid.getNetworkForNode(sourceId);
    const targetNetwork = this.grid.getNetworkForNode(targetId);
    
    if (!sourceNetwork || !targetNetwork) return null;
    
    const sourceNode = sourceNetwork.nodes.get(sourceId);
    const targetNode = targetNetwork.nodes.get(targetId);
    
    if (!sourceNode || !targetNode) return null;
    
    const distance = sourceNode.position.distanceTo(targetNode.position);
    const template = this.connectionTemplates.get(type) || {};
    
    const connection = new PowerConnection({
      sourceId,
      targetId,
      type,
      distance,
      maxCapacity: template.maxCapacity || 100,
      lossRate: template.lossRate || 0.01,
      active: true,
    });
    
    if (sourceNetwork.id === targetNetwork.id) {
      sourceNetwork.addConnection(connection);
    } else {
      this.pendingConnections.set(connection.id, connection);
    }
    
    return connection;
  }

  public confirmPendingConnection(connectionId: string): boolean {
    const connection = this.pendingConnections.get(connectionId);
    if (!connection) return false;
    
    this.pendingConnections.delete(connectionId);
    return true;
  }

  public cancelPendingConnection(connectionId: string): void {
    this.pendingConnections.delete(connectionId);
  }

  public removeConnection(connectionId: string): boolean {
    let removed = false;
    this.grid.networks.forEach(network => {
      if (network.connections.has(connectionId)) {
        network.removeConnection(connectionId);
        removed = true;
      }
    });
    return removed;
  }

  public autoConnectNodes(nodeIds: string[], type: ConnectionType = ConnectionType.Direct): PowerConnection[] {
    const connections: PowerConnection[] = [];
    
    for (let i = 0; i < nodeIds.length; i++) {
      for (let j = i + 1; j < nodeIds.length; j++) {
        const connection = this.createConnection(nodeIds[i], nodeIds[j], type);
        if (connection) {
          connections.push(connection);
        }
      }
    }
    
    return connections;
  }

  public createRelayChain(nodeIds: string[], type: ConnectionType = ConnectionType.Relay): PowerConnection[] {
    const connections: PowerConnection[] = [];
    
    for (let i = 0; i < nodeIds.length - 1; i++) {
      const connection = this.createConnection(nodeIds[i], nodeIds[i + 1], type);
      if (connection) {
        connections.push(connection);
      }
    }
    
    return connections;
  }

  public findPath(sourceId: string, targetId: string): string[] | null {
    for (const network of this.grid.networks.values()) {
      const path = network.getConnectionPath(sourceId, targetId);
      if (path) return path;
    }
    return null;
  }

  public getConnectionQuality(sourceId: string, targetId: string): number {
    const path = this.findPath(sourceId, targetId);
    if (!path || path.length === 0) return 0;
    
    let totalLoss = 0;
    let totalCapacity = 0;
    
    for (let i = 0; i < path.length - 1; i++) {
      const sourceNode = this.getNodeById(path[i]);
      const targetNode = this.getNodeById(path[i + 1]);
      
      if (!sourceNode || !targetNode) continue;
      
      const connection = this.findConnectionBetween(path[i], path[i + 1]);
      if (connection) {
        totalLoss += connection.lossRate * connection.distance;
        totalCapacity += connection.getEffectiveCapacity();
      }
    }
    
    const pathLoss = 1 - totalLoss;
    const capacityFactor = totalCapacity / (path.length * 200);
    
    return Math.max(0, Math.min(1, pathLoss * capacityFactor));
  }

  private getNodeById(nodeId: string): PowerNode | null {
    for (const network of this.grid.networks.values()) {
      const node = network.nodes.get(nodeId);
      if (node) return node;
    }
    return null;
  }

  private findConnectionBetween(nodeId1: string, nodeId2: string): PowerConnection | null {
    for (const network of this.grid.networks.values()) {
      for (const connection of network.connections.values()) {
        if (
          (connection.sourceId === nodeId1 && connection.targetId === nodeId2) ||
          (connection.sourceId === nodeId2 && connection.targetId === nodeId1)
        ) {
          return connection;
        }
      }
    }
    return null;
  }

  public getPendingConnections(): PowerConnection[] {
    return Array.from(this.pendingConnections.values());
  }
}

export class PowerDistribution {
  private grid: PowerGrid;
  private producers: Map<string, PowerProducer>;
  private consumers: Map<string, PowerConsumer>;
  private storages: Map<string, PowerStorage>;
  private distributionQueue: string[];

  constructor(grid: PowerGrid) {
    this.grid = grid;
    this.producers = new Map();
    this.consumers = new Map();
    this.storages = new Map();
    this.distributionQueue = [];
  }

  public registerProducer(producer: PowerProducer): void {
    this.producers.set(producer.buildingId, producer);
  }

  public unregisterProducer(buildingId: string): void {
    this.producers.delete(buildingId);
  }

  public registerConsumer(consumer: PowerConsumer): void {
    this.consumers.set(consumer.buildingId, consumer);
    this.updateDistributionQueue();
  }

  public unregisterConsumer(buildingId: string): void {
    this.consumers.delete(buildingId);
    this.updateDistributionQueue();
  }

  public registerStorage(storage: PowerStorage): void {
    this.storages.set(storage.buildingId, storage);
  }

  public unregisterStorage(buildingId: string): void {
    this.storages.delete(buildingId);
  }

  private updateDistributionQueue(): void {
    this.distributionQueue = Array.from(this.consumers.values())
      .sort((a, b) => b.data.priority - a.data.priority)
      .map(c => c.buildingId);
  }

  public distribute(deltaTime: number): void {
    let totalProduction = 0;
    
    this.producers.forEach(producer => {
      totalProduction += producer.producePower(deltaTime);
    });
    
    let remainingPower = totalProduction;
    const powerHistory: Map<string, number> = new Map();
    
    this.distributionQueue.forEach(consumerId => {
      const consumer = this.consumers.get(consumerId);
      if (!consumer) return;
      
      const required = consumer.data.baseConsumption;
      const allocated = Math.min(required, remainingPower);
      
      consumer.consumePower(allocated);
      powerHistory.set(consumerId, allocated);
      remainingPower -= allocated;
    });
    
    const totalStorageCapacity = this.getTotalStorageCapacity();
    const surplusPower = remainingPower;
    
    if (surplusPower > 0 && totalStorageCapacity > 0) {
      this.distributeToStorage(surplusPower, deltaTime);
    }
  }

  private distributeToStorage(power: number, deltaTime: number): void {
    let remainingPower = power;
    
    const sortedStorages = Array.from(this.storages.values())
      .filter(s => !s.isFull())
      .sort((a, b) => b.getChargePercentage() - a.getChargePercentage());
    
    for (const storage of sortedStorages) {
      if (remainingPower <= 0) break;
      
      const chargeAmount = Math.min(
        remainingPower,
        storage.data.chargeRate * deltaTime
      );
      
      remainingPower -= storage.charge(chargeAmount);
    }
  }

  public getTotalProduction(): number {
    let total = 0;
    this.producers.forEach(p => {
      total += p.getEffectiveOutput();
    });
    return total;
  }

  public getTotalConsumption(): number {
    let total = 0;
    this.consumers.forEach(c => {
      total += c.data.baseConsumption;
    });
    return total;
  }

  public getTotalStorageCapacity(): number {
    let total = 0;
    this.storages.forEach(s => {
      total += s.data.capacity;
    });
    return total;
  }

  public getTotalStoredPower(): number {
    let total = 0;
    this.storages.forEach(s => {
      total += s.chargeLevel;
    });
    return total;
  }

  public getNetPower(): number {
    return this.getTotalProduction() - this.getTotalConsumption();
  }

  public canMeetDemand(): boolean {
    return this.getNetPower() >= 0;
  }

  public getPowerStatus(): { production: number; consumption: number; surplus: number; deficit: number } {
    const production = this.getTotalProduction();
    const consumption = this.getTotalConsumption();
    const surplus = Math.max(0, production - consumption);
    const deficit = Math.max(0, consumption - production);
    
    return { production, consumption, surplus, deficit };
  }

  public getCriticalConsumers(): PowerConsumer[] {
    const critical: PowerConsumer[] = [];
    this.consumers.forEach(c => {
      if (c.data.critical) {
        critical.push(c);
      }
    });
    return critical;
  }

  public getConsumerById(id: string): PowerConsumer | undefined {
    return this.consumers.get(id);
  }

  public getProducerById(id: string): PowerProducer | undefined {
    return this.producers.get(id);
  }

  public getStorageById(id: string): PowerStorage | undefined {
    return this.storages.get(id);
  }
}

export class BrownoutManager {
  private grid: PowerGrid;
  private distribution: PowerDistribution;
  private brownoutThreshold: number;
  private brownoutConsumers: Set<string>;
  private brownoutHistory: Map<string, number[]>;
  private activeBrownouts: Map<string, number>;

  constructor(grid: PowerGrid, distribution: PowerDistribution, threshold: number = 0.8) {
    this.grid = grid;
    this.distribution = distribution;
    this.brownoutThreshold = threshold;
    this.brownoutConsumers = new Set();
    this.brownoutHistory = new Map();
    this.activeBrownouts = new Map();
  }

  public update(deltaTime: number): void {
    const status = this.distribution.getPowerStatus();
    const loadRatio = status.consumption / Math.max(1, status.production);
    
    if (loadRatio >= this.brownoutThreshold && status.consumption > status.production) {
      this.triggerBrownout();
    } else {
      this.resolveBrownout();
    }
    
    this.updateBrownoutHistory();
  }

  private triggerBrownout(): void {
    const nonCritical = Array.from(this.distribution.consumers.values())
      .filter(c => !c.data.critical && c.data.canBrownout)
      .sort((a, b) => a.data.priority - b.data.priority);
    
    const deficit = this.distribution.getPowerStatus().deficit;
    let remainingDeficit = deficit;
    
    for (const consumer of nonCritical) {
      if (remainingDeficit <= 0) break;
      
      const reduction = consumer.data.baseConsumption * 0.5;
      consumer.setConsumption(reduction);
      this.brownoutConsumers.add(consumer.buildingId);
      this.activeBrownouts.set(consumer.buildingId, Date.now());
      remainingDeficit -= reduction;
    }
  }

  private resolveBrownout(): void {
    this.brownoutConsumers.forEach(consumerId => {
      const consumer = this.distribution.getConsumerById(consumerId);
      if (consumer) {
        consumer.setConsumption(consumer.data.baseConsumption);
      }
    });
    
    this.brownoutConsumers.clear();
    this.activeBrownouts.clear();
  }

  private updateBrownoutHistory(): void {
    this.brownoutConsumers.forEach(consumerId => {
      let history = this.brownoutHistory.get(consumerId);
      if (!history) {
        history = [];
        this.brownoutHistory.set(consumerId, history);
      }
      history.push(Date.now());
      if (history.length > 1000) {
        history.shift();
      }
    });
  }

  public isInBrownout(): boolean {
    return this.brownoutConsumers.size > 0;
  }

  public getBrownoutDuration(consumerId: string): number {
    const startTime = this.activeBrownouts.get(consumerId);
    if (!startTime) return 0;
    return Date.now() - startTime;
  }

  public getBrownoutCount(consumerId: string): number {
    return this.brownoutHistory.get(consumerId)?.length || 0;
  }

  public setThreshold(threshold: number): void {
    this.brownoutThreshold = Math.max(0, Math.min(1, threshold));
  }

  public getAffectedConsumers(): string[] {
    return Array.from(this.brownoutConsumers);
  }

  public forceBrownoutConsumer(consumerId: string): boolean {
    const consumer = this.distribution.getConsumerById(consumerId);
    if (!consumer || consumer.data.critical) return false;
    
    consumer.setConsumption(consumer.data.baseConsumption * 0.5);
    this.brownoutConsumers.add(consumerId);
    this.activeBrownouts.set(consumerId, Date.now());
    return true;
  }
}

export class BlackoutManager {
  private grid: PowerGrid;
  private distribution: PowerDistribution;
  private blackoutNetworks: Set<string>;
  private affectedBuildings: Set<string>;
  private blackoutHistory: Map<string, { start: number; end?: number }>;
  private autoRecoveryEnabled: boolean;
  private recoveryDelay: number;

  constructor(grid: PowerGrid, distribution: PowerDistribution) {
    this.grid = grid;
    this.distribution = distribution;
    this.blackoutNetworks = new Set();
    this.affectedBuildings = new Set();
    this.blackoutHistory = new Map();
    this.autoRecoveryEnabled = true;
    this.recoveryDelay = 5000;
  }

  public update(deltaTime: number): void {
    this.checkForBlackouts();
    
    if (this.autoRecoveryEnabled) {
      this.attemptRecovery();
    }
  }

  private checkForBlackouts(): void {
    const status = this.distribution.getPowerStatus();
    
    if (status.deficit >= status.consumption) {
      this.triggerBlackout();
    }
  }

  private triggerBlackout(): void {
    this.grid.networks.forEach((network, networkId) => {
      if (network.getSurplus() < 0) {
        this.blackoutNetworks.add(networkId);
        
        network.nodes.forEach((node, nodeId) => {
          node.setState(PowerState.Blackout);
        });
        
        this.distribution.consumers.forEach((consumer, buildingId) => {
          consumer.consumePower(0);
          consumer.isPowered = false;
          this.affectedBuildings.add(buildingId);
        });
      }
    });
    
    const now = Date.now();
    this.affectedBuildings.forEach(buildingId => {
      if (!this.blackoutHistory.has(buildingId)) {
        this.blackoutHistory.set(buildingId, { start: now });
      }
    });
  }

  private attemptRecovery(): void {
    const status = this.distribution.getPowerStatus();
    
    if (status.surplus > 0) {
      this.blackoutNetworks.forEach(networkId => {
        const network = this.grid.getNetwork(networkId);
        if (network && network.getSurplus() > 0) {
          this.recoverNetwork(networkId);
        }
      });
    }
  }

  private recoverNetwork(networkId: string): void {
    const network = this.grid.getNetwork(networkId);
    if (!network) return;
    
    network.nodes.forEach(node => {
      node.setState(PowerState.Online);
    });
    
    this.blackoutNetworks.delete(networkId);
    
    const now = Date.now();
    this.blackoutHistory.forEach((record, buildingId) => {
      if (!record.end) {
        record.end = now;
      }
    });
  }

  public forceBlackout(buildingId: string): void {
    const consumer = this.distribution.getConsumerById(buildingId);
    if (consumer) {
      consumer.consumePower(0);
      consumer.isPowered = false;
      this.affectedBuildings.add(buildingId);
      
      if (!this.blackoutHistory.has(buildingId)) {
        this.blackoutHistory.set(buildingId, { start: Date.now() });
      }
    }
  }

  public forceRecovery(buildingId: string): void {
    const consumer = this.distribution.getConsumerById(buildingId);
    if (consumer) {
      consumer.consumePower(consumer.data.baseConsumption);
      consumer.isPowered = true;
      this.affectedBuildings.delete(buildingId);
      
      const record = this.blackoutHistory.get(buildingId);
      if (record && !record.end) {
        record.end = Date.now();
      }
    }
  }

  public isInBlackout(): boolean {
    return this.blackoutNetworks.size > 0 || this.affectedBuildings.size > 0;
  }

  public getBlackoutDuration(buildingId: string): number {
    const record = this.blackoutHistory.get(buildingId);
    if (!record) return 0;
    
    const endTime = record.end || Date.now();
    return endTime - record.start;
  }

  public getAffectedBuildings(): string[] {
    return Array.from(this.affectedBuildings);
  }

  public setAutoRecovery(enabled: boolean): void {
    this.autoRecoveryEnabled = enabled;
  }

  public setRecoveryDelay(delay: number): void {
    this.recoveryDelay = Math.max(0, delay);
  }

  public reset(): void {
    this.blackoutNetworks.clear();
    this.affectedBuildings.clear();
    this.blackoutHistory.clear();
  }
}

export class PowerSurge {
  public data: PowerSurgeData;
  private callback: ((surge: PowerSurge) => void) | null;

  constructor(data: Partial<PowerSurgeData>) {
    this.data = {
      id: data.id || crypto.randomUUID(),
      sourceId: data.sourceId || '',
      targetIds: data.targetIds || [],
      powerAmount: data.powerAmount || 0,
      duration: data.duration || 5000,
      startTime: data.startTime || Date.now(),
      active: data.active !== undefined ? data.active : false,
    };
    this.callback = null;
  }

  public activate(): void {
    this.data.active = true;
    this.data.startTime = Date.now();
  }

  public deactivate(): void {
    this.data.active = false;
  }

  public update(): boolean {
    if (!this.data.active) return false;
    
    const elapsed = Date.now() - this.data.startTime;
    if (elapsed >= this.data.duration) {
      this.deactivate();
      if (this.callback) {
        this.callback(this);
      }
      return false;
    }
    
    return true;
  }

  public getRemainingTime(): number {
    if (!this.data.active) return 0;
    const elapsed = Date.now() - this.data.startTime;
    return Math.max(0, this.data.duration - elapsed);
  }

  public getProgress(): number {
    if (!this.data.active) return 0;
    const elapsed = Date.now() - this.data.startTime;
    return Math.min(1, elapsed / this.data.duration);
  }

  public onComplete(callback: (surge: PowerSurge) => void): void {
    this.callback = callback;
  }

  public addTarget(targetId: string): void {
    if (!this.data.targetIds.includes(targetId)) {
      this.data.targetIds.push(targetId);
    }
  }

  public removeTarget(targetId: string): void {
    const index = this.data.targetIds.indexOf(targetId);
    if (index !== -1) {
      this.data.targetIds.splice(index, 1);
    }
  }

  public getEffectivePower(): number {
    if (!this.data.active) return 0;
    const progress = this.getProgress();
    const fadeMultiplier = 1 - Math.pow(progress, 2);
    return this.data.powerAmount * fadeMultiplier;
  }

  public toData(): PowerSurgeData {
    return { ...this.data };
  }
}

export class PowerSurgeManager {
  private surges: Map<string, PowerSurge>;
  private activeSurges: Set<string>;
  private surgeHistory: PowerSurgeData[];

  constructor() {
    this.surges = new Map();
    this.activeSurges = new Set();
    this.surgeHistory = [];
  }

  public createSurge(data: Partial<PowerSurgeData>): PowerSurge {
    const surge = new PowerSurge(data);
    this.surges.set(surge.data.id, surge);
    return surge;
  }

  public activateSurge(surgeId: string): boolean {
    const surge = this.surges.get(surgeId);
    if (!surge) return false;
    
    surge.activate();
    this.activeSurges.add(surgeId);
    return true;
  }

  public deactivateSurge(surgeId: string): boolean {
    const surge = this.surges.get(surgeId);
    if (!surge) return false;
    
    surge.deactivate();
    this.activeSurges.delete(surgeId);
    return true;
  }

  public update(): void {
    const completed: string[] = [];
    
    this.activeSurges.forEach(surgeId => {
      const surge = this.surges.get(surgeId);
      if (surge && !surge.update()) {
        completed.push(surgeId);
        this.surgeHistory.push(surge.toData());
      }
    });
    
    completed.forEach(surgeId => {
      this.activeSurges.delete(surgeId);
    });
  }

  public getTotalBoost(): number {
    let total = 0;
    this.activeSurges.forEach(surgeId => {
      const surge = this.surges.get(surgeId);
      if (surge) {
        total += surge.getEffectivePower();
      }
    });
    return total;
  }

  public getActiveSurges(): PowerSurge[] {
    const active: PowerSurge[] = [];
    this.activeSurges.forEach(surgeId => {
      const surge = this.surges.get(surgeId);
      if (surge) {
        active.push(surge);
      }
    });
    return active;
  }

  public getSurgeById(id: string): PowerSurge | undefined {
    return this.surges.get(id);
  }

  public getSurgeHistory(): PowerSurgeData[] {
    return [...this.surgeHistory];
  }

  public clearHistory(): void {
    this.surgeHistory = [];
  }

  public removeSurge(surgeId: string): boolean {
    this.activeSurges.delete(surgeId);
    return this.surges.delete(surgeId);
  }
}

export class PlacementValidator {
  private rules: PlacementRules;
  private terrainTypes: Map<string, string>;
  private occupiedPositions: Map<string, string>;

  constructor(rules?: Partial<PlacementRules>) {
    this.rules = {
      minDistance: rules?.minDistance || 2,
      maxSlope: rules?.maxSlope || 0.3,
      requiredTerrain: rules?.requiredTerrain || [],
      forbiddenTerrain: rules?.forbiddenTerrain || ['water', 'lava'],
      collisionRadius: rules?.collisionRadius || 1,
      alignmentRequired: rules?.alignmentRequired || false,
      gridSnapping: rules?.gridSnapping !== undefined ? rules?.gridSnapping : true,
    };
    this.terrainTypes = new Map();
    this.occupiedPositions = new Map();
  }

  public validatePlacement(
    position: Vector3,
    buildingType: string,
    existingBuildings: Map<string, { position: Vector3; radius: number }>
  ): { valid: boolean; errors: string[] } {
    const errors: string[] = [];
    
    if (!this.validateTerrain(position)) {
      errors.push('Invalid terrain type for placement');
    }
    
    if (!this.validateSlope(position)) {
      errors.push('Terrain slope too steep');
    }
    
    if (!this.validateCollisions(position, existingBuildings)) {
      errors.push('Collision with existing buildings');
    }
    
    if (!this.validateGridAlignment(position)) {
      errors.push('Building must be aligned to grid');
    }
    
    return {
      valid: errors.length === 0,
      errors,
    };
  }

  private validateTerrain(position: Vector3): boolean {
    const terrainType = this.terrainTypes.get(`${position.x},${position.z}`);
    
    if (this.rules.forbiddenTerrain.includes(terrainType || '')) {
      return false;
    }
    
    if (this.rules.requiredTerrain.length > 0) {
      return this.rules.requiredTerrain.includes(terrainType || '');
    }
    
    return true;
  }

  private validateSlope(position: Vector3): boolean {
    return true;
  }

  private validateCollisions(
    position: Vector3,
    buildings: Map<string, { position: Vector3; radius: number }>
  ): boolean {
    for (const [, building] of buildings) {
      const distance = position.distanceTo(building.position);
      if (distance < this.rules.collisionRadius + building.radius) {
        return false;
      }
    }
    return true;
  }

  private validateGridAlignment(position: Vector3): boolean {
    if (!this.rules.gridSnapping) return true;
    
    const gridSize = 1;
    const aligned = (
      Math.abs(position.x - Math.round(position.x / gridSize) * gridSize) < 0.01 &&
      Math.abs(position.z - Math.round(position.z / gridSize) * gridSize) < 0.01
    );
    
    return this.rules.alignmentRequired ? aligned : true;
  }

  public setTerrainType(x: number, z: number, type: string): void {
    this.terrainTypes.set(`${x},${z}`, type);
  }

  public getTerrainType(x: number, z: number): string | undefined {
    return this.terrainTypes.get(`${x},${z}`);
  }

  public markPositionOccupied(position: Vector3, buildingId: string): void {
    this.occupiedPositions.set(`${position.x},${position.z}`, buildingId);
  }

  public markPositionVacant(position: Vector3): void {
    this.occupiedPositions.delete(`${position.x},${position.z}`);
  }

  public isPositionOccupied(position: Vector3): boolean {
    return this.occupiedPositions.has(`${position.x},${position.z}`);
  }

  public getOccupant(position: Vector3): string | undefined {
    return this.occupiedPositions.get(`${position.x},${position.z}`);
  }

  public setRules(rules: Partial<PlacementRules>): void {
    Object.assign(this.rules, rules);
  }

  public getRules(): PlacementRules {
    return { ...this.rules };
  }

  public snapToGrid(position: Vector3, gridSize: number = 1): Vector3 {
    return new Vector3(
      Math.round(position.x / gridSize) * gridSize,
      position.y,
      Math.round(position.z / gridSize) * gridSize
    );
  }
}

export class GhostBuilding {
  public buildingId: string;
  public type: string;
  public position: Vector3;
  public rotation: number;
  public valid: boolean;
  public errors: string[];
  public cost: ConstructionCost;
  public powerRequirement: number;
  public visibility: number;
  public opacity: number;

  constructor(
    type: string,
    position: Vector3,
    cost: ConstructionCost,
    powerRequirement: number = 0
  ) {
    this.buildingId = crypto.randomUUID();
    this.type = type;
    this.position = position.clone();
    this.rotation = 0;
    this.valid = false;
    this.errors = [];
    this.cost = { ...cost };
    this.powerRequirement = powerRequirement;
    this.visibility = 1.0;
    this.opacity = 0.5;
  }

  public updatePosition(position: Vector3): void {
    this.position = position.clone();
  }

  public setRotation(rotation: number): void {
    this.rotation = rotation % (Math.PI * 2);
  }

  public setValidation(valid: boolean, errors: string[] = []): void {
    this.valid = valid;
    this.errors = errors;
  }

  public setVisibility(visibility: number): void {
    this.visibility = Math.max(0, Math.min(1, visibility));
  }

  public setOpacity(opacity: number): void {
    this.opacity = Math.max(0, Math.min(1, opacity));
  }

  public canAfford(resources: { minerals: number; energy: number }): boolean {
    return (
      resources.minerals >= this.cost.minerals &&
      resources.energy >= this.cost.energy
    );
  }

  public toData(): GhostBuildingData {
    return {
      buildingId: this.buildingId,
      type: this.type,
      position: this.position,
      rotation: this.rotation,
      valid: this.valid,
      errors: [...this.errors],
      cost: { ...this.cost },
      powerRequirement: this.powerRequirement,
    };
  }

  public clone(): GhostBuilding {
    const ghost = new GhostBuilding(
      this.type,
      this.position,
      { ...this.cost },
      this.powerRequirement
    );
    ghost.rotation = this.rotation;
    ghost.valid = this.valid;
    ghost.errors = [...this.errors];
    ghost.visibility = this.visibility;
    ghost.opacity = this.opacity;
    return ghost;
  }
}

export class ConstructionProgress {
  private progress: number;
  private startTime: number;
  private estimatedCompletion: number;
  private actualCompletion: number | null;
  private checkpoints: Map<string, number>;

  constructor(
    buildingId: string,
    totalTime: number,
    startTime: number = Date.now()
  ) {
    this.progress = 0;
    this.startTime = startTime;
    this.estimatedCompletion = startTime + totalTime;
    this.actualCompletion = null;
    this.checkpoints = new Map();
    this.checkpoints.set('start', 0);
    this.checkpoints.set('foundation', 10);
    this.checkpoints.set('structure', 30);
    this.checkpoints.set('mechanical', 60);
    this.checkpoints.set('electrical', 80);
    this.checkpoints.set('final', 100);
  }

  public update(deltaTime: number, workerCount: number = 1): number {
    if (this.isComplete()) return this.progress;
    
    const efficiency = Math.min(2, workerCount * 0.1 + 0.5);
    const progressIncrement = (deltaTime / (this.estimatedCompletion - this.startTime)) * 100 * efficiency;
    
    this.progress = Math.min(100, this.progress + progressIncrement);
    
    if (this.progress >= 100) {
      this.actualCompletion = Date.now();
    }
    
    return this.progress;
  }

  public isComplete(): boolean {
    return this.progress >= 100 || this.actualCompletion !== null;
  }

  public getProgress(): number {
    return this.progress;
  }

  public getEstimatedTimeRemaining(): number {
    if (this.isComplete()) return 0;
    
    const elapsed = Date.now() - this.startTime;
    const remaining = this.estimatedCompletion - Date.now();
    return Math.max(0, remaining);
  }

  public getCurrentPhase(): string {
    if (this.progress < 10) return 'foundation';
    if (this.progress < 30) return 'structure';
    if (this.progress < 60) return 'mechanical';
    if (this.progress < 80) return 'electrical';
    return 'final';
  }

  public getCheckpointProgress(checkpoint: string): number {
    const checkpointValue = this.checkpoints.get(checkpoint) || 0;
    const nextCheckpoint = this.getNextCheckpoint(checkpoint);
    
    if (nextCheckpoint === null) {
      return this.progress >= checkpointValue ? 100 : 0;
    }
    
    const nextValue = this.checkpoints.get(nextCheckpoint) || 100;
    const progressInPhase = this.progress - checkpointValue;
    const phaseRange = nextValue - checkpointValue;
    
    return Math.min(100, (progressInPhase / phaseRange) * 100);
  }

  private getNextCheckpoint(current: string): string | null {
    const checkpointOrder = ['start', 'foundation', 'structure', 'mechanical', 'electrical', 'final'];
    const currentIndex = checkpointOrder.indexOf(current);
    
    if (currentIndex === -1 || currentIndex === checkpointOrder.length - 1) {
      return null;
    }
    
    return checkpointOrder[currentIndex + 1];
  }

  public setEstimatedCompletion(time: number): void {
    this.estimatedCompletion = time;
  }

  public getElapsedTime(): number {
    return Date.now() - this.startTime;
  }

  public getActualDuration(): number | null {
    if (this.actualCompletion === null) return null;
    return this.actualCompletion - this.startTime;
  }
}

export class BuildQueue {
  private queue: BuildQueueItem[];
  private maxQueueSize: number;
  private currentBuild: BuildQueueItem | null;

  constructor(maxSize: number = 10) {
    this.queue = [];
    this.maxQueueSize = maxSize;
    this.currentBuild = null;
  }

  public addItem(
    buildingType: string,
    position: Vector3,
    rotation: number,
    cost: ConstructionCost,
    priority: number = 5
  ): string | null {
    if (this.queue.length >= this.maxQueueSize) {
      return null;
    }
    
    const id = crypto.randomUUID();
    const item: BuildQueueItem = {
      id,
      buildingType,
      position: position.clone(),
      rotation,
      priority,
      cost: { ...cost },
      progress: 0,
      status: ConstructionStatus.Pending,
      startTime: 0,
      estimatedCompletion: 0,
      workerCount: 0,
      requiredResources: new Map(),
    };
    
    this.queue.push(item);
    this.sortByPriority();
    
    return id;
  }

  public removeItem(id: string): boolean {
    const index = this.queue.findIndex(item => item.id === id);
    if (index === -1) return false;
    
    this.queue.splice(index, 1);
    return true;
  }

  public startConstruction(id: string, workerCount: number = 1): boolean {
    const item = this.queue.find(i => i.id === id);
    if (!item || item.status !== ConstructionStatus.Pending) return false;
    
    item.status = ConstructionStatus.InProgress;
    item.startTime = Date.now();
    item.workerCount = workerCount;
    item.estimatedCompletion = item.startTime + (item.cost.time * 1000);
    
    this.currentBuild = item;
    return true;
  }

  public updateConstruction(deltaTime: number): BuildQueueItem | null {
    if (!this.currentBuild) return null;
    
    const efficiency = Math.min(2, this.currentBuild.workerCount * 0.1 + 0.5);
    const progressIncrement = (deltaTime / (this.currentBuild.cost.time * 1000)) * 100 * efficiency;
    
    this.currentBuild.progress = Math.min(100, this.currentBuild.progress + progressIncrement);
    
    if (this.currentBuild.progress >= 100) {
      this.currentBuild.status = ConstructionStatus.Completed;
      const completed = this.currentBuild;
      this.removeItem(completed.id);
      this.currentBuild = this.getNextInQueue();
      return completed;
    }
    
    return null;
  }

  private getNextInQueue(): BuildQueueItem | null {
    const pending = this.queue.filter(item => item.status === ConstructionStatus.Pending);
    return pending.length > 0 ? pending[0] : null;
  }

  private sortByPriority(): void {
    this.queue.sort((a, b) => b.priority - a.priority);
  }

  public getQueue(): BuildQueueItem[] {
    return [...this.queue];
  }

  public getCurrentBuild(): BuildQueueItem | null {
    return this.currentBuild;
  }

  public getQueueSize(): number {
    return this.queue.length;
  }

  public isQueueFull(): boolean {
    return this.queue.length >= this.maxQueueSize;
  }

  public clearQueue(): void {
    this.queue = [];
    this.currentBuild = null;
  }

  public cancelItem(id: string): boolean {
    const item = this.queue.find(i => i.id === id);
    if (!item) return false;
    
    item.status = ConstructionStatus.Cancelled;
    
    if (this.currentBuild?.id === id) {
      this.removeItem(id);
      this.currentBuild = this.getNextInQueue();
    } else {
      this.removeItem(id);
    }
    
    return true;
  }

  public prioritizeItem(id: string, newPriority: number): boolean {
    const item = this.queue.find(i => i.id === id);
    if (!item) return false;
    
    item.priority = Math.max(1, Math.min(10, newPriority));
    this.sortByPriority();
    return true;
  }

  public getTotalQueueTime(): number {
    let total = 0;
    this.queue.forEach(item => {
      total += item.cost.time;
    });
    return total;
  }

  public getTotalQueueCost(): ConstructionCost {
    const total: ConstructionCost = { minerals: 0, energy: 0, time: 0, workers: 0 };
    
    this.queue.forEach(item => {
      total.minerals += item.cost.minerals;
      total.energy += item.cost.energy;
      total.time += item.cost.time;
      total.workers += item.cost.workers;
    });
    
    return total;
  }
}

export class ConstructionManager {
  private grid: PowerGrid;
  private distribution: PowerDistribution;
  private placementValidator: PlacementValidator;
  private buildQueue: BuildQueue;
  private activeConstructions: Map<string, ConstructionProgress>;
  private completedBuildings: Set<string>;
  private buildingDefinitions: Map<string, any>;
  private workers: number;
  private maxWorkers: number;

  constructor(
    grid: PowerGrid,
    distribution: PowerDistribution,
    maxWorkers: number = 10
  ) {
    this.grid = grid;
    this.distribution = distribution;
    this.placementValidator = new PlacementValidator();
    this.buildQueue = new BuildQueue();
    this.activeConstructions = new Map();
    this.completedBuildings = new Set();
    this.buildingDefinitions = new Map();
    this.workers = 0;
    this.maxWorkers = maxWorkers;
  }

  public registerBuildingType(type: string, definition: any): void {
    this.buildingDefinitions.set(type, definition);
  }

  public canPlaceBuilding(
    type: string,
    position: Vector3,
    existingBuildings: Map<string, { position: Vector3; radius: number }>
  ): { valid: boolean; errors: string[] } {
    return this.placementValidator.validatePlacement(
      position,
      type,
      existingBuildings
    );
  }

  public startConstruction(
    type: string,
    position: Vector3,
    rotation: number = 0
  ): string | null {
    const definition = this.buildingDefinitions.get(type);
    if (!definition) return null;
    
    const validation = this.canPlaceBuilding(
      type,
      position,
      new Map()
    );
    
    if (!validation.valid) return null;
    
    const cost: ConstructionCost = {
      minerals: definition.cost?.minerals || 100,
      energy: definition.cost?.energy || 50,
      time: definition.cost?.time || 30,
      workers: definition.cost?.workers || 2,
    };
    
    const queueId = this.buildQueue.addItem(
      type,
      position,
      rotation,
      cost,
      definition.priority || 5
    );
    
    if (queueId) {
      const constructionId = crypto.randomUUID();
      const progress = new ConstructionProgress(
        constructionId,
        cost.time * 1000
      );
      this.activeConstructions.set(constructionId, progress);
      return queueId;
    }
    
    return null;
  }

  public update(deltaTime: number): void {
    const completed = this.buildQueue.updateConstruction(deltaTime);
    
    if (completed) {
      this.completeConstruction(completed);
    }
  }

  private completeConstruction(item: BuildQueueItem): void {
    this.completedBuildings.add(item.id);
    
    const definition = this.buildingDefinitions.get(item.buildingType);
    if (definition) {
      if (definition.producesPower) {
        const producer = new PowerProducer(item.id, {
          type: item.buildingType,
          baseOutput: definition.powerOutput || 100,
          maxOutput: definition.maxPowerOutput || 150,
        });
        this.distribution.registerProducer(producer);
      }
      
      if (definition.consumesPower) {
        const consumer = new PowerConsumer(item.id, {
          type: item.buildingType,
          baseConsumption: definition.powerConsumption || 50,
          priority: definition.priority || 5,
          critical: definition.critical || false,
        });
        this.distribution.registerConsumer(consumer);
      }
      
      if (definition.storesPower) {
        const storage = new PowerStorage(item.id, {
          capacity: definition.storageCapacity || 500,
          chargeRate: definition.chargeRate || 50,
          dischargeRate: definition.dischargeRate || 50,
          efficiency: definition.storageEfficiency || 0.9,
        });
        this.distribution.registerStorage(storage);
      }
    }
    
    this.placementValidator.markPositionOccupied(item.position, item.id);
  }

  public cancelConstruction(id: string): boolean {
    return this.buildQueue.cancelItem(id);
  }

  public getActiveConstructions(): Map<string, ConstructionProgress> {
    return new Map(this.activeConstructions);
  }

  public getBuildQueue(): BuildQueueItem[] {
    return this.buildQueue.getQueue();
  }

  public assignWorkers(count: number): void {
    this.workers = Math.min(this.maxWorkers, Math.max(0, count));
  }

  public getAvailableWorkers(): number {
    return this.maxWorkers - this.workers;
  }

  public setMaxWorkers(max: number): void {
    this.maxWorkers = Math.max(1, max);
  }
}

export class RepairManager {
  private repairs: Map<string, RepairTask>;
  private buildingHealth: Map<string, number>;
  private repairRate: number;
  private assignedWorkers: Map<string, number>;
  private repairHistory: Map<string, { start: number; end?: number; cost: number }>;

  constructor(defaultRepairRate: number = 10) {
    this.repairs = new Map();
    this.buildingHealth = new Map();
    this.repairRate = defaultRepairRate;
    this.assignedWorkers = new Map();
    this.repairHistory = new Map();
  }

  public damageBuilding(buildingId: string, damage: number): void {
    const currentHealth = this.getBuildingHealth(buildingId);
    const newHealth = Math.max(0, currentHealth - damage);
    this.buildingHealth.set(buildingId, newHealth);
    
    if (newHealth < currentHealth && newHealth < 50) {
      this.createRepairTask(buildingId);
    }
  }

  public repairBuilding(buildingId: string, amount: number): number {
    const currentHealth = this.getBuildingHealth(buildingId);
    const maxHealth = 100;
    const actualRepair = Math.min(amount, maxHealth - currentHealth);
    const newHealth = currentHealth + actualRepair;
    
    this.buildingHealth.set(buildingId, newHealth);
    
    const task = this.repairs.get(buildingId);
    if (task && newHealth >= maxHealth) {
      this.completeRepairTask(buildingId);
    }
    
    return actualRepair;
  }

  public createRepairTask(buildingId: string): RepairTask | null {
    if (this.repairs.has(buildingId)) return null;
    
    const health = this.getBuildingHealth(buildingId);
    const damage = 100 - health;
    const repairCost = damage * this.repairRate;
    
    let priority: RepairPriority;
    if (health < 20) {
      priority = RepairPriority.Critical;
    } else if (health < 40) {
      priority = RepairPriority.High;
    } else if (health < 70) {
      priority = RepairPriority.Medium;
    } else {
      priority = RepairPriority.Low;
    }
    
    const task: RepairTask = {
      id: crypto.randomUUID(),
      buildingId,
      damage,
      repairCost,
      priority,
      estimatedTime: damage / this.repairRate,
      assignedWorkers: 0,
      status: ConstructionStatus.Pending,
    };
    
    this.repairs.set(buildingId, task);
    this.repairHistory.set(buildingId, { start: Date.now(), cost: repairCost });
    
    return task;
  }

  private completeRepairTask(buildingId: string): void {
    const task = this.repairs.get(buildingId);
    if (!task) return;
    
    task.status = ConstructionStatus.Completed;
    this.buildingHealth.set(buildingId, 100);
    this.assignedWorkers.delete(buildingId);
    
    const history = this.repairHistory.get(buildingId);
    if (history) {
      history.end = Date.now();
    }
    
    setTimeout(() => {
      this.repairs.delete(buildingId);
    }, 1000);
  }

  public assignWorkersToRepair(buildingId: string, count: number): boolean {
    const task = this.repairs.get(buildingId);
    if (!task) return false;
    
    task.assignedWorkers = count;
    this.assignedWorkers.set(buildingId, count);
    return true;
  }

  public update(deltaTime: number): void {
    this.repairs.forEach((task, buildingId) => {
      if (task.status !== ConstructionStatus.InProgress) return;
      
      const workers = task.assignedWorkers || 1;
      const efficiency = Math.min(2, workers * 0.1 + 0.5);
      const repairAmount = this.repairRate * efficiency * (deltaTime / 1000);
      
      this.repairBuilding(buildingId, repairAmount);
    });
  }

  public getBuildingHealth(buildingId: string): number {
    return this.buildingHealth.get(buildingId) || 100;
  }

  public getRepairTask(buildingId: string): RepairTask | undefined {
    return this.repairs.get(buildingId);
  }

  public getAllRepairTasks(): RepairTask[] {
    return Array.from(this.repairs.values());
  }

  public getCriticalRepairs(): RepairTask[] {
    return Array.from(this.repairs.values())
      .filter(task => task.priority === RepairPriority.Critical);
  }

  public cancelRepair(buildingId: string): boolean {
    this.assignedWorkers.delete(buildingId);
    return this.repairs.delete(buildingId);
  }

  public setRepairRate(rate: number): void {
    this.repairRate = Math.max(0.1, rate);
  }

  public getRepairHistory(buildingId: string): { start: number; end?: number; cost: number } | undefined {
    return this.repairHistory.get(buildingId);
  }

  public clearCompletedRepairs(): void {
    this.repairs.forEach((task, buildingId) => {
      if (task.status === ConstructionStatus.Completed) {
        this.repairs.delete(buildingId);
      }
    });
  }
}

export class SellManager {
  private sellRates: Map<string, number>;
  private sellHistory: Map<string, { buildingId: string; refund: number; time: number }>;

  constructor() {
    this.sellRates = new Map();
    this.sellHistory = new Map();
    this.initializeDefaultRates();
  }

  private initializeDefaultRates(): void {
    this.sellRates.set('powerPlant', 0.7);
    this.sellRates.set('consumer', 0.8);
    this.sellRates.set('storage', 0.75);
    this.sellRates.set('defense', 0.6);
    this.sellRates.set('production', 0.65);
    this.sellRates.set('research', 0.5);
    this.sellRates.set('utility', 0.7);
  }

  public setSellRate(buildingType: string, rate: number): void {
    this.sellRates.set(buildingType, Math.max(0, Math.min(1, rate)));
  }

  public getSellRate(buildingType: string): number {
    return this.sellRates.get(buildingType) || 0.5;
  }

  public calculateRefund(
    buildingType: string,
    originalCost: ConstructionCost,
    currentHealth: number
  ): ConstructionCost {
    const rate = this.getSellRate(buildingType);
    const healthMultiplier = currentHealth / 100;
    
    return {
      minerals: Math.floor(originalCost.minerals * rate * healthMultiplier),
      energy: Math.floor(originalCost.energy * rate * healthMultiplier),
      time: 0,
      workers: 0,
    };
  }

  public sellBuilding(
    buildingId: string,
    buildingType: string,
    originalCost: ConstructionCost,
    currentHealth: number
  ): ConstructionCost {
    const refund = this.calculateRefund(buildingType, originalCost, currentHealth);
    
    this.sellHistory.set(buildingId, {
      buildingId,
      refund: refund.minerals + refund.energy,
      time: Date.now(),
    });
    
    return refund;
  }

  public getTotalRefunds(): number {
    let total = 0;
    this.sellHistory.forEach(record => {
      total += record.refund;
    });
    return total;
  }

  public getRefundHistory(): { buildingId: string; refund: number; time: number }[] {
    return Array.from(this.sellHistory.values());
  }

  public clearHistory(): void {
    this.sellHistory.clear();
  }
}

export class PowerNetworkVisualization {
  private grid: PowerGrid;
  private visibleNodes: Set<string>;
  private visibleConnections: Set<string>;
  private highlightedPaths: string[][];
  private nodeColors: Map<PowerState, string>;
  private connectionColors: Map<ConnectionType, string>;
  private showFlow: boolean;
  private flowAnimationOffset: number;

  constructor(grid: PowerGrid) {
    this.grid = grid;
    this.visibleNodes = new Set();
    this.visibleConnections = new Set();
    this.highlightedPaths = [];
    this.showFlow = true;
    this.flowAnimationOffset = 0;
    
    this.nodeColors = new Map();
    this.nodeColors.set(PowerState.Online, '#00ff00');
    this.nodeColors.set(PowerState.Offline, '#888888');
    this.nodeColors.set(PowerState.Brownout, '#ffff00');
    this.nodeColors.set(PowerState.Blackout, '#ff0000');
    this.nodeColors.set(PowerState.Surge, '#00ffff');
    this.nodeColors.set(PowerState.Damaged, '#ff8800');
    this.nodeColors.set(PowerState.Constructing, '#ffffff');
    
    this.connectionColors = new Map();
    this.connectionColors.set(ConnectionType.Direct, '#00ff00');
    this.connectionColors.set(ConnectionType.Wireless, '#0088ff');
    this.connectionColors.set(ConnectionType.Relay, '#ffaa00');
    this.connectionColors.set(ConnectionType.Cable, '#00aa00');
    this.connectionColors.set(ConnectionType.Grid, '#aa00ff');
  }

  public update(deltaTime: number): void {
    if (this.showFlow) {
      this.flowAnimationOffset += deltaTime * 0.001;
      if (this.flowAnimationOffset > 1) {
        this.flowAnimationOffset = 0;
      }
    }
  }

  public setNodeVisible(nodeId: string, visible: boolean): void {
    if (visible) {
      this.visibleNodes.add(nodeId);
    } else {
      this.visibleNodes.delete(nodeId);
    }
  }

  public setConnectionVisible(connectionId: string, visible: boolean): void {
    if (visible) {
      this.visibleConnections.add(connectionId);
    } else {
      this.visibleConnections.delete(connectionId);
    }
  }

  public highlightPath(path: string[]): void {
    this.highlightedPaths.push(path);
    
    for (let i = 0; i < path.length - 1; i++) {
      this.setNodeVisible(path[i], true);
      this.setConnectionVisible(`${path[i]}-${path[i + 1]}`, true);
    }
    this.setNodeVisible(path[path.length - 1], true);
  }

  public clearHighlights(): void {
    this.highlightedPaths = [];
  }

  public getNodeColor(state: PowerState): string {
    return this.nodeColors.get(state) || '#ffffff';
  }

  public getConnectionColor(type: ConnectionType): string {
    return this.connectionColors.get(type) || '#ffffff';
  }

  public getFlowAnimationOffset(): number {
    return this.flowAnimationOffset;
  }

  public setShowFlow(show: boolean): void {
    this.showFlow = show;
  }

  public isShowingFlow(): boolean {
    return this.showFlow;
  }

  public getVisibleNodes(): string[] {
    return Array.from(this.visibleNodes);
  }

  public getVisibleConnections(): string[] {
    return Array.from(this.visibleConnections);
  }

  public getHighlightedPaths(): string[][] {
    return [...this.highlightedPaths];
  }

  public getConnectionFlowIntensity(connectionId: string): number {
    for (const network of this.grid.networks.values()) {
      const connection = network.connections.get(connectionId);
      if (connection) {
        return connection.currentFlow / connection.maxCapacity;
      }
    }
    return 0;
  }

  public getNodeLoadIntensity(nodeId: string): number {
    for (const network of this.grid.networks.values()) {
      const node = network.nodes.get(nodeId);
      if (node) {
        return node.currentLoad / Math.max(1, node.capacity);
      }
    }
    return 0;
  }
}

export class PowerEfficiencyOptimizer {
  private grid: PowerGrid;
  private distribution: PowerDistribution;
  private efficiencyHistory: number[];
  private optimizationEnabled: boolean;
  private lastOptimizationTime: number;

  constructor(grid: PowerGrid, distribution: PowerDistribution) {
    this.grid = grid;
    this.distribution = distribution;
    this.efficiencyHistory = [];
    this.optimizationEnabled = true;
    this.lastOptimizationTime = Date.now();
  }

  public update(deltaTime: number): void {
    this.recordEfficiency();
    
    if (this.optimizationEnabled && Date.now() - this.lastOptimizationTime > 60000) {
      this.optimize();
      this.lastOptimizationTime = Date.now();
    }
  }

  private recordEfficiency(): void {
    const efficiency = this.grid.networks.size > 0
      ? this.calculateGridEfficiency()
      : 0;
    
    this.efficiencyHistory.push(efficiency);
    if (this.efficiencyHistory.length > 1000) {
      this.efficiencyHistory.shift();
    }
  }

  public calculateGridEfficiency(): number {
    let totalEfficiency = 0;
    let networkCount = 0;
    
    this.grid.networks.forEach(network => {
      totalEfficiency += network.getEfficiency();
      networkCount++;
    });
    
    return networkCount > 0 ? totalEfficiency / networkCount : 0;
  }

  public optimize(): void {
    this.balanceLoads();
    this.consolidateNetworks();
    this.removeInefficientConnections();
  }

  private balanceLoads(): void {
    const producers = Array.from(this.distribution['producers'].values());
    const storages = Array.from(this.distribution['storages'].values());
    
    const totalSurplus = this.distribution.getTotalProduction() - this.distribution.getTotalConsumption();
    
    if (totalSurplus > 0) {
      let remainingSurplus = totalSurplus;
      
      storages.forEach(storage => {
        if (remainingSurplus <= 0) return;
        if (storage.isFull()) return;
        
        const chargeAmount = Math.min(
          remainingSurplus,
          storage.data.chargeRate
        );
        storage.charge(chargeAmount);
        remainingSurplus -= chargeAmount;
      });
    } else if (totalSurplus < 0) {
      let remainingDeficit = -totalSurplus;
      
      storages.forEach(storage => {
        if (remainingDeficit <= 0) return;
        if (storage.isEmpty()) return;
        
        const dischargeAmount = Math.min(
          remainingDeficit,
          storage.data.dischargeRate
        );
        storage.discharge(dischargeAmount);
        remainingDeficit -= dischargeAmount;
      });
    }
  }

  private consolidateNetworks(): void {
    const networkList = Array.from(this.grid.networks.values());
    
    for (let i = 0; i < networkList.length; i++) {
      for (let j = i + 1; j < networkList.length; j++) {
        if (this.networksCanConnect(networkList[i], networkList[j])) {
          this.grid.mergeNetworks(networkList[i].id, networkList[j].id);
          return;
        }
      }
    }
  }

  private networksCanConnect(net1: PowerNetwork, net2: PowerNetwork): boolean {
    for (const node1 of net1.nodes.values()) {
      for (const node2 of net2.nodes.values()) {
        const distance = node1.position.distanceTo(node2.position);
        if (distance < 50) {
          return true;
        }
      }
    }
    return false;
  }

  private removeInefficientConnections(): void {
    this.grid.networks.forEach(network => {
      const connectionsToRemove: string[] = [];
      
      network.connections.forEach((connection, connId) => {
        if (connection.currentFlow < connection.maxCapacity * 0.1) {
          connectionsToRemove.push(connId);
        }
      });
      
      connectionsToRemove.forEach(connId => {
        network.removeConnection(connId);
      });
    });
  }

  public getEfficiencyTrend(): number {
    if (this.efficiencyHistory.length < 10) return 0;
    
    const recent = this.efficiencyHistory.slice(-10);
    const older = this.efficiencyHistory.slice(-20, -10);
    
    const recentAvg = recent.reduce((a, b) => a + b, 0) / recent.length;
    const olderAvg = older.length > 0 ? older.reduce((a, b) => a + b, 0) / older.length : recentAvg;
    
    return recentAvg - olderAvg;
  }

  public getAverageEfficiency(): number {
    if (this.efficiencyHistory.length === 0) return 0;
    return this.efficiencyHistory.reduce((a, b) => a + b, 0) / this.efficiencyHistory.length;
  }

  public setOptimizationEnabled(enabled: boolean): void {
    this.optimizationEnabled = enabled;
  }

  public isOptimizationEnabled(): boolean {
    return this.optimizationEnabled;
  }

  public getEfficiencyHistory(): number[] {
    return [...this.efficiencyHistory];
  }

  public suggestOptimizations(): string[] {
    const suggestions: string[] = [];
    
    if (this.getEfficiencyTrend() < -0.01) {
      suggestions.push('Grid efficiency is declining. Consider adding more power producers.');
    }
    
    const powerStatus = this.distribution.getPowerStatus();
    if (powerStatus.deficit > 0) {
      suggestions.push(`Power deficit of ${powerStatus.deficit}. Add more producers or reduce consumption.`);
    }
    
    if (this.grid.networks.size > 5) {
      suggestions.push('Multiple isolated networks detected. Consider connecting them for better distribution.');
    }
    
    return suggestions;
  }
}

export class PowerGridSystem {
  public grid: PowerGrid;
  public distribution: PowerDistribution;
  public connectionManager: GridConnectionManager;
  public brownoutManager: BrownoutManager;
  public blackoutManager: BlackoutManager;
  public surgeManager: PowerSurgeManager;
  public constructionManager: ConstructionManager;
  public repairManager: RepairManager;
  public sellManager: SellManager;
  public visualization: PowerNetworkVisualization;
  public optimizer: PowerEfficiencyOptimizer;
  
  private config: PowerGridSystemConfig;
  private isRunning: boolean;
  private lastUpdateTime: number;
  private updateAccumulator: number;

  constructor(config?: Partial<PowerGridSystemConfig>) {
    this.config = {
      maxNetworkSize: config?.maxNetworkSize || 100,
      maxConnectionsPerNode: config?.maxConnectionsPerNode || 10,
      powerLossPerDistance: config?.powerLossPerDistance || 0.001,
      brownoutThreshold: config?.brownoutThreshold || 0.8,
      blackoutThreshold: config?.blackoutThreshold || 0.95,
      surgeDuration: config?.surgeDuration || 5000,
      defaultRepairRate: config?.defaultRepairRate || 10,
      gridUpdateInterval: config?.gridUpdateInterval || 100,
      visualizationEnabled: config?.visualizationEnabled !== undefined ? config.visualizationEnabled : true,
      debugMode: config?.debugMode || false,
    };

    this.grid = new PowerGrid();
    this.distribution = new PowerDistribution(this.grid);
    this.connectionManager = new GridConnectionManager(this.grid);
    this.brownoutManager = new BrownoutManager(this.grid, this.distribution, this.config.brownoutThreshold);
    this.blackoutManager = new BlackoutManager(this.grid, this.distribution);
    this.surgeManager = new PowerSurgeManager();
    this.constructionManager = new ConstructionManager(this.grid, this.distribution);
    this.repairManager = new RepairManager(this.config.defaultRepairRate);
    this.sellManager = new SellManager();
    this.visualization = new PowerNetworkVisualization(this.grid);
    this.optimizer = new PowerEfficiencyOptimizer(this.grid, this.distribution);

    this.isRunning = false;
    this.lastUpdateTime = Date.now();
    this.updateAccumulator = 0;
  }

  public start(): void {
    this.isRunning = true;
    this.lastUpdateTime = Date.now();
  }

  public stop(): void {
    this.isRunning = false;
  }

  public update(deltaTime: number): void {
    if (!this.isRunning) return;

    this.updateAccumulator += deltaTime;

    while (this.updateAccumulator >= this.config.gridUpdateInterval) {
      this.performUpdate(this.config.gridUpdateInterval);
      this.updateAccumulator -= this.config.gridUpdateInterval;
    }
  }

  private performUpdate(deltaTime: number): void {
    this.grid.updateGrid(deltaTime);
    this.distribution.distribute(deltaTime);
    this.brownoutManager.update(deltaTime);
    this.blackoutManager.update(deltaTime);
    this.surgeManager.update();
    this.constructionManager.update(deltaTime);
    this.repairManager.update(deltaTime);
    this.optimizer.update(deltaTime);
    
    if (this.config.visualizationEnabled) {
      this.visualization.update(deltaTime);
    }
  }

  public createPowerNode(
    id: string,
    position: Vector3,
    sector: GridSector = GridSector.Infrastructure,
    capacity: number = 100
  ): PowerNode {
    const network = this.grid.createNetwork(id + '_network');
    const node = new PowerNode({
      id,
      position,
      sector,
      capacity,
      state: PowerState.Online,
    });
    
    network.addNode(node);
    
    if (sector !== GridSector.Infrastructure) {
      let sectorNodes = this.grid.gridSectors.get(sector);
      if (!sectorNodes) {
        sectorNodes = new Set();
        this.grid.gridSectors.set(sector, sectorNodes);
      }
      sectorNodes.add(id);
    }
    
    return node;
  }

  public connectNodes(
    sourceId: string,
    targetId: string,
    type: ConnectionType = ConnectionType.Direct
  ): PowerConnection | null {
    return this.connectionManager.createConnection(sourceId, targetId, type);
  }

  public registerPowerProducer(
    buildingId: string,
    type: string,
    baseOutput: number,
    maxOutput?: number
  ): PowerProducer {
    const producer = new PowerProducer(buildingId, {
      type,
      baseOutput,
      maxOutput: maxOutput || baseOutput * 1.5,
    });
    
    this.distribution.registerProducer(producer);
    return producer;
  }

  public registerPowerConsumer(
    buildingId: string,
    type: string,
    baseConsumption: number,
    priority?: number,
    critical?: boolean
  ): PowerConsumer {
    const consumer = new PowerConsumer(buildingId, {
      type,
      baseConsumption,
      priority: priority || 5,
      critical: critical || false,
    });
    
    this.distribution.registerConsumer(consumer);
    return consumer;
  }

  public registerPowerStorage(
    buildingId: string,
    capacity: number,
    chargeRate?: number,
    dischargeRate?: number
  ): PowerStorage {
    const storage = new PowerStorage(buildingId, {
      capacity,
      chargeRate: chargeRate || 50,
      dischargeRate: dischargeRate || 50,
    });
    
    this.distribution.registerStorage(storage);
    return storage;
  }

  public createSurge(
    sourceId: string,
    targetIds: string[],
    powerAmount: number,
    duration?: number
  ): PowerSurge {
    const surge = this.surgeManager.createSurge({
      sourceId,
      targetIds,
      powerAmount,
      duration: duration || this.config.surgeDuration,
    });
    
    surge.onComplete((s) => {
      if (this.config.debugMode) {
        console.log(`Power surge ${s.data.id} completed`);
      }
    });
    
    return surge;
  }

  public activateSurge(surgeId: string): boolean {
    return this.surgeManager.activateSurge(surgeId);
  }

  public getGridStatus(): {
    totalCapacity: number;
    totalLoad: number;
    surplus: number;
    networkCount: number;
    efficiency: number;
  } {
    return {
      totalCapacity: this.grid.getTotalGridCapacity(),
      totalLoad: this.grid.getTotalGridLoad(),
      surplus: this.grid.getTotalGridSurplus(),
      networkCount: this.grid.networks.size,
      efficiency: this.optimizer.calculateGridEfficiency(),
    };
  }

  public getPowerDistributionStatus(): {
    production: number;
    consumption: number;
    surplus: number;
    deficit: number;
    storedPower: number;
    storageCapacity: number;
  } {
    const status = this.distribution.getPowerStatus();
    return {
      ...status,
      storedPower: this.distribution.getTotalStoredPower(),
      storageCapacity: this.distribution.getTotalStorageCapacity(),
    };
  }

  public getSystemAlerts(): string[] {
    const alerts: string[] = [];
    const status = this.getPowerDistributionStatus();
    
    if (status.deficit > 0) {
      alerts.push(`Power deficit: ${status.deficit.toFixed(2)}`);
    }
    
    if (this.brownoutManager.isInBrownout()) {
      alerts.push('Brownout in progress');
    }
    
    if (this.blackoutManager.isInBlackout()) {
      alerts.push('Blackout detected');
    }
    
    const optimizations = this.optimizer.suggestOptimizations();
    alerts.push(...optimizations);
    
    const criticalRepairs = this.repairManager.getCriticalRepairs();
    if (criticalRepairs.length > 0) {
      alerts.push(`${criticalRepairs.length} critical repair(s) needed`);
    }
    
    return alerts;
  }

  public getConfiguration(): PowerGridSystemConfig {
    return { ...this.config };
  }

  public setConfiguration(config: Partial<PowerGridSystemConfig>): void {
    Object.assign(this.config, config);
    
    if (config.brownoutThreshold !== undefined) {
      this.brownoutManager.setThreshold(config.brownoutThreshold);
    }
    
    if (config.defaultRepairRate !== undefined) {
      this.repairManager.setRepairRate(config.defaultRepairRate);
    }
  }

  public exportState(): any {
    return {
      config: this.config,
      grid: this.grid.toData(),
      powerStatus: this.getPowerDistributionStatus(),
      gridStatus: this.getGridStatus(),
      alerts: this.getSystemAlerts(),
      efficiencyHistory: this.optimizer.getEfficiencyHistory(),
    };
  }

  public importState(state: any): boolean {
    try {
      if (state.config) {
        this.setConfiguration(state.config);
      }
      return true;
    } catch (error) {
      if (this.config.debugMode) {
        console.error('Failed to import state:', error);
      }
      return false;
    }
  }

  public reset(): void {
    this.stop();
    
    this.grid = new PowerGrid();
    this.distribution = new PowerDistribution(this.grid);
    this.connectionManager = new GridConnectionManager(this.grid);
    this.brownoutManager = new BrownoutManager(this.grid, this.distribution, this.config.brownoutThreshold);
    this.blackoutManager = new BlackoutManager(this.grid, this.distribution);
    this.surgeManager = new PowerSurgeManager();
    this.repairManager = new RepairManager(this.config.defaultRepairRate);
    this.sellManager = new SellManager();
    this.visualization = new PowerNetworkVisualization(this.grid);
    this.optimizer = new PowerEfficiencyOptimizer(this.grid, this.distribution);
    
    this.updateAccumulator = 0;
  }

  public isSystemRunning(): boolean {
    return this.isRunning;
  }

  public getDeltaTime(): number {
    return Date.now() - this.lastUpdateTime;
  }
}

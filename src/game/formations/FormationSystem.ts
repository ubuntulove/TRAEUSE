import { Vector2, Vector3 } from 'three';

export enum FormationType {
  Line = 'line',
  Column = 'column',
  Wedge = 'wedge',
  Circle = 'circle',
  Square = 'square',
  Echelon = 'echelon',
  VShape = 'vshape',
  Box = 'box',
}

export enum FormationState {
  Idle = 'idle',
  Moving = 'moving',
  Engaging = 'engaging',
  Breaking = 'breaking',
  reforming = 'reforming',
  Scattered = 'scattered',
}

export enum SlotStatus {
  Empty = 'empty',
  Occupied = 'occupied',
  Reserved = 'reserved',
  Invalid = 'invalid',
}

export enum AssignmentPriority {
  Highest = 0,
  High = 1,
  Normal = 2,
  Low = 3,
  Lowest = 4,
}

export interface Position {
  x: number;
  y: number;
  z: number;
}

export interface SpacingConfig {
  horizontal: number;
  vertical: number;
  depth: number;
  unitRadius: number;
  safeZone: number;
}

export interface FormationStyle {
  name: string;
  tightness: number;
  aggressiveness: number;
  defensiveness: number;
  mobility: number;
}

export interface FormationSlot {
  id: string;
  localPosition: Position;
  status: SlotStatus;
  assignedUnitId: string | null;
  priority: AssignmentPriority;
  isFlanking: boolean;
  isSupport: boolean;
  distanceFromLeader: number;
  connectionSlots: string[];
}

export interface Formation {
  id: string;
  type: FormationType;
  name: string;
  slots: Map<string, FormationSlot>;
  leaderSlotId: string;
  spacing: SpacingConfig;
  style: FormationStyle;
  rotation: number;
  anchorPosition: Position;
  state: FormationState;
  maxUnits: number;
  currentUnitCount: number;
  bounds: FormationBounds;
  createdAt: number;
  lastModified: number;
}

export interface FormationBounds {
  minX: number;
  maxX: number;
  minZ: number;
  maxZ: number;
  centerX: number;
  centerZ: number;
  width: number;
  depth: number;
}

export interface SlotAssignment {
  unitId: string;
  slotId: string;
  priority: AssignmentPriority;
  assignedAt: number;
  distance: number;
}

export interface FormationTransition {
  fromType: FormationType;
  toType: FormationType;
  duration: number;
  easing: string;
  interpolatingSlots: boolean;
}

export interface FormationMorphTarget {
  targetType: FormationType;
  targetSpacing: SpacingConfig;
  prioritySlots: string[];
  preserveAssignments: boolean;
}

export interface Unit {
  id: string;
  position: Position;
  targetPosition: Position;
  velocity: Vector3;
  speed: number;
  formationId: string | null;
  slotId: string | null;
  isLeader: boolean;
  isSelected: boolean;
  health: number;
  maxHealth: number;
  unitType: string;
}

export interface FormationEvent {
  type: string;
  formationId: string;
  timestamp: number;
  data: any;
}

export class FormationSystem {
  private formations: Map<string, Formation>;
  private units: Map<string, Unit>;
  private eventListeners: Map<string, Function[]>;
  private formationGenerators: Map<FormationType, Function>;
  private transitionQueue: FormationTransition[];
  private activeTransitions: Map<string, FormationTransition>;
  private defaultSpacing: SpacingConfig;
  private defaultStyle: FormationStyle;

  constructor() {
    this.formations = new Map();
    this.units = new Map();
    this.eventListeners = new Map();
    this.formationGenerators = new Map();
    this.transitionQueue = [];
    this.activeTransitions = new Map();
    
    this.defaultSpacing = {
      horizontal: 2.0,
      vertical: 2.0,
      depth: 2.0,
      unitRadius: 1.0,
      safeZone: 0.5,
    };
    
    this.defaultStyle = {
      name: 'default',
      tightness: 0.7,
      aggressiveness: 0.5,
      defensiveness: 0.5,
      mobility: 0.6,
    };
    
    this.initializeGenerators();
  }

  private initializeGenerators(): void {
    this.formationGenerators.set(FormationType.Line, this.generateLineFormation.bind(this));
    this.formationGenerators.set(FormationType.Column, this.generateColumnFormation.bind(this));
    this.formationGenerators.set(FormationType.Wedge, this.generateWedgeFormation.bind(this));
    this.formationGenerators.set(FormationType.Circle, this.generateCircleFormation.bind(this));
    this.formationGenerators.set(FormationType.Square, this.generateSquareFormation.bind(this));
    this.formationGenerators.set(FormationType.Echelon, this.generateEchelonFormation.bind(this));
    this.formationGenerators.set(FormationType.VShape, this.generateVShapeFormation.bind(this));
    this.formationGenerators.set(FormationType.Box, this.generateBoxFormation.bind(this));
  }

  public createFormation(
    type: FormationType,
    unitCount: number,
    anchorPosition: Position,
    spacing?: SpacingConfig,
    style?: FormationStyle
  ): Formation {
    const id = this.generateFormationId();
    const formationSpacing = spacing || { ...this.defaultSpacing };
    const formationStyle = style || { ...this.defaultStyle };
    
    const generator = this.formationGenerators.get(type);
    if (!generator) {
      throw new Error(`Unknown formation type: ${type}`);
    }
    
    const slots = generator(unitCount, formationSpacing);
    const leaderSlotId = this.findLeaderSlotId(slots);
    
    const bounds = this.calculateFormationBounds(slots);
    
    const formation: Formation = {
      id,
      type,
      name: `${type}_${id}`,
      slots: new Map(Object.entries(slots)),
      leaderSlotId,
      spacing: formationSpacing,
      style: formationStyle,
      rotation: 0,
      anchorPosition: { ...anchorPosition },
      state: FormationState.Idle,
      maxUnits: unitCount,
      currentUnitCount: 0,
      bounds,
      createdAt: Date.now(),
      lastModified: Date.now(),
    };
    
    this.formations.set(id, formation);
    this.emitEvent('formationCreated', { formationId: id, type, unitCount });
    
    return formation;
  }

  private generateFormationId(): string {
    return `formation_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private generateSlotId(): string {
    return `slot_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private generateLineFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 10);
    const unitsPerRow = Math.ceil(unitCount / rows);
    
    for (let row = 0; row < rows; row++) {
      for (let col = 0; col < unitsPerRow; col++) {
        const index = row * unitsPerRow + col;
        if (index >= unitCount) break;
        
        const slotId = this.generateSlotId();
        const x = (col - (unitsPerRow - 1) / 2) * spacing.horizontal;
        const z = row * spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 ? AssignmentPriority.Highest : AssignmentPriority.Normal,
          isFlanking: col === 0 || col === unitsPerRow - 1,
          isSupport: row > 0,
          distanceFromLeader: row,
          connectionSlots: this.getConnectedSlots(slots, slotId, 'line'),
        };
      }
    }
    
    return slots;
  }

  private generateColumnFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const unitsPerColumn = Math.ceil(unitCount / 3);
    
    for (let col = 0; col < 3; col++) {
      for (let row = 0; row < unitsPerColumn; row++) {
        const index = col * unitsPerColumn + row;
        if (index >= unitCount) break;
        
        const slotId = this.generateSlotId();
        const x = (col - 1) * spacing.horizontal;
        const z = row * spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 && col === 1 ? AssignmentPriority.Highest : AssignmentPriority.Normal,
          isFlanking: col === 0 || col === 2,
          isSupport: col === 0 || col === 2,
          distanceFromLeader: row,
          connectionSlots: this.getConnectedSlots(slots, slotId, 'column'),
        };
      }
    }
    
    return slots;
  }

  private generateWedgeFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(Math.sqrt(unitCount * 2));
    
    let unitIndex = 0;
    for (let row = 0; row < rows && unitIndex < unitCount; row++) {
      const unitsInRow = row + 1;
      for (let col = 0; col < unitsInRow && unitIndex < unitCount; col++) {
        const slotId = this.generateSlotId();
        const rowOffset = -row / 2;
        const x = (col - (unitsInRow - 1) / 2) * spacing.horizontal;
        const z = row * spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 ? AssignmentPriority.Highest : AssignmentPriority.Normal - row,
          isFlanking: col === 0 || col === unitsInRow - 1,
          isSupport: row > 2,
          distanceFromLeader: row,
          connectionSlots: this.getConnectedSlots(slots, slotId, 'wedge'),
        };
        
        unitIndex++;
      }
    }
    
    return slots;
  }

  private generateCircleFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const innerRadius = spacing.horizontal * 1.5;
    const outerRadius = innerRadius + spacing.horizontal;
    
    const centerSlotId = this.generateSlotId();
    slots[centerSlotId] = {
      id: centerSlotId,
      localPosition: { x: 0, y: 0, z: 0 },
      status: SlotStatus.Empty,
      assignedUnitId: null,
      priority: AssignmentPriority.Highest,
      isFlanking: false,
      isSupport: true,
      distanceFromLeader: 0,
      connectionSlots: [],
    };
    
    const ringUnits = unitCount - 1;
    for (let i = 0; i < ringUnits; i++) {
      const angle = (i / ringUnits) * Math.PI * 2;
      const radius = spacing.horizontal * 2;
      const slotId = this.generateSlotId();
      
      slots[slotId] = {
        id: slotId,
        localPosition: { 
          x: Math.cos(angle) * radius, 
          y: 0, 
          z: Math.sin(angle) * radius 
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: i % 2 === 0 ? AssignmentPriority.High : AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: false,
        distanceFromLeader: 1,
        connectionSlots: [centerSlotId],
      };
      
      slots[centerSlotId].connectionSlots.push(slotId);
    }
    
    return slots;
  }

  private generateSquareFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const sideLength = Math.ceil(Math.sqrt(unitCount));
    const perimeter = sideLength * 4 - 4;
    let unitIndex = 0;
    
    for (let side = 0; side < 4 && unitIndex < unitCount; side++) {
      for (let i = 0; i < sideLength && unitIndex < unitCount; i++) {
        let x = 0, z = 0;
        
        switch (side) {
          case 0: x = i; z = 0; break;
          case 1: x = sideLength - 1; z = i; break;
          case 2: x = sideLength - 1 - i; z = sideLength - 1; break;
          case 3: x = 0; z = sideLength - 1 - i; break;
        }
        
        const slotId = this.generateSlotId();
        const isCorner = (i === 0) || (i === sideLength - 1);
        
        slots[slotId] = {
          id: slotId,
          localPosition: { 
            x: (x - (sideLength - 1) / 2) * spacing.horizontal, 
            y: 0, 
            z: (z - (sideLength - 1) / 2) * spacing.depth 
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: isCorner ? AssignmentPriority.High : AssignmentPriority.Normal,
          isFlanking: isCorner,
          isSupport: !isCorner && side % 2 === 1,
          distanceFromLeader: Math.min(x, z, sideLength - 1 - x, sideLength - 1 - z),
          connectionSlots: this.getConnectedSlots(slots, slotId, 'square'),
        };
        
        unitIndex++;
      }
    }
    
    return slots;
  }

  private generateEchelonFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 5);
    const unitsPerRow = Math.ceil(unitCount / rows);
    
    for (let row = 0; row < rows; row++) {
      const rowOffset = row * spacing.horizontal * 0.5;
      for (let col = 0; col < unitsPerRow; col++) {
        const index = row * unitsPerRow + col;
        if (index >= unitCount) break;
        
        const slotId = this.generateSlotId();
        const x = col * spacing.horizontal + rowOffset - (unitsPerRow * spacing.horizontal) / 2;
        const z = row * spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: col === 0 ? AssignmentPriority.High : AssignmentPriority.Normal,
          isFlanking: col === unitsPerRow - 1,
          isSupport: false,
          distanceFromLeader: row,
          connectionSlots: this.getConnectedSlots(slots, slotId, 'echelon'),
        };
      }
    }
    
    return slots;
  }

  private generateVShapeFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const halfUnits = Math.ceil(unitCount / 2);
    const rows = halfUnits;
    
    const leaderSlotId = this.generateSlotId();
    slots[leaderSlotId] = {
      id: leaderSlotId,
      localPosition: { x: 0, y: 0, z: 0 },
      status: SlotStatus.Empty,
      assignedUnitId: null,
      priority: AssignmentPriority.Highest,
      isFlanking: false,
      isSupport: false,
      distanceFromLeader: 0,
      connectionSlots: [],
    };
    
    for (let row = 1; row < rows && Object.keys(slots).length < unitCount; row++) {
      const leftSlotId = this.generateSlotId();
      const rightSlotId = this.generateSlotId();
      
      slots[leftSlotId] = {
        id: leftSlotId,
        localPosition: { 
          x: -row * spacing.horizontal, 
          y: 0, 
          z: row * spacing.depth 
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: row > 2,
        distanceFromLeader: row,
        connectionSlots: [leaderSlotId],
      };
      
      slots[rightSlotId] = {
        id: rightSlotId,
        localPosition: { 
          x: row * spacing.horizontal, 
          y: 0, 
          z: row * spacing.depth 
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: row > 2,
        distanceFromLeader: row,
        connectionSlots: [leaderSlotId],
      };
      
      slots[leaderSlotId].connectionSlots.push(leftSlotId, rightSlotId);
    }
    
    return slots;
  }

  private generateBoxFormation(unitCount: number, spacing: SpacingConfig): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 4);
    const unitsPerSide = Math.ceil(rows / 4) * 2;
    let unitIndex = 0;
    
    const frontRow: string[] = [];
    const backRow: string[] = [];
    const leftColumn: string[] = [];
    const rightColumn: string[] = [];
    
    for (let i = 0; i < unitsPerSide && unitIndex < unitCount; i++) {
      const frontSlotId = this.generateSlotId();
      const x = (i - (unitsPerSide - 1) / 2) * spacing.horizontal;
      
      slots[frontSlotId] = {
        id: frontSlotId,
        localPosition: { x, y: 0, z: -unitsPerSide * spacing.depth / 2 },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: i === 0 || i === unitsPerSide - 1 ? AssignmentPriority.High : AssignmentPriority.Normal,
        isFlanking: i === 0,
        isSupport: false,
        distanceFromLeader: unitsPerSide,
        connectionSlots: [],
      };
      frontRow.push(frontSlotId);
      unitIndex++;
      
      if (unitIndex >= unitCount) break;
      
      const backSlotId = this.generateSlotId();
      slots[backSlotId] = {
        id: backSlotId,
        localPosition: { x, y: 0, z: unitsPerSide * spacing.depth / 2 },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: AssignmentPriority.Normal,
        isFlanking: false,
        isSupport: true,
        distanceFromLeader: unitsPerSide,
        connectionSlots: [],
      };
      backRow.push(backSlotId);
      unitIndex++;
      
      if (unitIndex >= unitCount) break;
      
      if (i > 0 && i < unitsPerSide - 1) {
        const leftSlotId = this.generateSlotId();
        slots[leftSlotId] = {
          id: leftSlotId,
          localPosition: { 
            x: -unitsPerSide * spacing.horizontal / 2, 
            y: 0, 
            z: (i - (unitsPerSide - 1) / 2) * spacing.depth 
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: AssignmentPriority.Normal,
          isFlanking: true,
          isSupport: true,
          distanceFromLeader: 1,
          connectionSlots: [],
        };
        leftColumn.push(leftSlotId);
        unitIndex++;
        
        if (unitIndex >= unitCount) break;
        
        const rightSlotId = this.generateSlotId();
        slots[rightSlotId] = {
          id: rightSlotId,
          localPosition: { 
            x: unitsPerSide * spacing.horizontal / 2, 
            y: 0, 
            z: (i - (unitsPerSide - 1) / 2) * spacing.depth 
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: AssignmentPriority.Normal,
          isFlanking: true,
          isSupport: true,
          distanceFromLeader: 1,
          connectionSlots: [],
        };
        rightColumn.push(rightSlotId);
        unitIndex++;
      }
    }
    
    const allRows = [...frontRow, ...backRow, ...leftColumn, ...rightColumn];
    for (const slotId of allRows) {
      for (const otherSlotId of allRows) {
        if (slotId !== otherSlotId && !slots[slotId].connectionSlots.includes(otherSlotId)) {
          const dist = this.calculateDistance(slots[slotId].localPosition, slots[otherSlotId].localPosition);
          if (dist < spacing.horizontal * 3) {
            slots[slotId].connectionSlots.push(otherSlotId);
          }
        }
      }
    }
    
    return slots;
  }

  private getConnectedSlots(slots: Record<string, FormationSlot>, slotId: string, type: string): string[] {
    const connections: string[] = [];
    const slot = slots[slotId];
    
    for (const [otherId, otherSlot] of Object.entries(slots)) {
      if (otherId === slotId) continue;
      
      const distance = this.calculateDistance(slot.localPosition, otherSlot.localPosition);
      
      if (distance < 5.0) {
        connections.push(otherId);
      }
    }
    
    return connections;
  }

  private calculateDistance(a: Position, b: Position): number {
    const dx = a.x - b.x;
    const dz = a.z - b.z;
    return Math.sqrt(dx * dx + dz * dz);
  }

  private findLeaderSlotId(slots: Record<string, FormationSlot>): string {
    let leaderSlotId = Object.keys(slots)[0];
    let minDistance = Infinity;
    
    for (const [slotId, slot] of Object.entries(slots)) {
      const distance = this.calculateDistance(slot.localPosition, { x: 0, y: 0, z: 0 });
      if (distance < minDistance) {
        minDistance = distance;
        leaderSlotId = slotId;
      }
    }
    
    return leaderSlotId;
  }

  private calculateFormationBounds(slots: Record<string, FormationSlot>): FormationBounds {
    let minX = Infinity, maxX = -Infinity;
    let minZ = Infinity, maxZ = -Infinity;
    
    for (const slot of Object.values(slots)) {
      minX = Math.min(minX, slot.localPosition.x);
      maxX = Math.max(maxX, slot.localPosition.x);
      minZ = Math.min(minZ, slot.localPosition.z);
      maxZ = Math.max(maxZ, slot.localPosition.z);
    }
    
    return {
      minX,
      maxX,
      minZ,
      maxZ,
      centerX: (minX + maxX) / 2,
      centerZ: (minZ + maxZ) / 2,
      width: maxX - minX,
      depth: maxZ - minZ,
    };
  }

  public assignUnit(formationId: string, unitId: string, slotId?: string): SlotAssignment | null {
    const formation = this.formations.get(formationId);
    if (!formation) {
      this.emitEvent('error', { message: 'Formation not found', formationId });
      return null;
    }
    
    const unit = this.units.get(unitId);
    if (!unit) {
      this.emitEvent('error', { message: 'Unit not found', unitId });
      return null;
    }
    
    let targetSlot: FormationSlot | null = null;
    
    if (slotId) {
      targetSlot = formation.slots.get(slotId) || null;
      if (targetSlot && targetSlot.status !== SlotStatus.Empty) {
        this.emitEvent('error', { message: 'Slot is not empty', slotId });
        return null;
      }
    } else {
      targetSlot = this.findBestAvailableSlot(formation, unit);
    }
    
    if (!targetSlot) {
      this.emitEvent('error', { message: 'No available slots', formationId });
      return null;
    }
    
    if (unit.slotId) {
      this.unassignUnit(unitId);
    }
    
    targetSlot.status = SlotStatus.Occupied;
    targetSlot.assignedUnitId = unitId;
    formation.currentUnitCount++;
    
    unit.formationId = formationId;
    unit.slotId = targetSlot.id;
    
    const assignment: SlotAssignment = {
      unitId,
      slotId: targetSlot.id,
      priority: targetSlot.priority,
      assignedAt: Date.now(),
      distance: targetSlot.distanceFromLeader,
    };
    
    formation.lastModified = Date.now();
    this.emitEvent('unitAssigned', { formationId, unitId, slotId: targetSlot.id });
    
    return assignment;
  }

  public unassignUnit(unitId: string): boolean {
    const unit = this.units.get(unitId);
    if (!unit || !unit.formationId || !unit.slotId) {
      return false;
    }
    
    const formation = this.formations.get(unit.formationId);
    if (!formation) {
      return false;
    }
    
    const slot = formation.slots.get(unit.slotId);
    if (slot) {
      slot.status = SlotStatus.Empty;
      slot.assignedUnitId = null;
    }
    
    formation.currentUnitCount--;
    unit.formationId = null;
    unit.slotId = null;
    
    formation.lastModified = Date.now();
    this.emitEvent('unitUnassigned', { formationId: formation.id, unitId });
    
    return true;
  }

  private findBestAvailableSlot(formation: Formation, unit: Unit): FormationSlot | null {
    const availableSlots = Array.from(formation.slots.values())
      .filter(slot => slot.status === SlotStatus.Empty)
      .sort((a, b) => {
        if (a.priority !== b.priority) {
          return a.priority - b.priority;
        }
        return a.distanceFromLeader - b.distanceFromLeader;
      });
    
    return availableSlots[0] || null;
  }

  public getSlotWorldPosition(formationId: string, slotId: string): Position | null {
    const formation = this.formations.get(formationId);
    if (!formation) return null;
    
    const slot = formation.slots.get(slotId);
    if (!slot) return null;
    
    const rotatedPos = this.rotatePosition(slot.localPosition, formation.rotation);
    
    return {
      x: formation.anchorPosition.x + rotatedPos.x,
      y: formation.anchorPosition.y + rotatedPos.y,
      z: formation.anchorPosition.z + rotatedPos.z,
    };
  }

  private rotatePosition(pos: Position, angle: number): Position {
    const cos = Math.cos(angle);
    const sin = Math.sin(angle);
    
    return {
      x: pos.x * cos - pos.z * sin,
      y: pos.y,
      z: pos.x * sin + pos.z * cos,
    };
  }

  public moveFormation(formationId: string, targetPosition: Position): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    const deltaX = targetPosition.x - formation.anchorPosition.x;
    const deltaZ = targetPosition.z - formation.anchorPosition.z;
    
    formation.anchorPosition = { ...targetPosition };
    formation.state = FormationState.Moving;
    formation.lastModified = Date.now();
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId) {
        const worldPos = this.getSlotWorldPosition(formationId, unit.slotId!);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
    
    this.emitEvent('formationMoved', { formationId, targetPosition });
  }

  public rotateFormation(formationId: string, angle: number): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    formation.rotation = angle;
    formation.lastModified = Date.now();
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId) {
        const worldPos = this.getSlotWorldPosition(formationId, unit.slotId!);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
    
    this.emitEvent('formationRotated', { formationId, angle });
  }

  public morphFormation(formationId: string, targetType: FormationType, preserveAssignments: boolean = true): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    const unitCount = formation.currentUnitCount;
    const anchorPos = { ...formation.anchorPosition };
    const oldRotation = formation.rotation;
    
    const newFormation = this.createFormation(targetType, formation.maxUnits, anchorPos, formation.spacing, formation.style);
    
    this.rotateFormation(newFormation.id, oldRotation);
    
    if (preserveAssignments) {
      this.transferAssignments(formation, newFormation);
    }
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId) {
        unit.formationId = newFormation.id;
        const worldPos = this.getSlotWorldPosition(newFormation.id, unit.slotId!);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
    
    this.deleteFormation(formationId);
    
    this.emitEvent('formationMorphed', { 
      oldFormationId: formationId, 
      newFormationId: newFormation.id, 
      targetType 
    });
  }

  private transferAssignments(fromFormation: Formation, toFormation: Formation): void {
    const fromSlots = Array.from(fromFormation.slots.values());
    
    for (const fromSlot of fromSlots) {
      if (fromSlot.assignedUnitId && toFormation.currentUnitCount < toFormation.maxUnits) {
        const availableSlots = Array.from(toFormation.slots.values())
          .filter(slot => slot.status === SlotStatus.Empty)
          .sort((a, b) => Math.abs(a.distanceFromLeader - fromSlot.distanceFromLeader) - Math.abs(b.distanceFromLeader - fromSlot.distanceFromLeader));
        
        if (availableSlots.length > 0) {
          const toSlot = availableSlots[0];
          toSlot.status = SlotStatus.Occupied;
          toSlot.assignedUnitId = fromSlot.assignedUnitId;
          toFormation.currentUnitCount++;
          
          const unit = this.units.get(fromSlot.assignedUnitId);
          if (unit) {
            unit.slotId = toSlot.id;
          }
          
          fromSlot.status = SlotStatus.Empty;
          fromSlot.assignedUnitId = null;
          fromFormation.currentUnitCount--;
        }
      }
    }
  }

  public breakFormation(formationId: string, scatterRadius: number = 10): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    formation.state = FormationState.Breaking;
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId) {
        const angle = Math.random() * Math.PI * 2;
        const distance = Math.random() * scatterRadius;
        
        unit.targetPosition = {
          x: unit.position.x + Math.cos(angle) * distance,
          y: unit.position.y,
          z: unit.position.z + Math.sin(angle) * distance,
        };
        
        unit.formationId = null;
        unit.slotId = null;
      }
    }
    
    formation.state = FormationState.Scattered;
    this.emitEvent('formationBroken', { formationId, scatterRadius });
  }

  public reformFormation(formationId: string): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    formation.state = FormationState.reforming;
    
    const assignedUnits = Array.from(this.units.values())
      .filter(unit => unit.formationId === formationId);
    
    for (const unit of assignedUnits) {
      const worldPos = this.getSlotWorldPosition(formationId, unit.slotId!);
      if (worldPos) {
        unit.targetPosition = worldPos;
      }
    }
    
    formation.state = FormationState.Idle;
    this.emitEvent('formationReformed', { formationId });
  }

  public deleteFormation(formationId: string): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId) {
        unit.formationId = null;
        unit.slotId = null;
      }
    }
    
    this.formations.delete(formationId);
    this.emitEvent('formationDeleted', { formationId });
  }

  public registerUnit(unit: Unit): void {
    this.units.set(unit.id, unit);
    this.emitEvent('unitRegistered', { unitId: unit.id });
  }

  public unregisterUnit(unitId: string): void {
    this.unassignUnit(unitId);
    this.units.delete(unitId);
    this.emitEvent('unitUnregistered', { unitId });
  }

  public getFormation(formationId: string): Formation | undefined {
    return this.formations.get(formationId);
  }

  public getUnit(unitId: string): Unit | undefined {
    return this.units.get(unitId);
  }

  public getFormationsByType(type: FormationType): Formation[] {
    return Array.from(this.formations.values()).filter(f => f.type === type);
  }

  public getUnitsInFormation(formationId: string): Unit[] {
    return Array.from(this.units.values()).filter(u => u.formationId === formationId);
  }

  public updateUnitPositions(deltaTime: number): void {
    for (const [unitId, unit] of this.units) {
      if (unit.formationId && unit.slotId) {
        const formation = this.formations.get(unit.formationId);
        if (formation && formation.state === FormationState.Moving) {
          const worldPos = this.getSlotWorldPosition(unit.formationId, unit.slotId);
          if (worldPos) {
            const dx = worldPos.x - unit.position.x;
            const dz = worldPos.z - unit.position.z;
            const distance = Math.sqrt(dx * dx + dz * dz);
            
            if (distance > 0.1) {
              const speed = Math.min(unit.speed * deltaTime, distance);
              unit.position.x += (dx / distance) * speed;
              unit.position.z += (dz / distance) * speed;
            }
          }
        }
      }
    }
  }

  public maintainFormation(formationId: string, tolerance: number = 2.0): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    let needsCorrection = false;
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId && unit.slotId) {
        const worldPos = this.getSlotWorldPosition(formationId, unit.slotId);
        if (worldPos) {
          const dx = worldPos.x - unit.position.x;
          const dz = worldPos.z - unit.position.z;
          const distance = Math.sqrt(dx * dx + dz * dz);
          
          if (distance > tolerance) {
            unit.targetPosition = worldPos;
            needsCorrection = true;
          }
        }
      }
    }
    
    if (needsCorrection) {
      this.emitEvent('formationCorrected', { formationId, tolerance });
    }
  }

  public splitFormation(formationId: string, splitPoint: Position): { formation1: Formation; formation2: Formation } | null {
    const formation = this.formations.get(formationId);
    if (!formation) return null;
    
    const units = this.getUnitsInFormation(formationId);
    const splitUnits = units.filter(u => u.position.x < splitPoint.x);
    const remainingUnits = units.filter(u => u.position.x >= splitPoint.x);
    
    if (splitUnits.length === 0 || remainingUnits.length === 0) {
      return null;
    }
    
    const formation1 = this.createFormation(
      formation.type,
      splitUnits.length,
      { x: splitPoint.x - 10, y: 0, z: splitPoint.z },
      formation.spacing,
      formation.style
    );
    
    const formation2 = this.createFormation(
      formation.type,
      remainingUnits.length,
      { x: splitPoint.x + 10, y: 0, z: splitPoint.z },
      formation.spacing,
      formation.style
    );
    
    for (const unit of splitUnits) {
      this.assignUnit(formation1.id, unit.id);
    }
    
    for (const unit of remainingUnits) {
      this.assignUnit(formation2.id, unit.id);
    }
    
    this.deleteFormation(formationId);
    
    this.emitEvent('formationSplit', { 
      originalFormationId: formationId, 
      formation1Id: formation1.id, 
      formation2Id: formation2.id 
    });
    
    return { formation1, formation2 };
  }

  public mergeFormations(formationId1: string, formationId2: string, targetPosition: Position): Formation | null {
    const formation1 = this.formations.get(formationId1);
    const formation2 = this.formations.get(formationId2);
    
    if (!formation1 || !formation2) return null;
    
    const totalUnits = formation1.currentUnitCount + formation2.currentUnitCount;
    const mergedFormation = this.createFormation(
      formation1.type,
      totalUnits,
      targetPosition,
      formation1.spacing,
      formation1.style
    );
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId1 || unit.formationId === formationId2) {
        this.assignUnit(mergedFormation.id, unitId);
      }
    }
    
    this.deleteFormation(formationId1);
    this.deleteFormation(formationId2);
    
    this.emitEvent('formationsMerged', { 
      formation1Id: formationId1, 
      formation2Id: formationId2, 
      mergedFormationId: mergedFormation.id 
    });
    
    return mergedFormation;
  }

  public adjustSpacing(formationId: string, newSpacing: SpacingConfig): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    formation.spacing = { ...newSpacing };
    
    const generator = this.formationGenerators.get(formation.type);
    if (generator) {
      const newSlots = generator(formation.maxUnits, newSpacing);
      formation.slots = new Map(Object.entries(newSlots));
      formation.bounds = this.calculateFormationBounds(newSlots);
    }
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId && unit.slotId) {
        const worldPos = this.getSlotWorldPosition(formationId, unit.slotId);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
    
    formation.lastModified = Date.now();
    this.emitEvent('spacingAdjusted', { formationId, newSpacing });
  }

  public setFormationStyle(formationId: string, style: FormationStyle): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    formation.style = { ...style };
    formation.lastModified = Date.now();
    
    this.emitEvent('styleChanged', { formationId, style });
  }

  public on(eventType: string, callback: Function): void {
    if (!this.eventListeners.has(eventType)) {
      this.eventListeners.set(eventType, []);
    }
    this.eventListeners.get(eventType)!.push(callback);
  }

  public off(eventType: string, callback: Function): void {
    const listeners = this.eventListeners.get(eventType);
    if (listeners) {
      const index = listeners.indexOf(callback);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    }
  }

  private emitEvent(eventType: string, data: any): void {
    const listeners = this.eventListeners.get(eventType);
    if (listeners) {
      for (const listener of listeners) {
        listener({ type: eventType, timestamp: Date.now(), data });
      }
    }
  }

  public dispose(): void {
    this.formations.clear();
    this.units.clear();
    this.eventListeners.clear();
    this.transitionQueue = [];
    this.activeTransitions.clear();
  }
}

export class LineFormation {
  private spacing: SpacingConfig;
  
  constructor(spacing: SpacingConfig) {
    this.spacing = spacing;
  }
  
  public generateLayout(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 10);
    const unitsPerRow = Math.ceil(unitCount / rows);
    
    for (let row = 0; row < rows; row++) {
      for (let col = 0; col < unitsPerRow; col++) {
        const index = row * unitsPerRow + col;
        if (index >= unitCount) break;
        
        const slotId = `line_${row}_${col}`;
        const x = (col - (unitsPerRow - 1) / 2) * this.spacing.horizontal;
        const z = row * this.spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 ? AssignmentPriority.Highest : AssignmentPriority.Normal,
          isFlanking: col === 0 || col === unitsPerRow - 1,
          isSupport: row > 0,
          distanceFromLeader: row,
          connectionSlots: [],
        };
      }
    }
    
    return slots;
  }
  
  public getFrontLineUnits(slots: Record<string, FormationSlot>): FormationSlot[] {
    return Object.values(slots).filter(s => s.distanceFromLeader === 0);
  }
  
  public validateLine(slots: Record<string, FormationSlot>): boolean {
    const frontSlots = this.getFrontLineUnits(slots);
    if (frontSlots.length === 0) return false;
    
    const frontZ = frontSlots[0].localPosition.z;
    return Object.values(slots).every(s => Math.abs(s.localPosition.z - frontZ) <= this.spacing.depth);
  }
}

export class ColumnFormation {
  private spacing: SpacingConfig;
  
  constructor(spacing: SpacingConfig) {
    this.spacing = spacing;
  }
  
  public generateLayout(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const unitsPerColumn = Math.ceil(unitCount / 3);
    
    for (let col = 0; col < 3; col++) {
      for (let row = 0; row < unitsPerColumn; row++) {
        const index = col * unitsPerColumn + row;
        if (index >= unitCount) break;
        
        const slotId = `col_${col}_${row}`;
        const x = (col - 1) * this.spacing.horizontal;
        const z = row * this.spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 && col === 1 ? AssignmentPriority.Highest : AssignmentPriority.Normal,
          isFlanking: col === 0 || col === 2,
          isSupport: col === 0 || col === 2,
          distanceFromLeader: row,
          connectionSlots: [],
        };
      }
    }
    
    return slots;
  }
  
  public getColumnDepth(slots: Record<string, FormationSlot>): number {
    const maxZ = Math.max(...Object.values(slots).map(s => s.localPosition.z));
    return maxZ / this.spacing.depth + 1;
  }
}

export class WedgeFormation {
  private spacing: SpacingConfig;
  
  constructor(spacing: SpacingConfig) {
    this.spacing = spacing;
  }
  
  public generateLayout(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(Math.sqrt(unitCount * 2));
    
    let unitIndex = 0;
    for (let row = 0; row < rows && unitIndex < unitCount; row++) {
      const unitsInRow = row + 1;
      for (let col = 0; col < unitsInRow && unitIndex < unitCount; col++) {
        const slotId = `wedge_${row}_${col}`;
        const x = (col - (unitsInRow - 1) / 2) * this.spacing.horizontal;
        const z = row * this.spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 ? AssignmentPriority.Highest : AssignmentPriority.Normal - row,
          isFlanking: col === 0 || col === unitsInRow - 1,
          isSupport: row > 2,
          distanceFromLeader: row,
          connectionSlots: [],
        };
        
        unitIndex++;
      }
    }
    
    return slots;
  }
  
  public getTipUnit(slots: Record<string, FormationSlot>): FormationSlot | null {
    const tipSlots = Object.values(slots).filter(s => s.distanceFromLeader === 0);
    return tipSlots.length > 0 ? tipSlots[0] : null;
  }
  
  public getFlankingUnits(slots: Record<string, FormationSlot>): FormationSlot[] {
    return Object.values(slots).filter(s => s.isFlanking);
  }
}

export class CircleFormation {
  private spacing: SpacingConfig;
  
  constructor(spacing: SpacingConfig) {
    this.spacing = spacing;
  }
  
  public generateLayout(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    
    const centerSlotId = 'circle_center';
    slots[centerSlotId] = {
      id: centerSlotId,
      localPosition: { x: 0, y: 0, z: 0 },
      status: SlotStatus.Empty,
      assignedUnitId: null,
      priority: AssignmentPriority.Highest,
      isFlanking: false,
      isSupport: true,
      distanceFromLeader: 0,
      connectionSlots: [],
    };
    
    const ringUnits = unitCount - 1;
    for (let i = 0; i < ringUnits; i++) {
      const angle = (i / ringUnits) * Math.PI * 2;
      const radius = this.spacing.horizontal * 2;
      const slotId = `circle_ring_${i}`;
      
      slots[slotId] = {
        id: slotId,
        localPosition: { 
          x: Math.cos(angle) * radius, 
          y: 0, 
          z: Math.sin(angle) * radius 
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: i % 2 === 0 ? AssignmentPriority.High : AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: false,
        distanceFromLeader: 1,
        connectionSlots: [centerSlotId],
      };
      
      slots[centerSlotId].connectionSlots.push(slotId);
    }
    
    return slots;
  }
  
  public getDefenders(slots: Record<string, FormationSlot>): FormationSlot[] {
    return Object.values(slots).filter(s => s.isSupport);
  }
  
  public getOuterRing(slots: Record<string, FormationSlot>): FormationSlot[] {
    return Object.values(slots).filter(s => s.distanceFromLeader === 1);
  }
}

export class SquareFormation {
  private spacing: SpacingConfig;
  
  constructor(spacing: SpacingConfig) {
    this.spacing = spacing;
  }
  
  public generateLayout(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const sideLength = Math.ceil(Math.sqrt(unitCount));
    
    let unitIndex = 0;
    for (let side = 0; side < 4 && unitIndex < unitCount; side++) {
      for (let i = 0; i < sideLength && unitIndex < unitCount; i++) {
        let x = 0, z = 0;
        
        switch (side) {
          case 0: x = i; z = 0; break;
          case 1: x = sideLength - 1; z = i; break;
          case 2: x = sideLength - 1 - i; z = sideLength - 1; break;
          case 3: x = 0; z = sideLength - 1 - i; break;
        }
        
        const slotId = `square_${side}_${i}`;
        const isCorner = (i === 0) || (i === sideLength - 1);
        
        slots[slotId] = {
          id: slotId,
          localPosition: { 
            x: (x - (sideLength - 1) / 2) * this.spacing.horizontal, 
            y: 0, 
            z: (z - (sideLength - 1) / 2) * this.spacing.depth 
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: isCorner ? AssignmentPriority.High : AssignmentPriority.Normal,
          isFlanking: isCorner,
          isSupport: !isCorner && side % 2 === 1,
          distanceFromLeader: Math.min(x, z, sideLength - 1 - x, sideLength - 1 - z),
          connectionSlots: [],
        };
        
        unitIndex++;
      }
    }
    
    return slots;
  }
  
  public getCorners(slots: Record<string, FormationSlot>): FormationSlot[] {
    return Object.values(slots).filter(s => s.isFlanking);
  }
}

export class EchelonFormation {
  private spacing: SpacingConfig;
  
  constructor(spacing: SpacingConfig) {
    this.spacing = spacing;
  }
  
  public generateLayout(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 5);
    const unitsPerRow = Math.ceil(unitCount / rows);
    
    for (let row = 0; row < rows; row++) {
      const rowOffset = row * this.spacing.horizontal * 0.5;
      for (let col = 0; col < unitsPerRow; col++) {
        const index = row * unitsPerRow + col;
        if (index >= unitCount) break;
        
        const slotId = `echelon_${row}_${col}`;
        const x = col * this.spacing.horizontal + rowOffset - (unitsPerRow * this.spacing.horizontal) / 2;
        const z = row * this.spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: col === 0 ? AssignmentPriority.High : AssignmentPriority.Normal,
          isFlanking: col === unitsPerRow - 1,
          isSupport: false,
          distanceFromLeader: row,
          connectionSlots: [],
        };
      }
    }
    
    return slots;
  }
  
  public getLeadingEdge(slots: Record<string, FormationSlot>): FormationSlot[] {
    const maxCol = Math.max(...Object.values(slots).map(s => s.localPosition.x));
    return Object.values(slots).filter(s => s.localPosition.x === maxCol);
  }
}

export class DynamicFormation {
  private spacing: SpacingConfig;
  private adaptationThreshold: number;
  
  constructor(spacing: SpacingConfig, adaptationThreshold: number = 5.0) {
    this.spacing = spacing;
    this.adaptationThreshold = adaptationThreshold;
  }
  
  public generateAdaptiveLayout(
    unitCount: number, 
    enemyPositions: Position[], 
    terrainData: any
  ): Record<string, FormationSlot> {
    if (enemyPositions.length === 0) {
      return this.generateDefaultLayout(unitCount);
    }
    
    const centerOfMass = this.calculateCenterOfMass(enemyPositions);
    const threatLevel = this.assessThreatLevel(enemyPositions);
    
    if (threatLevel > 0.7) {
      return this.generateDefensiveLayout(unitCount, centerOfMass);
    } else if (threatLevel < 0.3) {
      return this.generateAggressiveLayout(unitCount, centerOfMass);
    } else {
      return this.generateBalancedLayout(unitCount, centerOfMass);
    }
  }
  
  private calculateCenterOfMass(positions: Position[]): Position {
    const sum = positions.reduce(
      (acc, pos) => ({ x: acc.x + pos.x, z: acc.z + pos.z }),
      { x: 0, z: 0 }
    );
    return {
      x: sum.x / positions.length,
      y: 0,
      z: sum.z / positions.length,
    };
  }
  
  private assessThreatLevel(positions: Position[]): number {
    const avgDistance = positions.reduce((sum, pos) => sum + Math.sqrt(pos.x * pos.x + pos.z * pos.z), 0) / positions.length;
    return Math.min(1.0, Math.max(0.0, 1.0 - avgDistance / 50.0));
  }
  
  private generateDefaultLayout(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const sideLength = Math.ceil(Math.sqrt(unitCount));
    
    let index = 0;
    for (let x = 0; x < sideLength && index < unitCount; x++) {
      for (let z = 0; z < sideLength && index < unitCount; z++) {
        const slotId = `dynamic_${x}_${z}`;
        slots[slotId] = {
          id: slotId,
          localPosition: { 
            x: (x - (sideLength - 1) / 2) * this.spacing.horizontal, 
            y: 0, 
            z: (z - (sideLength - 1) / 2) * this.spacing.depth 
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: AssignmentPriority.Normal,
          isFlanking: x === 0 || x === sideLength - 1 || z === 0 || z === sideLength - 1,
          isSupport: false,
          distanceFromLeader: Math.min(x, z, sideLength - 1 - x, sideLength - 1 - z),
          connectionSlots: [],
        };
        index++;
      }
    }
    
    return slots;
  }
  
  private generateDefensiveLayout(unitCount: number, enemyCenter: Position): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const innerRadius = this.spacing.horizontal * 2;
    const outerRadius = innerRadius + this.spacing.horizontal * 2;
    
    const centerSlotId = 'dynamic_defense_center';
    slots[centerSlotId] = {
      id: centerSlotId,
      localPosition: { x: 0, y: 0, z: 0 },
      status: SlotStatus.Empty,
      assignedUnitId: null,
      priority: AssignmentPriority.Highest,
      isFlanking: false,
      isSupport: true,
      distanceFromLeader: 0,
      connectionSlots: [],
    };
    
    const outerUnits = unitCount - 1;
    for (let i = 0; i < outerUnits; i++) {
      const angle = (i / outerUnits) * Math.PI * 2;
      const radius = outerRadius;
      const slotId = `dynamic_defense_${i}`;
      
      slots[slotId] = {
        id: slotId,
        localPosition: { 
          x: Math.cos(angle) * radius, 
          y: 0, 
          z: Math.sin(angle) * radius 
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: false,
        distanceFromLeader: 1,
        connectionSlots: [centerSlotId],
      };
    }
    
    return slots;
  }
  
  private generateAggressiveLayout(unitCount: number, enemyCenter: Position): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(Math.sqrt(unitCount * 2));
    
    let unitIndex = 0;
    for (let row = 0; row < rows && unitIndex < unitCount; row++) {
      const unitsInRow = row + 1;
      for (let col = 0; col < unitsInRow && unitIndex < unitCount; col++) {
        const slotId = `dynamic_agg_${row}_${col}`;
        const x = (col - (unitsInRow - 1) / 2) * this.spacing.horizontal;
        const z = row * this.spacing.depth;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { x, y: 0, z },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 ? AssignmentPriority.Highest : AssignmentPriority.Normal,
          isFlanking: col === 0 || col === unitsInRow - 1,
          isSupport: row > 2,
          distanceFromLeader: row,
          connectionSlots: [],
        };
        
        unitIndex++;
      }
    }
    
    return slots;
  }
  
  private generateBalancedLayout(unitCount: number, enemyCenter: Position): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const sideLength = Math.ceil(Math.sqrt(unitCount));
    
    let index = 0;
    for (let x = 0; x < sideLength && index < unitCount; x++) {
      for (let z = 0; z < sideLength && index < unitCount; z++) {
        const slotId = `dynamic_bal_${x}_${z}`;
        const isEdge = x === 0 || x === sideLength - 1 || z === 0 || z === sideLength - 1;
        
        slots[slotId] = {
          id: slotId,
          localPosition: { 
            x: (x - (sideLength - 1) / 2) * this.spacing.horizontal, 
            y: 0, 
            z: (z - (sideLength - 1) / 2) * this.spacing.depth 
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: isEdge ? AssignmentPriority.High : AssignmentPriority.Normal,
          isFlanking: isEdge,
          isSupport: !isEdge,
          distanceFromLeader: Math.min(x, z, sideLength - 1 - x, sideLength - 1 - z),
          connectionSlots: [],
        };
        index++;
      }
    }
    
    return slots;
  }
  
  public adaptFormation(
    slots: Record<string, FormationSlot>,
    newEnemyPositions: Position[]
  ): Record<string, FormationSlot> {
    if (newEnemyPositions.length === 0) {
      return slots;
    }
    
    const centerOfMass = this.calculateCenterOfMass(newEnemyPositions);
    const distance = Math.sqrt(centerOfMass.x * centerOfMass.x + centerOfMass.z * centerOfMass.z);
    
    if (distance > this.adaptationThreshold) {
      return this.generateDefaultLayout(Object.keys(slots).length);
    }
    
    return slots;
  }
}

export class FormationEditor {
  private slots: Record<string, FormationSlot>;
  private spacing: SpacingConfig;
  
  constructor(spacing: SpacingConfig) {
    this.slots = {};
    this.spacing = spacing;
  }
  
  public addSlot(position: Position, priority: AssignmentPriority = AssignmentPriority.Normal): FormationSlot {
    const slotId = `custom_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    
    this.slots[slotId] = {
      id: slotId,
      localPosition: { ...position },
      status: SlotStatus.Empty,
      assignedUnitId: null,
      priority,
      isFlanking: false,
      isSupport: false,
      distanceFromLeader: 0,
      connectionSlots: [],
    };
    
    return this.slots[slotId];
  }
  
  public removeSlot(slotId: string): boolean {
    if (this.slots[slotId]) {
      delete this.slots[slotId];
      return true;
    }
    return false;
  }
  
  public updateSlotPosition(slotId: string, position: Position): boolean {
    const slot = this.slots[slotId];
    if (slot) {
      slot.localPosition = { ...position };
      return true;
    }
    return false;
  }
  
  public setSlotPriority(slotId: string, priority: AssignmentPriority): boolean {
    const slot = this.slots[slotId];
    if (slot) {
      slot.priority = priority;
      return true;
    }
    return false;
  }
  
  public addConnection(slotId1: string, slotId2: string): boolean {
    const slot1 = this.slots[slotId1];
    const slot2 = this.slots[slotId2];
    
    if (slot1 && slot2) {
      if (!slot1.connectionSlots.includes(slotId2)) {
        slot1.connectionSlots.push(slotId2);
      }
      if (!slot2.connectionSlots.includes(slotId1)) {
        slot2.connectionSlots.push(slotId1);
      }
      return true;
    }
    return false;
  }
  
  public removeConnection(slotId1: string, slotId2: string): boolean {
    const slot1 = this.slots[slotId1];
    const slot2 = this.slots[slotId2];
    
    if (slot1 && slot2) {
      slot1.connectionSlots = slot1.connectionSlots.filter(id => id !== slotId2);
      slot2.connectionSlots = slot2.connectionSlots.filter(id => id !== slotId1);
      return true;
    }
    return false;
  }
  
  public mirrorFormation(axis: 'x' | 'z'): void {
    for (const slot of Object.values(this.slots)) {
      if (axis === 'x') {
        slot.localPosition.x = -slot.localPosition.x;
      } else {
        slot.localPosition.z = -slot.localPosition.z;
      }
    }
  }
  
  public rotateFormation(angle: number): void {
    const cos = Math.cos(angle);
    const sin = Math.sin(angle);
    
    for (const slot of Object.values(this.slots)) {
      const x = slot.localPosition.x * cos - slot.localPosition.z * sin;
      const z = slot.localPosition.x * sin + slot.localPosition.z * cos;
      slot.localPosition.x = x;
      slot.localPosition.z = z;
    }
  }
  
  public scaleFormation(scale: number): void {
    for (const slot of Object.values(this.slots)) {
      slot.localPosition.x *= scale;
      slot.localPosition.z *= scale;
    }
  }
  
  public getSlots(): Record<string, FormationSlot> {
    return { ...this.slots };
  }
  
  public clear(): void {
    this.slots = {};
  }
  
  public optimizeSpacing(): void {
    const slotList = Object.values(this.slots);
    
    for (let i = 0; i < slotList.length; i++) {
      for (let j = i + 1; j < slotList.length; j++) {
        const slot1 = slotList[i];
        const slot2 = slotList[j];
        
        const dx = slot2.localPosition.x - slot1.localPosition.x;
        const dz = slot2.localPosition.z - slot1.localPosition.z;
        const distance = Math.sqrt(dx * dx + dz * dz);
        
        if (distance < this.spacing.unitRadius * 2 + this.spacing.safeZone) {
          const overlap = this.spacing.unitRadius * 2 + this.spacing.safeZone - distance;
          const factor = overlap / 2 + 0.1;
          
          if (dx !== 0) {
            slot2.localPosition.x += (dx / distance) * factor;
          }
          if (dz !== 0) {
            slot2.localPosition.z += (dz / distance) * factor;
          }
        }
      }
    }
  }
  
  public validateFormation(): { valid: boolean; errors: string[] } {
    const errors: string[] = [];
    const slotList = Object.values(this.slots);
    
    if (slotList.length === 0) {
      errors.push('Formation has no slots');
      return { valid: false, errors };
    }
    
    const hasLeader = slotList.some(s => s.priority === AssignmentPriority.Highest);
    if (!hasLeader) {
      errors.push('Formation has no leader slot (Highest priority)');
    }
    
    for (const slot of slotList) {
      for (const otherSlot of slotList) {
        if (slot.id === otherSlot.id) continue;
        
        const dx = otherSlot.localPosition.x - slot.localPosition.x;
        const dz = otherSlot.localPosition.z - slot.localPosition.z;
        const distance = Math.sqrt(dx * dx + dz * dz);
        
        if (distance < this.spacing.unitRadius * 2) {
          errors.push(`Slots ${slot.id} and ${otherSlot.id} are overlapping`);
        }
      }
    }
    
    return { valid: errors.length === 0, errors };
  }
}

export class FormationMaintainer {
  private formation: Formation;
  private tolerance: number;
  private correctionSpeed: number;
  private maxCorrectionDistance: number;
  
  constructor(formation: Formation, tolerance: number = 2.0, correctionSpeed: number = 5.0) {
    this.formation = formation;
    this.tolerance = tolerance;
    this.correctionSpeed = correctionSpeed;
    this.maxCorrectionDistance = 20.0;
  }
  
  public update(units: Map<string, Unit>, deltaTime: number): void {
    for (const [unitId, unit] of units) {
      if (unit.formationId !== this.formation.id || !unit.slotId) continue;
      
      const expectedPosition = this.getSlotWorldPosition(unit.slotId);
      if (!expectedPosition) continue;
      
      const dx = expectedPosition.x - unit.position.x;
      const dz = expectedPosition.z - unit.position.z;
      const distance = Math.sqrt(dx * dx + dz * dz);
      
      if (distance > this.tolerance && distance < this.maxCorrectionDistance) {
        const correctionAmount = Math.min(this.correctionSpeed * deltaTime, distance);
        unit.targetPosition = {
          x: unit.position.x + (dx / distance) * correctionAmount,
          y: expectedPosition.y,
          z: unit.position.z + (dz / distance) * correctionAmount,
        };
      }
    }
  }
  
  private getSlotWorldPosition(slotId: string): Position | null {
    const slot = this.formation.slots.get(slotId);
    if (!slot) return null;
    
    const cos = Math.cos(this.formation.rotation);
    const sin = Math.sin(this.formation.rotation);
    
    const rotatedX = slot.localPosition.x * cos - slot.localPosition.z * sin;
    const rotatedZ = slot.localPosition.x * sin + slot.localPosition.z * cos;
    
    return {
      x: this.formation.anchorPosition.x + rotatedX,
      y: this.formation.anchorPosition.y + slot.localPosition.y,
      z: this.formation.anchorPosition.z + rotatedZ,
    };
  }
  
  public setTolerance(tolerance: number): void {
    this.tolerance = tolerance;
  }
  
  public setCorrectionSpeed(speed: number): void {
    this.correctionSpeed = speed;
  }
  
  public getFormationIntegrity(): number {
    let totalDeviation = 0;
    let unitCount = 0;
    
    for (const [unitId, unit] of this.formation.slots) {
      if (unit.status === SlotStatus.Occupied) {
        const expectedPosition = this.getSlotWorldPosition(unitId);
        if (expectedPosition) {
          const dx = expectedPosition.x - unit.position.x;
          const dz = expectedPosition.z - unit.position.z;
          totalDeviation += Math.sqrt(dx * dx + dz * dz);
          unitCount++;
        }
      }
    }
    
    if (unitCount === 0) return 1.0;
    
    const avgDeviation = totalDeviation / unitCount;
    return Math.max(0, 1 - avgDeviation / this.maxCorrectionDistance);
  }
}

export class FormationController {
  private formation: Formation;
  private movementSpeed: number;
  private rotationSpeed: number;
  private pathfinding: any;
  
  constructor(formation: Formation, movementSpeed: number = 10.0, rotationSpeed: number = 2.0) {
    this.formation = formation;
    this.movementSpeed = movementSpeed;
    this.rotationSpeed = rotationSpeed;
    this.pathfinding = null;
  }
  
  public moveTo(targetPosition: Position, units: Map<string, Unit>): void {
    this.formation.anchorPosition = { ...targetPosition };
    this.formation.state = FormationState.Moving;
    
    for (const [unitId, unit] of units) {
      if (unit.formationId === this.formation.id && unit.slotId) {
        const worldPos = this.getSlotWorldPosition(unit.slotId);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
  }
  
  public moveAlongPath(path: Position[], units: Map<string, Unit>, deltaTime: number): boolean {
    if (path.length === 0) {
      this.formation.state = FormationState.Idle;
      return true;
    }
    
    const target = path[0];
    const dx = target.x - this.formation.anchorPosition.x;
    const dz = target.z - this.formation.anchorPosition.z;
    const distance = Math.sqrt(dx * dx + dz * dz);
    
    if (distance < 0.5) {
      path.shift();
      return this.moveAlongPath(path, units, deltaTime);
    }
    
    const moveDistance = this.movementSpeed * deltaTime;
    const actualMove = Math.min(moveDistance, distance);
    
    this.formation.anchorPosition.x += (dx / distance) * actualMove;
    this.formation.anchorPosition.z += (dz / distance) * actualMove;
    
    const targetAngle = Math.atan2(dx, dz);
    let angleDiff = targetAngle - this.formation.rotation;
    
    while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
    while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;
    
    const rotationAmount = Math.sign(angleDiff) * Math.min(Math.abs(angleDiff), this.rotationSpeed * deltaTime);
    this.formation.rotation += rotationAmount;
    
    for (const [unitId, unit] of units) {
      if (unit.formationId === this.formation.id && unit.slotId) {
        const worldPos = this.getSlotWorldPosition(unit.slotId);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
    
    return false;
  }
  
  public rotateTo(angle: number, units: Map<string, Unit>): void {
    let angleDiff = angle - this.formation.rotation;
    
    while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
    while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;
    
    this.formation.rotation += angleDiff;
    
    for (const [unitId, unit] of units) {
      if (unit.formationId === this.formation.id && unit.slotId) {
        const worldPos = this.getSlotWorldPosition(unit.slotId);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
  }
  
  public engage(targetPosition: Position, units: Map<string, Unit>): void {
    this.formation.state = FormationState.Engaging;
    
    const dx = targetPosition.x - this.formation.anchorPosition.x;
    const dz = targetPosition.z - this.formation.anchorPosition.z;
    const distance = Math.sqrt(dx * dx + dz * dz);
    
    const engagementDistance = this.formation.bounds.depth / 2 + 5;
    
    if (distance > engagementDistance) {
      this.moveTo(targetPosition, units);
    }
    
    const targetAngle = Math.atan2(dx, dz);
    this.rotateTo(targetAngle, units);
  }
  
  public holdPosition(units: Map<string, Unit>): void {
    this.formation.state = FormationState.Idle;
    
    for (const [unitId, unit] of units) {
      if (unit.formationId === this.formation.id && unit.slotId) {
        const worldPos = this.getSlotWorldPosition(unit.slotId);
        if (worldPos) {
          unit.targetPosition = worldPos;
        }
      }
    }
  }
  
  private getSlotWorldPosition(slotId: string): Position | null {
    const slot = this.formation.slots.get(slotId);
    if (!slot) return null;
    
    const cos = Math.cos(this.formation.rotation);
    const sin = Math.sin(this.formation.rotation);
    
    const rotatedX = slot.localPosition.x * cos - slot.localPosition.z * sin;
    const rotatedZ = slot.localPosition.x * sin + slot.localPosition.z * cos;
    
    return {
      x: this.formation.anchorPosition.x + rotatedX,
      y: this.formation.anchorPosition.y + slot.localPosition.y,
      z: this.formation.anchorPosition.z + rotatedZ,
    };
  }
  
  public setMovementSpeed(speed: number): void {
    this.movementSpeed = speed;
  }
  
  public setRotationSpeed(speed: number): void {
    this.rotationSpeed = speed;
  }
  
  public getMovementProgress(targetPosition: Position): number {
    const dx = targetPosition.x - this.formation.anchorPosition.x;
    const dz = targetPosition.z - this.formation.anchorPosition.z;
    const totalDistance = Math.sqrt(dx * dx + dz * dz);
    
    return totalDistance;
  }
  
  public isMoving(): boolean {
    return this.formation.state === FormationState.Moving;
  }
  
  public isEngaging(): boolean {
    return this.formation.state === FormationState.Engaging;
  }
}

export class FormationManager {
  private formations: Map<string, Formation>;
  private units: Map<string, Unit>;
  private maxFormations: number;
  private autoReform: boolean;
  private reformThreshold: number;
  
  constructor(maxFormations: number = 50) {
    this.formations = new Map();
    this.units = new Map();
    this.maxFormations = maxFormations;
    this.autoReform = true;
    this.reformThreshold = 0.5;
  }
  
  public addFormation(formation: Formation): boolean {
    if (this.formations.size >= this.maxFormations) {
      return false;
    }
    this.formations.set(formation.id, formation);
    return true;
  }
  
  public removeFormation(formationId: string): boolean {
    const formation = this.formations.get(formationId);
    if (!formation) return false;
    
    for (const [unitId, unit] of this.units) {
      if (unit.formationId === formationId) {
        unit.formationId = null;
        unit.slotId = null;
      }
    }
    
    this.formations.delete(formationId);
    return true;
  }
  
  public getFormation(formationId: string): Formation | undefined {
    return this.formations.get(formationId);
  }
  
  public getAllFormations(): Formation[] {
    return Array.from(this.formations.values());
  }
  
  public registerUnit(unit: Unit): void {
    this.units.set(unit.id, unit);
  }
  
  public unregisterUnit(unitId: string): void {
    const unit = this.units.get(unitId);
    if (unit) {
      if (unit.formationId) {
        const formation = this.formations.get(unit.formationId);
        if (formation && unit.slotId) {
          const slot = formation.slots.get(unit.slotId);
          if (slot) {
            slot.status = SlotStatus.Empty;
            slot.assignedUnitId = null;
            formation.currentUnitCount--;
          }
        }
      }
      this.units.delete(unitId);
    }
  }
  
  public getUnit(unitId: string): Unit | undefined {
    return this.units.get(unitId);
  }
  
  public getUnitsInFormation(formationId: string): Unit[] {
    return Array.from(this.units.values()).filter(u => u.formationId === formationId);
  }
  
  public getFormationForUnit(unitId: string): Formation | undefined {
    const unit = this.units.get(unitId);
    if (!unit || !unit.formationId) return undefined;
    return this.formations.get(unit.formationId);
  }
  
  public getAvailableSlots(formationId: string): FormationSlot[] {
    const formation = this.formations.get(formationId);
    if (!formation) return [];
    
    return Array.from(formation.slots.values())
      .filter(slot => slot.status === SlotStatus.Empty);
  }
  
  public getOccupiedSlots(formationId: string): FormationSlot[] {
    const formation = this.formations.get(formationId);
    if (!formation) return [];
    
    return Array.from(formation.slots.values())
      .filter(slot => slot.status === SlotStatus.Occupied);
  }
  
  public setAutoReform(enabled: boolean): void {
    this.autoReform = enabled;
  }
  
  public setReformThreshold(threshold: number): void {
    this.reformThreshold = Math.max(0, Math.min(1, threshold));
  }
  
  public update(deltaTime: number): void {
    if (!this.autoReform) return;
    
    for (const [formationId, formation] of this.formations) {
      if (formation.currentUnitCount === 0) continue;
      
      const occupiedSlots = Array.from(formation.slots.values())
        .filter(s => s.status === SlotStatus.Occupied);
      const fillRate = occupiedSlots.length / formation.maxUnits;
      
      if (fillRate < this.reformThreshold) {
        this.rebalanceFormation(formationId);
      }
    }
  }
  
  private rebalanceFormation(formationId: string): void {
    const formation = this.formations.get(formationId);
    if (!formation) return;
    
    const units = this.getUnitsInFormation(formationId);
    
    const sortedSlots = Array.from(formation.slots.values())
      .filter(slot => slot.status === SlotStatus.Empty)
      .sort((a, b) => a.priority - b.priority);
    
    let slotIndex = 0;
    for (const unit of units) {
      if (unit.slotId) {
        const currentSlot = formation.slots.get(unit.slotId);
        if (currentSlot && currentSlot.priority > AssignmentPriority.Normal && slotIndex < sortedSlots.length) {
          const betterSlot = sortedSlots[slotIndex];
          
          currentSlot.status = SlotStatus.Empty;
          currentSlot.assignedUnitId = null;
          
          betterSlot.status = SlotStatus.Occupied;
          betterSlot.assignedUnitId = unit.id;
          unit.slotId = betterSlot.id;
          
          slotIndex++;
        }
      }
    }
  }
  
  public getFormationStatistics(): {
    totalFormations: number;
    totalUnits: number;
    averageFillRate: number;
    formationsByType: Record<FormationType, number>;
  } {
    let totalUnits = 0;
    let totalFillRate = 0;
    const formationsByType: Record<string, number> = {};
    
    for (const formation of this.formations.values()) {
      totalUnits += formation.currentUnitCount;
      totalFillRate += formation.currentUnitCount / formation.maxUnits;
      
      if (!formationsByType[formation.type]) {
        formationsByType[formation.type] = 0;
      }
      formationsByType[formation.type]++;
    }
    
    return {
      totalFormations: this.formations.size,
      totalUnits,
      averageFillRate: this.formations.size > 0 ? totalFillRate / this.formations.size : 0,
      formationsByType: formationsByType as Record<FormationType, number>,
    };
  }
  
  public dispose(): void {
    this.formations.clear();
    this.units.clear();
  }
}

export class FormationGenerator {
  private spacing: SpacingConfig;
  
  constructor(spacing?: SpacingConfig) {
    this.spacing = spacing || {
      horizontal: 2.0,
      vertical: 2.0,
      depth: 2.0,
      unitRadius: 1.0,
      safeZone: 0.5,
    };
  }
  
  public setSpacing(spacing: SpacingConfig): void {
    this.spacing = { ...spacing };
  }
  
  public generate(type: FormationType, unitCount: number): Record<string, FormationSlot> {
    switch (type) {
      case FormationType.Line:
        return this.generateLine(unitCount);
      case FormationType.Column:
        return this.generateColumn(unitCount);
      case FormationType.Wedge:
        return this.generateWedge(unitCount);
      case FormationType.Circle:
        return this.generateCircle(unitCount);
      case FormationType.Square:
        return this.generateSquare(unitCount);
      case FormationType.Echelon:
        return this.generateEchelon(unitCount);
      case FormationType.VShape:
        return this.generateVShape(unitCount);
      case FormationType.Box:
        return this.generateBox(unitCount);
      default:
        return this.generateLine(unitCount);
    }
  }
  
  private generateId(): string {
    return `gen_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }
  
  private generateLine(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 10);
    const unitsPerRow = Math.ceil(unitCount / rows);
    
    for (let row = 0; row < rows; row++) {
      for (let col = 0; col < unitsPerRow; col++) {
        const index = row * unitsPerRow + col;
        if (index >= unitCount) break;
        
        const slotId = this.generateId();
        slots[slotId] = {
          id: slotId,
          localPosition: {
            x: (col - (unitsPerRow - 1) / 2) * this.spacing.horizontal,
            y: 0,
            z: row * this.spacing.depth,
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 ? AssignmentPriority.Highest : AssignmentPriority.Normal,
          isFlanking: col === 0 || col === unitsPerRow - 1,
          isSupport: row > 0,
          distanceFromLeader: row,
          connectionSlots: [],
        };
      }
    }
    
    return slots;
  }
  
  private generateColumn(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const unitsPerColumn = Math.ceil(unitCount / 3);
    
    for (let col = 0; col < 3; col++) {
      for (let row = 0; row < unitsPerColumn; row++) {
        const index = col * unitsPerColumn + row;
        if (index >= unitCount) break;
        
        const slotId = this.generateId();
        slots[slotId] = {
          id: slotId,
          localPosition: {
            x: (col - 1) * this.spacing.horizontal,
            y: 0,
            z: row * this.spacing.depth,
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 && col === 1 ? AssignmentPriority.Highest : AssignmentPriority.Normal,
          isFlanking: col === 0 || col === 2,
          isSupport: col === 0 || col === 2,
          distanceFromLeader: row,
          connectionSlots: [],
        };
      }
    }
    
    return slots;
  }
  
  private generateWedge(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(Math.sqrt(unitCount * 2));
    
    let unitIndex = 0;
    for (let row = 0; row < rows && unitIndex < unitCount; row++) {
      const unitsInRow = row + 1;
      for (let col = 0; col < unitsInRow && unitIndex < unitCount; col++) {
        const slotId = this.generateId();
        slots[slotId] = {
          id: slotId,
          localPosition: {
            x: (col - (unitsInRow - 1) / 2) * this.spacing.horizontal,
            y: 0,
            z: row * this.spacing.depth,
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: row === 0 ? AssignmentPriority.Highest : AssignmentPriority.Normal - row,
          isFlanking: col === 0 || col === unitsInRow - 1,
          isSupport: row > 2,
          distanceFromLeader: row,
          connectionSlots: [],
        };
        unitIndex++;
      }
    }
    
    return slots;
  }
  
  private generateCircle(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    
    const centerSlotId = this.generateId();
    slots[centerSlotId] = {
      id: centerSlotId,
      localPosition: { x: 0, y: 0, z: 0 },
      status: SlotStatus.Empty,
      assignedUnitId: null,
      priority: AssignmentPriority.Highest,
      isFlanking: false,
      isSupport: true,
      distanceFromLeader: 0,
      connectionSlots: [],
    };
    
    const ringUnits = unitCount - 1;
    for (let i = 0; i < ringUnits; i++) {
      const angle = (i / ringUnits) * Math.PI * 2;
      const radius = this.spacing.horizontal * 2;
      const slotId = this.generateId();
      
      slots[slotId] = {
        id: slotId,
        localPosition: {
          x: Math.cos(angle) * radius,
          y: 0,
          z: Math.sin(angle) * radius,
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: i % 2 === 0 ? AssignmentPriority.High : AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: false,
        distanceFromLeader: 1,
        connectionSlots: [centerSlotId],
      };
    }
    
    return slots;
  }
  
  private generateSquare(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const sideLength = Math.ceil(Math.sqrt(unitCount));
    
    let unitIndex = 0;
    for (let side = 0; side < 4 && unitIndex < unitCount; side++) {
      for (let i = 0; i < sideLength && unitIndex < unitCount; i++) {
        let x = 0, z = 0;
        
        switch (side) {
          case 0: x = i; z = 0; break;
          case 1: x = sideLength - 1; z = i; break;
          case 2: x = sideLength - 1 - i; z = sideLength - 1; break;
          case 3: x = 0; z = sideLength - 1 - i; break;
        }
        
        const slotId = this.generateId();
        const isCorner = i === 0 || i === sideLength - 1;
        
        slots[slotId] = {
          id: slotId,
          localPosition: {
            x: (x - (sideLength - 1) / 2) * this.spacing.horizontal,
            y: 0,
            z: (z - (sideLength - 1) / 2) * this.spacing.depth,
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: isCorner ? AssignmentPriority.High : AssignmentPriority.Normal,
          isFlanking: isCorner,
          isSupport: !isCorner && side % 2 === 1,
          distanceFromLeader: Math.min(x, z, sideLength - 1 - x, sideLength - 1 - z),
          connectionSlots: [],
        };
        unitIndex++;
      }
    }
    
    return slots;
  }
  
  private generateEchelon(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 5);
    const unitsPerRow = Math.ceil(unitCount / rows);
    
    for (let row = 0; row < rows; row++) {
      const rowOffset = row * this.spacing.horizontal * 0.5;
      for (let col = 0; col < unitsPerRow; col++) {
        const index = row * unitsPerRow + col;
        if (index >= unitCount) break;
        
        const slotId = this.generateId();
        slots[slotId] = {
          id: slotId,
          localPosition: {
            x: col * this.spacing.horizontal + rowOffset - (unitsPerRow * this.spacing.horizontal) / 2,
            y: 0,
            z: row * this.spacing.depth,
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: col === 0 ? AssignmentPriority.High : AssignmentPriority.Normal,
          isFlanking: col === unitsPerRow - 1,
          isSupport: false,
          distanceFromLeader: row,
          connectionSlots: [],
        };
      }
    }
    
    return slots;
  }
  
  private generateVShape(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const halfUnits = Math.ceil(unitCount / 2);
    
    const leaderSlotId = this.generateId();
    slots[leaderSlotId] = {
      id: leaderSlotId,
      localPosition: { x: 0, y: 0, z: 0 },
      status: SlotStatus.Empty,
      assignedUnitId: null,
      priority: AssignmentPriority.Highest,
      isFlanking: false,
      isSupport: false,
      distanceFromLeader: 0,
      connectionSlots: [],
    };
    
    for (let row = 1; row < halfUnits && Object.keys(slots).length < unitCount; row++) {
      const leftSlotId = this.generateId();
      const rightSlotId = this.generateId();
      
      slots[leftSlotId] = {
        id: leftSlotId,
        localPosition: {
          x: -row * this.spacing.horizontal,
          y: 0,
          z: row * this.spacing.depth,
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: row > 2,
        distanceFromLeader: row,
        connectionSlots: [leaderSlotId],
      };
      
      slots[rightSlotId] = {
        id: rightSlotId,
        localPosition: {
          x: row * this.spacing.horizontal,
          y: 0,
          z: row * this.spacing.depth,
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: AssignmentPriority.Normal,
        isFlanking: true,
        isSupport: row > 2,
        distanceFromLeader: row,
        connectionSlots: [leaderSlotId],
      };
    }
    
    return slots;
  }
  
  private generateBox(unitCount: number): Record<string, FormationSlot> {
    const slots: Record<string, FormationSlot> = {};
    const rows = Math.ceil(unitCount / 4);
    const unitsPerSide = Math.ceil(rows / 4) * 2;
    
    const frontSlots: string[] = [];
    const backSlots: string[] = [];
    const leftSlots: string[] = [];
    const rightSlots: string[] = [];
    
    for (let i = 0; i < unitsPerSide && Object.keys(slots).length < unitCount; i++) {
      const frontSlotId = this.generateId();
      const x = (i - (unitsPerSide - 1) / 2) * this.spacing.horizontal;
      
      slots[frontSlotId] = {
        id: frontSlotId,
        localPosition: {
          x,
          y: 0,
          z: -unitsPerSide * this.spacing.depth / 2,
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: i === 0 || i === unitsPerSide - 1 ? AssignmentPriority.High : AssignmentPriority.Normal,
        isFlanking: i === 0,
        isSupport: false,
        distanceFromLeader: unitsPerSide,
        connectionSlots: [],
      };
      frontSlots.push(frontSlotId);
      
      if (Object.keys(slots).length >= unitCount) break;
      
      const backSlotId = this.generateId();
      slots[backSlotId] = {
        id: backSlotId,
        localPosition: {
          x,
          y: 0,
          z: unitsPerSide * this.spacing.depth / 2,
        },
        status: SlotStatus.Empty,
        assignedUnitId: null,
        priority: AssignmentPriority.Normal,
        isFlanking: false,
        isSupport: true,
        distanceFromLeader: unitsPerSide,
        connectionSlots: [],
      };
      backSlots.push(backSlotId);
      
      if (i > 0 && i < unitsPerSide - 1 && Object.keys(slots).length < unitCount) {
        const leftSlotId = this.generateId();
        slots[leftSlotId] = {
          id: leftSlotId,
          localPosition: {
            x: -unitsPerSide * this.spacing.horizontal / 2,
            y: 0,
            z: (i - (unitsPerSide - 1) / 2) * this.spacing.depth,
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: AssignmentPriority.Normal,
          isFlanking: true,
          isSupport: true,
          distanceFromLeader: 1,
          connectionSlots: [],
        };
        leftSlots.push(leftSlotId);
        
        if (Object.keys(slots).length >= unitCount) break;
        
        const rightSlotId = this.generateId();
        slots[rightSlotId] = {
          id: rightSlotId,
          localPosition: {
            x: unitsPerSide * this.spacing.horizontal / 2,
            y: 0,
            z: (i - (unitsPerSide - 1) / 2) * this.spacing.depth,
          },
          status: SlotStatus.Empty,
          assignedUnitId: null,
          priority: AssignmentPriority.Normal,
          isFlanking: true,
          isSupport: true,
          distanceFromLeader: 1,
          connectionSlots: [],
        };
        rightSlots.push(rightSlotId);
      }
    }
    
    return slots;
  }
}

export interface PhysicsWorld {
  id: string;
  gravity: Vector3;
  solverIterations: number;
  timestep: number;
  fixedTimestep: number;
  maxSubSteps: number;
  broadphase: BroadphaseType;
  entities: PhysicsEntity[];
  constraints: PhysicsConstraint[];
  manifolds: CollisionManifold[];
  paused: boolean;
  autoSimulation: boolean;
}

export interface PhysicsEntity {
  id: string;
  type: PhysicsBodyType;
  position: Vector3;
  rotation: Quaternion;
  velocity: Vector3;
  angularVelocity: Vector3;
  mass: number;
  inverseMass: number;
  inertia: Vector3;
  inverseInertia: Vector3;
  restitution: number;
  friction: number;
  rollingFriction: number;
  linearDamping: number;
  angularDamping: number;
  gravityScale: number;
  isStatic: boolean;
  isKinematic: boolean;
  isTrigger: boolean;
  collisionShape: CollisionShape;
  collisionGroup: number;
  collisionMask: number;
  sleeping: boolean;
  sleepTimer: number;
  linearSleepThreshold: number;
  angularSleepThreshold: number;
  forceAccumulator: Vector3;
  torqueAccumulator: Vector3;
  ccdEnabled: boolean;
  ccdRadius: number;
  ccdTimeOfImpact: number;
  userData: Record<string, unknown>;
}

export type PhysicsBodyType =
  | 'dynamic'
  | 'static'
  | 'kinematic';

export type CollisionShape =
  | BoxShape
  | SphereShape
  | CapsuleShape
  | CylinderShape
  | ConeShape
  | MeshShape
  | CompoundShape
  | HeightfieldShape
  | CustomShape;

export interface BoxShape {
  type: 'box';
  halfExtents: Vector3;
}

export interface SphereShape {
  type: 'sphere';
  radius: number;
}

export interface CapsuleShape {
  type: 'capsule';
  radius: number;
  height: number;
  direction: 'x' | 'y' | 'z';
}

export interface CylinderShape {
  type: 'cylinder';
  radius: number;
  height: number;
  direction: 'x' | 'y' | 'z';
}

export interface ConeShape {
  type: 'cone';
  radius: number;
  height: number;
  direction: 'x' | 'y' | 'z';
}

export interface MeshShape {
  type: 'mesh';
  vertices: Vector3[];
  indices: number[];
  meshType: 'convex' | 'concave' | 'triangle';
}

export interface CompoundShape {
  type: 'compound';
  children: CompoundChild[];
}

export interface CompoundChild {
  shape: CollisionShape;
  position: Vector3;
  rotation: Quaternion;
}

export interface HeightfieldShape {
  type: 'heightfield';
  heightData: number[];
  width: number;
  length: number;
  maxHeight: number;
  flipQuadEdges: boolean;
}

export interface CustomShape {
  type: 'custom';
  customType: string;
  parameters: Record<string, unknown>;
}

export interface Vector3 {
  x: number;
  y: number;
  z: number;
}

export interface Quaternion {
  x: number;
  y: number;
  z: number;
  w: number;
}

export interface PhysicsConstraint {
  id: string;
  type: ConstraintType;
  bodyA: string;
  bodyB: string;
  enabled: boolean;
  collideConnected: boolean;
  breakingImpulseThreshold: number;
  equations: ConstraintEquation[];
}

export type ConstraintType =
  | 'point2point'
  | 'hinge'
  | 'slider'
  | 'cone_twist'
  | 'generic_6dof'
  | 'gear'
  | 'rack'
  | 'fixed'
  | 'spring'
  | 'stiff_spring';

export interface ConstraintEquation {
  minValue: number;
  maxValue: number;
  stiffness: number;
  relaxation: number;
  damping: number;
  rhs: number;
  targetVelocity: number;
  accumulatedImpulse: number;
}

export interface CollisionManifold {
  id: string;
  bodyA: string;
  bodyB: string;
  contacts: ContactPoint[];
  normal: Vector3;
  depth: number;
  isUpdated: boolean;
}

export interface ContactPoint {
  position: Vector3;
  normal: Vector3;
  penetration: number;
  friction1: number;
  friction2: number;
  restitution: number;
  normalImpulse: number;
  frictionImpulse1: number;
  frictionImpulse2: number;
}

export type BroadphaseType =
  | 'sap_broadphase'
  | 'axis_sweep3'
  | 'axis_sweep3_32'
  | 'brute_force'
  | 'mlcpg'
  | 'quick'
  | 'simple';

export interface RaycastHit {
  entity: PhysicsEntity;
  point: Vector3;
  normal: Vector3;
  distance: number;
  faceIndex: number;
  uv?: Vector2;
}

export interface Vector2 {
  x: number;
  y: number;
}

export interface PhysicsMaterial {
  id: string;
  name: string;
  staticFriction: number;
  dynamicFriction: number;
  restitution: number;
  frictionCombine: CombineMode;
  restitutionCombine: CombineMode;
}

export type CombineMode =
  | 'average'
  | 'min'
  | 'max'
  | 'multiply';

export interface PhysicsVehicle {
  id: string;
  chassis: PhysicsEntity;
  wheels: VehicleWheel[];
  vehicleTuning: VehicleTuning;
  steering: number;
  engineForce: number;
  brakingForce: number;
}

export interface VehicleWheel {
  id: string;
  connectionPoint: Vector3;
  wheelDirection: Vector3;
  wheelAxle: Vector3;
  suspensionRestLength: number;
  radius: number;
  suspensionStiffness: number;
  suspensionCompression: number;
  suspensionDamping: number;
  maxSuspensionTravel: number;
  frictionSlip: number;
  maxSuspensionForce: number;
  rollInfluence: number;
  worldTransform: Transform;
  rotation: number;
  deltaRotation: number;
  suspensionLength: number;
  maxSuspensionCompression: number;
  clippedRivzet: number;
  suspensionRelativeVelocity: number;
  wheelWorldTransform: Transform;
  isFrontWheel: boolean;
  deltaAngle: number;
  engineForce: number;
  brakingForce: number;
  steerAngle: number;
}

export interface Transform {
  position: Vector3;
  rotation: Quaternion;
}

export interface VehicleTuning {
  suspensionStiffness: number;
  suspensionCompression: number;
  suspensionDamping: number;
  maxSuspensionTravelCm: number;
  frictionSlip: number;
  maxSuspensionForce: number;
  rollInfluence: number;
}

export interface CharacterController {
  id: string;
  entity: PhysicsEntity;
  shape: CollisionShape;
  stepHeight: number;
  maxSlope: number;
  gravity: Vector3;
  upAxis: Vector3;
  velocity: Vector3;
  angularVelocity: Vector3;
  setWalkDirection: Vector3;
  walkDirection: Vector3;
  wasJumping: boolean;
  useGhostSweep: boolean;
  turnAngle: number;
  jumpSpeed: number;
  verticalVelocity: number;
  appliedImpulse: number;
  collisionFlags: CollisionFlags;
}

export type CollisionFlags = {
  CF_NONE: number;
  CF_STATIC_OBJECT: number;
  CF_KINEMATIC_OBJECT: number;
  CF_NO_CONTACT_RESPONSE: number;
  CF_CUSTOM_MATERIAL_CALLBACK: number;
  CF_CHARACTER_OBJECT: number;
  CF_DISABLED_DEBRIS: number;
  CF_DISABLED_SOFT_OBJECTS: number;
};

export class PhysicsEngine {
  private worlds: Map<string, PhysicsWorld> = new Map();
  private materials: Map<string, PhysicsMaterial> = new Map();
  private vehicles: Map<string, PhysicsVehicle> = new Map();
  private characterControllers: Map<string, CharacterController> = new Map();
  private raycastCallbacks: ((result: RaycastHit | null) => void)[] = [];
  private collisionCallbacks: Map<string, (manifold: CollisionManifold) => void> = new Map();
  private triggerCallbacks: Map<string, (entityA: string, entityB: string) => void> = new Map();
  private activeWorld?: PhysicsWorld;
  private defaultGravity: Vector3 = { x: 0, y: -9.81, z: 0 };
  private tempVectors: Vector3[] = [];
  private tempQuaternions: Quaternion[] = [];
  private profiler: PhysicsProfiler;
  private statistics: PhysicsStatistics;

  constructor() {
    this.profiler = {
      broadphaseTime: 0,
      narrowphaseTime: 0,
      solverTime: 0,
      totalTime: 0
    };

    this.statistics = {
      totalEntities: 0,
      activeEntities: 0,
      sleepingEntities: 0,
      totalCollisions: 0,
      totalConstraints: 0,
      totalManifolds: 0,
      raycastsPerFrame: 0,
      memoryUsage: 0
    };

    for (let i = 0; i < 20; i++) {
      this.tempVectors.push({ x: 0, y: 0, z: 0 });
    }
    for (let i = 0; i < 10; i++) {
      this.tempQuaternions.push({ x: 0, y: 0, z: 0, w: 1 });
    }

    this.initializeDefaultMaterials();
  }

  private initializeDefaultMaterials(): void {
    this.createMaterial({
      id: 'default',
      name: 'Default',
      staticFriction: 0.5,
      dynamicFriction: 0.5,
      restitution: 0.3,
      frictionCombine: 'average',
      restitutionCombine: 'average'
    });

    this.createMaterial({
      id: 'metal',
      name: 'Metal',
      staticFriction: 0.4,
      dynamicFriction: 0.3,
      restitution: 0.2,
      frictionCombine: 'min',
      restitutionCombine: 'average'
    });

    this.createMaterial({
      id: 'rubber',
      name: 'Rubber',
      staticFriction: 0.9,
      dynamicFriction: 0.8,
      restitution: 0.8,
      frictionCombine: 'max',
      restitutionCombine: 'max'
    });

    this.createMaterial({
      id: 'ice',
      name: 'Ice',
      staticFriction: 0.02,
      dynamicFriction: 0.01,
      restitution: 0.1,
      frictionCombine: 'min',
      restitutionCombine: 'average'
    });

    this.createMaterial({
      id: 'wood',
      name: 'Wood',
      staticFriction: 0.6,
      dynamicFriction: 0.5,
      restitution: 0.3,
      frictionCombine: 'average',
      restitutionCombine: 'average'
    });

    this.createMaterial({
      id: 'concrete',
      name: 'Concrete',
      staticFriction: 0.7,
      dynamicFriction: 0.6,
      restitution: 0.1,
      frictionCombine: 'average',
      restitutionCombine: 'min'
    });
  }

  createWorld(config?: Partial<PhysicsWorld>): PhysicsWorld {
    const world: PhysicsWorld = {
      id: config?.id || `world_${Date.now()}`,
      gravity: config?.gravity || { ...this.defaultGravity },
      solverIterations: config?.solverIterations || 10,
      timestep: config?.timestep || 1 / 60,
      fixedTimestep: config?.fixedTimestep || 1 / 60,
      maxSubSteps: config?.maxSubSteps || 10,
      broadphase: config?.broadphase || 'axis_sweep3',
      entities: [],
      constraints: [],
      manifolds: [],
      paused: config?.paused ?? false,
      autoSimulation: config?.autoSimulation ?? true
    };

    this.worlds.set(world.id, world);

    if (!this.activeWorld) {
      this.activeWorld = world;
    }

    return world;
  }

  setActiveWorld(worldId: string): boolean {
    const world = this.worlds.get(worldId);
    if (!world) return false;

    this.activeWorld = world;
    return true;
  }

  getActiveWorld(): PhysicsWorld | undefined {
    return this.activeWorld;
  }

  destroyWorld(worldId: string): boolean {
    const world = this.worlds.get(worldId);
    if (!world) return false;

    world.entities.forEach(entity => {
      this.removeEntity(entity.id);
    });

    world.constraints.forEach(constraint => {
      this.removeConstraint(constraint.id);
    });

    this.worlds.delete(worldId);

    if (this.activeWorld?.id === worldId) {
      this.activeWorld = this.worlds.values().next().value;
    }

    return true;
  }

  createEntity(config: Partial<PhysicsEntity> & { type: PhysicsBodyType; collisionShape: CollisionShape }): PhysicsEntity {
    const entity: PhysicsEntity = {
      id: config.id || `entity_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      type: config.type,
      position: config.position || { x: 0, y: 0, z: 0 },
      rotation: config.rotation || { x: 0, y: 0, z: 0, w: 1 },
      velocity: config.velocity || { x: 0, y: 0, z: 0 },
      angularVelocity: config.angularVelocity || { x: 0, y: 0, z: 0 },
      mass: config.mass ?? (config.type === 'static' ? 0 : 1),
      inverseMass: config.type === 'static' ? 0 : (config.mass ? 1 / config.mass : 1),
      inertia: config.inertia || { x: 1, y: 1, z: 1 },
      inverseInertia: config.inverseInertia || { x: 1, y: 1, z: 1 },
      restitution: config.restitution ?? 0.3,
      friction: config.friction ?? 0.5,
      rollingFriction: config.rollingFriction ?? 0.1,
      linearDamping: config.linearDamping ?? 0.01,
      angularDamping: config.angularDamping ?? 0.01,
      gravityScale: config.gravityScale ?? 1,
      isStatic: config.type === 'static',
      isKinematic: config.type === 'kinematic',
      isTrigger: config.isTrigger ?? false,
      collisionShape: config.collisionShape,
      collisionGroup: config.collisionGroup ?? 1,
      collisionMask: config.collisionMask ?? 0xFFFF,
      sleeping: false,
      sleepTimer: 0,
      linearSleepThreshold: config.linearSleepThreshold ?? 0.8,
      angularSleepThreshold: config.angularSleepThreshold ?? 0.8,
      forceAccumulator: { x: 0, y: 0, z: 0 },
      torqueAccumulator: { x: 0, y: 0, z: 0 },
      ccdEnabled: config.ccdEnabled ?? false,
      ccdRadius: config.ccdRadius ?? 0,
      ccdTimeOfImpact: config.ccdTimeOfImpact ?? 0,
      userData: config.userData || {}
    };

    this.calculateInertia(entity);

    if (this.activeWorld) {
      this.activeWorld.entities.push(entity);
    }

    return entity;
  }

  private calculateInertia(entity: PhysicsEntity): void {
    const shape = entity.collisionShape;
    let mass = entity.mass;

    if (entity.isStatic) {
      entity.inertia = { x: 0, y: 0, z: 0 };
      entity.inverseInertia = { x: 0, y: 0, z: 0 };
      return;
    }

    let ix = 0, iy = 0, iz = 0;

    switch (shape.type) {
      case 'sphere':
        const sr = shape.radius;
        ix = iy = iz = (2 / 5) * mass * sr * sr;
        break;

      case 'box':
        const hx = shape.halfExtents.x;
        const hy = shape.halfExtents.y;
        const hz = shape.halfExtents.z;
        ix = (1 / 12) * mass * (hy * hy + hz * hz);
        iy = (1 / 12) * mass * (hx * hx + hz * hz);
        iz = (1 / 12) * mass * (hx * hx + hy * hy);
        break;

      case 'capsule':
        const cr = shape.radius;
        const ch = shape.height;
        const coreMass = mass * 0.3;
        const sphereMass = mass * 0.35;
        ix = (2 / 5) * sphereMass * cr * cr + coreMass * (ch * ch / 12 + cr * cr);
        iy = iy;
        iz = ix;
        break;

      case 'cylinder':
        const yr = shape.radius;
        const yh = shape.height;
        ix = (1 / 12) * mass * (yh * yh + yr * yr);
        iy = (1 / 2) * mass * yr * yr;
        iz = ix;
        break;

      case 'cone':
        const nr = shape.radius;
        const nh = shape.height;
        ix = (3 / 20) * mass * (nh * nh + nr * nr);
        iy = (3 / 10) * mass * nr * nr;
        iz = ix;
        break;

      default:
        ix = iy = iz = mass;
        break;
    }

    entity.inertia = { x: ix, y: iy, z: iz };
    entity.inverseInertia = {
      x: ix > 0 ? 1 / ix : 0,
      y: iy > 0 ? 1 / iy : 0,
      z: iz > 0 ? 1 / iz : 0
    };
  }

  getEntity(id: string): PhysicsEntity | undefined {
    for (const world of this.worlds.values()) {
      const entity = world.entities.find(e => e.id === id);
      if (entity) return entity;
    }
    return undefined;
  }

  removeEntity(id: string): boolean {
    for (const world of this.worlds.values()) {
      const index = world.entities.findIndex(e => e.id === id);
      if (index !== -1) {
        world.entities.splice(index, 1);
        return true;
      }
    }
    return false;
  }

  createMaterial(config: PhysicsMaterial): PhysicsMaterial {
    this.materials.set(config.id, config);
    return config;
  }

  getMaterial(id: string): PhysicsMaterial | undefined {
    return this.materials.get(id);
  }

  createConstraint(config: Partial<PhysicsConstraint> & { type: ConstraintType; bodyA: string; bodyB: string }): PhysicsConstraint | undefined {
    const entityA = this.getEntity(config.bodyA);
    const entityB = this.getEntity(config.bodyB);

    if (!entityA || !entityB) return undefined;

    const constraint: PhysicsConstraint = {
      id: config.id || `constraint_${Date.now()}`,
      type: config.type,
      bodyA: config.bodyA,
      bodyB: config.bodyB,
      enabled: config.enabled ?? true,
      collideConnected: config.collideConnected ?? false,
      breakingImpulseThreshold: config.breakingImpulseThreshold ?? Infinity,
      equations: config.equations || this.createDefaultEquations(config.type)
    };

    if (this.activeWorld) {
      this.activeWorld.constraints.push(constraint);
    }

    return constraint;
  }

  private createDefaultEquations(type: ConstraintType): ConstraintEquation[] {
    const defaultEquation: ConstraintEquation = {
      minValue: -Infinity,
      maxValue: Infinity,
      stiffness: 1e10,
      relaxation: 1,
      damping: 1,
      rhs: 0,
      targetVelocity: 0,
      accumulatedImpulse: 0
    };

    switch (type) {
      case 'point2point':
        return [
          { ...defaultEquation },
          { ...defaultEquation },
          { ...defaultEquation }
        ];

      case 'hinge':
        return [
          { ...defaultEquation },
          { ...defaultEquation },
          { ...defaultEquation }
        ];

      case 'slider':
        return [
          { ...defaultEquation },
          { ...defaultEquation }
        ];

      case 'generic_6dof':
        return Array(6).fill(null).map(() => ({ ...defaultEquation }));

      case 'spring':
        return [
          { ...defaultEquation, stiffness: 100, damping: 10 }
        ];

      default:
        return [{ ...defaultEquation }];
    }
  }

  removeConstraint(id: string): boolean {
    for (const world of this.worlds.values()) {
      const index = world.constraints.findIndex(c => c.id === id);
      if (index !== -1) {
        world.constraints.splice(index, 1);
        return true;
      }
    }
    return false;
  }

  applyForce(entityId: string, force: Vector3, point?: Vector3): boolean {
    const entity = this.getEntity(entityId);
    if (!entity || entity.isStatic) return false;

    entity.forceAccumulator.x += force.x;
    entity.forceAccumulator.y += force.y;
    entity.forceAccumulator.z += force.z;

    if (point) {
      const r = this.vector3Subtract(point, entity.position);
      const torque = this.vector3Cross(r, force);
      entity.torqueAccumulator.x += torque.x;
      entity.torqueAccumulator.y += torque.y;
      entity.torqueAccumulator.z += torque.z;
    }

    this.wakeUp(entityId);

    return true;
  }

  applyImpulse(entityId: string, impulse: Vector3, point?: Vector3): boolean {
    const entity = this.getEntity(entityId);
    if (!entity || entity.isStatic) return false;

    if (entity.inverseMass > 0) {
      entity.velocity.x += impulse.x * entity.inverseMass;
      entity.velocity.y += impulse.y * entity.inverseMass;
      entity.velocity.z += impulse.z * entity.inverseMass;
    }

    if (point) {
      const r = this.vector3Subtract(point, entity.position);
      const angularImpulse = this.vector3Scale(this.vector3Cross(r, impulse), entity.inverseMass);
      entity.angularVelocity.x += angularImpulse.x;
      entity.angularVelocity.y += angularImpulse.y;
      entity.angularVelocity.z += angularImpulse.z;
    }

    this.wakeUp(entityId);

    return true;
  }

  applyTorque(entityId: string, torque: Vector3): boolean {
    const entity = this.getEntity(entityId);
    if (!entity || entity.isStatic) return false;

    if (entity.inverseMass > 0) {
      entity.angularVelocity.x += torque.x * entity.inverseInertia.x;
      entity.angularVelocity.y += torque.y * entity.inverseInertia.y;
      entity.angularVelocity.z += torque.z * entity.inverseInertia.z;
    }

    this.wakeUp(entityId);

    return true;
  }

  setVelocity(entityId: string, velocity: Vector3): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.velocity = { ...velocity };
    this.wakeUp(entityId);

    return true;
  }

  getVelocity(entityId: string): Vector3 | undefined {
    const entity = this.getEntity(entityId);
    return entity ? { ...entity.velocity } : undefined;
  }

  setAngularVelocity(entityId: string, angularVelocity: Vector3): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.angularVelocity = { ...angularVelocity };
    this.wakeUp(entityId);

    return true;
  }

  getAngularVelocity(entityId: string): Vector3 | undefined {
    const entity = this.getEntity(entityId);
    return entity ? { ...entity.angularVelocity } : undefined;
  }

  setPosition(entityId: string, position: Vector3): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.position = { ...position };
    if (entity.isKinematic) {
      entity.velocity = { x: 0, y: 0, z: 0 };
    }

    return true;
  }

  getPosition(entityId: string): Vector3 | undefined {
    const entity = this.getEntity(entityId);
    return entity ? { ...entity.position } : undefined;
  }

  setRotation(entityId: string, rotation: Quaternion): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.rotation = { ...rotation };
    if (entity.isKinematic) {
      entity.angularVelocity = { x: 0, y: 0, z: 0 };
    }

    return true;
  }

  getRotation(entityId: string): Quaternion | undefined {
    const entity = this.getEntity(entityId);
    return entity ? { ...entity.rotation } : undefined;
  }

  setMass(entityId: string, mass: number): boolean {
    const entity = this.getEntity(entityId);
    if (!entity || entity.isStatic) return false;

    entity.mass = mass;
    entity.inverseMass = mass > 0 ? 1 / mass : 0;
    this.calculateInertia(entity);

    return true;
  }

  getMass(entityId: string): number | undefined {
    const entity = this.getEntity(entityId);
    return entity?.mass;
  }

  setLinearDamping(entityId: string, damping: number): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.linearDamping = damping;
    return true;
  }

  setAngularDamping(entityId: string, damping: number): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.angularDamping = damping;
    return true;
  }

  wakeUp(entityId: string): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.sleeping = false;
    entity.sleepTimer = 0;
    return true;
  }

  putToSleep(entityId: string): boolean {
    const entity = this.getEntity(entityId);
    if (!entity) return false;

    entity.sleeping = true;
    entity.velocity = { x: 0, y: 0, z: 0 };
    entity.angularVelocity = { x: 0, y: 0, z: 0 };
    entity.forceAccumulator = { x: 0, y: 0, z: 0 };
    entity.torqueAccumulator = { x: 0, y: 0, z: 0 };

    return true;
  }

  raycast(origin: Vector3, direction: Vector3, maxDistance: number, filter?: RaycastFilter): RaycastHit | null {
    const normalizedDir = this.vector3Normalize(direction);
    let closestHit: RaycastHit | null = null;
    let closestDistance = maxDistance;

    for (const world of this.worlds.values()) {
      for (const entity of world.entities) {
        if (filter && !this.shouldRaycast(entity, filter)) {
          continue;
        }

        const hit = this.raycastEntity(origin, normalizedDir, maxDistance, entity);
        if (hit && hit.distance < closestDistance) {
          closestDistance = hit.distance;
          closestHit = hit;
        }
      }
    }

    return closestHit;
  }

  private shouldRaycast(entity: PhysicsEntity, filter: RaycastFilter): boolean {
    if (filter.collisionGroup && (entity.collisionGroup & filter.collisionGroup) === 0) {
      return false;
    }
    if (filter.collisionMask && (entity.collisionMask & filter.collisionMask) === 0) {
      return false;
    }
    if (filter.excludeIds && filter.excludeIds.includes(entity.id)) {
      return false;
    }
    return true;
  }

  private raycastEntity(origin: Vector3, direction: Vector3, maxDistance: number, entity: PhysicsEntity): RaycastHit | null {
    const shape = entity.collisionShape;
    let distance = -1;
    let normal = { x: 0, y: 0, z: 0 };
    let point = { x: 0, y: 0, z: 0 };

    switch (shape.type) {
      case 'sphere':
        const sphereResult = this.raycastSphere(origin, direction, entity.position, shape.radius, maxDistance);
        if (sphereResult) {
          distance = sphereResult.distance;
          normal = sphereResult.normal;
          point = sphereResult.point;
        }
        break;

      case 'box':
        const boxResult = this.raycastBox(origin, direction, entity.position, entity.rotation, shape.halfExtents, maxDistance);
        if (boxResult) {
          distance = boxResult.distance;
          normal = boxResult.normal;
          point = boxResult.point;
        }
        break;

      case 'capsule':
        const capsuleResult = this.raycastCapsule(origin, direction, entity.position, shape.radius, shape.height, maxDistance);
        if (capsuleResult) {
          distance = capsuleResult.distance;
          normal = capsuleResult.normal;
          point = capsuleResult.point;
        }
        break;

      default:
        break;
    }

    if (distance >= 0 && distance <= maxDistance) {
      return {
        entity,
        point,
        normal,
        distance,
        faceIndex: 0
      };
    }

    return null;
  }

  private raycastSphere(origin: Vector3, direction: Vector3, center: Vector3, radius: number, maxDistance: number): { distance: number; normal: Vector3; point: Vector3 } | null {
    const oc = this.vector3Subtract(origin, center);
    const a = this.vector3Dot(direction, direction);
    const b = 2 * this.vector3Dot(oc, direction);
    const c = this.vector3Dot(oc, oc) - radius * radius;
    const discriminant = b * b - 4 * a * c;

    if (discriminant < 0) return null;

    const t = (-b - Math.sqrt(discriminant)) / (2 * a);
    if (t < 0 || t > maxDistance) return null;

    const point = {
      x: origin.x + direction.x * t,
      y: origin.y + direction.y * t,
      z: origin.z + direction.z * t
    };

    const normal = this.vector3Normalize(this.vector3Subtract(point, center));

    return { distance: t, normal, point };
  }

  private raycastBox(origin: Vector3, direction: Vector3, center: Vector3, rotation: Quaternion, halfExtents: Vector3, maxDistance: number): { distance: number; normal: Vector3; point: Vector3 } | null {
    const localOrigin = this.worldToLocal(origin, center, rotation);
    const localDir = this.rotateVector(direction, this.quaternionConjugate(rotation));

    const tMin = this.vector3Scale(this.vector3Subtract(this.vector3Negate(halfExtents), localOrigin), 1 / localDir.x);
    const tMax = this.vector3Scale(this.vector3Subtract(halfExtents, localOrigin), 1 / localDir.x);

    const txMin = Math.min(tMin.x, tMax.x);
    const txMax = Math.max(tMin.x, tMax.x);
    const tyMin = Math.min(tMin.y, tMax.y);
    const tyMax = Math.max(tMin.y, tMax.y);

    let t0 = Math.max(txMin, tyMin);
    let t1 = Math.min(txMax, tyMax);

    if (txMin > tyMax || tyMin > txMax) return null;
    if (t0 > maxDistance || t1 < 0) return null;

    t0 = Math.max(t0, 0);
    t1 = Math.min(t1, maxDistance);

    const localPoint = {
      x: localOrigin.x + localDir.x * t0,
      y: localOrigin.y + localDir.y * t0,
      z: localOrigin.z + localDir.z * t0
    };

    const point = this.localToWorld(localPoint, center, rotation);
    const normal = this.calculateBoxNormal(localPoint, halfExtents);

    return { distance: t0, normal: this.rotateVector(normal, rotation), point };
  }

  private raycastCapsule(origin: Vector3, direction: Vector3, center: Vector3, radius: number, height: number, maxDistance: number): { distance: number; normal: Vector3; point: Vector3 } | null {
    const halfHeight = height / 2;
    const sphereCenter1 = { x: center.x, y: center.y - halfHeight + radius, z: center.z };
    const sphereCenter2 = { x: center.x, y: center.y + halfHeight - radius, z: center.z };

    let closestHit: { distance: number; normal: Vector3; point: Vector3 } | null = null;
    let closestDistance = maxDistance;

    const hit1 = this.raycastSphere(origin, direction, sphereCenter1, radius, maxDistance);
    if (hit1 && hit1.distance < closestDistance) {
      closestDistance = hit1.distance;
      closestHit = hit1;
    }

    const hit2 = this.raycastSphere(origin, direction, sphereCenter2, radius, maxDistance);
    if (hit2 && hit2.distance < closestDistance) {
      closestDistance = hit2.distance;
      closestHit = hit2;
    }

    const cylinderAxis = { x: 0, y: 1, z: 0 };
    const cylinderStart = { x: center.x, y: center.y - halfHeight + radius, z: center.z };
    const cylinderEnd = { x: center.x, y: center.y + halfHeight - radius, z: center.z };

    return closestHit;
  }

  private worldToLocal(point: Vector3, center: Vector3, rotation: Quaternion): Vector3 {
    const translated = this.vector3Subtract(point, center);
    return this.rotateVector(translated, this.quaternionConjugate(rotation));
  }

  private localToWorld(point: Vector3, center: Vector3, rotation: Quaternion): Vector3 {
    const rotated = this.rotateVector(point, rotation);
    return this.vector3Add(center, rotated);
  }

  private rotateVector(v: Vector3, q: Quaternion): Vector3 {
    const u = { x: q.x, y: q.y, z: q.z };
    const s = q.w;

    return this.vector3Add(
      this.vector3Scale(this.vector3Cross(u, v), 2 * this.vector3Dot(u, v)),
      this.vector3Add(
        this.vector3Scale(v, s * s - this.vector3Dot(u, u)),
        this.vector3Scale(u, 2 * this.vector3Dot(u, v))
      )
    );
  }

  private quaternionConjugate(q: Quaternion): Quaternion {
    return { x: -q.x, y: -q.y, z: -q.z, w: q.w };
  }

  private calculateBoxNormal(point: Vector3, halfExtents: Vector3): Vector3 {
    const ax = Math.abs(point.x) - halfExtents.x;
    const ay = Math.abs(point.y) - halfExtents.y;
    const az = Math.abs(point.z) - halfExtents.z;

    if (ax > ay && ax > az) {
      return { x: point.x > 0 ? 1 : -1, y: 0, z: 0 };
    } else if (ay > ax && ay > az) {
      return { x: 0, y: point.y > 0 ? 1 : -1, z: 0 };
    } else {
      return { x: 0, y: 0, z: point.z > 0 ? 1 : -1 };
    }
  }

  private vector3Add(a: Vector3, b: Vector3): Vector3 {
    return { x: a.x + b.x, y: a.y + b.y, z: a.z + b.z };
  }

  private vector3Subtract(a: Vector3, b: Vector3): Vector3 {
    return { x: a.x - b.x, y: a.y - b.y, z: a.z - b.z };
  }

  private vector3Scale(v: Vector3, s: number): Vector3 {
    return { x: v.x * s, y: v.y * s, z: v.z * s };
  }

  private vector3Dot(a: Vector3, b: Vector3): number {
    return a.x * b.x + a.y * b.y + a.z * b.z;
  }

  private vector3Cross(a: Vector3, b: Vector3): Vector3 {
    return {
      x: a.y * b.z - a.z * b.y,
      y: a.z * b.x - a.x * b.z,
      z: a.x * b.y - a.y * b.x
    };
  }

  private vector3Normalize(v: Vector3): Vector3 {
    const length = Math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    if (length === 0) return { x: 0, y: 0, z: 0 };
    return { x: v.x / length, y: v.y / length, z: v.z / length };
  }

  private vector3Negate(v: Vector3): Vector3 {
    return { x: -v.x, y: -v.y, z: -v.z };
  }

  private vector3Length(v: Vector3): number {
    return Math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
  }

  private vector3LengthSq(v: Vector3): number {
    return v.x * v.x + v.y * v.y + v.z * v.z;
  }

  raycastAll(origin: Vector3, direction: Vector3, maxDistance: number, filter?: RaycastFilter): RaycastHit[] {
    const hits: RaycastHit[] = [];
    const normalizedDir = this.vector3Normalize(direction);

    for (const world of this.worlds.values()) {
      for (const entity of world.entities) {
        if (filter && !this.shouldRaycast(entity, filter)) {
          continue;
        }

        const hit = this.raycastEntity(origin, normalizedDir, maxDistance, entity);
        if (hit) {
          hits.push(hit);
        }
      }
    }

    hits.sort((a, b) => a.distance - b.distance);
    return hits;
  }

  sphereTest(center: Vector3, radius: number, filter?: RaycastFilter): RaycastHit[] {
    const hits: RaycastHit[] = [];

    for (const world of this.worlds.values()) {
      for (const entity of world.entities) {
        if (filter && !this.shouldRaycast(entity, filter)) {
          continue;
        }

        const shape = entity.collisionShape;
        let distance = -1;
        let normal = { x: 0, y: 0, z: 0 };

        switch (shape.type) {
          case 'sphere':
            const dist = this.vector3Length(this.vector3Subtract(entity.position, center));
            distance = dist - shape.radius - radius;
            if (distance < 0) {
              normal = this.vector3Normalize(this.vector3Subtract(center, entity.position));
            }
            break;

          case 'box':
            const boxResult = this.sphereBoxTest(center, radius, entity);
            if (boxResult) {
              distance = boxResult.distance;
              normal = boxResult.normal;
            }
            break;
        }

        if (distance !== null && distance <= 0) {
          hits.push({
            entity,
            point: this.vector3Add(center, this.vector3Scale(normal, radius)),
            normal,
            distance: Math.abs(distance),
            faceIndex: 0
          });
        }
      }
    }

    return hits;
  }

  private sphereBoxTest(center: Vector3, radius: number, entity: PhysicsEntity): { distance: number; normal: Vector3 } | null {
    const shape = entity.collisionShape;
    if (shape.type !== 'box') return null;

    const localCenter = this.worldToLocal(center, entity.position, entity.rotation);
    const closest = {
      x: Math.max(-shape.halfExtents.x, Math.min(localCenter.x, shape.halfExtents.x)),
      y: Math.max(-shape.halfExtents.y, Math.min(localCenter.y, shape.halfExtents.y)),
      z: Math.max(-shape.halfExtents.z, Math.min(localCenter.z, shape.halfExtents.z))
    };

    const diff = this.vector3Subtract(localCenter, closest);
    const distSq = this.vector3LengthSq(diff);

    if (distSq < radius * radius) {
      const dist = Math.sqrt(distSq);
      const normal = dist > 0 ? this.vector3Scale(diff, 1 / dist) : { x: 0, y: 1, z: 0 };
      return {
        distance: dist - radius,
        normal: this.rotateVector(normal, entity.rotation)
      };
    }

    return null;
  }

  sweepTest(shapeType: 'sphere' | 'box' | 'capsule', shape: CollisionShape, from: Vector3, to: Vector3, filter?: RaycastFilter): RaycastHit | null {
    const direction = this.vector3Subtract(to, from);
    const distance = this.vector3Length(direction);

    if (distance === 0) return null;

    const normalizedDir = this.vector3Scale(direction, 1 / distance);
    const steps = Math.ceil(distance / 0.1);
    const stepSize = 1 / steps;

    let closestHit: RaycastHit | null = null;
    let closestDistance = distance;

    for (let i = 0; i <= steps; i++) {
      const t = i * stepSize;
      const position = {
        x: from.x + direction.x * t,
        y: from.y + direction.y * t,
        z: from.z + direction.z * t
      };

      for (const world of this.worlds.values()) {
        for (const entity of world.entities) {
          if (filter && !this.shouldRaycast(entity, filter)) {
            continue;
          }

          const hit = this.checkShapeAtPoint(shapeType, shape, position, entity);
          if (hit) {
            const actualDistance = this.vector3Length(this.vector3Subtract(position, from));
            if (actualDistance < closestDistance) {
              closestDistance = actualDistance;
              closestHit = {
                entity,
                point: position,
                normal: normalizedDir,
                distance: actualDistance,
                faceIndex: 0
              };
            }
          }
        }
      }
    }

    return closestHit;
  }

  private checkShapeAtPoint(shapeType: string, shape: CollisionShape, position: Vector3, entity: PhysicsEntity): boolean {
    const entityShape = entity.collisionShape;

    switch (shapeType) {
      case 'sphere':
        if (entityShape.type === 'sphere') {
          const dist = this.vector3Length(this.vector3Subtract(position, entity.position));
          return dist <= (shape as SphereShape).radius + entityShape.radius;
        }
        break;
    }

    return false;
  }

  update(deltaTime: number): void {
    if (!this.activeWorld || this.activeWorld.paused) return;

    const startTime = performance.now();

    this.activeWorld.entities.forEach(entity => {
      if (!entity.sleeping && !entity.isStatic) {
        this.integrate(entity, deltaTime);
      }
    });

    this.detectCollisions();

    this.solveConstraints();

    this.activeWorld.entities.forEach(entity => {
      if (!entity.isStatic && !entity.isKinematic) {
        this.applyDamping(entity, deltaTime);
        this.updateSleepState(entity, deltaTime);
      }
    });

    const endTime = performance.now();
    this.profiler.totalTime = endTime - startTime;
  }

  private integrate(entity: PhysicsEntity, dt: number): void {
    if (entity.inverseMass === 0) return;

    const gravity = this.activeWorld?.gravity || this.defaultGravity;
    const scaledGravity = this.vector3Scale(gravity, entity.gravityScale * dt);

    entity.velocity.x += scaledGravity.x;
    entity.velocity.y += scaledGravity.y;
    entity.velocity.z += scaledGravity.z;

    if (entity.forceAccumulator.x !== 0 || entity.forceAccumulator.y !== 0 || entity.forceAccumulator.z !== 0) {
      const forceScale = dt * entity.inverseMass;
      entity.velocity.x += entity.forceAccumulator.x * forceScale;
      entity.velocity.y += entity.forceAccumulator.y * forceScale;
      entity.velocity.z += entity.forceAccumulator.z * forceScale;

      entity.forceAccumulator = { x: 0, y: 0, z: 0 };
    }

    if (entity.torqueAccumulator.x !== 0 || entity.torqueAccumulator.y !== 0 || entity.torqueAccumulator.z !== 0) {
      entity.angularVelocity.x += entity.torqueAccumulator.x * dt * entity.inverseInertia.x;
      entity.angularVelocity.y += entity.torqueAccumulator.y * dt * entity.inverseInertia.y;
      entity.angularVelocity.z += entity.torqueAccumulator.z * dt * entity.inverseInertia.z;

      entity.torqueAccumulator = { x: 0, y: 0, z: 0 };
    }

    entity.position.x += entity.velocity.x * dt;
    entity.position.y += entity.velocity.y * dt;
    entity.position.z += entity.velocity.z * dt;

    const angularChange = this.vector3Scale(entity.angularVelocity, dt);
    const rotationDelta = this.quaternionFromAxisAngle(
      this.vector3Normalize(angularChange),
      this.vector3Length(angularChange)
    );
    entity.rotation = this.quaternionMultiply(entity.rotation, rotationDelta);
    entity.rotation = this.quaternionNormalize(entity.rotation);
  }

  private quaternionFromAxisAngle(axis: Vector3, angle: number): Quaternion {
    const halfAngle = angle / 2;
    const sin = Math.sin(halfAngle);
    return {
      x: axis.x * sin,
      y: axis.y * sin,
      z: axis.z * sin,
      w: Math.cos(halfAngle)
    };
  }

  private quaternionMultiply(a: Quaternion, b: Quaternion): Quaternion {
    return {
      x: a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
      y: a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
      z: a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
      w: a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
    };
  }

  private quaternionNormalize(q: Quaternion): Quaternion {
    const length = Math.sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
    if (length === 0) return { x: 0, y: 0, z: 0, w: 1 };
    return {
      x: q.x / length,
      y: q.y / length,
      z: q.z / length,
      w: q.w / length
    };
  }

  private detectCollisions(): void {
    if (!this.activeWorld) return;

    const broadphaseStart = performance.now();

    const pairs = this.broadphase();
    this.activeWorld.manifolds = [];

    const broadphaseEnd = performance.now();
    this.profiler.broadphaseTime = broadphaseEnd - broadphaseStart;

    const narrowphaseStart = performance.now();

    for (const [idA, idB] of pairs) {
      const entityA = this.getEntity(idA);
      const entityB = this.getEntity(idB);

      if (!entityA || !entityB) continue;
      if (entityA.isStatic && entityB.isStatic) continue;
      if ((entityA.collisionMask & entityB.collisionGroup) === 0) continue;
      if ((entityB.collisionMask & entityA.collisionGroup) === 0) continue;

      const manifold = this.narrowphase(entityA, entityB);
      if (manifold && manifold.contacts.length > 0) {
        this.activeWorld.manifolds.push(manifold);

        const callback = this.collisionCallbacks.get(`${idA}_${idB}`);
        if (callback) {
          callback(manifold);
        }
      }
    }

    const narrowphaseEnd = performance.now();
    this.profiler.narrowphaseTime = narrowphaseEnd - narrowphaseStart;
  }

  private broadphase(): Set<[string, string]> {
    const pairs = new Set<[string, string]>();

    if (!this.activeWorld) return pairs;

    const entities = this.activeWorld.entities.filter(e => !e.isStatic);

    for (let i = 0; i < entities.length; i++) {
      for (let j = i + 1; j < entities.length; j++) {
        const a = entities[i];
        const b = entities[j];

        if (this.aabbOverlap(a, b)) {
          pairs.add([a.id, b.id]);
        }
      }
    }

    return pairs;
  }

  private aabbOverlap(a: PhysicsEntity, b: PhysicsEntity): boolean {
    const aAABB = this.getAABB(a);
    const bAABB = this.getAABB(b);

    return (
      aAABB.min.x <= bAABB.max.x && aAABB.max.x >= bAABB.min.x &&
      aAABB.min.y <= bAABB.max.y && aAABB.max.y >= bAABB.min.y &&
      aAABB.min.z <= bAABB.max.z && aAABB.max.z >= bAABB.min.z
    );
  }

  private getAABB(entity: PhysicsEntity): { min: Vector3; max: Vector3 } {
    const shape = entity.collisionShape;

    switch (shape.type) {
      case 'sphere':
        return {
          min: {
            x: entity.position.x - shape.radius,
            y: entity.position.y - shape.radius,
            z: entity.position.z - shape.radius
          },
          max: {
            x: entity.position.x + shape.radius,
            y: entity.position.y + shape.radius,
            z: entity.position.z + shape.radius
          }
        };

      case 'box':
        const extents = shape.halfExtents;
        return {
          min: {
            x: entity.position.x - extents.x,
            y: entity.position.y - extents.y,
            z: entity.position.z - extents.z
          },
          max: {
            x: entity.position.x + extents.x,
            y: entity.position.y + extents.y,
            z: entity.position.z + extents.z
          }
        };

      default:
        return {
          min: { ...entity.position },
          max: { ...entity.position }
        };
    }
  }

  private narrowphase(entityA: PhysicsEntity, entityB: PhysicsEntity): CollisionManifold | null {
    const shapeA = entityA.collisionShape;
    const shapeB = entityB.collisionShape;

    if (shapeA.type === 'sphere' && shapeB.type === 'sphere') {
      return this.sphereSphereCollision(entityA, entityB, shapeA as SphereShape, shapeB as SphereShape);
    }

    if (shapeA.type === 'box' && shapeB.type === 'box') {
      return this.boxBoxCollision(entityA, entityB, shapeA as BoxShape, shapeB as BoxShape);
    }

    if (shapeA.type === 'sphere' && shapeB.type === 'box') {
      return this.sphereBoxCollision(entityA, entityB, shapeA as SphereShape, shapeB as BoxShape);
    }

    if (shapeA.type === 'box' && shapeB.type === 'sphere') {
      const manifold = this.sphereBoxCollision(entityB, entityA, shapeB as SphereShape, shapeA as BoxShape);
      if (manifold) {
        manifold.normal = this.vector3Negate(manifold.normal);
      }
      return manifold;
    }

    return null;
  }

  private sphereSphereCollision(
    entityA: PhysicsEntity,
    entityB: PhysicsEntity,
    shapeA: SphereShape,
    shapeB: SphereShape
  ): CollisionManifold | null {
    const diff = this.vector3Subtract(entityB.position, entityA.position);
    const distSq = this.vector3LengthSq(diff);
    const radiusSum = shapeA.radius + shapeB.radius;

    if (distSq >= radiusSum * radiusSum) return null;

    const dist = Math.sqrt(distSq);
    const normal = dist > 0 ? this.vector3Scale(diff, 1 / dist) : { x: 0, y: 1, z: 0 };
    const penetration = radiusSum - dist;

    const contactPoint = this.vector3Add(
      entityA.position,
      this.vector3Scale(normal, shapeA.radius - penetration * 0.5)
    );

    return {
      id: `manifold_${entityA.id}_${entityB.id}`,
      bodyA: entityA.id,
      bodyB: entityB.id,
      contacts: [{
        position: contactPoint,
        normal,
        penetration,
        friction1: this.combineFriction(entityA.friction, entityB.friction, entityA.friction, entityB.friction),
        friction2: this.combineFriction(entityA.rollingFriction, entityB.rollingFriction, entityA.friction, entityB.friction),
        restitution: this.combineRestitution(entityA.restitution, entityB.restitution),
        normalImpulse: 0,
        frictionImpulse1: 0,
        frictionImpulse2: 0
      }],
      normal,
      depth: penetration,
      isUpdated: true
    };
  }

  private sphereBoxCollision(
    entityA: PhysicsEntity,
    entityB: PhysicsEntity,
    sphereShape: SphereShape,
    boxShape: BoxShape
  ): CollisionManifold | null {
    const localSphere = this.worldToLocal(entityA.position, entityB.position, entityB.rotation);

    const closest = {
      x: Math.max(-boxShape.halfExtents.x, Math.min(localSphere.x, boxShape.halfExtents.x)),
      y: Math.max(-boxShape.halfExtents.y, Math.min(localSphere.y, boxShape.halfExtents.y)),
      z: Math.max(-boxShape.halfExtents.z, Math.min(localSphere.z, boxShape.halfExtents.z))
    };

    const diff = this.vector3Subtract(localSphere, closest);
    const distSq = this.vector3LengthSq(diff);

    if (distSq >= sphereShape.radius * sphereShape.radius) return null;

    const dist = Math.sqrt(distSq);
    let normal: Vector3;

    if (dist > 0) {
      normal = this.vector3Scale(this.vector3Normalize(diff), -1);
    } else {
      const aabb = this.getAABB(entityB);
      const center = {
        x: (aabb.min.x + aabb.max.x) / 2 - entityA.position.x,
        y: (aabb.min.y + aabb.max.y) / 2 - entityA.position.y,
        z: (aabb.min.z + aabb.max.z) / 2 - entityA.position.z
      };
      normal = this.vector3Normalize(center);
    }

    const penetration = sphereShape.radius - dist;
    const contactPoint = this.vector3Add(
      entityA.position,
      this.vector3Scale(this.vector3Negate(normal), sphereShape.radius)
    );

    return {
      id: `manifold_${entityA.id}_${entityB.id}`,
      bodyA: entityA.id,
      bodyB: entityB.id,
      contacts: [{
        position: contactPoint,
        normal: this.rotateVector(normal, entityB.rotation),
        penetration,
        friction1: this.combineFriction(entityA.friction, entityB.friction, entityA.friction, entityB.friction),
        friction2: this.combineFriction(entityA.rollingFriction, entityB.rollingFriction, entityA.friction, entityB.friction),
        restitution: this.combineRestitution(entityA.restitution, entityB.restitution),
        normalImpulse: 0,
        frictionImpulse1: 0,
        frictionImpulse2: 0
      }],
      normal: this.rotateVector(normal, entityB.rotation),
      depth: penetration,
      isUpdated: true
    };
  }

  private boxBoxCollision(
    entityA: PhysicsEntity,
    entityB: PhysicsEntity,
    shapeA: BoxShape,
    shapeB: BoxShape
  ): CollisionManifold | null {
    const posA = entityA.position;
    const posB = entityB.position;
    const rotA = entityA.rotation;
    const rotB = entityB.rotation;

    const transforms = this.calculateBoxBoxTransforms(posA, rotA, shapeA.halfExtents, posB, rotB, shapeB.halfExtents);

    let minPenetration = Infinity;
    let bestNormal = { x: 0, y: 0, z: 0 };
    let bestFaceA = 0;
    let bestFaceB = 0;

    for (let i = 0; i < 3; i++) {
      const axis = transforms.axesA[i];
      const projA = this.projectBox(transforms.verticesA, axis);
      const projB = this.projectBox(transforms.verticesB, axis);

      const overlap = Math.min(projA.max - projB.min, projB.max - projA.min);

      if (overlap <= 0) return null;

      if (overlap < minPenetration) {
        minPenetration = overlap;
        bestNormal = axis;
        bestFaceA = i;
      }
    }

    const centerDiff = this.vector3Subtract(posB, posA);
    const centerAlongNormal = this.vector3Dot(centerDiff, bestNormal);

    if (centerAlongNormal < 0) {
      bestNormal = this.vector3Negate(bestNormal);
    }

    const contactPoint = this.vector3Add(
      posA,
      this.vector3Scale(bestNormal, minPenetration / 2)
    );

    return {
      id: `manifold_${entityA.id}_${entityB.id}`,
      bodyA: entityA.id,
      bodyB: entityB.id,
      contacts: [{
        position: contactPoint,
        normal: bestNormal,
        penetration: minPenetration,
        friction1: this.combineFriction(entityA.friction, entityB.friction, entityA.friction, entityB.friction),
        friction2: this.combineFriction(entityA.rollingFriction, entityB.rollingFriction, entityA.friction, entityB.friction),
        restitution: this.combineRestitution(entityA.restitution, entityB.restitution),
        normalImpulse: 0,
        frictionImpulse1: 0,
        frictionImpulse2: 0
      }],
      normal: bestNormal,
      depth: minPenetration,
      isUpdated: true
    };
  }

  private calculateBoxBoxTransforms(
    posA: Vector3, rotA: Quaternion, extA: Vector3,
    posB: Vector3, rotB: Quaternion, extB: Vector3
  ): { verticesA: Vector3[]; verticesB: Vector3[]; axesA: Vector3[]; axesB: Vector3[] } {
    const verticesA = this.createBoxVertices(posA, rotA, extA);
    const verticesB = this.createBoxVertices(posB, rotB, extB);

    const axesA = [
      this.rotateVector({ x: 1, y: 0, z: 0 }, rotA),
      this.rotateVector({ x: 0, y: 1, z: 0 }, rotA),
      this.rotateVector({ x: 0, y: 0, z: 1 }, rotA)
    ];

    const axesB = [
      this.rotateVector({ x: 1, y: 0, z: 0 }, rotB),
      this.rotateVector({ x: 0, y: 1, z: 0 }, rotB),
      this.rotateVector({ x: 0, y: 0, z: 1 }, rotB)
    ];

    return { verticesA, verticesB, axesA, axesB };
  }

  private createBoxVertices(center: Vector3, rotation: Quaternion, halfExtents: Vector3): Vector3[] {
    const corners = [
      { x: -1, y: -1, z: -1 },
      { x: 1, y: -1, z: -1 },
      { x: 1, y: 1, z: -1 },
      { x: -1, y: 1, z: -1 },
      { x: -1, y: -1, z: 1 },
      { x: 1, y: -1, z: 1 },
      { x: 1, y: 1, z: 1 },
      { x: -1, y: 1, z: 1 }
    ];

    return corners.map(corner => {
      const scaled = {
        x: corner.x * halfExtents.x,
        y: corner.y * halfExtents.y,
        z: corner.z * halfExtents.z
      };
      const rotated = this.rotateVector(scaled, rotation);
      return this.vector3Add(center, rotated);
    });
  }

  private projectBox(vertices: Vector3[], axis: Vector3): { min: number; max: number } {
    let min = Infinity;
    let max = -Infinity;

    for (const vertex of vertices) {
      const proj = this.vector3Dot(vertex, axis);
      min = Math.min(min, proj);
      max = Math.max(max, proj);
    }

    return { min, max };
  }

  private combineFriction(a: number, b: number, c: number, d: number): number {
    return Math.sqrt(a * b);
  }

  private combineRestitution(a: number, b: number): number {
    return Math.max(a, b);
  }

  private solveConstraints(): void {
    if (!this.activeWorld) return;

    const solverStart = performance.now();

    for (let iteration = 0; iteration < this.activeWorld.solverIterations; iteration++) {
      for (const manifold of this.activeWorld.manifolds) {
        this.solveContact(manifold);
      }

      for (const constraint of this.activeWorld.constraints) {
        this.solveConstraint(constraint);
      }
    }

    for (const manifold of this.activeWorld.manifolds) {
      this.applyContactImpulses(manifold);
    }

    const solverEnd = performance.now();
    this.profiler.solverTime = solverEnd - solverStart;
  }

  private solveContact(manifold: CollisionManifold): void {
    const entityA = this.getEntity(manifold.bodyA);
    const entityB = this.getEntity(manifold.bodyB);

    if (!entityA || !entityB) return;

    for (const contact of manifold.contacts) {
      const relativeVelocity = this.vector3Subtract(entityB.velocity, entityA.velocity);
      const contactVelocity = this.vector3Dot(relativeVelocity, contact.normal);

      if (contactVelocity > 0) continue;

      const restitution = contact.restitution;
      let j = -(1 + restitution) * contactVelocity;

      const invMassSum = entityA.inverseMass + entityB.inverseMass;
      j /= invMassSum;

      const impulse = this.vector3Scale(contact.normal, j);

      entityA.velocity = this.vector3Subtract(entityA.velocity, this.vector3Scale(impulse, entityA.inverseMass));
      entityB.velocity = this.vector3Add(entityB.velocity, this.vector3Scale(impulse, entityB.inverseMass));

      contact.normalImpulse = j;

      const tangent = this.vector3Normalize(
        this.vector3Subtract(relativeVelocity, this.vector3Scale(contact.normal, contactVelocity))
      );

      const jt = -this.vector3Dot(relativeVelocity, tangent) / invMassSum;

      const maxFriction = contact.friction1 * j;
      const frictionImpulse = Math.max(-maxFriction, Math.min(maxFriction, jt));

      const frictionImpulseVec = this.vector3Scale(tangent, frictionImpulse);

      entityA.velocity = this.vector3Subtract(entityA.velocity, this.vector3Scale(frictionImpulseVec, entityA.inverseMass));
      entityB.velocity = this.vector3Add(entityB.velocity, this.vector3Scale(frictionImpulseVec, entityB.inverseMass));

      contact.frictionImpulse1 = frictionImpulse;
    }
  }

  private solveConstraint(constraint: PhysicsConstraint): void {
    const entityA = this.getEntity(constraint.bodyA);
    const entityB = this.getEntity(constraint.bodyB);

    if (!entityA || !entityB) return;

    for (const equation of constraint.equations) {
      const relativeVelocity = this.vector3Subtract(entityB.velocity, entityA.velocity);
      const velocityDifference = equation.targetVelocity - this.vector3Dot(relativeVelocity, { x: 1, y: 0, z: 0 });

      const deltaImpulse = (velocityDifference - equation.accumulatedImpulse * equation.relaxation) / (entityA.inverseMass + entityB.inverseMass);
      equation.accumulatedImpulse += deltaImpulse;

      const impulse = deltaImpulse;

      entityA.velocity.x -= impulse * entityA.inverseMass;
      entityB.velocity.x += impulse * entityB.inverseMass;
    }
  }

  private applyContactImpulses(manifold: CollisionManifold): void {
    const entityA = this.getEntity(manifold.bodyA);
    const entityB = this.getEntity(manifold.bodyB);

    if (!entityA || !entityB) return;

    for (const contact of manifold.contacts) {
      if (contact.normalImpulse !== 0) {
        const impulse = this.vector3Scale(contact.normal, contact.normalImpulse);
        this.resolveInterpenetration(entityA, entityB, contact);
      }
    }
  }

  private resolveInterpenetration(entityA: PhysicsEntity, entityB: PhysicsEntity, contact: ContactPoint): void {
    if (contact.penetration <= 0) return;

    const percent = 0.4;
    const slop = 0.01;

    const correction = Math.max(contact.penetration - slop, 0) /
      (entityA.inverseMass + entityB.inverseMass) * percent;

    const correctionVec = this.vector3Scale(contact.normal, correction);

    entityA.position = this.vector3Subtract(entityA.position, this.vector3Scale(correctionVec, entityA.inverseMass));
    entityB.position = this.vector3Add(entityB.position, this.vector3Scale(correctionVec, entityB.inverseMass));
  }

  private applyDamping(entity: PhysicsEntity, dt: number): void {
    const ldFactor = Math.pow(1 - entity.linearDamping, dt);
    entity.velocity = this.vector3Scale(entity.velocity, ldFactor);

    const adFactor = Math.pow(1 - entity.angularDamping, dt);
    entity.angularVelocity = this.vector3Scale(entity.angularVelocity, adFactor);
  }

  private updateSleepState(entity: PhysicsEntity, dt: number): void {
    if (entity.isStatic || entity.isKinematic) return;

    const linearSpeedSq = this.vector3LengthSq(entity.velocity);
    const angularSpeedSq = this.vector3LengthSq(entity.angularVelocity);

    if (linearSpeedSq < entity.linearSleepThreshold * entity.linearSleepThreshold ||
      angularSpeedSq < entity.angularSleepThreshold * entity.angularSleepThreshold) {
      entity.sleepTimer += dt;

      if (entity.sleepTimer > 2) {
        entity.sleeping = true;
      }
    } else {
      entity.sleepTimer = 0;
    }
  }

  onCollision(callback: (manifold: CollisionManifold) => void): string {
    const id = `collision_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    this.collisionCallbacks.set(id, callback);
    return id;
  }

  onTrigger(callback: (entityA: string, entityB: string) => void): string {
    const id = `trigger_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    this.triggerCallbacks.set(id, callback);
    return id;
  }

  onRaycast(callback: (result: RaycastHit | null) => void): void {
    this.raycastCallbacks.push(callback);
  }

  removeCollisionCallback(id: string): boolean {
    return this.collisionCallbacks.delete(id);
  }

  removeTriggerCallback(id: string): boolean {
    return this.triggerCallbacks.delete(id);
  }

  createVehicle(chassisConfig: Partial<PhysicsEntity> & { collisionShape: CollisionShape }): PhysicsVehicle | undefined {
    const chassis = this.createEntity({
      ...chassisConfig,
      type: 'dynamic'
    });

    const vehicle: PhysicsVehicle = {
      id: `vehicle_${Date.now()}`,
      chassis,
      wheels: [],
      vehicleTuning: {
        suspensionStiffness: 30,
        suspensionCompression: 4.4,
        suspensionDamping: 2.5,
        maxSuspensionTravelCm: 500,
        frictionSlip: 2,
        maxSuspensionForce: 6000,
        rollInfluence: 0.1
      },
      steering: 0,
      engineForce: 0,
      brakingForce: 0
    };

    this.vehicles.set(vehicle.id, vehicle);
    return vehicle;
  }

  addWheel(vehicleId: string, config: Partial<VehicleWheel>): VehicleWheel | undefined {
    const vehicle = this.vehicles.get(vehicleId);
    if (!vehicle) return undefined;

    const wheel: VehicleWheel = {
      id: `wheel_${Date.now()}`,
      connectionPoint: config.connectionPoint || { x: 0, y: 0, z: 0 },
      wheelDirection: config.wheelDirection || { x: 0, y: -1, z: 0 },
      wheelAxle: config.wheelAxle || { x: -1, y: 0, z: 0 },
      suspensionRestLength: config.suspensionRestLength || 0.6,
      radius: config.radius || 0.5,
      suspensionStiffness: config.suspensionStiffness || vehicle.vehicleTuning.suspensionStiffness,
      suspensionCompression: config.suspensionCompression || vehicle.vehicleTuning.suspensionCompression,
      suspensionDamping: config.suspensionDamping || vehicle.vehicleTuning.suspensionDamping,
      maxSuspensionTravel: (config.maxSuspensionTravelCm || vehicle.vehicleTuning.maxSuspensionTravelCm) / 100,
      frictionSlip: config.frictionSlip || vehicle.vehicleTuning.frictionSlip,
      maxSuspensionForce: config.maxSuspensionForce || vehicle.vehicleTuning.maxSuspensionForce,
      rollInfluence: config.rollInfluence || vehicle.vehicleTuning.rollInfluence,
      worldTransform: { position: { x: 0, y: 0, z: 0 }, rotation: { x: 0, y: 0, z: 0, w: 1 } },
      rotation: 0,
      deltaRotation: 0,
      suspensionLength: 0,
      maxSuspensionCompression: 0,
      clippedRivzet: 0,
      suspensionRelativeVelocity: 0,
      wheelWorldTransform: { position: { x: 0, y: 0, z: 0 }, rotation: { x: 0, y: 0, z: 0, w: 1 } },
      isFrontWheel: config.isFrontWheel ?? false,
      deltaAngle: 0,
      engineForce: 0,
      brakingForce: 0,
      steerAngle: 0
    };

    vehicle.wheels.push(wheel);
    return wheel;
  }

  applyEngineForce(vehicleId: string, engineForce: number, wheelIndex?: number): boolean {
    const vehicle = this.vehicles.get(vehicleId);
    if (!vehicle) return false;

    if (wheelIndex !== undefined && wheelIndex >= 0 && wheelIndex < vehicle.wheels.length) {
      vehicle.wheels[wheelIndex].engineForce = engineForce;
    } else {
      vehicle.engineForce = engineForce;
      vehicle.wheels.forEach(wheel => {
        if (!wheel.isFrontWheel) {
          wheel.engineForce = engineForce;
        }
      });
    }

    return true;
  }

  applyBraking(vehicleId: string, brakingForce: number, wheelIndex?: number): boolean {
    const vehicle = this.vehicles.get(vehicleId);
    if (!vehicle) return false;

    if (wheelIndex !== undefined && wheelIndex >= 0 && wheelIndex < vehicle.wheels.length) {
      vehicle.wheels[wheelIndex].brakingForce = brakingForce;
    } else {
      vehicle.brakingForce = brakingForce;
      vehicle.wheels.forEach(wheel => {
        wheel.brakingForce = brakingForce;
      });
    }

    return true;
  }

  setSteering(vehicleId: string, steering: number): boolean {
    const vehicle = this.vehicles.get(vehicleId);
    if (!vehicle) return false;

    vehicle.steering = steering;
    vehicle.wheels.forEach(wheel => {
      if (wheel.isFrontWheel) {
        wheel.steerAngle = steering;
      }
    });

    return true;
  }

  updateVehicle(vehicleId: string, dt: number): boolean {
    const vehicle = this.vehicles.get(vehicleId);
    if (!vehicle) return false;

    vehicle.wheels.forEach(wheel => {
      const suspensionDir = { ...wheel.wheelDirection };
      const wheelPos = this.vector3Add(
        vehicle.chassis.position,
        this.vector3Add(wheel.connectionPoint, { x: 0, y: -wheel.suspensionLength, z: 0 })
      );

      wheel.worldTransform.position = wheelPos;
      wheel.rotation += wheel.deltaRotation;
    });

    return true;
  }

  createCharacterController(
    config: Partial<CharacterController> & { shape: CollisionShape }
  ): CharacterController | undefined {
    const entity = this.createEntity({
      id: config.id || `controller_${Date.now()}`,
      type: 'dynamic',
      collisionShape: config.shape,
      mass: 1,
      linearDamping: 0,
      angularDamping: 0
    });

    if (!entity) return undefined;

    const controller: CharacterController = {
      id: entity.id,
      entity,
      shape: config.shape,
      stepHeight: config.stepHeight ?? 0.5,
      maxSlope: config.maxSlope ?? 45,
      gravity: config.gravity || this.defaultGravity,
      upAxis: config.upAxis || { x: 0, y: 1, z: 0 },
      velocity: config.velocity || { x: 0, y: 0, z: 0 },
      angularVelocity: { x: 0, y: 0, z: 0 },
      setWalkDirection: { x: 0, y: 0, z: 0 },
      walkDirection: { x: 0, y: 0, z: 0 },
      wasJumping: false,
      useGhostSweep: config.useGhostSweep ?? true,
      turnAngle: 0,
      jumpSpeed: config.jumpSpeed ?? 5,
      verticalVelocity: 0,
      appliedImpulse: 0,
      collisionFlags: {
        CF_NONE: 0,
        CF_STATIC_OBJECT: 1,
        CF_KINEMATIC_OBJECT: 2,
        CF_NO_CONTACT_RESPONSE: 4,
        CF_CUSTOM_MATERIAL_CALLBACK: 8,
        CF_CHARACTER_OBJECT: 16,
        CF_DISABLED_DEBRIS: 32,
        CF_DISABLED_SOFT_OBJECTS: 64
      }
    };

    this.characterControllers.set(controller.id, controller);
    return controller;
  }

  moveCharacter(controllerId: string, displacement: Vector3): number {
    const controller = this.characterControllers.get(controllerId);
    if (!controller) return 0;

    controller.setWalkDirection = { ...displacement };

    return this.resolveCharacterCollision(controller, displacement);
  }

  private resolveCharacterCollision(controller: CharacterController, displacement: Vector3): number {
    const startPos = { ...controller.entity.position };
    const direction = this.vector3Normalize(displacement);
    const distance = this.vector3Length(displacement);

    const hit = this.raycast(startPos, direction, distance, {
      collisionGroup: controller.entity.collisionGroup,
      excludeIds: [controller.id]
    });

    if (hit) {
      const actualDistance = hit.distance;
      controller.entity.position = this.vector3Add(startPos, this.vector3Scale(direction, actualDistance * 0.9));

      if (hit.normal.y > Math.cos((controller.maxSlope * Math.PI) / 180)) {
        return actualDistance;
      }

      return 0;
    }

    controller.entity.position = this.vector3Add(startPos, displacement);
    return distance;
  }

  jumpCharacter(controllerId: string): boolean {
    const controller = this.characterControllers.get(controllerId);
    if (!controller) return false;

    if (Math.abs(controller.verticalVelocity) < 0.001) {
      controller.verticalVelocity = controller.jumpSpeed;
      return true;
    }

    return false;
  }

  getCharacterPosition(controllerId: string): Vector3 | undefined {
    const controller = this.characterControllers.get(controllerId);
    return controller?.entity.position;
  }

  setCharacterPosition(controllerId: string, position: Vector3): boolean {
    const controller = this.characterControllers.get(controllerId);
    if (!controller) return false;

    controller.entity.position = { ...position };
    return true;
  }

  destroyCharacterController(controllerId: string): boolean {
    this.removeEntity(controllerId);
    return this.characterControllers.delete(controllerId);
  }

  getProfiler(): PhysicsProfiler {
    return { ...this.profiler };
  }

  getStatistics(): PhysicsStatistics {
    let totalEntities = 0;
    let activeEntities = 0;
    let sleepingEntities = 0;
    let totalConstraints = 0;

    this.worlds.forEach(world => {
      totalEntities += world.entities.length;
      totalConstraints += world.constraints.length;
      world.entities.forEach(entity => {
        if (entity.sleeping) sleepingEntities++;
        else activeEntities++;
      });
    });

    this.statistics.totalEntities = totalEntities;
    this.statistics.activeEntities = activeEntities;
    this.statistics.sleepingEntities = sleepingEntities;
    this.statistics.totalCollisions = this.activeWorld?.manifolds.length || 0;
    this.statistics.totalConstraints = totalConstraints;
    this.statistics.totalManifolds = this.activeWorld?.manifolds.length || 0;

    return { ...this.statistics };
  }

  serialize(): PhysicsEngineState {
    return {
      worlds: Array.from(this.worlds.values()).map(world => ({
        ...world,
        entities: world.entities.map(e => ({ ...e })),
        constraints: world.constraints.map(c => ({ ...c })),
        manifolds: []
      })),
      materials: Array.from(this.materials.values()),
      vehicles: Array.from(this.vehicles.values()).map(v => ({
        ...v,
        chassis: { ...v.chassis },
        wheels: v.wheels.map(w => ({ ...w }))
      })),
      activeWorldId: this.activeWorld?.id
    };
  }

  deserialize(state: PhysicsEngineState): void {
    this.worlds.clear();
    this.materials.clear();
    this.vehicles.clear();
    this.characterControllers.clear();

    state.worlds.forEach(worldData => {
      const world: PhysicsWorld = {
        ...worldData,
        entities: worldData.entities.map(e => ({ ...e })),
        constraints: worldData.constraints.map(c => ({ ...c })),
        manifolds: []
      };
      this.worlds.set(world.id, world);
    });

    state.materials.forEach(material => {
      this.materials.set(material.id, material);
    });

    if (state.activeWorldId) {
      this.activeWorld = this.worlds.get(state.activeWorldId);
    }
  }

  reset(): void {
    this.worlds.clear();
    this.vehicles.clear();
    this.characterControllers.clear();
    this.activeWorld = undefined;
  }
}

export interface RaycastFilter {
  collisionGroup?: number;
  collisionMask?: number;
  excludeIds?: string[];
}

interface PhysicsProfiler {
  broadphaseTime: number;
  narrowphaseTime: number;
  solverTime: number;
  totalTime: number;
}

interface PhysicsStatistics {
  totalEntities: number;
  activeEntities: number;
  sleepingEntities: number;
  totalCollisions: number;
  totalConstraints: number;
  totalManifolds: number;
  raycastsPerFrame: number;
  memoryUsage: number;
}

interface PhysicsEngineState {
  worlds: PhysicsWorld[];
  materials: PhysicsMaterial[];
  vehicles: PhysicsVehicle[];
  activeWorldId?: string;
}

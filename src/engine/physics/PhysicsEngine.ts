
import { Vector2D, Position } from '../../entities/Entity';

export interface RigidBody {
  id: string;
  position: Vector2D;
  velocity: Vector2D;
  acceleration: Vector2D;
  mass: number;
  inverseMass: number;
  restitution: number;
  friction: number;
  isStatic: boolean;
  isKinematic: boolean;
  angularVelocity: number;
  torque: number;
  inertia: number;
  inverseInertia: number;
  angle: number;
  shape: CollisionShape;
}

export type CollisionShape = 
  | { type: 'circle'; radius: number }
  | { type: 'rectangle'; width: number; height: number }
  | { type: 'polygon'; vertices: Vector2D[] };

export interface AABB {
  minX: number;
  minY: number;
  maxX: number;
  maxY: number;
}

export interface CollisionManifold {
  bodyA: RigidBody;
  bodyB: RigidBody;
  normal: Vector2D;
  penetration: number;
  contactPoints: Vector2D[];
}

export interface PhysicsMaterial {
  density: number;
  restitution: number;
  friction: number;
}

export interface Force {
  position: Vector2D;
  direction: Vector2D;
  magnitude: number;
  type: 'force' | 'impulse' | 'torque';
}

export interface PhysicsWorldConfig {
  gravity: Vector2D;
  iterations: number;
  allowSleep: boolean;
  sleepThreshold: number;
  timeScale: number;
  broadPhase: 'brute' | 'sap' | 'bvh';
}

const DEFAULT_CONFIG: PhysicsWorldConfig = {
  gravity: { x: 0, y: 980 },
  iterations: 10,
  allowSleep: true,
  sleepThreshold: 0.1,
  timeScale: 1.0,
  broadPhase: 'sap',
};

export class PhysicsWorld {
  private bodies: Map<string, RigidBody> = new Map();
  private staticBodies: Set<string> = new Set();
  private sleepingBodies: Set<string> = new Set();
  private config: PhysicsWorldConfig;
  private collisionPairs: Set<string> = new Set();
  private forces: Map<string, Force[]> = new Map();
  private time: number = 0;

  constructor(config: Partial<PhysicsWorldConfig> = {}) {
    this.config = { ...DEFAULT_CONFIG, ...config };
  }

  public createBody(
    id: string,
    position: Vector2D,
    shape: CollisionShape,
    material: PhysicsMaterial,
    isStatic: boolean = false
  ): RigidBody {
    const mass = isStatic ? Infinity : material.density * this.calculateArea(shape);
    const body: RigidBody = {
      id,
      position: { ...position },
      velocity: { x: 0, y: 0 },
      acceleration: { x: 0, y: 0 },
      mass,
      inverseMass: isStatic ? 0 : 1 / mass,
      restitution: material.restitution,
      friction: material.friction,
      isStatic,
      isKinematic: false,
      angularVelocity: 0,
      torque: 0,
      inertia: isStatic ? Infinity : this.calculateInertia(shape, mass),
      inverseInertia: isStatic ? 0 : 1 / this.calculateInertia(shape, mass),
      angle: 0,
      shape,
    };

    this.bodies.set(id, body);
    if (isStatic) {
      this.staticBodies.add(id);
    }

    return body;
  }

  public removeBody(id: string): void {
    this.bodies.delete(id);
    this.staticBodies.delete(id);
    this.sleepingBodies.delete(id);
    this.forces.delete(id);
  }

  public getBody(id: string): RigidBody | undefined {
    return this.bodies.get(id);
  }

  public applyForce(bodyId: string, force: Vector2D, point?: Vector2D): void {
    const body = this.bodies.get(bodyId);
    if (!body || body.isStatic) return;

    body.acceleration.x += force.x * body.inverseMass;
    body.acceleration.y += force.y * body.inverseMass;

    if (point) {
      const r = this.subtract(point, body.position);
      const torque = r.x * force.y - r.y * force.x;
      body.torque += torque;
    }

    this.wakeBody(bodyId);
  }

  public applyImpulse(bodyId: string, impulse: Vector2D, point?: Vector2D): void {
    const body = this.bodies.get(bodyId);
    if (!body || body.isStatic) return;

    body.velocity.x += impulse.x * body.inverseMass;
    body.velocity.y += impulse.y * body.inverseMass;

    if (point) {
      const r = this.subtract(point, body.position);
      const angularImpulse = (r.x * impulse.y - r.y * impulse.x) * body.inverseInertia;
      body.angularVelocity += angularImpulse;
    }

    this.wakeBody(bodyId);
  }

  public applyTorque(bodyId: string, torque: number): void {
    const body = this.bodies.get(bodyId);
    if (!body || body.isStatic) return;

    body.angularVelocity += torque * body.inverseInertia;
    this.wakeBody(bodyId);
  }

  public applyForceToMany(bodyIds: string[], force: Vector2D): void {
    for (const id of bodyIds) {
      this.applyForce(id, force);
    }
  }

  private wakeBody(bodyId: string): void {
    this.sleepingBodies.delete(bodyId);
  }

  private sleepBody(bodyId: string): void {
    if (this.config.allowSleep) {
      this.sleepingBodies.add(bodyId);
    }
  }

  public step(dt: number): void {
    const scaledDt = dt * this.config.timeScale;
    this.time += scaledDt;

    for (const body of this.bodies.values()) {
      if (body.isStatic || this.sleepingBodies.has(body.id)) continue;

      if (this.config.allowSleep) {
        const speed = this.length(body.velocity);
        const angularSpeed = Math.abs(body.angularVelocity);

        if (speed < this.config.sleepThreshold && angularSpeed < this.config.sleepThreshold) {
          this.sleepBody(body.id);
          continue;
        }
      }

      this.integrateVelocity(body, scaledDt);
    }

    for (let i = 0; i < this.config.iterations; i++) {
      const pairs = this.broadPhase();
      for (const [idA, idB] of pairs) {
        const bodyA = this.bodies.get(idA);
        const bodyB = this.bodies.get(idB);
        if (!bodyA || !bodyB) continue;

        const manifold = this.detectCollision(bodyA, bodyB);
        if (manifold) {
          this.resolveCollision(manifold);
        }
      }
    }

    for (const body of this.bodies.values()) {
      if (body.isStatic || this.sleepingBodies.has(body.id)) continue;
      this.integratePosition(body, scaledDt);
    }

    this.collisionPairs.clear();
  }

  private integrateVelocity(body: RigidBody, dt: number): void {
    body.velocity.x += (this.config.gravity.x + body.acceleration.x) * dt;
    body.velocity.y += (this.config.gravity.y + body.acceleration.y) * dt;

    body.angularVelocity += body.torque * body.inverseInertia * dt;

    body.acceleration.x = 0;
    body.acceleration.y = 0;
    body.torque = 0;

    const damping = Math.pow(0.999, dt);
    body.velocity.x *= damping;
    body.velocity.y *= damping;
    body.angularVelocity *= damping;
  }

  private integratePosition(body: RigidBody, dt: number): void {
    body.position.x += body.velocity.x * dt;
    body.position.y += body.velocity.y * dt;
    body.angle += body.angularVelocity * dt;
  }

  private broadPhase(): [string, string][] {
    const pairs: [string, string][] = [];
    const bodyIds = Array.from(this.bodies.keys());

    for (let i = 0; i < bodyIds.length; i++) {
      for (let j = i + 1; j < bodyIds.length; j++) {
        const idA = bodyIds[i];
        const idB = bodyIds[j];

        if (this.staticBodies.has(idA) && this.staticBodies.has(idB)) continue;

        const bodyA = this.bodies.get(idA)!;
        const bodyB = this.bodies.get(idB)!;

        const aabbA = this.getAABB(bodyA);
        const aabbB = this.getAABB(bodyB);

        if (this.aabbIntersect(aabbA, aabbB)) {
          pairs.push([idA, idB]);
        }
      }
    }

    return pairs;
  }

  private getAABB(body: RigidBody): AABB {
    if (body.shape.type === 'circle') {
      return {
        minX: body.position.x - body.shape.radius,
        minY: body.position.y - body.shape.radius,
        maxX: body.position.x + body.shape.radius,
        maxY: body.position.y + body.shape.radius,
      };
    } else if (body.shape.type === 'rectangle') {
      const hw = body.shape.width / 2;
      const hh = body.shape.height / 2;
      return {
        minX: body.position.x - hw,
        minY: body.position.y - hh,
        maxX: body.position.x + hw,
        maxY: body.position.y + hh,
      };
    } else {
      let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
      for (const vertex of body.shape.vertices) {
        minX = Math.min(minX, vertex.x);
        minY = Math.min(minY, vertex.y);
        maxX = Math.max(maxX, vertex.x);
        maxY = Math.max(maxY, vertex.y);
      }
      return { minX, minY, maxX, maxY };
    }
  }

  private aabbIntersect(a: AABB, b: AABB): boolean {
    return a.minX <= b.maxX && a.maxX >= b.minX && a.minY <= b.maxY && a.maxY >= b.minY;
  }

  private detectCollision(bodyA: RigidBody, bodyB: RigidBody): CollisionManifold | null {
    if (bodyA.shape.type === 'circle' && bodyB.shape.type === 'circle') {
      return this.circleVsCircle(bodyA, bodyB);
    } else if (bodyA.shape.type === 'circle' && bodyB.shape.type === 'rectangle') {
      return this.circleVsRectangle(bodyA, bodyB);
    } else if (bodyA.shape.type === 'rectangle' && bodyB.shape.type === 'circle') {
      const manifold = this.circleVsRectangle(bodyB, bodyA);
      if (manifold) {
        manifold.bodyA = bodyA;
        manifold.bodyB = bodyB;
        manifold.normal = this.negate(manifold.normal);
      }
      return manifold;
    } else if (bodyA.shape.type === 'rectangle' && bodyB.shape.type === 'rectangle') {
      return this.rectangleVsRectangle(bodyA, bodyB);
    }
    return null;
  }

  private circleVsCircle(a: RigidBody, b: RigidBody): CollisionManifold | null {
    const radiusA = a.shape.radius;
    const radiusB = b.shape.radius;
    const totalRadius = radiusA + radiusB;

    const diff = this.subtract(b.position, a.position);
    const distSq = this.dot(diff, diff);

    if (distSq > totalRadius * totalRadius) {
      return null;
    }

    const dist = Math.sqrt(distSq);
    let normal: Vector2D;
    let penetration: number;

    if (dist === 0) {
      normal = { x: 1, y: 0 };
      penetration = radiusA;
    } else {
      normal = { x: diff.x / dist, y: diff.y / dist };
      penetration = totalRadius - dist;
    }

    const contactPoint = {
      x: a.position.x + normal.x * radiusA,
      y: a.position.y + normal.y * radiusA,
    };

    return {
      bodyA: a,
      bodyB: b,
      normal,
      penetration,
      contactPoints: [contactPoint],
    };
  }

  private circleVsRectangle(circle: RigidBody, rect: RigidBody): CollisionManifold | null {
    const radius = circle.shape.radius;
    const hw = rect.shape.width / 2;
    const hh = rect.shape.height / 2;

    const localCircle = {
      x: circle.position.x - rect.position.x,
      y: circle.position.y - rect.position.y,
    };

    const closestX = Math.max(-hw, Math.min(hw, localCircle.x));
    const closestY = Math.max(-hh, Math.min(hh, localCircle.y));
    const closest = { x: closestX, y: closestY };

    const diff = this.subtract(localCircle, closest);
    const distSq = this.dot(diff, diff);

    if (distSq > radius * radius) {
      return null;
    }

    const dist = Math.sqrt(distSq);
    let normal: Vector2D;
    let penetration: number;

    if (dist === 0) {
      normal = { x: 0, y: -1 };
      penetration = radius;
    } else {
      normal = { x: diff.x / dist, y: diff.y / dist };
      penetration = radius - dist;
    }

    const worldNormal = {
      x: normal.x * Math.cos(rect.angle) - normal.y * Math.sin(rect.angle),
      y: normal.x * Math.sin(rect.angle) + normal.y * Math.cos(rect.angle),
    };

    const contactPoint = {
      x: circle.position.x - normal.x * radius,
      y: circle.position.y - normal.y * radius,
    };

    return {
      bodyA: circle,
      bodyB: rect,
      normal: worldNormal,
      penetration,
      contactPoints: [contactPoint],
    };
  }

  private rectangleVsRectangle(a: RigidBody, b: RigidBody): CollisionManifold | null {
    const hwA = a.shape.width / 2;
    const hhA = a.shape.height / 2;
    const hwB = b.shape.width / 2;
    const hhB = b.shape.height / 2;

    const diff = this.subtract(b.position, a.position);
    const overlapX = hwA + hwB - Math.abs(diff.x);
    const overlapY = hhA + hhB - Math.abs(diff.y);

    if (overlapX <= 0 || overlapY <= 0) {
      return null;
    }

    let normal: Vector2D;
    let penetration: number;
    let contactPoint: Vector2D;

    if (overlapX < overlapY) {
      normal = { x: diff.x < 0 ? -1 : 1, y: 0 };
      penetration = overlapX;
      contactPoint = {
        x: a.position.x + (diff.x < 0 ? -hwA : hwA),
        y: a.position.y,
      };
    } else {
      normal = { x: 0, y: diff.y < 0 ? -1 : 1 };
      penetration = overlapY;
      contactPoint = {
        x: a.position.x,
        y: a.position.y + (diff.y < 0 ? -hhA : hhA),
      };
    }

    return {
      bodyA: a,
      bodyB: b,
      normal,
      penetration,
      contactPoints: [contactPoint],
    };
  }

  private resolveCollision(manifold: CollisionManifold): void {
    const { bodyA, bodyB, normal, penetration, contactPoints } = manifold;

    if (bodyA.isStatic && bodyB.isStatic) return;

    const restitution = Math.min(bodyA.restitution, bodyB.restitution);
    const friction = (bodyA.friction + bodyB.friction) / 2;

    const relativeVelocity = this.subtract(bodyB.velocity, bodyA.velocity);
    const velocityAlongNormal = this.dot(relativeVelocity, normal);

    if (velocityAlongNormal > 0) return;

    const j = -(1 + restitution) * velocityAlongNormal;
    const invMassSum = bodyA.inverseMass + bodyB.inverseMass;
    const impulse = j / invMassSum;

    const impulseVec = { x: impulse * normal.x, y: impulse * normal.y };

    bodyA.velocity.x -= impulseVec.x * bodyA.inverseMass;
    bodyA.velocity.y -= impulseVec.y * bodyA.inverseMass;
    bodyB.velocity.x += impulseVec.x * bodyB.inverseMass;
    bodyB.velocity.y += impulseVec.y * bodyB.inverseMass;

    for (const contact of contactPoints) {
      const rA = this.subtract(contact, bodyA.position);
      const rB = this.subtract(contact, bodyB.position);

      const velA = this.add(bodyA.velocity, {
        x: -bodyA.angularVelocity * rA.y,
        y: bodyA.angularVelocity * rA.x,
      });
      const velB = this.add(bodyB.velocity, {
        x: -bodyB.angularVelocity * rB.y,
        y: bodyB.angularVelocity * rB.x,
      });
      const relVel = this.subtract(velB, velA);
      const contactVel = this.dot(relVel, normal);

      if (contactVel > 0) continue;

      const rACrossN = rA.x * normal.y - rA.y * normal.x;
      const rBCrossN = rB.x * normal.y - rB.y * normal.x;
      const invMassSumContact = bodyA.inverseMass + bodyB.inverseMass +
        rACrossN * rACrossN * bodyA.inverseInertia +
        rBCrossN * rBCrossN * bodyB.inverseInertia;

      const jContact = -contactVel / invMassSumContact;

      const tangent = {
        x: relVel.x - normal.x * contactVel,
        y: relVel.y - normal.y * contactVel,
      };
      const tangentLen = this.length(tangent);
      if (tangentLen > 0.0001) {
        tangent.x /= tangentLen;
        tangent.y /= tangentLen;
      }

      const jTangent = -this.dot(relVel, tangent) / invMassSumContact;
      const maxFriction = Math.sqrt(jContact * jContact) * friction;

      const frictionImpulse = {
        x: -jTangent * tangent.x * Math.min(1, Math.abs(jTangent) / Math.abs(jContact)),
        y: -jTangent * tangent.y * Math.min(1, Math.abs(jTangent) / Math.abs(jContact)),
      };

      bodyA.velocity.x -= frictionImpulse.x * bodyA.inverseMass;
      bodyA.velocity.y -= frictionImpulse.y * bodyA.inverseMass;
      bodyB.velocity.x += frictionImpulse.x * bodyB.inverseMass;
      bodyB.velocity.y += frictionImpulse.y * bodyB.inverseMass;
    }

    bodyA.position.x -= normal.x * penetration * (bodyA.inverseMass / invMassSum);
    bodyA.position.y -= normal.y * penetration * (bodyA.inverseMass / invMassSum);
    bodyB.position.x += normal.x * penetration * (bodyB.inverseMass / invMassSum);
    bodyB.position.y += normal.y * penetration * (bodyB.inverseMass / invMassSum);
  }

  private calculateArea(shape: CollisionShape): number {
    if (shape.type === 'circle') {
      return Math.PI * shape.radius * shape.radius;
    } else if (shape.type === 'rectangle') {
      return shape.width * shape.height;
    } else {
      let area = 0;
      for (let i = 0; i < shape.vertices.length; i++) {
        const j = (i + 1) % shape.vertices.length;
        area += shape.vertices[i].x * shape.vertices[j].y;
        area -= shape.vertices[j].x * shape.vertices[i].y;
      }
      return Math.abs(area) / 2;
    }
  }

  private calculateInertia(shape: CollisionShape, mass: number): number {
    if (shape.type === 'circle') {
      return 0.5 * mass * shape.radius * shape.radius;
    } else if (shape.type === 'rectangle') {
      return (mass / 12) * (shape.width * shape.width + shape.height * shape.height);
    } else {
      let numerator = 0;
      let denominator = 0;
      for (let i = 0; i < shape.vertices.length; i++) {
        const j = (i + 1) % shape.vertices.length;
        const cross = Math.abs(this.cross2D(shape.vertices[i], shape.vertices[j]));
        numerator += cross * (
          shape.vertices[i].x * shape.vertices[i].x +
          shape.vertices[i].x * shape.vertices[j].x +
          shape.vertices[j].x * shape.vertices[j].x +
          shape.vertices[i].y * shape.vertices[i].y +
          shape.vertices[i].y * shape.vertices[j].y +
          shape.vertices[j].y * shape.vertices[j].y
        );
        denominator += cross;
      }
      return (mass * numerator) / (6 * denominator);
    }
  }

  private cross2D(a: Vector2D, b: Vector2D): number {
    return a.x * b.y - a.y * b.x;
  }

  private add(a: Vector2D, b: Vector2D): Vector2D {
    return { x: a.x + b.x, y: a.y + b.y };
  }

  private subtract(a: Vector2D, b: Vector2D): Vector2D {
    return { x: a.x - b.x, y: a.y - b.y };
  }

  private negate(v: Vector2D): Vector2D {
    return { x: -v.x, y: -v.y };
  }

  private dot(a: Vector2D, b: Vector2D): number {
    return a.x * b.x + a.y * b.y;
  }

  private length(v: Vector2D): number {
    return Math.sqrt(v.x * v.x + v.y * v.y);
  }

  public raycast(origin: Vector2D, direction: Vector2D, maxDistance: number): { body: RigidBody; point: Vector2D; normal: Vector2D } | null {
    let closestDist = maxDistance;
    let result: { body: RigidBody; point: Vector2D; normal: Vector2D } | null = null;

    for (const body of this.bodies.values()) {
      if (body.isStatic) continue;

      const hit = this.raycastBody(origin, direction, maxDistance, body);
      if (hit && hit.distance < closestDist) {
        closestDist = hit.distance;
        result = { body, point: hit.point, normal: hit.normal };
      }
    }

    return result;
  }

  private raycastBody(origin: Vector2D, direction: Vector2D, maxDistance: number, body: RigidBody): { point: Vector2D; normal: Vector2D; distance: number } | null {
    if (body.shape.type === 'circle') {
      const oc = this.subtract(origin, body.position);
      const a = this.dot(direction, direction);
      const b = 2 * this.dot(oc, direction);
      const c = this.dot(oc, oc) - body.shape.radius * body.shape.radius;
      const discriminant = b * b - 4 * a * c;

      if (discriminant < 0) return null;

      const t1 = (-b - Math.sqrt(discriminant)) / (2 * a);
      const t2 = (-b + Math.sqrt(discriminant)) / (2 * a);

      if (t1 > 0 && t1 < maxDistance) {
        const point = { x: origin.x + direction.x * t1, y: origin.y + direction.y * t1 };
        const normal = this.normalize(this.subtract(point, body.position));
        return { point, normal, distance: t1 };
      }

      if (t2 > 0 && t2 < maxDistance) {
        const point = { x: origin.x + direction.x * t2, y: origin.y + direction.y * t2 };
        const normal = this.normalize(this.subtract(point, body.position));
        return { point, normal, distance: t2 };
      }
    }

    return null;
  }

  private normalize(v: Vector2D): Vector2D {
    const len = this.length(v);
    if (len === 0) return { x: 0, y: 0 };
    return { x: v.x / len, y: v.y / len };
  }

  public getTime(): number {
    return this.time;
  }

  public getConfig(): PhysicsWorldConfig {
    return { ...this.config };
  }

  public setConfig(config: Partial<PhysicsWorldConfig>): void {
    this.config = { ...this.config, ...config };
  }

  public clear(): void {
    this.bodies.clear();
    this.staticBodies.clear();
    this.sleepingBodies.clear();
    this.forces.clear();
    this.collisionPairs.clear();
    this.time = 0;
  }
}

export class PhysicsMaterialLibrary {
  private static materials: Map<string, PhysicsMaterial> = new Map();

  static {
    this.materials.set('steel', { density: 7.8, restitution: 0.3, friction: 0.6 });
    this.materials.set('wood', { density: 0.6, restitution: 0.4, friction: 0.5 });
    this.materials.set('rubber', { density: 1.2, restitution: 0.8, friction: 0.9 });
    this.materials.set('ice', { density: 0.9, restitution: 0.1, friction: 0.05 });
    this.materials.set('concrete', { density: 2.4, restitution: 0.2, friction: 0.7 });
    this.materials.set('metal', { density: 8.0, restitution: 0.4, friction: 0.4 });
    this.materials.set('plastic', { density: 1.2, restitution: 0.5, friction: 0.3 });
  }

  static get(name: string): PhysicsMaterial | undefined {
    return this.materials.get(name);
  }

  static register(name: string, material: PhysicsMaterial): void {
    this.materials.set(name, material);
  }

  static getAll(): Map<string, PhysicsMaterial> {
    return new Map(this.materials);
  }
}

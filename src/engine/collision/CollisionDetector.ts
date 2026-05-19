
import { Vector2D, Position, AABB, Circle, Rectangle, Polygon } from '../../entities/Entity';

export interface Collider {
  id: string;
  type: 'circle' | 'rectangle' | 'polygon' | 'capsule';
  transform: Transform2D;
  isTrigger: boolean;
  layer: number;
  mask: number;
}

export interface Transform2D {
  position: Vector2D;
  rotation: number;
  scale: Vector2D;
}

export interface CollisionInfo {
  colliderA: Collider;
  colliderB: Collider;
  normal: Vector2D;
  penetration: number;
  contactPoints: Vector2D[];
}

export class CollisionDetector {
  private colliders: Map<string, Collider> = new Map();
  private layerMatrix: boolean[][] = [];
  private maxLayers: number = 32;

  constructor() {
    for (let i = 0; i < this.maxLayers; i++) {
      this.layerMatrix[i] = [];
      for (let j = 0; j < this.maxLayers; j++) {
        this.layerMatrix[i][j] = true;
      }
    }
  }

  public addCollider(collider: Collider): void {
    this.colliders.set(collider.id, collider);
  }

  public removeCollider(id: string): void {
    this.colliders.delete(id);
  }

  public getCollider(id: string): Collider | undefined {
    return this.colliders.get(id);
  }

  public setLayerCollision(layerA: number, layerB: number, enabled: boolean): void {
    if (layerA < this.maxLayers && layerB < this.maxLayers) {
      this.layerMatrix[layerA][layerB] = enabled;
      this.layerMatrix[layerB][layerA] = enabled;
    }
  }

  public areLayersColliding(layerA: number, layerB: number): boolean {
    if (layerA >= this.maxLayers || layerB >= this.maxLayers) return true;
    return this.layerMatrix[layerA][layerB];
  }

  public checkCollisions(): CollisionInfo[] {
    const collisions: CollisionInfo[] = [];
    const colliderList = Array.from(this.colliders.values());

    for (let i = 0; i < colliderList.length; i++) {
      for (let j = i + 1; j < colliderList.length; j++) {
        const a = colliderList[i];
        const b = colliderList[j];

        if (!this.areLayersColliding(a.layer, b.layer)) continue;
        if ((a.mask & b.layer) === 0) continue;
        if ((b.mask & a.layer) === 0) continue;

        const collision = this.checkCollision(a, b);
        if (collision) {
          collisions.push(collision);
        }
      }
    }

    return collisions;
  }

  private checkCollision(a: Collider, b: Collider): CollisionInfo | null {
    if (a.type === 'circle' && b.type === 'circle') {
      return this.circleVsCircle(a, b);
    } else if (a.type === 'circle' && b.type === 'rectangle') {
      return this.circleVsRectangle(a, b);
    } else if (a.type === 'rectangle' && b.type === 'circle') {
      const result = this.circleVsRectangle(b, a);
      if (result) {
        result.normal = { x: -result.normal.x, y: -result.normal.y };
        [result.colliderA, result.colliderB] = [result.colliderB, result.colliderA];
      }
      return result;
    } else if (a.type === 'rectangle' && b.type === 'rectangle') {
      return this.rectangleVsRectangle(a, b);
    } else if (a.type === 'polygon' || b.type === 'polygon') {
      return this.polygonVsPolygon(
        a.type === 'polygon' ? a : this.rectangleToPolygon(b),
        b.type === 'polygon' ? b : this.rectangleToPolygon(a)
      );
    }

    return null;
  }

  private circleVsCircle(a: Collider, b: Collider): CollisionInfo | null {
    const posA = a.transform.position;
    const posB = b.transform.position;
    const radiusA = (a.transform.scale.x + a.transform.scale.y) / 2;
    const radiusB = (b.transform.scale.x + b.transform.scale.y) / 2;

    const dx = posB.x - posA.x;
    const dy = posB.y - posA.y;
    const distSq = dx * dx + dy * dy;
    const radiusSum = radiusA + radiusB;

    if (distSq > radiusSum * radiusSum) {
      return null;
    }

    const dist = Math.sqrt(distSq);
    let normal: Vector2D;
    let penetration: number;

    if (dist === 0) {
      normal = { x: 1, y: 0 };
      penetration = radiusA;
    } else {
      normal = { x: dx / dist, y: dy / dist };
      penetration = radiusSum - dist;
    }

    const contactPoint = {
      x: posA.x + normal.x * radiusA,
      y: posA.y + normal.y * radiusA,
    };

    return {
      colliderA: a,
      colliderB: b,
      normal,
      penetration,
      contactPoints: [contactPoint],
    };
  }

  private circleVsRectangle(circle: Collider, rect: Collider): CollisionInfo | null {
    const circlePos = circle.transform.position;
    const rectPos = rect.transform.position;
    const rectWidth = rect.transform.scale.x;
    const rectHeight = rect.transform.scale.y;
    const radius = (circle.transform.scale.x + circle.transform.scale.y) / 2;

    const cos = Math.cos(rect.transform.rotation);
    const sin = Math.sin(rect.transform.rotation);

    const localCircleX = cos * (circlePos.x - rectPos.x) + sin * (circlePos.y - rectPos.y);
    const localCircleY = -sin * (circlePos.x - rectPos.x) + cos * (circlePos.y - rectPos.y);

    const closestX = Math.max(-rectWidth / 2, Math.min(rectWidth / 2, localCircleX));
    const closestY = Math.max(-rectHeight / 2, Math.min(rectHeight / 2, localCircleY));

    const dx = localCircleX - closestX;
    const dy = localCircleY - closestY;
    const distSq = dx * dx + dy * dy;

    if (distSq > radius * radius && distSq !== 0) {
      return null;
    }

    const dist = Math.sqrt(distSq);
    let normal: Vector2D;
    let penetration: number;

    if (dist === 0) {
      const outsideDistX = rectWidth / 2 - Math.abs(localCircleX);
      const outsideDistY = rectHeight / 2 - Math.abs(localCircleY);

      if (outsideDistX < outsideDistY) {
        normal = { x: localCircleX > 0 ? 1 : -1, y: 0 };
      } else {
        normal = { x: 0, y: localCircleY > 0 ? 1 : -1 };
      }
      penetration = Math.min(outsideDistX, outsideDistY) + radius;
    } else {
      normal = { x: dx / dist, y: dy / dist };
      penetration = radius - dist;
    }

    const worldNormal = {
      x: cos * normal.x - sin * normal.y,
      y: sin * normal.x + cos * normal.y,
    };

    const contactPoint = {
      x: circlePos.x - normal.x * radius,
      y: circlePos.y - normal.y * radius,
    };

    return {
      colliderA: circle,
      colliderB: rect,
      normal: worldNormal,
      penetration,
      contactPoints: [contactPoint],
    };
  }

  private rectangleVsRectangle(a: Collider, b: Collider): CollisionInfo | null {
    const posA = a.transform.position;
    const posB = b.transform.position;
    const sizeA = a.transform.scale;
    const sizeB = b.transform.scale;

    const cosA = Math.cos(a.transform.rotation);
    const sinA = Math.sin(a.transform.rotation);
    const cosB = Math.cos(b.transform.rotation);
    const sinB = Math.sin(b.transform.rotation);

    const verticesA = this.getRectangleVertices(posA, sizeA, cosA, sinA);
    const verticesB = this.getRectangleVertices(posB, sizeB, cosB, sinB);

    const axes: Vector2D[] = [];

    for (let i = 0; i < 4; i++) {
      const next = (i + 1) % 4;
      const edge = { x: verticesA[next].x - verticesA[i].x, y: verticesA[next].y - verticesA[i].y };
      axes.push(this.perp(edge));
    }

    for (let i = 0; i < 4; i++) {
      const next = (i + 1) % 4;
      const edge = { x: verticesB[next].x - verticesB[i].x, y: verticesB[next].y - verticesB[i].y };
      axes.push(this.perp(edge));
    }

    let minPenetration = Infinity;
    let minAxis: Vector2D | null = null;

    for (const axis of axes) {
      const projA = this.projectVertices(verticesA, axis);
      const projB = this.projectVertices(verticesB, axis);

      const overlap = Math.min(projA.max - projB.min, projB.max - projA.min);
      if (overlap <= 0) {
        return null;
      }

      if (overlap < minPenetration) {
        minPenetration = overlap;
        minAxis = axis;
      }
    }

    if (!minAxis) return null;

    const direction = { x: posB.x - posA.x, y: posB.y - posA.y };
    if (this.dot(direction, minAxis) < 0) {
      minAxis = { x: -minAxis.x, y: -minAxis.y };
    }

    const contactPoints = this.findContactPoints(verticesA, verticesB, minAxis);

    return {
      colliderA: a,
      colliderB: b,
      normal: minAxis,
      penetration: minPenetration,
      contactPoints,
    };
  }

  private polygonVsPolygon(a: Collider, b: Collider): CollisionInfo | null {
    const verticesA = this.getPolygonVertices(a);
    const verticesB = this.getPolygonVertices(b);

    const axes: Vector2D[] = [];

    for (let i = 0; i < verticesA.length; i++) {
      const next = (i + 1) % verticesA.length;
      const edge = { x: verticesA[next].x - verticesA[i].x, y: verticesA[next].y - verticesA[i].y };
      axes.push(this.normalize(this.perp(edge)));
    }

    for (let i = 0; i < verticesB.length; i++) {
      const next = (i + 1) % verticesB.length;
      const edge = { x: verticesB[next].x - verticesB[i].x, y: verticesB[next].y - verticesB[i].y };
      axes.push(this.normalize(this.perp(edge)));
    }

    let minPenetration = Infinity;
    let minAxis: Vector2D | null = null;

    for (const axis of axes) {
      const projA = this.projectVertices(verticesA, axis);
      const projB = this.projectVertices(verticesB, axis);

      const overlap = Math.min(projA.max - projB.min, projB.max - projA.min);
      if (overlap <= 0) {
        return null;
      }

      if (overlap < minPenetration) {
        minPenetration = overlap;
        minAxis = axis;
      }
    }

    if (!minAxis) return null;

    const contactPoints = this.findContactPoints(verticesA, verticesB, minAxis);

    return {
      colliderA: a,
      colliderB: b,
      normal: minAxis,
      penetration: minPenetration,
      contactPoints,
    };
  }

  private getRectangleVertices(pos: Vector2D, size: Vector2D, cos: number, sin: number): Vector2D[] {
    const hw = size.x / 2;
    const hh = size.y / 2;
    const localVerts = [
      { x: -hw, y: -hh },
      { x: hw, y: -hh },
      { x: hw, y: hh },
      { x: -hw, y: hh },
    ];

    return localVerts.map(v => ({
      x: pos.x + cos * v.x - sin * v.y,
      y: pos.y + sin * v.x + cos * v.y,
    }));
  }

  private rectangleToPolygon(rect: Collider): Collider {
    return {
      ...rect,
      type: 'polygon',
      transform: {
        ...rect.transform,
        scale: { x: 1, y: 1 },
      },
    };
  }

  private getPolygonVertices(polygon: Collider): Vector2D[] {
    const pos = polygon.transform.position;
    const cos = Math.cos(polygon.transform.rotation);
    const sin = Math.sin(polygon.transform.rotation);
    const scale = polygon.transform.scale;

    const verts = (polygon as any).vertices || [
      { x: -0.5, y: -0.5 },
      { x: 0.5, y: -0.5 },
      { x: 0.5, y: 0.5 },
      { x: -0.5, y: 0.5 },
    ];

    return verts.map((v: Vector2D) => ({
      x: pos.x + cos * v.x * scale.x - sin * v.y * scale.y,
      y: pos.y + sin * v.x * scale.x + cos * v.y * scale.y,
    }));
  }

  private projectVertices(vertices: Vector2D[], axis: Vector2D): { min: number; max: number } {
    let min = Infinity;
    let max = -Infinity;

    for (const v of vertices) {
      const proj = this.dot(v, axis);
      min = Math.min(min, proj);
      max = Math.max(max, proj);
    }

    return { min, max };
  }

  private perp(v: Vector2D): Vector2D {
    return { x: -v.y, y: v.x };
  }

  private dot(a: Vector2D, b: Vector2D): number {
    return a.x * b.x + a.y * b.y;
  }

  private normalize(v: Vector2D): Vector2D {
    const len = Math.sqrt(v.x * v.x + v.y * v.y);
    if (len === 0) return { x: 0, y: 0 };
    return { x: v.x / len, y: v.y / len };
  }

  private findContactPoints(vertsA: Vector2D[], vertsB: Vector2D[], normal: Vector2D): Vector2D[] {
    const contacts: Vector2D[] = [];

    for (const v of vertsA) {
      if (this.isPointInsidePolygon(v, vertsB)) {
        contacts.push(v);
      }
    }

    for (const v of vertsB) {
      if (this.isPointInsidePolygon(v, vertsA)) {
        contacts.push(v);
      }
    }

    if (contacts.length === 0) {
      const edgeA = this.findClosestEdge(vertsA, normal);
      const edgeB = this.findClosestEdge(vertsB, { x: -normal.x, y: -normal.y });

      contacts.push(this.findEdgeIntersection(edgeA[0], edgeA[1], edgeB[0], edgeB[1]));
    }

    return contacts.length > 2 ? contacts.slice(0, 2) : contacts;
  }

  private isPointInsidePolygon(point: Vector2D, vertices: Vector2D[]): boolean {
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

  private findClosestEdge(vertices: Vector2D[], direction: Vector2D): [Vector2D, Vector2D] {
    let maxProj = -Infinity;
    let edge: [Vector2D, Vector2D] = [vertices[0], vertices[1]];

    for (let i = 0; i < vertices.length; i++) {
      const next = (i + 1) % vertices.length;
      const midpoint = {
        x: (vertices[i].x + vertices[next].x) / 2,
        y: (vertices[i].y + vertices[next].y) / 2,
      };
      const proj = this.dot(midpoint, direction);
      if (proj > maxProj) {
        maxProj = proj;
        edge = [vertices[i], vertices[next]];
      }
    }

    return edge;
  }

  private findEdgeIntersection(a1: Vector2D, a2: Vector2D, b1: Vector2D, b2: Vector2D): Vector2D {
    const d1 = { x: a2.x - a1.x, y: a2.y - a1.y };
    const d2 = { x: b2.x - b1.x, y: b2.y - b1.y };

    const cross = d1.x * d2.y - d1.y * d2.x;
    if (Math.abs(cross) < 0.0001) {
      return { x: (a1.x + b1.x) / 2, y: (a1.y + b1.y) / 2 };
    }

    const t = ((b1.x - a1.x) * d2.y - (b1.y - a1.y) * d2.x) / cross;
    return { x: a1.x + t * d1.x, y: a1.y + t * d1.y };
  }

  public raycast(origin: Vector2D, direction: Vector2D, maxDistance: number, layerMask?: number): { point: Vector2D; normal: Vector2D; collider: Collider; distance: number } | null {
    let closestHit: { point: Vector2D; normal: Vector2D; collider: Collider; distance: number } | null = null;

    for (const collider of this.colliders.values()) {
      if (layerMask !== undefined && (collider.layer & layerMask) === 0) continue;

      const hit = this.raycastCollider(origin, direction, maxDistance, collider);
      if (hit && (!closestHit || hit.distance < closestHit.distance)) {
        closestHit = hit;
      }
    }

    return closestHit;
  }

  private raycastCollider(origin: Vector2D, direction: Vector2D, maxDistance: number, collider: Collider): { point: Vector2D; normal: Vector2D; collider: Collider; distance: number } | null {
    if (collider.type === 'circle') {
      const pos = collider.transform.position;
      const radius = (collider.transform.scale.x + collider.transform.scale.y) / 2;

      const oc = { x: origin.x - pos.x, y: origin.y - pos.y };
      const a = this.dot(direction, direction);
      const b = 2 * this.dot(oc, direction);
      const c = this.dot(oc, oc) - radius * radius;
      const discriminant = b * b - 4 * a * c;

      if (discriminant < 0) return null;

      const t1 = (-b - Math.sqrt(discriminant)) / (2 * a);
      const t2 = (-b + Math.sqrt(discriminant)) / (2 * a);

      if (t1 > 0 && t1 < maxDistance) {
        const point = { x: origin.x + direction.x * t1, y: origin.y + direction.y * t1 };
        const normal = this.normalize({ x: point.x - pos.x, y: point.y - pos.y });
        return { point, normal, collider, distance: t1 };
      }

      if (t2 > 0 && t2 < maxDistance) {
        const point = { x: origin.x + direction.x * t2, y: origin.y + direction.y * t2 };
        const normal = this.normalize({ x: point.x - pos.x, y: point.y - pos.y });
        return { point, normal, collider, distance: t2 };
      }
    } else if (collider.type === 'rectangle') {
      const vertices = this.getRectangleVertices(
        collider.transform.position,
        collider.transform.scale,
        Math.cos(collider.transform.rotation),
        Math.sin(collider.transform.rotation)
      );

      let minT = Infinity;
      let hitPoint: Vector2D | null = null;
      let hitNormal: Vector2D | null = null;

      for (let i = 0; i < vertices.length; i++) {
        const next = (i + 1) % vertices.length;
        const edge = { x: vertices[next].x - vertices[i].x, y: vertices[next].y - vertices[i].y };
        const edgeNormal = this.normalize(this.perp(edge));

        const denom = this.dot(direction, edgeNormal);
        if (Math.abs(denom) < 0.0001) continue;

        const t = this.dot({ x: vertices[i].x - origin.x, y: vertices[i].y - origin.y }, edgeNormal) / denom;
        if (t < 0 || t > maxDistance || t > minT) continue;

        const point = { x: origin.x + direction.x * t, y: origin.y + direction.y * t };

        if (this.isPointOnSegment(point, vertices[i], vertices[next])) {
          if (t < minT) {
            minT = t;
            hitPoint = point;
            hitNormal = denom < 0 ? edgeNormal : { x: -edgeNormal.x, y: -edgeNormal.y };
          }
        }
      }

      if (hitPoint && hitNormal) {
        return { point: hitPoint, normal: hitNormal, collider, distance: minT };
      }
    }

    return null;
  }

  private isPointOnSegment(point: Vector2D, a: Vector2D, b: Vector2D): boolean {
    const ap = { x: point.x - a.x, y: point.y - a.y };
    const ab = { x: b.x - a.x, y: b.y - a.y };
    const dot = this.dot(ap, ab);
    const lenSq = this.dot(ab, ab);
    return dot >= 0 && dot <= lenSq;
  }

  public queryPoint(point: Vector2D, layerMask?: number): Collider[] {
    const results: Collider[] = [];

    for (const collider of this.colliders.values()) {
      if (layerMask !== undefined && (collider.layer & layerMask) === 0) continue;

      if (this.pointInsideCollider(point, collider)) {
        results.push(collider);
      }
    }

    return results;
  }

  private pointInsideCollider(point: Vector2D, collider: Collider): boolean {
    if (collider.type === 'circle') {
      const pos = collider.transform.position;
      const radius = (collider.transform.scale.x + collider.transform.scale.y) / 2;
      const dx = point.x - pos.x;
      const dy = point.y - pos.y;
      return dx * dx + dy * dy <= radius * radius;
    } else if (collider.type === 'rectangle') {
      const vertices = this.getRectangleVertices(
        collider.transform.position,
        collider.transform.scale,
        Math.cos(collider.transform.rotation),
        Math.sin(collider.transform.rotation)
      );
      return this.isPointInsidePolygon(point, vertices);
    }

    return false;
  }

  public queryArea(center: Vector2D, radius: number, layerMask?: number): Collider[] {
    const results: Collider[] = [];

    for (const collider of this.colliders.values()) {
      if (layerMask !== undefined && (collider.layer & layerMask) === 0) continue;

      const pos = collider.transform.position;
      const dx = point.x - pos.x;
      const dy = point.y - pos.y;
      if (dx * dx + dy * dy <= radius * radius) {
        results.push(collider);
      }
    }

    return results;
  }

  public clear(): void {
    this.colliders.clear();
  }

  public getAllColliders(): Collider[] {
    return Array.from(this.colliders.values());
  }
}

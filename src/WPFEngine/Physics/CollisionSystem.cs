using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.ECS;
using WPFEngine.Scene;

namespace WPFEngine.Physics
{
    public enum CollisionShapeType
    {
        None,
        Circle,
        Box,
        Polygon,
        Capsule,
        Edge
    }

    public enum CollisionType
    {
        None,
        Enter,
        Stay,
        Exit
    }

    public enum CollisionLayer
    {
        Default = 0,
        Layer1 = 1,
        Layer2 = 2,
        Layer3 = 3,
        Layer4 = 4,
        Layer5 = 5,
        Layer6 = 6,
        Layer7 = 7,
        Layer8 = 8,
        Player = 9,
        Enemy = 10,
        Projectile = 11,
        Environment = 12,
        Pickup = 13,
        Trigger = 14
    }

    public class CollisionSystem : IDisposable
    {
        private static CollisionSystem instance;
        private Dictionary<Collider2D, List<Collider2D>> overlappingColliders;
        private List<CollisionPair> collisionPairs;
        private List<CollisionPair> newCollisionPairs;
        private List<CollisionInfo> activeCollisions;
        private List<CollisionInfo> triggerCollisions;
        private Dictionary<string, LayerCollisionConfig> layerConfigs;
        private int maxIterations;
        private float sleepThreshold;
        private float contactOffset;
        private float fixedDeltaTime;
        private bool isProcessing;
        private List<Collider2D> allColliders;
        private Dictionary<Collider2D, bool> collisionMatrix;
        private Dictionary<int, List<Collider2D>> spatialGrid;
        private int gridCellSize;
        private bool useSpatialPartitioning;
        private bool isEnabled;
        private CollisionDetectionMode detectionMode;
        private CollisionResponseMode responseMode;
        private int maxSubSteps;
        private float fixedUpdateTime;
        private float accumulator;

        public event EventHandler<CollisionEventArgs> CollisionEnter;
        public event EventHandler<CollisionEventArgs> CollisionStay;
        public event EventHandler<CollisionEventArgs> CollisionExit;
        public event EventHandler<CollisionEventArgs> TriggerEnter;
        public event EventHandler<CollisionEventArgs> TriggerStay;
        public event EventHandler<CollisionEventArgs> TriggerExit;

        public static CollisionSystem Instance
        {
            get
            {
                if (instance == null)
                    instance = new CollisionSystem();
                return instance;
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        public CollisionDetectionMode DetectionMode
        {
            get => detectionMode;
            set => detectionMode = value;
        }

        public CollisionResponseMode ResponseMode
        {
            get => responseMode;
            set => responseMode = value;
        }

        public float ContactOffset
        {
            get => contactOffset;
            set => contactOffset = Math.Max(0.001f, value);
        }

        public float SleepThreshold
        {
            get => sleepThreshold;
            set => sleepThreshold = Math.Max(0f, value);
        }

        public int MaxIterations
        {
            get => maxIterations;
            set => maxIterations = Math.Max(1, value);
        }

        public int GridCellSize
        {
            get => gridCellSize;
            set
            {
                gridCellSize = Math.Max(10, value);
                RebuildSpatialGrid();
            }
        }

        public bool UseSpatialPartitioning
        {
            get => useSpatialPartitioning;
            set => useSpatialPartitioning = value;
        }

        public IReadOnlyList<CollisionInfo> ActiveCollisions => activeCollisions.AsReadOnly();
        public IReadOnlyList<CollisionInfo> TriggerCollisions => triggerCollisions.AsReadOnly();

        public CollisionSystem()
        {
            instance = this;
            overlappingColliders = new Dictionary<Collider2D, List<Collider2D>>();
            collisionPairs = new List<CollisionPair>();
            newCollisionPairs = new List<CollisionPair>();
            activeCollisions = new List<CollisionInfo>();
            triggerCollisions = new List<CollisionInfo>();
            layerConfigs = new Dictionary<string, LayerCollisionConfig>();
            allColliders = new List<Collider2D>();
            collisionMatrix = new Dictionary<Collider2D, bool>();
            spatialGrid = new Dictionary<int, List<Collider2D>>();
            maxIterations = 10;
            sleepThreshold = 0.005f;
            contactOffset = 0.01f;
            fixedDeltaTime = 0.02f;
            isProcessing = false;
            isEnabled = true;
            detectionMode = CollisionDetectionMode.Continuous;
            responseMode = CollisionResponseMode.Callbacks;
            maxSubSteps = 4;
            fixedUpdateTime = 0.02f;
            accumulator = 0f;
            gridCellSize = 100;
            useSpatialPartitioning = true;

            InitializeDefaultLayerConfig();
        }

        private void InitializeDefaultLayerConfig()
        {
            for (int i = 0; i < 16; i++)
            {
                var layerName = $"Layer{i}";
                layerConfigs[layerName] = new LayerCollisionConfig
                {
                    LayerName = layerName,
                    LayerIndex = i,
                    CollidesWith = ~((1 << 16) - 1)
                };
            }
        }

        public void RegisterCollider(Collider2D collider)
        {
            if (!allColliders.Contains(collider))
            {
                allColliders.Add(collider);
                if (useSpatialPartitioning)
                    AddToSpatialGrid(collider);
            }
        }

        public void UnregisterCollider(Collider2D collider)
        {
            if (allColliders.Remove(collider))
            {
                RemoveFromSpatialGrid(collider);
                overlappingColliders.Remove(collider);
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            if (!isEnabled)
                return;

            accumulator += deltaTime;
            var subSteps = 0;

            while (accumulator >= fixedDeltaTime && subSteps < maxSubSteps)
            {
                ProcessCollisions(fixedDeltaTime);
                accumulator -= fixedDeltaTime;
                subSteps++;
            }
        }

        private void ProcessCollisions(float deltaTime)
        {
            isProcessing = true;

            UpdateOverlappingPairs();
            BroadPhase();
            NarrowPhase();

            ProcessCollisionCallbacks();

            isProcessing = false;
        }

        private void UpdateOverlappingPairs()
        {
            var currentPairs = new HashSet<CollisionPair>();
            foreach (var pair in newCollisionPairs)
            {
                currentPairs.Add(pair);
            }

            var oldPairs = new HashSet<CollisionPair>(collisionPairs);
            oldPairs.ExceptWith(currentPairs);

            foreach (var pair in oldPairs)
            {
                if (pair.IsValid())
                {
                    pair.Collider1.OnCollisionExit(pair);
                    pair.Collider2.OnCollisionExit(pair);
                }
            }

            collisionPairs.Clear();
            collisionPairs.AddRange(currentPairs);
        }

        private void BroadPhase()
        {
            newCollisionPairs.Clear();

            if (useSpatialPartitioning)
            {
                BroadPhaseSpatialGrid();
            }
            else
            {
                BroadPhaseBruteForce();
            }
        }

        private void BroadPhaseBruteForce()
        {
            for (int i = 0; i < allColliders.Count; i++)
            {
                var colliderA = allColliders[i];
                if (!colliderA.IsActive || !colliderA.IsEnabled)
                    continue;

                for (int j = i + 1; j < allColliders.Count; j++)
                {
                    var colliderB = allColliders[j];
                    if (!colliderB.IsActive || !colliderB.IsEnabled)
                        continue;

                    if (!CanCollide(colliderA, colliderB))
                        continue;

                    if (IsAABBOverlap(colliderA.Bounds, colliderB.Bounds))
                    {
                        newCollisionPairs.Add(new CollisionPair(colliderA, colliderB));
                    }
                }
            }
        }

        private void BroadPhaseSpatialGrid()
        {
            RebuildSpatialGrid();

            var checkedPairs = new HashSet<long>();

            foreach (var collider in allColliders)
            {
                if (!collider.IsActive || !collider.IsEnabled)
                    continue;

                var cellIndices = GetCellIndices(collider.Bounds);
                var nearbyColliders = new HashSet<Collider2D>();

                foreach (var index in cellIndices)
                {
                    if (spatialGrid.TryGetValue(index, out var cell))
                    {
                        nearbyColliders.UnionWith(cell);
                    }
                }

                foreach (var other in nearbyColliders)
                {
                    if (other == collider || !other.IsActive || !other.IsEnabled)
                        continue;

                    var pairId = GetPairId(collider, other);
                    if (checkedPairs.Contains(pairId))
                        continue;

                    checkedPairs.Add(pairId);

                    if (!CanCollide(collider, other))
                        continue;

                    if (IsAABBOverlap(collider.Bounds, other.Bounds))
                    {
                        newCollisionPairs.Add(new CollisionPair(collider, other));
                    }
                }
            }
        }

        private void RebuildSpatialGrid()
        {
            spatialGrid.Clear();

            for (int i = 0; i < allColliders.Count; i++)
            {
                var collider = allColliders[i];
                if (collider.IsActive && collider.IsEnabled)
                {
                    AddToSpatialGrid(collider);
                }
            }
        }

        private void AddToSpatialGrid(Collider2D collider)
        {
            var indices = GetCellIndices(collider.Bounds);
            foreach (var index in indices)
            {
                if (!spatialGrid.ContainsKey(index))
                    spatialGrid[index] = new List<Collider2D>();
                if (!spatialGrid[index].Contains(collider))
                    spatialGrid[index].Add(collider);
            }
        }

        private void RemoveFromSpatialGrid(Collider2D collider)
        {
            foreach (var cell in spatialGrid.Values)
            {
                cell.Remove(collider);
            }
        }

        private List<int> GetCellIndices(Bounds bounds)
        {
            var indices = new List<int>();
            var minX = (int)Math.Floor(bounds.Min.X / gridCellSize);
            var maxX = (int)Math.Floor(bounds.Max.X / gridCellSize);
            var minY = (int)Math.Floor(bounds.Min.Y / gridCellSize);
            var maxY = (int)Math.Floor(bounds.Max.Y / gridCellSize);

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    indices.Add(GetCellHash(x, y));
                }
            }

            return indices;
        }

        private int GetCellHash(int x, int y)
        {
            return (x * 73856093) ^ (y * 19349663);
        }

        private long GetPairId(Collider2D a, Collider2D b)
        {
            var idA = a.GetInstanceID();
            var idB = b.GetInstanceID();
            return idA < idB ? ((long)idA << 32) | (uint)idB : ((long)idB << 32) | (uint)idA;
        }

        private bool IsAABBOverlap(Bounds a, Bounds b)
        {
            if (a.Max.X < b.Min.X || a.Min.X > b.Max.X)
                return false;
            if (a.Max.Y < b.Min.Y || a.Min.Y > b.Max.Y)
                return false;
            return true;
        }

        private bool CanCollide(Collider2D a, Collider2D b)
        {
            if (a.Layer == b.Layer && !a.LayerSettings.CollidesWithSelf)
                return false;

            if (!a.LayerSettings.CollidesWith(b.Layer))
                return false;

            if (a.MatchesLayer(b.Layer) && !a.LayerSettings.CollidesWithSelf)
                return false;

            return true;
        }

        private void NarrowPhase()
        {
            foreach (var pair in newCollisionPairs)
            {
                if (!pair.IsValid())
                    continue;

                var collision = CheckCollision(pair.Collider1, pair.Collider2);
                if (collision != null && collision.IsColliding)
                {
                    pair.SetCollision(collision);
                }
            }
        }

        private CollisionInfo CheckCollision(Collider2D a, Collider2D b)
        {
            if (a.ShapeType == CollisionShapeType.Circle && b.ShapeType == CollisionShapeType.Circle)
            {
                return CircleVsCircle(a as CircleCollider, b as CircleCollider);
            }
            else if (a.ShapeType == CollisionShapeType.Box && b.ShapeType == CollisionShapeType.Box)
            {
                return BoxVsBox(a as BoxCollider, b as BoxCollider);
            }
            else if (a.ShapeType == CollisionShapeType.Circle && b.ShapeType == CollisionShapeType.Box)
            {
                return CircleVsBox(a as CircleCollider, b as BoxCollider);
            }
            else if (a.ShapeType == CollisionShapeType.Box && b.ShapeType == CollisionShapeType.Circle)
            {
                var result = CircleVsBox(b as CircleCollider, a as BoxCollider);
                if (result != null)
                    result.Normal = -result.Normal;
                return result;
            }
            else if (a.ShapeType == CollisionShapeType.Polygon || b.ShapeType == CollisionShapeType.Polygon)
            {
                return PolygonVsPolygon(a, b);
            }

            return AABBVsAABB(a, b);
        }

        private CollisionInfo CircleVsCircle(CircleCollider a, CircleCollider b)
        {
            if (a == null || b == null)
                return null;

            var posA = a.Transform.WorldPosition;
            var posB = b.Transform.WorldPosition;
            var distance = (posA - posB).Magnitude;
            var radiusSum = a.Radius + b.Radius;

            if (distance >= radiusSum)
                return null;

            var normal = distance > 0.0001f ? (posA - posB).Normalized : Vector2.Right;
            var penetration = radiusSum - distance;
            var contactPoint = posB + normal * b.Radius;

            return new CollisionInfo
            {
                IsColliding = true,
                ColliderA = a,
                ColliderB = b,
                Normal = normal,
                Penetration = penetration,
                ContactPoints = new[] { contactPoint }
            };
        }

        private CollisionInfo BoxVsBox(BoxCollider a, BoxCollider b)
        {
            if (a == null || b == null)
                return null;

            return PolygonVsPolygon(a, b);
        }

        private CollisionInfo CircleVsBox(CircleCollider circle, BoxCollider box)
        {
            if (circle == null || box == null)
                return null;

            var circlePos = circle.Transform.WorldPosition;
            var boxPos = box.Transform.WorldPosition;
            var boxRotation = box.Transform.LocalRotation;

            var localCirclePos = box.Transform.InverseTransformPoint(circlePos);
            var halfSize = box.Size * 0.5f;
            var closestPoint = new Vector2(
                Math.Max(-halfSize.X, Math.Min(halfSize.X, localCirclePos.X)),
                Math.Max(-halfSize.Y, Math.Min(halfSize.Y, localCirclePos.Y))
            );

            var closestWorld = box.Transform.TransformPoint(closestPoint);
            var diff = circlePos - closestWorld;
            var distance = diff.Magnitude;
            var circleRadius = circle.Radius;

            if (distance >= circleRadius)
                return null;

            var normal = distance > 0.0001f ? diff.Normalized : Vector2.Up;
            var penetration = circleRadius - distance;

            return new CollisionInfo
            {
                IsColliding = true,
                ColliderA = circle,
                ColliderB = box,
                Normal = normal,
                Penetration = penetration,
                ContactPoints = new[] { closestWorld }
            };
        }

        private CollisionInfo PolygonVsPolygon(Collider2D a, Collider2D b)
        {
            if (a == null || b == null)
                return null;

            var verticesA = GetWorldVertices(a);
            var verticesB = GetWorldVertices(b);

            var (minOverlap, smallestAxis) = FindMinSeparation(verticesA, verticesB);
            if (minOverlap >= 0)
                return null;

            var (minOverlapB, smallestAxisB) = FindMinSeparation(verticesB, verticesA);
            if (minOverlapB >= 0)
                return null;

            var penetration = Math.Min(Math.Abs(minOverlap), Math.Abs(minOverlapB));
            var normal = smallestAxis;

            if (Math.Abs(minOverlapB) < Math.Abs(minOverlap))
            {
                normal = -smallestAxisB;
                penetration = Math.Abs(minOverlapB);
            }

            var contacts = FindContactPoints(verticesA, verticesB, normal);

            return new CollisionInfo
            {
                IsColliding = true,
                ColliderA = a,
                ColliderB = b,
                Normal = normal,
                Penetration = penetration,
                ContactPoints = contacts
            };
        }

        private CollisionInfo AABBVsAABB(Collider2D a, Collider2D b)
        {
            var boundsA = a.Bounds;
            var boundsB = b.Bounds;

            if (!IsAABBOverlap(boundsA, boundsB))
                return null;

            var overlapX = Math.Min(boundsA.Max.X - boundsB.Min.X, boundsB.Max.X - boundsA.Min.X);
            var overlapY = Math.Min(boundsA.Max.Y - boundsB.Min.Y, boundsB.Max.Y - boundsA.Min.Y);

            Vector2 normal;
            float penetration;

            if (overlapX < overlapY)
            {
                penetration = overlapX;
                normal = boundsA.Center.X < boundsB.Center.X ? Vector2.Left : Vector2.Right;
            }
            else
            {
                penetration = overlapY;
                normal = boundsA.Center.Y < boundsB.Center.Y ? Vector2.Down : Vector2.Up;
            }

            var contactPoint = Vector2.Lerp(boundsA.Center, boundsB.Center, 0.5f);

            return new CollisionInfo
            {
                IsColliding = true,
                ColliderA = a,
                ColliderB = b,
                Normal = normal,
                Penetration = penetration,
                ContactPoints = new[] { contactPoint }
            };
        }

        private Vector2[] GetWorldVertices(Collider2D collider)
        {
            Vector2[] localVertices;

            if (collider is BoxCollider box)
            {
                var halfSize = box.Size * 0.5f;
                localVertices = new[]
                {
                    new Vector2(-halfSize.X, -halfSize.Y),
                    new Vector2(halfSize.X, -halfSize.Y),
                    new Vector2(halfSize.X, halfSize.Y),
                    new Vector2(-halfSize.X, halfSize.Y)
                };
            }
            else if (collider is PolygonCollider polygon && polygon.Vertices != null)
            {
                localVertices = polygon.Vertices;
            }
            else if (collider is CircleCollider circle)
            {
                localVertices = GenerateCircleVertices(circle.Radius, 8);
            }
            else
            {
                var bounds = collider.Bounds;
                var halfSize = bounds.Size * 0.5f;
                localVertices = new[]
                {
                    new Vector2(-halfSize.X, -halfSize.Y),
                    new Vector2(halfSize.X, -halfSize.Y),
                    new Vector2(halfSize.X, halfSize.Y),
                    new Vector2(-halfSize.X, halfSize.Y)
                };
            }

            var worldVertices = new Vector2[localVertices.Length];
            for (int i = 0; i < localVertices.Length; i++)
            {
                worldVertices[i] = collider.Transform.TransformPoint(localVertices[i]);
            }

            return worldVertices;
        }

        private Vector2[] GenerateCircleVertices(float radius, int segments)
        {
            var vertices = new Vector2[segments];
            var angleStep = (float)(Math.PI * 2 / segments);

            for (int i = 0; i < segments; i++)
            {
                var angle = i * angleStep;
                vertices[i] = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );
            }

            return vertices;
        }

        private (float minOverlap, Vector2 smallestAxis) FindMinSeparation(Vector2[] verticesA, Vector2[] verticesB)
        {
            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            for (int i = 0; i < verticesA.Length; i++)
            {
                var vertex = verticesA[i];
                var nextVertex = verticesA[(i + 1) % verticesA.Length];
                var edge = nextVertex - vertex;
                var axis = new Vector2(-edge.Y, edge.X).Normalized;

                var (minA, maxA) = ProjectVertices(verticesA, axis);
                var (minB, maxB) = ProjectVertices(verticesB, axis);

                if (maxA < minB || maxB < minA)
                    return (0f, Vector2.Zero);

                var overlap = Math.Min(maxA - minB, maxB - minA);
                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            return (minOverlap, smallestAxis);
        }

        private (float min, float max) ProjectVertices(Vector2[] vertices, Vector2 axis)
        {
            var min = float.MaxValue;
            var max = float.MinValue;

            foreach (var vertex in vertices)
            {
                var projection = vertex.Dot(axis);
                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            return (min, max);
        }

        private Vector2[] FindContactPoints(Vector2[] verticesA, Vector2[] verticesB, Vector2 normal)
        {
            var contacts = new List<Vector2>();
            var minDistance = float.MaxValue;
            Vector2 closestPoint = Vector2.Zero;

            foreach (var vertex in verticesA)
            {
                var projected = vertex - vertex.Dot(normal) * normal;
                var dist = (vertex - projected).Magnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPoint = vertex;
                }
            }

            foreach (var vertex in verticesB)
            {
                var projected = vertex - vertex.Dot(normal) * normal;
                var dist = (vertex - projected).Magnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPoint = vertex;
                }
            }

            contacts.Add(closestPoint);
            return contacts.ToArray();
        }

        private void ProcessCollisionCallbacks()
        {
            foreach (var pair in collisionPairs)
            {
                if (!pair.IsValid())
                    continue;

                var collision = pair.Collision;

                if (collision != null && collision.IsColliding)
                {
                    if (!pair.WasColliding)
                    {
                        pair.Collider1.OnCollisionEnter(collision);
                        pair.Collider2.OnCollisionEnter(collision);

                        if (pair.Collider1.IsTrigger || pair.Collider2.IsTrigger)
                        {
                            TriggerEnter?.Invoke(this, new CollisionEventArgs(collision));
                        }
                        else
                        {
                            CollisionEnter?.Invoke(this, new CollisionEventArgs(collision));
                        }
                    }
                    else
                    {
                        pair.Collider1.OnCollisionStay(collision);
                        pair.Collider2.OnCollisionStay(collision);

                        if (pair.Collider1.IsTrigger || pair.Collider2.IsTrigger)
                        {
                            TriggerStay?.Invoke(this, new CollisionEventArgs(collision));
                        }
                        else
                        {
                            CollisionStay?.Invoke(this, new CollisionEventArgs(collision));
                        }
                    }
                }
            }
        }

        public bool Raycast(Vector2 origin, Vector2 direction, float distance, out RaycastHit hit, LayerMask layerMask = null)
        {
            hit = null;
            var maxDistance = distance;
            var normalizedDir = direction.Normalized;

            foreach (var collider in allColliders)
            {
                if (!collider.IsActive || !collider.IsEnabled)
                    continue;

                if (layerMask != null && !layerMask.Contains((int)collider.Layer))
                    continue;

                var rayHit = RaycastAgainstCollider(origin, normalizedDir, maxDistance, collider);
                if (rayHit != null)
                {
                    if (hit == null || rayHit.Distance < hit.Distance)
                    {
                        hit = rayHit;
                    }
                }
            }

            return hit != null;
        }

        private RaycastHit RaycastAgainstCollider(Vector2 origin, Vector2 direction, float maxDistance, Collider2D collider)
        {
            if (collider is BoxCollider box)
            {
                return RaycastBox(origin, direction, maxDistance, box);
            }
            else if (collider is CircleCollider circle)
            {
                return RaycastCircle(origin, direction, maxDistance, circle);
            }

            return RaycastAABB(origin, direction, maxDistance, collider.Bounds);
        }

        private RaycastHit RaycastBox(Vector2 origin, Vector2 direction, float maxDistance, BoxCollider box)
        {
            var vertices = GetWorldVertices(box);
            var min = float.MaxValue;
            var max = float.MinValue;
            var axis = direction.Normalized;

            foreach (var vertex in vertices)
            {
                var projection = vertex.Dot(axis);
                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            var originProj = origin.Dot(axis);
            if (originProj < min || originProj > max)
                return null;

            var tMin = (min - originProj);
            var tMax = (max - originProj);

            if (tMin > tMax)
            {
                var temp = tMin;
                tMin = tMax;
                tMax = temp;
            }

            if (tMin > maxDistance || tMax < 0)
                return null;

            var t = tMin > 0 ? tMin : tMax;
            if (t < 0 || t > maxDistance)
                return null;

            var hitPoint = origin + direction * t;
            var closestVertex = vertices.OrderBy(v => (v - hitPoint).SqrMagnitude).First();

            return new RaycastHit
            {
                Point = hitPoint,
                Normal = (closestVertex - hitPoint).Normalized,
                Distance = t,
                Collider = box
            };
        }

        private RaycastHit RaycastCircle(Vector2 origin, Vector2 direction, float maxDistance, CircleCollider circle)
        {
            var center = circle.Transform.WorldPosition;
            var radius = circle.Radius;
            var originToCenter = center - origin;
            var projection = originToCenter.Dot(direction);

            if (projection < 0)
                return null;

            var perpendicularDistSq = originToCenter.SqrMagnitude - projection * projection;
            var radiusSq = radius * radius;

            if (perpendicularDistSq > radiusSq)
                return null;

            var offset = (float)Math.Sqrt(radiusSq - perpendicularDistSq);
            var t = projection - offset;

            if (t < 0 || t > maxDistance)
                return null;

            var hitPoint = origin + direction * t;
            var normal = (hitPoint - center).Normalized;

            return new RaycastHit
            {
                Point = hitPoint,
                Normal = normal,
                Distance = t,
                Collider = circle
            };
        }

        private RaycastHit RaycastAABB(Vector2 origin, Vector2 direction, float maxDistance, Bounds bounds)
        {
            var invDirX = direction.X != 0 ? 1f / direction.X : float.MaxValue;
            var invDirY = direction.Y != 0 ? 1f / direction.Y : float.MaxValue;

            var t1 = (bounds.Min.X - origin.X) * invDirX;
            var t2 = (bounds.Max.X - origin.X) * invDirX;
            var t3 = (bounds.Min.Y - origin.Y) * invDirY;
            var t4 = (bounds.Max.Y - origin.Y) * invDirY;

            var tMin = Math.Max(Math.Min(t1, t2), Math.Min(t3, t4));
            var tMax = Math.Min(Math.Max(t1, t2), Math.Max(t3, t4));

            if (tMax < 0 || tMin > tMax || tMin > maxDistance)
                return null;

            var t = tMin > 0 ? tMin : tMax;
            if (t < 0 || t > maxDistance)
                return null;

            var hitPoint = origin + direction * t;
            var normal = Vector2.Zero;

            if (Math.Abs(t - t1) < 0.001f)
                normal = Vector2.Left;
            else if (Math.Abs(t - t2) < 0.001f)
                normal = Vector2.Right;
            else if (Math.Abs(t - t3) < 0.001f)
                normal = Vector2.Down;
            else if (Math.Abs(t - t4) < 0.001f)
                normal = Vector2.Up;

            return new RaycastHit
            {
                Point = hitPoint,
                Normal = normal,
                Distance = t,
                Collider = null
            };
        }

        public RaycastHit[] RaycastAll(Vector2 origin, Vector2 direction, float distance, LayerMask layerMask = null)
        {
            var hits = new List<RaycastHit>();
            RaycastHit hit;

            if (Raycast(origin, direction, distance, out hit, layerMask))
            {
                hits.Add(hit);
            }

            return hits.ToArray();
        }

        public void SetLayerCollision(int layerA, int layerB, bool collides)
        {
            var key = $"{layerA}_{layerB}";
            if (!layerConfigs.ContainsKey(key))
            {
                layerConfigs[key] = new LayerCollisionConfig
                {
                    LayerIndex = layerA,
                    CollidesWith = 0
                };
            }

            var config = layerConfigs[key];
            if (collides)
                config.CollidesWith |= (1 << layerB);
            else
                config.CollidesWith &= ~(1 << layerB);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                allColliders.Clear();
                collisionPairs.Clear();
                newCollisionPairs.Clear();
                activeCollisions.Clear();
                triggerCollisions.Clear();
                overlappingColliders.Clear();
                spatialGrid.Clear();
            }
        }
    }

    public enum CollisionDetectionMode
    {
        Discrete,
        Continuous
    }

    public enum CollisionResponseMode
    {
        Callbacks,
        Physics
    }

    public class CollisionPair
    {
        public Collider2D Collider1 { get; }
        public Collider2D Collider2 { get; }
        public CollisionInfo Collision { get; private set; }
        public bool WasColliding { get; private set; }

        public CollisionPair(Collider2D a, Collider2D b)
        {
            Collider1 = a;
            Collider2 = b;
        }

        public void SetCollision(CollisionInfo collision)
        {
            Collision = collision;
            WasColliding = collision != null && collision.IsColliding;
        }

        public bool IsValid()
        {
            return Collider1 != null && Collider2 != null && 
                   Collider1.IsActive && Collider2.IsActive;
        }
    }

    public class CollisionInfo
    {
        public bool IsColliding { get; set; }
        public Collider2D ColliderA { get; set; }
        public Collider2D ColliderB { get; set; }
        public Vector2 Normal { get; set; }
        public float Penetration { get; set; }
        public Vector2[] ContactPoints { get; set; }
        public float RelativeVelocity { get; set; }
    }

    public class RaycastHit
    {
        public Vector2 Point { get; set; }
        public Vector2 Normal { get; set; }
        public float Distance { get; set; }
        public Collider2D Collider { get; set; }
    }

    public class CollisionEventArgs : EventArgs
    {
        public CollisionInfo Collision { get; }

        public CollisionEventArgs(CollisionInfo collision)
        {
            Collision = collision;
        }
    }

    public class LayerCollisionConfig
    {
        public string LayerName { get; set; }
        public int LayerIndex { get; set; }
        public int CollidesWith { get; set; }
        public bool CollidesWithSelf { get; set; }

        public bool CollidesWith(int otherLayer)
        {
            return (CollidesWith & (1 << otherLayer)) != 0;
        }
    }

    public abstract class Collider2D : Component
    {
        protected CollisionShapeType shapeType;
        protected bool isEnabled;
        protected bool isActive;
        protected bool isTrigger;
        protected int layer;
        protected LayerCollisionConfig layerSettings;
        protected Bounds bounds;
        protected float density;
        protected float friction;
        protected float restitution;
        protected Vector2 offset;
        protected bool useAutoMass;
        protected bool isSensor;

        public CollisionShapeType ShapeType => shapeType;
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        public bool IsActive => isActive;
        public bool IsTrigger { get => isTrigger; set => isTrigger = value; }
        public int Layer { get => layer; set => layer = value; }
        public LayerCollisionConfig LayerSettings => layerSettings;
        public Bounds Bounds => bounds;
        public float Density { get => density; set => density = Math.Max(0f, value); }
        public float Friction { get => friction; set => friction = Math.Max(0f, Math.Min(1f, value)); }
        public float Restitution { get => restitution; set => restitution = Math.Max(0f, Math.Min(1f, value)); }
        public Vector2 Offset { get => offset; set => offset = value; }
        public bool UseAutoMass { get => useAutoMass; set => useAutoMass = value; }
        public bool IsSensor { get => isSensor; set => isSensor = value; }

        public Collider2D()
        {
            shapeType = CollisionShapeType.None;
            isEnabled = true;
            isActive = true;
            isTrigger = false;
            layer = 0;
            layerSettings = new LayerCollisionConfig { LayerIndex = 0 };
            density = 1f;
            friction = 0.4f;
            restitution = 0f;
            offset = Vector2.Zero;
            useAutoMass = false;
            isSensor = false;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            CollisionSystem.Instance.RegisterCollider(this);
            UpdateBounds();
        }

        public virtual void UpdateBounds()
        {
        }

        protected void UpdateAABB()
        {
            if (Transform != null)
            {
                var worldPos = Transform.WorldPosition;
                bounds = new Bounds(worldPos + offset, new Vector2(100, 100));
            }
        }

        public virtual void OnCollisionEnter(CollisionInfo collision) { }
        public virtual void OnCollisionStay(CollisionInfo collision) { }
        public virtual void OnCollisionExit(CollisionInfo collision) { }

        public override void OnDestroy()
        {
            base.OnDestroy();
            CollisionSystem.Instance.UnregisterCollider(this);
        }
    }

    public class CircleCollider : Collider2D
    {
        private float radius;

        public float Radius
        {
            get => radius;
            set
            {
                radius = Math.Max(0f, value);
                UpdateBounds();
            }
        }

        public CircleCollider()
        {
            shapeType = CollisionShapeType.Circle;
            radius = 0.5f;
        }

        public override void UpdateBounds()
        {
            if (Transform != null)
            {
                var worldPos = Transform.WorldPosition + offset;
                bounds = new Bounds(worldPos, new Vector2(radius * 2, radius * 2));
            }
        }
    }

    public class BoxCollider : Collider2D
    {
        private Vector2 size;

        public Vector2 Size
        {
            get => size;
            set
            {
                size = new Vector2(Math.Max(0f, value.X), Math.Max(0f, value.Y));
                UpdateBounds();
            }
        }

        public BoxCollider()
        {
            shapeType = CollisionShapeType.Box;
            size = new Vector2(1f, 1f);
        }

        public override void UpdateBounds()
        {
            if (Transform != null)
            {
                var worldPos = Transform.WorldPosition + offset;
                var worldScale = Transform.WorldScale;
                bounds = new Bounds(worldPos, new Vector2(size.X * worldScale.X, size.Y * worldScale.Y));
            }
        }
    }

    public class PolygonCollider : Collider2D
    {
        private Vector2[] vertices;

        public Vector2[] Vertices
        {
            get => vertices;
            set
            {
                vertices = value;
                UpdateBounds();
            }
        }

        public PolygonCollider()
        {
            shapeType = CollisionShapeType.Polygon;
            vertices = new Vector2[4];
        }

        public void SetVertices(Vector2[] vertices)
        {
            Vertices = vertices;
        }

        public override void UpdateBounds()
        {
            if (vertices == null || vertices.Length == 0 || Transform == null)
                return;

            var worldPos = Transform.WorldPosition;
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            foreach (var vertex in vertices)
            {
                var worldVertex = Transform.TransformPoint(vertex);
                minX = Math.Min(minX, worldVertex.X);
                minY = Math.Min(minY, worldVertex.Y);
                maxX = Math.Max(maxX, worldVertex.X);
                maxY = Math.Max(maxY, worldVertex.Y);
            }

            bounds = new Bounds(
                new Vector2((minX + maxX) / 2, (minY + maxY) / 2),
                new Vector2(maxX - minX, maxY - minY)
            );
        }
    }
}

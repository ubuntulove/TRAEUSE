using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Physics
{
    public class PhysicsWorld
    {
        private static PhysicsWorld? _instance;
        public static PhysicsWorld Instance => _instance ??= new PhysicsWorld();

        private readonly List<RigidBody> _bodies;
        private readonly List<Collider> _colliders;
        private readonly BroadPhase _broadPhase;
        private readonly NarrowPhase _narrowPhase;
        private readonly CollisionSolver _solver;
        private readonly List<Manifold> _contacts;
        private readonly Vector2 _gravity;
        private int _velocityIterations;
        private int _positionIterations;
        private float _timeScale;
        private bool _isPaused;
        private float _fixedDeltaTime;

        public Vector2 Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        public int VelocityIterations
        {
            get => _velocityIterations;
            set => _velocityIterations = Math.Max(1, value);
        }

        public int PositionIterations
        {
            get => _positionIterations;
            set => _positionIterations = Math.Max(1, value);
        }

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Math.Clamp(value, 0f, 10f);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => _isPaused = value;
        }

        public float FixedDeltaTime
        {
            get => _fixedDeltaTime;
            set => _fixedDeltaTime = Math.Max(0.0001f, value);
        }

        public IReadOnlyList<RigidBody> Bodies => _bodies.AsReadOnly();
        public IReadOnlyList<Collider> Colliders => _colliders.AsReadOnly();
        public IReadOnlyList<Manifold> Contacts => _contacts.AsReadOnly();

        public event EventHandler<Manifold>? CollisionDetected;
        public event EventHandler<Manifold>? CollisionResolved;
        public event EventHandler<RigidBody>? BodyAdded;
        public event EventHandler<RigidBody>? BodyRemoved;

        private PhysicsWorld()
        {
            _bodies = new List<RigidBody>();
            _colliders = new List<Collider>();
            _broadPhase = new SpatialHashBroadPhase();
            _narrowPhase = new NarrowPhase();
            _solver = new CollisionSolver();
            _contacts = new List<Manifold>();
            _gravity = new Vector2(0f, -9.81f);
            _velocityIterations = 8;
            _positionIterations = 3;
            _timeScale = 1f;
            _isPaused = false;
            _fixedDeltaTime = 1f / 60f;
        }

        public RigidBody CreateBody(RigidBodyDefinition definition)
        {
            var body = new RigidBody(definition);
            _bodies.Add(body);

            if (definition.Collider != null)
            {
                var collider = definition.Collider;
                collider.Body = body;
                _colliders.Add(collider);
            }

            BodyAdded?.Invoke(this, body);
            return body;
        }

        public void RemoveBody(RigidBody body)
        {
            if (_bodies.Remove(body))
            {
                if (body.Collider != null)
                {
                    _colliders.Remove(body.Collider);
                }

                body.OnRemove();
                BodyRemoved?.Invoke(this, body);
            }
        }

        public void AddBody(RigidBody body)
        {
            if (!_bodies.Contains(body))
            {
                _bodies.Add(body);
                BodyAdded?.Invoke(this, body);
            }
        }

        public void Step(float deltaTime)
        {
            if (_isPaused)
                return;

            float stepDelta = deltaTime * _timeScale;
            float accumulatedTime = 0f;

            while (accumulatedTime < stepDelta)
            {
                float frameTime = Math.Min(_fixedDeltaTime, stepDelta - accumulatedTime);
                FixedStep(frameTime);
                accumulatedTime += frameTime;
            }
        }

        private void FixedStep(float deltaTime)
        {
            foreach (var body in _bodies)
            {
                if (body.BodyType == BodyType.Dynamic && body.IsAwake)
                {
                    body.ApplyGravity(_gravity * deltaTime);

                    if (body.UseAngularDamping)
                    {
                        body.AngularVelocity *= MathHelper.Pow(body.AngularDamping, deltaTime);
                    }

                    if (body.UseLinearDamping)
                    {
                        body.Velocity *= MathHelper.Pow(body.LinearDamping, deltaTime);
                    }
                }
            }

            foreach (var body in _bodies)
            {
                body.IntegrateForces(deltaTime);
            }

            _contacts.Clear();

            _broadPhase.Update(_colliders);

            var pairs = _broadPhase.GetPotentialPairs();

            foreach (var pair in pairs)
            {
                if (!_narrowPhase.IsValidPair(pair.A, pair.B))
                    continue;

                if (_narrowPhase.TestCollision(pair.A, pair.B, out var manifold))
                {
                    _contacts.Add(manifold);
                    CollisionDetected?.Invoke(this, manifold);
                }
            }

            _solver.Initialize(_contacts);

            for (int v = 0; v < _velocityIterations; v++)
            {
                _solver.SolveVelocities();
            }

            foreach (var body in _bodies)
            {
                body.IntegrateVelocities(deltaTime);
            }

            for (int p = 0; p < _positionIterations; p++)
            {
                _solver.SolvePositions();

                foreach (var body in _bodies)
                {
                    body.CorrectPositions(deltaTime);
                }

                bool allSolved = true;
                foreach (var contact in _contacts)
                {
                    if (!contact.IsResolved)
                    {
                        allSolved = false;
                        break;
                    }
                }

                if (allSolved)
                    break;
            }

            foreach (var contact in _contacts)
            {
                CollisionResolved?.Invoke(this, contact);
            }

            foreach (var body in _bodies)
            {
                body.SleepUpdate(deltaTime);
            }
        }

        public List<RaycastHit> Raycast(Vector2 origin, Vector2 direction, float maxDistance, int layerMask = -1)
        {
            var hits = new List<RaycastHit>();
            direction = direction.Normalized();

            var potentialColliders = _broadPhase.QueryRay(origin, direction, maxDistance, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if (RaycastCollider(collider, origin, direction, maxDistance, out var hit))
                {
                    hits.Add(hit);
                }
            }

            hits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            return hits;
        }

        public RaycastHit? RaycastSingle(Vector2 origin, Vector2 direction, float maxDistance, int layerMask = -1)
        {
            var hits = Raycast(origin, direction, maxDistance, layerMask);
            return hits.Count > 0 ? hits[0] : null;
        }

        private bool RaycastCollider(Collider collider, Vector2 origin, Vector2 direction, float maxDistance, out RaycastHit hit)
        {
            hit = default;

            if (collider.Shape == null)
                return false;

            Transform2D transform = collider.Body != null
                ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                : Transform2D.Identity;

            if (collider.Shape is CircleShape circle)
            {
                return RaycastCircle(transform.TransformPoint(circle.Center), circle.Radius, origin, direction, maxDistance, collider, out hit);
            }
            else if (collider.Shape is BoxShape box)
            {
                return RaycastBox(transform, box, origin, direction, maxDistance, collider, out hit);
            }
            else if (collider.Shape is PolygonShape polygon)
            {
                return RaycastPolygon(polygon.GetVertices(transform), origin, direction, maxDistance, collider, out hit);
            }

            return false;
        }

        private bool RaycastCircle(Vector2 center, float radius, Vector2 origin, Vector2 direction, float maxDistance, Collider collider, out RaycastHit hit)
        {
            hit = default;

            Vector2 toCenter = center - origin;
            float proj = toCenter.Dot(direction);
            float closest = origin + direction * proj;

            float distToCenter = (center - closest).Length();
            if (distToCenter > radius)
                return false;

            float offset = (float)Math.Sqrt(radius * radius - distToCenter * distToCenter);
            float t1 = proj - offset;
            float t2 = proj + offset;

            float t = t1 > 0 ? t1 : t2;
            if (t < 0 || t > maxDistance)
                return false;

            hit = new RaycastHit
            {
                Point = origin + direction * t,
                Normal = ((origin + direction * t) - center).Normalized(),
                Distance = t,
                Collider = collider,
                Fraction = t / maxDistance
            };

            return true;
        }

        private bool RaycastBox(Transform2D transform, BoxShape box, Vector2 origin, Vector2 direction, float maxDistance, Collider collider, out RaycastHit hit)
        {
            hit = default;

            Vector2[] vertices = box.GetVertices(transform);

            if (!AABB.Raycast(AABB.FromVertices(vertices), origin, direction, out float tMin, out Vector2 normal))
                return false;

            if (tMin < 0 || tMin > maxDistance)
                return false;

            hit = new RaycastHit
            {
                Point = origin + direction * tMin,
                Normal = normal,
                Distance = tMin,
                Collider = collider,
                Fraction = tMin / maxDistance
            };

            return true;
        }

        private bool RaycastPolygon(Vector2[] vertices, Vector2 origin, Vector2 direction, float maxDistance, Collider collider, out RaycastHit hit)
        {
            hit = default;

            if (!AABB.Raycast(AABB.FromVertices(vertices), origin, direction, out float tMin, out Vector2 normal))
                return false;

            if (tMin < 0 || tMin > maxDistance)
                return false;

            hit = new RaycastHit
            {
                Point = origin + direction * tMin,
                Normal = normal,
                Distance = tMin,
                Collider = collider,
                Fraction = tMin / maxDistance
            };

            return true;
        }

        public List<Collider> OverlapCircle(Vector2 center, float radius, int layerMask = -1)
        {
            var results = new List<Collider>();
            var aabb = AABB.CreateFromCircle(center, radius);

            var potentialColliders = _broadPhase.QueryAABB(aabb, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if (collider.Shape is CircleShape circle)
                {
                    Transform2D transform = collider.Body != null
                        ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                        : Transform2D.Identity;

                    Vector2 worldCenter = transform.TransformPoint(circle.Center);
                    float dist = (worldCenter - center).Length();
                    if (dist <= radius + circle.Radius)
                    {
                        results.Add(collider);
                    }
                }
                else
                {
                    results.Add(collider);
                }
            }

            return results;
        }

        public List<Collider> OverlapBox(Vector2 center, Vector2 halfExtents, float rotation, int layerMask = -1)
        {
            var results = new List<Collider>();
            var box = new BoxShape(halfExtents);
            var aabb = box.GetAABB(new Transform2D(center, rotation));

            var potentialColliders = _broadPhase.QueryAABB(aabb, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if (collider.Shape is BoxShape otherBox)
                {
                    Transform2D transform1 = new Transform2D(center, rotation);
                    Transform2D transform2 = collider.Body != null
                        ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                        : Transform2D.Identity;

                    if (IntersectPolygons(box.GetVertices(transform1), otherBox.GetVertices(transform2)))
                    {
                        results.Add(collider);
                    }
                }
                else
                {
                    results.Add(collider);
                }
            }

            return results;
        }

        private bool IntersectPolygons(Vector2[] poly1, Vector2[] poly2)
        {
            Vector2[] axes = GetPolygonAxes(poly1);
            Vector2[] axes2 = GetPolygonAxes(poly2);
            axes = axes.Concat(axes2).ToArray();

            foreach (var axis in axes)
            {
                float proj1 = GetProjection(poly1, axis);
                float proj2 = GetProjection(poly2, axis);

                float overlap1 = proj1.Max - proj2.Min;
                float overlap2 = proj2.Max - proj1.Min;

                if (overlap1 < 0 || overlap2 < 0)
                    return false;
            }

            return true;
        }

        private Vector2[] GetPolygonAxes(Vector2[] vertices)
        {
            var axes = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 edge = vertices[(i + 1) % vertices.Length] - vertices[i];
                axes[i] = new Vector2(-edge.Y, edge.X).Normalized();
            }
            return axes;
        }

        private (float Min, float Max) GetProjection(Vector2[] vertices, Vector2 axis)
        {
            float min = vertices[0].Dot(axis);
            float max = min;

            for (int i = 1; i < vertices.Length; i++)
            {
                float proj = vertices[i].Dot(axis);
                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }

            return (min, max);
        }

        public void Clear()
        {
            _bodies.Clear();
            _colliders.Clear();
            _contacts.Clear();
        }

        public void SetLayerCollision(int layer1, int layer2, bool enabled)
        {
            _broadPhase.SetLayerCollision(layer1, layer2, enabled);
        }
    }

    public class RigidBodyDefinition
    {
        public BodyType BodyType { get; set; } = BodyType.Dynamic;
        public float Mass { get; set; } = 1f;
        public float InverseMass { get; set; } = 1f;
        public float MomentOfInertia { get; set; } = 1f;
        public float InverseInertia { get; set; } = 1f;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Rotation { get; set; } = 0f;
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public float AngularVelocity { get; set; } = 0f;
        public Vector2 Force { get; set; } = Vector2.Zero;
        public float Torque { get; set; } = 0f;
        public float Restitution { get; set; } = 0.3f;
        public float Friction { get; set; } = 0.4f;
        public bool IsAwake { get; set; } = true;
        public bool AllowSleep { get; set; } = true;
        public float SleepTime { get; set; } = 0f;
        public float SleepVelocityThreshold { get; set; } = 0.01f;
        public float SleepAngularVelocityThreshold { get; set; } = 0.01f;
        public bool UseGravity { get; set; } = true;
        public bool UseAngularDamping { get; set; } = true;
        public bool UseLinearDamping { get; set; } = true;
        public float AngularDamping { get; set; } = 0.01f;
        public float LinearDamping { get; set; } = 0.01f;
        public Collider? Collider { get; set; }
        public int Layer { get; set; } = 0;
        public int CollisionLayer { get; set; } = -1;
        public int CollisionMask { get; set; } = -1;
        public bool IsTrigger { get; set; } = false;

        public void ApplyMassProperties()
        {
            if (Mass > 0)
            {
                InverseMass = 1f / Mass;
            }
            else
            {
                InverseMass = 0f;
            }

            if (MomentOfInertia > 0)
            {
                InverseInertia = 1f / MomentOfInertia;
            }
            else
            {
                InverseInertia = 0f;
            }
        }
    }

    public class RigidBody
    {
        private RigidBodyDefinition _definition;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _velocity;
        private float _angularVelocity;
        private Vector2 _force;
        private float _torque;
        private Vector2 _previousPosition;
        private float _previousRotation;
        private bool _isAwake;
        private float _sleepTime;

        public RigidBodyDefinition Definition => _definition;
        public BodyType BodyType => _definition.BodyType;
        public float Mass => _definition.Mass;
        public float InverseMass => _definition.InverseMass;
        public float MomentOfInertia => _definition.MomentOfInertia;
        public float InverseInertia => _definition.InverseInertia;
        public Vector2 Position
        {
            get => _position;
            set
            {
                _previousPosition = _position;
                _position = value;
                WakeUp();
            }
        }
        public float Rotation
        {
            get => _rotation;
            set
            {
                _previousRotation = _rotation;
                _rotation = value;
                WakeUp();
            }
        }
        public Vector2 Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
                WakeUp();
            }
        }
        public float AngularVelocity
        {
            get => _angularVelocity;
            set
            {
                _angularVelocity = value;
                WakeUp();
            }
        }
        public Vector2 Force => _force;
        public float Torque => _torque;
        public float Restitution { get => _definition.Restitution; set => _definition.Restitution = value; }
        public float Friction { get => _definition.Friction; set => _definition.Friction = value; }
        public bool IsAwake => _isAwake;
        public bool AllowSleep { get => _definition.AllowSleep; set => _definition.AllowSleep = value; }
        public float SleepVelocityThreshold => _definition.SleepVelocityThreshold;
        public float SleepAngularVelocityThreshold => _definition.SleepAngularVelocityThreshold;
        public bool UseGravity { get => _definition.UseGravity; set => _definition.UseGravity = value; }
        public bool UseAngularDamping { get => _definition.UseAngularDamping; set => _definition.UseAngularDamping = value; }
        public bool UseLinearDamping { get => _definition.UseLinearDamping; set => _definition.UseLinearDamping = value; }
        public float AngularDamping => _definition.AngularDamping;
        public float LinearDamping => _definition.LinearDamping;
        public Collider? Collider => _definition.Collider;
        public int Layer { get => _definition.Layer; set => _definition.Layer = value; }
        public int CollisionLayer { get => _definition.CollisionLayer; set => _definition.CollisionLayer = value; }
        public int CollisionMask { get => _definition.CollisionMask; set => _definition.CollisionMask = value; }
        public bool IsTrigger { get => _definition.IsTrigger; set => _definition.IsTrigger = value; }

        public event EventHandler? WakeChanged;
        public event EventHandler? Destroyed;

        public RigidBody(RigidBodyDefinition definition)
        {
            _definition = definition;
            _position = definition.Position;
            _rotation = definition.Rotation;
            _velocity = definition.Velocity;
            _angularVelocity = definition.AngularVelocity;
            _force = Vector2.Zero;
            _torque = 0f;
            _previousPosition = _position;
            _previousRotation = _rotation;
            _isAwake = definition.IsAwake;
            _sleepTime = 0f;
        }

        public void ApplyForce(Vector2 force)
        {
            _force += force;
            WakeUp();
        }

        public void ApplyForceAtPoint(Vector2 force, Vector2 point)
        {
            _force += force;
            Vector2 r = point - _position;
            _torque += r.Cross(force);
            WakeUp();
        }

        public void ApplyTorque(float torque)
        {
            _torque += torque;
            WakeUp();
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            _velocity += impulse * InverseMass;
            WakeUp();
        }

        public void ApplyImpulseAtPoint(Vector2 impulse, Vector2 point)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            _velocity += impulse * InverseMass;
            _angularVelocity += (point - _position).Cross(impulse) * InverseInertia;
            WakeUp();
        }

        public void ApplyAngularImpulse(float impulse)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            _angularVelocity += impulse * InverseInertia;
            WakeUp();
        }

        public void ApplyGravity(Vector2 gravity)
        {
            if (BodyType != BodyType.Dynamic || !UseGravity)
                return;

            _velocity += gravity;
        }

        public void IntegrateForces(float deltaTime)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            Vector2 acceleration = _force * InverseMass;
            _velocity += acceleration * deltaTime;

            float angularAcceleration = _torque * InverseInertia;
            _angularVelocity += angularAcceleration * deltaTime;
        }

        public void IntegrateVelocities(float deltaTime)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            _position += _velocity * deltaTime;
            _rotation += _angularVelocity * deltaTime;

            _force = Vector2.Zero;
            _torque = 0f;
        }

        public void CorrectPositions(float deltaTime)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            Vector2 correction = _position - _previousPosition;
            float angularCorrection = _rotation - _previousRotation;

            if (correction.LengthSquared() > 0.0001f || Math.Abs(angularCorrection) > 0.0001f)
            {
                _position = _previousPosition + correction * 0.2f;
                _rotation = _previousRotation + angularCorrection * 0.2f;
            }
        }

        public void WakeUp()
        {
            if (!_isAwake)
            {
                _isAwake = true;
                _sleepTime = 0f;
                WakeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Sleep()
        {
            if (_isAwake)
            {
                _isAwake = false;
                _velocity = Vector2.Zero;
                _angularVelocity = 0f;
                WakeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SleepUpdate(float deltaTime)
        {
            if (BodyType != BodyType.Dynamic || !AllowSleep)
                return;

            if (_isAwake)
            {
                float linearVelocitySq = _velocity.LengthSquared();
                float angularVelocitySq = _angularVelocity * _angularVelocity;

                if (linearVelocitySq < SleepVelocityThreshold * SleepVelocityThreshold &&
                    angularVelocitySq < SleepAngularVelocityThreshold * SleepAngularVelocityThreshold)
                {
                    _sleepTime += deltaTime;

                    if (_sleepTime > 0.5f)
                    {
                        Sleep();
                    }
                }
                else
                {
                    _sleepTime = 0f;
                }
            }
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        public void SetRotation(float rotation)
        {
            Rotation = rotation;
        }

        public void SetTransform(Vector2 position, float rotation)
        {
            _previousPosition = _position;
            _previousRotation = _rotation;
            _position = position;
            _rotation = rotation;
            WakeUp();
        }

        public Vector2 GetVelocityAtPoint(Vector2 point)
        {
            Vector2 r = point - _position;
            return _velocity + new Vector2(-r.Y, r.X) * _angularVelocity;
        }

        public float GetKineticEnergy()
        {
            float linear = 0.5f * Mass * _velocity.LengthSquared();
            float angular = 0.5f * MomentOfInertia * _angularVelocity * _angularVelocity;
            return linear + angular;
        }

        public float GetPotentialEnergy(Vector2 gravity)
        {
            return Mass * gravity.Dot(_position);
        }

        public void OnRemove()
        {
            Destroyed?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Collider
    {
        public RigidBody? Body { get; set; }
        public IShape? Shape { get; set; }
        public int Layer { get; set; } = 0;
        public int CollisionMask { get; set; } = -1;
        public bool IsTrigger { get; set; } = false;
        public float Friction { get; set; } = 0.4f;
        public float Restitution { get; set; } = 0.3f;

        public Collider()
        {
        }

        public Collider(IShape shape)
        {
            Shape = shape;
        }
    }

    public struct Manifold
    {
        public Collider A;
        public Collider B;
        public Vector2 Normal;
        public float Penetration;
        public List<ContactPoint> Contacts;
        public bool IsResolved;

        public Manifold(Collider a, Collider b)
        {
            A = a;
            B = b;
            Normal = Vector2.Zero;
            Penetration = 0f;
            Contacts = new List<ContactPoint>();
            IsResolved = false;
        }
    }

    public struct ContactPoint
    {
        public Vector2 Position;
        public float NormalImpulse;
        public float TangentImpulse;
        public float NormalMass;
        public float TangentMass;
        public float Bias;
    }

    public struct RaycastHit
    {
        public Vector2 Point;
        public Vector2 Normal;
        public float Distance;
        public Collider Collider;
        public float Fraction;
    }

    public abstract class BroadPhase
    {
        public abstract void Update(IEnumerable<Collider> colliders);
        public abstract IEnumerable<(Collider A, Collider B)> GetPotentialPairs();
        public abstract IEnumerable<Collider> QueryAABB(AABB aabb, int layerMask);
        public abstract IEnumerable<Collider> QueryRay(Vector2 origin, Vector2 direction, float maxDistance, int layerMask);
        public abstract void SetLayerCollision(int layer1, int layer2, bool enabled);
    }

    public class SpatialHashBroadPhase : BroadPhase
    {
        private readonly Dictionary<int, List<Collider>> _cells;
        private readonly Dictionary<(Collider, Collider), bool> _pairCache;
        private float _cellSize;
        private int _maxCellsPerAxis;
        private readonly Dictionary<int, bool> _layerCollisions;

        public SpatialHashBroadPhase(float cellSize = 100f)
        {
            _cells = new Dictionary<int, List<Collider>>();
            _pairCache = new Dictionary<(Collider, Collider), bool>();
            _cellSize = cellSize;
            _maxCellsPerAxis = 1000;
            _layerCollisions = new Dictionary<int, bool>();
        }

        public override void Update(IEnumerable<Collider> colliders)
        {
            _cells.Clear();

            foreach (var collider in colliders)
            {
                if (collider.Shape == null)
                    continue;

                Transform2D transform = collider.Body != null
                    ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                    : Transform2D.Identity;

                AABB aabb = collider.Shape.GetAABB(transform);
                var minCell = GetCell(aabb.Min);
                var maxCell = GetCell(aabb.Max);

                for (int x = minCell.X; x <= maxCell.X; x++)
                {
                    for (int y = minCell.Y; y <= maxCell.Y; y++)
                    {
                        int hash = HashCell(x, y);
                        if (!_cells.TryGetValue(hash, out var list))
                        {
                            list = new List<Collider>();
                            _cells[hash] = list;
                        }
                        list.Add(collider);
                    }
                }
            }
        }

        public override IEnumerable<(Collider A, Collider B)> GetPotentialPairs()
        {
            var pairs = new HashSet<(Collider, Collider)>();

            foreach (var cell in _cells.Values)
            {
                for (int i = 0; i < cell.Count; i++)
                {
                    for (int j = i + 1; j < cell.Count; j++)
                    {
                        var a = cell[i];
                        var b = cell[j];

                        if (a == b)
                            continue;

                        if (!IsValidPair(a, b))
                            continue;

                        var pair = a.GetHashCode() < b.GetHashCode()
                            ? (a, b)
                            : (b, a);

                        pairs.Add(pair);
                    }
                }
            }

            return pairs;
        }

        private bool IsValidPair(Collider a, Collider b)
        {
            if (a.Body != null && b.Body != null && a.Body == b.Body)
                return false;

            if ((a.CollisionMask & b.Layer) == 0)
                return false;

            int layerKey = (a.Layer << 16) | b.Layer;
            if (_layerCollisions.TryGetValue(layerKey, out bool enabled) && !enabled)
                return false;

            return true;
        }

        public override IEnumerable<Collider> QueryAABB(AABB aabb, int layerMask)
        {
            var results = new HashSet<Collider>();
            var minCell = GetCell(aabb.Min);
            var maxCell = GetCell(aabb.Max);

            for (int x = minCell.X; x <= maxCell.X; x++)
            {
                for (int y = minCell.Y; y <= maxCell.Y; y++)
                {
                    int hash = HashCell(x, y);
                    if (_cells.TryGetValue(hash, out var cell))
                    {
                        foreach (var collider in cell)
                        {
                            if ((collider.Layer & layerMask) == 0)
                                continue;

                            Transform2D transform = collider.Body != null
                                ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                                : Transform2D.Identity;

                            if (collider.Shape != null && collider.Shape.GetAABB(transform).Intersects(aabb))
                            {
                                results.Add(collider);
                            }
                        }
                    }
                }
            }

            return results;
        }

        public override IEnumerable<Collider> QueryRay(Vector2 origin, Vector2 direction, float maxDistance, int layerMask)
        {
            var results = new HashSet<Collider>();
            Vector2 endPoint = origin + direction * maxDistance;
            AABB rayBounds = new AABB(
                Vector2.Min(origin, endPoint),
                Vector2.Max(origin, endPoint)
            );

            var cell = GetCell(origin);
            Vector2 step = direction.X > 0 ? Vector2.Right : Vector2.Left;
            Vector2 tDelta = new Vector2(1f / Math.Abs(direction.X), 1f / Math.Abs(direction.Y));

            int maxIterations = _maxCellsPerAxis * 2;
            int iterations = 0;

            while (iterations < maxIterations)
            {
                iterations++;

                int hash = HashCell(cell.X, cell.Y);
                if (_cells.TryGetValue(hash, out var cellColliders))
                {
                    foreach (var collider in cellColliders)
                    {
                        if ((collider.Layer & layerMask) == 0)
                            continue;

                        results.Add(collider);
                    }
                }

                Vector2 tMax = new Vector2(
                    cell.X + (direction.X > 0 ? 1 : 0),
                    cell.Y + (direction.Y > 0 ? 1 : 0)
                );

                tMax = (tMax - origin.ToVector2Int()) * tDelta;

                if (tMax.X < tMax.Y)
                {
                    if (cell.X == (int)endPoint.X)
                        break;
                    cell.X += (int)Math.Sign(direction.X);
                }
                else
                {
                    if (cell.Y == (int)endPoint.Y)
                        break;
                    cell.Y += (int)Math.Sign(direction.Y);
                }
            }

            return results;
        }

        public override void SetLayerCollision(int layer1, int layer2, bool enabled)
        {
            int key = (layer1 << 16) | layer2;
            int reverseKey = (layer2 << 16) | layer1;

            if (enabled)
            {
                _layerCollisions.Remove(key);
                _layerCollisions.Remove(reverseKey);
            }
            else
            {
                _layerCollisions[key] = false;
                _layerCollisions[reverseKey] = false;
            }
        }

        private Vector2Int GetCell(Vector2 point)
        {
            return new Vector2Int(
                (int)Math.Floor(point.X / _cellSize),
                (int)Math.Floor(point.Y / _cellSize)
            );
        }

        private int HashCell(int x, int y)
        {
            int hash1 = x * 73856093;
            int hash2 = y * 19349663;
            return (hash1 ^ hash2) % (_maxCellsPerAxis * _maxCellsPerAxis);
        }
    }

    public class NarrowPhase
    {
        public bool TestCollision(Collider a, Collider b, out Manifold manifold)
        {
            manifold = new Manifold(a, b);

            if (a.Shape == null || b.Shape == null)
                return false;

            Transform2D transformA = a.Body != null
                ? new Transform2D(a.Body.Position, a.Body.Rotation)
                : Transform2D.Identity;

            Transform2D transformB = b.Body != null
                ? new Transform2D(b.Body.Position, b.Body.Rotation)
                : Transform2D.Identity;

            if (a.Shape is CircleShape circleA && b.Shape is CircleShape circleB)
            {
                return CircleVsCircle(transformA, circleA, transformB, circleB, ref manifold);
            }
            else if (a.Shape is CircleShape circle)
            {
                return CircleVsPolygon(transformA, circle, transformB, b.Shape as PolygonShape, ref manifold);
            }
            else if (b.Shape is CircleShape circle2)
            {
                if (CircleVsPolygon(transformB, circle2, transformA, a.Shape as PolygonShape, ref manifold))
                {
                    manifold.Normal = -manifold.Normal;
                    return true;
                }
                return false;
            }
            else if (a.Shape is PolygonShape polyA && b.Shape is PolygonShape polyB)
            {
                return PolygonVsPolygon(transformA, polyA, transformB, polyB, ref manifold);
            }
            else if (a.Shape is PolygonShape poly)
            {
                return PolygonShapeVsPolygon(transformA, poly, transformB, b.Shape, ref manifold);
            }
            else if (b.Shape is PolygonShape poly2)
            {
                if (PolygonShapeVsPolygon(transformB, poly2, transformA, a.Shape, ref manifold))
                {
                    manifold.Normal = -manifold.Normal;
                    return true;
                }
                return false;
            }

            return false;
        }

        private bool CircleVsCircle(Transform2D transformA, CircleShape a, Transform2D transformB, CircleShape b, ref Manifold manifold)
        {
            Vector2 centerA = transformA.TransformPoint(a.Center);
            Vector2 centerB = transformB.TransformPoint(b.Center);

            Vector2 delta = centerB - centerA;
            float distSq = delta.LengthSquared();
            float radiusSum = a.Radius + b.Radius;

            if (distSq >= radiusSum * radiusSum)
                return false;

            float dist = (float)Math.Sqrt(distSq);
            float penetration = radiusSum - dist;

            if (dist > MathHelper.Epsilon)
            {
                manifold.Normal = delta / dist;
                manifold.Penetration = penetration;
            }
            else
            {
                manifold.Normal = Vector2.Right;
                manifold.Penetration = radiusSum;
            }

            manifold.Contacts.Add(new ContactPoint
            {
                Position = centerA + manifold.Normal * a.Radius
            });

            return true;
        }

        private bool CircleVsPolygon(Transform2D circleTransform, CircleShape circle, Transform2D polyTransform, PolygonShape? polygon, ref Manifold manifold)
        {
            if (polygon == null)
                return false;

            Vector2[] vertices = polygon.GetVertices(polyTransform);
            Vector2 center = circleTransform.TransformPoint(circle.Center);

            float minDistance = float.MaxValue;
            Vector2 closestPoint = Vector2.Zero;
            Vector2 closestNormal = Vector2.Zero;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v1 = vertices[i];
                Vector2 v2 = vertices[(i + 1) % vertices.Length];

                if (!FindClosestPointOnSegment(center, v1, v2, out Vector2 closest, out float dist))
                    continue;

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPoint = closest;

                    Vector2 edge = v2 - v1;
                    closestNormal = new Vector2(-edge.Y, edge.X).Normalized();
                }
            }

            if (minDistance > circle.Radius)
                return false;

            manifold.Normal = (center - closestPoint).Normalized();
            if (manifold.Normal.LengthSquared() < MathHelper.Epsilon)
                manifold.Normal = Vector2.Up;

            manifold.Penetration = circle.Radius - minDistance;
            manifold.Contacts.Add(new ContactPoint
            {
                Position = closestPoint
            });

            return true;
        }

        private bool PolygonVsPolygon(Transform2D transformA, PolygonShape a, Transform2D transformB, PolygonShape b, ref Manifold manifold)
        {
            Vector2[] vertsA = a.GetVertices(transformA);
            Vector2[] vertsB = b.GetVertices(transformB);

            float minPenetration = float.MaxValue;
            Vector2 minNormal = Vector2.Zero;
            bool foundCollision = false;

            Vector2[] axesA = GetPolygonAxes(vertsA);
            Vector2[] axesB = GetPolygonAxes(vertsB);

            foreach (var axis in axesA)
            {
                if (axis.LengthSquared() < MathHelper.Epsilon)
                    continue;

                (float minA, float maxA) = ProjectPolygon(vertsA, axis);
                (float minB, float maxB) = ProjectPolygon(vertsB, axis);

                if (maxA < minB || maxB < minA)
                    return false;

                float overlap = Math.Min(maxA - minB, maxB - minA);
                if (overlap < minPenetration)
                {
                    minPenetration = overlap;
                    minNormal = axis;
                    foundCollision = true;
                }
            }

            foreach (var axis in axesB)
            {
                if (axis.LengthSquared() < MathHelper.Epsilon)
                    continue;

                (float minA, float maxA) = ProjectPolygon(vertsA, axis);
                (float minB, float maxB) = ProjectPolygon(vertsB, axis);

                if (maxA < minB || maxB < minA)
                    return false;

                float overlap = Math.Min(maxA - minB, maxB - minA);
                if (overlap < minPenetration)
                {
                    minPenetration = overlap;
                    minNormal = axis;
                    foundCollision = true;
                }
            }

            if (!foundCollision)
                return false;

            Vector2 centerA = GetPolygonCenter(vertsA);
            Vector2 centerB = GetPolygonCenter(vertsB);
            Vector2 direction = centerB - centerA;

            if (direction.Dot(minNormal) < 0)
                minNormal = -minNormal;

            manifold.Normal = minNormal;
            manifold.Penetration = minPenetration;

            FindContactPoints(vertsA, vertsB, manifold.Normal, ref manifold);

            return true;
        }

        private bool PolygonShapeVsPolygon(Transform2D transformA, PolygonShape polygon, Transform2D transformB, IShape? shape, ref Manifold manifold)
        {
            if (shape == null)
                return false;

            if (shape is PolygonShape polyB)
            {
                return PolygonVsPolygon(transformA, polygon, transformB, polyB, ref manifold);
            }
            else if (shape is BoxShape boxB)
            {
                var tempPoly = PolygonShape.CreateBox(boxB.HalfExtents);
                return PolygonVsPolygon(transformA, polygon, transformB, tempPoly, ref manifold);
            }
            else if (shape is CircleShape circleB)
            {
                if (CircleVsPolygon(transformB, circleB, transformA, polygon, ref manifold))
                {
                    manifold.Normal = -manifold.Normal;
                    return true;
                }
                return false;
            }

            return false;
        }

        private Vector2[] GetPolygonAxes(Vector2[] vertices)
        {
            var axes = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 edge = vertices[(i + 1) % vertices.Length] - vertices[i];
                axes[i] = new Vector2(-edge.Y, edge.X).Normalized();
            }
            return axes;
        }

        private (float Min, float Max) ProjectPolygon(Vector2[] vertices, Vector2 axis)
        {
            float min = vertices[0].Dot(axis);
            float max = min;

            for (int i = 1; i < vertices.Length; i++)
            {
                float proj = vertices[i].Dot(axis);
                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }

            return (min, max);
        }

        private Vector2 GetPolygonCenter(Vector2[] vertices)
        {
            Vector2 center = Vector2.Zero;
            for (int i = 0; i < vertices.Length; i++)
            {
                center += vertices[i];
            }
            return center / vertices.Length;
        }

        private bool FindClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b, out Vector2 closest, out float distance)
        {
            Vector2 ab = b - a;
            Vector2 ap = point - a;

            float t = ap.Dot(ab) / ab.Dot(ab);
            t = Math.Clamp(t, 0f, 1f);

            closest = a + ab * t;
            distance = (point - closest).Length();
            return true;
        }

        private void FindContactPoints(Vector2[] vertsA, Vector2[] vertsB, Vector2 normal, ref Manifold manifold)
        {
            manifold.Contacts.Clear();

            int minIndex = 0;
            float minDot = float.MaxValue;

            for (int i = 0; i < vertsA.Length; i++)
            {
                float dot = vertsA[i].Dot(normal);
                if (dot < minDot)
                {
                    minDot = dot;
                    minIndex = i;
                }
            }

            manifold.Contacts.Add(new ContactPoint
            {
                Position = vertsA[minIndex]
            });
        }

        public bool IsValidPair(Collider a, Collider b)
        {
            if (a.Body != null && b.Body != null && a.Body == b.Body)
                return false;

            if ((a.CollisionMask & b.Layer) == 0)
                return false;

            return true;
        }
    }

    public class CollisionSolver
    {
        private List<Manifold> _contacts;
        private float _slop;
        private float _baumgarte;
        private float _maxPenetration;

        public float Slop
        {
            get => _slop;
            set => _slop = Math.Max(0f, value);
        }

        public float Baumgarte
        {
            get => _baumgarte;
            set => _baumgarte = Math.Clamp(value, 0f, 1f);
        }

        public float MaxPenetration
        {
            get => _maxPenetration;
            set => _maxPenetration = Math.Max(0f, value);
        }

        public CollisionSolver()
        {
            _contacts = new List<Manifold>();
            _slop = 0.005f;
            _baumgarte = 0.2f;
            _maxPenetration = 0.05f;
        }

        public void Initialize(List<Manifold> contacts)
        {
            _contacts = contacts;
        }

        public void SolveVelocities()
        {
            foreach (var contact in _contacts)
            {
                if (contact.A.Body == null || contact.B.Body == null)
                    continue;

                if (contact.IsResolved)
                    continue;

                SolveContact(contact);
            }
        }

        public void SolvePositions()
        {
            foreach (var contact in _contacts)
            {
                if (contact.A.Body == null || contact.B.Body == null)
                    continue;

                if (contact.IsResolved)
                    continue;

                CorrectPosition(contact);
            }
        }

        private void SolveContact(Manifold contact)
        {
            var bodyA = contact.A.Body;
            var bodyB = contact.B.Body;

            float restitution = Math.Min(bodyA.Restitution, bodyB.Restitution);
            float friction = (float)Math.Sqrt(bodyA.Friction * bodyB.Friction);

            foreach (var point in contact.Contacts)
            {
                Vector2 rA = point.Position - bodyA.Position;
                Vector2 rB = point.Position - bodyB.Position;

                Vector2 velocityA = bodyA.Velocity + new Vector2(-rA.Y, rA.X) * bodyA.AngularVelocity;
                Vector2 velocityB = bodyB.Velocity + new Vector2(-rB.Y, rB.X) * bodyB.AngularVelocity;
                Vector2 relativeVelocity = velocityB - velocityA;

                float contactVelocity = relativeVelocity.Dot(contact.Normal);

                if (contactVelocity > 0)
                    continue;

                float raCrossN = rA.Cross(contact.Normal);
                float rbCrossN = rB.Cross(contact.Normal);

                float invMassSum = bodyA.InverseMass + bodyB.InverseMass +
                                   raCrossN * raCrossN * bodyA.InverseInertia +
                                   rbCrossN * rbCrossN * bodyB.InverseInertia;

                float j = -(1f + restitution) * contactVelocity;
                j /= invMassSum;
                j /= contact.Contacts.Count;

                Vector2 impulse = contact.Normal * j;

                bodyA.Velocity -= impulse * bodyA.InverseMass;
                bodyA.AngularVelocity -= rA.Cross(impulse) * bodyA.InverseInertia;

                bodyB.Velocity += impulse * bodyB.InverseMass;
                bodyB.AngularVelocity += rB.Cross(impulse) * bodyB.InverseInertia;

                Vector2 tangent = (relativeVelocity - contact.Normal * contactVelocity).Normalized();

                float tangentVelocity = relativeVelocity.Dot(tangent);

                float raCrossT = rA.Cross(tangent);
                float rbCrossT = rB.Cross(tangent);

                float invMassSumT = bodyA.InverseMass + bodyB.InverseMass +
                                    raCrossT * raCrossT * bodyA.InverseInertia +
                                    rbCrossT * rbCrossT * bodyB.InverseInertia;

                float jt = -tangentVelocity;
                jt /= invMassSumT;
                jt /= contact.Contacts.Count;

                float maxFriction = friction * j;
                jt = Math.Clamp(jt, -maxFriction, maxFriction);

                Vector2 tangentImpulse = tangent * jt;

                bodyA.Velocity -= tangentImpulse * bodyA.InverseMass;
                bodyA.AngularVelocity -= rA.Cross(tangentImpulse) * bodyA.InverseInertia;

                bodyB.Velocity += tangentImpulse * bodyB.InverseMass;
                bodyB.AngularVelocity += rB.Cross(tangentImpulse) * bodyB.InverseInertia;
            }
        }

        private void CorrectPosition(Manifold contact)
        {
            var bodyA = contact.A.Body;
            var bodyB = contact.B.Body;

            float penetration = Math.Max(contact.Penetration - _slop, 0f);

            if (penetration > _maxPenetration)
            {
                contact.IsResolved = true;
                return;
            }

            float invMassSum = bodyA.InverseMass + bodyB.InverseMass;
            if (invMassSum <= 0f)
                return;

            Vector2 correction = contact.Normal * (_baumgarte / invMassSum) * penetration;

            bodyA.Position -= correction * bodyA.InverseMass;
            bodyB.Position += correction * bodyB.InverseMass;

            if (contact.Penetration < 0.001f)
            {
                contact.IsResolved = true;
            }
        }
    }
}

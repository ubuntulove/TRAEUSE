using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;
using WPFEngine.Physics.Bodies;

namespace WPFEngine.Physics.Sensors
{
    public enum SensorType
    {
        Circle,
        Box,
        Capsule,
        Polygon
    }

    public enum TriggerMode
    {
        None,
        OnEnter,
        OnStay,
        OnExit,
        All
    }

    public class SensorEventArgs : EventArgs
    {
        public Collider Other { get; }
        public Vector2 ContactPoint { get; }
        public Vector2 ContactNormal { get; }
        public float Penetration { get; }
        public Sensor Sensor { get; }
        public SensorEventType EventType { get; }

        public SensorEventArgs(Sensor sensor, Collider other, Vector2 contactPoint, Vector2 contactNormal, float penetration, SensorEventType eventType)
        {
            Sensor = sensor;
            Other = other;
            ContactPoint = contactPoint;
            ContactNormal = contactNormal;
            Penetration = penetration;
            EventType = eventType;
        }
    }

    public enum SensorEventType
    {
        Enter,
        Stay,
        Exit
    }

    public abstract class Sensor
    {
        protected SensorType _type;
        protected TriggerMode _triggerMode;
        protected int _layer;
        protected int _collisionMask;
        protected bool _isActive;
        protected Vector2 _offset;
        protected float _rotation;
        protected RigidBody? _attachedBody;
        protected Collider? _attachedCollider;
        protected HashSet<Collider> _detectedColliders;
        protected List<ContactInfo> _contacts;

        public SensorType Type => _type;
        public TriggerMode TriggerMode
        {
            get => _triggerMode;
            set => _triggerMode = value;
        }
        public int Layer
        {
            get => _layer;
            set => _layer = value;
        }
        public int CollisionMask
        {
            get => _collisionMask;
            set => _collisionMask = value;
        }
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }
        public Vector2 Offset
        {
            get => _offset;
            set => _offset = value;
        }
        public float Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }
        public RigidBody? AttachedBody
        {
            get => _attachedBody;
            set => _attachedBody = value;
        }
        public Collider? AttachedCollider
        {
            get => _attachedCollider;
            set => _attachedCollider = value;
        }
        public IReadOnlyCollection<Collider> DetectedColliders => _detectedColliders;
        public IReadOnlyList<ContactInfo> Contacts => _contacts;

        public event EventHandler<SensorEventArgs>? OnEnter;
        public event EventHandler<SensorEventArgs>? OnStay;
        public event EventHandler<SensorEventArgs>? OnExit;

        protected Sensor(SensorType type)
        {
            _type = type;
            _triggerMode = TriggerMode.All;
            _layer = 0;
            _collisionMask = -1;
            _isActive = true;
            _offset = Vector2.Zero;
            _rotation = 0f;
            _detectedColliders = new HashSet<Collider>();
            _contacts = new List<ContactInfo>();
        }

        public Transform2D GetWorldTransform()
        {
            Vector2 worldPosition = _offset;
            float worldRotation = _rotation;

            if (_attachedBody != null)
            {
                worldPosition = _attachedBody.Position + _offset.Rotate(_attachedBody.Rotation);
                worldRotation += _attachedBody.Rotation;
            }
            else if (_attachedCollider?.Body != null)
            {
                worldPosition = _attachedCollider.Body.Position + _offset.Rotate(_attachedCollider.Body.Rotation);
                worldRotation += _attachedCollider.Body.Rotation;
            }

            return new Transform2D(worldPosition, worldRotation);
        }

        public abstract AABB GetAABB();
        public abstract bool ContainsPoint(Vector2 point);
        public abstract Vector2[] GetVertices();

        public virtual void AttachToBody(RigidBody body)
        {
            _attachedBody = body;
            _attachedCollider = body.Collider;
        }

        public virtual void Detach()
        {
            _attachedBody = null;
            _attachedCollider = null;
        }

        public virtual void ProcessDetection(List<Collider> detected, PhysicsWorld world)
        {
            if (!_isActive)
                return;

            var newDetected = new HashSet<Collider>(detected);
            var previousDetected = new HashSet<Collider>(_detectedColliders);

            var entered = newDetected.Except(previousDetected).ToList();
            var exited = previousDetected.Except(newDetected).ToList();
            var stayed = newDetected.Intersect(previousDetected).ToList();

            foreach (var collider in entered)
            {
                if (ShouldDetect(collider))
                {
                    _detectedColliders.Add(collider);
                    var contact = CreateContactInfo(collider, world);
                    _contacts.Add(contact);

                    if (_triggerMode.HasFlag(TriggerMode.OnEnter) || _triggerMode.HasFlag(TriggerMode.All))
                    {
                        OnEnter?.Invoke(this, new SensorEventArgs(this, collider, contact.Point, contact.Normal, contact.Penetration, SensorEventType.Enter));
                    }
                }
            }

            foreach (var collider in stayed)
            {
                if (ShouldDetect(collider))
                {
                    var contact = CreateContactInfo(collider, world);
                    UpdateContact(collider, contact);

                    if (_triggerMode.HasFlag(TriggerMode.OnStay) || _triggerMode.HasFlag(TriggerMode.All))
                    {
                        OnStay?.Invoke(this, new SensorEventArgs(this, collider, contact.Point, contact.Normal, contact.Penetration, SensorEventType.Stay));
                    }
                }
            }

            foreach (var collider in exited)
            {
                _detectedColliders.Remove(collider);
                RemoveContact(collider);

                if (_triggerMode.HasFlag(TriggerMode.OnExit) || _triggerMode.HasFlag(TriggerMode.All))
                {
                    OnExit?.Invoke(this, new SensorEventArgs(this, collider, Vector2.Zero, Vector2.Zero, 0f, SensorEventType.Exit));
                }
            }
        }

        protected virtual bool ShouldDetect(Collider collider)
        {
            if ((_collisionMask & collider.Layer) == 0)
                return false;

            if (_attachedBody != null && collider.Body == _attachedBody)
                return false;

            if (_attachedCollider != null && collider == _attachedCollider)
                return false;

            return true;
        }

        protected virtual ContactInfo CreateContactInfo(Collider collider, PhysicsWorld world)
        {
            return new ContactInfo
            {
                Collider = collider,
                Point = Vector2.Zero,
                Normal = Vector2.Zero,
                Penetration = 0f,
                FirstContactTime = world.Contacts.Count > 0 ? TimeSpan.Zero : TimeSpan.Zero
            };
        }

        protected virtual void UpdateContact(Collider collider, ContactInfo newContact)
        {
            var existingContact = _contacts.FirstOrDefault(c => c.Collider == collider);
            if (existingContact.Collider != null)
            {
                existingContact.Point = newContact.Point;
                existingContact.Normal = newContact.Normal;
                existingContact.Penetration = newContact.Penetration;
            }
        }

        protected virtual void RemoveContact(Collider collider)
        {
            _contacts.RemoveAll(c => c.Collider == collider);
        }

        public virtual void Reset()
        {
            _detectedColliders.Clear();
            _contacts.Clear();
        }
    }

    public struct ContactInfo
    {
        public Collider Collider;
        public Vector2 Point;
        public Vector2 Normal;
        public float Penetration;
        public TimeSpan FirstContactTime;
    }

    public class CircleSensor : Sensor
    {
        private float _radius;

        public float Radius
        {
            get => _radius;
            set => _radius = Math.Max(0f, value);
        }

        public CircleSensor(float radius) : base(SensorType.Circle)
        {
            _radius = Math.Max(0f, radius);
        }

        public CircleSensor(float radius, Vector2 offset) : base(SensorType.Circle)
        {
            _radius = Math.Max(0f, radius);
            _offset = offset;
        }

        public override AABB GetAABB()
        {
            Transform2D transform = GetWorldTransform();
            return AABB.CreateFromCircle(transform.Position, _radius);
        }

        public override bool ContainsPoint(Vector2 point)
        {
            Transform2D transform = GetWorldTransform();
            return point.Distance(transform.Position) <= _radius;
        }

        public override Vector2[] GetVertices()
        {
            Transform2D transform = GetWorldTransform();
            int segments = 32;
            Vector2[] vertices = new Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * MathHelper.TwoPi;
                vertices[i] = transform.Position + new Vector2(MathHelper.Cos(angle), MathHelper.Sin(angle)) * _radius;
            }

            return vertices;
        }

        public bool OverlapsWithCircle(Vector2 center, float otherRadius)
        {
            Transform2D transform = GetWorldTransform();
            float distance = transform.Position.Distance(center);
            return distance < _radius + otherRadius;
        }

        public float GetOverlapDepth(Vector2 center, float otherRadius)
        {
            Transform2D transform = GetWorldTransform();
            float distance = transform.Position.Distance(center);
            float sumRadii = _radius + otherRadius;
            return sumRadii - distance;
        }
    }

    public class BoxSensor : Sensor
    {
        private Vector2 _size;

        public Vector2 Size
        {
            get => _size;
            set
            {
                _size.X = Math.Max(0f, value.X);
                _size.Y = Math.Max(0f, value.Y);
            }
        }

        public Vector2 HalfExtents => _size * 0.5f;

        public BoxSensor(Vector2 size) : base(SensorType.Box)
        {
            _size = new Vector2(Math.Max(0f, size.X), Math.Max(0f, size.Y));
        }

        public BoxSensor(Vector2 size, Vector2 offset) : base(SensorType.Box)
        {
            _size = new Vector2(Math.Max(0f, size.X), Math.Max(0f, size.Y));
            _offset = offset;
        }

        public BoxSensor(float width, float height) : base(SensorType.Box)
        {
            _size = new Vector2(Math.Max(0f, width), Math.Max(0f, height));
        }

        public override AABB GetAABB()
        {
            return AABB.FromVertices(GetVertices());
        }

        public override bool ContainsPoint(Vector2 point)
        {
            Transform2D transform = GetWorldTransform();
            Vector2 localPoint = transform.InverseTransformPoint(point);
            return Math.Abs(localPoint.X) <= HalfExtents.X && Math.Abs(localPoint.Y) <= HalfExtents.Y;
        }

        public override Vector2[] GetVertices()
        {
            Transform2D transform = GetWorldTransform();
            Vector2[] localVertices = new Vector2[]
            {
                new Vector2(-HalfExtents.X, -HalfExtents.Y),
                new Vector2(HalfExtents.X, -HalfExtents.Y),
                new Vector2(HalfExtents.X, HalfExtents.Y),
                new Vector2(-HalfExtents.X, HalfExtents.Y)
            };

            Vector2[] worldVertices = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                worldVertices[i] = transform.TransformPoint(localVertices[i]);
            }

            return worldVertices;
        }

        public Vector2 GetCorner(int index)
        {
            if (index < 0 || index > 3)
                return Vector2.Zero;

            Transform2D transform = GetWorldTransform();
            Vector2[] corners = GetVertices();
            return corners[index];
        }

        public Vector2[] GetEdges()
        {
            Vector2[] vertices = GetVertices();
            Vector2[] edges = new Vector2[4];

            for (int i = 0; i < 4; i++)
            {
                edges[i] = vertices[(i + 1) % 4] - vertices[i];
            }

            return edges;
        }
    }

    public class CapsuleSensor : Sensor
    {
        private float _radius;
        private float _height;

        public float Radius
        {
            get => _radius;
            set => _radius = Math.Max(0f, value);
        }

        public float Height
        {
            get => _height;
            set => _height = Math.Max(0f, value);
        }

        public float HalfHeight => _height * 0.5f;

        public Vector2 TopCenter
        {
            get
            {
                Transform2D transform = GetWorldTransform();
                return transform.Position + new Vector2(0f, HalfHeight).Rotate(transform.Rotation);
            }
        }

        public Vector2 BottomCenter
        {
            get
            {
                Transform2D transform = GetWorldTransform();
                return transform.Position + new Vector2(0f, -HalfHeight).Rotate(transform.Rotation);
            }
        }

        public CapsuleSensor(float radius, float height) : base(SensorType.Capsule)
        {
            _radius = Math.Max(0f, radius);
            _height = Math.Max(0f, height);
        }

        public CapsuleSensor(float radius, float height, Vector2 offset) : base(SensorType.Capsule)
        {
            _radius = Math.Max(0f, radius);
            _height = Math.Max(0f, height);
            _offset = offset;
        }

        public override AABB GetAABB()
        {
            Transform2D transform = GetWorldTransform();
            float totalHeight = _height + _radius * 2f;
            float halfHeight = totalHeight * 0.5f;

            Vector2 center = transform.Position;
            float rotation = transform.Rotation;

            if (Math.Abs(rotation) < MathHelper.Epsilon || Math.Abs(rotation - MathHelper.Pi) < MathHelper.Epsilon)
            {
                return new AABB(
                    new Vector2(center.X - _radius, center.Y - halfHeight),
                    new Vector2(center.X + _radius, center.Y + halfHeight)
                );
            }
            else
            {
                Vector2[] vertices = GetVertices();
                return AABB.FromVertices(vertices);
            }
        }

        public override bool ContainsPoint(Vector2 point)
        {
            Transform2D transform = GetWorldTransform();
            Vector2 localPoint = transform.InverseTransformPoint(point);

            if (localPoint.Y > HalfHeight)
            {
                return localPoint.Distance(new Vector2(0f, HalfHeight)) <= _radius;
            }
            else if (localPoint.Y < -HalfHeight)
            {
                return localPoint.Distance(new Vector2(0f, -HalfHeight)) <= _radius;
            }
            else
            {
                return Math.Abs(localPoint.X) <= _radius;
            }
        }

        public override Vector2[] GetVertices()
        {
            Transform2D transform = GetWorldTransform();
            List<Vector2> vertices = new List<Vector2>();
            int segments = 8;

            for (int i = 0; i <= segments / 2; i++)
            {
                float angle = -MathHelper.PiOver2 + (float)i / (segments / 2) * MathHelper.Pi;
                vertices.Add(transform.TransformPoint(new Vector2(MathHelper.Cos(angle) * _radius, HalfHeight + MathHelper.Sin(angle) * _radius)));
            }

            for (int i = segments / 2; i <= segments; i++)
            {
                float angle = MathHelper.PiOver2 + (float)(i - segments / 2) / (segments / 2) * MathHelper.Pi;
                vertices.Add(transform.TransformPoint(new Vector2(MathHelper.Cos(angle) * _radius, -HalfHeight + MathHelper.Sin(angle) * _radius)));
            }

            return vertices.ToArray();
        }

        public Vector2 ClosestPointOnSegment(Vector2 point)
        {
            Transform2D transform = GetWorldTransform();
            Vector2 top = TopCenter;
            Vector2 bottom = BottomCenter;

            Vector2 segment = bottom - top;
            float t = (point - top).Dot(segment) / segment.Dot(segment);
            t = Math.Clamp(t, 0f, 1f);

            return top + segment * t;
        }
    }

    public class PolygonSensor : Sensor
    {
        private Vector2[] _localVertices;

        public Vector2[] LocalVertices => _localVertices;
        public int VertexCount => _localVertices?.Length ?? 0;

        public PolygonSensor(Vector2[] vertices) : base(SensorType.Polygon)
        {
            if (vertices == null || vertices.Length < 3)
            {
                _localVertices = Array.Empty<Vector2>();
            }
            else
            {
                _localVertices = (Vector2[])vertices.Clone();
            }
        }

        public PolygonSensor(List<Vector2> vertices) : base(SensorType.Polygon)
        {
            if (vertices == null || vertices.Count < 3)
            {
                _localVertices = Array.Empty<Vector2>();
            }
            else
            {
                _localVertices = vertices.ToArray();
            }
        }

        public void SetVertices(Vector2[] vertices)
        {
            if (vertices == null || vertices.Length < 3)
            {
                _localVertices = Array.Empty<Vector2>();
            }
            else
            {
                _localVertices = (Vector2[])vertices.Clone();
            }
        }

        public override AABB GetAABB()
        {
            return AABB.FromVertices(GetVertices());
        }

        public override bool ContainsPoint(Vector2 point)
        {
            Transform2D transform = GetWorldTransform();
            Vector2 localPoint = transform.InverseTransformPoint(point);
            int n = _localVertices.Length;

            for (int i = 0; i < n; i++)
            {
                Vector2 edge = _localVertices[(i + 1) % n] - _localVertices[i];
                Vector2 toPoint = localPoint - _localVertices[i];
                if (edge.Cross(toPoint) > 0f)
                    return false;
            }

            return true;
        }

        public override Vector2[] GetVertices()
        {
            Transform2D transform = GetWorldTransform();
            Vector2[] worldVertices = new Vector2[_localVertices.Length];

            for (int i = 0; i < _localVertices.Length; i++)
            {
                worldVertices[i] = transform.TransformPoint(_localVertices[i]);
            }

            return worldVertices;
        }

        public static PolygonSensor CreateRegular(int sides, float radius)
        {
            Vector2[] vertices = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (float)i / sides * MathHelper.TwoPi - MathHelper.PiOver2;
                vertices[i] = new Vector2(MathHelper.Cos(angle), MathHelper.Sin(angle)) * radius;
            }
            return new PolygonSensor(vertices);
        }

        public static PolygonSensor CreateBox(Vector2 halfExtents)
        {
            return new PolygonSensor(new Vector2[]
            {
                new Vector2(-halfExtents.X, -halfExtents.Y),
                new Vector2(halfExtents.X, -halfExtents.Y),
                new Vector2(halfExtents.X, halfExtents.Y),
                new Vector2(-halfExtents.X, halfExtents.Y)
            });
        }

        public Vector2 GetNormal(int edgeIndex)
        {
            if (edgeIndex < 0 || edgeIndex >= _localVertices.Length)
                return Vector2.Zero;

            Vector2 edge = _localVertices[(edgeIndex + 1) % _localVertices.Length] - _localVertices[edgeIndex];
            return new Vector2(-edge.Y, edge.X).Normalized();
        }

        public Vector2[] GetNormals()
        {
            Vector2[] normals = new Vector2[_localVertices.Length];
            for (int i = 0; i < _localVertices.Length; i++)
            {
                normals[i] = GetNormal(i);
            }
            return normals;
        }
    }

    public struct Ray
    {
        public Vector2 Origin { get; set; }
        public Vector2 Direction { get; set; }
        public float MaxDistance { get; set; }

        public Ray(Vector2 origin, Vector2 direction, float maxDistance = float.MaxValue)
        {
            Origin = origin;
            Direction = direction.Normalized();
            MaxDistance = maxDistance;
        }

        public Vector2 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        public Vector2 GetEndPoint()
        {
            return Origin + Direction * MaxDistance;
        }

        public static Ray FromPoints(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            return new Ray(start, distance > 0 ? direction / distance : Vector2.Right, distance);
        }
    }

    public struct RaycastHit2D
    {
        public bool IsHit { get; set; }
        public Vector2 Point { get; set; }
        public Vector2 Normal { get; set; }
        public float Distance { get; set; }
        public Collider? Collider { get; set; }
        public RigidBody? Body { get; set; }
        public float Fraction { get; set; }
        public int FaceIndex { get; set; }

        public static RaycastHit2D NoHit => new RaycastHit2D { IsHit = false, Point = Vector2.Zero, Normal = Vector2.Zero, Distance = float.MaxValue, Collider = null, Body = null, Fraction = 1f, FaceIndex = -1 };

        public static RaycastHit2D FromManifold(Manifold manifold, Vector2 origin, Vector2 direction)
        {
            Vector2 point = manifold.Contacts.Count > 0 ? manifold.Contacts[0].Position : origin + direction * 0.1f;
            return new RaycastHit2D
            {
                IsHit = true,
                Point = point,
                Normal = manifold.Normal,
                Distance = (point - origin).Length(),
                Collider = manifold.A,
                Body = manifold.A.Body,
                Fraction = 0f,
                FaceIndex = 0
            };
        }
    }

    public static class RaycastHelper
    {
        public static bool LineSegmentTest(Vector2 start, Vector2 end, Vector2 point, float radius, out Vector2 closestPoint, out float t)
        {
            closestPoint = Vector2.Zero;
            t = 0f;

            Vector2 segment = end - start;
            float segmentLength = segment.Length();
            if (segmentLength < MathHelper.Epsilon)
            {
                closestPoint = start;
                t = 0f;
                return (point - start).LengthSquared() <= radius * radius;
            }

            Vector2 direction = segment / segmentLength;
            Vector2 toPoint = point - start;
            float proj = toPoint.Dot(direction);

            t = proj / segmentLength;
            t = Math.Clamp(t, 0f, 1f);

            closestPoint = start + direction * (t * segmentLength);
            float dist = (point - closestPoint).Length();

            return dist <= radius;
        }

        public static bool CircleLineTest(Vector2 center, float radius, Vector2 lineStart, Vector2 lineEnd, out Vector2 intersection1, out Vector2 intersection2, out int intersectionCount)
        {
            intersection1 = Vector2.Zero;
            intersection2 = Vector2.Zero;
            intersectionCount = 0;

            Vector2 d = lineEnd - lineStart;
            Vector2 f = lineStart - center;

            float a = d.Dot(d);
            float b = 2f * f.Dot(d);
            float c = f.Dot(f) - radius * radius;

            float discriminant = b * b - 4f * a * c;

            if (discriminant < 0f)
                return false;

            float sqrtDiscriminant = (float)Math.Sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2f * a);
            float t2 = (-b + sqrtDiscriminant) / (2f * a);

            intersectionCount = 0;

            if (t1 >= 0f && t1 <= 1f)
            {
                intersection1 = lineStart + d * t1;
                intersectionCount++;
            }

            if (t2 >= 0f && t2 <= 1f)
            {
                if (intersectionCount == 0)
                {
                    intersection1 = lineStart + d * t2;
                    intersectionCount++;
                }
                else
                {
                    intersection2 = lineStart + d * t2;
                    intersectionCount++;
                }
            }

            return intersectionCount > 0;
        }

        public static Vector2 GetClosestPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 direction = lineEnd - lineStart;
            float t = (point - lineStart).Dot(direction) / direction.Dot(direction);
            t = Math.Clamp(t, 0f, 1f);
            return lineStart + direction * t;
        }

        public static float GetDistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 closest = GetClosestPointOnLine(point, lineStart, lineEnd);
            return (point - closest).Length();
        }

        public static Vector2 GetNormalAtPointOnCircle(Vector2 point, Vector2 circleCenter)
        {
            Vector2 direction = point - circleCenter;
            float length = direction.Length();
            if (length < MathHelper.Epsilon)
                return Vector2.Right;
            return direction / length;
        }

        public static Vector2 GetNormalAtPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 edge = segmentEnd - segmentStart;
            Vector2 normal = new Vector2(-edge.Y, edge.X);
            float length = normal.Length();
            if (length < MathHelper.Epsilon)
                return Vector2.Zero;
            return normal / length;
        }
    }

    public class PhysicsQuery
    {
        private readonly PhysicsWorld _world;

        public PhysicsQuery(PhysicsWorld world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public List<Collider> OverlapCircle(Vector2 center, float radius, int layerMask = -1)
        {
            return _world.OverlapCircle(center, radius, layerMask);
        }

        public List<Collider> OverlapBox(Vector2 center, Vector2 halfExtents, float rotation = 0f, int layerMask = -1)
        {
            return _world.OverlapBox(center, halfExtents, rotation, layerMask);
        }

        public List<Collider> OverlapCapsule(Vector2 start, Vector2 end, float radius, int layerMask = -1)
        {
            var results = new List<Collider>();
            float height = (end - start).Length();
            Vector2 center = (start + end) * 0.5f;

            var aabb = new AABB(
                new Vector2(Math.Min(start.X, end.X) - radius, Math.Min(start.Y, end.Y) - radius),
                new Vector2(Math.Max(start.X, end.X) + radius, Math.Max(start.Y, end.Y) + radius)
            );

            var broadPhase = GetBroadPhase();
            var potentialColliders = broadPhase.QueryAABB(aabb, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if (IsCapsuleOverlappingCollider(start, end, radius, collider))
                {
                    results.Add(collider);
                }
            }

            return results;
        }

        private bool IsCapsuleOverlappingCollider(Vector2 start, Vector2 end, float radius, Collider collider)
        {
            if (collider.Shape == null)
                return false;

            Transform2D transform = collider.Body != null
                ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                : Transform2D.Identity;

            if (collider.Shape is CircleShape circle)
            {
                Vector2 circleCenter = transform.TransformPoint(circle.Center);
                float dist = RaycastHelper.GetDistanceToLine(circleCenter, start, end);
                return dist <= radius + circle.Radius;
            }
            else if (collider.Shape is BoxShape box)
            {
                Vector2[] vertices = box.GetVertices(transform);
                foreach (var vertex in vertices)
                {
                    if (IsPointInCapsule(vertex, start, end, radius))
                        return true;
                }

                Vector2 capsuleCenter = (start + end) * 0.5f;
                if (box.ContainsPoint(capsuleCenter, transform))
                    return true;

                return false;
            }
            else if (collider.Shape is PolygonShape polygon)
            {
                Vector2[] vertices = polygon.GetVertices(transform);
                foreach (var vertex in vertices)
                {
                    if (IsPointInCapsule(vertex, start, end, radius))
                        return true;
                }

                Vector2 capsuleCenter = (start + end) * 0.5f;
                return polygon.ContainsPoint(capsuleCenter, transform);
            }
            else if (collider.Shape is CapsuleShape capsule)
            {
                Vector2[] vertices = capsule.GetVertices(transform);
                foreach (var vertex in vertices)
                {
                    if (IsPointInCapsule(vertex, start, end, radius))
                        return true;
                }
                return false;
            }

            return false;
        }

        private bool IsPointInCapsule(Vector2 point, Vector2 start, Vector2 end, float radius)
        {
            float dist = RaycastHelper.GetDistanceToLine(point, start, end);
            return dist <= radius;
        }

        public List<Collider> OverlapPolygon(Vector2[] vertices, int layerMask = -1)
        {
            var results = new List<Collider>();
            var aabb = AABB.FromVertices(vertices);

            var broadPhase = GetBroadPhase();
            var potentialColliders = broadPhase.QueryAABB(aabb, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if (IsPolygonOverlappingCollider(vertices, collider))
                {
                    results.Add(collider);
                }
            }

            return results;
        }

        private bool IsPolygonOverlappingCollider(Vector2[] vertices, Collider collider)
        {
            if (collider.Shape == null)
                return false;

            Transform2D transform = collider.Body != null
                ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                : Transform2D.Identity;

            if (collider.Shape is CircleShape circle)
            {
                Vector2 circleCenter = transform.TransformPoint(circle.Center);
                return IsPointInPolygon(circleCenter, vertices) || IsCircleOverlappingPolygon(circleCenter, circle.Radius, vertices);
            }
            else if (collider.Shape is BoxShape box)
            {
                Vector2[] boxVertices = box.GetVertices(transform);
                return ArePolygonsIntersecting(vertices, boxVertices);
            }
            else if (collider.Shape is PolygonShape polygon)
            {
                Vector2[] polyVertices = polygon.GetVertices(transform);
                return ArePolygonsIntersecting(vertices, polyVertices);
            }
            else if (collider.Shape is CapsuleShape capsule)
            {
                Vector2[] capsuleVertices = capsule.GetVertices(transform);
                return ArePolygonsIntersecting(vertices, capsuleVertices);
            }

            return false;
        }

        private bool IsPointInPolygon(Vector2 point, Vector2[] vertices)
        {
            int n = vertices.Length;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y)) &&
                    (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private bool IsCircleOverlappingPolygon(Vector2 center, float radius, Vector2[] vertices)
        {
            foreach (var vertex in vertices)
            {
                if ((vertex - center).LengthSquared() <= radius * radius)
                    return true;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v1 = vertices[i];
                Vector2 v2 = vertices[(i + 1) % vertices.Length];

                if (RaycastHelper.GetDistanceToLine(center, v1, v2) <= radius)
                    return true;
            }

            return IsPointInPolygon(center, vertices);
        }

        private bool ArePolygonsIntersecting(Vector2[] poly1, Vector2[] poly2)
        {
            Vector2[] axes = GetPolygonAxes(poly1);
            Vector2[] axes2 = GetPolygonAxes(poly2);
            axes = axes.Concat(axes2).ToArray();

            foreach (var axis in axes)
            {
                if (axis.LengthSquared() < MathHelper.Epsilon)
                    continue;

                (float min1, float max1) = ProjectPolygon(poly1, axis);
                (float min2, float max2) = ProjectPolygon(poly2, axis);

                if (max1 < min2 || max2 < min1)
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

        public List<RaycastHit2D> RaycastAll(Ray ray, int layerMask = -1)
        {
            var hits = new List<RaycastHit2D>();
            var allHits = _world.Raycast(ray.Origin, ray.Direction, ray.MaxDistance, layerMask);

            foreach (var hit in allHits)
            {
                if (hit.Distance <= ray.MaxDistance)
                {
                    hits.Add(new RaycastHit2D
                    {
                        IsHit = true,
                        Point = hit.Point,
                        Normal = hit.Normal,
                        Distance = hit.Distance,
                        Collider = hit.Collider,
                        Body = hit.Collider?.Body,
                        Fraction = hit.Fraction,
                        FaceIndex = 0
                    });
                }
            }

            return hits;
        }

        public RaycastHit2D RaycastSingle(Ray ray, int layerMask = -1)
        {
            var hit = _world.RaycastSingle(ray.Origin, ray.Direction, ray.MaxDistance, layerMask);
            if (hit == null)
                return RaycastHit2D.NoHit;

            return new RaycastHit2D
            {
                IsHit = true,
                Point = hit.Value.Point,
                Normal = hit.Value.Normal,
                Distance = hit.Value.Distance,
                Collider = hit.Value.Collider,
                Body = hit.Value.Collider?.Body,
                Fraction = hit.Value.Fraction,
                FaceIndex = 0
            };
        }

        public List<RaycastHit2D> LinecastAll(Vector2 start, Vector2 end, int layerMask = -1)
        {
            Ray ray = Ray.FromPoints(start, end);
            return RaycastAll(ray, layerMask);
        }

        public RaycastHit2D LinecastSingle(Vector2 start, Vector2 end, int layerMask = -1)
        {
            Ray ray = Ray.FromPoints(start, end);
            return RaycastSingle(ray, layerMask);
        }

        public bool PointCheck(Vector2 point, int layerMask = -1)
        {
            var aabb = new AABB(point - new Vector2(0.001f), point + new Vector2(0.001f));
            var broadPhase = GetBroadPhase();
            var potentialColliders = broadPhase.QueryAABB(aabb, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if (collider.Shape == null)
                    continue;

                Transform2D transform = collider.Body != null
                    ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                    : Transform2D.Identity;

                if (collider.Shape.ContainsPoint(point, transform))
                    return true;
            }

            return false;
        }

        public Collider? GetColliderAtPoint(Vector2 point, int layerMask = -1)
        {
            var aabb = new AABB(point - new Vector2(0.001f), point + new Vector2(0.001f));
            var broadPhase = GetBroadPhase();
            var potentialColliders = broadPhase.QueryAABB(aabb, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if (collider.Shape == null)
                    continue;

                Transform2D transform = collider.Body != null
                    ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                    : Transform2D.Identity;

                if (collider.Shape.ContainsPoint(point, transform))
                    return collider;
            }

            return null;
        }

        public List<Collider> QueryBox(Vector2 center, Vector2 halfExtents, float rotation = 0f, int layerMask = -1)
        {
            return OverlapBox(center, halfExtents, rotation, layerMask);
        }

        public List<Collider> QueryCircle(Vector2 center, float radius, int layerMask = -1)
        {
            return OverlapCircle(center, radius, layerMask);
        }

        public List<Collider> QueryLayer(int layer, int layerMask = -1)
        {
            var results = new List<Collider>();
            var allColliders = _world.Colliders;

            foreach (var collider in allColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                if ((collider.Layer & (1 << layer)) != 0)
                    results.Add(collider);
            }

            return results;
        }

        public List<Collider> QueryRegion(AABB region, int layerMask = -1)
        {
            var results = new List<Collider>();
            var broadPhase = GetBroadPhase();
            var potentialColliders = broadPhase.QueryAABB(region, layerMask);

            foreach (var collider in potentialColliders)
            {
                if ((collider.Layer & layerMask) == 0)
                    continue;

                Transform2D transform = collider.Body != null
                    ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                    : Transform2D.Identity;

                if (collider.Shape != null && collider.Shape.GetAABB(transform).Intersects(region))
                {
                    results.Add(collider);
                }
            }

            return results;
        }

        private BroadPhase GetBroadPhase()
        {
            var field = _world.GetType().GetField("_broadPhase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_world) as BroadPhase ?? new SpatialHashBroadPhase();
        }
    }

    public class SweepTest
    {
        private readonly PhysicsWorld _world;

        public SweepTest(PhysicsWorld world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public SweepResult SweepCircle(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask = -1)
        {
            Vector2 endPoint = origin + direction * distance;
            Ray ray = new Ray(origin, direction, distance);

            float closestDistance = float.MaxValue;
            RaycastHit2D closestHit = RaycastHit2D.NoHit;

            var query = new PhysicsQuery(_world);
            var hits = query.RaycastAll(ray, layerMask);

            foreach (var hit in hits)
            {
                if (hit.IsHit && hit.Distance < closestDistance)
                {
                    closestDistance = hit.Distance;
                    closestHit = hit;
                }
            }

            return new SweepResult
            {
                IsHit = closestHit.IsHit,
                Hit = closestHit,
                TimeOfImpact = closestHit.IsHit ? closestHit.Distance / distance : 1f,
                Distance = closestHit.Distance
            };
        }

        public SweepResult SweepBox(Vector2 center, Vector2 halfExtents, float rotation, Vector2 direction, float distance, int layerMask = -1)
        {
            Vector2 endCenter = center + direction * distance;
            Ray ray = new Ray(center, direction, distance);

            float closestDistance = float.MaxValue;
            RaycastHit2D closestHit = RaycastHit2D.NoHit;

            var query = new PhysicsQuery(_world);
            var hits = query.RaycastAll(ray, layerMask);

            foreach (var hit in hits)
            {
                if (hit.IsHit && hit.Distance < closestDistance)
                {
                    closestDistance = hit.Distance;
                    closestHit = hit;
                }
            }

            return new SweepResult
            {
                IsHit = closestHit.IsHit,
                Hit = closestHit,
                TimeOfImpact = closestHit.IsHit ? closestHit.Distance / distance : 1f,
                Distance = closestDistance
            };
        }

        public SweepResult SweepCapsule(Vector2 start, Vector2 end, float radius, Vector2 direction, float distance, int layerMask = -1)
        {
            Vector2 endStart = start + direction * distance;
            Vector2 endEnd = end + direction * distance;

            Ray ray = new Ray(start, direction, distance);

            float closestDistance = float.MaxValue;
            RaycastHit2D closestHit = RaycastHit2D.NoHit;

            var query = new PhysicsQuery(_world);
            var hits = query.RaycastAll(ray, layerMask);

            foreach (var hit in hits)
            {
                if (hit.IsHit && hit.Distance < closestDistance)
                {
                    closestDistance = hit.Distance;
                    closestHit = hit;
                }
            }

            return new SweepResult
            {
                IsHit = closestHit.IsHit,
                Hit = closestHit,
                TimeOfImpact = closestHit.IsHit ? closestHit.Distance / distance : 1f,
                Distance = closestDistance
            };
        }

        public List<SweepResult> SweepAll(Vector2 origin, Vector2 direction, float distance, int layerMask = -1)
        {
            var results = new List<SweepResult>();
            var query = new PhysicsQuery(_world);
            var hits = query.RaycastAll(new Ray(origin, direction, distance), layerMask);

            foreach (var hit in hits)
            {
                results.Add(new SweepResult
                {
                    IsHit = hit.IsHit,
                    Hit = hit,
                    TimeOfImpact = hit.IsHit ? hit.Distance / distance : 1f,
                    Distance = hit.Distance
                });
            }

            return results;
        }
    }

    public struct SweepResult
    {
        public bool IsHit { get; set; }
        public RaycastHit2D Hit { get; set; }
        public float TimeOfImpact { get; set; }
        public float Distance { get; set; }

        public static SweepResult NoHit => new SweepResult
        {
            IsHit = false,
            Hit = RaycastHit2D.NoHit,
            TimeOfImpact = 1f,
            Distance = float.MaxValue
        };
    }

    public class ContactManifold2D
    {
        public Collider A { get; set; }
        public Collider B { get; set; }
        public Vector2 Normal { get; set; }
        public float Penetration { get; set; }
        public List<ContactPoint2D> Contacts { get; set; }
        public float Friction { get; set; }
        public float Restitution { get; set; }

        public ContactManifold2D()
        {
            Contacts = new List<ContactPoint2D>();
        }

        public ContactManifold2D(Collider a, Collider b) : this()
        {
            A = a;
            B = b;
            Friction = (float)Math.Sqrt(a.Friction * b.Friction);
            Restitution = Math.Min(a.Restitution, b.Restitution);
        }

        public void AddContact(Vector2 position, Vector2 normal, float penetration)
        {
            Contacts.Add(new ContactPoint2D
            {
                Position = position,
                Normal = normal,
                Penetration = penetration
            });
        }

        public void Clear()
        {
            Contacts.Clear();
            Normal = Vector2.Zero;
            Penetration = 0f;
        }
    }

    public struct ContactPoint2D
    {
        public Vector2 Position { get; set; }
        public Vector2 Normal { get; set; }
        public float Penetration { get; set; }
        public float NormalImpulse { get; set; }
        public float TangentImpulse { get; set; }
        public Vector2 rA { get; set; }
        public Vector2 rB { get; set; }

        public Vector2 GetRelativeVelocity(RigidBody bodyA, RigidBody bodyB)
        {
            if (bodyA == null || bodyB == null)
                return Vector2.Zero;

            Vector2 velocityA = bodyA.Velocity + new Vector2(-rA.Y, rA.X) * bodyA.AngularVelocity;
            Vector2 velocityB = bodyB.Velocity + new Vector2(-rB.Y, rB.X) * bodyB.AngularVelocity;
            return velocityB - velocityA;
        }
    }

    public static class SensorDebugDraw
    {
        public static List<DebugLine> GetSensorLines(Sensor sensor, int segments = 32)
        {
            var lines = new List<DebugLine>();

            if (sensor is CircleSensor circle)
            {
                Vector2[] vertices = circle.GetVertices();
                for (int i = 0; i < vertices.Length; i++)
                {
                    lines.Add(new DebugLine(vertices[i], vertices[(i + 1) % vertices.Length], Color.Cyan));
                }
            }
            else if (sensor is BoxSensor box)
            {
                Vector2[] vertices = box.GetVertices();
                for (int i = 0; i < vertices.Length; i++)
                {
                    lines.Add(new DebugLine(vertices[i], vertices[(i + 1) % vertices.Length], Color.Cyan));
                }
            }
            else if (sensor is CapsuleSensor capsule)
            {
                Vector2[] vertices = capsule.GetVertices();
                for (int i = 0; i < vertices.Length; i++)
                {
                    lines.Add(new DebugLine(vertices[i], vertices[(i + 1) % vertices.Length], Color.Cyan));
                }
            }
            else if (sensor is PolygonSensor polygon)
            {
                Vector2[] vertices = polygon.GetVertices();
                for (int i = 0; i < vertices.Length; i++)
                {
                    lines.Add(new DebugLine(vertices[i], vertices[(i + 1) % vertices.Length], Color.Cyan));
                }
            }

            return lines;
        }

        public static List<DebugPoint> GetSensorPoints(Sensor sensor)
        {
            var points = new List<DebugPoint>();

            Transform2D transform = sensor.GetWorldTransform();
            points.Add(new DebugPoint(transform.Position, Color.Yellow, 4f));

            foreach (var collider in sensor.DetectedColliders)
            {
                if (collider.Body != null)
                {
                    points.Add(new DebugPoint(collider.Body.Position, Color.Red, 3f));
                }
            }

            return points;
        }

        public static List<DebugCircle> GetSensorCircles(CircleSensor sensor)
        {
            var circles = new List<DebugCircle>();

            Transform2D transform = sensor.GetWorldTransform();
            circles.Add(new DebugCircle(transform.Position, sensor.Radius, Color.Cyan));

            return circles;
        }
    }

    public static class RayDebugDraw
    {
        public static DebugLine GetRayLine(Ray ray, Color color)
        {
            return new DebugLine(ray.Origin, ray.GetEndPoint(), color);
        }

        public static DebugLine GetRayHitLine(Ray ray, RaycastHit2D hit, Color rayColor, Color hitColor)
        {
            return new DebugLine(ray.Origin, hit.Point, rayColor);
        }

        public static List<DebugLine> GetRayHitNormalLines(Ray ray, RaycastHit2D hit, float normalLength = 0.5f, Color normalColor = default)
        {
            var lines = new List<DebugLine>();

            if (hit.IsHit)
            {
                Vector2 normalEnd = hit.Point + hit.Normal * normalLength;
                lines.Add(new DebugLine(hit.Point, normalEnd, normalColor == default ? Color.Green : normalColor));
            }

            return lines;
        }

        public static List<DebugPoint> GetHitPoints(List<RaycastHit2D> hits, Color color)
        {
            var points = new List<DebugPoint>();

            foreach (var hit in hits)
            {
                if (hit.IsHit)
                {
                    points.Add(new DebugPoint(hit.Point, color, 5f));
                }
            }

            return points;
        }
    }

    public static class QueryDebugDraw
    {
        public static DebugLine GetOverlapRegionBox(Vector2 center, Vector2 halfExtents, float rotation, Color color)
        {
            Vector2[] localVertices = new Vector2[]
            {
                new Vector2(-halfExtents.X, -halfExtents.Y),
                new Vector2(halfExtents.X, -halfExtents.Y),
                new Vector2(halfExtents.X, halfExtents.Y),
                new Vector2(-halfExtents.X, halfExtents.Y)
            };

            Transform2D transform = new Transform2D(center, rotation);
            Vector2[] worldVertices = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                worldVertices[i] = transform.TransformPoint(localVertices[i]);
            }

            var lines = new List<DebugLine>();
            for (int i = 0; i < 4; i++)
            {
                lines.Add(new DebugLine(worldVertices[i], worldVertices[(i + 1) % 4], color));
            }

            return lines.Count > 0 ? lines[0] : new DebugLine(Vector2.Zero, Vector2.Zero, Color.White);
        }

        public static DebugCircle GetOverlapRegionCircle(Vector2 center, float radius, Color color)
        {
            return new DebugCircle(center, radius, color);
        }

        public static List<DebugPoint> GetQueryResultPoints(List<Collider> colliders, Color color)
        {
            var points = new List<DebugPoint>();

            foreach (var collider in colliders)
            {
                if (collider.Body != null)
                {
                    points.Add(new DebugPoint(collider.Body.Position, color, 4f));
                }
            }

            return points;
        }

        public static List<DebugLine> GetQueryResultConnections(Vector2 queryCenter, List<Collider> colliders, Color color)
        {
            var lines = new List<DebugLine>();

            foreach (var collider in colliders)
            {
                if (collider.Body != null)
                {
                    lines.Add(new DebugLine(queryCenter, collider.Body.Position, color));
                }
            }

            return lines;
        }
    }

    public struct DebugLine
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }
        public Color Color { get; set; }

        public DebugLine(Vector2 start, Vector2 end, Color color)
        {
            Start = start;
            End = end;
            Color = color;
        }
    }

    public struct DebugPoint
    {
        public Vector2 Position { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }

        public DebugPoint(Vector2 position, Color color, float size)
        {
            Position = position;
            Color = color;
            Size = size;
        }
    }

    public struct DebugCircle
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }
        public Color Color { get; set; }

        public DebugCircle(Vector2 center, float radius, Color color)
        {
            Center = center;
            Radius = radius;
            Color = color;
        }
    }

    public class SpatialQuery
    {
        private readonly Dictionary<int, HashSet<Collider>> _spatialCells;
        private readonly float _cellSize;
        private readonly int _gridWidth;
        private readonly int _gridHeight;

        public SpatialQuery(float worldSize = 1000f, float cellSize = 50f)
        {
            _cellSize = cellSize;
            _gridWidth = (int)Math.Ceiling(worldSize / cellSize);
            _gridHeight = (int)Math.Ceiling(worldSize / cellSize);
            _spatialCells = new Dictionary<int, HashSet<Collider>>();
        }

        public void Clear()
        {
            _spatialCells.Clear();
        }

        public void UpdateColliders(IEnumerable<Collider> colliders)
        {
            Clear();

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
                        if (!_spatialCells.TryGetValue(hash, out var cell))
                        {
                            cell = new HashSet<Collider>();
                            _spatialCells[hash] = cell;
                        }
                        cell.Add(collider);
                    }
                }
            }
        }

        public List<Collider> QueryPoint(Vector2 point)
        {
            var results = new List<Collider>();
            var cell = GetCell(point);
            int hash = HashCell(cell.X, cell.Y);

            if (_spatialCells.TryGetValue(hash, out var colliders))
            {
                foreach (var collider in colliders)
                {
                    if (collider.Shape == null)
                        continue;

                    Transform2D transform = collider.Body != null
                        ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                        : Transform2D.Identity;

                    if (collider.Shape.ContainsPoint(point, transform))
                    {
                        results.Add(collider);
                    }
                }
            }

            return results;
        }

        public List<Collider> QueryAABB(AABB aabb)
        {
            var results = new HashSet<Collider>();
            var minCell = GetCell(aabb.Min);
            var maxCell = GetCell(aabb.Max);

            for (int x = minCell.X; x <= maxCell.X; x++)
            {
                for (int y = minCell.Y; y <= maxCell.Y; y++)
                {
                    int hash = HashCell(x, y);
                    if (_spatialCells.TryGetValue(hash, out var cell))
                    {
                        foreach (var collider in cell)
                        {
                            if (collider.Shape == null)
                                continue;

                            Transform2D transform = collider.Body != null
                                ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                                : Transform2D.Identity;

                            if (collider.Shape.GetAABB(transform).Intersects(aabb))
                            {
                                results.Add(collider);
                            }
                        }
                    }
                }
            }

            return results.ToList();
        }

        public List<Collider> QueryCircle(Vector2 center, float radius)
        {
            var results = new List<Collider>();
            var aabb = AABB.CreateFromCircle(center, radius);
            var potentialColliders = QueryAABB(aabb);

            foreach (var collider in potentialColliders)
            {
                if (collider.Shape == null)
                    continue;

                Transform2D transform = collider.Body != null
                    ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                    : Transform2D.Identity;

                if (collider.Shape is CircleShape circle)
                {
                    Vector2 circleCenter = transform.TransformPoint(circle.Center);
                    float dist = center.Distance(circleCenter);
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

        public List<Collider> QueryLayer(int layer)
        {
            var results = new List<Collider>();

            foreach (var cell in _spatialCells.Values)
            {
                foreach (var collider in cell)
                {
                    if ((collider.Layer & (1 << layer)) != 0)
                    {
                        results.Add(collider);
                    }
                }
            }

            return results;
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
            int maxCells = _gridWidth * _gridHeight;
            return (hash1 ^ hash2) % maxCells;
        }
    }

    public class ContinuousCollisionDetection
    {
        private readonly PhysicsWorld _world;

        public ContinuousCollisionDetection(PhysicsWorld world)
        {
            _world = world;
        }

        public bool TestCollision(RigidBody body, Vector2 displacement, float angularDisplacement, out RaycastHit2D hit)
        {
            hit = RaycastHit2D.NoHit;

            if (body.Collider == null || body.Collider.Shape == null)
                return false;

            Transform2D startTransform = new Transform2D(body.Position, body.Rotation);
            Vector2 endPosition = body.Position + displacement;
            float endRotation = body.Rotation + angularDisplacement;
            Transform2D endTransform = new Transform2D(endPosition, endRotation);

            AABB startAABB = body.Collider.Shape.GetAABB(startTransform);
            AABB endAABB = body.Collider.Shape.GetAABB(endTransform);
            AABB sweptAABB = AABB.CreateMerged(startAABB, endAABB);

            var query = new PhysicsQuery(_world);
            var potentialColliders = query.QueryRegion(sweptAABB);

            float closestTime = 1f;
            bool foundCollision = false;

            foreach (var collider in potentialColliders)
            {
                if (collider == body.Collider)
                    continue;

                if ((body.CollisionMask & collider.Layer) == 0)
                    continue;

                if (TestBodyVsCollider(body, displacement, angularDisplacement, collider, out float time, out RaycastHit2D tempHit))
                {
                    if (time < closestTime)
                    {
                        closestTime = time;
                        hit = tempHit;
                        foundCollision = true;
                    }
                }
            }

            return foundCollision;
        }

        private bool TestBodyVsCollider(RigidBody body, Vector2 displacement, float angularDisplacement, Collider other, out float timeOfImpact, out RaycastHit2D hit)
        {
            timeOfImpact = 1f;
            hit = RaycastHit2D.NoHit;

            if (other.Shape == null)
                return false;

            Vector2 startPos = body.Position;
            Vector2 endPos = body.Position + displacement;
            Vector2 direction = endPos - startPos;
            float distance = direction.Length();
            direction = distance > 0 ? direction / distance : Vector2.Right;

            Ray ray = new Ray(startPos, direction, distance);
            var query = new PhysicsQuery(_world);
            var rayHit = query.RaycastSingle(ray, other.Layer);

            if (rayHit.IsHit && rayHit.Collider == other)
            {
                timeOfImpact = rayHit.Fraction;
                hit = rayHit;
                return true;
            }

            return false;
        }

        public Vector2? GetTimeOfImpact(RigidBody body, Vector2 velocity, float angularVelocity, float maxTime)
        {
            float time = 0f;
            float dt = maxTime / 10f;

            Vector2 currentPos = body.Position;
            float currentRot = body.Rotation;

            for (int i = 0; i < 10; i++)
            {
                Vector2 displacement = velocity * dt;
                float angDisp = angularVelocity * dt;

                if (TestCollision(body, displacement, angDisp, out RaycastHit2D hit))
                {
                    return body.Position + velocity * time;
                }

                currentPos += displacement;
                currentRot += angDisp;
                time += dt;
            }

            return null;
        }
    }

    public class CollisionLayerMatrix
    {
        private readonly bool[,] _matrix;
        private readonly int _size;

        public CollisionLayerMatrix(int size = 32)
        {
            _size = size;
            _matrix = new bool[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    _matrix[i, j] = true;
                }
            }
        }

        public void SetCollision(int layer1, int layer2, bool enabled)
        {
            if (layer1 < 0 || layer1 >= _size || layer2 < 0 || layer2 >= _size)
                return;

            _matrix[layer1, layer2] = enabled;
            _matrix[layer2, layer1] = enabled;
        }

        public bool GetCollision(int layer1, int layer2)
        {
            if (layer1 < 0 || layer1 >= _size || layer2 < 0 || layer2 >= _size)
                return false;

            return _matrix[layer1, layer2];
        }

        public void EnableAllCollisions()
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    _matrix[i, j] = true;
                }
            }
        }

        public void DisableAllCollisions()
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    _matrix[i, j] = false;
                }
            }
        }

        public int[] GetCollidingLayers(int layer)
        {
            if (layer < 0 || layer >= _size)
                return Array.Empty<int>();

            var colliding = new List<int>();
            for (int i = 0; i < _size; i++)
            {
                if (_matrix[layer, i])
                    colliding.Add(i);
            }
            return colliding.ToArray();
        }
    }

    public class ShapeCast2D
    {
        private readonly PhysicsWorld _world;

        public ShapeCast2D(PhysicsWorld world)
        {
            _world = world;
        }

        public ShapeCastResult Cast(IShape shape, Transform2D startTransform, Transform2D endTransform, int layerMask = -1)
        {
            Vector2 displacement = endTransform.Position - startTransform.Position;
            float rotation = endTransform.Rotation - startTransform.Rotation;
            float distance = displacement.Length();

            if (distance < MathHelper.Epsilon)
            {
                return new ShapeCastResult { IsHit = false, TimeOfImpact = 1f };
            }

            Vector2 direction = displacement / distance;
            AABB startAABB = shape.GetAABB(startTransform);
            AABB endAABB = shape.GetAABB(endTransform);
            AABB sweptAABB = AABB.CreateMerged(startAABB, endAABB);

            var query = new PhysicsQuery(_world);
            var potentialColliders = query.QueryRegion(sweptAABB, layerMask);

            float closestTime = 1f;
            RaycastHit2D closestHit = RaycastHit2D.NoHit;

            foreach (var collider in potentialColliders)
            {
                if (collider.Shape == null)
                    continue;

                if (TestShapeVsCollider(shape, startTransform, direction, distance, rotation, collider, out float time, out RaycastHit2D hit))
                {
                    if (time < closestTime)
                    {
                        closestTime = time;
                        closestHit = hit;
                    }
                }
            }

            return new ShapeCastResult
            {
                IsHit = closestHit.IsHit,
                TimeOfImpact = closestTime,
                Hit = closestHit,
                FinalTransform = new Transform2D(
                    Vector2.Lerp(startTransform.Position, endTransform.Position, closestTime),
                    startTransform.Rotation + rotation * closestTime
                )
            };
        }

        private bool TestShapeVsCollider(IShape shape, Transform2D startTransform, Vector2 direction, float distance, float rotation, Collider collider, out float time, out RaycastHit2D hit)
        {
            time = 1f;
            hit = RaycastHit2D.NoHit;

            if (shape is CircleShape circle)
            {
                return TestCircleVsCollider(circle, startTransform, direction, distance, collider, out time, out hit);
            }
            else if (shape is BoxShape box)
            {
                return TestBoxVsCollider(box, startTransform, direction, distance, collider, out time, out hit);
            }
            else if (shape is PolygonShape polygon)
            {
                return TestPolygonVsCollider(polygon, startTransform, direction, distance, collider, out time, out hit);
            }

            return false;
        }

        private bool TestCircleVsCollider(CircleShape circle, Transform2D transform, Vector2 direction, float distance, Collider collider, out float time, out RaycastHit2D hit)
        {
            time = 1f;
            hit = RaycastHit2D.NoHit;

            Transform2D colliderTransform = collider.Body != null
                ? new Transform2D(collider.Body.Position, collider.Body.Rotation)
                : Transform2D.Identity;

            if (collider.Shape is CircleShape otherCircle)
            {
                Vector2 center1 = transform.TransformPoint(circle.Center);
                Vector2 center2 = colliderTransform.TransformPoint(otherCircle.Center);
                float combinedRadius = circle.Radius + otherCircle.Radius;

                Vector2 toCenter = center2 - center1;
                float proj = toCenter.Dot(direction);
                float closest = proj;

                closest = Math.Clamp(closest, 0f, distance);

                Vector2 closestPoint = center1 + direction * closest;
                float dist = (center2 - closestPoint).Length();

                if (dist <= combinedRadius)
                {
                    time = closest / distance;
                    hit = new RaycastHit2D
                    {
                        IsHit = true,
                        Point = closestPoint,
                        Normal = (center2 - closestPoint).Normalized(),
                        Distance = closest,
                        Collider = collider,
                        Body = collider.Body,
                        Fraction = time
                    };
                    return true;
                }
            }

            return false;
        }

        private bool TestBoxVsCollider(BoxShape box, Transform2D transform, Vector2 direction, float distance, Collider collider, out float time, out RaycastHit2D hit)
        {
            time = 1f;
            hit = RaycastHit2D.NoHit;

            Ray ray = new Ray(transform.Position, direction, distance);
            var query = new PhysicsQuery(_world);
            var rayHit = query.RaycastSingle(ray);

            if (rayHit.IsHit && rayHit.Collider == collider)
            {
                time = rayHit.Fraction;
                hit = rayHit;
                return true;
            }

            return false;
        }

        private bool TestPolygonVsCollider(PolygonShape polygon, Transform2D transform, Vector2 direction, float distance, Collider collider, out float time, out RaycastHit2D hit)
        {
            time = 1f;
            hit = RaycastHit2D.NoHit;

            Ray ray = new Ray(transform.Position, direction, distance);
            var query = new PhysicsQuery(_world);
            var rayHit = query.RaycastSingle(ray);

            if (rayHit.IsHit && rayHit.Collider == collider)
            {
                time = rayHit.Fraction;
                hit = rayHit;
                return true;
            }

            return false;
        }
    }

    public struct ShapeCastResult
    {
        public bool IsHit { get; set; }
        public float TimeOfImpact { get; set; }
        public RaycastHit2D Hit { get; set; }
        public Transform2D FinalTransform { get; set; }

        public static ShapeCastResult NoHit => new ShapeCastResult
        {
            IsHit = false,
            TimeOfImpact = 1f,
            Hit = RaycastHit2D.NoHit,
            FinalTransform = Transform2D.Identity
        };
    }

    public class ContactCache
    {
        private readonly Dictionary<(Collider, Collider), ContactManifold2D> _contacts;
        private readonly float _lifeTime;
        private readonly Dictionary<(Collider, Collider), float> _contactAges;

        public ContactCache(float lifeTime = 1f)
        {
            _contacts = new Dictionary<(Collider, Collider), ContactManifold2D>();
            _contactAges = new Dictionary<(Collider, Collider), float>();
            _lifeTime = lifeTime;
        }

        public ContactManifold2D? GetContact(Collider a, Collider b)
        {
            var key = CreateKey(a, b);
            if (_contacts.TryGetValue(key, out var contact))
            {
                return contact;
            }
            return null;
        }

        public void AddContact(ContactManifold2D contact)
        {
            var key = CreateKey(contact.A, contact.B);
            _contacts[key] = contact;
            _contactAges[key] = 0f;
        }

        public void Update(float deltaTime)
        {
            var keysToRemove = new List<(Collider, Collider)>();

            foreach (var kvp in _contactAges)
            {
                kvp.Value += deltaTime;
                if (kvp.Value > _lifeTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _contacts.Remove(key);
                _contactAges.Remove(key);
            }
        }

        public void Clear()
        {
            _contacts.Clear();
            _contactAges.Clear();
        }

        private (Collider, Collider) CreateKey(Collider a, Collider b)
        {
            return a.GetHashCode() < b.GetHashCode() ? (a, b) : (b, a);
        }
    }

    public class SensorManager
    {
        private readonly List<Sensor> _sensors;
        private readonly PhysicsWorld _world;
        private readonly SpatialQuery _spatialQuery;

        public IReadOnlyList<Sensor> Sensors => _sensors;

        public SensorManager(PhysicsWorld world)
        {
            _world = world;
            _sensors = new List<Sensor>();
            _spatialQuery = new SpatialQuery();
        }

        public T CreateSensor<T>(T sensor) where T : Sensor
        {
            _sensors.Add(sensor);
            return sensor;
        }

        public void RemoveSensor(Sensor sensor)
        {
            _sensors.Remove(sensor);
        }

        public void Update(float deltaTime)
        {
            _spatialQuery.UpdateColliders(_world.Colliders);

            foreach (var sensor in _sensors)
            {
                AABB aabb = sensor.GetAABB();
                var nearbyColliders = _spatialQuery.QueryAABB(aabb);
                sensor.ProcessDetection(nearbyColliders, _world);
            }
        }

        public List<Sensor> GetSensorsAtPoint(Vector2 point)
        {
            var result = new List<Sensor>();
            foreach (var sensor in _sensors)
            {
                if (sensor.ContainsPoint(point))
                {
                    result.Add(sensor);
                }
            }
            return result;
        }

        public List<Sensor> GetSensorsOverlappingCollider(Collider collider)
        {
            var result = new List<Sensor>();
            foreach (var sensor in _sensors)
            {
                if (sensor.DetectedColliders.Contains(collider))
                {
                    result.Add(sensor);
                }
            }
            return result;
        }

        public void ClearAll()
        {
            foreach (var sensor in _sensors)
            {
                sensor.Reset();
            }
        }
    }
}

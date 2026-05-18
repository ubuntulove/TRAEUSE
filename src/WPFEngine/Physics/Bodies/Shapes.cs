using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Physics.Bodies
{
    public enum BodyType
    {
        Static,
        Kinematic,
        Dynamic
    }

    public enum ShapeType
    {
        None,
        Circle,
        Box,
        Polygon,
        Capsule
    }

    public interface IShape
    {
        ShapeType Type { get; }
        Vector2 Centroid { get; }
        float Area { get; }
        float MomentOfInertia(float mass);
        bool ContainsPoint(Vector2 point, Transform2D transform);
        AABB GetAABB(Transform2D transform);
        Vector2[] GetVertices(Transform2D transform);
        IShape Clone();
    }

    public struct Transform2D
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Scale;

        public static Transform2D Identity => new Transform2D
        {
            Position = Vector2.Zero,
            Rotation = 0f,
            Scale = Vector2.One
        };

        public Transform2D(Vector2 position, float rotation = 0f, Vector2? scale = null)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale ?? Vector2.One;
        }

        public Matrix3x3 ToMatrix()
        {
            return Matrix3x3.Translation(Position) *
                   Matrix3x3.Rotation(Rotation) *
                   Matrix3x3.Scale(Scale);
        }

        public Vector2 TransformPoint(Vector2 point)
        {
            return ToMatrix().Transform(point);
        }

        public Vector2 InverseTransformPoint(Vector2 point)
        {
            return ToMatrix().Inverse().Transform(point);
        }

        public Vector2 TransformDirection(Vector2 direction)
        {
            return Matrix3x3.Rotation(Rotation).Transform(direction);
        }

        public static Transform2D operator *(Transform2D a, Transform2D b)
        {
            return new Transform2D
            {
                Position = a.TransformPoint(b.Position),
                Rotation = a.Rotation + b.Rotation,
                Scale = a.Scale * b.Scale
            };
        }
    }

    public struct AABB : IEquatable<AABB>
    {
        public Vector2 Min;
        public Vector2 Max;

        public Vector2 Center => (Min + Max) * 0.5f;
        public Vector2 Size => Max - Min;
        public float Width => Max.X - Min.X;
        public float Height => Max.Y - Min.Y;
        public float Perimeter => 2f * (Width + Height);
        public float Area => Width * Height;

        public AABB(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public AABB(Vector2 center, float halfWidth, float halfHeight)
        {
            Min = center - new Vector2(halfWidth, halfHeight);
            Max = center + new Vector2(halfWidth, halfHeight);
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y;
        }

        public bool Contains(AABB other)
        {
            return Contains(other.Min) && Contains(other.Max);
        }

        public bool Intersects(AABB other)
        {
            return !(other.Min.X > Max.X || other.Max.X < Min.X ||
                     other.Min.Y > Max.Y || other.Max.Y < Min.Y);
        }

        public bool IntersectsCircle(Vector2 center, float radius)
        {
            float x = Math.Max(Min.X, Math.Min(center.X, Max.X));
            float y = Math.Max(Min.Y, Math.Min(center.Y, Max.Y));
            float dx = center.X - x;
            float dy = center.Y - y;
            return dx * dx + dy * dy < radius * radius;
        }

        public AABB Union(AABB other)
        {
            return new AABB(
                new Vector2(Math.Min(Min.X, other.Min.X), Math.Min(Min.Y, other.Min.Y)),
                new Vector2(Math.Max(Max.X, other.Max.X), Math.Max(Max.Y, other.Max.Y))
            );
        }

        public AABB Expand(float amount)
        {
            return new AABB(Min - new Vector2(amount), Max + new Vector2(amount));
        }

        public AABB Translate(Vector2 offset)
        {
            return new AABB(Min + offset, Max + offset);
        }

        public float ComputePerimeter()
        {
            return 2f * (Width + Height);
        }

        public float ComputeArea()
        {
            return Width * Height;
        }

        public float ComputeIC()
        {
            float w = Width;
            float h = Height;
            return (w * h * (w * w + h * h)) / 12f;
        }

        public static AABB FromVertices(Vector2[] vertices)
        {
            if (vertices == null || vertices.Length == 0)
                return new AABB(Vector2.Zero, Vector2.Zero);

            Vector2 min = vertices[0];
            Vector2 max = vertices[0];

            for (int i = 1; i < vertices.Length; i++)
            {
                min = Vector2.Min(min, vertices[i]);
                max = Vector2.Max(max, vertices[i]);
            }

            return new AABB(min, max);
        }

        public static AABB CreateMerged(AABB a, AABB b)
        {
            return new AABB(
                new Vector2(Math.Min(a.Min.X, b.Min.X), Math.Min(a.Min.Y, b.Min.Y)),
                new Vector2(Math.Max(a.Max.X, b.Max.X), Math.Max(a.Max.Y, b.Max.Y))
            );
        }

        public static AABB CreateFromCircle(Vector2 center, float radius)
        {
            return new AABB(
                center - new Vector2(radius),
                center + new Vector2(radius)
            );
        }

        public static bool Raycast(AABB box, Vector2 origin, Vector2 direction, out float tMin, out Vector2 normal)
        {
            tMin = 0f;
            normal = Vector2.Zero;

            float tMax = float.MaxValue;

            if (Math.Abs(direction.X) < MathHelper.Epsilon)
            {
                if (origin.X < box.Min.X || origin.X > box.Max.X)
                    return false;
            }
            else
            {
                float invD = 1f / direction.X;
                float t1 = (box.Min.X - origin.X) * invD;
                float t2 = (box.Max.X - origin.X) * invD;

                normal = new Vector2(-1f, 0f);
                if (t1 > t2)
                {
                    MathHelper.Swap(ref t1, ref t2);
                    normal = new Vector2(1f, 0f);
                }

                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);

                if (tMin > tMax)
                    return false;
            }

            if (Math.Abs(direction.Y) < MathHelper.Epsilon)
            {
                if (origin.Y < box.Min.Y || origin.Y > box.Max.Y)
                    return false;
            }
            else
            {
                float invD = 1f / direction.Y;
                float t1 = (box.Min.Y - origin.Y) * invD;
                float t2 = (box.Max.Y - origin.Y) * invD;

                Vector2 n = new Vector2(0f, -1f);
                if (t1 > t2)
                {
                    MathHelper.Swap(ref t1, ref t2);
                    n = new Vector2(0f, 1f);
                }

                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                normal = t1 > tMin ? n : normal;

                if (tMin > tMax)
                    return false;
            }

            if (tMin < 0f)
                return false;

            normal = normal.Normalized();
            return true;
        }

        public override bool Equals(object? obj) => obj is AABB other && Equals(other);
        public bool Equals(AABB other) => Min == other.Min && Max == other.Max;
        public override int GetHashCode() => HashCode.Combine(Min, Max);
        public override string ToString() => $"AABB(Center: {Center}, Size: {Size})";

        public static bool operator ==(AABB a, AABB b) => a.Equals(b);
        public static bool operator !=(AABB a, AABB b) => !a.Equals(b);
    }

    public class CircleShape : IShape
    {
        public float Radius { get; set; }
        public Vector2 Center { get; set; }

        ShapeType IShape.Type => ShapeType.Circle;
        Vector2 IShape.Centroid => Center;
        float IShape.Area => MathHelper.Pi * Radius * Radius;

        public CircleShape(float radius, Vector2? center = null)
        {
            Radius = radius;
            Center = center ?? Vector2.Zero;
        }

        public float MomentOfInertia(float mass)
        {
            return 0.5f * mass * Radius * Radius;
        }

        public bool ContainsPoint(Vector2 point, Transform2D transform)
        {
            Vector2 localPoint = transform.InverseTransformPoint(point);
            return localPoint.Distance(Center) <= Radius;
        }

        public AABB GetAABB(Transform2D transform)
        {
            Vector2 worldCenter = transform.TransformPoint(Center);
            Vector2 scaledRadius = transform.Scale * Radius;
            float maxRadius = Math.Max(scaledRadius.X, scaledRadius.Y);
            return AABB.CreateFromCircle(worldCenter, maxRadius);
        }

        public Vector2[] GetVertices(Transform2D transform)
        {
            int segments = 16;
            Vector2[] vertices = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * MathHelper.TwoPi;
                Vector2 localVertex = Center + new Vector2(MathHelper.Cos(angle), MathHelper.Sin(angle)) * Radius;
                vertices[i] = transform.TransformPoint(localVertex);
            }
            return vertices;
        }

        public IShape Clone() => new CircleShape(Radius, Center);
    }

    public class BoxShape : IShape
    {
        public Vector2 HalfExtents { get; set; }

        ShapeType IShape.Type => ShapeType.Box;
        Vector2 IShape.Centroid => Vector2.Zero;
        float IShape.Area => 4f * HalfExtents.X * HalfExtents.Y;

        public BoxShape(Vector2 halfExtents)
        {
            HalfExtents = halfExtents;
        }

        public BoxShape(float halfWidth, float halfHeight)
        {
            HalfExtents = new Vector2(halfWidth, halfHeight);
        }

        public float MomentOfInertia(float mass)
        {
            return mass * (HalfExtents.X * HalfExtents.X + HalfExtents.Y * HalfExtents.Y) / 12f;
        }

        public bool ContainsPoint(Vector2 point, Transform2D transform)
        {
            Vector2 localPoint = transform.InverseTransformPoint(point);
            return Math.Abs(localPoint.X) <= HalfExtents.X && Math.Abs(localPoint.Y) <= HalfExtents.Y;
        }

        public AABB GetAABB(Transform2D transform)
        {
            Vector2[] vertices = ((IShape)this).GetVertices(transform);
            return AABB.FromVertices(vertices);
        }

        public Vector2[] GetVertices(Transform2D transform)
        {
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

        public IShape Clone() => new BoxShape(HalfExtents);
    }

    public class PolygonShape : IShape
    {
        private Vector2[] _vertices;
        private Vector2 _centroid;
        private float _area;
        private float _momentOfInertia;

        public Vector2[] Vertices
        {
            get => _vertices;
            set
            {
                _vertices = value;
                ComputeProperties();
            }
        }

        ShapeType IShape.Type => ShapeType.Polygon;
        Vector2 IShape.Centroid => _centroid;
        float IShape.Area => _area;

        public PolygonShape(Vector2[] vertices)
        {
            _vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            ComputeProperties();
        }

        public PolygonShape()
        {
            _vertices = Array.Empty<Vector2>();
            _centroid = Vector2.Zero;
            _area = 0f;
            _momentOfInertia = 0f;
        }

        private void ComputeProperties()
        {
            if (_vertices == null || _vertices.Length < 3)
            {
                _centroid = Vector2.Zero;
                _area = 0f;
                _momentOfInertia = 0f;
                return;
            }

            float signedArea = 0f;
            _centroid = Vector2.Zero;
            float moment = 0f;

            for (int i = 0; i < _vertices.Length; i++)
            {
                Vector2 v1 = _vertices[i];
                Vector2 v2 = _vertices[(i + 1) % _vertices.Length];

                float cross = v1.Cross(v2);
                signedArea += cross;
                _centroid += (v1 + v2) * cross;
                moment += cross * (v1.Dot(v1) + v1.Dot(v2) + v2.Dot(v2));
            }

            signedArea *= 0.5f;
            _area = Math.Abs(signedArea);

            if (Math.Abs(signedArea) > MathHelper.Epsilon)
            {
                _centroid /= (6f * signedArea);
                moment = moment / 12f;
                _momentOfInertia = Math.Abs(moment);
            }
        }

        public static PolygonShape CreateBox(Vector2 halfExtents)
        {
            return new PolygonShape(new Vector2[]
            {
                new Vector2(-halfExtents.X, -halfExtents.Y),
                new Vector2(halfExtents.X, -halfExtents.Y),
                new Vector2(halfExtents.X, halfExtents.Y),
                new Vector2(-halfExtents.X, halfExtents.Y)
            });
        }

        public static PolygonShape CreateRegular(int sides, float radius)
        {
            Vector2[] vertices = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (float)i / sides * MathHelper.TwoPi - MathHelper.PiOver2;
                vertices[i] = new Vector2(MathHelper.Cos(angle), MathHelper.Sin(angle)) * radius;
            }
            return new PolygonShape(vertices);
        }

        public float MomentOfInertia(float mass)
        {
            return _momentOfInertia * mass / _area;
        }

        public bool ContainsPoint(Vector2 point, Transform2D transform)
        {
            Vector2 localPoint = transform.InverseTransformPoint(point);
            int n = _vertices.Length;

            for (int i = 0; i < n; i++)
            {
                Vector2 edge = _vertices[(i + 1) % n] - _vertices[i];
                Vector2 toPoint = localPoint - _vertices[i];
                if (edge.Cross(toPoint) > 0f)
                    return false;
            }

            return true;
        }

        public AABB GetAABB(Transform2D transform)
        {
            return AABB.FromVertices(((IShape)this).GetVertices(transform));
        }

        public Vector2[] GetVertices(Transform2D transform)
        {
            Vector2[] worldVertices = new Vector2[_vertices.Length];
            for (int i = 0; i < _vertices.Length; i++)
            {
                worldVertices[i] = transform.TransformPoint(_vertices[i]);
            }
            return worldVertices;
        }

        public IShape Clone() => new PolygonShape((Vector2[])_vertices.Clone());

        public void ComputeNormals(Vector2[] vertices, out Vector2[] normals)
        {
            int n = vertices.Length;
            normals = new Vector2[n];

            for (int i = 0; i < n; i++)
            {
                Vector2 edge = vertices[(i + 1) % n] - vertices[i];
                normals[i] = new Vector2(-edge.Y, edge.X).Normalized();
            }
        }
    }

    public class CapsuleShape : IShape
    {
        public float Radius { get; set; }
        public float Height { get; set; }

        ShapeType IShape.Type => ShapeType.Capsule;
        Vector2 IShape.Centroid => Vector2.Zero;
        float IShape.Area
        {
            get
            {
                float rectArea = Height * 2f * Radius;
                float circleArea = MathHelper.Pi * Radius * Radius;
                return rectArea + circleArea;
            }
        }

        public CapsuleShape(float radius, float height)
        {
            Radius = radius;
            Height = height;
        }

        public float MomentOfInertia(float mass)
        {
            float cylinderMass = mass * Height / (Height + 2f * Radius);
            float capMass = mass - cylinderMass;

            float cylinderMoI = cylinderMass * (0.25f * Radius * Radius + Height * Height / 12f);
            float capMoI = capMass * (0.5f * Radius * Radius);

            return cylinderMoI + capMoI;
        }

        public bool ContainsPoint(Vector2 point, Transform2D transform)
        {
            Vector2 localPoint = transform.InverseTransformPoint(point);
            float halfHeight = Height * 0.5f;

            if (localPoint.Y > halfHeight)
            {
                return localPoint.Distance(new Vector2(0f, halfHeight)) <= Radius;
            }
            else if (localPoint.Y < -halfHeight)
            {
                return localPoint.Distance(new Vector2(0f, -halfHeight)) <= Radius;
            }
            else
            {
                return Math.Abs(localPoint.X) <= Radius;
            }
        }

        public AABB GetAABB(Transform2D transform)
        {
            float halfHeight = Height * 0.5f;
            float maxRadius = Math.Max(transform.Scale.X, transform.Scale.Y) * Radius;

            Vector2 worldPos = transform.Position;
            Vector2 scaledScale = transform.Scale;

            float halfWidth = scaledScale.X * Radius;
            float halfBoxHeight = scaledScale.Y * halfHeight;

            float topY = worldPos.Y + halfBoxHeight + maxRadius;
            float bottomY = worldPos.Y - halfBoxHeight - maxRadius;

            return new AABB(
                new Vector2(worldPos.X - halfWidth, bottomY),
                new Vector2(worldPos.X + halfWidth, topY)
            );
        }

        public Vector2[] GetVertices(Transform2D transform)
        {
            List<Vector2> vertices = new List<Vector2>();
            int segments = 8;
            float halfHeight = Height * 0.5f;

            for (int i = 0; i <= segments / 2; i++)
            {
                float angle = -MathHelper.PiOver2 + (float)i / (segments / 2) * MathHelper.Pi;
                vertices.Add(transform.TransformPoint(new Vector2(MathHelper.Cos(angle) * Radius, halfHeight + MathHelper.Sin(angle) * Radius)));
            }

            for (int i = segments / 2; i <= segments; i++)
            {
                float angle = MathHelper.PiOver2 + (float)(i - segments / 2) / (segments / 2) * MathHelper.Pi;
                vertices.Add(transform.TransformPoint(new Vector2(MathHelper.Cos(angle) * Radius, -halfHeight + MathHelper.Sin(angle) * Radius)));
            }

            return vertices.ToArray();
        }

        public IShape Clone() => new CapsuleShape(Radius, Height);
    }
}

using System;
using System.Runtime.CompilerServices;

namespace WPFEngine.Math
{
    public struct Vector2 : IEquatable<Vector2>, IFormattable
    {
        public float X;
        public float Y;

        public static readonly Vector2 Zero = new Vector2(0f, 0f);
        public static readonly Vector2 One = new Vector2(1f, 1f);
        public static readonly Vector2 Up = new Vector2(0f, 1f);
        public static readonly Vector2 Down = new Vector2(0f, -1f);
        public static readonly Vector2 Left = new Vector2(-1f, 0f);
        public static readonly Vector2 Right = new Vector2(1f, 0f);

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2(float value)
        {
            X = value;
            Y = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float length = Length();
            if (length > 0f)
            {
                X /= length;
                Y /= length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Normalized()
        {
            Vector2 result = this;
            result.Normalize();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(Vector2 other)
        {
            return X * other.X + Y * other.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cross(Vector2 other)
        {
            return X * other.Y - Y * other.X;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(Vector2 other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceSquared(Vector2 other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            return dx * dx + dy * dy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Lerp(Vector2 other, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2(
                X + (other.X - X) * t,
                Y + (other.Y - Y) * t
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 MoveTowards(Vector2 target, float maxDistanceDelta)
        {
            float dx = target.X - X;
            float dy = target.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist <= maxDistanceDelta || dist < 1e-6f)
                return target;
            return new Vector2(X + dx / dist * maxDistanceDelta, Y + dy / dist * maxDistanceDelta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Rotate(float angleRadians)
        {
            float cos = (float)Math.Cos(angleRadians);
            float sin = (float)Math.Sin(angleRadians);
            return new Vector2(X * cos - Y * sin, X * sin + Y * cos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Perpendicular()
        {
            return new Vector2(-Y, X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Reflect(Vector2 normal)
        {
            float d = 2f * Dot(normal);
            return new Vector2(X - d * normal.X, Y - d * normal.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Project(Vector2 onto)
        {
            float dot = Dot(onto);
            float lenSq = onto.Dot(onto);
            if (lenSq < 1e-10f) return Zero;
            return onto * (dot / lenSq);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Scale(Vector2 scale)
        {
            return new Vector2(X * scale.X, Y * scale.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Angle()
        {
            return (float)Math.Atan2(Y, X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float AngleTo(Vector2 other)
        {
            float cos = Dot(other) / (Length() * other.Length());
            cos = Math.Clamp(cos, -1f, 1f);
            return (float)Math.Acos(cos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Abs()
        {
            return new Vector2(Math.Abs(X), Math.Abs(Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Clamp(Vector2 min, Vector2 max)
        {
            return new Vector2(
                Math.Clamp(X, min.X, max.X),
                Math.Clamp(Y, min.Y, max.Y)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(a.X * b.X, a.Y * b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.X * d, a.Y * d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.X * d, a.Y * d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(Vector2 a, Vector2 b) => new Vector2(a.X / b.X, a.Y / b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.X / d, a.Y / d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector2 a, Vector2 b) => a.X == b.X && a.Y == b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2 a, Vector2 b) => a.X != b.X || a.Y != b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Vector2 a, Vector2 b) => a.X < b.X && a.Y < b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Vector2 a, Vector2 b) => a.X > b.X && a.Y > b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Vector2 a, Vector2 b) => a.X <= b.X && a.Y <= b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Vector2 a, Vector2 b) => a.X >= b.X && a.Y >= b.Y;

        public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2 other) => X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"({X:F2}, {Y:F2})";
        public string ToString(string format) => $"({X.ToString(format)}, {Y.ToString(format)})";
        public string ToString(IFormatProvider provider) => $"({X.ToString(provider)}, {Y.ToString(provider)})";
        public string ToString(string? format, IFormatProvider? provider) =>
            $"({X.ToString(format, provider)}, {Y.ToString(format, provider)})";
    }

    public struct Vector3 : IEquatable<Vector3>, IFormattable
    {
        public float X;
        public float Y;
        public float Z;

        public static readonly Vector3 Zero = new Vector3(0f, 0f, 0f);
        public static readonly Vector3 One = new Vector3(1f, 1f, 1f);
        public static readonly Vector3 Up = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 Down = new Vector3(0f, -1f, 0f);
        public static readonly Vector3 Left = new Vector3(-1f, 0f, 0f);
        public static readonly Vector3 Right = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 Forward = new Vector3(0f, 0f, 1f);
        public static readonly Vector3 Back = new Vector3(0f, 0f, -1f);

        public Vector3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public Vector3(float value)
        {
            X = value; Y = value; Z = value;
        }

        public Vector3(Vector2 value, float z)
        {
            X = value.X; Y = value.Y; Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared() => X * X + Y * Y + Z * Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float len = Length();
            if (len > 0f)
            {
                X /= len; Y /= len; Z /= len;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Normalized()
        {
            Vector3 result = this;
            result.Normalize();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(Vector3 other) => X * other.X + Y * other.Y + Z * other.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Cross(Vector3 other)
        {
            return new Vector3(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(Vector3 other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Lerp(Vector3 other, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector3(
                X + (other.X - X) * t,
                Y + (other.Y - Y) * t,
                Z + (other.Z - Z) * t
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.X * d, a.Y * d, a.Z * d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.X * d, a.Y * d, a.Z * d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.X / d, a.Y / d, a.Z / d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3 a, Vector3 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3 a, Vector3 b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;

        public override bool Equals(object? obj) => obj is Vector3 other && Equals(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }

    public struct Vector4 : IEquatable<Vector4>, IFormattable
    {
        public float X, Y, Z, W;

        public static readonly Vector4 Zero = new Vector4(0f, 0f, 0f, 0f);
        public static readonly Vector4 One = new Vector4(1f, 1f, 1f, 1f);

        public Vector4(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }

        public Vector4(float value)
        {
            X = value; Y = value; Z = value; W = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(Vector4 other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Vector4 a, float d) => new Vector4(a.X * d, a.Y * d, a.Z * d, a.W * d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector4 a, Vector4 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);

        public override bool Equals(object? obj) => obj is Vector4 other && Equals(other);
        public bool Equals(Vector4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2}, {W:F2})";
    }

    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int X, Y;

        public static readonly Vector2Int Zero = new Vector2Int(0, 0);
        public static readonly Vector2Int One = new Vector2Int(1, 1);
        public static readonly Vector2Int Up = new Vector2Int(0, 1);
        public static readonly Vector2Int Down = new Vector2Int(0, -1);
        public static readonly Vector2Int Left = new Vector2Int(-1, 0);
        public static readonly Vector2Int Right = new Vector2Int(1, 0);

        public Vector2Int(int x, int y)
        {
            X = x; Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length() => (float)Math.Sqrt(X * X + Y * Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LengthSquared() => X * X + Y * Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.X + b.X, a.Y + b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.X - b.X, a.Y - b.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator *(Vector2Int a, int d) => new Vector2Int(a.X * d, a.Y * d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator /(Vector2Int a, int d) => new Vector2Int(a.X / d, a.Y / d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2Int a, Vector2Int b) => a.X != b.X || a.Y != b.Y;

        public override bool Equals(object? obj) => obj is Vector2Int other && Equals(other);
        public bool Equals(Vector2Int other) => X == other.X && Y == other.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";

        public Vector2 ToVector2() => new Vector2(X, Y);
    }
}

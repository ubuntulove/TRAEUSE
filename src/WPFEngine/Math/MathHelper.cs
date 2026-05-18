using System;
using System.Runtime.CompilerServices;

namespace WPFEngine.Math
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float X, Y, Z, W;

        public static readonly Quaternion Identity = new Quaternion(0f, 0f, 0f, 1f);

        public Quaternion(float x, float y, float z, float w)
        {
            X = x; Y = y; Z = z; W = w;
        }

        public Quaternion(Vector3 axis, float angle)
        {
            float halfAngle = angle * 0.5f;
            float s = (float)Math.Sin(halfAngle);
            X = axis.X * s;
            Y = axis.Y * s;
            Z = axis.Z * s;
            W = (float)Math.Cos(halfAngle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LengthSquared()
        {
            return X * X + Y * Y + Z * Z + W * W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float len = Length();
            if (len > 1e-6f)
            {
                X /= len; Y /= len; Z /= len; W /= len;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion Normalized()
        {
            Quaternion result = this;
            result.Normalize();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Conjugate()
        {
            X = -X; Y = -Y; Z = -Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion Conjugated()
        {
            return new Quaternion(-X, -Y, -Z, W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inverse()
        {
            float lenSq = LengthSquared();
            if (lenSq > 1e-6f)
            {
                float invLenSq = 1f / lenSq;
                X = -X * invLenSq;
                Y = -Y * invLenSq;
                Z = -Z * invLenSq;
                W = W * invLenSq;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion Inversed()
        {
            Quaternion result = this;
            result.Inverse();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(Quaternion other)
        {
            return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToEulerAngles()
        {
            float sinrCosp = 2f * (W * X + Y * Z);
            float cosrCosp = 1f - 2f * (X * X + Y * Y);
            float roll = (float)Math.Atan2(sinrCosp, cosrCosp);

            float sinp = 2f * (W * Y - Z * X);
            float pitch;
            if (Math.Abs(sinp) >= 1f)
                pitch = Math.Sign(sinp) * MathHelper.PiOver2;
            else
                pitch = (float)Math.Asin(sinp);

            float sinyCosp = 2f * (W * Z + X * Y);
            float cosyCosp = 1f - 2f * (Y * Y + Z * Z);
            float yaw = (float)Math.Atan2(sinyCosp, cosyCosp);

            return new Vector3(roll, pitch, yaw);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 ToMatrix()
        {
            float xx = X * X, xy = X * Y, xz = X * Z, xw = X * W;
            float yy = Y * Y, yz = Y * Z, yw = Y * W;
            float zz = Z * Z, zw = Z * W;

            return new Matrix4x4(
                1f - 2f * (yy + zz), 2f * (xy + zw), 2f * (xz - yw), 0f,
                2f * (xy - zw), 1f - 2f * (xx + zz), 2f * (yz + xw), 0f,
                2f * (xz + yw), 2f * (yz - xw), 1f - 2f * (xx + yy), 0f,
                0f, 0f, 0f, 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Rotate(Vector3 point)
        {
            Quaternion q = this;
            Quaternion conjugate = q.Conjugated();
            Quaternion p = new Quaternion(point.X, point.Y, point.Z, 0f);
            Quaternion result = q * p * conjugate;
            return new Vector3(result.X, result.Y, result.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            return rotation.Rotate(point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Quaternion a, Quaternion b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return !(a == b);
        }

        public static Quaternion FromEulerAngles(float x, float y, float z)
        {
            float cosX = (float)Math.Cos(x * 0.5f);
            float sinX = (float)Math.Sin(x * 0.5f);
            float cosY = (float)Math.Cos(y * 0.5f);
            float sinY = (float)Math.Sin(y * 0.5f);
            float cosZ = (float)Math.Cos(z * 0.5f);
            float sinZ = (float)Math.Sin(z * 0.5f);

            return new Quaternion(
                sinX * cosY * cosZ + cosX * sinY * sinZ,
                cosX * sinY * cosZ - sinX * cosY * sinZ,
                cosX * cosY * sinZ + sinX * sinY * cosZ,
                cosX * cosY * cosZ - sinX * sinY * sinZ
            );
        }

        public static Quaternion FromEulerAngles(Vector3 euler)
        {
            return FromEulerAngles(euler.X, euler.Y, euler.Z);
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 up)
        {
            forward = forward.Normalized();
            Vector3 right = up.Cross(forward).Normalized();
            Vector3 newUp = forward.Cross(right);

            float m00 = right.X, m01 = right.Y, m02 = right.Z;
            float m10 = newUp.X, m11 = newUp.Y, m12 = newUp.Z;
            float m20 = forward.X, m21 = forward.Y, m22 = forward.Z;

            float trace = m00 + m11 + m22;
            Quaternion q = new Quaternion();

            if (trace > 0f)
            {
                float s = (float)Math.Sqrt(trace + 1f) * 2f;
                q.W = 0.25f * s;
                q.X = (m21 - m12) / s;
                q.Y = (m02 - m20) / s;
                q.Z = (m10 - m01) / s;
            }
            else if (m00 > m11 && m00 > m22)
            {
                float s = (float)Math.Sqrt(1f + m00 - m11 - m22) * 2f;
                q.W = (m21 - m12) / s;
                q.X = 0.25f * s;
                q.Y = (m01 + m10) / s;
                q.Z = (m02 + m20) / s;
            }
            else if (m11 > m22)
            {
                float s = (float)Math.Sqrt(1f + m11 - m00 - m22) * 2f;
                q.W = (m02 - m20) / s;
                q.X = (m01 + m10) / s;
                q.Y = 0.25f * s;
                q.Z = (m12 + m21) / s;
            }
            else
            {
                float s = (float)Math.Sqrt(1f + m22 - m00 - m11) * 2f;
                q.W = (m10 - m01) / s;
                q.X = (m02 + m20) / s;
                q.Y = (m12 + m21) / s;
                q.Z = 0.25f * s;
            }

            q.Normalize();
            return q;
        }

        public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);

            float cosHalfAngle = a.Dot(b);
            if (cosHalfAngle < 0f)
            {
                b = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
                cosHalfAngle = -cosHalfAngle;
            }

            if (cosHalfAngle >= 0.9999f)
            {
                return new Quaternion(
                    a.X + (b.X - a.X) * t,
                    a.Y + (b.Y - a.Y) * t,
                    a.Z + (b.Z - a.Z) * t,
                    a.W + (b.W - a.W) * t
                ).Normalized();
            }

            float halfAngle = (float)Math.Acos(cosHalfAngle);
            float sinHalfAngle = (float)Math.Sqrt(1f - cosHalfAngle * cosHalfAngle);

            float aFactor = (float)Math.Sin((1f - t) * halfAngle) / sinHalfAngle;
            float bFactor = (float)Math.Sin(t * halfAngle) / sinHalfAngle;

            return new Quaternion(
                a.X * aFactor + b.X * bFactor,
                a.Y * aFactor + b.Y * bFactor,
                a.Z * aFactor + b.Z * bFactor,
                a.W * aFactor + b.W * bFactor
            );
        }

        public override bool Equals(object? obj) => obj is Quaternion other && Equals(other);
        public bool Equals(Quaternion other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
        public override string ToString() => $"Quaternion(X:{X:F3}, Y:{Y:F3}, Z:{Z:F3}, W:{W:F3})";
    }

    public static class MathHelper
    {
        public const float Pi = MathF.PI;
        public const float TwoPi = MathF.PI * 2f;
        public const float PiOver2 = MathF.PI / 2f;
        public const float PiOver4 = MathF.PI / 4f;
        public const float Deg2Rad = MathF.PI / 180f;
        public const float Rad2Deg = 180f / MathF.PI;
        public const float Epsilon = 1e-6f;
        public const float LargeEpsilon = 1e-4f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp(t, 0f, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(float a, float b, float value)
        {
            if (Math.Abs(a - b) < Epsilon) return 0f;
            return Clamp((value - a) / (b - a), 0f, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStep(float from, float to, float t)
        {
            t = Clamp(t, 0f, 1f);
            t = -2f * t * t * t + 3f * t * t;
            return to * t + from * (1f - t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmootherStep(float from, float to, float t)
        {
            t = Clamp(t, 0f, 1f);
            t = t * t * t * (t * (t * 6f - 15f) + 10f);
            return to * t + from * (1f - t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
                return target;
            return current + Math.Sign(target - current) * maxDelta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            return length - Math.Abs(t - length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Repeat(float t, float length)
        {
            return Clamp(t - Floor(t / length) * length, 0f, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(float value)
        {
            return value >= 0f ? (value > 0f ? 1 : 0) : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Floor(float value)
        {
            return MathF.Floor(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToInt(float value)
        {
            return (int)MathF.Floor(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Ceil(float value)
        {
            return MathF.Ceiling(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilToInt(float value)
        {
            return (int)MathF.Ceiling(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Round(float value)
        {
            return MathF.Round(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(float value)
        {
            return (int)MathF.Round(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float value)
        {
            return MathF.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b)
        {
            return a < b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b)
        {
            return a > b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow(float baseValue, float exponent)
        {
            return MathF.Pow(baseValue, exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp(float power)
        {
            return MathF.Exp(power);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float value)
        {
            return MathF.Sqrt(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float angle)
        {
            return MathF.Sin(angle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float angle)
        {
            return MathF.Cos(angle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan(float angle)
        {
            return MathF.Tan(angle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Asin(float value)
        {
            return MathF.Asin(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Acos(float value)
        {
            return MathF.Acos(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan(float value)
        {
            return MathF.Atan(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan2(float y, float x)
        {
            return MathF.Atan2(y, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float a, float b, float tolerance = Epsilon)
        {
            return MathF.Abs(a - b) < tolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float WrapAngle(float angle)
        {
            angle = Repeat(angle, 360f);
            if (angle > 180f) angle -= 360f;
            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleBetween(Vector2 from, Vector2 to)
        {
            return MathF.Acos(Math.Clamp(from.Normalized().Dot(to.Normalized()), -1f, 1f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 CartToPolar(Vector2 cart)
        {
            float r = cart.Length();
            float theta = MathF.Atan2(cart.Y, cart.X);
            return new Vector2(r, theta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 PolarToCart(Vector2 polar)
        {
            float x = polar.X * MathF.Cos(polar.Y);
            float y = polar.X * MathF.Sin(polar.Y);
            return new Vector2(x, y);
        }
    }
}

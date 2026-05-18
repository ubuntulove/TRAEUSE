using System;
using System.Runtime.CompilerServices;

namespace WPFEngine.Math
{
    public struct Matrix2x2
    {
        public float M11, M12;
        public float M21, M22;

        public static readonly Matrix2x2 Identity = new Matrix2x2(1f, 0f, 0f, 1f);
        public static readonly Matrix2x2 Zero = new Matrix2x2(0f, 0f, 0f, 0f);

        public Matrix2x2(float m11, float m12, float m21, float m22)
        {
            M11 = m11; M12 = m12;
            M21 = m21; M22 = m22;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Determinant()
        {
            return M11 * M22 - M12 * M21;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2 Transpose()
        {
            return new Matrix2x2(M11, M21, M12, M22);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2 Inverse()
        {
            float det = Determinant();
            if (Math.Abs(det) < 1e-10f)
                return Identity;
            float invDet = 1f / det;
            return new Matrix2x2(M22 * invDet, -M12 * invDet, -M21 * invDet, M11 * invDet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Transform(Vector2 position)
        {
            return new Vector2(
                position.X * M11 + position.Y * M21,
                position.X * M12 + position.Y * M22
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 Rotation(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            return new Matrix2x2(c, s, -s, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 Scale(float scale)
        {
            return new Matrix2x2(scale, 0f, 0f, scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 Scale(Vector2 scale)
        {
            return new Matrix2x2(scale.X, 0f, 0f, scale.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
        {
            return new Matrix2x2(
                a.M11 * b.M11 + a.M12 * b.M21,
                a.M11 * b.M12 + a.M12 * b.M22,
                a.M21 * b.M11 + a.M22 * b.M21,
                a.M21 * b.M12 + a.M22 * b.M22
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Matrix2x2 m, Vector2 v)
        {
            return m.Transform(v);
        }
    }

    public struct Matrix3x3
    {
        public float M11, M12, M13;
        public float M21, M22, M23;
        public float M31, M32, M33;

        public static readonly Matrix3x3 Identity = new Matrix3x3(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f);

        public Matrix3x3(
            float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33)
        {
            M11 = m11; M12 = m12; M13 = m13;
            M21 = m21; M22 = m22; M23 = m23;
            M31 = m31; M32 = m32; M33 = m33;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 Translation(float x, float y)
        {
            return new Matrix3x3(1f, 0f, 0f, 0f, 1f, 0f, x, y, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 Translation(Vector2 position)
        {
            return Translation(position.X, position.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 Rotation(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            return new Matrix3x3(c, s, 0f, -s, c, 0f, 0f, 0f, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 Scale(float scale)
        {
            return new Matrix3x3(scale, 0f, 0f, 0f, scale, 0f, 0f, 0f, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 Scale(Vector2 scale)
        {
            return new Matrix3x3(scale.X, 0f, 0f, 0f, scale.Y, 0f, 0f, 0f, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
        {
            return new Matrix3x3(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            );
        }

        public Vector2 Translation()
        {
            return new Vector2(M31, M32);
        }

        public float Rotation()
        {
            return (float)Math.Atan2(M12, M11);
        }

        public Vector2 Scale()
        {
            return new Vector2(
                (float)Math.Sqrt(M11 * M11 + M12 * M12),
                (float)Math.Sqrt(M21 * M21 + M22 * M22)
            );
        }
    }

    public struct Matrix4x4
    {
        public float M11, M12, M13, M14;
        public float M21, M22, M23, M24;
        public float M31, M32, M33, M34;
        public float M41, M42, M43, M44;

        public static readonly Matrix4x4 Identity = new Matrix4x4(
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
        );

        public Matrix4x4(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Translation(Vector3 position)
        {
            return new Matrix4x4(
                1f, 0f, 0f, 0f,
                0f, 1f, 0f, 0f,
                0f, 0f, 1f, 0f,
                position.X, position.Y, position.Z, 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Translation(float x, float y, float z)
        {
            return Translation(new Vector3(x, y, z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Scale(Vector3 scale)
        {
            return new Matrix4x4(
                scale.X, 0f, 0f, 0f,
                0f, scale.Y, 0f, 0f,
                0f, 0f, scale.Z, 0f,
                0f, 0f, 0f, 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 RotationX(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            return new Matrix4x4(
                1f, 0f, 0f, 0f,
                0f, c, s, 0f,
                0f, -s, c, 0f,
                0f, 0f, 0f, 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 RotationY(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            return new Matrix4x4(
                c, 0f, -s, 0f,
                0f, 1f, 0f, 0f,
                s, 0f, c, 0f,
                0f, 0f, 0f, 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 RotationZ(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            return new Matrix4x4(
                c, s, 0f, 0f,
                -s, c, 0f, 0f,
                0f, 0f, 1f, 0f,
                0f, 0f, 0f, 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Perspective(float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
        {
            float yScale = 1f / (float)Math.Tan(fieldOfView / 2f);
            float xScale = yScale / aspectRatio;
            float zRange = farPlane / (nearPlane - farPlane);

            return new Matrix4x4(
                xScale, 0f, 0f, 0f,
                0f, yScale, 0f, 0f,
                0f, 0f, zRange, -1f,
                0f, 0f, zRange * nearPlane, 0f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Orthographic(float left, float right, float bottom, float top, float nearPlane, float farPlane)
        {
            float width = right - left;
            float height = top - bottom;
            float depth = farPlane - nearPlane;

            return new Matrix4x4(
                2f / width, 0f, 0f, 0f,
                0f, 2f / height, 0f, 0f,
                0f, 0f, 1f / depth, 0f,
                -(right + left) / width, -(top + bottom) / height, -nearPlane / depth, 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 LookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUp)
        {
            Vector3 zAxis = (cameraPosition - cameraTarget).Normalized();
            Vector3 xAxis = cameraUp.Cross(zAxis).Normalized();
            Vector3 yAxis = zAxis.Cross(xAxis);

            return new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, 0f,
                xAxis.Y, yAxis.Y, zAxis.Y, 0f,
                xAxis.Z, yAxis.Z, zAxis.Z, 0f,
                -xAxis.Dot(cameraPosition), -yAxis.Dot(cameraPosition), -zAxis.Dot(cameraPosition), 1f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Translation()
        {
            return new Vector3(M41, M42, M43);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetRow(int row)
        {
            return row switch
            {
                0 => new Vector3(M11, M12, M13),
                1 => new Vector3(M21, M22, M23),
                2 => new Vector3(M31, M32, M33),
                3 => new Vector3(M41, M42, M43),
                _ => throw new ArgumentOutOfRangeException(nameof(row))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetColumn(int col)
        {
            return col switch
            {
                0 => new Vector3(M11, M21, M31),
                1 => new Vector3(M12, M22, M32),
                2 => new Vector3(M13, M23, M33),
                3 => new Vector3(M14, M24, M34),
                _ => throw new ArgumentOutOfRangeException(nameof(col))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(Matrix4x4 m, Vector4 v)
        {
            return new Vector4(
                m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W,
                m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W,
                m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W,
                m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W
            );
        }
    }
}

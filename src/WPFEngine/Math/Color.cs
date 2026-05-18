using System;
using System.Runtime.CompilerServices;

namespace WPFEngine.Math
{
    public struct Color : IEquatable<Color>
    {
        public float R, G, B, A;

        public static readonly Color White = new Color(1f, 1f, 1f, 1f);
        public static readonly Color Black = new Color(0f, 0f, 0f, 1f);
        public static readonly Color Red = new Color(1f, 0f, 0f, 1f);
        public static readonly Color Green = new Color(0f, 1f, 0f, 1f);
        public static readonly Color Blue = new Color(0f, 0f, 1f, 1f);
        public static readonly Color Yellow = new Color(1f, 1f, 0f, 1f);
        public static readonly Color Cyan = new Color(0f, 1f, 1f, 1f);
        public static readonly Color Magenta = new Color(1f, 0f, 1f, 1f);
        public static readonly Color Gray = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color Clear = new Color(0f, 0f, 0f, 0f);
        public static readonly Color Orange = new Color(1f, 0.5f, 0f, 1f);
        public static readonly Color Purple = new Color(0.5f, 0f, 0.5f, 1f);
        public static readonly Color Pink = new Color(1f, 0.75f, 0.8f, 1f);
        public static readonly Color Brown = new Color(0.6f, 0.4f, 0.2f, 1f);
        public static readonly Color Olive = new Color(0.5f, 0.5f, 0f, 1f);
        public static readonly Color Navy = new Color(0f, 0f, 0.5f, 1f);
        public static readonly Color Teal = new Color(0f, 0.5f, 0.5f, 1f);
        public static readonly Color Lime = new Color(0.75f, 1f, 0f, 1f);
        public static readonly Color Maroon = new Color(0.5f, 0f, 0f, 1f);
        public static readonly Color Silver = new Color(0.75f, 0.75f, 0.75f, 1f);

        public Color(float r, float g, float b, float a = 1f)
        {
            R = Math.Clamp(r, 0f, 1f);
            G = Math.Clamp(g, 0f, 1f);
            B = Math.Clamp(b, 0f, 1f);
            A = Math.Clamp(a, 0f, 1f);
        }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetRByte() => (byte)(R * 255);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetGByte() => (byte)(G * 255);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBByte() => (byte)(B * 255);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetAByte() => (byte)(A * 255);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color WithRed(float r) => new Color(r, G, B, A);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color WithGreen(float g) => new Color(R, g, B, A);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color WithBlue(float b) => new Color(R, G, b, A);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color WithAlpha(float a) => new Color(R, G, B, a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Multiply(Color other)
        {
            return new Color(R * other.R, G * other.G, B * other.B, A * other.A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Add(Color other)
        {
            return new Color(
                Math.Min(R + other.R, 1f),
                Math.Min(G + other.G, 1f),
                Math.Min(B + other.B, 1f),
                Math.Min(A + other.A, 1f)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Subtract(Color other)
        {
            return new Color(
                Math.Max(R - other.R, 0f),
                Math.Max(G - other.G, 0f),
                Math.Max(B - other.B, 0f),
                Math.Max(A - other.A, 0f)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Scale(float factor)
        {
            return new Color(
                Math.Clamp(R * factor, 0f, 1f),
                Math.Clamp(G * factor, 0f, 1f),
                Math.Clamp(B * factor, 0f, 1f),
                Math.Clamp(A * factor, 0f, 1f)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Lerp(Color target, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Color(
                R + (target.R - R) * t,
                G + (target.G - G) * t,
                B + (target.B - B) * t,
                A + (target.A - A) * t
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Grayscale()
        {
            return 0.299f * R + 0.587f * G + 0.114f * B;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Invert()
        {
            return new Color(1f - R, 1f - G, 1f - B, A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color ToLinear()
        {
            return new Color(
                (float)Math.Pow(R, 2.2f),
                (float)Math.Pow(G, 2.2f),
                (float)Math.Pow(B, 2.2f),
                A
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color ToGamma()
        {
            return new Color(
                (float)Math.Pow(R, 1f / 2.2f),
                (float)Math.Pow(G, 1f / 2.2f),
                (float)Math.Pow(B, 1f / 2.2f),
                A
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color PremultiplyAlpha()
        {
            return new Color(R * A, G * A, B * A, A);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToARGB()
        {
            return ((uint)(A * 255) << 24) | ((uint)(R * 255) << 16) | ((uint)(G * 255) << 8) | (uint)(B * 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToRGBA()
        {
            return ((uint)(R * 255) << 24) | ((uint)(G * 255) << 16) | ((uint)(B * 255) << 8) | (uint)(A * 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToABGR()
        {
            return ((uint)(A * 255) << 24) | ((uint)(B * 255) << 16) | ((uint)(G * 255) << 8) | (uint)(R * 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color FromARGB(uint argb)
        {
            return new Color(
                ((argb >> 16) & 0xFF) / 255f,
                ((argb >> 8) & 0xFF) / 255f,
                (argb & 0xFF) / 255f,
                ((argb >> 24) & 0xFF) / 255f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color FromRGBA(uint rgba)
        {
            return new Color(
                ((rgba >> 24) & 0xFF) / 255f,
                ((rgba >> 16) & 0xFF) / 255f,
                ((rgba >> 8) & 0xFF) / 255f,
                (rgba & 0xFF) / 255f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color FromHSV(float h, float s, float v)
        {
            h = ((h % 360f) + 360f) % 360f;
            s = Math.Clamp(s, 0f, 1f);
            v = Math.Clamp(v, 0f, 1f);

            float c = v * s;
            float x = c * (1f - Math.Abs((h / 60f) % 2f - 1f));
            float m = v - c;

            float r, g, b;
            if (h < 60f) { r = c; g = x; b = 0f; }
            else if (h < 120f) { r = x; g = c; b = 0f; }
            else if (h < 180f) { r = 0f; g = c; b = x; }
            else if (h < 240f) { r = 0f; g = x; b = c; }
            else if (h < 300f) { r = x; g = 0f; b = c; }
            else { r = c; g = 0f; b = x; }

            return new Color(r + m, g + m, b + m, 1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (float H, float S, float V) ToHSV()
        {
            float max = Math.Max(R, Math.Max(G, B));
            float min = Math.Min(R, Math.Min(G, B));
            float delta = max - min;

            float h = 0f;
            if (delta != 0f)
            {
                if (max == R) h = 60f * (((G - B) / delta) % 6f);
                else if (max == G) h = 60f * (((B - R) / delta) + 2f);
                else h = 60f * (((R - G) / delta) + 4f);
            }
            if (h < 0f) h += 360f;

            float s = max == 0f ? 0f : delta / max;
            float v = max;

            return (h, s, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color operator +(Color a, Color b) => a.Add(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color operator -(Color a, Color b) => a.Subtract(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color operator *(Color a, Color b) => a.Multiply(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color operator *(Color a, float d) => a.Scale(d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color operator *(float d, Color a) => a.Scale(d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color operator /(Color a, float d) => a.Scale(1f / d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color a, Color b) => !(a == b);

        public override bool Equals(object? obj) => obj is Color other && Equals(other);
        public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override int GetHashCode() => HashCode.Combine(R, G, B, A);
        public override string ToString() => $"Color(R:{R:F2}, G:{G:F2}, B:{B:F2}, A:{A:F2})";
    }

    public struct Color32 : IEquatable<Color32>
    {
        public byte R, G, B, A;

        public static readonly Color32 White = new Color32(255, 255, 255, 255);
        public static readonly Color32 Black = new Color32(0, 0, 0, 255);
        public static readonly Color32 Red = new Color32(255, 0, 0, 255);
        public static readonly Color32 Green = new Color32(0, 255, 0, 255);
        public static readonly Color32 Blue = new Color32(0, 0, 255, 255);
        public static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        public Color32(byte r, byte g, byte b, byte a = 255)
        {
            R = r; G = g; B = b; A = a;
        }

        public Color32(Color color)
        {
            R = (byte)(color.R * 255);
            G = (byte)(color.G * 255);
            B = (byte)(color.B * 255);
            A = (byte)(color.A * 255);
        }

        public Color ToColor() => new Color(R / 255f, G / 255f, B / 255f, A / 255f);
        public override bool Equals(object? obj) => obj is Color32 other && Equals(other);
        public bool Equals(Color32 other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override int GetHashCode() => HashCode.Combine(R, G, B, A);
        public override string ToString() => $"Color32(R:{R}, G:{G}, B:{B}, A:{A})";
    }
}

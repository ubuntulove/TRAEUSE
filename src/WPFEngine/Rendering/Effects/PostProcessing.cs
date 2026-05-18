using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFEngine.Math;

namespace WPFEngine.Rendering.Effects
{
    public interface IPostProcessingEffect
    {
        bool IsEnabled { get; set; }
        string Name { get; }
        int Order { get; set; }
        void Apply(WriteableBitmap source, WriteableBitmap destination);
        void Update(float deltaTime);
    }

    public abstract class PostProcessingEffect : IPostProcessingEffect
    {
        private bool isEnabled = true;
        private int order;

        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        public abstract string Name { get; }

        public int Order
        {
            get => order;
            set => order = value;
        }

        public abstract void Apply(WriteableBitmap source, WriteableBitmap destination);

        public virtual void Update(float deltaTime)
        {
        }

        protected static void CopyPixels(WriteableBitmap source, WriteableBitmap destination)
        {
            if (source.PixelWidth != destination.PixelWidth || source.PixelHeight != destination.PixelHeight)
                return;

            source.CopyPixels(destination.PixelBuffer.GetPixels(), destination.PixelWidth * 4, 0);
        }

        protected static Color[] GetPixels(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            bitmap.CopyPixels(pixels, stride, 0);

            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                int offset = i * 4;
                colors[i] = new Color(
                    pixels[offset + 2] / 255f,
                    pixels[offset + 1] / 255f,
                    pixels[offset] / 255f,
                    pixels[offset + 3] / 255f
                );
            }
            return colors;
        }

        protected static void SetPixels(WriteableBitmap bitmap, Color[] colors)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];

            for (int i = 0; i < colors.Length; i++)
            {
                int offset = i * 4;
                pixels[offset] = colors[i].GetBByte();
                pixels[offset + 1] = colors[i].GetGByte();
                pixels[offset + 2] = colors[i].GetRByte();
                pixels[offset + 3] = colors[i].GetAByte();
            }

            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        protected static Color GetPixel(Color[] pixels, int width, int x, int y)
        {
            x = Math.Max(0, Math.Min(width - 1, x));
            y = Math.Max(0, Math.Min(pixels.Length / width - 1, y));
            return pixels[y * width + x];
        }

        protected static void SetPixel(Color[] pixels, int width, int x, int y, Color color)
        {
            if (x < 0 || x >= width || y < 0 || y >= pixels.Length / width)
                return;
            pixels[y * width + x] = color;
        }
    }

    public enum BlurType
    {
        Gaussian,
        Box,
        Kawase
    }

    public enum EdgeDetectionType
    {
        Sobel,
        Laplacian
    }

    public enum ScreenBlendMode
    {
        Normal,
        Multiply,
        Screen,
        Overlay,
        Darken,
        Lighten,
        ColorDodge,
        ColorBurn,
        HardLight,
        SoftLight,
        Difference,
        Exclusion,
        Subtract,
        Add,
        GrainExtract,
        GrainMerge
    }

    public enum ColorGradeMode
    {
        None,
        LiftGammaGain,
        GainGammaLift,
        Lut
    }

    public enum DofBlurQuality
    {
        Low,
        Medium,
        High
    }

    public class EffectSettings
    {
        public float Time { get; set; }
        public Vector2 Resolution { get; set; }

        public EffectSettings()
        {
            Time = 0f;
            Resolution = new Vector2(1920, 1080);
        }
    }

    public class BloomEffect : PostProcessingEffect
    {
        private float threshold = 0.8f;
        private float intensity = 1.0f;
        private float radius = 0.5f;
        private int iterations = 6;
        private WriteableBitmap? blurBuffer1;
        private WriteableBitmap? blurBuffer2;

        public override string Name => "Bloom";

        public float Threshold
        {
            get => threshold;
            set => threshold = Math.Clamp(value, 0f, 1f);
        }

        public float Intensity
        {
            get => intensity;
            set => intensity = Math.Max(0f, value);
        }

        public float Radius
        {
            get => radius;
            set => radius = Math.Max(0f, Math.Min(1f, value));
        }

        public int Iterations
        {
            get => iterations;
            set => iterations = Math.Max(1, Math.Min(10, value));
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            EnsureBuffers(source.PixelWidth, source.PixelHeight);

            Color[] sourcePixels = GetPixels(source);
            Color[] brightPixels = ExtractBrightPixels(sourcePixels, source.PixelWidth, source.PixelHeight);
            Color[] blurredPixels = ApplyBoxBlurIterative(brightPixels, source.PixelWidth, source.PixelHeight);

            Color[] finalPixels = new Color[sourcePixels.Length];
            for (int i = 0; i < finalPixels.Length; i++)
            {
                Color bloom = blurredPixels[i].Scale(intensity);
                finalPixels[i] = sourcePixels[i].Add(bloom);
            }

            SetPixels(destination, finalPixels);
        }

        private Color[] ExtractBrightPixels(Color[] source, int width, int height)
        {
            Color[] result = new Color[source.Length];
            float thresholdValue = threshold * 3f;

            for (int i = 0; i < source.Length; i++)
            {
                float luminance = source[i].R * 0.299f + source[i].G * 0.587f + source[i].B * 0.114f;
                if (luminance > threshold)
                {
                    float factor = (luminance - threshold) / (1f - threshold);
                    result[i] = source[i].Scale(factor);
                }
                else
                {
                    result[i] = Color.Black;
                }
            }
            return result;
        }

        private Color[] ApplyBoxBlurIterative(Color[] source, int width, int height)
        {
            Color[] temp = new Color[source.Length];
            Color[] result = new Color[source.Length];

            int blurSize = (int)(Math.Max(width, height) * radius * 0.02f) + 1;
            blurSize = Math.Max(1, blurSize);

            Array.Copy(source, temp, source.Length);

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                float currentRadius = blurSize * (1f + iteration * 0.5f);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float r = 0f, g = 0f, b = 0f;
                        int count = 0;

                        int minX = Math.Max(0, x - (int)currentRadius);
                        int maxX = Math.Min(width - 1, x + (int)currentRadius);
                        int minY = Math.Max(0, y - (int)currentRadius);
                        int maxY = Math.Min(height - 1, y + (int)currentRadius);

                        for (int sy = minY; sy <= maxY; sy++)
                        {
                            for (int sx = minX; sx <= maxX; sx++)
                            {
                                Color pixel = GetPixel(temp, width, sx, sy);
                                r += pixel.R;
                                g += pixel.G;
                                b += pixel.B;
                                count++;
                            }
                        }

                        if (count > 0)
                        {
                            Color original = GetPixel(source, width, x, y);
                            result[y * width + x] = new Color(
                                original.R + (r / count - original.R) * 0.5f,
                                original.G + (g / count - original.G) * 0.5f,
                                original.B + (b / count - original.B) * 0.5f,
                                original.A
                            );
                        }
                    }
                }

                Array.Copy(result, temp, result.Length);
            }

            return result;
        }

        private void EnsureBuffers(int width, int height)
        {
            if (blurBuffer1 == null || blurBuffer1.PixelWidth != width || blurBuffer1.PixelHeight != height)
            {
                blurBuffer1 = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                blurBuffer2 = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            }
        }
    }

    public class BlurEffect : PostProcessingEffect
    {
        private BlurType blurType = BlurType.Gaussian;
        private float radius = 4f;
        private float sigma = 2f;

        public override string Name => "Blur";

        public BlurType BlurType
        {
            get => blurType;
            set => blurType = value;
        }

        public float Radius
        {
            get => radius;
            set => radius = Math.Max(0f, value);
        }

        public float Sigma
        {
            get => sigma;
            set => sigma = Math.Max(0.1f, value);
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels;

            switch (blurType)
            {
                case BlurType.Gaussian:
                    resultPixels = ApplyGaussianBlur(sourcePixels, source.PixelWidth, source.PixelHeight);
                    break;
                case BlurType.Box:
                    resultPixels = ApplyBoxBlur(sourcePixels, source.PixelWidth, source.PixelHeight);
                    break;
                case BlurType.Kawase:
                    resultPixels = ApplyKawaseBlur(sourcePixels, source.PixelWidth, source.PixelHeight);
                    break;
                default:
                    resultPixels = sourcePixels;
                    break;
            }

            SetPixels(destination, resultPixels);
        }

        private Color[] ApplyGaussianBlur(Color[] source, int width, int height)
        {
            Color[] temp = new Color[source.Length];
            Color[] result = new Color[source.Length];

            int kernelSize = (int)(radius * 2f);
            if (kernelSize % 2 == 0) kernelSize++;
            kernelSize = Math.Max(3, kernelSize);

            float[] kernel = new float[kernelSize];
            float twoSigmaSq = 2f * sigma * sigma;
            float sum = 0f;

            int halfKernel = kernelSize / 2;
            for (int i = 0; i < kernelSize; i++)
            {
                int x = i - halfKernel;
                kernel[i] = (float)Math.Exp(-(x * x) / twoSigmaSq);
                sum += kernel[i];
            }

            for (int i = 0; i < kernelSize; i++)
            {
                kernel[i] /= sum;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0f, g = 0f, b = 0f;

                    for (int kx = 0; kx < kernelSize; kx++)
                    {
                        int px = Math.Clamp(x + kx - halfKernel, 0, width - 1);
                        Color pixel = GetPixel(source, width, px, y);
                        float weight = kernel[kx];
                        r += pixel.R * weight;
                        g += pixel.G * weight;
                        b += pixel.B * weight;
                    }

                    temp[y * width + x] = new Color(r, g, b, GetPixel(source, width, x, y).A);
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0f, g = 0f, b = 0f;

                    for (int ky = 0; ky < kernelSize; ky++)
                    {
                        int py = Math.Clamp(y + ky - halfKernel, 0, height - 1);
                        Color pixel = GetPixel(temp, width, x, py);
                        float weight = kernel[ky];
                        r += pixel.R * weight;
                        g += pixel.G * weight;
                        b += pixel.B * weight;
                    }

                    result[y * width + x] = new Color(r, g, b, GetPixel(source, width, x, y).A);
                }
            }

            return result;
        }

        private Color[] ApplyBoxBlur(Color[] source, int width, int height)
        {
            Color[] temp = new Color[source.Length];
            Color[] result = new Color[source.Length];

            int kernelSize = (int)(radius * 2f);
            if (kernelSize % 2 == 0) kernelSize++;
            kernelSize = Math.Max(3, kernelSize);

            int halfKernel = kernelSize / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0f, g = 0f, b = 0f;
                    int count = 0;

                    for (int ky = -halfKernel; ky <= halfKernel; ky++)
                    {
                        for (int kx = -halfKernel; kx <= halfKernel; kx++)
                        {
                            int px = Math.Clamp(x + kx, 0, width - 1);
                            int py = Math.Clamp(y + ky, 0, height - 1);
                            Color pixel = GetPixel(source, width, px, py);
                            r += pixel.R;
                            g += pixel.G;
                            b += pixel.B;
                            count++;
                        }
                    }

                    temp[y * width + x] = new Color(r / count, g / count, b / count, GetPixel(source, width, x, y).A);
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0f, g = 0f, b = 0f;
                    int count = 0;

                    for (int ky = -halfKernel; ky <= halfKernel; ky++)
                    {
                        for (int kx = -halfKernel; kx <= halfKernel; kx++)
                        {
                            int px = Math.Clamp(x + kx, 0, width - 1);
                            int py = Math.Clamp(y + ky, 0, height - 1);
                            Color pixel = GetPixel(temp, width, px, py);
                            r += pixel.R;
                            g += pixel.G;
                            b += pixel.B;
                            count++;
                        }
                    }

                    result[y * width + x] = new Color(r / count, g / count, b / count, GetPixel(source, width, x, y).A);
                }
            }

            return result;
        }

        private Color[] ApplyKawaseBlur(Color[] source, int width, int height)
        {
            Color[] result = new Color[source.Length];
            int iterations = Math.Max(1, (int)radius);

            Color[] current = new Color[source.Length];
            Color[] next = new Color[source.Length];
            Array.Copy(source, current, source.Length);

            for (int i = 0; i < iterations; i++)
            {
                int offset = i + 1;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color tl = GetPixel(current, width, x - offset, y - offset);
                        Color tr = GetPixel(current, width, x + offset, y - offset);
                        Color bl = GetPixel(current, width, x - offset, y + offset);
                        Color br = GetPixel(current, width, x + offset, y + offset);

                        next[y * width + x] = new Color(
                            (tl.R + tr.R + bl.R + br.R) * 0.25f,
                            (tl.G + tr.G + bl.G + br.G) * 0.25f,
                            (tl.B + tr.B + bl.B + br.B) * 0.25f,
                            GetPixel(current, width, x, y).A
                        );
                    }
                }

                Array.Copy(next, current, next.Length);
            }

            return current;
        }
    }

    public class ColorGradingEffect : PostProcessingEffect
    {
        private ColorGradeMode mode = ColorGradeMode.LiftGammaGain;
        private WriteableBitmap? lutTexture;
        private int lutWidth = 16;
        private int lutHeight = 16;

        private Color lift = Color.Black;
        private Color gamma = new Color(1f, 1f, 1f, 1f);
        private Color gain = new Color(1f, 1f, 1f, 1f);

        private Color shadows = new Color(0.5f, 0.5f, 0.6f, 1f);
        private Color midtones = new Color(1f, 1f, 1f, 1f);
        private Color highlights = new Color(1f, 0.9f, 0.8f, 1f);
        private float shadowsRange = 0.3f;
        private float highlightsRange = 0.7f;

        public override string Name => "Color Grading";

        public ColorGradeMode Mode
        {
            get => mode;
            set => mode = value;
        }

        public Color Lift
        {
            get => lift;
            set => lift = value;
        }

        public Color Gamma
        {
            get => gamma;
            set => gamma = value;
        }

        public Color Gain
        {
            get => gain;
            set => gain = value;
        }

        public Color Shadows
        {
            get => shadows;
            set => shadows = value;
        }

        public Color Midtones
        {
            get => midtones;
            set => midtones = value;
        }

        public Color Highlights
        {
            get => highlights;
            set => highlights = value;
        }

        public float ShadowsRange
        {
            get => shadowsRange;
            set => shadowsRange = Math.Clamp(value, 0f, 1f);
        }

        public float HighlightsRange
        {
            get => highlightsRange;
            set => highlightsRange = Math.Clamp(value, 0f, 1f);
        }

        public void SetLut(WriteableBitmap lut)
        {
            lutTexture = lut;
            lutWidth = lut.PixelWidth;
            lutHeight = lut.PixelHeight;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            switch (mode)
            {
                case ColorGradeMode.LiftGammaGain:
                    resultPixels = ApplyLiftGammaGain(sourcePixels);
                    break;
                case ColorGradeMode.GainGammaLift:
                    resultPixels = ApplyGainGammaLift(sourcePixels);
                    break;
                case ColorGradeMode.Lut:
                    resultPixels = ApplyLut(sourcePixels);
                    break;
                default:
                    resultPixels = sourcePixels;
                    break;
            }

            SetPixels(destination, resultPixels);
        }

        private Color[] ApplyLiftGammaGain(Color[] source)
        {
            Color[] result = new Color[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                Color c = source[i];

                float r = (gain.R + lift.R * (1f - gain.R)) * Math.Pow(c.R, 1f / gamma.R);
                float g = (gain.G + lift.G * (1f - gain.G)) * Math.Pow(c.G, 1f / gamma.G);
                float b = (gain.B + lift.B * (1f - gain.B)) * Math.Pow(c.B, 1f / gamma.B);

                result[i] = new Color(r, g, b, c.A);
            }

            return result;
        }

        private Color[] ApplyGainGammaLift(Color[] source)
        {
            Color[] result = new Color[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                Color c = source[i];

                float r = lift.R + gain.R * Math.Pow(c.R, 1f / gamma.R);
                float g = lift.G + gain.G * Math.Pow(c.G, 1f / gamma.G);
                float b = lift.B + gain.B * Math.Pow(c.B, 1f / gamma.B);

                result[i] = new Color(r, g, b, c.A);
            }

            return result;
        }

        private Color[] ApplyLut(Color[] source)
        {
            if (lutTexture == null)
                return ApplyLiftGammaGain(source);

            Color[] lutPixels = GetPixels(lutTexture);
            Color[] result = new Color[source.Length];
            int depth = lutHeight;

            for (int i = 0; i < source.Length; i++)
            {
                Color c = source[i];
                int blueSlice = (int)(c.B * (depth - 1));
                int blue2 = Math.Min(depth - 1, blueSlice + 1);

                float blueSliceFrac = c.B * (depth - 1) - blueSlice;

                int sliceSize = lutWidth / depth;
                int sliceIndex = blueSlice;

                int xBase = (sliceIndex % (lutWidth / depth)) * sliceSize;
                int yBase = (sliceIndex / (lutWidth / depth)) * depth;

                int xBase2 = (blue2 % (lutWidth / depth)) * sliceSize;
                int yBase2 = (blue2 / (lutWidth / depth)) * depth;

                int x = xBase + (int)(c.R * (sliceSize - 1));
                int y = yBase + (int)(c.G * (depth - 1));

                int x2 = xBase2 + (int)(c.R * (sliceSize - 1));
                int y2 = yBase2 + (int)(c.G * (depth - 1));

                Color lutColor1 = GetPixel(lutPixels, lutWidth, x, y);
                Color lutColor2 = GetPixel(lutPixels, lutWidth, x2, y2);

                Color lutColor = lutColor1.Lerp(lutColor2, blueSliceFrac);

                result[i] = new Color(lutColor.R, lutColor.G, lutColor.B, c.A);
            }

            return result;
        }
    }

    public class ChromaticAberrationEffect : PostProcessingEffect
    {
        private float intensity = 1.0f;
        private float radialIntensity = 0.5f;
        private bool useRadial = true;

        public override string Name => "Chromatic Aberration";

        public float Intensity
        {
            get => intensity;
            set => intensity = Math.Max(0f, value);
        }

        public float RadialIntensity
        {
            get => radialIntensity;
            set => radialIntensity = Math.Clamp(value, 0f, 1f);
        }

        public bool UseRadial
        {
            get => useRadial;
            set => useRadial = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            float centerX = width * 0.5f;
            float centerY = height * 0.5f;

            float maxDist = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    float normalizedDist = dist / maxDist;

                    float rOffset, gOffset, bOffset;

                    if (useRadial)
                    {
                        float offset = intensity * normalizedDist * radialIntensity;
                        rOffset = -offset * 1.5f;
                        bOffset = offset * 1.5f;
                        gOffset = 0f;
                    }
                    else
                    {
                        rOffset = -intensity * 1.5f;
                        gOffset = 0f;
                        bOffset = intensity * 1.5f;
                    }

                    int rX = (int)Math.Clamp(x + rOffset, 0, width - 1);
                    int rY = (int)Math.Clamp(y, 0, height - 1);
                    int gX = (int)Math.Clamp(x + gOffset, 0, width - 1);
                    int gY = (int)Math.Clamp(y, 0, height - 1);
                    int bX = (int)Math.Clamp(x + bOffset, 0, width - 1);
                    int bY = (int)Math.Clamp(y, 0, height - 1);

                    float r = GetPixel(sourcePixels, width, rX, rY).R;
                    float g = GetPixel(sourcePixels, width, gX, gY).G;
                    float b = GetPixel(sourcePixels, width, bX, bY).B;
                    float a = GetPixel(sourcePixels, width, x, y).A;

                    resultPixels[y * width + x] = new Color(r, g, b, a);
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class VignetteEffect : PostProcessingEffect
    {
        private float intensity = 0.5f;
        private float smoothness = 0.5f;
        private Vector2 center = new Vector2(0.5f, 0.5f);
        private bool circular = true;

        public override string Name => "Vignette";

        public float Intensity
        {
            get => intensity;
            set => intensity = Math.Clamp(value, 0f, 1f);
        }

        public float Smoothness
        {
            get => smoothness;
            set => smoothness = Math.Clamp(value, 0.01f, 1f);
        }

        public Vector2 Center
        {
            get => center;
            set => center = value;
        }

        public bool Circular
        {
            get => circular;
            set => circular = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            float centerX = center.X * width;
            float centerY = center.Y * height;
            float maxDist = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

            float vignetteMin = (0.5f - smoothness * 0.5f) * intensity;
            float vignetteMax = (0.5f + smoothness * 0.5f) * intensity;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;

                    float dist;
                    if (circular)
                    {
                        dist = (float)Math.Sqrt(dx * dx + dy * dy) / maxDist;
                    }
                    else
                    {
                        float aspectRatio = (float)width / height;
                        dx /= width;
                        dy /= height;
                        dist = (float)Math.Sqrt(dx * dx * aspectRatio * aspectRatio + dy * dy);
                    }

                    float vignette = Math.Clamp(dist * 2f, 0f, 1f);
                    vignette = Math.Max(0f, vignette - vignetteMin) / Math.Max(0.001f, vignetteMax - vignetteMin);
                    vignette = 1f - vignette;

                    vignette = Math.Clamp(vignette, 0f, 1f);

                    Color pixel = GetPixel(sourcePixels, width, x, y);
                    resultPixels[y * width + x] = new Color(
                        pixel.R * vignette,
                        pixel.G * vignette,
                        pixel.B * vignette,
                        pixel.A
                    );
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class FilmGrainEffect : PostProcessingEffect
    {
        private float intensity = 0.1f;
        private float size = 1.0f;
        private float speed = 1.0f;
        private bool animated = true;
        private float timeOffset;
        private Random random = new Random();

        public override string Name => "Film Grain";

        public float Intensity
        {
            get => intensity;
            set => intensity = Math.Clamp(value, 0f, 1f);
        }

        public float Size
        {
            get => size;
            set => size = Math.Max(0.1f, value);
        }

        public float Speed
        {
            get => speed;
            set => speed = Math.Max(0f, value);
        }

        public bool Animated
        {
            get => animated;
            set => animated = value;
        }

        public override void Update(float deltaTime)
        {
            if (animated)
            {
                timeOffset += deltaTime * speed * 10f;
            }
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            int grainSize = Math.Max(1, (int)(size));

            for (int y = 0; y < height; y += grainSize)
            {
                for (int x = 0; x < width; x += grainSize)
                {
                    float grain;
                    if (animated)
                    {
                        grain = (float)(Math.Sin(x * 0.1f + timeOffset) * Math.Cos(y * 0.1f + timeOffset * 1.3f) +
                                       Math.Sin(x * 0.05f - timeOffset * 0.7f) * Math.Cos(y * 0.07f + timeOffset));
                    }
                    else
                    {
                        grain = (float)(random.NextDouble() * 2.0 - 1.0);
                    }

                    float grainAmount = grain * intensity;

                    for (int gy = y; gy < Math.Min(height, y + grainSize); gy++)
                    {
                        for (int gx = x; gx < Math.Min(width, x + grainSize); gx++)
                        {
                            int index = gy * width + gx;
                            Color pixel = sourcePixels[index];

                            float r = Math.Clamp(pixel.R + grainAmount, 0f, 1f);
                            float g = Math.Clamp(pixel.G + grainAmount * 0.9f, 0f, 1f);
                            float b = Math.Clamp(pixel.B + grainAmount * 1.1f, 0f, 1f);

                            resultPixels[index] = new Color(r, g, b, pixel.A);
                        }
                    }
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class ColorCorrectionEffect : PostProcessingEffect
    {
        private float brightness = 0f;
        private float contrast = 0f;
        private float saturation = 0f;
        private float hue = 0f;
        private float exposure = 0f;
        private float temperature = 0f;
        private float tint = 0f;

        public override string Name => "Color Correction";

        public float Brightness
        {
            get => brightness;
            set => brightness = Math.Clamp(value, -1f, 1f);
        }

        public float Contrast
        {
            get => contrast;
            set => contrast = Math.Clamp(value, -1f, 1f);
        }

        public float Saturation
        {
            get => saturation;
            set => saturation = Math.Clamp(value, -1f, 1f);
        }

        public float Hue
        {
            get => hue;
            set => hue = value % 360f;
        }

        public float Exposure
        {
            get => exposure;
            set => exposure = value;
        }

        public float Temperature
        {
            get => temperature;
            set => temperature = Math.Clamp(value, -1f, 1f);
        }

        public float Tint
        {
            get => tint;
            set => tint = Math.Clamp(value, -1f, 1f);
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            float contrastFactor = 1f + contrast;
            float saturationFactor = 1f + saturation;

            for (int i = 0; i < sourcePixels.Length; i++)
            {
                Color c = sourcePixels[i];

                float r = c.R;
                float g = c.G;
                float b = c.B;

                if (exposure != 0f)
                {
                    float exposureFactor = (float)Math.Pow(2f, exposure);
                    r *= exposureFactor;
                    g *= exposureFactor;
                    b *= exposureFactor;
                }

                r += brightness;
                g += brightness;
                b += brightness;

                float gray = 0.299f * r + 0.587f * g + 0.114f * b;
                r = gray + (r - gray) * contrastFactor;
                g = gray + (g - gray) * contrastFactor;
                b = gray + (b - gray) * contrastFactor;

                if (saturation != 0f)
                {
                    r = gray + (r - gray) * saturationFactor;
                    g = gray + (g - gray) * saturationFactor;
                    b = gray + (b - gray) * saturationFactor;
                }

                if (hue != 0f)
                {
                    (r, g, b) = RotateHue(r, g, b, hue / 360f);
                }

                if (temperature != 0f)
                {
                    if (temperature > 0)
                    {
                        r *= 1f + temperature * 0.1f;
                        b *= 1f - temperature * 0.1f;
                    }
                    else
                    {
                        r *= 1f + temperature * 0.1f;
                        b *= 1f - temperature * 0.1f;
                    }
                }

                if (tint != 0f)
                {
                    g *= 1f + tint * 0.1f;
                }

                resultPixels[i] = new Color(
                    Math.Clamp(r, 0f, 1f),
                    Math.Clamp(g, 0f, 1f),
                    Math.Clamp(b, 0f, 1f),
                    c.A
                );
            }

            SetPixels(destination, resultPixels);
        }

        private (float, float, float) RotateHue(float r, float g, float b, float hueShift)
        {
            float h, s, v;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            v = max;

            float d = max - min;
            s = max == 0 ? 0 : d / max;

            if (d == 0) h = 0;
            else if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;

            h /= 6;
            h = (h + hueShift) % 1;
            if (h < 0) h += 1;

            float i = (float)Math.Floor(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch ((int)i % 6)
            {
                case 0: return (v, t, p);
                case 1: return (q, v, p);
                case 2: return (p, v, t);
                case 3: return (p, q, v);
                case 4: return (t, p, v);
                default: return (v, p, q);
            }
        }
    }

    public class SharpenEffect : PostProcessingEffect
    {
        private float amount = 1.0f;
        private float threshold = 0f;
        private bool clamp = true;

        public override string Name => "Sharpen";

        public float Amount
        {
            get => amount;
            set => amount = Math.Max(0f, value);
        }

        public float Threshold
        {
            get => threshold;
            set => threshold = Math.Clamp(value, 0f, 1f);
        }

        public bool Clamp
        {
            get => clamp;
            set => clamp = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            float[] kernel = {
                -1f/8f, -1f/8f, -1f/8f,
                -1f/8f,  1f,    -1f/8f,
                -1f/8f, -1f/8f, -1f/8f
            };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    float r = 0f, g = 0f, b = 0f;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            Color pixel = GetPixel(sourcePixels, width, x + kx, y + ky);
                            float weight = kernel[(ky + 1) * 3 + (kx + 1)];
                            r += pixel.R * weight;
                            g += pixel.G * weight;
                            b += pixel.B * weight;
                        }
                    }

                    Color center = GetPixel(sourcePixels, width, x, y);
                    float grayCenter = center.R * 0.299f + center.G * 0.587f + center.B * 0.114f;

                    r = center.R + r * amount;
                    g = center.G + g * amount;
                    b = center.B + b * amount;

                    float grayResult = r * 0.299f + g * 0.587f + b * 0.114f;

                    if (Math.Abs(grayResult - grayCenter) < threshold)
                    {
                        r = center.R;
                        g = center.G;
                        b = center.B;
                    }

                    if (clamp)
                    {
                        r = Math.Clamp(r, 0f, 1f);
                        g = Math.Clamp(g, 0f, 1f);
                        b = Math.Clamp(b, 0f, 1f);
                    }

                    resultPixels[y * width + x] = new Color(r, g, b, center.A);
                }
            }

            for (int x = 0; x < width; x++)
            {
                resultPixels[x] = sourcePixels[x];
                resultPixels[(height - 1) * width + x] = sourcePixels[(height - 1) * width + x];
            }
            for (int y = 0; y < height; y++)
            {
                resultPixels[y * width] = sourcePixels[y * width];
                resultPixels[y * width + width - 1] = sourcePixels[y * width + width - 1];
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class EdgeDetectionEffect : PostProcessingEffect
    {
        private EdgeDetectionType detectionType = EdgeDetectionType.Sobel;
        private float threshold = 0.1f;
        private Color edgeColor = Color.White;
        private float edgeWidth = 1.0f;
        private bool invertColors = false;
        private bool preserveOriginal = false;

        public override string Name => "Edge Detection";

        public EdgeDetectionType DetectionType
        {
            get => detectionType;
            set => detectionType = value;
        }

        public float Threshold
        {
            get => threshold;
            set => threshold = Math.Max(0f, value);
        }

        public Color EdgeColor
        {
            get => edgeColor;
            set => edgeColor = value;
        }

        public float EdgeWidth
        {
            get => edgeWidth;
            set => edgeWidth = Math.Max(0.1f, Math.Min(3f, value));
        }

        public bool InvertColors
        {
            get => invertColors;
            set => invertColors = value;
        }

        public bool PreserveOriginal
        {
            get => preserveOriginal;
            set => preserveOriginal = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            float[,] edges = new float[width, height];

            if (detectionType == EdgeDetectionType.Sobel)
            {
                ApplySobel(sourcePixels, width, height, edges);
            }
            else
            {
                ApplyLaplacian(sourcePixels, width, height, edges);
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float edge = edges[x, y];

                    if (edge > threshold)
                    {
                        float edgeStrength = Math.Min(1f, (edge - threshold) / (1f - threshold));
                        Color original = GetPixel(sourcePixels, width, x, y);

                        if (preserveOriginal)
                        {
                            Color blended = original.Lerp(edgeColor, edgeStrength * edgeWidth);
                            resultPixels[y * width + x] = blended;
                        }
                        else
                        {
                            resultPixels[y * width + x] = edgeColor.Scale(edgeStrength);
                        }
                    }
                    else
                    {
                        if (invertColors)
                        {
                            Color original = GetPixel(sourcePixels, width, x, y);
                            resultPixels[y * width + x] = original.Invert();
                        }
                        else if (preserveOriginal)
                        {
                            resultPixels[y * width + x] = GetPixel(sourcePixels, width, x, y);
                        }
                        else
                        {
                            resultPixels[y * width + x] = Color.Black;
                        }
                    }
                }
            }

            SetPixels(destination, resultPixels);
        }

        private void ApplySobel(Color[] pixels, int width, int height, float[,] edges)
        {
            float[] sobelX = {
                -1f, 0f, 1f,
                -2f, 0f, 2f,
                -1f, 0f, 1f
            };

            float[] sobelY = {
                -1f, -2f, -1f,
                 0f,  0f,  0f,
                 1f,  2f,  1f
            };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    float gx = 0f, gy = 0f;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            Color pixel = GetPixel(pixels, width, x + kx, y + ky);
                            float luminance = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                            int ki = (ky + 1) * 3 + (kx + 1);

                            gx += luminance * sobelX[ki];
                            gy += luminance * sobelY[ki];
                        }
                    }

                    edges[x, y] = (float)Math.Sqrt(gx * gx + gy * gy);
                }
            }
        }

        private void ApplyLaplacian(Color[] pixels, int width, int height, float[,] edges)
        {
            float[] laplacian = {
                 0f, -1f,  0f,
                -1f,  4f, -1f,
                 0f, -1f,  0f
            };

            float[] laplacianDiag = {
                -1f, -1f, -1f,
                -1f,  8f, -1f,
                -1f, -1f, -1f
            };

            float[] kernel = laplacianDiag;

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    float sum = 0f;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            Color pixel = GetPixel(pixels, width, x + kx, y + ky);
                            float luminance = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                            int ki = (ky + 1) * 3 + (kx + 1);

                            sum += luminance * kernel[ki];
                        }
                    }

                    edges[x, y] = Math.Abs(sum);
                }
            }
        }
    }

    public class DepthOfFieldEffect : PostProcessingEffect
    {
        private float focusDistance = 0.5f;
        private float focalLength = 0.1f;
        private float aperture = 0.5f;
        private DofBlurQuality quality = DofBlurQuality.Medium;
        private Vector2 focusCenter = new Vector2(0.5f, 0.5f);
        private bool useBokeh = true;

        private WriteableBitmap? blurBuffer;

        public override string Name => "Depth of Field";

        public float FocusDistance
        {
            get => focusDistance;
            set => focusDistance = Math.Clamp(value, 0f, 1f);
        }

        public float FocalLength
        {
            get => focalLength;
            set => focalLength = Math.Max(0.01f, value);
        }

        public float Aperture
        {
            get => aperture;
            set => aperture = Math.Clamp(value, 0f, 1f);
        }

        public DofBlurQuality Quality
        {
            get => quality;
            set => quality = value;
        }

        public Vector2 FocusCenter
        {
            get => focusCenter;
            set => focusCenter = value;
        }

        public bool UseBokeh
        {
            get => useBokeh;
            set => useBokeh = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            int maxBlur = quality switch
            {
                DofBlurQuality.Low => 4,
                DofBlurQuality.Medium => 8,
                DofBlurQuality.High => 16,
                _ => 8
            };

            float coc = aperture * 10f;
            float focusX = focusCenter.X * width;
            float focusY = focusCenter.Y * height;

            Random random = new Random();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - focusX;
                    float dy = y - focusY;
                    float distanceFromFocus = (float)Math.Sqrt(dx * dx + dy * dy);

                    float blurRadius = Math.Max(0f, distanceFromFocus - focusDistance * width) / width * coc;
                    blurRadius = Math.Min(blurRadius, maxBlur);

                    if (blurRadius < 0.5f)
                    {
                        resultPixels[y * width + x] = GetPixel(sourcePixels, width, x, y);
                        continue;
                    }

                    float r = 0f, g = 0f, b = 0f;
                    float totalWeight = 0f;

                    int samples = (int)(blurRadius * 2f);
                    samples = Math.Max(4, Math.Min(samples, 32));

                    if (useBokeh)
                    {
                        for (int s = 0; s < samples; s++)
                        {
                            float angle = (float)random.NextDouble() * MathHelper.TwoPi;
                            float radius = (float)random.NextDouble() * blurRadius;

                            int sx = (int)Math.Clamp(x + Math.Cos(angle) * radius, 0, width - 1);
                            int sy = (int)Math.Clamp(y + Math.Sin(angle) * radius, 0, height - 1);

                            Color pixel = GetPixel(sourcePixels, width, sx, sy);
                            float weight = 1f;

                            r += pixel.R * weight;
                            g += pixel.G * weight;
                            b += pixel.B * weight;
                            totalWeight += weight;
                        }
                    }
                    else
                    {
                        for (int sy = -1; sy <= 1; sy++)
                        {
                            for (int sx = -1; sx <= 1; sx++)
                            {
                                int blurX = (int)Math.Clamp(x + sx * blurRadius * 0.5f, 0, width - 1);
                                int blurY = (int)Math.Clamp(y + sy * blurRadius * 0.5f, 0, height - 1);

                                Color pixel = GetPixel(sourcePixels, width, blurX, blurY);
                                r += pixel.R;
                                g += pixel.G;
                                b += pixel.B;
                                totalWeight += 1f;
                            }
                        }
                    }

                    if (totalWeight > 0f)
                    {
                        Color original = GetPixel(sourcePixels, width, x, y);
                        float blendedR = r / totalWeight;
                        float blendedG = g / totalWeight;
                        float blendedB = b / totalWeight;

                        float blendFactor = blurRadius / maxBlur;
                        blendFactor = Math.Min(1f, blendFactor);

                        resultPixels[y * width + x] = new Color(
                            original.R + (blendedR - original.R) * blendFactor,
                            original.G + (blendedG - original.G) * blendFactor,
                            original.B + (blendedB - original.B) * blendFactor,
                            original.A
                        );
                    }
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class MotionBlurEffect : PostProcessingEffect
    {
        private Vector2 direction = new Vector2(1f, 0f);
        private float intensity = 0.5f;
        private int samples = 8;
        private bool radialBlur = false;
        private Vector2 radialCenter = new Vector2(0.5f, 0.5f);

        public override string Name => "Motion Blur";

        public Vector2 Direction
        {
            get => direction;
            set => direction = value;
        }

        public float Intensity
        {
            get => intensity;
            set => intensity = Math.Clamp(value, 0f, 1f);
        }

        public int Samples
        {
            get => samples;
            set => samples = Math.Max(2, Math.Min(32, value));
        }

        public bool RadialBlur
        {
            get => radialBlur;
            set => radialBlur = value;
        }

        public Vector2 RadialCenter
        {
            get => radialCenter;
            set => radialCenter = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            float centerX = radialCenter.X * width;
            float centerY = radialCenter.Y * height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0f, g = 0f, b = 0f;
                    float totalWeight = 0f;

                    for (int s = 0; s < samples; s++)
                    {
                        float t = (float)s / samples * intensity;

                        float sampleX, sampleY;

                        if (radialBlur)
                        {
                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                            float angle = (float)Math.Atan2(dy, dx);

                            sampleX = x - dx * t * 0.1f;
                            sampleY = y - dy * t * 0.1f;
                        }
                        else
                        {
                            sampleX = x - direction.X * t * 10f;
                            sampleY = y - direction.Y * t * 10f;
                        }

                        int sx = (int)Math.Clamp(sampleX, 0, width - 1);
                        int sy = (int)Math.Clamp(sampleY, 0, height - 1);

                        Color pixel = GetPixel(sourcePixels, width, sx, sy);
                        float weight = 1f;

                        r += pixel.R * weight;
                        g += pixel.G * weight;
                        b += pixel.B * weight;
                        totalWeight += weight;
                    }

                    if (totalWeight > 0f)
                    {
                        Color original = GetPixel(sourcePixels, width, x, y);
                        float blendedR = r / totalWeight;
                        float blendedG = g / totalWeight;
                        float blendedB = b / totalWeight;

                        float blendFactor = intensity;
                        blendFactor = Math.Min(1f, blendFactor);

                        resultPixels[y * width + x] = new Color(
                            original.R + (blendedR - original.R) * blendFactor,
                            original.G + (blendedG - original.G) * blendFactor,
                            original.B + (blendedB - original.B) * blendFactor,
                            original.A
                        );
                    }
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class PixelateEffect : PostProcessingEffect
    {
        private int pixelSize = 4;
        private bool mosaicMode = false;

        public override string Name => "Pixelate";

        public int PixelSize
        {
            get => pixelSize;
            set => pixelSize = Math.Max(1, Math.Min(100, value));
        }

        public bool MosaicMode
        {
            get => mosaicMode;
            set => mosaicMode = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            for (int y = 0; y < height; y += pixelSize)
            {
                for (int x = 0; x < width; x += pixelSize)
                {
                    float r = 0f, g = 0f, b = 0f, a = 0f;
                    int count = 0;

                    for (int py = 0; py < pixelSize && y + py < height; py++)
                    {
                        for (int px = 0; px < pixelSize && x + px < width; px++)
                        {
                            Color pixel = GetPixel(sourcePixels, width, x + px, y + py);
                            r += pixel.R;
                            g += pixel.G;
                            b += pixel.B;
                            a += pixel.A;
                            count++;
                        }
                    }

                    if (count > 0)
                    {
                        r /= count;
                        g /= count;
                        b /= count;
                        a /= count;

                        Color avgColor = new Color(r, g, b, a);

                        for (int py = 0; py < pixelSize && y + py < height; py++)
                        {
                            for (int px = 0; px < pixelSize && x + px < width; px++)
                            {
                                int index = (y + py) * width + (x + px);

                                if (mosaicMode)
                                {
                                    resultPixels[index] = avgColor;
                                }
                                else
                                {
                                    Color original = GetPixel(sourcePixels, width, x + px, y + py);
                                    resultPixels[index] = original.Lerp(avgColor, 0.8f);
                                }
                            }
                        }
                    }
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class OutlineEffect : PostProcessingEffect
    {
        private Color outlineColor = Color.Black;
        private float thickness = 1f;
        private float sensitivity = 0.1f;
        private bool includeAlpha = false;

        public override string Name => "Outline";

        public Color OutlineColor
        {
            get => outlineColor;
            set => outlineColor = value;
        }

        public float Thickness
        {
            get => thickness;
            set => thickness = Math.Max(1f, Math.Min(10f, value));
        }

        public float Sensitivity
        {
            get => sensitivity;
            set => sensitivity = Math.Clamp(value, 0f, 1f);
        }

        public bool IncludeAlpha
        {
            get => includeAlpha;
            set => includeAlpha = value;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            int t = Math.Max(1, (int)thickness);

            Array.Copy(sourcePixels, resultPixels, sourcePixels.Length);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color center = GetPixel(sourcePixels, width, x, y);

                    bool isEdge = false;
                    int edgeCount = 0;
                    int totalSamples = 0;

                    for (int dy = -t; dy <= t && !isEdge; dy++)
                    {
                        for (int dx = -t; dx <= t && !isEdge; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            {
                                totalSamples++;
                                continue;
                            }

                            Color neighbor = GetPixel(sourcePixels, width, nx, ny);

                            float diff;
                            if (includeAlpha)
                            {
                                diff = Math.Abs(center.R - neighbor.R) + Math.Abs(center.G - neighbor.G) +
                                       Math.Abs(center.B - neighbor.B) + Math.Abs(center.A - neighbor.A);
                                diff /= 4f;
                            }
                            else
                            {
                                diff = Math.Abs(center.R - neighbor.R) + Math.Abs(center.G - neighbor.G) +
                                       Math.Abs(center.B - neighbor.B);
                                diff /= 3f;
                            }

                            if (diff > sensitivity)
                            {
                                edgeCount++;
                            }
                            totalSamples++;
                        }
                    }

                    if (totalSamples > 0 && edgeCount > 0)
                    {
                        float edgeRatio = (float)edgeCount / totalSamples;
                        if (edgeRatio > 0.25f)
                        {
                            resultPixels[y * width + x] = outlineColor;
                        }
                    }
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class NoiseEffect : PostProcessingEffect
    {
        private float amount = 0.1f;
        private float speed = 1.0f;
        private bool useColorNoise = true;
        private float time;

        public override string Name => "Noise";

        public float Amount
        {
            get => amount;
            set => amount = Math.Clamp(value, 0f, 1f);
        }

        public float Speed
        {
            get => speed;
            set => speed = Math.Max(0f, value);
        }

        public bool UseColorNoise
        {
            get => useColorNoise;
            set => useColorNoise = value;
        }

        public override void Update(float deltaTime)
        {
            time += deltaTime * speed;
        }

        public override void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            Color[] sourcePixels = GetPixels(source);
            Color[] resultPixels = new Color[sourcePixels.Length];

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            Random random = new Random((int)(time * 1000));

            for (int i = 0; i < sourcePixels.Length; i++)
            {
                Color pixel = sourcePixels[i];

                float noise = (float)(random.NextDouble() * 2.0 - 1.0);

                if (useColorNoise)
                {
                    float noiseR = (float)(random.NextDouble() * 2.0 - 1.0);
                    float noiseG = (float)(random.NextDouble() * 2.0 - 1.0);
                    float noiseB = (float)(random.NextDouble() * 2.0 - 1.0);

                    float r = Math.Clamp(pixel.R + noiseR * amount, 0f, 1f);
                    float g = Math.Clamp(pixel.G + noiseG * amount, 0f, 1f);
                    float b = Math.Clamp(pixel.B + noiseB * amount, 0f, 1f);

                    resultPixels[i] = new Color(r, g, b, pixel.A);
                }
                else
                {
                    float gray = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                    gray = Math.Clamp(gray + noise * amount, 0f, 1f);

                    resultPixels[i] = new Color(gray, gray, gray, pixel.A);
                }
            }

            SetPixels(destination, resultPixels);
        }
    }

    public class EffectComposer : IDisposable
    {
        private readonly List<IPostProcessingEffect> effects = new List<IPostProcessingEffect>();
        private readonly Dictionary<string, IPostProcessingEffect> effectMap = new Dictionary<string, IPostProcessingEffect>();
        private WriteableBitmap? renderTarget1;
        private WriteableBitmap? renderTarget2;
        private int width = 1920;
        private int height = 1080;
        private float deltaTime;

        public IReadOnlyList<IPostProcessingEffect> Effects => effects;

        public int Width
        {
            get => width;
            set
            {
                if (width != value)
                {
                    width = value;
                    InvalidateBuffers();
                }
            }
        }

        public int Height
        {
            get => height;
            set
            {
                if (height != value)
                {
                    height = value;
                    InvalidateBuffers();
                }
            }
        }

        public void AddEffect(IPostProcessingEffect effect)
        {
            if (effect == null)
                return;

            effects.Add(effect);
            effectMap[effect.Name] = effect;
            SortEffects();
        }

        public void RemoveEffect(string name)
        {
            if (effectMap.TryGetValue(name, out var effect))
            {
                effects.Remove(effect);
                effectMap.Remove(name);
            }
        }

        public void RemoveEffect(IPostProcessingEffect effect)
        {
            if (effects.Remove(effect))
            {
                effectMap.Remove(effect.Name);
            }
        }

        public T? GetEffect<T>() where T : class, IPostProcessingEffect
        {
            foreach (var effect in effects)
            {
                if (effect is T typedEffect)
                    return typedEffect;
            }
            return null;
        }

        public T? GetEffect<T>(string name) where T : class, IPostProcessingEffect
        {
            if (effectMap.TryGetValue(name, out var effect) && effect is T typedEffect)
                return typedEffect;
            return null;
        }

        public bool HasEffect<T>() where T : class, IPostProcessingEffect
        {
            return GetEffect<T>() != null;
        }

        public void ClearEffects()
        {
            effects.Clear();
            effectMap.Clear();
        }

        public void Update(float deltaTime)
        {
            this.deltaTime = deltaTime;
            foreach (var effect in effects)
            {
                if (effect.IsEnabled)
                    effect.Update(deltaTime);
            }
        }

        public void Apply(WriteableBitmap source, WriteableBitmap destination)
        {
            if (effects.Count == 0)
            {
                CopyPixels(source, destination);
                return;
            }

            EnsureBuffers();

            var enabledEffects = GetEnabledEffects();
            if (enabledEffects.Count == 0)
            {
                CopyPixels(source, destination);
                return;
            }

            if (enabledEffects.Count == 1)
            {
                enabledEffects[0].Apply(source, destination);
                return;
            }

            WriteableBitmap? currentSource = source;
            WriteableBitmap? currentTarget = renderTarget1;

            for (int i = 0; i < enabledEffects.Count; i++)
            {
                bool isLast = i == enabledEffects.Count - 1;

                if (isLast)
                {
                    enabledEffects[i].Apply(currentSource!, destination);
                }
                else
                {
                    enabledEffects[i].Apply(currentSource!, currentTarget!);
                    (currentSource, currentTarget) = (currentTarget, currentSource);
                }
            }
        }

        public void Apply(WriteableBitmap source, WriteableBitmap destination, Rect region)
        {
            int x = (int)region.X;
            int y = (int)region.Y;
            int w = (int)region.Width;
            int h = (int)region.Height;

            x = Math.Max(0, Math.Min(x, width - 1));
            y = Math.Max(0, Math.Min(y, height - 1));
            w = Math.Max(1, Math.Min(w, width - x));
            h = Math.Max(1, Math.Min(h, height - y));

            Color[] sourcePixels = GetPixels(source);
            Color[] regionPixels = new Color[w * h];

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    int srcX = x + px;
                    int srcY = y + py;
                    int srcIndex = srcY * width + srcX;
                    int dstIndex = py * w + px;

                    if (srcIndex >= 0 && srcIndex < sourcePixels.Length)
                        regionPixels[dstIndex] = sourcePixels[srcIndex];
                }
            }

            using (var regionBitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null))
            {
                SetPixels(regionBitmap, regionPixels);

                using (var outputBitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null))
                {
                    Apply(regionBitmap, outputBitmap);

                    Color[] outputPixels = GetPixels(outputBitmap);

                    for (int py = 0; py < h; py++)
                    {
                        for (int px = 0; px < w; px++)
                        {
                            int srcIndex = py * w + px;
                            int dstX = x + px;
                            int dstY = y + py;
                            int dstIndex = dstY * width + dstX;

                            if (dstIndex >= 0 && dstIndex < sourcePixels.Length)
                                sourcePixels[dstIndex] = outputPixels[srcIndex];
                        }
                    }
                }
            }

            SetPixels(destination, sourcePixels);
        }

        private List<IPostProcessingEffect> GetEnabledEffects()
        {
            var enabled = new List<IPostProcessingEffect>();
            foreach (var effect in effects)
            {
                if (effect.IsEnabled)
                    enabled.Add(effect);
            }
            return enabled;
        }

        private void SortEffects()
        {
            effects.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        private void EnsureBuffers()
        {
            if (renderTarget1 == null || renderTarget1.PixelWidth != width || renderTarget1.PixelHeight != height)
            {
                renderTarget1?.Dispose();
                renderTarget2?.Dispose();

                renderTarget1 = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                renderTarget2 = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            }
        }

        private void InvalidateBuffers()
        {
            renderTarget1?.Dispose();
            renderTarget2?.Dispose();
            renderTarget1 = null;
            renderTarget2 = null;
        }

        public void Dispose()
        {
            InvalidateBuffers();
            effects.Clear();
            effectMap.Clear();
        }
    }

    public static class ScreenBlendModeHelper
    {
        public static Color Blend(Color baseColor, Color blendColor, ScreenBlendMode mode)
        {
            float r, g, b, a;

            switch (mode)
            {
                case ScreenBlendMode.Normal:
                    r = blendColor.R;
                    g = blendColor.G;
                    b = blendColor.B;
                    a = blendColor.A;
                    break;

                case ScreenBlendMode.Multiply:
                    r = baseColor.R * blendColor.R;
                    g = baseColor.G * blendColor.G;
                    b = baseColor.B * blendColor.B;
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Screen:
                    r = 1f - (1f - baseColor.R) * (1f - blendColor.R);
                    g = 1f - (1f - baseColor.G) * (1f - blendColor.G);
                    b = 1f - (1f - baseColor.B) * (1f - blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Overlay:
                    r = OverlayChannel(baseColor.R, blendColor.R);
                    g = OverlayChannel(baseColor.G, blendColor.G);
                    b = OverlayChannel(baseColor.B, blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Darken:
                    r = Math.Min(baseColor.R, blendColor.R);
                    g = Math.Min(baseColor.G, blendColor.G);
                    b = Math.Min(baseColor.B, blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Lighten:
                    r = Math.Max(baseColor.R, blendColor.R);
                    g = Math.Max(baseColor.G, blendColor.G);
                    b = Math.Max(baseColor.B, blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.ColorDodge:
                    r = ColorDodgeChannel(baseColor.R, blendColor.R);
                    g = ColorDodgeChannel(baseColor.G, blendColor.G);
                    b = ColorDodgeChannel(baseColor.B, blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.ColorBurn:
                    r = ColorBurnChannel(baseColor.R, blendColor.R);
                    g = ColorBurnChannel(baseColor.G, blendColor.G);
                    b = ColorBurnChannel(baseColor.B, blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.HardLight:
                    r = HardLightChannel(baseColor.R, blendColor.R);
                    g = HardLightChannel(baseColor.G, blendColor.G);
                    b = HardLightChannel(baseColor.B, blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.SoftLight:
                    r = SoftLightChannel(baseColor.R, blendColor.R);
                    g = SoftLightChannel(baseColor.G, blendColor.G);
                    b = SoftLightChannel(baseColor.B, blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Difference:
                    r = Math.Abs(baseColor.R - blendColor.R);
                    g = Math.Abs(baseColor.G - blendColor.G);
                    b = Math.Abs(baseColor.B - blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Exclusion:
                    r = baseColor.R + blendColor.R - 2f * baseColor.R * blendColor.R;
                    g = baseColor.G + blendColor.G - 2f * baseColor.G * blendColor.G;
                    b = baseColor.B + blendColor.B - 2f * baseColor.B * blendColor.B;
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Subtract:
                    r = Math.Max(0f, baseColor.R - blendColor.R);
                    g = Math.Max(0f, baseColor.G - blendColor.G);
                    b = Math.Max(0f, baseColor.B - blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.Add:
                    r = Math.Min(1f, baseColor.R + blendColor.R);
                    g = Math.Min(1f, baseColor.G + blendColor.G);
                    b = Math.Min(1f, baseColor.B + blendColor.B);
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.GrainExtract:
                    r = baseColor.R - blendColor.R + 0.5f;
                    g = baseColor.G - blendColor.G + 0.5f;
                    b = baseColor.B - blendColor.B + 0.5f;
                    a = baseColor.A * blendColor.A;
                    break;

                case ScreenBlendMode.GrainMerge:
                    r = baseColor.R + blendColor.R - 0.5f;
                    g = baseColor.G + blendColor.G - 0.5f;
                    b = baseColor.B + blendColor.B - 0.5f;
                    a = baseColor.A * blendColor.A;
                    break;

                default:
                    r = blendColor.R;
                    g = blendColor.G;
                    b = blendColor.B;
                    a = blendColor.A;
                    break;
            }

            return new Color(
                Math.Clamp(r, 0f, 1f),
                Math.Clamp(g, 0f, 1f),
                Math.Clamp(b, 0f, 1f),
                Math.Clamp(a, 0f, 1f)
            );
        }

        private static float OverlayChannel(float baseChannel, float blendChannel)
        {
            if (baseChannel < 0.5f)
                return 2f * baseChannel * blendChannel;
            else
                return 1f - 2f * (1f - baseChannel) * (1f - blendChannel);
        }

        private static float ColorDodgeChannel(float baseChannel, float blendChannel)
        {
            if (blendChannel >= 1f)
                return 1f;
            else
                return Math.Min(1f, baseChannel / (1f - blendChannel));
        }

        private static float ColorBurnChannel(float baseChannel, float blendChannel)
        {
            if (blendChannel <= 0f)
                return 0f;
            else
                return Math.Max(0f, 1f - (1f - baseChannel) / blendChannel);
        }

        private static float HardLightChannel(float baseChannel, float blendChannel)
        {
            if (blendChannel < 0.5f)
                return 2f * baseChannel * blendChannel;
            else
                return 1f - 2f * (1f - baseChannel) * (1f - blendChannel);
        }

        private static float SoftLightChannel(float baseChannel, float blendChannel)
        {
            if (blendChannel < 0.5f)
                return 2f * baseChannel * blendChannel + baseChannel * baseChannel * (1f - 2f * blendChannel);
            else
                return (float)Math.Sqrt(baseChannel) * (2f * blendChannel - 1f) + 2f * baseChannel * (1f - blendChannel);
        }

        public static Color[] BlendLayer(Color[] baseLayer, Color[] blendLayer, ScreenBlendMode mode)
        {
            if (baseLayer.Length != blendLayer.Length)
                throw new ArgumentException("Layers must have the same size");

            Color[] result = new Color[baseLayer.Length];
            for (int i = 0; i < baseLayer.Length; i++)
            {
                result[i] = Blend(baseLayer[i], blendLayer[i], mode);
            }
            return result;
        }
    }

    public class EffectPreset
    {
        public string Name { get; set; } = string.Empty;
        public List<EffectSetting> Settings { get; set; } = new List<EffectSetting>();

        public class EffectSetting
        {
            public string EffectName { get; set; } = string.Empty;
            public string PropertyName { get; set; } = string.Empty;
            public object Value { get; set; } = 0f;
        }
    }

    public static class EffectPresets
    {
        public static EffectPreset CreateVignettePreset(float intensity = 0.5f, float smoothness = 0.5f)
        {
            return new EffectPreset
            {
                Name = "Vignette",
                Settings = new List<EffectPreset.EffectSetting>
                {
                    new() { EffectName = "Vignette", PropertyName = "Intensity", Value = intensity },
                    new() { EffectName = "Vignette", PropertyName = "Smoothness", Value = smoothness }
                }
            };
        }

        public static EffectPreset CreateBloomPreset(float threshold = 0.8f, float intensity = 1f, float radius = 0.5f)
        {
            return new EffectPreset
            {
                Name = "Bloom",
                Settings = new List<EffectPreset.EffectSetting>
                {
                    new() { EffectName = "Bloom", PropertyName = "Threshold", Value = threshold },
                    new() { EffectName = "Bloom", PropertyName = "Intensity", Value = intensity },
                    new() { EffectName = "Bloom", PropertyName = "Radius", Value = radius }
                }
            };
        }

        public static EffectPreset CreateCinematicPreset()
        {
            return new EffectPreset
            {
                Name = "Cinematic",
                Settings = new List<EffectPreset.EffectSetting>
                {
                    new() { EffectName = "Color Correction", PropertyName = "Contrast", Value = 0.2f },
                    new() { EffectName = "Color Correction", PropertyName = "Saturation", Value = -0.1f },
                    new() { EffectName = "Color Correction", PropertyName = "Temperature", Value = -0.1f },
                    new() { EffectName = "Vignette", PropertyName = "Intensity", Value = 0.4f },
                    new() { EffectName = "Vignette", PropertyName = "Smoothness", Value = 0.6f },
                    new() { EffectName = "Film Grain", PropertyName = "Intensity", Value = 0.05f },
                    new() { EffectName = "Film Grain", PropertyName = "Animated", Value = true }
                }
            };
        }

        public static EffectPreset CreateRetroPreset()
        {
            return new EffectPreset
            {
                Name = "Retro",
                Settings = new List<EffectPreset.EffectSetting>
                {
                    new() { EffectName = "Pixelate", PropertyName = "PixelSize", Value = 4 },
                    new() { EffectName = "Color Correction", PropertyName = "Saturation", Value = -0.3f },
                    new() { EffectName = "Color Correction", PropertyName = "Contrast", Value = 0.3f },
                    new() { EffectName = "Film Grain", PropertyName = "Intensity", Value = 0.15f }
                }
            };
        }

        public static EffectPreset CreateDreamyPreset()
        {
            return new EffectPreset
            {
                Name = "Dreamy",
                Settings = new List<EffectPreset.EffectSetting>
                {
                    new() { EffectName = "Bloom", PropertyName = "Threshold", Value = 0.6f },
                    new() { EffectName = "Bloom", PropertyName = "Intensity", Value = 0.5f },
                    new() { EffectName = "Blur", PropertyName = "BlurType", Value = BlurType.Gaussian },
                    new() { EffectName = "Blur", PropertyName = "Radius", Value = 2f },
                    new() { EffectName = "Color Correction", PropertyName = "Saturation", Value = 0.2f },
                    new() { EffectName = "Bloom", PropertyName = "Radius", Value = 0.3f }
                }
            };
        }

        public static EffectPreset CreateHorrorPreset()
        {
            return new EffectPreset
            {
                Name = "Horror",
                Settings = new List<EffectPreset.EffectSetting>
                {
                    new() { EffectName = "Color Correction", PropertyName = "Saturation", Value = -0.5f },
                    new() { EffectName = "Color Correction", PropertyName = "Brightness", Value = -0.2f },
                    new() { EffectName = "Vignette", PropertyName = "Intensity", Value = 0.7f },
                    new() { EffectName = "Vignette", PropertyName = "Smoothness", Value = 0.3f },
                    new() { EffectName = "Film Grain", PropertyName = "Intensity", Value = 0.2f },
                    new() { EffectName = "Film Grain", PropertyName = "Animated", Value = true }
                }
            };
        }
    }
}

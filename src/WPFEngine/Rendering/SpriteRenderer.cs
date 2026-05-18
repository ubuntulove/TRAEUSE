using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFEngine.ECS;
using WPFEngine.Scene;

namespace WPFEngine.Rendering
{
    public enum SpriteSortMode
    {
        Immediate,
        Deferred,
        Depth,
        BackToFront,
        FrontToBack
    }

    public enum SpriteFlipMode
    {
        None,
        Horizontal,
        Vertical,
        Both
    }

    public class SpriteRenderer : Component, IDisposable
    {
        private ImageSource spriteImage;
        private ImageSource normalMap;
        private Color color;
        private Vector2 size;
        private Vector2 pivot;
        private Vector2 tileSize;
        private Vector2 tileOffset;
        private int sortingLayer;
        private int orderInLayer;
        private float drawDepth;
        private bool isVisible;
        private bool flipX;
        private bool flipY;
        private SpriteFlipMode flipMode;
        private Material material;
        private BlendMode blendMode;
        private float alpha;
        private float hue;
        private float saturation;
        private float brightness;
        private bool allowOcclusion;
        private bool raycastTarget;
        private bool useLegacyColor;
        private string spritePath;
        private int frameIndex;
        private int frameCount;
        private float frameRate;
        private float frameTimer;
        private bool isPlaying;
        private SpriteAnimation currentAnimation;
        private Rect sourceRect;

        public event EventHandler<SpriteRenderEventArgs> SpriteChanged;
        public event EventHandler<SpriteRenderEventArgs> VisibilityChanged;
        public event EventHandler<SpriteRenderEventArgs> AnimationCompleted;

        public ImageSource Sprite
        {
            get => spriteImage;
            set
            {
                if (spriteImage != value)
                {
                    spriteImage = value;
                    OnSpriteChanged();
                }
            }
        }

        public ImageSource NormalMap
        {
            get => normalMap;
            set
            {
                if (normalMap != value)
                {
                    normalMap = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vector2 Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vector2 Pivot
        {
            get => pivot;
            set
            {
                if (pivot != value)
                {
                    pivot = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vector2 TileSize
        {
            get => tileSize;
            set
            {
                if (tileSize != value)
                {
                    tileSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vector2 TileOffset
        {
            get => tileOffset;
            set
            {
                if (tileOffset != value)
                {
                    tileOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SortingLayer
        {
            get => sortingLayer;
            set
            {
                if (sortingLayer != value)
                {
                    sortingLayer = value;
                    OnPropertyChanged();
                }
            }
        }

        public int OrderInLayer
        {
            get => orderInLayer;
            set
            {
                if (orderInLayer != value)
                {
                    orderInLayer = value;
                    OnPropertyChanged();
                }
            }
        }

        public float DrawDepth
        {
            get => drawDepth;
            set
            {
                if (Math.Abs(drawDepth - value) > 0.0001f)
                {
                    drawDepth = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    VisibilityChanged?.Invoke(this, new SpriteRenderEventArgs(this));
                    OnPropertyChanged();
                }
            }
        }

        public SpriteFlipMode FlipMode
        {
            get => flipMode;
            set
            {
                if (flipMode != value)
                {
                    flipMode = value;
                    UpdateFlipState();
                    OnPropertyChanged();
                }
            }
        }

        public Material Material
        {
            get => material;
            set
            {
                if (material != value)
                {
                    material = value;
                    OnPropertyChanged();
                }
            }
        }

        public BlendMode BlendMode
        {
            get => blendMode;
            set
            {
                if (blendMode != value)
                {
                    blendMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Alpha
        {
            get => alpha;
            set
            {
                var clampedAlpha = Math.Max(0f, Math.Min(1f, value));
                if (Math.Abs(alpha - clampedAlpha) > 0.0001f)
                {
                    alpha = clampedAlpha;
                    OnPropertyChanged();
                }
            }
        }

        public float Hue
        {
            get => hue;
            set
            {
                var normalizedHue = value % 360f;
                if (normalizedHue < 0) normalizedHue += 360f;
                if (Math.Abs(hue - normalizedHue) > 0.0001f)
                {
                    hue = normalizedHue;
                    OnPropertyChanged();
                }
            }
        }

        public float Saturation
        {
            get => saturation;
            set
            {
                var clampedSat = Math.Max(-1f, Math.Min(1f, value));
                if (Math.Abs(saturation - clampedSat) > 0.0001f)
                {
                    saturation = clampedSat;
                    OnPropertyChanged();
                }
            }
        }

        public float Brightness
        {
            get => brightness;
            set
            {
                var clampedBright = Math.Max(-1f, Math.Min(1f, value));
                if (Math.Abs(brightness - clampedBright) > 0.0001f)
                {
                    brightness = clampedBright;
                    OnPropertyChanged();
                }
            }
        }

        public bool AllowOcclusion
        {
            get => allowOcclusion;
            set
            {
                if (allowOcclusion != value)
                {
                    allowOcclusion = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool RaycastTarget
        {
            get => raycastTarget;
            set
            {
                if (raycastTarget != value)
                {
                    raycastTarget = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool FlipX
        {
            get => flipX;
            set
            {
                if (flipX != value)
                {
                    flipX = value;
                    UpdateFlipMode();
                    OnPropertyChanged();
                }
            }
        }

        public bool FlipY
        {
            get => flipY;
            set
            {
                if (flipY != value)
                {
                    flipY = value;
                    UpdateFlipMode();
                    OnPropertyChanged();
                }
            }
        }

        public Rect SourceRect
        {
            get => sourceRect;
            set
            {
                if (sourceRect != value)
                {
                    sourceRect = value;
                    OnPropertyChanged();
                }
            }
        }

        public int FrameIndex
        {
            get => frameIndex;
            set
            {
                if (frameIndex != value)
                {
                    frameIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public float FrameRate
        {
            get => frameRate;
            set
            {
                if (Math.Abs(frameRate - value) > 0.0001f)
                {
                    frameRate = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPlaying => isPlaying;

        public SpriteAnimation CurrentAnimation => currentAnimation;

        public SpriteRenderer()
        {
            color = Colors.White;
            size = new Vector2(100, 100);
            pivot = new Vector2(0.5f, 0.5f);
            tileSize = new Vector2(100, 100);
            tileOffset = Vector2.Zero;
            sortingLayer = 0;
            orderInLayer = 0;
            drawDepth = 0.5f;
            isVisible = true;
            flipMode = SpriteFlipMode.None;
            blendMode = BlendMode.Alpha;
            alpha = 1f;
            hue = 0f;
            saturation = 0f;
            brightness = 0f;
            allowOcclusion = true;
            raycastTarget = true;
            frameIndex = 0;
            frameCount = 1;
            frameRate = 12f;
            frameTimer = 0f;
            isPlaying = false;
            sourceRect = new Rect(0, 0, 100, 100);
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            if (Transform != null)
            {
                Transform.PositionChanged += OnTransformChanged;
                Transform.ScaleChanged += OnTransformChanged;
                Transform.RotationChanged += OnTransformChanged;
            }

            CalculateBounds();
        }

        private void OnTransformChanged(object sender, TransformEventArgs e)
        {
            CalculateBounds();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (isPlaying && currentAnimation != null && frameRate > 0)
            {
                frameTimer += deltaTime;
                var frameDuration = 1f / frameRate;

                if (frameTimer >= frameDuration)
                {
                    frameTimer -= frameDuration;
                    AdvanceFrame();
                }
            }
        }

        private void AdvanceFrame()
        {
            if (currentAnimation == null || currentAnimation.Frames.Count == 0)
                return;

            frameIndex++;
            if (frameIndex >= currentAnimation.Frames.Count)
            {
                if (currentAnimation.IsLooping)
                {
                    frameIndex = 0;
                }
                else
                {
                    frameIndex = currentAnimation.Frames.Count - 1;
                    Stop();
                    AnimationCompleted?.Invoke(this, new SpriteRenderEventArgs(this));
                }
            }

            UpdateSpriteFromAnimation();
        }

        private void UpdateSpriteFromAnimation()
        {
            if (currentAnimation == null || frameIndex >= currentAnimation.Frames.Count)
                return;

            var frame = currentAnimation.Frames[frameIndex];
            if (frame.Sprite != null)
            {
                Sprite = frame.Sprite;
            }
            if (frame.Duration > 0)
            {
                FrameRate = 1f / frame.Duration;
            }
        }

        public void Play()
        {
            if (currentAnimation != null && currentAnimation.Frames.Count > 0)
            {
                isPlaying = true;
                frameTimer = 0f;
                if (frameIndex >= currentAnimation.Frames.Count)
                    frameIndex = 0;
            }
        }

        public void Play(SpriteAnimation animation)
        {
            currentAnimation = animation;
            frameIndex = 0;
            frameCount = animation?.Frames.Count ?? 1;
            isPlaying = true;
            frameTimer = 0f;
            UpdateSpriteFromAnimation();
        }

        public void Stop()
        {
            isPlaying = false;
            frameTimer = 0f;
        }

        public void Pause()
        {
            isPlaying = false;
        }

        public void Resume()
        {
            if (currentAnimation != null)
                isPlaying = true;
        }

        public void SetFrame(int frame)
        {
            if (currentAnimation == null)
                return;

            frameIndex = Math.Max(0, Math.Min(frame, currentAnimation.Frames.Count - 1));
            UpdateSpriteFromAnimation();
        }

        public void LoadSprite(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                spriteImage = bitmap;
                spritePath = path;

                if (bitmap.Width > 0 && bitmap.Height > 0)
                {
                    size = new Vector2((float)bitmap.Width, (float)bitmap.Height);
                    sourceRect = new Rect(0, 0, bitmap.Width, bitmap.Height);
                }

                OnSpriteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load sprite from {path}: {ex.Message}");
            }
        }

        public void LoadSpriteFromResource(string resourcePath)
        {
            try
            {
                var uri = new Uri(resourcePath, UriKind.Relative);
                var bitmap = new BitmapImage(uri);
                bitmap.Freeze();

                spriteImage = bitmap;
                spritePath = resourcePath;

                if (bitmap.Width > 0 && bitmap.Height > 0)
                {
                    size = new Vector2((float)bitmap.Width, (float)bitmap.Height);
                    sourceRect = new Rect(0, 0, bitmap.Width, bitmap.Height);
                }

                OnSpriteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load sprite from resource {resourcePath}: {ex.Message}");
            }
        }

        private void OnSpriteChanged()
        {
            CalculateBounds();
            SpriteChanged?.Invoke(this, new SpriteRenderEventArgs(this));
        }

        private void CalculateBounds()
        {
            if (Transform == null)
                return;

            var worldScale = Transform.WorldScale;
            var worldPos = Transform.WorldPosition;
            var boundsSize = new Vector2(size.X * worldScale.X, size.Y * worldScale.Y);
            Bounds = new Bounds(worldPos, boundsSize);
        }

        private void UpdateFlipState()
        {
            flipX = flipMode == SpriteFlipMode.Horizontal || flipMode == SpriteFlipMode.Both;
            flipY = flipMode == SpriteFlipMode.Vertical || flipMode == SpriteFlipMode.Both;
        }

        private void UpdateFlipMode()
        {
            if (flipX && flipY)
                flipMode = SpriteFlipMode.Both;
            else if (flipX)
                flipMode = SpriteFlipMode.Horizontal;
            else if (flipY)
                flipMode = SpriteFlipMode.Vertical;
            else
                flipMode = SpriteFlipMode.None;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public Bounds GetWorldBounds()
        {
            if (Transform == null)
                return new Bounds(Vector2.Zero, size);

            var worldPos = Transform.WorldPosition;
            var worldScale = Transform.WorldScale;
            return new Bounds(worldPos, new Vector2(size.X * worldScale.X, size.Y * worldScale.Y));
        }

        public Rect GetScreenRect(Camera camera)
        {
            var worldBounds = GetWorldBounds();
            if (camera == null)
                return new Rect(worldBounds.Center.X - worldBounds.Size.X / 2,
                              worldBounds.Center.Y - worldBounds.Size.Y / 2,
                              worldBounds.Size.X, worldBounds.Size.Y);

            var cameraWorldPos = camera.Transform?.WorldPosition ?? Vector2.Zero;
            var cameraZoom = camera.Zoom;

            return new Rect(
                (worldBounds.Center.X - cameraWorldPos.X) * cameraZoom + camera.ViewportWidth / 2,
                (worldBounds.Center.Y - cameraWorldPos.Y) * cameraZoom + camera.ViewportHeight / 2,
                worldBounds.Size.X * cameraZoom,
                worldBounds.Size.Y * cameraZoom
            );
        }

        public Color GetFinalColor()
        {
            var finalColor = color;
            if (alpha < 1f)
            {
                finalColor.A = (byte)(finalColor.A * alpha);
            }
            return finalColor;
        }

        public int GetSortingScore()
        {
            return sortingLayer * 1000000 + (int)(drawDepth * 10000) + orderInLayer;
        }

        public override string ToString()
        {
            return $"SpriteRenderer({Sprite?.ToString() ?? "null"}, Layer={SortingLayer}, Order={OrderInLayer})";
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
                SpriteChanged = null;
                VisibilityChanged = null;
                AnimationCompleted = null;
                currentAnimation = null;
                spriteImage = null;
                normalMap = null;
                material = null;
            }
        }
    }

    public enum BlendMode
    {
        Alpha,
        Additive,
        Multiply,
        Screen,
        Overlay
    }

    public class SpriteRenderEventArgs : EventArgs
    {
        public SpriteRenderer Renderer { get; }

        public SpriteRenderEventArgs(SpriteRenderer renderer)
        {
            Renderer = renderer;
        }
    }

    public class SpriteAnimation : IDisposable
    {
        public string Name { get; set; }
        public List<SpriteFrame> Frames { get; private set; }
        public bool IsLooping { get; set; }
        public float FrameRate { get; set; }

        public SpriteAnimation()
        {
            Frames = new List<SpriteFrame>();
            IsLooping = true;
            FrameRate = 12f;
        }

        public SpriteAnimation(string name, int frameCount) : this()
        {
            Name = name;
        }

        public void AddFrame(ImageSource sprite, float duration = 0.1f)
        {
            Frames.Add(new SpriteFrame { Sprite = sprite, Duration = duration });
        }

        public void AddFrame(SpriteFrame frame)
        {
            Frames.Add(frame);
        }

        public float GetDuration()
        {
            float total = 0f;
            foreach (var frame in Frames)
            {
                total += frame.Duration;
            }
            return total;
        }

        public void Dispose()
        {
            Frames?.Clear();
            Frames = null;
        }
    }

    public class SpriteFrame
    {
        public ImageSource Sprite { get; set; }
        public float Duration { get; set; }
        public Rect SourceRect { get; set; }
        public Vector2 Pivot { get; set; }
        public Vector2 Offset { get; set; }

        public SpriteFrame()
        {
            Duration = 0.1f;
            Pivot = new Vector2(0.5f, 0.5f);
            Offset = Vector2.Zero;
        }

        public SpriteFrame(ImageSource sprite, float duration = 0.1f)
        {
            Sprite = sprite;
            Duration = duration;
            Pivot = new Vector2(0.5f, 0.5f);
            Offset = Vector2.Zero;
        }
    }

    public class Material : IDisposable
    {
        public string Name { get; set; }
        public Color BaseColor { get; set; }
        public ImageSource MainTexture { get; set; }
        public ImageSource NormalMap { get; set; }
        public float Metallic { get; set; }
        public float Smoothness { get; set; }
        public float NormalScale { get; set; }
        public BlendMode BlendMode { get; set; }
        public CullMode CullMode { get; set; }
        public MaterialRenderQueue RenderQueue { get; set; }

        public Material()
        {
            BaseColor = Colors.White;
            BlendMode = BlendMode.Alpha;
            CullMode = CullMode.Off;
            RenderQueue = MaterialRenderQueue.Geometry;
            Metallic = 0f;
            Smoothness = 0.5f;
            NormalScale = 1f;
        }

        public Material(Material other) : this()
        {
            if (other == null)
                return;

            Name = other.Name;
            BaseColor = other.BaseColor;
            MainTexture = other.MainTexture;
            NormalMap = other.NormalMap;
            Metallic = other.Metallic;
            Smoothness = other.Smoothness;
            NormalScale = other.NormalScale;
            BlendMode = other.BlendMode;
            CullMode = other.CullMode;
            RenderQueue = other.RenderQueue;
        }

        public virtual void Dispose()
        {
            MainTexture = null;
            NormalMap = null;
        }
    }

    public enum CullMode
    {
        Off,
        Front,
        Back
    }

    public enum MaterialRenderQueue
    {
        Background = 1000,
        Geometry = 2000,
        AlphaTest = 2450,
        Transparent = 3000,
        Overlay = 4000
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFEngine.ECS;
using WPFEngine.Scene;

namespace WPFEngine.Rendering.Lighting
{
    public enum LightType
    {
        Point,
        Spot,
        Directional,
        Global
    }

    public enum ShadowType
    {
        None,
        Hard,
        Soft
    }

    public enum LightBlendMode
    {
        Add,
        Multiply,
        Max
    }

    [Flags]
    public enum LightLayer
    {
        Layer0 = 1 << 0,
        Layer1 = 1 << 1,
        Layer2 = 1 << 2,
        Layer3 = 1 << 3,
        Layer4 = 1 << 4,
        Layer5 = 1 << 5,
        Layer6 = 1 << 6,
        Layer7 = 1 << 7,
        Layer8 = 1 << 8,
        Layer9 = 1 << 9,
        Layer10 = 1 << 10,
        Layer11 = 1 << 11,
        Layer12 = 1 << 12,
        Layer13 = 1 << 13,
        Layer14 = 1 << 14,
        Layer15 = 1 << 15,
        Layer16 = 1 << 16,
        Layer17 = 1 << 17,
        Layer18 = 1 << 18,
        Layer19 = 1 << 19,
        Layer20 = 1 << 20,
        Layer21 = 1 << 21,
        Layer22 = 1 << 22,
        Layer23 = 1 << 23,
        Layer24 = 1 << 24,
        Layer25 = 1 << 25,
        Layer26 = 1 << 26,
        Layer27 = 1 << 27,
        Layer28 = 1 << 28,
        Layer29 = 1 << 29,
        Layer30 = 1 << 30,
        Layer31 = 1 << 31,
        Everything = ~0
    }

    public struct LightLayerMask
    {
        private int _mask;

        public static LightLayerMask Everything => new LightLayerMask { _mask = -1 };
        public static LightLayerMask Nothing => new LightLayerMask { _mask = 0 };

        public static LightLayerMask FromLayer(LightLayer layer)
        {
            return new LightLayerMask { _mask = (int)layer };
        }

        public static LightLayerMask FromLayers(params LightLayer[] layers)
        {
            int mask = 0;
            foreach (var layer in layers)
            {
                mask |= (int)layer;
            }
            return new LightLayerMask { _mask = mask };
        }

        public bool Contains(LightLayer layer)
        {
            return (_mask & (int)layer) != 0;
        }

        public bool Contains(LightLayerMask other)
        {
            return (_mask & other._mask) == other._mask;
        }

        public LightLayerMask Add(LightLayer layer)
        {
            return new LightLayerMask { _mask = _mask | (int)layer };
        }

        public LightLayerMask Remove(LightLayer layer)
        {
            return new LightLayerMask { _mask = _mask & ~(int)layer };
        }

        public static LightLayerMask operator &(LightLayerMask a, LightLayerMask b)
        {
            return new LightLayerMask { _mask = a._mask & b._mask };
        }

        public static LightLayerMask operator |(LightLayerMask a, LightLayerMask b)
        {
            return new LightLayerMask { _mask = a._mask | b._mask };
        }

        public static LightLayerMask operator ~(LightLayerMask a)
        {
            return new LightLayerMask { _mask = ~a._mask };
        }

        public static implicit operator int(LightLayerMask mask) => mask._mask;
        public static implicit operator LightLayerMask(int mask) => new LightLayerMask { _mask = mask };
    }

    public abstract class Light2D : Component
    {
        protected LightType lightType;
        protected Color color;
        protected float intensity;
        protected float range;
        protected bool isEnabled;
        protected bool affectWorld;
        protected bool shadowCasting;
        protected ShadowType shadowType;
        protected LightLayerMask cullingMask;
        protected LightLayerMask shadowCastingLayers;
        protected LightBlendMode blendMode;
        protected float priority;
        protected bool drawGizmo;
        protected ImageSource lightCookie;
        protected Bounds lightBounds;

        public event EventHandler<LightEventArgs>? LightChanged;
        public event EventHandler<LightEventArgs>? IntensityChanged;
        public event EventHandler<LightEventArgs>? ColorChanged;
        public event EventHandler<LightEventArgs>? RangeChanged;

        public LightType LightType => lightType;

        public Color Color
        {
            get => color;
            set
            {
                if (color != value)
                {
                    color = value;
                    ColorChanged?.Invoke(this, new LightEventArgs(this));
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public float Intensity
        {
            get => intensity;
            set
            {
                var clampedIntensity = Math.Max(0f, value);
                if (Math.Abs(intensity - clampedIntensity) > 0.0001f)
                {
                    intensity = clampedIntensity;
                    IntensityChanged?.Invoke(this, new LightEventArgs(this));
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public float Range
        {
            get => range;
            set
            {
                var clampedRange = Math.Max(0.1f, value);
                if (Math.Abs(range - clampedRange) > 0.0001f)
                {
                    range = clampedRange;
                    RangeChanged?.Invoke(this, new LightEventArgs(this));
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public bool AffectWorld
        {
            get => affectWorld;
            set
            {
                if (affectWorld != value)
                {
                    affectWorld = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public bool ShadowCasting
        {
            get => shadowCasting;
            set
            {
                if (shadowCasting != value)
                {
                    shadowCasting = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public ShadowType ShadowType
        {
            get => shadowType;
            set
            {
                if (shadowType != value)
                {
                    shadowType = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public LightLayerMask CullingMask
        {
            get => cullingMask;
            set
            {
                if (cullingMask != value)
                {
                    cullingMask = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public LightLayerMask ShadowCastingLayers
        {
            get => shadowCastingLayers;
            set
            {
                if (shadowCastingLayers != value)
                {
                    shadowCastingLayers = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public LightBlendMode BlendMode
        {
            get => blendMode;
            set
            {
                if (blendMode != value)
                {
                    blendMode = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public float Priority
        {
            get => priority;
            set
            {
                priority = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public bool DrawGizmo
        {
            get => drawGizmo;
            set => drawGizmo = value;
        }

        public ImageSource LightCookie
        {
            get => lightCookie;
            set
            {
                if (lightCookie != value)
                {
                    lightCookie = value;
                    LightChanged?.Invoke(this, new LightEventArgs(this));
                }
            }
        }

        public Bounds LightBounds => lightBounds;

        public Vector2 Position => Transform?.WorldPosition ?? Vector2.Zero;

        public Light2D()
        {
            lightType = LightType.Point;
            color = Colors.White;
            intensity = 1f;
            range = 10f;
            isEnabled = true;
            affectWorld = true;
            shadowCasting = true;
            shadowType = ShadowType.Soft;
            cullingMask = LightLayerMask.Everything;
            shadowCastingLayers = LightLayerMask.Everything;
            blendMode = LightBlendMode.Add;
            priority = 0f;
            drawGizmo = true;
            lightBounds = new Bounds(Vector2.Zero, new Vector2(range * 2, range * 2));
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            if (Transform != null)
            {
                Transform.PositionChanged += OnTransformChanged;
                UpdateBounds();
            }
        }

        private void OnTransformChanged(object sender, TransformEventArgs e)
        {
            UpdateBounds();
            LightChanged?.Invoke(this, new LightEventArgs(this));
        }

        protected virtual void UpdateBounds()
        {
            if (Transform != null)
            {
                lightBounds = new Bounds(Position, new Vector2(range * 2, range * 2));
            }
        }

        public virtual float GetIntensityAtPoint(Vector2 point)
        {
            if (!isEnabled || intensity <= 0f)
                return 0f;

            var distance = Position.Distance(point);
            if (distance > range)
                return 0f;

            var falloff = 1f - (distance / range);
            return falloff * falloff * intensity;
        }

        public virtual Color GetColorAtPoint(Vector2 point)
        {
            var localIntensity = GetIntensityAtPoint(point);
            return new Color(
                color.R * localIntensity,
                color.G * localIntensity,
                color.B * localIntensity,
                color.A * localIntensity
            );
        }

        public virtual bool IsInRange(Vector2 point)
        {
            return Position.Distance(point) <= range;
        }

        public virtual bool CanAffectLayer(LightLayer layer)
        {
            return cullingMask.Contains(layer);
        }

        public virtual bool CanCastShadowOnLayer(LightLayer layer)
        {
            return shadowCasting && shadowCastingLayers.Contains(layer);
        }

        public abstract void RenderLightGizmo(DrawingContext context);

        public override string ToString()
        {
            return $"Light2D({lightType}, Color={color}, Intensity={intensity:F2}, Range={range:F2})";
        }
    }

    public class PointLight : Light2D
    {
        private float innerRadius;
        private float outerRadius;
        private float softness;
        private bool renderAsSprite;
        private ImageSource sprite;
        private float spriteScale;
        private Vector2 falloffOffset;
        private bool useVolume;

        public float InnerRadius
        {
            get => innerRadius;
            set
            {
                innerRadius = Math.Max(0f, Math.Min(value, outerRadius));
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float OuterRadius
        {
            get => outerRadius;
            set
            {
                outerRadius = Math.Max(innerRadius, value);
                Range = outerRadius;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float Softness
        {
            get => softness;
            set
            {
                softness = Math.Clamp(value, 0f, 1f);
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public bool RenderAsSprite
        {
            get => renderAsSprite;
            set
            {
                renderAsSprite = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public ImageSource Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float SpriteScale
        {
            get => spriteScale;
            set
            {
                spriteScale = Math.Max(0.1f, value);
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public Vector2 FalloffOffset
        {
            get => falloffOffset;
            set
            {
                falloffOffset = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public bool UseVolume
        {
            get => useVolume;
            set
            {
                useVolume = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public PointLight()
        {
            lightType = LightType.Point;
            innerRadius = 0f;
            outerRadius = 5f;
            softness = 0.5f;
            renderAsSprite = false;
            spriteScale = 1f;
            falloffOffset = Vector2.Zero;
            useVolume = false;
            range = outerRadius;
        }

        public override float GetIntensityAtPoint(Vector2 point)
        {
            if (!isEnabled || intensity <= 0f)
                return 0f;

            var adjustedPosition = Position + falloffOffset;
            var distance = adjustedPosition.Distance(point);
            
            if (distance > outerRadius)
                return 0f;

            float falloff;
            if (distance <= innerRadius)
            {
                falloff = 1f;
            }
            else
            {
                var t = (distance - innerRadius) / (outerRadius - innerRadius);
                falloff = 1f - t;
                
                if (softness < 1f)
                {
                    falloff = (float)Math.Pow(falloff, 1f / (1f - softness + 0.001f));
                }
            }

            return falloff * intensity;
        }

        public override Color GetColorAtPoint(Vector2 point)
        {
            var localIntensity = GetIntensityAtPoint(point);
            return new Color(
                color.R * localIntensity,
                color.G * localIntensity,
                color.B * localIntensity,
                color.A * localIntensity
            );
        }

        public override void RenderLightGizmo(DrawingContext context)
        {
            if (!drawGizmo)
                return;

            var pos = Position;
            var radius = outerRadius;

            var outerPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)(128 * intensity),
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255))), 1);

            context.DrawEllipse(null, outerPen, new Point(pos.X, pos.Y), radius, radius);

            if (innerRadius > 0)
            {
                var innerPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    (byte)(180 * intensity),
                    (byte)(color.R * 255),
                    (byte)(color.G * 255),
                    (byte)(color.B * 255))), 1);
                context.DrawEllipse(null, innerPen, new Point(pos.X, pos.Y), innerRadius, innerRadius);
            }

            if (renderAsSprite && sprite != null)
            {
                var size = range * 2 * spriteScale;
                var rect = new Rect(pos.X - size / 2, pos.Y - size / 2, size, size);
                context.DrawImage(sprite, rect);
            }
        }

        public void SetRadius(float inner, float outer)
        {
            innerRadius = Math.Max(0f, inner);
            outerRadius = Math.Max(innerRadius, outer);
            range = outerRadius;
            UpdateBounds();
        }

        public float GetRadiusIntensity(float normalizedRadius)
        {
            normalizedRadius = Math.Clamp(normalizedRadius, 0f, 1f);
            
            if (innerRadius >= outerRadius)
                return normalizedRadius >= 1f ? 0f : 1f;

            var actualRadius = normalizedRadius * outerRadius;
            if (actualRadius <= innerRadius)
                return 1f;

            var t = (actualRadius - innerRadius) / (outerRadius - innerRadius);
            return 1f - t;
        }
    }

    public class SpotLight : Light2D
    {
        private Vector2 direction;
        private float angle;
        private float penumbraAngle;
        private float innerAngle;
        private float outerAngle;
        private float rangeAtMinAngle;
        private float rangeAtMaxAngle;
        private bool renderCone;

        public Vector2 Direction
        {
            get => direction;
            set
            {
                direction = value.Normalized();
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float Angle
        {
            get => angle;
            set
            {
                angle = Math.Clamp(value, 0f, 180f);
                outerAngle = angle / 2f;
                if (innerAngle > outerAngle)
                    innerAngle = outerAngle;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float PenumbraAngle
        {
            get => penumbraAngle;
            set
            {
                penumbraAngle = Math.Clamp(value, 0f, angle);
                innerAngle = (angle - penumbraAngle) / 2f;
                outerAngle = (angle + penumbraAngle) / 2f;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float InnerAngle
        {
            get => innerAngle;
            set
            {
                innerAngle = Math.Clamp(value, 0f, outerAngle);
                penumbraAngle = angle - innerAngle;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float OuterAngle
        {
            get => outerAngle;
            set
            {
                outerAngle = Math.Clamp(value, innerAngle, 180f);
                angle = innerAngle + penumbraAngle;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float RangeAtMinAngle
        {
            get => rangeAtMinAngle;
            set
            {
                rangeAtMinAngle = Math.Max(0f, value);
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float RangeAtMaxAngle
        {
            get => rangeAtMaxAngle;
            set
            {
                rangeAtMaxAngle = Math.Max(0f, value);
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public bool RenderCone
        {
            get => renderCone;
            set
            {
                renderCone = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public SpotLight()
        {
            lightType = LightType.Spot;
            direction = Vector2.Up;
            angle = 45f;
            penumbraAngle = 5f;
            innerAngle = (45f - 5f) / 2f;
            outerAngle = (45f + 5f) / 2f;
            rangeAtMinAngle = 10f;
            rangeAtMaxAngle = 10f;
            renderCone = true;
            range = rangeAtMaxAngle;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            UpdateDirection();
        }

        private void UpdateDirection()
        {
            if (Transform != null)
            {
                direction = Transform.GetForwardVector();
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateDirection();
        }

        public override float GetIntensityAtPoint(Vector2 point)
        {
            if (!isEnabled || intensity <= 0f)
                return 0f;

            var pos = Position;
            var toPoint = point - pos;
            var distance = toPoint.Length();
            
            if (distance > range)
                return 0f;

            var angleToPoint = (float)Math.Atan2(toPoint.Y, toPoint.X) * 180f / (float)Math.PI;
            var lightAngle = (float)Math.Atan2(direction.Y, direction.X) * 180f / (float)Math.PI;
            var angleDiff = Math.Abs(NormalizeAngle(angleToPoint - lightAngle));

            if (angleDiff > outerAngle)
                return 0f;

            float angleFalloff;
            if (angleDiff <= innerAngle)
            {
                angleFalloff = 1f;
            }
            else
            {
                var t = (angleDiff - innerAngle) / (outerAngle - innerAngle);
                angleFalloff = 1f - t;
            }

            var distanceRatio = distance / range;
            var distanceFalloff = 1f - distanceRatio;
            distanceFalloff *= distanceFalloff;

            return angleFalloff * distanceFalloff * intensity;
        }

        public override Color GetColorAtPoint(Vector2 point)
        {
            var localIntensity = GetIntensityAtPoint(point);
            return new Color(
                color.R * localIntensity,
                color.G * localIntensity,
                color.B * localIntensity,
                color.A * localIntensity
            );
        }

        public override bool IsInRange(Vector2 point)
        {
            var toPoint = point - Position;
            if (toPoint.Length() > range)
                return false;

            var angleToPoint = (float)Math.Atan2(toPoint.Y, toPoint.X) * 180f / (float)Math.PI;
            var lightAngle = (float)Math.Atan2(direction.Y, direction.X) * 180f / (float)Math.PI;
            var angleDiff = Math.Abs(NormalizeAngle(angleToPoint - lightAngle));
            return angleDiff <= outerAngle;
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        public override void RenderLightGizmo(DrawingContext context)
        {
            if (!drawGizmo)
                return;

            var pos = Position;
            var angleRad = (float)Math.Atan2(direction.Y, direction.X);
            var halfAngleRad = outerAngle * (float)Math.PI / 180f;
            var innerHalfAngleRad = innerAngle * (float)Math.PI / 180f;

            var outerLeft = new Vector2(
                pos.X + (float)Math.Cos(angleRad - halfAngleRad) * range,
                pos.Y + (float)Math.Sin(angleRad - halfAngleRad) * range
            );
            var outerRight = new Vector2(
                pos.X + (float)Math.Cos(angleRad + halfAngleRad) * range,
                pos.Y + (float)Math.Sin(angleRad + halfAngleRad) * range
            );

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)(100 * intensity),
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255)));

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(pos.X, pos.Y), true, true);
                ctx.LineTo(new Point(outerLeft.X, outerLeft.Y), true, false);
                ctx.LineTo(new Point(outerRight.X, outerRight.Y), true, false);
            }
            geometry.Freeze();
            context.DrawGeometry(brush, null, geometry);

            var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255))), 1);
            context.DrawLine(pen, new Point(pos.X, pos.Y), new Point(outerLeft.X, outerLeft.Y));
            context.DrawLine(pen, new Point(pos.X, pos.Y), new Point(outerRight.X, outerRight.Y));

            if (innerAngle > 0)
            {
                var innerLeft = new Vector2(
                    pos.X + (float)Math.Cos(angleRad - innerHalfAngleRad) * range,
                    pos.Y + (float)Math.Sin(angleRad - innerHalfAngleRad) * range
                );
                var innerRight = new Vector2(
                    pos.X + (float)Math.Cos(angleRad + innerHalfAngleRad) * range,
                    pos.Y + (float)Math.Sin(angleRad + innerHalfAngleRad) * range
                );
                var innerPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(180,
                    (byte)(color.R * 255),
                    (byte)(color.G * 255),
                    (byte)(color.B * 255))), 1);
                context.DrawLine(innerPen, new Point(pos.X, pos.Y), new Point(innerLeft.X, innerLeft.Y));
                context.DrawLine(innerPen, new Point(pos.X, pos.Y), new Point(innerRight.X, innerRight.Y));
            }
        }

        public void SetDirectionAndAngle(Vector2 dir, float spotAngle)
        {
            direction = dir.Normalized();
            angle = Math.Clamp(spotAngle, 0f, 180f);
            outerAngle = angle / 2f;
            innerAngle = (angle - penumbraAngle) / 2f;
            LightChanged?.Invoke(this, new LightEventArgs(this));
        }
    }

    public class DirectionalLight : Light2D
    {
        private Vector2 lightDirection;
        private bool fullScreen;
        private float coverage;
        private Rect screenCoverage;

        public Vector2 LightDirection
        {
            get => lightDirection;
            set
            {
                lightDirection = value.Normalized();
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public bool FullScreen
        {
            get => fullScreen;
            set
            {
                fullScreen = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float Coverage
        {
            get => coverage;
            set
            {
                coverage = Math.Clamp(value, 0f, 1f);
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public Rect ScreenCoverage
        {
            get => screenCoverage;
            set
            {
                screenCoverage = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public DirectionalLight()
        {
            lightType = LightType.Directional;
            lightDirection = new Vector2(0f, -1f).Normalized();
            fullScreen = true;
            coverage = 1f;
            screenCoverage = new Rect(0, 0, 1, 1);
            range = float.MaxValue;
        }

        public override float GetIntensityAtPoint(Vector2 point)
        {
            if (!isEnabled || intensity <= 0f)
                return 0f;

            if (!fullScreen && !screenCoverage.Contains(new Point(point.X, point.Y)))
                return 0f;

            return intensity;
        }

        public override Color GetColorAtPoint(Vector2 point)
        {
            var localIntensity = GetIntensityAtPoint(point);
            return new Color(
                color.R * localIntensity,
                color.G * localIntensity,
                color.B * localIntensity,
                color.A * localIntensity
            );
        }

        public override bool IsInRange(Vector2 point)
        {
            if (fullScreen)
                return true;
            return screenCoverage.Contains(new Point(point.X, point.Y));
        }

        public override void RenderLightGizmo(DrawingContext context)
        {
            if (!drawGizmo)
                return;

            var pos = Position;
            var dir = lightDirection.Normalized() * 50f;

            var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)(200 * intensity),
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255))), 2);

            context.DrawLine(pen, new Point(pos.X, pos.Y), 
                new Point(pos.X + dir.X, pos.Y + dir.Y));

            var arrowSize = 10f;
            var angle = (float)Math.Atan2(dir.Y, dir.X);
            var arrow1 = new Point(
                pos.X + dir.X + (float)Math.Cos(angle + 2.5f) * arrowSize,
                pos.Y + dir.Y + (float)Math.Sin(angle + 2.5f) * arrowSize
            );
            var arrow2 = new Point(
                pos.X + dir.X + (float)Math.Cos(angle - 2.5f) * arrowSize,
                pos.Y + dir.Y + (float)Math.Sin(angle - 2.5f) * arrowSize
            );

            context.DrawLine(pen, new Point(pos.X + dir.X, pos.Y + dir.Y), arrow1);
            context.DrawLine(pen, new Point(pos.X + dir.X, pos.Y + dir.Y), arrow2);
        }

        public float GetShadowAtPoint(Vector2 point, List<ShadowCaster2D> casters)
        {
            if (!shadowCasting || casters == null || casters.Count == 0)
                return 0f;

            var totalShadow = 0f;
            foreach (var caster in casters)
            {
                if (!caster.IsEnabled || !caster.CanBeCastedBy(this))
                    continue;

                totalShadow += caster.GetShadowAtPoint(point, lightDirection);
            }

            return Math.Clamp(totalShadow, 0f, 1f);
        }
    }

    public class GlobalLight : Light2D
    {
        private bool useCustomAmbient;
        private Color ambientColor;
        private float ambientIntensity;
        private bool affectAllLayers;

        public bool UseCustomAmbient
        {
            get => useCustomAmbient;
            set
            {
                useCustomAmbient = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public Color AmbientColor
        {
            get => ambientColor;
            set
            {
                ambientColor = value;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public float AmbientIntensity
        {
            get => ambientIntensity;
            set
            {
                ambientIntensity = Math.Clamp(value, 0f, 1f);
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public bool AffectAllLayers
        {
            get => affectAllLayers;
            set
            {
                affectAllLayers = value;
                if (value)
                    cullingMask = LightLayerMask.Everything;
                LightChanged?.Invoke(this, new LightEventArgs(this));
            }
        }

        public GlobalLight()
        {
            lightType = LightType.Global;
            useCustomAmbient = false;
            ambientColor = Colors.White;
            ambientIntensity = 0.3f;
            affectAllLayers = true;
            cullingMask = LightLayerMask.Everything;
            range = float.MaxValue;
            isEnabled = true;
        }

        public override float GetIntensityAtPoint(Vector2 point)
        {
            if (!isEnabled || intensity <= 0f)
                return 0f;

            return intensity * ambientIntensity;
        }

        public override Color GetColorAtPoint(Vector2 point)
        {
            var localIntensity = GetIntensityAtPoint(point);
            var effectiveColor = useCustomAmbient ? ambientColor : color;
            return new Color(
                effectiveColor.R * localIntensity,
                effectiveColor.G * localIntensity,
                effectiveColor.B * localIntensity,
                effectiveColor.A * localIntensity
            );
        }

        public override bool IsInRange(Vector2 point)
        {
            return isEnabled;
        }

        public override void RenderLightGizmo(DrawingContext context)
        {
            if (!drawGizmo)
                return;

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)(50 * intensity * ambientIntensity),
                (byte)(color.R * 255),
                (byte)(color.G * 255),
                (byte)(color.B * 255)));

            context.DrawRectangle(brush, null, new Rect(-1000, -1000, 2000, 2000));
        }

        public Color GetAmbientColor()
        {
            if (useCustomAmbient)
            {
                return new Color(
                    ambientColor.R * ambientIntensity,
                    ambientColor.G * ambientIntensity,
                    ambientColor.B * ambientIntensity,
                    ambientColor.A * ambientIntensity
                );
            }

            return new Color(
                color.R * ambientIntensity * intensity,
                color.G * ambientIntensity * intensity,
                color.B * ambientIntensity * intensity,
                color.A * ambientIntensity * intensity
            );
        }
    }

    public class ShadowCaster2D : Component
    {
        private List<Vector2> edgeVertices;
        private List<ShadowHull> shadowHulls;
        private bool isEnabled;
        private LightLayer layer;
        private float shadowIntensity;
        private bool selfShadows;
        private bool castShadows;
        private bool receiveShadows;

        public event EventHandler? ShadowGeometryChanged;

        public List<Vector2> EdgeVertices
        {
            get => edgeVertices;
            set
            {
                edgeVertices = value ?? new List<Vector2>();
                BuildShadowHulls();
                ShadowGeometryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<ShadowHull> ShadowHulls => shadowHulls;

        public new bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    ShadowGeometryChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public LightLayer Layer
        {
            get => layer;
            set
            {
                if (layer != value)
                {
                    layer = value;
                    ShadowGeometryChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public float ShadowIntensity
        {
            get => shadowIntensity;
            set
            {
                shadowIntensity = Math.Clamp(value, 0f, 1f);
            }
        }

        public bool SelfShadows
        {
            get => selfShadows;
            set => selfShadows = value;
        }

        public bool CastShadows
        {
            get => castShadows;
            set
            {
                if (castShadows != value)
                {
                    castShadows = value;
                    ShadowGeometryChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool ReceiveShadows
        {
            get => receiveShadows;
            set => receiveShadows = value;
        }

        public ShadowCaster2D()
        {
            edgeVertices = new List<Vector2>();
            shadowHulls = new List<ShadowHull>();
            isEnabled = true;
            layer = LightLayer.Layer0;
            shadowIntensity = 1f;
            selfShadows = true;
            castShadows = true;
            receiveShadows = true;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            BuildShadowHullsFromCollider();
        }

        private void BuildShadowHullsFromCollider()
        {
            var occluder = GetComponent<LightOccluder2D>();
            if (occluder != null)
            {
                edgeVertices = occluder.GetEdgeVertices();
                BuildShadowHulls();
            }
        }

        public void BuildShadowHulls()
        {
            shadowHulls.Clear();
            if (edgeVertices.Count < 3)
                return;

            for (int i = 0; i < edgeVertices.Count; i++)
            {
                var v1 = edgeVertices[i];
                var v2 = edgeVertices[(i + 1) % edgeVertices.Count];
                shadowHulls.Add(new ShadowHull(v1, v2));
            }
        }

        public float GetShadowAtPoint(Vector2 point, Vector2 lightDirection)
        {
            if (!isEnabled || !castShadows || shadowHulls.Count == 0)
                return 0f;

            var worldPosition = Transform?.WorldPosition ?? Vector2.Zero;
            var worldScale = Transform?.WorldScale ?? Vector2.One;
            var worldRotation = Transform?.WorldRotation ?? 0f;

            var transformedHulls = new List<ShadowHull>();
            foreach (var hull in shadowHulls)
            {
                var v1 = TransformPoint(hull.Vertex1, worldPosition, worldScale, worldRotation);
                var v2 = TransformPoint(hull.Vertex2, worldPosition, worldScale, worldRotation);
                transformedHulls.Add(new ShadowHull(v1, v2));
            }

            foreach (var hull in transformedHulls)
            {
                var toPoint = point - hull.Vertex1;
                var edgeDir = (hull.Vertex2 - hull.Vertex1).Normalized();
                var perpendicular = new Vector2(-edgeDir.Y, edgeDir.X);
                var lightPerpDot = lightDirection.Dot(perpendicular);

                if (Math.Abs(lightPerpDot) < 0.0001f)
                    continue;

                var toPointPerpDot = toPoint.Dot(perpendicular);
                if (toPointPerpDot * lightPerpDot < 0)
                {
                    return shadowIntensity;
                }
            }

            return 0f;
        }

        private Vector2 TransformPoint(Vector2 point, Vector2 position, Vector2 scale, float rotation)
        {
            var rad = rotation * (float)Math.PI / 180f;
            var cos = (float)Math.Cos(rad);
            var sin = (float)Math.Sin(rad);

            var scaled = new Vector2(point.X * scale.X, point.Y * scale.Y);
            var rotated = new Vector2(
                scaled.X * cos - scaled.Y * sin,
                scaled.X * sin + scaled.Y * cos
            );

            return rotated + position;
        }

        public bool CanBeCastedBy(Light2D light)
        {
            return isEnabled && castShadows && light.CanCastShadowOnLayer(layer);
        }

        public void SetEdgeVertices(params Vector2[] vertices)
        {
            edgeVertices = new List<Vector2>(vertices);
            BuildShadowHulls();
            ShadowGeometryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddEdgeVertex(Vector2 vertex)
        {
            edgeVertices.Add(vertex);
            BuildShadowHulls();
            ShadowGeometryChanged?.Invoke(this, EventArgs.Empty);
        }

        public Bounds GetWorldBounds()
        {
            if (edgeVertices.Count == 0)
                return new Bounds(Vector2.Zero, Vector2.Zero);

            var worldPosition = Transform?.WorldPosition ?? Vector2.Zero;
            var worldScale = Transform?.WorldScale ?? Vector2.One;
            var worldRotation = Transform?.WorldRotation ?? 0f;

            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            foreach (var vertex in edgeVertices)
            {
                var worldVertex = TransformPoint(vertex, worldPosition, worldScale, worldRotation);
                minX = Math.Min(minX, worldVertex.X);
                minY = Math.Min(minY, worldVertex.Y);
                maxX = Math.Max(maxX, worldVertex.X);
                maxY = Math.Max(maxY, worldVertex.Y);
            }

            var center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
            var size = new Vector2(maxX - minX, maxY - minY);
            return new Bounds(center, size);
        }

        public List<Vector2> GetTransformedVertices()
        {
            var result = new List<Vector2>();
            var worldPosition = Transform?.WorldPosition ?? Vector2.Zero;
            var worldScale = Transform?.WorldScale ?? Vector2.One;
            var worldRotation = Transform?.WorldRotation ?? 0f;

            foreach (var vertex in edgeVertices)
            {
                result.Add(TransformPoint(vertex, worldPosition, worldScale, worldRotation));
            }

            return result;
        }
    }

    public struct ShadowHull
    {
        public Vector2 Vertex1 { get; }
        public Vector2 Vertex2 { get; }
        public Vector2 EdgeDirection { get; }
        public Vector2 Normal { get; }

        public ShadowHull(Vector2 v1, Vector2 v2)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            EdgeDirection = (v2 - v1).Normalized();
            Normal = new Vector2(-EdgeDirection.Y, EdgeDirection.X);
        }

        public Vector2 GetPointOnEdge(float t)
        {
            return Vector2.Lerp(Vertex1, Vertex2, t);
        }

        public float GetDistanceToPoint(Vector2 point)
        {
            var toPoint = point - Vertex1;
            var projection = toPoint.Dot(EdgeDirection);
            projection = Math.Clamp(projection, 0f, (Vertex2 - Vertex1).Length());
            var closest = Vertex1 + EdgeDirection * projection;
            return (point - closest).Length();
        }
    }

    public class LightOccluder2D : Component
    {
        private OccluderShape shape;
        private List<Vector2> customVertices;
        private Vector2 boxSize;
        private float circleRadius;
        private bool isEnabled;
        private LightLayer layer;
        private float opacity;
        private bool shadowsOnly;

        public event EventHandler? GeometryChanged;

        public OccluderShape Shape
        {
            get => shape;
            set
            {
                shape = value;
                GeometryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<Vector2> CustomVertices
        {
            get => customVertices;
            set
            {
                customVertices = value ?? new List<Vector2>();
                GeometryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Vector2 BoxSize
        {
            get => boxSize;
            set
            {
                boxSize = value;
                GeometryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public float CircleRadius
        {
            get => circleRadius;
            set
            {
                circleRadius = Math.Max(0f, value);
                GeometryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public new bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    GeometryChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public LightLayer Layer
        {
            get => layer;
            set
            {
                if (layer != value)
                {
                    layer = value;
                    GeometryChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public float Opacity
        {
            get => opacity;
            set
            {
                opacity = Math.Clamp(value, 0f, 1f);
            }
        }

        public bool ShadowsOnly
        {
            get => shadowsOnly;
            set
            {
                shadowsOnly = value;
                GeometryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public LightOccluder2D()
        {
            shape = OccluderShape.Box;
            customVertices = new List<Vector2>();
            boxSize = new Vector2(1f, 1f);
            circleRadius = 0.5f;
            isEnabled = true;
            layer = LightLayer.Layer0;
            opacity = 1f;
            shadowsOnly = false;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            UpdateShadowCaster();
        }

        private void UpdateShadowCaster()
        {
            var shadowCaster = GetComponent<ShadowCaster2D>();
            if (shadowCaster == null)
            {
                shadowCaster = GameObject?.AddComponent<ShadowCaster2D>();
            }

            if (shadowCaster != null)
            {
                shadowCaster.EdgeVertices = GetEdgeVertices();
                shadowCaster.Layer = layer;
            }
        }

        public List<Vector2> GetEdgeVertices()
        {
            var worldPosition = Transform?.WorldPosition ?? Vector2.Zero;
            var worldScale = Transform?.WorldScale ?? Vector2.One;
            var worldRotation = Transform?.WorldRotation ?? 0f;

            List<Vector2> localVertices;
            switch (shape)
            {
                case OccluderShape.Box:
                    localVertices = GetBoxVertices();
                    break;
                case OccluderShape.Circle:
                    localVertices = GetCircleVertices(16);
                    break;
                case OccluderShape.Custom:
                    localVertices = customVertices;
                    break;
                default:
                    localVertices = new List<Vector2>();
                    break;
            }

            var worldVertices = new List<Vector2>();
            foreach (var vertex in localVertices)
            {
                var scaled = new Vector2(vertex.X * worldScale.X, vertex.Y * worldScale.Y);
                var rad = worldRotation * (float)Math.PI / 180f;
                var cos = (float)Math.Cos(rad);
                var sin = (float)Math.Sin(rad);
                var rotated = new Vector2(
                    scaled.X * cos - scaled.Y * sin,
                    scaled.X * sin + scaled.Y * cos
                );
                worldVertices.Add(rotated + worldPosition);
            }

            return worldVertices;
        }

        private List<Vector2> GetBoxVertices()
        {
            var halfWidth = boxSize.X / 2f;
            var halfHeight = boxSize.Y / 2f;
            return new List<Vector2>
            {
                new Vector2(-halfWidth, -halfHeight),
                new Vector2(halfWidth, -halfHeight),
                new Vector2(halfWidth, halfHeight),
                new Vector2(-halfWidth, halfHeight)
            };
        }

        private List<Vector2> GetCircleVertices(int segments)
        {
            var vertices = new List<Vector2>();
            var angleStep = (float)(2 * Math.PI / segments);
            for (int i = 0; i < segments; i++)
            {
                var angle = i * angleStep;
                vertices.Add(new Vector2(
                    (float)Math.Cos(angle) * circleRadius,
                    (float)Math.Sin(angle) * circleRadius
                ));
            }
            return vertices;
        }

        public bool ContainsPoint(Vector2 point)
        {
            var worldPosition = Transform?.WorldPosition ?? Vector2.Zero;
            var localPoint = point - worldPosition;

            switch (shape)
            {
                case OccluderShape.Box:
                    var halfWidth = boxSize.X / 2f * (Transform?.WorldScale.X ?? 1f);
                    var halfHeight = boxSize.Y / 2f * (Transform?.WorldScale.Y ?? 1f);
                    return Math.Abs(localPoint.X) <= halfWidth && Math.Abs(localPoint.Y) <= halfHeight;

                case OccluderShape.Circle:
                    var radius = circleRadius * (Transform?.WorldScale.X ?? 1f);
                    return localPoint.LengthSquared() <= radius * radius;

                case OccluderShape.Custom:
                    if (customVertices.Count < 3)
                        return false;
                    return IsPointInPolygon(localPoint, customVertices);

                default:
                    return false;
            }
        }

        private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            bool inside = false;
            int j = polygon.Count - 1;

            for (int i = 0; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public Bounds GetBounds()
        {
            var vertices = GetEdgeVertices();
            if (vertices.Count == 0)
                return new Bounds(Vector2.Zero, Vector2.Zero);

            var minX = vertices.Min(v => v.X);
            var minY = vertices.Min(v => v.Y);
            var maxX = vertices.Max(v => v.X);
            var maxY = vertices.Max(v => v.Y);

            return new Bounds(
                new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f),
                new Vector2(maxX - minX, maxY - minY)
            );
        }
    }

    public enum OccluderShape
    {
        Box,
        Circle,
        Custom
    }

    public class LightSprite : Component
    {
        private ImageSource sprite;
        private Color color;
        private float alpha;
        private bool renderAboveLights;
        private int sortingLayer;
        private int orderInLayer;
        private Vector2 size;
        private Vector2 offset;

        public ImageSource Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                OnPropertyChanged();
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                OnPropertyChanged();
            }
        }

        public float Alpha
        {
            get => alpha;
            set
            {
                alpha = Math.Clamp(value, 0f, 1f);
                OnPropertyChanged();
            }
        }

        public bool RenderAboveLights
        {
            get => renderAboveLights;
            set
            {
                renderAboveLights = value;
                OnPropertyChanged();
            }
        }

        public int SortingLayer
        {
            get => sortingLayer;
            set
            {
                sortingLayer = value;
                OnPropertyChanged();
            }
        }

        public int OrderInLayer
        {
            get => orderInLayer;
            set
            {
                orderInLayer = value;
                OnPropertyChanged();
            }
        }

        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                OnPropertyChanged();
            }
        }

        public Vector2 Offset
        {
            get => offset;
            set
            {
                offset = value;
                OnPropertyChanged();
            }
        }

        public LightSprite()
        {
            color = Colors.White;
            alpha = 1f;
            renderAboveLights = false;
            sortingLayer = 0;
            orderInLayer = 0;
            size = new Vector2(1f, 1f);
            offset = Vector2.Zero;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public Color GetFinalColor()
        {
            return new Color(
                color.R,
                color.G,
                color.B,
                color.A * alpha
            );
        }

        public Vector2 GetWorldPosition()
        {
            var position = Transform?.WorldPosition ?? Vector2.Zero;
            return position + offset;
        }

        public Rect GetWorldBounds()
        {
            var pos = GetWorldPosition();
            var scale = Transform?.WorldScale ?? Vector2.One;
            return new Rect(
                pos.X - size.X * scale.X / 2f,
                pos.Y - size.Y * scale.Y / 2f,
                size.X * scale.X,
                size.Y * scale.Y
            );
        }
    }

    public class NormalMap2D : Component
    {
        private ImageSource normalMapTexture;
        private float normalScale;
        private bool isEnabled;

        public ImageSource NormalMapTexture
        {
            get => normalMapTexture;
            set
            {
                normalMapTexture = value;
                OnPropertyChanged();
            }
        }

        public float NormalScale
        {
            get => normalScale;
            set
            {
                normalScale = Math.Max(0f, value);
                OnPropertyChanged();
            }
        }

        public new bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public NormalMap2D()
        {
            normalScale = 1f;
            isEnabled = true;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public Vector3 GetNormalAtPoint(Vector2 localPoint, Vector2 size)
        {
            return Vector3.Up;
        }

        public Color SampleNormalMap(Vector2 uv)
        {
            return Color.White;
        }
    }

    public class LightCookieTexture : Component
    {
        private ImageSource cookieTexture;
        private Vector2 cookieSize;
        private bool tiling;
        private Vector2 tileScale;
        private Vector2 tileOffset;
        private float rotation;

        public ImageSource CookieTexture
        {
            get => cookieTexture;
            set
            {
                cookieTexture = value;
                OnPropertyChanged();
            }
        }

        public Vector2 CookieSize
        {
            get => cookieSize;
            set
            {
                cookieSize = value;
                OnPropertyChanged();
            }
        }

        public bool Tiling
        {
            get => tiling;
            set
            {
                tiling = value;
                OnPropertyChanged();
            }
        }

        public Vector2 TileScale
        {
            get => tileScale;
            set
            {
                tileScale = value;
                OnPropertyChanged();
            }
        }

        public Vector2 TileOffset
        {
            get => tileOffset;
            set
            {
                tileOffset = value;
                OnPropertyChanged();
            }
        }

        public float Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                OnPropertyChanged();
            }
        }

        public LightCookieTexture()
        {
            cookieSize = new Vector2(1f, 1f);
            tiling = false;
            tileScale = Vector2.One;
            tileOffset = Vector2.Zero;
            rotation = 0f;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public float GetIntensityAtUV(Vector2 uv)
        {
            if (cookieTexture == null)
                return 1f;

            var adjustedUV = uv;
            if (tiling)
            {
                adjustedUV = uv * tileScale + tileOffset;
            }

            adjustedUV.X = adjustedUV.X % 1f;
            adjustedUV.Y = adjustedUV.Y % 1f;
            if (adjustedUV.X < 0) adjustedUV.X += 1f;
            if (adjustedUV.Y < 0) adjustedUV.Y += 1f;

            return 1f;
        }
    }

    public class VolumetricLight : Component
    {
        private Light2D sourceLight;
        private float raySteps;
        private float decay;
        private float Density;
        private float Weight;
        private float Exposure;
        private bool isEnabled;
        private Color volumetricColor;
        private float maxRayLength;

        public Light2D SourceLight
        {
            get => sourceLight;
            set
            {
                sourceLight = value;
                OnPropertyChanged();
            }
        }

        public float RaySteps
        {
            get => raySteps;
            set
            {
                raySteps = Math.Max(1f, value);
                OnPropertyChanged();
            }
        }

        public float Decay
        {
            get => decay;
            set
            {
                decay = Math.Clamp(value, 0f, 1f);
                OnPropertyChanged();
            }
        }

        public float VolumetricDensity
        {
            get => Density;
            set
            {
                Density = Math.Max(0f, value);
                OnPropertyChanged();
            }
        }

        public float VolumetricWeight
        {
            get => Weight;
            set
            {
                Weight = Math.Clamp(value, 0f, 1f);
                OnPropertyChanged();
            }
        }

        public float VolumetricExposure
        {
            get => Exposure;
            set
            {
                Exposure = Math.Clamp(value, 0f, 1f);
                OnPropertyChanged();
            }
        }

        public new bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color VolumetricColor
        {
            get => volumetricColor;
            set
            {
                volumetricColor = value;
                OnPropertyChanged();
            }
        }

        public float MaxRayLength
        {
            get => maxRayLength;
            set
            {
                maxRayLength = Math.Max(0f, value);
                OnPropertyChanged();
            }
        }

        public VolumetricLight()
        {
            raySteps = 64f;
            decay = 0.95f;
            Density = 1f;
            Weight = 0.5f;
            Exposure = 0.3f;
            isEnabled = true;
            volumetricColor = Colors.White;
            maxRayLength = 100f;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public float CalculateVolumetricIntensity(Vector2 point, Vector2 lightPosition)
        {
            if (!isEnabled || sourceLight == null || !sourceLight.IsEnabled)
                return 0f;

            var direction = (point - lightPosition).Normalized();
            var distance = (point - lightPosition).Length();
            var stepSize = Math.Min(distance, maxRayLength) / raySteps;

            var totalIntensity = 0f;
            var illumination = 1f;

            for (float i = 0; i < raySteps; i++)
            {
                var samplePoint = lightPosition + direction * stepSize * i;
                var sampleIntensity = sourceLight.GetIntensityAtPoint(samplePoint);
                totalIntensity += illumination * sampleIntensity * Density;
                illumination *= decay;
            }

            return totalIntensity * Exposure * Weight;
        }

        public List<Vector2> GenerateLightRays(int rayCount, float spreadAngle)
        {
            var rays = new List<Vector2>();
            if (sourceLight == null)
                return rays;

            var lightPos = sourceLight.Transform?.WorldPosition ?? Vector2.Zero;
            var lightDir = Vector2.Up;

            var angleStep = spreadAngle / (rayCount - 1);
            var startAngle = -spreadAngle / 2f;

            for (int i = 0; i < rayCount; i++)
            {
                var angle = startAngle + angleStep * i;
                var rad = angle * (float)Math.PI / 180f;
                var rotatedDir = new Vector2(
                    lightDir.X * (float)Math.Cos(rad) - lightDir.Y * (float)Math.Sin(rad),
                    lightDir.X * (float)Math.Sin(rad) + lightDir.Y * (float)Math.Cos(rad)
                );
                rays.Add(rotatedDir);
            }

            return rays;
        }
    }

    public class AmbientOcclusion2D : Component
    {
        private float radius;
        private float intensity;
        private int samples;
        private bool isEnabled;
        private Color aoColor;
        private float bias;

        public float Radius
        {
            get => radius;
            set
            {
                radius = Math.Max(0f, value);
                OnPropertyChanged();
            }
        }

        public float Intensity
        {
            get => intensity;
            set
            {
                intensity = Math.Clamp(value, 0f, 1f);
                OnPropertyChanged();
            }
        }

        public int Samples
        {
            get => samples;
            set
            {
                samples = Math.Max(1, value);
                OnPropertyChanged();
            }
        }

        public new bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color AOColor
        {
            get => aoColor;
            set
            {
                aoColor = value;
                OnPropertyChanged();
            }
        }

        public float Bias
        {
            get => bias;
            set
            {
                bias = Math.Clamp(value, 0f, 1f);
                OnPropertyChanged();
            }
        }

        public AmbientOcclusion2D()
        {
            radius = 1f;
            intensity = 0.5f;
            samples = 8;
            isEnabled = true;
            aoColor = Colors.Black;
            bias = 0.01f;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public float CalculateAO(Vector2 point, List<ShadowCaster2D> occluders)
        {
            if (!isEnabled || occluders == null || occluders.Count == 0)
                return 0f;

            var occlusion = 0f;
            var sampleCount = 0;

            for (int i = 0; i < samples; i++)
            {
                var angle = (float)(2 * Math.PI * i / samples);
                var sampleDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                var samplePoint = point + sampleDir * radius;

                foreach (var occluder in occluders)
                {
                    if (occluder == null || !occluder.IsEnabled || !occluder.ReceiveShadows)
                        continue;

                    var worldPos = occluder.Transform?.WorldPosition ?? Vector2.Zero;
                    var distToOccluder = (worldPos - point).Length();
                    if (distToOccluder < radius)
                    {
                        occlusion += 1f - (distToOccluder / radius);
                        sampleCount++;
                    }
                }
            }

            if (sampleCount == 0)
                return 0f;

            return (occlusion / (samples * occluders.Count)) * intensity;
        }
    }

    public class BakedLightmap : Component
    {
        private ImageSource lightmapTexture;
        private Vector2 lightmapScale;
        private Vector2 lightmapOffset;
        private float intensity;
        private bool isEnabled;

        public ImageSource LightmapTexture
        {
            get => lightmapTexture;
            set
            {
                lightmapTexture = value;
                OnPropertyChanged();
            }
        }

        public Vector2 LightmapScale
        {
            get => lightmapScale;
            set
            {
                lightmapScale = value;
                OnPropertyChanged();
            }
        }

        public Vector2 LightmapOffset
        {
            get => lightmapOffset;
            set
            {
                lightmapOffset = value;
                OnPropertyChanged();
            }
        }

        public float Intensity
        {
            get => intensity;
            set
            {
                intensity = Math.Clamp(value, 0f, 1f);
                OnPropertyChanged();
            }
        }

        public new bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public BakedLightmap()
        {
            lightmapScale = Vector2.One;
            lightmapOffset = Vector2.Zero;
            intensity = 1f;
            isEnabled = true;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public Color SampleLightmap(Vector2 uv)
        {
            if (lightmapTexture == null)
                return Color.Black;

            var adjustedUV = uv * lightmapScale + lightmapOffset;
            adjustedUV.X = Math.Clamp(adjustedUV.X, 0f, 1f);
            adjustedUV.Y = Math.Clamp(adjustedUV.Y, 0f, 1f);

            return Color.White * intensity;
        }

        public void GenerateLightmap(List<Vector2> vertices, Vector2 textureSize)
        {
        }

        public void LoadLightmap(string path)
        {
        }

        public void ClearLightmap()
        {
            lightmapTexture = null;
            OnPropertyChanged();
        }
    }

    public class LightEventArgs : EventArgs
    {
        public Light2D Light { get; }
        public LightEventArgs(Light2D light)
        {
            Light = light;
        }
    }

    public class LightingSystem : Component
    {
        private static LightingSystem? instance;
        public static LightingSystem Instance => instance ??= new LightingSystem();

        private List<Light2D> lights;
        private List<ShadowCaster2D> shadowCasters;
        private List<LightOccluder2D> occluders;
        private List<VolumetricLight> volumetricLights;
        private GlobalLight? globalLight;
        private Camera? activeCamera;
        private bool isEnabled;
        private bool shadowsEnabled;
        private bool volumetricEnabled;
        private int maxShadowCasters;
        private int maxLightsPerPixel;
        private LightLayerMask globalMask;
        private float shadowSoftness;
        private float shadowResolution;
        private LightLODLevel lodLevel;
        private Dictionary<int, List<Light2D>> lightsByLayer;
        private Dictionary<LightLayerMask, List<ShadowCaster2D>> castersByMask;
        private bool needsUpdate;
        private Bounds viewportBounds;

        public List<Light2D> Lights => lights;
        public List<ShadowCaster2D> ShadowCasters => shadowCasters;
        public GlobalLight? GlobalLight => globalLight;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                MarkDirty();
            }
        }
        public bool ShadowsEnabled
        {
            get => shadowsEnabled;
            set
            {
                shadowsEnabled = value;
                MarkDirty();
            }
        }
        public bool VolumetricEnabled
        {
            get => volumetricEnabled;
            set
            {
                volumetricEnabled = value;
                MarkDirty();
            }
        }
        public int MaxShadowCasters
        {
            get => maxShadowCasters;
            set
            {
                maxShadowCasters = Math.Max(0, value);
                MarkDirty();
            }
        }
        public int MaxLightsPerPixel
        {
            get => maxLightsPerPixel;
            set
            {
                maxLightsPerPixel = Math.Max(1, value);
                MarkDirty();
            }
        }
        public LightLayerMask GlobalMask
        {
            get => globalMask;
            set
            {
                globalMask = value;
                MarkDirty();
            }
        }
        public float ShadowSoftness
        {
            get => shadowSoftness;
            set
            {
                shadowSoftness = Math.Clamp(value, 0f, 1f);
                MarkDirty();
            }
        }
        public float ShadowResolution
        {
            get => shadowResolution;
            set
            {
                shadowResolution = Math.Max(0.1f, value);
                MarkDirty();
            }
        }
        public LightLODLevel LODLevel
        {
            get => lodLevel;
            set
            {
                lodLevel = value;
                MarkDirty();
            }
        }
        public Bounds ViewportBounds => viewportBounds;

        public event EventHandler? LightsChanged;
        public event EventHandler? ShadowsUpdated;

        private LightingSystem()
        {
            lights = new List<Light2D>();
            shadowCasters = new List<ShadowCaster2D>();
            occluders = new List<LightOccluder2D>();
            volumetricLights = new List<VolumetricLight>();
            globalLight = null;
            activeCamera = null;
            isEnabled = true;
            shadowsEnabled = true;
            volumetricEnabled = false;
            maxShadowCasters = 256;
            maxLightsPerPixel = 4;
            globalMask = LightLayerMask.Everything;
            shadowSoftness = 0.5f;
            shadowResolution = 1f;
            lodLevel = LightLODLevel.Full;
            lightsByLayer = new Dictionary<int, List<Light2D>>();
            castersByMask = new Dictionary<LightLayerMask, List<ShadowCaster2D>>();
            needsUpdate = true;
            viewportBounds = new Bounds(Vector2.Zero, new Vector2(1920, 1080));
            instance = this;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            Scene?.AddSystem(this);
        }

        public void RegisterLight(Light2D light)
        {
            if (light == null || lights.Contains(light))
                return;

            lights.Add(light);
            light.LightChanged += OnLightChanged;
            
            var layer = GetLightLayer(light);
            if (!lightsByLayer.ContainsKey(layer))
                lightsByLayer[layer] = new List<Light2D>();
            lightsByLayer[layer].Add(light);

            if (light is GlobalLight global)
                globalLight = global;

            MarkDirty();
        }

        public void UnregisterLight(Light2D light)
        {
            if (light == null || !lights.Contains(light))
                return;

            lights.Remove(light);
            light.LightChanged -= OnLightChanged;

            var layer = GetLightLayer(light);
            if (lightsByLayer.ContainsKey(layer))
                lightsByLayer[layer].Remove(light);

            if (light == globalLight)
                globalLight = null;

            MarkDirty();
        }

        public void RegisterShadowCaster(ShadowCaster2D caster)
        {
            if (caster == null || shadowCasters.Contains(caster))
                return;

            shadowCasters.Add(caster);
            caster.ShadowGeometryChanged += OnCasterChanged;

            if (!castersByMask.ContainsKey(caster.Layer))
                castersByMask[(int)caster.Layer] = new List<ShadowCaster2D>();
            castersByMask[(int)caster.Layer].Add(caster);

            MarkDirty();
        }

        public void UnregisterShadowCaster(ShadowCaster2D caster)
        {
            if (caster == null || !shadowCasters.Contains(caster))
                return;

            shadowCasters.Remove(caster);
            caster.ShadowGeometryChanged -= OnCasterChanged;

            if (castersByMask.ContainsKey((int)caster.Layer))
                castersByMask[(int)caster.Layer].Remove(caster);

            MarkDirty();
        }

        public void RegisterOccluder(LightOccluder2D occluder)
        {
            if (occluder == null || occluders.Contains(occluder))
                return;

            occluders.Add(occluder);
            occluder.GeometryChanged += OnOccluderChanged;
            MarkDirty();
        }

        public void UnregisterOccluder(LightOccluder2D occluder)
        {
            if (occluder == null || !occluders.Contains(occluder))
                return;

            occluders.Remove(occluder);
            occluder.GeometryChanged -= OnOccluderChanged;
            MarkDirty();
        }

        public void RegisterVolumetricLight(VolumetricLight volumetric)
        {
            if (volumetric == null || volumetricLights.Contains(volumetric))
                return;

            volumetricLights.Add(volumetric);
            MarkDirty();
        }

        public void UnregisterVolumetricLight(VolumetricLight volumetric)
        {
            if (volumetric == null || !volumetricLights.Contains(volumetric))
                return;

            volumetricLights.Remove(volumetric);
            MarkDirty();
        }

        public void SetActiveCamera(Camera camera)
        {
            activeCamera = camera;
            UpdateViewportBounds();
            MarkDirty();
        }

        private void UpdateViewportBounds()
        {
            if (activeCamera == null)
                return;

            var cameraPos = activeCamera.Transform?.WorldPosition ?? Vector2.Zero;
            var viewportWidth = activeCamera.ViewportWidth;
            var viewportHeight = activeCamera.ViewportHeight;

            viewportBounds = new Bounds(
                cameraPos,
                new Vector2(viewportWidth, viewportHeight)
            );
        }

        private int GetLightLayer(Light2D light)
        {
            return light.CullingMask;
        }

        private void OnLightChanged(object? sender, LightEventArgs e)
        {
            MarkDirty();
        }

        private void OnCasterChanged(object? sender, EventArgs e)
        {
            MarkDirty();
        }

        private void OnOccluderChanged(object? sender, EventArgs e)
        {
            MarkDirty();
        }

        private void MarkDirty()
        {
            needsUpdate = true;
            LightsChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void Update(float deltaTime)
        {
            if (!isEnabled)
                return;

            if (needsUpdate)
            {
                UpdateVisibility();
                needsUpdate = false;
            }

            UpdateLights(deltaTime);
            UpdateVolumetricLights(deltaTime);
        }

        private void UpdateVisibility()
        {
            foreach (var light in lights)
            {
                if (light == null)
                    continue;

                var wasEnabled = light.IsEnabled;
                var inViewport = viewportBounds.Intersects(light.LightBounds);
                var affectsWorld = light.AffectWorld;

                if (lodLevel == LightLODLevel.Reduced && !inViewport)
                {
                }
                else if (lodLevel == LightLODLevel.Minimal)
                {
                    light.Intensity = light.Intensity * 0.5f;
                }
            }

            if (shadowsEnabled)
            {
                var visibleCasters = new List<ShadowCaster2D>();
                foreach (var caster in shadowCasters)
                {
                    if (caster != null && caster.IsEnabled && visibleCasters.Count < maxShadowCasters)
                    {
                        visibleCasters.Add(caster);
                    }
                }
                ShadowsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateLights(float deltaTime)
        {
            foreach (var light in lights)
            {
                if (light == null || !light.IsEnabled)
                    continue;

                light.Update(deltaTime);
            }
        }

        private void UpdateVolumetricLights(float deltaTime)
        {
            if (!volumetricEnabled)
                return;

            foreach (var volumetric in volumetricLights)
            {
                if (volumetric == null || !volumetric.IsEnabled || volumetric.SourceLight == null)
                    continue;

                volumetric.Update(deltaTime);
            }
        }

        public Color GetLightContributionAtPoint(Vector2 point)
        {
            if (!isEnabled)
                return Colors.Black;

            var totalColor = Colors.Black;

            if (globalLight != null && globalLight.IsEnabled)
            {
                totalColor = totalColor.Add(globalLight.GetColorAtPoint(point));
            }

            foreach (var light in lights)
            {
                if (light == null || !light.IsEnabled || !light.AffectWorld)
                    continue;

                if (!light.IsInRange(point))
                    continue;

                if (!light.CanAffectLayer(LightLayer.Layer0))
                    continue;

                var lightColor = light.GetColorAtPoint(point);

                if (shadowsEnabled && light.ShadowCasting)
                {
                    var shadowIntensity = CalculateShadowAtPoint(point, light);
                    lightColor = lightColor.Scale(1f - shadowIntensity);
                }

                switch (light.BlendMode)
                {
                    case LightBlendMode.Add:
                        totalColor = totalColor.Add(lightColor);
                        break;
                    case LightBlendMode.Multiply:
                        totalColor = totalColor.Multiply(lightColor);
                        break;
                    case LightBlendMode.Max:
                        totalColor = new Color(
                            Math.Max(totalColor.R, lightColor.R),
                            Math.Max(totalColor.G, lightColor.G),
                            Math.Max(totalColor.B, lightColor.B),
                            Math.Max(totalColor.A, lightColor.A)
                        );
                        break;
                }
            }

            return totalColor;
        }

        public float CalculateShadowAtPoint(Vector2 point, Light2D light)
        {
            if (!shadowsEnabled || light == null || !light.ShadowCasting)
                return 0f;

            var totalShadow = 0f;
            var casterCount = 0;

            foreach (var caster in shadowCasters)
            {
                if (caster == null || !caster.IsEnabled || !caster.CastShadows)
                    continue;

                if (!caster.CanBeCastedBy(light))
                    continue;

                var worldBounds = caster.GetWorldBounds();
                if (!worldBounds.Contains(point))
                {
                    var distanceToCenter = (worldBounds.Center - point).Length();
                    var maxDistance = worldBounds.Size.Length() / 2f + light.Range;
                    if (distanceToCenter > maxDistance)
                        continue;
                }

                var shadow = caster.GetShadowAtPoint(point, light.Position - point);
                totalShadow += shadow;
                casterCount++;

                if (casterCount >= maxShadowCasters)
                    break;
            }

            if (casterCount == 0)
                return 0f;

            return Math.Clamp(totalShadow / casterCount, 0f, 1f);
        }

        public List<Light2D> GetLightsAffectingPoint(Vector2 point)
        {
            var result = new List<Light2D>();

            foreach (var light in lights)
            {
                if (light == null || !light.IsEnabled || !light.AffectWorld)
                    continue;

                if (!light.IsInRange(point))
                    continue;

                result.Add(light);
            }

            result.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            return result;
        }

        public List<ShadowCaster2D> GetVisibleShadowCasters(Light2D light)
        {
            var result = new List<ShadowCaster2D>();

            foreach (var caster in shadowCasters)
            {
                if (caster == null || !caster.IsEnabled || !caster.CastShadows)
                    continue;

                if (!caster.CanBeCastedBy(light))
                    continue;

                var worldBounds = caster.GetWorldBounds();
                if (light.LightBounds.Intersects(worldBounds))
                {
                    result.Add(caster);
                }
            }

            return result;
        }

        public void RenderLightGizmos(DrawingContext context)
        {
            if (!isEnabled)
                return;

            foreach (var light in lights)
            {
                if (light == null || !light.IsEnabled || !light.DrawGizmo)
                    continue;

                if (activeCamera != null)
                {
                    var screenPos = WorldToScreen(light.Position, activeCamera);
                    if (!IsPointInViewport(screenPos, activeCamera))
                        continue;
                }

                light.RenderLightGizmo(context);
            }
        }

        private Vector2 WorldToScreen(Vector2 worldPos, Camera camera)
        {
            var cameraPos = camera.Transform?.WorldPosition ?? Vector2.Zero;
            var zoom = camera.Zoom;
            var viewportWidth = camera.ViewportWidth;
            var viewportHeight = camera.ViewportHeight;

            return new Vector2(
                (worldPos.X - cameraPos.X) * zoom + viewportWidth / 2f,
                (worldPos.Y - cameraPos.Y) * zoom + viewportHeight / 2f
            );
        }

        private bool IsPointInViewport(Vector2 screenPos, Camera camera)
        {
            var viewportWidth = camera.ViewportWidth;
            var viewportHeight = camera.ViewportHeight;

            return screenPos.X >= 0 && screenPos.X <= viewportWidth &&
                   screenPos.Y >= 0 && screenPos.Y <= viewportHeight;
        }

        public void ForceUpdate()
        {
            needsUpdate = true;
            UpdateVisibility();
        }

        public void Optimize()
        {
            lights.RemoveAll(l => l == null);
            shadowCasters.RemoveAll(c => c == null);
            occluders.RemoveAll(o => o == null);
            volumetricLights.RemoveAll(v => v == null);

            lights.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            shadowCasters.Sort((a, b) =>
            {
                var aBounds = a.GetWorldBounds();
                var bBounds = b.GetWorldBounds();
                return bBounds.Size.LengthSquared().CompareTo(aBounds.Size.LengthSquared());
            });
        }

        public Dictionary<LightType, int> GetLightCounts()
        {
            var counts = new Dictionary<LightType, int>
            {
                { LightType.Point, 0 },
                { LightType.Spot, 0 },
                { LightType.Directional, 0 },
                { LightType.Global, 0 }
            };

            foreach (var light in lights)
            {
                if (light != null && light.IsEnabled)
                {
                    counts[light.LightType]++;
                }
            }

            return counts;
        }

        public float CalculateTotalLightEnergy()
        {
            float totalEnergy = 0f;

            foreach (var light in lights)
            {
                if (light == null || !light.IsEnabled)
                    continue;

                var area = light.LightType == LightType.Point || light.LightType == LightType.Spot
                    ? (float)Math.PI * light.Range * light.Range
                    : 1000f;

                totalEnergy += light.Intensity * area;
            }

            return totalEnergy;
        }

        public void Clear()
        {
            lights.Clear();
            shadowCasters.Clear();
            occluders.Clear();
            volumetricLights.Clear();
            globalLight = null;
            lightsByLayer.Clear();
            castersByMask.Clear();
            MarkDirty();
        }

        public override string ToString()
        {
            var lightCounts = GetLightCounts();
            return $"LightingSystem(Lights={lights.Count}, Shadows={shadowCasters.Count}, " +
                   $"Volumetric={volumetricLights.Count}, Energy={CalculateTotalLightEnergy():F2})";
        }
    }

    public enum LightLODLevel
    {
        Full,
        Reduced,
        Minimal,
        Off
    }

    public class LightGizmoRenderer : Component
    {
        private bool showLightRadius;
        private bool showLightDirection;
        private bool showShadowCasters;
        private Color gizmoColor;
        private float lineWidth;

        public bool ShowLightRadius
        {
            get => showLightRadius;
            set
            {
                showLightRadius = value;
                OnPropertyChanged();
            }
        }

        public bool ShowLightDirection
        {
            get => showLightDirection;
            set
            {
                showLightDirection = value;
                OnPropertyChanged();
            }
        }

        public bool ShowShadowCasters
        {
            get => showShadowCasters;
            set
            {
                showShadowCasters = value;
                OnPropertyChanged();
            }
        }

        public Color GizmoColor
        {
            get => gizmoColor;
            set
            {
                gizmoColor = value;
                OnPropertyChanged();
            }
        }

        public float LineWidth
        {
            get => lineWidth;
            set
            {
                lineWidth = Math.Max(0.1f, value);
                OnPropertyChanged();
            }
        }

        public LightGizmoRenderer()
        {
            showLightRadius = true;
            showLightDirection = false;
            showShadowCasters = false;
            gizmoColor = Colors.Yellow;
            lineWidth = 1f;
        }

        protected virtual void OnPropertyChanged()
        {
        }

        public void RenderGizmos(DrawingContext context)
        {
            if (LightingSystem.Instance == null)
                return;

            if (showLightRadius || showLightDirection)
            {
                foreach (var light in LightingSystem.Instance.Lights)
                {
                    if (light == null || !light.IsEnabled || !light.DrawGizmo)
                        continue;

                    RenderLightGizmo(context, light);
                }
            }

            if (showShadowCasters)
            {
                foreach (var caster in LightingSystem.Instance.ShadowCasters)
                {
                    if (caster == null || !caster.IsEnabled)
                        continue;

                    RenderCasterGizmo(context, caster);
                }
            }
        }

        private void RenderLightGizmo(DrawingContext context, Light2D light)
        {
            var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (byte)(200 * light.Intensity),
                (byte)(gizmoColor.R * 255),
                (byte)(gizmoColor.G * 255),
                (byte)(gizmoColor.B * 255)
            )), lineWidth);

            if (showLightRadius && (light is PointLight || light is SpotLight))
            {
                var pos = light.Position;
                var radius = light.Range;
                context.DrawEllipse(null, pen, new Point(pos.X, pos.Y), radius, radius);
            }

            if (showLightDirection && light is SpotLight spotLight)
            {
                var pos = light.Position;
                var dir = spotLight.Direction;
                var endPos = pos + dir * spotLight.Range;
                context.DrawLine(pen, new Point(pos.X, pos.Y), new Point(endPos.X, endPos.Y));
            }
        }

        private void RenderCasterGizmo(DrawingContext context, ShadowCaster2D caster)
        {
            var vertices = caster.GetTransformedVertices();
            if (vertices.Count < 2)
                return;

            var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                200,
                (byte)(gizmoColor.R * 255),
                (byte)(gizmoColor.G * 255),
                (byte)(gizmoColor.B * 255)
            )), lineWidth);

            for (int i = 0; i < vertices.Count; i++)
            {
                var v1 = vertices[i];
                var v2 = vertices[(i + 1) % vertices.Count];
                context.DrawLine(pen, new Point(v1.X, v1.Y), new Point(v2.X, v2.Y));
            }
        }
    }

    public class EmissiveMaterial : Component
    {
        private Color emissiveColor;
        private float emissiveIntensity;
        private ImageSource? emissiveTexture;
        private bool useEmissive;

        public Color EmissiveColor
        {
            get => emissiveColor;
            set
            {
                emissiveColor = value;
                UpdateEmissive();
            }
        }

        public float EmissiveIntensity
        {
            get => emissiveIntensity;
            set
            {
                emissiveIntensity = Math.Max(0f, value);
                UpdateEmissive();
            }
        }

        public ImageSource? EmissiveTexture
        {
            get => emissiveTexture;
            set
            {
                emissiveTexture = value;
                UpdateEmissive();
            }
        }

        public bool UseEmissive
        {
            get => useEmissive;
            set
            {
                useEmissive = value;
                UpdateEmissive();
            }
        }

        public EmissiveMaterial()
        {
            emissiveColor = Colors.White;
            emissiveIntensity = 1f;
            useEmissive = true;
        }

        private void UpdateEmissive()
        {
        }

        public Color GetEmissiveColor()
        {
            if (!useEmissive)
                return Colors.Black;

            return new Color(
                emissiveColor.R * emissiveIntensity,
                emissiveColor.G * emissiveIntensity,
                emissiveColor.B * emissiveIntensity,
                emissiveColor.A * emissiveIntensity
            );
        }

        public Color GetEmissiveAtPoint(Vector2 uv)
        {
            var baseEmissive = GetEmissiveColor();

            if (emissiveTexture != null)
            {
            }

            return baseEmissive;
        }
    }

    public class GlowEffect : Component
    {
        private Color glowColor;
        private float glowIntensity;
        private float glowRadius;
        private int glowPasses;
        private bool isEnabled;
        private float threshold;

        public Color GlowColor
        {
            get => glowColor;
            set
            {
                glowColor = value;
                UpdateEffect();
            }
        }

        public float GlowIntensity
        {
            get => glowIntensity;
            set
            {
                glowIntensity = Math.Max(0f, value);
                UpdateEffect();
            }
        }

        public float GlowRadius
        {
            get => glowRadius;
            set
            {
                glowRadius = Math.Max(0f, value);
                UpdateEffect();
            }
        }

        public int GlowPasses
        {
            get => glowPasses;
            set
            {
                glowPasses = Math.Max(1, value);
                UpdateEffect();
            }
        }

        public new bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                UpdateEffect();
            }
        }

        public float Threshold
        {
            get => threshold;
            set
            {
                threshold = Math.Clamp(value, 0f, 1f);
                UpdateEffect();
            }
        }

        public GlowEffect()
        {
            glowColor = Colors.White;
            glowIntensity = 1f;
            glowRadius = 5f;
            glowPasses = 4;
            isEnabled = true;
            threshold = 0.8f;
        }

        private void UpdateEffect()
        {
        }

        public Color ApplyGlow(Color originalColor, float distance)
        {
            if (!isEnabled)
                return originalColor;

            var glowContribution = glowColor.Scale(glowIntensity * (1f - distance / glowRadius));
            return originalColor.Add(glowContribution);
        }

        public float GetGlowFalloff(float distance)
        {
            if (distance >= glowRadius)
                return 0f;

            var t = distance / glowRadius;
            return (1f - t) * glowIntensity;
        }
    }
}

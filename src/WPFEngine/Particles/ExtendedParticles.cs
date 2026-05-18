using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Particles
{
    public enum EmitterShape
    {
        Point,
        Line,
        Circle,
        Rectangle,
        Cone,
        Sphere,
        Hemisphere,
        Edge
    }

    public enum EmissionType
    {
        Continuous,
        Burst,
        Periodic
    }

    public enum Collision2DType
    {
        None,
        Planes,
        WorldBounds,
        CustomPlanes
    }

    public enum Collision3DType
    {
        None,
        Spheres,
        Planes,
        AABBs
    }

    public enum SimulationSpace
    {
        World,
        Local,
        Custom
    }

    public enum RenderMode
    {
        Billboard,
        Stretch,
        HorizontalBillboard,
        VerticalBillboard,
        Mesh
    }

    public enum BlendMode
    {
        Additive,
        Alpha,
        Multiply,
        Screen,
        Opaque
    }

    public enum TrailType
    {
        Ribbon,
        Line,
        Configurable
    }

    public enum CullingMode
    {
        Automatic,
        AlwaysAnimate,
        PauseAndCatchUp
    }

    public enum NoiseType
    {
        Perlin,
        Simplex,
        Value
    }

    public enum VelocityType
    {
        Linear,
        Orbital,
        Constrained
    }

    public enum RotationType
    {
        Constant,
        BySpeed,
        ByLifetime
    }

    public interface IParticleModule
    {
        void Initialize(ExtendedParticleSystem system);
        void Update(ExtendedParticle particle, float deltaTime);
        void Reset();
    }

    public class ExtendedParticle
    {
        public int Id { get; set; }
        public bool IsAlive { get; set; }
        public float Age { get; set; }
        public float Lifetime { get; set; }
        public float NormalizedAge => Lifetime > 0 ? Math.Clamp(Age / Lifetime, 0f, 1f) : 0f;

        public Vector3 Position { get; set; }
        public Vector3 PreviousPosition { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Acceleration { get; set; }

        public Vector3 StartPosition { get; set; }
        public Vector3 StartVelocity { get; set; }

        public float StartSize { get; set; }
        public float Size { get; set; }
        public float EndSize { get; set; }

        public Color StartColor { get; set; }
        public Color Color { get; set; }
        public Color EndColor { get; set; }

        public float StartRotation { get; set; }
        public float Rotation { get; set; }
        public float EndRotation { get; set; }
        public float RotationSpeed { get; set; }
        public float AngularVelocity { get; set; }

        public float StartLifetime { get; set; }
        public float RemainingLifetime { get; set; }

        public int Seed { get; set; }
        public float RandomSeed { get; set; }

        public Vector3[] TrailPositions { get; set; }
        public int TrailIndex { get; set; }
        public int MaxTrailLength { get; set; }

        public float[] CustomData { get; set; }
        public Vector3 MeshScale { get; set; }

        public Vector3[] OrbitalCenter { get; set; }
        public float OrbitalRadius { get; set; }
        public float OrbitalSpeed { get; set; }
        public float OrbitalAngle { get; set; }

        public Vector3[] LightPositions { get; set; }
        public float[] LightIntensities { get; set; }
        public int MaxLights { get; set; }

        public Vector3 InheritedVelocity { get; set; }
        public float VelocityInheritanceFactor { get; set; }

        public int CollisionCount { get; set; }
        public float BounceAmount { get; set; }
        public float LifetimeLossOnCollision { get; set; }

        public Vector3 NoiseOffset { get; set; }
        public float NoiseStrength { get; set; }

        public ExtendedParticle()
        {
            CustomData = new float[8];
            TrailPositions = new Vector3[32];
            MeshScale = Vector3.One;
            OrbitalCenter = new Vector3[1];
            LightPositions = new Vector3[4];
            LightIntensities = new float[4];
            MaxTrailLength = 32;
            MaxLights = 4;
            MaxLights = 4;
            VelocityInheritanceFactor = 0f;
            CollisionCount = 0;
            BounceAmount = 0.5f;
            LifetimeLossOnCollision = 0f;
            NoiseOffset = Vector3.Zero;
            NoiseStrength = 1f;
            Seed = 0;
            RandomSeed = 0f;
        }

        public void Reset()
        {
            Id = 0;
            IsAlive = false;
            Age = 0f;
            Lifetime = 1f;
            Position = Vector3.Zero;
            PreviousPosition = Vector3.Zero;
            Velocity = Vector3.Zero;
            Acceleration = Vector3.Zero;
            StartPosition = Vector3.Zero;
            StartVelocity = Vector3.Zero;
            StartSize = 1f;
            Size = 1f;
            EndSize = 0f;
            StartColor = Color.White;
            Color = Color.White;
            EndColor = Color.Clear;
            StartRotation = 0f;
            Rotation = 0f;
            EndRotation = 0f;
            RotationSpeed = 0f;
            AngularVelocity = 0f;
            StartLifetime = 1f;
            RemainingLifetime = 1f;
            TrailIndex = 0;
            OrbitalRadius = 0f;
            OrbitalSpeed = 0f;
            OrbitalAngle = 0f;
            CollisionCount = 0;
            InheritedVelocity = Vector3.Zero;
            Array.Clear(CustomData, 0, CustomData.Length);
        }

        public void UpdateTrail()
        {
            if (TrailPositions == null || MaxTrailLength <= 0)
                return;

            for (int i = MaxTrailLength - 1; i > 0; i--)
            {
                TrailPositions[i] = TrailPositions[i - 1];
            }
            TrailPositions[0] = Position;
            TrailIndex = Math.Min(TrailIndex + 1, MaxTrailLength - 1);
        }

        public float GetLifetimeRatio() => Lifetime > 0 ? Age / Lifetime : 0f;

        public ExtendedParticle Clone()
        {
            var clone = new ExtendedParticle
            {
                Id = Id,
                IsAlive = IsAlive,
                Age = Age,
                Lifetime = Lifetime,
                Position = Position,
                PreviousPosition = PreviousPosition,
                Velocity = Velocity,
                Acceleration = Acceleration,
                StartPosition = StartPosition,
                StartVelocity = StartVelocity,
                StartSize = StartSize,
                Size = Size,
                EndSize = EndSize,
                StartColor = StartColor,
                Color = Color,
                EndColor = EndColor,
                StartRotation = StartRotation,
                Rotation = Rotation,
                EndRotation = EndRotation,
                RotationSpeed = RotationSpeed,
                AngularVelocity = AngularVelocity,
                StartLifetime = StartLifetime,
                RemainingLifetime = RemainingLifetime,
                Seed = Seed,
                RandomSeed = RandomSeed,
                MaxTrailLength = MaxTrailLength,
                VelocityInheritanceFactor = VelocityInheritanceFactor,
                CollisionCount = CollisionCount,
                BounceAmount = BounceAmount,
                LifetimeLossOnCollision = LifetimeLossOnCollision,
                NoiseOffset = NoiseOffset,
                NoiseStrength = NoiseStrength
            };

            if (TrailPositions != null)
            {
                clone.TrailPositions = new Vector3[TrailPositions.Length];
                Array.Copy(TrailPositions, clone.TrailPositions, TrailPositions.Length);
            }

            if (CustomData != null)
            {
                clone.CustomData = new float[CustomData.Length];
                Array.Copy(CustomData, clone.CustomData, CustomData.Length);
            }

            return clone;
        }
    }

    public class EmissionModule : IParticleModule
    {
        public float Rate { get; set; } = 10f;
        public float RateOverTimeMultiplier { get; set; } = 1f;
        public float RateOverDistanceMultiplier { get; set; } = 0f;
        public List<Burst Emission> Bursts { get; set; } = new();
        public EmissionType Type { get; set; } = EmissionType.Continuous;
        public float Interval { get; set; } = 0.1f;

        private ExtendedParticleSystem? _system;
        private float _accumulator;
        private float _burstTime;
        private int _burstIndex;
        private float _previousRate;

        public void Initialize(ExtendedParticleSystem system)
        {
            _system = system;
            _accumulator = 0f;
            _burstTime = 0f;
            _burstIndex = 0;
            _previousRate = Rate;
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
        }

        public void Reset()
        {
            _accumulator = 0f;
            _burstTime = 0f;
            _burstIndex = 0;
        }

        public void UpdateEmission(float deltaTime, float currentRate, ref int particlesToEmit)
        {
            if (_system == null) return;

            _accumulator += deltaTime * currentRate * RateOverTimeMultiplier;
            particlesToEmit = (int)Math.Floor(_accumulator);
            _accumulator -= particlesToEmit;

            if (Type == EmissionType.Periodic && Interval > 0)
            {
                _burstTime += deltaTime;
                if (_burstTime >= Interval)
                {
                    _burstTime -= Interval;
                    particlesToEmit += 1;
                }
            }

            ProcessBursts(deltaTime, ref particlesToEmit);
        }

        private void ProcessBursts(float deltaTime, ref int particlesToEmit)
        {
            if (Bursts == null || Bursts.Count == 0) return;

            for (int i = 0; i < Bursts.Count; i++)
            {
                var burst = Bursts[i];
                if (burst.Time <= _system!.PlaybackTime && !burst.HasEmitted)
                {
                    if (_system.Random.NextFloat() <= burst.Probability)
                    {
                        particlesToEmit += burst.Count;
                        burst.HasEmitted = true;
                    }
                }
            }
        }

        public void ResetBursts()
        {
            foreach (var burst in Bursts)
            {
                burst.HasEmitted = false;
            }
        }
    }

    public class BurstEmission
    {
        public float Time { get; set; }
        public int Count { get; set; }
        public int CountMultiplier { get; set; } = 1;
        public float Probability { get; set; } = 1f;
        public bool HasEmitted { get; set; }
        public int Cycles { get; set; } = 1;
        public float Interval { get; set; } = 0f;
        public int CurrentCycle { get; set; }
        public float CycleTime { get; set; }
    }

    public class ShapeModule : IParticleModule
    {
        public EmitterShape Shape { get; set; } = EmitterShape.Point;
        public float Radius { get; set; } = 1f;
        public float RadiusThickness { get; set; } = 1f;
        public Vector2 Angle { get; set; } = new Vector2(-25f, 25f);
        public float Arc { get; set; } = 360f;
        public float ArcSpeed { get; set; } = 0f;
        public Vector3 Box { get; set; } = new Vector3(1f, 1f, 1f);
        public float ConeRadius { get; set; } = 1f;
        public float ConeAngle { get; set; } = 45f;
        public float SphereRadius { get; set; } = 1f;
        public float Thickness { get; set; } = 1f;
        public bool AlignToDirection { get; set; } = false;
        public float RandomDirectionAmount { get; set; } = 0f;
        public float SpherizeAmount { get; set; } = 0f;
        public float RandomPositionAmount { get; set; } = 0f;

        private ExtendedParticleSystem? _system;
        private float _arcAngle;

        public void Initialize(ExtendedParticleSystem system)
        {
            _system = system;
            _arcAngle = 0f;
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
        }

        public void Reset()
        {
            _arcAngle = 0f;
        }

        public Vector3 GetEmissionPosition(Vector3 emitterPosition, Vector3 emitterForward, Vector3 emitterUp)
        {
            if (_system == null) return emitterPosition;

            var random = _system.Random;
            Vector3 position = emitterPosition;
            Vector3 forward = emitterForward;
            Vector3 up = emitterUp;

            switch (Shape)
            {
                case EmitterShape.Point:
                    position += forward * random.Range(0f, Radius);
                    break;

                case EmitterShape.Line:
                    float lineT = random.Range(0f, 1f);
                    position += forward * Radius * lineT;
                    break;

                case EmitterShape.Circle:
                    float circleAngle = random.Range(0f, Arc) * MathHelper.Deg2Rad + _arcAngle;
                    _arcAngle += ArcSpeed * 0.016f * MathHelper.Deg2Rad;
                    float circleRadius = Radius * MathHelper.Sqrt(random.Range(0f, 1f)) * RadiusThickness;
                    position += forward * (float)Math.Cos(circleAngle) * circleRadius;
                    position += up * (float)Math.Sin(circleAngle) * circleRadius;
                    break;

                case EmitterShape.Rectangle:
                    position += forward * random.Range(-Box.X * 0.5f, Box.X * 0.5f);
                    position += up * random.Range(-Box.Y * 0.5f, Box.Y * 0.5f);
                    var right = forward.Cross(up).Normalized();
                    position += right * random.Range(-Box.Z * 0.5f, Box.Z * 0.5f);
                    break;

                case EmitterShape.Cone:
                    float coneRadius = random.Range(0f, ConeRadius);
                    float coneAngle = random.Range(Angle.X, Angle.Y) * MathHelper.Deg2Rad;
                    Vector3 coneDir = (forward * (float)Math.Cos(coneAngle) + up * (float)Math.Sin(coneAngle)).Normalized();
                    position += coneDir * coneRadius;
                    break;

                case EmitterShape.Sphere:
                    float sphereR = Radius * MathHelper.Pow(random.Range(0f, 1f), 1f / 3f) * Thickness;
                    float phi = random.Range(0f, 360f) * MathHelper.Deg2Rad;
                    float theta = random.Range(0f, 180f) * MathHelper.Deg2Rad;
                    position += forward * (float)(sphereR * Math.Sin(theta) * Math.Cos(phi));
                    position += up * (float)(sphereR * Math.Sin(theta) * Math.Sin(phi));
                    var sphereRight = forward.Cross(up).Normalized();
                    position += sphereRight * (float)(sphereR * Math.Cos(theta));
                    break;

                case EmitterShape.Hemisphere:
                    float hemiR = Radius * MathHelper.Pow(random.Range(0f, 1f), 1f / 3f) * Thickness;
                    float hemiPhi = random.Range(0f, 180f) * MathHelper.Deg2Rad;
                    float hemiTheta = random.Range(0f, 90f) * MathHelper.Deg2Rad;
                    position += forward * (float)(hemiR * Math.Sin(hemiTheta) * Math.Cos(hemiPhi));
                    position += up * (float)(hemiR * Math.Sin(hemiTheta) * Math.Sin(hemiPhi));
                    var hemiRight = forward.Cross(up).Normalized();
                    position += hemiRight * (float)(hemiR * Math.Cos(hemiTheta));
                    break;

                case EmitterShape.Edge:
                    float edgeT = random.Range(0f, 1f);
                    position += forward * Radius * edgeT;
                    break;
            }

            if (RandomPositionAmount > 0)
            {
                position += new Vector3(
                    random.Range(-1f, 1f),
                    random.Range(-1f, 1f),
                    random.Range(-1f, 1f)
                ) * RandomPositionAmount;
            }

            return position;
        }

        public Vector3 GetEmissionDirection(Vector3 emitterForward, Vector3 emitterUp)
        {
            if (_system == null) return emitterForward;

            var random = _system.Random;
            Vector3 direction = emitterForward;

            if (Angle.X != 0 || Angle.Y != 0)
            {
                float spreadAngle = random.Range(Angle.X, Angle.Y) * MathHelper.Deg2Rad;
                float angleX = spreadAngle * (random.NextFloat() * 2f - 1f);
                float angleY = spreadAngle * (random.NextFloat() * 2f - 1f);

                var right = emitterForward.Cross(emitterUp).Normalized();
                direction = direction.Rotate(emitterUp, angleX);
                direction = direction.Rotate(right, angleY);
            }

            if (RandomDirectionAmount > 0)
            {
                direction += new Vector3(
                    random.Range(-1f, 1f),
                    random.Range(-1f, 1f),
                    random.Range(-1f, 1f)
                ) * RandomDirectionAmount;
                direction.Normalize();
            }

            return direction;
        }
    }

    public class VelocityModule : IParticleModule
    {
        public Vector3 LinearVelocity { get; set; } = Vector3.Zero;
        public Vector3 LinearVelocityVariation { get; set; } = Vector3.Zero;
        public Vector3 OrbitalCenter { get; set; } = Vector3.Zero;
        public float OrbitalSpeed { get; set; } = 0f;
        public float OrbitalRadius { get; set; } = 0f;
        public float OrbitalRadiusVariation { get; set; } = 0f;
        public VelocityType Type { get; set; } = VelocityType.Linear;
        public Vector3 SpeedOverLifetime { get; set; } = Vector3.One;
        public CurveData SpeedBySpeed { get; set; } = new();

        public void Initialize(ExtendedParticleSystem system) { }
        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (Type == VelocityType.Orbital)
            {
                particle.OrbitalAngle += OrbitalSpeed * deltaTime;
                float radius = particle.OrbitalRadius + particle.RandomSeed * OrbitalRadiusVariation;
                Vector3 center = particle.OrbitalCenter[0] + OrbitalCenter;
                particle.Velocity = new Vector3(
                    (float)Math.Cos(particle.OrbitalAngle) * radius,
                    0f,
                    (float)Math.Sin(particle.OrbitalAngle) * radius
                );
            }
            else if (Type == VelocityType.Constrained)
            {
                var speedFactor = SpeedBySpeed.Evaluate(particle.NormalizedAge);
                particle.Velocity = particle.Velocity.Scale(SpeedOverLifetime * speedFactor);
            }
        }

        public void Reset() { }

        public Vector3 GetInitialVelocity()
        {
            return new Vector3(
                LinearVelocity.X + LinearVelocityVariation.X * (2f * (float)new Random().NextDouble() - 1f),
                LinearVelocity.Y + LinearVelocityVariation.Y * (2f * (float)new Random().NextDouble() - 1f),
                LinearVelocity.Z + LinearVelocityVariation.Z * (2f * (float)new Random().NextDouble() - 1f)
            );
        }
    }

    public class ColorModule : IParticleModule
    {
        public List<ColorKey> ColorOverLifetime { get; set; } = new();
        public List<ColorKey> ColorBySpeed { get; set; } = new();
        public bool UseGradient { get; set; } = true;

        public void Initialize(ExtendedParticleSystem system) { }
        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (ColorOverLifetime.Count > 0)
            {
                particle.Color = EvaluateColorOverLifetime(particle.NormalizedAge);
            }
        }

        public void Reset() { }

        public Color EvaluateColorOverLifetime(float normalizedAge)
        {
            if (ColorOverLifetime.Count == 0)
                return Color.White;

            if (ColorOverLifetime.Count == 1)
                return ColorOverLifetime[0].Color;

            for (int i = 0; i < ColorOverLifetime.Count - 1; i++)
            {
                if (normalizedAge >= ColorOverLifetime[i].Time && normalizedAge <= ColorOverLifetime[i + 1].Time)
                {
                    float t = (normalizedAge - ColorOverLifetime[i].Time) /
                              (ColorOverLifetime[i + 1].Time - ColorOverLifetime[i].Time);
                    return ColorOverLifetime[i].Color.Lerp(ColorOverLifetime[i + 1].Color, t);
                }
            }

            return ColorOverLifetime[^1].Color;
        }

        public Color EvaluateColorBySpeed(float speed)
        {
            if (ColorBySpeed.Count == 0)
                return Color.White;

            float minSpeed = ColorBySpeed.Min(k => k.Time);
            float maxSpeed = ColorBySpeed.Max(k => k.Time);
            float normalizedSpeed = maxSpeed > minSpeed ? (speed - minSpeed) / (maxSpeed - minSpeed) : 0f;

            for (int i = 0; i < ColorBySpeed.Count - 1; i++)
            {
                if (normalizedSpeed >= ColorBySpeed[i].Time && normalizedSpeed <= ColorBySpeed[i + 1].Time)
                {
                    float t = (normalizedSpeed - ColorBySpeed[i].Time) /
                              (ColorBySpeed[i + 1].Time - ColorBySpeed[i].Time);
                    return ColorBySpeed[i].Color.Lerp(ColorBySpeed[i + 1].Color, t);
                }
            }

            return ColorBySpeed[^1].Color;
        }
    }

    public class ColorKey
    {
        public float Time { get; set; }
        public Color Color { get; set; }

        public ColorKey() { }

        public ColorKey(float time, Color color)
        {
            Time = time;
            Color = color;
        }
    }

    public class SizeModule : IParticleModule
    {
        public List<SizeKey> SizeOverLifetime { get; set; } = new();
        public List<SizeKey> SizeBySpeed { get; set; } = new();
        public Vector3 SeparateAxes { get; set; } = Vector3.One;
        public float SizeScale { get; set; } = 1f;
        public float MinimumSize { get; set; } = 0f;

        public void Initialize(ExtendedParticleSystem system) { }
        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (SizeOverLifetime.Count > 0)
            {
                float size = EvaluateSizeOverLifetime(particle.NormalizedAge);
                particle.Size = Math.Max(MinimumSize, size * particle.StartSize * SizeScale);
            }
        }

        public void Reset() { }

        public float EvaluateSizeOverLifetime(float normalizedAge)
        {
            if (SizeOverLifetime.Count == 0)
                return 1f;

            if (SizeOverLifetime.Count == 1)
                return SizeOverLifetime[0].Size;

            for (int i = 0; i < SizeOverLifetime.Count - 1; i++)
            {
                if (normalizedAge >= SizeOverLifetime[i].Time && normalizedAge <= SizeOverLifetime[i + 1].Time)
                {
                    float t = (normalizedAge - SizeOverLifetime[i].Time) /
                              (SizeOverLifetime[i + 1].Time - SizeOverLifetime[i].Time);
                    return MathHelper.Lerp(SizeOverLifetime[i].Size, SizeOverLifetime[i + 1].Size, t);
                }
            }

            return SizeOverLifetime[^1].Size;
        }

        public Vector3 EvaluateSeparateAxes(float normalizedAge)
        {
            return new Vector3(
                EvaluateAxis(SizeOverLifetime, normalizedAge) * SeparateAxes.X,
                EvaluateAxis(SizeOverLifetime, normalizedAge) * SeparateAxes.Y,
                EvaluateAxis(SizeOverLifetime, normalizedAge) * SeparateAxes.Z
            );
        }

        private float EvaluateAxis(List<SizeKey> keys, float normalizedAge)
        {
            if (keys.Count == 0) return 1f;
            if (keys.Count == 1) return keys[0].Size;

            for (int i = 0; i < keys.Count - 1; i++)
            {
                if (normalizedAge >= keys[i].Time && normalizedAge <= keys[i + 1].Time)
                {
                    float t = (normalizedAge - keys[i].Time) / (keys[i + 1].Time - keys[i].Time);
                    return MathHelper.Lerp(keys[i].Size, keys[i + 1].Size, t);
                }
            }

            return keys[^1].Size;
        }
    }

    public class SizeKey
    {
        public float Time { get; set; }
        public float Size { get; set; }

        public SizeKey() { }

        public SizeKey(float time, float size)
        {
            Time = time;
            Size = size;
        }
    }

    public class RotationModule : IParticleModule
    {
        public RotationType Type { get; set; } = RotationType.Constant;
        public float AngularVelocity { get; set; } = 0f;
        public float AngularVelocityVariation { get; set; } = 0f;
        public float Lifetime { get; set; } = 0f;
        public float LifetimeVariation { get; set; } = 0f;
        public CurveData Curve { get; set; } = new();
        public bool SeparateAxes { get; set; } = false;
        public Vector3 BySpeedXYZ { get; set; } = Vector3.Zero;

        public void Initialize(ExtendedParticleSystem system) { }
        public void Update(ExtendedParticle particle, float deltaTime)
        {
            switch (Type)
            {
                case RotationType.Constant:
                    particle.AngularVelocity = AngularVelocity + particle.RandomSeed * AngularVelocityVariation;
                    particle.Rotation += particle.AngularVelocity * deltaTime;
                    break;

                case RotationType.BySpeed:
                    particle.Rotation += particle.RotationSpeed * deltaTime;
                    break;

                case RotationType.ByLifetime:
                    if (Curve.IsValid)
                    {
                        particle.Rotation = particle.StartRotation + Curve.Evaluate(particle.NormalizedAge) * MathHelper.Rad2Deg;
                    }
                    break;
            }
        }

        public void Reset() { }

        public float GetInitialAngularVelocity()
        {
            return AngularVelocity + AngularVelocityVariation * (2f * (float)new Random().NextDouble() - 1f);
        }
    }

    public class NoiseModule : IParticleModule
    {
        public bool Enabled { get; set; } = false;
        public NoiseType Type { get; set; } = NoiseType.Perlin;
        public float Strength { get; set; } = 1f;
        public float Frequency { get; set; } = 0.5f;
        public int Octaves { get; set; } = 3;
        public float Lacunarity { get; set; } = 2f;
        public float Persistence { get; set; } = 0.5f;
        public float ScrollSpeed { get; set; } = 0f;
        public Vector3 Offset { get; set; } = Vector3.Zero;
        public Vector3 StrengthOverLifetime { get; set; } = Vector3.One;
        public float Damping { get; set; } = 1f;

        private float[] _permutationTable;
        private System.Random _random;

        public void Initialize(ExtendedParticleSystem system)
        {
            _random = new System.Random();
            GeneratePermutationTable();
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (!Enabled) return;

            float time = _system != null ? _system.PlaybackTime : 0f;
            Vector3 noisePos = particle.Position * Frequency + Offset + new Vector3(0, time * ScrollSpeed, 0);

            float noiseX = SampleNoise(noisePos) * 2f - 1f;
            float noiseY = SampleNoise(noisePos + new Vector3(100, 0, 0)) * 2f - 1f;
            float noiseZ = SampleNoise(noisePos + new Vector3(0, 100, 0)) * 2f - 1f;

            Vector3 noise = new Vector3(noiseX, noiseY, noiseZ) * Strength;

            float strengthFactor = StrengthOverLifetime.X > 0 ? StrengthOverLifetime.X :
                                   StrengthOverLifetime.Y > 0 ? StrengthOverLifetime.Y : StrengthOverLifetime.Z;

            particle.Velocity += noise * strengthFactor * deltaTime * Damping;
        }

        private ExtendedParticleSystem? _system;

        public void SetSystem(ExtendedParticleSystem system)
        {
            _system = system;
        }

        public void Reset()
        {
            Offset = Vector3.Zero;
        }

        private float SampleNoise(Vector3 position)
        {
            switch (Type)
            {
                case NoiseType.Perlin:
                    return PerlinNoise(position);
                case NoiseType.Simplex:
                    return SimplexNoise(position);
                case NoiseType.Value:
                    return ValueNoise(position);
                default:
                    return 0f;
            }
        }

        private void GeneratePermutationTable()
        {
            _permutationTable = new float[256];
            for (int i = 0; i < 256; i++)
            {
                _permutationTable[i] = (float)_random.NextDouble();
            }
        }

        private float PerlinNoise(Vector3 position)
        {
            float x = position.X * Frequency;
            float y = position.Y * Frequency;
            float z = position.Z * Frequency;

            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;
            int zi = (int)Math.Floor(z) & 255;

            float xf = x - (float)Math.Floor(x);
            float yf = y - (float)Math.Floor(y);
            float zf = z - (float)Math.Floor(z);

            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);

            float aaa = _permutationTable[_permutationTable[_permutationTable[xi] + yi] + zi];
            float aba = _permutationTable[_permutationTable[_permutationTable[xi] + yi + 1] + zi];
            float aab = _permutationTable[_permutationTable[_permutationTable[xi] + yi] + zi + 1];
            float abb = _permutationTable[_permutationTable[_permutationTable[xi] + yi + 1] + zi + 1];
            float baa = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi] + zi];
            float bba = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi + 1] + zi];
            float bab = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi] + zi + 1];
            float bbb = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi + 1] + zi + 1];

            float x1 = MathHelper.Lerp(Gradient(aaa, xf, yf, zf), Gradient(baa, xf - 1, yf, zf), u);
            float x2 = MathHelper.Lerp(Gradient(aba, xf, yf - 1, zf), Gradient(bba, xf - 1, yf - 1, zf), u);
            float y1 = MathHelper.Lerp(x1, x2, v);
            x1 = MathHelper.Lerp(Gradient(aab, xf, yf, zf - 1), Gradient(bab, xf - 1, yf, zf - 1), u);
            x2 = MathHelper.Lerp(Gradient(abb, xf, yf - 1, zf - 1), Gradient(bbb, xf - 1, yf - 1, zf - 1), u);
            float y2 = MathHelper.Lerp(x1, x2, v);

            return MathHelper.Lerp(y1, y2, w);
        }

        private float SimplexNoise(Vector3 position)
        {
            return PerlinNoise(position);
        }

        private float ValueNoise(Vector3 position)
        {
            int xi = (int)Math.Floor(position.X) & 255;
            int yi = (int)Math.Floor(position.Y) & 255;
            int zi = (int)Math.Floor(position.Z) & 255;

            float xf = position.X - (float)Math.Floor(position.X);
            float yf = position.Y - (float)Math.Floor(position.Y);
            float zf = position.Z - (float)Math.Floor(position.Z);

            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);

            int aaa = _permutationTable[_permutationTable[_permutationTable[xi] + yi] + zi];
            int aba = _permutationTable[_permutationTable[_permutationTable[xi] + yi + 1] + zi];
            int aab = _permutationTable[_permutationTable[_permutationTable[xi] + yi] + zi + 1];
            int abb = _permutationTable[_permutationTable[_permutationTable[xi] + yi + 1] + zi + 1];
            int baa = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi] + zi];
            int bba = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi + 1] + zi];
            int bab = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi] + zi + 1];
            int bbb = _permutationTable[_permutationTable[_permutationTable[xi + 1] + yi + 1] + zi + 1];

            float x1 = MathHelper.Lerp(aaa, baa, u);
            float x2 = MathHelper.Lerp(aba, bba, u);
            float y1 = MathHelper.Lerp(x1, x2, v);
            x1 = MathHelper.Lerp(aab, bab, u);
            x2 = MathHelper.Lerp(abb, bbb, u);
            float y2 = MathHelper.Lerp(x1, x2, v);

            return MathHelper.Lerp(y1, y2, w);
        }

        private float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        private float Gradient(float hash, float x, float y, float z)
        {
            int h = (int)(hash * 255);
            h = (h << 8) | (h ^ (h >> 4));
            h = (h << 8) | (h ^ (h >> 2));
            h = (h << 8) | (h ^ (h >> 1));

            switch (h & 15)
            {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                case 4: return x + z;
                case 5: return -x + z;
                case 6: return x - z;
                case 7: return -x - z;
                case 8: return y + z;
                case 9: return -y + z;
                case 10: return y - z;
                case 11: return -y - z;
                case 12: return y + x;
                case 13: return -y + z;
                case 14: return y - x;
                case 15: return -y - z;
            }
            return 0f;
        }
    }

    public class TriggerModule : IParticleModule
    {
        public bool Enabled { get; set; } = false;
        public TriggerType Type { get; set; } = TriggerType.OnInterval;
        public float Interval { get; set; } = 1f;
        public float Radius { get; set; } = 1f;
        public List<Vector3> CollisionPlanes { get; set; } = new();
        public TriggerAction Action { get; set; } = TriggerAction.Kill;
        public float LifetimeLoss { get; set; } = 0.1f;
        public int MaxCollisions { get; set; } = 1;
        public float Bounce { get; set; } = 0.5f;

        private float _accumulator;

        public void Initialize(ExtendedParticleSystem system)
        {
            _accumulator = 0f;
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (!Enabled) return;

            switch (Type)
            {
                case TriggerType.OnInterval:
                    _accumulator += deltaTime;
                    if (_accumulator >= Interval)
                    {
                        Action?.Invoke(particle);
                        _accumulator = 0f;
                    }
                    break;

                case TriggerType.OnCollision:
                    CheckCollisions(particle, deltaTime);
                    break;
            }
        }

        public void Reset()
        {
            _accumulator = 0f;
        }

        private void CheckCollisions(ExtendedParticle particle, float deltaTime)
        {
            foreach (var plane in CollisionPlanes)
            {
                float distance = Vector3.Dot(particle.Position - plane, plane.Normalized());
                if (distance < Radius)
                {
                    particle.CollisionCount++;
                    if (particle.CollisionCount >= MaxCollisions)
                    {
                        Action?.Invoke(particle);
                        return;
                    }

                    particle.Velocity = particle.Velocity.Reflect(plane.Normalized()) * Bounce;
                    particle.RemainingLifetime -= LifetimeLoss;
                    particle.Position = plane + plane.Normalized() * Radius;
                }
            }
        }

        public event Action<ExtendedParticle>? Action;
    }

    public enum TriggerType
    {
        OnInterval,
        OnCollision,
        OnDeath
    }

    public enum TriggerAction
    {
        None,
        Kill,
        Callback,
        Pause
    }

    public class LightsModule : IParticleModule
    {
        public bool Enabled { get; set; } = false;
        public int MaxLights { get; set; } = 4;
        public float Ratio { get; set; } = 1f;
        public Color LightColor { get; set; } = Color.White;
        public float Intensity { get; set; } = 1f;
        public float Range { get; set; } = 10f;
        public bool UseParticleColor { get; set; } = true;
        public bool UseParticleSize { get; set; } = true;
        public float SizeFade { get; set; } = 1f;

        private List<LightInfo> _activeLights;
        private int _lightIndex;

        public LightsModule()
        {
            _activeLights = new List<LightInfo>();
        }

        public void Initialize(ExtendedParticleSystem system)
        {
            _activeLights.Clear();
            _lightIndex = 0;
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (!Enabled) return;
            if (_activeLights.Count >= MaxLights) return;
            if (particle.RandomSeed > Ratio) return;

            var light = new LightInfo
            {
                Position = particle.Position,
                Color = UseParticleColor ? particle.Color * LightColor : LightColor,
                Intensity = Intensity * (1f - particle.NormalizedAge * SizeFade),
                Range = Range * (UseParticleSize ? particle.Size : 1f)
            };

            _activeLights.Add(light);
        }

        public void Reset()
        {
            _activeLights.Clear();
        }

        public List<LightInfo> GetActiveLights() => _activeLights;
    }

    public class LightInfo
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public float Intensity { get; set; }
        public float Range { get; set; }
    }

    public class TrailsModule : IParticleModule
    {
        public bool Enabled { get; set; } = false;
        public TrailType Type { get; set; } = TrailType.Ribbon;
        public Material? TrailMaterial { get; set; }
        public float Lifetime { get; set; } = 1f;
        public float MinimumVertexDistance { get; set; } = 0.1f;
        public int MaximumTrailPoints { get; set; } = 100;
        public float WidthOverTrail { get; set; } = 1f;
        public CurveData WidthOverTrailCurve { get; set; } = new();
        public Color TrailColor { get; set; } = Color.White;
        public bool InheritParticleColor { get; set; } = true;
        public bool InheritParticleSize { get; set; } = true;
        public RatioMode RatioMode { get; set; } = RatioMode.Distance;
        public int Ratio { get; set; } = 100;
        public DieMode DieWithParticle { get; set; } = DieMode.Fade;

        private List<TrailPoint> _trailPoints;
        private Vector3 _lastPosition;

        public TrailsModule()
        {
            _trailPoints = new List<TrailPoint>();
        }

        public void Initialize(ExtendedParticleSystem system)
        {
            _trailPoints.Clear();
            _lastPosition = Vector3.Zero;
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (!Enabled) return;

            float distance = (particle.Position - _lastPosition).Length();
            if (distance < MinimumVertexDistance) return;

            var point = new TrailPoint
            {
                Position = particle.Position,
                Color = InheritParticleColor ? particle.Color : TrailColor,
                Width = particle.Size * WidthOverTrail * (InheritParticleSize ? 1f : 1f),
                Age = 0f,
                Lifetime = Lifetime
            };

            _trailPoints.Add(point);
            _lastPosition = particle.Position;

            TrimTrailPoints();
            UpdateTrailAges(deltaTime);
        }

        public void Reset()
        {
            _trailPoints.Clear();
        }

        private void TrimTrailPoints()
        {
            while (_trailPoints.Count > MaximumTrailPoints)
            {
                _trailPoints.RemoveAt(0);
            }
        }

        private void UpdateTrailAges(float deltaTime)
        {
            for (int i = _trailPoints.Count - 1; i >= 0; i--)
            {
                _trailPoints[i].Age += deltaTime;
                if (_trailPoints[i].Age >= _trailPoints[i].Lifetime)
                {
                    _trailPoints.RemoveAt(i);
                }
            }
        }

        public List<TrailPoint> GetTrailPoints() => _trailPoints;
    }

    public class TrailPoint
    {
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public float Width { get; set; }
        public float Age { get; set; }
        public float Lifetime { get; set; }
        public float Alpha => 1f - (Age / Lifetime);
    }

    public enum RatioMode
    {
        Distance,
        Time,
        Life
    }

    public enum DieMode
    {
        Fade,
        Follow,
        Persist
    }

    public class CustomDataModule : IParticleModule
    {
        public List<CustomDataType> DataTypes { get; set; } = new();
        public List<CustomDataValue> CustomData1 { get; set; } = new();
        public List<CustomDataValue> CustomData2 { get; set; } = new();

        public void Initialize(ExtendedParticleSystem system) { }
        public void Update(ExtendedParticle particle, float deltaTime) { }
        public void Reset() { }

        public void SetParticleData(ExtendedParticle particle, int dataIndex, float value)
        {
            if (dataIndex < particle.CustomData.Length)
            {
                particle.CustomData[dataIndex] = value;
            }
        }

        public float GetParticleData(ExtendedParticle particle, int dataIndex)
        {
            if (dataIndex < particle.CustomData.Length)
            {
                return particle.CustomData[dataIndex];
            }
            return 0f;
        }
    }

    public class CustomDataType
    {
        public string Name { get; set; } = "";
        public CustomDataValueType ValueType { get; set; }
    }

    public enum CustomDataValueType
    {
        Float,
        Vector2,
        Vector3,
        Color
    }

    public class CustomDataValue
    {
        public float FloatValue { get; set; }
        public Vector2 Vector2Value { get; set; }
        public Vector3 Vector3Value { get; set; }
        public Color ColorValue { get; set; }
    }

    public class CurveData
    {
        public List<CurveKey> Keys { get; set; } = new();
        public bool IsValid => Keys.Count > 0;

        public float Evaluate(float t)
        {
            if (Keys.Count == 0) return 1f;
            if (Keys.Count == 1) return Keys[0].Value;

            for (int i = 0; i < Keys.Count - 1; i++)
            {
                if (t >= Keys[i].Time && t <= Keys[i + 1].Time)
                {
                    float localT = (t - Keys[i].Time) / (Keys[i + 1].Time - Keys[i].Time);
                    return MathHelper.Lerp(Keys[i].Value, Keys[i + 1].Value, localT);
                }
            }

            return Keys[^1].Value;
        }
    }

    public class CurveKey
    {
        public float Time { get; set; }
        public float Value { get; set; }
        public float InTangent { get; set; }
        public float OutTangent { get; set; }

        public CurveKey() { }

        public CurveKey(float time, float value)
        {
            Time = time;
            Value = value;
        }
    }

    public class CollisionModule2D : IParticleModule
    {
        public Collision2DType Type { get; set; } = Collision2DType.None;
        public float Damping { get; set; } = 0.5f;
        public float Bounce { get; set; } = 0.5f;
        public float LifetimeLoss { get; set; } = 0f;
        public int MaxCollisions { get; set; } = 1;
        public List<Plane> Planes { get; set; } = new();
        public Vector3 WorldBoundsMin { get; set; } = new Vector3(-50, -50, -50);
        public Vector3 WorldBoundsMax { get; set; } = new Vector3(50, 50, 50);
        public bool VisualizeBounds { get; set; } = false;

        public void Initialize(ExtendedParticleSystem system)
        {
            if (Type == Collision2DType.WorldBounds && Planes.Count == 0)
            {
                CreateWorldBoundsPlanes();
            }
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (Type == Collision2DType.None) return;

            foreach (var plane in Planes)
            {
                ProcessPlaneCollision(particle, plane);
            }
        }

        public void Reset() { }

        private void CreateWorldBoundsPlanes()
        {
            Vector3 center = (WorldBoundsMin + WorldBoundsMax) * 0.5f;
            Vector3 extents = (WorldBoundsMax - WorldBoundsMin) * 0.5f;

            Planes.Add(new Plane { Position = center + new Vector3(0, 0, -extents.Z), Normal = Vector3.Forward });
            Planes.Add(new Plane { Position = center + new Vector3(0, 0, extents.Z), Normal = Vector3.Back });
            Planes.Add(new Plane { Position = center + new Vector3(-extents.X, 0, 0), Normal = Vector3.Right });
            Planes.Add(new Plane { Position = center + new Vector3(extents.X, 0, 0), Normal = Vector3.Left });
            Planes.Add(new Plane { Position = center + new Vector3(0, -extents.Y, 0), Normal = Vector3.Up });
            Planes.Add(new Plane { Position = center + new Vector3(0, extents.Y, 0), Normal = Vector3.Down });
        }

        private void ProcessPlaneCollision(ExtendedParticle particle, Plane plane)
        {
            float distance = Vector3.Dot(particle.Position - plane.Position, plane.Normal);
            if (distance < 0) return;

            Vector3 velocityDotNormal = plane.Normal * Vector3.Dot(particle.Velocity, plane.Normal);
            Vector3 velocityDotPlane = particle.Velocity - velocityDotNormal;

            particle.Velocity = velocityDotPlane * Damping - velocityDotNormal * Bounce;
            particle.Position = plane.Position + plane.Normal * (distance + 0.01f);

            particle.CollisionCount++;
            particle.RemainingLifetime -= LifetimeLoss;

            if (particle.CollisionCount >= MaxCollisions)
            {
                particle.IsAlive = false;
            }
        }
    }

    public class Plane
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }

        public float GetDistance(Vector3 point)
        {
            return Vector3.Dot(point - Position, Normal);
        }
    }

    public class CollisionModule3D : IParticleModule
    {
        public Collision3DType Type { get; set; } = Collision3DType.None;
        public float Damping { get; set; } = 0.5f;
        public float Bounce { get; set; } = 0.5f;
        public float LifetimeLoss { get; set; } = 0f;
        public int MaxCollisions { get; set; } = 1;
        public List<SphereCollider> Spheres { get; set; } = new();
        public List<Plane> Planes { get; set; } = new();
        public bool SendCollisionMessage { get; set; } = true;

        public event Action<ExtendedParticle, Vector3>? OnParticleCollide;

        public void Initialize(ExtendedParticleSystem system) { }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
            if (Type == Collision3DType.None) return;

            switch (Type)
            {
                case Collision3DType.Spheres:
                    ProcessSphereCollisions(particle);
                    break;
                case Collision3DType.Planes:
                    ProcessPlaneCollisions(particle);
                    break;
            }
        }

        public void Reset() { }

        private void ProcessSphereCollisions(ExtendedParticle particle)
        {
            foreach (var sphere in Spheres)
            {
                Vector3 toParticle = particle.Position - sphere.Center;
                float distance = toParticle.Length();

                if (distance < sphere.Radius)
                {
                    Vector3 normal = distance > 0.001f ? toParticle.Normalized() : Vector3.Up;
                    particle.Velocity = particle.Velocity.Reflect(normal) * Bounce;
                    particle.Position = sphere.Center + normal * sphere.Radius;

                    particle.CollisionCount++;
                    particle.RemainingLifetime -= LifetimeLoss;

                    if (SendCollisionMessage)
                    {
                        OnParticleCollide?.Invoke(particle, sphere.Center);
                    }

                    if (particle.CollisionCount >= MaxCollisions)
                    {
                        particle.IsAlive = false;
                        break;
                    }
                }
            }
        }

        private void ProcessPlaneCollisions(ExtendedParticle particle)
        {
            foreach (var plane in Planes)
            {
                float distance = Vector3.Dot(particle.Position - plane.Position, plane.Normal);
                if (distance < 0) continue;

                particle.Velocity = particle.Velocity.Reflect(plane.Normal) * Bounce;
                particle.Position = plane.Position + plane.Normal * (distance + 0.01f);

                particle.CollisionCount++;
                particle.RemainingLifetime -= LifetimeLoss;

                if (SendCollisionMessage)
                {
                    OnParticleCollide?.Invoke(particle, plane.Position);
                }

                if (particle.CollisionCount >= MaxCollisions)
                {
                    particle.IsAlive = false;
                    break;
                }
            }
        }

        public void AddSphere(Vector3 center, float radius)
        {
            Spheres.Add(new SphereCollider { Center = center, Radius = radius });
        }

        public void AddPlane(Vector3 position, Vector3 normal)
        {
            Planes.Add(new Plane { Position = position, Normal = normal.Normalized() });
        }

        public void ClearColliders()
        {
            Spheres.Clear();
            Planes.Clear();
        }
    }

    public class SphereCollider
    {
        public Vector3 Center { get; set; }
        public float Radius { get; set; }
    }

    public class SimulationModule : IParticleModule
    {
        public CullingMode CullingMode { get; set; } = CullingMode.Automatic;
        public float TimeScale { get; set; } = 1f;
        public float GravityScale { get; set; } = 1f;
        public Vector3 Gravity { get; set; } = new Vector3(0, -9.81f, 0);
        public float SpeedScale { get; set; } = 1f;
        public bool UsePxelMode { get; set; } = false;
        public bool UseUnsafeMode { get; set; } = false;
        public int MaxParticlesBeforeCulling { get; set; } = 1000;
        public float DeltaTimeCap { get; set; } = 0.1f;

        private float _scaledDeltaTime;
        private float _elapsedTime;

        public void Initialize(ExtendedParticleSystem system)
        {
            _scaledDeltaTime = 0f;
            _elapsedTime = 0f;
        }

        public void Update(ExtendedParticle particle, float deltaTime)
        {
            float scaledDelta = Math.Min(deltaTime * TimeScale, DeltaTimeCap);
            _scaledDeltaTime = scaledDelta;
            _elapsedTime += scaledDelta;

            if (GravityScale > 0)
            {
                particle.Velocity += Gravity * GravityScale * scaledDelta;
            }

            if (SpeedScale != 1f)
            {
                particle.Velocity *= SpeedScale;
            }
        }

        public void Reset()
        {
            _scaledDeltaTime = 0f;
            _elapsedTime = 0f;
        }

        public float GetScaledDeltaTime() => _scaledDeltaTime;
        public float GetElapsedTime() => _elapsedTime;

        public bool ShouldCull(ExtendedParticle particle, Vector3 cameraPosition)
        {
            if (CullingMode == CullingMode.AlwaysAnimate) return false;

            float maxDistance = MaxParticlesBeforeCulling * 0.1f;
            return particle.Position.Distance(cameraPosition) > maxDistance;
        }
    }

    public class RenderModule
    {
        public RenderMode Mode { get; set; } = RenderMode.Billboard;
        public BlendMode Blend { get; set; } = BlendMode.Alpha;
        public Material? Material { get; set; }
        public Mesh? Mesh { get; set; }
        public Vector3 MeshScale { get; set; } = Vector3.One;
        public bool Sorted { get; set; } = false;
        public int SortingLayer { get; set; } = 0;
        public int OrderInLayer { get; set; } = 0;
        public float StretchParticles { get; set; } = 1f;
        public float VelocityScale { get; set; } = 1f;
        public float LengthScale { get; set; } = 2f;
        public float StartRotation { get; set; } = 0f;
        public Vector3 CameraPosition { get; set; } = Vector3.Zero;
        public bool Flip { get; set; } = false;

        public void ApplyBillboard(ExtendedParticle particle, Vector3 cameraPosition)
        {
            switch (Mode)
            {
                case RenderMode.Billboard:
                    ApplyBillboardRotation(particle, cameraPosition);
                    break;
                case RenderMode.HorizontalBillboard:
                    ApplyHorizontalBillboard(particle, cameraPosition);
                    break;
                case RenderMode.VerticalBillboard:
                    ApplyVerticalBillboard(particle, cameraPosition);
                    break;
                case RenderMode.Stretch:
                    ApplyStretch(particle);
                    break;
            }
        }

        private void ApplyBillboardRotation(ExtendedParticle particle, Vector3 cameraPosition)
        {
            Vector3 toCamera = (cameraPosition - particle.Position).Normalized();
            float angle = (float)Math.Atan2(toCamera.X, toCamera.Z);
            particle.Rotation = angle + StartRotation * MathHelper.Deg2Rad;
        }

        private void ApplyHorizontalBillboard(ExtendedParticle particle, Vector3 cameraPosition)
        {
            particle.Rotation = 0f;
        }

        private void ApplyVerticalBillboard(ExtendedParticle particle, Vector3 cameraPosition)
        {
            particle.Rotation = (float)Math.Atan2(
                cameraPosition.X - particle.Position.X,
                cameraPosition.Z - particle.Position.Z
            );
        }

        private void ApplyStretch(ExtendedParticle particle)
        {
            float speed = particle.Velocity.Length();
            float stretch = Math.Min(speed * LengthScale * StretchParticles, 10f);
            particle.MeshScale = new Vector3(1f, stretch, 1f);
        }
    }

    public class Material
    {
        public Color Color { get; set; } = Color.White;
        public float Opacity { get; set; } = 1f;
        public BlendMode BlendMode { get; set; } = BlendMode.Alpha;
        public string? TexturePath { get; set; }
        public bool UseTexture { get; set; } = false;
        public Vector2 Tiling { get; set; } = Vector2.One;
        public Vector2 Offset { get; set; } = Vector2.Zero;
    }

    public class Mesh
    {
        public Vector3[] Vertices { get; set; } = Array.Empty<Vector3>();
        public int[] Triangles { get; set; } = Array.Empty<int>();
        public Vector3[] Normals { get; set; } = Array.Empty<Vector3>();
        public Vector2[] UVs { get; set; } = Array.Empty<Vector2>();

        public static Mesh Cube(float size = 1f)
        {
            float h = size * 0.5f;
            return new Mesh
            {
                Vertices = new Vector3[]
                {
                    new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(-h, h, -h),
                    new Vector3(-h, -h, h), new Vector3(h, -h, h), new Vector3(h, h, h), new Vector3(-h, h, h)
                },
                Triangles = new int[]
                {
                    0, 2, 1, 0, 3, 2,
                    4, 5, 6, 4, 6, 7,
                    0, 1, 5, 0, 5, 4,
                    2, 6, 5, 2, 5, 1,
                    0, 7, 6, 0, 6, 2,
                    3, 4, 7, 3, 4, 0
                }
            };
        }
    }

    public class GPUParticleEmitter
    {
        public int MaxParticles { get; set; } = 10000;
        public string? ComputeShaderPath { get; set; }
        public bool IsInitialized { get; set; } = false;

        public virtual void Initialize(int maxParticles)
        {
            MaxParticles = maxParticles;
            IsInitialized = true;
        }

        public virtual void Emit(int count, GPUParticleData data)
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void Render()
        {
        }

        public virtual void Dispose()
        {
            IsInitialized = false;
        }
    }

    public class GPUParticleData
    {
        public Vector3[] Positions { get; set; } = Array.Empty<Vector3>();
        public Vector3[] Velocities { get; set; } = Array.Empty<Vector3>();
        public float[] Lifetimes { get; set; } = Array.Empty<float>();
        public float[] Sizes { get; set; } = Array.Empty<float>();
        public Color[] Colors { get; set; } = Array.Empty<Color>();
    }

    public class ComputeShaderParticleSystem
    {
        public GPUParticleEmitter Emitter { get; set; }
        public bool SupportsComputeShaders { get; set; } = false;
        public int WorkGroupSize { get; set; } = 256;

        public ComputeShaderParticleSystem()
        {
            Emitter = new GPUParticleEmitter();
        }

        public void Initialize(int maxParticles)
        {
            if (SupportsComputeShaders)
            {
                Emitter.Initialize(maxParticles);
            }
        }

        public void Simulate(float deltaTime)
        {
            if (Emitter.IsInitialized)
            {
                Emitter.Update(deltaTime);
            }
        }

        public void Render()
        {
            if (Emitter.IsInitialized)
            {
                Emitter.Render();
            }
        }
    }

    public class RibbonTrail
    {
        private List<RibbonSegment> _segments;
        private int _maxSegments;
        private float _width;
        private Color _color;
        private bool _isEmitting;

        public int MaxSegments
        {
            get => _maxSegments;
            set => _maxSegments = Math.Max(2, value);
        }

        public RibbonTrail(int maxSegments = 50)
        {
            _maxSegments = maxSegments;
            _segments = new List<RibbonSegment>();
            _width = 1f;
            _color = Color.White;
            _isEmitting = false;
        }

        public void StartEmitting(Vector3 position, Vector3 normal)
        {
            _isEmitting = true;
            _segments.Clear();
            AddSegment(position, normal);
        }

        public void StopEmitting()
        {
            _isEmitting = false;
        }

        public void Update(Vector3 position, Vector3 normal, float deltaTime)
        {
            if (!_isEmitting) return;

            if (_segments.Count == 0 ||
                (_segments[0].Position - position).LengthSquared() > 0.01f)
            {
                AddSegment(position, normal);
                TrimSegments();
            }
        }

        private void AddSegment(Vector3 position, Vector3 normal)
        {
            var segment = new RibbonSegment
            {
                Position = position,
                Normal = normal,
                Color = _color,
                Width = _width,
                Age = 0f
            };

            _segments.Insert(0, segment);
        }

        private void TrimSegments()
        {
            while (_segments.Count > _maxSegments)
            {
                _segments.RemoveAt(_segments.Count - 1);
            }
        }

        public void UpdateAge(float deltaTime, float lifetime)
        {
            for (int i = _segments.Count - 1; i >= 0; i--)
            {
                _segments[i].Age += deltaTime;
                if (_segments[i].Age >= lifetime)
                {
                    _segments.RemoveAt(i);
                }
            }
        }

        public List<RibbonSegment> GetSegments() => _segments;
    }

    public class RibbonSegment
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Color Color { get; set; }
        public float Width { get; set; }
        public float Age { get; set; }
    }

    public class DecalProjector
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;
        public Material? Material { get; set; }
        public float FadeScale { get; set; } = 1f;
        public float MaxDistance { get; set; } = 100f;
        public float AngleOffset { get; set; } = 0f;
        public bool IgnoreLayers { get; set; } = false;
        public int[] LayerMask { get; set; } = Array.Empty<int>();

        public void Project(ExtendedParticle particle)
        {
            Vector3 toParticle = particle.Position - Position;
            float distance = toParticle.Length();

            if (distance > MaxDistance) return;

            float hitDot = Vector3.Dot(toParticle.Normalized(), Vector3.Forward);
            if (hitDot < Math.Cos(AngleOffset * MathHelper.Deg2Rad)) return;

            float scale = Scale.X * (1f - distance / MaxDistance);
            particle.Size *= scale;
        }
    }

    public class LightProjector
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; } = Vector3.Forward;
        public Color Color { get; set; } = Color.White;
        public float Intensity { get; set; } = 1f;
        public float Range { get; set; } = 10f;
        public float SpotAngle { get; set; } = 180f;
        public float Falloff { get; set; } = 1f;
        public bool Cookie { get; set; } = false;
        public string? CookieTexture { get; set; }

        public void ApplyToParticle(ExtendedParticle particle)
        {
            Vector3 toParticle = particle.Position - Position;
            float distance = toParticle.Length();

            if (distance > Range) return;

            float spotEffect = 1f;
            if (SpotAngle < 180f)
            {
                float angle = (float)Math.Acos(Vector3.Dot(toParticle.Normalized(), -Direction));
                spotEffect = Math.Clamp(1f - angle / (SpotAngle * 0.5f * MathHelper.Deg2Rad), 0f, 1f);
            }

            float falloff = 1f - Math.Pow(distance / Range, Falloff);
            float lightIntensity = Intensity * spotEffect * falloff;

            particle.Color = particle.Color * Color * lightIntensity;
        }
    }

    public class SoundEmitter
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public string? AudioClip { get; set; }
        public float Volume { get; set; } = 1f;
        public float Pitch { get; set; } = 1f;
        public float SpatialBlend { get; set; } = 1f;
        public float MaxDistance { get; set; } = 500f;
        public float MinDistance { get; set; } = 1f;
        public bool Loop { get; set; } = false;
        public bool PlayOnAwake { get; set; } = true;
        public DopplerLevel DopplerLevel { get; set; } = DopplerLevel.Standard;
        public float PanStereo { get; set; } = 0f;
        public int Priority { get; set; } = 128;

        private bool _isPlaying;
        private float _currentVolume;

        public bool IsPlaying => _isPlaying;

        public event Action? SoundFinished;

        public void Play()
        {
            _isPlaying = true;
            _currentVolume = Volume;
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        public void UnPause()
        {
            _isPlaying = true;
        }

        public void SetVolume(float volume)
        {
            Volume = Math.Clamp(volume, 0f, 1f);
            _currentVolume = Volume;
        }

        public void SetPitch(float pitch)
        {
            Pitch = Math.Clamp(pitch, 0.5f, 2f);
        }

        public void Update(ExtendedParticle particle)
        {
            Position = particle.Position;
            Velocity = particle.Velocity;
        }

        public void Update()
        {
            if (!_isPlaying) return;
        }

        public float GetVolumeAtDistance(float listenerDistance)
        {
            if (listenerDistance > MaxDistance) return 0f;
            if (listenerDistance < MinDistance) return _currentVolume;

            float normalizedDistance = (listenerDistance - MinDistance) / (MaxDistance - MinDistance);
            return _currentVolume * (1f - normalizedDistance);
        }

        public float GetPan(Vector3 listenerPosition)
        {
            if (SpatialBlend <= 0f) return PanStereo;

            Vector3 toListener = (listenerPosition - Position);
            float distance = toListener.Length();
            float pan = Vector3.Dot(toListener.Normalized(), Vector3.Right);

            return pan * SpatialBlend * GetVolumeAtDistance(distance);
        }
    }

    public enum DopplerLevel
    {
        Off,
        Low,
        Standard,
        High
    }

    public class ExtendedParticleSystem
    {
        private List<ExtendedParticle> _particles;
        private int _maxParticles;
        private int _nextParticleId;
        private bool _isPlaying;
        private bool _isPaused;
        private float _playbackTime;
        private float _playbackSpeed;
        private Vector3 _emitterPosition;
        private Vector3 _emitterVelocity;
        private Vector3 _emitterForward;
        private Vector3 _emitterUp;
        private EmissionModule _emissionModule;
        private ShapeModule _shapeModule;
        private VelocityModule _velocityModule;
        private ColorModule _colorModule;
        private SizeModule _sizeModule;
        private RotationModule _rotationModule;
        private NoiseModule _noiseModule;
        private TriggerModule _triggerModule;
        private LightsModule _lightsModule;
        private TrailsModule _trailsModule;
        private CustomDataModule _customDataModule;
        private CollisionModule2D _collision2DModule;
        private CollisionModule3D _collision3DModule;
        private SimulationModule _simulationModule;
        private RenderModule _renderModule;
        private ParticleRandom _random;
        private float _emitAccumulator;
        private int _activeParticleCount;
        private Vector3 _customSimulationSpace;
        private SimulationSpace _simulationSpace;
        private float _duration;
        private bool _loop;
        private float _startDelay;
        private float _delayTimer;
        private bool _useAutoRandomSeed;
        private int _randomSeed;

        public List<ExtendedParticle> Particles => _particles;
        public int MaxParticles => _maxParticles;
        public int ActiveParticleCount => _activeParticleCount;
        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        public float PlaybackTime => _playbackTime;
        public float PlaybackSpeed => _playbackSpeed;
        public float Duration => _duration;
        public bool Loop => _loop;

        public EmissionModule Emission => _emissionModule;
        public ShapeModule Shape => _shapeModule;
        public VelocityModule Velocity => _velocityModule;
        public ColorModule Color => _colorModule;
        public SizeModule Size => _sizeModule;
        public RotationModule Rotation => _rotationModule;
        public NoiseModule Noise => _noiseModule;
        public TriggerModule Trigger => _triggerModule;
        public LightsModule Lights => _lightsModule;
        public TrailsModule Trails => _trailsModule;
        public CustomDataModule CustomData => _customDataModule;
        public CollisionModule2D Collision2D => _collision2DModule;
        public CollisionModule3D Collision3D => _collision3DModule;
        public SimulationModule Simulation => _simulationModule;
        public RenderModule Render => _renderModule;

        public ParticleRandom Random => _random;

        public Vector3 Position
        {
            get => _emitterPosition;
            set => _emitterPosition = value;
        }

        public SimulationSpace SimulationSpace
        {
            get => _simulationSpace;
            set => _simulationSpace = value;
        }

        public Vector3 CustomSimulationSpace
        {
            get => _customSimulationSpace;
            set => _customSimulationSpace = value;
        }

        public event Action<ExtendedParticle>? ParticleEmitted;
        public event Action<ExtendedParticle>? ParticleDied;
        public event Action<ExtendedParticle>? ParticleCollided;

        public ExtendedParticleSystem(int maxParticles = 1000)
        {
            _maxParticles = maxParticles;
            _particles = new List<ExtendedParticle>(maxParticles);
            _nextParticleId = 0;
            _isPlaying = false;
            _isPaused = false;
            _playbackTime = 0f;
            _playbackSpeed = 1f;
            _emitterPosition = Vector3.Zero;
            _emitterVelocity = Vector3.Zero;
            _emitterForward = Vector3.Forward;
            _emitterUp = Vector3.Up;
            _emitAccumulator = 0f;
            _activeParticleCount = 0;
            _simulationSpace = SimulationSpace.Local;
            _duration = 5f;
            _loop = true;
            _startDelay = 0f;
            _delayTimer = 0f;
            _useAutoRandomSeed = true;
            _randomSeed = 0;

            _random = new ParticleRandom();

            _emissionModule = new EmissionModule();
            _shapeModule = new ShapeModule();
            _velocityModule = new VelocityModule();
            _colorModule = new ColorModule();
            _sizeModule = new SizeModule();
            _rotationModule = new RotationModule();
            _noiseModule = new NoiseModule();
            _triggerModule = new TriggerModule();
            _lightsModule = new LightsModule();
            _trailsModule = new TrailsModule();
            _customDataModule = new CustomDataModule();
            _collision2DModule = new CollisionModule2D();
            _collision3DModule = new CollisionModule3D();
            _simulationModule = new SimulationModule();
            _renderModule = new RenderModule();

            for (int i = 0; i < maxParticles; i++)
            {
                _particles.Add(new ExtendedParticle());
            }

            InitializeModules();
        }

        private void InitializeModules()
        {
            _emissionModule.Initialize(this);
            _shapeModule.Initialize(this);
            _velocityModule.Initialize(this);
            _colorModule.Initialize(this);
            _sizeModule.Initialize(this);
            _rotationModule.Initialize(this);
            _noiseModule.Initialize(this);
            _noiseModule.SetSystem(this);
            _triggerModule.Initialize(this);
            _lightsModule.Initialize(this);
            _trailsModule.Initialize(this);
            _customDataModule.Initialize(this);
            _collision2DModule.Initialize(this);
            _collision3DModule.Initialize(this);
            _simulationModule.Initialize(this);
        }

        public void Play()
        {
            if (_isPaused)
            {
                _isPaused = false;
            }
            else
            {
                Stop(true);
            }

            if (_useAutoRandomSeed)
            {
                _random.SetSeed(Environment.TickCount);
            }
            else
            {
                _random.SetSeed(_randomSeed);
            }

            _isPlaying = true;
            _playbackTime = 0f;
            _emitAccumulator = 0f;
            _delayTimer = 0f;

            _emissionModule.ResetBursts();
        }

        public void PlayOneShot()
        {
            Play();
            Emit((int)_emissionModule.Rate);
            if (!_loop)
            {
                Stop(false);
            }
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Stop()
        {
            Stop(true);
        }

        public void Stop(bool stopEmitting)
        {
            _isPlaying = false;
            _isPaused = false;

            if (stopEmitting)
            {
            }

            foreach (var particle in _particles)
            {
                particle.IsAlive = false;
            }

            _activeParticleCount = 0;
        }

        public void Emit(int count)
        {
            int emitted = 0;
            for (int i = 0; i < _particles.Count && emitted < count; i++)
            {
                var particle = _particles[i];
                if (!particle.IsAlive)
                {
                    InitializeParticle(particle);
                    emitted++;
                    ParticleEmitted?.Invoke(particle);
                }
            }
        }

        private void InitializeParticle(ExtendedParticle particle)
        {
            particle.Id = _nextParticleId++;
            particle.IsAlive = true;
            particle.Age = 0f;
            particle.RandomSeed = _random.NextFloat();
            particle.Seed = _random.Next();

            particle.Position = GetEmissionPosition();
            particle.PreviousPosition = particle.Position;
            particle.StartPosition = particle.Position;

            Vector3 direction = _shapeModule.GetEmissionDirection(_emitterForward, _emitterUp);
            float speed = _velocityModule.GetInitialVelocity().Length();
            particle.Velocity = direction * speed;
            particle.StartVelocity = particle.Velocity;
            particle.InheritedVelocity = _emitterVelocity * particle.VelocityInheritanceFactor;

            particle.Lifetime = _emissionModule.Rate > 0 ? 2f : 1f;
            particle.RemainingLifetime = particle.Lifetime;
            particle.StartLifetime = particle.Lifetime;
            particle.Age = 0f;

            particle.StartSize = 1f;
            particle.Size = particle.StartSize;
            particle.EndSize = 0f;

            particle.StartColor = Color.White;
            particle.Color = particle.StartColor;
            particle.EndColor = Color.Clear;

            particle.StartRotation = 0f;
            particle.Rotation = particle.StartRotation;
            particle.EndRotation = 0f;
            particle.RotationSpeed = _rotationModule.GetInitialAngularVelocity();
            particle.AngularVelocity = particle.RotationSpeed;

            particle.CollisionCount = 0;
            particle.InheritedVelocity = _emitterVelocity;
            particle.OrbitalCenter[0] = particle.Position;
            particle.OrbitalRadius = 0f;
            particle.OrbitalAngle = 0f;
            particle.OrbitalSpeed = _velocityModule.OrbitalSpeed;

            particle.TrailIndex = 0;
        }

        private Vector3 GetEmissionPosition()
        {
            Vector3 emitterPos = _emitterPosition;

            switch (_simulationSpace)
            {
                case SimulationSpace.Local:
                    break;
                case SimulationSpace.Custom:
                    emitterPos = _customSimulationSpace;
                    break;
                case SimulationSpace.World:
                    break;
            }

            return _shapeModule.GetEmissionPosition(emitterPos, _emitterForward, _emitterUp);
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying || _isPaused)
                return;

            if (_delayTimer < _startDelay)
            {
                _delayTimer += deltaTime;
                return;
            }

            float scaledDelta = deltaTime * _playbackSpeed;
            scaledDelta = Math.Min(scaledDelta, _simulationModule.DeltaTimeCap);

            _playbackTime += scaledDelta;

            if (!_loop && _playbackTime >= _duration)
            {
                Stop(false);
                return;
            }

            ProcessEmission(scaledDelta);
            UpdateParticles(scaledDelta);
            ProcessParticleDeath();
            UpdateActiveCount();
        }

        private void ProcessEmission(float deltaTime)
        {
            int particlesToEmit = 0;
            _emissionModule.UpdateEmission(deltaTime, _emissionModule.Rate, ref particlesToEmit);

            if (particlesToEmit > 0)
            {
                Emit(particlesToEmit);
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            foreach (var particle in _particles)
            {
                if (!particle.IsAlive)
                    continue;

                particle.Age += deltaTime;
                particle.RemainingLifetime -= deltaTime;

                if (particle.RemainingLifetime <= 0)
                {
                    particle.IsAlive = false;
                    ParticleDied?.Invoke(particle);
                    continue;
                }

                particle.Lifetime = particle.StartLifetime;
                particle.NormalizedAge = particle.Age / particle.Lifetime;

                _simulationModule.Update(particle, deltaTime);
                _velocityModule.Update(particle, deltaTime);
                _colorModule.Update(particle, deltaTime);
                _sizeModule.Update(particle, deltaTime);
                _rotationModule.Update(particle, deltaTime);
                _noiseModule.Update(particle, deltaTime);
                _triggerModule.Update(particle, deltaTime);
                _lightsModule.Update(particle, deltaTime);
                _trailsModule.Update(particle, deltaTime);

                if (_collision2DModule.Type != Collision2DType.None)
                {
                    _collision2DModule.Update(particle, deltaTime);
                }

                if (_collision3DModule.Type != Collision3DType.None)
                {
                    _collision3DModule.Update(particle, deltaTime);
                }

                particle.PreviousPosition = particle.Position;
                particle.Velocity += particle.Acceleration * deltaTime;
                particle.Position += particle.Velocity * deltaTime;
                particle.Acceleration = Vector3.Zero;

                if (_velocityModule.Type == VelocityType.Linear)
                {
                    particle.Position += particle.InheritedVelocity * deltaTime;
                }

                particle.UpdateTrail();

                if (_simulationModule.ShouldCull(particle, _renderModule.CameraPosition) &&
                    _simulationModule.CullingMode == CullingMode.Automatic)
                {
                    particle.IsAlive = false;
                }
            }
        }

        private void ProcessParticleDeath()
        {
            foreach (var particle in _particles)
            {
                if (!particle.IsAlive)
                    continue;

                if (particle.Age >= particle.Lifetime)
                {
                    particle.IsAlive = false;
                    ParticleDied?.Invoke(particle);
                }
            }
        }

        private void UpdateActiveCount()
        {
            _activeParticleCount = 0;
            foreach (var particle in _particles)
            {
                if (particle.IsAlive)
                    _activeParticleCount++;
            }
        }

        public void SetEmitterTransform(Vector3 position, Vector3 forward, Vector3 up)
        {
            _emitterPosition = position;
            _emitterForward = forward;
            _emitterUp = up;
        }

        public void SetEmitterVelocity(Vector3 velocity)
        {
            _emitterVelocity = velocity;
        }

        public List<ExtendedParticle> GetActiveParticles()
        {
            return _particles.Where(p => p.IsAlive).ToList();
        }

        public void Clear()
        {
            foreach (var particle in _particles)
            {
                particle.IsAlive = false;
            }
            _activeParticleCount = 0;
        }

        public void SetPlaybackSpeed(float speed)
        {
            _playbackSpeed = Math.Max(0f, speed);
        }

        public void SetMaxParticles(int max)
        {
            _maxParticles = Math.Max(1, max);
            while (_particles.Count < _maxParticles)
            {
                _particles.Add(new ExtendedParticle());
            }
        }

        public void Dispose()
        {
            foreach (var particle in _particles)
            {
                particle.Reset();
            }
            _particles.Clear();
        }
    }

    public class ParticleRandom
    {
        private System.Random _random;
        private int _seed;

        public ParticleRandom()
        {
            _random = new System.Random();
            _seed = 0;
        }

        public void SetSeed(int seed)
        {
            _seed = seed;
            _random = new System.Random(seed);
        }

        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        public float Range(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }

        public int Range(int min, int max)
        {
            return _random.Next(min, max);
        }

        public Vector3 InsideUnitSphere()
        {
            float theta = NextFloat() * MathHelper.TwoPi;
            float phi = (float)Math.Acos(2f * NextFloat() - 1f);
            float r = (float)Math.Pow(NextFloat(), 1f / 3f);
            return new Vector3(
                r * (float)Math.Sin(phi) * (float)Math.Cos(theta),
                r * (float)Math.Sin(phi) * (float)Math.Sin(theta),
                r * (float)Math.Cos(phi)
            );
        }

        public Vector3 OnUnitSphere()
        {
            return InsideUnitSphere().Normalized();
        }

        public Vector2 InsideUnitCircle()
        {
            float r = (float)Math.Sqrt(NextFloat());
            float theta = NextFloat() * MathHelper.TwoPi;
            return new Vector2(
                r * (float)Math.Cos(theta),
                r * (float)Math.Sin(theta)
            );
        }
    }

    public class ExtendedParticleEffects
    {
        public static void CreateExplosion(ExtendedParticleSystem system, Vector3 position, float intensity = 1f)
        {
            system.Position = position;
            system.Emission.Rate = 100 * (int)intensity;
            system.Velocity.LinearVelocity = new Vector3(5f * intensity, 5f * intensity, 5f * intensity);
            system.Color.ColorOverLifetime = new List<ColorKey>
            {
                new ColorKey(0f, new Color(1f, 0.8f, 0.2f, 1f)),
                new ColorKey(0.5f, new Color(1f, 0.3f, 0.1f, 1f)),
                new ColorKey(1f, new Color(0.2f, 0.1f, 0.05f, 0f))
            };
            system.Size.SizeOverLifetime = new List<SizeKey>
            {
                new SizeKey(0f, 1f),
                new SizeKey(0.5f, 0.8f),
                new SizeKey(1f, 0.2f)
            };
            system.Simulation.Gravity = new Vector3(0, -2f, 0);
            system.Play();
        }

        public static void CreateSmoke(ExtendedParticleSystem system, Vector3 position)
        {
            system.Position = position;
            system.Emission.Rate = 20;
            system.Shape.Shape = EmitterShape.Cone;
            system.Shape.ConeAngle = 15f;
            system.Velocity.LinearVelocity = new Vector3(0, 2f, 0);
            system.Color.ColorOverLifetime = new List<ColorKey>
            {
                new ColorKey(0f, new Color(0.3f, 0.3f, 0.3f, 0.1f)),
                new ColorKey(1f, new Color(0.1f, 0.1f, 0.1f, 0f))
            };
            system.Size.SizeOverLifetime = new List<SizeKey>
            {
                new SizeKey(0f, 0.5f),
                new SizeKey(1f, 2f)
            };
            system.Rotation.RotationModule.Type = RotationType.BySpeed;
            system.Rotation.AngularVelocity = 50f;
            system.Noise.Enabled = true;
            system.Noise.Strength = 0.5f;
            system.Noise.Frequency = 2f;
            system.Play();
        }

        public static void CreateFire(ExtendedParticleSystem system, Vector3 position)
        {
            system.Position = position;
            system.Emission.Rate = 50;
            system.Shape.Shape = EmitterShape.Cone;
            system.Shape.ConeAngle = 30f;
            system.Velocity.LinearVelocity = new Vector3(0, 3f, 0);
            system.Color.ColorOverLifetime = new List<ColorKey>
            {
                new ColorKey(0f, new Color(1f, 0.9f, 0.5f, 1f)),
                new ColorKey(0.3f, new Color(1f, 0.4f, 0.1f, 1f)),
                new ColorKey(0.7f, new Color(0.5f, 0.1f, 0f, 0.8f)),
                new ColorKey(1f, new Color(0.1f, 0.05f, 0f, 0f))
            };
            system.Size.SizeOverLifetime = new List<SizeKey>
            {
                new SizeKey(0f, 0.3f),
                new SizeKey(0.5f, 0.5f),
                new SizeKey(1f, 0.1f)
            };
            system.Simulation.Gravity = new Vector3(0, 1f, 0);
            system.Play();
        }

        public static void CreateSparkle(ExtendedParticleSystem system, Vector3 position)
        {
            system.Position = position;
            system.Emission.Rate = 30;
            system.Shape.Shape = EmitterShape.Sphere;
            system.Shape.SphereRadius = 0.5f;
            system.Velocity.LinearVelocity = new Vector3(2f, 2f, 2f);
            system.Color.ColorOverLifetime = new List<ColorKey>
            {
                new ColorKey(0f, Color.White),
                new ColorKey(0.5f, new Color(1f, 1f, 0.8f, 1f)),
                new ColorKey(1f, Color.Clear)
            };
            system.Size.SizeOverLifetime = new List<SizeKey>
            {
                new SizeKey(0f, 0.1f),
                new ColorKey(0.5f, 0.05f),
                new SizeKey(1f, 0f)
            };
            system.Lights.Enabled = true;
            system.Lights.Intensity = 0.5f;
            system.Lights.Range = 3f;
            system.Lights.Ratio = 0.5f;
            system.Play();
        }

        public static void CreateRain(ExtendedParticleSystem system, Vector3 position, Vector3 direction)
        {
            system.Position = position;
            system.Emission.Rate = 200;
            system.Shape.Shape = EmitterShape.Rectangle;
            system.Shape.Box = new Vector3(50f, 0f, 50f);
            system.Velocity.LinearVelocity = direction * 20f;
            system.Color.ColorOverLifetime = new List<ColorKey>
            {
                new ColorKey(0f, new Color(0.7f, 0.8f, 1f, 0.5f)),
                new ColorKey(1f, new Color(0.5f, 0.6f, 0.8f, 0f))
            };
            system.Size.SizeOverLifetime = new List<SizeKey>
            {
                new SizeKey(0f, 0.05f),
                new SizeKey(1f, 0.02f)
            };
            system.Simulation.Gravity = new Vector3(0, -9.81f, 0);
            system.Render.Mode = RenderMode.Stretch;
            system.Render.LengthScale = 0.5f;
            system.Play();
        }

        public static void CreateSnow(ExtendedParticleSystem system, Vector3 position, Vector3 direction)
        {
            system.Position = position;
            system.Emission.Rate = 100;
            system.Shape.Shape = EmitterShape.Rectangle;
            system.Shape.Box = new Vector3(100f, 0f, 100f);
            system.Velocity.LinearVelocity = direction * 2f;
            system.Color.ColorOverLifetime = new List<ColorKey>
            {
                new ColorKey(0f, Color.White),
                new ColorKey(1f, new Color(0.9f, 0.95f, 1f, 0f))
            };
            system.Size.SizeOverLifetime = new List<SizeKey>
            {
                new SizeKey(0f, 0.1f),
                new SizeKey(1f, 0.05f)
            };
            system.Noise.Enabled = true;
            system.Noise.Strength = 0.3f;
            system.Noise.Frequency = 1f;
            system.Rotation.RotationModule.Type = RotationType.BySpeed;
            system.Rotation.AngularVelocity = 30f;
            system.Play();
        }
    }
}

namespace WPFEngine.Math
{
    public static class VectorExtensions
    {
        public static Vector3 Reflect(this Vector3 vector, Vector3 normal)
        {
            float d = 2f * vector.Dot(normal);
            return vector - normal * d;
        }

        public static Vector3 Rotate(this Vector3 vector, Vector3 axis, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            float oneMinusCos = 1f - cos;

            float x = vector.X * (cos + axis.X * axis.X * oneMinusCos) +
                     vector.Y * (axis.X * axis.Y * oneMinusCos - axis.Z * sin) +
                     vector.Z * (axis.X * axis.Z * oneMinusCos + axis.Y * sin);

            float y = vector.X * (axis.Y * axis.X * oneMinusCos + axis.Z * sin) +
                     vector.Y * (cos + axis.Y * axis.Y * oneMinusCos) +
                     vector.Z * (axis.Y * axis.Z * oneMinusCos - axis.X * sin);

            float z = vector.X * (axis.Z * axis.X * oneMinusCos - axis.Y * sin) +
                     vector.Y * (axis.Z * axis.Y * oneMinusCos + axis.X * sin) +
                     vector.Z * (cos + axis.Z * axis.Z * oneMinusCos);

            return new Vector3(x, y, z);
        }
    }
}

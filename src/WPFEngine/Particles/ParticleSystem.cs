using System;
using System.Collections.Generic;
using System.Windows.Media;
using WPFEngine.ECS;
using WPFEngine.Scene;

namespace WPFEngine.Particles
{
    public enum ParticleShape
    {
        Point,
        Circle,
        Ring,
        Square,
        Line,
        Cone,
        Hemisphere
    }

    public enum ParticleBlendMode
    {
        Additive,
        Alpha,
        Multiply,
        Screen
    }

    public enum ParticleRenderMode
    {
        Billboard,
        Stretch,
        HorizontalBillboard,
        VerticalBillboard
    }

    public enum ParticleSimulationSpace
    {
        World,
        Local,
        Custom
    }

    public class ParticleSystem : Component, IDisposable
    {
        private int maxParticles;
        private int emissionRate;
        private float emissionRateOverDistance;
        private Vector2 emissionShape;
        private ParticleShape emissionShapeType;
        private Vector2 emissionAngle;
        private float emissionSpeed;
        private float emissionSpeedVariation;
        private int burstsCount;
        private float burstsInterval;
        private bool useBursts;
        private List<Burst> bursts;
        private int particlesPerSecond;
        private float particleLifetime;
        private float particleLifetimeVariation;
        private float startSize;
        private float startSizeVariation;
        private GradientBrush startColor;
        private GradientBrush overLifetimeColor;
        private GradientBrush overLifetimeSize;
        private Vector2 startVelocity;
        private Vector2 velocityOverLifetime;
        private float angularVelocity;
        private float angularVelocityVariation;
        private float angularVelocityOverLifetime;
        private float gravityModifier;
        private Vector2 gravity;
        private float noiseStrength;
        private float noiseFrequency;
        private int noiseOctaves;
        private bool useNoise;
        private float noiseScrollSpeed;
        private ParticleSimulationSpace simulationSpace;
        private Vector2 customSimulationSpace;
        private bool playOnAwake;
        private bool loop;
        private bool autoRandomSeed;
        private int randomSeed;
        private float duration;
        private float speedMultiplier;
        private float sizeMultiplier;
        private float gravityMultiplier;
        private ParticleRenderMode renderMode;
        private ParticleBlendMode blendMode;
        private float stretchBillboard;
        private Vector2 velocityScale;
        private float lengthScale;
        private Material particleMaterial;
        private ImageSource sprite;
        private bool useSprite;
        private float cullRatio;
        private float cullDelay;
        private int sortingLayer;
        private int orderInLayer;
        private float maxLights;
        private float lightsRadius;
        private Color lightsColor;
        private bool enableLights;
        private bool enableTrails;
        private TrailData trailData;
        private int activeParticleCount;
        private int totalParticleCount;
        private float emitAccumulator;
        private bool isEmitting;
        private bool isPaused;
        private bool isPlaying;
        private float playbackTime;
        private float playbackSpeed;
        private List<Particle> particles;
        private ParticleVertexBuffer vertexBuffer;
        private int[] indices;
        private float nextParticleTime;

        public event EventHandler<ParticleEventArgs> ParticleEmitted;
        public event EventHandler<ParticleEventArgs> ParticleDied;
        public event EventHandler<ParticleEventArgs> ParticleMoved;
        public event EventHandler<ParticleSystemEventArgs> SystemStarted;
        public event EventHandler<ParticleSystemEventArgs> SystemStopped;
        public event EventHandler<ParticleSystemEventArgs> SystemPaused;

        public int MaxParticles
        {
            get => maxParticles;
            set => maxParticles = Math.Max(1, value);
        }

        public int EmissionRate
        {
            get => emissionRate;
            set => emissionRate = Math.Max(0, value);
        }

        public float EmissionRateOverDistance
        {
            get => emissionRateOverDistance;
            set => emissionRateOverDistance = Math.Max(0f, value);
        }

        public Vector2 EmissionShape
        {
            get => emissionShape;
            set => emissionShape = value;
        }

        public ParticleShape EmissionShapeType
        {
            get => emissionShapeType;
            set => emissionShapeType = value;
        }

        public Vector2 EmissionAngle
        {
            get => emissionAngle;
            set => emissionAngle = value;
        }

        public float EmissionSpeed
        {
            get => emissionSpeed;
            set => emissionSpeed = Math.Max(0f, value);
        }

        public float EmissionSpeedVariation
        {
            get => emissionSpeedVariation;
            set => emissionSpeedVariation = Math.Max(0f, value);
        }

        public int BurstsCount
        {
            get => burstsCount;
            set => burstsCount = Math.Max(0, value);
        }

        public float BurstsInterval
        {
            get => burstsInterval;
            set => burstsInterval = Math.Max(0f, value);
        }

        public bool UseBursts
        {
            get => useBursts;
            set => useBursts = value;
        }

        public int ParticlesPerSecond
        {
            get => particlesPerSecond;
            set => particlesPerSecond = Math.Max(0, value);
        }

        public float ParticleLifetime
        {
            get => particleLifetime;
            set => particleLifetime = Math.Max(0f, value);
        }

        public float ParticleLifetimeVariation
        {
            get => particleLifetimeVariation;
            set => particleLifetimeVariation = Math.Max(0f, value);
        }

        public float StartSize
        {
            get => startSize;
            set => startSize = Math.Max(0f, value);
        }

        public float StartSizeVariation
        {
            get => startSizeVariation;
            set => startSizeVariation = Math.Max(0f, value);
        }

        public GradientBrush StartColor
        {
            get => startColor;
            set => startColor = value;
        }

        public GradientBrush OverLifetimeColor
        {
            get => overLifetimeColor;
            set => overLifetimeColor = value;
        }

        public GradientBrush OverLifetimeSize
        {
            get => overLifetimeSize;
            set => overLifetimeSize = value;
        }

        public Vector2 StartVelocity
        {
            get => startVelocity;
            set => startVelocity = value;
        }

        public Vector2 VelocityOverLifetime
        {
            get => velocityOverLifetime;
            set => velocityOverLifetime = value;
        }

        public float AngularVelocity
        {
            get => angularVelocity;
            set => angularVelocity = value;
        }

        public float AngularVelocityVariation
        {
            get => angularVelocityVariation;
            set => angularVelocityVariation = Math.Max(0f, value);
        }

        public float AngularVelocityOverLifetime
        {
            get => angularVelocityOverLifetime;
            set => angularVelocityOverLifetime = value;
        }

        public float GravityModifier
        {
            get => gravityModifier;
            set => gravityModifier = Math.Max(0f, value);
        }

        public Vector2 Gravity
        {
            get => gravity;
            set => gravity = value;
        }

        public float NoiseStrength
        {
            get => noiseStrength;
            set => noiseStrength = value;
        }

        public float NoiseFrequency
        {
            get => noiseFrequency;
            set => noiseFrequency = Math.Max(0.0001f, value);
        }

        public int NoiseOctaves
        {
            get => noiseOctaves;
            set => noiseOctaves = Math.Max(1, Math.Min(4, value));
        }

        public bool UseNoise
        {
            get => useNoise;
            set => useNoise = value;
        }

        public float NoiseScrollSpeed
        {
            get => noiseScrollSpeed;
            set => noiseScrollSpeed = value;
        }

        public ParticleSimulationSpace SimulationSpace
        {
            get => simulationSpace;
            set => simulationSpace = value;
        }

        public Vector2 CustomSimulationSpace
        {
            get => customSimulationSpace;
            set => customSimulationSpace = value;
        }

        public bool PlayOnAwake
        {
            get => playOnAwake;
            set => playOnAwake = value;
        }

        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        public bool AutoRandomSeed
        {
            get => autoRandomSeed;
            set => autoRandomSeed = value;
        }

        public int RandomSeed
        {
            get => randomSeed;
            set => randomSeed = value;
        }

        public float Duration => duration;

        public float SpeedMultiplier
        {
            get => speedMultiplier;
            set => speedMultiplier = Math.Max(0f, value);
        }

        public float SizeMultiplier
        {
            get => sizeMultiplier;
            set => sizeMultiplier = Math.Max(0f, value);
        }

        public float GravityMultiplier
        {
            get => gravityMultiplier;
            set => gravityMultiplier = Math.Max(0f, value);
        }

        public ParticleRenderMode RenderMode
        {
            get => renderMode;
            set => renderMode = value;
        }

        public ParticleBlendMode BlendMode
        {
            get => blendMode;
            set => blendMode = value;
        }

        public float StretchBillboard
        {
            get => stretchBillboard;
            set => stretchBillboard = Math.Max(0f, value);
        }

        public Vector2 VelocityScale
        {
            get => velocityScale;
            set => velocityScale = value;
        }

        public float LengthScale
        {
            get => lengthScale;
            set => lengthScale = Math.Max(0f, value);
        }

        public Material ParticleMaterial
        {
            get => particleMaterial;
            set => particleMaterial = value;
        }

        public ImageSource Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                useSprite = value != null;
            }
        }

        public int SortingLayer
        {
            get => sortingLayer;
            set => sortingLayer = value;
        }

        public int OrderInLayer
        {
            get => orderInLayer;
            set => orderInLayer = value;
        }

        public float MaxLights
        {
            get => maxLights;
            set => maxLights = Math.Max(0f, value);
        }

        public float LightsRadius
        {
            get => lightsRadius;
            set => lightsRadius = Math.Max(0f, value);
        }

        public Color LightsColor
        {
            get => lightsColor;
            set => lightsColor = value;
        }

        public bool EnableLights
        {
            get => enableLights;
            set => enableLights = value;
        }

        public bool EnableTrails
        {
            get => enableTrails;
            set => enableTrails = value;
        }

        public TrailData TrailData
        {
            get => trailData;
            set => trailData = value;
        }

        public int ActiveParticleCount => activeParticleCount;
        public int TotalParticleCount => totalParticleCount;
        public bool IsEmitting => isEmitting;
        public bool IsPaused => isPaused;
        public bool IsPlaying => isPlaying;
        public float PlaybackTime => playbackTime;

        public ParticleSystem()
        {
            maxParticles = 100;
            emissionRate = 10;
            emissionRateOverDistance = 0f;
            emissionShape = new Vector2(1, 1);
            emissionShapeType = ParticleShape.Point;
            emissionAngle = new Vector2(0, 360);
            emissionSpeed = 5f;
            emissionSpeedVariation = 0f;
            burstsCount = 0;
            burstsInterval = 0f;
            useBursts = false;
            bursts = new List<Burst>();
            particlesPerSecond = 10;
            particleLifetime = 2f;
            particleLifetimeVariation = 0f;
            startSize = 1f;
            startSizeVariation = 0f;
            startColor = null;
            overLifetimeColor = null;
            overLifetimeSize = null;
            startVelocity = Vector2.Zero;
            velocityOverLifetime = Vector2.Zero;
            angularVelocity = 0f;
            angularVelocityVariation = 0f;
            angularVelocityOverLifetime = 0f;
            gravityModifier = 1f;
            gravity = new Vector2(0, -9.81f);
            noiseStrength = 1f;
            noiseFrequency = 0.5f;
            noiseOctaves = 1;
            useNoise = false;
            noiseScrollSpeed = 0f;
            simulationSpace = ParticleSimulationSpace.Local;
            customSimulationSpace = Vector2.Zero;
            playOnAwake = true;
            loop = true;
            autoRandomSeed = true;
            randomSeed = 0;
            duration = 5f;
            speedMultiplier = 1f;
            sizeMultiplier = 1f;
            gravityMultiplier = 1f;
            renderMode = ParticleRenderMode.Billboard;
            blendMode = ParticleBlendMode.Alpha;
            stretchBillboard = 1f;
            velocityScale = new Vector2(1, 1);
            lengthScale = 2f;
            particleMaterial = null;
            sprite = null;
            useSprite = false;
            cullRatio = 0f;
            cullDelay = 0f;
            sortingLayer = 0;
            orderInLayer = 0;
            maxLights = 0f;
            lightsRadius = 1f;
            lightsColor = Colors.White;
            enableLights = false;
            enableTrails = false;
            trailData = new TrailData();
            activeParticleCount = 0;
            totalParticleCount = 0;
            emitAccumulator = 0f;
            isEmitting = false;
            isPaused = false;
            isPlaying = false;
            playbackTime = 0f;
            playbackSpeed = 1f;
            particles = new List<Particle>();
            nextParticleTime = 0f;

            InitializeParticles();
        }

        private void InitializeParticles()
        {
            particles.Clear();
            for (int i = 0; i < maxParticles; i++)
            {
                particles.Add(new Particle
                {
                    Index = i,
                    IsAlive = false
                });
            }
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            if (playOnAwake)
            {
                Play();
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!isPlaying || isPaused)
                return;

            var scaledDelta = deltaTime * playbackSpeed;

            playbackTime += scaledDelta;

            UpdateEmission(scaledDelta);
            UpdateParticles(scaledDelta);
            ProcessParticleDeath();
            UpdateBursts(scaledDelta);

            activeParticleCount = 0;
            foreach (var particle in particles)
            {
                if (particle.IsAlive)
                    activeParticleCount++;
            }
        }

        private void UpdateEmission(float deltaTime)
        {
            if (!isEmitting || emissionRate <= 0)
                return;

            var timeBetweenEmissions = 1f / emissionRate;
            emitAccumulator += deltaTime;

            while (emitAccumulator >= timeBetweenEmissions)
            {
                emitAccumulator -= timeBetweenEmissions;
                EmitParticle();
            }
        }

        private void UpdateBursts(float deltaTime)
        {
            if (!useBursts || bursts.Count == 0)
                return;

            foreach (var burst in bursts)
            {
                burst.Update(deltaTime);
                if (burst.ShouldEmit)
                {
                    EmitBurst(burst.Count);
                    burst.MarkEmitted();
                }
            }
        }

        private void EmitBurst(int count)
        {
            for (int i = 0; i < count; i++)
            {
                EmitParticle();
            }
        }

        public void Emit()
        {
            EmitParticle();
        }

        public void Emit(int count)
        {
            for (int i = 0; i < count; i++)
            {
                EmitParticle();
            }
        }

        private void EmitParticle()
        {
            if (!isEmitting)
                return;

            Particle particle = null;
            foreach (var p in particles)
            {
                if (!p.IsAlive)
                {
                    particle = p;
                    break;
                }
            }

            if (particle == null)
                return;

            InitializeParticle(particle);
            totalParticleCount++;
            ParticleEmitted?.Invoke(this, new ParticleEventArgs(particle));
        }

        private void InitializeParticle(Particle particle)
        {
            particle.IsAlive = true;
            particle.Age = 0f;

            var lifetime = particleLifetime + Random.Range(-particleLifetimeVariation, particleLifetimeVariation);
            particle.Lifetime = Math.Max(0.001f, lifetime);
            particle.NormalizedAge = 0f;

            particle.Position = GetEmissionPosition();
            particle.PreviousPosition = particle.Position;

            var angle = Random.Range(emissionAngle.X, emissionAngle.Y);
            var speed = emissionSpeed + Random.Range(-emissionSpeedVariation, emissionSpeedVariation);
            var radians = angle * (float)Math.PI / 180f;
            particle.Velocity = new Vector2(
                (float)Math.Cos(radians) * speed,
                (float)Math.Sin(radians) * speed
            );

            particle.Rotation = 0f;
            particle.AngularVelocity = angularVelocity + Random.Range(-angularVelocityVariation, angularVelocityVariation);

            var size = startSize + Random.Range(-startSizeVariation, startSizeVariation);
            particle.StartSize = Math.Max(0f, size);
            particle.Size = particle.StartSize;

            particle.Color = GetStartColor();
            particle.StartColor = particle.Color;

            particle.VertexIndex = particle.Index * 4;

            particle.Seed = autoRandomSeed ? Random.Range(0, int.MaxValue) : randomSeed;
        }

        private Vector2 GetEmissionPosition()
        {
            Vector2 emissionPos = Vector2.Zero;

            if (simulationSpace == ParticleSimulationSpace.Local && Transform != null)
            {
                emissionPos = Transform.WorldPosition;
            }
            else if (simulationSpace == ParticleSimulationSpace.Custom)
            {
                emissionPos = customSimulationSpace;
            }

            switch (emissionShapeType)
            {
                case ParticleShape.Point:
                    break;

                case ParticleShape.Circle:
                    var radius = emissionShape.X * 0.5f;
                    var angle = Random.Range(0f, 360f) * (float)Math.PI / 180f;
                    emissionPos += new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                    break;

                case ParticleShape.Ring:
                    var ringRadius = emissionShape.X * 0.5f;
                    var ringAngle = Random.Range(0f, 360f) * (float)Math.PI / 180f;
                    emissionPos += new Vector2((float)Math.Cos(ringAngle) * ringRadius, (float)Math.Sin(ringAngle) * ringRadius);
                    break;

                case ParticleShape.Square:
                    var halfWidth = emissionShape.X * 0.5f;
                    var halfHeight = emissionShape.Y * 0.5f;
                    emissionPos += new Vector2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight));
                    break;

                case ParticleShape.Line:
                    var lineLength = emissionShape.X;
                    var t = Random.Range(0f, 1f);
                    emissionPos += new Vector2(t * lineLength, 0);
                    break;
            }

            return emissionPos;
        }

        private Color GetStartColor()
        {
            if (startColor == null)
                return Colors.White;

            return GetGradientColor(startColor, 0f);
        }

        private Color GetGradientColor(GradientBrush gradient, float normalizedAge)
        {
            if (gradient == null || gradient.GradientStops.Count == 0)
                return Colors.White;

            var stops = gradient.GradientStops;
            if (stops.Count == 1)
                return stops[0].Color;

            var targetPos = normalizedAge * (stops.Count - 1);
            var lowerIndex = (int)Math.Floor(targetPos);
            var upperIndex = (int)Math.Ceiling(targetPos);

            if (upperIndex >= stops.Count)
                return stops[stops.Count - 1].Color;

            var t = targetPos - lowerIndex;
            var lowerColor = stops[lowerIndex].Color;
            var upperColor = stops[upperIndex].Color;

            return InterpolateColor(lowerColor, upperColor, t);
        }

        private Color InterpolateColor(Color a, Color b, float t)
        {
            var r = a.R + (b.R - a.R) * t;
            var g = a.G + (b.G - a.G) * t;
            var bl = a.B + (b.B - a.B) * t;
            var alpha = a.A + (b.A - a.A) * t;

            return Color.FromArgb((byte)alpha, (byte)r, (byte)g, (byte)bl);
        }

        private void UpdateParticles(float deltaTime)
        {
            foreach (var particle in particles)
            {
                if (!particle.IsAlive)
                    continue;

                particle.PreviousPosition = particle.Position;
                particle.Age += deltaTime;

                if (particle.Age >= particle.Lifetime)
                {
                    particle.IsAlive = false;
                    ParticleDied?.Invoke(this, new ParticleEventArgs(particle));
                    continue;
                }

                particle.NormalizedAge = particle.Age / particle.Lifetime;

                UpdateParticleVelocity(particle, deltaTime);
                UpdateParticlePosition(particle, deltaTime);
                UpdateParticleRotation(particle, deltaTime);
                UpdateParticleSize(particle);
                UpdateParticleColor(particle);

                if (useNoise)
                {
                    ApplyNoise(particle, deltaTime);
                }

                ParticleMoved?.Invoke(this, new ParticleEventArgs(particle));
            }
        }

        private void UpdateParticleVelocity(Particle particle, float deltaTime)
        {
            var velocityChange = velocityOverLifetime * deltaTime;
            particle.Velocity += velocityChange;

            if (gravityModifier > 0)
            {
                var gravityForce = gravity * gravityModifier * gravityMultiplier * deltaTime;
                particle.Velocity += gravityForce;
            }
        }

        private void UpdateParticlePosition(Particle particle, float deltaTime)
        {
            particle.Position += particle.Velocity * deltaTime * speedMultiplier;

            if (simulationSpace == ParticleSimulationSpace.Local && Transform != null)
            {
            }
        }

        private void UpdateParticleRotation(Particle particle, float deltaTime)
        {
            particle.Rotation += particle.AngularVelocity * deltaTime;
        }

        private void UpdateParticleSize(Particle particle)
        {
            if (overLifetimeSize != null)
            {
                var sizeRatio = GetGradientValue(overLifetimeSize, particle.NormalizedAge);
                particle.Size = particle.StartSize * sizeRatio * sizeMultiplier;
            }
            else
            {
                particle.Size = particle.StartSize * sizeMultiplier;
            }
        }

        private void UpdateParticleColor(Particle particle)
        {
            if (overLifetimeColor != null)
            {
                particle.Color = GetGradientColor(overLifetimeColor, particle.NormalizedAge);
            }
        }

        private float GetGradientValue(GradientBrush gradient, float normalizedAge)
        {
            if (gradient == null || gradient.GradientStops.Count == 0)
                return 1f;

            return 1f;
        }

        private void ApplyNoise(Particle particle, float deltaTime)
        {
        }

        private void ProcessParticleDeath()
        {
        }

        public void Play()
        {
            if (isPaused)
            {
                isPaused = false;
            }
            else
            {
                Stop(true);
            }

            isPlaying = true;
            isEmitting = true;
            playbackTime = 0f;
            emitAccumulator = 0f;

            foreach (var burst in bursts)
            {
                burst.Reset();
            }

            SystemStarted?.Invoke(this, new ParticleSystemEventArgs(this));
        }

        public void PlayOneShot()
        {
            Play();
            Emit();
            Stop(false);
        }

        public void Pause()
        {
            isPaused = true;
            SystemPaused?.Invoke(this, new ParticleSystemEventArgs(this));
        }

        public void Stop()
        {
            Stop(true);
        }

        public void Stop(bool stopEmitting)
        {
            isPlaying = false;
            isPaused = false;

            if (stopEmitting)
            {
                isEmitting = false;
            }

            foreach (var particle in particles)
            {
                particle.IsAlive = false;
            }

            SystemStopped?.Invoke(this, new ParticleSystemEventArgs(this));
        }

        public void Clear()
        {
            foreach (var particle in particles)
            {
                particle.IsAlive = false;
            }
            activeParticleCount = 0;
        }

        public void SetPlaybackSpeed(float speed)
        {
            playbackSpeed = Math.Max(0f, speed);
        }

        public void SetEmissionRate(int rate)
        {
            emissionRate = Math.Max(0, rate);
        }

        public void SetStartColor(Color color)
        {
            startColor = new LinearGradientBrush(color, color);
        }

        public void AddBurst(Burst burst)
        {
            bursts.Add(burst);
        }

        public void RemoveBurst(Burst burst)
        {
            bursts.Remove(burst);
        }

        public void ClearBursts()
        {
            bursts.Clear();
        }

        public Particle[] GetParticles()
        {
            var activeParticles = new List<Particle>();
            foreach (var particle in particles)
            {
                if (particle.IsAlive)
                {
                    activeParticles.Add(particle);
                }
            }
            return activeParticles.ToArray();
        }

        public void SetParticles(Particle[] newParticles)
        {
            for (int i = 0; i < Math.Min(newParticles.Length, particles.Count); i++)
            {
                particles[i] = newParticles[i];
            }
        }

        public override string ToString()
        {
            return $"ParticleSystem(Active={activeParticleCount}/{maxParticles}, Emission={emissionRate}/s)";
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
                foreach (var particle in particles)
                {
                    particle.Dispose();
                }
                particles.Clear();
                bursts.Clear();
                ParticleEmitted = null;
                ParticleDied = null;
                ParticleMoved = null;
                SystemStarted = null;
                SystemStopped = null;
                SystemPaused = null;
            }
        }
    }

    public class Particle : IDisposable
    {
        public int Index { get; set; }
        public bool IsAlive { get; set; }
        public float Age { get; set; }
        public float Lifetime { get; set; }
        public float NormalizedAge { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public float StartSize { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }
        public Color StartColor { get; set; }
        public int VertexIndex { get; set; }
        public int Seed { get; set; }
        public Vector2[] TrailPositions { get; set; }
        public int TrailIndex { get; set; }
        public float TrailLifetime { get; set; }

        public Particle()
        {
            TrailPositions = new Vector2[10];
            TrailIndex = 0;
        }

        public void Dispose()
        {
            TrailPositions = null;
        }
    }

    public class Burst
    {
        public float Time { get; set; }
        public int Count { get; set; }
        public float Probability { get; set; }
        public bool HasEmitted { get; private set; }
        private float currentTime;

        public Burst()
        {
            Time = 0f;
            Count = 10;
            Probability = 1f;
            HasEmitted = false;
            currentTime = 0f;
        }

        public Burst(float time, int count, float probability = 1f)
        {
            Time = time;
            Count = count;
            Probability = probability;
            HasEmitted = false;
            currentTime = 0f;
        }

        public bool ShouldEmit => !HasEmitted && currentTime >= Time && Random.value < Probability;

        public void Update(float deltaTime)
        {
            currentTime += deltaTime;
        }

        public void Reset()
        {
            currentTime = 0f;
            HasEmitted = false;
        }

        public void MarkEmitted()
        {
            HasEmitted = true;
        }
    }

    public class TrailData
    {
        public bool EnableTrails { get; set; }
        public float Lifetime { get; set; }
        public float MinimumVertexDistance { get; set; }
        public int MaximumTrailPoints { get; set; }
        public GradientBrush ColorOverLifetime { get; set; }
        public GradientBrush WidthOverTrail { get; set; }
        public Vector2 SizeOverTrail { get; set; }
        public bool InheritParticleColor { get; set; }
        public TrailMaterialSource MaterialSource { get; set; }
        public Material TrailMaterial { get; set; }
        public TrailRatioMode RatioMode { get; set; }
        public int Ratio { get; set; }
        public TrailDieMode DieMode { get; set; }

        public TrailData()
        {
            EnableTrails = false;
            Lifetime = 1f;
            MinimumVertexDistance = 0.1f;
            MaximumTrailPoints = 100;
            InheritParticleColor = true;
            MaterialSource = TrailMaterialSource.ParticleMaterial;
            RatioMode = TrailRatioMode.Distance;
            Ratio = 100;
            DieMode = TrailDieMode.Fade;
        }
    }

    public enum TrailMaterialSource
    {
        ParticleMaterial,
        TrailMaterial
    }

    public enum TrailRatioMode
    {
        Distance,
        Time,
        Life
    }

    public enum TrailDieMode
    {
        Fade,
        Follow,
        Persist
    }

    public class ParticleVertexBuffer
    {
        public Vector2[] Positions { get; set; }
        public Color[] Colors { get; set; }
        public Vector2[] UVs { get; set; }
        public float[] Sizes { get; set; }
        public float[] Rotations { get; set; }
        public int VertexCount { get; set; }

        public ParticleVertexBuffer(int maxParticles)
        {
            var totalVertices = maxParticles * 4;
            Positions = new Vector2[totalVertices];
            Colors = new Color[totalVertices];
            UVs = new Vector2[totalVertices];
            Sizes = new float[totalVertices];
            Rotations = new float[totalVertices];
            VertexCount = 0;
        }

        public void UpdateVertex(int particleIndex, Particle particle)
        {
            var baseIndex = particleIndex * 4;
            var halfSize = particle.Size * 0.5f;

            Positions[baseIndex] = new Vector2(-halfSize, -halfSize);
            Positions[baseIndex + 1] = new Vector2(halfSize, -halfSize);
            Positions[baseIndex + 2] = new Vector2(halfSize, halfSize);
            Positions[baseIndex + 3] = new Vector2(-halfSize, halfSize);

            Colors[baseIndex] = particle.Color;
            Colors[baseIndex + 1] = particle.Color;
            Colors[baseIndex + 2] = particle.Color;
            Colors[baseIndex + 3] = particle.Color;

            UVs[baseIndex] = new Vector2(0, 0);
            UVs[baseIndex + 1] = new Vector2(1, 0);
            UVs[baseIndex + 2] = new Vector2(1, 1);
            UVs[baseIndex + 3] = new Vector2(0, 1);

            Sizes[baseIndex] = particle.Size;
            Sizes[baseIndex + 1] = particle.Size;
            Sizes[baseIndex + 2] = particle.Size;
            Sizes[baseIndex + 3] = particle.Size;

            Rotations[baseIndex] = particle.Rotation;
            Rotations[baseIndex + 1] = particle.Rotation;
            Rotations[baseIndex + 2] = particle.Rotation;
            Rotations[baseIndex + 3] = particle.Rotation;
        }
    }

    public class ParticleEventArgs : EventArgs
    {
        public Particle Particle { get; }

        public ParticleEventArgs(Particle particle)
        {
            Particle = particle;
        }
    }

    public class ParticleSystemEventArgs : EventArgs
    {
        public ParticleSystem System { get; }

        public ParticleSystemEventArgs(ParticleSystem system)
        {
            System = system;
        }
    }

    public static class Random
    {
        private static System.Random random = new System.Random();
        private static int seed;
        private static System.Random seededRandom;

        public static float value => (float)random.NextDouble();
        public static float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);
        public static int Range(int min, int max) => random.Next(min, max);

        public static void SetSeed(int newSeed)
        {
            seed = newSeed;
            seededRandom = new System.Random(seed);
        }

        public static float SeededValue => (float)seededRandom.NextDouble();
        public static float SeededRange(float min, float max) => min + (float)seededRandom.NextDouble() * (max - min);
    }

    public class GradientBrush
    {
        public GradientStopCollection GradientStops { get; set; }
        public float Alpha { get; set; }

        public GradientBrush()
        {
            GradientStops = new GradientStopCollection();
            Alpha = 1f;
        }

        public GradientBrush(Color color)
        {
            GradientStops = new GradientStopCollection { new GradientStop(color, 0) };
            Alpha = 1f;
        }

        public GradientBrush(Color start, Color end)
        {
            GradientStops = new GradientStopCollection
            {
                new GradientStop(start, 0),
                new GradientStop(end, 1)
            };
            Alpha = 1f;
        }
    }

    public class LinearGradientBrush : GradientBrush
    {
        public LinearGradientBrush(Color start, Color end) : base(start, end)
        {
        }
    }

    public class RadialGradientBrush : GradientBrush
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }

        public RadialGradientBrush(Color start, Color end) : base(start, end)
        {
            Center = new Vector2(0.5f, 0.5f);
            Radius = 0.5f;
        }
    }

    public class GradientStop
    {
        public Color Color { get; set; }
        public float Offset { get; set; }

        public GradientStop()
        {
            Color = Colors.White;
            Offset = 0f;
        }

        public GradientStop(Color color, float offset)
        {
            Color = color;
            Offset = offset;
        }
    }

    public class GradientStopCollection : List<GradientStop>
    {
    }
}

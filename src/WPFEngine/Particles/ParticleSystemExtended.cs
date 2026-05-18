using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Particles
{
    public class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Life { get; set; }
        public float MaxLife { get; set; }
        public float Size { get; set; }
        public float StartSize { get; set; }
        public float EndSize { get; set; }
        public Color StartColor { get; set; }
        public Color EndColor { get; set; }
        public float Rotation { get; set; }
        public float RotationSpeed { get; set; }
        public float AngularVelocity { get; set; }
        public float Mass { get; set; }
        public float Damping { get; set; }
        public bool IsActive { get; set; }
        public float NormalizedLife => MaxLife > 0 ? Math.Clamp(Life / MaxLife, 0f, 1f) : 0f;
        public float RemainingLife => Life;
        public float Lifetime => MaxLife;

        public Particle()
        {
            Reset();
        }

        public void Reset()
        {
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            Life = 0;
            MaxLife = 1;
            Size = 1;
            StartSize = 1;
            EndSize = 0;
            StartColor = Color.White;
            EndColor = Color.Clear;
            Rotation = 0;
            RotationSpeed = 0;
            AngularVelocity = 0;
            Mass = 1;
            Damping = 1;
            IsActive = false;
        }

        public void Initialize(Vector2 position, Vector2 velocity, float life, float size)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = Vector2.Zero;
            Life = life;
            MaxLife = life;
            StartSize = size;
            EndSize = 0;
            Size = size;
            StartColor = Color.White;
            EndColor = Color.Clear;
            Rotation = 0;
            RotationSpeed = 0;
            AngularVelocity = 0;
            Mass = 1;
            Damping = 1;
            IsActive = true;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive)
                return;

            Life -= deltaTime;
            if (Life <= 0)
            {
                IsActive = false;
                return;
            }

            Velocity += Acceleration * deltaTime;
            Velocity *= (float)Math.Pow(Damping, deltaTime);
            Position += Velocity * deltaTime;
            Rotation += AngularVelocity * deltaTime;

            float t = NormalizedLife;
            Size = MathHelper.Lerp(StartSize, EndSize, t);
        }

        public Color GetColor()
        {
            return StartColor.Lerp(EndColor, NormalizedLife);
        }

        public Particle Clone()
        {
            return new Particle
            {
                Position = Position,
                Velocity = Velocity,
                Acceleration = Acceleration,
                Life = Life,
                MaxLife = MaxLife,
                Size = Size,
                StartSize = StartSize,
                EndSize = EndSize,
                StartColor = StartColor,
                EndColor = EndColor,
                Rotation = Rotation,
                RotationSpeed = RotationSpeed,
                AngularVelocity = AngularVelocity,
                Mass = Mass,
                Damping = Damping,
                IsActive = IsActive
            };
        }
    }

    public class ParticleEmitter
    {
        private readonly List<Particle> _particles;
        private readonly ObjectPool<Particle> _particlePool;
        private readonly int _maxParticles;

        public string Name { get; set; } = "Emitter";
        public Vector2 Position { get; set; }
        public Vector2 PositionVariance { get; set; } = Vector2.Zero;
        public Vector2 Direction { get; set; } = Vector2.Up;
        public float DirectionVariance { get; set; } = 0;
        public float Speed { get; set; } = 5;
        public float SpeedVariance { get; set; } = 0;
        public float Life { get; set; } = 1;
        public float LifeVariance { get; set; } = 0;
        public float Size { get; set; } = 1;
        public float SizeVariance { get; set; } = 0;
        public float StartSize { get; set; } = 1;
        public float StartSizeVariance { get; set; } = 0;
        public float EndSize { get; set; } = 0;
        public float EndSizeVariance { get; set; } = 0;
        public Color StartColor { get; set; } = Color.White;
        public Color StartColorVariance { get; set; } = Color.Clear;
        public Color EndColor { get; set; } = Color.Clear;
        public Color EndColorVariance { get; set; } = Color.Clear;
        public float Rotation { get; set; } = 0;
        public float RotationVariance { get; set; } = 0;
        public float RotationSpeed { get; set; } = 0;
        public float RotationSpeedVariance { get; set; } = 0;
        public float EmissionRate { get; set; } = 10;
        public float EmissionRateVariance { get; set; } = 0;
        public int BurstCount { get; set; } = 0;
        public bool IsPlaying { get; set; } = false;
        public bool IsLooping { get; set; } = true;
        public bool PlayOnAwake { get; set; } = true;
        public float GravityModifier { get; set; } = 1;
        public Vector2 Gravity { get; set; } = new Vector2(0, -9.81f);
        public float Damping { get; set; } = 1;
        public float Duration { get; set; } = -1;
        public float ElapsedTime { get; private set; }

        public int ActiveParticleCount => _particles.Count(p => p.IsActive);
        public int MaxParticles => _maxParticles;
        public IReadOnlyList<Particle> Particles => _particles.AsReadOnly();

        public event EventHandler<Particle>? ParticleCreated;
        public event EventHandler<Particle>? ParticleDied;
        public event EventHandler? EmissionFinished;

        public ParticleEmitter(int maxParticles = 1000)
        {
            _maxParticles = maxParticles;
            _particles = new List<Particle>(maxParticles);
            _particlePool = new ObjectPool<Particle>(maxParticles);

            for (int i = 0; i < maxParticles; i++)
            {
                _particles.Add(new Particle());
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                IsPlaying = true;
                ElapsedTime = 0;

                if (PlayOnAwake && BurstCount <= 0)
                {
                    Emit((int)EmissionRate);
                }
            }
        }

        public void Stop()
        {
            IsPlaying = false;
            foreach (var particle in _particles)
            {
                particle.IsActive = false;
            }
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Resume()
        {
            IsPlaying = true;
        }

        public void Emit(int count)
        {
            int emitted = 0;
            for (int i = 0; i < _particles.Count && emitted < count; i++)
            {
                var particle = _particles[i];
                if (!particle.IsActive)
                {
                    InitializeParticle(particle);
                    emitted++;
                    ParticleCreated?.Invoke(this, particle);
                }
            }
        }

        private void InitializeParticle(Particle particle)
        {
            float variance = RandomRange(-PositionVariance.X, PositionVariance.X);
            float varianceY = RandomRange(-PositionVariance.Y, PositionVariance.Y);
            particle.Position = new Vector2(Position.X + variance, Position.Y + varianceY);

            float angle = MathHelper.Atan2(Direction.Y, Direction.X);
            float angleVariance = RandomRange(-DirectionVariance, DirectionVariance);
            angle += angleVariance;
            float speed = Speed + RandomRange(-SpeedVariance, SpeedVariance);
            particle.Velocity = new Vector2(MathHelper.Cos(angle), MathHelper.Sin(angle)) * speed;

            particle.Life = Math.Max(0.001f, Life + RandomRange(-LifeVariance, LifeVariance));
            particle.MaxLife = particle.Life;

            particle.StartSize = Math.Max(0, Size + RandomRange(-SizeVariance, SizeVariance));
            particle.EndSize = Math.Max(0, EndSize + RandomRange(-EndSizeVariance, EndSizeVariance));
            particle.Size = particle.StartSize;

            particle.StartColor = AddColorVariance(StartColor, StartColorVariance);
            particle.EndColor = AddColorVariance(EndColor, EndColorVariance);

            particle.Rotation = Rotation + RandomRange(-RotationVariance, RotationVariance);
            particle.RotationSpeed = RotationSpeed + RandomRange(-RotationSpeedVariance, RotationSpeedVariance);
            particle.AngularVelocity = particle.RotationSpeed;

            particle.Acceleration = Gravity * GravityModifier;
            particle.Damping = Damping;
            particle.Mass = 1;

            particle.IsActive = true;
        }

        private Color AddColorVariance(Color baseColor, Color variance)
        {
            float r = Math.Clamp(baseColor.R + RandomRange(-variance.R, variance.R), 0, 1);
            float g = Math.Clamp(baseColor.G + RandomRange(-variance.G, variance.G), 0, 1);
            float b = Math.Clamp(baseColor.B + RandomRange(-variance.B, variance.B), 0, 1);
            float a = Math.Clamp(baseColor.A + RandomRange(-variance.A, variance.A), 0, 1);
            return new Color(r, g, b, a);
        }

        private float RandomRange(float min, float max)
        {
            return min + (float)new Random().NextDouble() * (max - min);
        }

        public void Update(float deltaTime)
        {
            if (!IsPlaying)
                return;

            ElapsedTime += deltaTime;

            if (Duration > 0 && ElapsedTime >= Duration)
            {
                IsPlaying = false;
                EmissionFinished?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (IsLooping || Duration < 0)
            {
                float emissionCount = EmissionRate + RandomRange(-EmissionRateVariance, EmissionRateVariance);
                float particlesToEmit = emissionCount * deltaTime;

                if (particlesToEmit >= 1)
                {
                    Emit((int)particlesToEmit);
                }
                else if (new Random().NextDouble() < particlesToEmit)
                {
                    Emit(1);
                }
            }

            for (int i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];
                if (particle.IsActive)
                {
                    particle.Update(deltaTime);

                    if (!particle.IsActive)
                    {
                        ParticleDied?.Invoke(this, particle);
                    }
                }
            }

            if (BurstCount > 0 && ElapsedTime <= deltaTime * 2)
            {
                Emit(BurstCount);
                BurstCount = 0;
            }
        }

        public void Clear()
        {
            foreach (var particle in _particles)
            {
                particle.IsActive = false;
            }
        }

        public List<Particle> GetActiveParticles()
        {
            return _particles.Where(p => p.IsActive).ToList();
        }
    }

    public class ParticleSystem
    {
        private readonly List<ParticleEmitter> _emitters;
        private readonly ObjectPool<Particle> _particlePool;
        private readonly int _maxParticles;
        private bool _isPlaying;
        private float _timeScale;
        private Vector2 _gravity;

        public string Name { get; set; } = "Particle System";
        public Vector2 Position { get; set; }
        public bool IsPlaying => _isPlaying;
        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Math.Clamp(value, 0f, 10f);
        }
        public Vector2 Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }
        public int MaxParticles => _maxParticles;
        public int TotalActiveParticles => _emitters.Sum(e => e.ActiveParticleCount);

        public event EventHandler<ParticleEmitter>? EmitterAdded;
        public event EventHandler<ParticleEmitter>? EmitterRemoved;

        public ParticleSystem(int maxParticles = 1000)
        {
            _maxParticles = maxParticles;
            _emitters = new List<ParticleEmitter>();
            _particlePool = new ObjectPool<Particle>(maxParticles);
            _isPlaying = false;
            _timeScale = 1f;
            _gravity = new Vector2(0, -9.81f);
        }

        public ParticleEmitter CreateEmitter(string name = "Emitter", int maxParticles = 100)
        {
            var emitter = new ParticleEmitter(maxParticles);
            emitter.Name = name;
            emitter.Gravity = _gravity;
            _emitters.Add(emitter);
            EmitterAdded?.Invoke(this, emitter);
            return emitter;
        }

        public void RemoveEmitter(ParticleEmitter emitter)
        {
            if (_emitters.Remove(emitter))
            {
                EmitterRemoved?.Invoke(this, emitter);
            }
        }

        public ParticleEmitter? GetEmitter(string name)
        {
            return _emitters.FirstOrDefault(e => e.Name == name);
        }

        public List<ParticleEmitter> GetAllEmitters()
        {
            return new List<ParticleEmitter>(_emitters);
        }

        public void Play()
        {
            _isPlaying = true;
            foreach (var emitter in _emitters)
            {
                emitter.Position += Position;
                emitter.Play();
            }
        }

        public void Stop()
        {
            _isPlaying = false;
            foreach (var emitter in _emitters)
            {
                emitter.Stop();
            }
        }

        public void Pause()
        {
            _isPlaying = false;
            foreach (var emitter in _emitters)
            {
                emitter.Pause();
            }
        }

        public void Resume()
        {
            _isPlaying = true;
            foreach (var emitter in _emitters)
            {
                emitter.Resume();
            }
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying)
                return;

            float scaledDelta = deltaTime * _timeScale;

            foreach (var emitter in _emitters)
            {
                emitter.Gravity = _gravity;
                emitter.Update(scaledDelta);
            }
        }

        public List<Particle> GetAllActiveParticles()
        {
            return _emitters.SelectMany(e => e.GetActiveParticles()).ToList();
        }

        public void Clear()
        {
            foreach (var emitter in _emitters)
            {
                emitter.Clear();
            }
        }

        public void SetPosition(Vector2 position)
        {
            foreach (var emitter in _emitters)
            {
                emitter.Position = position;
            }
        }

        public void SetGravity(Vector2 gravity)
        {
            _gravity = gravity;
            foreach (var emitter in _emitters)
            {
                emitter.Gravity = gravity;
            }
        }
    }

    public class TrailRenderer
    {
        private readonly RingBuffer<Vector2> _positions;
        private readonly RingBuffer<Color> _colors;
        private readonly RingBuffer<float> _widths;
        private readonly List<float> _times;
        private int _maxTrailLength;
        private float _time;
        private float _minVertexDistance;
        private Vector2 _lastPosition;
        private bool _isEmitting;
        private Color _startColor;
        private Color _endColor;
        private float _startWidth;
        private float _endWidth;
        private float _fadeTime;

        public int MaxTrailLength
        {
            get => _maxTrailLength;
            set
            {
                _maxTrailLength = Math.Max(2, value);
            }
        }

        public Color StartColor
        {
            get => _startColor;
            set => _startColor = value;
        }

        public Color EndColor
        {
            get => _endColor;
            set => _endColor = value;
        }

        public float StartWidth
        {
            get => _startWidth;
            set => _startWidth = value;
        }

        public float EndWidth
        {
            get => _endWidth;
            set => _endWidth = value;
        }

        public float FadeTime
        {
            get => _fadeTime;
            set => _fadeTime = Math.Max(0, value);
        }

        public float MinVertexDistance
        {
            get => _minVertexDistance;
            set => _minVertexDistance = value;
        }

        public int CurrentTrailLength => _positions.Count;
        public IReadOnlyList<Vector2> Positions => _positions.GetEnumerable().ToList();

        public TrailRenderer(int maxTrailLength = 50)
        {
            _maxTrailLength = maxTrailLength;
            _positions = new RingBuffer<Vector2>(maxTrailLength);
            _colors = new RingBuffer<Color>(maxTrailLength);
            _widths = new RingBuffer<float>(maxTrailLength);
            _times = new List<float>(maxTrailLength);
            _lastPosition = Vector2.Zero;
            _isEmitting = false;
            _startColor = Color.White;
            _endColor = Color.Clear;
            _startWidth = 1;
            _endWidth = 0;
            _fadeTime = 0.5f;
            _minVertexDistance = 0.1f;
            _time = 0;
        }

        public void StartEmitting(Vector2 position)
        {
            _isEmitting = true;
            _lastPosition = position;
            Clear();
            AddPoint(position);
        }

        public void StopEmitting()
        {
            _isEmitting = false;
        }

        public void Update(Vector2 position, float deltaTime)
        {
            _time += deltaTime;

            if (!_isEmitting)
                return;

            float distance = (position - _lastPosition).Length();
            if (distance >= _minVertexDistance)
            {
                AddPoint(position);
                _lastPosition = position;
            }
        }

        private void AddPoint(Vector2 position)
        {
            _positions.Enqueue(position);

            float t = _positions.Count / (float)_maxTrailLength;
            Color color = _startColor.Lerp(_endColor, t);
            float width = MathHelper.Lerp(_startWidth, _endWidth, t);

            _colors.Enqueue(color);
            _widths.Enqueue(width);
            _times.Add(_time);
        }

        public void Clear()
        {
            _positions.Clear();
            _colors.Clear();
            _widths.Clear();
            _times.Clear();
            _time = 0;
        }

        public (Vector2[] positions, Color[] colors, float[] widths) GetTrailData()
        {
            var positions = _positions.ToArray();
            var colors = new Color[_colors.Count];
            var widths = new float[_widths.Count];

            int i = 0;
            foreach (var color in _colors.GetEnumerable())
            {
                float t = i / (float)Math.Max(1, _colors.Count - 1);
                float fade = 1f;
                if (_fadeTime > 0 && i < _times.Count)
                {
                    float timeSincePoint = _time - _times[i];
                    fade = Math.Max(0, 1 - timeSincePoint / _fadeTime);
                }
                colors[i] = color * fade;
                i++;
            }

            i = 0;
            foreach (var width in _widths.GetEnumerable())
            {
                widths[i] = width;
                i++;
            }

            return (positions, colors, widths);
        }
    }

    public class LightningBolt
    {
        private readonly List<Vector2> _points;
        private readonly List<Vector2> _directions;
        private Vector2 _start;
        private Vector2 _end;
        private float _jaggedness;
        private int _segments;
        private float _width;
        private Color _color;
        private float _life;
        private float _maxLife;
        private bool _isActive;

        public Vector2 Start => _start;
        public Vector2 End => _end;
        public float Jaggedness
        {
            get => _jaggedness;
            set => _jaggedness = Math.Clamp(value, 0, 1);
        }
        public int Segments
        {
            get => _segments;
            set => _segments = Math.Max(3, value);
        }
        public float Width
        {
            get => _width;
            set => _width = Math.Max(0.1f, value);
        }
        public Color Color
        {
            get => _color;
            set => _color = value;
        }
        public float Life => _life;
        public float MaxLife => _maxLife;
        public float NormalizedLife => _maxLife > 0 ? _life / _maxLife : 0;
        public bool IsActive => _isActive;

        public LightningBolt()
        {
            _points = new List<Vector2>();
            _directions = new List<Vector2>();
            _jaggedness = 0.5f;
            _segments = 10;
            _width = 2;
            _color = Color.White;
            _life = 0;
            _maxLife = 0.5f;
            _isActive = false;
        }

        public void Initialize(Vector2 start, Vector2 end, float life = 0.5f)
        {
            _start = start;
            _end = end;
            _maxLife = life;
            _life = life;
            _isActive = true;

            GeneratePoints();
        }

        private void GeneratePoints()
        {
            _points.Clear();
            _directions.Clear();

            Vector2 direction = (_end - _start).Normalized();
            float length = (_end - _start).Length();
            float segmentLength = length / _segments;

            _points.Add(_start);
            _directions.Add(direction);

            Vector2 current = _start;
            Vector2 currentDir = direction;

            var random = new Random();

            for (int i = 1; i < _segments; i++)
            {
                float offsetAmount = segmentLength * _jaggedness;
                float perpX = -currentDir.Y;
                float perpY = currentDir.X;

                float offset = ((float)random.NextDouble() * 2 - 1) * offsetAmount;
                current += currentDir * segmentLength + new Vector2(perpX, perpY) * offset;

                _points.Add(current);

                if (i < _segments)
                {
                    currentDir = (i < _segments - 1 ? (_end - current).Normalized() : direction);
                    _directions.Add(currentDir);
                }
            }

            _points.Add(_end);
        }

        public void Update(float deltaTime)
        {
            if (!_isActive)
                return;

            _life -= deltaTime;
            if (_life <= 0)
            {
                _isActive = false;
                return;
            }

            if (_jaggedness > 0)
            {
                var random = new Random();
                float flicker = (float)Math.Sin(_life * 50) * 0.1f * _jaggedness;

                for (int i = 1; i < _points.Count - 1; i++)
                {
                    if (i < _directions.Count)
                    {
                        float offset = ((float)random.NextDouble() * 2 - 1) * 0.1f * _jaggedness * NormalizedLife;
                        Vector2 perp = new Vector2(-_directions[i].Y, _directions[i].X);
                        _points[i] += perp * offset;
                    }
                }
            }
        }

        public List<Vector2> GetPoints()
        {
            return new List<Vector2>(_points);
        }

        public Color GetCurrentColor()
        {
            return _color.WithAlpha(NormalizedLife);
        }

        public float GetCurrentWidth()
        {
            return _width * NormalizedLife;
        }
    }

    public class LightningGenerator
    {
        private readonly List<LightningBolt> _bolts;
        private readonly List<Vector2> _generationPoints;
        private int _maxBolts;
        private float _generationRate;
        private float _timeSinceLastGeneration;
        private Vector2 _currentStart;
        private Vector2 _currentEnd;
        private Random _random;

        public int MaxBolts
        {
            get => _maxBolts;
            set
            {
                _maxBolts = Math.Max(1, value);
                while (_bolts.Count > _maxBolts)
                {
                    _bolts.RemoveAt(0);
                }
            }
        }

        public float GenerationRate
        {
            get => _generationRate;
            set => _generationRate = Math.Max(0, value);
        }

        public LightningGenerator(int maxBolts = 10)
        {
            _maxBolts = maxBolts;
            _bolts = new List<LightningBolt>(maxBolts);
            _generationPoints = new List<Vector2>();
            _generationRate = 5;
            _timeSinceLastGeneration = 0;
            _random = new Random();
        }

        public void SetEndpoints(Vector2 start, Vector2 end)
        {
            _currentStart = start;
            _currentEnd = end;
        }

        public void AddGenerationPoint(Vector2 point)
        {
            _generationPoints.Add(point);
        }

        public void ClearGenerationPoints()
        {
            _generationPoints.Clear();
        }

        public void GenerateLightning(float jaggedness = 0.5f, float width = 2, float life = 0.5f)
        {
            var bolt = new LightningBolt();
            bolt.Initialize(_currentStart, _currentEnd, life);
            bolt.Jaggedness = jaggedness;
            bolt.Width = width;
            bolt.Color = Color.Cyan;

            _bolts.Add(bolt);

            if (_bolts.Count > _maxBolts)
            {
                _bolts.RemoveAt(0);
            }
        }

        public void Update(float deltaTime)
        {
            _timeSinceLastGeneration += deltaTime;

            if (_generationRate > 0 && _timeSinceLastGeneration >= 1f / _generationRate)
            {
                GenerateLightning();
                _timeSinceLastGeneration = 0;
            }

            for (int i = _bolts.Count - 1; i >= 0; i--)
            {
                _bolts[i].Update(deltaTime);
                if (!_bolts[i].IsActive)
                {
                    _bolts.RemoveAt(i);
                }
            }
        }

        public List<LightningBolt> GetActiveBolts()
        {
            return new List<LightningBolt>(_bolts);
        }

        public void Clear()
        {
            _bolts.Clear();
        }
    }

    public class Trail
    {
        private readonly RingBuffer<TrailPoint> _points;
        private int _maxPoints;
        private Vector2 _lastPosition;
        private float _minDistance;
        private float _width;
        private Color _startColor;
        private Color _endColor;
        private bool _isEmitting;
        private float _fadeSpeed;

        public int MaxPoints
        {
            get => _maxPoints;
            set
            {
                _maxPoints = Math.Max(2, value);
            }
        }

        public float Width
        {
            get => _width;
            set => _width = Math.Max(0.1f, value);
        }

        public Color StartColor
        {
            get => _startColor;
            set => _startColor = value;
        }

        public Color EndColor
        {
            get => _endColor;
            set => _endColor = value;
        }

        public float FadeSpeed
        {
            get => _fadeSpeed;
            set => _fadeSpeed = Math.Max(0, value);
        }

        public int CurrentPointCount => _points.Count;

        public Trail(int maxPoints = 100)
        {
            _maxPoints = maxPoints;
            _points = new RingBuffer<TrailPoint>(maxPoints);
            _lastPosition = Vector2.Zero;
            _minDistance = 0.1f;
            _width = 1;
            _startColor = Color.White;
            _endColor = Color.Clear;
            _isEmitting = false;
            _fadeSpeed = 1;
        }

        public void Start(Vector2 position)
        {
            _isEmitting = true;
            _lastPosition = position;
            Clear();
            AddPoint(position);
        }

        public void Stop()
        {
            _isEmitting = false;
        }

        public void Update(Vector2 position, float deltaTime)
        {
            if (!_isEmitting)
                return;

            float distance = (position - _lastPosition).Length();
            if (distance >= _minDistance)
            {
                AddPoint(position);
                _lastPosition = position;
            }

            foreach (var point in _points.GetEnumerable())
            {
                point.Alpha -= _fadeSpeed * deltaTime;
                if (point.Alpha < 0) point.Alpha = 0;
            }

            TrimPoints();
        }

        private void AddPoint(Vector2 position)
        {
            _points.Add(new TrailPoint
            {
                Position = position,
                Alpha = 1,
                Width = _width
            });
        }

        private void TrimPoints()
        {
            while (_points.Count > _maxPoints)
            {
                _points.TryDequeue(out _);
            }
        }

        public void Clear()
        {
            _points.Clear();
        }

        public List<TrailPoint> GetPoints()
        {
            return _points.GetEnumerable().ToList();
        }
    }

    public struct TrailPoint
    {
        public Vector2 Position { get; set; }
        public float Alpha { get; set; }
        public float Width { get; set; }
    }

    public class Explosion
    {
        private ParticleEmitter? _particles;
        private Shockwave? _shockwave;
        private float _duration;
        private float _elapsed;
        private bool _isActive;
        private Vector2 _position;

        public Vector2 Position => _position;
        public float Duration => _duration;
        public float Elapsed => _elapsed;
        public float NormalizedTime => _duration > 0 ? _elapsed / _duration : 0;
        public bool IsActive => _isActive;

        public Explosion()
        {
            _duration = 1;
            _elapsed = 0;
            _isActive = false;
        }

        public void Trigger(Vector2 position, float duration = 1)
        {
            _position = position;
            _duration = duration;
            _elapsed = 0;
            _isActive = true;

            _particles = new ParticleEmitter(100);
            _particles.Position = position;
            _particles.EmissionRate = 200;
            _particles.Life = 0.5f;
            _particles.Speed = 10;
            _particles.StartColor = new Color(1, 0.5f, 0, 1);
            _particles.EndColor = new Color(1, 0, 0, 0);
            _particles.StartSize = 0.5f;
            _particles.EndSize = 0;
            _particles.Direction = Vector2.Up;
            _particles.DirectionVariance = MathHelper.Pi;
            _particles.Play();

            _shockwave = new Shockwave();
            _shockwave.Initialize(position, 5, 2, duration);
        }

        public void Update(float deltaTime)
        {
            if (!_isActive)
                return;

            _elapsed += deltaTime;

            if (_elapsed >= _duration)
            {
                _isActive = false;
                return;
            }

            _particles?.Update(deltaTime);
            _shockwave?.Update(deltaTime);
        }

        public ParticleEmitter? GetParticles()
        {
            return _particles;
        }

        public Shockwave? GetShockwave()
        {
            return _shockwave;
        }
    }

    public class Shockwave
    {
        private Vector2 _center;
        private float _radius;
        private float _maxRadius;
        private float _speed;
        private float _width;
        private float _duration;
        private float _elapsed;
        private bool _isActive;
        private Color _color;

        public Vector2 Center => _center;
        public float Radius => _radius;
        public float MaxRadius => _maxRadius;
        public float Width => _width;
        public float Duration => _duration;
        public float Elapsed => _elapsed;
        public float NormalizedTime => _duration > 0 ? _elapsed / _duration : 0;
        public bool IsActive => _isActive;

        public Shockwave()
        {
            _isActive = false;
        }

        public void Initialize(Vector2 center, float maxRadius, float speed, float duration)
        {
            _center = center;
            _maxRadius = maxRadius;
            _speed = speed;
            _width = maxRadius * 0.1f;
            _duration = duration;
            _elapsed = 0;
            _radius = 0;
            _isActive = true;
            _color = Color.White;
        }

        public void Update(float deltaTime)
        {
            if (!_isActive)
                return;

            _elapsed += deltaTime;
            _radius = _elapsed * _speed;

            if (_elapsed >= _duration)
            {
                _isActive = false;
            }
        }

        public Color GetCurrentColor()
        {
            return _color.WithAlpha(1 - NormalizedTime);
        }

        public float GetCurrentWidth()
        {
            return _width * (1 - NormalizedTime * 0.5f);
        }
    }

    public class SmokePlume
    {
        private readonly List<SmokeParticle> _particles;
        private readonly int _maxParticles;
        private Vector2 _origin;
        private Vector2 _windDirection;
        private float _windStrength;
        private float _spawnRate;
        private float _spawnAccumulator;
        private bool _isActive;

        public Vector2 Origin
        {
            get => _origin;
            set => _origin = value;
        }

        public Vector2 WindDirection
        {
            get => _windDirection;
            set => _windDirection = value.Normalized();
        }

        public float WindStrength
        {
            get => _windStrength;
            set => _windStrength = value;
        }

        public float SpawnRate
        {
            get => _spawnRate;
            set => _spawnRate = value;
        }

        public bool IsActive => _isActive;

        public SmokePlume(int maxParticles = 50)
        {
            _maxParticles = maxParticles;
            _particles = new List<SmokeParticle>(maxParticles);
            _origin = Vector2.Zero;
            _windDirection = Vector2.Right;
            _windStrength = 0.5f;
            _spawnRate = 10;
            _spawnAccumulator = 0;
            _isActive = false;
        }

        public void Start()
        {
            _isActive = true;
        }

        public void Stop()
        {
            _isActive = false;
        }

        public void Update(float deltaTime)
        {
            if (!_isActive)
                return;

            _spawnAccumulator += _spawnRate * deltaTime;
            while (_spawnAccumulator >= 1 && _particles.Count < _maxParticles)
            {
                SpawnParticle();
                _spawnAccumulator -= 1;
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Life -= deltaTime;

                if (p.Life <= 0)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                p.Position += p.Velocity * deltaTime;
                p.Velocity += _windDirection * _windStrength * deltaTime;
                p.Velocity *= 0.99f;
                p.Size += deltaTime * 0.5f;
                p.Alpha -= deltaTime * 0.5f;
            }
        }

        private void SpawnParticle()
        {
            var random = new Random();
            float x = (float)(random.NextDouble() * 2 - 1) * 0.5f;
            float y = (float)(random.NextDouble() * 2 - 1) * 0.5f;

            _particles.Add(new SmokeParticle
            {
                Position = _origin + new Vector2(x, y),
                Velocity = new Vector2(0, 1 + (float)random.NextDouble() * 0.5f),
                Size = 0.5f + (float)random.NextDouble() * 0.5f,
                Life = 2 + (float)random.NextDouble(),
                Alpha = 0.8f
            });
        }

        public List<SmokeParticle> GetParticles()
        {
            return new List<SmokeParticle>(_particles);
        }
    }

    public class SmokeParticle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Size { get; set; }
        public float Life { get; set; }
        public float Alpha { get; set; }
    }

    public class Snowflake
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Size { get; set; }
        public float Rotation { get; set; }
        public float RotationSpeed { get; set; }
        public float Wobble { get; set; }
        public float WobbleSpeed { get; set; }
        public float Alpha { get; set; }
    }

    public class SnowSystem
    {
        private readonly List<Snowflake> _snowflakes;
        private readonly int _maxFlakes;
        private Vector2 _bounds;
        private Vector2 _wind;
        private float _gravity;
        private float _spawnRate;
        private float _spawnAccumulator;
        private Random _random;

        public Vector2 Bounds
        {
            get => _bounds;
            set => _bounds = value;
        }

        public Vector2 Wind
        {
            get => _wind;
            set => _wind = value;
        }

        public float Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        public float SpawnRate
        {
            get => _spawnRate;
            set => _spawnRate = value;
        }

        public int MaxFlakes
        {
            get => _maxFlakes;
        }

        public int ActiveFlakeCount => _snowflakes.Count;

        public SnowSystem(int maxFlakes = 100)
        {
            _maxFlakes = maxFlakes;
            _snowflakes = new List<Snowflake>(maxFlakes);
            _bounds = new Vector2(1920, 1080);
            _wind = Vector2.Zero;
            _gravity = 50;
            _spawnRate = 20;
            _spawnAccumulator = 0;
            _random = new Random();
        }

        public void Update(float deltaTime)
        {
            _spawnAccumulator += _spawnRate * deltaTime;
            while (_spawnAccumulator >= 1 && _snowflakes.Count < _maxFlakes)
            {
                SpawnSnowflake();
                _spawnAccumulator -= 1;
            }

            for (int i = _snowflakes.Count - 1; i >= 0; i--)
            {
                var flake = _snowflakes[i];

                flake.Position += flake.Velocity * deltaTime;
                flake.Position += _wind * deltaTime;
                flake.Position += new Vector2(
                    (float)Math.Sin(flake.Position.Y * flake.WobbleSpeed) * flake.Wobble,
                    0) * deltaTime;

                flake.Rotation += flake.RotationSpeed * deltaTime;

                if (flake.Position.Y < -flake.Size ||
                    flake.Position.X < -flake.Size ||
                    flake.Position.X > _bounds.X + flake.Size)
                {
                    _snowflakes.RemoveAt(i);
                }
            }
        }

        private void SpawnSnowflake()
        {
            float x = (float)_random.NextDouble() * _bounds.X;
            float size = 1 + (float)_random.NextDouble() * 3;

            _snowflakes.Add(new Snowflake
            {
                Position = new Vector2(x, _bounds.Y + size),
                Velocity = new Vector2(0, -(_gravity * 0.5f + (float)_random.NextDouble() * _gravity * 0.5f)),
                Size = size,
                Rotation = (float)_random.NextDouble() * MathHelper.TwoPi,
                RotationSpeed = ((float)_random.NextDouble() * 2 - 1) * 2,
                Wobble = 5 + (float)_random.NextDouble() * 10,
                WobbleSpeed = 0.5f + (float)_random.NextDouble() * 0.5f,
                Alpha = 0.3f + (float)_random.NextDouble() * 0.4f
            });
        }

        public List<Snowflake> GetSnowflakes()
        {
            return new List<Snowflake>(_snowflakes);
        }
    }
}

namespace WPFEngine.Math
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _availableObjects;
        private readonly Func<T>? _objectFactory;
        private readonly Action<T>? _resetAction;
        private int _totalCreated;
        private int _totalBorrowed;

        public int AvailableCount => _availableObjects.Count;
        public int BorrowedCount => _totalBorrowed - _availableObjects.Count;
        public int TotalCreated => _totalCreated;

        public ObjectPool(int initialCapacity = 0, Func<T>? factory = null, Action<T>? resetAction = null)
        {
            _availableObjects = new Stack<T>();
            _objectFactory = factory ?? (() => new T());
            _resetAction = resetAction;
            _totalCreated = 0;
            _totalBorrowed = 0;

            for (int i = 0; i < initialCapacity; i++)
            {
                Return(_objectFactory());
            }
        }

        public T Rent()
        {
            T obj;
            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Pop();
            }
            else
            {
                obj = _objectFactory();
                _totalCreated++;
            }
            _totalBorrowed++;
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null)
                return;

            _resetAction?.Invoke(obj);
            _availableObjects.Push(obj);
        }

        public void Clear()
        {
            _availableObjects.Clear();
        }

        public List<T> RentMultiple(int count)
        {
            var objects = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                objects.Add(Rent());
            }
            return objects;
        }

        public void ReturnMultiple(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Return(obj);
            }
        }
    }

    public class RingBuffer<T>
    {
        private readonly T[] _buffer;
        private readonly int _capacity;
        private int _head;
        private int _tail;
        private int _count;

        public int Count => _count;
        public int Capacity => _capacity;
        public bool IsEmpty => _count == 0;
        public bool IsFull => _count == _capacity;

        public RingBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        public void Enqueue(T item)
        {
            _buffer[_tail] = item;
            _tail = (_tail + 1) % _capacity;

            if (_count < _capacity)
            {
                _count++;
            }
            else
            {
                _head = (_head + 1) % _capacity;
            }
        }

        public T Dequeue()
        {
            if (_count == 0)
                throw new InvalidOperationException("Buffer is empty");

            T item = _buffer[_head];
            _buffer[_head] = default!;
            _head = (_head + 1) % _capacity;
            _count--;
            return item;
        }

        public bool TryDequeue(out T? item)
        {
            if (_count == 0)
            {
                item = default;
                return false;
            }

            item = Dequeue();
            return true;
        }

        public T Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Buffer is empty");

            return _buffer[_head];
        }

        public bool TryPeek(out T? item)
        {
            if (_count == 0)
            {
                item = default;
                return false;
            }

            item = Peek();
            return true;
        }

        public void Clear()
        {
            _head = 0;
            _tail = 0;
            _count = 0;
            Array.Clear(_buffer, 0, _capacity);
        }

        public T[] ToArray()
        {
            var result = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _buffer[(_head + i) % _capacity];
            }
            return result;
        }

        public IEnumerable<T> GetEnumerable()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[(_head + i) % _capacity];
            }
        }
    }
}

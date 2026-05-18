using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WPFEngine.Math;
using WPFEngine.Scene;
using WPFEngine.Rendering.Graphics;
using WPFEngine.Input;
using WPFEngine.Resources;
using WPFEngine.Audio;
using WPFEngine.Debug;

namespace WPFEngine.Core
{
    public class GameBase : IDisposable
    {
        private Window? _window;
        private FrameworkElement? _renderTarget;
        private DrawingGroup? _drawingGroup;
        private bool _isRunning;
        private bool _isPaused;
        private bool _isDisposed;
        private float _targetFrameRate;
        private float _deltaTime;
        private float _totalTime;
        private int _frameCount;
        private long _lastFrameTime;
        private GameState _state;
        private GameSettings _settings;
        private readonly List<IGameSystem> _systems;
        private SceneManager? _sceneManager;
        private InputManager? _inputManager;
        private ResourceManager? _resourceManager;
        private AudioManager? _audioManager;
        private DebugManager? _debugManager;

        public Window? Window
        {
            get => _window;
            set => _window = value;
        }

        public FrameworkElement? RenderTarget
        {
            get => _renderTarget;
            set => _renderTarget = value;
        }

        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;
        public bool IsDisposed => _isDisposed;
        public float TargetFrameRate
        {
            get => _targetFrameRate;
            set => _targetFrameRate = Math.Clamp(value, 1, 1000);
        }

        public float DeltaTime => _deltaTime;
        public float TotalTime => _totalTime;
        public int FrameCount => _frameCount;
        public float FramesPerSecond => _deltaTime > 0 ? 1f / _deltaTime : 0;
        public GameState State => _state;
        public GameSettings Settings => _settings;

        public SceneManager? SceneManager => _sceneManager;
        public InputManager? InputManager => _inputManager;
        public ResourceManager? ResourceManager => _resourceManager;
        public AudioManager? AudioManager => _audioManager;
        public DebugManager? DebugManager => _debugManager;

        public event EventHandler? GameStarted;
        public event EventHandler? GamePaused;
        public event EventHandler? GameResumed;
        public event EventHandler? GameStopped;
        public event EventHandler? GameExiting;
        public event EventHandler? GameUpdated;
        public event EventHandler? GameRendered;
        public event EventHandler<Exception>? GameError;

        public GameBase()
        {
            _isRunning = false;
            _isPaused = false;
            _isDisposed = false;
            _targetFrameRate = 60f;
            _deltaTime = 0f;
            _totalTime = 0f;
            _frameCount = 0;
            _lastFrameTime = 0;
            _state = GameState.Uninitialized;
            _settings = new GameSettings();
            _systems = new List<IGameSystem>();
            _drawingGroup = new DrawingGroup();
        }

        protected virtual void Initialize()
        {
            _state = GameState.Initializing;

            _sceneManager = SceneManager.Instance;
            _inputManager = InputManager.Instance;
            _resourceManager = ResourceManager.Instance;
            _audioManager = AudioManager.Instance;
            _debugManager = DebugManager.Instance;

            RegisterDefaultSystems();

            _state = GameState.Initialized;
        }

        protected virtual void RegisterDefaultSystems()
        {
            RegisterSystem(new TimeSystem());
            RegisterSystem(new PhysicsSystem());
            RegisterSystem(new RenderSystem());
            RegisterSystem(new AudioSystem());
            RegisterSystem(new InputSystem());
            RegisterSystem(new SceneSystem());
            RegisterSystem(new DebugSystem());
        }

        public void RegisterSystem(IGameSystem system)
        {
            if (!_systems.Contains(system))
            {
                _systems.Add(system);
                system.Initialize(this);
            }
        }

        public void UnregisterSystem(IGameSystem system)
        {
            if (_systems.Remove(system))
            {
                system.Shutdown();
            }
        }

        public T? GetSystem<T>() where T : class, IGameSystem
        {
            return _systems.FirstOrDefault(s => s is T) as T;
        }

        public IGameSystem? GetSystem(Type type)
        {
            return _systems.FirstOrDefault(s => s.GetType() == type);
        }

        public void Run()
        {
            if (_state != GameState.Initialized && _state != GameState.Stopped)
            {
                throw new InvalidOperationException("Game can only be run from Initialized or Stopped state.");
            }

            _state = GameState.Running;
            _isRunning = true;
            _isPaused = false;
            _lastFrameTime = GetCurrentTimeMillis();
            _totalTime = 0;
            _frameCount = 0;

            GameStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                GameLoop();
            }
            catch (Exception ex)
            {
                GameError?.Invoke(this, ex);
            }
            finally
            {
                _isRunning = false;
                _state = GameState.Stopped;
                GameStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RunOneFrame()
        {
            if (_state != GameState.Running)
            {
                Initialize();
                _state = GameState.Running;
                _isRunning = true;
            }

            UpdateFrame();
        }

        private void GameLoop()
        {
            while (_isRunning && !_isDisposed)
            {
                if (_isPaused)
                {
                    System.Threading.Thread.Sleep(10);
                    continue;
                }

                long currentTime = GetCurrentTimeMillis();
                float deltaTime = (currentTime - _lastFrameTime) / 1000f;
                _lastFrameTime = currentTime;

                if (deltaTime > 0 && deltaTime < 1f / (_targetFrameRate * 0.5f))
                {
                    UpdateFrame(deltaTime);
                }
                else if (deltaTime >= 1f / (_targetFrameRate * 0.5f))
                {
                    UpdateFrame(deltaTime);
                }

                if (_renderTarget != null)
                {
                    _renderTarget.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                }
            }
        }

        private void UpdateFrame(float deltaTime = 0.016666f)
        {
            _deltaTime = Math.Clamp(deltaTime, 0.001f, 1f);
            _totalTime += _deltaTime;
            _frameCount++;

            try
            {
                foreach (var system in _systems)
                {
                    if (system.IsEnabled)
                    {
                        system.Update(_deltaTime);
                    }
                }

                OnUpdate(_deltaTime);
                GameUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                GameError?.Invoke(this, ex);
            }
        }

        protected virtual void OnUpdate(float deltaTime)
        {
        }

        protected virtual void OnRender(DrawingContext context)
        {
        }

        public void Pause()
        {
            if (_isRunning && !_isPaused)
            {
                _isPaused = true;
                _state = GameState.Paused;
                GamePaused?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Resume()
        {
            if (_isRunning && _isPaused)
            {
                _isPaused = false;
                _state = GameState.Running;
                _lastFrameTime = GetCurrentTimeMillis();
                GameResumed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _state = GameState.Stopped;
            }
        }

        public void Exit()
        {
            if (_isRunning)
            {
                GameExiting?.Invoke(this, EventArgs.Empty);
                Stop();
            }
        }

        public void Render(DrawingContext context)
        {
            using (var dc = _drawingGroup?.Open())
            {
                if (dc != null)
                {
                    dc.DrawRectangle(System.Windows.Media.Brushes.Black, null, new Rect(0, 0, 1920, 1080));

                    OnRender(dc);

                    foreach (var system in _systems)
                    {
                        if (system is IRenderSystem renderSystem)
                        {
                            renderSystem.Render(dc);
                        }
                    }

                    GameRendered?.Invoke(this, EventArgs.Empty);
                }
            }

            context.DrawDrawing(_drawingGroup);
        }

        public void SetResolution(int width, int height)
        {
            _settings.ResolutionWidth = width;
            _settings.ResolutionHeight = height;
        }

        public void SetFullscreen(bool fullscreen)
        {
            _settings.Fullscreen = fullscreen;
            if (_window != null)
            {
                if (fullscreen)
                {
                    _window.WindowState = WindowState.Maximized;
                    _window.WindowStyle = WindowStyle.None;
                }
                else
                {
                    _window.WindowState = WindowState.Normal;
                    _window.WindowStyle = WindowStyle.SingleBorderWindow;
                }
            }
        }

        public void SetVSync(bool enabled)
        {
            _settings.VSync = enabled;
        }

        public void SetTargetFrameRate(float fps)
        {
            _targetFrameRate = Math.Clamp(fps, 1, 1000);
        }

        public void SetTimeScale(float scale)
        {
            _settings.TimeScale = Math.Clamp(scale, 0f, 10f);
        }

        public void SetMouseVisible(bool visible)
        {
            if (_window != null)
            {
                _window.Cursor = visible ? Cursors.Arrow : Cursors.None;
            }
        }

        private long GetCurrentTimeMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            Stop();

            foreach (var system in _systems.AsEnumerable().Reverse())
            {
                system.Shutdown();
            }

            _systems.Clear();

            GameExiting?.Invoke(this, EventArgs.Empty);
        }
    }

    public enum GameState
    {
        Uninitialized,
        Initializing,
        Initialized,
        Running,
        Paused,
        Stopped,
        Disposed
    }

    public interface IGameSystem
    {
        void Initialize(GameBase game);
        void Update(float deltaTime);
        void Shutdown();
        bool IsEnabled { get; set; }
    }

    public class TimeSystem : IGameSystem
    {
        private GameBase? _game;
        private float _timeScale = 1f;
        private float _unscaledDeltaTime;
        private float _scaledDeltaTime;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Math.Clamp(value, 0f, 10f);
        }

        public float UnscaledDeltaTime => _unscaledDeltaTime;
        public float ScaledDeltaTime => _scaledDeltaTime;

        public void Initialize(GameBase game)
        {
            _game = game;
        }

        public void Update(float deltaTime)
        {
            _unscaledDeltaTime = deltaTime;
            _scaledDeltaTime = deltaTime * _timeScale;
        }

        public void Shutdown()
        {
            _game = null;
        }
    }

    public class PhysicsSystem : IGameSystem
    {
        private GameBase? _game;
        private bool _isEnabled = true;
        private bool _isPaused;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public void Initialize(GameBase game)
        {
            _game = game;
        }

        public void Update(float deltaTime)
        {
            if (_isPaused)
                return;

            PhysicsWorld.Instance.Step(deltaTime);
        }

        public void Shutdown()
        {
            _game = null;
        }

        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;
    }

    public interface IRenderSystem
    {
        void Render(DrawingContext context);
    }

    public class RenderSystem : IGameSystem, IRenderSystem
    {
        private GameBase? _game;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public void Initialize(GameBase game)
        {
            _game = game;
        }

        public void Update(float deltaTime)
        {
        }

        public void Render(DrawingContext context)
        {
            var sceneManager = _game?.SceneManager;
            var scene = sceneManager?.CurrentScene;

            if (scene != null)
            {
                var camera = scene.MainCamera;
                if (camera != null)
                {
                    scene.Draw(deltaTime);
                }
            }
        }

        public void Shutdown()
        {
            _game = null;
        }
    }

    public class AudioSystem : IGameSystem
    {
        private GameBase? _game;
        private bool _isEnabled = true;
        private bool _isMuted;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => _isMuted = value;
        }

        public void Initialize(GameBase game)
        {
            _game = game;
        }

        public void Update(float deltaTime)
        {
            AudioManager.Instance.Update(deltaTime);
        }

        public void Shutdown()
        {
            AudioManager.Instance.StopAll();
            _game = null;
        }
    }

    public class InputSystem : IGameSystem
    {
        private GameBase? _game;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public void Initialize(GameBase game)
        {
            _game = game;
        }

        public void Update(float deltaTime)
        {
            InputManager.Instance.Update(deltaTime);
        }

        public void Shutdown()
        {
            InputManager.Instance.ClearAllInputs();
            _game = null;
        }
    }

    public class SceneSystem : IGameSystem
    {
        private GameBase? _game;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public void Initialize(GameBase game)
        {
            _game = game;
        }

        public void Update(float deltaTime)
        {
            SceneManager.Instance.Update(deltaTime);
        }

        public void Shutdown()
        {
            SceneManager.Instance.ClearAllScenes();
            _game = null;
        }
    }

    public class DebugSystem : IGameSystem
    {
        private GameBase? _game;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public void Initialize(GameBase game)
        {
            _game = game;
        }

        public void Update(float deltaTime)
        {
            Profiler.Instance.IsEnabled = _isEnabled;
            PerformanceMonitor.Instance.Update(deltaTime, _game?.FramesPerSecond ?? 0);
        }

        public void Shutdown()
        {
            _game = null;
        }
    }

    public class GameSettings
    {
        public int ResolutionWidth { get; set; } = 1920;
        public int ResolutionHeight { get; set; } = 1080;
        public bool Fullscreen { get; set; } = false;
        public bool VSync { get; set; } = true;
        public float TargetFrameRate { get; set; } = 60f;
        public float TimeScale { get; set; } = 1f;
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 0.8f;
        public float SfxVolume { get; set; } = 1f;
        public bool ShowFPS { get; set; } = true;
        public bool ShowDebugInfo { get; set; } = false;
        public int MaxParticles { get; set; } = 1000;
        public int PhysicsIterations { get; set; } = 8;
        public float GravityStrength { get; set; } = 9.81f;
        public bool EnableRaycasting { get; set; } = true;
        public bool EnableCollisionDetection { get; set; } = true;
        public bool EnableAudio { get; set; } = true;
        public bool EnableParticles { get; set; } = true;
        public int MaxSoundSources { get; set; } = 32;
        public float MouseSensitivity { get; set; } = 1f;
        public string Language { get; set; } = "en";
    }

    public class DebugManager
    {
        private static DebugManager? _instance;
        public static DebugManager Instance => _instance ??= new DebugManager();

        private bool _showDebugInfo;
        private bool _showFPS;
        private bool _showPhysicsInfo;
        private bool _showMemoryInfo;
        private bool _showVersion;
        private Vector2 _position;
        private Color _textColor;
        private Color _backgroundColor;
        private string _version;

        public bool ShowDebugInfo
        {
            get => _showDebugInfo;
            set => _showDebugInfo = value;
        }

        public bool ShowFPS
        {
            get => _showFPS;
            set => _showFPS = value;
        }

        public bool ShowPhysicsInfo
        {
            get => _showPhysicsInfo;
            set => _showPhysicsInfo = value;
        }

        public bool ShowMemoryInfo
        {
            get => _showMemoryInfo;
            set => _showMemoryInfo = value;
        }

        public bool ShowVersion
        {
            get => _showVersion;
            set => _showVersion = value;
        }

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public string Version
        {
            get => _version;
            set => _version = value;
        }

        private DebugManager()
        {
            _showDebugInfo = true;
            _showFPS = true;
            _showPhysicsInfo = false;
            _showMemoryInfo = false;
            _showVersion = true;
            _position = new Vector2(10, 10);
            _textColor = Color.White;
            _backgroundColor = new Color(0, 0, 0, 0.5f);
            _version = "1.0.0";
        }

        public void Render(DrawingContext context)
        {
            if (!_showDebugInfo)
                return;

            float y = _position.Y;
            var lines = new List<string>();

            if (_showVersion)
                lines.Add($"Version: {_version}");
            if (_showFPS)
                lines.Add($"FPS: {PerformanceMonitor.Instance.CurrentFPass:F0}");
            if (_showPhysicsInfo)
                lines.Add($"Physics Bodies: {PhysicsWorld.Instance.Bodies.Count}");
            if (_showMemoryInfo)
                lines.Add($"Memory: {GC.GetTotalMemory(false) / 1024 / 1024}MB");

            var typeface = new System.Windows.Media.Typeface("Consolas");
            var brush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_textColor.GetAByte(), _textColor.GetRByte(),
                    _textColor.GetGByte(), _textColor.GetBByte()));

            var bgBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_backgroundColor.GetAByte(), _backgroundColor.GetRByte(),
                    _backgroundColor.GetGByte(), _backgroundColor.GetBByte()));

            float height = lines.Count * 16 + 10;
            context.DrawRectangle(bgBrush, null,
                new Rect(_position.X, _position.Y, 200, height));

            foreach (var line in lines)
            {
                var ft = new System.Windows.Media.FormattedText(
                    line,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    12,
                    brush,
                    1.0);

                context.DrawText(ft, new Point(_position.X + 5, y + 5));
                y += 16;
            }
        }
    }

    public class PerformanceMonitor
    {
        private static PerformanceMonitor? _instance;
        public static PerformanceMonitor Instance => _instance ??= new PerformanceMonitor();

        private float _currentFPS;
        private float _averageFPS;
        private float _minFPS;
        private float _maxFPS;
        private long _currentMemory;
        private long _peakMemory;
        private int _frameCount;
        private float _fpsUpdateInterval;
        private float _fpsAccumulator;
        private float _fpsTimer;

        public float CurrentFPS => _currentFPS;
        public float AverageFPS => _averageFPS;
        public float MinFPS => _minFPS;
        public float MaxFPS => _maxFPS;
        public long CurrentMemory => _currentMemory;
        public long PeakMemory => _peakMemory;

        private PerformanceMonitor()
        {
            _currentFPS = 0;
            _averageFPS = 0;
            _minFPS = float.MaxValue;
            _maxFPS = 0;
            _currentMemory = 0;
            _peakMemory = 0;
            _frameCount = 0;
            _fpsUpdateInterval = 0.5f;
            _fpsAccumulator = 0;
            _fpsTimer = 0;
        }

        public void Update(float deltaTime, float fps)
        {
            _currentFPS = fps;
            _fpsAccumulator += fps;
            _fpsTimer += deltaTime;
            _frameCount++;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                _averageFPS = _fpsAccumulator / _fpsTimer;
                _minFPS = Math.Min(_minFPS, fps);
                _maxFPS = Math.Max(_maxFPS, fps);
                _fpsAccumulator = 0;
                _fpsTimer = 0;
            }

            _currentMemory = GC.GetTotalMemory(false);
            _peakMemory = Math.Max(_peakMemory, _currentMemory);
        }

        public void Reset()
        {
            _currentFPS = 0;
            _averageFPS = 0;
            _minFPS = float.MaxValue;
            _maxFPS = 0;
            _frameCount = 0;
        }
    }

    public class Profiler
    {
        private static Profiler? _instance;
        public static Profiler Instance => _instance ??= new Profiler();

        private readonly Dictionary<string, ProfileSample> _samples;
        private readonly Stack<string> _sampleStack;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        private Profiler()
        {
            _samples = new Dictionary<string, ProfileSample>();
            _sampleStack = new Stack<string>();
            _isEnabled = true;
        }

        public void BeginSample(string name)
        {
            if (!_isEnabled)
                return;

            _sampleStack.Push(name);

            if (!_samples.TryGetValue(name, out var sample))
            {
                sample = new ProfileSample { Name = name };
                _samples[name] = sample;
            }

            sample.StartTime = DateTime.Now;
            sample.CallCount++;
        }

        public void EndSample(string name)
        {
            if (!_isEnabled || _sampleStack.Count == 0)
                return;

            var popped = _sampleStack.Pop();
            if (popped != name)
                return;

            if (_samples.TryGetValue(name, out var sample))
            {
                sample.TotalTime += (DateTime.Now - sample.StartTime).TotalMilliseconds;
            }
        }

        public ProfileSample? GetSample(string name)
        {
            return _samples.TryGetValue(name, out var sample) ? sample : null;
        }

        public void Reset()
        {
            foreach (var sample in _samples.Values)
            {
                sample.Reset();
            }
        }
    }

    public class ProfileSample
    {
        public string Name { get; set; } = "";
        public DateTime StartTime { get; set; }
        public double TotalTime { get; set; }
        public int CallCount { get; set; }
        public double AverageTime => CallCount > 0 ? TotalTime / CallCount : 0;

        public void Reset()
        {
            TotalTime = 0;
            CallCount = 0;
        }
    }

    public class GameObjectFactory
    {
        private readonly Dictionary<string, Func<GameObject>> _prefabs;

        public GameObjectFactory()
        {
            _prefabs = new Dictionary<string, Func<GameObject>>();
        }

        public void RegisterPrefab(string name, Func<GameObject> factory)
        {
            _prefabs[name] = factory;
        }

        public void UnregisterPrefab(string name)
        {
            _prefabs.Remove(name);
        }

        public GameObject? Create(string name)
        {
            if (_prefabs.TryGetValue(name, out var factory))
            {
                return factory();
            }
            return null;
        }

        public T? Create<T>(string name) where T : GameObject, new()
        {
            if (_prefabs.TryGetValue(name, out var factory))
            {
                return factory() as T;
            }

            var obj = new T();
            _prefabs[name] = () => obj.Clone() as T ?? new T();
            return obj;
        }

        public GameObject CreateDefault(string name, Vector2 position)
        {
            var obj = new GameObject(name);
            obj.Transform.Position = position;
            return obj;
        }

        public GameObject CreateSprite(string name, Vector2 position, string texturePath)
        {
            var obj = new GameObject(name);
            obj.Transform.Position = position;
            var sprite = obj.AddComponent<SpriteRenderer>();
            sprite.TexturePath = texturePath;
            return obj;
        }

        public GameObject CreateText(string name, Vector2 position, string text)
        {
            var obj = new GameObject(name);
            obj.Transform.Position = position;
            var renderer = obj.AddComponent<TextRenderer>();
            renderer.Text = text;
            return obj;
        }

        public GameObject CreateButton(string name, Vector2 position, Vector2 size, string text)
        {
            var obj = new GameObject(name);
            obj.Transform.Position = position;
            obj.Transform.Scale = size;

            var sprite = obj.AddComponent<SpriteRenderer>();
            sprite.Color = new Color(0.8f, 0.8f, 0.8f);

            var collider = obj.AddComponent<BoxCollider2D>();
            collider.Size = size;

            var button = obj.AddComponent<UIButton>();
            button.Text = text;

            return obj;
        }
    }

    public class SpriteRenderer : Component
    {
        public string? TexturePath { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Opacity { get; set; } = 1f;
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public int SortingOrder { get; set; }
        public string? SortingLayer { get; set; }
        public float PixelsPerUnit { get; set; } = 100f;
        public Rectangle? SourceRect { get; set; }
        public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

        public override void Draw(float deltaTime, Camera camera)
        {
        }
    }

    public class TextRenderer : Component
    {
        public string Text { get; set; } = "";
        public string FontFamily { get; set; } = "Arial";
        public float FontSize { get; set; } = 16;
        public Color Color { get; set; } = Color.Black;
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public bool WordWrap { get; set; }
        public float LineSpacing { get; set; } = 1f;

        public override void Draw(float deltaTime, Camera camera)
        {
        }
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public class BoxCollider2D : Component
    {
        public Vector2 Size { get; set; } = Vector2.One;
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public bool IsTrigger { get; set; }
        public PhysicsMaterial? Material { get; set; }
    }

    public class CircleCollider2D : Component
    {
        public float Radius { get; set; } = 0.5f;
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public bool IsTrigger { get; set; }
        public PhysicsMaterial? Material { get; set; }
    }

    public class UIButton : Component
    {
        public string Text { get; set; } = "Button";
        public Color NormalColor { get; set; } = new Color(0.8f, 0.8f, 0.8f);
        public Color HoverColor { get; set; } = new Color(0.9f, 0.9f, 0.9f);
        public Color PressedColor { get; set; } = new Color(0.7f, 0.7f, 0.7f);
        public Color TextColor { get; set; } = Color.Black;

        public event EventHandler? OnClick;
        public event EventHandler? OnPointerEnter;
        public event EventHandler? OnPointerExit;
        public event EventHandler? OnPointerDown;
        public event EventHandler? OnPointerUp;
    }

    public class PhysicsMaterial
    {
        public float Friction { get; set; } = 0.4f;
        public float Bounciness { get; set; } = 0.3f;
        public float Density { get; set; } = 1f;
    }

    public enum BlendMode
    {
        Alpha,
        Additive,
        Multiply,
        Screen
    }

    public class Camera : Component
    {
        public float FieldOfView { get; set; } = 60f;
        public float NearClip { get; set; } = 0.1f;
        public float FarClip { get; set; } = 1000f;
        public Color BackgroundColor { get; set; } = Color.SkyBlue;
        public bool Orthographic { get; set; } = true;
        public float OrthographicSize { get; set; } = 5f;
        public int Depth { get; set; }
        public bool ClearFlags { get; set; } = true;
        public float Zoom { get; set; } = 1f;

        public override void Draw(float deltaTime, Camera camera)
        {
        }
    }
}

namespace WPFEngine.Pooling
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _availableObjects;
        private readonly Func<T>? _objectFactory;
        private readonly Action<T>? _resetAction;
        private readonly int _maxSize;
        private int _totalCreated;
        private int _totalRented;

        public int AvailableCount => _availableObjects.Count;
        public int RentedCount => _totalRented - _availableObjects.Count;
        public int TotalCreated => _totalCreated;
        public int MaxSize => _maxSize;

        public ObjectPool(int initialSize = 0, int maxSize = 100, Func<T>? factory = null, Action<T>? reset = null)
        {
            _maxSize = maxSize;
            _availableObjects = new Stack<T>();
            _objectFactory = factory ?? (() => new T());
            _resetAction = reset;
            _totalCreated = 0;
            _totalRented = 0;

            for (int i = 0; i < initialSize; i++)
            {
                _availableObjects.Push(_objectFactory());
                _totalCreated++;
            }
        }

        public T Rent()
        {
            _totalRented++;

            if (_availableObjects.Count > 0)
            {
                return _availableObjects.Pop();
            }

            if (_totalCreated < _maxSize)
            {
                _totalCreated++;
                return _objectFactory();
            }

            return _objectFactory();
        }

        public void Return(T obj)
        {
            if (obj == null)
                return;

            _resetAction?.Invoke(obj);

            if (_availableObjects.Count < _maxSize)
            {
                _availableObjects.Push(obj);
            }
        }

        public void Clear()
        {
            _availableObjects.Clear();
        }

        public void Prewarm(int count)
        {
            count = Math.Min(count, _maxSize - _totalCreated);

            for (int i = 0; i < count; i++)
            {
                _availableObjects.Push(_objectFactory());
                _totalCreated++;
            }
        }

        public List<T> RentBatch(int count)
        {
            var batch = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                batch.Add(Rent());
            }
            return batch;
        }

        public void ReturnBatch(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Return(obj);
            }
        }
    }

    public class PooledObject<T> where T : class, new()
    {
        private T? _instance;
        private ObjectPool<T>? _pool;

        public T Instance => _instance ?? throw new InvalidOperationException("Object not initialized");

        public void Initialize(T instance, ObjectPool<T> pool)
        {
            _instance = instance;
            _pool = pool;
        }

        public void ReturnToPool()
        {
            if (_pool != null && _instance != null)
            {
                _pool.Return(_instance);
                _instance = null;
                _pool = null;
            }
        }
    }
}

namespace WPFEngine.Events
{
    public class EventDispatcher
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers;
        private readonly object _lock;
        private bool _dispatching;

        public EventDispatcher()
        {
            _handlers = new Dictionary<Type, List<Delegate>>();
            _lock = new object();
            _dispatching = false;
        }

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_handlers.ContainsKey(type))
                {
                    _handlers[type] = new List<Delegate>();
                }
                _handlers[type].Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_handlers.TryGetValue(type, out var handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }

        public void Dispatch<T>(T eventData) where T : class
        {
            List<Delegate>? handlersCopy = null;

            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlersCopy = new List<Delegate>(handlers);
                }
            }

            if (handlersCopy != null)
            {
                _dispatching = true;
                foreach (var handler in handlersCopy)
                {
                    ((Action<T>)handler)(eventData);
                }
                _dispatching = false;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }

        public int GetHandlerCount<T>() where T : class
        {
            lock (_lock)
            {
                return _handlers.TryGetValue(typeof(T), out var handlers) ? handlers.Count : 0;
            }
        }
    }

    public abstract class GameEvent
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public string? Source { get; set; }
    }

    public class CollisionEvent : GameEvent
    {
        public GameObject? ObjectA { get; set; }
        public GameObject? ObjectB { get; set; }
        public Vector2 ContactPoint { get; set; }
        public Vector2 ContactNormal { get; set; }
        public float Penetration { get; set; }
    }

    public class TriggerEvent : GameEvent
    {
        public GameObject? Trigger { get; set; }
        public GameObject? Other { get; set; }
        public bool IsEntering { get; set; }
    }

    public class DamageEvent : GameEvent
    {
        public GameObject? Target { get; set; }
        public GameObject? Source { get; set; }
        public float Damage { get; set; }
        public string? DamageType { get; set; }
    }

    public class HealthChangeEvent : GameEvent
    {
        public GameObject? Target { get; set; }
        public float OldHealth { get; set; }
        public float NewHealth { get; set; }
        public float Delta { get; set; }
    }

    public class ScoreEvent : GameEvent
    {
        public GameObject? Source { get; set; }
        public int Points { get; set; }
        public int TotalScore { get; set; }
    }

    public class GameStateEvent : GameEvent
    {
        public string PreviousState { get; set; } = "";
        public string NewState { get; set; } = "";
    }
}

namespace WPFEngine.AI.Flocking
{
    public class Flock
    {
        private readonly List<Boid> _boids;
        private readonly int _maxBoids;
        private Vector2 _center;
        private float _boundsRadius;

        public List<Boid> Boids => _boids.AsReadOnly();
        public Vector2 Center
        {
            get => _center;
            set => _center = value;
        }

        public float BoundsRadius
        {
            get => _boundsRadius;
            set => _boundsRadius = value;
        }

        public Flock(int maxBoids = 100)
        {
            _maxBoids = maxBoids;
            _boids = new List<Boid>(maxBoids);
            _center = Vector2.Zero;
            _boundsRadius = 50f;
        }

        public Boid AddBoid(Vector2 position, Vector2 velocity)
        {
            if (_boids.Count >= _maxBoids)
                return null!;

            var boid = new Boid(position, velocity);
            _boids.Add(boid);
            return boid;
        }

        public void RemoveBoid(Boid boid)
        {
            _boids.Remove(boid);
        }

        public void Update(float deltaTime)
        {
            foreach (var boid in _boids)
            {
                var acceleration = Vector2.Zero;

                acceleration += CalculateSeparation(boid) * 1.5f;
                acceleration += CalculateAlignment(boid) * 1.0f;
                acceleration += CalculateCohesion(boid) * 1.0f;
                acceleration += CalculateBoundsForce(boid) * 2.0f;

                boid.Acceleration = acceleration;
                boid.Update(deltaTime);
            }
        }

        private Vector2 CalculateSeparation(Boid boid)
        {
            Vector2 steering = Vector2.Zero;
            int count = 0;

            foreach (var other in _boids)
            {
                if (other == boid)
                    continue;

                float distance = (boid.Position - other.Position).Length();
                if (distance < 2f && distance > 0)
                {
                    Vector2 diff = (boid.Position - other.Position).Normalized() / distance;
                    steering += diff;
                    count++;
                }
            }

            if (count > 0)
            {
                steering /= count;
                steering = steering.Normalized() * boid.MaxSpeed - boid.Velocity;
            }

            return steering;
        }

        private Vector2 CalculateAlignment(Boid boid)
        {
            Vector2 averageVelocity = Vector2.Zero;
            int count = 0;

            foreach (var other in _boids)
            {
                if (other == boid)
                    continue;

                float distance = (boid.Position - other.Position).Length();
                if (distance < 10f)
                {
                    averageVelocity += other.Velocity;
                    count++;
                }
            }

            if (count > 0)
            {
                averageVelocity /= count;
                return (averageVelocity.Normalized() * boid.MaxSpeed - boid.Velocity);
            }

            return Vector2.Zero;
        }

        private Vector2 CalculateCohesion(Boid boid)
        {
            Vector2 centerOfMass = Vector2.Zero;
            int count = 0;

            foreach (var other in _boids)
            {
                if (other == boid)
                    continue;

                float distance = (boid.Position - other.Position).Length();
                if (distance < 10f)
                {
                    centerOfMass += other.Position;
                    count++;
                }
            }

            if (count > 0)
            {
                centerOfMass /= count;
                return (centerOfMass - boid.Position).Normalized() * boid.MaxSpeed - boid.Velocity;
            }

            return Vector2.Zero;
        }

        private Vector2 CalculateBoundsForce(Boid boid)
        {
            Vector2 force = Vector2.Zero;
            Vector2 toCenter = _center - boid.Position;
            float distanceFromCenter = toCenter.Length();

            if (distanceFromCenter > _boundsRadius)
            {
                force = toCenter.Normalized() * (distanceFromCenter - _boundsRadius);
            }

            return force;
        }
    }

    public class Boid
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float MaxSpeed { get; set; } = 5f;
        public float MaxForce { get; set; } = 0.1f;

        public Boid(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = Vector2.Zero;
        }

        public void Update(float deltaTime)
        {
            Velocity += Acceleration * deltaTime;

            float speed = Velocity.Length();
            if (speed > MaxSpeed)
            {
                Velocity = Velocity.Normalized() * MaxSpeed;
            }

            Position += Velocity * deltaTime;
            Acceleration = Vector2.Zero;
        }

        public void ApplyForce(Vector2 force)
        {
            Acceleration += force;
        }
    }
}

namespace WPFEngine.AI.SteeringBehaviors
{
    public class SteeringController
    {
        private readonly Boid _boid;
        private readonly List<ISteeringBehavior> _behaviors;
        private float _maxSpeed;
        private float _maxForce;

        public Boid Boid => _boid;
        public float MaxSpeed
        {
            get => _maxSpeed;
            set { _maxSpeed = value; _boid.MaxSpeed = value; }
        }

        public float MaxForce
        {
            get => _maxForce;
            set { _maxForce = value; _boid.MaxForce = value; }
        }

        public SteeringController(Vector2 position, float maxSpeed = 5f, float maxForce = 0.1f)
        {
            _boid = new Boid(position, Vector2.Zero);
            _boid.MaxSpeed = maxSpeed;
            _boid.MaxForce = maxForce;
            _behaviors = new List<ISteeringBehavior>();
            _maxSpeed = maxSpeed;
            _maxForce = maxForce;
        }

        public void AddBehavior(ISteeringBehavior behavior)
        {
            _behaviors.Add(behavior);
        }

        public void RemoveBehavior(ISteeringBehavior behavior)
        {
            _behaviors.Remove(behavior);
        }

        public void ClearBehaviors()
        {
            _behaviors.Clear();
        }

        public void Update(float deltaTime)
        {
            Vector2 steering = Vector2.Zero;

            foreach (var behavior in _behaviors)
            {
                steering += behavior.Calculate(_boid);
            }

            if (steering.Length() > _maxForce)
            {
                steering = steering.Normalized() * _maxForce;
            }

            _boid.ApplyForce(steering);
            _boid.Update(deltaTime);
        }
    }

    public interface ISteeringBehavior
    {
        Vector2 Calculate(Boid boid);
    }

    public class SeekBehavior : ISteeringBehavior
    {
        public Vector2 Target { get; set; }

        public SeekBehavior(Vector2 target)
        {
            Target = target;
        }

        public Vector2 Calculate(Boid boid)
        {
            Vector2 desired = (Target - boid.Position).Normalized() * boid.MaxSpeed;
            return desired - boid.Velocity;
        }
    }

    public class FleeBehavior : ISteeringBehavior
    {
        public Vector2 Target { get; set; }
        public float PanicDistance { get; set; } = 10f;

        public FleeBehavior(Vector2 target)
        {
            Target = target;
            PanicDistance = 10f;
        }

        public Vector2 Calculate(Boid boid)
        {
            float distance = (boid.Position - Target).Length();
            if (distance > PanicDistance)
                return Vector2.Zero;

            Vector2 desired = (boid.Position - Target).Normalized() * boid.MaxSpeed;
            return desired - boid.Velocity;
        }
    }

    public class ArriveBehavior : ISteeringBehavior
    {
        public Vector2 Target { get; set; }
        public float SlowingRadius { get; set; } = 5f;

        public ArriveBehavior(Vector2 target)
        {
            Target = target;
            SlowingRadius = 5f;
        }

        public Vector2 Calculate(Boid boid)
        {
            Vector2 desired = Target - boid.Position;
            float distance = desired.Length();

            if (distance < 0.1f)
                return Vector2.Zero;

            float speed = distance < SlowingRadius ? boid.MaxSpeed * (distance / SlowingRadius) : boid.MaxSpeed;
            desired = desired.Normalized() * speed;

            return desired - boid.Velocity;
        }
    }

    public class WanderBehavior : ISteeringBehavior
    {
        private float _wanderAngle;
        private float _wanderRadius;
        private float _wanderDistance;
        private float _angleChange;

        public float WanderRadius
        {
            get => _wanderRadius;
            set => _wanderRadius = value;
        }

        public float WanderDistance
        {
            get => _wanderDistance;
            set => _wanderDistance = value;
        }

        public float AngleChange
        {
            get => _angleChange;
            set => _angleChange = value;
        }

        public WanderBehavior(float wanderRadius = 1.5f, float wanderDistance = 2f, float angleChange = 0.5f)
        {
            _wanderRadius = wanderRadius;
            _wanderDistance = wanderDistance;
            _angleChange = angleChange;
            _wanderAngle = 0;
        }

        public Vector2 Calculate(Boid boid)
        {
            _wanderAngle += ((float)new Random().NextDouble() * 2 - 1) * _angleChange;

            Vector2 circleCenter = boid.Velocity.Normalized() * _wanderDistance;
            Vector2 displacement = new Vector2(
                MathF.Cos(_wanderAngle) * _wanderRadius,
                MathF.Sin(_wanderAngle) * _wanderRadius
            );

            return circleCenter + displacement;
        }
    }

    public class PursueBehavior : ISteeringBehavior
    {
        public Boid Target { get; set; }
        public float MaxPredictionTime { get; set; } = 2f;

        public PursueBehavior(Boid target)
        {
            Target = target;
            MaxPredictionTime = 2f;
        }

        public Vector2 Calculate(Boid boid)
        {
            float distance = (boid.Position - Target.Position).Length();
            float speed = boid.Velocity.Length();
            float prediction = speed <= 0 ? MaxPredictionTime : Math.Min(distance / speed, MaxPredictionTime);

            Vector2 futurePosition = Target.Position + Target.Velocity * prediction;
            return new SeekBehavior(futurePosition).Calculate(boid);
        }
    }

    public class ObstacleAvoidanceBehavior : ISteeringBehavior
    {
        public List<Vector2> Obstacles { get; set; }
        public float DetectionBoxLength { get; set; } = 5f;
        public float DetectionBoxWidth { get; set; } = 2f;

        public ObstacleAvoidanceBehavior()
        {
            Obstacles = new List<Vector2>();
            DetectionBoxLength = 5f;
            DetectionBoxWidth = 2f;
        }

        public Vector2 Calculate(Boid boid)
        {
            Vector2 avoidance = Vector2.Zero;

            foreach (var obstacle in Obstacles)
            {
                float distance = (boid.Position - obstacle).Length();
                if (distance < DetectionBoxLength + 1f)
                {
                    Vector2 away = (boid.Position - obstacle).Normalized() / distance;
                    avoidance += away;
                }
            }

            return avoidance;
        }
    }
}

namespace WPFEngine.AI.DecisionTree
{
    public abstract class DecisionTreeNode
    {
        public virtual bool Evaluate(GameObject agent) => false;
        public virtual void Reset() { }
    }

    public class DecisionTree : DecisionTreeNode
    {
        private DecisionTreeNode? _root;

        public DecisionTreeNode? Root
        {
            get => _root;
            set => _root = value;
        }

        public override bool Evaluate(GameObject agent)
        {
            return _root?.Evaluate(agent) ?? false;
        }

        public override void Reset()
        {
            _root?.Reset();
        }
    }

    public class DecisionNode : DecisionTreeNode
    {
        public Func<GameObject, bool>? Condition { get; set; }
        public DecisionTreeNode? TrueNode { get; set; }
        public DecisionTreeNode? FalseNode { get; set; }

        public override bool Evaluate(GameObject agent)
        {
            if (Condition == null)
                return false;

            bool result = Condition(agent);

            if (result && TrueNode != null)
                return TrueNode.Evaluate(agent);

            if (!result && FalseNode != null)
                return FalseNode.Evaluate(agent);

            return result;
        }

        public override void Reset()
        {
            TrueNode?.Reset();
            FalseNode?.Reset();
        }
    }

    public class ActionNode : DecisionTreeNode
    {
        public Func<GameObject, bool>? Action { get; set; }

        public override bool Evaluate(GameObject agent)
        {
            return Action?.Invoke(agent) ?? false;
        }
    }
}

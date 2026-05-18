using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WPFEngine.Core.Time;
using WPFEngine.Core.Logging;
using WPFEngine.Input;

namespace WPFEngine.Core.Game
{
    public enum GameState
    {
        Uninitialized,
        Initializing,
        Running,
        Paused,
        Exiting,
        Disposed
    }

    public class Game : IDisposable
    {
        private GameState _state;
        private readonly object _stateLock = new object();
        private HighPrecisionTimer _timer;
        private GameTime _gameTime;
        private bool _isFixedTimeStep;
        private TimeSpan _targetElapsedTime;
        private TimeSpan _maxElapsedTime;
        private int _maxFrameCount;
        private bool _isMouseVisible;
        private bool _isFixedTimeStepEnabled;
        private TimeSpan _accumulatedTime;
        private int _frameCounter;
        private TimeSpan _fpsTimer;
        private ILogger? _logger;

        public GameState State
        {
            get { lock (_stateLock) return _state; }
            private set { lock (_stateLock) _state = value; }
        }

        public GameTime Time => _gameTime;
        public bool IsMouseVisible
        {
            get => _isMouseVisible;
            set => _isMouseVisible = value;
        }
        public bool IsFixedTimeStep
        {
            get => _isFixedTimeStepEnabled;
            set => _isFixedTimeStepEnabled = value;
        }
        public TimeSpan TargetElapsedTime
        {
            get => _targetElapsedTime;
            set => _targetElapsedTime = TimeSpan.FromTicks(Math.Max(value.Ticks, TimeSpan.TicksPerMillisecond));
        }
        public TimeSpan MaxElapsedTime
        {
            get => _maxElapsedTime;
            set => _maxElapsedTime = TimeSpan.FromTicks(Math.Max(value.Ticks, _targetElapsedTime.Ticks));
        }
        public int MaxFrameCount
        {
            get => _maxFrameCount;
            set => _maxFrameCount = Math.Max(value, 1);
        }
        public ILogger? Logger
        {
            get => _logger;
            set => _logger = value;
        }

        public event EventHandler<GameTime>? GameStarted;
        public event EventHandler<GameTime>? GamePaused;
        public event EventHandler<GameTime>? GameResumed;
        public event EventHandler<GameTime>? GameStopped;
        public event EventHandler? Disposing;
        public event EventHandler? Initialized;

        public Game()
        {
            _state = GameState.Uninitialized;
            _timer = new HighPrecisionTimer();
            _gameTime = new GameTime();
            _isFixedTimeStepEnabled = true;
            _targetElapsedTime = TimeSpan.FromTicks(166666);
            _maxElapsedTime = TimeSpan.FromMilliseconds(500);
            _maxFrameCount = 5;
            _isMouseVisible = true;
            _accumulatedTime = TimeSpan.Zero;
            _frameCounter = 0;
            _fpsTimer = TimeSpan.Zero;
        }

        public void Run()
        {
            if (State != GameState.Uninitialized)
            {
                throw new InvalidOperationException("Game can only be run once from Uninitialized state.");
            }

            Initialize();
            State = GameState.Running;
            GameStarted?.Invoke(this, _gameTime);

            _timer.Start();

            try
            {
                while (State != GameState.Exiting && State != GameState.Disposed)
                {
                    Tick();
                }
            }
            finally
            {
                State = GameState.Exiting;
            }
        }

        public void RunOneFrame()
        {
            if (State == GameState.Uninitialized)
            {
                Initialize();
                State = GameState.Running;
                GameStarted?.Invoke(this, _gameTime);
            }

            if (State == GameState.Running)
            {
                Tick();
            }
        }

        private void Tick()
        {
            var currentTime = _timer.Elapsed;
            var elapsed = currentTime - _gameTime.TotalTime;

            if (elapsed > _maxElapsedTime)
            {
                elapsed = _maxElapsedTime;
            }

            if (_isFixedTimeStepEnabled)
            {
                _accumulatedTime += elapsed;

                int stepCount = 0;
                while (_accumulatedTime >= _targetElapsedTime && stepCount < _maxFrameCount)
                {
                    _gameTime.Update(_targetElapsedTime, _targetElapsedTime);

                    InputManager.Instance.Update(_gameTime.DeltaTimeSeconds);
                    Update(_gameTime);

                    _accumulatedTime -= _targetElapsedTime;
                    stepCount++;
                }

                if (stepCount >= _maxFrameCount)
                {
                    _accumulatedTime = TimeSpan.Zero;
                }
            }
            else
            {
                _gameTime.Update(elapsed, elapsed);
                InputManager.Instance.Update(_gameTime.DeltaTimeSeconds);
                Update(_gameTime);
            }

            Draw(_gameTime);

            _fpsTimer += _gameTime.DeltaTime;
            _frameCounter++;
            if (_fpsTimer >= TimeSpan.FromSeconds(1))
            {
                _fpsTimer = TimeSpan.Zero;
                _frameCounter = 0;
            }

            ProcessEvents();
        }

        protected virtual void Initialize()
        {
            State = GameState.Initializing;
            _logger?.LogInfo("Game initializing...");
            InputManager.Instance.Initialize();
            _logger?.LogInfo("Game initialized successfully.");
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Update(GameTime gameTime)
        {
        }

        protected virtual void Draw(GameTime gameTime)
        {
        }

        protected virtual void ProcessEvents()
        {
        }

        public void Exit()
        {
            if (State == GameState.Running || State == GameState.Paused)
            {
                State = GameState.Exiting;
                _logger?.LogInfo("Game exit requested.");
            }
        }

        public void Pause()
        {
            if (State == GameState.Running)
            {
                State = GameState.Paused;
                _gameTime.Pause();
                _logger?.LogInfo("Game paused.");
                GamePaused?.Invoke(this, _gameTime);
            }
        }

        public void Resume()
        {
            if (State == GameState.Paused)
            {
                State = GameState.Running;
                _gameTime.Resume();
                _timer.Restart();
                _logger?.LogInfo("Game resumed.");
                GameResumed?.Invoke(this, _gameTime);
            }
        }

        public void TogglePause()
        {
            if (State == GameState.Running)
                Pause();
            else if (State == GameState.Paused)
                Resume();
        }

        public void ResetElapsedTime()
        {
            _timer.Reset();
            _timer.Start();
            _accumulatedTime = TimeSpan.Zero;
            _gameTime.Reset();
        }

        public void Dispose()
        {
            if (State == GameState.Disposed)
                return;

            Disposing?.Invoke(this, EventArgs.Empty);
            _logger?.LogInfo("Disposing game...");

            State = GameState.Disposed;
            GameStopped?.Invoke(this, _gameTime);
            _logger?.LogInfo("Game disposed.");
        }
    }
}

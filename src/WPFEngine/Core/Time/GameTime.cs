using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Core.Time;
using WPFEngine.Core.Logging;

namespace WPFEngine.Core.Game
{
    public class GameTime
    {
        private TimeSpan _totalTime;
        private TimeSpan _deltaTime;
        private TimeSpan _unscaledDeltaTime;
        private double _timeScale;
        private bool _isPaused;

        public TimeSpan TotalTime => _totalTime;
        public TimeSpan DeltaTime => _deltaTime;
        public TimeSpan UnscaledDeltaTime => _unscaledDeltaTime;
        public double TimeScale
        {
            get => _timeScale;
            set => _timeScale = Math.Clamp(value, 0.0, 10.0);
        }
        public bool IsPaused => _isPaused;
        public float DeltaTimeSeconds => (float)_deltaTime.TotalSeconds;
        public float UnscaledDeltaTimeSeconds => (float)_unscaledDeltaTime.TotalSeconds;
        public float TotalTimeSeconds => (float)_totalTime.TotalSeconds;
        public float FramesPerSecond { get; private set; }
        public int FrameCount { get; private set; }

        public GameTime()
        {
            _timeScale = 1.0;
            _isPaused = false;
            _deltaTime = TimeSpan.Zero;
            _unscaledDeltaTime = TimeSpan.Zero;
            _totalTime = TimeSpan.Zero;
            FramesPerSecond = 0;
            FrameCount = 0;
        }

        public void Update(TimeSpan elapsed, TimeSpan unscaledElapsed)
        {
            if (_isPaused)
            {
                _deltaTime = TimeSpan.Zero;
                _unscaledDeltaTime = TimeSpan.Zero;
            }
            else
            {
                _unscaledDeltaTime = unscaledElapsed;
                _deltaTime = TimeSpan.FromTicks((long)(unscaledElapsed.Ticks * _timeScale));
                _totalTime += _deltaTime;
            }

            FrameCount++;
            if (_deltaTime.TotalSeconds > 0)
            {
                FramesPerSecond = (float)(1.0 / _deltaTime.TotalSeconds);
            }
        }

        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;
        public void TogglePause() => _isPaused = !_isPaused;

        public void Reset()
        {
            _totalTime = TimeSpan.Zero;
            _deltaTime = TimeSpan.Zero;
            _unscaledDeltaTime = TimeSpan.Zero;
            FrameCount = 0;
            FramesPerSecond = 0;
            _isPaused = false;
            _timeScale = 1.0;
        }
    }
}

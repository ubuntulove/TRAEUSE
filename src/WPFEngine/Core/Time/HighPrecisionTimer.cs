using System;
using System.Diagnostics;

namespace WPFEngine.Core.Time
{
    public class HighPrecisionTimer
    {
        private readonly Stopwatch _stopwatch;
        private long _lastTick;
        private long _ticksPerSecond;
        private double _millisecondsPerTick;
        private bool _isRunning;

        public bool IsRunning => _isRunning;
        public long ElapsedTicks => _stopwatch.ElapsedTicks;
        public TimeSpan ElapsedTime => _stopwatch.Elapsed;
        public double ElapsedMilliseconds => _stopwatch.Elapsed.TotalMilliseconds;

        public HighPrecisionTimer()
        {
            _stopwatch = new Stopwatch();
            _ticksPerSecond = Stopwatch.Frequency;
            _millisecondsPerTick = 1000.0 / _ticksPerSecond;
            _lastTick = 0;
            _isRunning = false;
        }

        public void Start()
        {
            _stopwatch.Start();
            _lastTick = _stopwatch.ElapsedTicks;
            _isRunning = true;
        }

        public void Stop()
        {
            _stopwatch.Stop();
            _isRunning = false;
        }

        public void Reset()
        {
            _stopwatch.Reset();
            _lastTick = 0;
            _isRunning = false;
        }

        public void Restart()
        {
            _stopwatch.Restart();
            _lastTick = _stopwatch.ElapsedTicks;
            _isRunning = true;
        }

        public long GetDeltaTicks()
        {
            long currentTick = _stopwatch.ElapsedTicks;
            long delta = currentTick - _lastTick;
            _lastTick = currentTick;
            return delta;
        }

        public double GetDeltaMilliseconds()
        {
            return GetDeltaTicks() * _millisecondsPerTick;
        }

        public TimeSpan GetDeltaTime()
        {
            return TimeSpan.FromTicks(GetDeltaTicks());
        }
    }
}

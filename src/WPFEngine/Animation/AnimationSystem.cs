using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Animation
{
    public interface IInterpolator
    {
        object Interpolate(object start, object end, float t);
        float Interpolate(float start, float end, float t);
        Vector2 Interpolate(Vector2 start, Vector2 end, float t);
        Vector3 Interpolate(Vector3 start, Vector3 end, float t);
        Color Interpolate(Color start, Color end, float t);
    }

    public class LinearInterpolator : IInterpolator
    {
        public object Interpolate(object start, object end, float t)
        {
            if (start is float fStart && end is float fEnd)
                return Interpolate(fStart, fEnd, t);
            if (start is int iStart && end is int iEnd)
                return (int)(iStart + (iEnd - iStart) * t);
            if (start is double dStart && end is double dEnd)
                return dStart + (dEnd - dStart) * t;
            return start;
        }

        public float Interpolate(float start, float end, float t)
        {
            return start + (end - start) * t;
        }

        public Vector2 Interpolate(Vector2 start, Vector2 end, float t)
        {
            return start + (end - start) * t;
        }

        public Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            return start + (end - start) * t;
        }

        public Color Interpolate(Color start, Color end, float t)
        {
            return start.Lerp(end, t);
        }
    }

    public class EaseInInterpolator : IInterpolator
    {
        public object Interpolate(object start, object end, float t)
        {
            if (start is float fStart && end is float fEnd)
                return Interpolate(fStart, fEnd, t);
            return start;
        }

        public float Interpolate(float start, float end, float t)
        {
            t = t * t * t;
            return start + (end - start) * t;
        }

        public Vector2 Interpolate(Vector2 start, Vector2 end, float t)
        {
            t = t * t * t;
            return start + (end - start) * t;
        }

        public Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            t = t * t * t;
            return start + (end - start) * t;
        }

        public Color Interpolate(Color start, Color end, float t)
        {
            t = t * t * t;
            return start.Lerp(end, t);
        }
    }

    public class EaseOutInterpolator : IInterpolator
    {
        public object Interpolate(object start, object end, float t)
        {
            if (start is float fStart && end is float fEnd)
                return Interpolate(fStart, fEnd, t);
            return start;
        }

        public float Interpolate(float start, float end, float t)
        {
            t = 1f - (1f - t) * (1f - t) * (1f - t);
            return start + (end - start) * t;
        }

        public Vector2 Interpolate(Vector2 start, Vector2 end, float t)
        {
            t = 1f - (1f - t) * (1f - t) * (1f - t);
            return start + (end - start) * t;
        }

        public Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            t = 1f - (1f - t) * (1f - t) * (1f - t);
            return start + (end - start) * t;
        }

        public Color Interpolate(Color start, Color end, float t)
        {
            t = 1f - (1f - t) * (1f - t) * (1f - t);
            return start.Lerp(end, t);
        }
    }

    public class EaseInOutInterpolator : IInterpolator
    {
        public object Interpolate(object start, object end, float t)
        {
            if (start is float fStart && end is float fEnd)
                return Interpolate(fStart, fEnd, t);
            return start;
        }

        public float Interpolate(float start, float end, float t)
        {
            if (t < 0.5f)
            {
                t = 4f * t * t * t;
            }
            else
            {
                t = 1f - (float)Math.Pow(-2f * t + 2f, 3f) / 2f;
            }
            return start + (end - start) * t;
        }

        public Vector2 Interpolate(Vector2 start, Vector2 end, float t)
        {
            if (t < 0.5f)
            {
                t = 4f * t * t * t;
            }
            else
            {
                t = 1f - (float)Math.Pow(-2f * t + 2f, 3f) / 2f;
            }
            return start + (end - start) * t;
        }

        public Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            if (t < 0.5f)
            {
                t = 4f * t * t * t;
            }
            else
            {
                t = 1f - (float)Math.Pow(-2f * t + 2f, 3f) / 2f;
            }
            return start + (end - start) * t;
        }

        public Color Interpolate(Color start, Color end, float t)
        {
            if (t < 0.5f)
            {
                t = 4f * t * t * t;
            }
            else
            {
                t = 1f - (float)Math.Pow(-2f * t + 2f, 3f) / 2f;
            }
            return start.Lerp(end, t);
        }
    }

    public class BounceInterpolator : IInterpolator
    {
        public object Interpolate(object start, object end, float t)
        {
            if (start is float fStart && end is float fEnd)
                return Interpolate(fStart, fEnd, t);
            return start;
        }

        public float Interpolate(float start, float end, float t)
        {
            if (t < 0.5f)
            {
                return start + (end - start) * (1f - BounceOut(1f - 2f * t)) / 2f;
            }
            else
            {
                return start + (end - start) * (1f + BounceOut(2f * t - 1f)) / 2f;
            }
        }

        private float BounceOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / d1;
                return n1 * t * t + 0.984375f;
            }
        }

        public Vector2 Interpolate(Vector2 start, Vector2 end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start + (end - start) * value;
        }

        public Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start + (end - start) * value;
        }

        public Color Interpolate(Color start, Color end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start.Lerp(end, value);
        }
    }

    public class ElasticInterpolator : IInterpolator
    {
        public float Amplitude { get; set; } = 1f;
        public float Period { get; set; } = 0.3f;

        public object Interpolate(object start, object end, float t)
        {
            if (start is float fStart && end is float fEnd)
                return Interpolate(fStart, fEnd, t);
            return start;
        }

        public float Interpolate(float start, float end, float t)
        {
            if (t <= 0f) return start;
            if (t >= 1f) return end;

            float s = Period / (2f * MathHelper.Pi) * 1.57079632679f;
            t = t - 1f;
            return (end - start) * (float)Math.Pow(2f, -10f * t) * (float)Math.Sin((t - s) * (2f * Math.PI) / Period) + end;
        }

        public Vector2 Interpolate(Vector2 start, Vector2 end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start + (end - start) * value;
        }

        public Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start + (end - start) * value;
        }

        public Color Interpolate(Color start, Color end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start.Lerp(end, value);
        }
    }

    public class BackInterpolator : IInterpolator
    {
        public float Overshoot { get; set; } = 1.70158f;

        public object Interpolate(object start, object end, float t)
        {
            if (start is float fStart && end is float fEnd)
                return Interpolate(fStart, fEnd, t);
            return start;
        }

        public float Interpolate(float start, float end, float t)
        {
            if (t <= 0f) return start;
            if (t >= 1f) return end;

            float s = Overshoot * 1.70158f;
            t = t - 1f;
            return (end - start) * (t * t * ((s + 1f) * t + s) + 1f) + start;
        }

        public Vector2 Interpolate(Vector2 start, Vector2 end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start + (end - start) * value;
        }

        public Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start + (end - start) * value;
        }

        public Color Interpolate(Color start, Color end, float t)
        {
            float value = Interpolate(0f, 1f, t);
            return start.Lerp(end, value);
        }
    }

    public class Tween
    {
        private float _currentTime;
        private float _duration;
        private float _delay;
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isComplete;
        private bool _isLooping;
        private int _loopCount;
        private int _currentLoop;
        private IInterpolator _interpolator;
        private Action<Tween>? _onStart;
        private Action<Tween>? _onComplete;
        private Action<Tween, float>? _onUpdate;
        private Action<Tween, int>? _onLoop;
        private object? _userData;

        public float CurrentTime => _currentTime;
        public float Duration
        {
            get => _duration;
            set => _duration = Math.Max(0, value);
        }
        public float Delay
        {
            get => _delay;
            set => _delay = Math.Max(0, value);
        }
        public float Progress => _duration > 0 ? _currentTime / _duration : 0f;
        public bool IsPlaying => _isPlaying && !_isPaused;
        public bool IsPaused => _isPaused;
        public bool IsComplete => _isComplete;
        public bool IsLooping => _isLooping;
        public int LoopCount => _loopCount;
        public int CurrentLoop => _currentLoop;
        public object? UserData
        {
            get => _userData;
            set => _userData = value;
        }

        public Tween(float duration, IInterpolator? interpolator = null)
        {
            _duration = duration;
            _currentTime = 0;
            _delay = 0;
            _isPlaying = false;
            _isPaused = false;
            _isComplete = false;
            _isLooping = false;
            _loopCount = 0;
            _currentLoop = 0;
            _interpolator = interpolator ?? new LinearInterpolator();
        }

        public Tween OnStart(Action<Tween> callback)
        {
            _onStart = callback;
            return this;
        }

        public Tween OnUpdate(Action<Tween, float> callback)
        {
            _onUpdate = callback;
            return this;
        }

        public Tween OnComplete(Action<Tween> callback)
        {
            _onComplete = callback;
            return this;
        }

        public Tween OnLoop(Action<Tween, int> callback)
        {
            _onLoop = callback;
            return this;
        }

        public Tween SetLooping(int count = -1)
        {
            _isLooping = true;
            _loopCount = count;
            return this;
        }

        public Tween SetDelay(float delay)
        {
            _delay = delay;
            return this;
        }

        public Tween SetInterpolator(IInterpolator interpolator)
        {
            _interpolator = interpolator;
            return this;
        }

        public void Play()
        {
            if (!_isPlaying)
            {
                _isPlaying = true;
                _isPaused = false;
                _isComplete = false;
                _currentLoop = 0;
                _currentTime = -_delay;
                _onStart?.Invoke(this);
            }
            else if (_isPaused)
            {
                _isPaused = false;
            }
        }

        public void Pause()
        {
            if (_isPlaying && !_isPaused)
            {
                _isPaused = true;
            }
        }

        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
            _isComplete = false;
            _currentTime = 0;
            _currentLoop = 0;
        }

        public void Restart()
        {
            Stop();
            Play();
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying || _isPaused)
                return;

            float previousTime = _currentTime;
            _currentTime += deltaTime;

            if (_currentTime < 0)
            {
                return;
            }

            if (previousTime < 0 && _currentTime >= 0)
            {
                _onStart?.Invoke(this);
            }

            float t = Math.Clamp(_currentTime / _duration, 0f, 1f);
            _onUpdate?.Invoke(this, t);

            if (_currentTime >= _duration)
            {
                if (_isLooping)
                {
                    if (_loopCount < 0 || _currentLoop < _loopCount)
                    {
                        _currentLoop++;
                        _currentTime = 0;
                        _onLoop?.Invoke(this, _currentLoop);
                    }
                    else
                    {
                        Complete();
                    }
                }
                else
                {
                    Complete();
                }
            }
        }

        private void Complete()
        {
            _currentTime = _duration;
            _isComplete = true;
            _isPlaying = false;
            _onUpdate?.Invoke(this, 1f);
            _onComplete?.Invoke(this);
        }

        public float GetValue(float start, float end)
        {
            return _interpolator.Interpolate(start, end, Progress);
        }

        public Vector2 GetValue(Vector2 start, Vector2 end)
        {
            return _interpolator.Interpolate(start, end, Progress);
        }

        public Vector3 GetValue(Vector3 start, Vector3 end)
        {
            return _interpolator.Interpolate(start, end, Progress);
        }

        public Color GetValue(Color start, Color end)
        {
            return _interpolator.Interpolate(start, end, Progress);
        }
    }

    public class TweenManager
    {
        private static TweenManager? _instance;
        public static TweenManager Instance => _instance ??= new TweenManager();

        private readonly List<Tween> _tweens;
        private readonly List<Tween> _toAdd;
        private readonly List<Tween> _toRemove;
        private bool _isUpdating;

        public int ActiveTweenCount => _tweens.Count;

        public event EventHandler<Tween>? TweenStarted;
        public event EventHandler<Tween>? TweenCompleted;

        private TweenManager()
        {
            _tweens = new List<Tween>();
            _toAdd = new List<Tween>();
            _toRemove = new List<Tween>();
            _isUpdating = false;
        }

        public Tween CreateTween(float duration, IInterpolator? interpolator = null)
        {
            var tween = new Tween(duration, interpolator);
            AddTween(tween);
            return tween;
        }

        public Tween<T> CreateTween<T>(float duration, IInterpolator? interpolator = null)
        {
            var tween = new Tween<T>(duration, interpolator);
            AddTween(tween);
            return tween;
        }

        public void AddTween(Tween tween)
        {
            if (_isUpdating)
            {
                _toAdd.Add(tween);
            }
            else
            {
                _tweens.Add(tween);
            }
        }

        public void RemoveTween(Tween tween)
        {
            if (_isUpdating)
            {
                _toRemove.Add(tween);
            }
            else
            {
                _tweens.Remove(tween);
            }
        }

        public void Update(float deltaTime)
        {
            _isUpdating = true;

            foreach (var tween in _tweens)
            {
                if (!tween.IsPlaying)
                    continue;

                bool wasComplete = tween.IsComplete;
                tween.Update(deltaTime);

                if (!wasComplete && !tween.IsComplete)
                {
                    TweenStarted?.Invoke(this, tween);
                }
                else if (wasComplete && tween.IsComplete)
                {
                    TweenCompleted?.Invoke(this, tween);
                }
            }

            _isUpdating = false;

            foreach (var tween in _toRemove)
            {
                _tweens.Remove(tween);
            }
            _toRemove.Clear();

            foreach (var tween in _toAdd)
            {
                _tweens.Add(tween);
            }
            _toAdd.Clear();
        }

        public void RemoveAllTweens()
        {
            _tweens.Clear();
            _toAdd.Clear();
            _toRemove.Clear();
        }

        public void PauseAll()
        {
            foreach (var tween in _tweens)
            {
                if (tween.IsPlaying)
                    tween.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var tween in _tweens)
            {
                if (tween.IsPaused)
                    tween.Play();
            }
        }

        public List<Tween> GetTweensWithUserData(object userData)
        {
            return _tweens.Where(t => t.UserData == userData).ToList();
        }

        public void KillTweensWithUserData(object userData)
        {
            var tweens = GetTweensWithUserData(userData);
            foreach (var tween in tweens)
            {
                RemoveTween(tween);
            }
        }
    }

    public class Tween<T> : Tween where T : struct
    {
        private T _startValue;
        private T _endValue;
        private T _currentValue;
        private Func<T, T, float, T> _lerpFunction;
        private Action<Tween<T>, T>? _onValueUpdate;

        public T CurrentValue => _currentValue;

        public Tween(float duration, IInterpolator? interpolator = null)
            : base(duration, interpolator)
        {
            _lerpFunction = DefaultLerp;
        }

        public Tween<T> From(T value)
        {
            _startValue = value;
            return this;
        }

        public Tween<T> To(T value)
        {
            _endValue = value;
            return this;
        }

        public Tween<T> SetLerpFunction(Func<T, T, float, T> function)
        {
            _lerpFunction = function;
            return this;
        }

        public Tween<T> OnValueUpdate(Action<Tween<T>, T> callback)
        {
            _onValueUpdate = callback;
            return this;
        }

        private T DefaultLerp(T a, T b, float t)
        {
            if (typeof(T) == typeof(float) && _lerpFunction == DefaultLerp)
            {
                float aFloat = (float)(object)a!;
                float bFloat = (float)(object)b!;
                return (T)(object)(aFloat + (bFloat - aFloat) * t);
            }
            return a;
        }

        public new void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _currentValue = LerpValue(_startValue, _endValue, Progress);
            _onValueUpdate?.Invoke(this, _currentValue);
        }

        private T LerpValue(T start, T end, float t)
        {
            return _lerpFunction(start, end, t);
        }

        public T GetValue()
        {
            return LerpValue(_startValue, _endValue, Progress);
        }
    }

    public class Sequence
    {
        private readonly List<Tween> _tweens;
        private int _currentIndex;
        private bool _isPlaying;
        private Action<Sequence>? _onComplete;

        public bool IsPlaying => _isPlaying;
        public int CurrentIndex => _currentIndex;

        public Sequence()
        {
            _tweens = new List<Tween>();
            _currentIndex = 0;
            _isPlaying = false;
        }

        public Sequence Append(Tween tween)
        {
            _tweens.Add(tween);
            return this;
        }

        public Sequence AppendInterval(float interval)
        {
            var tween = TweenManager.Instance.CreateTween(interval);
            _tweens.Add(tween);
            return this;
        }

        public Sequence AppendCallback(Action callback)
        {
            var tween = TweenManager.Instance.CreateTween(0);
            tween.OnComplete(_ => callback());
            _tweens.Add(tween);
            return this;
        }

        public Sequence Join(Tween tween)
        {
            if (_tweens.Count > 0)
            {
                var lastTween = _tweens[_tweens.Count - 1];
                tween.SetDelay(lastTween.Duration);
            }
            _tweens.Add(tween);
            return this;
        }

        public Sequence OnComplete(Action<Sequence> callback)
        {
            _onComplete = callback;
            return this;
        }

        public void Play()
        {
            if (_tweens.Count == 0)
            {
                _onComplete?.Invoke(this);
                return;
            }

            _isPlaying = true;
            _currentIndex = 0;
            PlayCurrent();
        }

        private void PlayCurrent()
        {
            if (_currentIndex >= _tweens.Count)
            {
                _isPlaying = false;
                _onComplete?.Invoke(this);
                return;
            }

            var current = _tweens[_currentIndex];
            current.OnComplete(t =>
            {
                _currentIndex++;
                PlayCurrent();
            });
            current.Play();
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying)
                return;

            TweenManager.Instance.Update(deltaTime);
        }

        public void Stop()
        {
            _isPlaying = false;
            foreach (var tween in _tweens)
            {
                tween.Stop();
            }
        }
    }

    public class SpriteAnimation
    {
        private readonly List<SpriteFrame> _frames;
        private int _currentFrame;
        private float _currentTime;
        private bool _isPlaying;
        private bool _isLooping;

        public IReadOnlyList<SpriteFrame> Frames => _frames;
        public int CurrentFrame => _currentFrame;
        public bool IsPlaying => _isPlaying;
        public bool IsLooping
        {
            get => _isLooping;
            set => _isLooping = value;
        }
        public float FrameRate
        {
            get;
            set;
        } = 12f;

        public SpriteAnimation()
        {
            _frames = new List<SpriteFrame>();
            _currentFrame = 0;
            _currentTime = 0;
            _isPlaying = false;
            _isLooping = true;
        }

        public SpriteAnimation AddFrame(string texturePath, float duration)
        {
            _frames.Add(new SpriteFrame(texturePath, duration));
            return this;
        }

        public SpriteAnimation AddFrame(string texturePath, float duration, Rectangle sourceRect)
        {
            _frames.Add(new SpriteFrame(texturePath, duration) { SourceRect = sourceRect });
            return this;
        }

        public void Play()
        {
            if (_frames.Count == 0)
                return;

            _isPlaying = true;
            _currentTime = 0;
            _currentFrame = 0;
        }

        public void Stop()
        {
            _isPlaying = false;
            _currentFrame = 0;
            _currentTime = 0;
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        public void Resume()
        {
            if (_frames.Count > 0)
                _isPlaying = true;
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying || _frames.Count == 0)
                return;

            _currentTime += deltaTime;

            while (_currentTime >= _frames[_currentFrame].Duration)
            {
                _currentTime -= _frames[_currentFrame].Duration;
                _currentFrame++;

                if (_currentFrame >= _frames.Count)
                {
                    if (_isLooping)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _currentFrame = _frames.Count - 1;
                        _isPlaying = false;
                        return;
                    }
                }
            }
        }

        public SpriteFrame? GetCurrentFrame()
        {
            if (_currentFrame < _frames.Count)
                return _frames[_currentFrame];
            return null;
        }
    }

    public class SpriteFrame
    {
        public string TexturePath { get; set; }
        public float Duration { get; set; }
        public Rectangle SourceRect { get; set; }

        public SpriteFrame(string texturePath, float duration)
        {
            TexturePath = texturePath;
            Duration = duration;
            SourceRect = Rectangle.Empty;
        }
    }

    public struct Rectangle
    {
        public float X, Y, Width, Height;

        public static Rectangle Empty => new Rectangle(0, 0, 0, 0);

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);
        public Vector2 Center => new Vector2(X + Width / 2, Y + Height / 2);
        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;

        public bool Contains(Vector2 point)
        {
            return point.X >= X && point.X <= X + Width &&
                   point.Y >= Y && point.Y <= Y + Height;
        }

        public bool Intersects(Rectangle other)
        {
            return !(other.X > Right || other.Right < Left ||
                     other.Y > Bottom || other.Bottom < Top);
        }
    }

    public class Animator
    {
        private readonly Dictionary<string, SpriteAnimation> _animations;
        private SpriteAnimation? _currentAnimation;
        private string? _currentAnimationName;
        private bool _isPlaying;

        public IReadOnlyDictionary<string, SpriteAnimation> Animations => _animations;
        public SpriteAnimation? CurrentAnimation => _currentAnimation;
        public string? CurrentAnimationName => _currentAnimationName;
        public bool IsPlaying => _isPlaying;

        public event EventHandler<string>? AnimationChanged;
        public event EventHandler? AnimationFinished;

        public Animator()
        {
            _animations = new Dictionary<string, SpriteAnimation>();
            _currentAnimation = null;
            _currentAnimationName = null;
            _isPlaying = false;
        }

        public void AddAnimation(string name, SpriteAnimation animation)
        {
            _animations[name] = animation;
        }

        public bool RemoveAnimation(string name)
        {
            return _animations.Remove(name);
        }

        public SpriteAnimation? GetAnimation(string name)
        {
            return _animations.TryGetValue(name, out var anim) ? anim : null;
        }

        public void Play(string name, bool forceRestart = false)
        {
            if (!_animations.TryGetValue(name, out var animation))
                return;

            if (_currentAnimationName == name && !forceRestart)
                return;

            _currentAnimation?.Stop();
            _currentAnimation = animation;
            _currentAnimationName = name;
            _currentAnimation.Play();
            _isPlaying = true;
            AnimationChanged?.Invoke(this, name);
        }

        public void Stop()
        {
            _currentAnimation?.Stop();
            _isPlaying = false;
        }

        public void Pause()
        {
            _currentAnimation?.Pause();
            _isPlaying = false;
        }

        public void Resume()
        {
            _currentAnimation?.Resume();
            _isPlaying = _currentAnimation != null;
        }

        public void Update(float deltaTime)
        {
            if (_currentAnimation == null || !_isPlaying)
                return;

            _currentAnimation.Update(deltaTime);

            if (!_currentAnimation.IsPlaying)
            {
                _isPlaying = false;
                AnimationFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        public SpriteFrame? GetCurrentFrame()
        {
            return _currentAnimation?.GetCurrentFrame();
        }
    }

    public class Skeleton
    {
        private readonly Dictionary<string, Bone> _bones;
        private Bone? _rootBone;

        public Dictionary<string, Bone> Bones => _bones;
        public Bone? RootBone => _rootBone;

        public Skeleton()
        {
            _bones = new Dictionary<string, Bone>();
            _rootBone = null;
        }

        public Bone CreateBone(string name, string? parentName = null)
        {
            var bone = new Bone(name);

            if (parentName == null)
            {
                if (_rootBone == null)
                {
                    _rootBone = bone;
                }
            }
            else if (_bones.TryGetValue(parentName, out var parent))
            {
                parent.AddChild(bone);
            }

            _bones[name] = bone;
            return bone;
        }

        public Bone? GetBone(string name)
        {
            return _bones.TryGetValue(name, out var bone) ? bone : null;
        }

        public void Update(float deltaTime)
        {
            _rootBone?.Update(deltaTime);
        }
    }

    public class Bone
    {
        public string Name { get; }
        public Transform2D LocalTransform { get; set; }
        public Transform2D WorldTransform { get; private set; }
        public Bone? Parent { get; private set; }
        public List<Bone> Children { get; } = new List<Bone>();

        public Vector2 Position
        {
            get => LocalTransform.Position;
            set
            {
                LocalTransform.Position = value;
                UpdateWorldTransform();
            }
        }

        public float Rotation
        {
            get => LocalTransform.Rotation;
            set
            {
                LocalTransform.Rotation = value;
                UpdateWorldTransform();
            }
        }

        public Bone(string name)
        {
            Name = name;
            LocalTransform = Transform2D.Identity;
            WorldTransform = Transform2D.Identity;
        }

        public void AddChild(Bone child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(Bone child)
        {
            child.Parent = null;
            Children.Remove(child);
        }

        public void Update(float deltaTime)
        {
            UpdateWorldTransform();
            foreach (var child in Children)
            {
                child.Update(deltaTime);
            }
        }

        private void UpdateWorldTransform()
        {
            if (Parent != null)
            {
                WorldTransform = Parent.WorldTransform * LocalTransform;
            }
            else
            {
                WorldTransform = LocalTransform;
            }
        }
    }

    public class Keyframe
    {
        public float Time { get; set; }
        public object? Value { get; set; }
        public IInterpolator? Interpolator { get; set; }
    }

    public class Keyframe<T>
    {
        public float Time { get; set; }
        public T Value { get; set; }
        public IInterpolator? Interpolator { get; set; }
    }

    public class AnimationTrack
    {
        private readonly List<Keyframe> _keyframes;
        private string _targetPath;
        private Type _valueType;

        public IReadOnlyList<Keyframe> Keyframes => _keyframes;
        public string TargetPath => _targetPath;
        public Type ValueType => _valueType;

        public AnimationTrack(string targetPath, Type valueType)
        {
            _targetPath = targetPath;
            _valueType = valueType;
            _keyframes = new List<Keyframe>();
        }

        public AnimationTrack AddKeyframe(float time, object value, IInterpolator? interpolator = null)
        {
            _keyframes.Add(new Keyframe { Time = time, Value = value, Interpolator = interpolator });
            _keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
            return this;
        }

        public object? GetValueAtTime(float time)
        {
            if (_keyframes.Count == 0)
                return null;

            if (time <= _keyframes[0].Time)
                return _keyframes[0].Value;

            if (time >= _keyframes[_keyframes.Count - 1].Time)
                return _keyframes[_keyframes.Count - 1].Value;

            for (int i = 0; i < _keyframes.Count - 1; i++)
            {
                if (time >= _keyframes[i].Time && time < _keyframes[i + 1].Time)
                {
                    float t = (time - _keyframes[i].Time) / (_keyframes[i + 1].Time - _keyframes[i].Time);
                    IInterpolator interpolator = _keyframes[i].Interpolator ?? new LinearInterpolator();

                    if (_valueType == typeof(float) && _keyframes[i].Value is float f1 && _keyframes[i + 1].Value is float f2)
                    {
                        return interpolator.Interpolate(f1, f2, t);
                    }
                    else if (_valueType == typeof(Vector2) && _keyframes[i].Value is Vector2 v1 && _keyframes[i + 1].Value is Vector2 v2)
                    {
                        return interpolator.Interpolate(v1, v2, t);
                    }
                    else if (_valueType == typeof(Vector3) && _keyframes[i].Value is Vector3 v3_1 && _keyframes[i + 1].Value is Vector3 v3_2)
                    {
                        return interpolator.Interpolate(v3_1, v3_2, t);
                    }
                    else if (_valueType == typeof(Color) && _keyframes[i].Value is Color c1 && _keyframes[i + 1].Value is Color c2)
                    {
                        return interpolator.Interpolate(c1, c2, t);
                    }

                    return _keyframes[i].Value;
                }
            }

            return null;
        }
    }

    public class AnimationClip
    {
        private readonly List<AnimationTrack> _tracks;
        private float _duration;
        private bool _isLooping;

        public IReadOnlyList<AnimationTrack> Tracks => _tracks;
        public float Duration => _duration;
        public string Name { get; set; }

        public bool IsLooping
        {
            get => _isLooping;
            set => _isLooping = value;
        }

        public AnimationClip(string name)
        {
            Name = name;
            _tracks = new List<AnimationTrack>();
            _duration = 0;
            _isLooping = false;
        }

        public AnimationTrack CreateTrack(string targetPath, Type valueType)
        {
            var track = new AnimationTrack(targetPath, valueType);
            _tracks.Add(track);
            return track;
        }

        public AnimationTrack<T> CreateTrack<T>(string targetPath)
        {
            var track = new AnimationTrack(targetPath, typeof(T));
            _tracks.Add(track);
            return new AnimationTrack<T>(track);
        }

        public void UpdateDuration()
        {
            _duration = 0;
            foreach (var track in _tracks)
            {
                if (track.Keyframes.Count > 0)
                {
                    float trackDuration = track.Keyframes[track.Keyframes.Count - 1].Time;
                    if (trackDuration > _duration)
                        _duration = trackDuration;
                }
            }
        }
    }

    public class AnimationTrack<T>
    {
        private readonly AnimationTrack _track;

        public AnimationTrack(AnimationTrack track)
        {
            _track = track;
        }

        public AnimationTrack<T> AddKeyframe(float time, T value, IInterpolator? interpolator = null)
        {
            _track.AddKeyframe(time, value!, interpolator);
            return this;
        }

        public T? GetValueAtTime(float time)
        {
            return (T?)_track.GetValueAtTime(time);
        }
    }
}

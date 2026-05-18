using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Input;

namespace OfficeSuite.Presentation.Animation
{
    public class PresentationAnimationController
    {
        private readonly Dictionary<string, AnimationSequence> _sequences;
        private readonly AnimationTimelineFactory _factory;
        private readonly EasingFunctionProvider _easingProvider;
        private AnimationSequence _currentSequence;
        private int _currentStepIndex;

        public PresentationAnimationController()
        {
            _sequences = new Dictionary<string, AnimationSequence>();
            _factory = new AnimationTimelineFactory();
            _easingProvider = new EasingFunctionProvider();
            _currentStepIndex = 0;
        }

        public AnimationSequence CreateSequence(string name, AnimationSequenceOptions options = null)
        {
            options ??= new AnimationSequenceOptions();
            var sequence = new AnimationSequence
            {
                Name = name,
                Options = options,
                Steps = new List<AnimationStep>()
            };
            _sequences[name] = sequence;
            return sequence;
        }

        public void AddStep(string sequenceName, AnimationStep step)
        {
            if (_sequences.TryGetValue(sequenceName, out var sequence))
            {
                step.Index = sequence.Steps.Count;
                sequence.Steps.Add(step);
            }
        }

        public void Play(string sequenceName, FrameworkElement target)
        {
            if (_sequences.TryGetValue(sequenceName, out var sequence))
            {
                _currentSequence = sequence;
                _currentStepIndex = 0;
                PlayCurrentStep(target);
            }
        }

        public void PlayNext(FrameworkElement target)
        {
            if (_currentSequence == null) return;
            if (_currentStepIndex < _currentSequence.Steps.Count - 1)
            {
                _currentStepIndex++;
                PlayCurrentStep(target);
            }
        }

        public void PlayPrevious(FrameworkElement target)
        {
            if (_currentSequence == null) return;
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                PlayCurrentStep(target);
            }
        }

        public void Stop()
        {
            _currentSequence = null;
            _currentStepIndex = 0;
        }

        private void PlayCurrentStep(FrameworkElement target)
        {
            if (_currentSequence == null || target == null) return;
            
            var step = _currentSequence.Steps[_currentStepIndex];
            var timeline = _factory.CreateTimeline(step, target);
            
            if (timeline != null)
            {
                var clock = timeline.CreateClock();
                ApplyAnimation(target, step, clock);
            }
        }

        private void ApplyAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            switch (step.AnimationType)
            {
                case AnimationType.Entrance:
                    ApplyEntranceAnimation(target, step, clock);
                    break;
                case AnimationType.Exit:
                    ApplyExitAnimation(target, step, clock);
                    break;
                case AnimationType.Emphasis:
                    ApplyEmphasisAnimation(target, step, clock);
                    break;
                case AnimationType.Motion:
                    ApplyMotionAnimation(target, step, clock);
                    break;
                case AnimationType.Scale:
                    ApplyScaleAnimation(target, step, clock);
                    break;
                case AnimationType.Rotation:
                    ApplyRotationAnimation(target, step, clock);
                    break;
                case AnimationType.Color:
                    ApplyColorAnimation(target, step, clock);
                    break;
                case AnimationType.Transparency:
                    ApplyTransparencyAnimation(target, step, clock);
                    break;
            }
        }

        private void ApplyEntranceAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            switch (step.EntranceEffect)
            {
                case EntranceEffect.Fade:
                    ApplyFadeInAnimation(target, step, clock);
                    break;
                case EntranceEffect.FlyIn:
                    ApplyFlyInAnimation(target, step, clock);
                    break;
                case EntranceEffect.Zoom:
                    ApplyZoomInAnimation(target, step, clock);
                    break;
                case EntranceEffect.Wipe:
                    ApplyWipeAnimation(target, step, clock);
                    break;
                case EntranceEffect.Split:
                    ApplySplitAnimation(target, step, clock);
                    break;
                case EntranceEffect.Diamond:
                    ApplyDiamondAnimation(target, step, clock);
                    break;
                case EntranceEffect.Circle:
                    ApplyCircleAnimation(target, step, clock);
                    break;
                case EntranceEffect.Box:
                    ApplyBoxAnimation(target, step, clock);
                    break;
                case EntranceEffect.Wave:
                    ApplyWaveAnimation(target, step, clock);
                    break;
                case EntranceEffect.Boom:
                    ApplyBoomAnimation(target, step, clock);
                    break;
                case EntranceEffect.Pulse:
                    ApplyPulseAnimation(target, step, clock);
                    break;
                case EntranceEffect.GrowAndTurn:
                    ApplyGrowAndTurnAnimation(target, step, clock);
                    break;
                case EntranceEffect.Bounce:
                    ApplyBounceAnimation(target, step, clock);
                    break;
                case EntranceEffect.Swivel:
                    ApplySwivelAnimation(target, step, clock);
                    break;
                case EntranceEffect.Wiggle:
                    ApplyWiggleAnimation(target, step, clock);
                    break;
                case EntranceEffect.Spin:
                    ApplySpinAnimation(target, step, clock);
                    break;
            }
        }

        private void ApplyFadeInAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing),
                AutoReverse = step.AutoReverse
            };

            target.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void ApplyFlyInAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var fromPosition = step.Direction switch
            {
                Direction.Top => new Point(0, -target.ActualHeight),
                Direction.Bottom => new Point(0, target.ActualHeight),
                Direction.Left => new Point(-target.ActualWidth, 0),
                Direction.Right => new Point(target.ActualWidth, 0),
                Direction.TopLeft => new Point(-target.ActualWidth, -target.ActualHeight),
                Direction.TopRight => new Point(target.ActualWidth, -target.ActualHeight),
                Direction.BottomLeft => new Point(-target.ActualWidth, target.ActualHeight),
                Direction.BottomRight => new Point(target.ActualWidth, target.ActualHeight),
                _ => new Point(0, 0)
            };

            var translateAnimation = new DoubleAnimation
            {
                From = step.Direction switch
                {
                    Direction.Top or Direction.TopLeft or Direction.TopRight => -target.ActualHeight,
                    Direction.Bottom or Direction.BottomLeft or Direction.BottomRight => target.ActualHeight,
                    _ => 0
                },
                To = 0,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            var horizontalAnimation = new DoubleAnimation
            {
                From = step.Direction switch
                {
                    Direction.Left or Direction.TopLeft or Direction.BottomLeft => -target.ActualWidth,
                    Direction.Right or Direction.TopRight or Direction.BottomRight => target.ActualWidth,
                    _ => 0
                },
                To = 0,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            ApplyTransformAnimation(target, horizontalAnimation, translateAnimation);
        }

        private void ApplyZoomInAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var scaleAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            ApplyScaleTransformAnimation(target, scaleAnimation, scaleAnimation);
        }

        private void ApplyWipeAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var clipAnimation = step.Direction switch
            {
                Direction.Left => CreateWipeClipAnimation(target, true),
                Direction.Right => CreateWipeClipAnimation(target, false),
                Direction.Top => CreateVerticalWipeClipAnimation(target, true),
                Direction.Bottom => CreateVerticalWipeClipAnimation(target, false),
                _ => CreateWipeClipAnimation(target, true)
            };

            ApplyClipAnimation(target, clipAnimation);
        }

        private Timeline CreateWipeClipAnimation(FrameworkElement target, bool fromRight)
        {
            return new DoubleAnimation
            {
                From = fromRight ? target.ActualWidth : 0,
                To = fromRight ? 0 : target.ActualWidth,
                Duration = step?.Duration ?? TimeSpan.FromSeconds(0.5),
                EasingFunction = _easingProvider.GetEasingFunction(step?.Easing ?? EasingType.EaseOut)
            };
        }

        private Timeline CreateVerticalWipeClipAnimation(FrameworkElement target, bool fromBottom)
        {
            return new DoubleAnimation
            {
                From = fromBottom ? target.ActualHeight : 0,
                To = fromBottom ? 0 : target.ActualHeight,
                Duration = step?.Duration ?? TimeSpan.FromSeconds(0.5),
                EasingFunction = _easingProvider.GetEasingFunction(step?.Easing ?? EasingType.EaseOut)
            };
        }

        private void ApplySplitAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var splitType = step.SplitType;
            
            switch (splitType)
            {
                case SplitType.Horizontal:
                    ApplyHorizontalSplitAnimation(target, step);
                    break;
                case SplitType.Vertical:
                    ApplyVerticalSplitAnimation(target, step);
                    break;
                case SplitType.Both:
                    ApplyBothSplitAnimation(target, step);
                    break;
            }
        }

        private void ApplyHorizontalSplitAnimation(FrameworkElement target, AnimationStep step)
        {
            var topAnimation = new DoubleAnimation
            {
                From = -target.ActualHeight / 2,
                To = 0,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            var bottomAnimation = new DoubleAnimation
            {
                From = target.ActualHeight / 2,
                To = 0,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };
        }

        private void ApplyVerticalSplitAnimation(FrameworkElement target, AnimationStep step)
        {
            var leftAnimation = new DoubleAnimation
            {
                From = -target.ActualWidth / 2,
                To = 0,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            var rightAnimation = new DoubleAnimation
            {
                From = target.ActualWidth / 2,
                To = 0,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };
        }

        private void ApplyBothSplitAnimation(FrameworkElement target, AnimationStep step)
        {
        }

        private void ApplyDiamondAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyCircleAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyBoxAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyWaveAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyBoomAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyPulseAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyGrowAndTurnAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyBounceAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplySwivelAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyWiggleAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplySpinAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyExitAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            switch (step.ExitEffect)
            {
                case ExitEffect.Fade:
                    ApplyFadeOutAnimation(target, step, clock);
                    break;
                case ExitEffect.FlyOut:
                    ApplyFlyOutAnimation(target, step, clock);
                    break;
                case ExitEffect.Zoom:
                    ApplyZoomOutAnimation(target, step, clock);
                    break;
                case ExitEffect.Wipe:
                    ApplyWipeOutAnimation(target, step, clock);
                    break;
                case ExitEffect.Split:
                    ApplySplitOutAnimation(target, step, clock);
                    break;
                case ExitEffect.Shrink:
                    ApplyShrinkAnimation(target, step, clock);
                    break;
                case ExitEffect.Dissolve:
                    ApplyDissolveAnimation(target, step, clock);
                    break;
            }
        }

        private void ApplyFadeOutAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            target.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void ApplyFlyOutAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyZoomOutAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyWipeOutAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplySplitOutAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyShrinkAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyDissolveAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
        }

        private void ApplyEmphasisAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            switch (step.EmphasisEffect)
            {
                case EmphasisEffect.Pulse:
                    ApplyPulseEmphasisAnimation(target, step);
                    break;
                case EmphasisEffect.ColorPulse:
                    ApplyColorPulseAnimation(target, step);
                    break;
                case EmphasisEffect.Grow:
                    ApplyGrowEmphasisAnimation(target, step);
                    break;
                case EmphasisEffect.Spin:
                    ApplySpinEmphasisAnimation(target, step);
                    break;
                case EmphasisEffect.Flash:
                    ApplyFlashEmphasisAnimation(target, step);
                    break;
                case EmphasisEffect.Teeter:
                    ApplyTeeterEmphasisAnimation(target, step);
                    break;
            }
        }

        private void ApplyPulseEmphasisAnimation(FrameworkElement target, AnimationStep step)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(0.25)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0.5)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(0.75)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1)));
            
            animation.Duration = step.Duration;
            animation.RepeatBehavior = new RepeatBehavior(3);
        }

        private void ApplyColorPulseAnimation(FrameworkElement target, AnimationStep step)
        {
        }

        private void ApplyGrowEmphasisAnimation(FrameworkElement target, AnimationStep step)
        {
        }

        private void ApplySpinEmphasisAnimation(FrameworkElement target, AnimationStep step)
        {
        }

        private void ApplyFlashEmphasisAnimation(FrameworkElement target, AnimationStep step)
        {
        }

        private void ApplyTeeterEmphasisAnimation(FrameworkElement target, AnimationStep step)
        {
        }

        private void ApplyMotionAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var path = step.MotionPath;
            if (path == null) return;

            var pathAnimation = new DoubleAnimationUsingPath
            {
                PathGeometry = path.ToPathGeometry(),
                Source = PathAnimationSource.X
            };
        }

        private void ApplyScaleAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var fromScale = step.FromScale;
            var toScale = step.ToScale;

            var scaleXAnimation = new DoubleAnimation
            {
                From = fromScale,
                To = toScale,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            var scaleYAnimation = new DoubleAnimation
            {
                From = fromScale,
                To = toScale,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
            };

            ApplyScaleTransformAnimation(target, scaleXAnimation, scaleYAnimation);
        }

        private void ApplyRotationAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var angleAnimation = new DoubleAnimation
            {
                From = step.FromAngle,
                To = step.ToAngle,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing),
                AutoReverse = step.AutoReverse
            };

            ApplyRotationTransformAnimation(target, angleAnimation);
        }

        private void ApplyColorAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            if (target is TextBlock textBlock)
            {
                var colorAnimation = new ColorAnimation
                {
                    From = step.FromColor,
                    To = step.ToColor,
                    Duration = step.Duration,
                    BeginTime = step.Delay,
                    EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
                };

                textBlock.BeginAnimation(TextBlock.ForegroundProperty, colorAnimation);
            }
            else if (target is Control control)
            {
                var colorAnimation = new ColorAnimation
                {
                    From = step.FromColor,
                    To = step.ToColor,
                    Duration = step.Duration,
                    BeginTime = step.Delay,
                    EasingFunction = _easingProvider.GetEasingFunction(step.Easing)
                };

                control.BeginAnimation(Control.BackgroundProperty, colorAnimation);
            }
        }

        private void ApplyTransparencyAnimation(FrameworkElement target, AnimationStep step, AnimationClock clock)
        {
            var opacityAnimation = new DoubleAnimation
            {
                From = step.FromOpacity,
                To = step.ToOpacity,
                Duration = step.Duration,
                BeginTime = step.Delay,
                EasingFunction = _easingProvider.GetEasingFunction(step.Easing),
                AutoReverse = step.AutoReverse
            };

            target.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
        }

        private void ApplyTransformAnimation(FrameworkElement target, DoubleAnimation horizontalAnimation, DoubleAnimation verticalAnimation)
        {
        }

        private void ApplyScaleTransformAnimation(FrameworkElement target, DoubleAnimation scaleXAnimation, DoubleAnimation scaleYAnimation)
        {
        }

        private void ApplyRotationTransformAnimation(FrameworkElement target, DoubleAnimation angleAnimation)
        {
        }

        private void ApplyClipAnimation(FrameworkElement target, Timeline animation)
        {
        }

        public AnimationSequence GetSequence(string name)
        {
            return _sequences.TryGetValue(name, out var sequence) ? sequence : null;
        }

        public IEnumerable<string> GetAllSequenceNames()
        {
            return _sequences.Keys;
        }

        public void RemoveSequence(string name)
        {
            _sequences.Remove(name);
        }

        public void ClearAllSequences()
        {
            _sequences.Clear();
        }

        public AnimationStep CreatePresetStep(PresetAnimation preset)
        {
            return preset switch
            {
                PresetAnimation.FadeIn => new AnimationStep
                {
                    AnimationType = AnimationType.Entrance,
                    EntranceEffect = EntranceEffect.Fade,
                    Duration = TimeSpan.FromSeconds(0.5)
                },
                PresetAnimation.FlyInLeft => new AnimationStep
                {
                    AnimationType = AnimationType.Entrance,
                    EntranceEffect = EntranceEffect.FlyIn,
                    Direction = Direction.Left,
                    Duration = TimeSpan.FromSeconds(0.5)
                },
                PresetAnimation.FlyInRight => new AnimationStep
                {
                    AnimationType = AnimationType.Entrance,
                    EntranceEffect = EntranceEffect.FlyIn,
                    Direction = Direction.Right,
                    Duration = TimeSpan.FromSeconds(0.5)
                },
                PresetAnimation.ZoomIn => new AnimationStep
                {
                    AnimationType = AnimationType.Entrance,
                    EntranceEffect = EntranceEffect.Zoom,
                    Duration = TimeSpan.FromSeconds(0.5)
                },
                PresetAnimation.Bounce => new AnimationStep
                {
                    AnimationType = AnimationType.Emphasis,
                    EmphasisEffect = EmphasisEffect.Teeter,
                    Duration = TimeSpan.FromSeconds(1)
                },
                _ => new AnimationStep()
            };
        }
    }

    public class AnimationSequence
    {
        public string Name { get; set; }
        public AnimationSequenceOptions Options { get; set; }
        public List<AnimationStep> Steps { get; set; }
        public bool IsPlaying { get; set; }
        public int CurrentStep { get; set; }
    }

    public class AnimationSequenceOptions
    {
        public bool AutoStart { get; set; }
        public bool Loop { get; set; }
        public int RepeatCount { get; set; }
        public TimeSpan? StartDelay { get; set; }
        public AnimationTrigger Trigger { get; set; } = AnimationTrigger.OnClick;
    }

    public enum AnimationTrigger
    {
        OnClick,
        WithPrevious,
        AfterPrevious
    }

    public class AnimationStep
    {
        public int Index { get; set; }
        public AnimationType AnimationType { get; set; }
        public TimeSpan Delay { get; set; }
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(0.3);
        public EasingType Easing { get; set; } = EasingType.EaseOut;
        public bool AutoReverse { get; set; }

        public EntranceEffect EntranceEffect { get; set; }
        public ExitEffect ExitEffect { get; set; }
        public EmphasisEffect EmphasisEffect { get; set; }
        public MotionPath MotionPath { get; set; }

        public Direction Direction { get; set; } = Direction.Left;
        public SplitType SplitType { get; set; }

        public double FromScale { get; set; } = 0;
        public double ToScale { get; set; } = 1;
        public double FromAngle { get; set; }
        public double ToAngle { get; set; }
        public Color FromColor { get; set; }
        public Color ToColor { get; set; }
        public double FromOpacity { get; set; } = 1;
        public double ToOpacity { get; set; } = 1;

        public string Description { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public enum AnimationType
    {
        Entrance,
        Exit,
        Emphasis,
        Motion,
        Scale,
        Rotation,
        Color,
        Transparency
    }

    public enum EntranceEffect
    {
        Fade,
        FlyIn,
        Zoom,
        Wipe,
        Split,
        Diamond,
        Circle,
        Box,
        Wave,
        Boom,
        Pulse,
        GrowAndTurn,
        Bounce,
        Swivel,
        Wiggle,
        Spin
    }

    public enum ExitEffect
    {
        Fade,
        FlyOut,
        Zoom,
        Wipe,
        Split,
        Shrink,
        Dissolve
    }

    public enum EmphasisEffect
    {
        Pulse,
        ColorPulse,
        Grow,
        Spin,
        Flash,
        Teeter
    }

    public enum Direction
    {
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum SplitType
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        EaseOutBack,
        EaseOutBounce,
        EaseOutElastic,
        EaseInBack,
        EaseInBounce,
        EaseInElastic,
        EaseInOutBack,
        EaseInOutBounce,
        EaseInOutElastic
    }

    public enum PresetAnimation
    {
        FadeIn,
        FlyInLeft,
        FlyInRight,
        FlyInTop,
        FlyInBottom,
        ZoomIn,
        ZoomOut,
        Bounce,
        Spin,
        Pulse,
        Flash,
        WipeLeft,
        WipeRight,
        WipeUp,
        WipeDown
    }

    public class MotionPath
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public List<Point> ControlPoints { get; set; }
        public MotionPathType Type { get; set; } = MotionPathType.Line;

        public PathGeometry ToPathGeometry()
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = StartPoint };

            switch (Type)
            {
                case MotionPathType.Line:
                    figure.Segments.Add(new LineSegment(EndPoint, true));
                    break;
                case MotionPathType.Arc:
                    figure.Segments.Add(new ArcSegment(EndPoint, new Size(100, 100), 0, false, SweepDirection.Clockwise, true));
                    break;
                case MotionPathType.Curve:
                    if (ControlPoints.Count >= 2)
                    {
                        figure.Segments.Add(new BezierSegment(ControlPoints[0], ControlPoints[1], EndPoint, true));
                    }
                    break;
            }

            geometry.Figures.Add(figure);
            return geometry;
        }
    }

    public enum MotionPathType
    {
        Line,
        Arc,
        Curve,
        Custom
    }

    public class EasingFunctionProvider
    {
        public EasingFunctionBase GetEasingFunction(EasingType type)
        {
            return type switch
            {
                EasingType.Linear => null,
                EasingType.EaseIn => new CubicEase { EasingMode = EasingMode.EaseIn },
                EasingType.EaseOut => new CubicEase { EasingMode = EasingMode.EaseOut },
                EasingType.EaseInOut => new CubicEase { EasingMode = EasingMode.EaseInOut },
                EasingType.EaseOutBack => new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 1 },
                EasingType.EaseOutBounce => new BounceEase { EasingMode = EasingMode.EaseOut, Bounces = 3 },
                EasingType.EaseOutElastic => new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 3 },
                EasingType.EaseInBack => new BackEase { EasingMode = EasingMode.EaseIn, Amplitude = 1 },
                EasingType.EaseInBounce => new BounceEase { EasingMode = EasingMode.EaseIn, Bounces = 3 },
                EasingType.EaseInElastic => new ElasticEase { EasingMode = EasingMode.EaseIn, Oscillations = 3 },
                EasingType.EaseInOutBack => new BackEase { EasingMode = EasingMode.EaseInOut, Amplitude = 1 },
                EasingType.EaseInOutBounce => new BounceEase { EasingMode = EasingMode.EaseInOut, Bounces = 3 },
                EasingType.EaseInOutElastic => new ElasticEase { EasingMode = EasingMode.EaseInOut, Oscillations = 3 },
                _ => new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }
    }

    public class AnimationTimelineFactory
    {
        public Timeline CreateTimeline(AnimationStep step, FrameworkElement target)
        {
            return step.AnimationType switch
            {
                AnimationType.Entrance => CreateEntranceTimeline(step, target),
                AnimationType.Exit => CreateExitTimeline(step, target),
                AnimationType.Emphasis => CreateEmphasisTimeline(step, target),
                AnimationType.Scale => CreateScaleTimeline(step),
                AnimationType.Rotation => CreateRotationTimeline(step),
                AnimationType.Color => CreateColorTimeline(step),
                AnimationType.Transparency => CreateTransparencyTimeline(step),
                _ => null
            };
        }

        private Timeline CreateEntranceTimeline(AnimationStep step, FrameworkElement target)
        {
            return step.EntranceEffect switch
            {
                EntranceEffect.Fade => CreateFadeTimeline(step, true),
                EntranceEffect.Zoom => CreateZoomTimeline(step, true),
                _ => CreateFadeTimeline(step, true)
            };
        }

        private Timeline CreateExitTimeline(AnimationStep step, FrameworkElement target)
        {
            return step.ExitEffect switch
            {
                ExitEffect.Fade => CreateFadeTimeline(step, false),
                ExitEffect.Zoom => CreateZoomTimeline(step, false),
                _ => CreateFadeTimeline(step, false)
            };
        }

        private Timeline CreateEmphasisTimeline(AnimationStep step, FrameworkElement target)
        {
            var storyboard = new Storyboard();

            switch (step.EmphasisEffect)
            {
                case EmphasisEffect.Pulse:
                    var pulseAnimation = new DoubleAnimationUsingKeyFrames();
                    pulseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0)));
                    pulseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.1, KeyTime.FromPercent(0.5)));
                    pulseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1)));
                    pulseAnimation.Duration = step.Duration;
                    storyboard.Children.Add(pulseAnimation);
                    break;
                case EmphasisEffect.Teeter:
                    var teeterAnimation = new DoubleAnimationUsingKeyFrames();
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)));
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-5, KeyTime.FromPercent(0.1)));
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(5, KeyTime.FromPercent(0.2)));
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-3, KeyTime.FromPercent(0.3)));
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(3, KeyTime.FromPercent(0.4)));
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-1, KeyTime.FromPercent(0.5)));
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0.6)));
                    teeterAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0.7)));
                    teeterAnimation.Duration = step.Duration;
                    storyboard.Children.Add(teeterAnimation);
                    break;
            }

            return storyboard;
        }

        private Timeline CreateScaleTimeline(AnimationStep step)
        {
            var animation = new DoubleAnimation
            {
                From = step.FromScale,
                To = step.ToScale,
                Duration = step.Duration,
                BeginTime = step.Delay,
                AutoReverse = step.AutoReverse
            };
            return animation;
        }

        private Timeline CreateRotationTimeline(AnimationStep step)
        {
            var animation = new DoubleAnimation
            {
                From = step.FromAngle,
                To = step.ToAngle,
                Duration = step.Duration,
                BeginTime = step.Delay,
                AutoReverse = step.AutoReverse
            };
            return animation;
        }

        private Timeline CreateColorTimeline(AnimationStep step)
        {
            var animation = new ColorAnimation
            {
                From = step.FromColor,
                To = step.ToColor,
                Duration = step.Duration,
                BeginTime = step.Delay
            };
            return animation;
        }

        private Timeline CreateTransparencyTimeline(AnimationStep step)
        {
            var animation = new DoubleAnimation
            {
                From = step.FromOpacity,
                To = step.ToOpacity,
                Duration = step.Duration,
                BeginTime = step.Delay,
                AutoReverse = step.AutoReverse
            };
            return animation;
        }

        private Timeline CreateFadeTimeline(AnimationStep step, bool isEntrance)
        {
            return new DoubleAnimation
            {
                From = isEntrance ? 0 : 1,
                To = isEntrance ? 1 : 0,
                Duration = step.Duration,
                BeginTime = step.Delay
            };
        }

        private Timeline CreateZoomTimeline(AnimationStep step, bool isEntrance)
        {
            return new DoubleAnimation
            {
                From = isEntrance ? 0 : 1,
                To = isEntrance ? 1 : 0,
                Duration = step.Duration,
                BeginTime = step.Delay
            };
        }
    }

    public class TransitionManager
    {
        private readonly Dictionary<string, TransitionEffect> _effects;

        public TransitionManager()
        {
            _effects = new Dictionary<string, TransitionEffect>();
            InitializeDefaultEffects();
        }

        private void InitializeDefaultEffects()
        {
            _effects["Fade"] = new TransitionEffect
            {
                Name = "Fade",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Wipe"] = new TransitionEffect
            {
                Name = "Wipe",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Push"] = new TransitionEffect
            {
                Name = "Push",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Cover"] = new TransitionEffect
            {
                Name = "Cover",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Reveal"] = new TransitionEffect
            {
                Name = "Reveal",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Split"] = new TransitionEffect
            {
                Name = "Split",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Blind"] = new TransitionEffect
            {
                Name = "Blind",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Circle"] = new TransitionEffect
            {
                Name = "Circle",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Diamond"] = new TransitionEffect
            {
                Name = "Diamond",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Zoom"] = new TransitionEffect
            {
                Name = "Zoom",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Pan"] = new TransitionEffect
            {
                Name = "Pan",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            };

            _effects["Rotate"] = new TransitionEffect
            {
                Name = "Rotate",
                Duration = TimeSpan.FromSeconds(0.5),
                CreateAnimation = () => new DoubleAnimation(0, 360, TimeSpan.FromSeconds(0.5))
            };
        }

        public TransitionEffect GetEffect(string name)
        {
            return _effects.TryGetValue(name, out var effect) ? effect : null;
        }

        public IEnumerable<string> GetAllEffectNames()
        {
            return _effects.Keys;
        }

        public void AddCustomEffect(TransitionEffect effect)
        {
            if (effect != null && !string.IsNullOrEmpty(effect.Name))
            {
                _effects[effect.Name] = effect;
            }
        }

        public void RemoveEffect(string name)
        {
            _effects.Remove(name);
        }

        public void ApplyTransition(FrameworkElement oldSlide, FrameworkElement newSlide, string effectName, Direction direction = Direction.Left)
        {
            var effect = GetEffect(effectName);
            if (effect == null) return;

            var animation = effect.CreateAnimation();
            if (animation is DoubleAnimation doubleAnimation)
            {
                doubleAnimation.Completed += (s, e) =>
                {
                    oldSlide.Visibility = Visibility.Collapsed;
                };
            }
        }
    }

    public class TransitionEffect
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public EasingType Easing { get; set; } = EasingType.EaseOut;
        public Direction Direction { get; set; } = Direction.Left;
        public Func<Timeline> CreateAnimation { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}

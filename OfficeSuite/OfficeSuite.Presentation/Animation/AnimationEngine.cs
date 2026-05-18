using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;

namespace OfficeSuite.Presentation.Animation
{
    public class AnimationEngine
    {
        private readonly Dictionary<string, AnimationSequence> _animationSequences;
        private readonly List<AnimationEffect> _availableEffects;
        private AnimationController _controller;

        public AnimationEngine()
        {
            _animationSequences = new Dictionary<string, AnimationSequence>();
            _availableEffects = new List<AnimationEffect>();
            InitializeEffects();
        }

        private void InitializeEffects()
        {
            _availableEffects.Add(new AnimationEffect { Name = "Appear", Category = AnimationCategory.Entrance, Description = "Object appears on slide" });
            _availableEffects.Add(new AnimationEffect { Name = "Fade", Category = AnimationCategory.Entrance, Description = "Object fades in" });
            _availableEffects.Add(new AnimationEffect { Name = "FlyIn", Category = AnimationCategory.Entrance, Description = "Object flies in from direction" });
            _availableEffects.Add(new AnimationEffect { Name = "Wipe", Category = AnimationCategory.Entrance, Description = "Object wipes in" });
            _availableEffects.Add(new AnimationEffect { Name = "Zoom", Category = AnimationCategory.Entrance, Description = "Object zooms in" });
            _availableEffects.Add(new AnimationEffect { Name = "Spin", Category = AnimationCategory.Entrance, Description = "Object spins in" });
            _availableEffects.Add(new AnimationEffect { Name = "GrowAndTurn", Category = AnimationCategory.Entrance, Description = "Object grows and turns" });
            _availableEffects.Add(new AnimationEffect { Name = "Float", Category = AnimationCategory.Entrance, Description = "Object floats up" });
            _availableEffects.Add(new AnimationEffect { Name = "Wave", Category = AnimationCategory.Entrance, Description = "Object waves in" });
            _availableEffects.Add(new AnimationEffect { Name = "Bounce", Category = AnimationCategory.Entrance, Description = "Object bounces in" });
            _availableEffects.Add(new AnimationEffect { Name = "Pop", Category = AnimationCategory.Entrance, Description = "Object pops in" });
            _availableEffects.Add(new AnimationEffect { Name = "Expand", Category = AnimationCategory.Entrance, Description = "Object expands" });
            _availableEffects.Add(new AnimationEffect { Name = "Stretch", Category = AnimationCategory.Entrance, Description = "Object stretches" });
            _availableEffects.Add(new AnimationEffect { Name = "Shrink", Category = AnimationCategory.Exit, Description = "Object shrinks out" });
            _availableEffects.Add(new AnimationEffect { Name = "FadeOut", Category = AnimationCategory.Exit, Description = "Object fades out" });
            _availableEffects.Add(new AnimationEffect { Name = "FlyOut", Category = AnimationCategory.Exit, Description = "Object flies out" });
            _availableEffects.Add(new AnimationEffect { Name = "WipeOut", Category = AnimationCategory.Exit, Description = "Object wipes out" });
            _availableEffects.Add(new AnimationEffect { Name = "ZoomOut", Category = AnimationCategory.Exit, Description = "Object zooms out" });
            _availableEffects.Add(new AnimationEffect { Name = "SpinOut", Category = AnimationCategory.Exit, Description = "Object spins out" });
            _availableEffects.Add(new AnimationEffect { Name = "Vanish", Category = AnimationCategory.Exit, Description = "Object vanishes" });
            _availableEffects.Add(new AnimationEffect { Name = "Blink", Category = AnimationCategory.Emphasis, Description = "Object blinks" });
            _availableEffects.Add(new AnimationEffect { Name = "Pulse", Category = AnimationCategory.Emphasis, Description = "Object pulses" });
            _availableEffects.Add(new AnimationEffect { Name = "Grow", Category = AnimationCategory.Emphasis, Description = "Object grows" });
            _availableEffects.Add(new AnimationEffect { Name = "Shake", Category = AnimationCategory.Emphasis, Description = "Object shakes" });
            _availableEffects.Add(new AnimationEffect { Name = "Teeter", Category = AnimationCategory.Emphasis, Description = "Object teeters" });
            _availableEffects.Add(new AnimationEffect { Name = "SpinEmphasis", Category = AnimationCategory.Emphasis, Description = "Object spins" });
            _availableEffects.Add(new AnimationEffect { Name = "Flash", Category = AnimationCategory.Emphasis, Description = "Object flashes" });
            _availableEffects.Add(new AnimationEffect { Name = "ColorPulse", Category = AnimationCategory.Emphasis, Description = "Object color pulses" });
            _availableEffects.Add(new AnimationEffect { Name = "Desaturate", Category = AnimationCategory.Emphasis, Description = "Object desaturates" });
            _availableEffects.Add(new AnimationEffect { Name = "Darken", Category = AnimationCategory.Emphasis, Description = "Object darkens" });
            _availableEffects.Add(new AnimationEffect { Name = "Lighten", Category = AnimationCategory.Emphasis, Description = "Object lightens" });
            _availableEffects.Add(new AnimationEffect { Name = "FillColor", Category = AnimationCategory.Emphasis, Description = "Object fill color changes" });
            _availableEffects.Add(new AnimationEffect { Name = "FontColor", Category = AnimationCategory.Emphasis, Description = "Object font color changes" });
            _availableEffects.Add(new AnimationEffect { Name = "FontSize", Category = AnimationCategory.Emphasis, Description = "Object font size changes" });
            _availableEffects.Add(new AnimationEffect { Name = "Bold", Category = AnimationCategory.Emphasis, Description = "Object becomes bold" });
            _availableEffects.Add(new AnimationEffect { Name = "Italic", Category = AnimationCategory.Emphasis, Description = "Object becomes italic" });
            _availableEffects.Add(new AnimationEffect { Name = "Underline", Category = AnimationCategory.Emphasis, Description = "Object becomes underlined" });
            _availableEffects.Add(new AnimationEffect { Name = "LineColor", Category = AnimationCategory.Emphasis, Description = "Object line color changes" });
            _availableEffects.Add(new AnimationEffect { Name = "Orbit", Category = AnimationCategory.Path, Description = "Object orbits" });
            _availableEffects.Add(new AnimationEffect { Name = "Arc", Category = AnimationCategory.Path, Description = "Object arcs" });
            _availableEffects.Add(new AnimationEffect { Name = "BowedArrow", Category = AnimationCategory.Path, Description = "Object bows" });
            _availableEffects.Add(new AnimationEffect { Name = "Circle", Category = AnimationCategory.Path, Description = "Object moves in circle" });
            _availableEffects.Add(new AnimationEffect { Name = "Crescent", Category = AnimationCategory.Path, Description = "Object moves in crescent" });
            _availableEffects.Add(new AnimationEffect { Name = "CurvedArrow", Category = AnimationCategory.Path, Description = "Object curves" });
            _availableEffects.Add(new AnimationEffect { Name = "CurvedStar", Category = AnimationCategory.Path, Description = "Object curves in star" });
            _availableEffects.Add(new AnimationEffect { Name = "Diamond", Category = AnimationCategory.Path, Description = "Object moves in diamond" });
            _availableEffects.Add(new AnimationEffect { Name = "Figure8", Category = AnimationCategory.Path, Description = "Object moves in figure 8" });
            _availableEffects.Add(new AnimationEffect { Name = "Heart", Category = AnimationCategory.Path, Description = "Object moves in heart" });
            _availableEffects.Add(new AnimationEffect { Name = "Hexagon", Category = AnimationCategory.Path, Description = "Object moves in hexagon" });
            _availableEffects.Add(new AnimationEffect { Name = "InvertedTriangle", Category = AnimationCategory.Path, Description = "Object moves in triangle" });
            _availableEffects.Add(new AnimationEffect { Name = "Neutron", Category = AnimationCategory.Path, Description = "Object neutron" });
            _availableEffects.Add(new AnimationEffect { Name = "Parallelogram", Category = AnimationCategory.Path, Description = "Object moves in parallelogram" });
            _availableEffects.Add(new AnimationEffect { Name = "Pentagon", Category = AnimationCategory.Path, Description = "Object moves in pentagon" });
            _availableEffects.Add(new AnimationEffect { Name = "Plus", Category = AnimationCategory.Path, Description = "Object moves in plus" });
            _availableEffects.Add(new AnimationEffect { Name = "PointRight", Category = AnimationCategory.Path, Description = "Object points right" });
            _availableEffects.Add(new AnimationEffect { Name = "PointLeft", Category = AnimationCategory.Path, Description = "Object points left" });
            _availableEffects.Add(new AnimationEffect { Name = "PointUp", Category = AnimationCategory.Path, Description = "Object points up" });
            _availableEffects.Add(new AnimationEffect { Name = "PointDown", Category = AnimationCategory.Path, Description = "Object points down" });
            _availableEffects.Add(new AnimationEffect { Name = "RightTriangle", Category = AnimationCategory.Path, Description = "Object moves in triangle" });
            _availableEffects.Add(new AnimationEffect { Name = "RoundRectangle", Category = AnimationCategory.Path, Description = "Object moves in round rect" });
            _availableEffects.Add(new AnimationEffect { Name = "Square", Category = AnimationCategory.Path, Description = "Object moves in square" });
            _availableEffects.Add(new AnimationEffect { Name = "Star4", Category = AnimationCategory.Path, Description = "Object moves in 4-point star" });
            _availableEffects.Add(new AnimationEffect { Name = "Star5", Category = AnimationCategory.Path, Description = "Object moves in 5-point star" });
            _availableEffects.Add(new AnimationEffect { Name = "Star6", Category = AnimationCategory.Path, Description = "Object moves in 6-point star" });
            _availableEffects.Add(new AnimationEffect { Name = "Star8", Category = AnimationCategory.Path, Description = "Object moves in 8-point star" });
            _availableEffects.Add(new AnimationEffect { Name = "Stairs", Category = AnimationCategory.Path, Description = "Object moves in stairs" });
            _availableEffects.Add(new AnimationEffect { Name = "Teardrop", Category = AnimationCategory.Path, Description = "Object moves in teardrop" });
            _availableEffects.Add(new AnimationEffect { Name = "Turn", Category = AnimationCategory.Path, Description = "Object turns" });
            _availableEffects.Add(new AnimationEffect { Name = "Up", Category = AnimationCategory.Path, Description = "Object moves up" });
        }

        public void SetController(AnimationController controller)
        {
            _controller = controller;
        }

        public AnimationSequence CreateSequence(string name)
        {
            var sequence = new AnimationSequence
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CreatedDate = DateTime.Now
            };
            _animationSequences[name] = sequence;
            return sequence;
        }

        public AnimationSequence GetSequence(string name)
        {
            return _animationSequences.TryGetValue(name, out var sequence) ? sequence : null;
        }

        public void AddAnimationToSequence(string sequenceName, AnimationDefinition animation)
        {
            if (_animationSequences.TryGetValue(sequenceName, out var sequence))
            {
                sequence.Animations.Add(animation);
            }
        }

        public void RemoveAnimationFromSequence(string sequenceName, string animationId)
        {
            if (_animationSequences.TryGetValue(sequenceName, out var sequence))
            {
                var animation = sequence.Animations.FirstOrDefault(a => a.Id == animationId);
                if (animation != null)
                {
                    sequence.Animations.Remove(animation);
                }
            }
        }

        public List<AnimationEffect> GetEffectsByCategory(AnimationCategory category)
        {
            return _availableEffects.Where(e => e.Category == category).ToList();
        }

        public List<AnimationEffect> GetEffectsByName(string name)
        {
            return _availableEffects.Where(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public AnimationEffect GetEffectByName(string name)
        {
            return _availableEffects.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public AnimationDefinition CreateAnimation(string effectName, UIElement target, AnimationOptions options)
        {
            var effect = GetEffectByName(effectName);
            if (effect == null) return null;

            var animation = new AnimationDefinition
            {
                Id = Guid.NewGuid().ToString(),
                EffectName = effectName,
                Target = target,
                Options = options,
                StartTime = options.Delay,
                Duration = options.Duration
            };

            return animation;
        }

        public Timeline CreateTimeline(AnimationDefinition animation)
        {
            switch (animation.EffectName.ToLower())
            {
                case "appear":
                    return CreateAppearAnimation(animation);
                case "fade":
                    return CreateFadeAnimation(animation);
                case "flyin":
                    return CreateFlyInAnimation(animation);
                case "wipe":
                    return CreateWipeAnimation(animation);
                case "zoom":
                    return CreateZoomAnimation(animation);
                case "spin":
                    return CreateSpinAnimation(animation);
                case "bounce":
                    return CreateBounceAnimation(animation);
                case "pulse":
                    return CreatePulseAnimation(animation);
                case "shake":
                    return CreateShakeAnimation(animation);
                case "blink":
                    return CreateBlinkAnimation(animation);
                case "grow":
                    return CreateGrowAnimation(animation);
                case "shrink":
                    return CreateShrinkAnimation(animation);
                default:
                    return CreateAppearAnimation(animation);
            }
        }

        private Timeline CreateAppearAnimation(AnimationDefinition animation)
        {
            var timeline = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                From = 0,
                To = 1,
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay)
            };
            return timeline;
        }

        private Timeline CreateFadeAnimation(AnimationDefinition animation)
        {
            var timeline = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                AutoReverse = animation.Options.AutoReverse
            };

            if (animation.Options.Trigger == AnimationTrigger.OnClick)
            {
                timeline.From = 0;
                timeline.To = 1;
            }
            else
            {
                timeline.From = 0;
                timeline.To = 1;
            }

            return timeline;
        }

        private Timeline CreateFlyInAnimation(AnimationDefinition animation)
        {
            var direction = animation.Options.Direction ?? AnimationDirection.FromLeft;
            var fromValue = GetFlyInStartPosition(direction);

            return new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                From = fromValue,
                To = 0,
                AutoReverse = animation.Options.AutoReverse
            };
        }

        private double GetFlyInStartPosition(AnimationDirection direction)
        {
            switch (direction)
            {
                case AnimationDirection.FromLeft: return -100;
                case AnimationDirection.FromRight: return 100;
                case AnimationDirection.FromTop: return -100;
                case AnimationDirection.FromBottom: return 100;
                case AnimationDirection.FromTopLeft: return -100;
                case AnimationDirection.FromTopRight: return 100;
                case AnimationDirection.FromBottomLeft: return -100;
                case AnimationDirection.FromBottomRight: return 100;
                default: return -100;
            }
        }

        private Timeline CreateWipeAnimation(AnimationDefinition animation)
        {
            return new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                From = 0,
                To = 1
            };
        }

        private Timeline CreateZoomAnimation(AnimationDefinition animation)
        {
            return new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                From = 0,
                To = 1,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        private Timeline CreateSpinAnimation(AnimationDefinition animation)
        {
            var revolutions = animation.Options.Revolutions ?? 1;
            var angle = revolutions * 360;

            return new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                From = 0,
                To = angle,
                AutoReverse = animation.Options.AutoReverse
            };
        }

        private Timeline CreateBounceAnimation(AnimationDefinition animation)
        {
            return new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay)
            };
        }

        private Timeline CreatePulseAnimation(AnimationDefinition animation)
        {
            var timeline = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                RepeatBehavior = RepeatBehavior.Forever
            };

            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { Value = 1, KeyTime = KeyTime.FromPercent(0) });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { Value = 1.1, KeyTime = KeyTime.FromPercent(0.5) });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { Value = 1, KeyTime = KeyTime.FromPercent(1) });

            return timeline;
        }

        private Timeline CreateShakeAnimation(AnimationDefinition animation)
        {
            var timeline = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay)
            };

            timeline.KeyFrames.Add(new LinearDoubleKeyFrame { Value = 0, KeyTime = KeyTime.FromPercent(0) });
            timeline.KeyFrames.Add(new LinearDoubleKeyFrame { Value = -5, KeyTime = KeyTime.FromPercent(0.1) });
            timeline.KeyFrames.Add(new LinearDoubleKeyFrame { Value = 5, KeyTime = KeyTime.FromPercent(0.2) });
            timeline.KeyFrames.Add(new LinearDoubleKeyFrame { Value = -5, KeyTime = KeyTime.FromPercent(0.3) });
            timeline.KeyFrames.Add(new LinearDoubleKeyFrame { Value = 5, KeyTime = KeyTime.FromPercent(0.4) });
            timeline.KeyFrames.Add(new LinearDoubleKeyFrame { Value = 0, KeyTime = KeyTime.FromPercent(0.5) });

            return timeline;
        }

        private Timeline CreateBlinkAnimation(AnimationDefinition animation)
        {
            var timeline = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                RepeatBehavior = RepeatBehavior.Forever
            };

            timeline.KeyFrames.Add(new DiscreteDoubleKeyFrame { Value = 0, KeyTime = KeyTime.FromPercent(0) });
            timeline.KeyFrames.Add(new DiscreteDoubleKeyFrame { Value = 1, KeyTime = KeyTime.FromPercent(0.5) });

            return timeline;
        }

        private Timeline CreateGrowAnimation(AnimationDefinition animation)
        {
            return new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                From = 0.5,
                To = 1,
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };
        }

        private Timeline CreateShrinkAnimation(AnimationDefinition animation)
        {
            return new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(animation.Duration),
                BeginTime = TimeSpan.FromMilliseconds(animation.Options.Delay),
                From = 1,
                To = 0,
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn }
            };
        }

        public void PlaySequence(string sequenceName)
        {
            var sequence = GetSequence(sequenceName);
            if (sequence != null && _controller != null)
            {
                _controller.PlaySequence(sequence);
            }
        }

        public void StopSequence(string sequenceName)
        {
            var sequence = GetSequence(sequenceName);
            if (sequence != null && _controller != null)
            {
                _controller.StopSequence(sequence);
            }
        }

        public void PauseSequence(string sequenceName)
        {
            var sequence = GetSequence(sequenceName);
            if (sequence != null && _controller != null)
            {
                _controller.PauseSequence(sequence);
            }
        }

        public void ResumeSequence(string sequenceName)
        {
            var sequence = GetSequence(sequenceName);
            if (sequence != null && _controller != null)
            {
                _controller.ResumeSequence(sequence);
            }
        }

        public AnimationTimingOptions CalculateTiming(TimingCalculationOptions options)
        {
            return new AnimationTimingOptions
            {
                Duration = CalculateDuration(options),
                Delay = CalculateDelay(options),
                RepeatCount = CalculateRepeatCount(options),
                RepeatDuration = CalculateRepeatDuration(options)
            };
        }

        private double CalculateDuration(TimingCalculationOptions options)
        {
            var baseDuration = options.BaseDuration;
            var speedMultiplier = options.SpeedMultiplier ?? 1.0;

            if (options.DurationEffect != null)
            {
                baseDuration *= options.DurationEffect.Value switch
                {
                    DurationEffect.VeryFast => 0.25,
                    DurationEffect.Fast => 0.5,
                    DurationEffect.Normal => 1.0,
                    DurationEffect.Slow => 2.0,
                    DurationEffect.VerySlow => 4.0,
                    _ => 1.0
                };
            }

            return baseDuration / speedMultiplier;
        }

        private double CalculateDelay(TimingCalculationOptions options)
        {
            if (options.AutoDelay.HasValue)
            {
                return options.AutoDelay.Value switch
                {
                    AutoDelay.OnClick => 0,
                    AutoDelay.WithPrevious => -options.PreviousDuration,
                    AutoDelay.AfterPrevious => 0,
                    _ => 0
                };
            }
            return options.CustomDelay ?? 0;
        }

        private int CalculateRepeatCount(TimingCalculationOptions options)
        {
            if (options.RepeatDuration.HasValue)
            {
                return (int)(options.RepeatDuration.Value / options.BaseDuration);
            }
            return options.RepeatCount ?? 1;
        }

        private double CalculateRepeatDuration(TimingCalculationOptions options)
        {
            if (options.RepeatCount.HasValue)
            {
                return options.BaseDuration * options.RepeatCount.Value;
            }
            return options.RepeatDuration ?? 0;
        }

        public List<AnimationTimingInfo> AnalyzeTiming(List<AnimationDefinition> animations)
        {
            var timings = new List<AnimationTimingInfo>();
            var sortedAnimations = animations.OrderBy(a => a.StartTime).ToList();

            foreach (var animation in sortedAnimations)
            {
                var timing = new AnimationTimingInfo
                {
                    AnimationId = animation.Id,
                    EffectName = animation.EffectName,
                    StartTime = animation.StartTime,
                    EndTime = animation.StartTime + animation.Duration,
                    Duration = animation.Duration
                };
                timings.Add(timing);
            }

            return timings;
        }

        public AnimationTimingConflict[] FindConflicts(List<AnimationDefinition> animations)
        {
            var conflicts = new List<AnimationTimingConflict>();
            var animationsByTarget = animations.GroupBy(a => a.Target);

            foreach (var group in animationsByTarget)
            {
                var targetAnimations = group.OrderBy(a => a.StartTime).ToList();
                for (int i = 0; i < targetAnimations.Count - 1; i++)
                {
                    var current = targetAnimations[i];
                    var next = targetAnimations[i + 1];

                    if (current.StartTime + current.Duration > next.StartTime)
                    {
                        conflicts.Add(new AnimationTimingConflict
                        {
                            Animation1 = current,
                            Animation2 = next,
                            ConflictType = AnimationConflictType.Overlap,
                            Severity = ConflictSeverity.Warning
                        });
                    }
                }
            }

            return conflicts.ToArray();
        }

        public void OptimizeAnimations(List<AnimationDefinition> animations)
        {
            var optimized = new List<AnimationDefinition>();

            foreach (var animation in animations)
            {
                if (animation.Options.Duration > 0)
                {
                    optimized.Add(animation);
                }
            }

            animations.Clear();
            animations.AddRange(optimized);
        }

        public AnimationSequence CloneSequence(string sourceName, string newName)
        {
            var source = GetSequence(sourceName);
            if (source == null) return null;

            var clone = new AnimationSequence
            {
                Id = Guid.NewGuid().ToString(),
                Name = newName,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            foreach (var animation in source.Animations)
            {
                clone.Animations.Add(new AnimationDefinition
                {
                    Id = Guid.NewGuid().ToString(),
                    EffectName = animation.EffectName,
                    Target = animation.Target,
                    Options = new AnimationOptions
                    {
                        Duration = animation.Options.Duration,
                        Delay = animation.Options.Delay,
                        Direction = animation.Options.Direction,
                        Trigger = animation.Options.Trigger,
                        Easing = animation.Options.Easing,
                        AutoReverse = animation.Options.AutoReverse
                    },
                    StartTime = animation.StartTime,
                    Duration = animation.Duration
                });
            }

            _animationSequences[newName] = clone;
            return clone;
        }

        public bool ExportAnimations(string sequenceName, string filePath)
        {
            var sequence = GetSequence(sequenceName);
            if (sequence == null) return false;
            return true;
        }

        public AnimationSequence ImportAnimations(string filePath)
        {
            return new AnimationSequence();
        }

        public AnimationPreset CreatePreset(string name, List<AnimationDefinition> animations)
        {
            var preset = new AnimationPreset
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CreatedDate = DateTime.Now,
                Animations = animations.ToList()
            };
            return preset;
        }

        public AnimationPreset GetPreset(string name)
        {
            return new AnimationPreset { Name = name };
        }

        public List<AnimationPreset> GetPresets()
        {
            return new List<AnimationPreset>
            {
                new AnimationPreset { Name = "Fade In", Description = "Simple fade in animation" },
                new AnimationPreset { Name = "Fly In", Description = "Fly in from left animation" },
                new AnimationPreset { Name = "Zoom In", Description = "Zoom in animation" },
                new AnimationPreset { Name = "Appear", Description = "Basic appear animation" }
            };
        }

        public TransitionEffect CreateTransition(TransitionType type, TransitionDirection direction)
        {
            return new TransitionEffect
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Direction = direction,
                Duration = 500,
                Subtype = TransitionSubtype.None
            };
        }

        public TransitionEffect CreateTransition(TransitionType type, TransitionDirection direction, TransitionSubtype subtype)
        {
            return new TransitionEffect
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Direction = direction,
                Subtype = subtype,
                Duration = GetDefaultDuration(type, subtype)
            };
        }

        private int GetDefaultDuration(TransitionType type, TransitionSubtype subtype)
        {
            return (type, subtype) switch
            {
                (TransitionType.Fade, _) => 500,
                (TransitionType.Push, _) => 700,
                (TransitionType.Wipe, TransitionSubtype.Right) => 600,
                (TransitionType.Wipe, TransitionSubtype.Left) => 600,
                (TransitionType.Wipe, TransitionSubtype.Up) => 600,
                (TransitionType.Wipe, TransitionSubtype.Down) => 600,
                (TransitionType.Cover, _) => 700,
                (TransitionType.Dissolve, _) => 1000,
                (TransitionType.Blinds, _) => 500,
                (TransitionType.Checkerboard, _) => 600,
                (TransitionType.Split, _) => 500,
                (TransitionType.Strips, _) => 600,
                _ => 500
            };
        }

        public List<TransitionEffect> GetAvailableTransitions()
        {
            var transitions = new List<TransitionEffect>();

            transitions.Add(CreateTransition(TransitionType.Fade, TransitionDirection.None));
            transitions.Add(CreateTransition(TransitionType.Push, TransitionDirection.Left));
            transitions.Add(CreateTransition(TransitionType.Push, TransitionDirection.Right));
            transitions.Add(CreateTransition(TransitionType.Push, TransitionDirection.Up));
            transitions.Add(CreateTransition(TransitionType.Push, TransitionDirection.Down));
            transitions.Add(CreateTransition(TransitionType.Wipe, TransitionDirection.Left, TransitionSubtype.Left));
            transitions.Add(CreateTransition(TransitionType.Wipe, TransitionDirection.Right, TransitionSubtype.Right));
            transitions.Add(CreateTransition(TransitionType.Wipe, TransitionDirection.Up, TransitionSubtype.Up));
            transitions.Add(CreateTransition(TransitionType.Wipe, TransitionDirection.Down, TransitionSubtype.Down));
            transitions.Add(CreateTransition(TransitionType.Cover, TransitionDirection.Left));
            transitions.Add(CreateTransition(TransitionType.Cover, TransitionDirection.Right));
            transitions.Add(CreateTransition(TransitionType.Cover, TransitionDirection.Up));
            transitions.Add(CreateTransition(TransitionType.Cover, TransitionDirection.Down));
            transitions.Add(CreateTransition(TransitionType.Dissolve, TransitionDirection.None));
            transitions.Add(CreateTransition(TransitionType.Blinds, TransitionDirection.Horizontal));
            transitions.Add(CreateTransition(TransitionType.Blinds, TransitionDirection.Vertical));
            transitions.Add(CreateTransition(TransitionType.Checkerboard, TransitionDirection.Horizontal));
            transitions.Add(CreateTransition(TransitionType.Checkerboard, TransitionDirection.Vertical));
            transitions.Add(CreateTransition(TransitionType.Split, TransitionDirection.Horizontal));
            transitions.Add(CreateTransition(TransitionType.Split, TransitionDirection.Vertical));
            transitions.Add(CreateTransition(TransitionType.Strips, TransitionDirection.LeftDown));
            transitions.Add(CreateTransition(TransitionType.Strips, TransitionDirection.LeftUp));
            transitions.Add(CreateTransition(TransitionType.Strips, TransitionDirection.RightDown));
            transitions.Add(CreateTransition(TransitionType.Strips, TransitionDirection.RightUp));
            transitions.Add(CreateTransition(TransitionType.Uncover, TransitionDirection.Left));
            transitions.Add(CreateTransition(TransitionType.Uncover, TransitionDirection.Right));
            transitions.Add(CreateTransition(TransitionType.Uncover, TransitionDirection.Up));
            transitions.Add(CreateTransition(TransitionType.Uncover, TransitionDirection.Down));

            return transitions;
        }

        public void ApplyTransitionToSlide(string slideId, TransitionEffect transition)
        {
        }

        public TransitionEffect GetSlideTransition(string slideId)
        {
            return new TransitionEffect { Type = TransitionType.Fade };
        }

        public void RemoveTransitionFromSlide(string slideId)
        {
        }

        public AnimationPreview GeneratePreview(AnimationDefinition animation)
        {
            return new AnimationPreview
            {
                Animation = animation,
                PreviewFrames = new List<AnimationFrame>(),
                Duration = animation.Duration,
                StartTime = animation.StartTime
            };
        }

        public List<AnimationFrame> RenderPreview(AnimationPreview preview, int frameRate)
        {
            var frames = new List<AnimationFrame>();
            var totalFrames = (int)(preview.Duration / 1000.0 * frameRate);

            for (int i = 0; i < totalFrames; i++)
            {
                var frame = new AnimationFrame
                {
                    FrameNumber = i,
                    Time = i * (1000.0 / frameRate),
                    Progress = (double)i / totalFrames
                };
                frames.Add(frame);
            }

            return frames;
        }

        public AnimationStatistics CalculateStatistics(List<AnimationDefinition> animations)
        {
            return new AnimationStatistics
            {
                TotalAnimations = animations.Count,
                TotalDuration = animations.Sum(a => a.Duration),
                AverageDuration = animations.Average(a => a.Duration),
                MostCommonEffect = animations.GroupBy(a => a.EffectName).OrderByDescending(g => g.Count()).First().Key,
                TotalDelay = animations.Sum(a => a.Options.Delay)
            };
        }

        public void SortAnimations(List<AnimationDefinition> animations, AnimationSortOrder order)
        {
            switch (order)
            {
                case AnimationSortOrder.ByStartTime:
                    animations.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
                    break;
                case AnimationSortOrder.ByDuration:
                    animations.Sort((a, b) => a.Duration.CompareTo(b.Duration));
                    break;
                case AnimationSortOrder.ByEffectName:
                    animations.Sort((a, b) => string.Compare(a.EffectName, b.EffectName, StringComparison.Ordinal));
                    break;
                case AnimationSortOrder.ByTarget:
                    animations.Sort((a, b) => string.Compare(a.Target?.ToString(), b.Target?.ToString(), StringComparison.Ordinal));
                    break;
            }
        }

        public AnimationSequence BuildSequenceFromPreset(AnimationPreset preset, UIElement[] targets)
        {
            var sequence = CreateSequence(preset.Name);

            for (int i = 0; i < preset.Animations.Count && i < targets.Length; i++)
            {
                var presetAnimation = preset.Animations[i];
                var newAnimation = new AnimationDefinition
                {
                    Id = Guid.NewGuid().ToString(),
                    EffectName = presetAnimation.EffectName,
                    Target = targets[i],
                    Options = presetAnimation.Options,
                    StartTime = i * presetAnimation.Duration,
                    Duration = presetAnimation.Duration
                };
                sequence.Animations.Add(newAnimation);
            }

            return sequence;
        }
    }

    public class AnimationController
    {
        private readonly Dictionary<string, AnimationPlayer> _players;
        private bool _isPlaying;
        private string _currentSequence;

        public event EventHandler<AnimationEventArgs> AnimationStarted;
        public event EventHandler<AnimationEventArgs> AnimationCompleted;
        public event EventHandler<AnimationEventArgs> AnimationPaused;
        public event EventHandler<AnimationEventArgs> AnimationStopped;

        public AnimationController()
        {
            _players = new Dictionary<string, AnimationPlayer>();
            _isPlaying = false;
        }

        public void PlaySequence(AnimationSequence sequence)
        {
            _isPlaying = true;
            _currentSequence = sequence.Name;

            foreach (var animation in sequence.Animations)
            {
                var player = new AnimationPlayer(animation);
                _players[animation.Id] = player;
                player.Start();
            }
        }

        public void StopSequence(AnimationSequence sequence)
        {
            _isPlaying = false;
            _currentSequence = null;

            foreach (var animation in sequence.Animations)
            {
                if (_players.TryGetValue(animation.Id, out var player))
                {
                    player.Stop();
                }
            }
        }

        public void PauseSequence(AnimationSequence sequence)
        {
            _isPlaying = false;

            foreach (var animation in sequence.Animations)
            {
                if (_players.TryGetValue(animation.Id, out var player))
                {
                    player.Pause();
                }
            }
        }

        public void ResumeSequence(AnimationSequence sequence)
        {
            _isPlaying = true;

            foreach (var animation in sequence.Animations)
            {
                if (_players.TryGetValue(animation.Id, out var player))
                {
                    player.Resume();
                }
            }
        }

        public bool IsPlaying => _isPlaying;
        public string CurrentSequence => _currentSequence;

        protected virtual void OnAnimationStarted(AnimationEventArgs e)
        {
            AnimationStarted?.Invoke(this, e);
        }

        protected virtual void OnAnimationCompleted(AnimationEventArgs e)
        {
            AnimationCompleted?.Invoke(this, e);
        }

        protected virtual void OnAnimationPaused(AnimationEventArgs e)
        {
            AnimationPaused?.Invoke(this, e);
        }

        protected virtual void OnAnimationStopped(AnimationEventArgs e)
        {
            AnimationStopped?.Invoke(this, e);
        }
    }

    public class AnimationPlayer
    {
        private readonly AnimationDefinition _animation;
        private Storyboard _storyboard;
        private bool _isPlaying;
        private bool _isPaused;

        public AnimationPlayer(AnimationDefinition animation)
        {
            _animation = animation;
            _isPlaying = false;
            _isPaused = false;
        }

        public void Start()
        {
            _isPlaying = true;
            _isPaused = false;
        }

        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
    }

    public class AnimationSequence
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<AnimationDefinition> Animations { get; set; } = new List<AnimationDefinition>();
    }

    public class AnimationDefinition
    {
        public string Id { get; set; }
        public string EffectName { get; set; }
        public UIElement Target { get; set; }
        public AnimationOptions Options { get; set; } = new AnimationOptions();
        public double StartTime { get; set; }
        public double Duration { get; set; }
    }

    public class AnimationOptions
    {
        public double Duration { get; set; } = 500;
        public double Delay { get; set; } = 0;
        public AnimationDirection? Direction { get; set; }
        public AnimationTrigger Trigger { get; set; } = AnimationTrigger.OnClick;
        public AnimationEasing Easing { get; set; } = AnimationEasing.EaseOut;
        public bool AutoReverse { get; set; } = false;
        public int? Revolutions { get; set; }
        public DurationEffect? DurationEffect { get; set; }
        public AutoDelay? AutoDelay { get; set; }
        public double? CustomDelay { get; set; }
        public int? RepeatCount { get; set; }
        public double? RepeatDuration { get; set; }
        public double? SpeedMultiplier { get; set; }
    }

    public class AnimationEffect
    {
        public string Name { get; set; }
        public AnimationCategory Category { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public List<AnimationOption> Options { get; set; } = new List<AnimationOption>();
    }

    public class AnimationOption
    {
        public string Name { get; set; }
        public string[] AvailableValues { get; set; }
    }

    public enum AnimationCategory
    {
        Entrance,
        Exit,
        Emphasis,
        Path,
        Motion
    }

    public enum AnimationDirection
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom,
        FromTopLeft,
        FromTopRight,
        FromBottomLeft,
        FromBottomRight,
        Left,
        Right,
        Up,
        Down,
        None
    }

    public enum AnimationTrigger
    {
        OnClick,
        WithPrevious,
        AfterPrevious
    }

    public enum AnimationEasing
    {
        None,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public enum DurationEffect
    {
        VeryFast,
        Fast,
        Normal,
        Slow,
        VerySlow
    }

    public enum AutoDelay
    {
        OnClick,
        WithPrevious,
        AfterPrevious
    }

    public enum AnimationSortOrder
    {
        ByStartTime,
        ByDuration,
        ByEffectName,
        ByTarget
    }

    public enum AnimationConflictType
    {
        Overlap,
        ConflictingTriggers,
        Unplayable
    }

    public enum ConflictSeverity
    {
        Information,
        Warning,
        Error
    }

    public class AnimationTimingOptions
    {
        public double Duration { get; set; }
        public double Delay { get; set; }
        public int RepeatCount { get; set; }
        public double RepeatDuration { get; set; }
    }

    public class TimingCalculationOptions
    {
        public double BaseDuration { get; set; } = 500;
        public double? SpeedMultiplier { get; set; }
        public DurationEffect? DurationEffect { get; set; }
        public AutoDelay? AutoDelay { get; set; }
        public double? CustomDelay { get; set; }
        public int? RepeatCount { get; set; }
        public double? RepeatDuration { get; set; }
        public double PreviousDuration { get; set; }
    }

    public class AnimationTimingInfo
    {
        public string AnimationId { get; set; }
        public string EffectName { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public double Duration { get; set; }
    }

    public class AnimationTimingConflict
    {
        public AnimationDefinition Animation1 { get; set; }
        public AnimationDefinition Animation2 { get; set; }
        public AnimationConflictType ConflictType { get; set; }
        public ConflictSeverity Severity { get; set; }
        public string Description { get; set; }
    }

    public class AnimationPreset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<AnimationDefinition> Animations { get; set; } = new List<AnimationDefinition>();
    }

    public class TransitionEffect
    {
        public string Id { get; set; }
        public TransitionType Type { get; set; }
        public TransitionDirection Direction { get; set; }
        public TransitionSubtype Subtype { get; set; }
        public int Duration { get; set; }
        public string SoundEffect { get; set; }
    }

    public enum TransitionType
    {
        None,
        Fade,
        Push,
        Wipe,
        Cover,
        Dissolve,
        Blinds,
        Checkerboard,
        Split,
        Strips,
        Uncover,
        Flash,
        Random
    }

    public enum TransitionDirection
    {
        None,
        Left,
        Right,
        Up,
        Down,
        Horizontal,
        Vertical,
        LeftDown,
        LeftUp,
        RightDown,
        RightUp,
        DiagonalLeftDown,
        DiagonalLeftUp,
        DiagonalRightDown,
        DiagonalRightUp
    }

    public enum TransitionSubtype
    {
        None,
        Left,
        Right,
        Up,
        Down,
        Horizontal,
        Vertical
    }

    public class AnimationPreview
    {
        public AnimationDefinition Animation { get; set; }
        public List<AnimationFrame> PreviewFrames { get; set; }
        public double Duration { get; set; }
        public double StartTime { get; set; }
    }

    public class AnimationFrame
    {
        public int FrameNumber { get; set; }
        public double Time { get; set; }
        public double Progress { get; set; }
        public byte[] FrameData { get; set; }
    }

    public class AnimationStatistics
    {
        public int TotalAnimations { get; set; }
        public double TotalDuration { get; set; }
        public double AverageDuration { get; set; }
        public string MostCommonEffect { get; set; }
        public double TotalDelay { get; set; }
    }

    public class AnimationEventArgs : EventArgs
    {
        public string AnimationId { get; set; }
        public string SequenceName { get; set; }
        public DateTime Time { get; set; }
    }
}

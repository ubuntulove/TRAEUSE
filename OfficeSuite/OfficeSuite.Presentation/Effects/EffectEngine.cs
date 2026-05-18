using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeSuite.Presentation.Effects
{
    public enum EffectCategory
    {
        Entrance,
        Exit,
        Emphasis,
        Motion,
        Transition
    }

    public enum EffectType
    {
        Fade,
        Fly,
        Push,
        Wipe,
        Expand,
        Contract,
        Spin,
        Color,
        GrowAndTurn,
        Zoom,
        Float,
        Swivel,
        Bounce,
        Flash,
        Pulse,
        Shimmer,
        Sparkle,
        Desaturate,
        Darken,
        Lighten,
        Reveal,
        Cover,
        Uncover,
        Strokes,
        Wedge,
        Peel,
        Split,
        Exchange,
        EfCubes,
        FerrisWheel,
        Flip,
        Gallery,
        Prism,
        Rotate,
        Shapes,
        Stir,
        Thumbnail,
        Twister,
        Vortex,
        Warp,
        Ascend,
        Blast,
        Blur,
        BoldReveal,
        BottomFlip,
        BounceEnd,
        BrightReel,
        BrownReel,
        Burst,
        Camera,
        Canvas,
        Canyon,
        CardFlip,
        CenterRevolve,
        CenterRipple,
        ChangeArc,
        ChangePath,
        Charcoal,
        Chop,
        CircleOut,
        Comb,
        Confetti,
        CoralReveal,
        Crash,
        credits,
        Crumble,
        Crystalize,
        Cup,
        CustomAnimation,
        CustomAnimation1,
        CustomAnimation2,
        DashOut,
        Decline,
        Disappear,
        Distort,
        Divest,
        Dorian,
        DoubleStar,
        Drift,
        EaseIn,
        EaseOut,
        EndSilver,
        Explode,
        FadedSwivel,
        FadedZoom,
        Fall,
        Feather,
        Fisheye,
        FlashBulb,
        FlashOnce,
        FlipEnd,
        FlipStart,
        FloatBottom,
        FloatTop,
        Flowers,
        FlyTop,
        Fold,
        Frame,
        Freeze,
        GalleryThumb,
        Genie,
        Glide,
        Globe,
        Gold,
        Graceful,
        Grow,
        GrowAndTurnEnd,
        Halo,
        Handwriting,
        HztlBlur,
        Iris,
        JawOpen,
        Kaleidoscope,
        LightSpeed,
        LightenReveal,
        Linda,
        Mandelbrot,
        Merge,
        Metal,
        Mixer,
        Moebius,
        Morph,
        Mosaic,
        None,
        Opposite,
        Origami,
        PageCurl,
        PageFlip,
        Pan,
        Parallax,
        Paris,
        Pause,
        PeekIn,
        PeekOut,
        PencilSketch,
        Perspective,
        Pinwheel,
        Pixelate,
        Plus,
        Polygon,
        Portal,
        Pull,
        PushEnd,
        PushStart,
        Quotes,
        RandomBars,
        RandomEffects,
        RevealAlpha,
        RevealBox,
        RevealClip,
        RevealSmthOut,
        RevHold,
        Ripple,
        RiseUp,
        RoatedClock,
        RotateAndTurn,
        Satire,
        Scatter,
        Sepia,
        Shanghai,
        Sharpen,
        Shelf,
        Shrink,
        SideFly,
        Silver,
        SimS Brits,
        SlideBottomLeft,
        SlideBottomRight,
        SlideTopLeft,
        SlideTopRight,
        SlantDown,
        SlantUp,
        SlidoFlash,
        SlotMachine,
        SmartReveal,
        SparkleText,
        SpinBottom,
        SpinIn,
        SpinLeft,
        SpinRight,
        SpinTop,
        Sponge,
        Spy,
        Square,
        Strips,
        StyleApply,
        SwivelBottom,
        SwivelTop,
        Thick,
        Thread,
        TopLeftCube,
        TopRightCube,
        Trace,
        Transparency,
        Unfold,
        Up,
        VividReveal,
        VortexEnd,
        WarpBottom,
        WarpTop,
        Wave,
        Wheel,
        Whip,
        Wink,
        XferBlinds,
        XferBoxes,
        ZoomBottom,
        ZoomTop
    }

    public enum TimingFunction
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        Elastic,
        Back
    }

    public class EffectDefinition
    {
        public string Id { get; set; }
        public EffectType Type { get; set; }
        public EffectCategory Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Duration Duration { get; set; } = new Duration { Milliseconds = 500 };
        public TimingFunction TimingFunction { get; set; } = TimingFunction.EaseInOut;
        public double Delay { get; set; } = 0;
        public bool AutoReverse { get; set; } = false;
        public int RepeatCount { get; set; } = 1;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class Duration
    {
        public int Milliseconds { get; set; } = 500;
        public double Seconds => Milliseconds / 1000.0;
    }

    public class EffectSequence
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<EffectInstance> Effects { get; set; } = new List<EffectInstance>();
        public bool IsSequential { get; set; } = true;
        public int TotalDuration => Effects.Sum(e => e.Definition.Duration.Milliseconds);

        public void AddEffect(EffectInstance effect)
        {
            Effects.Add(effect);
        }

        public void RemoveEffect(string effectId)
        {
            Effects.RemoveAll(e => e.Definition.Id == effectId);
        }

        public void Reverse()
        {
            Effects.Reverse();
        }
    }

    public class EffectInstance
    {
        public string Id { get; set; }
        public EffectDefinition Definition { get; set; }
        public int TriggerIndex { get; set; }
        public double StartTime { get; set; }
        public bool IsVisible { get; set; } = true;

        public double EndTime => StartTime + Definition.Duration.Milliseconds;
    }

    public class EffectEngine
    {
        private readonly Dictionary<string, EffectDefinition> _effectDefinitions;
        private readonly List<EffectSequence> _sequences;

        public EffectEngine()
        {
            _effectDefinitions = new Dictionary<string, EffectDefinition>();
            _sequences = new List<EffectSequence>();
            InitializeBuiltInEffects();
        }

        private void InitializeBuiltInEffects()
        {
            RegisterEffect(new EffectDefinition
            {
                Id = "fade",
                Type = EffectType.Fade,
                Category = EffectCategory.Entrance,
                Name = "Fade",
                Description = "Object fades in"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "fly",
                Type = EffectType.Fly,
                Category = EffectCategory.Entrance,
                Name = "Fly",
                Description = "Object flies in from a direction",
                Parameters = { ["direction"] = "Left" }
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "push",
                Type = EffectType.Push,
                Category = EffectCategory.Entrance,
                Name = "Push",
                Description = "Object pushes in from a direction"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "wipe",
                Type = EffectType.Wipe,
                Category = EffectCategory.Entrance,
                Name = "Wipe",
                Description = "Object wipes in"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "zoom",
                Type = EffectType.Zoom,
                Category = EffectCategory.Entrance,
                Name = "Zoom",
                Description = "Object zooms in"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "expand",
                Type = EffectType.Expand,
                Category = EffectCategory.Entrance,
                Name = "Expand",
                Description = "Object expands from center"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "spin",
                Type = EffectType.Spin,
                Category = EffectCategory.Entrance,
                Name = "Spin",
                Description = "Object spins in"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "growAndTurn",
                Type = EffectType.GrowAndTurn,
                Category = EffectCategory.Entrance,
                Name = "Grow and Turn",
                Description = "Object grows and turns while appearing"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "bounce",
                Type = EffectType.Bounce,
                Category = EffectCategory.Entrance,
                Name = "Bounce",
                Description = "Object bounces in",
                TimingFunction = TimingFunction.Bounce
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "float",
                Type = EffectType.Float,
                Category = EffectCategory.Motion,
                Name = "Float",
                Description = "Object floats up and down"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "pulse",
                Type = EffectType.Pulse,
                Category = EffectCategory.Emphasis,
                Name = "Pulse",
                Description = "Object pulses"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "shimmer",
                Type = EffectType.Shimmer,
                Category = EffectCategory.Emphasis,
                Name = "Shimmer",
                Description = "Object shimmers"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "flash",
                Type = EffectType.Flash,
                Category = EffectCategory.Emphasis,
                Name = "Flash",
                Description = "Object flashes"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "desaturate",
                Type = EffectType.Desaturate,
                Category = EffectCategory.Emphasis,
                Name = "Desaturate",
                Description = "Object becomes grayscale"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "darken",
                Type = EffectType.Darken,
                Category = EffectCategory.Emphasis,
                Name = "Darken",
                Description = "Object darkens"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "lighten",
                Type = EffectType.Lighten,
                Category = EffectCategory.Emphasis,
                Name = "Lighten",
                Description = "Object lightens"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "fadeOut",
                Type = EffectType.Fade,
                Category = EffectCategory.Exit,
                Name = "Fade Out",
                Description = "Object fades out"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "flyOut",
                Type = EffectType.Fly,
                Category = EffectCategory.Exit,
                Name = "Fly Out",
                Description = "Object flies out",
                Parameters = { ["direction"] = "Right" }
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "wipeOut",
                Type = EffectType.Wipe,
                Category = EffectCategory.Exit,
                Name = "Wipe Out",
                Description = "Object wipes out"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "contract",
                Type = EffectType.Contract,
                Category = EffectCategory.Exit,
                Name = "Contract",
                Description = "Object contracts to center"
            });

            RegisterEffect(new EffectDefinition
            {
                Id = "spinOut",
                Type = EffectType.Spin,
                Category = EffectCategory.Exit,
                Name = "Spin Out",
                Description = "Object spins out"
            });
        }

        public void RegisterEffect(EffectDefinition effect)
        {
            _effectDefinitions[effect.Id] = effect;
        }

        public EffectDefinition GetEffect(string id)
        {
            return _effectDefinitions.TryGetValue(id, out var effect) ? effect : null;
        }

        public IEnumerable<EffectDefinition> GetEffectsByCategory(EffectCategory category)
        {
            return _effectDefinitions.Values.Where(e => e.Category == category);
        }

        public EffectSequence CreateSequence(string name)
        {
            var sequence = new EffectSequence
            {
                Id = Guid.NewGuid().ToString(),
                Name = name
            };
            _sequences.Add(sequence);
            return sequence;
        }

        public void AddToSequence(string sequenceId, EffectInstance effect)
        {
            var sequence = _sequences.FirstOrDefault(s => s.Id == sequenceId);
            if (sequence != null)
            {
                effect.StartTime = sequence.TotalDuration;
                sequence.AddEffect(effect);
            }
        }

        public EffectRenderState CalculateState(EffectInstance effect, double elapsedMs)
        {
            var state = new EffectRenderState
            {
                EffectId = effect.Definition.Id,
                IsVisible = effect.IsVisible,
                Progress = 0,
                Properties = new Dictionary<string, object>()
            };

            var adjustedElapsed = elapsedMs - effect.StartTime;
            if (adjustedElapsed < 0)
                return state;

            var duration = effect.Definition.Duration.Milliseconds;
            state.Progress = Math.Min(adjustedElapsed / duration, 1.0);

            if (effect.Definition.AutoReverse && state.Progress > 0.5)
            {
                state.Progress = 1 - (state.Progress - 0.5) * 2;
            }
            else if (effect.Definition.AutoReverse)
            {
                state.Progress = state.Progress * 2;
            }

            var timingProgress = ApplyTimingFunction(state.Progress, effect.Definition.TimingFunction);
            state.Properties["TimingProgress"] = timingProgress;
            state.Properties["RawProgress"] = state.Progress;

            return state;
        }

        private double ApplyTimingFunction(double progress, TimingFunction function)
        {
            return function switch
            {
                TimingFunction.Linear => progress,
                TimingFunction.EaseIn => progress * progress,
                TimingFunction.EaseOut => 1 - Math.Pow(1 - progress, 2),
                TimingFunction.EaseInOut => progress < 0.5
                    ? 2 * progress * progress
                    : 1 - Math.Pow(-2 * progress + 2, 2) / 2,
                TimingFunction.Bounce => BounceEase(progress),
                TimingFunction.Elastic => ElasticEase(progress),
                TimingFunction.Back => BackEase(progress),
                _ => progress
            };
        }

        private double BounceEase(double t)
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            if (t < 1 / d1)
                return n1 * t * t;
            if (t < 2 / d1)
                return n1 * (t -= 1.5 / d1) * t + 0.75;
            if (t < 2.5 / d1)
                return n1 * (t -= 2.25 / d1) * t + 0.9375;
            return n1 * (t -= 2.625 / d1) * t + 0.984375;
        }

        private double ElasticEase(double t)
        {
            const double c4 = (2 * Math.PI) / 3;
            return t == 0 ? 0 : t == 1 ? 1
                : Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1;
        }

        private double BackEase(double t)
        {
            const double c1 = 1.70158;
            const double c3 = c1 + 1;
            return c3 * t * t * t - c1 * t * t;
        }
    }

    public class EffectRenderState
    {
        public string EffectId { get; set; }
        public bool IsVisible { get; set; }
        public double Progress { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public class EffectParameterType
    {
        public string Name { get; set; }
        public Type ValueType { get; set; }
        public object DefaultValue { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public string[] AllowedValues { get; set; }
    }

    public class EffectPreset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<EffectInstance> Effects { get; set; } = new List<EffectInstance>();
        public string ThumbnailPath { get; set; }
    }

    public class EffectPresetLibrary
    {
        private readonly Dictionary<string, EffectPreset> _presets = new Dictionary<string, EffectPreset>();

        public EffectPresetLibrary()
        {
            InitializePresets();
        }

        private void InitializePresets()
        {
            _presets["fadeIn"] = new EffectPreset
            {
                Id = "fadeIn",
                Name = "Fade In",
                Category = "Basic",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "fade" } } }
            };

            _presets["fadeInSlow"] = new EffectPreset
            {
                Id = "fadeInSlow",
                Name = "Fade In (Slow)",
                Category = "Basic",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "fade", Duration = new Duration { Milliseconds = 1500 } } } }
            };

            _presets["flyInLeft"] = new EffectPreset
            {
                Id = "flyInLeft",
                Name = "Fly In from Left",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "fly", Parameters = { ["direction"] = "Left" } } } }
            };

            _presets["flyInRight"] = new EffectPreset
            {
                Id = "flyInRight",
                Name = "Fly In from Right",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "fly", Parameters = { ["direction"] = "Right" } } } }
            };

            _presets["zoomIn"] = new EffectPreset
            {
                Id = "zoomIn",
                Name = "Zoom In",
                Category = "Emphasis",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "zoom" } } }
            };

            _presets["bounceIn"] = new EffectPreset
            {
                Id = "bounceIn",
                Name = "Bounce In",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "bounce", TimingFunction = TimingFunction.Bounce } } }
            };

            _presets["spinIn"] = new EffectPreset
            {
                Id = "spinIn",
                Name = "Spin In",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "spin" } } }
            };

            _presets["growAndTurn"] = new EffectPreset
            {
                Id = "growAndTurn",
                Name = "Grow and Turn",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "growAndTurn" } } }
            };

            _presets["pulse"] = new EffectPreset
            {
                Id = "pulse",
                Name = "Pulse",
                Category = "Emphasis",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "pulse" } } }
            };

            _presets["shimmer"] = new EffectPreset
            {
                Id = "shimmer",
                Name = "Shimmer",
                Category = "Emphasis",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "shimmer" } } }
            };

            _presets["flash"] = new EffectPreset
            {
                Id = "flash",
                Name = "Flash",
                Category = "Emphasis",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "flash" } } }
            };

            _presets["fadeOut"] = new EffectPreset
            {
                Id = "fadeOut",
                Name = "Fade Out",
                Category = "Exit",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "fadeOut" } } }
            };

            _presets["flyOutRight"] = new EffectPreset
            {
                Id = "flyOutRight",
                Name = "Fly Out to Right",
                Category = "Exit",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "flyOut", Parameters = { ["direction"] = "Right" } } } }
            };

            _presets["zoomOut"] = new EffectPreset
            {
                Id = "zoomOut",
                Name = "Zoom Out",
                Category = "Exit",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "zoom", Category = EffectCategory.Exit } } }
            };

            _presets["spinOut"] = new EffectPreset
            {
                Id = "spinOut",
                Name = "Spin Out",
                Category = "Exit",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "spinOut" } } }
            };

            _presets["wipe"] = new EffectPreset
            {
                Id = "wipe",
                Name = "Wipe",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "wipe" } } }
            };

            _presets["push"] = new EffectPreset
            {
                Id = "push",
                Name = "Push",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "push" } } }
            };

            _presets["expand"] = new EffectPreset
            {
                Id = "expand",
                Name = "Expand",
                Category = "Entrance",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "expand" } } }
            };

            _presets["float"] = new EffectPreset
            {
                Id = "float",
                Name = "Float",
                Category = "Motion",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "float" } } }
            };

            _presets["desaturate"] = new EffectPreset
            {
                Id = "desaturate",
                Name = "Desaturate",
                Category = "Emphasis",
                Effects = { new EffectInstance { Definition = new EffectDefinition { Id = "desaturate" } } }
            };
        }

        public EffectPreset GetPreset(string id)
        {
            return _presets.TryGetValue(id, out var preset) ? preset : null;
        }

        public IEnumerable<EffectPreset> GetPresetsByCategory(string category)
        {
            return _presets.Values.Where(p => p.Category == category);
        }

        public IEnumerable<string> GetCategories()
        {
            return _presets.Values.Select(p => p.Category).Distinct();
        }
    }

    public class EffectTimingBuilder
    {
        private readonly EffectInstance _effect;

        public EffectTimingBuilder(EffectInstance effect)
        {
            _effect = effect;
        }

        public EffectTimingBuilder WithDelay(double milliseconds)
        {
            _effect.Definition.Delay = milliseconds;
            return this;
        }

        public EffectTimingBuilder WithDuration(int milliseconds)
        {
            _effect.Definition.Duration = new Duration { Milliseconds = milliseconds };
            return this;
        }

        public EffectTimingBuilder WithTimingFunction(TimingFunction function)
        {
            _effect.Definition.TimingFunction = function;
            return this;
        }

        public EffectTimingBuilder WithAutoReverse(bool autoReverse = true)
        {
            _effect.Definition.AutoReverse = autoReverse;
            return this;
        }

        public EffectTimingBuilder WithRepeatCount(int count)
        {
            _effect.Definition.RepeatCount = count;
            return this;
        }

        public EffectTimingBuilder WithParameter(string name, object value)
        {
            _effect.Definition.Parameters[name] = value;
            return this;
        }

        public EffectInstance Build()
        {
            return _effect;
        }
    }

    public class EffectGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<EffectInstance> Effects { get; set; } = new List<EffectInstance>();
        public bool IsSequential { get; set; } = false;
        public double StartTime { get; set; }
    }

    public class EffectTrigger
    {
        public string Id { get; set; }
        public EffectTriggerType Type { get; set; }
        public string TargetId { get; set; }
        public double Delay { get; set; }
    }

    public enum EffectTriggerType
    {
        OnClick,
        OnMouseOver,
        WithPrevious,
        AfterPrevious,
        AsClick,
        OnDoubleClick,
        OnRightClick
    }
}

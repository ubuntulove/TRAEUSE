using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeSuite.Presentation.Slides
{
    public class SlideManager
    {
        private readonly List<Slide> _slides = new List<Slide>();
        private readonly Dictionary<string, SlideLayout> _layouts = new Dictionary<string, SlideLayout>();
        private readonly Dictionary<string, SlideMaster> _masters = new Dictionary<string, SlideMaster>();
        private int _currentSlideIndex;

        public event EventHandler<SlideChangedEventArgs> SlideChanged;
        public event EventHandler<SlideEventArgs> SlideAdded;
        public event EventHandler<SlideEventArgs> SlideRemoved;

        public IReadOnlyList<Slide> Slides => _slides.AsReadOnly();
        public Slide CurrentSlide => _slides.ElementAtOrDefault(_currentSlideIndex);
        public int CurrentSlideIndex => _currentSlideIndex;
        public int SlideCount => _slides.Count;

        public SlideManager()
        {
            InitializeDefaultLayouts();
        }

        private void InitializeDefaultLayouts()
        {
            AddLayout(new SlideLayout
            {
                Id = "title",
                Name = "Title Slide",
                Type = LayoutType.Title,
                Width = 9144000,
                Height = 6858000,
                Background = new SlideBackground { Fill = new FillDefinition { Type = FillType.Solid, Color = "FFFFFF" } },
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 2130425, Width = 8229600, Height = 1143000, TextAlignment = TextAlignment.Center },
                    new Placeholder { Id = 2, Type = PlaceholderType.Subtitle, PositionX = 457200, PositionY = 3429000, Width = 8229600, Height = 1143000, TextAlignment = TextAlignment.Center }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "titleAndContent",
                Name = "Title and Content",
                Type = LayoutType.TitleAndContent,
                Width = 9144000,
                Height = 6858000,
                Background = new SlideBackground { Fill = new FillDefinition { Type = FillType.Solid, Color = "FFFFFF" } },
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 609600, Width = 8229600, Height = 1143000 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Body, PositionX = 457200, PositionY = 1828800, Width = 8229600, Height = 4267200 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "twoContent",
                Name = "Two Content",
                Type = LayoutType.TwoContent,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 609600, Width = 8229600, Height = 1143000 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Body, PositionX = 457200, PositionY = 1828800, Width = 4114800, Height = 4267200 },
                    new Placeholder { Id = 3, Type = PlaceholderType.Body, PositionX = 4572000, PositionY = 1828800, Width = 4114800, Height = 4267200 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "comparison",
                Name = "Comparison",
                Type = LayoutType.Comparison,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 609600, Width = 8229600, Height = 1143000 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Body, PositionX = 457200, PositionY = 1828800, Width = 4114800, Height = 3657600 },
                    new Placeholder { Id = 3, Type = PlaceholderType.Body, PositionX = 4572000, PositionY = 1828800, Width = 4114800, Height = 3657600 },
                    new Placeholder { Id = 4, Type = PlaceholderType.Body, PositionX = 457200, PositionY = 5486400, Width = 8229600, Height = 914400 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "titleOnly",
                Name = "Title Only",
                Type = LayoutType.TitleOnly,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 609600, Width = 8229600, Height = 1143000 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "blank",
                Name = "Blank",
                Type = LayoutType.Blank,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>()
            });

            AddLayout(new SlideLayout
            {
                Id = "sectionHeader",
                Name = "Section Header",
                Type = LayoutType.SectionHeader,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 2130425, Width = 8229600, Height = 1143000 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Body, PositionX = 457200, PositionY = 3429000, Width = 8229600, Height = 2286000 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "contentWithCaption",
                Name = "Content with Caption",
                Type = LayoutType.ContentWithCaption,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Picture, PositionX = 457200, PositionY = 609600, Width = 6096000, Height = 5029200 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Title, PositionX = 6858000, PositionY = 609600, Width = 1828800, Height = 457200 },
                    new Placeholder { Id = 3, Type = PlaceholderType.Body, PositionX = 6858000, PositionY = 1066800, Width = 1828800, Height = 3657600 },
                    new Placeholder { Id = 4, Type = PlaceholderType.Footer, PositionX = 457200, PositionY = 6400800, Width = 6858000, Height = 274320 },
                    new Placeholder { Id = 5, Type = PlaceholderType.SlideNumber, PositionX = 8077200, PositionY = 6400800, Width = 609600, Height = 274320 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "pictureWithCaption",
                Name = "Picture with Caption",
                Type = LayoutType.PictureWithCaption,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Picture, PositionX = 457200, PositionY = 914400, Width = 8229600, Height = 4572000 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 5486400, Width = 8229600, Height = 457200 },
                    new Placeholder { Id = 3, Type = PlaceholderType.Body, PositionX = 457200, PositionY = 5943600, Width = 8229600, Height = 685800 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "verticalTitleAndText",
                Name = "Vertical Title and Text",
                Type = LayoutType.VerticalTitleAndText,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 609600, Width = 1371600, Height = 5486400 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Body, PositionX = 2057400, PositionY = 609600, Width = 6400800, Height = 5486400 }
                }
            });

            AddLayout(new SlideLayout
            {
                Id = "verticalTwoContent",
                Name = "Vertical Two Content",
                Type = LayoutType.VerticalTwoContent,
                Width = 9144000,
                Height = 6858000,
                Placeholders = new List<Placeholder>
                {
                    new Placeholder { Id = 1, Type = PlaceholderType.Title, PositionX = 457200, PositionY = 609600, Width = 8229600, Height = 914400 },
                    new Placeholder { Id = 2, Type = PlaceholderType.Body, PositionX = 457200, PositionY = 1600200, Width = 3962400, Height = 4876800 },
                    new Placeholder { Id = 3, Type = PlaceholderType.Body, PositionX = 4724400, PositionY = 1600200, Width = 3962400, Height = 4876800 }
                }
            });
        }

        public Slide AddSlide()
        {
            var slide = new Slide
            {
                Id = Guid.NewGuid().ToString(),
                Index = _slides.Count,
                Layout = _layouts["titleAndContent"],
                Background = new SlideBackground { Fill = new FillDefinition { Type = FillType.Solid, Color = "FFFFFF" } }
            };

            _slides.Add(slide);
            SlideAdded?.Invoke(this, new SlideEventArgs(slide));
            return slide;
        }

        public Slide AddSlide(string layoutId)
        {
            var layout = _layouts.TryGetValue(layoutId, out var l) ? l : _layouts["titleAndContent"];
            var slide = new Slide
            {
                Id = Guid.NewGuid().ToString(),
                Index = _slides.Count,
                Layout = layout,
                Background = layout.Background.Clone()
            };

            foreach (var placeholder in layout.Placeholders)
            {
                slide.Shapes.Add(new Shape
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = placeholder.Type.ToString(),
                    Type = ShapeType.Placeholder,
                    PlaceholderType = placeholder.Type,
                    PositionX = placeholder.PositionX,
                    PositionY = placeholder.PositionY,
                    Width = placeholder.Width,
                    Height = placeholder.Height,
                    ZIndex = 0
                });
            }

            _slides.Add(slide);
            SlideAdded?.Invoke(this, new SlideEventArgs(slide));
            return slide;
        }

        public void InsertSlide(int index)
        {
            var slide = new Slide
            {
                Id = Guid.NewGuid().ToString(),
                Index = index,
                Layout = _layouts["titleAndContent"]
            };

            _slides.Insert(index, slide);
            UpdateSlideIndices();
            SlideAdded?.Invoke(this, new SlideEventArgs(slide));
        }

        public void RemoveSlide(int index)
        {
            if (index < 0 || index >= _slides.Count)
                return;

            var slide = _slides[index];
            _slides.RemoveAt(index);
            UpdateSlideIndices();
            SlideRemoved?.Invoke(this, new SlideEventArgs(slide));
        }

        public void MoveSlide(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= _slides.Count ||
                toIndex < 0 || toIndex >= _slides.Count)
                return;

            var slide = _slides[fromIndex];
            _slides.RemoveAt(fromIndex);
            _slides.Insert(toIndex, slide);
            UpdateSlideIndices();
        }

        public void DuplicateSlide(int index)
        {
            if (index < 0 || index >= _slides.Count)
                return;

            var original = _slides[index];
            var duplicate = original.Clone();
            duplicate.Id = Guid.NewGuid().ToString();
            duplicate.Index = index + 1;

            _slides.Insert(index + 1, duplicate);
            UpdateSlideIndices();
        }

        public void GoToSlide(int index)
        {
            if (index < 0 || index >= _slides.Count)
                return;

            var previousIndex = _currentSlideIndex;
            _currentSlideIndex = index;
            SlideChanged?.Invoke(this, new SlideChangedEventArgs(previousIndex, _currentSlideIndex));
        }

        public void NextSlide()
        {
            if (_currentSlideIndex < _slides.Count - 1)
                GoToSlide(_currentSlideIndex + 1);
        }

        public void PreviousSlide()
        {
            if (_currentSlideIndex > 0)
                GoToSlide(_currentSlideIndex - 1);
        }

        public void AddLayout(SlideLayout layout)
        {
            _layouts[layout.Id] = layout;
        }

        public SlideLayout GetLayout(string layoutId)
        {
            return _layouts.TryGetValue(layoutId, out var layout) ? layout : null;
        }

        public IEnumerable<SlideLayout> GetAllLayouts()
        {
            return _layouts.Values;
        }

        public void AddMaster(SlideMaster master)
        {
            _masters[master.Id] = master;
        }

        public SlideMaster GetMaster(string masterId)
        {
            return _masters.TryGetValue(masterId, out var master) ? master : null;
        }

        private void UpdateSlideIndices()
        {
            for (int i = 0; i < _slides.Count; i++)
            {
                _slides[i].Index = i;
            }
        }
    }

    public class Slide
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public SlideLayout Layout { get; set; }
        public SlideBackground Background { get; set; } = new SlideBackground();
        public List<Shape> Shapes { get; set; } = new List<Shape>();
        public List<SlideTransition> Transitions { get; set; } = new List<SlideTransition>();
        public SlideTiming Timing { get; set; } = new SlideTiming();
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
        public SlideNotes Notes { get; set; }
        public bool Hidden { get; set; } = false;
        public string Name { get; set; }
        public string Title { get; set; }

        public Slide Clone()
        {
            return new Slide
            {
                Id = Guid.NewGuid().ToString(),
                Index = Index,
                Layout = Layout,
                Background = Background.Clone(),
                Shapes = Shapes.Select(s => s.Clone()).ToList(),
                Transitions = Transitions.Select(t => t.Clone()).ToList(),
                Timing = Timing.Clone(),
                CustomProperties = new Dictionary<string, string>(CustomProperties),
                Notes = Notes?.Clone(),
                Hidden = Hidden,
                Name = Name + " (Copy)",
                Title = Title
            };
        }

        public Shape AddShape(ShapeType type, double x, double y, double width, double height)
        {
            var shape = new Shape
            {
                Id = Guid.NewGuid().ToString(),
                Name = type.ToString(),
                Type = type,
                PositionX = x,
                PositionY = y,
                Width = width,
                Height = height,
                ZIndex = Shapes.Count
            };

            Shapes.Add(shape);
            return shape;
        }

        public void RemoveShape(string shapeId)
        {
            Shapes.RemoveAll(s => s.Id == shapeId);
            UpdateZIndices();
        }

        public Shape GetShape(string shapeId)
        {
            return Shapes.FirstOrDefault(s => s.Id == shapeId);
        }

        public void MoveShape(string shapeId, int newIndex)
        {
            var shape = GetShape(shapeId);
            if (shape == null)
                return;

            Shapes.Remove(shape);
            Shapes.Insert(Math.Min(newIndex, Shapes.Count), shape);
            UpdateZIndices();
        }

        private void UpdateZIndices()
        {
            for (int i = 0; i < Shapes.Count; i++)
            {
                Shapes[i].ZIndex = i;
            }
        }
    }

    public class SlideLayout
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public LayoutType Type { get; set; }
        public double Width { get; set; } = 9144000;
        public double Height { get; set; } = 6858000;
        public SlideBackground Background { get; set; } = new SlideBackground();
        public List<Placeholder> Placeholders { get; set; } = new List<Placeholder>();
        public List<SlideHeader> Headers { get; set; } = new List<SlideHeader>();
        public List<SlideFooter> Footers { get; set; } = new List<SlideFooter>();
        public List<SlidePageNumber> PageNumbers { get; set; } = new List<SlidePageNumber>();
    }

    public enum LayoutType
    {
        Title,
        TitleAndContent,
        TwoContent,
        Comparison,
        TitleOnly,
        Blank,
        SectionHeader,
        ContentWithCaption,
        PictureWithCaption,
        VerticalTitleAndText,
        VerticalTwoContent,
        Object,
        Table,
        Chart,
        Diagram,
        Media,
        Custom
    }

    public class SlideMaster
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SlideBackground Background { get; set; } = new SlideBackground();
        public List<SlideLayout> Layouts { get; set; } = new List<SlideLayout>();
        public List<Shape> Shapes { get; set; } = new List<Shape>();
        public Theme Theme { get; set; }
    }

    public class SlideBackground
    {
        public FillDefinition Fill { get; set; } = new FillDefinition();
        public PictureDefinition Picture { get; set; }
        public EffectList Effect { get; set; }
        public bool ApplyToMaster { get; set; } = true;

        public SlideBackground Clone()
        {
            return new SlideBackground
            {
                Fill = Fill.Clone(),
                Picture = Picture?.Clone(),
                Effect = Effect?.Clone(),
                ApplyToMaster = ApplyToMaster
            };
        }
    }

    public class FillDefinition
    {
        public FillType Type { get; set; } = FillType.Solid;
        public string Color { get; set; } = "FFFFFF";
        public double Opacity { get; set; } = 1.0;
        public GradientFill Gradient { get; set; }
        public PatternFill Pattern { get; set; }
        public PictureFill Picture { get; set; }
    }

    public enum FillType
    {
        None,
        Solid,
        Gradient,
        Pattern,
        Picture,
        Blip
    }

    public class GradientFill
    {
        public GradientType Type { get; set; } = GradientType.Linear;
        public double Angle { get; set; } = 0;
        public List<GradientStop> Stops { get; set; } = new List<GradientStop>();
    }

    public enum GradientType
    {
        Linear,
        Radial,
        Rectangular,
        Path
    }

    public class GradientStop
    {
        public double Position { get; set; }
        public string Color { get; set; }
        public double Opacity { get; set; } = 1.0;
    }

    public class PatternFill
    {
        public PatternType PatternType { get; set; } = PatternType.DashedHorizontal;
        public string ForegroundColor { get; set; } = "000000";
        public string BackgroundColor { get; set; } = "FFFFFF";
    }

    public enum PatternType
    {
        DashedHorizontal,
        DashedVertical,
        DiagonalLeft,
        DiagonalRight,
        Horizontal,
        Vertical,
        Cross,
        DiagonalCross,
        Dotted,
        Dashed,
        Brick,
        CheckerBoard
    }

    public class PictureFill
    {
        public string ImageId { get; set; }
        public string FilePath { get; set; }
        public byte[] ImageData { get; set; }
        public PictureStretch Stretch { get; set; }
        public PictureAlignment Alignment { get; set; } = PictureAlignment.Center;
        public double Rotation { get; set; }
    }

    public class PictureStretch
    {
        public bool FillRectangle { get; set; }
    }

    public enum PictureAlignment
    {
        Center,
        Tile,
        Stretch,
        TopLeft,
        Top,
        TopRight,
        Left,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    public class PictureDefinition
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public byte[] Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string CropLeft { get; set; }
        public string CropTop { get; set; }
        public string CropRight { get; set; }
        public string CropBottom { get; set; }
        public string Transform { get; set; }
    }

    public class EffectList
    {
        public List<Effect> Effects { get; set; } = new List<Effect>();
        public bool HasEffects => Effects.Count > 0;

        public EffectList Clone()
        {
            return new EffectList
            {
                Effects = Effects.Select(e => e.Clone()).ToList()
            };
        }
    }

    public class Effect
    {
        public EffectType Type { get; set; }
        public string Name { get; set; }
        public double? Value { get; set; }
        public double? Radius { get; set; }
        public string Color { get; set; }
        public double? Blur { get; set; }
        public double? Offset { get; set; }
        public double? Angle { get; set; }
        public bool? Inside { get; set; }
        public double? ShadowColor { get; set; }
        public double? ShadowOpacity { get; set; }
        public double? ShadowRadius { get; set; }
        public double? ShadowOffsetX { get; set; }
        public double? ShadowOffsetY { get; set; }
        public double? GlowColor { get; set; }
        public double? GlowRadius { get; set; }
        public SoftEdgeType? SoftEdge { get; set; }

        public Effect Clone()
        {
            return new Effect
            {
                Type = Type,
                Name = Name,
                Value = Value,
                Radius = Radius,
                Color = Color,
                Blur = Blur,
                Offset = Offset,
                Angle = Angle,
                Inside = Inside,
                ShadowColor = ShadowColor,
                ShadowOpacity = ShadowOpacity,
                ShadowRadius = ShadowRadius,
                ShadowOffsetX = ShadowOffsetX,
                ShadowOffsetY = ShadowOffsetY,
                GlowColor = GlowColor,
                GlowRadius = GlowRadius,
                SoftEdge = SoftEdge
            };
        }
    }

    public enum EffectType
    {
        OuterShadow,
        InnerShadow,
        SoftEdge,
        Glow,
        Blur,
        Bevel,
        Reflection,
        3DRotation,
        LightRig
    }

    public class SoftEdgeType
    {
        public double Radius { get; set; }
    }

    public class Placeholder
    {
        public int Id { get; set; }
        public PlaceholderType Type { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
        public string Text { get; set; }
        public bool IsVisible { get; set; } = true;
        public string SuggestedStyle { get; set; }
    }

    public enum PlaceholderType
    {
        Title,
        Body,
        Subtitle,
        CenterTitle,
        CenterBody,
        Chart,
        ClipArt,
        Diagram,
        Media,
        Picture,
        Table,
        Footer,
        SlideNumber,
        Date,
        Header,
        Logo,
        Watermark,
        Background
    }

    public class SlideHeader
    {
        public string Text { get; set; }
        public TextStyle Style { get; set; } = new TextStyle();
        public bool Visible { get; set; } = true;
    }

    public class SlideFooter
    {
        public string Text { get; set; }
        public TextStyle Style { get; set; } = new TextStyle();
        public bool Visible { get; set; } = true;
    }

    public class SlidePageNumber
    {
        public TextStyle Style { get; set; } = new TextStyle();
        public bool Visible { get; set; } = true;
        public string Format { get; set; } = "1";
    }

    public class TextStyle
    {
        public string FontName { get; set; } = "Arial";
        public double FontSize { get; set; } = 18;
        public string FontColor { get; set; } = "000000";
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
        public bool Underline { get; set; } = false;
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Justify,
        JustifyLow,
        Distributed,
        ThaiDistributed
    }

    public class SlideTransition
    {
        public TransitionType Type { get; set; } = TransitionType.None;
        public TransitionDirection Direction { get; set; } = TransitionDirection.Right;
        public int Duration { get; set; } = 500;
        public TransitionSpeed Speed { get; set; } = TransitionSpeed.Medium;
        public bool SoundEnabled { get; set; } = false;
        public string SoundFile { get; set; }
        public bool LoopSoundUntilNext { get; set; } = false;
        public bool AdvanceOnClick { get; set; } = true;
        public bool AdvanceOnTime { get; set; } = false;
        public int? AdvanceTime { get; set; }
        public bool Automatic { get; set; } = false;
        public TransitionSubtype Subtype { get; set; } = TransitionSubtype.None;

        public SlideTransition Clone()
        {
            return new SlideTransition
            {
                Type = Type,
                Direction = Direction,
                Duration = Duration,
                Speed = Speed,
                SoundEnabled = SoundEnabled,
                SoundFile = SoundFile,
                LoopSoundUntilNext = LoopSoundUntilNext,
                AdvanceOnClick = AdvanceOnClick,
                AdvanceOnTime = AdvanceOnTime,
                AdvanceTime = AdvanceTime,
                Automatic = Automatic,
                Subtype = Subtype
            };
        }
    }

    public enum TransitionType
    {
        None,
        Fade,
        Push,
        Cover,
        Wipe,
        Split,
        Reveal,
        Random,
        Cut,
        DrillDown,
        Flash,
        Gloss,
        NewsFlash,
        Overlay,
        Peel,
        Pixellate,
        Shrink,
        Swivel,
        Uncover,
        Zoom
    }

    public enum TransitionDirection
    {
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum TransitionSpeed
    {
        Slow,
        Medium,
        Fast
    }

    public enum TransitionSubtype
    {
        None,
        Horizontal,
        Vertical,
        In,
        Out,
        Across,
        FromLeft,
        FromRight,
        FromTop,
        FromBottom,
        Clockwise,
        Counterclockwise,
        Full,
        Quarter,
        Half,
        ThreeQuarter,
        Fade,
        FromCenter,
        IntoRectangle,
        OutRectangle,
        Laser,
        Blast,
        Bloom
    }

    public class SlideTiming
    {
        public List<Trigger> Triggers { get; set; } = new List<Trigger>();
        public List<TimingNode> TimingNodes { get; set; } = new List<TimingNode>();
        public double DefaultDuration { get; set; } = 500;
        public double DefaultDelay { get; set; } = 0;
    }

    public class Trigger
    {
        public string Id { get; set; }
        public TriggerType Type { get; set; }
        public string TargetId { get; set; }
        public string EventId { get; set; }
        public double Delay { get; set; } = 0;
        public List<TimingNode> Animations { get; set; } = new List<TimingNode>();
    }

    public enum TriggerType
    {
        OnClick,
        WithPrevious,
        AfterPrevious,
        OnMouseOver,
        OnDoubleClick,
        OnRightClick
    }

    public class TimingNode
    {
        public string Id { get; set; }
        public TimingNodeType Type { get; set; }
        public string ShapeId { get; set; }
        public double Duration { get; set; } = 500;
        public double StartTime { get; set; }
        public double EndTime => StartTime + Duration;
        public TimingNodeFill Fill { get; set; } = TimingNodeFill.Hold;
        public List<AnimationEffect> Effects { get; set; } = new List<AnimationEffect>();
    }

    public enum TimingNodeType
    {
        Click,
        WithPrevious,
        AfterPrevious,
        MainSequence,
        InteractiveSequence,
        Bookmark
    }

    public enum TimingNodeFill
    {
        Hold,
        Remove,
        Freeze,
        HoldOnorepeat
    }

    public class AnimationEffect
    {
        public string Id { get; set; }
        public AnimationEffectType Type { get; set; }
        public AnimationPresetPreset PresetClass { get; set; }
        public string Name { get; set; }
        public double? Value { get; set; }
        public AnimationAttributeName? Attribute { get; set; }
        public string Formula { get; set; }
        public double? From { get; set; }
        public double? To { get; set; }
        public AnimationByType ByType { get; set; } = AnimationByType.Value;
        public double? By { get; set; }
        public double? FromCenter { get; set; }
        public bool? AutoReverse { get; set; }
        public AnimationRestartType? Restart { get; set; }
        public AnimationIterationCountType? RepeatCount { get; set; }
        public double? RepeatDuration { get; set; }
        public AnimationFillMode? Fill { get; set; }
        public TimingFunctionType TimingFunction { get; set; } = TimingFunctionType.Linear;
        public double? Speed { get; set; }
    }

    public enum AnimationEffectType
    {
        Exit,
        Entrance,
        Emphasis,
        Motion,
        Media
    }

    public enum AnimationPresetPreset
    {
        None,
        Entrance,
        Exit,
        Emphasis,
        Path,
        Media
    }

    public enum AnimationAttributeName
    {
        Blip,
        BldP,
        BldY,
        Chk,
        Clr,
        ClrRepr,
        DBy,
        DBw,
        DChg,
        CIr,
        DRot,
        DwBy,
        DyBy,
        GrpBld,
        GrpSp,
        Ins,
        IV,
        Opcty,
        PrChld,
        Rev,
        RlBy,
        Rot,
        Scale,
        Spc,
        Thumbs,
        Vis,
        X,
        XBy,
        Xfrm,
        Y,
        YBy,
        Z,
        ZBy,
        Zoom
    }

    public enum AnimationByType
    {
        Value,
        Percentage,
        Position
    }

    public enum AnimationRestartType
    {
        Always,
        WhenOff,
        Never
    }

    public enum AnimationIterationCountType
    {
        Infinity,
        Count
    }

    public enum AnimationFillMode
    {
        Retain,
        Remove,
        Freeze
    }

    public enum TimingFunctionType
    {
        Linear,
        Discrete,
        Pline,
        Ist,
        Ost,
        Ostr,
        L,
        T,
        TD,
        TL,
        Ts,
        Tw,
        Tm
    }

    public class Theme
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ThemeColors Colors { get; set; } = new ThemeColors();
        public ThemeFonts Fonts { get; set; } = new ThemeFonts();
        public ThemeFormats Formats { get; set; } = new ThemeFormats();
        public List<ThemeVariant> Variants { get; set; } = new List<ThemeVariant>();
    }

    public class ThemeColors
    {
        public string Dark1 { get; set; } = "000000";
        public string Light1 { get; set; } = "FFFFFF";
        public string Dark2 { get; set; } = "1F497D";
        public string Light2 { get; set; } = "EEF1F6";
        public string Accent1 { get; set; } = "4472C4";
        public string Accent2 { get; set; } = "ED7D31";
        public string Accent3 { get; set; } = "A5A5A5";
        public string Accent4 { get; set; } = "FFC000";
        public string Accent5 { get; set; } = "5B9BD5";
        public string Accent6 { get; set; } = "70AD47";
        public string Hyperlink { get; set; } = "0563C1";
        public string FollowedHyperlink { get; set; } = "954F72";
    }

    public class ThemeFonts
    {
        public string MajorFont { get; set; } = "Calibri Light";
        public string MinorFont { get; set; } = "Calibri";
        public FontScheme FontScheme { get; set; } = FontScheme.Mixed;
    }

    public enum FontScheme
    {
        Mixed,
        Major,
        Minor,
        Neither
    }

    public class ThemeFormats
    {
        public Dictionary<string, FormatDefinition> Formats { get; set; } = new Dictionary<string, FormatDefinition>();
    }

    public class FormatDefinition
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public FormatType Type { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public enum FormatType
    {
        Fill,
        Line,
        Effect,
        Font
    }

    public class ThemeVariant
    {
        public string Name { get; set; }
        public bool IsPrimary { get; set; }
        public ThemeColors Colors { get; set; }
        public ThemeFonts Fonts { get; set; }
    }

    public class SlideNotes
    {
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Text { get; set; }
        public List<NotesShape> Shapes { get; set; } = new List<NotesShape>();

        public SlideNotes Clone()
        {
            return new SlideNotes
            {
                Author = Author,
                Created = Created,
                Modified = Modified,
                Text = Text,
                Shapes = Shapes.Select(s => s.Clone()).ToList()
            };
        }
    }

    public class NotesShape
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public TextStyle Style { get; set; } = new TextStyle();
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public NotesShape Clone()
        {
            return new NotesShape
            {
                Id = Id,
                Text = Text,
                Style = new TextStyle
                {
                    FontName = Style.FontName,
                    FontSize = Style.FontSize,
                    FontColor = Style.FontColor,
                    Bold = Style.Bold,
                    Italic = Style.Italic,
                    Underline = Style.Underline,
                    TextAlignment = Style.TextAlignment
                },
                PositionX = PositionX,
                PositionY = PositionY,
                Width = Width,
                Height = Height
            };
        }
    }

    public class Shape
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ShapeType Type { get; set; }
        public PlaceholderType? PlaceholderType { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int ZIndex { get; set; }
        public bool Locked { get; set; } = false;
        public bool Hidden { get; set; } = false;
        public bool Print { get; set; } = true;

        public FillDefinition Fill { get; set; } = new FillDefinition();
        public LineDefinition Line { get; set; }
        public EffectList Effect { get; set; }
        public ThreeDRendering ThreeD { get; set; }
        public ShapeTransform Transform { get; set; } = new ShapeTransform();

        public string Text { get; set; }
        public TextContent TextContent { get; set; }
        public PictureDefinition Picture { get; set; }
        public MediaContent Media { get; set; }

        public Shape Clone()
        {
            return new Shape
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name + " (Copy)",
                Type = Type,
                PlaceholderType = PlaceholderType,
                PositionX = PositionX,
                PositionY = PositionY,
                Width = Width,
                Height = Height,
                ZIndex = ZIndex,
                Locked = Locked,
                Hidden = Hidden,
                Print = Print,
                Fill = Fill.Clone(),
                Line = Line?.Clone(),
                Effect = Effect?.Clone(),
                ThreeD = ThreeD?.Clone(),
                Transform = Transform.Clone(),
                Text = Text,
                TextContent = TextContent?.Clone(),
                Picture = Picture?.Clone(),
                Media = Media?.Clone()
            };
        }
    }

    public enum ShapeType
    {
        Rectangle,
        Ellipse,
        Diamond,
        Triangle,
        RightTriangle,
        Parallelogram,
        Trapezoid,
        Hexagon,
        Heptagon,
        Octagon,
        Decagon,
        Dodecagon,
        Star4,
        Star5,
        Star6,
        Star8,
        Star10,
        Star12,
        Star16,
        Star24,
        Star32,
        RoundRectangle,
        Bevel,
        Bulge,
        Cannon,
        Cube,
        Can,
        Cylinder,
        Pie,
        NonIsoscelesTriangle,
        RightTriangleShape,
        DiamondShape,
        TrapezoidShape,
        Pentagon,
        Chevron,
        RightArrow,
        LeftArrow,
        UpArrow,
        DownArrow,
        LeftRightArrow,
        UpDownArrow,
        QuadArrow,
        LeftRightUpArrow,
        BentArrow,
        UTurnArrow,
        CurvedRightArrow,
        CurvedLeftArrow,
        CurvedUpArrow,
        CurvedDownArrow,
        StackedArrow,
        CurvedStar,
        Heart,
        Lightning,
        Sun,
        Moon,
        Cloud,
        Gear,
        SmileyFace,
        Frame,
        Banner,
        Arc,
        Bracket,
        Brace,
        Callout,
        ActionButtonHome,
        ActionButtonHelp,
        ActionButtonInformation,
        ActionButtonNextComment,
        ActionButtonPreviousComment,
        ActionButtonEnd,
        ActionButtonReturn,
        ActionButtonDocument,
        ActionButtonSound,
        ActionButtonMovie,
        CornerTabs,
        PlaqueTabs,
        StripedRightArrow,
        NotchedRightArrow,
        BlockArc,
        Donut,
        NoSymbol,
        FourArrows,
        FourFadingArrows,
        FourTriangles,
        AlternatingArrows,
        LeftCircularArrow,
        LeftRightCircularArrow,
        CurveArrow,
        SwooshArrow,
        Picture,
        Table,
        SmartArt,
        Chart,
        Media,
        TextBox,
        ContentPlaceholder,
        PicturePlaceholder,
        Title,
        Header,
        Footer,
        SlideNumber,
        Date,
        Object,
        ClipArt,
        Ink,
        InkComment,
        Group,
        Graph,
        LinkedGraph,
        Diagram,
        Placeholder
    }

    public class LineDefinition
    {
        public LineType Type { get; set; } = LineType.Solid;
        public double Width { get; set; } = 0.75;
        public string Color { get; set; } = "000000";
        public double Opacity { get; set; } = 1.0;
        public LineCapStyle Cap { get; set; } = LineCapStyle.Flat;
        public LineJoinStyle Join { get; set; } = LineJoinStyle.Round;
        public CompoundType CompoundType { get; set; } = CompoundType.Simple;
        public PresetLineDashStyle PresetDash { get; set; } = PresetLineDashStyle.Solid;
        public ArrowheadStyle HeadType { get; set; } = ArrowheadStyle.None;
        public ArrowheadStyle TailType { get; set; } = ArrowheadStyle.None;
        public double? HeadWidth { get; set; }
        public double? HeadLength { get; set; }
        public double? TailWidth { get; set; }
        public double? TailLength { get; set; }

        public LineDefinition Clone()
        {
            return new LineDefinition
            {
                Type = Type,
                Width = Width,
                Color = Color,
                Opacity = Opacity,
                Cap = Cap,
                Join = Join,
                CompoundType = CompoundType,
                PresetDash = PresetDash,
                HeadType = HeadType,
                TailType = TailType,
                HeadWidth = HeadWidth,
                HeadLength = HeadLength,
                TailWidth = TailWidth,
                TailLength = TailLength
            };
        }
    }

    public enum LineType
    {
        None,
        Solid,
        Dash,
        DashDot,
        DashDotDot,
        Dot,
        Inside,
        Outside,
        Default
    }

    public enum LineCapStyle
    {
        Flat,
        Round,
        Square
    }

    public enum LineJoinStyle
    {
        Round,
        Bevel,
        Miter
    }

    public enum CompoundType
    {
        Single,
        Double,
        ThickThin,
        ThinThick,
        Tri
    }

    public enum PresetLineDashStyle
    {
        Solid,
        Dash,
        DashDot,
        lgDash,
        lgDashDot,
        lgDashDotDot,
        dot,
        SolidArrow,
        DashArrow,
        DashDotArrow,
        TriangleArrow,
        SolidChevron,
        DashChevron,
        DashDotDot,
        lgDashDotDot,
        dotted,
        dashed,
        lgDash,
        dash,
        mdDash,
        sdashdot,
        sdashdotdot
    }

    public enum ArrowheadStyle
    {
        None,
        Triangle,
        Arrow,
        Stealth,
        Diamond,
        Oval,
        Open
    }

    public class ThreeDRendering
    {
        public bool Enabled { get; set; } = false;
        public ThreeDRashType BevelType { get; set; } = ThreeDRashType.Circle;
        public double BevelTopWidth { get; set; } = 6;
        public double BevelBottomWidth { get; set; } = 6;
        public LightRigType LightRigType { get; set; } = LightRigType.Balanced;
        public double LightRigAngle { get; set; } = 0;
        public LightRigDirection LightRigDirection { get; set; } = LightRigDirection.Top;
        public CameraType CameraType { get; set; } = CameraType.Orthographic;
        public double CameraX { get; set; } = 0;
        public double CameraY { get; set; } = 0;
        public double CameraZ { get; set; } = 0;
        public PresetMaterialType MaterialType { get; set; } = PresetMaterialType.WarmMatte;
        public ContourColor ContourColor { get; set; }
        public double ContourWidth { get; set; } = 0;
        public ExtrusionColor ExtrusionColor { get; set; }
        public double ExtrusionHeight { get; set; } = 0;
        public bool ExtrusionColorEnabled { get; set; } = false;

        public ThreeDRendering Clone()
        {
            return new ThreeDRendering
            {
                Enabled = Enabled,
                BevelType = BevelType,
                BevelTopWidth = BevelTopWidth,
                BevelBottomWidth = BevelBottomWidth,
                LightRigType = LightRigType,
                LightRigAngle = LightRigAngle,
                LightRigDirection = LightRigDirection,
                CameraType = CameraType,
                CameraX = CameraX,
                CameraY = CameraY,
                CameraZ = CameraZ,
                MaterialType = MaterialType,
                ContourColor = ContourColor?.Clone(),
                ContourWidth = ContourWidth,
                ExtrusionColor = ExtrusionColor?.Clone(),
                ExtrusionHeight = ExtrusionHeight,
                ExtrusionColorEnabled = ExtrusionColorEnabled
            };
        }
    }

    public enum ThreeDRashType
    {
        Circle,
        Relaxed,
        RelaxedInverted,
        Cross,
        Angle,
        SoftRound,
        Convex,
        Inscribed,
        Rounded,
        RoundedInside,
        RoundedOutside,
        Bump,
        Cot,
        ArtDeco
    }

    public enum LightRigType
    {
        Balanced,
        BrightRoom,
        Chilly,
        Cloudy,
        Contrast,
        Flat,
        Flood,
        Freezing,
        Glow,
        Gold,
        Harsh,
        LegacyFlat1,
        LegacyFlat2,
        LegacyFlat3,
        LegacyFlat4,
        LegacyHarsh1,
        LegacyHarsh2,
        LegacyHarsh3,
        LegacyHarsh4,
        LegacyNormal1,
        LegacyNormal2,
        LegacyNormal3,
        LegacyNormal4,
        Morning,
        MorningWarm,
        Soft,
        Spotlight,
        Sunrise,
        Sunset,
        ThreePoint,
        TwoPoint,
        Weak
    }

    public enum LightRigDirection
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

    public enum CameraType
    {
        Orthographic,
        Perspective,
        PerspectiveContrastingLeft,
        PerspectiveContrastingRight,
        PerspectiveContrastingAbove,
        PerspectiveContrastingBelow,
        PerspectiveFront,
        PerspectiveHeroicExtremeLeft,
        PerspectiveHeroicExtremeRight,
        PerspectiveHeroicLeft,
        PerspectiveHeroicRight,
        PerspectiveHeroicBelow,
        PerspectiveHeroicAbove,
        PerspectiveRelaxed,
        PerspectiveRelaxedMirrored,
        PerspectiveShift,
        PerspectiveTop,
        PerspectiveBottom,
        PerspectiveTopLeft,
        PerspectiveTopRight,
        PerspectiveBottomLeft,
        PerspectiveBottomRight,
        PerspectiveAboveLeft,
        PerspectiveAboveRight,
        PerspectiveBelowLeft,
        PerspectiveBelowRight,
        PerspectiveFrontLeft,
        PerspectiveFrontRight
    }

    public enum PresetMaterialType
    {
        LegacyMatte,
        LegacyPlastic,
        LegacyMetal,
        LegacyWireframe,
        Matte,
        Plastic,
        Dvmatte,
        Softmatte,
        Softplastic,
        Clear,
        Flat,
        Softedge,
        Dvclear,
        Glaze,
        Pearl,
        Holiday,
        TranslucentPowder,
        ThinMaterial,
        LegacyHoliday,
        LegacyEmber,
        LegacyJade,
        LegacyMoss,
        LegacyNeon,
        LegacyPearl,
        LegacySapphire,
        WarmMatte
    }

    public class ContourColor
    {
        public string Color { get; set; } = "000000";
        public double? Tint { get; set; }
        public double? Shade { get; set; }

        public ContourColor Clone()
        {
            return new ContourColor
            {
                Color = Color,
                Tint = Tint,
                Shade = Shade
            };
        }
    }

    public class ExtrusionColor
    {
        public string Color { get; set; } = "FFFFFF";
        public double? Tint { get; set; }
        public double? Shade { get; set; }

        public ExtrusionColor Clone()
        {
            return new ExtrusionColor
            {
                Color = Color,
                Tint = Tint,
                Shade = Shade
            };
        }
    }

    public class ShapeTransform
    {
        public double Rotation { get; set; } = 0;
        public double? HorizontalFlip { get; set; }
        public double? VerticalFlip { get; set; }
        public Point2D Offset { get; set; } = new Point2D();
        public Extent Size { get; set; } = new Extent();
        public Point2D Origin { get; set; } = new Point2D();
        public TransformInfo TransformInfo { get; set; }

        public ShapeTransform Clone()
        {
            return new ShapeTransform
            {
                Rotation = Rotation,
                HorizontalFlip = HorizontalFlip,
                VerticalFlip = VerticalFlip,
                Offset = new Point2D { X = Offset.X, Y = Offset.Y },
                Size = new Extent { Cx = Size.Cx, Cy = Size.Cy },
                Origin = new Point2D { X = Origin.X, Y = Origin.Y },
                TransformInfo = TransformInfo?.Clone()
            };
        }
    }

    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Extent
    {
        public double Cx { get; set; }
        public double Cy { get; set; }
    }

    public class TransformInfo
    {
        public double? RotationCenterX { get; set; }
        public double? RotationCenterY { get; set; }
        public string Matrix { get; set; }

        public TransformInfo Clone()
        {
            return new TransformInfo
            {
                RotationCenterX = RotationCenterX,
                RotationCenterY = RotationCenterY,
                Matrix = Matrix
            };
        }
    }

    public class TextContent
    {
        public List<TextParagraph> Paragraphs { get; set; } = new List<TextParagraph>();
        public TextStyle DefaultStyle { get; set; } = new TextStyle();
    }

    public class TextParagraph
    {
        public int Level { get; set; }
        public string Text { get; set; }
        public TextStyle Style { get; set; }
        public List<TextRun> Runs { get; set; } = new List<TextRun>();
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public Indentation Indentation { get; set; }
        public Spacing LineSpacing { get; set; }
        public BulletType Bullet { get; set; }
    }

    public class TextRun
    {
        public string Text { get; set; }
        public TextStyle Style { get; set; }
        public Field Field { get; set; }
    }

    public class Field
    {
        public FieldType Type { get; set; }
        public string Value { get; set; }
        public string Format { get; set; }
    }

    public enum FieldType
    {
        None,
        SlideNumber,
        SlideTitle,
        Date,
        Time,
        Footer,
        Header,
        FileName,
        FilePath,
        Author,
        Company,
        Subject,
        Title,
        Keywords,
        Category,
        Manager
    }

    public class Indentation
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int FirstLine { get; set; }
        public int FirstLineChars { get; set; }
    }

    public class Spacing
    {
        public int? Before { get; set; }
        public int? After { get; set; }
        public double? Line { get; set; }
        public SpacingLineRule LineRule { get; set; } = SpacingLineRule.Auto;
    }

    public enum SpacingLineRule
    {
        Auto,
        Exact
    }

    public class BulletType
    {
        public BulletTypeStyle Style { get; set; } = BulletTypeStyle.None;
        public string Color { get; set; }
        public string FontName { get; set; }
        public int? Size { get; set; }
        public string Character { get; set; }
        public string PictureId { get; set; }
        public int StartValue { get; set; } = 1;
        public NumberingType NumberingType { get; set; } = NumberingType.Arabic;
        public int Level { get; set; }
    }

    public enum BulletTypeStyle
    {
        None,
        Bullet,
        Numbered,
        Picture,
        AutoNumbered
    }

    public enum NumberingType
    {
        Arabic,
        UpperRoman,
        LowerRoman,
        UpperLetter,
        LowerLetter,
        Ordinal,
        ThaiArabic,
        ThaiLetter,
        HebrewNonStandard,
        Aiueo,
        Iroha,
        ArabicHalfWidth,
        ArabicFullWidth,
        HebrewStandardAlpha,
        ArabicAlpha
    }

    public class MediaContent
    {
        public MediaType Type { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public byte[] Data { get; set; }
        public MediaVolume Volume { get; set; } = new MediaVolume();
        public bool Mute { get; set; } = false;
        public bool Loop { get; set; } = false;
        public MediaPlayMode PlayMode { get; set; } = MediaPlayMode.InSequence;
        public MediaTrigger StartTrigger { get; set; } = MediaTrigger.OnClick;
        public double StartPosition { get; set; } = 0;
        public double? EndPosition { get; set; }
        public bool FullScreen { get; set; } = false;
        public MediaFadeIn FadeInDuration { get; set; }
        public MediaFadeOut FadeOutDuration { get; set; }
    }

    public enum MediaType
    {
        None,
        Video,
        Audio,
        Flash,
        AnimatedGif
    }

    public class MediaVolume
    {
        public double Level { get; set; } = 1.0;
        public bool Muted { get; set; } = false;
    }

    public enum MediaPlayMode
    {
        InSequence,
        AllAtOnce,
        AllAtOnceShowAll,
        LoopUntilStopped
    }

    public enum MediaTrigger
    {
        OnClick,
        WithPrevious,
        AfterPrevious,
        OnMouseOver,
        OnRightClick,
        OnDoubleClick
    }

    public class MediaFadeIn
    {
        public double Duration { get; set; } = 0;
    }

    public class MediaFadeOut
    {
        public double Duration { get; set; } = 0;
    }

    public class SlideEventArgs : EventArgs
    {
        public Slide Slide { get; }

        public SlideEventArgs(Slide slide)
        {
            Slide = slide;
        }
    }

    public class SlideChangedEventArgs : EventArgs
    {
        public int PreviousIndex { get; }
        public int CurrentIndex { get; }

        public SlideChangedEventArgs(int previousIndex, int currentIndex)
        {
            PreviousIndex = previousIndex;
            CurrentIndex = currentIndex;
        }
    }
}

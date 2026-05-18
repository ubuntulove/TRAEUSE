using System;
using System.Collections.Generic;
using OfficeSuite.Core.Models;

namespace OfficeSuite.Presentation.Models
{
    public class PresentationDocument : ModelBase
    {
        private string _id;
        private string _title;
        private List<Slide> _slides;
        private int _currentSlideIndex;
        private PresentationSettings _settings;
        private DocumentProperties _properties;
        private SlideMaster _slideMaster;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public List<Slide> Slides
        {
            get => _slides ??= new List<Slide>();
            set => SetProperty(ref _slides, value);
        }

        public int CurrentSlideIndex
        {
            get => _currentSlideIndex;
            set => SetProperty(ref _currentSlideIndex, value);
        }

        public PresentationSettings Settings
        {
            get => _settings ??= new PresentationSettings();
            set => SetProperty(ref _settings, value);
        }

        public DocumentProperties Properties
        {
            get => _properties ??= new DocumentProperties();
            set => SetProperty(ref _properties, value);
        }

        public SlideMaster SlideMaster
        {
            get => _slideMaster ??= new SlideMaster();
            set => SetProperty(ref _slideMaster, value);
        }

        public PresentationDocument()
        {
            _id = Guid.NewGuid().ToString();
            _slides = new List<Slide>();
            _settings = new PresentationSettings();
            _properties = new DocumentProperties();
            _slideMaster = new SlideMaster();
        }

        public Slide AddSlide()
        {
            var slide = new Slide { Index = Slides.Count };
            Slides.Add(slide);
            return slide;
        }

        public void RemoveSlide(int index)
        {
            if (index >= 0 && index < Slides.Count)
            {
                Slides.RemoveAt(index);
                UpdateSlideIndices();
            }
        }

        private void UpdateSlideIndices()
        {
            for (int i = 0; i < Slides.Count; i++)
                Slides[i].Index = i;
        }

        public Slide GetCurrentSlide()
        {
            if (CurrentSlideIndex >= 0 && CurrentSlideIndex < Slides.Count)
                return Slides[CurrentSlideIndex];
            return null;
        }
    }

    public class Slide : ModelBase
    {
        private string _id;
        private int _index;
        private string _name;
        private string _title;
        private List<SlideShape> _shapes;
        private SlideLayout _layout;
        private SlideBackground _background;
        private Transition _transition;
        private bool _isHidden;
        private TimeSpan _timing;
        private List<Animation> _animations;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public List<SlideShape> Shapes
        {
            get => _shapes ??= new List<SlideShape>();
            set => SetProperty(ref _shapes, value);
        }

        public SlideLayout Layout
        {
            get => _layout;
            set => SetProperty(ref _layout, value);
        }

        public SlideBackground Background
        {
            get => _background ??= new SlideBackground();
            set => SetProperty(ref _background, value);
        }

        public Transition Transition
        {
            get => _transition ??= new Transition();
            set => SetProperty(ref _transition, value);
        }

        public bool IsHidden
        {
            get => _isHidden;
            set => SetProperty(ref _isHidden, value);
        }

        public TimeSpan Timing
        {
            get => _timing;
            set => SetProperty(ref _timing, value);
        }

        public List<Animation> Animations
        {
            get => _animations ??= new List<Animation>();
            set => SetProperty(ref _animations, value);
        }

        public Slide()
        {
            _id = Guid.NewGuid().ToString();
            _shapes = new List<SlideShape>();
            _background = new SlideBackground();
            _transition = new Transition();
            _animations = new List<Animation>();
        }

        public T AddShape<T>() where T : SlideShape, new()
        {
            var shape = new T();
            Shapes.Add(shape);
            return shape;
        }

        public void RemoveShape(SlideShape shape)
        {
            if (Shapes.Contains(shape))
                Shapes.Remove(shape);
        }
    }

    public abstract class SlideShape : ModelBase
    {
        private string _id;
        private string _name;
        private double _left;
        private double _top;
        private double _width;
        private double _height;
        private double _rotation;
        private bool _isLocked;
        private bool _isVisible;
        private ShapeType _shapeType;
        private FillFormat _fill;
        private LineFormat _line;
        private EffectFormat _effects;
        private ShadowFormat _shadow;
        private int _zIndex;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double Rotation
        {
            get => _rotation;
            set => SetProperty(ref _rotation, value);
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ShapeType ShapeType
        {
            get => _shapeType;
            set => SetProperty(ref _shapeType, value);
        }

        public FillFormat Fill
        {
            get => _fill ??= new FillFormat();
            set => SetProperty(ref _fill, value);
        }

        public LineFormat Line
        {
            get => _line ??= new LineFormat();
            set => SetProperty(ref _line, value);
        }

        public EffectFormat Effects
        {
            get => _effects ??= new EffectFormat();
            set => SetProperty(ref _effects, value);
        }

        public ShadowFormat Shadow
        {
            get => _shadow ??= new ShadowFormat();
            set => SetProperty(ref _shadow, value);
        }

        public int ZIndex
        {
            get => _zIndex;
            set => SetProperty(ref _zIndex, value);
        }

        public SlideShape()
        {
            _id = Guid.NewGuid().ToString();
            _isVisible = true;
            _fill = new FillFormat();
            _line = new LineFormat();
            _effects = new EffectFormat();
            _shadow = new ShadowFormat();
        }

        public abstract string GetShapePath();
    }

    public enum ShapeType
    {
        Rectangle,
        RoundedRectangle,
        Ellipse,
        Diamond,
        Triangle,
        RightTriangle,
        IsoscelesTriangle,
        Parallelogram,
        Trapezoid,
        Pentagon,
        Hexagon,
        Heptagon,
        Octagon,
        Decagon,
        Dodecagon,
        Pie,
        Chord,
        Arrow,
        ArrowRight,
        ArrowLeft,
        ArrowUp,
        ArrowDown,
        ArrowQuadArrow,
        ArrowBentArrow,
        ArrowCurvedArrow,
        Star,
        FivePointedStar,
        SixPointedStar,
        EightPointedStar,
        TenPointedStar,
        TwelvePointedStar,
        SixteenPointedStar,
        TwentyFourPointedStar,
        RoundStar,
        Seal,
        Explosion,
        Heart,
        LightningBolt,
        Sun,
        Crescent,
        Bevel,
        Frame,
        Can,
        Cube,
        Bowl,
        Donut,
        Smile,
        ActionButtonBlank,
        ActionButtonHome,
        ActionButtonInformation,
        ActionButtonNextPage,
        ActionButtonPreviousPage,
        ActionButtonForward,
        ActionButtonBack,
        Callout,
        RectangularCallout,
        RoundedRectangularCallout,
        OvalCallout,
        CloudCallout,
        Line,
        StraightConnector,
        ElbowConnector,
        CurvedConnector,
        PictureFrame,
        TextBox,
        ContentPlaceholder,
        TitlePlaceholder
    }

    public class TextShape : SlideShape
    {
        private string _text;
        private TextFormat _textFormat;
        private ParagraphCollection _paragraphs;
        private TextBody _textBody;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public TextFormat TextFormat
        {
            get => _textFormat ??= new TextFormat();
            set => SetProperty(ref _textFormat, value);
        }

        public ParagraphCollection Paragraphs
        {
            get => _paragraphs ??= new ParagraphCollection();
            set => SetProperty(ref _paragraphs, value);
        }

        public TextBody TextBody
        {
            get => _textBody ??= new TextBody();
            set => SetProperty(ref _textBody, value);
        }

        public TextShape()
        {
            _shapeType = ShapeType.TextBox;
            _textFormat = new TextFormat();
            _paragraphs = new ParagraphCollection();
            _textBody = new TextBody();
        }

        public override string GetShapePath() => "M 0 0 L 100 0 L 100 100 L 0 100 Z";
    }

    public class PictureShape : SlideShape
    {
        private byte[] _imageData;
        private string _imagePath;
        private PictureEffect _effects;

        public byte[] ImageData
        {
            get => _imageData;
            set => SetProperty(ref _imageData, value);
        }

        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public PictureEffect Effects
        {
            get => _effects ??= new PictureEffect();
            set => SetProperty(ref _effects, value);
        }

        public PictureShape()
        {
            _shapeType = ShapeType.PictureFrame;
            _effects = new PictureEffect();
        }

        public override string GetShapePath() => "M 0 0 L 100 0 L 100 100 L 0 100 Z";
    }

    public class GroupShape : SlideShape
    {
        private List<SlideShape> _children;

        public List<SlideShape> Children
        {
            get => _children ??= new List<SlideShape>();
            set => SetProperty(ref _children, value);
        }

        public GroupShape()
        {
            _shapeType = ShapeType.Rectangle;
        }

        public override string GetShapePath() => "M 0 0 L 100 0 L 100 100 L 0 100 Z";
    }

    public class RectangleShape : SlideShape
    {
        public RectangleShape()
        {
            _shapeType = ShapeType.Rectangle;
            _width = 100;
            _height = 100;
        }

        public override string GetShapePath() => "M 0 0 L 100 0 L 100 100 L 0 100 Z";
    }

    public class EllipseShape : SlideShape
    {
        public EllipseShape()
        {
            _shapeType = ShapeType.Ellipse;
            _width = 100;
            _height = 100;
        }

        public override string GetShapePath() => "M 50 0 A 50 50 0 1 1 50 100 A 50 50 0 1 1 50 0";
    }

    public class FillFormat : ModelBase
    {
        private FillType _type;
        private System.Windows.Media.Color _color;
        private double _opacity;
        private GradientFill _gradient;
        private PatternFill _pattern;
        private PictureFill _picture;
        private bool _isBackground;

        public FillType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, Math.Max(0, Math.Min(100, value)));
        }

        public GradientFill Gradient
        {
            get => _gradient ??= new GradientFill();
            set => SetProperty(ref _gradient, value);
        }

        public PatternFill Pattern
        {
            get => _pattern;
            set => SetProperty(ref _pattern, value);
        }

        public PictureFill Picture
        {
            get => _picture;
            set => SetProperty(ref _picture, value);
        }

        public bool IsBackground
        {
            get => _isBackground;
            set => SetProperty(ref _isBackground, value);
        }

        public FillFormat()
        {
            _type = FillType.Solid;
            _color = System.Windows.Media.Colors.White;
            _opacity = 100;
        }
    }

    public enum FillType
    {
        None,
        Solid,
        Gradient,
        Pattern,
        Picture,
        Texture
    }

    public class GradientFill : ModelBase
    {
        private GradientType _type;
        private GradientStopCollection _stops;
        private double _angle;

        public GradientType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public GradientStopCollection Stops
        {
            get => _stops ??= new GradientStopCollection();
            set => SetProperty(ref _stops, value);
        }

        public double Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }

        public GradientFill()
        {
            _type = GradientType.Linear;
            _stops = new GradientStopCollection();
        }
    }

    public enum GradientType
    {
        Linear,
        Radial,
        Rectangular
    }

    public class GradientStop : ModelBase
    {
        private double _position;
        private System.Windows.Media.Color _color;

        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, Math.Max(0, Math.Min(100, value)));
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public GradientStop()
        {
            _position = 0;
        }
    }

    public class GradientStopCollection : List<GradientStop> { }

    public class PatternFill : ModelBase
    {
        private string _patternType;
        private System.Windows.Media.Color _foregroundColor;
        private System.Windows.Media.Color _backgroundColor;

        public string PatternType
        {
            get => _patternType;
            set => SetProperty(ref _patternType, value);
        }

        public System.Windows.Media.Color ForegroundColor
        {
            get => _foregroundColor;
            set => SetProperty(ref _foregroundColor, value);
        }

        public System.Windows.Media.Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }
    }

    public class PictureFill : ModelBase
    {
        private byte[] _imageData;
        private double _offsetX;
        private double _offsetY;
        private double _scaleX;
        private double _scaleY;

        public byte[] ImageData
        {
            get => _imageData;
            set => SetProperty(ref _imageData, value);
        }

        public double OffsetX
        {
            get => _offsetX;
            set => SetProperty(ref _offsetX, value);
        }

        public double OffsetY
        {
            get => _offsetY;
            set => SetProperty(ref _offsetY, value);
        }

        public double ScaleX
        {
            get => _scaleX;
            set => SetProperty(ref _scaleX, value);
        }

        public double ScaleY
        {
            get => _scaleY;
            set => SetProperty(ref _scaleY, value);
        }
    }

    public class LineFormat : ModelBase
    {
        private LineType _type;
        private System.Windows.Media.Color _color;
        private double _width;
        private double _Opacity;
        private LineDashStyle _dashStyle;
        private LineCap _cap;
        private LineJoin _join;
        private ArrowSettings _beginArrow;
        private ArrowSettings _endArrow;

        public LineType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Opacity
        {
            get => _Opacity;
            set => SetProperty(ref _Opacity, Math.Max(0, Math.Min(100, value)));
        }

        public LineDashStyle DashStyle
        {
            get => _dashStyle;
            set => SetProperty(ref _dashStyle, value);
        }

        public LineCap Cap
        {
            get => _cap;
            set => SetProperty(ref _cap, value);
        }

        public LineJoin Join
        {
            get => _join;
            set => SetProperty(ref _join, value);
        }

        public ArrowSettings BeginArrow
        {
            get => _beginArrow ??= new ArrowSettings();
            set => SetProperty(ref _beginArrow, value);
        }

        public ArrowSettings EndArrow
        {
            get => _endArrow ??= new ArrowSettings();
            set => SetProperty(ref _endArrow, value);
        }

        public LineFormat()
        {
            _type = LineType.Solid;
            _color = System.Windows.Media.Colors.Black;
            _width = 1;
            _dashStyle = LineDashStyle.Solid;
            _cap = LineCap.Flat;
            _join = LineJoin.Round;
            _beginArrow = new ArrowSettings();
            _endArrow = new ArrowSettings();
        }
    }

    public enum LineType
    {
        None,
        Solid,
        Dash,
        Gradient
    }

    public enum LineDashStyle
    {
        Solid,
        Dash,
        Dot,
        DashDot,
        DashDotDot,
        LongDash,
        LongDashDot,
        RoundDot,
        SquareDot
    }

    public enum LineCap
    {
        Flat,
        Round,
        Square
    }

    public enum LineJoin
    {
        Round,
        Bevel,
        Miter
    }

    public class ArrowSettings : ModelBase
    {
        private ArrowStyle _style;
        private ArrowSize _size;
        private int _width;
        private int _Length;

        public ArrowStyle Style
        {
            get => _style;
            set => SetProperty(ref _style, value);
        }

        public ArrowSize Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public int Length
        {
            get => _Length;
            set => SetProperty(ref _Length, value);
        }

        public ArrowSettings()
        {
            _style = ArrowStyle.None;
            _size = ArrowSize.Medium;
        }
    }

    public enum ArrowStyle
    {
        None,
        Triangle,
        Arrow,
        Stealth,
        Diamond,
        Oval,
        Open
    }

    public enum ArrowSize
    {
        Small,
        Medium,
        Large
    }

    public class EffectFormat : ModelBase
    {
        private bool _hasGlow;
        private System.Windows.Media.Color _glowColor;
        private double _glowSize;
        private bool _hasSoftEdge;
        private double _softEdgeSize;
        private BevelEffect _bevel;
        private List<Effect> _effects;

        public bool HasGlow
        {
            get => _hasGlow;
            set => SetProperty(ref _hasGlow, value);
        }

        public System.Windows.Media.Color GlowColor
        {
            get => _glowColor;
            set => SetProperty(ref _glowColor, value);
        }

        public double GlowSize
        {
            get => _glowSize;
            set => SetProperty(ref _glowSize, value);
        }

        public bool HasSoftEdge
        {
            get => _hasSoftEdge;
            set => SetProperty(ref _hasSoftEdge, value);
        }

        public double SoftEdgeSize
        {
            get => _softEdgeSize;
            set => SetProperty(ref _softEdgeSize, value);
        }

        public BevelEffect Bevel
        {
            get => _bevel;
            set => SetProperty(ref _bevel, value);
        }

        public List<Effect> Effects
        {
            get => _effects ??= new List<Effect>();
            set => SetProperty(ref _effects, value);
        }

        public EffectFormat()
        {
            _glowColor = System.Windows.Media.Colors.Transparent;
            _bevel = new BevelEffect();
        }
    }

    public class BevelEffect : ModelBase
    {
        private BevelType _type;
        private double _angle;
        private double _bevelHeight;
        private double _bevelWidth;

        public BevelType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public double Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }

        public double BevelHeight
        {
            get => _bevelHeight;
            set => SetProperty(ref _bevelHeight, value);
        }

        public double BevelWidth
        {
            get => _bevelWidth;
            set => SetProperty(ref _bevelWidth, value);
        }
    }

    public enum BevelType
    {
        None,
        RelaxedInset,
        Circle,
        Slope,
        Cross,
        Angle,
        SoftRound,
        Convex,
        CoolSlant,
        Divot,
        Riblet,
        HardEdge,
        ArtDeco
    }

    public class Effect : ModelBase
    {
        private EffectType _type;
        private EffectTiming _timing;
        private double _amount;

        public EffectType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public EffectTiming Timing
        {
            get => _timing;
            set => SetProperty(ref _timing, value);
        }

        public double Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }
    }

    public enum EffectType
    {
        None,
        Glow,
        Shadow,
        Reflection,
        Blur
    }

    public enum EffectTiming
    {
        Instant,
        AfterPrevious,
        WithPrevious
    }

    public class ShadowFormat : ModelBase
    {
        private bool _isVisible;
        private ShadowType _type;
        private System.Windows.Media.Color _color;
        private double _blur;
        private double _offsetX;
        private double _offsetY;
        private double _angle;
        private double _distance;
        private double _opacity;
        private bool _isInherited;

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ShadowType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public double Blur
        {
            get => _blur;
            set => SetProperty(ref _blur, value);
        }

        public double OffsetX
        {
            get => _offsetX;
            set => SetProperty(ref _offsetX, value);
        }

        public double OffsetY
        {
            get => _offsetY;
            set => SetProperty(ref _OffsetY, value);
        }

        public double Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }

        public double Distance
        {
            get => _distance;
            set => SetProperty(ref _distance, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public bool IsInherited
        {
            get => _isInherited;
            set => SetProperty(ref _isInherited, value);
        }

        public ShadowFormat()
        {
            _isVisible = false;
            _type = ShadowType.Outer;
            _color = System.Windows.Media.Colors.Black;
            _blur = 4;
            _opacity = 50;
        }
    }

    public enum ShadowType
    {
        Outer,
        Inner
    }

    public class PictureEffect : ModelBase
    {
        private bool _hasCrop;
        private double _cropLeft;
        private double _cropRight;
        private double _cropTop;
        private double _cropBottom;
        private double _brightness;
        private double _contrast;
        private bool _hasRecolor;
        private string _recolorEffect;
        private bool _hasArtisticEffect;
        private string _artisticEffect;

        public bool HasCrop
        {
            get => _hasCrop;
            set => SetProperty(ref _hasCrop, value);
        }

        public double CropLeft
        {
            get => _cropLeft;
            set => SetProperty(ref _cropLeft, value);
        }

        public double CropRight
        {
            get => _cropRight;
            set => SetProperty(ref _cropRight, value);
        }

        public double CropTop
        {
            get => _cropTop;
            set => SetProperty(ref _cropTop, value);
        }

        public double CropBottom
        {
            get => _cropBottom;
            set => SetProperty(ref _cropBottom, value);
        }

        public double Brightness
        {
            get => _brightness;
            set => SetProperty(ref _brightness, value);
        }

        public double Contrast
        {
            get => _contrast;
            set => SetProperty(ref _contrast, value);
        }

        public bool HasRecolor
        {
            get => _hasRecolor;
            set => SetProperty(ref _hasRecolor, value);
        }

        public string RecolorEffect
        {
            get => _recolorEffect;
            set => SetProperty(ref _recolorEffect, value);
        }

        public bool HasArtisticEffect
        {
            get => _hasArtisticEffect;
            set => SetProperty(ref _hasArtisticEffect, value);
        }

        public string ArtisticEffect
        {
            get => _artisticEffect;
            set => SetProperty(ref _artisticEffect, value);
        }
    }

    public class TextFormat : ModelBase
    {
        private string _fontFamily;
        private double _fontSize;
        private System.Windows.Media.Color _fontColor;
        private bool _bold;
        private bool _italic;
        private bool _underline;
        private bool _strikethrough;
        private TextSpacing _spacing;
        private TextAlignment _alignment;

        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public System.Windows.Media.Color FontColor
        {
            get => _fontColor;
            set => SetProperty(ref _fontColor, value);
        }

        public bool Bold
        {
            get => _bold;
            set => SetProperty(ref _bold, value);
        }

        public bool Italic
        {
            get => _italic;
            set => SetProperty(ref _italic, value);
        }

        public bool Underline
        {
            get => _underline;
            set => SetProperty(ref _underline, value);
        }

        public bool Strikethrough
        {
            get => _strikethrough;
            set => SetProperty(ref _strikethrough, value);
        }

        public TextSpacing Spacing
        {
            get => _spacing ??= new TextSpacing();
            set => SetProperty(ref _spacing, value);
        }

        public TextAlignment Alignment
        {
            get => _alignment;
            set => SetProperty(ref _alignment, value);
        }

        public TextFormat()
        {
            _fontFamily = "Calibri";
            _fontSize = 18;
            _fontColor = System.Windows.Media.Colors.Black;
            _spacing = new TextSpacing();
        }
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    public class TextSpacing : ModelBase
    {
        private double _before;
        private double _after;
        private int _lineSpacing;
        private TextSpacingRule _rule;

        public double Before
        {
            get => _before;
            set => SetProperty(ref _before, value);
        }

        public double After
        {
            get => _after;
            set => SetProperty(ref _after, value);
        }

        public int LineSpacing
        {
            get => _lineSpacing;
            set => SetProperty(ref _lineSpacing, value);
        }

        public TextSpacingRule Rule
        {
            get => _rule;
            set => SetProperty(ref _rule, value);
        }

        public TextSpacing()
        {
            _rule = TextSpacingRule.Auto;
        }
    }

    public enum TextSpacingRule
    {
        Auto,
        Multiple,
        Exact
    }

    public class TextBody : ModelBase
    {
        private MarginSettings _margins;
        private bool _wordWrap;
        private VerticalAnchor _anchor;
        private TextWrapping _wrapping;
        private AutoFitSettings _autoFit;

        public MarginSettings Margins
        {
            get => _margins ??= new MarginSettings();
            set => SetProperty(ref _margins, value);
        }

        public bool WordWrap
        {
            get => _wordWrap;
            set => SetProperty(ref _wordWrap, value);
        }

        public VerticalAnchor Anchor
        {
            get => _anchor;
            set => SetProperty(ref _anchor, value);
        }

        public TextWrapping Wrapping
        {
            get => _wrapping;
            set => SetProperty(ref _wrapping, value);
        }

        public AutoFitSettings AutoFit
        {
            get => _autoFit ??= new AutoFitSettings();
            set => SetProperty(ref _autoFit, value);
        }

        public TextBody()
        {
            _margins = new MarginSettings();
            _wordWrap = true;
            _anchor = VerticalAnchor.Top;
            _autoFit = new AutoFitSettings();
        }
    }

    public class MarginSettings : ModelBase
    {
        private double _left;
        private double _right;
        private double _top;
        private double _bottom;

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Right
        {
            get => _right;
            set => SetProperty(ref _right, value);
        }

        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        public double Bottom
        {
            get => _bottom;
            set => SetProperty(ref _bottom, value);
        }
    }

    public enum VerticalAnchor
    {
        Top,
        Middle,
        Bottom
    }

    public enum TextWrapping
    {
        Wrap,
        NoWrap,
        ShrinkToFit
    }

    public class AutoFitSettings : ModelBase
    {
        private AutoFitMode _mode;
        private double _minFontSize;
        private double _maxFontSize;

        public AutoFitMode Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        public double MinFontSize
        {
            get => _minFontSize;
            set => SetProperty(ref _minFontSize, value);
        }

        public double MaxFontSize
        {
            get => _maxFontSize;
            set => SetProperty(ref _maxFontSize, value);
        }

        public AutoFitSettings()
        {
            _mode = AutoFitMode.Normal;
            _minFontSize = 8;
            _maxFontSize = 96;
        }
    }

    public enum AutoFitMode
    {
        Normal,
        ShrinkToFit,
        AutoGrow
    }

    public class ParagraphCollection : List<TextParagraph> { }

    public class TextParagraph : ModelBase
    {
        private string _text;
        private TextFormat _format;
        private List<TextRun> _runs;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public TextFormat Format
        {
            get => _format ??= new TextFormat();
            set => SetProperty(ref _format, value);
        }

        public List<TextRun> Runs
        {
            get => _runs ??= new List<TextRun>();
            set => SetProperty(ref _runs, value);
        }
    }

    public class TextRun : ModelBase
    {
        private string _text;
        private TextFormat _format;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public TextFormat Format
        {
            get => _format ??= new TextFormat();
            set => SetProperty(ref _format, value);
        }
    }

    public class ParagraphCollection : List<Paragraph> { }

    public class Paragraph : ModelBase
    {
        private string _text;
        private TextFormat _format;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public TextFormat Format
        {
            get => _format ??= new TextFormat();
            set => SetProperty(ref _format, value);
        }
    }
}

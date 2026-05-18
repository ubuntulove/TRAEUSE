using System;
using System.Collections.Generic;
using OfficeSuite.Core.Models;

namespace OfficeSuite.Presentation.Models
{
    public class PresentationSettings : ModelBase
    {
        private SlideSize _slideSize;
        private int _slideWidth;
        private int _slideHeight;
        private string _name;
        private AuthorInfo _author;
        private string _subject;
        private string _keywords;
        private string _category;
        private string _comments;
        private DateTime _createdDate;
        private DateTime _modifiedDate;
        private int _revision;
        private bool _showPresentation;
        private bool _loopUntilEsc;
        private bool _showNarration;
        private bool _showPlayerControls;
        private bool _usePen;
        private bool _useLaserPointer;
        private bool _showScrollbar;
        private bool _showAnimation;

        public SlideSize SlideSize
        {
            get => _slideSize;
            set => SetProperty(ref _slideSize, value);
        }

        public int SlideWidth
        {
            get => _slideWidth;
            set => SetProperty(ref _slideWidth, value);
        }

        public int SlideHeight
        {
            get => _slideHeight;
            set => SetProperty(ref _slideHeight, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public AuthorInfo Author
        {
            get => _author ??= new AuthorInfo();
            set => SetProperty(ref _author, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Keywords
        {
            get => _keywords;
            set => SetProperty(ref _keywords, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set => SetProperty(ref _modifiedDate, value);
        }

        public int Revision
        {
            get => _revision;
            set => SetProperty(ref _revision, value);
        }

        public bool ShowPresentation
        {
            get => _showPresentation;
            set => SetProperty(ref _showPresentation, value);
        }

        public bool LoopUntilEsc
        {
            get => _loopUntilEsc;
            set => SetProperty(ref _loopUntilEsc, value);
        }

        public bool ShowNarration
        {
            get => _showNarration;
            set => SetProperty(ref _showNarration, value);
        }

        public bool ShowPlayerControls
        {
            get => _showPlayerControls;
            set => SetProperty(ref _showPlayerControls, value);
        }

        public bool UsePen
        {
            get => _usePen;
            set => SetProperty(ref _usePen, value);
        }

        public bool UseLaserPointer
        {
            get => _useLaserPointer;
            set => SetProperty(ref _useLaserPointer, value);
        }

        public bool ShowScrollbar
        {
            get => _showScrollbar;
            set => SetProperty(ref _showScrollbar, value);
        }

        public bool ShowAnimation
        {
            get => _showAnimation;
            set => SetProperty(ref _showAnimation, value);
        }

        public PresentationSettings()
        {
            _slideSize = SlideSize.OnScreen16x9;
            _slideWidth = 960;
            _slideHeight = 540;
            _createdDate = DateTime.Now;
            _modifiedDate = DateTime.Now;
            _showPlayerControls = true;
            _showAnimation = true;
        }
    }

    public enum SlideSize
    {
        Custom,
        OnScreen16x9,
        OnScreen16x10,
        Letter,
        A4,
        Widescreen16x9,
        Widescreen16x10,
        Overhead,
        Banner,
        BusinessCard
    }

    public class AuthorInfo : ModelBase
    {
        private string _name;
        private string _email;
        private string _company;
        private string _title;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }

    public class DocumentProperties : ModelBase
    {
        private string _title;
        private string _subject;
        private string _author;
        private string _keywords;
        private string _category;
        private string _comments;
        private string _lastSavedBy;
        private DateTime _createdDate;
        private DateTime _modifiedDate;
        private DateTime _lastPrintedDate;
        private int _Revision;
        private string _contentStatus;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Keywords
        {
            get => _keywords;
            set => SetProperty(ref _keywords, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        public string LastSavedBy
        {
            get => _lastSavedBy;
            set => SetProperty(ref _lastSavedBy, value);
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set => SetProperty(ref _modifiedDate, value);
        }

        public DateTime LastPrintedDate
        {
            get => _lastPrintedDate;
            set => SetProperty(ref _lastPrintedDate, value);
        }

        public int Revision
        {
            get => _Revision;
            set => SetProperty(ref _Revision, value);
        }

        public string ContentStatus
        {
            get => _contentStatus;
            set => SetProperty(ref _contentStatus, value);
        }

        public DocumentProperties()
        {
            _createdDate = DateTime.Now;
            _modifiedDate = DateTime.Now;
        }
    }

    public class SlideLayout : ModelBase
    {
        private string _id;
        private string _name;
        private SlideLayoutType _type;
        private List<LayoutPlaceholder> _placeholders;
        private SlideBackground _background;

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

        public SlideLayoutType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public List<LayoutPlaceholder> Placeholders
        {
            get => _placeholders ??= new List<LayoutPlaceholder>();
            set => SetProperty(ref _placeholders, value);
        }

        public SlideBackground Background
        {
            get => _background ??= new SlideBackground();
            set => SetProperty(ref _background, value);
        }

        public SlideLayout()
        {
            _id = Guid.NewGuid().ToString();
            _placeholders = new List<LayoutPlaceholder>();
        }
    }

    public enum SlideLayoutType
    {
        Title,
        TitleAndContent,
        TwoContent,
        Comparison,
        TitleOnly,
        Blank,
        ContentWithCaption,
        PictureWithCaption,
        TitleAndCaption,
        VerticalTitleAndText,
        VerticalTwoContentTop,
        VerticalTwoContentBottom
    }

    public class LayoutPlaceholder : ModelBase
    {
        private PlaceholderType _type;
        private int _index;
        private double _left;
        private double _top;
        private double _width;
        private double _height;
        private bool _isVisible;

        public PlaceholderType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
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

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
    }

    public enum PlaceholderType
    {
        Title,
        Body,
        CenterTitle,
        Subtitle,
        Date,
        Footer,
        SlideNumber,
        Picture,
        ClipArt,
        Media,
        Chart,
        Table,
        Diagram,
        SmartArt
    }

    public class SlideBackground : ModelBase
    {
        private BackgroundType _type;
        private FillFormat _fill;
        private PictureBackground _picture;
        private GradientBackground _gradient;
        private bool _isHidden;

        public BackgroundType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public FillFormat Fill
        {
            get => _fill ??= new FillFormat();
            set => SetProperty(ref _fill, value);
        }

        public PictureBackground Picture
        {
            get => _picture;
            set => SetProperty(ref _picture, value);
        }

        public GradientBackground Gradient
        {
            get => _gradient;
            set => SetProperty(ref _gradient, value);
        }

        public bool IsHidden
        {
            get => _isHidden;
            set => SetProperty(ref _isHidden, value);
        }

        public SlideBackground()
        {
            _type = BackgroundType.Solid;
            _fill = new FillFormat();
        }
    }

    public enum BackgroundType
    {
        None,
        Solid,
        Gradient,
        Picture,
        Texture
    }

    public class PictureBackground : ModelBase
    {
        private byte[] _imageData;
        private string _imagePath;
        private double _offsetX;
        private double _offsetY;
        private double _scaleX;
        private double _scaleY;
        private bool _isTransparent;

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

        public bool IsTransparent
        {
            get => _isTransparent;
            set => SetProperty(ref _isTransparent, value);
        }
    }

    public class GradientBackground : ModelBase
    {
        private GradientType _type;
        private GradientStopCollection _stops;
        private double _angle;
        private bool _isRotatedWithShape;

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

        public bool IsRotatedWithShape
        {
            get => _isRotatedWithShape;
            set => SetProperty(ref _isRotatedWithShape, value);
        }

        public GradientBackground()
        {
            _stops = new GradientStopCollection();
        }
    }

    public class Transition : ModelBase
    {
        private TransitionType _type;
        private TransitionSpeed _speed;
        private double _duration;
        private bool _autoReverse;
        private bool _soundEnabled;
        private string _soundPath;
        private bool _loopUntilNext;
        private int _triggerType;
        private int _triggerDelay;

        public TransitionType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public TransitionSpeed Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public bool AutoReverse
        {
            get => _autoReverse;
            set => SetProperty(ref _autoReverse, value);
        }

        public bool SoundEnabled
        {
            get => _soundEnabled;
            set => SetProperty(ref _soundEnabled, value);
        }

        public string SoundPath
        {
            get => _soundPath;
            set => SetProperty(ref _soundPath, value);
        }

        public bool LoopUntilNext
        {
            get => _loopUntilNext;
            set => SetProperty(ref _loopUntilNext, value);
        }

        public int TriggerType
        {
            get => _triggerType;
            set => SetProperty(ref _triggerType, value);
        }

        public int TriggerDelay
        {
            get => _triggerDelay;
            set => SetProperty(ref _triggerDelay, value);
        }

        public Transition()
        {
            _type = TransitionType.None;
            _speed = TransitionSpeed.Medium;
            _duration = 1.0;
        }
    }

    public enum TransitionType
    {
        None,
        Cut,
        Fade,
        FadeSmoothly,
        Push,
        Wipe,
        Split,
        Reveal,
        Cover,
        Blinds,
        Checkboard,
        Crawl,
        CutThroughBlack,
        Dissolve,
        Flash,
        Fly,
        PageCurl,
        PageTurn,
        Prestretched,
        RandomBar,
        kenBurns,
        Cycle,
        Zoom
    }

    public enum TransitionSpeed
    {
        Slow,
        Medium,
        Fast
    }

    public class Animation : ModelBase
    {
        private string _id;
        private AnimationType _type;
        private AnimationTrigger _trigger;
        private double _duration;
        private double _delay;
        private AnimationSpeed _speed;
        private AnimationEffect _effect;
        private AnimationDirection _direction;
        private bool _autoReverse;
        private int _repeatCount;
        private AnimationTiming _timing;
        private List<AnimationStep> _steps;
        private AnimationTarget _target;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public AnimationType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public AnimationTrigger Trigger
        {
            get => _trigger;
            set => SetProperty(ref _trigger, value);
        }

        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public double Delay
        {
            get => _delay;
            set => SetProperty(ref _delay, value);
        }

        public AnimationSpeed Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        public AnimationEffect Effect
        {
            get => _effect;
            set => SetProperty(ref _effect, value);
        }

        public AnimationDirection Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        public bool AutoReverse
        {
            get => _autoReverse;
            set => SetProperty(ref _autoReverse, value);
        }

        public int RepeatCount
        {
            get => _repeatCount;
            set => SetProperty(ref _repeatCount, value);
        }

        public AnimationTiming Timing
        {
            get => _timing;
            set => SetProperty(ref _timing, value);
        }

        public List<AnimationStep> Steps
        {
            get => _steps ??= new List<AnimationStep>();
            set => SetProperty(ref _steps, value);
        }

        public AnimationTarget Target
        {
            get => _target;
            set => SetProperty(ref _target, value);
        }

        public Animation()
        {
            _id = Guid.NewGuid().ToString();
            _duration = 1.0;
            _speed = AnimationSpeed.Medium;
            _trigger = AnimationTrigger.OnClick;
            _timing = AnimationTiming.Linear;
            _steps = new List<AnimationStep>();
        }
    }

    public enum AnimationType
    {
        Entrance,
        Exit,
        Emphasis,
        MotionPath
    }

    public enum AnimationTrigger
    {
        OnClick,
        AfterPrevious,
        WithPrevious
    }

    public enum AnimationSpeed
    {
        VerySlow,
        Slow,
        Medium,
        Fast,
        VeryFast
    }

    public enum AnimationEffect
    {
        None,
        Appear,
        Fade,
        Fly,
        Wipe,
        Zoom,
        GrowAndTurn,
        Swivel,
        Bounce,
        Float,
        Spin,
        Crawl,
        Blinds,
        Checkboard,
        Dissolve,
        Flash,
        Unfold,
        Split,
        Strech,
        ShelbyFade,
        RiseUp,
        Ascend,
        Descend,
        Wheel,
        Fan,
        Grow,
        Pinwheel,
        Orbit,
        ArchDown,
        BridgeOver,
        BustUp,
        ByteOut,
        Camera,
        Candle,
        Cargo,
        CenterRevolve,
        Century,
        Chandelier,
        Chart,
        Choppy,
        Collapse,
        Compress,
        ContrarClockwiseTurn,
        CounterclockwiseTurn,
        CreditRoll,
        Crop,
        Cube,
        Custom,
        Decode,
        Diamond,
        Dispatcher,
        Expand,
        Explode,
        Fade,
        FadeToLevel,
        FadedZoom,
        Flicker,
        Flip,
        FloatDown,
        FloatUp,
        FlyIn,
        FlyInBottom,
        FlyInLeft,
        FlyInRight,
        FlyInTop,
        FlyOut,
        Fold,
        FollowPath,
        Frighten,
        Galaxy,
        Glide,
        GrowAndTurn,
        GrowTo,
        GrowShrink,
        LightSpeed,
        Magnify,
        Morph,
        OrbitDown,
        OrbitLeft,
        OrbitRight,
        OrbitUp,
        Origami,
        Outline,
        Overage,
        PeekIn,
        PeekOut,
        Pinwheel,
        Poof,
        Pop,
        Pulse,
        RandomBars,
        RandomEffects,
        RiseUp,
        Ripple,
        RollBottom,
        RollDown,
        RollLeft,
        RollRight,
        RollTop,
        ScaleDown,
        Scribble,
        Shanghai,
        Sharpen,
        Sheen,
        ShrinkTo,
        SimulateBullets,
        SizeDown,
        SlideDown,
        SlideInDown,
        SlideInLeft,
        SlideInRight,
        SlideInUp,
        SlideOutBottom,
        SlideOutLeft,
        SlideOutRight,
        SlideOutUp,
        SlideUp,
        Spin,
        SpinIn,
        SpinOut,
        Spirals,
        Splash,
        Spread,
        Stretch,
        StripsDown,
        StripsLeft,
        StripsRight,
        StripsUp,
        StylePulses,
        Swallow,
        Swivel,
        ThickTo,
        ThirdPersonFlyIn,
        ThirdPersonFlyInAlternate,
        ThirdPersonFlyOut,
        ThirdPersonFlyOutAlternate,
        ThirdPersonPath,
        ThreePoint,
        Thunder,
        Tiled,
        Timeline,
        TopLeft,
        TopRight,
        TracePath,
        Transparent,
        TurnDown,
        TwistAndTurn,
        UncoverDown,
        UncoverLeft,
        UncoverRight,
        UncoverUp,
        Underline,
        Up,
        VerticalGrowth,
        Wedge,
        Wheel,
        WheelReverse,
        Whip,
        Zoom,
        ZoomIn,
        ZoomOut
    }

    public enum AnimationDirection
    {
        None,
        In,
        Out,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Up,
        Down,
        Horizontal,
        Vertical,
        Clockwise,
        Counterclockwise
    }

    public enum AnimationTiming
    {
        Linear,
        Discrete,
        Inner,
        Outer,
        EaseIn,
        EaseOut,
        EaseInOut,
        Lighter,
        LighterTo,
        Darker,
        DarkerTo
    }

    public class AnimationStep : ModelBase
    {
        private int _index;
        private AnimationEffect _effect;
        private AnimationDirection _direction;
        private double _duration;
        private double _delay;

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public AnimationEffect Effect
        {
            get => _effect;
            set => SetProperty(ref _effect, value);
        }

        public AnimationDirection Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public double Delay
        {
            get => _delay;
            set => SetProperty(ref _delay, value);
        }
    }

    public class AnimationTarget : ModelBase
    {
        private string _shapeId;
        private TargetType _type;
        private string _property;

        public string ShapeId
        {
            get => _shapeId;
            set => SetProperty(ref _shapeId, value);
        }

        public TargetType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string Property
        {
            get => _property;
            set => SetProperty(ref _property, value);
        }
    }

    public enum TargetType
    {
        Shape,
        Text,
        Audio,
        Video,
        Group
    }

    public class SlideMaster : ModelBase
    {
        private string _id;
        private string _name;
        private SlideBackground _background;
        private List<SlideLayout> _layouts;
        private List<SlideShape> _shapes;
        private HeaderFooter _header;
        private HeaderFooter _footer;
        private PageNumber _pageNumber;
        private DateTime _createdDate;

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

        public SlideBackground Background
        {
            get => _background ??= new SlideBackground();
            set => SetProperty(ref _background, value);
        }

        public List<SlideLayout> Layouts
        {
            get => _layouts ??= new List<SlideLayout>();
            set => SetProperty(ref _layouts, value);
        }

        public List<SlideShape> Shapes
        {
            get => _shapes ??= new List<SlideShape>();
            set => SetProperty(ref _shapes, value);
        }

        public HeaderFooter Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public HeaderFooter Footer
        {
            get => _footer;
            set => SetProperty(ref _footer, value);
        }

        public PageNumber PageNumber
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public SlideMaster()
        {
            _id = Guid.NewGuid().ToString();
            _name = "Slide Master";
            _createdDate = DateTime.Now;
        }
    }

    public class HeaderFooter : ModelBase
    {
        private string _text;
        private bool _isVisible;
        private bool _isPlaceholder;
        private TextFormat _format;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsPlaceholder
        {
            get => _isPlaceholder;
            set => SetProperty(ref _isPlaceholder, value);
        }

        public TextFormat Format
        {
            get => _format ??= new TextFormat();
            set => SetProperty(ref _format, value);
        }
    }

    public class PageNumber : ModelBase
    {
        private bool _isVisible;
        private NumberFormat _format;
        private TextFormat _format;

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public NumberFormat Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        public TextFormat TextFormat
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }
    }

    public enum NumberFormat
    {
        Arabic,
        Arabic1,
        Arabic2,
        ArabicLeadingZero,
        AlphaLcPeriod,
        AlphaUcPeriod,
        AlphaLcParenR,
        AlphaUcParenR,
        AlphaLcParenBoth,
        AlphaUcParenBoth,
        ArabicParenR,
        ArabicParenBoth,
        ArabicDot,
        ArabicDash,
        RomanLcPeriod,
        RomanUcPeriod,
        RomanLcParenR,
        RomanUcParenR,
        RomanLcParenBoth,
        RomanUcParenBoth,
        CircleNumber,
        CircleNumberDouble,
        CircleNumberTriple,
        CircleNumberKorean,
        CircleNumberKoreanCont,
        ArabicDbPeriod,
        ArabicDbPlain,
        ArabicMinus,
        ArabicEqual,
        ArabicDbChar,
        ThaiLetter,
        ThaiArabic,
        ThaiChar,
        HindiLetter,
        HindiArabic,
        HindiChar,
        Arabic1,
        Arabic2,
        ArabicHard,
        HindiLetter1,
        HindiLetter2,
        HindiArabic1,
        HindiArabic2,
        ArabicDash,
        DollarSign,
        DollarSignL,
        DollarSignR,
        DollarSignLR,
        DollarSignDBL,
        Parentheses,
        ParenthesesColNeg,
        ParenthesesRed,
        ParenthesesColNegRed,
        PoundSign,
        PoundSignL,
        PoundSignR,
        PoundSignLR,
        EuroSign,
        EuroSignL,
        EuroSignR,
        EuroSignLR,
        EuroSignDBL,
        YellowCircle,
        YellowCircleL,
        YellowCircleR,
        YellowCircleLR,
        YellowCircleDBL,
        GreenCircle,
        GreenCircleL,
        GreenCircleR,
        GreenCircleLR,
        GreenCircleDBL,
        BlueCircle,
        BlueCircleL,
        BlueCircleR,
        BlueCircleLR,
        BlueCircleDBL,
        RedCircle,
        RedCircleL,
        RedCircleR,
        RedCircleLR,
        RedCircleDBL,
        LightBlueCircle,
        LightBlueCircleL,
        LightBlueCircleR,
        LightBlueCircleLR,
        LightBlueCircleDBL,
        PurpleCircle,
        PurpleCircleL,
        PurpleCircleR,
        PurpleCircleLR,
        PurpleCircleDBL
    }

    public class TextFormat
    {
        private string _fontFamily { get; set; }
        private double _fontSize { get; set; }
        private System.Windows.Media.Color _fontColor { get; set; }
        private bool _bold { get; set; }
        private bool _italic { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace OfficeSuite.Presentation.Masters
{
    public class SlideMasterView : INotifyPropertyChanged
    {
        private string _name = "Slide Master";
        private string _title = "Slide Master";
        private string _backgroundColor = "#FFFFFF";
        private bool _showBackground;
        private SlideLayoutType _layoutType = SlideLayoutType.TitleAndContent;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MasterShapeView> BackgroundShapes { get; } = new();
        public ObservableCollection<PlaceholderView> Placeholders { get; } = new();
        public ObservableCollection<MasterTimingView> Timings { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public bool ShowBackground
        {
            get => _showBackground;
            set { _showBackground = value; OnPropertyChanged(nameof(ShowBackground)); }
        }

        public SlideLayoutType LayoutType
        {
            get => _layoutType;
            set { _layoutType = value; OnPropertyChanged(nameof(LayoutType)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MasterShapeView : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "Shape";
        private ShapeType _shapeType = ShapeType.Rectangle;
        private double _left;
        private double _top;
        private double _width = 100;
        private double _height = 100;
        private string _fillColor = "#4472C4";
        private string _lineColor = "#000000";
        private bool _isVisible = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public ShapeType ShapeType
        {
            get => _shapeType;
            set { _shapeType = value; OnPropertyChanged(nameof(ShapeType)); }
        }

        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(nameof(Left)); }
        }

        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(nameof(Top)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
        }

        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(nameof(Height)); }
        }

        public string FillColor
        {
            get => _fillColor;
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
        }

        public string LineColor
        {
            get => _lineColor;
            set { _lineColor = value; OnPropertyChanged(nameof(LineColor)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PlaceholderView : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private PlaceholderType _placeholderType = PlaceholderType.Content;
        private double _left;
        private double _top;
        private double _width = 200;
        private double _height = 100;
        private string _text = string.Empty;
        private bool _isVisible = true;
        private bool _isLocked;
        private bool _showInLayout = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public PlaceholderType PlaceholderType
        {
            get => _placeholderType;
            set { _placeholderType = value; OnPropertyChanged(nameof(PlaceholderType)); }
        }

        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(nameof(Left)); }
        }

        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(nameof(Top)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
        }

        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(nameof(Height)); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(nameof(IsLocked)); }
        }

        public bool ShowInLayout
        {
            get => _showInLayout;
            set { _showInLayout = value; OnPropertyChanged(nameof(ShowInLayout)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MasterTimingView : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "Timing";
        private MasterTimingType _timingType = MasterTimingType.OnClick;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public MasterTimingType TimingType
        {
            get => _timingType;
            set { _timingType = value; OnPropertyChanged(nameof(TimingType)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum SlideLayoutType
    {
        TitleSlide,
        TitleAndContent,
        TwoContent,
        Comparison,
        TitleOnly,
        SectionHeader,
        PictureWithCaption,
        Blank,
        Custom
    }

    public enum PlaceholderType
    {
        Title,
        Subtitle,
        Body,
        Content,
        Table,
        Chart,
        Picture,
        ClipArt,
        Diagram,
        Media,
        Header,
        Footer,
        SlideNumber,
        DateTime,
        FooterPlaceholder,
        HeaderPlaceholder
    }

    public enum ShapeType
    {
        Rectangle,
        RoundedRectangle,
        Ellipse,
        Circle,
        Triangle,
        Diamond,
        Pentagon,
        Hexagon,
        Line,
        Arrow,
        Connector,
        TextBox,
        Picture,
        Chart,
        Table,
        SmartArt
    }

    public enum MasterTimingType
    {
        OnClick,
        WithPrevious,
        AfterPrevious
    }

    public class HandoutMasterView : INotifyPropertyChanged
    {
        private string _name = "Handout Master";
        private HandoutLayout _handoutLayout = HandoutLayout.TwoByTwo;
        private string _headerText = string.Empty;
        private string _footerText = string.Empty;
        private string _backgroundColor = "#FFFFFF";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public HandoutLayout HandoutLayout
        {
            get => _handoutLayout;
            set { _handoutLayout = value; OnPropertyChanged(nameof(HandoutLayout)); }
        }

        public string HeaderText
        {
            get => _headerText;
            set { _headerText = value; OnPropertyChanged(nameof(HeaderText)); }
        }

        public string FooterText
        {
            get => _footerText;
            set { _footerText = value; OnPropertyChanged(nameof(FooterText)); }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum HandoutLayout
    {
        OneSlidePerPage,
        TwoByOne,
        TwoByTwo,
        ThreeByThree,
        FourByThree,
        FourByFour,
        SixByThree,
        SixByFour,
        NineByNine
    }

    public class NotesMasterView : INotifyPropertyChanged
    {
        private string _name = "Notes Master";
        private string _backgroundColor = "#FFFFFF";
        private string _headerText = string.Empty;
        private string _footerText = string.Empty;
        private string _pageNumberFormat = "#";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public string HeaderText
        {
            get => _headerText;
            set { _headerText = value; OnPropertyChanged(nameof(HeaderText)); }
        }

        public string FooterText
        {
            get => _footerText;
            set { _footerText = value; OnPropertyChanged(nameof(FooterText)); }
        }

        public string PageNumberFormat
        {
            get => _pageNumberFormat;
            set { _pageNumberFormat = value; OnPropertyChanged(nameof(PageNumberFormat)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThemeView : INotifyPropertyChanged
    {
        private string _name = "Office Theme";
        private string _themeColorSet = "Office";
        private string _backgroundColor = "#FFFFFF";
        private bool _isCustom;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ThemeColorView> Colors { get; } = new();
        public ObservableCollection<ThemeFontView> Fonts { get; } = new();
        public ObservableCollection<ThemeEffectView> Effects { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string ThemeColorSet
        {
            get => _themeColorSet;
            set { _themeColorSet = value; OnPropertyChanged(nameof(ThemeColorSet)); }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public bool IsCustom
        {
            get => _isCustom;
            set { _isCustom = value; OnPropertyChanged(nameof(IsCustom)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThemeColorView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _color = "#000000";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThemeFontView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _headingFont = "Calibri";
        private string _bodyFont = "Calibri";
        private bool _isCustom;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string HeadingFont
        {
            get => _headingFont;
            set { _headingFont = value; OnPropertyChanged(nameof(HeadingFont)); }
        }

        public string BodyFont
        {
            get => _bodyFont;
            set { _bodyFont = value; OnPropertyChanged(nameof(BodyFont)); }
        }

        public bool IsCustom
        {
            get => _isCustom;
            set { _isCustom = value; OnPropertyChanged(nameof(IsCustom)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThemeEffectView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private EffectSetType _effectSet = EffectSetType.Subtle;
        private bool _isCustom;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public EffectSetType EffectSet
        {
            get => _effectSet;
            set { _effectSet = value; OnPropertyChanged(nameof(EffectSet)); }
        }

        public bool IsCustom
        {
            get => _isCustom;
            set { _isCustom = value; OnPropertyChanged(nameof(IsCustom)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum EffectSetType
    {
        Subtle,
        Moderate,
        Intense,
        None,
        Custom
    }

    public class TransitionGalleryView : INotifyPropertyChanged
    {
        private TransitionType _selectedTransition = TransitionType.Fade;
        private double _duration = 1.0;
        private TransitionSound _sound = TransitionSound.None;
        private bool _loopSoundUntilNext;
        private bool _applyToAll;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TransitionType SelectedTransition
        {
            get => _selectedTransition;
            set { _selectedTransition = value; OnPropertyChanged(nameof(SelectedTransition)); }
        }

        public double Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(nameof(Duration)); }
        }

        public TransitionSound Sound
        {
            get => _sound;
            set { _sound = value; OnPropertyChanged(nameof(Sound)); }
        }

        public bool LoopSoundUntilNext
        {
            get => _loopSoundUntilNext;
            set { _loopSoundUntilNext = value; OnPropertyChanged(nameof(LoopSoundUntilNext)); }
        }

        public bool ApplyToAll
        {
            get => _applyToAll;
            set { _applyToAll = value; OnPropertyChanged(nameof(ApplyToAll)); }
        }

        public ObservableCollection<TransitionOption> Options { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TransitionOption
    {
        public string Name { get; set; } = string.Empty;
        public TransitionType Type { get; set; }
        public string Preview { get; set; } = string.Empty;
    }

    public enum TransitionType
    {
        None,
        Fade,
        Push,
        Wipe,
        Split,
        Reveal,
        Blinds,
        Box,
        Cover,
        Curtain,
        Dissolve,
        Flash,
        Flip,
        Fly,
        Glitter,
        Morph,
        Orbit,
        RevealSmoothly,
        Ripple,
        Shatter,
        Sparkle,
        Swivel,
        Thumbnail,
        Wheel,
        Zoom
    }

    public enum TransitionSound
    {
        None,
        Click,
        ChaChing,
        Wind,
        Laser,
        Typewriter,
        Applause,
        Cannon,
        Explosion,
        Custom
    }

    public class AnimationGalleryView : INotifyPropertyChanged
    {
        private AnimationEffectType _selectedEffect = AnimationEffectType.Fade;
        private AnimationTiming _timing = AnimationTiming.OnClick;
        private AnimationDirection _direction = AnimationDirection.In;
        private double _duration = 1.0;
        private double _delay;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AnimationEffectType SelectedEffect
        {
            get => _selectedEffect;
            set { _selectedEffect = value; OnPropertyChanged(nameof(SelectedEffect)); }
        }

        public AnimationTiming Timing
        {
            get => _timing;
            set { _timing = value; OnPropertyChanged(nameof(Timing)); }
        }

        public AnimationDirection Direction
        {
            get => _direction;
            set { _direction = value; OnPropertyChanged(nameof(Direction)); }
        }

        public double Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(nameof(Duration)); }
        }

        public double Delay
        {
            get => _delay;
            set { _delay = value; OnPropertyChanged(nameof(Delay)); }
        }

        public ObservableCollection<AnimationPresetOption> EntrancePresets { get; } = new();
        public ObservableCollection<AnimationPresetOption> EmphasisPresets { get; } = new();
        public ObservableCollection<AnimationPresetOption> ExitPresets { get; } = new();
        public ObservableCollection<AnimationPresetOption> MotionPaths { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnimationPresetOption
    {
        public string Name { get; set; } = string.Empty;
        public AnimationEffectType EffectType { get; set; }
        public AnimationPresetCategory Category { get; set; }
        public string Preview { get; set; } = string.Empty;
    }

    public enum AnimationPresetCategory
    {
        Entrance,
        Emphasis,
        Exit,
        MotionPath
    }

    public enum AnimationEffectType
    {
        None,
        Appear,
        Fade,
        FadeIn,
        FadeOut,
        FlyIn,
        FlyOut,
        FlyInLeft,
        FlyInRight,
        FlyInUp,
        FlyInDown,
        Wipe,
        Blinds,
        Box,
        Checkerboard,
        Circle,
        Crawl,
        Diamond,
        Figure8,
        Flash,
        FlashOnce,
        Flip,
        Float,
        Grow,
        GrowAndTurn,
        LightSpeed,
        Orbit,
        Pinwheel,
        Plus,
        RandomBars,
        Reveal,
        Ripple,
        Roll,
        Shrink,
        Spin,
        Swivel,
        Transparency,
        Zoom,
        BrushOnColor,
        ColorEmphasis,
        Complement,
        Contrast,
        Desaturate,
        Recolor,
        Shade,
        Tone,
        Highlight,
        GrowWithColor,
        Darken,
        Lighten,
        FlashBulb,
        Flicker,
        Pulse
    }

    public enum AnimationTiming
    {
        OnClick,
        AfterPrevious,
        WithPrevious,
        Automatic
    }

    public enum AnimationDirection
    {
        In,
        Out,
        None,
        Up,
        Down,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center,
        Horizontal,
        Vertical,
        Clockwise,
        Counterclockwise,
        SlightLeft,
        SlightRight
    }

    public class SmartArtLayoutView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private SmartArtLayoutType _layoutType = SmartArtLayoutType.BasicList;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SmartArtNodeView> Nodes { get; } = new();
        public ObservableCollection<SmartArtConnectionView> Connections { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public SmartArtLayoutType LayoutType
        {
            get => _layoutType;
            set { _layoutType = value; OnPropertyChanged(nameof(LayoutType)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SmartArtNodeView : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _text = string.Empty;
        private int _level;
        private string _parentId = string.Empty;
        private double _left;
        private double _top;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        public int Level
        {
            get => _level;
            set { _level = value; OnPropertyChanged(nameof(Level)); }
        }

        public string ParentId
        {
            get => _parentId;
            set { _parentId = value; OnPropertyChanged(nameof(ParentId)); }
        }

        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(nameof(Left)); }
        }

        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(nameof(Top)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SmartArtConnectionView
    {
        public string FromNodeId { get; set; } = string.Empty;
        public string ToNodeId { get; set; } = string.Empty;
        public ConnectionStyle Style { get; set; } = ConnectionStyle.Straight;
    }

    public enum SmartArtLayoutType
    {
        BasicList,
        BulletList,
        NumberedList,
        Process,
        Cycle,
        Gear,
        PictureList,
        Pyramid,
        Target,
        Venn,
        OrganizationChart
    }

    public enum ConnectionStyle
    {
        Straight,
        Curved,
        Angle
    }

    public class VideoMediaView : INotifyPropertyChanged
    {
        private string _source = string.Empty;
        private string _posterFrame = string.Empty;
        private double _volume = 50;
        private bool _mute;
        private bool _loop;
        private VideoPlaybackMode _playbackMode = VideoPlaybackMode.OnClick;
        private double _startTime;
        private double _endTime;
        private double _fadeInDuration;
        private double _fadeOutDuration;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Source
        {
            get => _source;
            set { _source = value; OnPropertyChanged(nameof(Source)); }
        }

        public string PosterFrame
        {
            get => _posterFrame;
            set { _posterFrame = value; OnPropertyChanged(nameof(PosterFrame)); }
        }

        public double Volume
        {
            get => _volume;
            set { _volume = value; OnPropertyChanged(nameof(Volume)); }
        }

        public bool Mute
        {
            get => _mute;
            set { _mute = value; OnPropertyChanged(nameof(Mute)); }
        }

        public bool Loop
        {
            get => _loop;
            set { _loop = value; OnPropertyChanged(nameof(Loop)); }
        }

        public VideoPlaybackMode PlaybackMode
        {
            get => _playbackMode;
            set { _playbackMode = value; OnPropertyChanged(nameof(PlaybackMode)); }
        }

        public double StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        public double EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        public double FadeInDuration
        {
            get => _fadeInDuration;
            set { _fadeInDuration = value; OnPropertyChanged(nameof(FadeInDuration)); }
        }

        public double FadeOutDuration
        {
            get => _fadeOutDuration;
            set { _fadeOutDuration = value; OnPropertyChanged(nameof(FadeOutDuration)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum VideoPlaybackMode
    {
        OnClick,
        WhenClicked,
        Automatically,
        AllSlides
    }

    public class AudioMediaView : INotifyPropertyChanged
    {
        private string _source = string.Empty;
        private double _volume = 50;
        private bool _mute;
        private bool _loop;
        private bool _hideDuringShow;
        private AudioPlaybackMode _playbackMode = AudioPlaybackMode.OnClick;
        private double _startTime;
        private double _endTime;
        private double _fadeInDuration;
        private double _fadeOutDuration;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Source
        {
            get => _source;
            set { _source = value; OnPropertyChanged(nameof(Source)); }
        }

        public double Volume
        {
            get => _volume;
            set { _volume = value; OnPropertyChanged(nameof(Volume)); }
        }

        public bool Mute
        {
            get => _mute;
            set { _mute = value; OnPropertyChanged(nameof(Mute)); }
        }

        public bool Loop
        {
            get => _loop;
            set { _loop = value; OnPropertyChanged(nameof(Loop)); }
        }

        public bool HideDuringShow
        {
            get => _hideDuringShow;
            set { _hideDuringShow = value; OnPropertyChanged(nameof(HideDuringShow)); }
        }

        public AudioPlaybackMode PlaybackMode
        {
            get => _playbackMode;
            set { _playbackMode = value; OnPropertyChanged(nameof(PlaybackMode)); }
        }

        public double StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        public double EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        public double FadeInDuration
        {
            get => _fadeInDuration;
            set { _fadeInDuration = value; OnPropertyChanged(nameof(FadeInDuration)); }
        }

        public double FadeOutDuration
        {
            get => _fadeOutDuration;
            set { _fadeOutDuration = value; OnPropertyChanged(nameof(FadeOutDuration)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum AudioPlaybackMode
    {
        OnClick,
        Automatically,
        AcrossSlides
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OfficeSuite.Presentation.Services
{
    public class PresentationDocumentService : IPresentationDocumentService
    {
        private readonly List<PresentationInfo> _recentPresentations;
        private PresentationInfo? _currentPresentation;

        public PresentationDocumentService()
        {
            _recentPresentations = new List<PresentationInfo>();
        }

        public PresentationInfo? CurrentPresentation => _currentPresentation;
        public IReadOnlyList<PresentationInfo> RecentPresentations => _recentPresentations.AsReadOnly();

        public Task<PresentationInfo> CreateAsync(string? fileName = null)
        {
            var presentation = new PresentationInfo
            {
                FileName = fileName ?? "Untitled.pptx",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            presentation.Slides.Add(new SlideInfo { Title = "Title Slide", Index = 0 });
            presentation.Slides.Add(new SlideInfo { Title = "Content Slide", Index = 1 });

            _currentPresentation = presentation;
            return Task.FromResult(presentation);
        }

        public Task<PresentationInfo> OpenAsync(string filePath)
        {
            var presentation = new PresentationInfo
            {
                FileName = System.IO.Path.GetFileName(filePath),
                FilePath = filePath,
                CreatedDate = System.IO.File.GetCreationTime(filePath),
                ModifiedDate = System.IO.File.GetLastWriteTime(filePath)
            };

            presentation.Slides.Add(new SlideInfo { Title = "Slide 1", Index = 0 });

            _currentPresentation = presentation;

            if (!_recentPresentations.Any(p => p.FilePath == filePath))
            {
                _recentPresentations.Insert(0, presentation);
                if (_recentPresentations.Count > 10)
                    _recentPresentations.RemoveAt(_recentPresentations.Count - 1);
            }

            return Task.FromResult(presentation);
        }

        public Task<bool> SaveAsync(PresentationInfo presentation, string? filePath = null)
        {
            if (filePath != null)
            {
                presentation.FilePath = filePath;
                presentation.FileName = System.IO.Path.GetFileName(filePath);
            }

            presentation.ModifiedDate = DateTime.Now;
            return Task.FromResult(true);
        }

        public Task<bool> CloseAsync(PresentationInfo presentation)
        {
            if (_currentPresentation == presentation)
            {
                _currentPresentation = null;
            }

            return Task.FromResult(true);
        }

        public Task<bool> ExportAsync(PresentationInfo presentation, PresentationExportFormat format, string filePath)
        {
            return Task.FromResult(true);
        }
    }

    public class PresentationInfo : INotifyPropertyChanged
    {
        private string _fileName = "Untitled.pptx";
        private string _filePath = string.Empty;
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;
        private string _author = string.Empty;
        private string _title = string.Empty;
        private string _subject = string.Empty;
        private string _keywords = string.Empty;
        private string _category = string.Empty;
        private string _comments = string.Empty;
        private string _Company = string.Empty;
        private string _manager = string.Empty;
        private bool _isModified;
        private int _slideWidth = 9144000;
        private int _slideHeight = 6858000;
        private PresentationDesignTheme? _theme;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SlideInfo> Slides { get; } = new();
        public ObservableCollection<SlideSection> Sections { get; } = new();
        public ObservableCollection<SlideMaster> SlideMasters { get; } = new();
        public ObservableCollection<NotesMaster> NotesMasters { get; } = new();
        public ObservableCollection<HandoutMaster> HandoutMasters { get; } = new();
        public ObservableCollection<PresentationTag> Tags { get; } = new();

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(nameof(FileName)); }
        }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { _createdDate = value; OnPropertyChanged(nameof(CreatedDate)); }
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set { _modifiedDate = value; OnPropertyChanged(nameof(ModifiedDate)); }
        }

        public string Author
        {
            get => _author;
            set { _author = value; OnPropertyChanged(nameof(Author)); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string Subject
        {
            get => _subject;
            set { _subject = value; OnPropertyChanged(nameof(Subject)); }
        }

        public string Keywords
        {
            get => _keywords;
            set { _keywords = value; OnPropertyChanged(nameof(Keywords)); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        public string Comments
        {
            get => _comments;
            set { _comments = value; OnPropertyChanged(nameof(Comments)); }
        }

        public string Company
        {
            get => _Company;
            set { _Company = value; OnPropertyChanged(nameof(Company)); }
        }

        public string Manager
        {
            get => _manager;
            set { _manager = value; OnPropertyChanged(nameof(Manager)); }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(nameof(IsModified)); }
        }

        public int SlideWidth
        {
            get => _slideWidth;
            set { _slideWidth = value; OnPropertyChanged(nameof(SlideWidth)); }
        }

        public int SlideHeight
        {
            get => _slideHeight;
            set { _slideHeight = value; OnPropertyChanged(nameof(SlideHeight)); }
        }

        public PresentationDesignTheme? Theme
        {
            get => _theme;
            set { _theme = value; OnPropertyChanged(nameof(Theme)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlideInfo : INotifyPropertyChanged
    {
        private string _title = "Slide";
        private int _index;
        private bool _isHidden;
        private bool _isSelected;
        private bool _showTransition;
        private TransitionInfo? _transition;
        private SlideLayoutType _layout = SlideLayoutType.Blank;
        private string _backgroundColor = "#FFFFFF";
        private string _slideNumberFormat = "#";
        private bool _showSlideNumber;
        private bool _showHeader;
        private bool _showFooter;
        private string _headerText = string.Empty;
        private string _footerText = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ShapeInfo> Shapes { get; } = new();
        public ObservableCollection<AnimationInfo> Animations { get; } = new();
        public ObservableCollection<SpeakerNote> Notes { get; } = new();
        public ObservableCollection<SlideTime> Timings { get; } = new();

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public bool IsHidden
        {
            get => _isHidden;
            set { _isHidden = value; OnPropertyChanged(nameof(IsHidden)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool ShowTransition
        {
            get => _showTransition;
            set { _showTransition = value; OnPropertyChanged(nameof(ShowTransition)); }
        }

        public TransitionInfo? Transition
        {
            get => _transition;
            set { _transition = value; OnPropertyChanged(nameof(Transition)); }
        }

        public SlideLayoutType Layout
        {
            get => _layout;
            set { _layout = value; OnPropertyChanged(nameof(Layout)); }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public string SlideNumberFormat
        {
            get => _slideNumberFormat;
            set { _slideNumberFormat = value; OnPropertyChanged(nameof(SlideNumberFormat)); }
        }

        public bool ShowSlideNumber
        {
            get => _showSlideNumber;
            set { _showSlideNumber = value; OnPropertyChanged(nameof(ShowSlideNumber)); }
        }

        public bool ShowHeader
        {
            get => _showHeader;
            set { _showHeader = value; OnPropertyChanged(nameof(ShowHeader)); }
        }

        public bool ShowFooter
        {
            get => _showFooter;
            set { _showFooter = value; OnPropertyChanged(nameof(ShowFooter)); }
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TransitionInfo : INotifyPropertyChanged
    {
        private TransitionType _type = TransitionType.None;
        private double _duration = 1.0;
        private TransitionSound _sound = TransitionSound.None;
        private string? _customSound;
        private bool _loopSoundUntilNext;
        private bool _automaticallyAfter;
        private double _autoDuration = 5.0;
        private bool _applyToAll;
        private string? _transitionModifier;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TransitionType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
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

        public string? CustomSound
        {
            get => _customSound;
            set { _customSound = value; OnPropertyChanged(nameof(CustomSound)); }
        }

        public bool LoopSoundUntilNext
        {
            get => _loopSoundUntilNext;
            set { _loopSoundUntilNext = value; OnPropertyChanged(nameof(LoopSoundUntilNext)); }
        }

        public bool AutomaticallyAfter
        {
            get => _automaticallyAfter;
            set { _automaticallyAfter = value; OnPropertyChanged(nameof(AutomaticallyAfter)); }
        }

        public double AutoDuration
        {
            get => _autoDuration;
            set { _autoDuration = value; OnPropertyChanged(nameof(AutoDuration)); }
        }

        public bool ApplyToAll
        {
            get => _applyToAll;
            set { _applyToAll = value; OnPropertyChanged(nameof(ApplyToAll)); }
        }

        public string? TransitionModifier
        {
            get => _transitionModifier;
            set { _transitionModifier = value; OnPropertyChanged(nameof(TransitionModifier)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ShapeInfo : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "Shape";
        private ShapeType _shapeType = ShapeType.Rectangle;
        private double _left;
        private double _top;
        private double _width = 100;
        private double _height = 100;
        private double _rotation;
        private string _fillColor = "#4472C4";
        private string _lineColor = "#000000";
        private double _lineWidth = 1;
        private double _opacity = 100;
        private bool _isSelected;
        private bool _isLocked;
        private int _zIndex;
        private bool _flipHorizontal;
        private bool _flipVertical;
        private string? _hyperlink;
        private string? _alternativeText;
        private string? _action;
        private ShapeTextInfo? _textInfo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<EffectInfo> Effects { get; } = new();
        public ObservableCollection<ConnectionPoint> ConnectionPoints { get; } = new();

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

        public double Rotation
        {
            get => _rotation;
            set { _rotation = value; OnPropertyChanged(nameof(Rotation)); }
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

        public double LineWidth
        {
            get => _lineWidth;
            set { _lineWidth = value; OnPropertyChanged(nameof(LineWidth)); }
        }

        public double Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(nameof(Opacity)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(nameof(IsLocked)); }
        }

        public int ZIndex
        {
            get => _zIndex;
            set { _zIndex = value; OnPropertyChanged(nameof(ZIndex)); }
        }

        public bool FlipHorizontal
        {
            get => _flipHorizontal;
            set { _flipHorizontal = value; OnPropertyChanged(nameof(FlipHorizontal)); }
        }

        public bool FlipVertical
        {
            get => _flipVertical;
            set { _flipVertical = value; OnPropertyChanged(nameof(FlipVertical)); }
        }

        public string? Hyperlink
        {
            get => _hyperlink;
            set { _hyperlink = value; OnPropertyChanged(nameof(Hyperlink)); }
        }

        public string? AlternativeText
        {
            get => _alternativeText;
            set { _alternativeText = value; OnPropertyChanged(nameof(AlternativeText)); }
        }

        public string? Action
        {
            get => _action;
            set { _action = value; OnPropertyChanged(nameof(Action)); }
        }

        public ShapeTextInfo? TextInfo
        {
            get => _textInfo;
            set { _textInfo = value; OnPropertyChanged(nameof(TextInfo)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ShapeTextInfo : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private string _fontName = "Calibri";
        private double _fontSize = 18;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderline;
        private string _fontColor = "#000000";
        private TextAlignment _textAlignment = TextAlignment.Center;
        private VerticalTextAlignment _verticalAlignment = VerticalTextAlignment.Middle;
        private int _marginTop;
        private int _marginBottom;
        private int _marginLeft;
        private int _marginRight;
        private bool _wordWrap = true;
        private int _lineSpacing;
        private double _lineSpacingMultiple = 1.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        public string FontName
        {
            get => _fontName;
            set { _fontName = value; OnPropertyChanged(nameof(FontName)); }
        }

        public double FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }

        public bool IsBold
        {
            get => _isBold;
            set { _isBold = value; OnPropertyChanged(nameof(IsBold)); }
        }

        public bool IsItalic
        {
            get => _isItalic;
            set { _isItalic = value; OnPropertyChanged(nameof(IsItalic)); }
        }

        public bool IsUnderline
        {
            get => _isUnderline;
            set { _isUnderline = value; OnPropertyChanged(nameof(IsUnderline)); }
        }

        public string FontColor
        {
            get => _fontColor;
            set { _fontColor = value; OnPropertyChanged(nameof(FontColor)); }
        }

        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set { _textAlignment = value; OnPropertyChanged(nameof(TextAlignment)); }
        }

        public VerticalTextAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set { _verticalAlignment = value; OnPropertyChanged(nameof(VerticalAlignment)); }
        }

        public int MarginTop
        {
            get => _marginTop;
            set { _marginTop = value; OnPropertyChanged(nameof(MarginTop)); }
        }

        public int MarginBottom
        {
            get => _marginBottom;
            set { _marginBottom = value; OnPropertyChanged(nameof(MarginBottom)); }
        }

        public int MarginLeft
        {
            get => _marginLeft;
            set { _marginLeft = value; OnPropertyChanged(nameof(MarginLeft)); }
        }

        public int MarginRight
        {
            get => _marginRight;
            set { _marginRight = value; OnPropertyChanged(nameof(MarginRight)); }
        }

        public bool WordWrap
        {
            get => _wordWrap;
            set { _wordWrap = value; OnPropertyChanged(nameof(WordWrap)); }
        }

        public int LineSpacing
        {
            get => _lineSpacing;
            set { _lineSpacing = value; OnPropertyChanged(nameof(LineSpacing)); }
        }

        public double LineSpacingMultiple
        {
            get => _lineSpacingMultiple;
            set { _lineSpacingMultiple = value; OnPropertyChanged(nameof(LineSpacingMultiple)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EffectInfo : INotifyPropertyChanged
    {
        private EffectType _type = EffectType.None;
        private string _preset = string.Empty;
        private double _duration = 1.0;
        private double _delay;
        private EffectDirection _direction = EffectDirection.In;
        private string _fillColor = string.Empty;
        private double _size;
        private double _opacity;

        public event PropertyChangedEventHandler? PropertyChanged;

        public EffectType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string Preset
        {
            get => _preset;
            set { _preset = value; OnPropertyChanged(nameof(Preset)); }
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

        public EffectDirection Direction
        {
            get => _direction;
            set { _direction = value; OnPropertyChanged(nameof(Direction)); }
        }

        public string FillColor
        {
            get => _fillColor;
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
        }

        public double Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(nameof(Size)); }
        }

        public double Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(nameof(Opacity)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConnectionPoint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ConnectionPointType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsConnected { get; set; }
    }

    public class AnimationInfo : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _shapeId = string.Empty;
        private string _shapeName = string.Empty;
        private AnimationEffectType _effectType = AnimationEffectType.Fade;
        private AnimationTiming _timing = AnimationTiming.OnClick;
        private EffectDirection _direction = EffectDirection.In;
        private double _duration = 1.0;
        private double _delay;
        private int _orderIndex;
        private bool _isSelected;
        private bool _triggerAfterPrevious;
        private AnimationVariant _variant = AnimationVariant.Basic;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string ShapeId
        {
            get => _shapeId;
            set { _shapeId = value; OnPropertyChanged(nameof(ShapeId)); }
        }

        public string ShapeName
        {
            get => _shapeName;
            set { _shapeName = value; OnPropertyChanged(nameof(ShapeName)); }
        }

        public AnimationEffectType EffectType
        {
            get => _effectType;
            set { _effectType = value; OnPropertyChanged(nameof(EffectType)); }
        }

        public AnimationTiming Timing
        {
            get => _timing;
            set { _timing = value; OnPropertyChanged(nameof(Timing)); }
        }

        public EffectDirection Direction
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

        public int OrderIndex
        {
            get => _orderIndex;
            set { _orderIndex = value; OnPropertyChanged(nameof(OrderIndex)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool TriggerAfterPrevious
        {
            get => _triggerAfterPrevious;
            set { _triggerAfterPrevious = value; OnPropertyChanged(nameof(TriggerAfterPrevious)); }
        }

        public AnimationVariant Variant
        {
            get => _variant;
            set { _variant = value; OnPropertyChanged(nameof(Variant)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SpeakerNote : INotifyPropertyChanged
    {
        private int _slideIndex;
        private string _noteText = string.Empty;
        private string _speakerHints = string.Empty;
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int SlideIndex
        {
            get => _slideIndex;
            set { _slideIndex = value; OnPropertyChanged(nameof(SlideIndex)); }
        }

        public string NoteText
        {
            get => _noteText;
            set { _noteText = value; OnPropertyChanged(nameof(NoteText)); }
        }

        public string SpeakerHints
        {
            get => _speakerHints;
            set { _speakerHints = value; OnPropertyChanged(nameof(SpeakerHints)); }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { _createdDate = value; OnPropertyChanged(nameof(CreatedDate)); }
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set { _modifiedDate = value; OnPropertyChanged(nameof(ModifiedDate)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlideTime
    {
        public double Timing { get; set; }
        public string? Label { get; set; }
        public bool IsDuration { get; set; }
    }

    public class SlideSection : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private int _startSlideIndex;
        private int _endSlideIndex;
        private string _color = "#4472C4";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public int StartSlideIndex
        {
            get => _startSlideIndex;
            set { _startSlideIndex = value; OnPropertyChanged(nameof(StartSlideIndex)); }
        }

        public int EndSlideIndex
        {
            get => _endSlideIndex;
            set { _endSlideIndex = value; OnPropertyChanged(nameof(EndSlideIndex)); }
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

    public class SlideMaster : INotifyPropertyChanged
    {
        private string _name = "Slide Master";
        private string _title = "Slide Master";
        private string _backgroundColor = "#FFFFFF";
        private bool _showBackground;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ShapeInfo> BackgroundShapes { get; } = new();
        public ObservableCollection<PlaceholderInfo> Placeholders { get; } = new();

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PlaceholderInfo : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private PlaceholderType _type = PlaceholderType.Content;
        private double _left;
        private double _top;
        private double _width = 200;
        private double _height = 100;
        private string _text = string.Empty;
        private bool _isVisible = true;
        private bool _showInLayout = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public PlaceholderType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
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

    public class NotesMaster : INotifyPropertyChanged
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

    public class HandoutMaster : INotifyPropertyChanged
    {
        private string _name = "Handout Master";
        private HandoutLayout _layout = HandoutLayout.TwoByTwo;
        private string _headerText = string.Empty;
        private string _footerText = string.Empty;
        private string _backgroundColor = "#FFFFFF";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public HandoutLayout Layout
        {
            get => _layout;
            set { _layout = value; OnPropertyChanged(nameof(Layout)); }
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

    public class PresentationDesignTheme : INotifyPropertyChanged
    {
        private string _name = "Office Theme";
        private string _colorSet = "Office";
        private string _backgroundColor = "#FFFFFF";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ThemeColor> Colors { get; } = new();
        public ObservableCollection<ThemeFont> Fonts { get; } = new();
        public ObservableCollection<ThemeEffect> Effects { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string ColorSet
        {
            get => _colorSet;
            set { _colorSet = value; OnPropertyChanged(nameof(ColorSet)); }
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

    public class ThemeColor : INotifyPropertyChanged
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

    public class ThemeFont : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _headingFont = "Calibri";
        private string _bodyFont = "Calibri";

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThemeEffect : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _preset = "Subtle";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Preset
        {
            get => _preset;
            set { _preset = value; OnPropertyChanged(nameof(Preset)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PresentationTag
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public enum ShapeType
    {
        Rectangle,
        RoundedRectangle,
        Ellipse,
        Circle,
        Triangle,
        RightTriangle,
        Diamond,
        Pentagon,
        Hexagon,
        Octagon,
        Star,
        Arrow,
        LeftArrow,
        UpArrow,
        DownArrow,
        Chevron,
        Heart,
        LightningBolt,
        Moon,
        Sun,
        Cloud,
        Line,
        Connector,
        TextBox,
        Callout,
        ActionButton,
        Chart,
        Table,
        Image,
        Video,
        Audio,
        SmartArt,
        Content,
        Picture,
        Media
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
        TitleAndCaption,
        CenterTitleOnly,
        LargeTitle,
        Blank
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

    public enum AnimationEffectType
    {
        None,
        Appear,
        Fade,
        FadeIn,
        FadeOut,
        FlyIn,
        FlyOut,
        Wipe,
        Blinds,
        Box,
        Checkerboard,
        Circle,
        Crawl,
        Diamond,
        Flash,
        Flip,
        Float,
        Grow,
        LightSpeed,
        Orbit,
        Pinwheel,
        Reveal,
        Ripple,
        Roll,
        Spin,
        Swivel,
        Zoom
    }

    public enum AnimationTiming
    {
        OnClick,
        AfterPrevious,
        WithPrevious,
        Automatic
    }

    public enum EffectType
    {
        None,
        Shadow,
        Glow,
        Reflection,
        Bevel,
        SoftEdge
    }

    public enum EffectDirection
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
        Center
    }

    public enum AnimationVariant
    {
        Basic,
        Subtle,
        Moderate,
        Extensive
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
        HeaderPlaceholder,
        Object,
        PicturePlaceholder
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

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    public enum VerticalTextAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public enum ConnectionPointType
    {
        Center,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum PresentationExportFormat
    {
        Pptx,
        Pdf,
        Png,
        Jpg,
        Gif,
        Tiff,
        Bmp,
        Emf,
        Wmf,
        Html,
        Rtf,
        Ppt,
        Odp
    }

    public interface IPresentationDocumentService
    {
        Task<PresentationInfo> CreateAsync(string? fileName = null);
        Task<PresentationInfo> OpenAsync(string filePath);
        Task<bool> SaveAsync(PresentationInfo presentation, string? filePath = null);
        Task<bool> CloseAsync(PresentationInfo presentation);
        Task<bool> ExportAsync(PresentationInfo presentation, PresentationExportFormat format, string filePath);
    }
}

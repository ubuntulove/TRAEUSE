using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using OfficeSuite.Presentation.Models;

namespace OfficeSuite.Presentation.ViewModels
{
    public class PresentationViewModel : INotifyPropertyChanged
    {
        private string _title = "Untitled Presentation";
        private string _selectedSlideTitle = "Slide 1";
        private int _currentSlideIndex;
        private int _totalSlides;
        private bool _isModified;
        private double _zoomLevel = 100;
        private PresentationViewMode _viewMode = PresentationViewMode.Normal;
        private SlideShowState _slideShowState = SlideShowState.NotStarted;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PresentationDocument Presentation { get; }
        public SlideEditorViewModel SlideEditor { get; }
        public AnimationViewModel Animation { get; }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string SelectedSlideTitle
        {
            get => _selectedSlideTitle;
            set { _selectedSlideTitle = value; OnPropertyChanged(nameof(SelectedSlideTitle)); }
        }

        public int CurrentSlideIndex
        {
            get => _currentSlideIndex;
            set
            {
                _currentSlideIndex = value;
                OnPropertyChanged(nameof(CurrentSlideIndex));
                SelectedSlideTitle = $"Slide {value + 1}";
            }
        }

        public int TotalSlides
        {
            get => _totalSlides;
            set { _totalSlides = value; OnPropertyChanged(nameof(TotalSlides)); }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(nameof(IsModified)); }
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); }
        }

        public PresentationViewMode ViewMode
        {
            get => _viewMode;
            set { _viewMode = value; OnPropertyChanged(nameof(ViewMode)); }
        }

        public SlideShowState SlideShowState
        {
            get => _slideShowState;
            set { _slideShowState = value; OnPropertyChanged(nameof(SlideShowState)); }
        }

        public ObservableCollection<SlideViewModel> Slides { get; } = new();
        public ObservableCollection<string> RecentPresentations { get; } = new();

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand InsertSlideCommand { get; }
        public ICommand DeleteSlideCommand { get; }
        public ICommand DuplicateSlideCommand { get; }
        public ICommand StartSlideShowCommand { get; }
        public ICommand EndSlideShowCommand { get; }
        public ICommand NextSlideCommand { get; }
        public ICommand PreviousSlideCommand { get; }

        public PresentationViewModel()
        {
            Presentation = new PresentationDocument();
            SlideEditor = new SlideEditorViewModel();
            Animation = new AnimationViewModel();

            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand<string>(ExecuteOpen);
            SaveCommand = new RelayCommand<string>(ExecuteSave);
            SaveAsCommand = new RelayCommand<string>(ExecuteSaveAs);
            PrintCommand = new RelayCommand(ExecutePrint);
            UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
            InsertSlideCommand = new RelayCommand(ExecuteInsertSlide);
            DeleteSlideCommand = new RelayCommand(ExecuteDeleteSlide, CanExecuteDeleteSlide);
            DuplicateSlideCommand = new RelayCommand(ExecuteDuplicateSlide, CanExecuteDuplicateSlide);
            StartSlideShowCommand = new RelayCommand(ExecuteStartSlideShow, CanExecuteStartSlideShow);
            EndSlideShowCommand = new RelayCommand(ExecuteEndSlideShow, CanExecuteEndSlideShow);
            NextSlideCommand = new RelayCommand(ExecuteNextSlide, CanExecuteNextSlide);
            PreviousSlideCommand = new RelayCommand(ExecutePreviousSlide, CanExecutePreviousSlide);

            AddDefaultSlide();
        }

        private void AddDefaultSlide()
        {
            var slide = new SlideViewModel
            {
                Index = 0,
                Title = "Title Slide",
                Layout = SlideLayout.TitleSlide
            };
            Slides.Add(slide);
            TotalSlides = Slides.Count;
        }

        private void ExecuteNew()
        {
            Title = "Untitled Presentation";
            Slides.Clear();
            AddDefaultSlide();
            CurrentSlideIndex = 0;
            IsModified = false;
        }

        private void ExecuteOpen(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Title = System.IO.Path.GetFileName(path);
                RecentPresentations.Insert(0, path);
                if (RecentPresentations.Count > 10)
                    RecentPresentations.RemoveAt(10);
            }
        }

        private void ExecuteSave(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Title = System.IO.Path.GetFileName(path);
                IsModified = false;
            }
        }

        private void ExecuteSaveAs(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Title = System.IO.Path.GetFileName(path);
                IsModified = false;
            }
        }

        private void ExecutePrint() { }

        private void ExecuteUndo() { }
        private bool CanExecuteUndo() => true;

        private void ExecuteRedo() { }
        private bool CanExecuteRedo() => true;

        private void ExecuteInsertSlide()
        {
            var newIndex = CurrentSlideIndex + 1;
            var slide = new SlideViewModel
            {
                Index = newIndex,
                Title = $"Slide {newIndex + 1}",
                Layout = SlideLayout.TitleAndContent
            };

            Slides.Insert(newIndex, slide);
            UpdateSlideIndices();
            TotalSlides = Slides.Count;
            CurrentSlideIndex = newIndex;
        }

        private void ExecuteDeleteSlide()
        {
            if (Slides.Count > 1)
            {
                Slides.RemoveAt(CurrentSlideIndex);
                UpdateSlideIndices();
                TotalSlides = Slides.Count;
                if (CurrentSlideIndex >= Slides.Count)
                    CurrentSlideIndex = Slides.Count - 1;
            }
        }

        private bool CanExecuteDeleteSlide() => Slides.Count > 1;

        private void ExecuteDuplicateSlide()
        {
            if (CurrentSlideIndex >= 0 && CurrentSlideIndex < Slides.Count)
            {
                var original = Slides[CurrentSlideIndex];
                var duplicate = new SlideViewModel
                {
                    Index = CurrentSlideIndex + 1,
                    Title = original.Title + " (Copy)",
                    Layout = original.Layout
                };

                Slides.Insert(CurrentSlideIndex + 1, duplicate);
                UpdateSlideIndices();
                TotalSlides = Slides.Count;
                CurrentSlideIndex = CurrentSlideIndex + 1;
            }
        }

        private bool CanExecuteDuplicateSlide() => true;

        private void ExecuteStartSlideShow()
        {
            SlideShowState = SlideShowState.Running;
            ViewMode = PresentationViewMode.SlideShow;
        }

        private bool CanExecuteStartSlideShow() => SlideShowState != SlideShowState.Running;

        private void ExecuteEndSlideShow()
        {
            SlideShowState = SlideShowState.Ended;
            ViewMode = PresentationViewMode.Normal;
        }

        private bool CanExecuteEndSlideShow() => SlideShowState == SlideShowState.Running;

        private void ExecuteNextSlide()
        {
            if (CurrentSlideIndex < Slides.Count - 1)
            {
                CurrentSlideIndex++;
            }
            else if (SlideShowState == SlideShowState.Running)
            {
                ExecuteEndSlideShow();
            }
        }

        private bool CanExecuteNextSlide() => CurrentSlideIndex < Slides.Count - 1 || SlideShowState == SlideShowState.Running;

        private void ExecutePreviousSlide()
        {
            if (CurrentSlideIndex > 0)
            {
                CurrentSlideIndex--;
            }
        }

        private bool CanExecutePreviousSlide() => CurrentSlideIndex > 0;

        private void UpdateSlideIndices()
        {
            for (int i = 0; i < Slides.Count; i++)
            {
                Slides[i].Index = i;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum PresentationViewMode
    {
        Normal,
        Sorter,
        Notes,
        SlideShow,
        Master
    }

    public enum SlideShowState
    {
        NotStarted,
        Running,
        Paused,
        Ended
    }

    public class PresentationDocument : INotifyPropertyChanged
    {
        private string _filePath = string.Empty;
        private string _name = "Presentation";
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;
        private string _author = string.Empty;
        private string _company = string.Empty;
        private string _subject = string.Empty;
        private string _keywords = string.Empty;
        private int _slideWidth = 9144000;
        private int _slideHeight = 5143500;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PresentationTheme? Theme { get; set; }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
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

        public string Company
        {
            get => _company;
            set { _company = value; OnPropertyChanged(nameof(Company)); }
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlideViewModel : INotifyPropertyChanged
    {
        private int _index;
        private string _title = "Slide";
        private SlideLayout _layout = SlideLayout.Blank;
        private bool _isSelected;
        private bool _isHidden;
        private string _backgroundColor = "#FFFFFF";
        private TransitionType _transitionType = TransitionType.None;
        private double _transitionDuration = 0.5;
        private bool _loopUntilEscaped;
        private bool _showNarration;
        private bool _showAnimation;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ShapeViewModel> Shapes { get; } = new();
        public ObservableCollection<AnimationEffectViewModel> Animations { get; } = new();
        public ObservableCollection<SpeakerNoteViewModel> SpeakerNotes { get; } = new();

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public SlideLayout Layout
        {
            get => _layout;
            set { _layout = value; OnPropertyChanged(nameof(Layout)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsHidden
        {
            get => _isHidden;
            set { _isHidden = value; OnPropertyChanged(nameof(IsHidden)); }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public TransitionType TransitionType
        {
            get => _transitionType;
            set { _transitionType = value; OnPropertyChanged(nameof(TransitionType)); }
        }

        public double TransitionDuration
        {
            get => _transitionDuration;
            set { _transitionDuration = value; OnPropertyChanged(nameof(TransitionDuration)); }
        }

        public bool LoopUntilEscaped
        {
            get => _loopUntilEscaped;
            set { _loopUntilEscaped = value; OnPropertyChanged(nameof(LoopUntilEscaped)); }
        }

        public bool ShowNarration
        {
            get => _showNarration;
            set { _showNarration = value; OnPropertyChanged(nameof(ShowNarration)); }
        }

        public bool ShowAnimation
        {
            get => _showAnimation;
            set { _showAnimation = value; OnPropertyChanged(nameof(ShowAnimation)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ShapeViewModel : INotifyPropertyChanged
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
        private string _alternativeText = string.Empty;
        private string _hyperlink = string.Empty;

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

        public string AlternativeText
        {
            get => _alternativeText;
            set { _alternativeText = value; OnPropertyChanged(nameof(AlternativeText)); }
        }

        public string Hyperlink
        {
            get => _hyperlink;
            set { _hyperlink = value; OnPropertyChanged(nameof(Hyperlink)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlideEditorViewModel : INotifyPropertyChanged
    {
        private string _selectedTool = "Select";
        private ShapeType _selectedShapeType = ShapeType.Rectangle;
        private bool _snapToGrid = true;
        private bool _snapToShape = true;
        private double _gridSpacing = 10;
        private bool _showRuler = true;
        private bool _showGuides = true;
        private bool _showGrid = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string SelectedTool
        {
            get => _selectedTool;
            set { _selectedTool = value; OnPropertyChanged(nameof(SelectedTool)); }
        }

        public ShapeType SelectedShapeType
        {
            get => _selectedShapeType;
            set { _selectedShapeType = value; OnPropertyChanged(nameof(SelectedShapeType)); }
        }

        public bool SnapToGrid
        {
            get => _snapToGrid;
            set { _snapToGrid = value; OnPropertyChanged(nameof(SnapToGrid)); }
        }

        public bool SnapToShape
        {
            get => _snapToShape;
            set { _snapToShape = value; OnPropertyChanged(nameof(SnapToShape)); }
        }

        public double GridSpacing
        {
            get => _gridSpacing;
            set { _gridSpacing = value; OnPropertyChanged(nameof(GridSpacing)); }
        }

        public bool ShowRuler
        {
            get => _showRuler;
            set { _showRuler = value; OnPropertyChanged(nameof(ShowRuler)); }
        }

        public bool ShowGuides
        {
            get => _showGuides;
            set { _showGuides = value; OnPropertyChanged(nameof(ShowGuides)); }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set { _showGrid = value; OnPropertyChanged(nameof(ShowGrid)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnimationViewModel : INotifyPropertyChanged
    {
        private AnimationTiming _defaultTiming = AnimationTiming.OnClick;
        private double _defaultDuration = 1.0;
        private bool _useAnimationPainter;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<AnimationPreset> Presets { get; } = new();

        public AnimationTiming DefaultTiming
        {
            get => _defaultTiming;
            set { _defaultTiming = value; OnPropertyChanged(nameof(DefaultTiming)); }
        }

        public double DefaultDuration
        {
            get => _defaultDuration;
            set { _defaultDuration = value; OnPropertyChanged(nameof(DefaultDuration)); }
        }

        public bool UseAnimationPainter
        {
            get => _useAnimationPainter;
            set { _useAnimationPainter = value; OnPropertyChanged(nameof(UseAnimationPainter)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnimationEffectViewModel : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _shapeId = string.Empty;
        private AnimationEffectType _effectType = AnimationEffectType.Fade;
        private AnimationTiming _timing = AnimationTiming.OnClick;
        private AnimationDirection _direction = AnimationDirection.In;
        private double _duration = 1.0;
        private double _delay;
        private bool _triggerAfterPrevious;
        private int _orderIndex;

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

        public bool TriggerAfterPrevious
        {
            get => _triggerAfterPrevious;
            set { _triggerAfterPrevious = value; OnPropertyChanged(nameof(TriggerAfterPrevious)); }
        }

        public int OrderIndex
        {
            get => _orderIndex;
            set { _orderIndex = value; OnPropertyChanged(nameof(OrderIndex)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SpeakerNoteViewModel : INotifyPropertyChanged
    {
        private int _slideIndex;
        private string _noteText = string.Empty;
        private string _speakerHints = string.Empty;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnimationPreset : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public AnimationEffectType EffectType { get; set; }
        public AnimationTiming Timing { get; set; }
        public double Duration { get; set; }
        public AnimationDirection Direction { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum SlideLayout
    {
        Blank,
        TitleSlide,
        TitleAndContent,
        TwoContent,
        Comparison,
        TitleOnly,
        SectionHeader,
        PictureWithCaption,
        TitleAndCaption,
        CenterTitleOnly,
        LargeTitle
    }

    public enum ShapeType
    {
        Rectangle,
        Square,
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
        SmartArt
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

    public enum AnimationEffectType
    {
        None,
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
        Appear,
        Disappear,
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
        FlashOnce,
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

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
    }

    public class CommandManager
    {
        public static event EventHandler? RequerySuggested;
    }
}

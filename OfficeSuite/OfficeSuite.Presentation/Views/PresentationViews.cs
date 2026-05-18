using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace OfficeSuite.Presentation.Views
{
    public class SlideEditorControl : INotifyPropertyChanged
    {
        private double _slideWidth = 960;
        private double _slideHeight = 540;
        private double _zoomLevel = 100;
        private bool _showGrid = true;
        private bool _showGuides = true;
        private bool _snapToGrid = true;
        private bool _snapToObjects = true;
        private double _gridSpacing = 10;
        private ShapeView? _selectedShape;
        private EditorTool _currentTool = EditorTool.Select;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<ShapeSelectedEventArgs>? ShapeSelected;
        public event EventHandler<ShapeMovedEventArgs>? ShapeMoved;
        public event EventHandler<ShapeResizedEventArgs>? ShapeResized;

        public double SlideWidth
        {
            get => _slideWidth;
            set { _slideWidth = value; OnPropertyChanged(nameof(SlideWidth)); }
        }

        public double SlideHeight
        {
            get => _slideHeight;
            set { _slideHeight = value; OnPropertyChanged(nameof(SlideHeight)); }
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set { _showGrid = value; OnPropertyChanged(nameof(ShowGrid)); }
        }

        public bool ShowGuides
        {
            get => _showGuides;
            set { _showGuides = value; OnPropertyChanged(nameof(ShowGuides)); }
        }

        public bool SnapToGrid
        {
            get => _snapToGrid;
            set { _snapToGrid = value; OnPropertyChanged(nameof(SnapToGrid)); }
        }

        public bool SnapToObjects
        {
            get => _snapToObjects;
            set { _snapToObjects = value; OnPropertyChanged(nameof(SnapToObjects)); }
        }

        public double GridSpacing
        {
            get => _gridSpacing;
            set { _gridSpacing = value; OnPropertyChanged(nameof(GridSpacing)); }
        }

        public ShapeView? SelectedShape
        {
            get => _selectedShape;
            set
            {
                if (_selectedShape != null)
                    _selectedShape.IsSelected = false;

                _selectedShape = value;

                if (_selectedShape != null)
                    _selectedShape.IsSelected = true;

                OnPropertyChanged(nameof(SelectedShape));
                ShapeSelected?.Invoke(this, new ShapeSelectedEventArgs { Shape = value });
            }
        }

        public EditorTool CurrentTool
        {
            get => _currentTool;
            set { _currentTool = value; OnPropertyChanged(nameof(CurrentTool)); }
        }

        public ObservableCollection<ShapeView> Shapes { get; } = new();
        public ObservableCollection<GuideLine> HorizontalGuides { get; } = new();
        public ObservableCollection<GuideLine> VerticalGuides { get; } = new();
        public ObservableCollection<ConnectionPoint> ConnectionPoints { get; } = new();

        public void AddShape(ShapeView shape)
        {
            Shapes.Add(shape);
        }

        public void RemoveShape(ShapeView shape)
        {
            Shapes.Remove(shape);
        }

        public void BringToFront(ShapeView shape)
        {
            if (Shapes.Contains(shape))
            {
                Shapes.Remove(shape);
                Shapes.Add(shape);
            }
        }

        public void SendToBack(ShapeView shape)
        {
            if (Shapes.Contains(shape))
            {
                Shapes.Remove(shape);
                Shapes.Insert(0, shape);
            }
        }

        public void GroupShapes(IEnumerable<ShapeView> shapes)
        {
        }

        public void UngroupShapes(ShapeView group)
        {
        }

        public void AlignShapes(Alignment alignment)
        {
        }

        public void DistributeShapes(Distribution distribution)
        {
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ShapeView : INotifyPropertyChanged
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
        private string? _hyperlink;
        private string? _alternativeText;
        private string _text = string.Empty;
        private TextAlignment _textAlignment = TextAlignment.Center;
        private VerticalTextAlignment _verticalAlignment = VerticalTextAlignment.Middle;

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

        public int ZIndex
        {
            get => _zIndex;
            set { _zIndex = value; OnPropertyChanged(nameof(ZIndex)); }
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

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
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

        public ObservableCollection<ConnectionPoint> ConnectionPoints { get; } = new();
        public ObservableCollection<Effect> Effects { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GuideLine
    {
        public double Position { get; set; }
        public GuideOrientation Orientation { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public class ConnectionPoint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ConnectionPointType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsConnected { get; set; }
    }

    public class Effect
    {
        public EffectType Type { get; set; }
        public string Preset { get; set; } = string.Empty;
        public double Duration { get; set; } = 1;
        public double Delay { get; set; }
        public string FillColor { get; set; } = string.Empty;
    }

    public enum EditorTool
    {
        Select,
        Text,
        Rectangle,
        Ellipse,
        Line,
        Arrow,
        Connector,
        Freeform,
        Eraser
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
        Table
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public enum VerticalTextAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public enum GuideOrientation
    {
        Horizontal,
        Vertical
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

    public enum EffectType
    {
        None,
        Shadow,
        Glow,
        Reflection,
        Bevel,
        SoftEdge
    }

    public enum Alignment
    {
        Left,
        Center,
        Right,
        Top,
        Middle,
        Bottom
    }

    public enum Distribution
    {
        Horizontal,
        Vertical
    }

    public class ShapeSelectedEventArgs : EventArgs
    {
        public ShapeView? Shape { get; set; }
    }

    public class ShapeMovedEventArgs : EventArgs
    {
        public ShapeView? Shape { get; set; }
        public double OldLeft { get; set; }
        public double OldTop { get; set; }
        public double NewLeft { get; set; }
        public double NewTop { get; set; }
    }

    public class ShapeResizedEventArgs : EventArgs
    {
        public ShapeView? Shape { get; set; }
        public double OldWidth { get; set; }
        public double OldHeight { get; set; }
        public double NewWidth { get; set; }
        public double NewHeight { get; set; }
    }

    public class SlideSorterView : INotifyPropertyChanged
    {
        private int _selectedSlideIndex;
        private double _thumbnailWidth = 160;
        private double _thumbnailHeight = 90;
        private bool _showSlideNumbers = true;
        private bool _showThumbnails = true;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<SlideSelectedEventArgs>? SlideSelected;

        public int SelectedSlideIndex
        {
            get => _selectedSlideIndex;
            set
            {
                _selectedSlideIndex = value;
                OnPropertyChanged(nameof(SelectedSlideIndex));
            }
        }

        public double ThumbnailWidth
        {
            get => _thumbnailWidth;
            set { _thumbnailWidth = value; OnPropertyChanged(nameof(ThumbnailWidth)); }
        }

        public double ThumbnailHeight
        {
            get => _thumbnailHeight;
            set { _thumbnailHeight = value; OnPropertyChanged(nameof(ThumbnailHeight)); }
        }

        public bool ShowSlideNumbers
        {
            get => _showSlideNumbers;
            set { _showSlideNumbers = value; OnPropertyChanged(nameof(ShowSlideNumbers)); }
        }

        public bool ShowThumbnails
        {
            get => _showThumbnails;
            set { _showThumbnails = value; OnPropertyChanged(nameof(ShowThumbnails)); }
        }

        public ObservableCollection<SlideThumbnail> SlideThumbnails { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlideThumbnail : INotifyPropertyChanged
    {
        private int _index;
        private string _title = "Slide";
        private bool _isSelected;
        private bool _isHidden;
        private int _transitionDuration;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public int TransitionDuration
        {
            get => _transitionDuration;
            set { _transitionDuration = value; OnPropertyChanged(nameof(TransitionDuration)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlideSelectedEventArgs : EventArgs
    {
        public int SlideIndex { get; set; }
    }

    public class SlideShowView : INotifyPropertyChanged
    {
        private bool _isFullScreen;
        private bool _showControls = true;
        private bool _showProgressBar = true;
        private bool _showSlideNumbers = true;
        private bool _loopPresentation;
        private bool _isPlaying;
        private int _currentSlideIndex;
        private int _totalSlides;
        private int _elapsedSeconds;
        private int _totalDurationSeconds;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? SlideChanged;
        public event EventHandler? PresentationEnded;

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set { _isFullScreen = value; OnPropertyChanged(nameof(IsFullScreen)); }
        }

        public bool ShowControls
        {
            get => _showControls;
            set { _showControls = value; OnPropertyChanged(nameof(ShowControls)); }
        }

        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set { _showProgressBar = value; OnPropertyChanged(nameof(ShowProgressBar)); }
        }

        public bool ShowSlideNumbers
        {
            get => _showSlideNumbers;
            set { _showSlideNumbers = value; OnPropertyChanged(nameof(ShowSlideNumbers)); }
        }

        public bool LoopPresentation
        {
            get => _loopPresentation;
            set { _loopPresentation = value; OnPropertyChanged(nameof(LoopPresentation)); }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(nameof(IsPlaying)); }
        }

        public int CurrentSlideIndex
        {
            get => _currentSlideIndex;
            set
            {
                _currentSlideIndex = value;
                OnPropertyChanged(nameof(CurrentSlideIndex));
                SlideChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int TotalSlides
        {
            get => _totalSlides;
            set { _totalSlides = value; OnPropertyChanged(nameof(TotalSlides)); }
        }

        public int ElapsedSeconds
        {
            get => _elapsedSeconds;
            set { _elapsedSeconds = value; OnPropertyChanged(nameof(ElapsedSeconds)); }
        }

        public int TotalDurationSeconds
        {
            get => _totalDurationSeconds;
            set { _totalDurationSeconds = value; OnPropertyChanged(nameof(TotalDurationSeconds)); }
        }

        public void NextSlide()
        {
            if (CurrentSlideIndex < TotalSlides - 1)
            {
                CurrentSlideIndex++;
            }
            else if (LoopPresentation)
            {
                CurrentSlideIndex = 0;
            }
            else
            {
                PresentationEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void PreviousSlide()
        {
            if (CurrentSlideIndex > 0)
            {
                CurrentSlideIndex--;
            }
        }

        public void GoToSlide(int index)
        {
            if (index >= 0 && index < TotalSlides)
            {
                CurrentSlideIndex = index;
            }
        }

        public void StartPresentation()
        {
            IsPlaying = true;
            CurrentSlideIndex = 0;
        }

        public void EndPresentation()
        {
            IsPlaying = false;
            PresentationEnded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnimationPaneView : INotifyPropertyChanged
    {
        private AnimationEffectView? _selectedEffect;
        private AnimationTiming _defaultTiming = AnimationTiming.OnClick;
        private double _defaultDuration = 1.0;
        private bool _showAdvancedTiming;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<AnimationEffectSelectedEventArgs>? EffectSelected;

        public AnimationEffectView? SelectedEffect
        {
            get => _selectedEffect;
            set
            {
                _selectedEffect = value;
                OnPropertyChanged(nameof(SelectedEffect));
                if (value != null)
                {
                    EffectSelected?.Invoke(this, new AnimationEffectSelectedEventArgs { Effect = value });
                }
            }
        }

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

        public bool ShowAdvancedTiming
        {
            get => _showAdvancedTiming;
            set { _showAdvancedTiming = value; OnPropertyChanged(nameof(ShowAdvancedTiming)); }
        }

        public ObservableCollection<AnimationEffectView> Effects { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnimationEffectView : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "Effect";
        private AnimationEffectType _effectType = AnimationEffectType.Fade;
        private AnimationTiming _timing = AnimationTiming.OnClick;
        private AnimationDirection _direction = AnimationDirection.In;
        private double _duration = 1.0;
        private double _delay;
        private int _orderIndex;
        private string _shapeId = string.Empty;
        private string _shapeName = string.Empty;
        private bool _isSelected;
        private bool _triggerAfterPrevious;

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

        public int OrderIndex
        {
            get => _orderIndex;
            set { _orderIndex = value; OnPropertyChanged(nameof(OrderIndex)); }
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnimationEffectSelectedEventArgs : EventArgs
    {
        public AnimationEffectView? Effect { get; set; }
    }

    public enum AnimationEffectType
    {
        Fade,
        FlyIn,
        FlyOut,
        Wipe,
        Blinds,
        Box,
        Checkerboard,
        Circle,
        Crawl,
        Diamond,
        Expand,
        Flash,
        Flip,
        Float,
        Grow,
        LightSpeed,
        Pinwheel,
        Plus,
        RandomBars,
        Reveal,
        Ripple,
        Roll,
        Shimmer,
        Spin,
        Swivel,
        Zoom
    }

    public enum AnimationTiming
    {
        OnClick,
        AfterPrevious,
        WithPrevious
    }

    public enum AnimationDirection
    {
        In,
        Out,
        None
    }

    public class SelectionPaneView : INotifyPropertyChanged
    {
        private ShapeView? _selectedShape;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<ShapeSelectedEventArgs>? ShapeSelected;

        public ShapeView? SelectedShape
        {
            get => _selectedShape;
            set
            {
                _selectedShape = value;
                OnPropertyChanged(nameof(SelectedShape));
                ShapeSelected?.Invoke(this, new ShapeSelectedEventArgs { Shape = value });
            }
        }

        public ObservableCollection<ShapeView> Shapes { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Presentation.ViewModels
{
    public class PresentationViewModel : ViewModelBase
    {
        private PresentationDocument _document;
        private SlideViewModel _currentSlide;
        private int _currentSlideIndex;
        private SelectionViewModel _selection;
        private bool _isModified;
        private string _filePath;
        private double _zoomLevel;
        private string _statusMessage;
        private PresentationViewMode _viewMode = PresentationViewMode.NormalView;
        private bool _showPresenterView;
        private bool _showSlideThumbnails;
        private SlideShowSettingsViewModel _slideShowSettings;

        public PresentationDocument Document { get => _document; set => SetProperty(ref _document, value); }
        public SlideViewModel CurrentSlide { get => _currentSlide; set => SetProperty(ref _currentSlide, value); }
        public int CurrentSlideIndex { get => _currentSlideIndex; set => SetProperty(ref _currentSlideIndex, value); }
        public SelectionViewModel Selection { get => _selection; set => SetProperty(ref _selection, value); }
        public bool IsModified { get => _isModified; set => SetProperty(ref _isModified, value); }
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public PresentationViewMode ViewMode { get => _viewMode; set => SetProperty(ref _viewMode, value); }
        public bool ShowPresenterView { get => _showPresenterView; set => SetProperty(ref _showPresenterView, value); }
        public bool ShowSlideThumbnails { get => _showSlideThumbnails; set => SetProperty(ref _showSlideThumbnails, value); }
        public SlideShowSettingsViewModel SlideShowSettings { get => _slideShowSettings; set => SetProperty(ref _slideShowSettings, value); }

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand InsertSlideCommand { get; }
        public ICommand DeleteSlideCommand { get; }
        public ICommand DuplicateSlideCommand { get; }
        public ICommand MoveSlideCommand { get; }
        public ICommand InsertTextCommand { get; }
        public ICommand InsertImageCommand { get; }
        public ICommand InsertShapeCommand { get; }
        public ICommand InsertSmartArtCommand { get; }
        public ICommand InsertChartCommand { get; }
        public ICommand InsertTableCommand { get; }
        public ICommand InsertMediaCommand { get; }
        public ICommand InsertHyperlinkCommand { get; }
        public ICommand InsertActionCommand { get; }
        public ICommand InsertHeaderCommand { get; }
        public ICommand InsertFooterCommand { get; }
        public ICommand InsertPageNumberCommand { get; }
        public ICommand InsertDateCommand { get; }
        public ICommand InsertTimeCommand { get; }
        public ICommand FormatTextCommand { get; }
        public ICommand FormatShapeCommand { get; }
        public ICommand FormatPictureCommand { get; }
        public ICommand FormatParagraphCommand { get; }
        public ICommand SetTransitionCommand { get; }
        public ICommand SetAnimationCommand { get; }
        public ICommand ReorderAnimationCommand { get; }
        public ICommand SlideShowCommand { get; }
        public ICommand SlideShowFromBeginningCommand { get; }
        public ICommand SlideShowCustomCommand { get; }
        public ICommand EndSlideShowCommand { get; }
        public ICommand NextSlideCommand { get; }
        public ICommand PreviousSlideCommand { get; }
        public ICommand GoToSlideCommand { get; }
        public ICommand NewWindowCommand { get; }
        public ICommand ArrangeAllCommand { get; }
        public ICommand SaveAsPictureCommand { get; }
        public ICommand CheckAccessibilityCommand { get; }
        public ICommand CheckDesignCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        public PresentationViewModel()
        {
            Document = new PresentationDocument();
            Selection = new SelectionViewModel();
            SlideShowSettings = new SlideShowSettingsViewModel();
            _zoomLevel = 100;

            NewCommand = new RelayCommand(async _ => await NewPresentationAsync());
            OpenCommand = new RelayCommand(async _ => await OpenPresentationAsync());
            SaveCommand = new RelayCommand(async _ => await SavePresentationAsync(), _ => IsModified);
            SaveAsCommand = new RelayCommand(async _ => await SavePresentationAsAsync());
            PrintCommand = new RelayCommand(_ => Print());
            UndoCommand = new RelayCommand(_ => Undo());
            RedoCommand = new RelayCommand(_ => Redo());
            CutCommand = new RelayCommand(_ => Cut());
            CopyCommand = new RelayCommand(_ => Copy());
            PasteCommand = new RelayCommand(_ => Paste());
            DeleteCommand = new RelayCommand(_ => Delete());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            InsertSlideCommand = new RelayCommand(_ => InsertSlide());
            DeleteSlideCommand = new RelayCommand(_ => DeleteSlide());
            DuplicateSlideCommand = new RelayCommand(_ => DuplicateSlide());
            MoveSlideCommand = new RelayCommand(_ => MoveSlide());
            InsertTextCommand = new RelayCommand(_ => InsertText());
            InsertImageCommand = new RelayCommand(_ => InsertImage());
            InsertShapeCommand = new RelayCommand(_ => InsertShape());
            InsertSmartArtCommand = new RelayCommand(_ => InsertSmartArt());
            InsertChartCommand = new RelayCommand(_ => InsertChart());
            InsertTableCommand = new RelayCommand(_ => InsertTable());
            InsertMediaCommand = new RelayCommand(_ => InsertMedia());
            InsertHyperlinkCommand = new RelayCommand(_ => InsertHyperlink());
            InsertActionCommand = new RelayCommand(_ => InsertAction());
            InsertHeaderCommand = new RelayCommand(_ => InsertHeader());
            InsertFooterCommand = new RelayCommand(_ => InsertFooter());
            InsertPageNumberCommand = new RelayCommand(_ => InsertPageNumber());
            InsertDateCommand = new RelayCommand(_ => InsertDate());
            InsertTimeCommand = new RelayCommand(_ => InsertTime());
            FormatTextCommand = new RelayCommand(_ => FormatText());
            FormatShapeCommand = new RelayCommand(_ => FormatShape());
            FormatPictureCommand = new RelayCommand(_ => FormatPicture());
            FormatParagraphCommand = new RelayCommand(_ => FormatParagraph());
            SetTransitionCommand = new RelayCommand(_ => SetTransition());
            SetAnimationCommand = new RelayCommand(_ => SetAnimation());
            ReorderAnimationCommand = new RelayCommand(_ => ReorderAnimation());
            SlideShowCommand = new RelayCommand(_ => StartSlideShow());
            SlideShowFromBeginningCommand = new RelayCommand(_ => StartSlideShowFromBeginning());
            SlideShowCustomCommand = new RelayCommand(_ => StartCustomSlideShow());
            EndSlideShowCommand = new RelayCommand(_ => EndSlideShow());
            NextSlideCommand = new RelayCommand(_ => NextSlide());
            PreviousSlideCommand = new RelayCommand(_ => PreviousSlide());
            GoToSlideCommand = new RelayCommand(_ => GoToSlide());
            NewWindowCommand = new RelayCommand(_ => NewWindow());
            ArrangeAllCommand = new RelayCommand(_ => ArrangeAll());
            SaveAsPictureCommand = new RelayCommand(_ => SaveAsPicture());
            CheckAccessibilityCommand = new RelayCommand(_ => CheckAccessibility());
            CheckDesignCommand = new RelayCommand(_ => CheckDesign());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
        }

        protected virtual async Task NewPresentationAsync() { await Task.CompletedTask; }
        protected virtual async Task OpenPresentationAsync() { await Task.CompletedTask; }
        protected virtual async Task SavePresentationAsync() { IsModified = false; await Task.CompletedTask; }
        protected virtual async Task SavePresentationAsAsync() { IsModified = false; await Task.CompletedTask; }
        protected virtual void Print() { }
        protected virtual void Undo() { }
        protected virtual void Redo() { }
        protected virtual void Cut() { }
        protected virtual void Copy() { }
        protected virtual void Paste() { }
        protected virtual void Delete() { }
        protected virtual void SelectAll() { }
        protected virtual void InsertSlide() { }
        protected virtual void DeleteSlide() { }
        protected virtual void DuplicateSlide() { }
        protected virtual void MoveSlide() { }
        protected virtual void InsertText() { }
        protected virtual void InsertImage() { }
        protected virtual void InsertShape() { }
        protected virtual void InsertSmartArt() { }
        protected virtual void InsertChart() { }
        protected virtual void InsertTable() { }
        protected virtual void InsertMedia() { }
        protected virtual void InsertHyperlink() { }
        protected virtual void InsertAction() { }
        protected virtual void InsertHeader() { }
        protected virtual void InsertFooter() { }
        protected virtual void InsertPageNumber() { }
        protected virtual void InsertDate() { }
        protected virtual void InsertTime() { }
        protected virtual void FormatText() { }
        protected virtual void FormatShape() { }
        protected virtual void FormatPicture() { }
        protected virtual void FormatParagraph() { }
        protected virtual void SetTransition() { }
        protected virtual void SetAnimation() { }
        protected virtual void ReorderAnimation() { }
        protected virtual void StartSlideShow() { }
        protected virtual void StartSlideShowFromBeginning() { }
        protected virtual void StartCustomSlideShow() { }
        protected virtual void EndSlideShow() { }
        protected virtual void NextSlide() { if (CurrentSlideIndex < Document.Slides.Count - 1) CurrentSlideIndex++; }
        protected virtual void PreviousSlide() { if (CurrentSlideIndex > 0) CurrentSlideIndex--; }
        protected virtual void GoToSlide() { }
        protected virtual void NewWindow() { }
        protected virtual void ArrangeAll() { }
        protected virtual void SaveAsPicture() { }
        protected virtual void CheckAccessibility() { }
        protected virtual void CheckDesign() { }
        protected virtual void ZoomIn() { if (ZoomLevel < 400) ZoomLevel += 10; }
        protected virtual void ZoomOut() { if (ZoomLevel > 10) ZoomLevel -= 10; }
    }

    public enum PresentationViewMode { NormalView, OutlineView, SlideSorterView, NotesPageView, MasterView, SlideShowView }

    public class PresentationDocument : ViewModelBase
    {
        private string _name;
        private ObservableCollection<SlideViewModel> _slides = new ObservableCollection<SlideViewModel>();
        private SlideMasterViewModel _slideMaster;
        private ThemeViewModel _theme;
        private PresentationPropertiesViewModel _properties;
        private List<CustomShowViewModel> _customShows = new List<CustomShowViewModel>();
        private List<SectionViewModel> _sections = new List<SectionViewModel>();

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ObservableCollection<SlideViewModel> Slides { get => _slides; set => SetProperty(ref _slides, value); }
        public SlideMasterViewModel SlideMaster { get => _slideMaster; set => SetProperty(ref _slideMaster, value); }
        public ThemeViewModel Theme { get => _theme; set => SetProperty(ref _theme, value); }
        public PresentationPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public List<CustomShowViewModel> CustomShows { get => _customShows; set => SetProperty(ref _customShows, value); }
        public List<SectionViewModel> Sections { get => _sections; set => SetProperty(ref _sections, value); }
    }

    public class SlideViewModel : ViewModelBase
    {
        private string _id;
        private int _index;
        private string _name;
        private string _title;
        private SlideLayoutViewModel _layout;
        private SlideBackgroundViewModel _background;
        private ObservableCollection<ShapeViewModel> _shapes = new ObservableCollection<ShapeViewModel>();
        private SlideTimingViewModel _timing;
        private SlideTransitionViewModel _transition;
        private NotesViewModel _notes;
        private bool _hidden;
        private bool _isSelected;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public int Index { get => _index; set => SetProperty(ref _index, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public SlideLayoutViewModel Layout { get => _layout; set => SetProperty(ref _layout, value); }
        public SlideBackgroundViewModel Background { get => _background; set => SetProperty(ref _background, value); }
        public ObservableCollection<ShapeViewModel> Shapes { get => _shapes; set => SetProperty(ref _shapes, value); }
        public SlideTimingViewModel Timing { get => _timing; set => SetProperty(ref _timing, value); }
        public SlideTransitionViewModel Transition { get => _transition; set => SetProperty(ref _transition, value); }
        public NotesViewModel Notes { get => _notes; set => SetProperty(ref _notes, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class SlideLayoutViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private LayoutType _type;
        private double _width = 9144000;
        private double _height = 6858000;
        private SlideBackgroundViewModel _background;
        private ObservableCollection<PlaceholderViewModel> _placeholders = new ObservableCollection<PlaceholderViewModel>();

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public LayoutType Type { get => _type; set => SetProperty(ref _type, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public SlideBackgroundViewModel Background { get => _background; set => SetProperty(ref _background, value); }
        public ObservableCollection<PlaceholderViewModel> Placeholders { get => _placeholders; set => SetProperty(ref _placeholders, value); }
    }

    public enum LayoutType
    {
        Title, TitleAndContent, TwoContent, Comparison, TitleOnly, Blank,
        SectionHeader, ContentWithCaption, PictureWithCaption, VerticalTitleAndText,
        VerticalTwoContent, Object, Table, Chart, Diagram, Media, Custom
    }

    public class SlideMasterViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private SlideBackgroundViewModel _background;
        private ObservableCollection<SlideLayoutViewModel> _layouts = new ObservableCollection<SlideLayoutViewModel>();
        private ObservableCollection<ShapeViewModel> _shapes = new ObservableCollection<ShapeViewModel>();
        private ThemeViewModel _theme;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public SlideBackgroundViewModel Background { get => _background; set => SetProperty(ref _background, value); }
        public ObservableCollection<SlideLayoutViewModel> Layouts { get => _layouts; set => SetProperty(ref _layouts, value); }
        public ObservableCollection<ShapeViewModel> Shapes { get => _shapes; set => SetProperty(ref _shapes, value); }
        public ThemeViewModel Theme { get => _theme; set => SetProperty(ref _theme, value); }
    }

    public class SlideBackgroundViewModel : ViewModelBase
    {
        private FillType _fillType = FillType.Solid;
        private Color _solidColor;
        private GradientFillViewModel _gradient;
        private PatternFillViewModel _pattern;
        private PictureFillViewModel _picture;
        private bool _hidden;

        public FillType FillType { get => _fillType; set => SetProperty(ref _fillType, value); }
        public Color SolidColor { get => _solidColor; set => SetProperty(ref _solidColor, value); }
        public GradientFillViewModel Gradient { get => _gradient; set => SetProperty(ref _gradient, value); }
        public PatternFillViewModel Pattern { get => _pattern; set => SetProperty(ref _pattern, value); }
        public PictureFillViewModel Picture { get => _picture; set => SetProperty(ref _picture, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
    }

    public enum FillType { None, Solid, Gradient, Pattern, Picture }

    public class GradientFillViewModel
    {
        private GradientType _type = GradientType.Linear;
        private double _angle;
        private ObservableCollection<GradientStopViewModel> _stops = new ObservableCollection<GradientStopViewModel>();

        public GradientType Type { get => _type; set => _type = value; }
        public double Angle { get => _angle; set => _angle = value; }
        public ObservableCollection<GradientStopViewModel> Stops { get => _stops; set => _stops = value; }
    }

    public enum GradientType { Linear, Radial, Rectangular, Path }

    public class GradientStopViewModel
    {
        private double _position;
        private Color _color;

        public double Position { get => _position; set => _position = value; }
        public Color Color { get => _color; set => _color = value; }
    }

    public class PatternFillViewModel
    {
        private PatternType _patternType;
        private Color _foregroundColor;
        private Color _backgroundColor;

        public PatternType PatternType { get => _patternType; set => _patternType = value; }
        public Color ForegroundColor { get => _foregroundColor; set => _foregroundColor = value; }
        public Color BackgroundColor { get => _backgroundColor; set => _backgroundColor = value; }
    }

    public enum PatternType
    {
        DashedHorizontal, DashedVertical, DiagonalLeft, DiagonalRight, Horizontal,
        Vertical, Cross, DiagonalCross, Dotted, Dashed, Brick, CheckerBoard
    }

    public class PictureFillViewModel
    {
        private string _imagePath;
        private byte[] _imageData;
        private StretchType _stretch = StretchType.Fill;
        private AlignmentType _alignment = AlignmentType.Center;

        public string ImagePath { get => _imagePath; set => _imagePath = value; }
        public byte[] ImageData { get => _imagePath; set => _imagePath = value; }
        public StretchType Stretch { get => _stretch; set => _stretch = value; }
        public AlignmentType Alignment { get => _alignment; set => _alignment = value; }
    }

    public enum StretchType { Fill, Tile, Center, Zoom }

    public enum AlignmentType { Center, TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight }

    public class ShapeViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private ShapeType _type;
        private PlaceholderType? _placeholderType;
        private double _left;
        private double _top;
        private double _width;
        private double _height;
        private double _rotation;
        private int _zIndex;
        private bool _hidden;
        private bool _locked;
        private ShapeFillViewModel _fill;
        private ShapeLineViewModel _line;
        private ShapeEffectViewModel _effect;
        private Shape3DViewModel _threeD;
        private ShapeTextViewModel _text;
        private ShapeTransformViewModel _transform;
        private ActionSettingsViewModel _action;
        private MediaContentViewModel _media;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ShapeType Type { get => _type; set => SetProperty(ref _type, value); }
        public PlaceholderType? PlaceholderType { get => _placeholderType; set => SetProperty(ref _placeholderType, value); }
        public double Left { get => _left; set => SetProperty(ref _left, value); }
        public double Top { get => _top; set => SetProperty(ref _top, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public double Rotation { get => _rotation; set => SetProperty(ref _rotation, value); }
        public int ZIndex { get => _zIndex; set => SetProperty(ref _zIndex, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
        public bool Locked { get => _locked; set => SetProperty(ref _locked, value); }
        public ShapeFillViewModel Fill { get => _fill; set => SetProperty(ref _fill, value); }
        public ShapeLineViewModel Line { get => _line; set => SetProperty(ref _line, value); }
        public ShapeEffectViewModel Effect { get => _effect; set => SetProperty(ref _effect, value); }
        public Shape3DViewModel ThreeD { get => _threeD; set => SetProperty(ref _threeD, value); }
        public ShapeTextViewModel Text { get => _text; set => SetProperty(ref _text, value); }
        public ShapeTransformViewModel Transform { get => _transform; set => SetProperty(ref _transform, value); }
        public ActionSettingsViewModel Action { get => _action; set => SetProperty(ref _action, value); }
        public MediaContentViewModel Media { get => _media; set => SetProperty(ref _media, value); }
    }

    public enum ShapeType
    {
        Rectangle, Ellipse, Diamond, Triangle, RightTriangle, Parallelogram, Trapezoid, Hexagon,
        Star4, Star5, Star6, Star8, Star10, Star12, Star16, Star24, Star32, RoundRectangle,
        Bevel, Cube, Cylinder, Pie, Heart, Sun, Cloud, Gear, SmileyFace, Frame, Banner,
        Arrow, ArrowRight, ArrowLeft, ArrowUp, ArrowDown, Chevron, Callout, TextBox,
        Picture, Table, SmartArt, Chart, Media, ContentPlaceholder, Title, Group
    }

    public enum PlaceholderType
    {
        Title, Body, CenterTitle, Subtitle, Table, Chart, Picture, ClipArt, Diagram,
        Media, Header, Footer, SlideNumber, Date, Logo, Watermark
    }

    public class ShapeFillViewModel : ViewModelBase
    {
        private FillType _type = FillType.Solid;
        private Color _solidColor = Color.FromHex("#FFFFFF");
        private GradientFillViewModel _gradient;
        private PatternFillViewModel _pattern;
        private PictureFillViewModel _picture;
        private bool _noFill;
        private bool _inherit;

        public FillType Type { get => _type; set => SetProperty(ref _type, value); }
        public Color SolidColor { get => _solidColor; set => SetProperty(ref _solidColor, value); }
        public GradientFillViewModel Gradient { get => _gradient; set => SetProperty(ref _gradient, value); }
        public PatternFillViewModel Pattern { get => _pattern; set => SetProperty(ref _pattern, value); }
        public PictureFillViewModel Picture { get => _picture; set => SetProperty(ref _picture, value); }
        public bool NoFill { get => _noFill; set => SetProperty(ref _noFill, value); }
        public bool Inherit { get => _inherit; set => SetProperty(ref _inherit, value); }
    }

    public class ShapeLineViewModel : ViewModelBase
    {
        private LineType _type = LineType.Solid;
        private Color _color = Color.FromHex("#000000");
        private double _width = 0.75;
        private LineDashStyle _dashStyle = LineDashStyle.Solid;
        private LineCapStyle _capStyle = LineCapStyle.Flat;
        private LineJoinStyle _joinStyle = LineJoinStyle.Round;
        private ArrowHeadStyle _headType = ArrowHeadStyle.None;
        private ArrowHeadStyle _tailType = ArrowHeadStyle.None;
        private double? _headWidth;
        private double? _headLength;
        private bool _noLine;

        public LineType Type { get => _type; set => SetProperty(ref _type, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public LineDashStyle DashStyle { get => _dashStyle; set => SetProperty(ref _dashStyle, value); }
        public LineCapStyle CapStyle { get => _capStyle; set => SetProperty(ref _capStyle, value); }
        public LineJoinStyle JoinStyle { get => _joinStyle; set => SetProperty(ref _joinStyle, value); }
        public ArrowHeadStyle HeadType { get => _headType; set => SetProperty(ref _headType, value); }
        public ArrowHeadStyle TailType { get => _tailType; set => SetProperty(ref _tailType, value); }
        public double? HeadWidth { get => _headWidth; set => SetProperty(ref _headWidth, value); }
        public double? HeadLength { get => _headLength; set => SetProperty(ref _headLength, value); }
        public bool NoLine { get => _noLine; set => SetProperty(ref _noLine, value); }
    }

    public enum LineType { None, Solid, Dash, DashDot, DashDotDot, Dot }

    public enum LineDashStyle
    {
        Solid, Dash, DashDot, DashDotDot, Dot, lgDash, lgDashDot,
        lgDashDotDot, sysDash, sysDashDot, sysDashDotDot, sysDot
    }

    public enum LineCapStyle { Flat, Round, Square }

    public enum LineJoinStyle { Round, Bevel, Miter }

    public enum ArrowHeadStyle { None, Triangle, Arrow, Stealth, Diamond, Oval, Open }

    public class ShapeEffectViewModel : ViewModelBase
    {
        private ShadowViewModel _shadow;
        private GlowViewModel _glow;
        private SoftEdgeViewModel _softEdge;
        private BevelViewModel _bevel;
        private ReflectionViewModel _reflection;

        public ShadowViewModel Shadow { get => _shadow; set => SetProperty(ref _shadow, value); }
        public GlowViewModel Glow { get => _glow; set => SetProperty(ref _glow, value); }
        public SoftEdgeViewModel SoftEdge { get => _softEdge; set => SetProperty(ref _softEdge, value); }
        public BevelViewModel Bevel { get => _bevel; set => SetProperty(ref _bevel, value); }
        public ReflectionViewModel Reflection { get => _reflection; set => SetProperty(ref _reflection, value); }
    }

    public class ShadowViewModel : ViewModelBase
    {
        private bool _visible;
        private Color _color = Color.FromHex("#000000");
        private double _blur = 4;
        private double _offsetX;
        private double _offsetY;
        private double _angle;
        private double _distance;
        private ShadowType _type = ShadowType.Outer;
        private double _opacity = 0.5;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public double Blur { get => _blur; set => SetProperty(ref _blur, value); }
        public double OffsetX { get => _offsetX; set => SetProperty(ref _offsetX, value); }
        public double OffsetY { get => _offsetY; set => SetProperty(ref _offsetY, value); }
        public double Angle { get => _angle; set => SetProperty(ref _angle, value); }
        public double Distance { get => _distance; set => SetProperty(ref _distance, value); }
        public ShadowType Type { get => _type; set => SetProperty(ref _type, value); }
        public double Opacity { get => _opacity; set => SetProperty(ref _opacity, value); }
    }

    public enum ShadowType { None, Outer, Inner }

    public class GlowViewModel : ViewModelBase
    {
        private bool _visible;
        private Color _color = Color.FromHex("#4472C4");
        private double _radius = 4;
        private double _opacity = 0.5;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public double Radius { get => _radius; set => SetProperty(ref _radius, value); }
        public double Opacity { get => _opacity; set => SetProperty(ref _opacity, value); }
    }

    public class SoftEdgeViewModel : ViewModelBase
    {
        private bool _visible;
        private double _radius;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public double Radius { get => _radius; set => SetProperty(ref _radius, value); }
    }

    public class BevelViewModel : ViewModelBase
    {
        private bool _visible;
        private BevelType _type = BevelType.Circle;
        private BevelPresetType _presetType = BevelPresetType.Angle;
        private double _width = 6;
        private double _height = 6;
        private double _angle = 135;
        private LightRigDirection _lightRigDirection = LightRigDirection.Top;
        private LightRigType _lightRigType = LightRigType.Balanced;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public BevelType Type { get => _type; set => SetProperty(ref _type, value); }
        public BevelPresetType PresetType { get => _presetType; set => SetProperty(ref _presetType, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public double Angle { get => _angle; set => SetProperty(ref _angle, value); }
        public LightRigDirection LightRigDirection { get => _lightRigDirection; set => SetProperty(ref _lightRigDirection, value); }
        public LightRigType LightRigType { get => _lightRigType; set => SetProperty(ref _lightRigType, value); }
    }

    public enum BevelType { Circle, Relaxed, RelaxedInverted, Cross, Angle, SoftRound, Convex, Inscribed }

    public enum BevelPresetType
    {
        Angle, ArtDeco, Button, Card, CoolSlant, CounterAngle, Crystal, Cut,
        Divot, Edge1, Edge2, Funnel, Glow, HardEdge, Herringbone, Letter,
        None, Paragraph, Riblet, Slope, Soft, TitleBar, Valley
    }

    public enum LightRigDirection { Top, Bottom, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight }

    public enum LightRigType
    {
        Balanced, BrightRoom, Chilly, Cloudy, Contrast, Flat, Flood, Freezing,
        Glow, Gold, Harsh, Morning, MorningWarm, Soft, Spotlight, Sunrise, Sunset,
        ThreePoint, TwoPoint, Weak
    }

    public class ReflectionViewModel : ViewModelBase
    {
        private bool _visible;
        private double _blur = 4;
        private double _distance;
        private double _angle;
        private double _fadeDirection;
        private double _opacityStart = 0.75;
        private double _opacityEnd = 0;
        private ReflectionType _type = ReflectionType.Triangle;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public double Blur { get => _blur; set => SetProperty(ref _blur, value); }
        public double Distance { get => _distance; set => SetProperty(ref _distance, value); }
        public double Angle { get => _angle; set => SetProperty(ref _angle, value); }
        public double FadeDirection { get => _fadeDirection; set => SetProperty(ref _fadeDirection, value); }
        public double OpacityStart { get => _opacityStart; set => SetProperty(ref _opacityStart, value); }
        public double OpacityEnd { get => _opacityEnd; set => SetProperty(ref _opacityEnd, value); }
        public ReflectionType Type { get => _type; set => SetProperty(ref _type, value); }
    }

    public enum ReflectionType { Triangle, Half, Full }

    public class Shape3DViewModel : ViewModelBase
    {
        private bool _visible;
        private ExtrusionType _extrusionType = ExtrusionType.Over;
        private Color _extrusionColor;
        private double _extrusionHeight = 100000;
        private BevelPresetType _bevelType = BevelPresetType.None;
        private BevelType _bevelTopType = BevelType.Circle;
        private BevelType _bevelBottomType = BevelType.Circle;
        private double _bevelTopHeight = 6;
        private double _bevelBottomHeight = 6;
        private LightRigDirection _lightRigDirection = LightRigDirection.Top;
        private LightRigType _lightRigType = LightRigType.Balanced;
        private double _lightRigAngle;
        private ContourViewModel _contour;
        private CameraViewModel _camera;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public ExtrusionType ExtrusionType { get => _extrusionType; set => SetProperty(ref _extrusionType, value); }
        public Color ExtrusionColor { get => _extrusionColor; set => SetProperty(ref _extrusionColor, value); }
        public double ExtrusionHeight { get => _extrusionHeight; set => SetProperty(ref _extrusionHeight, value); }
        public BevelPresetType BevelType { get => _bevelType; set => SetProperty(ref _bevelType, value); }
        public BevelType BevelTopType { get => _bevelTopType; set => SetProperty(ref _bevelTopType, value); }
        public BevelType BevelBottomType { get => _bevelBottomType; set => SetProperty(ref _bevelBottomType, value); }
        public double BevelTopHeight { get => _bevelTopHeight; set => SetProperty(ref _bevelTopHeight, value); }
        public double BevelBottomHeight { get => _bevelBottomHeight; set => SetProperty(ref _bevelBottomHeight, value); }
        public LightRigDirection LightRigDirection { get => _lightRigDirection; set => SetProperty(ref _lightRigDirection, value); }
        public LightRigType LightRigType { get => _lightRigType; set => SetProperty(ref _lightRigType, value); }
        public double LightRigAngle { get => _lightRigAngle; set => SetProperty(ref _lightRigAngle, value); }
        public ContourViewModel Contour { get => _contour; set => SetProperty(ref _contour, value); }
        public CameraViewModel Camera { get => _camera; set => SetProperty(ref _camera, value); }
    }

    public enum ExtrusionType { None, Over, Perspective }

    public class ContourViewModel : ViewModelBase
    {
        private bool _visible;
        private Color _color = Color.FromHex("#000000");
        private double _width = 0.75;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
    }

    public class CameraViewModel : ViewModelBase
    {
        private CameraType _type = CameraType.Orthographic;
        private double _fieldOfView = 0;
        private double _zoom;
        private double _x;
        private double _y;
        private double _z;

        public CameraType Type { get => _type; set => SetProperty(ref _type, value); }
        public double FieldOfView { get => _fieldOfView; set => SetProperty(ref _fieldOfView, value); }
        public double Zoom { get => _zoom; set => SetProperty(ref _zoom, value); }
        public double X { get => _x; set => SetProperty(ref _x, value); }
        public double Y { get => _y; set => SetProperty(ref _y, value); }
        public double Z { get => _z; set => SetProperty(ref _z, value); }
    }

    public enum CameraType
    {
        Orthographic, Perspective, PerspectiveContrastingLeft, PerspectiveContrastingRight,
        PerspectiveContrastingAbove, PerspectiveContrastingBelow, PerspectiveFront,
        PerspectiveHeroicExtremeLeft, PerspectiveHeroicExtremeRight, PerspectiveHeroicLeft,
        PerspectiveHeroicRight, PerspectiveHeroicBelow, PerspectiveHeroicAbove,
        PerspectiveRelaxed, PerspectiveRelaxedMirrored, PerspectiveShift, PerspectiveTop,
        PerspectiveBottom, PerspectiveTopLeft, PerspectiveTopRight, PerspectiveBottomLeft,
        PerspectiveBottomRight, PerspectiveAboveLeft, PerspectiveAboveRight,
        PerspectiveBelowLeft, PerspectiveBelowRight, PerspectiveFrontLeft, PerspectiveFrontRight
    }

    public class ShapeTextViewModel : ViewModelBase
    {
        private string _text;
        private ObservableCollection<TextParagraphViewModel> _paragraphs = new ObservableCollection<TextParagraphViewModel>();
        private TextDirection _direction = TextDirection.LTR;
        private VerticalTextAlignment _verticalAlignment = VerticalTextAlignment.Top;
        private WrapTextType _wrap = WrapTextType.Square;
        private TextAutofit _autofit = TextAutofit.Normal;
        private double _marginLeft = 91440;
        private double _marginRight = 91440;
        private double _marginTop = 45720;
        private double _marginBottom = 45720;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public ObservableCollection<TextParagraphViewModel> Paragraphs { get => _paragraphs; set => SetProperty(ref _paragraphs, value); }
        public TextDirection Direction { get => _direction; set => SetProperty(ref _direction, value); }
        public VerticalTextAlignment VerticalAlignment { get => _verticalAlignment; set => SetProperty(ref _verticalAlignment, value); }
        public WrapTextType Wrap { get => _wrap; set => SetProperty(ref _wrap, value); }
        public TextAutofit Autofit { get => _autofit; set => SetProperty(ref _autofit, value); }
        public double MarginLeft { get => _marginLeft; set => SetProperty(ref _marginLeft, value); }
        public double MarginRight { get => _marginRight; set => SetProperty(ref _marginRight, value); }
        public double MarginTop { get => _marginTop; set => SetProperty(ref _marginTop, value); }
        public double MarginBottom { get => _marginBottom; set => SetProperty(ref _marginBottom, value); }
    }

    public enum TextDirection { LTR, RTL, TTB, Rotated90, Rotated270, WordArt }

    public enum VerticalTextAlignment { Top, Middle, Bottom, Justified }

    public enum WrapTextType { None, Square, Through, Tight, Oval }

    public enum TextAutofit { None, Normal, Shape }

    public class TextParagraphViewModel : ViewModelBase
    {
        private int _level;
        private string _text;
        private TextAlignment _alignment = TextAlignment.Left;
        private BulletViewModel _bullet;
        private TextSpacingViewModel _spacing;
        private TextIndentationViewModel _indentation;
        private ObservableCollection<TextRunViewModel> _runs = new ObservableCollection<TextRunViewModel>();

        public int Level { get => _level; set => SetProperty(ref _level, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public TextAlignment Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
        public BulletViewModel Bullet { get => _bullet; set => SetProperty(ref _bullet, value); }
        public TextSpacingViewModel Spacing { get => _spacing; set => SetProperty(ref _spacing, value); }
        public TextIndentationViewModel Indentation { get => _indentation; set => SetProperty(ref _indentation, value); }
        public ObservableCollection<TextRunViewModel> Runs { get => _runs; set => SetProperty(ref _runs, value); }
    }

    public enum TextAlignment { Left, Center, Right, Justify, Distribute }

    public class BulletViewModel : ViewModelBase
    {
        private bool _visible;
        private BulletType _type = BulletType.Text;
        private Color _color;
        private string _fontName;
        private double? _fontSize;
        private string _character;
        private string _picturePath;
        private int _startValue = 1;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public BulletType Type { get => _type; set => SetProperty(ref _type, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public string FontName { get => _fontName; set => SetProperty(ref _fontName, value); }
        public double? FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }
        public string Character { get => _character; set => SetProperty(ref _character, value); }
        public string PicturePath { get => _picturePath; set => SetProperty(ref _picturePath, value); }
        public int StartValue { get => _startValue; set => SetProperty(ref _startValue, value); }
    }

    public enum BulletType { None, Text, Number, Picture }

    public class TextSpacingViewModel : ViewModelBase
    {
        private int? _before;
        private int? _after;
        private double? _line;
        private LineSpacingRule _lineRule = LineSpacingRule.Auto;

        public int? Before { get => _before; set => SetProperty(ref _before, value); }
        public int? After { get => _after; set => SetProperty(ref _after, value); }
        public double? Line { get => _line; set => SetProperty(ref _line, value); }
        public LineSpacingRule LineRule { get => _lineRule; set => SetProperty(ref _lineRule, value); }
    }

    public enum LineSpacingRule { Auto, Exact, Multiple }

    public class TextIndentationViewModel : ViewModelBase
    {
        private int? _left;
        private int? _right;
        private int? _firstLine;
        private int? _firstLineChars;

        public int? Left { get => _left; set => SetProperty(ref _left, value); }
        public int? Right { get => _right; set => SetProperty(ref _right, value); }
        public int? FirstLine { get => _firstLine; set => SetProperty(ref _firstLine, value); }
        public int? FirstLineChars { get => _firstLineChars; set => SetProperty(ref _firstLineChars, value); }
    }

    public class TextRunViewModel : ViewModelBase
    {
        private string _text;
        private TextFontViewModel _font;
        private HighlightColor _highlight = HighlightColor.None;
        private StrikeStyle _strike = StrikeStyle.None;
        private UnderlineStyle _underline = UnderlineStyle.None;
        private VerticalTextPosition _verticalPosition = VerticalTextPosition.Baseline;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public TextFontViewModel Font { get => _font; set => SetProperty(ref _font, value); }
        public HighlightColor Highlight { get => _highlight; set => SetProperty(ref _highlight, value); }
        public StrikeStyle Strike { get => _strike; set => SetProperty(ref _strike, value); }
        public UnderlineStyle Underline { get => _underline; set => SetProperty(ref _underline, value); }
        public VerticalTextPosition VerticalPosition { get => _verticalPosition; set => SetProperty(ref _verticalPosition, value); }
    }

    public class TextFontViewModel : ViewModelBase
    {
        private string _typeface = "Calibri";
        private double _size = 18;
        private Color _color = Color.FromHex("#000000");
        private bool _bold;
        private bool _italic;
        private bool _underline;
        private bool _shadow;
        private bool _strikeThrough;
        private bool _outline;

        public string Typeface { get => _typeface; set => SetProperty(ref _typeface, value); }
        public double Size { get => _size; set => SetProperty(ref _size, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public bool Bold { get => _bold; set => SetProperty(ref _bold, value); }
        public bool Italic { get => _italic; set => SetProperty(ref _italic, value); }
        public bool Underline { get => _underline; set => SetProperty(ref _underline, value); }
        public bool Shadow { get => _shadow; set => SetProperty(ref _shadow, value); }
        public bool StrikeThrough { get => _strikeThrough; set => SetProperty(ref _strikeThrough, value); }
        public bool Outline { get => _outline; set => SetProperty(ref _outline, value); }
    }

    public enum HighlightColor { None, Yellow, Green, Cyan, Magenta, Blue, Red }

    public enum StrikeStyle { None, Single, Double }

    public enum UnderlineStyle { None, Single, Double, Dotted, Dashed, Heavy }

    public enum VerticalTextPosition { Baseline, Superscript, Subscript }

    public class ShapeTransformViewModel : ViewModelBase
    {
        private double _rotation;
        private bool _flipHorizontal;
        private bool _flipVertical;
        private double _x;
        private double _y;
        private double _cx;
        private double _cy;
        private double _offsetX;
        private double _offsetY;
        private double _widthPercent;
        private double _heightPercent;

        public double Rotation { get => _rotation; set => SetProperty(ref _rotation, value); }
        public bool FlipHorizontal { get => _flipHorizontal; set => SetProperty(ref _flipHorizontal, value); }
        public bool FlipVertical { get => _flipVertical; set => SetProperty(ref _flipVertical, value); }
        public double X { get => _x; set => SetProperty(ref _x, value); }
        public double Y { get => _y; set => SetProperty(ref _y, value); }
        public double Cx { get => _cx; set => SetProperty(ref _cx, value); }
        public double Cy { get => _cy; set => SetProperty(ref _cy, value); }
        public double OffsetX { get => _offsetX; set => SetProperty(ref _offsetX, value); }
        public double OffsetY { get => _offsetY; set => SetProperty(ref _offsetY, value); }
        public double WidthPercent { get => _widthPercent; set => SetProperty(ref _widthPercent, value); }
        public double HeightPercent { get => _heightPercent; set => SetProperty(ref _heightPercent, value); }
    }

    public class ActionSettingsViewModel : ViewModelBase
    {
        private bool _enabled;
        private ActionType _action = ActionType.None;
        private string _hyperlink;
        private string _jump;
        private string _program;
        private string _programArguments;
        private bool _stopPrevious;
        private string _mouseOverText;
        private string _actionSound;
        private bool _playSound;

        public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }
        public ActionType Action { get => _action; set => SetProperty(ref _action, value); }
        public string Hyperlink { get => _hyperlink; set => SetProperty(ref _hyperlink, value); }
        public string Jump { get => _jump; set => SetProperty(ref _jump, value); }
        public string Program { get => _program; set => SetProperty(ref _program, value); }
        public string ProgramArguments { get => _programArguments; set => SetProperty(ref _programArguments, value); }
        public bool StopPrevious { get => _stopPrevious; set => SetProperty(ref _stopPrevious, value); }
        public string MouseOverText { get => _mouseOverText; set => SetProperty(ref _mouseOverText, value); }
        public string ActionSound { get => _actionSound; set => SetProperty(ref _actionSound, value); }
        public bool PlaySound { get => _playSound; set => SetProperty(ref _playSound, value); }
    }

    public enum ActionType
    {
        None, Hyperlink, Jump, Program, EndShow, Play, CustomShow, PreviousSlide,
        NextSlide, LastSlideViewed, GoToSlide, NamedSlideShow, URL
    }

    public class MediaContentViewModel : ViewModelBase
    {
        private MediaType _type = MediaType.Video;
        private string _fileName;
        private byte[] _data;
        private string _mimeType;
        private double _volume = 1.0;
        private bool _mute;
        private bool _loop;
        private MediaPlayMode _playMode = MediaPlayMode.InSequence;
        private MediaTrigger _trigger = MediaTrigger.OnClick;
        private double _startPosition;
        private double? _endPosition;
        private bool _fullScreen;
        private FadeViewModel _fadeIn;
        private FadeViewModel _fadeOut;
        private string _posterFramePath;
        private TrimViewModel _trim;

        public MediaType Type { get => _type; set => SetProperty(ref _type, value); }
        public string FileName { get => _fileName; set => SetProperty(ref _fileName, value); }
        public byte[] Data { get => _data; set => SetProperty(ref _data, value); }
        public string MimeType { get => _mimeType; set => SetProperty(ref _mimeType, value); }
        public double Volume { get => _volume; set => SetProperty(ref _volume, value); }
        public bool Mute { get => _mute; set => SetProperty(ref _mute, value); }
        public bool Loop { get => _loop; set => SetProperty(ref _loop, value); }
        public MediaPlayMode PlayMode { get => _playMode; set => SetProperty(ref _playMode, value); }
        public MediaTrigger Trigger { get => _trigger; set => SetProperty(ref _trigger, value); }
        public double StartPosition { get => _startPosition; set => SetProperty(ref _startPosition, value); }
        public double? EndPosition { get => _endPosition; set => SetProperty(ref _endPosition, value); }
        public bool FullScreen { get => _fullScreen; set => SetProperty(ref _fullScreen, value); }
        public FadeViewModel FadeIn { get => _fadeIn; set => SetProperty(ref _fadeIn, value); }
        public FadeViewModel FadeOut { get => _fadeOut; set => SetProperty(ref _fadeOut, value); }
        public string PosterFramePath { get => _posterFramePath; set => SetProperty(ref _posterFramePath, value); }
        public TrimViewModel Trim { get => _trim; set => SetProperty(ref _trim, value); }
    }

    public enum MediaType { None, Video, Audio, Flash, AnimatedGif }

    public enum MediaPlayMode { InSequence, AllAtOnce, AllAtOnceShowAll, LoopUntilStopped }

    public enum MediaTrigger { OnClick, WithPrevious, AfterPrevious, OnMouseOver, OnRightClick, OnDoubleClick }

    public class FadeViewModel
    {
        private bool _enabled;
        private double _duration;

        public bool Enabled { get => _enabled; set => _enabled = value; }
        public double Duration { get => _duration; set => _duration = value; }
    }

    public class TrimViewModel
    {
        private double _start;
        private double _end;

        public double Start { get => _start; set => _start = value; }
        public double End { get => _end; set => _end = value; }
    }

    public class PlaceholderViewModel : ViewModelBase
    {
        private int _id;
        private PlaceholderType _type;
        private string _name;
        private double _left;
        private double _top;
        private double _width;
        private double _height;
        private TextAlignment _alignment = TextAlignment.Left;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public PlaceholderType Type { get => _type; set => SetProperty(ref _type, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public double Left { get => _left; set => SetProperty(ref _left, value); }
        public double Top { get => _top; set => SetProperty(ref _top, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public TextAlignment Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
    }

    public class SlideTimingViewModel : ViewModelBase
    {
        private ObservableCollection<AnimationViewModel> _animations = new ObservableCollection<AnimationViewModel>();
        private double _defaultDuration = 0.5;
        private double _defaultDelay;

        public ObservableCollection<AnimationViewModel> Animations { get => _animations; set => SetProperty(ref _animations, value); }
        public double DefaultDuration { get => _defaultDuration; set => SetProperty(ref _defaultDuration, value); }
        public double DefaultDelay { get => _defaultDelay; set => SetProperty(ref _defaultDelay, value); }
    }

    public class AnimationViewModel : ViewModelBase
    {
        private string _id;
        private AnimationType _type = AnimationType.Entrance;
        private string _effect;
        private string _shapeId;
        private double _duration;
        private double _delay;
        private int _triggerIndex;
        private TriggerType _trigger = TriggerType.OnClick;
        private bool _afterPrevious;
        private bool _withPrevious;
        public AnimationTiming _timing = AnimationTiming.Inherit;
        public EasingFunction _easing = EasingFunction.Inherit;
        private TextAnimationViewModel _textAnimation;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public AnimationType Type { get => _type; set => SetProperty(ref _type, value); }
        public string Effect { get => _effect; set => SetProperty(ref _effect, value); }
        public string ShapeId { get => _shapeId; set => SetProperty(ref _shapeId, value); }
        public double Duration { get => _duration; set => SetProperty(ref _duration, value); }
        public double Delay { get => _delay; set => SetProperty(ref _delay, value); }
        public int TriggerIndex { get => _triggerIndex; set => SetProperty(ref _triggerIndex, value); }
        public TriggerType Trigger { get => _trigger; set => SetProperty(ref _trigger, value); }
        public bool AfterPrevious { get => _afterPrevious; set => SetProperty(ref _afterPrevious, value); }
        public bool WithPrevious { get => _withPrevious; set => SetProperty(ref _withPrevious, value); }
        public AnimationTiming Timing { get => _timing; set => SetProperty(ref _timing, value); }
        public EasingFunction Easing { get => _easing; set => SetProperty(ref _easing, value); }
        public TextAnimationViewModel TextAnimation { get => _textAnimation; set => SetProperty(ref _textAnimation, value); }
    }

    public enum AnimationType { Entrance, Exit, Emphasis, Motion, Media }

    public enum TriggerType { OnClick, AfterPrevious, WithPrevious }

    public enum AnimationTiming { Inherit, Linear, Discrete, Pline, Ist, Ost, Ostr }

    public enum EasingFunction { Inherit, Linear, Discrete, Ease, EaseIn, EaseOut, EaseInOut }

    public class TextAnimationViewModel : ViewModelBase
    {
        private TextAnimationType _type = TextAnimationType.AsOneObject;
        private int _groupBy;
        private int? _firstLetter;
        private int? _firstWord;
        private int? _firstParagraph;

        public TextAnimationType Type { get => _type; set => SetProperty(ref _type, value); }
        public int GroupBy { get => _groupBy; set => SetProperty(ref _groupBy, value); }
        public int? FirstLetter { get => _firstLetter; set => SetProperty(ref _firstLetter, value); }
        public int? FirstWord { get => _firstWord; set => SetProperty(ref _firstWord, value); }
        public int? FirstParagraph { get => _firstParagraph; set => SetProperty(ref _firstParagraph, value); }
    }

    public enum TextAnimationType { AsOneObject, ByCharacter, ByWord, ByParagraph, AllAtOnce }

    public class SlideTransitionViewModel : ViewModelBase
    {
        private TransitionType _type = TransitionType.None;
        private TransitionSubtype _subtype = TransitionSubtype.None;
        private double _duration = 1.0;
        private TransitionSpeed _speed = TransitionSpeed.Medium;
        private bool _soundEnabled;
        private string _soundFile;
        private bool _loopSoundUntilNext;
        private bool _advanceOnClick = true;
        private bool _advanceOnTime;
        private double? _advanceTime;

        public TransitionType Type { get => _type; set => SetProperty(ref _type, value); }
        public TransitionSubtype Subtype { get => _subtype; set => SetProperty(ref _subtype, value); }
        public double Duration { get => _duration; set => SetProperty(ref _duration, value); }
        public TransitionSpeed Speed { get => _speed; set => SetProperty(ref _speed, value); }
        public bool SoundEnabled { get => _soundEnabled; set => SetProperty(ref _soundEnabled, value); }
        public string SoundFile { get => _soundFile; set => SetProperty(ref _soundFile, value); }
        public bool LoopSoundUntilNext { get => _loopSoundUntilNext; set => SetProperty(ref _loopSoundUntilNext, value); }
        public bool AdvanceOnClick { get => _advanceOnClick; set => SetProperty(ref _advanceOnClick, value); }
        public bool AdvanceOnTime { get => _advanceOnTime; set => SetProperty(ref _advanceOnTime, value); }
        public double? AdvanceTime { get => _advanceTime; set => SetProperty(ref _advanceTime, value); }
    }

    public enum TransitionType
    {
        None, Fade, Push, Cover, Wipe, Split, Reveal, Random, Cut, DrillDown,
        Flash, Gloss, NewsFlash, Overlay, Peel, Pixellate, Shrink, Swivel, Uncover, Zoom
    }

    public enum TransitionSubtype
    {
        None, Horizontal, Vertical, In, Out, Across, FromLeft, FromRight, FromTop, FromBottom,
        Clockwise, Counterclockwise, Full, Quarter, Half, ThreeQuarter, Fade, FromCenter,
        IntoRectangle, OutRectangle, Laser, Blast, Bloom
    }

    public enum TransitionSpeed { Slow, Medium, Fast }

    public class NotesViewModel : ViewModelBase
    {
        private string _author;
        private DateTime _created;
        private DateTime _modified;
        private string _text;
        private ObservableCollection<ShapeViewModel> _shapes = new ObservableCollection<ShapeViewModel>();

        public string Author { get => _author; set => SetProperty(ref _author, value); }
        public DateTime Created { get => _created; set => SetProperty(ref _created, value); }
        public DateTime Modified { get => _modified; set => SetProperty(ref _modified, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public ObservableCollection<ShapeViewModel> Shapes { get => _shapes; set => SetProperty(ref _shapes, value); }
    }

    public class ThemeViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private ThemeColorsViewModel _colors;
        private ThemeFontsViewModel _fonts;
        private ThemeEffectsViewModel _effects;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ThemeColorsViewModel Colors { get => _colors; set => SetProperty(ref _colors, value); }
        public ThemeFontsViewModel Fonts { get => _fonts; set => SetProperty(ref _fonts, value); }
        public ThemeEffectsViewModel Effects { get => _effects; set => SetProperty(ref _effects, value); }
    }

    public class ThemeColorsViewModel : ViewModelBase
    {
        private Color _dark1 = Color.FromHex("#000000");
        private Color _light1 = Color.FromHex("#FFFFFF");
        private Color _dark2 = Color.FromHex("#1F497D");
        private Color _light2 = Color.FromHex("#EEF1F6");
        private Color _accent1 = Color.FromHex("#4472C4");
        private Color _accent2 = Color.FromHex("#ED7D31");
        private Color _accent3 = Color.FromHex("#A5A5A5");
        private Color _accent4 = Color.FromHex("#FFC000");
        private Color _accent5 = Color.FromHex("#5B9BD5");
        private Color _accent6 = Color.FromHex("#70AD47");
        private Color _hyperlink = Color.FromHex("#0563C1");
        private Color _followedHyperlink = Color.FromHex("#954F72");

        public Color Dark1 { get => _dark1; set => SetProperty(ref _dark1, value); }
        public Color Light1 { get => _light1; set => SetProperty(ref _light1, value); }
        public Color Dark2 { get => _dark2; set => SetProperty(ref _dark2, value); }
        public Color Light2 { get => _light2; set => SetProperty(ref _light2, value); }
        public Color Accent1 { get => _accent1; set => SetProperty(ref _accent1, value); }
        public Color Accent2 { get => _accent2; set => SetProperty(ref _accent2, value); }
        public Color Accent3 { get => _accent3; set => SetProperty(ref _accent3, value); }
        public Color Accent4 { get => _accent4; set => SetProperty(ref _accent4, value); }
        public Color Accent5 { get => _accent5; set => SetProperty(ref _accent5, value); }
        public Color Accent6 { get => _accent6; set => SetProperty(ref _accent6, value); }
        public Color Hyperlink { get => _hyperlink; set => SetProperty(ref _hyperlink, value); }
        public Color FollowedHyperlink { get => _followedHyperlink; set => SetProperty(ref _followedHyperlink, value); }
    }

    public class ThemeFontsViewModel : ViewModelBase
    {
        private string _majorFont = "Calibri Light";
        private string _minorFont = "Calibri";
        private FontSchemeType _fontScheme = FontSchemeType.Mixed;

        public string MajorFont { get => _majorFont; set => SetProperty(ref _majorFont, value); }
        public string MinorFont { get => _minorFont; set => SetProperty(ref _minorFont, value); }
        public FontSchemeType FontScheme { get => _fontScheme; set => SetProperty(ref _fontScheme, value); }
    }

    public enum FontSchemeType { Mixed, Major, Minor, Neither }

    public class ThemeEffectsViewModel : ViewModelBase
    {
        private string _effectName;
        private string _fillStyle;
        private string _lineStyle;
        private string _tableStyle;

        public string EffectName { get => _effectName; set => SetProperty(ref _effectName, value); }
        public string FillStyle { get => _fillStyle; set => SetProperty(ref _fillStyle, value); }
        public string LineStyle { get => _lineStyle; set => SetProperty(ref _lineStyle, value); }
        public string TableStyle { get => _tableStyle; set => SetProperty(ref _tableStyle, value); }
    }

    public class PresentationPropertiesViewModel : ViewModelBase
    {
        private string _title;
        private string _subject;
        private string _author;
        private string _keywords;
        private string _description;
        private string _lastModifiedBy;
        private string _revision;
        private DateTime _created;
        private DateTime _modified;
        private string _category;
        private string _contentStatus;
        private string _identifier;
        private string _language;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Subject { get => _subject; set => SetProperty(ref _subject, value); }
        public string Author { get => _author; set => SetProperty(ref _author, value); }
        public string Keywords { get => _keywords; set => SetProperty(ref _keywords, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string LastModifiedBy { get => _lastModifiedBy; set => SetProperty(ref _lastModifiedBy, value); }
        public string Revision { get => _revision; set => SetProperty(ref _revision, value); }
        public DateTime Created { get => _created; set => SetProperty(ref _created, value); }
        public DateTime Modified { get => _modified; set => SetProperty(ref _modified, value); }
        public string Category { get => _category; set => SetProperty(ref _category, value); }
        public string ContentStatus { get => _contentStatus; set => SetProperty(ref _contentStatus, value); }
        public string Identifier { get => _identifier; set => SetProperty(ref _identifier, value); }
        public string Language { get => _language; set => SetProperty(ref _language, value); }
    }

    public class CustomShowViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private ObservableCollection<string> _slideIds = new ObservableCollection<string>();
        private bool _isRecursive;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ObservableCollection<string> SlideIds { get => _slideIds; set => SetProperty(ref _slideIds, value); }
        public bool IsRecursive { get => _isRecursive; set => SetProperty(ref _isRecursive, value); }
    }

    public class SectionViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private int _startIndex;
        private int _endIndex;
        private bool _hidden;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public int StartIndex { get => _startIndex; set => SetProperty(ref _startIndex, value); }
        public int EndIndex { get => _endIndex; set => SetProperty(ref _endIndex, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
    }

    public class SlideShowSettingsViewModel : ViewModelBase
    {
        private string _showName;
        private bool _loopUntilEsc;
        private bool _showNarration;
        private bool _showAnimation;
        private bool _useTimings;
        private bool _showWithoutNarr;
        private bool _usePresenterView;
        private bool _showScrollbar;
        private bool _showMediaControls;
        private SlideShowType _showType = SlideShowType.Presented;
        private PenColor _penColor = PenColor.Red;
        private int _slidesToShowCount;
        private ObservableCollection<int> _slidesToShow = new ObservableCollection<int>();

        public string ShowName { get => _showName; set => SetProperty(ref _showName, value); }
        public bool LoopUntilEsc { get => _loopUntilEsc; set => SetProperty(ref _loopUntilEsc, value); }
        public bool ShowNarration { get => _showNarration; set => SetProperty(ref _showNarration, value); }
        public bool ShowAnimation { get => _showAnimation; set => SetProperty(ref _showAnimation, value); }
        public bool UseTimings { get => _useTimings; set => SetProperty(ref _useTimings, value); }
        public bool ShowWithoutNarr { get => _showWithoutNarr; set => SetProperty(ref _showWithoutNarr, value); }
        public bool UsePresenterView { get => _usePresenterView; set => SetProperty(ref _usePresenterView, value); }
        public bool ShowScrollbar { get => _showScrollbar; set => SetProperty(ref _showScrollbar, value); }
        public bool ShowMediaControls { get => _showMediaControls; set => SetProperty(ref _showMediaControls, value); }
        public SlideShowType ShowType { get => _showType; set => SetProperty(ref _showType, value); }
        public PenColor PenColor { get => _penColor; set => SetProperty(ref _penColor, value); }
        public int SlidesToShowCount { get => _slidesToShowCount; set => SetProperty(ref _slidesToShowCount, value); }
        public ObservableCollection<int> SlidesToShow { get => _slidesToShow; set => SetProperty(ref _slidesToShow, value); }
    }

    public enum SlideShowType { Presented, Browsed, Custom, Kiosk }

    public enum PenColor { Black, Blue, Green, Red }

    public class SelectionViewModel : ViewModelBase
    {
        private string _selectedShapeId;
        private ObservableCollection<string> _selectedShapeIds = new ObservableCollection<string>();
        private bool _hasSelection;

        public string SelectedShapeId { get => _selectedShapeId; set => SetProperty(ref _selectedShapeId, value); }
        public ObservableCollection<string> SelectedShapeIds { get => _selectedShapeIds; set => SetProperty(ref _selectedShapeIds, value); }
        public bool HasSelection { get => _hasSelection; set => SetProperty(ref _hasSelection, value); }
    }

    public class Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } = 255;

        public static Color FromRgb(byte r, byte g, byte b) => new Color { R = r, G = g, B = b };
        public static Color FromHex(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            return new Color
            {
                R = Convert.ToByte(hex.Substring(0, 2), 16),
                G = Convert.ToByte(hex.Substring(2, 2), 16),
                B = Convert.ToByte(hex.Substring(4, 2), 16)
            };
        }
        public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";
        public override string ToString() => ToHex();
    }
}

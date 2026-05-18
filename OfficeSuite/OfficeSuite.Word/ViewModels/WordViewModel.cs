using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Word.ViewModels
{
    public class WordViewModel : ViewModelBase
    {
        private DocumentViewModel _document;
        private SelectionViewModel _selection;
        private ViewViewModel _view;
        private bool _isModified;
        private string _filePath;
        private double _zoomLevel;
        private string _statusMessage;
        private WordDocument _activeDocument;
        private bool _showRuler;
        private bool _showStatusBar;
        private bool _showVerticalRuler;
        private int _wordCount;
        private int _characterCount;
        private int _pageCount;

        public DocumentViewModel Document { get => _document; set => SetProperty(ref _document, value); }
        public SelectionViewModel Selection { get => _selection; set => SetProperty(ref _selection, value); }
        public ViewViewModel View { get => _view; set => SetProperty(ref _view, value); }
        public bool IsModified { get => _isModified; set => SetProperty(ref _isModified, value); }
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public WordDocument ActiveDocument { get => _activeDocument; set => SetProperty(ref _activeDocument, value); }
        public bool ShowRuler { get => _showRuler; set => SetProperty(ref _showRuler, value); }
        public bool ShowStatusBar { get => _showStatusBar; set => SetProperty(ref _showStatusBar, value); }
        public bool ShowVerticalRuler { get => _showVerticalRuler; set => SetProperty(ref _showVerticalRuler, value); }
        public int WordCount { get => _wordCount; set => SetProperty(ref _wordCount, value); }
        public int CharacterCount { get => _characterCount; set => SetProperty(ref _characterCount, value); }
        public int PageCount { get => _pageCount; set => SetProperty(ref _pageCount, value); }

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
        public ICommand SelectAllCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand BoldCommand { get; }
        public ICommand ItalicCommand { get; }
        public ICommand UnderlineCommand { get; }
        public ICommand StrikethroughCommand { get; }
        public ICommand SubscriptCommand { get; }
        public ICommand SuperscriptCommand { get; }
        public ICommand AlignLeftCommand { get; }
        public ICommand AlignCenterCommand { get; }
        public ICommand AlignRightCommand { get; }
        public ICommand AlignJustifyCommand { get; }
        public ICommand InsertTableCommand { get; }
        public ICommand InsertImageCommand { get; }
        public ICommand InsertHyperlinkCommand { get; }
        public ICommand InsertPageBreakCommand { get; }
        public ICommand InsertHeaderCommand { get; }
        public ICommand InsertFooterCommand { get; }
        public ICommand SetFontCommand { get; }
        public ICommand SetFontSizeCommand { get; }
        public ICommand SetFontColorCommand { get; }
        public ICommand ApplyHeading1Command { get; }
        public ICommand ApplyHeading2Command { get; }
        public ICommand ApplyNormalCommand { get; }
        public ICommand InsertBulletListCommand { get; }
        public ICommand InsertNumberedListCommand { get; }
        public ICommand IncreaseIndentCommand { get; }
        public ICommand DecreaseIndentCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ZoomFitCommand { get; }
        public ICommand ToggleRulerCommand { get; }
        public ICommand ToggleStatusBarCommand { get; }
        public ICommand PageSetupCommand { get; }
        public ICommand PreviewPrintCommand { get; }
        public ICommand SpellCheckCommand { get; }
        public ICommand ThesaurusCommand { get; }
        public ICommand WordCountCommand { get; }

        public event EventHandler<DocumentEventArgs> DocumentChanged;

        public WordViewModel()
        {
            Document = new DocumentViewModel();
            Selection = new SelectionViewModel();
            View = new ViewViewModel();
            _zoomLevel = 100;
            _showRuler = true;
            _showStatusBar = true;
            _showVerticalRuler = false;

            NewCommand = new RelayCommand(async _ => await NewDocumentAsync());
            OpenCommand = new RelayCommand(async _ => await OpenDocumentAsync());
            SaveCommand = new RelayCommand(async _ => await SaveDocumentAsync(), _ => IsModified);
            SaveAsCommand = new RelayCommand(async _ => await SaveDocumentAsAsync());
            PrintCommand = new RelayCommand(_ => Print());
            UndoCommand = new RelayCommand(_ => Undo());
            RedoCommand = new RelayCommand(_ => Redo());
            CutCommand = new RelayCommand(_ => Cut());
            CopyCommand = new RelayCommand(_ => Copy());
            PasteCommand = new RelayCommand(_ => Paste());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            FindCommand = new RelayCommand(_ => Find());
            ReplaceCommand = new RelayCommand(_ => Replace());
            BoldCommand = new RelayCommand(_ => ToggleBold());
            ItalicCommand = new RelayCommand(_ => ToggleItalic());
            UnderlineCommand = new RelayCommand(_ => ToggleUnderline());
            StrikethroughCommand = new RelayCommand(_ => ToggleStrikethrough());
            SubscriptCommand = new RelayCommand(_ => SetSubscript());
            SuperscriptCommand = new RelayCommand(_ => SetSuperscript());
            AlignLeftCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Left));
            AlignCenterCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Center));
            AlignRightCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Right));
            AlignJustifyCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Both));
            InsertTableCommand = new RelayCommand(_ => InsertTable());
            InsertImageCommand = new RelayCommand(_ => InsertImage());
            InsertHyperlinkCommand = new RelayCommand(_ => InsertHyperlink());
            InsertPageBreakCommand = new RelayCommand(_ => InsertPageBreak());
            InsertHeaderCommand = new RelayCommand(_ => InsertHeader());
            InsertFooterCommand = new RelayCommand(_ => InsertFooter());
            SetFontCommand = new RelayCommand(p => SetFont(p?.ToString()));
            SetFontSizeCommand = new RelayCommand(p => SetFontSize(p as int?));
            SetFontColorCommand = new RelayCommand(p => SetFontColor(p?.ToString()));
            ApplyHeading1Command = new RelayCommand(_ => ApplyStyle("Heading1"));
            ApplyHeading2Command = new RelayCommand(_ => ApplyStyle("Heading2"));
            ApplyNormalCommand = new RelayCommand(_ => ApplyStyle("Normal"));
            InsertBulletListCommand = new RelayCommand(_ => InsertBulletList());
            InsertNumberedListCommand = new RelayCommand(_ => InsertNumberedList());
            IncreaseIndentCommand = new RelayCommand(_ => IncreaseIndent());
            DecreaseIndentCommand = new RelayCommand(_ => DecreaseIndent());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            ZoomFitCommand = new RelayCommand(_ => ZoomFit());
            ToggleRulerCommand = new RelayCommand(_ => ShowRuler = !ShowRuler);
            ToggleStatusBarCommand = new RelayCommand(_ => ShowStatusBar = !ShowStatusBar);
            PageSetupCommand = new RelayCommand(_ => ShowPageSetup());
            PreviewPrintCommand = new RelayCommand(_ => PreviewPrint());
            SpellCheckCommand = new RelayCommand(_ => SpellCheck());
            ThesaurusCommand = new RelayCommand(_ => ShowThesaurus());
            WordCountCommand = new RelayCommand(_ => UpdateWordCount());
        }

        protected virtual async Task NewDocumentAsync()
        {
            ActiveDocument = new WordDocument();
            IsModified = false;
            FilePath = null;
            await Task.CompletedTask;
        }

        protected virtual async Task OpenDocumentAsync()
        {
            await Task.CompletedTask;
        }

        protected virtual async Task SaveDocumentAsync()
        {
            IsModified = false;
            await Task.CompletedTask;
        }

        protected virtual async Task SaveDocumentAsAsync()
        {
            IsModified = false;
            await Task.CompletedTask;
        }

        protected virtual void Print() { }
        protected virtual void Undo() { }
        protected virtual void Redo() { }
        protected virtual void Cut() { }
        protected virtual void Copy() { }
        protected virtual void Paste() { }
        protected virtual void SelectAll() { }
        protected virtual void Find() { }
        protected virtual void Replace() { }
        protected virtual void ToggleBold() { }
        protected virtual void ToggleItalic() { }
        protected virtual void ToggleUnderline() { }
        protected virtual void ToggleStrikethrough() { }
        protected virtual void SetSubscript() { }
        protected virtual void SetSuperscript() { }
        protected virtual void SetAlignment(TextAlignment alignment) { }
        protected virtual void InsertTable() { }
        protected virtual void InsertImage() { }
        protected virtual void InsertHyperlink() { }
        protected virtual void InsertPageBreak() { }
        protected virtual void InsertHeader() { }
        protected virtual void InsertFooter() { }
        protected virtual void SetFont(string fontName) { }
        protected virtual void SetFontSize(int? size) { }
        protected virtual void SetFontColor(string color) { }
        protected virtual void ApplyStyle(string styleId) { }
        protected virtual void InsertBulletList() { }
        protected virtual void InsertNumberedList() { }
        protected virtual void IncreaseIndent() { }
        protected virtual void DecreaseIndent() { }
        protected virtual void ZoomIn() { if (ZoomLevel < 500) ZoomLevel += 10; }
        protected virtual void ZoomOut() { if (ZoomLevel > 10) ZoomLevel -= 10; }
        protected virtual void ZoomFit() { ZoomLevel = 100; }
        protected virtual void ShowPageSetup() { }
        protected virtual void PreviewPrint() { }
        protected virtual void SpellCheck() { }
        protected virtual void ShowThesaurus() { }
        protected virtual void UpdateWordCount() { }
    }

    public class DocumentViewModel : ViewModelBase
    {
        private ObservableCollection<ParagraphViewModel> _paragraphs = new ObservableCollection<ParagraphViewModel>();
        private ObservableCollection<StyleViewModel> _styles = new ObservableCollection<StyleViewModel>();
        private TableViewModel _selectedTable;
        private ImageViewModel _selectedImage;
        private HeaderViewModel _header;
        private FooterViewModel _footer;
        private SectionViewModel _currentSection;
        private List<SectionViewModel> _sections = new List<SectionViewModel>();
        private DocumentPropertiesViewModel _properties;

        public ObservableCollection<ParagraphViewModel> Paragraphs { get => _paragraphs; set => SetProperty(ref _paragraphs, value); }
        public ObservableCollection<StyleViewModel> Styles { get => _styles; set => SetProperty(ref _styles, value); }
        public TableViewModel SelectedTable { get => _selectedTable; set => SetProperty(ref _selectedTable, value); }
        public ImageViewModel SelectedImage { get => _selectedImage; set => SetProperty(ref _selectedImage, value); }
        public HeaderViewModel Header { get => _header; set => SetProperty(ref _header, value); }
        public FooterViewModel Footer { get => _footer; set => SetProperty(ref _footer, value); }
        public SectionViewModel CurrentSection { get => _currentSection; set => SetProperty(ref _currentSection, value); }
        public List<SectionViewModel> Sections { get => _sections; set => SetProperty(ref _sections, value); }
        public DocumentPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
    }

    public class ParagraphViewModel : ViewModelBase
    {
        private string _text;
        private string _styleId;
        private ParagraphPropertiesViewModel _properties;
        private ObservableCollection<RunViewModel> _runs = new ObservableCollection<RunViewModel>();
        private bool _isSelected;
        private int _index;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public string StyleId { get => _styleId; set => SetProperty(ref _styleId, value); }
        public ParagraphPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public ObservableCollection<RunViewModel> Runs { get => _runs; set => SetProperty(ref _runs, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public int Index { get => _index; set => SetProperty(ref _index, value); }

        public string GetText()
        {
            var sb = new StringBuilder();
            foreach (var run in Runs)
            {
                sb.Append(run.Text);
            }
            return sb.ToString();
        }
    }

    public class ParagraphPropertiesViewModel : ViewModelBase
    {
        private TextAlignment _alignment = TextAlignment.Left;
        private ParagraphBordersViewModel _borders;
        private ParagraphIndentationViewModel _indentation;
        private ParagraphSpacingViewModel _spacing;
        private ShadingViewModel _shading;

        public TextAlignment Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
        public ParagraphBordersViewModel Borders { get => _borders; set => SetProperty(ref _borders, value); }
        public ParagraphIndentationViewModel Indentation { get => _indentation; set => SetProperty(ref _indentation, value); }
        public ParagraphSpacingViewModel Spacing { get => _spacing; set => SetProperty(ref _spacing, value); }
        public ShadingViewModel Shading { get => _shading; set => SetProperty(ref _shading, value); }
    }

    public class ParagraphBordersViewModel : ViewModelBase
    {
        private BorderViewModel _top;
        private BorderViewModel _bottom;
        private BorderViewModel _left;
        private BorderViewModel _right;
        private BorderViewModel _between;
        private BorderViewModel _bar;

        public BorderViewModel Top { get => _top; set => SetProperty(ref _top, value); }
        public BorderViewModel Bottom { get => _bottom; set => SetProperty(ref _bottom, value); }
        public BorderViewModel Left { get => _left; set => SetProperty(ref _left, value); }
        public BorderViewModel Right { get => _right; set => SetProperty(ref _right, value); }
        public BorderViewModel Between { get => _between; set => SetProperty(ref _between, value); }
        public BorderViewModel Bar { get => _bar; set => SetProperty(ref _bar, value); }
    }

    public class BorderViewModel : ViewModelBase
    {
        private BorderStyle _style = BorderStyle.None;
        private int _size = 0;
        private string _color = "auto";
        private int? _space;

        public BorderStyle Style { get => _style; set => SetProperty(ref _style, value); }
        public int Size { get => _size; set => SetProperty(ref _size, value); }
        public string Color { get => _color; set => SetProperty(ref _color, value); }
        public int? Space { get => _space; set => SetProperty(ref _space, value); }
    }

    public class ParagraphIndentationViewModel : ViewModelBase
    {
        private int? _left;
        private int? _right;
        private int? _firstLine;
        private int? _firstLineChars;
        private bool? _hanging;

        public int? Left { get => _left; set => SetProperty(ref _left, value); }
        public int? Right { get => _right; set => SetProperty(ref _right, value); }
        public int? FirstLine { get => _firstLine; set => SetProperty(ref _firstLine, value); }
        public int? FirstLineChars { get => _firstLineChars; set => SetProperty(ref _firstLineChars, value); }
        public bool? Hanging { get => _hanging; set => SetProperty(ref _hanging, value); }
    }

    public class ParagraphSpacingViewModel : ViewModelBase
    {
        private int? _before;
        private int? _after;
        private int? _beforeLines;
        private int? _afterLines;
        private double? _line;
        private LineSpacingRule _lineRule = LineSpacingRule.Auto;

        public int? Before { get => _before; set => SetProperty(ref _before, value); }
        public int? After { get => _after; set => SetProperty(ref _after, value); }
        public int? BeforeLines { get => _beforeLines; set => SetProperty(ref _beforeLines, value); }
        public int? AfterLines { get => _afterLines; set => SetProperty(ref _afterLines, value); }
        public double? Line { get => _line; set => SetProperty(ref _line, value); }
        public LineSpacingRule LineRule { get => _lineRule; set => SetProperty(ref _lineRule, value); }
    }

    public class LineSpacingRule
    {
        public static readonly int Auto = 0;
        public static readonly int Exactly = 1;
        public static readonly int Multiple = 2;
    }

    public class ShadingViewModel : ViewModelBase
    {
        private ShadingPattern _pattern = ShadingPattern.Clear;
        private string _fillColor = "auto";
        private string _color = "auto";

        public ShadingPattern Pattern { get => _pattern; set => SetProperty(ref _pattern, value); }
        public string FillColor { get => _fillColor; set => SetProperty(ref _fillColor, value); }
        public string Color { get => _color; set => SetProperty(ref _color, value); }
    }

    public enum ShadingPattern { Clear, Solid, HorizontalLine, VerticalLine, DiagonalUp, DiagonalDown, Cross }

    public class RunViewModel : ViewModelBase
    {
        private string _text;
        private RunPropertiesViewModel _properties;
        private bool _isSelected;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public RunPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class RunPropertiesViewModel : ViewModelBase
    {
        private string _fontName = "Calibri";
        private int _fontSize = 22;
        private string _fontColor = "auto";
        private bool _bold;
        private bool _italic;
        private UnderlineStyle _underline = UnderlineStyle.None;
        private StrikeStyle _strike = StrikeStyle.None;
        private VerticalPosition _verticalPosition = VerticalPosition.Baseline;
        private HighlightColor _highlight = HighlightColor.None;
        private ShadingViewModel _shading;

        public string FontName { get => _fontName; set => SetProperty(ref _fontName, value); }
        public int FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }
        public string FontColor { get => _fontColor; set => SetProperty(ref _fontColor, value); }
        public bool Bold { get => _bold; set => SetProperty(ref _bold, value); }
        public bool Italic { get => _italic; set => SetProperty(ref _italic, value); }
        public UnderlineStyle Underline { get => _underline; set => SetProperty(ref _underline, value); }
        public StrikeStyle Strike { get => _strike; set => SetProperty(ref _strike, value); }
        public VerticalPosition VerticalPosition { get => _verticalPosition; set => SetProperty(ref _verticalPosition, value); }
        public HighlightColor Highlight { get => _highlight; set => SetProperty(ref _highlight, value); }
        public ShadingViewModel Shading { get => _shading; set => SetProperty(ref _shading, value); }
    }

    public enum UnderlineStyle { None, Single, Double, Thick, Dotted, Dashed, DashDot, Wave }
    public enum StrikeStyle { None, Single, Double }
    public enum VerticalPosition { Baseline, Superscript, Subscript }
    public enum HighlightColor { None, Yellow, Green, Cyan, Magenta, Blue, Red }

    public class StyleViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private StyleType _type = StyleType.Paragraph;
        private string _basedOn;
        private string _nextStyle;
        private bool _primary;
        private ParagraphPropertiesViewModel _paragraphProperties;
        private RunPropertiesViewModel _runProperties;
        private bool _isSelected;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public StyleType Type { get => _type; set => SetProperty(ref _type, value); }
        public string BasedOn { get => _basedOn; set => SetProperty(ref _basedOn, value); }
        public string NextStyle { get => _nextStyle; set => SetProperty(ref _nextStyle, value); }
        public bool Primary { get => _primary; set => SetProperty(ref _primary, value); }
        public ParagraphPropertiesViewModel ParagraphProperties { get => _paragraphProperties; set => SetProperty(ref _paragraphProperties, value); }
        public RunPropertiesViewModel RunProperties { get => _runProperties; set => SetProperty(ref _runProperties, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public enum StyleType { Paragraph, Character, Table, List }

    public class TableViewModel : ViewModelBase
    {
        private string _id;
        private ObservableCollection<TableRowViewModel> _rows = new ObservableCollection<TableRowViewModel>();
        private TablePropertiesViewModel _properties;
        private bool _isSelected;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public ObservableCollection<TableRowViewModel> Rows { get => _rows; set => SetProperty(ref _rows, value); }
        public TablePropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class TableRowViewModel : ViewModelBase
    {
        private string _id;
        private ObservableCollection<TableCellViewModel> _cells = new ObservableCollection<TableCellViewModel>();
        private TableRowPropertiesViewModel _properties;
        private bool _isSelected;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public ObservableCollection<TableCellViewModel> Cells { get => _cells; set => SetProperty(ref _cells, value); }
        public TableRowPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class TableCellViewModel : ViewModelBase
    {
        private string _id;
        private ObservableCollection<ParagraphViewModel> _paragraphs = new ObservableCollection<ParagraphViewModel>();
        private TableCellPropertiesViewModel _properties;
        private bool _isSelected;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public ObservableCollection<ParagraphViewModel> Paragraphs { get => _paragraphs; set => SetProperty(ref _paragraphs, value); }
        public TableCellPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class TablePropertiesViewModel : ViewModelBase
    {
        private TableJustification _justification = TableJustification.Left;
        private int? _width;
        private WidthType _widthType = WidthType.Dxa;
        private TableBordersViewModel _borders;
        private ShadingViewModel _shading;

        public TableJustification Justification { get => _justification; set => SetProperty(ref _justification, value); }
        public int? Width { get => _width; set => SetProperty(ref _width, value); }
        public WidthType WidthType { get => _widthType; set => SetProperty(ref _widthType, value); }
        public TableBordersViewModel Borders { get => _borders; set => SetProperty(ref _borders, value); }
        public ShadingViewModel Shading { get => _shading; set => SetProperty(ref _shading, value); }
    }

    public enum TableJustification { Left, Center, Right, Both }
    public enum WidthType { Auto, Dxa, Percentage }

    public class TableRowPropertiesViewModel : ViewModelBase
    {
        private int? _height;
        private HeightRule _heightRule = HeightRule.Auto;
        private TableRowAlignment? _alignment;

        public int? Height { get => _height; set => SetProperty(ref _height, value); }
        public HeightRule HeightRule { get => _heightRule; set => SetProperty(ref _heightRule, value); }
        public TableRowAlignment? Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
    }

    public enum HeightRule { Auto, AtLeast, Exact }
    public enum TableRowAlignment { Left, Center, Right }

    public class TableCellPropertiesViewModel : ViewModelBase
    {
        private int? _width;
        private WidthType _widthType = WidthType.Dxa;
        private TableCellVerticalAlignment _verticalAlignment = TableCellVerticalAlignment.Top;
        private TableCellBordersViewModel _borders;
        private ShadingViewModel _shading;
        private MarginsViewModel _margins;
        private int? _rowSpan;
        private int? _columnSpan;

        public int? Width { get => _width; set => SetProperty(ref _width, value); }
        public WidthType WidthType { get => _widthType; set => SetProperty(ref _widthType, value); }
        public TableCellVerticalAlignment VerticalAlignment { get => _verticalAlignment; set => SetProperty(ref _verticalAlignment, value); }
        public TableCellBordersViewModel Borders { get => _borders; set => SetProperty(ref _borders, value); }
        public ShadingViewModel Shading { get => _shading; set => SetProperty(ref _shading, value); }
        public MarginsViewModel Margins { get => _margins; set => SetProperty(ref _margins, value); }
        public int? RowSpan { get => _rowSpan; set => SetProperty(ref _rowSpan, value); }
        public int? ColumnSpan { get => _columnSpan; set => SetProperty(ref _columnSpan, value); }
    }

    public enum TableCellVerticalAlignment { Top, Center, Bottom }

    public class TableBordersViewModel : ViewModelBase
    {
        private BorderViewModel _top;
        private BorderViewModel _bottom;
        private BorderViewModel _left;
        private BorderViewModel _right;
        private BorderViewModel _insideH;
        private BorderViewModel _insideV;

        public BorderViewModel Top { get => _top; set => SetProperty(ref _top, value); }
        public BorderViewModel Bottom { get => _bottom; set => SetProperty(ref _bottom, value); }
        public BorderViewModel Left { get => _left; set => SetProperty(ref _left, value); }
        public BorderViewModel Right { get => _right; set => SetProperty(ref _right, value); }
        public BorderViewModel InsideH { get => _insideH; set => SetProperty(ref _insideH, value); }
        public BorderViewModel InsideV { get => _insideV; set => SetProperty(ref _insideV, value); }
    }

    public class TableCellBordersViewModel : TableBordersViewModel { }

    public class MarginsViewModel : ViewModelBase
    {
        private int? _top;
        private int? _bottom;
        private int? _left;
        private int? _right;

        public int? Top { get => _top; set => SetProperty(ref _top, value); }
        public int? Bottom { get => _bottom; set => SetProperty(ref _bottom, value); }
        public int? Left { get => _left; set => SetProperty(ref _left, value); }
        public int? Right { get => _right; set => SetProperty(ref _right, value); }
    }

    public class ImageViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private byte[] _imageData;
        private int _width;
        private int _height;
        private string _contentType;
        private string _description;
        private double _rotation;
        private bool _flipHorizontal;
        private bool _flipVertical;
        private CropViewModel _cropping;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public byte[] ImageData { get => _imageData; set => SetProperty(ref _imageData, value); }
        public int Width { get => _width; set => SetProperty(ref _width, value); }
        public int Height { get => _height; set => SetProperty(ref _height, value); }
        public string ContentType { get => _contentType; set => SetProperty(ref _contentType, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public double Rotation { get => _rotation; set => SetProperty(ref _rotation, value); }
        public bool FlipHorizontal { get => _flipHorizontal; set => SetProperty(ref _flipHorizontal, value); }
        public bool FlipVertical { get => _flipVertical; set => SetProperty(ref _flipVertical, value); }
        public CropViewModel Cropping { get => _cropping; set => SetProperty(ref _cropping, value); }
    }

    public class CropViewModel : ViewModelBase
    {
        private double _left;
        private double _top;
        private double _right;
        private double _bottom;

        public double Left { get => _left; set => SetProperty(ref _left, value); }
        public double Top { get => _top; set => SetProperty(ref _top, value); }
        public double Right { get => _right; set => SetProperty(ref _right, value); }
        public double Bottom { get => _bottom; set => SetProperty(ref _bottom, value); }
    }

    public class HeaderViewModel : ViewModelBase
    {
        private string _id;
        private ObservableCollection<ParagraphViewModel> _paragraphs = new ObservableCollection<ParagraphViewModel>();
        private bool _isDefault;
        private bool _isFirstPage;
        private bool _isEvenPages;
        private HeaderFooterLink _linkType;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public ObservableCollection<ParagraphViewModel> Paragraphs { get => _paragraphs; set => SetProperty(ref _paragraphs, value); }
        public bool IsDefault { get => _isDefault; set => SetProperty(ref _isDefault, value); }
        public bool IsFirstPage { get => _isFirstPage; set => SetProperty(ref _isFirstPage, value); }
        public bool IsEvenPages { get => _isEvenPages; set => SetProperty(ref _isEvenPages, value); }
        public HeaderFooterLink LinkType { get => _linkType; set => SetProperty(ref _linkType, value); }
    }

    public class FooterViewModel : HeaderViewModel { }

    public enum HeaderFooterLink { Default, FirstPage, EvenPages }

    public class SectionViewModel : ViewModelBase
    {
        private string _id;
        private SectionType _type = SectionType.Continuous;
        private PageSetupViewModel _pageSetup;
        private HeaderViewModel _header;
        private FooterViewModel _footer;
        private HeaderViewModel _firstPageHeader;
        private FooterViewModel _firstPageFooter;
        private HeaderViewModel _evenPagesHeader;
        private FooterViewModel _evenPagesFooter;
        private SectionBordersViewModel _borders;
        private ColumnsViewModel _columns;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public SectionType Type { get => _type; set => SetProperty(ref _type, value); }
        public PageSetupViewModel PageSetup { get => _pageSetup; set => SetProperty(ref _pageSetup, value); }
        public HeaderViewModel Header { get => _header; set => SetProperty(ref _header, value); }
        public FooterViewModel Footer { get => _footer; set => SetProperty(ref _footer, value); }
        public HeaderViewModel FirstPageHeader { get => _firstPageHeader; set => SetProperty(ref _firstPageHeader, value); }
        public FooterViewModel FirstPageFooter { get => _firstPageFooter; set => SetProperty(ref _firstPageFooter, value); }
        public HeaderViewModel EvenPagesHeader { get => _evenPagesHeader; set => SetProperty(ref _evenPagesHeader, value); }
        public FooterViewModel EvenPagesFooter { get => _evenPagesFooter; set => SetProperty(ref _evenPagesFooter, value); }
        public SectionBordersViewModel Borders { get => _borders; set => SetProperty(ref _borders, value); }
        public ColumnsViewModel Columns { get => _columns; set => SetProperty(ref _columns, value); }
    }

    public enum SectionType { Continuous, NextPage, NextColumn, EvenPage, OddPage }

    public class PageSetupViewModel : ViewModelBase
    {
        private double _pageWidth = 12240;
        private double _pageHeight = 15840;
        private double _topMargin = 1440;
        private double _bottomMargin = 1440;
        private double _leftMargin = 1440;
        private double _rightMargin = 1440;
        private double _headerMargin = 720;
        private double _footerMargin = 720;
        private PageOrientation _orientation = PageOrientation.Portrait;
        private PaperSize _paperSize = PaperSize.Letter;

        public double PageWidth { get => _pageWidth; set => SetProperty(ref _pageWidth, value); }
        public double PageHeight { get => _pageHeight; set => SetProperty(ref _pageHeight, value); }
        public double TopMargin { get => _topMargin; set => SetProperty(ref _topMargin, value); }
        public double BottomMargin { get => _bottomMargin; set => SetProperty(ref _bottomMargin, value); }
        public double LeftMargin { get => _leftMargin; set => SetProperty(ref _leftMargin, value); }
        public double RightMargin { get => _rightMargin; set => SetProperty(ref _rightMargin, value); }
        public double HeaderMargin { get => _headerMargin; set => SetProperty(ref _headerMargin, value); }
        public double FooterMargin { get => _footerMargin; set => SetProperty(ref _footerMargin, value); }
        public PageOrientation Orientation { get => _orientation; set => SetProperty(ref _orientation, value); }
        public PaperSize PaperSize { get => _paperSize; set => SetProperty(ref _paperSize, value); }
    }

    public enum PageOrientation { Portrait, Landscape }
    public enum PaperSize { Letter, Legal, A4, A5, B5, Executive, Envelope10, EnvelopeDL }

    public class SectionBordersViewModel : ViewModelBase
    {
        private BorderViewModel _top;
        private BorderViewModel _bottom;
        private BorderViewModel _left;
        private BorderViewModel _right;
        private BorderViewModel _between;
        private BorderViewModel _bar;
        private SectionBordersOffsetFrom _offsetFrom = SectionBordersOffsetFrom.Text;

        public BorderViewModel Top { get => _top; set => SetProperty(ref _top, value); }
        public BorderViewModel Bottom { get => _bottom; set => SetProperty(ref _bottom, value); }
        public BorderViewModel Left { get => _left; set => SetProperty(ref _left, value); }
        public BorderViewModel Right { get => _right; set => SetProperty(ref _right, value); }
        public BorderViewModel Between { get => _between; set => SetProperty(ref _between, value); }
        public BorderViewModel Bar { get => _bar; set => SetProperty(ref _bar, value); }
        public SectionBordersOffsetFrom OffsetFrom { get => _offsetFrom; set => SetProperty(ref _offsetFrom, value); }
    }

    public enum SectionBordersOffsetFrom { Text, Margin }

    public class ColumnsViewModel : ViewModelBase
    {
        private int _columnCount = 1;
        private bool _equalWidth = true;
        private int _space = 720;
        private bool _separatorLine;
        private ObservableCollection<ColumnViewModel> _columns = new ObservableCollection<ColumnViewModel>();

        public int ColumnCount { get => _columnCount; set => SetProperty(ref _columnCount, value); }
        public bool EqualWidth { get => _equalWidth; set => SetProperty(ref _equalWidth, value); }
        public int Space { get => _space; set => SetProperty(ref _space, value); }
        public bool SeparatorLine { get => _separatorLine; set => SetProperty(ref _separatorLine, value); }
        public ObservableCollection<ColumnViewModel> Columns { get => _columns; set => SetProperty(ref _columns, value); }
    }

    public class ColumnViewModel : ViewModelBase
    {
        private int? _width;
        private int? _space;

        public int? Width { get => _width; set => SetProperty(ref _width, value); }
        public int? Space { get => _space; set => SetProperty(ref _space, value); }
    }

    public class DocumentPropertiesViewModel : ViewModelBase
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
        private DateTime? _lastPrinted;
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
        public DateTime? LastPrinted { get => _lastPrinted; set => SetProperty(ref _lastPrinted, value); }
        public string Category { get => _category; set => SetProperty(ref _category, value); }
        public string ContentStatus { get => _contentStatus; set => SetProperty(ref _contentStatus, value); }
        public string Identifier { get => _identifier; set => SetProperty(ref _identifier, value); }
        public string Language { get => _language; set => SetProperty(ref _language, value); }
    }

    public class SelectionViewModel : ViewModelBase
    {
        private string _selectedText;
        private int _startPosition;
        private int _endPosition;
        private ParagraphViewModel _selectedParagraph;
        private RunViewModel _selectedRun;
        private bool _hasSelection;
        private bool _isSelectionCollapsed;
        private TextAlignment _alignment;
        private RunPropertiesViewModel _characterProperties;
        private ParagraphPropertiesViewModel _paragraphProperties;

        public string SelectedText { get => _selectedText; set => SetProperty(ref _selectedText, value); }
        public int StartPosition { get => _startPosition; set => SetProperty(ref _startPosition, value); }
        public int EndPosition { get => _endPosition; set => SetProperty(ref _endPosition, value); }
        public ParagraphViewModel SelectedParagraph { get => _selectedParagraph; set => SetProperty(ref _selectedParagraph, value); }
        public RunViewModel SelectedRun { get => _selectedRun; set => SetProperty(ref _selectedRun, value); }
        public bool HasSelection { get => _hasSelection; set => SetProperty(ref _hasSelection, value); }
        public bool IsSelectionCollapsed { get => _isSelectionCollapsed; set => SetProperty(ref _isSelectionCollapsed, value); }
        public TextAlignment Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
        public RunPropertiesViewModel CharacterProperties { get => _characterProperties; set => SetProperty(ref _characterProperties, value); }
        public ParagraphPropertiesViewModel ParagraphProperties { get => _paragraphProperties; set => SetProperty(ref _paragraphProperties, value); }
    }

    public enum TextAlignment { Left, Center, Right, Both }

    public class ViewViewModel : ViewModelBase
    {
        private ViewType _viewType = ViewType.Print;
        private bool _showWhitespace;
        private bool _showRuler;
        private bool _showVerticalRuler;
        private bool _showGridlines;
        private bool _showBookmarks;
        private bool _showAnnotations;
        private bool _showRevisionMarks;
        private bool _showHiddenText;
        private int _zoomLevel = 100;
        private int _hScrollPosition;
        private int _vScrollPosition;
        private string _windowCaption;
        private bool _splitVertical;
        private int _splitPosition;

        public ViewType ViewType { get => _viewType; set => SetProperty(ref _viewType, value); }
        public bool ShowWhitespace { get => _showWhitespace; set => SetProperty(ref _showWhitespace, value); }
        public bool ShowRuler { get => _showRuler; set => SetProperty(ref _showRuler, value); }
        public bool ShowVerticalRuler { get => _showVerticalRuler; set => SetProperty(ref _showVerticalRuler, value); }
        public bool ShowGridlines { get => _showGridlines; set => SetProperty(ref _showGridlines, value); }
        public bool ShowBookmarks { get => _showBookmarks; set => SetProperty(ref _showBookmarks, value); }
        public bool ShowAnnotations { get => _showAnnotations; set => SetProperty(ref _showAnnotations, value); }
        public bool ShowRevisionMarks { get => _showRevisionMarks; set => SetProperty(ref _showRevisionMarks, value); }
        public bool ShowHiddenText { get => _showHiddenText; set => SetProperty(ref _showHiddenText, value); }
        public int ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }
        public int HScrollPosition { get => _hScrollPosition; set => SetProperty(ref _hScrollPosition, value); }
        public int VScrollPosition { get => _vScrollPosition; set => SetProperty(ref _vScrollPosition, value); }
        public string WindowCaption { get => _windowCaption; set => SetProperty(ref _windowCaption, value); }
        public bool SplitVertical { get => _splitVertical; set => SetProperty(ref _splitVertical, value); }
        public int SplitPosition { get => _splitPosition; set => SetProperty(ref _splitPosition, value); }
    }

    public enum ViewType { Print, Web, Outline, Draft, Reading }

    public class WordDocument
    {
        public string Id { get; set; }
        public string FilePath { get; set; }
        public DocumentViewModel Content { get; set; }
        public bool IsModified { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }

    public class FindReplaceViewModel : ViewModelBase
    {
        private string _searchText;
        private string _replaceText;
        private bool _matchCase;
        private bool _matchWholeWord;
        private bool _useWildcards;
        private bool _useRegularExpression;
        private bool _searchUp;
        private int _matchCount;
        private bool _findInHeaders;
        private bool _findInFooters;
        private bool _findInTextBoxes;
        private FindFormatViewModel _findFormat;
        private ReplaceFormatViewModel _replaceFormat;

        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
        public string ReplaceText { get => _replaceText; set => SetProperty(ref _replaceText, value); }
        public bool MatchCase { get => _matchCase; set => SetProperty(ref _matchCase, value); }
        public bool MatchWholeWord { get => _matchWholeWord; set => SetProperty(ref _matchWholeWord, value); }
        public bool UseWildcards { get => _useWildcards; set => SetProperty(ref _useWildcards, value); }
        public bool UseRegularExpression { get => _useRegularExpression; set => SetProperty(ref _useRegularExpression, value); }
        public bool SearchUp { get => _searchUp; set => SetProperty(ref _searchUp, value); }
        public int MatchCount { get => _matchCount; set => SetProperty(ref _matchCount, value); }
        public bool FindInHeaders { get => _findInHeaders; set => SetProperty(ref _findInHeaders, value); }
        public bool FindInFooters { get => _findInFooters; set => SetProperty(ref _findInFooters, value); }
        public bool FindInTextBoxes { get => _findInTextBoxes; set => SetProperty(ref _findInTextBoxes, value); }
        public FindFormatViewModel FindFormat { get => _findFormat; set => SetProperty(ref _findFormat, value); }
        public ReplaceFormatViewModel ReplaceFormat { get => _replaceFormat; set => SetProperty(ref _replaceFormat, value); }
    }

    public class FindFormatViewModel : ViewModelBase
    {
        private RunPropertiesViewModel _font;
        private ParagraphPropertiesViewModel _paragraph;
        private ShadingViewModel _highlight;
        private string _styleName;

        public RunPropertiesViewModel Font { get => _font; set => SetProperty(ref _font, value); }
        public ParagraphPropertiesViewModel Paragraph { get => _paragraph; set => SetProperty(ref _paragraph, value); }
        public ShadingViewModel Highlight { get => _highlight; set => SetProperty(ref _highlight, value); }
        public string StyleName { get => _styleName; set => SetProperty(ref _styleName, value); }
    }

    public class ReplaceFormatViewModel : FindFormatViewModel { }

    public class GoToViewModel : ViewModelBase
    {
        private GoToType _gotoType = GoToType.Page;
        private int? _pageNumber;
        private int? _lineNumber;
        private int? _sectionNumber;
        private string _bookmarkName;
        private int? _commentId;
        private int? _footnoteId;
        private int? _endnoteId;
        private int? _tableId;
        private int? _graphicId;
        private int? _objectId;
        private int? _revisionId;

        public GoToType GotoType { get => _gotoType; set => SetProperty(ref _gotoType, value); }
        public int? PageNumber { get => _pageNumber; set => SetProperty(ref _pageNumber, value); }
        public int? LineNumber { get => _lineNumber; set => SetProperty(ref _lineNumber, value); }
        public int? SectionNumber { get => _sectionNumber; set => SetProperty(ref _sectionNumber, value); }
        public string BookmarkName { get => _bookmarkName; set => SetProperty(ref _bookmarkName, value); }
        public int? CommentId { get => _commentId; set => SetProperty(ref _commentId, value); }
        public int? FootnoteId { get => _footnoteId; set => SetProperty(ref _footnoteId, value); }
        public int? EndnoteId { get => _endnoteId; set => SetProperty(ref _endnoteId, value); }
        public int? TableId { get => _tableId; set => SetProperty(ref _tableId, value); }
        public int? GraphicId { get => _graphicId; set => SetProperty(ref _graphicId, value); }
        public int? ObjectId { get => _objectId; set => SetProperty(ref _objectId, value); }
        public int? RevisionId { get => _revisionId; set => SetProperty(ref _revisionId, value); }
    }

    public enum GoToType { Page, Line, Section, Bookmark, Comment, Footnote, Endnote, Table, Graphic, Equation, Object }

    public class SpellingViewModel : ViewModelBase
    {
        private string _currentWord;
        private string _suggestion;
        private ObservableCollection<string> _suggestions = new ObservableCollection<string>();
        private int _suggestionIndex;
        private bool _ignoreAll;
        private bool _addToDictionary;
        private string _originalWord;
        private bool _hasSuggestions;
        private bool _isGrammarError;

        public string CurrentWord { get => _currentWord; set => SetProperty(ref _currentWord, value); }
        public string Suggestion { get => _suggestion; set => SetProperty(ref _suggestion, value); }
        public ObservableCollection<string> Suggestions { get => _suggestions; set => SetProperty(ref _suggestions, value); }
        public int SuggestionIndex { get => _suggestionIndex; set => SetProperty(ref _suggestionIndex, value); }
        public bool IgnoreAll { get => _ignoreAll; set => SetProperty(ref _ignoreAll, value); }
        public bool AddToDictionary { get => _addToDictionary; set => SetProperty(ref _addToDictionary, value); }
        public string OriginalWord { get => _originalWord; set => SetProperty(ref _originalWord, value); }
        public bool HasSuggestions { get => _hasSuggestions; set => SetProperty(ref _hasSuggestions, value); }
        public bool IsGrammarError { get => _isGrammarError; set => SetProperty(ref _isGrammarError, value); }
    }

    public class ThesaurusViewModel : ViewModelBase
    {
        private string _searchWord;
        private ObservableCollection<ThesaurusResultViewModel> _results = new ObservableCollection<ThesaurusResultViewModel>();
        private ObservableCollection<string> _relatedWords = new ObservableCollection<string>();
        private ObservableCollection<ThesaurusMeaningViewModel> _meanings = new ObservableCollection<ThesaurusMeaningViewModel>();

        public string SearchWord { get => _searchWord; set => SetProperty(ref _searchWord, value); }
        public ObservableCollection<ThesaurusResultViewModel> Results { get => _results; set => SetProperty(ref _results, value); }
        public ObservableCollection<string> RelatedWords { get => _relatedWords; set => SetProperty(ref _relatedWords, value); }
        public ObservableCollection<ThesaurusMeaningViewModel> Meanings { get => _meanings; set => SetProperty(ref _meanings, value); }
    }

    public class ThesaurusResultViewModel : ViewModelBase
    {
        private string _word;
        private string _partOfSpeech;
        private ObservableCollection<string> _synonyms = new ObservableCollection<string>();

        public string Word { get => _word; set => SetProperty(ref _word, value); }
        public string PartOfSpeech { get => _partOfSpeech; set => SetProperty(ref _partOfSpeech, value); }
        public ObservableCollection<string> Synonyms { get => _synonyms; set => SetProperty(ref _synonyms, value); }
    }

    public class ThesaurusMeaningViewModel : ViewModelBase
    {
        private string _partOfSpeech;
        private ObservableCollection<ThesaurusDefinitionViewModel> _definitions = new ObservableCollection<ThesaurusDefinitionViewModel>();

        public string PartOfSpeech { get => _partOfSpeech; set => SetProperty(ref _partOfSpeech, value); }
        public ObservableCollection<ThesaurusDefinitionViewModel> Definitions { get => _definitions; set => SetProperty(ref _definitions, value); }
    }

    public class ThesaurusDefinitionViewModel : ViewModelBase
    {
        private string _definition;
        private ObservableCollection<string> _examples = new ObservableCollection<string>();
        private ObservableCollection<string> _synonyms = new ObservableCollection<string>();

        public string Definition { get => _definition; set => SetProperty(ref _definition, value); }
        public ObservableCollection<string> Examples { get => _examples; set => SetProperty(ref _examples, value); }
        public ObservableCollection<string> Synonyms { get => _synonyms; set => SetProperty(ref _synonyms, value); }
    }

    public class HyperlinkViewModel : ViewModelBase
    {
        private string _target;
        private HyperlinkType _type = HyperlinkType.External;
        private string _displayText;
        private string _tooltip;
        private string _anchor;
        private bool _history = true;

        public string Target { get => _target; set => SetProperty(ref _target, value); }
        public HyperlinkType Type { get => _type; set => SetProperty(ref _type, value); }
        public string DisplayText { get => _displayText; set => SetProperty(ref _displayText, value); }
        public string Tooltip { get => _tooltip; set => SetProperty(ref _tooltip, value); }
        public string Anchor { get => _anchor; set => SetProperty(ref _anchor, value); }
        public bool History { get => _history; set => SetProperty(ref _history, value); }
    }

    public enum HyperlinkType { External, Internal, Email, File }

    public class BookmarkViewModel : ViewModelBase
    {
        private string _name;
        private int _startIndex;
        private int _endIndex;
        private string _content;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public int StartIndex { get => _startIndex; set => SetProperty(ref _startIndex, value); }
        public int EndIndex { get => _endIndex; set => SetProperty(ref _endIndex, value); }
        public string Content { get => _content; set => SetProperty(ref _content, value); }
    }

    public class CommentViewModel : ViewModelBase
    {
        private string _id;
        private string _author;
        private string _initials;
        private DateTime _date;
        private string _text;
        private int _startIndex;
        private int _endIndex;
        private ObservableCollection<CommentReplyViewModel> _replies = new ObservableCollection<CommentReplyViewModel>();
        private bool _done;
        private bool _isSelected;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Author { get => _author; set => SetProperty(ref _author, value); }
        public string Initials { get => _initials; set => SetProperty(ref _initials, value); }
        public DateTime Date { get => _date; set => SetProperty(ref _date, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public int StartIndex { get => _startIndex; set => SetProperty(ref _startIndex, value); }
        public int EndIndex { get => _endIndex; set => SetProperty(ref _endIndex, value); }
        public ObservableCollection<CommentReplyViewModel> Replies { get => _replies; set => SetProperty(ref _replies, value); }
        public bool Done { get => _done; set => SetProperty(ref _done, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class CommentReplyViewModel : ViewModelBase
    {
        private string _id;
        private string _author;
        private string _initials;
        private DateTime _date;
        private string _text;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Author { get => _author; set => SetProperty(ref _author, value); }
        public string Initials { get => _initials; set => SetProperty(ref _initials, value); }
        public DateTime Date { get => _date; set => SetProperty(ref _date, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
    }

    public class FootnoteViewModel : ViewModelBase
    {
        private string _id;
        private string _text;
        private int _referenceIndex;
        private ParagraphViewModel _reference;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public int ReferenceIndex { get => _referenceIndex; set => SetProperty(ref _referenceIndex, value); }
        public ParagraphViewModel Reference { get => _reference; set => SetProperty(ref _reference, value); }
    }

    public class EndnoteViewModel : FootnoteViewModel { }

    public class MailMergeViewModel : ViewModelBase
    {
        private string _dataSource;
        private string _query;
        private ObservableCollection<MailMergeFieldViewModel> _fields = new ObservableCollection<MailMergeFieldViewModel>();
        private MailMergeState _state = MailMergeState.NotActive;
        private string _outputFile;
        private MailMergeOutputType _outputType = MailMergeOutputType.Letter;
        private int _recordCount;
        private int _currentRecord;

        public string DataSource { get => _dataSource; set => SetProperty(ref _dataSource, value); }
        public string Query { get => _query; set => SetProperty(ref _query, value); }
        public ObservableCollection<MailMergeFieldViewModel> Fields { get => _fields; set => SetProperty(ref _fields, value); }
        public MailMergeState State { get => _state; set => SetProperty(ref _state, value); }
        public string OutputFile { get => _outputFile; set => SetProperty(ref _outputFile, value); }
        public MailMergeOutputType OutputType { get => _outputType; set => SetProperty(ref _outputType, value); }
        public int RecordCount { get => _recordCount; set => SetProperty(ref _recordCount, value); }
        public int CurrentRecord { get => _currentRecord; set => SetProperty(ref _currentRecord, value); }
    }

    public class MailMergeFieldViewModel : ViewModelBase
    {
        private string _name;
        private string _mappedName;
        private string _type;
        private string _value;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string MappedName { get => _mappedName; set => SetProperty(ref _mappedName, value); }
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
    }

    public enum MailMergeState { NotActive, Active, Ready, InProgress, Completed }
    public enum MailMergeOutputType { Letter, Email, Fax, Catalog }

    public class TableOfContentsViewModel : ViewModelBase
    {
        private string _title = "Table of Contents";
        private bool _useHeadingStyles = true;
        private bool _useOutlineLevels;
        private int _startLevel = 1;
        private int _endLevel = 3;
        private bool _showPageNumbers = true;
        private bool _rightAlignPageNumbers;
        private string _tabLeader = "dot";
        private bool _hyperlinks;
        private string _formatStyle = "Automatic1";
        private bool _preserveFormatting;
        private bool _updateOnOpen;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public bool UseHeadingStyles { get => _useHeadingStyles; set => SetProperty(ref _useHeadingStyles, value); }
        public bool UseOutlineLevels { get => _useOutlineLevels; set => SetProperty(ref _useOutlineLevels, value); }
        public int StartLevel { get => _startLevel; set => SetProperty(ref _startLevel, value); }
        public int EndLevel { get => _endLevel; set => SetProperty(ref _endLevel, value); }
        public bool ShowPageNumbers { get => _showPageNumbers; set => SetProperty(ref _showPageNumbers, value); }
        public bool RightAlignPageNumbers { get => _rightAlignPageNumbers; set => SetProperty(ref _rightAlignPageNumbers, value); }
        public string TabLeader { get => _tabLeader; set => SetProperty(ref _tabLeader, value); }
        public bool Hyperlinks { get => _hyperlinks; set => SetProperty(ref _hyperlinks, value); }
        public string FormatStyle { get => _formatStyle; set => SetProperty(ref _formatStyle, value); }
        public bool PreserveFormatting { get => _preserveFormatting; set => SetProperty(ref _preserveFormatting, value); }
        public bool UpdateOnOpen { get => _updateOnOpen; set => SetProperty(ref _updateOnOpen, value); }
    }

    public class IndexViewModel : ViewModelBase
    {
        private string _title = "Index";
        private IndexType _type = IndexType.Alphabetic;
        private int _columns = 2;
        private bool _runIn;
        private string _heading = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z";
        private string _formatStyle;
        private bool _preserveFormatting;
        private string _language;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public IndexType Type { get => _type; set => SetProperty(ref _type, value); }
        public int Columns { get => _columns; set => SetProperty(ref _columns, value); }
        public bool RunIn { get => _runIn; set => SetProperty(ref _runIn, value); }
        public string Heading { get => _heading; set => SetProperty(ref _heading, value); }
        public string FormatStyle { get => _formatStyle; set => SetProperty(ref _formatStyle, value); }
        public bool PreserveFormatting { get => _preserveFormatting; set => SetProperty(ref _preserveFormatting, value); }
        public string Language { get => _language; set => SetProperty(ref _language, value); }
    }

    public enum IndexType { Alphabetic, Heading, Table, CrossReference }

    public class TableOfFiguresViewModel : ViewModelBase
    {
        private string _captionLabel = "Figure";
        private string _title = "Table of Figures";
        private bool _showPageNumbers = true;
        private bool _rightAlignPageNumbers;
        private string _tabLeader = "dot";
        private bool _hyperlinks;
        private bool _includeLabelAndNumber;

        public string CaptionLabel { get => _captionLabel; set => SetProperty(ref _captionLabel, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public bool ShowPageNumbers { get => _showPageNumbers; set => SetProperty(ref _showPageNumbers, value); }
        public bool RightAlignPageNumbers { get => _rightAlignPageNumbers; set => SetProperty(ref _rightAlignPageNumbers, value); }
        public string TabLeader { get => _tabLeader; set => SetProperty(ref _tabLeader, value); }
        public bool Hyperlinks { get => _hyperlinks; set => SetProperty(ref _hyperlinks, value); }
        public bool IncludeLabelAndNumber { get => _includeLabelAndNumber; set => SetProperty(ref _includeLabelAndNumber, value); }
    }
}

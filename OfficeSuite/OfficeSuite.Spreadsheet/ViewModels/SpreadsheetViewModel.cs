using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Spreadsheet.ViewModels
{
    public class SpreadsheetViewModel : ViewModelBase
    {
        private WorkbookViewModel _workbook;
        private WorksheetViewModel _activeWorksheet;
        private SelectionViewModel _selection;
        private CellViewModel _activeCell;
        private bool _isModified;
        private string _filePath;
        private double _zoomLevel;
        private string _statusMessage;
        private int _formulaBarMode;
        private bool _showFormulaBar;
        private bool _showStatusBar;
        private bool _showGridlines;
        private bool _showHeaders;
        private bool _showRowColHeaders;

        public WorkbookViewModel Workbook { get => _workbook; set => SetProperty(ref _workbook, value); }
        public WorksheetViewModel ActiveWorksheet { get => _activeWorksheet; set => SetProperty(ref _activeWorksheet, value); }
        public SelectionViewModel Selection { get => _selection; set => SetProperty(ref _selection, value); }
        public CellViewModel ActiveCell { get => _activeCell; set => SetProperty(ref _activeCell, value); }
        public bool IsModified { get => _isModified; set => SetProperty(ref _isModified, value); }
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public int FormulaBarMode { get => _formulaBarMode; set => SetProperty(ref _formulaBarMode, value); }
        public bool ShowFormulaBar { get => _showFormulaBar; set => SetProperty(ref _showFormulaBar, value); }
        public bool ShowStatusBar { get => _showStatusBar; set => SetProperty(ref _showStatusBar, value); }
        public bool ShowGridlines { get => _showGridlines; set => SetProperty(ref _showGridlines, value); }
        public bool ShowHeaders { get => _showHeaders; set => SetProperty(ref _showHeaders, value); }
        public bool ShowRowColHeaders { get => _showRowColHeaders; set => SetProperty(ref _showRowColHeaders, value); }

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
        public ICommand InsertRowCommand { get; }
        public ICommand InsertColumnCommand { get; }
        public ICommand DeleteRowCommand { get; }
        public ICommand DeleteColumnCommand { get; }
        public ICommand InsertCellCommand { get; }
        public ICommand DeleteCellCommand { get; }
        public ICommand InsertWorksheetCommand { get; }
        public ICommand DeleteWorksheetCommand { get; }
        public ICommand RenameWorksheetCommand { get; }
        public ICommand MoveWorksheetCommand { get; }
        public ICommand CopyWorksheetCommand { get; }
        public ICommand FormatCellCommand { get; }
        public ICommand FormatRowCommand { get; }
        public ICommand FormatColumnCommand { get; }
        public ICommand FormatRangeCommand { get; }
        public ICommand ApplyStyleCommand { get; }
        public ICommand ClearFormatCommand { get; }
        public ICommand ClearContentsCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand GoToCommand { get; }
        public ICommand AutoSumCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand AdvancedFilterCommand { get; }
        public ICommand SubtotalCommand { get; }
        public ICommand PivotTableCommand { get; }
        public ICommand DataValidationCommand { get; }
        public ICommand ConsolidateCommand { get; }
        public ICommand WhatIfAnalysisCommand { get; }
        public ICommand ChartInsertCommand { get; }
        public ICommand PivotChartCommand { get; }
        public ICommand SparklinesCommand { get; }
        public ICommand TableCommand { get; }
        public ICommand PictureCommand { get; }
        public ICommand ShapeCommand { get; }
        public ICommand SmartArtCommand { get; }
        public ICommand ScreenshotCommand { get; }
        public ICommand LinkCommand { get; }
        public ICommand HyperlinkCommand { get; }
        public ICommand CommentCommand { get; }
        public ICommand DefineNameCommand { get; }
        public ICommand NameManagerCommand { get; }
        public ICommand TracePrecedentsCommand { get; }
        public ICommand TraceDependentsCommand { get; }
        public ICommand RemoveArrowsCommand { get; }
        public ICommand ShowErrorsCommand { get; }
        public ICommand WatchWindowCommand { get; }
        public ICommand MacroCommand { get; }
        public ICommand MacroSecurityCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ZoomFitCommand { get; }
        public ICommand FreezePanesCommand { get; }
        public ICommand SplitCommand { get; }

        public SpreadsheetViewModel()
        {
            Workbook = new WorkbookViewModel();
            Selection = new SelectionViewModel();
            _zoomLevel = 100;
            _showFormulaBar = true;
            _showStatusBar = true;
            _showGridlines = true;
            _showHeaders = true;
            _showRowColHeaders = true;

            NewCommand = new RelayCommand(async _ => await NewWorkbookAsync());
            OpenCommand = new RelayCommand(async _ => await OpenWorkbookAsync());
            SaveCommand = new RelayCommand(async _ => await SaveWorkbookAsync(), _ => IsModified);
            SaveAsCommand = new RelayCommand(async _ => await SaveWorkbookAsAsync());
            PrintCommand = new RelayCommand(_ => Print());
            UndoCommand = new RelayCommand(_ => Undo());
            RedoCommand = new RelayCommand(_ => Redo());
            CutCommand = new RelayCommand(_ => Cut());
            CopyCommand = new RelayCommand(_ => Copy());
            PasteCommand = new RelayCommand(_ => Paste());
            DeleteCommand = new RelayCommand(_ => Delete());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            InsertRowCommand = new RelayCommand(_ => InsertRow());
            InsertColumnCommand = new RelayCommand(_ => InsertColumn());
            DeleteRowCommand = new RelayCommand(_ => DeleteRow());
            DeleteColumnCommand = new RelayCommand(_ => DeleteColumn());
            InsertCellCommand = new RelayCommand(_ => InsertCell());
            DeleteCellCommand = new RelayCommand(_ => DeleteCell());
            InsertWorksheetCommand = new RelayCommand(_ => InsertWorksheet());
            DeleteWorksheetCommand = new RelayCommand(_ => DeleteWorksheet());
            RenameWorksheetCommand = new RelayCommand(_ => RenameWorksheet());
            MoveWorksheetCommand = new RelayCommand(_ => MoveWorksheet());
            CopyWorksheetCommand = new RelayCommand(_ => CopyWorksheet());
            FormatCellCommand = new RelayCommand(_ => FormatCell());
            FormatRowCommand = new RelayCommand(_ => FormatRow());
            FormatColumnCommand = new RelayCommand(_ => FormatColumn());
            FormatRangeCommand = new RelayCommand(_ => FormatRange());
            ApplyStyleCommand = new RelayCommand(_ => ApplyStyle());
            ClearFormatCommand = new RelayCommand(_ => ClearFormat());
            ClearContentsCommand = new RelayCommand(_ => ClearContents());
            ClearAllCommand = new RelayCommand(_ => ClearAll());
            FindCommand = new RelayCommand(_ => Find());
            ReplaceCommand = new RelayCommand(_ => Replace());
            GoToCommand = new RelayCommand(_ => GoTo());
            AutoSumCommand = new RelayCommand(_ => AutoSum());
            SortCommand = new RelayCommand(_ => Sort());
            FilterCommand = new RelayCommand(_ => Filter());
            AdvancedFilterCommand = new RelayCommand(_ => AdvancedFilter());
            SubtotalCommand = new RelayCommand(_ => Subtotal());
            PivotTableCommand = new RelayCommand(_ => InsertPivotTable());
            DataValidationCommand = new RelayCommand(_ => DataValidation());
            ConsolidateCommand = new RelayCommand(_ => Consolidate());
            WhatIfAnalysisCommand = new RelayCommand(_ => WhatIfAnalysis());
            ChartInsertCommand = new RelayCommand(_ => InsertChart());
            PivotChartCommand = new RelayCommand(_ => InsertPivotChart());
            SparklinesCommand = new RelayCommand(_ => InsertSparklines());
            TableCommand = new RelayCommand(_ => InsertTable());
            PictureCommand = new RelayCommand(_ => InsertPicture());
            ShapeCommand = new RelayCommand(_ => InsertShape());
            SmartArtCommand = new RelayCommand(_ => InsertSmartArt());
            ScreenshotCommand = new RelayCommand(_ => InsertScreenshot());
            LinkCommand = new RelayCommand(_ => InsertLink());
            HyperlinkCommand = new RelayCommand(_ => InsertHyperlink());
            CommentCommand = new RelayCommand(_ => InsertComment());
            DefineNameCommand = new RelayCommand(_ => DefineName());
            NameManagerCommand = new RelayCommand(_ => ShowNameManager());
            TracePrecedentsCommand = new RelayCommand(_ => TracePrecedents());
            TraceDependentsCommand = new RelayCommand(_ => TraceDependents());
            RemoveArrowsCommand = new RelayCommand(_ => RemoveArrows());
            ShowErrorsCommand = new RelayCommand(_ => ShowErrors());
            WatchWindowCommand = new RelayCommand(_ => ShowWatchWindow());
            MacroCommand = new RelayCommand(_ => ShowMacros());
            MacroSecurityCommand = new RelayCommand(_ => ShowMacroSecurity());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            ZoomFitCommand = new RelayCommand(_ => ZoomFit());
            FreezePanesCommand = new RelayCommand(_ => FreezePanes());
            SplitCommand = new RelayCommand(_ => SplitWindow());
        }

        protected virtual async Task NewWorkbookAsync() { await Task.CompletedTask; }
        protected virtual async Task OpenWorkbookAsync() { await Task.CompletedTask; }
        protected virtual async Task SaveWorkbookAsync() { IsModified = false; await Task.CompletedTask; }
        protected virtual async Task SaveWorkbookAsAsync() { IsModified = false; await Task.CompletedTask; }
        protected virtual void Print() { }
        protected virtual void Undo() { }
        protected virtual void Redo() { }
        protected virtual void Cut() { }
        protected virtual void Copy() { }
        protected virtual void Paste() { }
        protected virtual void Delete() { }
        protected virtual void SelectAll() { }
        protected virtual void InsertRow() { }
        protected virtual void InsertColumn() { }
        protected virtual void DeleteRow() { }
        protected virtual void DeleteColumn() { }
        protected virtual void InsertCell() { }
        protected virtual void DeleteCell() { }
        protected virtual void InsertWorksheet() { }
        protected virtual void DeleteWorksheet() { }
        protected virtual void RenameWorksheet() { }
        protected virtual void MoveWorksheet() { }
        protected virtual void CopyWorksheet() { }
        protected virtual void FormatCell() { }
        protected virtual void FormatRow() { }
        protected virtual void FormatColumn() { }
        protected virtual void FormatRange() { }
        protected virtual void ApplyStyle() { }
        protected virtual void ClearFormat() { }
        protected virtual void ClearContents() { }
        protected virtual void ClearAll() { }
        protected virtual void Find() { }
        protected virtual void Replace() { }
        protected virtual void GoTo() { }
        protected virtual void AutoSum() { }
        protected virtual void Sort() { }
        protected virtual void Filter() { }
        protected virtual void AdvancedFilter() { }
        protected virtual void Subtotal() { }
        protected virtual void InsertPivotTable() { }
        protected virtual void DataValidation() { }
        protected virtual void Consolidate() { }
        protected virtual void WhatIfAnalysis() { }
        protected virtual void InsertChart() { }
        protected virtual void InsertPivotChart() { }
        protected virtual void InsertSparklines() { }
        protected virtual void InsertTable() { }
        protected virtual void InsertPicture() { }
        protected virtual void InsertShape() { }
        protected virtual void InsertSmartArt() { }
        protected virtual void InsertScreenshot() { }
        protected virtual void InsertLink() { }
        protected virtual void InsertHyperlink() { }
        protected virtual void InsertComment() { }
        protected virtual void DefineName() { }
        protected virtual void ShowNameManager() { }
        protected virtual void TracePrecedents() { }
        protected virtual void TraceDependents() { }
        protected virtual void RemoveArrows() { }
        protected virtual void ShowErrors() { }
        protected virtual void ShowWatchWindow() { }
        protected virtual void ShowMacros() { }
        protected virtual void ShowMacroSecurity() { }
        protected virtual void ZoomIn() { if (ZoomLevel < 400) ZoomLevel += 10; }
        protected virtual void ZoomOut() { if (ZoomLevel > 10) ZoomLevel -= 10; }
        protected virtual void ZoomFit() { ZoomLevel = 100; }
        protected virtual void FreezePanes() { }
        protected virtual void SplitWindow() { }
    }

    public class WorkbookViewModel : ViewModelBase
    {
        private string _name;
        private ObservableCollection<WorksheetViewModel> _worksheets = new ObservableCollection<WorksheetViewModel>();
        private WorksheetViewModel _activeWorksheet;
        private List<DefinedNameViewModel> _definedNames = new List<DefinedNameViewModel>();
        private WorkbookPropertiesViewModel _properties;
        private List<DataConnectionViewModel> _dataConnections = new List<DataConnectionViewModel>();
        private List<TableViewModel> _tables = new List<TableViewModel>();
        private List<ChartViewModel> _charts = new List<ChartViewModel>();

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ObservableCollection<WorksheetViewModel> Worksheets { get => _worksheets; set => SetProperty(ref _worksheets, value); }
        public WorksheetViewModel ActiveWorksheet { get => _activeWorksheet; set => SetProperty(ref _activeWorksheet, value); }
        public List<DefinedNameViewModel> DefinedNames { get => _definedNames; set => SetProperty(ref _definedNames, value); }
        public WorkbookPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public List<DataConnectionViewModel> DataConnections { get => _dataConnections; set => SetProperty(ref _dataConnections, value); }
        public List<TableViewModel> Tables { get => _tables; set => SetProperty(ref _tables, value); }
        public List<ChartViewModel> Charts { get => _charts; set => SetProperty(ref _charts, value); }
    }

    public class WorksheetViewModel : ViewModelBase
    {
        private string _name;
        private string _id;
        private CellRange _usedRange;
        private CellViewModel _defaultCell;
        private ObservableCollection<RowViewModel> _rows = new ObservableCollection<RowViewModel>();
        private ObservableCollection<ColumnViewModel> _columns = new ObservableCollection<ColumnViewModel>();
        private Dictionary<string, CellViewModel> _cells = new Dictionary<string, CellViewModel>();
        private bool _isVisible = true;
        private bool _isActive;
        private TabColor _tabColor;
        private ViewMode _viewMode = ViewMode.Normal;
        private PageBreakCollection _horizontalBreaks = new PageBreakCollection();
        private PageBreakCollection _verticalBreaks = new PageBreakCollection();
        private GridlineColor _gridlineColor;
        private ZoomViewModel _zoom;
        private HeaderFooterViewModel _headerFooter;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public CellRange UsedRange { get => _usedRange; set => SetProperty(ref _usedRange, value); }
        public CellViewModel DefaultCell { get => _defaultCell; set => SetProperty(ref _defaultCell, value); }
        public ObservableCollection<RowViewModel> Rows { get => _rows; set => SetProperty(ref _rows, value); }
        public ObservableCollection<ColumnViewModel> Columns { get => _columns; set => SetProperty(ref _columns, value); }
        public Dictionary<string, CellViewModel> Cells { get => _cells; set => SetProperty(ref _cells, value); }
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
        public TabColor TabColor { get => _tabColor; set => SetProperty(ref _tabColor, value); }
        public ViewMode ViewMode { get => _viewMode; set => SetProperty(ref _viewMode, value); }
        public PageBreakCollection HorizontalBreaks { get => _horizontalBreaks; set => SetProperty(ref _horizontalBreaks, value); }
        public PageBreakCollection VerticalBreaks { get => _verticalBreaks; set => SetProperty(ref _verticalBreaks, value); }
        public GridlineColor GridlineColor { get => _gridlineColor; set => SetProperty(ref _gridlineColor, value); }
        public ZoomViewModel Zoom { get => _zoom; set => SetProperty(ref _zoom, value); }
        public HeaderFooterViewModel HeaderFooter { get => _headerFooter; set => SetProperty(ref _headerFooter, value); }

        public CellViewModel GetCell(string cellReference)
        {
            if (_cells.TryGetValue(cellReference, out var cell))
                return cell;
            return null;
        }

        public CellViewModel GetCell(int row, int column)
        {
            var reference = GetCellReference(row, column);
            return GetCell(reference);
        }

        public void SetCell(string cellReference, CellViewModel cell)
        {
            _cells[cellReference] = cell;
        }

        public static string GetCellReference(int row, int column)
        {
            return GetColumnLetter(column) + row.ToString();
        }

        public static string GetColumnLetter(int column)
        {
            column--;
            string result = "";
            while (column >= 0)
            {
                result = (char)('A' + column % 26) + result;
                column = column / 26 - 1;
            }
            return result;
        }

        public static (int row, int column) ParseCellReference(string cellReference)
        {
            var match = System.Text.RegularExpressions.Regex.Match(cellReference, @"([A-Za-z]+)(\d+)");
            if (match.Success)
            {
                var colStr = match.Groups[1].Value;
                var row = int.Parse(match.Groups[2].Value);
                var col = ParseColumn(colStr);
                return (row, col);
            }
            return (1, 1);
        }

        private static int ParseColumn(string column)
        {
            int result = 0;
            foreach (char c in column)
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result;
        }
    }

    public class RowViewModel : ViewModelBase
    {
        private int _index;
        private double _height;
        private bool _hidden;
        private bool _filtered;
        private OutlineLevel _outlineLevel = OutlineLevel.None;
        private RowPropertiesViewModel _properties;

        public int Index { get => _index; set => SetProperty(ref _index, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
        public bool Filtered { get => _filtered; set => SetProperty(ref _filtered, value); }
        public OutlineLevel OutlineLevel { get => _outlineLevel; set => SetProperty(ref _outlineLevel, value); }
        public RowPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
    }

    public class ColumnViewModel : ViewModelBase
    {
        private int _index;
        private double _width;
        private bool _hidden;
        private string _styleId;
        private OutlineLevel _outlineLevel = OutlineLevel.None;
        private ColumnPropertiesViewModel _properties;

        public int Index { get => _index; set => SetProperty(ref _index, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
        public string StyleId { get => _styleId; set => SetProperty(ref _styleId, value); }
        public OutlineLevel OutlineLevel { get => _outlineLevel; set => SetProperty(ref _outlineLevel, value); }
        public ColumnPropertiesViewModel Properties { get => _properties; set => SetProperty(ref _properties, value); }
    }

    public enum OutlineLevel { None, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8, Level9 }

    public class CellViewModel : ViewModelBase
    {
        private string _reference;
        private int _row;
        private int _column;
        private object _value;
        private string _formula;
        private string _displayValue;
        private CellType _cellType = CellType.General;
        private CellStyleViewModel _style;
        private bool _isSelected;
        private bool _isPartOfArrayFormula;
        private string _arrayFormulaRange;
        private CommentViewModel _comment;
        private List<ValidationViewModel> _validations = new List<ValidationViewModel>();

        public string Reference { get => _reference; set => SetProperty(ref _reference, value); }
        public int Row { get => _row; set => SetProperty(ref _row, value); }
        public int Column { get => _column; set => SetProperty(ref _column, value); }
        public object Value { get => _value; set => SetProperty(ref _value, value); }
        public string Formula { get => _formula; set => SetProperty(ref _formula, value); }
        public string DisplayValue { get => _displayValue; set => SetProperty(ref _displayValue, value); }
        public CellType CellType { get => _cellType; set => SetProperty(ref _cellType, value); }
        public CellStyleViewModel Style { get => _style; set => SetProperty(ref _style, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public bool IsPartOfArrayFormula { get => _isPartOfArrayFormula; set => SetProperty(ref _isPartOfArrayFormula, value); }
        public string ArrayFormulaRange { get => _arrayFormulaRange; set => SetProperty(ref _arrayFormulaRange, value); }
        public CommentViewModel Comment { get => _comment; set => SetProperty(ref _comment, value); }
        public List<ValidationViewModel> Validations { get => _validations; set => SetProperty(ref _validations, value); }

        public string GetTextValue()
        {
            if (Value == null) return "";
            if (Value is string s) return s;
            if (Value is double d) return d.ToString();
            if (Value is DateTime dt) return dt.ToString();
            return Value.ToString();
        }

        public double GetNumericValue()
        {
            if (Value is double d) return d;
            if (Value is int i) return i;
            if (double.TryParse(Value?.ToString(), out double result)) return result;
            return 0;
        }
    }

    public enum CellType { General, Number, Text, Boolean, Date, Time, Currency, Percentage, Scientific, Fraction, ZipCode, PhoneNumber, Custom }

    public class CellStyleViewModel : ViewModelBase
    {
        private string _id;
        private string _name;
        private FontViewModel _font;
        private FillViewModel _fill;
        private BorderViewModel _border;
        private AlignmentViewModel _alignment;
        private NumberFormatViewModel _numberFormat;
        private ProtectionViewModel _protection;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public FontViewModel Font { get => _font; set => SetProperty(ref _font, value); }
        public FillViewModel Fill { get => _fill; set => SetProperty(ref _fill, value); }
        public BorderViewModel Border { get => _border; set => SetProperty(ref _border, value); }
        public AlignmentViewModel Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
        public NumberFormatViewModel NumberFormat { get => _numberFormat; set => SetProperty(ref _numberFormat, value); }
        public ProtectionViewModel Protection { get => _protection; set => SetProperty(ref _protection, value); }
    }

    public class FontViewModel : ViewModelBase
    {
        private string _name = "Calibri";
        private double _size = 11;
        private bool _bold;
        private bool _italic;
        private bool _underline;
        private UnderlineStyle _underlineStyle = UnderlineStyle.None;
        private bool _strikethrough;
        private Color _color;
        private Color _backgroundColor;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Baseline;
        private bool _superscript;
        private bool _subscript;
        private ScriptType _scriptType = ScriptType.None;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public double Size { get => _size; set => SetProperty(ref _size, value); }
        public bool Bold { get => _bold; set => SetProperty(ref _bold, value); }
        public bool Italic { get => _italic; set => SetProperty(ref _italic, value); }
        public bool Underline { get => _underline; set => SetProperty(ref _underline, value); }
        public UnderlineStyle UnderlineStyle { get => _underlineStyle; set => SetProperty(ref _underlineStyle, value); }
        public bool Strikethrough { get => _strikethrough; set => SetProperty(ref _strikethrough, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public Color BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }
        public VerticalAlignment VerticalAlignment { get => _verticalAlignment; set => SetProperty(ref _verticalAlignment, value); }
        public bool Superscript { get => _superscript; set => SetProperty(ref _superscript, value); }
        public bool Subscript { get => _subscript; set => SetProperty(ref _subscript, value); }
        public ScriptType ScriptType { get => _scriptType; set => SetProperty(ref _scriptType, value); }
    }

    public enum UnderlineStyle { None, Single, Double, SingleAccounting, DoubleAccounting }
    public enum VerticalAlignment { Top, Center, Bottom, Baseline }
    public enum ScriptType { None, Superscript, Subscript }

    public class FillViewModel : ViewModelBase
    {
        private FillType _fillType = FillType.None;
        private Color _foregroundColor;
        private Color _backgroundColor;
        private GradientFillViewModel _gradient;
        private PatternFillViewModel _pattern;

        public FillType FillType { get => _fillType; set => SetProperty(ref _fillType, value); }
        public Color ForegroundColor { get => _foregroundColor; set => SetProperty(ref _foregroundColor, value); }
        public Color BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }
        public GradientFillViewModel Gradient { get => _gradient; set => SetProperty(ref _gradient, value); }
        public PatternFillViewModel Pattern { get => _pattern; set => SetProperty(ref _pattern, value); }
    }

    public enum FillType { None, Solid, Gradient, Pattern }

    public class GradientFillViewModel : ViewModelBase
    {
        private GradientType _type = GradientType.Linear;
        private double _angle;
        private ObservableCollection<GradientStopViewModel> _stops = new ObservableCollection<GradientStopViewModel>();

        public GradientType Type { get => _type; set => SetProperty(ref _type, value); }
        public double Angle { get => _angle; set => SetProperty(ref _angle, value); }
        public ObservableCollection<GradientStopViewModel> Stops { get => _stops; set => SetProperty(ref _stops, value); }
    }

    public enum GradientType { Linear, Radial }

    public class GradientStopViewModel : ViewModelBase
    {
        private double _position;
        private Color _color;

        public double Position { get => _position; set => SetProperty(ref _position, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
    }

    public class PatternFillViewModel : ViewModelBase
    {
        private PatternType _patternType = PatternType.DiagonalCrosshatch;
        private Color _foregroundColor;
        private Color _backgroundColor;

        public PatternType PatternType { get => _patternType; set => SetProperty(ref _patternType, value); }
        public Color ForegroundColor { get => _foregroundColor; set => SetProperty(ref _foregroundColor, value); }
        public Color BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }
    }

    public enum PatternType
    {
        DiagonalCrosshatch, DiagonalStripe, DownwardDiagonal, DottedDiamond, DottedGrid,
        HorizontalBrick, HorizontalStripe, LargeCheckerBoard, LargeConfetti, LargeGrid,
        LightDiagonalDown, LightDiagonalUp, LightDownwardDiagonal, LightHorizontal, LightUpwardDiagonal,
        LightVertical, None, ReverseDiagonalStripe, SolidDiamond, SparseGrid,
        VerticalStripe, Wave, Weave, WideDownwardDiagonal, WideUpwardDiagonal, ZigZag
    }

    public class BorderViewModel : ViewModelBase
    {
        private BorderStyle _topStyle = BorderStyle.None;
        private BorderStyle _bottomStyle = BorderStyle.None;
        private BorderStyle _leftStyle = BorderStyle.None;
        private BorderStyle _rightStyle = BorderStyle.None;
        private BorderStyle _diagonalStyle = BorderStyle.None;
        private Color _topColor;
        private Color _bottomColor;
        private Color _leftColor;
        private Color _rightColor;
        private Color _diagonalColor;
        private DiagonalBorderType _diagonalType = DiagonalBorderType.None;

        public BorderStyle TopStyle { get => _topStyle; set => SetProperty(ref _topStyle, value); }
        public BorderStyle BottomStyle { get => _bottomStyle; set => SetProperty(ref _bottomStyle, value); }
        public BorderStyle LeftStyle { get => _leftStyle; set => SetProperty(ref _leftStyle, value); }
        public BorderStyle RightStyle { get => _rightStyle; set => SetProperty(ref _rightStyle, value); }
        public BorderStyle DiagonalStyle { get => _diagonalStyle; set => SetProperty(ref _diagonalStyle, value); }
        public Color TopColor { get => _topColor; set => SetProperty(ref _topColor, value); }
        public Color BottomColor { get => _bottomColor; set => SetProperty(ref _bottomColor, value); }
        public Color LeftColor { get => _leftColor; set => SetProperty(ref _leftColor, value); }
        public Color RightColor { get => _rightColor; set => SetProperty(ref _rightColor, value); }
        public Color DiagonalColor { get => _diagonalColor; set => SetProperty(ref _diagonalColor, value); }
        public DiagonalBorderType DiagonalType { get => _diagonalType; set => SetProperty(ref _diagonalType, value); }
    }

    public enum BorderStyle
    {
        None, Thin, Medium, Thick, Double, Hair, MediumDashed, DashDot, MediumDashDot,
        DashDotDot, MediumDashDotDot, SlantDashDot, Dotted, Dashed, MediumThin, ThinThick,
        ThickThin, Outset, Inset
    }

    public enum DiagonalBorderType { None, Down, Up, Both }

    public class AlignmentViewModel : ViewModelBase
    {
        private HorizontalAlignment _horizontal = HorizontalAlignment.General;
        private VerticalAlignment _vertical = VerticalAlignment.Bottom;
        private int _indent;
        private bool _justify;
        private bool _shrinkToFit;
        private TextOrientation _textOrientation = TextOrientation.Horizontal;
        private TextRotation _textRotation = TextRotation.None;
        private WrapText _wrapText = WrapText.None;
        private ReadOrder _readOrder = ReadOrder.Context;

        public HorizontalAlignment Horizontal { get => _horizontal; set => SetProperty(ref _horizontal, value); }
        public VerticalAlignment Vertical { get => _vertical; set => SetProperty(ref _vertical, value); }
        public int Indent { get => _indent; set => SetProperty(ref _indent, value); }
        public bool Justify { get => _justify; set => SetProperty(ref _justify, value); }
        public bool ShrinkToFit { get => _shrinkToFit; set => SetProperty(ref _shrinkToFit, value); }
        public TextOrientation TextOrientation { get => _textOrientation; set => SetProperty(ref _textOrientation, value); }
        public TextRotation TextRotation { get => _textRotation; set => SetProperty(ref _textRotation, value); }
        public WrapText WrapText { get => _wrapText; set => SetProperty(ref _wrapText, value); }
        public ReadOrder ReadOrder { get => _readOrder; set => SetProperty(ref _readOrder, value); }
    }

    public enum HorizontalAlignment { General, Left, Center, Right, Fill, Justify, CenterContinuous, Distributed }
    public enum TextOrientation { Horizontal, Vertical, Downward, Upward, Rotated90, Rotated270, Stacked }
    public enum TextRotation { None, LettersUpward, LettersDownward }
    public enum WrapText { None, Wrap, Overflow }
    public enum ReadOrder { Context, LeftToRight, RightToLeft }

    public class NumberFormatViewModel : ViewModelBase
    {
        private string _formatCode = "General";
        private NumberFormatType _type = NumberFormatType.General;
        private int _decimalPlaces = 2;
        private bool _useThousandsSeparator;
        private string _currencySymbol = "$";
        private bool _negativeAsRed;

        public string FormatCode { get => _formatCode; set => SetProperty(ref _formatCode, value); }
        public NumberFormatType Type { get => _type; set => SetProperty(ref _type, value); }
        public int DecimalPlaces { get => _decimalPlaces; set => SetProperty(ref _decimalPlaces, value); }
        public bool UseThousandsSeparator { get => _useThousandsSeparator; set => SetProperty(ref _useThousandsSeparator, value); }
        public string CurrencySymbol { get => _currencySymbol; set => SetProperty(ref _currencySymbol, value); }
        public bool NegativeAsRed { get => _negativeAsRed; set => SetProperty(ref _negativeAsRed, value); }
    }

    public enum NumberFormatType
    {
        General, Number, Currency, Accounting, Date, Time, Percentage, Fraction, Scientific, Text, Special, Custom
    }

    public class ProtectionViewModel : ViewModelBase
    {
        private bool _locked;
        private bool _hidden;
        private bool _formulaHidden;
        private bool _formulaLocked;

        public bool Locked { get => _locked; set => SetProperty(ref _locked, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
        public bool FormulaHidden { get => _formulaHidden; set => SetProperty(ref _formulaHidden, value); }
        public bool FormulaLocked { get => _formulaLocked; set => SetProperty(ref _formulaLocked, value); }
    }

    public class Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } = 255;
        public string Theme { get; set; }
        public int? ThemeIndex { get; set; }
        public double? Tint { get; set; }

        public static Color FromRgb(byte r, byte g, byte b) => new Color { R = r, G = g, B = b };
        public static Color FromArgb(byte a, byte r, byte g, byte b) => new Color { A = a, R = r, G = g, B = b };
        public static Color FromHex(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length == 6)
            {
                return new Color
                {
                    R = Convert.ToByte(hex.Substring(0, 2), 16),
                    G = Convert.ToByte(hex.Substring(2, 2), 16),
                    B = Convert.ToByte(hex.Substring(4, 2), 16)
                };
            }
            return new Color();
        }

        public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";
        public override string ToString() => ToHex();
    }

    public class SelectionViewModel : ViewModelBase
    {
        private CellRange _range;
        private string _activeCellAddress;
        private bool _hasSelection;
        private bool _isSelectionRectangular;
        private ObservableCollection<CellRange> _areas = new ObservableCollection<CellRange>();
        private CellRange _entireRow;
        private CellRange _entireColumn;
        private CellRange _entireSheet;

        public CellRange Range { get => _range; set => SetProperty(ref _range, value); }
        public string ActiveCellAddress { get => _activeCellAddress; set => SetProperty(ref _activeCellAddress, value); }
        public bool HasSelection { get => _hasSelection; set => SetProperty(ref _hasSelection, value); }
        public bool IsSelectionRectangular { get => _isSelectionRectangular; set => SetProperty(ref _isSelectionRectangular, value); }
        public ObservableCollection<CellRange> Areas { get => _areas; set => SetProperty(ref _areas, value); }
        public CellRange EntireRow { get => _entireRow; set => SetProperty(ref _entireRow, value); }
        public CellRange EntireColumn { get => _entireColumn; set => SetProperty(ref _entireColumn, value); }
        public CellRange EntireSheet { get => _entireSheet; set => SetProperty(ref _entireSheet, value); }
    }

    public class CellRange
    {
        public string Reference { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public bool IsEntireRow => StartColumn == 1 && EndColumn == 16384;
        public bool IsEntireColumn => StartRow == 1 && EndRow == 1048576;
        public int RowCount => EndRow - StartRow + 1;
        public int ColumnCount => EndColumn - StartColumn + 1;

        public static CellRange FromReference(string reference)
        {
            var parts = reference.Split(':');
            if (parts.Length == 2)
            {
                var start = ParseCellReference(parts[0]);
                var end = ParseCellReference(parts[1]);
                return new CellRange
                {
                    Reference = reference,
                    StartRow = start.row,
                    EndRow = end.row,
                    StartColumn = start.column,
                    EndColumn = end.column
                };
            }
            else
            {
                var cell = ParseCellReference(reference);
                return new CellRange
                {
                    Reference = reference,
                    StartRow = cell.row,
                    EndRow = cell.row,
                    StartColumn = cell.column,
                    EndColumn = cell.column
                };
            }
        }

        private static (int row, int column) ParseCellReference(string reference)
        {
            var match = System.Text.RegularExpressions.Regex.Match(reference, @"([A-Za-z]+)(\d+)");
            if (match.Success)
            {
                var colStr = match.Groups[1].Value;
                var row = int.Parse(match.Groups[2].Value);
                var col = ParseColumn(colStr);
                return (row, col);
            }
            return (1, 1);
        }

        private static int ParseColumn(string column)
        {
            int result = 0;
            foreach (char c in column)
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result;
        }

        public bool Contains(int row, int column)
        {
            return row >= StartRow && row <= EndRow && column >= StartColumn && column <= EndColumn;
        }

        public bool Intersects(CellRange other)
        {
            return StartRow <= other.EndRow && EndRow >= other.StartRow &&
                   StartColumn <= other.EndColumn && EndColumn >= other.StartColumn;
        }
    }

    public class CellRangeCollection : List<CellRange> { }

    public class TableViewModel : ViewModelBase
    {
        private string _name;
        private string _id;
        private CellRange _range;
        private TableStyleViewModel _style;
        private bool _showHeaderRow = true;
        private bool _showTotalRow;
        private bool _showBandedRows = true;
        private bool _showFirstColumn;
        private bool _showLastColumn;
        private bool _showBandedColumns;
        private bool _autoFilter;
        private ObservableCollection<TableColumnViewModel> _columns = new ObservableCollection<TableColumnViewModel>();

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public CellRange Range { get => _range; set => SetProperty(ref _range, value); }
        public TableStyleViewModel Style { get => _style; set => SetProperty(ref _style, value); }
        public bool ShowHeaderRow { get => _showHeaderRow; set => SetProperty(ref _showHeaderRow, value); }
        public bool ShowTotalRow { get => _showTotalRow; set => SetProperty(ref _showTotalRow, value); }
        public bool ShowBandedRows { get => _showBandedRows; set => SetProperty(ref _showBandedRows, value); }
        public bool ShowFirstColumn { get => _showFirstColumn; set => SetProperty(ref _showFirstColumn, value); }
        public bool ShowLastColumn { get => _showLastColumn; set => SetProperty(ref _showLastColumn, value); }
        public bool ShowBandedColumns { get => _showBandedColumns; set => SetProperty(ref _showBandedColumns, value); }
        public bool AutoFilter { get => _autoFilter; set => SetProperty(ref _autoFilter, value); }
        public ObservableCollection<TableColumnViewModel> Columns { get => _columns; set => SetProperty(ref _columns, value); }
    }

    public class TableStyleViewModel : ViewModelBase
    {
        private string _name;
        private string _displayName;
        private bool _isDefault;
        private TableStyleElementViewModel _headerRow;
        private TableStyleElementViewModel _totalRow;
        private TableStyleElementViewModel _bandedRow;
        private TableStyleElementViewModel _firstColumn;
        private TableStyleElementViewModel _lastColumn;
        private TableStyleElementViewModel _bandedColumn;
        private TableStyleElementViewModel _firstHeaderCell;
        private TableStyleElementViewModel _lastHeaderCell;
        private TableStyleElementViewModel _firstTotalCell;
        private TableStyleElementViewModel _lastTotalCell;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public bool IsDefault { get => _isDefault; set => SetProperty(ref _isDefault, value); }
        public TableStyleElementViewModel HeaderRow { get => _headerRow; set => SetProperty(ref _headerRow, value); }
        public TableStyleElementViewModel TotalRow { get => _totalRow; set => SetProperty(ref _totalRow, value); }
        public TableStyleElementViewModel BandedRow { get => _bandedRow; set => SetProperty(ref _bandedRow, value); }
        public TableStyleElementViewModel FirstColumn { get => _firstColumn; set => SetProperty(ref _firstColumn, value); }
        public TableStyleElementViewModel LastColumn { get => _lastColumn; set => SetProperty(ref _lastColumn, value); }
        public TableStyleElementViewModel BandedColumn { get => _bandedColumn; set => SetProperty(ref _bandedColumn, value); }
        public TableStyleElementViewModel FirstHeaderCell { get => _firstHeaderCell; set => SetProperty(ref _firstHeaderCell, value); }
        public TableStyleElementViewModel LastHeaderCell { get => _lastHeaderCell; set => SetProperty(ref _lastHeaderCell, value); }
        public TableStyleElementViewModel FirstTotalCell { get => _firstTotalCell; set => SetProperty(ref _firstTotalCell, value); }
        public TableStyleElementViewModel LastTotalCell { get => _lastTotalCell; set => SetProperty(ref _lastTotalCell, value); }
    }

    public class TableStyleElementViewModel : ViewModelBase
    {
        private CellStyleViewModel _style;
        private bool _themeOnly;

        public CellStyleViewModel Style { get => _style; set => SetProperty(ref _style, value); }
        public bool ThemeOnly { get => _themeOnly; set => SetProperty(ref _themeOnly, value); }
    }

    public class TableColumnViewModel : ViewModelBase
    {
        private string _name;
        private string _id;
        private CellRange _totalsRowFunction;
        private string _totalsRowLabel;
        private bool _showFilterButton;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public CellRange TotalsRowFunction { get => _totalsRowFunction; set => SetProperty(ref _totalsRowFunction, value); }
        public string TotalsRowLabel { get => _totalsRowLabel; set => SetProperty(ref _totalsRowLabel, value); }
        public bool ShowFilterButton { get => _showFilterButton; set => SetProperty(ref _showFilterButton, value); }
    }

    public class ChartViewModel : ViewModelBase
    {
        private string _name;
        private ChartType _chartType = ChartType.Column;
        private ChartSubtype _subtype = ChartSubtype.Clustered;
        private CellRange _dataRange;
        private CellRange _titleRange;
        private ChartTitleViewModel _title;
        private ChartLegendViewModel _legend;
        private ChartPlotAreaViewModel _plotArea;
        private ChartAxesViewModel _axes;
        private ChartSeriesCollection _series = new ChartSeriesCollection();
        private bool _showLegend = true;
        private bool _showTitle;
        private bool _showDataLabels;
        private bool _showDataTable;
        private bool _autoScaling = true;
        private bool _upDownBars;
        private ChartAreaViewModel _chartArea;
        private FloorViewModel _floor;
        private SideWallViewModel _sideWall;
        private BackWallViewModel _backWall;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ChartType ChartType { get => _chartType; set => SetProperty(ref _chartType, value); }
        public ChartSubtype Subtype { get => _subtype; set => SetProperty(ref _subtype, value); }
        public CellRange DataRange { get => _dataRange; set => SetProperty(ref _dataRange, value); }
        public CellRange TitleRange { get => _titleRange; set => SetProperty(ref _titleRange, value); }
        public ChartTitleViewModel Title { get => _title; set => SetProperty(ref _title, value); }
        public ChartLegendViewModel Legend { get => _legend; set => SetProperty(ref _legend, value); }
        public ChartPlotAreaViewModel PlotArea { get => _plotArea; set => SetProperty(ref _plotArea, value); }
        public ChartAxesViewModel Axes { get => _axes; set => SetProperty(ref _axes, value); }
        public ChartSeriesCollection Series { get => _series; set => SetProperty(ref _series, value); }
        public bool ShowLegend { get => _showLegend; set => SetProperty(ref _showLegend, value); }
        public bool ShowTitle { get => _showTitle; set => SetProperty(ref _showTitle, value); }
        public bool ShowDataLabels { get => _showDataLabels; set => SetProperty(ref _showDataLabels, value); }
        public bool ShowDataTable { get => _showDataTable; set => SetProperty(ref _showDataTable, value); }
        public bool AutoScaling { get => _autoScaling; set => SetProperty(ref _autoScaling, value); }
        public bool UpDownBars { get => _upDownBars; set => SetProperty(ref _upDownBars, value); }
        public ChartAreaViewModel ChartArea { get => _chartArea; set => SetProperty(ref _chartArea, value); }
        public FloorViewModel Floor { get => _floor; set => SetProperty(ref _floor, value); }
        public SideWallViewModel SideWall { get => _sideWall; set => SetProperty(ref _sideWall, value); }
        public BackWallViewModel BackWall { get => _backWall; set => SetProperty(ref _backWall, value); }
    }

    public enum ChartType
    {
        Column, Bar, Line, Pie, Area, Scatter, Stock, Radar, Surface, Doughnut,
        Bubble, Treemap, Sunburst, Histogram, BoxAndWhisker, Waterfall, Funnel,
        Map, RadarFilled, Gauge, Pictograph, Gantt, Pareto, Region, Mekko
    }

    public enum ChartSubtype
    {
        None, Clustered, Stacked, PercentStacked, Standard, Markers, Smooth,
        SmoothMarkers, Exploded, Straight, Curved, WithMarkers, StackedWithMarkers,
        PieOfPie, BarOfPie, 3DColumn, 3DBar, 3DLine, 3DPie, 3DArea, RadarMarkers
    }

    public class ChartSeriesCollection : List<ChartSeriesViewModel> { }

    public class ChartSeriesViewModel : ViewModelBase
    {
        private string _name;
        private CellRange _valuesRange;
        private CellRange _xValuesRange;
        private CellRange _bubbleSizeRange;
        private CellRange _categoryLabelsRange;
        private ChartType _chartType;
        private ChartSubtype _subtype;
        private bool _showLegendEntry = true;
        private bool _showDataLabels;
        private ChartLineViewModel _line;
        private ChartMarkerViewModel _marker;
        private ChartDataLabelViewModel _dataLabel;
        private TrendlineViewModelCollection _trendlines = new TrendlineViewModelCollection();
        private ErrorBarViewModel _errorBarX;
        private ErrorBarViewModel _errorBarY;
        private UpBarViewModel _upBar;
        private DownBarViewModel _downBar;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public CellRange ValuesRange { get => _valuesRange; set => SetProperty(ref _valuesRange, value); }
        public CellRange XValuesRange { get => _xValuesRange; set => SetProperty(ref _xValuesRange, value); }
        public CellRange BubbleSizeRange { get => _bubbleSizeRange; set => SetProperty(ref _bubbleSizeRange, value); }
        public CellRange CategoryLabelsRange { get => _categoryLabelsRange; set => SetProperty(ref _categoryLabelsRange, value); }
        public ChartType ChartType { get => _chartType; set => SetProperty(ref _chartType, value); }
        public ChartSubtype Subtype { get => _subtype; set => SetProperty(ref _subtype, value); }
        public bool ShowLegendEntry { get => _showLegendEntry; set => SetProperty(ref _showLegendEntry, value); }
        public bool ShowDataLabels { get => _showDataLabels; set => SetProperty(ref _showDataLabels, value); }
        public ChartLineViewModel Line { get => _line; set => SetProperty(ref _line, value); }
        public ChartMarkerViewModel Marker { get => _marker; set => SetProperty(ref _marker, value); }
        public ChartDataLabelViewModel DataLabel { get => _dataLabel; set => SetProperty(ref _dataLabel, value); }
        public TrendlineViewModelCollection Trendlines { get => _trendlines; set => SetProperty(ref _trendlines, value); }
        public ErrorBarViewModel ErrorBarX { get => _errorBarX; set => SetProperty(ref _errorBarX, value); }
        public ErrorBarViewModel ErrorBarY { get => _errorBarY; set => SetProperty(ref _errorBarY, value); }
        public UpBarViewModel UpBar { get => _upBar; set => SetProperty(ref _upBar, value); }
        public DownBarViewModel DownBar { get => _downBar; set => SetProperty(ref _downBar, value); }
    }

    public class ChartTitleViewModel : ViewModelBase
    {
        private string _text;
        private FontViewModel _font;
        private Color _color;
        private bool _visible;
        private TextAlignment _alignment = TextAlignment.Center;
        private bool _overlay;
        private bool _includeInLayout = true;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public FontViewModel Font { get => _font; set => SetProperty(ref _font, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public TextAlignment Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
        public bool Overlay { get => _overlay; set => SetProperty(ref _overlay, value); }
        public bool IncludeInLayout { get => _includeInLayout; set => SetProperty(ref _includeInLayout, value); }
    }

    public class ChartLegendViewModel : ViewModelBase
    {
        private bool _visible = true;
        private LegendPosition _position = LegendPosition.Right;
        private FontViewModel _font;
        private ChartBorderViewModel _border;
        private ChartFillViewModel _fill;
        private bool _overlay;
        private HorizontalAlignment _alignment = HorizontalAlignment.Center;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public LegendPosition Position { get => _position; set => SetProperty(ref _position, value); }
        public FontViewModel Font { get => _font; set => SetProperty(ref _font, value); }
        public ChartBorderViewModel Border { get => _border; set => SetProperty(ref _border, value); }
        public ChartFillViewModel Fill { get => _fill; set => SetProperty(ref _fill, value); }
        public bool Overlay { get => _overlay; set => SetProperty(ref _overlay, value); }
        public HorizontalAlignment Alignment { get => _alignment; set => SetProperty(ref _alignment, value); }
    }

    public enum LegendPosition { Top, Bottom, Left, Right, TopRight, None }

    public class ChartPlotAreaViewModel : ViewModelBase
    {
        private ChartFillViewModel _fill;
        private ChartBorderViewModel _border;
        public ChartFillViewModel Fill { get => _fill; set => SetProperty(ref _fill, value); }
        public ChartBorderViewModel Border { get => _border; set => SetProperty(ref _border, value); }
    }

    public class ChartAxesViewModel : ViewModelBase
    {
        private ChartAxisViewModel _primaryCategoryAxis;
        private ChartAxisViewModel _primaryValueAxis;
        private ChartAxisViewModel _secondaryCategoryAxis;
        private ChartAxisViewModel _secondaryValueAxis;
        private ChartSeriesAxisViewModel _seriesAxis;

        public ChartAxisViewModel PrimaryCategoryAxis { get => _primaryCategoryAxis; set => SetProperty(ref _primaryCategoryAxis, value); }
        public ChartAxisViewModel PrimaryValueAxis { get => _primaryValueAxis; set => SetProperty(ref _primaryValueAxis, value); }
        public ChartAxisViewModel SecondaryCategoryAxis { get => _secondaryCategoryAxis; set => SetProperty(ref _secondaryCategoryAxis, value); }
        public ChartAxisViewModel SecondaryValueAxis { get => _secondaryValueAxis; set => SetProperty(ref _secondaryValueAxis, value); }
        public ChartSeriesAxisViewModel SeriesAxis { get => _seriesAxis; set => SetProperty(ref _seriesAxis, value); }
    }

    public class ChartAxisViewModel : ViewModelBase
    {
        private AxisType _axisType = AxisType.Category;
        private AxisPosition _position = AxisPosition.Bottom;
        private string _title;
        private FontViewModel _titleFont;
        private FontViewModel _tickLabelsFont;
        private Color _titleColor;
        private Color _axisColor;
        private double _majorUnit;
        private double _minorUnit;
        private double? _maximumValue;
        private double? _minimumValue;
        private bool _logScale;
        private bool _reverseValues;
        private bool _majorGridlines;
        private bool _minorGridlines;
        private bool _showLegendEntry;
        private AxisCrosses _crosses = AxisCrosses.Automatic;
        private double _crossingPoint;
        private bool _labelAutoGenerate;
        private string _numberFormat;
        private int _numberFormatLinked;
        private TickMarkType _majorTickMark = TickMarkType.Outside;
        private TickMarkType _minorTickMark = TickMarkType.None;
        private TickLabelType _tickLabelType = TickLabelType.Automatic;
        private int _tickLabelSpacing;
        private int _tickMarkSpacing;
        private bool _displayUnitLabel;
        private DisplayUnit _displayUnit = DisplayUnit.None;

        public AxisType AxisType { get => _axisType; set => SetProperty(ref _axisType, value); }
        public AxisPosition Position { get => _position; set => SetProperty(ref _position, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public FontViewModel TitleFont { get => _titleFont; set => SetProperty(ref _titleFont, value); }
        public FontViewModel TickLabelsFont { get => _tickLabelsFont; set => SetProperty(ref _tickLabelsFont, value); }
        public Color TitleColor { get => _titleColor; set => SetProperty(ref _titleColor, value); }
        public Color AxisColor { get => _axisColor; set => SetProperty(ref _axisColor, value); }
        public double MajorUnit { get => _majorUnit; set => SetProperty(ref _majorUnit, value); }
        public double MinorUnit { get => _minorUnit; set => SetProperty(ref _minorUnit, value); }
        public double? MaximumValue { get => _maximumValue; set => SetProperty(ref _maximumValue, value); }
        public double? MinimumValue { get => _minimumValue; set => SetProperty(ref _minimumValue, value); }
        public bool LogScale { get => _logScale; set => SetProperty(ref _logScale, value); }
        public bool ReverseValues { get => _reverseValues; set => SetProperty(ref _reverseValues, value); }
        public bool MajorGridlines { get => _majorGridlines; set => SetProperty(ref _majorGridlines, value); }
        public bool MinorGridlines { get => _minorGridlines; set => SetProperty(ref _minorGridlines, value); }
        public bool ShowLegendEntry { get => _showLegendEntry; set => SetProperty(ref _showLegendEntry, value); }
        public AxisCrosses Crosses { get => _crosses; set => SetProperty(ref _crosses, value); }
        public double CrossingPoint { get => _crossingPoint; set => SetProperty(ref _crossingPoint, value); }
        public bool LabelAutoGenerate { get => _labelAutoGenerate; set => SetProperty(ref _labelAutoGenerate, value); }
        public string NumberFormat { get => _numberFormat; set => SetProperty(ref _numberFormat, value); }
        public int NumberFormatLinked { get => _numberFormatLinked; set => SetProperty(ref _numberFormatLinked, value); }
        public TickMarkType MajorTickMark { get => _majorTickMark; set => SetProperty(ref _majorTickMark, value); }
        public TickMarkType MinorTickMark { get => _minorTickMark; set => SetProperty(ref _minorTickMark, value); }
        public TickLabelType TickLabelType { get => _tickLabelType; set => SetProperty(ref _tickLabelType, value); }
        public int TickLabelSpacing { get => _tickLabelSpacing; set => SetProperty(ref _tickLabelSpacing, value); }
        public int TickMarkSpacing { get => _tickMarkSpacing; set => SetProperty(ref _tickMarkSpacing, value); }
        public bool DisplayUnitLabel { get => _displayUnitLabel; set => SetProperty(ref _displayUnitLabel, value); }
        public DisplayUnit DisplayUnit { get => _displayUnit; set => SetProperty(ref _displayUnit, value); }
    }

    public enum AxisType { Category, Value, Series }
    public enum AxisPosition { Top, Bottom, Left, Right, Automatic }
    public enum AxisCrosses { Automatic, Maximum, Minimum }
    public enum TickMarkType { None, Outside, Inside, Cross }
    public enum TickLabelType { Automatic, None, Skip }
    public enum DisplayUnit { None, Hundreds, Thousands, TenThousands, HundredThousands, Millions, TenMillions, HundredMillions, Billions, Trillions }

    public class ChartSeriesAxisViewModel : ViewModelBase
    {
        private bool _visible = true;
        private int _crossingPoint;
        private AxisCrosses _crosses = AxisCrosses.Automatic;

        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
        public int CrossingPoint { get => _crossingPoint; set => SetProperty(ref _crossingPoint, value); }
        public AxisCrosses Crosses { get => _crosses; set => SetProperty(ref _crosses, value); }
    }

    public class ChartAreaViewModel : ViewModelBase
    {
        private ChartFillViewModel _fill;
        private ChartBorderViewModel _border;
        private FontViewModel _font;
        private double _width;
        private double _height;
        private double _left;
        private double _top;
        private double _rotation;
        private double _heightPercent;
        private bool _autoSize;
        private bool _noMoveable;
        private ChartShadowViewModel _shadow;

        public ChartFillViewModel Fill { get => _fill; set => SetProperty(ref _fill, value); }
        public ChartBorderViewModel Border { get => _border; set => SetProperty(ref _border, value); }
        public FontViewModel Font { get => _font; set => SetProperty(ref _font, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public double Left { get => _left; set => SetProperty(ref _left, value); }
        public double Top { get => _top; set => SetProperty(ref _top, value); }
        public double Rotation { get => _rotation; set => SetProperty(ref _rotation, value); }
        public double HeightPercent { get => _heightPercent; set => SetProperty(ref _heightPercent, value); }
        public bool AutoSize { get => _autoSize; set => SetProperty(ref _autoSize, value); }
        public bool NoMoveable { get => _noMoveable; set => SetProperty(ref _noMoveable, value); }
        public ChartShadowViewModel Shadow { get => _shadow; set => SetProperty(ref _shadow, value); }
    }

    public class ChartFillViewModel
    {
        private FillType _type = FillType.None;
        private Color _color;
        private PatternFillViewModel _pattern;
        private GradientFillViewModel _gradient;
        private bool _visible = true;

        public FillType Type { get => _type; set => _type = value; }
        public Color Color { get => _color; set => _color = value; }
        public PatternFillViewModel Pattern { get => _pattern; set => _pattern = value; }
        public GradientFillViewModel Gradient { get => _gradient; set => _gradient = value; }
        public bool Visible { get => _visible; set => _visible = value; }
    }

    public class ChartBorderViewModel
    {
        private Color _color = Color.FromHex("#000000");
        private double _width = 0.75;

        public Color Color { get => _color; set => _color = value; }
        public double Width { get => _width; set => _width = value; }
    }

    public class ChartShadowViewModel
    {
        private bool _visible;
        private Color _color;
        private double _blur;
        private double _offsetX;
        private double _offsetY;
        private ShadowType _type = ShadowType.Outer;
        private double _angle;
        private double _distance;

        public bool Visible { get => _visible; set => _visible = value; }
        public Color Color { get => _color; set => _color = value; }
        public double Blur { get => _blur; set => _blur = value; }
        public double OffsetX { get => _offsetX; set => _offsetX = value; }
        public double OffsetY { get => _offsetY; set => _offsetY = value; }
        public ShadowType Type { get => _type; set => _type = value; }
        public double Angle { get => _angle; set => _angle = value; }
        public double Distance { get => _distance; set => _distance = value; }
    }

    public enum ShadowType { None, Outer, Inner }

    public class FloorViewModel
    {
        private ChartFillViewModel _fill;
        private ChartBorderViewModel _border;
        private bool _visible = true;

        public ChartFillViewModel Fill { get => _fill; set => _fill = value; }
        public ChartBorderViewModel Border { get => _border; set => _border = value; }
        public bool Visible { get => _visible; set => _visible = value; }
    }

    public class SideWallViewModel
    {
        private ChartFillViewModel _fill;
        private ChartBorderViewModel _border;

        public ChartFillViewModel Fill { get => _fill; set => _fill = value; }
        public ChartBorderViewModel Border { get => _border; set => _border = value; }
    }

    public class BackWallViewModel
    {
        private ChartFillViewModel _fill;
        private ChartBorderViewModel _border;

        public ChartFillViewModel Fill { get => _fill; set => _fill = value; }
        public ChartBorderViewModel Border { get => _border; set => _border = value; }
    }

    public class ChartLineViewModel
    {
        private Color _color = Color.FromHex("#000000");
        private double _width = 0.75;
        private LineDashStyle _dashStyle = LineDashStyle.Solid;
        private LineStyle _style = LineStyle.Single;
        private bool _smooth;
        private ChartMarkerViewModel _marker;

        public Color Color { get => _color; set => _color = value; }
        public double Width { get => _width; set => _width = value; }
        public LineDashStyle DashStyle { get => _dashStyle; set => _dashStyle = value; }
        public LineStyle Style { get => _style; set => _style = value; }
        public bool Smooth { get => _smooth; set => _smooth = value; }
        public ChartMarkerViewModel Marker { get => _marker; set => _marker = value; }
    }

    public enum LineDashStyle { Solid, Dash, DashDot, DashDotDot, Dot, RoundDot }
    public enum LineStyle { Single, Thick, Thin, Double }

    public class ChartMarkerViewModel
    {
        private MarkerType _type = MarkerType.None;
        private double _size = 5;
        private Color _foregroundColor;
        private Color _backgroundColor;
        private ChartBorderViewModel _border;

        public MarkerType Type { get => _type; set => _type = value; }
        public double Size { get => _size; set => _size = value; }
        public Color ForegroundColor { get => _foregroundColor; set => _foregroundColor = value; }
        public Color BackgroundColor { get => _backgroundColor; set => _backgroundColor = value; }
        public ChartBorderViewModel Border { get => _border; set => _border = value; }
    }

    public enum MarkerType { None, Circle, Dash, Diamond, Dot, Picture, Square, Star, Triangle, X }

    public class ChartDataLabelViewModel
    {
        private bool _visible;
        private string _text;
        private FontViewModel _font;
        private Color _color;
        private TextAlignment _alignment = TextAlignment.Center;
        private Position _position = Position.Right;
        private string _numberFormat;
        private bool _showCategoryName;
        private bool _showSeriesName;
        private bool _showValue;
        private bool _showPercentage;
        private bool _showBubbleSize;
        private bool _showLabelAndCallout;
        private Color _fillColor;
        private ChartBorderViewModel _border;

        public bool Visible { get => _visible; set => _visible = value; }
        public string Text { get => _text; set => _text = value; }
        public FontViewModel Font { get => _font; set => _font = value; }
        public Color Color { get => _color; set => _color = value; }
        public TextAlignment Alignment { get => _alignment; set => _alignment = value; }
        public Position Position { get => _position; set => _position = value; }
        public string NumberFormat { get => _numberFormat; set => _numberFormat = value; }
        public bool ShowCategoryName { get => _showCategoryName; set => _showCategoryName = value; }
        public bool ShowSeriesName { get => _showSeriesName; set => _showSeriesName = value; }
        public bool ShowValue { get => _showValue; set => _showValue = value; }
        public bool ShowPercentage { get => _showPercentage; set => _showPercentage = value; }
        public bool ShowBubbleSize { get => _showBubbleSize; set => _showBubbleSize = value; }
        public bool ShowLabelAndCallout { get => _showLabelAndCallout; set => _showLabelAndCallout = value; }
        public Color FillColor { get => _fillColor; set => _fillColor = value; }
        public ChartBorderViewModel Border { get => _border; set => _border = value; }
    }

    public enum Position { Top, Bottom, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight, Center }

    public class TrendlineViewModelCollection : List<TrendlineViewModel> { }

    public class TrendlineViewModel : ViewModelBase
    {
        private string _name;
        private TrendlineType _type = TrendlineType.Linear;
        private bool _displayEquation;
        private bool _displayRSquared;
        private string _equationFormat;
        private double? _intercept;
        private Color _lineColor;
        private double _lineWidth;
        private LineDashStyle _lineDashStyle;
        private int _forwardPeriods;
        private int _backwardPeriods;
        private int _order;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public TrendlineType Type { get => _type; set => SetProperty(ref _type, value); }
        public bool DisplayEquation { get => _displayEquation; set => SetProperty(ref _displayEquation, value); }
        public bool DisplayRSquared { get => _displayRSquared; set => SetProperty(ref _displayRSquared, value); }
        public string EquationFormat { get => _equationFormat; set => SetProperty(ref _equationFormat, value); }
        public double? Intercept { get => _intercept; set => SetProperty(ref _intercept, value); }
        public Color LineColor { get => _lineColor; set => SetProperty(ref _lineColor, value); }
        public double LineWidth { get => _lineWidth; set => SetProperty(ref _lineWidth, value); }
        public LineDashStyle LineDashStyle { get => _lineDashStyle; set => SetProperty(ref _lineDashStyle, value); }
        public int ForwardPeriods { get => _forwardPeriods; set => SetProperty(ref _forwardPeriods, value); }
        public int BackwardPeriods { get => _backwardPeriods; set => SetProperty(ref _backwardPeriods, value); }
        public int Order { get => _order; set => SetProperty(ref _order, value); }
    }

    public enum TrendlineType { Linear, Logarithmic, Exponential, Power, MovingAverage, Polynomial }

    public class ErrorBarViewModel
    {
        private ErrorBarType _type = ErrorBarType.Fixed;
        private double _amount = 1;
        private double _percentage = 10;
        private double _standardDeviation = 1;
        private ErrorBarDirection _direction = ErrorBarDirection.Both;
        private ErrorBarEndStyle _endStyle = ErrorBarEndStyle.NoCap;
        private Color _color;
        private double _lineWidth;

        public ErrorBarType Type { get => _type; set => _type = value; }
        public double Amount { get => _amount; set => _amount = value; }
        public double Percentage { get => _percentage; set => _percentage = value; }
        public double StandardDeviation { get => _standardDeviation; set => _standardDeviation = value; }
        public ErrorBarDirection Direction { get => _direction; set => _direction = value; }
        public ErrorBarEndStyle EndStyle { get => _endStyle; set => _endStyle = value; }
        public Color Color { get => _color; set => _color = value; }
        public double LineWidth { get => _lineWidth; set => _lineWidth = value; }
    }

    public enum ErrorBarType { Fixed, Percentage, StandardDeviation, Custom }
    public enum ErrorBarDirection { Both, Plus, Minus }
    public enum ErrorBarEndStyle { Cap, NoCap }

    public class UpBarViewModel
    {
        private Color _fillColor;
        private Color _borderColor;
        private double _borderWidth;

        public Color FillColor { get => _fillColor; set => _fillColor = value; }
        public Color BorderColor { get => _borderColor; set => _borderColor = value; }
        public double BorderWidth { get => _borderWidth; set => _borderWidth = value; }
    }

    public class DownBarViewModel
    {
        private Color _fillColor;
        private Color _borderColor;
        private double _borderWidth;

        public Color FillColor { get => _fillColor; set => _fillColor = value; }
        public Color BorderColor { get => _borderColor; set => _borderColor = value; }
        public double BorderWidth { get => _borderWidth; set => _borderWidth = value; }
    }

    public class DefinedNameViewModel : ViewModelBase
    {
        private string _name;
        private string _value;
        private string _refersTo;
        private string _comment;
        private ScopeType _scope = ScopeType.Workbook;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
        public string RefersTo { get => _refersTo; set => SetProperty(ref _refersTo, value); }
        public string Comment { get => _comment; set => SetProperty(ref _comment, value); }
        public ScopeType Scope { get => _scope; set => SetProperty(ref _scope, value); }
    }

    public enum ScopeType { Workbook, Worksheet }

    public class WorkbookPropertiesViewModel : ViewModelBase
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

    public class DataConnectionViewModel : ViewModelBase
    {
        private string _name;
        private ConnectionType _type;
        private string _connectionString;
        private string _commandText;
        private CommandType _commandType = CommandType.Table;
        private RefreshPolicy _refreshPolicy = RefreshPolicy.Always;
        private int _refreshPeriod;
        private bool _savePassword;
        private bool _connectUsingWin;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ConnectionType Type { get => _type; set => SetProperty(ref _type, value); }
        public string ConnectionString { get => _connectionString; set => SetProperty(ref _connectionString, value); }
        public string CommandText { get => _commandText; set => SetProperty(ref _commandText, value); }
        public CommandType CommandType { get => _commandType; set => SetProperty(ref _commandType, value); }
        public RefreshPolicy RefreshPolicy { get => _refreshPolicy; set => SetProperty(ref _refreshPolicy, value); }
        public int RefreshPeriod { get => _refreshPeriod; set => SetProperty(ref _refreshPeriod, value); }
        public bool SavePassword { get => _savePassword; set => SetProperty(ref _savePassword, value); }
        public bool ConnectUsingWin { get => _connectUsingWin; set => SetProperty(ref _connectUsingWin, value); }
    }

    public enum ConnectionType { File, Database, Web, Olap, Odbc, Oledb }
    public enum CommandType { Table, Sql, Default }
    public enum RefreshPolicy { Always, Never }

    public class ValidationViewModel : ViewModelBase
    {
        private ValidationType _type = ValidationType.Any;
        private CellRange _range;
        private string _formula1;
        private string _formula2;
        private bool _ignoreBlank = true;
        private bool _showInputMessage = true;
        private string _inputTitle;
        private string _inputMessage;
        private bool _showErrorMessage = true;
        private string _errorTitle;
        private string _errorMessage;
        private ValidationStyle _errorStyle = ValidationStyle.Stop;

        public ValidationType Type { get => _type; set => SetProperty(ref _type, value); }
        public CellRange Range { get => _range; set => SetProperty(ref _range, value); }
        public string Formula1 { get => _formula1; set => SetProperty(ref _formula1, value); }
        public string Formula2 { get => _formula2; set => SetProperty(ref _formula2, value); }
        public bool IgnoreBlank { get => _ignoreBlank; set => SetProperty(ref _ignoreBlank, value); }
        public bool ShowInputMessage { get => _showInputMessage; set => SetProperty(ref _showInputMessage, value); }
        public string InputTitle { get => _inputTitle; set => SetProperty(ref _inputTitle, value); }
        public string InputMessage { get => _inputMessage; set => SetProperty(ref _inputMessage, value); }
        public bool ShowErrorMessage { get => _showErrorMessage; set => SetProperty(ref _showErrorMessage, value); }
        public string ErrorTitle { get => _errorTitle; set => SetProperty(ref _errorTitle, value); }
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
        public ValidationStyle ErrorStyle { get => _errorStyle; set => SetProperty(ref _errorStyle, value); }
    }

    public enum ValidationType { Any, WholeNumber, Decimal, List, Date, Time, TextLength, Custom }
    public enum ValidationStyle { Stop, Warning, Information }

    public class CommentViewModel : ViewModelBase
    {
        private string _id;
        private string _author;
        private string _initials;
        private DateTime _date;
        private string _text;
        private Color _backgroundColor;
        private bool _visible;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Author { get => _author; set => SetProperty(ref _author, value); }
        public string Initials { get => _initials; set => SetProperty(ref _initials, value); }
        public DateTime Date { get => _date; set => SetProperty(ref _date, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public Color BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }
        public bool Visible { get => _visible; set => SetProperty(ref _visible, value); }
    }

    public class RowPropertiesViewModel : ViewModelBase
    {
        private double _height = 15;
        private bool _hidden;
        private bool _filtered;
        private OutlineLevel _outlineLevel = OutlineLevel.None;
        private bool _collapsed;
        private bool _phonetic;

        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
        public bool Filtered { get => _filtered; set => SetProperty(ref _filtered, value); }
        public OutlineLevel OutlineLevel { get => _outlineLevel; set => SetProperty(ref _outlineLevel, value); }
        public bool Collapsed { get => _collapsed; set => SetProperty(ref _collapsed, value); }
        public bool Phonetic { get => _phonetic; set => SetProperty(ref _phonetic, value); }
    }

    public class ColumnPropertiesViewModel : ViewModelBase
    {
        private double _width = 8.43;
        private bool _hidden;
        private string _styleId;
        private OutlineLevel _outlineLevel = OutlineLevel.None;
        private bool _collapsed;
        private bool _phonetic;

        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }
        public string StyleId { get => _styleId; set => SetProperty(ref _styleId, value); }
        public OutlineLevel OutlineLevel { get => _outlineLevel; set => SetProperty(ref _outlineLevel, value); }
        public bool Collapsed { get => _collapsed; set => SetProperty(ref _collapsed, value); }
        public bool Phonetic { get => _phonetic; set => SetProperty(ref _phonetic, value); }
    }

    public class PageBreakCollection : List<PageBreakViewModel> { }

    public class PageBreakViewModel : ViewModelBase
    {
        private int _row;
        private int _column;
        private PageBreakType _type = PageBreakType.Manual;
        private bool _isAutomatic;

        public int Row { get => _row; set => SetProperty(ref _row, value); }
        public int Column { get => _column; set => SetProperty(ref _column, value); }
        public PageBreakType Type { get => _type; set => SetProperty(ref _type, value); }
        public bool IsAutomatic { get => _isAutomatic; set => SetProperty(ref _isAutomatic, value); }
    }

    public enum PageBreakType { None, Manual, Automatic }

    public class TabColor
    {
        public Color Color { get; set; }
        public bool IsAutomatic { get; set; }

        public static TabColor Automatic => new TabColor { IsAutomatic = true };
        public static TabColor FromColor(Color color) => new TabColor { Color = color };
    }

    public class GridlineColor
    {
        public Color Color { get; set; }
        public bool IsAutomatic { get; set; }

        public static GridlineColor Automatic => new GridlineColor { IsAutomatic = true };
        public static GridlineColor FromColor(Color color) => new GridlineColor { Color = color };
    }

    public class ZoomViewModel : ViewModelBase
    {
        private double _scale = 100;
        private bool _fitToPage;
        private bool _fitToWidth;
        private bool _fitToHeight;

        public double Scale { get => _scale; set => SetProperty(ref _scale, value); }
        public bool FitToPage { get => _fitToPage; set => SetProperty(ref _fitToPage, value); }
        public bool FitToWidth { get => _fitToWidth; set => SetProperty(ref _fitToWidth, value); }
        public bool FitToHeight { get => _fitToHeight; set => SetProperty(ref _fitToHeight, value); }
    }

    public class HeaderFooterViewModel : ViewModelBase
    {
        private string _headerLeft;
        private string _headerCenter;
        private string _headerRight;
        private string _footerLeft;
        private string _footerCenter;
        private string _footerRight;
        private bool _differentFirstPage;
        private bool _DifferentOddAndEvenPages;
        private FontViewModel _headerFont;
        private FontViewModel _footerFont;

        public string HeaderLeft { get => _headerLeft; set => SetProperty(ref _headerLeft, value); }
        public string HeaderCenter { get => _headerCenter; set => SetProperty(ref _headerCenter, value); }
        public string HeaderRight { get => _headerRight; set => SetProperty(ref _headerRight, value); }
        public string FooterLeft { get => _footerLeft; set => SetProperty(ref _footerLeft, value); }
        public string FooterCenter { get => _footerCenter; set => SetProperty(ref _footerCenter, value); }
        public string FooterRight { get => _footerRight; set => SetProperty(ref _footerRight, value); }
        public bool DifferentFirstPage { get => _differentFirstPage; set => SetProperty(ref _differentFirstPage, value); }
        public bool DifferentOddAndEvenPages { get => _DifferentOddAndEvenPages; set => SetProperty(ref _DifferentOddAndEvenPages, value); }
        public FontViewModel HeaderFont { get => _headerFont; set => SetProperty(ref _headerFont, value); }
        public FontViewModel FooterFont { get => _footerFont; set => SetProperty(ref _footerFont, value); }
    }

    public enum ViewMode { Normal, PageLayout, PageBreakPreview }
}

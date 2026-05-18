using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using OfficeSuite.Spreadsheet.Models;
using OfficeSuite.Spreadsheet.Analysis;

namespace OfficeSuite.Spreadsheet.ViewModels
{
    public class SpreadsheetViewModel : INotifyPropertyChanged
    {
        private string _title = "Untitled Workbook";
        private string _selectedSheetName = "Sheet1";
        private CellViewModel? _selectedCell;
        private string _selectedRange = "A1";
        private bool _isModified;
        private double _zoomLevel = 100;
        private int _rowCount = 1048576;
        private int _columnCount = 16384;
        private bool _showGridLines = true;
        private bool _showFormulaBar = true;
        private bool _showStatusBar = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public WorkbookViewModel Workbook { get; }
        public FormulaBarViewModel FormulaBar { get; }
        public CellViewModel SelectedCell
        {
            get => _selectedCell ?? new CellViewModel();
            set { _selectedCell = value; OnPropertyChanged(nameof(SelectedCell)); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string SelectedSheetName
        {
            get => _selectedSheetName;
            set { _selectedSheetName = value; OnPropertyChanged(nameof(SelectedSheetName)); }
        }

        public string SelectedRange
        {
            get => _selectedRange;
            set { _selectedRange = value; OnPropertyChanged(nameof(SelectedRange)); }
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

        public int RowCount
        {
            get => _rowCount;
            set { _rowCount = value; OnPropertyChanged(nameof(RowCount)); }
        }

        public int ColumnCount
        {
            get => _columnCount;
            set { _columnCount = value; OnPropertyChanged(nameof(ColumnCount)); }
        }

        public bool ShowGridLines
        {
            get => _showGridLines;
            set { _showGridLines = value; OnPropertyChanged(nameof(ShowGridLines)); }
        }

        public bool ShowFormulaBar
        {
            get => _showFormulaBar;
            set { _showFormulaBar = value; OnPropertyChanged(nameof(ShowFormulaBar)); }
        }

        public bool ShowStatusBar
        {
            get => _showStatusBar;
            set { _showStatusBar = value; OnPropertyChanged(nameof(ShowStatusBar)); }
        }

        public ObservableCollection<SheetViewModel> Sheets { get; } = new();
        public ObservableCollection<string> RecentWorkbooks { get; } = new();
        public ObservableCollection<ChartViewModel> Charts { get; } = new();

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
        public ICommand InsertChartCommand { get; }
        public ICommand InsertTableCommand { get; }

        public SpreadsheetViewModel()
        {
            Workbook = new WorkbookViewModel();
            FormulaBar = new FormulaBarViewModel();

            var defaultSheet = new SheetViewModel { Name = "Sheet1" };
            Sheets.Add(defaultSheet);
            SelectedSheetName = defaultSheet.Name;

            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand<string>(ExecuteOpen);
            SaveCommand = new RelayCommand<string>(ExecuteSave);
            SaveAsCommand = new RelayCommand<string>(ExecuteSaveAs);
            PrintCommand = new RelayCommand(ExecutePrint);
            UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
            CutCommand = new RelayCommand(ExecuteCut, CanExecuteCut);
            CopyCommand = new RelayCommand(ExecuteCopy, CanExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste, CanExecutePaste);
            InsertChartCommand = new RelayCommand<string>(ExecuteInsertChart);
            InsertTableCommand = new RelayCommand<string>(ExecuteInsertTable);
        }

        private void ExecuteNew()
        {
            Title = "Untitled Workbook";
            Workbook.Reset();
            Sheets.Clear();
            Sheets.Add(new SheetViewModel { Name = "Sheet1" });
            SelectedSheetName = "Sheet1";
            IsModified = false;
        }

        private void ExecuteOpen(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Title = System.IO.Path.GetFileName(path);
                RecentWorkbooks.Insert(0, path);
                if (RecentWorkbooks.Count > 10)
                    RecentWorkbooks.RemoveAt(10);
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
        private void ExecuteCut() { }
        private bool CanExecuteCut() => true;
        private void ExecuteCopy() { }
        private bool CanExecuteCopy() => true;
        private void ExecutePaste() { }
        private void ExecuteInsertChart(string? chartType) { }
        private void ExecuteInsertTable(string? tableName) { }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WorkbookViewModel : INotifyPropertyChanged
    {
        private string _filePath = string.Empty;
        private string _name = "Workbook";
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;
        private string _author = string.Empty;
        private string _company = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public void Reset()
        {
            FilePath = string.Empty;
            Name = "Workbook";
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            Author = string.Empty;
            Company = string.Empty;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SheetViewModel : INotifyPropertyChanged
    {
        private string _name = "Sheet";
        private bool _isVisible = true;
        private bool _isSelected;
        private int _tabColor;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public int TabColor
        {
            get => _tabColor;
            set { _tabColor = value; OnPropertyChanged(nameof(TabColor)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CellViewModel : INotifyPropertyChanged
    {
        private string _address = "A1";
        private string _value = string.Empty;
        private string _formula = string.Empty;
        private CellType _cellType = CellType.General;
        private string _format = string.Empty;
        private bool _isSelected;
        private bool _isEdited;
        private string _fontFamily = "Calibri";
        private double _fontSize = 11;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderline;
        private string _textColor = "#000000";
        private string _backgroundColor = "#FFFFFF";
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
        private BorderStyle _borderTop = BorderStyle.None;
        private BorderStyle _borderBottom = BorderStyle.None;
        private BorderStyle _borderLeft = BorderStyle.None;
        private BorderStyle _borderRight = BorderStyle.None;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }

        public string Formula
        {
            get => _formula;
            set { _formula = value; OnPropertyChanged(nameof(Formula)); }
        }

        public CellType CellType
        {
            get => _cellType;
            set { _cellType = value; OnPropertyChanged(nameof(CellType)); }
        }

        public string Format
        {
            get => _format;
            set { _format = value; OnPropertyChanged(nameof(Format)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsEdited
        {
            get => _isEdited;
            set { _isEdited = value; OnPropertyChanged(nameof(IsEdited)); }
        }

        public string FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; OnPropertyChanged(nameof(FontFamily)); }
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

        public string TextColor
        {
            get => _textColor;
            set { _textColor = value; OnPropertyChanged(nameof(TextColor)); }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set { _horizontalAlignment = value; OnPropertyChanged(nameof(HorizontalAlignment)); }
        }

        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set { _verticalAlignment = value; OnPropertyChanged(nameof(VerticalAlignment)); }
        }

        public BorderStyle BorderTop
        {
            get => _borderTop;
            set { _borderTop = value; OnPropertyChanged(nameof(BorderTop)); }
        }

        public BorderStyle BorderBottom
        {
            get => _borderBottom;
            set { _borderBottom = value; OnPropertyChanged(nameof(BorderBottom)); }
        }

        public BorderStyle BorderLeft
        {
            get => _borderLeft;
            set { _borderLeft = value; OnPropertyChanged(nameof(BorderLeft)); }
        }

        public BorderStyle BorderRight
        {
            get => _borderRight;
            set { _borderRight = value; OnPropertyChanged(nameof(BorderRight)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FormulaBarViewModel : INotifyPropertyChanged
    {
        private string _formula = string.Empty;
        private string _selectedRange = string.Empty;
        private bool _isEditing;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Formula
        {
            get => _formula;
            set { _formula = value; OnPropertyChanged(nameof(Formula)); }
        }

        public string SelectedRange
        {
            get => _selectedRange;
            set { _selectedRange = value; OnPropertyChanged(nameof(SelectedRange)); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartViewModel : INotifyPropertyChanged
    {
        private string _name = "Chart";
        private ChartType _chartType = ChartType.Column;
        private string _title = string.Empty;
        private bool _showLegend = true;
        private bool _showTitle;
        private bool _showGridLines = true;
        private bool _showDataLabels;
        private string _dataSourceRange = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public ChartType ChartType
        {
            get => _chartType;
            set { _chartType = value; OnPropertyChanged(nameof(ChartType)); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set { _showLegend = value; OnPropertyChanged(nameof(ShowLegend)); }
        }

        public bool ShowTitle
        {
            get => _showTitle;
            set { _showTitle = value; OnPropertyChanged(nameof(ShowTitle)); }
        }

        public bool ShowGridLines
        {
            get => _showGridLines;
            set { _showGridLines = value; OnPropertyChanged(nameof(ShowGridLines)); }
        }

        public bool ShowDataLabels
        {
            get => _showDataLabels;
            set { _showDataLabels = value; OnPropertyChanged(nameof(ShowDataLabels)); }
        }

        public string DataSourceRange
        {
            get => _dataSourceRange;
            set { _dataSourceRange = value; OnPropertyChanged(nameof(DataSourceRange)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PivotTableViewModel : INotifyPropertyChanged
    {
        private string _name = "PivotTable";
        private string _dataSourceRange = string.Empty;
        private string _targetRange = string.Empty;
        private string _dataSheet = string.Empty;
        private PivotTableStyle _style = PivotTableStyle.Medium1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<PivotFieldViewModel> RowFields { get; } = new();
        public ObservableCollection<PivotFieldViewModel> ColumnFields { get; } = new();
        public ObservableCollection<PivotFieldViewModel> DataFields { get; } = new();
        public ObservableCollection<PivotFieldViewModel> FilterFields { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string DataSourceRange
        {
            get => _dataSourceRange;
            set { _dataSourceRange = value; OnPropertyChanged(nameof(DataSourceRange)); }
        }

        public string TargetRange
        {
            get => _targetRange;
            set { _targetRange = value; OnPropertyChanged(nameof(TargetRange)); }
        }

        public string DataSheet
        {
            get => _dataSheet;
            set { _dataSheet = value; OnPropertyChanged(nameof(DataSheet)); }
        }

        public PivotTableStyle Style
        {
            get => _style;
            set { _style = value; OnPropertyChanged(nameof(Style)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PivotFieldViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private PivotFieldOrientation _orientation = PivotFieldOrientation.RowField;
        private string _customName = string.Empty;
        private bool _showSubtotals = true;
        private bool _showInOutlineForm = true;
        private int _compactRowIndent;
        private string _subtotalFunction = "SUM";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public PivotFieldOrientation Orientation
        {
            get => _orientation;
            set { _orientation = value; OnPropertyChanged(nameof(Orientation)); }
        }

        public string CustomName
        {
            get => _customName;
            set { _customName = value; OnPropertyChanged(nameof(CustomName)); }
        }

        public bool ShowSubtotals
        {
            get => _showSubtotals;
            set { _showSubtotals = value; OnPropertyChanged(nameof(ShowSubtotals)); }
        }

        public bool ShowInOutlineForm
        {
            get => _showInOutlineForm;
            set { _showInOutlineForm = value; OnPropertyChanged(nameof(ShowInOutlineForm)); }
        }

        public int CompactRowIndent
        {
            get => _compactRowIndent;
            set { _compactRowIndent = value; OnPropertyChanged(nameof(CompactRowIndent)); }
        }

        public string SubtotalFunction
        {
            get => _subtotalFunction;
            set { _subtotalFunction = value; OnPropertyChanged(nameof(SubtotalFunction)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RangeViewModel : INotifyPropertyChanged
    {
        private string _address = string.Empty;
        private int _startRow;
        private int _startColumn;
        private int _endRow;
        private int _endColumn;
        private string _selectedRange = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        public int StartRow
        {
            get => _startRow;
            set { _startRow = value; OnPropertyChanged(nameof(StartRow)); }
        }

        public int StartColumn
        {
            get => _startColumn;
            set { _startColumn = value; OnPropertyChanged(nameof(StartColumn)); }
        }

        public int EndRow
        {
            get => _endRow;
            set { _endRow = value; OnPropertyChanged(nameof(EndRow)); }
        }

        public int EndColumn
        {
            get => _endColumn;
            set { _endColumn = value; OnPropertyChanged(nameof(EndColumn)); }
        }

        public string SelectedRange
        {
            get => _selectedRange;
            set { _selectedRange = value; OnPropertyChanged(nameof(SelectedRange)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum BorderStyle
    {
        None,
        Thin,
        Medium,
        Thick,
        Double,
        Hairline
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
        General
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public enum ChartType
    {
        Column,
        Bar,
        Line,
        Pie,
        Area,
        Scatter,
        Stock,
        Surface,
        Doughnut,
        Radar
    }

    public enum PivotTableStyle
    {
        None,
        Medium1,
        Medium2,
        Medium3,
        Medium4,
        Medium5,
        Medium6,
        Medium7,
        Medium8,
        Medium9,
        Medium10,
        Medium11,
        Light1,
        Light2,
        Light3,
        Light4,
        Light5,
        Light6,
        Light7,
        Light8,
        Dark1,
        Dark2,
        Dark3,
        Dark4,
        Dark5,
        Dark6,
        Dark7,
        Dark8,
        Dark9,
        Dark10,
        Dark11
    }

    public enum PivotFieldOrientation
    {
        RowField,
        ColumnField,
        DataField,
        PageField,
        Hidden
    }

    public enum CellType
    {
        General,
        Number,
        Currency,
        Accounting,
        Date,
        Time,
        Percentage,
        Fraction,
        Scientific,
        Text,
        Special,
        Custom
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

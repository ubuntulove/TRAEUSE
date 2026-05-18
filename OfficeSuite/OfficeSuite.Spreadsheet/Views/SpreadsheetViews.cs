using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace OfficeSuite.Spreadsheet.Views
{
    public class SpreadsheetGridView : INotifyPropertyChanged
    {
        private double _rowHeight = 20;
        private double _columnWidth = 80;
        private int _frozenRows;
        private int _frozenColumns;
        private bool _showGridLines = true;
        private bool _showRowHeaders = true;
        private bool _showColumnHeaders = true;
        private SelectionMode _selectionMode = SelectionMode.Extended;
        private CellView? _selectedCell;
        private RangeView? _selectedRange;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<CellSelectedEventArgs>? CellSelected;
        public event EventHandler<RangeSelectedEventArgs>? RangeSelected;
        public event EventHandler<CellEditedEventArgs>? CellEdited;

        public double RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = value; OnPropertyChanged(nameof(RowHeight)); }
        }

        public double ColumnWidth
        {
            get => _columnWidth;
            set { _columnWidth = value; OnPropertyChanged(nameof(ColumnWidth)); }
        }

        public int FrozenRows
        {
            get => _frozenRows;
            set { _frozenRows = value; OnPropertyChanged(nameof(FrozenRows)); }
        }

        public int FrozenColumns
        {
            get => _frozenColumns;
            set { _frozenColumns = value; OnPropertyChanged(nameof(FrozenColumns)); }
        }

        public bool ShowGridLines
        {
            get => _showGridLines;
            set { _showGridLines = value; OnPropertyChanged(nameof(ShowGridLines)); }
        }

        public bool ShowRowHeaders
        {
            get => _showRowHeaders;
            set { _showRowHeaders = value; OnPropertyChanged(nameof(ShowRowHeaders)); }
        }

        public bool ShowColumnHeaders
        {
            get => _showColumnHeaders;
            set { _showColumnHeaders = value; OnPropertyChanged(nameof(ShowColumnHeaders)); }
        }

        public SelectionMode SelectionMode
        {
            get => _selectionMode;
            set { _selectionMode = value; OnPropertyChanged(nameof(SelectionMode)); }
        }

        public CellView? SelectedCell
        {
            get => _selectedCell;
            set
            {
                _selectedCell = value;
                OnPropertyChanged(nameof(SelectedCell));
                CellSelected?.Invoke(this, new CellSelectedEventArgs { Cell = value });
            }
        }

        public RangeView? SelectedRange
        {
            get => _selectedRange;
            set
            {
                _selectedRange = value;
                OnPropertyChanged(nameof(SelectedRange));
                RangeSelected?.Invoke(this, new RangeSelectedEventArgs { Range = value });
            }
        }

        public ObservableCollection<RowView> Rows { get; } = new();
        public ObservableCollection<ColumnView> Columns { get; } = new();
        public ObservableCollection<CellView> Cells { get; } = new();

        public void AddRow(int index)
        {
            var row = new RowView { Index = index, Height = RowHeight };
            Rows.Insert(index, row);
        }

        public void RemoveRow(int index)
        {
            if (index >= 0 && index < Rows.Count)
            {
                Rows.RemoveAt(index);
                for (int i = index; i < Rows.Count; i++)
                {
                    Rows[i].Index = i;
                }
            }
        }

        public void AddColumn(int index, string header)
        {
            var column = new ColumnView { Index = index, Header = header, Width = ColumnWidth };
            Columns.Insert(index, column);
        }

        public void RemoveColumn(int index)
        {
            if (index >= 0 && index < Columns.Count)
            {
                Columns.RemoveAt(index);
                for (int i = index; i < Columns.Count; i++)
                {
                    Columns[i].Index = i;
                }
            }
        }

        public CellView GetCell(int row, int column)
        {
            var address = $"{GetColumnLetter(column)}{row + 1}";
            return Cells.FirstOrDefault(c => c.Address == address) ?? new CellView { Address = address };
        }

        public void SetCellValue(int row, int column, object value)
        {
            var cell = GetCell(row, column);
            cell.Value = value?.ToString() ?? string.Empty;
            CellEdited?.Invoke(this, new CellEditedEventArgs { Cell = cell, NewValue = value });
        }

        private string GetColumnLetter(int column)
        {
            string result = string.Empty;
            while (column >= 0)
            {
                result = (char)('A' + column % 26) + result;
                column = column / 26 - 1;
            }
            return result;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CellView : INotifyPropertyChanged
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RowView : INotifyPropertyChanged
    {
        private int _index;
        private double _height = 20;
        private bool _isHidden;
        private bool _isSelected;
        private bool _isFiltered;
        private string? _groupLevel;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(nameof(Height)); }
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

        public bool IsFiltered
        {
            get => _isFiltered;
            set { _isFiltered = value; OnPropertyChanged(nameof(IsFiltered)); }
        }

        public string? GroupLevel
        {
            get => _groupLevel;
            set { _groupLevel = value; OnPropertyChanged(nameof(GroupLevel)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ColumnView : INotifyPropertyChanged
    {
        private int _index;
        private string _header = "A";
        private double _width = 80;
        private bool _isHidden;
        private bool _isSelected;
        private bool _isFiltered;
        private string? _groupLevel;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public string Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(nameof(Header)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
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

        public bool IsFiltered
        {
            get => _isFiltered;
            set { _isFiltered = value; OnPropertyChanged(nameof(IsFiltered)); }
        }

        public string? GroupLevel
        {
            get => _groupLevel;
            set { _groupLevel = value; OnPropertyChanged(nameof(GroupLevel)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RangeView : INotifyPropertyChanged
    {
        private string _address = string.Empty;
        private int _startRow;
        private int _startColumn;
        private int _endRow;
        private int _endColumn;
        private bool _isSelected;

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

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public int RowCount => EndRow - StartRow + 1;
        public int ColumnCount => EndColumn - StartColumn + 1;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum SelectionMode
    {
        None,
        Single,
        Extended,
        Multi
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

    public class CellSelectedEventArgs : EventArgs
    {
        public CellView? Cell { get; set; }
    }

    public class RangeSelectedEventArgs : EventArgs
    {
        public RangeView? Range { get; set; }
    }

    public class CellEditedEventArgs : EventArgs
    {
        public CellView? Cell { get; set; }
        public object? NewValue { get; set; }
    }

    public class FormulaBarView : INotifyPropertyChanged
    {
        private string _formula = string.Empty;
        private string _selectedRange = string.Empty;
        private bool _isEditing;
        private CellView? _referencedCell;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<FormulaEnteredEventArgs>? FormulaEntered;

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

        public CellView? ReferencedCell
        {
            get => _referencedCell;
            set { _referencedCell = value; OnPropertyChanged(nameof(ReferencedCell)); }
        }

        public void EnterFormula()
        {
            FormulaEntered?.Invoke(this, new FormulaEnteredEventArgs { Formula = Formula });
        }

        public void CancelEdit()
        {
            IsEditing = false;
            Formula = string.Empty;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FormulaEnteredEventArgs : EventArgs
    {
        public string Formula { get; set; } = string.Empty;
    }

    public class FunctionArgumentView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _value = string.Empty;
        private bool _isRequired;
        private bool _isValid = true;
        private string? _errorMessage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }

        public bool IsRequired
        {
            get => _isRequired;
            set { _isRequired = value; OnPropertyChanged(nameof(IsRequired)); }
        }

        public bool IsValid
        {
            get => _isValid;
            set { _isValid = value; OnPropertyChanged(nameof(IsValid)); }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FunctionBrowserView : INotifyPropertyChanged
    {
        private string _searchText = string.Empty;
        private FunctionCategoryView? _selectedCategory;
        private FunctionView? _selectedFunction;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<FunctionSelectedEventArgs>? FunctionSelected;

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        public FunctionCategoryView? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(nameof(SelectedCategory)); }
        }

        public FunctionView? SelectedFunction
        {
            get => _selectedFunction;
            set
            {
                _selectedFunction = value;
                OnPropertyChanged(nameof(SelectedFunction));
                if (value != null)
                {
                    FunctionSelected?.Invoke(this, new FunctionSelectedEventArgs { Function = value });
                }
            }
        }

        public ObservableCollection<FunctionCategoryView> Categories { get; } = new();
        public ObservableCollection<FunctionView> Functions { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FunctionCategoryView
    {
        public string Name { get; set; } = string.Empty;
        public int FunctionCount { get; set; }
    }

    public class FunctionView
    {
        public string Name { get; set; } = string.Empty;
        public string Syntax { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FunctionCategoryView? Category { get; set; }
        public ObservableCollection<FunctionArgumentView> Arguments { get; } = new();
    }

    public class FunctionSelectedEventArgs : EventArgs
    {
        public FunctionView? Function { get; set; }
    }

    public class NameManagerView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _formula = string.Empty;
        private string _scope = "Workbook";
        private string _comment = string.Empty;
        private bool _isEditing;
        private NameView? _selectedName;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<NameCreatedEventArgs>? NameCreated;
        public event EventHandler<NameDeletedEventArgs>? NameDeleted;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Formula
        {
            get => _formula;
            set { _formula = value; OnPropertyChanged(nameof(Formula)); }
        }

        public string Scope
        {
            get => _scope;
            set { _scope = value; OnPropertyChanged(nameof(Scope)); }
        }

        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(nameof(Comment)); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }

        public NameView? SelectedName
        {
            get => _selectedName;
            set { _selectedName = value; OnPropertyChanged(nameof(SelectedName)); }
        }

        public ObservableCollection<NameView> Names { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NameView
    {
        public string Name { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Scope { get; set; } = "Workbook";
        public string Comment { get; set; } = string.Empty;
    }

    public class NameCreatedEventArgs : EventArgs
    {
        public NameView? Name { get; set; }
    }

    public class NameDeletedEventArgs : EventArgs
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ChartAreaView : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _backgroundColor = "#FFFFFF";
        private bool _showBorder;
        private double _borderWidth = 1;
        private string _borderColor = "#000000";

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public bool ShowBorder
        {
            get => _showBorder;
            set { _showBorder = value; OnPropertyChanged(nameof(ShowBorder)); }
        }

        public double BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; OnPropertyChanged(nameof(BorderWidth)); }
        }

        public string BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; OnPropertyChanged(nameof(BorderColor)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartAxisView : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private AxisPosition _position = AxisPosition.Left;
        private bool _showTitle;
        private bool _showMajorGridLines = true;
        private bool _showMinorGridLines;
        private double _minimum;
        private double _maximum;
        private double _majorUnit;
        private double _minorUnit;
        private string _numberFormat = string.Empty;
        private bool _logScale;
        private bool _reversed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public AxisPosition Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public bool ShowTitle
        {
            get => _showTitle;
            set { _showTitle = value; OnPropertyChanged(nameof(ShowTitle)); }
        }

        public bool ShowMajorGridLines
        {
            get => _showMajorGridLines;
            set { _showMajorGridLines = value; OnPropertyChanged(nameof(ShowMajorGridLines)); }
        }

        public bool ShowMinorGridLines
        {
            get => _showMinorGridLines;
            set { _showMinorGridLines = value; OnPropertyChanged(nameof(ShowMinorGridLines)); }
        }

        public double Minimum
        {
            get => _minimum;
            set { _minimum = value; OnPropertyChanged(nameof(Minimum)); }
        }

        public double Maximum
        {
            get => _maximum;
            set { _maximum = value; OnPropertyChanged(nameof(Maximum)); }
        }

        public double MajorUnit
        {
            get => _majorUnit;
            set { _majorUnit = value; OnPropertyChanged(nameof(MajorUnit)); }
        }

        public double MinorUnit
        {
            get => _minorUnit;
            set { _minorUnit = value; OnPropertyChanged(nameof(MinorUnit)); }
        }

        public string NumberFormat
        {
            get => _numberFormat;
            set { _numberFormat = value; OnPropertyChanged(nameof(NumberFormat)); }
        }

        public bool LogScale
        {
            get => _logScale;
            set { _logScale = value; OnPropertyChanged(nameof(LogScale)); }
        }

        public bool Reversed
        {
            get => _reversed;
            set { _reversed = value; OnPropertyChanged(nameof(Reversed)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum AxisPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class DataSeriesView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _range = string.Empty;
        private ChartType _chartType = ChartType.Column;
        private string _color = "#4472C4";
        private MarkerStyle _markerStyle = MarkerStyle.None;
        public ObservableCollection<double> Values { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Range
        {
            get => _range;
            set { _range = value; OnPropertyChanged(nameof(Range)); }
        }

        public ChartType ChartType
        {
            get => _chartType;
            set { _chartType = value; OnPropertyChanged(nameof(ChartType)); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        public MarkerStyle MarkerStyle
        {
            get => _markerStyle;
            set { _markerStyle = value; OnPropertyChanged(nameof(MarkerStyle)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartLegendView : INotifyPropertyChanged
    {
        private string _position = "Right";
        private bool _showLegend = true;
        private string _backgroundColor = "#FFFFFF";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set { _showLegend = value; OnPropertyChanged(nameof(ShowLegend)); }
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

    public enum ChartType
    {
        Column,
        Bar,
        Line,
        Pie,
        Area,
        Scatter,
        Doughnut,
        Radar,
        Stock,
        Surface
    }

    public enum MarkerStyle
    {
        None,
        Circle,
        Square,
        Diamond,
        Triangle,
        X,
        Star,
        Plus
    }

    public class PivotTableFieldListView : INotifyPropertyChanged
    {
        private PivotFieldView? _selectedField;
        private string _activeField = "Row Fields";

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<FieldDroppedEventArgs>? FieldDropped;

        public PivotFieldView? SelectedField
        {
            get => _selectedField;
            set { _selectedField = value; OnPropertyChanged(nameof(SelectedField)); }
        }

        public string ActiveField
        {
            get => _activeField;
            set { _activeField = value; OnPropertyChanged(nameof(ActiveField)); }
        }

        public ObservableCollection<PivotFieldView> AvailableFields { get; } = new();
        public ObservableCollection<PivotFieldView> RowFields { get; } = new();
        public ObservableCollection<PivotFieldView> ColumnFields { get; } = new();
        public ObservableCollection<PivotFieldView> DataFields { get; } = new();
        public ObservableCollection<PivotFieldView> FilterFields { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PivotFieldView : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private PivotFieldOrientation _orientation = PivotFieldOrientation.RowField;
        private string _subtotalFunction = "SUM";
        private bool _showSubtotals = true;

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

        public string SubtotalFunction
        {
            get => _subtotalFunction;
            set { _subtotalFunction = value; OnPropertyChanged(nameof(SubtotalFunction)); }
        }

        public bool ShowSubtotals
        {
            get => _showSubtotals;
            set { _showSubtotals = value; OnPropertyChanged(nameof(ShowSubtotals)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum PivotFieldOrientation
    {
        RowField,
        ColumnField,
        DataField,
        PageField,
        Hidden
    }

    public class FieldDroppedEventArgs : EventArgs
    {
        public PivotFieldView? Field { get; set; }
        public PivotFieldOrientation TargetOrientation { get; set; }
    }

    public class ScenarioManagerView : INotifyPropertyChanged
    {
        private ScenarioView? _selectedScenario;
        private bool _isEditing;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<ScenarioSelectedEventArgs>? ScenarioSelected;

        public ScenarioView? SelectedScenario
        {
            get => _selectedScenario;
            set
            {
                _selectedScenario = value;
                OnPropertyChanged(nameof(SelectedScenario));
                if (value != null)
                {
                    ScenarioSelected?.Invoke(this, new ScenarioSelectedEventArgs { Scenario = value });
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(nameof(IsEditing)); }
        }

        public ObservableCollection<ScenarioView> Scenarios { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ScenarioView
    {
        public string Name { get; set; } = string.Empty;
        public string ChangingCells { get; set; } = string.Empty;
        public Dictionary<string, object> Values { get; set; } = new();
        public string Comment { get; set; } = string.Empty;
    }

    public class ScenarioSelectedEventArgs : EventArgs
    {
        public ScenarioView? Scenario { get; set; }
    }

    public class WhatIfAnalysisView : INotifyPropertyChanged
    {
        private string _type = "DataTable";
        private string _targetCell = string.Empty;
        private string _variableCell = string.Empty;
        private string _resultRange = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string TargetCell
        {
            get => _targetCell;
            set { _targetCell = value; OnPropertyChanged(nameof(TargetCell)); }
        }

        public string VariableCell
        {
            get => _variableCell;
            set { _variableCell = value; OnPropertyChanged(nameof(VariableCell)); }
        }

        public string ResultRange
        {
            get => _resultRange;
            set { _resultRange = value; OnPropertyChanged(nameof(ResultRange)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

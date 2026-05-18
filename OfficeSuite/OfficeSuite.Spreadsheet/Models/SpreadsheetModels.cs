using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OfficeSuite.Spreadsheet.Models
{
    public class SpreadsheetDocument : INotifyPropertyChanged
    {
        private string _title = "Untitled Workbook";
        private string _filePath = string.Empty;
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;
        private string _author = string.Empty;
        private string _subject = string.Empty;
        private string _keywords = string.Empty;
        private string _category = string.Empty;
        private string _comments = string.Empty;
        private string _company = string.Empty;
        private bool _isModified;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Worksheet> Sheets { get; } = new();
        public ObservableCollection<NamedRange> NamedRanges { get; } = new();
        public ObservableCollection<CustomProperty> CustomProperties { get; } = new();

        public WorksheetProperties DefaultProperties { get; } = new();

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
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
            get => _company;
            set { _company = value; OnPropertyChanged(nameof(Company)); }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(nameof(IsModified)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Worksheet : INotifyPropertyChanged
    {
        private string _name = "Sheet";
        private int _index;
        private bool _isVisible = true;
        private bool _isSelected;
        private string _tabColor = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Row> Rows { get; } = new();
        public ObservableCollection<Column> Columns { get; } = new();
        public ObservableCollection<Cell> Cells { get; } = new();
        public ObservableCollection<Chart> Charts { get; } = new();
        public ObservableCollection<PivotTable> PivotTables { get; } = new();
        public ObservableCollection<Sparkline> Sparklines { get; } = new();
        public ObservableCollection<NamedRange> NamedRanges { get; } = new();
        public ObservableCollection<Hyperlink> Hyperlinks { get; } = new();
        public ObservableCollection<DataValidation> DataValidations { get; } = new();
        public ObservableCollection<ConditionalFormatting> ConditionalFormattings { get; } = new();

        public WorksheetProtection? Protection { get; set; }
        public AutoFilter? Filter { get; set; }
        public FreezePanes? FreezePanes { get; set; }
        public PrintSettings? PrintSettings { get; set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
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

        public string TabColor
        {
            get => _tabColor;
            set { _tabColor = value; OnPropertyChanged(nameof(TabColor)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WorksheetProperties
    {
        public double StandardWidth = 8.43;
        public string StandardFont = "Calibri";
        public double StandardFontSize = 11;
        public bool ShowGridLines = true;
        public bool ShowHeadings = true;
        public GridLineColor GridLineColor = new() { Color = "#D0D0D0" };
        public ZoomLevel ZoomLevel = new() { Scale = 100 };
    }

    public class Cell : INotifyPropertyChanged
    {
        private string _address = "A1";
        private object? _value;
        private string _formula = string.Empty;
        private CellType _cellType = CellType.General;
        private string _format = string.Empty;
        private string _fontName = "Calibri";
        private double _fontSize = 11;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderline;
        private bool _isStrikethrough;
        private bool _isSubscript;
        private bool _isSuperscript;
        private string _fontColor = "#000000";
        private string _fillColor = "#FFFFFF";
        private string _borderTop = string.Empty;
        private string _borderBottom = string.Empty;
        private string _borderLeft = string.Empty;
        private string _borderRight = string.Empty;
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.General;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Bottom;
        private int _textRotation;
        private bool _wrapText;
        private bool _shrinkToFit;
        private int _indentLevel;
        private bool _isLocked = true;
        private bool _isHidden;
        private bool _isSelected;
        private string? _comment;
        private bool _hasValidationError;
        private string? _validationError;
        private string? _hyperlink;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        public object? Value
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

        public bool IsStrikethrough
        {
            get => _isStrikethrough;
            set { _isStrikethrough = value; OnPropertyChanged(nameof(IsStrikethrough)); }
        }

        public bool IsSubscript
        {
            get => _isSubscript;
            set { _isSubscript = value; OnPropertyChanged(nameof(IsSubscript)); }
        }

        public bool IsSuperscript
        {
            get => _isSuperscript;
            set { _isSuperscript = value; OnPropertyChanged(nameof(IsSuperscript)); }
        }

        public string FontColor
        {
            get => _fontColor;
            set { _fontColor = value; OnPropertyChanged(nameof(FontColor)); }
        }

        public string FillColor
        {
            get => _fillColor;
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
        }

        public string BorderTop
        {
            get => _borderTop;
            set { _borderTop = value; OnPropertyChanged(nameof(BorderTop)); }
        }

        public string BorderBottom
        {
            get => _borderBottom;
            set { _borderBottom = value; OnPropertyChanged(nameof(BorderBottom)); }
        }

        public string BorderLeft
        {
            get => _borderLeft;
            set { _borderLeft = value; OnPropertyChanged(nameof(BorderLeft)); }
        }

        public string BorderRight
        {
            get => _borderRight;
            set { _borderRight = value; OnPropertyChanged(nameof(BorderRight)); }
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

        public int TextRotation
        {
            get => _textRotation;
            set { _textRotation = value; OnPropertyChanged(nameof(TextRotation)); }
        }

        public bool WrapText
        {
            get => _wrapText;
            set { _wrapText = value; OnPropertyChanged(nameof(WrapText)); }
        }

        public bool ShrinkToFit
        {
            get => _shrinkToFit;
            set { _shrinkToFit = value; OnPropertyChanged(nameof(ShrinkToFit)); }
        }

        public int IndentLevel
        {
            get => _indentLevel;
            set { _indentLevel = value; OnPropertyChanged(nameof(IndentLevel)); }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(nameof(IsLocked)); }
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

        public string? Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(nameof(Comment)); }
        }

        public bool HasValidationError
        {
            get => _hasValidationError;
            set { _hasValidationError = value; OnPropertyChanged(nameof(HasValidationError)); }
        }

        public string? ValidationError
        {
            get => _validationError;
            set { _validationError = value; OnPropertyChanged(nameof(ValidationError)); }
        }

        public string? Hyperlink
        {
            get => _hyperlink;
            set { _hyperlink = value; OnPropertyChanged(nameof(Hyperlink)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Row : INotifyPropertyChanged
    {
        private int _index;
        private double _height = 15;
        private bool _isHidden;
        private bool _isFiltered;
        private bool _isGrouped;
        private int _groupLevel;
        private bool _isSelected;
        private bool _isProtected;
        private bool _hasCustomHeight;
        private RowOutlineLevel _outlineLevel = RowOutlineLevel.None;

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

        public bool IsFiltered
        {
            get => _isFiltered;
            set { _isFiltered = value; OnPropertyChanged(nameof(IsFiltered)); }
        }

        public bool IsGrouped
        {
            get => _isGrouped;
            set { _isGrouped = value; OnPropertyChanged(nameof(IsGrouped)); }
        }

        public int GroupLevel
        {
            get => _groupLevel;
            set { _groupLevel = value; OnPropertyChanged(nameof(GroupLevel)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsProtected
        {
            get => _isProtected;
            set { _isProtected = value; OnPropertyChanged(nameof(IsProtected)); }
        }

        public bool HasCustomHeight
        {
            get => _hasCustomHeight;
            set { _hasCustomHeight = value; OnPropertyChanged(nameof(HasCustomHeight)); }
        }

        public RowOutlineLevel OutlineLevel
        {
            get => _outlineLevel;
            set { _outlineLevel = value; OnPropertyChanged(nameof(OutlineLevel)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Column : INotifyPropertyChanged
    {
        private int _index;
        private string _header = "A";
        private double _width = 8.43;
        private bool _isHidden;
        private bool _isFiltered;
        private bool _isGrouped;
        private int _groupLevel;
        private bool _isSelected;
        private bool _isProtected;
        private bool _hasCustomWidth;
        private ColumnOutlineLevel _outlineLevel = ColumnOutlineLevel.None;

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

        public bool IsFiltered
        {
            get => _isFiltered;
            set { _isFiltered = value; OnPropertyChanged(nameof(IsFiltered)); }
        }

        public bool IsGrouped
        {
            get => _isGrouped;
            set { _isGrouped = value; OnPropertyChanged(nameof(IsGrouped)); }
        }

        public int GroupLevel
        {
            get => _groupLevel;
            set { _groupLevel = value; OnPropertyChanged(nameof(GroupLevel)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsProtected
        {
            get => _isProtected;
            set { _isProtected = value; OnPropertyChanged(nameof(IsProtected)); }
        }

        public bool HasCustomWidth
        {
            get => _hasCustomWidth;
            set { _hasCustomWidth = value; OnPropertyChanged(nameof(HasCustomWidth)); }
        }

        public ColumnOutlineLevel OutlineLevel
        {
            get => _outlineLevel;
            set { _outlineLevel = value; OnPropertyChanged(nameof(OutlineLevel)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Range
    {
        public string Address { get; set; } = string.Empty;
        public int StartRow { get; set; }
        public int StartColumn { get; set; }
        public int EndRow { get; set; }
        public int EndColumn { get; set; }
        public string Worksheet { get; set; } = string.Empty;

        public int RowCount => EndRow - StartRow + 1;
        public int ColumnCount => EndColumn - StartColumn + 1;
    }

    public class NamedRange : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _reference = string.Empty;
        private string _scope = "Workbook";
        private string _comment = string.Empty;
        private bool _isVisible = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Reference
        {
            get => _reference;
            set { _reference = value; OnPropertyChanged(nameof(Reference)); }
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

    public class Chart : INotifyPropertyChanged
    {
        private string _name = "Chart";
        private string _title = string.Empty;
        private ChartType _chartType = ChartType.Column;
        private double _left;
        private double _top;
        private double _width = 300;
        private double _height = 200;
        private bool _showTitle;
        private bool _showLegend = true;
        private bool _showDataLabels;
        private bool _showGridLines = true;
        private string _dataSourceRange = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChartArea? Area { get; set; }
        public ChartPlotArea? PlotArea { get; set; }
        public ChartLegend? Legend { get; set; }
        public ChartTitle? Title_ { get; set; }
        public ObservableCollection<ChartSeries> Series { get; } = new();
        public ObservableCollection<ChartAxis> Axes { get; } = new();

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

        public ChartType ChartType
        {
            get => _chartType;
            set { _chartType = value; OnPropertyChanged(nameof(ChartType)); }
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

        public bool ShowTitle
        {
            get => _showTitle;
            set { _showTitle = value; OnPropertyChanged(nameof(ShowTitle)); }
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set { _showLegend = value; OnPropertyChanged(nameof(ShowLegend)); }
        }

        public bool ShowDataLabels
        {
            get => _showDataLabels;
            set { _showDataLabels = value; OnPropertyChanged(nameof(ShowDataLabels)); }
        }

        public bool ShowGridLines
        {
            get => _showGridLines;
            set { _showGridLines = value; OnPropertyChanged(nameof(ShowGridLines)); }
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

    public class ChartArea : INotifyPropertyChanged
    {
        private string _fillColor = "#FFFFFF";
        private string _borderColor = "#000000";
        private double _borderWidth;
        private bool _showShadow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FillColor
        {
            get => _fillColor;
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
        }

        public string BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; OnPropertyChanged(nameof(BorderColor)); }
        }

        public double BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; OnPropertyChanged(nameof(BorderWidth)); }
        }

        public bool ShowShadow
        {
            get => _showShadow;
            set { _showShadow = value; OnPropertyChanged(nameof(ShowShadow)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartPlotArea : INotifyPropertyChanged
    {
        private string _fillColor = "#FFFFFF";
        private int _borderWidth;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FillColor
        {
            get => _fillColor;
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
        }

        public int BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; OnPropertyChanged(nameof(BorderWidth)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartLegend : INotifyPropertyChanged
    {
        private string _position = "Right";
        private string _fillColor = "#FFFFFF";
        private string _fontName = "Calibri";
        private double _fontSize = 9;
        private bool _showShadow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public string FillColor
        {
            get => _fillColor;
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
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

        public bool ShowShadow
        {
            get => _showShadow;
            set { _showShadow = value; OnPropertyChanged(nameof(ShowShadow)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartTitle : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private string _fontName = "Calibri";
        private double _fontSize = 14;
        private bool _isBold;
        private string _fontColor = "#000000";

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

        public string FontColor
        {
            get => _fontColor;
            set { _fontColor = value; OnPropertyChanged(nameof(FontColor)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartSeries : INotifyPropertyChanged
    {
        private string _name = "Series";
        private string _values = string.Empty;
        private string _categories = string.Empty;
        private ChartType _chartType = ChartType.Column;
        private string _color = "#4472C4";
        private MarkerStyle _markerStyle = MarkerStyle.None;
        public ObservableCollection<object> Values { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChartLine? Line { get; set; }
        public ChartDataLabels? DataLabels { get; set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Values_range
        {
            get => _values;
            set { _values = value; OnPropertyChanged(nameof(Values_range)); }
        }

        public string Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(nameof(Categories)); }
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

    public class ChartLine : INotifyPropertyChanged
    {
        private string _color = "#000000";
        private double _width = 1;
        private LineStyle _style = LineStyle.Solid;
        private bool _showShadow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
        }

        public LineStyle Style
        {
            get => _style;
            set { _style = value; OnPropertyChanged(nameof(Style)); }
        }

        public bool ShowShadow
        {
            get => _showShadow;
            set { _showShadow = value; OnPropertyChanged(nameof(ShowShadow)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartDataLabels : INotifyPropertyChanged
    {
        private bool _showValue;
        private bool _showCategory;
        private bool _showSeries;
        private bool _showPercentage;
        private bool _showBubbleSize;
        private string _separator = ",";
        private string _fontName = "Calibri";
        private double _fontSize = 9;
        private bool _isBold;
        private string _fontColor = "#000000";

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool ShowValue
        {
            get => _showValue;
            set { _showValue = value; OnPropertyChanged(nameof(ShowValue)); }
        }

        public bool ShowCategory
        {
            get => _showCategory;
            set { _showCategory = value; OnPropertyChanged(nameof(ShowCategory)); }
        }

        public bool ShowSeries
        {
            get => _showSeries;
            set { _showSeries = value; OnPropertyChanged(nameof(ShowSeries)); }
        }

        public bool ShowPercentage
        {
            get => _showPercentage;
            set { _showPercentage = value; OnPropertyChanged(nameof(ShowPercentage)); }
        }

        public bool ShowBubbleSize
        {
            get => _showBubbleSize;
            set { _showBubbleSize = value; OnPropertyChanged(nameof(ShowBubbleSize)); }
        }

        public string Separator
        {
            get => _separator;
            set { _separator = value; OnPropertyChanged(nameof(Separator)); }
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

        public string FontColor
        {
            get => _fontColor;
            set { _fontColor = value; OnPropertyChanged(nameof(FontColor)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChartAxis : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private AxisPosition _position = AxisPosition.Left;
        private double _minimum;
        private double _maximum;
        private double _majorUnit;
        private double _minorUnit;
        private bool _logScale;
        private bool _reversed;
        private bool _showMajorGridLines = true;
        private bool _showMinorGridLines;
        private string _numberFormat = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AxisType Type { get; set; } = AxisType.Value;

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

        public string NumberFormat
        {
            get => _numberFormat;
            set { _numberFormat = value; OnPropertyChanged(nameof(NumberFormat)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PivotTable : INotifyPropertyChanged
    {
        private string _name = "PivotTable";
        private string _dataSourceRange = string.Empty;
        private string _targetRange = string.Empty;
        private string _dataSheet = string.Empty;
        private bool _hasGrandTotals = true;
        private bool _hasSubtotals = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<PivotField> RowFields { get; } = new();
        public ObservableCollection<PivotField> ColumnFields { get; } = new();
        public ObservableCollection<PivotField> DataFields { get; } = new();
        public ObservableCollection<PivotField> FilterFields { get; } = new();

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

        public bool HasGrandTotals
        {
            get => _hasGrandTotals;
            set { _hasGrandTotals = value; OnPropertyChanged(nameof(HasGrandTotals)); }
        }

        public bool HasSubtotals
        {
            get => _hasSubtotals;
            set { _hasSubtotals = value; OnPropertyChanged(nameof(HasSubtotals)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PivotField : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private PivotFieldOrientation _orientation = PivotFieldOrientation.RowField;
        private string _customName = string.Empty;
        private bool _showSubtotals = true;
        private bool _showInOutlineForm = true;
        private int _compactRowIndent;
        private string _subtotalFunction = "SUM";
        private bool _includeNewItemsInFilter;
        private bool _isHidden;
        private bool _isExpanded;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<object> SelectedItems { get; } = new();

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

        public bool IncludeNewItemsInFilter
        {
            get => _includeNewItemsInFilter;
            set { _includeNewItemsInFilter = value; OnPropertyChanged(nameof(IncludeNewItemsInFilter)); }
        }

        public bool IsHidden
        {
            get => _isHidden;
            set { _isHidden = value; OnPropertyChanged(nameof(IsHidden)); }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Sparkline : INotifyPropertyChanged
    {
        private string _location = "A1";
        private string _dataRange = string.Empty;
        private SparklineType _type = SparklineType.Line;
        private string _color = "#4472C4";
        private string _highPointColor = "#C00000";
        private string _lowPointColor = "#00B050";
        private string _negativePointColor = "#C00000";
        private string _firstPointColor = "#4472C4";
        private string _lastPointColor = "#4472C4";
        private bool _showMarkers;
        private bool _showHighPoint;
        private bool _showLowPoint;
        private bool _showNegativePoints;
        private bool _showFirstPoint;
        private bool _showLastPoint;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(nameof(Location)); }
        }

        public string DataRange
        {
            get => _dataRange;
            set { _dataRange = value; OnPropertyChanged(nameof(DataRange)); }
        }

        public SparklineType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        public string HighPointColor
        {
            get => _highPointColor;
            set { _highPointColor = value; OnPropertyChanged(nameof(HighPointColor)); }
        }

        public string LowPointColor
        {
            get => _lowPointColor;
            set { _lowPointColor = value; OnPropertyChanged(nameof(LowPointColor)); }
        }

        public string NegativePointColor
        {
            get => _negativePointColor;
            set { _negativePointColor = value; OnPropertyChanged(nameof(NegativePointColor)); }
        }

        public string FirstPointColor
        {
            get => _firstPointColor;
            set { _firstPointColor = value; OnPropertyChanged(nameof(FirstPointColor)); }
        }

        public string LastPointColor
        {
            get => _lastPointColor;
            set { _lastPointColor = value; OnPropertyChanged(nameof(LastPointColor)); }
        }

        public bool ShowMarkers
        {
            get => _showMarkers;
            set { _showMarkers = value; OnPropertyChanged(nameof(ShowMarkers)); }
        }

        public bool ShowHighPoint
        {
            get => _showHighPoint;
            set { _showHighPoint = value; OnPropertyChanged(nameof(ShowHighPoint)); }
        }

        public bool ShowLowPoint
        {
            get => _showLowPoint;
            set { _showLowPoint = value; OnPropertyChanged(nameof(ShowLowPoint)); }
        }

        public bool ShowNegativePoints
        {
            get => _showNegativePoints;
            set { _showNegativePoints = value; OnPropertyChanged(nameof(ShowNegativePoints)); }
        }

        public bool ShowFirstPoint
        {
            get => _showFirstPoint;
            set { _showFirstPoint = value; OnPropertyChanged(nameof(ShowFirstPoint)); }
        }

        public bool ShowLastPoint
        {
            get => _showLastPoint;
            set { _showLastPoint = value; OnPropertyChanged(nameof(ShowLastPoint)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Hyperlink : INotifyPropertyChanged
    {
        private string _cellReference = string.Empty;
        private string _url = string.Empty;
        private string _displayText = string.Empty;
        private string _screenTip = string.Empty;
        private HyperlinkType _type = HyperlinkType.Web;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string CellReference
        {
            get => _cellReference;
            set { _cellReference = value; OnPropertyChanged(nameof(CellReference)); }
        }

        public string Url
        {
            get => _url;
            set { _url = value; OnPropertyChanged(nameof(Url)); }
        }

        public string DisplayText
        {
            get => _displayText;
            set { _displayText = value; OnPropertyChanged(nameof(DisplayText)); }
        }

        public string ScreenTip
        {
            get => _screenTip;
            set { _screenTip = value; OnPropertyChanged(nameof(ScreenTip)); }
        }

        public HyperlinkType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DataValidation : INotifyPropertyChanged
    {
        private string _range = string.Empty;
        private DataValidationType _type = DataValidationType.AnyValue;
        private string _formula1 = string.Empty;
        private string _formula2 = string.Empty;
        private bool _ignoreBlank = true;
        private bool _showDropDown = true;
        private bool _showInputMessage;
        private string _inputTitle = string.Empty;
        private string _inputMessage = string.Empty;
        private bool _showErrorAlert = true;
        private ErrorStyle _errorStyle = ErrorStyle.Stop;
        private string _errorTitle = string.Empty;
        private string _errorMessage = string.Empty;
        public ObservableCollection<string> ValueList { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Range
        {
            get => _range;
            set { _range = value; OnPropertyChanged(nameof(Range)); }
        }

        public DataValidationType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string Formula1
        {
            get => _formula1;
            set { _formula1 = value; OnPropertyChanged(nameof(Formula1)); }
        }

        public string Formula2
        {
            get => _formula2;
            set { _formula2 = value; OnPropertyChanged(nameof(Formula2)); }
        }

        public bool IgnoreBlank
        {
            get => _ignoreBlank;
            set { _ignoreBlank = value; OnPropertyChanged(nameof(IgnoreBlank)); }
        }

        public bool ShowDropDown
        {
            get => _showDropDown;
            set { _showDropDown = value; OnPropertyChanged(nameof(ShowDropDown)); }
        }

        public bool ShowInputMessage
        {
            get => _showInputMessage;
            set { _showInputMessage = value; OnPropertyChanged(nameof(ShowInputMessage)); }
        }

        public string InputTitle
        {
            get => _inputTitle;
            set { _inputTitle = value; OnPropertyChanged(nameof(InputTitle)); }
        }

        public string InputMessage
        {
            get => _inputMessage;
            set { _inputMessage = value; OnPropertyChanged(nameof(InputMessage)); }
        }

        public bool ShowErrorAlert
        {
            get => _showErrorAlert;
            set { _showErrorAlert = value; OnPropertyChanged(nameof(ShowErrorAlert)); }
        }

        public ErrorStyle ErrorStyle
        {
            get => _errorStyle;
            set { _errorStyle = value; OnPropertyChanged(nameof(ErrorStyle)); }
        }

        public string ErrorTitle
        {
            get => _errorTitle;
            set { _errorTitle = value; OnPropertyChanged(nameof(ErrorTitle)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConditionalFormatting : INotifyPropertyChanged
    {
        private string _range = string.Empty;
        private ConditionalRuleType _ruleType = ConditionalRuleType.CellValue;
        private ConditionalOperator _operator = ConditionalOperator.Equal;
        private string _formula1 = string.Empty;
        private string _formula2 = string.Empty;
        private string _fillColor = "#FFFFFF";
        private string _fontColor = "#000000";
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderline;
        private string _borderStyle = "None";
        private int _priority = 1;
        private bool _stopIfTrue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Range
        {
            get => _range;
            set { _range = value; OnPropertyChanged(nameof(Range)); }
        }

        public ConditionalRuleType RuleType
        {
            get => _ruleType;
            set { _ruleType = value; OnPropertyChanged(nameof(RuleType)); }
        }

        public ConditionalOperator Operator
        {
            get => _operator;
            set { _operator = value; OnPropertyChanged(nameof(Operator)); }
        }

        public string Formula1
        {
            get => _formula1;
            set { _formula1 = value; OnPropertyChanged(nameof(Formula1)); }
        }

        public string Formula2
        {
            get => _formula2;
            set { _formula2 = value; OnPropertyChanged(nameof(Formula2)); }
        }

        public string FillColor
        {
            get => _fillColor;
            set { _fillColor = value; OnPropertyChanged(nameof(FillColor)); }
        }

        public string FontColor
        {
            get => _fontColor;
            set { _fontColor = value; OnPropertyChanged(nameof(FontColor)); }
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

        public string BorderStyle
        {
            get => _borderStyle;
            set { _borderStyle = value; OnPropertyChanged(nameof(BorderStyle)); }
        }

        public int Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(nameof(Priority)); }
        }

        public bool StopIfTrue
        {
            get => _stopIfTrue;
            set { _stopIfTrue = value; OnPropertyChanged(nameof(StopIfTrue)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WorksheetProtection : INotifyPropertyChanged
    {
        private bool _isProtected;
        private string? _password;
        private bool _allowSelectingLockedCells = true;
        private bool _allowSelectingUnlockedCells = true;
        private bool _allowFormattingCells;
        private bool _allowFormattingColumns;
        private bool _allowFormattingRows;
        private bool _allowInsertingColumns;
        private bool _allowInsertingRows;
        private bool _allowInsertingHyperlinks;
        private bool _allowDeletingColumns;
        private bool _allowDeletingRows;
        private bool _allowSorting;
        private bool _allowFiltering;
        private bool _allowUsingPivotTables;
        private bool _allowEditingObjects;
        private bool _allowEditingScenarios;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsProtected
        {
            get => _isProtected;
            set { _isProtected = value; OnPropertyChanged(nameof(IsProtected)); }
        }

        public string? Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public bool AllowSelectingLockedCells
        {
            get => _allowSelectingLockedCells;
            set { _allowSelectingLockedCells = value; OnPropertyChanged(nameof(AllowSelectingLockedCells)); }
        }

        public bool AllowSelectingUnlockedCells
        {
            get => _allowSelectingUnlockedCells;
            set { _allowSelectingUnlockedCells = value; OnPropertyChanged(nameof(AllowSelectingUnlockedCells)); }
        }

        public bool AllowFormattingCells
        {
            get => _allowFormattingCells;
            set { _allowFormattingCells = value; OnPropertyChanged(nameof(AllowFormattingCells)); }
        }

        public bool AllowFormattingColumns
        {
            get => _allowFormattingColumns;
            set { _allowFormattingColumns = value; OnPropertyChanged(nameof(AllowFormattingColumns)); }
        }

        public bool AllowFormattingRows
        {
            get => _allowFormattingRows;
            set { _allowFormattingRows = value; OnPropertyChanged(nameof(AllowFormattingRows)); }
        }

        public bool AllowInsertingColumns
        {
            get => _allowInsertingColumns;
            set { _allowInsertingColumns = value; OnPropertyChanged(nameof(AllowInsertingColumns)); }
        }

        public bool AllowInsertingRows
        {
            get => _allowInsertingRows;
            set { _allowInsertingRows = value; OnPropertyChanged(nameof(AllowInsertingRows)); }
        }

        public bool AllowInsertingHyperlinks
        {
            get => _allowInsertingHyperlinks;
            set { _allowInsertingHyperlinks = value; OnPropertyChanged(nameof(AllowInsertingHyperlinks)); }
        }

        public bool AllowDeletingColumns
        {
            get => _allowDeletingColumns;
            set { _allowDeletingColumns = value; OnPropertyChanged(nameof(AllowDeletingColumns)); }
        }

        public bool AllowDeletingRows
        {
            get => _allowDeletingRows;
            set { _allowDeletingRows = value; OnPropertyChanged(nameof(AllowDeletingRows)); }
        }

        public bool AllowSorting
        {
            get => _allowSorting;
            set { _allowSorting = value; OnPropertyChanged(nameof(AllowSorting)); }
        }

        public bool AllowFiltering
        {
            get => _allowFiltering;
            set { _allowFiltering = value; OnPropertyChanged(nameof(AllowFiltering)); }
        }

        public bool AllowUsingPivotTables
        {
            get => _allowUsingPivotTables;
            set { _allowUsingPivotTables = value; OnPropertyChanged(nameof(AllowUsingPivotTables)); }
        }

        public bool AllowEditingObjects
        {
            get => _allowEditingObjects;
            set { _allowEditingObjects = value; OnPropertyChanged(nameof(AllowEditingObjects)); }
        }

        public bool AllowEditingScenarios
        {
            get => _allowEditingScenarios;
            set { _allowEditingScenarios = value; OnPropertyChanged(nameof(AllowEditingScenarios)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AutoFilter
    {
        public string Range { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public ObservableCollection<FilterColumn> FilterColumns { get; } = new();
    }

    public class FilterColumn
    {
        public int ColumnId { get; set; }
        public FilterType Type { get; set; }
        public string? Criteria1 { get; set; }
        public string? Criteria2 { get; set; }
        public ObservableCollection<object> Values { get; } = new();
    }

    public class FreezePanes
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class PrintSettings : INotifyPropertyChanged
    {
        private string _printArea = string.Empty;
        private string _printTitles = string.Empty;
        private bool _printGridLines;
        private bool _printHeadings;
        private bool _printNotes;
        private bool _printInBlackAndWhite;
        private bool _printDraft;
        private PageOrientation _orientation = PageOrientation.Portrait;
        private PaperSize _paperSize = PaperSize.Letter;
        private double _pageWidth = 8.5;
        private double _pageHeight = 11;
        private int _copies = 1;
        private bool _collate = true;
        private PageOrder _pageOrder = PageOrder.DownThenOver;
        private int _firstPageNumber = 1;
        private int _fitToWidth = 1;
        private int _fitToHeight = 1;
        private bool _fitToPage;
        private MarginSettings _margins = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string PrintArea
        {
            get => _printArea;
            set { _printArea = value; OnPropertyChanged(nameof(PrintArea)); }
        }

        public string PrintTitles
        {
            get => _printTitles;
            set { _printTitles = value; OnPropertyChanged(nameof(PrintTitles)); }
        }

        public bool PrintGridLines
        {
            get => _printGridLines;
            set { _printGridLines = value; OnPropertyChanged(nameof(PrintGridLines)); }
        }

        public bool PrintHeadings
        {
            get => _printHeadings;
            set { _printHeadings = value; OnPropertyChanged(nameof(PrintHeadings)); }
        }

        public bool PrintNotes
        {
            get => _printNotes;
            set { _printNotes = value; OnPropertyChanged(nameof(PrintNotes)); }
        }

        public bool PrintInBlackAndWhite
        {
            get => _printInBlackAndWhite;
            set { _printInBlackAndWhite = value; OnPropertyChanged(nameof(PrintInBlackAndWhite)); }
        }

        public bool PrintDraft
        {
            get => _printDraft;
            set { _printDraft = value; OnPropertyChanged(nameof(PrintDraft)); }
        }

        public PageOrientation Orientation
        {
            get => _orientation;
            set { _orientation = value; OnPropertyChanged(nameof(Orientation)); }
        }

        public PaperSize PaperSize
        {
            get => _paperSize;
            set { _paperSize = value; OnPropertyChanged(nameof(PaperSize)); }
        }

        public double PageWidth
        {
            get => _pageWidth;
            set { _pageWidth = value; OnPropertyChanged(nameof(PageWidth)); }
        }

        public double PageHeight
        {
            get => _pageHeight;
            set { _pageHeight = value; OnPropertyChanged(nameof(PageHeight)); }
        }

        public int Copies
        {
            get => _copies;
            set { _copies = value; OnPropertyChanged(nameof(Copies)); }
        }

        public bool Collate
        {
            get => _collate;
            set { _collate = value; OnPropertyChanged(nameof(Collate)); }
        }

        public PageOrder PageOrder
        {
            get => _pageOrder;
            set { _pageOrder = value; OnPropertyChanged(nameof(PageOrder)); }
        }

        public int FirstPageNumber
        {
            get => _firstPageNumber;
            set { _firstPageNumber = value; OnPropertyChanged(nameof(FirstPageNumber)); }
        }

        public int FitToWidth
        {
            get => _fitToWidth;
            set { _fitToWidth = value; OnPropertyChanged(nameof(FitToWidth)); }
        }

        public int FitToHeight
        {
            get => _fitToHeight;
            set { _fitToHeight = value; OnPropertyChanged(nameof(FitToHeight)); }
        }

        public bool FitToPage
        {
            get => _fitToPage;
            set { _fitToPage = value; OnPropertyChanged(nameof(FitToPage)); }
        }

        public MarginSettings Margins
        {
            get => _margins;
            set { _margins = value; OnPropertyChanged(nameof(Margins)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MarginSettings
    {
        public double Top { get; set; } = 0.75;
        public double Bottom { get; set; } = 0.75;
        public double Left { get; set; } = 0.75;
        public double Right { get; set; } = 0.75;
        public double Header { get; set; } = 0.3;
        public double Footer { get; set; } = 0.3;
    }

    public class CustomProperty
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ZoomLevel
    {
        public int Scale { get; set; } = 100;
    }

    public class GridLineColor
    {
        public string Color { get; set; } = "#D0D0D0";
    }

    public enum CellType
    {
        General,
        Number,
        Integer,
        Currency,
        Accounting,
        Date,
        Time,
        DateTime,
        Percentage,
        Fraction,
        Scientific,
        Text,
        Boolean,
        Error,
        RichText
    }

    public enum HorizontalAlignment
    {
        General,
        Left,
        Center,
        Right,
        Fill,
        Justify,
        CenterContinuous,
        Distributed
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Justify,
        Distributed
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
        Surface,
        Combo
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
        Plus,
        Dash,
        Dot
    }

    public enum LineStyle
    {
        Solid,
        Dash,
        Dot,
        DashDot,
        DashDotDot,
        None
    }

    public enum AxisType
    {
        Category,
        Value,
        Series
    }

    public enum AxisPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public enum PivotFieldOrientation
    {
        RowField,
        ColumnField,
        DataField,
        PageField,
        Hidden
    }

    public enum SparklineType
    {
        Line,
        Column,
        Stacked
    }

    public enum HyperlinkType
    {
        Web,
        File,
        Email,
        NamedRange
    }

    public enum DataValidationType
    {
        AnyValue,
        WholeNumber,
        Decimal,
        List,
        Date,
        Time,
        TextLength,
        Custom
    }

    public enum ErrorStyle
    {
        Stop,
        Warning,
        Information
    }

    public enum ConditionalRuleType
    {
        CellValue,
        Expression,
        DataBar,
        ColorScale,
        IconSet,
        Top10,
        Unique,
        Duplicate
    }

    public enum ConditionalOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Between,
        NotBetween
    }

    public enum FilterType
    {
        None,
        AutoFilter,
        Custom,
        Top10,
        Dynamic,
        ColorFilter,
        IconFilter
    }

    public enum PageOrientation
    {
        Portrait,
        Landscape
    }

    public enum PaperSize
    {
        Letter,
        Legal,
        A4,
        A5,
        B5,
        C5,
        Envelope,
        Custom
    }

    public enum PageOrder
    {
        DownThenOver,
        OverThenDown
    }

    public enum RowOutlineLevel
    {
        None,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
        Level8
    }

    public enum ColumnOutlineLevel
    {
        None,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
        Level8
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OfficeSuite.Spreadsheet.Services
{
    public class SpreadsheetDocumentService : ISpreadsheetDocumentService
    {
        private readonly List<WorkbookInfo> _recentWorkbooks;
        private WorkbookInfo? _currentWorkbook;

        public SpreadsheetDocumentService()
        {
            _recentWorkbooks = new List<WorkbookInfo>();
        }

        public WorkbookInfo? CurrentWorkbook => _currentWorkbook;
        public IReadOnlyList<WorkbookInfo> RecentWorkbooks => _recentWorkbooks.AsReadOnly();

        public Task<WorkbookInfo> CreateAsync(string? fileName = null)
        {
            var workbook = new WorkbookInfo
            {
                FileName = fileName ?? "Untitled.xlsx",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            workbook.Sheets.Add(new SheetInfo { Name = "Sheet1", Index = 0 });
            workbook.Sheets.Add(new SheetInfo { Name = "Sheet2", Index = 1 });
            workbook.Sheets.Add(new SheetInfo { Name = "Sheet3", Index = 2 });

            _currentWorkbook = workbook;
            return Task.FromResult(workbook);
        }

        public Task<WorkbookInfo> OpenAsync(string filePath)
        {
            var workbook = new WorkbookInfo
            {
                FileName = System.IO.Path.GetFileName(filePath),
                FilePath = filePath,
                CreatedDate = System.IO.File.GetCreationTime(filePath),
                ModifiedDate = System.IO.File.GetLastWriteTime(filePath)
            };

            workbook.Sheets.Add(new SheetInfo { Name = "Sheet1", Index = 0 });

            _currentWorkbook = workbook;

            if (!_recentWorkbooks.Any(w => w.FilePath == filePath))
            {
                _recentWorkbooks.Insert(0, workbook);
                if (_recentWorkbooks.Count > 10)
                    _recentWorkbooks.RemoveAt(_recentWorkbooks.Count - 1);
            }

            return Task.FromResult(workbook);
        }

        public Task<bool> SaveAsync(WorkbookInfo workbook, string? filePath = null)
        {
            if (filePath != null)
            {
                workbook.FilePath = filePath;
                workbook.FileName = System.IO.Path.GetFileName(filePath);
            }

            workbook.ModifiedDate = DateTime.Now;
            return Task.FromResult(true);
        }

        public Task<bool> CloseAsync(WorkbookInfo workbook)
        {
            if (_currentWorkbook == workbook)
            {
                _currentWorkbook = null;
            }

            return Task.FromResult(true);
        }

        public Task<bool> ExportAsync(WorkbookInfo workbook, SpreadsheetExportFormat format, string filePath)
        {
            return Task.FromResult(true);
        }
    }

    public class WorkbookInfo : INotifyPropertyChanged
    {
        private string _fileName = "Untitled.xlsx";
        private string _filePath = string.Empty;
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;
        private string _author = string.Empty;
        private string _title = string.Empty;
        private string _subject = string.Empty;
        private string _keywords = string.Empty;
        private string _category = string.Empty;
        private string _comments = string.Empty;
        private bool _isModified;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SheetInfo> Sheets { get; } = new();
        public ObservableCollection<NamedRange> NamedRanges { get; } = new();
        public ObservableCollection<WorksheetProtection> SheetProtections { get; } = new();

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

    public class SheetInfo : INotifyPropertyChanged
    {
        private string _name = "Sheet";
        private int _index;
        private bool _isVisible = true;
        private bool _isSelected;
        private string _tabColor = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

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

    public class NamedRange : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _reference = string.Empty;
        private string _scope = "Workbook";
        private string _comment = string.Empty;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WorksheetProtection : INotifyChanged
    {
        public string SheetName { get; set; } = string.Empty;
        public bool IsProtected { get; set; }
        public string? Password { get; set; }
        public bool AllowSelectingLockedCells { get; set; } = true;
        public bool AllowSelectingUnlockedCells { get; set; } = true;
        public bool AllowFormattingCells { get; set; }
        public bool AllowFormattingColumns { get; set; }
        public bool AllowFormattingRows { get; set; }
        public bool AllowInsertingColumns { get; set; }
        public bool AllowInsertingRows { get; set; }
        public bool AllowInsertingHyperlinks { get; set; }
        public bool AllowDeletingColumns { get; set; }
        public bool AllowDeletingRows { get; set; }
        public bool AllowSorting { get; set; }
        public bool AllowFiltering { get; set; }
        public bool AllowUsingPivotTables { get; set; }
        public bool AllowEditingObjects { get; set; }
        public bool AllowEditingScenarios { get; set; }
    }

    public class CellModel : INotifyPropertyChanged
    {
        private string _address = "A1";
        private object? _value;
        private string _formula = string.Empty;
        private CellDataType _dataType = CellDataType.General;
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
        private HorizontalCellAlignment _horizontalAlignment = HorizontalCellAlignment.General;
        private VerticalCellAlignment _verticalAlignment = VerticalCellAlignment.Bottom;
        private int _textRotation;
        private bool _wrapText;
        private bool _shrinkToFit;
        private int _indentLevel;
        private bool _isLocked = true;
        private bool _isHidden;

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

        public CellDataType DataType
        {
            get => _dataType;
            set { _dataType = value; OnPropertyChanged(nameof(DataType)); }
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

        public HorizontalCellAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set { _horizontalAlignment = value; OnPropertyChanged(nameof(HorizontalAlignment)); }
        }

        public VerticalCellAlignment VerticalAlignment
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RowModel : INotifyPropertyChanged
    {
        private int _index;
        private double _height = 15;
        private bool _isHidden;
        private bool _isFiltered;
        private bool _isGrouped;
        private int _groupLevel;
        private bool _isSelected;
        private bool _isProtected;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ColumnModel : INotifyPropertyChanged
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
        private string _customWidth = string.Empty;

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

        public string CustomWidth
        {
            get => _customWidth;
            set { _customWidth = value; OnPropertyChanged(nameof(CustomWidth)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum CellDataType
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

    public enum HorizontalCellAlignment
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

    public enum VerticalCellAlignment
    {
        Top,
        Center,
        Bottom,
        Justify,
        Distributed
    }

    public enum SpreadsheetExportFormat
    {
        Xlsx,
        Xls,
        Csv,
        Pdf,
        Html,
        Tsv,
        Xml
    }

    public class SortOptions
    {
        public string Range { get; set; } = string.Empty;
        public List<SortKey> Keys { get; set; } = new();
        public bool HasHeader;
        public SortOrientation Orientation = SortOrientation.TopToBottom;
        public SortMethod Method = SortMethod.Normal;
    }

    public class SortKey
    {
        public int KeyIndex { get; set; }
        public SortOrder Order { get; set; } = SortOrder.Ascending;
        public CustomSortOption? CustomSort { get; set; }
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public enum SortOrientation
    {
        TopToBottom,
        LeftToRight
    }

    public enum SortMethod
    {
        Normal,
        PinYin,
        Stroke
    }

    public class CustomSortOption
    {
        public string Name { get; set; } = string.Empty;
        public List<string> CustomList { get; set; } = new();
    }

    public class FilterOptions
    {
        public string Range { get; set; } = string.Empty;
        public bool AutoFilter { get; set; } = true;
        public List<ColumnFilter> ColumnFilters { get; set; } = new();
    }

    public class ColumnFilter
    {
        public int ColumnIndex { get; set; }
        public FilterType Type { get; set; } = FilterType.None;
        public List<object?> Values { get; set; } = new();
        public string? Criteria1 { get; set; }
        public string? Criteria2 { get; set; }
        public bool ShowBlanks { get; set; }
        public bool ShowNonBlanks { get; set; }
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

    public class AutoFilterCriteria
    {
        public string Operator { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string? Formula { get; set; }
    }

    public interface ISpreadsheetDocumentService
    {
        Task<WorkbookInfo> CreateAsync(string? fileName = null);
        Task<WorkbookInfo> OpenAsync(string filePath);
        Task<bool> SaveAsync(WorkbookInfo workbook, string? filePath = null);
        Task<bool> CloseAsync(WorkbookInfo workbook);
        Task<bool> ExportAsync(WorkbookInfo workbook, SpreadsheetExportFormat format, string filePath);
    }

    public interface INotifyChanged : INotifyPropertyChanged
    {
    }
}

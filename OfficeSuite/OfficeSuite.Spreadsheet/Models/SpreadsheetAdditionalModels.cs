using System;
using System.Collections.Generic;
using OfficeSuite.Core.Models;

namespace OfficeSuite.Spreadsheet.Models
{
    public class NamedRangeCollection : Dictionary<string, NamedRange> { }

    public class NamedRange : ModelBase
    {
        private string _name;
        private CellRange _range;
        private string _comment;
        private string _category;
        private bool _isVisible;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public CellRange Range
        {
            get => _range;
            set => SetProperty(ref _range, value);
        }

        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public NamedRange()
        {
            _isVisible = true;
        }
    }

    public class RowColumnCollection : ModelBase
    {
        private Dictionary<int, RowColumnInfo> _items;
        private bool _isRows;
        private bool _isDefaultSize;

        public Dictionary<int, RowColumnInfo> Items
        {
            get => _items ??= new Dictionary<int, RowColumnInfo>();
            set => SetProperty(ref _items, value);
        }

        public bool IsRows
        {
            get => _isRows;
            set => SetProperty(ref _isRows, value);
        }

        public bool IsDefaultSize
        {
            get => _isDefaultSize;
            set => SetProperty(ref _isDefaultSize, value);
        }

        public RowColumnCollection()
        {
            _items = new Dictionary<int, RowColumnInfo>();
            _isDefaultSize = true;
        }

        public RowColumnInfo GetInfo(int index)
        {
            if (Items.TryGetValue(index, out var info))
                return info;
            return null;
        }

        public void SetSize(int index, double size)
        {
            if (!Items.ContainsKey(index))
                Items[index] = new RowColumnInfo { Index = index };
            Items[index].Size = size;
            Items[index].IsCustomSize = true;
        }

        public void SetHidden(int index, bool hidden)
        {
            if (!Items.ContainsKey(index))
                Items[index] = new RowColumnInfo { Index = index };
            Items[index].IsHidden = hidden;
        }
    }

    public class RowColumnInfo : ModelBase
    {
        private int _index;
        private double _size;
        private bool _isHidden;
        private bool _isCustomSize;
        private bool _isFiltered;
        private int _outlineLevel;

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public double Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public bool IsHidden
        {
            get => _isHidden;
            set => SetProperty(ref _isHidden, value);
        }

        public bool IsCustomSize
        {
            get => _isCustomSize;
            set => SetProperty(ref _isCustomSize, value);
        }

        public bool IsFiltered
        {
            get => _isFiltered;
            set => SetProperty(ref _isFiltered, value);
        }

        public int OutlineLevel
        {
            get => _outlineLevel;
            set => SetProperty(ref _outlineLevel, value);
        }

        public RowColumnInfo()
        {
            _size = IsRows ? 15 : 8.43;
        }
    }

    public class MergedCellCollection : List<CellRange> { }

    public class ViewSettings : ModelBase
    {
        private bool _isGridLinesVisible;
        private bool _isRowColumnHeadersVisible;
        private bool _isFormulasVisible;
        private bool _isFrozen;
        private string _selectedRange;
        private int _horizontalScrollPosition;
        private int _verticalScrollPosition;
        private ZoomLevel _zoomLevel;

        public bool IsGridLinesVisible
        {
            get => _isGridLinesVisible;
            set => SetProperty(ref _isGridLinesVisible, value);
        }

        public bool IsRowColumnHeadersVisible
        {
            get => _isRowColumnHeadersVisible;
            set => SetProperty(ref _isRowColumnHeadersVisible, value);
        }

        public bool IsFormulasVisible
        {
            get => _isFormulasVisible;
            set => SetProperty(ref _isFormulasVisible, value);
        }

        public bool IsFrozen
        {
            get => _isFrozen;
            set => SetProperty(ref _isFrozen, value);
        }

        public string SelectedRange
        {
            get => _selectedRange;
            set => SetProperty(ref _selectedRange, value);
        }

        public int HorizontalScrollPosition
        {
            get => _horizontalScrollPosition;
            set => SetProperty(ref _horizontalScrollPosition, value);
        }

        public int VerticalScrollPosition
        {
            get => _verticalScrollPosition;
            set => SetProperty(ref _verticalScrollPosition, value);
        }

        public ZoomLevel ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public ViewSettings()
        {
            _isGridLinesVisible = true;
            _isRowColumnHeadersVisible = true;
            _isFormulasVisible = false;
            _isFrozen = false;
            _zoomLevel = new ZoomLevel { Percentage = 100 };
        }
    }

    public class ZoomLevel : ModelBase
    {
        private int _percentage;
        private bool _isCustom;

        public int Percentage
        {
            get => _percentage;
            set => SetProperty(ref _percentage, Math.Max(10, Math.Min(400, value)));
        }

        public bool IsCustom
        {
            get => _isCustom;
            set => SetProperty(ref _isCustom, value);
        }
    }

    public class FreezePane : ModelBase
    {
        private int _row;
        private int _column;
        private int _bottomRow;
        private int _rightColumn;
        private bool _isActive;

        public int Row
        {
            get => _row;
            set => SetProperty(ref _row, value);
        }

        public int Column
        {
            get => _column;
            set => SetProperty(ref _column, value);
        }

        public int BottomRow
        {
            get => _bottomRow;
            set => SetProperty(ref _bottomRow, value);
        }

        public int RightColumn
        {
            get => _rightColumn;
            set => SetProperty(ref _rightColumn, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
    }

    public class Protection : ModelBase
    {
        private bool _isProtected;
        private string _password;
        private bool _isPasswordHashed;
        private bool _allowFormattingCells;
        private bool _allowFormattingColumns;
        private bool _allowFormattingRows;
        private bool _allowInsertingColumns;
        private bool _allowInsertingRows;
        private bool _allowInsertingHyperlinks;
        private bool _allowDeletingColumns;
        private bool _allowDeletingRows;
        private bool _allowSelectingLockedCells;
        private bool _allowSelectingUnlockedCells;
        private bool _allowSorting;
        private bool _allowFiltering;
        private bool _allowUsingPivotTables;

        public bool IsProtected
        {
            get => _isProtected;
            set => SetProperty(ref _isProtected, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsPasswordHashed
        {
            get => _isPasswordHashed;
            set => SetProperty(ref _isPasswordHashed, value);
        }

        public bool AllowFormattingCells
        {
            get => _allowFormattingCells;
            set => SetProperty(ref _allowFormattingCells, value);
        }

        public bool AllowFormattingColumns
        {
            get => _allowFormattingColumns;
            set => SetProperty(ref _allowFormattingColumns, value);
        }

        public bool AllowFormattingRows
        {
            get => _allowFormattingRows;
            set => SetProperty(ref _allowFormattingRows, value);
        }

        public bool AllowInsertingColumns
        {
            get => _allowInsertingColumns;
            set => SetProperty(ref _allowInsertingColumns, value);
        }

        public bool AllowInsertingRows
        {
            get => _allowInsertingRows;
            set => SetProperty(ref _allowInsertingRows, value);
        }

        public bool AllowInsertingHyperlinks
        {
            get => _allowInsertingHyperlinks;
            set => SetProperty(ref _allowInsertingHyperlinks, value);
        }

        public bool AllowDeletingColumns
        {
            get => _allowDeletingColumns;
            set => SetProperty(ref _allowDeletingColumns, value);
        }

        public bool AllowDeletingRows
        {
            get => _allowDeletingRows;
            set => SetProperty(ref _allowDeletingRows, value);
        }

        public bool AllowSelectingLockedCells
        {
            get => _allowSelectingLockedCells;
            set => SetProperty(ref _allowSelectingLockedCells, value);
        }

        public bool AllowSelectingUnlockedCells
        {
            get => _allowSelectingUnlockedCells;
            set => SetProperty(ref _allowSelectingUnlockedCells, value);
        }

        public bool AllowSorting
        {
            get => _allowSorting;
            set => SetProperty(ref _allowSorting, value);
        }

        public bool AllowFiltering
        {
            get => _allowFiltering;
            set => SetProperty(ref _allowFiltering, value);
        }

        public bool AllowUsingPivotTables
        {
            get => _allowUsingPivotTables;
            set => SetProperty(ref _allowUsingPivotTables, value);
        }

        public Protection()
        {
            _allowSelectingUnlockedCells = true;
        }
    }

    public class DocumentProperties : ModelBase
    {
        private string _title;
        private string _subject;
        private string _author;
        private string _keywords;
        private string _category;
        private string _comments;
        private string _lastSavedBy;
        private DateTime _createdDate;
        private DateTime _modifiedDate;
        private DateTime _lastPrintedDate;
        private int _revision;
        private int _totalEditingTime;
        private bool _isLocked;
        private bool _isFinal;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Keywords
        {
            get => _keywords;
            set => SetProperty(ref _keywords, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        public string LastSavedBy
        {
            get => _lastSavedBy;
            set => SetProperty(ref _lastSavedBy, value);
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set => SetProperty(ref _modifiedDate, value);
        }

        public DateTime LastPrintedDate
        {
            get => _lastPrintedDate;
            set => SetProperty(ref _lastPrintedDate, value);
        }

        public int Revision
        {
            get => _revision;
            set => SetProperty(ref _revision, value);
        }

        public int TotalEditingTime
        {
            get => _totalEditingTime;
            set => SetProperty(ref _totalEditingTime, value);
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }

        public bool IsFinal
        {
            get => _isFinal;
            set => SetProperty(ref _isFinal, value);
        }

        public DocumentProperties()
        {
            _createdDate = DateTime.Now;
            _modifiedDate = DateTime.Now;
        }
    }

    public class StyleCollection : ModelBase
    {
        private List<CellStyle> _styles;
        private CellStyle _defaultStyle;

        public List<CellStyle> Styles
        {
            get => _styles ??= new List<CellStyle>();
            set => SetProperty(ref _styles, value);
        }

        public CellStyle DefaultStyle
        {
            get => _defaultStyle ??= new CellStyle { Name = "Normal" };
            set => SetProperty(ref _defaultStyle, value);
        }

        public StyleCollection()
        {
            _styles = new List<CellStyle>();
            InitializeDefaultStyles();
        }

        private void InitializeDefaultStyles()
        {
            DefaultStyle = new CellStyle { Name = "Normal" };
            Styles.Add(DefaultStyle);
            Styles.Add(new CellStyle { Name = "Good", Format = CreateGoodFormat() });
            Styles.Add(new CellStyle { Name = "Bad", Format = CreateBadFormat() });
            Styles.Add(new CellStyle { Name = "Neutral", Format = CreateNeutralFormat() });
            Styles.Add(new CellStyle { Name = "Input", Format = CreateInputFormat() });
            Styles.Add(new CellStyle { Name = "Output", Format = CreateOutputFormat() });
            Styles.Add(new CellStyle { Name = "Calculation", Format = CreateCalculationFormat() });
            Styles.Add(new CellStyle { Name = "CheckCell", Format = CreateCheckCellFormat() });
        }

        private CellFormat CreateGoodFormat()
        {
            return new CellFormat
            {
                FontColor = System.Windows.Media.Color.FromRgb(0, 97, 0),
                BackgroundColor = System.Windows.Media.Color.FromRgb(198, 239, 206)
            };
        }

        private CellFormat CreateBadFormat()
        {
            return new CellFormat
            {
                FontColor = System.Windows.Media.Color.FromRgb(156, 0, 6),
                BackgroundColor = System.Windows.Media.Color.FromRgb(255, 199, 206)
            };
        }

        private CellFormat CreateNeutralFormat()
        {
            return new CellFormat
            {
                FontColor = System.Windows.Media.Color.FromRgb(153, 153, 0),
                BackgroundColor = System.Windows.Media.Color.FromRgb(255, 235, 156)
            };
        }

        private CellFormat CreateInputFormat()
        {
            return new CellFormat
            {
                BackgroundColor = System.Windows.Media.Color.FromRgb(254, 242, 204)
            };
        }

        private CellFormat CreateOutputFormat()
        {
            return new CellFormat
            {
                BackgroundColor = System.Windows.Media.Color.FromRgb(217, 225, 242)
            };
        }

        private CellFormat CreateCalculationFormat()
        {
            return new CellFormat
            {
                BackgroundColor = System.Windows.Media.Color.FromRgb(217, 225, 242)
            };
        }

        private CellFormat CreateCheckCellFormat()
        {
            return new CellFormat
            {
                BackgroundColor = System.Windows.Media.Color.FromRgb(217, 225, 242)
            };
        }
    }

    public class CellStyle : ModelBase
    {
        private string _name;
        private CellFormat _format;
        private bool _isBuiltIn;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public CellFormat Format
        {
            get => _format ??= new CellFormat();
            set => SetProperty(ref _format, value);
        }

        public bool IsBuiltIn
        {
            get => _isBuiltIn;
            set => SetProperty(ref _isBuiltIn, value);
        }
    }
}

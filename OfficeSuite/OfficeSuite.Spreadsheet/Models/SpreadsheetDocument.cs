using System;
using System.Collections.Generic;
using OfficeSuite.Core.Models;

namespace OfficeSuite.Spreadsheet.Models
{
    public class SpreadsheetDocument : ModelBase
    {
        private string _id;
        private string _title;
        private List<Worksheet> _worksheets;
        private Worksheet _activeWorksheet;
        private DocumentProperties _properties;
        private NamedRangeCollection _namedRanges;
        private StyleCollection _styles;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public List<Worksheet> Worksheets
        {
            get => _worksheets ??= new List<Worksheet>();
            set => SetProperty(ref _worksheets, value);
        }

        public Worksheet ActiveWorksheet
        {
            get => _activeWorksheet;
            set => SetProperty(ref _activeWorksheet, value);
        }

        public DocumentProperties Properties
        {
            get => _properties ??= new DocumentProperties();
            set => SetProperty(ref _properties, value);
        }

        public NamedRangeCollection NamedRanges
        {
            get => _namedRanges ??= new NamedRangeCollection();
            set => SetProperty(ref _namedRanges, value);
        }

        public StyleCollection Styles
        {
            get => _styles ??= new StyleCollection();
            set => SetProperty(ref _styles, value);
        }

        public SpreadsheetDocument()
        {
            _id = Guid.NewGuid().ToString();
            _worksheets = new List<Worksheet>();
            _properties = new DocumentProperties();
            _namedRanges = new NamedRangeCollection();
            _styles = new StyleCollection();
            AddWorksheet("Sheet1");
        }

        public Worksheet AddWorksheet(string name)
        {
            var worksheet = new Worksheet { Name = name };
            Worksheets.Add(worksheet);
            if (ActiveWorksheet == null)
                ActiveWorksheet = worksheet;
            return worksheet;
        }

        public void RemoveWorksheet(Worksheet worksheet)
        {
            if (Worksheets.Contains(worksheet))
            {
                Worksheets.Remove(worksheet);
                if (ActiveWorksheet == worksheet)
                    ActiveWorksheet = Worksheets.Count > 0 ? Worksheets[0] : null;
            }
        }

        public Worksheet GetWorksheet(string name)
        {
            return Worksheets.Find(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class Worksheet : ModelBase
    {
        private string _id;
        private string _name;
        private CellRange _usedRange;
        private CellCollection _cells;
        private RowColumnCollection _rows;
        private RowColumnCollection _columns;
        private MergedCellCollection _mergedCells;
        private ViewSettings _viewSettings;
        private FreezePane _freezePane;
        private Protection _protection;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public CellRange UsedRange
        {
            get => _usedRange ??= new CellRange();
            set => SetProperty(ref _usedRange, value);
        }

        public CellCollection Cells
        {
            get => _cells ??= new CellCollection();
            set => SetProperty(ref _cells, value);
        }

        public RowColumnCollection Rows
        {
            get => _rows ??= new RowColumnCollection { IsRows = true };
            set => SetProperty(ref _rows, value);
        }

        public RowColumnCollection Columns
        {
            get => _columns ??= new RowColumnCollection { IsRows = false };
            set => SetProperty(ref _columns, value);
        }

        public MergedCellCollection MergedCells
        {
            get => _mergedCells ??= new MergedCellCollection();
            set => SetProperty(ref _mergedCells, value);
        }

        public ViewSettings ViewSettings
        {
            get => _viewSettings ??= new ViewSettings();
            set => SetProperty(ref _viewSettings, value);
        }

        public FreezePane FreezePane
        {
            get => _freezePane ??= new FreezePane();
            set => SetProperty(ref _freezePane, value);
        }

        public Protection Protection
        {
            get => _protection ??= new Protection();
            set => SetProperty(ref _protection, value);
        }

        public Worksheet()
        {
            _id = Guid.NewGuid().ToString();
            _cells = new CellCollection();
            _mergedCells = new MergedCellCollection();
            _viewSettings = new ViewSettings();
            _freezePane = new FreezePane();
            _protection = new Protection();
        }

        public Cell GetCell(int row, int column)
        {
            var key = CellReference.GetKey(row, column);
            if (Cells.ContainsKey(key))
                return Cells[key];
            return null;
        }

        public Cell GetOrCreateCell(int row, int column)
        {
            var key = CellReference.GetKey(row, column);
            if (!Cells.ContainsKey(key))
            {
                var cell = new Cell { Row = row, Column = column };
                Cells[key] = cell;
            }
            return Cells[key];
        }

        public void SetCellValue(int row, int column, object value)
        {
            var cell = GetOrCreateCell(row, column);
            cell.Value = value;
            UpdateUsedRange(row, column);
        }

        private void UpdateUsedRange(int row, int column)
        {
            if (UsedRange.StartRow > row) UsedRange.StartRow = row;
            if (UsedRange.EndRow < row) UsedRange.EndRow = row;
            if (UsedRange.StartColumn > column) UsedRange.StartColumn = column;
            if (UsedRange.EndColumn < column) UsedRange.EndColumn = column;
        }
    }

    public class Cell : ModelBase
    {
        private string _id;
        private int _row;
        private int _column;
        private object _value;
        private CellValueType _valueType;
        private CellFormat _format;
        private string _formula;
        private string _comment;
        private bool _isLocked;
        private bool _isHidden;
        private Hyperlink _hyperlink;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

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

        public object Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
                UpdateValueType();
            }
        }

        public CellValueType ValueType
        {
            get => _valueType;
            set => SetProperty(ref _valueType, value);
        }

        public CellFormat Format
        {
            get => _format ??= new CellFormat();
            set => SetProperty(ref _format, value);
        }

        public string Formula
        {
            get => _formula;
            set
            {
                SetProperty(ref _formula, value);
                if (!string.IsNullOrEmpty(value) && value.StartsWith("="))
                    ValueType = CellValueType.Formula;
            }
        }

        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }

        public bool IsHidden
        {
            get => _isHidden;
            set => SetProperty(ref _isHidden, value);
        }

        public Hyperlink Hyperlink
        {
            get => _hyperlink;
            set => SetProperty(ref _hyperlink, value);
        }

        public Cell()
        {
            _id = Guid.NewGuid().ToString();
            _valueType = CellValueType.Empty;
            _format = new CellFormat();
            _isLocked = true;
        }

        private void UpdateValueType()
        {
            if (_value == null)
                ValueType = CellValueType.Empty;
            else if (_value is string)
                ValueType = CellValueType.Text;
            else if (_value is double || _value is int || _value is decimal)
                ValueType = CellValueType.Number;
            else if (_value is bool)
                ValueType = CellValueType.Boolean;
            else if (_value is DateTime)
                ValueType = CellValueType.DateTime;
            else
                ValueType = CellValueType.Text;
        }

        public string GetDisplayText()
        {
            if (_value == null) return string.Empty;
            return _value.ToString();
        }

        public double GetNumericValue()
        {
            if (_value is double d) return d;
            if (_value is int i) return i;
            if (_value is decimal dec) return (double)dec;
            if (double.TryParse(_value.ToString(), out double result)) return result;
            return 0;
        }
    }

    public enum CellValueType
    {
        Empty,
        Text,
        Number,
        Boolean,
        DateTime,
        Error,
        Formula
    }

    public class CellFormat : ModelBase
    {
        private string _numberFormat;
        private System.Windows.Media.Color _fontColor;
        private string _fontFamily;
        private double _fontSize;
        private bool _bold;
        private bool _italic;
        private bool _underline;
        private System.Windows.Media.Color _backgroundColor;
        private string _patternType;
        private System.Windows.HorizontalAlignment _horizontalAlignment;
        private System.Windows.VerticalAlignment _verticalAlignment;
        private bool _wrapText;
        private bool _shrinkToFit;
        private int _textRotation;
        private BorderFormat _border;

        public string NumberFormat
        {
            get => _numberFormat;
            set => SetProperty(ref _numberFormat, value);
        }

        public System.Windows.Media.Color FontColor
        {
            get => _fontColor;
            set => SetProperty(ref _fontColor, value);
        }

        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public bool Bold
        {
            get => _bold;
            set => SetProperty(ref _bold, value);
        }

        public bool Italic
        {
            get => _italic;
            set => SetProperty(ref _italic, value);
        }

        public bool Underline
        {
            get => _underline;
            set => SetProperty(ref _underline, value);
        }

        public System.Windows.Media.Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public string PatternType
        {
            get => _patternType;
            set => SetProperty(ref _patternType, value);
        }

        public System.Windows.HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set => SetProperty(ref _horizontalAlignment, value);
        }

        public System.Windows.VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set => SetProperty(ref _verticalAlignment, value);
        }

        public bool WrapText
        {
            get => _wrapText;
            set => SetProperty(ref _wrapText, value);
        }

        public bool ShrinkToFit
        {
            get => _shrinkToFit;
            set => SetProperty(ref _shrinkToFit, value);
        }

        public int TextRotation
        {
            get => _textRotation;
            set => SetProperty(ref _textRotation, value);
        }

        public BorderFormat Border
        {
            get => _border ??= new BorderFormat();
            set => SetProperty(ref _border, value);
        }

        public CellFormat()
        {
            _fontColor = System.Windows.Media.Colors.Black;
            _fontFamily = "Calibri";
            _fontSize = 11;
            _backgroundColor = System.Windows.Media.Colors.Transparent;
            _horizontalAlignment = System.Windows.HorizontalAlignment.Left;
            _verticalAlignment = System.Windows.VerticalAlignment.Bottom;
            _border = new BorderFormat();
        }
    }

    public class BorderFormat : ModelBase
    {
        private BorderLineStyle _leftStyle;
        private System.Windows.Media.Color _leftColor;
        private BorderLineStyle _rightStyle;
        private System.Windows.Media.Color _rightColor;
        private BorderLineStyle _topStyle;
        private System.Windows.Media.Color _topColor;
        private BorderLineStyle _bottomStyle;
        private System.Windows.Media.Color _bottomColor;
        private BorderLineStyle _diagonalStyle;
        private System.Windows.Media.Color _diagonalColor;
        private bool _diagonalDown;
        private bool _diagonalUp;

        public BorderLineStyle LeftStyle
        {
            get => _leftStyle;
            set => SetProperty(ref _leftStyle, value);
        }

        public System.Windows.Media.Color LeftColor
        {
            get => _leftColor;
            set => SetProperty(ref _leftColor, value);
        }

        public BorderLineStyle RightStyle
        {
            get => _rightStyle;
            set => SetProperty(ref _rightStyle, value);
        }

        public System.Windows.Media.Color RightColor
        {
            get => _rightColor;
            set => SetProperty(ref _rightColor, value);
        }

        public BorderLineStyle TopStyle
        {
            get => _topStyle;
            set => SetProperty(ref _topStyle, value);
        }

        public System.Windows.Media.Color TopColor
        {
            get => _topColor;
            set => SetProperty(ref _topColor, value);
        }

        public BorderLineStyle BottomStyle
        {
            get => _bottomStyle;
            set => SetProperty(ref _bottomStyle, value);
        }

        public System.Windows.Media.Color BottomColor
        {
            get => _bottomColor;
            set => SetProperty(ref _bottomColor, value);
        }

        public BorderLineStyle DiagonalStyle
        {
            get => _diagonalStyle;
            set => SetProperty(ref _diagonalStyle, value);
        }

        public System.Windows.Media.Color DiagonalColor
        {
            get => _diagonalColor;
            set => SetProperty(ref _diagonalColor, value);
        }

        public bool DiagonalDown
        {
            get => _diagonalDown;
            set => SetProperty(ref _diagonalDown, value);
        }

        public bool DiagonalUp
        {
            get => _diagonalUp;
            set => SetProperty(ref _diagonalUp, value);
        }

        public BorderFormat()
        {
            _leftStyle = BorderLineStyle.None;
            _rightStyle = BorderLineStyle.None;
            _topStyle = BorderLineStyle.None;
            _bottomStyle = BorderLineStyle.None;
            _diagonalStyle = BorderLineStyle.None;
            _leftColor = System.Windows.Media.Colors.Black;
            _rightColor = System.Windows.Media.Colors.Black;
            _topColor = System.Windows.Media.Colors.Black;
            _bottomColor = System.Windows.Media.Colors.Black;
            _diagonalColor = System.Windows.Media.Colors.Black;
        }
    }

    public enum BorderLineStyle
    {
        None,
        Thin,
        Medium,
        Thick,
        Double,
        Dotted,
        Dashed,
        MediumDashed,
        ThinDashDotted,
        MediumDashDotted,
        ThinDashDotDot,
        MediumDashDotDot,
        SlantedDashDot
    }

    public class CellCollection : Dictionary<string, Cell> { }

    public class CellReference
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string AbsoluteRow { get; set; }
        public string AbsoluteColumn { get; set; }

        public static string GetKey(int row, int column)
        {
            return $"{row},{column}";
        }

        public static CellReference Parse(string reference)
        {
            var result = new CellReference();
            var columnPart = string.Empty;
            var rowPart = string.Empty;

            foreach (char c in reference)
            {
                if (char.IsLetter(c))
                    columnPart += c;
                else if (char.IsDigit(c))
                    rowPart += c;
            }

            result.Column = ColumnNameToIndex(columnPart);
            result.Row = int.Parse(rowPart) - 1;
            return result;
        }

        public static string ToReference(int row, int column, bool absoluteRow = false, bool absoluteColumn = false)
        {
            var colRef = IndexToColumnName(column);
            if (absoluteColumn) colRef = "$" + colRef;
            if (absoluteRow) return $"{colRef}${row + 1}";
            return $"{colRef}{row + 1}";
        }

        public static int ColumnNameToIndex(string columnName)
        {
            int result = 0;
            foreach (char c in columnName.ToUpper())
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result - 1;
        }

        public static string IndexToColumnName(int index)
        {
            string result = string.Empty;
            index++;
            while (index > 0)
            {
                index--;
                result = (char)('A' + index % 26) + result;
                index /= 26;
            }
            return result;
        }
    }

    public class CellRange : ModelBase
    {
        private int _startRow;
        private int _startColumn;
        private int _endRow;
        private int _endColumn;
        private string _worksheetName;

        public int StartRow
        {
            get => _startRow;
            set => SetProperty(ref _startRow, value);
        }

        public int StartColumn
        {
            get => _startColumn;
            set => SetProperty(ref _startColumn, value);
        }

        public int EndRow
        {
            get => _endRow;
            set => SetProperty(ref _endRow, value);
        }

        public int EndColumn
        {
            get => _endColumn;
            set => SetProperty(ref _endColumn, value);
        }

        public string WorksheetName
        {
            get => _worksheetName;
            set => SetProperty(ref _worksheetName, value);
        }

        public CellRange()
        {
            _startRow = 0;
            _startColumn = 0;
            _endRow = 0;
            _endColumn = 0;
        }

        public CellRange(int startRow, int startColumn, int endRow, int endColumn)
        {
            _startRow = startRow;
            _startColumn = startColumn;
            _endRow = endRow;
            _endColumn = endColumn;
        }

        public string GetReference()
        {
            var start = CellReference.ToReference(StartRow, StartColumn);
            var end = CellReference.ToReference(EndRow, EndColumn);
            if (!string.IsNullOrEmpty(WorksheetName))
                return $"{WorksheetName}!{start}:{end}";
            return $"{start}:{end}";
        }

        public bool Contains(int row, int column)
        {
            return row >= StartRow && row <= EndRow && column >= StartColumn && column <= EndColumn;
        }

        public int RowCount => EndRow - StartRow + 1;
        public int ColumnCount => EndColumn - StartColumn + 1;
    }

    public class Hyperlink : ModelBase
    {
        private string _id;
        private string _address;
        private string _tooltip;
        private HyperlinkType _type;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        public HyperlinkType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public Hyperlink()
        {
            _id = Guid.NewGuid().ToString();
        }
    }

    public enum HyperlinkType
    {
        URL,
        File,
        Email,
        Workbook,
        PlaceInDocument
    }
}

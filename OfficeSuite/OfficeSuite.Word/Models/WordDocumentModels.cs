using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Word.Models
{
    public class WordDocument : INotifyPropertyChanged
    {
        private string _title = "Untitled Document";
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

        public ObservableCollection<WordDocumentSection> Sections { get; } = new();
        public ObservableCollection<StyleDefinition> Styles { get; } = new();
        public ObservableCollection<Bookmark> Bookmarks { get; } = new();
        public ObservableCollection<Hyperlink> Hyperlinks { get; } = new();
        public ObservableCollection<TableDefinition> Tables { get; } = new();
        public ObservableCollection<Comment> Comments { get; } = new();
        public ObservableCollection<HeaderDefinition> Headers { get; } = new();
        public ObservableCollection<FooterDefinition> Footers { get; } = new();

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

        public string Comments_text
        {
            get => _comments;
            set { _comments = value; OnPropertyChanged(nameof(Comments_text)); }
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

    public class WordDocumentSection : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private int _startIndex;
        private int _endIndex;
        private PageSetup _pageSetup = new();
        private SectionProperties _properties = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public int StartIndex
        {
            get => _startIndex;
            set { _startIndex = value; OnPropertyChanged(nameof(StartIndex)); }
        }

        public int EndIndex
        {
            get => _endIndex;
            set { _endIndex = value; OnPropertyChanged(nameof(EndIndex)); }
        }

        public PageSetup PageSetup
        {
            get => _pageSetup;
            set { _pageSetup = value; OnPropertyChanged(nameof(PageSetup)); }
        }

        public SectionProperties Properties
        {
            get => _properties;
            set { _properties = value; OnPropertyChanged(nameof(Properties)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PageSetup : INotifyPropertyChanged
    {
        private double _pageWidth = 612;
        private double _pageHeight = 792;
        private double _leftMargin = 72;
        private double _rightMargin = 72;
        private double _topMargin = 72;
        private double _bottomMargin = 72;
        private double _headerMargin = 36;
        private double _footerMargin = 36;
        private PageOrientation _orientation = PageOrientation.Portrait;
        private PaperSize _paperSize = PaperSize.Letter;
        private bool _mirrorMargins;
        private bool _differentFirstPage;
        private bool _differentOddEvenPages;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public double LeftMargin
        {
            get => _leftMargin;
            set { _leftMargin = value; OnPropertyChanged(nameof(LeftMargin)); }
        }

        public double RightMargin
        {
            get => _rightMargin;
            set { _rightMargin = value; OnPropertyChanged(nameof(RightMargin)); }
        }

        public double TopMargin
        {
            get => _topMargin;
            set { _topMargin = value; OnPropertyChanged(nameof(TopMargin)); }
        }

        public double BottomMargin
        {
            get => _bottomMargin;
            set { _bottomMargin = value; OnPropertyChanged(nameof(BottomMargin)); }
        }

        public double HeaderMargin
        {
            get => _headerMargin;
            set { _headerMargin = value; OnPropertyChanged(nameof(HeaderMargin)); }
        }

        public double FooterMargin
        {
            get => _footerMargin;
            set { _footerMargin = value; OnPropertyChanged(nameof(FooterMargin)); }
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

        public bool MirrorMargins
        {
            get => _mirrorMargins;
            set { _mirrorMargins = value; OnPropertyChanged(nameof(MirrorMargins)); }
        }

        public bool DifferentFirstPage
        {
            get => _differentFirstPage;
            set { _differentFirstPage = value; OnPropertyChanged(nameof(DifferentFirstPage)); }
        }

        public bool DifferentOddEvenPages
        {
            get => _differentOddEvenPages;
            set { _differentOddEvenPages = value; OnPropertyChanged(nameof(DifferentOddEvenPages)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SectionProperties : INotifyPropertyChanged
    {
        private SectionType _type = SectionType.NextPage;
        private bool _hasTitlePage;
        private int _columns = 1;
        private string _columnSpacing = "Auto";
        private bool _equalWidth = true;
        private bool _lineBetweenColumns;
        private int _sectionStart = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SectionType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public bool HasTitlePage
        {
            get => _hasTitlePage;
            set { _hasTitlePage = value; OnPropertyChanged(nameof(HasTitlePage)); }
        }

        public int Columns
        {
            get => _columns;
            set { _columns = value; OnPropertyChanged(nameof(Columns)); }
        }

        public string ColumnSpacing
        {
            get => _columnSpacing;
            set { _columnSpacing = value; OnPropertyChanged(nameof(ColumnSpacing)); }
        }

        public bool EqualWidth
        {
            get => _equalWidth;
            set { _equalWidth = value; OnPropertyChanged(nameof(EqualWidth)); }
        }

        public bool LineBetweenColumns
        {
            get => _lineBetweenColumns;
            set { _lineBetweenColumns = value; OnPropertyChanged(nameof(LineBetweenColumns)); }
        }

        public int SectionStart
        {
            get => _sectionStart;
            set { _sectionStart = value; OnPropertyChanged(nameof(SectionStart)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StyleDefinition : INotifyPropertyChanged
    {
        private string _name = "Normal";
        private StyleType _type = StyleType.Paragraph;
        private string _baseStyle = string.Empty;
        private string _styleId = string.Empty;
        private bool _isDefault;
        private CharacterProperties _characterProperties = new();
        private ParagraphProperties _paragraphProperties = new();
        private TableProperties? _tableProperties;
        private ListLevelProperties? _listLevelProperties;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public StyleType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string BaseStyle
        {
            get => _baseStyle;
            set { _baseStyle = value; OnPropertyChanged(nameof(BaseStyle)); }
        }

        public string StyleId
        {
            get => _styleId;
            set { _styleId = value; OnPropertyChanged(nameof(StyleId)); }
        }

        public bool IsDefault
        {
            get => _isDefault;
            set { _isDefault = value; OnPropertyChanged(nameof(IsDefault)); }
        }

        public CharacterProperties CharacterProperties
        {
            get => _characterProperties;
            set { _characterProperties = value; OnPropertyChanged(nameof(CharacterProperties)); }
        }

        public ParagraphProperties ParagraphProperties
        {
            get => _paragraphProperties;
            set { _paragraphProperties = value; OnPropertyChanged(nameof(ParagraphProperties)); }
        }

        public TableProperties? TableProperties
        {
            get => _tableProperties;
            set { _tableProperties = value; OnPropertyChanged(nameof(TableProperties)); }
        }

        public ListLevelProperties? ListLevelProperties
        {
            get => _listLevelProperties;
            set { _listLevelProperties = value; OnPropertyChanged(nameof(ListLevelProperties)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CharacterProperties : INotifyPropertyChanged
    {
        private string _fontName = "Calibri";
        private double _fontSize = 11;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderline;
        private bool _isStrikethrough;
        private bool _isSubscript;
        private bool _isSuperscript;
        private string _textColor = "#000000";
        private string _highlightColor = "None";
        private string _language = "en-US";

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public string TextColor
        {
            get => _textColor;
            set { _textColor = value; OnPropertyChanged(nameof(TextColor)); }
        }

        public string HighlightColor
        {
            get => _highlightColor;
            set { _highlightColor = value; OnPropertyChanged(nameof(HighlightColor)); }
        }

        public string Language
        {
            get => _language;
            set { _language = value; OnPropertyChanged(nameof(Language)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ParagraphProperties : INotifyPropertyChanged
    {
        private int _leftIndent;
        private int _rightIndent;
        private int _firstLineIndent;
        private int _beforeSpacing;
        private int _afterSpacing;
        private int _lineSpacing;
        private double _lineSpacingMultiple = 1.0;
        private TextAlignmentType _alignment = TextAlignmentType.Left;
        private TextDirection _textDirection = TextDirection.LR;
        public bool KeepNext { get; set; }
        public bool KeepLines { get; set; }
        public bool PageBreakBefore { get; set; }
        public bool WidowControl { get; set; }
        public int OutlineLevel { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int LeftIndent
        {
            get => _leftIndent;
            set { _leftIndent = value; OnPropertyChanged(nameof(LeftIndent)); }
        }

        public int RightIndent
        {
            get => _rightIndent;
            set { _rightIndent = value; OnPropertyChanged(nameof(RightIndent)); }
        }

        public int FirstLineIndent
        {
            get => _firstLineIndent;
            set { _firstLineIndent = value; OnPropertyChanged(nameof(FirstLineIndent)); }
        }

        public int BeforeSpacing
        {
            get => _beforeSpacing;
            set { _beforeSpacing = value; OnPropertyChanged(nameof(BeforeSpacing)); }
        }

        public int AfterSpacing
        {
            get => _afterSpacing;
            set { _afterSpacing = value; OnPropertyChanged(nameof(AfterSpacing)); }
        }

        public int LineSpacing
        {
            get => _lineSpacing;
            set { _lineSpacing = value; OnPropertyChanged(nameof(LineSpacing)); }
        }

        public double LineSpacingMultiple
        {
            get => _lineSpacingMultiple;
            set { _lineSpacingMultiple = value; OnPropertyChanged(nameof(LineSpacingMultiple)); }
        }

        public TextAlignmentType Alignment
        {
            get => _alignment;
            set { _alignment = value; OnPropertyChanged(nameof(Alignment)); }
        }

        public TextDirection TextDirection
        {
            get => _textDirection;
            set { _textDirection = value; OnPropertyChanged(nameof(TextDirection)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TableProperties : INotifyPropertyChanged
    {
        private string _styleId = string.Empty;
        private double _width;
        private TableAlignmentType _alignment = TableAlignmentType.Left;
        private bool _isFloating;
        private TablePosition _position = new();
        private BorderProperties _borders = new();
        private ShadingProperties _shading = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string StyleId
        {
            get => _styleId;
            set { _styleId = value; OnPropertyChanged(nameof(StyleId)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
        }

        public TableAlignmentType Alignment
        {
            get => _alignment;
            set { _alignment = value; OnPropertyChanged(nameof(Alignment)); }
        }

        public bool IsFloating
        {
            get => _isFloating;
            set { _isFloating = value; OnPropertyChanged(nameof(IsFloating)); }
        }

        public TablePosition Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public BorderProperties Borders
        {
            get => _borders;
            set { _borders = value; OnPropertyChanged(nameof(Borders)); }
        }

        public ShadingProperties Shading
        {
            get => _shading;
            set { _shading = value; OnPropertyChanged(nameof(Shading)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TablePosition
    {
        public HorizontalPosition Horizontal { get; set; } = HorizontalPosition.Center;
        public double HorizontalOffset { get; set; }
        public VerticalPosition Vertical { get; set; } = VerticalPosition.Page;
        public double VerticalOffset { get; set; }
        public double RelativeFromH { get; set; } = 1;
        public double RelativeFromV { get; set; } = 1;
        public bool WrapText { get; set; } = true;
    }

    public class BorderProperties
    {
        public BorderDefinition Top { get; set; } = new();
        public BorderDefinition Bottom { get; set; } = new();
        public BorderDefinition Left { get; set; } = new();
        public BorderDefinition Right { get; set; } = new();
        public BorderDefinition InsideHorizontal { get; set; } = new();
        public BorderDefinition InsideVertical { get; set; } = new();
    }

    public class BorderDefinition
    {
        public string BorderStyle { get; set; } = "none";
        public double BorderWidth { get; set; }
        public string BorderColor { get; set; } = "#000000";
        public double Space { get; set; }
    }

    public class ShadingProperties
    {
        public ShadingPattern Pattern { get; set; } = ShadingPattern.Clear;
        public string FillColor { get; set; } = "#FFFFFF";
        public string PatternColor { get; set; } = "#000000";
    }

    public class ListLevelProperties : INotifyPropertyChanged
    {
        private int _level;
        private ListLevelType _type = ListLevelType.Bullet;
        private string _numberFormat = "decimal";
        private int _startValue = 1;
        private int _restartLevel;
        private string _levelText = "%1.";
        private int _legacy = -1;
        private bool _isLegalNumberingFormat;
        private BulletDefinition? _bulletDefinition;
        private PictureDefinition? _pictureDefinition;
        private ParagraphProperties _paragraphProperties = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Level
        {
            get => _level;
            set { _level = value; OnPropertyChanged(nameof(Level)); }
        }

        public ListLevelType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string NumberFormat
        {
            get => _numberFormat;
            set { _numberFormat = value; OnPropertyChanged(nameof(NumberFormat)); }
        }

        public int StartValue
        {
            get => _startValue;
            set { _startValue = value; OnPropertyChanged(nameof(StartValue)); }
        }

        public int RestartLevel
        {
            get => _restartLevel;
            set { _restartLevel = value; OnPropertyChanged(nameof(RestartLevel)); }
        }

        public string LevelText
        {
            get => _levelText;
            set { _levelText = value; OnPropertyChanged(nameof(LevelText)); }
        }

        public int Legacy
        {
            get => _legacy;
            set { _legacy = value; OnPropertyChanged(nameof(Legacy)); }
        }

        public bool IsLegalNumberingFormat
        {
            get => _isLegalNumberingFormat;
            set { _isLegalNumberingFormat = value; OnPropertyChanged(nameof(IsLegalNumberingFormat)); }
        }

        public BulletDefinition? BulletDefinition
        {
            get => _bulletDefinition;
            set { _bulletDefinition = value; OnPropertyChanged(nameof(BulletDefinition)); }
        }

        public PictureDefinition? PictureDefinition
        {
            get => _pictureDefinition;
            set { _pictureDefinition = value; OnPropertyChanged(nameof(PictureDefinition)); }
        }

        public ParagraphProperties ParagraphProperties
        {
            get => _paragraphProperties;
            set { _paragraphProperties = value; OnPropertyChanged(nameof(ParagraphProperties)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BulletDefinition
    {
        public string Character { get; set; } = "\u2022";
        public string FontName { get; set; } = "Arial";
        public string Color { get; set; } = "#000000";
        public double FontSize { get; set; } = 18;
    }

    public class PictureDefinition
    {
        public byte[]? ImageData { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class TableDefinition : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _styleId = string.Empty;
        private int _rows;
        private int _columns;
        private TableProperties _properties = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TableRowDefinition> Rows { get; } = new();

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string StyleId
        {
            get => _styleId;
            set { _styleId = value; OnPropertyChanged(nameof(StyleId)); }
        }

        public int Rows_count
        {
            get => _rows;
            set { _rows = value; OnPropertyChanged(nameof(Rows_count)); }
        }

        public int Columns
        {
            get => _columns;
            set { _columns = value; OnPropertyChanged(nameof(Columns)); }
        }

        public TableProperties Properties
        {
            get => _properties;
            set { _properties = value; OnPropertyChanged(nameof(Properties)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TableRowDefinition : INotifyPropertyChanged
    {
        private int _index;
        private int _height;
        private TableRowHeightType _heightType = TableRowHeightType.Auto;
        private bool _isHeaderRow;
        private bool _isTotalRow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TableCellDefinition> Cells { get; } = new();

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public int Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(nameof(Height)); }
        }

        public TableRowHeightType HeightType
        {
            get => _heightType;
            set { _heightType = value; OnPropertyChanged(nameof(HeightType)); }
        }

        public bool IsHeaderRow
        {
            get => _isHeaderRow;
            set { _isHeaderRow = value; OnPropertyChanged(nameof(IsHeaderRow)); }
        }

        public bool IsTotalRow
        {
            get => _isTotalRow;
            set { _isTotalRow = value; OnPropertyChanged(nameof(IsTotalRow)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TableCellDefinition : INotifyPropertyChanged
    {
        private int _index;
        private int _columnSpan = 1;
        private int _rowSpan = 1;
        private double _width;
        private CellVerticalAlignment _verticalAlignment = CellVerticalAlignment.Top;
        private CellMargins _margins = new();
        private ShadingProperties _shading = new();
        private BorderProperties _borders = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Content { get; set; } = string.Empty;
        public ParagraphProperties? ParagraphProperties { get; set; }

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public int ColumnSpan
        {
            get => _columnSpan;
            set { _columnSpan = value; OnPropertyChanged(nameof(ColumnSpan)); }
        }

        public int RowSpan
        {
            get => _rowSpan;
            set { _rowSpan = value; OnPropertyChanged(nameof(RowSpan)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
        }

        public CellVerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set { _verticalAlignment = value; OnPropertyChanged(nameof(VerticalAlignment)); }
        }

        public CellMargins Margins
        {
            get => _margins;
            set { _margins = value; OnPropertyChanged(nameof(Margins)); }
        }

        public ShadingProperties Shading
        {
            get => _shading;
            set { _shading = value; OnPropertyChanged(nameof(Shading)); }
        }

        public BorderProperties Borders
        {
            get => _borders;
            set { _borders = value; OnPropertyChanged(nameof(Borders)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CellMargins
    {
        public int Top { get; set; } = 0;
        public int Bottom { get; set; } = 0;
        public int Left { get; set; } = 108;
        public int Right { get; set; } = 108;
    }

    public class Bookmark : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private int _startIndex;
        private int _endIndex;
        private DateTime _createdDate = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public int StartIndex
        {
            get => _startIndex;
            set { _startIndex = value; OnPropertyChanged(nameof(StartIndex)); }
        }

        public int EndIndex
        {
            get => _endIndex;
            set { _endIndex = value; OnPropertyChanged(nameof(EndIndex)); }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { _createdDate = value; OnPropertyChanged(nameof(CreatedDate)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Hyperlink : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _url = string.Empty;
        private string _displayText = string.Empty;
        private int _startIndex;
        private int _endIndex;
        private HyperlinkType _type = HyperlinkType.Web;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
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

        public int StartIndex
        {
            get => _startIndex;
            set { _startIndex = value; OnPropertyChanged(nameof(StartIndex)); }
        }

        public int EndIndex
        {
            get => _endIndex;
            set { _endIndex = value; OnPropertyChanged(nameof(EndIndex)); }
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

    public class Comment : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _author = string.Empty;
        private string _authorInitials = string.Empty;
        private string _text = string.Empty;
        private int _startIndex;
        private int _endIndex;
        private DateTime _date = DateTime.Now;
        private string? _parentCommentId;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Author
        {
            get => _author;
            set { _author = value; OnPropertyChanged(nameof(Author)); }
        }

        public string AuthorInitials
        {
            get => _authorInitials;
            set { _authorInitials = value; OnPropertyChanged(nameof(AuthorInitials)); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        public int StartIndex
        {
            get => _startIndex;
            set { _startIndex = value; OnPropertyChanged(nameof(StartIndex)); }
        }

        public int EndIndex
        {
            get => _endIndex;
            set { _endIndex = value; OnPropertyChanged(nameof(EndIndex)); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }

        public string? ParentCommentId
        {
            get => _parentCommentId;
            set { _parentCommentId = value; OnPropertyChanged(nameof(ParentCommentId)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HeaderDefinition : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private HeaderFooterType _type = HeaderFooterType.Default;
        private int _sectionIndex;
        private string _content = string.Empty;
        private bool _isLinkedToPrevious;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public HeaderFooterType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public int SectionIndex
        {
            get => _sectionIndex;
            set { _sectionIndex = value; OnPropertyChanged(nameof(SectionIndex)); }
        }

        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        public bool IsLinkedToPrevious
        {
            get => _isLinkedToPrevious;
            set { _isLinkedToPrevious = value; OnPropertyChanged(nameof(IsLinkedToPrevious)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FooterDefinition : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private HeaderFooterType _type = HeaderFooterType.Default;
        private int _sectionIndex;
        private string _content = string.Empty;
        private bool _isLinkedToPrevious;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public HeaderFooterType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public int SectionIndex
        {
            get => _sectionIndex;
            set { _sectionIndex = value; OnPropertyChanged(nameof(SectionIndex)); }
        }

        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        public bool IsLinkedToPrevious
        {
            get => _isLinkedToPrevious;
            set { _isLinkedToPrevious = value; OnPropertyChanged(nameof(IsLinkedToPrevious)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

    public enum SectionType
    {
        NextPage,
        Continuous,
        SamePage,
        EvenPage,
        OddPage
    }

    public enum StyleType
    {
        Character,
        Paragraph,
        Table,
        List,
        Numbering
    }

    public enum TextAlignmentType
    {
        Left,
        Center,
        Right,
        Justify
    }

    public enum TextDirection
    {
        LR,
        RL,
        TB
    }

    public enum TableAlignmentType
    {
        Left,
        Center,
        Right
    }

    public enum HorizontalPosition
    {
        Margin,
        Page,
        Column
    }

    public enum VerticalPosition
    {
        Margin,
        Page,
        Paragraph
    }

    public enum TableRowHeightType
    {
        Auto,
        AtLeast,
        Exact
    }

    public enum CellVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public enum ShadingPattern
    {
        Clear,
        Solid,
        HorizontalStripe,
        VerticalStripe,
        DiagonalDown,
        DiagonalUp,
        HorizontalCross,
        VerticalCross,
        DiagonalCross
    }

    public enum ListLevelType
    {
        Bullet,
        Number,
        Picture,
        None
    }

    public enum HeaderFooterType
    {
        Default,
        First,
        Even
    }

    public enum HyperlinkType
    {
        Web,
        File,
        Email,
        Bookmark,
        Slide
    }
}

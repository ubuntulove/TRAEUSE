using System;
using System.Collections.Generic;

namespace OfficeSuite.Word.Models
{
    public class DocumentProperties : ModelBase
    {
        private string _title;
        private string _author;
        private string _subject;
        private string _keywords;
        private string _description;
        private DateTime _createdDate;
        private DateTime _modifiedDate;
        private DateTime _lastPrintedDate;
        private int _revision;
        private string _category;
        private string _manager;
        private string _company;
        private bool _isLocked;
        private Dictionary<string, string> _customProperties;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Keywords
        {
            get => _keywords;
            set => SetProperty(ref _keywords, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
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

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Manager
        {
            get => _manager;
            set => SetProperty(ref _manager, value);
        }

        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }

        public Dictionary<string, string> CustomProperties
        {
            get => _customProperties ??= new Dictionary<string, string>();
            set => SetProperty(ref _customProperties, value);
        }

        public DocumentProperties()
        {
            _createdDate = DateTime.Now;
            _modifiedDate = DateTime.Now;
            _revision = 1;
            _customProperties = new Dictionary<string, string>();
        }
    }

    public class PageSettings : ModelBase
    {
        private PageSize _size;
        private int _width;
        private int _height;
        private Orientation _orientation;
        private double _marginTop;
        private double _marginBottom;
        private double _marginLeft;
        private double _marginRight;
        private double _gutter;
        private bool _mirrorMargins;
        private int _pageCount;
        private string _paperSource;

        public PageSize Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public Orientation Orientation
        {
            get => _orientation;
            set => SetProperty(ref _orientation, value);
        }

        public double MarginTop
        {
            get => _marginTop;
            set => SetProperty(ref _marginTop, value);
        }

        public double MarginBottom
        {
            get => _marginBottom;
            set => SetProperty(ref _marginBottom, value);
        }

        public double MarginLeft
        {
            get => _marginLeft;
            set => SetProperty(ref _marginLeft, value);
        }

        public double MarginRight
        {
            get => _marginRight;
            set => SetProperty(ref _marginRight, value);
        }

        public double Gutter
        {
            get => _gutter;
            set => SetProperty(ref _gutter, value);
        }

        public bool MirrorMargins
        {
            get => _mirrorMargins;
            set => SetProperty(ref _mirrorMargins, value);
        }

        public int PageCount
        {
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
        }

        public string PaperSource
        {
            get => _paperSource;
            set => SetProperty(ref _paperSource, value);
        }

        public PageSettings()
        {
            _size = PageSize.A4;
            _width = 11906;
            _height = 16838;
            _orientation = Orientation.Portrait;
            _marginTop = 1440;
            _marginBottom = 1440;
            _marginLeft = 1440;
            _marginRight = 1440;
            _gutter = 0;
            _mirrorMargins = false;
            _pageCount = 1;
        }
    }

    public enum PageSize
    {
        Letter,
        A4,
        A5,
        B5,
        Legal,
        Executive,
        Custom
    }

    public enum Orientation
    {
        Portrait,
        Landscape
    }

    public class HeaderFooterCollection : ModelBase
    {
        private HeaderFooter _defaultHeader;
        private HeaderFooter _firstPageHeader;
        private HeaderFooter _defaultFooter;
        private HeaderFooter _firstPageFooter;
        private HeaderFooter _evenPagesHeader;
        private HeaderFooter _evenPagesFooter;
        private bool _differentFirstPage;
        private bool _differentOddEvenPages;

        public HeaderFooter DefaultHeader
        {
            get => _defaultHeader;
            set => SetProperty(ref _defaultHeader, value);
        }

        public HeaderFooter FirstPageHeader
        {
            get => _firstPageHeader;
            set => SetProperty(ref _firstPageHeader, value);
        }

        public HeaderFooter DefaultFooter
        {
            get => _defaultFooter;
            set => SetProperty(ref _defaultFooter, value);
        }

        public HeaderFooter FirstPageFooter
        {
            get => _firstPageFooter;
            set => SetProperty(ref _firstPageFooter, value);
        }

        public HeaderFooter EvenPagesHeader
        {
            get => _evenPagesHeader;
            set => SetProperty(ref _evenPagesHeader, value);
        }

        public HeaderFooter EvenPagesFooter
        {
            get => _evenPagesFooter;
            set => SetProperty(ref _evenPagesFooter, value);
        }

        public bool DifferentFirstPage
        {
            get => _differentFirstPage;
            set => SetProperty(ref _differentFirstPage, value);
        }

        public bool DifferentOddEvenPages
        {
            get => _differentOddEvenPages;
            set => SetProperty(ref _differentOddEvenPages, value);
        }
    }

    public class HeaderFooter : ModelBase
    {
        private string _id;
        private HeaderFooterType _type;
        private List<HeaderFooterItem> _items;
        private bool _isLinkedToPrevious;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public HeaderFooterType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public List<HeaderFooterItem> Items
        {
            get => _items ??= new List<HeaderFooterItem>();
            set => SetProperty(ref _items, value);
        }

        public bool IsLinkedToPrevious
        {
            get => _isLinkedToPrevious;
            set => SetProperty(ref _isLinkedToPrevious, value);
        }

        public HeaderFooter()
        {
            _id = Guid.NewGuid().ToString();
            _items = new List<HeaderFooterItem>();
        }
    }

    public enum HeaderFooterType
    {
        Header,
        Footer
    }

    public class HeaderFooterItem : ModelBase
    {
        private string _text;
        private HeaderFooterItemType _itemType;
        private Alignment _alignment;
        private CharacterFormat _format;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public HeaderFooterItemType ItemType
        {
            get => _itemType;
            set => SetProperty(ref _itemType, value);
        }

        public Alignment Alignment
        {
            get => _alignment;
            set => SetProperty(ref _alignment, value);
        }

        public CharacterFormat Format
        {
            get => _format ??= new CharacterFormat();
            set => SetProperty(ref _format, value);
        }
    }

    public enum HeaderFooterItemType
    {
        Text,
        PageNumber,
        NumberOfPages,
        Date,
        Time,
        FileName,
        Author
    }

    public enum Alignment
    {
        Left,
        Center,
        Right
    }

    public class StyleCollection : ModelBase
    {
        private List<Style> _styles;
        private Style _defaultParagraphStyle;
        private Style _defaultCharacterStyle;

        public List<Style> Styles
        {
            get => _styles ??= new List<Style>();
            set => SetProperty(ref _styles, value);
        }

        public Style DefaultParagraphStyle
        {
            get => _defaultParagraphStyle;
            set => SetProperty(ref _defaultParagraphStyle, value);
        }

        public Style DefaultCharacterStyle
        {
            get => _defaultCharacterStyle;
            set => SetProperty(ref _defaultCharacterStyle, value);
        }

        public StyleCollection()
        {
            _styles = new List<Style>();
            InitializeDefaultStyles();
        }

        private void InitializeDefaultStyles()
        {
            _defaultParagraphStyle = new Style
            {
                Name = "Normal",
                Type = StyleType.Paragraph,
                Format = new ParagraphFormat()
            };
            _defaultCharacterStyle = new Style
            {
                Name = "Default Paragraph Font",
                Type = StyleType.Character
            };
            _styles.Add(_defaultParagraphStyle);
            _styles.Add(_defaultCharacterStyle);
            _styles.Add(new Style { Name = "Heading 1", Type = StyleType.Paragraph, BasedOn = "Normal" });
            _styles.Add(new Style { Name = "Heading 2", Type = StyleType.Paragraph, BasedOn = "Normal" });
            _styles.Add(new Style { Name = "Heading 3", Type = StyleType.Paragraph, BasedOn = "Normal" });
        }
    }

    public class Style : ModelBase
    {
        private string _id;
        private string _name;
        private StyleType _type;
        private string _basedOn;
        private string _nextParagraphStyle;
        private bool _isDefault;
        private ParagraphFormat _format;
        private CharacterFormat _characterFormat;
        private string _styleId;

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

        public StyleType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string BasedOn
        {
            get => _basedOn;
            set => SetProperty(ref _basedOn, value);
        }

        public string NextParagraphStyle
        {
            get => _nextParagraphStyle;
            set => SetProperty(ref _nextParagraphStyle, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public ParagraphFormat Format
        {
            get => _format ??= new ParagraphFormat();
            set => SetProperty(ref _format, value);
        }

        public CharacterFormat CharacterFormat
        {
            get => _characterFormat ??= new CharacterFormat();
            set => SetProperty(ref _characterFormat, value);
        }

        public string StyleId
        {
            get => _styleId;
            set => SetProperty(ref _styleId, value);
        }

        public Style()
        {
            _id = Guid.NewGuid().ToString();
        }
    }

    public enum StyleType
    {
        Paragraph,
        Character,
        Table,
        List
    }
}

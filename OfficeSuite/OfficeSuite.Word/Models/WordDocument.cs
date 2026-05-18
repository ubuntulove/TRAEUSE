using System;
using System.Collections.Generic;
using OfficeSuite.Core.Models;

namespace OfficeSuite.Word.Models
{
    public class WordDocument : ModelBase
    {
        private string _id;
        private string _title;
        private List<Paragraph> _paragraphs;
        private DocumentProperties _properties;
        private PageSettings _pageSettings;
        private HeaderFooterCollection _headersFooters;
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

        public List<Paragraph> Paragraphs
        {
            get => _paragraphs ??= new List<Paragraph>();
            set => SetProperty(ref _paragraphs, value);
        }

        public DocumentProperties Properties
        {
            get => _properties ??= new DocumentProperties();
            set => SetProperty(ref _properties, value);
        }

        public PageSettings PageSettings
        {
            get => _pageSettings ??= new PageSettings();
            set => SetProperty(ref _pageSettings, value);
        }

        public HeaderFooterCollection HeadersFooters
        {
            get => _headersFooters ??= new HeaderFooterCollection();
            set => SetProperty(ref _headersFooters, value);
        }

        public StyleCollection Styles
        {
            get => _styles ??= new StyleCollection();
            set => SetProperty(ref _styles, value);
        }

        public WordDocument()
        {
            _id = Guid.NewGuid().ToString();
            _paragraphs = new List<Paragraph>();
            _properties = new DocumentProperties();
            _pageSettings = new PageSettings();
            _headersFooters = new HeaderFooterCollection();
            _styles = new StyleCollection();
        }
    }

    public class Paragraph : ModelBase
    {
        private string _id;
        private List<Run> _runs;
        private ParagraphFormat _format;
        private int _index;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public List<Run> Runs
        {
            get => _runs ??= new List<Run>();
            set => SetProperty(ref _runs, value);
        }

        public ParagraphFormat Format
        {
            get => _format ??= new ParagraphFormat();
            set => SetProperty(ref _format, value);
        }

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public Paragraph()
        {
            _id = Guid.NewGuid().ToString();
            _runs = new List<Run>();
            _format = new ParagraphFormat();
        }

        public string GetText()
        {
            return string.Join("", Runs.ConvertAll(r => r.Text));
        }
    }

    public class Run : ModelBase
    {
        private string _id;
        private string _text;
        private CharacterFormat _format;
        private int _startIndex;
        private int _length;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public CharacterFormat Format
        {
            get => _format ??= new CharacterFormat();
            set => SetProperty(ref _format, value);
        }

        public int StartIndex
        {
            get => _startIndex;
            set => SetProperty(ref _startIndex, value);
        }

        public int Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        public Run()
        {
            _id = Guid.NewGuid().ToString();
            _text = string.Empty;
            _format = new CharacterFormat();
        }
    }

    public class CharacterFormat : ModelBase
    {
        private string _fontFamily;
        private double _fontSize;
        private System.Windows.Media.Color _fontColor;
        private bool _bold;
        private bool _italic;
        private bool _underline;
        private bool _strikethrough;
        private bool _subscript;
        private bool _superscript;
        private HighlightColor _highlight;
        private string _language;

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

        public System.Windows.Media.Color FontColor
        {
            get => _fontColor;
            set => SetProperty(ref _fontColor, value);
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

        public bool Strikethrough
        {
            get => _strikethrough;
            set => SetProperty(ref _strikethrough, value);
        }

        public bool Subscript
        {
            get => _subscript;
            set => SetProperty(ref _subscript, value);
        }

        public bool Superscript
        {
            get => _superscript;
            set => SetProperty(ref _superscript, value);
        }

        public HighlightColor Highlight
        {
            get => _highlight;
            set => SetProperty(ref _highlight, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public CharacterFormat()
        {
            _fontFamily = "Calibri";
            _fontSize = 11;
            _fontColor = System.Windows.Media.Colors.Black;
            _highlight = HighlightColor.None;
            _language = "en-US";
        }
    }

    public enum HighlightColor
    {
        None,
        Yellow,
        Green,
        Blue,
        Red,
        Pink,
        Orange
    }

    public class ParagraphFormat : ModelBase
    {
        private TextAlignment _alignment;
        private double _lineSpacing;
        private double _spaceBefore;
        private double _spaceAfter;
        private double _leftIndent;
        private double _rightIndent;
        private double _firstLineIndent;
        private LineSpacingRule _lineSpacingRule;
        private bool _keepTogether;
        private bool _keepWithNext;
        private bool _pageBreakBefore;

        public TextAlignment Alignment
        {
            get => _alignment;
            set => SetProperty(ref _alignment, value);
        }

        public double LineSpacing
        {
            get => _lineSpacing;
            set => SetProperty(ref _lineSpacing, value);
        }

        public double SpaceBefore
        {
            get => _spaceBefore;
            set => SetProperty(ref _spaceBefore, value);
        }

        public double SpaceAfter
        {
            get => _spaceAfter;
            set => SetProperty(ref _spaceAfter, value);
        }

        public double LeftIndent
        {
            get => _leftIndent;
            set => SetProperty(ref _leftIndent, value);
        }

        public double RightIndent
        {
            get => _rightIndent;
            set => SetProperty(ref _rightIndent, value);
        }

        public double FirstLineIndent
        {
            get => _firstLineIndent;
            set => SetProperty(ref _firstLineIndent, value);
        }

        public LineSpacingRule LineSpacingRule
        {
            get => _lineSpacingRule;
            set => SetProperty(ref _lineSpacingRule, value);
        }

        public bool KeepTogether
        {
            get => _keepTogether;
            set => SetProperty(ref _keepTogether, value);
        }

        public bool KeepWithNext
        {
            get => _keepWithNext;
            set => SetProperty(ref _keepWithNext, value);
        }

        public bool PageBreakBefore
        {
            get => _pageBreakBefore;
            set => SetProperty(ref _pageBreakBefore, value);
        }

        public ParagraphFormat()
        {
            _alignment = TextAlignment.Left;
            _lineSpacing = 1.15;
            _spaceBefore = 0;
            _spaceAfter = 8;
            _lineSpacingRule = LineSpacingRule.Multiple;
        }
    }

    public enum LineSpacingRule
    {
        Single,
        OnePointFive,
        Double,
        AtLeast,
        Exactly,
        Multiple
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }
}

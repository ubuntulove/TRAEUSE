using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OfficeSuite.Word.Structure
{
    public class DocumentContent
    {
        public List<DocumentElement> Elements { get; set; } = new List<DocumentElement>();
        public DocumentProperties Properties { get; set; } = new DocumentProperties();
        public SectionSettings CurrentSection { get; set; } = new SectionSettings();
        public Dictionary<string, Bookmark> Bookmarks { get; set; } = new Dictionary<string, Bookmark>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Revision> Revisions { get; set; } = new List<Revision>();
        public HeaderFooterCollection HeadersFooters { get; set; } = new HeaderFooterCollection();
    }

    public class DocumentElement
    {
        public string Id { get; set; }
        public DocumentElementType Type { get; set; }
        public List<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();
        public Table Table { get; set; }
        public SectionBreak SectionBreak { get; set; }
    }

    public enum DocumentElementType
    {
        Paragraph,
        Table,
        SectionBreak,
        TableOfContents,
        Bookmark,
        Comment,
        Revision
    }

    public class Paragraph : ICloneable
    {
        public string Id { get; set; }
        public ParagraphProperties Properties { get; set; } = new ParagraphProperties();
        public List<Run> Runs { get; set; } = new List<Run>();
        public FieldRange FieldRange { get; set; }
        public CommentRange CommentRange { get; set; }
        public RevisionMarker Revision { get; set; }

        public object Clone()
        {
            return new Paragraph
            {
                Id = Guid.NewGuid().ToString(),
                Properties = Properties.Clone(),
                Runs = Runs.Select(r => r.Clone()).ToList(),
                FieldRange = FieldRange?.Clone(),
                CommentRange = CommentRange?.Clone(),
                Revision = Revision?.Clone()
            };
        }

        public string GetText()
        {
            var sb = new StringBuilder();
            foreach (var run in Runs)
            {
                sb.Append(run.Text);
            }
            return sb.ToString();
        }

        public void SetText(string text)
        {
            Runs.Clear();
            Runs.Add(new Run { Text = text });
        }
    }

    public class ParagraphProperties
    {
        public string StyleId { get; set; }
        public string StyleName { get; set; }
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public ParagraphBorders Borders { get; set; }
        public Shading Shading { get; set; }
        public ParagraphIndentation Indentation { get; set; } = new ParagraphIndentation();
        public ParagraphSpacing Spacing { get; set; } = new ParagraphSpacing();
        public PaginationControl Pagination { get; set; } = new PaginationControl();
        public TextDirection TextDirection { get; set; } = TextDirection.LRTB;
        public string LanguageId { get; set; } = "en-US";
        public bool? SpellCheck { get; set; }
        public bool AutoHyphenation { get; set; } = false;
        public int? Kinsoku { get; set; }
        public bool? WordWrap { get; set; }
        public int? OverflowPunctuation { get; set; }
        public TopLinePunctuationType? TopLinePunctuation { get; set; }
        public int? OutlineLevel { get; set; }
        public bool? DivId { get; set; }
        public bool? SuppressLineNumbers { get; set; }
        public string RevisionId { get; set; }
        public FrameProperties Frame { get; set; }
        public SnapToGrid SnapToGrid { get; set; } = new SnapToGrid();

        public ParagraphProperties Clone()
        {
            return new ParagraphProperties
            {
                StyleId = StyleId,
                StyleName = StyleName,
                Alignment = Alignment,
                Borders = Borders?.Clone(),
                Shading = Shading?.Clone(),
                Indentation = Indentation?.Clone(),
                Spacing = Spacing?.Clone(),
                Pagination = Pagination?.Clone(),
                TextDirection = TextDirection,
                LanguageId = LanguageId,
                SpellCheck = SpellCheck,
                AutoHyphenation = AutoHyphenation,
                Kinsoku = Kinsoku,
                WordWrap = WordWrap,
                OverflowPunctuation = OverflowPunctuation,
                TopLinePunctuation = TopLinePunctuation,
                OutlineLevel = OutlineLevel,
                DivId = DivId,
                SuppressLineNumbers = SuppressLineNumbers,
                RevisionId = RevisionId,
                Frame = Frame?.Clone(),
                SnapToGrid = SnapToGrid?.Clone()
            };
        }
    }

    public class ParagraphBorders
    {
        public BorderProperties Top { get; set; }
        public BorderProperties Bottom { get; set; }
        public BorderProperties Left { get; set; }
        public BorderProperties Right { get; set; }
        public BorderProperties Between { get; set; }
        public BorderProperties Bar { get; set; }
        public BorderConflictResolution ConflictMode { get; set; } = BorderConflictResolution.Adjust;

        public ParagraphBorders Clone()
        {
            return new ParagraphBorders
            {
                Top = Top?.Clone(),
                Bottom = Bottom?.Clone(),
                Left = Left?.Clone(),
                Right = Right?.Clone(),
                Between = Between?.Clone(),
                Bar = Bar?.Clone(),
                ConflictMode = ConflictMode
            };
        }
    }

    public class BorderProperties
    {
        public BorderStyle Style { get; set; } = BorderStyle.None;
        public int Size { get; set; } = 0;
        public string Color { get; set; } = "auto";
        public string ThemeColor { get; set; }
        public double? ThemeTint { get; set; }
        public double? ThemeShade { get; set; }
        public int? Space { get; set; }
        public BorderShadow Shadow { get; set; }
        public Shading Shading { get; set; }

        public BorderProperties Clone()
        {
            return new BorderProperties
            {
                Style = Style,
                Size = Size,
                Color = Color,
                ThemeColor = ThemeColor,
                ThemeTint = ThemeTint,
                ThemeShade = ThemeShade,
                Space = Space,
                Shadow = Shadow?.Clone(),
                Shading = Shading?.Clone()
            };
        }
    }

    public class BorderShadow
    {
        public bool Valued { get; set; } = false;
        public string Color { get; set; } = "auto";
        public int? OffsetX { get; set; }
        public int? OffsetY { get; set; }
        public double? BlurRadius { get; set; }
        public double? Opacity { get; set; }

        public BorderShadow Clone()
        {
            return new BorderShadow
            {
                Valued = Valued,
                Color = Color,
                OffsetX = OffsetX,
                OffsetY = OffsetY,
                BlurRadius = BlurRadius,
                Opacity = Opacity
            };
        }
    }

    public enum BorderConflictResolution
    {
        Adjust,
        Override
    }

    public class Shading
    {
        public ShadingPattern Pattern { get; set; } = ShadingPattern.Clear;
        public string FillColor { get; set; } = "auto";
        public string Color { get; set; } = "auto";
        public string ThemeColor { get; set; }
        public double? ThemeTint { get; set; }
        public double? ThemeShade { get; set; }

        public Shading Clone()
        {
            return new Shading
            {
                Pattern = Pattern,
                FillColor = FillColor,
                Color = Color,
                ThemeColor = ThemeColor,
                ThemeTint = ThemeTint,
                ThemeShade = ThemeShade
            };
        }
    }

    public enum ShadingPattern
    {
        Clear,
        Solid,
        HorizontalLine,
        VerticalLine,
        DiagonalUp,
        DiagonalDown,
        Cross,
        DarkHorizontal,
        DarkVertical,
        DarkDiagonalUp,
        DarkDiagonalDown,
        DarkCross,
        DarkDiagonalCross,
        Outlined,
        Dotted,
        Dashed,
        Striped,
        ReverseDiagonal,
        DiagonalBrick,
        HorizontalBrick,
        Dlite,
        Divot,
        DottedDiamond,
        DownwardDiagonal,
        LargeChecker,
        LargeGrid,
        LargeConfetti,
        LightHorizontal,
        LightVertical,
        LightDownwardDiagonal,
        LightUpwardDiagonal,
        LightGrid,
        NarrowHorizontal,
        NarrowVertical,
        OutlinedDiamond,
        OutlinedSquare,
        SmallChecker,
        SmallConfetti,
        SmallGrid
    }

    public class ParagraphIndentation
    {
        public int? Left { get; set; }
        public int? Right { get; set; }
        public int? FirstLine { get; set; }
        public int? FirstLineChars { get; set; }
        public HangingPunctuation Hanging { get; set; }
        public int? HangingChars { get; set; }
        public TextAdjustment TextAdjustment { get; set; }

        public ParagraphIndentation Clone()
        {
            return new ParagraphIndentation
            {
                Left = Left,
                Right = Right,
                FirstLine = FirstLine,
                FirstLineChars = FirstLineChars,
                Hanging = Hanging,
                HangingChars = HangingChars,
                TextAdjustment = TextAdjustment
            };
        }
    }

    public class HangingPunctuation
    {
        public bool Enabled { get; set; } = false;
    }

    public class TextAdjustment
    {
        public double? Value { get; set; }
    }

    public class ParagraphSpacing
    {
        public SpacingUnit Before { get; set; } = new SpacingUnit();
        public SpacingUnit After { get; set; } = new SpacingUnit();
        public LineSpacingUnit Line { get; set; } = new LineSpacingUnit();
        public LineSpacingRule LineRule { get; set; } = LineSpacingRule.Auto;
        public int? BeforeAutoAdjust { get; set; }
        public int? AfterAutoAdjust { get; set; }

        public ParagraphSpacing Clone()
        {
            return new ParagraphSpacing
            {
                Before = Before?.Clone(),
                After = After?.Clone(),
                Line = Line?.Clone(),
                LineRule = LineRule,
                BeforeAutoAdjust = BeforeAutoAdjust,
                AfterAutoAdjust = AfterAutoAdjust
            };
        }
    }

    public class SpacingUnit
    {
        public int? Points { get; set; }
        public int? Lines { get; set; }
        public int? Twips { get; set; }
        public bool Auto { get; set; } = true;
    }

    public class LineSpacingUnit
    {
        public double? Points { get; set; }
        public int? Lines { get; set; }
        public int? Percentage { get; set; }
    }

    public enum LineSpacingRule
    {
        Auto,
        Exact,
        Multiple
    }

    public class PaginationControl
    {
        public WidowControl WidowControl { get; set; } = WidowControl.On;
        public OrphanControl OrphanControl { get; set; } = OrphanControl.On;
        public KeepLines KeepLines { get; set; } = KeepLines.On;
        public KeepNext KeepNext { get; set; } = KeepNext.Off;
        public PageBreakBefore PageBreakBefore { get; set; } = PageBreakBefore.Off;
        public SuppressLineNumbers SuppressLineNumbers { get; set; } = SuppressLineNumbers.Off;

        public PaginationControl Clone()
        {
            return new PaginationControl
            {
                WidowControl = WidowControl,
                OrphanControl = OrphanControl,
                KeepLines = KeepLines,
                KeepNext = KeepNext,
                PageBreakBefore = PageBreakBefore,
                SuppressLineNumbers = SuppressLineNumbers
            };
        }
    }

    public enum WidowControl { On, Off }
    public enum OrphanControl { On, Off }
    public enum KeepLines { On, Off }
    public enum KeepNext { On, Off }
    public enum PageBreakBefore { On, Off }
    public enum SuppressLineNumbers { On, Off }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Both,
        Distribute,
        JustifyMedium,
        JustifyHigh,
        JustifyLow,
        ThaiJustify
    }

    public enum TextDirection
    {
        LRTB,
        LRTBV,
        LRBT,
        RLTB,
        RLTBV,
        RLBT,
        TBRL,
        TBRLV,
        TBLR,
        TBLRV,
        BTLR,
        BTLRV
    }

    public enum TopLinePunctuationType { On, Off }

    public class FrameProperties
    {
        public HorizontalAnchor HorizontalAnchor { get; set; } = HorizontalAnchor.Margin;
        public HorizontalPositionRelative HorizontalPositionRelative { get; set; } = HorizontalPositionRelative.Text;
        public VerticalAnchor VerticalAnchor { get; set; } = VerticalAnchor.Paragraph;
        public VerticalPositionRelative VerticalPositionRelative { get; set; } = VerticalPositionRelative.Text;
        public FrameHorizontalAlignment HorizontalAlignment { get; set; } = FrameHorizontalAlignment.Left;
        public double? HorizontalPosition { get; set; }
        public double? VerticalPosition { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public HeightRule HeightRule { get; set; } = HeightRule.AtLeast;
        public HeightType HeightType { get; set; } = HeightType.Dxa;
        public WidthType WidthType { get; set; } = WidthType.Dxa;
        public WrapType Wrap { get; set; } = WrapType.Square;
        public VAnchor VAnchor { get; set; } = VAnchor.Text;
        public HAnchor HAnchor { get; set; } = HAnchor.Text;
        public bool? LockAnchor { get; set; }
        public bool? DropCap { get; set; }
        public FlowDirection FrameFlowDirection { get; set; } = FlowDirection.LTR;
        public int? VAnchorOrig { get; set; }
        public int? HAnchorOrig { get; set; }
        public bool? XLockAnchor { get; set; }
        public bool? YLockAnchor { get; set; }
        public int? Lines { get; set; }
        public double? PositionH { get; set; }
        public double? PositionV { get; set; }
        public RelativeFrom PositionHRelative { get; set; } = RelativeFrom.Column;
        public RelativeFrom PositionVRelative { get; set; } = RelativeFrom.Paragraph;
        public bool? Overlap { get; set; }

        public FrameProperties Clone()
        {
            return new FrameProperties
            {
                HorizontalAnchor = HorizontalAnchor,
                HorizontalPositionRelative = HorizontalPositionRelative,
                VerticalAnchor = VerticalAnchor,
                VerticalPositionRelative = VerticalPositionRelative,
                HorizontalAlignment = HorizontalAlignment,
                HorizontalPosition = HorizontalPosition,
                VerticalPosition = VerticalPosition,
                Width = Width,
                Height = Height,
                HeightRule = HeightRule,
                HeightType = HeightType,
                WidthType = WidthType,
                Wrap = Wrap,
                VAnchor = VAnchor,
                HAnchor = HAnchor,
                LockAnchor = LockAnchor,
                DropCap = DropCap,
                FrameFlowDirection = FrameFlowDirection,
                VAnchorOrig = VAnchorOrig,
                HAnchorOrig = HAnchorOrig,
                XLockAnchor = XLockAnchor,
                YLockAnchor = YLockAnchor,
                Lines = Lines,
                PositionH = PositionH,
                PositionV = PositionV,
                PositionHRelative = PositionHRelative,
                PositionVRelative = PositionVRelative,
                Overlap = Overlap
            };
        }
    }

    public enum HorizontalAnchor { Margin, Page, Column, Character }
    public enum HorizontalPositionRelative { Text, Margin, Page }
    public enum VerticalAnchor { Margin, Page, Paragraph, Line }
    public enum VerticalPositionRelative { Text, Margin, Page }
    public enum FrameHorizontalAlignment { Left, Center, Right }
    public enum HeightRule { Auto, AtLeast, Exact }
    public enum HeightType { Auto, Dxa, Percentage }
    public enum WidthType { Auto, Dxa, Percentage }
    public enum WrapType { None, Square, Tight, Through, Around, TopBottom, Left, Right }
    public enum VAnchor { Margin, Page, Text }
    public enum HAnchor { Margin, Page, Text }
    public enum FlowDirection { LTR, RTL }
    public enum RelativeFrom { Character, Column, Margin, Page, Paragraph }

    public class SnapToGrid
    {
        public bool Enabled { get; set; } = true;
        public int? GridLines { get; set; }
    }

    public class Run : ICloneable
    {
        public string Id { get; set; }
        public RunProperties Properties { get; set; } = new RunProperties();
        public string Text { get; set; }
        public FieldRange FieldRange { get; set; }
        public PictureElement Picture { get; set; }
        public CommentRangeStart CommentStart { get; set; }
        public CommentRangeEnd CommentEnd { get; set; }
        public RevisionRange RevisionStart { get; set; }
        public RevisionRange RevisionEnd { get; set; }
        public Hyperlink Hyperlink { get; set; }
        public OfficeArtDrawing Drawing { get; set; }
        public object Content { get; set; }

        public object Clone()
        {
            return new Run
            {
                Id = Guid.NewGuid().ToString(),
                Properties = Properties.Clone(),
                Text = Text,
                FieldRange = FieldRange?.Clone(),
                Picture = Picture?.Clone(),
                CommentStart = CommentStart?.Clone(),
                CommentEnd = CommentEnd?.Clone(),
                RevisionStart = RevisionStart?.Clone(),
                RevisionEnd = RevisionEnd?.Clone(),
                Hyperlink = Hyperlink?.Clone(),
                Drawing = Drawing?.Clone(),
                Content = Content
            };
        }
    }

    public class RunProperties
    {
        public RunFonts Fonts { get; set; } = new RunFonts();
        public int FontSize { get; set; } = 22;
        public int FontSizeCs { get; set; } = 22;
        public string Color { get; set; } = "auto";
        public HighlightColor Highlight { get; set; }
        public Shading Shading { get; set; }
        public EmphasisMark Emphasis { get; set; }
        public bool Bold { get; set; } = false;
        public bool BoldComplexScript { get; set; } = false;
        public bool Italic { get; set; } = false;
        public bool ItalicComplexScript { get; set; } = false;
        public UnderlineType Underline { get; set; } = UnderlineType.None;
        public string UnderlineColor { get; set; } = "auto";
        public StrikeType Strike { get; set; } = StrikeType.None;
        public VerticalTextAlignment VerticalPosition { get; set; } = VerticalTextAlignment.Baseline;
        public double? Position { get; set; }
        public double CharacterSpacing { get; set; } = 0;
        public string LanguageId { get; set; } = "en-US";
        public string LanguageIdAlt { get; set; }
        public bool? SpellCheck { get; set; }
        public bool SmallCaps { get; set; } = false;
        public bool AllCaps { get; set; } = false;
        public bool Hdn { get; set; } = false;
        public bool Imprint { get; set; } = false;
        public bool Emboss { get; set; } = false;
        public Outline Outline { get; set; }
        public Shadow Shadow { get; set; }
        public ScriptType Script { get; set; } = ScriptType.None;
        public string StyleId { get; set; }
        public string RevisionId { get; set; }

        public RunProperties Clone()
        {
            return new RunProperties
            {
                Fonts = Fonts?.Clone(),
                FontSize = FontSize,
                FontSizeCs = FontSizeCs,
                Color = Color,
                Highlight = Highlight,
                Shading = Shading?.Clone(),
                Emphasis = Emphasis,
                Bold = Bold,
                BoldComplexScript = BoldComplexScript,
                Italic = Italic,
                ItalicComplexScript = ItalicComplexScript,
                Underline = Underline,
                UnderlineColor = UnderlineColor,
                Strike = Strike,
                VerticalPosition = VerticalPosition,
                Position = Position,
                CharacterSpacing = CharacterSpacing,
                LanguageId = LanguageId,
                LanguageIdAlt = LanguageIdAlt,
                SpellCheck = SpellCheck,
                SmallCaps = SmallCaps,
                AllCaps = AllCaps,
                Hdn = Hdn,
                Imprint = Imprint,
                Emboss = Emboss,
                Outline = Outline?.Clone(),
                Shadow = Shadow?.Clone(),
                Script = Script,
                StyleId = StyleId,
                RevisionId = RevisionId
            };
        }
    }

    public class RunFonts
    {
        public string Ascii { get; set; } = "Calibri";
        public string HighAnsi { get; set; } = "Calibri";
        public string ComplexScript { get; set; } = "Calibri";
        public string EastAsia { get; set; } = "Calibri";
        public string AsciiTheme { get; set; }
        public string HighAnsiTheme { get; set; }
        public string ComplexScriptTheme { get; set; }
        public string EastAsiaTheme { get; set; }
        public int? AsciiThemeSize { get; set; }
        public int? HighAnsiThemeSize { get; set; }

        public RunFonts Clone()
        {
            return new RunFonts
            {
                Ascii = Ascii,
                HighAnsi = HighAnsi,
                ComplexScript = ComplexScript,
                EastAsia = EastAsia,
                AsciiTheme = AsciiTheme,
                HighAnsiTheme = HighAnsiTheme,
                ComplexScriptTheme = ComplexScriptTheme,
                EastAsiaTheme = EastAsiaTheme,
                AsciiThemeSize = AsciiThemeSize,
                HighAnsiThemeSize = HighAnsiThemeSize
            };
        }
    }

    public enum HighlightColor
    {
        None,
        Black,
        Blue,
        Cyan,
        Green,
        Magenta,
        Red,
        Yellow,
        White,
        DarkBlue,
        DarkCyan,
        DarkGreen,
        DarkMagenta,
        DarkRed,
        DarkYellow,
        DarkGray,
        LightGray
    }

    public enum EmphasisMark
    {
        None,
        Comma,
        Circle,
        Dot,
        DoubleCircle,
        Triangle
    }

    public enum UnderlineType
    {
        None,
        Single,
        Words,
        Double,
        Thick,
        Dotted,
        DottedHeavy,
        Dash,
        DashedHeavy,
        DashLong,
        DashLongHeavy,
        DotDash,
        DashDotDot,
        Wave,
        WaveHeavy,
        WaveDouble
    }

    public enum StrikeType
    {
        None,
        Single,
        Double,
        SingleAccounting,
        DoubleAccounting
    }

    public enum VerticalTextAlignment
    {
        Baseline,
        Superscript,
        Subscript
    }

    public class Outline
    {
        public bool Valued { get; set; } = false;
        public string Color { get; set; } = "auto";
        public int? Size { get; set; }
        public string ThemeColor { get; set; }
        public double? ThemeTint { get; set; }
        public double? ThemeShade { get; set; }

        public Outline Clone()
        {
            return new Outline
            {
                Valued = Valued,
                Color = Color,
                Size = Size,
                ThemeColor = ThemeColor,
                ThemeTint = ThemeTint,
                ThemeShade = ThemeShade
            };
        }
    }

    public class Shadow
    {
        public ShadowType Type { get; set; } = ShadowType.None;
        public string Color { get; set; } = "auto";
        public int? OffsetX { get; set; }
        public int? OffsetY { get; set; }
        public int? BlurRadius { get; set; }
        public double? Opacity { get; set; }
        public string ThemeColor { get; set; }
        public double? ThemeTint { get; set; }
        public double? ThemeShade { get; set; }
        public bool? Alignment { get; set; }
    }

    public enum ShadowType
    {
        None,
        Single,
        Double,
        Outer,
        Inner,
        OuterCorner,
        InnerCorner
    }

    public enum ScriptType
    {
        None,
        Superscript,
        Subscript
    }

    public class FieldRange : ICloneable
    {
        public string FieldType { get; set; }
        public string FieldCode { get; set; }
        public string FieldValue { get; set; }
        public List<Run> Runs { get; set; } = new List<Run>();
        public FieldProperties Properties { get; set; } = new FieldProperties();

        public object Clone()
        {
            return new FieldRange
            {
                FieldType = FieldType,
                FieldCode = FieldCode,
                FieldValue = FieldValue,
                Runs = Runs.Select(r => r.Clone()).ToList(),
                Properties = Properties.Clone()
            };
        }
    }

    public class FieldProperties
    {
        public bool Dirty { get; set; } = false;
        public bool Locked { get; set; } = false;
        public bool Private { get; set; } = false;
        public bool NoProof { get; set; } = false;
        public string FieldId { get; set; }
    }

    public class PictureElement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public byte[] ImageData { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public string RelationshipId { get; set; }
        public bool PreferRelativeResize { get; set; } = false;
        public PictureCropping Cropping { get; set; }

        public PictureElement Clone()
        {
            return new PictureElement
            {
                Id = Id,
                Name = Name,
                Description = Description,
                ContentType = ContentType,
                ImageData = ImageData?.Clone() as byte[],
                Width = Width,
                Height = Height,
                OriginalWidth = OriginalWidth,
                OriginalHeight = OriginalHeight,
                RelationshipId = RelationshipId,
                PreferRelativeResize = PreferRelativeResize,
                Cropping = Cropping?.Clone()
            };
        }
    }

    public class PictureCropping
    {
        public double? Left { get; set; }
        public double? Top { get; set; }
        public double? Right { get; set; }
        public double? Bottom { get; set; }

        public PictureCropping Clone()
        {
            return new PictureCropping
            {
                Left = Left,
                Top = Top,
                Right = Right,
                Bottom = Bottom
            };
        }
    }

    public class CommentRangeStart
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Initials { get; set; }

        public CommentRangeStart Clone()
        {
            return new CommentRangeStart
            {
                Id = Id,
                Author = Author,
                Date = Date,
                Initials = Initials
            };
        }
    }

    public class CommentRangeEnd
    {
        public string Id { get; set; }

        public CommentRangeEnd Clone()
        {
            return new CommentRangeEnd { Id = Id };
        }
    }

    public class CommentRange
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Initials { get; set; }
        public string Text { get; set; }
        public List<CommentReply> Replies { get; set; } = new List<CommentReply>();
        public bool Done { get; set; } = false;

        public CommentRange Clone()
        {
            return new CommentRange
            {
                Id = Id,
                Author = Author,
                Date = Date,
                Initials = Initials,
                Text = Text,
                Replies = Replies.Select(r => r.Clone()).ToList(),
                Done = Done
            };
        }
    }

    public class CommentReply
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Initials { get; set; }
        public string Text { get; set; }

        public CommentReply Clone()
        {
            return new CommentReply
            {
                Id = Id,
                Author = Author,
                Date = Date,
                Initials = Initials,
                Text = Text
            };
        }
    }

    public class RevisionRange : ICloneable
    {
        public string Id { get; set; }
        public RevisionType Type { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string StyleId { get; set; }

        public object Clone()
        {
            return new RevisionRange
            {
                Id = Id,
                Type = Type,
                Author = Author,
                Date = Date,
                StyleId = StyleId
            };
        }
    }

    public enum RevisionType
    {
        Insertion,
        Deletion,
        FormatChange,
        StyleDefinition,
        InsertedRun,
        DeletedRun,
        MovedFrom,
        MovedTo,
        CellDeletion,
        CellInsertion,
        CellMerge,
        NumberingDefinition,
        ParagraphNumber,
        SectionProperties,
        TableProperties,
        TableRowDeletion,
        TableRowInsertion,
        TableCellDeletion,
        TableCellInsertion,
        TableCellMerge,
        TableCellSplit,
        TableBorder,
        TableContent
    }

    public class RevisionMarker : ICloneable
    {
        public RevisionType Type { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string StyleId { get; set; }
        public string PreviousStyleId { get; set; }
        public string PreviousContent { get; set; }
        public string NewContent { get; set; }

        public object Clone()
        {
            return new RevisionMarker
            {
                Type = Type,
                Author = Author,
                Date = Date,
                StyleId = StyleId,
                PreviousStyleId = PreviousStyleId,
                PreviousContent = PreviousContent,
                NewContent = NewContent
            };
        }
    }

    public class Hyperlink : ICloneable
    {
        public string Id { get; set; }
        public string Target { get; set; }
        public HyperlinkType Type { get; set; } = HyperlinkType.External;
        public string Anchor { get; set; }
        public string Tooltip { get; set; }
        public bool History { get; set; } = true;

        public object Clone()
        {
            return new Hyperlink
            {
                Id = Id,
                Target = Target,
                Type = Type,
                Anchor = Anchor,
                Tooltip = Tooltip,
                History = History
            };
        }
    }

    public enum HyperlinkType
    {
        External,
        Internal,
        Email,
        File
    }

    public class OfficeArtDrawing
    {
        public string Id { get; set; }
        public OfficeArtGraphic Graphic { get; set; }
        public OfficeArtPosition Position { get; set; } = new OfficeArtPosition();
        public OfficeArtExtent Extent { get; set; } = new OfficeArtExtent();
        public OfficeArtEffectExtent EffectExtent { get; set; }
        public OfficeArtAnchorproperties Anchor { get; set; }
        public OfficeArtDocAttributes DocAttributes { get; set; }
    }

    public class OfficeArtGraphic
    {
        public string Uri { get; set; }
        public OfficeArtGraphicData GraphicData { get; set; }
    }

    public class OfficeArtGraphicData
    {
        public string Uri { get; set; }
        public string Content { get; set; }
    }

    public class OfficeArtPosition
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }

    public class OfficeArtExtent
    {
        public int ExtentX { get; set; }
        public int ExtentY { get; set; }
    }

    public class OfficeArtEffectExtent
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }

    public class OfficeArtAnchorproperties
    {
        public bool BehindDoc { get; set; } = false;
        public bool Locked { get; set; } = false;
        public bool LayoutInCell { get; set; } = true;
        public bool AllowOverlap { get; set; } = true;
        public bool Hidden { get; set; } = false;
        public HorizontalPosition RelativeFrom HorizontalPositionRelative { get; set; } = HorizontalPositionRelative.Margin;
        public VerticalPositionRelative VerticalPositionRelative { get; set; } = VerticalPositionRelative.Paragraph;
    }

    public enum HorizontalPositionRelative
    {
        Margin,
        Page,
        Column,
        Character
    }

    public enum VerticalPositionRelative
    {
        Margin,
        Page,
        Paragraph,
        Line
    }

    public class OfficeArtDocAttributes
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Table
    {
        public string Id { get; set; }
        public TableProperties Properties { get; set; } = new TableProperties();
        public List<TableRow> Rows { get; set; } = new List<TableRow>();
        public TableGrid Grid { get; set; }

        public Table Clone()
        {
            return new Table
            {
                Id = Guid.NewGuid().ToString(),
                Properties = Properties.Clone(),
                Rows = Rows.Select(r => r.Clone()).ToList(),
                Grid = Grid?.Clone()
            };
        }

        public int RowCount => Rows.Count;
        public int ColumnCount => Rows.Count > 0 ? Rows[0].Cells.Count : 0;

        public TableCell GetCell(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count)
                return null;
            if (columnIndex < 0 || columnIndex >= Rows[rowIndex].Cells.Count)
                return null;
            return Rows[rowIndex].Cells[columnIndex];
        }
    }

    public class TableRow
    {
        public string Id { get; set; }
        public TableRowProperties Properties { get; set; } = new TableRowProperties();
        public List<TableCell> Cells { get; set; } = new List<TableCell>();

        public TableRow Clone()
        {
            return new TableRow
            {
                Id = Guid.NewGuid().ToString(),
                Properties = Properties.Clone(),
                Cells = Cells.Select(c => c.Clone()).ToList()
            };
        }
    }

    public class TableRowProperties
    {
        public Height Height { get; set; } = new Height();
        public TableRowAlignment? Alignment { get; set; }
        public int? GridSpan { get; set; }
        public int? HorizontalMerge { get; set; }
        public int? VerticalMerge { get; set; }
        public TableCellPropertiesTemplate RowHeader { get; set; }
        public CantSplit CantSplit { get; set; } = CantSplit.Off;
        public TableRowHeader TableHeader { get; set; } = TableRowHeader.Off;
    }

    public class Height
    {
        public int? Value { get; set; }
        public HeightRule Rule { get; set; } = HeightRule.Auto;
    }

    public enum HeightRule { Auto, AtLeast, Exact }
    public enum TableRowAlignment { Left, Center, Right, Both }
    public enum CantSplit { On, Off }
    public enum TableRowHeader { On, Off }

    public class TableCell
    {
        public string Id { get; set; }
        public TableCellProperties Properties { get; set; } = new TableCellProperties();
        public List<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();
        public Table NestedTable { get; set; }

        public TableCell Clone()
        {
            return new TableCell
            {
                Id = Guid.NewGuid().ToString(),
                Properties = Properties.Clone(),
                Paragraphs = Paragraphs.Select(p => p.Clone()).ToList(),
                NestedTable = NestedTable?.Clone()
            };
        }

        public string GetText()
        {
            var sb = new StringBuilder();
            foreach (var para in Paragraphs)
            {
                sb.AppendLine(para.GetText());
            }
            return sb.ToString();
        }

        public void SetText(string text)
        {
            Paragraphs.Clear();
            var para = new Paragraph();
            para.SetText(text);
            Paragraphs.Add(para);
        }
    }

    public class TableCellProperties
    {
        public Width Width { get; set; } = new Width();
        public TableCellVerticalAlignment VerticalAlignment { get; set; } = TableCellVerticalAlignment.Top;
        public Shading Shading { get; set; }
        public TableCellBorders Borders { get; set; }
        public Margins Margins { get; set; } = new Margins();
        public int? RowSpan { get; set; }
        public int? ColumnSpan { get; set; }
        public FitText FitText { get; set; }
        public bool NoWrap { get; set; } = false;
        public TableCellHeader Header { get; set; } = TableCellHeader.Off;
        public TableCellAnchor Anchor { get; set; }
        public TableCellMarginDefault MarginDefault { get; set; }
    }

    public class Width
    {
        public int? Value { get; set; }
        public WidthType Type { get; set; } = WidthType.Auto;
    }

    public enum WidthType { Auto, Dxa, Percentage }
    public enum TableCellVerticalAlignment { Top, Center, Bottom, Both }
    public enum TableCellHeader { On, Off }

    public class TableCellPropertiesTemplate { }
    public class FitText { public int? Width { get; set; } }

    public class TableCellAnchor
    {
        public VerticalAnchor AnchorType { get; set; } = VerticalAnchor.Margin;
        public int? HorizontalPosition { get; set; }
        public int? VerticalPosition { get; set; }
    }

    public enum VerticalAnchor { Margin, Page, Text }

    public class TableCellMarginDefault
    {
        public Margin Top { get; set; }
        public Margin Bottom { get; set; }
        public Margin Left { get; set; }
        public Margin Right { get; set; }
    }

    public class Margins
    {
        public int? Top { get; set; }
        public int? Bottom { get; set; }
        public int? Left { get; set; }
        public int? Right { get; set; }
        public MarginType Type { get; set; } = MarginType.Dxa;
    }

    public class Margin
    {
        public int? Width { get; set; }
        public MarginType Type { get; set; } = MarginType.Dxa;
    }

    public enum MarginType { Auto, Dxa, Percentage, Nil }

    public class TableCellBorders
    {
        public BorderProperties Top { get; set; }
        public BorderProperties Bottom { get; set; }
        public BorderProperties Left { get; set; }
        public BorderProperties Right { get; set; }
        public BorderProperties InsideH { get; set; }
        public BorderProperties InsideV { get; set; }
        public BorderProperties TopLeftToBottomRight { get; set; }
        public BorderProperties TopRightToBottomLeft { get; set; }
    }

    public enum BorderStyle
    {
        None,
        Single,
        Thick,
        Double,
        Hair,
        Dotted,
        Dashed,
        DotDash,
        DotDotDash,
        Triple,
        ThickThinSmall,
        ThinThickSmall,
        ThinThickThinSmall,
        ThickThinMed,
        ThinThickMed,
        ThinThickThinMed,
        ThickThinLarge,
        ThinThickLarge,
        ThinThickThinLarge,
        Wave,
        DoubleWave,
        DashedHeavy,
        DashDotStroked,
        ThreeDEmboss,
        ThreeDEngrave,
        Outset,
        Inset,
        TwistedFirst,
        TwistedSecond
    }

    public class TableProperties
    {
        public Width Width { get; set; } = new Width();
        public TableJustification Justification { get; set; } = TableJustification.Left;
        public TableBorders Borders { get; set; }
        public Shading Shading { get; set; }
        public TableLayout Layout { get; set; } = TableLayout.Auto;
        public TableOverlap Overlap { get; set; } = TableOverlap.No;
        public bool? AllowOverlap { get; set; }
        public TableStyle TableStyle { get; set; }
        public TableStyleRowBandSize RowBandSize { get; set; } = new TableStyleRowBandSize();
        public TableStyleColBandSize ColBandSize { get; set; } = new TableStyleColBandSize();
        public TableLook TableLook { get; set; }
        public TableId TableId { get; set; }
        public TablePositionProperties TablePosition { get; set; }
        public DecimalSymbol DecimalSymbol { get; set; }
        public ListSeparator ListSeparator { get; set; }

        public TableProperties Clone()
        {
            return new TableProperties
            {
                Width = Width?.Clone(),
                Justification = Justification,
                Borders = Borders?.Clone(),
                Shading = Shading?.Clone(),
                Layout = Layout,
                Overlap = Overlap,
                AllowOverlap = AllowOverlap,
                TableStyle = TableStyle?.Clone(),
                RowBandSize = RowBandSize?.Clone(),
                ColBandSize = ColBandSize?.Clone(),
                TableLook = TableLook?.Clone(),
                TableId = TableId?.Clone(),
                TablePosition = TablePosition?.Clone(),
                DecimalSymbol = DecimalSymbol?.Clone(),
                ListSeparator = ListSeparator?.Clone()
            };
        }
    }

    public class Width Clone()
    {
        return new Width { Value = Value, Type = Type };
    }

    public enum TableJustification { Left, Center, Right, Both, Fixed }
    public enum TableLayout { Auto, Fixed }
    public enum TableOverlap { Yes, No }

    public class TableStyle
    {
        public string Val { get; set; }
        public TableStyle Next { get; set; }
        public TableStyleLink Link { get; set; }

        public TableStyle Clone()
        {
            return new TableStyle { Val = Val };
        }
    }

    public class TableStyleNext { public string Val { get; set; } }
    public class TableStyleLink { public string Val { get; set; } }

    public class TableStyleRowBandSize { public int? Val { get; set; } }
    public class TableStyleColBandSize { public int? Val { get; set; } }

    public class TableLook
    {
        public bool FirstColumn { get; set; } = false;
        public bool FirstRow { get; set; } = false;
        public bool LastColumn { get; set; } = false;
        public bool LastRow { get; set; } = false;
        public bool HorizontalBand { get; set; } = false;
        public bool VerticalBand { get; set; } = false;

        public TableLook Clone()
        {
            return new TableLook
            {
                FirstColumn = FirstColumn,
                FirstRow = FirstRow,
                LastColumn = LastColumn,
                LastRow = LastRow,
                HorizontalBand = HorizontalBand,
                VerticalBand = VerticalBand
            };
        }
    }

    public class TableId { public int? Val { get; set; } }

    public class TablePositionProperties
    {
        public int? HorizontalPosition { get; set; }
        public HorizontalAnchor HorizontalAnchor { get; set; } = HorizontalAnchor.Margin;
        public RelativeFrom HorizontalPositionRelative { get; set; } = RelativeFrom.Margin;
        public int? VerticalPosition { get; set; }
        public VerticalAnchor VerticalAnchor { get; set; } = VerticalAnchor.Margin;
        public RelativeFrom VerticalPositionRelative { get; set; } = RelativeFrom.Margin;
        public bool? PositionOwned { get; set; }
        public TableOverlap Overlap { get; set; } = TableOverlap.Vertical;
        public WrapType Wrap { get; set; } = WrapType.None;
        public double? HorizontalDistance { get; set; }
        public double? VerticalDistance { get; set; }
    }

    public enum WrapType { None, Around, Square, Through }

    public class DecimalSymbol { public string Val { get; set; } }
    public class ListSeparator { public string Val { get; set; } }

    public class TableBorders
    {
        public BorderProperties Top { get; set; }
        public BorderProperties Bottom { get; set; }
        public BorderProperties Left { get; set; }
        public BorderProperties Right { get; set; }
        public BorderProperties InsideH { get; set; }
        public BorderProperties InsideV { get; set; }
        public BorderProperties Between { get; set; }
        public BorderProperties Bar { get; set; }

        public TableBorders Clone()
        {
            return new TableBorders
            {
                Top = Top?.Clone(),
                Bottom = Bottom?.Clone(),
                Left = Left?.Clone(),
                Right = Right?.Clone(),
                InsideH = InsideH?.Clone(),
                InsideV = InsideV?.Clone(),
                Between = Between?.Clone(),
                Bar = Bar?.Clone()
            };
        }
    }

    public class TableGrid
    {
        public List<GridColumn> Columns { get; set; } = new List<GridColumn>();

        public TableGrid Clone()
        {
            return new TableGrid { Columns = Columns.ToList() };
        }
    }

    public class GridColumn
    {
        public Width Width { get; set; }
    }

    public class SectionBreak
    {
        public SectionBreakType Type { get; set; } = SectionBreakType.NextPage;
    }

    public enum SectionBreakType { NextPage, Continuous, NextColumn, EvenPage, OddPage }

    public class Bookmark
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Content { get; set; }
    }

    public class Comment
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Initials { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public List<CommentReply> Replies { get; set; } = new List<CommentReply>();
        public bool Done { get; set; } = false;
        string parent { get; set; }
    }

    public class Revision
    {
        public string Id { get; set; }
        public RevisionType Type { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string StyleId { get; set; }
        public string Content { get; set; }
        public string PreviousContent { get; set; }
    }

    public class HeaderFooterCollection
    {
        public Header DefaultHeader { get; set; }
        public Header FirstHeader { get; set; }
        public Header EvenHeader { get; set; }
        public Footer DefaultFooter { get; set; }
        public Footer FirstFooter { get; set; }
        public Footer EvenFooter { get; set; }
    }

    public class Header
    {
        public string Id { get; set; }
        public List<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();
        public HeaderFooterLinkType LinkType { get; set; }
    }

    public class Footer
    {
        public string Id { get; set; }
        public List<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();
        public HeaderFooterLinkType LinkType { get; set; }
    }

    public enum HeaderFooterLinkType { Default, First, Even }
}

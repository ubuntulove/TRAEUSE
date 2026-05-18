using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OfficeSuite.Word.Formatting
{
    public class NumberingListDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public NumberingFormat Format { get; set; } = NumberingFormat.Decimal;
        public NumberingLevelCollection Levels { get; set; } = new NumberingLevelCollection();
        public bool IsMultiLevel { get; set; } = false;
        public NumberingRestartType RestartType { get; set; } = NumberingRestartType.Continuous;
    }

    public enum NumberingFormat
    {
        Decimal,
        UpperRoman,
        LowerRoman,
        UpperLetter,
        LowerLetter,
        Ordinal,
        OrdinalText,
        Arabic,
        Kanji,
        KanjiDigit,
        Aiueo,
        Iroha,
        ArabicHalfWidth,
        ArabicFullWidth,
        Katakana,
        KatakanaIroha,
        HebrewNonStandard,
        HebrewStandard,
        AlphaNumeric,
        ThaiArabic,
        ThaiLetter,
        VietArabic,
        NumberInCircle,
        ChineseCounting,
        ChineseCountingThousand,
        ChineseLegal,
        ChineseLegalSimplified,
        KoreanCounting,
        KoreanLegal,
        KoreanLegal2,
        KoreanNumber,
        HebrewAlpha,
        ArabicAlpha,
        HindiAlpha,
        ThaiAlpha,
        AlphaNumericDash,
        RussianLower,
        RussianUpper,
        HindiNumber,
        HindiNumber1,
        HindiNumber2,
        HindiNumber3,
        HindiNumber4,
        ThaiNumber,
        VietNumber,
        Burmese,
        KhmerLetter,
        LaoLetter,
        MyanmarLetter,
        MongolianLetter,
        PersianNumber,
        HindiDescription,
        ThaiDescription,
        ArabicDescription,
        ChineseDescription1,
        ChineseDescription2,
        KoreanDescription,
        RussianDescription
    }

    public enum NumberingRestartType
    {
        Continuous,
        RestartSection,
        RestartPage
    }

    public class NumberingLevelCollection
    {
        public List<NumberingLevel> Levels { get; set; } = new List<NumberingLevel>();

        public NumberingLevel GetLevel(int index)
        {
            if (index < 0 || index >= Levels.Count)
                return Levels.LastOrDefault();
            return Levels[index];
        }

        public void AddLevel(NumberingLevel level)
        {
            Levels.Add(level);
        }
    }

    public class NumberingLevel
    {
        public int LevelIndex { get; set; }
        public NumberingFormat Format { get; set; } = NumberingFormat.Decimal;
        public string LevelText { get; set; } = "%1.";
        public int StartNumber { get; set; } = 1;
        public int PreviousLevel { get; set; } = -1;
        public int? LinkedToLevel { get; set; }
        public bool Legacy { get; set; } = false;
        public int LegacySpace { get; set; } = 0;
        public int? TemplateCode { get; set; }
        public int? PictureBulletId { get; set; }

        public ParagraphStyle ParagraphStyle { get; set; }
        public TabStopCollection TabStops { get; set; } = new TabStopCollection();
        public TextFormatting TextFormatting { get; set; } = new TextFormatting();
        public ShadingDefinition Shading { get; set; }
        public BorderDefinition ParagraphBorder { get; set; }
        public int IndentLeft { get; set; } = 720;
        public int IndentHanging { get; set; } = 360;
        public SpacingAfterText Spacing { get; set; } = new SpacingAfterText();
    }

    public class TabStopCollection
    {
        public List<TabStop> Stops { get; set; } = new List<TabStop>();

        public void AddTabStop(TabStop tabStop)
        {
            Stops.Add(tabStop);
        }
    }

    public class TabStop
    {
        public int Position { get; set; }
        public TabStopAlignment Alignment { get; set; } = TabStopAlignment.Left;
        public TabStopLeader Leader { get; set; } = TabStopLeader.None;
    }

    public enum TabStopAlignment
    {
        Left,
        Center,
        Right,
        Decimal,
        Bar
    }

    public enum TabStopLeader
    {
        None,
        Dot,
        Hyphen,
        Underscore,
        Heavy,
        MiddleDot
    }

    public class TextFormatting
    {
        public string FontName { get; set; } = "Calibri";
        public int FontSize { get; set; } = 22;
        public int FontSizeComplex { get; set; } = 22;
        public string FontColor { get; set; } = "000000";
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
        public bool Underline { get; set; } = false;
        public UnderlineStyle UnderlineStyle { get; set; } = UnderlineStyle.None;
        public string UnderlineColor { get; set; } = "000000";
        public bool StrikeThrough { get; set; } = false;
        public DoubleStrikeThrough DoubleStrike { get; set; } = DoubleStrikeThrough.None;
        public bool SmallCaps { get; set; } = false;
        public bool AllCaps { get; set; } = false;
        public bool Hidden { get; set; } = false;
        public VerticalAlignment VerticalPosition { get; set; } = VerticalAlignment.Baseline;
        public int? Position { get; set; }
        public double CharacterSpacing { get; set; } = 0;
        public double LanguageId { get; set; } = 1033;
    }

    public enum UnderlineStyle
    {
        None,
        Single,
        Words,
        Double,
        Thick,
        Dotted,
        DottedHeavy,
        Dash,
        DashHeavy,
        DashLong,
        DashLongHeavy,
        DotDash,
        DashDotDot,
        Wave,
        WaveHeavy,
        WaveDouble
    }

    public enum DoubleStrikeThrough
    {
        None,
        On
    }

    public enum VerticalAlignment
    {
        Baseline,
        Superscript,
        Subscript
    }

    public class ShadingDefinition
    {
        public ShadingPattern Pattern { get; set; } = ShadingPattern.Clear;
        public string ForegroundColor { get; set; } = "auto";
        public string BackgroundColor { get; set; } = "auto";
        public string FillColor { get; set; }
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
        LargeConfetti2,
        LargeConfetti3,
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
        SmallGrid,
        SmallHorizBand,
        SmallVertBand,
        SmallConfetti4,
        SmallConfetti5,
        SmallConfetti6,
        DiagonalLeft,
        DiagonalRight,
        Percent10,
        Percent20,
        Percent30,
        Percent40,
        Percent5,
        Percent50,
        Percent60,
        Percent70,
        Percent80,
        Percent90
    }

    public class BorderDefinition
    {
        public BorderStyle Style { get; set; } = BorderStyle.None;
        public int Size { get; set; } = 0;
        public string Color { get; set; } = "auto";
        public int? Space { get; set; }
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

    public class SpacingAfterText
    {
        public int BeforeLines { get; set; } = 0;
        public int AfterLines { get; set; } = 0;
        public int BeforePoints { get; set; } = 0;
        public int AfterPoints { get; set; } = 0;
        public int LineRule { get; set; } = 1;
        public int? Line { get; set; }
        public int? LineExact { get; set; }
        public int? BeforeAutoAdjust { get; set; }
        public int? AfterAutoAdjust { get; set; }
    }

    public class ParagraphStyle
    {
        public string StyleId { get; set; }
        public string StyleName { get; set; }
        public StyleType Type { get; set; } = StyleType.Paragraph;
        public string BasedOn { get; set; }
        public string NextParagraphStyle { get; set; }
        public bool Link { get; set; } = false;
        public string LinkStyleId { get; set; }
        public bool QuickStyle { get; set; } = false;
        bool? _hidden;
        public bool? Hidden { get => _hidden; set => _hidden = value; }
        public bool PrimaryStyle { get; set; } = false;
        public int? UiPriority { get; set; }
        public bool Primary { get; set; } = false;
        public TextFormatting TextFormatting { get; set; } = new TextFormatting();
        public ParagraphFormatting ParagraphFormatting { get; set; } = new ParagraphFormatting();
        public ShadingDefinition Shading { get; set; }
        public BorderDefinition Border { get; set; }
        public SectionProperties SectionProperties { get; set; }
    }

    public class ParagraphFormatting
    {
        public SpacingAfterText Spacing { get; set; } = new SpacingAfterText();
        public int IndentationLeft { get; set; } = 0;
        public int IndentationRight { get; set; } = 0;
        public int? IndentationFirstLine { get; set; }
        public int? IndentationFirstLineChars { get; set; }
        public SpacingTextAlignment TextAlignment { get; set; } = SpacingTextAlignment.Left;
        public widowControl WidowControl { get; set; } = widowControl.On;
        public bool KeepWithNext { get; set; } = false;
        public bool KeepLines { get; set; } = false;
        public bool PageBreakBefore { get; set; } = false;
        public FrameLayout FrameLayout { get; set; } = new FrameLayout();
        public MirrorIndents MirrorIndents { get; set; } = MirrorIndents.None;
        public int? TextboxTightWrap { get; set; }
        public OutlineLevel OutlineLevel { get; set; } = OutlineLevel.Level1;
        public bool DivId { get; set; } = false;
        public SuppressLineNumbers SuppressLineNumbers { get; set; } = SuppressLineNumbers.None;
        publicSuppressAutoHyphens SuppressAutoHyphens { get; set; } = suppressAutoHyphens.None;
        public KinsokuType Kinsoku { get; set; } = KinsokuType.None;
        public WordWrap WordWrap { get; set; } = WordWrap.Normal;
        public int? OverflowPunctuation { get; set; }
        public TopLinePunctuation TopLinePunctuation { get; set; } = TopLinePunctuation.None;
        public AutoSpaceDE AutoSpaceDE { get; set; } = AutoSpaceDE.None;
        public AutoSpaceDN AutoSpaceDN { get; set; } = AutoSpaceDN.None;
        public AdjustRightIndent AdjustRightIndent { get; set; } = AdjustRightIndent.On;
        public AutoSpaceDNForced AutoSpaceDNForced { get; set; } = AutoSpaceDNForced.Off;
        public AutoSpaceDEForced AutoSpaceDEForced { get; set; } = AutoSpaceDEForced.Off;
        public IndentationStrategy IndentationStrategy { get; set; } = IndentationStrategy.None;
    }

    public enum SpacingTextAlignment
    {
        Left,
        Center,
        Right,
        Both,
        Distribute,
        JustifyMed,
        JustifyHi,
        JustifyLow,
        ThaiJustify
    }

    public enum widowControl
    {
        On,
        Off,
        Default
    }

    public class FrameLayout
    {
        public bool? Width { get; set; }
        public bool? Height { get; set; }
        public bool? HorizontalPosition { get; set; }
        public bool? VerticalPosition { get; set; }
        public HorizontalAnchor HorizontalAnchor { get; set; } = HorizontalAnchor.Margin;
        public VerticalAnchor VerticalAnchor { get; set; } = VerticalAnchor.Margin;
        public FrameHorizontalPosition HorizontalPositionRelative { get; set; } = FrameHorizontalPosition.Left;
        public FrameVerticalPosition VerticalPositionRelative { get; set; } = FrameVerticalPosition.Top;
        public bool LockAnchor { get; set; } = false;
        public FrameLayout Wrap { get; set; } = FrameLayout.Square;
        public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRight;
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? H { get; set; }
        public int? W { get; set; }
        public int? HRule { get; set; }
        public int? VRule { get; set; }
        public int? VRuleAlign { get; set; }
    }

    public enum MirrorIndents
    {
        None,
        On
    }

    public enum OutlineLevel
    {
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
        Level8,
        Level9,
        BodyText
    }

    public enum SuppressLineNumbers
    {
        None,
        On
    }

    public enum suppressAutoHyphens
    {
        None,
        On
    }

    public enum KinsokuType
    {
        None,
        On
    }

    public enum WordWrap
    {
        Normal,
        None,
        On
    }

    public enum TopLinePunctuation
    {
        None,
        On
    }

    public enum AutoSpaceDE
    {
        None,
        On
    }

    public enum AutoSpaceDN
    {
        None,
        On
    }

    public enum AdjustRightIndent
    {
        On,
        Off
    }

    public enum AutoSpaceDNForced
    {
        Off,
        On
    }

    public enum AutoSpaceDEForced
    {
        Off,
        On
    }

    public enum IndentationStrategy
    {
        None,
        HighLetter,
        LowLetter,
        HighArabic,
        LowArabic
    }

    public enum HorizontalAnchor
    {
        Margin,
        Page,
        Column,
        Character
    }

    public enum VerticalAnchor
    {
        Margin,
        Page,
        Paragraph,
        Line
    }

    public enum FrameHorizontalPosition
    {
        Left,
        Center,
        Right,
        Inside,
        Outside,
        Absolute
    }

    public enum FrameVerticalPosition
    {
        Top,
        Center,
        Bottom,
        Inside,
        Outside,
        Absolute
    }

    public enum FlowDirection
    {
        LeftToRight,
        RightToLeft
    }

    public class SectionProperties
    {
        public SectionType SectionType { get; set; } = SectionType.Continuous;
        public PageSize PageSize { get; set; } = new PageSize();
        public PageMargin PageMargin { get; set; } = new PageMargin();
        public PaperSource PaperSource { get; set; }
        public PageBorder PageBorder { get; set; }
        public LineNumberControl LineNumber { get; set; }
        public PageNumberControl PageNumber { get; set; } = new PageNumberControl();
        public ColumnProperties Columns { get; set; } = new ColumnProperties();
        public FootnoteProperties Footnote { get; set; }
        public EndnoteProperties Endnote { get; set; }
        public TextDirection TextDirection { get; set; } = TextDirection.LRTB;
        public VerticalAlignmentVertical SectionVerticalAlignment { get; set; } = VerticalAlignmentVertical.Top;
        public SectionBorders SectionBorders { get; set; }
        public DocGrid DocGrid { get; set; }
        public string HeaderReference { get; set; }
        public string FooterReference { get; set; }
        public int? GutterAtTop { get; set; }
        public int? GutterAtLeft { get; set; }
        public int? GutterAtRight { get; set; }
        public int? GutterAtBottom { get; set; }
        public int? GutterStyle { get; set; }
        public bool? GutterStyleLegacy { get; set; }
    }

    public enum SectionType
    {
        Continuous,
        NextPage,
        OddPage,
        EvenPage,
        NextColumn
    }

    public class PageSize
    {
        public int Width { get; set; } = 12240;
        public int Height { get; set; } = 15840;
        public bool? OrientLandscape { get; set; }
        public ushort? CodePage { get; set; }
    }

    public class PageMargin
    {
        public int Top { get; set; } = 1440;
        public int Bottom { get; set; } = 1440;
        public int Left { get; set; } = 1440;
        public int Right { get; set; } = 1440;
        public int? Header { get; set; } = 720;
        public int? Footer { get; set; } = 720;
        public int? Gutter { get; set; } = 0;
        public bool? MirrorMargins { get; set; }
        public bool? AutoAdjustMargins { get; set; } = true;
    }

    public class PaperSource
    {
        public PaperSourceCode First { get; set; } = PaperSourceCode.Auto;
        public PaperSourceCode Default { get; set; } = PaperSourceCode.Auto;
    }

    public enum PaperSourceCode
    {
        Auto,
        Manual,
        Sheet,
        Card,
        Envelope,
        Small108x35,
        Small89x36,
        Wide,
        Tabloid,
        USStandard11x17,
        Letter,
        LetterSmall,
        TabloidExtra,
        USStandard9x11,
        USStandard10x11,
        OnDescription16,
        Personal,
        A4Small,
        A4,
        A4Sheet,
        A5,
        B5,
        DoublePostcard,
        SinglePostcard,
        OnDescription9,
        LetterExtra,
        LegalExtra,
        TabloidExtra,
        A4Extra,
        TransverseLetter,
        TransverseA4,
        LetterPlus,
        A4Plus,
        A5Transverse,
        B5Transverse,
        A3,
        A3Extra,
        QUATRO,
        APL10P,
        DblJapanesePostcard,
        A6,
        JEnvKakemonov4,
        JEnvKakemonov6,
        JEnvChou3,
        JEnvChou4,
        JEnvYou4,
        JEnvKakemonov3,
        JEnvChou2,
        JEnvKakemonov5,
        JEnvKakemonov8,
        JEnvKakemonov10,
        JEnvKakemonov7,
        Custom10x14,
        Custom15x11,
        APL9x11,
        APL12x17,
        APL11x17,
        FanFold,
        Prc16K,
        Prc32K,
        Prc32KBig,
        FanFoldGerman,
        FanFoldGermanLegal,
        B4,
        JapanesePostcard,
        Standard9x11,
        Standard10x15,
        Standard15x19,
        InviteEnvelope,
        LetterExtraTransverse,
        A4Transverse,
        PrcEnvelope1,
        PrcEnvelope2,
        PrcEnvelope3,
        PrcEnvelope4,
        PrcEnvelope5,
        PrcEnvelope6,
        PrcEnvelope7,
        PrcEnvelope8,
        PrcEnvelope9,
        PrcEnvelope10,
        FanFoldSwedish,
        Custom12x19,
        CustomSmallPaper,
        LargePaper,
        TrxLetter,
        A4J,
        8x10,
        A5J,
        JEnvYou6,
        JEnvYou5,
        JEnvYou4Inverted,
        Custom6x9,
        PISheet,
        PU4,
        PU6,
        JPEnv1,
        JPEnv2,
        JPEnv3,
        JPEnv4,
        DblJapanesePostcardRotated,
        A4JRotated,
        A5JRotated,
        B5J,
        B5JRotated,
        B6J,
        B6JRotated,
        Custom16K210x297mm,
        Custom16K216x270mm,
        Custom16K230x300mm,
        Custom8K210x297mm,
        Custom8K270x390mm,
        Custom10x14cm,
        Custom10x15cm,
        Custom15x15cm,
        CustomUSStandardFoolscap
    }

    public class PageBorder
    {
        public BorderDefinition TopBorder { get; set; }
        public BorderDefinition BottomBorder { get; set; }
        public BorderDefinition LeftBorder { get; set; }
        public BorderDefinition RightBorder { get; set; }
        public BorderDefinition BetweenBorder { get; set; }
        public BorderDefinition BarBorder { get; set; }
        public PageBorderOffsetFrom OffsetFrom { get; set; } = PageBorderOffsetFrom.Text;
        public PageBorderSurroundHeader SurroundHeader { get; set; } = PageBorderSurroundHeader.NotSet;
        public PageBorderSurroundFooter SurroundFooter { get; set; } = PageBorderSurroundFooter.NotSet;
    }

    public enum PageBorderOffsetFrom
    {
        Text,
        Margin,
        Page
    }

    public enum PageBorderSurroundHeader
    {
        NotSet,
        On,
        Off
    }

    public enum PageBorderSurroundFooter
    {
        NotSet,
        On,
        Off
    }

    public class LineNumberControl
    {
        public bool CountBy { get; set; } = true;
        public int? Start { get; set; } = 1;
        public int? Distance { get; set; }
        public LineRestart LineRestart { get; set; } = LineRestart.NewSection;
    }

    public enum LineRestart
    {
        NewSection,
        Continuous
    }

    public class PageNumberControl
    {
        public int Format { get; set; } = 0;
        public int Start { get; set; } = 1;
    }

    public class ColumnProperties
    {
        public int ColumnCount { get; set; } = 1;
        public bool EqualWidth { get; set; } = true;
        public List<ColumnDefinition> ColumnDefinitions { get; set; } = new List<ColumnDefinition>();
        public int Space { get; set; } = 720;
        public bool SeparatorLine { get; set; } = false;
        public int? BalanceMultiline { get; set; }
    }

    public class ColumnDefinition
    {
        public int? Width { get; set; }
        public int? Space { get; set; }
    }

    public class FootnoteProperties
    {
        public FootnotePlacement Placement { get; set; } = FootnotePlacement.BottomOfPage;
        public FootnoteNumberingFormat NumberingFormat { get; set; } = FootnoteNumberingFormat.Decimal;
        public FootnoteStartingNumber StartNumber { get; set; } = FootnoteStartingNumber.Default;
        public FootnoteReference RestartPolicy { get; set; } = FootnoteReference.Continuous;
        public FootnoteIntlPolicy IntlPolicy { get; set; } = FootnoteIntlPolicy.Context;
    }

    public enum FootnotePlacement
    {
        BottomOfPage,
        BeneathText,
        PrintEndnotes,
        EndSectOnly
    }

    public enum FootnoteNumberingFormat
    {
        Decimal,
        UpperRoman,
        LowerRoman,
        UpperLetter,
        LowerLetter
    }

    public enum FootnoteStartingNumber
    {
        Default,
        Set
    }

    public enum FootnoteReference
    {
        Continuous,
        RestartSection,
        RestartPage
    }

    public enum FootnoteIntlPolicy
    {
        Context,
        EachColumn,
        Continuous
    }

    public class EndnoteProperties
    {
        public EndnotePlacement Placement { get; set; } = EndnotePlacement.EndOfDocument;
        public EndnoteNumberingFormat NumberingFormat { get; set; } = EndnoteNumberingFormat.LowerRoman;
        public EndnoteStartingNumber StartNumber { get; set; } = EndnoteStartingNumber.Default;
        public EndnoteReference RestartPolicy { get; set; } = EndnoteReference.Continuous;
    }

    public enum EndnotePlacement
    {
        EndOfDocument,
        EndOfSection,
        EndOfPage
    }

    public enum EndnoteNumberingFormat
    {
        Decimal,
        UpperRoman,
        LowerRoman,
        UpperLetter,
        LowerLetter
    }

    public enum EndnoteStartingNumber
    {
        Default,
        Set
    }

    public enum EndnoteReference
    {
        Continuous,
        RestartSection,
        RestartPage
    }

    public enum TextDirection
    {
        LRTB,
        LRTBV,
        LRBT,
        RLtb,
        RLtbv,
        RLBT,
        TBRL,
        TBRLV,
        TBLR,
        TBLRV,
        BTLR,
        BTLRV,
        LRTBGA,
        TBRLA
    }

    public enum VerticalAlignmentVertical
    {
        Top,
        Center,
        Both,
        Bottom
    }

    public class SectionBorders
    {
        public BorderDefinition TopBorder { get; set; }
        public BorderDefinition BottomBorder { get; set; }
        public BorderDefinition LeftBorder { get; set; }
        public BorderDefinition RightBorder { get; set; }
        public BorderDefinition BetweenBorder { get; set; }
        public BorderDefinition BarBorder { get; set; }
        public SectionBordersOffsetFrom OffsetFrom { get; set; } = SectionBordersOffsetFrom.Text;
    }

    public enum SectionBordersOffsetFrom
    {
        Text,
        Margin
    }

    public class DocGrid
    {
        public DocGridType Type { get; set; } = DocGridType.Lines;
        public int? LinePitch { get; set; } = 360;
        public int? CharPitch { get; set; } = 240;
        public DocGridDrawingGridIn DoNotUseMarginsForDrawingGridOrigin { get; set; } = DocGridDrawingGridIn.UseMargins;
        public int? DrawingGridHorizontalOrigin { get; set; }
        public int? DrawingGridVerticalOrigin { get; set; }
        public DocGridHorizontalLine spacing GridDistanceHorizontal { get; set; }
        public DocGridVerticalLineSpacing GridDistanceVertical { get; set; }
        public bool? DisplayHorizontalDrawingGridEvery { get; set; }
        public bool? DisplayVerticalDrawingGridEvery { get; set; }
        public bool? SnapToGridInDrawing { get; set; } = true;
    }

    public enum DocGridType
    {
        Lines,
        LinesAndChars,
        None
    }

    public enum DocGridDrawingGridIn
    {
        UseMargins,
        UseMarginsAndPage,
        IgnoreMargins,
        IgnoreMarginsAndPage
    }

    public enum DocGridHorizontalLineSpacing
    {
        LeftToRight,
        RightToLeft,
        LeftToRightNear,
        RightToLeftNear
    }

    public enum DocGridVerticalLineSpacing
    {
        Down,
        Up,
        DownRight,
        DownLeft
    }

    public class StyleType
    {
        public static int Paragraph { get; set; } = 1;
        public static int Character { get; set; } = 2;
        public static int Table { get; set; } = 3;
        public static int Numbering { get; set; } = 4;
    }

    public class StyleRepository
    {
        private readonly Dictionary<string, ParagraphStyle> _styles = new Dictionary<string, ParagraphStyle>();
        private readonly List<NumberingListDefinition> _numbering = new List<NumberingListDefinition>();

        public StyleRepository()
        {
            InitializeBuiltInStyles();
        }

        private void InitializeBuiltInStyles()
        {
            AddStyle(new ParagraphStyle
            {
                StyleId = "Normal",
                StyleName = "Normal",
                Type = (StyleType)1,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 22
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        AfterLines = 200,
                        Line = 276,
                        LineRule = 1
                    }
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Heading1",
                StyleName = "Heading 1",
                BasedOn = "Normal",
                NextParagraphStyle = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri Light",
                    FontSize = 56,
                    Bold = true,
                    FontColor = "1F3864"
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 480,
                        AfterLines = 200,
                        Line = 276,
                        LineRule = 1
                    },
                    KeepWithNext = true,
                    OutlineLevel = OutlineLevel.Level1
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Heading2",
                StyleName = "Heading 2",
                BasedOn = "Normal",
                NextParagraphStyle = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri Light",
                    FontSize = 44,
                    Bold = true,
                    FontColor = "2E74B5"
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 280,
                        AfterLines = 80,
                        Line = 276,
                        LineRule = 1
                    },
                    KeepWithNext = true,
                    OutlineLevel = OutlineLevel.Level2
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Heading3",
                StyleName = "Heading 3",
                BasedOn = "Normal",
                NextParagraphStyle = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri Light",
                    FontSize = 32,
                    Bold = true,
                    FontColor = "1F497D"
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 200,
                        AfterLines = 80,
                        Line = 276,
                        LineRule = 1
                    },
                    KeepWithNext = true,
                    OutlineLevel = OutlineLevel.Level3
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Heading4",
                StyleName = "Heading 4",
                BasedOn = "Normal",
                NextParagraphStyle = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 28,
                    Bold = true
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 160,
                        AfterLines = 80,
                        Line = 276,
                        LineRule = 1
                    },
                    KeepWithNext = true,
                    OutlineLevel = OutlineLevel.Level4
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Heading5",
                StyleName = "Heading 5",
                BasedOn = "Normal",
                NextParagraphStyle = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 24,
                    Bold = true
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 120,
                        AfterLines = 80,
                        Line = 276,
                        LineRule = 1
                    },
                    KeepWithNext = true,
                    OutlineLevel = OutlineLevel.Level5
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Heading6",
                StyleName = "Heading 6",
                BasedOn = "Normal",
                NextParagraphStyle = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 22,
                    Bold = true,
                    FontColor = "808080"
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 100,
                        AfterLines = 80,
                        Line = 276,
                        LineRule = 1
                    },
                    KeepWithNext = true,
                    OutlineLevel = OutlineLevel.Level6
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Title",
                StyleName = "Title",
                BasedOn = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri Light",
                    FontSize = 72,
                    Bold = false,
                    FontColor = "000000"
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        AfterLines = 120
                    }
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Subtitle",
                StyleName = "Subtitle",
                BasedOn = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 28,
                    FontColor = "5A5A5A"
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    Spacing = new SpacingAfterText
                    {
                        AfterLines = 200
                    }
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "Quote",
                StyleName = "Quote",
                BasedOn = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 22,
                    Italic = true
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    IndentationLeft = 1440,
                    IndentationRight = 1440,
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 120,
                        AfterLines = 120
                    }
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "IntenseQuote",
                StyleName = "Intense Quote",
                BasedOn = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 22,
                    Italic = true,
                    Bold = true
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    IndentationLeft = 1440,
                    IndentationRight = 1440,
                    Spacing = new SpacingAfterText
                    {
                        BeforeLines = 120,
                        AfterLines = 120
                    }
                },
                Shading = new ShadingDefinition
                {
                    Pattern = ShadingPattern.Clear,
                    ForegroundColor = "ThemeAccent1",
                    BackgroundColor = "ThemeAccent1_10"
                }
            });

            AddStyle(new ParagraphStyle
            {
                StyleId = "ListParagraph",
                StyleName = "List Paragraph",
                BasedOn = "Normal",
                PrimaryStyle = true,
                TextFormatting = new TextFormatting
                {
                    FontName = "Calibri",
                    FontSize = 22
                },
                ParagraphFormatting = new ParagraphFormatting
                {
                    IndentationLeft = 720,
                    Spacing = new SpacingAfterText
                    {
                        AfterLines = 120
                    }
                }
            });

            AddNumbering(new NumberingListDefinition
            {
                Id = "BulletList",
                Name = "Bullet List",
                Format = NumberingFormat.Bullet,
                Levels = new NumberingLevelCollection
                {
                    Levels = new List<NumberingLevel>
                    {
                        new NumberingLevel
                        {
                            LevelIndex = 0,
                            LevelText = "\u2022",
                            Format = NumberingFormat.Bullet,
                            TextFormatting = new TextFormatting
                            {
                                FontName = "Symbol"
                            },
                            IndentLeft = 720,
                            IndentHanging = 360
                        },
                        new NumberingLevel
                        {
                            LevelIndex = 1,
                            LevelText = "\u25E6",
                            Format = NumberingFormat.Bullet,
                            TextFormatting = new TextFormatting
                            {
                                FontName = "Courier New"
                            },
                            IndentLeft = 1440,
                            IndentHanging = 360,
                            PreviousLevel = 0
                        },
                        new NumberingLevel
                        {
                            LevelIndex = 2,
                            LevelText = "\u25AA",
                            Format = NumberingFormat.Bullet,
                            TextFormatting = new TextFormatting
                            {
                                FontName = "Wingdings"
                            },
                            IndentLeft = 2160,
                            IndentHanging = 360,
                            PreviousLevel = 1
                        }
                    }
                },
                IsMultiLevel = true
            });

            AddNumbering(new NumberingListDefinition
            {
                Id = "NumberedList",
                Name = "Numbered List",
                Format = NumberingFormat.Decimal,
                Levels = new NumberingLevelCollection
                {
                    Levels = new List<NumberingLevel>
                    {
                        new NumberingLevel
                        {
                            LevelIndex = 0,
                            LevelText = "%1.",
                            Format = NumberingFormat.Decimal,
                            IndentLeft = 720,
                            IndentHanging = 360
                        },
                        new NumberingLevel
                        {
                            LevelIndex = 1,
                            LevelText = "%2.",
                            Format = NumberingFormat.Decimal,
                            IndentLeft = 1440,
                            IndentHanging = 360,
                            PreviousLevel = 0
                        },
                        new NumberingLevel
                        {
                            LevelIndex = 2,
                            LevelText = "%3.",
                            Format = NumberingFormat.Decimal,
                            IndentLeft = 2160,
                            IndentHanging = 360,
                            PreviousLevel = 1
                        }
                    }
                },
                IsMultiLevel = true
            });

            AddNumbering(new NumberingListDefinition
            {
                Id = "OutlineNumbered",
                Name = "Outline Numbered",
                Format = NumberingFormat.Decimal,
                Levels = new NumberingLevelCollection
                {
                    Levels = new List<NumberingLevel>
                    {
                        new NumberingLevel
                        {
                            LevelIndex = 0,
                            LevelText = "%1",
                            Format = NumberingFormat.Decimal,
                            IndentLeft = 720,
                            IndentHanging = 360
                        },
                        new NumberingLevel
                        {
                            LevelIndex = 1,
                            LevelText = "%1.%2",
                            Format = NumberingFormat.Decimal,
                            IndentLeft = 1440,
                            IndentHanging = 360,
                            PreviousLevel = 0
                        },
                        new NumberingLevel
                        {
                            LevelIndex = 2,
                            LevelText = "%1.%2.%3",
                            Format = NumberingFormat.Decimal,
                            IndentLeft = 2160,
                            IndentHanging = 360,
                            PreviousLevel = 1
                        }
                    }
                },
                IsMultiLevel = true
            });
        }

        public void AddStyle(ParagraphStyle style)
        {
            _styles[style.StyleId] = style;
        }

        public ParagraphStyle GetStyle(string styleId)
        {
            return _styles.TryGetValue(styleId, out var style) ? style : null;
        }

        public IEnumerable<ParagraphStyle> GetAllStyles()
        {
            return _styles.Values;
        }

        public void AddNumbering(NumberingListDefinition numbering)
        {
            _numbering.Add(numbering);
        }

        public NumberingListDefinition GetNumbering(string id)
        {
            return _numbering.FirstOrDefault(n => n.Id == id);
        }

        public IEnumerable<NumberingListDefinition> GetAllNumbering()
        {
            return _numbering;
        }

        public XDocument ExportStyles()
        {
            var styles = new List<XElement>();

            foreach (var style in _styles.Values)
            {
                var styleElement = new XElement(W + "style",
                    new XAttribute(W + "type", ((int)style.Type).ToString()),
                    new XAttribute(W + "styleId", style.StyleId),
                    new XElement(W + "name",
                        new XAttribute(W + "val", style.StyleName)),
                    new XElement(W + "basedOn",
                        new XAttribute(W + "val", style.BasedOn ?? "")),
                    new XElement(W + "next",
                        new XAttribute(W + "val", style.NextParagraphStyle ?? "")));

                if (style.PrimaryStyle)
                {
                    styles.Add(new XElement(W + "uiPriority",
                        new XAttribute(W + "val", "1")));
                }

                styles.Add(styleElement);
            }

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(W + "styles",
                    new XAttribute(W + "docDefaults", ""),
                    styles));
        }

        private static XNamespace W => "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    }

    public class PageSetup
    {
        public int Width { get; set; } = 12240;
        public int Height { get; set; } = 15840;
        public int TopMargin { get; set; } = 1440;
        public int BottomMargin { get; set; } = 1440;
        public int LeftMargin { get; set; } = 1440;
        public int RightMargin { get; set; } = 1440;
        public int HeaderMargin { get; set; } = 720;
        public int FooterMargin { get; set; } = 720;
        public int Gutter { get; set; } = 0;
        public bool Landscape { get; set; } = false;
    }

    public class DocumentTheme
    {
        public string ThemeId { get; set; }
        public string Name { get; set; }
        public ThemeColors Colors { get; set; } = new ThemeColors();
        public ThemeFonts Fonts { get; set; } = new ThemeFonts();
        public ThemeFormats Formats { get; set; } = new ThemeFormats();
    }

    public class ThemeColors
    {
        public string Dark1 { get; set; } = "000000";
        public string Light1 { get; set; } = "FFFFFF";
        public string Dark2 { get; set; } = "44546A";
        public string Light2 { get; set; } = "E7E6E6";
        public string Accent1 { get; set; } = "4472C4";
        public string Accent2 { get; set; } = "ED7D31";
        public string Accent3 { get; set; } = "A5A5A5";
        public string Accent4 { get; set; } = "FFC000";
        public string Accent5 { get; set; } = "5B9BD5";
        public string Accent6 { get; set; } = "70AD47";
        public string Hyperlink { get; set; } = "0563C1";
        public string FollowedHyperlink { get; set; } = "954F72";
    }

    public class ThemeFonts
    {
        public string MajorFont { get; set; } = "Calibri Light";
        public string MinorFont { get; set; } = "Calibri";
    }

    public class ThemeFormats
    {
        public Dictionary<string, string> ColorDefinitions { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> FontDefinitions { get; set; } = new Dictionary<string, string>();
    }

    public class TableProperties
    {
        public TableStyle TableStyle { get; set; }
        public TableWidth Width { get; set; } = new TableWidth { Width = "5000", Type = TableWidthUnit.Percent };
        public TableLayout Layout { get; set; } = TableLayout.Auto;
        public TableOverlap Overlap { get; set; } = TableOverlap.Never;
        public bool? AllowOverlap { get; set; }
        public bool HideMark { get; set; } = false;
        public TableVerticalAlignment VerticalAlignment { get; set; } = TableVerticalAlignment.Top;
        public TableBorders Borders { get; set; }
        public SpacingBetweenTables Spacing { get; set; }
        public TableIndentation Indentation { get; set; }
        public TableCellMarginDefault CellMarginDefault { get; set; }
        public int? TableBorderTop { get; set; }
        public int? TableBorderBottom { get; set; }
        public int? TableBorderLeft { get; set; }
        public int? TableBorderRight { get; set; }
        public int? TableBorderInsideH { get; set; }
        public int? TableBorderInsideV { get; set; }
        public bool? RevisionsTableBorder { get; set; }
    }

    public class TableStyle
    {
        public string StyleId { get; set; }
        public string Name { get; set; }
        public int? UiPriority { get; set; }
        public bool PrimaryStyle { get; set; } = false;
        public TableStyleValues TableStyleCondition { get; set; } = TableStyleValues.Table;
    }

    public enum TableStyleValues
    {
        Table,
        Row,
        Cell
    }

    public class TableWidth
    {
        public string Width { get; set; }
        public TableWidthUnit Type { get; set; } = TableWidthUnit.Auto;
    }

    public enum TableWidthUnit
    {
        Auto,
        Percentage,
        Fixed,
        Dxa
    }

    public enum TableLayout
    {
        Auto,
        Fixed
    }

    public enum TableOverlap
    {
        Never,
        Overlap
    }

    public enum TableVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public class TableBorders
    {
        public BorderDefinition TopBorder { get; set; }
        public BorderDefinition BottomBorder { get; set; }
        public BorderDefinition LeftBorder { get; set; }
        public BorderDefinition RightBorder { get; set; }
        public BorderDefinition InsideHBorder { get; set; }
        public BorderDefinition InsideVBorder { get; set; }
        public BorderDefinition DiagonalDownBorder { get; set; }
        public BorderDefinition DiagonalUpBorder { get; set; }
    }

    public class SpacingBetweenTables
    {
        public SpacingBetweenTablesType Type { get; set; } = SpacingBetweenTablesType.Auto;
        public int? Between { get; set; }
        public int? Bottom { get; set; }
    }

    public enum SpacingBetweenTablesType
    {
        Auto,
        Points
    }

    public class TableIndentation
    {
        public int Width { get; set; }
        public TableWidthUnit Type { get; set; } = TableWidthUnit.Dxa;
    }

    public class TableCellMarginDefault
    {
        public TableCellMargin TopMargin { get; set; }
        public TableCellMargin BottomMargin { get; set; }
        public TableCellMargin LeftMargin { get; set; }
        public TableCellMargin RightMargin { get; set; }
        public TableCellMargin TopLeftCellMargin { get; set; }
        public TableCellMargin TopRightCellMargin { get; set; }
        public TableCellMargin BottomLeftCellMargin { get; set; }
        public TableCellMargin BottomRightCellMargin { get; set; }
    }

    public class TableCellMargin
    {
        public int Width { get; set; }
        public TableWidthUnit Type { get; set; } = TableWidthUnit.Dxa;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeSuite.Word.Models
{
    public enum SectionType
    {
        Continuous,
        NextPage,
        OddPage,
        EvenPage,
        NextColumn
    }

    public enum ColumnType
    {
        Single,
        Two,
        Three,
        TwoLeft,
        TwoRight,
        ThreeLeft,
        ThreeRight,
        ThreeCenter,
        Custom
    }

    public class Section
    {
        public int SectionId { get; set; }
        public SectionType SectionType { get; set; }
        public ColumnType ColumnType { get; set; }
        public int ColumnCount { get; set; }
        public double ColumnWidth { get; set; }
        public double ColumnSpacing { get; set; }
        public bool EqualColumnWidth { get; set; }
        public LineNumbering LineNumbering { get; set; }
        public PageSettings PageSettings { get; set; }
        public HeaderFooterCollection Headers { get; set; }
        public HeaderFooterCollection Footers { get; set; }
        public bool DifferentFirstPage { get; set; }
        public bool DifferentOddAndEvenPages { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public Section()
        {
            SectionId = Guid.NewGuid().GetHashCode();
            SectionType = SectionType.NextPage;
            ColumnType = ColumnType.Single;
            ColumnCount = 1;
            ColumnWidth = 6.5;
            ColumnSpacing = 0.5;
            EqualColumnWidth = true;
            LineNumbering = new LineNumbering();
            PageSettings = new PageSettings();
            Headers = new HeaderFooterCollection();
            Footers = new HeaderFooterCollection();
            DifferentFirstPage = false;
            DifferentOddAndEvenPages = false;
        }

        public Section Clone()
        {
            return new Section
            {
                SectionId = this.SectionId,
                SectionType = this.SectionType,
                ColumnType = this.ColumnType,
                ColumnCount = this.ColumnCount,
                ColumnWidth = this.ColumnWidth,
                ColumnSpacing = this.ColumnSpacing,
                EqualColumnWidth = this.EqualColumnWidth,
                LineNumbering = this.LineNumbering?.Clone(),
                PageSettings = this.PageSettings?.Clone(),
                Headers = this.Headers.Clone(),
                Footers = this.Footers.Clone(),
                DifferentFirstPage = this.DifferentFirstPage,
                DifferentOddAndEvenPages = this.DifferentOddAndEvenPages,
                StartIndex = this.StartIndex,
                EndIndex = this.EndIndex
            };
        }
    }

    public class LineNumbering
    {
        public bool IsActive { get; set; }
        public LineNumberingMode Mode { get; set; }
        public int CountBy { get; set; }
        public int StartNumber { get; set; }
        public int Distance { get; set; }
        public int RestartMode { get; set; }

        public LineNumbering()
        {
            IsActive = false;
            Mode = LineNumberingMode.Continuous;
            CountBy = 1;
            StartNumber = 1;
            Distance = 720;
            RestartMode = 0;
        }

        public LineNumbering Clone()
        {
            return new LineNumbering
            {
                IsActive = this.IsActive,
                Mode = this.Mode,
                CountBy = this.CountBy,
                StartNumber = this.StartNumber,
                Distance = this.Distance,
                RestartMode = this.RestartMode
            };
        }
    }

    public enum LineNumberingMode
    {
        Continuous,
        RestartPerPage,
        RestartPerSection,
        RestartPerDocument
    }

    public class HeaderFooterCollection
    {
        public HeaderFooter Default { get; set; }
        public HeaderFooter First { get; set; }
        public HeaderFooter Odd { get; set; }
        public HeaderFooter Even { get; set; }

        public HeaderFooterCollection Clone()
        {
            return new HeaderFooterCollection
            {
                Default = this.Default?.Clone(),
                First = this.First?.Clone(),
                Odd = this.Odd?.Clone(),
                Even = this.Even?.Clone()
            };
        }
    }

    public class HeaderFooter
    {
        public bool IsHeader { get; set; }
        public HeaderFooterType Type { get; set; }
        public List<Paragraph> Content { get; set; }
        public bool LinkToPrevious { get; set; }
        public bool IsLinked { get; set; }
        public double MarginTop { get; set; }
        public double MarginBottom { get; set; }
        public double Height { get; set; }
        public bool FitText { get; set; }

        public HeaderFooter()
        {
            Content = new List<Paragraph>();
            LinkToPrevious = true;
            IsLinked = false;
            MarginTop = 0.5;
            MarginBottom = 0.5;
            Height = 0;
            FitText = false;
        }

        public HeaderFooter Clone()
        {
            return new HeaderFooter
            {
                IsHeader = this.IsHeader,
                Type = this.Type,
                Content = this.Content.Select(p => p.Clone()).ToList(),
                LinkToPrevious = this.LinkToPrevious,
                IsLinked = this.IsLinked,
                MarginTop = this.MarginTop,
                MarginBottom = this.MarginBottom,
                Height = this.Height,
                FitText = this.FitText
            };
        }
    }

    public enum HeaderFooterType
    {
        Default,
        First,
        Even
    }

    public class Footnote
    {
        public int Id { get; set; }
        public int ReferenceMarkId { get; set; }
        public string Text { get; set; }
        public CharacterFormat Format { get; set; }
        public FootnotePosition Position { get; set; }
        public FootnoteNumberingStyle NumberingStyle { get; set; }
        public int StartNumber { get; set; }
        public int RestartIndex { get; set; }
        public FootnoteRestartLocation RestartLocation { get; set; }
        public bool IsAutoNumbered { get; set; }
        public string CustomReference { get; set; }

        public Footnote()
        {
            Id = Guid.NewGuid().GetHashCode();
            Text = string.Empty;
            Format = new CharacterFormat();
            Position = FootnotePosition.BottomOfPage;
            NumberingStyle = FootnoteNumberingStyle.Arabic;
            StartNumber = 1;
            RestartIndex = 1;
            RestartLocation = FootnoteRestartLocation.PerDocument;
            IsAutoNumbered = true;
        }

        public Footnote Clone()
        {
            return new Footnote
            {
                Id = this.Id,
                ReferenceMarkId = this.ReferenceMarkId,
                Text = this.Text,
                Format = this.Format.Clone(),
                Position = this.Position,
                NumberingStyle = this.NumberingStyle,
                StartNumber = this.StartNumber,
                RestartIndex = this.RestartIndex,
                RestartLocation = this.RestartLocation,
                IsAutoNumbered = this.IsAutoNumbered,
                CustomReference = this.CustomReference
            };
        }
    }

    public class Endnote
    {
        public int Id { get; set; }
        public int ReferenceMarkId { get; set; }
        public string Text { get; set; }
        public CharacterFormat Format { get; set; }
        public EndnotePosition Position { get; set; }
        public FootnoteNumberingStyle NumberingStyle { get; set; }
        public int StartNumber { get; set; }
        public int RestartIndex { get; set; }
        public EndnoteRestartLocation RestartLocation { get; set; }
        public bool IsAutoNumbered { get; set; }
        public string CustomReference { get; set; }

        public Endnote()
        {
            Id = Guid.NewGuid().GetHashCode();
            Text = string.Empty;
            Format = new CharacterFormat();
            Position = EndnotePosition.EndOfDocument;
            NumberingStyle = FootnoteNumberingStyle.LowercaseRoman;
            StartNumber = 1;
            RestartIndex = 1;
            RestartLocation = EndnoteRestartLocation.PerDocument;
            IsAutoNumbered = true;
        }

        public Endnote Clone()
        {
            return new Endnote
            {
                Id = this.Id,
                ReferenceMarkId = this.ReferenceMarkId,
                Text = this.Text,
                Format = this.Format.Clone(),
                Position = this.Position,
                NumberingStyle = this.NumberingStyle,
                StartNumber = this.StartNumber,
                RestartIndex = this.RestartIndex,
                RestartLocation = this.RestartLocation,
                IsAutoNumbered = this.IsAutoNumbered,
                CustomReference = this.CustomReference
            };
        }
    }

    public enum FootnotePosition
    {
        BottomOfPage,
        BottomOfText,
        BeneathText,
        EndOfSection,
        EndOfDocument
    }

    public enum EndnotePosition
    {
        EndOfDocument,
        EndOfSection
    }

    public enum FootnoteNumberingStyle
    {
        Arabic,
        UppercaseRoman,
        LowercaseRoman,
        UppercaseLetter,
        LowercaseLetter,
        Star,
        DoubleStar,
        PhysicalSymbol,
        MathematicalSymbol
    }

    public enum FootnoteRestartLocation
    {
        PerDocument,
        PerPage,
        PerSection
    }

    public enum EndnoteRestartLocation
    {
        PerDocument,
        PerSection
    }

    public class Comment
    {
        public int Id { get; set; }
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }
        public string Author { get; set; }
        public string Initials { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }
        public int? ParentCommentId { get; set; }
        public List<Comment> Replies { get; set; }
        public bool IsResolved { get; set; }
        public bool IsDeleted { get; set; }

        public Comment()
        {
            Id = Guid.NewGuid().GetHashCode();
            Author = string.Empty;
            Initials = string.Empty;
            DateTime = DateTime.Now;
            Text = string.Empty;
            Replies = new List<Comment>();
            IsResolved = false;
            IsDeleted = false;
        }

        public Comment Clone()
        {
            return new Comment
            {
                Id = this.Id,
                RangeStart = this.RangeStart,
                RangeEnd = this.RangeEnd,
                Author = this.Author,
                Initials = this.Initials,
                DateTime = this.DateTime,
                Text = this.Text,
                ParentCommentId = this.ParentCommentId,
                Replies = this.Replies.Select(r => r.Clone()).ToList(),
                IsResolved = this.IsResolved,
                IsDeleted = this.IsDeleted
            };
        }
    }

    public class Bookmark
    {
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string NavigateUrl { get; set; }
        public bool IsHidden { get; set; }
        public string ToolTip { get; set; }

        public Bookmark()
        {
            Name = string.Empty;
            StartIndex = 0;
            EndIndex = 0;
            IsHidden = false;
            ToolTip = string.Empty;
        }

        public Bookmark Clone()
        {
            return new Bookmark
            {
                Name = this.Name,
                StartIndex = this.StartIndex,
                EndIndex = this.EndIndex,
                NavigateUrl = this.NavigateUrl,
                IsHidden = this.IsHidden,
                ToolTip = this.ToolTip
            };
        }
    }

    public class Hyperlink
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Url { get; set; }
        public string DisplayText { get; set; }
        public string ToolTip { get; set; }
        public HyperlinkTarget Target { get; set; }
        public bool IsExternal { get; set; }
        public string FrameName { get; set; }

        public Hyperlink()
        {
            Url = string.Empty;
            DisplayText = string.Empty;
            ToolTip = string.Empty;
            Target = HyperlinkTarget.Top;
            IsExternal = true;
            FrameName = string.Empty;
        }

        public Hyperlink Clone()
        {
            return new Hyperlink
            {
                StartIndex = this.StartIndex,
                EndIndex = this.EndIndex,
                Url = this.Url,
                DisplayText = this.DisplayText,
                ToolTip = this.ToolTip,
                Target = this.Target,
                IsExternal = this.IsExternal,
                FrameName = this.FrameName
            };
        }
    }

    public enum HyperlinkTarget
    {
        Top,
        Self,
        Blank,
        Parent,
        FrameName
    }

    public class PageBreak
    {
        public int Position { get; set; }
        public PageBreakType Type { get; set; }

        public PageBreak()
        {
            Type = PageBreakType.Hard;
        }
    }

    public enum PageBreakType
    {
        Hard,
        Soft,
        ColumnBreak,
        SectionNextPage,
        SectionContinuous,
        SectionOddPage,
        SectionEvenPage
    }

    public class TextBox
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public TextBoxWrapMode WrapMode { get; set; }
        public HorizontalAlignment TextHorizontalAlignment { get; set; }
        public VerticalAlignment TextVerticalAlignment { get; set; }
        public bool IsLocked { get; set; }
        public bool CanGrow { get; set; }
        public bool CanShrink { get; set; }
        public CharacterFormat Format { get; set; }
        public ParagraphFormat ParagraphFormat { get; set; }
        public FillFormat FillFormat { get; set; }
        public LineFormat LineFormat { get; set; }
        public Margin Margin { get; set; }

        public TextBox()
        {
            Id = Guid.NewGuid().GetHashCode();
            Text = string.Empty;
            Width = 100;
            Height = 50;
            Left = 0;
            Top = 0;
            WrapMode = TextBoxWrapMode.Square;
            TextHorizontalAlignment = HorizontalAlignment.Left;
            TextVerticalAlignment = VerticalAlignment.Top;
            IsLocked = false;
            CanGrow = true;
            CanShrink = false;
            Format = new CharacterFormat();
            ParagraphFormat = new ParagraphFormat();
            FillFormat = new FillFormat();
            LineFormat = new LineFormat();
            Margin = new Margin();
        }

        public TextBox Clone()
        {
            return new TextBox
            {
                Id = this.Id,
                Text = this.Text,
                Width = this.Width,
                Height = this.Height,
                Left = this.Left,
                Top = this.Top,
                WrapMode = this.WrapMode,
                TextHorizontalAlignment = this.TextHorizontalAlignment,
                TextVerticalAlignment = this.TextVerticalAlignment,
                IsLocked = this.IsLocked,
                CanGrow = this.CanGrow,
                CanShrink = this.CanShrink,
                Format = this.Format.Clone(),
                ParagraphFormat = this.ParagraphFormat.Clone(),
                FillFormat = this.FillFormat.Clone(),
                LineFormat = this.LineFormat.Clone(),
                Margin = this.Margin.Clone()
            };
        }
    }

    public enum TextBoxWrapMode
    {
        Square,
        Tight,
        Through,
        TopBottom,
        None
    }

    public class Margin
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }

        public Margin()
        {
            Top = 0;
            Bottom = 0;
            Left = 0.1;
            Right = 0.1;
        }

        public Margin Clone()
        {
            return new Margin
            {
                Top = this.Top,
                Bottom = this.Bottom,
                Left = this.Left,
                Right = this.Right
            };
        }
    }

    public class TableOfFigures
    {
        public string CaptionLabel { get; set; }
        public string Title { get; set; }
        public string Format { get; set; }
        public bool IncludeLabelAndNumber { get; set; }
        public bool IncludeCaption { get; set; }
        public bool IncludePageNumber { get; set; }
        public PageNumberFormat PageNumberFormat { get; set; }
        public bool UseOutlineLevels { get; set; }
        public int LevelFrom { get; set; }
        public int LevelTo { get; set; }
        public string Separator { get; set; }
    }

    public class TableOfContents
    {
        public string Title { get; set; }
        public bool IncludePageNumbers { get; set; }
        public bool IncludeOutlineLevels { get; set; }
        public bool UseOutlineLevels { get; set; }
        public bool UseHyperlinks { get; set; }
        public int TabLeader { get; set; }
        public int RightTabPos { get; set; }
        public bool UseApplyStyles { get; set; }
        public int BuiltInStyle { get; set; }
        public int LevelFrom { get; set; }
        public int LevelTo { get; set; }
        public bool ShowPageNumbers { get; set; }
        public bool ShowLevels { get; set; }
        public string TabStop { get; set; }
    }

    public class Field
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public FieldType Type { get; set; }
        public string Code { get; set; }
        public string Result { get; set; }
        public bool IsDirty { get; set; }
        public bool IsLocked { get; set; }
        public bool PreserveFormatting { get; set; }

        public Field()
        {
            Type = FieldType.None;
            Code = string.Empty;
            Result = string.Empty;
            IsDirty = false;
            IsLocked = false;
            PreserveFormatting = true;
        }

        public Field Clone()
        {
            return new Field
            {
                StartIndex = this.StartIndex,
                EndIndex = this.EndIndex,
                Type = this.Type,
                Code = this.Code,
                Result = this.Result,
                IsDirty = this.IsDirty,
                IsLocked = this.IsLocked,
                PreserveFormatting = this.PreserveFormatting
            };
        }
    }

    public enum FieldType
    {
        None,
        Date,
        Time,
        PageNumber,
        PageCount,
        LineNumber,
        WordCount,
        AutoNum,
        AutoNumOut,
        AutoNumLegal,
        AutoNumArabic,
        AutoNumRomanUpper,
        AutoNumRomanLower,
        AutoNumLetterUpper,
        AutoNumLetterLower,
        Title,
        Author,
        Subject,
        Keywords,
        Comments,
        LastSavedBy,
        CreationDate,
        LastSavedDate,
        EditTime,
        NumberOfPages,
        NumberOfWords,
        NumberOfCharacters,
        NumberOfCharactersWithSpaces,
        FileName,
        FileSize,
        Template,
        Directory,
        FullFileName,
        Hyperlink,
        Email,
        MergeField,
        If,
        Compare,
        DocVariable,
        DocProperty,
        Section,
        SectionPages,
        Quote,
        StyleRef,
        PrintDate,
        GoToButton,
        MacroButton,
        Include,
        IncludePicture,
        IncludeText,
        Shape,
        Embed,
        FormCheckBox,
        FormDropDown,
        FormText,
        Advance,
        Print,
        Eq,
        Symbol,
        Annotation,
        Private,
        Database,
        XE,
        TC,
        RD,
        BIB,
        FILLIN,
        ASK,
        SET,
        INCLUDETEXT,
        MERGEFIELD,
        MERGEREC,
        MERGESEQ,
        NEXT,
        NEXTIF,
        SKIPIF,
        IF,
        DATAFIELD,
        PAGEREF,
        REF,
        FOOTNOTE,
        FOOTNOTEREF,
        ENDNOTE,
        ENDNOTEREF,
        INDEX,
        TOC,
        SEQ,
        NOTEREF,
        INCLUDETEXT
    }

    public class MailMerge
    {
        public MailMergeDataSource DataSource { get; set; }
        public MailMergeQuery Query { get; set; }
        public List<MailMergeField> Fields { get; set; }
        public MailMergeType Type { get; set; }
        public MailMergeDestination Destination { get; set; }
        public string OutputFileName { get; set; }
        public bool IsActive { get; set; }

        public MailMerge()
        {
            DataSource = new MailMergeDataSource();
            Query = new MailMergeQuery();
            Fields = new List<MailMergeField>();
            Type = MailMergeType.FormLetter;
            Destination = MailMergeDestination.NewDocument;
            IsActive = false;
        }
    }

    public class MailMergeDataSource
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public string QueryString { get; set; }
        public bool HeaderOnFirstPage { get; set; }
        public int CurrentRecord { get; set; }
        public int RecordCount { get; set; }
        public List<string> FieldNames { get; set; }

        public MailMergeDataSource()
        {
            FieldNames = new List<string>();
            CurrentRecord = 0;
            RecordCount = 0;
        }
    }

    public class MailMergeQuery
    {
        public string WhereClause { get; set; }
        public string SortBy { get; set; }
        public bool SortAscending { get; set; }
        public string Filter { get; set; }

        public MailMergeQuery()
        {
            SortAscending = true;
        }
    }

    public class MailMergeField
    {
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string DefaultText { get; set; }
        public bool IsMapped { get; set; }
        public string MappedName { get; set; }
    }

    public enum MailMergeType
    {
        FormLetter,
        Email,
        Fax,
        Catalog
    }

    public enum MailMergeDestination
    {
        NewDocument,
        Printer,
        Email,
        Fax
    }

    public class TrackChanges
    {
        public bool IsTracking { get; set; }
        public TrackRevisionMode Mode { get; set; }
        public bool ShowMarkup { get; set; }
        public bool ShowInsertions { get; set; }
        public bool ShowDeletions { get; set; }
        public bool ShowFormatting { get; set; }
        public bool ShowComments { get; set; }
        public bool ShowInkAnnotations { get; set; }
        public string TrackRevisionsView { get; set; }
        public List<Revision> Revisions { get; set; }

        public TrackChanges()
        {
            IsTracking = false;
            Mode = TrackRevisionMode.Markup;
            ShowMarkup = true;
            ShowInsertions = true;
            ShowDeletions = true;
            ShowFormatting = true;
            ShowComments = true;
            ShowInkAnnotations = true;
            Revisions = new List<Revision>();
        }
    }

    public enum TrackRevisionMode
    {
        NoMarkup,
        SimpleMarkup,
        AllMarkup
    }

    public class Revision
    {
        public int Id { get; set; }
        public RevisionType Type { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string OriginalText { get; set; }
        public string NewText { get; set; }
        public string Author { get; set; }
        public DateTime DateTime { get; set; }
        public RevisionAction Action { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }

        public Revision()
        {
            Id = Guid.NewGuid().GetHashCode();
            Author = string.Empty;
            DateTime = DateTime.Now;
            IsAccepted = false;
            IsRejected = false;
        }
    }

    public enum RevisionType
    {
        Insertion,
        Deletion,
        FormatChange,
        StyleDefinition,
        ParagraphNumber,
        Move,
        AutoFormat
    }

    public enum RevisionAction
    {
        Insert,
        Delete,
        Accept,
        Reject
    }

    public enum PageNumberFormat
    {
        Arabic,
        ArabicDash,
        UppercaseRoman,
        LowercaseRoman,
        UppercaseLetter,
        LowercaseLetter,
        CardText1,
        CardText2,
        CardText3,
        CardText4,
        Ordinal,
        OrdinalText,
        FarEastArabic,
        HalfWidthArabic,
        FullWidthArabic,
        Legal,
        NoNumber,
        Hex
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
        Justify,
        Distribute
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Justify
    }

    public class FillFormat
    {
        public FillType Type { get; set; }
        public string Color { get; set; }
        public string Pattern { get; set; }
        public string PatternColor { get; set; }
        public string GradientColor1 { get; set; }
        public string GradientColor2 { get; set; }
        public double GradientAngle { get; set; }
        public GradientType GradientType { get; set; }
        public double Opacity { get; set; }
        public FillBackground Background { get; set; }

        public FillFormat()
        {
            Type = FillType.Solid;
            Color = "#FFFFFF";
            Pattern = string.Empty;
            PatternColor = "#000000";
            GradientColor1 = "#FFFFFF";
            GradientColor2 = "#000000";
            GradientAngle = 0;
            GradientType = GradientType.Linear;
            Opacity = 1.0;
            Background = new FillBackground();
        }

        public FillFormat Clone()
        {
            return new FillFormat
            {
                Type = this.Type,
                Color = this.Color,
                Pattern = this.Pattern,
                PatternColor = this.PatternColor,
                GradientColor1 = this.GradientColor1,
                GradientColor2 = this.GradientColor2,
                GradientAngle = this.GradientAngle,
                GradientType = this.GradientType,
                Opacity = this.Opacity,
                Background = this.Background.Clone()
            };
        }
    }

    public enum FillType
    {
        None,
        Solid,
        Pattern,
        Gradient,
        Picture,
        Texture
    }

    public enum GradientType
    {
        Linear,
        Radial,
        Rectangular,
        Path
    }

    public class FillBackground
    {
        public string Color { get; set; }
        public string Tint { get; set; }
        public string Shade { get; set; }

        public FillBackground()
        {
            Color = string.Empty;
            Tint = string.Empty;
            Shade = string.Empty;
        }

        public FillBackground Clone()
        {
            return new FillBackground
            {
                Color = this.Color,
                Tint = this.Tint,
                Shade = this.Shade
            };
        }
    }

    public class LineFormat
    {
        public LineStyle Style { get; set; }
        public string Color { get; set; }
        public double Width { get; set; }
        public bool IsVisible { get; set; }
        public double Opacity { get; set; }
        public LineCapStyle CapStyle { get; set; }
        public LineJoinStyle JoinStyle { get; set; }
        public CompoundType CompoundType { get; set; }
        public DashStyle DashStyle { get; set; }
        public double[] DashPattern { get; set; }
        public ShadowFormat Shadow { get; set; }

        public LineFormat()
        {
            Style = LineStyle.Solid;
            Color = "#000000";
            Width = 0.75;
            IsVisible = true;
            Opacity = 1.0;
            CapStyle = LineCapStyle.Flat;
            JoinStyle = LineJoinStyle.Round;
            CompoundType = CompoundType.Simple;
            DashStyle = DashStyle.Solid;
            DashPattern = new double[] { };
            Shadow = new ShadowFormat();
        }

        public LineFormat Clone()
        {
            return new LineFormat
            {
                Style = this.Style,
                Color = this.Color,
                Width = this.Width,
                IsVisible = this.IsVisible,
                Opacity = this.Opacity,
                CapStyle = this.CapStyle,
                JoinStyle = this.JoinStyle,
                CompoundType = this.CompoundType,
                DashStyle = this.DashStyle,
                DashPattern = (double[])this.DashPattern.Clone(),
                Shadow = this.Shadow.Clone()
            };
        }
    }

    public enum LineStyle
    {
        None,
        Single,
        Double,
        Thick,
        Hair,
        Dot,
        Dash,
        DashDot,
        DashDotDot,
        Triple,
        ThickThin,
        ThinThick,
        Triple,
        SquareDot,
        CompactDot,
        Solid,
        DarkGray,
        MediumGray,
        LightGray
    }

    public enum LineCapStyle
    {
        Flat,
        Round,
        Square
    }

    public enum LineJoinStyle
    {
        Round,
        Bevel,
        Miter
    }

    public enum CompoundType
    {
        Simple,
        Double,
        ThickThin,
        ThinThick,
        Triple
    }

    public enum DashStyle
    {
        Solid,
        Dash,
        Dot,
        DashDot,
        LongDash,
        LongDashDot,
        LongDashDotDot
    }

    public class ShadowFormat
    {
        public bool IsVisible { get; set; }
        public string Color { get; set; }
        public double Opacity { get; set; }
        public double BlurRadius { get; set; }
        public double Angle { get; set; }
        public double Distance { get; set; }
        public double Size { get; set; }
        public ShadowType Type { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public ShadowFormat()
        {
            IsVisible = false;
            Color = "#000000";
            Opacity = 0.5;
            BlurRadius = 4;
            Angle = 315;
            Distance = 2;
            Size = 100;
            Type = ShadowType.Single;
        }

        public ShadowFormat Clone()
        {
            return new ShadowFormat
            {
                IsVisible = this.IsVisible,
                Color = this.Color,
                Opacity = this.Opacity,
                BlurRadius = this.BlurRadius,
                Angle = this.Angle,
                Distance = this.Distance,
                Size = this.Size,
                Type = this.Type,
                OffsetX = this.OffsetX,
                OffsetY = this.OffsetY
            };
        }
    }

    public enum ShadowType
    {
        Single,
        Double,
        Outer,
        Inner,
        Perspective
    }
}

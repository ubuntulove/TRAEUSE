using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeSuite.Word.Models
{
    public class Table
    {
        public int Id { get; set; }
        public List<TableRow> Rows { get; set; }
        public int ColumnCount { get; set; }
        public TableProperties Properties { get; set; }
        public TableStyle TableStyle { get; set; }
        public bool IsLinkedToStyle { get; set; }
        public string StyleName { get; set; }

        public Table()
        {
            Id = Guid.NewGuid().GetHashCode();
            Rows = new List<TableRow>();
            Properties = new TableProperties();
            IsLinkedToStyle = true;
            StyleName = "Table Grid";
        }

        public TableCell GetCell(int row, int column)
        {
            if (row < 0 || row >= Rows.Count)
                return null;
            if (column < 0 || column >= Rows[row].Cells.Count)
                return null;
            return Rows[row].Cells[column];
        }

        public void InsertRow(int position)
        {
            var newRow = new TableRow();
            for (int i = 0; i < ColumnCount; i++)
            {
                newRow.Cells.Add(new TableCell());
            }
            Rows.Insert(position, newRow);
        }

        public void InsertColumn(int position)
        {
            foreach (var row in Rows)
            {
                var newCell = new TableCell();
                row.Cells.Insert(position, newCell);
            }
            ColumnCount++;
        }

        public void DeleteRow(int position)
        {
            if (position >= 0 && position < Rows.Count)
            {
                Rows.RemoveAt(position);
            }
        }

        public void DeleteColumn(int position)
        {
            if (position >= 0 && position < ColumnCount)
            {
                foreach (var row in Rows)
                {
                    if (position < row.Cells.Count)
                    {
                        row.Cells.RemoveAt(position);
                    }
                }
                ColumnCount--;
            }
        }

        public Table Clone()
        {
            return new Table
            {
                Id = this.Id,
                Rows = this.Rows.Select(r => r.Clone()).ToList(),
                ColumnCount = this.ColumnCount,
                Properties = this.Properties.Clone(),
                TableStyle = this.TableStyle?.Clone(),
                IsLinkedToStyle = this.IsLinkedToStyle,
                StyleName = this.StyleName
            };
        }
    }

    public class TableRow
    {
        public int RowIndex { get; set; }
        public List<TableCell> Cells { get; set; }
        public RowProperties Properties { get; set; }
        public bool IsHeader { get; set; }
        public bool IsFooter { get; set; }

        public TableRow()
        {
            Cells = new List<TableCell>();
            Properties = new RowProperties();
        }

        public TableRow Clone()
        {
            return new TableRow
            {
                RowIndex = this.RowIndex,
                Cells = this.Cells.Select(c => c.Clone()).ToList(),
                Properties = this.Properties.Clone(),
                IsHeader = this.IsHeader,
                IsFooter = this.IsFooter
            };
        }
    }

    public class TableCell
    {
        public int ColumnIndex { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
        public CellProperties Properties { get; set; }
        public bool IsMerged { get; set; }
        public MergeDirection MergeDirection { get; set; }
        public bool IsFirstInMerge { get; set; }
        public bool IsLastInMerge { get; set; }

        public TableCell()
        {
            Paragraphs = new List<Paragraph> { new Paragraph() };
            Properties = new CellProperties();
            RowSpan = 1;
            ColumnSpan = 1;
        }

        public string GetText()
        {
            return string.Join("\n", Paragraphs.Select(p => p.GetText()));
        }

        public void SetText(string text)
        {
            if (Paragraphs.Count == 0)
                Paragraphs.Add(new Paragraph());
            Paragraphs[0].Runs.Clear();
            Paragraphs[0].Runs.Add(new Run { Text = text });
        }

        public TableCell Clone()
        {
            return new TableCell
            {
                ColumnIndex = this.ColumnIndex,
                RowSpan = this.RowSpan,
                ColumnSpan = this.ColumnSpan,
                Paragraphs = this.Paragraphs.Select(p => p.Clone()).ToList(),
                Properties = this.Properties.Clone(),
                IsMerged = this.IsMerged,
                MergeDirection = this.MergeDirection,
                IsFirstInMerge = this.IsFirstInMerge,
                IsLastInMerge = this.IsLastInMerge
            };
        }
    }

    public enum MergeDirection
    {
        None,
        Vertical,
        Horizontal,
        Both
    }

    public class TableProperties
    {
        public double Width { get; set; }
        public TableWidthUnit WidthUnit { get; set; }
        public HorizontalAlignment Alignment { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double TopMargin { get; set; }
        public double BottomMargin { get; set; }
        public bool AllowOverlap { get; set; }
        public bool IsFloating { get; set; }
        public HorizontalAnchor HorizontalAnchor { get; set; }
        public VerticalAnchor VerticalAnchor { get; set; }
        public double HorizontalPosition { get; set; }
        public double VerticalPosition { get; set; }
        public bool TextWrap { get; set; }
        public WrapType WrapType { get; set; }
        public double BetweenSpace { get; set; }
        public double BottomPadding { get; set; }
        public double TopPadding { get; set; }
        public Shading Shading { get; set; }
        public TableBorders Borders { get; set; }

        public TableProperties()
        {
            Width = 0;
            WidthUnit = TableWidthUnit.Auto;
            Alignment = HorizontalAlignment.Left;
            LeftMargin = 0;
            RightMargin = 0;
            TopMargin = 0;
            BottomMargin = 0;
            AllowOverlap = false;
            IsFloating = false;
            HorizontalAnchor = HorizontalAnchor.Margin;
            VerticalAnchor = VerticalAnchor.Paragraph;
            HorizontalPosition = 0;
            VerticalPosition = 0;
            TextWrap = true;
            WrapType = WrapType.Both;
            BetweenSpace = 0;
            BottomPadding = 0;
            TopPadding = 0;
            Shading = new Shading();
            Borders = new TableBorders();
        }

        public TableProperties Clone()
        {
            return new TableProperties
            {
                Width = this.Width,
                WidthUnit = this.WidthUnit,
                Alignment = this.Alignment,
                LeftMargin = this.LeftMargin,
                RightMargin = this.RightMargin,
                TopMargin = this.TopMargin,
                BottomMargin = this.BottomMargin,
                AllowOverlap = this.AllowOverlap,
                IsFloating = this.IsFloating,
                HorizontalAnchor = this.HorizontalAnchor,
                VerticalAnchor = this.VerticalAnchor,
                HorizontalPosition = this.HorizontalPosition,
                VerticalPosition = this.VerticalPosition,
                TextWrap = this.TextWrap,
                WrapType = this.WrapType,
                BetweenSpace = this.BetweenSpace,
                BottomPadding = this.BottomPadding,
                TopPadding = this.TopPadding,
                Shading = this.Shading.Clone(),
                Borders = this.Borders.Clone()
            };
        }
    }

    public enum TableWidthUnit
    {
        Auto,
        Percentage,
        Twips
    }

    public enum HorizontalAnchor
    {
        Margin,
        Page,
        Column
    }

    public enum VerticalAnchor
    {
        Margin,
        Page,
        Paragraph
    }

    public enum WrapType
    {
        None,
        Both,
        Left,
        Right,
        Largest
    }

    public class RowProperties
    {
        public double Height { get; set; }
        public HeightRule HeightRule { get; set; }
        public bool IsHeader { get; set; }
        public bool IsFooter { get; set; }
        public bool IsLastRow { get; set; }
        public bool IsFirstRow { get; set; }
        public bool CanSplit { get; set; }
        public Shading Shading { get; set; }
        public TableCellVerticalAlignment CellVerticalAlignment { get; set; }

        public RowProperties()
        {
            Height = 0;
            HeightRule = HeightRule.Auto;
            IsHeader = false;
            IsFooter = false;
            IsLastRow = false;
            IsFirstRow = false;
            CanSplit = true;
            Shading = new Shading();
            CellVerticalAlignment = TableCellVerticalAlignment.Top;
        }

        public RowProperties Clone()
        {
            return new RowProperties
            {
                Height = this.Height,
                HeightRule = this.HeightRule,
                IsHeader = this.IsHeader,
                IsFooter = this.IsFooter,
                IsLastRow = this.IsLastRow,
                IsFirstRow = this.IsFirstRow,
                CanSplit = this.CanSplit,
                Shading = this.Shading.Clone(),
                CellVerticalAlignment = this.CellVerticalAlignment
            };
        }
    }

    public enum HeightRule
    {
        Auto,
        AtLeast,
        Exactly
    }

    public class CellProperties
    {
        public double Width { get; set; }
        public TableWidthUnit WidthUnit { get; set; }
        public Shading Shading { get; set; }
        public CellBorders Borders { get; set; }
        public double TopPadding { get; set; }
        public double BottomPadding { get; set; }
        public double LeftPadding { get; set; }
        public double RightPadding { get; set; }
        public TableCellVerticalAlignment VerticalAlignment { get; set; }
        public string PreferredWidth { get; set; }

        public CellProperties()
        {
            Width = 0;
            WidthUnit = TableWidthUnit.Auto;
            Shading = new Shading();
            Borders = new CellBorders();
            TopPadding = 0;
            BottomPadding = 0;
            LeftPadding = 0.08;
            RightPadding = 0.08;
            VerticalAlignment = TableCellVerticalAlignment.Top;
            PreferredWidth = "0";
        }

        public CellProperties Clone()
        {
            return new CellProperties
            {
                Width = this.Width,
                WidthUnit = this.WidthUnit,
                Shading = this.Shading.Clone(),
                Borders = this.Borders.Clone(),
                TopPadding = this.TopPadding,
                BottomPadding = this.BottomPadding,
                LeftPadding = this.LeftPadding,
                RightPadding = this.RightPadding,
                VerticalAlignment = this.VerticalAlignment,
                PreferredWidth = this.PreferredWidth
            };
        }
    }

    public enum TableCellVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public class Shading
    {
        public ShadingType Type { get; set; }
        public string Color { get; set; }
        public string FillColor { get; set; }
        public string Pattern { get; set; }
        public string PatternColor { get; set; }
        public double Opacity { get; set; }

        public Shading()
        {
            Type = ShadingType.Clear;
            Color = "auto";
            FillColor = string.Empty;
            Pattern = string.Empty;
            PatternColor = "auto";
            Opacity = 1.0;
        }

        public Shading Clone()
        {
            return new Shading
            {
                Type = this.Type,
                Color = this.Color,
                FillColor = this.FillColor,
                Pattern = this.Pattern,
                PatternColor = this.PatternColor,
                Opacity = this.Opacity
            };
        }
    }

    public enum ShadingType
    {
        Clear,
        Solid,
        HorizontalStripe,
        VerticalStripe,
        DiagonalUp,
        DiagonalDown,
        Cross,
        ReverseDiagonal,
        HorizontalStripe,
        DiagonalStripe,
        ThickHorizontalStripe,
        ThickVerticalStripe,
        ThickDiagonalStripe,
        ThickCross,
        ThickReverseDiagonal
    }

    public class TableBorders
    {
        public Border TopBorder { get; set; }
        public Border BottomBorder { get; set; }
        public Border LeftBorder { get; set; }
        public Border RightBorder { get; set; }
        public Border InsideHorizontalBorder { get; set; }
        public Border InsideVerticalBorder { get; set; }
        public Border DiagonalBorder { get; set; }
        public bool HasAllBorders { get; set; }

        public TableBorders()
        {
            TopBorder = new Border();
            BottomBorder = new Border();
            LeftBorder = new Border();
            RightBorder = new Border();
            InsideHorizontalBorder = new Border();
            InsideVerticalBorder = new Border();
            DiagonalBorder = new Border();
            HasAllBorders = true;
        }

        public TableBorders Clone()
        {
            return new TableBorders
            {
                TopBorder = this.TopBorder.Clone(),
                BottomBorder = this.BottomBorder.Clone(),
                LeftBorder = this.LeftBorder.Clone(),
                RightBorder = this.RightBorder.Clone(),
                InsideHorizontalBorder = this.InsideHorizontalBorder.Clone(),
                InsideVerticalBorder = this.InsideVerticalBorder.Clone(),
                DiagonalBorder = this.DiagonalBorder.Clone(),
                HasAllBorders = this.HasAllBorders
            };
        }
    }

    public class CellBorders
    {
        public Border TopBorder { get; set; }
        public Border BottomBorder { get; set; }
        public Border LeftBorder { get; set; }
        public Border RightBorder { get; set; }
        public Border DiagonalBorder { get; set; }
        public Border DiagonalUpBorder { get; set; }
        public Border DiagonalDownBorder { get; set; }

        public CellBorders()
        {
            TopBorder = new Border();
            BottomBorder = new Border();
            LeftBorder = new Border();
            RightBorder = new Border();
            DiagonalBorder = new Border();
            DiagonalUpBorder = new Border();
            DiagonalDownBorder = new Border();
        }

        public CellBorders Clone()
        {
            return new CellBorders
            {
                TopBorder = this.TopBorder.Clone(),
                BottomBorder = this.BottomBorder.Clone(),
                LeftBorder = this.LeftBorder.Clone(),
                RightBorder = this.RightBorder.Clone(),
                DiagonalBorder = this.DiagonalBorder.Clone(),
                DiagonalUpBorder = this.DiagonalUpBorder.Clone(),
                DiagonalDownBorder = this.DiagonalDownBorder.Clone()
            };
        }
    }

    public class Border
    {
        public string Color { get; set; }
        public double Size { get; set; }
        public BorderStyle Style { get; set; }
        public double Width { get; set; }
        public bool IsVisible { get; set; }
        public string Space { get; set; }

        public Border()
        {
            Color = "auto";
            Size = 4;
            Style = BorderStyle.Single;
            Width = 0.5;
            IsVisible = true;
            Space = "4";
        }

        public Border Clone()
        {
            return new Border
            {
                Color = this.Color,
                Size = this.Size,
                Style = this.Style,
                Width = this.Width,
                IsVisible = this.IsVisible,
                Space = this.Space
            };
        }
    }

    public enum BorderStyle
    {
        None,
        Single,
        Thick,
        Double,
        Hairline,
        Dotted,
        Dashed,
        DashDot,
        DashDotDot,
        Triple,
        ThickThin,
        ThinThick,
        ThickBetweenThin,
        Outset,
        Inset,
        TVertical,
        THorizontal,
        TDiagonalDown,
        TDiagonalUp,
        Curved,
        Rounded
    }

    public class TableStyle
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool IsLinked { get; set; }
        public string LinkStyleName { get; set; }
        public string StyleId { get; set; }
        public bool IsPrimary { get; set; }
        public Shading HeaderRowShading { get; set; }
        public Shading DataRowShading { get; set; }
        public Shading LastRowShading { get; set; }
        public Shading FirstColumnShading { get; set; }
        public Shading LastColumnShading { get; set; }
        public bool HasBandRows { get; set; }
        public bool HasBandColumns { get; set; }
        public TableStyleRowSettings RowSettings { get; set; }
        public TableStyleCellSettings CellSettings { get; set; }

        public TableStyle()
        {
            Name = "Custom Table Style";
            IsDefault = false;
            IsLinked = false;
            IsPrimary = true;
            HeaderRowShading = new Shading();
            DataRowShading = new Shading();
            LastRowShading = new Shading();
            FirstColumnShading = new Shading();
            LastColumnShading = new Shading();
            RowSettings = new TableStyleRowSettings();
            CellSettings = new TableStyleCellSettings();
        }

        public TableStyle Clone()
        {
            return new TableStyle
            {
                Name = this.Name,
                IsDefault = this.IsDefault,
                IsLinked = this.IsLinked,
                LinkStyleName = this.LinkStyleName,
                StyleId = this.StyleId,
                IsPrimary = this.IsPrimary,
                HeaderRowShading = this.HeaderRowShading.Clone(),
                DataRowShading = this.DataRowShading.Clone(),
                LastRowShading = this.LastRowShading.Clone(),
                FirstColumnShading = this.FirstColumnShading.Clone(),
                LastColumnShading = this.LastColumnShading.Clone(),
                HasBandRows = this.HasBandRows,
                HasBandColumns = this.HasBandColumns,
                RowSettings = this.RowSettings.Clone(),
                CellSettings = this.CellSettings.Clone()
            };
        }
    }

    public class TableStyleRowSettings
    {
        public bool IsHeaderRow { get; set; }
        public bool IsTotalRow { get; set; }
        public bool IsFirstRow { get; set; }
        public bool IsLastRow { get; set; }
        public bool IsBandRow { get; set; }
        public HeightRule HeightRule { get; set; }
        public double Height { get; set; }
    }

    public class TableStyleCellSettings
    {
        public bool IsFirstColumn { get; set; }
        public bool IsLastColumn { get; set; }
        public bool IsBandColumn { get; set; }
        public bool IsSelected { get; set; }
        public TableCellVerticalAlignment VerticalAlignment { get; set; }
    }

    public class ListItem
    {
        public int Level { get; set; }
        public string Text { get; set; }
        public int Number { get; set; }
        public ListItemType ListType { get; set; }
        public string NumberingFormat { get; set; }
        public string NumberingStyle { get; set; }
        public int StartAtNumber { get; set; }
        public List<ListItem> SubItems { get; set; }

        public ListItem()
        {
            Level = 0;
            Text = string.Empty;
            Number = 1;
            ListType = ListItemType.Bullet;
            SubItems = new List<ListItem>();
        }

        public ListItem Clone()
        {
            return new ListItem
            {
                Level = this.Level,
                Text = this.Text,
                Number = this.Number,
                ListType = this.ListType,
                NumberingFormat = this.NumberingFormat,
                NumberingStyle = this.NumberingStyle,
                StartAtNumber = this.StartAtNumber,
                SubItems = this.SubItems.Select(s => s.Clone()).ToList()
            };
        }
    }

    public enum ListItemType
    {
        Bullet,
        Number,
        Picture,
        Multilevel,
        Legal,
        Outline
    }

    public class NumberingDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public NumberingType Type { get; set; }
        public string LevelText { get; set; }
        public string LevelFormat { get; set; }
        public string LevelSeparator { get; set; }
        public List<NumberingLevel> Levels { get; set; }
        public bool IsLinkedToStyle { get; set; }
        public string LinkStyleName { get; set; }
        public bool IsAbstract { get; set; }
        public int? AbstractId { get; set; }

        public NumberingDefinition()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            Type = NumberingType.Bullet;
            LevelText = "%1";
            LevelFormat = "decimal";
            LevelSeparator = ".";
            Levels = new List<NumberingLevel>();
            IsLinkedToStyle = true;
        }

        public NumberingDefinition Clone()
        {
            return new NumberingDefinition
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                LevelText = this.LevelText,
                LevelFormat = this.LevelFormat,
                LevelSeparator = this.LevelSeparator,
                Levels = this.Levels.Select(l => l.Clone()).ToList(),
                IsLinkedToStyle = this.IsLinkedToStyle,
                LinkStyleName = this.LinkStyleName,
                IsAbstract = this.IsAbstract,
                AbstractId = this.AbstractId
            };
        }
    }

    public enum NumberingType
    {
        Bullet,
        Number,
        Picture,
        Multilevel
    }

    public class NumberingLevel
    {
        public int Level { get; set; }
        public int StartAtNumber { get; set; }
        public string Format { get; set; }
        public string LevelText { get; set; }
        public string LevelJustification { get; set; }
        public int PreviousNumberText { get; set; }
        public int PreviousLevel { get; set; }
        public int StartNumberingValue { get; set; }
        public double LeftMargin { get; set; }
        public double HangingMargin { get; set; }
        public string NumberingStyle { get; set; }
        public string NumberingPicture { get; set; }
        public ListItemAlignment Alignment { get; set; }

        public NumberingLevel()
        {
            Level = 0;
            StartAtNumber = 1;
            Format = string.Empty;
            LevelText = "%1";
            LevelJustification = "left";
            PreviousNumberText = 0;
            PreviousLevel = -1;
            StartNumberingValue = 1;
            LeftMargin = 0;
            HangingMargin = 0;
            NumberingStyle = "bullet";
            Alignment = ListItemAlignment.Left;
        }

        public NumberingLevel Clone()
        {
            return new NumberingLevel
            {
                Level = this.Level,
                StartAtNumber = this.StartAtNumber,
                Format = this.Format,
                LevelText = this.LevelText,
                LevelJustification = this.LevelJustification,
                PreviousNumberText = this.PreviousNumberText,
                PreviousLevel = this.PreviousLevel,
                StartNumberingValue = this.StartNumberingValue,
                LeftMargin = this.LeftMargin,
                HangingMargin = this.HangingMargin,
                NumberingStyle = this.NumberingStyle,
                NumberingPicture = this.NumberingPicture,
                Alignment = this.Alignment
            };
        }
    }

    public enum ListItemAlignment
    {
        Left,
        Center,
        Right
    }

    public class SpellingError
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Word { get; set; }
        public List<string> Suggestions { get; set; }
        public SpellingErrorType ErrorType { get; set; }
        public string Language { get; set; }
        public bool IsIgnored { get; set; }
        public bool IsAutoCorrected { get; set; }

        public SpellingError()
        {
            Suggestions = new List<string>();
            ErrorType = SpellingErrorType.UnknownWord;
            Language = "en-US";
        }
    }

    public enum SpellingErrorType
    {
        UnknownWord,
        RepeatedWord,
        capitalizationError,
        GrammarError,
        SpellingError
    }

    public class AutoCorrectEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsExpanded { get; set; }
        public AutoCorrectType Type { get; set; }
        public string Description { get; set; }

        public AutoCorrectEntry()
        {
            IsExpanded = false;
            Type = AutoCorrectType.Word;
        }
    }

    public enum AutoCorrectType
    {
        Word,
        Sentence,
        Paragraph,
        Mathematical
    }

    public class SmartTag
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Name { get; set; }
        public string NamespaceUri { get; set; }
        public List<SmartTagProperty> Properties { get; set; }
        public List<SmartTagAction> Actions { get; set; }

        public SmartTag()
        {
            Properties = new List<SmartTagProperty>();
            Actions = new List<SmartTagAction>();
        }
    }

    public class SmartTagProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Uri { get; set; }
    }

    public class SmartTagAction
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public string Action { get; set; }
    }

    public class DropCap
    {
        public bool IsEnabled { get; set; }
        public DropCapPosition Position { get; set; }
        public int FontSize { get; set; }
        public int Lines { get; set; }
        public string FontName { get; set; }
        public double Margin { get; set; }
        public DropCapStyle Style { get; set; }

        public DropCap()
        {
            IsEnabled = false;
            Position = DropCapPosition.Left;
            FontSize = 36;
            Lines = 3;
            Margin = 0.1;
            Style = DropCapStyle.Normal;
        }
    }

    public enum DropCapPosition
    {
        Left,
        Right,
        Center,
        Drop,
        Margin
    }

    public enum DropCapStyle
    {
        Normal,
        Embossed,
        Engraved,
        Outline,
        Shadow
    }

    public class TextDirection
    {
        public TextDirectionType Direction { get; set; }
        public double Rotation { get; set; }
        public bool IsVertical { get; set; }

        public TextDirection()
        {
            Direction = TextDirectionType.LTR;
            Rotation = 0;
            IsVertical = false;
        }
    }

    public enum TextDirectionType
    {
        LTR,
        RTL,
        TTBRlSeq,
        TTBLrnSeq,
        Vertical,
        Horizontal
    }

    public class RevisionMark
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public RevisionMarkType Type { get; set; }
        public string Author { get; set; }
        public DateTime DateTime { get; set; }
        public string Id { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }

        public RevisionMark()
        {
            Author = string.Empty;
            DateTime = DateTime.Now;
            Id = Guid.NewGuid().ToString();
        }
    }

    public enum RevisionMarkType
    {
        Insertion,
        Deletion,
        FormatChange,
        MoveFrom,
        MoveTo,
        Comment,
        ParagraphNumber,
        TableCellInsertion,
        TableCellDeletion,
        TableCellMerge,
        TableCellSplit,
        TableInsertion,
        TableDeletion,
        SectionPropertyChange,
        PageMarginChange,
        HeaderFooterChange
    }
}

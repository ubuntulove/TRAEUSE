using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeSuite.Spreadsheet.Models
{
    public class DataValidationRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DataValidationType Type { get; set; }
        public string Formula1 { get; set; }
        public string Formula2 { get; set; }
        public bool IgnoreBlank { get; set; }
        public bool ShowInputMessage { get; set; }
        public string InputTitle { get; set; }
        public string InputMessage { get; set; }
        public bool ShowErrorAlert { get; set; }
        public DataValidationAlertStyle ErrorStyle { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
        public CellRange AppliesTo { get; set; }
        public DataValidationOperator Operator { get; set; }
        public bool IsInDropDownCell { get; set; }
        public List<string> FormulaList { get; set; }

        public DataValidationRule()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            Type = DataValidationType.Whole;
            IgnoreBlank = true;
            ShowInputMessage = true;
            ShowErrorAlert = true;
            ErrorStyle = DataValidationAlertStyle.Stop;
            AppliesTo = new CellRange();
            Operator = DataValidationOperator.Between;
            IsInDropDownCell = false;
            FormulaList = new List<string>();
        }

        public DataValidationRule Clone()
        {
            return new DataValidationRule
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                Formula1 = this.Formula1,
                Formula2 = this.Formula2,
                IgnoreBlank = this.IgnoreBlank,
                ShowInputMessage = this.ShowInputMessage,
                InputTitle = this.InputTitle,
                InputMessage = this.InputMessage,
                ShowErrorAlert = this.ShowErrorAlert,
                ErrorStyle = this.ErrorStyle,
                ErrorTitle = this.ErrorTitle,
                ErrorMessage = this.ErrorMessage,
                AppliesTo = this.AppliesTo.Clone(),
                Operator = this.Operator,
                IsInDropDownCell = this.IsInDropDownCell,
                FormulaList = new List<string>(this.FormulaList)
            };
        }

        public bool ValidateCell(object value)
        {
            if (value == null || value == DBNull.Value)
                return IgnoreBlank;

            switch (Type)
            {
                case DataValidationType.Whole:
                    return ValidateWhole(value);
                case DataValidationType.Decimal:
                    return ValidateDecimal(value);
                case DataValidationType.List:
                    return ValidateList(value);
                case DataValidationType.Date:
                    return ValidateDate(value);
                case DataValidationType.Time:
                    return ValidateTime(value);
                case DataValidationType.TextLength:
                    return ValidateTextLength(value);
                case DataValidationType.Custom:
                    return ValidateCustom(value);
                default:
                    return true;
            }
        }

        private bool ValidateWhole(object value)
        {
            if (!double.TryParse(value.ToString(), out double numValue))
                return false;

            switch (Operator)
            {
                case DataValidationOperator.Between:
                    double min, max;
                    if (double.TryParse(Formula1, out min) && double.TryParse(Formula2, out max))
                        return numValue >= min && numValue <= max;
                    break;
                case DataValidationOperator.NotBetween:
                    if (double.TryParse(Formula1, out min) && double.TryParse(Formula2, out max))
                        return numValue < min || numValue > max;
                    break;
                case DataValidationOperator.Equal:
                    if (double.TryParse(Formula1, out min))
                        return Math.Abs(numValue - min) < 0.0001;
                    break;
                case DataValidationOperator.NotEqual:
                    if (double.TryParse(Formula1, out min))
                        return Math.Abs(numValue - min) >= 0.0001;
                    break;
                case DataValidationOperator.GreaterThan:
                    if (double.TryParse(Formula1, out min))
                        return numValue > min;
                    break;
                case DataValidationOperator.LessThan:
                    if (double.TryParse(Formula1, out min))
                        return numValue < min;
                    break;
                case DataValidationOperator.GreaterThanOrEqual:
                    if (double.TryParse(Formula1, out min))
                        return numValue >= min;
                    break;
                case DataValidationOperator.LessThanOrEqual:
                    if (double.TryParse(Formula1, out min))
                        return numValue <= min;
                    break;
            }
            return true;
        }

        private bool ValidateDecimal(object value)
        {
            return ValidateWhole(value);
        }

        private bool ValidateList(object value)
        {
            return FormulaList.Contains(value.ToString());
        }

        private bool ValidateDate(object value)
        {
            if (DateTime.TryParse(value.ToString(), out DateTime dateValue))
            {
                return ValidateWhole(dateValue.Ticks);
            }
            return false;
        }

        private bool ValidateTime(object value)
        {
            if (TimeSpan.TryParse(value.ToString(), out TimeSpan timeValue))
            {
                return ValidateWhole(timeValue.TotalSeconds);
            }
            return false;
        }

        private bool ValidateTextLength(object value)
        {
            int length = value.ToString().Length;
            return ValidateWhole(length);
        }

        private bool ValidateCustom(object value)
        {
            return true;
        }
    }

    public enum DataValidationType
    {
        Whole,
        Decimal,
        List,
        Date,
        Time,
        TextLength,
        Custom,
        NoValidation
    }

    public enum DataValidationOperator
    {
        Between,
        NotBetween,
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public enum DataValidationAlertStyle
    {
        Stop,
        Warning,
        Information
    }

    public class ConditionalFormattingRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ConditionalFormattingType Type { get; set; }
        public string Formula { get; set; }
        public CellRange AppliesTo { get; set; }
        public bool StopIfTrue { get; set; }
        public bool IsVisible { get; set; }
        public Priority Priority { get; set; }
        public ConditionalFormatStyle Style { get; set; }
        public ConditionalFormattingOperator Operator { get; set; }
        public string Text { get; set; }
        public double TimePeriodValue { get; set; }
        public List<double> ColorScaleValues { get; set; }
        public List<string> ColorScaleColors { get; set; }
        public List<IconSetEntry> IconSetEntries { get; set; }
        public ConditionalFormattingRule BasedOn { get; set; }

        public ConditionalFormattingRule()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            Type = ConditionalFormattingType.CellValue;
            Formula = string.Empty;
            AppliesTo = new CellRange();
            StopIfTrue = false;
            IsVisible = true;
            Priority = new Priority();
            Style = new ConditionalFormatStyle();
            Operator = ConditionalFormattingOperator.Equal;
            Text = string.Empty;
            TimePeriodValue = 0;
            ColorScaleValues = new List<double>();
            ColorScaleColors = new List<string>();
            IconSetEntries = new List<IconSetEntry>();
        }

        public ConditionalFormattingRule Clone()
        {
            return new ConditionalFormattingRule
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                Formula = this.Formula,
                AppliesTo = this.AppliesTo.Clone(),
                StopIfTrue = this.StopIfTrue,
                IsVisible = this.IsVisible,
                Priority = this.Priority.Clone(),
                Style = this.Style.Clone(),
                Operator = this.Operator,
                Text = this.Text,
                TimePeriodValue = this.TimePeriodValue,
                ColorScaleValues = new List<double>(this.ColorScaleValues),
                ColorScaleColors = new List<string>(this.ColorScaleColors),
                IconSetEntries = this.IconSetEntries.Select(i => i.Clone()).ToList(),
                BasedOn = this.BasedOn
            };
        }
    }

    public enum ConditionalFormattingType
    {
        CellValue,
        Expression,
        ColorScale,
        DataBar,
        IconSet,
        TopBottom,
        UniqueValues,
        DuplicateValues,
        ContainsText,
        ContainsBlanks,
        ContainsNoBlanks,
        ContainsErrors,
        ContainsNoErrors,
        TimePeriod,
        AboveAverage,
        BelowAverage
    }

    public enum ConditionalFormattingOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Between,
        NotBetween,
        Contains,
        NotContains,
        BeginsWith,
        EndsWith
    }

    public class Priority
    {
        public int Value { get; set; }
        public bool IsPriorityValid { get; set; }

        public Priority()
        {
            Value = 1;
            IsPriorityValid = true;
        }

        public Priority Clone()
        {
            return new Priority
            {
                Value = this.Value,
                IsPriorityValid = this.IsPriorityValid
            };
        }
    }

    public class ConditionalFormatStyle
    {
        public string FontColor { get; set; }
        public string BackgroundColor { get; set; }
        public FillPatternType FillPattern { get; set; }
        public BorderFormat FontBorder { get; set; }
        public BorderFormat TopBorder { get; set; }
        public BorderFormat BottomBorder { get; set; }
        public BorderFormat LeftBorder { get; set; }
        public BorderFormat RightBorder { get; set; }
        public FontFormat FontFormat { get; set; }

        public ConditionalFormatStyle()
        {
            FontColor = "#000000";
            BackgroundColor = string.Empty;
            FillPattern = FillPatternType.Solid;
            FontBorder = new BorderFormat();
            TopBorder = new BorderFormat();
            BottomBorder = new BorderFormat();
            LeftBorder = new BorderFormat();
            RightBorder = new BorderFormat();
            FontFormat = new FontFormat();
        }

        public ConditionalFormatStyle Clone()
        {
            return new ConditionalFormatStyle
            {
                FontColor = this.FontColor,
                BackgroundColor = this.BackgroundColor,
                FillPattern = this.FillPattern,
                FontBorder = this.FontBorder.Clone(),
                TopBorder = this.TopBorder.Clone(),
                BottomBorder = this.BottomBorder.Clone(),
                LeftBorder = this.LeftBorder.Clone(),
                RightBorder = this.RightBorder.Clone(),
                FontFormat = this.FontFormat.Clone()
            };
        }
    }

    public enum FillPatternType
    {
        None,
        Solid,
        HorizontalStripe,
        VerticalStripe,
        ReverseDiagonalStripe,
        DiagonalStripe,
        DiagonalCrosshatch,
        ThickDiagonalCrosshatch,
        ThinHorizontalStripe,
        ThinVerticalStripe,
        ThinReverseDiagonalStripe,
        ThinDiagonalStripe,
        ThinHorizontalCrosshatch,
        ThinDiagonalCrosshatch,
        ThickHorizontalCrosshatch,
        ThickDiagonalCrosshatch
    }

    public class BorderFormat
    {
        public string Color { get; set; }
        public LineStyle Style { get; set; }
        public bool IsVisible { get; set; }

        public BorderFormat()
        {
            Color = "#000000";
            Style = LineStyle.None;
            IsVisible = false;
        }

        public BorderFormat Clone()
        {
            return new BorderFormat
            {
                Color = this.Color,
                Style = this.Style,
                IsVisible = this.IsVisible
            };
        }
    }

    public enum LineStyle
    {
        None,
        Continuous,
        Dash,
        Dot,
        DashDot,
        DashDotDot,
        Double,
        Thick,
        Thin,
        Hairline
    }

    public class FontFormat
    {
        public string FontName { get; set; }
        public double FontSize { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public UnderlineType UnderlineStyle { get; set; }
        public bool IsStrikeThrough { get; set; }
        public string Color { get; set; }
        public ScriptType ScriptType { get; set; }

        public FontFormat()
        {
            FontName = "Calibri";
            FontSize = 11;
            IsBold = false;
            IsItalic = false;
            IsUnderline = false;
            UnderlineStyle = UnderlineType.Single;
            IsStrikeThrough = false;
            Color = "#000000";
            ScriptType = ScriptType.None;
        }

        public FontFormat Clone()
        {
            return new FontFormat
            {
                FontName = this.FontName,
                FontSize = this.FontSize,
                IsBold = this.IsBold,
                IsItalic = this.IsItalic,
                IsUnderline = this.IsUnderline,
                UnderlineStyle = this.UnderlineStyle,
                IsStrikeThrough = this.IsStrikeThrough,
                Color = this.Color,
                ScriptType = this.ScriptType
            };
        }
    }

    public enum UnderlineType
    {
        None,
        Single,
        Double,
        SingleAccounting,
        DoubleAccounting,
        ThickSingle,
        ThickDouble
    }

    public enum ScriptType
    {
        None,
        Superscript,
        Subscript
    }

    public class IconSetEntry
    {
        public IconType IconType { get; set; }
        public IconValue IconValue { get; set; }
        public string CustomIcon { get; set; }
        public bool IsGreaterThanOrEqual { get; set; }

        public IconSetEntry()
        {
            IconType = IconType.NoIcon;
            IconValue = IconValue.Number;
            IsGreaterThanOrEqual = true;
        }

        public IconSetEntry Clone()
        {
            return new IconSetEntry
            {
                IconType = this.IconType,
                IconValue = this.IconValue,
                CustomIcon = this.CustomIcon,
                IsGreaterThanOrEqual = this.IsGreaterThanOrEqual
            };
        }
    }

    public enum IconType
    {
        NoIcon,
        ThreeSymbols,
        ThreeSymbolsCircled,
        FourSymbolsCircled,
        FiveSymbolsCircled,
        ThreeTrafficLights,
        ThreeTrafficLightsRimmed,
        FourTrafficLights,
        RedBlackGreenSignal,
        GreenBlackUpSignal,
        FourArrows,
        FourArrowsGray,
        FiveArrows,
        FiveArrowsGray,
        FourRating,
        FiveQuarters,
        FiveRating,
        FiveBoxes,
        BlackCircle,
        GreenCircle,
        RedCircle,
        YellowCircle,
        GreenFlag,
        RedFlag,
        YellowFlag,
        GreenCheck,
        RedCross,
        YellowExclamation,
        GreenCheckCircle,
        RedXCircle,
        YellowWarningCircle,
        GrayFill,
        DarkRedFill,
        RedFill,
        OrangeFill,
        YellowFill,
        YellowGreenFill,
        GreenFill,
        BlueFill,
        DarkBlueFill,
        PurpleFill,
        CustomIcon
    }

    public enum IconValue
    {
        Number,
        Percent,
        Formula,
        Percentile
    }

    public class PivotTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CellReference SourceRange { get; set; }
        public CellReference DestinationRange { get; set; }
        public Worksheet DestinationWorksheet { get; set; }
        public List<PivotField> RowFields { get; set; }
        public List<PivotField> ColumnFields { get; set; }
        public List<PivotField> PageFields { get; set; }
        public List<PivotField> DataFields { get; set; }
        public List<PivotField> HiddenFields { get; set; }
        public PivotTableOptions Options { get; set; }
        public PivotTableStyle Style { get; set; }
        public bool HasDataFields { get; set; }
        public bool PreserveFormatting { get; set; }
        public bool AutoFormat { get; set; }
        public bool EnableDrilldown { get; set; }
        public string DataSource { get; set; }
        public List<PivotCache> Cache { get; set; }

        public PivotTable()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            SourceRange = new CellReference();
            DestinationRange = new CellReference();
            RowFields = new List<PivotField>();
            ColumnFields = new List<PivotField>();
            PageFields = new List<PivotField>();
            DataFields = new List<PivotField>();
            HiddenFields = new List<PivotField>();
            Options = new PivotTableOptions();
            Style = new PivotTableStyle();
            Cache = new List<PivotCache>();
        }

        public PivotTable Clone()
        {
            return new PivotTable
            {
                Id = this.Id,
                Name = this.Name,
                SourceRange = this.SourceRange.Clone(),
                DestinationRange = this.DestinationRange.Clone(),
                DestinationWorksheet = this.DestinationWorksheet,
                RowFields = this.RowFields.Select(f => f.Clone()).ToList(),
                ColumnFields = this.ColumnFields.Select(f => f.Clone()).ToList(),
                PageFields = this.PageFields.Select(f => f.Clone()).ToList(),
                DataFields = this.DataFields.Select(f => f.Clone()).ToList(),
                HiddenFields = this.HiddenFields.Select(f => f.Clone()).ToList(),
                Options = this.Options.Clone(),
                Style = this.Style.Clone(),
                HasDataFields = this.HasDataFields,
                PreserveFormatting = this.PreserveFormatting,
                AutoFormat = this.AutoFormat,
                EnableDrilldown = this.EnableDrilldown,
                DataSource = this.DataSource,
                Cache = this.Cache.Select(c => c.Clone()).ToList()
            };
        }
    }

    public class PivotField
    {
        public string Name { get; set; }
        public int SourceIndex { get; set; }
        public PivotFieldOrientation Orientation { get; set; }
        public int Position { get; set; }
        public string DataRange { get; set; }
        public List<PivotItem> Items { get; set; }
        public PivotFieldOptions Options { get; set; }
        public PivotSubtotalFunction SubtotalFunction { get; set; }
        public string CustomName { get; set; }
        public bool IsHidden { get; set; }
        public bool ShowAllItems { get; set; }
        public bool IsDataField { get; set; }
        public string Formula { get; set; }
        public List<PivotFilter> Filters { get; set; }
        public SortOrder SortOrder { get; set; }
        public string SortField { get; set; }
        public bool AutoSort { get; set; }
        public SortType SortType { get; set; }
        public List<string> CalculatedMembers { get; set; }

        public PivotField()
        {
            Items = new List<PivotItem>();
            Options = new PivotFieldOptions();
            Filters = new List<PivotFilter>();
            CalculatedMembers = new List<string>();
            ShowAllItems = true;
            AutoSort = true;
            SortOrder = SortOrder.Ascending;
        }

        public PivotField Clone()
        {
            return new PivotField
            {
                Name = this.Name,
                SourceIndex = this.SourceIndex,
                Orientation = this.Orientation,
                Position = this.Position,
                DataRange = this.DataRange,
                Items = this.Items.Select(i => i.Clone()).ToList(),
                Options = this.Options.Clone(),
                SubtotalFunction = this.SubtotalFunction,
                CustomName = this.CustomName,
                IsHidden = this.IsHidden,
                ShowAllItems = this.ShowAllItems,
                IsDataField = this.IsDataField,
                Formula = this.Formula,
                Filters = this.Filters.Select(f => f.Clone()).ToList(),
                SortOrder = this.SortOrder,
                SortField = this.SortField,
                AutoSort = this.AutoSort,
                SortType = this.SortType,
                CalculatedMembers = new List<string>(this.CalculatedMembers)
            };
        }
    }

    public enum PivotFieldOrientation
    {
        RowField,
        ColumnField,
        PageField,
        DataField,
        HiddenField
    }

    public class PivotItem
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDataItem { get; set; }
        public bool ShowDetail { get; set; }
        public PivotItemPosition Position { get; set; }
        public string SourceName { get; set; }
        public object Value { get; set; }
        public string ParentItemName { get; set; }
        public int Level { get; set; }
        public List<PivotItem> ChildItems { get; set; }

        public PivotItem()
        {
            IsHidden = false;
            IsDataItem = true;
            ShowDetail = true;
            ChildItems = new List<PivotItem>();
        }

        public PivotItem Clone()
        {
            return new PivotItem
            {
                Name = this.Name,
                Index = this.Index,
                IsHidden = this.IsHidden,
                IsDataItem = this.IsDataItem,
                ShowDetail = this.ShowDetail,
                Position = this.Position,
                SourceName = this.SourceName,
                Value = this.Value,
                ParentItemName = this.ParentItemName,
                Level = this.Level,
                ChildItems = this.ChildItems.Select(c => c.Clone()).ToList()
            };
        }
    }

    public enum PivotItemPosition
    {
        DataItem,
        RowItem,
        ColumnItem,
        PageItem
    }

    public class PivotFieldOptions
    {
        public bool AutoSort { get; set; }
        public bool AutoShow { get; set; }
        public int ShowCount { get; set; }
        public bool ShowManualSubtotals { get; set; }
        public bool ShowAllSubtotals { get; set; }
        public bool InsertBlankRows { get; set; }
        public bool InsertPageBreaks { get; set; }
        public bool SubtotalsAtTop { get; set; }
        public bool OutlineSubtotals { get; set; }
        public bool MultipleItemSelectionAllowed { get; set; }
        public bool IsCalculatedField { get; set; }
        public bool HideNewItems { get; set; }
        public bool HideOldItems { get; set; }
        public string Formula { get; set; }
        public bool IncludeNewItemsInFilter { get; set; }

        public PivotFieldOptions()
        {
            AutoSort = true;
            AutoShow = true;
            ShowCount = 10;
            ShowManualSubtotals = true;
            ShowAllSubtotals = true;
            InsertBlankRows = false;
            InsertPageBreaks = false;
            SubtotalsAtTop = true;
            OutlineSubtotals = false;
            MultipleItemSelectionAllowed = false;
            IsCalculatedField = false;
            HideNewItems = false;
            HideOldItems = false;
            IncludeNewItemsInFilter = true;
        }

        public PivotFieldOptions Clone()
        {
            return new PivotFieldOptions
            {
                AutoSort = this.AutoSort,
                AutoShow = this.AutoShow,
                ShowCount = this.ShowCount,
                ShowManualSubtotals = this.ShowManualSubtotals,
                ShowAllSubtotals = this.ShowAllSubtotals,
                InsertBlankRows = this.InsertBlankRows,
                InsertPageBreaks = this.InsertPageBreaks,
                SubtotalsAtTop = this.SubtotalsAtTop,
                OutlineSubtotals = this.OutlineSubtotals,
                MultipleItemSelectionAllowed = this.MultipleItemSelectionAllowed,
                IsCalculatedField = this.IsCalculatedField,
                HideNewItems = this.HideNewItems,
                HideOldItems = this.HideOldItems,
                Formula = this.Formula,
                IncludeNewItemsInFilter = this.IncludeNewItemsInFilter
            };
        }
    }

    public enum PivotSubtotalFunction
    {
        Sum,
        Count,
        Average,
        Max,
        Min,
        Product,
        CountNumbers,
        StdDev,
        StdDevP,
        Var,
        VarP
    }

    public class PivotFilter
    {
        public string FieldName { get; set; }
        public PivotFilterType FilterType { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public List<string> Values { get; set; }
        public string Top10Value { get; set; }
        public PivotTop10Type Top10Type { get; set; }
        public PivotTop10Order Top10Order { get; set; }

        public PivotFilter()
        {
            FilterType = PivotFilterType.CaptionEqual;
            Values = new List<string>();
        }

        public PivotFilter Clone()
        {
            return new PivotFilter
            {
                FieldName = this.FieldName,
                FilterType = this.FilterType,
                Value1 = this.Value1,
                Value2 = this.Value2,
                Values = new List<string>(this.Values),
                Top10Value = this.Top10Value,
                Top10Type = this.Top10Type,
                Top10Order = this.Top10Order
            };
        }
    }

    public enum PivotFilterType
    {
        CaptionEqual,
        CaptionNotEqual,
        CaptionBeginsWith,
        CaptionEndsWith,
        CaptionContains,
        CaptionDoesNotContain,
        CaptionBetween,
        ValueEqual,
        ValueNotEqual,
        ValueGreaterThan,
        ValueLessThan,
        ValueBetween,
        Top10,
        Unknown
    }

    public enum PivotTop10Type
    {
        Top,
        Bottom
    }

    public enum PivotTop10Order
    {
        Items,
        Percent,
        Sum
    }

    public enum SortOrder
    {
        Ascending,
        Descending,
        Manual
    }

    public enum SortType
    {
        Automatic,
        Manual,
        UseFormat
    }

    public class PivotTableOptions
    {
        public bool AutoFormat { get; set; }
        public string AutoFormatName { get; set; }
        public bool PreserveFormatting { get; set; }
        public bool PageWrap { get; set; }
        public int PageColumnsBefore { get; set; }
        public bool ShowColumnHeaders { get; set; }
        public bool ShowRowHeaders { get; set; }
        public bool ShowDataValues { get; set; }
        public bool ShowItems { get; set; }
        public bool ShowSubtotals { get; set; }
        public bool ShowGrandTotals { get; set; }
        public bool ShowColumnGrandTotals { get; set; }
        public bool ShowRowGrandTotals { get; set; }
        public bool ShowFieldHeaders { get; set; }
        public bool ShowNullValues { get; set; }
        public string NullValueString { get; set; }
        public bool MergeLabels { get; set; }
        public bool DisplayErrorString { get; set; }
        public string ErrorString { get; set; }
        public bool DisplayEmptyRow { get; set; }
        public bool DisplayEmptyColumn { get; set; }
        public bool GridDisplay { get; set; }
        public bool EnableDrilldown { get; set; }
        public bool EnableFieldDialog { get; set; }
        public bool EnableWizard { get; set; }
        public bool ShowMultiplePageItems { get; set; }
        public bool PrintTitles { get; set; }
        public bool RepeatItemsOnPrintedPages { get; set; }
        public bool FitToPage { get; set; }
        public bool SetItemCount { get; set; }
        public int ItemCount { get; set; }
        public PivotTablePageLayout PageLayout { get; set; }
        public string Version { get; set; }

        public PivotTableOptions()
        {
            AutoFormat = true;
            PreserveFormatting = true;
            PageColumnsBefore = 0;
            ShowColumnHeaders = true;
            ShowRowHeaders = true;
            ShowDataValues = true;
            ShowItems = true;
            ShowSubtotals = true;
            ShowGrandTotals = true;
            ShowColumnGrandTotals = true;
            ShowRowGrandTotals = true;
            ShowFieldHeaders = true;
            ShowNullValues = true;
            NullValueString = string.Empty;
            MergeLabels = false;
            DisplayErrorString = true;
            ErrorString = string.Empty;
            DisplayEmptyRow = true;
            DisplayEmptyColumn = true;
            GridDisplay = true;
            EnableDrilldown = true;
            EnableFieldDialog = true;
            EnableWizard = true;
            ShowMultiplePageItems = true;
            PrintTitles = false;
            RepeatItemsOnPrintedPages = false;
            FitToPage = false;
            SetItemCount = false;
            ItemCount = 0;
            PageLayout = PivotTablePageLayout.Sequential;
            Version = "1.0";
        }

        public PivotTableOptions Clone()
        {
            return new PivotTableOptions
            {
                AutoFormat = this.AutoFormat,
                AutoFormatName = this.AutoFormatName,
                PreserveFormatting = this.PreserveFormatting,
                PageWrap = this.PageWrap,
                PageColumnsBefore = this.PageColumnsBefore,
                ShowColumnHeaders = this.ShowColumnHeaders,
                ShowRowHeaders = this.ShowRowHeaders,
                ShowDataValues = this.ShowDataValues,
                ShowItems = this.ShowItems,
                ShowSubtotals = this.ShowSubtotals,
                ShowGrandTotals = this.ShowGrandTotals,
                ShowColumnGrandTotals = this.ShowColumnGrandTotals,
                ShowRowGrandTotals = this.ShowRowGrandTotals,
                ShowFieldHeaders = this.ShowFieldHeaders,
                ShowNullValues = this.ShowNullValues,
                NullValueString = this.NullValueString,
                MergeLabels = this.MergeLabels,
                DisplayErrorString = this.DisplayErrorString,
                ErrorString = this.ErrorString,
                DisplayEmptyRow = this.DisplayEmptyRow,
                DisplayEmptyColumn = this.DisplayEmptyColumn,
                GridDisplay = this.GridDisplay,
                EnableDrilldown = this.EnableDrilldown,
                EnableFieldDialog = this.EnableFieldDialog,
                EnableWizard = this.EnableWizard,
                ShowMultiplePageItems = this.ShowMultiplePageItems,
                PrintTitles = this.PrintTitles,
                RepeatItemsOnPrintedPages = this.RepeatItemsOnPrintedPages,
                FitToPage = this.FitToPage,
                SetItemCount = this.SetItemCount,
                ItemCount = this.ItemCount,
                PageLayout = this.PageLayout,
                Version = this.Version
            };
        }
    }

    public enum PivotTablePageLayout
    {
        Sequential,
        AcrossThenDown,
        DownThenAcross
    }

    public class PivotTableStyle
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool ShowRowHeaders { get; set; }
        public bool ShowColumnStripes { get; set; }
        public bool ShowRowStripes { get; set; }
        public PivotTableStyleType StyleType { get; set; }

        public PivotTableStyle()
        {
            Name = "PivotStyleMedium2";
            IsDefault = false;
            ShowRowHeaders = true;
            ShowColumnStripes = false;
            ShowRowStripes = false;
            StyleType = PivotTableStyleType.Medium;
        }

        public PivotTableStyle Clone()
        {
            return new PivotTableStyle
            {
                Name = this.Name,
                IsDefault = this.IsDefault,
                ShowRowHeaders = this.ShowRowHeaders,
                ShowColumnStripes = this.ShowColumnStripes,
                ShowRowStripes = this.ShowRowStripes,
                StyleType = this.StyleType
            };
        }
    }

    public enum PivotTableStyleType
    {
        None,
        Light,
        Medium,
        Dark
    }

    public class PivotCache
    {
        public int Index { get; set; }
        public List<object> Values { get; set; }
        public List<PivotCacheItem> Items { get; set; }
        public Dictionary<string, object> SharedItems { get; set; }
        public bool IsBuilt { get; set; }
        public string FieldName { get; set; }

        public PivotCache()
        {
            Values = new List<object>();
            Items = new List<PivotCacheItem>();
            SharedItems = new Dictionary<string, object>();
            IsBuilt = false;
        }

        public PivotCache Clone()
        {
            return new PivotCache
            {
                Index = this.Index,
                Values = new List<object>(this.Values),
                Items = this.Items.Select(i => i.Clone()).ToList(),
                SharedItems = new Dictionary<string, object>(this.SharedItems),
                IsBuilt = this.IsBuilt,
                FieldName = this.FieldName
            };
        }
    }

    public class PivotCacheItem
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsHidden { get; set; }
        public int Count { get; set; }
        public double Sum { get; set; }

        public PivotCacheItem Clone()
        {
            return new PivotCacheItem
            {
                Name = this.Name,
                Value = this.Value,
                IsHidden = this.IsHidden,
                Count = this.Count,
                Sum = this.Sum
            };
        }
    }

    public class Sparkline
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CellReference Location { get; set; }
        public CellRange DataRange { get; set; }
        public SparklineType Type { get; set; }
        public SparklineAxis Axis { get; set; }
        public SparklineMarkers Markers { get; set; }
        public SparklineStyle Style { get; set; }
        public string Color { get; set; }
        public string Color2 { get; set; }
        public bool ShowFirst { get; set; }
        public bool ShowLast { get; set; }
        public bool ShowHigh { get; set; }
        public bool ShowLow { get; set; }
        public bool ShowNegative { get; set; }
        public bool ShowHidden { get; set; }
        public bool RightToLeft { get; set; }
        public double LineWeight { get; set; }

        public Sparkline()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            Location = new CellReference();
            DataRange = new CellRange();
            Type = SparklineType.Line;
            Axis = new SparklineAxis();
            Markers = new SparklineMarkers();
            Style = new SparklineStyle();
            Color = "#4472C4";
            Color2 = "#ED7D31";
            ShowFirst = false;
            ShowLast = true;
            ShowHigh = false;
            ShowLow = false;
            ShowNegative = false;
            ShowHidden = true;
            RightToLeft = false;
            LineWeight = 1.0;
        }

        public Sparkline Clone()
        {
            return new Sparkline
            {
                Id = this.Id,
                Name = this.Name,
                Location = this.Location.Clone(),
                DataRange = this.DataRange.Clone(),
                Type = this.Type,
                Axis = this.Axis.Clone(),
                Markers = this.Markers.Clone(),
                Style = this.Style.Clone(),
                Color = this.Color,
                Color2 = this.Color2,
                ShowFirst = this.ShowFirst,
                ShowLast = this.ShowLast,
                ShowHigh = this.ShowHigh,
                ShowLow = this.ShowLow,
                ShowNegative = this.ShowNegative,
                ShowHidden = this.ShowHidden,
                RightToLeft = this.RightToLeft,
                LineWeight = this.LineWeight
            };
        }
    }

    public enum SparklineType
    {
        Line,
        Column,
        Stacked
    }

    public class SparklineAxis
    {
        public SparklineAxisType Type { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public bool IsAutomaticMin { get; set; }
        public bool IsAutomaticMax { get; set; }
        public bool ShowAxis { get; set; }
        public bool IsVertical { get; set; }

        public SparklineAxis()
        {
            Type = SparklineAxisType.Automatic;
            ShowAxis = false;
            IsVertical = false;
            IsAutomaticMin = true;
            IsAutomaticMax = true;
        }

        public SparklineAxis Clone()
        {
            return new SparklineAxis
            {
                Type = this.Type,
                MinValue = this.MinValue,
                MaxValue = this.MaxValue,
                IsAutomaticMin = this.IsAutomaticMin,
                IsAutomaticMax = this.IsAutomaticMax,
                ShowAxis = this.ShowAxis,
                IsVertical = this.IsVertical
            };
        }
    }

    public enum SparklineAxisType
    {
        Automatic,
        Custom,
        GroupMinMax,
        IndividualMinMax
    }

    public class SparklineMarkers
    {
        public bool ShowFirst { get; set; }
        public bool ShowLast { get; set; }
        public bool ShowHigh { get; set; }
        public bool ShowLow { get; set; }
        public bool ShowNegative { get; set; }
        public bool ShowAll { get; set; }
        public string FirstColor { get; set; }
        public string LastColor { get; set; }
        public string HighColor { get; set; }
        public string LowColor { get; set; }
        public string NegativeColor { get; set; }

        public SparklineMarkers()
        {
            ShowFirst = false;
            ShowLast = false;
            ShowHigh = false;
            ShowLow = false;
            ShowNegative = false;
            ShowAll = false;
            FirstColor = "#000000";
            LastColor = "#000000";
            HighColor = "#000000";
            LowColor = "#000000";
            NegativeColor = "#000000";
        }

        public SparklineMarkers Clone()
        {
            return new SparklineMarkers
            {
                ShowFirst = this.ShowFirst,
                ShowLast = this.ShowLast,
                ShowHigh = this.ShowHigh,
                ShowLow = this.ShowLow,
                ShowNegative = this.ShowNegative,
                ShowAll = this.ShowAll,
                FirstColor = this.FirstColor,
                LastColor = this.LastColor,
                HighColor = this.HighColor,
                LowColor = this.LowColor,
                NegativeColor = this.NegativeColor
            };
        }
    }

    public class SparklineStyle
    {
        public int PresetIndex { get; set; }
        public string ColorSeries { get; set; }
        public string ColorNegative { get; set; }
        public string ColorAxis { get; set; }
        public string ColorMarkers { get; set; }
        public LineStyle LineStyleType { get; set; }

        public SparklineStyle()
        {
            PresetIndex = 0;
            ColorSeries = "#4472C4";
            ColorNegative = "#C00000";
            ColorAxis = "#808080";
            ColorMarkers = string.Empty;
            LineStyleType = LineStyle.Continuous;
        }

        public SparklineStyle Clone()
        {
            return new SparklineStyle
            {
                PresetIndex = this.PresetIndex,
                ColorSeries = this.ColorSeries,
                ColorNegative = this.ColorNegative,
                ColorAxis = this.ColorAxis,
                ColorMarkers = this.ColorMarkers,
                LineStyleType = this.LineStyleType
            };
        }
    }

    public class Scenario
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Author { get; set; }
        public DateTime CreationDate { get; set; }
        public List<ScenarioInputCell> InputCells { get; set; }
        public bool IsHidden { get; set; }
        public bool IsLocked { get; set; }
        public ScenarioValues Values { get; set; }

        public Scenario()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            Comment = string.Empty;
            Author = string.Empty;
            CreationDate = DateTime.Now;
            InputCells = new List<ScenarioInputCell>();
            IsHidden = false;
            IsLocked = false;
            Values = new ScenarioValues();
        }

        public Scenario Clone()
        {
            return new Scenario
            {
                Id = this.Id,
                Name = this.Name,
                Comment = this.Comment,
                Author = this.Author,
                CreationDate = this.CreationDate,
                InputCells = this.InputCells.Select(i => i.Clone()).ToList(),
                IsHidden = this.IsHidden,
                IsLocked = this.IsLocked,
                Values = this.Values.Clone()
            };
        }
    }

    public class ScenarioInputCell
    {
        public CellReference Cell { get; set; }
        public object CurrentValue { get; set; }
        public object OriginalValue { get; set; }

        public ScenarioInputCell()
        {
            Cell = new CellReference();
        }

        public ScenarioInputCell Clone()
        {
            return new ScenarioInputCell
            {
                Cell = this.Cell.Clone(),
                CurrentValue = this.CurrentValue,
                OriginalValue = this.OriginalValue
            };
        }
    }

    public class ScenarioValues
    {
        public object[] Values { get; set; }

        public ScenarioValues()
        {
            Values = new object[0];
        }

        public ScenarioValues Clone()
        {
            return new ScenarioValues
            {
                Values = (object[])this.Values.Clone()
            };
        }
    }

    public class WhatIfAnalysis
    {
        public WhatIfAnalysisType Type { get; set; }
        public string Name { get; set; }
        public CellReference TargetCell { get; set; }
        public CellReference VariableCell { get; set; }
        public double TargetValue { get; set; }
        public double VariableStartValue { get; set; }
        public double VariableEndValue { get; set; }
        public int VariableStepValue { get; set; }
        public GoalSeekResult Result { get; set; }
        public List<WhatIfResult> Results { get; set; }

        public WhatIfAnalysis()
        {
            Type = WhatIfAnalysisType.GoalSeek;
            Name = string.Empty;
            TargetCell = new CellReference();
            VariableCell = new CellReference();
            Results = new List<WhatIfResult>();
        }
    }

    public enum WhatIfAnalysisType
    {
        GoalSeek,
        DataTable,
        ScenarioManager
    }

    public class GoalSeekResult
    {
        public bool IsConverged { get; set; }
        public double FoundValue { get; set; }
        public double VariableValue { get; set; }
        public int Iterations { get; set; }
        public string ErrorMessage { get; set; }

        public GoalSeekResult()
        {
            IsConverged = false;
            Iterations = 0;
            ErrorMessage = string.Empty;
        }
    }

    public class WhatIfResult
    {
        public double VariableValue { get; set; }
        public double ResultValue { get; set; }
        public bool IsValid { get; set; }

        public WhatIfResult()
        {
            IsValid = true;
        }
    }

    public class SolverModel
    {
        public string Name { get; set; }
        public CellReference ObjectiveCell { get; set; }
        public SolverObjectiveType ObjectiveType { get; set; }
        public List<CellReference> VariableCells { get; set; }
        public List<SolverConstraint> Constraints { get; set; }
        public SolverOptions Options { get; set; }
        public SolverResult Result { get; set; }
        public bool IsActive { get; set; }

        public SolverModel()
        {
            Name = string.Empty;
            ObjectiveCell = new CellReference();
            ObjectiveType = SolverObjectiveType.Minimize;
            VariableCells = new List<CellReference>();
            Constraints = new List<SolverConstraint>();
            Options = new SolverOptions();
            Result = new SolverResult();
            IsActive = false;
        }
    }

    public enum SolverObjectiveType
    {
        Minimize,
        Maximize,
        ValueOf,
        MinMax
    }

    public class SolverConstraint
    {
        public CellReference Cell { get; set; }
        public SolverConstraintRelation Relation { get; set; }
        public string Formula { get; set; }
        public object Value { get; set; }
        public CellRange Range { get; set; }
        public bool IsInteger { get; set; }
        public bool IsBinary { get; set; }

        public SolverConstraint()
        {
            Cell = new CellReference();
            Relation = SolverConstraintRelation.Equal;
            Range = new CellRange();
        }
    }

    public enum SolverConstraintRelation
    {
        LessThanOrEqual,
        Equal,
        GreaterThanOrEqual,
        LessThan,
        GreaterThan,
        Integer,
        Binary
    }

    public class SolverOptions
    {
        public int MaxTime { get; set; }
        public int MaxIterations { get; set; }
        public double Precision { get; set; }
        public double Tolerance { get; set; }
        public bool Converge { get; set; }
        public bool Scaling { get; set; }
        public bool AssumeNonNeg { get; set; }
        public bool AssumeLinear { get; set; }
        public bool Estimates { get; set; }
        bool Derivatives { get; set; }
        bool Search { get; set; }
        public int PopulationSize { get; set; }
        public double MutationRate { get; set; }
        public double RandomSeed { get; set; }
        public int TimeLimit { get; set; }
        public int MaxSubProblems { get; set; }
        public int MaxIntegerSolutions { get; set; }

        public SolverOptions()
        {
            MaxTime = 100;
            MaxIterations = 100;
            Precision = 0.000001;
            Tolerance = 5;
            Converge = true;
            Scaling = true;
            AssumeNonNeg = false;
            AssumeLinear = false;
            Estimates = true;
            Derivatives = true;
            Search = true;
            PopulationSize = 100;
            MutationRate = 0.075;
            RandomSeed = 0;
            TimeLimit = 100;
            MaxSubProblems = 1000;
            MaxIntegerSolutions = 100;
        }
    }

    public class SolverResult
    {
        public SolverResultStatus Status { get; set; }
        public string Message { get; set; }
        public double ObjectiveValue { get; set; }
        public Dictionary<CellReference, object> VariableValues { get; set; }
        public int Iterations { get; set; }
        public int SubProblems { get; set; }

        public SolverResult()
        {
            Status = SolverResultStatus.Unknown;
            Message = string.Empty;
            VariableValues = new Dictionary<CellReference, object>();
        }
    }

    public enum SolverResultStatus
    {
        Unknown,
        Solved,
        Infeasible,
        Unbounded,
        Aborted,
        Manual,
        Converge,
        Error,
        ServerBusy,
        TimeOut
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OfficeSuite.Spreadsheet.Analysis
{
    public class ChartRenderer
    {
        public ChartModel Model { get; set; } = new();
        public ChartData Data { get; set; } = new();

        public void Render(ChartRenderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            ClearChart(context);

            switch (Model.ChartType)
            {
                case ChartType.Column:
                case ChartType.Bar:
                    RenderBarChart(context);
                    break;
                case ChartType.Line:
                    RenderLineChart(context);
                    break;
                case ChartType.Pie:
                    RenderPieChart(context);
                    break;
                case ChartType.Area:
                    RenderAreaChart(context);
                    break;
                case ChartType.Scatter:
                    RenderScatterChart(context);
                    break;
                case ChartType.Doughnut:
                    RenderDoughnutChart(context);
                    break;
                case ChartType.Radar:
                    RenderRadarChart(context);
                    break;
                case ChartType.Stock:
                    RenderStockChart(context);
                    break;
                default:
                    RenderDefaultChart(context);
                    break;
            }

            if (Model.ShowTitle)
                RenderTitle(context);

            if (Model.ShowLegend)
                RenderLegend(context);

            if (Model.ShowGridLines)
                RenderGridLines(context);

            if (Model.ShowDataLabels)
                RenderDataLabels(context);
        }

        private void ClearChart(ChartRenderContext context)
        {
            context.Graphics.Clear(Model.BackgroundColor);
        }

        private void RenderBarChart(ChartRenderContext context)
        {
            var seriesCount = Data.Series.Count;
            var categoryCount = Data.Categories.Count;

            if (seriesCount == 0 || categoryCount == 0)
                return;

            var chartArea = GetChartArea(context);
            var groupWidth = chartArea.Width / categoryCount;
            var barWidth = groupWidth / (seriesCount + 1) * 0.8;

            for (int seriesIdx = 0; seriesIdx < seriesCount; seriesIdx++)
            {
                var series = Data.Series[seriesIdx];
                var color = GetSeriesColor(seriesIdx);

                for (int catIdx = 0; catIdx < categoryCount; catIdx++)
                {
                    var value = series.Values.Count > catIdx ? series.Values[catIdx] : 0;
                    var barHeight = CalculateBarHeight(value, chartArea.Height);

                    var x = chartArea.X + catIdx * groupWidth + (seriesIdx + 0.5) * barWidth;
                    var y = chartArea.Y + chartArea.Height - barHeight;

                    RenderBar(context, x, y, barWidth, barHeight, color);
                }
            }
        }

        private void RenderLineChart(ChartRenderContext context)
        {
            var chartArea = GetChartArea(context);
            var seriesCount = Data.Series.Count;

            for (int seriesIdx = 0; seriesIdx < seriesCount; seriesIdx++)
            {
                var series = Data.Series[seriesIdx];
                var color = GetSeriesColor(seriesIdx);
                var points = new List<Point>();

                for (int i = 0; i < series.Values.Count; i++)
                {
                    var x = chartArea.X + i * (chartArea.Width / (series.Values.Count - 1));
                    var y = chartArea.Y + chartArea.Height - CalculatePointHeight(series.Values[i], chartArea.Height);
                    points.Add(new Point(x, y));
                }

                RenderLine(context, points, color);
                RenderMarkers(context, points, color);
            }
        }

        private void RenderPieChart(ChartRenderContext context)
        {
            var chartArea = GetChartArea(context);
            var centerX = chartArea.X + chartArea.Width / 2;
            var centerY = chartArea.Y + chartArea.Height / 2;
            var radius = Math.Min(chartArea.Width, chartArea.Height) / 2 * 0.9;

            var total = Data.Series.Sum(s => s.Values.Sum());
            if (total == 0) return;

            double startAngle = 0;
            for (int i = 0; i < Data.Series.Count; i++)
            {
                var series = Data.Series[i];
                var value = series.Values.FirstOrDefault();
                var percentage = value / total;
                var sweepAngle = percentage * 360;

                var color = GetSeriesColor(i);
                RenderPieSlice(context, centerX, centerY, radius, startAngle, sweepAngle, color);

                startAngle += sweepAngle;
            }
        }

        private void RenderAreaChart(ChartRenderContext context)
        {
            var chartArea = GetChartArea(context);

            for (int seriesIdx = 0; seriesIdx < Data.Series.Count; seriesIdx++)
            {
                var series = Data.Series[seriesIdx];
                var color = GetSeriesColor(seriesIdx);
                var points = new List<Point>();

                for (int i = 0; i < series.Values.Count; i++)
                {
                    var x = chartArea.X + i * (chartArea.Width / Math.Max(1, series.Values.Count - 1));
                    var y = chartArea.Y + chartArea.Height - CalculatePointHeight(series.Values[i], chartArea.Height);
                    points.Add(new Point(x, y));
                }

                points.Add(new Point(points.Last().X, chartArea.Y + chartArea.Height));
                points.Add(new Point(points.First().X, chartArea.Y + chartArea.Height));

                RenderArea(context, points, color);
            }
        }

        private void RenderScatterChart(ChartRenderContext context)
        {
            var chartArea = GetChartArea(context);

            for (int seriesIdx = 0; seriesIdx < Data.Series.Count; seriesIdx++)
            {
                var series = Data.Series[seriesIdx];
                var color = GetSeriesColor(seriesIdx);
                var points = new List<Point>();

                for (int i = 0; i < series.Values.Count; i += 2)
                {
                    if (i + 1 < series.Values.Count)
                    {
                        var x = chartArea.X + (series.Values[i] / Data.MaxX) * chartArea.Width;
                        var y = chartArea.Y + chartArea.Height - (series.Values[i + 1] / Data.MaxY) * chartArea.Height;
                        points.Add(new Point(x, y));
                    }
                }

                RenderScatterPoints(context, points, color);
            }
        }

        private void RenderDoughnutChart(ChartRenderContext context)
        {
            var chartArea = GetChartArea(context);
            var centerX = chartArea.X + chartArea.Width / 2;
            var centerY = chartArea.Y + chartArea.Height / 2;
            var outerRadius = Math.Min(chartArea.Width, chartArea.Height) / 2 * 0.9;
            var innerRadius = outerRadius * 0.5;

            var total = Data.Series.Sum(s => s.Values.Sum());
            if (total == 0) return;

            double startAngle = 0;
            for (int i = 0; i < Data.Series.Count; i++)
            {
                var series = Data.Series[i];
                var value = series.Values.FirstOrDefault();
                var percentage = value / total;
                var sweepAngle = percentage * 360;

                var color = GetSeriesColor(i);
                RenderDoughnutSlice(context, centerX, centerY, innerRadius, outerRadius, startAngle, sweepAngle, color);

                startAngle += sweepAngle;
            }
        }

        private void RenderRadarChart(ChartRenderContext context)
        {
            var chartArea = GetChartArea(context);
            var centerX = chartArea.X + chartArea.Width / 2;
            var centerY = chartArea.Y + chartArea.Height / 2;
            var radius = Math.Min(chartArea.Width, chartArea.Height) / 2 * 0.8;

            var categoryCount = Data.Categories.Count;
            var angleStep = 360.0 / categoryCount;

            RenderRadarGrid(context, centerX, centerY, radius, categoryCount);

            for (int seriesIdx = 0; seriesIdx < Data.Series.Count; seriesIdx++)
            {
                var series = Data.Series[seriesIdx];
                var color = GetSeriesColor(seriesIdx);
                var points = new List<Point>();

                for (int i = 0; i < categoryCount; i++)
                {
                    var value = series.Values.Count > i ? series.Values[i] : 0;
                    var valueRatio = value / Data.MaxValue;
                    var angle = (angleStep * i - 90) * Math.PI / 180;

                    var x = centerX + Math.Cos(angle) * radius * valueRatio;
                    var y = centerY + Math.Sin(angle) * radius * valueRatio;
                    points.Add(new Point(x, y));
                }

                if (points.Count > 0)
                    points.Add(points[0]);

                RenderLine(context, points, color);
            }
        }

        private void RenderStockChart(ChartRenderContext context)
        {
            var chartArea = GetChartArea(context);
            var candlestickWidth = chartArea.Width / Data.Categories.Count * 0.7;

            for (int i = 0; i < Data.Categories.Count; i++)
            {
                if (Data.Series.Count >= 4)
                {
                    var high = Data.Series[0].Values.ElementAtOrDefault(i);
                    var low = Data.Series[1].Values.ElementAtOrDefault(i);
                    var open = Data.Series[2].Values.ElementAtOrDefault(i);
                    var close = Data.Series[3].Values.ElementAtOrDefault(i);

                    RenderCandlestick(context,
                        chartArea.X + i * (chartArea.Width / Data.Categories.Count),
                        chartArea.Y,
                        candlestickWidth,
                        chartArea.Height,
                        high, low, open, close);
                }
            }
        }

        private void RenderDefaultChart(ChartRenderContext context)
        {
            RenderBarChart(context);
        }

        private void RenderTitle(ChartRenderContext context)
        {
        }

        private void RenderLegend(ChartRenderContext context)
        {
        }

        private void RenderGridLines(ChartRenderContext context)
        {
        }

        private void RenderDataLabels(ChartRenderContext context)
        {
        }

        private void RenderBar(ChartRenderContext context, double x, double y, double width, double height, string color)
        {
        }

        private void RenderLine(ChartRenderContext context, List<Point> points, string color)
        {
        }

        private void RenderMarkers(ChartRenderContext context, List<Point> points, string color)
        {
        }

        private void RenderPieSlice(ChartRenderContext context, double centerX, double centerY, double radius, double startAngle, double sweepAngle, string color)
        {
        }

        private void RenderDoughnutSlice(ChartRenderContext context, double centerX, double centerY, double innerRadius, double outerRadius, double startAngle, double sweepAngle, string color)
        {
        }

        private void RenderArea(ChartRenderContext context, List<Point> points, string color)
        {
        }

        private void RenderScatterPoints(ChartRenderContext context, List<Point> points, string color)
        {
        }

        private void RenderRadarGrid(ChartRenderContext context, double centerX, double centerY, double radius, int categoryCount)
        {
        }

        private void RenderCandlestick(ChartRenderContext context, double x, double y, double width, double height, double high, double low, double open, double close)
        {
        }

        private ChartArea GetChartArea(ChartRenderContext context)
        {
            var padding = 50;
            return new ChartArea
            {
                X = padding,
                Y = padding,
                Width = context.Width - padding * 2,
                Height = context.Height - padding * 2
            };
        }

        private double CalculateBarHeight(double value, double maxHeight)
        {
            var maxValue = Data.MaxValue > 0 ? Data.MaxValue : 1;
            return (value / maxValue) * maxHeight;
        }

        private double CalculatePointHeight(double value, double maxHeight)
        {
            var maxValue = Data.MaxValue > 0 ? Data.MaxValue : 1;
            return (value / maxValue) * maxHeight;
        }

        private string GetSeriesColor(int index)
        {
            var colors = new[]
            {
                "#4472C4", "#ED7D31", "#A5A5A5", "#FFC000", "#5B9BD5",
                "#70AD47", "#264478", "#9E480E", "#636363", "#997300"
            };
            return colors[index % colors.Length];
        }
    }

    public class ChartModel
    {
        public ChartType ChartType { get; set; } = ChartType.Column;
        public string Title { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string BorderColor { get; set; } = "#000000";
        public double BorderWidth { get; set; } = 1;
        public bool ShowTitle { get; set; }
        public bool ShowLegend { get; set; } = true;
        public LegendPosition LegendPosition { get; set; } = LegendPosition.Right;
        public bool ShowGridLines { get; set; } = true;
        public GridLineStyle GridLineStyle { get; set; } = GridLineStyle.Solid;
        public bool ShowDataLabels { get; set; }
        public DataLabelPosition DataLabelPosition { get; set; } = DataLabelPosition.OutsideEnd;
        public ChartStyle ChartStyle { get; set; } = ChartStyle.Style1;
    }

    public class ChartData
    {
        public ObservableCollection<ChartSeries> Series { get; set; } = new();
        public ObservableCollection<string> Categories { get; set; } = new();
        public double MaxValue => Series.SelectMany(s => s.Values).DefaultIfEmpty(0).Max();
        public double MinValue => Series.SelectMany(s => s.Values).DefaultIfEmpty(0).Min();
        public double MaxX { get; set; } = 100;
        public double MaxY { get; set; } = 100;
    }

    public class ChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<double> Values { get; set; } = new();
        public string Color { get; set; } = string.Empty;
        public ChartType LineType { get; set; } = ChartType.Line;
        public MarkerStyle MarkerStyle { get; set; } = MarkerStyle.Circle;
        public double LineWidth { get; set; } = 2;
        public bool ShowLine { get; set; } = true;
        public bool ShowMarkers { get; set; } = true;
    }

    public class ChartRenderContext
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public ChartGraphics Graphics { get; set; } = new();
    }

    public class ChartGraphics
    {
        public void Clear(string color) { }
        public void DrawLine(double x1, double y1, double x2, double y2, string color, double width) { }
        public void DrawRectangle(double x, double y, double width, double height, string fillColor, string strokeColor, double strokeWidth) { }
        public void DrawEllipse(double centerX, double centerY, double radiusX, double radiusY, string fillColor, string strokeColor, double strokeWidth) { }
        public void DrawText(string text, double x, double y, string color, string fontFamily, double fontSize) { }
        public void FillPolygon(List<Point> points, string color) { }
    }

    public class ChartArea
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public enum ChartType
    {
        Column,
        Bar,
        Line,
        Pie,
        Area,
        Scatter,
        Doughnut,
        Radar,
        Stock,
        Surface
    }

    public enum LegendPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    public enum GridLineStyle
    {
        Solid,
        Dash,
        Dot,
        DashDot,
        None
    }

    public enum DataLabelPosition
    {
        OutsideEnd,
        InsideEnd,
        InsideCenter,
        Left,
        Right,
        Top,
        Bottom
    }

    public enum ChartStyle
    {
        Style1,
        Style2,
        Style3,
        Style4,
        Style5,
        Style6,
        Style7,
        Style8,
        Style9,
        Style10
    }

    public enum MarkerStyle
    {
        None,
        Circle,
        Square,
        Diamond,
        Triangle,
        X,
        Star,
        Plus
    }

    public class DataValidationService
    {
        private readonly List<DataValidationRule> _rules;

        public DataValidationService()
        {
            _rules = new List<DataValidationRule>();
        }

        public void AddValidation(DataValidationRule rule)
        {
            _rules.Add(rule);
        }

        public void RemoveValidation(string range)
        {
            _rules.RemoveAll(r => r.Range == range);
        }

        public DataValidationResult ValidateValue(string range, object value, DataValidationType type)
        {
            var result = new DataValidationResult { IsValid = true };

            switch (type)
            {
                case DataValidationType.WholeNumber:
                    result = ValidateWholeNumber(value);
                    break;
                case DataValidationType.Decimal:
                    result = ValidateDecimal(value);
                    break;
                case DataValidationType.List:
                    result = ValidateList(value);
                    break;
                case DataValidationType.Date:
                    result = ValidateDate(value);
                    break;
                case DataValidationType.Time:
                    result = ValidateTime(value);
                    break;
                case DataValidationType.TextLength:
                    result = ValidateTextLength(value);
                    break;
                case DataValidationType.Custom:
                    result = ValidateCustom(value);
                    break;
            }

            result.Range = range;
            return result;
        }

        private DataValidationResult ValidateWholeNumber(object value)
        {
            var result = new DataValidationResult();
            if (value is int || value is long || value is double)
            {
                result.IsValid = true;
            }
            else if (double.TryParse(value?.ToString(), out _))
            {
                result.IsValid = true;
            }
            else
            {
                result.IsValid = false;
                result.ErrorMessage = "Value must be a whole number";
            }
            return result;
        }

        private DataValidationResult ValidateDecimal(object value)
        {
            var result = new DataValidationResult();
            if (double.TryParse(value?.ToString(), out _))
            {
                result.IsValid = true;
            }
            else
            {
                result.IsValid = false;
                result.ErrorMessage = "Value must be a decimal number";
            }
            return result;
        }

        private DataValidationResult ValidateList(object value)
        {
            return new DataValidationResult { IsValid = true };
        }

        private DataValidationResult ValidateDate(object value)
        {
            var result = new DataValidationResult();
            if (DateTime.TryParse(value?.ToString(), out _))
            {
                result.IsValid = true;
            }
            else
            {
                result.IsValid = false;
                result.ErrorMessage = "Value must be a valid date";
            }
            return result;
        }

        private DataValidationResult ValidateTime(object value)
        {
            var result = new DataValidationResult();
            if (TimeSpan.TryParse(value?.ToString(), out _))
            {
                result.IsValid = true;
            }
            else
            {
                result.IsValid = false;
                result.ErrorMessage = "Value must be a valid time";
            }
            return result;
        }

        private DataValidationResult ValidateTextLength(object value)
        {
            var result = new DataValidationResult { IsValid = true };
            return result;
        }

        private DataValidationResult ValidateCustom(object value)
        {
            return new DataValidationResult { IsValid = true };
        }
    }

    public class DataValidationRule
    {
        public string Range { get; set; } = string.Empty;
        public DataValidationType Type { get; set; } = DataValidationType.AnyValue;
        public object? AllowBlank { get; set; }
        public bool IgnoreBlank { get; set; } = true;
        public bool ShowDropDown { get; set; } = true;
        public bool ShowInputMessage { get; set; }
        public string InputTitle { get; set; } = string.Empty;
        public string InputMessage { get; set; } = string.Empty;
        public bool ShowErrorAlert { get; set; } = true;
        public ValidationAlertStyle ErrorAlertStyle { get; set; } = ValidationAlertStyle.Stop;
        public string ErrorTitle { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public object? Formula1 { get; set; }
        public object? Formula2 { get; set; }
        public List<string>? ValueList { get; set; }
    }

    public enum DataValidationType
    {
        AnyValue,
        WholeNumber,
        Decimal,
        List,
        Date,
        Time,
        TextLength,
        Custom
    }

    public enum ValidationAlertStyle
    {
        Stop,
        Warning,
        Information
    }

    public class DataValidationResult
    {
        public bool IsValid { get; set; }
        public string Range { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public object? InvalidValue { get; set; }
    }

    public class ConditionalFormattingService
    {
        private readonly List<ConditionalFormattingRule> _rules;

        public ConditionalFormattingService()
        {
            _rules = new List<ConditionalFormattingRule>();
        }

        public void AddRule(ConditionalFormattingRule rule)
        {
            _rules.Add(rule);
        }

        public void RemoveRule(string range)
        {
            _rules.RemoveAll(r => r.Range == range);
        }

        public List<ConditionalFormattingResult> Evaluate(List<CellValue> cells)
        {
            var results = new List<ConditionalFormattingResult>();

            foreach (var rule in _rules)
            {
                foreach (var cell in cells)
                {
                    if (IsInRange(cell.Address, rule.Range))
                    {
                        var result = EvaluateRule(cell, rule);
                        if (result.IsMatch)
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            return results;
        }

        private bool IsInRange(string address, string range)
        {
            return true;
        }

        private ConditionalFormattingResult EvaluateRule(CellValue cell, ConditionalFormattingRule rule)
        {
            var result = new ConditionalFormattingResult
            {
                CellAddress = cell.Address,
                RuleId = rule.Id,
                IsMatch = false
            };

            switch (rule.RuleType)
            {
                case ConditionalRuleType.CellValue:
                    result = EvaluateCellValueRule(cell, rule);
                    break;
                case ConditionalRuleType.Expression:
                    result = EvaluateExpressionRule(cell, rule);
                    break;
                case ConditionalRuleType.DataBar:
                    result.IsMatch = true;
                    break;
                case ConditionalRuleType.ColorScale:
                    result.IsMatch = true;
                    break;
                case ConditionalRuleType.IconSet:
                    result.IsMatch = true;
                    break;
                case ConditionalRuleType.Top10:
                    result = EvaluateTop10Rule(cell, rule);
                    break;
                case ConditionalRuleType.Unique:
                    result = EvaluateUniqueRule(cell, rule);
                    break;
                case ConditionalRuleType.Duplicate:
                    result = EvaluateDuplicateRule(cell, rule);
                    break;
            }

            return result;
        }

        private ConditionalFormattingResult EvaluateCellValueRule(CellValue cell, ConditionalFormattingRule rule)
        {
            return new ConditionalFormattingResult { IsMatch = false };
        }

        private ConditionalFormattingResult EvaluateExpressionRule(CellValue cell, ConditionalFormattingRule rule)
        {
            return new ConditionalFormattingResult { IsMatch = false };
        }

        private ConditionalFormattingResult EvaluateTop10Rule(CellValue cell, ConditionalFormattingRule rule)
        {
            return new ConditionalFormattingResult { IsMatch = false };
        }

        private ConditionalFormattingResult EvaluateUniqueRule(CellValue cell, ConditionalFormattingRule rule)
        {
            return new ConditionalFormattingResult { IsMatch = false };
        }

        private ConditionalFormattingResult EvaluateDuplicateRule(CellValue cell, ConditionalFormattingRule rule)
        {
            return new ConditionalFormattingResult { IsMatch = false };
        }
    }

    public class ConditionalFormattingRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Range { get; set; } = string.Empty;
        public ConditionalRuleType RuleType { get; set; } = ConditionalRuleType.CellValue;
        public ConditionalOperator Operator { get; set; } = ConditionalOperator.Between;
        public string Formula1 { get; set; } = string.Empty;
        public string Formula2 { get; set; } = string.Empty;
        public string FormatStyle { get; set; } = string.Empty;
        public string FillColor { get; set; } = "#FFFFFF";
        public string FontColor { get; set; } = "#000000";
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public string BorderStyle { get; set; } = "None";
        public int Priority { get; set; } = 1;
        public bool StopIfTrue { get; set; }
    }

    public enum ConditionalRuleType
    {
        CellValue,
        Expression,
        DataBar,
        ColorScale,
        IconSet,
        Top10,
        Unique,
        Duplicate,
        ContainsText,
        NotContainsText,
        BeginsWith,
        EndsWith,
        ContainsBlanks,
        NotContainsBlanks,
        Errors,
        NoErrors,
        Yesterday,
        Today,
        Tomorrow,
        Last7Days,
        LastWeek,
        ThisWeek,
        NextWeek,
        LastMonth,
        ThisMonth,
        NextMonth
    }

    public enum ConditionalOperator
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

    public class ConditionalFormattingResult
    {
        public string CellAddress { get; set; } = string.Empty;
        public string RuleId { get; set; } = string.Empty;
        public bool IsMatch { get; set; }
        public string? FillColor { get; set; }
        public string? FontColor { get; set; }
        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }
        public string? IconType { get; set; }
        public int? IconValue { get; set; }
    }

    public class CellValue
    {
        public string Address { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string? Formula { get; set; }
        public CellType Type { get; set; } = CellType.General;
    }

    public enum CellType
    {
        General,
        Number,
        Text,
        Boolean,
        Error,
        DateTime
    }
}

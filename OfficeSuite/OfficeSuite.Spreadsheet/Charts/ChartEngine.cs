using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeSuite.Spreadsheet.Charts
{
    public enum ChartType
    {
        Column,
        Bar,
        Line,
        Pie,
        Area,
        Scatter,
        Stock,
        Radar,
        Surface,
        Doughnut,
        Bubble,
        Treemap,
        Sunburst,
        Histogram,
        BoxAndWhisker,
        Waterfall,
        Funnel
    }

    public enum ChartSubtype
    {
        None,
        Clustered,
        Stacked,
        PercentStacked,
        LineWithMarkers,
        LineSmooth,
        LineSmoothWithMarkers,
        PieExploded,
        Pie3D,
        BarClustered3D,
        BarStacked3D,
        Column3D,
        Area3D,
        ScatterLine,
        ScatterSmooth,
        ScatterSmoothLine
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
        Dot,
        Dash,
        Plus
    }

    public class ChartFactory
    {
        public static IChart CreateChart(ChartType type, ChartSubtype subtype = ChartSubtype.None)
        {
            return type switch
            {
                ChartType.Column => new ColumnChart(subtype),
                ChartType.Bar => new BarChart(subtype),
                ChartType.Line => new LineChart(subtype),
                ChartType.Pie => new PieChart(subtype),
                ChartType.Area => new AreaChart(subtype),
                ChartType.Scatter => new ScatterChart(subtype),
                ChartType.Stock => new StockChart(subtype),
                ChartType.Radar => new RadarChart(subtype),
                ChartType.Surface => new SurfaceChart(subtype),
                ChartType.Doughnut => new DoughnutChart(subtype),
                ChartType.Bubble => new BubbleChart(subtype),
                ChartType.Treemap => new TreemapChart(),
                ChartType.Sunburst => new SunburstChart(),
                ChartType.Histogram => new HistogramChart(),
                ChartType.BoxAndWhisker => new BoxAndWhiskerChart(),
                ChartType.Waterfall => new WaterfallChart(),
                ChartType.Funnel => new FunnelChart(),
                _ => new ColumnChart(subtype)
            };
        }
    }

    public interface IChart
    {
        string Title { get; set; }
        ChartType Type { get; }
        ChartSubtype Subtype { get; }
        IReadOnlyList<ChartSeries> Series { get; }
        ChartAxes Axes { get; }
        ChartLegend Legend { get; }
        ChartPlotArea PlotArea { get; }

        void AddSeries(ChartSeries series);
        void RemoveSeries(string seriesName);
        void UpdateSeries(string seriesName, ChartSeries series);
        ChartDataPoint CalculatePoint(int seriesIndex, int pointIndex);
        void Render(ChartRenderingContext context);
    }

    public class ChartBase : IChart
    {
        public string Title { get; set; }
        public ChartType Type => GetChartType();
        public ChartSubtype Subtype { get; protected set; }
        protected List<ChartSeries> _series = new List<ChartSeries>();
        public IReadOnlyList<ChartSeries> Series => _series.AsReadOnly();
        public ChartAxes Axes { get; set; } = new ChartAxes();
        public ChartLegend Legend { get; set; } = new ChartLegend();
        public ChartPlotArea PlotArea { get; set; } = new ChartPlotArea();

        public virtual void AddSeries(ChartSeries series)
        {
            _series.Add(series);
        }

        public virtual void RemoveSeries(string seriesName)
        {
            _series.RemoveAll(s => s.Name == seriesName);
        }

        public virtual void UpdateSeries(string seriesName, ChartSeries series)
        {
            var index = _series.FindIndex(s => s.Name == seriesName);
            if (index >= 0)
                _series[index] = series;
        }

        public virtual ChartDataPoint CalculatePoint(int seriesIndex, int pointIndex)
        {
            if (seriesIndex < 0 || seriesIndex >= _series.Count)
                return null;

            var series = _series[seriesIndex];
            if (pointIndex < 0 || pointIndex >= series.Points.Count)
                return null;

            return series.Points[pointIndex];
        }

        public virtual void Render(ChartRenderingContext context)
        {
            RenderTitle(context);
            RenderPlotArea(context);
            RenderAxes(context);
            RenderLegend(context);
        }

        protected virtual void RenderTitle(ChartRenderingContext context)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                context.DrawText(Title, context.Width / 2, 30, new ChartTextFormat
                {
                    FontSize = 14,
                    FontWeight = ChartFontWeight.Bold,
                    HorizontalAlignment = ChartHorizontalAlignment.Center
                });
            }
        }

        protected virtual void RenderPlotArea(ChartRenderingContext context)
        {
            context.FillRectangle(PlotArea.FillColor, PlotArea.Bounds);
            context.DrawRectangle(PlotArea.BorderColor, PlotArea.BorderWidth, PlotArea.Bounds);
        }

        protected virtual void RenderAxes(ChartRenderingContext context)
        {
            Axes.Render(context);
        }

        protected virtual void RenderLegend(ChartRenderingContext context)
        {
            Legend.Render(context, _series);
        }

        protected virtual ChartType GetChartType() => ChartType.Column;
    }

    public class ColumnChart : ChartBase
    {
        public ColumnChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Column;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            var bounds = PlotArea.Bounds;
            var seriesCount = _series.Count;
            var maxPoints = _series.Max(s => s.Points.Count);
            
            if (maxPoints == 0 || seriesCount == 0)
                return;

            var groupWidth = bounds.Width / maxPoints;
            var barWidth = groupWidth / (seriesCount + 1);
            var gap = barWidth / 2;

            for (int seriesIdx = 0; seriesIdx < seriesCount; seriesIdx++)
            {
                var series = _series[seriesIdx];
                var offset = (seriesIdx - seriesCount / 2.0) * barWidth;

                for (int pointIdx = 0; pointIdx < series.Points.Count; pointIdx++)
                {
                    var point = series.Points[pointIdx];
                    var x = bounds.Left + pointIdx * groupWidth + groupWidth / 2 + offset;
                    var value = point.Value ?? 0;
                    var height = CalculateBarHeight(value, bounds, Axes.YAxis);
                    var y = Subtype == ChartSubtype.Stacked || Subtype == ChartSubtype.PercentStacked
                        ? CalculateStackedY(seriesIdx, pointIdx, bounds, value)
                        : value >= 0 ? bounds.Bottom - height : bounds.Bottom;

                    var rect = new ChartRectangle(x - barWidth / 2, y, barWidth, height);
                    context.FillRectangle(point.FillColor ?? series.Color, rect);

                    if (point.BorderColor != null)
                    {
                        context.DrawRectangle(point.BorderColor, point.BorderWidth, rect);
                    }
                }
            }
        }

        private double CalculateBarHeight(double value, ChartRectangle bounds, ChartAxis yAxis)
        {
            var axisMin = yAxis.Min ?? 0;
            var axisMax = yAxis.Max ?? 100;
            var normalizedValue = (value - axisMin) / (axisMax - axisMin);
            return Math.Abs(normalizedValue) * bounds.Height;
        }

        private double CalculateStackedY(int seriesIdx, int pointIdx, ChartRectangle bounds, double value)
        {
            double accumulated = 0;
            for (int i = 0; i < seriesIdx; i++)
            {
                if (_series[i].Points.Count > pointIdx)
                    accumulated += _series[i].Points[pointIdx].Value ?? 0;
            }
            return bounds.Bottom - accumulated / (Axes.YAxis.Max ?? 100) * bounds.Height;
        }
    }

    public class BarChart : ChartBase
    {
        public BarChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Bar;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            var bounds = PlotArea.Bounds;
            var seriesCount = _series.Count;
            var maxPoints = _series.Max(s => s.Points.Count);

            if (maxPoints == 0 || seriesCount == 0)
                return;

            var groupHeight = bounds.Height / maxPoints;
            var barHeight = groupHeight / (seriesCount + 1);

            for (int seriesIdx = 0; seriesIdx < seriesCount; seriesIdx++)
            {
                var series = _series[seriesIdx];
                var offset = (seriesIdx - seriesCount / 2.0) * barHeight;

                for (int pointIdx = 0; pointIdx < series.Points.Count; pointIdx++)
                {
                    var point = series.Points[pointIdx];
                    var y = bounds.Top + pointIdx * groupHeight + groupHeight / 2 + offset;
                    var value = point.Value ?? 0;
                    var width = CalculateBarWidth(value, bounds);
                    var x = bounds.Left;

                    var rect = new ChartRectangle(x, y - barHeight / 2, width, barHeight);
                    context.FillRectangle(point.FillColor ?? series.Color, rect);
                }
            }
        }

        private double CalculateBarWidth(double value, ChartRectangle bounds)
        {
            var axisMax = Axes.XAxis.Max ?? 100;
            return (value / axisMax) * bounds.Width;
        }
    }

    public class LineChart : ChartBase
    {
        public LineChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Line;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            var bounds = PlotArea.Bounds;
            var maxPoints = _series.Max(s => s.Points.Count);

            if (maxPoints == 0)
                return;

            var stepX = bounds.Width / (maxPoints - 1);

            foreach (var series in _series)
            {
                var points = new List<ChartPoint>();
                for (int i = 0; i < series.Points.Count; i++)
                {
                    var point = series.Points[i];
                    var x = bounds.Left + i * stepX;
                    var y = CalculateY(point.Value ?? 0, bounds);
                    points.Add(new ChartPoint { X = x, Y = y });
                }

                if (Subtype != ChartSubtype.None && points.Count > 1)
                {
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        context.DrawLine(series.Color, series.LineWidth, points[i], points[i + 1]);
                    }
                }
                else if (points.Count > 1)
                {
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        context.DrawLine(series.Color, series.LineWidth, points[i], points[i + 1]);
                    }
                }

                if (Subtype == ChartSubtype.LineWithMarkers || Subtype == ChartSubtype.LineSmoothWithMarkers)
                {
                    foreach (var pt in points)
                    {
                        RenderMarker(context, pt, series.MarkerStyle, series.MarkerSize, series.Color);
                    }
                }
            }
        }

        private double CalculateY(double value, ChartRectangle bounds)
        {
            var axisMin = Axes.YAxis.Min ?? 0;
            var axisMax = Axes.YAxis.Max ?? 100;
            return bounds.Bottom - ((value - axisMin) / (axisMax - axisMin)) * bounds.Height;
        }

        private void RenderMarker(ChartRenderingContext context, ChartPoint point, MarkerStyle style, double size, string color)
        {
            switch (style)
            {
                case MarkerStyle.Circle:
                    context.DrawEllipse(color, 1, new ChartPoint(point.X - size / 2, point.Y - size / 2), size, size);
                    break;
                case MarkerStyle.Square:
                    context.DrawRectangle(color, 1, new ChartRectangle(point.X - size / 2, point.Y - size / 2, size, size));
                    break;
                case MarkerStyle.Diamond:
                    var diamond = new List<ChartPoint>
                    {
                        new ChartPoint(point.X, point.Y - size / 2),
                        new ChartPoint(point.X + size / 2, point.Y),
                        new ChartPoint(point.X, point.Y + size / 2),
                        new ChartPoint(point.X - size / 2, point.Y)
                    };
                    context.DrawPolygon(color, 1, diamond);
                    break;
                case MarkerStyle.Triangle:
                    var triangle = new List<ChartPoint>
                    {
                        new ChartPoint(point.X, point.Y - size / 2),
                        new ChartPoint(point.X + size / 2, point.Y + size / 2),
                        new ChartPoint(point.X - size / 2, point.Y + size / 2)
                    };
                    context.DrawPolygon(color, 1, triangle);
                    break;
            }
        }
    }

    public class PieChart : ChartBase
    {
        public PieChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Pie;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            if (_series.Count == 0 || _series[0].Points.Count == 0)
                return;

            var series = _series[0];
            var total = series.Points.Sum(p => p.Value ?? 0);
            if (total == 0)
                return;

            var bounds = PlotArea.Bounds;
            var centerX = bounds.Left + bounds.Width / 2;
            var centerY = bounds.Top + bounds.Height / 2;
            var radius = Math.Min(bounds.Width, bounds.Height) / 2;

            double startAngle = 0;
            foreach (var point in series.Points)
            {
                var value = point.Value ?? 0;
                var sweepAngle = (value / total) * 360;

                context.FillPie(
                    point.FillColor ?? series.Color,
                    centerX,
                    centerY,
                    radius,
                    startAngle,
                    sweepAngle
                );

                if (point.BorderColor != null)
                {
                    context.DrawPieBorder(point.BorderColor, point.BorderWidth, centerX, centerY, radius, startAngle, sweepAngle);
                }

                var midAngle = startAngle + sweepAngle / 2;
                var labelX = centerX + (radius * 0.7) * Math.Cos(midAngle * Math.PI / 180);
                var labelY = centerY + (radius * 0.7) * Math.Sin(midAngle * Math.PI / 180);

                var percentage = (value / total * 100).ToString("F1") + "%";
                context.DrawText(percentage, labelX, labelY, new ChartTextFormat
                {
                    FontSize = 10,
                    HorizontalAlignment = ChartHorizontalAlignment.Center,
                    VerticalAlignment = ChartVerticalAlignment.Center
                });

                startAngle += sweepAngle;
            }
        }
    }

    public class AreaChart : ChartBase
    {
        public AreaChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Area;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            var bounds = PlotArea.Bounds;
            var maxPoints = _series.Max(s => s.Points.Count);

            if (maxPoints == 0)
                return;

            var stepX = bounds.Width / (maxPoints - 1);

            foreach (var series in _series)
            {
                var points = new List<ChartPoint>();
                for (int i = 0; i < series.Points.Count; i++)
                {
                    var point = series.Points[i];
                    var x = bounds.Left + i * stepX;
                    var y = CalculateY(point.Value ?? 0, bounds);
                    points.Add(new ChartPoint { X = x, Y = y });
                }

                points.Add(new ChartPoint { X = bounds.Right, Y = bounds.Bottom });
                points.Add(new ChartPoint { X = bounds.Left, Y = bounds.Bottom });

                context.FillPolygon(series.Color + "80", points);

                if (points.Count > 1)
                {
                    var linePoints = points.Take(series.Points.Count).ToList();
                    for (int i = 0; i < linePoints.Count - 1; i++)
                    {
                        context.DrawLine(series.Color, series.LineWidth, linePoints[i], linePoints[i + 1]);
                    }
                }
            }
        }

        private double CalculateY(double value, ChartRectangle bounds)
        {
            var axisMin = Axes.YAxis.Min ?? 0;
            var axisMax = Axes.YAxis.Max ?? 100;
            return bounds.Bottom - ((value - axisMin) / (axisMax - axisMin)) * bounds.Height;
        }
    }

    public class ScatterChart : ChartBase
    {
        public ScatterChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Scatter;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            if (_series.Count == 0)
                return;

            foreach (var series in _series)
            {
                for (int i = 0; i < series.Points.Count; i++)
                {
                    var point = series.Points[i];
                    var x = CalculateX(point.XValue ?? i, context.ChartArea.Bounds);
                    var y = CalculateY(point.Value ?? 0, context.ChartArea.Bounds);

                    RenderMarker(context, x, y, series.MarkerStyle, series.MarkerSize, point.FillColor ?? series.Color);

                    if (Subtype == ChartSubtype.ScatterLine || Subtype == ChartSubtype.ScatterSmooth || Subtype == ChartSubtype.ScatterSmoothLine)
                    {
                        if (i < series.Points.Count - 1)
                        {
                            var nextPoint = series.Points[i + 1];
                            var nextX = CalculateX(nextPoint.XValue ?? i + 1, context.ChartArea.Bounds);
                            var nextY = CalculateY(nextPoint.Value ?? 0, context.ChartArea.Bounds);
                            context.DrawLine(series.Color, series.LineWidth, new ChartPoint(x, y), new ChartPoint(nextX, nextY));
                        }
                    }
                }
            }
        }

        private double CalculateX(double value, ChartRectangle bounds)
        {
            var axisMin = Axes.XAxis.Min ?? 0;
            var axisMax = Axes.XAxis.Max ?? 100;
            return bounds.Left + ((value - axisMin) / (axisMax - axisMin)) * bounds.Width;
        }

        private double CalculateY(double value, ChartRectangle bounds)
        {
            var axisMin = Axes.YAxis.Min ?? 0;
            var axisMax = Axes.YAxis.Max ?? 100;
            return bounds.Bottom - ((value - axisMin) / (axisMax - axisMin)) * bounds.Height;
        }

        private void RenderMarker(ChartRenderingContext context, double x, double y, MarkerStyle style, double size, string color)
        {
            switch (style)
            {
                case MarkerStyle.Circle:
                    context.FillEllipse(color, new ChartPoint(x - size / 2, y - size / 2), size, size);
                    break;
                case MarkerStyle.Square:
                    context.FillRectangle(color, new ChartRectangle(x - size / 2, y - size / 2, size, size));
                    break;
                case MarkerStyle.Diamond:
                    var diamond = new List<ChartPoint>
                    {
                        new ChartPoint(x, y - size / 2),
                        new ChartPoint(x + size / 2, y),
                        new ChartPoint(x, y + size / 2),
                        new ChartPoint(x - size / 2, y)
                    };
                    context.FillPolygon(color, diamond);
                    break;
            }
        }
    }

    public class StockChart : ChartBase
    {
        public StockChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Stock;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            if (_series.Count < 4)
                return;

            var bounds = PlotArea.Bounds;
            var highSeries = _series[0];
            var lowSeries = _series[1];
            var openSeries = _series[2];
            var closeSeries = _series[3];

            var maxPoints = Math.Max(Math.Max(highSeries.Points.Count, lowSeries.Points.Count),
                                    Math.Max(openSeries.Points.Count, closeSeries.Points.Count));

            var stepX = bounds.Width / maxPoints;

            for (int i = 0; i < maxPoints; i++)
            {
                var high = highSeries.Points.ElementAtOrDefault(i)?.Value ?? 0;
                var low = lowSeries.Points.ElementAtOrDefault(i)?.Value ?? 0;
                var open = openSeries.Points.ElementAtOrDefault(i)?.Value ?? 0;
                var close = closeSeries.Points.ElementAtOrDefault(i)?.Value ?? 0;

                var x = bounds.Left + i * stepX + stepX / 2;
                var yHigh = CalculateY(high, bounds);
                var yLow = CalculateY(low, bounds);

                context.DrawLine("Black", 1, new ChartPoint(x, yHigh), new ChartPoint(x, yLow));

                var yOpen = CalculateY(open, bounds);
                var yClose = CalculateY(close, bounds);

                var rect = new ChartRectangle(x - 5, Math.Min(yOpen, yClose), 10, Math.Abs(yClose - yOpen));
                var color = close >= open ? "Green" : "Red";
                context.FillRectangle(color, rect);
            }
        }

        private double CalculateY(double value, ChartRectangle bounds)
        {
            var axisMin = Axes.YAxis.Min ?? 0;
            var axisMax = Axes.YAxis.Max ?? 100;
            return bounds.Bottom - ((value - axisMin) / (axisMax - axisMin)) * bounds.Height;
        }
    }

    public class RadarChart : ChartBase
    {
        public RadarChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Radar;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            if (_series.Count == 0 || _series[0].Points.Count == 0)
                return;

            var bounds = PlotArea.Bounds;
            var centerX = bounds.Left + bounds.Width / 2;
            var centerY = bounds.Top + bounds.Height / 2;
            var radius = Math.Min(bounds.Width, bounds.Height) / 2;

            var points = _series[0].Points.Count;
            var angleStep = 360.0 / points;

            for (int level = 1; level <= 5; level++)
            {
                var levelRadius = radius * level / 5;
                for (int i = 0; i < points; i++)
                {
                    var angle = (angleStep * i - 90) * Math.PI / 180;
                    var x = centerX + levelRadius * Math.Cos(angle);
                    var y = centerY + levelRadius * Math.Sin(angle);
                    var nextAngle = (angleStep * ((i + 1) % points) - 90) * Math.PI / 180;
                    var nextX = centerX + levelRadius * Math.Cos(nextAngle);
                    var nextY = centerY + levelRadius * Math.Sin(nextAngle);
                    context.DrawLine("LightGray", 1, new ChartPoint(x, y), new ChartPoint(nextX, nextY));
                }
            }

            foreach (var series in _series)
            {
                var seriesPoints = new List<ChartPoint>();
                for (int i = 0; i < series.Points.Count; i++)
                {
                    var value = series.Points[i].Value ?? 0;
                    var angle = (angleStep * i - 90) * Math.PI / 180;
                    var pointRadius = (value / (Axes.YAxis.Max ?? 100)) * radius;
                    seriesPoints.Add(new ChartPoint
                    {
                        X = centerX + pointRadius * Math.Cos(angle),
                        Y = centerY + pointRadius * Math.Sin(angle)
                    });
                }

                for (int i = 0; i < seriesPoints.Count; i++)
                {
                    var current = seriesPoints[i];
                    var next = seriesPoints[(i + 1) % seriesPoints.Count];
                    context.DrawLine(series.Color, series.LineWidth, current, next);
                }
            }
        }
    }

    public class SurfaceChart : ChartBase
    {
        public SurfaceChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Surface;
    }

    public class DoughnutChart : ChartBase
    {
        public DoughnutChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Doughnut;
        public double HoleSize { get; set; } = 0.5;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            if (_series.Count == 0 || _series[0].Points.Count == 0)
                return;

            var series = _series[0];
            var total = series.Points.Sum(p => p.Value ?? 0);
            if (total == 0)
                return;

            var bounds = PlotArea.Bounds;
            var centerX = bounds.Left + bounds.Width / 2;
            var centerY = bounds.Top + bounds.Height / 2;
            var outerRadius = Math.Min(bounds.Width, bounds.Height) / 2;
            var innerRadius = outerRadius * HoleSize;

            double startAngle = 0;
            foreach (var point in series.Points)
            {
                var value = point.Value ?? 0;
                var sweepAngle = (value / total) * 360;

                context.FillRing(
                    point.FillColor ?? series.Color,
                    centerX,
                    centerY,
                    innerRadius,
                    outerRadius,
                    startAngle,
                    sweepAngle
                );

                startAngle += sweepAngle;
            }
        }
    }

    public class BubbleChart : ChartBase
    {
        public BubbleChart(ChartSubtype subtype = ChartSubtype.None)
        {
            Subtype = subtype;
        }

        protected override ChartType GetChartType() => ChartType.Bubble;

        public override void Render(ChartRenderingContext context)
        {
            base.Render(context);

            var bounds = PlotArea.Bounds;

            foreach (var series in _series)
            {
                for (int i = 0; i < series.Points.Count; i++)
                {
                    var point = series.Points[i];
                    var x = CalculateX(point.XValue ?? i, bounds);
                    var y = CalculateY(point.Value ?? 0, bounds);
                    var size = (point.BubbleSize ?? 10) * 2;

                    context.FillEllipse(
                        point.FillColor ?? series.Color,
                        new ChartPoint(x - size / 2, y - size / 2),
                        size,
                        size
                    );
                }
            }
        }

        private double CalculateX(double value, ChartRectangle bounds)
        {
            var axisMin = Axes.XAxis.Min ?? 0;
            var axisMax = Axes.XAxis.Max ?? 100;
            return bounds.Left + ((value - axisMin) / (axisMax - axisMin)) * bounds.Width;
        }

        private double CalculateY(double value, ChartRectangle bounds)
        {
            var axisMin = Axes.YAxis.Min ?? 0;
            var axisMax = Axes.YAxis.Max ?? 100;
            return bounds.Bottom - ((value - axisMin) / (axisMax - axisMin)) * bounds.Height;
        }
    }

    public class TreemapChart : ChartBase
    {
        protected override ChartType GetChartType() => ChartType.Treemap;
    }

    public class SunburstChart : ChartBase
    {
        protected override ChartType GetChartType() => ChartType.Sunburst;
    }

    public class HistogramChart : ChartBase
    {
        protected override ChartType GetChartType() => ChartType.Histogram;
    }

    public class BoxAndWhiskerChart : ChartBase
    {
        protected override ChartType GetChartType() => ChartType.BoxAndWhisker;
    }

    public class WaterfallChart : ChartBase
    {
        protected override ChartType GetChartType() => ChartType.Waterfall;
    }

    public class FunnelChart : ChartBase
    {
        protected override ChartType GetChartType() => ChartType.Funnel;
    }

    public class ChartSeries
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public List<ChartDataPoint> Points { get; set; } = new List<ChartDataPoint>();
        public double LineWidth { get; set; } = 2;
        public MarkerStyle MarkerStyle { get; set; } = MarkerStyle.None;
        public double MarkerSize { get; set; } = 5;
    }

    public class ChartDataPoint
    {
        public double? Value { get; set; }
        public double? XValue { get; set; }
        public double? BubbleSize { get; set; }
        public string FillColor { get; set; }
        public string BorderColor { get; set; }
        public double BorderWidth { get; set; } = 1;
        public string Label { get; set; }
    }

    public class ChartPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class ChartRectangle
    {
        public double Left { get; }
        public double Top { get; }
        public double Width { get; }
        public double Height { get; }
        public double Right => Left + Width;
        public double Bottom => Top + Height;

        public ChartRectangle(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }

    public class ChartAxes
    {
        public ChartAxis XAxis { get; set; } = new ChartAxis();
        public ChartAxis YAxis { get; set; } = new ChartAxis();
        public ChartAxis ZAxis { get; set; } = new ChartAxis();

        public void Render(ChartRenderingContext context)
        {
            XAxis.Render(context, true);
            YAxis.Render(context, false);
        }
    }

    public class ChartAxis
    {
        public string Title { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double MajorUnit { get; set; } = 10;
        public double MinorUnit { get; set; } = 5;
        public bool ShowMajorGridlines { get; set; } = true;
        public bool ShowMinorGridlines { get; set; } = false;
        public bool LogarithmicScale { get; set; } = false;
        public bool ReverseValues { get; set; } = false;

        public void Render(ChartRenderingContext context, bool isXAxis)
        {
        }
    }

    public class ChartLegend
    {
        public bool Visible { get; set; } = true;
        public ChartHorizontalAlignment HorizontalAlignment { get; set; } = ChartHorizontalAlignment.Right;
        public ChartVerticalAlignment VerticalAlignment { get; set; } = ChartVerticalAlignment.Center;
        public string Position { get; set; } = "Right";

        public void Render(ChartRenderingContext context, IReadOnlyList<ChartSeries> series)
        {
            if (!Visible || series.Count == 0)
                return;

            var x = 10;
            var y = 10;

            foreach (var s in series)
            {
                context.FillRectangle(s.Color, new ChartRectangle(x, y, 15, 15));
                context.DrawText(s.Name, x + 20, y + 2, new ChartTextFormat { FontSize = 10 });
                y += 20;
            }
        }
    }

    public class ChartPlotArea
    {
        public ChartRectangle Bounds { get; set; }
        public string FillColor { get; set; } = "White";
        public string BorderColor { get; set; } = "Black";
        public double BorderWidth { get; set; } = 1;
    }

    public enum ChartHorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public enum ChartVerticalAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public enum ChartFontWeight
    {
        Normal,
        Bold
    }

    public class ChartTextFormat
    {
        public string FontFamily { get; set; } = "Arial";
        public double FontSize { get; set; } = 10;
        public string FontColor { get; set; } = "Black";
        public ChartFontWeight FontWeight { get; set; } = ChartFontWeight.Normal;
        public bool Italic { get; set; } = false;
        public ChartHorizontalAlignment HorizontalAlignment { get; set; } = ChartHorizontalAlignment.Left;
        public ChartVerticalAlignment VerticalAlignment { get; set; } = ChartVerticalAlignment.Top;
    }

    public class ChartRenderingContext
    {
        public ChartRectangle ChartArea { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public virtual void DrawText(string text, double x, double y, ChartTextFormat format) { }
        public virtual void FillRectangle(string color, ChartRectangle rect) { }
        public virtual void DrawRectangle(string color, double width, ChartRectangle rect) { }
        public virtual void DrawLine(string color, double width, ChartPoint start, ChartPoint end) { }
        public virtual void FillEllipse(string color, ChartPoint center, double width, double height) { }
        public virtual void DrawEllipse(string color, double width, ChartPoint center, double width2, double height) { }
        public virtual void FillPie(string color, double centerX, double centerY, double radius, double startAngle, double sweepAngle) { }
        public virtual void DrawPieBorder(string color, double width, double centerX, double centerY, double radius, double startAngle, double sweepAngle) { }
        public virtual void FillRing(string color, double centerX, double centerY, double innerRadius, double outerRadius, double startAngle, double sweepAngle) { }
        public virtual void FillPolygon(string color, List<ChartPoint> points) { }
        public virtual void DrawPolygon(string color, double width, List<ChartPoint> points) { }
    }
}

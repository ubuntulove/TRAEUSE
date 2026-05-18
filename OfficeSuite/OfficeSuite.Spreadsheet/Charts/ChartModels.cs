using System;
using System.Collections.Generic;

namespace OfficeSuite.Spreadsheet.Charts
{
    public class Chart : OfficeSuite.Core.Models.ModelBase
    {
        private string _id;
        private string _name;
        private ChartType _chartType;
        private ChartTitle _title;
        private ChartLegend _legend;
        private ChartPlotArea _plotArea;
        private ChartSeriesCollection _series;
        private ChartAxes _axes;
        private ChartArea _chartArea;
        private bool _showLegend;
        private bool _showTitle;
        private bool _showGridlines;
        private bool _autoScaling;
        private double _width;
        private double _height;
        private double _left;
        private double _Top;

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

        public ChartType ChartType
        {
            get => _chartType;
            set => SetProperty(ref _chartType, value);
        }

        public ChartTitle Title
        {
            get => _title ??= new ChartTitle();
            set => SetProperty(ref _title, value);
        }

        public ChartLegend Legend
        {
            get => _legend ??= new ChartLegend();
            set => SetProperty(ref _legend, value);
        }

        public ChartPlotArea PlotArea
        {
            get => _plotArea ??= new ChartPlotArea();
            set => SetProperty(ref _plotArea, value);
        }

        public ChartSeriesCollection Series
        {
            get => _series ??= new ChartSeriesCollection();
            set => SetProperty(ref _series, value);
        }

        public ChartAxes Axes
        {
            get => _axes ??= new ChartAxes();
            set => SetProperty(ref _axes, value);
        }

        public ChartArea ChartArea
        {
            get => _chartArea ??= new ChartArea();
            set => SetProperty(ref _chartArea, value);
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set => SetProperty(ref _showLegend, value);
        }

        public bool ShowTitle
        {
            get => _showTitle;
            set => SetProperty(ref _showTitle, value);
        }

        public bool ShowGridlines
        {
            get => _showGridlines;
            set => SetProperty(ref _showGridlines, value);
        }

        public bool AutoScaling
        {
            get => _autoScaling;
            set => SetProperty(ref _autoScaling, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }

        public Chart()
        {
            _id = Guid.NewGuid().ToString();
            _chartType = ChartType.Column;
            _series = new ChartSeriesCollection();
            _showLegend = true;
            _showGridlines = true;
            _width = 600;
            _height = 400;
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
        Stock,
        Radar,
        Doughnut,
        Surface,
        Bubble,
        Gantt,
        Combo
    }

    public class ChartTitle : OfficeSuite.Core.Models.ModelBase
    {
        private string _text;
        private bool _visible;
        private ChartTextFormat _format;
        private HorizontalAlignment _alignment;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public ChartTextFormat Format
        {
            get => _format ??= new ChartTextFormat();
            set => SetProperty(ref _format, value);
        }

        public HorizontalAlignment Alignment
        {
            get => _alignment;
            set => SetProperty(ref _alignment, value);
        }
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public class ChartTextFormat : OfficeSuite.Core.Models.ModelBase
    {
        private string _fontFamily;
        private double _fontSize;
        private System.Windows.Media.Color _fontColor;
        private bool _bold;
        private bool _italic;

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
    }

    public class ChartLegend : OfficeSuite.Core.Models.ModelBase
    {
        private bool _visible;
        private LegendPosition _position;
        private ChartTextFormat _format;
        private HorizontalAlignment _alignment;

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public LegendPosition Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public ChartTextFormat Format
        {
            get => _format ??= new ChartTextFormat();
            set => SetProperty(ref _format, value);
        }

        public HorizontalAlignment Alignment
        {
            get => _alignment;
            set => SetProperty(ref _alignment, value);
        }
    }

    public enum LegendPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        TopRight,
        None
    }

    public class ChartPlotArea : OfficeSuite.Core.Models.ModelBase
    {
        private FillFormat _fill;
        private BorderFormat _border;
        private double _width;
        private double _height;
        private double _left;
        private double _Top;

        public FillFormat Fill
        {
            get => _fill ??= new FillFormat();
            set => SetProperty(ref _fill, value);
        }

        public BorderFormat Border
        {
            get => _border ??= new BorderFormat();
            set => SetProperty(ref _border, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }
    }

    public class FillFormat : OfficeSuite.Core.Models.ModelBase
    {
        private FillType _type;
        private System.Windows.Media.Color _color;
        private double _opacity;
        private GradientFill _gradient;
        private PatternFill _pattern;

        public FillType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public GradientFill Gradient
        {
            get => _gradient;
            set => SetProperty(ref _gradient, value);
        }

        public PatternFill Pattern
        {
            get => _pattern;
            set => SetProperty(ref _pattern, value);
        }
    }

    public enum FillType
    {
        None,
        Solid,
        Gradient,
        Pattern,
        Automatic
    }

    public class GradientFill : OfficeSuite.Core.Models.ModelBase
    {
        private GradientType _type;
        private GradientStopCollection _stops;
        private double _angle;
        private double _scale;

        public GradientType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public GradientStopCollection Stops
        {
            get => _stops ??= new GradientStopCollection();
            set => SetProperty(ref _stops, value);
        }

        public double Angle
        {
            get => _angle;
            set => SetProperty(ref _angle, value);
        }

        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }
    }

    public enum GradientType
    {
        Linear,
        Radial
    }

    public class GradientStop : OfficeSuite.Core.Models.ModelBase
    {
        private double _position;
        private System.Windows.Media.Color _color;

        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
    }

    public class GradientStopCollection : List<GradientStop> { }

    public class PatternFill : OfficeSuite.Core.Models.ModelBase
    {
        private PatternType _patternType;
        private System.Windows.Media.Color _foregroundColor;
        private System.Windows.Media.Color _backgroundColor;

        public PatternType PatternType
        {
            get => _patternType;
            set => SetProperty(ref _patternType, value);
        }

        public System.Windows.Media.Color ForegroundColor
        {
            get => _foregroundColor;
            set => SetProperty(ref _foregroundColor, value);
        }

        public System.Windows.Media.Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }
    }

    public enum PatternType
    {
        None,
        Solid,
        DarkDownwardDiagonal,
        DarkHorizontal,
        DarkUpwardDiagonal,
        DashedDownwardDiagonal,
        DashedHorizontal,
        DashedUpwardDiagonal,
        DashedVertical,
        DiagonalBrick,
        DiagonalCrosshatch,
        Divot,
        DottedDiamond,
        DottedGrid,
        ForwardDiagonal,
        Horizontal,
        LargeCheckerBoard,
        LargeConfetti,
        LargeGrid,
        LightDownwardDiagonal,
        LightHorizontal,
        LightUpwardDiagonal,
        LightVertical,
        NarrowHorizontal,
        NarrowVertical,
        OutlinedDiamond,
        Packedie,
        LargeCheckerBoard,
        SmallCheckerBoard,
        SmallConfetti,
        SmallGrid,
        SolidDiamond,
        Sphere,
        Trellis,
        Wave,
        Weave,
        WideDownwardDiagonal,
        WideUpwardDiagonal,
        ZigZag
    }

    public class BorderFormat : OfficeSuite.Core.Models.ModelBase
    {
        private LineType _type;
        private System.Windows.Media.Color _color;
        private double _width;
        private LineDashStyle _dashStyle;

        public LineType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public LineDashStyle DashStyle
        {
            get => _dashStyle;
            set => SetProperty(ref _dashStyle, value);
        }
    }

    public enum LineType
    {
        None,
        Solid,
        Dashed,
        Dotted,
        Gradient
    }

    public enum LineDashStyle
    {
        Solid,
        Dash,
        DashDot,
        DashDotDot,
        Dot,
        LongDash,
        LongDashDot,
        RoundDot,
        SquareDot,
        LongDashDotDot
    }

    public class ChartSeries : OfficeSuite.Core.Models.ModelBase
    {
        private string _name;
        private ChartSeriesType _seriesType;
        private List<object> _values;
        private List<string> _categories;
        private ChartDataLabel _dataLabel;
        private LineFormat _line;
        private MarkerFormat _marker;
        private bool _visible;
        private int _order;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ChartSeriesType SeriesType
        {
            get => _seriesType;
            set => SetProperty(ref _seriesType, value);
        }

        public List<object> Values
        {
            get => _values ??= new List<object>();
            set => SetProperty(ref _values, value);
        }

        public List<string> Categories
        {
            get => _categories ??= new List<string>();
            set => SetProperty(ref _categories, value);
        }

        public ChartDataLabel DataLabel
        {
            get => _dataLabel;
            set => SetProperty(ref _dataLabel, value);
        }

        public LineFormat Line
        {
            get => _line ??= new LineFormat();
            set => SetProperty(ref _line, value);
        }

        public MarkerFormat Marker
        {
            get => _marker ??= new MarkerFormat();
            set => SetProperty(ref _marker, value);
        }

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public int Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }
    }

    public enum ChartSeriesType
    {
        Primary,
        Secondary
    }

    public class ChartSeriesCollection : List<ChartSeries> { }

    public class ChartDataLabel : OfficeSuite.Core.Models.ModelBase
    {
        private bool _visible;
        private string _format;
        private DataLabelPosition _position;
        private ChartTextFormat _formatText;
        private bool _showValue;
        private bool _showCategory;
        private bool _showSeries;
        private bool _showPercentage;
        private bool _showBubbleSize;

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public string Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        public DataLabelPosition Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public ChartTextFormat FormatText
        {
            get => _formatText ??= new ChartTextFormat();
            set => SetProperty(ref _formatText, value);
        }

        public bool ShowValue
        {
            get => _showValue;
            set => SetProperty(ref _showValue, value);
        }

        public bool ShowCategory
        {
            get => _showCategory;
            set => SetProperty(ref _showCategory, value);
        }

        public bool ShowSeries
        {
            get => _showSeries;
            set => SetProperty(ref _showSeries, value);
        }

        public bool ShowPercentage
        {
            get => _showPercentage;
            set => SetProperty(ref _showPercentage, value);
        }

        public bool ShowBubbleSize
        {
            get => _showBubbleSize;
            set => SetProperty(ref _showBubbleSize, value);
        }
    }

    public enum DataLabelPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        Center,
        InsideBase,
        InsideEnd,
        OutsideEnd,
        BestFit,
        Automatic
    }

    public class LineFormat : OfficeSuite.Core.Models.ModelBase
    {
        private LineType _type;
        private System.Windows.Media.Color _color;
        private double _width;
        private LineDashStyle _dashStyle;
        private LineCap _cap;
        private LineJoin _join;

        public LineType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public LineDashStyle DashStyle
        {
            get => _dashStyle;
            set => SetProperty(ref _dashStyle, value);
        }

        public LineCap Cap
        {
            get => _cap;
            set => SetProperty(ref _cap, value);
        }

        public LineJoin Join
        {
            get => _join;
            set => SetProperty(ref _join, value);
        }
    }

    public enum LineCap
    {
        Flat,
        Round,
        Square
    }

    public enum LineJoin
    {
        Round,
        Bevel,
        Miter
    }

    public class MarkerFormat : OfficeSuite.Core.Models.ModelBase
    {
        private MarkerType _type;
        private System.Windows.Media.Color _color;
        private System.Windows.Media.Color _borderColor;
        private double _size;
        private bool _visible;

        public MarkerType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public System.Windows.Media.Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public System.Windows.Media.Color BorderColor
        {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        public double Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }
    }

    public enum MarkerType
    {
        None,
        Circle,
        Dash,
        Diamond,
        Dot,
        Square,
        Triangle,
        X,
        Auto
    }

    public class ChartAxes : OfficeSuite.Core.Models.ModelBase
    {
        private ChartAxis _primaryCategoryAxis;
        private ChartAxis _primaryValueAxis;
        private ChartAxis _secondaryCategoryAxis;
        private ChartAxis _secondaryValueAxis;
        private ChartAxis _seriesAxis;

        public ChartAxis PrimaryCategoryAxis
        {
            get => _primaryCategoryAxis ??= new ChartAxis();
            set => SetProperty(ref _primaryCategoryAxis, value);
        }

        public ChartAxis PrimaryValueAxis
        {
            get => _primaryValueAxis ??= new ChartAxis();
            set => SetProperty(ref _primaryValueAxis, value);
        }

        public ChartAxis SecondaryCategoryAxis
        {
            get => _secondaryCategoryAxis ??= new ChartAxis();
            set => SetProperty(ref _secondaryCategoryAxis, value);
        }

        public ChartAxis SecondaryValueAxis
        {
            get => _secondaryValueAxis ??= new ChartAxis();
            set => SetProperty(ref _secondaryValueAxis, value);
        }

        public ChartAxis SeriesAxis
        {
            get => _seriesAxis ??= new ChartAxis();
            set => SetProperty(ref _seriesAxis, value);
        }
    }

    public class ChartAxis : OfficeSuite.Core.Models.ModelBase
    {
        private string _title;
        private bool _hasTitle;
        private bool _visible;
        private AxisPosition _position;
        private AxisType _type;
        private AxisScale _scale;
        private double _minimum;
        private double _maximum;
        private double _majorUnit;
        private double _minorUnit;
        private bool _logarithmicScale;
        private double _logBase;
        private bool _showMajorGridlines;
        private bool _showMinorGridlines;
        private LineFormat _majorGridlines;
        private LineFormat _minorGridlines;
        private AxisCrosses _crosses;
        private double _crossesAt;
        private bool _reversePlotOrder;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool HasTitle
        {
            get => _hasTitle;
            set => SetProperty(ref _hasTitle, value);
        }

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public AxisPosition Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public AxisType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public AxisScale Scale
        {
            get => _scale ??= new AxisScale();
            set => SetProperty(ref _scale, value);
        }

        public double Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        public double Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        public double MajorUnit
        {
            get => _majorUnit;
            set => SetProperty(ref _majorUnit, value);
        }

        public double MinorUnit
        {
            get => _minorUnit;
            set => SetProperty(ref _minorUnit, value);
        }

        public bool LogarithmicScale
        {
            get => _logarithmicScale;
            set => SetProperty(ref _logarithmicScale, value);
        }

        public double LogBase
        {
            get => _logBase;
            set => SetProperty(ref _logBase, value);
        }

        public bool ShowMajorGridlines
        {
            get => _showMajorGridlines;
            set => SetProperty(ref _showMajorGridlines, value);
        }

        public bool ShowMinorGridlines
        {
            get => _showMinorGridlines;
            set => SetProperty(ref _showMinorGridlines, value);
        }

        public LineFormat MajorGridlines
        {
            get => _majorGridlines ??= new LineFormat();
            set => SetProperty(ref _majorGridlines, value);
        }

        public LineFormat MinorGridlines
        {
            get => _minorGridlines ??= new LineFormat();
            set => SetProperty(ref _minorGridlines, value);
        }

        public AxisCrosses Crosses
        {
            get => _crosses;
            set => SetProperty(ref _crosses, value);
        }

        public double CrossesAt
        {
            get => _crossesAt;
            set => SetProperty(ref _crossesAt, value);
        }

        public bool ReversePlotOrder
        {
            get => _reversePlotOrder;
            set => SetProperty(ref _reversePlotOrder, value);
        }
    }

    public enum AxisPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        Automatic
    }

    public enum AxisType
    {
        Category,
        Value,
        Time,
        Series
    }

    public class AxisScale : OfficeSuite.Core.Models.ModelBase
    {
        private double _minimum;
        private double _maximum;
        private bool _autoMinimum;
        private bool _autoMaximum;

        public double Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        public double Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        public bool AutoMinimum
        {
            get => _autoMinimum;
            set => SetProperty(ref _autoMinimum, value);
        }

        public bool AutoMaximum
        {
            get => _autoMaximum;
            set => SetProperty(ref _autoMaximum, value);
        }
    }

    public enum AxisCrosses
    {
        Automatic,
        AxisCrossesAtZero,
        Maximum,
        Custom
    }

    public class ChartArea : OfficeSuite.Core.Models.ModelBase
    {
        private FillFormat _fill;
        private BorderFormat _border;
        private double _width;
        private double _height;
        private double _left;
        private double _Top;

        public FillFormat Fill
        {
            get => _fill ??= new FillFormat();
            set => SetProperty(ref _fill, value);
        }

        public BorderFormat Border
        {
            get => _border ??= new BorderFormat();
            set => SetProperty(ref _border, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }
    }

    public class ChartCreator
    {
        public Chart CreateColumnChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Column,
                Name = "Column Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;
            chart.Legend.Position = LegendPosition.Right;

            return chart;
        }

        public Chart CreateBarChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Bar,
                Name = "Bar Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;
            chart.Legend.Position = LegendPosition.Right;

            return chart;
        }

        public Chart CreateLineChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Line,
                Name = "Line Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;
            chart.ShowGridlines = true;

            return chart;
        }

        public Chart CreatePieChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Pie,
                Name = "Pie Chart"
            };

            chart.Legend.Position = LegendPosition.Right;
            chart.ShowLegend = true;

            return chart;
        }

        public Chart CreateAreaChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Area,
                Name = "Area Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;

            return chart;
        }

        public Chart CreateScatterChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Scatter,
                Name = "Scatter Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;
            chart.ShowGridlines = true;

            return chart;
        }

        public Chart CreateDoughnutChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Doughnut,
                Name = "Doughnut Chart"
            };

            chart.Legend.Position = LegendPosition.Right;
            chart.ShowLegend = true;

            return chart;
        }

        public Chart CreateRadarChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Radar,
                Name = "Radar Chart"
            };

            chart.ShowLegend = true;

            return chart;
        }

        public Chart CreateStockChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Stock,
                Name = "Stock Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;

            return chart;
        }

        public Chart CreateBubbleChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Bubble,
                Name = "Bubble Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;

            return chart;
        }

        public Chart CreateComboChart()
        {
            var chart = new Chart
            {
                ChartType = ChartType.Combo,
                Name = "Combo Chart"
            };

            chart.PrimaryValueAxis.HasTitle = true;
            chart.PrimaryCategoryAxis.HasTitle = true;
            chart.Legend.Position = LegendPosition.Right;

            return chart;
        }
    }

    public class ChartRenderer
    {
        public byte[] RenderToImage(Chart chart, int width, int height)
        {
            return Array.Empty<byte>();
        }

        public void RenderToStream(Chart chart, System.IO.Stream stream)
        {
        }

        public System.Windows.Media.Imaging.BitmapSource RenderToBitmap(Chart chart)
        {
            return null;
        }
    }
}

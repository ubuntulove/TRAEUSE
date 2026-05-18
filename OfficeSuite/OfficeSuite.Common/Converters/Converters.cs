using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OfficeSuite.Common.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            bool invert = parameter?.ToString()?.ToLower() == "invert";

            if (invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = value is Visibility v && v == Visibility.Visible;
            bool invert = parameter?.ToString()?.ToLower() == "invert";

            return invert ? !visibility : visibility;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool invert = parameter?.ToString()?.ToLower() == "invert";

            if (invert)
                return isNull ? Visibility.Visible : Visibility.Collapsed;

            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrEmpty(value as string);
            bool invert = parameter?.ToString()?.ToLower() == "invert";

            if (invert)
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;

            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString)
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color.ToString();
            }
            return "#000000";
        }
    }

    public class FontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isBold = value is bool b && b;
            return isBold ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontWeight weight)
            {
                return weight == FontWeights.Bold;
            }
            return false;
        }
    }

    public class FontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isItalic = value is bool b && b;
            return isItalic ? FontStyles.Italic : FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontStyle style)
            {
                return style == FontStyles.Italic;
            }
            return false;
        }
    }

    public class TextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isUnderline = value is bool b && b;
            return isUnderline ? TextDecorations.Underline : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value == TextDecorations.Underline;
        }
    }

    public class HorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string alignment = value as string ?? "Left";
            return alignment.ToLower() switch
            {
                "left" => HorizontalAlignment.Left,
                "center" => HorizontalAlignment.Center,
                "right" => HorizontalAlignment.Right,
                "justify" => HorizontalAlignment.Stretch,
                _ => HorizontalAlignment.Left
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HorizontalAlignment alignment)
            {
                return alignment switch
                {
                    HorizontalAlignment.Left => "Left",
                    HorizontalAlignment.Center => "Center",
                    HorizontalAlignment.Right => "Right",
                    HorizontalAlignment.Stretch => "Justify",
                    _ => "Left"
                };
            }
            return "Left";
        }
    }

    public class VerticalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string alignment = value as string ?? "Center";
            return alignment.ToLower() switch
            {
                "top" => VerticalAlignment.Top,
                "center" or "middle" => VerticalAlignment.Center,
                "bottom" => VerticalAlignment.Bottom,
                _ => VerticalAlignment.Center
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VerticalAlignment alignment)
            {
                return alignment switch
                {
                    VerticalAlignment.Top => "Top",
                    VerticalAlignment.Center => "Center",
                    VerticalAlignment.Bottom => "Bottom",
                    VerticalAlignment.Stretch => "Stretch",
                    _ => "Center"
                };
            }
            return "Center";
        }
    }

    public class ZoomLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double zoom)
            {
                return zoom / 100.0;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double scale)
            {
                return scale * 100.0;
            }
            return 100.0;
        }
    }

    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return $"{d:F0}%";
            }
            if (value is int i)
            {
                return $"{i}%";
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                s = s.TrimEnd('%').Trim();
                if (double.TryParse(s, out double result))
                {
                    return result;
                }
            }
            return 0;
        }
    }

    public class PageInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int page && parameter is int total)
            {
                return $"Page {page} of {total}";
            }
            return "Page 1 of 1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LineColumnInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string info)
            {
                return info;
            }
            return "Ln 1, Col 1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                string format = parameter as string ?? "g";
                return dt.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && DateTime.TryParse(s, culture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return DateTime.Now;
        }
    }

    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = bytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:F2} {sizes[order]}";
            }
            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CellAddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string address)
            {
                return address;
            }
            return "A1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FormulaResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is double d)
                return d.ToString("G", culture);

            if (value is bool b)
                return b.ToString(culture);

            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (double.TryParse(s, NumberStyles.Any, culture, out double d))
                    return d;

                if (bool.TryParse(s, out bool b))
                    return b;

                return s;
            }

            return value ?? string.Empty;
        }
    }

    public class ErrorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && s.StartsWith("#"))
            {
                return s;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToGridLineStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool show = value is bool b && b;
            return show ? GridLinesVisibility.All : GridLinesVisibility.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TabAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string alignment = value as string ?? "Left";
            return alignment.ToLower() switch
            {
                "left" => HorizontalAlignment.Left,
                "center" => HorizontalAlignment.Center,
                "right" => HorizontalAlignment.Right,
                "decimal" => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Left
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShapeTypeToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string shapeType = value as string ?? "Rectangle";

            return shapeType.ToLower() switch
            {
                "rectangle" => CreateRectangleGeometry(),
                "ellipse" => CreateEllipseGeometry(),
                "triangle" => CreateTriangleGeometry(),
                "diamond" => CreateDiamondGeometry(),
                "pentagon" => CreatePentagonGeometry(),
                "hexagon" => CreateHexagonGeometry(),
                _ => CreateRectangleGeometry()
            };
        }

        private Geometry CreateRectangleGeometry()
        {
            return new RectangleGeometry();
        }

        private Geometry CreateEllipseGeometry()
        {
            return new EllipseGeometry();
        }

        private Geometry CreateTriangleGeometry()
        {
            return Geometry.Parse("M 0,1 L 0.5,0 L 1,1 Z");
        }

        private Geometry CreateDiamondGeometry()
        {
            return Geometry.Parse("M 0.5,0 L 1,0.5 L 0.5,1 L 0,0.5 Z");
        }

        private Geometry CreatePentagonGeometry()
        {
            return Geometry.Parse("M 0.5,0 L 1,0.4 L 0.8,1 L 0.2,1 L 0,0.4 Z");
        }

        private Geometry CreateHexagonGeometry()
        {
            return Geometry.Parse("M 0.5,0 L 1,0.25 L 1,0.75 L 0.5,1 L 0,0.75 L 0,0.25 Z");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter != null)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }
            return Binding.DoNothing;
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return false;
        }
    }

    public class MultiValueEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return false;

            return values[0]?.Equals(values[1]) ?? false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double thickness)
            {
                return new Thickness(thickness);
            }
            if (value is string s)
            {
                var parts = s.Split(',');
                return parts.Length switch
                {
                    1 => new Thickness(double.Parse(parts[0])),
                    2 => new Thickness(double.Parse(parts[0]), double.Parse(parts[1]), double.Parse(parts[0]), double.Parse(parts[1])),
                    4 => new Thickness(double.Parse(parts[0]), double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3])),
                    _ => new Thickness(0)
                };
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PointCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string points)
            {
                var collection = new PointCollection();
                var parts = points.Split(';');
                foreach (var part in parts)
                {
                    var coords = part.Split(',');
                    if (coords.Length == 2 && double.TryParse(coords[0], out double x) && double.TryParse(coords[1], out double y))
                    {
                        collection.Add(new Point(x, y));
                    }
                }
                return collection;
            }
            return new PointCollection();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double radius)
            {
                return new CornerRadius(radius);
            }
            return new CornerRadius(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CornerRadius radius)
            {
                return radius.TopLeft;
            }
            return 0.0;
        }
    }

    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                using var stream = new System.IO.MemoryStream(bytes);
                var image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                return image;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GradientStopCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string gradientString)
            {
                var stops = new GradientStopCollection();
                var parts = gradientString.Split(';');

                foreach (var part in parts)
                {
                    var colorOffset = part.Split(':');
                    if (colorOffset.Length == 2 && double.TryParse(colorOffset[1], out double offset))
                    {
                        try
                        {
                            var color = (Color)ColorConverter.ConvertFromString(colorOffset[0]);
                            stops.Add(new GradientStop(color, offset));
                        }
                        catch { }
                    }
                }

                return stops;
            }
            return new GradientStopCollection();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

namespace OfficeSuite.Common.Helpers
{
    public static class CellAddressHelper
    {
        public static (int Row, int Column) Parse(string address)
        {
            if (string.IsNullOrEmpty(address))
                return (1, 1);

            int i = 0;
            while (i < address.Length && char.IsLetter(address[i]))
                i++;

            string columnPart = address.Substring(0, i);
            string rowPart = address.Substring(i);

            int column = ColumnFromLetters(columnPart);
            int row = int.TryParse(rowPart, out int r) ? r : 1;

            return (row, column);
        }

        public static string ToAddress(int row, int column)
        {
            return $"{ColumnToLetters(column)}{row}";
        }

        public static string ToRange((int Row, int Column) start, (int Row, int Column) end)
        {
            return $"{ToAddress(start.Row, start.Column)}:{ToAddress(end.Row, end.Column)}";
        }

        public static bool IsValid(string address)
        {
            if (string.IsNullOrEmpty(address))
                return false;

            int i = 0;
            while (i < address.Length && char.IsLetter(address[i]))
                i++;

            if (i == 0 || i == address.Length)
                return false;

            string rowPart = address.Substring(i);
            return int.TryParse(rowPart, out _);
        }

        private static int ColumnFromLetters(string letters)
        {
            int result = 0;
            foreach (char c in letters.ToUpper())
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result;
        }

        private static string ColumnToLetters(int column)
        {
            string result = string.Empty;
            while (column > 0)
            {
                column--;
                result = (char)('A' + column % 26) + result;
                column /= 26;
            }
            return result;
        }
    }

    public static class DateTimeHelper
    {
        public static int ToExcelSerial(DateTime date)
        {
            return (int)(date - new DateTime(1899, 12, 30)).TotalDays;
        }

        public static DateTime FromExcelSerial(int serial)
        {
            return new DateTime(1899, 12, 30).AddDays(serial);
        }

        public static double ToExcelTime(DateTime time)
        {
            return time.TimeOfDay.TotalDays;
        }

        public static DateTime FromExcelTime(double time)
        {
            return DateTime.Today.AddDays(time);
        }

        public static string FormatDate(DateTime date, string format)
        {
            return date.ToString(format);
        }

        public static string FormatTime(DateTime time, string format)
        {
            return time.ToString(format);
        }
    }

    public static class ColorHelper
    {
        public static Color FromHex(string hex)
        {
            try
            {
                return (Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            }
            catch
            {
                return Colors.Black;
            }
        }

        public static string ToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static Color Lighten(Color color, double amount)
        {
            return Color.FromArgb(
                color.A,
                (byte)Math.Min(255, color.R + (255 - color.R) * amount),
                (byte)Math.Min(255, color.G + (255 - color.G) * amount),
                (byte)Math.Min(255, color.B + (255 - color.B) * amount)
            );
        }

        public static Color Darken(Color color, double amount)
        {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * (1 - amount)),
                (byte)(color.G * (1 - amount)),
                (byte)(color.B * (1 - amount))
            );
        }

        public static Color Blend(Color color1, Color color2, double ratio)
        {
            return Color.FromArgb(
                (byte)((color1.A * (1 - ratio) + color2.A * ratio)),
                (byte)((color1.R * (1 - ratio) + color2.R * ratio)),
                (byte)((color1.G * (1 - ratio) + color2.G * ratio)),
                (byte)((color1.B * (1 - ratio) + color2.B * ratio))
            );
        }
    }

    public static class FileHelper
    {
        public static string GetFileExtension(string path)
        {
            return System.IO.Path.GetExtension(path)?.ToLower() ?? string.Empty;
        }

        public static string GetFileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public static string ChangeExtension(string path, string newExtension)
        {
            return System.IO.Path.ChangeExtension(path, newExtension);
        }

        public static bool IsOfficeFile(string path)
        {
            var extension = GetFileExtension(path);
            return extension switch
            {
                ".docx" or ".docm" or ".doc" => true,
                ".xlsx" or ".xlsm" or ".xls" or ".csv" => true,
                ".pptx" or ".pptm" or ".ppt" => true,
                _ => false
            };
        }
    }

    public static class StringHelper
    {
        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }

        public static string RemoveExtraSpaces(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(value.Trim(), @"\s+", " ");
        }

        public static string ToTitleCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static bool ContainsIgnoreCase(string source, string value)
        {
            if (source == null || value == null)
                return false;

            return source.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public static class MathHelper
    {
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double Round(double value, int decimals)
        {
            return Math.Round(value, decimals);
        }

        public static double Ceiling(double value, int significance)
        {
            return Math.Ceiling(value / significance) * significance;
        }

        public static double Floor(double value, int significance)
        {
            return Math.Floor(value / significance) * significance;
        }

        public static double Mean(params double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            return Sum(values) / values.Length;
        }

        public static double Sum(params double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            return values.Sum();
        }

        public static double Median(params double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            var sorted = values.OrderBy(v => v).ToArray();
            int mid = sorted.Length / 2;

            if (sorted.Length % 2 == 0)
                return (sorted[mid - 1] + sorted[mid]) / 2;

            return sorted[mid];
        }

        public static double StandardDeviation(params double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            var mean = Mean(values);
            var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumOfSquares / values.Length);
        }

        public static double Variance(params double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            var mean = Mean(values);
            return values.Average(v => Math.Pow(v - mean, 2));
        }
    }

    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsNumeric(string value)
        {
            return double.TryParse(value, out _);
        }

        public static bool IsInteger(string value)
        {
            return int.TryParse(value, out _);
        }

        public static bool IsDate(string value)
        {
            return DateTime.TryParse(value, out _);
        }
    }
}

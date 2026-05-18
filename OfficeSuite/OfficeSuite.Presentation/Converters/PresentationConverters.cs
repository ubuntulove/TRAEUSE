using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OfficeSuite.Presentation.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool invert = parameter?.ToString()?.ToLower() == "invert";
                if (invert)
                    boolValue = !boolValue;
                return boolValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Visibility visibility)
            {
                bool invert = parameter?.ToString()?.ToLower() == "invert";
                bool result = visibility == System.Windows.Visibility.Visible;
                return invert ? !result : result;
            }
            return false;
        }
    }

    public class ColorToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
                return new SolidColorBrush(color);
            if (value is string hex)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return brush.Color;
            return Colors.Transparent;
        }
    }

    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
                return enumValue.ToString();
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is Type enumType && value is string stringValue)
                return Enum.Parse(enumType, stringValue);
            return null;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            bool isNull = value == null;
            if (invert)
                isNull = !isNull;
            return isNull ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string format)
                return string.Format(format, value);
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
                return $"{doubleValue * 100:F0}%";
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                stringValue = stringValue.TrimEnd('%');
                if (double.TryParse(stringValue, out double result))
                    return result / 100;
            }
            return 0.0;
        }
    }

    public class SlideNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int slideNumber)
                return $"Slide {slideNumber}";
            return "Slide 1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.TextAlignment alignment)
            {
                return alignment switch
                {
                    System.Windows.TextAlignment.Left => HorizontalAlignment.Left,
                    System.Windows.TextAlignment.Center => HorizontalAlignment.Center,
                    System.Windows.TextAlignment.Right => HorizontalAlignment.Right,
                    System.Windows.TextAlignment.Justify => HorizontalAlignment.Stretch,
                    _ => HorizontalAlignment.Left
                };
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HorizontalAlignment alignment)
            {
                return alignment switch
                {
                    HorizontalAlignment.Left => System.Windows.TextAlignment.Left,
                    HorizontalAlignment.Center => System.Windows.TextAlignment.Center,
                    HorizontalAlignment.Right => System.Windows.TextAlignment.Right,
                    HorizontalAlignment.Stretch => System.Windows.TextAlignment.Justify,
                    _ => System.Windows.TextAlignment.Left
                };
            }
            return System.Windows.TextAlignment.Left;
        }
    }

    public class FontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBold && isBold)
                return FontWeights.Bold;
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontWeight weight)
                return weight == FontWeights.Bold;
            return false;
        }
    }

    public class FontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isItalic && isItalic)
                return FontStyles.Italic;
            return FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontStyle style)
                return style == FontStyles.Italic;
            return false;
        }
    }

    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                string format = parameter?.ToString() ?? "g";
                return dateTime.ToString(format, culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && DateTime.TryParse(stringValue, culture, DateTimeStyles.None, out DateTime result))
                return result;
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
                int order = 0;
                double size = bytes;

                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size /= 1024;
                }

                return $"{size:0.##} {sizes[order]}";
            }
            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                bool showIfEmpty = parameter?.ToString()?.ToLower() == "showifempty";
                bool hasItems = count > 0;
                
                if (showIfEmpty)
                    return hasItems ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                return hasItems ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double thickness)
            {
                if (parameter is string sides)
                {
                    var values = sides.Split(',');
                    if (values.Length == 4 &&
                        double.TryParse(values[0], out double left) &&
                        double.TryParse(values[1], out double top) &&
                        double.TryParse(values[2], out double right) &&
                        double.TryParse(values[3], out double bottom))
                    {
                        return new System.Windows.Thickness(left, top, right, bottom);
                    }
                }
                return new System.Windows.Thickness(thickness);
            }
            return new System.Windows.Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorBrightnessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                double factor = parameter is double d ? d : 0.2;
                return Color.FromArgb(color.A,
                    (byte)Math.Min(255, color.R * (1 + factor)),
                    (byte)Math.Min(255, color.G * (1 + factor)),
                    (byte)Math.Min(255, color.B * (1 + factor)));
            }
            return Colors.White;
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
            if (value is Color startColor && parameter is Color endColor)
            {
                return new GradientStopCollection
                {
                    new GradientStop(startColor, 0),
                    new GradientStop(endColor, 1)
                };
            }
            return new GradientStopCollection();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SlideTransitionToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string transition)
            {
                return transition switch
                {
                    "Fade" => "🎭",
                    "Push" => "➡️",
                    "Wipe" => "🧹",
                    "Split" => "✂️",
                    "Reveal" => "🔍",
                    "Cover" => "📦",
                    "Zoom" => "🔍",
                    _ => "➡️"
                };
            }
            return "➡️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AnimationTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string animation)
            {
                return animation switch
                {
                    "Appear" => "✨",
                    "Fade" => "🌫️",
                    "Fly In" => "✈️",
                    "Float In" => "🎈",
                    "Zoom In" => "🔍",
                    "Wipe" => "🧹",
                    "Grow & Turn" => "🌱",
                    "Spin" => "🔄",
                    "Bounce" => "🏀",
                    _ => "✨"
                };
            }
            return "✨";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ZoomLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double zoom)
                return $"{zoom * 100:F0}%";
            return "100%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                stringValue = stringValue.TrimEnd('%', ' ');
                if (double.TryParse(stringValue, out double result))
                    return result / 100;
            }
            return 1.0;
        }
    }

    public class ViewModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentView && parameter is string targetView)
            {
                return currentView == targetView 
                    ? System.Windows.Visibility.Visible 
                    : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue 
                    ? System.Windows.Visibility.Collapsed 
                    : System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double pixels)
            {
                bool isStar = parameter?.ToString()?.ToLower() == "star";
                return isStar 
                    ? new GridLength(pixels, GridUnitType.Star) 
                    : new GridLength(pixels);
            }
            return new GridLength(1, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SlideCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int current && values[1] is int total)
            {
                return $"Slide {current} of {total}";
            }
            return "Slide 1 of 1";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProgressToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress * 3.6;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Array array)
            {
                string separator = parameter?.ToString() ?? ", ";
                var items = new List<string>();
                foreach (var item in array)
                {
                    items.Add(item?.ToString() ?? "");
                }
                return string.Join(separator, items);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace OfficeSuite.Common.Helpers
{
    public static class FileHelper
    {
        public static string GetSafeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var result = new StringBuilder(fileName);
            foreach (var c in invalidChars)
            {
                result.Replace(c, '_');
            }
            return result.ToString();
        }

        public static string GetUniqueFileName(string directory, string fileName)
        {
            var safeName = GetSafeFileName(fileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(safeName);
            var ext = Path.GetExtension(safeName);
            var uniqueName = safeName;
            var counter = 1;

            while (File.Exists(Path.Combine(directory, uniqueName)))
            {
                uniqueName = $"{nameWithoutExt}_{counter}{ext}";
                counter++;
            }

            return uniqueName;
        }

        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
                return toPath;

            var fromUri = new Uri(AppendDirectorySeparator(fromPath));
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
                return toPath;

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparator(string path)
        {
            if (!Path.HasExtension(path) && !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path + Path.DirectorySeparatorChar;
            return path;
        }

        public static void EnsureDirectoryExists(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
                return 0;

            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }

        public static string FormatFileSize(long bytes)
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

        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";
        }

        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var invalidChars = Path.GetInvalidFileNameChars();
            return !fileName.Any(c => invalidChars.Contains(c));
        }

        public static void CopyDirectory(string sourceDir, string destDir, bool recursive = true)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            EnsureDirectoryExists(destDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string newDestinationDir = Path.Combine(destDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void DeleteDirectory(string path, bool recursive = true)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }

        public static IEnumerable<string> GetFilesByExtension(string directory, string extension, bool recursive = false)
        {
            if (!Directory.Exists(directory))
                return Enumerable.Empty<string>();

            return Directory.GetFiles(directory, $"*.{extension}", 
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static string ReadAllText(string path, Encoding encoding = null)
        {
            if (!File.Exists(path))
                return null;

            return encoding != null 
                ? File.ReadAllText(path, encoding) 
                : File.ReadAllText(path);
        }

        public static void WriteAllText(string path, string content, Encoding encoding = null)
        {
            EnsureDirectoryExists(path);
            if (encoding != null)
                File.WriteAllText(path, content, encoding);
            else
                File.WriteAllText(path, content);
        }

        public static byte[] ReadAllBytes(string path)
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            EnsureDirectoryExists(path);
            File.WriteAllBytes(path, bytes);
        }

        public static bool IsFileLocked(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        public static FileInfo GetFileInfo(string path)
        {
            return File.Exists(path) ? new FileInfo(path) : null;
        }

        public static DateTime GetLastModified(string path)
        {
            return File.Exists(path) ? File.GetLastWriteTime(path) : DateTime.MinValue;
        }

        public static DateTime GetCreatedDate(string path)
        {
            return File.Exists(path) ? File.GetCreationTime(path) : DateTime.MinValue;
        }

        public static void Touch(string path)
        {
            if (File.Exists(path))
            {
                File.SetLastWriteTime(path, DateTime.Now);
            }
            else
            {
                EnsureDirectoryExists(path);
                File.Create(path).Close();
            }
        }
    }

    public static class StringHelper
    {
        public static string Truncate(string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        public static string RemoveWhitespace(string value)
        {
            return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        public static string NormalizeWhitespace(string value)
        {
            return Regex.Replace(value.Trim(), @"\s+", " ");
        }

        public static bool ContainsIgnoreCase(string source, string value)
        {
            return source?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool EqualsIgnoreCase(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToTitleCase(string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string ToPascalCase(string value)
        {
            var words = value.Split(new[] { ' ', '_', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
        }

        public static string ToCamelCase(string value)
        {
            var pascal = ToPascalCase(value);
            return char.ToLower(pascal[0]) + pascal.Substring(1);
        }

        public static string ToSnakeCase(string value)
        {
            var words = Regex.Split(value, @"([A-Z][a-z]+)")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.ToLower());
            return string.Join("_", words);
        }

        public static string ToKebabCase(string value)
        {
            var words = Regex.Split(value, @"([A-Z][a-z]+)")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.ToLower());
            return string.Join("-", words);
        }

        public static string Reverse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string RemoveSpecialCharacters(string value, string allowed = "")
        {
            return new string(value.Where(c => char.IsLetterOrDigit(c) || allowed.Contains(c)).ToArray());
        }

        public static int CountWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return Regex.Matches(value, @"\S+").Count;
        }

        public static int CountLines(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static string ExtractNumbers(string value)
        {
            return new string(value.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
        }

        public static string ExtractLetters(string value)
        {
            return new string(value.Where(c => char.IsLetter(c)).ToArray());
        }

        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string JoinNonEmpty(string separator, params string[] values)
        {
            return string.Join(separator, values.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        public static string ReplaceMultiple(string value, Dictionary<string, string> replacements)
        {
            var result = value;
            foreach (var replacement in replacements)
            {
                result = result.Replace(replacement.Key, replacement.Value);
            }
            return result;
        }

        public static string PadLeft(string value, int totalWidth, char paddingChar = ' ')
        {
            return value?.PadLeft(totalWidth, paddingChar) ?? new string(paddingChar, totalWidth);
        }

        public static string PadRight(string value, int totalWidth, char paddingChar = ' ')
        {
            return value?.PadRight(totalWidth, paddingChar) ?? new string(paddingChar, totalWidth);
        }

        public static string WrapText(string text, int maxWidth)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 <= maxWidth)
                {
                    if (currentLine.Length > 0)
                        currentLine.Append(' ');
                    currentLine.Append(word);
                }
                else
                {
                    if (currentLine.Length > 0)
                        lines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return string.Join(Environment.NewLine, lines);
        }

        public static string[] SplitAndTrim(string value, char separator)
        {
            return value.Split(separator).Select(s => s.Trim()).ToArray();
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string MaskSensitiveData(string value, int visibleChars = 4)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= visibleChars)
                return new string('*', value.Length);

            var maskLength = value.Length - visibleChars;
            return new string('*', maskLength) + value.Substring(maskLength);
        }
    }

    public static class DateTimeHelper
    {
        public static string FormatDate(DateTime date, string format = "yyyy-MM-dd")
        {
            return date.ToString(format);
        }

        public static string FormatTime(DateTime time, string format = "HH:mm:ss")
        {
            return time.ToString(format);
        }

        public static string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return dateTime.ToString(format);
        }

        public static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";

            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }

        public static DateTime StartOfDay(DateTime date)
        {
            return date.Date;
        }

        public static DateTime EndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        public static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            return StartOfWeek(date, startOfWeek).AddDays(7).AddTicks(-1);
        }

        public static DateTime StartOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime EndOfMonth(DateTime date)
        {
            return StartOfMonth(date).AddMonths(1).AddTicks(-1);
        }

        public static DateTime StartOfYear(DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }

        public static DateTime EndOfYear(DateTime date)
        {
            return new DateTime(date.Year, 12, 31, 23, 59, 59, 999);
        }

        public static int GetWeekNumber(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsWeekday(DateTime date)
        {
            return !IsWeekend(date);
        }

        public static DateTime AddWorkdays(DateTime date, int workdays)
        {
            var direction = workdays < 0 ? -1 : 1;
            var currentDate = date;

            while (workdays != 0)
            {
                currentDate = currentDate.AddDays(direction);
                if (IsWeekday(currentDate))
                    workdays -= direction;
            }

            return currentDate;
        }

        public static int GetBusinessDays(DateTime startDate, DateTime endDate)
        {
            int businessDays = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (IsWeekday(currentDate))
                    businessDays++;
                currentDate = currentDate.AddDays(1);
            }

            return businessDays;
        }

        public static TimeSpan GetAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today - birthDate;
            return age;
        }

        public static int GetAgeInYears(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }

        public static string[] GetMonthNames()
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrEmpty(m)).ToArray();
        }

        public static string[] GetDayNames()
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
        }

        public static string GetQuarter(DateTime date)
        {
            return $"Q{(date.Month - 1) / 3 + 1} {date.Year}";
        }
    }

    public static class MathHelper
    {
        public static double Round(double value, int decimals = 2)
        {
            return Math.Round(value, decimals);
        }

        public static double RoundUp(double value, int decimals = 2)
        {
            var multiplier = Math.Pow(10, decimals);
            return Math.Ceiling(value * multiplier) / multiplier;
        }

        public static double RoundDown(double value, int decimals = 2)
        {
            var multiplier = Math.Pow(10, decimals);
            return Math.Floor(value * multiplier) / multiplier;
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        public static double Normalize(double value, double min, double max)
        {
            if (max == min)
                return 0;
            return (value - min) / (max - min);
        }

        public static double Denormalize(double normalized, double min, double max)
        {
            return normalized * (max - min) + min;
        }

        public static double Map(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            var normalized = Normalize(value, fromMin, fromMax);
            return Denormalize(normalized, toMin, toMax);
        }

        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static int Sign(double value)
        {
            if (value > 0) return 1;
            if (value < 0) return -1;
            return 0;
        }

        public static bool IsEven(int value)
        {
            return value % 2 == 0;
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (IsEven(number)) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));
            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }

        public static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                var t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        public static int Lcm(int a, int b)
        {
            return Math.Abs(a * b) / Gcd(a, b);
        }

        public static double Factorial(int n)
        {
            if (n < 0) throw new ArgumentException("Factorial is not defined for negative numbers");
            if (n > 170) return double.PositiveInfinity;

            double result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        public static double StandardDeviation(IEnumerable<double> values)
        {
            var array = values.ToArray();
            if (array.Length < 2) return 0;

            var avg = array.Average();
            var sumOfSquares = array.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / (array.Length - 1));
        }

        public static double Variance(IEnumerable<double> values)
        {
            var array = values.ToArray();
            if (array.Length < 2) return 0;

            var avg = array.Average();
            return array.Sum(v => Math.Pow(v - avg, 2)) / (array.Length - 1);
        }

        public static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(v => v).ToArray();
            int n = sorted.Length;

            if (n == 0) return 0;
            if (n % 2 == 0)
                return (sorted[n / 2 - 1] + sorted[n / 2]) / 2;
            return sorted[n / 2];
        }

        public static double Mode(IEnumerable<double> values)
        {
            var groups = values.GroupBy(v => v);
            var maxCount = groups.Max(g => g.Count());
            return groups.First(g => g.Count() == maxCount).Key;
        }

        public static (double min, double max) GetMinMax(IEnumerable<double> values)
        {
            var array = values.ToArray();
            if (array.Length == 0) return (0, 0);

            return (array.Min(), array.Max());
        }

        public static double PercentageChange(double oldValue, double newValue)
        {
            if (oldValue == 0) return newValue == 0 ? 0 : 100;
            return (newValue - oldValue) / oldValue * 100;
        }
    }

    public static class ColorHelper
    {
        public static Color FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                return Color.FromRgb(
                    Convert.ToByte(hex.Substring(0, 2), 16),
                    Convert.ToByte(hex.Substring(2, 2), 16),
                    Convert.ToByte(hex.Substring(4, 2), 16));
            }
            if (hex.Length == 8)
            {
                return Color.FromArgb(
                    Convert.ToByte(hex.Substring(0, 2), 16),
                    Convert.ToByte(hex.Substring(2, 2), 16),
                    Convert.ToByte(hex.Substring(4, 2), 16),
                    Convert.ToByte(hex.Substring(6, 2), 16));
            }
            return Colors.Black;
        }

        public static string ToHex(Color color, bool includeAlpha = false)
        {
            if (includeAlpha)
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static Color FromRgb(int r, int g, int b)
        {
            return Color.FromRgb((byte)Clamp(r, 0, 255), (byte)Clamp(g, 0, 255), (byte)Clamp(b, 0, 255));
        }

        public static Color FromHsl(double h, double s, double l)
        {
            h = h % 360;
            if (h < 0) h += 360;
            s = Clamp(s, 0, 1);
            l = Clamp(l, 0, 1);

            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double r1, g1, b1;
            if (h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            return FromRgb(
                (int)((r1 + m) * 255),
                (int)((g1 + m) * 255),
                (int)((b1 + m) * 255));
        }

        public static (double h, double s, double l) ToHsl(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double l = (max + min) / 2;
            double h = 0, s = 0;

            if (max != min)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (max == r)
                    h = ((g - b) / d + (g < b ? 6 : 0)) * 60;
                else if (max == g)
                    h = ((b - r) / d + 2) * 60;
                else
                    h = ((r - g) / d + 4) * 60;
            }

            return (h, s, l);
        }

        public static Color Lighten(Color color, double amount)
        {
            var (h, s, l) = ToHsl(color);
            l = Clamp(l + amount, 0, 1);
            return FromHsl(h, s, l);
        }

        public static Color Darken(Color color, double amount)
        {
            var (h, s, l) = ToHsl(color);
            l = Clamp(l - amount, 0, 1);
            return FromHsl(h, s, l);
        }

        public static Color Saturate(Color color, double amount)
        {
            var (h, s, l) = ToHsl(color);
            s = Clamp(s + amount, 0, 1);
            return FromHsl(h, s, l);
        }

        public static Color Desaturate(Color color, double amount)
        {
            var (h, s, l) = ToHsl(color);
            s = Clamp(s - amount, 0, 1);
            return FromHsl(h, s, l);
        }

        public static Color Blend(Color color1, Color color2, double ratio = 0.5)
        {
            ratio = Clamp(ratio, 0, 1);
            return FromRgb(
                (int)(color1.R * (1 - ratio) + color2.R * ratio),
                (int)(color1.G * (1 - ratio) + color2.G * ratio),
                (int)(color1.B * (1 - ratio) + color2.B * ratio));
        }

        public static Color Invert(Color color)
        {
            return FromRgb(255 - color.R, 255 - color.G, 255 - color.B);
        }

        public static double GetLuminance(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
            b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        public static double GetContrastRatio(Color color1, Color color2)
        {
            double l1 = GetLuminance(color1);
            double l2 = GetLuminance(color2);
            var lighter = Math.Max(l1, l2);
            var darker = Math.Min(l1, l2);
            return (lighter + 0.05) / (darker + 0.05);
        }

        public static Color GetComplementary(Color color)
        {
            var (h, s, l) = ToHsl(color);
            return FromHsl((h + 180) % 360, s, l);
        }

        public static Color[] GetAnalogous(Color color)
        {
            var (h, s, l) = ToHsl(color);
            return new[]
            {
                FromHsl((h - 30 + 360) % 360, s, l),
                color,
                FromHsl((h + 30) % 360, s, l)
            };
        }

        public static Color[] GetTriadic(Color color)
        {
            var (h, s, l) = ToHsl(color);
            return new[]
            {
                color,
                FromHsl((h + 120) % 360, s, l),
                FromHsl((h + 240) % 360, s, l)
            };
        }

        public static bool IsLight(Color color)
        {
            return GetLuminance(color) > 0.5;
        }

        public static bool IsDark(Color color)
        {
            return GetLuminance(color) <= 0.5;
        }

        public static Color GetBestForeground(Color background)
        {
            return IsLight(background) ? Colors.Black : Colors.White;
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
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
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return Regex.IsMatch(phone, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$");
        }

        public static bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            return System.Net.IPAddress.TryParse(ip, out _);
        }

        public static bool IsValidCreditCard(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            if (!cardNumber.All(char.IsDigit) || cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        public static bool IsValidHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return false;

            hex = hex.TrimStart('#');
            return Regex.IsMatch(hex, @"^[0-9A-Fa-f]{6}$|^[0-9A-Fa-f]{8}$");
        }

        public static bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _);
        }

        public static bool IsValidDate(string date, string format = "yyyy-MM-dd")
        {
            return DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }

        public static bool IsNumeric(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.All(c => char.IsDigit(c) || c == '.' || c == '-');
        }

        public static bool IsAlphabetic(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.All(c => char.IsLetter(c));
        }

        public static bool IsAlphanumeric(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.All(c => char.IsLetterOrDigit(c));
        }

        public static ValidationResult Validate(object value, List<ValidationRule> rules)
        {
            var result = new ValidationResult { IsValid = true };

            foreach (var rule in rules)
            {
                if (!rule.Validate(value))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        RuleName = rule.Name,
                        ErrorMessage = rule.ErrorMessage
                    });
                }
            }

            return result;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; }

        public ValidationResult()
        {
            Errors = new List<ValidationError>();
        }
    }

    public class ValidationError
    {
        public string RuleName { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ValidationRule
    {
        public string Name { get; set; }
        public string ErrorMessage { get; set; }
        public Func<object, bool> ValidateFunc { get; set; }

        public bool Validate(object value)
        {
            return ValidateFunc?.Invoke(value) ?? true;
        }

        public static ValidationRule Required(string fieldName)
        {
            return new ValidationRule
            {
                Name = "Required",
                ErrorMessage = $"{fieldName} is required",
                ValidateFunc = v => v != null && !string.IsNullOrWhiteSpace(v.ToString())
            };
        }

        public static ValidationRule MinLength(string fieldName, int minLength)
        {
            return new ValidationRule
            {
                Name = "MinLength",
                ErrorMessage = $"{fieldName} must be at least {minLength} characters",
                ValidateFunc = v => v != null && v.ToString().Length >= minLength
            };
        }

        public static ValidationRule MaxLength(string fieldName, int maxLength)
        {
            return new ValidationRule
            {
                Name = "MaxLength",
                ErrorMessage = $"{fieldName} must not exceed {maxLength} characters",
                ValidateFunc = v => v != null && v.ToString().Length <= maxLength
            };
        }

        public static ValidationRule Range(string fieldName, double min, double max)
        {
            return new ValidationRule
            {
                Name = "Range",
                ErrorMessage = $"{fieldName} must be between {min} and {max}",
                ValidateFunc = v =>
                {
                    if (double.TryParse(v?.ToString(), out double num))
                        return num >= min && num <= max;
                    return false;
                }
            };
        }

        public static ValidationRule Pattern(string fieldName, string pattern, string description = "")
        {
            return new ValidationRule
            {
                Name = "Pattern",
                ErrorMessage = $"{fieldName} is not valid. {description}",
                ValidateFunc = v => v != null && Regex.IsMatch(v.ToString(), pattern)
            };
        }

        public static ValidationRule Email(string fieldName)
        {
            return new ValidationRule
            {
                Name = "Email",
                ErrorMessage = $"{fieldName} must be a valid email address",
                ValidateFunc = v => v != null && ValidationHelper.IsValidEmail(v.ToString())
            };
        }

        public static ValidationRule Url(string fieldName)
        {
            return new ValidationRule
            {
                Name = "Url",
                ErrorMessage = $"{fieldName} must be a valid URL",
                ValidateFunc = v => v != null && ValidationHelper.IsValidUrl(v.ToString())
            };
        }
    }
}
